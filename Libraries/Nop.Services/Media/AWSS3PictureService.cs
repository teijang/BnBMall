using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Configuration;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Caching;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Events;
using Nop.Services.Seo;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Nop.Services.Media
{
    public class AWSS3PictureService : PictureService
    {
        #region Fields

        private static bool _isInitialized;
        private readonly IRepository<Picture> _pictureRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICacheKeyService _cacheKeyService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly INopFileProvider _fileProvider;
        private readonly IDownloadService _downloadService;
        private readonly MediaSettings _mediaSettings;
        private readonly object _locker = new object();
        private static TransferUtility utility;
        private static IAmazonS3 client;
        private static string _awsS3AccessKeyId;
        private static string _awsS3SecretAccessKey;
        private static string _awsS3Region;
        private static string _awsS3ImageBucketName;
        private static string _awsS3ReviewImageBucketName;
        private static string _awsS3ReviewVideoBucketName;
        private static string _awsS3ImageBucketCDN;
        private static string _awsS3ReviewImageBucketCDN;
        private static string _awsS3ReviewVideoBucketCDN;

        #endregion

        #region Ctor

        public AWSS3PictureService(INopDataProvider dataProvider,
            IDownloadService downloadService,
            ICacheKeyService cacheKeyService,
            IEventPublisher eventPublisher,
            IHttpContextAccessor httpContextAccessor,
            INopFileProvider fileProvider,
            IProductAttributeParser productAttributeParser,
            IRepository<Picture> pictureRepository,
            IRepository<PictureBinary> pictureBinaryRepository,
            IRepository<ProductPicture> productPictureRepository,
            ISettingService settingService,
            IStaticCacheManager staticCacheManager,
            IUrlRecordService urlRecordService,
            IWebHelper webHelper,
            MediaSettings mediaSettings,
            NopConfig config)
            : base(dataProvider,
                  downloadService,
                  eventPublisher,
                  httpContextAccessor,
                  fileProvider,
                  productAttributeParser,
                  pictureRepository,
                  pictureBinaryRepository,
                  productPictureRepository,
                  settingService,
                  urlRecordService,
                  webHelper,
                  mediaSettings) {
            _downloadService = downloadService;
            _cacheKeyService = cacheKeyService;
            _staticCacheManager = staticCacheManager;
            _mediaSettings = mediaSettings;
            _pictureRepository = pictureRepository;
            _eventPublisher = eventPublisher;
            _fileProvider = fileProvider;
            OneTimeInit(config);
        }

        #endregion

        #region Utilities

        protected void OneTimeInit(NopConfig config) {
            if(_isInitialized)
                return;

            if(string.IsNullOrEmpty(config.AWSS3AccessKeyId))
                throw new Exception("AWS S3 Acccess key id is not specified");

            if(string.IsNullOrEmpty(config.AWSS3SecretAccessKey))
                throw new Exception("AWS S3 Acccess secret access key is not specified");

            if(string.IsNullOrEmpty(config.AWSS3Region))
                throw new Exception("AWS S3 Region is not specified");

            lock(_locker) {
                if(_isInitialized)
                    return;


                _awsS3AccessKeyId = config.AWSS3AccessKeyId;
                _awsS3SecretAccessKey = config.AWSS3SecretAccessKey;
                _awsS3Region = config.AWSS3Region;
                _awsS3ImageBucketName = config.AWSS3ImageBucketName;
                _awsS3ReviewImageBucketName = config.AWSS3ReviewImageBucketName;
                _awsS3ReviewVideoBucketName = config.AWSS3ReviewVideoBucketName;
                _awsS3ImageBucketCDN = config.AWSS3ImageBucketCDN;
                _awsS3ReviewImageBucketCDN = config.AWSS3ReviewImageBucketCDN;
                _awsS3ReviewVideoBucketCDN = config.AWSS3ReviewVideoBucketCDN;

                CreateInstanceOfAmazonS3();

                _isInitialized = true;
            }
        }

        /// <summary>
        /// Create cloud blob container
        /// </summary>
        protected virtual void CreateInstanceOfAmazonS3() {
            client = new AmazonS3Client(_awsS3AccessKeyId, _awsS3SecretAccessKey, RegionEndpoint.GetBySystemName(_awsS3Region));
            utility = new TransferUtility(client);
        }

        /// <summary>
        /// Delete picture thumbs
        /// </summary>
        /// <param name="picture">Picture</param>
        protected override async void DeletePictureThumbs(Picture picture, bool isReview = false) {
            await DeletePictureThumbsAsync(picture, true);
        }

        /// <summary>
        /// Get picture (thumb) local path
        /// </summary>
        /// <param name="thumbFileName">Filename</param>
        /// <returns>Local picture thumb path</returns>
        protected override string GetThumbLocalPath(string thumbFileName, bool isReview = false) {
            return $"https://{(isReview ? _awsS3ReviewImageBucketCDN : _awsS3ImageBucketCDN)}/" + thumbFileName;
        }

        /// <summary>
        /// Get picture (thumb) URL 
        /// </summary>
        /// <param name="thumbFileName">Filename</param>
        /// <param name="storeLocation">Store location URL; null to use determine the current store location automatically</param>
        /// <returns>Local picture thumb path</returns>
        protected override string GetThumbUrl(string thumbFileName, string storeLocation = null, bool isReview = false) {
            return GetThumbLocalPath(thumbFileName, isReview);
        }

        /// <summary>
        /// Get a value indicating whether some file (thumb) already exists
        /// </summary>
        /// <param name="thumbFilePath">Thumb file path</param>
        /// <param name="thumbFileName">Thumb file name</param>
        /// <returns>Result</returns>
        protected override bool GeneratedThumbExists(string thumbFilePath, string thumbFileName, bool isReview = false) {
            return GeneratedThumbExistsAsync(thumbFilePath, thumbFileName, isReview).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Save a value indicating whether some file (thumb) already exists
        /// </summary>
        /// <param name="thumbFilePath">Thumb file path</param>
        /// <param name="thumbFileName">Thumb file name</param>
        /// <param name="mimeType">MIME type</param>
        /// <param name="binary">Picture binary</param>
        protected override void SaveThumb(string thumbFilePath, string thumbFileName, string mimeType, byte[] binary, bool isReview = false) {
            var extension = "." + GetFileExtensionFromMimeType(mimeType);

            var fileName = thumbFileName + extension;
            fileName = fileName.Replace(extension + extension, extension);

            TransferUtilityUploadRequest request = new TransferUtilityUploadRequest {
                BucketName = isReview ? _awsS3ReviewImageBucketName : _awsS3ImageBucketName,
                Key = fileName
            };

            if(!string.IsNullOrEmpty(mimeType))
                request.ContentType = mimeType;

            //set cache control
            if(!string.IsNullOrEmpty(_mediaSettings.AWSCacheControlHeader))
                request.Headers.CacheControl = _mediaSettings.AWSCacheControlHeader;

            request.Metadata["x-amz-meta-title"] = fileName;
            request.InputStream = new MemoryStream(binary);
            utility.UploadAsync(request).GetAwaiter().GetResult();

            _staticCacheManager.RemoveByPrefix(NopMediaDefaults.ThumbsExistsPrefixCacheKey);
        }

        /// <summary>
        /// Initiates an asynchronous operation to delete picture thumbs
        /// </summary>
        /// <param name="picture">Picture</param>
        protected virtual async Task DeletePictureThumbsAsync(Picture picture, bool isReview = false) {
            var prefix = $"{picture.Id:0000000}";

            var listObjectsRequest = new ListObjectsRequest() {
                BucketName = isReview ? _awsS3ReviewImageBucketName : _awsS3ImageBucketName,
                Prefix = prefix
            };

            var extension = "." + GetFileExtensionFromMimeType(picture.MimeType);
            var listResponse = await client.ListObjectsAsync(listObjectsRequest);
            if(listResponse.S3Objects.Count > 0) {
                foreach(var item in listResponse.S3Objects) {
                    if(item.Key != prefix + "_0" + extension) {
                        var deleteObjectRequest = new DeleteObjectRequest {
                            BucketName = isReview ? _awsS3ReviewImageBucketName : _awsS3ImageBucketName,
                            Key = item.Key
                        };
                        await client.DeleteObjectAsync(deleteObjectRequest);
                    }
                }
            }

            _staticCacheManager.RemoveByPrefix(NopMediaDefaults.AWSThumbsExistsPrefixCacheKey);
        }

        /// <summary>
        /// Initiates an asynchronous operation to get a value indicating whether some file (thumb) already exists
        /// </summary>
        /// <param name="thumbFilePath">Thumb file path</param>
        /// <param name="thumbFileName">Thumb file name</param>
        /// <returns>Result</returns>
        protected virtual async Task<bool> GeneratedThumbExistsAsync(string thumbFilePath, string thumbFileName, bool isReview = false) {
            try {
                var key = _cacheKeyService.PrepareKeyForDefaultCache(NopMediaDefaults.AWSThumbExistsCacheKey, thumbFileName);

                return await _staticCacheManager.GetAsync(key, async () => {
                    var request = new GetObjectRequest() {
                        BucketName = isReview ? _awsS3ReviewImageBucketName : _awsS3ImageBucketName,
                        Key = thumbFileName
                    };

                    var response = await client.GetObjectAsync(request);
                    return true;
                });
            } catch {
                return false;
            }
        }

        /// <summary>
        /// Inserts a picture
        /// </summary>
        /// <param name="pictureBinary">The picture binary</param>
        /// <param name="mimeType">The picture MIME type</param>
        /// <param name="seoFilename">The SEO filename</param>
        /// <param name="altAttribute">"alt" attribute for "img" HTML element</param>
        /// <param name="titleAttribute">"title" attribute for "img" HTML element</param>
        /// <param name="isNew">A value indicating whether the picture is new</param>
        /// <param name="validateBinary">A value indicating whether to validated provided picture binary</param>
        /// <returns>Picture</returns>
        public override Picture InsertPicture(byte[] pictureBinary, string mimeType, string seoFilename,
           string altAttribute = null, string titleAttribute = null,
           bool isNew = true, bool validateBinary = true, bool isReview = false) {
            mimeType = CommonHelper.EnsureNotNull(mimeType);
            mimeType = CommonHelper.EnsureMaximumLength(mimeType, 20);

            seoFilename = CommonHelper.EnsureMaximumLength(seoFilename, 100);

            if(validateBinary)
                pictureBinary = ValidatePicture(pictureBinary, mimeType);

            var picture = new Picture {
                MimeType = mimeType,
                SeoFilename = seoFilename,
                AltAttribute = altAttribute,
                TitleAttribute = titleAttribute,
                IsNew = isNew
            };

            _pictureRepository.Insert(picture);

            var imageName = $"{picture.Id:0000000}_0";
            SaveThumb("", imageName, mimeType, pictureBinary, isReview);
            //Thread.Sleep(3000);

            //event notification
            _eventPublisher.EntityInserted(picture);

            return picture;
        }

        /// <summary>
        /// Gets the loaded picture binary depending on picture storage settings
        /// </summary>
        /// <param name="picture">Picture</param>
        /// <param name="fromDb">Load from database; otherwise, from file system</param>
        /// <returns>Picture binary</returns>
        protected override byte[] LoadPictureBinary(Picture picture, bool fromDb, bool isReview = false) {
            if(picture == null)
                throw new ArgumentNullException(nameof(picture));

            var extension = GetFileExtensionFromMimeType(picture.MimeType);
            try {

                var fileName = $"{picture.Id:0000000}_0";
                var request = new GetObjectRequest() {
                    BucketName = isReview ? _awsS3ReviewImageBucketName : _awsS3ImageBucketName,
                    Key = fileName + '.' + extension,
                };

                var response = client.GetObjectAsync(request).Result;
                var result = ReadFully(response.ResponseStream);
                return result;
            } catch(Exception ex) {
                try {
                    var fileName = $"{picture.Id:0000000}_" + picture.SeoFilename;
                    var request = new GetObjectRequest() {
                        BucketName = isReview ? _awsS3ReviewImageBucketName : _awsS3ImageBucketName,
                        Key = fileName + '.' + extension,
                    };

                    var response = client.GetObjectAsync(request).Result;
                    var result = ReadFully(response.ResponseStream);
                    return result;
                } catch {
                    var result = fromDb
                     ? GetPictureBinaryByPictureId(picture.Id)?.BinaryData ?? Array.Empty<byte>()
                     : LoadPictureFromFile(picture.Id, picture.MimeType);

                    return result;
                }
            }
        }

        public static byte[] ReadFully(Stream input) {
            using MemoryStream ms = new MemoryStream();
            input.CopyTo(ms);
            return ms.ToArray();
        }

        public override Picture UpdatePicture(int pictureId, byte[] pictureBinary, string mimeType,
            string seoFilename, string altAttribute = null, string titleAttribute = null,
            bool isNew = true, bool validateBinary = true) {
            mimeType = CommonHelper.EnsureNotNull(mimeType);
            mimeType = CommonHelper.EnsureMaximumLength(mimeType, 20);

            seoFilename = CommonHelper.EnsureMaximumLength(seoFilename, 100);

            if(validateBinary)
                pictureBinary = ValidatePicture(pictureBinary, mimeType);

            var picture = GetPictureById(pictureId);
            if(picture == null)
                return null;

            //delete old thumbs if a picture has been changed
            if(seoFilename != picture.SeoFilename)
                DeletePictureThumbs(picture);

            picture.MimeType = mimeType;
            picture.SeoFilename = seoFilename;
            picture.AltAttribute = altAttribute;
            picture.TitleAttribute = titleAttribute;
            picture.IsNew = isNew;

            _pictureRepository.Update(picture);

            //event notification
            _eventPublisher.EntityUpdated(picture);

            return picture;
        }

        public override Picture SetSeoFilename(int pictureId, string seoFilename) {
            var picture = GetPictureById(pictureId);
            if(picture == null)
                throw new ArgumentException("No picture found with the specified id");

            return picture;
        }

        /// <summary>
        /// Insert video
        /// </summary>
        /// <param name="formFile"></param>
        /// <param name="defaultFileName"></param>
        /// <param name="virtualPath"></param>
        /// <returns></returns>
        public override string InsertVideo(IFormFile formFile, string defaultFileName = "", string virtualPath = "") {
            var fileName = formFile.FileName;
            if(string.IsNullOrEmpty(fileName) && !string.IsNullOrEmpty(defaultFileName))
                fileName = defaultFileName;

            //remove path (passed in IE)
            fileName = _fileProvider.GetFileName(fileName);

            var fileExtension = _fileProvider.GetFileExtension(fileName);
            if(!string.IsNullOrEmpty(fileExtension))
                fileExtension = fileExtension.ToLowerInvariant();

            var newFileName = Guid.NewGuid().ToString() + fileExtension;

            TransferUtilityUploadRequest request = new TransferUtilityUploadRequest {
                BucketName = _awsS3ReviewVideoBucketName,
                Key = newFileName
            };

            //set cache control
            if(!string.IsNullOrEmpty(_mediaSettings.AWSCacheControlHeader))
                request.Headers.CacheControl = _mediaSettings.AWSCacheControlHeader;

            request.Metadata["x-amz-meta-title"] = newFileName;
            request.InputStream = new MemoryStream(_downloadService.GetDownloadBits(formFile));
            utility.UploadAsync(request).GetAwaiter().GetResult();

            return newFileName;
        }

        #endregion
    }
}

﻿using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Configuration;
using Nop.Core.Domain;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Vendors;
using Nop.Web.Factories;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Themes;
using Nop.Web.Models.Common;
using System;
using System.Linq;

namespace Nop.Web.Controllers
{
    [AutoValidateAntiforgeryToken]
    public partial class CommonController : BasePublicController
    {
        #region Fields

        private readonly CaptchaSettings _captchaSettings;
        private readonly CommonSettings _commonSettings;
        private readonly ICommonModelFactory _commonModelFactory;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly IThemeContext _themeContext;
        private readonly IVendorService _vendorService;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly SitemapSettings _sitemapSettings;
        private readonly SitemapXmlSettings _sitemapXmlSettings;
        private readonly StoreInformationSettings _storeInformationSettings;
        private readonly VendorSettings _vendorSettings;
        private readonly IPictureService _pictureService;
        private readonly INopFileProvider _fileProvider;
        private int VideoSize = 15;

        #endregion

        #region Ctor

        public CommonController(CaptchaSettings captchaSettings,
            CommonSettings commonSettings,
            ICommonModelFactory commonModelFactory,
            ICurrencyService currencyService,
            ICustomerActivityService customerActivityService,
            IGenericAttributeService genericAttributeService,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IStoreContext storeContext,
            IThemeContext themeContext,
            IVendorService vendorService,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings,
            SitemapSettings sitemapSettings,
            SitemapXmlSettings sitemapXmlSettings,
            StoreInformationSettings storeInformationSettings,
            VendorSettings vendorSettings,
            IPictureService pictureService,
            INopFileProvider fileProvider,
            NopConfig config) {
            _captchaSettings = captchaSettings;
            _commonSettings = commonSettings;
            _commonModelFactory = commonModelFactory;
            _currencyService = currencyService;
            _customerActivityService = customerActivityService;
            _genericAttributeService = genericAttributeService;
            _languageService = languageService;
            _localizationService = localizationService;
            _storeContext = storeContext;
            _themeContext = themeContext;
            _vendorService = vendorService;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
            _sitemapSettings = sitemapSettings;
            _sitemapXmlSettings = sitemapXmlSettings;
            _storeInformationSettings = storeInformationSettings;
            _vendorSettings = vendorSettings;
            _pictureService = pictureService;
            _fileProvider = fileProvider;
            VideoSize = config.AWSS3VideoSize;
        }

        #endregion

        #region Methods

        //page not found
        public virtual IActionResult PageNotFound() {
            Response.StatusCode = 404;
            Response.ContentType = "text/html";

            return View();
        }

        //available even when a store is closed
        [CheckAccessClosedStore(true)]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual IActionResult SetLanguage(int langid, string returnUrl = "") {
            var language = _languageService.GetLanguageById(langid);
            if(!language?.Published ?? false)
                language = _workContext.WorkingLanguage;

            //home page
            if(string.IsNullOrEmpty(returnUrl))
                returnUrl = Url.RouteUrl("Homepage");

            //language part in URL
            if(_localizationSettings.SeoFriendlyUrlsForLanguagesEnabled) {
                //remove current language code if it's already localized URL
                if(returnUrl.IsLocalizedUrl(Request.PathBase, true, out var _))
                    returnUrl = returnUrl.RemoveLanguageSeoCodeFromUrl(Request.PathBase, true);

                //and add code of passed language
                returnUrl = returnUrl.AddLanguageSeoCodeToUrl(Request.PathBase, true, language);
            }

            _workContext.WorkingLanguage = language;

            //prevent open redirection attack
            if(!Url.IsLocalUrl(returnUrl))
                returnUrl = Url.RouteUrl("Homepage");

            return Redirect(returnUrl);
        }

        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual IActionResult SetCurrency(int customerCurrency, string returnUrl = "") {
            var currency = _currencyService.GetCurrencyById(customerCurrency);
            if(currency != null)
                _workContext.WorkingCurrency = currency;

            //home page
            if(string.IsNullOrEmpty(returnUrl))
                returnUrl = Url.RouteUrl("Homepage");

            //prevent open redirection attack
            if(!Url.IsLocalUrl(returnUrl))
                returnUrl = Url.RouteUrl("Homepage");

            return Redirect(returnUrl);
        }

        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual IActionResult SetTaxType(int customerTaxType, string returnUrl = "") {
            var taxDisplayType = (TaxDisplayType)Enum.ToObject(typeof(TaxDisplayType), customerTaxType);
            _workContext.TaxDisplayType = taxDisplayType;

            //home page
            if(string.IsNullOrEmpty(returnUrl))
                returnUrl = Url.RouteUrl("Homepage");

            //prevent open redirection attack
            if(!Url.IsLocalUrl(returnUrl))
                returnUrl = Url.RouteUrl("Homepage");

            return Redirect(returnUrl);
        }

        //contact us page
        [HttpsRequirement]
        //available even when a store is closed
        [CheckAccessClosedStore(true)]
        public virtual IActionResult ContactUs() {
            var model = new ContactUsModel();
            model = _commonModelFactory.PrepareContactUsModel(model, false);
            return View(model);
        }

        [HttpPost, ActionName("ContactUs")]
        [ValidateCaptcha]
        //available even when a store is closed
        [CheckAccessClosedStore(true)]
        public virtual IActionResult ContactUsSend(ContactUsModel model, bool captchaValid) {
            //validate CAPTCHA
            if(_captchaSettings.Enabled && _captchaSettings.ShowOnContactUsPage && !captchaValid) {
                ModelState.AddModelError("", _localizationService.GetResource("Common.WrongCaptchaMessage"));
            }

            model = _commonModelFactory.PrepareContactUsModel(model, true);

            if(ModelState.IsValid) {
                var subject = _commonSettings.SubjectFieldOnContactUsForm ? model.Subject : null;
                var body = Core.Html.HtmlHelper.FormatText(model.Enquiry, false, true, false, false, false, false);

                _workflowMessageService.SendContactUsMessage(_workContext.WorkingLanguage.Id,
                    model.Email.Trim(), model.FullName, subject, body);

                model.SuccessfullySent = true;
                model.Result = _localizationService.GetResource("ContactUs.YourEnquiryHasBeenSent");

                //activity log
                _customerActivityService.InsertActivity("PublicStore.ContactUs",
                    _localizationService.GetResource("ActivityLog.PublicStore.ContactUs"));

                return View(model);
            }

            return View(model);
        }

        //contact vendor page
        [HttpsRequirement]
        public virtual IActionResult ContactVendor(int vendorId) {
            if(!_vendorSettings.AllowCustomersToContactVendors)
                return RedirectToRoute("Homepage");

            var vendor = _vendorService.GetVendorById(vendorId);
            if(vendor == null || !vendor.Active || vendor.Deleted)
                return RedirectToRoute("Homepage");

            var model = new ContactVendorModel();
            model = _commonModelFactory.PrepareContactVendorModel(model, vendor, false);
            return View(model);
        }

        [HttpPost, ActionName("ContactVendor")]
        [ValidateCaptcha]
        public virtual IActionResult ContactVendorSend(ContactVendorModel model, bool captchaValid) {
            if(!_vendorSettings.AllowCustomersToContactVendors)
                return RedirectToRoute("Homepage");

            var vendor = _vendorService.GetVendorById(model.VendorId);
            if(vendor == null || !vendor.Active || vendor.Deleted)
                return RedirectToRoute("Homepage");

            //validate CAPTCHA
            if(_captchaSettings.Enabled && _captchaSettings.ShowOnContactUsPage && !captchaValid) {
                ModelState.AddModelError("", _localizationService.GetResource("Common.WrongCaptchaMessage"));
            }

            model = _commonModelFactory.PrepareContactVendorModel(model, vendor, true);

            if(ModelState.IsValid) {
                var subject = _commonSettings.SubjectFieldOnContactUsForm ? model.Subject : null;
                var body = Core.Html.HtmlHelper.FormatText(model.Enquiry, false, true, false, false, false, false);

                _workflowMessageService.SendContactVendorMessage(vendor, _workContext.WorkingLanguage.Id,
                    model.Email.Trim(), model.FullName, subject, body);

                model.SuccessfullySent = true;
                model.Result = _localizationService.GetResource("ContactVendor.YourEnquiryHasBeenSent");

                return View(model);
            }

            return View(model);
        }

        //sitemap page
        public virtual IActionResult Sitemap(SitemapPageModel pageModel) {
            if(!_sitemapSettings.SitemapEnabled)
                return RedirectToRoute("Homepage");

            var model = _commonModelFactory.PrepareSitemapModel(pageModel);
            return View(model);
        }

        //SEO sitemap page
        //available even when a store is closed
        [CheckAccessClosedStore(true)]
        public virtual IActionResult SitemapXml(int? id) {
            var siteMap = _sitemapXmlSettings.SitemapXmlEnabled
                ? _commonModelFactory.PrepareSitemapXml(id) : string.Empty;

            return Content(siteMap, "text/xml");
        }

        public virtual IActionResult SetStoreTheme(string themeName, string returnUrl = "") {
            _themeContext.WorkingThemeName = themeName;

            //home page
            if(string.IsNullOrEmpty(returnUrl))
                returnUrl = Url.RouteUrl("Homepage");

            //prevent open redirection attack
            if(!Url.IsLocalUrl(returnUrl))
                returnUrl = Url.RouteUrl("Homepage");

            return Redirect(returnUrl);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        //available even when a store is closed
        [CheckAccessClosedStore(true)]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual IActionResult EuCookieLawAccept() {
            if(!_storeInformationSettings.DisplayEuCookieLawWarning)
                //disabled
                return Json(new { stored = false });

            //save setting
            _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, NopCustomerDefaults.EuCookieLawAcceptedAttribute, true, _storeContext.CurrentStore.Id);
            return Json(new { stored = true });
        }

        //robots.txt file
        //available even when a store is closed
        [CheckAccessClosedStore(true)]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual IActionResult RobotsTextFile() {
            var robotsFileContent = _commonModelFactory.PrepareRobotsTextFile();
            return Content(robotsFileContent, MimeTypes.TextPlain);
        }

        public virtual IActionResult GenericUrl() {
            //seems that no entity was found
            return InvokeHttp404();
        }

        //store is closed
        //available even when a store is closed
        [CheckAccessClosedStore(true)]
        public virtual IActionResult StoreClosed() {
            return View();
        }

        //helper method to redirect users. Workaround for GenericPathRoute class where we're not allowed to do it
        public virtual IActionResult InternalRedirect(string url, bool permanentRedirect) {
            //ensure it's invoked from our GenericPathRoute class
            if(HttpContext.Items["nop.RedirectFromGenericPathRoute"] == null ||
                !Convert.ToBoolean(HttpContext.Items["nop.RedirectFromGenericPathRoute"])) {
                url = Url.RouteUrl("Homepage");
                permanentRedirect = false;
            }

            //home page
            if(string.IsNullOrEmpty(url)) {
                url = Url.RouteUrl("Homepage");
                permanentRedirect = false;
            }

            //prevent open redirection attack
            if(!Url.IsLocalUrl(url)) {
                url = Url.RouteUrl("Homepage");
                permanentRedirect = false;
            }

            if(permanentRedirect)
                return RedirectPermanent(url);

            return Redirect(url);
        }

        #region Methods

        [HttpPost]
        //do not validate request token (XSRF)
        [IgnoreAntiforgeryToken]
        public virtual IActionResult AsyncUpload() {
            //if (!_permissionService.Authorize(StandardPermissionProvider.UploadPictures))
            //    return Json(new { success = false, error = "You do not have required permissions" }, "text/plain");

            var httpPostedFile = Request.Form.Files.FirstOrDefault();
            if(httpPostedFile == null) {
                return Json(new {
                    success = false,
                    message = "No file uploaded"
                });
            }

            const string qqFileNameParameter = "qqfilename";

            var qqFileName = Request.Form.ContainsKey(qqFileNameParameter)
                ? Request.Form[qqFileNameParameter].ToString()
                : string.Empty;

            var picture = _pictureService.InsertPicture(httpPostedFile, qqFileName, isReview: true);

            //when returning JSON the mime-type must be set to text/plain
            //otherwise some browsers will pop-up a "Save As" dialog.
            return Json(new {
                success = true,
                pictureId = picture.Id,
                imageUrl = _pictureService.GetReviewPictureUrl(picture.Id, 100)
            });
        }

        [HttpPost]
        //do not validate request token (XSRF)
        [IgnoreAntiforgeryToken]
        public virtual IActionResult AsyncUploadVideo() {
            //if (!_permissionService.Authorize(StandardPermissionProvider.UploadPictures))
            //    return Json(new { success = false, error = "You do not have required permissions" }, "text/plain");


            

            var httpPostedFile = Request.Form.Files.FirstOrDefault();
            if(httpPostedFile == null) {
                return Json(new {
                    success = false,
                    message = "No file uploaded"
                });
            }

            if(httpPostedFile.Length > VideoSize * 1000000) {
                return Json(new {
                    success = false,
                    message = $"Please upload file of size less then {VideoSize}MB"
                });
            }

            if(!IsMediaFile(httpPostedFile.FileName)) {
                return Json(new {
                    success = false,
                    message = "Please upload video file only"
                });
            }

            const string qqFileNameParameter = "qqfilename";

            var qqFileName = Request.Form.ContainsKey(qqFileNameParameter)
                ? Request.Form[qqFileNameParameter].ToString()
                : string.Empty;

            var videoPath = _pictureService.InsertVideo(httpPostedFile, qqFileName);

            //when returning JSON the mime-type must be set to text/plain
            //otherwise some browsers will pop-up a "Save As" dialog.
            return Json(new {
                success = true,
                videoPath,
                message = ""
            });
        }

        public bool IsMediaFile(string path) {
            string[] mediaExtensions = {
                ".WAV", ".MID", ".MIDI", ".WMA", ".OGG", ".RMA", //etc
                ".AVI", ".MP4", ".DIVX", ".WMV", //etc
            };
            var fileExtension = _fileProvider.GetFileExtension(path).ToUpper();
            return -1 != Array.IndexOf(mediaExtensions, fileExtension);
        }

        #endregion

        #endregion
    }
}
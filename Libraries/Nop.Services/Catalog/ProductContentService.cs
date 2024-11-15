using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.Rest.Azure;

namespace Nop.Services.Catalog
{
    public class ProductContentService : IProductContentService
    {
        #region Fields

        private readonly IRepository<ProductContentType> _productContentTypeRepository;
        private readonly IRepository<ProductContent> _productContentRepository;
        private readonly IRepository<ProductContentMapping> _productContentMappingRepository;

        #endregion

        #region Ctor

        public ProductContentService(IRepository<ProductContentType> productContentTypeRepository,
            IRepository<ProductContent> productContentRepository,
            IRepository<ProductContentMapping> productContentMappingRepository) {
            _productContentTypeRepository = productContentTypeRepository;
            _productContentRepository = productContentRepository;
            _productContentMappingRepository = productContentMappingRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Insert product content type
        /// </summary>
        /// <param name="productContentType">product content type</param>
        public virtual void InsertProductContentType(ProductContentType productContentType) {
            if(productContentType == null)
                throw new ArgumentNullException(nameof(ProductContentType));

            productContentType.CreatedDate = DateTime.Now;
            _productContentTypeRepository.Insert(productContentType);
        }

        /// <summary>
        /// Update product content type
        /// </summary>
        /// <param name="productContentType">product content type</param>
        public virtual void UpdateProductContentType(ProductContentType productContentType) {
            if(productContentType == null)
                throw new ArgumentNullException(nameof(ProductContentType));

            _productContentTypeRepository.Update(productContentType);
        }

        /// <summary>
        /// Delete product content type
        /// </summary>
        /// <param name="productContentType">product content type</param>
        public virtual void DeleteProductContentType(ProductContentType productContentType) {
            if(productContentType == null)
                throw new ArgumentNullException(nameof(ProductContentType));

            _productContentTypeRepository.Delete(productContentType);
        }

        /// <summary>
        /// Get product content type by id
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        public virtual ProductContentType GetProductContentTypeById(int id) {
            return _productContentTypeRepository.GetById(id);
        }

        /// <summary>
        /// Get all product content types
        /// </summary>
        /// <param name="pageIndex">page index</param>
        /// <param name="pageSize">page size</param>
        /// <returns></returns>
        public virtual IPagedList<ProductContentType> GetAllProductContentTypes(int pageIndex, int pageSize) {
            var query = from p in _productContentTypeRepository.Table
                        orderby p.Id descending
                        select p;

            return new PagedList<ProductContentType>(query, pageIndex, pageSize);
        }



        /// <summary>
        /// Insert product content
        /// </summary>
        /// <param name="productContent">product content</param>
        public virtual void InsertProductContent(ProductContent productContent) {
            if(productContent == null)
                throw new ArgumentNullException(nameof(ProductContent));

            productContent.CreatedDate = DateTime.Now;
            _productContentRepository.Insert(productContent);
        }

        /// <summary>
        /// Update product content
        /// </summary>
        /// <param name="productContent">product content</param>
        public virtual void UpdateProductContent(ProductContent productContent) {
            if(productContent == null)
                throw new ArgumentNullException(nameof(ProductContent));

            _productContentRepository.Update(productContent);
        }

        /// <summary>
        /// Delete product content
        /// </summary>
        /// <param name="productContent">product content</param>
        public virtual void DeleteProductContent(ProductContent productContent) {
            if(productContent == null)
                throw new ArgumentNullException(nameof(ProductContent));

            _productContentRepository.Delete(productContent);
        }

        /// <summary>
        /// Get product content by id
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        public virtual ProductContent GetProductContentById(int id) {
            return _productContentRepository.GetById(id);
        }

        /// <summary>
        /// Get all product contents
        /// </summary>
        /// <param name="pageIndex">page index</param>
        /// <param name="pageSize">page size</param>
        /// <returns></returns>
        public virtual IPagedList<ProductContent> GetAllProductContents(int pageIndex, int pageSize, int productContentTypeId = 0, string searchTitle = "") {
            var query = from p in _productContentRepository.Table
                        where (p.ProductContentTypeId == productContentTypeId || productContentTypeId == 0) &&
                        (p.Title.Contains(searchTitle) || string.IsNullOrEmpty(searchTitle))
                        orderby p.Id descending
                        select p;

            return new PagedList<ProductContent>(query, pageIndex, pageSize);
        }



        /// <summary>
        /// Insert product content mapping
        /// </summary>
        /// <param name="productContentMapping">product content mapping</param>
        public virtual void InsertProductContentMapping(ProductContentMapping productContentMapping) {
            if(productContentMapping == null)
                throw new ArgumentNullException(nameof(ProductContentMapping));

            _productContentMappingRepository.Insert(productContentMapping);
        }

        /// <summary>
        /// Update product content mapping
        /// </summary>
        /// <param name="productContentMapping">product content mapping</param>
        public virtual void UpdateProductContentMapping(ProductContentMapping productContentMapping) {
            if(productContentMapping == null)
                throw new ArgumentNullException(nameof(ProductContentMapping));

            _productContentMappingRepository.Update(productContentMapping);
        }

        /// <summary>
        /// Delete product content mapping
        /// </summary> 
        /// <param name="productContentMapping">product content mapping</param>
        public virtual void DeleteProductContentMapping(ProductContentMapping productContentMapping) {
            if(productContentMapping == null)
                throw new ArgumentNullException(nameof(ProductContentMapping));

            _productContentMappingRepository.Delete(productContentMapping);
        }

        /// <summary>
        /// Get product content mapping by id
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        public virtual ProductContentMapping GetProductContentMappingById(int id) {
            return _productContentMappingRepository.GetById(id);
        }

        /// <summary>
        /// Get all product content mappings
        /// </summary>
        /// <param name="pageIndex">page index</param>
        /// <param name="pageSize">page size</param>
        /// <returns></returns>
        public virtual IPagedList<ProductContentMapping> GetAllProductContentMappings(int pageIndex, int pageSize) {
            var query = from p in _productContentMappingRepository.Table
                        orderby p.Id descending
                        select p;

            return new PagedList<ProductContentMapping>(query, pageIndex, pageSize);
        }

        /// <summary>
        /// Get product content mapping by content id
        /// </summary>
        /// <param name="productContentId">product content id</param>
        /// <returns></returns>
        public virtual IPagedList<ProductContentMapping> GetProductContentMappingByContentId(int productContentId, int pageIndex, int pageSize) {
            var query = from p in _productContentMappingRepository.Table
                        where p.ProductContentId == productContentId
                        orderby p.Id descending
                        select p;

            return new PagedList<ProductContentMapping>(query, pageIndex, pageSize);
        }

        #endregion
    }
}

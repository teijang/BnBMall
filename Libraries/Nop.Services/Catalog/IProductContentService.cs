using Nop.Core;
using Nop.Core.Domain.Catalog;
using System.Collections.Generic;

namespace Nop.Services.Catalog
{
    public interface IProductContentService
    {

        /// <summary>
        /// Insert product content type
        /// </summary>
        /// <param name="productContentType">product content type</param>
        void InsertProductContentType(ProductContentType productContentType);

        /// <summary>
        /// Update product content type
        /// </summary>
        /// <param name="productContentType">product content type</param>
        void UpdateProductContentType(ProductContentType productContentType);

        /// <summary>
        /// Delete product content type
        /// </summary>
        /// <param name="productContentType">product content type</param>
        void DeleteProductContentType(ProductContentType productContentType);

        /// <summary>
        /// Get product content type by id
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        ProductContentType GetProductContentTypeById(int id);

        /// <summary>
        /// Get all product content types
        /// </summary>
        /// <param name="pageIndex">page index</param>
        /// <param name="pageSize">page size</param>
        /// <returns></returns>
        IPagedList<ProductContentType> GetAllProductContentTypes(int pageIndex, int pageSize);



        /// <summary>
        /// Insert product content
        /// </summary>
        /// <param name="productContent">product content</param>
        void InsertProductContent(ProductContent productContent);

        /// <summary>
        /// Update product content
        /// </summary>
        /// <param name="productContent">product content</param>
        void UpdateProductContent(ProductContent productContent);

        /// <summary>
        /// Delete product content
        /// </summary>
        /// <param name="productContent">product content</param>
        void DeleteProductContent(ProductContent productContent);

        /// <summary>
        /// Get product content by id
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        ProductContent GetProductContentById(int id);

        /// <summary>
        /// Get all product contents
        /// </summary>
        /// <param name="pageIndex">page index</param>
        /// <param name="pageSize">page size</param>
        /// <returns></returns>
        IPagedList<ProductContent> GetAllProductContents(int pageIndex, int pageSize, int productContentTypeId = 0, string searchTitle = "");



        /// <summary>
        /// Insert product content mapping
        /// </summary>
        /// <param name="productContentMapping">product content mapping</param>
        void InsertProductContentMapping(ProductContentMapping productContentMapping);

        /// <summary>
        /// Update product content mapping
        /// </summary>
        /// <param name="productContentMapping">product content mapping</param>
        void UpdateProductContentMapping(ProductContentMapping productContentMapping);

        /// <summary>
        /// Delete product content mapping
        /// </summary> 
        /// <param name="productContentMapping">product content mapping</param>
        void DeleteProductContentMapping(ProductContentMapping productContentMapping);

        /// <summary>
        /// Get product content mapping by id
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        ProductContentMapping GetProductContentMappingById(int id);

        /// <summary>
        /// Get all product content mappings
        /// </summary>
        /// <param name="pageIndex">page index</param>
        /// <param name="pageSize">page size</param>
        /// <returns></returns>
        IPagedList<ProductContentMapping> GetAllProductContentMappings(int pageIndex, int pageSize);

        /// <summary>
        /// Get product content mapping by content id
        /// </summary>
        /// <param name="productContentId">product content id</param>
        /// <returns></returns>
        IPagedList<ProductContentMapping> GetProductContentMappingByContentId(int productContentId, int pageIndex, int pageSize);
    }
}
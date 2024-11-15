using Nop.Core;
using Nop.Core.Domain.Catalog;
using System;
using System.Collections.Generic;

namespace Nop.Services.Catalog
{
    public interface IProductQAService
    {
        /// <summary>
        /// Get product qas by identifiers
        /// </summary>
        /// <param name="productQAIds">Product qa identifiers</param>
        /// <returns>Product qaa</returns>
        IList<ProductQA> GetProductQAsByIds(int[] productQAIds);

        /// <summary>
        /// insert product qa
        /// </summary>
        /// <param name="productQA"></param>
        void InsertProductQA(ProductQA productQA);

        /// <summary>
        /// Delete product QA
        /// </summary>
        /// <param name="productQA"></param>
        void DeleteProductQA(ProductQA productQA);

        /// <summary>
        /// Update product QA
        /// </summary>
        /// <param name="productQA"></param>
        void UpdateProductQA(ProductQA productQA);

        /// <summary>
        /// Get product qa by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        ProductQA GetProductQAById(int id);

        /// <summary>
        /// Get all product QA
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        IList<ProductQA> GetAllProductQA(int productId);

        /// <summary>
        /// Deletes product QAs
        /// </summary>
        /// <param name="productQAs">Product qa</param>
        void DeleteProductQAs(IList<ProductQA> productQAs);

        /// <summary>
        /// Get all product qas
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="approved"></param>
        /// <param name="fromUtc"></param>
        /// <param name="toUtc"></param>
        /// <param name="message"></param>
        /// <param name="storeId"></param>
        /// <param name="productId"></param>
        /// <param name="vendorId"></param>
        /// <param name="showHidden"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        IPagedList<ProductQA> GetAllProductQAs(int customerId = 0, bool? approved = null,
           DateTime? fromUtc = null, DateTime? toUtc = null,
           string message = null, int storeId = 0, int productId = 0, int vendorId = 0, bool showHidden = false,
           int pageIndex = 0, int pageSize = int.MaxValue);
    }
}
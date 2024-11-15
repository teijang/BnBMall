using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Stores;
using Nop.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Services.Catalog
{
    public class ProductQAService : IProductQAService
    {
        #region Fields

        private readonly IRepository<ProductQA> _productQARepository;
        private readonly CatalogSettings _catalogSettings;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<StoreMapping> _storeMappingRepository;

        #endregion

        #region Ctor

        public ProductQAService(IRepository<ProductQA> productQARepository,
            CatalogSettings catalogSettings,
            IRepository<Product> productRepository,
            IRepository<StoreMapping> storeMappingRepository) {
            _productQARepository = productQARepository;
            _catalogSettings = catalogSettings;
            _productRepository = productRepository;
            _storeMappingRepository = storeMappingRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get product qas by identifiers
        /// </summary>
        /// <param name="productQAIds">Product qa identifiers</param>
        /// <returns>Product qaa</returns>
        public virtual IList<ProductQA> GetProductQAsByIds(int[] productQAIds) {
            if(productQAIds == null || productQAIds.Length == 0)
                return new List<ProductQA>();

            var query = from pr in _productQARepository.Table
                        where productQAIds.Contains(pr.Id)
                        select pr;
            var productQAs = query.ToList();
            //sort by passed identifiers
            var sortedProductQAs = new List<ProductQA>();
            foreach(var id in productQAIds) {
                var productReview = productQAs.Find(x => x.Id == id);
                if(productReview != null)
                    sortedProductQAs.Add(productReview);
            }

            return sortedProductQAs;
        }

        /// <summary>
        /// insert product qa
        /// </summary>
        /// <param name="productQA"></param>
        public virtual void InsertProductQA(ProductQA productQA) {
            if(productQA == null)
                throw new ArgumentNullException("ProductQA");

            _productQARepository.Insert(productQA);
        }

        /// <summary>
        /// Delete product QA
        /// </summary>
        /// <param name="productQA"></param>
        public virtual void DeleteProductQA(ProductQA productQA) {
            if(productQA == null)
                throw new ArgumentNullException("ProductQA");

            _productQARepository.Delete(productQA);
        }

        /// <summary>
        /// Update product QA
        /// </summary>
        /// <param name="productQA"></param>
        public virtual void UpdateProductQA(ProductQA productQA) {
            if(productQA == null)
                throw new ArgumentNullException("ProductQA");

            _productQARepository.Update(productQA);
        }

        /// <summary>
        /// Get product qa by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual ProductQA GetProductQAById(int id) {
            return _productQARepository.GetById(id);
        }

        /// <summary>
        /// Get all product QA
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public virtual IList<ProductQA> GetAllProductQA(int productId) {
            var query = from p in _productQARepository.Table
                        where p.ProductId == productId
                        && p.IsApproved 
                        && !string.IsNullOrEmpty(p.Answer)
                        orderby p.Id descending
                        select p;

            return query.ToList();
        }

        /// <summary>
        /// Deletes product QAs
        /// </summary>
        /// <param name="productQAs">Product qa</param>
        public virtual void DeleteProductQAs(IList<ProductQA> productQAs) {
            if(productQAs == null)
                throw new ArgumentNullException(nameof(productQAs));

            _productQARepository.Delete(productQAs);
        }

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
        public virtual IPagedList<ProductQA> GetAllProductQAs(int customerId = 0, bool? approved = null,
           DateTime? fromUtc = null, DateTime? toUtc = null,
           string message = null, int storeId = 0, int productId = 0, int vendorId = 0, bool showHidden = false,
           int pageIndex = 0, int pageSize = int.MaxValue) {
            var query = _productQARepository.Table;

            if(approved.HasValue)
                query = query.Where(pr => pr.IsApproved == approved);
            if(customerId > 0)
                query = query.Where(pr => pr.AskedBy == customerId);
            if(fromUtc.HasValue)
                query = query.Where(pr => fromUtc.Value <= pr.CreatedDate);
            if(toUtc.HasValue)
                query = query.Where(pr => toUtc.Value >= pr.CreatedDate);
            if(!string.IsNullOrEmpty(message))
                query = query.Where(pr => pr.Question.Contains(message) || pr.Question.Contains(message));
            if(storeId > 0 && (showHidden || _catalogSettings.ShowProductReviewsPerStore))
                query = query.Where(pr => pr.StoreId == storeId);
            if(productId > 0)
                query = query.Where(pr => pr.ProductId == productId);

            query = from productReview in query
                    join product in _productRepository.Table on productReview.ProductId equals product.Id
                    where
                        (vendorId == 0 || product.VendorId == vendorId) &&
                        //ignore deleted products
                        !product.Deleted
                    select productReview;

            //filter by limited to store products
            if(storeId > 0 && !showHidden && !_catalogSettings.IgnoreStoreLimitations) {
                query = from productReview in query
                        join product in _productRepository.Table on productReview.ProductId equals product.Id
                        join storeMapping in _storeMappingRepository.Table
                            on new { Id = productReview.ProductId, Name = nameof(Product) }
                            equals new { Id = storeMapping.EntityId, Name = storeMapping.EntityName } into storeMappingsWithNulls
                        from storeMapping in storeMappingsWithNulls.DefaultIfEmpty()
                        where !product.LimitedToStores || storeMapping.StoreId == storeId
                        select productReview;
            }

            query = _catalogSettings.ProductReviewsSortByCreatedDateAscending
                ? query.OrderBy(pr => pr.CreatedDate).ThenBy(pr => pr.Id)
                : query.OrderByDescending(pr => pr.CreatedDate).ThenBy(pr => pr.Id);

            var productReviews = new PagedList<ProductQA>(query, pageIndex, pageSize);

            return productReviews;
        }

        #endregion
    }
}

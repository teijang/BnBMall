using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Html;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Extensions;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the product review model factory implementation
    /// </summary>
    public partial class ProductQAModelFactory : IProductQAModelFactory
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IProductService _productService;
        private readonly IReviewTypeService _reviewTypeService;
        private readonly IStoreService _storeService;
        private readonly IWorkContext _workContext;
        private readonly IProductQAService _productQAService;

        #endregion

        #region Ctor

        public ProductQAModelFactory(CatalogSettings catalogSettings,
            IBaseAdminModelFactory baseAdminModelFactory,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            IProductService productService,
            IReviewTypeService reviewTypeService,
            IStoreService storeService,
            IWorkContext workContext,
            IProductQAService productQAService)
        {
            _catalogSettings = catalogSettings;
            _baseAdminModelFactory = baseAdminModelFactory;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _localizationService = localizationService;
            _productService = productService;
            _reviewTypeService = reviewTypeService;
            _storeService = storeService;
            _workContext = workContext;
            _productQAService = productQAService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare product qa search model
        /// </summary>
        /// <param name="searchModel">Product qa search model</param>
        /// <returns>Product qa search model</returns>
        public virtual ProductQASearchModel PrepareProductQASearchModel(ProductQASearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.IsLoggedInAsVendor = _workContext.CurrentVendor != null;

            //prepare available stores
            _baseAdminModelFactory.PrepareStores(searchModel.AvailableStores);

            //prepare "approved" property (0 - all; 1 - approved only; 2 - disapproved only)
            searchModel.AvailableApprovedOptions.Add(new SelectListItem
            {
                Text = _localizationService.GetResource("Admin.Catalog.ProductQA.List.SearchApproved.All"),
                Value = "0"
            });
            searchModel.AvailableApprovedOptions.Add(new SelectListItem
            {
                Text = _localizationService.GetResource("Admin.Catalog.ProductQA.List.SearchApproved.ApprovedOnly"),
                Value = "1"
            });
            searchModel.AvailableApprovedOptions.Add(new SelectListItem
            {
                Text = _localizationService.GetResource("Admin.Catalog.ProductQA.List.SearchApproved.DisapprovedOnly"),
                Value = "2"
            });

            searchModel.HideStoresList = _catalogSettings.IgnoreStoreLimitations || searchModel.AvailableStores.SelectionIsNotPossible();

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare paged product review list model
        /// </summary>
        /// <param name="searchModel">Product review search model</param>
        /// <returns>Product review list model</returns>
        public virtual ProductQAListModel PrepareProductQAListModel(ProductQASearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter reviews
            var createdOnFromValue = !searchModel.CreatedOnFrom.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.CreatedOnFrom.Value, _dateTimeHelper.CurrentTimeZone);
            var createdToFromValue = !searchModel.CreatedOnTo.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.CreatedOnTo.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);
            var isApprovedOnly = searchModel.SearchApprovedId == 0 ? null : searchModel.SearchApprovedId == 1 ? true : (bool?)false;
            var vendorId = _workContext.CurrentVendor?.Id ?? 0;

            //get product reviews
            var productQAs = _productQAService.GetAllProductQAs(showHidden: true,
                customerId: 0,
                approved: isApprovedOnly,
                fromUtc: createdOnFromValue,
                toUtc: createdToFromValue,
                message: searchModel.SearchText,
                storeId: searchModel.SearchStoreId,
                productId: searchModel.SearchProductId,
                vendorId: vendorId,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare list model
            var model = new ProductQAListModel().PrepareToGrid(searchModel, productQAs, () =>
            {
                return productQAs.Select(productQA =>
                {
                    //fill in model values from the entity
                    var productQAModel = productQA.ToModel<ProductQAModel>();

                    //convert dates to the user time
                    productQAModel.CreatedOn = _dateTimeHelper.ConvertToUserTime(productQA.CreatedDate, DateTimeKind.Utc);

                    //fill in additional values (not existing in the entity)
                    productQAModel.StoreName = _storeService.GetStoreById(productQA.StoreId)?.Name;
                    productQAModel.ProductName = _productService.GetProductById(productQA.ProductId)?.Name;
                    productQAModel.CustomerInfo = _customerService.GetCustomerById(productQA.AskedBy) is Customer customer && _customerService.IsRegistered(customer)
                        ? customer.Email : _localizationService.GetResource("Admin.Customers.Guest");
                    productQAModel.Question = HtmlHelper.FormatText(productQA.Question, false, true, false, false, false, false);
                    productQAModel.Answer = HtmlHelper.FormatText(productQA.Answer, false, true, false, false, false, false);

                    return productQAModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare product qa model
        /// </summary>
        /// <param name="model">Product qa model</param>
        /// <param name="productReview">Product qa</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>Product qa model</returns>
        public virtual ProductQAModel PrepareProductQAModel(ProductQAModel model,
            ProductQA productQA, bool excludeProperties = false)
        {
            if (productQA != null)
            {
                //fill in model values from the entity
                model ??= new ProductQAModel {
                    Id = productQA.Id,
                    StoreName = _storeService.GetStoreById(productQA.StoreId)?.Name,
                    ProductId = productQA.ProductId,
                    ProductName = _productService.GetProductById(productQA.ProductId)?.Name,
                    CustomerId = productQA.AskedBy
                };

                model.CustomerInfo = _customerService.GetCustomerById(productQA.AskedBy) is Customer customer && _customerService.IsRegistered(customer)
                    ? customer.Email : _localizationService.GetResource("Admin.Customers.Guest");

                model.CreatedOn = _dateTimeHelper.ConvertToUserTime(productQA.CreatedDate, DateTimeKind.Utc);

                if (!excludeProperties)
                {
                    model.Question = productQA.Question;
                    model.Answer = productQA.Answer;
                    model.IsApproved = productQA.IsApproved;
                }
            }

            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;

            return model;
        }

        #endregion
    }
}
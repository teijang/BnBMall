using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Web.Components
{
    public class ProductReviewsViewComponent : NopViewComponent
    {
        private readonly IProductService _productService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;
        private readonly CatalogSettings _catalogSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderService _orderService;

        public ProductReviewsViewComponent(IProductService productService,
            IProductModelFactory productModelFactory,
            ICustomerService customerService,
            IWorkContext workContext,
            CatalogSettings catalogSettings,
            ILocalizationService localizationService,
            IOrderService orderService) {
            _productService = productService;
            _productModelFactory = productModelFactory;
            _customerService = customerService;
            _workContext = workContext;
            _catalogSettings = catalogSettings;
            _localizationService = localizationService;
            _orderService = orderService;
        }

        public IViewComponentResult Invoke() {
            var productIdValue = Request.RouteValues["productId"];
            if(productIdValue != null) {
                var productId = Convert.ToInt32(productIdValue);
                var product = _productService.GetProductById(productId);
                if(product == null || product.Deleted || !product.Published || !product.AllowCustomerReviews)
                    return Content("");

                var model = new ProductReviewsModel();
                model = _productModelFactory.PrepareProductReviewsModel(model, product);
                //only registered users can leave reviews
                ModelState.Clear();
                if(_customerService.IsGuest(_workContext.CurrentCustomer) && !_catalogSettings.AllowAnonymousUsersToReviewProduct)
                    ModelState.AddModelError("", _localizationService.GetResource("Reviews.OnlyRegisteredUsersCanWriteReviews"));

                if(_catalogSettings.ProductReviewPossibleOnlyAfterPurchasing) {
                    var hasCompletedOrders = _orderService.SearchOrders(customerId: _workContext.CurrentCustomer.Id,
                        productId: productId,
                        osIds: new List<int> { (int)OrderStatus.Complete },
                        pageSize: 1).Any();
                    if(!hasCompletedOrders) {
                        ModelState.AddModelError(string.Empty, _localizationService.GetResource("Reviews.ProductReviewPossibleOnlyAfterPurchasing"));
                        model.AddProductReview.CanCurrentCustomerLeaveReview = false;
                    }
                }

                //default value
                model.AddProductReview.Rating = _catalogSettings.DefaultProductRatingValue;
                model.AddProductReview.ReviewProductId = productId;

                //default value for all additional review types
                if(model.ReviewTypeList.Count > 0)
                    foreach(var additionalProductReview in model.AddAdditionalProductReviewList) {
                        additionalProductReview.Rating = additionalProductReview.IsRequired ? _catalogSettings.DefaultProductRatingValue : 0;
                    }

                return View(model);
            } else {
                return Content("");
            }
        }
    }
}

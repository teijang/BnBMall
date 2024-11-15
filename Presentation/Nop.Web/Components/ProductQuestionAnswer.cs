using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Catalog;
using System;

namespace Nop.Web.Components
{
    public class ProductQuestionAnswerViewComponent : NopViewComponent
    {
        #region Fields

        private readonly IProductQAService _productQAService;
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor

        public ProductQuestionAnswerViewComponent(IProductQAService productQAService,
            ICustomerService customerService,
            IWorkContext workContext,
            ILocalizationService localizationService) {
            _productQAService = productQAService;
            _customerService = customerService;
            _workContext = workContext;
            _localizationService = localizationService;
        }

        #endregion

        #region Method

        public IViewComponentResult Invoke() {
            var productIdValue = Request.RouteValues["productId"];
            if(productIdValue != null) {
                var productId = Convert.ToInt32(productIdValue);
                ProductQAListModel model = new ProductQAListModel {
                    CustomerCanAddQuestion = _customerService.IsRegistered(_workContext.CurrentCustomer),
                    CurrentCustomerId = _workContext.CurrentCustomer.Id,
                    ProductId = productId
                };

                ModelState.Clear();
                if(!model.CustomerCanAddQuestion) {
                    ModelState.AddModelError("", _localizationService.GetResource("ProductQA.LoginCustomerCanAddQuestionOnly"));
                }

                var productQAs = _productQAService.GetAllProductQA(productId);
                foreach(var item in productQAs) {
                    ProductQAModel productQAModel = new ProductQAModel {
                        Question = item.Question,
                        Answer = item.Answer,
                        CreatedDate = item.CreatedDate.ToString("d MMM, yyyy")
                    };
                    if(item.AskedBy > 0) {
                        var askedByName = _customerService.GetCustomerFullName(_customerService.GetCustomerById(item.AskedBy));
                        productQAModel.AskedBy = askedByName;
                    }

                    if(item.RepliedBy > 0) {
                        var repliedByName = _customerService.GetCustomerFullName(_customerService.GetCustomerById(item.RepliedBy.Value));
                        productQAModel.RepliedBy = repliedByName;
                    }

                    model.ProductQAList.Add(productQAModel);
                }

                return View(model);
            } else {
                return Content("");
            }
        }

        #endregion
    }
}

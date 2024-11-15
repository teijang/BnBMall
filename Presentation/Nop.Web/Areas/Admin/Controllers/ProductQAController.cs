using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class ProductQAController : BaseAdminController
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IProductQAModelFactory _productQAModelFactory;
        private readonly IProductQAService _productQAService;
        private readonly IProductService _productService;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;

        #endregion Fields

        #region Ctor

        public ProductQAController(CatalogSettings catalogSettings,
            ICustomerActivityService customerActivityService,
            IEventPublisher eventPublisher,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            IProductQAService productQAService,
            IProductQAModelFactory productQAModelFactory,
            IProductService productService,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService)
        {
            _catalogSettings = catalogSettings;
            _customerActivityService = customerActivityService;
            _eventPublisher = eventPublisher;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _productQAModelFactory = productQAModelFactory;
            _productService = productService;
            _productQAService = productQAService;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
        }

        #endregion

        #region Methods

        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProductQuestions))
                return AccessDeniedView();

            //prepare model
            var model = _productQAModelFactory.PrepareProductQASearchModel(new ProductQASearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult List(ProductQASearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProductQuestions))
                return AccessDeniedDataTablesJson();

            //prepare model
            var model = _productQAModelFactory.PrepareProductQAListModel(searchModel);

            return Json(model);
        }

        public virtual IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProductQuestions))
                return AccessDeniedView();

            //try to get a product review with the specified id
            var productQA = _productQAService.GetProductQAById(id);
            if (productQA == null)
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && _productService.GetProductById(productQA.ProductId).VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List");

            //prepare model
            var model = _productQAModelFactory.PrepareProductQAModel(null, productQA);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult Edit(ProductQAModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProductQuestions))
                return AccessDeniedView();

            //try to get a product qa with the specified id
            var productQA = _productQAService.GetProductQAById(model.Id);
            if (productQA == null)
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && _productService.GetProductById(productQA.ProductId).VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                var previousIsApproved = productQA.IsApproved;

                //vendor can edit "Reply text" only
                var isLoggedInAsVendor = _workContext.CurrentVendor != null;
                if (!isLoggedInAsVendor)
                {
                    productQA.Question = model.Question;
                    productQA.IsApproved = model.IsApproved;
                }

                productQA.Answer = model.Answer;              

                _productQAService.UpdateProductQA(productQA);

                //activity log
                _customerActivityService.InsertActivity("EditProductQA",
                   string.Format(_localizationService.GetResource("ActivityLog.EditProductQA"), productQA.Id), productQA);


                _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Catalog.ProductQAs.Updated"));

                return continueEditing ? RedirectToAction("Edit", new { id = productQA.Id }) : RedirectToAction("List");
            }

            //prepare model
            model = _productQAModelFactory.PrepareProductQAModel(model, productQA, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public virtual IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProductQuestions))
                return AccessDeniedView();

            //try to get a product review with the specified id
            var productQA = _productQAService.GetProductQAById(id);
            if (productQA == null)
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("List");

            _productQAService.DeleteProductQA(productQA);

            //activity log
            _customerActivityService.InsertActivity("DeleteProductQA",
                string.Format(_localizationService.GetResource("ActivityLog.DeleteProductQA"), productQA.Id), productQA);

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Catalog.ProductQA.Deleted"));

            return RedirectToAction("List");
        }

        [HttpPost]
        public virtual IActionResult ApproveSelected(ICollection<int> selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProductQuestions))
                return AccessDeniedView();

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("List");

            if (selectedIds == null)
                return Json(new { Result = true });

            //filter not approved qa
            var productQAs = _productQAService.GetProductQAsByIds(selectedIds.ToArray()).Where(review => !review.IsApproved);
            foreach (var productQA in productQAs)
            {
                productQA.IsApproved = true;
                _productQAService.UpdateProductQA(productQA);
            }

            return Json(new { Result = true });
        }

        [HttpPost]
        public virtual IActionResult DisapproveSelected(ICollection<int> selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProductQuestions))
                return AccessDeniedView();

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("List");

            if (selectedIds == null)
                return Json(new { Result = true });

            //filter approved reviews
            var productQAs = _productQAService.GetProductQAsByIds(selectedIds.ToArray()).Where(review => review.IsApproved);
            foreach (var productQA in productQAs)
            {
                productQA.IsApproved = false;
                _productQAService.UpdateProductQA(productQA);
            }

            return Json(new { Result = true });
        }

        [HttpPost]
        public virtual IActionResult DeleteSelected(ICollection<int> selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProductQuestions))
                return AccessDeniedView();

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("List");

            if (selectedIds == null)
                return Json(new { Result = true });

            var productQAs = _productQAService.GetProductQAsByIds(selectedIds.ToArray());
            var products = _productService.GetProductsByIds(productQAs.Select(p => p.ProductId).Distinct().ToArray());

            _productQAService.DeleteProductQAs(productQAs);

            return Json(new { Result = true });
        }

        public virtual IActionResult InsertLocale() {
            _localizationService.InsertLocaleStringResource(new Core.Domain.Localization.LocaleStringResource {
                LanguageId = _workContext.WorkingLanguage.Id,
                ResourceName = "Admin.Catalog.ProductQA",
                ResourceValue = "Product Questions"
            });

            _localizationService.InsertLocaleStringResource(new Core.Domain.Localization.LocaleStringResource {
                LanguageId = _workContext.WorkingLanguage.Id,
                ResourceName = "Admin.Catalog.ProductQAs",
                ResourceValue = "Product Questions"
            });

            _localizationService.InsertLocaleStringResource(new Core.Domain.Localization.LocaleStringResource {
                LanguageId = _workContext.WorkingLanguage.Id,
                ResourceName = "Admin.Catalog.ProductQAs.ApproveSelected",
                ResourceValue = "Approve selected"
            });

            _localizationService.InsertLocaleStringResource(new Core.Domain.Localization.LocaleStringResource {
                LanguageId = _workContext.WorkingLanguage.Id,
                ResourceName = "Admin.Catalog.ProductQAs.BackToList",
                ResourceValue = "back to product question list"
            });

            _localizationService.InsertLocaleStringResource(new Core.Domain.Localization.LocaleStringResource {
                LanguageId = _workContext.WorkingLanguage.Id,
                ResourceName = "Admin.Catalog.ProductQAs.Deleted",
                ResourceValue = "The product question has been deleted successfully."
            });

            _localizationService.InsertLocaleStringResource(new Core.Domain.Localization.LocaleStringResource {
                LanguageId = _workContext.WorkingLanguage.Id,
                ResourceName = "Admin.Catalog.ProductQAs.DeleteSelected",
                ResourceValue = "Delete selected"
            });

            _localizationService.InsertLocaleStringResource(new Core.Domain.Localization.LocaleStringResource {
                LanguageId = _workContext.WorkingLanguage.Id,
                ResourceName = "Admin.Catalog.ProductQAs.DisapproveSelected",
                ResourceValue = "Disapprove selected"
            });

            _localizationService.InsertLocaleStringResource(new Core.Domain.Localization.LocaleStringResource {
                LanguageId = _workContext.WorkingLanguage.Id,
                ResourceName = "Admin.Catalog.ProductQAs.EditProductQADetails",
                ResourceValue = "Edit product question details"
            });

            _localizationService.InsertLocaleStringResource(new Core.Domain.Localization.LocaleStringResource {
                LanguageId = _workContext.WorkingLanguage.Id,
                ResourceName = "Admin.Catalog.ProductQA.Fields.CreatedOn",
                ResourceValue = "Created on"
            });

            _localizationService.InsertLocaleStringResource(new Core.Domain.Localization.LocaleStringResource {
                LanguageId = _workContext.WorkingLanguage.Id,
                ResourceName = "Admin.Catalog.ProductQA.Fields.CreatedOn.Hint",
                ResourceValue = "The date/time that the question was created."
            });

            _localizationService.InsertLocaleStringResource(new Core.Domain.Localization.LocaleStringResource {
                LanguageId = _workContext.WorkingLanguage.Id,
                ResourceName = "Admin.Catalog.ProductQA.Fields.Customer",
                ResourceValue = "Customer"
            });

            _localizationService.InsertLocaleStringResource(new Core.Domain.Localization.LocaleStringResource {
                LanguageId = _workContext.WorkingLanguage.Id,
                ResourceName = "Admin.Catalog.ProductQA.Fields.Customer.Hint",
                ResourceValue = "The customer who created the question."
            });

            _localizationService.InsertLocaleStringResource(new Core.Domain.Localization.LocaleStringResource {
                LanguageId = _workContext.WorkingLanguage.Id,
                ResourceName = "Admin.Catalog.ProductQA.Fields.IsApproved",
                ResourceValue = "Is approved"
            });

            _localizationService.InsertLocaleStringResource(new Core.Domain.Localization.LocaleStringResource {
                LanguageId = _workContext.WorkingLanguage.Id,
                ResourceName = "Admin.Catalog.ProductQA.Fields.IsApproved.Hint",
                ResourceValue = "Is the question approved? Marking it as approved means that it is visible to all your site's visitors."
            });

            _localizationService.InsertLocaleStringResource(new Core.Domain.Localization.LocaleStringResource {
                LanguageId = _workContext.WorkingLanguage.Id,
                ResourceName = "Admin.Catalog.ProductQA.Fields.Product",
                ResourceValue = "Product"
            });

            _localizationService.InsertLocaleStringResource(new Core.Domain.Localization.LocaleStringResource {
                LanguageId = _workContext.WorkingLanguage.Id,
                ResourceName = "Admin.Catalog.ProductQA.Fields.Product.Hint",
                ResourceValue = "The name of the product that the question is for."
            });

            _localizationService.InsertLocaleStringResource(new Core.Domain.Localization.LocaleStringResource {
                LanguageId = _workContext.WorkingLanguage.Id,
                ResourceName = "Admin.Catalog.ProductQA.Fields.Store",
                ResourceValue = "Store"
            });

            _localizationService.InsertLocaleStringResource(new Core.Domain.Localization.LocaleStringResource {
                LanguageId = _workContext.WorkingLanguage.Id,
                ResourceName = "Admin.Catalog.ProductQA.Fields.Store.Hint",
                ResourceValue = "A store name in which this question was written."
            });

            _localizationService.InsertLocaleStringResource(new Core.Domain.Localization.LocaleStringResource {
                LanguageId = _workContext.WorkingLanguage.Id,
                ResourceName = "Admin.Catalog.ProductQA.List.CreatedOnFrom",
                ResourceValue = "Created from"
            });

            _localizationService.InsertLocaleStringResource(new Core.Domain.Localization.LocaleStringResource {
                LanguageId = _workContext.WorkingLanguage.Id,
                ResourceName = "Admin.Catalog.ProductQA.List.CreatedOnFrom.Hint",
                ResourceValue = "The creation from date for the search."
            });

            _localizationService.InsertLocaleStringResource(new Core.Domain.Localization.LocaleStringResource {
                LanguageId = _workContext.WorkingLanguage.Id,
                ResourceName = "Admin.Catalog.ProductQA.List.CreatedOnTo",
                ResourceValue = "Created to"
            });

            _localizationService.InsertLocaleStringResource(new Core.Domain.Localization.LocaleStringResource {
                LanguageId = _workContext.WorkingLanguage.Id,
                ResourceName = "Admin.Catalog.ProductQA.List.CreatedOnTo.Hint",
                ResourceValue = "The creation to date for the search."
            });

            _localizationService.InsertLocaleStringResource(new Core.Domain.Localization.LocaleStringResource {
                LanguageId = _workContext.WorkingLanguage.Id,
                ResourceName = "Admin.Catalog.ProductQA.List.SearchApproved",
                ResourceValue = "Approved"
            });

            _localizationService.InsertLocaleStringResource(new Core.Domain.Localization.LocaleStringResource {
                LanguageId = _workContext.WorkingLanguage.Id,
                ResourceName = "Admin.Catalog.ProductQA.List.SearchApproved.All",
                ResourceValue = "All"
            });

            _localizationService.InsertLocaleStringResource(new Core.Domain.Localization.LocaleStringResource {
                LanguageId = _workContext.WorkingLanguage.Id,
                ResourceName = "Admin.Catalog.ProductQA.List.SearchApproved.ApprovedOnly",
                ResourceValue = "Approved only"
            });

            _localizationService.InsertLocaleStringResource(new Core.Domain.Localization.LocaleStringResource {
                LanguageId = _workContext.WorkingLanguage.Id,
                ResourceName = "Admin.Catalog.ProductQA.List.SearchApproved.DisapprovedOnly",
                ResourceValue = "Disapproved only"
            });

            _localizationService.InsertLocaleStringResource(new Core.Domain.Localization.LocaleStringResource {
                LanguageId = _workContext.WorkingLanguage.Id,
                ResourceName = "Admin.Catalog.ProductQA.List.SearchApproved.Hint",
                ResourceValue = "Search by a \"Approved\" property."
            });

            _localizationService.InsertLocaleStringResource(new Core.Domain.Localization.LocaleStringResource {
                LanguageId = _workContext.WorkingLanguage.Id,
                ResourceName = "Admin.Catalog.ProductQA.List.SearchProduct",
                ResourceValue = "Product"
            });

            _localizationService.InsertLocaleStringResource(new Core.Domain.Localization.LocaleStringResource {
                LanguageId = _workContext.WorkingLanguage.Id,
                ResourceName = "Admin.Catalog.ProductQA.List.SearchProduct.Hint",
                ResourceValue = "Search by a specific product."
            });

            _localizationService.InsertLocaleStringResource(new Core.Domain.Localization.LocaleStringResource {
                LanguageId = _workContext.WorkingLanguage.Id,
                ResourceName = "Admin.Catalog.ProductQA.List.SearchStore",
                ResourceValue = "Store"
            });

            _localizationService.InsertLocaleStringResource(new Core.Domain.Localization.LocaleStringResource {
                LanguageId = _workContext.WorkingLanguage.Id,
                ResourceName = "Admin.Catalog.ProductQA.List.SearchStore.Hint",
                ResourceValue = "Search by a specific store."
            });

            _localizationService.InsertLocaleStringResource(new Core.Domain.Localization.LocaleStringResource {
                LanguageId = _workContext.WorkingLanguage.Id,
                ResourceName = "Admin.Catalog.ProductQA.List.SearchText",
                ResourceValue = "Message"
            });

            _localizationService.InsertLocaleStringResource(new Core.Domain.Localization.LocaleStringResource {
                LanguageId = _workContext.WorkingLanguage.Id,
                ResourceName = "Admin.Catalog.ProductQA.List.SearchText.Hint",
                ResourceValue = "Search in question text."
            });

            _localizationService.InsertLocaleStringResource(new Core.Domain.Localization.LocaleStringResource {
                LanguageId = _workContext.WorkingLanguage.Id,
                ResourceName = "Admin.Catalog.ProductQA.Updated",
                ResourceValue = "The product question has been updated successfully."
            });

            _localizationService.InsertLocaleStringResource(new Core.Domain.Localization.LocaleStringResource {
                LanguageId = _workContext.WorkingLanguage.Id,
                ResourceName = "admin.catalog.productqa.fields.question",
                ResourceValue = "Question"
            });

            _localizationService.InsertLocaleStringResource(new Core.Domain.Localization.LocaleStringResource {
                LanguageId = _workContext.WorkingLanguage.Id,
                ResourceName = "admin.catalog.productqa.fields.question.hint",
                ResourceValue = "Question"
            });

            _localizationService.InsertLocaleStringResource(new Core.Domain.Localization.LocaleStringResource {
                LanguageId = _workContext.WorkingLanguage.Id,
                ResourceName = "admin.catalog.productqa.fields.answer",
                ResourceValue = "Answer"
            });

            _localizationService.InsertLocaleStringResource(new Core.Domain.Localization.LocaleStringResource {
                LanguageId = _workContext.WorkingLanguage.Id,
                ResourceName = "admin.catalog.productqa.fields.answer.hint",
                ResourceValue = "Answer"
            });

            _localizationService.InsertLocaleStringResource(new Core.Domain.Localization.LocaleStringResource {
                LanguageId = _workContext.WorkingLanguage.Id,
                ResourceName = "admin.catalog.productqa.backtolist",
                ResourceValue = "Back to list"
            });

            _localizationService.InsertLocaleStringResource(new Core.Domain.Localization.LocaleStringResource {
                LanguageId = _workContext.WorkingLanguage.Id,
                ResourceName = "admin.catalog.productqa.editproductqadetails",
                ResourceValue = "Edit Question"
            });

            _localizationService.InsertLocaleStringResource(new Core.Domain.Localization.LocaleStringResource {
                LanguageId = _workContext.WorkingLanguage.Id,
                ResourceName = "product.qa",
                ResourceValue = "Questions"
            });

            _localizationService.InsertLocaleStringResource(new Core.Domain.Localization.LocaleStringResource {
                LanguageId = _workContext.WorkingLanguage.Id,
                ResourceName = "productqa.writequestion",
                ResourceValue = "Write your own question"
            });

            _localizationService.InsertLocaleStringResource(new Core.Domain.Localization.LocaleStringResource {
                LanguageId = _workContext.WorkingLanguage.Id,
                ResourceName = "ProductQA.Question",
                ResourceValue = "Question"
            });

            _localizationService.InsertLocaleStringResource(new Core.Domain.Localization.LocaleStringResource {
                LanguageId = _workContext.WorkingLanguage.Id,
                ResourceName = "ProductQA.Enquiry.Hint",
                ResourceValue = "Enter your question"
            });

            _localizationService.InsertLocaleStringResource(new Core.Domain.Localization.LocaleStringResource {
                LanguageId = _workContext.WorkingLanguage.Id,
                ResourceName = "ProductQA.AddNew",
                ResourceValue = "Add New"
            });

            _localizationService.InsertLocaleStringResource(new Core.Domain.Localization.LocaleStringResource {
                LanguageId = _workContext.WorkingLanguage.Id,
                ResourceName = "reviews.cancelbutton",
                ResourceValue = "Cancel"
            });

            return Content("Done");
        }

        #endregion
    }
}
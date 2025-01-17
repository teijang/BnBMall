﻿using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Messages;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Messages;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Linq;

namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class EmailAccountController : BaseAdminController
    {
        #region Fields

        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IEmailAccountModelFactory _emailAccountModelFactory;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IEmailSender _emailSender;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IMessageTemplateService _messageTemplateService;

        #endregion

        #region Ctor

        public EmailAccountController(EmailAccountSettings emailAccountSettings,
            ICustomerActivityService customerActivityService,
            IEmailAccountModelFactory emailAccountModelFactory,
            IEmailAccountService emailAccountService,
            IEmailSender emailSender,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext,
            IMessageTemplateService messageTemplateService) {
            _emailAccountSettings = emailAccountSettings;
            _customerActivityService = customerActivityService;
            _emailAccountModelFactory = emailAccountModelFactory;
            _emailAccountService = emailAccountService;
            _emailSender = emailSender;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
            _messageTemplateService = messageTemplateService;
        }

        #endregion

        #region Methods

        public virtual IActionResult List() {
            if(!_permissionService.Authorize(StandardPermissionProvider.ManageEmailAccounts))
                return AccessDeniedView();

            //prepare model
            var model = _emailAccountModelFactory.PrepareEmailAccountSearchModel(new EmailAccountSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult List(EmailAccountSearchModel searchModel) {
            if(!_permissionService.Authorize(StandardPermissionProvider.ManageEmailAccounts))
                return AccessDeniedDataTablesJson();

            //prepare model
            var model = _emailAccountModelFactory.PrepareEmailAccountListModel(searchModel);

            return Json(model);
        }

        public virtual IActionResult MarkAsDefaultEmail(int id) {
            if(!_permissionService.Authorize(StandardPermissionProvider.ManageEmailAccounts))
                return AccessDeniedView();

            var defaultEmailAccount = _emailAccountService.GetEmailAccountById(id);
            if(defaultEmailAccount == null)
                return RedirectToAction("List");

            _emailAccountSettings.DefaultEmailAccountId = defaultEmailAccount.Id;
            _settingService.SaveSetting(_emailAccountSettings);

            return RedirectToAction("List");
        }

        public virtual IActionResult Create() {
            if(!_permissionService.Authorize(StandardPermissionProvider.ManageEmailAccounts))
                return AccessDeniedView();

            //prepare model
            var model = _emailAccountModelFactory.PrepareEmailAccountModel(new EmailAccountModel(), null);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult Create(EmailAccountModel model, bool continueEditing) {
            if(!_permissionService.Authorize(StandardPermissionProvider.ManageEmailAccounts))
                return AccessDeniedView();

            if(ModelState.IsValid) {
                var emailAccount = model.ToEntity<EmailAccount>();

                //set password manually
                emailAccount.Password = model.Password;
                _emailAccountService.InsertEmailAccount(emailAccount);

                //activity log
                _customerActivityService.InsertActivity("AddNewEmailAccount",
                    string.Format(_localizationService.GetResource("ActivityLog.AddNewEmailAccount"), emailAccount.Id), emailAccount);

                _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Configuration.EmailAccounts.Added"));

                return continueEditing ? RedirectToAction("Edit", new { id = emailAccount.Id }) : RedirectToAction("List");
            }

            //prepare model
            model = _emailAccountModelFactory.PrepareEmailAccountModel(model, null, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        public virtual IActionResult Edit(int id) {
            if(!_permissionService.Authorize(StandardPermissionProvider.ManageEmailAccounts))
                return AccessDeniedView();

            //try to get an email account with the specified id
            var emailAccount = _emailAccountService.GetEmailAccountById(id);
            if(emailAccount == null)
                return RedirectToAction("List");

            //prepare model
            var model = _emailAccountModelFactory.PrepareEmailAccountModel(null, emailAccount);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public virtual IActionResult Edit(EmailAccountModel model, bool continueEditing) {
            if(!_permissionService.Authorize(StandardPermissionProvider.ManageEmailAccounts))
                return AccessDeniedView();

            //try to get an email account with the specified id
            var emailAccount = _emailAccountService.GetEmailAccountById(model.Id);
            if(emailAccount == null)
                return RedirectToAction("List");

            if(ModelState.IsValid) {
                emailAccount = model.ToEntity(emailAccount);
                _emailAccountService.UpdateEmailAccount(emailAccount);

                //activity log
                _customerActivityService.InsertActivity("EditEmailAccount",
                    string.Format(_localizationService.GetResource("ActivityLog.EditEmailAccount"), emailAccount.Id), emailAccount);

                _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Configuration.EmailAccounts.Updated"));

                return continueEditing ? RedirectToAction("Edit", new { id = emailAccount.Id }) : RedirectToAction("List");
            }

            //prepare model
            model = _emailAccountModelFactory.PrepareEmailAccountModel(model, emailAccount, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("changepassword")]
        public virtual IActionResult ChangePassword(EmailAccountModel model) {
            if(!_permissionService.Authorize(StandardPermissionProvider.ManageEmailAccounts))
                return AccessDeniedView();

            //try to get an email account with the specified id
            var emailAccount = _emailAccountService.GetEmailAccountById(model.Id);
            if(emailAccount == null)
                return RedirectToAction("List");

            //do not validate model
            emailAccount.Password = model.Password;
            _emailAccountService.UpdateEmailAccount(emailAccount);

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Configuration.EmailAccounts.Fields.Password.PasswordChanged"));

            return RedirectToAction("Edit", new { id = emailAccount.Id });
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("sendtestemail")]
        public virtual IActionResult SendTestEmail(EmailAccountModel model) {
            if(!_permissionService.Authorize(StandardPermissionProvider.ManageEmailAccounts))
                return AccessDeniedView();

            //try to get an email account with the specified id
            var emailAccount = _emailAccountService.GetEmailAccountById(model.Id);
            if(emailAccount == null)
                return RedirectToAction("List");

            if(!CommonHelper.IsValidEmail(model.SendTestEmailTo)) {
                _notificationService.ErrorNotification(_localizationService.GetResource("Admin.Common.WrongEmail"));
                return View(model);
            }

            try {
                if(string.IsNullOrWhiteSpace(model.SendTestEmailTo))
                    throw new NopException("Enter test email address");

                var subject = _storeContext.CurrentStore.Name + ". Testing email functionality.";
                var body = "Email works fine.";
                _emailSender.SendEmail(emailAccount, subject, body, emailAccount.Email, emailAccount.DisplayName, model.SendTestEmailTo, null);

                _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Configuration.EmailAccounts.SendTestEmail.Success"));
            } catch(Exception exc) {
                _notificationService.ErrorNotification(exc.Message);
            }

            //prepare model
            model = _emailAccountModelFactory.PrepareEmailAccountModel(model, emailAccount, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public virtual IActionResult Delete(int id) {
            if(!_permissionService.Authorize(StandardPermissionProvider.ManageEmailAccounts))
                return AccessDeniedView();

            //try to get an email account with the specified id
            var emailAccount = _emailAccountService.GetEmailAccountById(id);
            if(emailAccount == null)
                return RedirectToAction("List");

            try {
                _emailAccountService.DeleteEmailAccount(emailAccount);

                //activity log
                _customerActivityService.InsertActivity("DeleteEmailAccount",
                    string.Format(_localizationService.GetResource("ActivityLog.DeleteEmailAccount"), emailAccount.Id), emailAccount);

                _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Configuration.EmailAccounts.Deleted"));

                return RedirectToAction("List");
            } catch(Exception exc) {
                _notificationService.ErrorNotification(exc);
                return RedirectToAction("Edit", new { id = emailAccount.Id });
            }
        }

        #endregion

        #region Email receivers 

        public virtual IActionResult EmailReceiversList() {
            EmailAccountSearchModel model = new EmailAccountSearchModel();
            return View(model);
        }

        [HttpPost]
        public virtual IActionResult EmailReceiversList(EmailAccountSearchModel searchModel) {
            if(!_permissionService.Authorize(StandardPermissionProvider.ManageEmailAccounts))
                return AccessDeniedDataTablesJson();

            //prepare model
            var model = _emailAccountModelFactory.PrepareEmailReceiversListModel(searchModel);

            return Json(model);
        }

        public virtual IActionResult CreateEmailReceiver() {
            if(!_permissionService.Authorize(StandardPermissionProvider.ManageEmailAccounts))
                return AccessDeniedDataTablesJson();

            EmailReceiverModel model = new EmailReceiverModel();
            return View(model);
        }

        [HttpPost]
        public virtual IActionResult CreateEmailReceiver(EmailReceiverModel model) {
            if(ModelState.IsValid) {
                EmailReceivers emailReceivers = new EmailReceivers {
                    Email = model.EmailAddress,
                    Permission = string.Join(",", model.Permissions)
                };
                _emailAccountService.InsertEmailReceiver(emailReceivers);
                _notificationService.SuccessNotification("Email receiver has been added successfully");
                return RedirectToAction("EmailReceiversList");
            }

            return View(model);
        }

        public virtual IActionResult EditEmailReceiver(int id) {
            if(!_permissionService.Authorize(StandardPermissionProvider.ManageEmailAccounts))
                return AccessDeniedDataTablesJson();

            var emailReciever = _emailAccountService.GetEmailReceiver(id);
            if(emailReciever == null)
                return RedirectToAction("EmailReceiversList");

            EmailReceiverModel model = new EmailReceiverModel {
                Id = id,
                EmailAddress = emailReciever.Email,
                Permissions = emailReciever.Permission.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(x => Convert.ToInt32(x)).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult EditEmailReceiver(EmailReceiverModel model) {
            if(ModelState.IsValid) {
                var emailReciever = _emailAccountService.GetEmailReceiver(model.Id);
                if(emailReciever == null)
                    return RedirectToAction("EmailReceiversList");

                emailReciever.Email = model.EmailAddress;
                emailReciever.Permission = string.Join(",", model.Permissions);
                _emailAccountService.UpdateEmailReceiver(emailReciever);

                _notificationService.SuccessNotification("Email receiver has been updated successfully");
                return RedirectToAction("EmailReceiversList");
            }

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult DeleteEmailReceiver(int id) {
            if(!_permissionService.Authorize(StandardPermissionProvider.ManageEmailAccounts))
                return AccessDeniedView();

            //try to get an email account with the specified id
            var emailAccount = _emailAccountService.GetEmailReceiver(id);
            if(emailAccount == null)
                return RedirectToAction("EmailReceiversList");

            try {
                _emailAccountService.DeleteEmailReceiver(emailAccount);

                return RedirectToAction("EmailReceiversList");
            } catch(Exception exc) {
                _notificationService.ErrorNotification(exc);
                return RedirectToAction("EditEmailReceiver", new { id = emailAccount.Id });
            }
        }

        public virtual IActionResult AddLocale() {
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.Configuration.EmailReceivers.AddNew", "Add New Email Receiver");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.Configuration.EmailReceivers.BackToList", "Back to list");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.Configuration.EmailReceivers.Edit", "Edit Email Receiver");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.Configuration.EmailReceivers", "Email Receivers");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.Configuration.EmailAccounts.Fields.Permissions", "Permissions");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.Configuration.EmailAccounts.Fields.Permissions.Hint", "Permissions");
            return Content("Done");
        }

        public virtual IActionResult AddMessageTemplate() {
            var eaGeneral = _emailAccountService.GetAllEmailAccounts().FirstOrDefault();
            if(eaGeneral == null)
                throw new Exception("Default email account cannot be loaded");

            var existingMessageTemplate = _messageTemplateService.GetMessageTemplatesByName(MessageTemplateSystemNames.NewQuestionPostMessage).FirstOrDefault();
            if(existingMessageTemplate == null) {
                var messageTemplate = new MessageTemplate {
                    Name = MessageTemplateSystemNames.NewQuestionPostMessage,
                    Subject = "%Store.Name%. New Product Question.",
                    Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}A new question has been created for product \"%ProductQA.ProductName%\".{Environment.NewLine}{Environment.NewLine}<br />{Environment.NewLine}Question: \"%ProductQA.Question%\"</p>{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id
                };
                _messageTemplateService.InsertMessageTemplate(messageTemplate);
            }
            return Content("Done");
        }

        #endregion
    }
}
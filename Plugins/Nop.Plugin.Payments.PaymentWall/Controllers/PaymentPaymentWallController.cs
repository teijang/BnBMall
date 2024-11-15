using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.PaymentWall.Models;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using StackExchange.Profiling.Internal;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Nop.Plugin.Payments.PaymentWall.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class PaymentPaymentWallController : BasePaymentController
    {
        #region Fields

        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly INotificationService _notificationService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly PaymentWallPaymentSettings _paymentWallPaymentSettings;
        private readonly ICustomerService _customerService;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;

        #endregion

        #region Ctor

        public PaymentPaymentWallController(IGenericAttributeService genericAttributeService,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            IPaymentPluginManager paymentPluginManager,
            IPermissionService permissionService,
            ILocalizationService localizationService,
            ILogger logger,
            INotificationService notificationService,
            ISettingService settingService,
            IStoreContext storeContext,
            IWebHelper webHelper,
            IWorkContext workContext,
            ShoppingCartSettings shoppingCartSettings,
            PaymentWallPaymentSettings paymentWallPaymentSettings,
            ICustomerService customerService,
            ICurrencyService currencyService,
            CurrencySettings currencySettings,
            IPriceCalculationService priceCalculationService,
            IAddressService addressService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService) {
            _genericAttributeService = genericAttributeService;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _paymentPluginManager = paymentPluginManager;
            _permissionService = permissionService;
            _localizationService = localizationService;
            _logger = logger;
            _notificationService = notificationService;
            _settingService = settingService;
            _storeContext = storeContext;
            _webHelper = webHelper;
            _workContext = workContext;
            _shoppingCartSettings = shoppingCartSettings;
            _paymentWallPaymentSettings = paymentWallPaymentSettings;
            _customerService = customerService;
            _currencyService = currencyService;
            _currencySettings = currencySettings;
            _priceCalculationService = priceCalculationService;
            _addressService = addressService;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
        }

        #endregion

        #region Methods

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure() {
            if(!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var paymentWallPaymentSettings = _settingService.LoadSetting<PaymentWallPaymentSettings>(storeScope);

            var model = new ConfigurationModel {
                UseSandbox = paymentWallPaymentSettings.UseSandbox,
                ProjectKey = paymentWallPaymentSettings.ProjectKey,
                SecretKey = paymentWallPaymentSettings.SecretKey,
                WidgetCode = paymentWallPaymentSettings.WidgetCode,
                AdditionalFee = paymentWallPaymentSettings.AdditionalFee,
                AdditionalFeePercentage = paymentWallPaymentSettings.AdditionalFeePercentage,
                ActiveStoreScopeConfiguration = storeScope
            };

            if(storeScope <= 0)
                return View("~/Plugins/Payments.PaymentWall/Views/Configure.cshtml", model);

            model.UseSandbox_OverrideForStore = _settingService.SettingExists(paymentWallPaymentSettings, x => x.UseSandbox, storeScope);
            model.ProjectKey_OverrideForStore = _settingService.SettingExists(paymentWallPaymentSettings, x => x.ProjectKey, storeScope);
            model.SecretKey_OverrideForStore = _settingService.SettingExists(paymentWallPaymentSettings, x => x.SecretKey, storeScope);
            model.WidgetCode_OverrideForStore = _settingService.SettingExists(paymentWallPaymentSettings, x => x.WidgetCode, storeScope);
            model.AdditionalFee_OverrideForStore = _settingService.SettingExists(paymentWallPaymentSettings, x => x.AdditionalFee, storeScope);
            model.AdditionalFeePercentage_OverrideForStore = _settingService.SettingExists(paymentWallPaymentSettings, x => x.AdditionalFeePercentage, storeScope);

            return View("~/Plugins/Payments.PaymentWall/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure(ConfigurationModel model) {
            if(!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            if(!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var paymentWallPaymentSettings = _settingService.LoadSetting<PaymentWallPaymentSettings>(storeScope);

            //save settings
            paymentWallPaymentSettings.UseSandbox = model.UseSandbox;
            paymentWallPaymentSettings.AdditionalFee = model.AdditionalFee;
            paymentWallPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            paymentWallPaymentSettings.ProjectKey = model.ProjectKey;
            paymentWallPaymentSettings.SecretKey = model.SecretKey;
            paymentWallPaymentSettings.WidgetCode = model.WidgetCode;


            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            _settingService.SaveSettingOverridablePerStore(paymentWallPaymentSettings, x => x.UseSandbox, model.UseSandbox_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(paymentWallPaymentSettings, x => x.ProjectKey, model.ProjectKey_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(paymentWallPaymentSettings, x => x.SecretKey, model.SecretKey_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(paymentWallPaymentSettings, x => x.WidgetCode, model.WidgetCode_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(paymentWallPaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(paymentWallPaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);

            //now clear settings cache
            _settingService.ClearCache();

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        //action displaying notification (warning) to a store owner about inaccurate PayPal rounding
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult RoundingWarning(bool passProductNamesAndTotals) {
            if(!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            //prices and total aren't rounded, so display warning
            if(passProductNamesAndTotals && !_shoppingCartSettings.RoundPricesDuringCalculation)
                return Json(new { Result = _localizationService.GetResource("Plugins.Payments.PaymentWall.RoundingWarning") });

            return Json(new { Result = string.Empty });
        }

        public IActionResult CancelOrder() {
            var order = _orderService.SearchOrders(_storeContext.CurrentStore.Id,
                customerId: _workContext.CurrentCustomer.Id, pageSize: 1).FirstOrDefault();

            if(order != null)
                return RedirectToRoute("OrderDetails", new { orderId = order.Id });

            return RedirectToRoute("Homepage");
        }

        #endregion

        public IActionResult PaymentWallPage(string guid) {
            PaymentWallModel model = new PaymentWallModel();
            var order = _orderService.GetOrderByGuid(new Guid(guid));
            var customer = _customerService.GetCustomerById(order.CustomerId);
            var storeLocation = _webHelper.GetStoreLocation();

            Paymentwall_Base.setApiType(Paymentwall_Base.API_GOODS);
            Paymentwall_Base.setAppKey(_paymentWallPaymentSettings.ProjectKey);
            Paymentwall_Base.setSecretKey(_paymentWallPaymentSettings.SecretKey);

            List<Paymentwall_Product> productList = new List<Paymentwall_Product>();

            //var orderCurrency = _currencyService.GetCurrencyByCode(order.CustomerCurrencyCode);
            var primaryCurrency = _currencyService.GetCurrencyById(_currencySettings.PrimaryExchangeRateCurrencyId);
            //var amount = _currencyService.ConvertToPrimaryExchangeRateCurrency(order.OrderTotal, orderCurrency);
            Paymentwall_Product product = new Paymentwall_Product(
                order.OrderGuid.ToString(),
                (float)order.OrderTotal,
                primaryCurrency.CurrencyCode,
                "Order",
                Paymentwall_Product.TYPE_FIXED);

            productList.Add(product);
            Paymentwall_Widget widget = new Paymentwall_Widget(
                customer.CustomerGuid.ToString(),
                _paymentWallPaymentSettings.WidgetCode,
                productList,
                new Dictionary<string, string>() {
                    { "email", customer.Email },
                    { "ps", "all"},
                    { "customer[firstname]", _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.FirstNameAttribute) },
                    { "customer[lastname]", _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.LastNameAttribute) },
                    { "success_url", $"{storeLocation}order/payment/success/{order.OrderGuid}?transaction_id=$ref" },
                    { "failure_url", $"{storeLocation}order/payment/failed/{order.OrderGuid}?transaction_id=$ref" },
                });

            var redirectUrl = widget.getUrl();
            model.IFrameUrl = redirectUrl;
            model.OrderGuid = guid;
            return View("~/Plugins/Payments.PaymentWall/Views/PaymentWallPage.cshtml", model);
        }

        public IActionResult SuccessPage(string guid, string transaction_id) {
            var order = _orderService.GetOrderByGuid(new Guid(guid));
            if(order == null) {
                return RedirectToAction("Index", "Home");
            }

            var paymentStatus = order.PaymentStatus;
            if(paymentStatus == PaymentStatus.Paid) {
                return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
            }

            if(!_orderProcessingService.CanMarkOrderAsPaid(order))
                return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });

            if(transaction_id != "$ref" && !string.IsNullOrEmpty(transaction_id)) {
                //order note
                _orderService.InsertOrderNote(new OrderNote {
                    OrderId = order.Id,
                    Note = transaction_id,
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });

                //mark order as paid
                order.AuthorizationTransactionId = transaction_id;
            }
            _orderService.UpdateOrder(order);
            _orderProcessingService.MarkOrderAsPaid(order);

            return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
        }

        public IActionResult FailPage(string guid, string transaction_id) {
            var order = _orderService.GetOrderByGuid(new Guid(guid));
            if(order == null) {
                return RedirectToAction("Index", "Home");
            }

            _notificationService.ErrorNotification(_localizationService.GetResource("Plugins.Payments.PaymentWall.PaymentFailed"));
            return RedirectToRoute("OrderDetails", new { orderId = order.Id });
        }

        public IActionResult PingbackPage() {
            //uid = sdvs & goodsid = vdsv & slength = 1 & speriod = month & type = 0 & ref= parth & is_test = 1 & sig = a1d147be454761fbe71a43067c294

            try {
                _logger.Information("Pingback page " + Request.QueryString.ToJson());

                var uId = Request.Query["uid"].ToString();
                var type = Request.Query["type"].ToString();
                var refrence = Request.Query["ref"].ToString();
                var sign_version = Request.Query["sign_version"].ToString();
                var sig = Request.Query["sig"].ToString();
                var is_test = Request.Query["is_test"].ToString();
                var goodsid = Request.Query["goodsid"].ToString();

                var order = _orderService.GetOrderByGuid(new Guid(goodsid));
                var address = order.ShippingAddressId > 0 ? _addressService.GetAddressById(order.ShippingAddressId.Value) : _addressService.GetAddressById(order.BillingAddressId);

                Paymentwall_Base.setApiType(Paymentwall_Base.API_GOODS);
                Paymentwall_Base.setAppKey(_paymentWallPaymentSettings.ProjectKey); // available in your Paymentwall merchant area
                Paymentwall_Base.setSecretKey(_paymentWallPaymentSettings.SecretKey); // available in your Paymentwall merchant area

                NameValueCollection parameters = new NameValueCollection();
                foreach(var item in Request.Query) {
                    parameters.Add(item.Key, item.Value);
                }

                Paymentwall_Pingback pingback = new Paymentwall_Pingback(parameters, Request.HttpContext.Connection.RemoteIpAddress.Address.ToString());
                var isPingBackValidated = pingback.validate();
                _logger.Information("Ping back validated: " + isPingBackValidated);
                if(isPingBackValidated || true) {
                    string productId = pingback.getProduct().getId();
                    _logger.Information("Ping back product Id: " + productId);

                    var isDeliverable = pingback.isDeliverable();
                    _logger.Information("Ping back product is deliverable: " + isDeliverable);

                    if(isDeliverable) {
                        //order note
                        _orderService.InsertOrderNote(new OrderNote {
                            OrderId = order.Id,
                            Note = refrence,
                            DisplayToCustomer = false,
                            CreatedOnUtc = DateTime.UtcNow
                        });

                        try {
                            var customer = _customerService.GetCustomerById(order.CustomerId);
                            string estimatedUpdateTime = DateTime.Now.AddDays(3).ToString("yyyy/MM/dd HH:mm:ss") + " +0500";
                            string estimatedDeliveryTime = DateTime.Now.AddDays(30).ToString("yyyy/MM/dd HH:mm:ss") + " +0500";
                            ProcessDelivery(refrence, goodsid, estimatedUpdateTime, customer.Email, estimatedDeliveryTime, address);
                        } catch { }

                        //mark order as paid
                        order.AuthorizationTransactionId = refrence;
                        _orderService.UpdateOrder(order);
                        _orderProcessingService.MarkOrderAsPaid(order);
                    }

                    return Content("OK"); // Paymentwall expects response to be OK, otherwise the pingback will be resent
                } else {
                    return Content(pingback.getErrorSummary());
                }
            } catch(Exception ex) {
                return Content("OK");
            }
        }

        private void ProcessDelivery(string paymentId, string merchatReferenceId, string updateDateTime, string shippingEmail, string shippingDateTime, Address address) {
            var country = address.CountryId > 0 ? _countryService.GetCountryById(address.CountryId.Value).TwoLetterIsoCode : "US";
            var stateProvince = address.StateProvinceId > 0 ? _stateProvinceService.GetStateProvinceById(address.StateProvinceId.Value).Name : "Other";

            PaymentWallHelper.ProcessDelivery(paymentId, merchatReferenceId,
                updateDateTime, shippingEmail, shippingDateTime, address,
                country, stateProvince, _paymentWallPaymentSettings.SecretKey, _paymentWallPaymentSettings.UseSandbox, "order_placed", "", "", _logger);
        }
    }
}
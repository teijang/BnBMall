using Nop.Core.Domain.Shipping;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Events;
using Nop.Services.Logging;
using Nop.Services.Orders;
using System;

namespace Nop.Plugin.Payments.PaymentWall.Services
{
    public class ShipmentDeliveredEventConsumer : IConsumer<ShipmentDeliveredEvent>
    {
        #region Fields

        private readonly IOrderService _orderService;
        private readonly ILogger _logger;
        private readonly PaymentWallPaymentSettings _paymentWallPaymentSettings;
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;

        #endregion

        #region Ctor

        public ShipmentDeliveredEventConsumer(IOrderService orderService,
            ILogger logger,
            PaymentWallPaymentSettings paymentWallPaymentSettings,
            IAddressService addressService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService) {
            _orderService = orderService;
            _logger = logger;
            _paymentWallPaymentSettings = paymentWallPaymentSettings;
            _addressService = addressService;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
        }

        #endregion

        #region Methods

        public void HandleEvent(ShipmentDeliveredEvent eventMessage) {
            try {
                var shipment = eventMessage.Shipment;
                var orderId = shipment.OrderId;
                var order = _orderService.GetOrderById(orderId);
                var address = order.ShippingAddressId > 0 ? _addressService.GetAddressById(order.ShippingAddressId.Value) : _addressService.GetAddressById(order.BillingAddressId);
                var country = address.CountryId > 0 ? _countryService.GetCountryById(address.CountryId.Value).TwoLetterIsoCode : "US";
                var stateProvince = address.StateProvinceId > 0 ? _stateProvinceService.GetStateProvinceById(address.StateProvinceId.Value).Name : "Other";
                var shippingEmail = address.Email;
                var paymentId = order.AuthorizationTransactionId;
                var merchatReferenceId = order.OrderGuid.ToString();
                var updateDateTime = shipment.ShippedDateUtc.HasValue ? shipment.ShippedDateUtc.Value.ToString("yyyy/MM/dd HH:mm:ss") + " +0500" : "";
                var shippingDateTime = shipment.DeliveryDateUtc.HasValue ? shipment.DeliveryDateUtc.Value.ToString("yyyy/MM/dd HH:mm:ss") + " +0500" : "";

                PaymentWallHelper.ProcessDelivery(paymentId, merchatReferenceId,
                    updateDateTime, shippingEmail, shippingDateTime, address,
                    country, stateProvince, _paymentWallPaymentSettings.SecretKey, _paymentWallPaymentSettings.UseSandbox, "delivered", shipment.TrackingNumber, order.ShippingRateComputationMethodSystemName, _logger);
            } catch { }
        }

        #endregion
    }

    public class ShipmentShipedEventConsumer : IConsumer<ShipmentSentEvent>
    {
        #region Fields

        private readonly IOrderService _orderService;
        private readonly ILogger _logger;
        private readonly PaymentWallPaymentSettings _paymentWallPaymentSettings;
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;

        #endregion

        #region Ctor

        public ShipmentShipedEventConsumer(IOrderService orderService,
            ILogger logger,
            PaymentWallPaymentSettings paymentWallPaymentSettings,
            IAddressService addressService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService) {
            _orderService = orderService;
            _logger = logger;
            _paymentWallPaymentSettings = paymentWallPaymentSettings;
            _addressService = addressService;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
        }

        #endregion

        #region Methods

        public void HandleEvent(ShipmentSentEvent eventMessage) {
            try {
                var shipment = eventMessage.Shipment;
                var orderId = shipment.OrderId;
                var order = _orderService.GetOrderById(orderId);
                var address = order.ShippingAddressId > 0 ? _addressService.GetAddressById(order.ShippingAddressId.Value) : _addressService.GetAddressById(order.BillingAddressId);
                var country = address.CountryId > 0 ? _countryService.GetCountryById(address.CountryId.Value).TwoLetterIsoCode : "US";
                var stateProvince = address.StateProvinceId > 0 ? _stateProvinceService.GetStateProvinceById(address.StateProvinceId.Value).Name : "";
                var shippingEmail = address.Email;
                var paymentId = order.AuthorizationTransactionId;
                var merchatReferenceId = order.OrderGuid.ToString();
                var updateDateTime = shipment.ShippedDateUtc.HasValue ? shipment.ShippedDateUtc.Value.ToString("yyyy/MM/dd HH:mm:ss") + " +0500" : "";
                var shippingDateTime = DateTime.Now.AddDays(3).ToString("yyyy/MM/dd HH:mm:ss") + " +0500";

                PaymentWallHelper.ProcessDelivery(paymentId, merchatReferenceId,
                    updateDateTime, shippingEmail, shippingDateTime, address,
                    country, stateProvince, _paymentWallPaymentSettings.SecretKey, _paymentWallPaymentSettings.UseSandbox, "delivering", shipment.TrackingNumber, order.ShippingMethod, _logger);
            } catch { }
        }

        #endregion
    }
}

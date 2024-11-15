using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Pickup.PickupInStore.Domain;
using Nop.Plugin.Pickup.PickupInStore.Services;
using Nop.Services.Affiliates;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Shipping.Pickup;
using Nop.Services.Shipping.Tracking;

namespace Nop.Plugin.Pickup.PickupInStore
{
    public class PickupInStoreProvider : BasePlugin, IPickupPointProvider
    {
        #region Fields

        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly ILocalizationService _localizationService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IStoreContext _storeContext;
        private readonly IStorePickupPointService _storePickupPointService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly IAffiliateService _affiliateService;

        #endregion

        #region Ctor

        public PickupInStoreProvider(IAddressService addressService,
            ICountryService countryService,
            ILocalizationService localizationService,
            IStateProvinceService stateProvinceService,
            IStoreContext storeContext,
            IStorePickupPointService storePickupPointService,
            IWebHelper webHelper,
            IWorkContext workContext,
            IAffiliateService affiliateService) {
            _addressService = addressService;
            _countryService = countryService;
            _localizationService = localizationService;
            _stateProvinceService = stateProvinceService;
            _storeContext = storeContext;
            _storePickupPointService = storePickupPointService;
            _webHelper = webHelper;
            _workContext = workContext;
            _affiliateService = affiliateService;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a shipment tracker
        /// </summary>
        public IShipmentTracker ShipmentTracker => null;

        #endregion

        #region Methods

        /// <summary>
        /// Get pickup points for the address
        /// </summary>
        /// <param name="address">Address</param>
        /// <returns>Represents a response of getting pickup points</returns>
        public GetPickupPointsResponse GetPickupPoints(Address address) {
            var result = new GetPickupPointsResponse();

            var customer = _workContext.CurrentCustomer;
            if(customer.AffiliateId > 0) {
                var affiliate = _affiliateService.GetAffiliateById(customer.AffiliateId);
                var affiliateType = _affiliateService.GetAffiliateTypeById(affiliate.AffiliateTypeId);
                if(affiliateType != null) {
                    if(affiliateType.Name.ToLower() == "hotel") {
                        var addressId = affiliate.AddressId;
                        foreach(var point in _storePickupPointService.GetAllStorePickupPointsByAddressId(addressId)) {
                            var pointAddress = _addressService.GetAddressById(point.AddressId);
                            if(pointAddress == null)
                                continue;

                            result.PickupPoints.Add(new PickupPoint {
                                Id = point.Id.ToString(),
                                Name = point.Name,
                                Description = point.Description,
                                Address = pointAddress.Address1,
                                City = pointAddress.City,
                                County = pointAddress.County,
                                StateAbbreviation = _stateProvinceService.GetStateProvinceByAddress(pointAddress)?.Abbreviation ?? string.Empty,
                                CountryCode = _countryService.GetCountryByAddress(pointAddress)?.TwoLetterIsoCode ?? string.Empty,
                                ZipPostalCode = pointAddress.ZipPostalCode,
                                OpeningHours = point.OpeningHours,
                                PickupFee = point.PickupFee,
                                DisplayOrder = point.DisplayOrder,
                                ProviderSystemName = PluginDescriptor.SystemName,
                                Latitude = point.Latitude,
                                Longitude = point.Longitude,
                                TransitDays = point.TransitDays,
                                IsHotel = point.IsHotel,
                                AddressObject = pointAddress
                            });
                        }

                        if(!result.PickupPoints.Any())
                            result.AddError(_localizationService.GetResource("Plugins.Pickup.PickupInStore.NoPickupPoints"));

                        return result;
                    }
                }
            }

            var pickupPoints = _storePickupPointService.GetAllStorePickupPoints(_storeContext.CurrentStore.Id).Where(x => !x.IsHotel).ToList();
            foreach(var point in pickupPoints) {
                var pointAddress = _addressService.GetAddressById(point.AddressId);
                if(pointAddress == null)
                    continue;

                result.PickupPoints.Add(new PickupPoint {
                    Id = point.Id.ToString(),
                    Name = point.Name,
                    Description = point.Description,
                    Address = pointAddress.Address1,
                    City = pointAddress.City,
                    County = pointAddress.County,
                    StateAbbreviation = _stateProvinceService.GetStateProvinceByAddress(pointAddress)?.Abbreviation ?? string.Empty,
                    CountryCode = _countryService.GetCountryByAddress(pointAddress)?.TwoLetterIsoCode ?? string.Empty,
                    ZipPostalCode = pointAddress.ZipPostalCode,
                    OpeningHours = point.OpeningHours,
                    PickupFee = point.PickupFee,
                    DisplayOrder = point.DisplayOrder,
                    ProviderSystemName = PluginDescriptor.SystemName,
                    Latitude = point.Latitude,
                    Longitude = point.Longitude,
                    TransitDays = point.TransitDays,
                    IsHotel = point.IsHotel,
                    AddressObject = pointAddress
                });
            }

            if(!result.PickupPoints.Any())
                result.AddError(_localizationService.GetResource("Plugins.Pickup.PickupInStore.NoPickupPoints"));

            return result;
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl() {
            return $"{_webHelper.GetStoreLocation()}Admin/PickupInStore/Configure";
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override void Install() {
            //sample pickup point
            var country = _countryService.GetCountryByThreeLetterIsoCode("USA");
            var state = _stateProvinceService.GetStateProvinceByAbbreviation("NY", country?.Id);

            var address = new Address {
                Address1 = "21 West 52nd Street",
                City = "New York",
                CountryId = country?.Id,
                StateProvinceId = state?.Id,
                ZipPostalCode = "10021",
                CreatedOnUtc = DateTime.UtcNow
            };
            _addressService.InsertAddress(address);

            var pickupPoint = new StorePickupPoint {
                Name = "New York store",
                AddressId = address.Id,
                OpeningHours = "10.00 - 19.00",
                PickupFee = 1.99m
            };
            _storePickupPointService.InsertStorePickupPoint(pickupPoint);

            //locales
            _localizationService.AddPluginLocaleResource(new Dictionary<string, string> {
                ["Plugins.Pickup.PickupInStore.AddNew"] = "Add a new pickup point",
                ["Plugins.Pickup.PickupInStore.Fields.Description"] = "Description",
                ["Plugins.Pickup.PickupInStore.Fields.Description.Hint"] = "Specify a description of the pickup point.",
                ["Plugins.Pickup.PickupInStore.Fields.DisplayOrder"] = "Display order",
                ["Plugins.Pickup.PickupInStore.Fields.DisplayOrder.Hint"] = "Specify the pickup point display order.",
                ["Plugins.Pickup.PickupInStore.Fields.Latitude"] = "Latitude",
                ["Plugins.Pickup.PickupInStore.Fields.Latitude.Hint"] = "Specify a latitude (DD.dddddddd°).",
                ["Plugins.Pickup.PickupInStore.Fields.Latitude.InvalidPrecision"] = "Precision should be less then 8",
                ["Plugins.Pickup.PickupInStore.Fields.Latitude.InvalidRange"] = "Latitude should be in range -90 to 90",
                ["Plugins.Pickup.PickupInStore.Fields.Latitude.IsNullWhenLongitudeHasValue"] = "Latitude and Longitude should be specify together",
                ["Plugins.Pickup.PickupInStore.Fields.Longitude"] = "Longitude",
                ["Plugins.Pickup.PickupInStore.Fields.Longitude.Hint"] = "Specify a longitude (DD.dddddddd°).",
                ["Plugins.Pickup.PickupInStore.Fields.Longitude.InvalidPrecision"] = "Precision should be less then 8",
                ["Plugins.Pickup.PickupInStore.Fields.Longitude.InvalidRange"] = "Longitude should be in range -180 to 180",
                ["Plugins.Pickup.PickupInStore.Fields.Longitude.IsNullWhenLatitudeHasValue"] = "Latitude and Longitude should be specify together",
                ["Plugins.Pickup.PickupInStore.Fields.Name"] = "Name",
                ["Plugins.Pickup.PickupInStore.Fields.Name.Hint"] = "Specify a name of the pickup point.",
                ["Plugins.Pickup.PickupInStore.Fields.OpeningHours"] = "Opening hours",
                ["Plugins.Pickup.PickupInStore.Fields.OpeningHours.Hint"] = "Specify opening hours of the pickup point (Monday - Friday: 09:00 - 19:00 for example).",
                ["Plugins.Pickup.PickupInStore.Fields.PickupFee"] = "Pickup fee",
                ["Plugins.Pickup.PickupInStore.Fields.PickupFee.Hint"] = "Specify a fee for the shipping to the pickup point.",
                ["Plugins.Pickup.PickupInStore.Fields.Store"] = "Store",
                ["Plugins.Pickup.PickupInStore.Fields.Store.Hint"] = "A store name for which this pickup point will be available.",
                ["Plugins.Pickup.PickupInStore.Fields.TransitDays"] = "Transit days",
                ["Plugins.Pickup.PickupInStore.Fields.TransitDays.Hint"] = "The number of days of delivery of the goods to pickup point.",
                ["Plugins.Pickup.PickupInStore.NoPickupPoints"] = "No pickup points are available",
                ["Checkout.Storepoint.CheckInDate"] = "Check-In Date",
                ["Checkout.Storepoint.CheckOutDate"] = "Check-Out Date",
                ["Checkout.Storepoint.RoomNumber"] = "Room Number",
                ["Checkout.Storepoint.ReservationName"] = "Reservation Name",
                ["admin.affiliates.editaffiliatetype"] = "Edit Affiliate Type",
                ["Admin.AffiliateTypes.Fields.Name"] = "Name",
                ["Admin.AffiliateTypes.Fields.Name.Hint"] = "Name",
                ["admin.affiliatetypes"] = "Affiliate Types",
                ["admin.affiliates.createaffiliatetype"] = "Create Affiliate Type",
                ["Admin.Affiliates.Fields.AffiliateType"] = "Affiliate Type",
            });

            base.Install();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall() {
            //locales
            _localizationService.DeletePluginLocaleResources("Plugins.Pickup.PickupInStore");
            _localizationService.DeletePluginLocaleResource("Checkout.Storepoint.CheckInDate");
            _localizationService.DeletePluginLocaleResource("Checkout.Storepoint.CheckOutDate");
            _localizationService.DeletePluginLocaleResource("Checkout.Storepoint.RoomNumber");
            _localizationService.DeletePluginLocaleResource("Checkout.Storepoint.ReservationName");

            base.Uninstall();
        }

        #endregion
    }
}
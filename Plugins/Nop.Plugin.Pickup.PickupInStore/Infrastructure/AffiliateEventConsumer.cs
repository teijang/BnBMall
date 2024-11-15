using Nop.Core.Domain.Affiliates;
using Nop.Core.Events;
using Nop.Plugin.Pickup.PickupInStore.Domain;
using Nop.Plugin.Pickup.PickupInStore.Services;
using Nop.Services.Affiliates;
using Nop.Services.Common;
using Nop.Services.Events;

namespace Nop.Plugin.Pickup.PickupInStore.Infrastructure
{
    public class AffiliateEventConsumer : IConsumer<EntityInsertedEvent<Affiliate>>
    {
        private readonly IStorePickupPointService _storePickupPointService;
        private readonly IAffiliateService _affiliateService;
        private readonly IAddressService _addressService;

        public AffiliateEventConsumer(IStorePickupPointService storePickupPointService,
            IAffiliateService affiliateService,
            IAddressService addressService) {
            _storePickupPointService = storePickupPointService;
            _affiliateService = affiliateService;
            _addressService = addressService;
        }

        public void HandleEvent(EntityInsertedEvent<Affiliate> eventMessage) {
            var affiliate = eventMessage.Entity;
            var affiliateType = _affiliateService.GetAffiliateTypeById(affiliate.AffiliateTypeId);
            if(affiliateType != null) {
                if(affiliateType.Name.ToLower() == "hotel") {
                    var address = _addressService.GetAddressById(affiliate.AddressId);
                    var pickupPoint = new StorePickupPoint {
                        AddressId = address.Id,
                        Description = "Hotel Pickup Point",
                        Name = "Hotel",
                        StoreId = 0,
                        IsHotel = true
                    };
                    _storePickupPointService.InsertStorePickupPoint(pickupPoint);
                }
            }
        }
    }
}

using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Models.Checkout
{
    public class CheckoutPickupPointsModel : BaseNopModel
    {
        public CheckoutPickupPointsModel()
        {
            Warnings = new List<string>();
            PickupPoints = new List<CheckoutPickupPointModel>();
        }

        public IList<string> Warnings { get; set; }

        public IList<CheckoutPickupPointModel> PickupPoints { get; set; }
        public bool AllowPickupInStore { get; set; }
        public bool PickupInStore { get; set; }
        public bool PickupInStoreOnly { get; set; }
        public bool DisplayPickupPointsOnMap { get; set; }
        public string GoogleMapsApiKey { get; set; }

        public bool AllowCheckInCheckOutSelection { get; set; }

        [NopResourceDisplayName("Checkout.Storepoint.CheckInDate")]
        public string CheckInDate { get; set; }
        [NopResourceDisplayName("Checkout.Storepoint.CheckOutDate")]
        public string CheckOutDate { get; set; }
        [NopResourceDisplayName("Checkout.Storepoint.RoomNumber")]
        public string RoomNumber { get; set; }
        [NopResourceDisplayName("Checkout.Storepoint.ReservationName")]
        public string ReservationName { get; set; }
    }
}

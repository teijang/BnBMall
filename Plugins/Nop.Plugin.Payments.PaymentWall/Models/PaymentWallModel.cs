using Nop.Web.Framework.Models;

namespace Nop.Plugin.Payments.PaymentWall.Models
{
    public class PaymentWallModel : BaseNopEntityModel
    {
        public string IFrameUrl { get; set; }

        public string OrderGuid { get; set; }
    }
}

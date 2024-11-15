using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.PaymentWall.Components
{
    [ViewComponent(Name = "PaymentPaymentWall")]
    public class PaymentPaymentWallViewComponent : NopViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("~/Plugins/Payments.PaymentWall/Views/PaymentInfo.cshtml");
        }
    }
}

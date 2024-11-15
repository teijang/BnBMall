using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Payments.PaymentWall.Infrastructure
{
    public partial class RouteProvider : IRouteProvider
    {
        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="endpointRouteBuilder">Route builder</param>
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapControllerRoute("Plugin.Payments.PaymentWall.Suceess", "order/payment/success/{guid}",
                 new { controller = "PaymentPaymentWall", action = "SuccessPage" });

            endpointRouteBuilder.MapControllerRoute("Plugin.Payments.PaymentWall.Fail", "order/payment/fail/{guid}",
                 new { controller = "PaymentPaymentWall", action = "FailPage" });

            endpointRouteBuilder.MapControllerRoute("Plugin.Payments.PaymentWall.Pingback", "order/payment/pingback",
                 new { controller = "PaymentPaymentWall", action = "PingbackPage" });

            endpointRouteBuilder.MapControllerRoute("Plugin.Payments.PaymentWall.Pingback", "order/paymentwall/{guid}",
              new { controller = "PaymentPaymentWall", action = "PaymentWallPage" });
        }

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority => -1;
    }
}
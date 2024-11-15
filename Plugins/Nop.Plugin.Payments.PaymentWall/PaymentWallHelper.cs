using Nop.Core.Domain.Common;
using Nop.Core.Domain.Payments;
using Nop.Services.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Nop.Plugin.Payments.PaymentWall
{
    /// <summary>
    /// Represents PayPal helper
    /// </summary>
    public class PaymentWallHelper
    {
        public static void ProcessDelivery(string paymentId, string merchatReferenceId, string updateDateTime, string shippingEmail, string shippingDateTime, Address address,
           string country, string stateProvince, string secretKey, bool isSandbox, string orderStatus, string carrier_tracking_id, string carrier_type, ILogger _logger) {

            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var parameters = $"payment_id={paymentId}" +
                $"&merchant_reference_id={merchatReferenceId}" +
                $"&type=physical" +
                $"&status=" + orderStatus + "" +
                $"&estimated_update_datetime={updateDateTime}" +
                $"&refundable=true" +
                (!string.IsNullOrEmpty(carrier_tracking_id) ? $"&carrier_tracking_id=" + carrier_tracking_id : "") +
                (!string.IsNullOrEmpty(carrier_tracking_id) ? $"&carrier_type=" + carrier_type : "") +
                $"&is_test=" + (isSandbox ? 1 : 0) + 
                $"&shipping_address[email]={shippingEmail}" +
                $"&shipping_address[country]={country}" +
                $"&shipping_address[city]={address.City}" +
                $"&shipping_address[zip]={address.ZipPostalCode}" +
                $"&shipping_address[state]={stateProvince}" +
                $"&shipping_address[street]={address.Address1?.Replace("#","")}" +
                $"&shipping_address[phone]={address.PhoneNumber}" +
                $"&shipping_address[firstname]={address.FirstName}" +
                $"&shipping_address[lastname]={address.LastName}" +
                $"&estimated_delivery_datetime={shippingDateTime}";
            _logger.Information(parameters);

            var url = "https://api.paymentwall.com/api/delivery?" + parameters;
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Add("x-apikey", secretKey);
            var content = new StringContent("", Encoding.UTF8, "application/json");
            var response = client.PostAsync("", content).GetAwaiter().GetResult();
            var res = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            _logger.Information("Delivery confirmation " + res);
        }
    }
}
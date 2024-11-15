using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.PaymentWall
{
    /// <summary>
    /// Represents settings of the PayPal Standard payment plugin
    /// </summary>
    public class PaymentWallPaymentSettings : ISettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether to use sandbox (testing environment)
        /// </summary>
        public bool UseSandbox { get; set; }

        /// <summary>
        /// Gets or sets value of project key
        /// </summary>
        public string ProjectKey { get; set; }

        /// <summary>
        /// Gets or sets value of secret key
        /// </summary>
        public string SecretKey { get; set; }

        /// <summary>
        /// Gets or sets value of widget code
        /// </summary>
        public string WidgetCode { get; set; }      

        /// <summary>
        /// Gets or sets an additional fee
        /// </summary>
        public decimal AdditionalFee { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to "additional fee" is specified as percentage. true - percentage, false - fixed value.
        /// </summary>
        public bool AdditionalFeePercentage { get; set; }
    }
}

namespace Nop.Core.Domain.Affiliates
{
    public class AffiliateType : BaseEntity
    {
        /// <summary>
        /// Get or sets value of name of affiliate type
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets value of is deleted or not
        /// </summary>
        public bool Deleted { get; set; }
    }
}

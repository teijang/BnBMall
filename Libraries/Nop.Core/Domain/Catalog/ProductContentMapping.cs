namespace Nop.Core.Domain.Catalog
{
    public class ProductContentMapping : BaseEntity
    {
        /// <summary>
        /// Gets or sets value of product id
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets value of product content id
        /// </summary>
        public int ProductContentId { get; set; }
    }
}

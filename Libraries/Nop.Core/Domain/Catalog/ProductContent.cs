using Nop.Core.Domain.Localization;
using System;

namespace Nop.Core.Domain.Catalog
{
    public class ProductContent : BaseEntity, ILocalizedEntity
    {
        /// <summary>
        /// Gets or sets value of product content type id
        /// </summary>
        public int ProductContentTypeId { get; set; }

        /// <summary>
        /// Gets or sets value of title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets value of link
        /// </summary>
        public string Link { get; set; }

        /// <summary>
        /// Gets or sets value of thumb picture id
        /// </summary>
        public int ThumbPictureId { get; set; }

        /// <summary>
        /// Gets or sets value of is active or not
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets value of created date
        /// </summary>
        public DateTime CreatedDate { get;set; }
    }
}

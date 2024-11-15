using Nop.Core.Domain.Localization;
using System;

namespace Nop.Core.Domain.Catalog
{
    public class ProductContentType : BaseEntity, ILocalizedEntity
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the created date.
        /// </summary>
        /// <value>
        /// The created date.
        /// </value>
        public DateTime CreatedDate { get; set; }
    }
}

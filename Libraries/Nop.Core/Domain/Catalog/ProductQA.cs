using System;

namespace Nop.Core.Domain.Catalog
{
    public class ProductQA : BaseEntity
    {
        /// <summary>
        /// Get or set value of product id
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Get or set value of Question
        /// </summary>
        public string Question { get; set; }

        /// <summary>
        /// Get or set value of answer
        /// </summary>
        public string Answer { get; set; }

        /// <summary>
        /// Get or set value of asked by customer id
        /// </summary>
        public int AskedBy { get; set; }

        /// <summary>
        /// Get or set value of replied by customer id
        /// </summary>
        public int? RepliedBy { get; set; }

        /// <summary>
        /// Get or set value of created date
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Get or set value of date on which answer added
        /// </summary>
        public DateTime? AnswerDate { get; set; }

        /// <summary>
        /// Get or set value of is approved or not
        /// </summary>
        public bool IsApproved { get; set; }

        /// <summary>
        /// Get or sets value of store id
        /// </summary>
        public int StoreId { get; set; }
    }
}

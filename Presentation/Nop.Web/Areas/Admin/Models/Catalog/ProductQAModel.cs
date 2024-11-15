using System;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Catalog
{
    /// <summary>
    /// Represents a product qa model
    /// </summary>
    public partial class ProductQAModel : BaseNopEntityModel
    {
        #region Ctor

        public ProductQAModel()
        {
            
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Admin.Catalog.ProductQA.Fields.Store")]
        public string StoreName { get; set; }

        [NopResourceDisplayName("Admin.Catalog.ProductQA.Fields.Product")]
        public int ProductId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.ProductQA.Fields.Product")]
        public string ProductName { get; set; }

        [NopResourceDisplayName("Admin.Catalog.ProductQA.Fields.Customer")]
        public int CustomerId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.ProductQA.Fields.Customer")]
        public string CustomerInfo { get; set; }
        
        [NopResourceDisplayName("Admin.Catalog.ProductQA.Fields.Question")]
        public string Question { get; set; }
        
        [NopResourceDisplayName("Admin.Catalog.ProductQA.Fields.Answer")]
        public string Answer { get; set; }

        [NopResourceDisplayName("Admin.Catalog.ProductQA.Fields.IsApproved")]
        public bool IsApproved { get; set; }

        [NopResourceDisplayName("Admin.Catalog.ProductQA.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        //vendor
        public bool IsLoggedInAsVendor { get; set; }

        #endregion
    }
}
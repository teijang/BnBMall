using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

namespace Nop.Web.Models.Catalog
{
    public class ProductQAListModel : BaseNopModel
    {
        public ProductQAListModel() {
            ProductQAList = new List<ProductQAModel>();
        }

        public int CurrentCustomerId { get; set; }

        public int ProductId { get; set; }

        public bool CustomerCanAddQuestion { get; set; }

        [NopResourceDisplayName("ProductQA.Question")]
        public string Question { get; set; }

        public IList<ProductQAModel> ProductQAList { get; set; }
    }

    public class ProductQAModel: BaseNopEntityModel
    {
        public string Question { get; set; }

        public string Answer { get; set; }

        public string AskedBy { get; set; }

        public string RepliedBy { get; set; }

        public string CreatedDate { get; set; }
    }
}

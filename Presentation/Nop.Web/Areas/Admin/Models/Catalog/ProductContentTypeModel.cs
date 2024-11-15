using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Nop.Web.Areas.Admin.Models.Catalog
{
    public class ProductContentTypeModel : BaseNopEntityModel, ILocalizedModel<ProductContentTypeLocalizedModel>
    {
        public ProductContentTypeModel() {
            Locales = new List<ProductContentTypeLocalizedModel>();
        }

        [NopResourceDisplayName("Admin.Product.ContentType.Name")]
        [Required(ErrorMessage = "Please enter name")]
        public string Name { get; set; }

        public IList<ProductContentTypeLocalizedModel> Locales { get; set; }
    }

    public partial class ProductContentTypeLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.Product.ContentType.Name")]
        public string Name { get; set; }

    }

    public class ProductContentTypeListModel : BasePagedListModel<ProductContentTypeModel>
    {

    }

    public class ProductContentTypeSearchModel : BaseSearchModel
    {

    }
}

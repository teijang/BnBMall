using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Affiliates
{
    public class AffiliateTypeModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.AffiliateTypes.Fields.Name")]
        public string Name { get; set; }
    }
}

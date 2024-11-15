using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Nop.Web.Areas.Admin.Models.Catalog
{
    public class ProductContentModel : BaseNopEntityModel, ILocalizedModel<ProductContentLocalizedModel>
    {
        public ProductContentModel() {
            ProductContentTypes = new List<SelectListItem>();
            Locales = new List<ProductContentLocalizedModel>();
        }

        [NopResourceDisplayName("Admin.Product.Content.ContentType")]
        public int ProductContentTypeId { get; set; }

        [NopResourceDisplayName("Admin.Product.Content.ContentType")]
        public string ProductContentTypeName { get; set; }

        [NopResourceDisplayName("Admin.Product.Content.Title")]
        [Required(ErrorMessage = "Please enter title")]
        public string Title { get; set; }

        [NopResourceDisplayName("Admin.Product.Content.Link")]
        public string Link { get; set; }

        [NopResourceDisplayName("Admin.Product.Content.ThumbPictureId")]
        [UIHint("Picture")]
        public int ThumbPictureId { get; set; }

        [NopResourceDisplayName("Admin.Product.Content.IsActive")]
        public bool IsActive { get; set; }

        public IList<ProductContentLocalizedModel> Locales { get; set; }

        public IList<SelectListItem> ProductContentTypes { get; set; }
    }

    public class ProductContentLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.Product.Content.Title")]
        public string Title { get; set; }
    }

    public class ProductContentListModel : BasePagedListModel<ProductContentModel>
    {

    }

    public class ProductContentSearchModel : BaseSearchModel
    {
        public ProductContentSearchModel() {
            ProductContentTypes = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.Product.Content.ContentType")]
        public int ProductContentTypeId { get; set; }

        [NopResourceDisplayName("Admin.Product.Content.Title")]
        public string SearchTitle { get; set; }

        public IList<SelectListItem> ProductContentTypes { get; set; }
    }

    public partial class ProductContentMappingSearchModel : BaseSearchModel
    {
        #region Properties

        public int ProductContentId { get; set; }

        #endregion
    }

    public partial class AddProductContentMappingModel : BaseNopModel
    {
        #region Ctor

        public AddProductContentMappingModel() {
            SelectedProductIds = new List<int>();
        }
        #endregion

        #region Properties

        public int ProductContentId { get; set; }

        public IList<int> SelectedProductIds { get; set; }

        #endregion
    }

    public partial class ProductContentMappingModel : BaseNopEntityModel
    {
        public string ProductName { get; set; }

        public int ProductId { get; set; }
    }

    public class ProductContentMappingListModel : BasePagedListModel<ProductContentMappingModel>
    {

    }
}

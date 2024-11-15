using Nop.Core.Domain.Catalog;
using Nop.Web.Areas.Admin.Models.Catalog;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the product review model factory
    /// </summary>
    public partial interface IProductQAModelFactory
    {
        #region ProductQA

        /// <summary>
        /// Prepare product qa search model
        /// </summary>
        /// <param name="searchModel">Product qa search model</param>
        /// <returns>Product qa search model</returns>
        ProductQASearchModel PrepareProductQASearchModel(ProductQASearchModel searchModel);

        /// <summary>
        /// Prepare paged product review list model
        /// </summary>
        /// <param name="searchModel">Product review search model</param>
        /// <returns>Product review list model</returns>
        ProductQAListModel PrepareProductQAListModel(ProductQASearchModel searchModel);

        /// <summary>
        /// Prepare product qa model
        /// </summary>
        /// <param name="model">Product qa model</param>
        /// <param name="productReview">Product qa</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>Product qa model</returns>
        ProductQAModel PrepareProductQAModel(ProductQAModel model,
            ProductQA productQA, bool excludeProperties = false);

        #endregion
    }
}
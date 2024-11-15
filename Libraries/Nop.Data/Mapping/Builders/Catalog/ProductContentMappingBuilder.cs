using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Catalog;
using Nop.Data.Extensions;

namespace Nop.Data.Mapping.Builders.Catalog
{
    public class ProductContentMappingBuilder : NopEntityBuilder<ProductContentMapping>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table) {
            //table
            //    .WithColumn(NameCompatibilityManager.GetColumnName(typeof(ProductContentMapping), nameof(ProductContentMapping.ProductId)))
            //        .AsInt32().PrimaryKey().ForeignKey<Product>()
            //    .WithColumn(NameCompatibilityManager.GetColumnName(typeof(ProductContentMapping), nameof(ProductContentMapping.ProductContentId)))
            //        .AsInt32().PrimaryKey().ForeignKey<ProductContent>();
        }

        #endregion
    }
}

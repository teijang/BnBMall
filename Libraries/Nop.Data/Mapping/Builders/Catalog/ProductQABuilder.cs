using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Catalog;

namespace Nop.Data.Mapping.Builders.Catalog
{
    public class ProductQABuilder : NopEntityBuilder<ProductQA>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table) {
            table
                .WithColumn(nameof(ProductQA.Question)).AsString(4000).NotNullable()
                .WithColumn(nameof(ProductQA.Answer)).AsString(4000).Nullable()
                .WithColumn(nameof(ProductQA.RepliedBy)).AsInt32().Nullable()
                .WithColumn(nameof(ProductQA.AnswerDate)).AsDateTime().Nullable();
        }

        #endregion
    }
}

using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Affiliates;

namespace Nop.Data.Mapping.Builders.Affiliates
{
    public class AffiliateTypeBuilder : NopEntityBuilder<AffiliateType>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table) {
        }

        #endregion
    }
}

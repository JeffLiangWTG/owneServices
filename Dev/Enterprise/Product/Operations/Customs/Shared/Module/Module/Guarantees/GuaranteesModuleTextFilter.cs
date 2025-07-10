using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Module
{
	public class GuaranteesModuleTextFilter : ModuleTextFilter
	{
		internal GuaranteesModuleTextFilter(ZString description, FilterStripBusinessObject filterBusinessObject)
			: base(description, (comparisonOperator, value) => GetReferenceQuery(comparisonOperator, value))
		{
			this.filterBusinessObject = filterBusinessObject;
		}

		readonly FilterStripBusinessObject filterBusinessObject;

		static ZQuery GetReferenceQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var guaranteeHeaderQuery = new ZDBOnlyQuery(typeof(BaseCusGuaranteeHeader));

			var guaranteeLineReferenceQuery = new ZDBOnlySubQuery(typeof(BaseCusPermitLineTransaction), CusPermitLineTransactionSchema.CPL_CPH_PermitHeader);
			guaranteeLineReferenceQuery.AddToFilter(CusPermitLineTransactionSchema.CPL_Reference, comparisonOperator, value);

			guaranteeHeaderQuery.AddSubQuery(guaranteeLineReferenceQuery, JoinCondition.And);
			return guaranteeHeaderQuery;
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new GuaranteesModuleTextFilterValidation(this, filterBusinessObject);
		}
	}
}

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal.Module
{
	public delegate ZQuery GetAttributeQueryDelegate(SQLComparisonOperator comparisonOperator, ZString attributeName, ZString value);

	public class ZZRefCusCodeListAttributeTextFilter : ModuleTextFilter
	{
		protected ZZRefCusCodeListAttributeTextFilter(ZString description, GetAttributeQueryDelegate queryDelegate) : base(description, queryDelegate)
		{
			SupportsBlankComparisonOperators = false;
		}

		public ZZRefCusCodeListAttributeTextFilter(ZString description, ZString attributeName) : this(description, GetAttributeQuery)
		{
			this.attributeName = attributeName;
		}
		readonly ZString attributeName;

		static ZQuery GetAttributeQuery(SQLComparisonOperator comparisonOperator, ZString attributeName, ZString value)
		{
			return GetAttributeFilter(attributeName, value, comparisonOperator);
		}

		public override bool HasComparisonOperator => true;

		protected override object[] QueryDelegateParameters
		{
			get { return new object[] { SqlComparisonOperator, attributeName, Property }; }
		}
		static ZQuery GetAttributeFilter(ZString name, ZString value, SQLComparisonOperator comparisonOperator = null)
		{
			ZDBOnlyQuery result = null;
			if (!name.IsEmpty && !value.IsEmpty)
			{
				result = new ZDBOnlyQuery(typeof(ZZRefCusCodeListCombined));
				var attrFilter = new ZDBOnlySubQuery(typeof(ZZRefCusCodeListAttributeCombined),
					ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZZD_CodeList,
					comparisonOperator != null && comparisonOperator.IsNegativeSQLOperator());
				attrFilter.AddToFilter(ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZXE_NKName, name);
				attrFilter.AddToFilter(ZZRefCusCodeListAttributeCombinedSchema.ZZE_Value,
					comparisonOperator == null ? SQLComparisonOperator.Equal : comparisonOperator.GetNegatingSQLOperatorIfNotInSubquery(),
					value);
				result.AddSubQuery(attrFilter, JoinCondition.And);
			}
			return result;
		}
	}
}

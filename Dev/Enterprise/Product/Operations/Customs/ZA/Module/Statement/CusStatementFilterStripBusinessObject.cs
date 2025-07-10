using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Module
{
	class CusStatementFilterStripBusinessObject : FilterStripBusinessObject
	{
		public static class Schema
		{
			public const string StatementDate = "Statement Date";
			public const string FAN = "FAN - Financial Account Number";
			public const string LRN = "LRN / PRN Number";
			public const string ChargeType = "Charge Type";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			var fanFilter = result.AddTextFilter(Schema.FAN, GetFANQuery);
			fanFilter.Category = FilterCategories.NumbersAndReferences;
			fanFilter.MaxLength = CusStatementHeaderSchema.B2_AccountNo.MaxLength;
			fanFilter.MultilingualDescription = ResString.GetMultilingualString("ModularFilterCollection|fan", Schema.FAN);

			var lrnFilter = result.AddTextFilter(Schema.LRN, GetLRNQuery);
			lrnFilter.Category = FilterCategories.NumbersAndReferences;
			lrnFilter.MaxLength = CusStatementLineSchema.B3_EntryNum.MaxLength;
			lrnFilter.MultilingualDescription = ResString.GetMultilingualString("ModularFilterCollection|lrn", Schema.LRN);

			var statementDateFilter = result.AddDateFilter(Schema.StatementDate, GetStatementDateQuery);
			statementDateFilter.Category = FilterCategories.Dates;
			statementDateFilter.MultilingualDescription = ResString.GetMultilingualString("ModularFilterCollection|statementDate", Schema.StatementDate);

			var chargeTypeFilter = result.AddTextFilter(Schema.ChargeType, GetChargeTypeQuery, StatementTypeList);
			chargeTypeFilter.Category = FilterCategories.ModesAndTypes;
			chargeTypeFilter.MultilingualDescription = ResString.GetMultilingualString("ModularFilterCollection|chargeType", Schema.ChargeType);

			return result;
		}

		ZQuery GetLRNQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var lineFilter = new ZDBOnlySubQuery(typeof(CusStatementLine), CusStatementLineChargeSchema.B4_B3);
			lineFilter.AddToFilter(CusStatementLineSchema.B3_EntryNum, comparisonOperator, value);

			var query = new ZDBOnlyQuery(typeof(CusStatementLineCharge));
			query.AddSubQuery(lineFilter, JoinCondition.And);

			return query;
		}

		ZQuery GetFANQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var headerFilter = new ZDBOnlySubQuery(typeof(CusStatementHeader), CusStatementLineSchema.B3_B2);
			headerFilter.AddToFilter(CusStatementHeaderSchema.B2_AccountNo, comparisonOperator, value);

			var lineFilter = new ZDBOnlySubQuery(typeof(CusStatementLine), CusStatementLineChargeSchema.B4_B3);
			lineFilter.AddSubQuery(headerFilter, JoinCondition.And);

			var query = new ZDBOnlyQuery(typeof(CusStatementLineCharge));
			query.AddSubQuery(lineFilter, JoinCondition.And);

			return query;
		}

		ZQuery GetChargeTypeQuery(ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(CusStatementLineCharge));

			query.AddToFilter(CusStatementLineChargeSchema.B4_ChargeType, value);

			return query;
		}

		ZQuery GetStatementDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var headerFilter = new ZDBOnlySubQuery(typeof(CusStatementHeader), CusStatementLineSchema.B3_B2);
			AddDateTimeRange(headerFilter, comparisonOperator, JoinCondition.And, CusStatementHeaderSchema.B2_ProcessDate, value1, value2);

			var lineFilter = new ZDBOnlySubQuery(typeof(CusStatementLine), CusStatementLineChargeSchema.B4_B3);
			lineFilter.AddSubQuery(headerFilter, JoinCondition.And);

			var query = new ZDBOnlyQuery(typeof(CusStatementLineCharge));
			query.AddSubQuery(lineFilter, JoinCondition.And);

			return query;
		}

		CodeDescriptionPairList StatementTypeList => Factory.GetCachedValue<CusStatementChargeTypeList>();
	}
}

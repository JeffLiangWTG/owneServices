using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Module
{
	public class USCVisaTariffFilterStripBusinessObject : FilterStripBusinessObject
	{
		public static class Schema
		{
			public const string TariffNo = "Tariff No";
			public const string CountryOfOrigin = "Country of Origin";
			public const string TextileCategoryNumber = "Textile Category Number";
			public const string VisaBeginningDate = "Visa Beginning Date";
			public const string VisaEndingDate = "Visa Ending Date";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			var tariffNo = result.AddTextFilter(Schema.TariffNo, USCVisaTariffSchema.UK_Tariff);
			tariffNo.MaxLength = USCVisaTariffSchema.UK_Tariff.MaxLength;
			var origin = result.AddTextFilter(Schema.CountryOfOrigin, GetCountryOfOriginQuery);
			origin.MaxLength = USCVisaSchema.UO_UC_NKOriginCountry.MaxLength;
			var textile = result.AddTextFilter(Schema.TextileCategoryNumber, GetTextileCategoryNumberQuery);
			textile.MaxLength = USCVisaSchema.UO_TextileCategoryNo.MaxLength;
			result.AddDateFilter(Schema.VisaBeginningDate, GetVisaBeginningDate);
			result.AddDateFilter(Schema.VisaEndingDate, GetVisaEndDate);

			return result;
		}

		ZQuery GetTextileCategoryNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return AddSubQueryToUSCVisa(USCVisaSchema.UO_TextileCategoryNo, comparisonOperator, value);
		}

		ZQuery GetCountryOfOriginQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return AddSubQueryToUSCVisa(USCVisaSchema.UO_UC_NKOriginCountry, comparisonOperator, value);
		}

		ZQuery GetVisaBeginningDate(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return AddSubQueryToUSCVisa(USCVisaSchema.UO_BeginDate, comparisonOperator, date1, date2);
		}

		ZQuery GetVisaEndDate(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return AddSubQueryToUSCVisa(USCVisaSchema.UO_EndDate, comparisonOperator, date1, date2);
		}

		ZQuery AddSubQueryToUSCVisa(SchemaColumn column, SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(USCVisaTariff));

			var subQuery = new ZDBOnlySubQuery(typeof(USCVisa), USCVisaSchema.PK);
			subQuery.AddToFilter(JoinCondition.And, column, comparisonOperator, value);

			result.AddSubQuery(USCVisaTariffSchema.UK_UO, subQuery, JoinCondition.And);
			return result;
		}

		ZQuery AddSubQueryToUSCVisa(SchemaDateTimeColumn column, DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			var result = new ZDBOnlyQuery(typeof(USCVisaTariff));

			var subQuery = new ZDBOnlySubQuery(typeof(USCVisa), USCVisaSchema.PK);
			AddDateRange(subQuery, comparisonOperator, JoinCondition.And, column, date1.Date, date2.Date);

			result.AddSubQuery(USCVisaTariffSchema.UK_UO, subQuery, JoinCondition.And);
			return result;
		}
	}
}

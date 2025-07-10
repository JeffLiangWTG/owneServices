using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Module
{
	public class USCQuotaFilterStripBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			result.AddTextFilter("Tariff/Category/Visa Number", USCQuotaSchema.UT_Code);
			result.AddTextFilter("Country of Origin", USCQuotaSchema.UT_UC_NKOriginCountry);
			result.AddTextFilter("Second Tariff Number", USCQuotaSchema.UT_SecondTariffNo);
			result.AddTextFilter("First Namesake", USCQuotaSchema.UT_FirstNamesake);
			result.AddTextFilter("Second Namesake", USCQuotaSchema.UT_SecondNamesake);

			result.AddDateFilter("Quota Beginning Date", GetQuotaBeginningDate);
			result.AddDateFilter("Quota Ending Date", GetQuotaEndDate);

			return result;
		}

		ZQuery GetQuotaBeginningDate(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			var result = new ZQuery();
			AddDateRange(result, comparisonOperator, JoinCondition.And, USCQuotaSchema.UT_BeginDate, date1.Date, date2.Date);
			return result;
		}

		ZQuery GetQuotaEndDate(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			var result = new ZQuery();
			AddDateRange(result, comparisonOperator, JoinCondition.And, USCQuotaSchema.UT_EndDate, date1.Date, date2.Date);
			return result;
		}
	}
}

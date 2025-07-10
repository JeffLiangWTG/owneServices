using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Module
{
	public class USCVisaFilterStripBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			result.AddTextFilter("Textile Category No", USCVisaSchema.UO_TextileCategoryNo);
			result.AddTextFilter("Country of Origin", USCVisaSchema.UO_UC_NKOriginCountry);
			result.AddDateFilter("Visa Beginning Date", GetVisaBeginningDate);
			result.AddDateFilter("Visa Ending Date", GetVisaEndDate);

			return result;
		}

		ZQuery GetVisaBeginningDate(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			var result = new ZQuery();
			AddDateRange(result, comparisonOperator, JoinCondition.And, USCVisaSchema.UO_BeginDate, date1.Date, date2.Date);
			return result;
		}

		ZQuery GetVisaEndDate(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			var result = new ZQuery();
			AddDateRange(result, comparisonOperator, JoinCondition.And, USCVisaSchema.UO_EndDate, date1.Date, date2.Date);
			return result;
		}
	}
}

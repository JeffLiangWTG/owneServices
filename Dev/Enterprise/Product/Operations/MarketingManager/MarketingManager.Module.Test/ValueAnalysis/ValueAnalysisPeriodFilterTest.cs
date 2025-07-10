using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Module.Testing
{
	[TestedType(typeof(ValueAnalysisFilterStripsHelper.ValueAnalysisPeriodFilter))]
	public class ValueAnalysisPeriodFilterTest : NonPersistentBusinessObjectTestCase
	{
		ValueAnalysisFilterStripsHelperForTest FilterStripsHelper => fFilterStripsHelper ?? (fFilterStripsHelper = new ValueAnalysisFilterStripsHelperForTest(ValueAnalysisModuleHelper.GetProductCode(ModuleIDs.ValueAnalysisForwardingOrg), OrgHeaderSchema.PK.Name, Factory));
		ValueAnalysisFilterStripsHelperForTest fFilterStripsHelper;
		SalesAnalysisPeriodList PeriodList => fPeriodList ?? (fPeriodList = new SalesAnalysisPeriodList());
		SalesAnalysisPeriodList fPeriodList;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ValueAnalysisFilterStripsHelper.ValueAnalysisPeriodFilter(ValueAnalysisFilterStripsHelper.Description.AnalysisPeriod, FilterStripsHelper.GetTradePeriodFilterExposed, PeriodList);
		}

		public class ValueAnalysisFilterStripsHelperForTest : ValueAnalysisFilterStripsHelper
		{
			public ValueAnalysisFilterStripsHelperForTest(string productCode, string moduleContext, BusinessObjectFactory factory) : base(productCode, moduleContext, factory)
			{
			}

			public ZQuery GetTradePeriodFilterExposed(ZString value)
			{
				return GetAnalysisPeriodFilter(value);
			}
		}
	}
}

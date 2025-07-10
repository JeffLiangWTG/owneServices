using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccAlternateGLAccountLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAlternateCharts()
		{
			var creator = new AccountingTestObjectCreator(Factory);
			creator.CreateAlternateChart("MGT");
			creator.CreateAlternateChart("TRR", isGlobal: false);
			var nonCurrentCompanyChart = creator.CreateAlternateChart("TCC", isGlobal: false);
			nonCurrentCompanyChart.AAC_GC_Company = creator.NonCurrentCompany.PK;
			Factory.Save();

			var charts = Factory.New<AccAlternateGLAccount>().Lookups.AlternateCharts;
			charts.Load();
			AssertEquals(2, charts.Count);
			Assert(!charts.Cast<AccAlternateChart>().Any(chart => chart.AAC_GC_Company == nonCurrentCompanyChart.PK));
		}
	}
}

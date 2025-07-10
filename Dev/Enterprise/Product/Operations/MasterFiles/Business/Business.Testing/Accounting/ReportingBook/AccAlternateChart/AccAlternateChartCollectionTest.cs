using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccAlternateChartLookups;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccAlternateChartCollection))]
	sealed class AccAlternateChartCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AccAlternateChartCollection(Factory);
		}

		public void TestRelationshipFilter()
		{
			var nonCurrrentCompanyChart = Creator.CreateAlternateChart("TCC", "Turkey Local Reporting Chart", false, false, BalanceSheetStyleCode.EAL);
			nonCurrrentCompanyChart.AAC_GC_Company = Creator.NonCurrentCompany.PK;
			Factory.Save();

			TestCollection.Load();
			AssertEquals(2, TestCollection.Count);
			Assert(!TestCollection.Cast<AccAlternateChart>().Any(x => x.AAC_GC_Company == nonCurrrentCompanyChart.AAC_GC_Company));
		}

		#region Override

		protected override void SetUp()
		{
			base.SetUp();
			Creator = new AccountingTestObjectCreator(Factory);
			Creator.CreateAlternateChart("TRR", "Turkey Local Reporting Chart", false, true, BalanceSheetStyleCode.EAL);
			Creator.CreateAlternateChart("MGT", "Management Reporting", true, false, BalanceSheetStyleCode.ELA, AccAlternateChartLookups.ReportOrderCode.BTP);
			Factory.Save();
		}

		#endregion

		AccAlternateChartCollection TestCollection
		{
			get { return testCollection ?? (testCollection = new AccAlternateChartCollection(Factory)); }
		}

		AccAlternateChartCollection testCollection;

		AccountingTestObjectCreator Creator;
	}
}

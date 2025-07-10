using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsABCCategory))]
	public class WhsABCCategoryTest : EnterpriseBusinessObjectTestCase
	{
		#region GetABCCategory

		public void TestGetABCCategory_NullFactory()
		{
			AssertExceptionThrown<ArgumentException>(() => WhsABCCategory.GetABCCategory(null, Guid.Empty, Guid.Empty, Guid.Empty));
		}

		[TestDate(2019, 1, 1)]
		public void TestGetABCCategory()
		{
			var product1 = Factory.NewWithValidTestData<OrgSupplierPart>();
			var client1 = Factory.NewWithValidTestData<OrgHeader>();
			var warehouse1 = Factory.NewWithValidTestData<WhsWarehouse>();

			var product2 = Factory.NewWithValidTestData<OrgSupplierPart>();
			var client2 = Factory.NewWithValidTestData<OrgHeader>();
			var warehouse2 = Factory.NewWithValidTestData<WhsWarehouse>();

			var abcCategory1 = Factory.New<WhsABCCategory>();
			abcCategory1.WJ_OP_Product = product1.PK;
			abcCategory1.WJ_OH_Client = client1.PK;
			abcCategory1.WJ_WW_Warehouse = warehouse1.PK;
			abcCategory1.WJ_Category = "XXX";
			abcCategory1.WJ_AnalysisDateTo = new ZDateTimeOffset(2019, 1, 1);

			var abcCategory2 = Factory.New<WhsABCCategory>();
			abcCategory2.WJ_OP_Product = product2.PK;
			abcCategory2.WJ_OH_Client = client2.PK;
			abcCategory2.WJ_WW_Warehouse = warehouse2.PK;
			abcCategory2.WJ_Category = "XXX";
			abcCategory2.WJ_AnalysisDateTo = new ZDateTimeOffset(2019, 1, 1);

			AssertNull("Precondition: Should not find an ABC Category.", WhsABCCategory.GetABCCategory(Factory, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty));
			AssertNull("Precondition: Should not find an ABC Category.", WhsABCCategory.GetABCCategory(Factory, product2.PK, client1.PK, warehouse1.PK));
			AssertNull("Precondition: Should not find an ABC Category.", WhsABCCategory.GetABCCategory(Factory, product1.PK, client2.PK, warehouse1.PK));
			AssertNull("Precondition: Should not find an ABC Category.", WhsABCCategory.GetABCCategory(Factory, product1.PK, client1.PK, warehouse2.PK));
			AssertEquals("Should find an ABC Category.", abcCategory1, WhsABCCategory.GetABCCategory(Factory, product1.PK, client1.PK, warehouse1.PK));
			AssertEquals("Should find an ABC Category.", abcCategory2, WhsABCCategory.GetABCCategory(Factory, product2.PK, client2.PK, warehouse2.PK));
		}

		[TestDate(2019, 1, 1)]
		public void TestGetABCCategory_MultipleInDifferentTimePeriods()
		{
			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();

			var abcCategory2018 = Factory.New<WhsABCCategory>();
			abcCategory2018.WJ_OP_Product = product.PK;
			abcCategory2018.WJ_OH_Client = client.PK;
			abcCategory2018.WJ_WW_Warehouse = warehouse.PK;
			abcCategory2018.WJ_Category = "XXX";
			abcCategory2018.WJ_AnalysisDateTo = new ZDateTimeOffset(2018, 1, 1);

			var abcCategory2019 = Factory.New<WhsABCCategory>();
			abcCategory2019.WJ_OP_Product = product.PK;
			abcCategory2019.WJ_OH_Client = client.PK;
			abcCategory2019.WJ_WW_Warehouse = warehouse.PK;
			abcCategory2019.WJ_Category = "XXX";
			abcCategory2019.WJ_AnalysisDateTo = new ZDateTimeOffset(2019, 1, 1);
			AssertEquals("Should find the later ABC Category.", abcCategory2019, WhsABCCategory.GetABCCategory(Factory, product.PK, client.PK, warehouse.PK));
		}

		#endregion

		#region TestABCAnalysisPeriodAsString

		[TestDate(2012, 1, 1)]
		public void TestABCAnalysisPeriodAsString()
		{
			var abcCategory = Factory.New<WhsABCCategory>();
			AssertEquals("", abcCategory.ABCAnalysisPeriodAsString);

			abcCategory = Factory.New<WhsABCCategory>();
			abcCategory.WJ_AnalysisDateTo = new ZDateTimeOffset(2012, 2, 1);
			abcCategory.WJ_AnalysisPeriod = "WKY";
			AssertEquals("25-Jan-12 to 31-Jan-12", abcCategory.ABCAnalysisPeriodAsString);

			abcCategory = Factory.New<WhsABCCategory>();
			abcCategory.WJ_AnalysisDateTo = new ZDateTimeOffset(2012, 2, 1);
			abcCategory.WJ_AnalysisPeriod = "LCW";
			AssertEquals("22-Jan-12 to 28-Jan-12", abcCategory.ABCAnalysisPeriodAsString);

			abcCategory = Factory.New<WhsABCCategory>();
			abcCategory.WJ_AnalysisDateTo = new ZDateTimeOffset(2012, 2, 1);
			abcCategory.WJ_AnalysisPeriod = "MLY";
			AssertEquals("02-Jan-12 to 31-Jan-12", abcCategory.ABCAnalysisPeriodAsString);

			abcCategory = Factory.New<WhsABCCategory>();
			abcCategory.WJ_AnalysisDateTo = new ZDateTimeOffset(2012, 2, 1);
			abcCategory.WJ_AnalysisPeriod = "LCM";
			AssertEquals("01-Jan-12 to 31-Jan-12", abcCategory.ABCAnalysisPeriodAsString);

			abcCategory = Factory.New<WhsABCCategory>();
			abcCategory.WJ_AnalysisDateTo = new ZDateTimeOffset(2012, 2, 1);
			abcCategory.WJ_AnalysisPeriod = "QLY";
			AssertEquals("03-Nov-11 to 31-Jan-12", abcCategory.ABCAnalysisPeriodAsString);

			abcCategory = Factory.New<WhsABCCategory>();
			abcCategory.WJ_AnalysisDateTo = new ZDateTimeOffset(2012, 2, 1);
			abcCategory.WJ_AnalysisPeriod = "LCQ";
			AssertEquals("01-Oct-11 to 31-Dec-11", abcCategory.ABCAnalysisPeriodAsString);

			abcCategory = Factory.New<WhsABCCategory>();
			abcCategory.WJ_AnalysisDateTo = new ZDateTimeOffset(2012, 2, 1);
			abcCategory.WJ_AnalysisPeriod = "BAN";
			AssertEquals("03-Aug-11 to 31-Jan-12", abcCategory.ABCAnalysisPeriodAsString);

			abcCategory = Factory.New<WhsABCCategory>();
			abcCategory.WJ_AnalysisDateTo = new ZDateTimeOffset(2012, 2, 1);
			abcCategory.WJ_AnalysisPeriod = "LCB";
			AssertEquals("01-Jul-11 to 31-Dec-11", abcCategory.ABCAnalysisPeriodAsString);

			abcCategory = Factory.New<WhsABCCategory>();
			abcCategory.WJ_AnalysisDateTo = new ZDateTimeOffset(2012, 2, 1);
			abcCategory.WJ_AnalysisPeriod = "ALY";
			AssertEquals("01-Feb-11 to 31-Jan-12", abcCategory.ABCAnalysisPeriodAsString);

			abcCategory = Factory.New<WhsABCCategory>();
			abcCategory.WJ_AnalysisDateTo = new ZDateTimeOffset(2012, 2, 1);
			abcCategory.WJ_AnalysisPeriod = "LCA";
			AssertEquals("01-Jan-11 to 31-Dec-11", abcCategory.ABCAnalysisPeriodAsString);
		}

		#endregion

		#region TestWarehouse

		public void TestWarehouse()
		{
			var warehouse = Factory.New<WhsWarehouse>();
			var abcCategory = Factory.New<WhsABCCategory>();
			abcCategory.WJ_WW_Warehouse = warehouse.PK;

			AssertNotNull(abcCategory.Warehouse);
			AssertEquals(warehouse, abcCategory.Warehouse);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var abcCategory = (WhsABCCategory)base.GetNewBusinessObjectForDeleteTest(factory);
			abcCategory.WJ_AnalysisDateFrom = new ZDateTimeOffset(2011, 1, 1);
			abcCategory.WJ_AnalysisDateTo = new ZDateTimeOffset(2011, 1, 2);
			return abcCategory;
		}

		#endregion
	}
}

using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsDocketTypeDeciderTestCase : WhsTestCaseWithFactory
	{
		public void TestLoadFromPK()
		{
			var docket1 = Factory.New<WhsReceive>();
			var docket2 = Factory.New<WhsOrder>();
			var docket3 = Factory.New<WhsTransfer>();
			var docket4 = Factory.New<WhsAdjustment>();
			var docket5 = Factory.New<WhsWorkOrder>();
			var docket6 = Factory.New<WhsDynamicWorkOrder>();

			Assert("Should have loaded Receive Docket", Factory.Load(typeof(WhsDocket), docket1.PK) is WhsReceive);
			Assert("Should have loaded Order Docket", Factory.Load(typeof(WhsDocket), docket2.PK) is WhsOrder);
			Assert("Should have loaded Transfer Docket", Factory.Load(typeof(WhsDocket), docket3.PK) is WhsTransfer);
			Assert("Should have loaded Adjustment Docket", Factory.Load(typeof(WhsDocket), docket4.PK) is WhsAdjustment);
			Assert("Should have loaded WorkOrder Docket", Factory.Load(typeof(WhsDocket), docket5.PK) is WhsWorkOrder);
			Assert("Should have loaded WhsDynamicWorkOrder Docket", Factory.Load(typeof(WhsDocket), docket6.PK) is WhsDynamicWorkOrder);
		}

		public void TestLoadFromDocketID()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var whs = helper.CreateWarehouse("1", "A");
			var org = helper.CreateClient();

			var docket1 = helper.CreateWhsReceive(org.PK, whs.PK);
			var docket2 = helper.CreateWhsOrder(org.PK, whs.PK);
			var docket3 = helper.CreateWhsTransfer(org.PK, whs.PK);
			var docket4 = helper.CreateWhsAdjustment(org.PK, whs.PK);
			var docket5 = helper.CreateWhsWorkOrder(org.PK, whs.PK);
			var docket6 = Factory.New<WhsDynamicWorkOrder>();
			docket6.WD_OH_Client = org.PK;
			docket6.WD_WW_Whs = whs.PK;

			// need to save factory so docketID's are generated
			Factory.Save();

			var filter1 = new ZQuery(WhsDocketSchema.WD_DocketID, docket1.WD_DocketID);
			Assert("Should have loaded Receive Docket", Factory.LoadTop1(typeof(WhsDocket), filter1) is WhsReceive);

			var filter2 = new ZQuery(WhsDocketSchema.WD_DocketID, docket2.WD_DocketID);
			Assert("Should have loaded Order Docket", Factory.LoadTop1(typeof(WhsDocket), filter2) is WhsOrder);

			var filter3 = new ZQuery(WhsDocketSchema.WD_DocketID, docket3.WD_DocketID);
			Assert("Should have loaded Transfer Docket", Factory.LoadTop1(typeof(WhsDocket), filter3) is WhsTransfer);

			var filter4 = new ZQuery(WhsDocketSchema.WD_DocketID, docket4.WD_DocketID);
			Assert("Should have loaded Adjustment Docket", Factory.LoadTop1(typeof(WhsDocket), filter4) is WhsAdjustment);

			var filter5 = new ZQuery(WhsDocketSchema.WD_DocketID, docket5.WD_DocketID);
			Assert("Should have loaded WorkOrder Docket", Factory.LoadTop1(typeof(WhsDocket), filter5) is WhsWorkOrder);

			var filter6 = new ZQuery(WhsDocketSchema.WD_DocketID, docket6.WD_DocketID);
			Assert("Should have loaded WorkOrder Docket", Factory.LoadTop1(typeof(WhsDocket), filter6) is WhsDynamicWorkOrder);
		}

		public void TestGetDocketTypeCodeFromType()
		{
			var typeDecider = new WhsDocketTypeDecider();
			AssertEquals(DocketType.Codes.Receive, typeDecider.GetDocketTypeCodeFromType(typeof(WhsReceive)));
			AssertEquals(DocketType.Codes.Order, typeDecider.GetDocketTypeCodeFromType(typeof(WhsOrder)));
			AssertEquals(DocketType.Codes.Transfer, typeDecider.GetDocketTypeCodeFromType(typeof(WhsTransfer)));
			AssertEquals(DocketType.Codes.Adjustment, typeDecider.GetDocketTypeCodeFromType(typeof(WhsAdjustment)));
			AssertEquals(DocketType.Codes.WorkOrder, typeDecider.GetDocketTypeCodeFromType(typeof(WhsWorkOrder)));
			AssertEquals(DocketType.Codes.DynamicWorkOrder, typeDecider.GetDocketTypeCodeFromType(typeof(WhsDynamicWorkOrder)));
		}
	}
}

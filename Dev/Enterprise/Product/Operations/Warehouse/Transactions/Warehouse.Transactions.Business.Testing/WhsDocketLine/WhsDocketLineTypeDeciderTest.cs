using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsDocketLineTypeDeciderTest : WhsTestCaseWithFactory
	{
		public void TestLoad_FromPK()
		{
			var docket1 = Factory.New<WhsReceive>();
			var line1 = Factory.New<WhsReceiveLine>();
			line1.WE_WD = docket1.PK;

			var docket2 = Factory.New<WhsOrder>();
			var line2 = Factory.New<WhsOrderLine>();
			line2.WE_WD = docket2.PK;

			var docket3 = Factory.New<WhsTransfer>();
			var line3 = Factory.New<WhsTransferLine>();
			line3.WE_WD = docket3.PK;

			var docket4 = Factory.New<WhsAdjustment>();
			var line4 = Factory.New<WhsAdjustmentLine>();
			line4.WE_WD = docket4.PK;

			var docket5 = Factory.New<WhsWorkOrder>();
			var line5 = Factory.New<WhsWorkOrderLine>();
			line5.WE_WD = docket5.PK;

			var docket6 = Factory.New<WhsDynamicWorkOrder>();
			var line6 = Factory.New<WhsDynamicWorkOrderLine>();
			line6.WE_WD = docket6.PK;

			Assert("Should have loaded Receive Line", Factory.Load(typeof(WhsDocketLine), line1.PK) is WhsReceiveLine);
			Assert("Should have loaded Order Line", Factory.Load(typeof(WhsDocketLine), line2.PK) is WhsOrderLine);
			Assert("Should have loaded Transfer Line", Factory.Load(typeof(WhsDocketLine), line3.PK) is WhsTransferLine);
			Assert("Should have loaded Adjustment Line", Factory.Load(typeof(WhsDocketLine), line4.PK) is WhsAdjustmentLine);
			Assert("Should have loaded Work Order Line", Factory.Load(typeof(WhsDocketLine), line5.PK) is WhsWorkOrderLine);
			Assert("Should have loaded Dynamic Work Order Line", Factory.Load(typeof(WhsDocketLine), line6.PK) is WhsDynamicWorkOrderLine);
		}

		public void TestLoad_NoParentDocket()
		{
			TestLoad_NoParentDocketCore<WhsReceiveLine>();
			TestLoad_NoParentDocketCore<WhsOrderLine>();
			TestLoad_NoParentDocketCore<WhsWorkOrderLine>();
			TestLoad_NoParentDocketCore<WhsTransferLine>();
			TestLoad_NoParentDocketCore<WhsAdjustmentLine>();
			TestLoad_NoParentDocketCore<WhsDynamicWorkOrderLine>();
		}

		void TestLoad_NoParentDocketCore<T>() where T : WhsDocketLine
		{
			var line = Factory.New<T>();
			AssertNoExceptionThrown(() =>
			{
				var newLine = Factory.Load(typeof(WhsDocketLine), line.PK);
				AssertEquals(typeof(T), newLine.GetType());
			});
		}

		public void TestLoad_NullParentDocket()
		{
			TestLoad_NullParentDocketCore<WhsReceiveLine>();
			TestLoad_NullParentDocketCore<WhsOrderLine>();
			TestLoad_NullParentDocketCore<WhsWorkOrderLine>();
			TestLoad_NullParentDocketCore<WhsTransferLine>();
			TestLoad_NullParentDocketCore<WhsAdjustmentLine>();
			TestLoad_NullParentDocketCore<WhsDynamicWorkOrderLine>();
		}

		void TestLoad_NullParentDocketCore<T>() where T : WhsDocketLine
		{
			var line = Factory.New<T>();
			AssertNoExceptionThrown(() =>
			{
				var row = ((IBusinessObjectInternals)line).Row;
				row[WhsDocketLineSchema.Constants.WE_WD] = DBNull.Value;

				var newLine = Factory.Load(typeof(WhsDocketLine), line.PK);
				AssertEquals(typeof(T), newLine.GetType());
			});
		}

		public void TestLoad_DocketLineWithErrorType()
		{
			var line = Factory.New<WhsReceiveLine>();
			line.WE_DocketLineType = "XXX";
			AssertExceptionThrown(typeof(NoConcreteTypeException), () => Factory.Load<WhsDocketLine>(line.PK));
		}

		public void TestLoad_FromDocketID()
		{
			var whs = Helper.CreateWarehouse("1", "A");
			var org = Helper.CreateClient();
			var part = Helper.CreateProduct(org, "P01");
			Factory.Save();

			var receive = Helper.CreateWhsReceive(org, whs, "R1", Notify);
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, part, 5m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var docket2 = Helper.CreateWhsOrder(org, whs);
			var line2 = Helper.CreateWhsOrderLine(docket2, part, 5m);

			var transfer = Helper.CreateWhsTransfer(org, whs);
			var transferLine = Helper.CreateWhsTransferLine(transfer, part, 5m, whs.DefaultLocation, whs.DefaultLocation);
			transferLine.RunPreSaveValidation(); // to commit inventory

			var docket4 = Helper.CreateWhsAdjustment(org, whs);
			var line4 = Helper.CreateWhsAdjustmentLine(docket4, part, 5m, whs.DefaultLocation);

			var docket5 = Helper.CreateWhsWorkOrder(org, whs);
			var line5 = Helper.CreateWhsWorkOrderLine(docket5, part, 5m);

			var dynamicWorkOrder = Factory.New<WhsDynamicWorkOrder>();
			dynamicWorkOrder.WD_OH_Client = org.PK;
			dynamicWorkOrder.WD_WW_Whs = whs.PK;
			var dynamicWorkOrderLine = dynamicWorkOrder.Lines.AddNew();
			dynamicWorkOrderLine.WE_OP = part.PK;
			dynamicWorkOrderLine.WE_TransactionQuantity = 2m;

			// need to save so docketID's are generated
			Factory.Save();

			var filter1 = new ZQuery(WhsDocketLineSchema.PK, receiveLine.InDocketLine.PK);
			Assert("Should have loaded Receive Line", Factory.LoadTop1(typeof(WhsDocketLine), filter1) is WhsReceiveLine);

			var filter2 = new ZQuery(WhsDocketLineSchema.PK, line2.PK);
			Assert("Should have loaded Order Line", Factory.LoadTop1(typeof(WhsDocketLine), filter2) is WhsOrderLine);

			var filter3 = new ZQuery(WhsDocketLineSchema.PK, transferLine.PK);
			Assert("Should have loaded Transfer Line", Factory.LoadTop1(typeof(WhsDocketLine), filter3) is WhsTransferLine);

			var filter4 = new ZQuery(WhsDocketLineSchema.PK, line4.PK);
			Assert("Should have loaded Adjustment Line", Factory.LoadTop1(typeof(WhsDocketLine), filter4) is WhsAdjustmentLine);

			var filter5 = new ZQuery(WhsDocketLineSchema.PK, line5.PK);
			Assert("Should have loaded Work Order Line", Factory.LoadTop1(typeof(WhsDocketLine), filter5) is WhsWorkOrderLine);

			var filter6 = new ZQuery(WhsDocketLineSchema.PK, dynamicWorkOrderLine.PK);
			Assert("Should have loaded Dynamic Work Order Line", Factory.LoadTop1(typeof(WhsDocketLine), filter6) is WhsDynamicWorkOrderLine);
		}
	}
}

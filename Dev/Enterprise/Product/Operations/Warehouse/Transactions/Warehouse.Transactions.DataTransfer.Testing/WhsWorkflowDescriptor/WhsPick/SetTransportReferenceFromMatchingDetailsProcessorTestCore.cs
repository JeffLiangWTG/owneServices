using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class SetTransportReferenceFromMatchingDetailsProcessorTestCore
	{
		#region TestProcess

		public static void TestProcess(BusinessObjectFactory factory, WhsTestHelperFunctions helper)
		{
			var data = new TestDataSimpleEnvironment(factory);
			var transportCo = factory.NewWithValidTestData<OrgHeader>();
			var stmNums = factory.NewWithValidTestData<OrganisationViewStmNums>();
			stmNums.SN_Type = OrgConstants.NumberFountains.Code.TransportReferenceNumbers;
			stmNums.SN_Prefix = "ABC";
			stmNums.SN_MaximumValue = 99999999;
			stmNums.SN_Owner = transportCo.PK;
			var matchingDetails = factory.NewWithValidTestData<StmNumberRangeMatchingDetail>();

			Assertion.AssertEquals("Precondition", 3, stmNums.SN_Type.Length);
			matchingDetails.NRM_OwnerTableCode = OrgHeaderSchema.Constants.Prefix;

			matchingDetails.NRM_RangeType = stmNums.SN_Type.Substring(0, 3);
			matchingDetails.NRM_Prefix = stmNums.SN_Prefix.Substring(0, 3);
			matchingDetails.NRM_OwnerId = stmNums.SN_Owner;
			factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			Assertion.AssertEquals("Precondition", true, string.IsNullOrEmpty(order.WD_TransportReference));

			var processor = new GenerateTransportReferenceNumberProcessor(order);
			Assertion.AssertEquals("Transport company is not valid - should not have error.", "", order.WD_TransportReference);

			order.TransportCoPK = transportCo.PK;
			processor.Process(null);
			Assertion.AssertEquals("Transport reference should get value from matching number fountain.", "ABC00000001", order.WD_TransportReference);
			AssertTransportReferenceHasBeenSaved(order.PK, "ABC00000001");

			processor.Process(null);
			Assertion.AssertEquals("Should not change if already has a value.", "ABC00000001", order.WD_TransportReference);

			order.WD_TransportReference = string.Empty;
			processor.Process(null);
			Assertion.AssertEquals("Should get new value if is empty.", "ABC00000002", order.WD_TransportReference);
			AssertTransportReferenceHasBeenSaved(order.PK, "ABC00000002");

			order.WD_TransportReference = string.Empty;
			order.WD_OH_Client = ZGuid.Empty;
			processor.Process(null);
			Assertion.AssertEquals("Should not set transport reference.", "", order.WD_TransportReference);

			order.WD_OH_Client = data.Org1.PK;
			order.WD_WW_Whs = ZGuid.Empty;
			processor.Process(null);
			Assertion.AssertEquals("Should not set transport reference.", "", order.WD_TransportReference);
		}

		#endregion

		#region TestProcessRanking

		public static void TestProcessRanking(BusinessObjectFactory factory, WhsTestHelperFunctions helper)
		{
			var data = new TestDataSimpleEnvironment(factory);
			var transportCo = factory.NewWithValidTestData<OrgHeader>();
			var client = helper.CreateClient("Client");
			var stmNums1 = factory.NewWithValidTestData<OrganisationViewStmNums>();
			stmNums1.SN_Type = OrgConstants.NumberFountains.Code.TransportReferenceNumbers;
			stmNums1.SN_Prefix = "ABC";
			stmNums1.SN_MaximumValue = 99999999;
			stmNums1.SN_Owner = transportCo.PK;
			var matchingDetails1 = factory.NewWithValidTestData<StmNumberRangeMatchingDetail>();

			Assertion.AssertEquals("Precondition", 3, stmNums1.SN_Type.Length);
			matchingDetails1.NRM_OwnerTableCode = OrgHeaderSchema.Constants.Prefix;

			matchingDetails1.NRM_RangeType = stmNums1.SN_Type.Substring(0, 3);
			matchingDetails1.NRM_Prefix = stmNums1.SN_Prefix.Substring(0, 3);
			matchingDetails1.NRM_OwnerId = stmNums1.SN_Owner;

			var stmNums2 = factory.NewWithValidTestData<OrganisationViewStmNums>();
			stmNums2.SN_Type = OrgConstants.NumberFountains.Code.TransportReferenceNumbers;
			stmNums2.SN_MaximumValue = 99999999;
			stmNums2.SN_Prefix = "DEF";
			stmNums2.SN_Owner = transportCo.PK;
			var matchingDetails2 = factory.NewWithValidTestData<StmNumberRangeMatchingDetail>();
			Assertion.AssertEquals("Precondition", 3, stmNums2.SN_Type.Length);
			Assertion.AssertEquals("Precondition", 3, stmNums2.SN_Prefix.Length);
			matchingDetails2.NRM_OwnerTableCode = OrgHeaderSchema.Constants.Prefix;
			matchingDetails2.NRM_RangeType = stmNums2.SN_Type.Substring(0, 3);
			matchingDetails2.NRM_Prefix = stmNums2.SN_Prefix.Substring(0, 3);
			matchingDetails2.NRM_OwnerId = stmNums2.SN_Owner;
			matchingDetails2.NRM_OH_Client = client.PK;
			factory.Save();

			var order1 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order2 = helper.CreateWhsOrderWithOrderLine(client, data.Whs1, "O1", data.Part1, 10m);
			order1.TransportCoPK = transportCo.PK;
			order2.TransportCoPK = transportCo.PK;
			Assertion.AssertEquals("Precondition", true, string.IsNullOrEmpty(order1.WD_TransportReference));
			Assertion.AssertEquals("Precondition", true, string.IsNullOrEmpty(order2.WD_TransportReference));

			var processor1 = new GenerateTransportReferenceNumberProcessor(order1);
			processor1.Process(null);
			Assertion.AssertEquals("Transport reference should get value from matching number fountain.", "ABC00000001", order1.WD_TransportReference);

			var processor2 = new GenerateTransportReferenceNumberProcessor(order2);
			processor2.Process(null);
			Assertion.AssertEquals("Transport reference should get value from matching number fountain.", "DEF00000001", order2.WD_TransportReference);
		}

		#endregion

		#region AssertTransportReferenceHasBeenSaved

		static void AssertTransportReferenceHasBeenSaved(ZGuid orderPK, string expectedTransportReference)
		{
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var orderInNewFactory = newFactory.Load<WhsOrder>(orderPK);
			Assertion.AssertEquals("Should have saved the changed Transport Reference.", expectedTransportReference, orderInNewFactory.WD_TransportReference);
		}

		#endregion

		#region TestProcess_Error_InvalidOrder

		public static void TestProcess_Error_InvalidOrder(BusinessObjectFactory factory, WhsTestHelperFunctions helper)
		{
			var data = new TestDataSimpleEnvironment(factory);
			var transportCo = factory.NewWithValidTestData<OrgHeader>();
			var stmNums = factory.NewWithValidTestData<OrganisationViewStmNums>();
			stmNums.SN_Type = OrgConstants.NumberFountains.Code.TransportReferenceNumbers;
			stmNums.SN_Prefix = "ABC";
			stmNums.SN_Owner = transportCo.PK;
			var matchingDetails = factory.NewWithValidTestData<StmNumberRangeMatchingDetail>();
			Assertion.AssertEquals("Precondition", 3, stmNums.SN_Type.Length);
			Assertion.AssertEquals("Precondition", 3, stmNums.SN_Prefix.Length);
			matchingDetails.NRM_OwnerTableCode = OrgHeaderSchema.Constants.Prefix;
			matchingDetails.NRM_RangeType = stmNums.SN_Type.Substring(0, 3);
			matchingDetails.NRM_Prefix = stmNums.SN_Prefix.Substring(0, 3);
			matchingDetails.NRM_OwnerId = stmNums.SN_Owner;
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			Assertion.AssertEquals("Precondition", "", order.WD_TransportReference);
			order.TransportCoPK = transportCo.PK;
			factory.Save();

			var newLine = order.Lines.AddNew(); // add line with error
			newLine.WE_F3_NKPackType = Constants.PkgUnit.Unit;

			var processor = new GenerateTransportReferenceNumberProcessor(order);
			var ex = Assertion.AssertExceptionThrown<ZSaveException>(() => processor.Process(null));
			Assertion.AssertEquals("Error Reporter should not save data when there is error in order", "The WhsDocketLine cannot be inserted/updated, requires a reference to a valid OrgSupplierPart.", ex.FriendlyMessage);
		}

		#endregion

		#region TestProcess_Error_NoNumberFountain

		public static void TestProcess_Error_NoNumberFountain(BusinessObjectFactory factory, WhsTestHelperFunctions helper)
		{
			var data = new TestDataSimpleEnvironment(factory);
			var transportCo = factory.NewWithValidTestData<OrgHeader>();
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			Assertion.AssertEquals("Precondition", "", order.WD_TransportReference);

			order.TransportCoPK = transportCo.PK;
			factory.Save();

			var processor = new GenerateTransportReferenceNumberProcessor(order);
			Assertion.AssertNoExceptionThrown(() => processor.Process(null));
		}

		#endregion
	}
}

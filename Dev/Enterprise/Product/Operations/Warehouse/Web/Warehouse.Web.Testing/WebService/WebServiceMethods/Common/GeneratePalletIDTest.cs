using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Moq;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class GeneratePalletIDTest : WhsTransferSecureServiceTestCase
	{
		#region TestGeneratePalletID

		public void TestGeneratePalletID()
		{
			var webService = GetNewWebService();
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.Factory.Save();

			var mockGenerator = new Mock<IPalletIDGenerator>();
			mockGenerator.Setup(g => g.GenerateIDs(It.IsAny<WhsReceive>(), It.IsAny<int>(), It.IsAny<bool>(), 1)).Returns(new List<GeneratedID> { new GeneratedID("0001", 1) });
			mockGenerator.Setup(g => g.GenerateIDs(It.IsAny<WhsReceive>(), It.IsAny<int>(), It.IsAny<bool>(), 2)).Returns(new List<GeneratedID> { new GeneratedID("0002", 2) });

			using (ObjectFactory.Substitute(mockGenerator.Object))
			{
				AssertEquals("Precondition", receive.Lines.Count, 0);
				var response = webService.GeneratePalletID(receive.PK.ToGuid(), 1);
				AssertSuccessfulResponse(response, webService);
				AssertEquals("0001", response.PalletID);
				AssertEquals(2, response.UpdatedCountToBuildFrom);
				mockGenerator.Verify(g => g.GenerateIDs(It.Is<WhsReceive>(r => r.PK == receive.PK), 1, false, 1), Times.Once);

				Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
				Helper.Factory.Save();
				AssertEquals("Precondition", receive.Lines.Count, 1);

				var webService2 = GetNewWebService();
				var response2 = webService2.GeneratePalletID(receive.PK.ToGuid(), 2);
				AssertSuccessfulResponse(response2, webService2);
				AssertEquals("0002", response2.PalletID);
				AssertEquals(3, response2.UpdatedCountToBuildFrom);
				mockGenerator.Verify(g => g.GenerateIDs(It.Is<WhsReceive>(r => r.PK == receive.PK), 1, false, 2), Times.Once);
			}
		}

		public void TestGeneratePalletID_WhsTransfer()
		{
			var webService = GetNewWebService();
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transferLine.RunPreSaveValidation();
			Helper.Factory.Save();

			var mockGenerator = new Mock<IPalletIDGenerator>();
			mockGenerator.Setup(g => g.GenerateIDs(It.IsAny<WhsTransfer>(), It.IsAny<int>(), It.IsAny<bool>(), 1)).Returns(new List<GeneratedID> { new GeneratedID("0001", 1) });
			mockGenerator.Setup(g => g.GenerateIDs(It.IsAny<WhsTransfer>(), It.IsAny<int>(), It.IsAny<bool>(), 2)).Returns(new List<GeneratedID> { new GeneratedID("0002", 2) });

			using (ObjectFactory.Substitute(mockGenerator.Object))
			{
				AssertEquals("Precondition", transfer.Lines.Count, 1);
				var response = webService.GeneratePalletID(transfer.PK.ToGuid(), 1);
				AssertSuccessfulResponse(response, webService);
				AssertEquals("0001", response.PalletID);
				AssertEquals(2, response.UpdatedCountToBuildFrom);
				mockGenerator.Verify(g => g.GenerateIDs(It.Is<WhsTransfer>(r => r.PK == transfer.PK), 1, false, 1), Times.Once);

				var webService2 = GetNewWebService();
				var response2 = webService2.GeneratePalletID(transfer.PK.ToGuid(), 2);
				AssertSuccessfulResponse(response2, webService2);
				AssertEquals("0002", response2.PalletID);
				AssertEquals(3, response2.UpdatedCountToBuildFrom);
				mockGenerator.Verify(g => g.GenerateIDs(It.Is<WhsTransfer>(r => r.PK == transfer.PK), 1, false, 2), Times.Once);
			}
		}

		public void TestGeneratePalletID_NoIDsLeft()
		{
			var webService = GetNewWebService();
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.Factory.Save();

			var mockGenerator = new Mock<IPalletIDGenerator>();
			mockGenerator.Setup(g => g.GenerateIDs(It.IsAny<WhsReceive>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>())).Returns(new List<GeneratedID>());

			using (ObjectFactory.Substitute(mockGenerator.Object))
			{
				var response = webService.GeneratePalletID(receive.PK.ToGuid(), 1);
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("No IDs left. Failed to generate Pallet ID.", response.ErrorMessage);
				mockGenerator.Verify(g => g.GenerateIDs(It.Is<WhsReceive>(r => r.PK == receive.PK), 1, false, 1), Times.Once);
			}
		}

		public void TestGeneratePalletID_NoIDsLeft_Transfer()
		{
			var webService = GetNewWebService();
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transferLine.RunPreSaveValidation();
			Helper.Factory.Save();

			var mockGenerator = new Mock<IPalletIDGenerator>();
			mockGenerator.Setup(g => g.GenerateIDs(It.IsAny<WhsTransfer>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>())).Returns(new List<GeneratedID>());

			using (ObjectFactory.Substitute(mockGenerator.Object))
			{
				var response = webService.GeneratePalletID(transfer.PK.ToGuid(), 1);
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("No IDs left. Failed to generate Pallet ID.", response.ErrorMessage);
				mockGenerator.Verify(g => g.GenerateIDs(It.Is<WhsTransfer>(r => r.PK == transfer.PK), 1, false, 1), Times.Once);
			}
		}

		public void TestGeneratePalletID_InvalidReceive()
		{
			var webService = GetNewWebService();
			var data = new TestDataSimpleEnvironment(webService.Factory);
			var randomGuid = new Guid();
			var receive = webService.Factory.Load<WhsReceive>(randomGuid);

			AssertNull("Precondition: receive is null.", receive);
			var response = webService.GeneratePalletID(randomGuid, 1);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Docket is invalid. Failed to generate Pallet ID.", response.ErrorMessage);
		}

		public void TestGeneratePalletID_InvalidTransfer()
		{
			var webService = GetNewWebService();
			var data = new TestDataSimpleEnvironment(webService.Factory);
			var randomGuid = new Guid();
			var transfer = webService.Factory.Load<WhsTransfer>(randomGuid);

			AssertNull("Precondition: transfer is null.", transfer);
			var response = webService.GeneratePalletID(randomGuid, 1);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Docket is invalid. Failed to generate Pallet ID.", response.ErrorMessage);
		}

		public void TestGeneratePalletID_AfterSuspending_EndToEnd()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var part3 = Helper.CreateProduct("P3", data.Org1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive.WD_DocketID = "R1";
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 6m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 6m);
			Helper.CreateWhsReceiveInventoryLine(receive, part3, 6m);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var receivePK = receive.PK.ToGuid();
			var receiveResponse = webService.GetWhsReceive(receivePK, isUnloadProcess: true);
			AssertSuccessfulResponseWithNoErrors(receiveResponse, webService);
			AssertEquals("Precondition: Receive has started Receiving.", true, receive.StartedReceiving);

			// simulate first pallet ID generated
			var generatePalletResponse1 = webService.GeneratePalletID(receivePK, 1);
			AssertEquals("Precondition: Correct Pallet ID generated.", "R1-0001", generatePalletResponse1.PalletID);
			AssertEquals("Precondition: Correct Count to build from.", 2, generatePalletResponse1.UpdatedCountToBuildFrom);

			// simulate unloading first line
			var receiveLine1 = CreateWhsDocketLineInfo(receivePK, data.Part1.OP_PartNum, 6m, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", generatePalletResponse1.PalletID, "", data.Whs1.WW_DefaultOutboundDockDoor.ToGuid());
			var unloadResponse1 = webService.UnloadWhsReceiveLines(new[] { receiveLine1 }, Array.Empty<Guid>(), isEmptyAsnPalletIdMatchingEnabledForUnloadLine: true);
			AssertSuccessfulResponseWithNoErrors(unloadResponse1, webService);

			// simulate second pallet ID generated
			var generatePalletResponse2 = webService.GeneratePalletID(receivePK, generatePalletResponse1.UpdatedCountToBuildFrom);
			AssertEquals("Correct Pallet ID generated.", "R1-0002", generatePalletResponse2.PalletID);
			AssertEquals("Correct Count to build from.", 3, generatePalletResponse2.UpdatedCountToBuildFrom);

			// simulate generate pallet ID called after suspend
			var generatePalletResponse3 = webService.GeneratePalletID(receivePK, 1);
			AssertEquals("Correct Pallet ID generated.", "R1-0002", generatePalletResponse3.PalletID);
			AssertEquals("Correct Count to build from.", 3, generatePalletResponse3.UpdatedCountToBuildFrom);

			// simulate unloading second line
			var receiveLine2 = CreateWhsDocketLineInfo(receivePK, data.Part2.OP_PartNum, 6m, "UNT", "", "", "", "", ZDate.Empty, ZDate.Empty, "", generatePalletResponse3.PalletID, "", data.Whs1.WW_DefaultOutboundDockDoor.ToGuid());
			var unloadResponse2 = webService.UnloadWhsReceiveLines(new[] { receiveLine2 }, Array.Empty<Guid>(), isEmptyAsnPalletIdMatchingEnabledForUnloadLine: true);
			AssertSuccessfulResponseWithNoErrors(unloadResponse2, webService);

			// simulate next pallet ID generated for next line
			var generatePalletResponse4 = webService.GeneratePalletID(receivePK, generatePalletResponse3.UpdatedCountToBuildFrom);
			AssertEquals("Correct Pallet ID generated.", "R1-0003", generatePalletResponse4.PalletID);
			AssertEquals("Correct Count to build from.", 4, generatePalletResponse4.UpdatedCountToBuildFrom);
		}

		public void TestGeneratePalletID_AfterSuspending_Transfer_EndToEnd()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transferLine1.PickedTime = now;
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transferLine2.PickedTime = now;
			var transferLine3 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transferLine3.PickedTime = now;
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var transferPK = transfer.PK.ToGuid();

			// simulate first pallet ID generated
			var generatePalletResponse1 = webService.GeneratePalletID(transferPK, 1);
			AssertEquals("Precondition: Correct Pallet ID generated.", "W00000002-0001", generatePalletResponse1.PalletID);
			AssertEquals("Precondition: Correct Count to build from.", 2, generatePalletResponse1.UpdatedCountToBuildFrom);

			// simulate transfer first line
			var transferLineInfo1 = CreatePutawayTransferLineInfo(null, "", generatePalletResponse1.PalletID, "A-2");
			transferLineInfo1.PK = transferLine1.PK.ToGuid();
			transferLineInfo1.Qty = 10m;
			var webService1 = GetNewWebService(data.Whs1, user);
			var response1 = webService1.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo1, true, Guid.Empty);
			AssertSuccessfulResponseWithNoErrors(response1, webService1);

			// simulate second pallet ID generated
			var generatePalletResponse2 = webService.GeneratePalletID(transferPK, generatePalletResponse1.UpdatedCountToBuildFrom);
			AssertEquals("Correct Pallet ID generated.", "W00000002-0002", generatePalletResponse2.PalletID);
			AssertEquals("Correct Count to build from.", 3, generatePalletResponse2.UpdatedCountToBuildFrom);

			// simulate generate pallet ID called after suspend
			var generatePalletResponse3 = webService.GeneratePalletID(transferPK, 1);
			AssertEquals("Correct Pallet ID generated.", "W00000002-0002", generatePalletResponse3.PalletID);
			AssertEquals("Correct Count to build from.", 3, generatePalletResponse3.UpdatedCountToBuildFrom);

			// simulate transfer second line
			var transferLineInfo2 = CreatePutawayTransferLineInfo(null, "", generatePalletResponse3.PalletID, "A-2");
			transferLineInfo2.PK = transferLine2.PK.ToGuid();
			transferLineInfo2.Qty = 10m;
			var webService2 = GetNewWebService(data.Whs1, user);
			var response2 = webService2.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo2, true, Guid.Empty);
			AssertSuccessfulResponseWithNoErrors(response2, webService2);

			// simulate next pallet ID generated for next line
			var generatePalletResponse4 = webService.GeneratePalletID(transferPK, generatePalletResponse3.UpdatedCountToBuildFrom);
			AssertEquals("Correct Pallet ID generated.", "W00000002-0003", generatePalletResponse4.PalletID);
			AssertEquals("Correct Count to build from.", 4, generatePalletResponse4.UpdatedCountToBuildFrom);
		}

		#endregion
	}
}

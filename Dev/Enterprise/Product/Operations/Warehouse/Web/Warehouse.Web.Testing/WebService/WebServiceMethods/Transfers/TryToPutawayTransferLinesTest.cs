using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class TryToPutawayTransferLinesTest : WhsTransferSecureServiceTestCase
	{
		#region TestTryToPutawayTransferLines

		[TestDate(2014, 02, 02)]
		public void TestTryToPutawayTransferLines()
		{
			var now = ZDateTimeOffset.Now;
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");

			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA1, "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, locationA1, "PLT-2");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, locationA1, "PLT-3");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLineForPart1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 3m, "A-1", "PLT-1", "A-2", "");
			var transferLineForPart2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 3m, "A-1", "PLT-2", "A-2", "");
			var transferLine2ForPart2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 3m, "A-1", "PLT-3", "A-2", "");
			var transferLineForDifferentLocation = Helper.CreateWhsTransferLine(transfer, data.Part2, 3m, "A-1", "PLT-2", "A-3", "");
			transfer.RunPreSaveValidation(); // to commit inventory
			transferLineForPart1.PickedTime = now;
			transferLineForPart2.PickedTime = now;
			transferLine2ForPart2.PickedTime = now;
			transferLineForDifferentLocation.PickedTime = now;
			Helper.Factory.Save();

			var docketLineInfoForTransferLineForPart1 = CreatePutawayTransferLineInfo(data.Part1, "PLT-1", "", "A-2");
			docketLineInfoForTransferLineForPart1.PK = transferLineForPart1.PK.ToGuid();
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, user);

			var response = webService.TryToPutawayTransferLines(transfer.PK.ToGuid(), docketLineInfoForTransferLineForPart1,
				new[] { transferLineForPart1.PK.ToGuid(), transferLineForPart2.PK.ToGuid(), transferLine2ForPart2.PK.ToGuid() }, Guid.Empty);
			AssertSuccessfulResponse(response, webService);
			AssertNull("There shouldn't be any error messages.", response.ErrorMessage);

			AssertPutawayTransferLineData(transferLineForPart1, true, "A-2", "", now, "A.A");
			AssertPutawayTransferLineData(transferLineForPart2, true, "A-2", "", now, "A.A");
			AssertPutawayTransferLineData(transferLine2ForPart2, true, "A-2", "", now, "A.A");
		}

		[TestDate(2014, 02, 02)]
		public void TestTryToPutawayTransferLines_WithRowError()
		{
			var now = ZDateTimeOffset.Now;
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");

			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA1, "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, locationA1, "PLT-2");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, locationA1, "PLT-3");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLineForPart1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 3m, "A-1", "PLT-1", "A-2", "");
			var transferLineForPart2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 3m, "A-1", "PLT-2", "A-2", "");
			var transferLineForPart2WithError = Helper.CreateWhsTransferLine(transfer, data.Part2, 3m, "A-1", "PLT-3", "A-2", "");
			var transferLineForDifferentLocation = Helper.CreateWhsTransferLine(transfer, data.Part2, 3m, "A-1", "PLT-2", "A-3", "");
			transfer.RunPreSaveValidation(); // to commit inventory
			transferLineForPart1.PickedTime = now;
			transferLineForPart2.PickedTime = now;
			transferLineForPart2WithError.PickedTime = now;
			transferLineForDifferentLocation.PickedTime = now;
			Helper.Factory.Save();

			var docketLineInfoForTransferLineForPart1 = CreatePutawayTransferLineInfo(data.Part1, "PLT-1", "", "A-2");
			docketLineInfoForTransferLineForPart1.PK = transferLineForPart1.PK.ToGuid();
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, user);
			webService.Factory.Loaded += (s, e) =>
			{
				var transferLineForPart2WithErrorNew = e.NewObjects.FirstOrDefault(b => b.PK == transferLineForPart2WithError.PK);
				if (transferLineForPart2WithErrorNew != null)
				{
					transferLineForPart2WithErrorNew.AddRowError("Error to stop finalise of the line.");
				}
			};

			var response = webService.TryToPutawayTransferLines(transfer.PK.ToGuid(), docketLineInfoForTransferLineForPart1,
				new[] { transferLineForPart1.PK.ToGuid(), transferLineForPart2.PK.ToGuid(), transferLineForPart2WithError.PK.ToGuid() }, Guid.Empty);
			AssertSuccessfulResponse(response, webService);
			AssertEquals("Error to stop finalise of the line.", response.ErrorMessage);

			CombineAssertions(() =>
			{
				AssertEquals(false, response.IsAllTransferLinesTransferredOrFinalised);
				AssertArrayEqualsByElements(response.NonFinalisedTransferLinesPks.ToArray(), new Guid[] { transferLineForPart1.PK.ToGuid(), transferLineForPart2.PK.ToGuid(), transferLineForPart2WithError.PK.ToGuid() });
			});
		}

		#endregion

		#region TestTryToPutawayTransferLines_ForPickFaceLocation

		[TestDate(2014, 02, 02)]
		public void TestTryToPutawayTransferLines_ForPickFaceLocation()
		{
			var now = ZDateTimeOffset.Now;
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, locationA2);

			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 8m, locationA1, "PLT-1");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLineForPart1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 3m, "A-1", "PLT-1", "A-2", "PLT-2");
			transferLineForPart1.PickedTime = now;
			Helper.Factory.Save();

			var docketLineInfoForTransferLineForPart1 = CreatePutawayTransferLineInfo(data.Part1, "PLT-1", "PLT-2", "A-2");
			docketLineInfoForTransferLineForPart1.PK = transferLineForPart1.PK.ToGuid();
			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToPutawayTransferLines(transfer.PK.ToGuid(), docketLineInfoForTransferLineForPart1,
				new[] { transferLineForPart1.PK.ToGuid() }, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			AssertNull("There shouldn't be any error messages.", response.ErrorMessage);
			// destination palletId should be cleared
			AssertPutawayTransferLineData(transferLineForPart1, true, "A-2", "", now, "A.A");
			AssertEquals(true, response.IsAllTransferLinesTransferredOrFinalised);
		}

		[TestDate(2014, 02, 02)]
		public void TestTryToPutawayTransferLines_ForPickFaceLocation_DifferentWarehousePickFaces()
		{
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var differentWarehouse = Helper.CreateWarehouse("WHS2", "A", 2, 1);
			Helper.Factory.Save();
			var bulkLocationInWhs1 = data.Whs1.FindLocation("A-1");
			var pickfaceLocationInWhs2 = differentWarehouse.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocationInWhs2);

			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 8m, bulkLocationInWhs1, "PLT-1");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLineForPart1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 3m, "A-1", "PLT-1", "A-2", "PLT-2");
			transferLineForPart1.PickedTime = ZDateTimeOffset.Now;
			Helper.Factory.Save();

			var docketLineInfoForTransferLineForPart1 = CreatePutawayTransferLineInfo(data.Part1, "PLT-1", "PLT-2", "A-2");
			docketLineInfoForTransferLineForPart1.PK = transferLineForPart1.PK.ToGuid();
			var webService1 = GetNewWebService(data.Whs1, user);
			var response1 = webService1.TryToPutawayTransferLines(transfer.PK.ToGuid(), docketLineInfoForTransferLineForPart1,
				new[] { transferLineForPart1.PK.ToGuid() }, Guid.Empty);
			AssertSuccessfulResponse(response1, webService1);

			AssertNull("There shouldn't be any error messages.", response1.ErrorMessage);
			AssertPutawayTransferLineData(transferLineForPart1, true, "A-2", "PLT-2", now.ToOffset(), "A.A"); // destination palletId should be not dropped since A-2 is a pickface in a different warehouse
			AssertEquals("All transfer lines should be finalised.", true, response1.IsAllTransferLinesTransferredOrFinalised);

			var webService2 = GetNewWebService(data.Whs1, user);
			var invalidPK = Guid.NewGuid();
			AssertBusinessValidationError(webService2, $"Transfer with PK = '{invalidPK}' cannot be found.", webService2.TryToPutawayTransferLines(invalidPK, docketLineInfoForTransferLineForPart1,
				new[] { transferLineForPart1.PK.ToGuid() }, Guid.Empty));
		}

		#endregion

		#region TestTryToPutawayTransferLines_InterWhsChild

		public void TestTryToPutawayTransferLines_InterWhsChild_Dest()
		{
			TestTryToPutawayTransferLines_InterWhsChild(isSource: false);
		}

		public void TestTryToPutawayTransferLines_InterWhsChild_Source()
		{
			TestTryToPutawayTransferLines_InterWhsChild(isSource: true);
		}

		void TestTryToPutawayTransferLines_InterWhsChild(bool isSource)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var warehouse2 = Helper.CreateWarehouse("WH2", "A", 1, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, isSource ? data.Whs1 : warehouse2, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_DocketSubType = isSource ? TransferType.Codes.InterWhsSource : TransferType.Codes.InterWhsDest;
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, "A", warehouse2.PK, "A");
			transferLine.RunPreSaveValidation();

			var now = ZDateTimeOffset.Now;
			transferLine.PickedTime = now;
			AssertEquals("Precondition.", DocketLineStatus.Codes.HeldForTransfer, transferLine.WE_DocketLineStatus);

			var childTransfer = transfer.ChildTransfers.First();
			AssertNotNull("Precondition: Created child transfer.", childTransfer);
			Helper.Factory.Save();

			var transferLineInfo = CreatePutawayTransferLineInfo(data.Part1, "", "", "A");
			transferLineInfo.PK = transferLine.PK.ToGuid();
			var webService = GetNewWebService(warehouse2, staff);
			AssertBusinessValidationError(webService, "Should have thrown an exception as child inter Warehouse transfers are not supported.",
				"Cannot transfer an Inter-Warehouse Transfer using the child job.",
				webService.TryToPutawayTransferLines(childTransfer.PK.ToGuid(), transferLineInfo, new[] { transferLine.PK.ToGuid() }, Guid.Empty));
		}

		#endregion

		#region TestTryToPutawayTransferLines_MaxPalletIdLengthExceeded

		public void TestTryToPutawayTransferLines_MaxPalletIdLengthExceeded()
		{
			var now = ZDateTimeOffset.Now;
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");

			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA1, "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, locationA1, "PLT-2");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLineForPart1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 3m, "A-1", "PLT-1", "A-2", "");
			var transferLineForPart2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 3m, "A-1", "PLT-2", "A-2", "");
			transfer.RunPreSaveValidation(); // to commit inventory
			transferLineForPart1.PickedTime = now;
			transferLineForPart2.PickedTime = now;
			Helper.Factory.Save();

			var destinationPalletId = "1234567890123456789012345678901";
			var docketLineInfoForTransferLineForPart1 = CreatePutawayTransferLineInfo(data.Part1, "PLT-1", destinationPalletId, "A-2");
			docketLineInfoForTransferLineForPart1.PK = transferLineForPart1.PK.ToGuid();
			Helper.Factory.Save();

			Assert("Precondition: Destination pallet id exceeds maximum length for pallet ids.", destinationPalletId.Length > WhsDocketLineSchema.WE_PalletID.MaxLength);
			var webService = GetNewWebService(data.Whs1, user);

			TransferLinesPutawayWebServiceResponse response = null;
			AssertNoExceptionThrown("No exception is thrown.", () =>
				response = webService.TryToPutawayTransferLines(
					transfer.PK.ToGuid(),
					docketLineInfoForTransferLineForPart1,
					new[] { transferLineForPart1.PK.ToGuid(), transferLineForPart2.PK.ToGuid() },
					Guid.Empty
					));

			AssertEquals("No error reported.", string.Empty, ErrorReporter.LastMessageReported);
			AssertNotNull("Response is not null.", response);
			CombineAssertions(() =>
			{
				AssertEquals("Error response is returned.", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Error response is returned.",
					"WE_PalletID exceeds maximum length allowed. The maximum length of this property is 30 characters, but 31 were entered.", response.ErrorMessage);
				AssertContainsExactElementsInAnyOrder(new[] { transferLineForPart1.PK.ToGuid(), transferLineForPart2.PK.ToGuid() }, response.NonFinalisedTransferLinesPks);
			});
		}

		#endregion

		#region TestTryToPutawayTransferLines_WithTransferTaskPK

		[TestDate(2025, 5, 14)]
		public void TestTryToPutawayTransferLines_WithTransferTaskPK_AllLinesOnSameTask()
		{
			var now = ZDateTimeOffset.Now;
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");

			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA1, "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, locationA1, "PLT-2");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, locationA1, "PLT-3");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA1, "PLT-4");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine1ForPart1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 3m, "A-1", "PLT-1", "A-2", "");
			var transferLine1ForPart2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 3m, "A-1", "PLT-2", "A-2", "");
			var transferLine2ForPart2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 3m, "A-1", "PLT-3", "A-2", "");
			var transferLine2ForPart1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 3m, "A-1", "PLT-4", "A-2", "");
			transfer.RunPreSaveValidation(); // to commit inventory
			transferLine1ForPart1.PickedTime = now;
			transferLine1ForPart2.PickedTime = now;
			transferLine2ForPart2.PickedTime = now;
			transferLine2ForPart1.PickedTime = now;
			Helper.Factory.Save();

			var transferProcessTask = Helper.CreateProcessTaskForTransfer(transfer, user);

			var docketLineInfoForTransferLineForPart1 = CreatePutawayTransferLineInfo(data.Part1, "PLT-1", "", "A-2");
			docketLineInfoForTransferLineForPart1.PK = transferLine1ForPart1.PK.ToGuid();
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToPutawayTransferLines(transfer.PK.ToGuid(), docketLineInfoForTransferLineForPart1,
				[transferLine1ForPart1.PK.ToGuid(), transferLine1ForPart2.PK.ToGuid(), transferLine2ForPart2.PK.ToGuid()], transferProcessTask.PK.ToGuid());
			AssertSuccessfulResponse(response, webService);
			AssertNull("There shouldn't be any error messages.", response.ErrorMessage);

			AssertPutawayTransferLineData(transferLine1ForPart1, true, "A-2", "", now, "A.A");
			AssertPutawayTransferLineData(transferLine1ForPart2, true, "A-2", "", now, "A.A");
			AssertPutawayTransferLineData(transferLine2ForPart2, true, "A-2", "", now, "A.A");
			AssertEquals(false, response.IsAllTransferLinesTransferredOrFinalised);
		}

		[TestDate(2025, 5, 14)]
		public void TestTryToPutawayTransferLines_WithTransferTaskPKCore_NotAllLinesOnProcessTask()
		{
			var now = ZDateTimeOffset.Now;
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");

			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA1, "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, locationA1, "PLT-2");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, locationA1, "PLT-3");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA1, "PLT-4");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine1ForPart1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 3m, "A-1", "PLT-1", "A-2", "");
			var transferLine1ForPart2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 3m, "A-1", "PLT-2", "A-2", "");
			var transferLine2ForPart2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 3m, "A-1", "PLT-3", "A-2", "");
			var transferLine2ForPart1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 3m, "A-1", "PLT-4", "A-2", "");
			transfer.RunPreSaveValidation(); // to commit inventory
			transferLine1ForPart1.PickedTime = now;
			transferLine1ForPart2.PickedTime = now;
			transferLine2ForPart2.PickedTime = now;
			transferLine2ForPart1.PickedTime = now;
			Helper.Factory.Save();

			var transferProcessTask = Helper.CreateProcessTaskForTransfer(transfer, user);
			transferLine2ForPart1.WE_P9_Task = Guid.Empty;

			var docketLineInfoForTransferLineForPart1 = CreatePutawayTransferLineInfo(data.Part1, "PLT-1", "", "A-2");
			docketLineInfoForTransferLineForPart1.PK = transferLine1ForPart1.PK.ToGuid();
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.TryToPutawayTransferLines(transfer.PK.ToGuid(), docketLineInfoForTransferLineForPart1,
				[transferLine1ForPart1.PK.ToGuid(), transferLine1ForPart2.PK.ToGuid(), transferLine2ForPart2.PK.ToGuid()], transferProcessTask.PK.ToGuid());
			AssertSuccessfulResponse(response, webService);
			AssertNull("There shouldn't be any error messages.", response.ErrorMessage);

			AssertPutawayTransferLineData(transferLine1ForPart1, true, "A-2", "", now, "A.A");
			AssertPutawayTransferLineData(transferLine1ForPart2, true, "A-2", "", now, "A.A");
			AssertPutawayTransferLineData(transferLine2ForPart2, true, "A-2", "", now, "A.A");
			AssertPutawayTransferLineData(transferLine2ForPart1, false, "A-2", "", ZDateTimeOffset.Empty, "");
			AssertEquals(true, response.IsAllTransferLinesTransferredOrFinalised);
		}

		#endregion
	}
}

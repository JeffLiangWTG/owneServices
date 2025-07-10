using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsPickLineInPickFaceAfterPickingService))]
	public class WhsPickLineInPickFaceAfterPickingServiceTest : WhsTestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new WhsPickLineInPickFaceAfterPickingService(null));
		}

		public void TestNudgesPFR()
		{
			var staff = Helper.CreateGlbStaff("ABC", "ABC");
			var data = new TestDataSimpleEnvironment(Factory);
			var defaultLocation = data.Whs1.FindLocation("A");
			defaultLocation.LocationType.WLT_LocationClass = "FIX";
			defaultLocation.LocationType.WLT_MaximumNumberOfProducts = 200;
			Helper.CreateProductPickFace(data.Part1, data.Org1, defaultLocation, 100m, 200m);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 200m);
			Factory.Save();

			var nudgerMock = new Mock<IServiceTaskNudger>();
			using (ObjectFactory.Substitute(nudgerMock.Object))
			{
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 101m);
				var pick = Helper.CreatePickNew(order);
				var pickLine = pick.GetAllPickLines().Single();
				pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;

				AssertEquals("Precondition: StockOnHand is less than PickReplenishmentMinimum.", true, receive.Lines.Sum(l => l.WE_StockOnHand) < 100);
				nudgerMock.Verify(nudger => nudger.NudgeServiceTask("PFR", null), Times.Never);

				Factory.Save();

				nudgerMock.Verify(nudger => nudger.NudgeServiceTask("PFR", null), Times.Once);
			}
		}

		public void TestNudgesPFR_WhenThereIsHeldStock()
		{
			var staff = Helper.CreateGlbStaff("ABC", "ABC");
			var data = new TestDataSimpleEnvironment(Factory);
			var defaultLocation = data.Whs1.FindLocation("A");
			defaultLocation.LocationType.WLT_LocationClass = "FIX";
			defaultLocation.LocationType.WLT_MaximumNumberOfProducts = 200;
			Helper.CreateProductPickFace(data.Part1, data.Org1, defaultLocation, 100m, 200m);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 250m);
			Factory.Save();

			var receiveLine = receive.Lines[0];
			receiveLine.HeldCodeChangeQuantity = 50m;
			receiveLine.HeldCodeToChangeTo = "HEL";
			receiveLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			var nudgerMock = new Mock<IServiceTaskNudger>();
			using (ObjectFactory.Substitute(nudgerMock.Object))
			{
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 101m);
				var pick = Helper.CreatePickNew(order);
				var pickLine = pick.GetAllPickLines().Single();
				pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;

				AssertEquals("Precondition: StockOnHand is less than PickReplenishmentMinimum.", true, receive.Lines.Sum(l => l.WE_StockOnHand) < 100);
				nudgerMock.Verify(nudger => nudger.NudgeServiceTask("PFR", null), Times.Never);

				Factory.Save();

				nudgerMock.Verify(nudger => nudger.NudgeServiceTask("PFR", null), Times.Once);
			}
		}

		public void TestNudgesPFR_WhenThereAreUnrelatedJobs_UnfinalisedReceive()
		{
			var staff = Helper.CreateGlbStaff("ABC", "ABC");
			var data = new TestDataSimpleEnvironment(Factory);
			var defaultLocation = data.Whs1.FindLocation("A");
			Helper.CreateProductPickFace(data.Part1, data.Org1, defaultLocation, 100m, 200m);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 200m, defaultLocation, "");
			Factory.Save();

			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, defaultLocation, "", finalise: false);
			Factory.Save();

			var nudgerMock = new Mock<IServiceTaskNudger>();
			using (ObjectFactory.Substitute(nudgerMock.Object))
			{
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 101);
				var pick = Helper.CreatePickNew(order);
				var pickLine = pick.GetAllPickLines().Single();
				pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;

				AssertEquals("Precondition: StockOnHand is less than PickReplenishmentMinimum.", true, receive1.Lines.Sum(l => l.WE_StockOnHand) < 100);
				nudgerMock.Verify(nudger => nudger.NudgeServiceTask("PFR", null), Times.Never);

				Factory.Save();

				nudgerMock.Verify(nudger => nudger.NudgeServiceTask("PFR", null), Times.Once);
			}
		}

		public void TestNudgesPFR_WhenThereAreUnrelatedJobs_FinalisedTransfer()
		{
			var staff = Helper.CreateGlbStaff("ABC", "ABC");
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var defaultLocation = data.Whs1.FindLocation("A-1");
			var pickFaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, 100m, 200m);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m, defaultLocation, "");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 200m, pickFaceLocation, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, defaultLocation, pickFaceLocation);
			transfer.RunPreSaveValidation();
			Factory.Save();

			var nudgerMock = new Mock<IServiceTaskNudger>();
			using (ObjectFactory.Substitute(nudgerMock.Object))
			{
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 101m);
				var pick = Helper.CreatePickNew(order);

				transferLine.FinaliseDocketLine();
				AssertEquals("Precondition: Finalized.", true, transferLine.IsFinalised);
				Factory.Save();

				var pickLine = pick.GetAllPickLines().Single();
				pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;

				AssertEquals("Precondition: StockOnHand is less than PickReplenishmentMinimum.", true, receive2.Lines.Sum(l => l.WE_StockOnHand) < 100);
				nudgerMock.Verify(nudger => nudger.NudgeServiceTask("PFR", null), Times.Never);

				Factory.Save();

				nudgerMock.Verify(nudger => nudger.NudgeServiceTask("PFR", null), Times.Once);
			}
		}

		public void TestNudgesPFR_WhenThereAreUnrelatedJobs_UnfinalisedTransfer_DifferentProduct()
		{
			var staff = Helper.CreateGlbStaff("ABC", "ABC");
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var defaultLocation = data.Whs1.FindLocation("A-1");
			var pickFaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, 100m, 200m);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickFaceLocation, 100m, 200m);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 200m, pickFaceLocation, "");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 200m, defaultLocation, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, defaultLocation, pickFaceLocation);
			transfer.RunPreSaveValidation();
			Factory.Save();

			var nudgerMock = new Mock<IServiceTaskNudger>();
			using (ObjectFactory.Substitute(nudgerMock.Object))
			{
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 101m);
				var pick = Helper.CreatePickNew(order);
				var pickLine = pick.GetAllPickLines().Single();
				pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;

				AssertLessThan("Precondition: StockOnHand is less than PickReplenishmentMinimum.", receive1.Lines.Sum(l => l.WE_StockOnHand), 100);
				nudgerMock.Verify(nudger => nudger.NudgeServiceTask("PFR", null), Times.Never);

				Factory.Save();

				nudgerMock.Verify(nudger => nudger.NudgeServiceTask("PFR", null), Times.Once);
			}
		}

		public void TestNudgesPFR_WhenThereAreUnrelatedJobs_UnfinalisedTransfer_DifferentClient()
		{
			var staff = Helper.CreateGlbStaff("ABC", "ABC");
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var org2 = Helper.CreateClient("C2");
			Helper.CreateProductClientRelationShip(org2, data.Part1);

			var defaultLocation = data.Whs1.FindLocation("A-1");
			var pickFaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, 100m, 200m);
			Helper.CreateProductPickFace(data.Part1, org2, pickFaceLocation, 100m, 200m);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 200m, pickFaceLocation, "");
			var receive2 = Helper.CreateWhsReceiveWithInventory(org2, data.Whs1, "R2", data.Part1, 200m, defaultLocation, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(org2, data.Whs1, "T1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, defaultLocation, pickFaceLocation);
			transfer.RunPreSaveValidation();
			Factory.Save();

			var nudgerMock = new Mock<IServiceTaskNudger>();
			using (ObjectFactory.Substitute(nudgerMock.Object))
			{
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 101m);
				var pick = Helper.CreatePickNew(order);
				var pickLine = pick.GetAllPickLines().Single();
				pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;

				AssertLessThan("Precondition: StockOnHand is less than PickReplenishmentMinimum.", receive1.Lines.Sum(l => l.WE_StockOnHand), 100);
				nudgerMock.Verify(nudger => nudger.NudgeServiceTask("PFR", null), Times.Never);

				Factory.Save();

				nudgerMock.Verify(nudger => nudger.NudgeServiceTask("PFR", null), Times.Once);
			}
		}

		public void TestNudgesPFR_WhenThereAreUnrelatedJobs_UnfinalisedTransfer_DifferentLocation()
		{
			var staff = Helper.CreateGlbStaff("ABC", "ABC");
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var defaultLocation = data.Whs1.FindLocation("A-1");
			var pickFaceLocation1 = data.Whs1.FindLocation("A-2");
			var pickFaceLocation2 = data.Whs1.FindLocation("A-3");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation1, 100m, 200m);
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation2, 100m, 200m);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m, defaultLocation, "");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 200m, pickFaceLocation1, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, defaultLocation, pickFaceLocation2);
			transfer.RunPreSaveValidation();
			Factory.Save();

			var nudgerMock = new Mock<IServiceTaskNudger>();
			using (ObjectFactory.Substitute(nudgerMock.Object))
			{
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 101m);
				var pick = Helper.CreatePickNew(order);
				var pickLine = pick.GetAllPickLines().Single();
				pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;

				AssertLessThan("Precondition: StockOnHand is less than PickReplenishmentMinimum.", receive2.Lines.Sum(l => l.WE_StockOnHand), 100);
				nudgerMock.Verify(nudger => nudger.NudgeServiceTask("PFR", null), Times.Never);

				Factory.Save();

				nudgerMock.Verify(nudger => nudger.NudgeServiceTask("PFR", null), Times.Once);
			}
		}

		public void TestDoesNotNudgePFR_WhenThereIsAnUnfinalisedTransfer()
		{
			var staff = Helper.CreateGlbStaff("ABC", "ABC");
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var defaultLocation = data.Whs1.FindLocation("A-1");
			var pickFaceLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation, 100m, 200m);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m, defaultLocation, "");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 200m, pickFaceLocation, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, defaultLocation, pickFaceLocation);
			transfer.RunPreSaveValidation();
			Factory.Save();

			var nudgerMock = new Mock<IServiceTaskNudger>();
			using (ObjectFactory.Substitute(nudgerMock.Object))
			{
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 101m);
				var pick = Helper.CreatePickNew(order);
				var pickLine = pick.GetAllPickLines().Single();
				pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;

				AssertEquals("Precondition: StockOnHand is less than PickReplenishmentMinimum.", true, receive2.Lines.Sum(l => l.WE_StockOnHand) < 100);
				nudgerMock.Verify(nudger => nudger.NudgeServiceTask("PFR", null), Times.Never);

				Factory.Save();

				// Should not nudge as there is a pending transfer that will block auto replenishment
				nudgerMock.Verify(nudger => nudger.NudgeServiceTask("PFR", null), Times.Never);
			}
		}

		public void TestDoesNotNudgePFR_WhenLocationClassIsNotFIX()
		{
			var staff = Helper.CreateGlbStaff("ABC", "ABC");
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var defaultLocation = data.Whs1.FindLocation("A-1");
			defaultLocation.LocationType.WLT_LocationClass = "NOR";

			var fixLocationType = Helper.CreateLocationType("FPF", "Fixed Pick Face", isPalletIDNeutral: false, 200, "FIX");
			var fixLocation = data.Whs1.FindLocation("A-2");
			fixLocation.WLV_WLT_LocationType = fixLocationType.PK;
			Helper.CreateProductPickFace(data.Part1, data.Org1, fixLocation, 100m, 200m);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 200m, defaultLocation);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var nudgerMock = new Mock<IServiceTaskNudger>();
			using (ObjectFactory.Substitute(nudgerMock.Object))
			{
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 111m);
				var pick = Helper.CreatePickNew(order);
				var pickLine = pick.GetAllPickLines().Single();
				pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
				Factory.Save();

				nudgerMock.Verify(nudger => nudger.NudgeServiceTask("PFR", null), Times.Never);
			}
		}

		public void TestDoesNotNudgePFR_WhenProductAndClientDoesNotHavePickFace()
		{
			var staff = Helper.CreateGlbStaff("ABC", "ABC");
			var data = new TestDataSimpleEnvironment(Factory);
			var defaultLocation = data.Whs1.FindLocation("A");
			defaultLocation.LocationType.WLT_LocationClass = "FIX";
			defaultLocation.LocationType.WLT_MaximumNumberOfProducts = 200;
			var pickFace = Helper.CreateProductPickFace(data.Part1, data.Org1, defaultLocation, 100m, 200m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 200m);
			Factory.Save();

			pickFace.Delete();
			Factory.Save();

			var nudgerMock = new Mock<IServiceTaskNudger>();
			using (ObjectFactory.Substitute(nudgerMock.Object))
			{
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 111m);
				var pick = Helper.CreatePickNew(order);
				var pickLine = pick.GetAllPickLines().Single();
				pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
				Factory.Save();
			}

			nudgerMock.Verify(nudger => nudger.NudgeServiceTask("PFR", null), Times.Never);
		}

		public void TestDoesNotNudgePFR_WhenStockOnHandIsMoreThanPickReplenishmentMinimum()
		{
			var staff = Helper.CreateGlbStaff("ABC", "ABC");
			var data = new TestDataSimpleEnvironment(Factory);
			var defaultLocation = data.Whs1.FindLocation("A");
			defaultLocation.LocationType.WLT_LocationClass = "FIX";
			defaultLocation.LocationType.WLT_MaximumNumberOfProducts = 200;
			Helper.CreateProductPickFace(data.Part1, data.Org1, defaultLocation, 100m, 200m);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 200m);
			Factory.Save();

			var nudgerMock = new Mock<IServiceTaskNudger>();
			using (ObjectFactory.Substitute(nudgerMock.Object))
			{
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
				var pick = Helper.CreatePickNew(order);
				var pickLine = pick.GetAllPickLines().Single();
				pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
				AssertEquals("Precondition: StockOnHand is more than PickReplenishmentMinimum.", true, receive.Lines.Sum(l => l.WE_StockOnHand) > 100);
				Factory.Save();

				nudgerMock.Verify(nudger => nudger.NudgeServiceTask("PFR", null), Times.Never);
			}
		}

		public void TestDoesNotNudgePFR_WhenStockOnHandEqualsReplenishMinimum()
		{
			var staff = Helper.CreateGlbStaff("ABC", "ABC");
			var data = new TestDataSimpleEnvironment(Factory);
			var defaultLocation = data.Whs1.FindLocation("A");
			defaultLocation.LocationType.WLT_LocationClass = "FIX";
			defaultLocation.LocationType.WLT_MaximumNumberOfProducts = 200;
			Helper.CreateProductPickFace(data.Part1, data.Org1, defaultLocation, 100m, 200m);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 200m);
			Factory.Save();

			var nudgerMock = new Mock<IServiceTaskNudger>();
			using (ObjectFactory.Substitute(nudgerMock.Object))
			{
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 100m);
				var pick = Helper.CreatePickNew(order);
				var pickLine = pick.GetAllPickLines().Single();
				pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
				AssertEquals("Precondition: StockOnHand is equal to PickReplenishmentMinimum.", true, receive.Lines.Sum(l => l.WE_StockOnHand) == 100);
				nudgerMock.Verify(nudger => nudger.NudgeServiceTask("PFR", null), Times.Never);
				Factory.Save();

				nudgerMock.Verify(nudger => nudger.NudgeServiceTask("PFR", null), Times.Once);
			}
		}

		public void TestDoesNotNudgePFR_WhenSumOfStockOnHandIsMoreThanPickReplenishmentMinimum()
		{
			var staff = Helper.CreateGlbStaff("ABC", "ABC");
			var data = new TestDataSimpleEnvironment(Factory);
			var defaultLocation = data.Whs1.FindLocation("A");
			defaultLocation.LocationType.WLT_LocationClass = "FIX";
			defaultLocation.LocationType.WLT_MaximumNumberOfProducts = 200;
			Helper.CreateProductPickFace(data.Part1, data.Org1, defaultLocation, 100m, 200m);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			receive1.WD_ArrivalDate = new DateTime(2020, 2, 2);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 100m);
			Factory.Save();

			var nudgerMock = new Mock<IServiceTaskNudger>();
			using (ObjectFactory.Substitute(nudgerMock.Object))
			{
				var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
				var pick = Helper.CreatePickNew(order1);
				var pickLine = pick.GetAllPickLines().Single();
				AssertEquals("Precondition: will pick from receive1 and cause the stock on hand lower than 100.", receive1.Lines[0].PK, pickLine.WZ_WE_InventoryLine);

				pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
				AssertEquals("Precondition: StockOnHand is more than PickReplenishmentMinimum.", true, receive1.Lines.Sum(l => l.WE_StockOnHand) + receive2.Lines.Sum(l => l.WE_StockOnHand) > 100);
				Factory.Save();

				nudgerMock.Verify(nudger => nudger.NudgeServiceTask("PFR", null), Times.Never);
			}
		}

		public void TestDoesNotNudgePFR_WhenSumOfStockOnHandEqualsPickReplenishmentMinimum()
		{
			var staff = Helper.CreateGlbStaff("ABC", "ABC");
			var data = new TestDataSimpleEnvironment(Factory);
			var defaultLocation = data.Whs1.FindLocation("A");
			defaultLocation.LocationType.WLT_LocationClass = "FIX";
			defaultLocation.LocationType.WLT_MaximumNumberOfProducts = 200;
			Helper.CreateProductPickFace(data.Part1, data.Org1, defaultLocation, 100m, 200m);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			receive1.WD_ArrivalDate = new DateTime(2020, 2, 2);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 100m);
			Factory.Save();

			var nudgerMock = new Mock<IServiceTaskNudger>();
			using (ObjectFactory.Substitute(nudgerMock.Object))
			{
				var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 100m);
				var pick = Helper.CreatePickNew(order1);
				var pickLine = pick.GetAllPickLines().Single();
				AssertEquals("Precondition: will pick from receive1 and cause the stock on hand equal to 100.", receive1.Lines[0].PK, pickLine.WZ_WE_InventoryLine);

				pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
				AssertEquals("Precondition: StockOnHand is equal to PickReplenishmentMinimum.", true, receive1.Lines.Sum(l => l.WE_StockOnHand) + receive2.Lines.Sum(l => l.WE_StockOnHand) == 100);
				nudgerMock.Verify(nudger => nudger.NudgeServiceTask("PFR", null), Times.Never);
				Factory.Save();

				nudgerMock.Verify(nudger => nudger.NudgeServiceTask("PFR", null), Times.Once);
			}
		}

		public void TestNudgesPFR_WhenSumOfStockOnHandIsLessThanPickReplenishmentMinimum()
		{
			var staff = Helper.CreateGlbStaff("ABC", "ABC");
			var data = new TestDataSimpleEnvironment(Factory);
			var defaultLocation = data.Whs1.FindLocation("A");
			defaultLocation.LocationType.WLT_LocationClass = "FIX";
			defaultLocation.LocationType.WLT_MaximumNumberOfProducts = 200;
			Helper.CreateProductPickFace(data.Part1, data.Org1, defaultLocation, 100m, 200m);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			receive1.WD_ArrivalDate = new DateTime(2020, 2, 2);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 50m);
			Factory.Save();

			var nudgerMock = new Mock<IServiceTaskNudger>();
			using (ObjectFactory.Substitute(nudgerMock.Object))
			{
				var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 90m);
				var pick = Helper.CreatePickNew(order1);
				var pickLine = pick.GetAllPickLines().Single();
				AssertEquals("Precondition: will pick from receive1 and cause the stock on hand lower than 100.", receive1.Lines[0].PK, pickLine.WZ_WE_InventoryLine);

				pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;

				AssertEquals("Precondition: StockOnHand is less than PickReplenishmentMinimum.", true, receive1.Lines.Sum(l => l.WE_StockOnHand) + receive2.Lines.Sum(l => l.WE_StockOnHand) < 100);
				nudgerMock.Verify(nudger => nudger.NudgeServiceTask("PFR", null), Times.Never);

				Factory.Save();

				nudgerMock.Verify(nudger => nudger.NudgeServiceTask("PFR", null), Times.Once);
			}
		}
	}
}

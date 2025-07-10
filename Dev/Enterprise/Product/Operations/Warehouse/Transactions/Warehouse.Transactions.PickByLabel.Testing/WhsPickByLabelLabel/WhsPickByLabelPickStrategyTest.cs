using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Warehouse.Transactions.PickByLabel.Testing
{
	public class WhsPickByLabelPickStrategyTest : WhsTestCaseWithFactory
	{
		#region OnFinalised

		public void TestOnFinalised_PickIsNotFinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			var releaseLine = order.Lines[0].ReleaseLines[0];
			package.Pack(releaseLine, 5m);
			Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package.PK);
			Factory.Save();

			var pickByLabelStrategy = new WhsPickByLabelPickStrategy();
			pickByLabelStrategy.OnFinalised(Factory, pick);

			AssertEquals("PickByLabelJob should not be finalised if Pick is not Finalised", true, pickByLabelJob.WTK_FinalisedDate.IsEmpty);
		}

		public void TestOnFinalised_OneJobWithMultipleLabelsLinkToDifferentPicks()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick1 = Helper.CreatePickNew(order1);
			var package1 = order1.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			var releaseLine1 = order1.Lines[0].ReleaseLines[0];
			package1.Pack(releaseLine1, 5m);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var pick2 = Helper.CreatePickNew(order2);
			var package2 = order2.PackageJob.Packages.AddNew("PLT", "PACKAGE-2");
			var releaseLine2 = order2.Lines[0].ReleaseLines[0];
			package2.Pack(releaseLine2, 5m);
			Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package1.PK);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package2.PK);
			Factory.Save();

			AssertEquals("Precondition: PickByLabelJob should has 2 labels", 2, pickByLabelJob.Labels.Count);

			var pickByLabelPickStrategyMock = new Mock<IWhsPickByLabelPickStrategy>();
			pickByLabelPickStrategyMock.Setup(m => m.OnFinalised(It.IsAny<BusinessObjectFactory>(), It.IsAny<IWhsPick>()))
				.Callback(() => { });
			using (ObjectFactory.Substitute(pickByLabelPickStrategyMock.Object))
			{
				pick1.FinaliseAllOrders();
				pick1.FinalisePick();
			}

			AssertIsFinalisedPrecondition(pick1);

			var pickByLabelStrategy = new WhsPickByLabelPickStrategy();
			pickByLabelStrategy.OnFinalised(Factory, pick1);

			AssertEquals("PickByLabelJob should be finalised", false, pickByLabelJob.WTK_FinalisedDate.IsEmpty);

			var pickByLabelQuery = new ZQuery(WhsPickByLabelJobSchema.PK, SQLComparisonOperator.NotEqual, pickByLabelJob.PK);
			var newPickByLabelJob = Factory.Load<WhsPickByLabelJob>(pickByLabelQuery).Single();
			AssertEquals("Warehouse", data.Whs1.PK, newPickByLabelJob.WTK_WW_Warehouse);
			AssertEquals("DockDoor", data.Whs1.WW_DefaultOutboundDockDoor, newPickByLabelJob.WTK_WL_DockDoor);
			AssertEquals("Assigned To", GlbStaff.CurrentUser.GS_Code, newPickByLabelJob.WTK_GS_NKAssignedTo);
			AssertEquals("FinalisedDate should be empty", true, newPickByLabelJob.WTK_FinalisedDate.IsEmpty);
			AssertEquals("Package2 should be assigned to new job", package2.PK, newPickByLabelJob.Labels.Cast<WhsPickByLabelLabel>().Single().WTL_KP_Package);
			pickByLabelPickStrategyMock.VerifyAll();
		}

		public void TestOnFinalised_MultipleJobsLinkToSameFinalisedPick()
		{
			var staff1 = Helper.CreateGlbStaff("AAA", "AAA");
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 2m);
			var orderLine2 = Helper.CreateWhsOrderLine(order1, data.Part1, 3m);
			var pick1 = Helper.CreatePickNew(order1);
			var package1 = order1.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			var package2 = order1.PackageJob.Packages.AddNew("PLT", "PACKAGE-2");
			var releaseLine1 = orderLine1.ReleaseLines[0];
			package1.Pack(releaseLine1, 2m);
			var releaseLine2 = orderLine2.ReleaseLines[0];
			package2.Pack(releaseLine2, 3m);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var pick2 = Helper.CreatePickNew(order2);
			var package3 = order2.PackageJob.Packages.AddNew("PLT", "PACKAGE-3");
			var releaseLine = order2.Lines[0].ReleaseLines[0];
			package3.Pack(releaseLine, 5m);
			Factory.Save();

			var pickByLabelJob1 = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package1.PK);
			var pickByLabelJob2 = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, staff1.GS_Code, package2.PK);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, staff1.GS_Code, package3.PK);
			Factory.Save();

			var pickByLabelPickStrategyMock = new Mock<IWhsPickByLabelPickStrategy>();
			pickByLabelPickStrategyMock.Setup(m => m.OnFinalised(It.IsAny<BusinessObjectFactory>(), It.IsAny<IWhsPick>()))
				.Callback(() => { });
			using (ObjectFactory.Substitute(pickByLabelPickStrategyMock.Object))
			{
				pick1.FinaliseAllOrders();
				pick1.FinalisePick();
			}

			AssertIsFinalisedPrecondition(pick1);

			var pickByLabelStrategy = new WhsPickByLabelPickStrategy();
			pickByLabelStrategy.OnFinalised(Factory, pick1);

			AssertEquals("PickByLabelJob should be finalised", false, pickByLabelJob1.WTK_FinalisedDate.IsEmpty);
			AssertEquals("PickByLabelJob should be finalised", false, pickByLabelJob2.WTK_FinalisedDate.IsEmpty);

			var pickByLabelQuery = new ZQuery(WhsPickByLabelJobSchema.PK, SQLComparisonOperator.NotEqual, new[] { pickByLabelJob1.PK, pickByLabelJob2.PK });
			var newPickByLabelJob = Factory.Load<WhsPickByLabelJob>(pickByLabelQuery).Single();
			AssertEquals("Warehouse", data.Whs1.PK, newPickByLabelJob.WTK_WW_Warehouse);
			AssertEquals("DockDoor", data.Whs1.WW_DefaultOutboundDockDoor, newPickByLabelJob.WTK_WL_DockDoor);
			AssertEquals("Assigned To", staff1.GS_Code, newPickByLabelJob.WTK_GS_NKAssignedTo);
			AssertEquals("FinalisedDate should be empty", true, newPickByLabelJob.WTK_FinalisedDate.IsEmpty);
			AssertEquals("Package3 should be assigned to new job", package3.PK, newPickByLabelJob.Labels.Cast<WhsPickByLabelLabel>().Single().WTL_KP_Package);
			pickByLabelPickStrategyMock.VerifyAll();
		}

		#endregion

		#region CanDetachOrder

		public void TestCanDetachOrder_OrderHasActivePickByLabelJob()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			AssertEquals("Precondition", 1, pick.Orders.Count);
			AssertEquals("Precondition: order.HasAnyPackagesAssignedToAPickByLabelJob should be false", false,
				order.HasAnyPackageAssignedToAPickByLabelJob());

			var package = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			var releaseLine = order.Lines[0].ReleaseLines[0];
			package.Pack(releaseLine, 5m);
			Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package.PK);
			Factory.Save();

			AssertEquals("Precondition: order.HasAnyPackagesAssignedToAPickByLabelJob should be true", true,
				order.HasAnyPackageAssignedToAPickByLabelJob());

			var notifications = (NotificationBuffer)pick.NotificationSubscriber;
			var strategy = new WhsPickByLabelPickStrategy();
			AssertEquals("PickByLabelPickStrategy should not allow the order to be detached", false, strategy.IsOrderActionAllowed(order, PickOrderAction.DetachOrder, out var reasonNotAllowed));
			AssertNotNull("reasonNotAllowed should not be null", reasonNotAllowed);
			AssertEquals("reasonNotAllowed should contain expected error message",
				"Error: Cannot perform this operation because the order has packages assigned to a pick by label job.",
				reasonNotAllowed.Message);
		}

		public void TestCanDetachOrder_OrderHasNoPickByLabelJob()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			AssertEquals("Precondition", 1, pick.Orders.Count);
			AssertEquals("Precondition: order.HasAnyPackagesAssignedToAPickByLabelJob should be false", false,
				order.HasAnyPackageAssignedToAPickByLabelJob());

			var notifications = (NotificationBuffer)pick.NotificationSubscriber;
			var strategy = new WhsPickByLabelPickStrategy();
			AssertEquals("PickByLabelPickStrategy should allow the order to be detached", true, strategy.IsOrderActionAllowed(order, PickOrderAction.DetachOrder, out var reasonNotAllowed));
			AssertNull("reasonNotAllowed should be null", reasonNotAllowed);
		}

		#endregion

		#region TestCancelPick_OrderHasActivePickByLabelJob

		public void TestCancelPick_OrderHasActivePickByLabelJob()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			AssertEquals("Precondition", 1, pick.Orders.Count);
			AssertEquals("Precondition: order.HasAnyPackagesAssignedToAPickByLabelJob should be false", false,
				order.HasAnyPackageAssignedToAPickByLabelJob());

			var package = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			var releaseLine = order.Lines[0].ReleaseLines[0];
			package.Pack(releaseLine, 5m);
			Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package.PK);
			Factory.Save();

			AssertEquals("Precondition: order.HasAnyPackagesAssignedToAPickByLabelJob should be true", true,
				order.HasAnyPackageAssignedToAPickByLabelJob());

			AssertNoExceptionThrown("No exception is thrown.", () => pick.CancelPick());
			AssertEquals("Pick is not cancelled.", false, pick.IsCancelled);
		}

		#endregion
	}
}

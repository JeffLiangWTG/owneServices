using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Transactions.PickByLabel;
using Enterprise.Warehouse.Transactions.TrolleyPicking;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(CancelPickApplicator))]
	public class CancelPickApplicatorTest : OperationalActionMethodApplicatorTest
	{
		#region TestConstructor

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CancelPickApplicator(null));
		}

		#endregion

		#region TestCancelPick_ShouldNotBeAbleToCancelPickedPick

		public void TestCancelPick_ShouldNotBeAbleToCancelPickedPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 15m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			Factory.Save();

			var expectedLog = string.Format(@"WARNING: Pick {0} [HL {0}] - Cannot perform this operation because the Order is partially or fully picked.", pick.WP_PickNo);
			ApplyApplicator(new[] { pick }, expectedLog, true);

			AssertEquals("Should sussesfuly save.", pick.PK, NewFactory().Load<WhsOrder>(order.PK).Pick.PK);
		}

		#endregion

		#region TestCancelPick_ShouldBeAbleToCancelPicked

		public void TestCancelPick_ShouldBeAbleToCancelPicked()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 15m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			ApplyApplicator(new[] { pick }, string.Format(@"INFO: Pick {0} [HL {0}] - was successfully canceled.", pick.WP_PickNo), true);
			AssertEquals("Should sussesfuly save.", null, NewFactory().Load<WhsOrder>(order.PK).Pick);
		}

		#endregion

		#region TestCancelPick_SaveCancelPickIfCannotCancelOtherPicks

		public void TestCancelPick_SaveCancelPickIfCannotCancelOtherPicks()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);
			Factory.Save();

			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);
			var pickLine = pick1.GetAllPickLines().Single();
			Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			Factory.Save();

			var picks = new[] { pick1, pick2 };
			var expectedLog = string.Format(@"WARNING: Pick {0} [HL {0}] - Cannot perform this operation because the Order is partially or fully picked.
INFO: Pick {1} [HL {1}] - was successfully canceled.", pick1.WP_PickNo, pick2.WP_PickNo);
			ApplyApplicator(picks, expectedLog, true);
			AssertEquals("Should sussesfuly save.", pick1.PK, NewFactory().Load<WhsOrder>(order1.PK).Pick.PK);
			AssertEquals("Should sussesfuly save.", null, NewFactory().Load<WhsOrder>(order2.PK).Pick);
		}

		#endregion

		#region TestCancelPick_ShouldNotBeAbleToCancelPickHasPackageLabels

		public void TestCancelPick_ShouldNotBeAbleToCancelPickHasPackageLabels()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			Factory.LoadTop1<RefPackType>(new ZQuery(RefPackTypeSchema.F3_Code, Constants.PkgUnit.Box)).F3_UOMType = UOMPackTypesList.Codes.Case;

			data.Part1.PartUnits.DeleteAll();
			Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, Constants.PkgUnit.Box, 5m);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 15m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_PickCasesByLabel = true;
			pick.AutoAllocateItems();
			Factory.Save();

			pick.AllocatePackageLabels();
			Factory.Save();
			AssertEquals("Precondition - Cartonised", true, pick.WP_IsCartonised);

			var expectedLog = string.Format(@"WARNING: Pick {0} [HL {0}] - Pick cannot be canceled as the Pick has already had Package Labels allocated, or is in the process of having Package Labels allocated in another instance.", pick.WP_PickNo);
			ApplyApplicator(new[] { pick }, expectedLog);
		}

		#endregion

		#region TestCancelPick_BondedWarehouse

		public void TestCancelPick_BondedWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var bondedArea = data.Whs1.Areas.Single(a => a.WA_AreaType == "BON");
			data.Whs1.DefaultLocation.WLV_WA_PickingArea = bondedArea.PK;
			Factory.Save();

			var bondedInwardsKey = "ABC123";
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, bondedInwardsKey, false, false);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 5m, bondedInwardsKey, "DummyOutwards");
			order.WD_DocketSubType = OrderType.Codes.Customs;

			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var expectedLog = string.Format(@"INFO: Pick {0} [HL {0}] - was successfully canceled.", pick.WP_PickNo);
			ApplyApplicator(new[] { pick }, expectedLog, true);
			AssertEquals("Should sussesfuly save.", null, NewFactory().Load<WhsOrder>(order.PK).Pick);
		}

		#endregion

		#region TestCancelPick_WorkOrder

		public void TestCancelPick_WorkOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part1, data.Part2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 2m);
			Factory.Save();
			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, data.Part1, 1m);
			var pick = Helper.CreatePickNew(workOrder);
			Factory.Save();

			var expectedLog = string.Format(@"INFO: Pick {0} [HL {0}] - was successfully canceled.", pick.WP_PickNo);
			ApplyApplicator(new[] { pick }, expectedLog, true);
			AssertEquals("Should sussesfuly save.", null, NewFactory().Load<WhsWorkOrder>(workOrder.PK).Pick);
		}

		#endregion

		#region TestCancelPick_ValidToCancel_ButHavingErrorDuringCancelPick

		public void TestCancelPick_ValidToCancel_ButHavingErrorDuringCancelPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var newFactory = new BusinessObjectFactory { NameForDebugging = "Factory from Service Mock for tesing" };
			var factoryServiceMock = new Mock<IFactoryService>();
			factoryServiceMock.Setup(fs => fs.GetFactory<Func<BusinessObjectFactory>>())
				.Returns(() => newFactory);
			var pickInNewFactory = newFactory.Load<WhsPick>(pick.PK);
			pickInNewFactory.AddRowError("Test");

			var expectedLog = string.Format(@"WARNING: Pick {0} [HL {0}] - had the following errors when trying to cancel Pick:
Error - Pick {0}: Test", pick.WP_PickNo);
			using (ObjectFactory.Substitute(factoryServiceMock.Object))
			{
				ApplyApplicator(new[] { pick }, expectedLog, true);
			}
		}

		#endregion

		#region TestCancelPick_WhenIsCartonising

		public void TestCancelPick_WhenIsCartonising()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var cartonisationMutex = new ZGlobalMutex(MutexIDs.WhsPickAllocatingPackageLabels, pick.PK.ToString());
			cartonisationMutex.Lock();

			using (cartonisationMutex)
			{
				var expectedLog = string.Format(@"WARNING: Pick {0} [HL {0}] - Pick cannot be canceled as the Pick has already had Package Labels allocated, or is in the process of having Package Labels allocated in another instance.", pick.WP_PickNo);
				ApplyApplicator(new[] { pick }, expectedLog, true);
			}
		}

		#endregion

		#region TestCancelPick_ConcurrencyException

		public void TestCancelPick_ConcurrencyException()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			void someoneChangePickDuringOperation(BusinessObjectFactory factory)
			{
				var otherUserFactory = new BusinessObjectFactory { RefreshEnabled = false };
				var helperWithNewFactory = new WhsTestHelperFunctions(otherUserFactory);
				var newOrder = helperWithNewFactory.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
				pick.Orders.Add(newOrder);
				otherUserFactory.Save();
				AssertEquals("pick has been updated in new factory", 2, NewFactory().Load<WhsPick>(pick.PK).Orders.Count);
			}

			var newFactory = new BusinessObjectFactory { NameForDebugging = "Factory from Service Mock for tesing" };
			var factoryServiceMock = new Mock<IFactoryService>();
			factoryServiceMock.Setup(fs => fs.GetFactory<Func<BusinessObjectFactory>>())
				.Returns(() => newFactory);
			newFactory.Saving += someoneChangePickDuringOperation;

			var expectedLog = string.Format(@"WARNING: Pick P00000001 [HL P00000001] - While you were editing your data, another user modified it. Your changes cannot be saved because they may conflict with the other user's changes. Please close and open this form to try again.", pick.WP_PickNo, true);
			using (ObjectFactory.Substitute(factoryServiceMock.Object))
			{
				ApplyApplicator(new[] { pick }, expectedLog, true);
			}
		}

		#endregion

		#region TestCancelPick_SaveExceptionError

		public void TestCancelPick_SaveExceptionError()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var newFactory = new BusinessObjectFactory { NameForDebugging = "Factory from Service Mock for tesing" };
			var factoryServiceMock = new Mock<IFactoryService>();
			factoryServiceMock.Setup(fs => fs.GetFactory<Func<BusinessObjectFactory>>())
				.Returns(() => newFactory);
			var pickInNewFactory = newFactory.Load<WhsPick>(pick.PK);
			pickInNewFactory.Orders[0].WD_WW_Whs = ZGuid.NewZGuid();// just change somthing to get save exception

			using (ObjectFactory.Substitute(factoryServiceMock.Object))
			{
				var ex = AssertExceptionThrown<ZSaveException>(() => ApplyApplicator(new[] { pick }, "", true));
				AssertEquals("The WhsDocket cannot be inserted/updated, requires a reference to a valid WhsWarehouse.", ex.FriendlyMessage);
			}
		}

		#endregion

		#region TestCancelPick_SaveExceptionError

		public void TestCancelPick_CannotSaveExceptionError()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var newFactory = new BusinessObjectFactory { NameForDebugging = "Factory from Service Mock for tesing" };
			var factoryServiceMock = new Mock<IFactoryService>();
			factoryServiceMock.Setup(fs => fs.GetFactory<Func<BusinessObjectFactory>>())
				.Returns(() => newFactory);
			newFactory.Saving += f => throw new ZCannotSaveException("You shall not SAVE!", "Test");

			var expectedLog = string.Format(@"WARNING: Pick {0} [HL {0}] - You shall not SAVE!", pick.WP_PickNo, true);
			using (ObjectFactory.Substitute(factoryServiceMock.Object))
			{
				ApplyApplicator(new[] { pick }, expectedLog, true);
			}
		}

		#endregion

		#region TestCancelPick_CartonisingWhileCancelling

		public void TestCancelPick_CartonisingWhileCancelling()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var cartonisationMutex = new ZGlobalMutex(MutexIDs.WhsPickAllocatingPackageLabels, pick.PK.ToString());
			var newFactory = SetupPickCartonisingOnNewFactoryAndReturnNewFactory(pick, cartonisationMutex);

			var factoryServiceMock = new Mock<IFactoryService>();
			factoryServiceMock.Setup(fs => fs.GetFactory<Func<BusinessObjectFactory>>())
				.Returns(() => newFactory);

			using (cartonisationMutex)
			using (ObjectFactory.Substitute(factoryServiceMock.Object))
			{
				AssertNoExceptionThrown("No exception is thrown.", () => ApplyApplicator(new[] { pick }, string.Format(@"WARNING: Pick {0} [HL {0}] - could not be canceled. try to cancel manually.", pick.WP_PickNo), true));
			}
		}

		#endregion

		#region TestCancelPick_CartonisingWhileCancelling_MultiplePicks

		public void TestCancelPick_CartonisingWhileCancelling_MultiplePicks()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick1 = Helper.CreatePickNew(order1);
			pick1.WP_PickNo = "P1";
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 5m);
			var pick2 = Helper.CreatePickNew(order2);
			pick2.WP_PickNo = "P2";
			Factory.Save();

			var cartonisationMutex = new ZGlobalMutex(MutexIDs.WhsPickAllocatingPackageLabels, pick1.PK.ToString());
			var newFactory1 = SetupPickCartonisingOnNewFactoryAndReturnNewFactory(pick1, cartonisationMutex);
			var newFactory2 = SetupPickCartonisingOnNewFactoryAndReturnNewFactory(pick1, cartonisationMutex);

			var i = 0;
			var factoryServiceMock = new Mock<IFactoryService>();
			factoryServiceMock.Setup(fs => fs.GetFactory<Func<BusinessObjectFactory>>())
				.Returns(() =>
				{
					if (i == 0)
					{
						i++;
						return newFactory1;
					}
					else
					{
						return newFactory2;
					}
				});

			var expectedLogTexts = new[]
			{
				string.Format("WARNING: Pick {0} [HL {0}] - could not be canceled. try to cancel manually.", pick1.WP_PickNo),
				string.Format("INFO: Pick {0} [HL {0}] - was successfully canceled.", pick2.WP_PickNo)
			};

			using (cartonisationMutex)
			using (ObjectFactory.Substitute(factoryServiceMock.Object))
			{
				AssertNoExceptionThrown("No exception is thrown.",
					() => ApplyApplicatorLogOrderIsUnimportantIgnoreString(new[] { pick1, pick2 },
					expectedLogTexts,
					"\n"));
			}
		}

		static BusinessObjectFactory SetupPickCartonisingOnNewFactoryAndReturnNewFactory(WhsPick pick, ZGlobalMutex cartonisationMutex)
		{
			var newFactory = new BusinessObjectFactory { NameForDebugging = "Factory from Service Mock for tesing" };
			var pickInNewFactory = newFactory.Load<WhsPick>(pick.PK);
			pickInNewFactory.OrderedInventories[0].AvailableInventories.CountChanged += (s, e) =>
			{
				cartonisationMutex.Lock();
				Assert(cartonisationMutex.IsLocked);
			};

			return newFactory;
		}

		#endregion

		#region TestCancelPick_OrderHasActivePickByLabelJob

		public void TestCancelPick_OrderHasActivePickByLabelJob()
		{
			var data = new TestDataSimpleEnvironment(Factory);
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

			var expectedLog = string.Format(@"WARNING: Pick {0} [HL {0}] - You cannot cancel pick because this pick has a package with an active Pick By Label job.", pick.WP_PickNo);
			AssertNoExceptionThrown("No exception is thrown.", () => ApplyApplicator(new[] { pick }, expectedLog, true));
			AssertEquals("Pick is not cancelled.", pick.PK, NewFactory().Load<WhsOrder>(order.PK).Pick.PK);
		}

		#endregion

		#region TestCancelPick_OrderHasActiveTrolleyJob

		public void TestCancelPick_OrderHasActiveTrolleyJob()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var strategy = new WhsPickByTrolleyPickStrategy();
			AssertEquals("Precondition", 1, pick.Orders.Count);
			AssertEquals("Precondition: order.HasAnyPackagesAssignedToATrolleyJob should be false", false,
				strategy.HasAnyPackageAssignedToATrolleyJob(order));

			var trolley = Helper.CreateTrolley("TR1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley.PK, "BLD");

			var package = order.PackageJob.Packages.AddNew();
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package.PK, 1);
			Factory.Save();

			AssertEquals("Precondition: order.HasAnyPackagesAssignedToATrolleyJob should be true", true,
				strategy.HasAnyPackageAssignedToATrolleyJob(order));

			var expectedLog = string.Format(@"WARNING: Pick {0} [HL {0}] - You cannot cancel pick because this pick has a package with an active Trolley job or was picked by Trolley.", pick.WP_PickNo);
			AssertNoExceptionThrown("No exception is thrown.", () => ApplyApplicator(new[] { pick }, expectedLog, true));
			AssertEquals("Pick is not cancelled.", pick.PK, NewFactory().Load<WhsOrder>(order.PK).Pick.PK);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var factoryService = ObjectFactory.New<IFactoryService>();
			factoryService.RegisterFactory(() => new BusinessObjectFactory());

			return new CancelPickApplicator(factoryService);
		}

		protected WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		#endregion
	}
}

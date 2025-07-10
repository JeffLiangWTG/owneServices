namespace Enterprise.Warehouse.Transactions.TrolleyPicking.Testing
{
	using Business;
	using Business.Testing;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Integration;
	using NUnit.Framework;
	using Packing.Business;

	[TestedType(typeof(WhsPickTrolleySlot))]
	internal class WhsPickTrolleySlotTest : WhsBusinessObjectTestCase
	{
		#region Related Business Objects

		#region TestTrolleyJob

		public void TestTrolleyJob()
		{
			var slot = Factory.New<WhsPickTrolleySlot>();
			AssertNull("Precondition", slot.TrolleyJob);

			var trolleyJob = Factory.New<WhsPickTrolleyJob>();
			slot.WTS_WTJ_TrolleyJob = trolleyJob.PK;
			AssertEquals(trolleyJob, slot.TrolleyJob);
		}

		#endregion

		#region TestPackage

		public void TestPackage()
		{
			var slot = Factory.New<WhsPickTrolleySlot>();
			AssertNull("Precondition", slot.TrolleyJob);

			var package = Factory.New<PkgPackage>();
			slot.WTS_KP_Package = package.PK;
			AssertEquals(package, slot.Package);
		}

		#endregion

		#region TestDeleteTote

		public void TestDeleteTote()
		{
			var emptySlot = Factory.New<WhsPickTrolleySlot>();
			emptySlot.Delete();
			AssertEquals("The slot should have been deleted.", true, emptySlot.IsDeleted);

			var slotWithCarton = Factory.New<WhsPickTrolleySlot>();
			var carton = Factory.New<PkgPackage>();
			slotWithCarton.WTS_KP_Package = carton.PK;
			slotWithCarton.Delete();
			AssertEquals("The slot with carton should have been deleted.", true, slotWithCarton.IsDeleted);
			AssertEquals("The carton should NOT have been deleted.", false, carton.IsDeleted);

			var slotWithTote = Factory.New<WhsPickTrolleySlot>();
			var tote = Factory.New<PkgPackage>();
			slotWithTote.WTS_KP_Package = tote.PK;
			tote.SetIsTote(true);
			slotWithTote.Delete();
			AssertEquals("The slot with tote should have been deleted.", true, slotWithTote.IsDeleted);
			AssertEquals("The tote should have been deleted.", true, tote.IsDeleted);
		}

		public void TestDeleteToteAndSlot_FromOrder_NoExceptionThrown()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order1", data.Part1, 5m);
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var tote = packageJob.Packages.AddNew();
			tote.SetIsTote(true);

			var trolleyJob = Factory.New<WhsPickTrolleyJob>();
			trolleyJob.WTJ_RQ_Equipment = Helper.CreateTrolley("T1").PK;
			var newSlot = trolleyJob.Slots.AddNew();
			newSlot.WTS_SlotNumber = 1;
			newSlot.WTS_KP_Package = tote.PK;

			AssertNoExceptionThrown(() => tote.Delete());
		}

		#endregion

		#region TestOnSaving

		public void TestOnSaving_BumpCriticalChangesVersionIDForTrolleyJob()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order1", data.Part1, 5m);
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var tote = packageJob.Packages.AddNew();
			tote.SetIsTote(true);

			var trolleyJob = Factory.New<WhsPickTrolleyJob>();
			trolleyJob.WTJ_RQ_Equipment = Helper.CreateTrolley("T1").PK;
			Factory.Save();
			AssertEquals("Precondition - WTJ_CriticalChangesVersionID should be Empty", ZGuid.Empty, trolleyJob.WTJ_CriticalChangesVersionID);
			var newSlot = trolleyJob.Slots.AddNew();
			newSlot.WTS_SlotNumber = 1;
			newSlot.WTS_KP_Package = tote.PK;
			Factory.Save();
			AssertNotEquals("Should bump WTJ_CriticalChangesVersionID when slot is added to the trolley", ZGuid.Empty, trolleyJob.WTJ_CriticalChangesVersionID);
			trolleyJob.WTJ_Status = PickTrolleyStatus.Codes.Finalised;
			Factory.Save();
			AssertEquals(PickTrolleyStatus.Codes.Finalised, trolleyJob.WTJ_Status);
			AssertEquals("Changing status to finalised should clear WTJ_CriticalChangesVersionID.", ZGuid.Empty, trolleyJob.WTJ_CriticalChangesVersionID);
		}

		#endregion

		#region IWhsPickTrolleySlot Members

		public void TestIWhsPickTrolleySlot_PK()
		{
			var trolleySlot = Factory.New<WhsPickTrolleySlot>();
			AssertEquals("Precondition", trolleySlot.PK, ((IWhsPickTrolleySlot)trolleySlot).PK);
		}

		public void TestIWhsPickTrolleySlot_WTS_KP_Package()
		{
			var trolleySlot = Factory.New<WhsPickTrolleySlot>();
			trolleySlot.WTS_KP_Package = Factory.New<PkgPackage>().PK;
			AssertEquals("Precondition", trolleySlot.WTS_KP_Package, ((IWhsPickTrolleySlot)trolleySlot).WTS_KP_Package);
		}

		public void TestIWhsPickTrolleySlot_WTS_KP_Package_ChangeClearsPackageActionStrategy()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order1", data.Part1, 5m);
			Helper.CreatePickNew(order);
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packageJob.Packages.AddNew();
			package.Pack(order.Lines[0].ReleaseLines[0], 5m);

			var trolleyJob = Factory.New<WhsPickTrolleyJob>();
			trolleyJob.WTJ_RQ_Equipment = Helper.CreateTrolley("T1").PK;
			var newSlot = trolleyJob.Slots.AddNew();
			newSlot.WTS_SlotNumber = 1;
			AssertEquals("Precondition: Package is unassigned should be unpackable", true, package.IsAvailableForUnpacking(out _));

			newSlot.WTS_KP_Package = package.PK;
			AssertEquals("Package is assigned to a trolley and should not be unpackable", false, package.IsAvailableForUnpacking(out _));
		}

		public void TestIWhsPickTrolleySlot_WTS_SlotNumber()
		{
			var trolleySlot = Factory.New<WhsPickTrolleySlot>();
			trolleySlot.WTS_SlotNumber = 1;
			AssertEquals("Precondition", trolleySlot.WTS_SlotNumber, ((IWhsPickTrolleySlot)trolleySlot).WTS_SlotNumber);
		}

		public void TestIWhsPickTrolleySlot_WTS_WTJ_TrolleyJob()
		{
			var trolleySlot = Factory.New<WhsPickTrolleySlot>();
			trolleySlot.WTS_WTJ_TrolleyJob = Factory.New<WhsPickTrolleyJob>().PK;
			AssertEquals("Precondition", trolleySlot.WTS_WTJ_TrolleyJob, ((IWhsPickTrolleySlot)trolleySlot).WTS_WTJ_TrolleyJob);
		}

		public void TestIWhsPickTrolleySlot_WTS_WTJ_TrolleyJob_ChangeClearsPackageActionStrategy()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order1", data.Part1, 5m);
			Helper.CreatePickNew(order);
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packageJob.Packages.AddNew();
			package.Pack(order.Lines[0].ReleaseLines[0], 5m);

			var trolleyJob = Factory.New<WhsPickTrolleyJob>();
			trolleyJob.WTJ_RQ_Equipment = Helper.CreateTrolley("T1").PK;
			var newSlot = Factory.New<WhsPickTrolleySlot>();
			newSlot.WTS_SlotNumber = 1;
			newSlot.WTS_KP_Package = package.PK;
			newSlot.WTS_WTJ_TrolleyJob = ZGuid.Empty;
			AssertEquals("Precondition: Package is NOT assigned to a trolley and should be unpackable", true, package.IsAvailableForUnpacking(out _));

			newSlot.WTS_WTJ_TrolleyJob = trolleyJob.PK;
			AssertEquals("Package is assigned to a trolley and should NOT be unpackable", false, package.IsAvailableForUnpacking(out _));
		}

		#endregion

		// in case PickLines property required later.
		//#region TestPickLines

		//public void TestPickLines_GrabAllLinesWithSingleDBHit()
		//{
		//	var numberOfRecordsToCreate = 10;

		//	var packingHelper = new PackingTestHelper(Factory);
		//	var data = new TestDataSimpleEnvironment(Factory);
		//	var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
		//	var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 100m);
		//	Factory.Save();

		//	var trolley = Helper.CreateTrolley("T001");
		//	var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley);
		//	for (int i = 0; i < numberOfRecordsToCreate; i++)
		//	{
		//		var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O" + i);
		//		var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, i + 1);
		//		var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, i + 1);

		//		var pick = Helper.CreatePickNew(order);
		//		AssertEquals("Precondition", 2m * (i + 1), pick.TotalPickLineQuantity);

		//		var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
		//		var pkgPackage = packingHelper.CreatePackage(pkgJob, "PKG" + i, 1, Constants.PkgUnit.Box);
		//		var divot1 = packingHelper.CreatePackageDivot(pkgPackage, orderLine1.PickLines.Single(), i + 1);
		//		var divot2 = packingHelper.CreatePackageDivot(pkgPackage, orderLine2.PickLines.Single(), i + 1);

		//		var slot = Helper.CreateWhsPickTrolleySlot(trolleyJob, pkgPackage, (short)(i + 1));
		//	}
		//	AssertEquals("Precondition", numberOfRecordsToCreate, trolleyJob.Slots.Count);

		//	Factory.Save();

		//	var otherFactory = new BusinessObjectFactory();
		//	var trolleyJobInOtherFactory = otherFactory.Load<WhsPickTrolleyJob>(trolleyJob.PK);
		//	foreach(var slot in trolleyJobInOtherFactory.Slots)
		//	{
		//		AssertEquals("Each slot should have 2 pick lines associated with it.", 2, slot.PickLines.Count());
		//	}

		//	var expectedDBHits = new Dictionary<string, int>();
		//	expectedDBHits.Add(WhsPickTrolleyJobSchema.Constants.TableName, 1);
		//	expectedDBHits.Add(WhsPickTrolleySlotSchema.Constants.TableName, 1);
		//	expectedDBHits.Add(PkgPackageSchema.Constants.TableName, 1);
		//	expectedDBHits.Add(PkgPackageItemDivotSchema.Constants.TableName, 1);
		//	expectedDBHits.Add(WhsPickLineSchema.Constants.TableName, 1);

		//	AssertDbHits(expectedDBHits, otherFactory);
		//}

		//#endregion

		#endregion

		#region Implementations

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var trolleySlot = factory.NewWithValidTestData<WhsPickTrolleySlot>();
			trolleySlot.WTS_SlotNumber = 1;

			return trolleySlot;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var trolleySlot = Factory.NewWithValidTestData<WhsPickTrolleySlot>();
			trolleySlot.WTS_SlotNumber = 1;

			return trolleySlot;
		}

		#endregion
	}
}


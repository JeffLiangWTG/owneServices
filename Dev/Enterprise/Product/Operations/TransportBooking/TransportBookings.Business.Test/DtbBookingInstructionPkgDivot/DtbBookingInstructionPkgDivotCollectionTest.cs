using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.TransportCommon.Shared;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Business.Testing
{
	[TestedType(typeof(DtbBookingInstructionPkgDivotCollection))]
	public class DtbBookingInstructionPkgDivotCollection_BookingTest : ActiveBusinessObjectCollectionTestCase<DtbBookingInstructionPkgDivotCollection>
	{
		public void TestAllowNew()
		{
			AssertEquals(false, ((IBindingList)GetCollectionToTest()).AllowNew);
		}

		public void TestAddRemove()
		{
			var booking = Helper.CreateBooking();
			var packageDivots = new DtbBookingInstructionPkgDivotCollection(booking);
			AssertEquals("Precondition", 0, packageDivots.Count);

			var instruction = booking.Instructions.AddNew();
			AssertEquals("Precondition", 0, packageDivots.Count);

			var packageDivot = instruction.PackageDivots.AddNew();
			AssertContainsExactElementsInAnyOrder("Added", new DtbBookingInstructionPkgDivot[] { packageDivot }, packageDivots);

			var package = Factory.New<PkgPackage>();
			packageDivot.KD_KP_Package = package.PK;
			AssertContainsExactElementsInAnyOrder("Added", new DtbBookingInstructionPkgDivot[] { packageDivot }, packageDivots);

			var instruction2 = booking.Instructions.AddNew();
			var packageDivot2 = instruction2.PackageDivots.AddNew();
			AssertContainsExactElementsInAnyOrder("Added", new DtbBookingInstructionPkgDivot[] { packageDivot, packageDivot2 }, packageDivots);

			packageDivot2.KD_KN_BookingInstruction = ZGuid.Empty;
			AssertContainsExactElementsInAnyOrder("Removed", new DtbBookingInstructionPkgDivot[] { packageDivot }, packageDivots);
		}

		public void TestCollectionOnSubBookingHasMasterInstructionDivotsOnPackagesForSubInstruction()
		{
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			Factory.Save();

			var subBooking1 = Helper.CreateBooking();
			var subBooking1LoosePackage = subBooking1.PackageJob.Packages.AddNew("PLT", "LOOSE1");
			var subBooking1Container = helper.CreatePackageContainer("CONT1");
			var subBooking1ContainerInnerLoosePackage = helper.CreatePackage("CONT1_IN");
			subBooking1.PackageJob.Packages.Add(subBooking1Container);
			subBooking1Container.Packages.Add(subBooking1ContainerInnerLoosePackage);

			var subBooking2 = Helper.CreateBooking();
			var subBooking2LoosePackage = subBooking2.PackageJob.Packages.AddNew("PLT", "LOOSE2");
			var subBooking2Container = helper.CreatePackageContainer("CONT2");
			var subBooking2ContainerInnerLoosePackage = helper.CreatePackage("CONT2_IN");
			subBooking2.PackageJob.Packages.Add(subBooking2Container);
			subBooking2Container.Packages.Add(subBooking2ContainerInnerLoosePackage);

			masterBooking.SubBookings.AddRange(new[] { subBooking1, subBooking2 });
			Factory.Save();

			var masterInstruction1 = masterBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var masterInstruction2 = masterBooking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			var subBooking1Instruction1 = subBooking1.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			subBooking1Instruction1.KN_KN_MasterBookingInstruction = masterInstruction1.PK;
			var subBooking1Instruction2 = subBooking1.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			subBooking1Instruction2.KN_KN_MasterBookingInstruction = masterInstruction2.PK;
			var subBooking2Instruction1 = subBooking2.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			subBooking2Instruction1.KN_KN_MasterBookingInstruction = masterInstruction1.PK;
			var subBooking2Instruction2 = subBooking2.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			subBooking2Instruction2.KN_KN_MasterBookingInstruction = masterInstruction2.PK;

			var divotInstruction1ToLoosePackage1 = Helper.CreatePackageDivot(masterInstruction1, subBooking1LoosePackage);
			var divotInstruction1ToContainer1 = Helper.CreatePackageDivot(masterInstruction1, subBooking1Container);
			var divotInstruction1ToContainerInnerPackage1 = Helper.CreatePackageDivot(masterInstruction1, subBooking1ContainerInnerLoosePackage);
			var divotInstruction1ToLoosePackage2 = Helper.CreatePackageDivot(masterInstruction1, subBooking2LoosePackage);
			var divotInstruction1ToContainer2 = Helper.CreatePackageDivot(masterInstruction1, subBooking2Container);
			var divotInstruction1ToContainerInnerPackage2 = Helper.CreatePackageDivot(masterInstruction1, subBooking2ContainerInnerLoosePackage);

			var divotInstruction2ToLoosePackage1 = Helper.CreatePackageDivot(masterInstruction2, subBooking1LoosePackage);
			var divotInstruction2ToContainer1 = Helper.CreatePackageDivot(masterInstruction2, subBooking1Container);
			var divotInstruction2ToContainerInnerPackage1 = Helper.CreatePackageDivot(masterInstruction2, subBooking1ContainerInnerLoosePackage);
			var divotInstruction2ToLoosePackage2 = Helper.CreatePackageDivot(masterInstruction2, subBooking2LoosePackage);
			var divotInstruction2ToContainer2 = Helper.CreatePackageDivot(masterInstruction2, subBooking2Container);
			var divotInstruction2ToContainerInnerPackage2 = Helper.CreatePackageDivot(masterInstruction2, subBooking2ContainerInnerLoosePackage);

			Factory.Save();

			var divotCollectionForSubBooking1 = new DtbBookingInstructionPkgDivotCollection(subBooking1);
			var divotCollectionForSubBooking2 = new DtbBookingInstructionPkgDivotCollection(subBooking2);

			var divotsForSubBooking1 = divotCollectionForSubBooking1.Select(d => (instructionPK: d.Instruction.PK, packagePK: d.Package.PK)).ToArray();
			var expectedDivotsForSubBooking1 = new (ZGuid instructionPK, ZGuid packagePK)[]
			{
				(instructionPK: masterInstruction1.PK, packagePK: subBooking1Container.PK),
				(instructionPK: masterInstruction1.PK, packagePK: subBooking1ContainerInnerLoosePackage.PK),
				(instructionPK: masterInstruction1.PK, packagePK: subBooking1LoosePackage.PK),
				(instructionPK: masterInstruction2.PK, packagePK: subBooking1Container.PK),
				(instructionPK: masterInstruction2.PK, packagePK: subBooking1ContainerInnerLoosePackage.PK),
				(instructionPK: masterInstruction2.PK, packagePK: subBooking1LoosePackage.PK),
			};

			var divotsForSubBooking2 = divotCollectionForSubBooking2.Select(d => (instructionPK: d.Instruction.PK, packagePK: d.Package.PK)).ToArray();
			var expectedDivotsForSubBooking2 = new (ZGuid instructionPK, ZGuid packagePK)[]
			{
				(instructionPK: masterInstruction1.PK, packagePK: subBooking2Container.PK),
				(instructionPK: masterInstruction1.PK, packagePK: subBooking2ContainerInnerLoosePackage.PK),
				(instructionPK: masterInstruction1.PK, packagePK: subBooking2LoosePackage.PK),
				(instructionPK: masterInstruction2.PK, packagePK: subBooking2Container.PK),
				(instructionPK: masterInstruction2.PK, packagePK: subBooking2ContainerInnerLoosePackage.PK),
				(instructionPK: masterInstruction2.PK, packagePK: subBooking2LoosePackage.PK),
			};

			CombineAssertions("Check DtbBookingInstructionPkgDivotCollection for sub-bookings go from masterInstructions to the packages on the subBooking", () =>
			{
				AssertContainsExactElementsInAnyOrder("Divots in DtbBookingInstructionPkgDivotCollection for subBooking1 should be the divots between masterInstruction and subBooking1's outer and inner packages", expectedDivotsForSubBooking1, divotsForSubBooking1);
				AssertContainsExactElementsInAnyOrder("Divots in DtbBookingInstructionPkgDivotCollection for subBooking2 should be the divots between masterInstruction and subBooking2's outer and inner packages", expectedDivotsForSubBooking2, divotsForSubBooking2);
			});
		}

		public void TestAddingDivotsToSubBookingThrowsException()
		{
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			Factory.Save();

			var subBooking = Helper.CreateBooking();
			var subBookingPackage = subBooking.PackageJob.Packages.AddNew("PLT", "LOOSE1");

			masterBooking.SubBookings.AddRange(new[] { subBooking });
			Factory.Save();

			var masterInstruction = masterBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var subBookingInstruction = subBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			subBookingInstruction.KN_KN_MasterBookingInstruction = masterInstruction.PK;
			Factory.Save();

			var subBookingDivotCollection = new DtbBookingInstructionPkgDivotCollection(subBooking);
			AssertEquals("Precondition: subBooking has no instruction-package divots", 0, subBookingDivotCollection.Count);

			var newSubDivot = Factory.New<DtbBookingInstructionPkgDivot>();
			newSubDivot.KD_KP_Package = subBookingPackage.PK;
			newSubDivot.KD_Quantity = 1;
			newSubDivot.KD_KN_BookingInstruction = subBookingInstruction.PK;
			AssertEquals("Precondition: subBooking still has no instruction-package divots", 0, subBookingDivotCollection.Count);

			AssertExceptionThrown<InvalidOperationException>("Adding divots to sub Booking should not be allowed", "Adding of new divots not allowed for a sub booking or instruction", () => subBookingDivotCollection.Add(newSubDivot));
			AssertEquals("subBooking still has no instruction-package divots", 0, subBookingDivotCollection.Count);
		}

		public void TestRemovingDivotsFromSubBookingThrowsException()
		{
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			Factory.Save();

			var subBooking = Helper.CreateBooking();
			var subBookingPackage = subBooking.PackageJob.Packages.AddNew("PLT", "LOOSE1");

			masterBooking.SubBookings.AddRange(new[] { subBooking });
			Factory.Save();

			var masterInstruction = masterBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var subBookingInstruction = subBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			subBookingInstruction.KN_KN_MasterBookingInstruction = masterInstruction.PK;
			Factory.Save();

			var masterDivot = Factory.New<DtbBookingInstructionPkgDivot>();
			masterDivot.KD_KP_Package = subBookingPackage.PK;
			masterDivot.KD_Quantity = 1;
			masterDivot.KD_KN_BookingInstruction = masterInstruction.PK;
			Factory.Save();

			var subBookingDivotCollection = new DtbBookingInstructionPkgDivotCollection(subBooking);
			AssertEquals("Precondition: subBooking has 1 instruction-package divots, from master", 1, subBookingDivotCollection.Count);

			AssertExceptionThrown<InvalidOperationException>("Removing divots from sub Booking should not be allowed", "Removing of divots not allowed for a sub booking or instruction", () => subBookingDivotCollection.Delete(masterDivot));
			AssertEquals("subBooking still has 1 instruction-package divot, from master", 1, subBookingDivotCollection.Count);
		}

		protected override DtbBookingInstructionPkgDivotCollection GetCollectionToTest()
		{
			return new DtbBookingInstructionPkgDivotCollection(Booking);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			// Relationship doesn't support AddNew ... So setup packageDivot and return 
			return Instruction.PackageDivots.AddNew();
		}

		DtbBooking Booking
		{
			get { return booking ?? (booking = Helper.CreateBooking()); }
		}

		DtbBookingInstruction Instruction
		{
			get { return instruction ?? (instruction = Booking.Instructions.AddNew()); }
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}

		DtbBooking booking;
		DtbBookingInstruction instruction;
		TransportBookingTestHelper helper;
	}

	[TestedType(typeof(DtbBookingInstructionPkgDivotCollection))]
	public class DtbBookingInstructionPkgDivotCollection_InstructionTest : ActiveBusinessObjectCollectionTestCase<DtbBookingInstructionPkgDivotCollection>
	{
		public void TestAllowNew()
		{
			AssertEquals(false, ((IBindingList)GetCollectionToTest()).AllowNew);
		}

		public void TestAddRemove()
		{
			var booking = Helper.CreateBooking();
			var instruction = booking.Instructions.AddNew();
			var packageDivots = new DtbBookingInstructionPkgDivotCollection(instruction);
			AssertEquals("Precondition", 0, packageDivots.Count);

			var packageDivot = instruction.PackageDivots.AddNew();
			AssertContainsExactElementsInAnyOrder("Added", new DtbBookingInstructionPkgDivot[] { packageDivot }, packageDivots);

			var package = Factory.New<PkgPackage>();
			packageDivot.KD_KP_Package = package.PK;
			AssertContainsExactElementsInAnyOrder("Package set ... no difference", new DtbBookingInstructionPkgDivot[] { packageDivot }, packageDivots);

			var packageDivot2 = instruction.PackageDivots.AddNew();
			AssertContainsExactElementsInAnyOrder("Added", new DtbBookingInstructionPkgDivot[] { packageDivot, packageDivot2 }, packageDivots);

			packageDivot2.KD_KN_BookingInstruction = ZGuid.Empty;
			AssertContainsExactElementsInAnyOrder("Removed", new DtbBookingInstructionPkgDivot[] { packageDivot }, packageDivots);
		}

		public void TestCollectionOnSubInstructionHasMasterInstructionDivotsOnPackagesForSubInstruction()
		{
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			Factory.Save();

			var subBooking1 = Helper.CreateBooking();
			var subBooking1LoosePackage = subBooking1.PackageJob.Packages.AddNew("PLT", "LOOSE1");
			var subBooking1Container = helper.CreatePackageContainer("CONT1");
			var subBooking1ContainerInnerLoosePackage = helper.CreatePackage("CONT1_IN");
			subBooking1.PackageJob.Packages.Add(subBooking1Container);
			subBooking1Container.Packages.Add(subBooking1ContainerInnerLoosePackage);

			var subBooking2 = Helper.CreateBooking();
			var subBooking2LoosePackage = subBooking2.PackageJob.Packages.AddNew("PLT", "LOOSE2");
			var subBooking2Container = helper.CreatePackageContainer("CONT2");
			var subBooking2ContainerInnerLoosePackage = helper.CreatePackage("CONT2_IN");
			subBooking2.PackageJob.Packages.Add(subBooking2Container);
			subBooking2Container.Packages.Add(subBooking2ContainerInnerLoosePackage);

			masterBooking.SubBookings.AddRange(new[] { subBooking1, subBooking2 });
			Factory.Save();

			var masterInstruction = masterBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var subBooking1Instruction = subBooking1.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			subBooking1Instruction.KN_KN_MasterBookingInstruction = masterInstruction.PK;
			var subBooking2Instruction = subBooking2.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			subBooking2Instruction.KN_KN_MasterBookingInstruction = masterInstruction.PK;
			var divotToLoosePackage1 = Helper.CreatePackageDivot(masterInstruction, subBooking1LoosePackage);
			var divotToContainer1 = Helper.CreatePackageDivot(masterInstruction, subBooking1Container);
			var divotToContainerInnerPackage1 = Helper.CreatePackageDivot(masterInstruction, subBooking1ContainerInnerLoosePackage);
			var divotToLoosePackage2 = Helper.CreatePackageDivot(masterInstruction, subBooking2LoosePackage);
			var divotToContainer2 = Helper.CreatePackageDivot(masterInstruction, subBooking2Container);
			var divotToContainerInnerPackage2 = Helper.CreatePackageDivot(masterInstruction, subBooking2ContainerInnerLoosePackage);
			Factory.Save();

			var divotCollectionForSubBooking1Instruction = new DtbBookingInstructionPkgDivotCollection(subBooking1Instruction);
			var divotCollectionForSubBooking2Instruction = new DtbBookingInstructionPkgDivotCollection(subBooking2Instruction);

			var divotsForSubBooking1Instruction = divotCollectionForSubBooking1Instruction.Select(d => (instructionPK: d.Instruction.PK, packagePK: d.Package.PK)).ToArray();
			var expectedDivotsForSubBooking1 = new (ZGuid instructionPK, ZGuid packagePK)[]
			{
				(instructionPK: masterInstruction.PK, packagePK: subBooking1Container.PK),
				(instructionPK: masterInstruction.PK, packagePK: subBooking1ContainerInnerLoosePackage.PK),
				(instructionPK: masterInstruction.PK, packagePK: subBooking1LoosePackage.PK),
			};

			var divotsForSubBooking2Instruction = divotCollectionForSubBooking2Instruction.Select(d => (instructionPK: d.Instruction.PK, packagePK: d.Package.PK)).ToArray();
			var expectedDivotsForSubBooking2 = new (ZGuid instructionPK, ZGuid packagePK)[]
			{
				(instructionPK: masterInstruction.PK, packagePK: subBooking2Container.PK),
				(instructionPK: masterInstruction.PK, packagePK: subBooking2ContainerInnerLoosePackage.PK),
				(instructionPK: masterInstruction.PK, packagePK: subBooking2LoosePackage.PK),
			};

			CombineAssertions("Check DtbBookingInstructionPkgDivotCollection for sub-booking instructions go from masterInstruction to the packages on the subBooking", () =>
			{
				AssertContainsExactElementsInAnyOrder("Divots in DtbBookingInstructionPkgDivotCollection for subBooking1 Instruction should be the divots between masterInstruction and subBooking1's outer and inner packages", expectedDivotsForSubBooking1, divotsForSubBooking1Instruction);
				AssertContainsExactElementsInAnyOrder("Divots in DtbBookingInstructionPkgDivotCollection for subBooking2 Instruction should be the divots between masterInstruction and subBooking2's outer and inner packages", expectedDivotsForSubBooking2, divotsForSubBooking2Instruction);
			});
		}

		public void TestAddingDivotsToSubBookingInstructionThrowsException()
		{
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			Factory.Save();

			var subBooking = Helper.CreateBooking();
			var subBookingPackage = subBooking.PackageJob.Packages.AddNew("PLT", "LOOSE1");

			masterBooking.SubBookings.AddRange(new[] { subBooking });
			Factory.Save();

			var masterInstruction = masterBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var subBookingInstruction = subBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			subBookingInstruction.KN_KN_MasterBookingInstruction = masterInstruction.PK;
			Factory.Save();

			var subBookingDivotCollection = new DtbBookingInstructionPkgDivotCollection(subBookingInstruction);
			AssertEquals("Precondition: subBooking has no instruction-package divots", 0, subBookingDivotCollection.Count);

			var newSubDivot = Factory.New<DtbBookingInstructionPkgDivot>();
			newSubDivot.KD_KP_Package = subBookingPackage.PK;
			newSubDivot.KD_Quantity = 1;
			newSubDivot.KD_KN_BookingInstruction = subBookingInstruction.PK;
			AssertEquals("Precondition: subBooking still has no instruction-package divots", 0, subBookingDivotCollection.Count);

			AssertExceptionThrown<InvalidOperationException>("Adding divots to sub Booking Instruction should not be allowed", "Adding of new divots not allowed for a sub booking or instruction", () => subBookingDivotCollection.Add(newSubDivot));
			AssertEquals("subBooking still has no instruction-package divots", 0, subBookingDivotCollection.Count);
		}

		public void TestRemovingDivotsFromSubBookingInstructionThrowsException()
		{
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			Factory.Save();

			var subBooking = Helper.CreateBooking();
			var subBookingPackage = subBooking.PackageJob.Packages.AddNew("PLT", "LOOSE1");

			masterBooking.SubBookings.AddRange(new[] { subBooking });
			Factory.Save();

			var masterInstruction = masterBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var subBookingInstruction = subBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			subBookingInstruction.KN_KN_MasterBookingInstruction = masterInstruction.PK;
			Factory.Save();

			var masterDivot = Factory.New<DtbBookingInstructionPkgDivot>();
			masterDivot.KD_KP_Package = subBookingPackage.PK;
			masterDivot.KD_Quantity = 1;
			masterDivot.KD_KN_BookingInstruction = masterInstruction.PK;
			Factory.Save();

			var subBookingDivotCollection = new DtbBookingInstructionPkgDivotCollection(subBookingInstruction);
			AssertEquals("Precondition: subBooking has 1 instruction-package divots, from master", 1, subBookingDivotCollection.Count);

			AssertExceptionThrown<InvalidOperationException>("Removing divots from sub Booking Instruction should not be allowed", "Removing of divots not allowed for a sub booking or instruction", () => subBookingDivotCollection.Delete(masterDivot));
			AssertEquals("subBooking still has 1 instruction-package divot, from master", 1, subBookingDivotCollection.Count);
		}

		public void TestIsDeliveryComplete()
		{
			var collection = GetCollectionToTest();
			AssertEquals(false, collection.IsDeliveryComplete);

			var divot1 = collection.AddNew();
			var divot2 = collection.AddNew();

			// add a completed delivery confirmation to package 1
			var divot1_c = divot1.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);
			divot1_c.KK_Actual = ZDateTime.Now;
			AssertEquals(false, collection.IsDeliveryComplete);

			// add an incomplete delivery confirmation to package 2
			var divot2_c = divot2.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);
			AssertEquals(false, collection.IsDeliveryComplete);

			// complete the delivery confirmation for package 2
			divot2_c.KK_Actual = ZDateTime.Now;
			AssertEquals(true, collection.IsDeliveryComplete);
		}

		public void TestIsPickUpComplete()
		{
			var collection = GetCollectionToTest();
			AssertEquals(false, collection.IsPickUpComplete);

			var divot1 = collection.AddNew();
			var divot2 = collection.AddNew();

			// add a completed pickUp confirmation to package 1
			var divot1_c = divot1.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp);
			divot1_c.KK_Actual = ZDateTime.Now;
			AssertEquals(false, collection.IsPickUpComplete);

			// add an incomplete pickUp confirmation to package 2
			var divot2_c = divot2.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp);
			AssertEquals(false, collection.IsPickUpComplete);

			// complete the pickUp confirmation for package 2
			divot2_c.KK_Actual = ZDateTime.Now;
			AssertEquals(true, collection.IsPickUpComplete);
		}

		public void TestTotalQuantity()
		{
			var collection = GetCollectionToTest();
			AssertEquals("Wrong initial total quantity", 0, collection.TotalQuantity());

			var divot1 = collection.AddNew();
			divot1.KD_Quantity = 1;
			var divot2 = collection.AddNew();
			divot2.KD_Quantity = 2;

			AssertEquals("Wrong total quantity", 3, collection.TotalQuantity());
		}

		protected override DtbBookingInstructionPkgDivotCollection GetCollectionToTest()
		{
			var booking = Helper.CreateBooking();
			var instruction = booking.Instructions.AddNew();

			return new DtbBookingInstructionPkgDivotCollection(instruction);
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}

		TransportBookingTestHelper helper;
	}

	[TestedType(typeof(DtbBookingInstructionPkgDivotCollection))]
	public class DtbBookingInstructionPkgDivotCollectionTest : ActiveBusinessObjectCollectionTestCase<DtbBookingInstructionPkgDivotCollection>
	{
		public void TestAllowNew()
		{
			AssertEquals(false, ((IBindingList)GetCollectionToTest()).AllowNew);
		}

		public void TestRelationship()
		{
			// create consolidation
			var consolidation = Helper.CreateConsolidation();

			// create 2 bookings
			var bookingA = Helper.CreateBooking(consolidation);
			var bookingB = Helper.CreateBooking(consolidation);

			// create 2 instructions per booking
			var picInstructionA = Helper.CreateInstruction(bookingA, InstructionTypes.Codes.PickUp);
			var dlvInstructionA = Helper.CreateInstruction(bookingA, InstructionTypes.Codes.Delivery);
			var picInstructionB = Helper.CreateInstruction(bookingB, InstructionTypes.Codes.PickUp);
			var dlvInstructionB = Helper.CreateInstruction(bookingB, InstructionTypes.Codes.Delivery);

			// create 2 packages
			var packageA = PackingHelper.CreatePackage(consolidation.PackageJob, 15, Constants.PkgUnit.Pallet);
			var packageB = PackingHelper.CreatePackage(consolidation.PackageJob, 20, Constants.PkgUnit.Box);

			// assign both packages to each booking
			var divotPicA_PackageA = Helper.CreatePackageDivot(picInstructionA, packageA, 10);
			var divotPicA_PackageB = Helper.CreatePackageDivot(picInstructionA, packageB, 12);
			var divotDlvA_PackageA = Helper.CreatePackageDivot(dlvInstructionA, packageA, 10);
			var divotDlvA_PackageB = Helper.CreatePackageDivot(dlvInstructionA, packageB, 12);
			var divotPicB_PackageA = Helper.CreatePackageDivot(picInstructionB, packageA, 5);
			var divotPicB_PackageB = Helper.CreatePackageDivot(picInstructionB, packageB, 8);
			var divotDlvB_PackageA = Helper.CreatePackageDivot(dlvInstructionB, packageA, 5);
			var divotDlvB_PackageB = Helper.CreatePackageDivot(dlvInstructionB, packageB, 8);

			// create Package Views
			var packageA_BookingA = new DtbBookingPackage_PackageView(packageA, bookingA);
			var packageA_BookingB = new DtbBookingPackage_PackageView(packageA, bookingB);
			var packageB_BookingA = new DtbBookingPackage_PackageView(packageB, bookingA);
			var packageB_BookingB = new DtbBookingPackage_PackageView(packageB, bookingB);

			AssertContainsExactElementsInAnyOrder(packageA_BookingA.InstructionDivots, new DtbBookingInstructionPkgDivot[2] { divotPicA_PackageA, divotDlvA_PackageA });
			AssertContainsExactElementsInAnyOrder(packageA_BookingB.InstructionDivots, new DtbBookingInstructionPkgDivot[2] { divotPicB_PackageA, divotDlvB_PackageA });
			AssertContainsExactElementsInAnyOrder(packageB_BookingA.InstructionDivots, new DtbBookingInstructionPkgDivot[2] { divotPicA_PackageB, divotDlvA_PackageB });
			AssertContainsExactElementsInAnyOrder(packageB_BookingB.InstructionDivots, new DtbBookingInstructionPkgDivot[2] { divotPicB_PackageB, divotDlvB_PackageB });
		}

		public void TestRelationshipForSubBooking()
		{
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			Factory.Save();

			var subBooking1 = Helper.CreateBooking();
			var subBooking1LoosePackage = subBooking1.PackageJob.Packages.AddNew("PLT", "LOOSE1");
			var subBooking1Container = helper.CreatePackageContainer("CONT1");
			var subBooking1ContainerInnerLoosePackage = helper.CreatePackage("CONT1_IN");
			subBooking1.PackageJob.Packages.Add(subBooking1Container);
			subBooking1Container.Packages.Add(subBooking1ContainerInnerLoosePackage);

			var subBooking2 = Helper.CreateBooking();
			var subBooking2LoosePackage = subBooking2.PackageJob.Packages.AddNew("PLT", "LOOSE2");
			var subBooking2Container = helper.CreatePackageContainer("CONT2");
			var subBooking2ContainerInnerLoosePackage = helper.CreatePackage("CONT2_IN");
			subBooking2.PackageJob.Packages.Add(subBooking2Container);
			subBooking2Container.Packages.Add(subBooking2ContainerInnerLoosePackage);

			masterBooking.SubBookings.AddRange(new[] { subBooking1, subBooking2 });
			Factory.Save();

			var masterInstruction1 = masterBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var masterInstruction2 = masterBooking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			var subBooking1Instruction1 = subBooking1.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			subBooking1Instruction1.KN_KN_MasterBookingInstruction = masterInstruction1.PK;
			var subBooking1Instruction2 = subBooking1.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			subBooking1Instruction2.KN_KN_MasterBookingInstruction = masterInstruction2.PK;
			var subBooking2Instruction1 = subBooking2.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			subBooking2Instruction1.KN_KN_MasterBookingInstruction = masterInstruction1.PK;
			var subBooking2Instruction2 = subBooking2.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			subBooking2Instruction2.KN_KN_MasterBookingInstruction = masterInstruction2.PK;

			var divotInstruction1ToLoosePackage1 = Helper.CreatePackageDivot(masterInstruction1, subBooking1LoosePackage);
			var divotInstruction1ToContainer1 = Helper.CreatePackageDivot(masterInstruction1, subBooking1Container);
			var divotInstruction1ToContainerInnerPackage1 = Helper.CreatePackageDivot(masterInstruction1, subBooking1ContainerInnerLoosePackage);
			var divotInstruction1ToLoosePackage2 = Helper.CreatePackageDivot(masterInstruction1, subBooking2LoosePackage);
			var divotInstruction1ToContainer2 = Helper.CreatePackageDivot(masterInstruction1, subBooking2Container);
			var divotInstruction1ToContainerInnerPackage2 = Helper.CreatePackageDivot(masterInstruction1, subBooking2ContainerInnerLoosePackage);

			var divotInstruction2ToLoosePackage1 = Helper.CreatePackageDivot(masterInstruction2, subBooking1LoosePackage);
			var divotInstruction2ToContainer1 = Helper.CreatePackageDivot(masterInstruction2, subBooking1Container);
			var divotInstruction2ToContainerInnerPackage1 = Helper.CreatePackageDivot(masterInstruction2, subBooking1ContainerInnerLoosePackage);
			var divotInstruction2ToLoosePackage2 = Helper.CreatePackageDivot(masterInstruction2, subBooking2LoosePackage);
			var divotInstruction2ToContainer2 = Helper.CreatePackageDivot(masterInstruction2, subBooking2Container);
			var divotInstruction2ToContainerInnerPackage2 = Helper.CreatePackageDivot(masterInstruction2, subBooking2ContainerInnerLoosePackage);

			Factory.Save();

			var subBooking1LoosePackagePackageView = new DtbBookingPackage_PackageView(subBooking1LoosePackage, subBooking1);
			var subBooking1ContainerPackageView = new DtbBookingPackage_PackageView(subBooking1Container, subBooking1);
			var subBooking1ContainerInnerLoosePackagePackageView = new DtbBookingPackage_PackageView(subBooking1ContainerInnerLoosePackage, subBooking1);
			var subBooking2LoosePackagePackageView = new DtbBookingPackage_PackageView(subBooking2LoosePackage, subBooking2);
			var subBooking2ContainerPackageView = new DtbBookingPackage_PackageView(subBooking2Container, subBooking2);
			var subBooking2ContainerInnerLoosePackagePackageView = new DtbBookingPackage_PackageView(subBooking2ContainerInnerLoosePackage, subBooking2);

			var subBooking1LoosePackageInstructionDivotCollection = new DtbBookingInstructionPkgDivotCollection(subBooking1LoosePackagePackageView);
			var subBooking1ContainerInstructionDivotCollection = new DtbBookingInstructionPkgDivotCollection(subBooking1ContainerPackageView);
			var subBooking1ContainerInnerLoosePackageInstructionDivotCollection = new DtbBookingInstructionPkgDivotCollection(subBooking1ContainerInnerLoosePackagePackageView);
			var subBooking2LoosePackageInstructionDivotCollection = new DtbBookingInstructionPkgDivotCollection(subBooking2LoosePackagePackageView);
			var subBooking2ContainerInstructionDivotCollection = new DtbBookingInstructionPkgDivotCollection(subBooking2ContainerPackageView);
			var subBooking2ContainerInnerLoosePackageInstructionDivotCollection = new DtbBookingInstructionPkgDivotCollection(subBooking2ContainerInnerLoosePackagePackageView);

			var subBooking1LoosePackageDivotInstructionAndPackagePKs = subBooking1LoosePackageInstructionDivotCollection.Select(d => (instructionPK: d.KD_KN_BookingInstruction, packagePK: d.KD_KP_Package)).ToArray();
			var subBooking1ContainerDivotInstructionAndPackagePKs = subBooking1ContainerInstructionDivotCollection.Select(d => (instructionPK: d.KD_KN_BookingInstruction, packagePK: d.KD_KP_Package)).ToArray();
			var subBooking1ContainerInnerLoosePackageDivotInstructionAndPackagePKs = subBooking1ContainerInnerLoosePackageInstructionDivotCollection.Select(d => (instructionPK: d.KD_KN_BookingInstruction, packagePK: d.KD_KP_Package)).ToArray();
			var subBooking2LoosePackageDivotInstructionAndPackagePKs = subBooking2LoosePackageInstructionDivotCollection.Select(d => (instructionPK: d.KD_KN_BookingInstruction, packagePK: d.KD_KP_Package)).ToArray();
			var subBooking2ContainerDivotInstructionAndPackagePKs = subBooking2ContainerInstructionDivotCollection.Select(d => (instructionPK: d.KD_KN_BookingInstruction, packagePK: d.KD_KP_Package)).ToArray();
			var subBooking2ContainerInnerLoosePackageDivotInstructionAndPackagePKs = subBooking2ContainerInnerLoosePackageInstructionDivotCollection.Select(d => (instructionPK: d.KD_KN_BookingInstruction, packagePK: d.KD_KP_Package)).ToArray();

			var expectedSubBooking1LoosePackageDivotInstructionAndPackagePKs = new[]
			{
				(instructionPK: masterInstruction1.PK, packagePK: subBooking1LoosePackage.PK),
				(instructionPK: masterInstruction2.PK, packagePK: subBooking1LoosePackage.PK),
			};
			var expectedSubBooking1ContainerDivotInstructionAndPackagePKs = new[]
			{
				(instructionPK: masterInstruction1.PK, packagePK: subBooking1Container.PK),
				(instructionPK: masterInstruction2.PK, packagePK: subBooking1Container.PK),
			};
			var expectedSubBooking1ContainerInnerLoosePackageDivotInstructionAndPackagePKs = new[]
			{
				(instructionPK: masterInstruction1.PK, packagePK: subBooking1ContainerInnerLoosePackage.PK),
				(instructionPK: masterInstruction2.PK, packagePK: subBooking1ContainerInnerLoosePackage.PK),
			};
			var expectedSubBooking2LoosePackageDivotInstructionAndPackagePKs = new[]
			{
				(instructionPK: masterInstruction1.PK, packagePK: subBooking2LoosePackage.PK),
				(instructionPK: masterInstruction2.PK, packagePK: subBooking2LoosePackage.PK),
			};
			var expectedSubBooking2ContainerDivotInstructionAndPackagePKs = new[]
			{
				(instructionPK: masterInstruction1.PK, packagePK: subBooking2Container.PK),
				(instructionPK: masterInstruction2.PK, packagePK: subBooking2Container.PK),
			};
			var expectedSubBooking2ContainerInnerLoosePackageDivotInstructionAndPackagePKs = new[]
			{
				(instructionPK: masterInstruction1.PK, packagePK: subBooking2ContainerInnerLoosePackage.PK),
				(instructionPK: masterInstruction2.PK, packagePK: subBooking2ContainerInnerLoosePackage.PK),
			};

			CombineAssertions("All DtbBookingInstructionPkgDivotCollections based on PackageViews where Booking is a sub should show the divots on the master booking of the sub booking", () =>
			{
				AssertContainsExactElementsInAnyOrder("DtbBookingInstructionPkgDivotCollection based on subBooking1LoosePackagePackageView should have divots for both master instructions on subBooking1LoosePackage", expectedSubBooking1LoosePackageDivotInstructionAndPackagePKs, subBooking1LoosePackageDivotInstructionAndPackagePKs);
				AssertContainsExactElementsInAnyOrder("DtbBookingInstructionPkgDivotCollection based on subBooking1ContainerPackageView should have divots for both master instructions on subBooking1Container", expectedSubBooking1ContainerDivotInstructionAndPackagePKs, subBooking1ContainerDivotInstructionAndPackagePKs);
				AssertContainsExactElementsInAnyOrder("DtbBookingInstructionPkgDivotCollection based on subBooking1ContainerInnerLoosePackagePackageView should have divots for both master instructions on subBooking1ContainerInnerLoosePackage", expectedSubBooking1ContainerInnerLoosePackageDivotInstructionAndPackagePKs, subBooking1ContainerInnerLoosePackageDivotInstructionAndPackagePKs);
				AssertContainsExactElementsInAnyOrder("DtbBookingInstructionPkgDivotCollection based on subBooking2LoosePackagePackageView should have divots for both master instructions on subBooking2LoosePackage", expectedSubBooking2LoosePackageDivotInstructionAndPackagePKs, subBooking2LoosePackageDivotInstructionAndPackagePKs);
				AssertContainsExactElementsInAnyOrder("DtbBookingInstructionPkgDivotCollection based on subBooking2ContainerPackageView should have divots for both master instructions on subBooking2Container", expectedSubBooking2ContainerDivotInstructionAndPackagePKs, subBooking2ContainerDivotInstructionAndPackagePKs);
				AssertContainsExactElementsInAnyOrder("DtbBookingInstructionPkgDivotCollection based on subBooking2ContainerInnerLoosePackagePackageView should have divots for both master instructions on subBooking2ContainerInnerLoosePackage", expectedSubBooking2ContainerInnerLoosePackageDivotInstructionAndPackagePKs, subBooking2ContainerInnerLoosePackageDivotInstructionAndPackagePKs);
			});
		}

		public void TestDeletingBookingDoesNotCauseRelationshipFilterToThrowException()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);

			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);

			var package = PackingHelper.CreatePackage(consolidation.PackageJob, 15, Constants.PkgUnit.Pallet);

			var divotPic_Package = Helper.CreatePackageDivot(picInstruction, package, 10);
			var divotDlv_Package = Helper.CreatePackageDivot(dlvInstruction, package, 10);

			var bookingPackageView = new DtbBookingPackage_PackageView(package, booking);

			booking.Delete();
			ZQuery testVar = null;
			AssertNoExceptionThrown("RelationshipFilter should not throw exception for accessing Booking.KM_KM_MasterBooking when trying to build relationship", () => testVar = bookingPackageView.InstructionDivots.Relationship.RelationshipFilter);
		}

		public void TestAddingDivotsToSubBookingPackageViewThrowsException()
		{
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			Factory.Save();

			var subBooking = Helper.CreateBooking();
			var subBookingPackage = subBooking.PackageJob.Packages.AddNew("PLT", "LOOSE1");

			masterBooking.SubBookings.AddRange(new[] { subBooking });
			Factory.Save();

			var masterInstruction = masterBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var subBookingInstruction = subBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			subBookingInstruction.KN_KN_MasterBookingInstruction = masterInstruction.PK;
			Factory.Save();

			var subBookingPackageView = new DtbBookingPackage_PackageView(subBookingPackage, subBooking);
			var subBookingPackageViewDivotCollection = new DtbBookingInstructionPkgDivotCollection(subBookingPackageView);
			AssertEquals("Precondition: subBooking has no instruction-package divots", 0, subBookingPackageViewDivotCollection.Count);

			var newSubDivot = Factory.New<DtbBookingInstructionPkgDivot>();
			newSubDivot.KD_KP_Package = subBookingPackage.PK;
			newSubDivot.KD_Quantity = 1;
			newSubDivot.KD_KN_BookingInstruction = subBookingInstruction.PK;
			AssertEquals("Precondition: subBooking still has no instruction-package divots", 0, subBookingPackageViewDivotCollection.Count);

			AssertExceptionThrown<InvalidOperationException>("Adding divots to sub Booking Package View should not be allowed", "Adding of new divots not allowed for a sub booking or instruction", () => subBookingPackageViewDivotCollection.Add(newSubDivot));
			AssertEquals("subBooking still has no instruction-package divots", 0, subBookingPackageViewDivotCollection.Count);
		}

		public void TestRemovingDivotsFromSubBookingPackageViewThrowsException()
		{
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			Factory.Save();

			var subBooking = Helper.CreateBooking();
			var subBookingPackage = subBooking.PackageJob.Packages.AddNew("PLT", "LOOSE1");

			masterBooking.SubBookings.AddRange(new[] { subBooking });
			Factory.Save();

			var masterInstruction = masterBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var subBookingInstruction = subBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			subBookingInstruction.KN_KN_MasterBookingInstruction = masterInstruction.PK;
			Factory.Save();

			var masterDivot = Factory.New<DtbBookingInstructionPkgDivot>();
			masterDivot.KD_KP_Package = subBookingPackage.PK;
			masterDivot.KD_Quantity = 1;
			masterDivot.KD_KN_BookingInstruction = masterInstruction.PK;
			Factory.Save();

			var subBookingPackageView = new DtbBookingPackage_PackageView(subBookingPackage, subBooking);
			var subBookingPackageViewDivotCollection = new DtbBookingInstructionPkgDivotCollection(subBookingPackageView);
			AssertEquals("Precondition: subBooking has 1 instruction-package divots, from master", 1, subBookingPackageViewDivotCollection.Count);

			AssertExceptionThrown<InvalidOperationException>("Removing divots from sub Booking Package View should not be allowed", "Removing of divots not allowed for a sub booking or instruction", () => subBookingPackageViewDivotCollection.Delete(masterDivot));
			AssertEquals("subBooking still has 1 instruction-package divot, from master", 1, subBookingPackageViewDivotCollection.Count);
		}

		protected override DtbBookingInstructionPkgDivotCollection GetCollectionToTest()
		{
			var consolidation = Helper.CreateConsolidation();
			var bookingA = Helper.CreateBooking(consolidation);
			Instruction = Helper.CreateInstruction(bookingA, InstructionTypes.Codes.PickUp);
			var packageA = PackingHelper.CreatePackage(consolidation.PackageJob, 15, Constants.PkgUnit.Pallet);
			var divotPicA_PackageA = Helper.CreatePackageDivot(Instruction, packageA, 10);
			var packageA_BookingA = new DtbBookingPackage_PackageView(packageA, bookingA);
			var collection = new DtbBookingInstructionPkgDivotCollection(packageA_BookingA);
			Factory.Save(); // tests require no changes

			return collection;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var divot = Factory.New<DtbBookingInstructionPkgDivot>();
			divot.KD_KN_BookingInstruction = Instruction.PK;
			return divot;
		}

		DtbBookingInstruction Instruction;

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}
		TransportBookingTestHelper helper;

		protected PackingTestHelper PackingHelper
		{
			get { return packingHelper ?? (packingHelper = new PackingTestHelper(Factory)); }
		}

		PackingTestHelper packingHelper;
	}
}

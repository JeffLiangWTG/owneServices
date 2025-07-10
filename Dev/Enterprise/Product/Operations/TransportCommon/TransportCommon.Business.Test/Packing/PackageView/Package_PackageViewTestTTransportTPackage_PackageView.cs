using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.TransportCommon.Business.Testing
{
	public abstract class Package_PackageViewTest<TTransport, TPackage_PackageView> : NonPersistentBusinessObjectTestCase
			where TTransport : DtbTransport
			where TPackage_PackageView : Package_PackageView
	{
		#region TestConstruction

		public void TestConstruction()
		{
			var transport = GetTransportBizO();
			var package = Factory.New<PkgPackage>();
			var package_PackageView = GetPackageView(package, transport);
			AssertEquals(package, package_PackageView.Package);
			AssertEquals(transport, package_PackageView.Transport);
		}

		#endregion

		#region Related Business Objects

		#region TestInstructionDivots

		public void TestInstructionDivots()
		{
			var transport = GetTransportBizO();
			var package = Factory.New<PkgPackage>();
			var package_PackageView = GetPackageView(package, transport);
			AssertEquals(0, package_PackageView.InstructionDivots.Count);
			AssertEquals(true, package_PackageView.IsRegisteredEditableChildObject(package_PackageView.InstructionDivots));

			var instruction = (DtbTransportInstruction)transport.Instructions.AddNew();
			var packageDivot = (DtbTransportInstructionPkgDivot)instruction.PackageDivots.AddNew();
			packageDivot.KD_KP_Package = package.PK;
			AssertContainsExactElementsInAnyOrder(new DtbTransportInstructionPkgDivot[] { packageDivot }, package_PackageView.InstructionDivots);
		}
		#endregion

		#region TestConfirmations

		public void TestConfirmations()
		{
			var transport = GetTransportBizO();
			var instruction = (DtbTransportInstruction)transport.Instructions.AddNew();
			var divot_p1 = Helper.CreatePackageDivot(instruction);
			var divot_p2 = Helper.CreatePackageDivot(instruction);
			var p1 = Helper.CreatePackage("p1", divot_p1);
			var p2 = Helper.CreatePackage("p2", divot_p2);
			var confirmation_ALL = Helper.CreateConfirmation(instruction, ConfirmationTypes.Codes.PickUp);
			var confirmation_p1 = Helper.CreateConfirmation(divot_p1, ConfirmationTypes.Codes.Delivery);
			var confirmation_p2 = Helper.CreateConfirmation(divot_p2, ConfirmationTypes.Codes.Delivery);

			var packageView_P1 = GetPackageView(p1, transport);
			var packageView_P2 = GetPackageView(p2, transport);

			AssertEquals(true, packageView_P1.IsRegisteredEditableChildObject(packageView_P1.Confirmations));

			AssertContainsExactElementsInAnyOrder(new DtbTransportConfirmation[] { confirmation_p1, confirmation_ALL }, packageView_P1.Confirmations);
			AssertContainsExactElementsInAnyOrder(new DtbTransportConfirmation[] { confirmation_p2, confirmation_ALL }, packageView_P2.Confirmations);

			var newInstruction = transport.Instructions.AddNew();
			divot_p1.KD_KN_BookingInstruction = newInstruction.PK;
			AssertContainsExactElementsInAnyOrder(new DtbTransportConfirmation[] { confirmation_p1 }, packageView_P1.Confirmations);

			var confirmation_p1New = Helper.CreateConfirmation(divot_p1, ConfirmationTypes.Codes.Delivery);
			AssertContainsExactElementsInAnyOrder(new DtbTransportConfirmation[] { confirmation_p1, confirmation_p1New }, packageView_P1.Confirmations);

			var confirmation_Added = (DtbTransportConfirmation)packageView_P1.Confirmations.AddNew();
			AssertContainsExactElementsInAnyOrder(new DtbTransportConfirmation[] { confirmation_p1, confirmation_p1New, confirmation_Added }, packageView_P1.Confirmations);
		}
		#endregion

		#region TestConfirmations_DontHitCollectionsBeforeAdd

		public void TestConfirmations_DontHitCollectionsBeforeAdd()
		{
			var transport = GetTransportBizO();
			var instruction = (DtbTransportInstruction)transport.Instructions.AddNew();
			var divot_p1 = Helper.CreatePackageDivot(instruction, 1);
			var divot_p2 = Helper.CreatePackageDivot(instruction, 1);
			var p1 = Helper.CreatePackage("p1", divot_p1);
			var p2 = Helper.CreatePackage("p2", divot_p2);
			var confirmation_ALL = Helper.CreateConfirmation(instruction, ConfirmationTypes.Codes.PickUp);
			var confirmation_p1 = Helper.CreateConfirmation(divot_p1, ConfirmationTypes.Codes.Delivery);

			var packageView_P1 = GetPackageView(p1, transport);
			var packageView_P2 = GetPackageView(p2, transport);

			AssertContainsExactElementsInAnyOrder(new DtbTransportConfirmation[] { confirmation_p1, confirmation_ALL }, packageView_P1.Confirmations);
			AssertContainsExactElementsInAnyOrder(new DtbTransportConfirmation[] { confirmation_ALL }, packageView_P2.Confirmations);

			confirmation_ALL.KK_KD_BookingInstructionPkgDivot = divot_p2.PK;

			AssertContainsExactElementsInAnyOrder(new DtbTransportConfirmation[] { confirmation_p1 }, packageView_P1.Confirmations);
			AssertContainsExactElementsInAnyOrder(new DtbTransportConfirmation[] { confirmation_ALL }, packageView_P2.Confirmations);
		}

		#endregion

		#region TestConfirmations_EnsureCollectionsHitBeforeAdd

		public void TestConfirmations_EnsureCollectionsHitBeforeAdd()
		{
			var transport = GetTransportBizO();
			var instruction1 = (DtbTransportInstruction)transport.Instructions.AddNew();
			var instruction2 = (DtbTransportInstruction)transport.Instructions.AddNew();
			var instruction3 = (DtbTransportInstruction)transport.Instructions.AddNew();

			var divot_p1 = Helper.CreatePackageDivot(instruction1, 1);
			var divot_p2 = Helper.CreatePackageDivot(instruction1, 1);
			var p1 = Helper.CreatePackage("p1", divot_p1);
			var p2 = Helper.CreatePackage("p2", divot_p2);

			var packageView_P1 = GetPackageView(p1, transport);
			var packageView_P2 = GetPackageView(p2, transport);
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<DtbTransportConfirmation>(), packageView_P1.Confirmations);
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<DtbTransportConfirmation>(), packageView_P2.Confirmations);

			var confirmation_ALL = Helper.CreateConfirmation(instruction1, ConfirmationTypes.Codes.PickUp);
			AssertContainsExactElementsInAnyOrder(new DtbTransportConfirmation[] { confirmation_ALL }, packageView_P1.Confirmations);
			AssertContainsExactElementsInAnyOrder(new DtbTransportConfirmation[] { confirmation_ALL }, packageView_P2.Confirmations);

			confirmation_ALL.KK_KD_BookingInstructionPkgDivot = divot_p2.PK;
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<DtbTransportConfirmation>(), packageView_P1.Confirmations);
			AssertContainsExactElementsInAnyOrder(new DtbTransportConfirmation[] { confirmation_ALL }, packageView_P2.Confirmations);

			confirmation_ALL.KK_KD_BookingInstructionPkgDivot = divot_p2.PK;
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<DtbTransportConfirmation>(), packageView_P1.Confirmations);
			AssertContainsExactElementsInAnyOrder(new DtbTransportConfirmation[] { confirmation_ALL }, packageView_P2.Confirmations);

			confirmation_ALL.KK_KD_BookingInstructionPkgDivot = divot_p1.PK;
			AssertContainsExactElementsInAnyOrder(new DtbTransportConfirmation[] { confirmation_ALL }, packageView_P1.Confirmations);
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<DtbTransportConfirmation>(), packageView_P2.Confirmations);

			confirmation_ALL.KK_KD_BookingInstructionPkgDivot = ZGuid.Empty;
			AssertContainsExactElementsInAnyOrder(new DtbTransportConfirmation[] { confirmation_ALL }, packageView_P1.Confirmations);
			AssertContainsExactElementsInAnyOrder(new DtbTransportConfirmation[] { confirmation_ALL }, packageView_P2.Confirmations);
		}

		#endregion

		#endregion

		#region TestTotalsFromInstructions

		#region TestQuantityFromInstructions

		public void TestQuantityFromInstructions()
		{
			TTransport transport;
			PkgPackage packageA;
			PkgPackage packageB;
			SetupDataForTotalsFromInstructionsTests(out transport, out packageA, out packageB);

			AssertEquals("Pickup Instructions for PackageA have more divots that Delivery Instructions (5 + 6 > 2 + 7).", 11, transport.Packages_PackageView.Find(packageA).QuantityFromInstructions);
			AssertEquals("Should use qty from PackageB because Delivery Instruction divots quantity bigger that max. (7 + 14 > 20).", 20, transport.Packages_PackageView.Find(packageB).QuantityFromInstructions);
		}

		#endregion

		#region TestWeightFromInstructions

		public void TestWeightFromInstructions()
		{
			TTransport transport;
			PkgPackage packageA;
			PkgPackage packageB;
			SetupDataForTotalsFromInstructionsTests(out transport, out packageA, out packageB);

			AssertEquals("Pickup Instructions for PackageA have more divots that Delivery Instructions (5 + 6 > 2 + 7).", 110m, transport.Packages_PackageView.Find(packageA).WeightFromInstructions.Amount);
			AssertEquals("Should have same WU as PackageA.", Constants.Weight.Kilograms, transport.Packages_PackageView.Find(packageA).WeightFromInstructions.Unit);
			AssertEquals("Should use qty from PackageB because Delivery Instruction divots quantity bigger that max. (7 + 14 > 20).", 200m, transport.Packages_PackageView.Find(packageB).WeightFromInstructions.Amount);
			AssertEquals("Should have same WU as PackageB.", Constants.Weight.Pounds, transport.Packages_PackageView.Find(packageB).WeightFromInstructions.Unit);
		}

		#endregion

		#region TestVolumeFromInstructions

		public void TestVolumeFromInstructions()
		{
			TTransport transport;
			PkgPackage packageA;
			PkgPackage packageB;
			SetupDataForTotalsFromInstructionsTests(out transport, out packageA, out packageB);

			AssertEquals("Pickup Instructions for PackageA have more divots that Delivery Instructions (5 + 6 > 2 + 7).", 110m, transport.Packages_PackageView.Find(packageA).VolumeFromInstructions.Amount);
			AssertEquals("Should have same VU as PackageA.", Constants.Volume.CubicMetres, transport.Packages_PackageView.Find(packageA).VolumeFromInstructions.Unit);
			AssertEquals("Should use qty from PackageB because Delivery Instruction divots quantity bigger that max. (7 + 14 > 20).", 200m, transport.Packages_PackageView.Find(packageB).VolumeFromInstructions.Amount);
			AssertEquals("Should have same VU as PackageB.", Constants.Volume.CubicFeet, transport.Packages_PackageView.Find(packageB).VolumeFromInstructions.Unit);
		}

		#endregion

		#region SetupDataForTotalsFromInstrucitonsTests

		void SetupDataForTotalsFromInstructionsTests(out TTransport transport, out PkgPackage packageA, out PkgPackage packageB)
		{
			transport = GetTransportBizO();
			var pickupInstruction1 = Helper.CreateInstruction(transport, InstructionTypes.Codes.PickUp);
			var pickupInstruction2 = Helper.CreateInstruction(transport, InstructionTypes.Codes.PickUp);
			var multiInstruction = Helper.CreateInstruction(transport, InstructionTypes.Codes.Multi);
			var deliveryInstruction1 = Helper.CreateInstruction(transport, InstructionTypes.Codes.Delivery);
			var deliveryInstruction2 = Helper.CreateInstruction(transport, InstructionTypes.Codes.Delivery);

			var packageJob = GetPackageJob(transport);
			packageA = PackingHelper.CreatePackage(packageJob, 15, Constants.PkgUnit.Pallet);
			packageB = PackingHelper.CreatePackage(packageJob, 20, Constants.PkgUnit.Box);
			packageA.KP_Weight = 150;
			packageA.KP_WeightUQ = Constants.Weight.Kilograms;
			packageA.KP_Volume = 150;
			packageA.KP_VolumeUQ = Constants.Volume.CubicMetres;

			packageB.KP_Weight = 200;
			packageB.KP_WeightUQ = Constants.Weight.Pounds;
			packageB.KP_Volume = 200;
			packageB.KP_VolumeUQ = Constants.Volume.CubicFeet;

			SetQuantityOnExistingDivotOrCreateNewDivot(pickupInstruction1, packageA, 5);
			SetQuantityOnExistingDivotOrCreateNewDivot(pickupInstruction1, packageB, 2);
			SetQuantityOnExistingDivotOrCreateNewDivot(pickupInstruction2, packageA, 6);
			SetQuantityOnExistingDivotOrCreateNewDivot(pickupInstruction2, packageB, 12);

			SetQuantityOnExistingDivotOrCreateNewDivot(multiInstruction, packageA, 15);
			SetQuantityOnExistingDivotOrCreateNewDivot(multiInstruction, packageB, 5);

			SetQuantityOnExistingDivotOrCreateNewDivot(deliveryInstruction1, packageA, 2);
			SetQuantityOnExistingDivotOrCreateNewDivot(deliveryInstruction1, packageB, 7);
			SetQuantityOnExistingDivotOrCreateNewDivot(deliveryInstruction2, packageA, 7);
			SetQuantityOnExistingDivotOrCreateNewDivot(deliveryInstruction2, packageB, 14);
		}

		void SetQuantityOnExistingDivotOrCreateNewDivot(DtbTransportInstruction instruction, PkgPackage package, int quantity)
		{
			var existingDivot = instruction.PackageDivots.Cast<DtbTransportInstructionPkgDivot>().SingleOrDefault(d => d.KD_KP_Package == package.PK);
			if (existingDivot != null)
			{
				existingDivot.KD_Quantity = quantity;
			}
			else
			{
				Helper.CreatePackageDivot(instruction, package, quantity);
			}
		}

		protected abstract PkgPackageJob GetPackageJob(DtbTransport transport);

		#endregion

		#endregion

		#region TestDelete

		public void TestDelete()
		{
			var transport = GetTransportBizO();
			var package1 = Factory.New<PkgPackage>();
			var package2 = Factory.New<PkgPackage>();
			var instructionA = (DtbTransportInstruction)transport.Instructions.AddNew();
			var instructionB = (DtbTransportInstruction)transport.Instructions.AddNew();
			var instructionC = (DtbTransportInstruction)transport.Instructions.AddNew();
			var package1ADivot = Helper.CreatePackageDivot(instructionA, package1, 0);
			var package1BDivot = Helper.CreatePackageDivot(instructionB, package1, 0);
			var package2BDivot = Helper.CreatePackageDivot(instructionB, package2, 0);
			var package2CDivot = Helper.CreatePackageDivot(instructionC, package2, 0);

			var package1_PackageView = GetPackageView(package1, transport);
			var package2_PackageView = GetPackageView(package2, transport);

			package1_PackageView.Delete();
			AssertEquals(true, package1ADivot.IsDeleted);
			AssertEquals(true, package1BDivot.IsDeleted);
			AssertEquals(false, package2BDivot.IsDeleted);
			AssertEquals(false, package2CDivot.IsDeleted);

			package2_PackageView.Delete();
			AssertEquals(true, package2BDivot.IsDeleted);
			AssertEquals(true, package2CDivot.IsDeleted);
		}

		public void TestIsDeleted()
		{
			var packageView = GetPackageView(Factory);
			AssertEquals(false, packageView.IsDeleted);
		}

		#endregion

		#region Implementation

		protected abstract TTransport GetTransportBizO();
		protected abstract TPackage_PackageView GetPackageView(PkgPackage package, TTransport transport);
		protected abstract TPackage_PackageView GetPackageView(BusinessObjectFactory factory);

		protected override BusinessObject GetNewBusinessObject()
		{
			var transport = GetTransportBizO();
			var package = Factory.New<PkgPackage>();
			return GetPackageView(package, transport);
		}

		#region Helper

		TransportCommonTestHelper Helper
		{
			get { return helper ?? (helper = new TransportCommonTestHelper(Factory)); }
		}
		TransportCommonTestHelper helper;

		#endregion

		#region PackingHelper

		protected PackingTestHelper PackingHelper
		{
			get { return packingHelper ?? (packingHelper = new PackingTestHelper(Factory)); }
		}

		PackingTestHelper packingHelper;

		#endregion

		#endregion
	}
}

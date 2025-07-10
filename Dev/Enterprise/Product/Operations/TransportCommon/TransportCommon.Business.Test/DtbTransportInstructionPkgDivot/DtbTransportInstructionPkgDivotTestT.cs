using System;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.TransportCommon.Shared;
using NUnit.Framework.TestHelper;

namespace Enterprise.TransportCommon.Business.Testing
{
	public abstract class DtbTransportInstructionPkgDivotTest : DtbTransportBusinessObjectTestCase
	{
		#region Related Entities

		#region TestInstruction

		public void TestInstruction()
		{
			var instruction = (DtbTransportInstruction)Factory.New(ExpectedInstructionType);
			var packageDivot = (DtbTransportInstructionPkgDivot)GetNewBusinessObject();

			packageDivot.KD_KN_BookingInstruction = instruction.PK;

			AssertEquals(instruction, packageDivot.Instruction);
			AssertEquals(ExpectedInstructionType, packageDivot.Instruction.GetType());
		}

		protected abstract Type ExpectedInstructionType { get; }

		#endregion

		#region TestPackage

		public void TestPackage()
		{
			var packageDivot = (DtbTransportInstructionPkgDivot)GetNewBusinessObject();
			var package = Factory.New<PkgPackage>();

			packageDivot.KD_KP_Package = package.PK;
			AssertEquals(package, packageDivot.Package);
		}

		#endregion

		#region TestConfirmationsDivotOnly

		public void TestConfirmationsDivotOnly()
		{
			var instruction = GetNewInstruction();
			var divot = (DtbTransportInstructionPkgDivot)instruction.PackageDivots.AddNew();
			var confirmationOnDivot = divot.ConfirmationsDivotOnly.AddNew();
			var confirmationOnInstruction = instruction.Confirmations.AddNew();

			AssertContainsExactElementsInAnyOrder(new[] { confirmationOnDivot }, divot.ConfirmationsDivotOnly);
		}

		#endregion

		#region Confirmations

		public void TestConfirmations()
		{
			var instruction = GetNewInstruction();
			var confirmation_ALL = Helper.CreateConfirmation(instruction, ConfirmationTypes.Codes.PickUp);
			var divot_p1 = Helper.CreatePackageDivot(instruction, 1);
			var divot_p2 = Helper.CreatePackageDivot(instruction, 1);
			var p1 = Helper.CreatePackage("p1", divot_p1);
			var p2 = Helper.CreatePackage("p2", divot_p2);
			var confirmation_p1 = Helper.CreateConfirmation(divot_p1, ConfirmationTypes.Codes.Delivery);
			var confirmation_p2 = Helper.CreateConfirmation(divot_p2, ConfirmationTypes.Codes.Delivery);

			AssertContainsExactElementsInAnyOrder(new DtbTransportConfirmation[] { confirmation_p1, confirmation_ALL }, divot_p1.Confirmations);
			AssertContainsExactElementsInAnyOrder(new DtbTransportConfirmation[] { confirmation_p2, confirmation_ALL }, divot_p2.Confirmations);

			var newInstruction = GetNewInstruction();
			divot_p1.KD_KN_BookingInstruction = newInstruction.PK;
			AssertContainsExactElementsInAnyOrder(new DtbTransportConfirmation[] { confirmation_p1 }, divot_p1.Confirmations);

			var confirmation_p1New = Helper.CreateConfirmation(divot_p1, ConfirmationTypes.Codes.Delivery);
			AssertContainsExactElementsInAnyOrder(new DtbTransportConfirmation[] { confirmation_p1, confirmation_p1New }, divot_p1.Confirmations);
		}

		#endregion

		#region TestWeight

		public void TestWeight()
		{
			var instruction = GetNewInstruction();
			var divot = (DtbTransportInstructionPkgDivot)instruction.PackageDivots.AddNew();

			AssertEquals(0m, divot.Weight);

			var package = Helper.CreatePackage("P1", 100, "BOX");
			package.KP_Weight = 10;
			divot.KD_KP_Package = package.PK;
			divot.KD_Quantity = 17;
			AssertEquals(1.7m, divot.Weight);

			divot.KD_Quantity = 200;
			AssertEquals(10m, divot.Weight);

			divot.KD_Quantity = 0;
			AssertEquals(0m, divot.Weight);
		}

		#endregion

		#region TestVolume

		public void TestVolume()
		{
			var instruction = GetNewInstruction();
			var divot = (DtbTransportInstructionPkgDivot)instruction.PackageDivots.AddNew();

			AssertEquals(0m, divot.Volume);

			var package = Helper.CreatePackage("P1", 100, "BOX");
			package.KP_Volume = 10;
			divot.KD_KP_Package = package.PK;
			divot.KD_Quantity = 17;
			AssertEquals(1.7m, divot.Volume);

			divot.KD_Quantity = 200;
			AssertEquals(10m, divot.Volume);

			divot.KD_Quantity = 0;
			AssertEquals(0m, divot.Volume);
		}

		#endregion

		#endregion

		#region Properties

		#region TestKD_KP_Package

		public void TestKD_KP_Package()
		{
			var package = Factory.New<PkgPackage>();
			package.KP_PackageQty = 10;

			var divot = (DtbTransportInstructionPkgDivot)Factory.New(TestedTypeHelper.GetTestedType(GetType()));

			divot.KD_KP_Package = package.PK;
			AssertEquals(10, divot.KD_Quantity);

			divot.KD_KP_Package = ZGuid.Empty;
			AssertEquals(0, divot.KD_Quantity);
		}

		#endregion

		#region TestKD_KP_PackageDefaultsDropModeOnInstruction

		public void TestKD_KP_PackageDefaultsDropModeOnInstruction()
		{
			var instruction = GetNewInstruction();
			var consignor = Helper.CreateOrganisation("CNR");
			consignor.MainAddress.OA_LCLEquipmentNeeded = "DM1";
			instruction.Address.OrganisationPK = consignor.PK;
			var divot = (DtbTransportInstructionPkgDivot)instruction.PackageDivots.AddNew();
			TestKD_KP_PackageDefaultsDropModeOnInstructionCore(instruction, divot);

			var package = Factory.New<PkgPackage>();
			divot.KD_KP_Package = package.PK;
			AssertEquals("DM1", instruction.KN_DropMode);
		}

		protected virtual void TestKD_KP_PackageDefaultsDropModeOnInstructionCore(DtbTransportInstruction instruction, DtbTransportInstructionPkgDivot divot)
		{
			AssertEquals("Precondition", "", instruction.KN_DropMode);
		}

		#endregion

		#endregion

		#region Implementation

		protected abstract DtbTransportInstruction GetNewInstruction();

		#endregion
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Packing.Business;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Business.Testing;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Business.Testing
{
	[TestedType(typeof(DtbBookingConfirmation))]
	public class DtbBookingConfirmationTest : DtbTransportBusinessObjectTestCase
	{
		public void TestKK_ConfirmationType_RefreshesInstructionConNoteNo()
		{
			AssertParentInstructionConNoteNoIsRefreshed("Changing KK_ConfirmationType should refresh the parent Instruction's ConNote #.", c => c.KK_ConfirmationType = ConfirmationTypes.Codes.PickUp);
		}

		public void TestKK_ReferenceNum_RefreshesInstructionConNoteNo()
		{
			AssertParentInstructionConNoteNoIsRefreshed("Changing KK_ReferenceNum should refresh the parent Instruction's ConNote #.", c => c.KK_ReferenceNum = "");
		}

		public void TestHasPackages()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			var instruction = booking.Instructions.AddNew();
			var confirmation = instruction.Confirmations.AddNew();
			AssertEquals("Since there are no packages, it should be false.", false, confirmation.HasPackages);

			var packageJob = Helper.CreatePackageJob(consolidation);
			var packageDivot = Helper.CreatePackageDivot(instruction, 1);
			Helper.CreatePackage("Package", packageDivot, 1);
			AssertEquals("Since there is a loose package on instruction, it should true.", true, confirmation.HasPackages);

			instruction.PackageDivots.RemoveFromRelationship(packageDivot);
			AssertEquals("Since there are no packages, it should be false.", false, confirmation.HasPackages);

			confirmation.KK_KD_BookingInstructionPkgDivot = packageDivot.PK;
			AssertEquals("Since there is a loose package on confirmation, it should true.", true, confirmation.HasPackages);
		}

		public void TestHumanReadableName()
		{
			var confirmation = Factory.New<DtbBookingConfirmation>();
			AssertEquals("Confirmation", confirmation.HumanReadableName);

			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			var instruction = booking.Instructions.AddNew();
			var packageJob = Helper.CreatePackageJob(consolidation);
			var packageDivot = Helper.CreatePackageDivot(instruction, 1);
			var package = Helper.CreatePackage("P1", packageDivot, 1);
			confirmation = packageDivot.ConfirmationsDivotOnly.AddNew();
			AssertEquals("Confirmation, P1", confirmation.HumanReadableName);

			confirmation.KK_ConfirmationType = ConfirmationTypes.Codes.Delivery;
			AssertEquals("Confirmation Delivery, P1", confirmation.HumanReadableName);

			instruction.OrganisationType = OrganisationTypesList.Codes.CNE;
			AssertEquals("Confirmation Delivery, CNE, P1", confirmation.HumanReadableName);

			var address = Helper.CreateOrganisation("ABCSYD").MainAddress;
			address.OA_City = "Sydney";
			instruction.Address.E2_OA_Address = address.PK;
			AssertEquals("Confirmation Delivery, CNE-Sydney, P1", confirmation.HumanReadableName);

			package.KP_F3_NKPackType = "PLT";
			package.KP_PackageID = "";
			AssertEquals("Confirmation Delivery, CNE-Sydney, 1x PLT", confirmation.HumanReadableName);

			package.KP_F3_NKPackType = "BOX";
			AssertEquals("Confirmation Delivery, CNE-Sydney, 1x BOX", confirmation.HumanReadableName);

			package.KP_PackageQty = 45;
			packageDivot.KD_Quantity = 45;
			confirmation.KK_Quantity = 45;
			AssertEquals("Confirmation Delivery, CNE-Sydney, 45x BOX", confirmation.HumanReadableName);

			confirmation = instruction.Confirmations.AddNew();
			confirmation.KK_ConfirmationType = ConfirmationTypes.Codes.Delivery;
			var packageDivot2 = Helper.CreatePackageDivot(instruction, 44);
			var package2 = Helper.CreatePackage("P2", packageDivot2, 44);
			package2.KP_F3_NKPackType = "BOX";
			AssertEquals("Confirmation Delivery, CNE-Sydney, 89x BOX", confirmation.HumanReadableName);

			package2.KP_F3_NKPackType = "BAG";
			AssertEquals("Confirmation Delivery, CNE-Sydney, 89x PCE", confirmation.HumanReadableName);

			package.KP_PackageQty = 1;
			package2.KP_PackageQty = 1;
			packageDivot.KD_Quantity = 1;
			packageDivot2.KD_Quantity = 1;
			package.KP_PackageID = "P1";
			package2.KP_PackageID = "P2";
			AssertEquals("Confirmation Delivery, CNE-Sydney, 2x PCE", confirmation.HumanReadableName);

			package2.KP_F3_NKPackType = "BOX";
			AssertEquals("Confirmation Delivery, CNE-Sydney, 2x BOX", confirmation.HumanReadableName);
		}

		protected override Type ExpectedLookupsType
		{
			get { return typeof(DtbBookingConfirmationLookups); }
		}

		protected override Type ExpectedValidationType
		{
			get { return typeof(DtbBookingConfirmationValidation); }
		}

		public void TestSetIsEmptyContainer()
		{
			var booking = Helper.CreateBooking();
			var instruction = booking.Instructions.AddNew();
			var divot = instruction.PackageDivots.AddNew();
			var package = Factory.New<PkgPackage>();
			divot.KD_KP_Package = package.PK;
			var confirmationWithoutInstructionOrDivot = Factory.New<DtbBookingConfirmation>();
			var confirmationWithPackageDivot = Helper.CreateConfirmation(divot, ConfirmationTypes.Codes.Delivery);
			var confirmationForInstruction = Helper.CreateConfirmation(instruction, ConfirmationTypes.Codes.Delivery);
			AssertEquals("Precondition", false, confirmationWithoutInstructionOrDivot.KK_IsEmptyContainer);
			AssertEquals("Precondition", false, confirmationWithPackageDivot.KK_IsEmptyContainer);
			AssertEquals("Precondition", false, confirmationForInstruction.KK_IsEmptyContainer);

			// Confirmation does not have an instruction or divot
			confirmationWithoutInstructionOrDivot.SetIsEmptyContainer();
			AssertEquals(false, confirmationWithoutInstructionOrDivot.KK_IsEmptyContainer);

			// Non containerised and not empty yard
			package.KP_F3_NKPackType = "PLT";
			instruction.OrganisationType = OrganisationTypesList.Codes.CTO;
			confirmationWithPackageDivot.SetIsEmptyContainer();
			AssertEquals(false, confirmationWithPackageDivot.KK_IsEmptyContainer);

			confirmationForInstruction.SetIsEmptyContainer();
			AssertEquals(false, confirmationForInstruction.KK_IsEmptyContainer);

			// Containerised and but not empty yard
			package.KP_F3_NKPackType = "CNT";
			confirmationWithPackageDivot.SetIsEmptyContainer();
			AssertEquals(false, confirmationWithPackageDivot.KK_IsEmptyContainer);

			confirmationForInstruction.SetIsEmptyContainer();
			AssertEquals(false, confirmationForInstruction.KK_IsEmptyContainer);

			// Containerised and empty yard
			instruction.OrganisationType = OrganisationTypesList.Codes.CYD;
			confirmationWithPackageDivot.SetIsEmptyContainer();
			AssertEquals(true, confirmationWithPackageDivot.KK_IsEmptyContainer);

			confirmationForInstruction.SetIsEmptyContainer();
			AssertEquals(true, confirmationForInstruction.KK_IsEmptyContainer);
		}

		public void TestSetMT_OnConfirmationAdd()
		{
			var booking = Helper.CreateBooking();

			var instructionCYD = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CYD);
			var instructionCNR = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNR);

			var container = booking.ConsolidationSingleJob.PackageJob.Packages.AddNew("CNT", "C1");

			instructionCYD.DivotsWithPackages.AddPackage(container);
			instructionCNR.DivotsWithPackages.AddPackage(container);
			AssertEquals("Precondition: CNR should have no Confirmations", false, instructionCNR.Confirmations.Any());

			var confirmationCNR = instructionCNR.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);
			AssertEquals("Previous instruction is CYD for C1, so the CNR Delivery should be IsEmpty.", true, confirmationCNR.KK_IsEmptyContainer);
		}

		public void TestSetMT_OnConfirmationPackageChange()
		{
			var booking = Helper.CreateBooking();

			var instructionCYD = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CYD);
			var instructionCNR = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNR);

			var container = booking.ConsolidationSingleJob.PackageJob.Packages.AddNew("CNT", "C1");
			var pallet = booking.ConsolidationSingleJob.PackageJob.Packages.AddNew("PLT", "P1");

			var cnrContainerDivot = Helper.CreatePackageDivot(instructionCNR, container);
			instructionCNR.DivotsWithPackages.AddPackage(pallet);
			instructionCYD.DivotsWithPackages.AddPackage(container);
			instructionCYD.DivotsWithPackages.AddPackage(pallet);
			AssertEquals("Precondition: CNR should have no Confirmations", false, instructionCNR.Confirmations.Any());

			var confirmationCNR = instructionCNR.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);
			AssertEquals("Delivery Confirmation is for All Packages (one is a Pallet so IsEmpty is not set).", false, confirmationCNR.KK_IsEmptyContainer);

			confirmationCNR.KK_KD_BookingInstructionPkgDivot = cnrContainerDivot.PK;
			AssertEquals("Previous instruction is CYD for JUST the C1, so the CNR Delivery should be IsEmpty.", true, confirmationCNR.KK_IsEmptyContainer);
		}

		public void TestSetMT_OnConfirmationPackageAdd_PairOfInstructionsWithPickupAsCYDInstruction()
		{
			var booking = Helper.CreateBooking();

			var instructionCYD = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CYD);
			var instructionCNR = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNR);

			var container = booking.ConsolidationSingleJob.PackageJob.Packages.AddNew("CNT", "C1");

			var confirmationCYD = instructionCYD.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp);
			var confirmationCNR = instructionCNR.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);
			instructionCYD.DivotsWithPackages.AddPackage(container);
			instructionCNR.DivotsWithPackages.AddPackage(container);
			AssertEquals("Pickup Confirmation is for Package On CYD Confirmation", true, confirmationCYD.KK_IsEmptyContainer);
			AssertEquals("Delivery Confirmation is for Package On CNR Confirmation", true, confirmationCNR.KK_IsEmptyContainer);
		}

		public void TestSetMT_OnConfirmationPackageAdd_PairOfInstructionsWithDeliveryAsCYDInstruction()
		{
			var booking = Helper.CreateBooking();

			var instructionCFS = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CFS);
			var instructionCYD = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CYD);

			var container = booking.ConsolidationSingleJob.PackageJob.Packages.AddNew("CNT", "C1");

			var confirmationCFS = instructionCFS.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp);
			var confirmationCYD = instructionCYD.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);
			instructionCFS.DivotsWithPackages.AddPackage(container);
			instructionCYD.DivotsWithPackages.AddPackage(container);
			AssertEquals("Pickup Confirmation is for Package On CFS Confirmation", true, confirmationCFS.KK_IsEmptyContainer);
			AssertEquals("Delivery Confirmation is for Package On CYD Confirmation", true, confirmationCYD.KK_IsEmptyContainer);
		}

		public void TestSetMT_NoPackages()
		{
			var booking = Helper.CreateBooking();

			var instructionCYD = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CYD);
			var instructionCNR = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNR);

			AssertEquals("Precondition: CNR should have no Confirmations", false, instructionCNR.Confirmations.Any());

			var confirmationCNR = instructionCNR.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);
			AssertEquals("There are no packages so the CNR Delivery should not be IsEmpty.", false, confirmationCNR.KK_IsEmptyContainer);
		}

		public void TestSetMT_Export_AddingPickupConfirmation()
		{
			var booking = Helper.CreateBooking();

			var instructionCYD = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CYD);
			var instructionCNR = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR);

			var container = booking.ConsolidationSingleJob.PackageJob.Packages.AddNew("CNT", "C1");

			instructionCYD.DivotsWithPackages.AddPackage(container);
			instructionCNR.DivotsWithPackages.AddPackage(container);
			AssertEquals("Precondition: CNR should have no Confirmations", false, instructionCNR.Confirmations.Any());

			var confirmationCNR = instructionCNR.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp);
			AssertEquals("Previous instruction is CYD for C1 but the CNR is a pickup not a delivery so should not be IsEmpty.", false, confirmationCNR.KK_IsEmptyContainer);
		}

		public void TestSetMT_Export_MultiAddress_CYD_CFS_CNR()
		{
			var booking = Helper.CreateBooking();

			var instructionCYD = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CYD);
			var instructionCFS = booking.Instructions.AddNew(InstructionTypes.Codes.Multi, OrganisationTypesList.Codes.CFS);
			var instructionCNR = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNR);

			var container = booking.ConsolidationSingleJob.PackageJob.Packages.AddNew("CNT", "C1");

			instructionCYD.DivotsWithPackages.AddPackage(container);
			instructionCFS.DivotsWithPackages.AddPackage(container);
			instructionCNR.DivotsWithPackages.AddPackage(container);
			AssertEquals("Precondition: CNR should have no Confirmations", false, instructionCNR.Confirmations.Any());

			var confirmationCFS = instructionCFS.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);
			AssertEquals("Previous instructions for CFS is CYD, so the CFS Delivery should be IsEmpty", true, confirmationCFS.KK_IsEmptyContainer);

			var confirmationCNR = instructionCNR.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);
			AssertEquals("Previous instructions for CNR is CFS, so the CNR Delivery should NOT be IsEmpty", false, confirmationCNR.KK_IsEmptyContainer);
		}

		public void TestSetMT_Export_MultiContainer_AllContainers()
		{
			var booking = Helper.CreateBooking();

			var instructionCYD = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CYD);
			var instructionCNR = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNR);

			var container1 = booking.ConsolidationSingleJob.PackageJob.Packages.AddNew("CNT", "C1");
			var container2 = booking.ConsolidationSingleJob.PackageJob.Packages.AddNew("CNT", "C2");

			instructionCYD.DivotsWithPackages.AddPackage(container1);
			instructionCYD.DivotsWithPackages.AddPackage(container2);
			instructionCNR.DivotsWithPackages.AddPackage(container1);
			instructionCNR.DivotsWithPackages.AddPackage(container2);
			AssertEquals("Precondition: CNR should have no Confirmations", false, instructionCNR.Confirmations.Any());

			var confirmationCNR = instructionCNR.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);
			AssertEquals("Previous instruction is CYD for C1 and C2, so the CNR Delivery should be IsEmpty", true, confirmationCNR.KK_IsEmptyContainer);
		}

		public void TestSetMT_Export_MultiContainer_DifferentRoutes()
		{
			var booking = Helper.CreateBooking();

			var instructionCYD = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CYD);
			var instructionCNR = booking.Instructions.AddNew(InstructionTypes.Codes.Multi, OrganisationTypesList.Codes.CNR);
			var instructionCFS = booking.Instructions.AddNew(InstructionTypes.Codes.Multi, OrganisationTypesList.Codes.CFS);
			var instructionCTO = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CTO);

			var container1 = booking.ConsolidationSingleJob.PackageJob.Packages.AddNew("CNT", "C1");
			var container2 = booking.ConsolidationSingleJob.PackageJob.Packages.AddNew("CNT", "C2");

			var containerDivotCYD1 = Helper.CreatePackageDivot(instructionCYD, container1);
			var containerDivotCYD2 = Helper.CreatePackageDivot(instructionCYD, container2);
			var containerDivotCNR = Helper.CreatePackageDivot(instructionCNR, container1);
			var containerDivotCFS = Helper.CreatePackageDivot(instructionCFS, container2);
			var containerDivotCTO1 = Helper.CreatePackageDivot(instructionCTO, container1);
			var containerDivotCTO2 = Helper.CreatePackageDivot(instructionCTO, container2);
			AssertEquals("Precondition: CNR should have no Confirmations", false, instructionCNR.Confirmations.Any());

			var confirmationCYD1 = instructionCYD.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp);
			var confirmationCYD2 = instructionCYD.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp);
			var confirmationCNR = instructionCNR.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);
			var confirmationCFS = instructionCFS.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);
			var confirmationCTO = instructionCTO.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);

			confirmationCYD1.KK_KD_BookingInstructionPkgDivot = containerDivotCYD1.PK;
			confirmationCYD2.KK_KD_BookingInstructionPkgDivot = containerDivotCYD2.PK;
			confirmationCNR.KK_KD_BookingInstructionPkgDivot = containerDivotCNR.PK;
			confirmationCFS.KK_KD_BookingInstructionPkgDivot = containerDivotCFS.PK;

			AssertEquals("Before instructionCNR is instructionCYD for C1, so confirmationCNR should be IsEmpty", true, confirmationCNR.KK_IsEmptyContainer);
			AssertEquals("Before instructionCFS is instructionCYD for C2, so confirmationCFS should be IsEmpty", true, confirmationCFS.KK_IsEmptyContainer);
			AssertEquals("Before instructionCTO is instructionCNR/CFS and then instructionCYD for C1, so the confirmationCTO should not be IsEmpty", false, confirmationCTO.KK_IsEmptyContainer);
		}

		public void TestSetMT_Export_MultiContainer_OneDirectFromCYD_OtherViaCFS()
		{
			var booking = Helper.CreateBooking();

			var instructionCYD = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CYD);
			var instructionCFS = booking.Instructions.AddNew(InstructionTypes.Codes.Multi, OrganisationTypesList.Codes.CFS);
			var instructionCNR = booking.Instructions.AddNew(InstructionTypes.Codes.Multi, OrganisationTypesList.Codes.CNR);

			var container1 = booking.ConsolidationSingleJob.PackageJob.Packages.AddNew("CNT", "C1");
			var container2 = booking.ConsolidationSingleJob.PackageJob.Packages.AddNew("CNT", "C2");

			var containerDivotCYD1 = Helper.CreatePackageDivot(instructionCYD, container1);
			var containerDivotCYD2 = Helper.CreatePackageDivot(instructionCYD, container2); // direct
			var containerDivotCFS = Helper.CreatePackageDivot(instructionCFS, container1);
			var containerDivotCNR1 = Helper.CreatePackageDivot(instructionCNR, container1);
			var containerDivotCNR2 = Helper.CreatePackageDivot(instructionCNR, container2); // direct
			AssertEquals("Precondition: CNR should have no Confirmations", false, instructionCNR.Confirmations.Any());

			var confirmationCYD = instructionCYD.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp);
			var confirmationCFS = instructionCFS.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);
			var confirmationCNR1 = instructionCNR.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);
			var confirmationCNR2 = instructionCNR.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);

			AssertEquals("confirmationCNR1 is currently for 2 containers (ALL), each has a different preceeding Org Instruction, so MT should be false.", false, confirmationCNR1.KK_IsEmptyContainer);
			AssertEquals("confirmationCNR2 is currently for 2 containers (ALL), each has a different preceeding Org Instruction, so MT should be false.", false, confirmationCNR2.KK_IsEmptyContainer);

			confirmationCNR1.KK_KD_BookingInstructionPkgDivot = containerDivotCNR1.PK;
			confirmationCNR2.KK_KD_BookingInstructionPkgDivot = containerDivotCNR2.PK;

			AssertEquals("Before instructionCFS is instruction CYD for C1, so confirmationCFS should be IsEmpty", true, confirmationCFS.KK_IsEmptyContainer);
			AssertEquals("Before instructionCNR is instruction CFS for C1, so confirmationCNR should be IsEmpty", false, confirmationCNR1.KK_IsEmptyContainer);
			AssertEquals("Before instructionCNR is instruction CYD for C2, so confirmationCNR should be IsEmpty", true, confirmationCNR2.KK_IsEmptyContainer);
		}

		public void TestSetMT_Import_AddingDeliveryConfirmation()
		{
			var booking = Helper.CreateBooking();

			var instructionCNE = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNE);
			var instructionCYD = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CYD);

			var container = booking.ConsolidationSingleJob.PackageJob.Packages.AddNew("CNT", "C1");

			instructionCNE.DivotsWithPackages.AddPackage(container);
			instructionCYD.DivotsWithPackages.AddPackage(container);
			AssertEquals("Precondition: CNE should have no Confirmations", false, instructionCNE.Confirmations.Any());

			var confirmationCNE = instructionCNE.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery); // Should normally be a Pickup.
			AssertEquals("Next instruction is CYD for C1 but the CNE is a delivery not a pickup so should not be IsEmpty.", false, confirmationCNE.KK_IsEmptyContainer);
		}

		public void TestSetMT_Import_MultiAddress_CYD_CYD()
		{
			var booking = Helper.CreateBooking();

			var instructionCNE = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNE);
			var instructionCYD1 = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CYD);
			var instructionCYD2 = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CYD);

			var container = booking.ConsolidationSingleJob.PackageJob.Packages.AddNew("CNT", "C1");

			instructionCNE.DivotsWithPackages.AddPackage(container);
			instructionCYD1.DivotsWithPackages.AddPackage(container);
			instructionCYD2.DivotsWithPackages.AddPackage(container);
			AssertEquals("Precondition: CNE should have no Confirmations", false, instructionCNE.Confirmations.Any());

			var confirmationCNE = instructionCNE.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp);
			AssertEquals("Next instructions are CYD and CYD for C1, so the CNE Delivery should be IsEmpty", true, confirmationCNE.KK_IsEmptyContainer);
		}

		public void TestSetMT_Import_MultiContainer_AllContainers()
		{
			var booking = Helper.CreateBooking();

			var instructionCNE = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNE);
			var instructionCYD = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CYD);

			var container1 = booking.ConsolidationSingleJob.PackageJob.Packages.AddNew("CNT", "C1");
			var container2 = booking.ConsolidationSingleJob.PackageJob.Packages.AddNew("CNT", "C2");

			instructionCNE.DivotsWithPackages.AddPackage(container1);
			instructionCNE.DivotsWithPackages.AddPackage(container2);
			instructionCYD.DivotsWithPackages.AddPackage(container1);
			instructionCYD.DivotsWithPackages.AddPackage(container2);
			AssertEquals("Precondition: CNR should have no Confirmations", false, instructionCNE.Confirmations.Any());

			var confirmationCNE = instructionCNE.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp);
			AssertEquals("Next instruction is CYD for C1 and C2, so the CNE Delivery should be IsEmpty", true, confirmationCNE.KK_IsEmptyContainer);
		}

		public void TestSetMT_Import_MultiContainer_DifferentRoutes()
		{
			var booking = Helper.CreateBooking();

			var instructionCTO = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CTO);
			var instructionCFS = booking.Instructions.AddNew(InstructionTypes.Codes.Multi, OrganisationTypesList.Codes.CFS);
			var instructionCNE = booking.Instructions.AddNew(InstructionTypes.Codes.Multi, OrganisationTypesList.Codes.CNE);
			var instructionCYD = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CYD);

			var container1 = booking.ConsolidationSingleJob.PackageJob.Packages.AddNew("CNT", "C1");
			var container2 = booking.ConsolidationSingleJob.PackageJob.Packages.AddNew("CNT", "C2");

			var containerDivotCTO1 = Helper.CreatePackageDivot(instructionCTO, container1);
			var containerDivotCTO2 = Helper.CreatePackageDivot(instructionCTO, container2);
			var containerDivotCFS = Helper.CreatePackageDivot(instructionCFS, container2);
			var containerDivotCNE = Helper.CreatePackageDivot(instructionCNE, container1);
			var containerDivotCYD1 = Helper.CreatePackageDivot(instructionCYD, container1);
			var containerDivotCYD2 = Helper.CreatePackageDivot(instructionCYD, container2);
			AssertEquals("Precondition: CNE should have no Confirmations", false, instructionCNE.Confirmations.Any());

			var confirmationCTO = instructionCTO.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp);
			var confirmationCFS = instructionCFS.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp);
			var confirmationCNE = instructionCNE.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp);
			var confirmationCYD1 = instructionCYD.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);
			var confirmationCYD2 = instructionCYD.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);

			confirmationCFS.KK_KD_BookingInstructionPkgDivot = containerDivotCFS.PK;
			confirmationCNE.KK_KD_BookingInstructionPkgDivot = containerDivotCNE.PK;
			confirmationCYD1.KK_KD_BookingInstructionPkgDivot = containerDivotCYD1.PK;
			confirmationCYD2.KK_KD_BookingInstructionPkgDivot = containerDivotCYD2.PK;

			AssertEquals("After instructionCTO is instructionCNE/CFS and then instructionCYD, so confirmationCTO should not be IsEmpty", false, confirmationCTO.KK_IsEmptyContainer);
			AssertEquals("After instructionCFS is instructionCYD for package C2, so confirmationCFS should be IsEmpty", true, confirmationCFS.KK_IsEmptyContainer);
			AssertEquals("After instructionCNE is instructionCYD for package C1, so confirmationCNE should be IsEmpty", true, confirmationCNE.KK_IsEmptyContainer);
		}

		public void TestPublishEvents_Containerised_NoExceptionsWhenPublishingSameEventAgain()
		{
			var now = ZDateTime.Now;
			var pickupOrganisation = Helper.CreateOrganisation("CNR");
			var deliveryOrganisation = Helper.CreateOrganisation("CNE");
			var shipment = (IDtbBookingParent)Factory.New<Integration.Forwarding.IForwardingShipment>();
			var consolidation = Helper.CreateConsolidation(shipment);
			var booking = Helper.CreateBooking(consolidation);
			booking.KM_JobID = "T1";

			var pickupInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, pickupOrganisation.MainAddress);
			var pickupConfirmation = Helper.CreateConfirmation(pickupInstruction, ConfirmationTypes.Codes.PickUp);
			var divot = Helper.CreatePackageDivot(pickupInstruction, 1);
			var container = Helper.CreatePackageContainer("CN1", divot);
			container.KP_Sequence = 1;
			consolidation.PackageJob.Packages.Add(container);
			pickupConfirmation.KK_Actual = now;
			Factory.Save();
			var pupLog1 = pickupConfirmation.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.PickedUp.Code).Single();
			AssertEquals("T1, 1 CNT CN1", pupLog1.ReferenceFreeText);

			pickupConfirmation.KK_IsEmptyContainer = true;
			pickupConfirmation.KK_Actual = now.AddHours(1);
			Factory.Save();
			var pupLogs = pickupConfirmation.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.PickedUp.Code);
			AssertEquals("There should be two pickup logs now since the reference is different.", 2, pupLogs.Count());
			var oldPUPLog = pupLogs.Single(l => l.SL_EventTime == now);
			var newPUPLog = pupLogs.Single(l => l.SL_EventTime == now.AddHours(1));
			AssertEquals("T1, 1 CNT CN1", oldPUPLog.ReferenceFreeText);
			AssertEquals("T1, 1 CNT CN1", newPUPLog.ReferenceFreeText);

			// change back to original values
			pickupConfirmation.KK_IsEmptyContainer = false;
			pickupConfirmation.KK_Actual = now;
			AssertNoExceptionThrown("Should not throw exception since original event was published.", () => Factory.Save());

			var pupLogsAfterChagingToOriginalValues = pickupConfirmation.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.PickedUp.Code);
			AssertEquals("There should be two pickup logs now since the reference is changed back to the original value which has an event with matching reference.",
				2, pupLogsAfterChagingToOriginalValues.Count());
			var oldPUPLogAfterChangingToOriginalValues = pupLogsAfterChagingToOriginalValues.Single(l => l.SL_EventTime == now);
			var newPUPLogAfterChangingToOriginalValues = pupLogsAfterChagingToOriginalValues.Single(l => l.SL_EventTime == now.AddHours(1));
			AssertEquals("T1, 1 CNT CN1", oldPUPLogAfterChangingToOriginalValues.ReferenceFreeText);
			AssertEquals("T1, 1 CNT CN1", newPUPLogAfterChangingToOriginalValues.ReferenceFreeText);
		}

		public void TestPublishEvents_Containerised()
		{
			var now = ZDateTime.Now;
			var pickupOrganisation = Helper.CreateOrganisation("CNR");
			var deliveryOrganisation = Helper.CreateOrganisation("CNE");
			var shipment = (BusinessObject)Factory.New<Integration.Forwarding.IForwardingShipment>();
			var consolidation = Helper.CreateConsolidation((IDtbBookingParent)shipment);

			var booking = Helper.CreateBooking(consolidation);
			booking.KM_JobID = "T1";
			var pickupInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, pickupOrganisation.MainAddress);
			var pickupConfirmation = Helper.CreateConfirmation(pickupInstruction, ConfirmationTypes.Codes.PickUp);
			var divot1 = Helper.CreatePackageDivot(pickupInstruction, 1);
			var container1 = Helper.CreatePackageContainer("CN1", divot1);
			container1.KP_Sequence = 1;
			consolidation.PackageJob.Packages.Add(container1);
			AssertEventOnConfirmation(c => c.KK_Actual = now, pickupConfirmation, Events.PickedUp, "T1, 1 CNT CN1", now, CargoWise.EventReference.Constants.Facilities.Code.Consignor);
			AssertEventOnConfirmation(c => c.KK_ReceivedBy = "DGF", pickupConfirmation, Events.SignatureCaptured, "T1, 1 CNT CN1", now.AddDays(1), CargoWise.EventReference.Constants.Facilities.Code.Consignor, "DGF", Constants.EventReferenceParameterReasons.Pickup);

			booking.KM_TransportReference = "M1";
			var deliveryInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, deliveryOrganisation.MainAddress);
			var deliveryConfirmation = Helper.CreateConfirmation(deliveryInstruction, ConfirmationTypes.Codes.Delivery);
			var divot2 = Helper.CreatePackageDivot(deliveryInstruction, 1);
			var container2 = Helper.CreatePackageContainer("CN2", divot2);
			container2.KP_Sequence = 2;
			consolidation.PackageJob.Packages.Add(container2);
			AssertEventOnConfirmation(c => c.KK_Actual = now, deliveryConfirmation, Events.Delivered, "M1, 1 CNT CN2", now, CargoWise.EventReference.Constants.Facilities.Code.Consignee);
			AssertEventOnConfirmation(c => c.KK_ReceivedBy = "XYZ", deliveryConfirmation, Events.SignatureCaptured, "M1, 1 CNT CN2", now.AddDays(1), CargoWise.EventReference.Constants.Facilities.Code.Consignee, "XYZ", Constants.EventReferenceParameterReasons.Delivery);
			AssertEquals("Precondition", 0, GetLogs(shipment.GetLogs(), Events.PickedUp).Count());
			AssertEquals("Precondition", 0, GetLogs(shipment.GetLogs(), Events.Delivered).Count());
			AssertEquals("Precondition", 0, GetLogs(shipment.GetLogs(), Events.SignatureCaptured).Count());

			Factory.Save();
			var pickedUpLogs = GetLogs(shipment.GetLogs(), Events.PickedUp);
			AssertEquals(1, pickedUpLogs.Count());

			var deliveredLogs = GetLogs(shipment.GetLogs(), Events.Delivered);
			AssertEquals(1, deliveredLogs.Count());

			var singatureCapturedLogs = GetLogs(shipment.GetLogs(), Events.SignatureCaptured);
			AssertEquals(2, singatureCapturedLogs.Count());
		}

		public void TestPublishEvents_Containerised_LargeContainerReferences()
		{
			var now = ZDateTime.Now;
			var pickupOrganisation = Helper.CreateOrganisation("CNR");
			var deliveryOrganisation = Helper.CreateOrganisation("CNE");
			var shipment = (BusinessObject)Factory.New<Integration.Forwarding.IForwardingShipment>();
			var consolidation = Helper.CreateConsolidation((IDtbBookingParent)shipment);

			var booking = Helper.CreateBooking(consolidation);
			var pickupInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, pickupOrganisation.MainAddress);
			var pickupConfirmation = Helper.CreateConfirmation(pickupInstruction, ConfirmationTypes.Codes.PickUp);
			string containerFreeTextReference = "100 CNT";
			for (int count = 1; count <= 100; count++)
			{
				var divot1 = Helper.CreatePackageDivot(pickupInstruction, 1);
				var containerNumber = $"CONTAINER{count}";
				var container = Helper.CreatePackageContainer(containerNumber, divot1);
				container.KP_Sequence = (ZShort)count;
				consolidation.PackageJob.Packages.Add(container);
				containerFreeTextReference = $"{containerFreeTextReference} {containerNumber}";
			}

			var expectedReference = "100 CNT CONTAINER1, CONTAINER2, CONTAINER3, CONTAINER4, CONTAINER5, CONTAINER6, CONTAINER7, CONTAINER8, CONTAINER9, CONTAINER10, CONTAINER11, CONTAINER12, CONTAINER13, CONTAINER14, CONTAINER15, CONTAINER16, CONTAINER17, CONTAINER18, CONTAINER19, CONTAINER20, CONTAINER21, CONTAINER22, CONTAINER23, CONTAINER24, CONTAINER25, CONTAINER26, CONTAINER27, CONTAINER28, CONTAINER29, CONTAINER30, CONTAINER31, CONTAINER32, CONTAINER33, CONTAINER34, CONTAINER35, CONTAINER36, CONTAINER37, CONTAINER38, CONTAINER39, CONTAINER40, CONTAINER41, CONTAINER42, CONTAINER43, CONTAINER44, CONTAINER45, CONTAINER46, CONTAINER47, CONTAINER48, CONTAINER49, CONTAINER50, CONTAINER51, CONTAINER52, CONTAINER53, CONTAINER54, CONTAINER55, CONTAINER56, CONTAINER57, CONTAINER58, CONTAINER59, CONTAINER60, CONTAINER61, CONTAINER62, CONTAINER63, CONTAINER64, CONTAINER65, CONTAINER66, CONTAINER67, CONTAINER68, CONTAINER69, CONTAINER70, CONTAINER71, CONTAINER72, CONTAINER73, CONTAINER74, CONTAINER75, CONTAINER76, CONTAINER77, CONTAINER78, CONTAINER79";
			AssertEventOnConfirmation(c => c.KK_Actual = now, pickupConfirmation, Events.PickedUp, expectedReference, now, CargoWise.EventReference.Constants.Facilities.Code.Consignor);
			AssertEventOnConfirmation(c => c.KK_ReceivedBy = "DGF", pickupConfirmation, Events.SignatureCaptured, expectedReference, now.AddDays(1), CargoWise.EventReference.Constants.Facilities.Code.Consignor, "DGF", Constants.EventReferenceParameterReasons.Pickup);
		}

		IEnumerable<StmALog> GetLogs(Logs logs, Event eventType)
		{
			return logs.GetAllLogs().Cast<StmALog>().Where(l => l.SL_SE_NKEvent == eventType.Code);
		}

		public void TestEventsNotToBePublished_KK_Estimated_PickedUp()
		{
			TestEventsNotToBePublished_Core(AutoEvents.PickedUp, AutoEvents.Delivered, "KK_Estimated");
		}

		public void TestEventsNotToBePublished_KK_RequiredFrom()
		{
			TestEventsNotToBePublished_Core(AutoEvents.CutOffDate, AutoEvents.CutOffDate, "KK_RequiredFrom", "Pickup Required From", "Delivery Required From");
		}

		public void TestEventsNotToBePublished_KK_RequiredTo()
		{
			TestEventsNotToBePublished_Core(AutoEvents.CutOffDate, AutoEvents.CutOffDate, "KK_RequiredTo", "Pickup Required To", "Delivery Required To");
		}

		void TestEventsNotToBePublished_Core(Event pickupEvent, Event deliveryEvent, string propertyName, string expectedTypeForPickup = "", string expectedTypeForDelivery = "")
		{
			var now = ZDateTime.Now;
			var pickupOrganisation = Helper.CreateOrganisation("CNR");
			var deliveryOrganisation = Helper.CreateOrganisation("CNE");
			var shipment = (BusinessObject)Factory.New<Integration.Forwarding.IForwardingShipment>();
			var consolidation = Helper.CreateConsolidation((IDtbBookingParent)shipment);

			var booking = Helper.CreateBooking(consolidation);
			booking.KM_JobID = "T1";

			var pickupInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, pickupOrganisation.MainAddress);
			var pickupConfirmation = Helper.CreateConfirmation(pickupInstruction, ConfirmationTypes.Codes.PickUp);
			var divot1 = Helper.CreatePackageDivot(pickupInstruction, 1);
			var container1 = Helper.CreatePackageContainer("CN1", divot1);
			container1.KP_Sequence = 1;
			consolidation.PackageJob.Packages.Add(container1);
			AssertEventOnConfirmation(c => c.SetPropertyValue(propertyName, now), pickupConfirmation, pickupEvent, "T1, 1 CNT CN1", now, CargoWise.EventReference.Constants.Facilities.Code.Consignor, expectedType: expectedTypeForPickup);

			booking.KM_TransportReference = "M1";
			var deliveryInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, deliveryOrganisation.MainAddress);
			var deliveryConfirmation = Helper.CreateConfirmation(deliveryInstruction, ConfirmationTypes.Codes.Delivery);
			var divot2 = Helper.CreatePackageDivot(deliveryInstruction, 1);
			var container2 = Helper.CreatePackageContainer("CN2", divot2);
			container2.KP_Sequence = 2;
			consolidation.PackageJob.Packages.Add(container2);
			AssertEventOnConfirmation(c => c.SetPropertyValue(propertyName, now), deliveryConfirmation, deliveryEvent, "M1, 1 CNT CN2", now, CargoWise.EventReference.Constants.Facilities.Code.Consignee, expectedType: expectedTypeForDelivery);

			AssertEquals("Precondition", 0, GetLogs(shipment.GetLogs(), pickupEvent).Count());
			AssertEquals("Precondition", 0, GetLogs(shipment.GetLogs(), deliveryEvent).Count());

			Factory.Save();

			var pickupEventLogs = GetLogs(shipment.GetLogs(), pickupEvent);
			AssertEquals(0, pickupEventLogs.Count());

			var deliveryEventLogs = GetLogs(shipment.GetLogs(), deliveryEvent);
			AssertEquals(0, deliveryEventLogs.Count());
		}

		public void TestEventIsNotCancelled()
		{
			var expectedEvent = AutoEvents.CutOffDate;
			var now = ZDateTime.Now;
			var pickupOrganisation = Helper.CreateOrganisation("CNR");
			var shipment = (BusinessObject)Factory.New<Integration.Forwarding.IForwardingShipment>();
			var consolidation = Helper.CreateConsolidation((IDtbBookingParent)shipment);

			var booking = Helper.CreateBooking(consolidation);
			booking.KM_JobID = "T1";

			var pickupInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, pickupOrganisation.MainAddress);
			var pickupConfirmation = Helper.CreateConfirmation(pickupInstruction, ConfirmationTypes.Codes.PickUp);
			pickupConfirmation.KK_RequiredFrom = now;
			Factory.Save();

			AssertEventOnConfirmation(null, pickupConfirmation, expectedEvent, "T1", now, CargoWise.EventReference.Constants.Facilities.Code.Consignor, expectedType: "Pickup Required From");

			pickupConfirmation.KK_RequiredTo = now.AddDays(1);
			Factory.Save();

			var logs = pickupConfirmation.Logs.Find(l => l.SL_SE_NKEvent == expectedEvent.Code);
			AssertEquals("Expected two logs", 2, logs.Count());

			foreach (var log in logs)
			{
				AssertEquals("Expected log not to be cancelled", false, log.IsCancelled);
			}
		}

		public void TestEventIsNotIgnored()
		{
			var expectedEvent = AutoEvents.CutOffDate;
			var now = ZDateTime.Now;
			var pickupOrganisation = Helper.CreateOrganisation("CNR");
			var shipment = (BusinessObject)Factory.New<Integration.Forwarding.IForwardingShipment>();
			var consolidation = Helper.CreateConsolidation((IDtbBookingParent)shipment);

			var booking = Helper.CreateBooking(consolidation);
			booking.KM_JobID = "T1";

			var pickupInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, pickupOrganisation.MainAddress);
			var pickupConfirmation = Helper.CreateConfirmation(pickupInstruction, ConfirmationTypes.Codes.PickUp);
			pickupConfirmation.KK_RequiredFrom = now;
			pickupConfirmation.KK_RequiredTo = now.AddDays(1);

			var logFrom = pickupConfirmation.Logs.Find(l => l.SL_SE_NKEvent == expectedEvent.Code && l.Parameters.ContainsKey("TYP") && l.Parameters["TYP"] == "Pickup Required From").FirstOrDefault();
			AssertNotNull("Expected a log for pickup from", logFrom);
			AssertEquals("Expected log not to be cancelled", false, logFrom.IsCancelled);

			var logTo = pickupConfirmation.Logs.Find(l => l.SL_SE_NKEvent == expectedEvent.Code && l.Parameters.ContainsKey("TYP") && l.Parameters["TYP"] == "Pickup Required To").FirstOrDefault();
			AssertNotNull("Expected a log for pickup to", logTo);
			AssertEquals("Expected log not to be cancelled", false, logTo.IsCancelled);
		}

		public void TestLogsAreRemoved()
		{
			var expectedEvent = AutoEvents.CutOffDate;
			var now = ZDateTime.Now;
			var pickupOrganisation = Helper.CreateOrganisation("CNR");
			var shipment = (BusinessObject)Factory.New<Integration.Forwarding.IForwardingShipment>();
			var consolidation = Helper.CreateConsolidation((IDtbBookingParent)shipment);

			var booking = Helper.CreateBooking(consolidation);
			booking.KM_JobID = "T1";

			var pickupInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, pickupOrganisation.MainAddress);
			var pickupConfirmation = Helper.CreateConfirmation(pickupInstruction, ConfirmationTypes.Codes.PickUp);
			pickupConfirmation.KK_RequiredFrom = now;
			pickupConfirmation.KK_RequiredTo = now.AddDays(1);

			var logFrom = pickupConfirmation.Logs.Find(l => l.SL_SE_NKEvent == expectedEvent.Code && l.Parameters.ContainsKey("TYP") && l.Parameters["TYP"] == "Pickup Required From").FirstOrDefault();
			AssertNotNull("Expected a log for pickup from", logFrom);

			var logTo = pickupConfirmation.Logs.Find(l => l.SL_SE_NKEvent == expectedEvent.Code && l.Parameters.ContainsKey("TYP") && l.Parameters["TYP"] == "Pickup Required To").FirstOrDefault();
			AssertNotNull("Expected a log for pickup to", logTo);

			pickupConfirmation.KK_RequiredFrom = ZDateTime.Empty;

			var remainingLog = pickupConfirmation.Logs.Find(l => l.SL_SE_NKEvent == expectedEvent.Code && l.Parameters.ContainsKey("TYP") && l.Parameters["TYP"] == "Pickup Required To").FirstOrDefault();
			AssertNotNull("Expected remaining log for Pickup To Date", remainingLog);

			pickupConfirmation.KK_RequiredTo = ZDateTime.Empty;
			AssertEquals("Expected no more logs", 0, pickupConfirmation.Logs.GetAllLogs().Count);
		}

		public void TestBothRequiredAndActualLogs()
		{
			var expectedEvent = AutoEvents.PickedUp;
			var now = ZDateTime.Now;
			var pickupOrganisation = Helper.CreateOrganisation("CNR");
			var shipment = (BusinessObject)Factory.New<Integration.Forwarding.IForwardingShipment>();
			var consolidation = Helper.CreateConsolidation((IDtbBookingParent)shipment);

			var booking = Helper.CreateBooking(consolidation);
			booking.KM_JobID = "T1";

			var pickupInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, pickupOrganisation.MainAddress);
			var pickupConfirmation = Helper.CreateConfirmation(pickupInstruction, ConfirmationTypes.Codes.PickUp);
			pickupConfirmation.KK_Estimated = now;
			pickupConfirmation.KK_Actual = now.AddDays(1);

			var logEstimated = pickupConfirmation.Logs.Find(l => l.SL_SE_NKEvent == expectedEvent.Code && l.SL_IsEstimate).SingleOrDefault();
			AssertNotNull("Expected a log for estimated date", logEstimated);

			var logActual = pickupConfirmation.Logs.Find(l => l.SL_SE_NKEvent == expectedEvent.Code && !l.SL_IsEstimate).SingleOrDefault();
			AssertNotNull("Expected a log for actual date", logActual);

			pickupConfirmation.KK_Estimated = ZDateTime.Empty;

			var remainingLog = pickupConfirmation.Logs.Find(l => l.SL_SE_NKEvent == expectedEvent.Code && !l.SL_IsEstimate).SingleOrDefault();
			AssertNotNull("Expected remaining log for Actual Date", remainingLog);

			pickupConfirmation.KK_Actual = ZDateTime.Empty;
			AssertEquals("Expected no more logs", 0, pickupConfirmation.Logs.GetAllLogs().Count);
		}

		public void TestKK_Actual_Events_Department()
		{
			var pickupOrganisation = Helper.CreateOrganisation("CNR");
			var deliveryOrganisation = Helper.CreateOrganisation("CNE");
			var transportOrg = Helper.CreateOrganisation("TRF");
			transportOrg.OH_FullName = "TRF";
			var booking = Helper.CreateBooking();
			var pickupInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, pickupOrganisation.MainAddress);
			var pickupConfirmation = Helper.CreateConfirmation(pickupInstruction, ConfirmationTypes.Codes.PickUp);

			var deliveryInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, deliveryOrganisation.MainAddress);
			var deliveryConfirmation = Helper.CreateConfirmation(deliveryInstruction, ConfirmationTypes.Codes.Delivery);
			AssertEquals("Precondition", 0, pickupConfirmation.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.PickedUp.Code).Count());
			AssertEquals("Precondition", 0, deliveryConfirmation.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.Delivered.Code).Count());
			AssertEquals("Precondition", 0, pickupConfirmation.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.SignatureCaptured.Code).Count());
			AssertEquals("Precondition", 0, deliveryConfirmation.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.SignatureCaptured.Code).Count());

			AssertDepartmentParameter_KKActual(pickupConfirmation, AutoEvents.PickedUp);
			AssertDepartmentParameter_KKActual(deliveryConfirmation, AutoEvents.Delivered);
			AssertDepartmentParameter_KKReceivedBy(pickupConfirmation);
			AssertDepartmentParameter_KKReceivedBy(deliveryConfirmation);

			booking.Address.E2_OA_Address = transportOrg.MainAddress.PK;
			AssertDepartmentParameter_KKActual(pickupConfirmation, AutoEvents.PickedUp);
			AssertDepartmentParameter_KKActual(deliveryConfirmation, AutoEvents.Delivered);
			AssertDepartmentParameter_KKReceivedBy(pickupConfirmation);
			AssertDepartmentParameter_KKReceivedBy(deliveryConfirmation);
		}

		void AssertDepartmentParameter_KKActual(DtbBookingConfirmation confirmation, Event eventCode)
		{
			confirmation.KK_Actual = ZDateTime.Now.AddDays(-5);
			var log = confirmation.Logs.Find(l => l.SL_SE_NKEvent == eventCode.Code).Single();
			AssertEquals("Transport Provider", log.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department]);
		}

		void AssertDepartmentParameter_KKReceivedBy(DtbBookingConfirmation confirmation)
		{
			confirmation.KK_ReceivedBy = "DGF";
			var log = confirmation.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.SignatureCaptured.Code).Single();
			AssertEquals("Transport Provider", log.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department]);
		}

		public void TestIsConNoteNo()
		{
			AssertFlag("IsConNoteNo", ConfirmationTypes.Codes.ConNoteNo, "abc");
		}

		public void TestDelete_RefreshesInstructionConNoteNo()
		{
			AssertParentInstructionConNoteNoIsRefreshed("Deleting a ConNote Confirmation should refresh the parent Instruction's ConNote #.", c => c.Delete());
		}

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest()
		{
			var booking = Helper.CreateBooking();
			var instruction = booking.Instructions.AddNew();
			var confirmation = instruction.Confirmations.AddNew();

			return confirmation;
		}

		public void TestGetEstimatedDate()
		{
			DtbBookingConfirmation confirmation = Factory.New<DtbBookingConfirmation>();
			Assert(confirmation.KK_EstimatedInfo.ReadOnly);
		}

		public void TestBookingAutoPopulationPickupEvent()
		{
			AssertAutoPopulationOfEvents(JobContainerLegsSchema.JU_PickupTimeIn, JobContainerLegsSchema.JU_PickupTimeOut, AutoEvents.PickedUp, InstructionTypes.Codes.PickUp, ConfirmationTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR);
		}

		public void TestBookingAutoPopulationDeliveryEvent()
		{
			AssertAutoPopulationOfEvents(JobContainerLegsSchema.JU_DeliverTimeIn, JobContainerLegsSchema.JU_DeliverTimeOut, AutoEvents.Delivered, InstructionTypes.Codes.Delivery, ConfirmationTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE);
		}

		void AssertAutoPopulationOfEvents(SchemaDateTimeColumn timeIn, SchemaDateTimeColumn timeOut, Event expectedEvent, string instructionType, string confirmationType, string orgType)
		{
			var now = ZDateTime.Now;
			var organisation = SetupOrganisation("PORG", "2001");

			var booking = Helper.CreateBooking();
			booking.KM_JobID = "DGF1";

			var instruction = Helper.CreateInstruction(booking, instructionType, orgType, organisation.MainAddress);
			var confirmation = Helper.CreateConfirmation(instruction, confirmationType);
			var divot = Helper.CreatePackageDivot(instruction, 1);
			var container = Helper.CreatePackageContainer("CN1", divot);
			container.KP_Sequence = 1;
			booking.ConsolidationSingleJob.PackageJob.Packages.Add(container);

			var cartage = (BusinessObject)Factory.New<ICommonCartage>();
			cartage[JobCartageSchema.JJ_ParentTableCode] = DtbBookingSchema.Constants.Prefix;
			cartage[JobCartageSchema.JJ_ParentID] = booking.PK;
			cartage[JobCartageSchema.JJ_OrderReferenceNumber] = "DGF1";

			var commonContainer = (BusinessObject)Factory.New<ICommonContainer>();
			commonContainer[JobContainerSchema.JC_ContainerNum] = "CN1";

			var move = (BusinessObject)Factory.New<ICommonBookedCtgMove>();
			move[JobBookedCtgMoveSchema.EW_JJ] = cartage.PK;
			move[JobBookedCtgMoveSchema.EW_JC_Container] = commonContainer.PK;
			move[JobBookedCtgMoveSchema.EW_BookedPackCount] = 1;

			var cartageLeg = (BusinessObject)Factory.New<ICommonCartageLeg>();
			cartageLeg[JobContainerLegsSchema.JU_EW] = move.PK;
			AssertEquals("Precondition", false, FindEventReferences(cartageLeg.PK, expectedEvent).Any());
			Factory.Save();

			cartageLeg[timeIn] = now;
			cartageLeg[timeOut] = now.AddHours(1);
			AssertEquals($"Precondition : Cartage leg should have an {expectedEvent.ToString()} event.", true, FindEventReferences(cartageLeg.PK, expectedEvent).Any());
			AssertEquals("Precondition", ZDateTime.Empty, confirmation.KK_Actual);

			Factory.Save();
			AssertEquals(now.AddHours(1), confirmation.KK_Actual);
		}

		IEnumerable<string> FindEventReferences(ZGuid parentID, Event eventType)
		{
			return FindEvents(parentID, eventType).Select(e => e.SL_Reference.ToString());
		}

		IEnumerable<StmALog> FindEvents(ZGuid parentID, Event eventType)
		{
			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, eventType.Code);
			query.AddToFilter(StmALogSchema.SL_Parent, parentID);

			return Factory.Load<StmALog>(query);
		}

		public void TestGetBusinessObjectBaseTypeFromTablePrefix()
		{
			var prefix = DtbBookingConfirmationSchema.Constants.Prefix;
			var businessObjectType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(prefix, false);
			AssertEquals("Regression test: supplies type which never made it to production", "DtbConsignmentConfirmation", businessObjectType.Name);
		}

		// No interface called IDtbTransportConfirmation, therefore no need for TestGetTypeFromObjectFactory or TestCreateFromInterface

		public void TestInstruction()
		{
			var instruction1 = GetNewInstruction();
			var confirmation = instruction1.Confirmations.AddNew();
			AssertEquals("Confirmation.Instruction should be directly via ParentID.", instruction1, confirmation.Instruction);
			Assert("Confirmation.Instruction should not use ParentID", confirmation.KK_KD_BookingInstructionPkgDivot.IsEmpty);

			var instruction2 = GetNewInstruction();
			var package = Helper.CreatePackage("P1");
			var divot = CreatePackageDivot(instruction2, package, 1);
			confirmation.KK_KD_BookingInstructionPkgDivot = divot.PK;
			AssertEquals("Confirmation.Instruction is still via original.", instruction1, confirmation.Instruction);
		}

		public void TestPackageDivot()
		{
			var instruction1 = GetNewInstruction();
			var confirmation = instruction1.Confirmations.AddNew();
			AssertNull("Confirmations Parent is Instruction, and because the is no Selected Package, we don't know what divot to use, plus there isn't one", confirmation.PackageDivot);

			var package1 = Helper.CreatePackage("p1");
			var divot1 = CreatePackageDivot(instruction1, package1, 1);
			var package2 = Helper.CreatePackage("p2");
			var divot2 = CreatePackageDivot(instruction1, package2, 1);
			AssertNull("We now have divots, but still not in package view", confirmation.PackageDivot);

			var instruction2 = GetNewInstruction();
			var divot11 = CreatePackageDivot(instruction2, package1, 1);
			var divot22 = CreatePackageDivot(instruction2, package2, 1);
			confirmation.KK_KN_BookingInstruction = instruction2.PK;
			AssertNull("confirmation has been moved to instruction 2, still should show null", confirmation.PackageDivot);

			confirmation.KK_KD_BookingInstructionPkgDivot = divot11.PK;
			AssertEquals("parent is now divot11, so now show divot11", divot11, confirmation.PackageDivot);
		}

		DtbBookingInstructionPkgDivot CreatePackageDivot(DtbBookingInstruction instruction, PkgPackage package, int qty)
		{
			var result = instruction.PackageDivots.AddNew();
			result.KD_KP_Package = package.PK;
			result.KD_Quantity = qty;

			return result;
		}

		public void TestBooking()
		{
			var booking = GetNewBooking();
			var instruction = booking.Instructions.AddNew();
			var confirmation = instruction.Confirmations.AddNew();
			AssertEquals("Should be the Instruction's Booking", booking, confirmation.Booking);

			var confirmationNoTransport = (DtbBookingConfirmation)GetNewBusinessObject();
			AssertNull("Should be null and not blow up.", confirmationNoTransport.Booking);
		}

		public void TestKK_KD_BookingInstructionPkgDivot()
		{
			var instruction = GetNewInstruction();
			instruction.KN_Status = "";
			AssertEquals("Precondition", "", instruction.KN_Status);

			var confirmation = instruction.Confirmations.AddNew();
			AssertEquals(TransportStatuses.Codes.Available, instruction.KN_Status);
			AssertEquals("Parent is empty, therefore is for all package on the Instruction", true, confirmation.KK_KD_BookingInstructionPkgDivot.IsEmpty);
			AssertEquals("KK_KN_BookingInstruction equals the Instruction", instruction.PK, confirmation.KK_KN_BookingInstruction);

			var booking = Helper.CreateBooking();
			instruction = booking.Instructions.AddNew();
			var divot1 = instruction.PackageDivots.AddNew();
			var divot2 = instruction.PackageDivots.AddNew();
			var package1 = Factory.New<PkgPackage>();
			var package2 = Factory.New<PkgPackage>();
			divot1.KD_KP_Package = package1.PK;
			divot2.KD_KP_Package = package2.PK;
			confirmation = instruction.Confirmations.AddNew();

			AssertEquals("ParentID_InstructionOrPackage should be the same", instruction.PK, confirmation.ParentID_InstructionOrPackageDivot);
			AssertEquals("Parent is Instruction", instruction, confirmation.Instruction);
			AssertNull("PackageDivot should be null, because it's for ALL the instructions packages", confirmation.PackageDivot);

			confirmation.ParentID_InstructionOrPackageDivot = divot1.PK;
			AssertEquals("Parent is Divot1", divot1.PK, confirmation.KK_KD_BookingInstructionPkgDivot);
			AssertEquals("ParentID_InstructionOrPackage should be the same", divot1.PK, confirmation.ParentID_InstructionOrPackageDivot);
			AssertEquals("Gets the instruction through divot 1", instruction, confirmation.Instruction);
			AssertEquals("PackageDivot should be divot 1", divot1, confirmation.PackageDivot);

			var invalidDivot = Factory.New<DtbBookingInstructionPkgDivot>();
			confirmation.ParentID_InstructionOrPackageDivot = invalidDivot.PK;
			AssertEquals("Parent is Divot1", divot1.PK, confirmation.KK_KD_BookingInstructionPkgDivot);
			AssertEquals("ParentID_InstructionOrPackage should be that random divot", invalidDivot.PK, confirmation.ParentID_InstructionOrPackageDivot);
			AssertEquals("new divot isn't part of this confirmation, so should still get to original Instruction through divot 1", instruction, confirmation.Instruction);
			AssertEquals("PackageDivot should still be divot 1", divot1, confirmation.PackageDivot);

			confirmation.ParentID_InstructionOrPackageDivot = divot2.PK;
			AssertEquals("Parent is Divot2", divot2.PK, confirmation.KK_KD_BookingInstructionPkgDivot);
			AssertEquals("ParentID_InstructionOrPackage should be the same", divot2.PK, confirmation.ParentID_InstructionOrPackageDivot);
			AssertEquals("Gets the instruction through divot 2", instruction, confirmation.Instruction);
			AssertEquals("PackageDivot should be divot 2", divot2, confirmation.PackageDivot);

			confirmation.ParentID_InstructionOrPackageDivot = ZGuid.Empty;
			AssertEquals("Parent is Divot2", divot2.PK, confirmation.KK_KD_BookingInstructionPkgDivot);
			AssertEquals("ParentID_InstructionOrPackage should be empty", ZGuid.Empty, confirmation.ParentID_InstructionOrPackageDivot);
			AssertEquals("Cannot set to empty, so still gets the instruction through divot 2", instruction, confirmation.Instruction);
			AssertEquals("PackageDivot should be divot 2", divot2, confirmation.PackageDivot);

			confirmation.ParentID_InstructionOrPackageDivot = instruction.PK;
			AssertEquals("Parent is instruction", ZGuid.Empty, confirmation.KK_KD_BookingInstructionPkgDivot);
			AssertEquals("ParentID_InstructionOrPackage should be the same", instruction.PK, confirmation.ParentID_InstructionOrPackageDivot);
			AssertEquals("Parent is back to Instruction", instruction, confirmation.Instruction);
			AssertNull("PackageDivot should be null, because it's for ALL the instructions packages", confirmation.PackageDivot);

			confirmation.ParentID_InstructionOrPackageDivot = ZGuid.Invalid;
			AssertEquals("Parent is instruction", ZGuid.Empty, confirmation.KK_KD_BookingInstructionPkgDivot);
			AssertEquals("ParentID_InstructionOrPackage should be invalid", ZGuid.Invalid, confirmation.ParentID_InstructionOrPackageDivot);
			AssertEquals("Cannot set to invalid, so instruction should still be the parent", instruction, confirmation.Instruction);
			AssertNull("PackageDivot should be null, because it's for ALL the instructions packages", confirmation.PackageDivot);

			confirmation.ParentID_InstructionOrPackageDivot = instruction.PK;
			AssertEquals("Parent is instruction", instruction.PK, confirmation.KK_KN_BookingInstruction);
			AssertEquals("Parent is instruction", true, confirmation.KK_KD_BookingInstructionPkgDivot.IsEmpty);
			AssertEquals("ParentID_InstructionOrPackage should be the same", instruction.PK, confirmation.ParentID_InstructionOrPackageDivot);
			AssertEquals("Parent is back to Instruction", instruction, confirmation.Instruction);
			AssertNull("PackageDivot should be null, because it's for ALL the instructions packages", confirmation.PackageDivot);

			confirmation.ParentID_InstructionOrPackageDivot = ZGuid.Missing;
			AssertEquals("Parent is instruction", instruction.PK, confirmation.KK_KN_BookingInstruction);
			AssertEquals("Parent is instruction", true, confirmation.KK_KD_BookingInstructionPkgDivot.IsEmpty);
			AssertEquals("ParentID_InstructionOrPackage should be reset to KK_KD_BookingInstructionPkgDivot", instruction.PK, confirmation.ParentID_InstructionOrPackageDivot);
			AssertEquals("Parent is back to Instruction", instruction, confirmation.Instruction);
			AssertNull("PackageDivot should be null, because it's for ALL the instructions packages", confirmation.PackageDivot);
		}

		public void TestKK_RequiredFromUtc()
		{
			var year = ZDateTime.Now.Year;

			var std = new ZDateTime(year, 9, 27, 10, 0, 0);
			var dst = new ZDateTime(year, 12, 25, 10, 0, 0);
			var instruction = GetNewInstruction();
			instruction.KN_InstructionType = InstructionTypes.Codes.PickUp;
			var confirmation = Helper.CreateConfirmation(instruction, ConfirmationTypes.Codes.PickUp);
			var org = Helper.CreateOrganisation("Org1");
			instruction.Address.E2_OA_Address = org.MainAddress.PK;
			org.OH_RL_NKClosestPort = "AUBNE";
			confirmation.KK_RequiredFrom = std;
			AssertEquals(Env.Time.GetUtcFromUnlocoTime(org.OH_RL_NKClosestPort, std.ToDateTime()), confirmation.KK_RequiredFromUtc);

			org.OH_RL_NKClosestPort = "USORD";
			confirmation.KK_RequiredFrom = std;
			AssertEquals(Env.Time.GetUtcFromUnlocoTime(org.OH_RL_NKClosestPort, std.ToDateTime()), confirmation.KK_RequiredFromUtc);

			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
			instruction.Address.E2_AddressOverride = true;
			confirmation.KK_RequiredFrom = std;
			AssertEquals(Env.Time.GetUtcFromUnlocoTime("AUSYD", std.ToDateTime()), confirmation.KK_RequiredFromUtc);
		}

		public void TestKK_RequiredToUtc()
		{
			var today = DateTime.Today;
			var instruction = GetNewInstruction();
			instruction.KN_InstructionType = InstructionTypes.Codes.PickUp;
			var confirmation = Helper.CreateConfirmation(instruction, ConfirmationTypes.Codes.PickUp);
			var org = Helper.CreateOrganisation("Org1");
			instruction.Address.E2_OA_Address = org.MainAddress.PK;
			org.OH_RL_NKClosestPort = "AUBNE";
			confirmation.KK_RequiredTo = today;
			AssertEquals(Env.Time.GetUtcFromUnlocoTime(org.OH_RL_NKClosestPort, today), confirmation.KK_RequiredToUtc);

			org.OH_RL_NKClosestPort = "USORD";
			confirmation.KK_RequiredTo = today;
			AssertEquals(Env.Time.GetUtcFromUnlocoTime(org.OH_RL_NKClosestPort, today), confirmation.KK_RequiredToUtc);

			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
			instruction.Address.E2_AddressOverride = true;
			confirmation.KK_RequiredTo = today;
			AssertEquals(Env.Time.GetUtcFromUnlocoTime("AUSYD", today), confirmation.KK_RequiredToUtc);
		}

		public void TestKK_EstimatedUtc()
		{
			var year = ZDateTime.Now.Year;

			var std = new ZDateTime(year, 9, 27, 10, 0, 0);
			var dst = new ZDateTime(year, 12, 25, 10, 0, 0);
			var instruction = GetNewInstruction();
			instruction.KN_InstructionType = InstructionTypes.Codes.PickUp;
			var confirmation = Helper.CreateConfirmation(instruction, ConfirmationTypes.Codes.PickUp);
			var org = Helper.CreateOrganisation("Org1");
			instruction.Address.E2_OA_Address = org.MainAddress.PK;
			org.OH_RL_NKClosestPort = "AUBNE";
			confirmation.KK_Estimated = std;
			AssertEquals(Env.Time.GetUtcFromUnlocoTime(org.OH_RL_NKClosestPort, std.ToDateTime()), confirmation.KK_EstimatedUtc);

			org.OH_RL_NKClosestPort = "USORD";
			confirmation.KK_Estimated = std;
			AssertEquals(Env.Time.GetUtcFromUnlocoTime(org.OH_RL_NKClosestPort, std.ToDateTime()), confirmation.KK_EstimatedUtc);

			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
			instruction.Address.E2_AddressOverride = true;
			confirmation.KK_Estimated = std;
			AssertEquals(Env.Time.GetUtcFromUnlocoTime("AUSYD", std.ToDateTime()), confirmation.KK_EstimatedUtc);
		}

		[TestDate(2016, 06, 10)]
		public void TestKK_Actual_PUPEvent()
		{
			AssertKK_ActualEvents(AutoEvents.PickedUp, LocalCartageJobOrgTypeList.Codes.CNR,
				InstructionTypes.Codes.PickUp, ConfirmationTypes.Codes.PickUp,
				CargoWise.EventReference.Constants.Facilities.Code.Consignor, Constants.EventReferenceParameterReasons.Pickup);
		}

		[TestDate(2016, 06, 10)]
		public void TestKK_Actual_DLVEvent()
		{
			AssertKK_ActualEvents(AutoEvents.Delivered, LocalCartageJobOrgTypeList.Codes.CNE,
				InstructionTypes.Codes.Delivery, ConfirmationTypes.Codes.Delivery,
				CargoWise.EventReference.Constants.Facilities.Code.Consignee, Constants.EventReferenceParameterReasons.Delivery);
		}

		void AssertKK_ActualEvents(Event eventCode, string orgType,
			string instructionType, string confirmationType,
			string facility, string expectedReason)
		{
			var now = ZDateTime.Now;
			Env.Registry.FreightWeightUnit = Constants.Weight.Grams;
			Env.Registry.FreightVolumeUnit = Constants.Volume.CubicCentimeters;
			var pickupOrganisation1 = SetupOrganisation("CNR", "Sydney");
			var pickupOrganisation2 = SetupOrganisation("CNR", "Alexandria");
			var booking = GetNewBooking();
			booking.KM_TransportReference = "T1";
			booking.KM_JobID = "J1";
			var pickupInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, orgType, pickupOrganisation1.MainAddress);
			Helper.CreateConfirmation(pickupInstruction, ConfirmationTypes.Codes.PickUp);

			var instruction1ForConsignor = Helper.CreateInstruction(booking, instructionType, orgType, pickupOrganisation1.MainAddress);
			var instruction2ForConsignor = Helper.CreateInstruction(booking, instructionType, orgType, null);
			var divot1 = Helper.CreatePackageDivot(instruction1ForConsignor, 1);
			var divot2 = Helper.CreatePackageDivot(instruction1ForConsignor, 3);
			var containerWithDivot = Helper.CreatePackageContainer("CN1", divot1);
			var containerForInstruction = Helper.CreatePackageContainer("CN2");
			CreatePackage(divot2, "P2", 2, Constants.PkgUnit.Box, 0.2m, 0.005m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

			var confirmation = Helper.CreateConfirmation(instruction1ForConsignor, confirmationType);
			var confirmationWithoutLocation = Helper.CreateConfirmation(instruction2ForConsignor, confirmationType);
			var confirmationWithContainerisedPackage = Helper.CreateConfirmation(divot1, confirmationType);
			var confirmationWithLoosePackage = Helper.CreateConfirmation(divot2, confirmationType);

			// set Received by before setting actual date
			confirmation.KK_ReceivedBy = "DGF";
			AssertEventOnConfirmation(c => c.KK_Actual = now.AddDays(1), confirmation, eventCode, "T1, 3 PCE 0.005 M3 0.2 KG", now.AddDays(1), facility);
			AssertEventOnConfirmation(null, confirmation, AutoEvents.SignatureCaptured, "T1, 3 PCE 0.005 M3 0.2 KG", now.AddDays(1), facility, "DGF", expectedReason);
			AssertEventOnConfirmation(c => c.KK_Actual = now.AddDays(2), confirmation, AutoEvents.SignatureCaptured, "T1, 3 PCE 0.005 M3 0.2 KG", now.AddDays(2), facility, "DGF", expectedReason);

			booking.KM_TransportReference = "";
			AssertEventOnConfirmation(c => c.KK_Actual = now.AddDays(2), confirmationWithoutLocation, eventCode, "J1", now.AddDays(2), facility);
			AssertEquals(0, confirmationWithoutLocation.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.SignatureCapturedCode).Count());
			AssertEventOnConfirmation(c => c.KK_ReceivedBy = "XYZ", confirmationWithoutLocation, AutoEvents.SignatureCaptured, "J1", now.AddDays(2), facility);

			instruction1ForConsignor.Address.E2_OA_Address = pickupOrganisation2.MainAddress.PK;
			AssertEventOnConfirmation(c => c.KK_Actual = now.AddDays(3), confirmationWithContainerisedPackage, eventCode, "J1, 1 CNT CN1", now.AddDays(3), facility);
			AssertEventOnConfirmation(c => c.KK_Actual = now.AddDays(4), confirmationWithLoosePackage, eventCode, "J1, 2 BOX 0.005 M3 0.2 KG", now.AddDays(4), facility);
			AssertEquals(0, confirmationWithContainerisedPackage.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.SignatureCapturedCode).Count());
			AssertEquals(0, confirmationWithLoosePackage.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.SignatureCapturedCode).Count());

			confirmationWithLoosePackage.KK_ReceivedBy = "XYZ";
			AssertEventOnConfirmation(c => c.KK_Actual = now, confirmationWithLoosePackage, AutoEvents.SignatureCaptured, "J1, 2 BOX 0.005 M3 0.2 KG", now, facility, "XYZ", expectedReason);
			AssertEventOnConfirmation(c => c.KK_ReceivedBy = "DGF", confirmationWithLoosePackage, AutoEvents.SignatureCaptured, "J1, 2 BOX 0.005 M3 0.2 KG", now, facility, "DGF", expectedReason);

			AssertEquals("Precondition: Should have one signature captured event log", 1, confirmation.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.SignatureCapturedCode).Count());
			AssertEquals($"Precondition: Should have one ${eventCode.Description} event log", 1, confirmation.Logs.Find(l => l.SL_SE_NKEvent == eventCode.Code).Count());
			confirmation.KK_ReceivedBy = "";
			AssertEquals($"No signature captured event logs expected for empty receiver", 0, confirmation.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.SignatureCapturedCode).Count());
			AssertEquals(1, confirmation.Logs.Find(l => l.SL_SE_NKEvent == eventCode.Code).Count());

			confirmation.KK_Actual = ZDateTime.Empty;
			AssertEquals($"No ${eventCode.Description} event logs expected for empty actual date", 0, confirmation.Logs.Find(l => l.SL_SE_NKEvent == eventCode.Code).Count());

			AssertEquals("Precondition: Should have one signature captured event log", 1, confirmationWithLoosePackage.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.SignatureCapturedCode).Count());
			AssertEquals($"Precondition: Should have one ${eventCode.Description} event log", 1, confirmationWithLoosePackage.Logs.Find(l => l.SL_SE_NKEvent == eventCode.Code).Count());
			confirmationWithLoosePackage.KK_Actual = ZDateTime.Empty;
			AssertEquals("No signature captured event logs expected for empty actual date", 0, confirmationWithLoosePackage.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.SignatureCapturedCode).Count());
			AssertEquals($"No ${eventCode.Description} event logs expected for empty actual date", 0, confirmationWithLoosePackage.Logs.Find(l => l.SL_SE_NKEvent == eventCode.Code).Count());
		}

		[TestDate(2016, 06, 10)]
		public void TestKK_Actual_Cleared_PUPEvent_Cancelled()
		{
			AssertKK_ActualClearedEvents(AutoEvents.PickedUp, LocalCartageJobOrgTypeList.Codes.CNR,
				InstructionTypes.Codes.PickUp, ConfirmationTypes.Codes.PickUp);
		}

		[TestDate(2016, 06, 10)]
		public void TestKK_Actual_Cleared_DLVEvent_Cancelled()
		{
			AssertKK_ActualClearedEvents(AutoEvents.Delivered, LocalCartageJobOrgTypeList.Codes.CNE,
				InstructionTypes.Codes.Delivery, ConfirmationTypes.Codes.Delivery);
		}

		void AssertKK_ActualClearedEvents(Event eventCode, string orgType,
			string instructionType, string confirmationType)
		{
			var now = ZDateTime.Now;
			var organisation = SetupOrganisation("CNR", "Sydney");
			var booking = GetNewBooking();
			booking.FillWithValidTestData();

			var instruction = Helper.CreateInstruction(booking, instructionType, orgType, organisation.MainAddress);

			var confirmation = Helper.CreateConfirmation(instruction, confirmationType);

			confirmation.KK_Actual = now.AddDays(1);

			Factory.Save();

			AssertEquals($"Precondition: No cancelled {eventCode.Description} event log expected", 0, confirmation.Logs.Find(l => l.SL_SE_NKEvent == eventCode.Code && l.IsCancelled).Count());
			AssertEquals($"Precondition: Non-cancelled {eventCode.Description} event log expected", 1, confirmation.Logs.Find(l => l.SL_SE_NKEvent == eventCode.Code && !l.IsCancelled).Count());

			confirmation.KK_Actual = ZDateTime.Empty;
			AssertEquals($"Cancelled {eventCode.Description} event log expected for empty actual date", 1, confirmation.Logs.Find(l => l.SL_SE_NKEvent == eventCode.Code && l.IsCancelled).Count());
			AssertEquals($"No non-cancelled {eventCode.Description} event log expected for empty actual date", 0, confirmation.Logs.Find(l => l.SL_SE_NKEvent == eventCode.Code && !l.IsCancelled).Count());
		}

		[TestDate(2016, 06, 10)]
		public void TestKK_Estimated_PUPEvent()
		{
			Test_DatePropertyEvents_Core(AutoEvents.PickedUp, "KK_Estimated", LocalCartageJobOrgTypeList.Codes.CNR, InstructionTypes.Codes.PickUp);
		}

		[TestDate(2016, 06, 10)]
		public void TestKK_Estimated_DLVEvent()
		{
			Test_DatePropertyEvents_Core(AutoEvents.Delivered, "KK_Estimated", LocalCartageJobOrgTypeList.Codes.CNE, InstructionTypes.Codes.Delivery);
		}

		[TestDate(2016, 06, 10)]
		public void TestKK_Estimated_Cleared_PUPEvent_Cancelled()
		{
			AssertKK_EstimatedClearedEvents(AutoEvents.PickedUp, LocalCartageJobOrgTypeList.Codes.CNR,
				InstructionTypes.Codes.PickUp, ConfirmationTypes.Codes.PickUp);
		}

		[TestDate(2016, 06, 10)]
		public void TestKK_Estimated_Cleared_DLVEvent_Cancelled()
		{
			AssertKK_EstimatedClearedEvents(AutoEvents.Delivered, LocalCartageJobOrgTypeList.Codes.CNE,
				InstructionTypes.Codes.Delivery, ConfirmationTypes.Codes.Delivery);
		}

		void AssertKK_EstimatedClearedEvents(Event eventCode, string orgType,
			string instructionType, string confirmationType)
		{
			var now = ZDateTime.Now;
			var organisation = SetupOrganisation("CNR", "Sydney");
			var booking = GetNewBooking();
			booking.FillWithValidTestData();

			var instruction = Helper.CreateInstruction(booking, instructionType, orgType, organisation.MainAddress);

			var confirmation = Helper.CreateConfirmation(instruction, confirmationType);

			confirmation.KK_Estimated = now.AddDays(1);

			Factory.Save();

			AssertEquals($"Precondition: No cancelled {eventCode.Description} event log expected", 0, confirmation.Logs.Find(l => l.SL_SE_NKEvent == eventCode.Code && l.IsCancelled && l.SL_IsEstimate).Count());
			AssertEquals($"Precondition: Non-cancelled {eventCode.Description} event log expected", 1, confirmation.Logs.Find(l => l.SL_SE_NKEvent == eventCode.Code && !l.IsCancelled && l.SL_IsEstimate).Count());

			confirmation.KK_Estimated = ZDateTime.Empty;
			AssertEquals($"Cancelled {eventCode.Description} event log expected for empty estimated date", 1, confirmation.Logs.Find(l => l.SL_SE_NKEvent == eventCode.Code && l.IsCancelled && l.SL_IsEstimate).Count());
			AssertEquals($"No non-cancelled {eventCode.Description} event log expected for empty estimated date", 0, confirmation.Logs.Find(l => l.SL_SE_NKEvent == eventCode.Code && !l.IsCancelled && l.SL_IsEstimate).Count());
		}

		[TestDate(2016, 06, 10)]
		public void TestKK_RequiredFrom_PUPEvent()
		{
			Test_DatePropertyEvents_Core(AutoEvents.CutOffDate, "KK_RequiredFrom", LocalCartageJobOrgTypeList.Codes.CNR, InstructionTypes.Codes.PickUp, expectedType: "Pickup Required From");
		}

		[TestDate(2016, 06, 10)]
		public void TestKK_RequiredFrom_DLVEvent()
		{
			Test_DatePropertyEvents_Core(AutoEvents.CutOffDate, "KK_RequiredFrom", LocalCartageJobOrgTypeList.Codes.CNE, InstructionTypes.Codes.Delivery, expectedType: "Delivery Required From");
		}

		[TestDate(2016, 06, 10)]
		public void TestKK_RequiredTo_PUPEvent()
		{
			Test_DatePropertyEvents_Core(AutoEvents.CutOffDate, "KK_RequiredTo", LocalCartageJobOrgTypeList.Codes.CNR, InstructionTypes.Codes.PickUp, expectedType: "Pickup Required To");
		}

		[TestDate(2016, 06, 10)]
		public void TestKK_RequiredTo_DLVEvent()
		{
			Test_DatePropertyEvents_Core(AutoEvents.CutOffDate, "KK_RequiredTo", LocalCartageJobOrgTypeList.Codes.CNE, InstructionTypes.Codes.Delivery, expectedType: "Delivery Required To");
		}

		[TestDate(2016, 06, 10)]
		public void TestNonActualNonEstimated_Cleared_EventNotCancelled()
		{
			AssertNonActualNonEstimatedClearedEvents(AutoEvents.CutOffDate, LocalCartageJobOrgTypeList.Codes.CNR,
				InstructionTypes.Codes.PickUp, ConfirmationTypes.Codes.PickUp);
		}

		void AssertNonActualNonEstimatedClearedEvents(Event eventCode, string orgType,
			string instructionType, string confirmationType)
		{
			var now = ZDateTime.Now;
			var organisation = SetupOrganisation("CNR", "Sydney");
			var booking = GetNewBooking();
			booking.FillWithValidTestData();

			var instruction = Helper.CreateInstruction(booking, instructionType, orgType, organisation.MainAddress);

			var confirmation = Helper.CreateConfirmation(instruction, confirmationType);

			confirmation.KK_RequiredFrom = now.AddDays(1);

			Factory.Save();

			AssertEquals($"Precondition: No cancelled {eventCode.Description} event log expected", 0, confirmation.Logs.Find(l => l.SL_SE_NKEvent == eventCode.Code && l.IsCancelled).Count());
			AssertEquals($"Precondition: Non-cancelled {eventCode.Description} event log expected", 1, confirmation.Logs.Find(l => l.SL_SE_NKEvent == eventCode.Code && !l.IsCancelled).Count());

			confirmation.KK_RequiredFrom = ZDateTime.Empty;
			AssertEquals($"No cancelled {eventCode.Description} event log expected for empty date", 0, confirmation.Logs.Find(l => l.SL_SE_NKEvent == eventCode.Code && l.IsCancelled).Count());
			AssertEquals($"{eventCode.Description} event log should remain uncancelled for empty date", 1, confirmation.Logs.Find(l => l.SL_SE_NKEvent == eventCode.Code && !l.IsCancelled).Count());
		}

		void Test_DatePropertyEvents_Core(Event eventInfo, string propertyName, string orgType, string instructionType, string expectedType = "")
		{
			var now = ZDateTime.Now;
			var isPickup = instructionType == InstructionTypes.Codes.PickUp;
			var confirmationType = isPickup ? ConfirmationTypes.Codes.PickUp : ConfirmationTypes.Codes.Delivery;
			var facility = orgType == LocalCartageJobOrgTypeList.Codes.CNE ? CargoWise.EventReference.Constants.Facilities.Code.Consignee : CargoWise.EventReference.Constants.Facilities.Code.Consignor;
			Env.Registry.FreightWeightUnit = Constants.Weight.Grams;
			Env.Registry.FreightVolumeUnit = Constants.Volume.CubicCentimeters;

			var pickupOrganisation1 = SetupOrganisation("SYD", "Sydney");
			var pickupOrganisation2 = SetupOrganisation("ALX", "Alexandria");
			var booking = GetNewBooking();
			booking.KM_TransportReference = "T1";
			booking.KM_JobID = "J1";

			var pickupInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, orgType, pickupOrganisation1.MainAddress);
			Helper.CreateConfirmation(pickupInstruction, ConfirmationTypes.Codes.PickUp);

			var instruction1ForConsignor = Helper.CreateInstruction(booking, instructionType, orgType, pickupOrganisation1.MainAddress);
			var instruction2ForConsignor = Helper.CreateInstruction(booking, instructionType, orgType, null);

			var divot1 = Helper.CreatePackageDivot(instruction1ForConsignor, 1);
			Helper.CreatePackageContainer("CN1", divot1);

			var divot2 = Helper.CreatePackageDivot(instruction1ForConsignor, 3);
			Helper.CreatePackageContainer("CN2");

			CreatePackage(divot2, "P2", 2, Constants.PkgUnit.Box, 0.2m, 0.005m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

			var confirmation = Helper.CreateConfirmation(instruction1ForConsignor, confirmationType);
			confirmation.KK_ReceivedBy = "DGF";

			var confirmationWithoutLocation = Helper.CreateConfirmation(instruction2ForConsignor, confirmationType);
			var confirmationWithContainerisedPackage = Helper.CreateConfirmation(divot1, confirmationType);
			var confirmationWithLoosePackage = Helper.CreateConfirmation(divot2, confirmationType);

			// Assertions
			AssertEventOnConfirmation(c => c[propertyName] = now.AddDays(1), confirmation, eventInfo, "T1, 3 PCE 0.005 M3 0.2 KG", now.AddDays(1), facility, expectedType: expectedType);

			booking.KM_TransportReference = "";
			AssertEventOnConfirmation(c => c[propertyName] = now.AddDays(2), confirmationWithoutLocation, eventInfo, "J1", now.AddDays(2), facility, expectedType: expectedType);

			instruction1ForConsignor.Address.E2_OA_Address = pickupOrganisation2.MainAddress.PK;
			AssertEventOnConfirmation(c => c[propertyName] = now.AddDays(3), confirmationWithContainerisedPackage, eventInfo, "J1, 1 CNT CN1", now.AddDays(3), facility, expectedType: expectedType);
			AssertEventOnConfirmation(c => c[propertyName] = now.AddDays(4), confirmationWithLoosePackage, eventInfo, "J1, 2 BOX 0.005 M3 0.2 KG", now.AddDays(4), facility, expectedType: expectedType);

			confirmation.KK_ReceivedBy = "";
			AssertEquals("No event logs expected for empty receiver", 1, confirmation.Logs.Find(l => l.SL_SE_NKEvent == eventInfo.Code).Count());

			confirmation[propertyName] = ZDateTime.Empty;
			AssertEquals("No event logs expected for empty date", 0, confirmation.Logs.Find(l => l.SL_SE_NKEvent == eventInfo.Code).Count());

			AssertEquals("Precondition: Should have one event log", 1, confirmationWithLoosePackage.Logs.Find(l => l.SL_SE_NKEvent == eventInfo.Code).Count());

			confirmationWithLoosePackage[propertyName] = ZDateTime.Empty;
			AssertEquals("No event logs expected for empty estimated date", 0, confirmationWithLoosePackage.Logs.Find(l => l.SL_SE_NKEvent == eventInfo.Code).Count());
		}

		void CreatePackage(DtbBookingInstructionPkgDivot divot, string packageID, int quantity, string packageType, decimal weight, decimal volume, string weightUQ, string volumeUQ)
		{
			var package = Helper.CreatePackage(packageID, divot, quantity);
			package.KP_F3_NKPackType = packageType;
			package.KP_WeightUQ = weightUQ;
			package.KP_Weight = weight;
			package.KP_VolumeUQ = volumeUQ;
			package.KP_Volume = volume;
		}

		protected OrgHeader SetupOrganisation(ZString code, ZString city)
		{
			var organisation = Helper.CreateOrganisation(code);
			organisation.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			organisation.MainAddress.OA_City = city;
			return organisation;
		}

		protected void AssertEventOnConfirmation(Action<DtbBookingConfirmation> actionToPerform, DtbBookingConfirmation confirmation, Event eventCode,
			string expectedReference, ZDateTime actual, string expectedFacility, string expectedName = "", string expectedReason = "", string expectedType = "")
		{
			actionToPerform?.Invoke(confirmation);

			var log = confirmation.Logs.Find(l => l.SL_SE_NKEvent == eventCode.Code).SingleOrDefault();
			AssertNotNull("No event log was found", log);
			AssertEquals("The event log reference is incorrect.", expectedReference, log.ReferenceFreeText);

			Assert("The event log should contain a FACILITY parameter.", log.Parameters.ContainsKey(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Facility));
			AssertEquals("Incorrect FACILITY in event log reference.", expectedFacility, log.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Facility]);

			if (!string.IsNullOrEmpty(expectedName))
			{
				Assert("The event log should contain a NAME parameter.", log.Parameters.ContainsKey(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Name));
				AssertEquals("Incorrect NAME in event log reference.", expectedName, log.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Name]);
			}

			if (!string.IsNullOrEmpty(expectedReason))
			{
				Assert("The event log should contain a REASON parameter.", log.Parameters.ContainsKey(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason));
				AssertEquals("Incorrect REASON in event log reference.", expectedReason, log.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason]);
			}

			if (!string.IsNullOrEmpty(expectedType))
			{
				Assert("The event log should contain a TYPE parameter.", log.Parameters.ContainsKey(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type));
				AssertEquals("Incorrect TYPE in event log reference.", expectedType, log.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type]);
			}
		}

		public void TestTypeParameter_IsContainerised()
		{
			var booking = GetNewBooking();
			var pickupInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var deliveryInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			var pickupConfirmation = Helper.CreateConfirmation(pickupInstruction, ConfirmationTypes.Codes.PickUp);
			var deliveryConfirmation = Helper.CreateConfirmation(deliveryInstruction, ConfirmationTypes.Codes.Delivery);

			var container = Factory.New<PkgPackage>();
			container.KP_F3_NKPackType = Constants.PkgUnit.Container;

			Helper.CreatePackageDivot(pickupInstruction, container, 1);
			Helper.CreatePackageDivot(deliveryInstruction, container, 1);
			Assert("Precondition", !pickupConfirmation.KK_IsEmptyContainer);
			Assert("Precondition", !deliveryConfirmation.KK_IsEmptyContainer);
			pickupConfirmation.KK_Actual = ZDateTime.Now.AddDays(1);
			deliveryConfirmation.KK_Actual = ZDateTime.Now.AddDays(2);
			AssertType(pickupConfirmation, AutoEvents.PickedUp, Constants.EventReferenceParameterTypes.FullContainer, true);
			AssertType(deliveryConfirmation, AutoEvents.Delivered, Constants.EventReferenceParameterTypes.FullContainer, true);

			pickupConfirmation.KK_IsEmptyContainer = true;
			pickupConfirmation.KK_Actual = ZDateTime.Now.AddDays(3);
			AssertType(pickupConfirmation, AutoEvents.PickedUp, Constants.EventReferenceParameterTypes.EmptyContainer, true);

			deliveryConfirmation.KK_IsEmptyContainer = true;
			deliveryConfirmation.KK_Actual = ZDateTime.Now.AddDays(4);
			AssertType(deliveryConfirmation, AutoEvents.Delivered, Constants.EventReferenceParameterTypes.EmptyContainer, true);
		}

		public void TestTypeParameter_IsLoose()
		{
			var booking = GetNewBooking();
			var pickupInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var deliveryInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			var pickupConfirmation = Helper.CreateConfirmation(pickupInstruction, ConfirmationTypes.Codes.PickUp);
			var deliveryConfirmation = Helper.CreateConfirmation(deliveryInstruction, ConfirmationTypes.Codes.Delivery);

			var container = Factory.New<PkgPackage>();
			container.KP_F3_NKPackType = Constants.PkgUnit.Box;

			Helper.CreatePackageDivot(pickupInstruction, container, 1);
			Helper.CreatePackageDivot(deliveryInstruction, container, 1);
			Assert("Precondition", !pickupConfirmation.KK_IsEmptyContainer);
			Assert("Precondition", !deliveryConfirmation.KK_IsEmptyContainer);
			pickupConfirmation.KK_Actual = ZDateTime.Now.AddDays(1);
			deliveryConfirmation.KK_Actual = ZDateTime.Now.AddDays(2);
			AssertType(pickupConfirmation, AutoEvents.PickedUp);
			AssertType(deliveryConfirmation, AutoEvents.Delivered);
		}

		static void AssertType(DtbBookingConfirmation confirmation, Event eventCode, string parameterType = "", bool expectParameterType = false)
		{
			var log = confirmation.Logs.Find(l => l.SL_SE_NKEvent == eventCode.Code).Single();
			AssertEquals(expectParameterType, log.Parameters.ContainsKey(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type));
			if (expectParameterType)
			{
				AssertEquals(parameterType, log.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type]);
			}
		}

		public void TestIsDelivery()
		{
			AssertFlag("IsDelivery", ConfirmationTypes.Codes.Delivery, "abc");
		}

		public void TestIsPickup()
		{
			AssertFlag("IsPickUp", ConfirmationTypes.Codes.PickUp, "abc");
		}

		public void TestIsDepot()
		{
			var booking = GetNewBooking();
			var instruction = booking.Instructions.AddNew();
			var confirmation = instruction.Confirmations.AddNew();
			instruction.OrganisationType = "CNR";
			AssertEquals(false, confirmation.IsOwnDepot);

			instruction.OrganisationType = "CFS";
			AssertEquals(false, confirmation.IsOwnDepot);

			instruction.KN_InstructionType = InstructionTypes.Codes.Multi;
			AssertEquals(true, confirmation.IsOwnDepot);

			instruction.OrganisationType = "CNE";
			AssertEquals(false, confirmation.IsOwnDepot);
		}

		protected void AssertFlag(ZString flagPropertyName, ZString validCode, ZString invalidCode)
		{
			var confirmation = (DtbBookingConfirmation)GetNewBusinessObject();
			AssertEquals(false, confirmation[flagPropertyName]);

			confirmation.KK_ConfirmationType = validCode;
			AssertEquals(true, confirmation[flagPropertyName]);

			confirmation.KK_ConfirmationType = invalidCode;
			AssertEquals(false, confirmation[flagPropertyName]);
		}

		public void TestIConsignmentAction_Properties()
		{
			var instruction = GetNewInstruction();
			instruction.KN_InstructionType = InstructionTypes.Codes.PickUp;
			var confirmation = Helper.CreateConfirmation(instruction, ConfirmationTypes.Codes.PickUp);

			var now = ZDateTime.Now;
			confirmation.KK_Estimated = now.AddMinutes(1);
			confirmation.KK_RequiredFrom = now.AddMinutes(2);
			confirmation.KK_RequiredTo = now.AddMinutes(3);
			confirmation.KK_ReferenceNum = "KK1";
			SetActualQuantity(confirmation, 2);

			AssertEquals(confirmation.KK_Quantity, confirmation.ActionQuantity);
			AssertEquals(instruction, confirmation.ConsignmentAddress);
			AssertEquals(now.AddMinutes(1), confirmation.Estimated);
			AssertEquals(now.AddMinutes(2), confirmation.RequiredFrom);
			AssertEquals(now.AddMinutes(3), confirmation.RequiredTo);
			AssertEquals(ConfirmationTypes.Codes.PickUp, confirmation.ActionType);
			AssertEquals("KK1", confirmation.ReferenceNumber);
			AssertEquals(false, confirmation.IsEmptyContainer);
		}

		public void TestNewConfirmationSetsMasterFlagAndVersionIfGrandparentBookingIsMaster()
		{
			var booking = Helper.CreateBooking();
			booking.KM_IsMaster = booking.ConsolidationSingleJob.KB_IsMaster = true;
			booking.KM_MasterBookingVersion = booking.ConsolidationSingleJob.KB_MasterBookingVersion = (short)1;
			var instruction = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			instruction.FillWithValidTestData();
			var confirmation = Helper.CreateConfirmation(instruction, ConfirmationTypes.Codes.PickUp);
			confirmation.FillWithValidTestData();

			Factory.Save();

			CombineAssertions("Master booking fields on confirmation should be set correctly to be a master booking as grandparent Booking is a master booking", () =>
			{
				AssertEquals("KK_IsMaster should be set to true as parent Booking.KM_IsMaster == true", true, confirmation.KK_IsMaster);
				AssertEquals("KK_MasterBookingVersion should be set to 1 as parent Booking.KM_IsMaster == true", (short)1, confirmation.KK_MasterBookingVersion);
			});
		}

		public void TestNewConfirmationBlanksMasterFlagAndVersionIfGrandparentBookingIsNotMaster()
		{
			var booking = Helper.CreateBooking();
			CombineAssertions("Precondition: Master booking fields on booking should have defaulted to not master booking", () =>
			{
				AssertEquals("Precondition: KM_IsMaster defaults to false on DtbBooking", false, booking.KM_IsMaster);
				AssertEquals("Precondition: KM_MasterBookingVersion defaults to 0 on DtbBooking", (short)0, booking.KM_MasterBookingVersion);
				AssertEquals("Precondition: KM_KM_MasterBooking defaults to empty on DtbBooking", ZGuid.Empty, booking.KM_KM_MasterBooking);
			});
			var instruction = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			instruction.FillWithValidTestData();
			var confirmation = Helper.CreateConfirmation(instruction, ConfirmationTypes.Codes.PickUp);
			confirmation.FillWithValidTestData();

			Factory.Save();

			CombineAssertions("Master booking fields on confirmation should be set correctly for a non-master booking as grandparent Booking is a master booking", () =>
			{
				AssertEquals("KK_IsMaster should be set to false as grandparent Booking.KM_IsMaster == false", false, confirmation.KK_IsMaster);
				AssertEquals("KK_MasterBookingVersion should be set to 0 as grandparent Booking.KM_IsMaster == false", (short)0, confirmation.KK_MasterBookingVersion);
			});
		}

		public void TestNewConfirmationBlanksMasterFlagAndVersionIfGrandparentBookingIsNotMasterEvenIfMasterBookingFieldsIncorrectlySet()
		{
			var booking = Helper.CreateBooking();
			CombineAssertions("Precondition: Master booking fields on booking should have defaulted to not master booking", () =>
			{
				AssertEquals("Precondition: KM_IsMaster defaults to false on DtbBooking", false, booking.KM_IsMaster);
				AssertEquals("Precondition: KM_MasterBookingVersion defaults to 0 on DtbBooking", (short)0, booking.KM_MasterBookingVersion);
				AssertEquals("Precondition: KM_KM_MasterBooking defaults to empty on DtbBooking", ZGuid.Empty, booking.KM_KM_MasterBooking);
			});
			var instruction = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			instruction.FillWithValidTestData();
			var confirmation = Helper.CreateConfirmation(instruction, ConfirmationTypes.Codes.PickUp);
			confirmation.FillWithValidTestData();
			confirmation.KK_IsMaster = true;
			confirmation.KK_MasterBookingVersion = (short)10;

			Factory.Save();

			CombineAssertions("Master booking fields on confirmation should be set correctly for a non-master booking as grandparent Booking is a master booking", () =>
			{
				AssertEquals("KK_IsMaster should be set to false as grandparent Booking.KM_IsMaster == false", false, confirmation.KK_IsMaster);
				AssertEquals("KK_MasterBookingVersion should be set to 0 as grandparent Booking.KM_IsMaster == false", (short)0, confirmation.KK_MasterBookingVersion);
			});
		}

		public void TestSubBookingConfirmationUpdatesMasterBookingVersionIfNewAndNotYetSet()
		{
			var masterConsolidation = Helper.CreateConsolidation();
			var masterBooking = Helper.CreateBooking(masterConsolidation);
			masterBooking.KM_IsMaster = true;
			Factory.Save();
			var masterBookingInstruction = masterBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			masterBookingInstruction.FillWithValidTestData();
			masterBookingInstruction.KN_IsMaster = true;
			var masterBookingConfirmation = Helper.CreateConfirmation(masterBookingInstruction, ConfirmationTypes.Codes.PickUp);
			masterBookingConfirmation.FillWithValidTestData();
			masterBookingConfirmation.KK_IsMaster = true;
			Factory.Save();

			var subConsolidation = Helper.CreateConsolidation();
			subConsolidation.KB_KB_MasterBookingConsolidation = masterConsolidation.PK;
			subConsolidation.KB_MasterBookingVersion = (short)1;
			var subBooking = Helper.CreateBooking(subConsolidation);
			subBooking.KM_KM_MasterBooking = masterBooking.PK;
			subBooking.KM_MasterBookingVersion = (short)1;
			var subBookingInstruction = subBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			subBookingInstruction.FillWithValidTestData();
			subBookingInstruction.KN_KN_MasterBookingInstruction = masterBookingInstruction.PK;
			subBookingInstruction.KN_MasterBookingVersion = (short)1;
			var subBookingConfirmation = Helper.CreateConfirmation(subBookingInstruction, ConfirmationTypes.Codes.PickUp);
			subBookingConfirmation.FillWithValidTestData();
			subBookingConfirmation.KK_KK_MasterBookingConfirmation = masterBookingConfirmation.PK;

			AssertEquals("Precondition: sub booking confirmation MasterBookingVersion has not yet been set (still 0)", ZShort.Zero, subBookingConfirmation.KK_MasterBookingVersion);
			Factory.Save();

			AssertEquals("Sub booking confirmation MasterBookingVersion should have updated to 1", (short)1, subBookingConfirmation.KK_MasterBookingVersion);
		}

		public void TestSubBookingConfirmationDoesNotUpdateMasterBookingVersionIfNewButAlreadySet()
		{
			var masterConsolidation = Helper.CreateConsolidation();
			var masterBooking = Helper.CreateBooking(masterConsolidation);
			masterBooking.KM_IsMaster = true;
			Factory.Save();
			var masterBookingInstruction = masterBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			masterBookingInstruction.FillWithValidTestData();
			masterBookingInstruction.KN_IsMaster = true;
			var masterBookingConfirmation = Helper.CreateConfirmation(masterBookingInstruction, ConfirmationTypes.Codes.PickUp);
			masterBookingConfirmation.FillWithValidTestData();
			masterBookingConfirmation.KK_IsMaster = true;
			Factory.Save();

			masterConsolidation.KB_MasterBookingVersion = masterBooking.KM_MasterBookingVersion = masterBookingInstruction.KN_MasterBookingVersion = (short)2;

			var subConsolidation = Helper.CreateConsolidation();
			subConsolidation.KB_KB_MasterBookingConsolidation = masterConsolidation.PK;
			subConsolidation.KB_MasterBookingVersion = (short)1;
			var subBooking = Helper.CreateBooking(subConsolidation);
			subBooking.KM_KM_MasterBooking = masterBooking.PK;
			subBooking.KM_MasterBookingVersion = (short)1;
			var subBookingInstruction = subBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			subBookingInstruction.FillWithValidTestData();
			subBookingInstruction.KN_KN_MasterBookingInstruction = masterBookingInstruction.PK;
			subBookingInstruction.KN_MasterBookingVersion = (short)1;
			var subBookingConfirmation = Helper.CreateConfirmation(subBookingInstruction, ConfirmationTypes.Codes.PickUp);
			subBookingConfirmation.FillWithValidTestData();
			subBookingConfirmation.KK_KK_MasterBookingConfirmation = masterBookingConfirmation.PK;
			subBookingConfirmation.KK_MasterBookingVersion = (short)2;
			Factory.Save();

			AssertEquals("Sub booking confirmation MasterBookingVersion should have stayed at 2 (process that creates sub probably populated it from the master)", (short)2, subBookingConfirmation.KK_MasterBookingVersion);
		}

		void SetActualQuantity(DtbBookingConfirmation confirmation, int quantity)
		{
			var instruction = confirmation.Instruction;
			var divot = instruction.PackageDivots.AddNew();
			var package = Factory.New<PkgPackage>();
			package.KP_PackageQty = quantity;
			divot.KD_KP_Package = package.PK;
			divot.KD_Quantity = quantity;
			confirmation.KK_Quantity = quantity;
		}

		public void TestDeleteBookingConfirmationDeletesSubBookingConfirmation()
		{
			var masterConsolidation = Helper.CreateConsolidation();
			var masterBooking = Helper.CreateBooking(masterConsolidation);
			masterBooking.KM_IsMaster = true;
			Factory.Save();
			var masterBookingInstruction = masterBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			masterBookingInstruction.FillWithValidTestData();
			masterBookingInstruction.KN_IsMaster = true;
			var masterBookingConfirmation = Helper.CreateConfirmation(masterBookingInstruction, ConfirmationTypes.Codes.PickUp);
			masterBookingConfirmation.FillWithValidTestData();
			masterBookingConfirmation.KK_IsMaster = true;
			Factory.Save();

			var subConsolidation = Helper.CreateConsolidation();
			subConsolidation.KB_KB_MasterBookingConsolidation = masterConsolidation.PK;
			subConsolidation.KB_MasterBookingVersion = (short)1;
			var subBooking = Helper.CreateBooking(subConsolidation);
			subBooking.KM_KM_MasterBooking = masterBooking.PK;
			subBooking.KM_MasterBookingVersion = (short)1;
			var subBookingInstruction = subBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			subBookingInstruction.FillWithValidTestData();
			subBookingInstruction.KN_KN_MasterBookingInstruction = masterBookingInstruction.PK;
			subBookingInstruction.KN_MasterBookingVersion = (short)1;
			var subBookingConfirmation = Helper.CreateConfirmation(subBookingInstruction, ConfirmationTypes.Codes.PickUp);
			subBookingConfirmation.FillWithValidTestData();
			subBookingConfirmation.KK_KK_MasterBookingConfirmation = masterBookingConfirmation.PK;

			AssertEquals("Precondition: sub booking confirmation MasterBookingVersion has not yet been set (still 0)", ZShort.Zero, subBookingConfirmation.KK_MasterBookingVersion);
			Factory.Save();

			masterBookingConfirmation.Delete();

			Assert(subBookingConfirmation.IsDeleted);
		}

		public void TestKK_OC_DriverResourceStringDataAttribute()
		{
			var confirmation = Factory.New<DtbBookingConfirmation>();

			var driverResourceStringDataAttribute = confirmation.KK_OC_DriverInfo.GetAttribute<ResourceStringDataAttribute>();
			AssertEquals($"{nameof(confirmation.KK_OC_Driver)} should have the correct caption", "Staff Driver", driverResourceStringDataAttribute.Caption);
		}

		public void TestDriverNameResourceStringDataAttribute()
		{
			var confirmation = Factory.New<DtbBookingConfirmation>();

			var resourceStringAttribute = (ResourceStringDataAttribute)confirmation.GetType().GetProperty("DriverName").GetCustomAttributes(false).Where(a => a is ResourceStringDataAttribute).Single();
			AssertEquals($"{nameof(confirmation.DriverName)} should have the correct caption", "Driver Name", resourceStringAttribute.Caption);
		}

		public void TestDriverNameReadOnlyAttribute()
		{
			var confirmation = Factory.New<DtbBookingConfirmation>();
			var readonlyAttribute = (ReadOnlyMemberAttribute)confirmation.GetType().GetProperty("DriverName").GetCustomAttributes(false).Where(a => a is ReadOnlyMemberAttribute).Single();
			readonlyAttribute.Member.ToArray();
			AssertEquals("DriverNameIsReadOnly", readonlyAttribute.Member);
		}
		public void TestDriverNameWhenDriverIsNull()
		{
			var confirmation = Factory.New<DtbBookingConfirmation>();

			var adHocDriverName = "Ad Hoc Driver Name";
			confirmation.KK_AdHocDriverName = adHocDriverName;

			AssertEquals("Driver should be null", null, confirmation.Driver);
			AssertEquals($"{nameof(confirmation.DriverName)} should return the value of {nameof(confirmation.KK_AdHocDriverName)}", adHocDriverName, confirmation.DriverName);
		}

		public void TestDriverNameWhenDriverIsNotNull()
		{
			var booking = Helper.CreateBooking();
			var instruction = booking.Instructions.AddNew();
			var confirmation = instruction.Confirmations.AddNew();

			confirmation.KK_AdHocDriverName = "Ad Hoc Driver Name";

			var driver = Factory.NewWithValidTestData<OrgContact>();
			var driverName = "Mr Driver";
			driver.OC_ContactName = driverName;
			confirmation.KK_OC_Driver = driver.PK;

			AssertEquals($"{nameof(confirmation.DriverName)} should return the selected Driver's name", driverName, confirmation.DriverName);
		}

		public void TestSettingDriverNameSetsKK_AdHocDriverName()
		{
			var booking = Helper.CreateBooking();
			var instruction = booking.Instructions.AddNew();
			var confirmation = instruction.Confirmations.AddNew();

			confirmation.KK_AdHocDriverName = "Old Driver Name";

			var newDriverName = "New Driver Name";
			confirmation.DriverName = newDriverName;

			AssertEquals($"Setting {nameof(confirmation.DriverName)} should have set the value of {nameof(confirmation.KK_AdHocDriverName)}", newDriverName, confirmation.KK_AdHocDriverName);
		}

		DtbBookingInstruction GetNewInstruction()
		{
			return Factory.New<DtbBookingInstruction>();
		}

		DtbBooking GetNewBooking()
		{
			return Factory.New<DtbBooking>();
		}

		void AssertParentInstructionConNoteNoIsRefreshed(string message, Action<DtbBookingConfirmation> actionThatShouldResultInConNoteNoRefresh)
		{
			var instruction = Helper.CreateInstruction(InstructionTypes.Codes.Delivery);
			var confirmation = Helper.CreateConfirmation(instruction, ConfirmationTypes.Codes.ConNoteNo);
			confirmation.KK_ReferenceNum = "abc";
			AssertEquals("Precondition", "abc", instruction.ConNoteNo);

			bool instructionConNoteNoRefreshBindingFired = false;
			instruction.ConNoteNoInfo.ValueChanged += (sender, e) => instructionConNoteNoRefreshBindingFired = true;

			actionThatShouldResultInConNoteNoRefresh(confirmation);
			AssertEquals(message, true, instructionConNoteNoRefreshBindingFired); // this assertion is the real test
			AssertEquals("", instruction.ConNoteNo);
		}

		public void TestUpdateCO2eStatusToNotCurrent_WhenIsEmptyContainerChanged()
		{
			// Arrange
			var booking = Helper.CreateBooking();
			var instruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var confirmation1 = Helper.CreateConfirmation(instruction, ConfirmationTypes.Codes.PickUp);
			booking.SetCO2eStatus(CO2eStatusList.Codes.Current);
			Factory.Save();
			AssertNoWarning(booking.TotalCO2eForBindingInfo, CO2eTestHelper.CO2Warning);

			// Act & Assert
			CO2eTestHelper.AssertHasCO2Warning(booking, () => confirmation1.KK_IsEmptyContainer = true, "KK_IsEmptyContainer [N]->[Y]");
			Factory.Save();
			CO2eTestHelper.AssertHasCO2Warning(booking, () => confirmation1.KK_IsEmptyContainer = false, "KK_IsEmptyContainer [Y]->[N]");
		}

		protected new TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = GetNewTestHelper()); }
		}

		TransportBookingTestHelper helper;

		protected new TransportBookingTestHelper GetNewTestHelper()
		{
			return new TransportBookingTestHelper(Factory);
		}
	}

	[TestedType(typeof(DtbBookingConfirmation))]
	sealed class DtbBookingConfirmationMasterBookingEntityTest : BaseIDtbMasterBookingEntityTest
	{
		protected override IEnumerable<SchemaColumn> ReplicatedColumns => DtbMasterBookingReplication.DtbBookingConfirmationReplicatedColumns;
		protected override IEnumerable<SchemaColumn> NonReplicatedColumns => new SchemaColumn[]
		{
			DtbBookingConfirmationSchema.PK,
			DtbBookingConfirmationSchema.KK_SystemCreateUser,
			DtbBookingConfirmationSchema.KK_SystemCreateTimeUtc,
			DtbBookingConfirmationSchema.KK_SystemLastEditUser,
			DtbBookingConfirmationSchema.KK_SystemLastEditTimeUtc,
			DtbBookingConfirmationSchema.KK_IsMaster,
			DtbBookingConfirmationSchema.KK_MasterBookingVersion,
			DtbBookingConfirmationSchema.KK_KK_MasterBookingConfirmation,
			DtbBookingConfirmationSchema.KK_KN_BookingInstruction,
			DtbBookingConfirmationSchema.KK_IsEmptyContainer,
			DtbBookingConfirmationSchema.KK_K1_RunSheetInstruction,
			DtbBookingConfirmationSchema.KK_KD_BookingInstructionPkgDivot,
			DtbBookingConfirmationSchema.KK_Quantity,
			DtbBookingConfirmationSchema.KK_AdHocDriverName
		};

		protected override ITableSchema tableSchema => DtbBookingConfirmationSchema.Instance;

		public void TestMasterBookingConfirmationUpdateToConfirmationTypeUpdatesMasterBookingVersion()
		{
			CoreTestMasterBookingConfirmationUpdateToReplicationFieldUpdatesMasterBookingVersion(DtbBookingConfirmationSchema.KK_ConfirmationType, ConfirmationTypes.Codes.Delivery);
		}

		public void TestMasterBookingConfirmationUpdateToEstimatedAndEstimatedUtcUpdatesMasterBookingVersion()
		{
			CoreTestMasterBookingConfirmationUpdateToReplicationFieldUpdatesMasterBookingVersion(DtbBookingConfirmationSchema.KK_Estimated, ZDateTime.Now, " (as well as KK_EstimatedUtc - changes when update KK_Estimated)");
		}

		public void TestMasterBookingConfirmationUpdateToActualUpdatesMasterBookingVersion()
		{
			CoreTestMasterBookingConfirmationUpdateToReplicationFieldUpdatesMasterBookingVersion(DtbBookingConfirmationSchema.KK_Actual, ZDateTime.Now);
		}

		public void TestMasterBookingConfirmationUpdateToRequiredToAndFromAndUtcDatesUpdatesMasterBookingVersion()
		{
			CoreTestMasterBookingConfirmationUpdateToReplicationFieldUpdatesMasterBookingVersion(DtbBookingConfirmationSchema.KK_RequiredFrom, ZDateTime.Now, " (as well as KK_RequiredTo - cannot change from without ensuring to is greater than it, KK_RequiredFromUtc and KK_RequiredToUtc - change when you change the non-Utc dates)");
		}

		public void TestMasterBookingConfirmationUpdateToReferenceNumUpdatesMasterBookingVersion()
		{
			CoreTestMasterBookingConfirmationUpdateToReplicationFieldUpdatesMasterBookingVersion(DtbBookingConfirmationSchema.KK_ReferenceNum, "ChangedRefNum");
		}

		public void TestMasterBookingConfirmationUpdateToReceivedByUpdatesMasterBookingVersion()
		{
			CoreTestMasterBookingConfirmationUpdateToReplicationFieldUpdatesMasterBookingVersion(DtbBookingConfirmationSchema.KK_ReceivedBy, "Me");
		}

		public void TestMasterBookingConfirmationUpdateToReceivedBySignatureUpdatesMasterBookingVersion()
		{
			CoreTestMasterBookingConfirmationUpdateToReplicationFieldUpdatesMasterBookingVersion(DtbBookingConfirmationSchema.KK_ReceivedBySignature, ZBlob.FromAscii("I was here"));
		}

		public void TestMasterBookingConfirmationUpdateToSlotDateTimeUpdatesMasterBookingVersion()
		{
			CoreTestMasterBookingConfirmationUpdateToReplicationFieldUpdatesMasterBookingVersion(DtbBookingConfirmationSchema.KK_SlotDateTime, ZDateTime.Now);
		}

		public void TestMasterBookingConfirmationUpdateToSlotReferenceUpdatesMasterBookingVersion()
		{
			CoreTestMasterBookingConfirmationUpdateToReplicationFieldUpdatesMasterBookingVersion(DtbBookingConfirmationSchema.KK_SlotReference, "NewSlotRef");
		}

		public void TestMasterBookingConfirmationUpdateToDriverUpdatesMasterBookingVersion()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ANOTHERORG";
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_OH = org.PK;
			contact.OC_ContactName = "Mr Driver";
			Factory.Save();
			CoreTestMasterBookingConfirmationUpdateToReplicationFieldUpdatesMasterBookingVersion(DtbBookingConfirmationSchema.KK_OC_Driver, contact.PK);
		}

		public void TestMasterBookingConfirmationUpdateToVehicleRegistrationUpdatesMasterBookingVersion()
		{
			CoreTestMasterBookingConfirmationUpdateToReplicationFieldUpdatesMasterBookingVersion(DtbBookingConfirmationSchema.KK_VehicleRegistration, "VEH002");
		}

		void CoreTestMasterBookingConfirmationUpdateToReplicationFieldUpdatesMasterBookingVersion(SchemaColumn columnChanged, object updatedValue, string extraFieldChangedMessage = "")
		{
			var bookingConfirmation = CreateBookingConfirmationForMasterBookingReplicationTests(true);

			var previousMasterBookingVersion = bookingConfirmation.KK_MasterBookingVersion;

			if (columnChanged == DtbBookingConfirmationSchema.KK_IsEmptyContainer)
			{
				bookingConfirmation.Instruction.OrganisationType = OrganisationTypesList.Codes.CYD;
			}
			bookingConfirmation[columnChanged] = updatedValue;
			if (columnChanged == DtbBookingConfirmationSchema.KK_RequiredFrom)
			{
				bookingConfirmation.KK_RequiredTo = bookingConfirmation.KK_RequiredFrom.AddDays(7);
			}

			var propertyInfo = bookingConfirmation.FindPropertyInfo(columnChanged.Name);
			CombineAssertions("Preconditions for master booking version update", () =>
			{
				Assert("Precondition: BookingConfirmation.HasChanges is true", bookingConfirmation.HasChanges);
				Assert("Precondition: BookingConfirmation." + columnChanged.Name + " value has changed" + extraFieldChangedMessage, propertyInfo.HasChanges);
			});
			Factory.Save();

			AssertGreaterThan("On a Master Booking Confirmation, update to " + columnChanged.Name + " should update master booking version", bookingConfirmation.KK_MasterBookingVersion, previousMasterBookingVersion);
		}

		public void TestNonMasterBookingConfirmationUpdatesDoNotUpdateMasterBookingVersion()
		{
			var bookingConfirmation = CreateBookingConfirmationForMasterBookingReplicationTests(false);

			AssertEquals("Precondition: Non-master booking confirmation should have KK_MasterBookingVersion of 0", (short)0, bookingConfirmation.KK_MasterBookingVersion);

			var now = ZDateTime.Now;
			bookingConfirmation.Instruction.KN_InstructionType = "DLV";
			bookingConfirmation.KK_ConfirmationType = "DLV";
			bookingConfirmation.KK_ReferenceNum = "ChangedRefNum";
			bookingConfirmation.KK_Estimated = now;
			bookingConfirmation.KK_Actual = now;
			bookingConfirmation.KK_RequiredFrom = now;
			bookingConfirmation.KK_RequiredTo = now.AddDays(7);
			bookingConfirmation.KK_KD_BookingInstructionPkgDivot = bookingConfirmation.Instruction.PackageDivots.First().PK;
			bookingConfirmation.KK_Quantity = 2;
			bookingConfirmation.Instruction.OrganisationType = OrganisationTypesList.Codes.CYD;
			bookingConfirmation.KK_IsEmptyContainer = true;
			bookingConfirmation.KK_ReceivedBy = "Me";
			bookingConfirmation.KK_ReceivedBySignature = ZBlob.FromAscii("I was here");
			bookingConfirmation.KK_SlotDateTime = now;
			bookingConfirmation.KK_SlotReference = "NewSlotRef";

			Assert("Precondition: BookingConfirmation.HasChanges is true", bookingConfirmation.HasChanges);
			Factory.Save();

			AssertEquals("On a Non-master booking confirmation, any updates should leave master booking version at 0", (short)0, bookingConfirmation.KK_MasterBookingVersion);
		}

		DtbBookingConfirmation CreateBookingConfirmationForMasterBookingReplicationTests(bool isMaster)
		{
			var booking = Helper.CreateBooking();
			booking.KM_IsMaster = isMaster;
			booking.KM_MasterBookingVersion = (short)(isMaster ? 1 : 0);

			var bookingInstruction = booking.Instructions.AddNew();
			bookingInstruction.KN_IsMaster = isMaster;
			bookingInstruction.KN_MasterBookingVersion = (short)(isMaster ? 1 : 0);
			bookingInstruction.KN_InstructionType = InstructionTypes.Codes.PickUp;
			bookingInstruction.FillWithValidTestData();

			if (isMaster)
			{
				var subBooking = Helper.CreateBooking();
				subBooking.KM_KM_MasterBooking = booking.PK;
				subBooking.KM_MasterBookingVersion = booking.KM_MasterBookingVersion;

				var package = subBooking.PackageJob.Packages.AddNew("CNT", 2);
				package.Container.K0_RC_ContainerType = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

				Factory.Save();

				subBooking.ConsolidationSingleJob.KB_KB_MasterBookingConsolidation = booking.ConsolidationSingleJob.PK;
				subBooking.ConsolidationSingleJob.KB_MasterBookingVersion = booking.ConsolidationSingleJob.KB_MasterBookingVersion;

				var subBookingInstruction = subBooking.Instructions.AddNew();
				subBookingInstruction.KN_IsMaster = false;
				subBookingInstruction.KN_MasterBookingVersion = bookingInstruction.KN_MasterBookingVersion;

				var divot = subBookingInstruction.PackageDivots.AddNew();
				divot.KD_KP_Package = package.PK;
				divot.KD_Quantity = package.KP_PackageQty;
			}
			else
			{
				var package = booking.PackageJob.Packages.AddNew("CNT", 2);
				package.Container.K0_RC_ContainerType = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

				var divot = bookingInstruction.PackageDivots.AddNew();
				divot.KD_KP_Package = package.PK;
				divot.KD_Quantity = package.KP_PackageQty;
			}

			Factory.Save();

			var bookingConfirmation = bookingInstruction.Confirmations.AddNew();
			bookingConfirmation.KK_IsMaster = isMaster;
			bookingConfirmation.KK_MasterBookingVersion = (short)(isMaster ? 1 : 0);
			bookingConfirmation.KK_ConfirmationType = ConfirmationTypes.Codes.PickUp;
			bookingConfirmation.KK_ReferenceNum = ZString.Empty;
			bookingConfirmation.KK_Estimated = ZDateTime.Empty;
			bookingConfirmation.KK_Actual = ZDateTime.Empty;
			bookingConfirmation.KK_RequiredFrom = ZDateTime.Empty;
			bookingConfirmation.KK_RequiredTo = ZDateTime.Empty;
			bookingConfirmation.KK_Quantity = 0;
			bookingConfirmation.KK_IsEmptyContainer = false;
			bookingConfirmation.KK_ReceivedBy = ZString.Empty;
			bookingConfirmation.KK_ReceivedBySignature = ZBlob.Empty;
			bookingConfirmation.KK_SlotDateTime = ZDateTime.Empty;
			bookingConfirmation.KK_SlotReference = ZString.Empty;

			Factory.Save();

			return bookingConfirmation;
		}
	}

	/// <summary>
	/// Do *NOT* add new tests here, this code should eventually be migrated into the above test case.
	/// </summary>
	public class DtbBookingConfirmationOldTest : DtbBookingTestCaseWithFactory
	{
		public void TestBooking()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			var instruction = booking.Instructions.AddNew();
			var confirmation = instruction.Confirmations.AddNew();
			AssertEquals(booking, confirmation.Booking);

			AssertNull(Factory.New<DtbBookingConfirmation>().Booking);
		}

		public void TestPackageDivot_WithTransportBookingViews()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			var instruction = booking.Instructions.AddNew();
			var confirmation = instruction.Confirmations.AddNew();
			AssertNull("Confirmations Parent is Instruction, and because the is no Selected Package, we don't know what divot to use, plus there isn't one", confirmation.PackageDivot);

			var package1 = Helper.CreatePackage("p1");
			var divot1 = Helper.CreatePackageDivot(instruction, package1, 1);
			var package2 = Helper.CreatePackage("p2");
			var divot2 = Helper.CreatePackageDivot(instruction, package2, 1);
			AssertNull("We now have divots, but still not in package view", confirmation.PackageDivot);

			booking.SetInstructionViewAndSelectedPackage(TransportBookingInstructionView.Package, booking.Packages_PackageView.Find(package1));
			AssertNull("we are in package1 view, divot between inst1 and p1 is divot1, but the confirmation parent is the Instruction (ALL) so divot should be null", confirmation.PackageDivot);

			booking.SetInstructionViewAndSelectedPackage(TransportBookingInstructionView.Package, booking.Packages_PackageView.Find(package2));
			AssertNull("we are in package2 view, divot between inst1 and p2 is divot2, but the confirmation parent is the Instruction (ALL) so divot should be null", confirmation.PackageDivot);

			var instruction2 = booking.Instructions.AddNew();
			var divot11 = Helper.CreatePackageDivot(instruction2, package1, 1);
			var divot22 = Helper.CreatePackageDivot(instruction2, package2, 1);
			confirmation.KK_KD_BookingInstructionPkgDivot = ZGuid.Empty;
			AssertNull("confirmation has been moved to instruction 2, still should show null", confirmation.PackageDivot);

			booking.SetInstructionViewAndSelectedPackage(TransportBookingInstructionView.Package, booking.Packages_PackageView.Find(package1));
			AssertNull("view changed to package1, still should show null", confirmation.PackageDivot);

			confirmation.KK_KD_BookingInstructionPkgDivot = divot11.PK;
			AssertEquals("parent is now divot11, so now show divot11", divot11, confirmation.PackageDivot);

			booking.SetInstructionViewAndSelectedPackage(TransportBookingInstructionView.Instruction, null);
			AssertEquals("view changed to instruction, but divot is the parent so still show divot11", divot11, confirmation.PackageDivot);
		}

		public void TestSelectedPackage_PackageView()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			var instruction = booking.Instructions.AddNew();
			var confirmation = instruction.Confirmations.AddNew();
			AssertNull("no packages", confirmation.SelectedPackage_PackageView);

			var package1 = Helper.CreatePackage("p1");
			var divot1 = Helper.CreatePackageDivot(instruction, package1, 1);
			var package2 = Helper.CreatePackage("p2");
			var divot2 = Helper.CreatePackageDivot(instruction, package2, 1);
			AssertNull("we have packages, but not in packageview", confirmation.SelectedPackage_PackageView);

			var package1Wrapper = booking.Packages_PackageView.Find(package1);
			var package2Wrapper = booking.Packages_PackageView.Find(package2);
			booking.SetInstructionViewAndSelectedPackage(TransportBookingInstructionView.Package, package1Wrapper);
			AssertEquals("we are in package1 view", package1Wrapper, confirmation.SelectedPackage_PackageView);

			booking.SetInstructionViewAndSelectedPackage(TransportBookingInstructionView.Package, package2Wrapper);
			AssertEquals("we are in package2 view", package2Wrapper, confirmation.SelectedPackage_PackageView);
		}

		// persistent

		public void TestKK_KD_BookingInstructionPkgDivot_UpdatesInstructionStatus()
		{
			var instruction = Helper.CreateInstruction(InstructionTypes.Codes.PickUp);
			AssertEquals("Precondition", TransportStatuses.Codes.Available, instruction.KN_Status);

			var pickupConfirmation = instruction.Confirmations.AddNew();
			pickupConfirmation.KK_ConfirmationType = ConfirmationTypes.Codes.PickUp;
			pickupConfirmation.KK_Actual = ZDateTime.Now;
			AssertEquals(TransportStatuses.Codes.PickedUp, instruction.KN_Status);

			// ensure confirmation delete updates the instruction status
			pickupConfirmation.Delete();
			AssertEquals(TransportStatuses.Codes.Available, instruction.KN_Status);
		}

		public void TestKN_Actual_UpdatesInstructionStatus()
		{
			var instruction = Helper.CreateInstruction(InstructionTypes.Codes.PickUp);
			var pickUpConfirmation = instruction.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp);
			AssertEquals("Precondition", TransportStatuses.Codes.Available, instruction.KN_Status);

			pickUpConfirmation.KK_Actual = ZDateTime.Now;
			AssertEquals(TransportStatuses.Codes.PickedUp, instruction.KN_Status);
		}

		public void TestKN_Description_UpdatesInstructionStatus()
		{
			var instruction = Helper.CreateInstruction(InstructionTypes.Codes.PickUp);
			var pickUpConfirmation = instruction.Confirmations.AddNew();
			pickUpConfirmation.KK_Actual = ZDateTime.Now;
			AssertEquals("Precondition", TransportStatuses.Codes.Available, instruction.KN_Status);

			pickUpConfirmation.KK_ConfirmationType = ConfirmationTypes.Codes.PickUp;
			AssertEquals(TransportStatuses.Codes.PickedUp, instruction.KN_Status);
		}

		public void TestKN_Quantity()
		{
			var instruction = Helper.CreateInstruction(InstructionTypes.Codes.PickUp);
			var confirmation = instruction.Confirmations.AddNew();
			AssertEquals(0, confirmation.KK_Quantity);

			var divot = instruction.PackageDivots.AddNew();
			var package = Factory.New<PkgPackage>();
			package.KP_PackageQty = 10;
			divot.KD_KP_Package = package.PK;
			divot.KD_Quantity = 6;

			confirmation.ParentID_InstructionOrPackageDivot = divot.PK;
			AssertEquals(6, confirmation.KK_Quantity);

			confirmation.KK_Quantity = 2;
			AssertEquals(2, confirmation.KK_Quantity);

			var newConfirmation = divot.Confirmations.AddNew();
			AssertEquals(4, newConfirmation.KK_Quantity);

			newConfirmation.KK_ConfirmationType = ConfirmationTypes.Codes.ConNoteNo;
			AssertEquals(6, newConfirmation.KK_Quantity);

			newConfirmation.KK_Quantity = 5;

			var newConNoteConfirmation = divot.Confirmations.AddNew();
			newConNoteConfirmation.KK_ConfirmationType = ConfirmationTypes.Codes.ConNoteNo;
			AssertEquals(1, newConNoteConfirmation.KK_Quantity);

			var divot2 = instruction.PackageDivots.AddNew();
			divot2.KD_KP_Package = package.PK;
			divot2.KD_Quantity = 2;
		}

		public void TestKN_Quantity_UpdatesInstructionStatus()
		{
			var instruction = Helper.CreateInstruction(InstructionTypes.Codes.PickUp);
			var confirmation = instruction.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp);
			confirmation.KK_Actual = ZDateTime.Now;
			AssertEquals(0, confirmation.KK_Quantity);
			AssertEquals(TransportStatuses.Codes.PickedUp, instruction.KN_Status);

			var divot = instruction.PackageDivots.AddNew();
			var package = Factory.New<PkgPackage>();
			package.KP_PackageQty = 10;
			divot.KD_KP_Package = package.PK;
			divot.KD_Quantity = 6;

			confirmation.ParentID_InstructionOrPackageDivot = divot.PK;
			AssertEquals(6, confirmation.KK_Quantity);
			AssertEquals(TransportStatuses.Codes.PickedUp, instruction.KN_Status);

			confirmation.KK_Quantity = 2;
			AssertEquals(2, confirmation.KK_Quantity);
			AssertEquals(TransportStatuses.Codes.Available, instruction.KN_Status);
		}

		public void TestKN_QuantityReadOnly()
		{
			var instruction = Helper.CreateInstruction(InstructionTypes.Codes.PickUp);
			var confirmation = instruction.Confirmations.AddNew();
			AssertEquals(true, confirmation.KK_QuantityInfo.ReadOnly);

			var divot = instruction.PackageDivots.AddNew();
			var package = Factory.New<PkgPackage>();
			divot.KD_KP_Package = package.PK;

			confirmation.ParentID_InstructionOrPackageDivot = divot.PK;
			AssertEquals(false, confirmation.KK_QuantityInfo.ReadOnly);
		}

		public void TestKK_IsEmptyContainer()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			var cfsInstruction = booking.Instructions.AddNew();
			cfsInstruction.OrganisationType = OrganisationTypesList.Codes.CFS;
			var cfsConfirmation = cfsInstruction.Confirmations.AddNew();
			AssertEquals("Is a CFS confirmation with no container", false, cfsConfirmation.KK_IsEmptyContainer);

			var packageJob = Helper.CreatePackageJob(consolidation);
			var packageDivot = Helper.CreatePackageDivot(cfsInstruction, 1);
			var package = Helper.CreatePackage("Package1", packageDivot, 1);
			AssertEquals("Is a CFS confirmation with a package, but no container", false, cfsConfirmation.KK_IsEmptyContainer);

			var containerDivot = Helper.CreatePackageDivot(cfsInstruction, 1);
			var container = Helper.CreatePackageContainer("CONT1", containerDivot);
			AssertEquals("Is a CFS confirmation with a container and package, no update cause package was added later", false, cfsConfirmation.KK_IsEmptyContainer);

			var cydInstruction = booking.Instructions.AddNew();
			cydInstruction.OrganisationType = OrganisationTypesList.Codes.CYD;
			var cydConfirmation = cydInstruction.Confirmations.AddNew();
			AssertEquals("Is a CYD confirmation with no package", false, cydConfirmation.KK_IsEmptyContainer);

			packageDivot = Helper.CreatePackageDivot(cydInstruction, 1);
			package = Helper.CreatePackageContainer("Cont1", packageDivot);
			AssertEquals("Is a CYD confirmation with a container, but container added after, no update atm", false, cydConfirmation.KK_IsEmptyContainer);

			var newCYDConfirmation = cydInstruction.Confirmations.AddNew();
			AssertEquals("New CYD confirmation with a container", true, newCYDConfirmation.KK_IsEmptyContainer);
		}

		public void TestKK_IsEmptyContainer_CYD_Mixed()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			var cydInstruction = booking.Instructions.AddNew();
			cydInstruction.OrganisationType = OrganisationTypesList.Codes.CYD;

			var packageJob = Helper.CreatePackageJob(consolidation);
			var packageDivot = Helper.CreatePackageDivot(cydInstruction, 1);
			var containerDivot = Helper.CreatePackageDivot(cydInstruction, 1);
			var package = Helper.CreatePackage("Package1", packageDivot, 1);
			var container = Helper.CreatePackageContainer("Cont1", containerDivot);

			var cydConfirmation = cydInstruction.Confirmations.AddNew();
			AssertEquals("Confirmation is attached to the instruction and not all are loose", false, cydConfirmation.KK_IsEmptyContainer);

			cydConfirmation.ParentID_InstructionOrPackageDivot = containerDivot.PK;
			AssertEquals("Confirmation becomes a Container Only confirmation", true, cydConfirmation.KK_IsEmptyContainer);
		}

		// calculated

		public void TestParentID_InstructionOrPackage()
		{
			var booking = Helper.CreateBooking();
			var instruction = booking.Instructions.AddNew();
			var divot1 = instruction.PackageDivots.AddNew();
			var divot2 = instruction.PackageDivots.AddNew();
			var package1 = Factory.New<PkgPackage>();
			var package2 = Factory.New<PkgPackage>();
			divot1.KD_KP_Package = package1.PK;
			divot2.KD_KP_Package = package2.PK;
			var confirmation = instruction.Confirmations.AddNew();
			AssertEquals("Parent is Instruction", instruction, confirmation.Instruction);
			AssertNull("PackageDivot should be null, because it's for ALL the instructions packages", confirmation.PackageDivot);

			confirmation.ParentID_InstructionOrPackageDivot = divot1.PK;
			AssertEquals("Gets the instruction through divot 1", instruction, confirmation.Instruction);
			AssertEquals("PackageDivot should be divot 1", divot1, confirmation.PackageDivot);

			confirmation.ParentID_InstructionOrPackageDivot = Factory.New<DtbBookingInstructionPkgDivot>().PK;
			AssertEquals("new divot isn't part of this confirmation, so should still get to original Instruction through divot 1", instruction, confirmation.Instruction);
			AssertEquals("PackageDivot should still be divot 1", divot1, confirmation.PackageDivot);

			confirmation.ParentID_InstructionOrPackageDivot = divot2.PK;
			AssertEquals("Gets the instruction through divot 2", instruction, confirmation.Instruction);
			AssertEquals("PackageDivot should be divot 2", divot2, confirmation.PackageDivot);

			confirmation.ParentID_InstructionOrPackageDivot = ZGuid.Empty;
			AssertEquals("Cannot set to empty, so still gets the instruction through divot 2", instruction, confirmation.Instruction);
			AssertEquals("PackageDivot should be divot 2", divot2, confirmation.PackageDivot);

			confirmation.ParentID_InstructionOrPackageDivot = instruction.PK;
			AssertEquals("Parent is back to Instruction", instruction, confirmation.Instruction);
			AssertNull("PackageDivot should be null, because it's for ALL the instructions packages", confirmation.PackageDivot);

			confirmation.ParentID_InstructionOrPackageDivot = ZGuid.Invalid;
			AssertEquals("Cannot set to invalid, so instruction should still be the parent", instruction, confirmation.Instruction);
			AssertNull("PackageDivot should be null, because it's for ALL the instructions packages", confirmation.PackageDivot);

			confirmation.ParentID_InstructionOrPackageDivot = instruction.PK;
			AssertEquals("Parent is back to Instruction", instruction, confirmation.Instruction);
			AssertNull("PackageDivot should be null, because it's for ALL the instructions packages", confirmation.PackageDivot);
		}

		public void TestRequiredFromLabel()
		{
			var dateAndReferences = DateAndReferenceCollection.GetDefault();
			var dateAndReference = dateAndReferences.AddNew();
			dateAndReference.Code = "CUS";
			dateAndReference.Description = (NoResString)"Customs";
			dateAndReference.AllowRequiredFromDate = true;
			dateAndReference.AllowRequiredFromLabel = (NoResString)"Hello";

			TransportRegistry.Instance.DateAndReference.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, dateAndReferences);

			var booking = Helper.CreateConsolidation();
			var movement = booking.Bookings.AddNew();
			var instruction = movement.Instructions.AddNew();
			var confirmation = instruction.Confirmations.AddNew();
			AssertEquals("", confirmation.RequiredFromLabel);

			confirmation.KK_ConfirmationType = "CUS";
			AssertEquals("Hello", confirmation.RequiredFromLabel);
		}

		public void TestRequiredToLabel()
		{
			var dateAndReferences = DateAndReferenceCollection.GetDefault();
			var dateAndReference = dateAndReferences.AddNew();
			dateAndReference.Code = "CUS";
			dateAndReference.Description = (NoResString)"Customs";
			dateAndReference.AllowRequiredToDate = true;
			dateAndReference.AllowRequiredToLabel = (NoResString)"Hello";

			TransportRegistry.Instance.DateAndReference.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, dateAndReferences);

			var booking = Helper.CreateConsolidation();
			var movement = booking.Bookings.AddNew();
			var instruction = movement.Instructions.AddNew();
			var confirmation = instruction.Confirmations.AddNew();
			AssertEquals("", confirmation.RequiredToLabel);

			confirmation.KK_ConfirmationType = "CUS";
			AssertEquals("Hello", confirmation.RequiredToLabel);
		}

		public void TestConfirmationDescription()
		{
			var dateAndReferences = DateAndReferenceCollection.GetDefault();
			var dateAndReferenceCFS = dateAndReferences.AddNew();
			dateAndReferenceCFS.Code = "CUS";
			dateAndReferenceCFS.Description = (NoResString)"Custom";
			dateAndReferenceCFS.OrganisationType = "CFS";
			dateAndReferenceCFS.AllowActualDate = true;

			using (TransportRegistry.Instance.DateAndReference.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, dateAndReferences))
			{
				var consolidation = Helper.CreateConsolidation();
				var booking = consolidation.Bookings.AddNew();
				var instruction = booking.Instructions.AddNew();
				var confirmation = instruction.Confirmations.AddNew();

				instruction.OrganisationType = "CFS";
				confirmation.ConfirmationDescription = "Custom";
				AssertEquals("CUS", confirmation.KK_ConfirmationType);

				var confirmation2 = instruction.Confirmations.AddNew();
				instruction.OrganisationType = "CTO";
				confirmation2.ConfirmationDescription = "Custom";
				AssertEquals("Custom is not vali for CTO", "", confirmation2.KK_ConfirmationType);

				var confirmation3 = instruction.Confirmations.AddNew();
				confirmation3.KK_ConfirmationType = "CUS";
				AssertEquals("Custom", confirmation3.ConfirmationDescription);

				var confirmation4 = instruction.Confirmations.AddNew();
				confirmation4.KK_ConfirmationType = "CU1";
				AssertEquals("CU1", confirmation4.ConfirmationDescription);
			}
		}

		public void TestFetchStrategy()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			var instruction = booking.Instructions.AddNew();
			var confirmation = instruction.Confirmations.AddNew();
			AssertNotNull(confirmation.FetchStrategy);
			AssertEquals(typeof(DtbBookingConfirmationFetchStrategy), confirmation.FetchStrategy.GetType());
		}

		public void TestDateAndReferenceRegistry()
		{
			var dateAndReferences = DateAndReferenceCollection.GetDefault();
			var dateAndReference = dateAndReferences.AddNew();
			dateAndReference.Code = "CUS";
			dateAndReference.Description = (NoResString)"Custom";
			dateAndReference.AllowActualDate = true;
			dateAndReference.AllowEstimatedDate = false;
			dateAndReference.AllowRequiredFromDate = true;
			dateAndReference.AllowRequiredToDate = false;
			dateAndReference.AllowReference = true;
			dateAndReference.AllowReceivedBy = false;
			dateAndReference.IsSystemDefined = false;

			TransportRegistry.Instance.DateAndReference.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, dateAndReferences);

			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			var instruction = booking.Instructions.AddNew();
			var confirmation = instruction.Confirmations.AddNew();

			confirmation.KK_ConfirmationType = "CUS";
			AssertEquals(false, confirmation.KK_ActualInfo.ReadOnly);
			AssertEquals(true, confirmation.KK_EstimatedInfo.ReadOnly);
			AssertEquals(false, confirmation.KK_RequiredFromInfo.ReadOnly);
			AssertEquals(true, confirmation.KK_RequiredToInfo.ReadOnly);
			AssertEquals(false, confirmation.KK_ReferenceNumInfo.ReadOnly);
			AssertEquals(true, confirmation.KK_ReceivedByInfo.ReadOnly);

			confirmation.KK_ConfirmationType = "";
			AssertEquals(true, confirmation.KK_ActualInfo.ReadOnly);
			AssertEquals(true, confirmation.KK_EstimatedInfo.ReadOnly);
			AssertEquals(true, confirmation.KK_RequiredFromInfo.ReadOnly);
			AssertEquals(true, confirmation.KK_RequiredToInfo.ReadOnly);
			AssertEquals(true, confirmation.KK_ReferenceNumInfo.ReadOnly);
			AssertEquals(true, confirmation.KK_ReceivedByInfo.ReadOnly);
		}

		public void TestSplitFromInstructionToPackageDivots()
		{
			var year = ZDateTime.Now.Year;

			var booking = Helper.CreateBooking();
			var instructionCTO = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, "CTO", null);
			var instructionCFS = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, "CFS", null);
			var package1 = Helper.CreatePackage("p1");
			var package2 = Helper.CreatePackage("p2");
			var package3 = Helper.CreatePackage("p3");
			var divot_CTO_P1 = Helper.CreatePackageDivot(instructionCTO, package1, 1);
			var divot_CTO_P2 = Helper.CreatePackageDivot(instructionCTO, package2, 1);
			var divot_CTO_P3 = Helper.CreatePackageDivot(instructionCTO, package3, 1);
			var divot_CFS_P1 = Helper.CreatePackageDivot(instructionCFS, package1, 1);
			var divot_CFS_P2 = Helper.CreatePackageDivot(instructionCFS, package2, 1);
			var divot_CFS_P3 = Helper.CreatePackageDivot(instructionCFS, package3, 1);

			var confirmation = instructionCTO.Confirmations.AddNew();
			AssertContainsExactElementsInAnyOrder("instructionCTO", new DtbBookingConfirmation[] { confirmation }, instructionCTO.Confirmations);
			AssertContainsExactElementsInAnyOrder("instructionCFS", Array.Empty<DtbBookingConfirmation>(), instructionCFS.Confirmations);
			AssertContainsExactElementsInAnyOrder("divot_CTO_P1", new DtbBookingConfirmation[] { confirmation }, divot_CTO_P1.Confirmations);
			AssertContainsExactElementsInAnyOrder("divot_CTO_P2", new DtbBookingConfirmation[] { confirmation }, divot_CTO_P2.Confirmations);
			AssertContainsExactElementsInAnyOrder("divot_CTO_P3", new DtbBookingConfirmation[] { confirmation }, divot_CTO_P3.Confirmations);
			AssertContainsExactElementsInAnyOrder("divot_CFS_P1", Array.Empty<DtbBookingConfirmation>(), divot_CFS_P1.Confirmations);
			AssertContainsExactElementsInAnyOrder("divot_CFS_P2", Array.Empty<DtbBookingConfirmation>(), divot_CFS_P2.Confirmations);
			AssertContainsExactElementsInAnyOrder("divot_CFS_P3", Array.Empty<DtbBookingConfirmation>(), divot_CFS_P3.Confirmations);

			confirmation.KK_Estimated = new ZDateTime(year, 01, 01);
			AssertContainsExactElementsInAnyOrder("instructionCTO", new DtbBookingConfirmation[] { confirmation }, instructionCTO.Confirmations);
			AssertContainsExactElementsInAnyOrder("instructionCFS", Array.Empty<DtbBookingConfirmation>(), instructionCFS.Confirmations);
			AssertContainsExactElementsInAnyOrder("divot_CTO_P1", new DtbBookingConfirmation[] { confirmation }, divot_CTO_P1.Confirmations);
			AssertContainsExactElementsInAnyOrder("divot_CTO_P2", new DtbBookingConfirmation[] { confirmation }, divot_CTO_P2.Confirmations);
			AssertContainsExactElementsInAnyOrder("divot_CTO_P3", new DtbBookingConfirmation[] { confirmation }, divot_CTO_P3.Confirmations);
			AssertContainsExactElementsInAnyOrder("divot_CFS_P1", Array.Empty<DtbBookingConfirmation>(), divot_CFS_P1.Confirmations);
			AssertContainsExactElementsInAnyOrder("divot_CFS_P2", Array.Empty<DtbBookingConfirmation>(), divot_CFS_P2.Confirmations);
			AssertContainsExactElementsInAnyOrder("divot_CFS_P3", Array.Empty<DtbBookingConfirmation>(), divot_CFS_P3.Confirmations);

			var package1Wrapper = booking.Packages_PackageView.Find(package1);
			var package2Wrapper = booking.Packages_PackageView.Find(package2);
			var package3Wrapper = booking.Packages_PackageView.Find(package3);

			confirmation.SplitFromInstructionToPackageDivots(divot_CTO_P1);
			var confirmationP2 = package2Wrapper.Confirmations[0];
			var confirmationP3 = package3Wrapper.Confirmations[0];
			AssertNotEquals(confirmationP2, confirmation);
			AssertNotEquals(confirmationP3, confirmation);
			AssertEquals("original value", new ZDateTime(year, 01, 01), confirmation.KK_Estimated);
			AssertEquals("original value", new ZDateTime(year, 01, 01), confirmationP2.KK_Estimated);
			AssertEquals("original value", new ZDateTime(year, 01, 01), confirmationP3.KK_Estimated);
			AssertContainsExactElementsInAnyOrder("instructionCTO", new DtbBookingConfirmation[] { confirmation, confirmationP2, confirmationP3 }, instructionCTO.Confirmations);
			AssertContainsExactElementsInAnyOrder("instructionCFS", Array.Empty<DtbBookingConfirmation>(), instructionCFS.Confirmations);
			AssertContainsExactElementsInAnyOrder("divot_CTO_P1", new DtbBookingConfirmation[] { confirmation }, divot_CTO_P1.Confirmations);
			AssertContainsExactElementsInAnyOrder("divot_CTO_P2", new DtbBookingConfirmation[] { confirmationP2 }, divot_CTO_P2.Confirmations);
			AssertContainsExactElementsInAnyOrder("divot_CTO_P3", new DtbBookingConfirmation[] { confirmationP3 }, divot_CTO_P3.Confirmations);
			AssertContainsExactElementsInAnyOrder("divot_CFS_P1", Array.Empty<DtbBookingConfirmation>(), divot_CFS_P1.Confirmations);
			AssertContainsExactElementsInAnyOrder("divot_CFS_P2", Array.Empty<DtbBookingConfirmation>(), divot_CFS_P2.Confirmations);
			AssertContainsExactElementsInAnyOrder("divot_CFS_P3", Array.Empty<DtbBookingConfirmation>(), divot_CFS_P3.Confirmations);
		}

		public void TestClone()
		{
			var day = ZDateTime.Today;
			var oneDay = new TimeSpan(1, 0, 0, 0);

			var org = Helper.CreateOrganisation("ORG");
			var booking = Helper.CreateBooking();
			var instruction = booking.Instructions.AddNew();
			instruction.OrganisationType = "CFS";
			instruction.Address.E2_OA_Address = org.MainAddress.PK;

			var allConfirmation = instruction.Confirmations.AddNew();
			allConfirmation.KK_ConfirmationType = ConfirmationTypes.Codes.PickUp;
			allConfirmation.KK_Estimated = day;
			allConfirmation.KK_Actual = day += oneDay;
			allConfirmation.KK_RequiredFrom = day += oneDay;
			allConfirmation.KK_RequiredTo = day += oneDay;

			var packageDivot = instruction.PackageDivots.AddNew();
			var packageConfirmation = Factory.New<DtbBookingConfirmation>();
			packageConfirmation.KK_KD_BookingInstructionPkgDivot = packageDivot.PK;
			packageConfirmation.KK_Estimated = day += oneDay;
			packageConfirmation.KK_Actual = day += oneDay;
			packageConfirmation.KK_RequiredFrom = day += oneDay;
			packageConfirmation.KK_RequiredTo = day += oneDay;

			var allClone = (DtbBookingConfirmation)allConfirmation.Clone();
			AssertEquals(allConfirmation.KK_KD_BookingInstructionPkgDivot, allConfirmation.KK_KD_BookingInstructionPkgDivot);
			AssertEquals(allConfirmation.KK_Estimated, allConfirmation.KK_Estimated);
			AssertEquals(allConfirmation.KK_Actual, allConfirmation.KK_Actual);
			AssertEquals(allConfirmation.KK_RequiredFrom, allConfirmation.KK_RequiredFrom);
			AssertEquals(allConfirmation.KK_RequiredTo, allConfirmation.KK_RequiredTo);

			var divotClone = (DtbBookingConfirmation)packageConfirmation.Clone();
			AssertEquals(packageConfirmation.KK_KD_BookingInstructionPkgDivot, divotClone.KK_KD_BookingInstructionPkgDivot);
			AssertEquals(packageConfirmation.KK_Estimated, divotClone.KK_Estimated);
			AssertEquals(packageConfirmation.KK_Actual, divotClone.KK_Actual);
			AssertEquals(packageConfirmation.KK_RequiredFrom, divotClone.KK_RequiredFrom);
			AssertEquals(packageConfirmation.KK_RequiredTo, divotClone.KK_RequiredTo);
		}
	}

	[TestedType(typeof(DtbBookingConfirmation))]
	public class DtbBookingConfirmationWorkflowProviderTest : WorkflowProviderTest<DtbBookingConfirmation, DtbBookingConfirmationProcessTaskCollection>
	{
		protected override DtbBookingConfirmation GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			var instruction = booking.Instructions.AddNew();
			return instruction.Confirmations.AddNew();
		}

		protected override ZString ExpectedWorkflowType
		{
			get { return WorkflowDescriptors.DtbBookingConfirmationWorkflowDescriptorCode; }
		}

		public override void TestProcessTasksCreatedOnSave()
		{
			Assert("Confirmations don't support workflow templates", true);
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}

		TransportBookingTestHelper helper;
	}
}

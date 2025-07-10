using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Packing.Business;
using Enterprise.TransportCommon.Business.Testing;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Business.Testing
{
	[TestedType(typeof(DtbBookingInstructionPkgDivot))]
	public sealed class DtbBookingInstructionPkgDivotBizOTest : DtbTransportBusinessObjectTestCase
	{
		protected override Type ExpectedLookupsType
		{
			get { return typeof(DtbBookingInstructionPkgDivotLookups); }
		}

		protected override Type ExpectedValidationType
		{
			get { return typeof(DtbBookingInstructionPkgDivotValidation); }
		}

		public void TestInstruction()
		{
			var instruction = Factory.New<DtbBookingInstruction>();
			var packageDivot = (DtbBookingInstructionPkgDivot)GetNewBusinessObject();
			packageDivot.KD_KN_BookingInstruction = instruction.PK;

			AssertEquals(instruction, packageDivot.Instruction);
			AssertEquals(typeof(DtbBookingInstruction), packageDivot.Instruction.GetType());
		}

		public void TestPackage()
		{
			var packageDivot = (DtbBookingInstructionPkgDivot)GetNewBusinessObject();
			var package = Factory.New<PkgPackage>();
			packageDivot.KD_KP_Package = package.PK;
			AssertEquals(package, packageDivot.Package);
		}

		public void TestConfirmationsDivotOnly()
		{
			var instruction = GetNewInstruction();
			var divot = instruction.PackageDivots.AddNew();
			var confirmationOnDivot = divot.ConfirmationsDivotOnly.AddNew();
			var confirmationOnInstruction = instruction.Confirmations.AddNew();
			AssertContainsExactElementsInAnyOrder(new[] { confirmationOnDivot }, divot.ConfirmationsDivotOnly);
		}

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

			AssertContainsExactElementsInAnyOrder(new DtbBookingConfirmation[] { confirmation_p1, confirmation_ALL }, divot_p1.Confirmations);
			AssertContainsExactElementsInAnyOrder(new DtbBookingConfirmation[] { confirmation_p2, confirmation_ALL }, divot_p2.Confirmations);

			var newInstruction = GetNewInstruction();
			divot_p1.KD_KN_BookingInstruction = newInstruction.PK;
			AssertContainsExactElementsInAnyOrder(new DtbBookingConfirmation[] { confirmation_p1 }, divot_p1.Confirmations);

			var confirmation_p1New = Helper.CreateConfirmation(divot_p1, ConfirmationTypes.Codes.Delivery);
			AssertContainsExactElementsInAnyOrder(new DtbBookingConfirmation[] { confirmation_p1, confirmation_p1New }, divot_p1.Confirmations);
		}

		public void TestWeight()
		{
			var instruction = GetNewInstruction();
			var divot = instruction.PackageDivots.AddNew();
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

		public void TestVolume()
		{
			var instruction = GetNewInstruction();
			var divot = instruction.PackageDivots.AddNew();
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

		public void TestKD_KP_Package()
		{
			var package = Factory.New<PkgPackage>();
			package.KP_PackageQty = 10;

			var divot = Factory.New<DtbBookingInstructionPkgDivot>();
			divot.KD_KP_Package = package.PK;
			AssertEquals(10, divot.KD_Quantity);

			divot.KD_KP_Package = ZGuid.Empty;
			AssertEquals(0, divot.KD_Quantity);
		}

		public void TestKD_KP_PackageDefaultsDropModeOnInstruction()
		{
			var instruction = GetNewInstruction();
			var consignor = Helper.CreateOrganisation("CNR");
			consignor.MainAddress.OA_LCLEquipmentNeeded = "DM1";
			instruction.Address.OrganisationPK = consignor.PK;
			var divot = instruction.PackageDivots.AddNew();
			AssertEquals("Precondition", "", instruction.KN_DropMode);

			var package = Factory.New<PkgPackage>();
			divot.KD_KP_Package = package.PK;
			AssertEquals("DM1", instruction.KN_DropMode);
		}

		public void TestSettingDelayInstructionUpdatesFromAddingDivotsSkipsOnDivotAssignedToInstruction()
		{
			var (booking, instruction, divot) = CoreDelayAndAfterDivotsAssignedToInstructionTestSetup();

			using (ActiveBusinessObjectCollection.DelayListChangedEvents(Factory))
			{
				booking.DelayInstructionUpdatesFromAddingDivots = true;
				divot.KD_KN_BookingInstruction = instruction.PK;
			}

			CombineAssertions("Check that updates from OnDivotAssignedToInstruction() were not run after instruction PK was set on divot because booking.DelayInstructionUpdatesFromAddingDivots was set", () =>
			{
				AssertEquals("Because booking.DelayInstructionUpdatesFromAddingDivots was set true, should still not have re-created confirmations", 0, instruction.Confirmations.Count);
				AssertEquals("Because booking.DelayInstructionUpdatesFromAddingDivots was set true, should still not have set instruction drop mode", string.Empty, instruction.KN_DropMode);
			});
		}

		public void TestAfterDivotsAssignedToInstruction()
		{
			var (booking, instruction, divot) = CoreDelayAndAfterDivotsAssignedToInstructionTestSetup();

			using (ActiveBusinessObjectCollection.DelayListChangedEvents(Factory))
			{
				booking.DelayInstructionUpdatesFromAddingDivots = true;
				divot.KD_KN_BookingInstruction = instruction.PK;
			}

			CombineAssertions("Precondition: Check that updates from OnDivotAssignedToInstruction() were not run after instruction PK was set on divot because booking.DelayInstructionUpdatesFromAddingDivots was set", () =>
			{
				AssertEquals("Precondition: Because booking.DelayInstructionUpdatesFromAddingDivots was set true, OnDivotAssignedToInstruction() was not run, and should still not have re-created confirmations", 0, instruction.Confirmations.Count);
				AssertEquals("Precondition: Because booking.DelayInstructionUpdatesFromAddingDivots was set true, OnDivotAssignedToInstruction() was not run, and should still not have set instruction drop mode", string.Empty, instruction.KN_DropMode);
			});
			booking.DelayInstructionUpdatesFromAddingDivots = false;

			DtbBookingInstructionPkgDivot.AfterDivotsAssignedToInstruction(instruction);

			CombineAssertions("Check that updates from AfterDivotsAssignedToInstruction() were run", () =>
			{
				AssertEquals("AfterDivotsAssignedToInstruction() should have re-created confirmations", 1, instruction.Confirmations.Count);
				AssertEquals("AfterDivotsAssignedToInstruction() should have set instruction drop mode", "DM1", instruction.KN_DropMode);
			});
		}

		public void TestOnDivotAssignedToInstruction()
		{
			var (booking, instruction, divot) = CoreDelayAndAfterDivotsAssignedToInstructionTestSetup();

			CombineAssertions("Precondition: Check that updates from OnDivotAssignedToInstruction() have not run yet", () =>
			{
				AssertEquals("Precondition: Because updates from OnDivotAssignedToInstruction() have not run yet, should still not have re-created confirmations", 0, instruction.Confirmations.Count);
				AssertEquals("Precondition: Because updates from OnDivotAssignedToInstruction() have not run yet, should still not have set instruction drop mode", string.Empty, instruction.KN_DropMode);
			});

			divot.KD_KN_BookingInstruction = instruction.PK;

			CombineAssertions("Check that all updates from OnDivotAssignedToInstruction() [run after divot assigned instruction] were run", () =>
			{
				AssertEquals("Should have re-created confirmations", 1, instruction.Confirmations.Count);
				AssertEquals("Should have set instruction drop mode", "DM1", instruction.KN_DropMode);
			});
		}

		(DtbBooking booking, DtbBookingInstruction instruction, DtbBookingInstructionPkgDivot divot) CoreDelayAndAfterDivotsAssignedToInstructionTestSetup()
		{
			var booking = Helper.CreateBooking();
			booking.ConsolidationSingleJob.KB_JobDirection = "DLV";
			booking.KM_Direction = "DST";

			var consignor = Helper.CreateOrganisation("CNR");
			consignor.MainAddress.OA_LCLEquipmentNeeded = "DM1";

			var instruction = Helper.CreateInstruction(booking, instructionType: "PIC", orgType: "CNR", consignor.MainAddress);

			var packageJob = booking.PackageJob;
			var package = packageJob.Packages.AddNew();

			var divot = Factory.New<DtbBookingInstructionPkgDivot>();
			divot.KD_KP_Package = package.PK;

			instruction.Confirmations.DeleteAll();
			AssertEquals("Precondition: Instruction drop mode should still be blank", string.Empty, instruction.KN_DropMode);

			return (booking, instruction, divot);
		}

		DtbBookingInstruction GetNewInstruction()
		{
			return Factory.New<DtbBookingInstruction>();
		}

		new TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}

		TransportBookingTestHelper helper;
	}

	public class DtbBookingInstructionPkgDivotTest : DtbBookingTestCaseWithFactory
	{
		public void TestKD_KP_Package_ChangeClearsPackagesActionStrategies()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			var instruction = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			var packageJob = Helper.CreatePackageJob(consolidation);

			var package1 = packageJob.Packages.AddNew("BOX");
			AssertEquals("Precondition: package1 is deletable", true, package1.CanDelete);

			var divot = Helper.CreatePackageDivot(instruction, package1, 1);
			AssertEquals("Package1 is assigned to instruction should be undeletable", false, package1.CanDelete);

			var package2 = packageJob.Packages.AddNew("BOX");
			AssertEquals("Precondition: package2 is deletable", true, package2.CanDelete);

			divot.KD_KP_Package = package2.PK;
			AssertEquals("Package1 has been unassigned from instruction should be deletable", true, package1.CanDelete);
			AssertEquals("Package2 is assigned to instruction should be undeletable", false, package2.CanDelete);
		}

		// calculated

		public void TestPackageDescription()
		{
			var divot = Factory.New<DtbBookingInstructionPkgDivot>();
			AssertEquals("", divot.PackageDescription);

			var package = Helper.CreatePackage("P1", 10, "BOX");
			divot.KD_KP_Package = package.PK;
			AssertEquals("10x Boxes", divot.PackageDescription);
		}

		public void TestPackageDescriptionWithIDAndQty()
		{
			var divot = Factory.New<DtbBookingInstructionPkgDivot>();
			AssertEquals("", divot.PackageDescriptionWithIDAndQty);

			var package = Helper.CreatePackage("P1", 10, "BOX");
			divot.KD_KP_Package = package.PK;
			AssertEquals("P1", divot.PackageDescriptionWithIDAndQty);

			divot.KD_Quantity = 4;
			package.KP_PackageID = "";
			AssertEquals("4 of 10x Boxes", divot.PackageDescriptionWithIDAndQty);
		}

		public void TestDivotDelete_ClearsPackagesActionStrategies()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			var instruction = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			var packageJob = Helper.CreatePackageJob(consolidation);

			var parentPackage = packageJob.Packages.AddNew("BOX");
			AssertEquals("Precondition: parentPackage is deletable", true, parentPackage.CanDelete);

			var childPackage = parentPackage.Packages.AddNew("BOX");
			AssertEquals("Precondition: childPackage is deletable", true, parentPackage.CanDelete);

			var divot = Helper.CreatePackageDivot(instruction, childPackage, 1);
			AssertEquals("childPackage is assigned to instruction should be not deletable", false, childPackage.CanDelete);
			AssertEquals("childPackage is assigned to instruction, parentPackage should also be not deletable", false, parentPackage.CanDelete);

			// delete package assignment in instruction
			instruction.PackageDivots.DeleteAll();
			AssertEquals("childPackage is not assigned to instruction, childPackage should now be deletable", true, childPackage.CanDelete);
			AssertEquals("childPackage is not assigned to instruction, parentPackage should also now be deletable", true, parentPackage.CanDelete);
		}

		public void TestAssignPackageToPackageDivot()
		{
			// divot created from an instruction
			var booking = Helper.CreateBooking();
			booking.KM_Direction = nameof(DtbBookingDirection.DLV);
			var instruction1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, "CTO", null);
			var divotCreatedFromInstruction = Helper.CreatePackageDivot(instruction1);
			Helper.CreatePackage("p1", divotCreatedFromInstruction);
			AssertEquals(1, instruction1.Confirmations.Count);
			instruction1.Confirmations.Single(c => c.KK_ConfirmationType == ConfirmationTypes.Codes.PickUp);

			// divot created for an instruction with a confirmation
			var instruction2 = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, "CTO", null);
			AssertEquals("Precondition", 1, instruction2.Confirmations.Count);
			var divot1 = Helper.CreatePackageDivot(instruction2);
			var package1 = Helper.CreatePackage("p2", 1, Constants.PkgUnit.Container);
			divot1.KD_KP_Package = package1.PK;
			AssertEquals(1, instruction2.Confirmations.Count);
			var deliveryConfirmation = instruction2.Confirmations.Single(c => c.KK_ConfirmationType == ConfirmationTypes.Codes.Delivery);

			// confirmation assign to a package
			AssertEquals("Precondition", instruction2.PK, deliveryConfirmation.ParentID_InstructionOrPackageDivot);
			deliveryConfirmation.ParentID_InstructionOrPackageDivot = divot1.PK;
			var newDivot = Helper.CreatePackageDivot(instruction2);
			var newPackage = Helper.CreatePackage("p3", 1, Constants.PkgUnit.Container);
			newDivot.KD_KP_Package = newPackage.PK;
			AssertEquals(2, instruction2.Confirmations.Count);
			instruction2.Confirmations.Single(c => c.KK_ConfirmationType == ConfirmationTypes.Codes.Delivery && c.ParentID_InstructionOrPackageDivot == divot1.PK);
			instruction2.Confirmations.Single(c => c.KK_ConfirmationType == ConfirmationTypes.Codes.Delivery && c.ParentID_InstructionOrPackageDivot == newDivot.PK);
		}

		public void TestAssignInstructionToPackageDivot()
		{
			// divot created from package
			var booking = Helper.CreateBooking();
			booking.KM_Direction = nameof(DtbBookingDirection.DLV);
			var instruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, "CTO", null);
			var divotForInstruction = Factory.New<DtbBookingInstructionPkgDivot>();
			var packageForDivotCreatedFromPackage = Helper.CreatePackage("p1", 1, Constants.PkgUnit.Box);
			divotForInstruction.KD_KP_Package = packageForDivotCreatedFromPackage.PK;
			divotForInstruction.KD_KN_BookingInstruction = instruction.PK;
			AssertEquals(1, instruction.Confirmations.Count);
			instruction.Confirmations.Single(c => c.KK_ConfirmationType == ConfirmationTypes.Codes.PickUp);

			// divot created for an instruction with a confirmation
			var instructionWithConfirmation = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, "CTO", null);
			AssertEquals("Precondition", 1, instructionWithConfirmation.Confirmations.Count);
			var divot1 = Factory.New<DtbBookingInstructionPkgDivot>();
			var package1 = Helper.CreatePackage("p2", 1, Constants.PkgUnit.Box);
			divot1.KD_KP_Package = package1.PK;
			divot1.KD_KN_BookingInstruction = instructionWithConfirmation.PK;
			AssertEquals(1, instructionWithConfirmation.Confirmations.Count);
			var deliveryConfirmation = instructionWithConfirmation.Confirmations.Single(c => c.KK_ConfirmationType == ConfirmationTypes.Codes.Delivery);

			// Divot created for an instruction with an existing confirmation assign to a single package
			AssertEquals("Precondition", instructionWithConfirmation.PK, deliveryConfirmation.ParentID_InstructionOrPackageDivot);
			deliveryConfirmation.ParentID_InstructionOrPackageDivot = divot1.PK;
			var newDivotForInstruction2 = Factory.New<DtbBookingInstructionPkgDivot>();
			var newPackage = Helper.CreatePackage("p2", 1, Constants.PkgUnit.Box);
			newDivotForInstruction2.KD_KP_Package = newPackage.PK;
			newDivotForInstruction2.KD_KN_BookingInstruction = instructionWithConfirmation.PK;
			AssertEquals(2, instructionWithConfirmation.Confirmations.Count);
			instructionWithConfirmation.Confirmations.Single(c => c.KK_ConfirmationType == ConfirmationTypes.Codes.Delivery && c.ParentID_InstructionOrPackageDivot == divot1.PK);
			instructionWithConfirmation.Confirmations.Single(c => c.KK_ConfirmationType == ConfirmationTypes.Codes.Delivery && c.ParentID_InstructionOrPackageDivot == newDivotForInstruction2.PK);
		}

		public void TestUniqueIndexFailureHandler()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			var instruction = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			var packageJob = Helper.CreatePackageJob(consolidation);
			var package = packageJob.Packages.AddNew("BOX");

			var divot1 = Helper.CreatePackageDivot(instruction, package, 1);
			var divot2 = Helper.CreatePackageDivot(instruction, package, 1);

			try
			{
				Factory.Save();
				Fail("Expected a unique index violation exception");
			}
			catch (ZSaveException ex)
			{
				var uniqueIndexName = ex.IndexNameIfUniqueIndexViolation;
				AssertEquals("1 bizo should be involved with the unique constraint violation (one should save fine)", 1, ex.BusinessObjects.Length);
				IBusinessObjectInternals divotInError = (DtbBookingInstructionPkgDivot)ex.BusinessObjects[0];

				AssertEquals("Correct unique constraint should be violated", DtbBookingInstructionPkgDivotSchema.Constants.Indexes.FK_UC__KD_KN_BookingInstruction_KD_KP_Package, uniqueIndexName);
				var notifier = new MockNotificationHandler();
				divotInError.UniqueIndexFailureHandlers.Single().NotifyUserAndAttemptToResolve(notifier, uniqueIndexName);
				AssertEquals("User should be notified of the situation", "Error", notifier.LastErrorCaption);
				AssertEquals("User should be notified of the situation", "Another user has saved changes to this Form while you were working on it. Please close and re-open the Form for the latest changes.", notifier.LastErrorMessage);
			}
		}

		class MockNotificationHandler : INotificationHandler
		{
			public string LastErrorMessage;
			public string LastErrorCaption;

			public void ReportError(string message, string caption, string errorContext = null, Exception exception = null)
			{
				LastErrorMessage = message;
				LastErrorCaption = caption;
			}

			public void ReportInformation(string message, string caption)
			{
				throw new NotSupportedException();
			}
		}

		[TestDate(2024, 1, 1)]
		public void TestUpdateCO2eStatusToNotCurrent_WhenPackageWeightChanged()
		{
			// Arrange
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			var package1 = Helper.CreatePackage("", null, 1, 1000);
			var package2 = Helper.CreatePackage("", null, 1, 1000);
			package1.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;
			package2.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;
			var instruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var divot = Helper.CreatePackageDivot(instruction, package1, 1);
			booking.SetCO2eStatus(CO2eStatusList.Codes.Current);
			Factory.Save();
			AssertNoWarning(booking.TotalCO2eForBindingInfo, CO2eTestHelper.CO2Warning);

			// Act & Assert
			TestDateAttribute.AddMinutes(1);
			CO2eTestHelper.AssertHasCO2Warning(booking, () => package1.KP_Weight = 800, "KP_Weight [1000]->[800]");

			TestDateAttribute.AddMinutes(1);
			CO2eTestHelper.AssertHasCO2Warning(booking, () => package1.KP_WeightUQ = Constants.Weight.Pounds, "KP_WeightUQ [KG]->[LB]");

			TestDateAttribute.AddMinutes(1);
			CO2eTestHelper.AssertHasCO2Warning(booking, () => package1.KP_PackageQty = 8, "KP_PackageQty [1]->[8]");

			CO2eTestHelper.AssertNoCO2Warning(booking, () => package2.KP_Weight = 800);
			CO2eTestHelper.AssertNoCO2Warning(booking, () => package2.KP_WeightUQ = Constants.Weight.Pounds);
			CO2eTestHelper.AssertNoCO2Warning(booking, () => package2.KP_PackageQty = 8);

			Factory.Save();
			Helper.CreatePackageDivot(instruction, package2, 1);

			TestDateAttribute.AddMinutes(1);
			CO2eTestHelper.AssertHasCO2Warning(booking, () => package2.KP_Weight = 1000, "KP_Weight [6400]->[1000]");

			TestDateAttribute.AddMinutes(1);
			CO2eTestHelper.AssertHasCO2Warning(booking, () => package2.KP_WeightUQ = Constants.Weight.Kilograms, "KP_WeightUQ [LB]->[KG]");

			TestDateAttribute.AddMinutes(1);
			CO2eTestHelper.AssertHasCO2Warning(booking, () => package2.KP_PackageQty = 1, "KP_PackageQty [8]->[1]");
		}

		[TestDate(2024, 1, 1)]
		public void TestUpdateCO2eStatusToNotCurrent_WhenAssignOrUnAssignPackage()
		{
			// Arrange
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			var package1 = Helper.CreatePackage("PKG1", null, 1, 1000);
			package1.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;
			var instruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			booking.SetCO2eStatus(CO2eStatusList.Codes.Current);
			Factory.Save();
			AssertNoWarning("Pre-condition", booking.TotalCO2eForBindingInfo, CO2eTestHelper.CO2Warning);
			AssertEquals("Pre-condition", 0, booking.PackageDivots.Count);

			// Act & Assert
			CO2eTestHelper.AssertHasCO2Warning(booking, () => Helper.CreatePackageDivot(instruction, package1, 1), "Instruction package divot added");

			TestDateAttribute.AddMinutes(1);
			CO2eTestHelper.AssertHasCO2Warning(booking, () => booking.PackageDivots.DeleteAll(), "Instruction package divot deleted");
		}

		[TestDate(2024, 1, 1)]
		public void TestUpdateCO2eStatusToNotCurrent_WhenPackageChanged()
		{
			// Arrange
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			var package1 = Helper.CreatePackage("PKG1", null, 1, 1000);
			var package2 = Helper.CreatePackage("PKG2", null, 1, 1000);
			package1.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;
			package2.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;
			var instruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var divot = Helper.CreatePackageDivot(instruction, package1, 1);
			booking.SetCO2eStatus(CO2eStatusList.Codes.Current);
			Factory.Save();
			AssertNoWarning(booking.TotalCO2eForBindingInfo, CO2eTestHelper.CO2Warning);

			// Act & Assert
			CO2eTestHelper.AssertHasCO2Warning(booking, () => divot.KD_KP_Package = package2.PK, $"KD_KP_Package [{package1.PK}]->[{package2.PK}]");

			TestDateAttribute.AddMinutes(1);
			CO2eTestHelper.AssertHasCO2Warning(booking, () => divot.KD_KP_Package = package1.PK, $"KD_KP_Package [{package2.PK}]->[{package1.PK}]");

			TestDateAttribute.AddMinutes(1);
			CO2eTestHelper.AssertHasCO2Warning(booking, () => package1.KP_Weight = 800, "KP_Weight [1000]->[800]");
			CO2eTestHelper.AssertNoCO2Warning(booking, () => package2.KP_Weight = 800);

			TestDateAttribute.AddMinutes(1);
			CO2eTestHelper.AssertHasCO2Warning(booking, () => divot.KD_KP_Package = Guid.Empty, $"KD_KP_Package [{package1.PK}]->[{ZGuid.Empty}]");
			CO2eTestHelper.AssertNoCO2Warning(booking, () => package1.KP_Weight = 200);
		}
	}
}

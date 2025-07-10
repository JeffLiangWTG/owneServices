using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.TransportConsignment.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.TransportCommon.Business.Testing
{
	public abstract class DtbTransportConfirmationTest : DtbTransportBusinessObjectTestCase
	{
		#region Related Entities

		#region TestInstruction

		public void TestInstruction()
		{
			var instruction1 = GetNewInstruction();
			var confirmation = (DtbTransportConfirmation)instruction1.Confirmations.AddNew();
			AssertEquals("Confirmation.Instruction should be directly via ParentID.", instruction1, confirmation.Instruction);
			Assert("Confirmation.Instruction should not use ParentID", confirmation.KK_KD_BookingInstructionPkgDivot.IsEmpty);

			var instruction2 = GetNewInstruction();
			var package = Helper.CreatePackage("P1");
			var divot = CreatePackageDivot(instruction2, package, 1);
			confirmation.KK_KD_BookingInstructionPkgDivot = divot.PK;
			AssertEquals("Confirmation.Instruction is still via original.", instruction1, confirmation.Instruction);
		}

		#endregion

		#region TestPackageDivot

		public void TestPackageDivot()
		{
			var instruction1 = GetNewInstruction();
			var confirmation = (DtbTransportConfirmation)instruction1.Confirmations.AddNew();
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

		DtbTransportInstructionPkgDivot CreatePackageDivot(DtbTransportInstruction instruction, PkgPackage package, int qty)
		{
			var result = (DtbTransportInstructionPkgDivot)instruction.PackageDivots.AddNew();
			result.KD_KP_Package = package.PK;
			result.KD_Quantity = qty;

			return result;
		}

		#endregion

		#region TestTransport

		public void TestTransport()
		{
			var transport = GetNewTransport();
			var instruction = (DtbTransportInstruction)transport.Instructions.AddNew();
			var confirmation = (DtbTransportConfirmation)instruction.Confirmations.AddNew();
			AssertEquals("Should be the Instructions Transport Job", transport, confirmation.Transport);

			var confirmationNoTransport = (DtbTransportConfirmation)GetNewBusinessObject();

			AssertNull("Should be null and not blow up.", confirmationNoTransport.Transport);
		}

		#endregion

		#endregion

		#region Properties

		#region TestKK_KD_BookingInstructionPkgDivot

		public void TestKK_KD_BookingInstructionPkgDivot()
		{
			var instruction = GetNewInstruction();
			instruction.KN_Status = "";
			AssertEquals("Precondition", "", instruction.KN_Status);

			var confirmation = (DtbTransportConfirmation)instruction.Confirmations.AddNew();
			AssertEquals(TransportStatuses.Codes.Available, instruction.KN_Status);
			AssertEquals("Parent is empty, therefore is for all package on the Instruction", true, confirmation.KK_KD_BookingInstructionPkgDivot.IsEmpty);
			AssertEquals("KK_KN_BookingInstruction equals the Instruction", instruction.PK, confirmation.KK_KN_BookingInstruction);

			TestKK_KD_BookingInstructionPkgDivotCore();
		}

		protected virtual void TestKK_KD_BookingInstructionPkgDivotCore()
		{
		}

		#endregion

		#region TestKK_RequiredFromUtc

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

		#endregion

		#region TestKK_RequiredToUtc

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

		#endregion

		#region TestKK_EstimatedUtc

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

		#endregion

		#region TestEvents

		#region TestKK_Actual_Events

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
			var transport = GetNewTransport();
			transport.KM_TransportReference = "T1";
			transport.KM_JobID = "J1";
			var pickupInstruction = Helper.CreateInstruction(transport, InstructionTypes.Codes.PickUp, orgType, pickupOrganisation1.MainAddress);
			Helper.CreateConfirmation(pickupInstruction, ConfirmationTypes.Codes.PickUp);

			var instruction1ForConsignor = Helper.CreateInstruction(transport, instructionType, orgType, pickupOrganisation1.MainAddress);
			var instruction2ForConsignor = Helper.CreateInstruction(transport, instructionType, orgType, null);
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

			transport.KM_TransportReference = "";
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
			var transport = GetNewTransport();
			transport.FillWithValidTestData();

			var instruction = Helper.CreateInstruction(transport, instructionType, orgType, organisation.MainAddress);

			var confirmation = Helper.CreateConfirmation(instruction, confirmationType);

			confirmation.KK_Actual = now.AddDays(1);

			Factory.Save();

			AssertEquals($"Precondition: No cancelled {eventCode.Description} event log expected", 0, confirmation.Logs.Find(l => l.SL_SE_NKEvent == eventCode.Code && l.IsCancelled).Count());
			AssertEquals($"Precondition: Non-cancelled {eventCode.Description} event log expected", 1, confirmation.Logs.Find(l => l.SL_SE_NKEvent == eventCode.Code && !l.IsCancelled).Count());

			confirmation.KK_Actual = ZDateTime.Empty;
			AssertEquals($"Cancelled {eventCode.Description} event log expected for empty actual date", 1, confirmation.Logs.Find(l => l.SL_SE_NKEvent == eventCode.Code && l.IsCancelled).Count());
			AssertEquals($"No non-cancelled {eventCode.Description} event log expected for empty actual date", 0, confirmation.Logs.Find(l => l.SL_SE_NKEvent == eventCode.Code && !l.IsCancelled).Count());
		}

		#endregion

		#region TestKK_Estimated_Events

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
			var transport = GetNewTransport();
			transport.FillWithValidTestData();

			var instruction = Helper.CreateInstruction(transport, instructionType, orgType, organisation.MainAddress);

			var confirmation = Helper.CreateConfirmation(instruction, confirmationType);

			confirmation.KK_Estimated = now.AddDays(1);

			Factory.Save();

			AssertEquals($"Precondition: No cancelled {eventCode.Description} event log expected", 0, confirmation.Logs.Find(l => l.SL_SE_NKEvent == eventCode.Code && l.IsCancelled && l.SL_IsEstimate).Count());
			AssertEquals($"Precondition: Non-cancelled {eventCode.Description} event log expected", 1, confirmation.Logs.Find(l => l.SL_SE_NKEvent == eventCode.Code && !l.IsCancelled && l.SL_IsEstimate).Count());

			confirmation.KK_Estimated = ZDateTime.Empty;
			AssertEquals($"Cancelled {eventCode.Description} event log expected for empty estimated date", 1, confirmation.Logs.Find(l => l.SL_SE_NKEvent == eventCode.Code && l.IsCancelled && l.SL_IsEstimate).Count());
			AssertEquals($"No non-cancelled {eventCode.Description} event log expected for empty estimated date", 0, confirmation.Logs.Find(l => l.SL_SE_NKEvent == eventCode.Code && !l.IsCancelled && l.SL_IsEstimate).Count());
		}

		#endregion

		#region TestKK_RequiredFrom_Events

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

		#endregion

		#region TestKK_RequiredTo_Events

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

		#endregion

		#region TestKK_Actual_Events_Department

		public abstract void TestKK_Actual_Events_Department();

		#endregion

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
			var transport = GetNewTransport();
			transport.FillWithValidTestData();

			var instruction = Helper.CreateInstruction(transport, instructionType, orgType, organisation.MainAddress);

			var confirmation = Helper.CreateConfirmation(instruction, confirmationType);

			confirmation.KK_RequiredFrom = now.AddDays(1);

			Factory.Save();

			AssertEquals($"Precondition: No cancelled {eventCode.Description} event log expected", 0, confirmation.Logs.Find(l => l.SL_SE_NKEvent == eventCode.Code && l.IsCancelled).Count());
			AssertEquals($"Precondition: Non-cancelled {eventCode.Description} event log expected", 1, confirmation.Logs.Find(l => l.SL_SE_NKEvent == eventCode.Code && !l.IsCancelled).Count());

			confirmation.KK_RequiredFrom = ZDateTime.Empty;
			if (!(confirmation is IDtbConsignmentConfirmation))
			{
				AssertEquals($"No cancelled {eventCode.Description} event log expected for empty date", 0, confirmation.Logs.Find(l => l.SL_SE_NKEvent == eventCode.Code && l.IsCancelled).Count());
				AssertEquals($"{eventCode.Description} event log should remain uncancelled for empty date", 1, confirmation.Logs.Find(l => l.SL_SE_NKEvent == eventCode.Code && !l.IsCancelled).Count());
			}
			else
			{
				// DtbConsignmentConfirmation doesn't have an OnFactorySaved override which calls ClearEventLogCachedOnSaved, and therefore the event is cancelled by the method RemoveMatchingEventIfNotCommitted
				AssertEquals($"Cancelled {eventCode.Description} event log expected for empty date", 1, confirmation.Logs.Find(l => l.SL_SE_NKEvent == eventCode.Code && l.IsCancelled).Count());
				AssertEquals($"No {eventCode.Description} event log should remain uncancelled for empty date", 0, confirmation.Logs.Find(l => l.SL_SE_NKEvent == eventCode.Code && !l.IsCancelled).Count());
			}
		}

		#region Test_DatePropertyEvents_Core

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
			var transport = GetNewTransport();
			transport.KM_TransportReference = "T1";
			transport.KM_JobID = "J1";

			var pickupInstruction = Helper.CreateInstruction(transport, InstructionTypes.Codes.PickUp, orgType, pickupOrganisation1.MainAddress);
			Helper.CreateConfirmation(pickupInstruction, ConfirmationTypes.Codes.PickUp);

			var instruction1ForConsignor = Helper.CreateInstruction(transport, instructionType, orgType, pickupOrganisation1.MainAddress);
			var instruction2ForConsignor = Helper.CreateInstruction(transport, instructionType, orgType, null);

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

			transport.KM_TransportReference = "";
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

		#endregion

		#region TestEventsHelpers

		void CreatePackage(DtbTransportInstructionPkgDivot divot, string packageID, int quantity, string packageType, decimal weight, decimal volume, string weightUQ, string volumeUQ)
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

		protected void AssertEventOnConfirmation(Action<DtbTransportConfirmation> actionToPerform, DtbTransportConfirmation confirmation, Event eventCode,
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

		#endregion

		#endregion

		#region TestTypeParameter

		public void TestTypeParameter_IsContainerised()
		{
			var transport = GetNewTransport();
			var pickupInstruction = Helper.CreateInstruction(transport, InstructionTypes.Codes.PickUp);
			var deliveryInstruction = Helper.CreateInstruction(transport, InstructionTypes.Codes.Delivery);
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
			var transport = GetNewTransport();
			var pickupInstruction = Helper.CreateInstruction(transport, InstructionTypes.Codes.PickUp);
			var deliveryInstruction = Helper.CreateInstruction(transport, InstructionTypes.Codes.Delivery);
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

		static void AssertType(DtbTransportConfirmation confirmation, Event eventCode, string parameterType = "", bool expectParameterType = false)
		{
			var log = confirmation.Logs.Find(l => l.SL_SE_NKEvent == eventCode.Code).Single();
			AssertEquals(expectParameterType, log.Parameters.ContainsKey(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type));
			if (expectParameterType)
			{
				AssertEquals(parameterType, log.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type]);
			}
		}

		#endregion

		#endregion

		#region Flags

		#region TestIsDelivery

		public void TestIsDelivery()
		{
			AssertFlag("IsDelivery", ConfirmationTypes.Codes.Delivery, "abc");
		}

		#endregion

		#region TestIsPickup

		public void TestIsPickup()
		{
			AssertFlag("IsPickUp", ConfirmationTypes.Codes.PickUp, "abc");
		}

		#endregion

		#region TestIsDepot

		public void TestIsDepot()
		{
			var transport = GetNewTransport();
			var instruction = (DtbTransportInstruction)transport.Instructions.AddNew();
			var confirmation = (DtbTransportConfirmation)instruction.Confirmations.AddNew();
			instruction.OrganisationType = "CNR";
			AssertEquals(false, confirmation.IsOwnDepot);

			instruction.OrganisationType = "CFS";
			AssertEquals(false, confirmation.IsOwnDepot);

			instruction.KN_InstructionType = InstructionTypes.Codes.Multi;
			AssertEquals(true, confirmation.IsOwnDepot);

			instruction.OrganisationType = "CNE";
			AssertEquals(false, confirmation.IsOwnDepot);
		}

		#endregion

		protected void AssertFlag(ZString flagPropertyName, ZString validCode, ZString invalidCode)
		{
			var confirmation = (DtbTransportConfirmation)GetNewBusinessObject();

			AssertEquals(false, confirmation[flagPropertyName]);

			confirmation.KK_ConfirmationType = validCode;
			AssertEquals(true, confirmation[flagPropertyName]);

			confirmation.KK_ConfirmationType = invalidCode;
			AssertEquals(false, confirmation[flagPropertyName]);
		}

		#endregion

		#region TestIConsignmentAction_Properties

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

		protected virtual void SetActualQuantity(DtbTransportConfirmation confirmation, int quantity)
		{
			confirmation.KK_Quantity = quantity;
		}

		#endregion

		#region Implementation

		protected abstract DtbTransportInstruction GetNewInstruction();
		protected abstract DtbTransport GetNewTransport();

		#endregion
	}
}

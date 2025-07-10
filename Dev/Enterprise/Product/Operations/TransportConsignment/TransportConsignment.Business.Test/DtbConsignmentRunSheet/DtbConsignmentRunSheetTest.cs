using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbConsignmentRunSheet))]
	sealed class DtbConsignmentRunSheetTest : DtbBookingConsignmentBusinessObjectTestCase
	{
		#region ICustomFieldProvider

		[TestedType(typeof(DtbConsignmentRunSheet))]
		class CustomFieldsTest : TestICustomFieldProvider
		{
		}

		public void TestICustomFieldProvider()
		{
			var customFieldProvider = Factory.NewWithValidTestData<DtbConsignmentRunSheet>() as ICustomFieldProvider;
			AssertNotNull(customFieldProvider);
			var customBusinessObject = customFieldProvider.GetCustomBusinessObject();
			AssertNotNull(customBusinessObject);
		}

		public void TestCustomFieldsReadOnlyState_IsTheSameAsDtbConsignmentReadOnlyState_WhenBizoReadOnlyStateChanged()
		{
			var runSheet = Factory.New<DtbConsignmentRunSheet>();

			runSheet.ReadOnly = true;
			var runSheetProvider = (ICustomFieldProvider)runSheet;
			var runSheetCustomBizo = runSheetProvider.GetCustomBusinessObject();
			AssertEquals(runSheet.ReadOnly, runSheetCustomBizo.ReadOnly);

			runSheet.ReadOnly = false;
			runSheetCustomBizo = runSheetProvider.GetCustomBusinessObject();
			AssertEquals(runSheet.ReadOnly, runSheetCustomBizo.ReadOnly);
		}

		public void TestCustomFieldsDoChange_WhenParameter_shouldRefresh_IsTrue()
		{
			var runSheet = Factory.New<DtbConsignmentRunSheet>();

			var runSheetProvider = (ICustomFieldProvider)runSheet;
			var runSheetCustomBizo1 = runSheetProvider.GetCustomBusinessObject();
			var runSheetCustomBizo2 = runSheetProvider.GetCustomBusinessObject(false);
			AssertEquals("Expecting same instance of CustomBusinessObject when shouldRefresh parameter is false.", runSheetCustomBizo1, runSheetCustomBizo2);
			var runSheetCustomBizo3 = runSheetProvider.GetCustomBusinessObject(true);
			AssertNotEquals("Expecting different instance of CustomBusinessObject when shouldRefresh parameter is true.", runSheetCustomBizo1, runSheetCustomBizo3);
		}

		#endregion

		#region Related Entities

		public void TestNoteTypes()
		{
			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			AssertContainsExactElementsInAnyOrder(new[]
			{
				PredefinedNoteTypes.Instance.AutoRatingAuditLog
			}, runSheet.NoteTypes);
		}

		#region TestRunSheetInstructions

		public void TestRunSheetInstructions()
		{
			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			AssertNotNull(runSheet.RunSheetInstructions);
			AssertEquals("Collection should be cached.", runSheet.RunSheetInstructions, runSheet.RunSheetInstructions);
			AssertEquals("Instructions should be registered editable on the RunSheet.", true, runSheet.IsRegisteredEditableChildObject(runSheet.RunSheetInstructions));
		}

		#endregion

		#endregion

		#region TestDefaults

		#region TestKG_StartTimeDefault

		[TestDate(2013, 8, 12)]
		[TestUtcOffset(10, 0, 0)]
		public void TestKG_StartTimeDefault()
		{
			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			AssertEquals("Start time should default to today at 12:00am.", ZDateTimeOffset.Today, runSheet.KG_StartTime);
		}

		#endregion

		#region TestKG_EndTimeDefault

		[TestDate(2013, 8, 12)]
		[TestUtcOffset(10, 0, 0)]
		public void TestKG_EndTimeDefault()
		{
			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			AssertEquals("End time should default to today at 23:59pm.", ZDateTimeOffset.Today.EndOfDay(), runSheet.KG_EndTime);
		}

		#endregion

		#region TestKG_Duration

		public void TestKG_Duration()
		{
			var year = ZDateTime.Now.Year;
			var runsheet = Factory.New<DtbConsignmentRunSheet>();
			runsheet.KG_EndTime = new ZDateTimeOffset(new ZDateTime(year, 06, 05, 11, 45, 00), DateTimeKind.Local);
			runsheet.KG_StartTime = new ZDateTimeOffset(new ZDateTime(year, 06, 05, 10, 40, 00), DateTimeKind.Local);
			AssertEquals("01:05", runsheet.KG_Duration.ToShortTimeString());

			runsheet.KG_Duration = new ZDateTime(year, 1, 1, 5, 10, 0);
			AssertEquals("05:10", runsheet.KG_Duration.ToShortTimeString());

			runsheet.KG_Duration = ZDateTime.Empty;
			AssertEquals("01:05", runsheet.KG_Duration.ToShortTimeString());
		}

		public void TestKG_DurationWhenStartAndEndTimeChanges()
		{
			var year = ZDateTimeOffset.Now.Year;
			var runsheet = Factory.New<DtbConsignmentRunSheet>();
			runsheet.KG_EndTime = new ZDateTimeOffset(new ZDateTime(year, 06, 05, 11, 45, 00), DateTimeKind.Local);
			AssertEquals("11:45", runsheet.KG_Duration.ToShortTimeString());

			runsheet.KG_StartTime = new ZDateTimeOffset(new ZDateTime(year, 06, 05, 10, 40, 00), DateTimeKind.Local);
			AssertEquals("01:05", runsheet.KG_Duration.ToShortTimeString());

			runsheet.KG_Duration = new ZDateTime(year, 1, 1, 5, 10, 0);
			AssertEquals("05:10", runsheet.KG_Duration.ToShortTimeString());

			runsheet.KG_EndTime = new ZDateTimeOffset(new ZDateTime(year, 06, 05, 12, 50, 00), DateTimeKind.Local);
			AssertEquals("Ensure it has overriden our manually entered value.", "02:10", runsheet.KG_Duration.ToShortTimeString());

			runsheet.KG_StartTime = new ZDateTimeOffset(new ZDateTime(year, 06, 05, 12, 40, 00), DateTimeKind.Local);
			runsheet.KG_EndTime = new ZDateTimeOffset(new ZDateTime(2015, 06, 05, 10, 50, 00), DateTimeKind.Local);
			AssertEquals("22:10", runsheet.KG_Duration.ToShortTimeString());
		}

		#endregion

		public void TestKG_GB_Branch()
		{
			var runsheet = Factory.New<DtbConsignmentRunSheet>();
			AssertEquals(GlbBranch.CurrentBranch.PK, runsheet.KG_GB_Branch);
		}

		#endregion

		#region Properties

		#region TestKG_AdHocDriversName_ReadOnly

		public void TestKG_AdHocDriversName_ReadOnly()
		{
			var runSheet = Helper.CreateRunSheet();
			AssertEquals("Precondition", false, runSheet.KG_AdHocDriversNameInfo.ReadOnly);

			runSheet.KG_GS_NKTruckDriver = "XX";
			AssertEquals(false, runSheet.KG_AdHocDriversNameInfo.ReadOnly);

			runSheet.KG_GS_NKTruckDriver = Helper.CreateDriver("Barney Rubble", "Barny").GS_Code;
			AssertEquals(true, runSheet.KG_AdHocDriversNameInfo.ReadOnly);
		}

		#endregion

		#region TestKG_AdHocDriversLicence_ReadOnly

		public void TestKG_AdHocDriversLicence_ReadOnly()
		{
			var runSheet = Helper.CreateRunSheet();
			AssertEquals("Precondition", false, runSheet.KG_AdHocDriversLicenceInfo.ReadOnly);

			runSheet.KG_GS_NKTruckDriver = "XX";
			AssertEquals(false, runSheet.KG_AdHocDriversLicenceInfo.ReadOnly);

			runSheet.KG_GS_NKTruckDriver = Helper.CreateDriver("Barney Rubble", "Barny").GS_Code;
			AssertEquals(true, runSheet.KG_AdHocDriversLicenceInfo.ReadOnly);
		}

		#endregion

		#region TestKG_AdHocTruckRegistration_ReadOnly

		public void TestKG_AdHocTruckRegistration_ReadOnly()
		{
			var runSheet = Helper.CreateRunSheet();
			AssertEquals("Precondition", false, runSheet.KG_AdHocTruckRegistrationInfo.ReadOnly);

			runSheet.KG_RQ_Truck = ZGuid.NewZGuid();
			AssertEquals(false, runSheet.KG_AdHocTruckRegistrationInfo.ReadOnly);

			runSheet.KG_RQ_Truck = Helper.CreateVehicle("V1").PK;
			AssertEquals(true, runSheet.KG_AdHocTruckRegistrationInfo.ReadOnly);
		}

		#endregion

		#region TestKG_AdHocTransportCoName_ReadOnly

		public void TestKG_AdHocTransportCoName_ReadOnly()
		{
			var runSheet = Helper.CreateRunSheet();
			AssertEquals("Precondition", false, runSheet.KG_AdHocTransportCoNameInfo.ReadOnly);

			runSheet.KG_OH_TransportCo = ZGuid.NewZGuid();
			AssertEquals(false, runSheet.KG_AdHocTransportCoNameInfo.ReadOnly);

			runSheet.KG_OH_TransportCo = Helper.CreateOrganisation("RARA").PK;
			AssertEquals(true, runSheet.KG_AdHocTransportCoNameInfo.ReadOnly);
		}

		#endregion

		#region TestKG_GS_NKTruckDriver

		public void TestKG_GS_NKTruckDriver()
		{
			var runSheet = Helper.CreateRunSheet();

			runSheet.KG_AdHocDriversName = "Fred";
			runSheet.KG_AdHocDriversLicence = "FEET";
			AssertNotEquals("Precondition", "", runSheet.KG_AdHocDriversName);
			AssertNotEquals("Precondition", "", runSheet.KG_AdHocDriversLicence);

			var driver = Helper.CreateDriver("Barney Rubble", "Barny");
			runSheet.KG_GS_NKTruckDriver = driver.GS_Code;
			AssertEquals(driver, runSheet.TruckDriver);
			AssertEquals("", runSheet.KG_AdHocDriversName);
			AssertEquals("", runSheet.KG_AdHocDriversLicence);
		}

		#endregion

		#region TestKG_OH_TransportCo

		public void TestKG_OH_TransportCo()
		{
			var runSheet = Helper.CreateRunSheet();

			runSheet.KG_AdHocTransportCoName = "Fred";
			AssertNotEquals("Precondition", "", runSheet.KG_AdHocTransportCoName);

			var transportCo = Helper.CreateOrganisation("RAR");
			runSheet.KG_OH_TransportCo = transportCo.PK;
			AssertEquals(transportCo.PK, runSheet.KG_OH_TransportCo);
			AssertEquals("", runSheet.KG_AdHocTransportCoName);
		}

		#endregion

		#region TestKG_RQ_Truck

		public void TestKG_RQ_Truck()
		{
			var runSheet = Helper.CreateRunSheet();

			runSheet.KG_AdHocTruckRegistration = "HH252";
			AssertNotEquals("Precondition", "", runSheet.KG_AdHocTruckRegistration);

			var truck = Helper.CreateVehicle("V1");
			runSheet.KG_RQ_Truck = truck.PK;
			AssertEquals(truck.PK, runSheet.KG_RQ_Truck);
			AssertEquals("", runSheet.KG_AdHocTruckRegistration);
		}

		#endregion

		// calculated

		#region TestDriversLicense

		public void TestDriversLicense()
		{
			var runSheet = Helper.CreateRunSheet();
			AssertEquals("", runSheet.DriversLicense);

			runSheet.KG_AdHocDriversLicence = "123";
			AssertEquals("123", runSheet.DriversLicense);

			var driver = Helper.CreateDriver("Hefty Smurf", "Hefty");
			var certification = driver.Certificates.AddNew();
			certification.XZ_Type = CertificateTypePairList.Codes.DG1;
			certification.XZ_RefNumber = "HH252";

			runSheet.KG_GS_NKTruckDriver = driver.GS_Code;
			AssertEquals("", runSheet.DriversLicense);

			certification.XZ_Type = CertificateTypePairList.Codes.CA1;
			AssertEquals("HH252", runSheet.DriversLicense);
		}

		#endregion

		#region TestDriversName

		public void TestDriversName()
		{
			var runSheet = Helper.CreateRunSheet();
			AssertEquals("", runSheet.DriversName);

			runSheet.KG_AdHocDriversName = "Dodgy Trucker";
			AssertEquals("Dodgy Trucker", runSheet.DriversName);

			var driver = Helper.CreateDriver("Hefty Smurf", "Hefty");
			runSheet.KG_GS_NKTruckDriver = driver.GS_Code;
			AssertEquals("Hefty Smurf", runSheet.DriversName);
		}

		#endregion

		#region TestTransportCoName

		public void TestTransportCoName()
		{
			var runSheet = Helper.CreateRunSheet();
			AssertEquals("", runSheet.TransportCoName);

			runSheet.KG_AdHocTransportCoName = "Dodgy Transport Co";
			AssertEquals("Dodgy Transport Co", runSheet.TransportCoName);

			var transportCo = Helper.CreateOrganisation("ABC");
			transportCo.OH_FullName = "ABC Movers";

			runSheet.KG_OH_TransportCo = transportCo.PK;
			AssertEquals("ABC Movers", runSheet.TransportCoName);
		}

		#endregion

		#region TestTruckRegistration

		public void TestTruckRegistration()
		{
			var runSheet = Helper.CreateRunSheet();
			AssertEquals("", runSheet.TruckRegistration);

			runSheet.KG_AdHocTruckRegistration = "DodgyReg";
			AssertEquals("DodgyReg", runSheet.TruckRegistration);

			var truck = Helper.CreateVehicle("V1");
			truck.RQ_Registration = "ABC";

			runSheet.KG_RQ_Truck = truck.PK;
			AssertEquals("ABC", runSheet.TruckRegistration);
		}

		#endregion

		#region IsHazardous

		public void TestIsHazardous()
		{
			var runSheet = Helper.CreateRunSheet();
			AssertEquals("Precondition: No Hazardous Goods on an empty runsheet.", false, runSheet.IsHazardous);

			var consignment1 = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			var consignment2 = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			var addressPoint1 = Helper.CreateAddressPoint(consignment1.PickupInstruction.Address.Address, consignment1.PickupInstruction.Confirmations[0]);
			var addressPoint2 = Helper.CreateAddressPoint(consignment2.DeliveryInstruction.Address.Address, consignment2.DeliveryInstruction.Confirmations[0]);
			var runSheetInstruction1 = runSheet.AddNewRunSheetInstructions(new[] { addressPoint1 })[0];
			var runSheetInstruction2 = runSheet.AddNewRunSheetInstructions(new[] { addressPoint2 })[0];
			AssertEquals(false, runSheet.IsHazardous);

			consignment1.KM_IsHazardous = true;
			AssertEquals(true, runSheet.IsHazardous);

			consignment1.KM_IsHazardous = false;
			AssertEquals(false, runSheet.IsHazardous);

			consignment2.KM_IsHazardous = true;
			AssertEquals(true, runSheet.IsHazardous);
		}

		public void TestIsHazardous_Refreshed()
		{
			var runSheet = Helper.CreateRunSheet();
			AssertEquals("Precondition: No Hazardous Goods on an empty runsheet.", false, runSheet.IsHazardous);

			var hasIsHazardousRefreshed = false;
			runSheet.IsHazardousInfo.ValueChanged += delegate
			{ hasIsHazardousRefreshed = true; };
			AssertEquals(false, hasIsHazardousRefreshed);

			var consignment1 = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			var consignment2 = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			var addressPoint1 = Helper.CreateAddressPoint(consignment1.PickupInstruction.Address.Address, consignment1.PickupInstruction.Confirmations[0]);

			// Add RunSheet Instruction
			AssertEquals(false, hasIsHazardousRefreshed);
			var runSheetInstruction1 = runSheet.AddNewRunSheetInstructions(new[] { addressPoint1 })[0];
			AssertEquals(true, hasIsHazardousRefreshed);

			// Remove RunSheet Instruction
			hasIsHazardousRefreshed = false;
			runSheet.RemoveRunSheetInstructions(true, runSheetInstruction1);
			AssertEquals(true, hasIsHazardousRefreshed);

			// Add new confirmation
			var runSheetInstruction2 = runSheet.AddNewRunSheetInstructions(new[] { addressPoint1 })[0];
			hasIsHazardousRefreshed = false;

			AssertEquals(false, runSheet.IsHazardous);
			consignment2.KM_IsHazardous = true;
			runSheetInstruction2.Confirmations.Add(consignment2.PickupInstruction.Confirmations[0]);
			AssertEquals(true, hasIsHazardousRefreshed);
			AssertEquals(true, runSheet.IsHazardous);

			// Remove new confirmation
			hasIsHazardousRefreshed = false;
			runSheetInstruction2.Confirmations.RemoveFromRelationship(consignment2.PickupInstruction.Confirmations[0]);
			AssertEquals(true, hasIsHazardousRefreshed);
			AssertEquals(false, runSheet.IsHazardous);

			// Assert No Exceptions Thrown With Deleted Run Sheet Instructions
			// This happened a couple of times functionally but we were unable to reliably reproduce
			runSheetInstruction2.Confirmations.Add(consignment2.PickupInstruction.Confirmations[0]);
			hasIsHazardousRefreshed = false;
			AssertEquals(true, runSheet.IsHazardous);

			using (((IBusinessObjectCollection)runSheet.RunSheetInstructions).SuspendListChanged())
			{
				runSheetInstruction2.Delete();
			}

			AssertEquals(true, hasIsHazardousRefreshed);
			AssertEquals(false, runSheet.IsHazardous);
		}

		#endregion

		#region RequiresRefrigeration

		public void TestRequiresRefrigeration()
		{
			var runSheet = Helper.CreateRunSheet();
			AssertEquals("Precondition: No goods that Require Refrigeration on an empty runsheet.", false, runSheet.RequiresRefrigeration);

			var consignment1 = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			var consignment2 = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			var addressPoint1 = Helper.CreateAddressPoint(consignment1.PickupInstruction.Address.Address, consignment1.PickupInstruction.Confirmations[0]);
			var addressPoint2 = Helper.CreateAddressPoint(consignment2.DeliveryInstruction.Address.Address, consignment2.DeliveryInstruction.Confirmations[0]);
			var runSheetInstruction1 = runSheet.AddNewRunSheetInstructions(new[] { addressPoint1 })[0];
			var runSheetInstruction2 = runSheet.AddNewRunSheetInstructions(new[] { addressPoint2 })[0];
			AssertEquals(false, runSheet.RequiresRefrigeration);

			consignment1.KM_RequiresRefrigeration = true;
			AssertEquals(true, runSheet.RequiresRefrigeration);

			consignment1.KM_RequiresRefrigeration = false;
			AssertEquals(false, runSheet.RequiresRefrigeration);

			consignment2.KM_RequiresRefrigeration = true;
			AssertEquals(true, runSheet.RequiresRefrigeration);
		}

		public void TestRequiresRefrigeration_Refreshed()
		{
			var runSheet = Helper.CreateRunSheet();
			AssertEquals("Precondition: No goods that Require Refrigeration on an empty runsheet.", false, runSheet.RequiresRefrigeration);

			var hasRequiresRefrigerationRefreshed = false;
			runSheet.RequiresRefrigerationInfo.ValueChanged += delegate
			{
				hasRequiresRefrigerationRefreshed = true;
			};

			AssertEquals(false, hasRequiresRefrigerationRefreshed);

			var consignment1 = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			var consignment2 = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			var addressPoint1 = Helper.CreateAddressPoint(consignment1.PickupInstruction.Address.Address, consignment1.PickupInstruction.Confirmations[0]);

			// Add RunSheet Instruction
			AssertEquals(false, hasRequiresRefrigerationRefreshed);
			var runSheetInstruction1 = runSheet.AddNewRunSheetInstructions(new[] { addressPoint1 })[0];
			AssertEquals(true, hasRequiresRefrigerationRefreshed);

			// Remove RunSheet Instruction
			hasRequiresRefrigerationRefreshed = false;
			runSheet.RemoveRunSheetInstructions(true, runSheetInstruction1);
			AssertEquals(true, hasRequiresRefrigerationRefreshed);

			// Add new confirmation
			var runSheetInstruction2 = runSheet.AddNewRunSheetInstructions(new[] { addressPoint1 })[0];
			hasRequiresRefrigerationRefreshed = false;

			AssertEquals(false, runSheet.RequiresRefrigeration);
			consignment2.KM_RequiresRefrigeration = true;
			runSheetInstruction2.Confirmations.Add(consignment2.PickupInstruction.Confirmations[0]);
			AssertEquals(true, hasRequiresRefrigerationRefreshed);
			AssertEquals(true, runSheet.RequiresRefrigeration);

			// Remove new confirmation
			hasRequiresRefrigerationRefreshed = false;
			runSheetInstruction2.Confirmations.RemoveFromRelationship(consignment2.PickupInstruction.Confirmations[0]);
			AssertEquals(true, hasRequiresRefrigerationRefreshed);
			AssertEquals(false, runSheet.RequiresRefrigeration);

			// Assert No Exceptions Thrown With Deleted Run Sheet Instructions
			// This happened a couple of times functionally but we were unable to reliably reproduce
			runSheetInstruction2.Confirmations.Add(consignment2.PickupInstruction.Confirmations[0]);
			hasRequiresRefrigerationRefreshed = false;
			AssertEquals(true, runSheet.RequiresRefrigeration);

			using (((IBusinessObjectCollection)runSheet.RunSheetInstructions).SuspendListChanged())
			{
				runSheetInstruction2.Delete();
			}

			AssertEquals(true, hasRequiresRefrigerationRefreshed);
			AssertEquals(false, runSheet.RequiresRefrigeration);
		}

		#endregion

		#endregion

		#region Flags

		#region Status

		public void TestStatus()
		{
			var runSheet = Helper.CreateRunSheet();
			AssertEquals("pre-requisite", true, runSheet.KG_IsPlanning);
			AssertEquals("The default status of run sheets is Not Started.", "Planning", runSheet.Status);
			AssertEquals(false, runSheet.IsInProgress);

			var consignment = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			var consignment2 = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			var addressPoint1 = Helper.CreateAddressPoint(consignment.PickupInstruction.Address.Address, consignment.PickupInstruction.Confirmations[0]);
			var addressPoint2 = Helper.CreateAddressPoint(consignment2.DeliveryInstruction.Address.Address, consignment2.DeliveryInstruction.Confirmations[0]);
			var runSheetInstruction1 = runSheet.AddNewRunSheetInstructions(new[] { addressPoint1 })[0];
			var runSheetInstruction2 = runSheet.AddNewRunSheetInstructions(new[] { addressPoint2 })[0];
			AssertEquals("The default status of run sheets is Planning.", "Planning", runSheet.Status);
			AssertEquals(false, runSheet.IsInProgress);

			runSheet.KG_IsPlanning = false;
			AssertEquals("The default status of run sheets is Not Started.", "Not Started", runSheet.Status);

			runSheetInstruction1.K1_IsAcceptedByDriver = true;
			AssertEquals("Driver accepts the first job, so now in progress.", "In Progress", runSheet.Status);
			AssertEquals(true, runSheet.IsInProgress);

			runSheetInstruction2.K1_IsAcceptedByDriver = true;
			AssertEquals("Driver accepts the second job, still in progress.", "In Progress", runSheet.Status);
			AssertEquals(true, runSheet.IsInProgress);

			runSheetInstruction1.K1_TimeIn = ZDateTimeOffset.Now;
			AssertEquals("Driver sets time in, still in progress.", "In Progress", runSheet.Status);
			AssertEquals(true, runSheet.IsInProgress);

			runSheetInstruction1.K1_TimeOut = ZDateTimeOffset.Now;
			AssertEquals("Job 1 is now complete, but there is still another job to do, so still in progress.", "In Progress", runSheet.Status);
			AssertEquals(true, runSheet.IsInProgress);

			foreach (var route in runSheet.RunSheetInstructions) // complete depot instructions
			{
				SetRunsheetInstructionTimeInOutAndReceivedBy(route, ZDateTimeOffset.Now, ZDateTimeOffset.Now, "TST");
			}
			AssertEquals("All Jobs now have Time In and Out set, so should be Completed and no longer in progress", "Completed", runSheet.Status);
			AssertEquals(false, runSheet.IsInProgress);
		}

		#endregion

		public void TestIsInProgress_OneOfInstructionIsCompleted()
		{
			var runSheet = Helper.CreateRunSheet();
			AssertEquals(false, runSheet.IsInProgress);

			var consignment = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			var addressPoint = Helper.CreateAddressPoint(consignment.PickupInstruction.Address.Address, consignment.PickupInstruction.Confirmations[0]);
			var runSheetInstruction = runSheet.AddNewRunSheetInstructions(new[] { addressPoint })[0];
			AssertEquals(false, runSheet.IsInProgress);

			SetRunsheetInstructionTimeInOutAndReceivedBy(runSheetInstruction, ZDateTimeOffset.Now, ZDateTimeOffset.Now, "TST");
			AssertEquals(true, runSheetInstruction.IsCompleted);
			AssertEquals("One of runsheet instruction is Failed, this runsheet should be InProgress", true, runSheet.IsInProgress);
		}

		void SetRunsheetInstructionTimeInOutAndReceivedBy(DtbConsignmentRunSheetInstruction instruction, ZDateTimeOffset timeIn, ZDateTimeOffset timeOut, ZString receivedBy)
		{
			instruction.K1_TimeIn = timeIn;
			instruction.K1_TimeOut = timeOut;
			instruction.K1_ReceivedBy = receivedBy;
		}

		#region TestHasNewConsignments

		public void TestHasNewConsignments()
		{
			var runSheetWithNewConsignments = Helper.CreateRunSheet();
			var newConsignment = ConsignmentTestHelper.CreateConsignment("C1");
			var addressInConsignment1 = ConsignmentTestHelper.CreateConsignmentAddressWithAction(newConsignment, ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp);
			ConsignmentTestHelper.CreateRunSheetInstruction(runSheetWithNewConsignments, addressInConsignment1.PickupAction);

			var runSheetWithOldConsignments = Helper.CreateRunSheet();
			var oldConsignment = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			runSheetWithOldConsignments.AddNewRunSheetInstructions(new[] { oldConsignment.PickupInstruction.Confirmations.First(c => c.IsPickUp) });

			AssertEquals(true, runSheetWithNewConsignments.HasNewConsignments);
			AssertEquals(false, runSheetWithOldConsignments.HasNewConsignments);
		}

		#endregion

		#region IsStaffDriver

		public void IsStaffDriver()
		{
			var runSheet = Helper.CreateRunSheet();
			AssertEquals(false, runSheet.IsStaffDriver);

			runSheet.KG_AdHocDriversName = "Dodgy Trucker";
			AssertEquals(false, runSheet.IsStaffDriver);

			var driver = Helper.CreateDriver("Hefty Smurf", "Hefty");
			runSheet.KG_GS_NKTruckDriver = driver.GS_Code;
			AssertEquals(true, runSheet.IsStaffDriver);
		}

		#endregion

		#region TestInstructionsHaveAnyDirectDeliveryConfirmations

		public void TestInstructionsHaveAnyDirectDeliveryConfirmations()
		{
			var directConsignment = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			var runSheet1 = Helper.CreateRunSheet();
			var pickupRouteInstruction = Helper.CreateRunSheetInstruction(runSheet1, directConsignment.PickupInstruction.PickupConfirmation);
			var deliveryRouteInstruction = Helper.CreateRunSheetInstruction(runSheet1, directConsignment.DeliveryInstruction.DeliveryConfirmation);
			AssertEquals("Pickups are not deliveries.", false, runSheet1.InstructionsHaveAnyDirectDeliveryConfirmations(new[] { pickupRouteInstruction }));
			AssertEquals("RunSheet instructions with direct deliveries should be considered direct.", true,
				runSheet1.InstructionsHaveAnyDirectDeliveryConfirmations(new[] { deliveryRouteInstruction }));
			AssertEquals("If the related Pickup of a direct delivery is also selected for removal there is no point asking the user to remove the Related Pickup.",
				false, runSheet1.InstructionsHaveAnyDirectDeliveryConfirmations(new[] { pickupRouteInstruction, deliveryRouteInstruction }));

			var consignment = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			var runSheet2 = Helper.CreateRunSheet();
			var runSheet3 = Helper.CreateRunSheet();
			Helper.CreateRunSheetInstruction(runSheet2, consignment.PickupInstruction.PickupConfirmation);
			var nonDirectDelivery = Helper.CreateRunSheetInstruction(runSheet3, consignment.DeliveryInstruction.DeliveryConfirmation);
			AssertEquals("Non-direct Delivery instructions should not be considered direct.", false,
				runSheet3.InstructionsHaveAnyDirectDeliveryConfirmations(new[] { nonDirectDelivery }));
		}

		#endregion

		#region TestSomeDeliveryConfirmationsAreDirect

		public void TestSomeDeliveryConfirmationsAreDirect()
		{
			var directConsignment = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			var runSheet1 = Helper.CreateRunSheet();
			var pickup = Helper.CreateRunSheetInstruction(runSheet1, directConsignment.PickupInstruction.PickupConfirmation);
			Helper.CreateRunSheetInstruction(runSheet1, directConsignment.DeliveryInstruction.DeliveryConfirmation);
			AssertEquals("Pickup Confirmations are not deliveries.", false,
				runSheet1.SomeDeliveryConfirmationsAreDirect(new[] { directConsignment.PickupInstruction.PickupConfirmation }));
			AssertEquals("The selected confirmations contain a direct delivery.", true,
				runSheet1.SomeDeliveryConfirmationsAreDirect(new[] { directConsignment.DeliveryInstruction.DeliveryConfirmation }));

			SetRunsheetInstructionTimeInOutAndReceivedBy(pickup, ZDateTimeOffset.Now, ZDateTimeOffset.Now, "TST");
			AssertEquals("If a direct delivery's pickup is completed it cannot be removed and thus there is no reason to ask the user to unallocate the pickup.", false,
				runSheet1.SomeDeliveryConfirmationsAreDirect(new[] { directConsignment.DeliveryInstruction.DeliveryConfirmation }));

			var consignment = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			var runSheet2 = Helper.CreateRunSheet();
			var runSheet3 = Helper.CreateRunSheet();
			Helper.CreateRunSheetInstruction(runSheet2, consignment.PickupInstruction.PickupConfirmation);
			Helper.CreateRunSheetInstruction(runSheet3, consignment.DeliveryInstruction.DeliveryConfirmation);
			AssertEquals("Pickup Confirmations are not deliveries.", false,
				runSheet2.SomeDeliveryConfirmationsAreDirect(new[] { consignment.PickupInstruction.PickupConfirmation }));
			AssertEquals("Non-direct deliveries should not be considered Direct Deliveries.", false,
				runSheet3.SomeDeliveryConfirmationsAreDirect(new[] { consignment.DeliveryInstruction.DeliveryConfirmation }));
		}

		#endregion

		#endregion

		#region TestAddNewRunSheetInstructions

		#region Add Once

		public void TestAddRoutes_AddPickup()
		{
			var picConfirmation = GetPickupConfirmation();

			//#warning uncomment once we handle adding new confirmations (they currently have no PackTotals cache and therefore will return 0 until refreshed)
			// add packages
			//var package = picConfirmation.Instruction.Booking.PackageJob.Packages.AddNew("PLT", "ABC123");
			//package.KP_Weight = 100m;
			//package.KP_Volume = 50m;
			//Factory.Save();

			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			runSheet.AddNewRunSheetInstructions(GetAddressPoints(picConfirmation));

			var depotConfirmation = AssertDepotConfirmation(picConfirmation.Instruction.Booking, ConfirmationTypes.Codes.Delivery);
			AssertRunSheetInstruction(runSheet, 1, new[] { picConfirmation });
			AssertRunSheetInstruction(runSheet, 2, new[] { depotConfirmation });
			AssertEquals(2, runSheet.RunSheetInstructions.Count);

			//#warning uncomment once we handle adding new confirmations (they currently have no PackTotals cache and therefore will return 0 until refreshed)
			// ensure totals cache is updated
			//var depotRSI = runSheet.RunSheetInstructions.Single(i => i.K1_Sequence == 2);
			//AssertEquals(0m, depotRSI.TotalPickupWeight);
			//AssertEquals(10m, depotRSI.TotalDeliveryWeight);
		}

		public void TestAddRoutes_AddDelivery()
		{
			TestAddRoutes_AddDelivery_Core((c) => { });
		}

		public void TestAddRoutes_AddDelivery_CFS()
		{
			TestAddRoutes_AddDelivery_Core((c) => { c.Instruction.OrganisationType = "CFS"; });
		}

		void TestAddRoutes_AddDelivery_Core(Action<DtbConsignmentConfirmation> changeConfirmation)
		{
			var dlvConfirmation = GetDeliveryConfirmation();
			changeConfirmation(dlvConfirmation);

			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			runSheet.AddNewRunSheetInstructions(GetAddressPoints(dlvConfirmation));

			var depotConfirmation = AssertDepotConfirmation(dlvConfirmation.Instruction.Booking, ConfirmationTypes.Codes.PickUp);
			AssertRunSheetInstruction(runSheet, 1, new[] { depotConfirmation });
			AssertRunSheetInstruction(runSheet, 2, new[] { dlvConfirmation });
			AssertEquals(2, runSheet.RunSheetInstructions.Count);
		}

		public void TestAddRoutes_AddPickupAndDelivery()
		{
			TestAddRoutes_AddPickupAndDelivery_Core((c1, c2) => { });
		}

		public void TestAddRoutes_AddPickupAndDelivery_PickupCFS()
		{
			TestAddRoutes_AddPickupAndDelivery_Core((c1, c2) => { c1.Instruction.OrganisationType = "CFS"; });
		}

		public void TestAddRoutes_AddPickupAndDelivery_PickupAndDeliveryCFS()
		{
			TestAddRoutes_AddPickupAndDelivery_Core((c1, c2) => { c1.Instruction.OrganisationType = "CFS"; });
		}

		void TestAddRoutes_AddPickupAndDelivery_Core(Action<DtbConsignmentConfirmation, DtbConsignmentConfirmation> changeConfirmations)
		{
			var picConfirmationC1 = GetPickupConfirmation();
			var dlvConfirmationC2 = GetDeliveryConfirmation();
			changeConfirmations(picConfirmationC1, dlvConfirmationC2);

			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			runSheet.AddNewRunSheetInstructions(GetAddressPoints(new[] { picConfirmationC1, dlvConfirmationC2 })); // Consolidated Address Point should stay consolidated

			var dlvDepotConfirmation = AssertDepotConfirmation(picConfirmationC1.Instruction.Booking, ConfirmationTypes.Codes.Delivery);
			var picDepotConfirmation = AssertDepotConfirmation(dlvConfirmationC2.Instruction.Booking, ConfirmationTypes.Codes.PickUp);
			AssertRunSheetInstruction(runSheet, 1, new[] { picDepotConfirmation });
			AssertRunSheetInstruction(runSheet, 2, new[] { dlvConfirmationC2, picConfirmationC1 });
			AssertRunSheetInstruction(runSheet, 3, new[] { dlvDepotConfirmation });
			AssertEquals(3, runSheet.RunSheetInstructions.Count);
		}

		public void TestAddRoutes_AddDeliveryAndPickup()
		{
			var picConfirmation = GetPickupConfirmation();
			var dlvConfirmation = GetDeliveryConfirmation();

			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			runSheet.AddNewRunSheetInstructions(GetAddressPoints(new[] { dlvConfirmation, picConfirmation }));

			var dlvDepotConfirmation = AssertDepotConfirmation(picConfirmation.Instruction.Booking, ConfirmationTypes.Codes.Delivery);
			var picDepotConfirmation = AssertDepotConfirmation(dlvConfirmation.Instruction.Booking, ConfirmationTypes.Codes.PickUp);
			AssertRunSheetInstruction(runSheet, 1, new[] { picDepotConfirmation });
			AssertRunSheetInstruction(runSheet, 2, new[] { picConfirmation, dlvConfirmation });
			AssertRunSheetInstruction(runSheet, 3, new[] { dlvDepotConfirmation });
			AssertEquals(3, runSheet.RunSheetInstructions.Count);
		}

		public void TestAddRoutes_AddPickupAndDelivery_SameConsignment()
		{
			var picConfirmation = GetPickupConfirmation();
			var consignment = picConfirmation.Instruction.Booking;
			var dlvConfirmation = GetDeliveryConfirmation("BBB", picConfirmation.Instruction);

			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			runSheet.AddNewRunSheetInstructions(GetAddressPoints(new[] { picConfirmation, dlvConfirmation }));
			AssertRunSheetInstruction(runSheet, 1, new[] { picConfirmation });
			AssertRunSheetInstruction(runSheet, 2, new[] { dlvConfirmation });
			AssertEquals(2, runSheet.RunSheetInstructions.Count);
		}

		public void TestAddRoutes_AddPickupAndDelivery_SameConsignment_PickupFromExternalDepot()
		{
			var picConfirmation = GetPickupConfirmation();
			picConfirmation.Instruction.OrganisationType = "CFS";
			var consignment = picConfirmation.Instruction.Booking;
			var dlvConfirmation = GetDeliveryConfirmation("BBB", picConfirmation.Instruction);

			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			runSheet.AddNewRunSheetInstructions(GetAddressPoints(new[] { picConfirmation, dlvConfirmation }));
			AssertRunSheetInstruction(runSheet, 1, new[] { picConfirmation });
			AssertRunSheetInstruction(runSheet, 2, new[] { dlvConfirmation });
			AssertEquals(2, runSheet.RunSheetInstructions.Count);
		}

		[ExpectException(typeof(NotSupportedException))]
		public void TestAddRoutes_AddDeliveryAndPickup_SameConsignment()
		{
			var picConfirmation = GetPickupConfirmation();
			var consignment = picConfirmation.Instruction.Booking;
			var dlvConfirmation = GetDeliveryConfirmation("BBB", picConfirmation.Instruction);

			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			runSheet.AddNewRunSheetInstructions(GetAddressPoints(new[] { dlvConfirmation, picConfirmation }));
		}

		#endregion

		#region Add Twice

		public void TestAddRoutes_AddPickup_AddDelivery()
		{
			var picConfirmation = GetPickupConfirmation();
			var dlvConfirmation = GetDeliveryConfirmation("BBB");

			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			runSheet.AddNewRunSheetInstructions(GetAddressPoints(new[] { picConfirmation })); // tested in TestEmptyRunSheet_AddPickup
			runSheet.AddNewRunSheetInstructions(GetAddressPoints(new[] { dlvConfirmation }));

			var dlvDepotConfirmation = AssertDepotConfirmation(picConfirmation.Instruction.Booking, ConfirmationTypes.Codes.Delivery);
			var picDepotConfirmation = AssertDepotConfirmation(dlvConfirmation.Instruction.Booking, ConfirmationTypes.Codes.PickUp);
			AssertRunSheetInstruction(runSheet, 1, new[] { picDepotConfirmation });
			AssertRunSheetInstruction(runSheet, 2, new[] { picConfirmation });
			AssertRunSheetInstruction(runSheet, 3, new[] { dlvConfirmation });
			AssertRunSheetInstruction(runSheet, 4, new[] { dlvDepotConfirmation });
			AssertEquals(4, runSheet.RunSheetInstructions.Count);
		}

		#endregion

		#region Add Same Address Twice

		public void TestAddRoutes_AddPickup_AddSameDelivery()
		{
			var picConfirmation = GetPickupConfirmation();
			var dlvConfirmation = GetDeliveryConfirmation();

			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			runSheet.AddNewRunSheetInstructions(GetAddressPoints(new[] { picConfirmation })); // tested in TestEmptyRunSheet_AddPickup
			runSheet.AddNewRunSheetInstructions(GetAddressPoints(new[] { dlvConfirmation }));

			var dlvDepotConfirmation = AssertDepotConfirmation(picConfirmation.Instruction.Booking, ConfirmationTypes.Codes.Delivery);
			var picDepotConfirmation = AssertDepotConfirmation(dlvConfirmation.Instruction.Booking, ConfirmationTypes.Codes.PickUp);
			AssertRunSheetInstruction(runSheet, 1, new[] { picDepotConfirmation });
			AssertRunSheetInstruction(runSheet, 2, new[] { picConfirmation });
			AssertRunSheetInstruction(runSheet, 3, new[] { dlvConfirmation });
			AssertRunSheetInstruction(runSheet, 4, new[] { dlvDepotConfirmation });
			AssertEquals(4, runSheet.RunSheetInstructions.Count);
		}

		public void TestAddRoutes_AddDelivery_AddSamePickup()
		{
			var picConfirmation = GetPickupConfirmation();
			var dlvConfirmation = GetDeliveryConfirmation();

			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			runSheet.AddNewRunSheetInstructions(GetAddressPoints(new[] { dlvConfirmation })); // tested in TestEmptyRunSheet_AddDelivery
			runSheet.AddNewRunSheetInstructions(GetAddressPoints(new[] { picConfirmation }));

			var dlvDepotConfirmation = AssertDepotConfirmation(picConfirmation.Instruction.Booking, ConfirmationTypes.Codes.Delivery);
			var picDepotConfirmation = AssertDepotConfirmation(dlvConfirmation.Instruction.Booking, ConfirmationTypes.Codes.PickUp);
			AssertRunSheetInstruction(runSheet, 1, new[] { picDepotConfirmation });
			AssertRunSheetInstruction(runSheet, 2, new[] { dlvConfirmation });
			AssertRunSheetInstruction(runSheet, 3, new[] { picConfirmation });
			AssertRunSheetInstruction(runSheet, 4, new[] { dlvDepotConfirmation });
			AssertEquals(4, runSheet.RunSheetInstructions.Count);
		}

		public void TestAddRoutes_AddSamePickupTwice()
		{
			var picConfirmation = GetPickupConfirmation();
			var pic2Confirmation = GetPickupConfirmation();

			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			runSheet.AddNewRunSheetInstructions(GetAddressPoints(new[] { picConfirmation })); // tested in TestEmptyRunSheet_AddPickup
			runSheet.AddNewRunSheetInstructions(GetAddressPoints(new[] { pic2Confirmation }));

			var dlvDepotConfirmation = AssertDepotConfirmation(picConfirmation.Instruction.Booking, ConfirmationTypes.Codes.Delivery);
			var dlv2DepotConfirmation = AssertDepotConfirmation(pic2Confirmation.Instruction.Booking, ConfirmationTypes.Codes.Delivery);
			AssertRunSheetInstruction(runSheet, 1, new[] { picConfirmation });
			AssertRunSheetInstruction(runSheet, 2, new[] { pic2Confirmation });
			AssertRunSheetInstruction(runSheet, 3, new[] { dlvDepotConfirmation, dlv2DepotConfirmation });
			AssertEquals(3, runSheet.RunSheetInstructions.Count);
		}

		public void TestAddRoutes_AddSameDeliveryTwice()
		{
			var dlvConfirmation = GetDeliveryConfirmation();
			var dlv2Confirmation = GetDeliveryConfirmation();

			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			runSheet.AddNewRunSheetInstructions(GetAddressPoints(new[] { dlvConfirmation })); // tested in TestEmptyRunSheet_AddDelivery
			runSheet.AddNewRunSheetInstructions(GetAddressPoints(new[] { dlv2Confirmation }));

			var picDepotConfirmation = AssertDepotConfirmation(dlvConfirmation.Instruction.Booking, ConfirmationTypes.Codes.PickUp);
			var pic2DepotConfirmation = AssertDepotConfirmation(dlv2Confirmation.Instruction.Booking, ConfirmationTypes.Codes.PickUp);
			AssertRunSheetInstruction(runSheet, 1, new[] { picDepotConfirmation, pic2DepotConfirmation });
			AssertRunSheetInstruction(runSheet, 2, new[] { dlvConfirmation });
			AssertRunSheetInstruction(runSheet, 3, new[] { dlv2Confirmation });
			AssertEquals(3, runSheet.RunSheetInstructions.Count);
		}

		#endregion

		#region Add Many Once

		public void TestAddRoutes_AddManyPickups()
		{
			var picConfirmationA = GetPickupConfirmation();
			var picConfirmationB = GetPickupConfirmation("BBB");
			var picConfirmationC = GetPickupConfirmation("CCC");

			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			runSheet.AddNewRunSheetInstructions(GetAddressPoints(new[] { picConfirmationA, picConfirmationB, picConfirmationC })); // 3 address points

			var depotConfirmationA = AssertDepotConfirmation(picConfirmationA.Instruction.Booking, ConfirmationTypes.Codes.Delivery);
			var depotConfirmationB = AssertDepotConfirmation(picConfirmationB.Instruction.Booking, ConfirmationTypes.Codes.Delivery);
			var depotConfirmationC = AssertDepotConfirmation(picConfirmationC.Instruction.Booking, ConfirmationTypes.Codes.Delivery);
			AssertRunSheetInstruction(runSheet, 1, new[] { picConfirmationA });
			AssertRunSheetInstruction(runSheet, 2, new[] { picConfirmationB });
			AssertRunSheetInstruction(runSheet, 3, new[] { picConfirmationC });
			AssertRunSheetInstruction(runSheet, 4, new[] { depotConfirmationA, depotConfirmationB, depotConfirmationC });
			AssertEquals(4, runSheet.RunSheetInstructions.Count);
		}

		public void TestAddRoutes_AddManyDeliveries()
		{
			var dlvConfirmationA = GetDeliveryConfirmation();
			var dlvConfirmationB = GetDeliveryConfirmation("BBB");
			var dlvConfirmationC = GetDeliveryConfirmation("CCC");

			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			runSheet.AddNewRunSheetInstructions(GetAddressPoints(new[] { dlvConfirmationA, dlvConfirmationB, dlvConfirmationC }));

			var depotConfirmationA = AssertDepotConfirmation(dlvConfirmationA.Instruction.Booking, ConfirmationTypes.Codes.PickUp);
			var depotConfirmationB = AssertDepotConfirmation(dlvConfirmationB.Instruction.Booking, ConfirmationTypes.Codes.PickUp);
			var depotConfirmationC = AssertDepotConfirmation(dlvConfirmationC.Instruction.Booking, ConfirmationTypes.Codes.PickUp);
			AssertRunSheetInstruction(runSheet, 1, new[] { depotConfirmationA, depotConfirmationB, depotConfirmationC });
			AssertRunSheetInstruction(runSheet, 2, new[] { dlvConfirmationA });
			AssertRunSheetInstruction(runSheet, 3, new[] { dlvConfirmationB });
			AssertRunSheetInstruction(runSheet, 4, new[] { dlvConfirmationC });
			AssertEquals(4, runSheet.RunSheetInstructions.Count);
		}

		public void TestAddRoutes_AddManyPickupAndDeliveries()
		{
			var picConfirmationA = GetPickupConfirmation();
			var picConfirmationB = GetPickupConfirmation("BBB");
			var picConfirmationC = GetPickupConfirmation("CCC");
			var dlvConfirmationA = GetDeliveryConfirmation();
			var dlvConfirmationB = GetDeliveryConfirmation("BBB");
			var dlvConfirmationC = GetDeliveryConfirmation("CCC");

			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			runSheet.AddNewRunSheetInstructions(GetAddressPoints(new[] { picConfirmationA, dlvConfirmationA, picConfirmationB, dlvConfirmationB, picConfirmationC, dlvConfirmationC })); // 3 address points

			var depotConfirmationDlvA = AssertDepotConfirmation(picConfirmationA.Instruction.Booking, ConfirmationTypes.Codes.Delivery);
			var depotConfirmationDlvB = AssertDepotConfirmation(picConfirmationB.Instruction.Booking, ConfirmationTypes.Codes.Delivery);
			var depotConfirmationDlvC = AssertDepotConfirmation(picConfirmationC.Instruction.Booking, ConfirmationTypes.Codes.Delivery);
			var depotConfirmationPicA = AssertDepotConfirmation(dlvConfirmationA.Instruction.Booking, ConfirmationTypes.Codes.PickUp);
			var depotConfirmationPicB = AssertDepotConfirmation(dlvConfirmationB.Instruction.Booking, ConfirmationTypes.Codes.PickUp);
			var depotConfirmationPicC = AssertDepotConfirmation(dlvConfirmationC.Instruction.Booking, ConfirmationTypes.Codes.PickUp);

			AssertRunSheetInstruction(runSheet, 1, new[] { depotConfirmationPicA, depotConfirmationPicB, depotConfirmationPicC });
			AssertRunSheetInstruction(runSheet, 2, new[] { dlvConfirmationA, picConfirmationA });
			AssertRunSheetInstruction(runSheet, 3, new[] { dlvConfirmationB, picConfirmationB });
			AssertRunSheetInstruction(runSheet, 4, new[] { dlvConfirmationC, picConfirmationC });
			AssertRunSheetInstruction(runSheet, 5, new[] { depotConfirmationDlvA, depotConfirmationDlvB, depotConfirmationDlvC });
			AssertEquals(5, runSheet.RunSheetInstructions.Count);
		}

		public void TestAddRoutes_AddManyPickupAndDeliveries_DirectMode()
		{
			// Cursory test. 
			var consignmentA = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			var consignmentB = Helper.CreateBookingConsignmentWithTemplateAndAddresses();

			var picConfirmationA = consignmentA.PickupInstruction.PickupConfirmation;
			var picConfirmationB = consignmentB.PickupInstruction.PickupConfirmation;
			var dlvConfirmationA = consignmentA.DeliveryInstruction.DeliveryConfirmation;
			var dlvConfirmationB = consignmentB.DeliveryInstruction.DeliveryConfirmation;

			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			runSheet.AddNewRunSheetInstructions(GetAddressPoints(new[] { picConfirmationA, dlvConfirmationA, picConfirmationB, dlvConfirmationB }));

			AssertRunSheetInstruction(runSheet, 1, new[] { picConfirmationA, picConfirmationB });
			AssertRunSheetInstruction(runSheet, 2, new[] { dlvConfirmationA, dlvConfirmationB });
			AssertEquals(2, runSheet.RunSheetInstructions.Count);
		}

		#endregion

		#region Add Many Twice

		public void TestAddRoutes_AddManyPickupsAndDeliveries_Twice()
		{
			var picConfirmationA = GetPickupConfirmation();
			var picConfirmationB = GetPickupConfirmation("BBB");
			var picConfirmationC = GetPickupConfirmation("CCC");
			var picConfirmationD = GetPickupConfirmation("DDD");
			var dlvConfirmationA = GetDeliveryConfirmation();
			var dlvConfirmationB = GetDeliveryConfirmation("BBB");
			var dlvConfirmationC = GetDeliveryConfirmation("CCC");
			var dlvConfirmationD = GetDeliveryConfirmation("DDD");

			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			runSheet.AddNewRunSheetInstructions(GetAddressPoints(new[] { picConfirmationA, dlvConfirmationA, picConfirmationB, dlvConfirmationB })); // tested TestEmptyRunSheet_AddManyPickupsAndDeliveries
			runSheet.AddNewRunSheetInstructions(GetAddressPoints(new[] { picConfirmationC, dlvConfirmationC, picConfirmationD, dlvConfirmationD }));

			var depotConfirmationDlvA = AssertDepotConfirmation(picConfirmationA.Instruction.Booking, ConfirmationTypes.Codes.Delivery);
			var depotConfirmationDlvB = AssertDepotConfirmation(picConfirmationB.Instruction.Booking, ConfirmationTypes.Codes.Delivery);
			var depotConfirmationDlvC = AssertDepotConfirmation(picConfirmationC.Instruction.Booking, ConfirmationTypes.Codes.Delivery);
			var depotConfirmationDlvD = AssertDepotConfirmation(picConfirmationD.Instruction.Booking, ConfirmationTypes.Codes.Delivery);
			var depotConfirmationPicA = AssertDepotConfirmation(dlvConfirmationA.Instruction.Booking, ConfirmationTypes.Codes.PickUp);
			var depotConfirmationPicB = AssertDepotConfirmation(dlvConfirmationB.Instruction.Booking, ConfirmationTypes.Codes.PickUp);
			var depotConfirmationPicC = AssertDepotConfirmation(dlvConfirmationC.Instruction.Booking, ConfirmationTypes.Codes.PickUp);
			var depotConfirmationPicD = AssertDepotConfirmation(dlvConfirmationD.Instruction.Booking, ConfirmationTypes.Codes.PickUp);

			AssertRunSheetInstruction(runSheet, 1, new[] { depotConfirmationPicA, depotConfirmationPicB, depotConfirmationPicC, depotConfirmationPicD });
			AssertRunSheetInstruction(runSheet, 2, new[] { dlvConfirmationA, picConfirmationA });
			AssertRunSheetInstruction(runSheet, 3, new[] { dlvConfirmationB, picConfirmationB });
			AssertRunSheetInstruction(runSheet, 4, new[] { dlvConfirmationC, picConfirmationC });
			AssertRunSheetInstruction(runSheet, 5, new[] { dlvConfirmationD, picConfirmationD });
			AssertRunSheetInstruction(runSheet, 6, new[] { depotConfirmationDlvA, depotConfirmationDlvB, depotConfirmationDlvC, depotConfirmationDlvD });
			AssertEquals(6, runSheet.RunSheetInstructions.Count);
		}

		#endregion

		#region Add Many Many

		public void TestAddRoutes_AddManyPickupsAndDeliveries_Many()
		{
			var picConfirmationA = GetPickupConfirmation();
			var picConfirmationB = GetPickupConfirmation("BBB");
			var picConfirmationC = GetPickupConfirmation("CCC");
			var picConfirmationD = GetPickupConfirmation("DDD");
			var dlvConfirmationA = GetDeliveryConfirmation();
			var dlvConfirmationB = GetDeliveryConfirmation("BBB");
			var dlvConfirmationC = GetDeliveryConfirmation("CCC");
			var dlvConfirmationD = GetDeliveryConfirmation("DDD");

			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			runSheet.AddNewRunSheetInstructions(GetAddressPoints(new[] { picConfirmationA }));
			runSheet.AddNewRunSheetInstructions(GetAddressPoints(new[] { dlvConfirmationD }));
			runSheet.AddNewRunSheetInstructions(GetAddressPoints(new[] { dlvConfirmationC }));
			runSheet.AddNewRunSheetInstructions(GetAddressPoints(new[] { picConfirmationB }));
			runSheet.AddNewRunSheetInstructions(GetAddressPoints(new[] { dlvConfirmationB }));
			runSheet.AddNewRunSheetInstructions(GetAddressPoints(new[] { picConfirmationC }));
			runSheet.AddNewRunSheetInstructions(GetAddressPoints(new[] { picConfirmationD }));
			runSheet.AddNewRunSheetInstructions(GetAddressPoints(new[] { dlvConfirmationA }));

			var depotConfirmationDlvA = AssertDepotConfirmation(picConfirmationA.Instruction.Booking, ConfirmationTypes.Codes.Delivery);
			var depotConfirmationDlvB = AssertDepotConfirmation(picConfirmationB.Instruction.Booking, ConfirmationTypes.Codes.Delivery);
			var depotConfirmationDlvC = AssertDepotConfirmation(picConfirmationC.Instruction.Booking, ConfirmationTypes.Codes.Delivery);
			var depotConfirmationDlvD = AssertDepotConfirmation(picConfirmationD.Instruction.Booking, ConfirmationTypes.Codes.Delivery);
			var depotConfirmationPicA = AssertDepotConfirmation(dlvConfirmationA.Instruction.Booking, ConfirmationTypes.Codes.PickUp);
			var depotConfirmationPicB = AssertDepotConfirmation(dlvConfirmationB.Instruction.Booking, ConfirmationTypes.Codes.PickUp);
			var depotConfirmationPicC = AssertDepotConfirmation(dlvConfirmationC.Instruction.Booking, ConfirmationTypes.Codes.PickUp);
			var depotConfirmationPicD = AssertDepotConfirmation(dlvConfirmationD.Instruction.Booking, ConfirmationTypes.Codes.PickUp);

			AssertRunSheetInstruction(runSheet, 1, new[] { depotConfirmationPicA, depotConfirmationPicB, depotConfirmationPicC, depotConfirmationPicD });
			AssertRunSheetInstruction(runSheet, 2, new[] { picConfirmationA });
			AssertRunSheetInstruction(runSheet, 3, new[] { dlvConfirmationD });
			AssertRunSheetInstruction(runSheet, 4, new[] { dlvConfirmationC });
			AssertRunSheetInstruction(runSheet, 5, new[] { picConfirmationB });
			AssertRunSheetInstruction(runSheet, 6, new[] { dlvConfirmationB });
			AssertRunSheetInstruction(runSheet, 7, new[] { picConfirmationC });
			AssertRunSheetInstruction(runSheet, 8, new[] { picConfirmationD });
			AssertRunSheetInstruction(runSheet, 9, new[] { dlvConfirmationA });
			AssertRunSheetInstruction(runSheet, 10, new[] { depotConfirmationDlvA, depotConfirmationDlvB, depotConfirmationDlvC, depotConfirmationDlvD });
			AssertEquals(10, runSheet.RunSheetInstructions.Count);
		}

		#endregion

		#region Add With Errors

		public void TestAddNewRunSheetInstructions_WithErrors()
		{
			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			AssertExceptionThrown(typeof(ArgumentNullException), () => runSheet.AddNewRunSheetInstructions((DtbAddressPoint[])null));

			// first address point is null
			AssertExceptionThrown(typeof(ArgumentNullException), () => runSheet.AddNewRunSheetInstructions(new DtbAddressPoint[] { null }));

			// consequent address points are null;
			var consignment = Helper.CreateBookingConsignmentWithTemplate();
			var address = Factory.New<OrgAddress>();
			var instruction = consignment.PickupInstruction;
			instruction.Address.E2_OA_Address = address.PK;
			instruction.Booking.KM_JobID = "AU001";
			instruction.Booking.DeliveryInstruction.Address.E2_CompanyName = "Health Department";
			var confirmation = instruction.PickupConfirmation;
			var addressPoint = Helper.CreateAddressPoint(address, confirmation);
			AssertExceptionThrown(typeof(ArgumentNullException), () => runSheet.AddNewRunSheetInstructions(new DtbAddressPoint[] { addressPoint, null }));

			//#warning we may add error instead of exception or allow to reallocate to a new RunSheetInstruction.
			var runSheetInstruction = runSheet.AddNewRunSheetInstructions(new[] { addressPoint }).Single();
			AssertExceptionThrown(typeof(ArgumentException), "One or more confirmations are already attached to a RunSheet Instruction.", () => runSheet.AddNewRunSheetInstructions(new[] { addressPoint }));

			// this situation shouldn't be able to occur in the Route Planner.
			var runSheetWithDelivery = Factory.New<DtbConsignmentRunSheet>();
			runSheetWithDelivery.KG_RunSheetNumber = "X123";
			runSheetWithDelivery.AddNewRunSheetInstructions(consignment.DeliveryInstruction.DeliveryConfirmation);
			var deliveryConfirmation = Helper.CreateConfirmation(instruction, "SLO", "SLOTNUM");
			AssertExceptionThrown("Should have an exception", typeof(NotSupportedException), "A Delivery Confirmation was added to this RunSheet before its Pickup for the same Consignment. Details of the delivery: run sheet number is X123, consignmentID is AU001, consignee name is Health Department.", () => runSheetWithDelivery.AddNewRunSheetInstructions(deliveryConfirmation));
		}

		#endregion

		#region TestAddRoutes_MultipleConfirmationsWithSameOverriddenAddressRollsUp

		public void TestAddRoutes_MultipleConfirmationsWithSameOverriddenAddressRollsUp()
		{
			var consignment1 = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			var consignment2 = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			consignment1.PickupInstruction.Address.E2_AddressOverride = true;
			consignment2.PickupInstruction.Address.E2_AddressOverride = true;

			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			runSheet.AddNewRunSheetInstructions(consignment1.PickupInstruction.PickupConfirmation, consignment2.PickupInstruction.PickupConfirmation);
			AssertEquals("Should have rolled up the overridden addresses into a single Pickup Run Sheet Instruction and one Depot Instruction.",
					2, runSheet.RunSheetInstructions.Count);
			// Overriden addresses should be grouped
			AssertRunSheetInstruction(runSheet, 1, new[] { consignment1.PickupInstruction.PickupConfirmation, consignment2.PickupInstruction.PickupConfirmation });
			AssertEquals("Second Instruction should be a depot.", true, runSheet.RunSheetInstructions[1].IsOwnDepot);
		}

		#endregion

		#region Asserts / Helper Methods

		#region GetOrCreateAddress

		OrgAddress GetOrCreateAddress(string orgCode)
		{
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, orgCode))
				?? Helper.CreateOrganisation(orgCode);

			return org.MainAddress;
		}

		#endregion

		#region GetConfirmation

		DtbConsignmentConfirmation GetPickupConfirmation(string picOrgCode = "AAA")
		{
			var consignment = Helper.CreateBookingConsignment();
			var pickupInstruction = Helper.CreateInstruction(consignment, InstructionTypes.Codes.PickUp, GetOrCreateAddress(picOrgCode));
			return pickupInstruction.Confirmations.Single(c => c.IsPickUp);
		}

		DtbConsignmentConfirmation GetDeliveryConfirmation(string dlvOrgCode = "AAA", DtbConsignmentInstruction pickupInstruction = null)
		{
			if (pickupInstruction == null)
			{
				pickupInstruction = Helper.CreateInstruction(Helper.CreateBookingConsignment(), InstructionTypes.Codes.PickUp, GetOrCreateAddress("P123"));
			}

			var consignment = pickupInstruction.Booking;
			var deliveryInstruction = Helper.CreateInstruction(consignment, InstructionTypes.Codes.Delivery, GetOrCreateAddress(dlvOrgCode));
			return deliveryInstruction.Confirmations.Single(c => c.IsDelivery);
		}

		#endregion

		#region GetAddressPoints

		DtbAddressPoint[] GetAddressPoints(DtbConsignmentConfirmation confirmation)
		{
			return GetAddressPoints(new[] { confirmation });
		}

		DtbAddressPoint[] GetAddressPoints(DtbConsignmentConfirmation[] confirmations)
		{
			var addressPointsByAddress = new Dictionary<OrgAddress, DtbAddressPoint>();

			foreach (var confirmation in confirmations)
			{
				var address = confirmation.Instruction.Address.Address;
				DtbAddressPoint addressPoint;
				if (!addressPointsByAddress.TryGetValue(address, out addressPoint))
				{
					addressPoint = new DtbAddressPoint(confirmations.First().Instruction.Address);
					addressPointsByAddress.Add(address, addressPoint);
				}
				addressPoint.Confirmations.Add(confirmation);
			}

			return addressPointsByAddress.Values.OrderBy((a) =>
				{
					var result = ZString.Empty;

					var address = a.Address;
					if (address != null)
					{
						var org = address.Organisation;
						if (org != null)
						{
							result = org.OH_Code;
						}
					}

					return result;
				}).ToArray();
		}

		#endregion

		#region Assert Depot Confirmation / Instruction

		DtbConsignmentConfirmation AssertDepotConfirmation(DtbBookingConsignment consignment, ZString confirmationType)
		{
			var depotInstruction = AssertDepotInstruction(consignment);
			var depotConfirmations = depotInstruction.Confirmations.Where(c => c.KK_ConfirmationType == confirmationType);
			AssertEquals("Should find exactly 1 depot delivery confirmation.", 1, depotConfirmations.Count());
			return depotConfirmations.First();
		}

		DtbConsignmentInstruction AssertDepotInstruction(DtbBookingConsignment consignment)
		{
			var depotInstructions = consignment.Instructions.Where(i => i.KN_Sequence == 2 && i.KN_InstructionType == InstructionTypes.Codes.Multi && i.OrganisationType == OrganisationTypesList.Codes.CFS);
			AssertEquals("Should find exactly 1 depot instruction.", 1, depotInstructions.Count());
			return depotInstructions.First();
		}

		#endregion

		#region AssertRunSheetInstruction

		void AssertRunSheetInstruction(DtbConsignmentRunSheet runSheet, ZInt sequence, DtbConsignmentConfirmation[] expectedConfirmations)
		{
			var runSheetInstruction = runSheet.RunSheetInstructions.Single(i => i.K1_Sequence == sequence);
			AssertContainsExactElementsInAnyOrder(expectedConfirmations, runSheetInstruction.Confirmations);
		}

		#endregion

		#endregion

		#endregion

		#region TestRemoveRunSheetInstructions

		#region TestRemoveRunSheetInstructions_WrongRelatedConfirmationType

		public void TestRemoveRunSheetInstructions_WrongRelatedConfirmationType()
		{
			var picConfirmation = GetPickupConfirmation();

			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			runSheet.AddNewRunSheetInstructions(GetAddressPoints(picConfirmation));

			picConfirmation.KK_ConfirmationType = null;
			var depotConfirmation = AssertDepotConfirmation(picConfirmation.Instruction.Booking, ConfirmationTypes.Codes.Delivery);
			var depotInstruction = depotConfirmation.Instruction;
			AssertEquals("Precondition", 2, runSheet.RunSheetInstructions.Count);

			AssertNoExceptionThrown(() => { runSheet.RemoveRunSheetInstructions(false, runSheet.RunSheetInstructions.Single(i => i.K1_Sequence == 1)); });
		}

		#endregion

		#region TestRemoveRunSheetInstructions_Pickup

		public void TestRemoveRunSheetInstructions_Pickup()
		{
			var picConfirmation = GetPickupConfirmation();

			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			runSheet.AddNewRunSheetInstructions(GetAddressPoints(picConfirmation));

			var depotConfirmation = AssertDepotConfirmation(picConfirmation.Instruction.Booking, ConfirmationTypes.Codes.Delivery);
			var depotInstruction = depotConfirmation.Instruction;
			AssertEquals(2, runSheet.RunSheetInstructions.Count);

			runSheet.RemoveRunSheetInstructions(false, runSheet.RunSheetInstructions.Single(i => i.K1_Sequence == 1));
			AssertEquals(0, runSheet.RunSheetInstructions.Count);
			AssertEquals(false, depotConfirmation.IsDeleted);
			AssertEquals(false, depotInstruction.IsDeleted);
		}

		#endregion

		#region TestRemoveRunSheetInstructions_Pickup_CFS

		public void TestRemoveRunSheetInstructions_Pickup_CFS()
		{
			var picConfirmation = GetPickupConfirmation();
			picConfirmation.Instruction.OrganisationType = "CFS";

			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			runSheet.AddNewRunSheetInstructions(GetAddressPoints(picConfirmation));

			var depotConfirmation = AssertDepotConfirmation(picConfirmation.Instruction.Booking, ConfirmationTypes.Codes.Delivery);
			var depotInstruction = depotConfirmation.Instruction;
			AssertEquals(2, runSheet.RunSheetInstructions.Count);

			runSheet.RemoveRunSheetInstructions(false, runSheet.RunSheetInstructions.Single(i => i.K1_Sequence == 1));
			AssertEquals(0, runSheet.RunSheetInstructions.Count);
			AssertEquals(false, depotConfirmation.IsDeleted);
			AssertEquals(false, depotInstruction.IsDeleted);
		}

		#endregion

		#region TestRemoveRunSheetInstructions_Delivery

		public void TestRemoveRunSheetInstructions_Delivery()
		{
			var dlvConfirmation = GetDeliveryConfirmation();

			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			runSheet.AddNewRunSheetInstructions(GetAddressPoints(dlvConfirmation));

			var depotConfirmation = AssertDepotConfirmation(dlvConfirmation.Instruction.Booking, ConfirmationTypes.Codes.PickUp);
			var depotInstruction = depotConfirmation.Instruction;
			AssertEquals(2, runSheet.RunSheetInstructions.Count);

			runSheet.RemoveRunSheetInstructions(false, runSheet.RunSheetInstructions.Single(i => i.K1_Sequence == 2));
			AssertEquals(0, runSheet.RunSheetInstructions.Count);
			AssertEquals(false, depotConfirmation.IsDeleted);
			AssertEquals(false, depotInstruction.IsDeleted);
		}

		#endregion

		#region TestRemoveRunSheetInstructions_PickupAndDelivery

		public void TestRemoveRunSheetInstructions_PickupAndDelivery()
		{
			var picConfirmation = GetPickupConfirmation();
			var dlvConfirmation = GetDeliveryConfirmation("BBB", picConfirmation.Instruction);

			var picRunSheet = Factory.New<DtbConsignmentRunSheet>();
			picRunSheet.AddNewRunSheetInstructions(GetAddressPoints(new[] { picConfirmation }));

			var dlvRunSheet = Factory.New<DtbConsignmentRunSheet>();
			dlvRunSheet.AddNewRunSheetInstructions(GetAddressPoints(new[] { dlvConfirmation }));

			var dlvDepotConfirmation = AssertDepotConfirmation(picConfirmation.Instruction.Booking, ConfirmationTypes.Codes.Delivery);
			var picDepotConfirmation = AssertDepotConfirmation(dlvConfirmation.Instruction.Booking, ConfirmationTypes.Codes.PickUp);
			AssertEquals(2, picRunSheet.RunSheetInstructions.Count);
			AssertEquals(2, dlvRunSheet.RunSheetInstructions.Count);
			AssertEquals(dlvDepotConfirmation.Instruction, picDepotConfirmation.Instruction);
			var depotInstruction = dlvDepotConfirmation.Instruction;

			dlvRunSheet.RemoveRunSheetInstructions(false, dlvRunSheet.RunSheetInstructions.Single(i => i.K1_Sequence == 2));
			AssertEquals(0, dlvRunSheet.RunSheetInstructions.Count);
			AssertEquals("Don't delete, as instruction is still being used", false, picDepotConfirmation.IsDeleted);
			AssertEquals("Instruction is still being used, but confirmation should be detached from the run sheet instruction", true, picDepotConfirmation.KK_K1_RunSheetInstruction.IsEmpty);
			AssertEquals("Don't delete, as instruction is still being used", false, depotInstruction.IsDeleted);

			picRunSheet.RemoveRunSheetInstructions(false, picRunSheet.RunSheetInstructions.Single(i => i.K1_Sequence == 1));
			AssertEquals(0, picRunSheet.RunSheetInstructions.Count);
			AssertEquals(false, dlvDepotConfirmation.IsDeleted);
			AssertEquals(false, picDepotConfirmation.IsDeleted);
			AssertEquals(false, depotInstruction.IsDeleted);
		}

		#endregion

		#region TestRemoveRunSheetInstructions_Pickup_SameConsignment

		public void TestRemoveRunSheetInstructions_Pickup_SameConsignment()
		{
			var picConfirmation = GetPickupConfirmation();
			var dlvConfirmation = GetDeliveryConfirmation("BBB", picConfirmation.Instruction);

			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			runSheet.AddNewRunSheetInstructions(GetAddressPoints(new[] { picConfirmation, dlvConfirmation }));

			runSheet.RemoveRunSheetInstructions(false, runSheet.RunSheetInstructions.Single(i => i.K1_Sequence == 1)); // remove pickup
			AssertEquals("Should remove the delivery as well, GUI to warn the user?", 0, runSheet.RunSheetInstructions.Count);
			AssertNull("Ensure picConfirmation has been removed", picConfirmation.RunSheetInstruction);
			AssertNull("Ensure dlvConfirmation has been removed", dlvConfirmation.RunSheetInstruction);
		}

		#endregion

		#region TestRemoveRunSheetInstructions_Delivery_SameConsignment

		public void TestRemoveRunSheetInstructions_Delivery_SameConsignment()
		{
			var picConfirmation = GetPickupConfirmation();
			var consignment = picConfirmation.Instruction.Booking;
			var dlvConfirmation = GetDeliveryConfirmation("BBB", picConfirmation.Instruction);

			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			runSheet.AddNewRunSheetInstructions(GetAddressPoints(new[] { picConfirmation, dlvConfirmation }));
			AssertRunSheetInstruction(runSheet, 1, new[] { picConfirmation });
			AssertRunSheetInstruction(runSheet, 2, new[] { dlvConfirmation });
			AssertEquals(2, runSheet.RunSheetInstructions.Count);

			runSheet.RemoveRunSheetInstructions(false, runSheet.RunSheetInstructions.Single(i => i.K1_Sequence == 2)); // remove delivery
			var depotConfirmation = AssertDepotConfirmation(picConfirmation.Instruction.Booking, ConfirmationTypes.Codes.Delivery);
			AssertRunSheetInstruction(runSheet, 1, new[] { picConfirmation });
			AssertRunSheetInstruction(runSheet, 2, new[] { depotConfirmation }); // delivery replaced with Depot
			AssertEquals(2, runSheet.RunSheetInstructions.Count);
			AssertNull("Ensure dlvConfirmation has been removed", dlvConfirmation.RunSheetInstruction);
		}

		#endregion

		#region TestRemoveRunSheetInstruction_OnCompletedInstructions

		public void TestRemoveRunSheetInstruction_OnCompletedInstructions()
		{
			var picConfirmation = GetPickupConfirmation();
			var dlvConfirmation = GetDeliveryConfirmation("AAA", picConfirmation.Instruction);

			var picRunSheet = Factory.New<DtbConsignmentRunSheet>();
			picRunSheet.AddNewRunSheetInstructions(GetAddressPoints(new[] { picConfirmation }));

			var dlvRunSheet = Factory.New<DtbConsignmentRunSheet>();
			dlvRunSheet.AddNewRunSheetInstructions(GetAddressPoints(new[] { dlvConfirmation }));

			var dlvInstruction = dlvRunSheet.RunSheetInstructions.Single(i => i.K1_Sequence == 2);
			SetRunsheetInstructionTimeInOutAndReceivedBy(dlvInstruction, ZDateTimeOffset.Now, ZDateTimeOffset.Now, "TST");

			var dlvDepotConfirmation = AssertDepotConfirmation(picConfirmation.Instruction.Booking, ConfirmationTypes.Codes.Delivery);
			var picDepotConfirmation = AssertDepotConfirmation(dlvConfirmation.Instruction.Booking, ConfirmationTypes.Codes.PickUp);
			AssertEquals("Precondition - 2 Instructions in Runsheet", 2, picRunSheet.RunSheetInstructions.Count);
			AssertEquals("Precondition - 2 Instructions in Runsheet", 2, dlvRunSheet.RunSheetInstructions.Count);
			AssertEquals("The Pick up's depot should be the same as the deliverie's depot", dlvDepotConfirmation.Instruction, picDepotConfirmation.Instruction);
			var depotInstruction = dlvDepotConfirmation.Instruction;

			// try to delete completed dlv rsi
			var isDlvRemoved = !dlvRunSheet.RemoveRunSheetInstructions(removePickupsForDirectDeliveries: false, instructions: dlvRunSheet.RunSheetInstructions.Single(i => i.K1_Sequence == 2));
			AssertEquals("Cannot remove as Delivery is flagged completed", false, isDlvRemoved);
			AssertEquals(2, dlvRunSheet.RunSheetInstructions.Count);

			// try to delete pickup with completed related
			var isPicRemoved = !picRunSheet.RemoveRunSheetInstructions(removePickupsForDirectDeliveries: false, instructions: picRunSheet.RunSheetInstructions.Single(i => i.K1_Sequence == 1));
			AssertEquals("Cannot remove as related Delivery is flagged Completed", false, isPicRemoved);
			AssertEquals(2, picRunSheet.RunSheetInstructions.Count);

			// remove dlv confirmation
			var picInstruction = picRunSheet.RunSheetInstructions.Single(i => i.K1_Sequence == 1);
			SetRunsheetInstructionTimeInOutAndReceivedBy(picInstruction, ZDateTimeOffset.Now, ZDateTimeOffset.Now, "TST");
			SetRunsheetInstructionTimeInOutAndReceivedBy(dlvInstruction, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, "");
			isDlvRemoved = !dlvRunSheet.RemoveRunSheetInstructions(removePickupsForDirectDeliveries: false, instructions: dlvRunSheet.RunSheetInstructions.Single(i => i.K1_Sequence == 2));
			AssertEquals("Runsheet Delivery instrctuions should be removed", true, isDlvRemoved);
			AssertEquals(0, dlvRunSheet.RunSheetInstructions.Count);

			// try to remove completed pickup
			isPicRemoved = !picRunSheet.RemoveRunSheetInstructions(removePickupsForDirectDeliveries: false, instructions: picRunSheet.RunSheetInstructions.Single(i => i.K1_Sequence == 1));
			AssertEquals("Cannot remove as pickup is flagged Completed", false, isPicRemoved);
			AssertEquals(2, picRunSheet.RunSheetInstructions.Count);

			// readd the delivery rsi confirmation for related removal test.
			dlvRunSheet.AddNewRunSheetInstructions(GetAddressPoints(new[] { dlvConfirmation }));
			AssertEquals("Should have added back confirmations for next test.", 2, dlvRunSheet.RunSheetInstructions.Count);

			// remove pickup and related dlv.
			SetRunsheetInstructionTimeInOutAndReceivedBy(picInstruction, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, "");
			var areBothRemoved = !picRunSheet.RemoveRunSheetInstructions(removePickupsForDirectDeliveries: true, instructions: picRunSheet.RunSheetInstructions.Single(i => i.K1_Sequence == 1));
			AssertEquals("Removal should have succeeded.", true, areBothRemoved);
			AssertEquals("Runsheet Pickup Instructions removed from Runsheet", 0, picRunSheet.RunSheetInstructions.Count);
			AssertEquals("Runsheet Delivery instrctuions should also be removed", 0, dlvRunSheet.RunSheetInstructions.Count);

			AssertEquals(false, dlvDepotConfirmation.IsDeleted);
			AssertEquals(false, picDepotConfirmation.IsDeleted);
			AssertEquals(false, depotInstruction.IsDeleted);
		}

		#endregion

		#region TestRemoveRunSheetInstructions_WithDirectDelivery

		public void TestRemoveRunSheetInstructions_WithDirectDelivery()
		{
			var consignment1 = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			var runSheet1 = Helper.CreateRunSheet();
			var pickupRouteInstruction1 = Helper.CreateRunSheetInstruction(runSheet1, consignment1.PickupInstruction.PickupConfirmation);
			var deliveryRouteInstruction1 = Helper.CreateRunSheetInstruction(runSheet1, consignment1.DeliveryInstruction.DeliveryConfirmation);
			AssertContainsExactElementsInAnyOrder(new[] { pickupRouteInstruction1, deliveryRouteInstruction1 }, runSheet1.RunSheetInstructions);

			runSheet1.RemoveRunSheetInstructions(false, deliveryRouteInstruction1);
			AssertEquals("Removing Delivery instructions for Direct should only remove the Delivery instruction by default.", 2, runSheet1.RunSheetInstructions.Count);
			AssertCollectionNotContains(deliveryRouteInstruction1, runSheet1.RunSheetInstructions);

			var consignment2 = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			var runSheet2 = Helper.CreateRunSheet();
			var pickupRouteInstruction2 = Helper.CreateRunSheetInstruction(runSheet2, consignment2.PickupInstruction.PickupConfirmation);
			var deliveryRouteInstruction2 = Helper.CreateRunSheetInstruction(runSheet2, consignment2.DeliveryInstruction.DeliveryConfirmation);
			AssertContainsExactElementsInAnyOrder(new[] { pickupRouteInstruction2, deliveryRouteInstruction2 }, runSheet2.RunSheetInstructions);

			runSheet2.RemoveRunSheetInstructions(true, deliveryRouteInstruction2);
			AssertEquals("As removing pickups for direct deliveries was specified, all instructions should be removed from the runsheet.", 0, runSheet2.RunSheetInstructions.Count);
		}

		#endregion

		#region TestRemoveRunSheetInstructions_WithError

		public void TestRemoveRunSheetInstructions_WithError()
		{
			var consignment = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			var runSheetInstuctionFromAnotherRunSheet = Helper.CreateRunSheetInstruction(Helper.CreateRunSheet(), consignment.PickupInstruction.PickupConfirmation);
			var runSheet = Helper.CreateRunSheet();
			AssertExceptionThrown<InvalidOperationException>("Should not be removing Depot RunSheet Instructions or RunSheet Instructions from another RunSheet this way.",
				() => runSheet.RemoveRunSheetInstructions(false, runSheetInstuctionFromAnotherRunSheet));

			Helper.CreateRunSheetInstruction(runSheet, consignment.DeliveryInstruction.DeliveryConfirmation);
			AssertExceptionThrown<InvalidOperationException>("Should not be removing Depot RunSheet Instructions or RunSheet Instructions from another RunSheet this way.",
				() => runSheet.RemoveRunSheetInstructions(false, new[] { runSheet.RunSheetInstructions.Single(rsi => rsi.IsOwnDepot) }));
		}

		#endregion

		#endregion

		#region TestRemoveConfirmationFromInstruction

		#region TestRemovePickupConfirmationFromInstruction

		public void TestRemovePickupConfirmationFromInstruction()
		{
			var picConfirmation = GetPickupConfirmation();

			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			runSheet.AddNewRunSheetInstructions(GetAddressPoints(picConfirmation));

			var depotConfirmation = AssertDepotConfirmation(picConfirmation.Instruction.Booking, ConfirmationTypes.Codes.Delivery);
			var depotInstruction = depotConfirmation.Instruction;
			var instruction = picConfirmation.Instruction;
			AssertEquals(2, runSheet.RunSheetInstructions.Count);

			runSheet.RemoveConfirmationFromInstruction(false, picConfirmation);
			AssertEquals(0, runSheet.RunSheetInstructions.Count);
			AssertEquals(false, depotConfirmation.IsDeleted);
			AssertEquals(false, depotInstruction.IsDeleted);
		}

		#endregion

		#region TestRemoveDeliveryConfirmationFromInstruction

		public void TestRemoveDeliveryConfirmationFromInstruction()
		{
			var dlvConfirmation = GetDeliveryConfirmation();

			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			runSheet.AddNewRunSheetInstructions(GetAddressPoints(dlvConfirmation));

			var depotConfirmation = AssertDepotConfirmation(dlvConfirmation.Instruction.Booking, ConfirmationTypes.Codes.PickUp);
			var depotInstruction = depotConfirmation.Instruction;
			var instruction = dlvConfirmation.Instruction;
			AssertEquals(2, runSheet.RunSheetInstructions.Count);

			runSheet.RemoveConfirmationFromInstruction(false, dlvConfirmation);
			AssertEquals(0, runSheet.RunSheetInstructions.Count);
			AssertEquals(false, depotConfirmation.IsDeleted);
			AssertEquals(false, depotInstruction.IsDeleted);
		}

		#endregion

		#region TestRemovePickupAndDeliveryConfirmationFromInstruction

		public void TestRemovePickupAndDeliveryConfirmationFromInstruction()
		{
			var picConfirmation = GetPickupConfirmation();
			var dlvConfirmation = GetDeliveryConfirmation("BBB", picConfirmation.Instruction);

			var picRunSheet = Factory.New<DtbConsignmentRunSheet>();
			picRunSheet.AddNewRunSheetInstructions(GetAddressPoints(picConfirmation));

			var dlvRunSheet = Factory.New<DtbConsignmentRunSheet>();
			dlvRunSheet.AddNewRunSheetInstructions(GetAddressPoints(dlvConfirmation));

			var dlvDepotConfirmation = AssertDepotConfirmation(picConfirmation.Instruction.Booking,
				ConfirmationTypes.Codes.Delivery);
			var picDepotConfirmation = AssertDepotConfirmation(dlvConfirmation.Instruction.Booking,
				ConfirmationTypes.Codes.PickUp);
			var picInstruction = picConfirmation.Instruction;
			var dlvInstruction = dlvConfirmation.Instruction;

			AssertEquals(2, picRunSheet.RunSheetInstructions.Count);
			AssertEquals(2, dlvRunSheet.RunSheetInstructions.Count);
			AssertEquals(dlvDepotConfirmation.Instruction, picDepotConfirmation.Instruction);
			var depotInstruction = dlvDepotConfirmation.Instruction;

			dlvRunSheet.RemoveConfirmationFromInstruction(false, dlvInstruction.DeliveryConfirmation);
			AssertEquals(0, dlvRunSheet.RunSheetInstructions.Count);
			AssertEquals("Don't delete, as instruction is still being used", false, picDepotConfirmation.IsDeleted);
			AssertEquals("Instruction is still being used, but confirmation should be detached from the run sheet instruction",
				true, picDepotConfirmation.KK_K1_RunSheetInstruction.IsEmpty);
			AssertEquals("Don't delete, as instruction is still being used", false, depotInstruction.IsDeleted);

			picRunSheet.RemoveConfirmationFromInstruction(false, picInstruction.PickupConfirmation);
			AssertEquals(0, picRunSheet.RunSheetInstructions.Count);
			AssertEquals(false, dlvDepotConfirmation.IsDeleted);
			AssertEquals(false, picDepotConfirmation.IsDeleted);
		}

		#endregion

		#region TestRemoveDeliveryConfirmationFromInstruction_SameConsignment

		public void TestRemoveDeliveryConfirmationFromInstruction_SameConsignment()
		{
			var picConfirmation = GetPickupConfirmation();
			var consignment = picConfirmation.Instruction.Booking;
			var dlvConfirmation = GetDeliveryConfirmation("BBB", picConfirmation.Instruction);

			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			runSheet.AddNewRunSheetInstructions(GetAddressPoints(new[] { picConfirmation, dlvConfirmation }));
			AssertRunSheetInstruction(runSheet, 1, new[] { picConfirmation });
			AssertRunSheetInstruction(runSheet, 2, new[] { dlvConfirmation });
			AssertEquals(2, runSheet.RunSheetInstructions.Count);

			runSheet.RemoveConfirmationFromInstruction(false, dlvConfirmation); // remove delivery
			var depotConfirmation = AssertDepotConfirmation(picConfirmation.Instruction.Booking, ConfirmationTypes.Codes.Delivery);
			AssertRunSheetInstruction(runSheet, 1, new[] { picConfirmation });
			AssertRunSheetInstruction(runSheet, 2, new[] { depotConfirmation }); // delivery replaced with Depot
			AssertEquals(2, runSheet.RunSheetInstructions.Count);
			AssertNull("Ensure dlvConfirmation has been removed", dlvConfirmation.RunSheetInstruction);
		}

		#endregion

		#region TestRemoveConfirmationFromInstruction_OnCompletedInstructions

		public void TestRemoveConfirmationFromInstruction_OnCompletedInstructions()
		{
			var picConfirmation = GetPickupConfirmation();
			var dlvConfirmation = GetDeliveryConfirmation("AAA", picConfirmation.Instruction);

			var picRunSheet = Factory.New<DtbConsignmentRunSheet>();
			picRunSheet.AddNewRunSheetInstructions(GetAddressPoints(picConfirmation));

			var dlvRunSheet = Factory.New<DtbConsignmentRunSheet>();
			dlvRunSheet.AddNewRunSheetInstructions(GetAddressPoints(dlvConfirmation));

			var dlvInstruction = dlvRunSheet.RunSheetInstructions.Single(i => i.K1_Sequence == 2);
			SetRunsheetInstructionTimeInOutAndReceivedBy(dlvInstruction, ZDateTimeOffset.Now, ZDateTimeOffset.Now, "TST");

			var dlvDepotConfirmation = AssertDepotConfirmation(picConfirmation.Instruction.Booking, ConfirmationTypes.Codes.Delivery);
			var picDepotConfirmation = AssertDepotConfirmation(dlvConfirmation.Instruction.Booking, ConfirmationTypes.Codes.PickUp);
			AssertEquals("Precondition - 2 Instructions in Runsheet", 2, picRunSheet.RunSheetInstructions.Count);
			AssertEquals("Precondition - 2 Instructions in Runsheet", 2, dlvRunSheet.RunSheetInstructions.Count);
			AssertEquals("The Pick up's depot should be the same as the deliverie's depot", dlvDepotConfirmation.Instruction, picDepotConfirmation.Instruction);
			var depotInstruction = dlvDepotConfirmation.Instruction;

			dlvRunSheet.RemoveConfirmationFromInstruction(false, dlvConfirmation);
			AssertEquals("Cannot remove as Delivery is flagged completed", 2, dlvRunSheet.RunSheetInstructions.Count);

			picRunSheet.RemoveConfirmationFromInstruction(false, picConfirmation);
			AssertEquals("Cannot remove as related Delivery is flagged Completed", 2, picRunSheet.RunSheetInstructions.Count);

			SetRunsheetInstructionTimeInOutAndReceivedBy(dlvInstruction, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, "");
			picRunSheet.RemoveConfirmationFromInstruction(false, picConfirmation);
			AssertEquals("Runsheet Pickup Instructions removed from Runsheet", 0, picRunSheet.RunSheetInstructions.Count);
			AssertEquals("Runsheet Delivery instrctuions should also be removed", 0, dlvRunSheet.RunSheetInstructions.Count);

			AssertEquals(false, dlvDepotConfirmation.IsDeleted);
			AssertEquals(false, picDepotConfirmation.IsDeleted);
			AssertEquals(false, depotInstruction.IsDeleted);
		}

		#endregion

		#region TestRemoveConfirmationFromInstruction_WithDirectDelivery

		public void TestRemoveConfirmationFromInstruction_WithDirectDelivery()
		{
			var consignment1 = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			var runSheet1 = Helper.CreateRunSheet();
			var pickupRouteInstruction1 = Helper.CreateRunSheetInstruction(runSheet1, consignment1.PickupInstruction.PickupConfirmation);
			var deliveryRouteInstruction1 = Helper.CreateRunSheetInstruction(runSheet1, consignment1.DeliveryInstruction.DeliveryConfirmation);
			AssertContainsExactElementsInAnyOrder(new[] { pickupRouteInstruction1, deliveryRouteInstruction1 }, runSheet1.RunSheetInstructions);

			runSheet1.RemoveConfirmationFromInstruction(false, consignment1.DeliveryInstruction.DeliveryConfirmation);
			AssertEquals("Removing Delivery Confirmations for Direct should only remove the Delivery Confirmation by default.", 2, runSheet1.RunSheetInstructions.Count);
			AssertCollectionNotContains(consignment1.DeliveryInstruction.DeliveryConfirmation, runSheet1.RunSheetInstructions.SelectMany(r => r.Confirmations));

			var consignment2 = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			var runSheet2 = Helper.CreateRunSheet();
			var pickupRouteInstruction2 = Helper.CreateRunSheetInstruction(runSheet2, consignment2.PickupInstruction.PickupConfirmation);
			var deliveryRouteInstruction2 = Helper.CreateRunSheetInstruction(runSheet2, consignment2.DeliveryInstruction.DeliveryConfirmation);
			AssertContainsExactElementsInAnyOrder(new[] { pickupRouteInstruction2, deliveryRouteInstruction2 }, runSheet2.RunSheetInstructions);

			runSheet2.RemoveConfirmationFromInstruction(true, consignment2.DeliveryInstruction.DeliveryConfirmation);
			AssertEquals("As removing pickups for direct deliveries was specified, all confirmations should be removed from the runsheet.", 0, runSheet2.RunSheetInstructions.Count);
		}

		#endregion

		#region TestRemoveConfirmationFromInstruction_WithError

		public void TestRemoveConfirmationFromInstruction_WithError()
		{
			var consignment = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			var runSheet = Helper.CreateRunSheet();
			var runSheetInstructionFromAnotherRunSheet = Helper.CreateRunSheetInstruction(Helper.CreateRunSheet(), consignment.PickupInstruction.PickupConfirmation);
			AssertExceptionThrown<InvalidOperationException>("Should not be removing Confirmations from another RunSheet this way.",
				() => runSheet.RemoveConfirmationFromInstruction(false, consignment.PickupInstruction.PickupConfirmation));
		}

		#endregion

		#endregion

		#region Business Objects with Related Events

		public void TestBusinessObjectsWithRelatedEventsCore_ShouldFindVariations()
		{
			var consignment = ConsignmentTestHelper.CreateConsignment();
			var pickupAddress = ConsignmentTestHelper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var deliveryAddress = ConsignmentTestHelper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery);
			var pickupAction = ConsignmentTestHelper.CreateConsignmentAction(pickupAddress, ActionTypes.Codes.PickUp);
			var deliveryAction = ConsignmentTestHelper.CreateConsignmentAction(deliveryAddress, ActionTypes.Codes.Delivery);

			var package = consignment.PackageJob.Packages.AddNew();
			package.KP_PackageQty = 1;
			var divot = Factory.New<DtbConsignmentActionPackageDivot>();
			divot.LTP_LTA_ConsignmentAction = pickupAction.PK;
			divot.LTP_KP_Package = package.PK;
			divot.LTP_PackageQuantity = 1;

			var runSheet = ConsignmentTestHelper.CreateRunSheet();
			ConsignmentTestHelper.CreateRunSheetInstruction(pickupAction, runSheet.PK);
			ConsignmentTestHelper.CreateRunSheetInstruction(deliveryAction, runSheet.PK);

			var variation = ConsignmentTestHelper.CreateVariation(consignment, divot);
			Factory.Save();

			var relatedObjects = runSheet.BusinessObjectsWithRelatedEvents;
			AssertContainsExactElementsInAnyOrder(new BusinessObject[] { variation }, relatedObjects);
		}

		#endregion

		#region TestHumanReadableName

		public void TestHumanReadableName()
		{
			var runSheet = Helper.CreateRunSheet(ZDateTimeOffset.Now);
			AssertEquals("Run Sheet" + runSheet.KG_RunSheetNumber, runSheet.HumanReadableName.Trim());

			Factory.Save();
			AssertEquals("Precondition", false, runSheet.KG_RunSheetNumber.IsEmpty);
			AssertEquals(string.Format("Run Sheet {0}", runSheet.KG_RunSheetNumber), runSheet.HumanReadableName);
		}

		#endregion

		#region TestSave

		#region TestSave

		public void TestSave()
		{
			var runSheetNoID1 = Helper.CreateRunSheet();
			var runSheetWithID = Helper.CreateRunSheet();
			runSheetWithID.KG_RunSheetNumber = "ABC";
			runSheetNoID1.KG_RunSheetNumber = "";
			AssertEquals("Precondition", "", runSheetNoID1.KG_RunSheetNumber);

			Factory.Save();
			AssertEquals("CR00000001", runSheetNoID1.KG_RunSheetNumber);
			AssertEquals("Should not overwrite a non-empty ID.", "ABC", runSheetWithID.KG_RunSheetNumber);

			runSheetWithID.KG_RunSheetNumber = "";
			Factory.Save();
			AssertEquals("Should not overwrite ID if already in DB.", "", runSheetWithID.KG_RunSheetNumber);

			var deletedRunSheet = Helper.CreateRunSheet();
			deletedRunSheet.Delete();
			Factory.Save();

			var runSheetNoID2 = Helper.CreateRunSheet();
			runSheetNoID2.KG_RunSheetNumber = "";
			Factory.Save();
			AssertEquals("Deleted RunSheet should not have used up a fountain number.", "CR00000002", runSheetNoID2.KG_RunSheetNumber);
		}

		#endregion

		#endregion

		#region TestSpecialInstructionsExist

		public void TestSpecialInstructionsExist()
		{
			var consignment1 = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			var consignment2 = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			var runsheet = Helper.CreateRunSheet();
			runsheet.AddNewRunSheetInstructions(new[] { consignment1.PickupInstruction.Confirmations.First(c => c.IsPickUp) });
			runsheet.AddNewRunSheetInstructions(new[] { consignment2.PickupInstruction.Confirmations.First(c => c.IsPickUp) });
			AssertEquals(false, runsheet.SpecialInstructionsExist);

			consignment1.PickupInstruction.KN_ServiceInstruction = "C1 Instructions";
			consignment2.PickupInstruction.KN_ServiceInstruction = "";
			AssertEquals(true, runsheet.SpecialInstructionsExist);

			consignment1.PickupInstruction.KN_ServiceInstruction = "";
			consignment2.PickupInstruction.KN_ServiceInstruction = "C2 Instructions";
			AssertEquals(true, runsheet.SpecialInstructionsExist);

			consignment1.PickupInstruction.KN_ServiceInstruction = "";
			consignment2.PickupInstruction.KN_ServiceInstruction = "";
			AssertEquals(false, runsheet.SpecialInstructionsExist);

			consignment1.PickupInstruction.KN_ServiceInstruction = "C1 Instructions";
			consignment2.PickupInstruction.KN_ServiceInstruction = "C2 Instructions";
			AssertEquals(true, runsheet.SpecialInstructionsExist);
		}

		#endregion

		#region TestDelete

		public void TestDelete()
		{
			var runSheet = Helper.CreateRunSheet();
			var instruction1 = runSheet.RunSheetInstructions.AddNew();
			var instruction2 = runSheet.RunSheetInstructions.AddNew();
			var instruction3 = runSheet.RunSheetInstructions.AddNew();
			var instruction4 = runSheet.RunSheetInstructions.AddNew();
			AssertNoExceptionThrown(() => runSheet.Delete());
			AssertEquals(true, instruction1.IsDeleted);
			AssertEquals(true, instruction2.IsDeleted);
			AssertEquals(true, instruction3.IsDeleted);
			AssertEquals(true, instruction4.IsDeleted);
		}

		#endregion

		#region TestLookups

		protected override Type ExpectedLookupsType
		{
			get { return typeof(DtbConsignmentRunSheetLookups); }
		}

		protected override bool IsLookupsOverridden
		{
			get { return false; }
		}

		#endregion

		#region TestValidation

		protected override Type ExpectedValidationType
		{
			get { return typeof(DtbConsignmentRunSheetValidation); }
		}

		protected override bool IsValidationOverridden
		{
			get { return false; }
		}

		#endregion

		#region IAllowUserToDelete Members

		public void TestAllowUserToDelete()
		{
			var runSheet = Helper.CreateRunSheet();
			AssertEquals(true, runSheet.CanDelete);

			runSheet.RunSheetInstructions.AddNew();
			AssertEquals(false, runSheet.CanDelete);
			AssertEquals("A Run-sheet containing Instructions cannot be deleted. Remove all Instructions before deleting.", runSheet.ReasonForNotAbleToDelete);
		}

		#endregion

		#region IDocumentSupportable Members

		public void TestDocumentSupporter()
		{
			IDocumentSupportable runsheet = Helper.CreateRunSheet();
			AssertEquals(typeof(DtbConsignmentRunSheetDocumentSupporter), runsheet.DocumentSupporter.GetType());
			AssertEquals(runsheet.DocumentSupporter, runsheet.DocumentSupporter);
		}

		#endregion

		#region IEDocsProvider Members

		public void TestIEDocsProvider_GetEDocsProviderSupporter()
		{
			IEDocsProvider runsheet = Helper.CreateRunSheet();
			AssertNotNull(runsheet.GetEDocsProviderSupporter());
		}

		#endregion

		#region IDocManagerSupport Members

		public void TestIDocManagerSupport_DocManagerInfo()
		{
			IDocManagerSupport runsheet = Helper.CreateRunSheet();
			AssertNotNull(runsheet.DocManagerInfo);
			AssertEquals(typeof(DtbConsignmentRunSheetDocManagerInfo), runsheet.DocManagerInfo.GetType());
			AssertEquals(Constants.DocManagerCodes.DomesticTransportRunSheet, runsheet.DocManagerInfo.DocManagerCode);
		}

		#endregion

		#region IJobCostingPlugIn Members

		public void TestIJobCostingPlugIn_Properties()
		{
			IJobCostingPlugIn costingPlugIn = Helper.CreateRunSheet("RS0001000");

			AssertEquals("ConsolExchangeRate", 0m, costingPlugIn.ConsolExchangeRate);
			AssertEquals("IsMasterCollect", false, costingPlugIn.IsMasterCollect);
			AssertEquals("JK_UniqueConsignRef", "RS0001000", costingPlugIn.JK_UniqueConsignRef);
			AssertEquals("TransportMode", "", costingPlugIn.TransportMode);
			AssertEquals("ContainerMode", ZString.Empty, costingPlugIn.ContainerMode);
			AssertEquals("ConsolType", ZString.Empty, costingPlugIn.ConsolType);
			AssertEquals("Module", ApportionmentMethodModules.TransportBooking, costingPlugIn.Module);
			AssertEquals("Direction", ZString.Empty, costingPlugIn.Direction);
			AssertEquals("ExchangeRateForCurrency", 0m, costingPlugIn.ExchangeRateForCurrency(null, ZGuid.Empty));
			AssertEquals("CostSupporter", typeof(DtbConsignmentRunSheetCostSupporter), costingPlugIn.CostSupporter.GetType());
			AssertEquals(((BusinessObject)costingPlugIn).PK, costingPlugIn.CostSupporter.PK);

			AssertNull("ConsolCurrency", costingPlugIn.ConsolCurrency);
			AssertNull("DischargePort", costingPlugIn.DischargePort);
			AssertNull("LoadPort", costingPlugIn.LoadPort);
			AssertNull("ProfitLossContainer", costingPlugIn.ProfitLossContainer);
			AssertNull("ReceivingAgent", costingPlugIn.ReceivingAgent);
			AssertNull("ReceivingAgentAPInvoicingParty", costingPlugIn.ReceivingAgentAPInvoicingParty);
			AssertNull("ReceivingAgentARInvoicingParty", costingPlugIn.ReceivingAgentARInvoicingParty);
			AssertNull("SendingAgent", costingPlugIn.SendingAgent);
			AssertNull("SendingAgentAPInvoicingParty", costingPlugIn.SendingAgentAPInvoicingParty);
			AssertNull("SendingAgentARInvoicingParty", costingPlugIn.SendingAgentARInvoicingParty);
		}

		#region TestPrepaidCollectList

		public void TestPrepaidCollectList()
		{
			IJobCostingPlugIn costingPlugIn = Helper.CreateRunSheet("RS0001000");
			AssertContainsExactElementsInAnyOrder(new[] { InstructionTypes.Codes.PickUp, InstructionTypes.Codes.Delivery, PrepaidCollectList.Codes.All, PrepaidCollectList.Codes.CTS },
				costingPlugIn.PrepaidCollectList.ToArray().Select(x => x.Code));
		}

		#endregion

		#endregion

		#region IRatingSupporter Members

		public void TestIRatingSupporter_AdaptersProvider()
		{
			IRatingSupporter ratingSupporter = Helper.CreateRunSheet();

			AssertEquals("AdaptersProvider", typeof(DtbConsignmentRunSheetConsolRatingAdaptersProvider), ratingSupporter.AdaptersProvider.GetType());
		}

		#endregion

		#region TestIJobNumberMembers

		public void TestIJobNumberMembers()
		{
			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			runSheet.KG_RunSheetNumber = "TestNumber";
			AssertEquals("TestNumber", ((IJobNumber)runSheet).JobNumber);
		}

		#endregion

		#region TestIHaveServices

		public void TestIHaveServices()
		{
			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			var runSheetServices = (IHaveServices)runSheet;

			AssertEquals("", runSheetServices.TransportMode);
			AssertEquals("", runSheetServices.ContainerMode);
			AssertEquals(false, runSheetServices.DependentServiceParents.Any());
			AssertEquals(runSheet, runSheetServices.ServiceParent);
			AssertEquals(false, runSheetServices.NeedsServiceEvents);
			AssertEquals(runSheet.TablePrefix, runSheetServices.TableCode);
		}

		public void TestServiceBranch()
		{
			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			var iHaveServices = (IHaveServices)runSheet;
			AssertEquals("Service branch is defaulted", Env.CurrentBranch.PK, iHaveServices.ServiceBranch.PK);

			var company = Factory.New<GlbCompany>();
			var branch = company.Branches.AddNew();
			runSheet.KG_GB_Branch = branch.PK;
			AssertEquals("Service branch", branch.PK, iHaveServices.ServiceBranch.PK);
		}

		#endregion

		#region IProcessHandlingInfoProvider Memebers

		public void TestIProcessHandlingInfoProviderMembers()
		{
			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			AssertType<DtbConsignmentRunSheetProcessHandlingInfo>("TestNumber", ((IProcessHandlingInfoProvider)runSheet).ProcessHandlingInfo);
		}

		#endregion

		#region CarrierServiceLevel

		public void TestCarrierServiceLevelNoExceptionFromNonUniqueCode()
		{
			var sheet = Factory.New<DtbConsignmentRunSheet>();
			sheet.KG_PL_NKCarrierServiceLevel = "ABC";

			var serviceLevel = Factory.New<OrgCarrierServiceLevel>();
			serviceLevel.PL_Code = "ABC";

			var serviceLevel2 = Factory.New<OrgCarrierServiceLevel>();
			serviceLevel2.PL_Code = "ABC";

			AssertNoExceptionThrown("Accessing CarrierServiceLevel should not throw exception", () => { _ = sheet.CarrierServiceLevel; });
		}

		#endregion

		#region Implementation

		new TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}

		TransportBookingConsignmentTestHelper helper;

		TransportConsignmentTestHelper ConsignmentTestHelper => consignmentTestHelper ?? (consignmentTestHelper = new TransportConsignmentTestHelper(Factory));
		TransportConsignmentTestHelper consignmentTestHelper;

		#endregion
	}
}

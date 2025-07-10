using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.TransportBookings.Business.Testing
{
	public class DtbBookingValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateKM_PL_NKCarrierServiceLevel()
		{
			var booking = Helper.CreateBooking();
			booking.KM_PL_NKCarrierServiceLevel = "";
			AssertNoNotifications(booking.KM_PL_NKCarrierServiceLevelInfo);

			booking.KM_PL_NKCarrierServiceLevel = "ABC";
			AssertHasError(booking.KM_PL_NKCarrierServiceLevelInfo, "Enter a valid Carrier Service Level.");

			var transportCo = Helper.CreateOrganisation("XYZ");
			var serviceLevel = transportCo.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel.PL_Code = "ABC";
			booking.Address.OrganisationPK = transportCo.PK;
			booking.KM_PL_NKCarrierServiceLevel = "ABC";
			AssertNoError(booking.KM_PL_NKCarrierServiceLevelInfo, "Enter a valid Carrier Service Level.");

			booking.KM_PL_NKCarrierServiceLevel = "STD";
			AssertNoErrors("STD is available for all TransportCo's.", booking.KM_PL_NKCarrierServiceLevelInfo);

			booking.KM_PL_NKCarrierServiceLevel = "XXX";
			AssertHasError(booking.KM_PL_NKCarrierServiceLevelInfo, "Enter a valid Carrier Service Level.");

			booking.KM_PL_NKCarrierServiceLevel = "";
			AssertNoNotifications(booking.KM_PL_NKCarrierServiceLevelInfo);
		}

		public void TestKM_ChargeableValidation_WithoutOverride()
		{
			TestKM_ChargeableValidationCore(overrideChargeable: false);
		}

		public void TestKM_ChargeableValidation_WithOverride()
		{
			TestKM_ChargeableValidationCore(overrideChargeable: true);
		}

		void TestKM_ChargeableValidationCore(bool overrideChargeable = false)
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			var packingJob = Helper.CreatePackageJob(consolidation);
			var package = PackingHelper.CreatePackage(packingJob, "P1", 1, "PKG", "");
			var instruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CFS, null);
			var divot = Helper.CreatePackageDivot(instruction, package).KD_Quantity = 1;
			package.KP_Volume = 999.99m;
			package.KP_Weight = 999.99m;

			booking.KM_OverrideChargeable = overrideChargeable;
			booking.Validation.ValidateKM_Chargeable();
			AssertEquals("Precondition: Validation errors not expected for KM_Chargeable.", false, booking.KM_ChargeableInfo.HasErrors());

			package.KP_Weight = 999999999.999m;
			booking.Validation.ValidateKM_Chargeable();
			AssertEquals($"No validation errors expected for KM_Chargeable when overriden.", false, booking.KM_ChargeableInfo.HasErrors());
		}

		public void TestValidateTransportCoAgainstMultiJobConsolidation()
		{
			var transportCoABC = Helper.CreateOrganisation("ABC");
			var transportCoXYZ = Helper.CreateOrganisation("XYZ");

			// multi-job consolidation
			var consolidation = Helper.CreateConsolidationMultiJob();
			var booking = consolidation.Bookings.AddNew();

			booking.Address.OrganisationPK = transportCoABC.PK;
			booking.Validation.ValidateTransportCoAgainstMultiJobConsolidation();
			AssertEquals("Consolidation has no TransportCo, Booking should have no error.", 0, booking.RowErrors.Count());

			consolidation.Address.OrganisationPK = transportCoXYZ.PK;
			booking.Validation.ValidateTransportCoAgainstMultiJobConsolidation();
			booking.RowErrors.Single(e => e.Message == "This Booking does not have the same Transport Company as the Consolidation.");

			booking.Address.OrganisationPK = transportCoXYZ.PK;
			booking.Validation.ValidateTransportCoAgainstMultiJobConsolidation();
			AssertEquals("Consolidation and Booking have the same TransportCo, Booking should have no error.", 0, booking.RowErrors.Count());

			// single-job consolidation

			booking.Address.OrganisationPK = transportCoABC.PK;
			booking.Validation.ValidateTransportCoAgainstMultiJobConsolidation();
			AssertEquals("Precondition - Booking should have an error.", 1, booking.RowErrors.Count());

			consolidation.Bookings.RemoveFromRelationship(booking);
			booking.Validation.ValidateTransportCoAgainstMultiJobConsolidation();
			AssertEquals("Not on a Multi-Job Consolidation, Booking should have no error.", 0, booking.RowErrors.Count());
		}

		public void TestValidateParentJobConAddedToConsolidation()
		{
			var consol = (BusinessObject)Factory.New<IForwardingConsol>();
			consol[JobConsolSchema.JK_RL_NKLoadPort] = "AUBNE";
			consol[JobConsolSchema.JK_RL_NKDischargePort] = "USLAX";
			consol[JobConsolSchema.JK_IsCancelled] = false;
			consol[JobConsolSchema.JK_IsForwarding] = true;

			var consolidation = Helper.CreateConsolidation((IDtbBookingParent)consol);
			var booking = Helper.CreateBooking(consolidation);

			var transportCoABC = Helper.CreateOrganisation("ABC");
			var consolidationMultiJob = Helper.CreateConsolidationMultiJob(transportCoABC);
			booking.Address.OrganisationPK = transportCoABC.PK;
			consolidationMultiJob.Bookings.Add(booking);

			booking.Validation.ValidateAll();
			AssertNoRowErrors(booking);
		}

		public void TestValidateAll()
		{
			// ensure ValidateTransportCoAgainstMultiJobConsolidation is called

			var transportCoABC = Helper.CreateOrganisation("ABC");
			var transportCoXYZ = Helper.CreateOrganisation("XYZ");
			var consolidation = Helper.CreateConsolidationMultiJob(transportCoABC);
			var booking = Helper.CreateBooking();
			consolidation.Bookings.Add(booking);

			booking.Address.OrganisationPK = transportCoXYZ.PK; // transportCo does not match Consolidation
			AssertEquals("Precondition", 0, booking.RowErrors.Count());

			booking.Validation.ValidateAll();
			booking.RowErrors.Single(e => e.Message == "This Booking does not have the same Transport Company as the Consolidation.");
		}

		public void TestValidateAll_ValidationForSendingXUSToCTOIsNotRunIfIsSendingXUSToCTOIsFalse()
		{
			var consol = Factory.New<IForwardingConsol>() as IDtbBookingParent;
			var booking = Helper.CreateBookingThatWillPassValidationForSendingXUSToCTO(consol);

			booking.NotificationBufferForSendingXUSToCTO.AddError("Error");
			booking.NotificationBufferForSendingXUSToCTO.AddMessageError("Message Error");

			Factory.Save();

			var notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			AssertEquals($"Precondition: {nameof(DtbBooking.IsSendingXUSToCTO)} should be false.", false, booking.IsSendingXUSToCTO);
			AssertEquals($"Precondition: {nameof(DtbBooking.NotificationBufferForSendingXUSToCTO)} should have 2 errors.", 2, notificationBufferErrorAndMessageErrorMessages.Count());

			booking.Validation.ValidateAll();
			notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			AssertEquals("Although validation for sending XUS to CTO would have failed due to an incorrect parent type, there should not be any errors in the notification buffer since the validation should not have run.", 0, notificationBufferErrorAndMessageErrorMessages.Count());
		}

		public void TestCheckKM_KT_NKBookingTemplate()
		{
			var booking = GetNewBooking();
			booking.Validation.ValidateKM_KT_NKBookingTemplate();
			AssertNoErrors("Booking Template is not a mandatory field.", booking.KM_KT_NKBookingTemplateInfo);

			booking.KM_KT_NKBookingTemplate = "NVLD";
			AssertHasError(booking.KM_KT_NKBookingTemplateInfo, "Enter a valid Template.");

			booking.KM_KT_NKBookingTemplate = "IFCX"; // Import FCL, CNR to CNE
			AssertNoErrors(booking.KM_KT_NKBookingTemplateInfo);
		}

		public void TestCheckKM_IsHazardous()
		{
			var booking = GetNewBooking();
			var packageJob = GetPackageJob(booking);
			var package = packageJob.Packages.AddNew();

			var expectedNoDGUsedErrorMessage = "No packages with Dangerous Goods have been assigned to Instructions.";
			var expectedDGUsedErrorMessage = "There are packages with Dangerous Goods assigned to Instructions. Is Hazardous needs to be checked.";

			// No packages attached
			AssertNoError(booking.KM_IsHazardousInfo, expectedNoDGUsedErrorMessage);
			AssertNoError(booking.KM_IsHazardousInfo, expectedDGUsedErrorMessage);

			booking.KM_IsHazardous = true;
			AssertHasError(booking.KM_IsHazardousInfo, expectedNoDGUsedErrorMessage);
			AssertNoError(booking.KM_IsHazardousInfo, expectedDGUsedErrorMessage);

			package.UNDGs.AddNew();
			booking.KM_IsHazardous = false;
			booking.KM_IsHazardous = true;
			AssertHasError(booking.KM_IsHazardousInfo, expectedNoDGUsedErrorMessage);
			AssertNoError(booking.KM_IsHazardousInfo, expectedDGUsedErrorMessage);

			// With package with Dangerous Goods.
			booking.Instructions.AddNew().DivotsWithPackages.AddPackage(package);
			booking.KM_IsHazardous = false;
			booking.KM_IsHazardous = true;
			AssertNoError(booking.KM_IsHazardousInfo, expectedNoDGUsedErrorMessage);
			AssertNoError(booking.KM_IsHazardousInfo, expectedDGUsedErrorMessage);

			booking.KM_IsHazardous = false;
			AssertNoError(booking.KM_IsHazardousInfo, expectedNoDGUsedErrorMessage);
			AssertHasError(booking.KM_IsHazardousInfo, expectedDGUsedErrorMessage);
		}

		public void TestCheckKM_IsHazardous_Recursively()
		{
			var booking = GetNewBooking();
			var packageJob = GetPackageJob(booking);
			var packageTopLevel = packageJob.Packages.AddNew();
			var packageChild = packageTopLevel.Packages.AddNew();

			var expectedNoDGUsedErrorMessage = "No packages with Dangerous Goods have been assigned to Instructions.";
			var expectedDGUsedErrorMessage = "There are packages with Dangerous Goods assigned to Instructions. Is Hazardous needs to be checked.";

			// No packages attached
			AssertNoError(booking.KM_IsHazardousInfo, expectedNoDGUsedErrorMessage);
			AssertNoError(booking.KM_IsHazardousInfo, expectedDGUsedErrorMessage);

			booking.KM_IsHazardous = true;
			AssertHasError(booking.KM_IsHazardousInfo, expectedNoDGUsedErrorMessage);
			AssertNoError(booking.KM_IsHazardousInfo, expectedDGUsedErrorMessage);

			packageChild.UNDGs.AddNew();
			booking.KM_IsHazardous = false;
			booking.KM_IsHazardous = true;
			AssertHasError(booking.KM_IsHazardousInfo, expectedNoDGUsedErrorMessage);
			AssertNoError(booking.KM_IsHazardousInfo, expectedDGUsedErrorMessage);

			// With package with Dangerous Goods.
			booking.Instructions.AddNew().DivotsWithPackages.AddPackage(packageTopLevel);
			booking.KM_IsHazardous = false;
			booking.KM_IsHazardous = true;
			AssertNoError(booking.KM_IsHazardousInfo, expectedNoDGUsedErrorMessage);
			AssertNoError(booking.KM_IsHazardousInfo, expectedDGUsedErrorMessage);

			booking.KM_IsHazardous = false;
			AssertNoError(booking.KM_IsHazardousInfo, expectedNoDGUsedErrorMessage);
			AssertHasError(booking.KM_IsHazardousInfo, expectedDGUsedErrorMessage);
		}

		public void TestCheckKM_IsHazardous_SkippedForSubBooking()
		{
			const string NoExpectedErrorsAssertionMessage = "Sub bookings should not trigger validation error on KM_IsHazardous";
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			Factory.Save();

			var subBooking = Helper.CreateBooking();
			var package = subBooking.PackageJob.Packages.AddNew();
			Factory.Save();

			masterBooking.SubBookings.Add(subBooking);
			Factory.Save();

			AssertNoErrors(NoExpectedErrorsAssertionMessage, subBooking.KM_IsHazardousInfo);

			var masterInstruction = masterBooking.Instructions.AddNew();

			masterInstruction.DivotsWithPackages.AddPackage(package);

			AssertEquals("Precondition: subBooking.KM_IsHazardous is false", false, subBooking.KM_IsHazardous);
			AssertNoErrors(NoExpectedErrorsAssertionMessage, subBooking.KM_IsHazardousInfo);

			subBooking.KM_IsHazardous = true;
			AssertNoErrors(NoExpectedErrorsAssertionMessage, subBooking.KM_IsHazardousInfo);

			package.UNDGs.AddNew();
			AssertNoErrors(NoExpectedErrorsAssertionMessage, subBooking.KM_IsHazardousInfo);

			subBooking.KM_IsHazardous = false;
			AssertNoErrors(NoExpectedErrorsAssertionMessage, subBooking.KM_IsHazardousInfo);
		}

		public void TestCheckKM_RequiresRefrigeration()
		{
			var booking = GetNewBooking();
			var packageJob = GetPackageJob(booking);
			var package = packageJob.Packages.AddNew();

			var expectedRequireNoRefrigerationErrorMessage = "No packages that Require Refrigeration have been assigned to Instructions.";
			var expectedRequireRefrigerationErrorMessage = "There are packages that Require Refrigeration assigned to Instructions. Require Refrigeration need to be checked.";

			// No packages attached
			AssertNoError(booking.KM_RequiresRefrigerationInfo, expectedRequireNoRefrigerationErrorMessage);
			AssertNoError(booking.KM_RequiresRefrigerationInfo, expectedRequireRefrigerationErrorMessage);

			booking.KM_RequiresRefrigeration = true;
			AssertHasError(booking.KM_RequiresRefrigerationInfo, expectedRequireNoRefrigerationErrorMessage);
			AssertNoError(booking.KM_RequiresRefrigerationInfo, expectedRequireRefrigerationErrorMessage);

			package.KP_RequiresTemperatureControl = true;
			booking.KM_RequiresRefrigeration = false;
			booking.KM_RequiresRefrigeration = true;
			AssertHasError(booking.KM_RequiresRefrigerationInfo, expectedRequireNoRefrigerationErrorMessage);
			AssertNoError(booking.KM_RequiresRefrigerationInfo, expectedRequireRefrigerationErrorMessage);

			// With package with temperature control required
			booking.Instructions.AddNew().DivotsWithPackages.AddPackage(package);
			booking.KM_RequiresRefrigeration = false;
			booking.KM_RequiresRefrigeration = true;
			AssertNoError(booking.KM_RequiresRefrigerationInfo, expectedRequireNoRefrigerationErrorMessage);
			AssertNoError(booking.KM_RequiresRefrigerationInfo, expectedRequireRefrigerationErrorMessage);

			booking.KM_RequiresRefrigeration = false;
			AssertNoError(booking.KM_RequiresRefrigerationInfo, expectedRequireNoRefrigerationErrorMessage);
			AssertHasError(booking.KM_RequiresRefrigerationInfo, expectedRequireRefrigerationErrorMessage);

			// With container with no temperature control required
			package.KP_RequiresTemperatureControl = false;
			package.KP_F3_NKPackType = Constants.PkgUnit.Container;
			booking.KM_RequiresRefrigeration = true;
			AssertHasError(booking.KM_RequiresRefrigerationInfo, expectedRequireNoRefrigerationErrorMessage);
			AssertNoError(booking.KM_RequiresRefrigerationInfo, expectedRequireRefrigerationErrorMessage);

			// With container with temperature control required
			package.Container.K0_IsControlledAtmosphere = true;
			booking.KM_RequiresRefrigeration = false;
			booking.KM_RequiresRefrigeration = true;
			AssertNoError(booking.KM_RequiresRefrigerationInfo, expectedRequireNoRefrigerationErrorMessage);
			AssertNoError(booking.KM_RequiresRefrigerationInfo, expectedRequireRefrigerationErrorMessage);

			booking.KM_RequiresRefrigeration = false;
			AssertNoError(booking.KM_RequiresRefrigerationInfo, expectedRequireNoRefrigerationErrorMessage);
			AssertHasError(booking.KM_RequiresRefrigerationInfo, expectedRequireRefrigerationErrorMessage);
		}

		public void TestCheckKM_RequiresRefrigeration_Recursively()
		{
			var booking = GetNewBooking();
			var packageJob = GetPackageJob(booking);
			var packageTopLevel = packageJob.Packages.AddNew();
			var packageChild = packageTopLevel.Packages.AddNew();

			var expectedRequireNoRefrigerationErrorMessage = "No packages that Require Refrigeration have been assigned to Instructions.";
			var expectedRequireRefrigerationErrorMessage = "There are packages that Require Refrigeration assigned to Instructions. Require Refrigeration need to be checked.";

			// No packages attached
			AssertNoError(booking.KM_RequiresRefrigerationInfo, expectedRequireNoRefrigerationErrorMessage);
			AssertNoError(booking.KM_RequiresRefrigerationInfo, expectedRequireRefrigerationErrorMessage);

			booking.KM_RequiresRefrigeration = true;
			AssertHasError(booking.KM_RequiresRefrigerationInfo, expectedRequireNoRefrigerationErrorMessage);
			AssertNoError(booking.KM_RequiresRefrigerationInfo, expectedRequireRefrigerationErrorMessage);

			packageChild.KP_RequiresTemperatureControl = true;
			booking.KM_RequiresRefrigeration = false;
			AssertNoError(booking.KM_RequiresRefrigerationInfo, expectedRequireNoRefrigerationErrorMessage);
			AssertNoError(booking.KM_RequiresRefrigerationInfo, expectedRequireRefrigerationErrorMessage);

			// With package with temperature control required
			booking.Instructions.AddNew().DivotsWithPackages.AddPackage(packageTopLevel);
			booking.KM_RequiresRefrigeration = true;
			AssertNoError(booking.KM_RequiresRefrigerationInfo, expectedRequireNoRefrigerationErrorMessage);
			AssertNoError(booking.KM_RequiresRefrigerationInfo, expectedRequireRefrigerationErrorMessage);

			booking.KM_RequiresRefrigeration = false;
			AssertNoError(booking.KM_RequiresRefrigerationInfo, expectedRequireNoRefrigerationErrorMessage);
			AssertHasError(booking.KM_RequiresRefrigerationInfo, expectedRequireRefrigerationErrorMessage);

			// With container with no temperature control required
			packageChild.KP_RequiresTemperatureControl = false;
			packageChild.KP_F3_NKPackType = Constants.PkgUnit.Container;
			booking.KM_RequiresRefrigeration = true;
			packageChild.KP_RequiresTemperatureControl = false;
			AssertHasError(booking.KM_RequiresRefrigerationInfo, expectedRequireNoRefrigerationErrorMessage);
			AssertNoError(booking.KM_RequiresRefrigerationInfo, expectedRequireRefrigerationErrorMessage);

			// With container with temperature control required
			packageChild.Container.K0_IsControlledAtmosphere = true;
			booking.KM_RequiresRefrigeration = true;
			AssertNoError(booking.KM_RequiresRefrigerationInfo, expectedRequireNoRefrigerationErrorMessage);
			AssertNoError(booking.KM_RequiresRefrigerationInfo, expectedRequireRefrigerationErrorMessage);

			booking.KM_RequiresRefrigeration = false;
			AssertNoError(booking.KM_RequiresRefrigerationInfo, expectedRequireNoRefrigerationErrorMessage);
			AssertHasError(booking.KM_RequiresRefrigerationInfo, expectedRequireRefrigerationErrorMessage);
		}

		public void TestCheckKM_RequiresRefrigeration_SkippedForSubBooking()
		{
			const string NoExpectedErrorsAssertionMessage = "Sub bookings should not trigger validation error on KM_RequiresRefrigeration";
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			Factory.Save();

			var subBooking = Helper.CreateBooking();
			var package = subBooking.PackageJob.Packages.AddNew();
			Factory.Save();

			masterBooking.SubBookings.Add(subBooking);
			Factory.Save();

			AssertNoErrors(NoExpectedErrorsAssertionMessage, subBooking.KM_RequiresRefrigerationInfo);

			var masterInstruction = masterBooking.Instructions.AddNew();

			masterInstruction.DivotsWithPackages.AddPackage(package);

			AssertEquals("Precondition: subBooking.KM_RequiresRefrigeration is false", false, subBooking.KM_RequiresRefrigeration);
			AssertNoErrors(NoExpectedErrorsAssertionMessage, subBooking.KM_RequiresRefrigerationInfo);

			subBooking.KM_RequiresRefrigeration = true;
			AssertNoErrors(NoExpectedErrorsAssertionMessage, subBooking.KM_RequiresRefrigerationInfo);

			package.KP_RequiresTemperatureControl = true;
			AssertNoErrors(NoExpectedErrorsAssertionMessage, subBooking.KM_RequiresRefrigerationInfo);

			subBooking.KM_RequiresRefrigeration = false;
			AssertNoErrors(NoExpectedErrorsAssertionMessage, subBooking.KM_RequiresRefrigerationInfo);
		}

		public void TestValidateKM_RS_NKServiceLevel()
		{
			var booking = GetNewBooking();
			booking.KM_RS_NKServiceLevel = "";
			AssertNoNotifications(booking.KM_RS_NKServiceLevelInfo);

			booking.KM_RS_NKServiceLevel = "ABC";
			AssertHasError(booking.KM_RS_NKServiceLevelInfo, "Enter a valid Service Level.");

			booking.KM_RS_NKServiceLevel = "STD";
			AssertNoErrors("STD is a system default service level.", booking.KM_RS_NKServiceLevelInfo);

			booking.KM_RS_NKServiceLevel = "AAA";
			AssertHasError(booking.KM_RS_NKServiceLevelInfo, "Enter a valid Service Level.");

			booking.KM_RS_NKServiceLevel = "";
			AssertNoNotifications(booking.KM_RS_NKServiceLevelInfo);
		}

		public void TestValidateMasterAttachment_KM_IsMaster()
		{
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			masterBooking.KM_MasterBookingVersion = 2;

			var subBooking = Helper.CreateBooking();
			subBooking.KM_IsMaster = true;
			subBooking.KM_MasterBookingVersion = 1;

			Factory.Save();

			subBooking.Validation.ValidateAll();
			AssertNoErrors(subBooking);

			masterBooking.SubBookings.Add(subBooking);
			subBooking.Validation.ValidateAll();
			AssertHasError("Should say the sub booking is invalid because KM_IsMaster is true", subBooking.KM_IsMasterInfo, "Master Bookings cannot be attached to another Master Booking.");
		}

		public void TestValidateMasterAttachment_KM_KM_MasterBooking()
		{
			var masterBooking1 = Helper.CreateBooking();
			masterBooking1.KM_IsMaster = true;
			masterBooking1.KM_MasterBookingVersion = 1;

			var masterBooking2 = Helper.CreateBooking();
			masterBooking2.KM_IsMaster = true;
			masterBooking2.KM_MasterBookingVersion = 1;

			var subBooking = Helper.CreateBooking();

			Factory.Save();

			masterBooking1.SubBookings.Add(subBooking);
			Factory.Save();

			subBooking.Validation.ValidateAll();
			AssertNoErrors(subBooking);

			masterBooking2.SubBookings.Add(subBooking);
			subBooking.Validation.ValidateAll();
			AssertHasError("Should say switching from one master to another master is invalid", subBooking.KM_KM_MasterBookingInfo, "Booking cannot change directly from one Master Booking to another Master Booking.");
		}

		public void TestValidateMasterAttachment_KM_KM_MasterBooking_AllowsDetachment()
		{
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			masterBooking.KM_MasterBookingVersion = 1;

			var subBooking = Helper.CreateBooking();

			Factory.Save();

			masterBooking.SubBookings.Add(subBooking);
			Factory.Save();

			masterBooking.SubBookings.RemoveFromRelationship(subBooking);
			subBooking.Validation.ValidateAll();
			AssertNoErrors(subBooking);
		}

		public void TestValidateMasterAttachment_KM_MasterBookingVersion()
		{
			var masterBooking1 = Helper.CreateBooking();
			masterBooking1.KM_IsMaster = true;
			masterBooking1.KM_MasterBookingVersion = 1;

			var masterBooking2 = Helper.CreateBooking();
			masterBooking2.KM_IsMaster = true;
			masterBooking2.KM_MasterBookingVersion = 1;

			var subBooking = Helper.CreateBooking();

			Factory.Save();

			masterBooking1.SubBookings.Add(subBooking);
			Factory.Save();
			AssertNoErrors("Should not regard as invalid a sub booking having its MasterBookingVersion set as an attached sub", subBooking);

			subBooking.Validation.ValidateAll();
			AssertNoErrors("Should not regard as invalid a sub booking having its MasterBookingVersion set as an attached sub", subBooking);
			AssertEquals("Precondition: MasterBookingVersion is non-zero", true, subBooking.KM_MasterBookingVersion > 0);

			masterBooking2.SubBookings.Add(subBooking);
			subBooking.Validation.ValidateAll();
			AssertHasError("Should say attaching a booking which already has a non-zero MasterBookingVersion is invalid", subBooking.KM_MasterBookingVersionInfo, "Cannot attach a Booking with a non-zero Master Booking Version.");
		}

		public void TestValidateMasterAttachment_DoesNotCreateAddresses()
		{
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			masterBooking.KM_MasterBookingVersion = 1;

			var subBooking = Helper.CreateBooking();

			Factory.Save();

			masterBooking.SubBookings.Add(subBooking);
			subBooking.OnSaving();

			subBooking.Validation.ValidateAll();
			var docAddresses = new JobDocAddressDependentCollection(subBooking);
			docAddresses.Load();
			AssertEquals("Should not have any addresses associated with the sub booking", 0, docAddresses.Count);
		}

		public void TestValidateMasterAttachment_MultiJob()
		{
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			masterBooking.KM_MasterBookingVersion = 1;

			var subBooking = Helper.CreateBooking();

			var consolidationMultiJob = Helper.CreateConsolidationMultiJob();
			var collectionMultiJob = new DtbBookingCollection(consolidationMultiJob);
			collectionMultiJob.Add(subBooking);

			Factory.Save();

			subBooking.Validation.ValidateAll();
			AssertNoErrors(subBooking);

			masterBooking.SubBookings.Add(subBooking);
			subBooking.Validation.ValidateAll();
			AssertHasError("Should say the sub booking is invalid because is has a multijob", subBooking.KM_KB_BookingConsolidationMultiJobInfo, "Cannot attach Booking which is part of a Consolidated Booking to a Master Booking.");
		}

		public void TestValidateMasterAttachment_KM_Status()
		{
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			masterBooking.KM_MasterBookingVersion = 1;

			var subBooking = Helper.CreateBooking();
			subBooking.KM_Status = TransportStatuses.Codes.Delivered;

			Factory.Save();

			subBooking.Validation.ValidateAll();
			AssertNoErrors(subBooking);

			masterBooking.SubBookings.Add(subBooking);
			subBooking.Validation.ValidateAll();
			AssertHasError("Should say the sub booking is invalid because it status is not available", subBooking.KM_StatusInfo, "Cannot attach Booking which does not have a status of Available");

			subBooking.KM_Status = TransportStatuses.Codes.Available;
			Factory.Save();

			AssertEquals("Precondition: sub is attached to master", true, subBooking.IsSub);
			masterBooking.KM_Status = TransportStatuses.Codes.Delivered;
			subBooking.KM_Status = TransportStatuses.Codes.Delivered;
			subBooking.Validation.ValidateAll();
			AssertNoErrors("Should allow an already attached booking to have a status other than available", subBooking);
		}

		public void TestValidateMasterAttachment_KM_IsAgentBooking()
		{
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			masterBooking.KM_MasterBookingVersion = 1;

			var subBooking = Helper.CreateBooking();
			subBooking.KM_IsAgentBooking = true;

			Factory.Save();

			subBooking.Validation.ValidateAll();
			AssertNoErrors(subBooking);

			masterBooking.SubBookings.Add(subBooking);
			subBooking.Validation.ValidateAll();
			AssertHasError("Should say the sub booking is invalid because it is an agent booking", subBooking.KM_IsAgentBookingInfo, "Cannot attach an Agent Booking");
		}

		public void TestValidateMasterAttachment_KM_Direction()
		{
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			masterBooking.KM_MasterBookingVersion = 1;

			var matchingBooking = Helper.CreateBooking();
			var nonMatchingBooking = Helper.CreateBooking();

			matchingBooking.KM_Direction = masterBooking.KM_Direction = "PIC";
			nonMatchingBooking.KM_Direction = "DLV";

			Factory.Save();

			var subBookings = new DtbBooking[] { matchingBooking, nonMatchingBooking };
			subBookings.ForEach(b => b.Validation.ValidateAll());
			AssertNoErrors(matchingBooking);
			AssertNoErrors(nonMatchingBooking);

			masterBooking.SubBookings.AddRange(subBookings);
			subBookings.ForEach(b => b.Validation.ValidateAll());
			AssertNoErrors("Should accept the sub with matching KM_Direction", matchingBooking);
			AssertHasError("Should say the sub booking is invalid because KM_Direction does not match the master", nonMatchingBooking.KM_DirectionInfo, "This Booking has a different booking direction to the Master Booking.");

			masterBooking.SubBookings.RemoveFromRelationship(nonMatchingBooking);
			Factory.Save();

			subBookings.ForEach(b => b.Validation.ValidateAll());
			AssertNoErrors("Should not raise validation error when already attached", matchingBooking);

			masterBooking.KM_Direction = "ORG";
			subBookings.ForEach(b => b.Validation.ValidateAll());
			AssertNoErrors("Should not raise validation error when master changes", matchingBooking);
		}

		public void TestValidateMasterAttachment_KM_KT_NKBookingTemplate()
		{
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			masterBooking.KM_MasterBookingVersion = 1;

			var matchingBooking = Helper.CreateBooking();
			var nonMatchingBooking = Helper.CreateBooking();

			matchingBooking.KM_KT_NKBookingTemplate = masterBooking.KM_KT_NKBookingTemplate = "PLCW";
			nonMatchingBooking.KM_KT_NKBookingTemplate = "LC2C";

			Factory.Save();

			var subBookings = new DtbBooking[] { matchingBooking, nonMatchingBooking };
			subBookings.ForEach(b => b.Validation.ValidateAll());
			AssertNoErrors(matchingBooking);
			AssertNoErrors(nonMatchingBooking);

			masterBooking.SubBookings.AddRange(subBookings);
			subBookings.ForEach(b => b.Validation.ValidateAll());
			AssertNoErrors("Should accept the sub with matching Booking Template", matchingBooking);
			AssertHasError("Should say the sub booking is invalid because Booking Template does not match the master", nonMatchingBooking.KM_KT_NKBookingTemplateInfo, "This Booking has a different template to the Master Booking.");

			masterBooking.SubBookings.RemoveFromRelationship(nonMatchingBooking);
			Factory.Save();

			subBookings.ForEach(b => b.Validation.ValidateAll());
			AssertNoErrors("Should not raise validation error when already attached", matchingBooking);

			masterBooking.KM_KT_NKBookingTemplate = "EFPR"; // since templates change the KM_Direction this could cause the other validation for direction to fail, if replication doesn't happen (which it doesn't here). also should there be a save here?
			subBookings.ForEach(b => b.Validation.ValidateAll());
			AssertNoErrors("Should not raise validation error when master changes", matchingBooking);
		}

		public void TestValidateMasterAttachment_CarrierBookingAgentForBlanks()
		{
			Func<DtbBooking, JobDocAddress> returnCarrierBookingAgentAddress = delegate(DtbBooking booking) { return booking.CarrierBookingAgentDocAddress; };
			TestValidateMasterAttachmentUsingOrgsForBlanksCore(returnCarrierBookingAgentAddress, "Carrier Booking Agent", "This Booking does not have the same Carrier Booking Agent as the Master Booking ().");
		}

		public void TestValidateMasterAttachment_CarrierBookingAgentForNonBlanks()
		{
			Func<DtbBooking, JobDocAddress> returnCarrierBookingAgentAddress = delegate(DtbBooking booking) { return booking.CarrierBookingAgentDocAddress; };
			TestValidateMasterAttachmentUsingOrgsForNonBlanksCore(returnCarrierBookingAgentAddress, "Carrier Booking Agent", "This Booking does not have the same Carrier Booking Agent as the Master Booking (ABC).");
		}

		public void TestValidateMasterAttachment_TransportCompanyForBlanks()
		{
			Func<DtbBooking, JobDocAddress> returnTransportCoAddress = delegate(DtbBooking booking) { return booking.Address; };
			TestValidateMasterAttachmentUsingOrgsForBlanksCore(returnTransportCoAddress, "Transport Company", "This Booking does not have the same Transport Company as the Master Booking ().");
		}

		public void TestValidateMasterAttachment_TransportCompanyForNonBlanks()
		{
			Func<DtbBooking, JobDocAddress> returnTransportCoAddress = delegate(DtbBooking booking) { return booking.Address; };
			TestValidateMasterAttachmentUsingOrgsForNonBlanksCore(returnTransportCoAddress, "Transport Company", "This Booking does not have the same Transport Company as the Master Booking (ABC).");
		}

		public void TestValidateMasterAttachment_FirstPickupAddressForBlanks()
		{
			Func<DtbBooking, JobDocAddress> returnFirstPUA = delegate(DtbBooking booking) { return booking.FirstPickup.Address; };
			TestValidateMasterAttachmentUsingAddressesForNonBlanksCore(returnFirstPUA, "PIC", "This Booking has a different First Pickup Address to the Master Booking.");
		}

		public void TestValidateMasterAttachment_FirstPickupAddressForNonBlanks()
		{
			Func<DtbBooking, JobDocAddress> returnFirstPUA = delegate(DtbBooking booking) { return booking.FirstPickup.Address; };
			TestValidateMasterAttachmentUsingAddressesForBlanksCore(returnFirstPUA, "PIC", "This Booking has a different First Pickup Address to the Master Booking.");
		}

		public void TestValidateMasterAttachment_FirstConsigneeAddressForNonBlanks()
		{
			Func<DtbBooking, JobDocAddress> returnFirstConsigneeAddress = delegate(DtbBooking booking) { return booking.FirstConsignee.Address; };
			TestValidateMasterAttachmentUsingAddressesForNonBlanksCore(returnFirstConsigneeAddress, "CNE", "This Booking has a different First Consignee Address to the Master Booking.");
		}

		public void TestValidateMasterAttachment_FirstConsigneeAddressForBlanks()
		{
			Func<DtbBooking, JobDocAddress> returnFirstConsigneeAddress = delegate(DtbBooking booking) { return booking.FirstConsignee.Address; };
			TestValidateMasterAttachmentUsingAddressesForBlanksCore(returnFirstConsigneeAddress, "CNE", "This Booking has a different First Consignee Address to the Master Booking.");
		}

		public void TestValidateMasterAttachment_FirstConsignorAddressForNonBlanks()
		{
			Func<DtbBooking, JobDocAddress> returnFirstConsignorAddress = delegate(DtbBooking booking) { return booking.FirstConsignor.Address; };
			TestValidateMasterAttachmentUsingAddressesForNonBlanksCore(returnFirstConsignorAddress, "CNR", "This Booking has a different First Consignor Address to the Master Booking.");
		}

		public void TestValidateMasterAttachment_FirstConsignorAddressForBlanks()
		{
			Func<DtbBooking, JobDocAddress> returnFirstConsignorAddress = delegate(DtbBooking booking) { return booking.FirstConsignor.Address; };
			TestValidateMasterAttachmentUsingAddressesForBlanksCore(returnFirstConsignorAddress, "CNR", "This Booking has a different First Consignor Address to the Master Booking.");
		}

		public void TestValidateMasterAttachment_LastDeliveryAddressForNonBlanks()
		{
			Func<DtbBooking, JobDocAddress> returnLastDeliveryAddress = delegate(DtbBooking booking) { return booking.LastDelivery.Address; };
			TestValidateMasterAttachmentUsingAddressesForNonBlanksCore(returnLastDeliveryAddress, "DLV", "This Booking has a different Last Delivery Address to the Master Booking.");
		}

		public void TestValidateMasterAttachment_LastDeliveryAddressForBlanks()
		{
			Func<DtbBooking, JobDocAddress> returnLastDeliveryAddress = delegate(DtbBooking booking) { return booking.LastDelivery.Address; };
			TestValidateMasterAttachmentUsingAddressesForBlanksCore(returnLastDeliveryAddress, "DLV", "This Booking has a different Last Delivery Address to the Master Booking.");
		}

		void TestValidateMasterAttachmentUsingAddressesForNonBlanksCore(Func<DtbBooking, JobDocAddress> getInstructionType, string testItem, string warningMessage)
		{
			var address1 = Factory.New<JobDocAddress>();
			address1.Address1 = "10 Road St";
			address1.Address2 = "Unit 75A";
			address1.Postcode = "2133";
			address1.State = "New South Wales";
			address1.E2_RN_NKCountryCode = "AU";
			var address2 = Factory.New<JobDocAddress>();
			address2.Address1 = "10 Test St";
			address2.Address2 = "Warehouse 5";
			address2.Postcode = "2177";
			address2.State = "British Columbia";
			address2.E2_RN_NKCountryCode = "CA";
			var address3 = Factory.New<JobDocAddress>();
			address3.Address1 = "10 Street Rd";
			address3.Address2 = "Building 1";
			address3.Postcode = "2216";
			address3.State = "Ohio";
			address3.E2_RN_NKCountryCode = "US";

			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			masterBooking.KM_MasterBookingVersion = 1;

			var subBookingWithSameAddress = Helper.CreateBooking();

			var subBookingWithDifferentAddress = Helper.CreateBooking();

			if (testItem == "PIC" || testItem == "DLV")
			{
				var instruction1 = Helper.CreateInstruction(subBookingWithSameAddress, testItem);
				var instruction2 = Helper.CreateInstruction(subBookingWithDifferentAddress, testItem);
				var instruction3 = Helper.CreateInstruction(masterBooking, testItem);
			}
			if (testItem == "CNE" || testItem == "CNR")
			{
				var instruction1 = Helper.CreateInstruction(subBookingWithSameAddress);
				var instruction2 = Helper.CreateInstruction(subBookingWithDifferentAddress);
				var instruction3 = Helper.CreateInstruction(masterBooking);
				instruction1.OrganisationType = testItem;
				instruction2.OrganisationType = testItem;
				instruction3.OrganisationType = testItem;
			}

			getInstructionType(masterBooking).Address1 = getInstructionType(subBookingWithSameAddress).Address1 = address1.Address1;
			getInstructionType(masterBooking).Address2 = getInstructionType(subBookingWithSameAddress).Address2 = address1.Address2;
			getInstructionType(masterBooking).Postcode = getInstructionType(subBookingWithSameAddress).Postcode = address1.Postcode;
			getInstructionType(masterBooking).State = getInstructionType(subBookingWithSameAddress).State = address1.State;
			getInstructionType(masterBooking).E2_RN_NKCountryCode = getInstructionType(subBookingWithSameAddress).E2_RN_NKCountryCode = address1.E2_RN_NKCountryCode;
			getInstructionType(subBookingWithDifferentAddress).Address1 = address2.Address1;
			getInstructionType(subBookingWithDifferentAddress).Address2 = address2.Address2;
			getInstructionType(subBookingWithDifferentAddress).Postcode = address2.Postcode;
			getInstructionType(subBookingWithDifferentAddress).State = address2.State;
			getInstructionType(subBookingWithDifferentAddress).E2_RN_NKCountryCode = address2.E2_RN_NKCountryCode;

			var subBookingWithNoAddress = Helper.CreateBooking();

			Factory.Save();

			var subBookings = new DtbBooking[] { subBookingWithSameAddress, subBookingWithDifferentAddress, subBookingWithNoAddress };
			masterBooking.SubBookings.AddRange(subBookings);

			subBookings.ForEach(b => b.Validation.ValidateAll());
			AssertHasRowError("Should not accept sub booking with blank " + testItem + " address", subBookingWithNoAddress, warningMessage);
			AssertHasRowError("Should not accept sub booking with different " + testItem + " address", subBookingWithDifferentAddress, warningMessage);
			AssertNoErrors("Should accept sub booking with matching " + testItem + " address", subBookingWithSameAddress);

			masterBooking.SubBookings.RemoveFromRelationship(subBookingWithDifferentAddress);
			masterBooking.SubBookings.RemoveFromRelationship(subBookingWithNoAddress);
			Factory.Save();

			subBookingWithSameAddress.Validation.ValidateAll();
			AssertNoErrors("Should not raise validation error when already attached", subBookingWithSameAddress);

			getInstructionType(masterBooking).Address1 = address3.Address1;
			getInstructionType(masterBooking).Address2 = address3.Address2;
			getInstructionType(masterBooking).Postcode = address3.Postcode;
			getInstructionType(masterBooking).State = address3.State;
			getInstructionType(masterBooking).E2_RN_NKCountryCode = address3.E2_RN_NKCountryCode;
			subBookingWithSameAddress.Validation.ValidateAll();
			AssertNoErrors("Should not raise validation error when master changes", subBookingWithSameAddress);
		}

		void TestValidateMasterAttachmentUsingAddressesForBlanksCore(Func<DtbBooking, JobDocAddress> getInstructionType, string testItem, string warningMessage)
		{
			var address1 = Factory.New<JobDocAddress>();
			address1.Address1 = "10 Road St";
			address1.Address2 = "Unit 75A";
			address1.Postcode = "2133";
			address1.State = "New South Wales";
			address1.E2_RN_NKCountryCode = "AU";
			var address2 = Factory.New<JobDocAddress>();
			address2.Address1 = "10 Test St";
			address2.Address2 = "Warehouse 5";
			address2.Postcode = "2177";
			address2.State = "British Columbia";
			address2.E2_RN_NKCountryCode = "CA";

			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			masterBooking.KM_MasterBookingVersion = 1;
			var subBookingWithNonBlankAddress = Helper.CreateBooking();

			if (testItem == "PIC" || testItem == "DLV")
			{
				var instruction1 = Helper.CreateInstruction(subBookingWithNonBlankAddress, testItem);
			}
			if (testItem == "CNE" || testItem == "CNR")
			{
				var instruction1 = Helper.CreateInstruction(subBookingWithNonBlankAddress);
				instruction1.OrganisationType = testItem;
			}

			getInstructionType(subBookingWithNonBlankAddress).Address1 = address1.Address1;
			getInstructionType(subBookingWithNonBlankAddress).Address2 = address1.Address2;
			getInstructionType(subBookingWithNonBlankAddress).Postcode = address1.Postcode;
			getInstructionType(subBookingWithNonBlankAddress).State = address1.State;
			getInstructionType(subBookingWithNonBlankAddress).E2_RN_NKCountryCode = address1.E2_RN_NKCountryCode;

			var subBookingWithNoAddress = Helper.CreateBooking();

			Factory.Save();

			var subBookings = new DtbBooking[] { subBookingWithNonBlankAddress, subBookingWithNoAddress };
			masterBooking.SubBookings.AddRange(subBookings);

			subBookings.ForEach(b => b.Validation.ValidateAll());
			AssertHasRowError("Should not accept sub booking with different " + testItem + " address", subBookingWithNonBlankAddress, warningMessage);
			AssertNoErrors("Should accept sub booking with matching blank " + testItem + " address", subBookingWithNoAddress);

			masterBooking.SubBookings.RemoveFromRelationship(subBookingWithNonBlankAddress);
			Factory.Save();

			subBookingWithNoAddress.Validation.ValidateAll();
			AssertNoErrors("Should not raise validation error when already attached", subBookingWithNoAddress);

			if (testItem == "PIC" || testItem == "DLV")
			{
				var instruction2 = Helper.CreateInstruction(masterBooking, testItem);
			}
			if (testItem == "CNE" || testItem == "CNR")
			{
				var instruction2 = Helper.CreateInstruction(masterBooking);
				instruction2.OrganisationType = testItem;
			}

			getInstructionType(masterBooking).Address1 = address2.Address1;
			getInstructionType(masterBooking).Address2 = address2.Address2;
			getInstructionType(masterBooking).Postcode = address2.Postcode;
			getInstructionType(masterBooking).State = address2.State;
			getInstructionType(masterBooking).E2_RN_NKCountryCode = address2.E2_RN_NKCountryCode;
			subBookingWithNoAddress.Validation.ValidateAll();
			AssertNoErrors("Should not raise validation error when master changes", subBookingWithNoAddress);
		}

		void TestValidateMasterAttachmentUsingOrgsForNonBlanksCore(Func<DtbBooking, JobDocAddress> getInstructionType, string testItem, string warningMessage)
		{
			var orgABC = Helper.CreateOrganisation("ABC");
			var orgAddressABC = Helper.CreateOrgAddress(Factory, orgABC, "ABC");
			var orgAddressABC2 = Helper.CreateOrgAddress(Factory, orgABC, "ABD");
			var orgDEF = Helper.CreateOrganisation("DEF");
			var orgAddressDEF = Helper.CreateOrgAddress(Factory, orgDEF, "DEF");
			var orgGHI = Helper.CreateOrganisation("GHI");
			var orgAddressGHI = Helper.CreateOrgAddress(Factory, orgGHI, "GHI");

			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			masterBooking.KM_MasterBookingVersion = 1;

			var subBookingWithSameOrgHeaderSameAddress = Helper.CreateBooking();
			var subBookingWithSameOrgHeaderDiffAddress = Helper.CreateBooking();

			var subBookingWithDiffOrgHeaderDiffAddress = Helper.CreateBooking();

			getInstructionType(masterBooking).E2_OA_Address = orgAddressABC.PK;
			getInstructionType(subBookingWithSameOrgHeaderSameAddress).E2_OA_Address = orgAddressABC.PK;
			getInstructionType(subBookingWithSameOrgHeaderDiffAddress).E2_OA_Address = orgAddressABC2.PK;
			getInstructionType(subBookingWithDiffOrgHeaderDiffAddress).E2_OA_Address = orgAddressDEF.PK;

			var subBookingWithNoAddress = Helper.CreateBooking();

			Factory.Save();

			var subBookings = new DtbBooking[] { subBookingWithDiffOrgHeaderDiffAddress, subBookingWithSameOrgHeaderDiffAddress, subBookingWithSameOrgHeaderSameAddress, subBookingWithNoAddress };
			masterBooking.SubBookings.AddRange(subBookings);

			subBookings.ForEach(b => b.Validation.ValidateAll());
			AssertHasRowError("Should not accept sub booking with blank " + testItem + " address", subBookingWithNoAddress, warningMessage);
			AssertHasRowError("Should not accept sub booking with different " + testItem + " address", subBookingWithDiffOrgHeaderDiffAddress, warningMessage);
			AssertHasRowError("Should not accept sub booking with different " + testItem + " address", subBookingWithSameOrgHeaderDiffAddress, warningMessage);
			AssertNoErrors("Should accept sub booking with matching " + testItem + " address", subBookingWithSameOrgHeaderSameAddress);

			masterBooking.SubBookings.RemoveFromRelationship(subBookingWithSameOrgHeaderDiffAddress);
			masterBooking.SubBookings.RemoveFromRelationship(subBookingWithDiffOrgHeaderDiffAddress);
			masterBooking.SubBookings.RemoveFromRelationship(subBookingWithNoAddress);
			Factory.Save();

			subBookingWithSameOrgHeaderSameAddress.Validation.ValidateAll();
			AssertNoErrors("Should not raise validation error when already attached", subBookingWithSameOrgHeaderSameAddress);

			getInstructionType(masterBooking).E2_OA_Address = orgAddressGHI.PK;
			subBookingWithSameOrgHeaderSameAddress.Validation.ValidateAll();
			AssertNoErrors("Should not raise validation error when master changes", subBookingWithSameOrgHeaderSameAddress);
		}

		void TestValidateMasterAttachmentUsingOrgsForBlanksCore(Func<DtbBooking, JobDocAddress> getInstructionType, string testItem, string warningMessage)
		{
			var orgABC = Helper.CreateOrganisation("ABC");
			var orgAddressABC = Helper.CreateOrgAddress(Factory, orgABC, "ABC");
			var orgDEF = Helper.CreateOrganisation("DEF");
			var orgAddressDEF = Helper.CreateOrgAddress(Factory, orgDEF, "DEF");

			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			masterBooking.KM_MasterBookingVersion = 1;
			var subBookingWithNonBlankAddress = Helper.CreateBooking();

			getInstructionType(subBookingWithNonBlankAddress).E2_OA_Address = orgAddressABC.PK;

			var subBookingWithNoAddress = Helper.CreateBooking();

			Factory.Save();

			var subBookings = new DtbBooking[] { subBookingWithNonBlankAddress, subBookingWithNoAddress };
			masterBooking.SubBookings.AddRange(subBookings);

			subBookings.ForEach(b => b.Validation.ValidateAll());
			AssertHasRowError("Should not accept sub booking with different " + testItem + " address", subBookingWithNonBlankAddress, warningMessage);
			AssertNoErrors("Should accept sub booking with matching blank " + testItem + " address", subBookingWithNoAddress);

			masterBooking.SubBookings.RemoveFromRelationship(subBookingWithNonBlankAddress);
			Factory.Save();

			subBookingWithNoAddress.Validation.ValidateAll();
			AssertNoErrors("Should not raise validation error when already attached", subBookingWithNoAddress);

			getInstructionType(masterBooking).E2_OA_Address = orgAddressDEF.PK;
			subBookingWithNoAddress.Validation.ValidateAll();
			AssertNoErrors("Should not raise validation error when master changes", subBookingWithNoAddress);
		}

		public void TestNoWarningsWhenSubsInSyncWithMaster()
		{
			(var masterBooking, var subBooking) = CreateInSyncMasterAndSubBookingWithInstructionsAndConfirmations();

			Factory.Save();

			Assert("Master Booking has a KM_MasterBookingVersion of 1", masterBooking.KM_MasterBookingVersion == 1);
			Assert("Sub Booking has a KM_MasterBookingVersion of 1", subBooking.KM_MasterBookingVersion == 1);

			masterBooking.KM_MasterBookingVersion = 2;
			subBooking.KM_MasterBookingVersion = 2;

			Factory.Save();

			AssertNoRowWarnings(subBooking);
		}

		public void TestWarningWhenSubOutOfSyncWithMaster()
		{
			(var masterBooking, var subBooking) = CreateInSyncMasterAndSubBookingWithInstructionsAndConfirmations();

			Factory.Save();

			Assert("Master Booking has a KM_MasterBookingVersion of 1", masterBooking.KM_MasterBookingVersion == 1);
			Assert("Sub Booking has a KM_MasterBookingVersion of 1", subBooking.KM_MasterBookingVersion == 1);

			masterBooking.KM_MasterBookingVersion = 2;

			Factory.Save();

			AssertHasRowWarning("When sub is not in sync, it should contain an out of sync warning.", subBooking, "Not in sync with Master - Update service task in progress.");
		}

		public void TestSpecificWarningsForSubsWhenSomeOutOfSyncWithMaster()
		{
			(var masterBooking, var subBooking1) = CreateInSyncMasterAndSubBookingWithNoInstructions();

			var subBooking2 = CreateInSyncMasterAndSubBookingWithNoInstructions(masterBooking);

			var subBooking3 = CreateInSyncMasterAndSubBookingWithNoInstructions(masterBooking);

			Factory.Save();

			Assert("Master Booking has a KM_MasterBookingVersion of 1", masterBooking.KM_MasterBookingVersion == 1);
			Assert("Sub Booking 1 has a KM_MasterBookingVersion of 1", subBooking1.KM_MasterBookingVersion == 1);
			Assert("Sub Booking 2 has a KM_MasterBookingVersion of 1", subBooking2.KM_MasterBookingVersion == 1);
			Assert("Sub Booking 3 has a KM_MasterBookingVersion of 1", subBooking3.KM_MasterBookingVersion == 1);

			masterBooking.KM_MasterBookingVersion = 2;
			subBooking1.KM_MasterBookingVersion = 2;
			subBooking3.KM_MasterBookingVersion = 2;

			Factory.Save();

			AssertNoRowWarnings(subBooking1);
			AssertHasRowWarning("When sub is not in sync, it should contain an out of sync warning.", subBooking2, "Not in sync with Master - Update service task in progress.");
			AssertNoRowWarnings(subBooking3);
		}

		public void TestWarningsOnSubsWhenOutOfSyncWithMasterWhenFieldModifiedWithoutServiceTasksRun()
		{
			(var masterBooking, var subBooking) = CreateInSyncMasterAndSubBookingWithNoInstructions();

			Factory.Save();

			Assert("Master Booking has a KM_MasterBookingVersion of 1", masterBooking.KM_MasterBookingVersion == 1);
			Assert("Sub Booking has a KM_MasterBookingVersion of 1", subBooking.KM_MasterBookingVersion == 1);

			masterBooking.KM_RS_NKServiceLevel = "EXP";

			Factory.Save();

			Assert("Precondition", masterBooking.KM_RS_NKServiceLevel == "EXP");

			AssertHasRowWarning("When sub is not in sync, it should contain an out of sync warning.", subBooking, "Not in sync with Master - Update service task in progress.");
		}

		public void TestWarningsOnSubsWhenOutOfSyncWithMasterAndMasterIsReloaded()
		{
			(var masterBooking, var subBooking) = CreateInSyncMasterAndSubBookingWithNoInstructions();

			Factory.Save();

			Assert("Master Booking has a KM_MasterBookingVersion of 1", masterBooking.KM_MasterBookingVersion == 1);
			Assert("Sub Booking has a KM_MasterBookingVersion of 1", subBooking.KM_MasterBookingVersion == 1);

			masterBooking.KM_RS_NKServiceLevel = "EXP";

			Assert("Precondition", masterBooking.KM_RS_NKServiceLevel == "EXP");

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var masterNewFactory = newFactory.Load<DtbBooking>(masterBooking.PK);
			var subNewFactory = masterNewFactory.SubBookings.First();

			AssertHasRowWarning("When sub is not in sync, it should contain an out of sync warning.", subNewFactory, "Not in sync with Master - Update service task in progress.");
		}

		(DtbBooking masterBooking, DtbBooking subBooking) CreateInSyncMasterAndSubBookingWithNoInstructions()
		{
			var subBookingConsolidation = Helper.CreateConsolidation();
			subBookingConsolidation.KB_JobType = "BKG";
			subBookingConsolidation.KB_JobDirection = "PIC";
			var subBooking = Helper.CreateBooking(subBookingConsolidation);
			subBooking.KM_Direction = "ORG";
			var masterBookingConsolidation = Helper.CreateConsolidation();
			masterBookingConsolidation.KB_JobType = "BKG";
			masterBookingConsolidation.KB_JobDirection = "PIC";
			var masterBooking = Helper.CreateBooking(masterBookingConsolidation);
			masterBooking.KM_IsMaster = true;
			masterBooking.KM_Direction = "ORG";
			subBooking.KM_KM_MasterBooking = masterBooking.PK;
			subBookingConsolidation.KB_KB_MasterBookingConsolidation = masterBookingConsolidation.PK;
			Factory.Save();
			subBooking.KM_MasterBookingVersion = masterBooking.KM_MasterBookingVersion;
			subBookingConsolidation.KB_MasterBookingVersion = masterBookingConsolidation.KB_MasterBookingVersion;
			Factory.Save();
			return (masterBooking, subBooking);
		}

		DtbBooking CreateInSyncMasterAndSubBookingWithNoInstructions(DtbBooking masterBooking)
		{
			var subBookingConsolidation = Helper.CreateConsolidation();
			subBookingConsolidation.KB_JobType = "BKG";
			subBookingConsolidation.KB_JobDirection = "PIC";
			var subBooking = Helper.CreateBooking(subBookingConsolidation);
			subBooking.KM_Direction = "ORG";
			subBooking.KM_KM_MasterBooking = masterBooking.PK;
			subBookingConsolidation.KB_KB_MasterBookingConsolidation = masterBooking.ConsolidationSingleJob.PK;
			Factory.Save();
			subBooking.KM_MasterBookingVersion = masterBooking.KM_MasterBookingVersion;
			subBookingConsolidation.KB_MasterBookingVersion = masterBooking.ConsolidationSingleJob.KB_MasterBookingVersion;
			Factory.Save();
			return subBooking;
		}

		(DtbBooking masterBooking, DtbBooking subBooking) CreateInSyncMasterAndSubBookingWithInstructionsAndConfirmations()
		{
			(var masterBooking, var subBooking) = CreateInSyncMasterAndSubBookingWithNoInstructions();
			var masterBookingPicInstruction = masterBooking.Instructions.AddNew("PIC");
			masterBookingPicInstruction.KN_IsMaster = true;
			masterBookingPicInstruction.KN_MasterBookingVersion = 1;
			var masterBookingDlvInstruction = masterBooking.Instructions.AddNew("DLV");
			masterBookingDlvInstruction.KN_IsMaster = true;
			masterBookingDlvInstruction.KN_MasterBookingVersion = 1;
			var subBookingPicInstruction = subBooking.Instructions.AddNew("PIC");
			subBookingPicInstruction.KN_KN_MasterBookingInstruction = masterBookingPicInstruction.PK;
			subBookingPicInstruction.KN_MasterBookingVersion = 1;
			var subBookingDlvInstruction = subBooking.Instructions.AddNew("DLV");
			subBookingDlvInstruction.KN_KN_MasterBookingInstruction = masterBookingDlvInstruction.PK;
			subBookingDlvInstruction.KN_MasterBookingVersion = 1;
			var masterBookingPicConfirmation = masterBookingPicInstruction.Confirmations.AddNew("PIC");
			masterBookingPicConfirmation.KK_IsMaster = true;
			masterBookingPicConfirmation.KK_MasterBookingVersion = 1;
			var masterBookingDlvConfirmation = masterBookingDlvInstruction.Confirmations.AddNew("DLV");
			masterBookingDlvConfirmation.KK_IsMaster = true;
			masterBookingDlvConfirmation.KK_MasterBookingVersion = 1;
			var subBookingPicConfirmation = subBookingPicInstruction.Confirmations.AddNew("PIC");
			subBookingPicConfirmation.KK_KK_MasterBookingConfirmation = masterBookingPicConfirmation.PK;
			subBookingPicConfirmation.KK_MasterBookingVersion = 1;
			var subBookingDlvConfirmation = subBookingDlvInstruction.Confirmations.AddNew("DLV");
			subBookingDlvConfirmation.KK_KK_MasterBookingConfirmation = masterBookingDlvConfirmation.PK;
			subBookingDlvConfirmation.KK_MasterBookingVersion = 1;
			Factory.Save();
			return (masterBooking, subBooking);
		}

		public void TestGetNonMasterAddress()
		{
			var orgABC = Helper.CreateOrganisation("ABC");
			var orgAddressABC = Helper.CreateOrgAddress(Factory, orgABC, "ABC");
			var orgDEF = Helper.CreateOrganisation("DEF");
			var orgAddressDEF = Helper.CreateOrgAddress(Factory, orgDEF, "DEF");

			var booking = Helper.CreateBooking();
			booking.Address.E2_OA_Address = orgAddressABC.PK;
			var booking2 = Helper.CreateBooking();
			booking2.CarrierBookingAgentDocAddress.E2_OA_Address = orgAddressDEF.PK;

			Factory.Save();

			var nonMasterAddress = booking.Validation.GetNonMasterAddress(DocAddressType.TransportCompanyDocumentaryAddress, booking.PK);

			AssertEquals("E2_OA_Address given using the helper should correspond to the correct org addresses' PK", orgAddressABC.PK, nonMasterAddress.E2_OA_Address);
		}

		public void TestCheckKM_TransportModeWhenEmpty()
		{
			var booking = Helper.CreateBooking();

			AssertNoErrors("Precondition", booking.KM_TransportModeInfo);

			booking.KM_TransportMode = ZString.Empty;

			booking.Validation.ValidateAll();

			AssertHasError("Booking Transport Mode cannot be empty and must be a valid code (ROA, RAI or IWT).", booking.KM_TransportModeInfo, "Booking Transport Mode cannot be empty.");
		}

		public void TestCheckKM_TransportModeWhenCodeInvalid()
		{
			var booking = Helper.CreateBooking();

			AssertNoErrors("Precondition", booking.KM_TransportModeInfo);

			booking.KM_TransportMode = "XXX";

			booking.Validation.ValidateAll();

			AssertHasError("Booking Transport Mode must be a valid code (ROA, RAI or IWT).", booking.KM_TransportModeInfo, "Enter a valid Booking Transport Mode.");
		}

		public void TestValidateMasterAttachment_KM_TransportMode()
		{
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			masterBooking.KM_MasterBookingVersion = 1;

			var matchingBooking = Helper.CreateBooking();
			var nonMatchingBooking = Helper.CreateBooking();

			matchingBooking.KM_TransportMode = masterBooking.KM_TransportMode = "ROA";
			nonMatchingBooking.KM_TransportMode = "RAI";

			Factory.Save();

			var subBookings = new DtbBooking[] { matchingBooking, nonMatchingBooking };
			subBookings.ForEach(b => b.Validation.ValidateAll());
			AssertNoErrors(matchingBooking);
			AssertNoErrors(nonMatchingBooking);

			masterBooking.SubBookings.AddRange(subBookings);
			subBookings.ForEach(b => b.Validation.ValidateAll());
			AssertNoErrors("Should accept the sub with matching Booking Transport Mode", matchingBooking);
			AssertHasError("Should say the sub booking is invalid because Booking Transport Mode does not match the master", nonMatchingBooking.KM_TransportModeInfo, "This Booking has a different transport mode to the Master Booking.");

			masterBooking.SubBookings.RemoveFromRelationship(nonMatchingBooking);
			Factory.Save();

			subBookings.ForEach(b => b.Validation.ValidateAll());
			AssertNoErrors("Should not raise validation error when already attached", matchingBooking);

			masterBooking.KM_TransportMode = "RAI";
			subBookings.ForEach(b => b.Validation.ValidateAll());
			AssertNoErrors("Should not raise validation error when master changes", matchingBooking);
		}

		public void TestValidateTotalCO2eForBinding()
		{
			var booking = Helper.CreateBooking();
			AssertNoWarnings("Precondition", booking.TotalCO2eForBindingInfo);

			booking.SetCO2eStatus(CO2eStatusList.Codes.NotCurrent);
			AssertHasWarning("Warning when status is NotCurrent", booking.TotalCO2eForBindingInfo, "Greenhouse gas emissions recalculation is required because the input data has changed.");

			booking.SetCO2eStatus(CO2eStatusList.Codes.Rejected);
			AssertHasWarning("Warning when status is Rejected", booking.TotalCO2eForBindingInfo, "The greenhouse gas emissions value could not be calculated.");

			booking.SetCO2eStatus(CO2eStatusList.Codes.Current);
			AssertNoWarnings("No warnings when status is Current", booking.TotalCO2eForBindingInfo);
		}

		public void TestValidateTotalCO2eForSorting()
		{
			var booking = Helper.CreateBooking();
			AssertNoWarnings("Precondition", booking.TotalCO2eForSortingInfo);

			booking.SetCO2eStatus(CO2eStatusList.Codes.NotCurrent);
			AssertHasWarning("Warning when status is NotCurrent", booking.TotalCO2eForSortingInfo, "Greenhouse gas emissions recalculation is required because the input data has changed.");

			booking.SetCO2eStatus(CO2eStatusList.Codes.Rejected);
			AssertHasWarning("Warning when status is Rejected", booking.TotalCO2eForSortingInfo, "The greenhouse gas emissions value could not be calculated.");

			booking.SetCO2eStatus(CO2eStatusList.Codes.Current);
			AssertNoWarnings("No warnings when status is Current", booking.TotalCO2eForSortingInfo);
		}

		public void TestValidateWhenSendingXUSToCTO_WhenValidationPasses()
		{
			var booking = Helper.CreateBookingThatWillPassValidationForSendingXUSToCTO();
			EnsureIsSendingXUSToCTOReturnsTrue(booking);

			Factory.Save();

			booking.KM_Status = TransportStatuses.Codes.ActionRequired;

			booking.Validation.ValidateAll();
			var notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			AssertEquals("Should have no errors.", 0, notificationBufferErrorAndMessageErrorMessages.Count());
			AssertEquals("The validation process should not have made any changes to KM_Status.", TransportStatuses.Codes.ActionRequired, booking.KM_Status);
		}

		public void TestValidateWhenSendingXUSToCTO_NotificationBufferForSendingXUSToCTOShouldNotIncludeOldErrors()
		{
			var booking = Helper.CreateBookingThatWillPassValidationForSendingXUSToCTO();

			EnsureIsSendingXUSToCTOReturnsTrue(booking);

			booking.Address.OrganisationPK = GlbCompany.CurrentCompany.OrgProxy.PK;

			Factory.Save();

			booking.NotificationBufferForSendingXUSToCTO.AddError("Error");
			booking.NotificationBufferForSendingXUSToCTO.AddMessageError("Message Error");

			var notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);
			AssertEquals($"Precondition: {nameof(DtbBooking.NotificationBufferForSendingXUSToCTO)} should have 2 errors.", 2, notificationBufferErrorAndMessageErrorMessages.Count());

			booking.Validation.ValidateAll();
			notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			AssertEquals("Should only have one error due to having a non-null Transport Company address.", 1, notificationBufferErrorAndMessageErrorMessages.Count());

			booking.Address.Delete();

			Factory.Save();

			booking.Validation.ValidateAll();
			notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			AssertEquals("Should now have no errors", 0, notificationBufferErrorAndMessageErrorMessages.Count());
		}

		public void TestValidateWhenSendingXUSToCTO_FailingValidationDoesNotChangeBookingStatus()
		{
			var consol = Factory.New<IForwardingConsol>() as IDtbBookingParent;
			var booking = Helper.CreateBookingThatWillPassValidationForSendingXUSToCTO(consol);

			EnsureIsSendingXUSToCTOReturnsTrue(booking);

			Factory.Save();

			booking.KM_Status = TransportStatuses.Codes.Available;

			booking.Validation.ValidateAll();
			var notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			AssertGreaterThan("Should have at least one error.", notificationBufferErrorAndMessageErrorMessages.Count(), 0);
			AssertEquals("The validation process should not have made any changes to KM_Status.", TransportStatuses.Codes.Available, booking.KM_Status);
		}

		public void TestValidateWhenSendingXUSToCTO_ValidateBookingHasCorrectInstructionTypes()
		{
			var booking = Helper.CreateBookingThatWillPassValidationForSendingXUSToCTO();
			booking.ConsolidationSingleJob.KB_JobDirection = nameof(DtbBookingDirection.PIC);

			EnsureIsSendingXUSToCTOReturnsTrue(booking);

			var pickupInstruction = booking.Instructions.Single(i => i.KN_InstructionType == InstructionTypes.Codes.PickUp);
			var multiInstruction = booking.Instructions.Single(i => i.KN_InstructionType == InstructionTypes.Codes.Multi);
			var deliveryInstruction = booking.Instructions.Single(i => i.KN_InstructionType == InstructionTypes.Codes.Delivery);

			pickupInstruction.OrganisationType = OrganisationTypesList.Codes.CYD;
			multiInstruction.OrganisationType = OrganisationTypesList.Codes.CNR;
			deliveryInstruction.OrganisationType = OrganisationTypesList.Codes.CTO;

			Factory.Save();

			booking.Validation.ValidateAll();
			var notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			AssertEquals("Should have no errors for Booking with Consolidation with 'PIC' direction.", 0, notificationBufferErrorAndMessageErrorMessages.Count());

			pickupInstruction.OrganisationType = OrganisationTypesList.Codes.MSC;

			booking.Validation.ValidateAll();
			notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			CombineAssertions(() =>
			{
				AssertStringContainsExpectedInstructionTypeError("Instruction Type: PIC, Organization Type: CYD", notificationBufferErrorAndMessageErrorMessages.SingleOrDefault());
				AssertEquals("There should only be 1 instruction type error, so there should be 2 line breaks.", 2, Regex.Matches(notificationBufferErrorAndMessageErrorMessages.SingleOrDefault(), "\r\n").Count);
			});

			pickupInstruction.OrganisationType = OrganisationTypesList.Codes.CYD;
			multiInstruction.OrganisationType = OrganisationTypesList.Codes.MSC;

			booking.Validation.ValidateAll();
			notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			CombineAssertions(() =>
			{
				AssertStringContainsExpectedInstructionTypeError("Instruction Type: MLT, Organization Type: CNR", notificationBufferErrorAndMessageErrorMessages.SingleOrDefault());
				AssertEquals("There should only be 1 instruction type error, so there should be 2 line breaks.", 2, Regex.Matches(notificationBufferErrorAndMessageErrorMessages.SingleOrDefault(), "\r\n").Count);
			});

			multiInstruction.OrganisationType = OrganisationTypesList.Codes.CNR;
			deliveryInstruction.OrganisationType = OrganisationTypesList.Codes.MSC;

			booking.Validation.ValidateAll();
			notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			CombineAssertions(() =>
			{
				AssertStringContainsExpectedInstructionTypeError("Instruction Type: DLV, Organization Type: CTO", notificationBufferErrorAndMessageErrorMessages.SingleOrDefault());
				AssertEquals("There should only be 1 instruction type error, so there should be 2 line breaks.", 2, Regex.Matches(notificationBufferErrorAndMessageErrorMessages.SingleOrDefault(), "\r\n").Count);
			});

			pickupInstruction.OrganisationType = OrganisationTypesList.Codes.MSC;
			multiInstruction.OrganisationType = OrganisationTypesList.Codes.MSC;

			booking.Validation.ValidateAll();
			notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			CombineAssertions(() =>
			{
				AssertStringContainsExpectedInstructionTypeError("Instruction Type: PIC, Organization Type: CYD", notificationBufferErrorAndMessageErrorMessages.SingleOrDefault());
				AssertStringContainsExpectedInstructionTypeError("Instruction Type: MLT, Organization Type: CNR", notificationBufferErrorAndMessageErrorMessages.SingleOrDefault());
				AssertStringContainsExpectedInstructionTypeError("Instruction Type: DLV, Organization Type: CTO", notificationBufferErrorAndMessageErrorMessages.SingleOrDefault());
				AssertEquals("There should be 3 instruction type errors, so there should be 4 line breaks.", 4, Regex.Matches(notificationBufferErrorAndMessageErrorMessages.SingleOrDefault(), "\r\n").Count);
			});

			booking.ConsolidationSingleJob.KB_JobDirection = nameof(DtbBookingDirection.DLV);

			pickupInstruction.OrganisationType = OrganisationTypesList.Codes.CTO;
			multiInstruction.OrganisationType = OrganisationTypesList.Codes.CNE;
			deliveryInstruction.OrganisationType = OrganisationTypesList.Codes.CYD;

			Factory.Save();

			booking.Validation.ValidateAll();
			notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			AssertEquals("Should have no errors for Booking with Consolidation with 'DLV' direction.", 0, notificationBufferErrorAndMessageErrorMessages.Count());

			pickupInstruction.OrganisationType = OrganisationTypesList.Codes.MSC;

			booking.Validation.ValidateAll();
			notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			CombineAssertions(() =>
			{
				AssertStringContainsExpectedInstructionTypeError("Instruction Type: PIC, Organization Type: CTO", notificationBufferErrorAndMessageErrorMessages.SingleOrDefault());
				AssertEquals("There should only be 1 instruction type error, so there should be 2 line breaks.", 2, Regex.Matches(notificationBufferErrorAndMessageErrorMessages.SingleOrDefault(), "\r\n").Count);
			});

			pickupInstruction.OrganisationType = OrganisationTypesList.Codes.CTO;
			multiInstruction.OrganisationType = OrganisationTypesList.Codes.MSC;

			booking.Validation.ValidateAll();
			notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			CombineAssertions(() =>
			{
				AssertStringContainsExpectedInstructionTypeError("Instruction Type: MLT, Organization Type: CNE", notificationBufferErrorAndMessageErrorMessages.SingleOrDefault());
				AssertEquals("There should only be 1 instruction type error, so there should be 2 line breaks.", 2, Regex.Matches(notificationBufferErrorAndMessageErrorMessages.SingleOrDefault(), "\r\n").Count);
			});

			multiInstruction.OrganisationType = OrganisationTypesList.Codes.CNE;
			deliveryInstruction.OrganisationType = OrganisationTypesList.Codes.MSC;

			booking.Validation.ValidateAll();
			notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			CombineAssertions(() =>
			{
				AssertStringContainsExpectedInstructionTypeError("Instruction Type: DLV, Organization Type: CYD", notificationBufferErrorAndMessageErrorMessages.SingleOrDefault());
				AssertEquals("There should only be 1 instruction type error, so there should be 2 line breaks.", 2, Regex.Matches(notificationBufferErrorAndMessageErrorMessages.SingleOrDefault(), "\r\n").Count);
			});

			pickupInstruction.OrganisationType = OrganisationTypesList.Codes.MSC;
			multiInstruction.OrganisationType = OrganisationTypesList.Codes.MSC;

			booking.Validation.ValidateAll();
			notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			CombineAssertions(() =>
			{
				AssertStringContainsExpectedInstructionTypeError("Instruction Type: PIC, Organization Type: CTO", notificationBufferErrorAndMessageErrorMessages.SingleOrDefault());
				AssertStringContainsExpectedInstructionTypeError("Instruction Type: MLT, Organization Type: CNE", notificationBufferErrorAndMessageErrorMessages.SingleOrDefault());
				AssertStringContainsExpectedInstructionTypeError("Instruction Type: DLV, Organization Type: CYD", notificationBufferErrorAndMessageErrorMessages.SingleOrDefault());
				AssertEquals("There should be 3 instruction type errors, so there should be 4 line breaks.", 4, Regex.Matches(notificationBufferErrorAndMessageErrorMessages.SingleOrDefault(), "\r\n").Count);
			});
		}

		void AssertStringContainsExpectedInstructionTypeError(string expectedErrorMessage, string instructionTypeErrorMessagesString)
		{
			AssertStartsWith("Error message should start with the line about the booking requiring particular instruction types.", "Booking requires the following instruction types for sending to CTO:\r\n", instructionTypeErrorMessagesString);
			AssertContains(expectedErrorMessage, instructionTypeErrorMessagesString);
		}

		public void TestValidateWhenSendingXUSToCTO_BookingParentMustBeAShipment()
		{
			var consol = Factory.New<IForwardingConsol>() as IDtbBookingParent;
			var booking = Helper.CreateBookingThatWillPassValidationForSendingXUSToCTO(consol);

			EnsureIsSendingXUSToCTOReturnsTrue(booking);

			Factory.Save();

			booking.Validation.ValidateAll();

			var notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			AssertContains("Should contain the message for when the Booking Parent is not a Shipment", "The Booking must have a Shipment as a Parent.", notificationBufferErrorAndMessageErrorMessages.SingleOrDefault());
		}

		public void TestValidateWhenSendingXUSToCTO_TransportCompanyMustBeNull()
		{
			var booking = Helper.CreateBookingThatWillPassValidationForSendingXUSToCTO();

			booking.Address.OrganisationPK = GlbCompany.CurrentCompany.OrgProxy.PK;

			EnsureIsSendingXUSToCTOReturnsTrue(booking);

			Factory.Save();

			AssertNotNull("Precondition: Booking should have attached Transport Company", booking.Address.Organisation);

			booking.Validation.ValidateAll();
			var notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			CombineAssertions(() =>
			{
				AssertContains("Should contain message for when the booking has an assigned Transport Company", "The Booking must have no assigned Transport Company.", notificationBufferErrorAndMessageErrorMessages.SingleOrDefault());
				AssertHasMessageError("Transport Company organisation should have a message error for not being empty", booking.Address.OrganisationPKInfo, "The Booking must have no assigned Transport Company.");
			});
		}

		public void TestValidateWhenSendingXUSToCTO_TBParentTransportModeMustBeSEA()
		{
			var shipment = Factory.New<IForwardingShipment>();
			shipment.JS_TransportMode = "AAA";
			var booking = Helper.CreateBookingThatWillPassValidationForSendingXUSToCTO((IDtbBookingParent)shipment);

			EnsureIsSendingXUSToCTOReturnsTrue(booking);

			Factory.Save();

			AssertNotEquals("Precondition: Booking Parent should NOT have Transport Mode of SEA", booking.ConsolidationSingleJob.Parent.TransportMode, Constants.TransportModes.Sea);

			booking.Validation.ValidateAll();
			var notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			AssertContains("Should contain message for when the Booking Parent doesn't have a Transport Mode of SEA", "The Booking Parent must have a Transport Mode of SEA.", notificationBufferErrorAndMessageErrorMessages.SingleOrDefault());
		}

		public void TestValidateWhenSendingXUSToCTO_TBParentTransportModeValidation_WhenNoBookingParent_ShouldNotThrowException()
		{
			var booking = Helper.CreateBooking();
			var carrierBookingReferenceNumber = booking.AdditionalReferenceNumbers.AddNew();
			carrierBookingReferenceNumber.CE_EntryType = CargoWise.Definitions.TransportAdditionalReferenceTypes.Codes.CarrierBookingReference;
			carrierBookingReferenceNumber.CE_EntryNum = "Carrier booking reference 123";

			EnsureIsSendingXUSToCTOReturnsTrue(booking);

			Factory.Save();

			AssertEquals("Precondition: ConsolidationSingleJob does not have a booking parent", null, booking.ConsolidationSingleJob.Parent);

			AssertNoExceptionThrown("Should not throw exception when no booking parent is available", booking.Validation.ValidateAll);

			var notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			AssertContains("Should contain the message for when the Booking Parent is not a Shipment", "The Booking must have a Shipment as a Parent.", notificationBufferErrorAndMessageErrorMessages.SingleOrDefault());

			var parentJobShipment = Helper.CreateForwardingShipment();
			parentJobShipment.JS_TransportMode = Constants.TransportModes.Sea;
			booking.ConsolidationSingleJob.KB_ParentTableCode = ((BusinessObject)parentJobShipment).TablePrefix;
			booking.ConsolidationSingleJob.KB_ParentID = parentJobShipment.PK;

			Factory.Save();

			booking.Validation.ValidateAll();

			notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			AssertContainsExactElementsInAnyOrder("Should have no errors", Enumerable.Empty<string>(), notificationBufferErrorAndMessageErrorMessages);
		}

		public void TestValidateWhenSendingXUSToCTO_ContainerCodeNotBlank()
		{
			var booking = Helper.CreateBookingThatWillPassValidationForSendingXUSToCTO();

			var deliveryInstruction = booking.Instructions.First(i => i.KN_InstructionType == InstructionTypes.Codes.Delivery);

			var container = booking.PackageJob.Packages.AddNew();
			container.KP_F3_NKPackType = Constants.PkgUnit.Container;
			var seaContainerType = Factory.New<RefContainer>();
			container.Container.K0_RC_ContainerType = seaContainerType.PK;
			seaContainerType.RC_Description = "Valid Container Type Description";
			seaContainerType.RC_ContainerType = Enterprise.Core.Constants.ContainerTypes.DryStorage;

			seaContainerType.RC_DescriptionInfo.AdditionalValidation += TriggerValidationErrorOnRefContainerPropertyUnrelatedToSendingXUSToCTOWhenItIsValidated;

			Helper.CreatePackageDivot(deliveryInstruction, container);

			EnsureIsSendingXUSToCTOReturnsTrue(booking);

			Factory.Save();

			AssertEquals("Precondition: Container Code should be blank", ZString.Empty, seaContainerType.RC_Code);

			booking.Validation.ValidateAll();

			var notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			CombineAssertions(() =>
			{
				AssertContains("Should only contain the message for container code should not be blank", "Please enter a Container Code.", notificationBufferErrorAndMessageErrorMessages.SingleOrDefault());
				AssertHasError("RC_Code should have a non-message error for being empty", seaContainerType.RC_CodeInfo, "Please enter a Container Code.");
			});

			seaContainerType.RC_Code = "SEA1";

			Factory.Save();

			booking.Validation.ValidateAll();

			notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			AssertContainsExactElementsInAnyOrder("Should have no errors", Enumerable.Empty<string>(), notificationBufferErrorAndMessageErrorMessages);

			void TriggerValidationErrorOnRefContainerPropertyUnrelatedToSendingXUSToCTOWhenItIsValidated()
			{
				seaContainerType.RC_DescriptionInfo.AddError("This property is unrelated to sending to CTO and should not have been validated.");
			}
		}

		public void TestValidateWhenSendingXUSToCTO_ContainerCodeValidationShouldNotThrowException()
		{
			var booking = Helper.CreateBookingThatWillPassValidationForSendingXUSToCTO();

			var pickupInstruction = booking.Instructions.First(i => i.KN_InstructionType == InstructionTypes.Codes.PickUp);

			var nonContainerPackage = booking.PackageJob.Packages.AddNew();
			nonContainerPackage.KP_F3_NKPackType = Constants.PkgUnit.Package;
			Helper.CreatePackageDivot(pickupInstruction, nonContainerPackage);

			AssertEquals("Precondition: Package should not be a container.", false, nonContainerPackage.IsContainer);

			EnsureIsSendingXUSToCTOReturnsTrue(booking);

			Factory.Save();

			AssertNoExceptionThrown("Should not throw exception when running validation for container code", booking.Validation.ValidateAll);

			var notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			CombineAssertions(() =>
			{
				AssertContains("Should have errors as the package is not a container", "This package must be a container.", notificationBufferErrorAndMessageErrorMessages.SingleOrDefault());
				AssertCollectionNotContains("Should not have error for empty container code as the package is not a container", "At least one Container Code is not specified.", notificationBufferErrorAndMessageErrorMessages);
			});
		}

		public void TestValidateWhenSendingXUSToCTO_KK_RequiredFromNotBlank()
		{
			var booking = Helper.CreateBookingThatWillPassValidationForSendingXUSToCTO();

			var pickupInstruction = booking.Instructions.First(i => i.KN_InstructionType == InstructionTypes.Codes.PickUp);

			var additionalPickupConfirmation = Helper.CreateConfirmation(pickupInstruction, ConfirmationTypes.Codes.PickUp);
			additionalPickupConfirmation.KK_RequiredTo = new ZDateTime(2025, 2, 13);
			additionalPickupConfirmation.KK_ReferenceNum = "Ref123";

			additionalPickupConfirmation.Validation.ValidateAll();

			CombineAssertions(() =>
			{
				AssertEquals("Precondition: KK_RequiredFrom should be blank", ZDateTime.Empty, additionalPickupConfirmation.KK_RequiredFrom);
				AssertNoNotifications("Precondition: KK_RequiredFrom should not have any kind of notification as IsSendingXUSToCTO is not yet true on the booking.", additionalPickupConfirmation.KK_RequiredFromInfo);
			});

			EnsureIsSendingXUSToCTOReturnsTrue(booking);

			Factory.Save();

			booking.Validation.ValidateAll();

			var notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			CombineAssertions(() =>
			{
				AssertContains("Should contain the message for KK_RequiredFrom should not be blank", "This Booking must have Required From and Required To entered for all Confirmations.", notificationBufferErrorAndMessageErrorMessages.SingleOrDefault());
				AssertHasMessageError("Should have message error for KK_RequiredFrom", additionalPickupConfirmation.KK_RequiredFromInfo, "This Booking must have Required From and Required To entered for all Confirmations.");
			});

			additionalPickupConfirmation.KK_RequiredFrom = new ZDateTime(2025, 2, 11);

			Factory.Save();

			booking.Validation.ValidateAll();

			notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Should have no errors", Enumerable.Empty<string>(), notificationBufferErrorAndMessageErrorMessages);
				AssertNoNotifications("KK_RequiredFrom should not have any kind of notification", additionalPickupConfirmation.KK_RequiredFromInfo);
			});
		}

		public void TestValidateWhenSendingXUSToCTO_KK_RequiredToNotBlank()
		{
			var booking = Helper.CreateBookingThatWillPassValidationForSendingXUSToCTO();

			var pickupInstruction = booking.Instructions.First(i => i.KN_InstructionType == InstructionTypes.Codes.PickUp);

			var additionalPickupConfirmation = Helper.CreateConfirmation(pickupInstruction, ConfirmationTypes.Codes.PickUp);
			additionalPickupConfirmation.KK_RequiredFrom = new ZDateTime(2025, 2, 11);
			additionalPickupConfirmation.KK_ReferenceNum = "Ref123";
			additionalPickupConfirmation.Validation.ValidateAll();

			CombineAssertions(() =>
			{
				AssertEquals("Precondition: KK_RequiredTo should be blank", ZDateTime.Empty, additionalPickupConfirmation.KK_RequiredTo);
				AssertNoNotifications("Precondition: KK_RequiredTo should not have any kind of notification as IsSendingXUSToCTO is not yet true on the booking.", additionalPickupConfirmation.KK_RequiredToInfo);
			});

			EnsureIsSendingXUSToCTOReturnsTrue(booking);

			Factory.Save();

			booking.Validation.ValidateAll();

			var notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			CombineAssertions(() =>
			{
				AssertContains("Should contain the message for KK_RequiredTo should not be blank", "This Booking must have Required From and Required To entered for all Confirmations.", notificationBufferErrorAndMessageErrorMessages.SingleOrDefault());
				AssertHasMessageError("Should have message error for KK_RequiredTo", additionalPickupConfirmation.KK_RequiredToInfo, "This Booking must have Required From and Required To entered for all Confirmations.");
			});

			additionalPickupConfirmation.KK_RequiredTo = new ZDateTime(2025, 2, 13);

			Factory.Save();

			booking.Validation.ValidateAll();

			notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Should have no errors", Enumerable.Empty<string>(), notificationBufferErrorAndMessageErrorMessages);
				AssertNoNotifications("KK_RequiredTo should not have any kind of notification", additionalPickupConfirmation.KK_RequiredToInfo);
			});
		}

		public void TestValidateWhenSendingXUSToCTO_KK_RequiredFromLessThanKK_RequiredTo()
		{
			var booking = Helper.CreateBookingThatWillPassValidationForSendingXUSToCTO();

			var pickupInstruction = booking.Instructions.First(i => i.KN_InstructionType == InstructionTypes.Codes.PickUp);

			var additionalPickupConfirmation = Helper.CreateConfirmation(pickupInstruction, ConfirmationTypes.Codes.PickUp);
			additionalPickupConfirmation.KK_RequiredFrom = new ZDateTime(2025, 2, 11);
			additionalPickupConfirmation.KK_RequiredTo = new ZDateTime(2025, 2, 9);
			additionalPickupConfirmation.KK_ReferenceNum = "Ref123";

			additionalPickupConfirmation.Validation.ValidateAll();

			CombineAssertions(() =>
			{
				AssertHasError("Precondition: KK_RequiredFrom should have a non-message error", additionalPickupConfirmation.KK_RequiredFromInfo, "'Required From' needs to be earlier than 'Required To'.");
				AssertHasError("Precondition: KK_RequiredTo should have a non-message error", additionalPickupConfirmation.KK_RequiredToInfo, "'Required To' needs to be later than 'Required From'.");
			});

			EnsureIsSendingXUSToCTOReturnsTrue(booking);

			Factory.Save();

			booking.Validation.ValidateAll();

			var notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			CombineAssertions(() =>
			{
				AssertEquals("Should have 2 errors", 2, notificationBufferErrorAndMessageErrorMessages.Count());
				AssertContains("Should contain the message for KK_RequiredFrom needing to be earlier than KK_RequiredTo.", "'Required From' needs to be earlier than 'Required To'.", string.Join(", ", notificationBufferErrorAndMessageErrorMessages));
				AssertContains("Should contain the message for KK_RequiredTo needing to be later than KK_RequiredFrom.", "'Required To' needs to be later than 'Required From'.", string.Join(", ", notificationBufferErrorAndMessageErrorMessages));
			});

			additionalPickupConfirmation.KK_RequiredTo = new ZDateTime(2025, 2, 13);

			Factory.Save();

			booking.Validation.ValidateAll();

			notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Should have no errors", Enumerable.Empty<string>(), notificationBufferErrorAndMessageErrorMessages);
				AssertNoNotifications("KK_RequiredFrom should not have any kind of notification", additionalPickupConfirmation.KK_RequiredFromInfo);
				AssertNoNotifications("KK_RequiredTo should not have any kind of notification", additionalPickupConfirmation.KK_RequiredToInfo);
			});
		}

		public void TestValidateWhenSendingXUSToCTO_InstructionAddressesNotEmpty()
		{
			var booking = Helper.CreateBookingThatWillPassValidationForSendingXUSToCTO();
			var additionalInstruction = Helper.CreateInstructionForBookingThatWillPassValidationForSendingXUSToCTO(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, "CONT4444441");
			additionalInstruction.Address.Delete();

			AssertEquals("Precondition: Additional instruction should not have an address", true, additionalInstruction.Address.IsEmpty);

			EnsureIsSendingXUSToCTOReturnsTrue(booking);

			Factory.Save();

			booking.Validation.ValidateAll();

			var notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			AssertContains("Should contain the message for instruction address not empty", "This Booking must have valid Addresses entered for all Instructions.", notificationBufferErrorAndMessageErrorMessages.SingleOrDefault());

			var org = Factory.NewWithValidTestData<OrgHeader>();
			additionalInstruction.Address.E2_OA_Address = org.MainAddress.PK;
			additionalInstruction.Address.ValidationStatus = AddressValidationStatus.Verified;
			additionalInstruction.Address.E2_City = "City1";
			additionalInstruction.Address.Postcode = "1234";
			additionalInstruction.Address.E2_CompanyName = "Company1";
			additionalInstruction.Address.E2_State = "NSW";

			Factory.Save();

			booking.Validation.ValidateAll();

			notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			AssertContainsExactElementsInAnyOrder("Should have no errors", Enumerable.Empty<string>(), notificationBufferErrorAndMessageErrorMessages);
		}

		public void TestValidateWhenSendingXUSToCTO_InstructionAddressesValidated()
		{
			var booking = Helper.CreateBookingThatWillPassValidationForSendingXUSToCTO();
			var pickupInstruction = booking.Instructions.First(i => i.KN_InstructionType == InstructionTypes.Codes.PickUp);
			pickupInstruction.KN_DropMode = Constants.FCLEquipmentNeeded.WaitForUnpack;
			pickupInstruction.Address.ValidationStatus = AddressValidationStatus.Invalid;

			pickupInstruction.Address.Validation.ValidateAll();

			AssertNoNotifications("Precondition: Instruction address validation Status should not have any kind of notification as IsSendingXUSToCTO is not yet true on the booking.", pickupInstruction.Address.E2_ValidationStatusInfo);

			EnsureIsSendingXUSToCTOReturnsTrue(booking);

			Factory.Save();

			booking.Validation.ValidateAll();

			var notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			CombineAssertions(() =>
			{
				AssertContains("Should contain the message for instruction address validation status", "This Booking must have valid Addresses entered for all Instructions.", notificationBufferErrorAndMessageErrorMessages.SingleOrDefault());
				AssertHasMessageError("Should have message error for instruction address validation", pickupInstruction.Address.E2_ValidationStatusInfo, "This Booking must have valid Addresses entered for all Instructions.");
			});

			pickupInstruction.Address.ValidationStatus = AddressValidationStatus.Verified;

			Factory.Save();

			booking.Validation.ValidateAll();

			notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Should have no errors", Enumerable.Empty<string>(), notificationBufferErrorAndMessageErrorMessages);
				AssertNoNotifications("Instruction address validation status should not have any kind of notification", pickupInstruction.Address.E2_ValidationStatusInfo);
			});
		}

		public void TestValidateWhenSendingXUSToCTO_InstructionDropMode()
		{
			var booking = Helper.CreateBookingThatWillPassValidationForSendingXUSToCTO();
			var pickupInstruction = booking.Instructions.First(i => i.KN_InstructionType == InstructionTypes.Codes.PickUp);
			pickupInstruction.KN_DropMode = string.Empty;
			pickupInstruction.Address.ValidationStatus = AddressValidationStatus.Verified;
			booking.Validation.ValidateAll();
			pickupInstruction.Validation.ValidateAll();

			CombineAssertions(() =>
			{
				AssertEquals("Precondition: KN_DropMode should be blank", string.Empty, pickupInstruction.KN_DropMode);
				AssertNoNotifications("Precondition: Drop mode should not have any kind of notification as IsSendingXUSToCTO is not yet true on the booking.", pickupInstruction.KN_DropModeInfo);
			});

			EnsureIsSendingXUSToCTOReturnsTrue(booking);

			Factory.Save();

			booking.Validation.ValidateAll();

			var notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			CombineAssertions(() =>
			{
				AssertContains("Should contain the message for drop mode", "This Booking must have Drop Mode entered for all Instructions.", notificationBufferErrorAndMessageErrorMessages.SingleOrDefault());
				AssertHasMessageError("Should have message error for drop mode", pickupInstruction.KN_DropModeInfo, "This Booking must have Drop Mode entered for all Instructions.");
			});

			pickupInstruction.KN_DropMode = Constants.FCLEquipmentNeeded.WaitForUnpack;

			Factory.Save();

			booking.Validation.ValidateAll();

			notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Should have no errors", Enumerable.Empty<string>(), notificationBufferErrorAndMessageErrorMessages);
				AssertNoNotifications("Drop mode should not have any kind of notification", pickupInstruction.KN_DropModeInfo);
			});
		}

		public void TestValidateWhenSendingXUSToCTO_BookingInstructionsShouldHaveCorrectOrgType()
		{
			var booking = Helper.CreateBookingThatWillPassValidationForSendingXUSToCTO();

			var picInstruction = booking.Instructions.First(i => i.KN_InstructionType == InstructionTypes.Codes.PickUp);
			picInstruction.OrganisationType = OrganisationTypesList.Codes.MSC;
			var mltInstruction = booking.Instructions.First(i => i.KN_InstructionType == InstructionTypes.Codes.Multi);
			mltInstruction.OrganisationType = OrganisationTypesList.Codes.CNE;
			var dlvInstruction = booking.Instructions.First(i => i.KN_InstructionType == InstructionTypes.Codes.Delivery);
			dlvInstruction.OrganisationType = OrganisationTypesList.Codes.CYD;

			EnsureIsSendingXUSToCTOReturnsTrue(booking);

			Factory.Save();

			booking.Validation.ValidateAll();

			var notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			AssertCollectionContains("Should contain the message for when the Booking does not have correct instruction types", "Booking requires the following instruction types for sending to CTO:\r\nInstruction Type: PIC, Organization Type: CTO\r\n", notificationBufferErrorAndMessageErrorMessages);

			picInstruction.OrganisationType = OrganisationTypesList.Codes.CTO;

			Factory.Save();

			booking.Validation.ValidateAll();

			notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			AssertContainsExactElementsInAnyOrder("Should have no errors", Enumerable.Empty<string>(), notificationBufferErrorAndMessageErrorMessages);
		}

		public void TestValidateWhenSendingXUSToCTO_BookingInstructionsShouldOnlyHaveContainerPackages()
		{
			var booking = Helper.CreateBookingThatWillPassValidationForSendingXUSToCTO();

			var package = booking.ConsolidationSingleJob.PackageJob.Packages.AddNew();
			var picInstruction = booking.Instructions.First(i => i.KN_InstructionType == InstructionTypes.Codes.PickUp);
			picInstruction.DivotsWithPackages.AddPackage(package);
			package.KP_F3_NKPackType = Constants.PkgUnit.Package;

			EnsureIsSendingXUSToCTOReturnsTrue(booking);

			Factory.Save();

			booking.Validation.ValidateAll();

			var notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			AssertContains("Should contain the message for when the Booking Instruction does not have only containers", "This package must be a container.", notificationBufferErrorAndMessageErrorMessages.SingleOrDefault());

			package.KP_F3_NKPackType = Constants.PkgUnit.Container;
			var twentyGPO = Helper.LoadRefContainer("20GP");
			package.Container.K0_RC_ContainerType = twentyGPO.PK;
			package.KP_PackageID = "CONT4444441";

			Factory.Save();

			booking.Validation.ValidateAll();

			notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Should have no errors", Enumerable.Empty<string>(), notificationBufferErrorAndMessageErrorMessages);
				AssertNoNotifications("Pack Type should not have any kind of notification", package.KP_F3_NKPackTypeInfo);
			});
		}

		public void TestValidateWhenSendingXUSToCTO_BookingInstructionsContainersShouldHaveContainerNumberPopulated()
		{
			var booking = Helper.CreateBookingThatWillPassValidationForSendingXUSToCTO();

			var firstInstruction = booking.Instructions.First();
			var firstInstructionPackage = firstInstruction.GetPackages.SingleOrDefault();
			firstInstructionPackage.KP_PackageID = ZString.Empty;

			EnsureIsSendingXUSToCTOReturnsTrue(booking);

			Factory.Save();

			booking.Validation.ValidateAll();

			var notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			AssertContains("Should contain the message for when the Booking Instruction's container does not have a container number", "Container must have a container number.", notificationBufferErrorAndMessageErrorMessages.SingleOrDefault());

			firstInstructionPackage.KP_PackageID = "CONT4444441";

			Factory.Save();

			booking.Validation.ValidateAll();

			notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Should have no errors", Enumerable.Empty<string>(), notificationBufferErrorAndMessageErrorMessages);
				AssertNoNotifications("Package ID should not have any kind of notification", firstInstructionPackage.KP_PackageIDInfo);
			});
		}

		public void TestValidateWhenSendingXUSToCTO_ContainerNumber()
		{
			var booking = Helper.CreateBookingThatWillPassValidationForSendingXUSToCTO();
			var container1 = booking.ConsolidationSingleJob.PackageJob.Packages[0];
			container1.KP_PackageID = "F@KE4100011";
			var container2 = booking.ConsolidationSingleJob.PackageJob.Packages[1];
			container2.KP_PackageID = "FAKE4100017";

			container1.KP_GoodsDescriptionInfo.AdditionalValidation += () => TriggerValidationErrorOnPkgPackagePropertyUnrelatedToSendingXUSToCTOWhenItIsValidated(container1);
			container2.KP_GoodsDescriptionInfo.AdditionalValidation += () => TriggerValidationErrorOnPkgPackagePropertyUnrelatedToSendingXUSToCTOWhenItIsValidated(container2);

			CombineAssertions(() =>
			{
				AssertHasWarning("Should have warning about Invalid ISO Format as IsSendingXUSToCTO is false", container1.KP_PackageIDInfo, "Container number does not conform to ISO standard of 4 letters followed by 6 digits and a check digit.");
				AssertHasWarning("Should have warning about invalid check digit as IsSendingXUSToCTO is false", container2.KP_PackageIDInfo, "Container number does not have a valid check (last) digit. The check digit should be 1.");
			});

			EnsureIsSendingXUSToCTOReturnsTrue(booking);

			Factory.Save();

			booking.Validation.ValidateAll();

			var notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			CombineAssertions(() =>
			{
				AssertEquals("Should only have 2 errors", 2, notificationBufferErrorAndMessageErrorMessages.Count());
				AssertContains("Should contain the message for Invalid ISO Format", $"[Package {container1.KP_PackageID}] Package ID: Container number does not conform to ISO standard of 4 letters followed by 6 digits and a check digit.", string.Join(", ", notificationBufferErrorAndMessageErrorMessages));
				AssertContains("Should contain the message for invalid check digit", $"[Package {container2.KP_PackageID}] Package ID: Container number does not have a valid check (last) digit. The check digit should be 1.", string.Join(", ", notificationBufferErrorAndMessageErrorMessages));
				AssertHasMessageError("Should have message error about Invalid ISO Format as IsSendingXUSToCTO is true", container1.KP_PackageIDInfo, "Container number does not conform to ISO standard of 4 letters followed by 6 digits and a check digit.");
				AssertHasMessageError("Should have message error about invalid check digit as IsSendingXUSToCTO is true", container2.KP_PackageIDInfo, "Container number does not have a valid check (last) digit. The check digit should be 1.");
			});

			container1.KP_PackageID = "FAKE4100011";
			container2.KP_F3_NKPackType = Constants.PkgUnit.Package;

			Factory.Save();

			booking.Validation.ValidateAll();

			notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			CombineAssertions(() =>
			{
				foreach (var errorMessage in notificationBufferErrorAndMessageErrorMessages)
				{
					AssertContains("All errors should be that a package is not a container.", "This package must be a container", errorMessage);
				}
				AssertNoNotifications("Container number should not have any kind of notification, as it is valid", container1.KP_PackageIDInfo);
				AssertNoNotifications("Container number should not have any kind of notification, as it is not a container", container2.KP_PackageIDInfo);
			});

			container2.KP_F3_NKPackType = Constants.PkgUnit.Container;
			container2.Container.K0_RC_ContainerType = container1.Container.K0_RC_ContainerType;
			container2.KP_PackageID = "FAKE4100027";

			Factory.Save();

			booking.Validation.ValidateAll();

			notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Should have no errors", Enumerable.Empty<string>(), notificationBufferErrorAndMessageErrorMessages);
				AssertNoNotifications("Container number should not have any kind of notification, as it remained valid", container1.KP_PackageIDInfo);
				AssertNoNotifications("Container number should not have any kind of notification, as it is now valid", container2.KP_PackageIDInfo);
			});

			void TriggerValidationErrorOnPkgPackagePropertyUnrelatedToSendingXUSToCTOWhenItIsValidated(PkgPackage container)
			{
				container.KP_GoodsDescriptionInfo.AddError("This property is unrelated to sending to CTO and should not have been validated.");
			}
		}

		public void TestValidateWhenSendingXUSToCTO_ContainerNumber_ContainerNotAssignedToBooking()
		{
			var booking = Helper.CreateBookingThatWillPassValidationForSendingXUSToCTO();

			var invalidContainer = booking.ConsolidationSingleJob.PackageJob.Packages[0];
			invalidContainer.KP_PackageID = "F@KE4100011";
			booking.Instructions.ForEach(i => i.PackageDivots.DeleteAll());

			AssertHasWarning("Should have warning about Invalid ISO Format as IsSendingXUSToCTO is false", invalidContainer.KP_PackageIDInfo, "Container number does not conform to ISO standard of 4 letters followed by 6 digits and a check digit.");

			EnsureIsSendingXUSToCTOReturnsTrue(booking);

			Factory.Save();

			using (invalidContainer.SuspendValidationTesting())
			{
				invalidContainer.KP_PackageIDInfo.ClearAllNotifications();
			}

			booking.Validation.ValidateAll();

			var notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Should have no errors, as it doesn't check containers that aren't assigned to an instruction associated with the booking", Enumerable.Empty<string>(), notificationBufferErrorAndMessageErrorMessages);
				AssertNoNotifications("Container number should not have any kind of notification, as its validation isn't called", invalidContainer.KP_PackageIDInfo);
			});

			foreach (var instruction in booking.Instructions)
			{
				var packageDivot = instruction.PackageDivots.AddNew();
				packageDivot.KD_KP_Package = invalidContainer.PK;
				packageDivot.KD_Quantity = invalidContainer.KP_PackageQty;
			}

			Factory.Save();

			booking.Validation.ValidateAll();

			notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			CombineAssertions(() =>
			{
				AssertContains("Should contain the message for Invalid ISO Format", "Container number does not conform to ISO standard of 4 letters followed by 6 digits and a check digit.", notificationBufferErrorAndMessageErrorMessages.SingleOrDefault());
				AssertHasMessageError("Should have message error about Invalid ISO Format as IsSendingXUSToCTO is true", invalidContainer.KP_PackageIDInfo, "Container number does not conform to ISO standard of 4 letters followed by 6 digits and a check digit.");
			});
		}

		public void TestValidateWhenSendingXUSToCTO_KK_ReferenceNum()
		{
			var booking = Helper.CreateBookingThatWillPassValidationForSendingXUSToCTO();
			var consolidation = booking.ConsolidationSingleJob;

			consolidation.KB_JobDirection = nameof(DtbBookingDirection.PIC);

			var pickupInstruction = booking.Instructions.Single(i => i.KN_InstructionType == InstructionTypes.Codes.PickUp);
			var multiInstruction = booking.Instructions.Single(i => i.KN_InstructionType == InstructionTypes.Codes.Multi);
			var deliveryInstruction = booking.Instructions.Single(i => i.KN_InstructionType == InstructionTypes.Codes.Delivery);

			pickupInstruction.OrganisationType = OrganisationTypesList.Codes.CYD;
			multiInstruction.OrganisationType = OrganisationTypesList.Codes.CNR;
			deliveryInstruction.OrganisationType = OrganisationTypesList.Codes.CTO;

			var pickupConfirmation = pickupInstruction.Confirmations.SingleOrDefault();
			pickupConfirmation.KK_ReferenceNum = ZString.Empty;

			AssertNoNotifications("Precondition: Confirmation Reference Number should not have any kind of notification as IsSendingXUSToCTO is not yet true on the booking.", pickupConfirmation.KK_ReferenceNumInfo);

			EnsureIsSendingXUSToCTOReturnsTrue(booking);

			Factory.Save();

			booking.Validation.ValidateAll();

			var notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			CombineAssertions(() =>
			{
				AssertContains("Should contain the message for Confirmation Reference Number", "Confirmation must have Reference Number", notificationBufferErrorAndMessageErrorMessages.SingleOrDefault());
				AssertHasMessageError("Should have message error about empty KK_ReferenceNum", pickupConfirmation.KK_ReferenceNumInfo, "Confirmation must have Reference Number");
			});

			pickupConfirmation.KK_ReferenceNum = "Ref123";

			Factory.Save();

			booking.Validation.ValidateAll();

			notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Should have no notification buffer error or message error messages", Enumerable.Empty<string>(), notificationBufferErrorAndMessageErrorMessages);
				AssertNoNotifications("Should not have any property notifications when KK_ReferenceNum is not empty", pickupConfirmation.KK_ReferenceNumInfo);
			});
		}

		public void TestValidateWhenSendingXUSToCTO_CarrierBookingReference_NoAdditionalReferenceNumber()
		{
			var booking = Helper.CreateBookingThatWillPassValidationForSendingXUSToCTO();
			booking.AdditionalReferenceNumbers.Delete();

			EnsureIsSendingXUSToCTOReturnsTrue(booking);

			Factory.Save();

			booking.Validation.ValidateAll();

			var notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			AssertCollectionContains("Should contain the message for Carrier Booking Reference", "Carrier Booking Reference has not been entered.", notificationBufferErrorAndMessageErrorMessages);
		}

		public void TestValidateWhenSendingXUSToCTO_CarrierBookingReference_ValidAdditionalReferenceNumber()
		{
			var booking = Helper.CreateBookingThatWillPassValidationForSendingXUSToCTO();
			booking.AdditionalReferenceNumbers.Delete();
			var additionalReferenceNumber = booking.AdditionalReferenceNumbers.AddNew();
			additionalReferenceNumber.CE_EntryType = CargoWise.Definitions.TransportAdditionalReferenceTypes.Codes.CarrierBookingReference;
			additionalReferenceNumber.CE_EntryNum = "Carrier booking reference 123";

			EnsureIsSendingXUSToCTOReturnsTrue(booking);

			Factory.Save();

			booking.Validation.ValidateAll();

			var notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			AssertContainsExactElementsInAnyOrder("Should have no notification buffer error or message error messages", Enumerable.Empty<string>(), notificationBufferErrorAndMessageErrorMessages);
		}

		public void TestValidateWhenSendingXUSToCTO_CarrierBookingReference_AdditionalReferenceNumberOnConsolidation()
		{
			var booking = Helper.CreateBookingThatWillPassValidationForSendingXUSToCTO();
			booking.AdditionalReferenceNumbers.Delete();
			var additionalReferenceNumber = booking.ConsolidationSingleJob.AdditionalReferenceNumbers.AddNew();
			additionalReferenceNumber.CE_EntryType = CargoWise.Definitions.TransportAdditionalReferenceTypes.Codes.CarrierBookingReference;
			additionalReferenceNumber.CE_EntryNum = "Carrier booking reference 456";

			EnsureIsSendingXUSToCTOReturnsTrue(booking);

			Factory.Save();

			booking.Validation.ValidateAll();

			var notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			AssertContainsExactElementsInAnyOrder("Should have no notification buffer error or message error messages, even if the Additional Reference Number is on the consolidation", Enumerable.Empty<string>(), notificationBufferErrorAndMessageErrorMessages);
		}

		public void TestValidateWhenSendingXUSToCTO_CarrierBookingReference_EmptyCE_EntryNum()
		{
			var booking = Helper.CreateBookingThatWillPassValidationForSendingXUSToCTO();
			booking.AdditionalReferenceNumbers.Delete();
			var additionalReferenceNumber = booking.AdditionalReferenceNumbers.AddNew();
			additionalReferenceNumber.CE_EntryType = CargoWise.Definitions.TransportAdditionalReferenceTypes.Codes.CarrierBookingReference;
			additionalReferenceNumber.CE_EntryNum = string.Empty;

			EnsureIsSendingXUSToCTOReturnsTrue(booking);

			Factory.Save();

			booking.Validation.ValidateAll();

			var notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			AssertCollectionContains("Should contain the message for Carrier Booking Reference", "Carrier Booking Reference has not been entered.", notificationBufferErrorAndMessageErrorMessages);
		}

		public void TestValidateWhenSendingXUSToCTO_CarrierBookingReference_IncorrectCE_EntryType()
		{
			var booking = Helper.CreateBookingThatWillPassValidationForSendingXUSToCTO();
			booking.AdditionalReferenceNumbers.Delete();
			var additionalReferenceNumber = booking.AdditionalReferenceNumbers.AddNew();
			additionalReferenceNumber.CE_EntryNum = "Commercial invoice number 123";
			additionalReferenceNumber.CE_EntryType = CargoWise.Definitions.TransportAdditionalReferenceTypes.Codes.CommercialInvoiceNumber;

			EnsureIsSendingXUSToCTOReturnsTrue(booking);

			Factory.Save();

			booking.Validation.ValidateAll();

			var notificationBufferErrorAndMessageErrorMessages = GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(booking);

			AssertCollectionContains("Should contain the message for Carrier Booking Reference", "Carrier Booking Reference has not been entered.", notificationBufferErrorAndMessageErrorMessages);
		}

		void EnsureIsSendingXUSToCTOReturnsTrue(DtbBooking booking)
		{
			if (!booking.IsSendingXUSToCTO)
			{
				booking.IsValidatingSendingXUSToCTO = true;
			}

			AssertEquals("Precondition: IsSendingXUSToCTO should return true for the given booking.", true, booking.IsSendingXUSToCTO);
		}

		IEnumerable<string> GetErrorAndMessageErrorMessagesFromNotificationBufferForSendingXUSToCTOAsListOfStrings(DtbBooking booking)
		{
			var messageErrors = booking.NotificationBufferForSendingXUSToCTO.Events.GetMessageErrors().Select(e => e.Message);
			var errors = booking.NotificationBufferForSendingXUSToCTO.Events.GetErrors().Select(e => e.Message);
			return messageErrors.Concat(errors);
		}

		DtbBooking GetNewBooking()
		{
			return Helper.CreateBooking();
		}

		PkgPackageJob GetPackageJob(DtbBooking booking)
		{
			return booking.ConsolidationSingleJob.PackageJob;
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}

		PackingTestHelper PackingHelper
		{
			get { return packingHelper ?? (packingHelper = new PackingTestHelper(Factory)); }
		}

		TransportBookingTestHelper helper;
		PackingTestHelper packingHelper;
	}
}

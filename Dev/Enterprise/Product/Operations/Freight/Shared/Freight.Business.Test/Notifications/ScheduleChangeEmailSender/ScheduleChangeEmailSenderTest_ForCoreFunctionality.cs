using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ScheduleChangeEmailSenderTest_ForCoreFunctionality : ScheduleChangeEmailSenderTestBase
	{
		[ExpectNoExceptions]
		[TestDate(2013, 1, 1)]
		public void TestSendEmailIfRequired_WhenNotificationGroupNotConfigured()
		{
			NotificationGroupRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			CreateNewScheduleAndMakeChanges(true);
			Factory.Save();
			ChangeEmailSender.SendEmailIfRequired(Factory, ZDateTime.Empty, ZDateTime.Empty, Notifications);
		}

		[ExpectNoExceptions]
		[TestDate(2013, 1, 1)]
		public void TestSendEmailIfRequired_DontBlowUpIfNoHeaderOrFooterImageSpecified()
		{
			SystemDataRegistry.Instance.HtmlEmailBannerImage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, null);
			SystemDataRegistry.Instance.HtmlEmailFooterImage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, null);

			JobsForTesting.ImportConsol.JK_UniqueConsignRef = "C00005001";
			JobsForTesting.ExportConsol.JK_UniqueConsignRef = "C00005002";
			JobsForTesting.ImportConsol.Transports[0].JW_JX = Voyage1.Sailings[0].PK;
			JobsForTesting.ExportConsol.Transports[0].JW_JX = Voyage1.Sailings[0].PK;

			JobsForTesting.BookingShipment.UniqueConsignRef = "S00005001";
			JobsForTesting.BookingShipment.SailingJX = Voyage1.Sailings[0].PK;
			Factory.Save();

			ResetEmailTemplate();
			CreateNewScheduleAndMakeChanges(true);
			Factory.Save();
			ChangeEmailSender.SendEmailIfRequired(Factory, ZDateTime.Empty, ZDateTime.Empty, Notifications);
		}

		[ExpectNoExceptions]
		[TestDate(2013, 1, 1)]
		public void TestSendEmailIfRequired_DontBlowUpIfStaffDeleted()
		{
			SystemDataRegistry.Instance.HtmlEmailBannerImage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, null);
			SystemDataRegistry.Instance.HtmlEmailFooterImage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, null);

			JobsForTesting.ImportConsol.JK_UniqueConsignRef = "C00005001";
			JobsForTesting.ExportConsol.JK_UniqueConsignRef = "C00005002";
			JobsForTesting.ImportConsol.Transports[0].JW_JX = Voyage1.Sailings[0].PK;
			JobsForTesting.ExportConsol.Transports[0].JW_JX = Voyage1.Sailings[0].PK;

			JobsForTesting.BookingShipment.UniqueConsignRef = "S00005001";
			JobsForTesting.BookingShipment.SailingJX = Voyage1.Sailings[0].PK;
			Factory.Save();

			ResetEmailTemplate();
			CreateNewScheduleAndMakeChanges(true);
			Factory.Save();
			GlbStaff[] allStaff = Factory.Load<GlbStaff>(new ZQuery());
			foreach (GlbStaff staff in new List<GlbStaff>(allStaff))
			{
				staff.Delete();
			}
			ChangeEmailSender.SendEmailIfRequired(Factory, ZDateTime.Empty, ZDateTime.Empty, Notifications);
		}

		[TestDate(2013, 1, 1)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSendEmailIfRequired_HtmlFormatting()
		{
			Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK).GS_FullName = "Developer";
			JobsForTesting.ImportConsol.JK_UniqueConsignRef = "C00005001";
			JobsForTesting.ExportConsol.JK_UniqueConsignRef = "C00005002";
			JobsForTesting.ImportConsol.Transports[0].JW_JX = Voyage1.Sailings[0].PK;
			JobsForTesting.ExportConsol.Transports[0].JW_JX = Voyage1.Sailings[0].PK;

			JobsForTesting.BookingShipment.UniqueConsignRef = "S00005001";
			JobsForTesting.BookingShipment.SailingJX = Voyage1.Sailings[0].PK;
			Factory.Save();

			ResetEmailTemplate();
			CreateNewScheduleAndMakeChanges(true);
			Factory.Save();

			ChangeEmailSender.SendEmailIfRequired(Factory, ZDateTime.Empty, new ZDateTime(2013, 2, 2), Notifications);

			var expectedHtml = new StringBuilder(File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Freight\Shared\Freight.Business\Notifications\ScheduleChangeEmailSender\Testing\ExpectedScheduleChangeEmail.htm"));
			expectedHtml.Replace("(*StyleSheet*)", SystemDataRegistry.Instance.HtmlEmailStyleSheet.Value);
			expectedHtml.Replace("(*ImportConsolUrl*)", ShowEditFormUrlCreator.Create(ControllerIDs.JobConsol, JobsForTesting.ImportConsol.PK.ToGuid()));
			expectedHtml.Replace("(*ExportConsolUrl*)", ShowEditFormUrlCreator.Create(ControllerIDs.JobConsol, JobsForTesting.ExportConsol.PK.ToGuid()));
			expectedHtml.Replace("(*BookingShipmentUrl*)", ShowEditFormUrlCreator.Create(ControllerIDs.QuotedBookings, JobsForTesting.BookingShipment.ViewPK.ToGuid()));

			AssertMultilineASCIIEquals("Html content should be correct", expectedHtml.ToString(), Env.OutgoingMailManager.EmailsCreated[0].Body);
		}

		[TestDate(2013, 1, 1)]
		public void TestSendEmailIfRequired_IncludeImportConsolJobsAffected()
		{
			JobsForTesting.ImportConsol.JK_UniqueConsignRef = "C00005001";
			JobsForTesting.ExportConsol.JK_UniqueConsignRef = "C00005002";
			JobsForTesting.ImportConsol.Transports[0].JW_JX = ImportSailing.PK;
			JobsForTesting.ExportConsol.Transports[0].JW_JX = ImportSailing.PK;
			Factory.Save();

			ImportSailing.Destination.JB_A_ARV = new ZDateTime(2013, 1, 11);
			Factory.Save();

			SeaScheduleChangeEmailSender changeEmailSender = new SeaScheduleChangeEmailSender();
			changeEmailSender.SendEmailIfRequired(Factory, ZDateTime.Empty, new ZDateTime(2013, 1, 1, 0, 1, 0), Notifications);
			AssertEmailSent("Email should include Import Consols affected by arrival date change", "Sailing Schedules have changed",
@"Sailing Schedule changes (first time run) to 01-Jan-13 00:01 UTC

Load     Discharge  Field              Old Value       Updated Value
====================================================================
Changed By: " + GlbStaff.CurrentUser.GS_FullName + @"
Vessel: Vessel
Voyage: Voyage
Carrier: Carrier
---
         AUSYD      ATA                04-Jan-13 00:00 11-Jan-13 00:00

The following Consolidation (and related shipment) jobs are affected:
C00005001 (" + ShowEditFormUrlCreator.Create(ControllerIDs.JobConsol, JobsForTesting.ImportConsol.PK.ToGuid()) + @") 

Schedule change notifications");
		}

		[TestDate(2013, 1, 1)]
		public void TestSendEmailIfRequired_IncludeExportConsolJobsAffected()
		{
			JobsForTesting.ImportConsol.JK_UniqueConsignRef = "C00005001";
			JobsForTesting.ExportConsol.JK_UniqueConsignRef = "C00005002";
			JobsForTesting.ImportConsol.Transports[0].JW_JX = ExportSailing.PK;
			JobsForTesting.ExportConsol.Transports[0].JW_JX = ExportSailing.PK;
			Factory.Save();

			ExportSailing.Origin.JA_A_DEP = new ZDateTime(2013, 1, 11);
			Factory.Save();

			SeaScheduleChangeEmailSender changeEmailSender = new SeaScheduleChangeEmailSender();
			changeEmailSender.SendEmailIfRequired(Factory, ZDateTime.Empty, new ZDateTime(2013, 1, 1, 0, 1, 0), Notifications);
			AssertEmailSent("Email should include Export Consols affected by departure date change", "Sailing Schedules have changed",
@"Sailing Schedule changes (first time run) to 01-Jan-13 00:01 UTC

Load     Discharge  Field              Old Value       Updated Value
====================================================================
Changed By: " + GlbStaff.CurrentUser.GS_FullName + @"
Vessel: Vessel
Voyage: Voyage
Carrier: Carrier
---
AUSYD               ATD                02-Jan-13 00:00 11-Jan-13 00:00

The following Consolidation (and related shipment) jobs are affected:
C00005002 (" + ShowEditFormUrlCreator.Create(ControllerIDs.JobConsol, JobsForTesting.ExportConsol.PK.ToGuid()) + @") 

Schedule change notifications");
		}

		[TestDate(2013, 1, 1)]
		public void TestSendEmailIfRequired_IncludeDeclarationJobsAffected_Export_JA_E_DEP()
		{
			TestSendEmailIfRequired_IncludeDeclarationJobsAffected(JobVoyOriginSchema.JA_E_DEP, "AUSYD               ETD                01-Jan-13 00:00 11-Jan-13 00:00", false);
		}

		[TestDate(2013, 1, 1)]
		public void TestSendEmailIfRequired_IncludeDeclarationJobsAffected_Export_JA_A_DEP()
		{
			TestSendEmailIfRequired_IncludeDeclarationJobsAffected(JobVoyOriginSchema.JA_A_DEP, "AUSYD               ATD                02-Jan-13 00:00 11-Jan-13 00:00", false);
		}

		[TestDate(2013, 1, 1)]
		public void TestSendEmailIfRequired_IncludeDeclarationJobsAffected_Export_JA_CutOff()
		{
			TestSendEmailIfRequired_IncludeDeclarationJobsAffected(JobVoyOriginSchema.JA_CutOff, "AUSYD               Cargo Cut Off      (empty)         11-Jan-13 00:00", false);
		}

		[TestDate(2013, 1, 1)]
		public void TestSendEmailIfRequired_IncludeDeclarationJobsAffected_Export_JA_ReceivalCommences()
		{
			TestSendEmailIfRequired_IncludeDeclarationJobsAffected(JobVoyOriginSchema.JA_ReceivalCommences, "AUSYD               Receival Commences (empty)         11-Jan-13 00:00", false);
		}

		[TestDate(2013, 1, 1)]
		public void TestSendEmailIfRequired_IncludeDeclarationJobsAffected_Import_JB_E_ARV()
		{
			TestSendEmailIfRequired_IncludeDeclarationJobsAffected(JobVoyDestinationSchema.JB_E_ARV, "         AUSYD      ETA                03-Jan-13 00:00 11-Jan-13 00:00", true);
		}

		[TestDate(2013, 1, 1)]
		public void TestSendEmailIfRequired_IncludeDeclarationJobsAffected_Import_JB_A_ARV()
		{
			TestSendEmailIfRequired_IncludeDeclarationJobsAffected(JobVoyDestinationSchema.JB_A_ARV, "         AUSYD      ATA                04-Jan-13 00:00 11-Jan-13 00:00", true);
		}

		[TestDate(2013, 1, 1)]
		public void TestSendEmailIfRequired_IncludeDeclarationJobsAffected_Import_JB_AvailabilityDate()
		{
			TestSendEmailIfRequired_IncludeDeclarationJobsAffected(JobVoyDestinationSchema.JB_AvailabilityDate, "         AUSYD      Availability       (empty)         11-Jan-13 00:00", true);
		}

		[TestDate(2013, 1, 1)]
		public void TestSendEmailIfRequired_IncludeDeclarationJobsAffected_Import_JB_StorageDate()
		{
			TestSendEmailIfRequired_IncludeDeclarationJobsAffected(JobVoyDestinationSchema.JB_StorageDate, "         AUSYD      Storage            (empty)         11-Jan-13 00:00", true);
		}

		void TestSendEmailIfRequired_IncludeDeclarationJobsAffected(SchemaDateTimeColumn originOrDestinationDate, string dateChangedContent, bool isImport)
		{
			FreightConfigurationRegistry.Instance.AutoDeliverDelayAlertDocuments.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DelayAlertDeliveryRuleCollection());

			JobSailing sailing = isImport ? ImportSailing : ExportSailing;
			sailing.Voyage.JV_AirSeaRoad = TransportMode;

			BusinessObject declaration = isImport ? JobsForTesting.ImportDeclaration : JobsForTesting.ExportDeclaration;
			JobsForTesting.ImportDeclaration[JobDeclarationSchema.JE_TransportMode] = TransportMode;
			JobsForTesting.ExportDeclaration[JobDeclarationSchema.JE_TransportMode] = TransportMode;
			JobsForTesting.ImportDeclaration[JobDeclarationSchema.JE_DeclarationReference] = "B00005002";
			JobsForTesting.ExportDeclaration[JobDeclarationSchema.JE_DeclarationReference] = "B00005002";
			declaration[JobDeclarationSchema.JE_DeclarationReference] = "B00005001";

			JobsForTesting.ImportDeclaration[JobDeclarationSchema.JE_VesselName] = Voyage.JV_RV_NKVessel;
			JobsForTesting.ExportDeclaration[JobDeclarationSchema.JE_VesselName] = Voyage.JV_RV_NKVessel;
			JobsForTesting.ImportDeclaration[JobDeclarationSchema.JE_VoyageFlightNo] = Voyage.JV_VoyageFlight;
			JobsForTesting.ExportDeclaration[JobDeclarationSchema.JE_VoyageFlightNo] = Voyage.JV_VoyageFlight;
			JobsForTesting.ImportDeclaration[JobDeclarationSchema.JE_OH_ShippingLine] = Voyage.JV_OH_Line;
			JobsForTesting.ExportDeclaration[JobDeclarationSchema.JE_OH_ShippingLine] = Voyage.JV_OH_Line;

			JobsForTesting.ImportDeclaration[JobDeclarationSchema.JE_RL_NKOrigin] = ZString.Empty;
			JobsForTesting.ImportDeclaration[JobDeclarationSchema.JE_RL_NKPortOfLoading] = ZString.Empty;
			JobsForTesting.ImportDeclaration[JobDeclarationSchema.JE_RL_NKFinalDestination] = ZString.Empty;
			JobsForTesting.ImportDeclaration[JobDeclarationSchema.JE_RL_NKPortOfArrival] = ZString.Empty;
			JobsForTesting.ImportDeclaration[JobDeclarationSchema.JE_RL_NKPortOfFirstArrival] = ZString.Empty;
			JobsForTesting.ExportDeclaration[JobDeclarationSchema.JE_RL_NKOrigin] = ZString.Empty;
			JobsForTesting.ExportDeclaration[JobDeclarationSchema.JE_RL_NKPortOfLoading] = ZString.Empty;
			JobsForTesting.ExportDeclaration[JobDeclarationSchema.JE_RL_NKFinalDestination] = ZString.Empty;
			JobsForTesting.ExportDeclaration[JobDeclarationSchema.JE_RL_NKPortOfArrival] = ZString.Empty;
			JobsForTesting.ExportDeclaration[JobDeclarationSchema.JE_RL_NKPortOfFirstArrival] = ZString.Empty;

			JobsForTesting.ImportDeclaration[JobDeclarationSchema.JE_RL_NKPortOfLoading] = sailing.Origin.JA_RL_NKPortOfLoading;
			JobsForTesting.ExportDeclaration[JobDeclarationSchema.JE_RL_NKPortOfLoading] = sailing.Origin.JA_RL_NKPortOfLoading;
			JobsForTesting.ImportDeclaration[JobDeclarationSchema.JE_RL_NKPortOfArrival] = sailing.Destination.JB_RL_NKPortOfDischarge;
			JobsForTesting.ExportDeclaration[JobDeclarationSchema.JE_RL_NKPortOfArrival] = sailing.Destination.JB_RL_NKPortOfDischarge;
			Factory.Save();

			SetDateOnSailing(sailing, originOrDestinationDate);
			Factory.Save();

			SeaScheduleChangeEmailSender changeEmailSender = new SeaScheduleChangeEmailSender();
			changeEmailSender.SendEmailIfRequired(Factory, ZDateTime.Empty, new ZDateTime(2013, 1, 1, 0, 1, 0), Notifications);
			AssertEmailSent("Email should include Declaration jobs affected by arrival/departure date change", "Sailing Schedules have changed",
@"Sailing Schedule changes (first time run) to 01-Jan-13 00:01 UTC

Load     Discharge  Field              Old Value       Updated Value
====================================================================
Changed By: " + GlbStaff.CurrentUser.GS_FullName + @"
Vessel: Vessel
Voyage: Voyage
Carrier: Carrier
---
" + dateChangedContent + @"

The following Customs Declaration jobs are affected:
B00005001 (" + ShowEditFormUrlCreator.Create(ControllerIDs.Customs.JobDeclaration, declaration.PK.ToGuid()) + @") 

Schedule change notifications");
		}

		[TestDate(2013, 1, 1)]
		public void TestSendEmailIfRequired_IncludeImportLoadListJobsAffected()
		{
			JobsForTesting.ImportLoadList.JK_UniqueConsignRef = "L00005001";
			JobsForTesting.ExportLoadList.JK_UniqueConsignRef = "L00005002";
			JobsForTesting.ImportLoadList.Transports[0].JW_JX = ImportSailing.PK;
			JobsForTesting.ExportLoadList.Transports[0].JW_JX = ImportSailing.PK;
			Factory.Save();

			ImportSailing.Destination.JB_A_ARV = new ZDateTime(2013, 1, 11);
			Factory.Save();

			SeaScheduleChangeEmailSender changeEmailSender = new SeaScheduleChangeEmailSender();
			changeEmailSender.SendEmailIfRequired(Factory, ZDateTime.Empty, new ZDateTime(2013, 1, 1, 0, 1, 0), Notifications);
			AssertEmailSent("Email should include Import Load Lists affected by arrival date change", "Sailing Schedules have changed",
@"Sailing Schedule changes (first time run) to 01-Jan-13 00:01 UTC

Load     Discharge  Field              Old Value       Updated Value
====================================================================
Changed By: " + GlbStaff.CurrentUser.GS_FullName + @"
Vessel: Vessel
Voyage: Voyage
Carrier: Carrier
---
         AUSYD      ATA                04-Jan-13 00:00 11-Jan-13 00:00

The following CFS Load List jobs are affected:
L00005001 (" + ShowEditFormUrlCreator.Create(ControllerIDs.LoadListConsol, JobsForTesting.ImportLoadList.PK.ToGuid()) + @") 

Schedule change notifications");
		}

		[TestDate(2013, 1, 1)]
		public void TestSendEmailIfRequired_IncludeExportLoadListJobsAffected()
		{
			JobsForTesting.ImportLoadList.JK_UniqueConsignRef = "L00005001";
			JobsForTesting.ExportLoadList.JK_UniqueConsignRef = "L00005002";
			JobsForTesting.ImportLoadList.Transports[0].JW_JX = ExportSailing.PK;
			JobsForTesting.ExportLoadList.Transports[0].JW_JX = ExportSailing.PK;
			Factory.Save();

			ExportSailing.Origin.JA_A_DEP = new ZDateTime(2013, 1, 11);
			Factory.Save();

			SeaScheduleChangeEmailSender changeEmailSender = new SeaScheduleChangeEmailSender();
			changeEmailSender.SendEmailIfRequired(Factory, ZDateTime.Empty, new ZDateTime(2013, 1, 1, 0, 1, 0), Notifications);
			AssertEmailSent("Email should include Export Load Lists affected by departure date change", "Sailing Schedules have changed",
@"Sailing Schedule changes (first time run) to 01-Jan-13 00:01 UTC

Load     Discharge  Field              Old Value       Updated Value
====================================================================
Changed By: " + GlbStaff.CurrentUser.GS_FullName + @"
Vessel: Vessel
Voyage: Voyage
Carrier: Carrier
---
AUSYD               ATD                02-Jan-13 00:00 11-Jan-13 00:00

The following CFS Load List jobs are affected:
L00005002 (" + ShowEditFormUrlCreator.Create(ControllerIDs.LoadListConsol, JobsForTesting.ExportLoadList.PK.ToGuid()) + @") 

Schedule change notifications");
		}

		[TestDate(2013, 1, 1)]
		public void TestSendEmailIfRequired_IncludeExportBookingJobsAffected()
		{
			JobsForTesting.BookingShipment.SailingJX = ExportSailing.PK;
			JobsForTesting.BookingShipment.UniqueConsignRef = "S00005001";
			Factory.Save();

			ExportSailing.Destination.JB_A_ARV = new ZDateTime(2013, 1, 11);
			Factory.Save();

			ChangeEmailSender.SendEmailIfRequired(Factory, ZDateTime.Empty, new ZDateTime(2013, 1, 1, 0, 1, 0), Notifications);
			AssertEmailSent("No Booking jobs affected unless departure dates are changed", "Sailing Schedules have changed",
@"Sailing Schedule changes (first time run) to 01-Jan-13 00:01 UTC

Load     Discharge  Field              Old Value       Updated Value
====================================================================
Changed By: " + GlbStaff.CurrentUser.GS_FullName + @"
Vessel: Vessel
Voyage: Voyage
Carrier: Carrier
---
         MYPKG      ATA                04-Jan-13 00:00 11-Jan-13 00:00

Schedule change notifications");
			Env.OutgoingMailManager.EmailsCreated.Clear();

			ExportSailing.Origin.JA_A_DEP = new ZDateTime(2013, 1, 11);
			Factory.Save();

			ChangeEmailSender.SendEmailIfRequired(Factory, ZDateTime.Empty, new ZDateTime(2013, 1, 1, 0, 1, 0), Notifications);
			AssertEmailSent("Email should include Booking jobs affected by departure date change", "Sailing Schedules have changed",
@"Sailing Schedule changes (first time run) to 01-Jan-13 00:01 UTC

Load     Discharge  Field              Old Value       Updated Value
====================================================================
Changed By: " + GlbStaff.CurrentUser.GS_FullName + @"
Vessel: Vessel
Voyage: Voyage
Carrier: Carrier
---
         MYPKG      ATA                04-Jan-13 00:00 11-Jan-13 00:00
AUSYD               ATD                02-Jan-13 00:00 11-Jan-13 00:00

The following Booking jobs are affected:
S00005001 (" + ShowEditFormUrlCreator.Create(ControllerIDs.QuotedBookings, JobsForTesting.BookingShipment.ViewPK.ToGuid()) + @") 

Schedule change notifications");
		}

		[TestDate(2013, 1, 1)]
		public void TestSendEmailIfRequired_IncludeImportAgencyBookingJobsAffected()
		{
			JobsForTesting.ImportAgencyBooking.JS_UniqueConsignRef = "V00005001";
			JobsForTesting.ImportAgencyBooking.JS_JX = ImportSailing.PK;
			Factory.Save();

			ImportSailing.Destination.JB_A_ARV = new ZDateTime(2013, 1, 11);
			Factory.Save();

			ChangeEmailSender.SendEmailIfRequired(Factory, ZDateTime.Empty, new ZDateTime(2013, 1, 1, 0, 1, 0), Notifications);
			AssertEmailSent("Email should include Shipping Booking jobs affected by departure date change", "Sailing Schedules have changed",
@"Sailing Schedule changes (first time run) to 01-Jan-13 00:01 UTC

Load     Discharge  Field              Old Value       Updated Value
====================================================================
Changed By: " + GlbStaff.CurrentUser.GS_FullName + @"
Vessel: Vessel
Voyage: Voyage
Carrier: Carrier
---
         AUSYD      ATA                04-Jan-13 00:00 11-Jan-13 00:00

The following Shipping Booking jobs are affected:
V00005001 (" + ShowEditFormUrlCreator.Create(ControllerIDs.AgencyBooking, JobsForTesting.ImportAgencyBooking.PK.ToGuid()) + @") 

Schedule change notifications");
		}

		[TestDate(2013, 1, 1)]
		public void TestSendEmailIfRequired_IncludeExportAgencyBookingJobsAffected()
		{
			JobsForTesting.ExportAgencyBooking.JS_UniqueConsignRef = "V00005001";
			JobsForTesting.ExportAgencyBooking.JS_JX = ExportSailing.PK;
			Factory.Save();

			ExportSailing.Origin.JA_A_DEP = new ZDateTime(2013, 1, 11);
			Factory.Save();

			ChangeEmailSender.SendEmailIfRequired(Factory, ZDateTime.Empty, new ZDateTime(2013, 1, 1, 0, 1, 0), Notifications);
			AssertEmailSent("Email should include Shipping Booking jobs affected by departure date change", "Sailing Schedules have changed",
@"Sailing Schedule changes (first time run) to 01-Jan-13 00:01 UTC

Load     Discharge  Field              Old Value       Updated Value
====================================================================
Changed By: " + GlbStaff.CurrentUser.GS_FullName + @"
Vessel: Vessel
Voyage: Voyage
Carrier: Carrier
---
AUSYD               ATD                02-Jan-13 00:00 11-Jan-13 00:00

The following Shipping Booking jobs are affected:
V00005001 (" + ShowEditFormUrlCreator.Create(ControllerIDs.AgencyBooking, JobsForTesting.ExportAgencyBooking.PK.ToGuid()) + @") 

Schedule change notifications");
		}

		[TestDate(2013, 1, 1)]
		public void TestSendEmailIfRequired_IncludeImportAgencyDocumentationJobsAffected()
		{
			JobsForTesting.ImportAgencyDocumentation.JS_UniqueConsignRef = "V00005001";
			JobsForTesting.ExportAgencyDocumentation.JS_UniqueConsignRef = "V00005002";
			JobsForTesting.ImportAgencyDocumentation.JS_JX = ImportSailing.PK;
			JobsForTesting.ExportAgencyDocumentation.JS_JX = ImportSailing.PK;
			Factory.Save();

			ImportSailing.Destination.JB_A_ARV = new ZDateTime(2013, 1, 11);
			Factory.Save();

			SeaScheduleChangeEmailSender changeEmailSender = new SeaScheduleChangeEmailSender();
			changeEmailSender.SendEmailIfRequired(Factory, ZDateTime.Empty, new ZDateTime(2013, 1, 1, 0, 1, 0), Notifications);
			AssertEmailSent("Email should include Import Shipping Bill of Lading jobs affected by arrival date change", "Sailing Schedules have changed",
@"Sailing Schedule changes (first time run) to 01-Jan-13 00:01 UTC

Load     Discharge  Field              Old Value       Updated Value
====================================================================
Changed By: " + GlbStaff.CurrentUser.GS_FullName + @"
Vessel: Vessel
Voyage: Voyage
Carrier: Carrier
---
         AUSYD      ATA                04-Jan-13 00:00 11-Jan-13 00:00

The following Shipping Bill of Lading jobs are affected:
V00005001 (" + ShowEditFormUrlCreator.Create(ControllerIDs.AgencyBillOfLading, JobsForTesting.ImportAgencyDocumentation.PK.ToGuid()) + @") 

Schedule change notifications");
		}

		[TestDate(2013, 1, 1)]
		public void TestSendEmailIfRequired_IncludeExportAgencyDocumentationJobsAffected()
		{
			JobsForTesting.ImportAgencyDocumentation.JS_UniqueConsignRef = "V00005001";
			JobsForTesting.ExportAgencyDocumentation.JS_UniqueConsignRef = "V00005002";
			JobsForTesting.ImportAgencyDocumentation.JS_JX = ExportSailing.PK;
			JobsForTesting.ExportAgencyDocumentation.JS_JX = ExportSailing.PK;
			Factory.Save();

			ExportSailing.Origin.JA_A_DEP = new ZDateTime(2013, 1, 11);
			Factory.Save();

			SeaScheduleChangeEmailSender changeEmailSender = new SeaScheduleChangeEmailSender();
			changeEmailSender.SendEmailIfRequired(Factory, ZDateTime.Empty, new ZDateTime(2013, 1, 1, 0, 1, 0), Notifications);
			AssertEmailSent("Email should include Export Shipping Bill of Lading jobs affected by departure date change", "Sailing Schedules have changed",
@"Sailing Schedule changes (first time run) to 01-Jan-13 00:01 UTC

Load     Discharge  Field              Old Value       Updated Value
====================================================================
Changed By: " + GlbStaff.CurrentUser.GS_FullName + @"
Vessel: Vessel
Voyage: Voyage
Carrier: Carrier
---
AUSYD               ATD                02-Jan-13 00:00 11-Jan-13 00:00

The following Shipping Bill of Lading jobs are affected:
V00005002 (" + ShowEditFormUrlCreator.Create(ControllerIDs.AgencyBillOfLading, JobsForTesting.ExportAgencyDocumentation.PK.ToGuid()) + @") 

Schedule change notifications");
		}

		[TestDate(2013, 1, 1)]
		public void TestSendEmailIfRequired_IncludeImportOrderJobsAffected()
		{
			JobsForTesting.Order[JobOrderHeaderSchema.JD_OrderNumber] = "P00005001";
			JobsForTesting.OrderLineDeliverContainer[JobOrderLineDeliverContainerSchema.J5_RV_NKArrivalVessel] = Voyage.JV_RV_NKVessel;
			JobsForTesting.OrderLineDeliverContainer[JobOrderLineDeliverContainerSchema.J5_Voyage] = Voyage.JV_VoyageFlight;
			JobsForTesting.OrderLineDelivery[JobOrderLineDeliverySchema.J4_RL_NKDestinationPort] = ImportSailing.Destination.JB_RL_NKPortOfDischarge;
			Factory.Save();

			ImportSailing.Destination.JB_A_ARV = new ZDateTime(2013, 1, 11);
			Factory.Save();

			SeaScheduleChangeEmailSender changeEmailSender = new SeaScheduleChangeEmailSender();
			changeEmailSender.SendEmailIfRequired(Factory, ZDateTime.Empty, new ZDateTime(2013, 1, 1, 0, 1, 0), Notifications);
			AssertEmailSent("Email should include Order jobs affected by arrival date change", "Sailing Schedules have changed",
@"Sailing Schedule changes (first time run) to 01-Jan-13 00:01 UTC

Load     Discharge  Field              Old Value       Updated Value
====================================================================
Changed By: " + GlbStaff.CurrentUser.GS_FullName + @"
Vessel: Vessel
Voyage: Voyage
Carrier: Carrier
---
         AUSYD      ATA                04-Jan-13 00:00 11-Jan-13 00:00

The following Order jobs are affected:
P00005001 (" + ShowEditFormUrlCreator.Create(ControllerIDs.Orders, JobsForTesting.Order.PK.ToGuid()) + @") 

Schedule change notifications");
		}

		[TestDate(2013, 1, 1)]
		public void TestSendEmailIfRequired_IncludeExportOrderJobsAffected()
		{
			JobsForTesting.Order[JobOrderHeaderSchema.JD_OrderNumber] = "P00005002";
			JobsForTesting.OrderLineDeliverContainer[JobOrderLineDeliverContainerSchema.J5_RV_NKArrivalVessel] = Voyage.JV_RV_NKVessel;
			JobsForTesting.OrderLineDeliverContainer[JobOrderLineDeliverContainerSchema.J5_Voyage] = Voyage.JV_VoyageFlight;
			JobsForTesting.OrderLineDeliverContainer[JobOrderLineDeliverContainerSchema.J5_RL_NKLoadPort] = ExportSailing.Origin.JA_RL_NKPortOfLoading;
			Factory.Save();

			ExportSailing.Origin.JA_A_DEP = new ZDateTime(2013, 1, 11);
			Factory.Save();

			SeaScheduleChangeEmailSender changeEmailSender = new SeaScheduleChangeEmailSender();
			changeEmailSender.SendEmailIfRequired(Factory, ZDateTime.Empty, new ZDateTime(2013, 1, 1, 0, 1, 0), Notifications);
			AssertEmailSent("Email should include Order jobs affected by departure date change", "Sailing Schedules have changed",
@"Sailing Schedule changes (first time run) to 01-Jan-13 00:01 UTC

Load     Discharge  Field              Old Value       Updated Value
====================================================================
Changed By: " + GlbStaff.CurrentUser.GS_FullName + @"
Vessel: Vessel
Voyage: Voyage
Carrier: Carrier
---
AUSYD               ATD                02-Jan-13 00:00 11-Jan-13 00:00

The following Order jobs are affected:
P00005002 (" + ShowEditFormUrlCreator.Create(ControllerIDs.Orders, JobsForTesting.Order.PK.ToGuid()) + @") 

Schedule change notifications");
		}

		[TestDate(2018, 1, 2)]
		public void TestDelayAlertDeliveryStatus_WhenDeliveryInstructionsIncomplete()
		{
			JobsForTesting.ImportConsol.JK_UniqueConsignRef = "C00000100";
			JobsForTesting.ImportConsol.Transports[0].JW_JX = ImportSailing.PK;
			CommonShipment importShipment = JobsForTesting.ImportConsol.Shipments.AddNew();
			importShipment.JS_UniqueConsignRef = "S00000100";
			importShipment.ConsigneePK = DelayAlertRecipient.PK;

			OrgContact contact = DelayAlertRecipient.Contacts.AddNew();
			contact.OC_Email = ""; // incomplete delivery instructions
			DelayAlertRecipient.MainAddress.OA_Email = ""; // incomplete delivery instructions
			OrgDocument document = contact.Documents.AddNew();
			document.OD_DeliverBy = Core.Constants.ContactNotifyModes.Email;
			document.OD_SU_MenuItem = ShipmentDelayAlertDocument.PK;

			ImportSailing.Destination.JB_E_ARV = ZDateTime.Now;
			Factory.Save();
			ImportSailing.Destination.JB_E_ARV = ZDateTime.Now.AddDays(1);
			Factory.Save();
			AssertDocumentNotDelivered(DelayAlertRecipient);

			SeaScheduleChangeEmailSender changeEmailSender = new SeaScheduleChangeEmailSender();
			changeEmailSender.SendEmailIfRequired(Factory, ZDateTime.Empty, new ZDateTime(2018, 1, 2, 0, 1, 0), Notifications);
			AssertEmailSent("Email should include Import Consols affected by arrival date change", "Sailing Schedules have changed",
@"Sailing Schedule changes (first time run) to 02-Jan-18 00:01 UTC
<strong><span style=""color: #FF0000"">!! Delay alert document failed to be delivered to one or more clients. !!</span></strong><br>
Check the event log of each job, or try to deliver the document manually.<br><br>
Load     Discharge  Field              Old Value       Updated Value
====================================================================
Changed By: " + GlbStaff.CurrentUser.GS_FullName + @"
Vessel: Vessel
Voyage: Voyage
Carrier: Carrier
---
         AUSYD      ETA                02-Jan-18 00:00 03-Jan-18 00:00

The following Consolidation (and related shipment) jobs are affected:
C00000100 (" + ShowEditFormUrlCreator.Create(ControllerIDs.JobConsol, JobsForTesting.ImportConsol.PK.ToGuid()) + @") 
&nbsp;&nbsp;&#3S00000100 (" + ShowEditFormUrlCreator.Create(ControllerIDs.JobShipment, importShipment.PK.ToGuid()) + @") - <strong><span style=""color: #FF0000"">delay alert failed to be delivered (Insufficient delay alert delivery information: Error: DeliveryAddress: Please enter an Email Address.)</span></strong>

Schedule change notifications");
		}

		[TestDate(2018, 1, 2)]
		public void TestDelayAlertDeliveryStatus_WhenDeliveryFailed()
		{
			JobsForTesting.ImportConsol.JK_UniqueConsignRef = "C00000100";
			JobsForTesting.ImportConsol.Transports[0].JW_JX = ImportSailing.PK;
			CommonShipment importShipment = JobsForTesting.ImportConsol.Shipments.AddNew();
			importShipment.JS_UniqueConsignRef = "S00000100";
			importShipment.ConsigneePK = DelayAlertRecipient.PK;

			ImportSailing.Destination.JB_E_ARV = ZDateTime.Now;
			Factory.Save();
			ImportSailing.Destination.JB_E_ARV = ZDateTime.Now.AddDays(1);
			Factory.Save();
			AssertDocumentDelivered(DelayAlertRecipient);

			SeaScheduleChangeEmailSender changeEmailSender = new SeaScheduleChangeEmailSender();
			changeEmailSender.SendEmailIfRequired(Factory, ZDateTime.Empty, new ZDateTime(2018, 1, 2, 0, 1, 0), Notifications);
			AssertEmailSent("Email should include Import Consols affected by arrival date change", "Sailing Schedules have changed",
@"Sailing Schedule changes (first time run) to 02-Jan-18 00:01 UTC
<strong><span style=""color: #FF0000"">!! Delay alert document failed to be delivered to one or more clients. !!</span></strong><br>
Check the event log of each job, or try to deliver the document manually.<br><br>
Load     Discharge  Field              Old Value       Updated Value
====================================================================
Changed By: " + GlbStaff.CurrentUser.GS_FullName + @"
Vessel: Vessel
Voyage: Voyage
Carrier: Carrier
---
         AUSYD      ETA                02-Jan-18 00:00 03-Jan-18 00:00

The following Consolidation (and related shipment) jobs are affected:
C00000100 (" + ShowEditFormUrlCreator.Create(ControllerIDs.JobConsol, JobsForTesting.ImportConsol.PK.ToGuid()) + @") 
&nbsp;&nbsp;&#3S00000100 (" + ShowEditFormUrlCreator.Create(ControllerIDs.JobShipment, importShipment.PK.ToGuid()) + @") - <strong><span style=""color: #FF0000"">delay alert failed to be delivered</span></strong>

Schedule change notifications");
		}

		[TestDate(2018, 1, 2)]
		public void TestDelayAlertDeliveryStatus_WhenDeliveryCancelledByUser()
		{
			Factory.LoadFromNaturalKey<StmEvent>(StmEventSchema.SE_Code, Events.DocumentNotDeliveredCode).SE_PropagateToParent = false;

			JobsForTesting.ImportConsol.JK_UniqueConsignRef = "C00000100";
			JobsForTesting.ImportConsol.Transports[0].JW_JX = ImportSailing.PK;
			CommonShipment importShipment = JobsForTesting.ImportConsol.Shipments.AddNew();
			importShipment.JS_UniqueConsignRef = "S00000100";
			importShipment.ConsigneePK = DelayAlertRecipient.PK;

			ImportSailing.Destination.JB_E_ARV = ZDateTime.Now;
			Factory.Save();
			ImportSailing.Destination.JB_E_ARV = ZDateTime.Now.AddDays(1);

			var queryProvider = new Mock<IScheduleUpdateQueryProvider>();
			ScheduleUpdateQueryProviderFactory.Set(Factory, () => queryProvider.Object);
			queryProvider.Setup(m => m.ShouldSendDelayAlerts(true, false)).Returns(false);
			Factory.Save();
			AssertDocumentNotDelivered(DelayAlertRecipient);

			SeaScheduleChangeEmailSender changeEmailSender = new SeaScheduleChangeEmailSender();
			changeEmailSender.SendEmailIfRequired(Factory, ZDateTime.Empty, new ZDateTime(2018, 1, 2, 0, 1, 0), Notifications);
			AssertEmailSent("Email should include Import Consols affected by arrival date change", "Sailing Schedules have changed",
@"Sailing Schedule changes (first time run) to 02-Jan-18 00:01 UTC

Load     Discharge  Field              Old Value       Updated Value
====================================================================
Changed By: " + GlbStaff.CurrentUser.GS_FullName + @"
Vessel: Vessel
Voyage: Voyage
Carrier: Carrier
---
         AUSYD      ETA                02-Jan-18 00:00 03-Jan-18 00:00

The following Consolidation (and related shipment) jobs are affected:
C00000100 (" + ShowEditFormUrlCreator.Create(ControllerIDs.JobConsol, JobsForTesting.ImportConsol.PK.ToGuid()) + @") 
&nbsp;&nbsp;&#3S00000100 (" + ShowEditFormUrlCreator.Create(ControllerIDs.JobShipment, importShipment.PK.ToGuid()) + @") - " + "delay alert delivery was canceled at the user's request" + @"

Schedule change notifications");

			queryProvider.VerifyAll();
		}

		[TestDate(2013, 11, 28)]
		public void TestHowtoUnsubscribeText()
		{
			JobsForTesting.ExportConsol.JK_UniqueConsignRef = "C00005002";
			JobsForTesting.ExportConsol.Transports[0].JW_JX = ImportSailing.PK;
			Factory.Save();

			ImportSailing.Destination.JB_A_ARV = new ZDateTime(2013, 12, 11);
			Factory.Save();

			var changeEmailSender = new SeaScheduleChangeEmailSender();
			changeEmailSender.SendEmailIfRequired(Factory, ZDateTime.Empty, new ZDateTime(2013, 11, 28, 0, 1, 0), Notifications);

			var expectedTextToUnsubscribe = @$"Unless otherwise configured, this email is delivered to all staff members.
You can configure this email to be delivered to a predefined group of users in the {Core.Constants.ProductName} registry.

Go to Maintain->System->Registry to change the registry item at:
Freight->Notifications->Sailing Schedules";

			AssertEquals("Expected text when HowToUnsubscribe.txt referenced", expectedTextToUnsubscribe, Encoding.ASCII.GetString(Env.OutgoingMailManager.EmailsCreated[0].Attachments[2].Data));
		}

		#region Implementation

		void SetDateOnSailing(JobSailing sailing, SchemaDateTimeColumn dateProperty)
		{
			if (dateProperty.TableName == JobVoyOriginSchema.Constants.TableName)
			{
				sailing.Origin[dateProperty] = new ZDateTime(2013, 1, 11);
			}
			else if (dateProperty.TableName == JobVoyDestinationSchema.Constants.TableName)
			{
				sailing.Destination[dateProperty] = new ZDateTime(2013, 1, 11);
			}
			else if (dateProperty.TableName == JobSailingSchema.Constants.TableName)
			{
				sailing[dateProperty] = new ZDateTime(2013, 1, 11);
			}
		}

		void AssertDocumentNotDelivered(OrgHeader recipient)
		{
			AssertDocumentNotDelivered("Document should NOT be delivered", recipient);
		}

		void AssertDocumentNotDelivered(string message, OrgHeader recipient)
		{
			var query = new ZDBOnlyQuery(typeof(StmPrintJob));
			query.AddToFilter(StmPrintJobSchema.SP_RunDateTime, SQLComparisonOperator.GreaterThan, ZDateTime.Now.AddSeconds(-10));
			var subQuery = new ZDBOnlySubQuery(typeof(StmPrintJobCopyRecipient), StmPrintJobCopyRecipientSchema.SPR_SP);
			subQuery.AddToFilter(StmPrintJobCopyRecipientSchema.SPR_RecipientType, SQLComparisonOperator.Equal, @"TO");
			subQuery.AddToFilter(StmPrintJobCopyRecipientSchema.SPR_EmailAddress, SQLComparisonOperator.Equal, recipient.MainAddress.OA_Email);
			query.AddSubQuery(subQuery, JoinCondition.And);

			var printJob = Factory.LoadTop1<StmPrintJob>(query);
			AssertNull("Document should NOT be delivered", printJob);
		}

		void AssertDocumentDelivered(OrgHeader recipient)
		{
			AssertDocumentDelivered("Document should be delivered", recipient);
		}

		void AssertDocumentDelivered(string message, OrgHeader recipient)
		{
			var query = new ZDBOnlyQuery(typeof(StmPrintJob));
			query.AddToFilter(StmPrintJobSchema.SP_RunDateTime, SQLComparisonOperator.GreaterThan, ZDateTime.Now.AddSeconds(-10));
			var subQuery = new ZDBOnlySubQuery(typeof(StmPrintJobCopyRecipient), StmPrintJobCopyRecipientSchema.SPR_SP);
			subQuery.AddToFilter(StmPrintJobCopyRecipientSchema.SPR_RecipientType, SQLComparisonOperator.Equal, @"TO");
			subQuery.AddToFilter(StmPrintJobCopyRecipientSchema.SPR_EmailAddress, SQLComparisonOperator.Equal, recipient.MainAddress.OA_Email);
			query.AddSubQuery(subQuery, JoinCondition.And);

			var printJob = Factory.LoadTop1<StmPrintJob>(query);
			AssertNotNull("Document should be delivered", printJob);
		}

		protected override string TransportMode
		{
			get { return Core.Constants.TransportModes.Sea; }
		}

		protected override GuidRegistryItem NotificationGroupRegistryItem
		{
			get { return FreightDataRegistry.Instance.SeaScheduleChangeNotificationGroup; }
		}

		protected override void SetUp()
		{
			base.SetUp();
			FreightConfigurationRegistry.Instance.AutoDeliverDelayAlertDocuments.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DelayAlertDeliveryRuleCollection()
			{
				new DelayAlertDeliveryRule()
			});

			FreightDataRegistry.Instance.DefaultShipmentDestinationFromConsolDischarge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			FreightDataRegistry.Instance.DefaultShipmentOriginFromConsolLoad.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		JobVoyage Voyage
		{
			get
			{
				if (voyage == null || voyage.IsDeleted)
				{
					voyage = NewJobVoyage(Core.Constants.TransportModes.Sea, "Vessel", "Voyage", ShippingLine.PK);
				}
				return voyage;
			}
		}
		JobVoyage voyage;

		JobSailing ImportSailing
		{
			get
			{
				if (importSailing == null || importSailing.IsDeleted)
				{
					importSailing = NewJobSailing(Voyage, "MYPKG", "AUSYD", new ZDateTime(2013, 1, 1), new ZDateTime(2013, 1, 2), new ZDateTime(2013, 1, 3), new ZDateTime(2013, 1, 4));
				}
				return importSailing;
			}
		}
		JobSailing importSailing;

		JobSailing ExportSailing
		{
			get
			{
				if (exportSailing == null || exportSailing.IsDeleted)
				{
					exportSailing = NewJobSailing(Voyage, "AUSYD", "MYPKG", new ZDateTime(2013, 1, 1), new ZDateTime(2013, 1, 2), new ZDateTime(2013, 1, 3), new ZDateTime(2013, 1, 4));
				}
				return exportSailing;
			}
		}
		JobSailing exportSailing;

		JobsForTesting JobsForTesting
		{
			get
			{
				if (jobsForTesting == null)
				{
					jobsForTesting = new JobsForTesting(Factory);
				}
				return jobsForTesting;
			}
		}
		JobsForTesting jobsForTesting;

		OrgHeader DelayAlertRecipient
		{
			get
			{
				if (delayAlertRecipient == null)
				{
					delayAlertRecipient = Factory.NewWithValidTestData<OrgHeader>();
					delayAlertRecipient.MainAddress.OA_Email = "DelayAlertRecipient@edi.com.au";
				}
				return delayAlertRecipient;
			}
		}
		OrgHeader delayAlertRecipient;

		DocumentCommand ShipmentDelayAlertDocument
		{
			get
			{
				if (shipmentDelayAlertDocument == null)
				{
					DocumentZQuery query = new DocumentZQuery(BusinessContext.Shipment, "Delay Alert");
					shipmentDelayAlertDocument = Factory.LoadTop1<DocumentCommand>(query);
				}
				return shipmentDelayAlertDocument;
			}
		}
		DocumentCommand shipmentDelayAlertDocument;

		IShowEditFormUrlCreator ShowEditFormUrlCreator
		{
			get { return showEditFormUrlCreator ?? (showEditFormUrlCreator = ObjectFactory.Get<IShowEditFormUrlCreator>()); }
		}
		IShowEditFormUrlCreator showEditFormUrlCreator;

		#endregion
	}
}

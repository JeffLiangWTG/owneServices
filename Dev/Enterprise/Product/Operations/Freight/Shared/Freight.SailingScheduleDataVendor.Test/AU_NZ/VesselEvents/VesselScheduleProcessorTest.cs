using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.SailingDataVendor.Shared;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.SailingDataVendor.Business.Testing
{
	sealed class VesselScheduleProcessorTest : OneStopProcessorBaseTest
	{
		[TestDate(2005, 3, 1)]
		public void TestCanRunInAnyBranch()
		{
			ErrorReporter.Clear();

			VesselScheduleMailItem.ExtractAttachments();

			var vesselSchedule = Factory.New<JobVesselSchedule>();
			vesselSchedule.EV_TerminalID = "CTLPB";
			vesselSchedule.EV_IMOLloydsNumber = "1234548";
			vesselSchedule.EV_ShipOperatorVoyageIn = "999S";
			vesselSchedule.EV_ShipOperatorVoyageOut = "999N";
			vesselSchedule.EV_LineOperator = "MOL";
			vesselSchedule.EV_OperatorsDescription = "DIFFERENT";
			vesselSchedule.EV_ShipOperatorsCode = "ME";
			vesselSchedule.EV_RL_NKPortCode = "DIFFR";
			vesselSchedule.EV_ETA = ZDateTime.Now.AddDays(-2);
			vesselSchedule.EV_ETD = ZDateTime.Now.AddDays(-1);
			vesselSchedule.EV_ShipName = "DIFFER";
			vesselSchedule.EV_CargoCuttOff = ZDateTime.Now;
			vesselSchedule.EV_ReeferCutOff = ZDateTime.Now;
			vesselSchedule.EV_ImportAvailability = ZDateTime.Now;
			vesselSchedule.EV_ImportStorageCommences = ZDateTime.Now;
			vesselSchedule.EV_ContainerVessel = "DIFF";
			vesselSchedule.EV_ActualArrival = ZDateTime.Now;
			vesselSchedule.EV_ActualDeparture = ZDateTime.Now;
			vesselSchedule.EV_VesselCode = "DIF";
			vesselSchedule.EV_ExportReceivalCommencementDate = ZDateTime.Now;

			var org = Factory.New<OrgHeader>();
			org.OH_IsActive = true;
			org.OH_Code = "TST";

			var oneStopCusCode = Factory.New<OrgCusCode>();
			oneStopCusCode.OK_CustomsRegNo = vesselSchedule.EV_LineOperator;
			oneStopCusCode.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
			oneStopCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			oneStopCusCode.OK_OH = org.PK;

			var vessel = Factory.New<RefVessel>();
			vessel.RV_LloydsNumber = vesselSchedule.EV_IMOLloydsNumber;
			vessel.RV_Name = "vessel1";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_VoyageFlight = vesselSchedule.EV_ShipOperatorVoyageIn;
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage.JV_RV_NKVessel = vessel.RV_Name;
			voyage.JV_OH_Line = org.PK;

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "NZAKL";

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUSYD";

			Factory.Save();

			var logger = new Mock<ILogger>();
			var processor = new VesselScheduleProcessor();

			using (Env.Instance.TemporaryServiceTaskContext("MAP", canRunInAnyBranch: true))
			{
				processor.ProcessVesselSchedule(VesselScheduleMailItem.PK, logger.Object);
			}

			Assert(string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
			AssertRecordHasBeenUpdated();
		}

		[TestDate(2005, 02, 25)]
		public void TestMessageFilter()
		{
			VesselScheduleMailItem.ExtractAttachments();
			int startingRecordCount = Factory.GetDatabaseCount(typeof(JobVesselSchedule));
			SetupTestData();
			AssertEquals("Number of records to start with is correct", 2, Factory.GetDatabaseCount(typeof(JobVesselSchedule)) - startingRecordCount);
			Factory.Save();

			var helper = new MessageFilterTestHelper<VesselScheduleProcessor, MailItem>();
			Assert(helper.Process(VesselScheduleMailItem));
			AssertEquals("Number of records in table is correct", 154, Factory.GetDatabaseCount(typeof(JobVesselSchedule)) - startingRecordCount);
			AssertRecordHasBeenUpdated();
			AssertEquals(2, helper.Log.Count);
			AssertEquals("Information|1-STOP : 152 records have been added to the JobVesselSchedule table", helper.Log[0]);
			AssertEquals("Information|1-STOP : 1 records have been updated in the JobVesselSchedule table", helper.Log[1]);
		}

		[TestDate(2005, 3, 1)]
		public override void TestProcessWithEmptyTable()
		{
			VesselScheduleMailItem.ExtractAttachments();

			var startingRecordCount = Factory.GetDatabaseCount(typeof(JobVesselSchedule));
			var processor = new VesselScheduleProcessorForTest();
			processor.ProcessMailItemForTest(VesselScheduleMailItem);

			AssertEquals("Number of records in table is correct", 153, Factory.GetDatabaseCount(typeof(JobVesselSchedule)) - startingRecordCount);

			var allJobVesselSchedules = Factory.Load<JobVesselSchedule>(new ZQuery());
			AssertFactoryContains(processor.OriginalFactory, allJobVesselSchedules);
			AssertFactoryNotContains(processor.CurrentFactory, allJobVesselSchedules);
		}

		[TestDate(2005, 3, 1)]
		public override void TestProcessWithSomeDataAlreadyInTable()
		{
			VesselScheduleMailItem.ExtractAttachments();

			var startingRecordCount = Factory.GetDatabaseCount(typeof(JobVesselSchedule));
			SetupTestData();
			AssertEquals("Number of records to start with is correct", 2, Factory.GetDatabaseCount(typeof(JobVesselSchedule)) - startingRecordCount);

			var processor = new VesselScheduleProcessorForTest();
			processor.ProcessMailItemForTest(VesselScheduleMailItem);
			AssertEquals("Number of records in table is correct", 154, Factory.GetDatabaseCount(typeof(JobVesselSchedule)) - startingRecordCount);
			AssertRecordHasBeenUpdated();

			var allJobVesselSchedules = Factory.Load<JobVesselSchedule>(new ZQuery());
			AssertFactoryContains(processor.OriginalFactory, allJobVesselSchedules);
			AssertFactoryNotContains(processor.CurrentFactory, allJobVesselSchedules);
		}

		public void TestProcessInvalidFile()
		{
			VesselScheduleMailItem.MI_Body = System.Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes("splaty"));
			VesselScheduleMailItem.ExtractAttachments();
			VesselScheduleProcessor processor = new VesselScheduleProcessor();
			try
			{
				processor.ProcessMailItemForTest(VesselScheduleMailItem);
			}
			catch
			{
			}
			AssertEquals("Mail item should be marked as failed", MailStatus.Failed, VesselScheduleMailItem.MI_Status);
		}

		[TestDate(2005, 3, 1)]
		public void TestDataProviderAndDataProviderReference()
		{
			VesselScheduleMailItem.ExtractAttachments();
			int startingRecordCount = Factory.GetDatabaseCount(typeof(JobVesselSchedule));
			VesselScheduleProcessor processor = new VesselScheduleProcessor();
			processor.ProcessMailItemForTest(VesselScheduleMailItem);
			AssertEquals("Number of records in table is correct", 153, Factory.GetDatabaseCount(typeof(JobVesselSchedule)) - startingRecordCount);

			foreach (JobVesselSchedule schedule in Factory.Load<JobVesselSchedule>(new ZQuery()))
			{
				AssertEquals("DataProvider", FreightConstants.VesselDataProviders.OneStop, schedule.EV_DataProvider);
				AssertEquals("DataProviderReference", ZString.Empty, schedule.EV_DataProviderReference);
			}
		}

		[TestDate(2005, 3, 1)]
		public void TestProcessUpdatesOnlyOneStopRecords()
		{
			VesselScheduleMailItem.ExtractAttachments();
			int startingRecordCount = Factory.GetDatabaseCount(typeof(JobVesselSchedule));
			VesselScheduleProcessor processor = new VesselScheduleProcessor();
			processor.ProcessMailItemForTest(VesselScheduleMailItem);
			AssertEquals("Number of records in table is correct", 153, Factory.GetDatabaseCount(typeof(JobVesselSchedule)) - startingRecordCount);

			JobVesselSchedule[] schedules = Factory.Load<JobVesselSchedule>(new ZQuery(JobVesselScheduleSchema.EV_DataProvider, FreightConstants.VesselDataProviders.OneStop));
			foreach (JobVesselSchedule schedule in schedules)
			{
				schedule.EV_DataProvider = FreightConstants.VesselDataProviders.DBH;
			}

			Factory.Save();

			VesselScheduleMailItem.ExtractAttachments();
			startingRecordCount = Factory.GetDatabaseCount(typeof(JobVesselSchedule));
			processor.ProcessMailItemForTest(VesselScheduleMailItem);
			AssertEquals("Number of records in table is correct", 153, Factory.GetDatabaseCount(typeof(JobVesselSchedule)) - startingRecordCount);
		}

		[TestDate(1912, 6, 1)]
		public void TestProcess_VesselScheduleWithoutFirstFreeImportDate_UseImportAvailabilityDate()
		{
			// Configuring the test mail
			var mailItem = CreateMailItem();
			var importAvailabilityDate = "2012-08-02 06:00:00";
			AddMailAttachment(mailItem, importAvailability: importAvailabilityDate, numberOfParamsInRow: 20);

			// Testing
			var processor = new VesselScheduleProcessor();
			processor.ProcessMailItemForTest(mailItem);

			// Checking result
			ZDateTime expectedValue;
			ZDateTime.TryParseExact(importAvailabilityDate, out expectedValue, FileDateTimeFormat);
			var vesselSchedules = Factory.Load<JobVesselSchedule>(new ZDBOnlyQuery(typeof(JobVesselSchedule)));

			AssertEquals("Schedule from an attachment is parsed and added to a table", 1, vesselSchedules.Length);
			AssertEquals("ImportAvailabilityDate was used as ImportAvailability field.", expectedValue, vesselSchedules[0].EV_ImportAvailability);
		}

		public void TestPreventVoyageErrorReportDuringProcessingVesselSchedule()
		{
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var vessel = RefVessel.LookupVesselByName("AUSTRALIAN ENDEAVOUR", Factory).First();

				var voyage = Factory.NewWithValidTestData<JobVoyage>();
				voyage.JV_AirSeaRoad = "SEA";
				voyage.JV_VoyageFlight = "031N";
				voyage.JV_RV_NKVessel = vessel.RV_FK;

				var origin = voyage.Origins.AddNew();
				origin.JA_RL_NKPortOfLoading = "AUSYD";

				var dest = voyage.Destinations.AddNew();
				dest.JB_RL_NKPortOfDischarge = "CTLPB";

				Factory.Save();

				// Configuring the test mail
				var mailItem = CreateMailItem();
				var importAvailabilityDate = "2012-08-02 06:00:00";
				AddMailAttachment(mailItem, importAvailability: importAvailabilityDate, numberOfParamsInRow: 20);

				var processor = new VesselScheduleProcessor();

				ErrorReporter.Clear();
				processor.ProcessMailItemForTest(mailItem);

				Assert(string.IsNullOrEmpty(ErrorReporter.LastKeyReported));
				Assert(string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
			}

			ErrorReporter.Clear();
		}

		public void TestPreventVoyageErrorReportDuringProcessingVesselSchedule_InvalidSailing()
		{
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var vessel = RefVessel.LookupVesselByName("AUSTRALIAN ENDEAVOUR", Factory).First();

				var voyage = Factory.NewWithValidTestData<JobVoyage>();
				voyage.JV_AirSeaRoad = "SEA";
				voyage.JV_VoyageFlight = "031N";
				voyage.JV_RV_NKVessel = vessel.RV_FK;

				var origin = voyage.Origins.AddNew();
				origin.JA_RL_NKPortOfLoading = "AUSYD";

				var dest = voyage.Destinations.AddNew();
				dest.JB_RL_NKPortOfDischarge = "CTLPB";
				dest.JB_E_ARV = new DateTime(1912, 1, 1);

				Factory.Save();

				// Configuring the test mail
				var mailItem = CreateMailItem();
				var importAvailabilityDate = "2012-08-02 06:00:00";
				AddMailAttachment(mailItem, importAvailability: importAvailabilityDate, numberOfParamsInRow: 20,
					eTA: "1912-04-14 00:02:00", eTD: "1912-04-10 00:02:00");

				var processor = new VesselScheduleProcessor();

				ErrorReporter.Clear();
				processor.ProcessMailItemForTest(mailItem);

				Assert(string.IsNullOrEmpty(ErrorReporter.LastKeyReported));
				Assert(string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
			}

			ErrorReporter.Clear();
		}

		public void TestPreventVoyageErrorReportDuringProcessingVesselSchedule_SailingMissing()
		{
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var vessel = RefVessel.LookupVesselByName("AUSTRALIAN ENDEAVOUR", Factory).First();

				var voyage = Factory.NewWithValidTestData<JobVoyage>();
				voyage.JV_AirSeaRoad = "SEA";
				voyage.JV_VoyageFlight = "031N";
				voyage.JV_RV_NKVessel = vessel.RV_FK;

				var origin = voyage.Origins.AddNew();
				origin.JA_RL_NKPortOfLoading = "AUSYD";
				origin.JA_E_DEP = new DateTime(1913, 1, 1);

				var dest = voyage.Destinations.AddNew();
				dest.JB_RL_NKPortOfDischarge = "CTLPB";
				dest.JB_E_ARV = new DateTime(1912, 6, 1);

				Factory.Save();

				// Configuring the test mail
				var mailItem = CreateMailItem();
				var importAvailabilityDate = "2012-08-02 06:00:00";
				AddMailAttachment(mailItem, importAvailability: importAvailabilityDate, numberOfParamsInRow: 20,
					eTA: "1912-04-14 00:02:00", eTD: "1912-04-10 00:02:00");

				var processor = new VesselScheduleProcessor();

				ErrorReporter.Clear();
				processor.ProcessMailItemForTest(mailItem);

				Assert(string.IsNullOrEmpty(ErrorReporter.LastKeyReported));
				Assert(string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
			}

			ErrorReporter.Clear();
		}

		public void TestPreventVoyageErrorReportDuringProcessingVesselSchedule_SailingMissing_ForBadData()
		{
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var vessel = RefVessel.LookupVesselByName("AUSTRALIAN ENDEAVOUR", Factory).First();

				var voyage = Factory.NewWithValidTestData<JobVoyage>();
				voyage.JV_AirSeaRoad = "SEA";
				voyage.JV_VoyageFlight = "031N";
				voyage.JV_RV_NKVessel = vessel.RV_FK;

				var origin = voyage.Origins.AddNew();
				origin.JA_RL_NKPortOfLoading = "AUSYD";
				origin.JA_E_DEP = new DateTime(2019, 3, 1);

				var dest = voyage.Destinations.AddNew();
				dest.JB_RL_NKPortOfDischarge = "CTLPB";
				dest.JB_E_ARV = new DateTime(2019, 4, 10);

				Factory.Save();

				voyage.Sailings.RemoveAndDeleteAll();
				Factory.Save();

				// Configuring the test mail
				var mailItem = CreateMailItem();
				var importAvailabilityDate = "2012-08-02 06:00:00";
				AddMailAttachment(mailItem, importAvailability: importAvailabilityDate, numberOfParamsInRow: 20,
					eTD: "2019-03-01 00:00:00", eTA: "2019-04-10 00:00:00");

				var processor = new VesselScheduleProcessor();

				ErrorReporter.Clear();
				processor.ProcessMailItemForTest(mailItem);

				Assert(string.IsNullOrEmpty(ErrorReporter.LastKeyReported));
				Assert(string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
			}

			ErrorReporter.Clear();
		}

		[TestDate(1912, 6, 1)]
		public void TestProcess_VesselScheduleWithFirstFreeImportDate_UseFirstFreeImportDate()
		{
			// Configuring the test mail
			var mailItem = CreateMailItem();
			var firstFreeImportDate = "2012-08-02 06:00:00";
			AddMailAttachment(mailItem, firstFreeImportDate: firstFreeImportDate, numberOfParamsInRow: 21);

			// Testing
			var processor = new VesselScheduleProcessor();
			processor.ProcessMailItemForTest(mailItem);

			// Checking result
			ZDateTime expectedValue;
			ZDateTime.TryParseExact(firstFreeImportDate, out expectedValue, FileDateTimeFormat);
			var vesselSchedules = Factory.Load<JobVesselSchedule>(new ZDBOnlyQuery(typeof(JobVesselSchedule)));

			AssertEquals("Schedule from an attachment is parsed and added to a table", 1, vesselSchedules.Length);
			AssertEquals("FirstFreeImportDate was used as ImportAvailability field.", expectedValue, vesselSchedules[0].EV_ImportAvailability);
		}

		[TestDate(1912, 6, 1)]
		public void TestProcess_VesselScheduleWithEmptyFirstFreeImportDate_UseImportAvailabilityDate()
		{
			// Configuring the test mail
			var mailItem = CreateMailItem();
			var firstFreeImportDate = "";
			var importAvailabilityDate = "2012-08-02 06:00:00";
			AddMailAttachment(mailItem, firstFreeImportDate: firstFreeImportDate, importAvailability: importAvailabilityDate, numberOfParamsInRow: 21);

			// Testing
			var processor = new VesselScheduleProcessor();
			processor.ProcessMailItemForTest(mailItem);

			// Checking result
			ZDateTime expectedValue;
			ZDateTime.TryParseExact(importAvailabilityDate, out expectedValue, FileDateTimeFormat);
			var vesselSchedules = Factory.Load<JobVesselSchedule>(new ZDBOnlyQuery(typeof(JobVesselSchedule)));

			AssertEquals("Schedule from an attachment is parsed and added to a table", 1, vesselSchedules.Length);
			AssertEquals("FirstFreeImportDate was used as ImportAvailability field.", expectedValue, vesselSchedules[0].EV_ImportAvailability);
		}

		public void TestProcess_VesselScheduleWithInvalidFirstFreeImportDate_DoNotAddVesselIntoTheTable()
		{
			// Configuring the test mail
			var mailItem = CreateMailItem();
			var firstFreeImportDate = "aaaaa";
			var importAvailabilityDate = "2012-08-02 06:00:00";
			AddMailAttachment(mailItem, firstFreeImportDate: firstFreeImportDate, importAvailability: importAvailabilityDate, numberOfParamsInRow: 21);

			// Testing
			var processor = new VesselScheduleProcessor();
			processor.ProcessMailItemForTest(mailItem);

			// Checking result
			ZDateTime expectedValue;
			ZDateTime.TryParseExact(importAvailabilityDate, out expectedValue, FileDateTimeFormat);
			var vesselSchedules = Factory.Load<JobVesselSchedule>(new ZDBOnlyQuery(typeof(JobVesselSchedule)));

			AssertEquals("Schedule from an attachment was not added to a table as it contains an invalid data", 0, vesselSchedules.Length);
		}

		public void TestProcess_VesselScheduleWithInvalidVoyageNo_DoNotAddVesselIntoTheTable()
		{
			var mailItem = CreateMailItem();
			AddMailAttachment(mailItem, shipOperatorVoyageIn: "NA", shipOperatorVoyageOut: "TBA");

			var processor = new VesselScheduleProcessor();
			processor.ProcessMailItemForTest(mailItem);

			var vesselSchedules = Factory.Load<JobVesselSchedule>(new ZDBOnlyQuery(typeof(JobVesselSchedule)));
			AssertEquals("Schedule from an attachment was not added to a table as it contains invalid voyage numbers", 0, vesselSchedules.Length);
		}

		[TestDate(1912, 05, 15)]
		public void TestProcessDiscardsObsoleteRecords_WithPastDates()
		{
			var mailItem1 = CreateMailItem();
			var voyageIn = "ABC";
			var today = ZDateTime.Today.ToString(FileDateTimeFormat);

			AddMailAttachment(mailItem1, shipOperatorVoyageIn: voyageIn, eTD: today, numberOfParamsInRow: 20);
			Factory.Save();

			var logger = new Mock<ILogger>();
			var processor = new VesselScheduleProcessor();
			processor.ProcessVesselSchedule(mailItem1.PK, logger.Object);

			var vesselSchedules = Factory.Load<JobVesselSchedule>(new ZDBOnlyQuery(typeof(JobVesselSchedule)));
			AssertEquals("Schedule from an attachment is parsed and added to a table.", 1, vesselSchedules.Length);
			var vesselSchedule = vesselSchedules[0];
			AssertEquals("ShipOperatorVoyageIn", "ABC", vesselSchedule.EV_ShipOperatorVoyageIn);

			this.tempDirectory.Dispose();
			this.tempDirectory = new TempDirectory();

			var mailItem2 = CreateMailItem();
			voyageIn = "XYZ";
			AddMailAttachment(mailItem2, shipOperatorVoyageIn: voyageIn, eTD: today, numberOfParamsInRow: 20);

			vesselSchedule.EV_DataProvider = FreightConstants.VesselDataProviders.DBH;
			Factory.Save();

			processor.ProcessVesselSchedule(mailItem2.PK, logger.Object);

			vesselSchedules = Factory.Load<JobVesselSchedule>(new ZDBOnlyQuery(typeof(JobVesselSchedule)));
			vesselSchedules.Cast<JobVesselSchedule>().OrderBy(schedule => schedule.EV_ShipOperatorVoyageIn).ToArray();
			AssertEquals("Schedule from an attachment is parsed and added to a table. Existing records are retained.", 2, vesselSchedules.Length);
			AssertEquals("ShipOperatorVoyageIn", "ABC", vesselSchedules[0].EV_ShipOperatorVoyageIn);
			AssertEquals("ShipOperatorVoyageIn", "XYZ", vesselSchedules[1].EV_ShipOperatorVoyageIn);

			this.tempDirectory.Dispose();
			this.tempDirectory = new TempDirectory();

			var mailItem3 = CreateMailItem();
			voyageIn = "EDI";
			AddMailAttachment(mailItem3, shipOperatorVoyageIn: voyageIn, eTD: today, numberOfParamsInRow: 20);

			vesselSchedule.EV_DataProvider = FreightConstants.VesselDataProviders.OneStop;
			Factory.Save();

			processor.ProcessVesselSchedule(mailItem3.PK, logger.Object);

			vesselSchedules = Factory.Load<JobVesselSchedule>(new ZDBOnlyQuery(typeof(JobVesselSchedule)));
			vesselSchedules.Cast<JobVesselSchedule>().OrderBy(schedule => schedule.EV_ShipOperatorVoyageIn).ToArray();
			AssertEquals("Schedule from an attachment is parsed and added to a table. Existing records are retained.", 3, vesselSchedules.Length);
			AssertEquals("ShipOperatorVoyageIn", "ABC", vesselSchedules[0].EV_ShipOperatorVoyageIn);
			AssertEquals("ShipOperatorVoyageIn", "XYZ", vesselSchedules[1].EV_ShipOperatorVoyageIn);
			AssertEquals("ShipOperatorVoyageIn", "EDI", vesselSchedules[2].EV_ShipOperatorVoyageIn);
		}

		[TestDate(1912, 05, 15)]
		public void TestProcessDiscardsObsoleteRecords_WithFutureDates()
		{
			var mailItem1 = CreateMailItem();
			var voyageIn = "ABC";
			var futureDate = ZDateTime.Today.AddDays(1).ToString(FileDateTimeFormat);

			AddMailAttachment(mailItem1, shipOperatorVoyageIn: voyageIn, eTD: futureDate, numberOfParamsInRow: 20);
			Factory.Save();

			var logger = new Mock<ILogger>();
			var processor = new VesselScheduleProcessor();
			processor.ProcessVesselSchedule(mailItem1.PK, logger.Object);

			var vesselSchedules = Factory.Load<JobVesselSchedule>(new ZDBOnlyQuery(typeof(JobVesselSchedule)));
			AssertEquals("Schedule from an attachment is parsed and added to a table.", 1, vesselSchedules.Length);
			var vesselSchedule = vesselSchedules[0];
			AssertEquals("ShipOperatorVoyageIn", "ABC", vesselSchedule.EV_ShipOperatorVoyageIn);

			this.tempDirectory.Dispose();
			this.tempDirectory = new TempDirectory();

			var mailItem2 = CreateMailItem();
			voyageIn = "XYZ";
			AddMailAttachment(mailItem2, shipOperatorVoyageIn: voyageIn, eTD: futureDate, numberOfParamsInRow: 20);

			vesselSchedule.EV_DataProvider = FreightConstants.VesselDataProviders.DBH;
			Factory.Save();

			processor.ProcessVesselSchedule(mailItem2.PK, logger.Object);

			vesselSchedules = Factory.Load<JobVesselSchedule>(new ZDBOnlyQuery(typeof(JobVesselSchedule)));
			vesselSchedules.Cast<JobVesselSchedule>().OrderBy(schedule => schedule.EV_ShipOperatorVoyageIn).ToArray();
			AssertEquals("Schedule from an attachment is parsed and added to a table. Existing records are retained.", 2, vesselSchedules.Length);
			AssertEquals("ShipOperatorVoyageIn", "ABC", vesselSchedules[0].EV_ShipOperatorVoyageIn);
			AssertEquals("ShipOperatorVoyageIn", "XYZ", vesselSchedules[1].EV_ShipOperatorVoyageIn);

			this.tempDirectory.Dispose();
			this.tempDirectory = new TempDirectory();

			var mailItem3 = CreateMailItem();
			voyageIn = "EDI";
			AddMailAttachment(mailItem3, shipOperatorVoyageIn: voyageIn, eTD: futureDate, numberOfParamsInRow: 20);

			vesselSchedule.EV_DataProvider = FreightConstants.VesselDataProviders.OneStop;
			Factory.Save();

			processor.ProcessVesselSchedule(mailItem3.PK, logger.Object);

			vesselSchedules = Factory.Load<JobVesselSchedule>(new ZDBOnlyQuery(typeof(JobVesselSchedule)));
			AssertEquals("Schedule from an attachment is parsed and added to a table. Obsolete One Stop records are deleted.", 1, vesselSchedules.Length);
			AssertEquals("ShipOperatorVoyageIn", "EDI", vesselSchedules[0].EV_ShipOperatorVoyageIn);
		}

		public void TestProcessDiscardsObsoleteRecords_Outdated()
		{
			var timeNow = ZDateTime.Now;
			var mailItem1 = CreateMailItem();

			var routing1 = Factory.New<JobVesselRouting>();
			routing1.E1_LloydsID = "0000001";
			routing1.E1_VoyageNumber = "EDI";

			var routing3 = Factory.New<JobVesselRouting>();
			routing3.E1_LloydsID = "0000002";
			routing3.E1_VoyageNumber = "EDI";

			var schedule = Factory.New<JobVesselSchedule>();
			schedule.EV_ShipOperatorVoyageIn = "EDI";
			schedule.EV_IMOLloydsNumber = "0000002";
			schedule.EV_ActualArrival = timeNow.AddDays(-15);

			Factory.Save();

			AddMailAttachment(mailItem1
				, shipOperatorVoyageIn: "EDI"
				, lloydsID: "0000001"
				, eTA: timeNow.AddMonths(-5).ToString(FileDateTimeFormat)
				, eTD: timeNow.AddMonths(-6).ToString(FileDateTimeFormat)
				, actualArrival: timeNow.AddMonths(-4).ToString(FileDateTimeFormat)
				, actualDepart: timeNow.AddMonths(-5).ToString(FileDateTimeFormat)
				, numberOfParamsInRow: 20);

			Factory.Save();

			var logger = new Mock<ILogger>();
			var processor = new VesselScheduleProcessor();
			processor.ProcessVesselSchedule(mailItem1.PK, logger.Object);

			var vesselSchedules = Factory.Load<JobVesselSchedule>(new ZDBOnlyQuery(typeof(JobVesselSchedule)));
			AssertEquals("Schedule from an attachment is parsed, but the outdated data is removed.", 1, vesselSchedules.Length);
			AssertEquals("0000002", vesselSchedules[0].EV_IMOLloydsNumber);

			var vesselRoutings = Factory.Load<JobVesselRouting>(new ZDBOnlyQuery(typeof(JobVesselRouting)));
			AssertEquals("routings are not removed by VesselScheduleProcessor.", 2, vesselRoutings.Length);
		}

		public void TestProcess_VesselSchedule_CatchAllZipExceptionForUAT()
		{
			AssertProcessVesselSchedule_CatchAllZipExceptionForUAT("HYE");
			AssertProcessVesselSchedule_CatchAllZipExceptionForUAT("ABC");
		}

		void AssertProcessVesselSchedule_CatchAllZipExceptionForUAT(string code)
		{
			ErrorReporter.Clear();

			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = code;

			var mailItem = CreateMailItem();

			var attachment = mailItem.MailAttachments.AddNew();
			attachment.MA_Data = System.Text.Encoding.Default.GetBytes(@"0000 0304 0a00 0000 0000 7b55 6349 0000
0000 0000 0000 0000 0000 0700 0000 7465
6d2e 746d 7050 4b01 021f 000a 0000 0000
007b 5563 4900 0000 0000 0000 0000 0000
0007 0024 0000 0000 0000 0020 0000 0000
0000 0074 656d 2e74 6d70 0a00 2000 0000
0000 0100 1800 709d ca1e 7c35 d201 709d
ca1e 7c35 d201 709d ca1e 7c35 d201 504b
0506 0000 0000 0100 0100 5900 0000 2500
0000 0000 ");
			attachment.MA_FileName = "BrokenZipFile.zip";

			Factory.Save();

			var logger = new Mock<ILogger>(MockBehavior.Strict);
			var processor = new VesselScheduleProcessor();

			AssertNoExceptionThrown(() => processor.ProcessVesselSchedule(mailItem.PK, logger.Object));

			if (code == "HYE")
			{
				AssertEquals("Invalid1StopSailingScheduleAttachedItem", ErrorReporter.LastKeyReported);
				AssertContains("The attached item is invalid", ErrorReporter.LastMessageReported);
			}
			else
			{
				Assert(string.IsNullOrEmpty(ErrorReporter.LastKeyReported));
				Assert(string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
			}

			ErrorReporter.Clear();
		}

		public void TestProcessCSVLine_InvalidLengthRecords()
		{
			AssertProcessCSVLine_InvalidLengthRecords("HYE");
			AssertProcessCSVLine_InvalidLengthRecords("ABC");
		}

		void AssertProcessCSVLine_InvalidLengthRecords(string code)
		{
			ErrorReporter.Clear();

			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = code;

			var mailItem = CreateMailItem();
			AddMailAttachment(mailItem, numberOfParamsInRow: 21);

			var processor = new VesselScheduleProcessor();

			AssertNoExceptionThrown(() => processor.ProcessMailItemForTest(mailItem));
			Assert(string.IsNullOrEmpty(ErrorReporter.LastKeyReported));
			Assert(string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

			mailItem = CreateMailItem();
			AddMailAttachment(mailItem, numberOfParamsInRow: 20);

			AssertNoExceptionThrown(() => processor.ProcessMailItemForTest(mailItem));
			Assert(string.IsNullOrEmpty(ErrorReporter.LastKeyReported));
			Assert(string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

			mailItem = CreateMailItem();
			AddMailAttachment(mailItem, numberOfParamsInRow: 19);

			AssertNoExceptionThrown(() => processor.ProcessMailItemForTest(mailItem));

			if (code == "HYE")
			{
				string csvRecord = "\"AUSYD\",\"CTLPB\",\"1912-04-14 00:02:00\",\"1912-04-14 00:02:00\",\"Titanic\",\"031N\",\"8913681\",\"1912-04-14 00:02:00\","
					+ "\"1912-04-14 00:02:00\",\"ANZ\",\"Harland and Wolff\",\"ANZ\",\"031S\",\"1912-04-14 00:02:00\",\"1912-04-14 00:02:00\",\"\",\"\",\"\",\"\"";

				AssertContains(csvRecord, ErrorReporter.LastMessageReported);
				AssertEquals("Invalid1StopVesselScheduleFileRecordLine", ErrorReporter.LastKeyReported);
				AssertContains("Invalid number of fields in a record.", ErrorReporter.LastMessageReported);
			}
			else
			{
				Assert(string.IsNullOrEmpty(ErrorReporter.LastKeyReported));
				Assert(string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
			}

			ErrorReporter.Clear();
		}

		public void TestProcessCSVLine_UniqueFieldsExceedMaxLength()
		{
			AssertProcessCSVLine_UniqueFieldsExceedMaxLength("HYE");
			AssertProcessCSVLine_UniqueFieldsExceedMaxLength("ABC");
		}

		void AssertProcessCSVLine_UniqueFieldsExceedMaxLength(string code)
		{
			ErrorReporter.Clear();

			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = code;

			var mailItem = CreateMailItem();
			var portCode = "AUSYD";
			var terminalID = "T1234567890";
			var lloyds = "12345678";
			var voyageOut = "V1234567890-1";
			var lineOperator = "ABCD";
			var voyageIn = "V1234567890-2";

			AddMailAttachment(mailItem, uNLOCO: portCode, terminalID: terminalID, lloydsID: lloyds, shipOperatorVoyageOut: voyageOut,
			lineOperator: lineOperator, shipOperatorVoyageIn: voyageIn, numberOfParamsInRow: 20);

			Assert(string.IsNullOrEmpty(ErrorReporter.LastKeyReported));
			Assert(string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

			var processor = new VesselScheduleProcessor();
			AssertNoExceptionThrown(() => processor.ProcessMailItemForTest(mailItem));

			JobVesselSchedule[] schedules = Factory.Load<JobVesselSchedule>(new ZQuery(JobVesselScheduleSchema.EV_RL_NKPortCode, "AUSYD"));
			AssertEquals("No lines should get processed.", 0, schedules.Length);

			if (code == "HYE")
			{
				string csvRecord = "\"AUSYD\",\"T1234567890\",\"1912-04-14 00:02:00\",\"1912-04-14 00:02:00\",\"Titanic\",\"V1234567890-1\",\"12345678\",\"1912-04-14 00:02:00\","
					+ "\"1912-04-14 00:02:00\",\"ABCD\",\"Harland and Wolff\",\"ANZ\",\"V1234567890-2\",\"1912-04-14 00:02:00\",\"1912-04-14 00:02:00\",\"\",\"\",\"\",\"\"";

				AssertContains(csvRecord, ErrorReporter.LastMessageReported);
				AssertEquals("Invalid1StopVesselScheduleFileRecordLine", ErrorReporter.LastKeyReported);
				AssertContains("One or more identifying fields have exceeded the maximum length.", ErrorReporter.LastMessageReported);
			}
			else
			{
				Assert(string.IsNullOrEmpty(ErrorReporter.LastKeyReported));
				Assert(string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
			}

			ErrorReporter.Clear();
		}

		[TestDate(1912, 6, 1)]
		public void TestProcessCSVLine_NonUniqueFieldsExceedMaxLength()
		{
			ErrorReporter.Clear();

			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "HYE";

			var mailItem = CreateMailItem();
			var terminalID = "T123456789";
			var portCode = "AUSYD11";
			var shipName = "Vessel with a very very very very long name";
			var operatorDescription = "Operator with a very long description";
			var operatorCode = "EFGH";
			var containerVessel = "LOLO11";
			var vesselCode = "IJKL";

			AddMailAttachment(mailItem, terminalID: terminalID, uNLOCO: portCode, shipName: shipName, operatorDescription: operatorDescription,
			shipOperatorCode: operatorCode, containerVessel: containerVessel, vesselCode: vesselCode, numberOfParamsInRow: 20);

			Assert(string.IsNullOrEmpty(ErrorReporter.LastKeyReported));
			Assert(string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

			var processor = new VesselScheduleProcessor();
			AssertNoExceptionThrown(() => processor.ProcessMailItemForTest(mailItem));

			JobVesselSchedule[] schedules = Factory.Load<JobVesselSchedule>(new ZQuery(JobVesselScheduleSchema.EV_TerminalID, "T123456789"));

			AssertEquals("One line should get processed.", 1, schedules.Length);
			Assert(string.IsNullOrEmpty(ErrorReporter.LastKeyReported));
			Assert(string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

			registrationKey.EnterpriseCodeForTest = "ABC";

			AssertNoExceptionThrown(() => processor.ProcessMailItemForTest(mailItem));

			schedules = Factory.Load<JobVesselSchedule>(new ZQuery(JobVesselScheduleSchema.EV_TerminalID, "T123456789"));

			AssertEquals("One line should get processed.", 1, schedules.Length);
			Assert(string.IsNullOrEmpty(ErrorReporter.LastKeyReported));
			Assert(string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

			ErrorReporter.Clear();
		}

		public void TestProcessCSVLine_ParseInvalidFormatDate()
		{
			AssertProcessCSVLine_ParseInvalidFormatDate("HYE");
			AssertProcessCSVLine_ParseInvalidFormatDate("ABC");
		}

		void AssertProcessCSVLine_ParseInvalidFormatDate(string code)
		{
			ErrorReporter.Clear();

			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = code;

			var mailItem = CreateMailItem();
			var invalidETA = "Invalid Date";

			AddMailAttachment(mailItem, eTA: invalidETA, numberOfParamsInRow: 20);

			Assert(string.IsNullOrEmpty(ErrorReporter.LastKeyReported));
			Assert(string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

			var processor = new VesselScheduleProcessor();
			AssertNoExceptionThrown(() => processor.ProcessMailItemForTest(mailItem));

			JobVesselSchedule[] schedules = Factory.Load<JobVesselSchedule>(new ZQuery(JobVesselScheduleSchema.EV_RL_NKPortCode, "AUSYD"));
			AssertEquals("No lines should get processed.", 0, schedules.Length);

			if (code == "HYE")
			{
				string csvRecord = "\"AUSYD\",\"CTLPB\",\"Invalid Date\",\"1912-04-14 00:02:00\",\"Titanic\",\"031N\",\"8913681\",\"1912-04-14 00:02:00\",\"1912-04-14 00:02:00\","
					+ "\"ANZ\",\"Harland and Wolff\",\"ANZ\",\"031S\",\"1912-04-14 00:02:00\",\"1912-04-14 00:02:00\",\"\",\"\",\"\",\"\",\"\"";

				AssertContains(csvRecord, ErrorReporter.LastMessageReported);
				AssertEquals("Invalid1StopVesselScheduleFileRecordLine", ErrorReporter.LastKeyReported);
				AssertContains("One or more date time field values in a record are either of invalid format or out of smalldatetime range:", ErrorReporter.LastMessageReported);
			}
			else
			{
				Assert(string.IsNullOrEmpty(ErrorReporter.LastKeyReported));
				Assert(string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
			}

			ErrorReporter.Clear();
		}

		[TestDate(2020, 9, 16)]
		public void TestProcess_DoNotAddOutOfRangeDateSchedules()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "HYE";

			var csvLines = @"""AUMEL"",""VICTM"",""2020-06-21 19:37:00"",""2020-06-24 19:37:00"",""VICTORIA WEBB"",""143"",""0123456"","""","""",""VIC"",""VICT Test Shipping Line"","""",""143"","""","""","""","""","""","""",""""
""PECLL"",""APMCLL"",""2015-01-17 05:00:00"",""2015-01-18 05:00:00"",""WARNOW DOLPHIN"",""GI204N"",""9395070"",""2015-01-16 23:00:00"",""2015-01-17 07:00:00"",""CMA"",""CMA CGM"",""CMA"",""GI204N"","""","""",""2015-02-11 03:08:00"","""",""2015-01-17 22:50:00"",""2015-01-18 04:25:00"",""""
""PECLL"",""APMCLL"",""2020-09-24 19:00:00"",""2020-09-25 19:00:00"",""MSC ARICA"",""NX035A"",""9619452"","""","""",""MSC"",""MEDITERRANEAN SHIPPING COMPANY"",""MSC"",""NX035A"","""","""","""","""","""","""",""""
""PECLL"",""APMCLL"",""2020-09-19 07:00:00"",""2020-09-20 13:00:00"",""BBC REEF"",""1293006"",""9539365"","""","""",""BBC"",""BBC CHARTERING"","""",""1293006"","""","""","""","""","""","""",""""
""PECLL"",""APMCLL"",""2020-09-15 15:00:00"",""2020-09-16 11:00:00"",""MSC CAPELLA"",""FA032A"",""9465289"",""2020-09-14 11:00:00"",""2020-09-14 19:00:00"",""MSC"",""MEDITERRANEAN SHIPPING COMPANY"",""MSC"",""FA032A"",""2020-09-12 11:00:00"","""","""","""",""2020-09-15 16:02:00"","""",""""
""PECLL"",""APMCLL"",""2020-09-27 23:00:00"",""2020-09-29 07:00:00"",""E.R. BERLIN"",""039W"",""9214214"","""","""",""MAE"",""MAERSK LINES - MAE"",""MAE"",""035E"","""","""","""","""","""","""",""""
""AUFRE"",""ASLFR"",""2020-04-22 07:00:00"",""2020-04-22 20:00:00"",""MP THE BROWN"",""FC016R"",""9403396"",""2020-04-21 15:00:00"",""2020-04-21 15:00:00"",""MSC"",""MEDITERRANEAN SHIPPING COMPANY"","""",""FC016A"",""2020-04-17 07:00:00"",""2020-04-23 07:00:00"",""2020-04-27 00:00:00"",""LOLO"",""2020-04-22 06:30:00"",""2020-04-22 21:00:00"",""""
""PHMNL"",""ICTSI"",""2019-02-17 07:20:00"",""2019-02-18 16:20:00"",""SITC LIAONING"",""1906W"",""9712369"",""2019-01-31 19:00:00"","""",""GSL"",""GOLD STAR SHIPPING"","""",""1905E"",""2019-01-24 19:00:00"","""","""","""","""","""",""""
""NZAKL"",""NZCCO"",""2018-04-01 06:00:00"",""2021-12-31 15:30:00"",""PLANETA"",""TBA"",""7811111"","""","""",""MSK"",""MAERSK"",""MSK"",""TBAS"","""","""","""","""","""","""",""""
""NZAKL"",""NZCCO"",""2018-04-01 06:00:00"",""2021-12-31 15:30:00"",""MSC EDITH"",""NA"",""9169029"","""","""",""MSK"",""MAERSK"",""MSK"",""NA"","""","""","""","""","""","""",""""
""NZAKL"",""NZCCO"",""2018-04-01 06:00:00"",""2021-12-31 15:30:00"",""ARDMORE"",""TBA"",""9077202"","""","""",""MSK"",""MAERSK"",""MSK"",""TBAS"","""","""","""","""","""","""",""""
""NZAKL"",""NZCCO"",""2018-04-01 06:00:00"",""2021-12-31 15:30:00"",""NINGPO"",""TBA"",""9134658"","""","""",""MSK"",""MAERSK"",""MSK"",""TBAS"","""","""","""","""","""","""",""""";

			var mailItem = CreateMailItem();
			AddMailAttachment(mailItem, csvLines: csvLines);

			var processor = new VesselScheduleProcessor();
			AssertNoExceptionThrown(() => processor.ProcessMailItemForTest(mailItem));

			var schedules = Factory.Load<JobVesselSchedule>(new ZQuery());
			AssertCollectionContains("VICTORIA WEBB", schedules.Select(s => s.EV_ShipName));
			AssertCollectionContains("MSC ARICA", schedules.Select(s => s.EV_ShipName));
			AssertCollectionContains("BBC REEF", schedules.Select(s => s.EV_ShipName));
			AssertCollectionContains("MSC CAPELLA", schedules.Select(s => s.EV_ShipName));
			AssertCollectionContains("E.R. BERLIN", schedules.Select(s => s.EV_ShipName));

			AssertEquals("Other records are out of date range, so only 5 schedules should be loaded.", 5, schedules.Length);
		}

		public void TestProcessCSVLine_ParseOutOfRangeDate()
		{
			AssertProcessCSVLine_ParseOutOfRangeDate("HYE");
			AssertProcessCSVLine_ParseOutOfRangeDate("ABC");
		}

		void AssertProcessCSVLine_ParseOutOfRangeDate(string enterpriseCode)
		{
			ErrorReporter.Clear();

			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = enterpriseCode;

			var invalidMailItem = CreateMailItem();
			var outOfRangeDate = "2099-05-08 00:02:50";

			AddMailAttachment(invalidMailItem, eTA: outOfRangeDate, uNLOCO: "AUMEL", numberOfParamsInRow: 20);

			var processor = new VesselScheduleProcessor();
			AssertNoExceptionThrown(() => processor.ProcessMailItemForTest(invalidMailItem));

			var schedules = Factory.Load<JobVesselSchedule>(new ZQuery(JobVesselScheduleSchema.EV_RL_NKPortCode, "AUMEL"));
			AssertEquals("No lines should get processed.", 0, schedules.Length);

			if (enterpriseCode == "HYE")
			{
				string csvRecord = "\"AUMEL\",\"CTLPB\",\"2099-05-08 00:02:50\",\"1912-04-14 00:02:00\",\"Titanic\",\"031N\",\"8913681\",\"1912-04-14 00:02:00\",\"1912-04-14 00:02:00\","
					+ "\"ANZ\",\"Harland and Wolff\",\"ANZ\",\"031S\",\"1912-04-14 00:02:00\",\"1912-04-14 00:02:00\",\"\",\"\",\"\",\"\",\"\"";

				AssertContains(csvRecord, ErrorReporter.LastMessageReported);
				AssertEquals("Invalid1StopVesselScheduleFileRecordLine", ErrorReporter.LastKeyReported);
				AssertContains("One or more date time field values in a record are either of invalid format or out of smalldatetime range:", ErrorReporter.LastMessageReported);
			}
			else
			{
				Assert("No issue reported: should be reported in UAT systems only.", string.IsNullOrEmpty(ErrorReporter.LastKeyReported));
				Assert("No issue reported: should be reported in UAT systems only.", string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
			}

			ErrorReporter.Clear();
		}

		[TestDate(2020, 6, 1)]
		public void TestProcess_SavesDatesAsSmallDateTime()
		{
			var processor = new VesselScheduleProcessor();
			var mailItem = CreateMailItem();
			AddMailAttachment(mailItem,
				eTD: "2020-04-12 15:12:35",
				eTA: "2020-04-13 00:10:15",
				actualDepart: "2020-04-12 15:15:35",
				actualArrival: "2020-04-13 00:15:15",
				cargoCutoff: "2020-04-12 18:12:35",
				reeferCutoff: "2020-04-12 17:12:35",
				exportRecivalCommencement: "2020-04-12 16:12:35",
				importAvailability: "2020-04-12 17:12:35",
				importStorage: "2020-04-12 18:12:35");

			mailItem.ExtractAttachments();
			processor.ProcessMailItemForTest(mailItem);

			var schedules = Factory.Load<JobVesselSchedule>(new ZQuery());
			AssertEquals(1, schedules.Length);

			var schedule = schedules[0];

			AssertEquals(new ZDateTime(2020, 4, 12, 15, 13, 00), schedule.EV_ETD);
			AssertEquals(new ZDateTime(2020, 4, 13, 00, 10, 00), schedule.EV_ETA);
			AssertEquals(new ZDateTime(2020, 4, 12, 15, 16, 00), schedule.EV_ActualDeparture);
			AssertEquals(new ZDateTime(2020, 4, 13, 00, 15, 00), schedule.EV_ActualArrival);
			AssertEquals(new ZDateTime(2020, 4, 12, 18, 13, 00), schedule.EV_CargoCuttOff);
			AssertEquals(new ZDateTime(2020, 4, 12, 17, 13, 00), schedule.EV_ReeferCutOff);
			AssertEquals(new ZDateTime(2020, 4, 12, 16, 13, 00), schedule.EV_ExportReceivalCommencementDate);
			AssertEquals(new ZDateTime(2020, 4, 12, 17, 13, 00), schedule.EV_ImportAvailability);
			AssertEquals(new ZDateTime(2020, 4, 12, 18, 13, 00), schedule.EV_ImportStorageCommences);
		}

		public void TestProcessCSV_DbHits()
		{
			ErrorReporter.Clear();

			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "HYE";

			var mailItem = CreateMailItem();

			var mailItemData =
@"""NZNPE"",""NPENZ"",""2016-04-12 07:00:00"",""2016-04-12 18:00:00"",""SPIRIT OF MELBOURNE"",""609N"",""9362413"","","",""CGM"",""CMA & CGM & ANL AGENCIES (AUST) P/L"",""HSD"",""609N"","","","",""CON"","","",""
""AUMEL"",""ASES1"",""2020-02-20 07:00:00"",""2020-02-22 01:00:00"",""CONTI EVEREST"",""MA008R"",""9286231"","","",""CMA"",""CMA CGM"","",""MA001A"","","","",""LOLO"","","",""
""AUBNE"",""HPAFI"",""2020-01-15 10:30:00"",""2020-01-16 14:02:00"",""ITAL MELODIA"",""147N"",""9315965"",""2020-01-14 22:00:00"",""2020-01-14 22:00:00"",""ONE"",""OCEAN NETWORK EXPRESS (AUSTRALIA) P"",""EMC"",""147S"",""2020-01-08 07:00:00"",""2020-01-16 11:00:00"",""2020-01-22 00:00:00"","",""2020-01-15 10:30:00"",""2020-01-16 14:02:00"",""
""AUBNE"",""HPAFI"",""2020-01-13 01:35:00"",""2020-01-14 14:34:00"",""COSCO FELIXSTOWE"",""151N"",""9246401"",""2020-01-10 22:00:00"",""2020-01-10 22:00:00"",""HSD"",""HAMBURG SUD"",""COS"",""151S"",""2020-01-06 07:00:00"",""2020-01-14 06:00:00"",""2020-01-17 00:00:00"","",""2020-01-13 01:35:00"",""2020-01-14 14:34:00"",""
""AUMEL"",""VICTM"",""2019-12-09 22:00:00"",""2019-12-11 14:00:00"",""CEZANNE"",""948N"",""9697416"",""2019-12-06 22:00:00"",""2019-12-06 22:00:00"",""HSD"",""HAMBURG SUD"","",""946S"",""2019-12-02 05:45:00"",""2019-12-09 21:30:00"",""2019-12-13 00:01:00"","",""2019-12-09 21:30:00"",""2019-12-11 14:30:00"",""";

			AddMailAttachment(mailItem, csvLines: mailItemData);

			var processor = new VesselScheduleProcessor();

			var expectedHits = new Dictionary<string, int>
			{
				{ "JobVesselSchedule", 1 }
			};

			using (AssertDbHitsForAllFactories(expectedHits, ignoreUnspecified: true))
			{
				AssertNoExceptionThrown(() => processor.ProcessMailItemForTest(mailItem));
			}

			ErrorReporter.Clear();
		}

		public void TestConcurrencyError_NumberOfRetriesBeforeFailure()
		{
			var mailItem = CreateMailItem();
			AddMailAttachment(mailItem);

			Factory.Save();

			var logger = new Mock<ILogger>();
			var processor = new VesselScheduleProcessor();
			var retryCount = 0;
			BusinessObjectFactory.SetOnFactorySaveHookForTest(delegate(BusinessObjectFactory factory)
			{
				if (factory.NameForDebugging == "One Stop Processor Factory")
				{
					retryCount++;
					throw new ZSaveConcurrencyException(new ZDataConcurrencyException(new InvalidOperationException("~ConcurrencyError~"), null, ((IDbConnected)factory).Connection), factory);
				}
			});

			var result = true;
			AssertNoExceptionThrown(() => result = processor.ProcessVesselSchedule(mailItem.PK, logger.Object));
			AssertEquals("Maximum retry count", 10, retryCount);
			AssertEquals("Process email should return false", false, result);
			logger.Verify(l => l.Log(LogType.Error, string.Format(CultureInfo.InvariantCulture, "{0} : Failed due to ConcurrencyError. You can reprocess from Emails module", FreightConstants.VesselDataProviderNames.OneStop)));
		}

		public void TestProcess_DoesNotHandleSaveException()
		{
			var mailItem = CreateMailItem();
			AddMailAttachment(mailItem);

			Factory.Save();

			var logger = new Mock<ILogger>();
			var processor = new VesselScheduleProcessor();
			BusinessObjectFactory.SetOnFactorySaveHookForTest(delegate(BusinessObjectFactory factory)
			{
				if (factory.NameForDebugging == "One Stop Processor Factory")
				{
					throw new ZSaveException(new ZDataConcurrencyException(new InvalidOperationException("~ConcurrencyError~"), null, ((IDbConnected)factory).Connection), factory);
				}
			});

			AssertExceptionThrown<ZSaveException>(() => processor.ProcessVesselSchedule(mailItem.PK, logger.Object));
		}

		public void TestProcess_ReportsWhenJobSailingUnexpectedlyDeleted()
		{
			// Arrange
			DbEnv.SetDbEnvironment(new BaseDbEnvironment());

			var mailItem = CreateMailItem();
			AddMailAttachment(mailItem);

			Factory.Save();

			var logger = new Mock<ILogger>();
			var processor = new VesselScheduleProcessor();
			BusinessObjectFactory.SetOnFactorySaveHookForTest(delegate(BusinessObjectFactory factory)
			{
				if (factory.NameForDebugging == "One Stop Processor Factory")
				{
					var jobSailing = new DataTable(AutoJobSailing.Schema.TableName);
					var pkColumn = new DataColumn(AutoJobSailing.Schema.PK, typeof(Guid));
					jobSailing.Columns.Add(pkColumn);

					var jobSailingRow = jobSailing.Rows.Add();
					jobSailingRow[0] = Guid.NewGuid();
					jobSailing.AcceptChanges();

					jobSailingRow.Delete();

					throw new ZSaveException(new ZDataConcurrencyException(
						new InvalidOperationException("~ConcurrencyError~"),
						jobSailingRow, ((IDbConnected)factory).Connection), factory);
				}
			});

			CombineAssertions(() =>
			{
				AssertExceptionThrown<ZSaveException>(() => processor.ProcessVesselSchedule(mailItem.PK, logger.Object));
				AssertEquals("Exactly one error must be reported.", 1, ErrorReporter.TotalErrorCount);
				AssertEquals("Error Message Key must be correct.", "JobSailing.Delete() Stacktrace for CS00968241", ErrorReporter.LastKeyReported);
				AssertNull("Exception instance must not be reported.", ErrorReporter.LastExceptionReported);
				AssertMatch("Error Message must contain expected details.", new Regex("No Stacktrace found for JobSailing with PK=[{]?[0-9a-fA-F]{8}-([0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}[}]?."), ErrorReporter.LastMessageReported);

				ErrorReporter.Clear();
			});
		}

		public void TestProcess_DoesNotReportAnyExceptions()
		{
			// Arrange
			DbEnv.SetDbEnvironment(new BaseDbEnvironment());

			var mailItem = CreateMailItem();
			AddMailAttachment(mailItem);

			Factory.Save();

			BusinessObjectFactory.SetOnFactorySaveHookForTest(delegate(BusinessObjectFactory factory)
			{
				if (factory.NameForDebugging == "One Stop Processor Factory")
				{
					throw new ZSaveConcurrencyException(
						new ZDataConcurrencyException(new InvalidOperationException("~ConcurrencyError~ 9F048992-5A83-40B5-8BA2-CB2F0DE527C1"), null,
							((IDbConnected)factory).Connection), factory);
				}
			});

			var processor = new VesselScheduleProcessor();

			// Act
			processor.ProcessVesselSchedule(mailItem.PK, new Mock<ILogger>().Object);

			// Assert
			AssertEquals("No exceptions must be reported.", 0, ExceptionReporterTestListener.Instance.Count);
		}

		[TestDate(2025, 1, 1)]
		public void TestProcess_MailLinesWithInvalidETDETA()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_IsActive = true;
			org.OH_Code = "TST";
			var oneStopCusCode = Factory.New<OrgCusCode>();
			oneStopCusCode.OK_CustomsRegNo = "MOL";
			oneStopCusCode.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
			oneStopCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			oneStopCusCode.OK_OH = org.PK;

			var vessel = Factory.New<RefVessel>();
			vessel.RV_LloydsNumber = "1234548";
			vessel.RV_Name = "COMMUNITY VESSEL";
			var voyage1 = Factory.New<JobVoyage>();
			voyage1.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage1.JV_RV_NKVessel = vessel.RV_FK;
			voyage1.JV_VoyageFlight = "999N";
			voyage1.JV_OH_Line = org.PK;
			var origin1 = voyage1.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "AUSYD";
			origin1.JA_E_DEP = new ZDateTime(2025, 10, 5);
			var destination1 = voyage1.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "AEDUJ";
			destination1.JB_E_ARV = new ZDateTime(2025, 10, 6);

			var voyage2 = Factory.New<JobVoyage>();
			voyage2.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage2.JV_RV_NKVessel = vessel.RV_FK;
			voyage2.JV_VoyageFlight = "999S";
			voyage2.JV_OH_Line = org.PK;
			var origin2 = voyage2.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "AUBNE";
			origin2.JA_E_DEP = new ZDateTime(2025, 2, 5);
			var destination2 = voyage2.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = "AUMEL";
			destination2.JB_E_ARV = new ZDateTime(2025, 2, 6);
			Factory.Save();
			AssertEquals(1, voyage1.Sailings.Count);
			AssertEquals(1, voyage2.Sailings.Count);

			// Voyage1 original sailing: AUSYD 10.05 - AEDUJ 10.06
			// 1st mail line: Voyage1 AUSYD 10.5 -- update to --> AUSYD 12.24 - invalid
			// 2nd mail line: Voyage1 AEDUJ 10.6 -- update to --> AEDUJ 06.23 - invalid
			// Voyage2 original sailing: AUBNE 02.05 - AUMEL 02.06
			// 3rd mail line: Voyage2 AUMEL 02.6 -- update to --> AUMEL 02.23 - valid
			var mailItem = CreateMailItem();
			var mailItemData = @"""AUSYD"",""CTLPB"",""2025-12-23 22:00:00"",""2025-12-24 22:00:00"",""COMMUNITY VESSEL"",""999N"",""1234548"",""2025-12-24 22:00:00"",""2025-12-24 22:00:00"",""MOL"",""MITSUI OSK LINES (AUSTRALIA) P/L"",""POC"",""999N"",""2024-11-10 22:00:00"","""","""","""","""","""",""""
""AEDUJ"",""CTLPB"",""2025-06-23 22:00:00"",""2025-06-24 22:00:00"",""COMMUNITY VESSEL"",""999N"",""1234548"",""2025-06-24 22:00:00"",""2025-06-24 22:00:00"",""MOL"",""MITSUI OSK LINES (AUSTRALIA) P/L"",""POC"",""999N"",""2024-05-10 22:00:00"","""","""","""","""","""",""""
""AUMEL"",""CTLPB"",""2025-02-23 22:00:00"",""2025-02-24 22:00:00"",""COMMUNITY VESSEL"",""999N"",""1234548"",""2025-02-24 22:00:00"",""2025-02-24 22:00:00"",""MOL"",""MITSUI OSK LINES (AUSTRALIA) P/L"",""POC"",""999S"",""2024-01-10 22:00:00"","""","""","""","""","""",""""";
			AddMailAttachment(mailItem, csvLines: mailItemData);
			Factory.Save();

			Globals.IsUserInteractive = false;
			var helper = new MessageFilterTestHelper<VesselScheduleProcessor, MailItem>();
			Assert(helper.Process(mailItem));

			AssertNoExceptionThrown(Factory.Save);
			AssertEquals("Information|1-STOP : 1 records have been added to the JobVesselSchedule table", helper.Log[0]);
			AssertEquals("Information|1-STOP : 0 records have been updated in the JobVesselSchedule table", helper.Log[1]);
			AssertEquals("Warning|1-STOP : 2 records have been rejected during updating the JobVesselSchedule table", helper.Log[2]);
			AssertEquals(1, voyage1.Sailings.Count);
			AssertEquals(new ZDateTime(2025, 10, 5), origin1.JA_E_DEP);
			AssertEquals(new ZDateTime(2025, 10, 6), destination1.JB_E_ARV);
			AssertEquals(1, voyage2.Sailings.Count);
			AssertEquals(new ZDateTime(2025, 2, 5), origin2.JA_E_DEP);
			AssertEquals(new ZDateTime(2025, 2, 23, 22, 0, 0), destination2.JB_E_ARV);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			tempDirectory = new TempDirectory();
			VesselScheduleMailItem = GetVesselScheduleMailItem(Factory);
		}

		public static MailItem GetVesselScheduleMailItem(BusinessObjectFactory factory)
		{
			var item = factory.New<MailItem>();
			item.MI_Status = MailStatus.Queued;
			item.MI_Direction = MailDirection.Receive;
			item.MI_SendDateTime = ZDateTime.Now;
			item.MI_ReceivedDateTime = ZDateTime.Now;
			item.AddRecipientForUserCommunication("testaddress@edi.net.au <testaddress@edi.net.au>", MailRecipient.RecipientTypes.TO);
			using (var resourceRetriever = new EmbeddedResourceRetriever(typeof(VesselScheduleLineRecordTest).Assembly))
			{
				item.MI_Body = resourceRetriever.GetString("Enterprise.Freight.SailingScheduleDataVendor.Test.AU_NZ.TestFiles.VesselScheduleEmailBody.txt");
				item.MI_Header = resourceRetriever.GetString("Enterprise.Freight.SailingScheduleDataVendor.Test.AU_NZ.TestFiles.VesselScheduleEmailHeader.txt");
			}
			item.MI_Subject = "Enterprise Vessel Schedule";
			item.MI_From = "Eagle Vessel Schedule and Routing <1-stop@edi.net.au>";

			return item;
		}

		protected override void TearDown()
		{
			base.TearDown();

			this.tempDirectory.Dispose();
		}

		protected override void SetupTestData()
		{
			JobVesselSchedule test1 = Factory.New<JobVesselSchedule>();
			test1.EV_TerminalID = "CTLPB";
			test1.EV_IMOLloydsNumber = "1234548";
			test1.EV_ShipOperatorVoyageIn = "999S";
			test1.EV_ShipOperatorVoyageOut = "999N";
			test1.EV_LineOperator = "MOL";
			test1.EV_OperatorsDescription = "DIFFERENT";
			test1.EV_ShipOperatorsCode = "ME";
			test1.EV_RL_NKPortCode = "DIFFR";
			test1.EV_ETA = ZDateTime.Now;
			test1.EV_ETD = ZDateTime.Now;
			test1.EV_ShipName = "DIFFER";
			test1.EV_CargoCuttOff = ZDateTime.Now;
			test1.EV_ReeferCutOff = ZDateTime.Now;
			test1.EV_ImportAvailability = ZDateTime.Now;
			test1.EV_ImportStorageCommences = ZDateTime.Now;
			test1.EV_ContainerVessel = "DIFF";
			test1.EV_ActualArrival = ZDateTime.Now;
			test1.EV_ActualDeparture = ZDateTime.Now;
			test1.EV_VesselCode = "DIF";
			test1.EV_ExportReceivalCommencementDate = ZDateTime.Now;

			JobVesselSchedule test2 = Factory.New<JobVesselSchedule>();
			test2.EV_TerminalID = "THIS";
			test2.EV_IMOLloydsNumber = "VESSEL";
			test2.EV_ShipOperatorVoyageIn = "ENTRY";
			test2.EV_ShipOperatorVoyageOut = "WILL";
			test2.EV_LineOperator = "NOT";
			test2.EV_OperatorsDescription = "LAZY";
			test2.EV_ShipOperatorsCode = "LZ";
			test2.EV_RL_NKPortCode = "MATCH";
			test2.EV_ETA = ZDateTime.Now;
			test2.EV_ETD = ZDateTime.Now;
			test2.EV_ShipName = "ANYTHING";
			test2.EV_CargoCuttOff = ZDateTime.Now;
			test2.EV_ReeferCutOff = ZDateTime.Now;
			test2.EV_ImportAvailability = ZDateTime.Now;
			test2.EV_ImportStorageCommences = ZDateTime.Now;
			test2.EV_ContainerVessel = "ELSE";
			test2.EV_ActualArrival = ZDateTime.Now;
			test2.EV_ActualDeparture = ZDateTime.Now;
			test2.EV_VesselCode = "ROO";
			test2.EV_ExportReceivalCommencementDate = ZDateTime.Now;

			Factory.Save();
		}

		protected override void AssertRecordHasBeenUpdated()
		{
			ZQuery sQLFilter = new ZQuery();
			sQLFilter.AddToFilter(JobVesselScheduleSchema.EV_TerminalID, "CTLPB");
			sQLFilter.AddToFilter(JobVesselScheduleSchema.EV_IMOLloydsNumber, "1234548");
			sQLFilter.AddToFilter(JobVesselScheduleSchema.EV_ShipOperatorVoyageIn, "999S");
			sQLFilter.AddToFilter(JobVesselScheduleSchema.EV_ShipOperatorVoyageOut, "999N");
			sQLFilter.AddToFilter(JobVesselScheduleSchema.EV_LineOperator, "MOL");

			var updatedRecord = Factory.LoadTop1<JobVesselSchedule>(sQLFilter);
			AssertNotNull("Record should definitely exist", updatedRecord);
			AssertEquals("Terminal name has been updated", "AUSYD", updatedRecord.EV_RL_NKPortCode);
			AssertEquals("Discharge country has been updated", "COMMUNITY VESSEL", updatedRecord.EV_ShipName);
			AssertEquals("Discharge port name has been updated", "", updatedRecord.EV_ContainerVessel);
			AssertEquals("Discharge port state has been updated", "", updatedRecord.EV_VesselCode);

			ZDateTime eTA, eTD, cargoCutoff, reeferCutoff, exportCommencement;
			ZDateTime.TryParseExact("2005-12-23 22:00:00", out eTA, FileDateTimeFormat);
			ZDateTime.TryParseExact("2005-12-24 22:00:00", out eTD, FileDateTimeFormat);
			ZDateTime.TryParseExact("2005-12-24 22:00:00", out cargoCutoff, FileDateTimeFormat);
			ZDateTime.TryParseExact("2005-12-24 22:00:00", out reeferCutoff, FileDateTimeFormat);
			ZDateTime.TryParseExact("2004-11-10 22:00:00", out exportCommencement, FileDateTimeFormat);

			AssertEquals("ETA has been updated", eTA, updatedRecord.EV_ETA);
			AssertEquals("ETD has been updated", eTD, updatedRecord.EV_ETD);
			AssertEquals("Cargo Cutoff has been updated", cargoCutoff, updatedRecord.EV_CargoCuttOff);
			AssertEquals("Reefer Cutoff has been updated", reeferCutoff, updatedRecord.EV_ReeferCutOff);
			AssertEquals("Import Availability has been updated", ZDateTime.Empty, updatedRecord.EV_ImportAvailability);
			AssertEquals("Import Storage has been updated", ZDateTime.Empty, updatedRecord.EV_ImportStorageCommences);
			AssertEquals("ATA has been updated", ZDateTime.Empty, updatedRecord.EV_ActualArrival);
			AssertEquals("ATD has been updated", ZDateTime.Empty, updatedRecord.EV_ActualDeparture);
			AssertEquals("Operators description has been updated", "MITSUI OSK LINES (AUSTRALIA) P/L", updatedRecord.EV_OperatorsDescription);
			AssertEquals("Ship operators code has been updated", "POC", updatedRecord.EV_ShipOperatorsCode);
			AssertEquals("Export Commencement has been updated", exportCommencement, updatedRecord.EV_ExportReceivalCommencementDate);
		}

		MailItem CreateMailItem()
		{
			var mailItem = Factory.New<MailItem>();
			mailItem.MI_Status = MailStatus.Queued;
			mailItem.MI_Direction = MailDirection.Receive;
			mailItem.MI_SendDateTime = ZDateTime.Now;
			mailItem.MI_ReceivedDateTime = ZDateTime.Now;
			mailItem.AddRecipientForUserCommunication("testaddress@edi.net.au <testaddress@edi.net.au>", MailRecipient.RecipientTypes.TO);
			mailItem.MI_Subject = "Enterprise Vessel Schedule";
			mailItem.MI_From = "Eagle Vessel Schedule and Routing <1-stop@edi.net.au>";

			return mailItem;
		}

		/// <summary>
		///		Creates an attachment with default values and adds it to a <paramref name="mail"/>.
		/// </summary>
		/// <param name="mail">
		///		The mail in which to add an attachment.
		/// </param>
		/// <param name="numberOfParamsInRow">
		///		Indicates the number of vessel scheduler parameters to include into row. Use this parameter to simulate different specifications of the protocol.
		///		For example, if you specify 20 (old specification, without <paramref name="firstFreeImportDate"/>), only the parameters from <paramref name="uNLOCO"/> to
		///		<paramref name="vesselCode"/> will be included into row;
		/// </param>
		/// <returns>
		///		The created attachment. Use it for manual modifications.
		/// </returns>
		MailAttachment AddMailAttachment(
			MailItem mail,
			string uNLOCO = "AUSYD",
			string terminalID = "CTLPB",
			string eTA = "1912-04-14 00:02:00",
			string eTD = "1912-04-14 00:02:00",
			string shipName = "Titanic",
			string shipOperatorVoyageOut = "031N",
			string lloydsID = "8913681",
			string cargoCutoff = "1912-04-14 00:02:00",
			string reeferCutoff = "1912-04-14 00:02:00",
			string lineOperator = "ANZ",
			string operatorDescription = "Harland and Wolff",
			string shipOperatorCode = "ANZ",
			string shipOperatorVoyageIn = "031S",
			string exportRecivalCommencement = "1912-04-14 00:02:00",
			string importAvailability = "1912-04-14 00:02:00",
			string importStorage = "",
			string containerVessel = "",
			string actualArrival = "",
			string actualDepart = "",
			string vesselCode = "",
			string firstFreeImportDate = "",
			int numberOfParamsInRow = 21,
			string csvLines = "")
		{
			var parameters = new string[] { uNLOCO, terminalID, eTA, eTD, shipName, shipOperatorVoyageOut, lloydsID, cargoCutoff, reeferCutoff, lineOperator, operatorDescription, shipOperatorCode, shipOperatorVoyageIn,
											exportRecivalCommencement, importAvailability, importStorage, containerVessel, actualArrival, actualDepart, vesselCode, firstFreeImportDate };

			if (string.IsNullOrEmpty(csvLines))
			{
				// [v1, v2, v3] => '"v1","v2","v3"'
				var vesselScheduleRecord = string.Join(",", parameters.Cast<string>().Select(s => string.Format("\"{0}\"", s)).ToArray(), 0, numberOfParamsInRow);

				return AddMailAttachment(mail, vesselScheduleRecord);
			}
			else
			{
				return AddMailAttachment(mail, csvLines);
			}
		}

		MailAttachment AddMailAttachment(
			MailItem mail,
			string csvLines)
		{
			var fileName = Guid.NewGuid().ToString();
			var directoryToZip = Path.Combine(this.tempDirectory, "McLaren");
			var attachmentFileName = string.Format("{0}.tmp", fileName);
			var attachmentFilePath = Path.Combine(directoryToZip, attachmentFileName);
			var zipFileName = string.Format("{0}.zip", fileName);
			var zipFilePath = Path.Combine(this.tempDirectory, zipFileName);

			this.CreateDirectory(directoryToZip);
			File.WriteAllText(attachmentFilePath, csvLines);
			ZipFile.CreateFromDirectory(directoryToZip, zipFilePath, CompressionLevel.Fastest, false);

			var attachment = mail.MailAttachments.AddNew();
			attachment.MA_Data = File.ReadAllBytes(zipFilePath);
			attachment.MA_FileName = zipFileName;

			return attachment;
		}

		MailItem VesselScheduleMailItem;
		TempDirectory tempDirectory;
		const string FileDateTimeFormat = "yyyy-MM-dd HH:mm:ss";

		#endregion

		#region Test Class

		class VesselScheduleProcessorForTest : VesselScheduleProcessor
		{
			public VesselScheduleProcessorForTest()
				: base()
			{
				OriginalFactory = Factory;
			}

			public BusinessObjectFactory OriginalFactory { get; }

			public BusinessObjectFactory CurrentFactory => Factory;
		}

		#endregion
	}
}

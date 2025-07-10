using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Freight.SailingDataVendor.Shared;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.Test;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.SailingDataVendor.Business.Testing
{
	sealed class VesselRoutingProcessorTest : OneStopProcessorBaseTest
	{
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

			var mailItemData =
@"""TermCode"",""TerminalName"",""ShipName"",""LlydsID"",""VoyNum"",""DischargeCountry"",""DischargePortName"",""DPCde"",""DischargePortState""";
			var mailItem = InitialisedMailItem(mailItemData, tempDirectory);
			mailItem.ExtractAttachments();

			var processor = new VesselRoutingProcessor();

			AssertNoExceptionThrown(() => processor.ProcessMailItemForTest(mailItem));
			Assert(string.IsNullOrEmpty(ErrorReporter.LastKeyReported));
			Assert(string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

			mailItemData =
@"""TermCode"",""TerminalName"",""ShipName"",""LlydsID"",""VoyNum"",""DischargeCountry"",""DischargePortName"",""DPCde""";
			mailItem = InitialisedMailItem(mailItemData, tempDirectory);
			mailItem.ExtractAttachments();

			AssertNoExceptionThrown(() => processor.ProcessMailItemForTest(mailItem));

			if (code == "HYE")
			{
				AssertEquals("Invalid1StopVesselRoutingFileRecordLine", ErrorReporter.LastKeyReported);
				AssertContains("Invalid number of fields in a record.", ErrorReporter.LastMessageReported);
				AssertContains(mailItemData, ErrorReporter.LastMessageReported);
			}
			else
			{
				Assert(string.IsNullOrEmpty(ErrorReporter.LastKeyReported));
				Assert(string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
			}

			ErrorReporter.Clear();
		}

		[TestDate(2005, 3, 1)]
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

			SetupVesselSchedules();

			var mailItemData =
	@"""TermCode"",""TerminalName"",""ShipName"",""1234548"",""999S"",""DischargeCountry"",""DischargePortName"",""DPCde"",""DischargePortState - VALIDCSV""
""LongTerminalCode"",""TerminalName"",""ShipName"",""1234548"",""999S"",""DischargeCountry"",""DischargePortName"",""DPCde"",""DischargePortState - INVALIDCSV""
""TerminalCode"",""TerminalName"",""ShipName"",""1234548"",""999S"",""DischargeCountry"",""DischargePortName"",""DPCde"",""DischargePortState - INVALIDCSV""
""TerminalCode"",""TerminalName"",""ShipName"",""1234548"",""LongVoyageNum"",""DischargeCountry"",""DischargePortName"",""DPCde"",""DischargePortState - INVALIDCSV""
""TerminalCode"",""TerminalName"",""ShipName"",""1234548"",""999S"",""DischargeCountry"",""DischargePortName"",""LongDischargePortCode"",""DischargePortState - INVALIDCSV""";
			var mailItem = InitialisedMailItem(mailItemData, tempDirectory);
			mailItem.ExtractAttachments();

			Assert(string.IsNullOrEmpty(ErrorReporter.LastKeyReported));
			Assert(string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

			var processor = new VesselRoutingProcessor();
			AssertNoExceptionThrown(() => processor.ProcessMailItemForTest(mailItem));

			JobVesselRouting[] jobVesselRoutings = Factory.Load<JobVesselRouting>(new ZQuery(JobVesselRoutingSchema.E1_TerminalName, "TerminalName"));
			AssertEquals("Only one valid CSV record was used", 1, jobVesselRoutings.Length);

			if (code == "HYE")
			{
				AssertEquals("Invalid1StopVesselRoutingFileRecordLine", ErrorReporter.LastKeyReported);
				AssertContains("One or more identifying fields have exceeded the maximum length.", ErrorReporter.LastMessageReported);
				AssertContains("\"LongTerminalCode\",\"TerminalName\",\"ShipName\",\"1234548\",\"999S\",\"DischargeCountry\",\"DischargePortName\",\"DPCde\",\"DischargePortState - INVALIDCSV\"", ErrorReporter.LastMessageReported);
			}
			else
			{
				Assert(string.IsNullOrEmpty(ErrorReporter.LastKeyReported));
				Assert(string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
			}

			ErrorReporter.Clear();
		}

		[TestDate(2005, 3, 1)]
		public void TestProcessCSVLine_NonUniqueFieldsExceedMaxLength()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "HYE";

			SetupVesselSchedules();

			var mailItemData =
@"""TermCode0"",""LESS THAN 35 CHARACTERS"",""LESS THAN 35 CHARACTERS"",""1234548"",""999S"",""LESS THAN 35 CHARACTERS"",""LESS THAN 35 CHARACTERS"",""DPCde"",""LESSTHAN35CHARACTERS - VALIDCSV""
""TermCode1"",""TERMINAL NAME WITH MORE THAN 35 CHARACTERS"",""LESS THAN 35 CHARACTERS"",""1234548"",""999S"",""LESS THAN 35 CHARACTERS"",""LESS THAN 35 CHARACTERS"",""DPCde"",""LESSTHAN35CHARACTERS - INVALIDCSV""
""TermCode2"",""LESS THAN 35 CHARACTERS"",""SHIP NAME WITH A LOT MORE THAN 35 CHARACTERS"",""1234548"",""999S"",""LESS THAN 35 CHARACTERS"",""LESS THAN 35 CHARACTERS"",""DPCde"",""LESSTHAN35CHARACTERS - INVALIDCSV""
""TermCode5"",""LESS THAN 35 CHARACTERS"",""LESS THAN 35 CHARACTERS"",""1234548"",""999S"",""DISCHARGE COUNTRY WITH MORE THAN 35 CHARACTERS"",""LESS THAN 35 CHARACTERS"",""DPCde"",""LESSTHAN35CHARACTERS - INVALIDCSV""
""TermCode6"",""LESS THAN 35 CHARACTERS"",""LESS THAN 35 CHARACTERS"",""1234548"",""999S"",""LESS THAN 35 CHARACTERS"",""DISCHARGE PORTS NAME WITH MORE THAN 35 CHARACTERS"",""DPCde"",""LESSTHAN35CHARACTERS - INVALIDCSV""
""TermCode8"",""LESS THAN 35 CHARACTERS"",""LESS THAN 35 CHARACTERS"",""1234548"",""999S"",""LESS THAN 35 CHARACTERS"",""LESS THAN 35 CHARACTERS"",""DPCde"",""DISCHARGE PORT STATE WITH MORE THAN 35 CHARACTERS - INVALIDCSV""";
			var mailItem = InitialisedMailItem(mailItemData, tempDirectory);
			mailItem.ExtractAttachments();

			var startingRecordCount = Factory.GetDatabaseCount(typeof(JobVesselRouting));
			var processor = new VesselRoutingProcessor();

			AssertNoExceptionThrown(() => processor.ProcessMailItemForTest(mailItem));
			AssertEquals("Number of records in table is correct", 6, Factory.GetDatabaseCount(typeof(JobVesselRouting)) - startingRecordCount);

			ZQuery query = new ZQuery();
			query.AddToFilter(JobVesselRoutingSchema.E1_LloydsID, "1234548");
			query.AddToFilter(JobVesselRoutingSchema.E1_VoyageNumber, "999S");
			query.AddToFilter(JobVesselRoutingSchema.E1_RL_NKDischargePortCode, "DPCde");
			query.AddToFilter(JobVesselRoutingSchema.E1_EV, null);

			JobVesselRouting[] routings = Factory.Load<JobVesselRouting>(query);
			var indexesToCheck = new List<int> { 0, 1, 2, 5, 6, 8 };

			foreach (JobVesselRouting routing in routings)
			{
				indexesToCheck.ForEach(x => AssertLengths(routing, x));
			}

			registrationKey.EnterpriseCodeForTest = "ABC";

			AssertNoExceptionThrown(() => processor.ProcessMailItemForTest(mailItem));
			AssertEquals("Number of records in table is correct", 6, Factory.GetDatabaseCount(typeof(JobVesselRouting)) - startingRecordCount);

			routings = Factory.Load<JobVesselRouting>(query);

			foreach (JobVesselRouting routing in routings)
			{
				indexesToCheck.ForEach(x => AssertLengths(routing, x));
			}
		}

		public void TestProcessCSV_DbHits()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "HYE";

			ZStringBuilder zStringBuilder = new ZStringBuilder();

			string mailItemDataFunction(int code)
			{
				return @"""" + code + @""",""LESS THAN 35 CHARACTERS"",""LESS THAN 35 CHARACTERS"",""LlydsID"",""VoyNum"",""LESS THAN 35 CHARACTERS"",""LESS THAN 35 CHARACTERS"",""DPCde"",""LESSTHAN35CHARACTERS - VALIDCSV""";
			}

			for (int i = 0; i < 5; i++)
			{
				zStringBuilder.AppendLine(mailItemDataFunction(i));
			}

			var mailItem = InitialisedMailItem(zStringBuilder.ToString(), tempDirectory);
			mailItem.ExtractAttachments();

			var expectedHits = new Dictionary<string, int>
			{
				{ "JobVesselRouting", 1 }
			};

			ZStringBuilder stringBuilder = new ZStringBuilder();

			var processor = new VesselRoutingProcessor();

			using (AssertDbHitsForAllFactories(expectedHits, ignoreUnspecified: true))
			{
				AssertNoExceptionThrown(() => processor.ProcessMailItemForTest(mailItem));
			}
		}

		public void TestRemoveRecordsWithoutMatchingSchedules()
		{
			var routing1 = Factory.New<JobVesselRouting>();
			routing1.E1_LloydsID = "0000001";
			routing1.E1_VoyageNumber = "EDI";

			var routing2 = Factory.New<JobVesselRouting>();
			routing2.E1_LloydsID = "0000002";
			routing2.E1_VoyageNumber = "EDI";

			var routing3 = Factory.New<JobVesselRouting>();
			routing3.E1_LloydsID = "0000001";
			routing3.E1_VoyageNumber = "XYZ";

			var schedule = Factory.New<JobVesselSchedule>();
			schedule.EV_ShipOperatorVoyageIn = "EDI";
			schedule.EV_IMOLloydsNumber = "0000001";
			schedule.EV_DataProvider = "1ST";
			schedule.EV_ActualArrival = ZDateTime.Now.AddMonths(-2);

			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "HYE";

			var mailItemData = @"""TermCode"",""TerminalName"",""ShipName"",""LlydsID"",""VoyNum"",""DischargeCountry"",""DischargePortName"",""DPCde"",""DischargePortState""";
			var mailItem = InitialisedMailItem(mailItemData, tempDirectory);
			mailItem.ExtractAttachments();

			Factory.Save();

			var logger = new Mock<ILogger>();
			var processor = new VesselRoutingProcessor();

			processor.ProcessVesselRouting(mailItem.PK, logger.Object);

			var newFactory = new BusinessObjectFactory();
			AssertNotNull("routing1: with matching schedule", newFactory.Load<JobVesselRouting>(routing1.PK));
			AssertNull("routing2: without matching schedule", newFactory.Load<JobVesselRouting>(routing2.PK));
			AssertNull("routing3: without matching schedule", newFactory.Load<JobVesselRouting>(routing3.PK));
		}

		void AssertLengths(JobVesselRouting jobVesselRouting, int trimmedValueIndex)
		{
			var valueArray = new ZPropertyInfo[]
			{
				jobVesselRouting.E1_TerminalCodeInfo,
				jobVesselRouting.E1_TerminalNameInfo,
				jobVesselRouting.E1_ShipNameInfo,
				jobVesselRouting.E1_LloydsIDInfo,
				jobVesselRouting.E1_VoyageNumberInfo,
				jobVesselRouting.E1_DischargeCountryInfo,
				jobVesselRouting.E1_DischargePortNameInfo,
				jobVesselRouting.E1_RL_NKDischargePortCodeInfo,
				jobVesselRouting.E1_DischargePortStateInfo
			};
			var trimmedValue = valueArray[trimmedValueIndex];

			new List<int> { 1, 2, 5, 6, 8 }.Where(x => x != trimmedValueIndex).Select(x => valueArray[x]).ToList().ForEach(x => Assert(string.Format("Value of {0} should not have been changed", x.Name), ((ZString)x.Value).Length <= x.MaxLength));
			Assert(string.Format("Value of {0} should have been truncated at 35 characters", trimmedValue.Name), trimmedValue != 0 || trimmedValue.Value.ToString().Length == trimmedValue.MaxLength);
		}

		MailItem InitialisedMailItem(string mailItemStringData, TempDirectory tempDirectory)
		{
			var mailItem = Factory.New<MailItem>();
			Func<string, string> createFilePath = fileExtension => Path.Combine(tempDirectory, string.Format("1_2.{0}", fileExtension));

			var zipFilePath = createFilePath("zip");
			var textPath = createFilePath("txt");
			var tmpPath = createFilePath("tmp");

			using (var fs = new FileStream(zipFilePath, FileMode.Create))
			using (var zipArchive = new ZipArchive(fs, ZipArchiveMode.Create))
			{
				File.WriteAllText(tmpPath, mailItemStringData);
				zipArchive.CreateEntryFromFile(tmpPath, Path.GetFileName(tmpPath));
			}

			using (StreamWriter writer = new StreamWriter(textPath))
			{
				writer.Write(Convert.ToBase64String(File.ReadAllBytes(zipFilePath)));
			}

			mailItem.MI_Status = MailStatus.Queued;
			mailItem.MI_Direction = MailDirection.Receive;
			mailItem.MI_ReceivedDateTime = ZDateTime.Now;
			mailItem.MI_SendDateTime = ZDateTime.Now;
			using (var embeddedResourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				mailItem.MI_Header = embeddedResourceRetriever.GetString("Enterprise.Freight.SailingScheduleDataVendor.Test.AU_NZ.TestFiles.VesselRoutingEmailHeader.txt");
			}
			mailItem.MI_Body = File.ReadAllText(textPath);
			mailItem.MI_Subject = "Enterprise Vessel Routing";

			return mailItem;
		}

		[TestDate(2005, 3, 1)]
		public void TestMessageFilter()
		{
			SetupVesselSchedules();

			vesselRoutingMailItem.ExtractAttachments();
			int startingRecordCount = Factory.GetDatabaseCount(typeof(JobVesselRouting));
			SetupTestData();
			AssertEquals("Number of records to start with is correct", 2, Factory.GetDatabaseCount(typeof(JobVesselRouting)) - startingRecordCount);
			Factory.Save();

			var helper = new MessageFilterTestHelper<VesselRoutingProcessor, MailItem>();
			Assert(helper.Process(vesselRoutingMailItem));
			AssertEquals("Number of records in table is correct", 34, Factory.GetDatabaseCount(typeof(JobVesselRouting)) - startingRecordCount);
			AssertRecordHasBeenUpdated();
			AssertEquals(2, helper.Log.Count);
			AssertEquals("Information|1-STOP : 33 records have been added to the JobVesselRouting table", helper.Log[0]);
			AssertEquals("Information|1-STOP : 1 records have been updated in the JobVesselRouting table", helper.Log[1]);
		}

		[TestDate(2005, 3, 1)]
		public override void TestProcessWithEmptyTable()
		{
			SetupVesselSchedules();

			vesselRoutingMailItem.ExtractAttachments();
			int startingRecordCount = Factory.GetDatabaseCount(typeof(JobVesselRouting));
			var processor = new VesselRoutingProcessorForTest();
			processor.ProcessMailItemForTest(vesselRoutingMailItem);
			AssertEquals("Number of records in table is correct", 34, Factory.GetDatabaseCount(typeof(JobVesselRouting)) - startingRecordCount);

			var allJobVesselRoutings = Factory.Load<JobVesselRouting>(new ZQuery());
			AssertFactoryContains(processor.OriginalFactory, allJobVesselRoutings);
			AssertFactoryNotContains(processor.CurrentFactory, allJobVesselRoutings);
		}

		[TestDate(2005, 3, 1)]
		public override void TestProcessWithSomeDataAlreadyInTable()
		{
			SetupVesselSchedules();

			vesselRoutingMailItem.ExtractAttachments();
			int startingRecordCount = Factory.GetDatabaseCount(typeof(JobVesselRouting));
			SetupTestData();
			AssertEquals("Number of records to start with is correct", 2, Factory.GetDatabaseCount(typeof(JobVesselRouting)) - startingRecordCount);

			var processor = new VesselRoutingProcessorForTest();
			processor.ProcessMailItemForTest(vesselRoutingMailItem);
			AssertEquals("Number of records in table is correct", 35, Factory.GetDatabaseCount(typeof(JobVesselRouting)) - startingRecordCount);
			AssertRecordHasBeenUpdated();

			var allJobVesselRoutings = Factory.Load<JobVesselRouting>(new ZQuery());
			AssertFactoryContains(processor.OriginalFactory, allJobVesselRoutings);
			AssertFactoryNotContains(processor.CurrentFactory, allJobVesselRoutings);
		}

		public void TestProcessInvalidFile()
		{
			vesselRoutingMailItem.MI_Body = System.Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes("splaty"));
			vesselRoutingMailItem.ExtractAttachments();
			VesselScheduleProcessor processor = new VesselScheduleProcessor();
			try
			{
				processor.ProcessMailItemForTest(vesselRoutingMailItem);
			}
			catch
			{
			}
			AssertEquals("Mail item should be marked as failed", MailStatus.Failed, vesselRoutingMailItem.MI_Status);
		}

		[TestDate(2005, 3, 1)]
		public void TestProcessForeignKeyIsEmpty()
		{
			SetupVesselSchedules();

			vesselRoutingMailItem.ExtractAttachments();
			int startingRecordCount = Factory.GetDatabaseCount(typeof(JobVesselRouting));

			var processor = new VesselRoutingProcessor();
			processor.ProcessMailItemForTest(vesselRoutingMailItem);
			AssertEquals("Number of records in table is correct", 34, Factory.GetDatabaseCount(typeof(JobVesselRouting)) - startingRecordCount);
			foreach (var routing in Factory.Load<JobVesselRouting>(new ZQuery()))
			{
				AssertEquals("E1_EV (FK to JobVesselSchedule)", ZGuid.Empty, routing.E1_EV);
			}
		}

		public void TestProcessCSVLine_InvalidMailItemPk()
		{
			var processor = new VesselRoutingProcessor();
			Assert(!processor.ProcessVesselRouting(ZGuid.Empty, null));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			vesselRoutingMailItem = Factory.New<MailItem>();
			vesselRoutingMailItem.MI_Status = MailStatus.Queued;
			vesselRoutingMailItem.MI_Direction = MailDirection.Receive;
			vesselRoutingMailItem.MI_ReceivedDateTime = ZDateTime.Now;
			vesselRoutingMailItem.MI_SendDateTime = ZDateTime.Now;
			using (var embeddedResourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				vesselRoutingMailItem.MI_Header = embeddedResourceRetriever.GetString("Enterprise.Freight.SailingScheduleDataVendor.Test.AU_NZ.TestFiles.VesselRoutingEmailHeader.txt");
				vesselRoutingMailItem.MI_Body = embeddedResourceRetriever.GetString("Enterprise.Freight.SailingScheduleDataVendor.Test.AU_NZ.TestFiles.VesselRoutingEmailBody.txt");
			}
			vesselRoutingMailItem.MI_Subject = "Enterprise Vessel Routing";

			this.tempDirectory = new TempDirectory();
		}

		protected override void TearDown()
		{
			base.TearDown();

			this.tempDirectory.Dispose();
		}

		MailItem vesselRoutingMailItem;
		TempDirectory tempDirectory;

		void SetupVesselSchedules()
		{
			var vesselScheduleMailItem = VesselScheduleProcessorTest.GetVesselScheduleMailItem(Factory);
			vesselScheduleMailItem.ExtractAttachments();

			Factory.Save();

			var helper = new MessageFilterTestHelper<VesselScheduleProcessor, MailItem>();
			helper.Process(vesselScheduleMailItem);
		}

		protected override void SetupTestData()
		{
			JobVesselRouting test1 = Factory.New<JobVesselRouting>();
			test1.E1_TerminalCode = "CTLPB";
			test1.E1_LloydsID = "1234548";
			test1.E1_VoyageNumber = "999N";
			test1.E1_RL_NKDischargePortCode = "GBTIL";
			test1.E1_TerminalName = "PATRICK, NS, PORT BOTANY";
			test1.E1_ShipName = "MSC SARISKA";
			test1.E1_DischargeCountry = "NEW ZEALAND";
			test1.E1_DischargePortName = "TAURANGA";
			test1.E1_DischargePortState = "BAY OF PLENTY";

			JobVesselRouting test2 = Factory.New<JobVesselRouting>();
			test2.E1_TerminalCode = "THIS";
			test2.E1_LloydsID = "SHOULD";
			test2.E1_VoyageNumber = "NOT";
			test2.E1_RL_NKDischargePortCode = "MATCH";
			test2.E1_TerminalName = "ANY";
			test2.E1_ShipName = "TEST";
			test2.E1_DischargeCountry = "RECORDS";
			test2.E1_DischargePortName = "HOPEFULLY";
			test2.E1_DischargePortState = "YEAH";

			Factory.Save();
		}

		protected override void AssertRecordHasBeenUpdated()
		{
			ZQuery sqlFilter = new ZQuery();
			sqlFilter.AddToFilter(JobVesselRoutingSchema.E1_TerminalCode, "CTLPB");
			sqlFilter.AddToFilter(JobVesselRoutingSchema.E1_LloydsID, "1234548");
			sqlFilter.AddToFilter(JobVesselRoutingSchema.E1_VoyageNumber, "999N");
			sqlFilter.AddToFilter(JobVesselRoutingSchema.E1_RL_NKDischargePortCode, "GBTIL");

			var updatedRecord = Factory.LoadTop1<JobVesselRouting>(sqlFilter);
			AssertNotNull("Record should definately exist", updatedRecord);
			AssertEquals("Terminal name has been updated", "P&O PORTS, PORT BOTANY", updatedRecord.E1_TerminalName);
			AssertEquals("Ship name has been updated", "COMMUNITY VESSEL", updatedRecord.E1_ShipName);
			AssertEquals("Discharge country has been updated", "UNITED KINGDOM", updatedRecord.E1_DischargeCountry);
			AssertEquals("Discharge port name has been updated", "TILBURY", updatedRecord.E1_DischargePortName);
			AssertEquals("Discharge port state has been updated", "ESSEX", updatedRecord.E1_DischargePortState);
		}

		#endregion

		#region Test Class

		class VesselRoutingProcessorForTest : VesselRoutingProcessor
		{
			public VesselRoutingProcessorForTest()
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

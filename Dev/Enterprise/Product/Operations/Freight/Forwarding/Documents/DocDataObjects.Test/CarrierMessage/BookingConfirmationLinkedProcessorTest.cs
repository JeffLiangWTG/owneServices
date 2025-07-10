using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	sealed class BookingConfirmationLinkedProcessorTest : TestCaseWithFactory
	{
		#region TestCreateEvent

		public void TestCreateEvent_UnsavedLog()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var docType = GetBookingConfirmationDocType();
			docType.RT_SE_NKDocumentReceivedEvent = Events.BookingConfirmedCode;
			docType.RT_LogMacro = "Hello <typeof(@data).Name>";

			var message = Factory.CreateBookingConfirmationMessage();
			message.EM_Status = EDIMessageStatusList.Codes.Queued;

			Factory.Save();

			var log = Factory.New<StmALog>();

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = Events.DataLinkedCode;
				log.SL_Parent = consol.PK;
				log.SL_Table = JobConsolSchema.Constants.TableName;
			}

			message.AddUniversalDataLink(log);

			Factory.Save();

			var consolBookingConfirmedLogs = GetLogs(consol, Events.BookingConfirmedCode);
			AssertEquals("BKC event was created", 1, consolBookingConfirmedLogs.Length);
			AssertEquals("Log reference was created from macro", "Hello ForwardingConsol", consolBookingConfirmedLogs[0].SL_Reference);
			Assert("Log set to defer fire workflow", consolBookingConfirmedLogs[0].SL_FireWorkflow);
		}

		public void TestCreateEvent_SavedLog()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var docType = GetBookingConfirmationDocType();
			docType.RT_SE_NKDocumentReceivedEvent = Events.BookingConfirmedCode;
			docType.RT_LogMacro = "Hello <typeof(@data).Name>";

			var message = Factory.CreateBookingConfirmationMessage();
			message.EM_Status = EDIMessageStatusList.Codes.Queued;

			Factory.Save();

			var log = Factory.New<StmALog>();

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = Events.DataLinkedCode;
				log.SL_Parent = consol.PK;
				log.SL_Table = JobConsolSchema.Constants.TableName;

				message.AddUniversalDataLink(log);

				Factory.Save();
			}

			var consolBookingConfirmedLogs = GetLogs(consol, Events.BookingConfirmedCode);
			AssertEquals("BKC event was created", 1, consolBookingConfirmedLogs.Length);
			AssertEquals("Log reference was created from macro", "Hello ForwardingConsol", consolBookingConfirmedLogs[0].SL_Reference);
			Assert("Log set to defer fire workflow", consolBookingConfirmedLogs[0].SL_FireWorkflow);
		}

		public void TestDoNotCreateEvent()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var docType = GetBookingConfirmationDocType();
			docType.RT_SE_NKDocumentReceivedEvent = string.Empty;

			Factory.Save();

			var log = Factory.New<StmALog>();

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = Events.DataLinkedCode;
				log.SL_Parent = consol.PK;
				log.SL_Table = JobConsolSchema.Constants.TableName;
			}

			Factory.Save();

			var consolBookingConfirmedLogs = GetLogs(consol, Events.BookingConfirmedCode);
			AssertEquals("BKC event was not created", 0, consolBookingConfirmedLogs.Length);
		}

		#endregion

		public void TestCreateEDoc()
		{
			TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var docType = GetBookingConfirmationDocType();
			docType.RT_SE_NKDocumentReceivedEvent = ZString.Empty;
			docType.RT_LogSystemCreatedDocsToEDocs = true;

			var message = Factory.CreateBookingConfirmationMessage();
			message.EM_Status = EDIMessageStatusList.Codes.Queued;

			Factory.Save();

			var log = Factory.New<StmALog>();

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = Events.DataLinkedCode;
				log.SL_Parent = consol.PK;
				log.SL_Table = JobConsolSchema.Constants.TableName;
			}

			message.AddUniversalDataLink(log);

			void AssertPrintJobs(string assertMessage, int expectedShipmentPrintJobs)
			{
				var printJobsQuery = new ZQuery();
				printJobsQuery.AddToFilter(StmPrintJobSchema.SP_ParentTableName, JobConsolSchema.Constants.TableName);
				printJobsQuery.AddToFilter(StmPrintJobSchema.SP_ParentGuid, consol.PK);

				var printJobs = Factory.Load<StmPrintJob>(printJobsQuery);
				AssertEquals(assertMessage, expectedShipmentPrintJobs, printJobs.Length);
			}

			AssertPrintJobs("no print jobs were created before factory save", 0);

			Factory.Save();

			AssertPrintJobs("one print job was created after factory save", 1);
		}

		public void TestDoNotCreateEDocOnSave()
		{
			TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var docType = GetBookingConfirmationDocType();
			docType.RT_SE_NKDocumentReceivedEvent = ZString.Empty;
			docType.RT_LogSystemCreatedDocsToEDocs = false;

			var message = Factory.CreateBookingConfirmationMessage();
			message.EM_Status = EDIMessageStatusList.Codes.Queued;

			Factory.Save();

			var log = Factory.New<StmALog>();

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = Events.DataLinkedCode;
				log.SL_Parent = consol.PK;
				log.SL_Table = JobConsolSchema.Constants.TableName;
			}

			message.AddUniversalDataLink(log);

			void AssertPrintJobs(string assertMessage, int expectedShipmentPrintJobs)
			{
				var printJobsQuery = new ZQuery();
				printJobsQuery.AddToFilter(StmPrintJobSchema.SP_ParentTableName, JobConsolSchema.Constants.TableName);
				printJobsQuery.AddToFilter(StmPrintJobSchema.SP_ParentGuid, consol.PK);

				var printJobs = Factory.Load<StmPrintJob>(printJobsQuery);
				AssertEquals(assertMessage, expectedShipmentPrintJobs, printJobs.Length);
			}

			AssertPrintJobs("no print jobs were created before factory save", 0);

			Factory.Save();

			AssertPrintJobs("print job was not created after factory save", 0);
		}

		StmALog[] GetLogs(IStmALogParent logParent, ZString eventCode)
		{
			return logParent
				.Logs
				.GetAllLogs()
				.Cast<StmALog>()
				.Where(l => l.SL_SE_NKEvent == eventCode)
				.ToArray();
		}

		RefDocType GetBookingConfirmationDocType()
		{
			var menuItem = GetBookingConfirmationMenuItem();

			return menuItem
				?.Documents
				.Cast<StmMenuTemplatePivot>()
				.Single()
				.DocType;
		}

		StmMenuItemBase GetBookingConfirmationMenuItem()
		{
			var bookingConfirmationPK = new ZGuid("913BDCEC-A6F5-4A30-84E9-E5941484BDEA");
			return Factory.Load<StmMenuItemBase>(bookingConfirmationPK);
		}
	}
}

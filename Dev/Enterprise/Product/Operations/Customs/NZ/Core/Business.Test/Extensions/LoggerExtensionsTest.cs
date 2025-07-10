using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.Express.Testing
{
	using CargoWise.EntityFramework.Testing;
	using Enterprise.Customs.NZ.Business.Declaration;
	using Enterprise.Freight.Forwarding.Business;

	public class LoggerExtensionsTest : TestCaseWithFactory
	{
		public void TestLogCustomsCompletedIfRequired()
		{
			var logProvider = Factory.New<JobDeclaration>() as IStmALogProvider;

			logProvider.LogCustomsEventsIfRequired(TSWEntryStatusList.Codes.CCP, TSWEntryStatusList.Codes.CCI, "agency A", MessageTypeList.Codes.ICR, ZString.Empty, isExport: false, "NZ Import");
			var impedimentLog = logProvider.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("HLD - agency A", impedimentLog[0].SL_Reference);

			var clearedLog = logProvider.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("Not a cleared status", 0, clearedLog.Length);

			logProvider.LogCustomsEventsIfRequired(TSWEntryStatusList.Codes.CCI, TSWEntryStatusList.Codes.CCC, "agency B", MessageTypeList.Codes.IPI, ZString.Empty, isExport: false, "NZ Import");
			impedimentLog = logProvider.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("CLR - agency B", impedimentLog[1].SL_Reference);

			clearedLog = logProvider.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("IPI is not logged", 0, clearedLog.Length);

			logProvider.LogCustomsEventsIfRequired(TSWEntryStatusList.Codes.CCI, TSWEntryStatusList.Codes.CCC, "agency C", MessageTypeList.Codes.ICR, ZString.Empty, isExport: false, "NZ Import");
			impedimentLog = logProvider.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("CLR - agency C", impedimentLog[2].SL_Reference);

			clearedLog = logProvider.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("NZ Import", clearedLog[0].SL_Reference);
		}

		public void TestLogCustomsCompletedIfRequired_ParentProvider()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00003434";
			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "S00006565";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;

			var logProvider = declaration as IStmALogProvider;
			var parentProvider = (logProvider as IStmALogParentProvider).LogParent;
			AssertSame(parentProvider, shipment);

			logProvider.LogCustomsEventsIfRequired(TSWEntryStatusList.Codes.CCP, TSWEntryStatusList.Codes.CCI, "agency A", MessageTypeList.Codes.ICR, ZString.Empty, isExport: false, "NZ Import");
			var impedimentLog = logProvider.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("HLD - agency A", impedimentLog[0].SL_Reference);

			var clearedLog = logProvider.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("Not a completed status", 0, clearedLog.Length);

			logProvider.LogCustomsEventsIfRequired(TSWEntryStatusList.Codes.CCI, TSWEntryStatusList.Codes.CCC, "agency B", MessageTypeList.Codes.IPI, ZString.Empty, isExport: false, "NZ Import");
			impedimentLog = logProvider.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("CLR - agency B", impedimentLog[1].SL_Reference);

			var clearedLogProvider = logProvider.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("IPI is not logged", 0, clearedLogProvider.Length);

			var clearedLogParent = parentProvider.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("IPI is not logged", 0, clearedLogParent.Length);

			logProvider.LogCustomsEventsIfRequired(TSWEntryStatusList.Codes.CCI, TSWEntryStatusList.Codes.CCC, "agency C", MessageTypeList.Codes.ICR, ZString.Empty, isExport: false, "NZ Import");
			impedimentLog = logProvider.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("CLR - agency C", impedimentLog[2].SL_Reference);

			clearedLogProvider = logProvider.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("CLR event not logged against provider when there is a parent", 0, clearedLogProvider.Length);

			clearedLogParent = parentProvider.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("NZ Import", clearedLogParent[0].SL_Reference);
		}

		public void TestEventLogCreatedForCodeAndReference()
		{
			/*
			 *	Customs clearance CLR logs are not being created if user has already manually created or had a workflow create a CLR event within a log collection.
			 *	create a log collection with a dummy CLR event.
			 *	process a new custom cleared CLR event.
			 *	Assert the customs cleared CLR event exists in the logs.
			 */

			var logProvider = Factory.New<JobDeclaration>() as IStmALogProvider;
			logProvider.Logs.AddNew(Events.CustomsCleared, "Some Dummy CLR Log", ZDateTimeOffset.Today.AddDays(-1));
			var clearedLogs = logProvider.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("Pre-condition: Dummy CLR log exists", 1, clearedLogs.Length);

			var expectedLogReference = "NZ Import";
			logProvider.LogCustomsEventsIfRequired(TSWEntryStatusList.Codes.PCC, TSWEntryStatusList.Codes.CCC, "NZCS", MessageTypeList.Codes.I10, ZString.Empty, isExport: false, expectedLogReference);
			clearedLogs = logProvider.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("Customs Cleared event log has been added", 2, clearedLogs.Length);

			var customsCLRQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code);
			customsCLRQuery.AddToFilter(StmALogSchema.SL_Reference, "NZ Import");
			var customsLog = logProvider.Logs.Find(customsCLRQuery);
			AssertEquals("CLR event log should have been created for NZ Import", expectedLogReference, customsLog[0].SL_Reference);
		}

		public void TestLogCustomsStatusDOR()
		{
			var logProvider = Factory.New<JobDeclaration>() as IStmALogProvider;
			logProvider.LogCustomsEventsIfRequired(TSWEntryStatusList.Codes.CCP, TSWEntryStatusList.Codes.CCI, "agency A", "", ZString.Empty, isExport: false, "NZ Import");
			var impedimentLog = logProvider.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("HLD - agency A", impedimentLog[0].SL_Reference);

			var clearedLog = logProvider.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("Not a cleared status", 0, clearedLog.Length);

			logProvider.LogCustomsEventsIfRequired(TSWEntryStatusList.Codes.CCI, TSWEntryStatusList.Codes.CCC, "agency B", "", ZString.Empty, isExport: false, "NZ Import");
			impedimentLog = logProvider.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("CLR - agency B", impedimentLog[1].SL_Reference);

			logProvider.LogCustomsEventsIfRequired(TSWEntryStatusList.Codes.CCI, TSWEntryStatusList.Codes.CCC, "agency C", "", ZString.Empty, isExport: false, "NZ Import");
			impedimentLog = logProvider.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("CLR - agency C", impedimentLog[2].SL_Reference);

			clearedLog = logProvider.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("NZ Import", clearedLog[0].SL_Reference);

			var entryStatusDORLog = logProvider.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsEntryStatus.Code));
			AssertEquals("DOR", entryStatusDORLog[0].SL_Reference);
		}

		public void TestLogCustomsStatusWOF()
		{
			var logProvider = Factory.New<JobDeclaration>() as IStmALogProvider;
			logProvider.LogCustomsEventsIfRequired(LowValueConsignmentStatusList.Codes.PP, LowValueConsignmentStatusList.Codes.PH, "agency A", TSWEntryStatusList.EntryTypes.WriteOff, ZString.Empty, isExport: false, "NZ Import");
			var impedimentLog = logProvider.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("HLD - agency A", impedimentLog[0].SL_Reference);

			var clearedLog = logProvider.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("Not a cleared status", 0, clearedLog.Length);

			logProvider.LogCustomsEventsIfRequired(LowValueConsignmentStatusList.Codes.PH, LowValueConsignmentStatusList.Codes.CH, "agency B", TSWEntryStatusList.EntryTypes.WriteOff, ZString.Empty, isExport: false, "NZ Import");

			logProvider.LogCustomsEventsIfRequired(LowValueConsignmentStatusList.Codes.CH, LowValueConsignmentStatusList.Codes.CC, "agency A", TSWEntryStatusList.EntryTypes.WriteOff, ZString.Empty, isExport: false, "NZ Import");
			impedimentLog = logProvider.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("CLR - agency A", impedimentLog[1].SL_Reference);

			clearedLog = logProvider.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("NZ Import", clearedLog[0].SL_Reference);

			var entryStatusDORLog = logProvider.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsEntryStatus.Code));
			AssertEquals("WOF", entryStatusDORLog[0].SL_Reference);
		}

		public void TestLogCustomsEventsIfRequired_Performance()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			Factory.RowsLoaded += Factory_RowsLoaded;
			declaration.LogCustomsEventsIfRequired(TSWEntryStatusList.Codes.CCI, TSWEntryStatusList.Codes.CLR, "agency A", MessageTypeList.Codes.ICR, ZString.Empty, isExport: false, "A");
			Factory.RowsLoaded -= Factory_RowsLoaded;
			AssertEquals("CLR/A Event not exists, in Cache", 1, declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code)).Length);

			Factory.RowsLoaded += Factory_RowsLoaded;
			declaration.LogCustomsEventsIfRequired(TSWEntryStatusList.Codes.CCI, TSWEntryStatusList.Codes.CLR, "agency A", MessageTypeList.Codes.ICR, ZString.Empty, isExport: false, "A");
			Factory.RowsLoaded -= Factory_RowsLoaded;
			AssertEquals("CLR/A Event exists, in Cache", 1, declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code)).Length);

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var logProvider = anotherFactory.Load<JobDeclaration>(declaration.PK) as IStmALogProvider;
			anotherFactory.RowsLoaded += Factory_RowsLoaded;
			logProvider.LogCustomsEventsIfRequired(TSWEntryStatusList.Codes.CCI, TSWEntryStatusList.Codes.CLR, "agency A", MessageTypeList.Codes.ICR, ZString.Empty, isExport: false, "A");
			anotherFactory.RowsLoaded -= Factory_RowsLoaded;
			AssertEquals("CLR/A Event exists, in DB", 1, logProvider.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code)).Length);

			anotherFactory.RowsLoaded += Factory_RowsLoaded;
			logProvider.LogCustomsEventsIfRequired(TSWEntryStatusList.Codes.CCI, TSWEntryStatusList.Codes.CLR, "agency A", MessageTypeList.Codes.ICR, ZString.Empty, isExport: false, "B");
			anotherFactory.RowsLoaded -= Factory_RowsLoaded;
			AssertEquals("CLR/B Event not exists, in DB", 2, logProvider.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code)).Length);

			anotherFactory.RowsLoaded += Factory_RowsLoaded;
			logProvider.LogCustomsEventsIfRequired(TSWEntryStatusList.Codes.CCI, TSWEntryStatusList.Codes.CLR, "agency A", MessageTypeList.Codes.ICR, ZString.Empty, isExport: false, "B");
			anotherFactory.RowsLoaded -= Factory_RowsLoaded;
			AssertEquals("CLR/B Event exists, in DB", 2, logProvider.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code)).Length);
		}

		public void TestCIPEventRecordsForDifferentAgencies()
		{
			var logProvider = Factory.New<JobDeclaration>() as IStmALogProvider;
			logProvider.LogCustomsEventsIfRequired(LowValueConsignmentStatusList.Codes.PP, LowValueConsignmentStatusList.Codes.PH, TSWMessage.ResponseMessage.MessageSubTypes.MPIFood, TSWEntryStatusList.EntryTypes.WriteOff, ZString.Empty, isExport: false, "NZ Import");
			var impedimentLogs = logProvider.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("HLD - MPI", impedimentLogs[0].SL_Reference);

			logProvider.LogCustomsEventsIfRequired(TSWEntryStatusList.Codes.CPP, TSWEntryStatusList.Codes.CII, TSWMessage.ResponseMessage.MessageSubTypes.MPIBIO, TSWEntryStatusList.EntryTypes.WriteOff, ZString.Empty, isExport: false, "NZ Import");
			impedimentLogs = logProvider.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("log count", 2, impedimentLogs.Length);
			AssertEquals("HLD - MPI", impedimentLogs[0].SL_Reference);
			AssertEquals("Impediment log for MPI Biosecurity should generate even though the impediment log for MPI Food already exists", "HLD - BIO", impedimentLogs[1].SL_Reference);
		}

		void Factory_RowsLoaded(object sender, RowsLoadedEventArgs e)
		{
			AssertNotEquals($"Should not load StmALog, but {e.Rows.Length} StmALog row(s) loaded", StmALogSchema.Constants.TableName, e.TableOrViewName);
		}
	}
}

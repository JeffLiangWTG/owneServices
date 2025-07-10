using System;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.MessageDelivery.Testing
{
	sealed class MessageDataExportImportLogLinkerTest : TestCaseWithFactory
	{
		public void TestLinkMessageToParentBOLogs()
		{
			var dummyBO = (BusinessObject)Factory.New<Forwarding.IForwardingConsol>();
			var parentBOWithLogs = dummyBO as IStmALogParent;
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			parentBOWithLogs.Logs.AddNew(Events.EditedARecord);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			var message = Factory.New<IEDIMessage>();

			var linker = new MessageDataExportImportLogLinker(Events.DataExport, Factory);
			linker.LinkMessageToParentBOLogs(message, parentBOWithLogs);
			var dataExportLog = parentBOWithLogs.Logs.MostRecentLogByEventTime(Events.DataExport);
			(new GenPivotTestHelper()).AssertPivotExists(StmALogSchema.Constants.Prefix, dataExportLog.PK, EDIMessageSchema.Constants.Prefix, message.PK, Enterprise.Core.Constants.GenPivotTypes.XmlEdiMessage, Factory);

			Thread.Sleep(100);
			var addedLog = parentBOWithLogs.Logs.AddNew(Events.DataExport);
			linker.LinkMessageToParentBOLogs(message, (IStmALogParent)dummyBO);
			(new GenPivotTestHelper()).AssertPivotExists(StmALogSchema.Constants.Prefix, addedLog.PK, EDIMessageSchema.Constants.Prefix, message.PK, Enterprise.Core.Constants.GenPivotTypes.XmlEdiMessage, Factory);
		}

		public void TestConstructor()
		{
			AssertExceptionThrown(typeof(ArgumentException), "Only Events.DataExport or Events.DataImport can be accepted as argument.", delegate
			{ new MessageDataExportImportLogLinker(Events.EditedARecord, Factory); });
			AssertExceptionThrown(typeof(ArgumentException), "Argument 'factory' can not be null.", delegate
			{ new MessageDataExportImportLogLinker(Events.DataExport, null); });
		}

		public void TestTriggerPurposeIsPopulatedOnEventReference()
		{
			var purpose = Factory.New<IEDIMessagePurpose>();
			purpose.EMP_Code = "XXX";
			purpose.EMP_Description = "XXX Desc";

			Factory.Save();

			AssertExportEventWithTriggerPurpose(ZString.Empty, ZString.Empty);

			AssertExportEventWithTriggerPurpose("XXX", "Purpose: XXX - XXX Desc");
			AssertExportEventWithTriggerPurpose("YYY", "Purpose: YYY");
		}

		public void TestLinkMessageToParentBOLogs_EventReferenceIsIncludedInMatching()
		{
			var logParent = Factory.New<Forwarding.IForwardingConsol>() as IStmALogParent;
			var dexLog1 = logParent.Logs.AddNew(Events.DataExport, "Purpose: Reference ONE");
			var dexLog2 = logParent.Logs.AddNew(Events.DataExport, "Purpose: Reference TWO");
			var dexLog3 = logParent.Logs.AddNew(Events.DataExport, "");

			var message = Factory.New<IEDIMessage>();

			var linker = new MessageDataExportImportLogLinker(Events.DataExport, Factory, "Reference TWO");
			linker.LinkMessageToParentBOLogs(message, logParent);

			var linkedLog = FindLinkedLog(message, logParent);
			AssertEquals("Correct DEX log have been used", dexLog2, linkedLog);

			var anotherMessage = Factory.New<IEDIMessage>();

			linker = new MessageDataExportImportLogLinker(Events.DataExport, Factory, "Something Something");
			linker.LinkMessageToParentBOLogs(anotherMessage, logParent);

			var newLinkedLog = FindLinkedLog(anotherMessage, logParent);
			AssertNotNull("New event log have been created", newLinkedLog);
			AssertEquals("Correct reference", "Purpose: Something Something", newLinkedLog.SL_Reference);
		}

		StmALog FindLinkedLog(IEDIMessage message, IStmALogParent logParent)
		{
			var pivotQuery = new ZQuery();
			pivotQuery.AddToFilter(GenPivotSchema.XX_RelationType, Enterprise.Core.Constants.GenPivotTypes.XmlEdiMessage);
			pivotQuery.AddToFilter(GenPivotSchema.XX_Relation1TableCode, StmALogSchema.Constants.Prefix);
			pivotQuery.AddToFilter(GenPivotSchema.XX_Relation2TableCode, EDIMessageSchema.Constants.Prefix);
			pivotQuery.AddToFilter(GenPivotSchema.XX_Relation2ID, message.PK);

			var pivot = Factory.LoadTop1<GenPivot>(pivotQuery);
			AssertNotNull("Pivot exists", pivot);

			var logQuery = new ZQuery();
			logQuery.AddToFilter(StmALogSchema.PK, pivot.XX_Relation1ID);
			logQuery.AddToFilter(StmALogSchema.SL_Parent, logParent.LogsParentPK);
			logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.DataExport.Code);

			return Factory.LoadTop1<StmALog>(logQuery);
		}

		void AssertExportEventWithTriggerPurpose(ZString triggerPurpose, ZString expected)
		{
			var dummyBO = (BusinessObject)Factory.New<Forwarding.IForwardingConsol>();
			var parentBOWithLogs = dummyBO as IStmALogParent;
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			parentBOWithLogs.Logs.AddNew(Events.EditedARecord);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

			var message = Factory.New<IEDIMessage>();

			var linker = new MessageDataExportImportLogLinker(Events.DataExport, Factory, triggerPurpose);
			linker.LinkMessageToParentBOLogs(message, parentBOWithLogs);
			var dataExportLog = parentBOWithLogs.Logs.MostRecentLogByEventTime(Events.DataExport);

			AssertNotNull(dataExportLog);
			AssertEquals("Event Reference should populate the trigger purpose", expected, dataExportLog.SL_Reference);
		}
	}
}

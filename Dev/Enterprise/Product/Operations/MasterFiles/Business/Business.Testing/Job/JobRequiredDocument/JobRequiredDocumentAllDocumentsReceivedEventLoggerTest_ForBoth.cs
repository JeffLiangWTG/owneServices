using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(JobRequiredDocumentAllDocumentsReceivedEventLoggerForImport))]
	sealed class JobRequiredDocumentAllDocumentsReceivedEventLoggerTest_ForBoth : JobRequiredDocumentAllDocumentsReceivedEventLoggerTest<JobRequiredDocumentAllDocumentsReceivedEventLoggerForImport>
	{
		protected override JobRequiredDocumentAllDocumentsReceivedEventLoggerForImport NewCollectionEventLogger()
		{
			return new JobRequiredDocumentAllDocumentsReceivedEventLoggerForImport(Parent);
		}

		protected override KeyValuePair<SchemaDateTimeOffsetColumn, Event>[] DatePropertiesTrackedAsEventsOnParent
		{
			get
			{
				var result = new List<KeyValuePair<SchemaDateTimeOffsetColumn, Event>>();
				result.Add(new KeyValuePair<SchemaDateTimeOffsetColumn, Event>(JobRequiredDocumentSchema.EQ_DateReceived, Events.AllImportDocumentsReceived));
				result.Add(new KeyValuePair<SchemaDateTimeOffsetColumn, Event>(JobRequiredDocumentSchema.EQ_DateReceived, Events.AllExportDocumentsReceived));
				return result.ToArray();
			}
		}

		protected override ZString ImportOrExport
		{
			get { return JobRequiredDocument.DocUsage.Both; }
		}

		public void TestEventAlreadyLoggedCheckOnlyLoadTop1()
		{
			Parent.Logs.AddNew(Events.AllImportDocumentsReceived);
			Parent.Factory.Save();
			JobRequiredDocumentAllDocumentsReceivedEventLogger.LogEventOnParentsOnSave(Parent);
			var buildIndexesSql = SqlEventTracker.Instance.LastSqlQuery;
			AssertContains("FROM dbo.StmALog", buildIndexesSql);
			AssertContains("SELECT  TOP 1", buildIndexesSql);
			AssertContains("ORDER BY SL_EventTime DESC", buildIndexesSql);
		}

		public void TestJobRequiredDocumentNotThrownExceptionIfIStmALogParentIsNull()
		{
			var declaration = Factory.New<Enterprise.Integration.Customs.AU.IJobDeclaration>();
			var docsAndCartage = (Enterprise.Integration.Freight.IJobDocsAndCartage)declaration["DocsAndCartage"];
			var oldPk = docsAndCartage.JP_ParentID;
			docsAndCartage.JP_ParentID = ZGuid.Empty;

			var requiredDocumentsParent = ((IDocsAndCartageParent)declaration).RequiredDocumentsProvider;
			var requiredDocumentToUpdate = requiredDocumentsParent.RequiredDocuments.AddNew();
			requiredDocumentToUpdate.EQ_DateReceived = ZDateTimeOffset.Now;

			docsAndCartage.JP_ParentID = oldPk;
			AssertNoExceptionThrown(() => Factory.Save());
		}
	}
}

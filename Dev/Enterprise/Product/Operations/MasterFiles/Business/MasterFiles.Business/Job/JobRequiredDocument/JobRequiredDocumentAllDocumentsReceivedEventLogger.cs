using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.JobRequiredDocument;

namespace Enterprise.MasterFiles.Business
{
	internal abstract class JobRequiredDocumentAllDocumentsReceivedEventLogger : CollectionEventLogger
	{
		protected JobRequiredDocumentAllDocumentsReceivedEventLogger(IHaveRequiredDocuments parent, ZString importExport)
			: base(GetStmALogParent(parent), GetDocuments(parent, importExport))
		{
			requiredDocumentsParent = parent;
		}

		readonly IHaveRequiredDocuments requiredDocumentsParent;

		protected override void OnBeforeLogEvent()
		{
			requiredDocumentsParent.PreLogAllDocumentsReceivedEvents();
		}

		public static void LogEventOnParentsOnSave(IHaveRequiredDocuments parent) //d
		{
			if (parent != null && GetStmALogParent(parent) != null)
			{
				if (HasRequiredDocumentWithNoReceivedDate(parent, DocUsage.Import) || !EventAlreadyLogged(parent, Events.AllImportDocumentsReceived))
				{
					CollectionEventLoggerService.LogEventOnParentOnSave((BusinessObject)parent, delegate
					{
						return new JobRequiredDocumentAllDocumentsReceivedEventLoggerForImport(parent);
					});
				}

				if (HasRequiredDocumentWithNoReceivedDate(parent, DocUsage.Export) || !EventAlreadyLogged(parent, Events.AllExportDocumentsReceived))
				{
					CollectionEventLoggerService.LogEventOnParentOnSave((BusinessObject)parent, delegate
					{
						return new JobRequiredDocumentAllDocumentsReceivedEventLoggerForExport(parent);
					});
				}
			}
		}

		static bool HasRequiredDocumentWithNoReceivedDate(IHaveRequiredDocuments parent, string usage)
		{
			var query = new ZQuery(JobRequiredDocumentSchema.EQ_DateReceived, null);
			var usageQuery = new ZQuery(JobRequiredDocumentSchema.EQ_DocUsage, usage);
			usageQuery.AddToFilter(JoinCondition.Or, JobRequiredDocumentSchema.EQ_DocUsage, DocUsage.Both);
			query.AddToFilter(usageQuery);
			return parent.RequiredDocuments.Find(query).Length > 0;
		}

		static bool EventAlreadyLogged(IHaveRequiredDocuments parent, Event @event)
		{
			var parentLogs = parent.UltimateDocumentParent == null ? parent.Logs : parent.UltimateDocumentParent.GetLogs();
			var targetLog = parentLogs.MostRecentLogByEventTime(@event);
			return targetLog != null;
		}

		static IStmALogParent GetStmALogParent(IHaveRequiredDocuments parent) => (IStmALogParent)parent.UltimateDocumentParent ?? parent as IStmALogParent;

		protected static IEnumerable GetDocuments(IHaveRequiredDocuments parent, ZString importOrExport)
		{
			parent.RequiredDocuments.Load();
			foreach (JobRequiredDocument document in parent.RequiredDocuments)
			{
				if (document.EQ_DocUsage == importOrExport || document.EQ_DocUsage == DocUsage.Both)
				{
					yield return document;
				}
			}
		}

		protected override bool ShouldCreateOrCancelEvents()
		{
			var hasItem = false;
			var hasAddedEDoc = false;
			foreach (JobRequiredDocument item in Collection)
			{
				hasItem = true;
				if (item.Origin == JRDOrigin.FromRequirements)
				{
					return true;
				}

				// If there is a requiredDocument is not in database and created by AddedEDoc, then we just skip it and need not do anything
				if (!item.IsInDatabase && item.Origin == JRDOrigin.AddedEDoc)
				{
					hasAddedEDoc = true;
				}
			}

			if (hasAddedEDoc || SystemDataRegistry.Instance.RestrictAEDAndAIDEvents.Value)
			{
				return false;
			}

			return hasItem;
		}
	}
}

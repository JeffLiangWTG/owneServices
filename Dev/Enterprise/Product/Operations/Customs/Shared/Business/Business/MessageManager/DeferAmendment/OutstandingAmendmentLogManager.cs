using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business
{
	public class OutstandingAmendmentLogManager
	{
		public OutstandingAmendmentLogManager(BusinessObject bizObj)
		{
			this.bizObj = bizObj;
		}

		#region HasOutstandingAmendments

		public bool HasOutstandingAmendments
		{
			get { return AllAmendmentLogs.Count > 0; }
		}
		public bool HasOutstandingAmendmentsNotQueued
		{
			get { return AllAmendmentLogsNotQueued.Count > 0; }
		}
		public bool HasOutstandingManualAmendments
		{
			get { return AllManualAmendmentLogs.Count > 0; }
		}
		public bool HasOutstandingFailedAmendments
		{
			get { return AllAmendmentRejectedLogs.Count > 0; }
		}
		public bool HasConsolidatedEntryChanges
		{
			get
			{
				var result = ConsolidatedEntryChangedLogs.Count > 0;

				if (bizObj is BaseJobDeclaration declaration)
				{
					result |= ConsolidatedDeclaration.GetConsolidatedDeclaration(declaration)?.HasConsolidatedEntryChanges ?? false;
				}

				return result;
			}
		}

		#endregion

		#region CancelAllOutstandingAmendments

		public void CancelAllOutstandingAmendments()
		{
			AllAmendmentLogs.CancelAll();
			AllAmendmentLogsNotQueued.CancelAll();
			CancelConsolidatedEntryChangedLogs();
		}

		void CancelConsolidatedEntryChangedLogs()
		{
			ConsolidatedEntryChangedLogs.CancelAll();

			if (bizObj is BaseJobDeclaration declaration)
			{
				var consolidatedEntry = ConsolidatedDeclaration.GetConsolidatedDeclaration(declaration);
				if (consolidatedEntry?.CRD_JE_LeadDeclaration == declaration.PK)
				{
					consolidatedEntry.OutstandingAmendmentLogManager.CancelAllOutstandingAmendments();
				}
			}
		}

		#endregion

		#region AddANewOutstandingAmendmentLog

		public StmALog AddANewOutstandingAmendmentLog(ZString reference)
		{
			return AllAmendmentLogs.AddNew(reference);
		}

		public StmALog AddANewOutstandingAmendmentLog(ZString reference, ZDateTime eventTime)
		{
			return AllAmendmentLogs.AddNew(reference, eventTime.ToOffset());
		}

		public StmALog AddANewLogForSaveWithoutEntryChanges()
		{
			return AddANewLogForSaveWithoutEntryChanges(ZDateTime.Empty);
		}

		public StmALog AddANewLogForSaveWithoutEntryChanges(ZDateTime eventTime)
		{
			return SavedWithoutEntryChangesLogs.AddNew("Saved without sending amendment and no entry changes", eventTime.ToOffset());
		}

		public StmALog AddRejectedAmendmentLog()
		{
			return AllAmendmentRejectedLogs.AddNew("Amendment rejected, no changes lodged with Customs");
		}

		#endregion

		#region AllOutstandingAmendmentDetailsIncludingEventTimes
		public ZString AllOutstandingAmendmentDetailsIncludingEventTimes
		{
			get
			{
				if (allOutstandingAmendmentDetailsIncludingEventTimesCached == null)
				{
					allOutstandingAmendmentDetailsIncludingEventTimesCached = new CachedProperty<ZString>(bizObj.Factory, GetAllOutstandingAmendmentDetailsIncludingEventTimes);
				}
				return allOutstandingAmendmentDetailsIncludingEventTimesCached.Value;
			}
		}
		CachedProperty<ZString> allOutstandingAmendmentDetailsIncludingEventTimesCached;

		ZString GetAllOutstandingAmendmentDetailsIncludingEventTimes()
		{
			ZStringBuilder result = new ZStringBuilder();
			foreach (StmALog aLog in AllAmendmentLogs)
			{
				if (!aLog.SL_IsCancelled)
				{
					result.Append(aLog.SL_EventTime.ToString());
					result.Append(": ");
					result.Append(aLog.SL_Reference);
					result.Append("\r\n");
				}
			}
			return result.ToString();
		}

		#endregion

		#region AllOustandingAmendmentReferences
		public ZString AllOustandingAmendmentReferences
		{
			get
			{
				if (allOutstadingAmendmentReferencesCached == null)
				{
					allOutstadingAmendmentReferencesCached = new CachedProperty<ZString>(bizObj.Factory, GetAllOustandingAmendmentReferences);
				}
				return allOutstadingAmendmentReferencesCached.Value;
			}
		}
		CachedProperty<ZString> allOutstadingAmendmentReferencesCached;

		ZString GetAllOustandingAmendmentReferences()
		{
			var result = new ZStringBuilder();
			foreach (StmALog aLog in AllAmendmentLogs)
			{
				if (!aLog.SL_IsCancelled && !aLog.SL_Reference.IsEmpty)
				{
					result.Append(aLog.SL_Reference);
				}
			}
			return result.ToStringWithNewLineBetweenAppends();
		}

		#endregion

		#region AllAmendmentLogs

		public LogsForNominatedEvent AllAmendmentLogs
		{
			get
			{
				if (fAllOutstandingAmendments == null)
				{
					fAllOutstandingAmendments = new LogsForNominatedEvent(bizObj.GetLogs(), Events.DeclarationAmendmentQueued);
				}
				return fAllOutstandingAmendments;
			}
		}
		LogsForNominatedEvent fAllOutstandingAmendments;

		public LogsForNominatedEvent AllAmendmentLogsNotQueued
		{
			get
			{
				if (fAllOutstandingAmendmentsNotQueued == null)
				{
					fAllOutstandingAmendmentsNotQueued = new LogsForNominatedEvent(bizObj.GetLogs(), Events.DeclarationAmendedPermitApproved);
				}
				return fAllOutstandingAmendmentsNotQueued;
			}
		}
		LogsForNominatedEvent fAllOutstandingAmendmentsNotQueued;

		public LogsForNominatedEvent AllManualAmendmentLogs
		{
			get
			{
				if (fAllOutstandingManualAmendments == null)
				{
					fAllOutstandingManualAmendments = new LogsForNominatedEvent(bizObj.GetLogs(), Events.ManualMatchDone);
				}
				return fAllOutstandingManualAmendments;
			}
		}
		LogsForNominatedEvent fAllOutstandingManualAmendments;

		public LogsForNominatedEvent AllAmendmentRejectedLogs
		{
			get
			{
				if (fAllOutstandingAmendmentsRejected == null)
				{
					fAllOutstandingAmendmentsRejected = new LogsForNominatedEvent(bizObj.GetLogs(), Events.DeclarationAmendmentRejected);
				}
				return fAllOutstandingAmendmentsRejected;
			}
		}
		LogsForNominatedEvent fAllOutstandingAmendmentsRejected;

		public LogsForNominatedEvent SavedWithoutEntryChangesLogs
		{
			get
			{
				if (fSavedWithoutEntryChanges == null)
				{
					fSavedWithoutEntryChanges = new LogsForNominatedEvent(bizObj.GetLogs(), Events.DeclarationAmendedPermitApproved);
				}
				return fSavedWithoutEntryChanges;
			}
		}
		LogsForNominatedEvent fSavedWithoutEntryChanges;

		public LogsForNominatedEvent ConsolidatedEntryChangedLogs
		{
			get
			{
				if (fConsolidatedEntryChangedLogs == null)
				{
					fConsolidatedEntryChangedLogs = new LogsForNominatedEvent(bizObj.GetLogs(), Events.ConsolidatedEntryChanged);
				}
				return fConsolidatedEntryChangedLogs;
			}
		}
		LogsForNominatedEvent fConsolidatedEntryChangedLogs;

		readonly BusinessObject bizObj;

		#endregion
	}
}

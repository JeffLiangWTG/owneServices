using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public abstract class CommissionAgreementModifiedLogsManager<T> where T : BusinessObject, ICommissionAgreementRelated<T>, IStmALogParent
	{
		protected CommissionAgreementModifiedLogsManager(T draft)
		{
			Argument.NotNull(draft, "draft");

			this.Draft = draft;
		}

		readonly T Draft;

		protected T MainVersion
		{
			get { return mainVersion ?? (mainVersion = Draft.GetMainVersion()); }
		}
		T mainVersion;

		protected BusinessObjectFactory Factory
		{
			get { return Draft.Factory; }
		}

		protected T OldSource
		{
			get { return Draft.IsInDatabase ? Draft : MainVersion; }
		}

		protected T NewSource
		{
			get { return Draft; }
		}

		#region Change Log

		public void AddLogIfChanged(Func<T, ZPropertyInfo> getPropertyInfo, bool changeCanBeAutoApproved)
		{
			var oldSource = OldSource;
			var newSource = NewSource;
			var newPropertyInfo = getPropertyInfo(newSource);
			if (oldSource != newSource || newPropertyInfo.HasChanges)
			{
				var originalValue = getPropertyInfo(oldSource).OriginalValue;
				var newValue = newPropertyInfo.PersistentValue;
				if (!originalValue.Equals(newValue))
				{
					AddChangeLog(newPropertyInfo.HumanReadableName, originalValue.ToString(), newValue.ToString(), changeCanBeAutoApproved);
				}
			}
		}

		public void AddLogIfChanged(Func<T, ZPropertyInfo> getPropertyInfo, bool changeCanBeAutoApproved, Func<IZType, string> propertyToStringValue)
		{
			var oldSource = OldSource;
			var newSource = NewSource;
			var newPropertyInfo = getPropertyInfo(newSource);
			if (oldSource != newSource || newPropertyInfo.HasChanges)
			{
				var originalValue = getPropertyInfo(oldSource).OriginalValue;
				var newValue = newPropertyInfo.PersistentValue;
				if (!originalValue.Equals(newValue))
				{
					AddChangeLog(newPropertyInfo.HumanReadableName, propertyToStringValue(originalValue), propertyToStringValue(newValue), changeCanBeAutoApproved);
				}
			}
		}

		public StmALog AddChangeLog(string description, string originalValue, string newValue, bool changeCanBeAutoApproved)
		{
			var reference = BuildChangeLogReference(description, originalValue, newValue);
			return AddChangeLog(reference, changeCanBeAutoApproved);
		}

		public StmALog AddChangeLog(string reference, bool changeCanBeAutoApproved, bool isOnDeleteLog = false)
		{
			var log = MainVersion.Logs.AddNew(Events.StatusChange, reference);
			ModifiedLogPks.Add(log.PK);
			if (isOnDeleteLog)
			{
				OnDeleteLogPks.Add(log.PK);
			}

			if (!changeCanBeAutoApproved)
			{
				NewSource.CommissionAgreement.NotifyHasChangedPreventingAutoApproval();
			}

			return log;
		}

		static string BuildChangeLogReference(string description, string originalValue, string newValue)
		{
			return string.Format("{0}: {1} > {2}", description, originalValue, newValue);
		}

		#endregion

		#region Child Logs

		public StmALog AddChildObjectAttachedLog(BusinessObject childObj)
		{
			return AddChangeLog(Res.GetString("6352a3d3-c408-4eda-82b7-adbaf27ced2f", "Attached {0}", childObj.HumanReadableName), AutoApprovalHelper.OnlyIfNotEffectiveInThePast(OldSource.CommissionAgreement));
		}

		public StmALog AddChildObjectDetachedLog(BusinessObject childObj)
		{
			return AddChangeLog(Res.GetString("d30bb292-35f5-48fb-a6f3-73ba56f336c1", "Detached {0}", childObj.HumanReadableName), AutoApprovalHelper.OnlyIfNotEffectiveInThePast(OldSource.CommissionAgreement), true);
		}

		#endregion

		#region Logs on Factory Saving

		public void AddLogsOnFactorySaving()
		{
			if (ShouldAddLogsOnFactorySaving)
			{
				AddLogsOnFactorySavingCore();
			}
		}

		ZBool ShouldAddLogsOnFactorySaving
		{
			get
			{
				if (NewSource.IsDeleted || NewSource.IsUncommittedDraft())
				{
					return false;
				}

				if (!HasChangesThatCanCreateLogs)
				{
					return false;
				}

				if (!NewSource.IsDraft())
				{
					return false;
				}

				if (NewSource.CommissionAgreement == null || NewSource.CommissionAgreement.HasDraft)
				{
					return false;
				}

				return true;
			}
		}

		protected virtual ZBool HasChangesThatCanCreateLogs
		{
			get { return !NewSource.IsInDatabase || NewSource.HasChangesNotIncludingChildren; }
		}

		protected abstract void AddLogsOnFactorySavingCore();

		public void OnFactorySaved(bool saveSucceeded)
		{
			if (!saveSucceeded)
			{
				foreach (var log in MainVersion.Logs.LogsNotInDB)
				{
					if (ModifiedLogPks.Contains(log.PK) && !OnDeleteLogPks.Contains(log.PK))
					{
						log.Delete();
						ModifiedLogPks.Remove(log.PK);
					}
				}
			}
			else
			{
				ModifiedLogPks.Clear();
				OnDeleteLogPks.Clear();
			}
		}

		#endregion

		#region Logs on Delete

		public void AddDetachedLogOnDelete()
		{
			if (ShouldAddDetachedLogOnDelete)
			{
				AddDetachedLogOnDeleteCore();
			}
		}

		ZBool ShouldAddDetachedLogOnDelete
		{
			get
			{
				if (!NewSource.IsDraft())
				{
					return false;
				}

				if (!MainVersion.IsInDatabase || MainVersion.IsDeleted || MainVersion.CommissionAgreement == null)
				{
					return false;
				}

				if (NewSource.IsMainVersion())
				{
					return true;
				}
				else
				{
					return
						!NewSource.CommissionAgreement.IsDeleted &&
						MainVersion.CommissionAgreement == NewSource.CommissionAgreement.ParentVersion;
				}
			}
		}

		protected abstract void AddDetachedLogOnDeleteCore();

		#endregion

		#region Implementation

		readonly HashSet<ZGuid> ModifiedLogPks = new HashSet<ZGuid>();
		readonly HashSet<ZGuid> OnDeleteLogPks = new HashSet<ZGuid>();

		#endregion
	}

	public static class AutoApprovalHelper
	{
		public static bool Always
		{
			get { return true; }
		}

		public static bool Never
		{
			get { return false; }
		}

		public static bool OnlyIfNotEffectiveInThePast(OrgCommissionAgreement commissionAgreement)
		{
			return commissionAgreement == null || commissionAgreement.EffectiveDate.IsEmpty || commissionAgreement.EffectiveDate > ZDate.Today;
		}

		public static bool OnlyIfNoDatesInThePast(params ZDate[] dates)
		{
			return dates.All(date => date.IsEmpty || date > ZDate.Today);
		}
	}
}

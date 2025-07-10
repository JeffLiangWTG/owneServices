using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	#region ProcessQueueType

	public abstract class ProcessQueueType
	{
		public enum Enum
		{
			Commercial,
			Customs
		}

		public const string Commercial = "COM";
		public const string Customs = "CUS";
	}

	#endregion

	public class ProcessQueue : AutoProcessQueue
	{
		public ProcessQueue(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Business Object Overrides

		public override void OnSaving()
		{
			base.OnSaving();
			if (CustomsQueueHasChanges)
			{
				CustomsQueueLogs.AddNew();
			}

			if (CommercialQueueHasChanges)
			{
				CommercialQueueLogs.AddNew();
			}
		}

		#endregion

		#region Property Overrides

		public override ZString P4_CustomsQueue
		{
			get { return base.P4_CustomsQueue; }
			set
			{
				base.P4_CustomsQueue = value;
				SetDependentProperty(P4_CustomsQueueInfo);
			}
		}

		public override ZString P4_CustomsStatus
		{
			get { return base.P4_CustomsStatus; }
			set
			{
				base.P4_CustomsStatus = value;
				SetDependentProperty(P4_CustomsStatusInfo);
			}
		}

		public override ZString P4_QueueName
		{
			get { return base.P4_QueueName; }
			set
			{
				base.P4_QueueName = value;
				SetDependentProperty(P4_QueueNameInfo);
			}
		}

		public override ZString P4_Status
		{
			get { return base.P4_Status; }
			set
			{
				base.P4_Status = value;
				SetDependentProperty(P4_StatusInfo);
			}
		}

		void SetDependentProperty(ZPropertyInfo info)
		{
			if (IsInDatabase && info.Value.Equals(info.OriginalValue))
			{
				ResetToOriginalValues(info);
			}
			else
			{
				PopulateDependentPropertyFromList(info);
			}
		}

		void ResetToOriginalValues(ZPropertyInfo info)
		{
			switch (info.Name)
			{
				case Schema.P4_CustomsQueue:
					P4_CustomsStatus = (ZString)P4_CustomsStatusInfo.OriginalValue;
					break;

				case Schema.P4_CustomsStatus:
					P4_CustomsSubStatus = (ZString)P4_CustomsSubStatusInfo.OriginalValue;
					break;

				case Schema.P4_QueueName:
					P4_Status = (ZString)P4_StatusInfo.OriginalValue;
					break;

				case Schema.P4_Status:
					P4_SubStatus = (ZString)P4_SubStatusInfo.OriginalValue;
					break;
			}
		}

		void PopulateDependentPropertyFromList(ZPropertyInfo info)
		{
			switch (info.Name)
			{
				case Schema.P4_CustomsQueue:
					SetDefaultForCustomsStatusAndCustomsSubStatus();
					break;

				case Schema.P4_CustomsStatus:
					P4_CustomsSubStatus = (Lookups.CustomsSubStatusList.Count == 1) ? Lookups.CustomsSubStatusList[0].Code : "";
					break;

				case Schema.P4_QueueName:
					P4_SubStatus = ZString.Empty;
					P4_Status = (Lookups.CommercialStatusList.Count == 1) ? Lookups.CommercialStatusList[0].Code : "";
					break;

				case Schema.P4_Status:
					P4_SubStatus = (Lookups.CommercialSubStatusList.Count == 1) ? Lookups.CommercialSubStatusList[0].Code : "";
					break;
			}
		}

		protected virtual void SetDefaultForCustomsStatusAndCustomsSubStatus()
		{
			P4_CustomsStatus = (Lookups.CustomsStatusList.Count == 1) ? Lookups.CustomsStatusList[0].Code : "";
			P4_CustomsSubStatus = ZString.Empty;
		}

		#endregion

		#region QueueLogs

		public ProcessQueueLogCollection CustomsQueueLogs
		{
			get
			{
				if (fCustomsQueueLogs == null)
				{
					fCustomsQueueLogs = GetNewQueueLogs(ProcessQueueType.Enum.Customs);
					fCustomsQueueLogs.Load();
					fCustomsQueueLogs.IsManagedForDataRefresh = true;
				}
				return fCustomsQueueLogs;
			}
		}

		public ProcessQueueLogCollection CommercialQueueLogs
		{
			get
			{
				if (fCommercialQueueLogs == null)
				{
					fCommercialQueueLogs = GetNewQueueLogs(ProcessQueueType.Enum.Commercial);
					fCommercialQueueLogs.Load();
					fCommercialQueueLogs.IsManagedForDataRefresh = true;
				}
				return fCommercialQueueLogs;
			}
		}

		protected virtual ProcessQueueLogCollection GetNewQueueLogs(ProcessQueueType.Enum queueType)
		{
			return new ProcessQueueLogCollection(this, queueType);
		}

		public bool IsCustomsQueueLogsLoaded
		{
			get { return fCustomsQueueLogs != null && fCustomsQueueLogs.IsLoaded; }
		}

		public bool IsCommercialQueueLogsLoaded
		{
			get { return fCommercialQueueLogs != null && fCommercialQueueLogs.IsLoaded; }
		}

		ProcessQueueLogCollection fCustomsQueueLogs;
		ProcessQueueLogCollection fCommercialQueueLogs;

		#endregion

		#region QueueHasChanges

		public bool CustomsQueueHasChanges
		{
			get { return CustomsQueueHasChangesFromDB || NewCustomsQueueHasValues; }
		}

		public bool CommercialQueueHasChanges
		{
			get { return CommercialQueueHasChangesFromDB || NewCommercialQueueHasValues; }
		}

		bool CustomsQueueHasChangesFromDB
		{
			get
			{
				return IsInDatabase &&
					(P4_CustomsQueueInfo.HasChanges ||
					P4_CustomsStatusInfo.HasChanges ||
					P4_CustomsSubStatusInfo.HasChanges ||
					P4_CustomsReasonInfo.HasChanges ||
					P4_GS_NKCustomsTaskAssignedToInfo.HasChanges);
			}
		}

		bool NewCustomsQueueHasValues
		{
			get
			{
				return !IsInDatabase &&
					(!P4_CustomsQueue.IsEmpty ||
					!P4_CustomsStatus.IsEmpty ||
					!P4_CustomsSubStatus.IsEmpty ||
					!P4_CustomsReason.IsEmpty ||
					!P4_GS_NKCustomsTaskAssignedTo.IsEmpty);
			}
		}

		bool CommercialQueueHasChangesFromDB
		{
			get
			{
				return IsInDatabase &&
					(P4_QueueNameInfo.HasChanges ||
					P4_StatusInfo.HasChanges ||
					P4_SubStatusInfo.HasChanges ||
					P4_ReasonInfo.HasChanges ||
					P4_GS_NKTaskAssignedToInfo.HasChanges);
			}
		}

		bool NewCommercialQueueHasValues
		{
			get
			{
				return !IsInDatabase &&
					(!P4_QueueName.IsEmpty ||
					!P4_Status.IsEmpty ||
					!P4_SubStatus.IsEmpty ||
					!P4_Reason.IsEmpty ||
					!P4_GS_NKTaskAssignedTo.IsEmpty);
			}
		}

		#endregion

		public virtual IProcessQueueParent Parent
		{
			get { return fParent; }
			set
			{
				fParent = value;
				if (value != null)
				{
					P4_ParentTableCode = value.TablePrefix;
					P4_ParentID = value.PK;
				}
			}
		}

		IProcessQueueParent fParent;
	}
}

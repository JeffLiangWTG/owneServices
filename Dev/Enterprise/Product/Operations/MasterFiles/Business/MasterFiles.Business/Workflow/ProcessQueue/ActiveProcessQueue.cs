using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class ActiveProcessQueue : NonPersistentBusinessObject, IActiveProcessQueue, IObsoleteValidation
	{
		public static ActiveProcessQueue New(ProcessQueue processQueue)
		{
			ActiveProcessQueue result;
			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(processQueue);
			}
			else
			{
				result = new ActiveProcessQueue(processQueue);
			}
			return result;
		}

		protected ActiveProcessQueue(ProcessQueue processQueue) : base(processQueue.Factory)
		{
			InitialiseQueueType();
			this.ProcessQueue = processQueue;
		}

		#region IActiveProcessQueue Members

		#region QueueName
		[List("Lookups.QueueList")]
		public ZString QueueName
		{
			get { return (ZString)InnerQueueNameInfo.Value; }
			set { InnerQueueNameInfo.Value = value; }
		}

		public ZPropertyInfo QueueNameInfo
		{
			get { return InnerQueueNameInfo == null ? null : GetWrappedZPropertyInfo(nameof(QueueName), x => InnerQueueNameInfo); }
		}

		public virtual MultilingualString QueueNameCaption
		{
			get { return ResString.GetMultilingualString("5edc0f1f-77ab-499e-86c7-2388d244436e", "Queue"); }
		}

		public ZPropertyInfo QueueNameCaptionInfo
		{
			get { return GetZPropertyInfo(nameof(QueueNameCaption)); }
		}

		ZPropertyInfo InnerQueueNameInfo
		{
			get { return ProcessQueue == null ? null : (QueueType == ProcessQueueType.Enum.Customs) ? ProcessQueue.P4_CustomsQueueInfo : ProcessQueue.P4_QueueNameInfo; }
		}

		#endregion

		#region Status
		[List("Lookups.StatusList")]
		public ZString Status
		{
			get { return (ZString)InnerStatusInfo.Value; }
			set { InnerStatusInfo.Value = value; }
		}

		public ZPropertyInfo StatusInfo
		{
			get { return InnerStatusInfo == null ? null : GetWrappedZPropertyInfo(nameof(Status), x => InnerStatusInfo); }
		}

		public virtual MultilingualString StatusCaption
		{
			get { return ResString.GetMultilingualString("742322f4-c500-45b4-a82c-f6e67d218065", "Status"); }
		}

		public ZPropertyInfo StatusCaptionInfo
		{
			get { return GetZPropertyInfo(nameof(StatusCaption)); }
		}

		ZPropertyInfo InnerStatusInfo
		{
			get { return ProcessQueue == null ? null : (QueueType == ProcessQueueType.Enum.Customs) ? ProcessQueue.P4_CustomsStatusInfo : ProcessQueue.P4_StatusInfo; }
		}

		#endregion

		#region SubStatus

		[List("Lookups.SubStatusList")]
		public ZString SubStatus
		{
			get { return (ZString)InnerSubStatusInfo.Value; }
			set { InnerSubStatusInfo.Value = value; }
		}

		public ZPropertyInfo SubStatusInfo
		{
			get { return InnerSubStatusInfo == null ? null : GetWrappedZPropertyInfo(nameof(SubStatus), x => InnerSubStatusInfo); }
		}

		public virtual MultilingualString SubStatusCaption
		{
			get { return ResString.GetMultilingualString("209b52a2-1b40-4a02-9285-0591e236575b", "Sub Status"); }
		}

		public ZPropertyInfo SubStatusCaptionInfo
		{
			get { return GetZPropertyInfo(nameof(SubStatusCaption)); }
		}

		ZPropertyInfo InnerSubStatusInfo
		{
			get { return ProcessQueue == null ? null : (QueueType == ProcessQueueType.Enum.Customs) ? ProcessQueue.P4_CustomsSubStatusInfo : ProcessQueue.P4_SubStatusInfo; }
		}

		public bool HasSubStatuses
		{
			get { return Lookups.SubStatusList.Count > 0; }
		}

		#endregion

		#region AssignedTo
		[List("Lookups.TaskAssignedToList")]
		public ZString AssignedTo
		{
			get { return (ZString)InnerAssignedToInfo.Value; }
			set { InnerAssignedToInfo.Value = value; }
		}

		public ZPropertyInfo AssignedToInfo
		{
			get { return InnerAssignedToInfo == null ? null : GetWrappedZPropertyInfo(nameof(AssignedTo), x => InnerAssignedToInfo); }
		}

		public virtual MultilingualString AssignedToCaption
		{
			get { return ResString.GetMultilingualString("21bc4a34-aeae-44c9-a39e-64ca61742460", "Assigned To"); }
		}

		public ZPropertyInfo AssignedToCaptionInfo
		{
			get { return GetZPropertyInfo(nameof(AssignedToCaption)); }
		}

		ZPropertyInfo InnerAssignedToInfo
		{
			get { return ProcessQueue == null ? null : (QueueType == ProcessQueueType.Enum.Customs) ? ProcessQueue.P4_GS_NKCustomsTaskAssignedToInfo : ProcessQueue.P4_GS_NKTaskAssignedToInfo; }
		}

		#endregion

		#region Reason

		public ZString Reason
		{
			get { return (ZString)InnerReasonInfo.Value; }
			set { InnerReasonInfo.Value = value; }
		}

		public ZPropertyInfo ReasonInfo
		{
			get { return InnerReasonInfo == null ? null : GetWrappedZPropertyInfo(nameof(Reason), x => InnerReasonInfo); }
		}

		public virtual MultilingualString ReasonCaption
		{
			get { return ResString.GetMultilingualString("8bcaa305-b653-48e5-adf0-3cbcf0c34ecc", "Reason"); }
		}

		public ZPropertyInfo ReasonCaptionInfo
		{
			get { return GetZPropertyInfo(nameof(ReasonCaption)); }
		}

		ZPropertyInfo InnerReasonInfo
		{
			get { return ProcessQueue == null ? null : (QueueType == ProcessQueueType.Enum.Customs) ? ProcessQueue.P4_CustomsReasonInfo : ProcessQueue.P4_ReasonInfo; }
		}

		#endregion

		#region P4_CustomAttrib8

		public ZString P4_CustomAttrib8
		{
			get { return ProcessQueue.P4_CustomAttrib8; }
			set { ProcessQueue.P4_CustomAttrib8 = value; }
		}

		public ZPropertyInfo P4_CustomAttrib8Info
		{
			get { return ProcessQueue == null ? null : GetWrappedZPropertyInfo(ProcessQueueSchema.Constants.P4_CustomAttrib8, x => ProcessQueue.P4_CustomAttrib8Info); }
		}

		#endregion

		#region P4_CustomDate4

		public ZDateTime P4_CustomDate4
		{
			get { return ProcessQueue.P4_CustomDate4; }
			set { ProcessQueue.P4_CustomDate4 = value; }
		}

		public ZPropertyInfo P4_CustomDate4Info
		{
			get { return ProcessQueue == null ? null : GetWrappedZPropertyInfo(ProcessQueueSchema.Constants.P4_CustomDate4, x => ProcessQueue.P4_CustomDate4Info); }
		}

		#endregion

		#region Lookups

		public IActiveProcessQueueLookups Lookups
		{
			get
			{
				if (fLookups == null)
				{
					fLookups = new ActiveProcessQueueLookups(this);
				}
				return fLookups;
			}
		}

		ActiveProcessQueueLookups fLookups;

		#endregion

		#endregion

		#region QueueLogs

		public ActiveProcessQueueLogCollectionView QueueLogs
		{
			get
			{
				if (fQueueLogs == null)
				{
					ActiveProcessQueueLogCollection collection = new ActiveProcessQueueLogCollection(this);
					fQueueLogs = new ActiveProcessQueueLogCollectionView(collection);
				}
				return fQueueLogs;
			}
		}

		public virtual bool IncludeLogsWithEmptyQueueName
		{
			get { return true; }
		}

		ActiveProcessQueueLogCollectionView fQueueLogs;

		#endregion

		#region QueueType

		public ZBool IsCustomsQueue
		{
			get { return (QueueType == ProcessQueueType.Enum.Customs); }
			set
			{
				if (value != IsCustomsQueue)
				{
					QueueType = value ? ProcessQueueType.Enum.Customs : ProcessQueueType.Enum.Commercial;
				}
				IsCustomsQueueInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsCustomsQueueInfo
		{
			get { return GetZPropertyInfo(nameof(IsCustomsQueue)); }
		}

		public ZBool IsCommercialQueue
		{
			get { return (QueueType == ProcessQueueType.Enum.Commercial); }
			set
			{
				if (value != IsCommercialQueue)
				{
					QueueType = value ? ProcessQueueType.Enum.Commercial : ProcessQueueType.Enum.Customs;
				}
				IsCommercialQueueInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsCommercialQueueInfo
		{
			get { return GetZPropertyInfo(nameof(IsCommercialQueue)); }
		}

		public ProcessQueueType.Enum QueueType
		{
			get { return fQueueType; }
			set
			{
				if (fQueueType != value)
				{
					fQueueType = value;
					RebuildQueueLogsIfLoaded();
					RefreshBinding();
				}
			}
		}

		protected virtual ProcessQueueType.Enum DefaultQueueType
		{
			get { return ProcessQueueType.Enum.Customs; }
		}

		void RebuildQueueLogsIfLoaded()
		{
			if (fQueueLogs != null)
			{
				QueueLogs.Rebuild();
			}
		}

		void InitialiseQueueType()
		{
			fQueueType = DefaultQueueType;
		}

		ProcessQueueType.Enum fQueueType;

		#region For Test
#if DEBUG

		public bool IsQueueLogsLoaded
		{
			get { return fQueueLogs != null; }
		}

#endif
		#endregion

		#endregion

		protected delegate ActiveProcessQueue NewDelegate(ProcessQueue processQueue);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();
		public readonly ProcessQueue ProcessQueue;
	}
}

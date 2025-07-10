using System;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class ActiveProcessQueueLogCollection : ProcessQueueLogCollectionBase
	{
		public ActiveProcessQueueLogCollection(ActiveProcessQueue activeProcessQueue) : base(activeProcessQueue.ProcessQueue)
		{
			this.ActiveProcessQueue = activeProcessQueue;
			HookCollectionCountChangedEventHandler();
			Load();
		}

		public override bool ReadOnly
		{
			get { return true; }
		}

		public override void Load()
		{
			IsLoaded = true;
			RemoveAllButLeaveRelationshipsIntact();

			ProcessQueueLogCollection collection = (ActiveProcessQueue.QueueType == ProcessQueueType.Enum.Customs) ? CustomsQueueLogs : CommercialQueueLogs;
			foreach (ProcessQueueLog log in collection)
			{
				Add(log);
			}
		}

		public readonly ActiveProcessQueue ActiveProcessQueue;

		#region Events

		void HookCollectionCountChangedEventHandler()
		{
			CollectionCountChangedEventHandler handler = new CollectionCountChangedEventHandler(QueueLogs_CountChanged);
			CustomsQueueLogs.CountChanged += handler;
			CommercialQueueLogs.CountChanged += handler;
		}

		void QueueLogs_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			HandleQueueLogsCountChanged();
		}

		void HandleQueueLogsCountChanged()
		{
			Load();
		}

		#endregion

		#region Implementation

		protected override BusinessObject AddNewCore()
		{
			throw new NotSupportedException("Should not be adding ProcessQueueLog from here");
		}

		ProcessQueueLogCollection CustomsQueueLogs
		{
			get { return ActiveProcessQueue.ProcessQueue.CustomsQueueLogs; }
		}

		ProcessQueueLogCollection CommercialQueueLogs
		{
			get { return ActiveProcessQueue.ProcessQueue.CommercialQueueLogs; }
		}

		#endregion
	}
}

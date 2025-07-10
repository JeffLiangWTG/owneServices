using System.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class ActiveProcessQueueLogCollectionView : BusinessObjectCollectionView<ProcessQueueLog>
	{
		public ActiveProcessQueueLogCollectionView(ActiveProcessQueueLogCollection collection) : base(collection)
		{
			SortByEventTimeByDefault();
		}

		protected override void RebuildCore()
		{
			CollectionToFilter.Load();
			base.RebuildCore();
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			bool result = false;

			ProcessQueueLog log = element as ProcessQueueLog;
			if (log != null && CollectionToFilter.ActiveProcessQueue != null)
			{
				result = (!log.Queue.IsEmpty || CollectionToFilter.ActiveProcessQueue.IncludeLogsWithEmptyQueueName);
			}

			return result;
		}

		void SortByEventTimeByDefault()
		{
			Sort(ProcessQueueLog.Schema.SL_EventTime, ListSortDirection.Descending);
		}

		new ActiveProcessQueueLogCollection CollectionToFilter
		{
			get { return (ActiveProcessQueueLogCollection)base.CollectionToFilter; }
		}
	}
}

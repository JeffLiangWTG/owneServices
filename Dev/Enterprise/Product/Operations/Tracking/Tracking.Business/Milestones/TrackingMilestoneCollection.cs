using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Web;

namespace Enterprise.Tracking.Business
{
	public class TrackingMilestoneCollection : NonPersistentBusinessObjectCollection<TrackingMilestone>
	{
		#region Schema

		public abstract class Schema
		{
			public const string LastMilestone = "LastMilestone";
			public const string NextMilestone = "NextMilestone";
		}

		#endregion

		public TrackingMilestoneCollection(IWorkflowProvider provider, IUpdatableMilestoneEventsProvider updatableEventsProvider, bool editMode)
		{
			milestoneEventsProvider = updatableEventsProvider;
			MilestoneCollectionIncludingRelatedView processTaskCollection = provider.WorkflowItems.MilestonesIncludingRelated;

			if (!processTaskCollection.IsLoaded)
			{
				processTaskCollection.Load();
			}

			foreach (ProcessTask task in processTaskCollection)
			{
				if (!task.IsDeleted && task.P9_IsPublished)
				{
					TrackingMilestone trackingMilestone = new TrackingMilestone(task, provider as IFlightDetailsSuppression);
					if (editMode && updatableEventsProvider.UpdatableMilestoneEventCodes.Contains(trackingMilestone.EventCode))
					{
						base.Add(trackingMilestone);
					}
					else if (!editMode)
					{
						Add(trackingMilestone);
					}
				}
			}

			SortAndFilterByRegistry(editMode);
			modificationDenied = true;
		}

		public TrackingMilestoneCollection(IWorkflowProvider provider)
			: this(provider, provider as IUpdatableMilestoneEventsProvider, false)
		{
		}

		public TrackingMilestoneCollection(IWorkflowProvider provider, bool editableMilestones)
			: this(provider, provider as IUpdatableMilestoneEventsProvider, editableMilestones)
		{
		}

		void SortAndFilterByRegistry(bool editMode)
		{
			switch (WebDataRegistry.Instance.MilestoneSortOrder.Value)
			{
				case (MilestoneSortOrderList.Codes.Chronological):
					Sort(TrackingMilestone.Schema.DisplayDate);
					break;
				case (MilestoneSortOrderList.Codes.ReversedChronological):
					Sort(TrackingMilestone.Schema.DisplayDate, ListSortDirection.Descending);
					break;
				case (MilestoneSortOrderList.Codes.Sequence):
					Sort(TrackingMilestone.Schema.Sequence);
					break;
				case (MilestoneSortOrderList.Codes.NoSort):
					break;
			}

			if (!editMode && WebDataRegistry.Instance.MilestoneVisibility.Value == MilestoneVisibilityList.Codes.LastCompletedMilestoneOnly)
			{
				var lastCompleted = LastMilestone;

				for (int i = 0; i < Count; i++)
				{
					if (this[i] != lastCompleted)
					{
						Remove(this[i]);
						i--;
					}
				}
			}
		}

		readonly bool modificationDenied;

		public void AddForEmailReporting(DataState state, PropertyChangeInfoCollection propertiesForEmailReporting)
		{
			foreach (TrackingMilestone milestone in this)
			{
				if (milestone.Task == null || !milestone.Task.IsDeleted)
				{
					propertiesForEmailReporting.Add(state, ResString.GetMultilingualString("4967647f-91b8-4c36-9231-f403a98486fe", "Milestone: {0}", milestone.Description), milestone.GenerateDetailsForEmailReporting());
				}
			}
		}

		public override void Add(BusinessObject businessObject)
		{
			switch (WebDataRegistry.Instance.MilestoneVisibility.Value)
			{
				case MilestoneVisibilityList.Codes.All:
					base.Add(businessObject);
					break;

				case MilestoneVisibilityList.Codes.CompletedMilestonesOnly:
				case MilestoneVisibilityList.Codes.LastCompletedMilestoneOnly:
					if (((TrackingMilestone)businessObject).Status == ProcessTask.Completed || ((TrackingMilestone)businessObject).Status == ProcessTask.CompletedLate)
					{
						base.Add(businessObject);
					}
					break;
			}
		}

		protected override bool AllowNewCore
		{
			get { return modificationDenied; }
		}

		protected override bool AllowRemoveCore
		{
			get { return modificationDenied; }
		}

		protected override bool AllowSort
		{
			get { return modificationDenied; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new TrackingMilestone(ZString.Empty, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, ZInt.Zero);
		}

		public TrackingMilestone LastMilestone => this
			.OfType<TrackingMilestone>()
			.Where(m => m.ActualStatus == ProcessTask.Completed || m.ActualStatus == ProcessTask.CompletedLate)
			.OrderByDescending(m => m.ActualDate)
			.ThenByDescending(m => m.Sequence)
			.FirstOrDefault();

		public TrackingMilestone NextMilestone => this
			.OfType<TrackingMilestone>()
.FirstOrDefault(m => m.ActualStatus == ProcessTask.Pending || m.ActualStatus == ProcessTask.Overdue);

		public IUpdatableMilestoneEventsProvider MilestoneEventsProvider => milestoneEventsProvider;

		readonly IUpdatableMilestoneEventsProvider milestoneEventsProvider;
	}
}

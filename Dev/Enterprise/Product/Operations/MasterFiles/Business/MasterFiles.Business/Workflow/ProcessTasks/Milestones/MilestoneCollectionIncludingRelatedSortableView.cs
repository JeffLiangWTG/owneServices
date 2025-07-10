namespace Enterprise.MasterFiles.Business
{
	public class MilestoneCollectionIncludingRelatedSortableView : MilestoneCollectionIncludingRelatedView
	{
		public MilestoneCollectionIncludingRelatedSortableView(ProcessTaskCollection collection) : base(collection) { }

		protected override bool AllowSort
		{
			get { return true; }
		}
	}
}

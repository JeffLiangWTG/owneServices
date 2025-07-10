using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class GlbTimeAllocationCollectionView : BusinessObjectCollectionView<GlbTimeAllocation>
	{
		public GlbTimeAllocationCollectionView(GlbTimeAllocationFilter filter)
			: base(filter.Collection)
		{
			this.Filter = filter;
		}

		readonly GlbTimeAllocationFilter Filter;

		#region Implementation

		protected override void RebuildOnConstruction()
		{
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			GlbTimeAllocation time = (GlbTimeAllocation)element;
			return (Filter.StartTime.IsEmpty || Filter.StartTime <= time.GA_StartTime) &&
				(Filter.EndTime.IsEmpty || Filter.EndTime >= time.GA_EndTime);
		}

		#endregion
	}
}

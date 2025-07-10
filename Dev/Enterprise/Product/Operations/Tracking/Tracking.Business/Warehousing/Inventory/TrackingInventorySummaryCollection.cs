using CargoWise.EntityFramework;

namespace Enterprise.Tracking.Business
{
	public class TrackingInventorySummaryCollection : NonPersistentBusinessObjectCollection<TrackingInventorySummary>
	{
		public TrackingInventorySummaryCollection(BusinessObjectFactory factory)
			: base(factory) { }

		public override void Load() => Load(new ZQuery());

		public override void Load(ZQuery filter) => BuildSummaries(filter);

		protected override bool AllowNewCore => isBuilding;

		protected override bool AllowRemoveCore => isBuilding;

		protected override BusinessObject CreateNonPersistentBusinessObject() => TrackingInventorySummaryProvider.GetEmptySummary(Factory);

		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy()
		{
			return new TrackingInventorySummaryCollectionFetchStrategy(this);
		}

		#region Implementation

		bool isBuilding;

		void BuildSummaries(ZQuery inventoryFilter)
		{
			try
			{
				isBuilding = true;
				RemoveAll();

				var summaries = TrackingInventorySummaryProvider.GetSummaries(Factory, inventoryFilter);
				AddRange(summaries);

				var strategy = (TrackingInventorySummaryCollectionFetchStrategy)FetchStrategy;
				strategy.AddFetchHints(summaries);
				IsLoaded = true;
			}
			finally
			{
				isBuilding = false;
			}
		}

		#endregion
	}
}

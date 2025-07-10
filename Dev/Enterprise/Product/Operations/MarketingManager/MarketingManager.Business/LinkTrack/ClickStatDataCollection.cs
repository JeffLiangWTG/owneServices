using CargoWise.EntityFramework;

namespace Enterprise.MarketingManager.Business
{
	public class ClickStatDataCollection : NonPersistentBusinessObjectCollection<ClickStatData>
	{
		public ClickStatDataCollection(GlbCompanyCampaign campaign)
			: base(campaign.Factory)
		{
			this.Campaign = campaign;
		}
		readonly GlbCompanyCampaign Campaign;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ClickStatData(Campaign);
		}

		protected override bool AllowNewCore
		{
			get { return Campaign.IsLinkTrackCampaign; }
		}

		protected override bool AllowRemoveCore
		{
			get { return Campaign.IsLinkTrackCampaign; }
		}

		protected override System.Collections.IComparer GetComparerForSort(System.ComponentModel.PropertyDescriptor property, System.ComponentModel.ListSortDirection direction)
		{
			if (property.Name == ClickStatData.Schema.ClicksRatePercentage)
			{
				return new ClickRatePercentageComparer(direction);
			}
			return base.GetComparerForSort(property, direction);
		}

		class ClickRatePercentageComparer : System.Collections.IComparer
		{
			static int sortOrderModifier = 1;

			public ClickRatePercentageComparer(System.ComponentModel.ListSortDirection direction)
			{
				if (direction == System.ComponentModel.ListSortDirection.Descending)
				{
					sortOrderModifier = -1;
				}
				else if (direction == System.ComponentModel.ListSortDirection.Ascending)
				{
					sortOrderModifier = 1;
				}
			}

			public int Compare(object x, object y)
			{
				ClickStatData data1 = (ClickStatData)x;
				ClickStatData data2 = (ClickStatData)y;

				int result = System.Decimal.Compare(data1.ClicksRate, data2.ClicksRate);

				return result * sortOrderModifier;
			}
		}
	}
}

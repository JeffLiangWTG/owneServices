using System.Collections.Generic;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// Used to display RateService RateLineItems in the grid in Multimodal serach
	/// </summary>
	public class IRateLineItemsView : RateLineItemMapperView<WiseLineItemView>
	{
		public IRateLineItemsView(WiseLineItemViewsCollection wiseLineItems, IRateLine parent)
			: base(wiseLineItems, parent)
		{
			Rebuild();
			Sort((IComparer<WiseLineItemView>)new WiseLineItemViewComparer());
		}

		protected override void RebuildOnConstruction()
		{
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;
	}
}


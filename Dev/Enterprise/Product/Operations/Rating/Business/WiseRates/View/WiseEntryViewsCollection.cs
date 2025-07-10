using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Rating.Business
{
	public class WiseEntryViewsCollection : NonPersistentBusinessObjectCollection<WiseEntryView>
	{
		public WiseEntryViewsCollection()
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new WiseEntryView();
		}

		protected override bool AllowNewCore => false;
	}

	public class WiseLineViewsCollection : NonPersistentBusinessObjectCollection<WiseLineView>
	{
		public WiseLineViewsCollection(IEnumerable<IRateLine> rateLines)
		{
			this.AddRange(rateLines.Select(x => new WiseLineView(x)));
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return null;
		}

		protected override bool AllowNewCore => false;
	}

	public class WiseLineItemViewsCollection : NonPersistentBusinessObjectCollection<WiseLineItemView>
	{
		public WiseLineItemViewsCollection(IEnumerable<IRateLineItem> rateLineItems)
		{
			this.AddRange(rateLineItems.Select(x => new WiseLineItemView(x)));
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return null;
		}

		protected override bool AllowNewCore => false;
	}

	internal class IRateLineItemComparer : IComparer<IRateLineItem>
	{
		public int Compare(IRateLineItem x, IRateLineItem y)
		{
			if (ReferenceEquals(x, y) || x == null || y == null)
			{
				return 0;
			}

			var value = x.TM_Break - y.TM_Break;

			if (value > 0)
			{
				return 1;
			}

			if (value < 0)
			{
				return -1;
			}

			if (x.TM_Type == Calculator.Items.Operator.Minus && y.TM_Type == Calculator.Items.Operator.Plus)
			{
				return -1;
			}

			if (x.TM_Type == Calculator.Items.Operator.Plus && y.TM_Type == Calculator.Items.Operator.Minus)
			{
				return 1;
			}

			return String.Compare(y.TM_Type.ToString(), x.TM_Type.ToString(), StringComparison.Ordinal);
		}
	}

	internal class WiseLineItemViewComparer :
		IRateLineItemComparer, IComparer<WiseLineItemView>
	{
		public int Compare(WiseLineItemView x, WiseLineItemView y) 
			=> base.Compare(x, y);
	}
}

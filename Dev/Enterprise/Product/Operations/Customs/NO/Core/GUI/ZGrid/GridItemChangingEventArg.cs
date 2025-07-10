using System;

namespace Enterprise.Customs.NO.GUI;

sealed class GridItemChangingEventArg<TItemType> : EventArgs where TItemType : class
{
	public GridItemChangingEventArg(TItemType oldItem, TItemType newItem)
	{
		OldItem = oldItem;
		NewItem = newItem;
	}

	public TItemType OldItem { get; }
	public TItemType NewItem { get; }
}

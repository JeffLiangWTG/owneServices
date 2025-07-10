using System;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.GUI;

sealed class ZGridItemTracker<TItemType> : IDisposable
	where TItemType : class 
{
	public ZGridItemTracker(ZGrid grid)
	{
		this.grid = Argument.NotNull(grid, nameof(grid));
		AttachEventsToGrid();
	}

	readonly ZGrid grid;

	public TItemType CurrentItem { get; private set; }
	public event EventHandler<GridItemChangingEventArg<TItemType>> OnCurrentItemChanging;
	public event EventHandler<EventArgs> OnCurrentItemChanged;

	public void Dispose()
	{
		grid.AfterBind -= GridAfterBind;
		var listManager = ListManager;
		if (listManager is not null)
		{
			listManager.CurrentChanged -= ListItemChanged;
		}
	}

	void AttachEventsToGrid()
	{
		grid.AfterBind += GridAfterBind;
	}

	void GridAfterBind(object sender, EventArgs e)
	{
		var listManager = ListManager;
		if (listManager is not null)
		{
			listManager.CurrentChanged += ListItemChanged;
			ListItemChanged(this, EventArgs.Empty);
		}
	}

	void ListItemChanged(object sender, EventArgs e)
	{
		var listManager = ListManager;
		if (listManager is { Count: > 0 })
		{
			if (listManager.GetCurrent() is TItemType item && item != CurrentItem)
			{
				OnCurrentItemChanging?.Invoke(this, new GridItemChangingEventArg<TItemType>(CurrentItem, item));
				CurrentItem = item;
				OnCurrentItemChanged?.Invoke(this, EventArgs.Empty);
			}

			return;
		}

		OnCurrentItemChanging?.Invoke(this, new GridItemChangingEventArg<TItemType>(CurrentItem, null));
		CurrentItem = null;
		OnCurrentItemChanged?.Invoke(this, EventArgs.Empty);
	}

	CurrencyManager ListManager => grid.ListManager;
}

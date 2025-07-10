using System.Collections;
using System.Drawing;
using WinzorFramework;
using WinzorFramework.Extensions;

namespace System.Windows.Forms;

[Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Referenced in ListBox.razor")]
public partial class ListBox : ListControl
{
	int lastSelectedIndex = NoMatches;

	public const int NoMatches = -1;
	bool selectedValueChangedFired;
	internal int scaledListItemBordersHeight = 2;

	public ObjectCollection Items => itemsCollection ??= CreateItemCollection();
	ObjectCollection? itemsCollection;

	public override bool UseParentDivForLayout => false;

	public bool Sorted
	{
		get
		{
			return _sorted;
		}
		set
		{
			if (UpdateProperty(ref _sorted, value))
			{
				if (_sorted && Items is not null && Items.Count >= 1)
				{
					Sort();
				}
			}
		}
	}
	bool _sorted;

	protected internal string ListBoxStyleString
	{
		get
		{
			var styleString = "";

			if (!MultiColumn)
			{
				styleString = "overflow-y:auto;";
				if (ScrollAlwaysVisible)
				{
					styleString = "overflow-y:scroll;";
				}
			}

			return styleString;
		}
	}

	public ListBoxItemData[]? ListBoxItemsData { get; private set; }

	public virtual SelectionMode SelectionMode { get; set; } = SelectionMode.One;

	public virtual int ItemHeight { get; set; } = DefaultItemHeight;

	public bool IntegralHeight { get; set; } = true;

	public virtual DrawMode DrawMode { get; set; } = DrawMode.Normal;

	public BorderStyle BorderStyle { get; set; }

	public bool MultiColumn { get; set; }

	public int ColumnWidth { get; set; }

	public bool HorizontalScrollbar { get; set; }

	public bool ScrollAlwaysVisible { get; set; }

	public bool UseTabStops { get; set; }

	protected bool Draggable { get; set; }

	public void BeginUpdate()
	{
	}

	public void EndUpdate()
	{
	}

	/// <summary>
	///  Sorts the items in the listbox.
	/// </summary>
	protected virtual void Sort()
	{
		if (_sorted && Items is not null)
		{
			Items.Sort();
			RecreateDataItems();
		}
	}

	public object? SelectedItem
	{
		get
		{
			return SelectedIndex < 0 ? null : Items?[SelectedIndex];
		}
		set
		{
			if (value is not null)
			{
				SelectedIndex = FindItemIndex(value);
			}
		}
	}

	public ListBox()
	{
		RecreateDataItems();
	}

	public override Color BackColor
	{
		get => this.ShouldSerializeBackColor() ? base.BackColor : SystemColors.Window;
		set => base.BackColor = value;
	}

	protected int FindItemIndex(object target)
	{
		var foundIndex = NoMatches;

		for (var i = 0; i < Items.Count; i++)
		{
			var item = Items[i];
			if (object.ReferenceEquals(Items[i], target))
			{
				foundIndex = i;
				break;
			}
		}

		return foundIndex;
	}

	public SelectedObjectCollection SelectedItems => selectedObjectCollection ??= new SelectedObjectCollection(this);
	SelectedObjectCollection? selectedObjectCollection;

	public override int SelectedIndex
	{
		get
		{
			var firstSelected = NoMatches;
			for (var i = 0; i < Items.Count; i++)
			{
				var item = Items.GetInnerItem(i);
				if (SelectedSet.Contains(item))
				{
					firstSelected = i;
					break;
				}
			}

			return firstSelected;
		}
		set
		{
			CheckIndex(value);
			lastSelectedIndex = value;
			SetSelected(value, true);
		}
	}

	public event EventHandler? SelectedIndexChanged;

	public SelectedIndexCollection SelectedIndices => selectedIndexCollection ??= new SelectedIndexCollection(this);
	SelectedIndexCollection? selectedIndexCollection;

	readonly Lazy<HashSet<object>> lazySelectedSet = new Lazy<HashSet<object>>(() => new HashSet<object>());

	internal HashSet<object> SelectedSet { get => lazySelectedSet.Value; }

	WebMouseEventArgs defaultMouseArg = new WebMouseEventArgs();

	public void SetSelected(int index, bool value, WebMouseEventArgs? args = null)
	{
		CheckIndex(index);
		if (index == NoMatches)
		{
			if (SelectedIndex != NoMatches)
			{
				ClearSelected();
			}
		}
		else
		{
			SetSelected(args ?? defaultMouseArg, index, value);
		}
	}

	protected void CheckIndex(int index)
	{
		if (index < NoMatches || index >= Items.Count)
		{
			throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
		}

		if (SelectionMode == SelectionMode.None)
		{
			throw new ArgumentException("SelectionMode is set to None, selection is not allowed.");
		}
	}

	void SetSelected(WebMouseEventArgs args, int index, bool value)
	{
		if (SelectionMode == SelectionMode.One)
		{
			SelectItem(defaultMouseArg, index, value);
		}
		else if (SelectionMode == SelectionMode.MultiSimple || SelectionMode == SelectionMode.MultiExtended)
		{
			SelectItem(args, index, value);
		}
		else
		{
			ResetSelection();
		}
		NotifyRenderRequired();
	}

	void SelectItem(WebMouseEventArgs args, int index, bool value)
	{
		if (SelectionMode == SelectionMode.MultiExtended &&
			lastSelectedIndex != NoMatches && args.ShiftKey && args.Buttons > 0)
		{
			//Select multiple items in a range (Shift+Click)
			var lowerRow = Math.Min(lastSelectedIndex, index);
			var upperRow = Math.Max(lastSelectedIndex, index);

			ResetSelection(false);

			for (var i = lowerRow; i <= upperRow; i++)
			{
				SelectedSet.Add(Items.GetInnerItem(i));
			}

			RecreateDataItems();
		}
		else if (SelectionMode == SelectionMode.MultiSimple ||
			(SelectionMode == SelectionMode.MultiExtended && (args.Buttons == 0 || args.CtrlKey)))
		{
			//Select multiple individual items. This is always used for MultiSimple.
			//For MultiExtended, this is used for Ctrl+Click or when SelectItem() is used outside of a mouse event.
			if (IsSelected(index))
			{
				UnSelect(index);
				if (lastSelectedIndex == index)
				{
					lastSelectedIndex = NoMatches;
				}
			}
			else
			{
				if (value)
				{
					Select(index);
					lastSelectedIndex = index;
				}
				else
				{
					UnSelect(index);
					if (lastSelectedIndex == index)
					{
						lastSelectedIndex = NoMatches;
					}
				}
			}
		}
		else
		{
			//Select an individual item
			ResetSelection();
			Select(index);
		}
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);
		SetBoundsCore(Left, Top, Width, Height, BoundsSpecified.Height);
	}

	int AdjustedHeight(int height)
	{
		if (Visible && IntegralHeight && IsHandleCreated && ItemHeight > 0)
		{
			height = Math.Max((int)Math.Floor((double)height / ItemHeight), 1) * ItemHeight;
		}

		return height;
	}

	protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified) => base.SetBoundsCore(x, y, width, AdjustedHeight(height), specified);

	public virtual void Select(int index)
	{
		var innerItem = Items.GetInnerItem(index);
		if (innerItem != null && !SelectedSet.Contains(innerItem))
		{
			lastSelectedIndex = index;
			SelectedSet.Add(innerItem);
			RecreateDataItems();
			OnSelectedChange();
		}
	}

	public void OnSelectedChange()
	{
		SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
	}

	void UnSelect(int index)
	{
		var innerItem = Items.GetInnerItem(index);
		if (innerItem != null && SelectedSet.Contains(innerItem))
		{
			SelectedSet.Remove(innerItem);
			RecreateDataItems();
			SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
		}
	}

	public virtual void ResetSelection(bool resetLastSelectedIndex = true)
	{
		SelectedSet.Clear();
		RecreateDataItems();
		if (resetLastSelectedIndex)
		{
			lastSelectedIndex = NoMatches;
		}
	}

	public bool GetSelected(int index)
	{
		CheckIndex(index);
		return IsSelected(index);
	}

	protected bool IsSelected(int index)
	{
		return Items?[index] != null && SelectedSet.Contains(Items.GetInnerItem(index));
	}

	public void ClearSelected()
	{
		SelectedSet.Clear();
		RecreateDataItems();
	}

	protected async Task MouseOverAsync(WebMouseEventArgs e, int index)
	{
		await InvokeWinzorDispatcherAsync(() =>
		{
			currentMouseOverIndex = index;
		});
		OnMouseMove(new MouseEventArgs(e.GetMouseButtons(), (int)e.Detail, (int)e.ClientX, (int)e.ClientY, 0));
	}

	protected int currentMouseOverIndex;

	public int IndexFromPoint(int x, int y) => currentMouseOverIndex;

	public int IndexFromPoint(Point p) => currentMouseOverIndex;

	public Rectangle GetItemRectangle(int index) => Rectangle.Empty;

	protected virtual void OnMeasureItem(MeasureItemEventArgs e)
	{
	}

	#region DrawItem

	event DrawItemEventHandler? drawItem;
	public event DrawItemEventHandler? DrawItem
	{
		add
		{
			drawItem += value;
			RecreateDataItems();
		}
		remove
		{
			drawItem -= value;
			RecreateDataItems();
		}
	}

	protected virtual void OnDrawItem(DrawItemEventArgs e)
	{
		drawItem?.Invoke(this, e);
	}

	#endregion

	/// <summary>
	///  Reparses the objects, getting new text strings for them.
	/// </summary>
	protected override void RefreshItems()
	{
		// Store the currently selected object collection.
		//
		ObjectCollection? savedItems = itemsCollection;

		// Clear the items.
		//
		itemsCollection = null;
		selectedIndexCollection = null;

		object[]? newItems = null;

		// if we have a dataSource and a DisplayMember, then use it
		// to populate the Items collection
		//z
		if (DataManager != null && DataManager.Count != -1)
		{
			newItems = new object[DataManager.Count];
			for (int i = 0; i < newItems.Length; i++)
			{
				newItems[i] = DataManager[i];
			}
		}
		else if (savedItems != null)
		{
			newItems = new object[savedItems.Count];
			savedItems.CopyTo(newItems, 0);
		}

		// Store the current list of items
		//
		if (newItems != null)
		{
			Items.AddRange(newItems);
		}

		// Restore the selected indices if SelectionMode allows it.
		//
		if (SelectionMode != SelectionMode.None)
		{
			if (DataManager != null)
			{
				// put the selectedIndex in sync w/ the position in the dataManager
				SelectedIndex = DataManager.Position;
			}
		}
	}

	protected override void OnDataSourceChanged(EventArgs e)
	{
		if (DataSource == null)
		{
			BeginUpdate();
			SelectedIndex = -1;
			Items.Clear();
			EndUpdate();
		}
		base.OnDataSourceChanged(e);
		RefreshItems();
	}

	public const int DefaultItemHeight = 13;

	protected override void SetItemsCore(IList items)
	{
		Items.Clear();

		foreach (var item in items)
		{
			Items.Add(item);
		}

		if (DataManager != null)
		{
			if (DataSource is ICurrencyManagerProvider)
			{
				selectedValueChangedFired = false;
			}

			SelectedIndex = DataManager.Position;
			if (!selectedValueChangedFired)
			{
				OnSelectedValueChanged(EventArgs.Empty);
				selectedValueChangedFired = false;
			}
		}
	}

	protected override void OnSelectedValueChanged(EventArgs e)
	{
		base.OnSelectedValueChanged(e);
		selectedValueChangedFired = true;
	}

	void StartDrag(WebDragEventArgs arg)
	{
	}

	async Task OnDragEnterAsync(WebDragEventArgs arg, int index)
	{
		await InvokeWinzorDispatcherAsync(() => OnDragEnter(
			new DragEventArgs(
				new DataObject(index),
				0,
				(int)arg.ClientX,
				(int)arg.ClientY,
				DragDropEffects.Move,
				DragDropEffects.Move))
		);
	}

	async Task OnWinzorDragEndAsync(WinzorDragEndEventArgs arg, int index)
	{
		await InvokeWinzorDispatcherAsync(() => OnDragEnd(
			new DragEventArgs(
				new DataObject(index),
				0,
				arg.ClientX,
				arg.ClientY,
				DragDropEffects.Move,
				DragDropEffects.Move))
		);
	}

	async Task OnDropAsync(WebDragEventArgs arg)
	{
		await InvokeWinzorDispatcherAsync(() =>
		{
			defaultMouseArg = arg;

			OnDragDrop(
				new DragEventArgs(
					new DataObject(),
					0,
					(int)arg.ClientX,
					(int)arg.ClientY,
					DragDropEffects.Move,
					DragDropEffects.Move));
		});
	}

	int mouseDownIndex = NoMatches;
	WebMouseEventArgs? mouseDownEventArgs;

	async Task OnMouseDownAsync(WebMouseEventArgs args, int itemIndex)
	{
		if (!Enabled)
		{
			return;
		}

		await InvokeWinzorDispatcherAsync(() =>
		{
			if (Focused)
			{
				SetSelected(args, itemIndex, true);
			}
			else
			{
				mouseDownIndex = itemIndex;
				mouseDownEventArgs = args;
			}

			OnMouseDownCore(args);
		});
	}

	protected override void OnGotFocus(EventArgs e)
	{
		base.OnGotFocus(e);
		if (mouseDownIndex != NoMatches)
		{
			SetSelected(mouseDownIndex, true, mouseDownEventArgs);
			mouseDownIndex = NoMatches;
			mouseDownEventArgs = null;
		}
	}

	protected override Size DefaultSize => new Size(120, 96);

	protected override void Dispose(bool disposing)
	{
		if (IsDisposed)
		{
			return;
		}

		base.Dispose(disposing);
		if (disposing)
		{
			SelectedSet.Clear();
			ListBoxItemsData = null;
		}
	}

	#region ListBoxItemData Implementation

	public virtual void RecreateDataItems()
	{
		ListBoxItemsData = new ListBoxItemData[Items.Count];
		for (int index = 0; index < Items.Count; index++)
		{
			ListBoxItemsData[index] = new ListBoxItemData(Items[index].ToString() ?? string.Empty, SelectedSet.Contains(Items.GetInnerItem(index)));
			var drawItemEventArgs = new DrawItemEventArgs(index, ListBoxItemsData[index].IsSelected ? DrawItemState.Selected : DrawItemState.None);
			ListBoxItemsData[index].TextColor = drawItemEventArgs.ForeColor;

			if (DrawMode == DrawMode.OwnerDrawFixed || DrawMode == DrawMode.OwnerDrawVariable)
			{
				OnDrawItem(drawItemEventArgs);
			}
		}
		NotifyRenderRequired();
	}

	protected virtual ObjectCollection CreateItemCollection()
	{
		return new ObjectCollection(this);
	}

	public class ListBoxItemData
	{
		public ListBoxItemData(string displayText, bool isSelected)
		{
			DisplayText = displayText;
			IsSelected = isSelected;
		}

		public string DisplayText { get; } = string.Empty;

		public bool IsSelected { get; }

		public Color TextColor { get; set; }

		public string StyleString => $"color:rgb({TextColor.R},{TextColor.G},{TextColor.B});";
	}

	#endregion
}

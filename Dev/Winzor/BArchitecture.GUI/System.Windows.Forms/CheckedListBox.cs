using System.Collections;
using System.ComponentModel;
using System.Drawing;
using WinzorFramework;

namespace System.Windows.Forms;

public partial class CheckedListBox : ListBox
{
	public new ObjectCollection Items => (ObjectCollection)base.Items;

	int lastSelected = -1;

	int currentSelected = -1;

	public CheckedItemCollection CheckedItems => checkedItems ??= new CheckedItemCollection(this);
	CheckedItemCollection? checkedItems;

	public CheckedIndexCollection CheckedIndices => checkedIndices ??= new CheckedIndexCollection(this);
	CheckedIndexCollection? checkedIndices;

	public bool CheckOnClick { get; set; }

	public new const int DefaultItemHeight = 15;

	public bool ThreeDCheckBoxes { get; set; }

	public override bool UseParentDivForLayout => true;

	public override int ItemHeight
	{
		get
		{
			return Font.Height + scaledListItemBordersHeight;
		}
		set
		{
		}
	}

	public CheckedListBox()
	{
	}

	protected override bool SelectableByTabKey => false;

	public void SetItemChecked(int index, bool value)
	{
		SetItemCheckState(index, value ? CheckState.Checked : CheckState.Unchecked);
	}

	[Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Referenced in CheckedListBox.razor")]
	Task CheckedChangedAsync(WebMouseEventArgs e, int itemIndex)
	{
		if (!Enabled)
		{
			return Task.CompletedTask;
		}

		return InvokeWinzorDispatcherAsync(() =>
		{
			var currentIndex = itemIndex;
			currentSelected = itemIndex;
			if (currentIndex == lastSelected || CheckOnClick)
			{
				SetItemChecked(currentIndex, !ListBoxItemsData?[currentIndex]?.IsSelected ?? false);
			}
			lastSelected = currentIndex;
			NotifyRenderRequired();
		});
	}

	public ContentAlignment CheckAlign { get; set; }

	public string CheckedListBoxAppearanceStyleString()
	{
		string styleString = $"width: {Width}px;height: {Height}px;display: flex;flex-direction: column;";
		string scrollStyle = ScrollAlwaysVisible ? "scroll" : "auto";
		if (MultiColumn)
		{
			styleString += $"flex-wrap: wrap;overflow-x: {scrollStyle};";
		}
		else
		{
			styleString += $"overflow-y: {scrollStyle};";
		}

		return styleString;
	}

	public string ItemStyleString()
	{
		string result = $"height:{DefaultItemHeight}px;";
		if (MultiColumn)
		{
			string width = ColumnWidth == 0 ? "150" : ColumnWidth.ToString();
			result += $"min-width: {width}px;max-width: {width}px;";
		}
		return result;
	}

	public void SetItemCheckState(int index, CheckState value)
	{
		if (index < 0 || index >= Items.Count)
		{
			throw new ArgumentOutOfRangeException(nameof(index), index, string.Format(SR.InvalidArgument, nameof(index), index));
		}
		// valid values are 0-2 inclusive.
		if (!ClientUtils.IsEnumValid(value, (int)value, (int)CheckState.Unchecked, (int)CheckState.Indeterminate))
		{
			throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(CheckState));
		}
		CheckState currentValue = GetCheckedState(index);

		if (value != currentValue)
		{
			ItemCheckEventArgs itemCheckEvent = new ItemCheckEventArgs(index, value, currentValue);
			OnItemCheck(itemCheckEvent);

			if (itemCheckEvent.NewValue != currentValue)
			{
				SetCheckedState(index, itemCheckEvent.NewValue);
			}
		}
	}

	public CheckState GetItemCheckState(int index)
	{
		return GetCheckedState(index);
	}

	internal void SetCheckedState(int index, CheckState value)
	{
		CheckIndex(index);
		bool isChecked;

		switch (value)
		{
			case CheckState.Checked:
				isChecked = true;
				break;

			case CheckState.Indeterminate:
				isChecked = false;
				break;

			default:
				isChecked = false;
				break;
		}

		bool wasChecked = IsSelected(index);
		SetSelected(index, isChecked);
	}

	public override void ResetSelection(bool resetLastSelectedIndex = true)
	{
		RecreateDataItems();
		if (resetLastSelectedIndex)
		{
			lastSelected = NoMatches;
		}
	}

	public override void Select(int index)
	{
		var innerItem = Items.GetInnerItem(index);
		if (innerItem != null)
		{
			lastSelected = index;
			currentSelected = index;
			if (SelectedSet.Contains(innerItem))
			{
				SelectedSet.Remove(innerItem);
			}
			else
			{
				SelectedSet.Add(innerItem);
			}
			RecreateDataItems();
			if (IsHandleCreated)
			{
				OnSelectedChange();
			}
		}
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);

		if (e.KeyCode == Keys.Space)
		{
			if (SelectedIndex == NoMatches && Items.Count > 0)
			{
				currentSelected = 0;
				NotifyRenderRequired();
				return;
			}

			if (SelectedIndex != NoMatches && Items.Count > 0)
			{
				var isSelected = ListBoxItemsData?[SelectedIndex]?.IsSelected ?? false;
				SetItemChecked(SelectedIndex, !isSelected);
			}
		}
	}

	internal CheckState GetCheckedState(int index)
	{
		base.CheckIndex(index);
		bool isChecked = base.IsSelected(index);

		if (isChecked)
		{
			return CheckState.Checked;
		}

		return CheckState.Unchecked;
	}

	protected override ListBox.ObjectCollection CreateItemCollection()
	{
		return new ObjectCollection(this);
	}

	public override int SelectedIndex
	{
		get
		{
			var firstSelected = NoMatches;
			firstSelected = currentSelected;

			return firstSelected;
		}
		set
		{
			CheckIndex(value);
			lastSelected = value;
			SetSelected(value, true);
		}
	}

	public bool GetItemChecked(int index) => GetCheckedState(index) == CheckState.Checked;

	public event ItemCheckEventHandler? ItemCheck;

	public new event EventHandler? Click
	{
		add => base.Click += value;
		remove => base.Click -= value;
	}

	protected virtual void OnItemCheck(ItemCheckEventArgs ice)
	{
		ItemCheck?.Invoke(this, ice);
	}

	public new class ObjectCollection : ListBox.ObjectCollection
	{
		public ObjectCollection(ListBox owner)
		: base(owner)
		{
		}

		public new int Add(object item)
		{
			var innerItem = item as Entry;
			innerItem ??= new Entry(item);
			var index = AddInternal(innerItem);
			OnAdd(item, index);
			return index;
		}

		public int Add(object item, bool isChecked) => Add(item, isChecked ? CheckState.Checked : CheckState.Unchecked);

		public int Add(object item, CheckState check)
		{
			if (!ClientUtils.IsEnumValid(check, (int)check, (int)CheckState.Unchecked, (int)CheckState.Indeterminate))
			{
				throw new InvalidEnumArgumentException(nameof(check), (int)check, typeof(CheckState));
			}

			var innerItem = new Entry(item);
			if (check == CheckState.Checked)
			{
				Owner.SelectedSet.Add(innerItem);
			}

			return Add(innerItem);
		}
	}

	public class CheckedItemCollection : IList
	{
		readonly CheckedListBox owner;

		internal CheckedItemCollection(CheckedListBox owner)
		{
			this.owner = owner;
		}

		public object? this[int index]
		{
			get => throw new NotImplementedException();
			set => throw new NotSupportedException("CheckedListBox.CheckedItem Is ReadOnly");
		}

		/// <summary>
		///  Number of current checked items.
		/// </summary>
		public int Count
		{
			get
			{
				return owner.SelectedSet.Count;
			}
		}

		public bool IsFixedSize => true;

		public bool IsReadOnly => true;

		public bool IsSynchronized => false;

		public object SyncRoot => this;

		public int Add(object? value)
		{
			throw new NotSupportedException("CheckedListBox.CheckedItem Is ReadOnly");
		}

		public void Clear()
		{
			throw new NotSupportedException("CheckedListBox.CheckedItem Is ReadOnly");
		}

		public bool Contains(int index)
		{
			return (IndexOf(index) != -1);
		}

		bool IList.Contains(object? index)
		{
			if (index is int intIndex)
			{
				return Contains(intIndex);
			}
			return false;
		}

		public void CopyTo(Array array, int index)
		{
			foreach (var item in owner.SelectedSet)
			{
				array.SetValue((item as Entry)?.Item, index);
				index++;
			}
		}

		public IEnumerator GetEnumerator()
		{
			return owner.SelectedSet.GetEnumerator();
		}

		public int IndexOf(int index)
		{
			throw new NotImplementedException();
		}

		int IList.IndexOf(object? index)
		{
			if (index is int intIndex)
			{
				return IndexOf(intIndex);
			}
			return -1;
		}

		public void Insert(int index, object? value)
		{
			throw new NotSupportedException("CheckedListBox.CheckedItem Is ReadOnly");
		}

		public void Remove(object? value)
		{
			throw new NotSupportedException("CheckedListBox.CheckedItem Is ReadOnly");
		}

		public void RemoveAt(int index)
		{
			throw new NotSupportedException("CheckedListBox.CheckedItem Is ReadOnly");
		}
	}

	/// <summary>
	/// Based on CheckedIndexCollection from System.Windows.Forms of WinForms Version release/3.0
	/// https://github.com/dotnet/winforms/blob/release/3.0/src/System.Windows.Forms/src/System/Windows/Forms/CheckedListBox.cs#L1132
	/// </summary>
	public class CheckedIndexCollection : IList
	{
		readonly CheckedListBox owner;

		internal CheckedIndexCollection(CheckedListBox owner)
		{
			this.owner = owner;
		}

		/// <summary>
		///  Number of current checked items.
		/// </summary>
		public int Count => owner.CheckedItems.Count;

		object ICollection.SyncRoot => this;

		bool ICollection.IsSynchronized => false;

		bool IList.IsFixedSize => true;

		public bool IsReadOnly => true;

		/// <summary>
		///  Retrieves the specified checked item.
		/// </summary>
		public int this[int index]
		{
			get
			{
				return GetCheckedIndexList()[index];
			}
		}

		object? IList.this[int index]
		{
			get
			{
				return GetCheckedIndexList()[index];
			}
			set => throw new NotSupportedException("CheckedListBox.CheckedIndex Is ReadOnly");
		}

		int IList.Add(object? value)
		{
			throw new NotSupportedException("CheckedListBox.CheckedIndex Is ReadOnly");
		}

		void IList.Clear()
		{
			throw new NotSupportedException("CheckedListBox.CheckedIndex Is ReadOnly");
		}

		void IList.Insert(int index, object? value)
		{
			throw new NotSupportedException("CheckedListBox.CheckedIndex Is ReadOnly");
		}

		void IList.Remove(object? value)
		{
			throw new NotSupportedException("CheckedListBox.CheckedIndex Is ReadOnly");
		}

		void IList.RemoveAt(int index)
		{
			throw new NotSupportedException("CheckedListBox.CheckedIndex Is ReadOnly");
		}

		public bool Contains(int index)
		{
			return IndexOf(index) != -1;
		}

		bool IList.Contains(object? index)
		{
			if (index is int indexAsInt)
			{
				return Contains(indexAsInt);
			}

			return false;
		}

		public void CopyTo(Array dest, int index)
		{
			var cnt = owner.CheckedItems.Count;
			for (var i = 0; i < cnt; i++)
			{
				dest.SetValue(this[i], i + index);
			}
		}

		public IEnumerator GetEnumerator()
		{
			return GetCheckedIndexList().GetEnumerator();
		}

		public int IndexOf(int index)
		{
			if (index >= 0 && index < owner.Items.Count)
			{
				return GetCheckedIndexList().IndexOf(index);
			}

			return -1;
		}

		int IList.IndexOf(object? index)
		{
			if (index is int indexAsInt)
			{
				return IndexOf(indexAsInt);
			}

			return -1;
		}

		List<int> GetCheckedIndexList()
		{
			var items = new List<int>();
			for (var i = 0; i < owner.Items.Count; i++)
			{
				var innerItem = owner.Items.GetInnerItem(i);
				if (owner.SelectedSet.Contains(innerItem))
				{
					var itemToAdd = i;
					items.Add(itemToAdd);
				}
			}

			return items;
		}
	}
}

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using Microsoft.AspNetCore.Components;
using WinzorFramework;
using WinzorFramework.JSInterop;

namespace System.Windows.Forms;

[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Referenced in ComboBox.razor")]
public partial class ComboBox : ListControl
{
	public ComboBox()
	{
		items = new ObjectCollection(this);
	}

	public override bool UseParentDivForLayout => false;

	public ObjectCollection Items => items;
	readonly ObjectCollection items;

	[AllowNull]
	public override string Text
	{
		get
		{
			if (SelectedItem != null && !BindingFieldEmpty)
			{
				//preserve everett behavior if "formatting enabled == false" -- just return selecteditem text.
				if (FormattingEnabled)
				{
					string? candidate = GetItemText(SelectedItem);
					if (!string.IsNullOrEmpty(candidate))
					{
						if (string.Compare(candidate, base.Text, true, CultureInfo.CurrentCulture) == 0)
						{
							return candidate;   //for whidbey, if we only differ by case -- return the candidate;
						}
					}
				}
				else
				{
					return FilterItemOnProperty(SelectedItem)!.ToString()!;
				}
			}
			return base.Text;
		}
		set
		{
			if (DropDownStyle == ComboBoxStyle.DropDownList && !string.IsNullOrEmpty(value) && FindStringExact(value) == -1)
			{
				return;
			}

			base.Text = value;
			object? selectedItem = null;

			selectedItem = SelectedItem;

			if (!DesignMode)
			{
				if (value == null)
				{
					SelectedIndex = -1;
				}
				else if (selectedItem == null || (string.Compare(value, GetItemText(selectedItem), false, CultureInfo.CurrentCulture) != 0))
				{
					int index = FindStringIgnoreCase(value);

					//we cannot set the index to -1 unless we want to do something unusual and save/restore text
					//because the native control will erase the text when we change the index to -1
					if (index != -1)
					{
						SelectedIndex = index;
					}
				}
			}
		}
	}

	protected override Size DefaultSize => new Size(121, PreferredHeight);

	public int PreferredHeight => FontHeight;

	public override int SelectedIndex
	{
		get => selectedIndex;
		set
		{
			if (SelectedIndex != value)
			{
				int itemCount = 0;
				if (items != null)
				{
					itemCount = items.Count;
				}

				if (value < -1 || value >= itemCount)
				{
					throw new ArgumentOutOfRangeException(nameof(value), value, string.Format(SR.InvalidArgument, nameof(SelectedIndex), value));
				}

				UpdateProperty(ref selectedIndex, value);

				UpdateText();

				if (IsHandleCreated)
				{
					OnTextChanged(EventArgs.Empty);
				}

				OnSelectedItemChanged(EventArgs.Empty);
				OnSelectedIndexChanged(EventArgs.Empty);
			}
		}
	}
	int selectedIndex = -1;

	int highLightedIndex = -1;

	public event EventHandler? SelectedItemChanged;

	/// <summary>
	///  This is the code that actually fires the selectedItemChanged event.
	///  Don't forget to call base.onSelectedItemChanged() to ensure
	///  that selectedItemChanged events are correctly fired at all other times.
	/// </summary>
	protected virtual void OnSelectedItemChanged(EventArgs e)
	{
		SelectedItemChanged?.Invoke(this, e);
	}

	/// <summary>
	///  Forces the text to be updated based on the current selection.
	/// </summary>
	void UpdateText()
	{
		string? s = null;

		if (SelectedIndex != -1)
		{
			object item = Items[SelectedIndex];
			if (item != null)
			{
				s = GetItemText(item);
			}
		}

		Text = s;
	}

	string UpdateDropDownItemTag(int index)
	{
		if (highLightedIndex == -1)
		{
			return selectedIndex == index ? "combobox__dropdown-item--selected" : string.Empty;
		}
		else
		{
			return highLightedIndex == index && selectedIndex == highLightedIndex ? "combobox__dropdown-item--selected" : string.Empty;
		}
	}

	// Indicates whether the dropdown list will be closed  after
	// selection (on getting CBN_SELENDOK notification) to prevent
	// focusing on the list item after hiding the list.
	bool dropDownWillBeClosed;

	protected override void OnSelectedIndexChanged(EventArgs e)
	{
		base.OnSelectedIndexChanged(e);
		SelectedIndexChanged?.Invoke(this, e);

		if (dropDownWillBeClosed)
		{
			// This is after-closing selection - do not focus on the list item
			// and reset the state to announce the selections later.
			dropDownWillBeClosed = false;
		}

		// set the position in the dataSource, if there is any
		// we will only set the position in the currencyManager if it is different
		// from the SelectedIndex. Setting CurrencyManager::Position (even w/o changing it)
		// calls CurrencyManager::EndCurrentEdit, and that will pull the dataFrom the controls
		// into the backEnd. We do not need to do that.
		//
		// don't change the position if SelectedIndex is -1 because this indicates a selection not from the list.
		if (DataManager != null && DataManager.Position != SelectedIndex)
		{
			//read this as "if everett or   (whidbey and selindex is valid)"
			if (!FormattingEnabled || SelectedIndex != -1)
			{
				DataManager.Position = SelectedIndex;
			}
		}
	}

	public event EventHandler? SelectedIndexChanged;

	bool selectedValueChangedFired;

	protected override void OnSelectedValueChanged(EventArgs e)
	{
		base.OnSelectedValueChanged(e);
		selectedValueChangedFired = true;
	}

	/// <summary>
	///  The handle to the object that is currently selected in the
	///  combos list.
	/// </summary>
	public object? SelectedItem
	{
		get
		{
			int index = SelectedIndex;
			return (index == -1) ? null : Items[index];
		}
		set
		{
			int x = -1;

			if (items != null)
			{
				if (value != null)
				{
					x = items.IndexOf(value);
				}
				else
				{
					SelectedIndex = -1;
				}
			}

			if (x != -1)
			{
				SelectedIndex = x;
			}
		}
	}

	public int MaxLength { get; set; }

	public int MaxDropDownItems { get; set; }

	public int DropDownWidth
	{
		get => dropDownWidth ?? Width;
		set
		{
			UpdateProperty(ref dropDownWidth, value);
		}
	}
	int? dropDownWidth;

	public int DropDownHeight
	{
		get => dropDownHeight;
		set
		{
			if (UpdateProperty(ref dropDownHeight, value))
			{
				integralHeight = false;
			}
		}
	}

	const int DefaultDropDownHeight = 106;

	int dropDownHeight = DefaultDropDownHeight; // Winforms Default

	const int IntegralHeightLimit = 30;

	bool integralHeight = true;

	public bool IntegralHeight
	{
		get
		{
			return integralHeight;
		}

		set
		{
			if (integralHeight != value)
			{
				integralHeight = value;
			}
		}
	}

	public int CalculateHeight()
	{
		var itemCount = items.Count;
		var calculatedIntegralHeight = Math.Min(itemCount, IntegralHeightLimit) * ItemHeight;
		var calculatedSpecificHeight = dropDownHeight;
		if (dropDownHeight == DefaultDropDownHeight)
		{
			calculatedSpecificHeight = Math.Min(itemCount, MaxDropDownItems) * ItemHeight;
		}

		if (!integralHeight)
		{
			return calculatedSpecificHeight;
		}
		else
		{
			return Math.Max(calculatedSpecificHeight, calculatedIntegralHeight);
		}
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		return ProcessCmdKeyCore(keyData) || base.ProcessCmdKey(ref msg, keyData);
	}

	protected virtual bool ProcessCmdKeyCore(Keys keyData)
	{
		if (keyData == Keys.Enter && DroppedDown)
		{
			if (highLightedIndex != -1)
			{
				SelectedIndex = highLightedIndex;
			}
			DroppedDown = false;
			return true;
		}
		return false;
	}

	public int ItemHeight
	{
		get
		{
			if (itemHeight == 0)
			{
				return Font.Height;
			}
			return itemHeight;
		}

		set
		{
			if (value < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value, string.Format(SR.InvalidArgument, nameof(ItemHeight), value));
			}

			UpdateProperty(ref itemHeight, value);
		}
	}

	int itemHeight; // Font Size + 2

	public ComboBoxStyle DropDownStyle
	{
		get => dropdownStyle;
		set
		{
			if (value == ComboBoxStyle.Simple)
			{
				throw new NotImplementedException("This ComboBoxStyle has not been implemented");
			}
			UpdateProperty(ref dropdownStyle, value);
		}
	}
	ComboBoxStyle dropdownStyle = ComboBoxStyle.DropDown;

	public AutoCompleteMode AutoCompleteMode
	{
		get => autoCompleteMode;
		set
		{
			if (UpdateProperty(ref autoCompleteMode, value) && value != AutoCompleteMode.None)
			{
				SetAutoCompleteItems();
			}
		}
	}
	AutoCompleteMode autoCompleteMode = AutoCompleteMode.None;

	bool showSuggestionsDropdown => AutoCompleteMode == AutoCompleteMode.Suggest || AutoCompleteMode == AutoCompleteMode.SuggestAppend;
	bool appendAutoComplete => AutoCompleteMode == AutoCompleteMode.Append || AutoCompleteMode == AutoCompleteMode.SuggestAppend;

	public int SelectionStart
	{
		get => selectionStart;
		set
		{
			UpdateProperty(ref selectionStart, value);
		}
	}
	int selectionStart;

	public int SelectionLength
	{
		get => selectionLength;
		set
		{
			UpdateProperty(ref selectionLength, value);
		}
	}
	int selectionLength;

	int FindStringIgnoreCase(string value)
	{
		// Copied from Winforms
		//look for an exact match and then a case insensitive match if that fails.
		int index = FindStringExact(value, -1, false);

		if (index == -1)
		{
			index = FindStringExact(value, -1, true);
		}

		return index;
	}

	public int FindString(string s) => FindString(s, startIndex: -1);

	public int FindString(string s, int startIndex)
	{
		return FindStringInternal(s, items, startIndex, exact: false, ignoreCase: true);
	}

	public int FindStringExact(string s)
	{
		return FindStringExact(s, startIndex: -1, ignoreCase: true);
	}

	public int FindStringExact(string s, int startIndex)
	{
		return FindStringExact(s, startIndex, ignoreCase: true);
	}

	internal int FindStringExact(string s, int startIndex, bool ignoreCase)
	{
		return FindStringInternal(s, items, startIndex, exact: true, ignoreCase);
	}

	public bool DroppedDown
	{
		get => droppedDown;
		set
		{
			if (UpdateProperty(ref droppedDown, value))
			{
				if (value)
				{
					OnDropDown(EventArgs.Empty);
				}
				else
				{
					OnDropDownClosed(EventArgs.Empty);
				}
			}
		}
	}
	bool droppedDown;

	protected virtual void OnDropDown(EventArgs e)
	{
		highLightedIndex = -1;
		DropDown?.Invoke(this, e);
	}
	public event EventHandler? DropDown;

	/// <summary>
	///  This event is fired when the dropdown portion of the combobox is hidden.
	/// </summary>
	protected virtual void OnDropDownClosed(EventArgs e)
	{
		DropDownClosed?.Invoke(this, e);

		// Collapsing the DropDown, so reset the flag.
		dropDownWillBeClosed = false;
	}

	public event EventHandler? DropDownClosed;

	public event EventHandler? SelectionChangeCommitted;

	/// <summary>
	///  This is the code that actually fires the SelectionChangeCommitted event.
	///  Don't forget to call base.OnSelectionChangeCommitted() to ensure
	///  that SelectionChangeCommitted events are correctly fired at all other times.
	/// </summary>
	protected virtual void OnSelectionChangeCommitted(EventArgs e)
	{
		SelectionChangeCommitted?.Invoke(this, e);

		// The user selects a list item or selects an item and then closes the list.
		// It indicates that the user's selection is to be processed but should not
		// be focused after closing the list.
		if (DroppedDown)
		{
			dropDownWillBeClosed = true;
		}
	}

	public AutoCompleteSource AutoCompleteSource
	{
		get => autoCompleteSource;
		set
		{
			if (UpdateProperty(ref autoCompleteSource, value))
			{
				SetAutoCompleteItems();
			}
		}
	}
	AutoCompleteSource autoCompleteSource = AutoCompleteSource.None;

	[SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "Analyzer runner incorrectly detecting.")]
	ElementReference inputReference;

	async Task OnInputAsync(ChangeEventArgs args)
	{
		if (!Enabled)
		{
			return;
		}

		var inputValue = args?.Value?.ToString();
		if (inputValue is null)
		{
			return;
		}

		if (showSuggestionsDropdown)
		{
			suggestionItems = string.IsNullOrEmpty(inputValue) ? Array.Empty<string>() : autoCompleteItems.Where(i => i.Contains(inputValue, StringComparison.InvariantCultureIgnoreCase)).ToArray();
			if (suggestionItems.Length == 1 && string.Equals(suggestionItems[0], inputValue, StringComparison.InvariantCultureIgnoreCase))
			{
				suggestionItems = Array.Empty<string>();
			}
			await InvokeWinzorDispatcherAsync(() => NotifyRenderRequired());
		}
		if (appendAutoComplete && previousInputValue.Length < inputValue.Length)
		{
			var item = autoCompleteItems.FirstOrDefault(i => i.StartsWith(inputValue, StringComparison.InvariantCultureIgnoreCase));
			if (!string.IsNullOrEmpty(item))
			{
				await (GetJSInterop<ITextBoxJSInterop>()?.SetTextContentAsync(inputReference, item) ?? Task.CompletedTask);
				await (GetJSInterop<ITextBoxJSInterop>()?.SetSelectionAsync(inputReference, inputValue.Length, item.Length) ?? Task.CompletedTask);
			}
		}
		previousInputValue = inputValue;
	}

	string[] suggestionItems = Array.Empty<string>();
	string previousInputValue = string.Empty;

	async Task OnDropdownButtonClickAsync()
	{
		if (Enabled)
		{
			await InvokeWinzorDispatcherAsync(() =>
			{
				DroppedDown = !DroppedDown;
			});

			if (DroppedDown)
			{
				await (GetJSInterop<ITextBoxJSInterop>()?.FocusAsync(inputReference) ?? Task.CompletedTask);
			}
		}
	}

	async Task OnDropdownItemClickAsync(WebMouseEventArgs args, int index)
	{
		if (args.Button == 0) //consider left click only
		{
			await InvokeWinzorDispatcherAsync(() =>
			{
				SelectedIndex = index;
				CloseDropdown();
			});
		}
	}

	async Task OnDropdownItemMouseOverAsync(int index)
	{
		await InvokeWinzorDispatcherAsync(() =>
		{
			highLightedIndex = index;
			UpdateDropDownItemTag(index);
			NotifyRenderRequired();
		});
	}

	async Task OnSuggestionItemClickAsync(WebMouseEventArgs args, string item)
	{
		if (args.Button == 0) //consider left click only
		{
			await InvokeWinzorDispatcherAsync(() =>
			{
				SelectedIndex = FindStringExact(item, 0, false);
				CloseDropdown();
			});
		}
	}

	void CloseDropdown()
	{
		DroppedDown = false;
		suggestionItems = Array.Empty<string>();
		NotifyRenderRequired();
	}

	protected override void RefreshItems()
	{
		var selectedIndex = SelectedIndex;
		var savedItems = items;

		items.Clear();
		object[]? newItems = null;

		// Copied From Winforms
		if (DataManager is not null && DataManager.Count != -1)
		{
			newItems = new object[DataManager.Count];
			for (int i = 0; i < newItems.Length; i++)
			{
				newItems[i] = DataManager[i];
			}
		}
		else if (savedItems is not null)
		{
			newItems = new object[savedItems.Count];
			savedItems.CopyTo(newItems, arrayIndex: 0);
		}

		if (newItems is not null)
		{
			Items.AddRange(newItems);
		}

		if (DataManager is not null)
		{
			SelectedIndex = DataManager.Position;
		}
		else
		{
			SelectedIndex = selectedIndex;
		}
		SetAutoCompleteItems();
	}

	protected override void OnDataSourceChanged(EventArgs e)
	{
		if (DataSource == null)
		{
			SelectedIndex = -1;
			Items.Clear();
		}
		base.OnDataSourceChanged(e);
		RefreshItems();
	}

	protected override void OnDisplayMemberChanged(EventArgs e)
	{
		base.OnDisplayMemberChanged(e);
		RefreshItems();
	}

	protected override void SetItemsCore(IList items)
	{
		Items.Clear();
		foreach (object item in items)
		{
			Items.Add(item);
		}

		// if the list changed, we want to keep the same selected index
		// CurrencyManager will provide the PositionChanged event
		// it will be provided before changing the list though...
		if (DataManager != null)
		{
			if (DataSource is ICurrencyManagerProvider)
			{
				selectedValueChangedFired = false;
			}

			SelectedIndex = DataManager.Position;

			// if set_SelectedIndexChanged did not fire OnSelectedValueChanged
			// then we have to fire it ourselves, cos the list changed anyway
			if (!selectedValueChangedFired)
			{
				OnSelectedValueChanged(EventArgs.Empty);
				selectedValueChangedFired = false;
			}
		}

		SetAutoCompleteItems();
	}

	protected override void SetItemCore(int index, object value)
	{
		Items[index] = value;
		SetAutoCompleteItems();
	}

	protected override void OnLostFocus(EventArgs e)
	{
		base.OnLostFocus(e);
		if (Enabled)
		{
			CloseDropdown();
		}
	}

	protected override void OnGotFocus(EventArgs e)
	{
		base.OnGotFocus(e);
		if (DropDownStyle != ComboBoxStyle.DropDownList || !Enabled)
		{
			RegisterAfterRenderAction(async () => await ((GetJSInterop<ITextBoxJSInterop>()?.SelectAllAsync(inputReference) ?? Task.CompletedTask)));
		}
	}

	void SetAutoCompleteItems()
	{
		autoCompleteItems.Clear();
		switch (AutoCompleteSource)
		{
			case AutoCompleteSource.None:
				{
					break;
				}
			case AutoCompleteSource.ListItems:
				{
					for (int i = 0; i < Items.Count; i++)
					{
						var item = Items[i];
						var itemText = GetItemText(item);
						if (!string.IsNullOrEmpty(itemText))
						{
							autoCompleteItems.Add(itemText);
						}
					}
					break;
				}
			default:
				{
					throw new NotImplementedException("AutoCompleteSource not implemented");
				}
		}
	}
	readonly WrappedList<string> autoCompleteItems = new WrappedList<string>();

	public class ObjectCollection : WrappedList<object>
	{
		public ObjectCollection(ComboBox owner)
		{
			this.owner = owner;
		}

		public override void Remove(object item)
		{
			var index = inner.IndexOf(item);
			if (index != -1)
			{
				RemoveAt(index);
			}
		}

		public override void RemoveAt(int index)
		{
			base.RemoveAt(index);
			if (index < owner.SelectedIndex)
			{
				owner.selectedIndex--;
			}
		}

		public override void Clear()
		{
			base.Clear();
			owner.selectedIndex = -1;
		}

		readonly ComboBox owner;
	}
}

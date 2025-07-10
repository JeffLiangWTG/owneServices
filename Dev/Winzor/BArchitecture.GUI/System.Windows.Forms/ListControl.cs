using System.Collections;
using System.ComponentModel;
using System.Globalization;

namespace System.Windows.Forms;

public abstract class ListControl : Control
{
	public bool FormattingEnabled
	{
		get => formattingEnabled;
		set
		{
			if (UpdateProperty(ref formattingEnabled, value))
			{
				RefreshItems();
				OnFormattingEnabledChanged(EventArgs.Empty);
			}
		}
	}

	bool formattingEnabled;

	public event EventHandler? FormattingEnabledChanged;

	protected virtual void OnFormattingEnabledChanged(EventArgs e)
	{
		FormattingEnabledChanged?.Invoke(this, e);
	}

	public string DisplayMember
	{
		get => displayMember.BindingMember;
		set
		{
			SetDataConnection(dataSource, new BindingMemberInfo(value), force: false);
		}
	}
	BindingMemberInfo displayMember = new BindingMemberInfo(string.Empty);

	public string ValueMember
	{
		get => valueMember.BindingMember;
		set
		{
			if (value == null)
			{
				value = string.Empty;
			}

			BindingMemberInfo newValueMember = new BindingMemberInfo(value);
			if (!newValueMember.Equals(valueMember))
			{
				// If the displayMember is set to the EmptyString, then recreate the dataConnection
				if (DisplayMember.Length == 0)
				{
					SetDataConnection(DataSource, newValueMember, force: false);
				}

				// See if the valueMember is a member of
				// the properties in the dataManager
				if (DataManager != null && !string.IsNullOrEmpty(value))
				{
					if (!BindingMemberInfoInDataManager(DataManager, newValueMember))
					{
						throw new ArgumentException(SR.ListControlWrongValueMember, nameof(value));
					}
				}

				valueMember = newValueMember;
				OnValueMemberChanged(EventArgs.Empty);
				OnSelectedValueChanged(EventArgs.Empty);
			}
		}
	}

	BindingMemberInfo valueMember;

	public event EventHandler? ValueMemberChanged;

	public event EventHandler? SelectedValueChanged;

	protected virtual void OnValueMemberChanged(EventArgs e)
	{
		ValueMemberChanged?.Invoke(this, e);
	}

	protected virtual void OnSelectedValueChanged(EventArgs e)
	{
		SelectedValueChanged?.Invoke(this, e);
	}

	/// <summary>
	///  The ListSource to consume as this ListBox's source of data.
	///  When set, a user can not modify the Items collection.
	/// </summary>
	public object? DataSource
	{
		get => dataSource;
		set
		{
			if (value != null && !(value is IList || value is IListSource))
			{
				throw new ArgumentException(SR.BadDataSourceForComplexBinding, nameof(value));
			}

			if (dataSource == value)
			{
				return;
			}

			// When we change the dataSource to null, we should reset
			// the displayMember to "".
			try
			{
				SetDataConnection(value, displayMember, force: false);
			}
			catch
			{
				// There are several possibilities why setting the data source throws an exception:
				// 1. the app throws an exception in the events that fire when we change the data source: DataSourceChanged,
				// 2. we get an exception when we set the data source and populate the list controls (say,something went wrong while formatting the data)
				// 3. the DisplayMember does not fit w/ the new data source (this could happen if the user resets the data source but did not reset the DisplayMember)
				// in all cases ListControl should reset the DisplayMember to String.Empty
				// the ListControl should also eat the exception - this is the RTM behavior and doing anything else is a breaking change
				DisplayMember = string.Empty;
			}

			if (value == null)
			{
				DisplayMember = string.Empty;
			}
		}
	}
	object? dataSource;

	public abstract int SelectedIndex { get; set; }

	public object? SelectedValue { get; set; }

	protected CurrencyManager? DataManager => dataManager;
	CurrencyManager? dataManager;

	/// <summary>
	///  Actually goes and fires the selectedIndexChanged event. Inheriting controls
	///  should use this to know when the event is fired [this is preferable to
	///  adding an event handler on yourself for this event]. They should,
	///  however, remember to call base.OnSelectedIndexChanged(e); to ensure the event is
	///  still fired to external listeners
	/// </summary>
	protected virtual void OnSelectedIndexChanged(EventArgs e)
	{
		OnSelectedValueChanged(e);
	}

	public event EventHandler? DataSourceChanged;

	protected virtual void OnDataSourceChanged(EventArgs e)
	{
		DataSourceChanged?.Invoke(this, e);
	}
	public event EventHandler? DisplayMemberChanged;

	protected virtual void OnDisplayMemberChanged(EventArgs e)
	{
		DisplayMemberChanged?.Invoke(this, e);
	}
	protected override void OnBindingContextChanged(EventArgs e)
	{
		SetDataConnection(dataSource, displayMember, force: true);
		base.OnBindingContextChanged(e);
	}

	/// <remarks>
	///  We use this to prevent getting the selected item when mouse is hovering
	///  over the dropdown.
	/// </remarks>
	private protected bool BindingFieldEmpty => displayMember.BindingField.Length == 0;

	private protected int FindStringInternal(string str, IList items, int startIndex, bool exact, bool ignoreCase)
	{
		// Copied from Winforms
		if (str is null)
		{
			return -1;
		}

		if (items is null || items.Count == 0)
		{
			return -1;
		}

		if (startIndex < -1 || startIndex >= items.Count)
		{
			throw new ArgumentOutOfRangeException(nameof(startIndex));
		}

		// Start from the start index and wrap around until we find the string
		// in question. Use a separate counter to ensure that we aren't cycling through the list infinitely.
		int numberOfTimesThroughLoop = 0;

		// this API is really Find NEXT String...
		for (int index = (startIndex + 1) % items.Count; numberOfTimesThroughLoop < items.Count; index = (index + 1) % items.Count)
		{
			numberOfTimesThroughLoop++;

			bool found;
			if (exact)
			{
				found = string.Compare(str, GetItemText(items[index]), ignoreCase, CultureInfo.CurrentCulture) == 0;
			}
			else
			{
				found = string.Compare(str, 0, GetItemText(items[index]), 0, str.Length, ignoreCase, CultureInfo.CurrentCulture) == 0;
			}

			if (found)
			{
				return index;
			}
		}

		return -1;
	}

	public string? GetItemText(object? item)
	{
		if (item is null)
		{
			return string.Empty;
		}

		item = FilterItemOnProperty(item, displayMember.BindingField);
		if (item is null)
		{
			return string.Empty;
		}

		return Convert.ToString(item, CultureInfo.CurrentCulture);
	}

	protected object? FilterItemOnProperty(object? item)
	{
		return FilterItemOnProperty(item, displayMember.BindingField);
	}

	protected object? FilterItemOnProperty(object? item, string? field)
	{
		if (item is not null && !string.IsNullOrEmpty(field))
		{
			try
			{
				// if we have a dataSource, then use that to display the string
				PropertyDescriptor? descriptor;
				if (DataManager is not null)
				{
					descriptor = DataManager.GetItemProperties().Find(field, true);
				}
				else
				{
					descriptor = TypeDescriptor.GetProperties(item).Find(field, true);
				}

				if (descriptor is not null)
				{
					item = descriptor.GetValue(item);
				}
			}
			catch
			{
			}
		}

		return item;
	}

	protected virtual void RefreshItems()
	{
	}

	protected virtual bool AllowSelection => true;

	void DataManager_PositionChanged(object? sender, EventArgs e)
	{
		// Copied from Winforms
		if (dataManager is not null)
		{
			if (AllowSelection)
			{
				SelectedIndex = dataManager.Position;
			}
		}
	}

	void DataManager_ItemChanged(object? sender, ItemChangedEventArgs e)
	{
		// Copied from Winforms
		// Note this is being called internally with a null event.
		if (dataManager is not null)
		{
			if (e.Index == -1)
			{
				SetItemsCore(dataManager.List);
				if (AllowSelection)
				{
					SelectedIndex = dataManager.Position;
				}
			}
			else
			{
				SetItemCore(e.Index, dataManager[e.Index]);
			}
		}
	}

	static bool BindingMemberInfoInDataManager(CurrencyManager dataManager, BindingMemberInfo bindingMemberInfo)
	{
		// Copied from Winforms
		PropertyDescriptorCollection props = dataManager.GetItemProperties();

		for (int i = 0; i < props.Count; i++)
		{
			if (typeof(IList).IsAssignableFrom(props[i].PropertyType))
			{
				continue;
			}

			if (props[i].Name.Equals(bindingMemberInfo.BindingField))
			{
				return true;
			}
		}

		for (int i = 0; i < props.Count; i++)
		{
			if (typeof(IList).IsAssignableFrom(props[i].PropertyType))
			{
				continue;
			}

			if (string.Equals(props[i].Name, bindingMemberInfo.BindingField, StringComparison.CurrentCultureIgnoreCase))
			{
				return true;
			}
		}

		return false;
	}

	void SetDataConnection(object? newDataSource, BindingMemberInfo newDisplayMember, bool force)
	{
		// Copied from Winforms
		var dataSourceChanged = DataSource != newDataSource;
		var displayMemberChanged = !displayMember.Equals(newDisplayMember);

		if (inSetDataConnection)
		{
			return;
		}

		try
		{
			if (force || dataSourceChanged || displayMemberChanged)
			{
				inSetDataConnection = true;
				IList? currentList = DataManager?.List;
				bool currentManagerIsNull = DataManager is null;

				UnwireDataSource();

				dataSource = newDataSource;
				displayMember = newDisplayMember;

				WireDataSource();

				// Provided the data source has been fully initialized, start listening to change events on its
				// currency manager and refresh our list. If the data source has not yet been initialized, we will
				// skip this step for now, and try again later (once the data source has fired its Initialized event).
				if (isDataSourceInitialized)
				{
					CurrencyManager? newDataManager = null;
					if (newDataSource is not null && BindingContext is not null && newDataSource != Convert.DBNull)
					{
						newDataManager = (CurrencyManager)BindingContext[newDataSource, newDisplayMember.BindingPath];
					}

					if (dataManager != newDataManager)
					{
						if (dataManager is not null)
						{
							dataManager.ItemChanged -= new ItemChangedEventHandler(DataManager_ItemChanged);
							dataManager.PositionChanged -= new EventHandler(DataManager_PositionChanged);
						}

						dataManager = newDataManager;

						if (dataManager is not null)
						{
							dataManager.ItemChanged += new ItemChangedEventHandler(DataManager_ItemChanged);
							dataManager.PositionChanged += new EventHandler(DataManager_PositionChanged);
						}
					}

					// See if the BindingField in the newDisplayMember is valid
					// The same thing if dataSource Changed
					// "" is a good value for displayMember
					if (dataManager is not null && (displayMemberChanged || dataSourceChanged) && !string.IsNullOrEmpty(displayMember.BindingMember))
					{
						if (!BindingMemberInfoInDataManager(dataManager, displayMember))
						{
							throw new ArgumentException(SR.ListControlWrongDisplayMember, nameof(newDisplayMember));
						}
					}

					if (dataManager is not null && (dataSourceChanged || displayMemberChanged || force))
					{
						// If we force a new data manager, then change the items in the list control
						// only if the list changed or if we go from a null dataManager to a full fledged one
						// or if the DisplayMember changed
						if (displayMemberChanged || (force && (currentList != dataManager.List || currentManagerIsNull)))
						{
							DataManager_ItemChanged(dataManager, new ItemChangedEventArgs(-1));
						}
					}
				}
			}

			if (dataSourceChanged)
			{
				OnDataSourceChanged(EventArgs.Empty);
			}

			if (displayMemberChanged)
			{
				OnDisplayMemberChanged(EventArgs.Empty);
			}
		}
		finally
		{
			inSetDataConnection = false;
		}
	}

	void DataSourceDisposed(object? sender, EventArgs e)
	{
		// Copied from Winforms
		SetDataConnection(null, new BindingMemberInfo(string.Empty), true);
	}

	void DataSourceInitialized(object? sender, EventArgs e)
	{
		// Copied from Winforms
		SetDataConnection(dataSource, displayMember, true);
	}

	void UnwireDataSource()
	{
		// Copied from Winforms
		// If the source is a component, then unhook the Disposed event
		if (dataSource is IComponent componentDataSource)
		{
			componentDataSource.Disposed -= new EventHandler(DataSourceDisposed);
		}

		if (dataSource is ISupportInitializeNotification dsInit && isDataSourceInitEventHooked)
		{
			// If we previously hooked the data source's ISupportInitializeNotification
			// Initialized event, then unhook it now (we don't always hook this event,
			// only if we needed to because the data source was previously uninitialized)
			dsInit.Initialized -= new EventHandler(DataSourceInitialized);
			isDataSourceInitEventHooked = false;
		}
	}

	void WireDataSource()
	{
		// Copied from Winforms
		// If the source is a component, then hook the Disposed event,
		// so we know when the component is deleted from the form
		if (dataSource is IComponent componentDataSource)
		{
			componentDataSource.Disposed += new EventHandler(DataSourceDisposed);
		}

		if (dataSource is ISupportInitializeNotification dsInit && !dsInit.IsInitialized)
		{
			// If the source provides initialization notification, and is not yet
			// fully initialized, then hook the Initialized event, so that we can
			// delay connecting to it until it *is* initialized.
			dsInit.Initialized += new EventHandler(DataSourceInitialized);
			isDataSourceInitEventHooked = true;
			isDataSourceInitialized = false;
		}
		else
		{
			// Otherwise either the data source says it *is* initialized, or it
			// does not support the capability to report whether its initialized,
			// in which case we have to just assume it that is initialized.
			isDataSourceInitialized = true;
		}
	}
	bool isDataSourceInitialized;
	bool isDataSourceInitEventHooked;
	bool inSetDataConnection;

	protected abstract void SetItemsCore(IList items);

	protected virtual void SetItemCore(int index, object value)
	{
	}
}

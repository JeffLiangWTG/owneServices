using WinzorFramework;

namespace System.Windows.Forms;

public class StatusBarPanelCollection : WrappedList<StatusBarPanel>
{
	/// <summary>
	///  Returns true if the collection contains an item with the specified key, false otherwise.
	/// </summary>
	public virtual bool ContainsKey(string? key)
	{
		return IsValidIndex(IndexOfKey(key));
	}

	///  A caching mechanism for key accessor
	///  We use an index here rather than control so that we don't have lifetime
	///  issues by holding on to extra references.
	///  Note this is not Thread Safe - but WinForms has to be run in a STA anyways.
	int lastAccessedIndex = -1;

	/// <summary>
	///  The zero-based index of the first occurrence of value within the entire CollectionBase, if found; otherwise, -1.
	/// </summary>
	public virtual int IndexOfKey(string? key)
	{
		if (string.IsNullOrEmpty(key))
		{
			return -1;
		}

		if (IsValidIndex(lastAccessedIndex))
		{
			if (WindowsFormsUtils.SafeCompareStrings(this[lastAccessedIndex].Name, key, ignoreCase: true))
			{
				return lastAccessedIndex;
			}
		}

		for (var i = 0; i < Count; i++)
		{
			if (WindowsFormsUtils.SafeCompareStrings(this[i].Name, key, ignoreCase: true))
			{
				lastAccessedIndex = i;
				return i;
			}
		}

		lastAccessedIndex = -1;
		return -1;
	}

	/// <summary>
	///  Removes the child control with the specified key.
	/// </summary>
	public virtual void RemoveByKey(string? key)
	{
		var index = IndexOfKey(key);
		if (IsValidIndex(index))
		{
			RemoveAt(index);
		}
	}

	/// <summary>
	///  Determines if the index is valid for the collection.
	/// </summary>
	bool IsValidIndex(int index)
	{
		return ((index >= 0) && (index < Count));
	}
}

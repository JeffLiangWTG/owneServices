using System.Collections;

namespace System.Windows.Forms;

public partial class ListBox
{
	public class SelectedObjectCollection : ListBoxSelectedCollection, IList
	{
		public SelectedObjectCollection(ListBox owner) : base(owner)
		{
		}

		public object? this[int index]
		{
			get => GetSelectedItemList(false)[index];
			set => throw new NotImplementedException();
		}

		int IList.Add(object? value) => throw new NotImplementedException();

		public void Add(object item)
		{
			if (!Owner.SelectedSet.Contains(Owner.Items.GetInnerItem(item)))
			{
				var itemIndex = FindItemIndex(item);
				Owner.SetSelected(itemIndex, true);
			}
		}

		public void Remove(object? item)
		{
			if (item is null)
			{
				return;
			}

			if (Owner.SelectedSet.Contains(Owner.Items.GetInnerItem(item)))
			{
				var itemIndex = FindItemIndex(item);
				Owner.SetSelected(itemIndex, false);
			}
		}

		public IEnumerator<object> GetEnumerator()
		{
			return GetSelectedItemList(false).GetEnumerator();
		}

		int FindItemIndex(object target)
		{
			var foundIndex = NoMatches;

			for (var i = 0; i < Owner.Items.Count; i++)
			{
				if (object.ReferenceEquals(Owner.Items[i], target))
				{
					foundIndex = i;
					break;
				}
			}

			return foundIndex;
		}

		public bool Contains(object? value) => value is not null && Owner.SelectedSet.Contains(Owner.Items.GetInnerItem(value));

		public int IndexOf(object? value) => value is null ? -1 : FindItemIndex(value);

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}

using System.Collections;

namespace System.Windows.Forms;

public partial class ListBox
{
	public class SelectedIndexCollection : ListBoxSelectedCollection, IList
	{
		public SelectedIndexCollection(ListBox owner) : base(owner)
		{
		}

		public int this[int index]
		{
			get
			{
				return (int)GetSelectedItemList(true)[index];
			}
			set => throw new NotImplementedException();
		}

		object? IList.this[int index]
		{
			get
			{
				return GetSelectedItemList(true)[index];
			}
			set => throw new NotImplementedException();
		}

		int IList.Add(object? value) => throw new NotImplementedException();

		public void Add(int index)
		{
			if (!Owner.SelectedSet.Contains(Owner.Items.GetInnerItem(index)))
			{
				Owner.SetSelected(index, true);
			}
		}

		void IList.Remove(object? value) => throw new NotImplementedException();

		public void Remove(int index)
		{
			if (Owner.SelectedSet.Contains(Owner.Items.GetInnerItem(index)))
			{
				Owner.SetSelected(index, false);
			}
		}

		public IEnumerator<object> GetEnumerator()
		{
			return GetSelectedItems();
		}

		IEnumerator<object> GetSelectedItems()
		{
			return GetSelectedItemList(true).GetEnumerator();
		}

		public bool Contains(object? value)
		{
			if (value is null)
			{
				return false;
			}
			return GetSelectedItemList(true).Contains(value);
		}

		public int IndexOf(object? value)
		{
			if (value is null)
			{
				return -1;
			}
			return GetSelectedItemList(true).IndexOf(value);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}

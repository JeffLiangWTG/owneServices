namespace System.Windows.Forms;

public partial class ListBox
{
	public abstract class ListBoxSelectedCollection
	{
		public ListBoxSelectedCollection(ListBox owner)
		{
			Owner = owner;
		}

		protected ListBox Owner { get; }

		public int Count => Owner.SelectedSet.Count;

		public bool IsFixedSize => false;

		public bool IsReadOnly => true;

		public bool IsSynchronized => false;

		public object SyncRoot => false;

		public void Clear()
		{
			Owner.SelectedSet.Clear();
		}

		public void CopyTo(Array array, int index)
		{
			Array.Copy(Owner.SelectedSet.ToArray(), 0, array, index, Owner.SelectedSet.Count);
		}

		protected List<object> GetSelectedItemList(bool returnIndex)
		{
			var items = new List<object>();
			for (var i = 0; i < Owner.Items.Count; i++)
			{
				var innerItem = Owner.Items.GetInnerItem(i);
				if (Owner.SelectedSet.Contains(innerItem))
				{
					var itemToAdd = returnIndex ? i : innerItem.Item;
					items.Add(itemToAdd);
				}
			}

			return items;
		}

		public void Insert(int index, object? value) => throw new NotImplementedException();

		public void RemoveAt(int index) => throw new NotImplementedException();
	}
}

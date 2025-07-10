using System.Collections;
using WinzorFramework;

namespace System.Windows.Forms;

public partial class ListBox
{
	public class ObjectCollection : WrappedList<object>
	{
		protected ListBox Owner { get; set; }

		public override object this[int index]
		{
			get => ((Entry)base[index]).Item;
			set => ((Entry)base[index]).Item = value;
		}

		internal Entry GetInnerItem(int index) => (Entry)base[index];

#pragma warning disable CS8603 // Possible null reference return.
		internal Entry GetInnerItem(object item) => inner.OfType<Entry>().FirstOrDefault(e => e.Item == item);
#pragma warning restore CS8603 // Possible null reference return.

		public void Sort()
		{
			ArrayList.Adapter(this).Sort(new OjectCollectionItemComparer(Owner));
		}

		public override IEnumerator<object> GetEnumerator()
		{
			return new EntryEnumerator(inner.OfType<Entry>().ToList());
		}

		public ObjectCollection(ListBox owner)
		{
			Owner = owner;
		}

		public override void Insert(int index, object item)
		{
			if (Owner._sorted)
			{
				Add(item);
			}
			else
			{
				var innerItem = item as Entry;
				innerItem ??= new Entry(item);

				base.Insert(index, innerItem);
			}
		}

		public override int IndexOf(object item)
		{
			var innerItem = item as Entry;
			innerItem ??= GetInnerItem(item);

			return innerItem != null ? inner.OfType<Entry>().ToList().IndexOf(innerItem) : -1;
		}

		public override void Remove(object item)
		{
			var innerItem = item as Entry;
			if (innerItem == null && item != null)
			{
				innerItem = GetInnerItem(item);
			}

#pragma warning disable CS8604 // Possible null reference argument.
			base.Remove(innerItem);
#pragma warning restore CS8604 // Possible null reference argument.
		}

		public override void Add(object item)
		{
			var innerItem = item as Entry;
			innerItem ??= new Entry(item);

			var index = AddInternal(innerItem);
			OnAdd(item, index);
		}

		protected override void OnAdd(object item, int index)
		{
			base.OnAdd(item, index);
			Owner.RecreateDataItems();
		}

		public override bool Contains(object item)
		{
			var innerItem = item as Entry;
			innerItem ??= GetInnerItem(item);

			return innerItem != null && base.Contains(innerItem);
		}

		protected int AddInternal(object item)
		{
			int index = -1;
			if (Owner._sorted)
			{
				if (inner.Count > 0)
				{
					index = inner.BinarySearch(item, new OjectCollectionItemComparer(Owner));
					if (index < 0)
					{
						// getting the index of the first element that is larger than the search value
						// this index will be used for insert
						index = ~index;
					}
				}
				else
				{
					index = 0;
				}
				inner = inner.Insert(index, item);
			}
			else
			{
				inner = inner.Add(item);
				index = inner.Count - 1;
			}
			return index;
		}

		protected override void OnRemove(object item)
		{
			base.OnRemove(item);
			Owner.SelectedSet.Remove(item);
			Owner.RecreateDataItems();
			Owner.lastSelectedIndex = NoMatches;
		}

		class OjectCollectionItemComparer : IComparer<object>, IComparer
		{
			ListBox Owner { get; set; }
			public OjectCollectionItemComparer(ListBox listBox)
			{
				Owner = listBox;
			}

			public int Compare(object? x, object? y)
			{
				if (x is null)
				{
					if (y is null)
					{
						return 0; //both null, then they are equal
					}

					return -1; //item1 is null, but item2 is valid (greater)
				}

				if (y is null)
				{
					return 1; //item2 is null, so item 1 is greater
				}
				return string.Compare(Owner.GetItemText(x), Owner.GetItemText(y));
			}
		}
	}

	internal class Entry
	{
		public object Item { get; set; }

		public Entry(object data)
		{
			Item = data;
		}

		public override string ToString()
		{
			return Item.ToString() ?? string.Empty;
		}
	}

	class EntryEnumerator : IEnumerator<object>
	{
		readonly List<Entry> entries;
		int currentIndex = -1;

		public EntryEnumerator(List<Entry> entries)
		{
			this.entries = entries;
		}

		public object Current
		{
			get
			{
				if (currentIndex == -1 || currentIndex == entries.Count)
				{
					throw new IndexOutOfRangeException();
				}

				return entries[currentIndex].Item;
			}
		}

		public void Dispose()
		{
			entries.Clear();
		}

		public bool MoveNext()
		{
			while (true)
			{
				if (currentIndex < entries.Count - 1)
				{
					currentIndex++;
					return true;
				}
				else
				{
					currentIndex = entries.Count;
					return false;
				}
			}
		}

		public void Reset()
		{
			currentIndex = -1;
		}
	}
}

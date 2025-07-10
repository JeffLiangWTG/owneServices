using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace CargoWise.Winzor.AppServer
{
	sealed class ConcurrentSet<T> : ICollection<T>
		where T : notnull
	{
		readonly ConcurrentDictionary<T, byte> internalCollection = new ();

		public int Count => internalCollection.Count;

		public bool IsReadOnly => false;

		public void Add(T item)
		{
			internalCollection.TryAdd(key: item, 0);
		}

		public void Clear()
		{
			internalCollection.Clear();
		}

		public bool Contains(T item)
		{
			return internalCollection.ContainsKey(key: item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			internalCollection.Keys.CopyTo(array, arrayIndex);
		}

		public IEnumerator<T> GetEnumerator()
		{
			return internalCollection.Keys.GetEnumerator();
		}

		bool ICollection<T>.Remove(T item) => internalCollection.Remove(item, out byte _);

		public bool TryRemove(T item)
		{
			return internalCollection.TryRemove(item, out byte _);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return internalCollection.Keys.GetEnumerator();
		}
	}
}

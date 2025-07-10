using System.Collections;
using System.Collections.Immutable;

namespace WinzorFramework;

public class WrappedList<T> : IList<T>, IList
{
	protected virtual void OnAdd(T item, int index)
	{
	}

	protected virtual void OnRemove(T item)
	{
	}

	public virtual T this[int index] { get => inner[index]; set => inner = inner.SetItem(index, value); }

	public virtual int Count => inner.Count;

	public virtual bool IsReadOnly => false;

	bool IList.IsFixedSize => throw new NotImplementedException();

	bool IList.IsReadOnly => throw new NotImplementedException();

	int ICollection.Count => inner.Count;

	bool ICollection.IsSynchronized => throw new NotImplementedException();

	object ICollection.SyncRoot => throw new NotImplementedException();

	object? IList.this[int index] { get => this[index]; set => this[index] = (T)value!; }

	public virtual void Add(T item)
	{
		inner = inner.Add(item);
		OnAdd(item, inner.Count - 1);
	}

	public virtual void AddRange(IEnumerable<T> items)
	{
		foreach (var item in items)
		{
			Add(item);
		}
	}

	public virtual void Clear()
	{
		var removed = inner;
		inner = inner.Clear();
		foreach (var item in removed)
		{
			OnRemove(item);
		}
	}

		public virtual bool Contains(T item) => inner.Contains(item);

	public void Move(T item, int fromIndex, int toIndex)
	{
		int delta = toIndex - fromIndex;

		switch (delta)
		{
			case -1:
			case 1:
				// Simple swap
				this[fromIndex] = this[toIndex];
				break;

			default:
				int start;
				int dest;

				// Which direction are we moving?
				if (delta > 0)
				{
					// Shift down by the delta to open the new spot
					start = fromIndex + 1;
					dest = fromIndex;
				}
				else
				{
					// Shift up by the delta to open the new spot
					start = toIndex;
					dest = toIndex + 1;

					// Make it positive
					delta = -delta;
				}

				Copy(this, start, this, dest, delta);
				break;
		}

		this[toIndex] = item;
	}

	static void Copy(WrappedList<T> sourceList, int sourceIndex, WrappedList<T> destinationList, int destinationIndex, int length)
	{
		if (sourceIndex < destinationIndex)
		{
			// We need to copy from the back forward to prevent overwrite if source and
			// destination lists are the same, so we need to flip the source/dest indices
			// to point at the end of the spans to be copied.
			sourceIndex += length;
			destinationIndex += length;

			for (; length > 0; length--)
			{
				destinationList[--destinationIndex] = sourceList[--sourceIndex];
			}
		}
		else
		{
			for (; length > 0; length--)
			{
				destinationList[destinationIndex++] = sourceList[sourceIndex++];
			}
		}
	}

	public void CopyTo(T[] array, int arrayIndex) => inner.CopyTo(array, arrayIndex);

	public void CopyTo(Array array, int arrayIndex) => Array.Copy(inner.ToArray(), 0, array, arrayIndex, inner.Count);

		public virtual IEnumerator<T> GetEnumerator() => inner.GetEnumerator();

		public virtual int IndexOf(T item) => inner.IndexOf(item);

	public virtual void Insert(int index, T item)
	{
		inner = inner.Insert(index, item);
		OnAdd(item, index);
	}

	public virtual void Remove(T item)
	{
		if (item != null)
		{
			DoRemove(item);
		}
	}

	bool DoRemove(T item)
	{
		var updated = inner.Remove(item);
		var result = updated != inner;
		if (result)
		{
			inner = updated;
			OnRemove(item);
		}
		return result;
	}

	bool ICollection<T>.Remove(T item) => DoRemove(item);

	public virtual void RemoveAt(int index)
	{
		var item = inner[index];
		inner = inner.RemoveAt(index);
		OnRemove(item);
	}

	public IEnumerable<T> GetSnapshot() => inner;

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	int IList.Add(object? value)
	{
		Add((T)value!);
		return Count - 1;
	}

	void IList.Clear()
	{
		Clear();
	}

	bool IList.Contains(object? value)
	{
		return Contains((T)value!);
	}

	int IList.IndexOf(object? value)
	{
		return IndexOf((T)value!);
	}

	void IList.Insert(int index, object? value)
	{
		Insert(index, (T)value!);
	}

	void IList.Remove(object? value)
	{
		Remove((T)value!);
	}

	void IList.RemoveAt(int index)
	{
		RemoveAt(index);
	}

	void ICollection.CopyTo(Array array, int index)
	{
		CopyTo(array, index);
	}

	protected ImmutableList<T> inner = ImmutableList<T>.Empty;
}

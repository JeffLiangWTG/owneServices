using System;
using System.Collections;
using System.Collections.Generic;

namespace Enterprise.MasterFiles.Business
{
	[System.Diagnostics.DebuggerDisplay("Count = {Count}")]
	public sealed class NumberGeneratorValueProviderCollection : ICollection<INumberGeneratorValueProvider>, ICollection
	{
		public NumberGeneratorValueProviderCollection()
		{
			lookup = new Dictionary<string, INumberGeneratorValueProvider>();
		}

		public void AddRange(IEnumerable<INumberGeneratorValueProvider> collection)
		{
			if (collection == null)
			{
				throw new ArgumentNullException(nameof(collection));
			}

			foreach (INumberGeneratorValueProvider provider in collection)
			{
				Add(provider);
			}
		}

		public INumberGeneratorValueProvider this[string key]
		{
			get
			{
				if (key == null)
				{
					throw new ArgumentNullException(nameof(key));
				}

				INumberGeneratorValueProvider result;
				lookup.TryGetValue(key, out result);
				return result;
			}
		}

		#region ICollection<INumberGeneratorValueProvider> Members

		public void Add(INumberGeneratorValueProvider item)
		{
			if (item == null)
			{
				throw new ArgumentNullException(nameof(item));
			}

			lookup.Add(item.Key, item);
		}

		public void Clear()
		{
			lookup.Clear();
		}

		public bool Contains(INumberGeneratorValueProvider item)
		{
			if (item == null)
			{
				throw new ArgumentNullException(nameof(item));
			}

			return lookup.ContainsKey(item.Key);
		}

		public void CopyTo(INumberGeneratorValueProvider[] array, int arrayIndex)
		{
			lookup.Values.CopyTo(array, 0);
		}

		public int Count
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return lookup.Count; }
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		bool ICollection<INumberGeneratorValueProvider>.IsReadOnly
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return false; }
		}

		public bool Remove(INumberGeneratorValueProvider item)
		{
			if (item == null)
			{
				throw new ArgumentNullException(nameof(item));
			}

			return lookup.Remove(item.Key);
		}

		#endregion

		#region IEnumerable<INumberGeneratorValueProvider> Members

		public IEnumerator<INumberGeneratorValueProvider> GetEnumerator()
		{
			return lookup.Values.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		#region ICollection Members

		void ICollection.CopyTo(Array array, int index)
		{
			((ICollection)lookup.Values).CopyTo(array, index);
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		bool ICollection.IsSynchronized
		{
			get { return false; }
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		object ICollection.SyncRoot
		{
			get { return ((ICollection)lookup).SyncRoot; }
		}

		#endregion

		readonly Dictionary<string, INumberGeneratorValueProvider> lookup;
	}
}

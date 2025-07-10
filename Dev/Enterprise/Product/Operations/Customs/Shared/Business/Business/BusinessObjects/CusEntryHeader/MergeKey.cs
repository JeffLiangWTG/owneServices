using System;
using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class MergeKey
	{
		public MergeKey(int capacity)
		{
			keys = new List<IZType>(capacity);
		}

		public MergeKey()
			: this(3)
		{
		}

#if DEBUG

		public bool Contains(IZType obj)
		{
			return IndexOf(obj) > -1;
		}

		public int IndexOf(IZType obj)
		{
			int result = -1;
			for (int i = 0; i < keys.Count; i++)
			{
				if (keys[i].Equals(obj))
				{
					result = i;
					break;
				}
			}
			return result;
		}

		public List<IZType> Keys
		{
			get { return keys; }
		}

#endif

		public MergeKey Clone()
		{
			MergeKey result = new MergeKey(keys.Count);
			result.keys.AddRange(keys);
			return result;
		}

		public void Add(IZType obj)
		{
			hashCode = null;
			keys.Add(obj);
		}

		public void Remove(IZType obj)
		{
			hashCode = null;
			keys.Remove(obj);
		}

		public static MergeKey operator +(MergeKey a, MergeKey b)
		{
			if (ReferenceEquals(a, null))
			{
				throw new ArgumentNullException(nameof(a));
			}
			if (ReferenceEquals(b, null))
			{
				throw new ArgumentNullException(nameof(b));
			}

			MergeKey result = a.Clone();
			result.keys.AddRange(b.keys);
			return result;
		}

		public static bool operator ==(MergeKey a, object b)
		{
			if (ReferenceEquals(a, null))
			{
				return b == null;
			}
			else
			{
				return a.Equals(b);
			}
		}

		public static bool operator !=(MergeKey a, object b)
		{
			return !(a == b);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(obj, null))
			{
				return false;
			}
			MergeKey other = obj as MergeKey;
			if (other == null)
			{
				return false;
			}
			if (other.keys.Count != keys.Count)
			{
				return false;
			}
			if (GetHashCode() != other.GetHashCode())
			{
				return false;
			}
			for (int i = 0; i < keys.Count; i++)
			{
				if (!keys[i].Equals(other.keys[i]))
				{
					return false;
				}
			}

			return true;
		}

		int? hashCode;
		public override int GetHashCode()
		{
			if (hashCode == null)
			{
				hashCode = keys.Count.GetHashCode();
				foreach (object obj in keys)
				{
					if (obj == null)
					{
						hashCode = hashCode ^ 104729;   // prime number simply chosen because it is the 10,000th prime
					}
					else
					{
						hashCode = hashCode ^ obj.GetHashCode();
					}
				}
			}
			return hashCode.Value;
		}

		readonly List<IZType> keys;
	}
}

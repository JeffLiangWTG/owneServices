using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Enterprise.MasterFiles.Business
{
	public class UniqueListWithPriority<T> : IEnumerable<T> where T : class
	{
		public UniqueListWithPriority(IEqualityComparer<T> comparer)
		{
			inner = new Dictionary<T, List<int>>(comparer);
		}

		public void Add(int priority, params T[] objects)
		{
			AddRange(priority, objects);
		}

		public void AddRange(int priority, IEnumerable<T> objects)
		{
			if (priority > 0 && objects != null)
			{
				foreach (var obj in objects)
				{
					if (obj != null)
					{
						List<int> priorities;

						if (inner.TryGetValue(obj, out priorities))
						{
							if (!priorities.Contains(priority))
							{
								priorities.Add(priority);
							}
						}
						else
						{
							inner[obj] = new List<int> { priority };
						}
					}
				}
			}
		}

		public List<int> GetPriorities(T obj)
		{
			List<int> result;

			if (obj != null && inner.TryGetValue(obj, out result))
			{
				return result;
			}
			else
			{
				return new List<int>();
			}
		}

		public int Count
		{
			get { return inner.Count; }
		}

		public void Merge(UniqueListWithPriority<T> other)
		{
			if (other != null)
			{
				foreach (var pair in other.inner)
				{
					List<int> thisList;
					if (inner.TryGetValue(pair.Key, out thisList))
					{
						var thisListLookup = new HashSet<int>(thisList);

						foreach (var priority in pair.Value.Where(x => !thisListLookup.Contains(x)))
						{
							thisList.Add(priority);
						}
					}
					else
					{
						inner[pair.Key] = pair.Value;
					}
				}
			}
		}

		readonly Dictionary<T, List<int>> inner;

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return inner.Keys.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return inner.Keys.GetEnumerator();
		}
	}
}

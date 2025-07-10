using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Enterprise.MasterData.Business
{
	class ScoringResultGrouping<TKey, TElement> : IGrouping<TKey, TElement>
	{
		readonly List<TElement> elements;

		public TKey Key { get; }

		public ScoringResultGrouping(IGrouping<TKey, TElement> grouping)
		{
			if (grouping == null)
			{
				throw new ArgumentNullException(nameof(grouping));
			}

			Key = grouping.Key;
			elements = grouping.ToList();
		}

		public ScoringResultGrouping(TKey key, TElement element)
		{
			elements = new List<TElement>();

			Key = key;
			elements.Add(element);
		}

		public IEnumerator<TElement> GetEnumerator()
		{
			return elements.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}

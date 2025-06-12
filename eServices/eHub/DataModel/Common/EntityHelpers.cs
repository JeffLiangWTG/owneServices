using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CargoWise.eHub.DataModel.Common
{
	public class EntityHelpers
	{
		public static void ReorderEntities<T>(IEnumerable<T> entities, string orderingProperty, int defaultSpacing = 1000)
		{
			if (entities == null) throw new ArgumentNullException("entities");
			if (orderingProperty == null) throw new ArgumentNullException("orderingProperty");
			if (defaultSpacing <= 0) throw new ArgumentOutOfRangeException("defaultSpacing", "Default spacing must be greater than zero.");

			var orderingPropertyInfo = typeof(T).GetProperty(orderingProperty);
			if (orderingPropertyInfo == null) throw new ArgumentException("Ordering property not found on entity type.");
			if (orderingPropertyInfo.PropertyType != typeof(int?)) throw new ArgumentException("Ordering property must be of type 'Nullable<int>'", "orderingProperty");

			var entityList = entities.ToList();
			var orderingValues = new OrderingValues<T>(entities, orderingPropertyInfo, defaultSpacing);
			ReorderEntities(orderingValues, 0, entityList.Count, 0, null);
		}

		class OrderingValues<T>
		{
			List<T> entities;
			PropertyInfo orderingProperty;

			internal int DefaultSpacing { get; private set; }

			internal OrderingValues(IEnumerable<T> entities, PropertyInfo orderingProperty, int defaultSpacing)
			{
				this.entities = entities.ToList();
				this.orderingProperty = orderingProperty;
				this.DefaultSpacing = defaultSpacing;
			}

			internal int? this[int i]
			{
				get { return (int?)orderingProperty.GetValue(entities[i], null); }
				set { orderingProperty.SetValue(entities[i], value, null); }
			}
		}

		static void ReorderEntities<T>(OrderingValues<T> orderingValues, int rangeStartPos, int rangeEndPos, int rangeBaseOrdering, int? rangeMaxOrdering)
		{
			/*
			 * Find longest range of correctly ordered records (i.e., contiguous ascending ordering values)
			 */
			int longestCorrectLen = 0, longestCorrectStart = 0;
			for (int currPos = rangeStartPos; currPos < rangeEndPos; currPos++)
			{
				if (!rangeMaxOrdering.HasValue || orderingValues[currPos] < rangeMaxOrdering.Value)
					if (orderingValues[currPos] - rangeBaseOrdering > currPos - rangeStartPos)
					{
						int start = currPos;
						int length = 1;
						for (; currPos < rangeEndPos - 1; currPos++, length++)
							if (!(orderingValues[currPos + 1] > orderingValues[currPos])) break;
						if (length > longestCorrectLen)
						{
							longestCorrectLen = length;
							longestCorrectStart = start;
						}
					}
			}

			/*
			 * If none of the records have valid ordering values then evenly distribute them all.
			 */
			if (longestCorrectLen == 0)
			{
				int spacing = rangeMaxOrdering.HasValue ? (rangeMaxOrdering.Value - rangeBaseOrdering) / (rangeEndPos - rangeStartPos + 1) : orderingValues.DefaultSpacing;
				if (!rangeMaxOrdering.HasValue)
					rangeBaseOrdering = (rangeBaseOrdering / orderingValues.DefaultSpacing) * orderingValues.DefaultSpacing;
				for (int i = rangeStartPos, newPosition = rangeBaseOrdering + spacing; i < rangeEndPos; i++, newPosition += spacing)
					orderingValues[i] = newPosition;
			}
			else
			{
				/*
				 * If there are records immediately preceding the longest correct group then distribute them in the available ordering range.
				 */
				if (longestCorrectStart > rangeStartPos)
				{
					/*
					 * Search backwards from longest correct range to find the first record with a ordering value that is low enough 
					 * to allow contained records to be reordered correctly.
					 */
					int reposLen = 1;
					int reposStart = longestCorrectStart - 1;
					for (; reposStart >= rangeStartPos; reposStart--, reposLen++)
					{
						if (orderingValues[reposStart] - rangeBaseOrdering > longestCorrectStart - rangeStartPos - reposLen)
							if (orderingValues[longestCorrectStart] - orderingValues[reposStart] >= reposLen) break;
					}

					int basePosition = reposStart < rangeStartPos ? rangeBaseOrdering : (orderingValues[reposStart]).Value;
					int spacing = (orderingValues[longestCorrectStart].Value - basePosition) / reposLen;
					int newPosition = basePosition + spacing;

					for (int i = reposStart + 1; i < longestCorrectStart; i++, newPosition += spacing)
						orderingValues[i] = newPosition;

					/*
					 * If we have not repositioned back to the start of the range then reorder the preceding records.
					 */
					if (reposStart > rangeStartPos)
						ReorderEntities(orderingValues, rangeStartPos, reposStart, rangeBaseOrdering, orderingValues[reposStart]);
				}

				/*
				 * If the longest correct sequence ends before the end of the list then reorder the records after it.
				 */
				int followingStartPos = longestCorrectStart + longestCorrectLen;
				if (followingStartPos < rangeEndPos)
				{
					ReorderEntities(orderingValues, followingStartPos, rangeEndPos, orderingValues[followingStartPos - 1].Value, rangeMaxOrdering);
				}
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Rating.Rateable;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// Base class for a comparer where a part list may not have the attribute being matched, but can still match if other part lists have the attribute.
	/// </summary>
	/// <typeparam name="T">type of the part attribute</typeparam>
	abstract class MultiPartDimensionComparer<T> : DimensionComparer<T>
	{
		public override Similarity GetSimilarity(FastLine line,
			IDistinctPartRateDimensions measures,
			IHasPartDimensions partList,
			IRateablePart part,
			IPointMatchingLog pointMatchingLog = null)
		{
			Similarity similarity;

			if (partList != null && HasDimension(partList))
			{
				similarity = base.GetSimilarity(line, measures, partList, part, pointMatchingLog);
			}
			else if (LineHasValue(line))
			{
				// Line has a value, but the part list doesn't. Check if any other part list does.
				var allValues = GetAllPartValues(measures);
				if (allValues.Count == 1)
				{
					// If there is only one value on the entire job, then treat that as the value of the part
					// and do the normal similarity check.
					var singleValue = allValues.First();
					similarity = GetValueSimilarity(line, singleValue);
				}
				else if (allValues.Count > 1)
				{
					similarity = GetSimilarityWhenLineHasValueAndJobHasMultipleValues(line, allValues);
				}
				else
				{
					similarity = GetSimilarityWhenLineHasValueAndJobHasNoValue(line);
				}

				AddToLogWhenNotSimilar(similarity, line, partList, part, pointMatchingLog, includePartValue: false);
			}
			else
			{
				similarity = GetSimilarityWhenJobHasNoValueAndLineHasNoValue(line);
			}

			return similarity;
		}

		public override bool AreRatelinesCompatibleForSimultaneousUsage(RateableMeasureSet measures, FastLine line1, FastLine line2)
		{
			var allValues = GetAllPartValues(measures);
			if (allValues.Count == 1)
			{
				// If the job has only one value then the lines must have already matched it and so are compatible
				return true;
			}
			else if (allValues.Count > 1 && LineHasValue(line1))
			{
				// If line1 has a value then all job values that are similar to line1 must also be similar to line2 for the lines to be compatible.
				// E.g. if the line is for 20GP and the job is for 20GP and 40GP containers, then line1 is similar to 20GP (but not 40GP)
				// so line2 must also be similar to 20GP. The similarity of line2 to 40GP doesn't matter.
				foreach (var val in allValues)
				{
					if (GetValueSimilarity(line1, val) != Similarity.None &&
						GetValueSimilarity(line2, val) == Similarity.None)
					{
						return false;
					}
				}
			}

			return true;
		}

		protected abstract IReadOnlyCollection<T> GetAllPartValues(IDistinctPartRateDimensions measures);

		protected abstract bool LineHasValue(FastLine line);

		protected abstract Similarity GetSimilarityWhenJobHasNoValueAndLineHasNoValue(FastLine line);

		protected abstract Similarity GetSimilarityWhenLineHasValueAndJobHasNoValue(FastLine line);

		/// <summary>
		/// Called when the part list being matched doesn't have the dimension, but other part lists do, and have multiple distinct values.
		/// The default logic is if any value is similar to the line then to consider the part as similar, but at the lowest rank.
		/// </summary>
		protected virtual Similarity GetSimilarityWhenLineHasValueAndJobHasMultipleValues(FastLine line, IReadOnlyCollection<T> otherValues)
			=> otherValues.Any(x => GetValueSimilarity(line, x) >= Similarity.Lowest)
				? Similarity.Lowest
				: Similarity.None;
	}

	/// <summary>
	/// Guid dimensions are foreign keys. E.g., to the Warehouse, RefContainer, or OrgProduct table.
	/// If the job and line are the same PK (or both null) => Exact
	/// If just the line PK is null => Generic
	/// For container types there is additional matching such as on class => Generic
	/// </summary>
	abstract class GuidDimensionComparer : MultiPartDimensionComparer<Guid?>
	{
		protected override Similarity GetSimilarityWhenJobHasNoValueAndLineHasNoValue(FastLine line)
			=> Similarity.Exact;

		protected override Similarity GetSimilarityWhenLineHasValueAndJobHasNoValue(FastLine line)
			=> Similarity.None;
	}

	abstract class StringDimensionComparer : MultiPartDimensionComparer<string>
	{
	}

	abstract class BooleanDimensionComparer : MultiPartDimensionComparer<bool>
	{
	}
}

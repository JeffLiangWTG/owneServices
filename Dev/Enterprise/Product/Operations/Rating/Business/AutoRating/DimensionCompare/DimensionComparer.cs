using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// Abstract base class implementing common logic for IDimensionComparer.
	/// </summary>
	/// <typeparam name="T">type of the dimension</typeparam>
	internal abstract class DimensionComparer<T> : IDimensionComparer
	{
		public virtual Similarity GetSimilarity(FastLine line,
			IDistinctPartRateDimensions measures,
			IHasPartDimensions partList,
			IRateablePart part,
			IPointMatchingLog pointMatchingLog = null)
		{
			Argument.NotNull(partList, nameof(partList));

			Similarity similarity;

			var partValue = GetPartValue(part);
			similarity = GetValueSimilarity(line, partValue);
			AddToLogWhenNotSimilar(similarity, line, partList, part, pointMatchingLog, includePartValue: true);
			return similarity;
		}

		public abstract bool AreRatelinesCompatibleForSimultaneousUsage(RateableMeasureSet measures, FastLine line1, FastLine line2);

		protected internal abstract Similarity GetValueSimilarity(FastLine line, T partValue);
		public abstract bool HasDimension(IHasPartDimensions partList);

		protected void AddToLogWhenNotSimilar(Similarity similarity, FastLine line, IHasPartDimensions partList, IRateablePart part, IPointMatchingLog pointMatchingLog, bool includePartValue)
		{
			// Log when not similar, so user can diagnose why a rate did not apply
			if (similarity == Similarity.None && pointMatchingLog != null)
			{
				var partName = pointMatchingLog.GetPartName(partList, part);
				pointMatchingLog.LogPartDidNotMatchLine(
					line.DisplayInfo(),
					partName,
					DimensionReadableNameForLogging,
					includePartValue ? GetPartValueForLogging(pointMatchingLog.Factory, part, pointMatchingLog.CurrentMeasureType) : string.Empty,
					GetLineValueForLogging(line, pointMatchingLog.CurrentMeasureType));
			}
		}

		protected abstract T GetPartValue(IRateablePart part);
		protected abstract string GetPartValueForLogging(BusinessObjectFactory factory, IRateablePart part, MeasureType currentMeasureType);
		protected abstract string GetLineValueForLogging(FastLine line, MeasureType currentMeasureType);

		protected abstract string DimensionReadableNameForLogging { get; }
	}
}

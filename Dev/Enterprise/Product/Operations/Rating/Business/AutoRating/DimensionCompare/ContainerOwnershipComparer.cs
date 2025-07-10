using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// Compare job to rate line for the container ownership dimension.
	/// </summary>
	internal sealed class ContainerOwnershipComparer : StringDimensionComparer
	{
		protected internal override Similarity GetValueSimilarity(FastLine line, string partValue)
		{
			if (!LineHasValue(line))
			{
				return Similarity.Lowest;
			}
			else
			{
				return GetSimilarityWhenLineHasValue(line, partValue);
			}
		}

		Similarity GetSimilarityWhenLineHasValue(FastLine line, string partValue)
		{
			if (line.Line.TL_WeightVolume != RatingConstants.Units.CN)
			{
				return Similarity.Exact;
			}

			return line.Line.TL_ContainerOwnership == partValue
				? Similarity.Exact
				: Similarity.None;
		}

		protected override Similarity GetSimilarityWhenJobHasNoValueAndLineHasNoValue(FastLine line)
			=> Similarity.Lowest;

		protected override Similarity GetSimilarityWhenLineHasValueAndJobHasNoValue(FastLine line)
			=> GetSimilarityWhenLineHasValue(line, string.Empty);

		protected override string DimensionReadableNameForLogging
			=> "ContainerOwnership"; // dimension name is not translated in the log yet

		protected override string GetLineValueForLogging(FastLine line, MeasureType currentMeasureType)
			=> line.Line.TL_ContainerOwnership;

		protected override IReadOnlyCollection<string> GetAllPartValues(IDistinctPartRateDimensions measures)
			=> measures.GetDistinctContainerOwnerships();

		protected override string GetPartValue(IRateablePart part)
			=> part.ContainerOwnership;

		protected override string GetPartValueForLogging(BusinessObjectFactory factory, IRateablePart part, MeasureType currentMeasureType)
			=> part.ContainerOwnership;

		public override bool HasDimension(IHasPartDimensions partList)
			=> partList.HasContainerOwnership;

		protected override bool LineHasValue(FastLine line)
			=> !line.Line.TL_ContainerOwnership.IsEmpty;
	}
}

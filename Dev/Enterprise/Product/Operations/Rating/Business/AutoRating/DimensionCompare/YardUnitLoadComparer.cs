using CargoWise.EntityFramework;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// Compares the Load (Empty or Laden) of a Yard Unit with the config (TI_YardUnitLoad) from a rate entry as part of matching rates to the job.
	/// </summary>
	class YardUnitLoadComparer : DimensionComparer<string>
	{
		protected override string DimensionReadableNameForLogging => "YardUnitLoad";

		public override bool AreRatelinesCompatibleForSimultaneousUsage(RateableMeasureSet measures, FastLine line1, FastLine line2) => true;

		public override Similarity GetSimilarity(FastLine line, IDistinctPartRateDimensions measures, IHasPartDimensions partList, IRateablePart part, IPointMatchingLog pointMatchingLog = null)
		{
			if (partList == null || part == null || !partList.HasYardUnitLoad)
			{
				return Similarity.Exact;
			}
			else
			{
				return base.GetSimilarity(line, measures, partList, part, pointMatchingLog);
			}
		}

		protected internal override Similarity GetValueSimilarity(FastLine line, string partValue)
		{
			return SimilarityHelper.GetStringSimilarity(line.ParentRateEntry.TI_YardUnitLoad, partValue);
		}

		protected override string GetPartValue(IRateablePart part)
		{
			return part is IRateableContainer container ? container.YardUnitLoad : string.Empty;
		}

		public override bool HasDimension(IHasPartDimensions partList)
			=> partList.HasYardUnitLoad;

		protected override string GetLineValueForLogging(FastLine line, MeasureType currentMeasureType)
			=> line.ParentRateEntry.TI_YardUnitLoad;

		protected override string GetPartValueForLogging(BusinessObjectFactory factory, IRateablePart part, MeasureType currentMeasureType)
			=> GetPartValue(part);
	}
}

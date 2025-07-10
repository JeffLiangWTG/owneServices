using CargoWise.EntityFramework;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// Compares the Type (e.g. CNT/CHS/GEN/BLK) of a Yard Unit with the config (TI_YardUnitType) from a rate entry as part of matching rates to the job.
	/// </summary>
	class YardUnitTypeComparer : DimensionComparer<string>
	{
		protected override string DimensionReadableNameForLogging => "YardUnitType";

		public override bool AreRatelinesCompatibleForSimultaneousUsage(RateableMeasureSet measures, FastLine line1, FastLine line2) => true;

		public override Similarity GetSimilarity(FastLine line, IDistinctPartRateDimensions measures, IHasPartDimensions partList, IRateablePart part, IPointMatchingLog pointMatchingLog = null)
		{
			if (partList == null || part == null || !partList.HasYardUnitType)
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
			return SimilarityHelper.GetStringSimilarity(line.ParentRateEntry.TI_YardUnitType, partValue);
		}

		protected override string GetPartValue(IRateablePart part)
		{
			return part is IRateableContainer container ? container.YardUnitType : string.Empty;
		}

		public override bool HasDimension(IHasPartDimensions partList)
			=> partList.HasYardUnitType;

		protected override string GetLineValueForLogging(FastLine line, MeasureType currentMeasureType)
			=> line.ParentRateEntry.TI_YardUnitType;

		protected override string GetPartValueForLogging(BusinessObjectFactory factory, IRateablePart part, MeasureType currentMeasureType)
			=> GetPartValue(part);
	}
}

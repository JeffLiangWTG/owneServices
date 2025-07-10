using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// Compares the Client of a Yard Unit with the Drop off/Pick up Client (Controlling Customer) from a rate entry as part of matching rates to the job.
	/// If the job has no Yard Unit Client, the rates should match exactly.
	/// else if the rate entry has the same Yard Unit Client, the rates should match exactly.
	/// else if the rate entry has empty Client, the rates should has the lowest similarity.
	/// else the rates should not match.
	/// </summary>
	internal sealed class YardUnitClientComparer : DimensionComparer<Guid>
	{
		protected override string DimensionReadableNameForLogging => "YardUnitClient";

		public override bool AreRatelinesCompatibleForSimultaneousUsage(RateableMeasureSet measures, FastLine line1, FastLine line2) => true;

		public override Similarity GetSimilarity(FastLine line, IDistinctPartRateDimensions measures, IHasPartDimensions partList, IRateablePart part, IPointMatchingLog pointMatchingLog = null)
		{
			if (partList == null || part == null || !partList.HasYardUnitClient)
			{
				return Similarity.Exact;
			}
			else
			{
				return base.GetSimilarity(line, measures, partList, part, pointMatchingLog);
			}
		}

		protected internal override Similarity GetValueSimilarity(FastLine line, Guid partValue)
		{
			return SimilarityHelper.GetGuidSimilarity(GetLineValue(line), partValue);
		}

		protected override Guid GetPartValue(IRateablePart part)
		{
			return part is IRateableContainer container ? container.YardUnitClient : Guid.Empty;
		}

		public override bool HasDimension(IHasPartDimensions partList)
			=> partList.HasYardUnitClient;

		protected override string GetLineValueForLogging(FastLine line, MeasureType currentMeasureType)
			=> GetLineValue(line).ToString();

		protected override string GetPartValueForLogging(BusinessObjectFactory factory, IRateablePart part, MeasureType currentMeasureType)
			=> GetPartValue(part).ToString();

		ZGuid GetLineValue(FastLine line)
			=> line.ParentRateEntry.TI_OH_ControllingCustomer;
	}
}

using CargoWise.EntityFramework;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// Compares charge group on a job to charge group on a rate line as part of picking matching rates.
	/// A rate line always has a charge code which always has a charge group, i.e., it can't be blank/null.
	/// If the job does not have charge group defined it will match any rate line.
	/// It's only if the job has a specific charge group defined that it requires a rate with the same charge code group.
	/// </summary>
	internal sealed class ChargeGroupToUseComparer : DimensionComparer<string>
	{
		public override Similarity GetSimilarity(FastLine line, IDistinctPartRateDimensions measures, IHasPartDimensions partList, IRateablePart part, IPointMatchingLog pointMatchingLog = null)
		{
			if (partList == null || part == null)
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
			if (string.IsNullOrEmpty(partValue))
			{
				return Similarity.Exact;
			}
			else if (line.ChargeCode.AC_ChargeGroup == partValue)
			{
				// Returning Generic for historical reasons, but Exact should work too and would be consistent with other dimensions.
				return Similarity.Generic;
			}
			else
			{
				return Similarity.None;
			}
		}

		/// <summary>
		/// This is only called for lines with the same charge code.
		/// So they automatically have the same charge group and are compatible.
		/// </summary>
		public override bool AreRatelinesCompatibleForSimultaneousUsage(RateableMeasureSet measures, FastLine line1, FastLine line2)
			=> true;

		protected override string GetLineValueForLogging(FastLine line, MeasureType currentMeasureType)
			=> line.ChargeCode.AC_ChargeGroup;

		protected override string GetPartValue(IRateablePart part)
			=> part.ChargeGroupToUse;

		protected override string GetPartValueForLogging(BusinessObjectFactory factory, IRateablePart part, MeasureType currentMeasureType)
			=> part.ChargeGroupToUse;

		public override bool HasDimension(IHasPartDimensions partList)
			=> partList.HasChargeGroupToUse;

		protected override string DimensionReadableNameForLogging
			=> "ChargeGroupToUse"; // dimension name is not translated in the log yet
	}
}

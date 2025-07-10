using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// Compares IsPalletized dimension on a job part with IsOnPallets field on a rate line.
	/// Note a rate line only considers IsOnPallets if the unit is per container.
	///
	/// Matching rules (in order):
	///	- Line is not per CN => Similarity.Exact
	///	- Line is per CN and the part has value IsPalletized defined:
	///		- part == line => Similarity.Exact
	///		- part <> line => Similarity.None
	/// - Line is per CN and the part doesn't have a palletized field then check all the other parts for one:
	///		- If no palletized field anywhere on job then treat the job as not palletized:
	///			- Line is IsOnPallets => Similarity.None
	///			- Line is not IsOnPallets => Similarity.Exact
	///		- If exactly one palletized value elsewhere on job => treat it as the part value and use the "part has value" rules.
	///		- If multiple palletized values on job (i.e., some palletized and some not) then treat the job as not palletized using the "part has value" rules.
	/// </summary>
	internal sealed class PalletizedComparer : MultiPartDimensionComparer<bool>
	{
		protected internal override Similarity GetValueSimilarity(FastLine line, bool partValue)
		{
			if (!LineHasValue(line))
			{
				return Similarity.Exact;
			}
			else
			{
				return GetSimilarityWhenLineHasValue(line, partValue);
			}
		}

		Similarity GetSimilarityWhenLineHasValue(FastLine line, bool partValue)
			=> line.Line.TL_IsOnPallets == partValue ? Similarity.Exact : Similarity.None;

		public override bool AreRatelinesCompatibleForSimultaneousUsage(RateableMeasureSet measures, FastLine line1, FastLine line2)
			=> true;

		protected override Similarity GetSimilarityWhenJobHasNoValueAndLineHasNoValue(FastLine line)
			=> Similarity.Exact;

		protected override Similarity GetSimilarityWhenLineHasValueAndJobHasNoValue(FastLine line)
			=> GetSimilarityWhenLineHasValue(line, DefaultValue);

		// A part list that doesn't have the palletized attribute where other parts have both possible values
		// does not match any rate line that has the attribute.
		protected override Similarity GetSimilarityWhenLineHasValueAndJobHasMultipleValues(FastLine line, IReadOnlyCollection<bool> otherValues)
			=> Similarity.None;

		// Job is considered by default to be not palletized.
		const bool DefaultValue = false;

		protected override string DimensionReadableNameForLogging
			=> (NoResString)"Palletized"; // dimension name is not translated in the log yet

		bool? GetLineValue(FastLine line)
			=> LineHasValue(line)
				? line.Line.TL_IsOnPallets
				: null;

		protected override string GetLineValueForLogging(FastLine line, MeasureType currentMeasureType)
			=> GetValueForLogging(GetLineValue(line));

		protected override IReadOnlyCollection<bool> GetAllPartValues(IDistinctPartRateDimensions measures)
			=> measures.GetDistinctPalletized();

		protected override bool GetPartValue(IRateablePart part)
			=> part.IsOnPallets;

		protected override string GetPartValueForLogging(BusinessObjectFactory factory, IRateablePart part, MeasureType currentMeasureType)
			=> GetValueForLogging(GetPartValue(part));

		static string GetValueForLogging(bool? val)
			=> val.HasValue ? val.Value ? "Y" : "N" : string.Empty;

		public override bool HasDimension(IHasPartDimensions partList)
			=> partList.HasPalletized;

		protected override bool LineHasValue(FastLine line)
			=> line.Line.TL_WeightVolume == RatingConstants.Units.CN;
	}
}

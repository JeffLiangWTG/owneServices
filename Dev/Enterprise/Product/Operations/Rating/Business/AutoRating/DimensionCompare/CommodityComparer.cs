using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// Compares commodity on a job to commodity on a rate entry as part of picking matching rates.
	/// </summary>
	sealed class CommodityComparer : StringDimensionComparer
	{
		protected internal override Similarity GetValueSimilarity(FastLine line, string partValue)
		{
			var entry = line.ParentRateEntry;
			var lineCommodity = entry.TI_RH_NKCommodityCode;
			var lineIsTACT = entry.TI_IsTact;

			return GetSimilarity(lineCommodity, lineIsTACT, partValue);
		}

		internal static Similarity GetSimilarity(string lineCommodity, bool lineIsTACT, string commodityCode)
		{
			if (string.IsNullOrEmpty(lineCommodity) || lineIsTACT)
			{
				return Similarity.Lowest;
			}
			else
			{
				return GetSimilarityWhenLineHasValue(lineCommodity, commodityCode);
			}
		}

		static Similarity GetSimilarityWhenLineHasValue(string lineCommodity, string commodityCode)
		{
			if (lineCommodity == "GEN" && string.IsNullOrEmpty(commodityCode))
			{
				return Similarity.Generic;
			}

			if (string.Equals(lineCommodity, commodityCode, StringComparison.OrdinalIgnoreCase))
			{
				return Similarity.Exact;
			}

			return Similarity.None;
		}

		protected override Similarity GetSimilarityWhenJobHasNoValueAndLineHasNoValue(FastLine line)
			=> Similarity.Lowest;

		protected override Similarity GetSimilarityWhenLineHasValueAndJobHasNoValue(FastLine line)
			=> GetSimilarityWhenLineHasValue(line.ParentRateEntry.TI_RH_NKCommodityCode, string.Empty);

		protected override Similarity GetSimilarityWhenLineHasValueAndJobHasMultipleValues(FastLine line, IReadOnlyCollection<string> otherValues)
		{
			var lineCommodity = (string)line.ParentRateEntry.TI_RH_NKCommodityCode;
			if (lineCommodity == "GEN" && NeedSpecialCaseSimilarityForGEN(line))
			{
				return GetSpecialCaseSimilarityForGEN(otherValues);
			}
			else
			{
				return GetStandardSimilarityWhenLineHasValueAndJobHasMultipleValues(lineCommodity, otherValues);
			}
		}

		Similarity GetStandardSimilarityWhenLineHasValueAndJobHasMultipleValues(string lineCommodity, IReadOnlyCollection<string> otherValues)
		{
			var similarity = GetSimilarityWhenLineHasValue(lineCommodity, string.Empty);
			if (similarity == Similarity.None)
			{
				if (otherValues.Any(x => GetSimilarityWhenLineHasValue(lineCommodity, x) >= Similarity.Lowest))
				{
					similarity = Similarity.Lowest;
				}
			}
			return similarity;
		}

		bool NeedSpecialCaseSimilarityForGEN(FastLine line)
			=> line.Line.RateCalculatorType == CalculatorType.Percentage;

		Similarity GetSpecialCaseSimilarityForGEN(IReadOnlyCollection<string> otherValues)
		{
			// See WI00466534
			// Don't give GEN a higher similarity than any other commodity if the GEN commodity is on the job.
			if (otherValues.Any(x => string.Equals("GEN", x, StringComparison.OrdinalIgnoreCase)))
			{
				return Similarity.Lowest;
			}
			else
			{
				// GEN isn't on the job and the job's overall commodity value is considered to be blank (since there are multiple values).
				// So GEN is treated as a higher match than other commodity codes which return Similarity.Lowest.
				return Similarity.Generic;
			}
		}

		protected override bool LineHasValue(FastLine line)
		{
			var entry = line.ParentRateEntry;
			return !entry.TI_RH_NKCommodityCode.IsEmpty && !entry.TI_IsTact;
		}

		protected override string GetLineValueForLogging(FastLine line, MeasureType currentMeasureType)
			=> line.ParentRateEntry.TI_RH_NKCommodityCode;

		protected override IReadOnlyCollection<string> GetAllPartValues(IDistinctPartRateDimensions measures)
			=> measures.GetDistinctCommodities();

		protected override string GetPartValue(IRateablePart part)
			=> part.CommodityCode;

		protected override string GetPartValueForLogging(BusinessObjectFactory factory, IRateablePart part, MeasureType currentMeasureType)
			=> GetPartValue(part);

		public override bool HasDimension(IHasPartDimensions partList)
			=> partList.HasCommodity;

		protected override string DimensionReadableNameForLogging
			=> (NoResString)"Commodity"; // dimension name is not translated in the log yet
	}
}

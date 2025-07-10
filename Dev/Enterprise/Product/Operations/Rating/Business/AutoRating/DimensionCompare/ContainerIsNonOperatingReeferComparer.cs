using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// Compares job container IsNonOperatingReefer fields against the IsNonOperatedReefer field on rates
	/// </summary>
	sealed class ContainerIsNonOperatingReeferComparer : BooleanDimensionComparer
	{
		protected internal override Similarity GetValueSimilarity(FastLine line, bool partValue)
		{
			var lineNonOperatingReefer = line.ParentRateEntry.TI_IsNonOperatedReefer;
			if (lineNonOperatingReefer.IsEmpty)
			{
				return Similarity.Generic;
			}

			return
				lineNonOperatingReefer == "Y" && partValue ? Similarity.Exact :
				lineNonOperatingReefer == "N" && !partValue ? Similarity.Exact :
				Similarity.None;
		}

		protected override Similarity GetSimilarityWhenJobHasNoValueAndLineHasNoValue(FastLine line)
			=> Similarity.Lowest;

		protected override Similarity GetSimilarityWhenLineHasValueAndJobHasNoValue(FastLine line)
			=> Similarity.Lowest;

		protected override Similarity GetSimilarityWhenLineHasValueAndJobHasMultipleValues(FastLine line, IReadOnlyCollection<bool> otherValues)
		{
			var lineNonOperatingReefer = line.ParentRateEntry.TI_IsNonOperatedReefer;

			if (lineNonOperatingReefer.IsEmpty)
			{
				return Similarity.Generic;
			}
			else if (lineNonOperatingReefer == "Y")
			{
				return
					otherValues.All(x => x) ? Similarity.Exact :
					otherValues.Any(x => x) ? Similarity.Lowest : Similarity.None;
			}
			else if (lineNonOperatingReefer == "N")
			{
				return 
					otherValues.All(x => !x) ? Similarity.Exact :
					otherValues.Any(x => !x) ? Similarity.Lowest : Similarity.None;
			}

			return Similarity.None;
		}

		protected override bool LineHasValue(FastLine line)
		{
			// Any line has at least an IsNonOperatedReefer field of blank.
			return true;
		}

		protected override string GetLineValueForLogging(FastLine line, MeasureType currentMeasureType)
			=> line.ParentRateEntry.TI_IsNonOperatedReefer.IsEmpty
			? Res.GetString("72ea9608-5d7e-433a-9ae8-d5165ef733fd", "blank")
			: line.ParentRateEntry.TI_IsNonOperatedReefer;

		protected override IReadOnlyCollection<bool> GetAllPartValues(IDistinctPartRateDimensions measures)
			=> measures.GetDistinctContainerIsNonOperatingReefers();

		protected override bool GetPartValue(IRateablePart part)
			=> part.ContainerIsNonOperatingReefer;

		protected override string GetPartValueForLogging(BusinessObjectFactory factory, IRateablePart part, MeasureType currentMeasureType)
			=> GetPartValue(part).ToString();

		public override bool HasDimension(IHasPartDimensions partList)
			=> partList.HasContainerType; // If the job has a container, then it has a reefer information

		protected override string DimensionReadableNameForLogging
			=> (NoResString)"ContainerIsNonOperatingReefer"; // dimension name is not translated in the log yet
	}
}

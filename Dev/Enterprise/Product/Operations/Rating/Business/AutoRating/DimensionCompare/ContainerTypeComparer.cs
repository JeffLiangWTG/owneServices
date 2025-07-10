using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// Compares container type on a job with container type from a rate entry as part of matching rates to the job.
	/// </summary>
	internal sealed class ContainerTypeComparer : GuidDimensionComparer
	{
		protected internal override Similarity GetValueSimilarity(FastLine line, Guid? partValue)
		{
			var entry = line.ParentRateEntry;
			var lineValue = NullableHelper.ToNullable(entry.TI_RC);
			var similarity = SimilarityHelper.GetGuidSimilarity(lineValue, partValue);
			if (similarity == Similarity.None && partValue.HasValue && lineValue.HasValue)
			{
				if (entry.TI_MatchContainerRateClass &&
					entry.IsSameContainerOrSameClass(partValue.Value, true))
				{
					similarity = Similarity.Generic;
				}
			}

			return similarity;
		}

		protected override bool LineHasValue(FastLine line)
			=> !line.ParentRateEntry.TI_RC.IsEmpty;

		protected override IReadOnlyCollection<Guid?> GetAllPartValues(IDistinctPartRateDimensions measures)
			=> measures.GetDistinctContainerTypePKs();

		protected override Guid? GetPartValue(IRateablePart part)
			=> part.ContainerTypePk;

		public override bool HasDimension(IHasPartDimensions partList)
			=> partList.HasContainerType;

		protected override string GetLineValueForLogging(FastLine line, MeasureType currentMeasureType)
			=> line.ParentRateEntry.Container?.RC_Code ?? string.Empty;

		protected override string GetPartValueForLogging(BusinessObjectFactory factory, IRateablePart part, MeasureType currentMeasureType)
			=> part.ContainerTypePk.HasValue
				? (string)factory.Load<RefContainer>(part.ContainerTypePk.Value)?.RC_Code ?? string.Empty
				: string.Empty;

		protected override string DimensionReadableNameForLogging
			=> "ContainerType"; // dimension name is not translated in the log yet
	}
}

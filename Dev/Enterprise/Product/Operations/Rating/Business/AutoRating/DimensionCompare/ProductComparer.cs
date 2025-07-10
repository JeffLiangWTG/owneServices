using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// Compares product on a job with product from a rate line (TL_OP_ProductNumber) as part of matching rates to the job.
	/// </summary>
	internal sealed class ProductComparer : GuidDimensionComparer
	{
		protected internal override Similarity GetValueSimilarity(FastLine line, Guid? partValue)
			=> SimilarityHelper.GetGuidSimilarity(line.Line.TL_OP_ProductNumber, partValue);

		protected override bool LineHasValue(FastLine line)
			=> !line.Line.TL_OP_ProductNumber.IsEmpty;

		protected override IReadOnlyCollection<Guid?> GetAllPartValues(IDistinctPartRateDimensions measures)
			=> measures.GetDistinctProductPKs();

		protected override Guid? GetPartValue(IRateablePart part)
			=> part.ProductPk;

		public override bool HasDimension(IHasPartDimensions partList)
			=> partList.HasProduct;

		protected override string DimensionReadableNameForLogging
			=> (NoResString)"Product"; // dimension name is not translated in the log yet

		protected override string GetLineValueForLogging(FastLine line, MeasureType currentMeasureType)
			=> string.Empty; // The original code didn't convert this from a PK, so leaving it empty

		protected override string GetPartValueForLogging(BusinessObjectFactory factory, IRateablePart part, MeasureType currentMeasureType)
			=> string.Empty; // The original code didn't convert this from a PK, so leaving it empty
	}
}

using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// Compares warehouse PK on a job part with warehouse PK from a rate entry to match rates to the job.
	/// 
	/// As of 2021-08, warehouse jobs are currently only rated for a single warehouse.
	/// Rates entries for the wrong warehouse should have already been filtered out at the job level filter.
	/// This class should only have to calculate similarity between rates for all warehouses and rates for the job warehouse.
	/// </summary>
	internal sealed class WarehouseComparer : GuidDimensionComparer
	{
		protected internal override Similarity GetValueSimilarity(FastLine line, Guid? partValue)
			=> SimilarityHelper.GetGuidSimilarity(NullableHelper.ToNullable(line.ParentRateEntry.TI_WW_Warehouse), partValue);

		protected override IReadOnlyCollection<Guid?> GetAllPartValues(IDistinctPartRateDimensions measures)
			=> measures.GetDistinctWarehousePKs();

		protected override Guid? GetPartValue(IRateablePart part)
			=> part.WarehousePk;

		public override bool HasDimension(IHasPartDimensions partList)
			=> partList.HasWarehouse;

		protected override string DimensionReadableNameForLogging
			=> (NoResString)"Warehouse"; // dimension name is not translated in the log yet

		// Expected not to be called since rates should always match.
		// If it is needed in the future it should return the warehouse code, not the PK.
		protected override string GetLineValueForLogging(FastLine line, MeasureType currentMeasureType)
			=> string.Empty;

		// Expected not to be called since rates should always match.
		// If it is needed in the future it should return the warehouse code, not the PK.
		protected override string GetPartValueForLogging(BusinessObjectFactory factory, IRateablePart part, MeasureType currentMeasureType)
			=> string.Empty;

		protected override bool LineHasValue(FastLine line)
			=> !line.ParentRateEntry.TI_WW_Warehouse.IsEmpty;
	}
}

using System.Collections.Generic;

namespace Enterprise.Rating.Rateable
{
	/// <summary>
	/// Interface for a list of one or more parts on a job (e.g, containers or packlines)
	/// </summary>
	public interface IRateablePartList : IHasPartDimensions, IReadOnlyList<IRateablePart>, IDistinctPartRateDimensions
	{
		bool HasPendingLazyPopulate { get; }

		/// <summary>
		/// WeightUnit. Null if weight is not applicable.
		/// May be empty even if the part is linked to MeasureType.Weight. Rating code seems to cope. It assumes KG?
		/// Would be good to fix the relevant code so empty is not allowed when HasWeight.
		/// Note, there is no need for a HasWeight property as yet. Linking the PartList
		/// to MeasureType.Weight is sufficient to indicate it has weight.
		/// </summary>
		string WeightUnit { get; }

		string VolumeUnit { get; }
		string AreaUnit { get; }
		string LengthUnit { get; }

		string ChargeableUnit { get; }
		string PickupDistanceUnit { get; }
		string DeliveryDistanceUnit { get; }

		// Only CartageRatingAdapter and OrderRatingAdapter sets this list level package unit. Other adapters use the part level PackageType.
		// This is the unit for MeasureType.Package (i.e., the unit for the number in IRateablePart.PackageCount).
		// It should be possible to remove this and use IRateablePart.PackageType in all adapters. To be decided...
		string PackageUnit { get; }

		/// <summary>
		/// Make a new list with all properties cloned, but empty of any parts.
		/// Used for combining multiple part lists into one.
		/// </summary>
		IRateablePartList NewEmptyClone();
	}
}

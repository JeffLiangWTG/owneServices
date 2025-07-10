using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Rating.Rateable;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// Interface for a filter on rateable parts.
	/// Used to group parts on particular dimensions.
	/// For example, some warehouse parts are grouped by docket and rated together, others are grouped by container type and number.
	/// An implementation of this interface will compare the relevant attributes on two parts.
	/// Inherits IHasPartDimensions to support querying which dimensions are being compared.
	/// </summary>
	public interface IPartFilter : IEqualityComparer<PartWithDimensions>, IHasPartDimensions
	{
		/// <summary>
		/// Satisfies is a weaker check than Equality.
		/// Only filter dimensions that are also part dimensions are compared to the key values.
		/// If it does not have any of the filter dimensions then it will automatically satisy the filter.
		/// E.g, if filtering on docket, this allows all parts that don't have a docket to pass the filter.
		/// Only parts that have a docket where the value differs from the key are filtered out.
		/// </summary>
		bool Satisfies(PartWithDimensions keyPart, PartWithDimensions partToCheck);

		/// <summary>
		/// Return true if the given IHasPartDimensions has any dimensions that match the filter.
		/// E.g., if the filter is on Docket and Product then returns true for either HasDocketReference or HasProduct.
		/// </summary>
		bool HasAnyDimensions(IHasPartDimensions dimensions);

		// Methods to get the filter values from the given part for any of the possible dimensions.
		// Returns empty/null if the filter does not include a dimension, even if the part has a value.
		// For example, GetDocketReference will return empty for a filter on location given a part with a docket, since only location is applicable.
		LocationMeasure GetLocation(PartWithDimensions part);
		string GetDocketReference(PartWithDimensions part);
		ProductAttributesMeasure GetProductAttributes(PartWithDimensions part);
		Guid? GetProductPk(PartWithDimensions part);
		Guid? GetCartageLegPk(PartWithDimensions part);
		ZString? GetContainerNumber(PartWithDimensions part);
		Guid? GetContainerTypePk(PartWithDimensions part);
	}
}

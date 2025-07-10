using System;
using Enterprise.Accounting.Integration;

namespace Enterprise.Rating.Rateable
{
	/// <summary>
	/// Dimensions (attributes) of part of a job that are used for reference/grouping purposes, and not for matching rates.
	/// </summary>
	public interface IPartReferenceDimensions
	{
		/// <summary>
		/// Equivalent to old MeasureDimension.ProductAttributes
		/// </summary>
		ProductAttributesMeasure ProductAttributes { get; }

		/// <summary>
		/// Equivalent to old MeasureDimension.Location
		/// Null if not applicable.
		/// </summary>
		LocationMeasure Location { get; }

		/// <summary>
		/// Equivalent to old MeasureDimension.DocketReference
		/// Null if not applicable.
		/// </summary>
		string DocketReference { get; }

		/// <summary>
		/// Equivalent to old MeasureDimension.CartageLegPK
		/// Null if not applicable.
		/// </summary>
		Guid? CartageLegPK { get; }

		/// <summary>
		/// Equivalent to old MeasureDimension.PalletID
		/// Null if not applicable.
		/// </summary>
		string PalletID { get; }

		/// <summary>
		/// PK of the container record itself. NOT the PK of the RefContainer.
		/// Used to match individual containers with services or spot rates for that container.
		/// And also used in identifying Containers while calculating per container chargeable in Combined calculator
		/// </summary>
		Guid? ContainerPK { get; }

		/// <summary>
		/// Equivalent to old MeasureDimension.ContainerNumber
		/// Null if not applicable.
		/// </summary>
		string ContainerNumber { get; }

		/// <summary>
		///	Used in Combined Calculator to override break value if only one break specified
		///	Default value 0; 0 == no override
		/// </summary>
		decimal PivotBreak { get; }
	}

	/// <summary>
	/// Interface for querying what part reference dimensions are present.
	/// See IPartReferenceDimensions for the equivalent property values.
	/// Not putting this in a separate file since it goes so closely with IPartReferenceDimensions
	/// </summary>
	public interface IHasPartReferenceDimensions
	{
		bool HasProductAttributes { get; }
		bool HasLocation { get; }
		bool HasDocketReference { get; }
		bool HasCartageLegPK { get; }
		bool HasPalletID { get; }
		bool HasContainerNumber { get; }
		bool HasWarehouseLine { get; }
	}
}

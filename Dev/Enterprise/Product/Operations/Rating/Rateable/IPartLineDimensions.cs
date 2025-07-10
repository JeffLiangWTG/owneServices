using System;

namespace Enterprise.Rating.Rateable
{
	/// <summary>
	/// Attributes of a job part that can be matched to a rate line
	/// </summary>
	public interface IPartLineDimensions
	{
		/// <summary>
		/// Product PK. When defined, expected to be an OrgSupplierPart.PK
		/// Equivalent to the legacy MeasureDimension.Product
		/// Matches RateLine.TL_OP_ProductNumber
		/// </summary>
		Guid? ProductPk { get; }

		/// <summary>
		/// Equivalent to the legacy MeasureDimension.Palletized
		/// Matches RateLine.TL_IsOnPallets (only used when TL_WeightVolume == RatingConstants.Units.CN )
		/// </summary>
		bool IsOnPallets { get; }

		/// <summary>
		/// Equivalent to the legacy MeasureDimension.ContainerOwnership
		/// Matches RateLine.TL_ContainerOwnership (only used when TL_WeightVolume == RatingConstants.Units.CN )
		/// </summary>
		string ContainerOwnership { get; }

		/// <summary>
		/// Charge code group to match.
		/// Expected to be a value from AccChargeCode.AC_ChargeGroup.
		/// Equivalent to the legacy MeasureDimension.ChargeGroupToUse.
		/// Matches RateLine.ChargeCode.AC_ChargeGroup
		/// As at 2021-07, only used in WhsAdjustmentRatingAdapter with WHSInwards or WHSOutwards (see ChargeCodeGroupList.Codes)
		/// </summary>
		string ChargeGroupToUse { get; }

		/// <summary>
		/// Equivalent to old MeasureDimension.PackageType
		/// Null if not applicable.
		/// Matches RateLine.TL_WeightVolume
		/// </summary>
		string PackageType { get; }
	}

	/// <summary>
	/// Interface for querying what part line dimensions are present.
	/// See IPartLineDimensions for the equivalent property values.
	/// Not putting this in a separate file since it goes so closely with IPartLineDimensions
	/// </summary>
	public interface IHasPartLineDimensions
	{
		/// <summary>
		/// <see cref="IPartLineDimensions.ProductPk"/>
		/// </summary>
		bool HasProduct { get; }

		/// <summary>
		/// <see cref="IPartLineDimensions.IsOnPallets"/>
		/// </summary>
		bool HasPalletized { get; }

		/// <summary>
		/// <see cref="IPartLineDimensions.ContainerOwnership"/>
		/// </summary>
		bool HasContainerOwnership { get; }

		/// <summary>
		/// <see cref="IPartLineDimensions.ChargeGroupToUse"/>
		/// </summary>
		bool HasChargeGroupToUse { get; }

		/// <summary>
		/// <see cref="IPartLineDimensions.PackageType"/>
		/// </summary>
		bool HasPackageType { get; }
	}
}

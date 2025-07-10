using System;
using System.Collections.Generic;

namespace Enterprise.Rating.Rateable
{
	/// <summary>
	/// All the dimensions of a job that match fields on a rate (RateEntry or RateLine)
	/// </summary>
	public interface IPartRateDimensions : IPartEntryDimensions, IPartLineDimensions
	{
	}

	/// <summary>
	/// Interface representing the set of distinct values for each dimension.
	/// If a job has no values in a dimension that set will be empty.
	/// For example, if a job has 10 containers, but only two container types
	/// this interface allows for easy querying of those two container types.
	///
	/// Note regarding Guid.Empty:
	/// The underlying parts are expected to use (Guid?)null instead of Guid.Empty.
	/// So Guid results are expected to not contain Guid.Empty.
	/// This need not be enforced by implementations of this interface though.
	/// It should be enforced by the parts.
	///
	/// Note, may not include dimensions where the distinct values are not currently needed.
	/// </summary>
	public interface IDistinctPartRateDimensions
	{
		/// <summary>
		/// Returns distinct commodity codes. Will not contain a null string. May contain an empty string.
		/// </summary>
		/// <returns>not null</returns>
		IReadOnlyCollection<string> GetDistinctCommodities();

		IReadOnlyCollection<string> GetDistinctRefUnitSection();

		IReadOnlyCollection<Guid?> GetDistinctJobRefContainerComponents();

		IReadOnlyCollection<Guid?> GetDistinctRefContainerMaterials();

		IReadOnlyCollection<Guid?> GetDistinctRefContainerRepairs();

		IReadOnlyCollection<RefContainerInfoParts> GetDistinctRefContainerInfo();

		/// <summary>
		/// Returns distinct container type PKs on the parts. Should not contain Guid.Empty (see class summary note).
		/// May contain nulls, since container type is not mandatory and may not be defined for some containers.
		/// </summary>
		/// <returns>not null</returns>
		IReadOnlyCollection<Guid?> GetDistinctContainerTypePKs();

		/// <summary>
		/// Returns distinct container qualities on the parts. Will not contain a null string. May contain an empty string.
		/// </summary>
		/// <returns>not null</returns>
		IReadOnlyCollection<string> GetDistinctContainerQualities();

		/// <summary>
		/// Returns the distinct values of the NonOperatedReefer flags from the containers
		/// If there are ANY containers, then this will contain at least one value. 
		/// </summary>
		/// <returns></returns>
		IReadOnlyCollection<bool> GetDistinctContainerIsNonOperatingReefers();

		/// <summary>
		/// Distinct package types. Will not contain a null string. May contain an empty string.
		/// </summary>
		/// <returns>not null</returns>
		IReadOnlyCollection<string> GetDistinctPackageTypes();

		/// <summary>
		/// Distinct warehouse PKs on the parts. Should not contain Guid.Empty (see class summary note).
		/// </summary>
		/// <returns>not null</returns>
		IReadOnlyCollection<Guid?> GetDistinctWarehousePKs();

		/// <summary>
		/// Distinct product PKs. Should not contain Guid.Empty (see class summary note).
		/// </summary>
		/// <returns>not null</returns>
		IReadOnlyCollection<Guid?> GetDistinctProductPKs();

		/// <summary>
		/// Distinct container ownership attributes. Will not contain a null string. May contain an empty string.
		/// </summary>
		/// <returns>not null</returns>
		IReadOnlyCollection<string> GetDistinctContainerOwnerships();

		/// <summary>
		/// Distinct palletized values.
		/// </summary>
		/// <returns>not null</returns>
		IReadOnlyCollection<bool> GetDistinctPalletized();
	}

	/// <summary>
	/// Interface for querying the presence of any part dimension that can match a rate, either via a RateEntry or a RateLine.
	/// </summary>
	public interface IHasPartRateDimensions : IHasPartEntryDimensions, IHasPartLineDimensions
	{
	}

	/// <summary>
	/// All the dimensions of part of a job.
	/// Note, contains as properties all the values of the legacy MeasureDimension enum,
	/// except ContainerCalculatedWeight and ContainerCalculatedVolume, since those are only used in quick calculate and there may be a better way (to be decided).
	/// </summary>
	public interface IPartDimensions : IPartReferenceDimensions, IPartRateDimensions
	{
	}

	/// <summary>
	/// Interface for querying what part dimensions are present, combining those that match rates and those that are for reference only.
	/// </summary>
	public interface IHasPartDimensions : IHasPartReferenceDimensions, IHasPartRateDimensions
	{
	}
}

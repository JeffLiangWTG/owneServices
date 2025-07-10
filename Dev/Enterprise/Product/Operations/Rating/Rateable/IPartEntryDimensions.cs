using System;

namespace Enterprise.Rating.Rateable
{
	/// <summary>
	/// Dimensions (attributes) of a job part that can be matched to a rate entry.
	/// 
	/// A rate entry has many more attributes than this interface, but all the other attributes match once at the job level.
	/// This interface is for those attributes that can match part of a job.
	/// For example, a rate entry with a container type matches only those job parts (containers) that have the same container type (or class if matching on class).
	/// A job with two containers of different types can have two matches, one entry matches the first type, and another entry matches the second.
	/// </summary>
	public interface IPartEntryDimensions
	{
		/// <summary>
		/// Container type PK of the part.
		/// Null if not applicable, e.g., a warehouse location does not have a container type.
		/// When it is applicable but optional the value may be null.
		/// When not null, must be a RefContainer.PK (more specifically a valid value of RateEntry.TI_RC)
		/// 
		/// Matches RateEntry.TI_RC combined with RateEntry.TI_MatchContainerRateClass
		///
		/// Equivalent to the legacy MeasureDimension.ContainerType
		/// </summary>
		Guid? ContainerTypePk { get; }

		/// <summary>
		/// Commodity code.
		/// Expected to always be a RefCommodityCode.RH_Code if not null/empty.
		/// Should be null/empty if HasCommodity is false.
		/// May be null/empty if HasCommodity is true, but the value is optional and not defined for a particular part.
		/// 
		/// Matches RateEntry.TI_RH_NKCommodityCode
		/// 
		/// Equivalent to the legacy MeasureDimension.Commodity
		/// </summary>
		string CommodityCode { get; }

		/// <summary>
		/// MNR Work Order Line
		/// When auto-rating MNRWorkOrderHeader we need a mapping between rate charges and work order line
		/// </summary>
		Guid? WorkOrderLinePK { get; }

		/// <summary>
		/// Ref Unit Section
		/// This is to hold UnitSection value
		/// </summary>
		string RefUnitSection { get; }

		/// <summary>
		/// Ref Container Component
		/// This is to hold Ref Component Code value
		/// </summary>
		Guid? RefContainerComponent { get; }

		/// <summary>
		/// Ref Repair Code
		/// This is to hold Ref Repair Code value
		/// </summary>
		Guid? RefRepairCode { get; }

		/// <summary>
		/// Ref Material Code
		/// This is to hold Ref Material Code value
		/// </summary>
		Guid? RefMaterialCode { get; }

		/// <summary>
		/// Is Non Operated Reefer
		/// Whenever there is a container, there is a value for IsNonOperatedReefer
		/// If there is no ContainerTypePk then the content of this field is
		/// irrelevant and should not be used.
		///
		/// Matches RateEntry.TI_IsNonOperatedReefer
		/// </summary>
		bool ContainerIsNonOperatingReefer { get; }

		/// <summary>
		/// Warehouse PK.
		/// If defined must be a valid value for RateEntry.TI_WW_Warehouse, which is expected to be WhsWarehouse.PK.
		/// Null if not applicable or is applicable but the field is optional and has no value for this part.
		/// Must never be Guid.Empty.
		/// 
		/// Matches RateEntry.TI_WW_Warehouse
		/// 
		/// Equivalent to the legacy MeasureDimension.Warehouse
		/// </summary>
		Guid? WarehousePk { get; }
	}

	/// <summary>
	/// Interface for querying what part entry dimensions are present.
	/// See IPartEntryDimensions for the equivalent property values.
	/// Not putting this in a separate file since it goes so closely with IPartEntryDimensions
	/// </summary>
	public interface IHasPartEntryDimensions
	{
		/// <summary>
		/// <see cref="IPartEntryDimensions.ContainerTypePk"/>
		/// </summary>
		bool HasContainerType { get; }

		/// <summary>
		/// <see cref="IPartEntryDimensions.CommodityCode"/>
		/// </summary>
		bool HasCommodity { get; }

		/// <summary>
		/// <see cref="IPartEntryDimensions.WarehousePk"/>
		/// </summary>
		bool HasWarehouse { get; }

		/// <summary>
		/// <see cref="IPartEntryDimensions.YardUnitType"/>
		/// </summary>
		bool HasYardUnitType { get; }

		/// <summary>
		/// <see cref="IPartEntryDimensions.YardUnitLoad"/>
		/// </summary>
		bool HasYardUnitLoad { get; }

		/// <summary>
		/// <see cref="IPartEntryDimensions.YardUnitClient"/>
		/// </summary>
		bool HasYardUnitClient { get; }
	}
}

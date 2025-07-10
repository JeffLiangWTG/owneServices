using System;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;

namespace Enterprise.Rating.Rateable
{
	/// <summary>
	/// A part of a job that can be rated on its own (or with a group of similar parts).
	/// Not to be used for job level information.
	/// Consists of:
	/// - Attributes (dimensions) that must match with a rate (e.g., container type).
	/// - Measure values that contain quantities that can be charged (e.g., weight).
	/// - Reference values that are used to build a description of what was rated and can be used to group parts to be calculated as one.
	/// Examples of parts are: containers, order lines, and services.
	/// </summary>
	public class RateablePart : IRateablePart
	{
		#region Measures

		public IClientProviderValues WeightMeasure { get; set; }
		public decimal Weight { get => WeightMeasure?.Actual ?? 0m; set => WeightMeasure = new ClientProviderValues(value); }
		public bool IsWeightWithoutVolume { get; set; }

		public IClientProviderValues VolumeMeasure { get; set; }
		public decimal Volume { get => VolumeMeasure?.Actual ?? 0m; set => VolumeMeasure = new ClientProviderValues(value); }

		public IClientProviderValues AreaMeasure { get; set; }
		public decimal Area { get => AreaMeasure?.Actual ?? 0m; set => AreaMeasure = new ClientProviderValues(value); }

		public IClientProviderValues LengthMeasure { get; set; }
		public decimal Length { get => LengthMeasure?.Actual ?? 0m; set => LengthMeasure = new ClientProviderValues(value); }

		public IClientProviderValues ChargeableMeasure { get; set; }

		public IClientProviderValues LoadingMeterMeasure { get; set; }
		public decimal LoadingMeter { get => LoadingMeterMeasure?.Actual ?? 0m; set => LoadingMeterMeasure = new ClientProviderValues(value); }

		/// <summary>
		/// <see cref="IPartMeasures.PackageCount"/>
		/// </summary>
		public decimal? PackageCount { get; set; }
		public string PackageCountReference { get; set; }

		/// <summary>
		/// <see cref="IPartMeasures.UnitCount"/>
		/// </summary>
		public decimal? UnitCount { get; set; }

		/// <summary>
		/// <see cref="IPartMeasures.ShipmentCount"/>
		/// </summary>
		public decimal? ShipmentCount => null;

		/// <summary>
		/// <see cref="IPartMeasures.LowestBillCount"/>
		/// </summary>
		public decimal? LowestBillCount => null;

		/// <summary>
		/// <see cref="IPartMeasures.ChargeablePalletCount"/>
		/// </summary>
		public decimal? ChargeablePalletCount => null;

		/// <summary>
		/// <see cref="IPartMeasures.LocationPalletCount"/>
		/// </summary>
		public decimal? LocationPalletCount { get; set; }

		/// <summary>
		/// Job level measure. Not applicable here.
		/// </summary>
		public int? UnidentifiedCount => null;

		#endregion

		#region Dimensions (attributes)

		public ProductAttributesMeasure ProductAttributes { get; set; }

		public LocationMeasure Location { get; set; }

		public string DocketReference { get; set; }

		public string PackageType { get; set; }

		public bool IsPackageLoaded { get; set; }

		public Guid? CartageLegPK { get; set; }

		public string PalletID { get; set; }

		/// <summary>
		/// PK of the container record itself. NOT the PK of the RefContainer.
		/// Used to match individual containers with services or spot rates for that container.
		/// And also used in identifying Containers while calculating per container chargeable in Combined calculator
		/// </summary>
		public Guid? ContainerPK { get; set; }

		public string ContainerNumber { get; set; }

		/// <summary>
		/// RefContainer PK (i.e., Container Type PK).
		/// Null if not applicable or is applicable but the field is optional and has no value for this part.
		/// Should never be Guid.Empty.
		/// </summary>
		public Guid? ContainerTypePk { get; set; }

		public bool ContainerIsNonOperatingReefer { get; set; }

		public decimal PivotBreak { get; set; }

		public string CommodityCode { get; set; }

		public string RefUnitSection { get; set; }

		public Guid? RefContainerComponent { get; set; }

		public Guid? RefRepairCode { get; set; }

		public Guid? RefMaterialCode { get; set; }

		public Guid? WorkOrderLinePK { get; set; }

		/// <summary>
		/// <see cref="IPartEntryDimensions.WarehousePk"/>
		public Guid? WarehousePk { get; set; }

		public Guid? ProductPk { get; set; }

		public bool IsOnPallets { get; set; }

		public string ContainerOwnership { get; set; }

		public string ChargeGroupToUse { get; set; }

		public decimal? LineCount { get; set; }

		public decimal PickupDistance { get; set; }

		public decimal DeliveryDistance { get; set; }

		public TimeInfo Time { get; set; }

		public int GetDeclarationLineCount(MeasureType measureType)
		{
			throw new InvalidOperationException("job level measure is not valid for a part " + measureType);
		}

		#endregion
	}
}

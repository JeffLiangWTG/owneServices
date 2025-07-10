using System;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;

namespace Enterprise.Rating.Rateable
{
	/// <summary>
	/// Abstract implementation of IRateablePart with all properties returning null, meaning the value is not defined.
	/// Useful for implementing a specific part with minimal code and memory overhead.
	/// Derived classes only need to implement those properties that apply to that particular part.
	/// For example, to create a class just for Pallet IDs, which only uses the PalletID property,
	/// use this and override PalletID.
	/// </summary>
	internal abstract class BaseRateablePart : IRateablePart
	{
		public virtual IClientProviderValues WeightMeasure => null;
		public bool IsWeightWithoutVolume => false;

		public virtual IClientProviderValues VolumeMeasure => null;

		public virtual IClientProviderValues AreaMeasure => null;

		public virtual IClientProviderValues LengthMeasure => null;

		public virtual IClientProviderValues ChargeableMeasure => null;

		public virtual decimal? PackageCount => null;

		public bool IsPackageLoaded => false;

		public virtual decimal? UnitCount => null;

		public virtual decimal? ShipmentCount => null;

		public virtual decimal? LowestBillCount => null;

		public virtual decimal? ChargeablePalletCount => null;

		public virtual decimal? LocationPalletCount => null;

		public virtual int? UnidentifiedCount => null;

		public virtual ProductAttributesMeasure ProductAttributes => null;

		public virtual LocationMeasure Location => null;

		public virtual string DocketReference => null;

		public virtual string PackageType => null;

		public virtual Guid? WarehouseLine => null;

		public virtual Guid? CartageLegPK => null;

		public virtual string PalletID => null;

		public virtual Guid? ContainerPK => null;

		public virtual string ContainerNumber => null;

		public decimal PivotBreak => 0;

		public virtual Guid? ContainerTypePk => null;

		public virtual bool ContainerIsNonOperatingReefer => false;

		public virtual string CommodityCode => null;

		public virtual Guid? WarehousePk => null;

		public virtual Guid? ProductPk => null;

		public virtual bool IsOnPallets => false;

		public virtual string ContainerOwnership => null;

		public virtual string ChargeGroupToUse => null;

		public virtual decimal? LineCount => null;

		public string PackageCountReference => null;

		public IClientProviderValues LoadingMeterMeasure => null;

		public decimal PickupDistance => 0;

		public decimal DeliveryDistance => 0;

		public TimeInfo Time => null;

		public string RefUnitSection => null;

		public Guid? RefContainerComponent => Guid.Empty;

		public Guid? RefRepairCode => Guid.Empty;

		public Guid? RefMaterialCode => Guid.Empty;

		public Guid? WorkOrderLinePK => Guid.Empty;

		public int GetDeclarationLineCount(MeasureType measureType) => 0;
	}
}

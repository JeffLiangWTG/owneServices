using System;
using System.Collections.Generic;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;

namespace Enterprise.Rating.Rateable
{
	/// <summary>
	/// Job level measures and attributes.
	/// There may be more than one of these for the same job since some measures have different attributes.
	/// E.g., the commodity may be relevant to the weight measure, but not applicable to the ShipmentCount.
	/// There will be only one per MeasureType value though.
	///
	/// In the future, it may be possible to have only one of these, but the system would need to know what attributes to ignore for each measure.
	/// For example, somewhere there would need to be logic that ignores commodity when using ShipmentCount.
	/// Only one would be better since it would ensure there was only one value for each attribute, e.g., only one CommodityCode defined.
	/// </summary>
	public class JobLevelPart : IRateablePartList, IRateablePart
	{
		#region Quantities

		public IClientProviderValues WeightMeasure { get; internal set; }
		public decimal Weight { get => WeightMeasure?.Actual ?? 0m; set => WeightMeasure = new ClientProviderValues(value); }
		public string WeightUnit { get; set; }
		public bool IsWeightWithoutVolume { get; set; }

		public IClientProviderValues VolumeMeasure { get; internal set; }
		public decimal Volume { get => VolumeMeasure?.Actual ?? 0m; set => VolumeMeasure = new ClientProviderValues(value); }
		public string VolumeUnit { get; set; }

		public IClientProviderValues AreaMeasure { get; internal set; }
		public decimal Area { get => AreaMeasure?.Actual ?? 0m; set => AreaMeasure = new ClientProviderValues(value); }
		public string AreaUnit { get; set; }

		public IClientProviderValues LengthMeasure { get; internal set; }
		public decimal Length { get => LengthMeasure?.Actual ?? 0m; set => LengthMeasure = new ClientProviderValues(value); }
		public string LengthUnit { get; set; }
		public IClientProviderValues ChargeableMeasure { get; set; }
		public decimal Chargeable => ChargeableMeasure?.Actual ?? 0m;
		public string ChargeableUnit { get; set; }

		/// <summary>
		/// Value for MeasureType.Package
		/// However, it seems jobs that have multiple packages don't set the unit. They set MeasureDimension.PackageType (now IPartLineDimensions.PackageType).
		/// For now just using IPartLineDimensions.PackageType for unit.
		/// </summary>
		public decimal? PackageCount { get; internal set; }
		public bool HasPackageCount => PackageCount.HasValue;

		/// <summary>
		/// Value for MeasureType.Unit.
		/// Future: see if this can be rolled into MeasureType.Package since they seem to have the same number.
		/// </summary>
		public decimal? UnitCount { get; internal set; }

		/// <summary>
		/// Value for MeasureType.Shipment
		/// </summary>
		public decimal? ShipmentCount { get; set; }

		/// <summary>
		/// Value for MeasureType.LowestBill
		/// </summary>
		public decimal? LowestBillCount { get; set; }

		/// <summary>
		/// Value for MeasureType.ChargeablePallet
		/// </summary>
		public decimal? ChargeablePalletCount { get; internal set; }

		/// <summary>
		/// Value for MeasureType.Unidentified.
		/// Seems currently only used to indicate the job has services or might have services.
		/// The unit on a rate line of "SV - Service" maps to MeasureType.Unidentified and is the only unit that does.
		/// <see cref="Business.QuantityUnit.SV"/>
		/// The actual number doesn't have to be the number of services and seems not used.
		/// In legacy measures, this quantity also had a unit of "SV". To support this if needed, add a new property "UnidentifiedUnit".
		/// Would be good to refactor this into sensible code.
		/// </summary>
		public int? UnidentifiedCount { get; internal set; }
		public bool HasUnidentifiedCount => UnidentifiedCount.HasValue;

		public string PickupDistanceUnit { get; internal set; }
		public decimal PickupDistance { get; internal set; }

		public string DeliveryDistanceUnit { get; internal set; }
		public decimal DeliveryDistance { get; internal set; }

		public string PackageCountReference { get; internal set; }

		public IClientProviderValues LoadingMeterMeasure { get; internal set; }

		public decimal? LocationPalletCount { get; internal set; }

		public decimal? LineCount { get; internal set; }

		public TimeInfo Time { get; internal set; }

		#endregion

		#region Dimensions (Attributes)

		/// <summary>
		/// Commodity code. Blank if defined, but has no value. Null if not defined.
		/// </summary>
		public string CommodityCode { get; set; }
		public bool HasCommodity => CommodityCode != null;

		/// <summary>
		/// CartageLegPK. Null if not applicable, or has no value.
		/// Should never be Guid.Empty.
		/// </summary>
		public Guid? CartageLegPK { get; internal set; }
		public bool HasCartageLegPK => CartageLegPK.HasValue;

		public ProductAttributesMeasure ProductAttributes { get; internal set; }
		public bool HasProductAttributes { get; internal set; }

		public LocationMeasure Location { get; internal set; }
		public bool HasLocation { get; internal set; }

		public string DocketReference { get; internal set; }
		public bool HasDocketReference => DocketReference != null;

		public bool HasWarehouseLine => false;

		public string PackageType { get; internal set; }
		public bool HasPackageType { get; internal set; }

		public bool IsPackageLoaded => false;

		public string PackageUnit { get; internal set; }

		public string PalletID { get; internal set; }
		public bool HasPalletID { get; internal set; }

		/// <summary>
		/// PK of the container record itself. NOT the PK of the RefContainer.
		/// Used to match individual containers with services or spot rates for that container.
		/// And also used in identifying Containers while calculating per container chargeable in Combined calculator
		/// </summary>
		public Guid? ContainerPK { get; internal set; }

		public bool ContainerIsNonOperatingReefer { get; internal set; }

		public string ContainerNumber { get; internal set; }
		public bool HasContainerNumber { get; internal set; }

		public Guid? ContainerTypePk { get; set; }
		public bool HasContainerType { get; internal set; }

		public decimal PivotBreak { get; internal set; }

		public Guid? WarehousePk { get; internal set; }
		public bool HasWarehouse => WarehousePk.HasValue;

		public Guid? ProductPk { get; internal set; }
		public bool HasProduct { get; internal set; }

		public bool IsOnPallets { get; internal set; }
		public bool HasPalletized { get; internal set; }

		public string ContainerOwnership { get; internal set; }
		public bool HasContainerOwnership { get; internal set; }

		public string ChargeGroupToUse { get; internal set; }
		public bool HasChargeGroupToUse { get; internal set; }

		public string YardUnitType { get; internal set; }
		public bool HasYardUnitType { get; internal set; }

		public string YardUnitLoad { get; internal set; }
		public bool HasYardUnitLoad { get; internal set; }

		public Guid? YardUnitClient { get; internal set; }
		public bool HasYardUnitClient { get; internal set; }

		#endregion

		#region Parts

		public IRateablePart this[int index]
			=> index == 0 ? this : throw new InvalidOperationException("index must be zero for job level parts");

		public int Count => 1;

		public bool HasPendingLazyPopulate => false;

		public IEnumerator<IRateablePart> GetEnumerator()
		{
			yield return this;
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

		public string RefUnitSection { get; set; }

		public Guid? RefContainerComponent => Guid.Empty;

		public Guid? RefRepairCode => Guid.Empty;

		public Guid? RefMaterialCode => Guid.Empty;

		public Guid? WorkOrderLinePK => Guid.Empty;

		public IReadOnlyCollection<string> GetDistinctCommodities()
			=> HasCommodity ? new[] { CommodityCode } : Array.Empty<string>();

		public IReadOnlyCollection<Guid?> GetDistinctContainerTypePKs()
			=> HasContainerType ? new[] { ContainerTypePk } : Array.Empty<Guid?>();

		public IReadOnlyCollection<bool> GetDistinctContainerIsNonOperatingReefers()
			=> HasContainerType ? new[] { ContainerIsNonOperatingReefer } : Array.Empty<bool>();

		public IReadOnlyCollection<string> GetDistinctContainerQualities()
			=> Array.Empty<string>();

		public IReadOnlyCollection<string> GetDistinctPackageTypes()
			=> HasPackageType ? new[] { PackageType } : Array.Empty<string>();

		public IReadOnlyCollection<Guid?> GetDistinctWarehousePKs()
			=> HasWarehouse ? new[] { WarehousePk } : Array.Empty<Guid?>();

		public IReadOnlyCollection<Guid?> GetDistinctProductPKs()
			=> HasProduct ? new[] { ProductPk } : Array.Empty<Guid?>();

		public IReadOnlyCollection<string> GetDistinctContainerOwnerships()
			=> !string.IsNullOrEmpty(ContainerOwnership) ? new[] { ContainerOwnership } : Array.Empty<string>();

		public IReadOnlyCollection<bool> GetDistinctPalletized()
			=> HasPalletized ? new[] { IsOnPallets } : Array.Empty<bool>();

		public IRateablePartList NewEmptyClone()
		{
			// Clone will likely need to contain more than one part, so use RateablePartList instead of JobPart
			var partList = new RateablePartList();
			partList.PopulateEmptyClone(this);
			return partList;
		}

		#endregion

		internal void SetQuantity(MeasureType measureType, decimal amount, string unit)
		{
			switch (measureType)
			{
				case MeasureType.Weight:
				case MeasureType.JobWeight:
				case MeasureType.StorageWeight:
					{
						Weight = amount;
						WeightUnit = unit;
						break;
					}
				case MeasureType.Volume:
				case MeasureType.JobVolume:
				case MeasureType.StorageVolume:
					{
						Volume = amount;
						VolumeUnit = unit;
						break;
					}
				case MeasureType.Area:
					{
						Area = amount;
						AreaUnit = unit;
						break;
					}
				case MeasureType.Length:
					{
						Length = amount;
						LengthUnit = unit;
						break;
					}
				case MeasureType.Shipment:
					{
						ShipmentCount = amount;
						// unit not applicable
						break;
					}
				case MeasureType.LowestBill:
					{
						LowestBillCount = amount;
						// unit not applicable
						break;
					}
				case MeasureType.Package:
					{
						PackageCount = amount;
						PackageUnit = unit;
						break;
					}
				case MeasureType.Line:
					{
						throw new InvalidOperationException("use AddLineCountWithWarehouseDocket");
					}
				case MeasureType.Unit:
				case MeasureType.JobUnit:
				case MeasureType.StorageUnit:
					{
						UnitCount = amount;
						// unit not applicable (is effectively "PK - Any Package?)
						break;
					}
				case MeasureType.LoadingMeters:
					{
						LoadingMeterMeasure = new ClientProviderValues(amount);
						break;
					}
				case MeasureType.Chargeable:
					{
						ChargeableMeasure = new ClientProviderValues(amount);
						ChargeableUnit = unit;
						break;
					}
				case MeasureType.ContainerCount:
					{
						throw new InvalidOperationException("use CreateContainerList");
					}
				case MeasureType.ChargeablePallet:
					{
						ChargeablePalletCount = amount;
						// unit not applicable
						break;
					}
				case MeasureType.LocationPallet:
					{
						throw new InvalidOperationException("use AddLocationPallet instead");
					}
				case MeasureType.PalletID:
					{
						throw new InvalidOperationException("use AddWarehousePalletId instead");
					}
				case MeasureType.PickupDistance:
					{
						PickupDistance = amount;
						PickupDistanceUnit = unit;
						break;
					}
				case MeasureType.DeliveryDistance:
					{
						DeliveryDistance = amount;
						DeliveryDistanceUnit = unit;
						break;
					}
				case MeasureType.Unidentified:
					{
						UnidentifiedCount = (int)amount;
						// unit not applicable (is effectively SV - Service)
						break;
					}
				case MeasureType.FDALine:
				case MeasureType.PNFDALine:
				case MeasureType.OMCLine:
				case MeasureType.CPSCLine:
				case MeasureType.DEALine:
				case MeasureType.FCCLine:
				case MeasureType.DOTLine:
				case MeasureType.LaceyLine:
				case MeasureType.CFIALine:
				case MeasureType.SITTLine:
				case MeasureType.NRCANLine:
				case MeasureType.TCLine:
				case MeasureType.HCLine:
				case MeasureType.PHACLine:
				case MeasureType.ECCCLine:
				case MeasureType.DFOLine:
				case MeasureType.CNSCLine:
				case MeasureType.GACLine:
				case MeasureType.OtherPGALine:
				case MeasureType.NMFS370:
				case MeasureType.NMFSCOA:
				case MeasureType.NMFSAMR:
				case MeasureType.NMFSHMS:
				case MeasureType.NMFSSIM:
				case MeasureType.AMS:
				case MeasureType.APHIS:
				case MeasureType.ATF:
				case MeasureType.DDTC:
				case MeasureType.FSIS:
				case MeasureType.FWS:
				case MeasureType.PST:
				case MeasureType.HFC:
				case MeasureType.TTB:
				case MeasureType.VNE:
				case MeasureType.ODS:
				case MeasureType.TSCA:
				case MeasureType.TCC:
				case MeasureType.NOP:
				case MeasureType.FDADisclaim:
				case MeasureType.OMCDisclaim:
				case MeasureType.CPSCDisclaim:
				case MeasureType.DEADisclaim:
				case MeasureType.FCCDisclaim:
				case MeasureType.DOTDisclaim:
				case MeasureType.LaceyDisclaim:
				case MeasureType.NMFS370Disclaim:
				case MeasureType.NMFSAMRDisclaim:
				case MeasureType.NMFSHMSDisclaim:
				case MeasureType.AMSDisclaim:
				case MeasureType.AMSNOPDisclaim:
				case MeasureType.APHISDisclaim:
				case MeasureType.FSISDisclaim:
				case MeasureType.FWSDisclaim:
				case MeasureType.PSTDisclaim:
				case MeasureType.HFCDisclaim:
				case MeasureType.TTBDisclaim:
				case MeasureType.VNEDisclaim:
				case MeasureType.ODSDisclaim:
				case MeasureType.TSCADisclaim:
				case MeasureType.DeliveryOrders:
				case MeasureType.SteelLicenses:
				case MeasureType.SG_TPLCertificate:
				case MeasureType.CA_NAFTA_TPLCertificate:
				case MeasureType.MX_NAFTA_TPLCertificate:
				case MeasureType.BeefExportCertificate:
				case MeasureType.DiamondCertificate:
				case MeasureType.ATPDEACertificate:
				case MeasureType.AU_FTA_ExportCertificate:
				case MeasureType.MXCementLicense:
				case MeasureType.CAFTA_TPLCertificate:
				case MeasureType.ALBCertificate:
				case MeasureType.CottonShirtingFabricLicense:
				case MeasureType.HaitiEarnedAllowance:
				case MeasureType.AgriculturalLicense:
				case MeasureType.CAExportSugarCertificate:
				case MeasureType.WoolLicense:
				case MeasureType.CBTPACertificate:
				case MeasureType.AGOATextileProvisionNumber:
				case MeasureType.OtherNonStandardVisa:
				case MeasureType.USDASugarCertificate:
				case MeasureType.OrganicProductExemptionCertificate:
				case MeasureType.AMSCertificateOfExemption:
				case MeasureType.DominicanRepublicEarnedAllowanceProgramCertificate:
				case MeasureType.MexicanSugarExportLicense:
				case MeasureType.GeneralNote15cWaiverCertificate:
				case MeasureType.AluminumLicenses:
				case MeasureType.CanadianUSMCA_TPLCertificate:
				case MeasureType.MexicanUSMCA_TPLCertificate:
				case MeasureType.ArgentineWhiteGrapeJuiceConcentrateExportLicense:
				case MeasureType.KRExportSteelCertificate:
				case MeasureType.VISANumbers:
				case MeasureType.PGALines:
				case MeasureType.PGADisclaims:
				case MeasureType.HTS9902Line:
				case MeasureType.HTS9903Line:
					{
						SetDeclarationLineCount(measureType, amount);
						break;
					}
				case MeasureType.Time:
					{
						throw new InvalidOperationException("can't set time as a number, use Time property instead");
					}
				default:
					{
						throw new NotImplementedException("Unsupported " + measureType);
					}
			}
		}

		public int GetDeclarationLineCount(MeasureType measureType)
		{
			return declarationLineCounts != null && declarationLineCounts.TryGetValue(measureType, out var count)
				? count
				: 0;
		}

		void SetDeclarationLineCount(MeasureType measureType, decimal amount)
		{
			if (declarationLineCounts == null)
			{
				declarationLineCounts = new Dictionary<MeasureType, int>();
			}
			declarationLineCounts[measureType] = (int)amount;
		}

		public IReadOnlyCollection<string> GetDistinctRefUnitSection()
		{
			return new List<string>();
		}

		public IReadOnlyCollection<Guid?> GetDistinctJobRefContainerComponents()
		{
			return new List<Guid?>();
		}

		public IReadOnlyCollection<Guid?> GetDistinctRefContainerMaterials()
		{
			return new List<Guid?>();
		}

		public IReadOnlyCollection<Guid?> GetDistinctRefContainerRepairs()
		{
			return new List<Guid?>();
		}

		public IReadOnlyCollection<RefContainerInfoParts> GetDistinctRefContainerInfo()
		{
			return new List<RefContainerInfoParts>();
		}

		Dictionary<MeasureType, int> declarationLineCounts;
	}
}

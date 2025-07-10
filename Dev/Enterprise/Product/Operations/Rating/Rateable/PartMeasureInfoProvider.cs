using System;
using Enterprise.Rating.Integration;

namespace Enterprise.Rating.Rateable
{
	/// <summary>
	/// Interface to return a measure value for a RateablePart for a single MeasureType.
	/// A MeasureType corresponds to a particular property of RateablePart.
	/// For example, MeasureType.Weight means the Weight property.
	/// The concept is similar to using reflection to get a property value given the property name.
	/// So this is like PropertyInfo in System.Reflection.
	/// An instance of the class can be obtained once and then used over and over
	/// to efficiently get the required value from any number of parts.
	///
	/// All implementations are expected to be private, nested classes of PartMeasureInfoProvider.
	/// </summary>
	public interface IPartMeasureInfo
	{
		decimal GetActualValueWithUnit(IRateablePart part, string unit);

		decimal GetActualValue(IRateablePart part);
		decimal GetProviderValue(IRateablePart part);
		decimal GetClientValue(IRateablePart part);

		string GetUnit(IRateablePartList partList);
	}

	/// <summary>
	/// Provides an IPartMeasureInfo instance for a given MeasureType.
	/// </summary>
	public class PartMeasureInfoProvider
	{
		static public PartMeasureInfoProvider Instance
		{
			get
			{
				PartMeasureInfoProvider provider;
				if (instance == null || !instance.TryGetTarget(out provider))
				{
					provider = new PartMeasureInfoProvider();
					if (instance == null)
					{
						instance = new WeakReference<PartMeasureInfoProvider>(provider);
					}
					else
					{
						instance.SetTarget(provider);
					}
				}

				return provider;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static WeakReference<PartMeasureInfoProvider> instance;

		readonly IPartMeasureInfo[] allInfos = new IPartMeasureInfo[MeasureTypeHelper.MeasureTypeMaxValue + 1];

		public IPartMeasureInfo GetPartMeasureInfo(MeasureType measureType)
		{
			var result = allInfos[(int)measureType];
			if (result == null)
			{
				result = CreatePartMeasureInfo(measureType);
				allInfos[(int)measureType] = result;
			}
			return result;
		}

		IPartMeasureInfo CreatePartMeasureInfo(MeasureType measureType)
		{
			switch (measureType)
			{
				case MeasureType.Weight:
				case MeasureType.JobWeight:
				case MeasureType.StorageWeight:
				case MeasureType.InnerPacksWeight:
				case MeasureType.WarehousePackageWeight:
					return new PartWeight();

				case MeasureType.Volume:
				case MeasureType.JobVolume:
				case MeasureType.StorageVolume:
				case MeasureType.InnerPacksVolume:
				case MeasureType.WarehousePackageVolume:
					return new PartVolume();

				case MeasureType.Length:
					return new PartLength();

				case MeasureType.Area:
					return new PartArea();

				case MeasureType.Shipment:
					return new PartShipment();
				case MeasureType.LowestBill:
					return new PartLowestBill();
				case MeasureType.Package:
				case MeasureType.InnerPacksPackage:
					return new PartPackage();
				case MeasureType.Line:
					return new PartLine();

				case MeasureType.Unit:
				case MeasureType.JobUnit:
				case MeasureType.StorageUnit:
				case MeasureType.InnerPacksUnit:
				case MeasureType.BOMKit:
					return new PartUnit();

				case MeasureType.LoadingMeters:
					return new PartLoadingMeters();
				case MeasureType.Chargeable:
					return new PartChargeable();
				case MeasureType.ContainerCount:
					return new PartContainerCount();
				case MeasureType.ChargeablePallet:
					return new PartChargeablePallet();
				case MeasureType.LocationPallet:
					return new PartLocationPallet();
				case MeasureType.PalletID:
					return new PartPalletID();
				case MeasureType.PickupDistance:
					return new PartPickupDistance();
				case MeasureType.DeliveryDistance:
					return new PartDeliveryDistance();
				case MeasureType.Unidentified:
					return new PartUnidentified();
				case MeasureType.WarehousePackage:
					return new PartWarehousePackage();
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
					return new PartDeclarationLineCount(measureType);
				case MeasureType.Time:
					return new PartTime();
			}

			throw new NotImplementedException("Unsupported " + measureType);
		}

		// Using specific implementations for every property, at least for common properties, for speed.
		// The alternative would be a generic implementation based on custom attributes or reflection.

		/// <summary>
		/// Base class for measures that have a single value, not separate values for provider and for client.
		/// </summary>
		abstract class SingleValuePartMeasure : IPartMeasureInfo
		{
			public virtual decimal GetActualValueWithUnit(IRateablePart part, string unit) => 0m;

			public abstract decimal GetActualValue(IRateablePart part);
			public abstract string GetUnit(IRateablePartList partList);

			public decimal GetProviderValue(IRateablePart part) => GetActualValue(part);
			public decimal GetClientValue(IRateablePart part) => GetActualValue(part);
		}

		abstract class SingleValuePartMeasureWithoutUnits : SingleValuePartMeasure
		{
			public override string GetUnit(IRateablePartList partList) => string.Empty;
		}

		/// <summary>
		/// Base class for measures that have separate values for provider and for client using IMeasureValue
		/// </summary>
		abstract class MultiValuePartMeasure : IPartMeasureInfo
		{
			public virtual decimal GetActualValueWithUnit(IRateablePart part, string unit) => 0m;

			public abstract string GetUnit(IRateablePartList partList);
			public decimal GetActualValue(IRateablePart part) => GetMeasureValue(part)?.Actual ?? 0;
			public decimal GetProviderValue(IRateablePart part) => GetMeasureValue(part)?.ForProvider ?? 0;
			public decimal GetClientValue(IRateablePart part) => GetMeasureValue(part)?.ForClient ?? 0;

			protected abstract IClientProviderValues GetMeasureValue(IRateablePart part);
		}

		class PartWeight : MultiValuePartMeasure
		{
			public override string GetUnit(IRateablePartList partList) => partList.WeightUnit;
			protected override IClientProviderValues GetMeasureValue(IRateablePart part) => part.WeightMeasure;
		}

		class PartVolume : MultiValuePartMeasure
		{
			public override string GetUnit(IRateablePartList partList) => partList.VolumeUnit;
			protected override IClientProviderValues GetMeasureValue(IRateablePart part) => part.VolumeMeasure;
		}

		class PartArea : MultiValuePartMeasure
		{
			public override string GetUnit(IRateablePartList partList) => partList.AreaUnit;
			protected override IClientProviderValues GetMeasureValue(IRateablePart part) => part.AreaMeasure;
		}

		class PartLength : MultiValuePartMeasure
		{
			public override string GetUnit(IRateablePartList partList) => partList.LengthUnit;
			protected override IClientProviderValues GetMeasureValue(IRateablePart part) => part.LengthMeasure;
		}

		class PartUnit : SingleValuePartMeasureWithoutUnits
		{
			public override decimal GetActualValue(IRateablePart part) => part.UnitCount ?? 0;
		}

		class PartUnidentified : SingleValuePartMeasure
		{
			public override decimal GetActualValue(IRateablePart part) => part.UnidentifiedCount ?? 0;
			public override string GetUnit(IRateablePartList partList) => "SV";
		}

		class PartShipment : SingleValuePartMeasureWithoutUnits
		{
			public override decimal GetActualValue(IRateablePart part) => part.ShipmentCount ?? 0;
		}

		class PartLowestBill : SingleValuePartMeasureWithoutUnits
		{
			public override decimal GetActualValue(IRateablePart part) => part.LowestBillCount ?? 0;
		}

		class PartPackage : SingleValuePartMeasure
		{
			public override decimal GetActualValue(IRateablePart part) => part.PackageCount ?? 0;
			public override string GetUnit(IRateablePartList partList) => partList.PackageUnit ?? string.Empty;
		}

		class PartLine : SingleValuePartMeasureWithoutUnits
		{
			public override decimal GetActualValue(IRateablePart part) => part.LineCount ?? 0;
		}

		class PartLoadingMeters : MultiValuePartMeasure
		{
			public override string GetUnit(IRateablePartList partList) => string.Empty;
			protected override IClientProviderValues GetMeasureValue(IRateablePart part) => part.LoadingMeterMeasure;
		}

		class PartChargeable : MultiValuePartMeasure
		{
			public override string GetUnit(IRateablePartList partList) => partList.ChargeableUnit;
			protected override IClientProviderValues GetMeasureValue(IRateablePart part) => part.ChargeableMeasure;
		}

		class PartContainerCount : SingleValuePartMeasure
		{
			public override decimal GetActualValue(IRateablePart part) => (part as IRateableContainer)?.ContainerCount ?? 1;
			public override string GetUnit(IRateablePartList partList) => Core.Constants.BusinessQuantityUnit.Container;
		}

		class PartChargeablePallet : SingleValuePartMeasureWithoutUnits
		{
			public override decimal GetActualValue(IRateablePart part) => part.ChargeablePalletCount ?? 0;
		}

		class PartLocationPallet : SingleValuePartMeasureWithoutUnits
		{
			public override decimal GetActualValue(IRateablePart part) => part.LocationPalletCount ?? 0;
		}

		class PartPalletID : SingleValuePartMeasureWithoutUnits
		{
			/// <summary>
			/// PalletIDs have no specific quantity other than 1, meaning the pallet ID exists.
			/// All the information is in the dimensions.
			/// </summary>
			public override decimal GetActualValue(IRateablePart part) => 1m;
		}

		class PartPickupDistance : SingleValuePartMeasure
		{
			public override decimal GetActualValue(IRateablePart part) => part.PickupDistance;
			public override string GetUnit(IRateablePartList partList) => partList.PickupDistanceUnit;
		}

		class PartDeliveryDistance : SingleValuePartMeasure
		{
			public override decimal GetActualValue(IRateablePart part) => part.DeliveryDistance;
			public override string GetUnit(IRateablePartList partList) => partList.DeliveryDistanceUnit;
		}

		/// <summary>
		/// Time is unique in that all information is in the special TimeInfo class
		/// </summary>
		class PartTime : SingleValuePartMeasureWithoutUnits
		{
			public override decimal GetActualValue(IRateablePart part) => 0m;
		}

		#region Warehouse Package

		class PartWarehousePackage : SingleValuePartMeasure
		{
			public override decimal GetActualValueWithUnit(IRateablePart part, string unit)
			{
				var actualValue = 0m;

				if (part.PackageType == unit && part.PackageCount.HasValue)
				{
					actualValue += part.PackageCount.Value;
				}

				return actualValue;
			}

			public override decimal GetActualValue(IRateablePart part) => 0m;

			public override string GetUnit(IRateablePartList partList) => String.Empty;
		}

		#endregion

		class PartDeclarationLineCount : SingleValuePartMeasureWithoutUnits
		{
			readonly MeasureType measureType;

			public PartDeclarationLineCount(MeasureType measureType)
			{
				this.measureType = measureType;
			}

			public override decimal GetActualValue(IRateablePart part)
				=> part.GetDeclarationLineCount(measureType);
		}
	}
}

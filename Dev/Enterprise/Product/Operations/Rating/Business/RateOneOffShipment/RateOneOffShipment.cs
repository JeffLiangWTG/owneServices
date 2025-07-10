using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Rating.Business
{
	[UniversalCopyWithExtendedEntities]
	[CodeProperty("ParentQuoteNumber"), DescriptionProperty("ParentQuoteNumber")]
	public class RateOneOffShipment : AutoRateOneOffShipment,
	IImportExport,
	IDocAddresses,
	ITemplateCopyable,
	ITemplateReversible,
	ITopLevelBizOProviderForJobDocAddress,
	IDefaultNumberOfDecimalsSupporterWithSchemaColumn,
	IAdditionalReferenceNumberSupporter,
	IAdditionalReferenceNumberTypeProvider
	{
		#region Schema

		public new abstract class Schema : AutoRateOneOffShipment.Schema
		{
			public const string TotalLooseVolume = "TotalLooseVolume";
			public const string TotalLooseWeight = "TotalLooseWeight";
		}

		#endregion

		public RateOneOffShipment(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public IAutoRating RatingAdapter
		{
			get
			{
				if (ratingAdapter == null)
				{
					var adapterType = ObjectFactory.GetType("RateOneOffShipmentRatingAdapter");
					ratingAdapter = (IAutoRating)Activator.CreateInstance(adapterType, this);
				}

				return ratingAdapter;
			}
		}
		IAutoRating ratingAdapter;

		#region Clone

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var cloned = (RateOneOffShipment)base.CloneInternal(args);

			using (cloned.SuspendSettingHasChanges())
			using (cloned.GetValidationSuspender())
			{
				if (TryGetPropertyValueByName(Schema.TT_ContainerMode, out var containerMode)
					&& containerMode is IZTypeInternals source)
				{
					cloned[Schema.TT_ContainerMode] = source;
				}
			}

			foreach (var cargo in LooseCargo)
			{
				var clonedCargo = (RateOneOffPackLine)cargo.Clone();
				cloned.LooseCargo.Add(clonedCargo);
			}

			foreach (RateOneOffContainers container in Containers)
			{
				var clonedContainer = (RateOneOffContainers)container.Clone();
				cloned.Containers.Add(clonedContainer);
			}

			foreach (RateOneOffCarrier possibleCarriers in PossibleCarriers)
			{
				var clonedCarrier = (RateOneOffCarrier)possibleCarriers.Clone();
				cloned.PossibleCarriers.Add(clonedCarrier);
			}

			foreach (var item in Numbers)
			{
				var newItem = item.Clone();
				cloned.Numbers.Add(newItem);
			}

			foreach (JobDocAddress address in DocAddresses)
			{
				var clonedAddress = (JobDocAddress)address.Clone();
				cloned.DocAddresses.Add(clonedAddress);
			}

			return cloned;
		}

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			TT_NumberOfEntries = 1;
			TT_NumberOfEntryLines = 1;
			TT_TransportMode = ZString.Empty;
			IsDomesticFreight = GlbDepartment.CurrentDepartment.GE_Domestic;
			TT_UnitOfWeight = Env.Registry.FreightWeightUnit;
			TT_UnitOfVolume = Env.Registry.FreightVolumeUnit;
			TT_RX_NKGoodsCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			TT_RX_NKInsureValCurr = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			UpdateFreightInformationFieldsReadOnly();
			SetIsDomestic();
		}

		#endregion

		#region Properties

		#region IImportExport Members

		public Directions JobDirection
		{
			get { return ImportExportHelper.GetJobDirection(TT_RL_NKReceivalLocation, TT_RL_NKDeliveryLocation); }
		}

		void SetIsDomesticAndRelatedProperties()
		{
			if (!this.IsUnknown())
			{
				IsDomesticFreight = this.IsDomestic();
			}
		}

		void SetIsDomestic()
		{
			if (!this.IsUnknown())
			{
				isDomesticFreight = this.IsDomestic();
			}
		}

		public ZBool IsDomesticFreight
		{
			get { return isDomesticFreight; }
			set
			{
				if (isDomesticFreight != value && !isSettingDomesticFreight)
				{
					isSettingDomesticFreight = true;
					try
					{
						isDomesticFreight = value;
						IsDomesticFreightInfo.RefreshBinding();

						if (value)
						{
							if (TT_RL_NKDeliveryLocation.IsEmpty)
							{
								TT_RL_NKDeliveryLocation = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
							}

							if (TT_RL_NKReceivalLocation.IsEmpty)
							{
								TT_RL_NKReceivalLocation = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
							}
						}

						if (!TT_IncoTerm.IsEmpty && !Lookups.IncoTerms.ContainsCode(TT_IncoTerm))
						{
							TT_IncoTerm = ZString.Empty;
						}

						SetChargeableWeight(TT_ActualVolume, TT_ActualWeight);
					}
					finally
					{
						isSettingDomesticFreight = false;
					}
				}
			}
		}
		ZBool isDomesticFreight;
		bool isSettingDomesticFreight;

		public ZPropertyInfo IsDomesticFreightInfo
		{
			get { return GetZPropertyInfo(nameof(IsDomesticFreight)); }
		}

		#endregion

		#region TT_DeliveryEquipment

		[List("Lookups.Equipments")]
		public override ZString TT_DeliveryEquipment
		{
			get { return base.TT_DeliveryEquipment; }
			set { base.TT_DeliveryEquipment = value; }
		}

		#endregion

		#region TT_RX_NKGoodsCurrency

		[List("Lookups.GoodsCurrencies")]
		public override ZString TT_RX_NKGoodsCurrency
		{
			get { return base.TT_RX_NKGoodsCurrency; }
			set { base.TT_RX_NKGoodsCurrency = value; }
		}

		#endregion

		#region TT_PickupEquipment

		[List("Lookups.Equipments")]
		public override ZString TT_PickupEquipment
		{
			get { return base.TT_PickupEquipment; }
			set { base.TT_PickupEquipment = value; }
		}

		#endregion

		#region CarrierServiceLevel

		// Test Failure : DefaultClientServiceLevelFromParent
		// LoadFromNaturalKey was used on a table + field that does not have a unique index
		public override OrgCarrierServiceLevel CarrierServiceLevel
		{
			get { return Factory.LoadTop1<OrgCarrierServiceLevel>(new ZQuery(OrgCarrierServiceLevelSchema.PL_Code, TT_PL_NKCarrierServiceLevel)); }
		}

		#endregion

		#region TT_TransportMode & TT_ContainerMode

		bool isSettingDefaultContainer;

		public override ZString TT_TransportMode
		{
			get { return base.TT_TransportMode; }
			set
			{
				if (TT_TransportMode != value)
				{
					base.TT_TransportMode = value;

					isSettingDefaultContainer = true;

					try
					{
						var defaultContainerMode = ZString.Empty;

						var containerModeList = Lookups.ContainerModes;
						if (value != ZString.Empty && containerModeList != null && containerModeList.Count > 0)
						{
							if (Containers.Count > 0)
							{
								defaultContainerMode = containerModeList.ToArray()
									.FirstOrDefault(w =>
										w.Code == Constants.ContainerModes.FCL ||
										w.Code == Constants.ContainerModes.ULD)?.Code;
							}

							if (defaultContainerMode.IsEmpty)
							{
								defaultContainerMode = containerModeList[0].Code;
							}
						}

						if (!defaultContainerMode.IsEmpty)
						{
							TT_ContainerMode = defaultContainerMode;
						}
						UpdateContainerInformation();
					}
					finally
					{
						isSettingDefaultContainer = false;
					}

					UpdateChargeableWeight();
					SetDefaultUnits();

					foreach (var item in LooseCargo.Cast<RateOneOffPackLine>())
					{
						if (TT_TransportMode != Core.Constants.RateMode.ULD)
						{
							item.LooseCargoContainerType = ZString.Empty;
						}
					}
				}
			}
		}

		public override ZString TT_ContainerMode
		{
			get { return base.TT_ContainerMode; }
			set
			{
				if (TT_ContainerMode != value)
				{
					base.TT_ContainerMode = value;

					switch (value)
					{
						case Constants.ContainerModes.FCL:
							TT_HBLDeliveryMode = FreightDataRegistry.Instance.HBLDeliveryMode_FCL.Value.DefaultHBLDeliveryMode;
							break;
						case Constants.ContainerModes.Loose:
							TT_HBLDeliveryMode = FreightDataRegistry.Instance.HBLDeliveryMode_LSE.Value.DefaultHBLDeliveryMode;
							break;
						case Constants.ContainerModes.ULD:
							TT_HBLDeliveryMode = FreightDataRegistry.Instance.HBLDeliveryMode_ULD.Value.DefaultHBLDeliveryMode;
							break;
						case Constants.ContainerModes.LCL:
							TT_HBLDeliveryMode = FreightDataRegistry.Instance.HBLDeliveryMode_LCL.Value.DefaultHBLDeliveryMode;
							break;
						case Constants.ContainerModes.BuyersConsol:
							TT_HBLDeliveryMode = FreightDataRegistry.Instance.HBLDeliveryMode_BCN.Value.DefaultHBLDeliveryMode;
							break;
						case Constants.ContainerModes.ShippersConsol:
							TT_HBLDeliveryMode = FreightDataRegistry.Instance.HBLDeliveryMode_SCN.Value.DefaultHBLDeliveryMode;
							break;
						case Constants.ContainerModes.RollOnRollOff:
						case Constants.ContainerModes.Bulk:
							TT_HBLDeliveryMode = FreightDataRegistry.Instance.HBLDeliveryMode_BBK_ROR_BLK_LQD.Value.DefaultHBLDeliveryMode;
							break;
						default:
							TT_HBLDeliveryMode = ZString.Empty;
							break;
					}
				}

				if (!isSettingDefaultContainer)
				{
					UpdateChargeableWeight();
					UpdateFreightInformationFieldsReadOnly();
					foreach (var container in Containers)
					{
						container.MarkAsNeedingValidation();
					}
					SetDefaultUnits();
				}
			}
		}

		void SetDefaultUnits()
		{
			if (string.IsNullOrEmpty(TT_UnitOfVolume))
			{
				TT_UnitOfVolume = (ZString)Env.Registry.FreightVolumeUnit;
			}

			if (string.IsNullOrEmpty(TT_UnitOfWeight))
			{
				TT_UnitOfWeight = (ZString)Env.Registry.FreightWeightUnit;
			}
		}

		void UpdateChargeableWeight()
		{
			if (!fSettingActualChargeable)
			{
				SetChargeableWeight(TT_ActualVolume, TT_ActualWeight);
			}

			TT_ChargeableUnitInfo.RefreshBinding();
		}

		void UpdateFreightInformationFieldsReadOnly()
		{
			if (ReadOnly)
			{
				return;
			}

			TT_ActualVolumeInfo.RefreshBinding();
			TT_ActualWeightInfo.RefreshBinding();
			TT_ChargeableInfo.RefreshBinding();
			TT_UnitOfWeightInfo.RefreshBinding();
			TT_UnitOfVolumeInfo.RefreshBinding();
			UpdateContainerInformation();
			if (TT_ContainerMode == Constants.ContainerModes.FCL)
			{
				Validation.ValidateTT_ActualVolume();
				Validation.ValidateTT_ActualWeight();
				Validation.ValidateTT_Chargeable();
			}
		}

		void UpdateContainerInformation()
		{
			Containers.SetReadOnlyIncludingChildren(false);
			LooseCargo.SetReadOnlyIncludingChildren(false);

			if (Mode == Core.Constants.RateMode.LSE ||
					Mode == Core.Constants.RateMode.LCL ||
					TT_ContainerMode == Core.Constants.ContainerModes.Bulk ||
					TT_ContainerMode == Core.Constants.ContainerModes.BreakBulk ||
					TT_ContainerMode == Core.Constants.ContainerModes.RollOnRollOff ||
					TT_ContainerMode == Core.Constants.ContainerModes.Liquid ||
					TT_ContainerMode == Core.Constants.ContainerModes.OnBoardCourier ||
					TT_ContainerMode == Core.Constants.ContainerModes.Unaccompanied)
			{
				if (ParentQuote == null || (ParentQuote != null && !ParentQuote.IsInComparisonMode))
				{
					Containers.SetReadOnlyIncludingChildren(true);
					Containers.RemoveAndDeleteAll();
				}
			}
			else if (Mode == Core.Constants.RateMode.FCL)
			{
				if (ParentQuote == null || (!ParentQuote.IsInComparisonMode && !ParentQuote.HasBindBooking))
				{
					LooseCargo.SetReadOnlyIncludingChildren(true);
					LooseCargo.DeleteAll();
				}
			}
		}

		public string StandardTransportMode
		{
			get
			{
				if (IsAir || IsOther)
				{
					return Core.Constants.TransportModes.Air;
				}
				if (IsSea)
				{
					return Core.Constants.TransportModes.Sea;
				}
				if (IsRoad)
				{
					return Core.Constants.TransportModes.Road;
				}
				if (IsRail)
				{
					return Core.Constants.TransportModes.Rail;
				}
				if (IsCourier)
				{
					return Core.Constants.TransportModes.Courier;
				}

				return TT_TransportMode;
			}
		}

		/// <summary>
		/// Converted version of TT_ContainerMode
		/// Use this in transfers to other systems that do not understand SEA, RAI, ROA and FWL OOQ container modes.
		/// </summary>
		public ZString BookingContainerMode => FreightModeConverter.GetBookingContainerModeFromContainerMode(TT_ContainerMode);

		public ZString Mode => FreightModeConverter.GetMode(TT_TransportMode, TT_ContainerMode);

		public bool IsAir => TT_TransportMode == Constants.TransportModes.Air;

		public bool IsSea => TT_TransportMode == Constants.TransportModes.Sea;

		public bool IsRail => TT_TransportMode == Constants.TransportModes.Rail;

		public bool IsRoad => TT_TransportMode == Constants.TransportModes.Road;

		public bool IsCourier => TT_TransportMode == Constants.TransportModes.Courier || TT_TransportMode == Constants.TransportModes.Mail;

		public bool IsOther => TT_TransportMode == Constants.TransportModes.Other;

		public bool IsFCL => TT_ContainerMode == Constants.ContainerModes.FCL;

		#endregion

		#region TT_ChargeableUnit

		[List("Lookups.UnitOfWeightList")]
		public ZString TT_ChargeableUnit
		{
			get { return ChargeableAmountCalculator.GetChargeableUnit(StandardTransportMode, TT_UnitOfWeight, TT_UnitOfVolume); }
		}

		public ZPropertyInfo TT_ChargeableUnitInfo
		{
			get { return GetZPropertyInfo(nameof(TT_ChargeableUnit)); }
		}

		#endregion

		#region TT_Chargeable

		bool fSettingActualChargeable;

		[ReadOnlyMember(nameof(IsFCL))]
		public override ZDecimal TT_Chargeable
		{
			get { return base.TT_Chargeable; }
			set
			{
				if (fSettingActualChargeable)
				{
					return;
				}

				fSettingActualChargeable = true;
				try
				{
					if (IsAir)
					{
						value = ChargeableWeightRoundingHelper.GetRoundedValueAir(RateOneOffShipmentSchema.TT_Chargeable, value);
					}

					var roundedValue = this.GetRoundedValue(RateOneOffShipmentSchema.TT_Chargeable, TT_ChargeableInfo, value);
					base.TT_Chargeable = roundedValue;

					UpdateActualsFromChargeable();
				}
				finally
				{
					fSettingActualChargeable = false;
				}
				Validation.ValidateTT_ActualVolume();
				Validation.ValidateTT_ActualWeight();
			}
		}

		void UpdateActualsFromChargeable()
		{
			if (TT_Chargeable.IsEmpty)
			{
				return;
			}

			if (FreightDataRegistry.Instance.WeightChargableTransportModes.Contains(StandardTransportMode) && TT_ActualVolume.IsEmpty)
			{
				var chargeableWeight = new ZWeight(TT_Chargeable, TT_ChargeableUnit);
				var actualWeight = new ZWeight(TT_ActualWeight, TT_UnitOfWeight);
				var actualVolume = new ZVolume(TT_ActualVolume, TT_UnitOfVolume);

				if (chargeableWeight.IsValid && actualWeight.IsValid && actualVolume.IsValid)
				{
					var newActualVolume = ChargeableAmountCalculator.GetActualFromChargeable(StandardTransportMode, IsDomesticFreight, chargeableWeight, actualWeight, actualVolume);

					if (newActualVolume.HasValue)
					{
						TT_ActualVolume = newActualVolume.Value.Amount;
					}
				}
			}
			else if (FreightDataRegistry.Instance.VolumeChargableTransportModes.Contains(StandardTransportMode) && TT_ActualWeight.IsEmpty)
			{
				var chargeableVolume = new ZVolume(TT_Chargeable, TT_ChargeableUnit);
				var actualWeight = new ZWeight(TT_ActualWeight, TT_UnitOfWeight);
				var actualVolume = new ZVolume(TT_ActualVolume, TT_UnitOfVolume);

				if (chargeableVolume.IsValid && actualWeight.IsValid && actualVolume.IsValid)
				{
					var newActualWeight = ChargeableAmountCalculator.GetActualFromChargeable(StandardTransportMode, IsDomesticFreight, chargeableVolume, actualWeight, actualVolume);

					if (newActualWeight.HasValue)
					{
						TT_ActualWeight = newActualWeight.Value.Amount;
					}
				}
			}
		}

		#endregion

		#region TT_ActualWeight

		[ReadOnlyMember(nameof(IsFCL))]
		[MeasureUnit(Schema.TT_UnitOfWeight, MeasureUnitType.Weight)]
		public override ZDecimal TT_ActualWeight
		{
			get { return base.TT_ActualWeight; }
			set
			{
				base.TT_ActualWeight = value;

				if (TT_ActualWeight > 0.0M && TT_UnitOfWeight.IsEmpty)
				{
					TT_UnitOfWeight = Env.Registry.FreightWeightUnit;
				}

				if (!TT_ActualWeightInfo.HasErrors() && !fSettingActualChargeable)
				{
					SetChargeableWeight(TT_ActualVolume, TT_ActualWeight);
				}

				Validation.ValidateTT_ActualVolume();
				Validation.ValidateTT_TransportMode();
			}
		}

		#endregion

		#region TT_ActualVolume

		[ReadOnlyMember(nameof(IsFCL))]
		[MeasureUnit(Schema.TT_UnitOfVolume, MeasureUnitType.Volume)]
		public override ZDecimal TT_ActualVolume
		{
			get { return base.TT_ActualVolume; }
			set
			{
				base.TT_ActualVolume = value;

				if (TT_ActualVolume > 0.0M && TT_UnitOfVolume.IsEmpty)
				{
					TT_UnitOfVolume = Env.Registry.FreightVolumeUnit;
				}

				if (!TT_ActualVolumeInfo.HasErrors() && !fSettingActualChargeable)
				{
					SetChargeableWeight(TT_ActualVolume, TT_ActualWeight);
				}

				Validation.ValidateTT_ActualWeight();
				Validation.ValidateTT_TransportMode();
			}
		}

		#endregion

		#region Recalculations

		public void RecalculateVolume()
		{
			RecalculateVolumeUnit();
			if (TotalLooseVolume.IsWithinSqlPrecisionAndScale(RateOneOffShipmentSchema.TT_ActualVolume.Precision, RateOneOffShipmentSchema.TT_ActualVolume.Scale))
			{
				TT_ActualVolume = TotalLooseVolume;
			}
			else
			{
				CalculatedVolumeIsNotWithinSqlPrecisionAndScale?.Invoke(this, null);
			}
		}

		public event EventHandler CalculatedVolumeIsNotWithinSqlPrecisionAndScale;

		internal void RecalculateVolumeUnit()
		{
			var cargo = LooseCargo.Where(l => !l.IsDeleted);

			var firstUnit = new ZString(cargo.FirstOrDefault()?.TPL_VolumeUQ);
			if (cargo.All(c => c.TPL_VolumeUQ.Equals(firstUnit)))
			{
				TT_UnitOfVolume = firstUnit;
			}
		}

		public ZDecimal TotalLooseVolume
		{
			get
			{
				ZDecimal result = 0;
				if (Core.Constants.Volume.ContainsCode(TT_UnitOfVolume))
				{
					foreach (var cargo in LooseCargo)
					{
						if (Core.Constants.Volume.ContainsCode(cargo.TPL_VolumeUQ))
						{
							var cargoVolume = Core.Constants.Volume.Convert(cargo.TPL_Volume, cargo.TPL_VolumeUQ, TT_UnitOfVolume);
							result += cargoVolume;
						}
					}
				}

				var roundedValue = this.GetRoundedValue(RateOneOffShipmentSchema.TT_ActualVolume, TotalLooseVolumeInfo, result);
				return roundedValue;
			}
		}

		public ZPropertyInfo TotalLooseVolumeInfo
		{
			get { return GetZPropertyInfo(Schema.TotalLooseVolume); }
		}

		public void RecalculateWeight()
		{
			RecalculateWeightUnit();

			if (TotalLooseWeight.IsWithinSqlPrecisionAndScale(RateOneOffShipmentSchema.TT_ActualWeight.Precision, RateOneOffShipmentSchema.TT_ActualWeight.Scale))
			{
				TT_ActualWeight = TotalLooseWeight;
			}
			else
			{
				CalculatedWeightIsNotWithinSqlPrecisionAndScale?.Invoke(this, null);
			}
		}

		public event EventHandler CalculatedWeightIsNotWithinSqlPrecisionAndScale;

		internal void RecalculateWeightUnit()
		{
			var cargo = LooseCargo.Where(l => !l.IsDeleted);

			var firstUnit = new ZString(cargo.FirstOrDefault()?.TPL_WeightUQ);
			if (cargo.All(c => c.TPL_WeightUQ.Equals(firstUnit)))
			{
				TT_UnitOfWeight = firstUnit;
			}
		}

		public ZDecimal TotalLooseWeight
		{
			get
			{
				ZDecimal result = 0;
				if (Core.Constants.Weight.ContainsCode(TT_UnitOfWeight))
				{
					foreach (var cargo in LooseCargo)
					{
						if (Core.Constants.Weight.ContainsCode(cargo.TPL_WeightUQ))
						{
							var cargoWeight = Core.Constants.Weight.Convert(cargo.TPL_Weight, cargo.TPL_WeightUQ, TT_UnitOfWeight);
							result += cargoWeight;
						}
					}
				}

				result = this.GetRoundedValue(TotalLooseWeightInfo, result);
				return result;
			}
		}

		public ZPropertyInfo TotalLooseWeightInfo
		{
			get { return GetZPropertyInfo(Schema.TotalLooseWeight); }
		}

		#endregion

		#region TT_UnitOfVolume

		[ReadOnlyMember(nameof(IsFCL))]
		[List("Lookups.UnitOfVolumeList")]
		public override ZString TT_UnitOfVolume
		{
			get { return base.TT_UnitOfVolume; }
			set
			{
				if (TT_UnitOfVolume != value)
				{
					base.TT_UnitOfVolume = value;

					if (!fSettingActualChargeable)
					{
						SetChargeableWeight(TT_ActualVolume, TT_ActualWeight);
					}
				}
			}
		}

		#endregion

		#region TT_UnitOfWeight

		[ReadOnlyMember(nameof(IsFCL))]
		[List("Lookups.UnitOfWeightList")]
		public override ZString TT_UnitOfWeight
		{
			get { return base.TT_UnitOfWeight; }
			set
			{
				if (TT_UnitOfWeight != value)
				{
					base.TT_UnitOfWeight = value;

					if (!fSettingActualChargeable)
					{
						SetChargeableWeight(TT_ActualVolume, TT_ActualWeight);
					}
				}
			}
		}

		#endregion

		#region Chargeable Calculation

		void SetChargeableWeight(decimal volume, decimal weight)
		{
			Validation.ValidateTT_UnitOfWeight();
			Validation.ValidateTT_UnitOfVolume();
			var weightUnit = TT_UnitOfWeight.ToString();
			var volumeUnit = TT_UnitOfVolume.ToString();

			if (volume == 0 && weight == 0)
			{
				TT_Chargeable = 0;
			}
			else if (Constants.Weight.ContainsCode(weightUnit) && Constants.Volume.ContainsCode(volumeUnit))
			{
				var chargeable = ChargeableAmountCalculator.CalculateChargeable(new ChargeableParameters
				{
					Weight = new ZWeight(weight, weightUnit),
					Volume = new ZVolume(volume, volumeUnit),
					TargetUnit = TT_ChargeableUnit,
					ConversionFactors = ChargeableAmountCalculator.GetDefaultConversionFactors(this.IsDomestic(), StandardTransportMode, TT_ChargeableUnit)
				}).Chargeable.Amount;

				if (chargeable.IsWithinSqlPrecisionAndScale(RateOneOffShipmentSchema.TT_Chargeable.Precision, RateOneOffShipmentSchema.TT_Chargeable.Scale))
				{
					TT_Chargeable = chargeable;
				}
				else
				{
					CalculatedChargeable = chargeable;
					CalculatedChargeableIsNotWithinSqlPrecisionAndScale?.Invoke(this, null);
				}
			}
		}

		public ZDecimal CalculatedChargeable
		{
			get;set;
		}

		public event EventHandler CalculatedChargeableIsNotWithinSqlPrecisionAndScale;

		#endregion

		#region TT_RS_NKServiceLevel

		[List("Lookups.ServiceLevels")]
		public override ZString TT_RS_NKServiceLevel
		{
			get { return base.TT_RS_NKServiceLevel; }
			set { base.TT_RS_NKServiceLevel = value; }
		}

		#endregion

		#region TT_RH_NKCommodity

		[List("Lookups.Commodities")]
		public override ZString TT_RH_NKCommodity
		{
			get { return base.TT_RH_NKCommodity; }
			set { base.TT_RH_NKCommodity = value; }
		}

		#endregion

		#region TT_RL_NKDeliveryLocation

		public override ZString TT_RL_NKDeliveryLocation
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return base.TT_RL_NKDeliveryLocation; }
			set
			{
				if (TT_RL_NKDeliveryLocation != value)
				{
					base.TT_RL_NKDeliveryLocation = value;
					SetIsDomesticAndRelatedProperties();
				}
			}
		}

		#endregion

		#region TT_RL_NKReceivalLocation

		public override ZString TT_RL_NKReceivalLocation
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return base.TT_RL_NKReceivalLocation; }
			set
			{
				if (TT_RL_NKReceivalLocation != value)
				{
					base.TT_RL_NKReceivalLocation = value;
					SetIsDomesticAndRelatedProperties();
				}
			}
		}

		#endregion

		#region TT_QuoteKPI

		[List("Lookups.OneOffQuoteKPIList")]
		public override ZString TT_QuoteKPI { get => base.TT_QuoteKPI; set => base.TT_QuoteKPI = value; }

		#endregion

		#region TT_QuoteSource

		[List("Lookups.OneOffQuoteSourceList")]
		public override ZString TT_QuoteSource { get => base.TT_QuoteSource; set => base.TT_QuoteSource = value; }

		#endregion

		#region TT_RevisionReason

		[List("Lookups.OneOffQuoteRevisionReasonList")]
		public override ZString TT_RevisionReason { get => base.TT_RevisionReason; set => base.TT_RevisionReason = value; }

		#endregion

		#endregion

		[List("Lookups.CompanyTariffLevelList")]
		public override ZByte TT_CompanyTariffLevelOverride { get => base.TT_CompanyTariffLevelOverride; set => base.TT_CompanyTariffLevelOverride = value; }

		[List("Lookups.HBLDeliveryModesList")]
		public override ZString TT_HBLDeliveryMode { get => base.TT_HBLDeliveryMode; set => base.TT_HBLDeliveryMode = value; }

		#region Related Business Objects

		#region Container + Loose Cargo Collection

		[ChildEditable(true)]
		// Use different collection name so this property will not be accessed and initialized during copy - data will be loaded directly from factory/db
		[UniversalCopyCollectionEntity(RateOneOffContainersSchema.Constants.TableName, RateOneOffContainersSchema.Constants.TC_TT, OverrideCollectionName = "Containers")]
		public RateOneOffContainersCollection Containers
		{
			get
			{
				if (fContainers == null)
				{
					fContainers = new RateOneOffContainersCollection(this, false);
					fContainers.Load();

					RegisterEditableChildObject(fContainers);
				}
				return fContainers;
			}
		}
		RateOneOffContainersCollection fContainers;

		[ChildEditable(true)]
		// Use different collection name so this property will not be accessed and initialized during copy - data will be loaded directly from factory/db
		[UniversalCopyCollectionEntity(RateOneOffPackLineSchema.Constants.TableName, RateOneOffPackLineSchema.Constants.TPL_TT_RateOneOffShipment, OverrideCollectionName = "LooseCargo")]
		public RateOneOffPackLineCollection LooseCargo
		{
			get
			{
				if (fLooseCargo == null)
				{
					fLooseCargo = new RateOneOffPackLineCollection(this);
					RegisterEditableChildObject(fLooseCargo);
				}
				return fLooseCargo;
			}
		}

		RateOneOffPackLineCollection fLooseCargo;

		#endregion

		#region PickUp

		[UniversalCopyRelatedEntity(CommaSeparatedSkipPropertiesNames = "E2_ParentID", DisableCopyMethodLink = true, MakeRelatedEntitySavedByFactoryMethod = "MakePersistentEvenIfEmpty")]
		public virtual JobDocAddress PickUpDocAddress
		{
			get
			{
				if (pickUpDocAddress == null || pickUpDocAddress.IsDeleted)
				{
					pickUpDocAddress = DocAddresses.FindOrCreateWithRequirement(PickupDocAddressRequirement);
				}

				return pickUpDocAddress;
			}
		}
		JobDocAddress pickUpDocAddress;

		JobDocAddressRequirement PickupDocAddressRequirement
		{
			get
			{
				if (pickupDocAddressRequirement == null)
				{
					pickupDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.OneOffQuotePickupAddress, AddressType.PIC);
					pickupDocAddressRequirement.ValidateOrganisationPK = Validation.ValidateConsignorPK;
				}
				return pickupDocAddressRequirement;
			}
		}
		JobDocAddressRequirement pickupDocAddressRequirement;

		#endregion

		#region DocAddresses

		[ChildEditable(true)]
		public virtual JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (docAddresses == null)
				{
					docAddresses = new JobDocAddressDependentCollection(this);
					docAddresses.Load();
					RegisterEditableChildObject(docAddresses);
				}

				return docAddresses;
			}
		}
		JobDocAddressDependentCollection docAddresses;

		#endregion

		#region Delivery

		[UniversalCopyRelatedEntity(CommaSeparatedSkipPropertiesNames = "E2_ParentID", DisableCopyMethodLink = true, MakeRelatedEntitySavedByFactoryMethod = "MakePersistentEvenIfEmpty")]
		public virtual JobDocAddress DeliveryDocAddress
		{
			get
			{
				if (deliveryDocAddress == null || deliveryDocAddress.IsDeleted)
				{
					deliveryDocAddress = DocAddresses.FindOrCreateWithRequirement(DeliverDocAddressRequirement);
				}

				return deliveryDocAddress;
			}
		}
		JobDocAddress deliveryDocAddress;

		JobDocAddressRequirement DeliverDocAddressRequirement
		{
			get
			{
				if (deliverDocAddressRequirement == null)
				{
					deliverDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.OneOffQuoteDeliveryAddress, AddressType.DLV);
					deliverDocAddressRequirement.ValidateOrganisationPK = Validation.ValidateConsigneePK;
				}

				return deliverDocAddressRequirement;
			}
		}
		JobDocAddressRequirement deliverDocAddressRequirement;

		#endregion

		#region Possible Carriers

		[ChildEditable(true)]
		// Use different collection name so this property will not be accessed and initialized during copy - data will be loaded directly from factory/db
		[UniversalCopyCollectionEntity(RateOneOffCarrierSchema.Constants.TableName, RateOneOffCarrierSchema.Constants.TTC_TT, OverrideCollectionName = "Carriers")]
		public RateOneOffCarrierCollection PossibleCarriers
		{
			get
			{
				if (possibleCarriers == null)
				{
					possibleCarriers = new (this);
					possibleCarriers.Load();

					RegisterEditableChildObject(possibleCarriers);
				}
				return possibleCarriers;
			}
		}
		RateOneOffCarrierCollection possibleCarriers;

		#endregion

		public override void Delete()
		{
			Containers.RemoveAndDeleteAll();
			LooseCargo.DeleteAll();
			Numbers.RemoveAndDeleteAll();
			PickUpDocAddress.Delete();
			DeliveryDocAddress.Delete();
			PossibleCarriers.RemoveAndDeleteAll();
			base.Delete();
		}

		#endregion

		#region Parent Quotation

		public Quote ParentQuote
		{
			get { return Factory.Load<Quote>(TT_TH); }
		}

		public ZString ParentQuoteNumber
		{
			get
			{
				if (ParentQuote == null)
				{
					return ZString.Empty;
				}

				return ParentQuote.TH_QuoteNumber;
			}
		}

		#endregion

		#region Event Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region Notes

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				var notes = base.NoteTypesCore;
				notes.Add(PredefinedNoteTypes.Instance.AutoRatingAuditLog);
				notes.Add(PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes);
				return notes;
			}
		}

		#endregion

		#region IDocAddresses Members

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return GetCanOverrideAddressCheckpoint(docAddress);
		}

		protected virtual SecurityCheckpoint GetCanOverrideAddressCheckpoint(JobDocAddress docAddress)
		{
			SecurityCheckpoint result;

			if (docAddress.E2_AddressType == DocAddressTypes.Codes.OneOffQuotePickupAddress)
			{
				result = ConsignorAddressOverrideCheckpoint;
			}
			else if (docAddress.E2_AddressType == DocAddressTypes.Codes.OneOffQuoteDeliveryAddress)
			{
				result = ConsigneeAddressOverrideCheckpoint;
			}
			else
			{
				result = Env.Security.MaintainOneOffQuotes;
			}

			return result;
		}

		#region Address Override Security

		SecurityCheckpoint ConsigneeAddressOverrideCheckpoint
		{
			get
			{
				SecurityCheckpoint result;

				if (this.IsImport())
				{
					if (IsAir)
					{
						result = Env.Security.MaintainOneOffQuoteImpAirConsigneeD;
					}
					else if (IsSea)
					{
						result = Env.Security.MaintainOneOffQuoteImpSeaConsigneeD;
					}
					else if (IsRail)
					{
						result = Env.Security.MaintainOneOffQuoteImpRailConsigneeD;
					}
					else if (IsRoad)
					{
						result = Env.Security.MaintainOneOffQuoteImpRoadConsigneeD;
					}
					else
					{
						result = Env.Security.MaintainOneOffQuoteImpOtConsignee;
					}
				}
				else if (this.IsExport())
				{
					if (IsAir)
					{
						result = Env.Security.MaintainOneOffQuoteExpAirConsigneeD;
					}
					else if (IsSea)
					{
						result = Env.Security.MaintainOneOffQuoteExpSeaConsigneeD;
					}
					else if (IsRail)
					{
						result = Env.Security.MaintainOneOffQuoteExpRailConsigneeD;
					}
					else if (IsRoad)
					{
						result = Env.Security.MaintainOneOffQuoteExpRoadConsigneeD;
					}
					else
					{
						result = Env.Security.MaintainOneOffQuoteExpOtConsignee;
					}
				}
				else
				{
					result = Env.Security.MaintainOneOffQuotes;
				}

				return result;
			}
		}

		SecurityCheckpoint ConsignorAddressOverrideCheckpoint
		{
			get
			{
				SecurityCheckpoint result;

				if (this.IsImport())
				{
					if (IsAir)
					{
						result = Env.Security.MaintainOneOffQuoteImpAirConsignorD;
					}
					else if (IsSea)
					{
						result = Env.Security.MaintainOneOffQuoteImpSeaConsignorD;
					}
					else if (IsRail)
					{
						result = Env.Security.MaintainOneOffQuoteImpRailConsignorD;
					}
					else if (IsRoad)
					{
						result = Env.Security.MaintainOneOffQuoteImpRoadConsignorD;
					}
					else
					{
						result = Env.Security.MaintainOneOffQuoteImpOtConsignor;
					}
				}
				else if (this.IsExport())
				{
					if (IsAir)
					{
						result = Env.Security.MaintainOneOffQuoteExpAirConsignorD;
					}
					else if (IsSea)
					{
						result = Env.Security.MaintainOneOffQuoteExpSeaConsignorD;
					}
					else if (IsRail)
					{
						result = Env.Security.MaintainOneOffQuoteExpRailConsignorD;
					}
					else if (IsRoad)
					{
						result = Env.Security.MaintainOneOffQuoteExpRoadConsignorD;
					}
					else
					{
						result = Env.Security.MaintainOneOffQuoteExpOtConsignor;
					}
				}
				else
				{
					result = Env.Security.MaintainOneOffQuotes;
				}

				return result;
			}
		}

		#endregion

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return new RateOneOffShipmentDocAddressValidation(addressToValidate);
		}

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
		{
			get
			{
				return new DocAddressType[]
				{
					DocAddressType.OneOffQuoteDeliveryAddress,
										DocAddressType.OneOffQuotePickupAddress
				};
			}
		}

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			switch (addressType)
			{
				case DocAddressType.OneOffQuoteDeliveryAddress:
					return DeliverDocAddressRequirement;
				case DocAddressType.OneOffQuotePickupAddress:
					return PickupDocAddressRequirement;
				default:
					return null;
			}
		}

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress)
		{
			return false;
		}

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			return null;
		}

		#endregion

		#region ITemplateReversible Members

		void ITemplateReversible.Reverse()
		{
			var newDeliveryLocation = TT_RL_NKReceivalLocation;
			var newRecivalLocation = TT_RL_NKDeliveryLocation;

			TT_RL_NKDeliveryLocation = newDeliveryLocation;
			TT_RL_NKReceivalLocation = newRecivalLocation;

			var deliveryAddressType = DeliveryDocAddress.E2_AddressType;
			DeliveryDocAddress.E2_AddressType = PickUpDocAddress.E2_AddressType;
			PickUpDocAddress.E2_AddressType = deliveryAddressType;
			deliveryDocAddress = null;
			pickUpDocAddress = null;
		}

		#endregion

		#region ITemplateCopyable Members

		IBusiness ITemplateCopyable.TemplateCopy()
		{
			return Clone();
		}

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new RateOneOffShipmentFetchStrategy(this);
		}

		#endregion

		public BusinessObject GetTopBusinessObject() => Factory.Load<RatingHeader>(TT_TH);

		#region IDefaultNumberOfDecimalsSupporter Members

		ZString IDefaultNumberOfDecimalsSupporter.TransportMode
		{
			get { return StandardTransportMode; }
		}

		ZString IDefaultNumberOfDecimalsSupporter.GetUnitOfMeasure(PropertyDescriptor property)
		{
			var unitOfMeasure = ZString.Empty;
			var propertyName = DefaultNumberOfDecimals.GetBoundPropertyName(property.Name);

			switch (propertyName)
			{
				case Schema.TotalLooseVolume:
					unitOfMeasure = TT_UnitOfVolume;
					break;

				case Schema.TotalLooseWeight:
					unitOfMeasure = TT_UnitOfWeight;
					break;

				case Schema.TT_Chargeable:
					unitOfMeasure = TT_ChargeableUnit;
					break;

				default:
					break;
			}

			return unitOfMeasure;
		}

		public int GetDefaultNumberOfDecimals(PropertyDescriptor property)
		{
			return DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(this, property);
		}

		public ZDecimal GetRoundedValue(SchemaColumn column, PropertyDescriptor property, ZDecimal value)
		{
			return DefaultNumberOfDecimalsSupporterWithSchemaColumnHelperForFreight.GetRoundedValue(this, column, property, value);
		}

		public ZDecimal GetRoundedValue(PropertyDescriptor property, ZDecimal value)
		{
			return DefaultNumberOfDecimalsSupporterHelperForFreight.GetRoundedValue(this, property, value);
		}

		void IDefaultNumberOfDecimalsSupporter.RoundMeasurePropertiesOnTransportModeChanged()
		{
			this.SetRoundedValue(TotalLooseVolumeInfo);
			this.SetRoundedValue(TotalLooseWeightInfo);
		}

		#endregion

		#region Mode Converter Class

		public static class FreightModeConverter
		{
			public static FreightMode GetFreightMode(ZString transportMode, ZString containerMode, RateOneOffShipment shipment)
			{
				if (transportMode.IsEmpty && containerMode.IsEmpty)
				{
					return FreightMode.UKN;
				}
				var isContainerized = false;
				var mode = GetMode(transportMode, containerMode);

				if (mode == Constants.RateMode.SEA)
				{
					return shipment.Containers.Count > 0 ? FreightMode.FCL : FreightMode.LCL;
				}

				if (mode == Constants.RateMode.ROA)
				{
					return shipment.Containers.Count > 0 ? FreightMode.FRO : FreightMode.ROA;
				}

				if (mode == Constants.RateMode.RAI)
				{
					return shipment.Containers.Count > 0 ? FreightMode.FRA : FreightMode.RAI;
				}

				if (mode == Constants.RateMode.FWL)
				{
					var result = FreightRatingHelper.CalculateFreightModeFromTransportMode(transportMode);
					return result | FreightMode.FullLoad | FreightMode.NonContainerised;
				}

				return FreightRatingHelper.CalculateFreightMode(transportMode, containerMode, isContainerized);
			}

			public static ZString GetMode(ZString transportMode, ZString containerMode)
			{
				ZString result = "";
				if (!transportMode.IsEmpty)
				{
					switch (transportMode)
					{
						case Constants.TransportModes.Air:
							switch (containerMode)
							{
								case Constants.ContainerModes.AIR:
								case Constants.ContainerModes.Loose:
									result = Constants.RateMode.LSE;
									break;
								default:
									result = containerMode;
									break;
							}
							break;
						case Constants.TransportModes.Road:
							switch (containerMode)
							{
								case Constants.ContainerModes.FCL:
									result = Constants.RateMode.FRO;
									break;
								case Constants.ContainerModes.LCL:
								case Constants.ContainerModes.LTL:
									result = Constants.RateMode.LRO;
									break;
								default:
									result = containerMode;
									break;
							}
							break;
						case Constants.TransportModes.Rail:
							switch (containerMode)
							{
								case Constants.ContainerModes.FCL:
									result = Constants.RateMode.FRA;
									break;
								case Constants.ContainerModes.LCL:
									result = Constants.RateMode.LRA;
									break;
								default:
									result = containerMode;
									break;
							}
							break;
						case Constants.TransportModes.Sea:
						case Constants.TransportModes.SeaAir:
						case Constants.TransportModes.AirSea:
						case Constants.TransportModes.Courier:
							result = containerMode;
							break;
						default:
							break;
					}
				}

				return result;
			}

			public static string GetBookingContainerModeFromContainerMode(string containerModeInOOQ)
			{
				switch (containerModeInOOQ)
				{
					case Constants.RateMode.SEA: //Sea Freight (LCL and FCL)
						return Constants.ContainerModes.FCL;
					case Constants.RateMode.ROA: //Road Freight (LCL/LTL, FCL and FTL)
					case Constants.RateMode.LRO: //Road Freight (LCL/LTL)
					case Constants.RateMode.RAI: //Rail Freight (LCL, FCL and FWL)
					case Constants.RateMode.FWL: //Rail Freight (FWL)
						return Constants.ContainerModes.LCL;
					case Constants.RateMode.COU: //Courier (OBC and UNA)
						return Constants.ContainerModes.OnBoardCourier;
					default:
						return containerModeInOOQ;
				}
			}

			public static string[] GetRateModes(FreightMode freightMode)
			{
				return new[] { freightMode.ToString(), GetParentMode(freightMode).ToString() }.Distinct().ToArray();
			}

			static FreightMode GetParentMode(FreightMode mode)
			{
				switch (mode)
				{
					case (FreightMode.FCL):
					case (FreightMode.LCL):
						return FreightMode.SEA;

					case (FreightMode.FRA):
					case (FreightMode.LRA):
					case (FreightMode.FWL):
						return FreightMode.RAI;

					case (FreightMode.FRO):
					case (FreightMode.LRO):
					case (FreightMode.FTL):
						return FreightMode.ROA;

					default:
						return mode;
				}
			}
		}

		#endregion
		#region IAdditionalReferenceNumberTypeProvider Members

		CodeDescriptionPairList IAdditionalReferenceNumberTypeProvider.GetAdditionalReferenceNumberTypeList(ZString category, ZString countryCode)
		{
			var categoryCacheKeyPart = category == CusEntryNumber.Categories.AdditionalReferenceNumber ? "ARN" : "NAN"; // Cache Key
			string cacheKey = string.Format(CultureInfo.CurrentCulture, "GetAdditionalReferenceNumberTypeList_{0}_{1}", this.GetType().ToString(), categoryCacheKeyPart); // Cache Key
			return Factory.GetCachedValue(cacheKey,
				() =>
				{
					var validNumberTypes = new string[]
					{
						CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON,
						CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ContractNamedAccount,
						CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CarrierQuoteNumber
					};

					CodeDescriptionPairList result = new CodeDescriptionPairList();

					if (category == CusEntryNumber.Categories.AdditionalReferenceNumber)
					{
						result.AddRange
							(
								CusEntryNumLookups
								.GetAdditionalReferenceNumberTypes(countryCode)
								.ToArray()
								.Where(i => validNumberTypes.Contains(i.Code))
								.ToList()
							);
					}
					return result;
				});
		}

		#endregion

		#region IAdditionalReferenceNumberSupporter Members

		void IAdditionalReferenceNumberSupporter.CreateOrUpdate(ZString numberType, ZString countryCode, ZString value, INotifications notify)
		{
			var additionalReferenceNumberTypes = ((IAdditionalReferenceNumberTypeProvider)this).GetAdditionalReferenceNumberTypeList(
				CusEntryNumber.Categories.AdditionalReferenceNumber,
				countryCode);

			if (!additionalReferenceNumberTypes.ContainsCode(numberType))
			{
				notify.Notify(new WarningNotification(WarningType.Warning, Res.GetString("8d5c5cb9-8115-45ac-bcbe-bb9c2563ad2a", "No support for additional reference number type '{0}'", numberType)));
				return;
			}

			var country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, countryCode);
			if (country != null)
			{
				CusEntryNumber number = GetCusEntryNumber(numberType, country, CusEntryNumber.Categories.AdditionalReferenceNumber);
				if (number != null)
				{
					number.CE_EntryNum = value;
				}
				else
				{
					CreateCusEntryNumber(numberType, country, value, CusEntryNumber.Categories.AdditionalReferenceNumber);
				}
			}
		}

		bool IAdditionalReferenceNumberSupporter.IncludeSpecialCustomsInstructionsItems
		{
			get { return true; }
		}

		void IAdditionalReferenceNumberSupporter.OnEntryNumChanged(CusEntryNumber additionalReferenceNumber)
		{
		}

		CusEntryNumAdditionalReferenceCollection IAdditionalReferenceNumberSupporter.AdditionalReferenceNumbers
		{
			get { return Numbers; }
		}

		void IAdditionalReferenceNumberSupporter.AdditionalEntryNumberValidation(ZPropertyInfo info, ZString type, ZString number)
		{
		}

		CusEntryNumber GetCusEntryNumber(ZString numberType, RefCountry country, string category)
		{
			ZQuery query = new ZQuery(CusEntryNumSchema.CE_EntryType, numberType);
			query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, country.Code);
			query.AddToFilter(CusEntryNumSchema.CE_ParentID, PK);
			query.AddToFilter(CusEntryNumSchema.CE_Category, category);
			query.AddToFilter(CusEntryNumSchema.CE_ParentTable, TableName);

			return Factory.LoadTop1<CusEntryNumber>(query);
		}

		void CreateCusEntryNumber(ZString numberType, RefCountry country, ZString value, string category)
		{
			var number = Factory.New<CusEntryNumber>();
			number.CE_Category = category;
			number.CE_EntryNum = value;
			number.CE_EntryType = numberType;
			number.CE_ParentID = PK;
			number.CE_ParentTable = TableName;
			number.CE_RN_NKCountryCode = country.Code;
		}

		#endregion

		#region Numbers

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(CusEntryNumber.Schema.TableName, CusEntryNumber.Schema.CE_ParentID, CusEntryNumber.Schema.CE_ParentTable)]
		public CusEntryNumAdditionalReferenceCollection Numbers
		{
			get
			{
				if (fNumbers == null)
				{
					fNumbers = new CusEntryNumAdditionalReferenceCollection(this);
					fNumbers.Load();
					RegisterEditableChildObject(fNumbers);
					OnNumbersLoaded();
					RaiseNumbersLoaded();
				}
				return fNumbers;
			}
		}

		CusEntryNumAdditionalReferenceCollection fNumbers;

		public ZString NumbersAsString => Numbers.AllNumbersAsString;

		public event EventHandler NumbersLoaded;

		void RaiseNumbersLoaded()
		{
			if (NumbersLoaded != null)
			{
				NumbersLoaded(this, EventArgs.Empty);
			}
		}

		protected virtual void OnNumbersLoaded() { }

		#endregion

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			return base.GetPropertiesToExcludeFromCloning().Concat(new[]
			{
				RateOneOffShipmentSchema.Constants.TT_QuoteKPI,
				RateOneOffShipmentSchema.Constants.TT_RevisionReason
			});
		}
	}
}


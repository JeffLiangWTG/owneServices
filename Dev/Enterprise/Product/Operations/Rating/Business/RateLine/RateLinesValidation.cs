using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using Enterprise.Core;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public class ActualRateLinesValidation : RateLinesValidation
	{
		public ActualRateLinesValidation(AutoRateLines parent)
			: base(parent)
		{
		}

		new RateLine Parent
		{
			get { return (RateLine)base.Parent; }
		}

		#region TL_AC

		protected override void CheckTL_ACIsValidZGuid()
		{
			if (!Parent.ParentRateEntry.IsGlobal())
			{
				base.CheckTL_ACIsValidZGuid();
			}
		}

		protected override void CheckTL_AC()
		{
			base.CheckTL_AC();
			if (Parent.Parent != null && (!Parent.Parent.IsExpired() || (Parent.Parent.IsExpired() && (Parent.TL_ACInfo.HasChanges || !Parent.IsInDatabase))))
			{
				MultilingualString message = Parent.Parent.IsGlobal() ? ErrorMessages.InvalidGlobalChargeCode : ErrorMessages.InvalidChargeCode;
				ListValidation.ErrorIfInvalidPK(Parent.TL_ACInfo, Parent.Lookups.ChargeCodes, message);
			}

			if (!(Parent is RelatedRateLine))
			{
				if (Parent.Parent != null && (!Parent.Parent.IsExpired() || (Parent.Parent.IsExpired() && (Parent.TL_ACInfo.HasChanges || !Parent.IsInDatabase))))
				{
					ValidateActiveChargeCode();
				}

				ValidateIsJobLevelAvailableIsConsistent();
				ValidateChargeCodeGroup();
				ValidateFeesAndChargesChargeCode();
				ValidateFRTCalculatorRecursiveReference();
			}
		}

		void ValidateFeesAndChargesChargeCode()
		{
			if (Parent.Header == null || !Parent.Header.IsClientRate())
			{
				return;
			}

			if (!Parent.TL_FeeChargeType.IsEmpty && !Parent.IsFeeChargeApplicable(Parent.Parent.Parent.Header))
			{
				Parent.TL_ACInfo.AddError(ErrorMessages.FeesAndChargesChargeCodeDoesNotBelongToOrganization);
			}
		}

		void ValidateIsJobLevelAvailableIsConsistent()
		{
			var shouldCheckIsJobLevel = Parent.ChargeCode != null
				&& Parent.IsJobLevelAvailable
				&& Parent.Parent != null
				&& !Parent.Calculator.ShowEquipmentType
				&& !Parent.Calculator.ShowMessageTypeSubType;

			if (shouldCheckIsJobLevel && !Parent.Parent.IsJobLevelAvailableIsConsistent(Parent))
			{
				Parent.TL_ACInfo.AddError(Res.GetString("82bd6804-e319-4380-9cc8-57b10585d271", "Charges using the same charge code must be either all Product level, or all Job level - but not a mixture."));
			}
		}

		void ValidateActiveChargeCode()
		{
			if (Parent.ChargeCode != null && !Parent.ChargeCode.AC_IsActive)
			{
				Parent.TL_ACInfo.AddError(Res.GetString("d33e5c57-8f3d-4a40-a928-e79ddb39483e", "This charge code is marked as inactive and cannot be included."));
			}
		}

		void ValidateChargeCodeGroup()
		{
			if (Parent.ChargeCode != null && !Parent.Lookups.IsChargeGroupApplicableToRateLine(Parent.ChargeCode.AC_ChargeGroup))
			{
				Parent.TL_ACInfo.AddError(Res.GetString("e828ae4c-e66d-43c0-b47d-ed6883aad069", "Cannot add charge code as the group is not compatible with this rate line."));
			}

			if (Parent.ChargeCode != null &&
				!Parent.TL_WeightVolume.IsEmpty &&
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.UnitedStates &&
				IsUnitUSSpecific(Parent.TL_WeightVolume))
			{
				var applicableChargeCodeGroup = new[]
				{
					ChargeCodeGroupList.Codes.Brokerage,
					ChargeCodeGroupList.Codes.BrokerageOnly,
					ChargeCodeGroupList.Codes.CustomsDuty,
					ChargeCodeGroupList.Codes.OriginBrokerage,
					ChargeCodeGroupList.Codes.OriginBrokerageOnly
				};

				if (!applicableChargeCodeGroup.Contains(Parent.ChargeCode.AC_ChargeGroup.ToString(), StringComparer.OrdinalIgnoreCase))
				{
					Parent.TL_ACInfo.AddError(Res.GetString("B5D5381B-1EF8-4A9D-960E-7B7B6E48EA54", "Cannot add charge code as the group must be Brokerage/Customs Duty based on the unit defined on this rate line."));
				}
			}
		}

		void ValidateFRTCalculatorRecursiveReference()
		{
			if (Parent.RateCalculatorType != CalculatorType.FreightInclusive)
			{
				return;
			}

			if (Parent.Parent != null && Parent.Parent.HasFRTCalculatorItemsHaveSameChargeCode(Parent))
			{
				Parent.TL_ACInfo.AddError(ErrorMessages.FRTCalculatorRecursiveReference);
			}
		}

		bool IsUnitUSSpecific(string unitToCheck)
		{
			var usSpecificUnits = new[]
			{
				QuantityUnit.FD, QuantityUnit.PN, QuantityUnit.FC, QuantityUnit.DO,
				QuantityUnit.LY, QuantityUnit.N3, QuantityUnit.NA,
				QuantityUnit.AM, QuantityUnit.AS, QuantityUnit.AF, QuantityUnit.DC,
				QuantityUnit.FS, QuantityUnit.FW, QuantityUnit.NS, QuantityUnit.PS,
				QuantityUnit.TB, QuantityUnit.VE, QuantityUnit.OD, QuantityUnit.TS,
				QuantityUnit.OM, QuantityUnit.CS, QuantityUnit.DE, QuantityUnit.CL
			};

			return usSpecificUnits.Contains(unitToCheck, StringComparer.OrdinalIgnoreCase);
		}

		#endregion

		#region TL_RateDesc

		protected override void CheckTL_RateDesc()
		{
			base.CheckTL_RateDesc();
			MandatoryValidation.CheckEntered(Parent.TL_RateDescInfo);
		}

		public void ValidateOverrideChargeDescription()
		{
			ValidateCalculatedProperty(Parent.OverrideChargeDescriptionInfo);
		}

		protected virtual void CheckOverrideChargeDescription()
		{
			if (Parent.IsCosting() && Parent.OverrideChargeDescription)
			{
				Parent.OverrideChargeDescriptionInfo.AddWarning(Res.GetString("5bd363f7-4e04-4b95-af17-10e107319939", "Overridden Cost Descriptions are only visible on billing jobs when the Revenue is autorated using a cost based calculator."));
			}
		}

		#endregion

		#region TL_UnitFactor

		protected override void CheckTL_UnitFactor()
		{
			base.CheckTL_UnitFactor();

			if (!Parent.TL_UnitFactor.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.TL_UnitFactorInfo, Parent.Lookups.UnitFactors);

				switch (Parent.TL_UnitFactor.ToString())
				{
					case UnitFactorList.Codes.PacksWeight:
						// PacksWeight is only applicable when EnableWarehouseUnitFactorRatesDevelopment registry is enabled, when disable it UnitFactors won't list it
						if (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.Value)
						{
							var measureType = RatingCache.GetMeasureTypeFromUnit(Parent.TL_WeightVolume);
							if (measureType != MeasureType.Weight)
							{
								Parent.TL_UnitFactorInfo.AddError(ErrorMessages.PacksWeightUnitFactorRequiresWeightUnit);
							}

							if (!Parent.Calculator.SupportsPacksWeightUnitFactor)
							{
								Parent.TL_UnitFactorInfo.AddError(ErrorMessages.PacksWeightUnitFactorOnlySupportCMBCalculator);
							}

							if (Parent.TL_RateCalculator == CombinedCalculator.Code && Parent.Calculator.IsAccumulated)
							{
								Parent.TL_UnitFactorInfo.AddError(ErrorMessages.PacksWeightUnitFactorOnlySupportNonCumulativeBreak);
							}
						}
						break;
					case UnitFactorList.Codes.ProductLine:
						if (!Parent.Calculator.SupportsProductLineUnitFactor)
						{
							Parent.TL_UnitFactorInfo.AddError(ErrorMessages.CalculatorIsNotSupportedForProductLine);
						}
						break;
					case UnitFactorList.Codes.PackageLine:
						if (!Parent.Calculator.SupportsPackageLineUnitFactor)
						{
							Parent.TL_UnitFactorInfo.AddError(ErrorMessages.PackageLineUnitFactorSupportedCalculator);
						}
						break;
					case UnitFactorList.Codes.LoadedPackagesOnly:
						if (Parent.TL_WeightVolume != QuantityUnit.PK || Parent.TL_RateCalculator != WarehousePackCalculator.Code)
						{
							Parent.TL_UnitFactorInfo.AddError(ErrorMessages.LoadedPackagesOnlyFactorSupportedCalculator);
						}
						break;
				}

				ValidateTL_WeightVolume();
			}
		}

		#endregion

		#region TL_WeightVolume

		protected override void CheckTL_WeightVolume()
		{
			base.CheckTL_WeightVolume();

			if (!Parent.TL_WeightVolume.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.TL_WeightVolumeInfo, Parent.Lookups.WeightVolumes);

				if (!Parent.TL_OP_ProductNumber.IsEmpty && UnitHelper.IsPkgUnit(Parent.TL_WeightVolume))
				{
					var product = Parent.ProductNumber;
					if (product != null &&
						!product.UnitConverter.Convertible(product.OP_StockKeepingUnit, Parent.TL_WeightVolume))
					{
						Parent.TL_WeightVolumeInfo.AddError(Res.GetString("d9ac6cb6-5cc2-4352-93ff-2e585f076b5b", "Product {0} has no {1} definition.", Parent.ProductNumber.OP_Desc, Parent.TL_WeightVolume));
					}
				}

				if (Parent.TL_IsWhsJobLevelCharge)
				{
					if (Parent.TL_WeightVolume != Constants.PkgUnit.Unit
						&& Parent.TL_WeightVolume != Constants.PkgUnit.Pallet
						&& Parent.TL_WeightVolume != Constants.PkgUnit.Package
						&& Parent.TL_WeightVolume != QuantityUnit.LI &&
						!Constants.Weight.ContainsCode(Parent.TL_WeightVolume) &&
						!Constants.Volume.ContainsCode(Parent.TL_WeightVolume))
					{
						Parent.TL_WeightVolumeInfo.AddError(Res.GetString("1571f401-cdde-43ed-9fe1-526e5894912b", "You can only specify a unit of Weight, Volume, Unit (UNT), Package (PKG), Pallet (PLT) or Line (LI) when the Job Level indicator is enabled."));
					}
				}

				CheckTL_WeightVolume_WhsPackTypeCalculator();
			}
			else if (Parent.RequiresWeightVolume())
			{
				MandatoryValidation.CheckEntered(Parent.TL_WeightVolumeInfo);
			}

			ValidateTimeCalculator();
			ValidateTL_IsWhsJobLevelCharge();
		}

		void CheckTL_WeightVolume_WhsPackTypeCalculator()
		{
			if (Parent.RateCalculatorType == CalculatorType.WarehousePack)
			{
				if (Parent.TL_WeightVolume != Constants.PkgUnit.Unit && Parent.TL_WeightVolume != QuantityUnit.PK)
				{
					Parent.TL_WeightVolumeInfo.AddError(Res.GetString("7FDC4BF9-07D3-4212-BD8A-697B3C29E38A", "You can only use Unit (UNT) or Package (PK) when Warehouse Pack Type Calculator is used."));
				}
				else if (Parent.TL_UnitFactor == UnitFactorList.Codes.LoadedPackagesOnly && Parent.TL_WeightVolume != QuantityUnit.PK)
				{
					Parent.TL_WeightVolumeInfo.AddError(Res.GetString("5e717eb2-eb85-4c0e-b796-e2e63fea5137", "You can only use Package (PK) when Loaded Packages Only (LPO) Unit Factor is used."));
				}
			}
		}

		void ValidateTimeCalculator()
		{
			if (Parent.RateCalculatorType == CalculatorType.Time)
			{
				foreach (RateLineItem item in Parent.RateLineItems)
				{
					if (Parent.TL_WeightVolume == item.TM_BreakWeightVolume)
					{
						Parent.TL_WeightVolumeInfo.AddError(Res.GetString("4ca97c38-fd0f-47c7-9307-ae4c651039b0",
							"You should not use the Time Calculator to specify per hour/day/week rates - simply use the Unit calculator."));
						break;
					}
				}
			}
		}

		#endregion

		#region TL_RX_NKCurrency

		protected override void CheckTL_RX_NKCurrency()
		{
			base.CheckTL_RX_NKCurrency();
			if (Parent.TL_RX_NKCurrency.IsEmpty)
			{
				if (Parent.TL_RateCalculator != FreightInclusiveCalculator.Code)
				{
					MandatoryValidation.CheckEntered(Parent.TL_RX_NKCurrencyInfo);
				}
			}
			else
			{
				ListValidation.ErrorIfInvalidCode(Parent.TL_RX_NKCurrencyInfo, Parent.Lookups.Currencies);
				ValidateChargeCodeCurrency();
			}
		}

		void ValidateChargeCodeCurrency()
		{
			if (Parent.Parent != null && Parent.Parent.HasSameChargeCodeWithOverlappingCurrencies(Parent))
			{
				Parent.TL_RX_NKCurrencyInfo.AddError(Res.GetString("28b13fb4-8175-4d1c-8c33-c5b0da500c51", "Rate lines with same charge code and different currencies should not have overlapping dates."));
			}
		}

		#endregion

		#region TL_RateCalculator

		protected override void CheckTL_RateCalculator()
		{
			base.CheckTL_RateCalculator();
			ValidateTL_WeightVolume();

			MandatoryValidation.CheckEntered(Parent.TL_RateCalculatorInfo);
			ListValidation.ErrorIfInvalidCode(Parent.TL_RateCalculatorInfo, Parent.Lookups.RateCalculators);

			ValidateAgencyCalcDisallowedOnFreightEntry();
			ValidateCartageCalcDisallowedOnSeaOrAllEntry();
			ValidateCartageZoneCalcDisallowedOnFreightEntry();
			ValidateCalculatorsForCostings();
			ValidateIDependentCalculator();
			ValidateTariffBasedCalcDisallowedOnBaseTariffOrCosting();
			ValidateTariffBasedCalcHasWarningIfTariffLevelEqualsZero();
			ValidateIfNonCartageCalcOnCartageCharge();
			ValidateWarehouseLocationTypeCalcOnlyAllowedOnWarehouseStorageCharges();
			ValidateFreightInclusiveCalculatorOnFreightCharge();
			ValidateEqualizationCalculatorAllowedOnCostingForwardingCharges();
			ValidateExcludeCompanyTariffsCalculatorAllowedOnClientRate();
			ValidateCalculatorsSupportedOnFCLRateEntryWithEmptyContainer();
		}

		void ValidateAgencyCalcDisallowedOnFreightEntry()
		{
			if (Parent.Parent != null && Parent.Parent.IsFreightEntry() && Parent.Uses(CalculatorType.Agency))
			{
				Parent.TL_RateCalculatorInfo.AddError(ErrorMessages.AgencyChargeOnFreightEntry);
			}
		}

		void ValidateWarehouseLocationTypeCalcOnlyAllowedOnWarehouseStorageCharges()
		{
			if (Parent.Uses(CalculatorType.WarehouseLocationType) &&
				Parent.ChargeCode != null && Parent.ChargeCode.AC_ChargeGroup != ChargeCodeGroupList.Codes.WHSStorage)
			{
				Parent.TL_RateCalculatorInfo.AddError(ErrorMessages.WarehouseLocationTypeCalcOnNonWarehouseStorageCharge);
			}
		}

		void ValidateCartageZoneCalcDisallowedOnFreightEntry()
		{
			if (Parent.Parent != null && Parent.Parent.IsFreightEntry() && Parent.Uses(CalculatorType.CartageZoneDistance))
			{
				Parent.TL_RateCalculatorInfo.AddError(ErrorMessages.CartageZoneChargeOnFreightEntry);
			}
		}

		void ValidateCartageCalcDisallowedOnSeaOrAllEntry()
		{
			if (IsCartageOrCartageZoneCalculatorDisallowedDueToMode && (Parent.Uses(CalculatorType.Cartage) || Parent.Uses(CalculatorType.CartageZoneDistance)))
			{
				Parent.TL_RateCalculatorInfo.AddError(ErrorMessages.CartageCalculatorNotAllowedOnSeaOrAll);
			}
		}

		bool IsCartageOrCartageZoneCalculatorDisallowedDueToMode
		{
			get
			{
				var entry = Parent.Parent;
				if (entry == null || entry.IsFreightEntry() || entry.IsWHS() || entry.IsTRW() || entry.IsTWU() || entry.IsAir())
				{
					return false;
				}

				if (entry.TI_RateCategory == RatingConstants.RateCategory.CST)
				{
					return entry.TI_Mode == Core.Constants.RateMode.ALL;
				}

				return
					entry.TI_Mode == Core.Constants.RateMode.ALL
					|| entry.TI_Mode == Core.Constants.RateMode.AIR
					|| entry.TI_Mode == Core.Constants.RateMode.SEA
					|| entry.TI_Mode == Core.Constants.RateMode.ROA
					|| entry.TI_Mode == Core.Constants.RateMode.RAI;
			}
		}

		void ValidateCalculatorsForCostings()
		{
			if (Parent.Parent != null && Parent.Parent.IsCosting())
			{
				if (Parent.UsesCostBasedCalculator())
				{
					if (Parent.Parent.Parent.IsStandardCostRate())
					{
						Parent.TL_RateCalculatorInfo.AddError(ErrorMessages.CostBasedCalculatorNotAllowed);
					}
					else
					{
						Parent.TL_RateCalculatorInfo.AddWarning(ErrorMessages.CostBasedCalculatorWarning);
					}
				}
				else if (Parent.Uses(CalculatorType.Note))
				{
					Parent.TL_RateCalculatorInfo.AddError(ErrorMessages.NoteCalculatorOnCosting);
				}
				else if (Parent.Uses(CalculatorType.ProfitShareRebate))
				{
					Parent.TL_RateCalculatorInfo.AddError(ErrorMessages.ProfitShareRebateCalculatorNotAllowedOnCosting);
				}
			}
		}

		void ValidateTariffBasedCalcDisallowedOnBaseTariffOrCosting()
		{
			if (Parent.IsCalculatorInitialized && Parent.UsesCompanyTariffBasedCalculator() && Parent.Parent != null && (Parent.Parent.IsCosting() || (Parent.Parent.IsCompanyTariff() && Parent.TL_CompanyTariffLevel <= 1)))
			{
				Parent.TL_RateCalculatorInfo.AddError(ErrorMessages.CompanyTariffBasedCalculatorNotAllowed);
			}
		}

		void ValidateTariffBasedCalcHasWarningIfTariffLevelEqualsZero()
		{
			if (Parent.IsCalculatorInitialized &&
				Parent.UsesCompanyTariffBasedCalculator() &&
				Parent.Parent != null && Parent.Parent.GetCompanyTariffLevels().All(level => level == 0) &&
				Parent.Parent.Parent != null && Parent.Parent.Parent.Header != null)
			{
				if (Parent.Parent.IsClientRate() || Parent.Parent.IsQuote())
				{
					Parent.TL_RateCalculatorInfo.AddWarning(ErrorMessages.CompanyTariffBasedCalculatorAppliesToBaseCompanyTariff);
				}
			}
		}

		void ValidateIfNonCartageCalcOnCartageCharge()
		{
			if (Parent.ChargeCode != null &&
				Parent.IsCartageCalculatorCode(Parent.ChargeCode.AC_RateCalculator) &&
				!Parent.IsCartageCalculatorCode(Parent.TL_RateCalculator) &&
				!Parent.UsesCompanyTariffOrCostBasedCalculator())
			{
				Parent.TL_RateCalculatorInfo.AddWarning(ErrorMessages.NonCartageCalcOnCartageCharge);
			}
		}

		void ValidateEqualizationCalculatorAllowedOnCostingForwardingCharges()
		{
			if (Parent != null && Parent.Parent != null && Parent.Uses(CalculatorType.Equalization)
				&& (!Parent.IsCosting() || (!Parent.Parent.IsAir() && !(Parent.Parent.IsFCL() && Parent.Parent.IsForwarding()))))
			{
				Parent.TL_RateCalculatorInfo.AddError(ErrorMessages.EqualisationCalculatorNotAllowed);
			}
		}

		void ValidateFreightInclusiveCalculatorOnFreightCharge()
		{
			if (Parent.Parent != null && Parent.TL_AC == Env.Registry.FreightChargeCode && Parent.Uses(CalculatorType.FreightInclusive))
			{
				Parent.TL_RateCalculatorInfo.AddError(ErrorMessages.FRTCalculatorOnFreightCharge);
			}
		}

		void ValidateExcludeCompanyTariffsCalculatorAllowedOnClientRate()
		{
			if (Parent.Parent != null && Parent.Uses(CalculatorType.ExcludeCompanyTariffs) && !Parent.Parent.IsClientRate())
			{
				Parent.TL_RateCalculatorInfo.AddError(ErrorMessages.ExcludeCompanyTariffsCalculatorNotAllowed);
			}
		}

		void ValidateCalculatorsSupportedOnFCLRateEntryWithEmptyContainer()
		{
			if (!Parent.IsBulkRateUpdateActionLine)
			{
				var entry = Parent.Parent;
				if (entry != null && entry.IsFCLEntryWithEmptyContainer() && !Parent.UsesCalculatorsSupportedOnFCLRateEntryWithEmptyContainer())
				{
					Parent.TL_RateCalculatorInfo.AddError(ErrorMessages.CalculatorsSupportedOnFCLRateEntryWithEmptyContainer);
				}
			}
		}

		#region ValidateIDependentCalculator

		void ValidateIDependentCalculator()
		{
			RateLine master = Parent;
			ClearRowNotificationsIncludingChildren(master);

			if (!(master.Calculator is IDependentCalculator))
			{
				return;
			}

			var items = master.RateLineItems.Cast<RateLineItem>().ToArray();
			ValidateDoubleSEQ(items);

			if (items.All(item => !item.RateOperatorIsApplyToOrMNT()))
			{
				master.AddRowError(ErrorMessages.AtLeastOneChargeType);
			}
			else
			{
				ValidateDependencies(master, items);
				ValidateDoubleChargeCodesOrGroupsOnPercentageCalculator(master);
			}
		}

		static void ClearRowNotificationsIncludingChildren(RateLine master)
		{
			master.ClearRowNotifications();
			foreach (RateLineItem item in master.RateLineItems)
			{
				item.ClearRowNotifications();
			}
		}

		static void ValidateDependencies(RateLine master, RateLineItem[] items)
		{
			var resolver = new CalculationOrderResolver(master.Parent.ChildRateLines);

			List<IRateLine> conflicts;
			if (resolver.TryGetConflicts(master, out conflicts))
			{
				foreach (var item in items.Where(item => item.RateOperatorIsApplyToOrMNT()))
				{
					foreach (var line in conflicts.Where(line => resolver.IsItemApplicableToLine(item, line)))
					{
						item.AddRowWarning(Res.GetString("10896892-04ab-4e13-a5eb-e54337745ad0", "This item is in conflict with {0} charge. Use Calculation Order if you need to resolve it.", line.ChargeCode.AC_Code));
					}
				}
			}
		}

		static void ValidateDoubleChargeCodesOrGroupsOnPercentageCalculator(RateLine master)
		{
			for (var i = 0; i < master.RateLineItems.Count; i++)
			{
				for (var j = 0; j < master.RateLineItems.Count; j++)
				{
					if (i != j && master.RateLineItems[j].RateOperatorIsApplyToOrMNT())
					{
						if (master.RateLineItems[i].TM_Text == master.RateLineItems[j].TM_Text)
						{
							if (master.RateLineItems[i].TM_Text != CalculatorConstants.Text.ChargeCode)
							{
								master.RateLineItems[i].AddRowError(ErrorMessages.DoubledChargeTypes);
								break;
							}

							if (master.RateLineItems[i].TM_AC == master.RateLineItems[j].TM_AC)
							{
								master.RateLineItems[i].AddRowError(ErrorMessages.DoubledChargeCodes);
								break;
							}
						}
					}
				}
			}
		}

		static void ValidateDoubleSEQ(RateLineItem[] items)
		{
			var sequenceItems = items.Where(x => x.RateOperatorIsSequenceItem()).ToArray();
			if (sequenceItems.Length >= 2)
			{
				foreach (var item in sequenceItems)
				{
					item.AddRowError(ErrorMessages.DoubledChargeTypes);
				}
			}
		}

		#endregion

		#endregion

		#region UnitMultipleAsString

		public void ValidateUnitMultipleAsString()
		{
			ValidateCalculatedProperty(Parent.UnitMultipleAsStringInfo);
		}

		protected void CheckUnitMultipleAsString()
		{
			if (Parent.ShowUnitMultipleAsStringInfoParsingError)
			{
				Parent.UnitMultipleAsStringInfo.AddError(Res.GetString("8f2ab60e-7db1-4086-b726-a5f88c8c613a", "Units multiple must be a decimal."));
			}
			else
			{
				Parent.UnitMultipleAsStringInfo.AddAllNotificationsFrom(Parent.TL_WeightVolumeMultipleInfo);
			}
		}

		#endregion

		#region TL_Rounding

		protected override void CheckTL_Rounding()
		{
			base.CheckTL_Rounding();

			if (Parent.TL_Rounding.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.TL_RoundingInfo);
			}
			else
			{
				ListValidation.ErrorIfInvalidCode(Parent.TL_RoundingInfo, Parent.Lookups.Roundings);
			}
		}

		#endregion

		#region TL_RoundingFactor

		protected override void CheckTL_RoundingFactor()
		{
			base.CheckTL_RoundingFactor();

			if (Parent.TL_Rounding == RatingRoundingTypes.Custom)
			{
				if (Parent.TL_RoundingFactor.IsEmpty)
				{
					MandatoryValidation.CheckEntered(Parent.TL_RoundingFactorInfo);
				}
				else
				{
					MandatoryValidation.CheckNotNegative(Parent.TL_RoundingFactorInfo);
					MandatoryValidation.CheckNotZero(Parent.TL_RoundingFactorInfo);
				}
			}
		}

		#endregion

		#region TL_OP_ProductNumber

		protected override void CheckTL_ParentID()
		{
			base.CheckTL_ParentID();

			ValidateTL_AC();

			if (Parent.TL_ParentTableCode == OrgSupplierPartSchema.Constants.Prefix &&
				Parent.Parent != null &&
				Parent.Parent.CommodityCode != null &&
				Parent.ProductNumber != null &&
				Parent.ProductNumber.CommodityCode != null &&
				Parent.Parent.CommodityCode.PK != Parent.ProductNumber.CommodityCode.PK)
			{
				Parent.TL_OP_ProductNumberInfo.AddError(Res.GetString("93c15785-efb2-44b4-b7e3-396ec66b13bb", "The product you have entered has a conflicting commodity code to that entered on the trade lane. Please rectify this."));
			}

			ListValidation.ErrorIfInvalidPK(Parent.TL_ParentIDInfo, Parent.Lookups.ProductNumbers, ErrorMessages.InapplicableProductNumber);
		}

		#endregion

		#region TL_IsOnPallets

		protected override void CheckTL_IsOnPallets()
		{
			base.CheckTL_IsOnPallets();

			ValidateTL_AC();
		}

		#endregion

		#region TL_IsWhsJobLevelCharge

		protected override void CheckTL_IsWhsJobLevelCharge()
		{
			base.CheckTL_IsWhsJobLevelCharge();

			if (!Parent.TL_IsWhsJobLevelCharge)
			{
				if (Parent.TL_WeightVolume == RatingConstants.Units.LI)
				{
					Parent.TL_IsWhsJobLevelChargeInfo.AddError(Res.GetString("5a032295-5600-458e-b859-3370a99fc00a", "Unit type of Lines (LI) can only be used when rating at the Job level and cannot be rated at the Product level."));
				}
			}
			ValidateTL_WeightVolume();
			ValidateTL_AC();
		}

		#endregion

		#region TL_ActualPercentage

		protected override void CheckTL_ActualPercentage()
		{
			base.CheckTL_ActualPercentage();

			if (Parent.TL_ActualPercentage < 0 || Parent.TL_ActualPercentage > 100)
			{
				Parent.TL_ActualPercentageInfo.AddError(Res.GetString("c090f942-b01e-4d01-b3de-9c9835413df8", "Please enter a valid percent."));
			}
		}

		#endregion

		#region CheckUseOnlyActualWeightMeasure

		protected override void CheckUseOnlyActualWeightMeasure()
		{
			if (!Parent.UseOnlyActualWeightMeasure)
			{
				var chargeCode = Parent.ChargeCode;
				if (chargeCode != null)
				{
					if (chargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.WHSStorage)
					{
						Parent.UseOnlyActualWeightMeasureInfo.AddWarning(Res.GetString("5d027e41-c635-4169-89a1-4e67ccacd897",
							"If storage rating units are volume or weight, unchecking the 'Actual' flag will cause the chargeable volumetric weight calculation to be applied using parameters defined in the system registry at {0}. The greater of actual or calculated units will then be used as the basis for charging.",
							((IRegistryItemInternals)WarehouseDataRegistry.Instance.WarehouseChargeableFactorStorage).Location));
					}
					else if (chargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.WHSInwards || chargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.WHSOutwards)
					{
						Parent.UseOnlyActualWeightMeasureInfo.AddWarning(Res.GetString("4ac994d8-3c1c-4f1e-a66a-bceddf383dac",
							"If storage rating units are volume or weight, unchecking the 'Actual' flag will cause the chargeable volumetric weight calculation to be applied using parameters defined in the system registry at {0}. The greater of actual or calculated units will then be used as the basis for charging.",
							((IRegistryItemInternals)WarehouseDataRegistry.Instance.WarehouseChargeableFactorHandling).Location));
					}
					else if (chargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.TRWReceiveTransportationUnit || chargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.TRWDispatchTransportationUnit)
					{
						Parent.UseOnlyActualWeightMeasureInfo.AddWarning(Res.GetString("8e72f930-f8f5-4c71-b2fa-98c16b939f8f",
								@"If rating units are volume or weight, unchecking the 'Actual' flag will cause the chargeable volumetric weight calculation to be applied using parameters defined in the following system registries. The greater of actual or calculated units will then be used as the basis for charging.
{0}
{1}
{2}",
								((IRegistryItemInternals)WarehouseDataRegistry.Instance.TransitChargeableFactorForAir).Location,
								((IRegistryItemInternals)WarehouseDataRegistry.Instance.TransitChargeableFactorForSea).Location,
								((IRegistryItemInternals)WarehouseDataRegistry.Instance.TransitChargeableFactorForRoad).Location));
					}
				}
			}
		}

		#endregion

		#region ViewResults

		public void ValidateViewResults()
		{
			ValidateCalculatedProperty(Parent.ViewResultsInfo);
		}

		protected void CheckViewResults()
		{
			if (Parent.ShowViewResultsWarning)
			{
				Parent.ViewResultsInfo.AddWarning(Res.GetString("326e9c5c-fbac-4891-ad69-f651b0f2faf2", "No relevant company tariff or costing was found, however Autorating may still be able to find an appropriate costing to use."));
			}
		}

		#endregion

		#region TL_ConditionalExpression

		protected override void CheckTL_ConditionalExpression()
		{
			var conditionalExpression = Parent.TL_ConditionalExpression;

			if (!conditionalExpression.IsEmpty && Parent.TL_Condition != RateLineConditions.UserDefined)
			{
				Parent.TL_ConditionalExpressionInfo.AddError(Res.GetString("31a24581-bb04-42c3-9bf5-251c25bbdd3d", "User defined expression can only be set when TL_Condition is set to {0} mode", RateLineConditions.UserDefined));
				return;
			}

			var expression = conditionalExpression
				.ToString()
				.With<StandardLibrary>()
				.CreateExpression();

			var result = expression.Evaluate();
			if (result != null)
			{
				return;
			}

			if (!conditionalExpression.IsEmpty && !ZExpressionEvaluator.IsInnermostRegexMatch(conditionalExpression) && !ZExpressionEvaluator.IsValidFilter(conditionalExpression, false, out var message))
			{
				var expressionMessage = Res.GetString("27099468-6f47-4238-b76a-98cd9ebfbefc", "Alternatively you can create expressions using predefined macros.");
				Parent.TL_ConditionalExpressionInfo.AddError(message + System.Environment.NewLine + expressionMessage);
			}
			else if (conditionalExpression.IsEmpty && Parent.TL_Condition == RateLineConditions.UserDefined)
			{
				Parent.TL_ConditionalExpressionInfo.AddError(Res.GetString("bf4c61e5-0efc-40b3-9f7b-99454c33ce81", "Please enter Expression"));
			}
		}

		#endregion

		#region TL_ContainerOwnership

		protected override void CheckTL_ContainerOwnership()
		{
			base.CheckTL_ContainerOwnership();
			ListValidation.ErrorIfInvalidCode(Parent.TL_ContainerOwnershipInfo, Parent.Lookups.ContainerOwnerships);
		}

		#endregion

		#region TL_Condition

		protected override void CheckTL_Condition()
		{
			base.CheckTL_Condition();
			ListValidation.ErrorIfInvalidCode(Parent.TL_ConditionInfo, Parent.Lookups.RateLineConditions);

			if (Parent.ChargeCode != null &&
				Parent.Calculator != null &&
				Parent.IsCartageCalculatorCode(Parent.TL_RateCalculator) &&
				Parent.Calculator.HasDuplicateEquipmentTypes)
			{
				Parent.TL_ConditionInfo.AddError(ErrorMessages.EquipmentTypeDuplicate);
			}
		}

		#endregion

		#region Fees and Charges

		protected override void CheckTL_FeeChargeType()
		{
			base.CheckTL_FeeChargeType();

			if (!Parent.TL_FeeChargeTypeInfo.ReadOnly)
			{
				ListValidation.ErrorIfInvalidCode(Parent.TL_FeeChargeTypeInfo);

				if (!Parent.TL_FeeChargeType.IsEmpty && Parent.TL_CompanyTariffLevel != 1 && !Parent.IsCosting())
				{
					var message = ResString.GetMultilingualString("ce019214-c237-4233-862a-a62dd8bb75a6", "Fees and Charges Type on a Rate Line unless the line belongs to a Costing or Company Tariff Level 1");
					MandatoryValidation.CheckNotEntered(Parent.TL_FeeChargeTypeInfo, message);
				}
			}
		}

		protected override void CheckTL_FeeChargeLevel()
		{
			base.CheckTL_FeeChargeLevel();

			if (!Parent.TL_FeeChargeLevelInfo.ReadOnly)
			{
				ListValidation.ErrorIfInvalidCode(Parent.TL_FeeChargeLevelInfo);

				if (!Parent.TL_FeeChargeType.IsEmpty)
				{
					MandatoryValidation.CheckEntered(Parent.TL_FeeChargeLevelInfo);
				}
			}
		}

		#endregion

		#region Start/End date

		protected override void CheckTL_RateStartDate()
		{
			var startDate = Parent.TL_RateStartDate;
			if (!startDate.IsEmpty)
			{
				var endDate = Parent.TL_RateEndDate;
				if (!endDate.IsEmpty && startDate > endDate)
				{
					Parent.TL_RateStartDateInfo.AddError(Res.GetString("F3D201C9-9F02-4A0E-81CB-36C3AB6B1A6A", "You cannot have a Start Date that is after the Expiry Date."));
				}
				else
				{
					var entryStartDate = Parent.Parent.TI_RateStartDate;
					if (entryStartDate.IsValid && startDate < entryStartDate)
					{
						Parent.TL_RateStartDateInfo.AddError(Res.GetString("9CCE0894-DBAD-467E-942C-8BECEEC43619", "The Start Date of Rate Line must be the same day or later than the Start Date of Rate Entry."));
					}
					else
					{
						CompareValidation.CheckDateIsNotAfterAnotherDate(Parent.TL_RateStartDateInfo, Parent.Parent.TI_RateEndDateInfo);
					}
				}
			}

			ValidateTL_RX_NKCurrency();
		}

		protected override void CheckTL_RateEndDate()
		{
			var endDate = Parent.TL_RateEndDate;
			if (!endDate.IsEmpty)
			{
				var startDate = Parent.TL_RateStartDate;
				if (!startDate.IsEmpty && startDate > endDate)
				{
					Parent.TL_RateEndDateInfo.AddError(Res.GetString("6F1E1AFB-B977-4EDF-A9C5-1F8B7F40F5FE", "You cannot have an Expiry Date that is before the Start Date."));
				}
				else
				{
					var entryEndDate = Parent.Parent.TI_RateEndDate;
					if (entryEndDate.IsValid && endDate > entryEndDate)
					{
						Parent.TL_RateEndDateInfo.AddError(Res.GetString("7DD166BC-2E94-4381-8851-58F6B38F4E06", "The Expiry Date of Rate Line must be the same day or earlier than the Expiry Date of Rate Entry."));
					}
					else
					{
						CompareValidation.CheckDateIsNotBeforeAnotherDate(Parent.TL_RateEndDateInfo, Parent.Parent.TI_RateStartDate);
					}
				}
			}

			ValidateTL_RX_NKCurrency();
		}

		#endregion

		#region Implementation

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateUnitMultipleAsString();
			ValidateUseOnlyActualWeightMeasure();
		}

		#endregion
	}
}


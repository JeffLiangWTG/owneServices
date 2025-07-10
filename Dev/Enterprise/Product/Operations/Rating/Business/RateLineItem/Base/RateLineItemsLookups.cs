using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	public class RateLineItemsLookups : AutoRateLineItemsLookups
	{
		public RateLineItemsLookups(AutoRateLineItems parent)
			: base(parent)
		{
			RateLineItem = (IRateLineItem)parent;
			Factory = parent.Factory;
		}

		public RateLineItemsLookups(IRateLineItem parent, BusinessObjectFactory factory) : base(null)
		{
			RateLineItem = parent;
			Factory = factory;
		}

		protected override BusinessObjectFactory Factory { get; }

		IRateLineItem RateLineItem { get; }

		#region Weight Breaks

		public CodeDescriptionPairList WeightBreaks => RateLineItem.ParentRateLine?.Lookups().WeightBreaks;

		#endregion

		#region Time Units

		public CodeDescriptionPairList TimeUnits
		{
			get
			{
				if (RateLineItem.ParentRateLine?.ParentRateEntry == null)
				{
					return new CodeDescriptionPairList();
				}

				var cacheKey = string.Format("{0}|{1}", "TimeUnits", RateLineItem.ParentRateLine.ParentRateEntry.RateType());

				return Factory.GetCachedValue(cacheKey, delegate
				{
					var result = new CodeDescriptionPairList();

					foreach (ICodeDescription pair in RateLineItem.ParentRateLine.Lookups().WeightVolumes)
					{
						if (RatingConstants.Units.IsTime(pair.Code))
						{
							result.AddPair(pair.Code, pair.Description);
						}
					}

					return result;
				});
			}
		}

		#endregion

		#region Housebill Release Types

		public const string StandardReleaseType = "STD";

		public CodeDescriptionPairList HousebillReleaseTypes
		{
			get
			{
				return Factory.GetCachedValue("HousebillReleaseTypes", delegate
				{
					var result = new CodeDescriptionPairList(OLookUpEditType.ShipmentReleaseType);
					result.AddPair(StandardReleaseType, Res.GetString("eeac00d2-b9b4-44aa-b95b-ae0d07ae7ec9", "Standard Release Type"));
					return result;
				});
			}
		}

		#endregion

		#region Warehouse Packages Types

		public RefPackTypeCollection WarehousePackagesTypes
		{
			get { return new RefPackTypeCollection(Factory); }
		}

		#endregion

		#region Warehouse Location Types

		public IWhsLocationTypeCollection WarehouseLocationTypes
		{
			get
			{
				return Factory.GetCachedValue("WarehouseLocationTypes", () =>
					(IWhsLocationTypeCollection)Activator.CreateInstance(ObjectFactory.GetType<IWhsLocationTypeCollection>(), Factory));
			}
		}

		#endregion

		#region Calculation Order Types

		public CodeDescriptionPairList CalculationOrderList
		{
			get
			{
				return Factory.GetCachedValue("CalculationOrderList", delegate
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(CompanyTariffOrCostBasedCalculator.Items.PercentFirst, CompanyTariffOrCostBasedCalculator.Items.PercentFirstDescription);
					result.AddPair(CompanyTariffOrCostBasedCalculator.Items.IncreaseFirst, CompanyTariffOrCostBasedCalculator.Items.IncreaseFirstDescription);
					return result;
				});
			}
		}

		#endregion

		#region Percentage Apply To List

		public ValueApplyToListCodeDescriptionPairList PercentageApplyToList
		{
			get
			{
				var rateEntry = RateLineItem.ParentRateLine?.ParentRateEntry;
				var rateCategory = Groups.GetGroupNameByCategory(rateEntry?.TI_RateCategory);
				if (rateEntry != null
					&& (rateEntry.IsClientRate() || rateEntry.IsCompanyTariff() || rateEntry.IsCosting() || rateEntry.IsQuote())
					&& (rateCategory == Groups.Forwarding || rateCategory == Groups.LinerAndAgency))
				{
					return Factory.GetCachedValue("PercentageApplyToListWithLoadingAndCustomsBrokerageCharges", () => ValueApplyToListCodeDescriptionPairList.NewPercentageApplyToList(RateLineItem.ParentRateLine?.Country()?.PK ?? GlbCompany.CurrentCompany.Country.PK, true));
				}
				return Factory.GetCachedValue("PercentageApplyToList", () => ValueApplyToListCodeDescriptionPairList.NewPercentageApplyToList(RateLineItem.ParentRateLine?.Country()?.PK ?? GlbCompany.CurrentCompany.Country.PK));
			}
		}

		#endregion

		#region Profit Share / Rebate Apply To List

		public CodeDescriptionPairList ProfitShareRebateApplyToList
		{
			get { return Factory.GetCachedValue("ProfitShareRebateApplyToList", GetProfitShareRebateApplyToList); }
		}

		CodeDescriptionPairList GetProfitShareRebateApplyToList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(CalculatorConstants.Text.ChargeCode, Calculator.Items.Value.ChargesApplyToTypesDescription.ChargeCodeDescription);
			result.AddPair(CalculatorConstants.Text.AllCharges, Calculator.Items.Value.ChargesApplyToTypesDescription.AllChargesDescription);
			result.AddPair(CalculatorConstants.Text.FreightCharges, Calculator.Items.Value.ChargesApplyToTypesDescription.FreightChargesDescription);
			result.AddPair(CalculatorConstants.Text.OriginCharges, Calculator.Items.Value.ChargesApplyToTypesDescription.OriginChargesDescription);
			result.AddPair(CalculatorConstants.Text.DestinationCharges, Calculator.Items.Value.ChargesApplyToTypesDescription.DestinationChargesDescription);

			return result;
		}

		#endregion

		#region Units

		public CodeDescriptionPairList WeightVolumes
		{
			get
			{
				var result = new CodeDescriptionPairList();

				var rateEntry = RateLineItem.ParentRateLine?.ParentRateEntry;
				if (rateEntry == null)
				{
					return result;
				}

				var chargeableUnit = RateLineItem.ParentRateLine.TL_WeightVolume;
				if (UnitHelper.IsTopPack(chargeableUnit, Factory) || RateLineItem.ParentRateLine.Calculator is HighestRateCalculator)
				{
					var withoutTeu = rateEntry.IsAir() || rateEntry.IsSea() && rateEntry.TI_Mode == Core.Constants.RateMode.LCL;
					var teuSpecificCacheKey = withoutTeu ? "WithoutTeu" : string.Empty;

					return Factory.GetCachedValue("RateLineItemsLookups.WeightVolumesRestricted" + teuSpecificCacheKey, () =>
					{
						result.AddRange(Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight));
						result.AddRange(Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume));

						// PackageLine UnitFactor does not support PK and TU because Warehouse does not have MeasureInfo for ContainerInfoPackages(PK) and TEU(TU)
						if (RateLineItem.ParentRateLine.TL_UnitFactor != UnitFactorList.Codes.PackageLine)
						{
							result.AddPair(QuantityUnit.PK, QuantityUnit.GetDescription(QuantityUnit.PK, rateEntry.RateType()));

							if (!withoutTeu)
							{
								result.AddPair(QuantityUnit.TU, QuantityUnit.GetDescription(QuantityUnit.TU, rateEntry.RateType()));
							}
						}

						return result;
					});
				}

				if (!chargeableUnit.IsEmpty && !QuantityUnit.IsDistance(chargeableUnit) && !rateEntry.IsFreightEntry())
				{
					return Factory.GetCachedValue("RateLineItemsLookups.DistanceUnits", GetDistanceUnits);
				}

				return result;
			}
		}

		CodeDescriptionPairList GetDistanceUnits()
		{
			var lengthUnits = Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Length)
				.Cast<CodeDescriptionPair>()
				.Where(length => QuantityUnit.IsDistance(length.Code));

			var result = new CodeDescriptionPairList();
			foreach (var length in lengthUnits)
			{
				result.Add(length);
			}

			return result;
		}

		#endregion

		#region UnitMultiples

		public CodeDescriptionPairList UnitMultiples
		{
			get { return Factory.GetCachedValue("RateLineItemsLookups.UnitMultiples", delegate { return new UnitHelper().GetUnitMultiple(Factory); }); }
		}

		#endregion

		#region EquipmentTypes

		public CodeDescriptionPairList EquipmentTypes
		{
			get
			{
				return Factory.GetCachedValue("EquipmentTypes", delegate
				{
					var result = new CodeDescriptionPairList();
					var containers = new RefContainerCollection(Factory, RefContainerLookups.ShippingModes.Road);
					foreach (var container in containers)
					{
						result.AddPair(container.RC_Code, container.RC_DescriptionMultilingual);
					}
					return result;
				});
			}
		}

		#endregion

		#region PercentOfs

		#region SuppressResourceStringsCheckRegion

		public override AccChargeCodeCollection ChargeCodes
		{
			get { return Factory.GetCachedValue("ChargeCodes." + RateLineItem.IsGlobal(), GetChargeCodeCollection); }
		}

		AccChargeCodeCollection GetChargeCodeCollection()
		{
			var collection = RateLineItem.IsGlobal()
				? new GlobalChargeCodesCollection(Factory, new ZQuery(), Guid.Empty)
				: new AccChargeCodeCollection(Factory, new ZQuery(), Env.CurrentCompanyPK);

			return collection;
		}

		#endregion

		#endregion
	}
}


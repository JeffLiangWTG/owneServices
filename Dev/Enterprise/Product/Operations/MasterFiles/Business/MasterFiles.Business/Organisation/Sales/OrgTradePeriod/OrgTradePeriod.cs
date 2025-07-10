using System;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MarketingManager.Integration;
using Enterprise.ZArchitecture;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgTradePeriod : AutoOrgTradePeriod
	{
		public OrgTradePeriod(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		OrgSales Sales => TradeDetail?.Sales;
		public bool ShouldDefaultProperties => !PAS_IsTraded && (TradeDetail?.ShouldDefaultProperties ?? false);
		ZString SalesProductCode => TradeDetail?.SalesProductCode ?? ZString.Empty;
		bool IsFreight => TradeDetail?.IsFreight ?? false;
		bool IsSeaFcl => TradeDetail?.IsSeaFcl ?? false;

		#region PAS_RX_NKCurrency

		[List("Lookups.Currencies")]
		public override ZString PAS_RX_NKCurrency
		{
			get { return base.PAS_RX_NKCurrency; }
			set
			{
				if (value != base.PAS_RX_NKCurrency)
				{
					base.PAS_RX_NKCurrency = value;

					if (PAS_Period.IsEmpty)
					{
						var tradeDetail = TradeDetail;
						if (tradeDetail != null)
						{
							foreach (var period in tradeDetail.ProspectPeriods)
							{
								period.PAS_RX_NKCurrency = PAS_RX_NKCurrency;
							}
						}
					}
				}
			}
		}

		#endregion

		#region PAS_RateOffered

		public override ZDecimal PAS_RateOffered
		{
			get { return base.PAS_RateOffered; }
			set
			{
				if (base.PAS_RateOffered != value)
				{
					base.PAS_RateOffered = value;

					if (ShouldDefaultProperties)
					{
						RecalculateEstimatedRevenueIfProspect();
					}
				}
			}
		}

		#endregion

		#region PA_Units

		public override ZLong PAS_Units
		{
			get { return base.PAS_Units; }
			set
			{
				if (base.PAS_Units != value)
				{
					base.PAS_Units = value;

					if (ShouldDefaultProperties)
					{
						RecalculateTEUQuantity(TradeDetail?.ProspectDetail.Container);
						if (IsFreight && IsSeaFcl)
						{
							RecalculateEstimatedRevenueIfProspect();
						}
					}
				}
			}
		}

		protected bool PAS_Units_ReadOnly
		{
			get { return !TradeDetail.AllowUnitsAndTEUQuantity; }
		}

		#endregion

		#region PAS_RepeatsMnth

		public override ZDecimal PAS_RepeatsMnth
		{
			get { return base.PAS_RepeatsMnth; }
			set
			{
				if (base.PAS_RepeatsMnth != value)
				{
					base.PAS_RepeatsMnth = value;

					if (!PAS_IsTraded)
					{
						RecalculateProspectPeriodEstimatedRevenue();

						var sales = Sales;
						if (sales != null && !sales.IsActual)
						{
							sales.RefreshTotalAnnualStats();
						}
					}
				}
			}
		}

		#endregion

		#region PAS_Weight

		public override ZDecimal PAS_Weight
		{
			get { return base.PAS_Weight; }
			set
			{
				if (base.PAS_Weight != value)
				{
					base.PAS_Weight = value;

					if (!PAS_IsTraded)
					{
						var sales = Sales;
						if (sales != null && !sales.IsActual)
						{
							UpdateChargeableIfProspect();
							sales.OW_Calc_TotalAnnualWeightInfo.RefreshBinding();
						}
					}
				}
			}
		}

		#endregion

		#region PAS_WeightUQ

		[List("Lookups.UnitOfWeightList")]
		public override ZString PAS_WeightUQ
		{
			get { return base.PAS_WeightUQ; }
			set
			{
				if (base.PAS_WeightUQ != value)
				{
					base.PAS_WeightUQ = value;
					UpdateChargeableIfProspect();
				}
			}
		}

		public bool WeightUQIsMetric
		{
			get { return !Constants.Volume.IsImperial(PAS_WeightUQ); }
		}

		#endregion

		#region PAS_Volume

		public override ZDecimal PAS_Volume
		{
			get { return base.PAS_Volume; }
			set
			{
				if (base.PAS_Volume != value)
				{
					base.PAS_Volume = value;

					if (!PAS_IsTraded)
					{
						var sales = Sales;
						if (sales != null && !sales.IsActual)
						{
							UpdateChargeableIfProspect();
							sales.OW_Calc_TotalAnnualVolumeInfo.RefreshBinding();
						}
					}
				}
			}
		}

		#endregion

		#region PA_VolumeUQ

		[List("Lookups.UnitOfVolumeList")]
		public override ZString PAS_VolumeUQ
		{
			get { return base.PAS_VolumeUQ; }
			set
			{
				if (base.PAS_VolumeUQ != value)
				{
					base.PAS_VolumeUQ = value;
					UpdateChargeableIfProspect();
				}
			}
		}

		public bool VolumeUQIsMetric
		{
			get { return !Constants.Volume.IsImperial(PAS_VolumeUQ); }
		}

		#endregion

		#region PA_Chargeable

		public override ZDecimal PAS_Chargeable
		{
			get { return base.PAS_Chargeable; }
			set
			{
				if (base.PAS_Chargeable != value)
				{
					base.PAS_Chargeable = value;

					var sales = Sales;
					if (sales != null && !sales.IsActual)
					{
						if (ShouldDefaultProperties && IsFreight && !IsSeaFcl)
						{
							RecalculateEstimatedRevenueIfProspect();
						}

						sales.OW_Calc_TotalAnnualChargeableInfo.RefreshBinding();
					}
				}
			}
		}

		public ZString PAS_Calc_ChargeableUQ
		{
			get { return ChargeableAmountCalculator.GetChargeableUnit(TradeDetail.TransportMode, PAS_WeightUQ, PAS_VolumeUQ); }
		}

		void UpdateChargeableIfProspect()
		{
			if (!PAS_IsTraded && ShouldDefaultProperties && !PAS_VolumeInfo.HasErrors() && !PAS_WeightInfo.HasErrors())
			{
				PAS_Chargeable = CalculateChargeable(PAS_Weight, PAS_WeightUQ, PAS_Volume, PAS_VolumeUQ, PAS_Calc_ChargeableUQ, TradeDetail.Sales.IsDomestic, TradeDetail.TransportMode);
			}
		}

		public static ZDecimal CalculateChargeable(ZDecimal pasWeight, ZString pasWeightUQ, ZDecimal pasVolume, ZString pasVolumeUQ, ZString pasCalcChargeableUQ, bool isDomestic, string transportMode)
		{
			return ChargeableAmountCalculator.CalculateChargeable(new ChargeableParameters
			{
				Weight = new ZWeight(pasWeight, pasWeightUQ),
				Volume = new ZVolume(pasVolume, pasVolumeUQ),
				TargetUnit = pasCalcChargeableUQ,
				ConversionFactors = ChargeableAmountCalculator.GetDefaultConversionFactors(isDomestic, transportMode, pasCalcChargeableUQ)
			}).Chargeable.Amount;
		}

		#endregion

		#region PAS_TEUQuantity

		public override ZDecimal PAS_TEUQuantity
		{
			get { return base.PAS_TEUQuantity; }
			set
			{
				if (base.PAS_TEUQuantity != value)
				{
					base.PAS_TEUQuantity = value;

					if (!PAS_IsTraded)
					{
						var sales = Sales;
						if (sales != null && !sales.IsActual)
						{
							sales.OW_Calc_TotalAnnualTEUInfo.RefreshBinding();
						}
					}
				}
			}
		}

		public bool PAS_TEUQuantity_ReadOnly
		{
			get
			{
				if (TradeDetail == null)
				{
					return false;
				}
				else
				{
					return !TradeDetail.ProspectDetail.PAP_RC_NKContainer.IsEmpty || !TradeDetail.AllowUnitsAndTEUQuantity;
				}
			}
		}

		#endregion

		#region PAS_PalletCount

		public override ZInt PAS_PalletCount
		{
			get { return base.PAS_PalletCount; }
			set
			{
				if (base.PAS_PalletCount != value)
				{
					base.PAS_PalletCount = value;

					if (!PAS_IsTraded)
					{
						var sales = Sales;
						if (sales != null && !sales.IsActual)
						{
							if (SalesProductCode == SystemDefinedSalesProductList.Codes.Warehouse)
							{
								RecalculateEstimatedRevenueIfProspect();
							}
							sales.OW_Calc_TotalAnnualPalletCountInfo.RefreshBinding();
						}
					}
				}
			}
		}

		#endregion

		#region EstimatedRevenue

		public override ZDecimal PAS_EstimatedProfit
		{
			get => base.PAS_EstimatedProfit;
			set
			{
				if (value != base.PAS_EstimatedProfit)
				{
					base.PAS_EstimatedProfit = value;
					if (!PAS_IsTraded)
					{
						RecalculateProspectPeriodEstimatedRevenue();

						var sales = Sales;
						if (sales != null && !sales.IsActual)
						{
							sales.RefreshTotalAnnualStats();
						}
					}
				}
			}
		}

		void RecalculateProspectPeriodEstimatedRevenue()
		{
			if (PAS_Period.IsEmpty)
			{
				var tradeDetail = TradeDetail;
				if (tradeDetail != null)
				{
					var jobCount = Math.Max(1, PAS_RepeatsMnth);
					foreach (var period in tradeDetail.ProspectPeriods)
					{
						period.PAS_EstimatedProfit = CalculateEstimatedProfitPerPeriod(tradeDetail.ProspectDetail.PAP_RecurrenceType, PAS_EstimatedProfit, jobCount, tradeDetail.ProspectPeriods.Count);
					}
				}
			}
		}

		void RecalculateEstimatedRevenueIfProspect()
		{
			if (ShouldDefaultProperties)
			{
				PAS_EstimatedProfit = GetCalculatedEstimatedRevenue(SalesProductCode, (ZDecimal)PAS_PalletCount, IsFreight, IsSeaFcl, PAS_Units, PAS_Chargeable, PAS_RateOffered);
			}
		}

		public static ZDecimal GetCalculatedEstimatedRevenue(ZString salesProductCode, ZDecimal pasPalletCount, bool isFreight, bool isSeaFcl, ZLong pasUnits, ZDecimal pasChargeable, ZDecimal pasRateOffered)
		{
			ZDecimal units;
			if (salesProductCode == SystemDefinedSalesProductList.Codes.Warehouse)
			{
				units = pasPalletCount;
			}
			else if (isFreight)
			{
				units = isSeaFcl ? (ZDecimal)pasUnits : pasChargeable;
			}
			else
			{
				units = 1;
			}

			return units * pasRateOffered;
		}

		public ZString EstimatedProfitPerUnitType
		{
			get
			{
				if (SalesProductCode == SystemDefinedSalesProductList.Codes.Warehouse)
				{
					return Constants.PkgUnit.Pallet;
				}
				else if (IsFreight)
				{
					return IsSeaFcl ? (ZString)Constants.PkgUnit.Container : PAS_Calc_ChargeableUQ;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public static ZDecimal CalculateEstimatedProfitPerPeriod(ZString recurrenceType, ZDecimal estimatedProfit, ZDecimal jobCount, int prospectPeriodsCount)
		{
			var result = ZDecimal.Zero;

			if (recurrenceType == OrgTradeProspectRecurrenceTypeList.Codes.Yearly)
			{
				result = estimatedProfit * jobCount / 12m;
			}
			else if (recurrenceType == OrgTradeProspectRecurrenceTypeList.Codes.Monthly)
			{
				result = estimatedProfit * jobCount;
			}
			else if (recurrenceType == OrgTradeProspectRecurrenceTypeList.Codes.Weekly)
			{
				result = estimatedProfit * jobCount * 52m / 12m;
			}
			else if (recurrenceType == OrgTradeProspectRecurrenceTypeList.Codes.OneOff)
			{
				result = estimatedProfit * jobCount / prospectPeriodsCount;
			}

			return result;
		}

		#endregion

		#region Estimated TEU Quantity

		public static ZDecimal CalculateEstimatedTEUQuantityPerPeriod(ZString recurrenceType, ZDecimal teuQuantity, ZDecimal jobCount, int prospectPeriodsCount)
		{
			switch (recurrenceType)
			{
				case OrgTradeProspectRecurrenceTypeList.Codes.Yearly:
					return (teuQuantity * jobCount) / 12m;
				case OrgTradeProspectRecurrenceTypeList.Codes.Monthly:
					return teuQuantity * jobCount;
				case OrgTradeProspectRecurrenceTypeList.Codes.Weekly:
					return (teuQuantity * jobCount * 52m) / 12;
				case OrgTradeProspectRecurrenceTypeList.Codes.OneOff:
					return (teuQuantity * jobCount) / prospectPeriodsCount;
				default:
					return ZDecimal.Zero;
			}
		}

		#endregion

		public void RecalculateTEUQuantity(RefContainer container)
		{
			PAS_TEUQuantity = container != null
				? (ZDecimal)(container.RC_TEU * PAS_Units)
				: (ZDecimal)0m;
		}

		#endregion

		#region ReadOnly

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			bool shouldBeReadOnly = false;
			if (TradeDetail?.Parent?.ParentOrganisation != null)
			{
				shouldBeReadOnly = !TradeDetail.Parent.ParentOrganisation.SecurityProvider.HasModifySalesTradeProfileSecurity;
			}

			return shouldBeReadOnly || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion

		#region Traded Values

		public OrgTradeValueCollection TradeValues
		{
			get
			{
				if (tradeValues == null)
				{
					tradeValues = new OrgTradeValueCollection(this);
				}
				return tradeValues;
			}
		}
		OrgTradeValueCollection tradeValues;

		#endregion

		#region SupportsNotes

		public override bool SupportsNotes => false;

		#endregion

		#region Delete

		public override void Delete()
		{
			TradeValues.DeleteAll();
			base.Delete();
		}

		#endregion
	}
}

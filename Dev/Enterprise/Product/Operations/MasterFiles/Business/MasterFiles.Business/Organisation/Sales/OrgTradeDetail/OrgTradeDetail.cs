using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[UserDefinedValues]
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgTradeDetail : AutoOrgTradeDetail, IOrgTradeDetail, ISalesValue
	{
		public OrgTradeDetail(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Constants

		public new class Schema : AutoOrgTradeDetail.Schema
		{
			public const string PA_Calc_Destination = "PA_Calc_Destination";
			public const string PA_Calc_LocalExchangeRate = "PA_Calc_LocalExchangeRate";
			public const string PA_Calc_OH_Buyer = "PA_Calc_OH_Buyer";
			public const string PA_Calc_OH_Supplier = "PA_Calc_OH_Supplier";
			public const string PA_Calc_Origin = "PA_Calc_Origin";
		}

		public class TradeLaneStatus : CodeDescriptionPairList
		{
			public TradeLaneStatus()
			{
				AddTradeLaneStatusPairs();
			}

			public const string Quoted = "QTE";
			public const string Confirmed = "CNF";
			public const string Shipped = "SHP";

			void AddTradeLaneStatusPairs()
			{
				AddPair(Quoted, ResString.GetMultilingualString("OrgTradeDetailLookups|Quoted", "Quoted"));
				AddPair(Confirmed, ResString.GetMultilingualString("OrgTradeDetailLookups|Confirmed", "Confirmed"));
				AddPair(Shipped, ResString.GetMultilingualString("OrgTradeDetailLookups|Shipped", "Shipped"));
			}
		}

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			PA_Status = OpportunityTradeStatus.Codes.Active;
		}

		#endregion

		#region OnSaving

		public override void OnSaving()
		{
			base.OnSaving();

			ResetProspectPeriodStartAndPeriodEndIfNeeded();
			if (IsProspect)
			{
				new OrgTradeDetailValueSuperceder(this, ProspectPeriodStart).SupercedeOverlappedPeriods();
			}
		}

		#endregion

		#region Properties

		#region PA_TradeMode

		[List("Lookups.TradeModes")]
		public override ZString PA_TradeMode
		{
			get { return base.PA_TradeMode; }
			set
			{
				if (base.PA_TradeMode != value)
				{
					base.PA_TradeMode = value;

					if (ShouldDefaultProperties)
					{
						var defaultCurrency = GetDefaultCurrency(Parent);
						if (!defaultCurrency.IsEmpty && (CurrentProspectPeriod.PAS_RX_NKCurrency.IsEmpty || !IsInDatabase))
						{
							CurrentProspectPeriod.PAS_RX_NKCurrency = defaultCurrency;
						}

						var calculatedTradeType = GetCalculatedTradeType(SalesProductCode, PA_TradeMode, ProspectDetail.PAP_RC_NKContainer);
						if (calculatedTradeType.HasValue)
						{
							PA_TradeType = calculatedTradeType.Value.Value;
						}
					}
				}
			}
		}

		protected bool PA_TradeMode_ReadOnly
		{
			get
			{
				var calculatedTradeMode = GetCalculatedTradeMode(SalesProductCode);
				return (calculatedTradeMode.HasValue && calculatedTradeMode.Value.IsReadOnly) || (Parent != null && !Parent.IsEditable);
			}
		}

		public void SetTradeModeDirectly(ZString tradeMode)
		{
			base.PA_TradeMode = tradeMode;
		}

		static CalculatedValue? GetCalculatedTradeMode(ZString productCode)
		{
			if (productCode == SystemDefinedSalesProductList.Codes.Transport)
			{
				return CalculatedValue.ReadOnly(OrgTradeDetailLookups.TransportTradeModes.TransportBooking);
			}

			return null;
		}

		protected ZString GetDefaultCurrency(OrgSales parentSales)
		{
			var defaultCurrency = ZString.Empty;
			if (parentSales != null)
			{
				ZString salesProductCode = SalesProductCode;
				if (salesProductCode == SystemDefinedSalesProductList.Codes.CustomsBrokerage)
				{
					defaultCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				}
				else if ((salesProductCode == SystemDefinedSalesProductList.Codes.Transport) || (IsFreight && PA_TradeMode == Constants.TransportModes.Air))
				{
					defaultCurrency =
						parentSales != null &&
						parentSales.Origin != null &&
						parentSales.Origin.Country != null &&
						parentSales.Origin.Country.LocalCurrency != null ? parentSales.Origin.Country.RN_RX_NKLocalCurrency : ZString.Empty;
				}
				else if (!IsFreight)
				{
					defaultCurrency =
						parentSales != null &&
						parentSales.Destination != null &&
						parentSales.Destination.Country != null &&
						parentSales.Destination.Country.LocalCurrency != null ? parentSales.Destination.Country.RN_RX_NKLocalCurrency : ZString.Empty;
				}
				else
				{
					defaultCurrency = Constants.CurrencyCodes.UnitedStates;
				}
			}

			return defaultCurrency;
		}

		#endregion

		#region PA_TradeType

		[List("Lookups.TradeTypes")]
		public override ZString PA_TradeType
		{
			get { return base.PA_TradeType; }
			set
			{
				bool wasChanged = PA_TradeType != value;
				base.PA_TradeType = value;
				if (wasChanged)
				{
					if (ShouldDefaultProperties && !AllowUnitsAndTEUQuantity)
					{
						CurrentProspectPeriod.PAS_Units = 0;
						CurrentProspectPeriod.PAS_TEUQuantity = 0;
					}
				}
			}
		}

		protected bool PA_TradeTypeReadOnly
		{
			get
			{
				var calculatedTradeType = GetCalculatedTradeType(SalesProductCode, PA_TradeMode, ProspectDetail.PAP_RC_NKContainer);
				return (calculatedTradeType.HasValue && calculatedTradeType.Value.IsReadOnly) || (Parent != null && !Parent.IsEditable);
			}
		}

		public void SetTradeTypeDirectly(ZString tradeType)
		{
			base.PA_TradeType = tradeType;
		}

		public bool AllowUnitsAndTEUQuantity
		{
			get
			{
				var salesProductCode = SalesProductCode;
				if (salesProductCode == SystemDefinedSalesProductList.Codes.ForwardingShipment)
				{
					return
						(PA_TradeType == Constants.ContainerModes.FCL)
						|| (PA_TradeType == Constants.ContainerModes.RollOnRollOff)
						|| (PA_TradeType == Constants.ContainerModes.ULD)
						|| (PA_TradeType == Constants.ContainerModes.FTL);
				}
				else if (salesProductCode == SystemDefinedSalesProductList.Codes.Transport)
				{
					return
						(PA_TradeType == Constants.ContainerModes.FTL);
				}
				else if (salesProductCode == SystemDefinedSalesProductList.Codes.LinerAgency)
				{
					return
						(PA_TradeType == Constants.ContainerModes.FCL)
						|| (PA_TradeType == Constants.ContainerModes.RollOnRollOff);
				}
				else
				{
					return true;
				}
			}
		}

		public void SetDefaultTradeType()
		{
			var defaultTradeType = GetCalculatedTradeType(SalesProductCode, PA_TradeMode, ProspectDetail.PAP_RC_NKContainer);
			if (defaultTradeType.HasValue)
			{
				PA_TradeType = defaultTradeType.Value.Value;
			}
		}

		static CalculatedValue? GetCalculatedTradeType(ZString productCode, ZString tradeMode, ZString containerType)
		{
			if (productCode == SystemDefinedSalesProductList.Codes.Transport)
			{
				return containerType.IsEmpty ? CalculatedValue.Editable(Constants.ContainerModes.LTL) : CalculatedValue.ReadOnly(Constants.ContainerModes.FTL);
			}
			else if (productCode == SystemDefinedSalesProductList.Codes.ForwardingShipment)
			{
				if (tradeMode == Constants.TransportModes.Air)
				{
					return containerType.IsEmpty ? CalculatedValue.Editable(Constants.ContainerModes.Loose) : CalculatedValue.ReadOnly(Constants.ContainerModes.ULD);
				}
				else if (tradeMode == Constants.TransportModes.Sea || tradeMode == Constants.TransportModes.Rail)
				{
					return containerType.IsEmpty ? CalculatedValue.Editable(Constants.ContainerModes.LCL) : CalculatedValue.ReadOnly(Constants.ContainerModes.FCL);
				}
				else if (tradeMode == Constants.TransportModes.Road)
				{
					return containerType.IsEmpty ? CalculatedValue.Editable(Constants.ContainerModes.LTL) : CalculatedValue.ReadOnly(Constants.ContainerModes.FTL);
				}
			}

			return null;
		}

		struct CalculatedValue
		{
			CalculatedValue(ZString value, ZBool isReadOnly)
			{
				Value = value;
				IsReadOnly = isReadOnly;
			}

			public readonly ZString Value;
			public readonly ZBool IsReadOnly;

			public static CalculatedValue ReadOnly(ZString value)
			{
				return new CalculatedValue(value, true);
			}

			public static CalculatedValue Editable(ZString value)
			{
				return new CalculatedValue(value, false);
			}
		}

		public bool IsFreight
		{
			get { return Parent != null && Parent.Product != null && Parent.Product.IsFreight; }
		}

		public bool IsSea
		{
			get { return PA_TradeMode == Constants.TransportModes.Sea || SalesProductCode == SystemDefinedSalesProductList.Codes.LinerAgency; }
		}

		public bool IsSeaFcl
		{
			get { return IsSea && PA_TradeType == Constants.ContainerModes.FCL; }
		}

		#endregion

		#region Estimated Revenue

		[ResourceStringData("Enterprise.MasterFiles.Business.OrgTradeDetail|PA_Calc_EstimatedAnnualValue", Caption = "Total Estimate Value (p.a)")]
		public ZDecimal PA_Calc_EstimatedAnnualValue
		{
			get
			{
				if (PA_Status == OpportunityTradeStatus.Codes.Successful)
				{
					return PA_Calc_TotalCommittedValueIncludeForecast;
				}
				else
				{
					return PA_Calc_EstimatedPipelineAnnualValue;
				}
			}
		}

		public ZPropertyInfo PA_Calc_EstimatedAnnualValueInfo => GetZPropertyInfo(nameof(PA_Calc_EstimatedAnnualValue));

		[ResourceStringData("Enterprise.MasterFiles.Business.OrgTradeDetail|PA_Calc_EstimatedMonthlyValue", Caption = "Total Estimate Value (m/avg)")]
		public ZDecimal PA_Calc_EstimatedMonthlyValue => PA_Calc_EstimatedAnnualValue / 12;
		public ZPropertyInfo PA_Calc_EstimatedMonthlyValueInfo => GetZPropertyInfo(nameof(PA_Calc_EstimatedMonthlyValue));

		public ZDecimal PA_Calc_EstimatedPipelineMonthlyValue => CurrentProspectPeriod == null ? 0 : ConvertRecurringToMonthlyAmount(CurrentProspectPeriod.PAS_EstimatedProfit);

		public ZDecimal PA_Calc_EstimatedPipelineAnnualValue => CurrentProspectPeriod == null ? 0 : ConvertRecurringToAnnualAmount(CurrentProspectPeriod.PAS_EstimatedProfit, true);

		public ZDecimal PA_Calc_EstimatedPipelineAnnualTEUQuantity => CurrentProspectPeriod == null ? 0 : ConvertRecurringToAnnualAmount(CurrentProspectPeriod.PAS_TEUQuantity, true);

		ZDecimal ConvertRecurringToAnnualAmount(ZDecimal recurringAmount, bool useRecurringCountOfOneIfZero)
		{
			var repeats = (!useRecurringCountOfOneIfZero || CurrentProspectPeriod.PAS_RepeatsMnth > 0) ? CurrentProspectPeriod.PAS_RepeatsMnth : 1;
			return GetAnnualAmount(repeats, recurringAmount, ProspectDetail.PAP_RecurrenceType);
		}

		public static ZDecimal GetAnnualAmount(ZDecimal repeats, ZDecimal recurringAmount, ZString recurrenceType)
		{
			switch (recurrenceType)
			{
				case OrgTradeProspectRecurrenceTypeList.Codes.Monthly:
					return repeats * recurringAmount * 12;

				case OrgTradeProspectRecurrenceTypeList.Codes.Weekly:
					return repeats * recurringAmount * 52;

				case OrgTradeProspectRecurrenceTypeList.Codes.Yearly:
				case OrgTradeProspectRecurrenceTypeList.Codes.OneOff:
				default:
					return repeats * recurringAmount;
			}
		}

		ZDecimal ConvertRecurringToMonthlyAmount(ZDecimal recurringAmount)
		{
			var repeats = (CurrentProspectPeriod.PAS_RepeatsMnth > 0) ? CurrentProspectPeriod.PAS_RepeatsMnth : 1;

			switch (ProspectDetail.PAP_RecurrenceType)
			{
				case OrgTradeProspectRecurrenceTypeList.Codes.Yearly:
					return repeats * recurringAmount / 12m;

				case OrgTradeProspectRecurrenceTypeList.Codes.Monthly:
					return repeats * recurringAmount;

				case OrgTradeProspectRecurrenceTypeList.Codes.Weekly:
					return repeats * recurringAmount * 52m / 12m;

				case OrgTradeProspectRecurrenceTypeList.Codes.OneOff:
					return 0m;

				default:
					return repeats * recurringAmount;
			}
		}

		public ZDecimal PA_Calc_CommittedValue => CalculateProspectPeriodsValue(x => !x.PAS_IsSuperseded && !x.PAS_IsForecast && !x.PAS_IsExpired);

		public ZDecimal PA_Calc_TotalCommittedValueIncludeForecast => CalculateProspectPeriodsValue(x => !x.PAS_IsSuperseded && !x.PAS_IsExpired);

		ZDecimal CalculateProspectPeriodsValue(Func<OrgTradePeriod, bool> periodCondition)
		{
			var committedProspectPeriods = ProspectPeriods.Where(periodCondition).ToList();
			if (committedProspectPeriods.Count <= 12)
			{
				return committedProspectPeriods.Sum(x => x.PAS_EstimatedProfit);
			}
			else if (committedProspectPeriods.Any())
			{
				return committedProspectPeriods.OrderBy(x => x.PAS_Period).Take(12).Sum(x => x.PAS_EstimatedProfit);
			}
			else
			{
				return 0m;
			}
		}

		#endregion

		#region PA_Calc_AnnualCount

		public ZDecimal PA_Calc_AnnualCount
		{
			get { return ConvertRecurringToAnnualAmount(1, false); }
		}

		#endregion

		#region PA_Calc_AnnualWeight

		public ZDecimal PA_Calc_AnnualWeight => CurrentProspectPeriod == null ? 0 : ConvertRecurringToAnnualAmount(CurrentProspectPeriod.PAS_Weight, true);

		#endregion

		#region PA_Calc_AnnualVolume

		public ZDecimal PA_Calc_AnnualVolume => CurrentProspectPeriod == null ? 0 : ConvertRecurringToAnnualAmount(CurrentProspectPeriod.PAS_Volume, true);

		#endregion

		#region TransportMode

		public string TransportMode
		{
			get
			{
				var salesProductCode = SalesProductCode;
				return
					salesProductCode == SystemDefinedSalesProductList.Codes.Transport ? Constants.TransportModes.Road :
					salesProductCode == SystemDefinedSalesProductList.Codes.LinerAgency ? Constants.TransportModes.Sea :
					(string)PA_TradeMode;
			}
		}

		#endregion

		#region PA_Calc_AnnualChargeable

		public ZDecimal PA_Calc_AnnualChargeable => CurrentProspectPeriod == null ? 0 : ConvertRecurringToAnnualAmount(CurrentProspectPeriod.PAS_Chargeable, true);

		public bool ChargeableUQIsMetric => CurrentProspectPeriod != null && ChargeableAmountCalculator.ChargeableUnitIsMetric(CurrentProspectPeriod.PAS_WeightUQ, CurrentProspectPeriod.PAS_VolumeUQ);

		#endregion

		#region PA_Calc_AnnualTEU

		public ZDecimal PA_Calc_AnnualTEU => CurrentProspectPeriod == null ? 0 : ConvertRecurringToAnnualAmount(CurrentProspectPeriod.PAS_TEUQuantity, true);

		#endregion

		#region PA_Calc_AnnualPalletCount

		public ZDecimal PA_Calc_AnnualPalletCount => CurrentProspectPeriod == null ? 0 : ConvertRecurringToAnnualAmount((ZDecimal)CurrentProspectPeriod.PAS_PalletCount, true);

		#endregion

		#region PA_Calc_Origin

		public ZString PA_Calc_Origin
		{
			get { return Parent != null ? Parent.OriginCode : ZString.Empty; }
		}

		public ZPropertyInfo PA_Calc_OriginInfo
		{
			get { return GetZPropertyInfo(OrgTradeDetail.Schema.PA_Calc_Origin); }
		}

		#endregion

		#region PA_Calc_Destination

		public ZString PA_Calc_Destination
		{
			get { return Parent != null ? Parent.DestinationCode : ZString.Empty; }
		}

		public ZPropertyInfo PA_Calc_DestinationInfo
		{
			get { return GetZPropertyInfo(OrgTradeDetail.Schema.PA_Calc_Destination); }
		}

		#endregion

		#region PA_Calc_OH_Buyer

		[List("Parent.Lookups.Buyers")]
		public ZGuid PA_Calc_OH_Buyer
		{
			get { return Parent != null ? Parent.OW_OH_Buyer : ZGuid.Empty; }
		}

		public ZPropertyInfo PA_Calc_OH_BuyerInfo
		{
			get { return GetZPropertyInfo(OrgTradeDetail.Schema.PA_Calc_OH_Buyer); }
		}

		#endregion

		#region PA_Calc_OH_Supplier

		[List("Parent.Lookups.Suppliers")]
		public ZGuid PA_Calc_OH_Supplier
		{
			get { return Parent != null ? Parent.OW_OH_Supplier : ZGuid.Empty; }
		}

		public ZPropertyInfo PA_Calc_OH_SupplierInfo
		{
			get { return GetZPropertyInfo(OrgTradeDetail.Schema.PA_Calc_OH_Supplier); }
		}

		#endregion

		#region PA_OP

		public ZString SupplierPartDescription
		{
			get
			{
				var supplierPart = SupplierPart;
				if (supplierPart == null)
				{
					return Res.GetString("2f3c87df-afe5-4de1-94ed-a52c5e0f8e92", "No Product");
				}

				return supplierPart.OP_Desc;
			}
		}

		#endregion

		#region PA_Status

		[List("Lookups.TradeLaneStatuses")]
		public override ZString PA_Status
		{
			get { return base.PA_Status; }
			set { base.PA_Status = value; }
		}

		public ZString PA_StatusDescription
		{
			get
			{
				var statusDesc = Lookups.TradeLaneStatuses.GetDescriptionFromCode(PA_Status);

				if (IsCommitted && IsSuperceded)
				{
					return Res.GetString("F438AEB9-F702-4069-83F0-F887CDFDACCB", "{0} (Superseded)", statusDesc);
				}
				else if (IsCommitted && IsExpired)
				{
					return Res.GetString("55E01CC6-FCC7-4DE5-B1BB-43D2BA294260", "{0} (Expired)", statusDesc);
				}
				else
				{
					return statusDesc ?? ZString.Empty;
				}
			}
		}

		public bool IsCommitted => PA_Status == OpportunityTradeStatus.Codes.Successful;
		public bool IsPipeline => PA_Status == OpportunityTradeStatus.Codes.Active;
		public bool IsUnsuccessful => PA_Status == OpportunityTradeStatus.Codes.Unsuccessful;

		#endregion

		#region Is Actual / Prospect

		public bool IsActual => Sales?.IsActual ?? false;

		public bool IsProspect => !IsActual && PA_Status != OrgTradeDetail.TradeLaneStatus.Confirmed && PA_Status != OrgTradeDetail.TradeLaneStatus.Quoted;

		#endregion

		#region Is Superceded / Expired

		public ZBool IsSuperceded => ProspectPeriods.Any(x => x.PAS_IsSuperseded);

		public ZBool IsExpired => !ProspectDetail.PAP_ExpiryReason.IsEmpty;

		#endregion

		#region Is Oneoff

		public bool IsOneoff => ProspectDetail.PAP_RecurrenceType == OrgTradeProspectRecurrenceTypeList.Codes.OneOff;

		#endregion

		#region Prospect Period Start / End

		[ResourceStringData("Enterprise.MasterFiles.Business.OrgTradeDetail|ProspectPeriodEndType", Caption = "Trade Commitment")]
		[List("Lookups.OrgTradeProspectPeriodEndTypes")]
		public ZString ProspectPeriodEndType
		{
			get => prospectPeriodEndType;
			set
			{
				if (value != prospectPeriodEndType)
				{
					SetNonPersistentPropertyValue(ProspectPeriodEndTypeInfo, ref prospectPeriodEndType, value);
					UpdateProspectPeriodEnd();
				}
			}
		}
		ZString prospectPeriodEndType;

		public ZPropertyInfo ProspectPeriodEndTypeInfo => GetZPropertyInfo(nameof(ProspectPeriodEndType));

		public bool ProspectPeriodEndType_ReadOnly => !IsCommitted || IsSuperceded;

		void SetDefaultProspectPeriodEndType()
		{
			if (prospectPeriodEndType.IsEmpty
				&& prospectPeriodStart.HasValue && prospectPeriodStart.Value.IsValid
				&& prospectPeriodEnd.HasValue && prospectPeriodEnd.Value.IsValid)
			{
				var periodStart = prospectPeriodStart.Value;
				var periodEnd = prospectPeriodEnd.Value;
				var periodMonths = ((periodEnd.Year - periodStart.Year) * 12) + periodEnd.Month - periodStart.Month + 1;
				switch (periodMonths)
				{
					case 1:
						prospectPeriodEndType = OrgTradeProspectPeriodEndTypeList.Codes._1Month;
						break;
					case 3:
						prospectPeriodEndType = OrgTradeProspectPeriodEndTypeList.Codes._3Months;
						break;
					case 6:
						prospectPeriodEndType = OrgTradeProspectPeriodEndTypeList.Codes._6Months;
						break;
					case 12:
						prospectPeriodEndType = OrgTradeProspectPeriodEndTypeList.Codes._12Months;
						break;
				}

				if (prospectPeriodEndType.IsEmpty)
				{
					var financialYearEnd = GetFirstDayOfCurrentFinancialYearEndPeriod();
					if (financialYearEnd.IsValid)
					{
						if (periodEnd == financialYearEnd)
						{
							prospectPeriodEndType = OrgTradeProspectPeriodEndTypeList.Codes.EndOfCurrentFinancialYear;
						}
						else if (periodEnd == financialYearEnd.AddYears(1))
						{
							prospectPeriodEndType = OrgTradeProspectPeriodEndTypeList.Codes.EndOfNextFinancialYear;
						}
					}
				}

				if (prospectPeriodEndType.IsEmpty)
				{
					prospectPeriodEndType = OrgTradeProspectPeriodEndTypeList.Codes.ManuallyEnter;
				}

				ProspectPeriodEndTypeInfo.RefreshBinding();
			}
		}

		void UpdateProspectPeriodEnd()
		{
			if (ProspectPeriodStart.IsValid)
			{
				switch (ProspectPeriodEndType)
				{
					case OrgTradeProspectPeriodEndTypeList.Codes._1Month:
						ProspectPeriodEnd = ProspectPeriodStart;
						break;
					case OrgTradeProspectPeriodEndTypeList.Codes._3Months:
						ProspectPeriodEnd = ProspectPeriodStart.AddMonths(2);
						break;
					case OrgTradeProspectPeriodEndTypeList.Codes._6Months:
						ProspectPeriodEnd = ProspectPeriodStart.AddMonths(5);
						break;
					case OrgTradeProspectPeriodEndTypeList.Codes._12Months:
						ProspectPeriodEnd = ProspectPeriodStart.AddMonths(11);
						break;
					case OrgTradeProspectPeriodEndTypeList.Codes.EndOfCurrentFinancialYear:
						ProspectPeriodEnd = GetFirstDayOfCurrentFinancialYearEndPeriod();
						break;
					case OrgTradeProspectPeriodEndTypeList.Codes.EndOfNextFinancialYear:
						var currentFinancialYearEnd = GetFirstDayOfCurrentFinancialYearEndPeriod();
						ProspectPeriodEnd = currentFinancialYearEnd.IsValid ? currentFinancialYearEnd.AddYears(1) : ZDate.Empty;
						break;
				}
			}
		}

		ZDate GetFirstDayOfCurrentFinancialYearEndPeriod()
		{
			var result = ZDate.Empty;

			var today = ZDate.Today;
			var firstDayOfCurrentCalendarMonth = new ZDate(today.Year, today.Month, 1);
			var periodCalculator = new AccountingPeriodCalculator(Factory);
			var currentFinancialPeriod = periodCalculator.GetPeriodFromDate(today);
			if (currentFinancialPeriod > 0)
			{
				result = firstDayOfCurrentCalendarMonth.AddMonths(12 - currentFinancialPeriod % 100);
			}
			return result;
		}

		[ResourceStringData("Enterprise.MasterFiles.Business.OrgTradeDetail|ProspectPeriodStart", Caption = "Start Period")]
		public ZDate ProspectPeriodStart
		{
			get
			{
				if (!prospectPeriodStart.HasValue)
				{
					var periodMonths = ProspectPeriods.Where(x => !x.PAS_IsForecast).Select(x => x.PAS_Period);
					prospectPeriodStart = periodMonths.Any() ? periodMonths.Min() : ZDate.Empty;
					SetDefaultProspectPeriodEndType();
				}
				return prospectPeriodStart.Value;
			}
			set
			{
				if (value != prospectPeriodStart)
				{
					SetNonPersistentPropertyValue(ProspectPeriodStartInfo, ref prospectPeriodStart, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateProspectPeriodStart();
					}
					UpdateProspectPeriodEnd();
					UpdateProspectPeriods();
				}
			}
		}
		ZDate? prospectPeriodStart;

		public ZPropertyInfo ProspectPeriodStartInfo => GetZPropertyInfo(nameof(ProspectPeriodStart));
		public bool ProspectPeriodStart_ReadOnly => !IsCommitted || ProspectPeriodEndType != OrgTradeProspectPeriodEndTypeList.Codes.ManuallyEnter || IsSuperceded;

		[ResourceStringData("Enterprise.MasterFiles.Business.OrgTradeDetail|ProspectPeriodEnd", Caption = "End Period")]
		public ZDate ProspectPeriodEnd
		{
			get
			{
				if (!prospectPeriodEnd.HasValue)
				{
					var periodMonths = ProspectPeriods.Where(x => !x.PAS_IsForecast).Select(x => x.PAS_Period);
					prospectPeriodEnd = periodMonths.Any() ? periodMonths.Max() : ZDate.Empty;
					SetDefaultProspectPeriodEndType();
				}
				return prospectPeriodEnd.Value;
			}
			set
			{
				if (value != prospectPeriodEnd)
				{
					SetNonPersistentPropertyValue(ProspectPeriodEndInfo, ref prospectPeriodEnd, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateProspectPeriodEnd();
					}
					UpdateProspectPeriods();
				}
			}
		}
		ZDate? prospectPeriodEnd;

		public ZPropertyInfo ProspectPeriodEndInfo => GetZPropertyInfo(nameof(ProspectPeriodEnd));

		public bool ProspectPeriodEnd_ReadOnly => !IsCommitted || ProspectPeriodEndType != OrgTradeProspectPeriodEndTypeList.Codes.ManuallyEnter || IsSuperceded;

		void ResetProspectPeriodStartAndPeriodEndIfNeeded()
		{
			if (ProspectPeriods.Any(x => !x.PAS_IsForecast && !x.PAS_Period.IsEmpty))
			{
				if (ProspectPeriodStart.IsEmpty)
				{
					prospectPeriodStart = null;
				}

				if (ProspectPeriodEnd.IsEmpty)
				{
					prospectPeriodEnd = null;
				}
			}
		}

		#endregion

		#region ShouldDefaultProperties

		public bool ShouldDefaultProperties
		{
			get { return Sales != null && Sales.ShouldDefaultProperties; }
		}

		#endregion

		#region SupportsNotes

		public override bool SupportsNotes
		{
			get { return false; }
		}

		#endregion

		#endregion

		#region Parent

		public OrgSales Parent
		{
			get { return Sales; }
		}

		public override ZGuid PA_OW
		{
			get { return base.PA_OW; }
			set
			{
				if (base.PA_OW != value)
				{
					var previousSalesProductPk = Sales != null ? Sales.OW_MP_Product : ZGuid.Empty;
					base.PA_OW = value;
					var newSalesProductPk = Sales != null ? Sales.OW_MP_Product : ZGuid.Empty;

					if (ShouldDefaultProperties && previousSalesProductPk != newSalesProductPk)
					{
						SetDefaults(SalesProduct);
					}
				}
			}
		}

		void SetDefaults(IOrgSalesProduct salesProduct)
		{
			var salesProductCode = salesProduct != null ? salesProduct.MP_Code : ZString.Empty;

			var calculatedTradeMode = GetCalculatedTradeMode(salesProductCode);
			if (calculatedTradeMode.HasValue)
			{
				PA_TradeMode = calculatedTradeMode.Value.Value;
			}

			var calculatedTradeType = GetCalculatedTradeType(salesProductCode, PA_TradeMode, ProspectDetail.PAP_RC_NKContainer);
			if (calculatedTradeType.HasValue)
			{
				PA_TradeType = calculatedTradeType.Value.Value;
			}

			if (CurrentProspectPeriod.PAS_RX_NKCurrency.IsEmpty)
			{
				if (salesProductCode == SystemDefinedSalesProductList.Codes.LinerAgency)
				{
					CurrentProspectPeriod.PAS_RX_NKCurrency = Constants.CurrencyCodes.UnitedStates;
				}
				else
				{
					CurrentProspectPeriod.PAS_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				}
			}
		}

		public IOrgSalesProduct SalesProduct
		{
			get { return Sales != null ? Sales.Product : null; }
		}

		public ZString SalesProductCode
		{
			get
			{
				var salesProduct = SalesProduct;
				return salesProduct != null ? salesProduct.MP_Code : ZString.Empty;
			}
		}

		#endregion

		#region Prospect Detail

		public OrgTradeProspect ProspectDetail
		{
			get
			{
				if (IsActual)
				{
					return null;
				}
				else
				{
					if (prospectDetail == null)
					{
						LoadOrCreateProspectDetail();
						RegisterEditableChildObject(prospectDetail);
					}
					return prospectDetail;
				}
			}
		}
		protected OrgTradeProspect prospectDetail;

		protected virtual void LoadOrCreateProspectDetail()
		{
			prospectDetail = Factory.LoadTop1<OrgTradeProspect>(new ZQuery(OrgTradeProspectSchema.PAP_PA, PK));
			if (prospectDetail == null)
			{
				prospectDetail = Factory.New<OrgTradeProspect>();
				prospectDetail.PAP_PA = PK;
			}
		}

		#endregion

		#region Prospect Period

		public OrgTradePeriod CurrentProspectPeriod
		{
			get
			{
				if (IsActual)
				{
					return null;
				}
				else
				{
					if (currentProspectPeriod == null)
					{
						LoadOrCreateCurrentProspectPeriod();
						RegisterEditableChildObject(currentProspectPeriod);
					}
					return currentProspectPeriod;
				}
			}
		}
		protected OrgTradePeriod currentProspectPeriod;

		protected virtual void LoadOrCreateCurrentProspectPeriod()
		{
			var query = new ZQuery(OrgTradePeriodSchema.PAS_PA, PK);
			query.AddToFilter(OrgTradePeriodSchema.PAS_IsTraded, ZBool.False);
			query.AddToFilter(OrgTradePeriodSchema.PAS_Period, null);
			currentProspectPeriod = Factory.LoadTop1<OrgTradePeriod>(query);
			if (currentProspectPeriod == null)
			{
				currentProspectPeriod = Factory.New<OrgTradePeriod>();
				currentProspectPeriod.PAS_RX_NKCurrency = GetDefaultCurrency(Parent);
				currentProspectPeriod.PAS_PA = PK;
				currentProspectPeriod.PAS_OH_Client = Parent?.OW_OH_Primary ?? ZGuid.Empty;
			}
		}

		public OrgTradePeriodCollection ProspectPeriods
		{
			get
			{
				if (prospectPeriods == null)
				{
					prospectPeriods = new OrgTradePeriodCollection(this, defaultIsTraded: false);
					var additionalFilter = new ZQuery(OrgTradePeriodSchema.PAS_IsTraded, ZBool.False);
					additionalFilter.AddToFilter(OrgTradePeriodSchema.PAS_Period, SQLComparisonOperator.NotEqual, null);
					prospectPeriods.AdditionalFilter = additionalFilter;
				}
				return prospectPeriods;
			}
		}
		OrgTradePeriodCollection prospectPeriods;

		internal void UpdateProspectPeriods()
		{
			ProspectPeriods.DeleteAll();

			const int maxAllowedPeriodMonths = 24;
			if (ProspectPeriodStart.IsValid && ProspectPeriodEnd.IsValid)
			{
				int prospectPeriodMonthsCount = (ProspectPeriodEnd.Year - ProspectPeriodStart.Year) * 12 + ProspectPeriodEnd.Month - ProspectPeriodStart.Month + 1;
				prospectPeriodMonthsCount = Math.Min(maxAllowedPeriodMonths, prospectPeriodMonthsCount);
				if (prospectPeriodMonthsCount > 0)
				{
					var periodMonthStart = new ZDate(ProspectPeriodStart.Year, ProspectPeriodStart.Month, 1);
					var prospectPeriodMonthsCountIncludingForecast = IsOneoff ? prospectPeriodMonthsCount : Math.Max(prospectPeriodMonthsCount, 12);

					for (int i = 0; i < prospectPeriodMonthsCountIncludingForecast; i++)
					{
						var period = ProspectPeriods.AddNew();
						period.PAS_Period = periodMonthStart.AddMonths(i);
						period.PAS_IsForecast = (i >= prospectPeriodMonthsCount);
						period.PAS_OH_Client = CurrentProspectPeriod.PAS_OH_Client;
						period.PAS_RX_NKCurrency = CurrentProspectPeriod.PAS_RX_NKCurrency;

						var jobCount = Math.Max(1, CurrentProspectPeriod.PAS_RepeatsMnth);
						period.PAS_EstimatedProfit = OrgTradePeriod.CalculateEstimatedProfitPerPeriod(ProspectDetail.PAP_RecurrenceType, CurrentProspectPeriod.PAS_EstimatedProfit, jobCount, prospectPeriodMonthsCountIncludingForecast);
						period.PAS_TEUQuantity = OrgTradePeriod.CalculateEstimatedTEUQuantityPerPeriod(ProspectDetail.PAP_RecurrenceType, CurrentProspectPeriod.PAS_TEUQuantity, jobCount, prospectPeriodMonthsCountIncludingForecast);
					}

					var hasForecastPeriods = (prospectPeriodMonthsCountIncludingForecast > prospectPeriodMonthsCount) && !IsOneoff;
					if (hasForecastPeriods)
					{
						ProspectDetail.PAP_ForecastType = OrgTradeProspectForecastTypeList.Codes.Static;
						ProspectDetail.PAP_ExpiryDate = periodMonthStart.AddMonths(prospectPeriodMonthsCountIncludingForecast);
					}
					else
					{
						ProspectDetail.PAP_ForecastType = ZString.Empty;
						ProspectDetail.PAP_ExpiryDate = ZDate.Empty;
					}
				}
			}
		}

		#endregion

		#region Prospect Value Expiry

		public void SetProspectExpiry(ZDate expiryDate, ZString expiryReason)
		{
			var tradeProspect = ProspectDetail;
			if (IsCommitted && tradeProspect != null && tradeProspect.AllowManualExpiry)
			{
				bool anyPeriodExpired = false;
				foreach (var period in ProspectPeriods)
				{
					if (period.PAS_Period >= expiryDate)
					{
						period.PAS_IsExpired = true;
						anyPeriodExpired = true;
					}
				}

				if (anyPeriodExpired)
				{
					tradeProspect.PAP_ExpiryDate = expiryDate;
					tradeProspect.PAP_ExpiryReason = expiryReason;
				}
			}
		}

		public void UndoProspectExpiry()
		{
			var tradeProspect = ProspectDetail;
			if (IsExpired && tradeProspect != null && tradeProspect.AllowManualExpiry)
			{
				tradeProspect.PAP_ExpiryDate = ZDate.Empty;
				tradeProspect.PAP_ExpiryReason = ZString.Empty;
				foreach (var period in ProspectPeriods)
				{
					period.PAS_IsExpired = false;
				}
			}
		}

		public void UndoProspectSupersessionExpiry()
		{
			var tradeProspect = ProspectDetail;
			if (IsExpired && tradeProspect != null && tradeProspect.PAP_ExpiryReason == OrgTradeProspectExpiryReasonList.Codes.Superseded)
			{
				tradeProspect.PAP_ExpiryDate = ZDate.Empty;
				tradeProspect.PAP_ExpiryReason = ZString.Empty;
				foreach (var period in ProspectPeriods)
				{
					period.PAS_IsExpired = false;
				}
			}
		}

		#endregion

		#region Traded Period

		public OrgTradePeriodCollection TradedPeriods
		{
			get
			{
				if (tradedPeriods == null)
				{
					tradedPeriods = new OrgTradePeriodCollection(this, defaultIsTraded: true)
					{
						AdditionalFilter = new ZQuery(OrgTradePeriodSchema.PAS_IsTraded, ZBool.True)
					};
				}
				return tradedPeriods;
			}
		}
		OrgTradePeriodCollection tradedPeriods;

		#endregion

		#region ReadOnly

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			bool shouldBeReadOnly = false;
			if (Parent != null && Parent.ParentOrganisation != null)
			{
				shouldBeReadOnly = !Parent.ParentOrganisation.SecurityProvider.HasModifySalesTradeProfileSecurity;
			}

			return shouldBeReadOnly || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion

		#region Clone

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			if (!IsDeleted)
			{
				DeleteRelatedObjects();
				DeleteAllAssociations();
			}
			base.Delete();
		}

		void DeleteRelatedObjects()
		{
			if (!IsActual)
			{
				var prospectDetails = Factory.Load<OrgTradeProspect>(new ZQuery(OrgTradeProspectSchema.PAP_PA, PK));
				foreach (var prospect in prospectDetails)
				{
					prospect.Delete();
				}
			}

			var tradePeriods = Factory.Load<OrgTradePeriod>(new ZQuery(OrgTradePeriodSchema.PAS_PA, PK));
			foreach (var period in tradePeriods)
			{
				period.Delete();
			}
		}

		void DeleteAllAssociations()
		{
			if (!IsActual)
			{
				OrgSalesValueAssociationPivot.DeleteAll(this);
			}
		}

		#endregion

		#region Product Columns

		public CustomBusinessObject CustomProductFieldColumns
		{
			get
			{
				if (customProductFieldColumns == null)
				{
					var salesProduct = SalesProduct;
					customProductFieldColumns = (salesProduct != null) ? salesProduct.GetNewCustomTradeDetailFieldColumnsBusinessObject(this) : new CustomBusinessObject(this, new UserDefinedPropertyCollection(this));
				}

				return customProductFieldColumns;
			}
		}
		CustomBusinessObject customProductFieldColumns;

		#endregion

		#region Sales Association

		[ChildEditable]
		public SalesValueAssociationPivotCollection SalesAssociationPivotCollectionCompanyView
		{
			get
			{
				if (salesAssociationPivotCollectionCurrentCompany == null)
				{
					salesAssociationPivotCollectionCurrentCompany = GetNewSalesAssociationPivotCollectionCompanyView();
					RegisterEditableChildObject(salesAssociationPivotCollectionCurrentCompany);
				}
				return salesAssociationPivotCollectionCurrentCompany;
			}
		}
		SalesValueAssociationPivotCollection salesAssociationPivotCollectionCurrentCompany;

		public SalesValueAssociationPivotCollection SalesAssociationPivotCollectionGlobal
		{
			get
			{
				if (salesAssociationPivotCollectionGlobal == null)
				{
					salesAssociationPivotCollectionGlobal = GetNewSalesAssociationPivotCollectionGlobal();
				}
				return salesAssociationPivotCollectionGlobal;
			}
		}
		SalesValueAssociationPivotCollection salesAssociationPivotCollectionGlobal;

		public bool HasMultipleSalesAssociations
		{
			get { return SalesAssociationPivotCollectionGlobal.Where(x => x.SVP_ActivityTableCode != OrgHeaderSchema.Constants.Prefix).Skip(1).Any(); }
		}

		protected virtual SalesValueAssociationPivotCollection GetNewSalesAssociationPivotCollectionCompanyView()
		{
			return new SalesValueAssociationPivotCollection(this, true);
		}

		protected virtual SalesValueAssociationPivotCollection GetNewSalesAssociationPivotCollectionGlobal()
		{
			return new SalesValueAssociationPivotCollection(this, false);
		}

		#endregion

		#region IAuditDetails Members

		ZDateTime IAuditDetails.SystemCreateTimeUtc
		{
			get
			{
				IAuditDetails sales = Sales;
				if (sales == null)
				{
					return ZDateTime.Empty;
				}

				return sales.SystemCreateTimeUtc;
			}
		}

		ZString IAuditDetails.SystemCreateUser
		{
			get
			{
				IAuditDetails sales = Sales;
				if (sales == null)
				{
					return ZString.Empty;
				}

				return sales.SystemCreateUser;
			}
		}

		ZDateTime IAuditDetails.SystemLastEditTimeUtc
		{
			get { return ZDateTime.Empty; }
		}

		ZString IAuditDetails.SystemLastEditUser
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region IOrgTradeDetail Members

		ZBool IOrgTradeDetail.PA_RequiresPacking
		{
			get { return ProspectDetail?.PAP_RequiresPacking ?? ZBool.False; }
			set
			{
				if (ProspectDetail != null)
				{
					ProspectDetail.PAP_RequiresPacking = value;
				}
			}
		}

		#endregion
	}
}

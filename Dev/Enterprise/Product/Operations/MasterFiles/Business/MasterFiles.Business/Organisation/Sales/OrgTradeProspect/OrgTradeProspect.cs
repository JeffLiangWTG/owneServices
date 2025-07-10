using System;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MarketingManager.Integration;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgTradeProspect : AutoOrgTradeProspect
	{
		public OrgTradeProspect(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ConversionCertaintyLikertItemDescription = CertaintyLikertItemList.Descriptions._5ExtremelyLikely;
		}

		#endregion

		#region Load

		public override void OnLoaded()
		{
			base.OnLoaded();
			isPeriodOfActivityOverridden = !PAP_PeriodOfActivity.IsEmpty;
			isIndustryVerticalOverridden = !PAP_IndustryVertical.IsEmpty;
		}

		#endregion

		#region Properties

		OrgSales Sales => TradeDetail?.Sales;
		bool ShouldDefaultProperties => TradeDetail?.ShouldDefaultProperties ?? false;

		IOrgSalesProduct SalesProduct => TradeDetail?.SalesProduct;

		#region PAP_Density

		[List("Lookups.Densities")]
		public override ZString PAP_Density
		{
			get { return base.PAP_Density; }
			set { base.PAP_Density = value; }
		}

		protected bool PA_Density_ReadOnly
		{
			get
			{
				if (TradeDetail == null)
				{
					return false;
				}
				else
				{
					return TradeDetail.PA_TradeMode != Constants.TransportModes.Air;
				}
			}
		}

		#endregion

		#region PAP_IncoTradeTerm

		[List("Lookups.Incoterms")]
		public override ZString PAP_IncoTradeTerm
		{
			get { return base.PAP_IncoTradeTerm; }
			set { base.PAP_IncoTradeTerm = value; }
		}

		#endregion

		#region PAP_PeriodOfActivity

		[List("Lookups.ActivePeriodOfActivityTypes")]
		public override ZString PAP_PeriodOfActivity
		{
			get { return base.PAP_PeriodOfActivity; }
			set { base.PAP_PeriodOfActivity = value; }
		}

		#region OverallPeriodOfActivity

		[List("Lookups.ActivePeriodOfActivityTypes")]
		public ZString OverallPeriodOfActivity
		{
			get
			{
				if (IsPeriodOfActivityOverridden)
				{
					return PAP_PeriodOfActivity;
				}
				else
				{
					var sales = Sales;
					if (sales != null)
					{
						var fallbackOrgPk =
							!sales.OW_OH_Primary.IsEmpty ? sales.OW_OH_Primary :
							!sales.OW_OH_Buyer.IsEmpty ? sales.OW_OH_Buyer :
							!sales.OW_OH_Supplier.IsEmpty ? sales.OW_OH_Supplier :
							ZGuid.Empty;

						if (!fallbackOrgPk.IsEmpty)
						{
							var fallbackOrg = Factory.Load<OrgHeader>(fallbackOrgPk);
							if (fallbackOrg != null)
							{
								return fallbackOrg.MiscServ.OM_CMPeriodOfActivity;
							}
						}
					}

					return ZString.Empty;
				}
			}
			set
			{
				if (!IsPeriodOfActivityOverridden)
				{
					throw new InvalidOperationException("Cannot set OverallPeriodOfActivity while IsPeriodOfActivityOverridden is false");
				}

				PAP_PeriodOfActivity = value;
			}
		}

		public ZPropertyInfo OverallPeriodOfActivityInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(OverallPeriodOfActivity), x => PAP_PeriodOfActivityInfo); }
		}

		protected bool OverallPeriodOfActivity_ReadOnly
		{
			get { return !IsPeriodOfActivityOverridden; }
		}

		#endregion

		#region IsPeriodOfActivityOverridden

		public ZBool IsPeriodOfActivityOverridden
		{
			get { return isPeriodOfActivityOverridden; }
			set
			{
				if (isPeriodOfActivityOverridden != value)
				{
					SetNonPersistentPropertyValue(IsPeriodOfActivityOverriddenInfo, ref isPeriodOfActivityOverridden, value);
					PAP_PeriodOfActivity = ZString.Empty;
				}
			}
		}
		ZBool isPeriodOfActivityOverridden;

		public ZPropertyInfo IsPeriodOfActivityOverriddenInfo
		{
			get { return GetZPropertyInfo(nameof(IsPeriodOfActivityOverridden)); }
		}

		#endregion

		#endregion

		#region PAP_IndustryVertical

		[List("Lookups.ActiveIndustryVerticalTypes")]
		public override ZString PAP_IndustryVertical
		{
			get { return base.PAP_IndustryVertical; }
			set { base.PAP_IndustryVertical = value; }
		}

		#region OverallIndustryVertical

		[List("Lookups.ActiveIndustryVerticalTypes")]
		public ZString OverallIndustryVertical
		{
			get
			{
				if (IsIndustryVerticalOverridden)
				{
					return PAP_IndustryVertical;
				}
				else
				{
					var sales = Sales;
					if (sales != null)
					{
						var fallbackOrgPk =
							!sales.OW_OH_Primary.IsEmpty ? sales.OW_OH_Primary :
							!sales.OW_OH_Buyer.IsEmpty ? sales.OW_OH_Buyer :
							!sales.OW_OH_Supplier.IsEmpty ? sales.OW_OH_Supplier :
							ZGuid.Empty;

						if (!fallbackOrgPk.IsEmpty)
						{
							var fallbackOrg = Factory.Load<OrgHeader>(fallbackOrgPk);
							if (fallbackOrg != null)
							{
								return fallbackOrg.MiscServ.OM_CMIndustryVertical;
							}
						}
					}

					return ZString.Empty;
				}
			}
			set
			{
				if (!IsIndustryVerticalOverridden)
				{
					throw new InvalidOperationException("Cannot set OverallIndustryVertical while IsIndustryVerticalOverridden is false");
				}

				PAP_IndustryVertical = value;
			}
		}

		public ZPropertyInfo OverallIndustryVerticalInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(OverallIndustryVertical), x => PAP_IndustryVerticalInfo); }
		}

		protected bool OverallIndustryVertical_ReadOnly
		{
			get { return !IsIndustryVerticalOverridden; }
		}

		#endregion

		#region IsIndustryVerticalOverridden

		public ZBool IsIndustryVerticalOverridden
		{
			get { return isIndustryVerticalOverridden; }
			set
			{
				if (isIndustryVerticalOverridden != value)
				{
					SetNonPersistentPropertyValue(IsIndustryVerticalOverriddenInfo, ref isIndustryVerticalOverridden, value);
					PAP_IndustryVertical = ZString.Empty;
				}
			}
		}
		ZBool isIndustryVerticalOverridden;

		public ZPropertyInfo IsIndustryVerticalOverriddenInfo
		{
			get { return GetZPropertyInfo(nameof(IsIndustryVerticalOverridden)); }
		}

		#endregion

		#endregion

		#region PAP_RequiresPacking

		public override ZBool PAP_RequiresPacking
		{
			get { return base.PAP_RequiresPacking; }
			set
			{
				if (base.PAP_RequiresPacking != value)
				{
					base.PAP_RequiresPacking = value;

					var salesProduct = SalesProduct;
					if (salesProduct != null)
					{
						if (!salesProduct.IsContainerTypeAllowed(TradeDetail))
						{
							PAP_RC_NKContainer = ZString.Empty;
						}
					}
				}
			}
		}

		#endregion

		#region PAP_RC_NKContainer

		public override ZString PAP_RC_NKContainer
		{
			get { return base.PAP_RC_NKContainer; }
			set
			{
				if (base.PAP_RC_NKContainer != value)
				{
					base.PAP_RC_NKContainer = value;

					if (Container != null)
					{
						TradeDetail.CurrentProspectPeriod.PAS_Units = 1;
					}

					if (ShouldDefaultProperties)
					{
						TradeDetail.CurrentProspectPeriod.RecalculateTEUQuantity(Container);
						TradeDetail.SetDefaultTradeType();
					}
				}
			}
		}

		protected bool PAP_RC_NKContainer_ReadOnly
		{
			get { return SalesProduct != null && !SalesProduct.IsContainerTypeAllowed(TradeDetail); }
		}

		#endregion

		#region PAP_OH_ControllingAgent

		[List("Lookups.ControllingAgents")]
		public override ZGuid PAP_OH_ControllingAgent
		{
			get { return base.PAP_OH_ControllingAgent; }
			set { base.PAP_OH_ControllingAgent = value; }
		}

		#endregion

		#region PAP_OH_Competitor

		[List("Lookups.Competitors")]
		public override ZGuid PAP_OH_Competitor
		{
			get { return base.PAP_OH_Competitor; }
			set { base.PAP_OH_Competitor = value; }
		}

		#endregion

		#region PAP_OH_ServiceProvider

		[List("Lookups.ServiceProviders")]
		public override ZGuid PAP_OH_ServiceProvider
		{
			get { return base.PAP_OH_ServiceProvider; }
			set { base.PAP_OH_ServiceProvider = value; }
		}

		#endregion

		#region PAP_RS_NKServiceLevel

		[List("Lookups.ServiceLevels")]
		public override ZString PAP_RS_NKServiceLevel
		{
			get { return base.PAP_RS_NKServiceLevel; }
			set { base.PAP_RS_NKServiceLevel = value; }
		}

		#endregion

		#region PA_RH_NKCommodityCode

		[List("Lookups.CommodityCodes")]
		public override ZString PAP_RH_NKCommodityCode
		{
			get { return base.PAP_RH_NKCommodityCode; }
			set
			{
				if (base.PAP_RH_NKCommodityCode != value)
				{
					base.PAP_RH_NKCommodityCode = value;

					var commodity = CommodityCode;
					if (commodity != null)
					{
						PAP_IsDangerous = commodity.RH_IsHazardous || commodity.RH_IsFlammable;
						PAP_RequiresTemperatureControl = commodity.RH_IsPerishable;
					}
				}
			}
		}

		#endregion

		#region ConversionCertaintyLikertItem

		[List("Lookups.ConversionCertaintyLikertInverseItems")]
		public ZString ConversionCertaintyLikertItemDescription
		{
			get { return CertaintyLikertItemHelper.ToItem(PAP_ConversionCertainty).Description; }
			set
			{
				var itemCode = Lookups.ConversionCertaintyLikertInverseItems.GetDescriptionFromCode(value);
				PAP_ConversionCertainty = CertaintyLikertItemHelper.ToPercentage(itemCode);
			}
		}

		public ZPropertyInfo ConversionCertaintyLikertItemDescriptionInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(ConversionCertaintyLikertItemDescription), x => PAP_ConversionCertaintyInfo); }
		}

		CertaintyLikertItemHelper CertaintyLikertItemHelper
		{
			get { return certaintyLikertItemHelper ?? (certaintyLikertItemHelper = new CertaintyLikertItemHelper()); }
		}
		CertaintyLikertItemHelper certaintyLikertItemHelper;

		#endregion

		#region PAP_RecurrenceType

		[List("Lookups.RecurrenceTypes")]
		public override ZString PAP_RecurrenceType
		{
			get { return base.PAP_RecurrenceType; }
			set
			{
				if (base.PAP_RecurrenceType != value)
				{
					base.PAP_RecurrenceType = value;
					SetProspectPeriodRange();
					UpdateProspectPeriodsValue();

					var sales = Sales;
					if (sales != null && !sales.IsActual)
					{
						sales.RefreshTotalAnnualStats();
					}
				}
			}
		}

		internal void SetProspectPeriodRange()
		{
			var tradeDetail = TradeDetail;
			if (tradeDetail != null && tradeDetail.IsCommitted)
			{
				if (PAP_ExpectedTradeStartDate.IsValid)
				{
					tradeDetail.ProspectPeriodStart = new ZDate(PAP_ExpectedTradeStartDate.Year, PAP_ExpectedTradeStartDate.Month, 1);

					if (tradeDetail.ProspectPeriodEndType.IsEmpty)
					{
						var periodMonths = GetDefaultProspectPeriodMonths();
						if (periodMonths == 1)
						{
							tradeDetail.ProspectPeriodEndType = OrgTradeProspectPeriodEndTypeList.Codes._1Month;
						}
						else if (periodMonths == 12)
						{
							tradeDetail.ProspectPeriodEndType = OrgTradeProspectPeriodEndTypeList.Codes._12Months;
						}
					}
				}
				else if (PAP_ExpectedTradeStartDate.IsEmpty)
				{
					tradeDetail.ProspectPeriodStart = ZDate.Empty;
					tradeDetail.ProspectPeriodEnd = ZDate.Empty;
					tradeDetail.ProspectPeriodEndType = ZString.Empty;
				}
			}
		}

		void UpdateProspectPeriodsValue()
		{
			var tradeDetail = TradeDetail;
			if (tradeDetail != null)
			{
				var jobCount = Math.Max(1, tradeDetail.CurrentProspectPeriod.PAS_RepeatsMnth);
				var estimatedProfit = tradeDetail.CurrentProspectPeriod.PAS_EstimatedProfit;
				foreach (var period in tradeDetail.ProspectPeriods)
				{
					period.PAS_EstimatedProfit = OrgTradePeriod.CalculateEstimatedProfitPerPeriod(PAP_RecurrenceType, estimatedProfit, jobCount, tradeDetail.ProspectPeriods.Count);
				}
			}
		}

		#endregion

		#region ExpectedTradeStartDate

		public override ZDate PAP_ExpectedTradeStartDate
		{
			get => base.PAP_ExpectedTradeStartDate;
			set
			{
				if (base.PAP_ExpectedTradeStartDate != value)
				{
					base.PAP_ExpectedTradeStartDate = value;
					SetProspectPeriodRange();
				}
			}
		}

		int GetDefaultProspectPeriodMonths()
		{
			switch (PAP_RecurrenceType)
			{
				case OrgTradeProspectRecurrenceTypeList.Codes.OneOff:
					return 1;
				case OrgTradeProspectRecurrenceTypeList.Codes.Weekly:
				case OrgTradeProspectRecurrenceTypeList.Codes.Monthly:
				case OrgTradeProspectRecurrenceTypeList.Codes.Yearly:
					return 12;
				default:
					return 1;
			}
		}

		public bool PAP_ExpectedTradeStartDate_ReadOnly => TradeDetail.IsSuperceded;

		#endregion

		#region Expiry

		[List("Lookups.ExpiryReasons")]
		public override ZString PAP_ExpiryReason
		{
			get => base.PAP_ExpiryReason;
			set => base.PAP_ExpiryReason = value;
		}

		public bool AllowManualExpiry => PAP_ExpiryReason != OrgTradeProspectExpiryReasonList.Codes.Superseded && PAP_ExpiryReason != OrgTradeProspectExpiryReasonList.Codes.Lost;

		#endregion

		#region SupportsNotes

		public override bool SupportsNotes => false;

		#endregion

		#endregion

		#region ReadOnly

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			bool shouldBeReadOnly = false;
			if (Sales?.ParentOrganisation != null)
			{
				shouldBeReadOnly = !Sales.ParentOrganisation.SecurityProvider.HasModifySalesTradeProfileSecurity;
			}

			return shouldBeReadOnly || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion
	}
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class EntitySalesWrapper : OrgSales
	{
		#region Schema

		public new class Schema : OrgSales.Schema
		{
			public const string TotalTradedValue = "TotalTradedValue";
			public const string TotalTradedTEUQuantity = "TotalTradedTEUQuantity";
			public const string TotalEstimatedAnnualValue = "TotalEstimatedAnnualValue";
			public const string CommittedAnnualValue = "CommittedAnnualValue";
			public const string CommittedAnnualTEUQuantity = "CommittedAnnualTEUQuantity";
			public const string CommittedMonthlyValue = "CommittedMonthlyValue";
			public const string PipelineValue = "PipelineValue";
			public const string PipelineTEUQuantity = "PipelineTEUQuantity";
			public const string UnsuccessfulValue = "UnsuccessfulValue";
			public const string UnsuccessfulTEUQuantity = "UnsuccessfulTEUQuantity";
			public const string EntityExchangeRate = "EntityExchangeRate";
			public const string TotalRevenueCurrencyCode = "TotalRevenueCurrencyCode";
		}

		#endregion

		#region Get / Constructor

		public static EntitySalesWrapper Get(OrgSales sales, ISalesValueAssociatedEntity entity)
		{
			Argument.NotNull(sales, "sales");
			Argument.NotNull(entity, "entity");

			var result = sales.Factory.Load<EntitySalesWrapper>(sales.PK);
			result.Entity = entity;
			return result;
		}

		public EntitySalesWrapper(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#endregion

		#region Fields

		public ISalesValueAssociatedEntity Entity
		{
			get { return entity; }
			set
			{
				if (entity != value)
				{
					entity = value;
					revenueCalculator = null;
					actualsInformation = null;
				}
			}
		}
		ISalesValueAssociatedEntity entity;

		public bool EntityIsOrg
		{
			get { return Entity is OrgHeader; }
		}

		public OrgHeader Org
		{
			get
			{
				var org = Entity as OrgHeader;
				if (org != null)
				{
					return org;
				}

				return Entity.OrgPkInfo != null ? Factory.Load<OrgHeader>((ZGuid)Entity.OrgPkInfo.Value) : null;
			}
		}

		#endregion

		#region Properties

		#region Total Traded Value

		[ResourceStringData("Enterprise.MarketingManager.EntitySalesWrapper|TotalTradedValue", Caption = "Revenue")]
		public ZDecimal TotalTradedValue => ActualsInformation.ActualsAnnualTotal;
		public ZPropertyInfo TotalTradedValueInfo => GetZPropertyInfo(Schema.TotalTradedValue);

		#endregion

		#region Total Traded TEU Quantity

		[ResourceStringData("Enterprise.MarketingManager.EntitySalesWrapper|TotalTradedTEUQuantity", Caption = "Actual TEU (c.f.y)")]
		public ZDecimal TotalTradedTEUQuantity => ActualsInformation.ActualsAnnualTEUTotalQuantity;
		public ZPropertyInfo TotalTradedTEUQuantityInfo => GetZPropertyInfo(Schema.TotalTradedTEUQuantity);

		#endregion

		#region Total Estimated Annual Value

		[ResourceStringData("Enterprise.MarketingManager.EntitySalesWrapper|TotalEstimatedAnnualValue", ShortCaption = "Total Est Value (p.a)", Caption = "Total Estimated Value (p.a)")]
		public ZDecimal TotalEstimatedAnnualValue => RevenueCalculator.GetTotalInEntityCurrency(EntityTradeDetailsCompanyView.Cast<EntityTradeDetailWrapper>().Where(x => x.IsCommitted || x.IsPipeline), x => x.CurrencyCode, x => x.PA_Calc_EstimatedAnnualValue);
		public ZPropertyInfo TotalEstimatedAnnualValueInfo => GetZPropertyInfo(Schema.TotalEstimatedAnnualValue);

		#endregion

		#region Committed Annual Value

		[ResourceStringData("Enterprise.MarketingManager.EntitySalesWrapper|CommittedAnnualValue", Caption = "Committed (p.a)")]
		public ZDecimal CommittedAnnualValue => RevenueCalculator.GetTotalInEntityCurrency(EntityTradeDetailsCompanyView.Cast<EntityTradeDetailWrapper>().Where(x => x.IsCommitted), x => x.CurrencyCode, x => x.PA_Calc_CommittedValue);
		public ZPropertyInfo CommittedAnnualValueInfo => GetZPropertyInfo(Schema.CommittedAnnualValue);

		#endregion

		#region Committed Annual TEU Quantity

		[ResourceStringData("Enterprise.MarketingManager.EntitySalesWrapper|CommittedAnnualTEUQuantity", Caption = "Committed TEU (p.a)")]
		public ZDecimal CommittedAnnualTEUQuantity => EntityTradeDetailsCompanyView.Cast<EntityTradeDetailWrapper>().Where(x => x.IsCommitted).Sum(x => x.PA_Calc_AnnualTEU);
		public ZPropertyInfo CommittedAnnualTEUQuantityInfo => GetZPropertyInfo(Schema.CommittedAnnualTEUQuantity);

		#endregion

		#region Committed Monthly Value

		[ResourceStringData("Enterprise.MarketingManager.EntitySalesWrapper|CommittedMonthlyValue", Caption = "Committed m/avg (p.a)")]
		public ZDecimal CommittedMonthlyValue => CommittedAnnualValue / 12;
		public ZPropertyInfo CommittedMonthlyValueInfo => GetZPropertyInfo(Schema.CommittedMonthlyValue);

		#endregion

		#region Pipeline Value

		[ResourceStringData("Enterprise.MarketingManager.EntitySalesWrapper|PipelineAnnualValue", Caption = "Pipeline (p.a)")]
		public ZDecimal PipelineValue => RevenueCalculator.GetTotalInEntityCurrency(EntityTradeDetailsCompanyView.Cast<EntityTradeDetailWrapper>().Where(x => x.IsPipeline), x => x.CurrencyCode, x => x.PA_Calc_EstimatedPipelineAnnualValue);
		public ZPropertyInfo PipelineValueInfo => GetZPropertyInfo(Schema.PipelineValue);

		#endregion

		#region Pipeline TEU Quantity

		[ResourceStringData("Enterprise.MarketingManager.EntitySalesWrapper|PipelineAnnualTEUQuantity", Caption = "Pipeline TEU (p.a)")]
		public ZDecimal PipelineTEUQuantity => EntityTradeDetailsCompanyView.Cast<EntityTradeDetailWrapper>().Where(x => x.IsPipeline).Sum(x => x.PA_Calc_AnnualTEU);
		public ZPropertyInfo PipelineTEUQuantityInfo => GetZPropertyInfo(Schema.PipelineTEUQuantity);

		#endregion

		#region Unsuccessful Value

		[ResourceStringData("Enterprise.MarketingManager.EntitySalesWrapper|UnsuccessfulAnnualValue", Caption = "Unsuccessful (p.a)")]
		public ZDecimal UnsuccessfulValue => RevenueCalculator.GetTotalInEntityCurrency(EntityTradeDetailsCompanyView.Cast<EntityTradeDetailWrapper>().Where(x => x.IsUnsuccessful), x => x.CurrencyCode, x => x.PA_Calc_EstimatedPipelineAnnualValue);
		public ZPropertyInfo UnsuccessfulValueInfo => GetZPropertyInfo(Schema.UnsuccessfulValue);

		#endregion

		#region Unsuccessful TEU Quantity

		[ResourceStringData("Enterprise.MarketingManager.EntitySalesWrapper|UnsuccessfulAnnualTEUQuantity", Caption = "Unsuccessful TEU (p.a)")]
		public ZDecimal UnsuccessfulTEUQuantity => EntityTradeDetailsCompanyView.Cast<EntityTradeDetailWrapper>().Where(x => x.IsUnsuccessful).Sum(x => x.PA_Calc_AnnualTEU);
		public ZPropertyInfo UnsuccessfulTEUQuantityInfo => GetZPropertyInfo(Schema.UnsuccessfulTEUQuantity);

		#endregion

		#region TotalRevenueCurrencyCode

		[List("Lookups.Currencies")]
		[MaxLength(OrgSales.Schema.OW_RX_NKRevenueCurrencyMaxLength)]
		[ResourceStringData("Enterprise.MarketingManager.EntitySalesWrapper|TotalRevenueCurrencyCode", ShortCaption = "CUR", Caption = "Currency")]
		public ZString TotalRevenueCurrencyCode
		{
			get { return OW_IsCustomRevenue ? OW_RX_NKRevenueCurrency : CalculatedRevenueCurrencyCode; }
			set
			{
				if (OW_IsCustomRevenue)
				{
					OW_RX_NKRevenueCurrency = value;
					TotalRevenueCurrencyCodeInfo.RefreshBinding();
				}
				else
				{
					throw new InvalidOperationException("Should not be editable if not custom revenue");
				}
			}
		}

		public ZPropertyInfo TotalRevenueCurrencyCodeInfo
		{
			get { return GetZPropertyInfo(Schema.TotalRevenueCurrencyCode); }
		}

		protected bool TotalRevenueCurrencyCode_ReadOnly
		{
			get { return !OW_IsCustomRevenue; }
		}

		public RefCurrency TotalRevenueCurrency
		{
			get { return Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, TotalRevenueCurrencyCode); }
		}

		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public ZString CalculatedRevenueCurrencyCode
		{
			get { return RevenueCalculator.EntityCurrencyCode; }
		}

		[ResourceStringData("Enterprise.MarketingManager.EntitySalesWrapper|DateForExchangeRate", Caption = "Exchange Rate Date")]
		public ZDateTime DateForExchangeRate
		{
			get { return RevenueCalculator.DateForExchangeRate; }
		}

		public GlbCompany CompanyForExchangeRate
		{
			get { return RevenueCalculator.CompanyForExchangeRate; }
		}

		public IEnumerable<RefCurrency> GetMissingExchangeRates()
		{
			return RevenueCalculator.GetMissingExchangeRatesToConvertTo(TotalRevenueCurrency);
		}

		#endregion

		#region EntityExchangeRate

		[ResourceStringData("Enterprise.MarketingManager.EntitySalesWrapper|EntityExchangeRate", ShortCaption = "Ex. Rate", Caption = "Exchange Rate")]
		[DecimalPlaces(4)]
		public ZDecimal EntityExchangeRate
		{
			get
			{
				if (TotalRevenueCurrencyCode == RevenueCalculator.EntityCurrencyCode)
				{
					return 1m;
				}

				return RevenueCalculator.GetExchangeRate(TotalRevenueCurrency);
			}
		}

		public ZPropertyInfo EntityExchangeRateInfo
		{
			get { return GetZPropertyInfo(Schema.EntityExchangeRate); }
		}

		#endregion

		#region ReadOnly

		public override bool ReadOnly
		{
			get => base.ReadOnly || !IsEditable;
			set
			{
				base.ReadOnly = value;
				EntityDetailGroupings?.SetReadOnlyIncludingChildren(value);
			}
		}

		#endregion

		#region Company Filter

		public ZGuid CompanyFilter
		{
			get => companyFilter;
			set
			{
				if (IsFilteredByCompany && companyFilter != value)
				{
					companyFilter = value;
					if (detailsCompanyView != null)
					{
						EntityTradeDetailsCompanyView.Rebuild();
						if (entityDetailGroupings != null)
						{
							var entityDetailGroupingsReadOnly = entityDetailGroupings.ReadOnly;
							entityDetailGroupings = null;
							EntityDetailGroupings.SetReadOnlyIncludingChildren(entityDetailGroupingsReadOnly);
						}
					}
					if (entityAssociationPivotCollection != null)
					{
						SalesAssociationPivotCollectionCompanyView.RefreshFromDb();
					}
				}
			}
		}
		ZGuid companyFilter;

		bool IsFilteredByCompany => entity is OrgHeader;

		#endregion

		#region Prospect Companies

		public List<ZGuid> ProspectCompanyPks
		{
			get => prospectCompanyPks ?? (prospectCompanyPks = new List<ZGuid>(0));
			set => prospectCompanyPks = value;
		}
		List<ZGuid> prospectCompanyPks;

		#endregion

		protected override IEnumerable<OrgTradeDetail> TradeDetailsToIncludeInTotal
		{
			get { return EntityTradeDetailsCompanyView.Cast<OrgTradeDetail>(); }
		}

		public void RefreshCalculatedValues()
		{
			TotalTradedValueInfo.RefreshBinding();
			ActualsInformation.CommittedYearToDateValueInfo.RefreshBinding();
			ActualsInformation.CommittedYearToDateTEUQuantityInfo.RefreshBinding();
			ActualsInformation.CommittedCurrentYearValueInfo.RefreshBinding();
			ActualsInformation.CommittedCurrentYearTEUQuantityInfo.RefreshBinding();
			ActualsInformation.CommittedAndForecastCurrentYearValueInfo.RefreshBinding();
			ActualsInformation.CommittedAndForecastCurrentYearTEUQuantityInfo.RefreshBinding();
			ActualsInformation.CommittedNextYearValueInfo.RefreshBinding();
			ActualsInformation.CommittedNextYearTEUQuantityInfo.RefreshBinding();
			ActualsInformation.CommittedAndForecastNextYearValueInfo.RefreshBinding();
			ActualsInformation.CommittedAndForecastNextYearTEUQuantityInfo.RefreshBinding();
		}

		#endregion

		#region RevenueCalculator

		public OrgSalesRevenueCalculator RevenueCalculator
		{
			get
			{
				if (revenueCalculator == null)
				{
					revenueCalculator = new OrgSalesRevenueCalculator(Factory, Entity);
				}

				return revenueCalculator;
			}
		}

		OrgSalesRevenueCalculator revenueCalculator;

		#endregion

		#region RevertSupersededPeriodsForDetachedSales

		public List<OrgTradeDetail> RevertSupersededPeriodsForDetachedSales()
		{
			var successfulDetailsBeingDetached = EntityTradeDetailsCollection.Cast<EntityTradeDetailWrapper>().Where(x => x.PA_Status == OpportunityTradeStatus.Codes.Successful).ToArray();
			var detailsToRevert = FindTradeDetailsToRevert(successfulDetailsBeingDetached);
			RevertDetails(detailsToRevert);
			return detailsToRevert.Select(x => x.Item1).ToList();
		}

		List<Tuple<OrgTradeDetail, ZDate, ZDate>> FindTradeDetailsToRevert(EntityTradeDetailWrapper[] successfulDetailsBeingDetached)
		{
			var detailsToRevert = new List<Tuple<OrgTradeDetail, ZDate, ZDate>>();
			foreach (var entityDetailWrapper in successfulDetailsBeingDetached)
			{
				var detachedPeriodTo = entityDetailWrapper.ProspectPeriods.Max(x => x.PAS_Period);
				var detachedPeriodFrom = entityDetailWrapper.ProspectPeriods.Min(x => x.PAS_Period);
				var latestSupersededDetails = FindDetailsWithLatestPeriod(entityDetailWrapper, detachedPeriodFrom, detachedPeriodTo);
				OrgTradeDetail detailToRevert = null;

				if (latestSupersededDetails.Count > 1)
				{
					detailToRevert = FindDetailWithLatestOpportunityClosedDate(latestSupersededDetails);
				}
				else if (latestSupersededDetails.Count == 1)
				{
					detailToRevert = latestSupersededDetails[0];
				}

				if (detailToRevert != null)
				{
					var revertInfo = new Tuple<OrgTradeDetail, ZDate, ZDate>(detailToRevert, detachedPeriodFrom, detachedPeriodTo);
					detailsToRevert.Add(revertInfo);
				}
			}
			return detailsToRevert;
		}

		List<OrgTradeDetail> FindDetailsWithLatestPeriod(EntityTradeDetailWrapper detachedEntityDetailWrapper, ZDate detachedPeriodFrom, ZDate detachedPeriodTo)
		{
			var latestSupersededDetails = new List<OrgTradeDetail>();
			var latestMatchedDate = ZDate.Empty;
			var nonEntityDetails = TradeDetails.Cast<OrgTradeDetail>().Where
			(
				x => x.PK != detachedEntityDetailWrapper.PK
				&& x.PA_Status == OpportunityTradeStatus.Codes.Successful
				&& x.PA_TradeMode == detachedEntityDetailWrapper.PA_TradeMode
				&& x.PA_TradeType == detachedEntityDetailWrapper.PA_TradeType).ToArray();

			foreach (var detail in nonEntityDetails)
			{
				var detailMatchedPeriods = detail.ProspectPeriods.Where(p => p.PAS_Period >= detachedPeriodFrom && p.PAS_Period <= detachedPeriodTo && p.PAS_IsSuperseded);
				var detailLatestPeriodDate = detailMatchedPeriods.Any() ? detailMatchedPeriods.Max(p => p.PAS_Period) : ZDate.Empty;

				if (latestMatchedDate.IsEmpty || latestMatchedDate < detailLatestPeriodDate)
				{
					latestSupersededDetails.Clear();
					latestSupersededDetails.Add(detail);
					latestMatchedDate = detailLatestPeriodDate;
				}
				else if (latestMatchedDate == detailLatestPeriodDate)
				{
					latestSupersededDetails.Add(detail);
				}
			}

			return latestSupersededDetails;
		}

		static OrgTradeDetail FindDetailWithLatestOpportunityClosedDate(List<OrgTradeDetail> latestSupersededDetails)
		{
			var latestOpportunityClosedDate = ZDate.Empty;
			var detailsWithLatestOpportunityClosedDate = new List<OrgTradeDetail>(latestSupersededDetails.Count);

			foreach (var detail in latestSupersededDetails)
			{
				var opportunityAssociation = detail.SalesAssociationPivotCollectionGlobal.FirstOrDefault(x => x.AssociatedEntity.EntityType == RelatableActivityTypeList.Codes.OpportunityManager);
				if (opportunityAssociation != null)
				{
					var opportunity = detail.Factory.LoadFromUniqueKey<OrgOpportunity>(OrgOpportunitySchema.P8_OpportunityID, opportunityAssociation.AssociatedEntity.ID);
					if (opportunity != null)
					{
						if (latestOpportunityClosedDate.IsEmpty || !opportunity.P8_ClosedDate.IsEmpty && latestOpportunityClosedDate < opportunity.P8_ClosedDate.Date)
						{
							detailsWithLatestOpportunityClosedDate.Clear();
							detailsWithLatestOpportunityClosedDate.Add(detail);
							latestOpportunityClosedDate = opportunity.P8_ClosedDate.Date;
						}
						else if (latestOpportunityClosedDate == opportunity.P8_ClosedDate.Date)
						{
							detailsWithLatestOpportunityClosedDate.Add(detail);
						}
					}
				}
			}

			return detailsWithLatestOpportunityClosedDate.FirstOrDefault();
		}
		void RevertDetails(List<Tuple<OrgTradeDetail, ZDate, ZDate>> revertInfoList)
		{
			foreach (var revertInfo in revertInfoList)
			{
				var detailToRevert = revertInfo.Item1;
				var periodFrom = revertInfo.Item2;
				var periodTo = revertInfo.Item3;

				foreach (var period in detailToRevert.ProspectPeriods)
				{
					if (period.PAS_Period >= periodFrom && period.PAS_Period <= periodTo)
					{
						period.PAS_IsSuperseded = false;
					}
				}
				if (!detailToRevert.ProspectPeriods.Any(x => x.PAS_IsSuperseded))
				{
					detailToRevert.UndoProspectSupersessionExpiry();
				}
			}
		}

		#endregion

		#region Related Business Objects

		public IEnumerable<EntityTradeDetailWrapper> EntityTradeDetails
		{
			get { return EntityTradeDetailsCollection.Cast<EntityTradeDetailWrapper>(); }
		}

		[ChildEditable]
		public EntityTradeDetailWrapperCollection EntityTradeDetailsCollection
		{
			get
			{
				if (entityTradeDetails == null)
				{
					entityTradeDetails = new EntityTradeDetailWrapperCollection(this);
					entityTradeDetails.Load();
					RegisterEditableChildObject(entityTradeDetails);
				}

				return entityTradeDetails;
			}
		}
		EntityTradeDetailWrapperCollection entityTradeDetails;

		public EntityTradeDetailWrapperCollectionCompanyView EntityTradeDetailsCompanyView
		{
			get { return detailsCompanyView ?? (detailsCompanyView = new EntityTradeDetailWrapperCollectionCompanyView(EntityTradeDetailsCollection)); }
		}
		EntityTradeDetailWrapperCollectionCompanyView detailsCompanyView;

		public OrgSalesActualsInformation ActualsInformation
		{
			get { return actualsInformation ?? (actualsInformation = new OrgSalesActualsInformation(this, Org)); }
		}
		OrgSalesActualsInformation actualsInformation;

		[BusinessObjectTestExclude]
		public OrgTradeDetailJobCommonGroupingCollection EntityDetailGroupings
		{
			get
			{
				if (entityDetailGroupings == null && Product != null)
				{
					entityDetailGroupings = new OrgTradeDetailJobCommonGroupingCollection(EntityTradeDetailsCompanyView, (OrgSalesProduct)Product);
					entityDetailGroupings.PopulateDefaultElements();
				}
				return entityDetailGroupings;
			}
		}
		OrgTradeDetailJobCommonGroupingCollection entityDetailGroupings;

		public bool HasGroupings => entityDetailGroupings != null;

		[ChildEditable]
		public override SalesValueAssociationPivotCollection SalesAssociationPivotCollectionCompanyView
		{
			get
			{
				if (entityAssociationPivotCollection == null)
				{
					entityAssociationPivotCollection = new EntitySalesAssociationPivotCollection(this);
					RegisterEditableChildObject(entityAssociationPivotCollection);
				}
				return entityAssociationPivotCollection;
			}
		}
		EntitySalesAssociationPivotCollection entityAssociationPivotCollection;

		#endregion

		#region Delete

		public override void Delete()
		{
			EntityTradeDetailsCollection.RemoveAndDeleteAll();

			base.Delete();
		}

		#endregion

		#region Validation

		public new EntitySalesWrapperValidation Validation
		{
			get { return (EntitySalesWrapperValidation)base.Validation; }
		}

		protected override OrgSalesValidation GetNewValidation()
		{
			return new EntitySalesWrapperValidation(this);
		}

		#endregion

		#region FetchStrategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new EntitySalesWrapperFetchStrategy(this);
		}

		#endregion
	}
}

using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class EntityTradeDetailWrapper : OrgTradeDetail
	{
		#region Schema

		public new class Schema : OrgTradeDetail.Schema
		{
			public const string EntityCurrencyExchangeRate = "EntityCurrencyExchangeRate";
		}

		#endregion

		#region Get / Constructor

		public static EntityTradeDetailWrapper Get(OrgTradeDetail tradeDetail, ISalesValueAssociatedEntity entity)
		{
			Argument.NotNull(tradeDetail, "tradeDetail");
			Argument.NotNull(entity, "entity");

			var result = tradeDetail.Factory.Load<EntityTradeDetailWrapper>(tradeDetail.PK);
			if (result != null)
			{
				result.Entity = entity;
			}
			return result;
		}

		public EntityTradeDetailWrapper(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#endregion

		#region Fields

		public ISalesValueAssociatedEntity Entity
		{
			get;
			internal set;
		}

		ZGuid EntityPk
		{
			get { return Entity != null ? Entity.Identifier : ZGuid.Invalid; }
		}

		#endregion

		protected override SalesValueAssociationPivotCollection GetNewSalesAssociationPivotCollectionCompanyView()
		{
			return new TradeDetailAssociationPivotCollection(this, true, Parent?.ProductCode);
		}

		protected override SalesValueAssociationPivotCollection GetNewSalesAssociationPivotCollectionGlobal()
		{
			return new TradeDetailAssociationPivotCollection(this, false, Parent?.ProductCode);
		}

		#region Related Business Objects

		protected override void LoadOrCreateProspectDetail()
		{
			prospectDetail = Factory.LoadTop1<EntityTradeProspect>(new ZQuery(OrgTradeProspectSchema.PAP_PA, PK));
			if (prospectDetail == null)
			{
				prospectDetail = Factory.New<EntityTradeProspect>();
				prospectDetail.PAP_PA = PK;
			}
		}

		protected override void LoadOrCreateCurrentProspectPeriod()
		{
			var query = new ZQuery(OrgTradePeriodSchema.PAS_PA, PK);
			query.AddToFilter(OrgTradePeriodSchema.PAS_Period, null);
			currentProspectPeriod = Factory.LoadTop1<EntityTradePeriod>(query);
			if (currentProspectPeriod == null)
			{
				currentProspectPeriod = Factory.New<EntityTradePeriod>();
				currentProspectPeriod.PAS_PA = PK;

				var salesWrapper = Factory.Load<EntitySalesWrapper>(PA_OW);
				currentProspectPeriod.PAS_OH_Client = salesWrapper?.OW_OH_Primary ?? ZGuid.Empty;
				currentProspectPeriod.PAS_RX_NKCurrency = GetDefaultCurrency(salesWrapper);
			}
			RegisterListChangedCalledRefreshBinding(currentProspectPeriod);
		}

		#endregion

		#region Properties

		#region ParentEntitySales

		public new EntitySalesWrapper Parent
		{
			get { return (EntitySalesWrapper)base.Parent; }
		}

		public override OrgSales Sales
		{
			get
			{
				if (IsDeleted)
				{
					return parentEntitySalesCache != null ? parentEntitySalesCache.Item3 : null;
				}
				else if (parentEntitySalesCache == null || parentEntitySalesCache.Item1 != PA_OW || parentEntitySalesCache.Item2 != EntityPk || parentEntitySalesCache.Item3 == null)
				{
					var parent = base.Sales;
					var entitySales = parent != null && Entity != null ? EntitySalesWrapper.Get(parent, Entity) : null;
					parentEntitySalesCache = Tuple.Create(PA_OW, EntityPk, entitySales);
				}

				return parentEntitySalesCache.Item3;
			}
		}
		Tuple<ZGuid, ZGuid, EntitySalesWrapper> parentEntitySalesCache;

		#endregion

		#region TradeType

		[List("Lookups.TradeTypes")]
		[ResourceStringData("Enterprise.MarketingManager.EntityTradeDetailWrapper|TradeType", Caption = "Type")]
		public ZString TradeType
		{
			get { return base.PA_TradeType; }
			set
			{
				if (base.PA_TradeType != value)
				{
					base.PA_TradeType = value;
					RefreshEstimatedTotalsIfUsingCalculated(Parent);
				}
			}
		}

		public ZWrappedPropertyInfo TradeTypeInfo => GetWrappedZPropertyInfo(nameof(TradeType), x => PA_TradeTypeInfo);

		#endregion

		#region CurrencyCode

		[List("CurrentProspectPeriod.Lookups.Currencies")]
		[ResourceStringData("Enterprise.MarketingManager.EntityTradeDetailWrapper|CurrencyCode", Caption = "Currency")]
		public ZString CurrencyCode
		{
			get { return CurrentProspectPeriod.PAS_RX_NKCurrency; }
			set
			{
				if (CurrentProspectPeriod.PAS_RX_NKCurrency != value)
				{
					CurrentProspectPeriod.PAS_RX_NKCurrency = value;
					RefreshEstimatedTotalsIfUsingCalculated(Parent);
				}
			}
		}

		public ZWrappedPropertyInfo CurrencyCodeInfo => GetWrappedZPropertyInfo(nameof(CurrencyCode), x => CurrentProspectPeriod.PAS_RX_NKCurrencyInfo);

		#endregion

		#region JobCount

		[ResourceStringData("Enterprise.MarketingManager.EntityTradeDetailWrapper|JobCount", ShortCaption = "Jobs", Caption = "Job Count")]
		public ZDecimal JobCount
		{
			get { return CurrentProspectPeriod.PAS_RepeatsMnth; }
			set
			{
				if (CurrentProspectPeriod.PAS_RepeatsMnth != value)
				{
					CurrentProspectPeriod.PAS_RepeatsMnth = value;
					RefreshEstimatedTotalsIfUsingCalculated(Parent);
				}
			}
		}

		public ZWrappedPropertyInfo JobCountInfo => GetWrappedZPropertyInfo(nameof(JobCount), x => CurrentProspectPeriod.PAS_RepeatsMnthInfo);

		#endregion

		#region EstimatedProfit

		[ResourceStringData("Enterprise.MarketingManager.EntityTradeDetailWrapper|EstimatedProfit", ShortCaption = "Est Value", Caption = "Estimated Value")]
		public ZDecimal EstimatedProfit
		{
			get { return CurrentProspectPeriod.PAS_EstimatedProfit; }
			set
			{
				if (CurrentProspectPeriod.PAS_EstimatedProfit != value)
				{
					CurrentProspectPeriod.PAS_EstimatedProfit = value;
					RefreshEstimatedTotalsIfUsingCalculated(Parent);
				}
			}
		}

		public ZWrappedPropertyInfo EstimatedProfitInfo => GetWrappedZPropertyInfo(nameof(EstimatedProfit), x => CurrentProspectPeriod.PAS_EstimatedProfitInfo);

		#endregion

		#region Exchange Rate

		[DecimalPlaces(4)]
		[ResourceStringData("Enterprise.MarketingManager.EntityTradeDetailWrapper|EntityCurrencyExchangeRate", ShortCaption = "Ex. Rate", Caption = "Exchange Rate")]
		public ZDecimal EntityCurrencyExchangeRate
		{
			get
			{
				if (Parent == null)
				{
					return ZDecimal.Zero;
				}
				else if (CurrencyCode == Parent.TotalRevenueCurrencyCode)
				{
					return 1m;
				}
				else
				{
					return Parent.RevenueCalculator.GetExchangeRate(CurrentProspectPeriod.Currency);
				}
			}
		}

		public ZPropertyInfo EntityCurrencyExchangeRateInfo
		{
			get { return GetZPropertyInfo(Schema.EntityCurrencyExchangeRate); }
		}

		#endregion

		#region Prospect Companies

		public List<ZGuid> ProspectCompanyPks
		{
			get => prospectCompanyPks ?? (prospectCompanyPks = new List<ZGuid>(0));
			set => prospectCompanyPks = value;
		}
		List<ZGuid> prospectCompanyPks;

		#endregion

		#endregion

		#region RefreshEstimatedTotals

		void RefreshEstimatedTotalsIfUsingCalculated(EntitySalesWrapper sales)
		{
			if (sales != null && !sales.IsDeleted && !sales.IsActual && !sales.OW_IsCustomRevenue && !((IBusinessObjectInternals)this).IsUnCommittedRow)
			{
				sales.TotalEstimatedAnnualValueInfo.RefreshBinding();
				sales.CommittedAnnualValueInfo.RefreshBinding();
				sales.CommittedMonthlyValueInfo.RefreshBinding();
				sales.PipelineValueInfo.RefreshBinding();
				sales.UnsuccessfulValueInfo.RefreshBinding();
			}
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			var previousParent = Parent;
			base.Delete();

			RefreshEstimatedTotalsIfUsingCalculated(previousParent);
		}

		#endregion

		#region FetchStrategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new EntityTradeDetailWrapperFetchStrategy(this);
		}

		#endregion
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class SalesHeader : AutoSalesHeader, ISalesHeader
	{
		#region Constructor

		public SalesHeader(OrgHeader org, OrgSalesProduct salesProduct)
			: this(org, org, salesProduct)
		{
			Argument.NotNull(org, "org");
			Argument.NotNull(salesProduct, "salesProduct");
		}

		public SalesHeader(OrgHeader org, ISalesValueAssociatedEntity entity, OrgSalesProduct salesProduct)
			: this(GetAllEntitySalesCollection(entity), new TradedSalesCollection(org), salesProduct, entity)
		{
			Argument.NotNull(org, "org");
			Argument.NotNull(entity, "entity");
			Argument.NotNull(salesProduct, "salesProduct");
		}

		static EntitySalesWrapperCollection GetAllEntitySalesCollection(ISalesValueAssociatedEntity entity)
		{
			var result = new EntitySalesWrapperCollection(entity);
			result.Load();
			return result;
		}

		public SalesHeader(EntitySalesWrapperCollection allEntitySalesCollection, TradedSalesCollection allTradedSalesCollection, OrgSalesProduct salesProduct, ISalesValueAssociatedEntity entity)
			: base(allEntitySalesCollection.Factory)
		{
			Argument.NotNull(allEntitySalesCollection, "allEntitySalesCollection");
			Argument.NotNull(salesProduct, "salesProduct");

			this.allEntitySalesCollection = allEntitySalesCollection;
			this.allTradedSalesCollection = allTradedSalesCollection;
			this.salesProduct = salesProduct;
			this.revenueDisplayOption = RevenueDisplay.FinancialYear;
			this.entity = entity;

			if (IsFilteredByCompany)
			{
				companyFilter = Env.CurrentCompany.PK;
			}
		}

		readonly ISalesValueAssociatedEntity entity;
		readonly EntitySalesWrapperCollection allEntitySalesCollection;
		readonly TradedSalesCollection allTradedSalesCollection;

		#endregion

		#region Properties

		#region ViewingOrg

		public OrgHeader ViewingOrg
		{
			get { return allEntitySalesCollection.Org; }
		}

		public ISalesValueAssociatedEntity Entity
		{
			get { return allEntitySalesCollection.Entity; }
		}

		#endregion

		#region SalesProduct

		IOrgSalesProduct ISalesHeader.SalesProduct
		{
			get { return salesProduct; }
		}

		public OrgSalesProduct SalesProduct
		{
			get { return salesProduct; }
		}
		readonly OrgSalesProduct salesProduct;

		[ResourceStringData("SalesHeader|SalesProductCode", Caption = "Code")]
		public ZString SalesProductCode
		{
			get { return SalesProduct.MP_Code; }
		}

		public ZString SalesProductName
		{
			get { return SalesProduct.MP_NameMultilingual; }
		}

		#endregion

		#region TotalAnnualVolume

		public override ZDecimal TotalAnnualMetricVolume
		{
			get
			{
				if (!totalAnnualMetricVolume.HasValue)
				{
					totalAnnualMetricVolume = CalculateTotalAnnualMetricVolume(EntitySales);
				}
				return totalAnnualMetricVolume.Value;
			}
		}
		ZDecimal? totalAnnualMetricVolume;

		public static ZDecimal CalculateTotalAnnualMetricVolume(IEnumerable<EntitySalesWrapper> entitySales)
		{
			var result = entitySales.Sum(i => i.OW_Calc_TotalAnnualMetricVolume);
			return result;
		}

		#endregion

		#region TotalCurrency

		IRefCurrency ISalesHeader.TotalCurrency
		{
			get { return TotalCurrency; }
		}

		public RefCurrency TotalCurrency
		{
			get { return Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, TotalCurrencyCode); }
		}

		public override ZString TotalCurrencyCode
		{
			get { return RevenueCalculator.EntityCurrencyCode; }
		}

		public IEnumerable<RefCurrency> GetMissingExchangeRates()
		{
			return RevenueCalculator.GetMissingExchangeRatesToConvertTo(TotalCurrency);
		}

		#endregion

		#region Revenue Display

		public enum RevenueDisplay
		{
			FinancialYear,
			PerAnnum
		}

		public RevenueDisplay RevenueDisplayOption
		{
			get => revenueDisplayOption;
			set
			{
				if (value != revenueDisplayOption)
				{
					revenueDisplayOption = value;
					TradedAnnualTotalInfo.RefreshBinding();

					foreach (var sales in FilterableEntitySalesCollection.Cast<EntitySalesWrapper>())
					{
						sales.ActualsInformation.RevenueDisplayOption = value;
						sales.RefreshCalculatedValues();
					}

					OnRevenueDisplayOptionChanged();
				}
			}
		}
		RevenueDisplay revenueDisplayOption;

		public event EventHandler RevenueDisplayOptionChanged;

		void OnRevenueDisplayOptionChanged() => RevenueDisplayOptionChanged?.Invoke(this, EventArgs.Empty);

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

					if (FilterableEntitySalesCollection.CompanyFilter != value)
					{
						FilterableEntitySalesCollection.CompanyFilter = value;
						foreach (var sales in FilterableEntitySalesCollection.Cast<EntitySalesWrapper>())
						{
							sales.CompanyFilter = value;
							sales.ActualsInformation.CompanyFilter = value;
							sales.RefreshCalculatedValues();
						}
					}
				}
			}
		}
		ZGuid companyFilter;

		bool IsFilteredByCompany => entity is OrgHeader;

		#endregion

		#region Prospect Value

		#region Total Estimate

		public override ZDecimal TotalEstimatedAnnualValue
		{
			get
			{
				var salesToInclude = FilterableEntitySalesCollection.Cast<EntitySalesWrapper>();
				RevenueCalculator.AddFetchHintsForView(salesToInclude, EntitySalesWrapper.Schema.TotalEstimatedAnnualValue);
				return RevenueCalculator.GetTotalInEntityCurrency(salesToInclude, x => x.TotalRevenueCurrencyCode, x => x.TotalEstimatedAnnualValue);
			}
		}

		public override ZDecimal TotalEstimatedMonthlyAverage => TotalEstimatedAnnualValue / 12;

		#endregion

		#region Committed

		public override ZDecimal CommittedAnnualValue
		{
			get
			{
				var salesToInclude = FilterableEntitySalesCollection.Cast<EntitySalesWrapper>();
				RevenueCalculator.AddFetchHintsForView(salesToInclude, EntitySalesWrapper.Schema.CommittedAnnualValue);
				return RevenueCalculator.GetTotalInEntityCurrency(salesToInclude, x => x.TotalRevenueCurrencyCode, x => x.CommittedAnnualValue);
			}
		}

		public override ZDecimal CommittedMonthlyValue
		{
			get
			{
				var salesToInclude = FilterableEntitySalesCollection.Cast<EntitySalesWrapper>();
				RevenueCalculator.AddFetchHintsForView(salesToInclude, EntitySalesWrapper.Schema.CommittedMonthlyValue);
				return RevenueCalculator.GetTotalInEntityCurrency(salesToInclude, x => x.TotalRevenueCurrencyCode, x => x.CommittedMonthlyValue);
			}
		}

		public ZDecimal CommittedCurrentYearValue
		{
			get { return RevenueDisplayOption == RevenueDisplay.FinancialYear ? CommittedCurrentFinancialYearValue : CommittedTrailing12MonthsValue; }
		}
		public ZPropertyInfo CommittedCurrentYearValueInfo => GetZPropertyInfo(nameof(CommittedCurrentYearValue));

		public ZDecimal CommittedCurrentYearTEUQuantity
		{
			get { return RevenueDisplayOption == RevenueDisplay.FinancialYear ? CommittedCurrentFinancialYearTEUQuantity : CommittedTrailing12MonthsTEUQuantity; }
		}
		public ZPropertyInfo CommittedCurrentYearTEUQuantityInfo => GetZPropertyInfo(nameof(CommittedCurrentYearTEUQuantity));

		public ZDecimal CommittedNextYearValue
		{
			get { return RevenueDisplayOption == RevenueDisplay.FinancialYear ? CommittedNextFinancialYearValue : CommittedNext12MonthsValue; }
		}
		public ZPropertyInfo CommittedNextYearValueInfo => GetZPropertyInfo(nameof(CommittedNextYearValue));

		public ZDecimal CommittedNextYearTEUQuantity
		{
			get { return RevenueDisplayOption == RevenueDisplay.FinancialYear ? CommittedNextFinancialYearTEUQuantity : CommittedNext12MonthsTEUQuantity; }
		}
		public ZPropertyInfo CommittedNextYearTEUQuantityInfo => GetZPropertyInfo(nameof(CommittedNextYearTEUQuantity));

		public ZDecimal CommittedYearToDateValue
		{
			get { return RevenueDisplayOption == RevenueDisplay.FinancialYear ? CommittedCurrentFinancialYearToDateValue : ZDecimal.Zero; }
		}
		public ZPropertyInfo CommittedYearToDateValueInfo => GetZPropertyInfo(nameof(CommittedYearToDateValue));

		public ZDecimal CommittedYearToDateTEUQuantity
		{
			get { return RevenueDisplayOption == RevenueDisplay.FinancialYear ? CommittedCurrentFinancialYearToDateTEUQuantity : ZDecimal.Zero; }
		}
		public ZPropertyInfo CommittedYearToDateTEUQuantityInfo => GetZPropertyInfo(nameof(CommittedYearToDateTEUQuantity));

		public ZDecimal CommittedAndForecastCurrentYearValue
		{
			get { return RevenueDisplayOption == RevenueDisplay.FinancialYear ? (CommittedCurrentFinancialYearValue + ForecastCurrentFinancialYearValue) : (CommittedTrailing12MonthsValue + ForecastTrailing12MonthsValue); }
		}
		public ZPropertyInfo CommittedAndForecastCurrentYearValueInfo => GetZPropertyInfo(nameof(CommittedAndForecastCurrentYearValue));

		public ZDecimal CommittedAndForecastCurrentYearTEUQuantity
		{
			get { return RevenueDisplayOption == RevenueDisplay.FinancialYear ? (CommittedCurrentFinancialYearTEUQuantity + ForecastCurrentFinancialYearTEUQuantity) : (CommittedTrailing12MonthsTEUQuantity + ForecastTrailing12MonthsTEUQuantity); }
		}
		public ZPropertyInfo CommittedAndForecastCurrentYearTEUQuantityInfo => GetZPropertyInfo(nameof(CommittedAndForecastCurrentYearTEUQuantity));

		public ZDecimal CommittedAndForecastNextYearValue
		{
			get { return RevenueDisplayOption == RevenueDisplay.FinancialYear ? (CommittedNextFinancialYearValue + ForecastNextFinancialYearValue) : (CommittedNext12MonthsValue + ForecastNext12MonthsValue); }
		}
		public ZPropertyInfo CommittedAndForecastNextYearValueInfo => GetZPropertyInfo(nameof(CommittedAndForecastNextYearValue));

		public ZDecimal CommittedAndForecastNextYearTEUQuantity
		{
			get { return RevenueDisplayOption == RevenueDisplay.FinancialYear ? (CommittedNextFinancialYearTEUQuantity + ForecastNextFinancialYearTEUQuantity) : (CommittedNext12MonthsTEUQuantity + ForecastNext12MonthsTEUQuantity); }
		}
		public ZPropertyInfo CommittedAndForecastNextYearTEUQuantityInfo => GetZPropertyInfo(nameof(CommittedAndForecastNextYearTEUQuantity));

		public override ZDecimal CommittedCurrentFinancialYearToDateValue => ValueCalculator.GetProductValue(SalesValueCalculator.ValueType.Committed, SalesValueCalculator.PeriodRange.CurrentFinancialYearToDate, companyPk: CompanyFilter, salesProduct: SalesProduct).TotalRevenue;

		public override ZDecimal CommittedCurrentFinancialYearToDateTEUQuantity => ValueCalculator.GetProductValue(SalesValueCalculator.ValueType.Committed, SalesValueCalculator.PeriodRange.CurrentFinancialYearToDate, companyPk: CompanyFilter, salesProduct: SalesProduct).TEUQuantity;

		public override ZDecimal CommittedCurrentFinancialYearValue => ValueCalculator.GetProductValue(SalesValueCalculator.ValueType.Committed, SalesValueCalculator.PeriodRange.CurrentFinancialYear, companyPk: CompanyFilter, salesProduct: SalesProduct).TotalRevenue;

		public override ZDecimal CommittedCurrentFinancialYearTEUQuantity => ValueCalculator.GetProductValue(SalesValueCalculator.ValueType.Committed, SalesValueCalculator.PeriodRange.CurrentFinancialYear, companyPk: CompanyFilter, salesProduct: SalesProduct).TEUQuantity;

		public override ZDecimal CommittedNextFinancialYearValue => ValueCalculator.GetProductValue(SalesValueCalculator.ValueType.Committed, SalesValueCalculator.PeriodRange.NextFinancialYear, companyPk: CompanyFilter, salesProduct: SalesProduct).TotalRevenue;

		public override ZDecimal CommittedNextFinancialYearTEUQuantity => ValueCalculator.GetProductValue(SalesValueCalculator.ValueType.Committed, SalesValueCalculator.PeriodRange.NextFinancialYear, companyPk: CompanyFilter, salesProduct: SalesProduct).TEUQuantity;

		public override ZDecimal ForecastCurrentFinancialYearValue => ValueCalculator.GetProductValue(SalesValueCalculator.ValueType.Forecast, SalesValueCalculator.PeriodRange.CurrentFinancialYear, companyPk: CompanyFilter, salesProduct: SalesProduct).TotalRevenue;

		public override ZDecimal ForecastCurrentFinancialYearTEUQuantity => ValueCalculator.GetProductValue(SalesValueCalculator.ValueType.Forecast, SalesValueCalculator.PeriodRange.CurrentFinancialYear, companyPk: CompanyFilter, salesProduct: SalesProduct).TEUQuantity;

		public override ZDecimal ForecastNextFinancialYearValue => ValueCalculator.GetProductValue(SalesValueCalculator.ValueType.Forecast, SalesValueCalculator.PeriodRange.NextFinancialYear, companyPk: CompanyFilter, salesProduct: SalesProduct).TotalRevenue;

		public override ZDecimal ForecastNextFinancialYearTEUQuantity => ValueCalculator.GetProductValue(SalesValueCalculator.ValueType.Forecast, SalesValueCalculator.PeriodRange.NextFinancialYear, companyPk: CompanyFilter, salesProduct: SalesProduct).TEUQuantity;

		public override ZDecimal CommittedTrailing12MonthsValue => ValueCalculator.GetProductValue(SalesValueCalculator.ValueType.Committed, SalesValueCalculator.PeriodRange.Trailing12Months, companyPk: CompanyFilter, salesProduct: SalesProduct).TotalRevenue;

		public override ZDecimal CommittedTrailing12MonthsTEUQuantity => ValueCalculator.GetProductValue(SalesValueCalculator.ValueType.Committed, SalesValueCalculator.PeriodRange.Trailing12Months, companyPk: CompanyFilter, salesProduct: SalesProduct).TEUQuantity;

		public override ZDecimal CommittedNext12MonthsValue => ValueCalculator.GetProductValue(SalesValueCalculator.ValueType.Committed, SalesValueCalculator.PeriodRange.Next12Months, companyPk: CompanyFilter, salesProduct: SalesProduct).TotalRevenue;

		public override ZDecimal CommittedNext12MonthsTEUQuantity => ValueCalculator.GetProductValue(SalesValueCalculator.ValueType.Committed, SalesValueCalculator.PeriodRange.Next12Months, companyPk: CompanyFilter, salesProduct: SalesProduct).TEUQuantity;

		public override ZDecimal ForecastTrailing12MonthsValue => ValueCalculator.GetProductValue(SalesValueCalculator.ValueType.Forecast, SalesValueCalculator.PeriodRange.Trailing12Months, companyPk: CompanyFilter, salesProduct: SalesProduct).TotalRevenue;

		public override ZDecimal ForecastTrailing12MonthsTEUQuantity => ValueCalculator.GetProductValue(SalesValueCalculator.ValueType.Forecast, SalesValueCalculator.PeriodRange.Trailing12Months, companyPk: CompanyFilter, salesProduct: SalesProduct).TEUQuantity;

		public override ZDecimal ForecastNext12MonthsValue => ValueCalculator.GetProductValue(SalesValueCalculator.ValueType.Forecast, SalesValueCalculator.PeriodRange.Next12Months, companyPk: CompanyFilter, salesProduct: SalesProduct).TotalRevenue;

		public override ZDecimal ForecastNext12MonthsTEUQuantity => ValueCalculator.GetProductValue(SalesValueCalculator.ValueType.Forecast, SalesValueCalculator.PeriodRange.Next12Months, companyPk: CompanyFilter, salesProduct: SalesProduct).TEUQuantity;

		#endregion

		#region Pipeline / Unsuccessful

		public override ZDecimal PipelineValue
		{
			get
			{
				if (IsFilteredByCompany)
				{
					return ValueCalculator.GetProductValue(SalesValueCalculator.ValueType.Pipeline, SalesValueCalculator.PeriodRange.None, companyPk: CompanyFilter, salesProduct: SalesProduct).TotalRevenue;
				}
				else
				{
					RevenueCalculator.AddFetchHintsForView(EntitySales, EntitySalesWrapper.Schema.PipelineValue);
					return RevenueCalculator.GetTotalInEntityCurrency(EntitySales, x => x.TotalRevenueCurrencyCode, x => x.PipelineValue);
				}
			}
		}

		public override ZDecimal PipelineTEUQuantity
		{
			get
			{
				if (IsFilteredByCompany)
				{
					return ValueCalculator.GetProductValue(SalesValueCalculator.ValueType.Pipeline, SalesValueCalculator.PeriodRange.None, companyPk: CompanyFilter, salesProduct: SalesProduct).TEUQuantity;
				}
				else
				{
					RevenueCalculator.AddFetchHintsForView(EntitySales, EntitySalesWrapper.Schema.PipelineTEUQuantity);
					return EntitySales.Sum(x => x.PipelineTEUQuantity);
				}
			}
		}

		public override ZDecimal UnsuccessfulValue
		{
			get
			{
				if (IsFilteredByCompany)
				{
					return ValueCalculator.GetProductValue(SalesValueCalculator.ValueType.Unsuccessful, SalesValueCalculator.PeriodRange.None, companyPk: CompanyFilter, salesProduct: SalesProduct).TotalRevenue;
				}
				else
				{
					RevenueCalculator.AddFetchHintsForView(EntitySales, EntitySalesWrapper.Schema.UnsuccessfulValue);
					return RevenueCalculator.GetTotalInEntityCurrency(EntitySales, x => x.TotalRevenueCurrencyCode, x => x.UnsuccessfulValue);
				}
			}
		}

		public override ZDecimal UnsuccessfulTEUQuantity
		{
			get
			{
				if (IsFilteredByCompany)
				{
					return ValueCalculator.GetProductValue(SalesValueCalculator.ValueType.Unsuccessful, SalesValueCalculator.PeriodRange.None, companyPk: CompanyFilter, salesProduct: SalesProduct).TEUQuantity;
				}
				else
				{
					RevenueCalculator.AddFetchHintsForView(EntitySales, EntitySalesWrapper.Schema.UnsuccessfulTEUQuantity);
					return RevenueCalculator.GetTotalInEntityCurrency(EntitySales, x => x.TotalRevenueCurrencyCode, x => x.UnsuccessfulTEUQuantity);
				}
			}
		}

		#endregion

		#endregion

		#region Traded Value

		#region TradedAnnualTotal

		public override ZDecimal TradedAnnualTotal => RevenueDisplayOption == RevenueDisplay.FinancialYear ? TradedCurrentFinancialYearValue : TradedTrailing12MonthsValue;

		ZDecimal TradedCurrentFinancialYearValue => ValueCalculator.GetProductValue(SalesValueCalculator.ValueType.Traded, SalesValueCalculator.PeriodRange.CurrentFinancialYear, Env.CurrentCompany.PK, SalesProduct).TotalRevenue;

		ZDecimal TradedTrailing12MonthsValue => ValueCalculator.GetProductValue(SalesValueCalculator.ValueType.Traded, SalesValueCalculator.PeriodRange.Trailing12Months, Env.CurrentCompany.PK, SalesProduct).TotalRevenue;

		#endregion

		#region TradedAnnualTEUTotalQuantity
		public override ZDecimal TradedAnnualTEUTotalQuantity => RevenueDisplayOption == RevenueDisplay.FinancialYear ? TradedCurrentFinancialYearTEUQuantity : TradedTrailing12MonthsTEUQuantity;

		ZDecimal TradedCurrentFinancialYearTEUQuantity => ValueCalculator.GetProductValue(SalesValueCalculator.ValueType.Traded, SalesValueCalculator.PeriodRange.CurrentFinancialYear, Env.CurrentCompany.PK, SalesProduct).TEUQuantity;

		ZDecimal TradedTrailing12MonthsTEUQuantity => ValueCalculator.GetProductValue(SalesValueCalculator.ValueType.Traded, SalesValueCalculator.PeriodRange.Trailing12Months, Env.CurrentCompany.PK, SalesProduct).TEUQuantity;

		#endregion

		#region TradedMonthlyAverage

		public override ZDecimal TradedMonthlyAverage
		{
			get
			{
				var result = ZDecimal.Zero;
				if (RevenueDisplayOption == RevenueDisplay.FinancialYear)
				{
					var periodCalculator = new AccountingPeriodCalculator(Factory);
					var currentFinancialPeriod = periodCalculator.GetPeriodFromDate(ZDate.Today);
					if (currentFinancialPeriod > 0)
					{
						result = TradedAnnualTotal / (currentFinancialPeriod % 100);
					}
				}
				else
				{
					result = TradedAnnualTotal / 12;
				}
				return result;
			}
		}

		#endregion

		#endregion

		#endregion

		#region Calculators

		public OrgSalesRevenueCalculator RevenueCalculator => revenueCalculator ?? (revenueCalculator = new OrgSalesRevenueCalculator(Factory, Entity));
		OrgSalesRevenueCalculator revenueCalculator;

		SalesValueCalculator ValueCalculator => valueCalculator ?? (valueCalculator = new SalesValueCalculator(Factory, ViewingOrg.PK, RevenueCalculator));
		SalesValueCalculator valueCalculator;

		#endregion

		#region Delete

		public override void Delete()
		{
			foreach (var sales in EntitySalesCollectionProductView.ToArray<EntitySalesWrapper>())
			{
				if (sales.IsEditable)
				{
					EntitySalesCollectionProductView.RemoveAndDelete(sales);
				}
				else
				{
					if (sales.HasGroupings)
					{
						sales.EntityDetailGroupings.RemoveAndDeleteAll();
					}
					else
					{
						sales.EntityTradeDetailsCollection.RemoveAndDeleteAll();
					}

					EntitySalesCollectionProductView.Remove(sales);
				}
			}

			base.Delete();
			foreach (BusinessObjectCollection parentCollection in ParentCollections.ToList())
			{
				parentCollection.Remove(this);
			}

			if (Entity is ISalesRelatedBusinessObject entityAsSalesRelatedBizObj)
			{
				entityAsSalesRelatedBizObj.OnSalesEstimatedValueChange();
			}
		}

		#endregion

		#region Fetch Strategy

		protected override IBusinessObjectFetchStrategy GetFetchStrategy()
		{
			return new SalesHeaderFetchStrategy(this);
		}

		#endregion

		#region Related Business Objects

		#region ProspectiveSales

		public IEnumerable<EntitySalesWrapper> EntitySales
		{
			get { return EntitySalesCollectionProductView.Cast<EntitySalesWrapper>(); }
		}

		[ChildEditable]
		public EntitySalesWrapperCollectionProductView EntitySalesCollectionProductView
		{
			get
			{
				if (entitySalesCollectionProductView == null)
				{
					entitySalesCollectionProductView = new EntitySalesWrapperCollectionProductView(allEntitySalesCollection, SalesProduct);
					entitySalesCollectionProductView.EstimatedValueChanged += SalesCollectionProductView_EstimatedValueChanged;
					if (Entity is OrgHeader)
					{
						entitySalesCollectionProductView.SettingDefaultsForNewChild += ProspectiveSalesCollection_SettingDefaultsForNewChild;
						entitySalesCollectionProductView.ProspectSalesHasChangesChanged += ProspectiveSalesCollection_ProspectSalesHasChangesChanged;
					}
					RegisterEditableChildObject(entitySalesCollectionProductView);
				}

				return entitySalesCollectionProductView;
			}
		}
		EntitySalesWrapperCollectionProductView entitySalesCollectionProductView;

		public EntitySalesWrapperCollectionFilterableView FilterableEntitySalesCollection
		{
			get
			{
				if (filterableEntitySalesCollection == null)
				{
					filterableEntitySalesCollection = new EntitySalesWrapperCollectionFilterableView(EntitySalesCollectionProductView, CompanyFilter);
					foreach (var sales in filterableEntitySalesCollection.Cast<EntitySalesWrapper>())
					{
						sales.CompanyFilter = CompanyFilter;
						sales.ActualsInformation.CompanyFilter = CompanyFilter;
						sales.RefreshCalculatedValues();
					}
				}

				return filterableEntitySalesCollection;
			}
		}
		EntitySalesWrapperCollectionFilterableView filterableEntitySalesCollection;

		void SalesCollectionProductView_EstimatedValueChanged(object sender, EventArgs e)
		{
			RefreshProspectTotals();
		}

		void RefreshProspectTotals()
		{
			TotalEstimatedAnnualValueInfo.RefreshBinding();
			TotalEstimatedMonthlyAverageInfo.RefreshBinding();
		}

		void RefreshCachedProspectTotals()
		{
			ValueCalculator.InvalidateCache();

			CommittedCurrentYearValueInfo.RefreshBinding();
			CommittedCurrentYearTEUQuantityInfo.RefreshBinding();
			CommittedNextYearValueInfo.RefreshBinding();
			CommittedNextYearTEUQuantityInfo.RefreshBinding();
			CommittedYearToDateValueInfo.RefreshBinding();
			CommittedYearToDateTEUQuantityInfo.RefreshBinding();
			CommittedAndForecastNextYearValueInfo.RefreshBinding();
			CommittedAndForecastNextYearTEUQuantityInfo.RefreshBinding();

			if (filterableEntitySalesCollection != null)
			{
				foreach (var sales in filterableEntitySalesCollection.Cast<EntitySalesWrapper>())
				{
					sales.ActualsInformation.RefreshCachedValues();
				}
			}
		}

		void ProspectiveSalesCollection_SettingDefaultsForNewChild(object sender, EntitySalesEventArgs e)
		{
			var entitySales = e.EntitySales;
			AddOrgAssociationIfNotExist(entitySales);
		}

		void ProspectiveSalesCollection_ProspectSalesHasChangesChanged(object sender, EntitySalesEventArgs e)
		{
			var entitySales = e.EntitySales;
			if (entitySales.HasChanges)
			{
				AddOrgAssociationIfNotExist(entitySales);
			}
		}

		void AddOrgAssociationIfNotExist(OrgSales sales)
		{
			if (!currentlyAddingOrgAssociation)
			{
				var header = Entity as OrgHeader;
				if (header != null)
				{
					try
					{
						currentlyAddingOrgAssociation = true;
						if (sales.ShouldDefaultProperties && !sales.SalesAssociationPivotCollectionGlobal.Any())
						{
							sales.SalesAssociationPivotCollectionGlobal.AddNew(header);
						}
					}
					finally
					{
						currentlyAddingOrgAssociation = false;
					}
				}
			}
		}

		bool currentlyAddingOrgAssociation;

		#endregion

		#region TradedSales

		public IEnumerable<OrgSales> TradedSales
		{
			get { return TradedSalesCollectionProductView.Cast<OrgSales>(); }
		}

		public TradedSalesCollectionProductView TradedSalesCollectionProductView
		{
			get
			{
				if (tradedSalesCollection == null && allTradedSalesCollection != null)
				{
					tradedSalesCollection = new TradedSalesCollectionProductView(allTradedSalesCollection, SalesProduct);
				}

				return tradedSalesCollection;
			}
		}
		TradedSalesCollectionProductView tradedSalesCollection;

		#endregion

		#endregion

		#region OnFactorySaving

		bool requireRefresh;

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();

			if (HasChanges)
			{
				RefreshProspectTotals();
				UpdateRelatedBizObjsEstimatedValue();
				requireRefresh = true;
			}
		}

		void UpdateRelatedBizObjsEstimatedValue()
		{
			var entityAsSalesRelatedBizObj = Entity as ISalesRelatedBusinessObject;
			if (entityAsSalesRelatedBizObj != null)
			{
				if (!entityAsSalesRelatedBizObj.IsDeleted)
				{
					entityAsSalesRelatedBizObj.AddFetchHintsForSalesEstimatedValueChange();
					entityAsSalesRelatedBizObj.OnSalesEstimatedValueChange();
				}
			}
			else
			{
				var changedEntitySales = EntitySales.Where(x => !x.IsInDatabase || x.HasChanges);

				foreach (var entitySales in changedEntitySales)
				{
					var query = new ZQuery(OrgSalesValueAssociationPivotSchema.SVP_TradeId, entitySales.PK);
					query.AddToFilter(OrgSalesValueAssociationPivotSchema.SVP_TradeTableCode, entitySales.TablePrefix);
					Factory.AddFetchHint(OrgSalesValueAssociationPivotSchema.Instance, query);
				}

				foreach (var pivot in changedEntitySales.SelectMany(x => x.SalesAssociationPivotCollectionGlobal))
				{
					SalesValueAssociationPivotFetchStrategy.AddFetchHintForAccessingAssociatedEntity(pivot);
				}

				var relatedBizObjs = changedEntitySales.SelectMany(x => x.SalesRelatedBusinessObjects).Where(x => !x.IsDeleted).Distinct();
				foreach (var relatedBizObj in relatedBizObjs)
				{
					relatedBizObj.AddFetchHintsForSalesEstimatedValueChange();
				}

				foreach (var relatedBizObj in relatedBizObjs)
				{
					relatedBizObj.OnSalesEstimatedValueChange();
				}
			}
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			if (saveSucceeded && requireRefresh)
			{
				RefreshCachedProspectTotals();
				requireRefresh = false;
			}
		}

		#endregion
	}
}

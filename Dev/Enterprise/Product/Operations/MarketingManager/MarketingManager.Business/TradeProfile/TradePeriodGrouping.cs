using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class TradePeriodGrouping : AutoTradePeriodGrouping
	{
		#region Constructor

		public TradePeriodGrouping(TradedSalesAnalysis salesAnalysis, ZString description, IEnumerable<OrgTradePeriod> tradePeriods, Type tradePeriodGrouperType)
			: base(salesAnalysis.Factory)
		{
			this.description = description;
			this.SalesAnalysis = salesAnalysis;
			this.Children = null;
			this.TradePeriods = new List<OrgTradePeriod>(tradePeriods);
			this.TradePeriodGrouperType = tradePeriodGrouperType;
		}

		public TradePeriodGrouping(TradedSalesAnalysis salesAnalysis, ZString description, IEnumerable<TradePeriodGrouping> children, Type tradePeriodGrouperType)
			: base(salesAnalysis.Factory)
		{
			if (!children.Any())
			{
				throw new ArgumentException("children must have at least one element");
			}

			this.description = description;
			this.SalesAnalysis = salesAnalysis;
			this.Children = new List<TradePeriodGrouping>(children);
			this.TradePeriods = null;
			this.TradePeriodGrouperType = tradePeriodGrouperType;
		}

		public TradePeriodGrouping(TradedSalesAnalysis salesAnalysis, IEnumerable<OrgTradePeriod> tradePeriods, Type tradePeriodGrouperType)
			: base(salesAnalysis.Factory)
		{
			this.SalesAnalysis = salesAnalysis;
			this.description = null;
			this.Children = null;
			this.TradePeriods = new List<OrgTradePeriod>(tradePeriods);
			this.TradePeriodGrouperType = tradePeriodGrouperType;
		}

		public readonly Type TradePeriodGrouperType;

		#endregion

		#region Currency

		protected override ZString GetCurrencyCode()
		{
			return RevenueCalculator.EntityCurrencyCode;
		}

		public RefCurrency Currency
		{
			get { return Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, CurrencyCode); }
		}

		public int CurrencyDecimals
		{
			get
			{
				var currency = Currency;
				return currency != null ? currency.Decimals : 2;
			}
		}

		#endregion

		#region Description

		protected override ZString GetDescription()
		{
			return description;
		}
		readonly ZString description;

		#endregion

		#region Destination

		protected override ZString GetDestination()
		{
			return IsLeaf ?
				TradePeriods.First().TradeDetail.Parent.DestinationDescription :
				Children.First().Destination;
		}

		#endregion

		#region DestinationCountry

		protected override ZString GetDestinationCountry()
		{
			return IsLeaf ?
				TradePeriods.First().TradeDetail.Parent.DestinationCountryCode :
				Children.First().DestinationCountry;
		}

		#endregion

		#region DestinationState

		protected override ZString GetDestinationState()
		{
			return IsLeaf ?
				TradePeriods.First().TradeDetail.Parent.DestinationStateCode :
				Children.First().DestinationState;
		}

		#endregion

		#region DestinationUnloco

		protected override ZString GetDestinationUnloco()
		{
			return IsLeaf ?
				TradePeriods.First().TradeDetail.Parent.DestinationUnlocoCode :
				Children.First().DestinationUnloco;
		}

		#endregion

		#region GrossRevenue

		protected override ZDecimal GetGrossRevenue()
		{
			if (IsLeaf)
			{
				if (SalesAnalysis.ViewpointOrg == null)
				{
					return ZDecimal.Zero;
				}
				else
				{
					return CalculateValue(x => x.PAV_Revenue, isJobValue: false);
				}
			}
			else
			{
				return Children.Sum(x => x.GrossRevenue);
			}
		}

		#endregion

		#region JobRevenue

		protected override ZDecimal GetJobRevenue()
		{
			if (IsLeaf)
			{
				if (SalesAnalysis.ViewpointOrg == null)
				{
					return ZDecimal.Zero;
				}
				else
				{
					return CalculateValue(x => x.PAV_Revenue, isJobValue: true);
				}
			}
			else
			{
				return Children.Sum(x => x.JobRevenue);
			}
		}

		#endregion

		#region JobCost
		protected override ZDecimal GetJobCost()
		{
			if (IsLeaf)
			{
				if (SalesAnalysis.ViewpointOrg == null)
				{
					return ZDecimal.Zero;
				}
				else
				{
					return CalculateValue(x => x.PAV_Cost, isJobValue: true);
				}
			}
			else
			{
				return Children.Sum(x => x.JobCost);
			}
		}

		#endregion

		#region JobProfit

		protected override ZDecimal GetJobProfit()
		{
			return JobRevenue - JobCost;
		}

		#endregion

		#region RevenueCalculator

		OrgSalesRevenueCalculator RevenueCalculator
		{
			get { return revenueCalculator ?? (revenueCalculator = new OrgSalesRevenueCalculator(Factory, SalesAnalysis.ViewpointOrg)); }
		}
		OrgSalesRevenueCalculator revenueCalculator;

		ZDecimal CalculateValue(Func<OrgTradeValue, ZDecimal> valueAmountGetter, bool isJobValue)
		{
			var tradeValuesToInclude = GetTradeValues(isJobValue);
			return RevenueCalculator.GetTotalInEntityCurrency(tradeValuesToInclude, x => x.PAV_RX_NKCurrency, valueAmountGetter);
		}

		IEnumerable<OrgTradeValue> GetTradeValues(bool isJobValue)
		{
			var result = new List<OrgTradeValue>(TradePeriods?.Count ?? 1);

			var tradePeriods = isJobValue ? JobTradePeriods : OrgTradePeriods;

			foreach (var period in tradePeriods)
			{
				Factory.AddFetchHint(OrgTradeValueSchema.PAV_PAS, period.PK);
			}

			foreach (var period in tradePeriods)
			{
				var tradeValueQuery = new ZQuery(OrgTradeValueSchema.PAV_PAS, period.PK);
				var tradeValues = Factory.Load<OrgTradeValue>(tradeValueQuery);
				result.AddRange(tradeValues.Where(x => x.PAV_GC == Env.CurrentCompanyPK));
			}

			return result;
		}

		#endregion

		#region Mode

		protected override ZString GetMode()
		{
			return IsLeaf ?
				TradePeriods.First().TradeDetail.PA_TradeMode :
				Children.First().Mode;
		}

		#endregion

		#region Origin

		protected override ZString GetOrigin()
		{
			return IsLeaf ?
				TradePeriods.First().TradeDetail.Parent.OriginDescription :
				Children.First().Origin;
		}

		#endregion

		#region OriginCountry

		protected override ZString GetOriginCountry()
		{
			return IsLeaf ?
				TradePeriods.First().TradeDetail.Parent.OriginCountryCode :
				Children.First().OriginCountry;
		}

		#endregion

		#region OriginState

		protected override ZString GetOriginState()
		{
			return IsLeaf ?
				TradePeriods.First().TradeDetail.Parent.OriginStateCode :
				Children.First().OriginState;
		}

		#endregion

		#region OriginUnloco

		protected override ZString GetOriginUnloco()
		{
			return IsLeaf ?
				TradePeriods.First().TradeDetail.Parent.OriginUnlocoCode :
				Children.First().OriginUnloco;
		}

		#endregion

		#region Service

		protected override ZString GetService()
		{
			return IsLeaf ?
				TradePeriods.First().TradeDetail.Parent.OW_Service :
				Children.First().Service;
		}

		#endregion

		#region SupplierPart

		protected override ZGuid GetSupplierPartPk()
		{
			if (IsLeaf)
			{
				return TradePeriods.First().TradeDetail.PA_OP;
			}
			else
			{
				return Children.First().SupplierPartPk;
			}
		}

		protected override ZString GetSupplierPartNum()
		{
			if (IsLeaf)
			{
				var supplierPart = TradePeriods.First().TradeDetail.SupplierPart;
				return supplierPart != null ? supplierPart.OP_PartNum : ZString.Empty;
			}
			else
			{
				return Children.First().SupplierPartNum;
			}
		}

		protected override ZString GetSupplierPartDescription()
		{
			return IsLeaf ?
				TradePeriods.First().TradeDetail.SupplierPartDescription :
				Children.First().SupplierPartDescription;
		}

		#endregion

		#region Type

		public override ZString Type
		{
			get
			{
				return IsLeaf ?
					TradePeriods.First().TradeDetail.PA_TradeType :
					Children.First().Type;
			}
		}

		#endregion

		#region Warehouse

		protected override ZString GetWarehouse()
		{
			return IsLeaf ?
				TradePeriods.First().TradeDetail.Parent.WarehouseDescription :
				Children.First().Warehouse;
		}

		#endregion

		#region WarehouseCountry

		protected override ZString GetWarehouseCountry()
		{
			if (IsLeaf)
			{
				var warehouse = TradePeriods.First().TradeDetail.Parent.Warehouse;
				return warehouse != null ? warehouse.CountryCode : ZString.Empty;
			}
			else
			{
				return Children.First().WarehouseCountry;
			}
		}

		#endregion

		#region Buyer

		protected override ZString GetBuyerCode()
		{
			return IsLeaf ?
				GetBuyerCode(TradePeriods.First().TradeDetail.Parent) :
				Children.First().BuyerCode;
		}

		static ZString GetBuyerCode(OrgSales sales)
		{
			var factory = sales.Factory;
			var buyer = factory.Load<OrgHeader>(sales.OW_OH_Buyer);
			return buyer != null ? buyer.OH_Code : ZString.Empty;
		}

		#endregion

		#region Count

		protected override ZInt GetCount()
		{
			if (SalesAnalysis.SalesHeader.SalesProductCode == SystemDefinedSalesProductList.Codes.Warehouse
				&& !IsLeaf
				&& Children.First().TradePeriodGrouperType == typeof(TradePeriodSupplierPartGrouper)
				&& (Service == OrgSalesWarehouseServiceTypesList.Codes.Orders || Service == OrgSalesWarehouseServiceTypesList.Codes.Receipts))
			{
				var allOrgTradePeriods = OrgTradePeriods ?? RecursivelyGetAllOrgTradePeriods(Children);
				var tradePeriodsToSum = allOrgTradePeriods.Where(x => x.TradeDetail.PA_OP.IsEmpty);
				return (ZInt)tradePeriodsToSum.Sum(x => x.PAS_RepeatsMnth);
			}
			else
			{
				return (ZInt)(IsLeaf ?
					OrgTradePeriods.Sum(x => x.PAS_RepeatsMnth) :
					Children.Sum(x => x.Count));
			}
		}

		#endregion

		#region UnitCount

		protected override ZDecimal GetUnitCount()
		{
			return (ZDecimal)(IsLeaf ?
				OrgTradePeriods.Sum(x => x.PAS_Units) :
				Children.Sum(x => x.UnitCount));
		}

		#endregion

		#region TEUQuantity

		protected override ZDecimal GetTEUQuantity()
		{
			return (IsLeaf ?
				OrgTradePeriods.Sum(x => x.PAS_TEUQuantity) :
				Children.Sum(x => x.TEUQuantity));
		}

		#endregion

		#region PalletCount

		protected override ZInt GetPalletCount()
		{
			return (ZInt)(IsLeaf ?
				OrgTradePeriods.Sum(x => x.PAS_PalletCount) :
				Children.Sum(x => x.PalletCount));
		}

		#endregion

		#region LineCount

		protected override ZInt GetLineCount()
		{
			return (ZInt)(IsLeaf ?
				OrgTradePeriods.Sum(x => x.PAS_LineCount) :
				Children.Sum(x => x.LineCount));
		}

		#endregion

		#region GrossWeight

		protected override ZDecimal GetGrossWeight()
		{
			return IsLeaf ?
				OrgTradePeriods.Sum(x => x.PAS_Weight) :
				Children.Sum(x => x.GrossWeight);
		}

		protected override ZString GetWeightUnit()
		{
			if (IsLeaf)
			{
				var period = OrgTradePeriods.FirstOrDefault();
				return period != null ? period.PAS_WeightUQ.ToString() : Enterprise.Core.Constants.Weight.Tonnes;
			}
			else
			{
				var child = Children.FirstOrDefault();
				return child != null ? child.WeightUnit.ToString() : Enterprise.Core.Constants.Weight.Tonnes;
			}
		}

		#endregion

		#region Last Job Registration

		protected override ZDateTime GetLastJobRegistration()
		{
			return IsLeaf ?
				TradePeriods.Max(x => x.PAS_LastTraded) :
				Children.Max(x => x.LastJobRegistration);
		}

		#endregion

		#region NetVolume

		protected override ZDecimal GetNetVolume()
		{
			return IsLeaf ?
				OrgTradePeriods.Sum(x => x.PAS_Volume) :
				Children.Sum(x => x.NetVolume);
		}

		protected override ZString GetVolumeUnit()
		{
			if (IsLeaf)
			{
				var period = OrgTradePeriods.FirstOrDefault();
				return period != null ? period.PAS_VolumeUQ.ToString() : Enterprise.Core.Constants.Volume.CubicMetres;
			}
			else
			{
				var child = Children.FirstOrDefault();
				return child != null ? child.VolumeUnit.ToString() : Enterprise.Core.Constants.Volume.CubicMetres;
			}
		}

		#endregion

		#region Supplier

		protected override ZString GetSupplierCode()
		{
			return IsLeaf ?
				GetSupplierCode(TradePeriods.First().TradeDetail.Parent) :
				Children.First().SupplierCode;
		}

		static ZString GetSupplierCode(OrgSales sales)
		{
			var factory = sales.Factory;
			var supplier = factory.Load<OrgHeader>(sales.OW_OH_Supplier);
			return supplier != null ? supplier.OH_Code : ZString.Empty;
		}

		#endregion

		#region IsLeaf

		public bool IsLeaf
		{
			get { return Children == null; }
		}

		#endregion

		#region SubTree

		public TradedSalesTreeModel SubTree
		{
			get
			{
				if (subTree == null)
				{
					subTree = new TradedSalesTreeModel(SalesAnalysis);
					using (subTree.GetRebuildSuspender())
					{
						subTree.TradePeriods = TradePeriods;
						subTree.Groupers = SalesAnalysis.TreeGroupers;
					}

					SalesAnalysis.TreeGroupingChanged += salesAnalysis_TreeGroupingChanged;
				}

				return subTree;
			}
		}
		TradedSalesTreeModel subTree;

		void salesAnalysis_TreeGroupingChanged(object sender, EventArgs e)
		{
			if (subTree != null)
			{
				subTree.Groupers = SalesAnalysis.TreeGroupers;
			}
		}

		public readonly TradedSalesAnalysis SalesAnalysis;

		#endregion

		#region Delete

		public override void Delete()
		{
			SalesAnalysis.TreeGroupingChanged -= salesAnalysis_TreeGroupingChanged;
			base.Delete();
		}

		#endregion

		IEnumerable<OrgTradePeriod> RecursivelyGetAllOrgTradePeriods(IEnumerable<TradePeriodGrouping> groupings)
		{
			foreach (var grouping in groupings)
			{
				if (grouping.IsLeaf)
				{
					foreach (var tradePeriod in grouping.OrgTradePeriods)
					{
						yield return tradePeriod;
					}
				}
				else
				{
					foreach (var tradePeriod in RecursivelyGetAllOrgTradePeriods(grouping.Children))
					{
						yield return tradePeriod;
					}
				}
			}
		}

		public readonly IEnumerable<TradePeriodGrouping> Children;
		public readonly ICollection<OrgTradePeriod> TradePeriods;

		IEnumerable<OrgTradePeriod> OrgTradePeriods
		{
			get
			{
				var orgPk = SalesAnalysis.ViewpointOrg?.PK ?? ZGuid.Empty;
				return TradePeriods?.Where(x => !x.PAS_IsJobValue && x.PAS_OH_Client == orgPk);
			}
		}

		IEnumerable<OrgTradePeriod> JobTradePeriods
		{
			get
			{
				var orgPk = SalesAnalysis.ViewpointOrg?.PK ?? ZGuid.Empty;
				return TradePeriods?.Where(x => x.PAS_IsJobValue && x.PAS_OH_Client == orgPk);
			}
		}

		#region WarehouseColumns

		public static HashSet<string> GetWarehouseOrdAndRecColumnsThatArePerJobRatherThanSupplierPart(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("GetWarehouseOrdAndRecColumnsThatArePerJobRatherThanSupplierPart", () =>
			{
				return new HashSet<string>()
				{
					TradePeriodGrouping.Schema.PalletCount,
					TradePeriodGrouping.Schema.GrossWeight,
					TradePeriodGrouping.Schema.WeightUnit,
					TradePeriodGrouping.Schema.NetVolume,
					TradePeriodGrouping.Schema.VolumeUnit
				};
			});
		}

		public static HashSet<string> GetWarehouseOrdAndRecColumnsThatHaveDifferentJobAndSupplierPartTotals(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("GetWarehouseOrdAndRecColumnsThatHaveDifferentJobAndSupplierPartTotals", () =>
			{
				return new HashSet<string>()
				{
					TradePeriodGrouping.Schema.Count,
					TradePeriodGrouping.Schema.GrossRevenue,
					TradePeriodGrouping.Schema.JobRevenue,
					TradePeriodGrouping.Schema.CurrencyCode
				};
			});
		}

		public static HashSet<string> GetWarehouseStgUnusedColumns(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("GetWarehouseStgUnusedColumns", () =>
			{
				return new HashSet<string>()
				{
					TradePeriodGrouping.Schema.UnitCount,
					TradePeriodGrouping.Schema.PalletCount,
					TradePeriodGrouping.Schema.LineCount,
					TradePeriodGrouping.Schema.GrossWeight,
					TradePeriodGrouping.Schema.WeightUnit,
					TradePeriodGrouping.Schema.NetVolume,
					TradePeriodGrouping.Schema.VolumeUnit
				};
			});
		}
		#endregion
	}
}

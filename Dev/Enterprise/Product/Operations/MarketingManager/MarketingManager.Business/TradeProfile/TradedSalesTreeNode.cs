using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Types;
using Enterprise.MarketingManager.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MarketingManager.Business
{
	public class TradedSalesTreeNode : ZNode<TradePeriodGrouping>
	{
		public TradedSalesTreeNode(TradedSalesTreeModel model, TradePeriodGrouping grouping)
			: base(model, grouping)
		{
		}

		#region Properties

		#region Currency

		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public ZString CurrencyCode
		{
			get { return BizObjForBinding.CurrencyCode; }
		}

		#endregion

		#region Destination

		public ZString Destination
		{
			get { return BizObjForBinding.Destination; }
		}

		#endregion

		#region DestinationCountry

		public ZString DestinationCountry
		{
			get { return BizObjForBinding.DestinationCountry; }
		}

		#endregion

		#region DestinationState

		public ZString DestinationState
		{
			get { return BizObjForBinding.DestinationState; }
		}

		#endregion

		#region DestinationUnloco

		public ZString DestinationUnloco
		{
			get { return BizObjForBinding.DestinationUnloco; }
		}

		#endregion

		#region Description

		public ZString Description
		{
			get { return BizObjForBinding.Description; }
		}

		#endregion

		#region GrossRevenue

		public ZString GrossRevenue
		{
			get { return Utilities.FormatNumberNationalWithGroupSeparators((decimal)BizObjForBinding.GrossRevenue, BizObjForBinding.CurrencyDecimals); }
		}
		public ZDecimal GrossRevenueAsZDecimal
		{
			get { return BizObjForBinding.GrossRevenue; }
		}

		#endregion

		#region JobRevenue

		public ZString JobRevenue
		{
			get { return Utilities.FormatNumberNationalWithGroupSeparators((decimal)BizObjForBinding.JobRevenue, BizObjForBinding.CurrencyDecimals); }
		}

		public ZDecimal JobRevenueAsZDecimal
		{
			get { return BizObjForBinding.JobRevenue; }
		}

		#endregion

		#region JobCost

		public ZString JobCost
		{
			get { return Utilities.FormatNumberNationalWithGroupSeparators((decimal)BizObjForBinding.JobCost, BizObjForBinding.CurrencyDecimals); }
		}

		public ZDecimal JobCostAsZDecimal
		{
			get { return BizObjForBinding.JobCost; }
		}

		#endregion

		#region JobProfit

		public ZString JobProfit
		{
			get { return Utilities.FormatNumberNationalWithGroupSeparators((decimal)BizObjForBinding.JobProfit, BizObjForBinding.CurrencyDecimals); }
		}
		public ZDecimal JobProfitAsZDecimal
		{
			get { return BizObjForBinding.JobProfit; }
		}

		#endregion

		#region Mode

		public ZString Mode
		{
			get { return BizObjForBinding.Mode; }
		}

		#endregion

		#region Origin

		public ZString Origin
		{
			get { return BizObjForBinding.Origin; }
		}

		#endregion

		#region OriginCountry

		public ZString OriginCountry
		{
			get { return BizObjForBinding.OriginCountry; }
		}

		#endregion

		#region OriginState

		public ZString OriginState
		{
			get { return BizObjForBinding.OriginState; }
		}

		#endregion

		#region OriginUnloco

		public ZString OriginUnloco
		{
			get { return BizObjForBinding.OriginUnloco; }
		}

		#endregion

		#region Type

		public ZString Type
		{
			get { return BizObjForBinding.Type; }
		}

		#endregion

		#region Buyer

		public ZString BuyerCode
		{
			get { return BizObjForBinding.BuyerCode; }
		}

		#endregion

		#region Count

		public ZString Count
		{
			get { return "(" + BizObjForBinding.Count + ")"; }
		}

		#endregion

		#region UnitCount

		public ZString UnitCount
		{
			get { return BizObjForBinding.UnitCount.ToString(2); }
		}

		#endregion

		#region PalletCount

		public ZString PalletCount
		{
			get { return BizObjForBinding.PalletCount.ToString(); }
		}

		#endregion

		#region LineCount

		public ZString LineCount
		{
			get { return BizObjForBinding.LineCount.ToString(); }
		}

		#endregion

		#region GrossWeight

		public ZString GrossWeight
		{
			get { return Utilities.FormatNumberNationalWithGroupSeparators((decimal)BizObjForBinding.GrossWeight, 2); }
		}

		public ZDecimal GrossWeightAsZDecimal
		{
			get { return BizObjForBinding.GrossWeight; }
		}

		public ZString WeightUnit
		{
			get { return BizObjForBinding.WeightUnit; }
		}

		#endregion

		#region Last Job Registration

		public ZString LastJobRegistration
		{
			get { return BizObjForBinding.LastJobRegistration.IsValid ? BizObjForBinding.LastJobRegistration.ToShortDateString() : ""; }
		}

		public ZDateTime LastJobRegistrationAsZDateTime
		{
			get { return BizObjForBinding.LastJobRegistration; }
		}

		#endregion

		#region NetVolume

		public ZString NetVolume
		{
			get { return Utilities.FormatNumberNationalWithGroupSeparators((decimal)BizObjForBinding.NetVolume, 2); }
		}

		public ZDecimal NetVolumeAsZDecimal
		{
			get { return BizObjForBinding.NetVolume; }
		}

		public ZString VolumeUnit
		{
			get { return BizObjForBinding.VolumeUnit; }
		}

		#endregion

		#region Supplier

		public ZString SupplierCode
		{
			get { return BizObjForBinding.SupplierCode; }
		}

		#endregion

		#region TEUQuantity

		public ZString TEUQuantity
		{
			get { return Utilities.FormatNumberNationalWithGroupSeparators((decimal)BizObjForBinding.TEUQuantity, 2); }
		}

		public ZDecimal TEUQuantityAsZDecimal
		{
			get { return BizObjForBinding.TEUQuantity; }
		}

		#endregion

		#endregion

		#region Overrides

		protected override ChangeParentOnBizObjResult ChangeParentOnBizObj(TradePeriodGrouping previousParent, TradePeriodGrouping newParent, bool checkValid)
		{
			throw new NotSupportedException();
		}

		protected override IEnumerable<TradePeriodGrouping> LoadChildBizObjs()
		{
			if (BizObjForBinding.Children == null)
			{
				return Enumerable.Empty<TradePeriodGrouping>();
			}

			if (BizObjForBinding.SalesAnalysis.SalesHeader.SalesProductCode == SystemDefinedSalesProductList.Codes.Warehouse)
			{
				return BizObjForBinding.Children.Where(x => x.TradePeriodGrouperType != typeof(TradePeriodSupplierPartGrouper) || !x.SupplierPartPk.IsEmpty);
			}
			else
			{
				return BizObjForBinding.Children;
			}
		}

		protected override TradePeriodGrouping LoadParentBizObj()
		{
			throw new NotSupportedException();
		}

		#endregion
	}
}

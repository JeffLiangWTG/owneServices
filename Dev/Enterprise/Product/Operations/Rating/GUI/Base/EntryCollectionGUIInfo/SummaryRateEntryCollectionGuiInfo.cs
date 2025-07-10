using System.Collections.Generic;
using Enterprise.Core.Forms;
using Enterprise.Environment;

namespace Enterprise.Rating.GUI
{
	public class SummaryRateEntryCollectionGUIInfo : RateEntryCollectionGUIInfo
	{
		public override string Category
		{
			get { return ""; }
		}

		public override List<ZGridColumnInfo> Columns => new List<ZGridColumnInfo>
		{
			RateCategory,
			Mode,
			Origin,
			Destination,
			{ CrossTrade, !IsQuote && !IsIntercompanyTariff },
			{ Via, !IsIntercompanyTariff },
			{ PlannedLoad, !IsCosting, null, false },
			{ PlannedDischarge, !IsCosting, null, false },
			{ RateOrigin, !IsCosting, null, false },
			{ RateDestination, !IsCosting, null, false },
			AllWarehouses,
			Warehouse,
			{ Supplier, !IsCosting && !IsIntercompanyTariff },
			Carrier("Lookups.ShippingProviders"),
			AirlineCode,
			ControllingCustomer,
			Consignor,
			{ CartagePickupPostcode, true, null, false },
			Consignee,
			{ CartageDeliveryPostcode, true, null, false },
			ServiceLevel,
			CarrierServiceLevel,
			{ GatewayServiceLevel, IsIntercompanyTariff },
			{ ShipmentGatewayServiceLevel, IsIntercompanyTariff },
			{ GatewayAgentType, IsIntercompanyTariff },
			CommodityCode,
			{ CommodityDescription, null, false },
			{ CommodityLocalCode, !IsIntercompanyTariff },
			{ FMCTariffID, IsFMCTariffIDAllowed },
			Container,
			MatchContainerClass,
			IsNonOperatedReefer,
			Unit,
			Currency,
			{ PaymentTermOverride, !IsCosting },
			{ RateStartDate, !IsQuote },
			{ RateEndDate, !IsQuote },
			{ ContractNumber, !IsIntercompanyTariff },
			{ ContractNumberLinked, IsCosting },
			{ IsPublished, ShowIsPublishedColumn },
			{ Publisher, isGlobal && !IsIntercompanyTariff },
			{ ShipmentConsolidationStatus, IsCosting, null, false },
			{ CreationSource, Env.CurrentUser.IsSupportUser }
		};
	}
}


using System.Collections.Generic;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Rating.Business;

namespace Enterprise.Rating.GUI
{
	public class FCLRateEntryCollectionGUIInfo : RateEntryCollectionGUIInfo
	{
		public override string Category
		{
			get { return RatingConstants.RateCategory.FCL; }
		}

		public override List<ZGridColumnInfo> Columns => new List<ZGridColumnInfo>
		{
			Origin,
			Destination,
			{ CrossTrade, !IsQuote && !IsIntercompanyTariff },
			{ Via, !IsIntercompanyTariff },
			{ PlannedLoad, !IsCosting, null, false },
			{ PlannedDischarge, !IsCosting, null, false },
			{ RateOrigin, !IsCosting, null, false },
			{ RateDestination, !IsCosting, null, false },
			Mode,
			Currency,
			{ PaymentTermOverride, !IsCosting },
			{ Supplier, !IsCosting && !IsIntercompanyTariff },
			Carrier("Lookups.ShippingProviders"),
			Container,
			MatchContainerClass,
			IsNonOperatedReefer,
			ServiceLevel,
			CarrierServiceLevel,
			{ GatewayServiceLevel, IsIntercompanyTariff },
			{ ShipmentGatewayServiceLevel, IsIntercompanyTariff },
			{ GatewayAgentType, IsIntercompanyTariff },
			CommodityCode,
			{ CommodityDescription, null, false },
			CommodityLocalCode,
			ControllingCustomer,
			Consignor,
			Consignee,
			Unit,
			TransitTime("Lookups.SeaTransitTimes"),
			Frequency,
			FrequencyUnit,
			{ RateStartDate, !IsQuote },
			{ RateEndDate, !IsQuote },
			{ DataChecked, IsQuote },
			{ IncoTerm, IsQuote },
			{ ContractNumber, !IsIntercompanyTariff },
			{ ContractNumberLinked, IsCosting },
			{ CartagePickupPostcode, Res.GetData("3e0816ff-8a33-48b4-a6e4-2d31e534ba33", "Origin P/C", "Origin Post Code", "The Post Code at Origin"), false },
			{ CartageDeliveryPostcode, Res.GetData("aaf4e224-4f67-4cda-b7d9-6ef85a29a12d", "Dest. P/C", "Destination Post Code", "The Post Code at Destination"), false },
			{ IsPublished, ShowIsPublishedColumn },
			{ HBLDeliveryMode, (IsClientRate || IsTariff || IsQuote) && !IsCustoms, null, false },
			{ Publisher, isGlobal && !IsIntercompanyTariff },
			{ ShipmentConsolidationStatus, IsCosting, null, false },
			{ FMCTariffID, IsFMCTariffIDAllowed },
			{ CreationSource, Env.CurrentUser.IsSupportUser },
		};
	}
}


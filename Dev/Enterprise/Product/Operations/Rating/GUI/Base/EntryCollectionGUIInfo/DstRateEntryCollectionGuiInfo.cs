using System.Collections.Generic;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Rating.Business;

namespace Enterprise.Rating.GUI
{
	public class DSTRateEntryCollectionGUIInfo : RateEntryCollectionGUIInfo
	{
		public override string Category
		{
			get { return RatingConstants.RateCategory.DST; }
		}

		public override List<ZGridColumnInfo> Columns => new List<ZGridColumnInfo>
		{
			Destination,
			Origin,
			{ CrossTrade, !IsQuote && !IsIntercompanyTariff },
			{ Via, !IsIntercompanyTariff },
			{ PlannedLoad, !IsCosting, null, false },
			{ PlannedDischarge, !IsCosting, null, false },
			{ RateOrigin, !IsCosting, null, false },
			{ RateDestination, !IsCosting, null, false },
			Mode,
			{ Currency, true, null, false },
			{ PaymentTermOverride, !IsCosting },
			Container,
			MatchContainerClass,
			IsNonOperatedReefer,
			ControllingCustomer,
			Consignee,
			{ CartageDeliveryAddress, true, Res.GetString("e5ad8e3b-c312-47ab-b386-b4b3557107a9", "Port Transport Address") },
			{ CartageDeliveryPostcode, true, Res.GetString("48464a46-3289-4514-8ad5-59bca4101934", "Postcode") },
			{ Consignor, true, null, false },
			{ Supplier, !IsCosting && !IsIntercompanyTariff },
			Carrier("Lookups.ShippingProviders"),
			AirlineCode,
			ServiceLevel,
			CarrierServiceLevel,
			{ GatewayServiceLevel, IsIntercompanyTariff },
			{ ShipmentGatewayServiceLevel, IsIntercompanyTariff },
			{ GatewayAgentType, IsIntercompanyTariff },
			CommodityCode,
			{ CommodityDescription, null, false },
			CommodityLocalCode,
			{ RateStartDate, !IsQuote },
			{ RateEndDate, !IsQuote },
			{ DataChecked, IsQuote },
			{ ContractNumber, !IsIntercompanyTariff },
			{ ContractNumberLinked, IsCosting },
			{ IsPublished, ShowIsPublishedColumn },
			{ Publisher, isGlobal && !IsIntercompanyTariff },
			{ AircraftType, null, false },
			{ HBLDeliveryMode, (IsClientRate || IsTariff || IsQuote) && !IsCustoms, null, false },
			{ ShipmentConsolidationStatus, IsCosting, null, false },
			{ FMCTariffID, IsFMCTariffIDAllowed },
			{ CreationSource, Env.CurrentUser.IsSupportUser },
		};
	}
}


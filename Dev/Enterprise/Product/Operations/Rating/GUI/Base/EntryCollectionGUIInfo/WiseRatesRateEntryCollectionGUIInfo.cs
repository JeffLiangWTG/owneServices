using System.Collections.Generic;
using Enterprise.Core.Forms;

namespace Enterprise.Rating.GUI
{
	public class WiseRatesRateEntryCollectionGUIInfo : RateEntryCollectionGUIInfo
	{
		public override string Category
		{
			get { return "WiseRates"; }
		}

		public override List<ZGridColumnInfo> Columns => new List<ZGridColumnInfo>
		{
			RateCategory,
			Mode,
			Origin,
			Destination,
			Via,
			WiseCarrierCode,
			ContractNumber,
			CGReference,
			RateProvider,
			ControllingCustomer,
			Consignor,
			Consignee,
			ServiceLevel,
			CarrierServiceLevel,
			{ GatewayServiceLevel, IsIntercompanyTariff },
			{ ShipmentGatewayServiceLevel, IsIntercompanyTariff },
			CommodityGroup,
			ProductName,
			Commodities,
			Container,
			ContainerQuality,
			ContainerPayloadWeight,
			ContainerPayloadVolume,
			MatchContainerClass,
			RateStartDate,
			RateEndDate,
			AllInCost,
			FreightRatePerChargeableUnit,
			UniversalCarrierServiceLevel,
			CargoguideProductCode,
		};
	}
}


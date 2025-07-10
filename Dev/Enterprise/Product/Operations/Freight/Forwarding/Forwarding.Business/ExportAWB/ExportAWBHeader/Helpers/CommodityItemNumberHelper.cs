using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	static class CommodityItemNumberHelper
	{
		public static ZString GetCommodityItemNumberFromShipments(CommonConsol consol)
		{
			var shipmentCommodities = consol
				.Shipments
				.Cast<CommonShipment>()
				.SelectMany(shipment => shipment.OuterPackLines)
				.Cast<PackLine>()
				.Select(packLine => packLine.Commodity)
				.WhereNotNull()
				.Select(commodity => commodity.RH_IATACommodityItem)
				.Distinct();

			if (shipmentCommodities.IsCountEqualTo(1) && shipmentCommodities.FirstOrDefault() == ZString.Empty)
			{
				return ZString.Empty;
			}

			return GetCommodityItemNumberFromCommodities(shipmentCommodities);
		}

		public static ZString GetCommodityItemNumberFromULDContainerPackLines(IEnumerable<CommonContainer> uldContainers)
		{
			var packlineCommodities = uldContainers
				.SelectMany(container => container.PackLines)
				.Cast<PackLine>()
				.Select(packLine => packLine.Commodity)
				.WhereNotNull()
				.Select(commodity => commodity.RH_IATACommodityItem)
				.Distinct();

			return GetCommodityItemNumberFromCommodities(packlineCommodities);
		}

		public static ZString GetCommodityItemNumberFromULDContainers(IEnumerable<CommonContainer> uldContainers)
		{
			var containerCommodities = uldContainers
				.Select(container => container.ContainerCommodityCode)
				.WhereNotNull()
				.Select(commodity => commodity.RH_IATACommodityItem)
				.Distinct();

			return GetCommodityItemNumberFromCommodities(containerCommodities);
		}

		static ZString GetCommodityItemNumberFromCommodities(IEnumerable<ZString> commodityItems)
		{
			if (!commodityItems.Any())
			{
				return ZString.Empty;
			}

			if (commodityItems.IsCountEqualTo(1) && commodityItems.FirstOrDefault() != ZString.Empty)
			{
				return commodityItems.FirstOrDefault();
			}
			else
			{
				return mixedCommodityItemNumber;
			}
		}

		public const string mixedCommodityItemNumber = "9999";
	}
}

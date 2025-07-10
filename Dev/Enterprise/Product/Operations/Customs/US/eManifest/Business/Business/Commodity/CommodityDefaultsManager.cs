using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.US.eManifest.Business
{
	class CommodityDefaultsManager
	{
		internal CommodityDefaultsManager(Commodity commodity)
		{
			this.commodity = commodity;
			shipment = commodity.Shipment;
		}

		internal void SetDefaults()
		{
			if (shipment != null)
			{
				commodity.BY_ManifestUnitCode = shipment.B0_ManifestUQ.IsEmpty ? ZString.Empty : shipment.B0_ManifestUQ.Left(commodity.BY_ManifestUnitCodeInfo.MaxLength);
				commodity.BY_GrossWeightUnit = shipment.B0_WeightUQ;
				if (commodity.BY_Description.IsEmpty)
				{
					commodity.BY_Description = shipment.B0_DescriptionOfCargo.Left(commodity.BY_DescriptionInfo.MaxLength);
				}
				commodity.BY_RN_NKCountryOfOrigin = shipment.B0_RN_NKCountryOfExport;

				var tripEquipment = shipment?.Trip?.AllEquipmentIncludingMainConveyance.FirstOrDefault();
				if (tripEquipment != null && !tripEquipment.IsEmpty)
				{
					commodity.BY_BJ_Equipment = tripEquipment.PK;
				}
			}
		}
		readonly Commodity commodity;
		readonly Shipment shipment;
	}
}

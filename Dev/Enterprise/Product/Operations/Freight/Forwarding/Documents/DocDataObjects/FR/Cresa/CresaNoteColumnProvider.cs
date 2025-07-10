using CargoWise.Types;
using Enterprise.Warehouse.Transit.Business;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	class CresaNoteColumnProvider : TransitLogTableHelper<Cresa, TransitLogColumnIDs.CRESAColumn>
	{
		protected override ZString GetHeader(TransitLogColumnIDs.CRESAColumn columnID)
		{
			switch (columnID)
			{
				case TransitLogColumnIDs.CRESAColumn.OperationalPort:
					return Res.GetString("2c410662-3d18-4e5f-9015-c33bd5b2d4a8", "Operational Port");
				case TransitLogColumnIDs.CRESAColumn.PCS:
					return Res.GetString("6db0625d-580f-4c9c-be6b-c6005f1537b1", "PCS");
				case TransitLogColumnIDs.CRESAColumn.TransportMode:
					return Res.GetString("8f33d832-5a74-4744-bc29-6274d38a0d59", "Transport Mode");
				case TransitLogColumnIDs.CRESAColumn.TranshipmentPort:
					return Res.GetString("9025e966-81c7-413c-a0a5-544543f805a8", "Transhipment Port");
				case TransitLogColumnIDs.CRESAColumn.PortOfArrival:
					return Res.GetString("416635af-84a2-4f63-8cbf-f27445ea0205", "Port of Arrival");
				case TransitLogColumnIDs.CRESAColumn.PortArea:
					return Res.GetString("d0e627a3-905a-4210-b24d-0e6e0ba69aa4", "Port Area");
				case TransitLogColumnIDs.CRESAColumn.PortServiceReference:
					return Res.GetString("7c93b925-bb07-48e5-9be2-48e757ba2145", "Port Service Reference");
				case TransitLogColumnIDs.CRESAColumn.PortLocation:
					return Res.GetString("9dedf719-35d5-474c-9762-4ba21f881670", "Port Location");
				case TransitLogColumnIDs.CRESAColumn.CargoReceiptDate:
					return Res.GetString("a80eeab8-0e6e-48a3-8b25-31a04ac72229", "Cargo Receipt Date");
				case TransitLogColumnIDs.CRESAColumn.ETAatPortOfArrival:
					return Res.GetString("01140731-95dd-47e6-a4f6-dc16e134d722", "ETA at Port of Arrival");
				case TransitLogColumnIDs.CRESAColumn.Buyer:
					return Res.GetString("e5d7d2ad-24c2-4986-887c-a1d220887f0b", "Buyer");
				case TransitLogColumnIDs.CRESAColumn.Supplier:
					return Res.GetString("23c0abc3-009e-4d41-83bf-41526f89deed", "Supplier");
				case TransitLogColumnIDs.CRESAColumn.SendingParty:
					return Res.GetString("e887047b-7d05-4336-b5fb-b82f23bfe16e", "Sending Party");
				case TransitLogColumnIDs.CRESAColumn.Forwarder:
					return Res.GetString("5ab949f4-6356-4a47-9074-01f55b9f2a03", "Forwarder");
				case TransitLogColumnIDs.CRESAColumn.Agent:
					return Res.GetString("40b27759-aa4a-4d62-9f14-81eb5ae6f869", "Agent");
				case TransitLogColumnIDs.CRESAColumn.BookingReference:
					return Res.GetString("f8948b6c-c67c-4d3d-b7e6-9c5663a7a607", "Booking Reference");
				case TransitLogColumnIDs.CRESAColumn.WarehouseEntryNumber:
					return Res.GetString("18f394bd-a4be-43da-8a75-15b29b28dc28", "Warehouse Entry Number");
				case TransitLogColumnIDs.CRESAColumn.ECVReference:
					return Res.GetString("3ccfbae3-d012-4eca-9cb6-95a770cdd4cc", "ECV Reference");
				case TransitLogColumnIDs.CRESAColumn.CRESAReference:
					return Res.GetString("d98f350c-2aa4-4aa3-8409-5c0baae91c7b", "CRESA Reference");
				case TransitLogColumnIDs.CRESAColumn.ShipmentNumber:
					return Res.GetString("1e08e768-584c-4701-adba-ee26c2914efd", "Shipment Number");
				case TransitLogColumnIDs.CRESAColumn.CommodityReference:
					return Res.GetString("50b147d3-4871-4738-8cf6-45ef246de3d4", "Commodity Reference");
				case TransitLogColumnIDs.CRESAColumn.GoodsIn:
					return Res.GetString("a454d44d-5213-4637-99ff-a3a471b2ea5f", "Goods In");
				case TransitLogColumnIDs.CRESAColumn.GoodsAreSealed:
					return Res.GetString("444b9e97-9668-455c-9fd0-35c521dee242", "Goods are Sealed");
				default:
					return ZString.Empty;
			}
		}

		protected override ZString GetValue(Cresa cresa, TransitLogColumnIDs.CRESAColumn columnID)
		{
			switch (columnID)
			{
				case TransitLogColumnIDs.CRESAColumn.OperationalPort:
					return cresa.OperationalPort.Code;
				case TransitLogColumnIDs.CRESAColumn.PCS:
					return cresa.PCS;
				case TransitLogColumnIDs.CRESAColumn.TransportMode:
					return cresa.TransportMode;
				case TransitLogColumnIDs.CRESAColumn.TranshipmentPort:
					return cresa.PortOfTranshipment.Code;
				case TransitLogColumnIDs.CRESAColumn.PortOfArrival:
					return cresa.PortOfArrival.Code;
				case TransitLogColumnIDs.CRESAColumn.PortArea:
					return cresa.PortArea;
				case TransitLogColumnIDs.CRESAColumn.PortServiceReference:
					return cresa.PortServiceCodeReference;
				case TransitLogColumnIDs.CRESAColumn.PortLocation:
					return cresa.PortLocation;
				case TransitLogColumnIDs.CRESAColumn.CargoReceiptDate:
					return cresa.CargoReceiptDate.ToString();
				case TransitLogColumnIDs.CRESAColumn.ETAatPortOfArrival:
					return cresa.ETA.ToString();
				case TransitLogColumnIDs.CRESAColumn.Buyer:
					return cresa.Buyer.CompanyName;
				case TransitLogColumnIDs.CRESAColumn.Supplier:
					return cresa.Supplier.CompanyName;
				case TransitLogColumnIDs.CRESAColumn.SendingParty:
					return cresa.SendingParty.CompanyName;
				case TransitLogColumnIDs.CRESAColumn.Forwarder:
					return cresa.SendingForwarder.CompanyName;
				case TransitLogColumnIDs.CRESAColumn.Agent:
					return cresa.Agent.CompanyName;
				case TransitLogColumnIDs.CRESAColumn.BookingReference:
					return cresa.CarrierBookingReference;
				case TransitLogColumnIDs.CRESAColumn.WarehouseEntryNumber:
					return cresa.EntryNumber;
				case TransitLogColumnIDs.CRESAColumn.ECVReference:
					return cresa.ECVReference;
				case TransitLogColumnIDs.CRESAColumn.CRESAReference:
					return cresa.CRESAReference;
				case TransitLogColumnIDs.CRESAColumn.ShipmentNumber:
					return cresa.ShipmentNumber;
				case TransitLogColumnIDs.CRESAColumn.CommodityReference:
					return cresa.CommodityReference;
				case TransitLogColumnIDs.CRESAColumn.GoodsIn:
					return cresa.GoodsInDateTime.ToString();
				case TransitLogColumnIDs.CRESAColumn.GoodsAreSealed:
					return cresa.GoodsSealed ? Res.GetString("6980a47a-7dfe-4012-b49c-4ca72915067d", "Yes") : Res.GetString("7773fb0d-dbd3-435a-941a-3b9c2216ff2a", "No");
				default:
					return ZString.Empty;
			}
		}
	}
}

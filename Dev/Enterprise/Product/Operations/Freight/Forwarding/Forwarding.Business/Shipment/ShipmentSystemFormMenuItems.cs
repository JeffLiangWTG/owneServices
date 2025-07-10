using System.Diagnostics.CodeAnalysis;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Freight.Forwarding.Business
{
	[ThreadSafe]
	[SuppressMessage("Microsoft.Usage", "CA2211: Non-constant fields should not be visible")]
	public static class ShipmentSystemFormMenuItems
	{
		public static ZGuid YusenBillOfLadingPK => new ZGuid("9aa06efa-4a07-475f-9523-04a0c0e63346");
		public static ZGuid DHLBillOfLadingPK => new ZGuid("40f9b079-fe3d-4d6b-888e-85f39e960bc5");
		public static ZGuid EasipassPK => new ZGuid("c9f380b3-6dc9-4640-ac24-925713b0ed9a");
		public static ZGuid DocumentMenuACASShipmentReport => new ZGuid("4BE1F18B-6B93-4AA8-8E25-522615F7D012");
		public static ZGuid DocumentMenuCCTShipmentReport => new ZGuid("005EEF75-4C34-428F-9F73-850055B743B7");
		public static ZGuid PreprintedBillOfLadingPK => new ZGuid("1D86E64D-004C-4C97-85DD-3DBBBDA701EF");
		public static ZGuid DocumentMenuCAEDRequestFRPortMessagingExportPK => new ZGuid("d3858f60-7ff7-45f3-94e3-b4ef707df4c3");
		public static ZGuid DocumentMenuCAEDRequestFRPortMessagingImportPK => new ZGuid("c7f551e2-4d7a-4ece-8d61-f0ee037c4b45");
		public static ZGuid DocumentMenuGoodsReceivedFRPortMessagingExportPK => new ZGuid("6530e4b6-6872-4dfc-a0e1-25761e36fa6f");
		public static ZGuid DocumentMenuDOSRequestFRPortMessagingExportPK => new ZGuid("c98b97dd-929c-46e4-8802-4e2bae067ac8");
		public static ZGuid DocumentMenuDOSRequestFRPortMessagingImportPK => new ZGuid("09298223-c90a-4353-b4ba-5518ad896492");
		public static ZGuid BillOfLadingPK => ForwardingConstants.SystemFormMenuItems.BillOfLadingPK;
		public static ZGuid BookingRequestPK => new ZGuid("5fce62d9-cc1f-49b3-9fba-b569ec1a9467");
		public static ZGuid DocumentMenuXFZBRequestMXPortMessagingExportPK => new ZGuid("4dd5702d-4dda-4b76-bb50-ac3ddbaf284d");
		public static ZGuid DocumentMenuConsolidationAdvicePK => new ZGuid("c6d40edc-7241-47dc-89ac-9aab96bef1ae");
		public static ZGuid DocumentMenuCarrierBillOfLadingPK => new ZGuid("d23fd083-3b81-4539-9c26-837c67cbcaa2");
		public static ZGuid CINExportNotificationPK => new ZGuid("bc0bf826-9bd6-478d-b44d-52a3f1ba1129");
		public static ZGuid SendDeliveryOrderPK = new ZGuid("F480D88E-8437-4700-9551-6EDD4A2DFEC6");
		public static ZGuid SendGatePassMovementPK = new ZGuid("6432A649-23A8-4B3B-AA1D-E56047A29DC5");
	}
}

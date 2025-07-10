using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Manifest.Business
{
	class N5101HGoodsShipmentConsignment : IN5101HConsignment
	{
		public N5101HGoodsShipmentConsignment(AsycudaBill bill)
		{
			this.bill = bill;
		}

		readonly AsycudaBill bill;

		ZDecimal IN5101HConsignment.BoardedQuantity => ZDecimal.Zero;

		ZDecimal IN5101HConsignment.TotalPackageQuantity => ZDecimal.Zero;

		ZString IN5101HConsignment.EscortMark => ZString.Empty;

		ZDecimal IN5101HConsignment.TotalGrossMassMeasure => ZDecimal.Zero;

		ZString IN5101HConsignment.TypeCode => ZString.Empty;

		ZString IN5101HConsignment.AssociatedTransportDocumentId => ZString.Empty;

		IPartyDetails IN5101HConsignment.Consignee => null;

		IN5101HConsignmentItem IN5101HConsignment.ConsignmentItem => null;

		IPartyDetails IN5101HConsignment.Consignor => null;

		ZString IN5101HConsignment.GoodsLocationId => bill?.ABL_GoodsLocation ?? ZString.Empty;

		ILocation IN5101HConsignment.LoadingLocation => null;

		IEnumerable<IPartyDetails> IN5101HConsignment.NotifyParties => null;

		ITransportContractDocument IN5101HConsignment.TransportContractDocument => new N5101HTransportContractDocument(bill);

		IEnumerable<ITransportEquipment> IN5101HConsignment.TransportEquipments => null;

		ZString IN5101HConsignment.UnloadingLocationId => ZString.Empty;
	}
}

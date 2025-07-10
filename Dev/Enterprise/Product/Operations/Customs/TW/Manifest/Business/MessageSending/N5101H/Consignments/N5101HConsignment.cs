using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class N5101HConsignment : IN5101HConsignment
	{
		public N5101HConsignment(AsycudaBill bill, ZDecimal bagQuantity)
		{
			this.bill = bill;
			if (!bill.BagNumber.IsEmpty)
			{
				BoardedQuantity = bagQuantity;
			}
		}

		readonly AsycudaBill bill;

		public ZDecimal BoardedQuantity { get; }

		public ZDecimal TotalPackageQuantity => (ZDecimal)bill.ABL_ManifestQty;

		public ZString EscortMark => bill.ABL_SpecialCargoCode;

		public ZDecimal TotalGrossMassMeasure => Core.Constants.Weight.Convert(bill.ABL_GrossWeight, bill.ABL_GrossWeightUQ, Core.Constants.Weight.Kilograms);

		public ZString TypeCode => bill.ABL_ShipmentType;

		public ZString AssociatedTransportDocumentId => bill.BagNumber;

		public IPartyDetails Consignee => new N5101HConsignee(bill);

		public IN5101HConsignmentItem ConsignmentItem => new N5101HConsignmentItem(bill);

		public IPartyDetails Consignor => new N5101HConsignor(bill);

		public ZString GoodsLocationId => bill?.ABL_GoodsLocation ?? ZString.Empty;

		public ILocation LoadingLocation => new N5101HLoadingLocation(bill);

		public IEnumerable<IPartyDetails> NotifyParties => new[] { new N5101HNotifyParty(bill) };

		public ITransportContractDocument TransportContractDocument => new N5101HTransportContractDocument(bill);

		public IEnumerable<ITransportEquipment> TransportEquipments => bill.AsycudaBillLinkAsycudaContainers.Cast<AsycudaBillLinkAsycudaContainer>().Where(x => x.Link).Select(x => new N5101HTransportEquipment(x));

		public ZString UnloadingLocationId => bill?.ABL_RL_NKFinalDestination ?? ZString.Empty;
	}
}

using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business.N5301
{
	public class Consignment : IConsignment
	{
		public Consignment(CusInBondHeader header)
		{
			this.header = header;
			moveHeader = header.MovementHeader;
		}

		readonly CusInBondHeader header;
		readonly CusInBondMoveHeader moveHeader;

		public ZString ManifestSerialNumber => header.ArrivalBill?.B0_ReferenceID ?? ZString.Empty;

		public IEnumerable<IAdditionalInformation> AdditionalInformations => null;

		public ZString ArrivalTransportMeansTypeCode => ZString.Empty;

		public ITransportMeans BorderTransportMeans => null;

		public IPartyDetails Carrier => null;

		public IConsignmentItem ConsignmentItem => new ConsignmentItem(header);

		public ZString GoodsLocation => ZString.Empty;

		public ILocation LoadingLocation => new LocationWrapper(header.BH_RL_NKImportLoadPort);

		public IEnumerable<ITransportContractDocument> TransportContractDocuments => header.GetTransportContractDocuments(header.ArrivalBill, (id, typeCode) => new TransportContractDocumentWrapper(id, typeCode));

		public IEnumerable<ITransportEquipment> TransportEquipments
		{
			get
			{
				var containers = moveHeader?.InBondMoveDetail?.Containers;
				if (containers != null)
				{
					foreach (var cusInBondContainer in containers)
					{
						yield return new TransportEquipment(cusInBondContainer);
					}
				}
			}
		}

		public IBondedGoods BondedGoods => null;

		public ZString ShippingOrderNumber => header.MovementBill?.B0_ReferenceID ?? ZString.Empty;

		public ITransportMeans DepartureTransportMeans
		{
			get
			{
				ITransportMeans departureTransportMeans = null;
				if (moveHeader != null)
				{
					departureTransportMeans = new DepartureTransportMeans(moveHeader);
				}
				return departureTransportMeans;
			}
		}

		public ZString TransitTransportMeansTypeCode => moveHeader?.BM_ExportTransportMode ?? ZString.Empty;

		public IEnumerable<ZString> GoodsLocations => null;

		public ILocation UnloadingLocation => new LocationWrapper();

		public IGovernmentAgencyGoodsItem GovernmentAgencyGoodsItem => null;

		public ILocation TranshipmentLocation => null;

		public ILocation TransitDeparture => null;
	}
}

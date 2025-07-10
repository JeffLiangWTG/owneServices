using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business.N5203
{
	public class Consignment : IConsignment
	{
		public Consignment(CusEntryHeader entryHeader)
		{
			this.entryHeader = entryHeader;
			declaration = entryHeader.Declaration;
		}

		readonly CusEntryHeader entryHeader;
		readonly JobDeclaration declaration;

		public ZString ManifestSerialNumber => ZString.Empty;

		public IEnumerable<IAdditionalInformation> AdditionalInformations => declaration.GetAdditionalInformations();

		public ZString ArrivalTransportMeansTypeCode => ZString.Empty;

		public ITransportMeans BorderTransportMeans => new BorderTransportMeans(declaration);

		public IPartyDetails Carrier => null;

		public IConsignmentItem ConsignmentItem => null;

		public ZString GoodsLocation => ZString.Empty;

		public ILocation LoadingLocation => new LocationWrapper(declaration?.JE_RL_NKOrigin ?? ZString.Empty);

		public IEnumerable<ITransportContractDocument> TransportContractDocuments => declaration?.GetTransportContractDocumentsWithMasterBillSegmentID((id, typeCode) => new TransportContractDocumentWrapper(id, typeCode));

		public IEnumerable<ITransportEquipment> TransportEquipments
		{
			get
			{
				var containers = declaration?.CusContainers;
				if (containers != null)
				{
					foreach (var cusContainer in containers)
					{
						yield return new TransportEquipmentWrapper(cusContainer);
					}
				}
			}
		}

		public IBondedGoods BondedGoods => new BondedGoods(entryHeader);

		public ZString ShippingOrderNumber => declaration?.JE_SLD ?? ZString.Empty;

		public ITransportMeans DepartureTransportMeans => new DepartureTransportMeans(declaration);

		public ZString TransitTransportMeansTypeCode => ZString.Empty;

		public IEnumerable<ZString> GoodsLocations
		{
			get
			{
				var twGoodsLocation = entryHeader?.EntryInstruction?.CEI_GoodsLocation ?? ZString.Empty;
				var jeLocationOfGoods = declaration?.JE_LocationOfGoods ?? ZString.Empty;
				yield return twGoodsLocation;

				if (twGoodsLocation != jeLocationOfGoods && !jeLocationOfGoods.IsEmpty)
				{
					yield return jeLocationOfGoods;
				}
			}
		}

		public ILocation UnloadingLocation => new LocationWrapper(declaration?.JE_RL_NKFinalDestination ?? ZString.Empty);

		public IGovernmentAgencyGoodsItem GovernmentAgencyGoodsItem => null;

		public ILocation TranshipmentLocation => null;

		public ILocation TransitDeparture => null;
	}
}

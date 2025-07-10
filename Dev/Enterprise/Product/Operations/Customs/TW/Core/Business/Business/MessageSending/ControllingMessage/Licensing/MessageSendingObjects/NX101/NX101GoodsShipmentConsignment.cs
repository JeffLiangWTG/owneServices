using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class NX101GoodsShipmentConsignment : IConsignment
	{
		readonly CusTWControllingMessageHeader header;
		readonly JobDeclaration declaration;

		public NX101GoodsShipmentConsignment(CusTWControllingMessageHeader header)
		{
			this.header = Argument.NotNull(header, nameof(header));
			declaration = Argument.NotNull(header.Declaration, nameof(header.Declaration));
		}

		public ZString ManifestSerialNumber => ZString.Empty;

		public IEnumerable<IAdditionalInformation> AdditionalInformations
		{
			get
			{
				var notes = header.TW_Notes;
				return notes.IsEmpty ? Enumerable.Empty<IAdditionalInformation>() : new List<AdditionalInformationWrapper> { new AdditionalInformationWrapper("", notes) };
			}
		}

		public ZString ArrivalTransportMeansTypeCode => ZString.Empty;

		public ITransportMeans BorderTransportMeans => new TransportMeansWrapper(declaration.JE_VoyageFlightNo);

		public IPartyDetails Carrier => null;

		public IConsignmentItem ConsignmentItem => null;

		public ZString GoodsLocation => ZString.Empty;

		public ILocation LoadingLocation
		{
			get
			{
				var isCertificate15 = header.IsCertificate15;
				var loadingLocationLoadingDateTime = isCertificate15 ? declaration.JE_ExportDate.Date : ZDate.Empty;
				var loadingLocationEstimatedLoadingCode = isCertificate15 ? new ZString(header.TW1_IsEstimatedLoadingDate ? YesNoList.Codes.Yes : YesNoList.Codes.No) : ZString.Empty;
				var isPortRequiredCertificateTypes = header.IsPortRequiredCertificateTypes;
				return new LocationWrapper(isPortRequiredCertificateTypes ? header.TW1_RL_NKPortOfLoading : ZString.Empty, isPortRequiredCertificateTypes ? header.TW1_PortOfLoadingName : ZString.Empty, loadingLocationLoadingDateTime, loadingLocationEstimatedLoadingCode);
			}
		}

		public IEnumerable<ITransportContractDocument> TransportContractDocuments => null;

		public IEnumerable<ITransportEquipment> TransportEquipments => declaration.CusContainers.Select(x => new TransportEquipmentWrapper(x));

		public IBondedGoods BondedGoods => null;

		public ZString ShippingOrderNumber => ZString.Empty;

		public ITransportMeans DepartureTransportMeans
		{
			get
			{
				var vesselName = declaration.JE_VesselName;
				return vesselName.IsEmpty ? null : new TransportMeansWrapper(vesselName);
			}
		}

		public ZString TransitTransportMeansTypeCode => ZString.Empty;

		public IEnumerable<ZString> GoodsLocations => null;

		public ILocation UnloadingLocation => new LocationWrapper(declaration.JE_RL_NKFinalDestination);

		public IGovernmentAgencyGoodsItem GovernmentAgencyGoodsItem => null;

		public ILocation TranshipmentLocation => null;

		public ILocation TransitDeparture => null;
	}
}

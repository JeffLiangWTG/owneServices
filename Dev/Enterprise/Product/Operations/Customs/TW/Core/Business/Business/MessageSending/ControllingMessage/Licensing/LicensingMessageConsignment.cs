using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class LicensingMessageConsignment : IConsignment
	{
		CusTWControllingMessageHeader Header { get; }
		protected JobDeclaration Declaration { get; }

		public LicensingMessageConsignment(CusTWControllingMessageHeader header)
		{
			Header = Argument.NotNull(header, nameof(header));
			Declaration = Argument.NotNull(header.Declaration, nameof(header.Declaration));
		}

		JobComInvoiceLine FirstInvoiceLine => Header.Factory.GetValue(ref firstInvoiceLineCached, () => Header.ControllingMessageHeaderLinkInvoiceLines.Where(x => x.Link).Select(h => h.Invoiceline).OrderBy(l => l.JI_LineNo).FirstOrDefault());
		CachedProperty<JobComInvoiceLine> firstInvoiceLineCached;

		public ZString ManifestSerialNumber => GetManifestSerialNumberCore();

		protected virtual ZString GetManifestSerialNumberCore() => default;

		public IEnumerable<IAdditionalInformation> AdditionalInformations => GetAdditionalInformationsCore();

		protected virtual IEnumerable<IAdditionalInformation> GetAdditionalInformationsCore() => Enumerable.Empty<IAdditionalInformation>();

		public ZString ArrivalTransportMeansTypeCode => GetArrivalTransportMeansTypeCodeCore();

		protected virtual ZString GetArrivalTransportMeansTypeCodeCore() => ZString.Empty;

		public ITransportMeans BorderTransportMeans => GetBorderTransportMeansCore();

		protected virtual ITransportMeans GetBorderTransportMeansCore() => new LicensingMessageTransportMeans(Declaration);

		public IPartyDetails Carrier => new PartyDetailsWrapper(ZString.Empty, ZString.Empty, ZString.Empty, null);

		public IConsignmentItem ConsignmentItem => FirstInvoiceLine != null ? new LicensingMessageConsignmentItem(FirstInvoiceLine) : null;

		public ZString GoodsLocation => Declaration.CusEntryInstruction.CEI_GoodsLocation;

		public ILocation LoadingLocation => GetLoadingLocationCore();

		protected virtual ILocation GetLoadingLocationCore() => new LocationWrapper(Declaration.JE_RL_NKOrigin);

		public IEnumerable<ITransportContractDocument> TransportContractDocuments => GetTransportContractDocumentsCore();

		protected virtual IEnumerable<ITransportContractDocument> GetTransportContractDocumentsCore() => default;

		public IEnumerable<ITransportEquipment> TransportEquipments => GetTransportEquipmentsCore();

		protected virtual IEnumerable<ITransportEquipment> GetTransportEquipmentsCore() => default;

		public IBondedGoods BondedGoods => null;

		public ZString ShippingOrderNumber => null;

		public ITransportMeans DepartureTransportMeans => GetDepartureTransportMeans();

		protected virtual ITransportMeans GetDepartureTransportMeans() => new LicensingMessageTransportMeans(Declaration);

		public ZString TransitTransportMeansTypeCode => null;

		public IEnumerable<ZString> GoodsLocations => null;

		public ILocation UnloadingLocation => GetUnloadingLocationCore();

		protected virtual ILocation GetUnloadingLocationCore() => new LocationWrapper();

		public IGovernmentAgencyGoodsItem GovernmentAgencyGoodsItem => new LicensingMessageConsignmentGovernmentAgencyGoodsItem(FirstInvoiceLine);

		public ILocation TranshipmentLocation => GetTranshipmentLocationCore();

		protected virtual ILocation GetTranshipmentLocationCore() => new LocationWrapper();

		public ILocation TransitDeparture => GetTransitDepartureCore();

		protected virtual ILocation GetTransitDepartureCore() => new LocationWrapper();
	}
}

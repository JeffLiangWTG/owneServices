using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class NX401ExportMessageSendingObject : NX401MessageSendingObject
	{
		public NX401ExportMessageSendingObject(CusTWControllingMessageHeader header) : base(header)
		{
		}

		protected override IGoodsShipment GetGoodsShipmentCore() => new NX401ExportLicensingMessageGoodsShipment(Header);

		protected override IPartyDetails GetImporterCore() => default;

		protected override ITransportMeans GetTransportMeansCore() => new NX401ExportLicensingMessageTransportMeans(Declaration);
	}
}

using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Business
{
	public interface IGatePassMovementProvider : IILElectronicMessageProvider
	{
		ZString SourceType { get; }
		ZString SourceID { get; }
		ZString ProcessType { get; }
		ZString OriginSiteCode { get; }
		ZString DestinationSiteCode { get; }
		ZString CargoTypeCode { get; }
		ZString CargoIdentifierTypeCode { get; }
		ZString CargoIdentifierKey1 { get; }
		ZString CargoIdentifierKey2 { get; }
		ZString CargoIdentifierKey3 { get; }
		ZBool CargoIdentifierKey3IsVisible { get; }
		ZString TransportMode { get; }
		CodeDescriptionPairList CargoTypeCodeCollection { get; }
	}
}

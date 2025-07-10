using CargoWise.Types;

namespace Enterprise.Customs.NO.Manifest.Business;

interface ITransportModeProvider
{
	ZString TransportMode { get; }
}

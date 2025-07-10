using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.US.AIM.Messaging;

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class AIMCargoControlLocation : IAIMCargoControlLocation
	{
		public AIMCargoControlLocation(AsycudaManifestHeader manifestHeader)
		{
			this.manifestHeader = Argument.NotNull(manifestHeader, "manifestHeader");
		}

		readonly AsycudaManifestHeader manifestHeader;

		public ZString AirportOfArrival
		{
			get
			{
				return manifestHeader.PortOfFirstArrival?.RL_IATA ?? (manifestHeader.PortOfDischarge?.RL_IATA ?? ZString.Empty);
			}
		}

		public ZString CargoTerminalOperator => manifestHeader.AMA_CarrierCode.Left(3);
	}
}

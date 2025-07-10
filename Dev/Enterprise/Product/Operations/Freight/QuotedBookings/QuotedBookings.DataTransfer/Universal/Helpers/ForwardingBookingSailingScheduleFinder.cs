using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Universal
{
	sealed class ForwardingBookingSailingScheduleFinder : TransportLegSailingScheduleFinder
	{
		public ForwardingBookingSailingScheduleFinder(UniversalObjectFactory universalObjectFactory, TransportLeg transportLeg, IXmlImportLogger logger)
			: base(universalObjectFactory, transportLeg, logger)
		{
		}

		protected override bool SailingCreationAllowedCore(SailingDetails details, RefVessel refVessel)
		{
			return details.TransportMode != Core.Constants.TransportModes.Sea || refVessel != null;
		}

		protected override ZGuid GetCarrierFromTransportParent()
		{
			return ZGuid.Empty;
		}
	}
}

using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	public class ForwardingConsolTRCDetailsProvider : ITRCDetails
	{
		public ForwardingConsolTRCDetailsProvider(ForwardingConsol consol)
		{
			this.consol = consol;
		}
		readonly ForwardingConsol consol;

		BusinessObject ITRCDetails.BusinessObject => consol;

		ZString ITRCDetails.SourceType => nameof(DataContextType.ForwardingConsol);

		ZString ITRCDetails.SourceID => consol.JK_UniqueConsignRef;

		CommonContainer[] ITRCDetails.Containers => consol.Containers.Cast<ForwardingContainer>().ToArray();

		ZString ITRCDetails.PortOfOrigin => consol.JK_RL_NKLoadPort;

		ZString ITRCDetails.PortOfDestination => consol.JK_RL_NKDischargePort;

		RefUNLOCO ITRCDetails.OperationalPortImport => consol.Transports?.LastTransportWithTransportMode(Core.Constants.TransportModes.Sea)?.DiscPort;

		RefUNLOCO ITRCDetails.OperationalPortExport => consol.Transports?.FirstTransportWithTransportMode(Core.Constants.TransportModes.Sea)?.LoadPort;

		ZString ITRCDetails.BookingConfirmationReference => consol.JK_BookingReference;

		CodeDescriptionPairList ITRCDetails.ContainerModeList => consol.JK_ConsolMode_List;
		ZString ITRCDetails.ContainerMode => consol.JK_ConsolMode;

		CodeDescriptionPairList ITRCDetails.ShipmentTypeList => consol.JK_AgentType_List;
		ZString ITRCDetails.ShipmentType => consol.JK_AgentType;

		ZString ITRCDetails.WaybillNumber => consol.JK_MasterBillNum;

		OrgAddress ITRCDetails.ReceivingForwarder => consol.ReceivingForwarderAddress;

		OrgAddress ITRCDetails.SendingForwarder => consol.SendingForwarderAddress;

		OrgAddress ITRCDetails.Carrier => consol.ShippingLineAddress;

		ZDateTime ITRCDetails.ETD => SeaTransportsInLegOrder.FirstOrDefault()?.JW_ETD ?? ZDateTime.Empty;
		ZDateTime ITRCDetails.ETA => SeaTransportsInLegOrder.LastOrDefault()?.JW_ETA ?? ZDateTime.Empty;

		IReadOnlyCollection<Freight.Business.Transport> SeaTransportsInLegOrder => seaTransportsInLegOrder ?? (seaTransportsInLegOrder = GetSeaTransports());
		IReadOnlyCollection<Freight.Business.Transport> seaTransportsInLegOrder;

		IReadOnlyCollection<Freight.Business.Transport> GetSeaTransports()
		{
			consol.Transports.Sort(MovementLegComparer.PortsAndDatesBased(consol.Transports));
			return consol.Transports
				.OfType<Freight.Business.Transport>()
				.Where(t => t.JW_TransportMode == Core.Constants.TransportModes.Sea)
				.ToArray();
		}
	}
}

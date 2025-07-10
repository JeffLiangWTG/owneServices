using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Security;

namespace Enterprise.Freight.Forwarding.Business
{
	public class NonMatchingAgentsSecurity : SecurityOverridenLogin
	{
		public NonMatchingAgentsSecurity()
			: this(Enumerable.Empty<CommonConsol>(), Enumerable.Empty<CommonShipment>())
		{
		}

		public NonMatchingAgentsSecurity(IEnumerable<CommonConsol> consols, IEnumerable<CommonShipment> shipments)
			: base()
		{
			if (consols.Any() && shipments.Any())
			{
				var checkRelatedReceivingAgent = FreightShipmentVsConsolMessageHelper.Instance.CheckRelatedReceivingAgents(shipments, consols);
				var checkRelatedSendingAgent = FreightShipmentVsConsolMessageHelper.Instance.CheckRelatedSendingAgents(shipments, consols);

				if (!string.IsNullOrEmpty(checkRelatedReceivingAgent))
				{
					Message = checkRelatedReceivingAgent;
				}
				if (!string.IsNullOrEmpty(checkRelatedSendingAgent))
				{
					Message = (!Message.IsEmpty) ? (Message + "\r\n" + checkRelatedSendingAgent) : checkRelatedSendingAgent;
				}
			}
		}

		public ZString Message { get; private set; }
	}
}

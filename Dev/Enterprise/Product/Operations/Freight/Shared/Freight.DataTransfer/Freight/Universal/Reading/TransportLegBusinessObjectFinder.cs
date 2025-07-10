using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.Freight.Business;
using Enterprise.Integration.Schedule;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class TransportLegBusinessObjectFinder : MatchingBusinessObjectFinder<TransportLeg, Transport>
	{
		public TransportLegBusinessObjectFinder(TransportLeg dataObject, ITransportParentCommon transportParent)
			: base(dataObject)
		{
			this.transportParent = Argument.NotNull(transportParent, "transportParent");
		}

		readonly ITransportParentCommon transportParent;

		#region Implementation

		protected override Transport FindCore(IEnumerable<Transport> transportLegs)
		{
			Transport result = null;

			var parentType = transportParent.GetType();

			foreach (Transport transport in transportLegs)
			{
				if (MatchesLoadAndDischargePorts(transport))
				{
					result = transport;

					if (transport.ParentType == parentType)
					{
						break;
					}
				}
			}

			return result;
		}

		protected bool MatchesLoadAndDischargePorts(Transport transport)
		{
			return transport.JW_RL_NKLoadPort == dataObject.PortOfLoading.GetUNLOCOAsUpperCase(transport.Factory) &&
				transport.JW_RL_NKDiscPort == dataObject.PortOfDischarge.GetUNLOCOAsUpperCase(transport.Factory);
		}

		#endregion
	}
}

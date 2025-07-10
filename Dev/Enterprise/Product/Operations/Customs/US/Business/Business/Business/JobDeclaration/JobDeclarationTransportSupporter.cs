using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.US.Business
{
	public class JobDeclarationTransportSupporter : BaseJobDeclarationTransportSupporter<JobDeclaration>
	{
		public JobDeclarationTransportSupporter(JobDeclaration jobDeclaration)
			: base(jobDeclaration)
		{
		}

		protected override void NotifyTransportTypeChangedCore(Transport transport, ZString previousValue)
		{
			if (transport.JW_TransportMode == Core.Constants.TransportModes.Road)
			{
				Parent.UpdateRoutingDefaultIfAllowedAndSingleLeg(Parent.JE_TransportModeInfo, (ZString)Core.Constants.TransportModes.Truck, transport);
			}
			else
			{
				base.NotifyTransportTypeChangedCore(transport, previousValue);
			}
		}

		protected override void NotifyDischargeChangedCore(Transport transport, ZString previousValue)
		{
			base.NotifyDischargeChangedCore(transport, previousValue);

			var parent = Parent;
			var transports = parent.Transports;
			if (parent.IsImport && transports.Count > 1)
			{
				var leg = parent.MostInterestingLegProvider.GetInboundLeg(new TypedEnumerable<IMovementLeg>(transports)) as Transport;
				if (leg != null && leg == transport)
				{
					parent.UpdateRoutingDefaultIfAllowed(parent.JE_RL_NKPortOfArrivalInfo, transport.JW_RL_NKDiscPort);
				}
			}
		}
	}
}

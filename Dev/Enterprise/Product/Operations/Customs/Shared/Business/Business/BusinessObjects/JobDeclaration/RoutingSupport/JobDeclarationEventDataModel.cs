using System.Linq;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class JobDeclarationEventDataModel : BusinessObjectEventDataModel<BaseJobDeclaration>
	{
		public JobDeclarationEventDataModel(BaseJobDeclaration parentBusinessObject)
			: base(parentBusinessObject)
		{
		}

		public string Origin => Parent.JE_RL_NKOrigin;

		public string Destination => Parent.JE_RL_NKFinalDestination;

		public TransportEventDataModel FirstLeg => Transports.Length > 0 ? new TransportEventDataModel(Transports[0]) : null;

		public TransportEventDataModel SecondLeg => Transports.Length > 1 ? new TransportEventDataModel(Transports[1]) : null;

		public TransportEventDataModel ThirdLeg => Transports.Length > 2 ? new TransportEventDataModel(Transports[2]) : null;

		public TransportEventDataModel FourthLeg => Transports.Length > 3 ? new TransportEventDataModel(Transports[3]) : null;

		public TransportEventDataModel LastLeg => Transports.Length > 0 ? new TransportEventDataModel(Transports.Last()) : null;

		Transport[] Transports
		{
			get
			{
				if (transports == null)
				{
					var legs = Parent.Transports.Cast<Transport>().ToArray();
					MovementLegComparer.SortMovementLegsByPorts(legs);
					transports = legs;
				}

				return transports;
			}
		}
		Transport[] transports;
	}
}

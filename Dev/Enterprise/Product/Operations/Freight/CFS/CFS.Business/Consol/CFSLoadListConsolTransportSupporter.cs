using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Security;

namespace Enterprise.Freight.CFS.Business
{
	public sealed class CFSLoadListConsolTransportSupporter : CommonConsolTransportSupporter<CFSLoadListConsol>
	{
		public CFSLoadListConsolTransportSupporter(CFSLoadListConsol parent)
			: base(parent) { }

		protected override void NotifySailingChangedCore(Transport transport, ZGuid previousValue)
		{
			base.NotifySailingChangedCore(transport, previousValue);

			if (transport.PK == Parent.Transports.MostInterestingTransport.PK)
			{
				foreach (CFSContainer container in Parent.Containers)
				{
					container.RefreshAllSailingFields();
				}
			}
		}

		public override SecurityCheckpoint DistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceCFS; }
		}
	}
}

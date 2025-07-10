using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.CFS.Business
{
	public class CFSLoadListConsolJobDatesProvider : JobDatesProvider<CFSLoadListConsol>
	{
		public CFSLoadListConsolJobDatesProvider(CFSLoadListConsol consol)
			: base(consol) { }

		protected override ZDateTime GetArrivalDateCore()
		{
			return Parent.JK_JX_JB_E_ARV;
		}

		protected override ZDateTime GetDepartureDateCore()
		{
			return Parent.JK_JX_JA_E_DEP;
		}

		protected override ZDateTime GetFirstContainerGateInDateCore()
		{
			return GetEarliestValidDate(Parent.Containers.Cast<CFSContainer>().Select(c => c.JC_FCLWharfGateIn));
		}

		protected override ZDateTime GetLastContainerGateInDateCore()
		{
			return GetLatestValidDate(Parent.Containers.Cast<CFSContainer>().Select(c => c.JC_FCLWharfGateIn));
		}

		protected override ZDateTime GetCFSReceivalStartDateCore()
		{
			var result = ZDateTime.Empty;
			var routeSets = ((IRoutingSupport)Parent).TransportsIncludingRelated?.RouteSets;
			if (routeSets.Any())
			{
				result = routeSets.Select(x => x.ReferenceLeg).First().JW_DepotReceivalCommences;
			}

			return result;
		}
	}
}

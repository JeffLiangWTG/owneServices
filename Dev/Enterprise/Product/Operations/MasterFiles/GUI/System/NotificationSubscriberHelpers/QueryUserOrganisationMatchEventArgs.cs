using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.MasterFiles.GUI
{
	public class QueryUserOrganisationMatchEventArgs : QueryUserEventArgs
	{
		public QueryUserOrganisationMatchEventArgs(OrgHeader match)
		{
			this.Match = match;
		}

		public readonly OrgHeader Match;
	}
}

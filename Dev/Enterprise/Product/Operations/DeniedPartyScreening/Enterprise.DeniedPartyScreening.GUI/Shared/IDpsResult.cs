using System.Collections.Generic;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public interface IDpsResult
	{
		bool AllPartiesClear { get; }
		IEnumerable<IScreenedParty> AllScreenedParties { get; }
		string CredentialOverride { get; set; }
	}
}

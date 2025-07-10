using System.Collections.Generic;

namespace Enterprise.MasterFiles.Business
{
	public interface IScreeningPartiesProviderHelper
	{
		ScreeningParty[] GetJobInvoicingScreeningParties(IJobHeaderParent parent, IEnumerable<JobCharge> charges);
	}
}

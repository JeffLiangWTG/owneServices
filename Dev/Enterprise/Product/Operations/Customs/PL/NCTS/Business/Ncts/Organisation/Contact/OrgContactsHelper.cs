using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

static class OrgContactsHelper
{
	internal static bool TryGetContactForMessage(OrgHeader orgHeader, out OrgContact result)
	{
		if (orgHeader == null)
		{
			result = null;
			return false;
		}

		var activeContacts = orgHeader.GetActiveContacts();
		result = activeContacts.Count == 1
			? activeContacts[0]
			: activeContacts.Cast<OrgContact>().FirstOrDefault(x => x.Allocations.Cast<OrgContactAllocation>()
				.Any(allocation => allocation.PC_Type == OrgConstants.ContactAllocationType.CUS));
		return result != null;
	}
}

using System.Collections.Generic;

namespace Enterprise.MasterFiles.Business
{
	public interface IEmailAddressGetterForTrigger
	{
		IEnumerable<string> GetEmailAddressesFromTriggerParty(string triggerParty);
	}
}

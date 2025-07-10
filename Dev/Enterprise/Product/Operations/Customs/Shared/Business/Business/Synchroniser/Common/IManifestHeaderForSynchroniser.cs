using System.Collections.Generic;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Customs.Business
{
	public interface IManifestHeaderForSynchroniser
	{
		IForwardingConsol Consol { get; }
		IEnumerable<IManifestBillForSynchroniser> Bills { get; }
		IManifestBillForSynchroniser AddNewBill();
		bool OverrideFreightDefaults { get; }
		BusinessObjectSynchroniser Synchroniser { get; }
	}
}

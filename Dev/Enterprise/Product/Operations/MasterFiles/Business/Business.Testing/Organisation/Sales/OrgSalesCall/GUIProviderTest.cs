using System;
using static Enterprise.MasterFiles.Business.OrgSalesCallEmailParser;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GUIProviderTest : IGUIProvider
	{
		public OrgContact SelectOrgContact(FilteredContactsCollectionWrapper contacts)
		{
			throw new NotImplementedException();
		}
	}
}

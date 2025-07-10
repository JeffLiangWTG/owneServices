//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobStorageLookups
//
//    This class should be used for overriding collections in AutoJobStorageLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business
{
	public class JobStorageLookups : AutoJobStorageLookups
	{
		public JobStorageLookups(AutoJobStorage parent)
			: base(parent)
		{
		}

		public override OrgHeaderCollection Clients
		{
			get
			{
				if (fClients == null)
				{
					fClients = new OrganisationsFindBoxCollection(Factory);
				}
				return fClients;
			}
		}
		OrgHeaderCollection fClients;
	}
}


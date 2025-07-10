//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobVoyAccountLookups
//
//    This class should be used for overriding collections in AutoJobVoyAccountLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class JobVoyAccountLookups : AutoJobVoyAccountLookups
	{
		public JobVoyAccountLookups(AutoJobVoyAccount parent)
			: base(parent) { }

		public override OrgHeaderCollection Headers
		{
			get { return new ShipsAgencyPrincipalCollectionWithSecurityCheck(Factory); }
		}
	}
}



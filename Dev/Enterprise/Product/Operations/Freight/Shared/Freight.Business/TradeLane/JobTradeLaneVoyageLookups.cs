//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobTradeLaneVoyageLookups
//
//    This class should be used for overriding collections in AutoJobTradeLaneVoyageLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public class JobTradeLaneVoyageLookups : AutoJobTradeLaneVoyageLookups
	{
		public JobTradeLaneVoyageLookups(AutoJobTradeLaneVoyage parent) : base(parent)
		{
		}

		#region PrincipalList

		public override OrgHeaderCollection Headers
		{
			get { return new ShipsAgencyPrincipalCollection(Factory); }
		}

		#endregion

		#region JobTradeLaneList

		public JobTradeLaneCollection JobTradeLaneList
		{
			get { return new JobTradeLaneCollection(Factory); }
		}

		#endregion
	}
}

//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSCPSCRuleAddInfoLookups
//
//    This class should be used for overriding collections in AutoUSCPSCRuleAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class USCPSCRuleAddInfoLookups : AutoUSCPSCRuleAddInfoLookups
	{
		public USCPSCRuleAddInfoLookups(AutoUSCPSCRuleAddInfo parent) : base(parent)
		{
		}

		public OrganisationsFindBoxCollection Organizations
		{
			get { return new OrganisationsFindBoxCollection(Factory); }
		}
	}
}

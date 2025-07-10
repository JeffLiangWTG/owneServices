//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSNHTSADocumentAddInfoLookups
//
//    This class should be used for overriding collections in AutoUSNHTSADocumentAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USNHTSADocumentAddInfoLookups : AutoUSNHTSADocumentAddInfoLookups
	{
		public USNHTSADocumentAddInfoLookups(AutoUSNHTSADocumentAddInfo parent) : base(parent)
		{
		}

		public NHTSADocumentTypeList DocumentTypes
		{
			get { return Factory.GetCachedValue<NHTSADocumentTypeList>(); }
		}

		public NHTSAOrganizationTypeList OrganizationTypes
		{
			get { return Factory.GetCachedValue<NHTSAOrganizationTypeList>(); }
		}
	}
}

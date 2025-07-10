//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNLOrgImpAddInfoLookups
//
//    This class should be used for overriding collections in AutoNLOrgImpAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.Business;

public class NLOrgImpAddInfoLookups : AutoNLOrgImpAddInfoLookups
{
	public NLOrgImpAddInfoLookups(AutoNLOrgImpAddInfo parent) : base(parent)
	{
	}

	public CodeDescriptionPairList VATDefermentList => Factory.GetCachedValue<YesNoList>();
}

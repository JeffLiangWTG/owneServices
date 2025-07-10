//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefHarbourRateLookups
//
//    This class should be used for overriding collections in AutoRefHarbourRateLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Universal;

public class RefHarbourRateLookups : AutoRefHarbourRateLookups
{
	public RefHarbourRateLookups(AutoRefHarbourRate parent) : base(parent)
	{
	}

	public CodeDescriptionPairList ModeList => Factory.GetCachedValue<RefHarbourRateModeList>();

	public IBusinessObjectCollection DataGroupingList => new RefDataGroupingCollection(Factory);
}

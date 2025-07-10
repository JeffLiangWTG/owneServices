//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusReconDeclarationLookups
//
//    This class should be used for overriding collections in AutoCusReconDeclarationLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.Customs.Common.Shared;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business.CusReconBase
{
	public class CusReconDeclarationLookups : AutoCusReconDeclarationLookups
	{
		public CusReconDeclarationLookups(AutoCusReconDeclaration parent) : base(parent)
		{
		}

		public virtual CodeDescriptionPairList MessageStatusList => Factory.GetCachedValue<MessageStatusList>();
	}
}

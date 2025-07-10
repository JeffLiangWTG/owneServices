//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoTWOrgImpAddInfoLookups
//
//    This class should be used for overriding collections in AutoTWOrgImpAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class TWOrgImpAddInfoLookups : AutoTWOrgImpAddInfoLookups
	{
		public TWOrgImpAddInfoLookups(AutoTWOrgImpAddInfo parent) : base(parent)
		{
		}

		public CodeDescriptionPairList ExamModeList => Factory.GetCachedValue<ExamModeList>();

		public CodeDescriptionPairList IMPPaymentMethodList => Factory.GetCachedValue<IMPPaymentMethod>();

		public CodeDescriptionPairList EXPPaymentMethodList => Factory.GetCachedValue<EXPPaymentMethod>();
	}
}

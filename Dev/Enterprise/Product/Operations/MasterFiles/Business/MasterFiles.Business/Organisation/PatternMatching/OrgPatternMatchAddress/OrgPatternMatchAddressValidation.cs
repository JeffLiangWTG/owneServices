//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgPatternMatchAddressValidation
//
//    This class should be used for overriding validation in AutoOrgPatternMatchAddressValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class OrgPatternMatchAddressValidation : AutoOrgPatternMatchAddressValidation
	{
		public OrgPatternMatchAddressValidation(AutoOrgPatternMatchAddress parent) : base(parent)
		{
		}
	}
}

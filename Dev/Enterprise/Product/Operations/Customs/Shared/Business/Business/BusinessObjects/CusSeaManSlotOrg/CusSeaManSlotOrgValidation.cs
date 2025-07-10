//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusSeaManSlotOrgValidation
//
//    This class should be used for overriding validation in AutoCusSeaManSlotOrgValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusSeaManSlotOrgValidation : AutoCusSeaManSlotOrgValidation
	{
		public CusSeaManSlotOrgValidation(AutoCusSeaManSlotOrg parent) : base(parent)
		{
		}
	}
}

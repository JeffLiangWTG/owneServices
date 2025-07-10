//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoMDMAdminPanelAddressViewValidation
//
//    This class should be used for overriding validation in AutoMDMAdminPanelAddressViewValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class MDMAdminPanelAddressViewValidation : AutoMDMAdminPanelAddressViewValidation
	{
		public MDMAdminPanelAddressViewValidation(AutoMDMAdminPanelAddressView parent) : base(parent)
		{
		}
	}
}

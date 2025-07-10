//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoMDMAdminPanelPersonViewValidation
//
//    This class should be used for overriding validation in AutoMDMAdminPanelPersonViewValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterData.Business
{
	public class MDMAdminPanelPersonViewValidation : AutoMDMAdminPanelPersonViewValidation
	{
		public MDMAdminPanelPersonViewValidation(AutoMDMAdminPanelPersonView parent) : base(parent)
		{
		}
	}
}


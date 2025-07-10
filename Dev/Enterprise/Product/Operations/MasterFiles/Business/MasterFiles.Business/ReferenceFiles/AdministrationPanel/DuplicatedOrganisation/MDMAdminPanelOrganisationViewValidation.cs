//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoMDMAdminPanelOrganisationViewValidation
//
//    This class should be used for overriding validation in AutoMDMAdminPanelOrganisationViewValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class MDMAdminPanelOrganisationViewValidation : AutoMDMAdminPanelOrganisationViewValidation
	{
		public MDMAdminPanelOrganisationViewValidation(AutoMDMAdminPanelOrganisationView parent) : base(parent)
		{
		}
	}
}

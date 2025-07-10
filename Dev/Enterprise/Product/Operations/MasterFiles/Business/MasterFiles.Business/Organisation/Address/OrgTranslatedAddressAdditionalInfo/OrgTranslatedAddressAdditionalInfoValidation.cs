//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgTranslatedAddressAdditionalInfoValidation
//
//    This class should be used for overriding validation in AutoOrgTranslatedAddressAdditionalInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class OrgTranslatedAddressAdditionalInfoValidation : AutoOrgTranslatedAddressAdditionalInfoValidation
	{
		public OrgTranslatedAddressAdditionalInfoValidation(AutoOrgTranslatedAddressAdditionalInfo parent) : base(parent)
		{
		}
	}
}

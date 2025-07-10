//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgCarrierAccountMetaDataValidation
//
//    This class should be used for overriding validation in AutoOrgCarrierAccountMetaDataValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class OrgCarrierAccountMetaDataValidation : AutoOrgCarrierAccountMetaDataValidation
	{
		public OrgCarrierAccountMetaDataValidation(AutoOrgCarrierAccountMetaData parent) : base(parent)
		{
		}
	}
}

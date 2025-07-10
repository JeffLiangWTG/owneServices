//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgServiceLevelValidation
//
//    This class should be used for overriding validation in AutoOrgServiceLevelValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class OrgServiceLevelValidation : AutoOrgServiceLevelValidation
	{
		public OrgServiceLevelValidation(AutoOrgServiceLevel parent) : base(parent)
		{
		}
	}
}

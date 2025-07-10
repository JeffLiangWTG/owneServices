//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgOpportunityStageProgressValidation
//
//    This class should be used for overriding validation in AutoOrgOpportunityStageProgressValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class OrgOpportunityStageProgressValidation : AutoOrgOpportunityStageProgressValidation
	{
		public OrgOpportunityStageProgressValidation(AutoOrgOpportunityStageProgress parent) : base(parent)
		{
		}
	}
}

//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgWhsChgAttribGrpByValidation
//
//    This class should be used for overriding validation in AutoOrgWhsChgAttribGrpByValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class OrgWhsChgAttribGrpByValidation : AutoOrgWhsChgAttribGrpByValidation
	{
		public OrgWhsChgAttribGrpByValidation(AutoOrgWhsChgAttribGrpBy parent) : base(parent)
		{
		}
	}
}

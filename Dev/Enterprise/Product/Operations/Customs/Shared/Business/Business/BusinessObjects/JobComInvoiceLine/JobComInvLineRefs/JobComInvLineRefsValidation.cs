//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobComInvLineRefsValidation
//
//    This class should be used for overriding validation in AutoJobComInvLineRefsValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class JobComInvLineRefsValidation : AutoJobComInvLineRefsValidation
	{
		public JobComInvLineRefsValidation(AutoJobComInvLineRefs parent)
			: base(parent)
		{
		}

		protected new JobComInvLineRefs Parent
		{
			get { return (JobComInvLineRefs)base.Parent; }
		}
	}
}

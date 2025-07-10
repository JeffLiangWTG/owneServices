//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobDecRefsValidation
//
//    This class should be used for overriding validation in AutoJobDecRefsValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class JobDecRefsValidation : AutoJobDecRefsValidation
	{
		public JobDecRefsValidation(AutoJobDecRefs parent)
			: base(parent)
		{
		}

		protected new JobDecRefs Parent
		{
			get { return (JobDecRefs)base.Parent; }
		}
	}
}

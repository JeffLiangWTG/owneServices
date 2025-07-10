//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccGLAggregateValidation
//
//    This class should be used for overriding validation in AutoAccGLAggregateValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class AccGLAggregateValidation : AutoAccGLAggregateValidation
	{
		public AccGLAggregateValidation(AutoAccGLAggregate parent) : base(parent)
		{
		}
	}
}

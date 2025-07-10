//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoProcessQueueValidation
//
//    This class should be used for overriding validation in AutoProcessQueueValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class ProcessQueueValidation : AutoProcessQueueValidation
	{
		public ProcessQueueValidation(AutoProcessQueue parent) : base(parent)
		{
		}
	}
}

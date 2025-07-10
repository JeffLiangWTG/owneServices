//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobStorageValidation
//
//    This class should be used for overriding validation in AutoJobStorageValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Rating.Business
{
	public class JobStorageValidation : AutoJobStorageValidation
	{
		public JobStorageValidation(AutoJobStorage parent)
			: base(parent)
		{
		}
	}
}


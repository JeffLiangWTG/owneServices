//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoPatternMatchingResultValidation
//
//    This class should be used for overriding validation in AutoPatternMatchingResultValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class PatternMatchingResultValidation : AutoPatternMatchingResultValidation
	{
		public PatternMatchingResultValidation(AutoPatternMatchingResult parent) : base(parent)
		{
		}
	}
}

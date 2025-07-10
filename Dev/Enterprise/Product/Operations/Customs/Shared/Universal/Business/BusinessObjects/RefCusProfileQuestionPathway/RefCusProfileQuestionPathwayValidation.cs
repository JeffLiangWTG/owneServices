//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusProfileQuestionPathwayValidation
//
//    This class should be used for overriding validation in AutoRefCusProfileQuestionPathwayValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCusProfileQuestionPathwayValidation : AutoRefCusProfileQuestionPathwayValidation
	{
		public RefCusProfileQuestionPathwayValidation(AutoRefCusProfileQuestionPathway parent) : base(parent)
		{
		}
	}
}

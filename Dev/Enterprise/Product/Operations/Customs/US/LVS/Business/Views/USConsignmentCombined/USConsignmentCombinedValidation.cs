//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSConsignmentCombinedValidation
//
//    This class should be used for overriding validation in AutoUSConsignmentCombinedValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.LVS.Business
{
	public class USConsignmentCombinedValidation : AutoUSConsignmentCombinedValidation
	{
		public USConsignmentCombinedValidation(AutoUSConsignmentCombined parent)
			: base(parent)
		{
		}
	}
}

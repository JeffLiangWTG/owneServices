//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoTransitTimeServiceLevelCombinationViewValidation
//
//    This class should be used for overriding validation in AutoTransitTimeServiceLevelCombinationViewValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class TransitTimeServiceLevelCombinationViewValidation : AutoTransitTimeServiceLevelCombinationViewValidation
	{
		public TransitTimeServiceLevelCombinationViewValidation(AutoTransitTimeServiceLevelCombinationView parent) : base(parent)
		{
		}
	}
}


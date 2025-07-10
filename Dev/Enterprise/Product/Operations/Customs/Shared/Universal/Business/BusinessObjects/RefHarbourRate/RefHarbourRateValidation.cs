//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefHarbourRateValidation
//
//    This class should be used for overriding validation in AutoRefHarbourRateValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefHarbourRateValidation : AutoRefHarbourRateValidation
	{
		public RefHarbourRateValidation(AutoRefHarbourRate parent) : base(parent)
		{
		}
	}
}

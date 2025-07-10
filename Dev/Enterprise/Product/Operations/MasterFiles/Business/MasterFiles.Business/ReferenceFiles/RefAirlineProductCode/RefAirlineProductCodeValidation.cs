//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefAirlineProductCodeValidation
//
//    This class should be used for overriding validation in AutoRefAirlineProductCodeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class RefAirlineProductCodeValidation : AutoRefAirlineProductCodeValidation
	{
		public RefAirlineProductCodeValidation(AutoRefAirlineProductCode parent) : base(parent)
		{
		}
	}
}

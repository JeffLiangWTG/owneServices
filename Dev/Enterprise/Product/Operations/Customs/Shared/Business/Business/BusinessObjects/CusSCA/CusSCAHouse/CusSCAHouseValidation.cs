//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusSCAHouseValidation
//
//    This class should be used for overriding validation in AutoCusSCAHouseValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusSCAHouseValidation : AutoCusSCAHouseValidation
	{
		public CusSCAHouseValidation(AutoCusSCAHouse parent) : base(parent)
		{
		}
	}
}

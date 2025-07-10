//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusSeaManArrivalPortValidation
//
//    This class should be used for overriding validation in AutoCusSeaManArrivalPortValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusSeaManArrivalPortValidation : AutoCusSeaManArrivalPortValidation
	{
		public CusSeaManArrivalPortValidation(AutoCusSeaManArrivalPort parent) : base(parent)
		{
		}
	}
}

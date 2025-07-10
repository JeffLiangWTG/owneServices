//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusSeaManOBLHeaderValidation
//
//    This class should be used for overriding validation in AutoCusSeaManOBLHeaderValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusSeaManOBLHeaderValidation : AutoCusSeaManOBLHeaderValidation
	{
		public CusSeaManOBLHeaderValidation(AutoCusSeaManOBLHeader parent) : base(parent)
		{
		}
	}
}

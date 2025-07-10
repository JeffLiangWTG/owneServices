//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusSeaManOBLDetailValidation
//
//    This class should be used for overriding validation in AutoCusSeaManOBLDetailValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusSeaManOBLDetailValidation : AutoCusSeaManOBLDetailValidation
	{
		public CusSeaManOBLDetailValidation(AutoCusSeaManOBLDetail parent) : base(parent)
		{
		}
	}
}

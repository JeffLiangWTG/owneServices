//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSCForeignAndRegionPortValidation
//
//    This class should be used for overriding validation in AutoUSCForeignAndRegionPortValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USCForeignAndRegionPortValidation : AutoUSCForeignAndRegionPortValidation
	{
		public USCForeignAndRegionPortValidation(AutoUSCForeignAndRegionPort parent) : base(parent)
		{
		}
	}
}

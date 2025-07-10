//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusStatementLineGroupValidation
//
//    This class should be used for overriding validation in AutoCusStatementLineGroupValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusStatementLineGroupValidation : AutoCusStatementLineGroupValidation
	{
		public CusStatementLineGroupValidation(AutoCusStatementLineGroup parent) : base(parent)
		{
		}
	}
}

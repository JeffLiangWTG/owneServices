//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsItemReceiveASNValidation
//
//    This class should be used for overriding validation in AutoWhsItemReceiveASNValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemReceiveASNValidation : AutoWhsItemReceiveASNValidation
	{
		public WhsItemReceiveASNValidation(AutoWhsItemReceiveASN parent)
			: base(parent)
		{
		}
	}
}

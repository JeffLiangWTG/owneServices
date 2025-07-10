//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsItemUnloadTaskValidation
//
//    This class should be used for overriding validation in AutoWhsItemUnloadTaskValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemUnloadTaskValidation : AutoWhsItemUnloadTaskValidation
	{
		public WhsItemUnloadTaskValidation(AutoWhsItemUnloadTask parent) : base(parent)
		{
		}
	}
}

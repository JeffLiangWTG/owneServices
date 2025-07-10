//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCYDYardStorageLinesValidation
//
//    This class should be used for overriding validation in AutoCYDYardStorageLinesValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDYardStorageLinesValidation : AutoCYDYardStorageLinesValidation
	{
		public CYDYardStorageLinesValidation(AutoCYDYardStorageLines parent) : base(parent)
		{
		}
	}
}

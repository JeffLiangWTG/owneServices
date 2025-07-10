//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsItemPackageStateValidation
//
//    This class should be used for overriding validation in AutoWhsItemPackageStateValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemPackageStateValidation : AutoWhsItemPackageStateValidation
	{
		public WhsItemPackageStateValidation(AutoWhsItemPackageState parent) : base(parent)
		{
		}
	}
}

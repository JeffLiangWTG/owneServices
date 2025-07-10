//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsPickTrolleyJobValidation
//
//    This class should be used for overriding validation in AutoWhsPickTrolleyJobValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Warehouse.Transactions.TrolleyPicking
{
	public class WhsPickTrolleyJobValidation : AutoWhsPickTrolleyJobValidation
	{
		public WhsPickTrolleyJobValidation(AutoWhsPickTrolleyJob parent) : base(parent)
		{
		}
	}
}

// Add tests to TrolleyPicking.Testing project.

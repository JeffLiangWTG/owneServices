//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusTRPreviousDocumentItemValidation
//
//    This class should be used for overriding validation in AutoCusTRPreviousDocumentItemValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class CusTRPreviousDocumentItemValidation : AutoCusTRPreviousDocumentItemValidation
	{
		public CusTRPreviousDocumentItemValidation(AutoCusTRPreviousDocumentItem parent) : base(parent)
		{
		}
	}
}


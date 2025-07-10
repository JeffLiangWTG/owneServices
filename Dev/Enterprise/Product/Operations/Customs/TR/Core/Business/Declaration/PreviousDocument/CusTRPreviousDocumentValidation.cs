//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusTRPreviousDocumentValidation
//
//    This class should be used for overriding validation in AutoCusTRPreviousDocumentValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class CusTRPreviousDocumentValidation : AutoCusTRPreviousDocumentValidation
	{
		public CusTRPreviousDocumentValidation(AutoCusTRPreviousDocument parent) : base(parent)
		{
		}
	}
}


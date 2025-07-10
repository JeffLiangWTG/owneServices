//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusReconDeclarationValidation
//
//    This class should be used for overriding validation in AutoCusReconDeclarationValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business.CusReconBase
{
	public class CusReconDeclarationValidation : AutoCusReconDeclarationValidation
	{
		public CusReconDeclarationValidation(AutoCusReconDeclaration parent)
			: base(parent)
		{
		}
	}
}

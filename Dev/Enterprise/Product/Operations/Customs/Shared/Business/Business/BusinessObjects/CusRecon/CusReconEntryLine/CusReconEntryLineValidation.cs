//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusReconEntryLineValidation
//
//    This class should be used for overriding validation in AutoCusReconEntryLineValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusReconEntryLineValidation : AutoCusReconEntryLineValidation
	{
		public CusReconEntryLineValidation(AutoCusReconEntryLine parent) : base(parent)
		{
		}
	}
}

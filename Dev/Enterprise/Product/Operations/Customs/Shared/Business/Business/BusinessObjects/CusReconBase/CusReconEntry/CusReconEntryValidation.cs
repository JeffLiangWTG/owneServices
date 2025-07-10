//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusReconEntryValidation
//
//    This class should be used for overriding validation in AutoCusReconEntryValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business.CusReconBase
{
	public class CusReconEntryValidation : AutoCusReconEntryValidation
	{
		public CusReconEntryValidation(AutoCusReconEntry parent) : base(parent)
		{
		}
	}
}

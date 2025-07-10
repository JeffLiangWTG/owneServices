//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUNDGSubstanceADNValidation
//
//    This class should be used for overriding validation in AutoUNDGSubstanceADNValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class UNDGSubstanceADNValidation : AutoUNDGSubstanceADNValidation
	{
		public UNDGSubstanceADNValidation(AutoUNDGSubstanceADN parent) : base(parent)
		{
		}
	}
}

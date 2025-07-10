//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCarrierVoyageTransactionValidation
//
//    This class should be used for overriding validation in AutoReeferSettingValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public sealed class ReeferSettingValidation : AutoReeferSettingValidation
	{
		public ReeferSettingValidation(AutoReeferSetting parent) : base(parent)
		{
		}
	}
}

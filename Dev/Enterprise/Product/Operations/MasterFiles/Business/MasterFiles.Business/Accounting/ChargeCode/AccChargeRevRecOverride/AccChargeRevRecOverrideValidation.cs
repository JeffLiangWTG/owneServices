//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccChargeRevRecOverrideValidation
//
//    This class should be used for overriding validation in AutoAccChargeRevRecOverrideValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccChargeRevRecOverrideValidation : AutoAccChargeRevRecOverrideValidation
	{
		public AccChargeRevRecOverrideValidation(AutoAccChargeRevRecOverride parent) : base(parent)
		{
			RevRecValidation = new RevenueRecognitionValidation((IRevenueRecognition)parent);
		}

		readonly RevenueRecognitionValidation RevRecValidation;

		protected override void CheckAE_JobType()
		{
			base.CheckAE_JobType();
			RevRecValidation.ValidateJobType();
		}

		protected override void CheckAE_Mode()
		{
			base.CheckAE_Mode();
			RevRecValidation.ValidateMode();
		}

		protected override void CheckAE_Direction()
		{
			base.CheckAE_Direction();
			RevRecValidation.ValidateDirectionCode();
		}

		protected override void CheckAE_RecognitionType()
		{
			base.CheckAE_RecognitionType();
			RevRecValidation.ValidateRecognitionDateOptionCode();
		}

		protected override void CheckAE_BrokerType()
		{
			base.CheckAE_BrokerType();
			RevRecValidation.ValidateBrokerCode();
		}

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
		{
			if (info.Name == AccChargeRevRecOverrideSchema.AE_AC.Name)
			{
				return false;
			}

			return base.ShouldValidateFKToCancelledRecord(info);
		}
	}
}

//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccChargeSupplyTypeOverrideValidation
//
//    This class should be used for overriding validation in AutoAccChargeSupplyTypeOverrideValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class AccChargeSupplyTypeOverrideValidation : AutoAccChargeSupplyTypeOverrideValidation
	{
		public AccChargeSupplyTypeOverrideValidation(AutoAccChargeSupplyTypeOverride parent) : base(parent)
		{
			SupplyTypeOverrideValidation = new SupplyTypeConfigurationValidation((ISupplyTypeSelector)parent);
		}

		readonly SupplyTypeConfigurationValidation SupplyTypeOverrideValidation;

		protected override void CheckACS_JobType()
		{
			base.CheckACS_JobType();
			SupplyTypeOverrideValidation.ValidateJobType();
		}

		protected override void CheckACS_TransportMode()
		{
			base.CheckACS_TransportMode();
			SupplyTypeOverrideValidation.ValidateMode();
		}

		protected override void CheckACS_Direction()
		{
			base.CheckACS_Direction();
			SupplyTypeOverrideValidation.ValidateDirectionCode();
		}

		protected override void CheckACS_IncoTerm()
		{
			base.CheckACS_IncoTerm();
			SupplyTypeOverrideValidation.ValidateIncoterm();
		}

		protected override void CheckACS_SupplyType()
		{
			base.CheckACS_SupplyType();
			SupplyTypeOverrideValidation.ValidateSupplyType();
		}

		protected override void CheckACS_GE()
		{
			base.CheckACS_GE();
			SupplyTypeOverrideValidation.ValidateLineDepartmentPK();
		}
	}
}

using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccWithholdingValidation : AutoAccWithholdingValidation
	{
		public AccWithholdingValidation(AutoAccWithholding parent) : base(parent)
		{
		}

		protected override void CheckAW_Code()
		{
			base.CheckAW_Code();
			MandatoryValidation.CheckEntered(Parent.AW_CodeInfo);
		}

		protected override void CheckAW_Description()
		{
			base.CheckAW_Description();
			MandatoryValidation.CheckEntered(Parent.AW_DescriptionInfo);
		}

		protected override void CheckAW_Rate()
		{
			base.CheckAW_Rate();
			if (!Parent.AW_RateInfo.HasErrors())
			{
				if (Parent.AW_Rate < 0)
				{
					Parent.AW_RateInfo.AddError(Res.GetString("e7ae4efa-8743-4fba-9621-e5e57c15c3b3", "Rate cannot be less than 0"));
				}
			}
		}
	}
}

using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class UNDGSubstanceValidation : AutoUNDGSubstanceValidation
	{
		public UNDGSubstanceValidation(AutoUNDGSubstance parent) : base(parent)
		{
		}

		protected override void CheckDG_UNNO()
		{
			base.CheckDG_UNNO();
			MandatoryValidation.CheckEntered(Parent.DG_UNNOInfo);
		}

		protected override void CheckDG_PSN()
		{
			base.CheckDG_PSN();
			MandatoryValidation.CheckEntered(Parent.DG_PSNInfo);
		}

		protected override void CheckDG_Class()
		{
			base.CheckDG_Class();
			MandatoryValidation.CheckEntered(Parent.DG_ClassInfo);
		}
	}
}

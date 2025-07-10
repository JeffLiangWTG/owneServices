using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class CusEngineValidation : Customs.Business.CusEngineValidation
	{
		public CusEngineValidation(AutoCusEngine parent) : base(parent)
		{
		}

		protected override void CheckCEG_EngineType()
		{
			base.CheckCEG_EngineType();
			ListValidation.MessageErrorIfInvalidCode(Parent.CEG_EngineTypeInfo);
		}

		protected override void CheckCEG_CapacityCC()
		{
			base.CheckCEG_CapacityCC();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CEG_CapacityCCInfo);
			MandatoryValidation.MessageErrorIfIsNegative(Parent.CEG_CapacityCCInfo);
		}

		protected override void CheckCEG_Cylinders()
		{
			base.CheckCEG_Cylinders();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CEG_CylindersInfo);
			MandatoryValidation.MessageErrorIfIsNegative(Parent.CEG_CylindersInfo);
		}

		protected override void CheckCEG_CapacityHP()
		{
			base.CheckCEG_CapacityHP();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CEG_CapacityHPInfo);
			MandatoryValidation.MessageErrorIfIsNegative(Parent.CEG_CapacityHPInfo);
		}
	}
}

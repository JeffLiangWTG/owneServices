using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.V4.Business
{
	public class CMDPermitNumberValidation : Customs.Business.CusCodeDataValidation
	{
		public CMDPermitNumberValidation(CMDPermitNumber parent)
			: base(parent)
		{
		}

		protected override void CheckCY_Data()
		{
			MandatoryValidation.CheckEntered(Parent.CY_DataInfo, "CMD Permit Number");
		}

		protected override void CheckCY_Code()
		{
		}
	}
}

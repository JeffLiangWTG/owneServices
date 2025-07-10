using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.V4.Business
{
	public class CALicenceNumberValidation : Customs.Business.CusCodeDataValidation
	{
		public CALicenceNumberValidation(CALicenceNumber parent)
			: base(parent)
		{
		}

		protected override void CheckCY_Data()
		{
			MandatoryValidation.CheckEntered(Parent.CY_DataInfo, "CA Licence Number");
		}

		protected override void CheckCY_Code()
		{
		}
	}
}

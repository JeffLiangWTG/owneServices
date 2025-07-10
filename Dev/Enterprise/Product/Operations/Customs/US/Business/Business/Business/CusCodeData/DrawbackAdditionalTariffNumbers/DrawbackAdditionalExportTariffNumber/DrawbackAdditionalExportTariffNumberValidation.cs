using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class DrawbackAdditionalExportTariffNumberValidation : Customs.Business.CusCodeDataValidation
	{
		public DrawbackAdditionalExportTariffNumberValidation(DrawbackAdditionalExportTariffNumber bizObj)
			: base(bizObj)
		{
		}

		new DrawbackAdditionalExportTariffNumber Parent
		{
			get { return (DrawbackAdditionalExportTariffNumber)base.Parent; }
		}

		protected override void CheckCY_Code()
		{
			//do not want to validate
		}

		protected override void CheckCY_Data()
		{
			base.CheckCY_Data();
			ListValidation.MessageErrorIfInvalidCode(Parent.CY_DataInfo, Parent.ExportTariffs);
		}
	}
}

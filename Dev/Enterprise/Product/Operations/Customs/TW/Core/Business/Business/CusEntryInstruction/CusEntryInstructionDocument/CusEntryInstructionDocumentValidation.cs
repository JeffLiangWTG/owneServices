using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business
{
	public class CusEntryInstructionDocumentValidation : Customs.Business.CusCodeDataValidation
	{
		public CusEntryInstructionDocumentValidation(CusEntryInstructionDocument parent) : base(parent)
		{
		}

		protected override void CheckCY_Code()
		{
		}

		protected override void CheckCY_Data()
		{
			base.CheckCY_Data();
			MandatoryValidation.CheckEntered(Parent.CY_DataInfo);
		}
	}
}

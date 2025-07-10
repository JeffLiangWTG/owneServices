using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.eManifest.Business
{
	public class CusCodeDataValidation : Customs.Business.CusCodeDataValidation
	{
		public CusCodeDataValidation(AutoCusCodeData parent)
			: base(parent)
		{
		}

		protected override void CheckCY_Data()
		{
			base.CheckCY_Data();
			if (Parent.CY_Type == CusCodeDataTypeList.Codes.SealNumber && Parent.CY_Data.IsEmpty)
			{
				Parent.CY_DataInfo.AddMessageError("If you create a seal, the value must not be blank. Please delete the empty row or supply a seal number. Note that a seal is not required.");
			}
			else
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CY_DataInfo);
			}
		}
	}
}

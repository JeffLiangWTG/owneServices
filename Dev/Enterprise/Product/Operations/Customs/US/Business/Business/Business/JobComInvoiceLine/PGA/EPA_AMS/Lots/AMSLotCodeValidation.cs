using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class AMSLotCodeValidation : Customs.Business.CusCodeDataValidation
	{
		public AMSLotCodeValidation(AMSLotCode parent)
			: base(parent)
		{
		}

		protected override void CheckCY_Code()
		{
			var code = (AMSLotCode)Parent;
			ListValidation.MessageErrorIfInvalidCode(code.CY_CodeInfo, code.ALCList);

			if (!code.CY_Data.IsEmpty && code.Parent is AMS amsHeader && amsHeader.IsOR2Program)
			{
				MandatoryValidation.MessageErrorIfNotEntered(code.CY_CodeInfo);
			}
		}

		protected override void CheckCY_Data()
		{
			base.CheckCY_Data();
			if (!Parent.CY_Code.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CY_DataInfo);
			}
		}
	}
}

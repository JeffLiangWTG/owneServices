using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business
{
	public class CusCustomsOfficeValidation
	{
		public CusCustomsOfficeValidation(CusCustomsOffice parent)
		{
			this.parent = parent;
		}

		readonly CusCustomsOffice parent;

		public void ValidateAll()
		{
			ValidateCustomsOfficeCode();
		}

		public void ValidateCustomsOfficeCode()
		{
			var targetInfo = parent.CustomsOfficeCodeInfo;
			targetInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(targetInfo);
			ListValidation.ErrorIfInvalidCode(targetInfo);
		}
	}
}

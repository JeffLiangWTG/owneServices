using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class USAMSMO3AddInfoValidation : USAMSLineAddInfoValidation
	{
		public USAMSMO3AddInfoValidation(AutoUSAMSLineAddInfo parent)
			: base(parent)
		{
		}

		protected override void CheckUS_AuthorizationNumber()
		{
			base.CheckUS_AuthorizationNumber();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.US_AuthorizationNumberInfo);
			if (Parent.US_AuthorizationNumber.Length != 12)
			{
				Parent.US_AuthorizationNumberInfo.AddMessageError(AuthorizationNumberInValid);
			}
		}
		internal const string AuthorizationNumberInValid = "Please enter 12 characters as Exemption Authorization Number.";
	}
}

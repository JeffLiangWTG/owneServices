using CargoWise.Common;

namespace Enterprise.Rating.Business
{
	public class CargoGuideCredentialsValidation
	{
		public CargoGuideCredentialsValidation(CargoGuideCredentials parent) : base()
		{
			Parent = Argument.NotNull(parent, nameof(parent));
		}
		readonly CargoGuideCredentials Parent;

		public void ValidateCargoGuideCredentialsPassword()
		{
			Parent.PasswordInfo.ClearAllNotifications();
			if (!Parent.Password.IsPrintableASCIIOrEmpty)
			{
				Parent.PasswordInfo.AddError(Res.GetString("308F5820-CAB6-4F3E-A0FF-017983CD0BE4", "The password contains non-ASCII characters. Please check that you entered the intended text correctly. If you pasted it into the text box, please try typing it instead."));
			}
		}

		public void ValidateCargoGuideCredentialsLogin()
		{
			Parent.LoginInfo.ClearAllNotifications();
			if (!Parent.Login.IsPrintableASCIIOrEmpty)
			{
				Parent.LoginInfo.AddError(Res.GetString("F38A0CE9-5BD1-4C5C-AAB9-ECCA26A5394F", "The login contains non-ASCII characters. Please check that you entered the intended text correctly. If you pasted it into the text box, please try typing it instead."));
			}
		}
	}
}

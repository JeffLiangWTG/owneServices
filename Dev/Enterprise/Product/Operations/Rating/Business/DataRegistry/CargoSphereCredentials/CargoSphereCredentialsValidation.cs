using CargoWise.Common;

namespace Enterprise.Rating.Business
{
	public class CargoSphereCredentialsValidation
	{
		public CargoSphereCredentialsValidation(CargoSphereCredentials parent) : base()
		{
			Parent = Argument.NotNull(parent, nameof(parent));
		}
		readonly CargoSphereCredentials Parent;

		public void ValidateCargoSphereCredentialsPassword()
		{
			Parent.PasswordInfo.ClearAllNotifications();
			if (!Parent.Password.IsPrintableASCIIOrEmpty)
			{
				Parent.PasswordInfo.AddError(Res.GetString("308F5820-CAB6-4F3E-A0FF-017983CD0BE4", "The password contains non-ASCII characters. Please check that you entered the intended text correctly. If you pasted it into the text box, please try typing it instead."));
			}
		}

		public void ValidateCargoSphereCredentialsLogin()
		{
			Parent.LoginInfo.ClearAllNotifications();
			if (!Parent.Login.IsPrintableASCIIOrEmpty)
			{
				Parent.LoginInfo.AddError(Res.GetString("F38A0CE9-5BD1-4C5C-AAB9-ECCA26A5394F", "The login contains non-ASCII characters. Please check that you entered the intended text correctly. If you pasted it into the text box, please try typing it instead."));
			}
		}

		public void ValidateCargoSphereCredentialsSystemCode()
		{
			Parent.SystemCodeInfo.ClearAllNotifications();
			if (!Parent.SystemCode.IsPrintableASCIIOrEmpty)
			{
				Parent.SystemCodeInfo.AddError(Res.GetString("86E8660D-5DFA-4907-939C-E3818DADB49A", "The system code contains non-ASCII characters. Please check that you entered the intended text correctly. If you pasted it into the text box, please try typing it instead."));
			}
		}
	}
}

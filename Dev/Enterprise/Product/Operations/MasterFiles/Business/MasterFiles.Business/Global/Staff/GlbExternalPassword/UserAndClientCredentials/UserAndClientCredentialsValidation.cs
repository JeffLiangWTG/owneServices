using System;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class UserAndClientCredentialsValidation : ZValidation
	{
		public UserAndClientCredentialsValidation(UserAndClientCredentials parent)
			: base(parent)
		{
			Parent = Argument.NotNull(parent, nameof(parent));
			Parent.UsernameInfo.AdditionalValidation += CheckUsername_AdditionalValidation;
		}

		UserAndClientCredentials Parent { get; }

		public override Type AutoValidationType => typeof(UserAndClientCredentialsValidation);

		#region Public Validate Methods

		public override void ValidateAll()
		{
			ValidateUserCredential();
			ValidateClientCredential();
		}

		public void ValidateUserCredential()
		{
			ValidateUsername();
			ValidatePassword();
			ValidatePasswordConfirmation();
			ValidateUserCredentialPasswordStatusReason();
		}

		public void ValidateClientCredential()
		{
			ValidateClientId();
			ValidateClientSecret();
			ValidateClientCredentialPasswordStatusReason();
		}

		public void ValidateUsername() => ValidateCalculatedProperty(Parent.UsernameInfo);

		public void ValidatePassword() => ValidateCalculatedProperty(Parent.PasswordInfo);

		public void ValidatePasswordConfirmation() => ValidateCalculatedProperty(Parent.PasswordConfirmationInfo);

		public void ValidateUserCredentialPasswordStatusReason() => ValidateCalculatedProperty(Parent.UserCredentialPasswordStatusReasonInfo);

		public void ValidateClientId() => ValidateCalculatedProperty(Parent.ClientIdInfo);

		public void ValidateClientSecret() => ValidateCalculatedProperty(Parent.ClientSecretInfo);

		public void ValidateClientCredentialPasswordStatusReason() => ValidateCalculatedProperty(Parent.ClientCredentialPasswordStatusReasonInfo);

		#endregion

		#region Protected Implementation

		protected void CheckUsername()
		{
			Parent.UserCredential.Validation.ValidateGP_UserID();
		}

		void CheckUsername_AdditionalValidation()
		{
			if (!Parent.ClientId.IsEmpty && Parent.Username.IsEmpty)
			{
				Parent.UsernameInfo.AddError(Res.GetString("a481d9c1-8a56-4753-b2d2-d322b5dfa383", "{0} is required when {1} is entered", Parent.UsernameInfo.HumanReadableName, Parent.ClientIdInfo.HumanReadableName));
			}
		}

		protected void CheckPassword()
		{
			Parent.UserCredential.Validation.ValidateCurrentDecryptedPassword();
		}

		protected void CheckPasswordConfirmation()
		{
			if (Parent.Password != Parent.PasswordConfirmation && !Parent.Password.IsEmpty)
			{
				Parent.PasswordConfirmationInfo.AddError(Res.GetString("c4dbeee8-ce08-459f-b251-1dc9aa845c0e", "Password and confirmation password do not match"));
			}
		}

		protected void CheckUserCredentialPasswordStatusReason()
		{
			Parent.UserCredential.Validation.ValidateGP_StatusReason();
		}

		protected void CheckClientId()
		{
			Parent.ClientCredential.Validation.ValidateGP_UserID();
		}

		protected void CheckClientSecret()
		{
			Parent.ClientCredential.Validation.ValidateCurrentDecryptedPassword();
		}

		protected void CheckClientCredentialPasswordStatusReason()
		{
			Parent.ClientCredential.Validation.ValidateGP_StatusReason();
		}

		#endregion
	}
}

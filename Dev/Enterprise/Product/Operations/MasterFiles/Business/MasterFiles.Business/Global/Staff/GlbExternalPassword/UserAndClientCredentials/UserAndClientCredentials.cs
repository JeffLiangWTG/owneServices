using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public abstract class UserAndClientCredentials : NonPersistentBusinessObject, IObsoleteValidation
	{
		public UserAndClientCredentials(GlbExternalPassword userCredential, GlbExternalPassword clientCredential)
			: base(userCredential?.Factory)
		{
			UserCredential = Argument.NotNull(userCredential, nameof(userCredential));
			ClientCredential = Argument.NotNull(clientCredential, nameof(clientCredential));
		}

		#region Properties

		#region User Credential

		internal GlbExternalPassword UserCredential { get; }

		#region Username

		[ResourceStringData("432a01b9-d43b-442c-b402-a12d8c003222", Caption = "User Id")]
		public ZString Username
		{
			get => UserCredential.GP_UserID;
			set
			{
				UserCredential.GP_UserID = value;
				UserCredential.GP_PasswordStatus = ZString.Empty;
				HasChanges = true;
				UsernameInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateUsername();
					Validation.ValidatePassword();
				}
			}
		}

		public ZPropertyInfo UsernameInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get => GetWrappedZPropertyInfo(nameof(Username), (sender) => UserCredential.GP_UserIDInfo);
		}

		#endregion

		#region Password

		[ResourceStringData("f98dd6d6-e4b5-45f2-a988-6e2dc5466aa0", Caption = "Password")]
		[Password]
		[MaxLength(GlbExternalPassword.Schema.CurrentDecryptedPasswordMaxLength)]
		public ZString Password
		{
			get => UserCredential.CurrentDecryptedPassword;
			set
			{
				UserCredential.CurrentDecryptedPassword = value;
				HasChanges = true;
				PasswordInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateUsername();
					Validation.ValidatePasswordConfirmation();
				}
			}
		}

		public ZPropertyInfo PasswordInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get => GetWrappedZPropertyInfo(nameof(Password), (sender) => UserCredential.CurrentDecryptedPasswordInfo);
		}

		#endregion

		#region PasswordConfirmation

		ZString fPasswordConfirmation;

		[ResourceStringData("4757ac6d-ce28-4119-9185-99a62667a222", Caption = "Confirm Password", ShortCaption = "Confirm")]
		[Password]
		[MaxLength(GlbExternalPassword.Schema.CurrentDecryptedPasswordMaxLength)]
		public ZString PasswordConfirmation
		{
			get => fPasswordConfirmation;
			set
			{
				CheckMaximumLength(PasswordConfirmationInfo, value);
				fPasswordConfirmation = value;
				HasChanges = true;
				PasswordConfirmationInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidatePassword();
					Validation.ValidatePasswordConfirmation();
				}
			}
		}

		public ZPropertyInfo PasswordConfirmationInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get => GetZPropertyInfo(nameof(PasswordConfirmation));
		}

		#endregion

		#region UserCredentialPasswordStatus

		public ZString UserCredentialPasswordStatus
		{
			get => PasswordStatusLookup.GetDescriptionFromCode(UserCredential.GP_PasswordStatus) ?? ZString.Empty;
		}

		public ZPropertyInfo UserCredentialPasswordStatusInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get => GetZPropertyInfo(nameof(UserCredentialPasswordStatus));
		}

		#endregion

		#region UserCredentialPasswordStatusReason

		[ResourceStringData("bbabd2fc-10fa-439b-8994-74104180a4df", Caption = "Password Error", ShortCaption = "Error")]
		public ZString UserCredentialPasswordStatusReason
		{
			get => UserCredential.GP_StatusReason;
		}

		public ZPropertyInfo UserCredentialPasswordStatusReasonInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get => GetWrappedZPropertyInfo(nameof(UserCredentialPasswordStatusReason), (sender) => UserCredential.GP_StatusReasonInfo);
		}

		#endregion

		#endregion

		#region Client Credential

		internal GlbExternalPassword ClientCredential { get; }

		#region ClientId

		[ResourceStringData("bbffeb28-2de4-4b62-a2ef-383ee908d556", Caption = "Client Id")]
		public virtual ZString ClientId
		{
			get => ClientCredential.GP_UserID;
			set
			{
				ClientCredential.GP_UserID = value;
				ClientCredential.GP_PasswordStatus = ZString.Empty;
				HasChanges = true;
				ClientIdInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateClientId();
					Validation.ValidateClientSecret();
					Validation.ValidateUsername();
				}
			}
		}

		public ZPropertyInfo ClientIdInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get => GetWrappedZPropertyInfo(nameof(ClientId), (sender) => ClientCredential.GP_UserIDInfo);
		}

		#endregion

		#region ClientSecret

		[ResourceStringData("743de767-43e3-4994-ac0e-bfcbe2644df2", Caption = "Client Secret")]
		public virtual ZString ClientSecret
		{
			get => ClientCredential.CurrentDecryptedPassword;
			set
			{
				ClientCredential.CurrentDecryptedPassword = value;
				HasChanges = true;
				ClientSecretInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateClientSecret();
					Validation.ValidateClientId();
				}
			}
		}

		public ZPropertyInfo ClientSecretInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get => GetWrappedZPropertyInfo(nameof(ClientSecret), (sender) => ClientCredential.CurrentDecryptedPasswordInfo);
		}

		#endregion

		#region ClientCredentialPasswordStatus

		public ZString ClientCredentialPasswordStatus
		{
			get => PasswordStatusLookup.GetDescriptionFromCode(ClientCredential.GP_PasswordStatus) ?? ZString.Empty;
		}

		public ZPropertyInfo ClientCredentialPasswordStatusInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get => GetZPropertyInfo(nameof(ClientCredentialPasswordStatus));
		}

		#endregion

		#region ClientCredentialPasswordStatusReason

		[ResourceStringData("c5534d6e-7abe-494b-ac87-ca8350d98c41", Caption = "Client Secret Error", ShortCaption = "Error")]
		public ZString ClientCredentialPasswordStatusReason
		{
			get => ClientCredential.GP_StatusReason;
		}

		public ZPropertyInfo ClientCredentialPasswordStatusReasonInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get => GetWrappedZPropertyInfo(nameof(ClientCredentialPasswordStatusReason), (sender) => ClientCredential.GP_StatusReasonInfo);
		}

		#endregion

		#endregion

		#endregion

		#region Overrides

		protected override void OnFactorySaving()
		{
			UpdatePasswordStatusesOnSaving();
		}

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public UserAndClientCredentialsValidation Validation
		{
			get { return GetNewValidation(); }
		}

		protected virtual UserAndClientCredentialsValidation GetNewValidation()
		{
			return new UserAndClientCredentialsValidation(this);
		}

		#endregion

		#endregion

		#region Lookups

		public CodeDescriptionPairList PasswordStatusLookup
		{
			get
			{
				return Factory.GetCachedValue("UserAndClientCredentials.PasswordStatusLookup", () =>
				{
					var list = new CodeDescriptionPairList();
					list.AddPair(PasswordStatusList.Codes.PasswordOK, Res.GetString("ad75c12f-8c2d-4237-919b-05e279967d0d", "Saved"));
					list.AddPair(PasswordStatusList.Codes.Invalid, Res.GetString("eec8b2f1-6d3d-4c93-894f-fa60d9128ff6", "Error"));
					return list;
				});
			}
		}

		#endregion

		#region Implementation

		void UpdatePasswordStatusesOnSaving()
		{
			using (UserCredential.GetValidationSuspender())
			using (ClientCredential.GetValidationSuspender())
			{
				UserCredential.GP_PasswordStatus = Username.IsEmpty ? string.Empty : PasswordStatusList.Codes.PasswordOK;
				UserCredential.GP_StatusReason = ZString.Empty;
				ClientCredential.GP_PasswordStatus = ClientId.IsEmpty ? string.Empty : PasswordStatusList.Codes.PasswordOK;
				ClientCredential.GP_StatusReason = ZString.Empty;
			}

			UserCredential.Validation.ValidateGP_StatusReason();
			ClientCredential.Validation.ValidateGP_StatusReason();
		}

		#endregion
	}
}

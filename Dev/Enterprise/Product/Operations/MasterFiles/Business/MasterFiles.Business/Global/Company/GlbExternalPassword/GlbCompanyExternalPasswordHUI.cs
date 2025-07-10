using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// External Credentials for Hungary EInvoicing.
	/// </summary>
	public sealed class GlbCompanyExternalPasswordHUI : GlbExternalPassword
	{
		public GlbCompanyExternalPasswordHUI(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			DisableConfigurationToSender = true;
		}

		public static bool IsAllowed(GlbCompany company) => company.GC_RN_NKCountryCode == CountryCodes.Hungary;

		#region Load

		public static GlbCompanyExternalPasswordHUI LoadForCompany(BusinessObjectFactory factory, GlbCompany company)
		{
			var query = new ZQuery(GlbExternalPasswordSchema.GP_PasswordType, PasswordTypesList.Codes.HUI);
			query.AddToFilter(GlbExternalPasswordSchema.GP_GC, company.PK);

			var credentials = factory.Load<GlbCompanyExternalPasswordHUI>(query);
			if (credentials.Length > 1)
			{
				ErrorReporter.ReportOnce("GlbCompanyExternalPasswordHUI.LoadForCompany.NotUnique", FormattableString.Invariant($"When loading GlbExternalPassword for 'HUI' + company '{company.GC_Code}' {credentials.Length:N0} bizos were loaded. There should only be one HUI credential for a company; most recently modified credential will be returned."));
			}

			var result = credentials.Cast<GlbCompanyExternalPasswordHUI>().OrderByDescending(x => x.GP_SystemLastEditTimeUtc).FirstOrDefault();
			return result;
		}

		public static GlbCompanyExternalPasswordHUI LoadForCompanyOrNew(BusinessObjectFactory factory, GlbCompany company)
		{
			var loadedCredential = LoadForCompany(factory, company);
			if (loadedCredential != null)
			{
				return loadedCredential;
			}

			var newCredential = factory.New<GlbCompanyExternalPasswordHUI>();
			newCredential.GP_GC = company.PK;
			return newCredential;
		}

		#endregion

		#region Properties

		public new class Schema : GlbExternalPassword.Schema
		{
			public const int PasswordHashMaxLength = 128;
			public const int SignatureKeyMaxLength = 40;
			public const int ReplacementKeyMaxLength = 40;
		}

		#region Login

		[ResourceStringData("b92b39d0-ba3c-4c6b-9dac-a72807ce8bd9", Caption = "Login")]
		[MaxLength(Schema.GP_UserIDMaxLength)]
		public ZString Login
		{
			get => base.GP_UserID;
			set
			{
				var oldValue = Login;
				if (oldValue != value)
				{
					base.GP_UserID = value;
					LoginInfo.RefreshBinding(oldValue);
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateHungaryCredentials();
				}
			}
		}

		public ZPropertyInfo LoginInfo
		{
			[System.Diagnostics.DebuggerStepThrough]
			get => GetZPropertyInfo(nameof(Login));
		}

		#endregion

		#region PasswordHash

		[ResourceStringData("1d201ebc-0149-42fc-bdb0-08994801e55c", Caption = "Password Hash")]
		[MaxLength(Schema.PasswordHashMaxLength)]
		public ZString PasswordHash
		{
			get => PasswordHashWasNotEncrypted ? base.GP_CurrentPassword : DecodeValueForDisplay(base.GP_CurrentPassword);
			set
			{
				var oldValue = PasswordHash;
				if (oldValue != value)
				{
					CheckMaximumLength(PasswordHashInfo, value);
					var (encodedValue, successful) = TryEncodeValueForStorage(value);
					PasswordHashWasNotEncrypted = !successful;
					base.GP_CurrentPassword = successful ? encodedValue : value;
					PasswordHashInfo.RefreshBinding(oldValue);
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateHungaryCredentials();
				}
			}
		}
		bool PasswordHashWasNotEncrypted;

		public ZPropertyInfo PasswordHashInfo
		{
			[System.Diagnostics.DebuggerStepThrough]
			get => GetZPropertyInfo(nameof(PasswordHash));
		}

		#endregion

		#region SignatureKey

		[ResourceStringData("30aa8391-985e-4dfc-827c-8f16a653ece9", Caption = "Signature Key")]
		[MaxLength(Schema.SignatureKeyMaxLength)]
		public ZString SignatureKey
		{
			get => base.GP_CertificatePassPhrase.IsEmpty ? ZString.Empty : (ZString)Encoder.Decrypt(GP_CertificatePassPhrase);
			set
			{
				var oldValue = GP_CertificatePassPhrase;
				if (oldValue != value)
				{
					CheckMaximumLength(SignatureKeyInfo, value);
					base.GP_CertificatePassPhrase = value.IsEmpty ? ZString.Empty : (ZString)Encoder.Encrypt(value);
					SignatureKeyInfo.RefreshBinding(oldValue);
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateHungaryCredentials();
				}
			}
		}

		public ZPropertyInfo SignatureKeyInfo
		{
			[System.Diagnostics.DebuggerStepThrough]
			get => GetZPropertyInfo(nameof(SignatureKey));
		}

		#endregion

		#region ReplacementKey

		[ResourceStringData("cf0b0e10-549c-4b6d-bfda-baadd2cf4de9", Caption = "Replacement Key")]
		[MaxLength(Schema.ReplacementKeyMaxLength)]
		public ZString ReplacementKey
		{
			get => base.GP_NextPassword.IsEmpty ? ZString.Empty : (ZString)Encoder.Decrypt(GP_NextPassword);
			set
			{
				var oldValue = ReplacementKey;
				if (oldValue != value)
				{
					CheckMaximumLength(ReplacementKeyInfo, value);
					base.GP_NextPassword = value.IsEmpty ? ZString.Empty : (ZString)Encoder.Encrypt(value);
					ReplacementKeyInfo.RefreshBinding(oldValue);
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateHungaryCredentials();
				}
			}
		}

		public ZPropertyInfo ReplacementKeyInfo
		{
			[System.Diagnostics.DebuggerStepThrough]
			get => GetZPropertyInfo(nameof(ReplacementKey));
		}

		#endregion

		#endregion

		#region Overrides

		protected override ZString HumanReadableNameCore => Res.GetString("8579c96d-4614-4d96-8c1e-0904485ac44e", "Hungary E-Invoicing Credential");

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			GP_PasswordType = PasswordTypesList.Codes.HUI;
			GP_GC = GlbCompany.CurrentCompany.PK;
		}

		public override void OnSaving()
		{
			base.OnSaving();

			if (!Login.IsEmpty
				&& !PasswordHash.IsEmpty
				&& !SignatureKey.IsEmpty
				&& !ReplacementKey.IsEmpty)
			{
				GP_PasswordStatus = PasswordStatusList.Codes.PasswordOK;
			}
			else
			{
				GP_PasswordStatus = ZString.Empty;
			}
		}

		public new GlbCompanyExternalPasswordHUIValidation Validation => (GlbCompanyExternalPasswordHUIValidation)base.Validation;

		protected override GlbExternalPasswordValidation GetNewValidation()
		{
			return new GlbCompanyExternalPasswordHUIValidation(this);
		}

		#endregion

		#region Implementation

		ZString DecodeValueForDisplay(ZString encodedValue)
		{
			if (encodedValue.IsEmpty)
			{
				return ZString.Empty;
			}

			var encodedBytes = Convert.FromBase64String(encodedValue);
			var plainBytes = Encoder.Decrypt(encodedBytes);
			var plaintextValue = BytesToHexString(plainBytes);
			return plaintextValue;
		}

		(ZString encodedValue, bool successful) TryEncodeValueForStorage(ZString plaintextValue)
		{
			if (plaintextValue.IsEmpty)
			{
				return (ZString.Empty, true);
			}
			if (!GlbCompanyExternalPasswordHUIValidation.IsHexStringOfBytes(plaintextValue))
			{
				return (ZString.Empty, false);
			}

			var plainBytes = HexStringToBytes(plaintextValue);
			var encodedBytes = Encoder.Encrypt(plainBytes);
			var encodedValue = Convert.ToBase64String(encodedBytes);
			return (encodedValue, true);
		}

		static byte[] HexStringToBytes(string s)
		{
			var result = new byte[s.Length / 2];
			for (int i = 0; i < result.Length; i++)
			{
				result[i] = byte.Parse(s.Substring(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
			}
			return result;
		}

		static string BytesToHexString(byte[] bytes)
			=> string.Concat(bytes.Select(b => b.ToString("X2", CultureInfo.InvariantCulture)));

		#endregion
	}
}

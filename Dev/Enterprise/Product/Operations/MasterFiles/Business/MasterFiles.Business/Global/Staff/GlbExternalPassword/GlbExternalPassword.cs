using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core.Encryption;

namespace Enterprise.MasterFiles.Business
{
	[SingleObjectAroundARow]
	public class GlbExternalPassword : AutoGlbExternalPassword, Integration.IGlbExternalPassword
	{
		public GlbExternalPassword(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Type Decider

		public static readonly GlbExternalPasswordTypeDecider TypeDecider = new GlbExternalPasswordTypeDecider();

		#endregion

		public new class Schema : AutoGlbExternalPassword.Schema
		{
			public const string CurrentDecryptedPassword = "CurrentDecryptedPassword";
			public const string CurrentDecryptedCertificatePassphrase = "CurrentDecryptedCertificatePassphrase";
			public const string NextDecryptedPassword = "NextDecryptedPassword";
			public const string GP_PasswordStatusDescription = "GP_PasswordStatusDescription";
			public const string GP_PasswordTypeDescription = "GP_PasswordTypeDescription";

			public const int CurrentDecryptedPasswordMaxLength = 47;
			public const int CurrentDecryptedCertificatePassphraseMaxLength = 47;
			public const int NextDecryptedPasswordMaxLength = 47;
		}

		[ReadOnly(true)]
		public override ZGuid GP_GS
		{
			get => base.GP_GS;
			set => base.GP_GS = value;
		}

		#region GP_PasswordStatusDescription

		public ZString GP_PasswordStatusDescription => GP_PasswordStatus.IsEmpty ? string.Empty : Res.GetString("{27EB9865-0B59-4783-900B-07EF4C779E13}", "{0} ({1})", Lookups.PasswordStatusList.GetDescriptionFromCode(GP_PasswordStatus), GP_PasswordStatus);

		public ZPropertyInfo GP_PasswordStatusDescriptionInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.GP_PasswordStatusDescription); }
		}

		#endregion

		#region GP_PasswordTypeDescription

		public ZString GP_PasswordTypeDescription => Lookups.PasswordTypeList.GetDescriptionFromCode(GP_PasswordType) ?? GP_PasswordType;

		public ZPropertyInfo GP_PasswordTypeDescriptionInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.GP_PasswordTypeDescription); }
		}

		#endregion

		[List("Lookups.PasswordStatusList")]
		public override ZString GP_PasswordStatus { get => base.GP_PasswordStatus; set => base.GP_PasswordStatus = value; }

		[Password]
		public override ZString GP_CurrentPassword
		{
			get { return base.GP_CurrentPassword; }
			set
			{
				if (base.GP_CurrentPassword != value)
				{
					var oldValue = GP_CurrentPassword;
					base.GP_CurrentPassword = value;
					ChangeStatusWhenUpdateCurrentPassword(oldValue);
				}
			}
		}

		protected virtual void ChangeStatusWhenUpdateCurrentPassword(ZString oldValue)
		{
			var newValue = GP_CurrentPassword;
			if (oldValue != newValue)
			{
				GP_PasswordStatus = newValue.IsEmpty ? string.Empty : Core.Constants.PasswordOK;
			}
		}

		[Password]
		public override ZString GP_NextPassword
		{
			get { return base.GP_NextPassword; }
			set
			{
				if (base.GP_NextPassword != value)
				{
					base.GP_NextPassword = value;
					if (!value.IsEmpty)
					{
						GP_PasswordStatus = Core.Constants.PasswordOK;
					}
				}
			}
		}

		[MaxLength(Schema.CurrentDecryptedPasswordMaxLength)]
		[Password]
		public virtual ZString CurrentDecryptedPassword
		{
			get { return GP_CurrentPassword.IsEmpty ? ZString.Empty : (ZString)Encoder.Decrypt(GP_CurrentPassword); }
			set
			{
				var oldValue = CurrentDecryptedPassword;
				if (oldValue != value)
				{
					CheckMaximumLength(CurrentDecryptedPasswordInfo, value);
					GP_CurrentPassword = value.IsEmpty ? ZString.Empty : (ZString)Encoder.Encrypt(value);
					CurrentDecryptedPasswordInfo.RefreshBinding(oldValue);
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateCurrentDecryptedPassword();
				}
			}
		}

		public virtual ZPropertyInfo CurrentDecryptedPasswordInfo
		{
			get { return GetZPropertyInfo(Schema.CurrentDecryptedPassword); }
		}

		[MaxLength(Schema.NextDecryptedPasswordMaxLength)]
		[Password]
		public virtual ZString NextDecryptedPassword
		{
			get { return GP_NextPassword.IsEmpty ? ZString.Empty : (ZString)Encoder.Decrypt(GP_NextPassword); }
			set
			{
				var oldValue = NextDecryptedPassword;
				if (oldValue != value)
				{
					CheckMaximumLength(NextDecryptedPasswordInfo, value);
					GP_NextPassword = value.IsEmpty ? ZString.Empty : (ZString)Encoder.Encrypt(value);
					NextDecryptedPasswordInfo.RefreshBinding(oldValue);
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateNextDecryptedPassword();
				}
			}
		}

		public virtual ZPropertyInfo NextDecryptedPasswordInfo
		{
			get { return GetZPropertyInfo(Schema.NextDecryptedPassword); }
		}

		[MaxLength(Schema.CurrentDecryptedCertificatePassphraseMaxLength)]
		[Password]
		public virtual ZString CurrentDecryptedCertificatePassphrase
		{
			get { return GP_CertificatePassPhrase.IsEmpty ? ZString.Empty : (ZString)Encoder.Decrypt(GP_CertificatePassPhrase); }
			set
			{
				var oldValue = CurrentDecryptedCertificatePassphrase;
				if (oldValue != value)
				{
					CheckMaximumLength(CurrentDecryptedCertificatePassphraseInfo, value);
					GP_CertificatePassPhrase = value.IsEmpty ? ZString.Empty : (ZString)Encoder.Encrypt(value);
					CurrentDecryptedCertificatePassphraseInfo.RefreshBinding(oldValue);
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateCurrentDecryptedCertificatePassphrase();
				}
			}
		}

		public ZPropertyInfo CurrentDecryptedCertificatePassphraseInfo
		{
			get { return GetZPropertyInfo(Schema.CurrentDecryptedCertificatePassphrase); }
		}

		#region Encoder

		protected TwoWayEncoder Encoder
		{
			get { return encoder ?? (encoder = new TwoWayEncoder(Staff != null ? Staff.PK.ToGuid() : PK.ToGuid())); }
		}
		TwoWayEncoder encoder;

		#endregion

		public bool DisableConfigurationToSender;

		public override void OnSaving()
		{
			base.OnSaving();
			if (!DisableConfigurationToSender && ShouldSendCredential())
			{
				RegisterConfigurationForSending();
			}
		}

		protected internal virtual bool ShouldSendCredential()
		{
			return IsInDatabase ? HasDataChangesSinceLastSaved() : HasData();
		}

		protected virtual string InterchangeTypeForSending => EDIInterchangeTypeList.Codes.Configuration;

		protected virtual ZPropertyInfo[] CredentialApplicableInfos()
		{
			return new ZPropertyInfo[] { GP_CurrentPasswordInfo };
		}
		protected bool HasData()
		{
			return CredentialApplicableInfos().Any(x => !x.Value.IsEmpty);
		}

		protected bool HasDataChangesSinceLastSaved()
		{
			return CredentialApplicableInfos().Any(x => !x.Value.Equals(x.OriginalValue));
		}

		public override void Delete()
		{
			if (!DisableConfigurationToSender && IsInDatabase && ShouldSendDeleteCredential())
			{
				RegisterConfigurationForSending();
			}

			base.Delete();
		}

		protected virtual void RegisterConfigurationForSending()
		{
			ExternalPasswordConfigurationToSender.RegisterForSending(Factory, ConfigurationName, Company, Group, Staff ?? OriginalStaff, InterchangeTypeForSending, CredentialRecipient);
		}

		protected virtual bool ShouldSendDeleteCredential()
		{
			return !GP_PasswordStatusInfo.OriginalValue.IsEmpty;
		}

		public virtual CredentialRecipient CredentialRecipient => CredentialRecipient.eHub;

		public override GlbStaff Staff => GP_GS.IsValid ? base.Staff : null;
		public override GlbGroup Group => GP_GG.IsValid ? base.Group : null;
		public override GlbCompany Company => GP_GC.IsValid ? base.Company : null;

		internal GlbStaff OriginalStaff => GP_GSInfo.OriginalValue.IsValid ? Factory.Load<GlbStaff>((ZGuid)GP_GSInfo.OriginalValue) : null;

		public virtual ZString ConfigurationName => ZString.Empty;

		public bool HasExpiredByUtc
			=> GP_ExpiryDate <= ZDateTime.UtcNow;

		public virtual object CreateCredentialData()
		{
			var result = new Group()
			{
				Type = GetCredentialType(),
				Status = GetCredentialStatus(),
				Items = CreateCredentialItems()
			};

			return result;
		}

		#region Implementation

		protected virtual string GetCredentialType()
		{
			return GP_PasswordType;
		}

		protected virtual ZString GetCredentialStatus()
		{
			return GP_PasswordStatus;
		}

		protected virtual object[] CreateCredentialItems()
		{
			return System.Array.Empty<object>();
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			GP_GC = GlbCompany.CurrentCompany.PK;
		}

		#endregion
	}
}

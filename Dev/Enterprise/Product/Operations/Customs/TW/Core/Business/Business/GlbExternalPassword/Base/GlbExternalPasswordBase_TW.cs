using System.Data;
using System.Security.Cryptography.X509Certificates;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.TW.Business
{
	[CodeProperty(AutoGlbExternalPassword.Schema.GP_MailBoxID)]
	[DescriptionProperty(AutoGlbExternalPassword.Schema.GP_MailBoxID)]
	[SystemDefinedValues]
	public abstract class GlbExternalPasswordBase_TW : GlbExternalPasswordWithCertificate, ITemplateCopyable
	{
		public GlbExternalPasswordBase_TW(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : MasterFiles.Business.GlbExternalPassword.Schema
		{
			public const string CertExpiryDate = "CertExpiryDate";
			public const string GP_ReceiveAutomatically = "GP_ReceiveAutomatically";
		}

		#region ITemplateCopyable Members

		protected override bool SupportsCloneCore() => true;

		IBusiness ITemplateCopyable.TemplateCopy()
		{
			var passwordCopy = Clone() as GlbExternalPasswordBase_TW;

			passwordCopy.GP_MailBoxID = ZString.Empty;
			passwordCopy.GP_UserID = ZString.Empty;
			passwordCopy.GP_CurrentPassword = ZString.Empty;

			return passwordCopy;
		}

		#endregion

		[MaxLength(16)]
		public override ZString GP_MailBoxID
		{
			get { return base.GP_MailBoxID; }
			set
			{
				var oldValue = GP_MailBoxID;
				base.GP_MailBoxID = value;
				if (!IsCopying && oldValue != GP_MailBoxID)
				{
					CalculateGP_CertificatePassphraseStatus();
				}
			}
		}

		[MaxLength(35)]
		public override ZString GP_UserID
		{
			get { return base.GP_UserID; }
			set
			{
				var oldValue = GP_UserID;
				base.GP_UserID = value;
				if (!IsCopying && oldValue != GP_UserID)
				{
					CalculateGP_CertificatePassphraseStatus();
				}
			}
		}

		protected override bool GP_UserID_ReadOnly => false;

		public ZString SubscriptionTag
		{
			get
			{
				var combiner = !GP_MailBoxID.IsEmpty && !GP_UserID.IsEmpty ? " | " : string.Empty;
				return ZString.Format("{0}{1}{2}", GP_MailBoxID, combiner, GP_UserID);
			}
		}

		[List(nameof(Lookups) + "." + nameof(GlbExternalPasswordLookups.PasswordTypeList))]
		public override ZString GP_PasswordType
		{
			get => base.GP_PasswordType;
			set
			{
				var oldValue = GP_PasswordType;
				base.GP_PasswordType = value;
				GP_UserIDInfo.RefreshBinding();
				if (!IsCopying && oldValue != GP_PasswordType)
				{
					CalculateGP_CertificatePassphraseStatus();
				}
			}
		}

		[MaxLength(20)]
		public override ZString CurrentDecryptedPassword
		{
			get => base.CurrentDecryptedPassword;
			set
			{
				var oldValue = CurrentDecryptedPassword;
				base.CurrentDecryptedPassword = value;
				if (!IsCopying && oldValue != CurrentDecryptedPassword)
				{
					CalculateGP_CertificatePassphraseStatus();
				}
			}
		}

		public override ZBlob GP_Certificate
		{
			get => base.GP_Certificate;
			set
			{
				var oldValue = GP_Certificate;
				base.GP_Certificate = value;
				if (!IsCopying && oldValue != GP_Certificate)
				{
					CalculateGP_CertificatePassphraseStatus();
				}
			}
		}

		[MaxLength(10)]
		public override ZString CurrentDecryptedCertificatePassphrase
		{
			get => base.CurrentDecryptedCertificatePassphrase;
			set
			{
				var oldValue = CurrentDecryptedCertificatePassphrase;
				base.CurrentDecryptedCertificatePassphrase = value;
				if (!IsCopying && oldValue != CurrentDecryptedCertificatePassphrase)
				{
					CalculateGP_CertificatePassphraseStatus();
				}
			}
		}

		protected override bool IsCertificateValid => !GP_MailBoxID.IsEmpty && !GP_UserID.IsEmpty && !HasErrors && base.IsCertificateValid;

		protected override object[] CreateCredentialItems()
		{
			var platform = CredentialSender.CreateItem(MasterFiles.Business.Customs.XmlCredential.Constants.ItemTypes.Platform, GP_PasswordType);
			var receiveAutomatically = CredentialSender.CreateItem(MasterFiles.Business.Customs.XmlCredential.Constants.ItemTypes.ReceiveAutomatically, GP_ReceiveAutomatically ? "1" : "0");
			var sender = CredentialSender.CreateCredential(ZString.Empty, GP_UserID, CurrentDecryptedPassword);
			var certificate = CredentialSender.CreateCertificate(GP_Certificate, CurrentDecryptedCertificatePassphrase);
			return new object[] { platform, receiveAutomatically, sender, certificate };
		}

		public override object CreateCredentialData()
		{
			var result = new Group()
			{
				Type = MasterFiles.Business.Customs.XmlCredential.Constants.ItemTypes.MailBoxID,
				Reference = GP_MailBoxID,
				Status = GetCredentialStatus(),
				Items = CreateCredentialItems()
			};

			return result;
		}

		protected override ZPropertyInfo[] CredentialApplicableInfos()
		{
			return new ZPropertyInfo[] { GP_MailBoxIDInfo, GP_UserIDInfo, GP_CertificateInfo, GP_CertificatePassPhraseInfo, GP_CurrentPasswordInfo, GP_PasswordTypeInfo };
		}

		protected override bool ShouldSendDeleteCredential()
		{
			return !GP_MailBoxIDInfo.OriginalValue.IsEmpty
				|| !GP_UserIDInfo.OriginalValue.IsEmpty
				|| !GP_CertificateInfo.OriginalValue.IsEmpty
				|| !GP_CertificatePassPhraseInfo.OriginalValue.IsEmpty;
		}

		public override ZString ConfigurationName => TWCustomsSubscribers;
		public const string TWCustomsSubscribers = "TWCustomsSubscribers";

		public new GlbExternalPasswordValidationBase_TW Validation => (GlbExternalPasswordValidationBase_TW)GetNewValidation();
		protected override MasterFiles.Business.GlbExternalPasswordValidation GetNewValidation()
		{
			return new GlbExternalPasswordValidationBase_TW(this);
		}

		#region GenAddOnColumn
		[ResourceStringData("Enterprise.Customs.TW.Business.GlbExternalPasswordBase_TW|GP_ReceiveAutomatically", Caption = "Receive Automatically")]
		public ZBool GP_ReceiveAutomatically
		{
			get => this.GetSystemDefinedValue<ZBool>(Schema.GP_ReceiveAutomatically);
			set
			{
				var oldValue = GP_ReceiveAutomatically;
				this.SetSystemDefinedValue(Schema.GP_ReceiveAutomatically, value);
				GP_ReceiveAutomaticallyInfo.RefreshBinding(oldValue);
				if (!IsCopying && IsInDatabase && HasData() && oldValue != GP_ReceiveAutomatically)
				{
					ExternalPasswordConfigurationToSender.RegisterForSending(Factory, ConfigurationName, Company, Group, Staff, InterchangeTypeForSending);
				}
			}
		}

		protected override void DefaultDataFromCertificate(X509Certificate2 certificate)
		{
			GP_IssueDate = certificate.NotBefore;
			GP_ExpiryDate = certificate.NotAfter;
		}

		protected override void ClearDataDefaultedFromCertificate()
		{
			GP_IssueDate = ZDateTime.Empty;
			GP_ExpiryDate = ZDateTime.Empty;
		}

		public ZPropertyInfo GP_ReceiveAutomaticallyInfo
		{
			get { return GetZPropertyInfo(Schema.GP_ReceiveAutomatically); }
		}
		#endregion

		protected override void ChangeStatusWhenUpdateCurrentPassword(ZString oldValue)
		{
			var newValue = GP_CurrentPassword;
			if (oldValue != newValue)
			{
				CalculateGP_CertificatePassphraseStatus();
			}
		}

		public override ZString GP_PasswordStatus
		{
			get => base.GP_PasswordStatus;
			set
			{
				var oldValue = GP_PasswordStatus;
				base.GP_PasswordStatus = value;
				if (!IsCopying && oldValue != GP_PasswordStatus)
				{
					ClearStatusReasonIfRequired();
				}
			}
		}

		void ClearStatusReasonIfRequired()
		{
			if ((GP_PasswordStatus == PasswordStatusList.Codes.Valid || GP_PasswordStatus.IsEmpty) && !GP_StatusReason.IsEmpty)
			{
				GP_StatusReasonInfo.ClearValue();
			}
		}

		protected override string InterchangeTypeForSending => EDIInterchangeTypeList.Codes.ForwarderConfiguration;
	}
}

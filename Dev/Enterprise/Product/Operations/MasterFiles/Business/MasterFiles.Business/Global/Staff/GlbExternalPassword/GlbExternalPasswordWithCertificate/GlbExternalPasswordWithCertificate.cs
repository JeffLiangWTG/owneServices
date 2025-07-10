using System;
using System.Data;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public abstract class GlbExternalPasswordWithCertificate : GlbExternalPassword, Integration.IGlbExternalPasswordWithCertificate
	{
		protected GlbExternalPasswordWithCertificate(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : GlbExternalPassword.Schema
		{
			public const string CertificateStatus = "CertificateStatus";
		}

		[ReadOnlyMember(nameof(GP_IssueDate_ReadOnly))]
		public override ZDateTime GP_IssueDate { get => base.GP_IssueDate; set => base.GP_IssueDate = value; }
		protected virtual bool GP_IssueDate_ReadOnly => true;

		[ReadOnlyMember(nameof(GP_ExpiryDate_ReadOnly))]
		public override ZDateTime GP_ExpiryDate { get => base.GP_ExpiryDate; set => base.GP_ExpiryDate = value; }
		protected virtual bool GP_ExpiryDate_ReadOnly => true;

		[ReadOnlyMember(nameof(GP_UserID_ReadOnly))]
		public override ZString GP_UserID { get => base.GP_UserID; set => base.GP_UserID = value; }
		protected virtual bool GP_UserID_ReadOnly => true;

		public override ZBlob GP_Certificate
		{
			get => base.GP_Certificate;
			set
			{
				try
				{
					using (GetValidationSuspender())
					{
						var oldValue = GP_Certificate;
						base.GP_Certificate = value;
						if (!IsCopying && oldValue != GP_Certificate)
						{
							DefaultDataFromCertificate();
						}
					}
				}
				finally
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateGP_Certificate();
					}
				}
			}
		}

		public override ZString CurrentDecryptedCertificatePassphrase
		{
			get => base.CurrentDecryptedCertificatePassphrase;
			set
			{
				try
				{
					using (GetValidationSuspender())
					{
						var oldValue = CurrentDecryptedCertificatePassphrase;
						base.CurrentDecryptedCertificatePassphrase = value;
						if (!IsCopying && oldValue != CurrentDecryptedCertificatePassphrase)
						{
							DefaultDataFromCertificate();
						}
					}
				}
				finally
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateCurrentDecryptedCertificatePassphrase();
						Validation.ValidateGP_Certificate();
					}
				}
			}
		}

		public ZString CertificateStatus
		{
			get
			{
				if (!certificateStatus.HasValue)
				{
					CalculateCertificateStatus();
				}
				return certificateStatus.Value;
			}
		}
		ZString? certificateStatus;

		public ZPropertyInfo CertificateStatusInfo => GetZPropertyInfo(Schema.CertificateStatus);

		public static string CertificateLoaded => Res.GetString("1706668C-15B0-4719-9C71-FE007C34C8B0", "Loaded");
		public static string CertificateEmpty => Res.GetString("01256EFF-F8F5-488F-AD1C-D605AB67AC43", "Empty");
		public static string CertificateInvalid => Res.GetString("DBDD032F-A463-43F0-A960-E68AE8F90E94", "Invalid");

		public new GlbExternalPasswordWithCertificateValidation Validation => (GlbExternalPasswordWithCertificateValidation)base.Validation;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
		}

		protected void CalculateGP_CertificatePassphraseStatus()
		{
			GP_PasswordStatus = IsCertificateValid ? PasswordStatusList.Codes.Valid : PasswordStatusList.Codes.Invalid;
		}

		protected virtual bool IsCertificateValid => !CurrentDecryptedCertificatePassphrase.IsEmpty && CertificateStatus == CertificateLoaded;

		bool Integration.IGlbExternalPasswordWithCertificate.IsCertificateValid => IsCertificateValid;

		protected void DefaultDataFromCertificate()
		{
			CalculateCertificateStatus(DefaultDataFromCertificate);
			if (CertificateStatus != CertificateLoaded)
			{
				ClearDataDefaultedFromCertificate();
			}
			CalculateGP_CertificatePassphraseStatus();
		}

		protected virtual void DefaultDataFromCertificate(X509Certificate2 certificate)
		{
			GP_IssueDate = certificate.NotBefore;
			GP_ExpiryDate = certificate.NotAfter;
			GP_UserID = certificate.Thumbprint;
		}

		protected virtual void ClearDataDefaultedFromCertificate()
		{
			GP_IssueDate = ZDateTime.Empty;
			GP_ExpiryDate = ZDateTime.Empty;
			GP_UserID = ZString.Empty;
		}

		bool ExtraDataFromCertificate(Action<X509Certificate2> extraData)
		{
			var valid = false;
			var certificateBytes = (byte[])GP_Certificate;
			var certificatePassword = CurrentDecryptedCertificatePassphrase;
			if (certificateBytes != null && certificateBytes.Length != 0 && !string.IsNullOrEmpty(CurrentDecryptedCertificatePassphrase))
			{
				try
				{
					using (var certificate = new X509Certificate2(certificateBytes, certificatePassword, X509KeyStorageFlags.MachineKeySet))
					{
						extraData?.Invoke(certificate);
						valid = !string.IsNullOrEmpty(certificate.SerialNumber) || !string.IsNullOrEmpty(certificate.Issuer);
					}
				}
				catch (CryptographicException ex)
				{
					if (ex.HResult != ERROR_INVALID_OBJECT && ex.HResult != ERROR_INVALID_PASSWORD)
					{
						ErrorReporter.ReportOnce("CertificateCryptographicException", $"An unexpected cryptographic exception was thrown: {ex.Message}", ex);
					}
				}
			}

			return valid;
		}

		#region SuppressResourceStringsCheckRegion

		const int ERROR_INVALID_OBJECT = unchecked((int)0x80092009);
		const int ERROR_INVALID_PASSWORD = unchecked((int)0x80070056);

		#endregion

		void CalculateCertificateStatus(Action<X509Certificate2> extraData = null)
		{
			certificateStatus = CertificateLoaded;
			if (GP_Certificate == null)
			{
				certificateStatus = CertificateEmpty;
			}
			else if (!ExtraDataFromCertificate(extraData))
			{
				certificateStatus = CertificateInvalid;
			}
		}
	}
}

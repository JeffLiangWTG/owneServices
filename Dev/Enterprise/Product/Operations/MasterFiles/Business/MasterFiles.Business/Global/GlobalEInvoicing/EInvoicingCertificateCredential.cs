using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[UniversalDataContext(DataContextType.AccEInvoicingCredential)]
	public class EInvoicingCertificateCredential : GlbExternalPassword
	{
		public EInvoicingCertificateCredential(BusinessObjectFactory factory, System.Data.DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		#region CredentialSettings property

		IEInvoicingCredentialSettings credentialSettingsValue;
		public IEInvoicingCredentialSettings CredentialSettings
		{
			get => credentialSettingsValue;
			set
			{
				if (credentialSettingsValue == null)
				{
					credentialSettingsValue = value;
				}
				else
				{
					throw new InvalidOperationException($"{nameof(CredentialSettings)} can only be set once");
				}
			}
		}

		#endregion

		#region GP_UserID

		protected bool GP_UserID_ReadOnly => true;

		#endregion

		#region GP_Certificate

		public override ZBlob GP_Certificate
		{
			get => base.GP_Certificate;
			set
			{
				base.GP_Certificate = value;
				ExtractDetailsFromCertificate(true);
				UpdatePreferredSequenceNumberOnEntireCollection();
			}
		}

		#endregion

		#region CurrentDecryptedCertificatePassphrase

		public override ZString CurrentDecryptedCertificatePassphrase
		{
			get => base.CurrentDecryptedCertificatePassphrase;
			set
			{
				base.CurrentDecryptedCertificatePassphrase = value;
				ExtractDetailsFromCertificate(true);
				UpdatePreferredSequenceNumberOnEntireCollection();
			}
		}

		#endregion

		#region PasswordStatus

		public ZString PasswordStatus
			=> GP_PasswordStatus == PasswordStatusList.Codes.Valid && CertificateHasExpired ? ResString.GetMultilingualString("ca0ab7a2-6ebf-4481-92d2-b4ae8f46b5f5", "Expired")
			 : Lookups.PasswordStatusList.GetDescriptionFromCode(base.GP_PasswordStatus) ?? base.GP_PasswordStatus;

		#endregion

		#region SubjectNameCommonName

		public ZString SubjectNameCommonName
		{
			get
			{
				if (subjectNameCommonName.IsEmpty)
				{
					ExtractDetailsFromCertificate();
				}

				return subjectNameCommonName;
			}
		}
		protected ZString subjectNameCommonName;

		#endregion

		#region SubjectNameSerialNumber

		public ZString SerialNumber
		{
			get
			{
				if (serialNumber.IsEmpty)
				{
					ExtractDetailsFromCertificate();
				}

				return serialNumber;
			}
		}
		protected ZString serialNumber;

		#endregion

		#region IssuerNameCommonName

		public ZString IssuerNameCommonName
		{
			get
			{
				if (issuerNameCommonName.IsEmpty)
				{
					ExtractDetailsFromCertificate();
				}

				return issuerNameCommonName;
			}
		}
		protected ZString issuerNameCommonName;

		#endregion

		#region PreferredSequenceNumber

		public ZString PreferredSequenceNumberForDisplay
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get => fPreferredSequenceNumber <= 0 ? string.Empty : fPreferredSequenceNumber.ToString();
		}

		[ReadOnly(true)]
		public ZInt PreferredSequenceNumber
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get => fPreferredSequenceNumber;
			set
			{
				if (fPreferredSequenceNumber != value)
				{
					var oldValue = fPreferredSequenceNumber;
					fPreferredSequenceNumber = value;
					PreferredSequenceNumberInfo.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo PreferredSequenceNumberInfo
		{
			get { return GetZPropertyInfo(nameof(PreferredSequenceNumber)); }
		}

		ZInt fPreferredSequenceNumber;

		void UpdatePreferredSequenceNumberOnEntireCollection()
		{
			var parentCollection = ((IBusinessObjectInternals)this).ParentCollections.FirstOrDefault(x => typeof(EInvoicingCertificateCredential).IsAssignableFrom(x.TypeOfElements));
			if (parentCollection is GlbBranchEInvoicingCredentialCollection branchCollection)
			{
				branchCollection.UpdatePreferredSequenceNumberOnChildren();
			}
			else if (parentCollection is GlbCompanyEInvoicingCredentialCollection companyCollection)
			{
				companyCollection.UpdatePreferredSequenceNumberOnChildren();
			}
		}

		#endregion

		#endregion

		#region Base Class Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			GP_PasswordType = PasswordTypesList.Codes.EIM;
			GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
			GP_GS = ZGuid.Empty;
		}

		public new EInvoicingCredentialLookups Lookups => (EInvoicingCredentialLookups)base.Lookups;

		protected override GlbExternalPasswordLookups GetNewLookups()
			=> new EInvoicingCredentialLookups(this);

		public new EInvoicingCertificateCredentialValidation Validation
			=> (EInvoicingCertificateCredentialValidation)GetNewValidation();

		protected override GlbExternalPasswordValidation GetNewValidation()
			=> new EInvoicingCertificateCredentialValidation(this);

		public override MultilingualString GetWarningBeforeBeingDeleted()
		{
			if (CredentialSettings.IsCertificate())
			{
				if (!IsValidCertificate)
				{
					return (NoResString)string.Empty;
				}

				var parentCollection = ((IBusinessObjectInternals)this).ParentCollections.FirstOrDefault(x => typeof(EInvoicingCertificateCredential).IsAssignableFrom(x.TypeOfElements));
				if (parentCollection == null)
				{
					return (NoResString)string.Empty;
				}

				var countOfOtherValidCertificates = parentCollection.OfType<EInvoicingCertificateCredential>().Count(x => x.PK != this.PK && x.IsValidCertificate);
				if (countOfOtherValidCertificates > 0)
				{
					return (NoResString)string.Empty;
				}
				else
				{
					return ResString.GetMultilingualString("c1636f90-4f3d-4472-9caa-63a05a3e9342", "Deleting the last certificate may cause E-Invoicing errors.");
				}
			}
			else
			{
				return base.GetWarningBeforeBeingDeleted();
			}
		}

		#endregion

		#region UniqueIndexFailureHandler

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return new EInvoicingCredentialUniqueIndexFailureHandler(this); }
		}

		public bool IsMovingFromAnotherPerson { get; set; }

		class EInvoicingCredentialUniqueIndexFailureHandler : IUniqueIndexFailureHandler
		{
			public EInvoicingCredentialUniqueIndexFailureHandler(EInvoicingCertificateCredential parent)
			{
				Parent = parent;
			}

			EInvoicingCertificateCredential Parent { get; }

			public IEnumerable<string> HandledUniqueIndexNames
			{
				get { yield return GlbExternalPasswordSchema.Constants.Indexes.FK_UX__GP_GS_GP_GG_GP_GC_GP_GB_GP_PasswordType_GP_UserID_GP_MailBoxID; }
			}

			public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
			{
				var errorDetail = Res.GetString("ae8593d7-98b9-4468-86e5-1a69ea214084", @"This Certificate has already been entered.

Company: '{0}', Branch: '{1}', Password Type: '{2}', Thumb-print: '{3}'.",
	Parent.Company?.GC_Code,
	Parent.Branch?.GB_Code,
	Parent.GP_PasswordType,
	Parent.GP_UserID);
				notifier.ReportError(errorDetail, Res.GetString("8745404a-0399-4fc0-ba16-161337d79f3c", "Duplicate E-Invoicing Certificate Credential."));
			}
		}

		#endregion

		void ExtractDetailsFromCertificate(bool forceExtractDetailsFromCertificate = false)
		{
			if (forceExtractDetailsFromCertificate || !hasExtractedDetailsFromCertificate)
			{
				hasExtractedDetailsFromCertificate = true;

				var valid = false;

				var certificateBytes = CertificateRawData;
				var certificatePassword = CurrentDecryptedCertificatePassphrase;
				if (certificateBytes != null && certificateBytes.Length != 0
					&& (!string.IsNullOrEmpty(CurrentDecryptedCertificatePassphrase) || IsSupportCertificateDataBase64EncodedTwice))
				{
					try
					{
						using (var certificate = new X509Certificate2(certificateBytes, certificatePassword))
						{
							var subjectFields = GetDistinguishedNameFields(certificate.SubjectName);
							var issuerFields = GetDistinguishedNameFields(certificate.IssuerName);
							serialNumber = GetSerialNumber(certificate);
							subjectNameCommonName = subjectFields.TryGetValue("CN", out var subjectNameCommonNameTemp) ? subjectNameCommonNameTemp : string.Empty;
							issuerNameCommonName = issuerFields.TryGetValue("CN", out var issuerNameCommonNameTemp) ? issuerNameCommonNameTemp : string.Empty;

							var notBeforeUTC = certificate.NotBefore.ToUniversalTime();
							var notAfterUTC = certificate.NotAfter.ToUniversalTime();

							if (GP_GB != ZGuid.Empty && Branch?.HomePort?.TimeZoneSet != null)
							{
								var calculationTimeZone = Branch.HomePort.TimeZoneSet.GetCalculationTimeZone();
								GP_IssueDate = calculationTimeZone.ToLocalTime(notBeforeUTC);
								GP_ExpiryDate = calculationTimeZone.ToLocalTime(notAfterUTC);
							}
							else if (GP_GC != ZGuid.Empty && Company?.OrgProxy?.UNLOCO?.TimeZoneSet != null)
							{
								var calculationTimeZone = Company.OrgProxy.UNLOCO.TimeZoneSet.GetCalculationTimeZone();
								GP_IssueDate = calculationTimeZone.ToLocalTime(notBeforeUTC);
								GP_ExpiryDate = calculationTimeZone.ToLocalTime(notAfterUTC);
							}
							else
							{
								GP_IssueDate = certificate.NotBefore;
								GP_ExpiryDate = certificate.NotAfter;
							}

							GP_UserID = certificate.Thumbprint;
							GP_PasswordStatus = PasswordStatusList.Codes.Valid;

							valid = true;
						}
					}
					catch (CryptographicException)
					{
						valid = false;
					}
				}

				if (!valid)
				{
					serialNumber = ZString.Empty;
					subjectNameCommonName = ZString.Empty;
					issuerNameCommonName = ZString.Empty;
					GP_IssueDate = ZDateTime.Empty;
					GP_ExpiryDate = ZDateTime.Empty;
					GP_UserID = ZString.Empty;
					GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
				}
			}
		}
		bool hasExtractedDetailsFromCertificate;

		public byte[] CertificateRawData
		{
			get
			{
				var result = (byte[])GP_Certificate;
				if (IsSupportCertificateDataBase64EncodedTwice)
				{
					var decodedStr = System.Text.Encoding.UTF8.GetString(GP_Certificate);
					try
					{
						result = Convert.FromBase64String(decodedStr);
					}
					catch (FormatException)
					{
						//if invalid Base-64 string, then just return the original data without conversion
					}
				}
				return result;
			}
		}

		internal bool IsSupportCertificateDataBase64EncodedTwice => CountriesWithCertificateDataBase64EncodedTwice.Contains(Company?.GC_RN_NKCountryCode ?? ZString.Empty);

		internal readonly static ImmutableHashSet<ZString> CountriesWithCertificateDataBase64EncodedTwice = new ZString[]
		{
			 Core.Constants.CountryCodes.SaudiArabia
		}.ToImmutableHashSet();

		protected static Dictionary<string, string> GetDistinguishedNameFields(X500DistinguishedName distinguishedName)
		{
			return distinguishedName
				.Decode(X500DistinguishedNameFlags.UseSemicolons)
				.Split(';')
				.Select(distinguishedNameElement => { var parts = distinguishedNameElement.Split('='); return new { Key = parts.FirstOrDefault()?.Trim(), Value = string.Join("=", parts.Skip(1)) }; })
				.GroupBy(keyValue => keyValue.Key)
				.ToDictionary(keyValueGroup => keyValueGroup.Key, keyValueGroup => string.Join(".", keyValueGroup.Select(keyValue => keyValue.Value)).Trim());
		}

		protected virtual string GetSerialNumber(X509Certificate2 certificate)
		{
			return certificate.SerialNumber;
		}

		public bool CertificateHasExpired
			=> !GP_Certificate.IsEmpty
			&& GP_ExpiryDate <= ZDateTime.Now;

		public bool IsValidCertificate
			=> !GP_Certificate.IsEmpty
			&& GP_PasswordStatus == PasswordStatusList.Codes.Valid
			&& !CertificateHasExpired;

		protected static ZQuery GetQueryForCertificate(GlbBranch branch, ZDateTime now, string passwordType, string orderByColumn)
		{
			var query = new ZQuery(GlbExternalPasswordSchema.GP_GB, branch.PK);
			AddCommonCertificateCritera(query, now, passwordType, orderByColumn);
			return query;
		}

		protected static ZQuery GetQueryForCertificate(GlbCompany company, ZDateTime now, string passwordType, string orderByColumn)
		{
			var query = new ZQuery(GlbExternalPasswordSchema.GP_GC, company.PK);
			AddCommonCertificateCritera(query, now, passwordType, orderByColumn);
			return query;
		}

		static void AddCommonCertificateCritera(ZQuery query, ZDateTime now, string passwordType, string orderByColumn)
		{
			query.AddToFilter(GlbExternalPasswordSchema.GP_PasswordType, SQLComparisonOperator.Equal, passwordType);
			query.AddToFilter(GlbExternalPasswordSchema.GP_IssueDate, SQLComparisonOperator.LessThanOrEqualTo, now);
			query.AddToFilter(GlbExternalPasswordSchema.GP_ExpiryDate, SQLComparisonOperator.GreaterThanOrEqualTo, now);
			query.AddToFilter(GlbExternalPasswordSchema.GP_Certificate, SQLComparisonOperator.NotEqual, null);
			query.AddToFilter(GlbExternalPasswordSchema.GP_PasswordStatus, SQLComparisonOperator.Equal, PasswordStatusList.Codes.Valid);
			query.OrderBy = orderByColumn + OrderByClause.Descending + ", " + GlbExternalPassword.Schema.GP_SystemCreateTimeUtc;
		}

		public static EInvoicingCertificateCredential LoadBestCertificate(GlbBranch branch, ZDateTime? currentTime = null, string passwordType = PasswordTypesList.Codes.EIM, string orderByColumn = GlbExternalPassword.Schema.GP_IssueDate)
		{
			Argument.NotNull(branch, nameof(branch));
			var now = currentTime ?? ZDateTime.Now;

			var query = GetQueryForCertificate(branch, now, passwordType, orderByColumn);
			var result = branch.Factory.LoadTop1<GlbBranchEInvoicingCertificateCredential>(query);
			return result;
		}

		public static EInvoicingCertificateCredential LoadBestCertificate(GlbCompany company, ZDateTime? currentTime = null, string passwordType = PasswordTypesList.Codes.EIM, string orderByColumn = GlbExternalPassword.Schema.GP_IssueDate)
		{
			Argument.NotNull(company, nameof(company));
			var now = currentTime ?? ZDateTime.Now;

			var query = GetQueryForCertificate(company, now, passwordType, orderByColumn);
			var result = company.Factory.LoadTop1<GlbCompanyEInvoicingCertificateCredential>(query);
			return result;
		}
	}
}

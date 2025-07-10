using System;
using System.Collections.Concurrent;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ServiceTasks
{
	public sealed class CertificateRequirementChecker
	{
		#region Singleton Pattern

		static CertificateRequirementChecker Instance => LazyInstance.Value;

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<CertificateRequirementChecker> LazyInstance = CreateLazyInstance();

		static Lazy<CertificateRequirementChecker> CreateLazyInstance() => new(() => new CertificateRequirementChecker());

		#endregion

		record EntryKey(string ParentTablePrefix, string CountryCode, string CertificateType, string PasswordStatus);

		readonly ConcurrentDictionary<EntryKey, bool> certificatePresentDictionary = new();

		public static bool ExistsCompanyWithCertificate(string countryCode, string certificateType, string passwordStatus = null)
			=> Instance.ExistsCompanyWithCertificateCached(new EntryKey(GlbCompanySchema.Constants.Prefix, countryCode, certificateType, passwordStatus));

		bool ExistsCompanyWithCertificateCached(EntryKey entryKey) => certificatePresentDictionary.GetOrAdd(entryKey, ExistsCompanyWithCertificateUncached);

		bool ExistsCompanyWithCertificateUncached(EntryKey entryKey)
		{
			var factory = new BusinessObjectFactory();

			var companyQuery = new ZDBOnlyQuery(typeof(GlbCompany));
			companyQuery.AddToFilter(GlbCompanySchema.GC_IsActive, ZBool.True);
			companyQuery.AddToFilter(GlbCompanySchema.GC_RN_NKCountryCode, entryKey.CountryCode);

			var extPwdSubQuery = new ZDBOnlySubQuery(typeof(GlbExternalPassword), GlbExternalPasswordSchema.GP_GC);
			extPwdSubQuery.AddToFilter(GlbExternalPasswordSchema.GP_PasswordType, entryKey.CertificateType);

			if (entryKey.PasswordStatus != null)
			{
				extPwdSubQuery.AddToFilter(GlbExternalPasswordSchema.GP_PasswordStatus, entryKey.PasswordStatus);
			}

			companyQuery.AddSubQuery(extPwdSubQuery, JoinCondition.And);

			return factory.ExistsInDatabase(GlbCompanySchema.Constants.TableName, companyQuery);
		}

		public static bool ExistsStaffWithCertificate(string certificateType, string passwordStatus = null)
			=> Instance.ExistsStaffWithCertificateCached(new EntryKey(GlbStaffSchema.Constants.Prefix, string.Empty, certificateType, passwordStatus));

		bool ExistsStaffWithCertificateCached(EntryKey entryKey) => certificatePresentDictionary.GetOrAdd(entryKey, ExistsStaffWithCertificateUncached);

		bool ExistsStaffWithCertificateUncached(EntryKey entryKey)
		{
			var factory = new BusinessObjectFactory();

			var staffQuery = new ZDBOnlyQuery(typeof(GlbStaff));
			staffQuery.AddToFilter(GlbStaffSchema.GS_IsActive, ZBool.True);

			var extPwdSubQuery = new ZDBOnlySubQuery(typeof(GlbExternalPassword), GlbExternalPasswordSchema.GP_GS);
			extPwdSubQuery.AddToFilter(GlbExternalPasswordSchema.GP_PasswordType, entryKey.CertificateType);

			if (entryKey.PasswordStatus != null)
			{
				extPwdSubQuery.AddToFilter(GlbExternalPasswordSchema.GP_PasswordStatus, entryKey.PasswordStatus);
			}

			staffQuery.AddSubQuery(extPwdSubQuery, JoinCondition.And);

			return factory.ExistsInDatabase(GlbStaffSchema.Constants.TableName, staffQuery);
		}

#if DEBUG
		public static void ResetForTesting() => Instance.certificatePresentDictionary.Clear();
#endif
	}
}

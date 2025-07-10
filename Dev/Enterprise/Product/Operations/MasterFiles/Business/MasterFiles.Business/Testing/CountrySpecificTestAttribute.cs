#if DEBUG

using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	/// <summary>
	/// CountrySpecificTestAttribute.  Sets the current company to wherever you like.
	/// </summary>
	[AttributeUsage(System.AttributeTargets.Assembly | System.AttributeTargets.Class)]
	public sealed class CountrySpecificTestAttribute : TestSetupAttribute
	{
		public CountrySpecificTestAttribute(string countryCode)
		{
			CountryCode = countryCode;
		}

		public override void SetUp(TestCase testCase)
		{
			if (!string.IsNullOrEmpty(CountryCode) && GlbCompany.CurrentCompany.GC_RN_NKCountryCode != CountryCode)
			{
				StoredCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				if (TransactionedTestCase.InTransactionedTestCase || UseSnapshotProtectionAttribute.IsProtected)
				{
					StoredCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
					StoredHomePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

					var factory = GlbCompany.CurrentCompany.Factory;
					var refCountry = factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCode)
						?? throw new InvalidOperationException("Could not find country : " + CountryCode);

					var refUnloco = factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, CountryCode) { OrderBy = RefUNLOCOSchema.Constants.RL_Code })
						?? throw new InvalidOperationException("Could not find a valid home port for : " + CountryCode);

					FastUpdateCompanyBranchAndReload(CountryCode, refCountry.RN_RX_NKLocalCurrency, refUnloco.RL_Code);
				}
				else
				{
					Env.Security.BranchModify.IsAllowed = true;
					GlbCompany.CurrentCompany.SetCountry(CountryCode);
				}
			}
		}

		public override void TearDown(TestCase testCase)
		{
			if (!string.IsNullOrEmpty(StoredCountry) && StoredCountry != GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
			{
				if (TransactionedTestCase.InTransactionedTestCase || UseSnapshotProtectionAttribute.IsProtected)
				{
					FastUpdateCompanyBranchAndReload(StoredCountry, StoredCurrency, StoredHomePort);
				}
				else
				{
					GlbCompany.CurrentCompany.SetCountry(StoredCountry);
				}
			}
		}

		void FastUpdateCompanyBranchAndReload(string country, string currency, string homePort)
		{
			var sql = @"
UPDATE dbo.GlbCompany SET
	GC_RN_NKCountryCode      = @countryCode,
	GC_RX_NKLocalCurrency    = @currencyCode,
	GC_SystemLastEditUser    = 'USR',
	GC_SystemLastEditTimeUtc = GetUtcDate()
WHERE
	GC_PK                    = @companyPk

UPDATE dbo.GlbBranch SET
	GB_RL_NKHomePort         = @homePort,
	GB_SystemLastEditUser    = 'USR',
	GB_SystemLastEditTimeUtc = GetUtcDate()
WHERE
	GB_PK                    = @branchPk"
				;
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPk", SqlDbType.UniqueIdentifier, Env.CurrentCompanyPK);
				command.AddParameter("@branchPk", SqlDbType.UniqueIdentifier, Env.CurrentBranchPK);
				command.AddParameter("@countryCode", SqlDbType.VarChar, country);
				command.AddParameter("@currencyCode", SqlDbType.VarChar, currency);
				command.AddParameter("@homePort", SqlDbType.VarChar, homePort);
				command.ExecuteNonQuery();
			}
			GlbCompany.CurrentCompany.Reload();
			GlbBranch.CurrentBranch.Reload();
		}

		#region Implementation

		public ZString CountryCode { get; }

		public ZString StoredCountry { get; private set; }
		public ZString StoredCurrency { get; private set; }
		public ZString StoredHomePort { get; private set; }

		#endregion
	}
}
#endif

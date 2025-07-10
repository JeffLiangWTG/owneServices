using System.Data;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgMergeParameterisedSqlProvider))]
	public class MergeJobExchangeRatesSqlProviderTest : OrgMergeParameterisedSqlProviderTestBase
	{
		public override string ExpectedFullSqlText => $@"-- Merge Job Exchange Rates
MERGE 
	dbo.JobExRate as target
USING 
	(SELECT 
		JF_OH_Org, 
		JF_JH, 
		JF_OrgType, 
		JF_RX_NKRateCurrency 
	FROM 
		dbo.JobExRate
	WHERE
		JF_OH_Org = {OrgMergeParameterisedSqlProvider.NewPkParameter}) as source
ON
	(target.JF_JH = source.JF_JH AND
	target.JF_OrgType = source.JF_OrgType AND
	target.JF_RX_NKRateCurrency = source.JF_RX_NKRateCurrency AND
	target.JF_OH_Org = {OrgMergeParameterisedSqlProvider.OldPkParameter})
WHEN MATCHED
	THEN DELETE;";

		public override OrgMergeParameterisedSqlProvider Provider => new MergeJobExchangeRatesSqlProvider();

		public void TestJobExchangeRatesMerge()
		{
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			var oldOrg = Factory.NewWithValidTestData<OrgHeader>();
			var newOrg = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var sqlInsertRates =
@"INSERT INTO dbo.JobExRate(JF_PK, JF_RX_NKRateCurrency, JF_JH, JF_BaseRate, JF_CFXMinimum, JF_CFXPercent, JF_IsTransformed, JF_OH_Org, JF_OrgType, JF_SystemCreateTimeUtc, JF_SystemCreateUser, JF_SystemLastEditTimeUtc, JF_SystemLastEditUser) VALUES
(NEWID(), 'USD', @JobPk, 1.5, 0, 5, 1, @OldOrg, 'DEB', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
(NEWID(), 'USD', @JobPk, 1.8, 0, 7, 1, @NewOrg, 'DEB', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (var cmd = TestConnection.Command(sqlInsertRates))
			{
				cmd.AddParameter("@JobPk", SqlDbType.UniqueIdentifier, jobHeader.PK.ToGuid());
				cmd.AddParameter("@OldOrg", SqlDbType.UniqueIdentifier, oldOrg.PK.ToGuid());
				cmd.AddParameter("@NewOrg", SqlDbType.UniqueIdentifier, newOrg.PK.ToGuid());
				cmd.ExecuteNonQuery();
			}

			AssertNoExceptionThrown("We should not have any issues running JobExRates sql", () => RunProviderSql(oldOrg, newOrg));

			var sqlGetRatesCount =
@"SELECT
	COUNT(1)
FROM
	dbo.JobExRate
WHERE
	JF_RX_NKRateCurrency = 'USD' AND
	JF_JH = @JobPk AND
	JF_OrgType = 'DEB' AND
	JF_OH_Org IN (@OldOrg, @NewOrg)";

			using (var cmd = TestConnection.Command(sqlGetRatesCount))
			{
				cmd.AddParameter("@JobPk", SqlDbType.UniqueIdentifier, jobHeader.PK.ToGuid());
				cmd.AddParameter("@OldOrg", SqlDbType.UniqueIdentifier, oldOrg.PK.ToGuid());
				cmd.AddParameter("@NewOrg", SqlDbType.UniqueIdentifier, newOrg.PK.ToGuid());

				AssertEquals("Only one rate left, so no index violations expected after merge", 1, (int)cmd.ExecuteScalar());
			}
		}
	}
}

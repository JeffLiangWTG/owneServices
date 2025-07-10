using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class LocationQueryProviderTest : TestCaseWithFactory
	{
		public void TestGetQueryFromUNLOCO()
		{
			LocationQueryProvider provider = new LocationQueryProvider(Factory, OrgHeaderSchema.OH_RL_NKClosestPort, typeof(OrgHeader));
			ZQuery query = provider.GetQuery(SQLComparisonOperator.Equal, "AUSYD");
			AssertEquals("(OH_RL_NKClosestPort IN ('AUSYD'))", query.LiteralTextADO);

			provider = new LocationQueryProvider(Factory, OrgHeaderSchema.OH_RL_NKClosestPort, typeof(OrgHeader));
			query = provider.GetQuery(SQLComparisonOperator.Equal, new ZString[] { "AUSYD", "AUBNE" });
			AssertEquals("(OH_RL_NKClosestPort IN ('AUSYD', 'AUBNE'))", query.LiteralTextADO);
		}

		public void TestGetQueryFromRegion()
		{
			LocationQueryProvider provider = new LocationQueryProvider(Factory, OrgHeaderSchema.OH_RL_NKClosestPort, typeof(OrgHeader));
			ZQuery query = provider.GetQuery(SQLComparisonOperator.Equal, "AUEC");
			AssertEquals(@"(OH_RL_NKClosestPort IN
(
	SELECT DISTINCT RL_Code
	FROM dbo.vw_ZoneUNLOCO WITH (NOEXPAND)
	WHERE FZ_Code IN ('AUEC')
	UNION ALL
	SELECT DISTINCT RL_Code
	FROM dbo.vw_ZoneUNLOCOByCountry WITH (NOEXPAND)
	WHERE FZ_Code IN ('AUEC')
))",
				query.LiteralTextADO);

			provider = new LocationQueryProvider(Factory, OrgHeaderSchema.OH_RL_NKClosestPort, typeof(OrgHeader));
			query = provider.GetQuery(SQLComparisonOperator.Equal, new ZString[] { "AUEC", "USEC" });
			AssertEquals(@"(OH_RL_NKClosestPort IN
(
	SELECT DISTINCT RL_Code
	FROM dbo.vw_ZoneUNLOCO WITH (NOEXPAND)
	WHERE FZ_Code IN ('AUEC', 'USEC')
	UNION ALL
	SELECT DISTINCT RL_Code
	FROM dbo.vw_ZoneUNLOCOByCountry WITH (NOEXPAND)
	WHERE FZ_Code IN ('AUEC', 'USEC')
))",
				query.LiteralTextADO);
		}

		public void TestGetQueryFromCountry()
		{
			LocationQueryProvider provider = new LocationQueryProvider(Factory, OrgHeaderSchema.OH_RL_NKClosestPort, typeof(OrgHeader));
			ZQuery query = provider.GetQuery(SQLComparisonOperator.Equal, "AU");
			AssertEquals("(OH_RL_NKClosestPort like 'AU%')", query.LiteralTextADO);

			provider = new LocationQueryProvider(Factory, OrgHeaderSchema.OH_RL_NKClosestPort, typeof(OrgHeader));
			query = provider.GetQuery(SQLComparisonOperator.Equal, new ZString[] { "AU", "NZ" });
			AssertEquals("(OH_RL_NKClosestPort like 'AU%' OR OH_RL_NKClosestPort like 'NZ%')", query.LiteralTextADO);
		}
	}
}

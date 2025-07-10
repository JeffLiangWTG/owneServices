using System;
using System.Linq;
using CargoWise.EntityFramework;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	sealed class RefCusCodeListTransportModeQueryBuilderTest : TestCase
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("When QueryBuilder is null", () => new RefCusCodeListTransportModeQueryBuilder(queryBuilder: null, "ABC"));
			AssertExceptionThrown<ArgumentException>("When transport mode is null", () => new RefCusCodeListTransportModeQueryBuilder(new Mock<IRefCusCodeListSqlQueryBuilder>().Object, transportMode: null));
			AssertExceptionThrown<ArgumentException>("When transport mode is empty", () => new RefCusCodeListTransportModeQueryBuilder(new Mock<IRefCusCodeListSqlQueryBuilder>().Object, transportMode: ""));
		}

		public void TestBuild()
		{
			var dynamicSqlQuery = new ZJoinQuery("RefCusCodeList", new[] { "Code" });
			var mockQueryBuilder = new Mock<IRefCusCodeListSqlQueryBuilder>();
			mockQueryBuilder.Setup(m => m.Build(It.IsAny<ZQuery>())).Returns(dynamicSqlQuery);

			IRefCusCodeListSqlQueryBuilder builder = new RefCusCodeListTransportModeQueryBuilder(mockQueryBuilder.Object, "ABC");
			var query = builder.Build(new ZQuery());
			var (result, parameters) = query.GetSql();
			var sqlParameters = (parameters ?? Array.Empty<ZSqlParameter>()).ToArray();

			AssertNotNull("SQL Query", result);

			CombineAssertions(() =>
			{
				AssertContains("Contains LEFT Join with specific transport type", "LEFT JOIN RefDatabase_RefCusCodeOrAttributeTransportMode SpecificTransportMode ON SpecificTransportMode.ZZU_ZZD_CodeList = ZZD_PK AND SpecificTransportMode.ZZU_TransportMode = @TransportMode", result);
				AssertContains("Contains OUTER Join", "OUTER APPLY (SELECT TOP 1 ZZU_ZZD_CodeList FROM RefDatabase_RefCusCodeOrAttributeTransportMode WHERE ZZU_ZZD_CodeList = ZZD_PK) AnyTransportMode", result);
				AssertContains("Where Condition", "WHERE SpecificTransportMode.ZZU_TransportMode IS NOT NULL OR AnyTransportMode.ZZU_ZZD_CodeList IS NULL", result);
				AssertEquals("Parameters Count", 1, sqlParameters.Length);
				AssertEquals("Parameter Name", "@TransportMode", sqlParameters[0].ParameterName);
				AssertEquals("Parameter Value", "ABC", sqlParameters[0].Value);
			});
		}
	}
}

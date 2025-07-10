using System;
using System.Linq;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	sealed class RefCusCodeListQueryBuilderTest : TestCase
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>("When languagecode is null", () => new RefCusCodeListQueryBuilder(null, false));
			AssertNoExceptionThrown("When languagecode is empty", () => new RefCusCodeListQueryBuilder("", false));
		}

		public void TestBuild()
		{
			IRefCusCodeListSqlQueryBuilder builder = new RefCusCodeListQueryBuilder("EN", false);
			var query = builder.Build(new ZQuery());
			var (sqlText, parameters) = query.GetSql();

			var sqlParameters = (parameters ?? Array.Empty<ZSqlParameter>()).ToArray();

			AssertNotNull("SQL Query", sqlText);
			var expectedSql = @"SELECT ZZD_Code as Code, ISNULL(ZXA_Description, ZZD_Description) as Description
FROM dbo.ZZRefCusCodeListCombined
LEFT JOIN RefDatabase_RefCusCodeListLanguage ON ZXA_ZZD_CodeList = ZZD_PK AND ZXA_ZX6_NKLanguage = @Language";

			AssertEquals("SQL", expectedSql, sqlText);
			AssertEquals("Parameters Count", 1, sqlParameters.Length);
			AssertEquals("Parameter Name", "@Language", sqlParameters[0].ParameterName);
			AssertEquals("Parameter Name", "EN", sqlParameters[0].Value);
		}

		public void TestBuild_WhenOnlyCurrentLanguageIsTrue()
		{
			IRefCusCodeListSqlQueryBuilder builder = new RefCusCodeListQueryBuilder("EN", true);
			var query = builder.Build(new ZQuery());
			var (sqlText, _) = query.GetSql();
			var expectedSql = @"SELECT ZZD_Code as Code, ISNULL(ZXA_Description, ZZD_Description) as Description
FROM dbo.ZZRefCusCodeListCombined
INNER JOIN RefDatabase_RefCusCodeListLanguage ON ZXA_ZZD_CodeList = ZZD_PK AND ZXA_ZX6_NKLanguage = @Language";

			AssertEquals("SQL", expectedSql, sqlText);
		}
	}
}

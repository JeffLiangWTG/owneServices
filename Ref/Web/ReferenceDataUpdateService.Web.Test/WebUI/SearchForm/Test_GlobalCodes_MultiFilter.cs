using System;
using System.Collections.Generic;
using System.Data;

namespace ReferenceDataUpdateService.Web.Test.WebUI.SearchForm
{
	public class Test_GlobalCodes_MultiFilter : IPortalSearchFormTest
	{
		public void PrepareData(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			var sql = @"
INSERT INTO RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description)
VALUES (newid(), 'ZA', 'X');

INSERT INTO RefCusCodeType (ZZK_PK, ZZK_CodeType, ZZK_Description, ZZK_IsReadonly, ZZK_MaxLength, ZZK_ZZZ_NKDataGrouping)
VALUES (newid(), 'CUSOF', 'AAA', 1, 0, 'ZA');

INSERT INTO RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping)
VALUES
('5C05D030-C204-488D-8169-B8C640EC5070', 'CUSOF', '4701', 'JOHN F KENNEDY AIRPORT, N', '2018-01-19', '2079-06-06 23:59:00', 'ZA');
INSERT INTO RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping)
VALUES
('47B20241-A8EF-4260-80DA-E13FCB50FB49', 'CUSOF', '4702', 'LA AIRPORT', '2018-01-19', '2079-06-06 23:59:00', 'ZA');
INSERT INTO RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping)
VALUES
('A9D001C5-497A-4810-AF9F-09CFB4E6EC06', 'CUSOF', '4703', 'CALIFORNIA AIRPORT', '2018-01-19', '2079-06-06 23:59:00', 'ZA');
";
			safeCommand.CommandText = sql;
			safeCommand.ExecuteNonQuery();
		}

		public IEnumerable<PortalSearchFormFilter> GetFilters()
		{
			return new[]
			{
				new PortalSearchFormFilter { Name = "Code", Data = "4703" },
				new PortalSearchFormFilter {Name = "Description", Data = "CALIFORNIA AIRPORT"}
			};
		}

		public Uri GetPageUrl()
		{
			return new Uri("/RefCusCodeListUserViewSearchForm", UriKind.Relative);
		}

		public IEnumerable<IEnumerable<string>> GetExpectedResult()
		{
			return new[]
			{
				new[]
				{
					"CUSOF", "4703", "CALIFORNIA AIRPORT", "ZA", true.ToString(),
					"/RefCusCodeListUserViewDetailsForm/a9d001c5-497a-4810-af9f-09cfb4e6ec06"
				}
			};
		}
	}
}

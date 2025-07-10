using System;
using System.Collections.Generic;
using System.Data;

namespace ReferenceDataUpdateService.Web.Test.WebUI.SearchForm
{
	public class Test_GlobalCodes_MatchNoRow : IPortalSearchFormTest
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
('1B4C96F5-11FD-4748-8E5B-8CC885932232', 'CUSOF', '4701', 'JOHN F KENNEDY AIRPORT, N', '2018-01-19', '2079-06-06 23:59:00', 'ZA');
INSERT INTO RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping)
VALUES
('20F4BCC3-60CC-4CB2-BB46-7AA23D6BA96E', 'CUSOF', '4702', 'LA AIRPORT', '2018-01-19', '2079-06-06 23:59:00', 'ZA');
";
			safeCommand.CommandText = sql;
			safeCommand.ExecuteNonQuery();
		}

		public IEnumerable<PortalSearchFormFilter> GetFilters()
		{
			return new[]
			{
				new PortalSearchFormFilter
				{
					Name = "Code", Operation = PortalSearchFormFilter.FilterOperation.Equals, Data = "4703"
				}
			};
		}

		public Uri GetPageUrl()
		{
			return new Uri("/RefCusCodeListUserViewSearchForm", UriKind.Relative);
		}

		public IEnumerable<IEnumerable<string>> GetExpectedResult()
		{
			return new List<IEnumerable<string>>();
		}
	}
}

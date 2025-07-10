using System;
using System.Collections.Generic;
using System.Data;

namespace ReferenceDataUpdateService.Web.Test.WebUI.SearchForm
{
	public class Test_GlobalCodes_SingleRow : IPortalSearchFormTest
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
('C8E604B7-53CF-4F16-B820-75CB0986446B', 'CUSOF', '4701', 'JOHN F KENNEDY AIRPORT, N', '2018-01-19', '2079-06-06 23:59:00', 'ZA');
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
					Name = "Code", Operation = PortalSearchFormFilter.FilterOperation.Equals, Data = "4701"
				}
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
					"CUSOF", "4701", "JOHN F KENNEDY AIRPORT, N", "ZA", true.ToString(),
					"/RefCusCodeListUserViewDetailsForm/c8e604b7-53cf-4f16-b820-75cb0986446b"
				}
			};
		}
	}
}

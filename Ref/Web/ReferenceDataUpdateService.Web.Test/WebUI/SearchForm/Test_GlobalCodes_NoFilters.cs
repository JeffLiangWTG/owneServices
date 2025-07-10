using System;
using System.Collections.Generic;
using System.Data;

namespace ReferenceDataUpdateService.Web.Test.WebUI.SearchForm
{
	public class Test_GlobalCodes_NoFilters : IPortalSearchFormTest
	{
		public void PrepareData(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			var sql = @"
INSERT INTO RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description)
VALUES (newid(), 'ZA', 'X');

INSERT INTO RefCusCodeType (ZZK_PK, ZZK_CodeType, ZZK_Description, ZZK_IsReadonly, ZZK_MaxLength, ZZK_ZZZ_NKDataGrouping)
VALUES (newid(), 'CUSOF', 'AAA', 1, 0, 'ZA');

INSERT INTO dbo.RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping)
VALUES ('07EAD196-9011-4A8E-A291-DEA2AE66D020', 'CUSOF', '1234', 'Description 1', '2023-01-01', '2023-12-31', 'ZA');

INSERT INTO dbo.RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping)
VALUES ('325089A6-DD34-4141-ACD6-2D54CCF06221', 'CUSOF', '5678', 'Description 2', '2023-01-01', '2023-12-31', 'ZA');

INSERT INTO dbo.RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping)
VALUES ('9E21255C-B409-401B-BA79-E8FE6DEA509B', 'CUSOF', '9101', 'Description 3', '2023-01-01', '2023-12-31', 'ZA');

INSERT INTO dbo.RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping)
VALUES ('23C5546D-BBE3-473A-B26B-DCDEDA9029C4', 'CUSOF', '1121', 'Description 4', '2023-01-01', '2023-12-31', 'ZA');

INSERT INTO dbo.RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping)
VALUES ('16D957CB-1FEE-4DA7-8027-0F5BB891566A', 'CUSOF', '3141', 'Description 5', '2023-01-01', '2023-12-31', 'ZA');
";
			safeCommand.CommandText = sql;
			safeCommand.ExecuteNonQuery();
		}

		public IEnumerable<PortalSearchFormFilter> GetFilters()
		{
			return Array.Empty<PortalSearchFormFilter>();
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
					"CUSOF", "3141", "Description 5", "ZA", true.ToString(),
					"/RefCusCodeListUserViewDetailsForm/16d957cb-1fee-4da7-8027-0f5bb891566a"
				},
				new[]
				{
					"CUSOF", "1121", "Description 4", "ZA", true.ToString(),
					"/RefCusCodeListUserViewDetailsForm/23c5546d-bbe3-473a-b26b-dcdeda9029c4"
				},
				new[]
				{
					"CUSOF", "9101", "Description 3", "ZA", true.ToString(),
					"/RefCusCodeListUserViewDetailsForm/9e21255c-b409-401b-ba79-e8fe6dea509b"
				},
				new[]
				{
					"CUSOF", "5678", "Description 2", "ZA", true.ToString(),
					"/RefCusCodeListUserViewDetailsForm/325089a6-dd34-4141-acd6-2d54ccf06221"
				},
				new[]
				{
					"CUSOF", "1234", "Description 1", "ZA", true.ToString(),
					"/RefCusCodeListUserViewDetailsForm/07ead196-9011-4a8e-a291-dea2ae66d020"
				},
			};
		}
	}
}

using System.Collections.Generic;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public partial class RefDocOrgCusCodeUpdaterInfo_1 : IUpdaterScriptInfo
	{
		public string DataSetName => "RefDocOrgCusCode";

		public Dictionary<string, string> PrepareTemporaryTablesScripts => new Dictionary<string, string>
		{
{ "#TempRefDocOrgCusCode", @"
IF OBJECT_ID('tempdb..#TempRefDocOrgCusCode') IS NOT NULL
BEGIN
	DROP TABLE #TempRefDocOrgCusCode
END
	SELECT DOC_PK AS DOC_PK,DOC_RN_NKRegulatingCountry AS DOC_RN_NKRegulatingCountry,DOC_RN_NKCodeCountry AS DOC_RN_NKCodeCountry,DOC_CodeType AS DOC_CodeType,DOC_DocumentType AS DOC_DocumentType,DOC_Priority AS DOC_Priority,DOC_Notes AS DOC_Notes,DOC_ShortLabel AS DOC_ShortLabel,DOC_LongLabel AS DOC_LongLabel,DOC_Description AS DOC_Description,CAST (0 AS BIT) AS Deleted INTO #TempRefDocOrgCusCode FROM RefDocOrgCusCode WHERE 1 =0
	CREATE CLUSTERED INDEX PK_#TempRefDocOrgCusCode ON #TempRefDocOrgCusCode (DOC_PK)
" }
		};

		public string MergeScript => @"
DECLARE @dummy bit = 0

DECLARE @RefDocOrgCusCode_DELETE TABLE
(
	DOC_PK uniqueidentifier INDEX _@RefDocOrgCusCode_DELETE_PK CLUSTERED
)



INSERT INTO @RefDocOrgCusCode_DELETE
SELECT t.DOC_PK
FROM RefDocOrgCusCode AS t
JOIN #TempRefDocOrgCusCode AS s ON (((t.DOC_RN_NKRegulatingCountry = s.DOC_RN_NKRegulatingCountry) AND (t.DOC_RN_NKCodeCountry = s.DOC_RN_NKCodeCountry) AND (t.DOC_CodeType = s.DOC_CodeType) AND (t.DOC_DocumentType = s.DOC_DocumentType))) ;




DELETE FROM RefDocOrgCusCode
WHERE DOC_PK IN (SELECT DOC_PK FROM @RefDocOrgCusCode_DELETE);


INSERT RefDocOrgCusCode (DOC_PK,DOC_RN_NKRegulatingCountry,DOC_RN_NKCodeCountry,DOC_CodeType,DOC_DocumentType,DOC_Priority,DOC_Notes,DOC_ShortLabel,DOC_LongLabel,DOC_Description)
SELECT DOC_PK,DOC_RN_NKRegulatingCountry,DOC_RN_NKCodeCountry,DOC_CodeType,DOC_DocumentType,DOC_Priority,DOC_Notes,DOC_ShortLabel,DOC_LongLabel,DOC_Description FROM #TempRefDocOrgCusCode
WHERE Deleted = 0;


TRUNCATE TABLE #TempRefDocOrgCusCode


";

		public string[] Prerequisites => new string[0];
		public int UpdaterVersion => 1;
	}
}

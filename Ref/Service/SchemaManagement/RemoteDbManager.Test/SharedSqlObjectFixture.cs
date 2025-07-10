using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class SharedSqlObjectFixture
	{
		[Test]
		public void TestGetCreateSqlScript_View()
		{
			Assert.AreEqual(RateViewSql, testView.GetCreateSqlScriptByVersion(1));
		}

		[Test]
		public void TestGetCreateSqlScript_Trigger()
		{
			Assert.AreEqual(TriggerSql, testTrigger.GetCreateSqlScriptByVersion(0));
		}

		readonly SqlObject testView = new View("RateView");
		readonly SqlObject testTrigger = new Trigger("TG_RefCusCodeList_INS_UPD");

		const string RateViewSql = @"
IF NOT EXISTS(
	SELECT * FROM sys.objects o
	WHERE o.type = 'V' AND o.name = 'RateView_V1'
)
EXEC dbo.sp_executesql @statement = N'CREATE VIEW RateView_V1
WITH SCHEMABINDING
AS
SELECT ZZ2_PK
, ZZ2_ZZ1_Tariff AS ZZ2_ZZ1_ParentTariffOrNationalCode
, ''ZZ1'' AS ZZ2_ParentTableType
, ZZ2_StartDate
, ZZ2_EndDate
, ZZ2_ZY1_RateCode
, ZZ2_RateFormula
, ZZ2_ZZS_Preference
, ZZ2_RateFormulaDerivedFrom
, CASE ZZ2_ZZZ_NKDataGrouping 
WHEN '''' THEN ZZ1_ZZZ_NKDataGrouping
ELSE ZZ2_ZZZ_NKDataGrouping
END AS ZZ2_ZZZ_NKDataGrouping
,ZZ2_RX_NKCurrencyOverride
FROM dbo.RefCusRate AS rate
INNER JOIN dbo.RefCusTariff
on ZZ2_ZZ1_Tariff = ZZ1_PK
UNION ALL
SELECT 
ZZ2_PK
, ZZ2_ZZW_TariffNationalCode AS ZZ2_ZZ1_ParentTariffOrNationalCode
, ''ZZW'' AS ZZ2_ParentTableType
, ZZ2_StartDate
, ZZ2_EndDate
, ZZ2_ZY1_RateCode
, ZZ2_RateFormula
, ZZ2_ZZS_Preference
, ZZ2_RateFormulaDerivedFrom
, CASE ZZ2_ZZZ_NKDataGrouping 
WHEN '''' THEN ZZW_ZZZ_NKDataGrouping
ELSE ZZ2_ZZZ_NKDataGrouping
END AS ZZ2_ZZZ_NKDataGrouping
,ZZ2_RX_NKCurrencyOverride
FROM dbo.RefCusRate AS rate
INNER JOIN dbo.RefCusTariffNationalCode
on ZZ2_ZZW_TariffNationalCode = ZZW_PK'";

		const string TriggerSql = @"
IF NOT EXISTS(
	SELECT * FROM sys.objects o
	WHERE o.type = 'TR' AND o.name = 'TG_RefCusCodeList_INS_UPD'
)
EXEC dbo.sp_executesql @statement = N'CREATE TRIGGER [dbo].[TG_RefCusCodeList_INS_UPD] ON [dbo].[RefCusCodeList]
FOR INSERT, UPDATE
AS
BEGIN
	IF UPDATE(ZZD_ZZK_NKCodeType) OR UPDATE(ZZD_ZZZ_NKDataGrouping)
	BEGIN
		IF EXISTS
		(
			SELECT NULL FROM inserted I
			LEFT JOIN RefCusCodeType C ON C.ZZK_CodeType = I.ZZD_ZZK_NKCodeType AND C.ZZK_ZZZ_NKDataGrouping = I.ZZD_ZZZ_NKDataGrouping
			WHERE C.ZZK_PK IS NULL
		)
		THROW 58012, ''A RefCusCodeList was inserted/updated with invalid values in ZZD_ZZK_NKCodeType and/or ZZD_ZZZ_NKDataGrouping which are not found in RefCusCodeType.ZZK_CodeType and RefCusCodeType.ZZK_ZZZ_NKDataGrouping'', 1;
	END
END
'";
	}
}

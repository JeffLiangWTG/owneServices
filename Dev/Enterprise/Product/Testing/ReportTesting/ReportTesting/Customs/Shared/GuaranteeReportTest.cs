namespace Enterprise.ReportTesting.Customs.Shared
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using CargoWise.Data;
	using CargoWise.Types;
	using Enterprise.MasterFiles.Business;
	using Enterprise.MasterFiles.Module;

	[TemplateName("Guarantee Report")]
	public class TestGuaranteeReportTemplate : TemplateTestCase
	{
	}

	public class TestGuaranteeReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustFilesReports(); }
		}

		public override string MenuName => "Guarantee Report";

		public override string Hint => "";

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestGuaranteeReportTemplate();
		}
	}

	class GuaranteeReportTest : ReportFunctionalTestCase
	{
		protected override ZString ObjectName => "Report_Guarantee";

		protected override List<ReportSchemaColumn> ExpectedColumnsInAnyOrder
		{
			get
			{
				var allCols = new List<ReportSchemaColumn>();
				foreach (var stringCol in new string[] { "CountryCode", "GuaranteeType", "GuaranteeSubType", "GuaranteeHolder", "GuaranteeHolderName", "GuaranteeNumber", "QtyValIndicator", "UnitOfMeasure", "TransactionType", "TransactionTypeDescription", "TransactionTypeReference", "TransactionComment", "TransactionApplicationID", "TransactionStatus" })
				{
					allCols.Add(new ReportSchemaColumn(typeof(string), stringCol));
				}

				foreach (var dateCol in new string[] { "StartDate", "EndDate", "TransactionDate", "TransactionDateMin", "TransactionDateMax" })
				{
					allCols.Add(new ReportSchemaColumn(typeof(DateTime), dateCol));
				}

				foreach (var decimalCol in new string[] { "GuaranteeAmount", "TransactionValue", "RemainingBalance" })
				{
					allCols.Add(new ReportSchemaColumn(typeof(decimal), decimalCol));
				}

				foreach (var guidCol in new string[] { "CPH_OH_PermitHolder" })
				{
					allCols.Add(new ReportSchemaColumn(typeof(Guid), guidCol));
				}

				return allCols;
			}
		}

		protected override SqlObjectType SqlObjectType => Enterprise.ReportTesting.SqlObjectType.FunctionTable;

		protected override void AssertTestResults(DataTable resultsOrderedByExpectedColumnNames)
		{
			AssertEquals(1, resultsOrderedByExpectedColumnNames.Rows.Count);
			var row1 = FormatRowsValues(resultsOrderedByExpectedColumnNames.Rows[0], resultsOrderedByExpectedColumnNames);
			AssertContainsMoreHelpfully($"[CountryCode]='AU'; [GuaranteeType]='IMP'; [GuaranteeSubType]=''; [CPH_OH_PermitHolder]='{organisationPK}'; [GuaranteeHolder]='TESTORG'; [GuaranteeHolderName]=''; [GuaranteeNumber]='NUMBER1'; [StartDate]='2020-01-01T00:00:00'; [EndDate]='2021-01-03T00:00:00'; [GuaranteeAmount]='100.0000000'; [QtyValIndicator]='BTH'; [UnitOfMeasure]=''; [TransactionDate]='2021-01-01T00:00:00'; [TransactionType]='OBL'; [TransactionTypeDescription]='Opening Balance'; [TransactionTypeReference]='REFOBL'; [TransactionComment]='CMTOBL'; [TransactionApplicationID]='APPIDOBL'; [TransactionValue]='100.0000000'; [TransactionStatus]='PND'; [TransactionDateMin]='2021-01-01T00:00:00'; [TransactionDateMax]='2021-01-01T00:00:00'; [RemainingBalance]='300.0000000'", row1);
		}

		protected override void PrepareTestData()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_Code = "TESTORG";
			Factory.Save();
			organisationPK = header.PK;
			var sql = $@"
INSERT INTO dbo.CusPermitHeader (CPH_PK, CPH_OH_PermitHolder, CPH_StartDate, CPH_EndDate, CPH_Number, CPH_QtyValIndicator, CPH_RN_NKCountryCode, CPH_Type, CPH_ApplicationCode, CPH_Balance, CPH_SystemCreateTimeUtc, CPH_SystemCreateUser, CPH_SystemLastEditTimeUtc, CPH_SystemLastEditUser) VALUES
('F36121E2-EBFC-4784-9064-CE2545001A58', '{organisationPK}', '2020-01-01', '2021-01-03', 'NUMBER1', 'BTH', 'AU', 'IMP', 'GUA', 200, GetUtcDate(), '~BP', GetUtcDate(), '~BP');
INSERT INTO dbo.CusPermitLineTransaction (CPL_PK, CPL_CPH_PermitHeader, CPL_TransactionDate, CPL_TransactionCategory, CPL_TransactionType, CPL_Reference, CPL_Comment, CPL_AppId, CPL_TransactionStatus, CPL_TranValue, CPL_SystemCreateTimeUtc, CPL_SystemCreateUser, CPL_SystemLastEditTimeUtc, CPL_SystemLastEditUser) VALUES
('6B03896A-C80B-4FF2-A02A-FB5777D27328', 'F36121E2-EBFC-4784-9064-CE2545001A58', '2021-01-01', 'CUM', 'OBL', 'REFOBL', 'CMTOBL', 'APPIDOBL', 'PND', 100, GetUtcDate(), '~BP', GetUtcDate(), '~BP');
";
			Db.Connection.ExecuteNonQuery(sql);
		}

		protected override List<string> ParametersValuesList
		{
			get
			{
				return new List<string>() { "'AU,US'", "'2020-01-01'", "'2021-01-01'", "'2021-01-02'" };
			}
		}

		ZGuid organisationPK;
	}
}

using System;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ExportStatementCreatorObjectTest : ExportStatementCreatorTestCase
	{
		public void TestConstructor_HandlingOfNullParameter()
		{
			AssertArgumentExceptionThrown<ArgumentNullException>("ExportStatementSetting", () => new ExportStatementCreator(null));
		}

		public void TestExportStatement()
		{
			CountryExportStatementSetting countrySetting = new CountryExportStatementSetting();
			countrySetting.CountryCode = "AU";
			ExportStatementSetting statementSetting = countrySetting.Statements.AddNew();
			statementSetting.Code = "NDR";
			statementSetting.Statement = "TESTING STATEMENT";
			statementSetting.Field1 = "FL1";
			statementSetting.Field2 = "FL2";
			ExportStatementCreator creator = new ExportStatementCreator(statementSetting);
			AssertEquals("ExportStatement", "TESTING STATEMENT-FL1-FL2", creator.ExportStatement);
		}
	}
}

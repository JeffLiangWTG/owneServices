using System;
using CargoWise.Types;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	public class DeclarationExportStatementCreatorTest : ExportStatementCreatorTestCase
	{
#if NETFRAMEWORK
		[ExpectExceptionMessage(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: declaration")]
#else
		[ExpectExceptionMessage(typeof(ArgumentNullException), "Value cannot be null. (Parameter 'declaration')")]
#endif
		public void TestConstructor_HandlingOfNullParameter()
		{
			new DeclarationExportStatementCreator(null, new ExportStatementSetting());
		}

		public void TestExportStatement()
		{
			JobDeclaration declaration = GetJobDeclaration("AGT123", "SHP123", "HWB123");

			CountryExportStatementSetting countrySetting = new CountryExportStatementSetting();
			countrySetting.CountryCode = Core.Constants.CountryCodes.Australia;
			ExportStatementSetting statementSetting1 = countrySetting.Statements.AddNew();
			statementSetting1.Code = "ST1";
			statementSetting1.Statement = "TESTING STATEMENT1";
			statementSetting1.Field1 = SEDStatementFieldType.Codes.AgentEIN;
			statementSetting1.Field2 = SEDStatementFieldType.Codes.ITN;
			ExportStatementSetting statementSetting2 = countrySetting.Statements.AddNew();
			statementSetting2.Code = "ST2";
			statementSetting2.Statement = "TESTING STATEMENT2";
			statementSetting2.Field1 = SEDStatementFieldType.Codes.ShipperEIN;
			statementSetting2.Field2 = SEDStatementFieldType.Codes.SRN;
			ExportStatementSetting statementSetting3 = countrySetting.Statements.AddNew();
			statementSetting3.Code = "ST3";
			statementSetting3.Statement = "TESTING STATEMENT3";
			statementSetting3.Field1 = SEDStatementFieldType.Codes.XTN;
			DeclarationExportStatementCreator creator1 = new DeclarationExportStatementCreator(declaration, statementSetting1);
			DeclarationExportStatementCreator creator2 = new DeclarationExportStatementCreator(declaration, statementSetting2);
			DeclarationExportStatementCreator creator3 = new DeclarationExportStatementCreator(declaration, statementSetting3);
			AssertEquals("Declaration.CustomsEntryHeaders.Count", 0, declaration.CustomsEntryHeaders.Count);
			AssertEquals("ExportStatement1", "", creator1.ExportStatement);
			AssertEquals("ExportStatement2", "", creator2.ExportStatement);
			AssertEquals("ExportStatement3", "", creator3.ExportStatement);
			countrySetting.CountryCode = Core.Constants.CountryCodes.UnitedStates;
			AssertEquals("ExportStatement1", "", creator1.ExportStatement);
			AssertEquals("ExportStatement2", "", creator2.ExportStatement);
			AssertEquals("ExportStatement3", "", creator3.ExportStatement);

			CusEntryHeader entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader1.EntryNumber = "1ITN123";
			entryHeader1.US_XTN = "1XTN123";
			CusEntryHeader entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader2.EntryNumber = "2ITN123";
			entryHeader2.US_XTN = "2XTN123";
			countrySetting.CountryCode = Core.Constants.CountryCodes.Australia;

			AssertEquals("Declaration.CustomsEntryHeaders.Count", 2, declaration.CustomsEntryHeaders.Count);
			AssertMultilineASCIIEquals("ExportStatement1", string.Format("TESTING STATEMENT1-{0}-{1}", SEDStatementFieldType.Codes.AgentEIN, SEDStatementFieldType.Codes.ITN), creator1.ExportStatement);
			AssertMultilineASCIIEquals("ExportStatement2", string.Format("TESTING STATEMENT2-{0}-{1}", SEDStatementFieldType.Codes.ShipperEIN, SEDStatementFieldType.Codes.SRN), creator2.ExportStatement);
			AssertMultilineASCIIEquals("ExportStatement3", string.Format("TESTING STATEMENT3-{0}", SEDStatementFieldType.Codes.XTN), creator3.ExportStatement);
			countrySetting.CountryCode = Core.Constants.CountryCodes.UnitedStates;
			AssertMultilineASCIIEquals("ExportStatement1", "TESTING STATEMENT1 AGT123 1ITN123\nTESTING STATEMENT1 AGT123 2ITN123", creator1.ExportStatement);
			AssertMultilineASCIIEquals("ExportStatement2", "TESTING STATEMENT2 SHP123 HWB123", creator2.ExportStatement);
			AssertMultilineASCIIEquals("ExportStatement3", "TESTING STATEMENT3 1XTN123\nTESTING STATEMENT3 2XTN123", creator3.ExportStatement);
		}

		JobDeclaration GetJobDeclaration(ZString agentEIN, ZString shipperEIN, ZString houseBill)
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			OrgHeader forwarder = CreateOrganisation("FORWARDER NAME", "FORWARDER ADDRESS1", agentEIN);
			declaration.JE_OH_Forwarder = forwarder.PK;
			OrgHeader consignor = CreateOrganisation("CONSIGNOR NAME", "CONSIGNOR ADDRESS1", shipperEIN);
			declaration.JE_OH_Supplier = consignor.PK;
			declaration.JE_HouseBill = houseBill;
			return declaration;
		}
	}
}

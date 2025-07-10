using System;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs.US;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CusEntryHeaderExportStatementCreatorTest : ExportStatementCreatorTestCase
	{
		public void TestShipperEINAndFilerIDAndDateOfExport()
		{
			var filer = new ExportEntryFilerID();
			filer.EntryFilerID = "831-23-2123";
			filer.EntryFilerIDType = "S";
			USCustomsDataRegistry.Instance.ExportEntryFilerID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);
			var filerOnBranchLevel = new ExportEntryFilerID();
			filerOnBranchLevel.EntryFilerID = "851-98-4453";
			filerOnBranchLevel.EntryFilerIDType = "S";
			USCustomsDataRegistry.Instance.ExportEntryFilerID.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, filerOnBranchLevel);
			var entryHeader = GetCusEntryHeader("AGT-123", "SHP-123", "HWB123", "ITN123", "XTN123");
			entryHeader.MergedLines.AddNew();
			entryHeader.MergedLines[0].InvoiceLines.AddNew();
			entryHeader.MergedLines[0].InvoiceLines[0].US_DateOfExport = new ZDateTime(2008, 9, 20);
			var countrySetting = new CountryExportStatementSetting();
			countrySetting.CountryCode = Core.Constants.CountryCodes.Australia;
			var statementSetting1 = countrySetting.Statements.AddNew();
			statementSetting1.Code = "ST1";
			statementSetting1.Statement = "AESPOST";
			statementSetting1.Field1 = SEDStatementFieldType.Codes.ShipperEINAndFilerID;
			statementSetting1.Field2 = SEDStatementFieldType.Codes.DateOfExport;
			var statementSetting2 = countrySetting.Statements.AddNew();
			statementSetting2.Code = "ST2";
			statementSetting2.Statement = "AESDOWN";
			statementSetting2.Field1 = SEDStatementFieldType.Codes.FilerID;
			statementSetting2.Field2 = SEDStatementFieldType.Codes.DateOfExport;
			var creator1 = new CusEntryHeaderExportStatementCreator(entryHeader, statementSetting1);
			var creator2 = new CusEntryHeaderExportStatementCreator(entryHeader, statementSetting2);
			AssertEquals("ExportStatement1", string.Format("AESPOST-{0}-{1}", SEDStatementFieldType.Codes.ShipperEINAndFilerID, SEDStatementFieldType.Codes.DateOfExport), creator1.ExportStatement);
			AssertEquals("ExportStatement2", string.Format("AESDOWN-{0}-{1}", SEDStatementFieldType.Codes.FilerID, SEDStatementFieldType.Codes.DateOfExport), creator2.ExportStatement);

			countrySetting.CountryCode = Core.Constants.CountryCodes.UnitedStates;
			AssertEquals("ExportStatement1", "AESPOST SHP123-851984453 09/20/2008", creator1.ExportStatement);
			AssertEquals("ExportStatement2", "AESDOWN 851984453 09/20/2008", creator2.ExportStatement);

			countrySetting.CountryCode = Core.Constants.CountryCodes.PuertoRico;
			AssertEquals("ExportStatement1", "AESPOST SHP123-851984453 09/20/2008", creator1.ExportStatement);
			AssertEquals("ExportStatement2", "AESDOWN 851984453 09/20/2008", creator2.ExportStatement);

			((IRegistryItemInternals)USCustomsDataRegistry.Instance.ExportEntryFilerID).DeleteValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			AssertEquals("ExportStatement1", "AESPOST SHP123-831232123 09/20/2008", creator1.ExportStatement);
			AssertEquals("ExportStatement2", "AESDOWN 831232123 09/20/2008", creator2.ExportStatement);
		}

		public void TestExportStatement()
		{
			var entryHeader = GetCusEntryHeader("AGT123", "SHP123", "HWB123", "ITN123", "XTN123");
			var countrySetting = new CountryExportStatementSetting();
			countrySetting.CountryCode = "AU";
			var statementSetting1 = countrySetting.Statements.AddNew();
			statementSetting1.Code = "ST1";
			statementSetting1.Statement = "TESTING STATEMENT1";
			statementSetting1.Field1 = SEDStatementFieldType.Codes.AgentEIN;
			statementSetting1.Field2 = SEDStatementFieldType.Codes.ITN;
			var statementSetting2 = countrySetting.Statements.AddNew();
			statementSetting2.Code = "ST2";
			statementSetting2.Statement = "TESTING STATEMENT2";
			statementSetting2.Field1 = SEDStatementFieldType.Codes.ShipperEIN;
			statementSetting2.Field2 = SEDStatementFieldType.Codes.SRN;
			var statementSetting3 = countrySetting.Statements.AddNew();
			statementSetting3.Code = "ST3";
			statementSetting3.Statement = "TESTING STATEMENT3";
			statementSetting3.Field1 = SEDStatementFieldType.Codes.XTN;
			var creator1 = new CusEntryHeaderExportStatementCreator(entryHeader, statementSetting1);
			var creator2 = new CusEntryHeaderExportStatementCreator(entryHeader, statementSetting2);
			var creator3 = new CusEntryHeaderExportStatementCreator(entryHeader, statementSetting3);
			AssertEquals("ExportStatement1", string.Format("TESTING STATEMENT1-{0}-{1}", SEDStatementFieldType.Codes.AgentEIN, SEDStatementFieldType.Codes.ITN), creator1.ExportStatement);
			AssertEquals("ExportStatement2", string.Format("TESTING STATEMENT2-{0}-{1}", SEDStatementFieldType.Codes.ShipperEIN, SEDStatementFieldType.Codes.SRN), creator2.ExportStatement);
			AssertEquals("ExportStatement3", string.Format("TESTING STATEMENT3-{0}", SEDStatementFieldType.Codes.XTN), creator3.ExportStatement);
			countrySetting.CountryCode = Core.Constants.CountryCodes.UnitedStates;
			AssertEquals("ExportStatement1", "TESTING STATEMENT1 AGT123 ITN123", creator1.ExportStatement);
			AssertEquals("ExportStatement2", "TESTING STATEMENT2 SHP123 HWB123", creator2.ExportStatement);
			AssertEquals("ExportStatement3", "TESTING STATEMENT3 XTN123", creator3.ExportStatement);
			entryHeader.CH_Status = AESDirectCustomsEntryStatus.Codes.DeleteSEDClear;
			AssertEquals("pre-condition", true, entryHeader.HasBeenWithdrawn);
			AssertEquals("ExportStatement1", "TESTING STATEMENT1 AGT123", creator1.ExportStatement.TrimEnd());
			AssertEquals("ExportStatement3", "TESTING STATEMENT3", creator3.ExportStatement.TrimEnd());
		}

		CusEntryHeader GetCusEntryHeader(ZString agentEIN, ZString shipperEIN, ZString houseBill, ZString iTN, ZString xTN)
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			var forwarder = CreateOrganisation("FORWARDER NAME", "FORWARDER ADDRESS1", agentEIN);
			declaration.JE_OH_Forwarder = forwarder.PK;
			var consignor = CreateOrganisation("CONSIGNOR NAME", "CONSIGNOR ADDRESS1", shipperEIN);
			declaration.JE_OH_Supplier = consignor.PK;
			declaration.JE_HouseBill = houseBill;
			entryHeader.EntryNumber = iTN;
			entryHeader.US_XTN = xTN;
			return entryHeader;
		}
	}
}

using System;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs.US;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ShipmentExportStatementCreatorTest : ExportStatementCreatorTestCase
	{
		public void TestConstructor_HandlingOfNullParameter()
		{
			AssertArgumentExceptionThrown<ArgumentNullException>("Shipment",
				() => new ShipmentExportStatementCreator(null, new ExportStatementSetting(), DateFormat));
		}

		public void TestShipperEINAndFilerIDAndDateOfExport()
		{
			var filer = new ExportEntryFilerID();
			filer.EntryFilerID = "831-23-2123";
			filer.EntryFilerIDType = "S";
			ObjectFactory.Get<Enterprise.Integration.Customs.US.IUSCustomsDataRegistry>().ExportEntryFilerID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);

			var filerOnBranchLevel = new ExportEntryFilerID();
			filerOnBranchLevel.EntryFilerID = "851-98-4453";
			filerOnBranchLevel.EntryFilerIDType = "S";
			ObjectFactory.Get<Enterprise.Integration.Customs.US.IUSCustomsDataRegistry>().ExportEntryFilerID.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, filerOnBranchLevel);

			var shipment = GetShipment("AG-T123", "SH-P123", "HWB123", "ITN123");
			shipment.JS_E_DEP = new ZDateTime(2008, 9, 20);
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
			var creator1 = new ShipmentExportStatementCreator(shipment, statementSetting1, DateFormat);
			var creator2 = new ShipmentExportStatementCreator(shipment, statementSetting2, DateFormat);
			AssertEquals("ExportStatement1", string.Format("AESPOST-{0}-{1}", SEDStatementFieldType.Codes.ShipperEINAndFilerID, SEDStatementFieldType.Codes.DateOfExport), creator1.ExportStatement);
			AssertEquals("ExportStatement2", string.Format("AESDOWN-{0}-{1}", SEDStatementFieldType.Codes.FilerID, SEDStatementFieldType.Codes.DateOfExport), creator2.ExportStatement);

			countrySetting.CountryCode = Core.Constants.CountryCodes.UnitedStates;
			AssertEquals("ExportStatement1", "AESPOST SHP123-851984453 09/20/2008", creator1.ExportStatement);
			AssertEquals("ExportStatement2", "AESDOWN 851984453 09/20/2008", creator2.ExportStatement);

			countrySetting.CountryCode = Core.Constants.CountryCodes.PuertoRico;
			AssertEquals("ExportStatement1", "AESPOST SHP123-851984453 09/20/2008", creator1.ExportStatement);
			AssertEquals("ExportStatement2", "AESDOWN 851984453 09/20/2008", creator2.ExportStatement);

			((IRegistryItemInternals)ObjectFactory.Get<Enterprise.Integration.Customs.US.IUSCustomsDataRegistry>().ExportEntryFilerID).DeleteValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			AssertEquals("ExportStatement1", "AESPOST SHP123-831232123 09/20/2008", creator1.ExportStatement);
			AssertEquals("ExportStatement2", "AESDOWN 831232123 09/20/2008", creator2.ExportStatement);
		}

		public void TestExportStatement()
		{
			CommonShipment shipment = GetShipment("AGT123", "SHP123", "HWB123", "ITN123");
			CountryExportStatementSetting countrySetting = new CountryExportStatementSetting();
			countrySetting.CountryCode = "AU";
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
			ShipmentExportStatementCreator creator1 = new ShipmentExportStatementCreator(shipment, statementSetting1, DateFormat);
			ShipmentExportStatementCreator creator2 = new ShipmentExportStatementCreator(shipment, statementSetting2, DateFormat);
			ShipmentExportStatementCreator creator3 = new ShipmentExportStatementCreator(shipment, statementSetting3, DateFormat);
			AssertEquals("ExportStatement1", string.Format("TESTING STATEMENT1-{0}-{1}", SEDStatementFieldType.Codes.AgentEIN, SEDStatementFieldType.Codes.ITN), creator1.ExportStatement);
			AssertEquals("ExportStatement2", string.Format("TESTING STATEMENT2-{0}-{1}", SEDStatementFieldType.Codes.ShipperEIN, SEDStatementFieldType.Codes.SRN), creator2.ExportStatement);
			AssertEquals("ExportStatement3", string.Format("TESTING STATEMENT3-{0}", SEDStatementFieldType.Codes.XTN), creator3.ExportStatement);
			countrySetting.CountryCode = Core.Constants.CountryCodes.UnitedStates;
			AssertEquals("ExportStatement1", "TESTING STATEMENT1 AGT123 ITN123", creator1.ExportStatement);
			AssertEquals("ExportStatement2", "TESTING STATEMENT2 SHP123 HWB123", creator2.ExportStatement);
			AssertEquals("ExportStatement3", string.Format("TESTING STATEMENT3 {0}", SEDStatementFieldType.Codes.XTN), creator3.ExportStatement);
		}

		CommonShipment GetShipment(ZString agentEIN, ZString shipperEIN, ZString houseBill, ZString iTN)
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			OrgHeader exportBroker = CreateOrganisation("EXPORT BROKER NAME", "EXPORT BROKER ADDRESS1", agentEIN);
			shipment.JS_OH_ExportBroker = exportBroker.PK;
			OrgHeader consignor = CreateOrganisation("CONSIGNOR NAME", "CONSIGNOR ADDRESS1", shipperEIN);
			shipment.ConsignorPK = consignor.PK;
			shipment.JS_HouseBill = houseBill;
			shipment.CustomsEntryNumber = iTN;
			return shipment;
		}

		readonly string DateFormat = "MM/dd/yyyy";
	}
}

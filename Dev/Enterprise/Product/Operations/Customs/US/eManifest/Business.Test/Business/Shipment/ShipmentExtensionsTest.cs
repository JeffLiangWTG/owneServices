using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	sealed class ShipmentExtensionsTest : TestCaseWithFactory
	{
		public void TestCreateDecarations()
		{
			var glbCompany = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany.GC_RN_NKCountryCode = "US";
			var branch = glbCompany.Branches.AddNew();
			branch.GB_GC = glbCompany.PK;
			var tripBO = Factory.NewWithValidTestData<Trip>();
			tripBO.BH_JobReference = "Voyage1";
			tripBO.DocManagerInfo().AddFileOrDocument(new byte[] { 5, 6, 7 }, "123456789.txt", "TXT");
			tripBO.DocManagerInfo().Save();
			var shipment = tripBO.Shipments.AddNew();
			shipment.B0_ReferenceID = "Ship1";
			Factory.Save();
			var declaration = shipment.CreateCustomsDeclaration(tripBO);
			AssertEquals("Event Processed", tripBO.BH_JobReference, declaration.JE_VoyageFlightNo);
			AssertEquals("dDocs copied", tripBO.DocManagerInfo().AllEDocs.Count, declaration.DocManagerInfo.AllEDocs.Count);
		}

		public void TestCreateShipment2ForDeclaration()
		{
			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer.OH_Code = "MFG";
			var shipTo = Factory.NewWithValidTestData<OrgHeader>();
			shipTo.OH_Code = "SST";
			var glbCompany = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany.GC_RN_NKCountryCode = "US";
			var branch = glbCompany.Branches.AddNew();
			branch.GB_GC = glbCompany.PK;
			branch.GB_RN_NKCountryCode = "US";
			Factory.Save();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var trip1 = Factory.NewWithValidTestData<Trip>();
				trip1.BH_GB = branch.PK;
				trip1.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.eManifest;
				trip1.BH_VoyageNumber = "Voyage1";
				trip1.BH_PortUnladingDCode = "2704";
				trip1.BH_RL_NKPortUnlading = "USBUF";
				trip1.BH_ETA = ZDateTime.Today;
				trip1.BH_CarrierSCAC = "OKSC";
				var importer = Factory.NewWithValidTestData<OrgAddress>();
				importer.Header.OH_Code = "IMPORTER1";
				trip1.BH_OA_Importer = importer.PK;
				trip1.BH_TransitDirection = "I";
				var carrier = Factory.NewWithValidTestData<OrgHeader>();
				carrier.OH_Code = "CARRIER1";
				var scacCode = carrier.CustomsCodes.AddNew();
				scacCode.OK_CodeType = "CCC";
				scacCode.OK_CustomsRegNo = "OKSC";
				trip1.BH_OH_Carrier = carrier.PK;
				var shipment1 = trip1.Shipments.AddNew();
				shipment1.B0_Firms = "FIRM";
				shipment1.B0_ReferenceID = "Shipment1";
				shipment1.B0_ManifestQty = 3;
				shipment1.B0_ManifestUQ = "PK";
				shipment1.B0_Weight = 30;
				shipment1.B0_WeightUQ = "KG";
				shipment1.B0_DescriptionOfCargo = "DESC";
				var ship1Commodity = shipment1.Commodities[0];
				ship1Commodity.BY_InvoiceQuantity = 22m;
				ship1Commodity.BY_PieceCount = 11;
				ship1Commodity.BY_GrossWeight = 12m;
				ship1Commodity.BY_Description = "ship1comm1";
				var tariff = ship1Commodity.HarmonizedNumbers.AddNew();
				tariff.CY_TariffFormatted = "3920.99.1000";
				ship1Commodity.BY_HazardousGoodsIdentifier = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
				var undg = ship1Commodity.UNDGs.AddNew();
				undg.DI_DG = ship1Commodity.BY_HazardousGoodsIdentifier;
				var manufacturerParty = shipment1.Parties.AddNew();
				manufacturerParty.E2_AddressType = PartyTypes.Codes.ManufacturerOfGoods;
				manufacturerParty.OrganisationPK = manufacturer.PK;
				var shipToParty = shipment1.Parties.AddNew();
				shipToParty.E2_AddressType = PartyTypes.Codes.ShipTo;
				shipToParty.OrganisationPK = shipTo.PK;
				var shipment2 = trip1.Shipments.AddNew();
				shipment2.B0_Firms = "FIRM";
				shipment2.B0_ReferenceID = "Shipment2";
				shipment2.B0_ManifestQty = 2;
				shipment2.B0_ManifestUQ = "PK";
				shipment2.B0_Weight = 20;
				shipment2.B0_WeightUQ = "KG";
				shipment2.B0_DescriptionOfCargo = "DESC";
				var ship2Commodity = shipment2.Commodities[0];
				ship2Commodity.BY_InvoiceQuantity = 44m;
				ship2Commodity.BY_PieceCount = 44;
				ship2Commodity.BY_GrossWeight = 44m;
				ship2Commodity.BY_Description = "ship1comm1";
				ship2Commodity.BY_HazardousGoodsIdentifier = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
				var tariff2 = ship2Commodity.HarmonizedNumbers.AddNew();
				tariff2.CY_TariffFormatted = "3920.99.1002";
				var ship2Commodity2 = shipment2.Commodities.AddNew();
				ship2Commodity2.BY_InvoiceQuantity = 55m;
				ship2Commodity2.BY_PieceCount = 55;
				ship2Commodity2.BY_GrossWeight = 55m;
				ship2Commodity2.BY_Description = "ship1comm2";
				ship2Commodity2.BY_HazardousGoodsIdentifier = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "b", "IMO").First().PK;
				Factory.Save();
				var created = shipment2.CreateCustomsDeclaration(trip1);
				var declaration = Factory.Load<JobDeclaration>(created.PK);
				AssertEquals("Shipment type", US.Business.JobMessageTypeList.Codes.Import, declaration.JE_MessageType);
				AssertEquals("Transport", Core.Constants.TransportModes.Truck, declaration.JE_TransportMode);
				AssertNotNull(declaration);
				CombineAssertions(() =>
				{
					AssertEquals("Client -> Importer", trip1.Importer.Header.OH_Code, declaration.Importer.OH_Code);
					AssertEquals("Carrier Code -> Issuer and Carrier SCAC", trip1.BH_OH_Carrier, declaration.JE_OH_ShippingLine);
					AssertEquals("Trip Reference – Trip ID", trip1.BH_VoyageNumber, declaration.JE_VoyageFlightNo);
					AssertEquals("Carrier and Carrier SCAC ->  Transport> Carrier", trip1.BH_OH_Carrier, declaration.JE_OH_ShippingLine);
					AssertEquals("Estimated Date of Arrival - Departure Date", ZDateTime.Today, declaration.JE_ExportDate);
					AssertEquals("Estimated Date of Arrival - Arrival Date", trip1.BH_ETA, declaration.JE_DateOfArrival);
					AssertEquals("Estimated Date of Arrival - Date at port of Entry", trip1.BH_ETA, declaration.US_EntryDate);
					AssertEquals("Shipment Firm - Firm", shipment1.B0_Firms, declaration.US_US_NKLocationOfGoods);
					AssertEquals("Shipment Quantity UQ - Totoal Number of Package and UQ", shipment1.B0_ManifestQty + shipment2.B0_ManifestQty, declaration.JE_TotalNoOfPacks);
					AssertEquals("Shipment Quantity UQ - Package UQ", shipment1.B0_ManifestUQ, declaration.JE_TotalNoOfPacksPackType);
					AssertEquals("Shipment Weight UQ - Total Weight and UQ", shipment1.B0_Weight + shipment2.B0_Weight, declaration.JE_TotalWeight);
					AssertEquals("Shipment Weight UQ - UQ", shipment1.B0_WeightUQ, declaration.JE_TotalWeightUnit);
					var packLine = declaration.Bills.OfType<US.Business.Bill>().FirstOrDefault(x => x.CU_BillType == BillTypeList.Codes.MasterBill && !x.US_UI_NKBillIssuerSCAC.IsEmpty);
					AssertEquals("Trip Reference – Pack > Bill of Loading > Issuer SCAC", trip1.BH_CarrierSCAC, packLine.US_UI_NKBillIssuerSCAC);
					AssertEquals("Declaration SCAC", trip1.BH_CarrierSCAC, declaration.JE_MasterBillIssuerSCAC);
					AssertEquals("Declaration SCAC from carrier", trip1.BH_CarrierSCAC, declaration.US_UI_NKCarrierSCAC);
					var line = declaration.Invoices[0].InvoiceLines[1];
					AssertEquals("Packages1", ship2Commodity.BY_PieceCount, (int)line.JI_InvoiceQuantity);
					AssertEquals("Gross weight1", ship2Commodity.BY_GrossWeight, line.GrossWeightInKG);
					var line2 = declaration.Invoices[0].InvoiceLines[0];
					AssertEquals("Packages2", ship2Commodity2.BY_PieceCount, (int)line2.JI_InvoiceQuantity);
					AssertEquals("Gross weight2", ship2Commodity2.BY_GrossWeight, line2.GrossWeightInKG);
				});
			}
		}

		public void TestCreate2InvoiceLines()
		{
			var glbCompany = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany.GC_RN_NKCountryCode = "US";
			var branch = glbCompany.Branches.AddNew();
			branch.GB_GC = glbCompany.PK;
			var tripBO = Factory.NewWithValidTestData<Trip>();
			tripBO.BH_GB = branch.PK;
			tripBO.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.eManifest;
			tripBO.BH_VoyageNumber = "Voyage1";
			var shipment = tripBO.Shipments.AddNew();
			shipment.B0_Firms = "FIRM";
			shipment.B0_ReferenceID = "Shipment1";
			shipment.B0_ManifestQty = 3;
			shipment.B0_ManifestUQ = "PK";
			shipment.B0_DescriptionOfCargo = "DESC";
			var ship2Commodity = shipment.Commodities.AddNew();
			ship2Commodity.BY_InvoiceQuantity = 44m;
			ship2Commodity.BY_PieceCount = 44;
			ship2Commodity.BY_GrossWeight = 44m;
			ship2Commodity.BY_Description = "ship1comm1";
			ship2Commodity.BY_HazardousGoodsIdentifier = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			var tariff2 = ship2Commodity.HarmonizedNumbers.AddNew();
			tariff2.CY_TariffFormatted = "3920.99.1002";
			var ship2Commodity2 = shipment.Commodities.AddNew();
			ship2Commodity2.BY_InvoiceQuantity = 55m;
			ship2Commodity2.BY_PieceCount = 55;
			ship2Commodity2.BY_GrossWeight = 55m;
			ship2Commodity2.BY_Description = "ship1comm2";
			ship2Commodity2.BY_HazardousGoodsIdentifier = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "b", "IMO").First().PK;
			Factory.Save();
			var declaration = shipment.CreateCustomsDeclaration(tripBO);
			AssertEquals("Event Processed", tripBO.BH_VoyageNumber, declaration.JE_VoyageFlightNo);
			var header = declaration.Invoices[0];
			AssertEquals("Qty total", 99.00000m, header.InvoiceLines[0].JI_InvoiceQuantity + header.InvoiceLines[1].JI_InvoiceQuantity);
		}

		public void TestCreateShipmentForDeclaration()
		{
			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer.OH_Code = "MFG";
			var shipTo = Factory.NewWithValidTestData<OrgHeader>();
			shipTo.OH_Code = "SST";
			var glbCompany = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany.GC_RN_NKCountryCode = "US";
			var branch = glbCompany.Branches.AddNew();
			branch.GB_GC = glbCompany.PK;
			branch.GB_RN_NKCountryCode = "US";
			Factory.Save();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var trip1 = Factory.NewWithValidTestData<Trip>();
				trip1.BH_GB = branch.PK;
				trip1.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.eManifest;
				trip1.BH_VoyageNumber = "Voyage1";
				trip1.BH_PortUnladingDCode = "2704";
				trip1.BH_RL_NKPortUnlading = "USBUF";
				trip1.BH_ETA = ZDateTime.Today;
				trip1.BH_CarrierSCAC = "OKSC";
				var importer = Factory.NewWithValidTestData<OrgAddress>();
				importer.Header.OH_Code = "IMPORTER1";
				trip1.BH_OA_Importer = importer.PK;
				trip1.BH_TransitDirection = "I";
				var carrier = Factory.NewWithValidTestData<OrgHeader>();
				carrier.OH_Code = "CARRIER1";
				var scacCode = carrier.CustomsCodes.AddNew();
				scacCode.OK_CodeType = "CCC";
				scacCode.OK_CustomsRegNo = "OKSC";
				trip1.BH_OH_Carrier = carrier.PK;
				var shipment1 = trip1.Shipments.AddNew();
				shipment1.B0_Firms = "FIRM";
				shipment1.B0_ReferenceID = "Shipment1";
				shipment1.B0_ManifestQty = 3;
				shipment1.B0_ManifestUQ = "PK";
				shipment1.B0_Weight = 30;
				shipment1.B0_WeightUQ = "KG";
				shipment1.B0_DescriptionOfCargo = "DESC";
				var ship1Commodity = shipment1.Commodities.AddNew();
				ship1Commodity.BY_InvoiceQuantity = 22m;
				ship1Commodity.BY_PieceCount = 11;
				ship1Commodity.BY_GrossWeight = 12m;
				ship1Commodity.BY_Description = "ship1comm1";
				var tariff = ship1Commodity.HarmonizedNumbers.AddNew();
				tariff.CY_TariffFormatted = "3920.99.1000";
				ship1Commodity.BY_HazardousGoodsIdentifier = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
				var undg = ship1Commodity.UNDGs.AddNew();
				undg.DI_DG = ship1Commodity.BY_HazardousGoodsIdentifier;
				var manufacturerParty = shipment1.Parties.AddNew();
				manufacturerParty.E2_AddressType = PartyTypes.Codes.ManufacturerOfGoods;
				manufacturerParty.OrganisationPK = manufacturer.PK;
				var shipToParty = shipment1.Parties.AddNew();
				shipToParty.E2_AddressType = PartyTypes.Codes.ShipTo;
				shipToParty.OrganisationPK = shipTo.PK;
				Factory.Save();
				var created = shipment1.CreateCustomsDeclaration(trip1);
				var declaration = Factory.Load<JobDeclaration>(created.PK);
				AssertEquals("Shipment type", US.Business.JobMessageTypeList.Codes.Import, declaration.JE_MessageType);
				AssertEquals("Transport", Core.Constants.TransportModes.Truck, declaration.JE_TransportMode);
				AssertNotNull(declaration);
				CombineAssertions(() =>
				{
					AssertEquals("Client -> Importer", trip1.Importer.Header.OH_Code, declaration.Importer.OH_Code);
					AssertEquals("Carrier Code -> Issuer and Carrier SCAC", trip1.BH_OH_Carrier, declaration.JE_OH_ShippingLine);
					AssertEquals("Trip Reference – Trip ID", trip1.BH_VoyageNumber, declaration.JE_VoyageFlightNo);
					AssertEquals("Carrier and Carrier SCAC ->  Transport> Carrier", trip1.BH_OH_Carrier, declaration.JE_OH_ShippingLine);
					AssertEquals("Estimated Date of Arrival - Departure Date", trip1.BH_ETA, declaration.JE_ExportDate);
					AssertEquals("Estimated Date of Arrival - Arrival Date", trip1.BH_ETA, declaration.JE_DateOfArrival);
					AssertEquals("Estimated Date of Arrival - Date at port of Entry", trip1.BH_ETA, declaration.US_EntryDate);
					AssertEquals("Shipment Firm - Firm", shipment1.B0_Firms, declaration.US_US_NKLocationOfGoods);
					AssertEquals("Shipment Quantity UQ - Totoal Number of Package and UQ", shipment1.B0_ManifestQty, declaration.JE_TotalNoOfPacks);
					AssertEquals("Shipment Quantity UQ - Package UQ", shipment1.B0_ManifestUQ, declaration.JE_TotalNoOfPacksPackType);
					AssertEquals("Shipment Weight UQ - Total Weight and UQ", shipment1.B0_Weight, declaration.JE_TotalWeight);
					AssertEquals("Shipment Weight UQ - UQ", shipment1.B0_WeightUQ, declaration.JE_TotalWeightUnit);
					var packLine = declaration.Bills.OfType<US.Business.Bill>().FirstOrDefault(x => x.CU_BillType == BillTypeList.Codes.MasterBill && !x.US_UI_NKBillIssuerSCAC.IsEmpty);
					AssertEquals("Trip Reference – Pack > Bill of Loading > Issuer SCAC", trip1.BH_CarrierSCAC, packLine.US_UI_NKBillIssuerSCAC);
					AssertEquals("Declaration SCAC", trip1.BH_CarrierSCAC, declaration.JE_MasterBillIssuerSCAC);
					AssertEquals("Declaration SCAC from carrier", trip1.BH_CarrierSCAC, declaration.US_UI_NKCarrierSCAC);
					AssertEquals("InvoiceHeader", 1, declaration.Invoices.Count);
					var header = declaration.Invoices[0];
					AssertEquals("Manufacturer", manufacturerParty.OrganisationPK, header.JZ_OA_ManufacturerAddress_ZAddress.OrgPK);
					AssertEquals("ShipToParty", shipToParty.OrganisationPK, header.ShipToPartyOrgPK);
					AssertEquals("Invoice Line", 2, declaration.Invoices[0].InvoiceLines.Count);
					var line = declaration.Invoices[0].InvoiceLines[0];
					AssertEquals("Packages", ship1Commodity.BY_PieceCount, (int)line.JI_InvoiceQuantity);
					AssertEquals("Gross weight", ship1Commodity.BY_GrossWeight, line.GrossWeightInKG);
					AssertEquals("BY_Description", ship1Commodity.BY_Description, line.JI_Description);
					AssertNotNull(line.UNDGs);
					AssertEquals("UNDG Code", ship1Commodity.UNDGs[0].Substance.DG_Code, line.UNDGs[0].Substance?.DG_Code);
					AssertEquals("Tariff", "3920.99.1000", line.JI_FormattedTariff);
				});
			}
		}

		public void TestCopyeDocsFiles()
		{
			var trip1 = Factory.NewWithValidTestData<Trip>();
			SetupTrip(trip1);
			CreateEdocsRow(trip1.PK.ToGuid());
			AssertEquals("one eDocs row is created", 1, ((IDocManagerSupport)trip1).DocManagerInfo.AllEDocs.Count);
			var declaration = trip1.Shipments[0].CreateCustomsDeclaration(trip1);
			AssertEquals(1, declaration.DocManagerInfo.Files.Count);
		}

		void CreateEdocsRow(Guid parentPK)
		{
			// don't have access to the bizos here, so use sql to create
			Guid gSM_PK = Guid.NewGuid();
			Guid gSC_PK = Guid.NewGuid();
			byte[] imageData = new byte[] { 1, 2, 3, 4 }; //System.IO.File.ReadAllBytes(SamplePdfPath);

			string cmdString = "INSERT INTO " + StorageMainSchema.Constants.SqlSchemaName + "." + StorageMainSchema.Constants.TableName
				+ " (" + StorageMainSchema.Constants.PK + ", "
				+ StorageMainSchema.Constants.SM_Type + ", "
				+ StorageMainSchema.Constants.SM_ParentFK + ", "
				+ StorageMainSchema.Constants.SM_DB + " ) "
				+ @" VALUES 
				(@SM_PK, 
				@SM_Type, 
				@SM_ParentFK,
				@SM_DB)";
			var cmd = CargoWise.Data.Db.Connection.Command(cmdString); // don't have access to BizOs here (they're built after us), use SQL to create
			cmd.AddParameterBasedOnDbColumn("@SM_PK", gSM_PK, StorageMainSchema.PK);
			cmd.AddParameterBasedOnDbColumn("@SM_Type", "ORG", StorageMainSchema.SM_Type);
			cmd.AddParameterBasedOnDbColumn("@SM_ParentFK", parentPK, StorageMainSchema.SM_ParentFK);
			cmd.AddParameterBasedOnDbColumn("@SM_DB", 1, StorageMainSchema.SM_DB);
			cmd.ExecuteNonQuery();

			cmdString = string.Format("INSERT INTO {0}_SD001..", Db.DatabaseName.Trim()) + StorageDocsSchema.Constants.TableName
				+ " (" + StorageDocsSchema.Constants.PK + ", "
				+ StorageDocsSchema.Constants.SC_Date + ", "
				+ StorageDocsSchema.Constants.SC_SystemCreateTimeUtc + ", "
				+ StorageDocsSchema.Constants.SC_SystemLastEditTimeUtc + ", "
				+ StorageDocsSchema.Constants.SC_DocType + ", "
				+ StorageDocsSchema.Constants.SC_Desc + ", "
				+ StorageDocsSchema.Constants.SC_FileName + ", "
				+ StorageDocsSchema.Constants.SC_SM + ", "
				+ StorageDocsSchema.Constants.SC_ImageData + ", "
				+ StorageDocsSchema.Constants.SC_IsSystemGenerated + ", "
				+ StorageDocsSchema.Constants.SC_DataType + " ) "
				+ @" VALUES 
				(@SC_PK, 
				getdate(), 
				getdate(), 
				getdate(), 
				@SC_DocType, 
				@SC_Desc,
				@SC_FileName,
				@SC_SM, 
				@SC_ImageData,
				@SC_IsSystemGenerated,
				@SC_DataType)";
			cmd = CargoWise.Data.Db.Connection.Command(cmdString); // don't have access to BizOs here (they're built after us), use SQL to create
			cmd.AddParameterBasedOnDbColumn("@SC_PK", gSC_PK, StorageDocsSchema.PK);
			cmd.AddParameterBasedOnDbColumn("@SC_DocType", Core.Constants.RefDocTypes.MiscellaneousDocument, StorageDocsSchema.SC_DocType);
			cmd.AddParameterBasedOnDbColumn("@SC_Desc", "MISC TEST", StorageDocsSchema.SC_Desc);
			cmd.AddParameterBasedOnDbColumn("@SC_FileName", "FileName", StorageDocsSchema.SC_FileName);
			cmd.AddParameterBasedOnDbColumn("@SC_SM", gSM_PK, StorageDocsSchema.SC_SM);
			cmd.AddParameterBasedOnDbColumn("@SC_ImageData", imageData, StorageDocsSchema.SC_ImageData);
			cmd.AddParameterBasedOnDbColumn("@SC_IsSystemGenerated", "Y", StorageDocsSchema.SC_IsSystemGenerated);
			cmd.AddParameterBasedOnDbColumn("@SC_DataType", "XML", StorageDocsSchema.SC_DataType);
			cmd.ExecuteNonQuery();
		}

		void SetupTrip(Trip trip1)
		{
			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer.OH_Code = "MFG";
			var shipTo = Factory.NewWithValidTestData<OrgHeader>();
			shipTo.OH_Code = "SST";
			var glbCompany = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany.GC_RN_NKCountryCode = "US";
			var branch = glbCompany.Branches.AddNew();
			branch.GB_GC = glbCompany.PK;
			branch.GB_RN_NKCountryCode = "US";
			trip1.BH_GB = branch.PK;
			trip1.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.eManifest;
			trip1.BH_VoyageNumber = "Voyage1";
			trip1.BH_PortUnladingDCode = "1003";
			trip1.BH_ETA = ZDateTime.Today;
			var impoter = Factory.NewWithValidTestData<OrgAddress>();
			impoter.Header.OH_Code = "IMPORTER1";
			trip1.BH_OA_Importer = impoter.PK;
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "CARRIER1";
			var scacCode = carrier.CustomsCodes.AddNew();
			scacCode.OK_CodeType = "CCC";
			scacCode.OK_CustomsRegNo = "OKSC";
			trip1.BH_OH_Carrier = carrier.PK;
			var shipment1 = trip1.Shipments.AddNew();
			shipment1.B0_Firms = "FIRM";
			shipment1.B0_ReferenceID = "Shipment1";
			shipment1.B0_ManifestQty = 3;
			shipment1.B0_ManifestUQ = "BOL";
			shipment1.B0_Weight = 30;
			shipment1.B0_WeightUQ = "KG";
			shipment1.B0_DescriptionOfCargo = "DESC";
			var ship1Commodity = shipment1.Commodities.AddNew();
			ship1Commodity.BY_InvoiceQuantity = 22;
			ship1Commodity.BY_PieceCount = 11;
			ship1Commodity.BY_GrossWeight = 12;
			ship1Commodity.BY_Description = "ship1comm1";
			var tariff = ship1Commodity.HarmonizedNumbers.AddNew();
			tariff.CY_TariffFormatted = "3920.99.1000";
			ship1Commodity.BY_HazardousGoodsIdentifier = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			var undg = ship1Commodity.UNDGs.AddNew();
			undg.DI_DG = ship1Commodity.BY_HazardousGoodsIdentifier;
			var manufacturerParty = shipment1.Parties.AddNew();
			manufacturerParty.E2_AddressType = PartyTypes.Codes.ManufacturerOfGoods;
			manufacturerParty.OrganisationPK = manufacturer.PK;
			var shipToParty = shipment1.Parties.AddNew();
			shipToParty.E2_AddressType = PartyTypes.Codes.ShipTo;
			shipToParty.OrganisationPK = shipTo.PK;
			Factory.Save();
		}
	}
}

using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ACEBIRDVNEDataProcessorTest : ACEBIRDCommonPGADataProcessorTest
	{
		public override void TestEndToEndProcessDisclaimedPGA()
		{
			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_VNEDisclaimReason = PGADisclaimReasonList.Codes.A;
			invoiceLine.JI_Description = "TEST VNE DISCLAIMED";
			Factory.Save();

			var action = GetAction(declaration);
			action.US_DateOfDeclaration = ZDateTime.Today;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			ImportMessageBlocks(message);
			AssertEquals(1, DeclarationImported.InvoiceLines.Count);

			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			AssertEquals(OGAIndicatorList.Codes.Disclaimed, invoiceLineImported.US_VNEInd);
			AssertEquals(PGADisclaimReasonList.Codes.A, invoiceLineImported.US_VNEDisclaimReason);
			AssertEquals("TEST VNE DISCLAIMED", invoiceLineImported.JI_Description);
		}

		public void TestBothVehicleAndEngineDetailsSent()
		{
			invoiceLine.JI_Description = "TOYOTA SEDANS";
			var vehicleLine = invoiceLine.VehicleLines.AddNew();
			vehicleLine.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_1;
			vehicleLine.US_ModelYear = "2014";
			vehicleLine.US_VehicleModel = "PRIUS";
			vehicleLine.US_ImportCode = ImportCodesForm3520_1List.Codes.A;

			var vehicleDetails = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails.US_BuildMonth = MonthList.Codes._12;
			vehicleDetails.US_BuildYear = "2013";
			vehicleDetails.US_IdentityNumberQualifier = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			vehicleDetails.US_IdentityNumber = "JTDBBADU5B0865DK0";
			vehicleDetails.US_VehicleManufacturer = "TEST VEHICLE MANUFACTURER";
			vehicleDetails.US_EngineNumber = "MHO-874200";
			vehicleDetails.US_EngineBuildDate = new ZDateTime(2013, 12, 1);
			vehicleDetails.US_EngineModel = "EM1000";
			vehicleDetails.US_EngineManufacturer = "TEST ENGINE MANUFACTURER";
			vehicleDetails.US_MfrDateType = ManufactureDateTypeList.Codes.ENG;
			var vehicleDetails2 = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails2.US_IdentityNumber = "JTDBBADU5B1865DK1";
			vehicleDetails2.US_EngineNumber = "MHO-874201";
			var vehicleDetails3 = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails3.US_IdentityNumber = "JTDBBADU5B1865DK2";
			vehicleDetails3.US_EngineNumber = "MHO-874202";

			Factory.Save();

			var action = GetAction(declaration);
			action.US_DateOfDeclaration = ZDateTime.Today;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			ImportMessageBlocks(message);
			AssertEquals(1, DeclarationImported.InvoiceLines.Count);

			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			AssertEquals("TOYOTA SEDANS", invoiceLineImported.JI_Description);
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLineImported.US_VNEInd);
			AssertEquals(1, invoiceLineImported.VehicleLines.Count);

			var vneLineImported = invoiceLineImported.VehicleLines[0];
			AssertEquals(EPAVNEDocumentIdentifierList.Codes.EPA3520_1, vneLineImported.US_FormType);
			AssertEquals("2014", vneLineImported.US_ModelYear);
			AssertEquals("PRIUS", vneLineImported.US_VehicleModel);
			AssertEquals(ImportCodesForm3520_1List.Codes.A, vneLineImported.US_ImportCode);
			AssertEquals(3, vneLineImported.VehicleAndEngineDetails.Count);

			var vehicleDetails0Imported = vneLineImported.VehicleAndEngineDetails.OfType<VehicleDetails>().FirstOrDefault(x => x.US_IdentityNumber == "JTDBBADU5B0865DK0");
			AssertEquals(MonthList.Codes._12, vehicleDetails0Imported.US_BuildMonth);
			AssertEquals("2013", vehicleDetails0Imported.US_BuildYear);
			AssertEquals(ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN, vehicleDetails0Imported.US_IdentityNumberQualifier);
			AssertEquals("TEST VEHICLE MANUFACTURER", vehicleDetails0Imported.US_VehicleManufacturer);
			AssertEquals("MHO-874200", vehicleDetails0Imported.US_EngineNumber);
			AssertEquals(new ZDateTime(2013, 12, 1), vehicleDetails0Imported.US_EngineBuildDate);
			AssertEquals("EM1000", vehicleDetails0Imported.US_EngineModel);
			AssertEquals("TEST ENGINE MANUFACTURER", vehicleDetails0Imported.US_EngineManufacturer);
			AssertEquals(ManufactureDateTypeList.Codes.ENG, vehicleDetails0Imported.US_MfrDateType);
			var vehicleDetails1Imported = vneLineImported.VehicleAndEngineDetails.OfType<VehicleDetails>().FirstOrDefault(x => x.US_IdentityNumber == "JTDBBADU5B1865DK1");
			AssertNotNull(vehicleDetails1Imported);
			AssertEquals("MHO-874201", vehicleDetails1Imported.US_EngineNumber);
			var vehicleDetails2Imported = vneLineImported.VehicleAndEngineDetails.OfType<VehicleDetails>().FirstOrDefault(x => x.US_IdentityNumber == "JTDBBADU5B1865DK2");
			AssertNotNull(vehicleDetails2Imported);
			AssertEquals("MHO-874202", vehicleDetails2Imported.US_EngineNumber);
		}

		protected override MQEDIMessage GetEDIMessageForEndToEndTest()
		{
			declaration.US_FDAContactName = "PGA CONTACT FOR TEST";
			declaration.US_FDAContactEmail = "TEST@ABC.COM";
			declaration.US_FDAContactPhoneNo = "091245022";
			invoiceLine.JI_Description = "TOYOTA SEDANS";
			var vehicleLine = invoiceLine.VehicleLines.AddNew();
			vehicleLine.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_1;
			vehicleLine.US_OA_Owner = manufacturer.MainAddress.PK;
			vehicleLine.US_OA_StorageLocation = importer.MainAddress.PK;
			vehicleLine.US_ModelYear = "2014";
			vehicleLine.US_VehicleModel = "PRIUS";
			vehicleLine.US_ImportCode = ImportCodesForm3520_1List.Codes.A;
			vehicleLine.US_CertOfConformity = "9EPAV01.0ABC";
			vehicleLine.US_CertOfConformityExpiryDate = new ZDateTime(2020, 12, 31);
			vehicleLine.US_CertifyingIndividual = PartyTypeList.Codes.CustomsBroker;
			vehicleLine.US_VNEElectronicImage = true;

			var vehicleDetails = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails.US_BuildMonth = MonthList.Codes._12;
			vehicleDetails.US_BuildYear = "2013";
			vehicleDetails.US_IdentityNumberQualifier = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			vehicleDetails.US_IdentityNumber = "JTDBBADU5B0865DK0";
			var vehicleDetails2 = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails2.US_IdentityNumber = "JTDBBADU5B1865DK1";
			var vehicleDetails3 = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails3.US_IdentityNumber = "JTDBBADU5B1865DK2";
			var vehicleDetails4 = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails4.US_IdentityNumber = "JTDBBADU5B1865DK3";
			var vehicleDetails5 = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails5.US_IdentityNumber = "JTDBBADU5B1865DK4";

			Factory.Save();

			var action = GetAction(declaration);
			action.US_DateOfDeclaration = ZDateTime.Today;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			return builder.PopulateMessage();
		}

		protected override void AssertEndToEndTestResult()
		{
			AssertEquals("PGA CONTACT FOR TEST", DeclarationImported.US_FDAContactName);
			AssertEquals("TEST@ABC.COM", DeclarationImported.US_FDAContactEmail);
			AssertEquals("091245022", DeclarationImported.US_FDAContactPhoneNo);
			AssertEquals(1, DeclarationImported.InvoiceLines.Count);

			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			AssertEquals("TOYOTA SEDANS", invoiceLineImported.JI_Description);
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLineImported.US_VNEInd);
			AssertEquals(1, invoiceLineImported.VehicleLines.Count);

			CombineAssertions(() =>
			{
				var vneLineImported = invoiceLineImported.VehicleLines[0];
				AssertEquals(EPAVNEDocumentIdentifierList.Codes.EPA3520_1, vneLineImported.US_FormType);
				AssertEquals(manufacturer.MainAddress.PK, vneLineImported.US_OA_Owner);
				AssertEquals(importer.MainAddress.PK, vneLineImported.US_OA_StorageLocation);
				AssertEquals("2014", vneLineImported.US_ModelYear);
				AssertEquals("PRIUS", vneLineImported.US_VehicleModel);
				AssertEquals(ImportCodesForm3520_1List.Codes.A, vneLineImported.US_ImportCode);
				AssertEquals("9EPAV01.0ABC", vneLineImported.US_CertOfConformity);
				AssertEquals(new ZDateTime(2020, 12, 31), vneLineImported.US_CertOfConformityExpiryDate);
				AssertEquals(PartyTypeList.Codes.CustomsBroker, vneLineImported.US_CertifyingIndividual);
				AssertEquals(true, vneLineImported.US_VNEElectronicImage);
				AssertEquals(5, vneLineImported.VehicleAndEngineDetails.Count);

				var vehicleDetails0Imported = vneLineImported.VehicleAndEngineDetails.OfType<VehicleDetails>().FirstOrDefault(x => x.US_IdentityNumber == "JTDBBADU5B0865DK0");
				AssertEquals(MonthList.Codes._12, vehicleDetails0Imported.US_BuildMonth);
				AssertEquals("2013", vehicleDetails0Imported.US_BuildYear);
				AssertEquals(ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN, vehicleDetails0Imported.US_IdentityNumberQualifier);
				var vehicleDetails1Imported = vneLineImported.VehicleAndEngineDetails.OfType<VehicleDetails>().FirstOrDefault(x => x.US_IdentityNumber == "JTDBBADU5B1865DK1");
				AssertNotNull(vehicleDetails1Imported);
				var vehicleDetails2Imported = vneLineImported.VehicleAndEngineDetails.OfType<VehicleDetails>().FirstOrDefault(x => x.US_IdentityNumber == "JTDBBADU5B1865DK2");
				AssertNotNull(vehicleDetails2Imported);
				var vehicleDetails3Imported = vneLineImported.VehicleAndEngineDetails.OfType<VehicleDetails>().FirstOrDefault(x => x.US_IdentityNumber == "JTDBBADU5B1865DK3");
				AssertNotNull(vehicleDetails3Imported);
				var vehicleDetails4Imported = vneLineImported.VehicleAndEngineDetails.OfType<VehicleDetails>().FirstOrDefault(x => x.US_IdentityNumber == "JTDBBADU5B1865DK4");
				AssertNotNull(vehicleDetails4Imported);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Declared;

			manufacturer = Factory.New<OrgHeader>();
			manufacturer.OH_Code = "TESTMAN";
			manufacturer.OH_FullName = "TOYOTA (JAPAN)";
			var address = manufacturer.MainAddress;
			address.OA_Address1 = "1234 PEACHTREE STREET";
			address.OA_City = "ATLANTA";
			address.OA_State = "GA";
			address.OA_RL_NKRelatedPortCode = "USLAX";
			address.OA_PostCode = "30301";
			DeclarationTestHelper.AddPGAContact(manufacturer, "JANE", "SMITH", "7062345678", "J.SMITH@TOYOTAAMERICA.COM", null);

			importer = Factory.New<OrgHeader>();
			importer.OH_Code = "TESTIMP";
			importer.OH_FullName = "TEST IMPORTER";
			importer.MainAddress.OA_Address1 = "IMPORTER STREET 1";
			importer.MainAddress.OA_Address2 = "IMPORTER STREET 2";
			importer.MainAddress.OA_Email = "importer@test.com";
			importer.MainAddress.OA_Phone = "6934568700";
			importer.MainAddress.OA_Fax = "";
			importer.MainAddress.OA_PostCode = "96358";
			importer.MainAddress.OA_RL_NKRelatedPortCode = "USLAX";
			importer.MainAddress.OA_City = "LOS ANGELES";
			DeclarationTestHelper.AddPGAContact(importer, "JANE", "SMITH", "7062345678", "J.SMITH@TOYOTAAMERICA.COM", null);
		}
		OrgHeader manufacturer;
		OrgHeader importer;
	}
}

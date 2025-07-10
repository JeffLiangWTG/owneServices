using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ACEBIRDFWSDataProcessorTest : ACEBIRDCommonPGADataProcessorTest
	{
		public override void TestEndToEndProcessDisclaimedPGA()
		{
			invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_FWSDisclaimReason = PGADisclaimReasonList.Codes.C;
			invoiceLine.JI_Description = "TEST FWS DISCLAIMED";
			Factory.Save();

			var action = GetAction(declaration);
			action.US_AcknowledgeAndSign = true;
			action.US_DateOfDeclaration = new ZDateTime(2016, 07, 20);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			ImportMessageBlocks(message);
			AssertEquals(1, DeclarationImported.InvoiceLines.Count);

			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			AssertEquals(OGAIndicatorList.Codes.Disclaimed, invoiceLineImported.US_FWSInd);
			AssertEquals(PGADisclaimReasonList.Codes.C, invoiceLineImported.US_FWSDisclaimReason);
			AssertEquals("TEST FWS DISCLAIMED", invoiceLineImported.JI_Description);
		}

		protected override MQEDIMessage GetEDIMessageForEndToEndTest()
		{
			invoiceLine.JI_Tariff = "0304590003";
			invoiceLine.JI_Description = "SCALLOPED HAMMERHEAD SHARK";

			var header = invoiceLine.FWSHeaders.AddNew();
			header.US_ProcessingCode = FWSProcessingCodeList.Codes.EDS;
			header.US_IsDocSubmitted = ZBool.True;
			header.US_IntendedUseCode = IntendedUseCodesList.Codes.ForBreedingInCaptivityOrArtificialPropagation;
			header.US_SpeciesOrigin = Core.Constants.CountryCodes.HongKong;
			header.US_FIRMS = "B815";
			header.US_OA_FWSImporterAddress = ImporterAddress.PK;
			header.US_OA_FWSExporterAddress = ExporterAddress.PK;
			header.US_WildlifeDescriptionCode = FWSWildlifeDescriptionCodesList.Codes.LIV;
			header.US_WildlifeSource = FWSWildlifeSourceList.Codes.R;
			header.US_CommoditySpecificName = "SPECIFIC NAME";
			header.US_CommodityGeneralName = "DOG";
			header.US_InvCurrPGAValue = 1000m;
			header.US_CartonQty = 10;
			header.US_NetCommodity = 10m;
			header.US_NetCommodityUQ = FWSUnitOfMeasureList.Codes.NumberIndividualUnits;
			header.US_ScientificGenusName = "CANIS";
			header.US_ScientificSpeciesName = "FAMILIARIS";

			var license1 = header.Licenses.AddNew();
			license1.US_Type = FWSLicenseTypeList.Codes.ForeignWildlifeExportDocument;
			license1.US_Number = "382332KD";
			var license2 = header.Licenses.AddNew();
			license2.US_Type = FWSLicenseTypeList.Codes.FWSImportExportLicense;
			license2.US_Number = "83455333";
			Factory.Save();

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			return builder.PopulateMessage();
		}

		protected override void AssertEndToEndTestResult()
		{
			AssertEquals(1, DeclarationImported.InvoiceLines.Count);

			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLineImported.US_FWSInd);
			AssertEquals("SCALLOPED HAMMERHEAD SHARK", invoiceLineImported.JI_Description);
			AssertEquals(1, invoiceLineImported.FWSHeaders.Count);

			var fwsHeaderImported = invoiceLineImported.FWSHeaders[0];
			CombineAssertions(() =>
			{
				AssertEquals("header.US_ProcessingCode", FWSProcessingCodeList.Codes.EDS, fwsHeaderImported.US_ProcessingCode);
				AssertEquals("header.US_IsDocSubmitted", true, fwsHeaderImported.US_IsDocSubmitted);
				AssertEquals("header.US_IntendedUseCode", IntendedUseCodesList.Codes.ForBreedingInCaptivityOrArtificialPropagation, fwsHeaderImported.US_IntendedUseCode);
				AssertEquals("header.US_SpeciesOrigin", Core.Constants.CountryCodes.HongKong, fwsHeaderImported.US_SpeciesOrigin);
				AssertEquals("header.US_FIRMS", "B815", fwsHeaderImported.US_FIRMS);
				AssertEquals("header.US_OA_FWSImporterAddress", ImporterAddress.PK, fwsHeaderImported.US_OA_FWSImporterAddress);
				AssertEquals("header.US_OA_FWSExporterAddress", ExporterAddress.PK, fwsHeaderImported.US_OA_FWSExporterAddress);
				AssertEquals("header.US_WildlifeDescriptionCode", FWSWildlifeDescriptionCodesList.Codes.LIV, fwsHeaderImported.US_WildlifeDescriptionCode);
				AssertEquals("header.US_WildlifeSource", FWSWildlifeSourceList.Codes.R, fwsHeaderImported.US_WildlifeSource);
				AssertEquals("header.US_CommoditySpecificName", "SPECIFIC NAME", fwsHeaderImported.US_CommoditySpecificName);
				AssertEquals("header.US_CommodityGeneralName", "DOG", fwsHeaderImported.US_CommodityGeneralName);
				AssertEquals("header.US_CartonQty", (ZShort)10, fwsHeaderImported.US_CartonQty);
				AssertEquals("header.US_NetCommodity", 10m, fwsHeaderImported.US_NetCommodity);
				AssertEquals("header.US_NetCommodityUQ", FWSUnitOfMeasureList.Codes.NumberIndividualUnits, fwsHeaderImported.US_NetCommodityUQ);
				AssertEquals("header.US_ScientificGenusName", "CANIS", fwsHeaderImported.US_ScientificGenusName);
				AssertEquals("header.US_ScientificSpeciesName", "FAMILIARIS", fwsHeaderImported.US_ScientificSpeciesName);
				AssertEquals("header.Licenses.Count", 2, fwsHeaderImported.Licenses.Count);

				var license0Imported = fwsHeaderImported.Licenses[0];
				AssertEquals("license1.US_Type", FWSLicenseTypeList.Codes.ForeignWildlifeExportDocument, license0Imported.US_Type);
				AssertEquals("license1.US_Number", "382332KD", license0Imported.US_Number);
				var license1Imported = fwsHeaderImported.Licenses[1];
				AssertEquals("license2.US_Type", FWSLicenseTypeList.Codes.FWSImportExportLicense, license1Imported.US_Type);
				AssertEquals("license2.US_Number", "83455333", license1Imported.US_Number);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Declared;
		}

		OrgAddress ExporterAddress
		{
			get
			{
				if (exporterAddress == null)
				{
					var org = Factory.New<OrgHeader>();
					org.OH_Code = "TESTEXP";
					org.OH_FullName = "FWS EXPORTER";
					org.OH_RL_NKClosestPort = "AUSYD";
					exporterAddress = org.MainAddress;
					exporterAddress.OA_Address1 = "EXP ADDRESS 1";
					exporterAddress.OA_Address2 = "EXP ADDRESS 2";
					exporterAddress.OA_RL_NKRelatedPortCode = "AUSYD";
					exporterAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "DUNS87433", Core.Constants.CountryCodes.UnitedStates);
				}
				return exporterAddress;
			}
		}
		OrgAddress exporterAddress;

		OrgAddress ImporterAddress
		{
			get
			{
				if (importerAddress == null)
				{
					var org = Factory.New<OrgHeader>();
					org.OH_Code = "TESTIMP";
					org.OH_FullName = "FWS IMPORTER";
					org.OH_RL_NKClosestPort = "USCHI";
					importerAddress = org.MainAddress;
					importerAddress.OA_Address1 = "IMP ADDRESS 1";
					importerAddress.OA_Address2 = "IMP ADDRESS 2";
					importerAddress.OA_RL_NKRelatedPortCode = "USCHI";
					importerAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "DUNS132123", Core.Constants.CountryCodes.UnitedStates);
				}
				return importerAddress;
			}
		}
		OrgAddress importerAddress;
	}
}

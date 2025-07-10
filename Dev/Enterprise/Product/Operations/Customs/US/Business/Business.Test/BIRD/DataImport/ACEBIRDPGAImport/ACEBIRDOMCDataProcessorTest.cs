using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ACEBIRDOMCDataProcessorTest : ACEBIRDCommonPGADataProcessorTest
	{
		public override void TestEndToEndProcessDisclaimedPGA()
		{
			invoiceLine.US_OMCInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_OMCDisclaimReason = PGADisclaimReasonList.Codes.A;
			invoiceLine.JI_Description = "TEST OMC DISCLAIMED";
			Factory.Save();

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			ImportMessageBlocks(message);
			AssertEquals(1, DeclarationImported.InvoiceLines.Count);

			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			CombineAssertions(delegate
			{
				AssertEquals(OGAIndicatorList.Codes.Disclaimed, invoiceLineImported.US_OMCInd);
				AssertEquals(PGADisclaimReasonList.Codes.A, invoiceLineImported.US_OMCDisclaimReason);
				AssertEquals("TEST OMC DISCLAIMED", invoiceLineImported.JI_Description);
			});
		}

		protected override MQEDIMessage GetEDIMessageForEndToEndTest()
		{
			invoiceLine.JI_Description = "TEST OMC DESC";
			var omcHeader = invoiceLine.OMCHeaders.AddNew();
			omcHeader.US_SourceCountry = Core.Constants.CountryCodes.Mexico;
			omcHeader.US_DepartureDate = ZDateTime.BrettsBirthday;
			omcHeader.US_ElectronicImageSubmitted = true;
			omcHeader.US_DeclarationCode = "7A1";
			omcHeader.US_OA_AquacultureFacility = aquacultureAddress.PK;
			omcHeader.US_OA_Exporter = exporterAddress.PK;
			omcHeader.US_ExporterPGAContactName = "EXPORTER NAME";
			omcHeader.US_ExporterPGAContactPhoneNo = "123123131";
			omcHeader.US_ExporterPGAContactEmail = "EXPORTER@ABC.COM";
			omcHeader.US_ExporterCertificationDate = new ZDateTime(2016, 12, 13);
			omcHeader.US_OA_ResponsibleGovernmentOfficial = governmentAddress.PK;
			omcHeader.US_OfficialPGAContactName = "OFFICIAL NAME";
			omcHeader.US_OfficialPGAContactPhoneNo = "789789789";
			omcHeader.US_OfficialPGAContactEmail = "OFFICIAL@ABC.COM";
			omcHeader.US_OfficialCertificationDate = new ZDateTime(2016, 12, 31);
			omcHeader.US_NetWeight = 100m;
			omcHeader.US_NetWeightUQ = "KG";
			var additionalFacility = omcHeader.AquacultureFacilities.AddNew();
			additionalFacility.US_OA_AquacultureFacility = additionalAddress.PK;

			Factory.Save();

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			return builder.PopulateMessage();
		}

		protected override void AssertEndToEndTestResult()
		{
			AssertEquals(1, DeclarationImported.InvoiceLines.Count);

			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			CombineAssertions(delegate
			{
				AssertEquals("invoiceLineImported.US_OMCInd", OGAIndicatorList.Codes.Declared, invoiceLineImported.US_OMCInd);
				AssertEquals("invoiceLineImported.JI_Description", "TEST OMC DESC", invoiceLineImported.JI_Description);
				AssertEquals("invoiceLineImported.OMCHeaders.Count", 1, invoiceLineImported.OMCHeaders.Count);

				var omcHeaderImported = invoiceLineImported.OMCHeaders[0];
				AssertEquals("omcHeaderImported.US_SourceCountry", Core.Constants.CountryCodes.Mexico, omcHeaderImported.US_SourceCountry);
				AssertEquals("omcHeaderImported.US_DepartureDate", ZDateTime.BrettsBirthday, omcHeaderImported.US_DepartureDate);
				AssertEquals("omcHeaderImported.US_ElectronicImageSubmitted", true, omcHeaderImported.US_ElectronicImageSubmitted);
				AssertEquals("omcHeaderImported.US_DeclarationCode", "7A1", omcHeaderImported.US_DeclarationCode);
				AssertEquals("omcHeaderImported.US_OA_AquacultureFacility", aquacultureAddress.PK, omcHeaderImported.US_OA_AquacultureFacility);
				AssertEquals("omcHeaderImported.US_OA_Exporter", exporterAddress.PK, omcHeaderImported.US_OA_Exporter);
				AssertEquals("omcHeaderImported.US_ExporterPGAContactName", "EXPORTER NAME", omcHeaderImported.US_ExporterPGAContactName);
				AssertEquals("omcHeaderImported.US_ExporterPGAContactPhoneNo", "123123131", omcHeaderImported.US_ExporterPGAContactPhoneNo);
				AssertEquals("omcHeaderImported.US_ExporterPGAContactEmail", "EXPORTER@ABC.COM", omcHeaderImported.US_ExporterPGAContactEmail);
				AssertEquals("omcHeaderImported.US_ExporterCertificationDate", new ZDateTime(2016, 12, 13), omcHeaderImported.US_ExporterCertificationDate);
				AssertEquals("omcHeaderImported.US_OA_ResponsibleGovernmentOfficial", governmentAddress.PK, omcHeaderImported.US_OA_ResponsibleGovernmentOfficial);
				AssertEquals("omcHeaderImported.US_OfficialPGAContactName", "OFFICIAL NAME", omcHeaderImported.US_OfficialPGAContactName);
				AssertEquals("omcHeaderImported.US_OfficialPGAContactPhoneNo", "789789789", omcHeaderImported.US_OfficialPGAContactPhoneNo);
				AssertEquals("omcHeaderImported.US_OfficialPGAContactEmail", "OFFICIAL@ABC.COM", omcHeaderImported.US_OfficialPGAContactEmail);
				AssertEquals("omcHeaderImported.US_OfficialCertificationDate", new ZDateTime(2016, 12, 31), omcHeaderImported.US_OfficialCertificationDate);
				AssertEquals("omcHeaderImported.US_NetWeight", 100m, omcHeaderImported.US_NetWeight);
				AssertEquals("omcHeaderImported.US_NetWeightUQ", "KG", omcHeaderImported.US_NetWeightUQ);
				AssertEquals("omcHeaderImported.AquacultureFacilities.Count", 1, omcHeaderImported.AquacultureFacilities.Count);

				var additionalFacilityImported = omcHeaderImported.AquacultureFacilities[0];
				AssertEquals("additionalFacilityImported.US_OA_AquacultureFacility", additionalAddress.PK, additionalFacilityImported.US_OA_AquacultureFacility);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			invoiceLine.US_OMCInd = OGAIndicatorList.Codes.Declared;

			var exporter = Factory.New<OrgHeader>();
			exporter.OH_FullName = "TOYOTA (JAPAN)";
			exporter.OH_Code = "TESEXP";
			exporter.MainAddress.OA_Address1 = "EXPORTER ADDRESS";
			exporterAddress = exporter.Addresses.AddNew();
			exporterAddress.OA_Address1 = "1234 PEACHTREE STREET";
			exporterAddress.OA_City = "ATLANTA";
			exporterAddress.OA_State = "GA";
			exporterAddress.OA_RL_NKRelatedPortCode = "USLAX";
			exporterAddress.OA_PostCode = "30301";
			exporterAddress.OA_CompanyNameOverride = "TEST ATF";

			var aquaculture = Factory.New<OrgHeader>();
			aquaculture.OH_FullName = "MAIN FACILITY";
			aquaculture.OH_Code = "TESTFAC";
			aquaculture.MainAddress.OA_Address1 = "MAIN FACILITY";
			aquacultureAddress = aquaculture.Addresses.AddNew();
			aquacultureAddress.OA_Address1 = "Facility Address2";
			aquacultureAddress.OA_City = "NJ";
			aquacultureAddress.OA_State = "JS";
			aquacultureAddress.OA_RL_NKRelatedPortCode = "USLAX";
			aquacultureAddress.OA_PostCode = "10110";
			aquacultureAddress.OA_CompanyNameOverride = "TEST OMC";

			var governmentOfficial = Factory.New<OrgHeader>();
			governmentOfficial.OH_FullName = "GOVERNMENT OFFICIAL";
			governmentOfficial.OH_Code = "TESTGOV";
			governmentOfficial.MainAddress.OA_Address1 = "MAIN GOVERNMENT";
			governmentAddress = governmentOfficial.Addresses.AddNew();
			governmentAddress.OA_Address1 = "Government Address3";
			governmentAddress.OA_City = "NJ3";
			governmentAddress.OA_State = "J3";
			governmentAddress.OA_RL_NKRelatedPortCode = "USLAX";
			governmentAddress.OA_PostCode = "10111";
			governmentAddress.OA_CompanyNameOverride = "TEST OMC3";

			var additionalAquaculture = Factory.New<OrgHeader>();
			additionalAquaculture.OH_FullName = "ADDITIONAL AQUACULTURE";
			additionalAquaculture.OH_Code = "TESTAQU";
			additionalAquaculture.MainAddress.OA_Address1 = "ADDITIONAL AQUACULTURE";
			additionalAddress = additionalAquaculture.Addresses.AddNew();
			additionalAddress.OA_Address1 = "Additional Address4";
			additionalAddress.OA_City = "NJ4";
			additionalAddress.OA_State = "J4";
			additionalAddress.OA_RL_NKRelatedPortCode = "USLAX";
			additionalAddress.OA_PostCode = "10114";
			additionalAddress.OA_CompanyNameOverride = "TEST OMC4";
		}
		OrgAddress exporterAddress;
		OrgAddress aquacultureAddress;
		OrgAddress governmentAddress;
		OrgAddress additionalAddress;
	}
}

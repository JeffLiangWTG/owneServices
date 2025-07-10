using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ACEBIRDAMSDataProcessorTest : ACEBIRDCommonPGADataProcessorTest
	{
		[TestDate(2021, 12, 01)]
		public void TestAMSEG1DisclaimedAPGA()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000000";
			tariff.UE_PGACodes = "EG1";
			tariff.UE_DateFrom = ZDateTime.Today.AddDays(-10);
			tariff.UE_DateTo = ZDateTime.Today.AddDays(10);

			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariffView = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, tariff.UE_Tariff, new ZDateTime(2021, 01, 01), new ZDateTime(2022, 01, 01));
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, "EG1", tariffView);
			Factory.Save();

			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.JI_Description = "TEST AMS EG1 DISCLAIMED A";
			Factory.Save();

			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Disclaimed;
			AssertEquals(invoiceLine.US_AMSDisclaimReason, PGADisclaimReasonList.Codes.A);
			invoiceLine.US_AMSDisclaimProgram = AMSProgramList.Codes.EG1;
			var action = GetAction(declaration);
			action.US_AcknowledgeAndSign = true;
			action.US_DateOfDeclaration = new ZDateTime(2016, 07, 20);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			ImportMessageBlocks(message);
			AssertEquals(1, DeclarationImported.InvoiceLines.Count);

			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			AssertEquals(OGAIndicatorList.Codes.Disclaimed, invoiceLineImported.US_AMSInd);
			AssertEquals(PGADisclaimReasonList.Codes.A, invoiceLineImported.US_AMSDisclaimReason);
			AssertEquals(AMSProgramList.Codes.EG1, invoiceLineImported.US_AMSDisclaimProgram);
			AssertEquals("TEST AMS EG1 DISCLAIMED A", invoiceLineImported.JI_Description);
		}

		public override void TestEndToEndProcessDisclaimedPGA()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000000";
			tariff.UE_PGACodes = "AM4";
			tariff.UE_DateFrom = ZDateTime.Today.AddDays(-10);
			tariff.UE_DateTo = ZDateTime.Today.AddDays(10);

			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_AMSDisclaimReason = PGADisclaimReasonList.Codes.B;
			invoiceLine.US_AMSDisclaimProgram = AMSProgramList.Codes.MO8;
			invoiceLine.JI_Description = "TEST AMS DISCLAIMED";
			Factory.Save();

			var action = GetAction(declaration);
			action.US_AcknowledgeAndSign = true;
			action.US_DateOfDeclaration = new ZDateTime(2016, 07, 20);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			ImportMessageBlocks(message);
			AssertEquals(1, DeclarationImported.InvoiceLines.Count);

			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			AssertEquals(OGAIndicatorList.Codes.Disclaimed, invoiceLineImported.US_AMSInd);
			AssertEquals(PGADisclaimReasonList.Codes.B, invoiceLineImported.US_AMSDisclaimReason);
			AssertEquals(AMSProgramList.Codes.MO8, invoiceLineImported.US_AMSDisclaimProgram);
			AssertEquals("TEST AMS DISCLAIMED", invoiceLineImported.JI_Description);
		}

		public void TestProcessMO2DeclaredMessageBlocks()
		{
			declaration.IOROrgPK = GetIOROrgHeader().PK;
			declaration.US_FDAContactName = "AMS TEST CONTACT";
			declaration.US_FDAContactEmail = "AMS.TEST@TEST.COM";
			declaration.US_FDAContactPhoneNo = "091245022";

			var amsLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = AMSProgramList.Codes.MO2;
			amsLine.US_IntendedUseCode = AMSIntendedUseCodesList.Codes._230000;
			amsLine.US_CommercialDescription = "IAN TEST MO2";

			var mo2Line = amsLine.AMSLines.AddNew();
			mo2Line.US_IsDocSubmitted = true;
			mo2Line.US_IssueDate = new ZDateTime(2016, 7, 21);
			mo2Line.US_Party = "Canadian Food Inspection Agency";
			mo2Line.US_InspectionLocation = CanadaStatesList.Codes.AB;
			mo2Line.US_Weight = 40000m;
			mo2Line.US_WeightUQ = "LB";
			mo2Line.US_CertNumber = "E0000998";
			mo2Line.US_CertType = LPCOTypeList.Codes.AM6;
			Factory.Save();

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			ImportMessageBlocks(message);
			AssertEquals(1, DeclarationImported.InvoiceLines.Count);
			AssertEquals("AMS TEST CONTACT", DeclarationImported.US_FDAContactName);
			AssertEquals("AMS.TEST@TEST.COM", DeclarationImported.US_FDAContactEmail);
			AssertEquals("091245022", DeclarationImported.US_FDAContactPhoneNo);

			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLineImported.US_AMSInd);
			AssertEquals(1, invoiceLineImported.AMSLines.Count);

			var amsLineImported = invoiceLineImported.AMSLines[0];
			AssertEquals(AMSProgramList.Codes.MO2, amsLineImported.US_Program);
			AssertEquals(AMSIntendedUseCodesList.Codes._230000, amsLineImported.US_IntendedUseCode);
			AssertEquals("IAN TEST MO2", amsLineImported.US_CommercialDescription);
			AssertEquals(1, amsLineImported.AMSLines.Count);

			var mo2LineImported = amsLineImported.AMSLines[0];
			AssertEquals(true, mo2LineImported.US_IsDocSubmitted);
			AssertEquals(new ZDateTime(2016, 7, 21), mo2LineImported.US_IssueDate);
			AssertEquals("CANADIAN FOOD INSPECTION AGENCY", mo2LineImported.US_Party);
			AssertEquals(CanadaStatesList.Codes.AB, mo2LineImported.US_InspectionLocation);
			AssertEquals(40000m, mo2LineImported.US_Weight);
			AssertEquals("LB", mo2LineImported.US_WeightUQ);
			AssertEquals("E0000998", mo2LineImported.US_CertNumber);
			AssertEquals(LPCOTypeList.Codes.AM6, mo2LineImported.US_CertType);
		}

		public void TestProcessMO3DeclaredMessageBlocks()
		{
			var amsLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = AMSProgramList.Codes.MO3;
			amsLine.US_IntendedUseCode = AMSIntendedUseCodesList.Codes._980000;
			amsLine.US_IntendedUseDescription = "TEST DESC";
			amsLine.US_CommercialDescription = "IAN TEST MO3";
			var mo3Line = amsLine.AMSLines.AddNew();
			mo3Line.US_AuthorizationNumber = "39323";

			Factory.Save();

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			ImportMessageBlocks(message);
			AssertEquals(1, DeclarationImported.InvoiceLines.Count);

			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLineImported.US_AMSInd);
			AssertEquals(1, invoiceLineImported.AMSLines.Count);

			var amsLineImported = invoiceLineImported.AMSLines[0];
			AssertEquals(AMSProgramList.Codes.MO3, amsLineImported.US_Program);
			AssertEquals(AMSIntendedUseCodesList.Codes._980000, amsLineImported.US_IntendedUseCode);
			AssertEquals("TEST DESC", amsLineImported.US_IntendedUseDescription);
			AssertEquals("IAN TEST MO3", amsLineImported.US_CommercialDescription);
			AssertEquals(1, amsLineImported.AMSLines.Count);

			var mo3LineImported = amsLineImported.AMSLines[0];
			AssertEquals("39323", mo3LineImported.US_AuthorizationNumber);
		}

		public void TestProcessMO4DeclaredMessageBlocks()
		{
			var iorOrgHeader = GetIOROrgHeader();
			iorOrgHeader.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.SocialSecurityNumber, "256", "US");
			declaration.IOROrgPK = iorOrgHeader.PK;

			var cne = Factory.New<OrgHeader>();
			cne.OH_Code = "TESTCNE";
			cne.OH_FullName = "ULTIMATE CONSIGNEE";
			cne.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.SocialSecurityNumber, "425", "US");
			cne.MainAddress.OA_Address1 = "CONSIGNEE ADDRESS";
			invoiceLine.JI_OA_ConsigneeAddress = cne.MainAddress.PK;

			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			var amsLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = AMSProgramList.Codes.MO4;
			amsLine.US_IntendedUseCode = AMSIntendedUseCodesList.Codes._025000;
			amsLine.US_CommercialDescription = "IAN TEST MO4";
			var mo4Line = amsLine.AMSLines.AddNew();
			mo4Line.US_NetWeight = 22000m;
			mo4Line.US_NetWeightUQ = "LB";
			mo4Line.US_ProductNumber = "50401736";
			Factory.Save();

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			ImportMessageBlocks(message);
			AssertEquals(1, DeclarationImported.InvoiceLines.Count);

			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLineImported.US_AMSInd);
			AssertEquals(cne.MainAddress.PK, invoiceLineImported.JI_OA_ConsigneeAddress);
			AssertEquals(1, invoiceLineImported.AMSLines.Count);

			var amsLineImported = invoiceLineImported.AMSLines[0];
			AssertEquals(AMSProgramList.Codes.MO4, amsLineImported.US_Program);
			AssertEquals(AMSIntendedUseCodesList.Codes._025000, amsLineImported.US_IntendedUseCode);
			AssertEquals("IAN TEST MO4", amsLineImported.US_CommercialDescription);
			AssertEquals(1, amsLineImported.AMSLines.Count);

			var mo4LineImported = amsLineImported.AMSLines[0];
			AssertEquals(22000m, mo4LineImported.US_NetWeight);
			AssertEquals("LB", mo4LineImported.US_NetWeightUQ);
			AssertEquals("50401736", mo4LineImported.US_ProductNumber);
		}

		public void TestProcessMO7DeclaredMessageBlocks()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000000";
			tariff.UE_PGACodes = "AM4";
			tariff.UE_DateFrom = ZDateTime.Today.AddDays(-10);
			tariff.UE_DateTo = ZDateTime.Today.AddDays(10);

			declaration.US_FDAADTA = new ZDateTime(2016, 7, 21, 8, 40, 0);
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			var amsLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = AMSProgramList.Codes.MO7;
			invoiceLine.JI_Description = "TEST AMS DISCLAIMED";
			Factory.Save();

			var action = GetAction(declaration);
			action.US_AcknowledgeAndSign = true;
			action.US_DateOfDeclaration = new ZDateTime(2016, 07, 20);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			ImportMessageBlocks(message);
			AssertEquals(new ZDateTime(2016, 7, 21, 8, 40, 0), DeclarationImported.US_FDAADTA);
			AssertEquals(1, DeclarationImported.InvoiceLines.Count);

			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLineImported.US_AMSInd);
			AssertEquals(1, invoiceLineImported.AMSLines.Count);

			var amsLineImported = invoiceLineImported.AMSLines[0];
			AssertEquals(AMSProgramList.Codes.MO7, amsLineImported.US_Program);
		}

		public void TestProcessPN1DeclaredMessageBlocks()
		{
			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CONT1";
			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "CONT2";
			var containerPivots = invoiceLine.ContainersForInvoiceLinesForBindingOnly.Cast<NonPersistentCusContainer>().OrderBy(x => x.ContainerNumber).ToArray();
			containerPivots[0].IsForInvoiceLine = true;
			containerPivots[1].IsForInvoiceLine = true;

			var amsLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = AMSProgramList.Codes.PN1;
			amsLine.US_IntendedUseCode = AMSIntendedUseCodesList.Codes._230000;
			amsLine.US_CommercialDescription = "INC TEST PN1";

			var pn1Line = amsLine.AMSLines.AddNew();
			pn1Line.US_InspecDateTime = new ZDateTime(2016, 7, 21);
			pn1Line.US_InspecRemarks = "PLEASE CALL 30 MIN BEFFORE ARRIVAL";
			pn1Line.US_NetWeight = 6000m;
			pn1Line.US_NetWeightUQ = "LB";

			pn1Line.US_Packages = 3m;
			pn1Line.US_PackagesUQ = "MB";
			pn1Line.US_PackageWeight = 2000m;
			pn1Line.US_PackageWeightUQ = "LB";
			pn1Line.US_ProductNumber = "50102504";

			var lotCode1 = pn1Line.LotCodes.AddNew();
			lotCode1.CY_Code = "1";
			lotCode1.CY_Data = "4-521413";

			var lotCode2 = pn1Line.LotCodes.AddNew();
			lotCode2.CY_Code = "2";
			lotCode2.CY_Data = "4-521412";

			var lotCode3 = pn1Line.LotCodes.AddNew();
			lotCode3.CY_Code = "3";
			lotCode3.CY_Data = "4-528743";
			Factory.Save();

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			ImportMessageBlocks(message);
			AssertEquals(1, DeclarationImported.InvoiceLines.Count);
			AssertEquals(2, DeclarationImported.CusContainers.Count);

			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLineImported.US_AMSInd);
			AssertEquals(1, invoiceLineImported.AMSLines.Count);

			var amsLineImported = invoiceLineImported.AMSLines[0];
			AssertEquals(AMSProgramList.Codes.PN1, amsLineImported.US_Program);
			AssertEquals(AMSIntendedUseCodesList.Codes._230000, amsLineImported.US_IntendedUseCode);
			AssertEquals("INC TEST PN1", amsLineImported.US_CommercialDescription);
			AssertEquals(1, amsLineImported.AMSLines.Count);

			var pn1LineImported = amsLineImported.AMSLines[0];
			AssertEquals(new ZDateTime(2016, 7, 21), pn1LineImported.US_InspecDateTime);
			AssertEquals("PLEASE CALL 30 MIN BEFFORE ARRIVAL", pn1LineImported.US_InspecRemarks);
			AssertEquals(6000m, pn1LineImported.US_NetWeight);
			AssertEquals("LB", pn1LineImported.US_NetWeightUQ);
			AssertEquals(3m, pn1LineImported.US_Packages);
			AssertEquals("MB", pn1LineImported.US_PackagesUQ);
			AssertEquals(2000m, pn1LineImported.US_PackageWeight);
			AssertEquals("LB", pn1LineImported.US_PackageWeightUQ);
			AssertEquals("50102504", pn1LineImported.US_ProductNumber);
			AssertEquals(3, pn1LineImported.LotCodes.Count);

			var lotCode0Imported = pn1LineImported.LotCodes.OfType<AMSLotCode>().FirstOrDefault(x => x.CY_Code == "1");
			AssertEquals("4-521413", lotCode0Imported.CY_Data);
			var lotCode1Imported = pn1LineImported.LotCodes.OfType<AMSLotCode>().FirstOrDefault(x => x.CY_Code == "2");
			AssertEquals("4-521412", lotCode1Imported.CY_Data);
			var lotCode2Imported = pn1LineImported.LotCodes.OfType<AMSLotCode>().FirstOrDefault(x => x.CY_Code == "3");
			AssertEquals("4-528743", lotCode2Imported.CY_Data);
		}

		protected override MQEDIMessage GetEDIMessageForEndToEndTest()
		{
			declaration.IOROrgPK = GetIOROrgHeader().PK;
			declaration.US_FDAADTA = new ZDateTime(2016, 7, 20);

			var amsLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = AMSProgramList.Codes.MO1;
			amsLine.US_IntendedUseCode = AMSIntendedUseCodesList.Codes._230000;
			amsLine.US_CommercialDescription = "IAN TEST MO1";

			var mo1Line = amsLine.AMSLines.AddNew();
			mo1Line.US_InspecDateTime = new ZDateTime(2016, 7, 20, 12, 12, 0);
			mo1Line.US_InspecRemarks = "PLEASE CALL 30 MIN BEFFORE ARRIVAL";
			mo1Line.US_NetWeight = 270000m;
			mo1Line.US_NetWeightUQ = "LB";
			mo1Line.US_PackageWeight = 27m;
			mo1Line.US_PackageWeightUQ = "LB";
			mo1Line.US_Packages = 1000m;
			mo1Line.US_PackagesUQ = "CT";
			mo1Line.US_ProductNumber = "50303904";
			mo1Line.US_QtyPerPackage = 60m;
			mo1Line.US_QtyPerPackageUQ = "NO";
			mo1Line.US_OA_Applicant = applicant.MainAddress.PK;
			mo1Line.US_OA_GoodsLocation = goodsLocation.MainAddress.PK;

			Factory.Save();

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			return builder.PopulateMessage();
		}

		protected override void AssertEndToEndTestResult()
		{
			AssertEquals(1, DeclarationImported.InvoiceLines.Count);
			AssertEquals(new ZDateTime(2016, 7, 20), DeclarationImported.US_FDAADTA);

			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLineImported.US_AMSInd);
			AssertEquals(1, invoiceLine.AMSLines.Count);

			var amsLineImported = invoiceLineImported.AMSLines[0];
			CombineAssertions(() =>
			{
				AssertEquals(AMSProgramList.Codes.MO1, amsLineImported.US_Program);
				AssertEquals(AMSIntendedUseCodesList.Codes._230000, amsLineImported.US_IntendedUseCode);
				AssertEquals("IAN TEST MO1", amsLineImported.US_CommercialDescription);
				AssertEquals(1, amsLineImported.AMSLines.Count);

				var mo1LineImported = amsLineImported.AMSLines[0];
				AssertEquals(new ZDateTime(2016, 7, 20, 12, 12, 0), mo1LineImported.US_InspecDateTime);
				AssertEquals("PLEASE CALL 30 MIN BEFFORE ARRIVAL", mo1LineImported.US_InspecRemarks);
				AssertEquals(270000m, mo1LineImported.US_NetWeight);
				AssertEquals("LB", mo1LineImported.US_NetWeightUQ);
				AssertEquals(27m, mo1LineImported.US_PackageWeight);
				AssertEquals("LB", mo1LineImported.US_PackageWeightUQ);
				AssertEquals(1000m, mo1LineImported.US_Packages);
				AssertEquals("CT", mo1LineImported.US_PackagesUQ);
				AssertEquals("50303904", mo1LineImported.US_ProductNumber);
				AssertEquals(60m, mo1LineImported.US_QtyPerPackage);
				AssertEquals("NO", mo1LineImported.US_QtyPerPackageUQ);
				AssertEquals(applicant.MainAddress.PK, mo1LineImported.US_OA_Applicant);
				AssertEquals(goodsLocation.MainAddress.PK, mo1LineImported.US_OA_GoodsLocation);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;

			applicant = Factory.New<OrgHeader>();
			applicant.OH_Code = "TESTAPP";
			applicant.OH_FullName = "BUYER";
			var imAddress = applicant.MainAddress;
			imAddress.OA_RL_NKRelatedPortCode = "US2CW";
			imAddress.OA_Address1 = "BUYER ADDRESS 1";
			imAddress.OA_Address2 = "BUYER ADDRESS 2";
			imAddress.OA_City = "MELBOURN";
			imAddress.OA_State = "MEL";
			imAddress.OA_PostCode = "2011";
			DeclarationTestHelper.AddPGAContact(imAddress, "BUYER", "CONTACT", "04 654321", "BUYER EMAIL", "BUYER FAX");

			goodsLocation = Factory.New<OrgHeader>();
			invoiceLine.InvoiceHeader.JZ_OH_Buyer = goodsLocation.PK;
			goodsLocation.OH_FullName = "GOODS";
			var goodLoactionAddress = goodsLocation.MainAddress;
			goodLoactionAddress.OA_RL_NKRelatedPortCode = "US2CW";
			goodLoactionAddress.OA_Address1 = "GOODS ADDRESS 1";
			goodLoactionAddress.OA_Address2 = "GOODS ADDRESS 2";
			goodLoactionAddress.OA_City = "MELBOURN";
			goodLoactionAddress.OA_State = "MEK";
			goodLoactionAddress.OA_PostCode = "2013";
			DeclarationTestHelper.AddPGAContact(goodsLocation, "GOODS", "CONTACT", "04 654323", "GOODS EMAIL", "GOODS FAX");
		}
		OrgHeader applicant;
		OrgHeader goodsLocation;

		OrgHeader GetIOROrgHeader()
		{
			var importOfRecord = Factory.New<OrgHeader>();
			importOfRecord.OH_FullName = "IMPORTER OF RECORD";
			var iorAddress = importOfRecord.MainAddress;
			iorAddress.OA_RL_NKRelatedPortCode = "US2CW";
			iorAddress.OA_Address1 = "IOR ADDRESS 1";
			iorAddress.OA_Address2 = "IOR ADDRESS 2";
			iorAddress.OA_City = "SYDNEY";
			iorAddress.OA_State = "NSW";
			iorAddress.OA_PostCode = "2017";

			DeclarationTestHelper.AddPGAContact(importOfRecord, "IOR", "ALEXANDER THE GREATEST OF ALL", "04 123456", "IOR EMAIL", "IOR FAX");
			return importOfRecord;
		}
	}
}

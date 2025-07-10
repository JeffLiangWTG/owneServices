using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	abstract class CommonImportAddInfoComInvoiceLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_FlavorContentCreditInd()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var validation = invoiceLine.AddInfoValidation;
			validation.ValidateUS_FlavorContentCreditInd();
			AssertNoWarnings(invoiceLine.US_FlavorContentCreditIndInfo);
			invoiceLine.US_FlavorContentCreditInd = true;
			AssertHasWarningContaining(invoiceLine.US_FlavorContentCreditIndInfo, "The Flavor Content Credit Indicator usually only applies to spirits.");
			invoiceLine.US_TaxCode = Core.Constants.USCustoms.FeeCodes.DistilledSpirits;
			validation.ValidateUS_FlavorContentCreditInd();
			AssertNoWarnings(invoiceLine.US_FlavorContentCreditIndInfo);
			invoiceLine.US_TaxCode = Core.Constants.USCustoms.FeeCodes.Avocado;
			validation.ValidateUS_FlavorContentCreditInd();
			AssertHasWarningContaining(invoiceLine.US_FlavorContentCreditIndInfo, "The Flavor Content Credit Indicator usually only applies to spirits.");
			invoiceLine.US_FlavorContentCreditInd = false;
			AssertNoWarnings(invoiceLine.US_FlavorContentCreditIndInfo);
		}

		[TestDate(2018, 10, 22)]
		public void TestCheckSPIForCombinedLines()
		{
			var testHelper = new CombinedLinesHelperTest();
			var testJob = testHelper.CombinedJob;
			var invoiceLine1 = testHelper.InvoiceHeaderForCombined.InvoiceLines.AddNew();
			var invoiceLine2 = testHelper.InvoiceHeaderForCombined.InvoiceLines.AddNew();
			invoiceLine1.US_UC_NKCountryOfOrigin = "CN";
			invoiceLine1.US_UC_NKCountryOfExport = "CN";
			invoiceLine1.US_SupTariff = testHelper.Chapter98TestingHelper.Test99038801Tariff.UE_Tariff;
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			invoiceLine2.US_SupTariff = "9802.00.4020";
			invoiceLine2.JI_Tariff = "8803.30.0060";
			invoiceLine2.US_UC_NKCountryOfOrigin = "CN";
			invoiceLine2.US_UC_NKCountryOfExport = "CN";
			testJob.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			var spiList = invoiceLine1.AddInfoLookups.SPIList;
			AssertEquals(4, spiList.Count);
			Assert(spiList.ContainsCode("N/A"));
			Assert(spiList.ContainsCode("C"));
			Assert(spiList.ContainsCode("K"));
			Assert(spiList.ContainsCode("L"));
			invoiceLine1.US_SPI = "C";
			AssertNoMessageError(invoiceLine1.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.NotEligibleForSPI);
			invoiceLine1.US_SPI = "R";
			AssertHasMessageError(invoiceLine1.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.NotEligibleForSPI);
		}

		public void TestCheckUS_SPIForCAFTA()
		{
			USCustomsDataRegistry.Instance.SPIValidation.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, SPIValidationTypeList.Codes.MER);
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "20203108";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff.UE_SPICode = "D R P AUBHCACLILJ+JOMAMXSG";
			var gtCountry = Factory.LoadTop1<USCCountry>(new ZQuery(USCCountrySchema.UC_ISOCountryCode, "GT"));
			if (gtCountry == null)
			{
				gtCountry = Factory.New<USCCountry>();
				gtCountry.UC_ISOCountryCode = "GT";
			}

			gtCountry.UC_GSPIndicator = true;
			gtCountry.UC_GSPBeginDate = ZDateTime.BrettsBirthday;
			gtCountry.UC_GSPEndDate = ZDateTime.Today.AddYears(1);
			gtCountry.UC_MiscellaneousSPIIndicator = "P";
			gtCountry.UC_MiscellaneousSPIBeginDate = ZDateTime.BrettsBirthday;
			gtCountry.UC_MiscellaneousSPIEndDate = ZDateTime.Today.AddYears(1);
			var ttCountry = Factory.LoadTop1<USCCountry>(new ZQuery(USCCountrySchema.UC_ISOCountryCode, "TT"));
			if (ttCountry == null)
			{
				ttCountry = Factory.New<USCCountry>();
				ttCountry.UC_ISOCountryCode = "TT";
			}

			ttCountry.UC_GSPIndicator = true;
			ttCountry.UC_GSPBeginDate = ZDateTime.BrettsBirthday;
			ttCountry.UC_GSPEndDate = ZDateTime.Today.AddYears(1);
			ttCountry.UC_MiscellaneousSPIIndicator = "";
			ttCountry.UC_MiscellaneousSPIBeginDate = ZDateTime.BrettsBirthday;
			ttCountry.UC_MiscellaneousSPIEndDate = ZDateTime.Today.AddYears(1);
			Factory.Save();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "20203108";
			invoiceLine.US_UC_NKCountryOfOrigin = "TT";
			invoiceLine.US_UC_NKCountryOfExport = "TT";
			invoiceLine.US_SPI = "P";
			AssertHasMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.CAFTAOriginatingClaim);
			invoiceLine.US_UC_NKCountryOfExport = "GT";
			invoiceLine.AddInfoValidation.ValidateUS_SPI();
			AssertNoMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.CAFTAOriginatingClaim);
		}

		public void TestCheckUS_SPIForCertificateOfOrigin()
		{
			var doc = Factory.New<JobRequiredDocument>();
			doc.EQ_DocType = Core.Constants.RefDocTypes.CertificateOfOrigin;
			var attr = doc.Attributes.AddNew();
			attr.D0_AttribName = JobRequiredDocAttribTypeList.Codes.TradePreferenceCode;
			attr.D0_AttribValue = "S";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var importer = Factory.New<OrgHeader>();
			importer.OH_IsConsignee = true;
			declaration.JE_OH_Importer = importer.PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_SPI = "S";
			var part = Factory.New<OrgSupplierPart>();
			invoiceLine.SetPartForTesting(part);

			using (USCustomsDataRegistry.Instance.SeverityLevelForMissingUSMCACertificate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, ProductAuditActions.Codes.NoAction))
			{
				invoiceLine.AddInfoValidation.ValidateUS_SPI();
				AssertNoMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.CheckCertificateOfOrigin);
			}

			using (USCustomsDataRegistry.Instance.SeverityLevelForMissingUSMCACertificate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, ProductAuditActions.Codes.AddWarningValidation))
			{
				invoiceLine.AddInfoValidation.ValidateUS_SPI();
				AssertHasWarning(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.CheckCertificateOfOrigin);
			}

			using (USCustomsDataRegistry.Instance.SeverityLevelForMissingUSMCACertificate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, ProductAuditActions.Codes.AddMessageErrorValidation))
			{
				invoiceLine.AddInfoValidation.ValidateUS_SPI();
				AssertHasMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.CheckCertificateOfOrigin);

				part.RequiredDocuments.Add(doc);
				invoiceLine.AddInfoValidation.ValidateUS_SPI();
				AssertNoMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.CheckCertificateOfOrigin);

				part.RequiredDocuments.RemoveAll();
				invoiceLine.AddInfoValidation.ValidateUS_SPI();
				AssertHasMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.CheckCertificateOfOrigin);

				importer.RequiredDocuments.Add(doc);
				invoiceLine.AddInfoValidation.ValidateUS_SPI();
				AssertNoMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.CheckCertificateOfOrigin);

				doc.EQ_ValidToDate = invoiceLine.EffectiveDateForDutyRate.AddDays(-1);
				invoiceLine.AddInfoValidation.ValidateUS_SPI();
				AssertHasMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.CheckCertificateOfOrigin);
			}
		}

		public void TestCheckUS_CVD_NA()
		{
			var addCase = Factory.New<USCACCase>();
			addCase.U5_CaseNumber = "ADD111";
			addCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			addCase.U5_CaseStatusDate = ZDateTime.Today;
			addCase.U5_ISOCountryCode = "CA";
			addCase.CaseTariffs.AddNew().U9_TariffNumber = "22030060";
			var cvdCase = Factory.New<USCACCase>();
			cvdCase.U5_CaseNumber = "CVD111";
			cvdCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			cvdCase.U5_CaseStatusDate = ZDateTime.Today;
			cvdCase.U5_ISOCountryCode = "CA";
			cvdCase.CaseTariffs.AddNew().U9_TariffNumber = "22030060";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_CVD_NA = false;
			invoiceLine.US_CVDCaseNo = "CVD111";
			invoiceLine.US_ADD_NA = false;
			invoiceLine.US_ADDCaseNo = "ADD111";
			AssertNoMessageErrorContaining("CVD N/A cannot be ticked if Case information is entered.", invoiceLine.US_CVD_NAInfo, FTZAddInfoJobComInvoiceLineValidation.NACannotBeTickedWithCaseNum);
			AssertNoMessageErrorContaining("ADD N/A cannot be ticked if Case information is entered.", invoiceLine.US_ADD_NAInfo, FTZAddInfoJobComInvoiceLineValidation.NACannotBeTickedWithCaseNum);
			invoiceLine.US_CVD_NA = true;
			invoiceLine.US_ADD_NA = true;
			AssertHasMessageErrorContaining("CVD N/A cannot be ticked if Case information is entered.", invoiceLine.US_CVD_NAInfo, FTZAddInfoJobComInvoiceLineValidation.NACannotBeTickedWithCaseNum);
			AssertHasMessageErrorContaining("ADD N/A cannot be ticked if Case information is entered.", invoiceLine.US_ADD_NAInfo, FTZAddInfoJobComInvoiceLineValidation.NACannotBeTickedWithCaseNum);
		}

		public void TestCheckUS_ADDNumberIsReportable()
		{
			USCTariff tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "9010000010";
			tariff1.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff1.UE_DateTo = ZDateTime.MaxSmallDateTime;
			USCTariff tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "9010000020";
			tariff2.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff2.UE_DateTo = ZDateTime.MaxSmallDateTime;
			USCTariff tariff3 = Factory.New<USCTariff>();
			tariff3.UE_Tariff = "9010000030";
			tariff3.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff3.UE_DateTo = ZDateTime.MaxSmallDateTime;
			USCTariff tariff4 = Factory.New<USCTariff>();
			tariff4.UE_Tariff = "9010000040";
			tariff4.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff4.UE_DateTo = ZDateTime.MaxSmallDateTime;
			USCTariff tariff5 = Factory.New<USCTariff>();
			tariff5.UE_Tariff = "9010000050";
			tariff5.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff5.UE_DateTo = ZDateTime.MaxSmallDateTime;
			USCTariff tariff6 = Factory.New<USCTariff>();
			tariff6.UE_Tariff = "9010000060";
			tariff6.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff6.UE_DateTo = ZDateTime.MaxSmallDateTime;
			var effectiveDutyDate = ZDateTime.Now;
			var lastWeek = effectiveDutyDate.AddDays(-7);
			var nextWeek = effectiveDutyDate.AddDays(7);
			var today = effectiveDutyDate;
			var yesterday = today.AddDays(-1);
			//1
			var acCase1 = Factory.New<USCACCase>();
			acCase1.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			acCase1.U5_CaseNumber = "AA0101";
			acCase1.U5_CaseStatusDate = ZDateTime.BrettsBirthday;
			var caTariff1 = acCase1.CaseTariffs.AddNew();
			caTariff1.U9_TariffNumber = tariff1.UE_Tariff;
			var suspension1 = acCase1.LiqSuspensions.AddNew();
			suspension1.UN_Action = "START";
			suspension1.UN_EffectiveDate = lastWeek;
			suspension1.UN_AddedDate = lastWeek;
			suspension1.UN_InactivatedDate = ZDateTime.Empty;
			AssertEquals(true, ((IACCase)acCase1).IsReportable(effectiveDutyDate));
			//2
			var acCase2 = Factory.New<USCACCase>();
			acCase2.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			acCase2.U5_CaseNumber = "AA0102";
			acCase2.U5_CaseStatusDate = ZDateTime.BrettsBirthday;
			var caTariff2 = acCase2.CaseTariffs.AddNew();
			caTariff2.U9_TariffNumber = tariff1.UE_Tariff;
			var suspension2 = acCase2.LiqSuspensions.AddNew();
			suspension2.UN_Action = "START";
			suspension2.UN_EffectiveDate = lastWeek;
			suspension2.UN_AddedDate = lastWeek;
			suspension2.UN_InactivatedDate = yesterday;
			AssertEquals(false, ((IACCase)acCase2).IsReportable(effectiveDutyDate));
			//3
			var acCase3 = Factory.New<USCACCase>();
			acCase3.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			acCase3.U5_CaseNumber = "AA0103";
			acCase3.U5_CaseStatusDate = ZDateTime.BrettsBirthday;
			var caTariff3 = acCase3.CaseTariffs.AddNew();
			caTariff3.U9_TariffNumber = tariff1.UE_Tariff;
			var suspension3 = acCase3.LiqSuspensions.AddNew();
			suspension3.UN_Action = "START";
			suspension3.UN_EffectiveDate = lastWeek;
			suspension3.UN_AddedDate = lastWeek;
			suspension3.UN_InactivatedDate = nextWeek;
			AssertEquals(false, ((IACCase)acCase3).IsReportable(effectiveDutyDate));
			//4
			var acCase4 = Factory.New<USCACCase>();
			acCase4.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			acCase4.U5_CaseNumber = "AA0104";
			acCase4.U5_CaseStatusDate = ZDateTime.BrettsBirthday;
			var caTariff4 = acCase4.CaseTariffs.AddNew();
			caTariff4.U9_TariffNumber = tariff1.UE_Tariff;
			var suspension4 = acCase4.LiqSuspensions.AddNew();
			suspension4.UN_Action = "STOP";
			suspension4.UN_EffectiveDate = lastWeek;
			suspension4.UN_AddedDate = lastWeek;
			suspension4.UN_InactivatedDate = ZDateTime.Empty;
			AssertEquals(false, ((IACCase)acCase4).IsReportable(effectiveDutyDate));
			//5
			var acCase5 = Factory.New<USCACCase>();
			acCase5.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			acCase5.U5_CaseNumber = "AA0105";
			acCase5.U5_CaseStatusDate = ZDateTime.BrettsBirthday;
			var caTariff5 = acCase5.CaseTariffs.AddNew();
			caTariff5.U9_TariffNumber = tariff1.UE_Tariff;
			var suspension5 = acCase5.LiqSuspensions.AddNew();
			suspension5.UN_Action = "STOP";
			suspension5.UN_EffectiveDate = lastWeek;
			suspension5.UN_AddedDate = lastWeek;
			suspension5.UN_InactivatedDate = yesterday;
			AssertEquals(false, ((IACCase)acCase5).IsReportable(effectiveDutyDate));
			//6
			var acCase6 = Factory.New<USCACCase>();
			acCase6.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			acCase6.U5_CaseNumber = "AA0106";
			acCase6.U5_CaseStatusDate = ZDateTime.BrettsBirthday;
			var caTariff6 = acCase3.CaseTariffs.AddNew();
			caTariff6.U9_TariffNumber = tariff1.UE_Tariff;
			var suspension6 = acCase6.LiqSuspensions.AddNew();
			suspension6.UN_Action = "STOP";
			suspension6.UN_EffectiveDate = lastWeek;
			suspension6.UN_AddedDate = lastWeek;
			suspension6.UN_InactivatedDate = nextWeek;
			AssertEquals(false, ((IACCase)acCase6).IsReportable(effectiveDutyDate));
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "HD11";
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariff1.UE_Tariff;
			Factory.Save();
			invoiceLine.US_ADDCaseNo = "AA0101"; // SUSPENSION ACTIVE
			AssertNoMessageErrorContaining("1. No Message Error", invoiceLine.US_ADDCaseNoInfo, (ZString)CommonImportAddInfoJobComInvoiceLineValidation.ADD_CVDIsNotReportable);
			invoiceLine.US_ADDCaseNo = "AA0102"; // SUSPENSION INACTIVE
			AssertHasWarning("2. Message Error", invoiceLine.US_ADDCaseNoInfo, (ZString)CommonImportAddInfoJobComInvoiceLineValidation.ADD_CVDIsNotReportable);
			invoiceLine.US_ADDCaseNo = "AA0103"; // SUSPENSION INACTIVE
			AssertHasWarning("3. Message Error", invoiceLine.US_ADDCaseNoInfo, (ZString)CommonImportAddInfoJobComInvoiceLineValidation.ADD_CVDIsNotReportable);
			invoiceLine.US_ADDCaseNo = "AA0104"; // SUSPENSION INACTIVE
			AssertHasWarning("4. Message Error", invoiceLine.US_ADDCaseNoInfo, (ZString)CommonImportAddInfoJobComInvoiceLineValidation.ADD_CVDIsNotReportable);
			invoiceLine.US_ADDCaseNo = "AA0105"; // SUSPENSION INACTIVE
			AssertHasWarning("5. Message Error", invoiceLine.US_ADDCaseNoInfo, (ZString)CommonImportAddInfoJobComInvoiceLineValidation.ADD_CVDIsNotReportable);
			invoiceLine.US_ADDCaseNo = "AA0106"; // SUSPENSION INACTIVE
			AssertHasWarning("6. Message Error", invoiceLine.US_ADDCaseNoInfo, (ZString)CommonImportAddInfoJobComInvoiceLineValidation.ADD_CVDIsNotReportable);
		}

		public void TestCheckUS_ADDNumberIsNotReportableWithoutSuspensionRecords()
		{
			USCTariff tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "9010000010";
			tariff1.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff1.UE_DateTo = ZDateTime.MaxSmallDateTime;
			var effectiveDutyDate = ZDateTime.Now;
			var today = effectiveDutyDate;
			var acCase1 = Factory.New<USCACCase>();
			acCase1.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			acCase1.U5_CaseNumber = "AA0101";
			acCase1.U5_CaseStatusDate = ZDateTime.BrettsBirthday;
			var caTariff1 = acCase1.CaseTariffs.AddNew();
			caTariff1.U9_TariffNumber = tariff1.UE_Tariff;
			AssertEquals(false, ((IACCase)acCase1).IsReportable(effectiveDutyDate));
		}

		public void TestNAFTCertificatesAgainstOrgSupplierPart()
		{
			var declaration = new DeclarationTestHelper().CreateSimpleImportDeclaration(Factory);
			var importer = Factory.New<OrgHeader>();
			declaration.IOROrgPK = importer.PK;
			declaration.JE_OH_Importer = importer.PK;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.DoMerge();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "ProductForTesting";
			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = importer.PK;
			relOrg.OU_Relationship = "OWN";
			invoiceLine.JI_PartNo = product.OP_PartNum;
			AssertNotNull(invoiceLine.Part);
			var partHasRequiredDocuments = (IHaveRequiredDocuments)invoiceLine.Part;
			var nafDoc = partHasRequiredDocuments.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.Nafta);
			nafDoc.EQ_ValidToDate = ZDateTime.Now.AddDays(20);
			invoiceLine.US_SPI = SpecialProgramList.Codes.MX;
			AssertNoMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.CheckNAFTACertificate);
			var nafDoc2 = partHasRequiredDocuments.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.Nafta);
			nafDoc2.EQ_ValidToDate = ZDateTime.Now.AddDays(-10);
			invoiceLine.US_SPI = SpecialProgramList.Codes.CA;
			AssertNoMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.CheckNAFTACertificate);
			nafDoc.EQ_ValidToDate = ZDateTime.Now.AddDays(-20);
			invoiceLine.US_SPI = SpecialProgramList.Codes.MX;
			AssertHasMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.CheckNAFTACertificate);
		}

		public void TestCheckUS_SPINotEnteredWhenThereAreApplicableSPIsWithRegistry()
		{
			USCustomsDataRegistry.Instance.SPIValidation.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, SPIValidationTypeList.Codes.WRN);
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff.UE_SPICode = "D R P AUBHCACLILJ+JOMAMXSG";
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0000";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.US_SPI = "";
			AssertHasWarning(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.ThereAreSPIsApplicableNonEntered);
			invoiceLine.US_SPI = "AU";
			AssertNoWarning(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.ThereAreSPIsApplicableNonEntered);
			USCustomsDataRegistry.Instance.SPIValidation.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, SPIValidationTypeList.Codes.MER);
			invoiceLine.US_SPI = "";
			AssertHasMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.ThereAreSPIsApplicableNonEntered);
			invoiceLine.US_SPI = "AU";
			AssertNoMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.ThereAreSPIsApplicableNonEntered);
		}

		public void TestNAFTACertificatesAgainstOrgSupplierPartAndImporter()
		{
			var declaration = new DeclarationTestHelper().CreateSimpleImportDeclaration(Factory);
			var importer = Factory.New<OrgHeader>();
			var nafDoc = importer.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.Nafta);
			declaration.IOROrgPK = importer.PK;
			declaration.JE_OH_Importer = importer.PK;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.DoMerge();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "ProductForTesting";
			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = importer.PK;
			relOrg.OU_Relationship = "OWN";
			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_TariffNum = USCTariff.CottonFeeApplicable;
			invoiceLine.JI_PartNo = product.OP_PartNum;
			AssertNotNull(invoiceLine.Part);
			nafDoc.EQ_ValidToDate = ZDateTime.Now.AddDays(10);
			invoiceLine.US_SPI = SpecialProgramList.Codes.CA;
			AssertNoMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.CheckNAFTACertificate);
			invoiceLine.US_SPI = SpecialProgramList.Codes.IL;
			nafDoc.EQ_ValidToDate = ZDateTime.Now.AddDays(-10);
			invoiceLine.US_SPI = SpecialProgramList.Codes.CA;
			AssertHasMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.CheckNAFTACertificate);
			invoiceLine.US_SPI = SpecialProgramList.Codes.MX;
			AssertHasMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.CheckNAFTACertificate);
			importer.RequiredDocuments.Remove(nafDoc.PK);
			var pivot = invoiceLine.Part.PivotsForBinding.AddNew();
			var classification = Factory.New<CusClassification>();
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			classification.CC_LookupCode = "Z1Z!";
			classification.CC_TariffNum = "30000000";
			pivot.CI_CC = classification.PK;
			invoiceLine.JI_CC = classification.PK;
			var partHasRequiredDocuments = (IHaveRequiredDocuments)invoiceLine.Part;
			var nafDoc2 = partHasRequiredDocuments.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.Nafta);
			nafDoc2.EQ_ValidToDate = ZDateTime.Now.AddDays(20);
			invoiceLine.US_SPI = SpecialProgramList.Codes.MX;
			AssertNoMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.CheckNAFTACertificate);
			nafDoc2.EQ_ValidToDate = ZDateTime.Now.AddDays(-5);
			invoiceLine.US_SPI = SpecialProgramList.Codes.CA;
			AssertHasMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.CheckNAFTACertificate);
			invoiceLine.US_SPI = SpecialProgramList.Codes.MX;
			AssertHasMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.CheckNAFTACertificate);
			var nafDoc3 = importer.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.Nafta);
			nafDoc3.EQ_ValidToDate = ZDateTime.Now.AddDays(10);
			invoiceLine.US_SPI = SpecialProgramList.Codes.CA;
			AssertNoMessageError("No error message, because If a certificate doc is expired against a product, but there is a certificate doc against importer that is still valid", invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.CheckNAFTACertificate);
		}

		public void TestNAFTAMXAndCAExportCountry()
		{
			var declaration = new DeclarationTestHelper().CreateSimpleImportDeclaration(Factory);
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.DoMerge();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.US_UC_NKCountryOfOrigin = "MX";
			invoiceLine1.US_UC_NKCountryOfExport = "AU";
			invoiceLine1.US_SPI = "MX";
			AssertHasMessageError(invoiceLine1.US_SPIInfo, "Mexican Special Rate under NAFTA requires that goods are originated from Mexico and exported from a NAFTA country.");
			invoiceLine1.US_UC_NKCountryOfExport = "MX";
			AssertNoMessageError(invoiceLine1.US_SPIInfo, "Mexican Special Rate under NAFTA requires that goods are originated from Mexico and exported from a NAFTA country.");
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.US_UC_NKCountryOfOrigin = "XO";
			invoiceLine2.US_UC_NKCountryOfExport = "AU";
			invoiceLine2.US_SPI = "CA";
			AssertHasMessageError(invoiceLine2.US_SPIInfo, "Canadian Special Rate under NAFTA requires that goods are originated from Canada and exported from a NAFTA country.");
			invoiceLine2.US_UC_NKCountryOfExport = "CA";
			AssertNoMessageError(invoiceLine2.US_SPIInfo, "Canadian Special Rate under NAFTA requires that goods are originated from Canada and exported from a NAFTA country.");
		}

		public void TestNAFTAMXAndCAExportCountry_SpiS()
		{
			var declaration = new DeclarationTestHelper().CreateSimpleImportDeclaration(Factory);
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.DoMerge();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.US_UC_NKCountryOfOrigin = "XO";
			invoiceLine1.US_UC_NKCountryOfExport = "AU";
			invoiceLine1.US_SPI = "S";
			AssertHasMessageError(invoiceLine1.US_SPIInfo, "USMCA (Originating) requires that goods are originated from Canada or Mexico and exported from a NAFTA country.");
			invoiceLine1.US_UC_NKCountryOfExport = "CA";
			AssertNoMessageError(invoiceLine1.US_SPIInfo, "USMCA (Originating) requires that goods are originated from Canada or Mexico and exported from a NAFTA country.");
			invoiceLine1.US_UC_NKCountryOfOrigin = "XO";
			invoiceLine1.US_UC_NKCountryOfExport = "AU";
			invoiceLine1.US_SPI = "S+";
			AssertHasMessageError(invoiceLine1.US_SPIInfo, "USMCA (Qualifying) requires that goods are originated from Canada or Mexico and exported from a NAFTA country.");
			invoiceLine1.US_UC_NKCountryOfExport = "CA";
			AssertNoMessageError(invoiceLine1.US_SPIInfo, "USMCA (Qualifying) requires that goods are originated from Canada or Mexico and exported from a NAFTA country.");
		}

		public void TestNoNAFTACheckWhenSPIIsSAndSPlus()
		{
			var declaration = new DeclarationTestHelper().CreateSimpleImportDeclaration(Factory);
			var importer = Factory.New<OrgHeader>();
			declaration.IOROrgPK = importer.PK;
			declaration.JE_OH_Importer = importer.PK;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.DoMerge();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "ProductForTesting";
			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = importer.PK;
			relOrg.OU_Relationship = "OWN";
			invoiceLine1.JI_PartNo = product.OP_PartNum;
			AssertNotNull(invoiceLine1.Part);
			var partHasRequiredDocuments = (IHaveRequiredDocuments)invoiceLine1.Part;
			var nafDoc = partHasRequiredDocuments.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.Nafta);
			nafDoc.EQ_ValidToDate = ZDateTime.Now.AddDays(-20);
			invoiceLine1.US_SPI = "CA";
			AssertHasMessageErrorContaining(invoiceLine1.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.CheckNAFTACertificate);
			invoiceLine1.US_SPI = "S";
			AssertNoMessageErrorContaining(invoiceLine1.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.CheckNAFTACertificate);
			invoiceLine1.US_SPI = "MX";
			AssertHasMessageErrorContaining(invoiceLine1.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.CheckNAFTACertificate);
			invoiceLine1.US_SPI = "S+";
			AssertNoMessageErrorContaining(invoiceLine1.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.CheckNAFTACertificate);
		}

		public void TestFDAIndicatorsWhenACEFDAIsRelevant()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			invoiceLine.Declaration.US_CertifyCargoRelease = true;
			invoiceLine.JI_Tariff = USCTariff.FDAAdmissibilityReviewRequiredTariff;
			invoiceLine.Declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			invoiceLine.Declaration.US_CertifyCargoRelease = true;
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			AssertHasMessageError("Validation should still run for ACS certification", invoiceLine.US_FDAIndicatorInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.FDA.FDALineRequired);
			invoiceLine.Declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			AssertNoMessageError("Should not run validations on US_FDAIndicator as ACE FDA is effective", invoiceLine.US_FDAIndicatorInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.FDA.FDALineRequired);
		}

		public void TestCheck_SupTariffDoesntHaveNotificationWithSTNTariff()
		{
			var tariffNumber1 = "99003355";
			var tariffNumber2 = "8822330000";
			var tariffUS = Factory.New<USCTariff>();
			tariffUS.UE_Tariff = tariffNumber2;
			tariffUS.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariffUS.UE_DateTo = ZDateTime.MaxSmallDateTime;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var startDate = new ZDateTime(2016, 01, 01);
			var endDate = new ZDateTime(2079, 01, 01);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			var stnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, TariffRuleList.Codes.EligibleForSecondaryTariffNumbers);
			Factory.Save();
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, tariffNumber1, new ZDateTime(2010, 12, 29), new ZDateTime(2079, 06, 06), "Description A99", 1);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariff1);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			var header = declaration.Invoices.AddNew();
			var invoiceLine = header.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariffNumber2;
			invoiceLine.US_SupTariff = tariffNumber1;
			AssertHasMessageErrorContaining(invoiceLine.US_SupTariffInfo, CommonImportAddInfoJobComInvoiceLineValidation.ProvTariffIsNotAllowed);
			invoiceLine.JI_Tariff = string.Empty;
			invoiceLine.US_SupTariff = string.Empty;
			var tariffNumber3 = "8855440000";
			var tariffUS3 = Factory.New<USCTariff>();
			tariffUS3.UE_Tariff = tariffNumber3;
			tariffUS3.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariffUS3.UE_DateTo = ZDateTime.MaxSmallDateTime;
			var tariffRule3 = Factory.New<USCTariffRule>();
			tariffRule3.U1_Tariff = tariffNumber3;
			tariffRule3.U1_RuleCode = TariffRuleList.Codes.EligibleForSecondaryTariffNumbers;
			tariffRule3.U1_DateFrom = startDate;
			tariffRule3.U1_DateTo = endDate;
			var ruleSecondaryTariff = tariffRule3.SecondaryTariffs.AddNew();
			ruleSecondaryTariff.U3_TariffFrom = "99003355";
			ruleSecondaryTariff.U3_DateFrom = startDate;
			ruleSecondaryTariff.U3_DateTo = endDate;
			Factory.Save();
			invoiceLine.JI_Tariff = tariffNumber3;
			invoiceLine.US_SupTariff = tariffNumber1;
			AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, CommonImportAddInfoJobComInvoiceLineValidation.ProvTariffIsNotAllowed);
		}

		public void TestCheckUS_SecondarySPI_H()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9999.00.60";
			invoiceLine.JI_Tariff = "6203.42.2005";
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.H;
			AssertNoNotifications(invoiceLine.US_SecondarySPIInfo);
		}

		public void TestCheckOGAIndicator()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_CertifyCargoRelease = true;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			AssertNoMessageError("No notification for FDA. FDA is reportable both in ACE and ACS certification mode", invoiceLine.US_FDAIndicatorInfo, FormalImportAddInfoJobDeclarationValidation.ACECertificationModeMessageError);
			invoiceLine.US_FCCIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertNoMessageError("No notification for FCC. FCC is entry summary data, not OGA data", invoiceLine.US_FCCIndicatorInfo, FormalImportAddInfoJobDeclarationValidation.ACECertificationModeMessageError);
			invoiceLine.US_DOTIndicator = OGAIndicatorList.Codes.Declared;
			AssertHasMessageError(invoiceLine.US_DOTIndicatorInfo, FormalImportAddInfoJobDeclarationValidation.ACECertificationModeMessageError);
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			invoiceLine.US_DOTIndicator = OGAIndicatorList.Codes.Declared;
			AssertNoMessageError(invoiceLine.US_DOTIndicatorInfo, FormalImportAddInfoJobDeclarationValidation.ACECertificationModeMessageError);
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			AssertNoMessageError(invoiceLine.US_FDAIndicatorInfo, FormalImportAddInfoJobDeclarationValidation.ACECertificationModeMessageError);
			invoiceLine.US_DOTIndicator = OGAIndicatorList.Codes.Declared;
			AssertNoMessageError(invoiceLine.US_DOTIndicatorInfo, FormalImportAddInfoJobDeclarationValidation.ACECertificationModeMessageError);
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			AssertNoMessageError(invoiceLine.US_FDAIndicatorInfo, FormalImportAddInfoJobDeclarationValidation.ACECertificationModeMessageError);
			invoiceLine.US_DOTIndicator = OGAIndicatorList.Codes.Declared;
			AssertHasMessageError(invoiceLine.US_DOTIndicatorInfo, FormalImportAddInfoJobDeclarationValidation.ACECertificationModeMessageError);
		}

		public void TestCheckFTZPGAIndicator()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.US_EnableSPN = true;
			declaration.US_F_PNMode = PriorNoticeModeCodeList.Codes.P;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			AssertHasMessageError(invoiceLine.US_FDAIndicatorInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.FDA.FDALineRequired);
			invoiceLine.ACE_FDALines.AddNew();
			invoiceLine.AddInfoValidation.ValidateUS_FDAIndicator();
			AssertNoMessageError(invoiceLine.US_FDAIndicatorInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.FDA.FDALineRequired);
			declaration.US_F_PNMode = PriorNoticeModeCodeList.Codes.O;
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			AssertHasMessageError(invoiceLine2.US_FDAIndicatorInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.FDA.FDALineRequired);
			invoiceLine2.FDAs.AddNew();
			invoiceLine2.AddInfoValidation.ValidateUS_FDAIndicator();
			AssertNoMessageError(invoiceLine2.US_FDAIndicatorInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.FDA.FDALineRequired);
		}

		public void TestRestrictedSPIAndTariff()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var importer = Factory.New<OrgHeader>();
			declaration.IOROrgPK = importer.PK;
			declaration.IORWrapper.RestrictedSPIs.AddNew(RestrictedCodeTypeList.Codes.RestrictedSPI, PrimarySpecProgramIndicatorList.Codes.C);
			declaration.IORWrapper.RestrictedSPIs.AddNew(RestrictedCodeTypeList.Codes.RestrictedSPI, SpecialProgramList.Codes.BH);
			declaration.IORWrapper.RestrictedTariffs.AddNew(RestrictedCodeTypeList.Codes.RestrictedTariff, "9813");
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var message = "is not allowed for Importer of Record. For more details please refer to Importer of Record > Details > Config > US Defaults";
			invoiceLine.US_SPI = SpecialProgramList.Codes.BH;
			AssertHasMessageErrorContaining(invoiceLine.US_SPIInfo, message);
			invoiceLine.US_SecondarySPI = PrimarySpecProgramIndicatorList.Codes.C;
			AssertHasMessageErrorContaining(invoiceLine.US_SecondarySPIInfo, message);
			invoiceLine.US_SupTariff = "98130075";
			AssertHasMessageErrorContaining(invoiceLine.US_SupTariffInfo, message);
			invoiceLine.US_SPI = SpecialProgramList.Codes.BSharp;
			AssertNoMessageErrorContaining(invoiceLine.US_SPIInfo, message);
			invoiceLine.US_SecondarySPI = PrimarySpecProgramIndicatorList.Codes.D;
			AssertNoMessageErrorContaining(invoiceLine.US_SecondarySPIInfo, message);
			invoiceLine.US_SupTariff = "98140075";
			AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, message);
		}

		public void TestCheckUS_AMMV()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLineX = invoice.JobComInvoiceLines.AddNew();
			invoiceLineX.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLineX.US_AMMVPerUnit = 10m;
			AssertHasMessageError(invoiceLineX.US_AMMVPerUnitInfo, FormalImportAddInfoJobComInvoiceLineValidation.AMMVNotApplicableForXLine);
			invoiceLineX.US_AMMVPerUnit = 0m;
			AssertNoMessageError(invoiceLineX.US_AMMVPerUnitInfo, FormalImportAddInfoJobComInvoiceLineValidation.AMMVNotApplicableForXLine);
			var invoiceLineV = invoice.JobComInvoiceLines.AddNew();
			invoiceLineV.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			invoiceLineV.US_AMMVPerUnit = 10m;
			AssertNoMessageError(invoiceLineV.US_AMMVPerUnitInfo, FormalImportAddInfoJobComInvoiceLineValidation.AMMVNotApplicableForXLine);
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			invoiceLineX.US_AMMVPerUnit = 10m;
			AssertHasMessageError(invoiceLineX.US_AMMVPerUnitInfo, FormalImportAddInfoJobComInvoiceLineValidation.AMMVNotApplicableForXLine);
			invoiceLineX.US_AMMVPerUnit = 0m;
			AssertNoMessageError(invoiceLineX.US_AMMVPerUnitInfo, FormalImportAddInfoJobComInvoiceLineValidation.AMMVNotApplicableForXLine);
			invoiceLineV.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			invoiceLineV.US_AMMVPerUnit = 10m;
			AssertNoMessageError(invoiceLineV.US_AMMVPerUnitInfo, FormalImportAddInfoJobComInvoiceLineValidation.AMMVNotApplicableForXLine);
			invoiceLineX.US_AMMVPercentage = 10m;
			AssertHasMessageError(invoiceLineX.US_AMMVPercentageInfo, FormalImportAddInfoJobComInvoiceLineValidation.AMMVNotApplicableForXLine);
		}

		public void TestCheckUS_TransactionsRelated()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			declaration.US_TransactionsRelated = "";
			invoice.US_TransactionsRelated = "~";
			AssertHasMessageError(invoice.US_TransactionsRelatedInfo, ListValidation.InvalidCodeMessageError);
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_TransactionsRelated = "!";
			AssertHasMessageError(invoiceLine.US_TransactionsRelatedInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			invoice.US_TransactionsRelated = "";
			AssertNoMessageError(invoice.US_TransactionsRelatedInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(invoice.US_TransactionsRelatedInfo, CommonImportAddInfoJobComInvoiceLineValidation.TransactionRelatedRequiredForNonInformalEntry);
			invoiceLine.US_TransactionsRelated = "";
			AssertNoMessageError(invoiceLine.US_TransactionsRelatedInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(invoiceLine.US_TransactionsRelatedInfo, CommonImportAddInfoJobComInvoiceLineValidation.TransactionRelatedRequiredForNonInformalEntry);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			invoice.AddInfoValidation.ValidateUS_TransactionsRelated();
			AssertNoMessageError(invoice.US_TransactionsRelatedInfo, CommonImportAddInfoJobComInvoiceLineValidation.TransactionRelatedRequiredForNonInformalEntry);
			invoiceLine.AddInfoValidation.ValidateUS_TransactionsRelated();
			AssertHasMessageError(invoiceLine.US_TransactionsRelatedInfo, CommonImportAddInfoJobComInvoiceLineValidation.TransactionRelatedRequiredForNonInformalEntry);
			invoiceLine.US_TransactionsRelated = Enterprise.MasterFiles.Business.Customs.RelatedPartyList.Codes.Related;
			AssertNoMessageError(invoiceLine.US_TransactionsRelatedInfo, CommonImportAddInfoJobComInvoiceLineValidation.TransactionRelatedRequiredForNonInformalEntry);
		}

		[TestDate(2015, 01, 12)]
		public virtual void TestCheckUS_MiscPermitNo()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableSPN = true;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "7221000075";
			invoiceLine.US_MiscPermitNo = ZString.Empty;
			AssertNoMessageErrors(invoiceLine.US_MiscPermitNoInfo);
		}

		public void TestCheckUS_SupTariffWhenAttributeR99AndBBF()
		{
			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates);
			var tariffType = helper.CreateNewOrGetExistingTariffType(dataGrouping.ZZZ_DataGrouping, Universal.Constants.TariffTypes.HarmonizedSystem);
			var parent = helper.LoadOrCreateNewTariff(dataGrouping.ZZZ_DataGrouping, tariffType.PK, "1111111111", startDate, endDate);
			var child = helper.LoadOrCreateNewTariff(dataGrouping.ZZZ_DataGrouping, tariffType.PK, "2222222222", startDate, endDate);
			var attribute11 = helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.RussianTariffs, child);
			var attribute12 = helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values.BabyFomula, child);
			var relation11 = helper.CreateTariffRelationship(child.PK, tariffType.PK, parent.ZZ1_TariffCode);

			var rateType = helper.CreateNewOrGetExistingRateType(dataGrouping.ZZZ_DataGrouping, Universal.Constants.RateTypes.Duty);
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RateCodes.Codes.Duty, rateType.PK);
			var rate01 = helper.CreateRefCusRate(child.PK, rateCode.PK, startDate, endDate);
			var applicability01 = helper.CreateCusApplicability(rate01.PK, tradeGroup, startDate, endDate);

			Factory.Save();

			var uscTariff01 = Factory.New<USCTariff>();
			uscTariff01.UE_Tariff = parent.ZZ1_TariffCode;
			uscTariff01.UE_DateFrom = parent.ZZ1_StartDate;
			uscTariff01.UE_DateTo = parent.ZZ1_EndDate;

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = parent.ZZ1_TariffCode;
			invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
			AssertEquals(2, invoiceLine.ApplicableSupTariffs.Length);
			AssertEquals(ZString.Empty, invoiceLine.US_SupTariff);
			AssertHasMessageError(invoiceLine.US_SupTariffInfo, CommonImportAddInfoJobComInvoiceLineValidation.FormulaActTariffOrNARequired);

			invoiceLine.US_SupTariff = invoiceLine.ApplicableSupTariffs[0].Tariff;
			invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
			AssertNoMessageError(invoiceLine.US_SupTariffInfo, CommonImportAddInfoJobComInvoiceLineValidation.FormulaActTariffOrNARequired);

			invoiceLine.US_SupTariff = invoiceLine.ApplicableSupTariffs[1].Tariff;
			invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
			AssertNoMessageError(invoiceLine.US_SupTariffInfo, CommonImportAddInfoJobComInvoiceLineValidation.FormulaActTariffOrNARequired);
		}

		public void TestCheckUS_SupTariffWhenGAEOr232Attribute()
		{
			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;

			var uscTariffNoGAE = Factory.New<USCTariff>();
			uscTariffNoGAE.UE_Tariff = "3344433222";
			uscTariffNoGAE.UE_DateFrom = startDate;
			uscTariffNoGAE.UE_DateTo = endDate;

			var uscTariff = Factory.New<USCTariff>();
			uscTariff.UE_Tariff = "3333333333";
			uscTariff.UE_DateFrom = startDate;
			uscTariff.UE_DateTo = endDate;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariffNoGAE = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "33444332221", startDate, endDate);
			var tariff3333 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "3333333333", startDate, endDate);
			var tariff99With301 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "5555555555", startDate, endDate);
			var tariff99With232 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "7777777777", startDate, endDate);
			var attribute1 = helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values.GAE, tariff3333);
			var attribute4 = helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariff99With301);
			var attribute5 = helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values._301, tariff99With301);
			var attribute6 = helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariff99With232);
			var attribute7 = helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values._232, tariff99With232);
			var relationship2 = helper.CreateTariffRelationship(tariff99With301.PK, tariffType.PK, tariff3333.ZZ1_TariffCode);
			var relationship3 = helper.CreateTariffRelationship(tariff99With232.PK, tariffType.PK, tariff3333.ZZ1_TariffCode);

			var relationship4 = helper.CreateTariffRelationship(tariff99With301.PK, tariffType.PK, tariffNoGAE.ZZ1_TariffCode);
			var relationship5 = helper.CreateTariffRelationship(tariff99With232.PK, tariffType.PK, tariffNoGAE.ZZ1_TariffCode);

			var dutyRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, Universal.Constants.RateTypes.Duty, dutyRateType.PK);
			var rate3 = helper.CreateRate(tariff99With301, rateCode.PK, startDate, endDate, "0");
			var rate4 = helper.CreateRate(tariff99With232, rateCode.PK, startDate, endDate, "0");

			var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.HongKong, Core.Constants.CountryCodes.HongKong, startDate, endDate);
			var tradeGroupCountry = helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.HongKong, startDate.Date, endDate.Date);
			var applicability1 = helper.CreateCusApplicability(rate3, tradeGroup, startDate, endDate);
			var applicability2 = helper.CreateCusApplicability(rate4, tradeGroup, startDate, endDate);

			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.HongKong;
			invoiceLine.JI_Tariff = tariff3333.ZZ1_TariffCode;
			AssertEquals(tariff3333.ZZ1_TariffCode, invoiceLine.JI_Tariff);
			AssertEquals(tariff99With301.ZZ1_TariffCode, invoiceLine.US_SupTariff);
			AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, CommonImportAddInfoJobComInvoiceLineValidation.ProvTariffIsNotAllowed);
			invoiceLine.US_SupTariff = string.Empty;
			invoiceLine.AddInfoValidation.ValidateUS_SupTariff();

			invoiceLine.JI_Tariff = tariffNoGAE.ZZ1_TariffCode;
			AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, CommonImportAddInfoJobComInvoiceLineValidation.ProvTariffIsNotAllowed);
			invoiceLine.US_SupTariff = tariff99With232.ZZ1_TariffCode;
			invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
			AssertHasMessageErrorContaining(invoiceLine.US_SupTariffInfo, CommonImportAddInfoJobComInvoiceLineValidation.ProvTariffIsNotAllowed);
		}

		public void TestCheckUS_SupTariffWhenTariffHasGAEAttribute()
		{
			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;
			var uscTariffNoGAE = Factory.New<USCTariff>();
			uscTariffNoGAE.UE_Tariff = "3344433222";
			uscTariffNoGAE.UE_DateFrom = startDate;
			uscTariffNoGAE.UE_DateTo = endDate;

			var uscTariff = Factory.New<USCTariff>();
			uscTariff.UE_Tariff = "3333333333";
			uscTariff.UE_DateFrom = startDate;
			uscTariff.UE_DateTo = endDate;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType("US", "HSN");
			var tariff1 = helper.LoadOrCreateNewTariff("US", tariffType.PK, "3333333333", startDate, endDate);
			var tariff3 = helper.LoadOrCreateNewTariff("US", tariffType.PK, "5555555555", startDate, endDate);
			var tariffNoGAE = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "3344433222", startDate, endDate);
			var attribute1 = helper.CreateTariffAttribute("TYPE", "GAE", tariff1);
			var attribute4 = helper.CreateTariffAttribute("RULE", "A99", tariff3);
			var relationship2 = helper.CreateTariffRelationship(tariff3.PK, tariffType.PK, tariff1.ZZ1_TariffCode);

			var relationship4 = helper.CreateTariffRelationship(tariff1.PK, tariffType.PK, tariffNoGAE.ZZ1_TariffCode);
			var relationship5 = helper.CreateTariffRelationship(tariff3.PK, tariffType.PK, tariffNoGAE.ZZ1_TariffCode);

			var dutyRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DTY", dutyRateType.PK);
			var rate1 = helper.CreateRate(tariff1, rateCode.PK, startDate, endDate, "0");
			var rate3 = helper.CreateRate(tariff3, rateCode.PK, startDate, endDate, "0");
			var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.HongKong, "HK", startDate, endDate);
			var tradeGroupCountry = helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.HongKong, startDate.Date, endDate.Date);
			var applicability1 = helper.CreateCusApplicability(rate1, tradeGroup, startDate, endDate);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_ProductExclusion = "";
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.HongKong;
			var validation = invoiceLine.AddInfoValidation;
			validation.ValidateUS_SupTariff();
			AssertEquals("", invoiceLine.JI_Tariff);
			AssertEquals("", invoiceLine.US_SupTariff);
			AssertNoMessageErrors(invoiceLine.US_SupTariffInfo);

			invoiceLine.JI_Tariff = tariffNoGAE.ZZ1_TariffCode;
			invoiceLine.US_ProductExclusion = AdditionalDeclarationTypeCodeList.Codes._02;
			validation.ValidateUS_SupTariff();
			AssertEquals("", invoiceLine.US_SupTariff);
			AssertNoMessageErrors(invoiceLine.US_SupTariffInfo);
			invoiceLine.US_SupTariff = tariff3.ZZ1_TariffCode;
			validation.ValidateUS_SupTariff();
			AssertHasMessageErrorContaining(invoiceLine.US_SupTariffInfo, CommonImportAddInfoJobComInvoiceLineValidation.ProvTariffIsNotAllowed);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var newHelper = new UniversalReferenceTestDataHelper(newFactory);
			var tariff2 = newHelper.LoadOrCreateNewTariff("US", tariffType.PK, "4444444444", startDate, endDate);
			var attribute2 = newHelper.CreateTariffAttribute("TYPE", "GAE", tariff2);
			var attribute3 = newHelper.CreateTariffAttribute("RULE", "A99", tariff2);
			var relationship1 = newHelper.CreateTariffRelationship(tariff2.PK, tariffType.PK, tariff1.ZZ1_TariffCode);
			var rate2 = newHelper.CreateRate(tariff2, rateCode.PK, startDate, endDate, "0");
			var applicability2 = newHelper.CreateCusApplicability(rate2, tradeGroup, startDate, endDate);
			newFactory.Save();
			var newLine = newFactory.Load<JobComInvoiceLine>(invoiceLine.PK);
			newLine.JI_Tariff = "";
			newLine.JI_Tariff = tariff1.ZZ1_TariffCode;
			newLine.AddInfoValidation.ValidateUS_SupTariff();
			AssertEquals(tariff1.ZZ1_TariffCode, newLine.JI_Tariff);
			AssertEquals(tariff2.ZZ1_TariffCode, newLine.US_SupTariff);
			AssertNoMessageErrorContaining(newLine.US_SupTariffInfo, CommonImportAddInfoJobComInvoiceLineValidation.ProvTariffIsNotAllowed);
		}

		public void TestCheckUS_SupTariffNoMessageForSetWith9903CargoReleaseSEOnly()
		{
			SetupDataForTest();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.US_EstimatedEntryDate = new ZDateTime(2019, 11, 11);
			var invoice = declaration.Invoices.AddNew();
			var parentLine = invoice.JobComInvoiceLines.AddNew();
			parentLine.JI_Tariff = "8215200000";
			parentLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			parentLine.AddInfoValidation.ValidateUS_SupTariff();
			AssertHasMessageError(parentLine.US_SupTariffInfo, CommonImportAddInfoJobComInvoiceLineValidation.ChildLineTariffNotEntered);
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;
			parentLine.AddInfoValidation.ValidateUS_SupTariff();
			AssertNoMessageError(parentLine.US_SupTariffInfo, CommonImportAddInfoJobComInvoiceLineValidation.ChildLineTariffNotEntered);
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			parentLine.AddInfoValidation.ValidateUS_SupTariff();
			AssertHasMessageError(parentLine.US_SupTariffInfo, CommonImportAddInfoJobComInvoiceLineValidation.ChildLineTariffNotEntered);
			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;
			var uscTariff = Factory.New<USCTariff>();
			uscTariff.UE_Tariff = "3333333333";
			uscTariff.UE_DateFrom = startDate;
			uscTariff.UE_DateTo = endDate;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType("US", "HSN");
			var tariff1 = helper.LoadOrCreateNewTariff("US", tariffType.PK, "3333333333", startDate, endDate);
			var attribute1 = helper.CreateTariffAttribute("TYPE", "GAE", tariff1);
			Factory.Save();
			parentLine.JI_Tariff = "3333333333";
			parentLine.AddInfoValidation.ValidateUS_SupTariff();
			AssertNoMessageError(parentLine.US_SupTariffInfo, CommonImportAddInfoJobComInvoiceLineValidation.ChildLineTariffNotEntered);
		}

		public void TestCheckUS_SupTariffForSetWithNo9903Tariffs()
		{
			SetupDataForTest();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EstimatedEntryDate = new ZDateTime(2019, 11, 11);
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = false;
			declaration.US_CertifyCargoRelease = false;
			var invoice = declaration.Invoices.AddNew();
			var parentLine = invoice.JobComInvoiceLines.AddNew();
			parentLine.JI_Tariff = "8215200000";
			parentLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			parentLine.AddInfoValidation.ValidateUS_SupTariff();
			AssertHasMessageError(parentLine.US_SupTariffInfo, CommonImportAddInfoJobComInvoiceLineValidation.ChildLineTariffNotEntered);
			parentLine.US_SupTariff = "99038803";
			AssertNoMessageError(parentLine.US_SupTariffInfo, CommonImportAddInfoJobComInvoiceLineValidation.ChildLineTariffNotEntered);
			var childLine = invoice.JobComInvoiceLines.AddNew();
			childLine.JI_Tariff = "8215994030";
			parentLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			childLine.JI_ParentID = parentLine.PK;
			childLine.AddInfoValidation.ValidateUS_SupTariff();
			AssertNoMessageError(childLine.US_SupTariffInfo, CommonImportAddInfoJobComInvoiceLineValidation.ChildLineTariffNotEntered);
		}

		public void TestCheckUS_SupTariffForSetWithWashingMachineTariffs()
		{
			var startDate = new ZDateTime(2019, 01, 01);
			var endDate = new ZDateTime(2079, 06, 06);
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "8450020080";
			tariff.UE_DateFrom = startDate;
			tariff.UE_DateTo = endDate;
			var tariff3 = Factory.New<USCTariff>();
			tariff3.UE_Tariff = "8215200000";
			tariff3.UE_DateFrom = startDate;
			tariff3.UE_DateTo = endDate;
			SetupDataForTest();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = false;
			declaration.US_CertifyCargoRelease = false;
			declaration.US_EstimatedEntryDate = new ZDateTime(2019, 11, 11);
			var invoice = declaration.Invoices.AddNew();
			var parentLine = invoice.JobComInvoiceLines.AddNew();
			parentLine.JI_Tariff = "8215200000";
			parentLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			AssertEquals(ZString.Empty, parentLine.US_SupTariff);
			parentLine.AddInfoValidation.ValidateUS_SupTariff();
			AssertHasMessageError(parentLine.US_SupTariffInfo, CommonImportAddInfoJobComInvoiceLineValidation.ChildLineTariffNotEntered);
			parentLine.JI_Tariff = "8450020080";
			parentLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			AssertEquals(ZString.Empty, parentLine.US_SupTariff);
			parentLine.AddInfoValidation.ValidateUS_SupTariff();
			AssertNoMessageError(parentLine.US_SupTariffInfo, CommonImportAddInfoJobComInvoiceLineValidation.ChildLineTariffNotEntered);
			parentLine.JI_Tariff = "";
			var tariffNumber2 = "99034502";
			var tariffNumber3 = "8450020080";
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "HSN");
			var washerTariff2 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, tariffNumber2, startDate, endDate);
			var dutyRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DTY", dutyRateType.PK);
			var rate4 = helper.CreateRate(washerTariff2, rateCode.PK, startDate, endDate, "0");
			helper.CreateTariffRelationship(washerTariff2.PK, hsnTariffType.PK, tariffNumber3);
			helper.CreateTariffAttribute("RULE", "A99", washerTariff2);
			if (tradeGroup == null)
			{
				tradeGroup = Factory.New<CusRefTradeGroupView>();
				helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.China);
				tradeGroup.ZZA_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.China;
				tradeGroup.ZZA_TradeGroup = Core.Constants.CountryCodes.China;
				tradeGroup.ZZA_Description = (Core.Constants.CountryCodes.China + Core.Constants.CountryCodes.China + " DESC");
				tradeGroup.ZZA_StartDate = startDate;
				tradeGroup.ZZA_EndDate = endDate;
			}

			helper.CreateCusApplicability(rate4, tradeGroup, startDate, endDate);
			helper.CreateTariffAttribute("MAND", "N", washerTariff2);
			Factory.Save();
			parentLine.JI_Tariff = "8450020080";
			AssertEquals(ZString.Empty, parentLine.US_SupTariff);
			parentLine.AddInfoValidation.ValidateUS_SupTariff();
			AssertNoMessageError(parentLine.US_SupTariffInfo, CommonImportAddInfoJobComInvoiceLineValidation.ChildLineTariffNotEntered);
		}

		public void TestCheckUS_SupTariffForSetWithNotAssociated9903Tariffs()
		{
			SetupDataForTest();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EstimatedEntryDate = new ZDateTime(2019, 11, 11);
			var invoice = declaration.Invoices.AddNew();
			var parentLine = invoice.JobComInvoiceLines.AddNew();
			parentLine.JI_Tariff = "4901990093";
			parentLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			parentLine.US_SupTariff = "99038803";
			AssertHasMessageError(parentLine.US_SupTariffInfo, CommonImportAddInfoJobComInvoiceLineValidation.ProvTariffIsNotAllowed);
			AssertHasMessageError(parentLine.US_SupTariffInfo, CommonImportAddInfoJobComInvoiceLineValidation.ChildLineTariffNotFoundInRuleTariffs);
			parentLine.US_SupTariff = "99038815";
			AssertNoMessageError(parentLine.US_SupTariffInfo, CommonImportAddInfoJobComInvoiceLineValidation.ProvTariffIsNotAllowed);
			AssertNoMessageError(parentLine.US_SupTariffInfo, CommonImportAddInfoJobComInvoiceLineValidation.ChildLineTariffNotFoundInRuleTariffs);
			var childLine = invoice.JobComInvoiceLines.AddNew();
			childLine.JI_Tariff = "4823908600";
			childLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			childLine.JI_ParentID = parentLine.PK;
			childLine.AddInfoValidation.ValidateUS_SupTariff();
			AssertNoMessageError(childLine.US_SupTariffInfo, CommonImportAddInfoJobComInvoiceLineValidation.ProvTariffIsNotAllowed);
			AssertNoMessageError(childLine.US_SupTariffInfo, CommonImportAddInfoJobComInvoiceLineValidation.ChildLineTariffNotFoundInRuleTariffs);
		}

		public void TestValidateTariffAgainstXLineForLaceyAct()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1234567890";
			tariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff.UE_OGACodes = "AAAFD4";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine.LaceyActLines.AddNew();
			invoiceLine.JI_Tariff = "1234567890";
			AssertHasMessageError("Lacey Act data is not allowed on X line", invoiceLine.US_SecondarySPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.LaceyDataActIsNotNeededForXLine);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageError("Lacey Act data is not allowed on X line", invoiceLine.US_SetIndInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.LaceyDataActIsNotNeededForXLine);
		}

		[TestDate(2016, 09, 26)]
		public void TestNotEligibleForSPI()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1701991011";
			tariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1701991011";
			invoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Jordan;
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Jordan;
			Factory.Save();
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.A;
			AssertHasMessageErrorContaining(invoiceLine.US_SPIInfo, ListValidation.InvalidCodeMessageError);
			tariff.UE_SPICode = "A*E*J P BHCACLILJOMAMXOMSG";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var invoiceLineLoaded = newFactory.Load<JobComInvoiceLine>(invoiceLine.PK);
			invoiceLineLoaded.AddInfoValidation.ValidateUS_SPI();
			AssertNoMessageErrorContaining(invoiceLineLoaded.US_SPIInfo, ListValidation.InvalidCodeMessageError);

			var supTariff = Factory.New<USCTariff>();
			supTariff.UE_Tariff = "99031524";
			supTariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			supTariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			var tariffRuleQuery = new ZQuery(USCTariffRuleSchema.U1_RuleCode, TariffRuleList.Codes.InLieuTariffs);
			tariffRuleQuery.AddToFilter(USCTariffRuleSchema.U1_Tariff, "9903");
			var supTariffRule = Factory.LoadTop1<USCTariffRule>(tariffRuleQuery);
			if (supTariffRule == null)
			{
				supTariffRule = Factory.New<USCTariffRule>();
				supTariffRule.U1_RuleCode = TariffRuleList.Codes.InLieuTariffs;
				supTariffRule.U1_Tariff = "9903";
				supTariffRule.U1_DateFrom = new ZDateTime(2000, 1, 1);
			}

			var exceptionRuleQuery = new ZQuery(USCTariffRuleExceptionSchema.U2_Tariff, "990317");
			exceptionRuleQuery.AddToFilter(USCTariffRuleExceptionSchema.U2_TariffTo, "990318");
			var exceptionRule = Factory.LoadTop1<USCTariffRuleException>(exceptionRuleQuery);
			if (exceptionRule == null)
			{
				exceptionRule = Factory.New<USCTariffRuleException>();
				exceptionRule.U2_U1 = supTariffRule.PK;
				exceptionRule.U2_Tariff = "990317";
				exceptionRule.U2_TariffTo = "990318";
				exceptionRule.U2_DateFrom = new ZDateTime(2000, 1, 1);
				exceptionRule.U2_DateTo = new ZDateTime(2099, 12, 31);
			}

			Factory.Save();
			invoiceLine.US_SupTariff = "99031524";
			invoiceLine.AddInfoValidation.ValidateUS_SPI();
			AssertHasMessageErrorContaining(invoiceLine.US_SPIInfo, ListValidation.InvalidCodeMessageError);
			supTariff.UE_Tariff = "99031724";
			Factory.Save();
			invoiceLine.US_SupTariff = "99031724";
			invoiceLine.AddInfoValidation.ValidateUS_SPI();
			AssertNoMessageErrorContaining(invoiceLine.US_SPIInfo, ListValidation.InvalidCodeMessageError);
		}

		protected void AssertUS_OA_ManufacturerAddressForMID(JobComInvoiceLine invoiceLine)
		{
			invoiceLine.JI_OA_ManufacturerAddress = ZGuid.Empty;
			AssertHasMessageErrorContaining(invoiceLine.JI_OA_ManufacturerAddressInfo, MandatoryValidation.YouHaveNotEntered);
			var party = Factory.New<OrgHeader>();
			var mainAddress = party.MainAddress;
			string messageError = string.Format(OrganisationValidation.ManufacturerIDMissing, party.MainAddress.OA_Code);
			invoiceLine.JI_OA_ManufacturerAddress = mainAddress.PK;
			AssertNoMessageErrorContaining(invoiceLine.JI_OA_ManufacturerAddressInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, messageError);
			var cusCode = mainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "AU34567");
			invoiceLine.JI_OA_ManufacturerAddress = mainAddress.PK;
			AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, messageError);
		}

		protected void AssertUS_OA_ManufacturerAddressForMIDAgainstCanadianProvince(JobComInvoiceLine invoiceLine)
		{
			var party = Factory.New<OrgHeader>();
			var mainAddress = party.MainAddress;
			var cusCode = mainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "AU34567");
			invoiceLine.JI_OA_ManufacturerAddress = mainAddress.PK;
			cusCode.OK_CustomsRegNo = "AU34567";
			invoiceLine.JI_OA_ManufacturerAddress = mainAddress.PK;
			invoiceLine.US_UC_NKCountryOfExport = "CA";
			invoiceLine.US_UC_NKCountryOfOrigin = CanadaProvinceTerritoryCodes.Codes.XY;
			invoiceLine.AddInfoValidation.ValidateUS_UC_NKCountryOfOrigin();
			AssertHasMessageErrorContaining(invoiceLine.US_UC_NKCountryOfOriginInfo, ManufacturerIDValidator.Constants.Canadian);
			cusCode.OK_CustomsRegNo = "XO1";
			invoiceLine.JI_OA_ManufacturerAddress = mainAddress.PK;
			invoiceLine.AddInfoValidation.ValidateUS_UC_NKCountryOfOrigin();
			AssertNoMessageErrorContaining(invoiceLine.US_UC_NKCountryOfOriginInfo, ManufacturerIDValidator.Constants.Canadian);
		}

		protected void AssertUS_UC_NKCountryOfExport(JobComInvoiceLine invoiceLine)
		{
			invoiceLine.US_UC_NKCountryOfExport = "US";
			AssertHasMessageError(invoiceLine.US_UC_NKCountryOfExportInfo, ValidationConstants.Declaration.ImportEntryShouldNotHaveUSOrPRAsCountryOfExport);
			invoiceLine.Declaration.US_EntryType = "21";
			invoiceLine.AddInfoValidation.ValidateUS_UC_NKCountryOfExport();
			AssertNoMessageError(invoiceLine.US_UC_NKCountryOfExportInfo, ValidationConstants.Declaration.ImportEntryShouldNotHaveUSOrPRAsCountryOfExport);
			invoiceLine.Declaration.US_EntryType = "22";
			invoiceLine.AddInfoValidation.ValidateUS_UC_NKCountryOfExport();
			AssertNoMessageError(invoiceLine.US_UC_NKCountryOfExportInfo, ValidationConstants.Declaration.ImportEntryShouldNotHaveUSOrPRAsCountryOfExport);
			invoiceLine.Declaration.US_EntryType = "01";
			invoiceLine.US_UC_NKCountryOfExport = "PR";
			AssertHasMessageError(invoiceLine.US_UC_NKCountryOfExportInfo, ValidationConstants.Declaration.ImportEntryShouldNotHaveUSOrPRAsCountryOfExport);
			invoiceLine.Declaration.US_EntryType = "31";
			invoiceLine.AddInfoValidation.ValidateUS_UC_NKCountryOfExport();
			AssertNoMessageError(invoiceLine.US_UC_NKCountryOfExportInfo, ValidationConstants.Declaration.ImportEntryShouldNotHaveUSOrPRAsCountryOfExport);
			invoiceLine.US_UC_NKCountryOfExport = "~";
			AssertHasMessageError(invoiceLine.US_UC_NKCountryOfExportInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_UC_NKCountryOfExport = "AU";
			AssertNoMessageError(invoiceLine.US_UC_NKCountryOfExportInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_UC_NKCountryOfExport = CanadaProvinceTerritoryCodes.Codes.XA;
			AssertHasMessageError(invoiceLine.US_UC_NKCountryOfExportInfo, ExternalValidation.CanadianProvinceCodeNotAllowedForCountryOfExport);
			invoiceLine.US_UC_NKCountryOfExport = "CA";
			AssertNoMessageError(invoiceLine.US_UC_NKCountryOfExportInfo, ExternalValidation.CanadianProvinceCodeNotAllowedForCountryOfExport);
			invoiceLine.US_UC_NKCountryOfExport = ZString.Empty;
			AssertHasMessageError(invoiceLine.US_UC_NKCountryOfExportInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.CountryOfExport.CountryOfExportNeeded);
			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			Assert("Pre-condition", invoiceLine.Declaration.IsConsumptionFTZ);
			invoiceLine.US_UC_NKCountryOfExport = ZString.Empty;
			AssertNoMessageError(invoiceLine.US_UC_NKCountryOfExportInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.CountryOfExport.CountryOfExportNeeded);
		}

		protected void AssertUS_UC_NKCountryOfOrigin(JobComInvoiceLine invoiceLine)
		{
			invoiceLine.AddInfoValidation.ValidateUS_UC_NKCountryOfOrigin();
			AssertHasMessageErrors(invoiceLine.US_UC_NKCountryOfOriginInfo);
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			AssertNoMessageErrors(invoiceLine.US_UC_NKCountryOfOriginInfo);
			invoiceLine.InvoiceHeader.US_UC_NKCountryOfOrigin = "CA";
			invoiceLine.US_UC_NKCountryOfOrigin = "";
			AssertNoMessageErrors(invoiceLine.US_UC_NKCountryOfOriginInfo);
			invoiceLine.US_UC_NKCountryOfOrigin = "!!";
			AssertHasMessageErrors(invoiceLine.US_UC_NKCountryOfOriginInfo);
			invoiceLine.US_UC_NKCountryOfOrigin = USCCountry.Unknown;
			AssertNoMessageErrors(invoiceLine.US_UC_NKCountryOfOriginInfo);
			invoiceLine.InvoiceHeader.US_UC_NKCountryOfOrigin = "";
			JobComInvoiceLine secondaryLine = invoiceLine.InvoiceHeader.JobComInvoiceLines.AddNew();
			secondaryLine.US_UC_NKCountryOfOrigin = "";
			AssertHasMessageErrorContaining(secondaryLine.US_UC_NKCountryOfOriginInfo, MandatoryValidation.YouHaveNotEntered);
			secondaryLine.JI_ParentID = invoiceLine.PK;
			secondaryLine.AddInfoValidation.ValidateUS_UC_NKCountryOfOrigin();
			AssertNoMessageErrorContaining(secondaryLine.US_UC_NKCountryOfOriginInfo, MandatoryValidation.YouHaveNotEntered);
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000111122";
			tariff.UE_ISOCountryofOriginEditCode = "AU";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(100);
			invoiceLine.JI_Tariff = "0000111122";
			invoiceLine.US_UC_NKCountryOfOrigin = "NZ";
			AssertHasMessageError(invoiceLine.US_UC_NKCountryOfOriginInfo, string.Format(FormalImportAddInfoJobComInvoiceLineValidation.InvalidCountryOfOrigin, "AU"));
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			AssertNoMessageError(invoiceLine.US_UC_NKCountryOfOriginInfo, string.Format(FormalImportAddInfoJobComInvoiceLineValidation.InvalidCountryOfOrigin, "AU"));
			invoiceLine.US_UC_NKCountryOfExport = "CA";
			invoiceLine.US_UC_NKCountryOfOrigin = "CA";
			AssertHasMessageError(invoiceLine.US_UC_NKCountryOfOriginInfo, ExternalValidation.CAIsNotAValidCountryForCustomsMessagingPurpose);
			invoiceLine.US_UC_NKCountryOfOrigin = CanadaProvinceTerritoryCodes.Codes.XA;
			AssertNoMessageError(invoiceLine.US_UC_NKCountryOfOriginInfo, ExternalValidation.CAIsNotAValidCountryForCustomsMessagingPurpose);
		}

		protected void AssertUS_PrivilegedStatusDate(JobComInvoiceLine invoiceLine)
		{
			for (int i = 0; i < invoiceLine.AddInfoLookups.US_ZoneStatusList.Count; i++)
			{
				invoiceLine.US_PrivilegedStatusDate = ZDateTime.Invalid;
				invoiceLine.US_PrivilegedStatusDateInfo.ClearAllNotifications();
				invoiceLine.US_ZoneStatus = invoiceLine.AddInfoLookups.US_ZoneStatusList[i].Code;
				if (invoiceLine.AddInfoLookups.US_ZoneStatusList[i].Code == ZoneStatusList.Codes.PrivilegedForeign)
				{
					AssertHasNotifications("ValidateUS_PrivilegedStatusDate should be called here", invoiceLine.US_PrivilegedStatusDateInfo);
				}
				else
				{
					AssertNoNotifications("ValidateUS_PrivilegedStatusDate should be called here", invoiceLine.US_PrivilegedStatusDateInfo);
				}
			}
		}

		protected void AssertUS_SPI(JobComInvoiceLine invoiceLine)
		{
			var part = Factory.New<OrgSupplierPart>();
			var pivot = Factory.New<CusClassPartPivot>();
			part.OP_PartNum = "1337";
			invoiceLine.JI_OP = part.PK;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_OP = part.PK;
			invoiceLine.JI_PartNo = "1337";
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			AssertNoMessageError(invoiceLine.US_SPIInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.APlus;
			AssertHasMessageError(invoiceLine.US_SPIInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_SupTariff = "9802008042";
			invoiceLine.US_SPI = "D";
			AssertNotNull("PreCondition:Import Supplementary tariff should not be null", invoiceLine.ImportSupTariff);
			AssertEquals("IsEligibleForAGOATextile", true, invoiceLine.ImportSupTariff.IsEligibleForAGOATextileBenefits(invoiceLine.EffectiveDateForDutyRate));
			AssertHasMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.TPLClaimedAndNoSPINeeded);
			invoiceLine.US_SPI = "";
			AssertNoMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.TPLClaimedAndNoSPINeeded);
			invoiceLine.US_SupTariff = "9802008044";
			invoiceLine.US_SPI = "D";
			AssertNotNull("PreCondition:Import Supplementary tariff should not be null", invoiceLine.ImportSupTariff);
			AssertEquals("IsEligibleForAGOATextile", true, invoiceLine.ImportSupTariff.IsEligibleForCBTPATextileBenefits(invoiceLine.EffectiveDateForDutyRate));
			AssertHasMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.TPLClaimedAndNoSPINeeded);
			invoiceLine.US_SPI = "";
			AssertNoMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.TPLClaimedAndNoSPINeeded);
			invoiceLine.JI_Tariff = "0101100010";
			invoiceLine.US_UC_NKCountryOfOrigin = "";
			invoiceLine.US_SupTariff = "";
			invoiceLine.Declaration.US_EstimatedEntryDate = ZDateTime.Today;
			invoiceLine.US_SPI = "~";
			AssertHasMessageErrorContaining(invoiceLine.US_SPIInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_SPI = invoiceLine.AddInfoLookups.SPIList.Count > 0 ? invoiceLine.AddInfoLookups.SPIList[0].Code : "";
			AssertNoMessageErrorContaining(invoiceLine.US_SPIInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Australia;
			invoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Australia;
			invoiceLine.US_SPI = SpecialProgramList.Codes.AU;
			AssertNoMessageErrors(invoiceLine.US_SPIInfo);
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Algeria;
			AssertHasMessageErrorContaining(invoiceLine.US_SPIInfo, "Australia Free Trade Agreement requires that goods are exported and originated directly from Australia.");
			invoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Algeria;
			AssertHasMessageErrorContaining(invoiceLine.US_SPIInfo, "Australia Free Trade Agreement requires that goods are exported and originated directly from Australia.");
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Australia;
			invoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Australia;
			invoiceLine.US_SPI = invoiceLine.AddInfoLookups.SPIList.GetCodeFromDescription("Not Applicable");
			AssertNoMessageErrors(invoiceLine.US_SPIInfo);
		}

		protected void AssertUS_SecondarySPI(JobComInvoiceLine invoiceLine)
		{
			var message = "Secondary SPI, 'F' is valid only for entry types, 02/07/32/38.";
			var mesageForListValidation = "Please enter a valid Secondary SPI Code. The code you have selected is not in the Secondary SPI codes List.";
			invoiceLine.US_SecondarySPI = "~";
			AssertHasMessageError(invoiceLine.US_SecondarySPIInfo, mesageForListValidation);
			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.F;
			AssertHasMessageError(invoiceLine.US_SecondarySPIInfo, message);
			invoiceLine.Declaration.US_EntryType = "";
			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVDQuotaVisa;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.F;
			AssertNoMessageError(invoiceLine.US_SecondarySPIInfo, message);
			var testdecl = Factory.New<JobDeclaration>();
			testdecl.JE_MessageType = JobMessageTypeList.Codes.Import;
			testdecl.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			testdecl.US_EnableENS = true;
			testdecl.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			message = "Product Claim of 'F' should only be selected where a folklore agreement exists for the Country of Origin. The U.S. has folklore agreements with the following countries: BD, CO, IN, JP, KR, MY, MX, PK, PE, PH, TW and TH.";
			mesageForListValidation = "Please enter a valid Product Claim Code. The code you have selected is not in the Product Claim codes List.";
			var header = testdecl.Invoices.AddNew();
			var line = header.JobComInvoiceLines.AddNew();
			line.US_SecondarySPI = "!";
			AssertHasMessageError(line.US_SecondarySPIInfo, mesageForListValidation);
			line.US_SecondarySPI = header.AddInfoLookups.ProductClaimList[0].Code;
			AssertNoMessageError(line.US_SecondarySPIInfo, mesageForListValidation);
			line.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Italy;
			line.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.F;
			AssertHasMessageError(line.US_SecondarySPIInfo, message);
			line.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Japan;
			line.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.F;
			AssertNoMessageError(line.US_SecondarySPIInfo, message);
		}

		protected void AssertUS_TexileCategoryNo(JobComInvoiceLine invoiceLine)
		{
			string tariff = "98206225";
			invoiceLine.JI_Tariff = "5407820090";
			AssertNoMessageError(invoiceLine.US_TextileCategoryNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.InvalidTextileCategory + "629");
			invoiceLine.US_TextileCategoryNo = "123";
			AssertHasMessageError(invoiceLine.US_TextileCategoryNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.InvalidTextileCategory + "629");
			invoiceLine.US_TextileCategoryNo = "";
			AssertHasWarning(invoiceLine.US_TextileCategoryNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.TariffSubjectToQuota);
			invoiceLine.JI_Tariff = "8512300030";
			invoiceLine.US_TextileCategoryNo = "111";
			AssertHasMessageError(invoiceLine.US_TextileCategoryNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.TariffNotSubjectToQuota);
			invoiceLine.US_TextileCategoryNo = "";
			AssertNoMessageError(invoiceLine.US_TextileCategoryNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.TariffNotSubjectToQuota);
			JobComInvoiceLine secondaryLine = invoiceLine.AddSecondaryInvoiceLine();
			secondaryLine.JI_Tariff = "6110121010";
			AssertNoWarning("Secondary Lines should not be validating Textile category", secondaryLine.US_TextileCategoryNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.TariffSubjectToQuota);
			AssertNoMessageError("Secondary Lines should not be validating Textile category", secondaryLine.US_TextileCategoryNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.TariffNotSubjectToQuota);
			USCTariff tariff_textile = Factory.New<USCTariff>();
			tariff_textile.UE_Tariff = tariff;
			tariff_textile.UE_DateFrom = new ZDate(2000, 1, 1);
			tariff_textile.UE_DateTo = ZDateTime.Today.AddDays(1);
			invoiceLine.JI_Tariff = tariff;
			secondaryLine.US_TextileCategoryNo = "";
			invoiceLine.US_TextileCategoryNo = "";
			AssertHasMessageError(invoiceLine.US_TextileCategoryNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.TextileCategoryIsMandatory);
			invoiceLine.US_TextileCategoryNo = "55";
			AssertNoMessageError(invoiceLine.US_TextileCategoryNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.TextileCategoryIsMandatory);
			secondaryLine.US_TextileCategoryNo = "764";
			invoiceLine.US_TextileCategoryNo = "";
			AssertNoMessageError(invoiceLine.US_TextileCategoryNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.TextileCategoryIsMandatory);
			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine.US_TextileCategoryNo = "123";
			AssertHasMessageError(invoiceLine.US_TextileCategoryNoInfo, string.Format(CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.DataIsNotNeededForXLine, CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.TextileCLassificationDetails));
		}

		protected void AssertTExtileCategoryNoAganstXLine(JobComInvoiceLine invoiceLine)
		{
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine.US_TextileCategoryNo = "123";
			AssertHasMessageError(invoiceLine.US_TextileCategoryNoInfo, string.Format(CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.DataIsNotNeededForXLine, CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.TextileCLassificationDetails));
		}

		void SetupDataForTest()
		{
			var tariffNumber1 = "99034501";
			var tariffNumber3 = "8450020080";
			var startDate = new ZDateTime(2019, 01, 01);
			var endDate = new ZDateTime(2079, 06, 06);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "HSN");
			Factory.Save();
			var washerTariff1 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, tariffNumber1, startDate, endDate);
			var washerTariff3 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, tariffNumber3, startDate, endDate);
			var progTariff1 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99038803", startDate, endDate);
			var progTariff2 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99038815", startDate, endDate);
			var dutyRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DTY", dutyRateType.PK);
			Factory.Save();
			var rate1 = helper.CreateRate(progTariff1, rateCode.PK, new ZDateTime(2019, 01, 01), new ZDateTime(2079, 06, 06), "0");
			var rate2 = helper.CreateRate(progTariff2, rateCode.PK, new ZDateTime(2019, 01, 01), new ZDateTime(2079, 06, 06), "0");
			var rate3 = helper.CreateRate(washerTariff1, rateCode.PK, startDate, endDate, "0");
			tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.China, "CN", new ZDateTime(2000, 01, 01), new ZDateTime(2079, 06, 06));
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.China, new ZDate(2000, 01, 01), new ZDate(2079, 06, 06));
			helper.CreateCusApplicability(rate1, tradeGroup, new ZDateTime(2019, 01, 01), new ZDateTime(2079, 06, 06));
			helper.CreateCusApplicability(rate2, tradeGroup, new ZDateTime(2019, 01, 01), new ZDateTime(2079, 06, 06));
			helper.CreateCusApplicability(rate3, tradeGroup, startDate, endDate);
			helper.CreateTariffRelationship(washerTariff1.PK, hsnTariffType.PK, tariffNumber3);
			helper.CreateTariffAttribute("RULE", "A99", washerTariff1);
			helper.CreateTariffAttribute("MAND", "N", washerTariff1);
			helper.CreateTariffRelationship(progTariff1.PK, hsnTariffType.PK, "4823908600");
			helper.CreateTariffRelationship(progTariff1.PK, hsnTariffType.PK, "8215200000");
			helper.CreateTariffRelationship(progTariff2.PK, hsnTariffType.PK, "8215200000");
			helper.CreateTariffRelationship(progTariff2.PK, hsnTariffType.PK, "4901990093");
			helper.CreateTariffAttribute("RULE", "A99", progTariff1);
			helper.CreateTariffAttribute("RULE", "A99", progTariff2);
			Factory.Save();
		}

		CusRefTradeGroupView tradeGroup;
	}
}

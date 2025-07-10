using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs.Universal;
using static Enterprise.Customs.US.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ACEImportJobComInvoiceLineValidationTest : FormalImportJobComInvoiceLineValidationTest
	{
		public void TestLinePriceForDerived()
		{
			var testHelper = new Chapter98HelperTest();

			var job = Factory.NewWithValidTestData<JobDeclaration>();
			job.JE_MessageType = JobMessageTypeList.Codes.Import;
			job.US_EntryFilerCode = "XJ5";
			job.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			job.US_EnableENS = true;
			var invoice = job.Invoices.AddNew();
			var parentLine = invoice.JobComInvoiceLines.AddNew();
			parentLine.JI_Tariff = "8215.20.0000";
			parentLine.US_SupTariff = testHelper.Test99038801Tariff.UE_Tariff;

			var childLine1 = invoice.JobComInvoiceLines.AddNew();
			childLine1.JI_ParentID = parentLine.PK;
			childLine1.JI_Tariff = "8215.99.3500"; //6.8%
			childLine1.JI_LinePrice = 2000;
			childLine1.US_SupTariff = ZString.Empty;
			Factory.Save();
			job.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			parentLine.JI_LinePrice = 1000m;
			AssertHasMessageError(parentLine.JI_LinePriceInfo, ACEImportJobComInvoiceLineValidation.PriceShouldBeZeroForDerived);

			parentLine.JI_LinePrice = 0m;
			AssertNoMessageError(parentLine.JI_LinePriceInfo, ACEImportJobComInvoiceLineValidation.PriceShouldBeZeroForDerived);
		}

		public void TestWarningWhenWatchTariffRequiredForCN()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "HSN");
			Factory.Save();

			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "0303530003", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariffAttribute = helper.CreateTariffAttribute("TYPE", "301CN", tariff);
			Factory.Save();

			USCTariffRule tariffRule = Factory.New<USCTariffRule>();
			tariffRule.U1_RuleCode = TariffRuleList.Codes.EligibleForSecondaryTariffNumbers;
			tariffRule.U1_DateFrom = ZDateTime.BrettsBirthday;
			tariffRule.U1_Tariff = "0303530003";

			USCRuleSecondaryTariff secondaryTariff = tariffRule.SecondaryTariffs.AddNew();
			secondaryTariff.U3_TariffFrom = "0303530002";
			secondaryTariff.U3_DateFrom = ZDateTime.BrettsBirthday;

			secondaryTariff = tariffRule.SecondaryTariffs.AddNew();
			secondaryTariff.U3_TariffFrom = "0303530009";
			secondaryTariff.U3_DateFrom = ZDateTime.BrettsBirthday;

			USCTariff tariffUS = Factory.New<USCTariff>();
			tariffUS.UE_Tariff = "0303530003";
			tariffUS.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariffUS.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariffUS.UE_AdditionalTariffNumberIndicator = true;

			Factory.Save();

			invoiceLine.JI_Tariff = "0303530003";
			invoiceLine.US_SupTariff = "0303530009";
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoWarning(invoiceLine.JI_TariffInfo, TariffValidator.WatchOrClockShouldReportSeparatelyWarning);

			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Kazakhstan;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasWarning(invoiceLine.JI_TariffInfo, TariffValidator.WatchOrClockShouldReportSeparatelyWarning);
		}

		public void TestMessageErrorWhenWatchTariffRequiredForNonCN()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "HSN");
			Factory.Save();

			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "0303530003", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariffAttribute = helper.CreateTariffAttribute("TYPE", "301CN", tariff);
			Factory.Save();

			USCTariffRule tariffRule = Factory.New<USCTariffRule>();
			tariffRule.U1_RuleCode = TariffRuleList.Codes.EligibleForSecondaryTariffNumbers;
			tariffRule.U1_DateFrom = ZDateTime.BrettsBirthday;
			tariffRule.U1_Tariff = "0303530003";

			USCRuleSecondaryTariff secondaryTariff = tariffRule.SecondaryTariffs.AddNew();
			secondaryTariff.U3_TariffFrom = "0303530002";
			secondaryTariff.U3_DateFrom = ZDateTime.BrettsBirthday;

			secondaryTariff = tariffRule.SecondaryTariffs.AddNew();
			secondaryTariff.U3_TariffFrom = "0303530009";
			secondaryTariff.U3_DateFrom = ZDateTime.BrettsBirthday;

			USCTariff tariffUS = Factory.New<USCTariff>();
			tariffUS.UE_Tariff = "0303530003";
			tariffUS.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariffUS.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariffUS.UE_AdditionalTariffNumberIndicator = true;

			Factory.Save();

			invoiceLine.JI_Tariff = "0303530003";
			invoiceLine.US_SupTariff = "0303530009";
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.HongKong;
			invoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Kazakhstan;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, TariffValidator.WatchOrClockShouldReportSeparatelyError);

			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageErrorContaining(invoiceLine.JI_TariffInfo, TariffValidator.WatchOrClockShouldReportSeparatelyError);
		}

		public void TestOriginOfCountryMXNeedsWarning()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "HSN");  // TariffType
			var ctrlType1 = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.UnitedStates, RefCusConditionTypes.ConditionClass.Control, TariffConditionTypes.Codes.IRCMF, "Test IRCMF"); // ConditionType
			Factory.Save();

			string warningMessage = "Prohibition exists on the importation into the United States from Mexico of all shrimp, curvina, sierra, and chano fish and fish products harvested by gillnets in the upper Gulf of California (UGC) within the vaquita’s geographic range.";

			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "0303590000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			var testCondCtrl1_1 = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, ctrlType1.PK, tariff.PK, "Fish: WARNING WARNING of Country of Origin", true, false, new ZDateTime(2018, 08, 15), ZDateTime.MaxSmallDateTime);
			testCondCtrl1_1.ZX1_Comment = warningMessage;
			testCondCtrl1_1.Factory.Save();

			var tradeGroupMX = helper.CreateTradeGroup(Core.Constants.CountryCodes.UnitedStates, "MX", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.AddCountry(tradeGroupMX, Core.Constants.CountryCodes.Mexico, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);

			var tradeGroupApplicability1 = helper.CreateCusApplicability(testCondCtrl1_1, tradeGroupMX, new ZDateTime(2018, 08, 15), ZDateTime.MaxSmallDateTime);
			tradeGroupApplicability1.ZZT_ZZ2_Rate = Guid.Empty;

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "TEST55";
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Description = "Test Warning message with HTS FOR MX";
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Mexico;
			invoiceLine.JI_Tariff = "7301100000";

			AssertNoWarningContaining(invoiceLine.JI_TariffInfo, warningMessage);

			invoiceLine.JI_Tariff = "0303590000";
			AssertHasWarningContaining(invoiceLine.JI_TariffInfo, warningMessage);
		}

		public void TestLinePriceForCombinedLines()
		{
			var testHelper = new CombinedLinesHelperTest();
			var testJob = testHelper.CombinedJob;
			var invoiceLine1 = testHelper.InvoiceHeaderForCombined.InvoiceLines.AddNew();
			var invoiceLine2 = testHelper.InvoiceHeaderForCombined.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = testHelper.Chapter98TestingHelper.Test99038801Tariff.UE_Tariff;
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			invoiceLine2.US_SupTariff = testHelper.Chapter98TestingHelper.Test99038802Tariff.UE_Tariff;
			invoiceLine2.JI_Tariff = testHelper.Chapter98TestingHelper.TestCTariff.UE_Tariff;
			invoiceLine2.JI_LinePrice = ZDecimal.Zero;
			testJob.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			invoiceLine1.Validation.ValidateJI_LinePrice();
			AssertNoMessageError(invoiceLine1.JI_LinePriceInfo, ACEImportJobComInvoiceLineValidation.PriceShouldBeZeroForCombinedLine);
			invoiceLine2.Validation.ValidateJI_LinePrice();
			AssertNoMessageError(invoiceLine2.JI_LinePriceInfo, ACEImportJobComInvoiceLineValidation.PriceShouldBeZeroForCombinedLine);

			invoiceLine1.JI_LinePrice = 100m;
			invoiceLine2.JI_LinePrice = 100m;
			invoiceLine1.Validation.ValidateJI_LinePrice();
			AssertHasMessageError(invoiceLine1.JI_LinePriceInfo, ACEImportJobComInvoiceLineValidation.PriceShouldBeZeroForCombinedLine);
			invoiceLine2.Validation.ValidateJI_LinePrice();
			AssertNoMessageError(invoiceLine2.JI_LinePriceInfo, ACEImportJobComInvoiceLineValidation.PriceShouldBeZeroForCombinedLine);
		}

		public void TestTariffForCombinedLines()
		{
			var testHelper = new CombinedLinesHelperTest();
			var testJob = testHelper.CombinedJob;
			var invoiceLine1 = testHelper.InvoiceHeaderForCombined.InvoiceLines.AddNew();
			var invoiceLine2 = testHelper.InvoiceHeaderForCombined.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = testHelper.Chapter98TestingHelper.Test99038801Tariff.UE_Tariff;
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			invoiceLine2.US_SupTariff = testHelper.Chapter98TestingHelper.Test99038802Tariff.UE_Tariff;
			invoiceLine2.JI_Tariff = testHelper.Chapter98TestingHelper.TestCTariff.UE_Tariff;
			invoiceLine2.JI_LinePrice = 1000m;
			testJob.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			invoiceLine1.Validation.ValidateJI_Tariff();
			AssertNoMessageError(invoiceLine1.JI_TariffInfo, ACEImportJobComInvoiceLineValidation.NormalCombieLineTariffOrderNo98Line);
			invoiceLine2.Validation.ValidateJI_Tariff();
			AssertNoMessageError(invoiceLine2.JI_TariffInfo, ACEImportJobComInvoiceLineValidation.NormalCombieLineTariffOrderNo98Line);

			invoiceLine1.JI_Tariff = testHelper.Chapter98TestingHelper.TestCTariff.UE_Tariff;
			invoiceLine2.JI_Tariff = ZString.Empty;
			invoiceLine1.Validation.ValidateJI_Tariff();
			AssertHasMessageError(invoiceLine1.JI_TariffInfo, ACEImportJobComInvoiceLineValidation.NormalCombieLineTariffOrderNo98Line);
			invoiceLine2.Validation.ValidateJI_Tariff();
			AssertHasMessageError(invoiceLine2.JI_TariffInfo, ACEImportJobComInvoiceLineValidation.NormalCombieLineTariffOrderNo98Line);

			invoiceLine1.JI_Tariff = ZString.Empty;
			invoiceLine1.US_SupTariff = testHelper.Chapter98TestingHelper.Test99038801Tariff.UE_Tariff;
			invoiceLine2.US_SupTariff = testHelper.Chapter98TestingHelper.Test9817002000Tariff.UE_Tariff;
			invoiceLine2.JI_Tariff = testHelper.Chapter98TestingHelper.TestCTariff.UE_Tariff;
			Factory.Save();
			invoiceLine1.Validation.ValidateJI_Tariff();
			AssertHasMessageError(invoiceLine1.JI_TariffInfo, ACEImportJobComInvoiceLineValidation.NormalCombieLineTariffOrder);
			invoiceLine2.Validation.ValidateJI_Tariff();
			AssertHasMessageError(invoiceLine2.JI_TariffInfo, ACEImportJobComInvoiceLineValidation.NormalCombieLineTariffOrder);

			invoiceLine1.US_SupTariff = testHelper.Chapter98TestingHelper.Test9817002000Tariff.UE_Tariff;
			invoiceLine2.US_SupTariff = testHelper.Chapter98TestingHelper.Test99038801Tariff.UE_Tariff;
			invoiceLine1.Validation.ValidateJI_Tariff();
			AssertNoMessageError(invoiceLine1.JI_TariffInfo, ACEImportJobComInvoiceLineValidation.NormalCombieLineTariffOrder);
			invoiceLine2.Validation.ValidateJI_Tariff();
			AssertNoMessageError(invoiceLine2.JI_TariffInfo, ACEImportJobComInvoiceLineValidation.NormalCombieLineTariffOrder);

			invoiceLine1.JI_Tariff = testHelper.Chapter98TestingHelper.TestCTariff.UE_Tariff;
			invoiceLine2.JI_Tariff = ZString.Empty;
			Factory.Save();
			invoiceLine1.Validation.ValidateJI_Tariff();
			AssertHasMessageError(invoiceLine1.JI_TariffInfo, ACEImportJobComInvoiceLineValidation.NormalCombieLineTariffOrder);
			invoiceLine2.Validation.ValidateJI_Tariff();
			AssertHasMessageError(invoiceLine2.JI_TariffInfo, ACEImportJobComInvoiceLineValidation.NormalCombieLineTariffOrder);

			testJob.US_EntryType = "23";
			invoiceLine1.US_SupTariff = testHelper.Chapter98TestingHelper.Test9813Tariff.UE_Tariff;
			invoiceLine2.US_SupTariff = testHelper.Chapter98TestingHelper.Test99038801Tariff.UE_Tariff;
			invoiceLine2.JI_Tariff = testHelper.Chapter98TestingHelper.TestCTariff.UE_Tariff;
			Factory.Save();
			invoiceLine1.Validation.ValidateJI_Tariff();
			AssertNoMessageError(invoiceLine1.JI_TariffInfo, ACEImportJobComInvoiceLineValidation.TIBTariffOrder);
			invoiceLine2.Validation.ValidateJI_Tariff();
			AssertNoMessageError(invoiceLine2.JI_TariffInfo, ACEImportJobComInvoiceLineValidation.TIBTariffOrder);

			invoiceLine1.JI_Tariff = testHelper.Chapter98TestingHelper.TestCTariff.UE_Tariff;
			invoiceLine2.JI_Tariff = ZString.Empty;
			Factory.Save();
			invoiceLine1.Validation.ValidateJI_Tariff();
			AssertHasMessageError(invoiceLine1.JI_TariffInfo, ACEImportJobComInvoiceLineValidation.TIBTariffOrder);
			invoiceLine2.Validation.ValidateJI_Tariff();
			AssertHasMessageError(invoiceLine2.JI_TariffInfo, ACEImportJobComInvoiceLineValidation.TIBTariffOrder);
		}

		public void TestCheckUS_OA_SoldToPartyAddress()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableCRL = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_OA_SoldToPartyAddress = ZGuid.Empty;
			invoiceLine.Validation.ValidateJI_OA_SoldToPartyAddress();
			AssertHasMessageErrorContaining(invoiceLine.JI_OA_SoldToPartyAddressInfo, MandatoryValidation.YouHaveNotEntered);

			var soldToParty = Factory.New<OrgHeader>();
			invoiceLine.JI_OA_SoldToPartyAddress = soldToParty.MainAddress.PK;
			AssertNoMessageErrorContaining(invoiceLine.JI_OA_SoldToPartyAddressInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(invoiceLine.JI_OA_SoldToPartyAddressInfo, string.Format(OrganisationValidation.EIN_SSN_CBNCodeRequired, "Sold To Party"));

			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			invoiceLine.JI_OA_SoldToPartyAddress = soldToParty.MainAddress.PK;
			invoiceLine.Validation.ValidateJI_OA_SoldToPartyAddress();
			AssertNoMessageErrorContaining(invoiceLine.JI_OA_SoldToPartyAddressInfo, string.Format(OrganisationValidation.EIN_SSN_CBNCodeRequired, "Sold To Party"));

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			soldToParty.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "91-013199000");
			invoiceLine.JI_OA_SoldToPartyAddress = soldToParty.MainAddress.PK;
			invoiceLine.Validation.ValidateJI_OA_SoldToPartyAddress();
			AssertNoMessageErrorContaining(invoiceLine.JI_OA_SoldToPartyAddressInfo, string.Format(OrganisationValidation.EIN_SSN_CBNCodeRequired, "Sold To Party"));
		}

		public void TestCheckJI_OA_SellerAddress()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.Validation.ValidateJI_OA_Seller();
			AssertHasMessageErrorContaining(invoiceLine.JI_OA_SellerInfo, MandatoryValidation.YouHaveNotEntered);

			var seller = Factory.NewWithValidTestData<OrgHeader>();
			invoiceLine.JI_OA_Seller = seller.MainAddress.PK;
			AssertNoMessageErrorContaining(invoiceLine.JI_OA_SellerInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.US_EnableCRL = false;
			invoiceLine.Validation.ValidateJI_OA_Seller();
			AssertNoMessageErrorContaining(invoiceLine.JI_OA_SellerInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJI_OA_SellerAddressWhenCargoRelaseEnabled()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.US_EnableENS = true;

			invoiceLine.Validation.ValidateJI_OA_Seller();
			AssertHasMessageErrorContaining(invoiceLine.JI_OA_SellerInfo, MandatoryValidation.YouHaveNotEntered);

			var seller = Factory.NewWithValidTestData<OrgHeader>();
			invoiceLine.JI_OA_Seller = seller.MainAddress.PK;
			AssertNoMessageErrorContaining(invoiceLine.JI_OA_SellerInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.US_EnableCRL = false;
			invoiceLine.Validation.ValidateJI_OA_Seller();
			AssertNoMessageErrorContaining(invoiceLine.JI_OA_SellerInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_EnableENS = true;
			declaration.US_CertifyCargoRelease = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			invoiceLine.JI_OA_Seller = ZGuid.Empty;
			AssertHasMessageErrorContaining(invoiceLine.JI_OA_SellerInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.US_CertifyCargoRelease = true;
			invoiceLine.Validation.ValidateJI_OA_Seller();
			AssertHasMessageErrorContaining(invoiceLine.JI_OA_SellerInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_OA_Seller = seller.MainAddress.PK;
			AssertNoMessageErrorContaining(invoiceLine.JI_OA_SellerInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJI_PartNo()
		{
			var importer = Factory.New<OrgHeader>();
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			var orgAddress1 = importer.Addresses.AddNew();
			var cusCode1 = importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FIRMSCode, "A000", "US");
			cusCode1.OK_OA_PremisesAddress = orgAddress1.PK;

			var warehouseOrg = Factory.New<OrgHeader>();
			warehouseOrg.OH_IsWarehouseClient = true;
			var orgAddress2 = warehouseOrg.Addresses.AddNew();
			var cusCode2 = warehouseOrg.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FIRMSCode, "A000", "US");
			cusCode2.OK_OA_PremisesAddress = orgAddress2.PK;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.US_EnableCRL = true;
			declaration.US_EntryDateElectionCode = EntryDateElectionCodeList.Codes.WeeklyEstimateFilingDate;
			declaration.JE_OH_Importer = importer.PK;
			declaration.WarehouseDocAddress.E2_OA_Address = orgAddress2.PK;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.US_SupTariff = "9813000520";
			invoiceLine.Validation.ValidateJI_PartNo();
			AssertHasMessageError(invoiceLine.JI_PartNoInfo, ValidationConstants.InvoiceLine.ImportTariff.ProductCannotBeEmpty);

			invoiceLine.US_SupTariff = ZString.Empty;
			invoiceLine.Validation.ValidateJI_PartNo();
			AssertNoMessageError(invoiceLine.JI_PartNoInfo, ValidationConstants.InvoiceLine.ImportTariff.ProductCannotBeEmpty);

			invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine.Validation.ValidateJI_PartNo();
			AssertHasMessageError(invoiceLine.JI_PartNoInfo, ValidationConstants.InvoiceLine.ImportTariff.ProductCannotBeEmpty);
		}

		public void TestCheckJI_TariffForSimplifiedEntry()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000000";
			tariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddMonths(1);
			tariff.UE_OGACodes = "FD2";
			tariff.UE_PGACodes = "FD2EP4";

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;//SE

			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			AssertNoMessageErrors("ACE FDA reportable in ACE certification", invoiceLine.JI_TariffInfo);

			declaration.US_EnableCRL = false;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			AssertNoMessageErrors("ACE FDA reportable in ACE certification", invoiceLine.JI_TariffInfo);

			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageErrors(invoiceLine.JI_TariffInfo);

			string errorMessage = "Tariff may not be empty";
			invoiceLine.JI_Tariff = "";
			AssertHasMessageError(invoiceLine.JI_TariffInfo, errorMessage);
			invoiceLine.JI_Tariff = USCTariff.FCCMayBeApplicable;
			AssertNoMessageError(invoiceLine.JI_TariffInfo, errorMessage);
		}

		public override void TestCheckJI_TariffForLaceyAct()
		{
			base.TestCheckJI_TariffForLaceyAct();

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			invoiceLine.US_SetInd = ZString.Empty;
			invoiceLine.LaceyActLines.RemoveAndDeleteAll();

			invoiceLine.Validation.ValidateJI_Tariff();

			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;

			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasWarning(invoiceLine.JI_TariffInfo, ACEImportJobComInvoiceLineValidation.LaceyActDataCertifyInACE);

			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoWarning(invoiceLine.JI_TariffInfo, ACEImportJobComInvoiceLineValidation.LaceyActDataCertifyInACE);
		}

		public void TestJI_TariffForSoftwoodLumber()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._11, "Atlantic Lumber Board (ALB) Certificate", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "44091020";
			tariff.UE_PermitLicenseIndicator = MiscellaneousPermitLicenseList.Codes.CanadaSoftwoodLumberExportNumber;
			tariff.UE_DateFrom = ZDateTime.Today;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);

			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			invoiceLine.JI_Tariff = "44091020";
			AssertHasWarningContaining(invoiceLine.JI_TariffInfo, LicenceValidator.SoftwoodLumberMaybeRequired);
		}

		public new void TestCheckJI_OA_ManufacturerAddress()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			invoiceLine.Validation.ValidateJI_OA_ManufacturerAddress();
			AssertHasMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, ACEImportJobComInvoiceLineValidation.ManufacturerRequiredForSE);

			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			invoiceLine.JI_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, ACEImportJobComInvoiceLineValidation.ManufacturerRequiredForSE);
			AssertHasMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, string.Format(OrganisationValidation.ManufacturerIDMissing, manufacturer.MainAddress.OA_Code));
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			invoiceLine.Validation.ValidateJI_OA_ManufacturerAddress();
			AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, string.Format(OrganisationValidation.ManufacturerIDMissing, manufacturer.MainAddress.OA_Code));

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.PostalCodeIsRequiredForChinaMF, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, false))
			{
				manufacturer.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.China;
				manufacturer.MainAddress.OA_PostCode = ZString.Empty;
				declaration.US_EnableCRL = false;
				declaration.US_CertifyCargoRelease = false;
				invoiceLine.JI_OA_ManufacturerAddress = ZGuid.Empty;
				AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeRequiredForChinaManufacturer);
				AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeIsInvalidForChinaManufacturer);
				invoiceLine.JI_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
				AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeRequiredForChinaManufacturer);
				AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeIsInvalidForChinaManufacturer);
				declaration.US_EnableCRL = true;
				invoiceLine.Validation.ValidateJI_OA_ManufacturerAddress();
				AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeRequiredForChinaManufacturer);
				AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeIsInvalidForChinaManufacturer);
				declaration.US_CertifyCargoRelease = true;
				invoiceLine.Validation.ValidateJI_OA_ManufacturerAddress();
				AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeRequiredForChinaManufacturer);
				AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeIsInvalidForChinaManufacturer);
				manufacturer.MainAddress.OA_PostCode = "123456";
				invoiceLine.Validation.ValidateJI_OA_ManufacturerAddress();
				AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeRequiredForChinaManufacturer);
				AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeIsInvalidForChinaManufacturer);
				manufacturer.MainAddress.OA_PostCode = "ABCDEF";
				invoiceLine.Validation.ValidateJI_OA_ManufacturerAddress();
				AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeRequiredForChinaManufacturer);
				AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeIsInvalidForChinaManufacturer);
				manufacturer.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
				invoiceLine.Validation.ValidateJI_OA_ManufacturerAddress();
				AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeRequiredForChinaManufacturer);
				AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeIsInvalidForChinaManufacturer);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.PostalCodeIsRequiredForChinaMF, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				manufacturer.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.China;
				manufacturer.MainAddress.OA_PostCode = ZString.Empty;
				declaration.US_EnableCRL = false;
				declaration.US_CertifyCargoRelease = false;
				invoiceLine.JI_OA_ManufacturerAddress = ZGuid.Empty;
				AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeRequiredForChinaManufacturer);
				AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeIsInvalidForChinaManufacturer);
				invoiceLine.JI_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
				AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeRequiredForChinaManufacturer);
				AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeIsInvalidForChinaManufacturer);
				declaration.US_EnableCRL = true;
				invoiceLine.Validation.ValidateJI_OA_ManufacturerAddress();
				AssertHasMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeRequiredForChinaManufacturer);
				AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeIsInvalidForChinaManufacturer);
				declaration.US_CertifyCargoRelease = true;
				invoiceLine.Validation.ValidateJI_OA_ManufacturerAddress();
				AssertHasMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeRequiredForChinaManufacturer);
				AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeIsInvalidForChinaManufacturer);
				manufacturer.MainAddress.OA_PostCode = "123456";
				invoiceLine.Validation.ValidateJI_OA_ManufacturerAddress();
				AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeRequiredForChinaManufacturer);
				AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeIsInvalidForChinaManufacturer);
				manufacturer.MainAddress.OA_PostCode = "ABCDEF";
				invoiceLine.Validation.ValidateJI_OA_ManufacturerAddress();
				AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeRequiredForChinaManufacturer);
				AssertHasMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeIsInvalidForChinaManufacturer);
				manufacturer.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
				invoiceLine.Validation.ValidateJI_OA_ManufacturerAddress();
				AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeRequiredForChinaManufacturer);
				AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, ZipCodeValidation.PostalCodeIsInvalidForChinaManufacturer);
			}
		}

		public void TestCheckJI_OA_ExporterAddress()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_ADDCaseNo = "A1";
			invoiceLine.JI_OA_ExporterAddress = ZGuid.Empty;
			AssertNoMessageError(invoiceLine.JI_OA_ExporterAddressInfo, ACEImportJobComInvoiceLineValidation.ForeignExporterIsRequiredForADD_CVD);

			var foreignExporter = Factory.New<OrgHeader>();
			invoiceLine.JI_OA_ExporterAddress = foreignExporter.MainAddress.PK;
			AssertNoMessageError(invoiceLine.JI_OA_ExporterAddressInfo, ACEImportJobComInvoiceLineValidation.ForeignExporterIsRequiredForADD_CVD);
			AssertHasMessageErrorContaining(invoiceLine.JI_OA_ExporterAddressInfo, "There is no MID linked to address: ");

			foreignExporter.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "1");
			invoiceLine.JI_OA_ExporterAddress = foreignExporter.PK;
			AssertNoMessageErrorContaining(invoiceLine.JI_OA_ExporterAddressInfo, "There is no MID linked to address: ");

			declaration.US_EnableENS = true;
			invoiceLine.JI_OA_ExporterAddress = ZGuid.Empty;
			AssertHasMessageError(invoiceLine.JI_OA_ExporterAddressInfo, ACEImportJobComInvoiceLineValidation.ForeignExporterIsRequiredForADD_CVD);

			invoiceLine.US_NOPInd = "Y";
			var ams = invoiceLine.AMSLines.AddNew();
			ams.US_Program = AMSProgramList.Codes.OR1;
			invoiceLine.JI_OA_ExporterAddress = ZGuid.Empty;
			AssertHasMessageError(invoiceLine.JI_OA_ExporterAddressInfo, "You have not entered an Exporter.");

			invoiceLine.JI_OA_ExporterAddress = foreignExporter.MainAddress.PK;
			AssertNoMessageError(invoiceLine.JI_OA_ExporterAddressInfo, "You have not entered an Exporter.");
			AssertHasMessageError(invoiceLine.JI_OA_ExporterAddressInfo, "Organization must have Registration Code of type AMS on file.");

			var cusCode = foreignExporter.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.AMSRegistrationNumber, "111", Core.Constants.CountryCodes.UnitedStates);
			invoiceLine.JI_OA_ExporterAddress = foreignExporter.MainAddress.PK;
			AssertNoMessageError(invoiceLine.JI_OA_ExporterAddressInfo, "Organization must have Registration Code of type AMS on file.");
			AssertHasMessageError(invoiceLine.JI_OA_ExporterAddressInfo, "AMS code must be 10 digits.");

			foreignExporter.CustomsCodes.RemoveAndDelete(cusCode);
			cusCode = foreignExporter.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.AMSRegistrationNumber, "1234567891", Core.Constants.CountryCodes.UnitedStates);
			invoiceLine.JI_OA_ExporterAddress = foreignExporter.MainAddress.PK;
			AssertNoMessageError(invoiceLine.JI_OA_ExporterAddressInfo, "AMS code must be 10 digits.");
		}

		public void TestLicenseValidatorForJI_Tariff()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._01, "Steel Import License", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "44091020";
			tariff.UE_PermitLicenseIndicator = "01";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(10);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = false;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "44091020";
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, "The Steel Import License number is required.");

			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageErrorContaining(invoiceLine.JI_TariffInfo, "The Steel Import License number is required.");
		}

		public void TestCheckJI_LinePriceForFWS()
		{
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var fwsHeader = invoiceLine.FWSHeaders.AddNew();
			fwsHeader.US_ProcessingCode = FWSProcessingCodeList.Codes.EDS;
			invoiceLine.JI_LinePrice = 100m;
			invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Declared;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_ParentID = invoiceLine.PK;
			invoiceLine2.JI_LinePrice = 200m;
			invoiceLine2.US_FWSInd = OGAIndicatorList.Codes.Declared;
			var fwsHeader2 = invoiceLine2.FWSHeaders.AddNew();
			fwsHeader2.US_ProcessingCode = FWSProcessingCodeList.Codes.EDS;

			declaration.US_EnableENS = true;

			invoiceLine.Validation.ValidateJI_LinePrice();
			AssertNoMessageErrorContaining(invoiceLine.JI_LinePriceInfo, "The total of Inv. Curr. Value");
			AssertNoMessageErrorContaining(invoiceLine2.JI_LinePriceInfo, "The total of Inv. Curr. Value");

			fwsHeader2.US_InvCurrPGAValue = 250m;
			invoiceLine.Validation.ValidateJI_LinePrice();
			AssertHasMessageErrorContaining(invoiceLine.JI_LinePriceInfo, "The total of Inv. Curr. Value");
			AssertNoMessageErrorContaining(invoiceLine2.JI_LinePriceInfo, "The total of Inv. Curr. Value");
		}

		public void TestCheckJI_LinePriceForSecondaryLine()
		{
			var testHelper = new Chapter98HelperTest();
			testHelper.ParentLine.US_SupTariff = testHelper.Test99038501Tariff.UE_Tariff;//99038501
			testHelper.ChildLine.US_SupTariff = testHelper.Test9802005060Tariff.UE_Tariff;//9802005060
			testHelper.Charpter98Job.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			testHelper.ChildLine.JI_LinePrice = 1m;
			AssertHasMessageError(testHelper.ChildLine.JI_LinePriceInfo, ACEImportJobComInvoiceLineValidation.PriceShouldBeZeroForCombinedLine);
			testHelper.ChildLine.JI_LinePrice = ZDecimal.Zero;
			AssertNoMessageError(testHelper.ChildLine.JI_LinePriceInfo, ACEImportJobComInvoiceLineValidation.PriceShouldBeZeroForCombinedLine);
		}

		public void TestCheckJI_LinePriceForLowValueEntries()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableCRL = true;
			invoiceLine.Validation.ValidateJI_LinePrice();
			AssertNoMessageErrorContaining(invoiceLine.JI_LinePriceInfo, ACEImportJobComInvoiceLineValidation.PriceIsMandatoryForLowValueEntries);

			declaration.US_EntryType = EntryTypeList.Codes.LowValue;
			invoiceLine.Validation.ValidateJI_LinePrice();
			AssertHasMessageErrorContaining(invoiceLine.JI_LinePriceInfo, ACEImportJobComInvoiceLineValidation.PriceIsMandatoryForLowValueEntries);

			invoiceLine.JI_LinePrice = 100m;
			AssertNoMessageErrorContaining(invoiceLine.JI_LinePriceInfo, ACEImportJobComInvoiceLineValidation.PriceIsMandatoryForLowValueEntries);
		}

		public void TestCheckJI_OA_ConsigneeAddress()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_CertifyCargoRelease = false;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var consignee = Factory.New<OrgHeader>();
			consignee.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "91-013199000");
			invoiceLine.JI_OA_ConsigneeAddress = consignee.MainAddress.PK;
			AssertHasMessageError(invoiceLine.JI_OA_ConsigneeAddressInfo, ACEImportJobComInvoiceLineValidation.NoLineConsigneeAllowedWhenHeaderHasNoConsigneeEntered);

			declaration.JE_OA_ConsigneeAddress = consignee.MainAddress.PK;
			invoiceLine.JI_OA_ConsigneeAddress = consignee.MainAddress.PK;
			AssertNoMessageError(invoiceLine.JI_OA_ConsigneeAddressInfo, ACEImportJobComInvoiceLineValidation.NoLineConsigneeAllowedWhenHeaderHasNoConsigneeEntered);

			declaration.US_EnableCRL = true;
			declaration.JE_OA_ConsigneeAddress = ZGuid.Empty;
			AssertNoMessageError(invoiceLine.JI_OA_ConsigneeAddressInfo, ACEImportJobComInvoiceLineValidation.NoLineConsigneeAllowedWhenHeaderHasNoConsigneeEntered);

			invoiceLine.JI_OA_ConsigneeAddress = consignee.MainAddress.PK;
			var amsLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = AMSProgramList.Codes.MO4;
			Assert(invoiceLine.HasAMSMO4);
			invoiceLine.Validation.ValidateJI_OA_ConsigneeAddress();
			AssertHasMessageErrorContaining(invoiceLine.JI_OA_ConsigneeAddressInfo, "AMS MO4 is selected in this invoice line, AMS ID number is required against this organization.");

			consignee.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.AMSRegistrationNumber, "AM0000001");
			invoiceLine.JI_OA_ConsigneeAddress = consignee.MainAddress.PK;
			invoiceLine.Validation.ValidateJI_OA_ConsigneeAddress();
			AssertNoMessageErrorContaining(invoiceLine.JI_OA_ConsigneeAddressInfo, "AMS MO4 is selected in this invoice line, AMS ID number is required against this organization.");

			declaration.US_EntryType = EntryTypeList.Codes.LowValue;
			var newConsignee = Factory.New<OrgHeader>();
			declaration.JE_OA_ConsigneeAddress = newConsignee.MainAddress.PK;
			invoiceLine.Validation.ValidateJI_OA_ConsigneeAddress();
			AssertHasMessageError(invoiceLine.JI_OA_ConsigneeAddressInfo, ACEImportJobComInvoiceLineValidation.ConsigneeNotMatchingMessageText);
			declaration.JE_OA_ConsigneeAddress = consignee.MainAddress.PK;
			invoiceLine.Validation.ValidateJI_OA_ConsigneeAddress();
			AssertNoMessageError(invoiceLine.JI_OA_ConsigneeAddressInfo, ACEImportJobComInvoiceLineValidation.ConsigneeNotMatchingMessageText);
		}

		public void TestSmeltAndCastInformationRequiredForTariff()
		{
			#region Setup Tariffs

			var helper = new UniversalReferenceTestDataHelper(Factory);

			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "HSN");
			var conditionType = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Rate, UniversalReferenceConstants.TariffConditionTypes.Codes.SMELT);
			Factory.Save();

			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "7601103000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, conditionType.PK, tariff.PK, "", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			Factory.Save();

			#endregion

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.USSmelt, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, false))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
				declaration.US_EnableENS = true;

				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "7601103000";
				AssertNoMessageError(invoiceLine.JI_TariffInfo, ACEImportJobComInvoiceLineValidation.SmeltAndCastInformationNotRequired);

				invoiceLine.US_RN_NKCastCtry = Core.Constants.CountryCodes.Australia;
				invoiceLine.Validation.ValidateJI_Tariff();
				AssertHasMessageError(invoiceLine.JI_TariffInfo, ACEImportJobComInvoiceLineValidation.SmeltAndCastInformationNotRequired);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.USSmelt, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
				declaration.US_EnableENS = true;

				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "7601103000";
				AssertHasMessageError(invoiceLine.JI_TariffInfo, ACEImportJobComInvoiceLineValidation.SmeltAndCastInformationRequired);

				invoiceLine.US_RN_NKCastCtry = Core.Constants.CountryCodes.Australia;
				invoiceLine.Validation.ValidateJI_Tariff();
				AssertNoMessageError(invoiceLine.JI_TariffInfo, ACEImportJobComInvoiceLineValidation.SmeltAndCastInformationRequired);
			}
		}

		[TestDate(2020, 03, 05)]
		public void TestValidateFieldsForEmbroideryChildLine()
		{
			#region Setup Tariffs

			var helper = new UniversalReferenceTestDataHelper(Factory);

			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "HSN");
			Factory.Save();

			var zzTariff = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "5810929080", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateTariffAttribute("RULE", "EMB", zzTariff);

			var tariff5810929080 = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "5810929080")).LastOrDefault();
			if (tariff5810929080 == null)
			{
				tariff5810929080 = Factory.New<USCTariff>();
				tariff5810929080.UE_Tariff = "5810929080";
				tariff5810929080.UE_DutyComputationCode = "7";
				tariff5810929080.UE_Column1RateAdValorem = 0.074m;
			}
			tariff5810929080.UE_DateFrom = new ZDateTime(2020, 01, 01);
			tariff5810929080.UE_DateTo = new ZDateTime(2021, 01, 01);

			var tariff5407532060 = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "5407532060")).LastOrDefault();
			if (tariff5407532060 == null)
			{
				tariff5407532060 = Factory.New<USCTariff>();
				tariff5407532060.UE_Tariff = "5407532060";
				tariff5407532060.UE_DutyComputationCode = "7";
				tariff5407532060.UE_Column1RateAdValorem = 0.12m;
			}
			tariff5407532060.UE_DateFrom = new ZDateTime(2020, 01, 01);
			tariff5407532060.UE_DateTo = new ZDateTime(2021, 01, 01);

			Factory.Save();

			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.ImportEntryNumber = "~9342838";

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLineOne = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLineOne.JI_Tariff = "5810929080";
			invoiceLineOne.JI_LinePrice = 5000m;
			AssertNoMessageError(invoiceLineOne.JI_LinePriceInfo, ZString.Format(ValidationConstants.InvoiceLine.ValueShouldBeDeclaredOnParent, invoiceLineOne.JI_LinePriceInfo.HumanReadableName));
			invoiceLineOne.US_UC_NKCountryOfOrigin = "CN";
			invoiceLineOne.US_UC_NKCountryOfExport = "CN";
			invoiceLineOne.JI_InvoiceQuantity = 220m;
			invoiceLineOne.JI_InvoiceUQ = "KG";
			AssertNoMessageError(invoiceLineOne.JI_InvoiceQuantityInfo, ZString.Format(ValidationConstants.InvoiceLine.ValueShouldBeDeclaredOnParent, invoiceLineOne.JI_InvoiceQuantityInfo.HumanReadableName));
			invoiceLineOne.JI_Weight = 100m;
			invoiceLineOne.JI_WeightUQ = "KG";
			AssertNoMessageError(invoiceLineOne.JI_WeightInfo, ZString.Format(ValidationConstants.InvoiceLine.ValueShouldBeDeclaredOnParent, invoiceLineOne.JI_WeightInfo.HumanReadableName));
			invoiceLineOne.JI_NetWeight = 100m;
			invoiceLineOne.JI_NetWeightUQ = "KG";
			AssertNoMessageError(invoiceLineOne.JI_NetWeightInfo, ZString.Format(ValidationConstants.InvoiceLine.ValueShouldBeDeclaredOnParent, invoiceLineOne.JI_NetWeightInfo.HumanReadableName));
			invoiceLineOne.JI_Volume = 10m;
			invoiceLineOne.JI_VolumeUQ = "M3";
			AssertNoMessageError(invoiceLineOne.JI_VolumeInfo, ZString.Format(ValidationConstants.InvoiceLine.ValueShouldBeDeclaredOnParent, invoiceLineOne.JI_VolumeInfo.HumanReadableName));

			var invoiceLineTwo = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLineTwo.JI_ParentID = invoiceLineOne.PK;
			invoiceLineTwo.JI_Tariff = "5407532060";
			invoiceLineTwo.US_UC_NKCountryOfOrigin = "CN";
			invoiceLineTwo.US_UC_NKCountryOfExport = "CN";
			invoiceLineTwo.JI_LinePrice = 100m;
			AssertHasMessageError(invoiceLineTwo.JI_LinePriceInfo, ZString.Format(ValidationConstants.InvoiceLine.ValueShouldBeDeclaredOnParent, invoiceLineTwo.JI_LinePriceInfo.HumanReadableName));
			invoiceLineTwo.JI_InvoiceQuantity = 579m;
			invoiceLineTwo.JI_InvoiceUQ = "KG";
			AssertHasMessageError(invoiceLineTwo.JI_InvoiceQuantityInfo, ZString.Format(ValidationConstants.InvoiceLine.ValueShouldBeDeclaredOnParent, invoiceLineTwo.JI_InvoiceQuantityInfo.HumanReadableName));
			invoiceLineTwo.JI_Weight = 100m;
			invoiceLineTwo.JI_WeightUQ = "KG";
			AssertHasMessageError(invoiceLineTwo.JI_WeightInfo, ZString.Format(ValidationConstants.InvoiceLine.ValueShouldBeDeclaredOnParent, invoiceLineTwo.JI_WeightInfo.HumanReadableName));
			invoiceLineTwo.JI_NetWeight = 100m;
			invoiceLineTwo.JI_NetWeightUQ = "KG";
			AssertHasMessageError(invoiceLineTwo.JI_NetWeightInfo, ZString.Format(ValidationConstants.InvoiceLine.ValueShouldBeDeclaredOnParent, invoiceLineTwo.JI_NetWeightInfo.HumanReadableName));
			invoiceLineTwo.JI_Volume = 10m;
			invoiceLineTwo.JI_VolumeUQ = "M3";
			AssertHasMessageError(invoiceLineTwo.JI_VolumeInfo, ZString.Format(ValidationConstants.InvoiceLine.ValueShouldBeDeclaredOnParent, invoiceLineTwo.JI_VolumeInfo.HumanReadableName));

			invoiceLineTwo.JI_LinePrice = 0m;
			AssertNoMessageError(invoiceLineTwo.JI_LinePriceInfo, ZString.Format(ValidationConstants.InvoiceLine.ValueShouldBeDeclaredOnParent, invoiceLineTwo.JI_LinePriceInfo.HumanReadableName));
			invoiceLineTwo.JI_InvoiceQuantity = 0m;
			AssertNoMessageError(invoiceLineTwo.JI_InvoiceQuantityInfo, ZString.Format(ValidationConstants.InvoiceLine.ValueShouldBeDeclaredOnParent, invoiceLineTwo.JI_InvoiceQuantityInfo.HumanReadableName));
			invoiceLineTwo.JI_Weight = 0m;
			AssertNoMessageError(invoiceLineTwo.JI_WeightInfo, ZString.Format(ValidationConstants.InvoiceLine.ValueShouldBeDeclaredOnParent, invoiceLineTwo.JI_WeightInfo.HumanReadableName));
			invoiceLineTwo.JI_NetWeight = 0m;
			AssertNoMessageError(invoiceLineTwo.JI_NetWeightInfo, ZString.Format(ValidationConstants.InvoiceLine.ValueShouldBeDeclaredOnParent, invoiceLineTwo.JI_NetWeightInfo.HumanReadableName));
			invoiceLineTwo.JI_Volume = 0m;
			AssertNoMessageError(invoiceLineTwo.JI_VolumeInfo, ZString.Format(ValidationConstants.InvoiceLine.ValueShouldBeDeclaredOnParent, invoiceLineTwo.JI_VolumeInfo.HumanReadableName));
		}

		public void TestOrganizaitonAddressValidationsForLowValueEntries()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.LowValue;
			declaration.US_EnableCRL = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.Validation.ValidateJI_OA_ManufacturerAddress();
			AssertNoMessageErrorContaining(invoiceLine.JI_OA_ManufacturerAddressInfo, ACEImportJobComInvoiceLineValidation.ManufacturerRequiredForSE);
			invoiceLine.Validation.ValidateJI_OA_SoldToPartyAddress();
			AssertNoMessageErrorContaining(invoiceLine.JI_OA_SoldToPartyAddressInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			invoiceLine.Validation.ValidateJI_OA_ManufacturerAddress();
			AssertHasMessageErrorContaining(invoiceLine.JI_OA_ManufacturerAddressInfo, ACEImportJobComInvoiceLineValidation.ManufacturerRequiredForSE);
			invoiceLine.Validation.ValidateJI_OA_SoldToPartyAddress();
			AssertHasMessageErrorContaining(invoiceLine.JI_OA_SoldToPartyAddressInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestSupAdditionalTariffsOnInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "7601.10.3000";
			invoiceLine.SupFormattedAdditionalTariff1 = "9903.85.01";
			AssertHasMessageErrorContaining(invoiceLine.SupFormattedAdditionalTariff1Info, "Tariff unable to be found. ");
			AssertHasMessageError(invoiceLine.SupFormattedAdditionalTariff1Info, "Only 'Prov/Prog. Tariff' should be filled when there is one supplementary tariff on invoice line.");

			invoiceLine.US_SupTariff = "99038501";
			invoiceLine.SupFormattedAdditionalTariff1 = ZString.Empty;
			AssertNoMessageErrorContaining(invoiceLine.SupFormattedAdditionalTariff1Info, "Tariff unable to be found. ");
			AssertNoMessageError(invoiceLine.SupFormattedAdditionalTariff1Info, "Only 'Prov/Prog. Tariff' should be filled when there is one supplementary tariff on invoice line.");

			invoiceLine.SupFormattedAdditionalTariff2 = "9903.85.02";
			AssertHasMessageErrorContaining(invoiceLine.SupFormattedAdditionalTariff2Info, "Tariff unable to be found. ");
			AssertHasMessageError(invoiceLine.SupFormattedAdditionalTariff2Info, "Only 'Prov/Prog. Additional Tariff 1' and 'Prov/Prog. Tariff' should be filled when there are two supplementary tariffs on invoice line.");

			invoiceLine.SupFormattedAdditionalTariff1 = "9903.85.02";
			invoiceLine.SupFormattedAdditionalTariff2 = ZString.Empty;
			AssertNoMessageErrorContaining(invoiceLine.SupFormattedAdditionalTariff2Info, "Tariff unable to be found. ");
			AssertNoMessageError(invoiceLine.SupFormattedAdditionalTariff2Info, "Only 'Prov/Prog. Additional Tariff 1' and 'Prov/Prog. Tariff' should be filled when there are two supplementary tariffs on invoice line.");

			invoiceLine.SupFormattedAdditionalTariff3 = "9903.85.03";
			AssertHasMessageErrorContaining(invoiceLine.SupFormattedAdditionalTariff3Info, "Tariff unable to be found. ");
			AssertHasMessageError(invoiceLine.SupFormattedAdditionalTariff3Info, "Only 'Prov/Prog. Additional Tariff 1', 'Prov/Prog. Additional Tariff 2' and 'Prov/Prog. Tariff' should be filled when there are three supplementary tariffs on invoice line.");

			invoiceLine.SupFormattedAdditionalTariff2 = "9903.85.03";
			invoiceLine.SupFormattedAdditionalTariff3 = ZString.Empty;
			AssertNoMessageErrorContaining(invoiceLine.SupFormattedAdditionalTariff3Info, "Tariff unable to be found. ");
			AssertNoMessageError(invoiceLine.SupFormattedAdditionalTariff3Info, "Only 'Prov/Prog. Additional Tariff 1', 'Prov/Prog. Additional Tariff 2' and 'Prov/Prog. Tariff' should be filled when there are three supplementary tariffs on invoice line.");

			invoiceLine.SupFormattedAdditionalTariff4 = "9903.85.04";
			AssertHasMessageErrorContaining(invoiceLine.SupFormattedAdditionalTariff4Info, "Tariff unable to be found. ");
			AssertHasMessageError(invoiceLine.SupFormattedAdditionalTariff4Info, "Only 'Prov/Prog. Additional Tariff 1', 'Prov/Prog. Additional Tariff 2', 'Prov/Prog. Additional Tariff 3' and 'Prov/Prog. Tariff' should be filled when there are four supplementary tariffs on invoice line.");

			invoiceLine.SupFormattedAdditionalTariff3 = "9903.85.04";
			invoiceLine.SupFormattedAdditionalTariff4 = ZString.Empty;
			AssertNoMessageErrorContaining(invoiceLine.SupFormattedAdditionalTariff4Info, "Tariff unable to be found. ");
			AssertNoMessageError(invoiceLine.SupFormattedAdditionalTariff4Info, "Only 'Prov/Prog. Additional Tariff 1', 'Prov/Prog. Additional Tariff 2', 'Prov/Prog. Additional Tariff 3' and 'Prov/Prog. Tariff' should be filled when there are four supplementary tariffs on invoice line.");

			invoiceLine.SupFormattedAdditionalTariff5 = "9903.85.05";
			AssertHasMessageErrorContaining(invoiceLine.SupFormattedAdditionalTariff5Info, "Tariff unable to be found. ");
			AssertHasMessageError(invoiceLine.SupFormattedAdditionalTariff5Info, "Only 'Prov/Prog. Additional Tariff 1', 'Prov/Prog. Additional Tariff 2', 'Prov/Prog. Additional Tariff 3', 'Prov/Prog. Additional Tariff 4' and 'Prov/Prog. Tariff' should be filled when there are five supplementary tariffs on invoice line.");

			invoiceLine.SupFormattedAdditionalTariff4 = "9903.85.05";
			invoiceLine.SupFormattedAdditionalTariff5 = ZString.Empty;
			AssertNoMessageErrorContaining(invoiceLine.SupFormattedAdditionalTariff5Info, "Tariff unable to be found. ");
			AssertNoMessageError(invoiceLine.SupFormattedAdditionalTariff5Info, "Only 'Prov/Prog. Additional Tariff 1', 'Prov/Prog. Additional Tariff 2', 'Prov/Prog. Additional Tariff 3', 'Prov/Prog. Additional Tariff 4' and 'Prov/Prog. Tariff' should be filled when there are five supplementary tariffs on invoice line.");
		}

		public void TestValidateSupplementryTariffsOnDerivedSetLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_FormattedTariff = "8206.00.0000";
			invoiceLine1.JI_LinePrice = 0m;
			invoiceLine1.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			invoiceLine2.JI_FormattedTariff = "8203.20.4000";
			invoiceLine2.SupTariffFormatted = "9903.01.20";
			invoiceLine2.SupFormattedAdditionalTariff1 = "9903.88.02";
			invoiceLine2.SupFormattedAdditionalTariff2 = "9903.88.03";
			invoiceLine2.SupFormattedAdditionalTariff3 = "9903.88.04";
			invoiceLine2.SupFormattedAdditionalTariff4 = "9903.88.05";
			invoiceLine2.SupFormattedAdditionalTariff5 = "9903.88.06";
			invoiceLine2.JI_LinePrice = 10000m;
			AssertNoMessageError(invoiceLine1.US_SupTariffInfo, TariffValidator.SupplementaryTariffShouldBeEnteredOnParentLineForDerivedSets);
			AssertNoMessageError(invoiceLine1.SupFormattedAdditionalTariff1Info, TariffValidator.SupplementaryTariffShouldBeEnteredOnParentLineForDerivedSets);
			AssertNoMessageError(invoiceLine1.SupFormattedAdditionalTariff2Info, TariffValidator.SupplementaryTariffShouldBeEnteredOnParentLineForDerivedSets);
			AssertNoMessageError(invoiceLine1.SupFormattedAdditionalTariff3Info, TariffValidator.SupplementaryTariffShouldBeEnteredOnParentLineForDerivedSets);
			AssertNoMessageError(invoiceLine1.SupFormattedAdditionalTariff4Info, TariffValidator.SupplementaryTariffShouldBeEnteredOnParentLineForDerivedSets);
			AssertNoMessageError(invoiceLine1.SupFormattedAdditionalTariff5Info, TariffValidator.SupplementaryTariffShouldBeEnteredOnParentLineForDerivedSets);
			AssertHasMessageError(invoiceLine2.US_SupTariffInfo, TariffValidator.SupplementaryTariffShouldBeEnteredOnParentLineForDerivedSets);
			AssertHasMessageError(invoiceLine2.SupFormattedAdditionalTariff1Info, TariffValidator.SupplementaryTariffShouldBeEnteredOnParentLineForDerivedSets);
			AssertHasMessageError(invoiceLine2.SupFormattedAdditionalTariff2Info, TariffValidator.SupplementaryTariffShouldBeEnteredOnParentLineForDerivedSets);
			AssertHasMessageError(invoiceLine2.SupFormattedAdditionalTariff3Info, TariffValidator.SupplementaryTariffShouldBeEnteredOnParentLineForDerivedSets);
			AssertHasMessageError(invoiceLine2.SupFormattedAdditionalTariff4Info, TariffValidator.SupplementaryTariffShouldBeEnteredOnParentLineForDerivedSets);
			AssertHasMessageError(invoiceLine2.SupFormattedAdditionalTariff5Info, TariffValidator.SupplementaryTariffShouldBeEnteredOnParentLineForDerivedSets);

			invoiceLine1.SupTariffFormatted = "9903.01.20";
			invoiceLine1.SupFormattedAdditionalTariff1 = "9903.88.02";
			invoiceLine1.SupFormattedAdditionalTariff2 = "9903.88.03";
			invoiceLine1.SupFormattedAdditionalTariff3 = "9903.88.04";
			invoiceLine1.SupFormattedAdditionalTariff4 = "9903.88.05";
			invoiceLine1.SupFormattedAdditionalTariff5 = "9903.88.06";
			invoiceLine2.SupTariffFormatted = ZString.Empty;
			invoiceLine2.SupFormattedAdditionalTariff1 = ZString.Empty;
			invoiceLine2.SupFormattedAdditionalTariff2 = ZString.Empty;
			invoiceLine2.SupFormattedAdditionalTariff3 = ZString.Empty;
			invoiceLine2.SupFormattedAdditionalTariff4 = ZString.Empty;
			invoiceLine2.SupFormattedAdditionalTariff5 = ZString.Empty;
			AssertNoMessageError(invoiceLine1.US_SupTariffInfo, TariffValidator.SupplementaryTariffShouldBeEnteredOnParentLineForDerivedSets);
			AssertNoMessageError(invoiceLine1.SupFormattedAdditionalTariff1Info, TariffValidator.SupplementaryTariffShouldBeEnteredOnParentLineForDerivedSets);
			AssertNoMessageError(invoiceLine1.SupFormattedAdditionalTariff2Info, TariffValidator.SupplementaryTariffShouldBeEnteredOnParentLineForDerivedSets);
			AssertNoMessageError(invoiceLine1.SupFormattedAdditionalTariff3Info, TariffValidator.SupplementaryTariffShouldBeEnteredOnParentLineForDerivedSets);
			AssertNoMessageError(invoiceLine1.SupFormattedAdditionalTariff4Info, TariffValidator.SupplementaryTariffShouldBeEnteredOnParentLineForDerivedSets);
			AssertNoMessageError(invoiceLine1.SupFormattedAdditionalTariff5Info, TariffValidator.SupplementaryTariffShouldBeEnteredOnParentLineForDerivedSets);
			AssertNoMessageError(invoiceLine2.US_SupTariffInfo, TariffValidator.SupplementaryTariffShouldBeEnteredOnParentLineForDerivedSets);
			AssertNoMessageError(invoiceLine2.SupFormattedAdditionalTariff1Info, TariffValidator.SupplementaryTariffShouldBeEnteredOnParentLineForDerivedSets);
			AssertNoMessageError(invoiceLine2.SupFormattedAdditionalTariff2Info, TariffValidator.SupplementaryTariffShouldBeEnteredOnParentLineForDerivedSets);
			AssertNoMessageError(invoiceLine2.SupFormattedAdditionalTariff3Info, TariffValidator.SupplementaryTariffShouldBeEnteredOnParentLineForDerivedSets);
			AssertNoMessageError(invoiceLine2.SupFormattedAdditionalTariff4Info, TariffValidator.SupplementaryTariffShouldBeEnteredOnParentLineForDerivedSets);
			AssertNoMessageError(invoiceLine2.SupFormattedAdditionalTariff5Info, TariffValidator.SupplementaryTariffShouldBeEnteredOnParentLineForDerivedSets);
		}
	}
}

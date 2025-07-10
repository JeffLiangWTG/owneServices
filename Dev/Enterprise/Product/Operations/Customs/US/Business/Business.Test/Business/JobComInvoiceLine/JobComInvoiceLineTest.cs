using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Business.Testing;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Integration.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Customs.US.Business.UniversalReferenceConstants;
using static Enterprise.Integration.Customs;
using DefaultOptions = Enterprise.Core.Constants.Customs.ASNRefreshDefaultsOptions;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(JobComInvoiceLine))]
	partial class JobComInvoiceLineTest : Customs.Business.Testing.BaseJobComInvoiceLineAbstractTest
	{
		public void TestSetDefaultValueOrCleanSanctionsIfNeeded()
		{
			SetUpFishingAndMiningTariff();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var header = declaration.Invoices.AddNew();
			var invoiceLine1 = header.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "0301930000";
			invoiceLine1.US_UC_NKCountryOfOrigin = "RU";
			var fishing = invoiceLine1.FishingInformations.AddNew();
			fishing.US_MethodOfHarvest = "HFC";
			invoiceLine1.JI_Tariff = "7102310000";
			AssertEquals(0, invoiceLine1.FishingInformations.Count);

			var mining = invoiceLine1.MiningInformations.AddNew();
			mining.CY_Data = "CA";
			invoiceLine1.US_UC_NKCountryOfOrigin = "CA";
			AssertEquals(0, invoiceLine1.MiningInformations.Count);

			var invoiceLine2 = header.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "7102310000";
			invoiceLine2.US_UC_NKCountryOfOrigin = "RU";
			mining = invoiceLine2.MiningInformations.AddNew();
			mining.CY_Data = "CA";

			invoiceLine2.JI_Tariff = "0301930000";
			AssertEquals(0, invoiceLine2.MiningInformations.Count);
			fishing = invoiceLine2.FishingInformations.AddNew();
			fishing.US_MethodOfHarvest = "HFC";
			invoiceLine2.US_UC_NKCountryOfOrigin = "CA";
			AssertEquals(0, invoiceLine2.FishingInformations.Count);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.Sanctions, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				declaration.US_DisclaimSanctions = true;
				AssertEquals(false, invoiceLine2.US_DisclaimSanctions);
				invoiceLine2.US_UC_NKCountryOfOrigin = "RU";
				AssertEquals(true, invoiceLine2.US_DisclaimSanctions);
			}
		}

		public void TestCopyNMFSDataToSanctions()
		{
			SetUpFishingAndMiningTariff();
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "vessel 2";
			vessel.RV_LloydsNumber = "0002";
			var declarationVessel = Factory.New<RefVessel>();
			declarationVessel.RV_Code = "Declaration Vessel Name";
			declarationVessel.RV_LloydsNumber = "DEC0001";
			declarationVessel.RV_RN_NKCountryOfReg = "DE";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_VesselName = "Declaration Vessel Name";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var header = declaration.Invoices.AddNew();
			var invoiceLine1 = header.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "0301930000";
			invoiceLine1.US_UC_NKCountryOfOrigin = "RU";

			var nmfsLine1 = invoiceLine1.NMFSLines.AddNew();
			nmfsLine1.US_ProgramType = NMFSProgramCodeList.Codes.COA;
			nmfsLine1.US_SourceType = SourceTypeCodesList.Codes.SmallVesselHarvest;
			var harvesting1 = nmfsLine1.HarvestingDetails.AddNew();
			harvesting1.US_VesselCountry = "CA";
			harvesting1.US_HarvestedCountry = "CA";

			var invoiceLine2 = header.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0301930000";
			invoiceLine2.US_UC_NKCountryOfOrigin = "RU";
			var nmfsLine2 = invoiceLine2.NMFSLines.AddNew();
			nmfsLine2.US_ProgramType = NMFSProgramCodeList.Codes.SIM;
			nmfsLine2.US_SourceType = SourceTypeCodesList.Codes.HarvestOfCaptureFisheries;
			var harvesting2 = nmfsLine2.HarvestingDetails.AddNew();
			harvesting2.US_HarvestedCountry = "CA";
			var vessel1 = harvesting2.HarvestingVessles.AddNew();
			vessel1.US_HarvestedVessel = "vessel 1";
			vessel1.US_HarvestedCountry = "US";
			var vessel2 = harvesting2.HarvestingVessles.AddNew();
			vessel2.US_HarvestedVessel = "vessel 2";
			vessel2.US_HarvestedCountry = "RU";

			var invoiceLine3 = header.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "0301930000";
			invoiceLine3.US_UC_NKCountryOfOrigin = "RU";
			var nmfsLine3 = invoiceLine3.NMFSLines.AddNew();
			nmfsLine3.US_ProgramType = NMFSProgramCodeList.Codes._370;
			var harvesting3 = nmfsLine3.HarvestingDetails.AddNew();
			harvesting3.US_VesselCountry = "UK";
			harvesting3.US_HarvestedCountry = "CN";
			var harvesting4 = nmfsLine3.HarvestingDetails.AddNew();
			harvesting4.US_VesselCountry = "CN";
			harvesting4.US_HarvestedCountry = "CA";

			invoiceLine1.CopyNMFSDataToSanctions();
			AssertEquals(1, invoiceLine1.FishingInformations.Count);
			AssertFishingInfomation(invoiceLine1.FishingInformations, "SVH", "Declaration Vessel Name", "CA", "CA", "DEC0001");

			invoiceLine2.CopyNMFSDataToSanctions();
			AssertEquals(2, invoiceLine2.FishingInformations.Count);
			AssertFishingInfomation(invoiceLine2.FishingInformations, "HCF", "Declaration Vessel Name", "US", "CA", "DEC0001");
			AssertFishingInfomation(invoiceLine2.FishingInformations, "HCF", "Declaration Vessel Name", "RU", "CA", "0002");

			invoiceLine3.CopyNMFSDataToSanctions();
			AssertEquals(2, invoiceLine3.FishingInformations.Count);
			AssertFishingInfomation(invoiceLine3.FishingInformations, ZString.Empty, "Declaration Vessel Name", "UK", "CN", "DEC0001");
			AssertFishingInfomation(invoiceLine3.FishingInformations, ZString.Empty, "Declaration Vessel Name", "CN", "CA", "DEC0001");

			invoiceLine3.US_DisclaimSanctions = true;
			AssertEquals(0, invoiceLine3.FishingInformations.Count);
			invoiceLine3.CopyNMFSDataToSanctions();
			AssertEquals(0, invoiceLine3.FishingInformations.Count);
		}

		void AssertFishingInfomation(FishingInformationCollection fishingInformations, ZString methodOfHarvest, ZString vesselName, ZString vesselFlag, ZString countryOfHarvest, ZString vesselIMO)
		{
			Assert(fishingInformations.Cast<FishingInformation>()
				.Any(x => x.US_MethodOfHarvest == methodOfHarvest && x.US_VesselName == vesselName
				&& x.US_VesselCountry == vesselFlag && x.US_HarvestedCountry == countryOfHarvest && x.US_VesselIMO == vesselIMO));
		}

		void SetUpFishingAndMiningTariff()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates);
			var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.Russia, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tradeGroupCountry = helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Russia);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			var conditionType1 = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, UniversalReferenceConstants.TariffConditionTypes.Codes.Fishing);
			var conditionType2 = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, UniversalReferenceConstants.TariffConditionTypes.Codes.Mining);
			Factory.Save();

			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "0301930000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var condition1 = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, conditionType1.PK, tariff1.PK, "Fishing Info", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusApplicability(condition1, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			var tariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "7102310000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var condition2 = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, conditionType2.PK, tariff2.PK, "Mining Info", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusApplicability(condition2, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();
		}

		public void TestFWSIndicatorReadOnly()
		{
			var fwsCodeType = Universal.Constants.FunctionalityTypes.EnableFWS;
			var dataGrouping = Core.Constants.CountryCodes.UnitedStates;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(fwsCodeType, dataGrouping, ZDateTime.Today, false))
			{
				invoiceLine.US_FWSInd = ZString.Empty;
				AssertEquals(false, invoiceLine.US_FWSIndInfo.ReadOnly);
				invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Declared;
				AssertEquals(false, invoiceLine.US_FWSIndInfo.ReadOnly);
				invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Disclaimed;
				AssertEquals(false, invoiceLine.US_FWSIndInfo.ReadOnly);
			}
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(fwsCodeType, dataGrouping, ZDateTime.Today, true))
			{
				invoiceLine.US_FWSInd = ZString.Empty;
				AssertEquals(false, invoiceLine.US_FWSIndInfo.ReadOnly);
				invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Declared;
				AssertEquals(false, invoiceLine.US_FWSIndInfo.ReadOnly);
				invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Disclaimed;
				AssertEquals(false, invoiceLine.US_FWSIndInfo.ReadOnly);
			}

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(fwsCodeType, dataGrouping, ZDateTime.Today, false))
			{
				invoiceLine.US_FWSInd = ZString.Empty;
				AssertEquals(true, invoiceLine.US_FWSIndInfo.ReadOnly);
				invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Declared;
				AssertEquals(false, invoiceLine.US_FWSIndInfo.ReadOnly);
				invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Disclaimed;
				AssertEquals(false, invoiceLine.US_FWSIndInfo.ReadOnly);
			}
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(fwsCodeType, dataGrouping, ZDateTime.Today, true))
			{
				invoiceLine.US_FWSInd = ZString.Empty;
				AssertEquals(false, invoiceLine.US_FWSIndInfo.ReadOnly);
				invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Declared;
				AssertEquals(false, invoiceLine.US_FWSIndInfo.ReadOnly);
				invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Disclaimed;
				AssertEquals(false, invoiceLine.US_FWSIndInfo.ReadOnly);
			}
		}

		public void TestDefaultPGAContactInfoInTSCAODSForStandaloneCommercialInvoice()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			invoiceHeader.JZ_OH_Supplier = Factory.New<OrgHeader>().PK;
			var importer = Factory.New<OrgHeader>();
			invoiceHeader.JZ_OH_Buyer = importer.PK;
			invoiceHeader.JZ_MessageType = JobMessageTypeList.Codes.Import;
			invoiceHeader.JZ_InvoiceNumber = "INV123";
			_ = new FakeDeclarationCreatorForInvoice(invoiceHeader);
			DeclarationTestHelper.AddPGAContact(importer, "FIRST", "LAST", "123456", "ior@ian.com", null);

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "TEST PRODUCT";
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_FormattedTariffNum = "4412.99.9700";
			pivot.CD_ODSIndicator = OGAIndicatorList.Codes.Declared;
			pivot.CD_TSCAIndicator = OGAIndicatorList.Codes.Declared;
			pivot.US_TSCACertification = TSCAIndicatorList.Codes.TSCANegative;
			pivot.US_TSCAODSCertIndividual = PartyTypeList.Codes.Importer;
			product.RelatedOrganisations.RemoveAndDeleteAll();
			product.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			invoiceLine.JI_PartNo = product.OP_PartNum;

			AssertEquals("FIRST LAST", invoiceLine.US_FDAContactName);
			AssertEquals("123456", invoiceLine.US_FDAContactPhoneNo);
			AssertEquals("ior@ian.com", invoiceLine.US_FDAContactEmail);
		}

		public void TestProvProgAdditionalTariffThatUsedInInvoiceLineReport()
		{
			#region set up tariff
			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_DateFrom = ZDateTime.Today.AddDays(-1);
			tariff1.UE_DateTo = ZDateTime.Today.AddDays(1);
			tariff1.UE_Column1RateAdValorem = 0.1m;
			tariff1.UE_Tariff = "11111111";
			tariff1.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;

			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_DateFrom = ZDateTime.Today.AddDays(-1);
			tariff2.UE_DateTo = ZDateTime.Today.AddDays(1);
			tariff2.UE_Column1RateAdValorem = 0.2m;
			tariff2.UE_Tariff = "22222222";
			tariff2.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;

			var tariff3 = Factory.New<USCTariff>();
			tariff3.UE_DateFrom = ZDateTime.Today.AddDays(-1);
			tariff3.UE_DateTo = ZDateTime.Today.AddDays(1);
			tariff3.UE_Column1RateAdValorem = 0.3m;
			tariff3.UE_Tariff = "33333333";
			tariff3.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;

			var tariff4 = Factory.New<USCTariff>();
			tariff4.UE_DateFrom = ZDateTime.Today.AddDays(-1);
			tariff4.UE_DateTo = ZDateTime.Today.AddDays(1);
			tariff4.UE_Column1RateAdValorem = 0.4m;
			tariff4.UE_Tariff = "44444444";
			tariff4.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;

			var tariff5 = Factory.New<USCTariff>();
			tariff5.UE_DateFrom = ZDateTime.Today.AddDays(-1);
			tariff5.UE_DateTo = ZDateTime.Today.AddDays(1);
			tariff5.UE_Column1RateAdValorem = 0.5m;
			tariff5.UE_Tariff = "55555555";
			tariff5.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;

			var tariff6 = Factory.New<USCTariff>();
			tariff6.UE_DateFrom = ZDateTime.Today.AddDays(-1);
			tariff6.UE_DateTo = ZDateTime.Today.AddDays(1);
			tariff6.UE_Column1RateAdValorem = 0.6m;
			tariff6.UE_Tariff = "66666666";
			tariff6.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;
			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "11111111";
			invoiceLine.SupFormattedAdditionalTariff1 = "22222222";
			invoiceLine.SupFormattedAdditionalTariff2 = "33333333";
			invoiceLine.SupFormattedAdditionalTariff3 = "44444444";
			invoiceLine.SupFormattedAdditionalTariff4 = "55555555";
			invoiceLine.SupFormattedAdditionalTariff5 = "66666666";
			invoiceLine.US_SupUQ1 = "M3";
			invoiceLine.US_SupAdditionalTariff1UQ = "KG";
			invoiceLine.US_SupAdditionalTariff2UQ = "NO";
			invoiceLine.US_SupAdditionalTariff3UQ = "M2";
			invoiceLine.US_SupAdditionalTariff4UQ = "OZ";
			invoiceLine.US_SupAdditionalTariff5UQ = "L";
			invoiceLine.US_SupQty1 = 1;
			invoiceLine.US_SupAdditionalTariff1Qty = 2;
			invoiceLine.US_SupAdditionalTariff2Qty = 3;
			invoiceLine.US_SupAdditionalTariff3Qty = 4;
			invoiceLine.US_SupAdditionalTariff4Qty = 5;
			invoiceLine.US_SupAdditionalTariff5Qty = 6;

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			AssertEquals("ProvProgTariff", "1111.11.11", invoiceLine.ProvProgTariff);
			AssertEquals("ProvProgDutyRate", "10%", invoiceLine.ProvProgDutyRate);
			AssertEquals("ProvProgCustomsQty", "1.00 M3", invoiceLine.ProvProgCustomsQty);
			AssertEquals("ProvProgTariff1", "2222.22.22", invoiceLine.ProvProgTariff1);
			AssertEquals("ProvProgDutyRate1", "20%", invoiceLine.ProvProgDutyRate1);
			AssertEquals("ProvProgCustomsQty1", "2.00 KG", invoiceLine.ProvProgCustomsQty1);
			AssertEquals("ProvProgTariff2", "3333.33.33", invoiceLine.ProvProgTariff2);
			AssertEquals("ProvProgDutyRate2", "30%", invoiceLine.ProvProgDutyRate2);
			AssertEquals("ProvProgCustomsQty2", "3.00 NO", invoiceLine.ProvProgCustomsQty2);
			AssertEquals("ProvProgTariff3", "4444.44.44", invoiceLine.ProvProgTariff3);
			AssertEquals("ProvProgDutyRate3", "40%", invoiceLine.ProvProgDutyRate3);
			AssertEquals("ProvProgCustomsQty3", "4.00 M2", invoiceLine.ProvProgCustomsQty3);
			AssertEquals("ProvProgTariff4", "5555.55.55", invoiceLine.ProvProgTariff4);
			AssertEquals("ProvProgDutyRate4", "50%", invoiceLine.ProvProgDutyRate4);
			AssertEquals("ProvProgCustomsQty4", "5.00 OZ", invoiceLine.ProvProgCustomsQty4);
			AssertEquals("ProvProgTariff5", "6666.66.66", invoiceLine.ProvProgTariff5);
			AssertEquals("ProvProgDutyRate5", "60%", invoiceLine.ProvProgDutyRate5);
			AssertEquals("ProvProgCustomsQty5", "6.00 L", invoiceLine.ProvProgCustomsQty5);
		}

		public void TestDefaultUS_CottonFeeExemptWhenProductHasCottonFeeExempt()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PART";

			var ownerOrg = Factory.New<OrgHeader>();
			ownerOrg.FillWithValidTestData();

			var ownerRelation = part.RelatedOrganisations.AddNew();
			ownerRelation.OU_OH = ownerOrg.PK;

			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "4412947000";
			pivot.CI_SupplementalTariff = "4412947001";
			pivot.CD_CottonFeeExempt = "N";

			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "4412947000";
			tariff1.UE_DateFrom = new ZDate(2000, 1, 1);
			tariff1.UE_DateTo = ZDateTime.Today.AddDays(10);

			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "4412947001";
			tariff2.UE_DateFrom = new ZDate(2000, 1, 1);
			tariff2.UE_DateTo = ZDateTime.Today.AddDays(10);
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = ownerOrg.PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.US_CottonFeeExempt = "N";
			invoiceLine.JI_Tariff = "4412947000";
			AssertEquals(null, invoiceLine.Pivot);
			AssertEquals("", invoiceLine.US_CottonFeeExempt);

			invoiceLine.US_CottonFeeExempt = "N";
			invoiceLine.US_SupTariff = "4412947000";
			AssertEquals(null, invoiceLine.Pivot);
			AssertEquals("N", invoiceLine.US_CottonFeeExempt);

			invoiceLine.JI_PartNo = part.OP_PartNum;
			AssertEquals(pivot.PK, invoiceLine.Pivot.PK);
			factory.Save();

			invoiceLine.US_CottonFeeExempt = "N";
			invoiceLine.JI_Tariff = "4412947001";
			AssertEquals("", invoiceLine.US_CottonFeeExempt);

			invoiceLine.US_CottonFeeExempt = "N";
			invoiceLine.JI_Tariff = "4412947000";
			AssertEquals("N", invoiceLine.US_CottonFeeExempt);

			invoiceLine.US_CottonFeeExempt = "N";
			invoiceLine.US_SupTariff = "4412947000";
			AssertEquals("N", invoiceLine.US_CottonFeeExempt);

			invoiceLine.US_CottonFeeExempt = "N";
			invoiceLine.US_SupTariff = "4412947001";
			AssertEquals("N", invoiceLine.US_CottonFeeExempt);
		}

		public void TestDefaultUS_ADD_NAAndUS_CVD_NAWhenProductHasADD_CVDNAIndicator()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PART";

			var ownerOrg = Factory.New<OrgHeader>();
			ownerOrg.FillWithValidTestData();

			var ownerRelation = part.RelatedOrganisations.AddNew();
			ownerRelation.OU_OH = ownerOrg.PK;
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "4412947000";
			pivot.CI_SupplementalTariff = "4412947001";
			pivot.CD_ADDApplicable = true;
			pivot.CD_CVDApplicable = true;
			pivot.CD_UC_NKCountryOfOrigin = "CA";

			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "4412947000";
			tariff1.UE_DateFrom = new ZDate(2000, 1, 1);
			tariff1.UE_DateTo = ZDateTime.Today.AddDays(10);

			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "4412947001";
			tariff2.UE_DateFrom = new ZDate(2000, 1, 1);
			tariff2.UE_DateTo = ZDateTime.Today.AddDays(10);
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = ownerOrg.PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.US_ADD_NA = true;
			invoiceLine.US_CVD_NA = true;
			invoiceLine.JI_Tariff = "4412947000";
			AssertEquals(null, invoiceLine.Pivot);
			AssertEquals(false, invoiceLine.US_ADD_NA);
			AssertEquals(false, invoiceLine.US_CVD_NA);

			invoiceLine.US_ADD_NA = true;
			invoiceLine.US_CVD_NA = true;
			invoiceLine.US_UC_NKCountryOfOrigin = "CA";
			AssertEquals(true, invoiceLine.US_ADD_NA);
			AssertEquals(true, invoiceLine.US_CVD_NA);

			invoiceLine.JI_PartNo = part.OP_PartNum;
			AssertEquals(pivot.PK, invoiceLine.Pivot.PK);
			AssertEquals(true, invoiceLine.US_ADD_NA);
			AssertEquals(true, invoiceLine.US_CVD_NA);
			factory.Save();

			invoiceLine.US_ADD_NA = true;
			invoiceLine.US_CVD_NA = true;
			invoiceLine.JI_Tariff = "4412947001";
			AssertEquals(false, invoiceLine.US_ADD_NA);
			AssertEquals(false, invoiceLine.US_CVD_NA);

			invoiceLine.US_ADD_NA = true;
			invoiceLine.US_CVD_NA = true;
			invoiceLine.JI_Tariff = "4412947000";
			AssertEquals(true, invoiceLine.US_ADD_NA);
			AssertEquals(true, invoiceLine.US_CVD_NA);

			invoiceLine.US_ADD_NA = true;
			invoiceLine.US_CVD_NA = true;
			invoiceLine.US_UC_NKCountryOfOrigin = "US";
			AssertEquals(true, invoiceLine.US_ADD_NA);
			AssertEquals(true, invoiceLine.US_CVD_NA);

			invoiceLine.US_ADD_NA = true;
			invoiceLine.US_CVD_NA = true;
			invoiceLine.US_UC_NKCountryOfOrigin = "CA";
			AssertEquals(true, invoiceLine.US_ADD_NA);
			AssertEquals(true, invoiceLine.US_CVD_NA);
		}

		public void TestDefaultPGAIndicatorWhenProductHasPGAIndicator()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PART";

			var ownerOrg = Factory.New<OrgHeader>();
			ownerOrg.FillWithValidTestData();

			var ownerRelation = part.RelatedOrganisations.AddNew();
			ownerRelation.OU_OH = ownerOrg.PK;
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "4412947000";
			pivot.CI_SupplementalTariff = "4412947001";
			pivot.CD_ATFIndicator = OGAIndicatorList.Codes.Declared;
			pivot.CD_DEAIndicator = OGAIndicatorList.Codes.Declared;
			pivot.CD_LaceyActIndicator = OGAIndicatorList.Codes.Declared;

			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "4412947000";
			tariff1.UE_DateFrom = new ZDate(2000, 1, 1);
			tariff1.UE_DateTo = ZDateTime.Today.AddDays(10);

			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "4412947001";
			tariff2.UE_DateFrom = new ZDate(2000, 1, 1);
			tariff2.UE_DateTo = ZDateTime.Today.AddDays(10);
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = ownerOrg.PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.US_ATFInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_DEAInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_LaceyIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.JI_Tariff = "4412947000";
			AssertEquals(null, invoiceLine.Pivot);
			AssertEquals("", invoiceLine.US_ATFInd);
			AssertEquals("", invoiceLine.US_DEAInd);
			AssertEquals("", invoiceLine.US_LaceyIndicator);

			invoiceLine.JI_PartNo = part.OP_PartNum;
			AssertEquals(pivot.PK, invoiceLine.Pivot.PK);
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_ATFInd);
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_DEAInd);
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_LaceyIndicator);
			factory.Save();

			invoiceLine.US_ATFInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_DEAInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_LaceyIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.JI_Tariff = "4412947001";
			AssertEquals("", invoiceLine.US_ATFInd);
			AssertEquals("", invoiceLine.US_DEAInd);
			AssertEquals("", invoiceLine.US_LaceyIndicator);

			invoiceLine.US_ATFInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_DEAInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_LaceyIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.JI_Tariff = "4412947000";
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_ATFInd);
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_DEAInd);
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_LaceyIndicator);
		}

		public void TestSetUS_IsUsedVehicleWhenSetJI_Tariff()
		{
			var startDate = ZDateTime.Now.AddMonths(-1);
			var endDate = ZDateTime.Now.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates).ZZZ_DataGrouping;
			var tariffType1 = helper.CreateNewOrGetExistingTariffType(dataGrouping, Universal.Constants.TariffTypes.ScheduleB);
			var tariffType2 = helper.CreateNewOrGetExistingTariffType(dataGrouping, Universal.Constants.TariffTypes.Export);
			var tariff1 = helper.LoadOrCreateNewTariff(dataGrouping, tariffType1.PK, "3333333333", startDate, endDate);
			var tariff2 = helper.LoadOrCreateNewTariff(dataGrouping, tariffType2.PK, "4444444444", startDate, endDate);
			var tariff3 = helper.LoadOrCreateNewTariff(dataGrouping, tariffType1.PK, "5555555555", startDate, endDate);
			var attributeName1 = helper.CreateNewOrGetExistingRefCusTariffAttributeName(Factory, UniversalReferenceConstants.TariffAttributeTypes.Codes.EV1, UniversalReferenceConstants.TariffAttributeTypes.Codes.EV1, UniversalReferenceConstants.TariffAttributeTypes.Codes.EV1, dataGrouping, tariffType1.ZZI_TariffType);
			var attributeName2 = helper.CreateNewOrGetExistingRefCusTariffAttributeName(Factory, UniversalReferenceConstants.TariffAttributeTypes.Codes.EV1, UniversalReferenceConstants.TariffAttributeTypes.Codes.EV1, UniversalReferenceConstants.TariffAttributeTypes.Codes.EV1, dataGrouping, tariffType2.ZZI_TariffType);
			var attribute1 = helper.CreateNewOrGetExistingTariffAttribute(attributeName1.ZY6_Name, UniversalReferenceConstants.TariffAttributeTypes.Values.Mandatory, tariff1);
			var attribute2 = helper.CreateNewOrGetExistingTariffAttribute(attributeName2.ZY6_Name, UniversalReferenceConstants.TariffAttributeTypes.Values.Mandatory, tariff2);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceLine.US_TariffType = TariffTypeList.Codes.ScheduleB;
			invoiceLine.JI_Tariff = tariff1.ZZ1_TariffCode;
			AssertEquals(true, invoiceLine.US_IsUsedVehicle);

			invoiceLine.US_IsUsedVehicle = false;
			invoiceLine.JI_Tariff = tariff2.ZZ1_TariffCode;
			AssertEquals(false, invoiceLine.US_IsUsedVehicle);

			invoiceLine.JI_Tariff = tariff3.ZZ1_TariffCode;
			AssertEquals(false, invoiceLine.US_IsUsedVehicle);

			invoiceLine.US_TariffType = TariffTypeList.Codes.HTS;
			invoiceLine.JI_Tariff = tariff2.ZZ1_TariffCode;
			AssertEquals(true, invoiceLine.US_IsUsedVehicle);
		}

		public void TestExportInvoiceLineDefaultPGAIndicatorsWhenTariffChanges()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var shBTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.ScheduleB);
			var exportTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.Export);
			var conditionType = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, UniversalReferenceConstants.TariffConditionTypes.Codes.PGA);
			var shbTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, shBTariffType.PK, "0000000001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var condition1 = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, conditionType.PK, shbTariff.PK, "PGA Data requirements", false, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			var shbTariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, shBTariffType.PK, "0000000002", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var condition2 = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, conditionType.PK, shbTariff1.PK, "PGA Data requirements", false, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			AddConditionValue(helper, condition2);

			var exportTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, exportTariffType.PK, "0000000003", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var condition3 = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, conditionType.PK, exportTariff.PK, "PGA Data requirements", false, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			var exportTariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, exportTariffType.PK, "0000000004", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var condition4 = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, conditionType.PK, exportTariff1.PK, "PGA Data requirements", false, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			AddConditionValue(helper, condition4);

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.US_TariffType = TariffTypeList.Codes.ScheduleB;
			invoiceLine1.JI_Tariff = "0000000001";
			AssertExportPGAIndicators(ZString.Empty, invoiceLine1);
			invoiceLine1.JI_Tariff = ZString.Empty;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.US_TariffType = TariffTypeList.Codes.HTS;
			invoiceLine2.JI_Tariff = "0000000003";
			AssertExportPGAIndicators(ZString.Empty, invoiceLine2);
			invoiceLine2.JI_Tariff = ZString.Empty;

			invoiceLine1.JI_Tariff = "0000000002";
			AssertExportPGAIndicators(OGAIndicatorList.Codes.Declared, invoiceLine1);

			invoiceLine2.JI_Tariff = "0000000004";
			AssertExportPGAIndicators(OGAIndicatorList.Codes.Declared, invoiceLine2);

			invoiceLine1.US_TariffType = TariffTypeList.Codes.HTS;
			AssertExportPGAIndicators(ZString.Empty, invoiceLine1);

			invoiceLine2.US_TariffType = TariffTypeList.Codes.ScheduleB;
			AssertExportPGAIndicators(ZString.Empty, invoiceLine2);

			invoiceLine1.JI_Tariff = "0000000004";
			AssertExportPGAIndicators(OGAIndicatorList.Codes.Declared, invoiceLine1);

			invoiceLine2.JI_Tariff = "0000000002";
			AssertExportPGAIndicators(OGAIndicatorList.Codes.Declared, invoiceLine2);
		}

		void AssertExportPGAIndicators(string expectValue, JobComInvoiceLine invoiceLine)
		{
			AssertEquals(expectValue, invoiceLine.US_AMSInd);
			AssertEquals(expectValue, invoiceLine.US_ATFInd);
			AssertEquals(expectValue, invoiceLine.US_FWSInd);
			AssertEquals(expectValue, invoiceLine.US_PSTIndicator);
			AssertEquals(expectValue, invoiceLine.US_NMFSHMSInd);
			AssertEquals(expectValue, invoiceLine.US_TTBInd);
		}

		void AddConditionValue(UniversalReferenceTestDataHelper helper, RefCusCondition condition)
		{
			var pgas = new string[] {
				GovernmentAgencyProgramCodeList.Codes.AMS,
				GovernmentAgencyProgramCodeList.Codes.EPA,
				GovernmentAgencyProgramCodeList.NMFS,
				GovernmentAgencyProgramCodeList.Codes.ATF,
				GovernmentAgencyProgramCodeList.Codes.FWS,
				GovernmentAgencyProgramCodeList.Codes.TTB
			};
			foreach (var conditionValueType in pgas)
			{
				var refCusConditionValueType = helper.CreateOrGetExistingRefCusConditionValueType(Core.Constants.CountryCodes.UnitedStates, conditionValueType);
				helper.CreateOrGetExistingRefCusConditionValue(refCusConditionValueType.PK, condition.PK, UniversalReferenceConstants.TariffConditionValue.Values.Mandatory);
			}
		}

		public void TestCustomsUQDefaultedBySHBTariffExpiredWithin30Days()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var shBTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.ScheduleB);
			Factory.Save();
			var shbTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, shBTariffType.PK, "0000000001", ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddDays(-20));
			helper.CreateTariffUOM(shbTariff, "CU1", "A");
			helper.CreateTariffUOM(shbTariff, "CU2", "B");
			helper.CreateTariffUOM(shbTariff, "CU3", "C");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.US_TariffType = TariffTypeList.Codes.ScheduleB;
			declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();

			invoiceLine.US_DateOfExport = ZDateTime.Today;
			invoiceLine.US_TariffType = TariffTypeList.Codes.ScheduleB;
			invoiceLine.JI_Tariff = "0000000001";

			AssertNull(invoiceLine.ScheduleBTariff);
			AssertNotNull(invoiceLine.TariffExpirationDateWinin30Days);

			AssertEquals("A", invoiceLine.JI_CustomsUnitQty);
			AssertEquals("B", invoiceLine.JI_CustomsSecondUnitQty);
			AssertEquals("C", invoiceLine.JI_CustomsThirdUnitQty);
		}

		public void TestTariffExpirationDateWinin30Days()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var shBTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.ScheduleB);
			Factory.Save();
			var shbTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, shBTariffType.PK, "0000000001", ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddDays(-20));

			var shbTariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, shBTariffType.PK, "0000000002", ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddDays(-40));

			var shbTariff3 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, shBTariffType.PK, "0000000003", ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddDays(10));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.US_TariffType = TariffTypeList.Codes.ScheduleB;
			declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();

			invoiceLine.US_DateOfExport = ZDateTime.Today;
			invoiceLine.US_TariffType = TariffTypeList.Codes.ScheduleB;
			invoiceLine.JI_Tariff = "0000000001";

			AssertNull(invoiceLine.ScheduleBTariff);
			AssertEquals(shbTariff.PK, invoiceLine.TariffExpirationDateWinin30Days.PK);

			invoiceLine.JI_Tariff = "0000000002";
			AssertNull(invoiceLine.ScheduleBTariff);
			AssertNull(invoiceLine.TariffExpirationDateWinin30Days);

			invoiceLine.JI_Tariff = "0000000003";
			AssertEquals(shbTariff3.PK, invoiceLine.ScheduleBTariff.PK);
			AssertEquals(shbTariff3.PK, invoiceLine.TariffExpirationDateWinin30Days.PK);
		}

		public void TestHasPGAIndicatorsForHFC()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			Assert(!invoiceLine.HasPGAIndicators);
			invoiceLine.US_HFCInd = OGAIndicatorList.Codes.Declared;
			Assert(invoiceLine.HasPGAIndicators);
			invoiceLine.US_HFCInd = OGAIndicatorList.Codes.Disclaimed;
			Assert(invoiceLine.HasPGAIndicators);
		}

		public void TestPGADataCorrectionsForHFC()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.US_HFCInd = OGAIndicatorList.Codes.Declared;
			var hfcHeader = invoiceLine.USHFCHeaders.AddNew();
			AssertCollectionContains(hfcHeader, invoiceLine.PGADataCorrections);
		}

		public void TestDeletaForHFC()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.US_HFCInd = OGAIndicatorList.Codes.Declared;
			var hfcHeader = invoiceLine.USHFCHeaders.AddNew();
			invoiceLine.Delete();
			Assert(hfcHeader.IsDeleted);
		}

		public void TestUS_FTZCurrentTariffCanFormatInvalidCharacters()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.FTZCurrentTariffFormatted = "A";
			AssertEquals(ZString.Empty, invoiceLine.US_FTZCurrentTariff);
			AssertEquals(ZString.Empty, invoiceLine.US_FTZCurrentTariff);

			invoiceLine.FTZCurrentTariffFormatted = "123.234.567A";
			AssertEquals("123234567", invoiceLine.US_FTZCurrentTariff);
			AssertEquals("1232.34.567", invoiceLine.FTZCurrentTariffFormatted);
		}

		public void TestSynchroniseOriginFromForwardingOrderLine()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			invoiceHeader.JZ_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var order = Factory.New<Order>();
			order.JD_OrderNumber = "090909";
			var orderLine = order.OrderLines.AddNew();
			orderLine.JO_F3_NKPackType = "UNT";
			orderLine.JO_QtyInvoiced = 1;

			((IBaseJobComInvoiceLine)invoiceLine).SynchroniseFromForwardingOrderLine(orderLine, true);
			AssertEquals(ZString.Empty, invoiceLine.CountryOfOriginFieldInfo.Value);

			order.BuyerPK = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			invoiceHeader.JZ_OH_Buyer = order.BuyerPK;
			invoiceHeader.JZ_OH_Supplier = order.BuyerPK;
			var product = Factory.New<OrgSupplierPart>();
			product.RelatedOrganisations.AddOrganisationIfNotExist(order.BuyerPK, OrgPartRelation.RelationshipTypes.Owner);
			product.OP_PartNum = "AMYTEST";
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CD_UC_NKCountryOfOrigin = "US";
			orderLine.JO_Partno = product.OP_PartNum;
			Factory.Save();
			((IBaseJobComInvoiceLine)invoiceLine).SynchroniseFromForwardingOrderLine(orderLine, true);
			AssertEquals("US", invoiceLine.CountryOfOriginFieldInfo.Value);

			orderLine.JO_RN_NKCountryOfOrigin = "CN";
			Factory.Save();
			((IBaseJobComInvoiceLine)invoiceLine).SynchroniseFromForwardingOrderLine(orderLine, true);
			AssertEquals("CN", invoiceLine.CountryOfOriginFieldInfo.Value);
		}

		public void TestIsAluminumSmeltEffective()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.USSmelt, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, false))
			{
				Assert(!invoiceLine.IsAluminumSmeltEffective);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.USSmelt, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				Assert(invoiceLine.IsAluminumSmeltEffective);
			}
		}

		public JobComInvoiceHeader SetupDeclarationInvoiceHeader()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffHelper = new USCTariffTestCase(Factory);
			var startDate = new ZDate(2024, 01, 01);
			var endDate = new ZDate(2079, 06, 06);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "HSN");
			Factory.Save();
			helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "8401100000", startDate, endDate);
			helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "4101201010", startDate, endDate);
			var tariff9903 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99038801", startDate, endDate);
			tariffHelper.CreateNewTariffIfNotExists("8401100000", startDate, endDate, "KG", "", "", "");
			tariffHelper.CreateNewTariffIfNotExists("4101201010", startDate, endDate, "KG", "", "", "");
			tariffHelper.CreateNewTariffIfNotExists("99038801", startDate, endDate, "KG", "", "", "");
			var dutyRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DTY", dutyRateType.PK);
			Factory.Save();
			var rate = helper.CreateRate(tariff9903, rateCode.PK, startDate, endDate, "0.25");
			var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.HongKong, "HK", startDate, endDate);
			var tradeGroupCountry = helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.HongKong, startDate, endDate);
			var applicability = helper.CreateCusApplicability(rate, tradeGroup, startDate, endDate);
			var relationship = helper.CreateTariffRelationship(tariff9903.PK, hsnTariffType.PK, "84");
			var tariffAttribute = helper.CreateTariffAttribute("RULE", "A99", tariff9903);
			Factory.Save();

			var testHelper = new UniversalReferenceTestDataHelper(Factory);
			var currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var tradeGroupStandard = testHelper.CreateTradeGroup(currentCountryCode, "STANDARD", startDate, endDate);
			testHelper.AddCountry(tradeGroupStandard, Core.Constants.CountryCodes.Australia, startDate, endDate);
			Factory.Save();
			var cusTariff = testHelper.CreateTariff(currentCountryCode, hsnTariffType.PK, "TARFFTES1", startDate, endDate, "TARFFTES1");
			var cusTariff2 = testHelper.CreateTariff(currentCountryCode, hsnTariffType.PK, "SUPTARFF1", startDate, endDate, "SUPTARFF1");
			var dutyRateType2 = testHelper.CreateNewOrGetExistingRateType(currentCountryCode, Universal.Constants.RateTypes.Duty, "Duty");
			Factory.Save();

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "AABBCC";
			part.OP_Weight = 100m;
			part.OP_WeightUQ = "HG";

			var consignor = Factory.New<OrgHeader>();
			consignor.FillWithValidTestData();
			consignor.OH_Code = "ORGAUSYD";
			consignor.OH_IsConsignor = ZBool.True;
			consignor.OH_RL_NKClosestPort = "AUSYD";
			var consignee = Factory.New<OrgHeader>();
			consignee.FillWithValidTestData();
			consignee.OH_Code = "ORGUSCHI";
			consignee.OH_IsConsignee = ZBool.True;
			consignee.OH_RL_NKClosestPort = "USCHI";

			var supplierRelation = part.RelatedOrganisations.AddSupplier(consignor);
			part.RelatedOrganisations.AddOwner(consignee);

			var foreignExporterOnPart = Factory.New<OrgHeader>();
			foreignExporterOnPart.FillWithValidTestData();
			foreignExporterOnPart.OH_Code = "FREXPART";
			foreignExporterOnPart.MainAddress.AddressCode = "2213";

			var importPivot = part.PivotsForBinding.AddNew();
			importPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			importPivot.CI_OH = supplierRelation.OU_OH;
			importPivot.CI_TariffNum = "8401100000";
			importPivot.CI_DateStart = startDate;
			importPivot.CI_DateEnd = endDate;
			importPivot.CI_SupplementalTariff = "99038801";
			importPivot.CD_OA_Exporter_ZAddress.OrgPK = foreignExporterOnPart.PK;
			importPivot.CI_LastAuditedDate = startDate.AddDays(1);

			var relatedPivot = importPivot.Children.AddNew();
			relatedPivot.CI_ChildType = ClassificationChildTypeList.Codes.Related;
			relatedPivot.CI_TariffNum = "99038801";
			relatedPivot.CD_PerUnitCost = 2;
			relatedPivot.CD_OA_Exporter_ZAddress.OrgPK = foreignExporterOnPart.PK;
			relatedPivot.CI_LastAuditedDate = startDate.AddDays(1);

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Buyer = consignee.PK;
			invoice.JZ_OH_Supplier = consignor.PK;
			return invoice;
		}

		public void TestInvoiceLineLoadingRefreshPartDetails()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			const string PartNo = "WEIGHTTEST";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = PartNo;
			var secondaryInvoiceLine = invoiceLine.AddSecondaryInvoiceLine();
			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var product = newFactory.New<OrgSupplierPart>();
			product.OP_PartNum = PartNo;
			product.OP_Desc = "WEIGHTTEST";
			product.RelatedOrganisations.AddOwner(importer);
			product.OP_Weight = 10;
			product.OP_WeightUQ = Core.Constants.Weight.Kilograms;
			product.OP_NetWeight = 9;
			product.OP_StockKeepingUnit = "UNT";

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "9101000010";
			pivot.CI_SupplementalTariff = "9101000020";

			var relatedTariffPivot1 = pivot.Children.AddNew();
			relatedTariffPivot1.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			relatedTariffPivot1.CI_TariffNum = "8101000010";
			relatedTariffPivot1.CI_SupplementalTariff = "8101000020";
			newFactory.Save();

			newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			declaration = newFactory.Load<JobDeclaration>(declaration.PK);
			AssertEquals("declaration.InvoiceLines.Count", 2, declaration.InvoiceLines.Count);
			invoiceLine = (JobComInvoiceLine)declaration.InvoiceLines.FindByPK(invoiceLine.PK);
			AssertEquals("invoiceLine.US_SupTariff", "9101000020", invoiceLine.US_SupTariff);
			var childLine = invoiceLine.ChildLines.Single();
			AssertEquals("childLine.US_SupTariff", "8101000020", childLine.US_SupTariff);
		}

		public void TestWhenSeverityLevelOfInvoiceLineValidationERR()
		{
			var foreignExporterOnPart = Factory.New<OrgHeader>();
			foreignExporterOnPart.FillWithValidTestData();
			foreignExporterOnPart.OH_Code = "FREXPART";
			foreignExporterOnPart.MainAddress.AddressCode = "2213";

			var foreignExporter = Factory.New<OrgHeader>();
			foreignExporter.FillWithValidTestData();
			foreignExporter.OH_Code = "FRNEXPER";
			foreignExporter.MainAddress.AddressCode = "2000";

			var msg = "Sup Tariff does not match product code file.";
			var msgExporterAddress = "Exporter does not match product code file.";

			var declaration = Factory.New<JobDeclaration>();
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "WEIGHTTEST";
			product.OP_Desc = "WEIGHTTEST";
			product.RelatedOrganisations.AddOwner(importer);
			product.OP_Weight = 10;
			product.OP_WeightUQ = Core.Constants.Weight.Kilograms;
			product.OP_NetWeight = 9;
			product.OP_StockKeepingUnit = "UNT";

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "9101000010";
			pivot.CI_SupplementalTariff = "9101000011";
			pivot.CD_PerUnitCost = 0m;
			pivot.CD_RX_NKPerUnitCostCurr = "USD";
			pivot.CD_OA_Exporter_ZAddress.OrgPK = foreignExporterOnPart.PK;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = product.OP_PartNum;
			invoiceLine.UpdateDetailsOnPartChange();
			AssertEquals("Tariff defaulted", "9101000010", invoiceLine.JI_Tariff);
			AssertEquals("Tariff defaulted", "9101000011", invoiceLine.US_SupTariff);
			AssertEquals("Only 1 line created", 1, invoice.InvoiceLines.Count);

			var exporterGuidOnPart = invoiceLine.Pivot.Details.CD_OA_Exporter_ZAddress.OrgPK;

			using (CustomsDataRegistry.Instance.SeverityLevelOfInvoiceLineValidationAgainstProductData.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, "ERR"))
			{
				AssertNoMessageError(invoiceLine.US_SupTariffInfo, msg);

				invoiceLine.US_SupTariff = "9801001010";
				AssertHasMessageError(invoiceLine.US_SupTariffInfo, msg);

				invoiceLine.ForeignExporterOrgPK = exporterGuidOnPart;
				AssertNoMessageError(invoiceLine.JI_OA_ExporterAddressInfo, msgExporterAddress);
				invoiceLine.ForeignExporterOrgPK = foreignExporter.PK;
				AssertHasMessageError(invoiceLine.JI_OA_ExporterAddressInfo, msgExporterAddress);
			}
		}

		public void TestWhenSeverityLevelOfInvoiceLineValidationWRN()
		{
			var foreignExporterOnPart = Factory.New<OrgHeader>();
			foreignExporterOnPart.FillWithValidTestData();
			foreignExporterOnPart.OH_Code = "FREXPART";
			foreignExporterOnPart.MainAddress.AddressCode = "2213";

			var foreignExporter = Factory.New<OrgHeader>();
			foreignExporter.FillWithValidTestData();
			foreignExporter.OH_Code = "FRNEXPER";
			foreignExporter.MainAddress.AddressCode = "2000";

			var msg = "Sup Tariff does not match product code file.";
			var msgExporterAddress = "Exporter does not match product code file.";

			var declaration = Factory.New<JobDeclaration>();
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "WEIGHTTEST";
			product.OP_Desc = "WEIGHTTEST";
			product.RelatedOrganisations.AddOwner(importer);
			product.OP_Weight = 10;
			product.OP_WeightUQ = Core.Constants.Weight.Kilograms;
			product.OP_NetWeight = 9;
			product.OP_StockKeepingUnit = "UNT";

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "9101000010";
			pivot.CI_SupplementalTariff = "9101000011";
			pivot.CD_PerUnitCost = 0m;
			pivot.CD_RX_NKPerUnitCostCurr = "USD";
			pivot.CD_OA_Exporter_ZAddress.OrgPK = foreignExporterOnPart.PK;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = product.OP_PartNum;
			invoiceLine.UpdateDetailsOnPartChange();
			AssertEquals("Tariff defaulted", "9101000010", invoiceLine.JI_Tariff);
			AssertEquals("Tariff defaulted", "9101000011", invoiceLine.US_SupTariff);
			AssertEquals("Only 1 line created", 1, invoice.InvoiceLines.Count);

			var exporterGuidOnPart = invoiceLine.Pivot.Details.CD_OA_Exporter_ZAddress.OrgPK;

			using (CustomsDataRegistry.Instance.SeverityLevelOfInvoiceLineValidationAgainstProductData.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, "WRN"))
			{
				AssertNoWarning(invoiceLine.US_SupTariffInfo, msg);

				invoiceLine.US_SupTariff = "9801001010";
				AssertHasWarning(invoiceLine.US_SupTariffInfo, msg);

				invoiceLine.ForeignExporterOrgPK = exporterGuidOnPart;
				AssertNoWarning(invoiceLine.JI_OA_ExporterAddressInfo, msgExporterAddress);
				invoiceLine.ForeignExporterOrgPK = foreignExporter.PK;
				AssertHasWarning(invoiceLine.JI_OA_ExporterAddressInfo, msgExporterAddress);
			}
		}

		public void TestResourceStringOnUS_RN_NKCertOrigin()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			AssertEquals("Certificate Of Origin", Enterprise.Customs.Business.ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(invoiceLine.US_RN_NKCertOriginInfo).Caption);
			AssertEquals("Cert. Orig.", Enterprise.Customs.Business.ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(invoiceLine.US_RN_NKCertOriginInfo).ShortCaption);
		}

		public void TestResourceStringOnUS_RN_NKMeltCtry()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			AssertEquals("Melted/Poured Country/Region", Enterprise.Customs.Business.ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(invoiceLine.US_RN_NKMeltCtryInfo).Caption);
			AssertEquals("Melt. Ctry/Rgn.", Enterprise.Customs.Business.ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(invoiceLine.US_RN_NKMeltCtryInfo).ShortCaption);
		}

		public void TestIsAluminumSmeltAndCastCountryClaimed()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			Assert(!invoiceLine.IsAluminumSmeltAndCastCountryClaimed);
			invoiceLine.US_Prim_NA = true;
			Assert(invoiceLine.IsAluminumSmeltAndCastCountryClaimed);
			invoiceLine.US_Prim_NA = false;
			invoiceLine.US_RN_NKPrimCtry = Core.Constants.CountryCodes.UnitedStates;
			Assert(invoiceLine.IsAluminumSmeltAndCastCountryClaimed);
			invoiceLine.US_RN_NKPrimCtry = ZString.Empty;
			invoiceLine.US_Sec_NA = true;
			Assert(invoiceLine.IsAluminumSmeltAndCastCountryClaimed);
			invoiceLine.US_Sec_NA = false;
			invoiceLine.US_RN_NKSecCtry = Core.Constants.CountryCodes.UnitedStates;
			Assert(invoiceLine.IsAluminumSmeltAndCastCountryClaimed);
			invoiceLine.US_RN_NKSecCtry = ZString.Empty;
			invoiceLine.US_RN_NKCastCtry = Core.Constants.CountryCodes.UnitedStates;
			Assert(invoiceLine.IsAluminumSmeltAndCastCountryClaimed);
			invoiceLine.US_RN_NKCastCtry = ZString.Empty;
			Assert(!invoiceLine.IsAluminumSmeltAndCastCountryClaimed);
		}

		public void TestResourceStringDataOfAlumlnumSmeltAndCastCountryFields()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.US_Prim_NA), false, attribute => attribute.ShortCaption == "Prim. Ctry/Rgn. N/A" && attribute.Caption == "Primary Country/Region N/A");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.US_RN_NKPrimCtry), false, attribute => attribute.ShortCaption == "Prim. Ctry/Rgn." && attribute.Caption == "Primary Country/Region");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.US_Sec_NA), false, attribute => attribute.ShortCaption == "Sec. Ctry/Rgn. N/A" && attribute.Caption == "Secondary Country/Region N/A");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.US_RN_NKSecCtry), false, attribute => attribute.ShortCaption == "Sec. Ctry/Rgn." && attribute.Caption == "Secondary Country/Region");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.US_RN_NKCastCtry), false, attribute => attribute.ShortCaption == "Cast Ctry/Rgn." && attribute.Caption == "Country/Region of Cast");
		}

		[ExpectNoExceptions]
		public void TestAllAddInfoColumnsAreInModelView()
		{
			var jobComInvoiceLine = Factory.New<JobComInvoiceLine>();
			ModelViewTestHelper.AssertAllAddInfoColumnsAreInModelView(jobComInvoiceLine,
				"USJobComInvoiceLine",
				schemaTypeName: nameof(AutoJobComInvoiceLine.Schema));
		}

		public override void TestWipeNKTaxType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			Assert(!invoiceLine.ShouldWipeNKTaxType);
		}

		public void TestSteelOriginFromEUN()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "72000000001";
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.HongKong;
			AssertEquals("Steel Tariff + Not From EUN", false, invoiceLine.IsSteelOriginFromEUN);

			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Germany;
			AssertEquals("Steel Tariff + Orgin From EUN", true, invoiceLine.IsSteelOriginFromEUN);

			invoiceLine.JI_Tariff = "76000000001";
			AssertEquals("Aluminum Tariff + Orgin From EUN", false, invoiceLine.IsSteelOriginFromEUN);
		}

		public void TestIsSteelOriginFromEUNForAnySupTariff()
		{
			var startDate = ZDateTime.Now.AddMonths(-1);
			var endDate = ZDateTime.Now.AddMonths(1);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates);
			var tariffType = helper.CreateNewOrGetExistingTariffType(dataGrouping.ZZZ_DataGrouping, Enterprise.Customs.Universal.Constants.TariffTypes.HarmonizedSystem);
			var tradeGroupEUN = helper.LoadOrCreateTradeGroup(dataGrouping.ZZZ_DataGrouping, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			var tradeGroupCDS = helper.LoadOrCreateTradeGroup(dataGrouping.ZZZ_DataGrouping, Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService);
			var tradeGroupCountry01 = helper.AddCountry(tradeGroupEUN, Core.Constants.CountryCodes.Italy, startDate.Date, endDate.Date);
			var tradeGroupCountry02 = helper.AddCountry(tradeGroupCDS, Core.Constants.CountryCodes.Italy, startDate.Date, endDate.Date);

			var rateType = helper.CreateNewOrGetExistingRateType(dataGrouping.ZZZ_DataGrouping, Universal.Constants.RateTypes.Duty);
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RateCodes.Codes.Duty, rateType.PK);

			var parent01 = helper.LoadOrCreateNewTariff(dataGrouping.ZZZ_DataGrouping, tariffType.PK, "7304390008", startDate, endDate);
			var parent02 = helper.LoadOrCreateNewTariff(dataGrouping.ZZZ_DataGrouping, tariffType.PK, "7304390002", startDate, endDate);

			var child01 = helper.LoadOrCreateNewTariff(dataGrouping.ZZZ_DataGrouping, tariffType.PK, "99038085", startDate, endDate);
			var child02 = helper.LoadOrCreateNewTariff(dataGrouping.ZZZ_DataGrouping, tariffType.PK, "99038081", startDate, endDate);

			var attribute01 = helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, child01);
			var attribute02 = helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values.BabyFomula, child01);
			var attribute03 = helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, child02);
			var attribute04 = helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values.BabyFomula, child02);

			var rate01 = helper.CreateRefCusRate(child01.PK, rateCode.PK, startDate, endDate);
			var rate02 = helper.CreateRefCusRate(child02.PK, rateCode.PK, startDate, endDate);

			var applicability01 = helper.CreateCusApplicability(rate01.PK, tradeGroupEUN, startDate, endDate);
			var applicability02 = helper.CreateCusApplicability(rate02.PK, tradeGroupCDS, startDate, endDate);

			var relation01 = helper.CreateTariffRelationship(child01.PK, tariffType.PK, parent01.ZZ1_TariffCode);
			var relation02 = helper.CreateTariffRelationship(child02.PK, tariffType.PK, parent02.ZZ1_TariffCode);

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "7304390008";
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Italy;
			var supTariffs = invoiceLine.ApplicableSupTariffs;
			AssertEquals(1, supTariffs.Length);
			AssertEquals("Steel Tariff + Orgin From EUN + one of sup tariffs is origin from EUN", true, invoiceLine.IsSteelOriginFromEUNForAnySupTariff);

			invoiceLine.JI_Tariff = "7304390002";
			supTariffs = invoiceLine.ApplicableSupTariffs;
			AssertEquals(1, supTariffs.Length);
			AssertEquals("Steel Tariff + Orgin From EUN + none of sup tariffs are origin from EUN", false, invoiceLine.IsSteelOriginFromEUNForAnySupTariff);
		}

		public void TestProvideNotApplicableCodeForConditionTypeDESIGAndValueOption()
		{
			#region Setup Tariffs

			var helper = new UniversalReferenceTestDataHelper(Factory);

			var allTradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.UnitedStates, "All Countries", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.AddCountry(allTradeGroup, Core.Constants.CountryCodes.Ukraine, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			helper.AddCountry(allTradeGroup, Core.Constants.CountryCodes.Australia, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			helper.AddCountry(allTradeGroup, Core.Constants.CountryCodes.Canada, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			var canadaTradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.Canada, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.AddCountry(canadaTradeGroup, Core.Constants.CountryCodes.Canada, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			var unitedStatesTradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.UnitedStates, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.AddCountry(unitedStatesTradeGroup, Core.Constants.CountryCodes.UnitedStates, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "HSN");
			var smeltConditionType = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Rate, UniversalReferenceConstants.TariffConditionTypes.Codes.SMELT);
			var meltConditionType = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, UniversalReferenceConstants.TariffConditionTypes.Codes.MELT);
			var conditionValueTypeForEntry = helper.CreateOrGetExistingRefCusConditionValueType(Core.Constants.CountryCodes.UnitedStates, UniversalReferenceConstants.TariffConditionValueTypes.Codes.Entry);
			var conditionValueTypeForDesig = helper.CreateOrGetExistingRefCusConditionValueType(Core.Constants.CountryCodes.UnitedStates, UniversalReferenceConstants.TariffConditionValueTypes.Codes.DESIG);
			var dutyRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.RateTypes.Duty);
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, Universal.Constants.RateTypes.Duty, dutyRateType.PK);

			Factory.Save();

			var tariff7201101000 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "7201101000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariff99038001 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "99038001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariff99038001);
			helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values._232, tariff99038001);
			var meltConditionFor99038001 = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, meltConditionType.PK, tariff99038001.PK, "", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusApplicability(meltConditionFor99038001, allTradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueTypeForEntry.PK, meltConditionFor99038001.PK, "~06");
			var rate99038001 = helper.CreateRate(tariff99038001, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusApplicability(rate99038001, allTradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateTariffRelationship(tariff99038001.PK, tariffType.PK, "7201");

			var tariff7301101000 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "7301101000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariff99038504 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "99038504", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariff99038504);
			helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values._232, tariff99038504);
			var smeltConditionFor99038504 = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, smeltConditionType.PK, tariff99038504.PK, "", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusApplicability(smeltConditionFor99038504, allTradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueTypeForDesig.PK, smeltConditionFor99038504.PK, TariffConditionValue.Values.Optional);
			var rate99038504 = helper.CreateRate(tariff99038504, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusApplicability(rate99038504, canadaTradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateTariffRelationship(tariff99038504.PK, tariffType.PK, "7301");

			var tariff7401101000 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "7401101000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariff99038189 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "99038189", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariff99038189);
			helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values._232, tariff99038189);
			var meltConditionFor99038189 = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, meltConditionType.PK, tariff99038189.PK, "", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var applicability = helper.CreateCusApplicability(meltConditionFor99038189, allTradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateExcludedTradeGroup(unitedStatesTradeGroup, applicability);
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueTypeForDesig.PK, meltConditionFor99038189.PK, TariffConditionValue.Values.Optional);
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueTypeForEntry.PK, meltConditionFor99038189.PK, "~06");
			var rate99038189 = helper.CreateRate(tariff99038189, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusApplicability(rate99038189, canadaTradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateTariffRelationship(tariff99038189.PK, tariffType.PK, "7401");

			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "7201101000";
			tariff1.UE_DateFrom = ZDateTime.MinSmallDateTimeValue;
			tariff1.UE_DateTo = ZDateTime.Today.AddDays(10);

			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "7301101000";
			tariff2.UE_DateFrom = ZDateTime.MinSmallDateTimeValue;
			tariff2.UE_DateTo = ZDateTime.Today.AddDays(10);

			var tariff3 = Factory.New<USCTariff>();
			tariff3.UE_Tariff = "7401101000";
			tariff3.UE_DateFrom = ZDateTime.MinSmallDateTimeValue;
			tariff3.UE_DateTo = ZDateTime.Today.AddDays(10);
			Factory.Save();

			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "7201101000";
			AssertEquals("Should not have any sup tariff in the list", 0, invoiceLine.ApplicableSupTariffs.Length);

			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Australia;
			AssertEquals("Should have 1 tariff in the list", 1, invoiceLine.ApplicableSupTariffs.Length);
			AssertEquals("Sup tariff 99038001 should be the default.", "99038001", invoiceLine.US_SupTariff);

			invoiceLine.JI_Tariff = "7301101000";
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Canada;
			AssertEquals("Should have 2 tariffs in the list.", 2, invoiceLine.ApplicableSupTariffs.Length);
			AssertEquals("Sup tariff list includes 99038504.", true, invoiceLine.ApplicableSupTariffs.FirstOrDefault(x => x.Tariff == "99038504") != null);

			var notApplicableTariff = invoiceLine.ApplicableSupTariffs.FirstOrDefault(x => x.Tariff == TariffViewAsCodeDescription.NotApplicableCode);
			AssertNotNull("Sup tariff list includes N/A.", notApplicableTariff);
			AssertEquals("N/A Description", TariffViewAsCodeDescription.NotApplicableDescriptionForSection232, ((ICodeDescription)notApplicableTariff).Description);
			AssertEquals("N/A is mandatory", true, notApplicableTariff.IsMandatory);

			invoiceLine.JI_Tariff = "7401101000";
			AssertEquals("Should have 2 tariffs in the list.", 2, invoiceLine.ApplicableSupTariffs.Length);
			AssertEquals("Sup tariff list includes 99038189.", true, invoiceLine.ApplicableSupTariffs.FirstOrDefault(x => x.Tariff == "99038189") != null);

			notApplicableTariff = invoiceLine.ApplicableSupTariffs.FirstOrDefault(x => x.Tariff == TariffViewAsCodeDescription.NotApplicableCode);
			AssertNotNull("Sup tariff list includes N/A.", notApplicableTariff);
			AssertEquals("N/A Description", TariffViewAsCodeDescription.NotApplicableDescriptionForSection232, ((ICodeDescription)notApplicableTariff).Description);
			AssertEquals("N/A is mandatory", true, notApplicableTariff.IsMandatory);

			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Australia;
			AssertEquals("Should not have any sup tariff in the list", 0, invoiceLine.ApplicableSupTariffs.Length);
		}

		public void TestApplicableSupTariffsForAluminumSmeltCase()
		{
			#region Setup Tariffs

			var helper = new UniversalReferenceTestDataHelper(Factory);

			var allTradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.UnitedStates, "All Countries", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.AddCountry(allTradeGroup, Core.Constants.CountryCodes.HongKong, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			helper.AddCountry(allTradeGroup, Core.Constants.CountryCodes.Russia, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			var russiaTradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.Russia, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.AddCountry(russiaTradeGroup, Core.Constants.CountryCodes.Russia, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			var mexicoTradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.Mexico, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.AddCountry(mexicoTradeGroup, Core.Constants.CountryCodes.Mexico, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			var chinaTradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.China, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.AddCountry(chinaTradeGroup, Core.Constants.CountryCodes.China, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			var iranTradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.Iran, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.AddCountry(iranTradeGroup, Core.Constants.CountryCodes.Iran, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "HSN");
			var conditionType = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Rate, UniversalReferenceConstants.TariffConditionTypes.Codes.SMELT);
			var conditionValueType = helper.CreateOrGetExistingRefCusConditionValueType(Core.Constants.CountryCodes.UnitedStates, UniversalReferenceConstants.TariffConditionValueTypes.Codes.Entry);

			var dutyRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.RateTypes.Duty);
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, Universal.Constants.RateTypes.Duty, dutyRateType.PK);

			Factory.Save();

			var tariff7601 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "7601103000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, conditionType.PK, tariff7601.PK, "", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			var tariff99038501 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "99038501", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariff99038501);
			helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values._232, tariff99038501);
			helper.CreateTariffRelationship(tariff99038501.PK, tariffType.PK, "7601");
			var rate99038501 = helper.CreateRate(tariff99038501, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusApplicability(rate99038501, allTradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			var tariff99038567 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "99038567", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariff99038567);
			helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values._232, tariff99038567);
			helper.CreateTariffRelationship(tariff99038567.PK, tariffType.PK, "7601");
			var rate99038567 = helper.CreateRate(tariff99038567, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusApplicability(rate99038567, russiaTradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var condition99038567 = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, conditionType.PK, tariff99038567.PK, "", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueType.PK, condition99038567.PK, "~06");
			helper.CreateCusApplicability(condition99038567, russiaTradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			var tariff99038569 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "99038569", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariff99038569);
			helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values._232, tariff99038569);
			helper.CreateTariffRelationship(tariff99038569.PK, tariffType.PK, "7601");
			var rate99038569 = helper.CreateRate(tariff99038569, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusApplicability(rate99038569, russiaTradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var condition99038569 = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, conditionType.PK, tariff99038569.PK, "", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueType.PK, condition99038569.PK, "06");
			helper.CreateCusApplicability(condition99038569, russiaTradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			var tariff99038571 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "99038571", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariff99038571);
			helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values._232, tariff99038571);
			helper.CreateTariffRelationship(tariff99038571.PK, tariffType.PK, "7601");
			var rate99038571 = helper.CreateRate(tariff99038571, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusApplicability(rate99038571, mexicoTradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var condition99038571 = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, conditionType.PK, tariff99038571.PK, "", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusApplicability(condition99038571, chinaTradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusApplicability(condition99038571, iranTradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			Factory.Save();

			#endregion

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
				AssertEquals("There should be NO sup tariff defaulted, because COO is not entered yet.", ZString.Empty, invoiceLine.US_SupTariff);

				invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.HongKong;
				AssertEquals("Sup tariff 99038501 should be the default, because COO is HK", "99038501", invoiceLine.US_SupTariff);

				invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Russia;
				AssertEquals("Sup tariff 99038567 should be the default, because COO is RU", "99038567", invoiceLine.US_SupTariff);

				invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.HongKong;
				invoiceLine.US_RN_NKPrimCtry = Core.Constants.CountryCodes.Russia;
				AssertEquals("Should not change US_SupTariff when US_RN_NKPrimCtry is changed", "99038501", invoiceLine.US_SupTariff);

				declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
				AssertEquals("Sup tariff 99038567 should be the default, because entry type changed to '06' but zone status is NOT 'P'.", "99038567", invoiceLine.US_SupTariff);

				invoiceLine.US_ZoneStatus = ZoneStatusList.Codes.PrivilegedForeign;
				AssertEquals("Sup tariff 99038569 should be the default, because entry type changed to '06' and zone status is 'P'.", "99038569", invoiceLine.US_SupTariff);

				invoiceLine.US_RN_NKPrimCtry = ZString.Empty;
				AssertEquals("Should not change US_SupTariff when US_RN_NKPrimCtry is changed.", "99038569", invoiceLine.US_SupTariff);

				invoiceLine.US_RN_NKCastCtry = Core.Constants.CountryCodes.HongKong;
				AssertEquals("Should not change US_SupTariff when US_RN_NKCastCtry is changed.", "99038569", invoiceLine.US_SupTariff);

				invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Mexico;
				invoiceLine.US_RN_NKPrimCtry = ZString.Empty;
				invoiceLine.US_RN_NKSecCtry = ZString.Empty;
				invoiceLine.US_RN_NKCastCtry = ZString.Empty;
				AssertEquals("Should be only 1 sup tariff in the list.", 1, invoiceLine.ApplicableSupTariffs.Length);
				AssertEquals("Sup tariff 99038571 should be the default.", "99038571", invoiceLine.US_SupTariff);

				invoiceLine.US_RN_NKPrimCtry = Core.Constants.CountryCodes.China;
				AssertEquals("China meets the SMETL conditions, so the tariff will not be removed.", 1, invoiceLine.ApplicableSupTariffs.Length);
				AssertEquals("Should not change US_SupTariff when US_RN_NKPrimCtry is changed.", "99038571", invoiceLine.US_SupTariff);

				invoiceLine.US_RN_NKPrimCtry = Core.Constants.CountryCodes.Iran;
				AssertEquals("Iran meets the SMELT conditions, so the tariff will not be removed.", 1, invoiceLine.ApplicableSupTariffs.Length);
				AssertEquals("Sup tariff 99038571 should be the default.", "99038571", invoiceLine.US_SupTariff);

				invoiceLine.US_RN_NKPrimCtry = Core.Constants.CountryCodes.HongKong;
				AssertEquals("Although HongKong does not meet the SMELT conditions, the tariff will not be removed", 1, invoiceLine.ApplicableSupTariffs.Length);
				AssertEquals("Should not change US_SupTariff when US_RN_NKPrimCtry is changed.", "99038571", invoiceLine.US_SupTariff);

				invoiceLine.US_RN_NKPrimCtry = ZString.Empty;
				invoiceLine.US_RN_NKSecCtry = Core.Constants.CountryCodes.China;
				AssertEquals("China meets the SMETL conditions, so the tariff will not be removed.", 1, invoiceLine.ApplicableSupTariffs.Length);
				AssertEquals("Should not change US_RN_NKSecCtry when US_RN_NKPrimCtry is changed.", "99038571", invoiceLine.US_SupTariff);

				invoiceLine.US_RN_NKSecCtry = Core.Constants.CountryCodes.Iran;
				AssertEquals("Iran meets the SMELT conditions, so the tariff will not be removed.", 1, invoiceLine.ApplicableSupTariffs.Length);
				AssertEquals("Should not change US_RN_NKSecCtry when US_RN_NKPrimCtry is changed.", "99038571", invoiceLine.US_SupTariff);

				invoiceLine.US_RN_NKSecCtry = Core.Constants.CountryCodes.HongKong;
				AssertEquals("Although HongKong does not meet the SMELT conditions, the tariff will not be removed.", 1, invoiceLine.ApplicableSupTariffs.Length);
				AssertEquals("Should not change US_RN_NKSecCtry when US_RN_NKPrimCtry is changed.", "99038571", invoiceLine.US_SupTariff);

				invoiceLine.US_RN_NKSecCtry = ZString.Empty;
				invoiceLine.US_RN_NKCastCtry = Core.Constants.CountryCodes.China;
				AssertEquals("China meets the SMETL conditions, so the tariff will not be removed.", 1, invoiceLine.ApplicableSupTariffs.Length);
				AssertEquals("Should not change US_RN_NKCastCtry when US_RN_NKPrimCtry is changed.", "99038571", invoiceLine.US_SupTariff);

				invoiceLine.US_RN_NKCastCtry = Core.Constants.CountryCodes.Iran;
				AssertEquals("Iran meets the SMELT conditions, so the tariff will not be removed.", 1, invoiceLine.ApplicableSupTariffs.Length);
				AssertEquals("Should not change US_RN_NKCastCtry when US_RN_NKPrimCtry is changed.", "99038571", invoiceLine.US_SupTariff);

				invoiceLine.US_RN_NKCastCtry = Core.Constants.CountryCodes.HongKong;
				AssertEquals("Although HongKong does not meet the SMELT conditions, the tariff will not be removed.", 1, invoiceLine.ApplicableSupTariffs.Length);
				AssertEquals("Should not change US_RN_NKCastCtry when US_RN_NKPrimCtry is changed.", "99038571", invoiceLine.US_SupTariff);
			}
		}

		public void TestApplicableSupTariffsForCertificateOfOrigin()
		{
			#region Setup Tariffs

			var helper = new UniversalReferenceTestDataHelper(Factory);

			var allTradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.UnitedStates, "All Countries", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.AddCountry(allTradeGroup, Core.Constants.CountryCodes.Ukraine, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			helper.AddCountry(allTradeGroup, Core.Constants.CountryCodes.Australia, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			var ukraineTradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.Ukraine, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.AddCountry(ukraineTradeGroup, Core.Constants.CountryCodes.Ukraine, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			var australiaTradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.Australia, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.AddCountry(australiaTradeGroup, Core.Constants.CountryCodes.Australia, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "HSN");
			var conditionType = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, UniversalReferenceConstants.TariffConditionTypes.Codes.CTORG);
			var dutyRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.RateTypes.Duty);
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, Universal.Constants.RateTypes.Duty, dutyRateType.PK);

			Factory.Save();

			var tariff7206100000 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "7206100000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariff99038183 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "99038183", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariff99038183);
			helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values._232, tariff99038183);
			var condition99038183 = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, conditionType.PK, tariff99038183.PK, "", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusApplicability(condition99038183, australiaTradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var rate99038183 = helper.CreateRate(tariff99038183, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusApplicability(rate99038183, ukraineTradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateTariffRelationship(tariff99038183.PK, tariffType.PK, "7206");

			Factory.Save();

			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "7206100000";
			AssertEquals("There should be NO sup tariff defaulted, because COO is not entered yet.", ZString.Empty, invoiceLine.US_SupTariff);
			AssertEquals("Should not have any sup tariff in the list", 0, invoiceLine.ApplicableSupTariffs.Length);

			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Australia;
			AssertEquals("There should be NO sup tariff defaulted, because COO is AU and no sup tariff applies.", ZString.Empty, invoiceLine.US_SupTariff);
			AssertEquals("Should not have any sup tariff in the list", 0, invoiceLine.ApplicableSupTariffs.Length);

			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Ukraine;
			AssertEquals("Sup tariff 99038183 should be the default, because COO is UA.", "99038183", invoiceLine.US_SupTariff);
			AssertEquals("Should be only 1 sup tariff in the list", 1, invoiceLine.ApplicableSupTariffs.Length);

			invoiceLine.US_SupTariff = ZString.Empty;
			invoiceLine.US_RN_NKCertOrigin = Core.Constants.CountryCodes.Australia;
			AssertEquals("Should not change US_SupTariff when US_RN_NKCertOrigin is changed", ZString.Empty, invoiceLine.US_SupTariff);
			AssertEquals("Should be only 1 sup tariff in the list", 1, invoiceLine.ApplicableSupTariffs.Length);

			invoiceLine.US_SupTariff = ZString.Empty;
			invoiceLine.US_RN_NKCertOrigin = Core.Constants.CountryCodes.Ukraine;
			AssertEquals("Should not change US_SupTariff when US_RN_NKCertOrigin is changed", ZString.Empty, invoiceLine.US_SupTariff);
			AssertEquals("Although Ukraine does not meet the CTORG conditions, the tariff will not be removed", 1, invoiceLine.ApplicableSupTariffs.Length);
		}

		public void TestApplicableSupTariffsForMeltedPouredCountry()
		{
			#region Setup Tariffs

			var helper = new UniversalReferenceTestDataHelper(Factory);

			var allTradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.UnitedStates, "All Countries", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.AddCountry(allTradeGroup, Core.Constants.CountryCodes.Ukraine, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			helper.AddCountry(allTradeGroup, Core.Constants.CountryCodes.Australia, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			var ukraineTradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.Ukraine, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.AddCountry(ukraineTradeGroup, Core.Constants.CountryCodes.Ukraine, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			var mexicoTradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.Mexico, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.AddCountry(mexicoTradeGroup, Core.Constants.CountryCodes.Mexico, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			var europeanTradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.UnitedStates, EconomicGroupList.Codes.EuropeanUnion, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.AddCountry(europeanTradeGroup, Core.Constants.CountryCodes.Germany, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "HSN");
			var ctorgConditionType = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, UniversalReferenceConstants.TariffConditionTypes.Codes.CTORG);
			var meltConditionType = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, UniversalReferenceConstants.TariffConditionTypes.Codes.MELT);
			var dutyRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.RateTypes.Duty);
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, Universal.Constants.RateTypes.Duty, dutyRateType.PK);

			Factory.Save();

			var tariff7206100000 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "7206100000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariff99038001 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "99038001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariff99038001);
			helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values._232, tariff99038001);
			var meltConditionFor99038001 = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, meltConditionType.PK, tariff99038001.PK, "", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusApplicability(meltConditionFor99038001, ukraineTradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var rate99038001 = helper.CreateRate(tariff99038001, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusApplicability(rate99038001, europeanTradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateTariffRelationship(tariff99038001.PK, tariffType.PK, "7206");

			var tariff99038182 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "99038182", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariff99038182);
			helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values._232, tariff99038182);
			var meltConditionFor99038182 = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, meltConditionType.PK, tariff99038182.PK, "", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusApplicability(meltConditionFor99038182, ukraineTradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var rate99038182 = helper.CreateRate(tariff99038182, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusApplicability(rate99038182, europeanTradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateTariffRelationship(tariff99038182.PK, tariffType.PK, "7206");

			var tariff99038185 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "99038185", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariff99038185);
			helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values._232, tariff99038185);
			var meltConditionFor99038185 = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, meltConditionType.PK, tariff99038185.PK, "", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var applicability = helper.CreateCusApplicability(meltConditionFor99038185, allTradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateExcludedTradeGroup(mexicoTradeGroup, applicability);
			var rate99038185 = helper.CreateRate(tariff99038185, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusApplicability(rate99038185, mexicoTradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateTariffRelationship(tariff99038185.PK, tariffType.PK, "7206");

			Factory.Save();

			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "7206100000";
			AssertEquals("There should be NO sup tariff defaulted, because COO is not entered yet.", ZString.Empty, invoiceLine.US_SupTariff);
			AssertEquals("Should not have any sup tariff in the list", 0, invoiceLine.ApplicableSupTariffs.Length);

			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Germany;
			AssertEquals("There should be NO sup tariff defaulted, because there're multiple sup tariffs applicable.", ZString.Empty, invoiceLine.US_SupTariff);
			AssertEquals("Should have 2 tariffs in the list", 2, invoiceLine.ApplicableSupTariffs.Length);

			invoiceLine.US_RN_NKMeltCtry = Core.Constants.CountryCodes.Germany;
			AssertEquals("Although Germany does not meet the MELT conditions, the tariff will not be removed.", 2, invoiceLine.ApplicableSupTariffs.Length);
			AssertEquals("Should not change US_SupTariff when US_RN_NKMeltCtry is changed.", ZString.Empty, invoiceLine.US_SupTariff);

			invoiceLine.US_RN_NKMeltCtry = Core.Constants.CountryCodes.Ukraine;
			AssertEquals("Should not change US_SupTariff when US_RN_NKMeltCtry is changed.", ZString.Empty, invoiceLine.US_SupTariff);
			AssertEquals("Should have 2 tariffs in the list", 2, invoiceLine.ApplicableSupTariffs.Length);

			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Mexico;
			invoiceLine.US_RN_NKMeltCtry = ZString.Empty;
			AssertEquals("Should be only 1 sup tariff in the list.", 1, invoiceLine.ApplicableSupTariffs.Length);
			AssertEquals("Sup tariff 99038185 should be the default when US_UC_NKCountryOfOrigin is set.", "99038185", invoiceLine.US_SupTariff);

			invoiceLine.US_RN_NKMeltCtry = Core.Constants.CountryCodes.Mexico;
			AssertEquals("Although Mexico does not meet the MELT conditions, the tariff will not be removed.", 1, invoiceLine.ApplicableSupTariffs.Length);
			AssertEquals("Should not change US_SupTariff when US_RN_NKMeltCtry is changed.", "99038185", invoiceLine.US_SupTariff);
		}

		public void TestTaxQty_ReadOnlyAndTaxRate_ReadOnly()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.US_EntryFilerCode = "XJ5";

			var entry = reconDeclaration.OriginalEntries.AddNew();
			entry.US_R_DutyRateDate = ZDateTime.Today;
			entry.US_R_CalcOrigDuty = true;
			entry.Invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = entry.Invoice.InvoiceLines.AddNew();
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine.US_TaxRateS = AppendixBTaxRateList.Codes.Specify;
			Assert("Tax Rate Value should not be readonly", !invoiceLine.US_TaxRateInfo.ReadOnly);
			Assert("Tax Qty should be readonly", invoiceLine.US_TaxQtyInfo.ReadOnly);
			invoiceLine.US_TaxRateS = AppendixBTaxRateList.Codes.Other_1;
			Assert("Tax Rate Value should be readonly", invoiceLine.US_TaxRateInfo.ReadOnly);
			Assert("Tax Qty should not be readonly", !invoiceLine.US_TaxQtyInfo.ReadOnly);
		}

		public void TestOGAAgencyRequirementIndShouldBeSet_WhenIsCopyingIsTrue()
		{
			InvoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Assert(!((IBusinessObjectInternals)InvoiceLine).IsCopying);
			AssertNotEquals("The count of OGAAgencyRequirements should not equals to 0", 0, InvoiceLine.OGAAgencyRequirements.Count);
			foreach (var ogaAgencyRequirement in InvoiceLine.OGAAgencyRequirements.Cast<OGAAgencyRequirement>())
			{
				AssertEquals(ogaAgencyRequirement.AgencyCode + " indicator should be empty.", ZString.Empty, ogaAgencyRequirement.Indicator);
			}
			using (new CopyingOperation(InvoiceLine))
			{
				Assert(((IBusinessObjectInternals)InvoiceLine).IsCopying);
				foreach (var ogaAgencyRequirement in InvoiceLine.OGAAgencyRequirements.Cast<OGAAgencyRequirement>())
				{
					ogaAgencyRequirement.Indicator = OGAIndicatorList.Codes.Declared;
					AssertEquals(ogaAgencyRequirement.AgencyCode + " indicator should be D.", OGAIndicatorList.Codes.Declared, ogaAgencyRequirement.Indicator);
				}
			}
		}

		public void TestCustomsCountryCode()
		{
			AssertEquals("CustomsCountryCodeCore should be US", Core.Constants.CountryCodes.UnitedStates, Factory.New<JobComInvoiceLine>().CustomsCountryCode);
		}

		public void TestIInvoiceLinePartDetailsMembers()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				IInvoiceLinePartDetails partDetails = Factory.New<JobComInvoiceLine>();
				AssertEquals(Core.Constants.CountryCodes.UnitedStates, partDetails.CustomsCountryCode);
				AssertEquals(typeof(OrgSupplierPart), partDetails.TypeOfPartUsed);
			}
		}

		public void TestUSInvoiceLineSetupCorrectly()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			Factory.Save();

			var usComInvoiceLine = invoiceLine.AddInfoChild;
			AssertEquals(invoiceLine.PK, usComInvoiceLine.USI_JI);
			AssertEquals(true, usComInvoiceLine.IsInDatabase);
			AssertEquals(invoiceLine.JI_ClusterKey, usComInvoiceLine.USI_ClusterKey);

			var newFactory = new BusinessObjectFactory();
			declaration = newFactory.Load<JobDeclaration>(declaration.PK);
			invoiceLine = declaration.InvoiceLines[0];
			usComInvoiceLine = invoiceLine.AddInfoChild;
			AssertEquals(invoiceLine.PK, usComInvoiceLine.USI_JI);
			AssertEquals(true, usComInvoiceLine.IsInDatabase);
			AssertEquals(invoiceLine.JI_ClusterKey, usComInvoiceLine.USI_ClusterKey);

			declaration.Delete();
			AssertEquals(true, usComInvoiceLine.IsDeleted);

			declaration = Factory.New<JobDeclaration>();
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			Factory.Save();

			usComInvoiceLine = invoiceLine.AddInfoChild;
			AssertEquals(invoiceLine.PK, usComInvoiceLine.USI_JI);
			AssertEquals(invoiceLine.JI_ClusterKey, usComInvoiceLine.USI_ClusterKey);
		}

		public void TestDoNotLoadPGARelatedDataIfIsUncommitted()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = Factory.New<JobComInvoiceLineForTest>();
			invoiceLine.JI_JZ = invoice.PK;
			var properties = new (ZPropertyInfo info, IZType value)[]
			{
				(invoiceLine.US_ZoneStatusInfo, (ZString)"$"),
				(invoiceLine.US_UC_NKCountryOfOriginInfo, (ZString)"$"),
				(invoiceLine.US_UC_NKCountryOfExportInfo, (ZString)"$"),
				(invoiceLine.JI_OA_ManufacturerAddressInfo, ZGuid.BrettsGuid),
				(invoiceLine.US_TSCACertificationInfo, (ZString)"$"),
				(invoiceLine.US_TSCADisclaimReasonInfo, (ZString)"$"),
				(invoiceLine.US_FDAContactNameInfo, (ZString)"$"),
				(invoiceLine.US_FDAContactPhoneNoInfo, (ZString)"$"),
				(invoiceLine.US_FDAContactEmailInfo, (ZString)"$"),
				(invoiceLine.US_AMSDisclaimProgramInfo, (ZString)"$"),
				(invoiceLine.US_AMSDisclaimReasonInfo, (ZString)"$"),
				(invoiceLine.US_NOPDisclaimReasonInfo, (ZString)"$"),
				(invoiceLine.US_PSTDisclaimReasonInfo, (ZString)"$"),
				(invoiceLine.US_VNEDisclaimReasonInfo, (ZString)"$"),
				(invoiceLine.US_FSISDisclaimReasonInfo, (ZString)"$"),
				(invoiceLine.US_NMFS370DisclaimReasonInfo, (ZString)"$"),
				(invoiceLine.US_NMFSAMRDisclaimReasonInfo, (ZString)"$"),
				(invoiceLine.US_NMFSHMSDisclaimReasonInfo, (ZString)"$"),
				(invoiceLine.US_LaceyDisclaimReasonInfo, (ZString)"$"),
				(invoiceLine.JI_DescriptionInfo, (ZString)"$"),
				(invoiceLine.US_DDTCRegistrationNoInfo, (ZString)"$"),
				(invoiceLine.US_DDTCLicenseNoInfo, (ZString)"$"),
				(invoiceLine.US_DDTCLicenseTypeInfo, (ZString)"$"),
				(invoiceLine.US_DDTCExemptionCodeInfo, (ZString)"$"),
				(invoiceLine.US_DDTCArrivalDateInfo, ZDateTime.BrettsBirthday),
				(invoiceLine.US_SupTariffInfo, (ZString)"1"),
				(invoiceLine.JI_TariffInfo, (ZString)"1"),
				(invoiceLine.US_APHISDisclaimReasonInfo, (ZString)"$"),
				(invoiceLine.US_TTBDisclaimReasonInfo, (ZString)"$"),
				(invoiceLine.US_FWSDisclaimReasonInfo, (ZString)"$"),
				(invoiceLine.US_ODSDisclaimReasonInfo, (ZString)"$"),
				(invoiceLine.US_OMCDisclaimReasonInfo, (ZString)"$"),
				(invoiceLine.US_CPSCDisclaimReasonInfo, (ZString)"$"),
				(invoiceLine.US_DEADisclaimReasonInfo, (ZString)"$"),
				(invoiceLine.US_SetIndInfo, (ZString)"$"),
				(invoiceLine.JI_ParentIDInfo, ZGuid.BrettsGuid),
				(invoiceLine.JI_OA_ConsigneeAddressInfo, ZGuid.BrettsGuid),
				(invoiceLine.JI_OA_ExporterAddressInfo, ZGuid.BrettsGuid),
				(invoiceLine.US_FDADisclaimReasonInfo, (ZString)"$"),
				(invoiceLine.US_NHTDisclaimReasonInfo, (ZString)"$"),
				(invoiceLine.US_TSCAIndInfo, (ZString)"$"),
				(invoiceLine.US_AMSIndInfo, (ZString)"$"),
				(invoiceLine.US_NOPIndInfo, (ZString)"$"),
				(invoiceLine.US_DDTCIndInfo, (ZString)"$"),
				(invoiceLine.US_APHISIndInfo, (ZString)"$"),
				(invoiceLine.US_TTBIndInfo, (ZString)"$"),
				(invoiceLine.US_FWSIndInfo, (ZString)"$"),
				(invoiceLine.US_ATFIndInfo, (ZString)"$"),
				(invoiceLine.US_ODSIndInfo, (ZString)"$"),
				(invoiceLine.US_VNEIndInfo, (ZString)"$"),
				(invoiceLine.US_PSTIndicatorInfo, (ZString)"$"),
				(invoiceLine.US_NHTSAIndicatorInfo, (ZString)"$"),
				(invoiceLine.US_OMCIndInfo, (ZString)"$"),
				(invoiceLine.US_NMFS370IndInfo, (ZString)"$"),
				(invoiceLine.US_NMFSSIMPIndInfo, (ZString)"$"),
				(invoiceLine.US_NMFSAMRIndInfo, (ZString)"$"),
				(invoiceLine.US_NMFSHMSIndInfo, (ZString)"$"),
				(invoiceLine.US_CPSCIndInfo, (ZString)"$"),
				(invoiceLine.US_DEAIndInfo, (ZString)"$"),
				(invoiceLine.US_LaceyIndicatorInfo, (ZString)"$"),
				(invoiceLine.US_FDAIndicatorInfo, (ZString)"$"),
				(invoiceLine.US_HFCIndInfo, (ZString)"$"),
				(invoiceLine.US_HFCDisclaimReasonInfo, (ZString)"$"),
			};

			CombineAssertions(() =>
			{
				invoiceLine.IsUnCommittedRow_Mock = true;
				foreach (var property in properties)
				{
					var info = property.info;
					info.Value = property.value;
					AssertEquals($"Setting {info.Name} should not trigger PGA Tracker", 0, invoiceLine.TrackerCount);
					info.Value = info.DefaultValue;
					AssertEquals($"Clearing {info.Name} should not trigger PGA Tracker", 0, invoiceLine.TrackerCount);
				}

				invoiceLine.IsUnCommittedRow_Mock = false;
				foreach (var property in properties)
				{
					invoiceLine.TrackerCount = 0;
					var info = property.info;
					info.Value = property.value;
					AssertEquals($"Setting {info.Name} should trigger PGA Tracker", 1, invoiceLine.TrackerCount);
					info.Value = info.DefaultValue;
					AssertEquals($"Clearing {info.Name} should trigger PGA Tracker", 2, invoiceLine.TrackerCount);
				}
			});
		}

		public void TestTariffHasGAEType()
		{
			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;
			var uscTariff = Factory.New<USCTariff>();
			uscTariff.UE_Tariff = "3333333333";
			uscTariff.UE_DateFrom = startDate;
			uscTariff.UE_DateTo = endDate;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariff7217 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "3333333333", startDate, endDate);
			var tariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "5555555555", startDate, endDate);
			var attribute1 = helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values.GAE, tariff7217);
			var attribute2 = helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values._301, tariff2);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.HongKong;
			invoiceLine.JI_Tariff = tariff7217.ZZ1_TariffCode;
			AssertEquals("GAE type attribute", true, invoiceLine.IsTariffWithGAEType);
			invoiceLine.JI_Tariff = tariff2.ZZ1_TariffCode;
			AssertEquals("No GAE Attribute", false, invoiceLine.IsTariffWithGAEType);
		}

		public void TestSubTariffHasGAEType()
		{
			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;
			var uscTariff = Factory.New<USCTariff>();
			uscTariff.UE_Tariff = "3333333333";
			uscTariff.UE_DateFrom = startDate;
			uscTariff.UE_DateTo = endDate;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariff301 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "5555555555", startDate, endDate);
			var tariffGAE = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "7777777777", startDate, endDate);
			var attribute5 = helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values._301, tariff301);
			var attribute7 = helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values.GAE, tariffGAE);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.HongKong;

			invoiceLine.US_SupTariff = tariffGAE.ZZ1_TariffCode;
			AssertEquals("GAE type attribute", true, invoiceLine.IsSupTariffWithGAEType);
			invoiceLine.US_SupTariff = tariff301.ZZ1_TariffCode;
			AssertEquals("No GAE attribute", false, invoiceLine.IsSupTariffWithGAEType);
		}

		public void TestApplicableSupTariffsIsCorrectlyCached()
		{
			var startDate = ZDateTime.Today.AddMonths(-5);
			var endDate = ZDateTime.MaxSmallDateTimeValue;
			var uscTariff = Factory.New<USCTariff>();
			uscTariff.UE_Tariff = "3333333333";
			uscTariff.UE_DateFrom = startDate;
			uscTariff.UE_DateTo = endDate;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariff7217 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "3333333333", startDate, endDate);
			var tariff99With301 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "5555555555", startDate, endDate);
			var tariff99With232 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "7777777777", startDate, endDate);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values.GAE, tariff7217);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariff99With301);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values._301, tariff99With301);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariff99With232);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values._232, tariff99With232);
			helper.CreateTariffRelationship(tariff99With301.PK, tariffType.PK, tariff7217.ZZ1_TariffCode);
			helper.CreateTariffRelationship(tariff99With232.PK, tariffType.PK, tariff7217.ZZ1_TariffCode);

			var dutyRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, Universal.Constants.RateTypes.Duty, dutyRateType.PK);
			var rate3 = helper.CreateRate(tariff99With301, rateCode.PK, startDate, endDate, "0");
			var rate4 = helper.CreateRate(tariff99With232, rateCode.PK, startDate, endDate, "0");

			var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.HongKong, Core.Constants.CountryCodes.HongKong, startDate, endDate);
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.HongKong, startDate.Date, endDate.Date);
			helper.CreateCusApplicability(rate3, tradeGroup, startDate, endDate);
			helper.CreateCusApplicability(rate4, tradeGroup, startDate, endDate);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.HongKong;
			invoiceLine.JI_Tariff = tariff7217.ZZ1_TariffCode;
			AssertEquals(tariff7217.ZZ1_TariffCode, invoiceLine.JI_Tariff);
			AssertEquals("DropDownList has 2 item", 2, invoiceLine.Lookups.SupTariffsList.Count);
			AssertEquals(tariff99With301.ZZ1_TariffCode, invoiceLine.US_SupTariff);

			AssertEquals(ZDate.Today, invoiceLine.EffectiveDateForDutyRate);
			AssertEquals(2, invoiceLine.ApplicableSupTariffs.Length);

			declaration.US_EstimatedEntryDate = ZDate.Today.AddYears(-1);
			AssertEquals(ZDate.Today.AddYears(-1), invoiceLine.EffectiveDateForDutyRate);
			AssertEquals(0, invoiceLine.ApplicableSupTariffs.Length);
		}

		public void TestSetDefaultSupTariffsWhenTariffHasGAEAttribute()
		{
			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;
			var uscTariff = Factory.New<USCTariff>();
			uscTariff.UE_Tariff = "3333333333";
			uscTariff.UE_DateFrom = startDate;
			uscTariff.UE_DateTo = endDate;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType("US", "HSN");
			var tariff1 = helper.LoadOrCreateNewTariff("US", tariffType.PK, "3333333333", startDate, endDate);
			var tariff3 = helper.LoadOrCreateNewTariff("US", tariffType.PK, "5555555555", startDate, endDate);
			var attribute1 = helper.CreateTariffAttribute("TYPE", "GAE", tariff1);
			var attribute4 = helper.CreateTariffAttribute("RULE", "A99", tariff3);
			var relationship2 = helper.CreateTariffRelationship(tariff3.PK, tariffType.PK, tariff1.ZZ1_TariffCode);

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
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.HongKong;
			AssertEquals("", invoiceLine.JI_Tariff);
			AssertEquals("", invoiceLine.US_SupTariff);

			invoiceLine.JI_Tariff = tariff1.ZZ1_TariffCode;
			AssertEquals(tariff1.ZZ1_TariffCode, invoiceLine.JI_Tariff);
			AssertEquals("", invoiceLine.US_SupTariff);
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
			AssertEquals(tariff1.ZZ1_TariffCode, newLine.JI_Tariff);
			AssertEquals(tariff2.ZZ1_TariffCode, newLine.US_SupTariff);
		}

		public void TestSetDefaultSupTariffsWithAttributeR99()
		{
			var startDate = ZDateTime.Now.AddMonths(-1);
			var endDate = ZDateTime.Now.AddMonths(1);

			var helper = new UniversalReferenceTestDataHelper(Factory);

			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates);
			var tariffType = helper.CreateNewOrGetExistingTariffType(dataGrouping.ZZZ_DataGrouping, Enterprise.Customs.Universal.Constants.TariffTypes.HarmonizedSystem);
			var tradeGroup = helper.LoadOrCreateTradeGroup(dataGrouping.ZZZ_DataGrouping, Core.Constants.CountryCodes.Russia);
			var tradeGroupCountry = helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Russia, startDate.Date, endDate.Date);
			var rateType = helper.CreateNewOrGetExistingRateType(dataGrouping.ZZZ_DataGrouping, Universal.Constants.RateTypes.Duty);
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RateCodes.Codes.Duty, rateType.PK);

			var parent01 = helper.LoadOrCreateNewTariff(dataGrouping.ZZZ_DataGrouping, tariffType.PK, "1111111111", startDate, endDate);
			var parent02 = helper.LoadOrCreateNewTariff(dataGrouping.ZZZ_DataGrouping, tariffType.PK, "2222222222", startDate, endDate);
			var parent03 = helper.LoadOrCreateNewTariff(dataGrouping.ZZZ_DataGrouping, tariffType.PK, "3333333333", startDate, endDate);

			var child01 = helper.LoadOrCreateNewTariff(dataGrouping.ZZZ_DataGrouping, tariffType.PK, "4444444444", startDate, endDate);
			var child02 = helper.LoadOrCreateNewTariff(dataGrouping.ZZZ_DataGrouping, tariffType.PK, "5555555555", startDate, endDate);
			var child03 = helper.LoadOrCreateNewTariff(dataGrouping.ZZZ_DataGrouping, tariffType.PK, "6666666666", startDate, endDate);

			var attribute11 = helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.RussianTariffs, child01);
			var attribute12 = helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values.BabyFomula, child01);
			var attribute21 = helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.RussianTariffs, child02);
			var attribute32 = helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values.BabyFomula, child03);

			var rate01 = helper.CreateRefCusRate(child01.PK, rateCode.PK, startDate, endDate);
			var rate02 = helper.CreateRefCusRate(child02.PK, rateCode.PK, startDate, endDate);
			var rate03 = helper.CreateRefCusRate(child03.PK, rateCode.PK, startDate, endDate);

			var applicability01 = helper.CreateCusApplicability(rate01.PK, tradeGroup, startDate, endDate);
			var applicability02 = helper.CreateCusApplicability(rate02.PK, tradeGroup, startDate, endDate);
			var applicability03 = helper.CreateCusApplicability(rate03.PK, tradeGroup, startDate, endDate);

			var relation11 = helper.CreateTariffRelationship(child01.PK, tariffType.PK, parent01.ZZ1_TariffCode);
			var relation12 = helper.CreateTariffRelationship(child01.PK, tariffType.PK, parent02.ZZ1_TariffCode);
			var relation22 = helper.CreateTariffRelationship(child02.PK, tariffType.PK, parent02.ZZ1_TariffCode);
			var relation23 = helper.CreateTariffRelationship(child02.PK, tariffType.PK, parent03.ZZ1_TariffCode);
			var relation33 = helper.CreateTariffRelationship(child03.PK, tariffType.PK, parent03.ZZ1_TariffCode);
			var relation31 = helper.CreateTariffRelationship(child03.PK, tariffType.PK, parent01.ZZ1_TariffCode);

			Factory.Save();

			var uscTariff01 = Factory.New<USCTariff>();
			uscTariff01.UE_Tariff = parent01.ZZ1_TariffCode;
			uscTariff01.UE_DateFrom = parent01.ZZ1_StartDate;
			uscTariff01.UE_DateTo = parent01.ZZ1_EndDate;

			var uscTariff02 = Factory.New<USCTariff>();
			uscTariff02.UE_Tariff = parent02.ZZ1_TariffCode;
			uscTariff02.UE_DateFrom = parent02.ZZ1_StartDate;
			uscTariff02.UE_DateTo = parent02.ZZ1_EndDate;

			var uscTariff03 = Factory.New<USCTariff>();
			uscTariff03.UE_Tariff = parent03.ZZ1_TariffCode;
			uscTariff03.UE_DateFrom = parent03.ZZ1_StartDate;
			uscTariff03.UE_DateTo = parent03.ZZ1_EndDate;

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Russia;
			invoiceLine.JI_Tariff = "1111111111";
			var supTariffs = invoiceLine.ApplicableSupTariffs;
			AssertEquals(2, supTariffs.Length);
			AssertEquals("Sup tariff list includes N/A.", true, supTariffs.FirstOrDefault(x => x.Tariff == TariffViewAsCodeDescription.NotApplicableCode) != null);
			AssertEquals("Do not default sup tariff.", ZString.Empty, invoiceLine.US_SupTariff);

			invoiceLine.JI_Tariff = "2222222222";
			supTariffs = invoiceLine.ApplicableSupTariffs;
			AssertEquals(3, supTariffs.Length);
			AssertEquals("Sup tariff list includes N/A.", true, supTariffs.FirstOrDefault(x => x.Tariff == TariffViewAsCodeDescription.NotApplicableCode) != null);
			AssertEquals("Do not default sup tariff.", ZString.Empty, invoiceLine.US_SupTariff);

			invoiceLine.JI_Tariff = "3333333333";
			supTariffs = invoiceLine.ApplicableSupTariffs;
			AssertEquals(1, supTariffs.Length);
			AssertEquals("Sup tariff list does not include N/A.", false, supTariffs.FirstOrDefault(x => x.Tariff == TariffViewAsCodeDescription.NotApplicableCode) != null);
			AssertEquals("Default sup tariff.", "5555555555", invoiceLine.US_SupTariff);
		}

		public void TestCheckUS_SupTariffWhenGAEAndOtherAttribute()
		{
			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;
			var uscTariff = Factory.New<USCTariff>();
			uscTariff.UE_Tariff = "3333333333";
			uscTariff.UE_DateFrom = startDate;
			uscTariff.UE_DateTo = endDate;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariff7217 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "3333333333", startDate, endDate);
			var tariff99With301 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "5555555555", startDate, endDate);
			var tariff99With232 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "7777777777", startDate, endDate);
			var attribute1 = helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values.GAE, tariff7217);
			var attribute4 = helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariff99With301);
			var attribute5 = helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values._301, tariff99With301);
			var attribute6 = helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariff99With232);
			var attribute7 = helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values._232, tariff99With232);
			var relationship2 = helper.CreateTariffRelationship(tariff99With301.PK, tariffType.PK, tariff7217.ZZ1_TariffCode);
			var relationship3 = helper.CreateTariffRelationship(tariff99With232.PK, tariffType.PK, tariff7217.ZZ1_TariffCode);

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
			invoiceLine.JI_Tariff = tariff7217.ZZ1_TariffCode;
			AssertEquals(tariff7217.ZZ1_TariffCode, invoiceLine.JI_Tariff);
			AssertEquals("DropDownList has 2 item", 2, invoiceLine.Lookups.SupTariffsList.Count);
			AssertEquals(tariff99With301.ZZ1_TariffCode, invoiceLine.US_SupTariff);
		}

		public void TestISetterSuspenderSupporter_SupportedFields()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			ISetterSuspenderSupporter supporter = invoiceLine;
			var supportedFields = supporter.SupportedFields.ToArray();
			AssertArrayEqualsByElements(new[]
			{
				JobComInvoiceLine.Schema.JI_Tariff,
				JobComInvoiceLine.Schema.JI_InvoiceUQ,
				JobComInvoiceLine.Schema.JI_CustomsQuantity,
				JobComInvoiceLine.Schema.JI_CustomsUnitQty,
				JobComInvoiceLine.Schema.JI_CustomsSecondQuantity,
				JobComInvoiceLine.Schema.JI_CustomsSecondUnitQty,
				JobComInvoiceLine.Schema.JI_CustomsThirdQuantity,
				JobComInvoiceLine.Schema.JI_CustomsThirdUnitQty,
				JobComInvoiceLine.Schema.JI_CustomsFourthQuantity,
				JobComInvoiceLine.Schema.JI_CustomsFourthUnitQty,
				AutoUSAddInfo.Schema.US_SupTariff,
				AutoUSAddInfo.Schema.US_DRWAdValoremRate,
				AutoUSAddInfo.Schema.US_DRWWeightedRatio,
				AutoUSAddInfo.Schema.US_DRWMPFWeightedRatio,
				AutoUSAddInfo.Schema.US_DRWLineDuty,
				AutoUSAddInfo.Schema.US_DRWDeclaredTax,
				AutoUSAddInfo.Schema.US_DRWDeclaredVFD,
				AutoUSAddInfo.Schema.US_DRWDeclaredHMF,
				AutoUSAddInfo.Schema.US_DRWDeclaredMPF,
				AutoUSAddInfo.Schema.US_DRWImportQuantity,
				AutoUSAddInfo.Schema.US_DRWImportUQ,
				AutoUSAddInfo.Schema.US_DRWExportQuantity,
				AutoUSAddInfo.Schema.US_DRWExportUQ,
				AutoUSAddInfo.Schema.US_DRWImportQuantity2,
				AutoUSAddInfo.Schema.US_DRWImportUQ2,
				AutoUSAddInfo.Schema.US_DRWImportQuantity3,
				AutoUSAddInfo.Schema.US_DRWImportUQ3,
				AutoUSAddInfo.Schema.US_DRWValuePerUQ,
				AutoUSAddInfo.Schema.US_DRWValuePerUQ2,
				AutoUSAddInfo.Schema.US_DRWValuePerUQ3,
				AutoUSAddInfo.Schema.US_DRWLineDutyRateDesc,
				AutoUSAddInfo.Schema.US_DRWCalcDutyWithAdValoremRate
			}, supportedFields);
		}

		public void TestTaxComputationCodeForCBMA()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.US_EstimatedEntryDate = new ZDateTime(2022, 01, 01);
			var invoiceLine = dec.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CustomsUnitQty = "D!";
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine.US_TaxCode = Core.Constants.USCustoms.FeeCodes.Tobacco;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.C;
			invoiceLine.US_TaxRateS = AppendixBTaxRateList.Codes.Tobacco_2;
			AssertEquals(ComputationCodeList.Codes.SpecificRateFirstQuantity, invoiceLine.TaxComputationCode);
			invoiceLine.US_TaxApply = TaxApplyList.Codes.No;
			invoiceLine.US_TaxRateS = AppendixBTaxRateList.Codes.Tobacco_2;
			AssertEquals(ComputationCodeList.Codes.AdValorem, invoiceLine.TaxComputationCode);
		}

		public void TestExportDateForLicenseType()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoiceLine = dec.Invoices.AddNew().InvoiceLines.AddNew();
			AssertEquals(ZDateTime.Today, invoiceLine.ExportDateForLicenseType);
			invoiceLine.US_DateOfExport = ZDateTime.Today.AddDays(-1);
			AssertEquals(ZDateTime.Today.AddDays(-1), invoiceLine.ExportDateForLicenseType);

			invoiceLine.US_DateOfExport = ZDateTime.Invalid;
			AssertEquals("Invalidat Date should now allow on US_DateOfExport in InvoiceLine", ZDateTime.Today, invoiceLine.ExportDateForLicenseType);
			dec.US_DateOfExport = ZDateTime.Today.AddDays(-5);
			AssertEquals("fallback to US_DateOfExport on JobDeclaration", dec.US_DateOfExport, invoiceLine.ExportDateForLicenseType);
		}

		public void TestFUNCEffective()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			Assert(!invoiceLine.IsAMSNOPEffective);
			Assert(!invoiceLine.IsAMSPNTEffective);
			Assert(!invoiceLine.IsAMSEGGEffective);
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AMSNOP, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AMSPNT, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AMSEGG, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				Assert(invoiceLine.IsAMSNOPEffective);
				Assert(invoiceLine.IsAMSPNTEffective);
				Assert(invoiceLine.IsAMSEGGEffective);
			}
		}

		public void TestDefaultNOPIndWhenIsAMSOR1Effective()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var amsTariff = Factory.New<USCTariff>();
			amsTariff.UE_Tariff = "3333333333";
			amsTariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			amsTariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			amsTariff.UE_PGACodes = "AMSNOPOR1AM8";
			amsTariff.UE_OGACodes = "AMSNOPOR1AM8";
			AssertEquals(ZString.Empty, invoiceLine.US_NOPInd);
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AMSNOP, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				invoiceLine.JI_Tariff = "3333333333";
				AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_NOPInd);
			}
		}

		public void TestJI_TariffSetterSuspending()
		{
			SetDataForSupTariffTest();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.HongKong;
			using (invoiceLine.SetterSuspender.SuspendSetting(BaseJobComInvoiceLine.Schema.JI_Tariff))
			{
				invoiceLine.JI_Tariff = "7301100000";
				AssertEquals("JI_Tariff", ZString.Empty, invoiceLine.JI_Tariff);
				AssertEquals("Zone Status", ZString.Empty, invoiceLine.US_ZoneStatus);
				using (invoiceLine.SetterSuspender.ResumeSetting(BaseJobComInvoiceLine.Schema.JI_Tariff))
				{
					invoiceLine.JI_Tariff = "7301100000";
					AssertEquals("JI_Tariff", "7301100000", invoiceLine.JI_Tariff);
					AssertEquals("Zone Status", ZoneStatusList.Codes.PrivilegedForeign, invoiceLine.US_ZoneStatus);
				}
			}
		}

		public void TestJI_TariffWhenSetThenCalculateExceptionCleared()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.TariffCalculateExceptionMessage = "Calculate Error";
			Assert("Calculate error exists", !invoiceLine.TariffCalculateExceptionMessage.IsEmpty);

			invoiceLine.JI_Tariff = "00000000";
			Assert("Calculate error cleared", invoiceLine.TariffCalculateExceptionMessage.IsEmpty);
		}

		public void TestSuspendUS_SupTariffSetter()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "98038801";
			using (invoiceLine.SetterSuspender.SuspendSetting(JobComInvoiceLine.Schema.US_SupTariff))
			{
				invoiceLine.US_SupTariff = "98038802";
				AssertEquals("98038801", invoiceLine.US_SupTariff);
			}

			invoiceLine.US_SupTariff = "98038803";
			AssertEquals("98038803", invoiceLine.US_SupTariff);
		}

		public void TestEffectiveDateForDutyUsingExportDate()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var expTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.Export);
			var shBTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.ScheduleB);
			Factory.Save();
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, expTariffType.PK, "0000000000", new ZDateTime(2009, 1, 1), new ZDateTime(2009, 6, 30));
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, shBTariffType.PK, "0000000001", new ZDateTime(2009, 1, 1), new ZDateTime(2009, 6, 30));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.US_TariffType = TariffTypeList.Codes.HTS;

			declaration.Invoices.AddNew();
			var todayAdd2 = ZDateTime.Today.AddDays(2);
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_DateOfExport = todayAdd2;
			invoiceLine.US_TariffType = TariffTypeList.Codes.ScheduleB;
			invoiceLine.JI_Tariff = "0000000001";

			AssertNull(invoiceLine.ScheduleBTariff);
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, "Tariff was found but is not valid for " + todayAdd2.ToShortDateString());

			invoiceLine.US_TariffType = TariffTypeList.Codes.HTS;
			invoiceLine.US_DateOfExport = todayAdd2;
			invoiceLine.JI_Tariff = "0000000000";

			AssertNull(invoiceLine.ExportTariff);
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, "Tariff was found but is not valid for " + todayAdd2.ToShortDateString());

			invoiceLine.US_DateOfExport = ZDateTime.Empty;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, "Tariff was found but is not valid for " + ZDateTime.Today.ToShortDateString());
		}

		public void TestJI_TariffReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(false, invoiceLine.JI_TariffInfo.ReadOnly);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			AssertEquals(true, invoiceLine.JI_TariffInfo.ReadOnly);
		}

		public void TestRefreshSupTariffWhenUS_PrivilegedStatusDateAndZoneStatusChanges()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "HSN");
			Factory.Save();

			var startDate1 = new ZDate(2017, 01, 01);
			var endDate1 = new ZDate(2019, 01, 01);
			var startDate2 = new ZDate(2018, 01, 01);
			var endDate2 = new ZDate(2020, 01, 01);

			var tariff4 = Factory.New<USCTariff>();
			tariff4.UE_Tariff = "84501100";
			tariff4.UE_DateFrom = startDate1;
			tariff4.UE_DateTo = endDate2;

			var progTariff1 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99038801", startDate1, endDate1);
			var progTariff2 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99038802", startDate2, endDate2);
			var dutyRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DTY", dutyRateType.PK);
			Factory.Save();
			var rate1 = helper.CreateRate(progTariff1, rateCode.PK, startDate1, endDate1, "0");
			var rate2 = helper.CreateRate(progTariff2, rateCode.PK, startDate2, endDate2, "0");
			var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.HongKong, "HK", startDate1, endDate2);
			var tradeGroupCountry = helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.HongKong, startDate1, endDate2);
			var applicability1 = helper.CreateCusApplicability(rate1, tradeGroup, startDate1, endDate1);
			var applicability2 = helper.CreateCusApplicability(rate2, tradeGroup, startDate2, endDate2);
			var relationship2 = helper.CreateTariffRelationship(progTariff1.PK, hsnTariffType.PK, "84501100");
			var relationship3 = helper.CreateTariffRelationship(progTariff2.PK, hsnTariffType.PK, "84501100");
			var tariffAttribute1 = helper.CreateTariffAttribute("RULE", "A99", progTariff1);
			var tariffAttribute2 = helper.CreateTariffAttribute("RULE", "A99", progTariff2);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLines = invoice.JobComInvoiceLines;
			var invoiceLine1 = invoiceLines.AddNew();
			invoiceLine1.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.HongKong;
			invoiceLine1.JI_Tariff = "84501100";
			invoiceLine1.US_PrivilegedStatusDate = new ZDate(2017, 01, 02);
			invoiceLine1.US_ZoneStatus = ZoneStatusList.Codes.PrivilegedForeign;
			AssertEquals("Will set to the only one applicable sup tariff", "99038801", invoiceLine1.US_SupTariff);
			AssertEquals(1, invoiceLine1.ApplicableSupTariffList.Count);

			invoiceLine1.US_PrivilegedStatusDate = new ZDate(2018, 01, 02);
			AssertEquals("Will not reset to empty when sup tariff exists in applicable sup tariffs", "99038801", invoiceLine1.US_SupTariff);
			AssertEquals(2, invoiceLine1.ApplicableSupTariffList.Count);

			invoiceLine1.US_PrivilegedStatusDate = new ZDate(2019, 01, 02);
			AssertEquals("Will set to the only one applicable sup tariff", "99038802", invoiceLine1.US_SupTariff);
			AssertEquals(1, invoiceLine1.ApplicableSupTariffList.Count);

			invoiceLine1.US_ZoneStatus = ZoneStatusList.Codes.Domestic;
			AssertEquals("Will not reset when no applicable sup tariff", "99038802", invoiceLine1.US_SupTariff);
			AssertEquals(0, invoiceLine1.ApplicableSupTariffList.Count);

			invoiceLine1.US_SupTariff = ZString.Empty;
			invoiceLine1.US_ZoneStatus = ZoneStatusList.Codes.PrivilegedForeign;
			invoiceLine1.US_PrivilegedStatusDate = new ZDate(2020, 01, 02);
			AssertEquals("Will not reset when no applicable sup tariff", ZString.Empty, invoiceLine1.US_SupTariff);
			AssertEquals(0, invoiceLine1.ApplicableSupTariffList.Count);

			invoiceLine1.US_SupTariff = "99038803";
			invoiceLine1.US_ZoneStatus = ZoneStatusList.Codes.PrivilegedForeign;
			invoiceLine1.US_PrivilegedStatusDate = new ZDate(2018, 01, 03);
			AssertEquals("Will reset to empty when sup tariff not exists in applicable sup tariff", ZString.Empty, invoiceLine1.US_SupTariff);
			AssertEquals(2, invoiceLine1.ApplicableSupTariffList.Count);
		}

		public void TestIsInformal()
		{
			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_MessageType = JobMessageTypeList.Codes.Export;
			dec1.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec2.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			var dec3 = Factory.New<JobDeclaration>();
			dec3.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec3.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;

			var invoice1 = dec1.Invoices.AddNew();
			var invoice2 = dec2.Invoices.AddNew();
			var invoice3 = dec3.Invoices.AddNew();
			var invoice4 = Factory.New<JobComInvoiceHeader>();

			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			var invoiceLine3 = invoice3.JobComInvoiceLines.AddNew();
			var invoiceLine4 = Factory.New<JobComInvoiceLine>();

			AssertEquals("invoiceLine1.IsInformal", false, invoiceLine1.IsInformal);
			AssertEquals("invoiceLine2.IsInformal", true, invoiceLine2.IsInformal);
			AssertEquals("invoiceLine3.IsInformal", false, invoiceLine3.IsInformal);
			AssertEquals("invoiceLine4.IsInformal", false, invoiceLine4.IsInformal);

			invoiceLine4.JI_JZ = invoice4.PK;
			AssertEquals("invoiceLine1.IsInformal", false, invoiceLine1.IsInformal);
			AssertEquals("invoiceLine2.IsInformal", true, invoiceLine2.IsInformal);
			AssertEquals("invoiceLine3.IsInformal", false, invoiceLine3.IsInformal);
			AssertEquals("invoiceLine4.IsInformal", false, invoiceLine4.IsInformal);

			invoice4.JZ_JE = dec2.PK;
			AssertEquals("invoiceLine1.IsInformal", false, invoiceLine1.IsInformal);
			AssertEquals("invoiceLine2.IsInformal", true, invoiceLine2.IsInformal);
			AssertEquals("invoiceLine3.IsInformal", false, invoiceLine3.IsInformal);
			AssertEquals("invoiceLine4.IsInformal", true, invoiceLine4.IsInformal);

			invoiceLine4.JI_JZ = invoice3.PK;
			AssertEquals("invoiceLine1.IsInformal", false, invoiceLine1.IsInformal);
			AssertEquals("invoiceLine2.IsInformal", true, invoiceLine2.IsInformal);
			AssertEquals("invoiceLine3.IsInformal", false, invoiceLine3.IsInformal);
			AssertEquals("invoiceLine4.IsInformal", false, invoiceLine4.IsInformal);

			dec3.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			AssertEquals("invoiceLine1.IsInformal", false, invoiceLine1.IsInformal);
			AssertEquals("invoiceLine2.IsInformal", true, invoiceLine2.IsInformal);
			AssertEquals("invoiceLine3.IsInformal", true, invoiceLine3.IsInformal);
			AssertEquals("invoiceLine4.IsInformal", true, invoiceLine4.IsInformal);
		}

		public void TestIsChildVLineAndIsParentVLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoice = declaration.Invoices.AddNew();
			var parent = declaration.InvoiceLines.AddNew();
			parent.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;

			var firstV = parent.AddSecondaryInvoiceLine();
			firstV.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			Assert("Only one V Line not a v child", !firstV.IsVChildLine);
			Assert("Only one V Line not a v parent", !firstV.IsVParentLine);

			var secondV = firstV.AddSecondaryInvoiceLine();
			secondV.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;

			Assert(secondV.IsVChildLine);
			Assert(firstV.IsVParentLine);
		}

		public void TestUS_LumberExportPricePrecision()
		{
			InvoiceLine.US_LumberExportPrice = new ZDecimal(99999999.99);
			AssertNoNotifications(InvoiceLine.US_LumberExportPriceInfo);
			InvoiceLine.US_LumberExportPrice = new ZDecimal(100000000.00);
			AssertHasErrorContaining(InvoiceLine.US_LumberExportPriceInfo, "The number 100,000,000 is too large, the maximum value allowed for selection is 99,999,999.99.");
		}

		public void TestUS_LumberExportChargesPrecision()
		{
			InvoiceLine.US_LumberExportCharges = new ZDecimal(99999999.99);
			AssertNoNotifications(InvoiceLine.US_LumberExportChargesInfo);
			InvoiceLine.US_LumberExportCharges = new ZDecimal(100000000.00);
			AssertHasErrorContaining(InvoiceLine.US_LumberExportChargesInfo, "The number 100,000,000 is too large, the maximum value allowed for selection is 99,999,999.99.");
		}

		public void TestManufacturerOrgPK_ReadOnly()
		{
			AssertNull(InvoiceLine.Manufacturer);
			AssertEquals(false, InvoiceLine.ManufacturerOrgPKInfo.ReadOnly);
			AssertEquals(false, InvoiceLine.JI_OA_ManufacturerAddressInfo.ReadOnly);

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ORG";
			org.OH_FullName = "Org Name";
			org.MainAddress.OA_Address1 = "Address1";
			InvoiceLine.JI_OA_ManufacturerAddress = org.MainAddress.PK;
			AssertEquals("When the manufacturer is not in the DB, it's readonly", true, InvoiceLine.ManufacturerOrgPKInfo.ReadOnly);
			AssertEquals("When the manufacturer is not in the DB, it's readonly", true, InvoiceLine.JI_OA_ManufacturerAddressInfo.ReadOnly);

			Factory.Save();
			AssertEquals(false, InvoiceLine.ManufacturerOrgPKInfo.ReadOnly);
			AssertEquals(false, InvoiceLine.JI_OA_ManufacturerAddressInfo.ReadOnly);
		}

		public void TestToMakeSurePGALineCloningGetValueCorrectlySet()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Declaration.US_EnableENS = true;
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_DDTCTrackingStatus = PGATrackingStatusList.Codes.Added;
			invoiceLine.US_ODSTrackingStatus = PGATrackingStatusList.Codes.Added;
			invoiceLine.US_TSCATrackingStatus = PGATrackingStatusList.Codes.Added;
			invoiceLine.SetTrackingID();
			invoiceLine.FeeCusCodes.AddNew();
			invoiceLine.FeeCusCodes[0].CY_Code = "CC";

			var fda = invoiceLine.FDAs.AddNew();
			fda.US_FDAQty1 = 5m;
			fda.US_FDAValue = 5m;
			fda.US_FDAConfirmDate = ZDateTime.Today;

			var fdaCloned = fda.Clone();
			AssertEquals("", fdaCloned.B7_ParentTableCode);
			AssertEquals(ZGuid.Empty, fdaCloned.B7_ParentID);
			invoiceLine.FDAs.RemoveAndDeleteAll();
			invoiceLine.FDAs.Add(fdaCloned);
			AssertEquals("JI", fdaCloned.B7_ParentTableCode);
			AssertEquals(invoiceLine.PK, fdaCloned.B7_ParentID);
			BusinessObjectFactory factory1 = null;
			var clonedNull = invoiceLine.Clone(new BusinessObjectCloneArgs(factory1, new string[] { JobComInvoiceLine.Schema.JI_JZ }, typeof(JobComInvoiceLine), false));
			AssertEquals("JI", clonedNull.FDAs[0].B7_ParentTableCode);
			AssertEquals(clonedNull.PK, clonedNull.FDAs[0].B7_ParentID);
			AssertEquals(Factory.GetHashCode(), clonedNull.Factory.GetHashCode());
			AssertEquals(Factory.GetHashCode(), clonedNull.FDAs[0].Factory.GetHashCode());

			var factory2 = new BusinessObjectFactory();
			var cloned = invoiceLine.Clone(new BusinessObjectCloneArgs(factory2, new string[] { JobComInvoiceLine.Schema.JI_JZ }, typeof(JobComInvoiceLine), false));
			AssertEquals("JI", cloned.FDAs[0].B7_ParentTableCode);
			AssertEquals(cloned.PK, cloned.FDAs[0].B7_ParentID);
			AssertNotEquals(Factory.GetHashCode(), cloned.Factory.GetHashCode());
			AssertNotEquals(Factory.GetHashCode(), cloned.FDAs[0].Factory.GetHashCode());
			AssertEquals(factory2.GetHashCode(), cloned.Factory.GetHashCode());
			AssertEquals(factory2.GetHashCode(), cloned.FDAs[0].Factory.GetHashCode());
		}

		public void TestCloneInternalPGALinesWillBeClonedInSameFactory()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Declaration.US_EnableENS = true;
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_DDTCTrackingStatus = PGATrackingStatusList.Codes.Added;
			invoiceLine.US_ODSTrackingStatus = PGATrackingStatusList.Codes.Added;
			invoiceLine.US_TSCATrackingStatus = PGATrackingStatusList.Codes.Added;
			invoiceLine.SetTrackingID();
			invoiceLine.FeeCusCodes.AddNew();
			invoiceLine.FeeCusCodes[0].CY_Code = "CC";

			var fda = invoiceLine.FDAs.AddNew();
			fda.US_FDAQty1 = 5m;
			fda.US_FDAValue = 5m;
			fda.US_FDAConfirmDate = ZDateTime.Today;

			var ace_fda = invoiceLine.ACE_FDALines.AddNew();
			ace_fda.US_TrackingStatus = "fda";

			var vne = invoiceLine.VehicleLines.AddNew();
			vne.US_TrackingStatus = "vne";

			var ams = invoiceLine.AMSLines.AddNew();
			ams.US_TrackingStatus = "ams";

			var fccs = invoiceLine.FCCs.AddNew();
			fccs.US_FCCID = "fcc";

			var dot = invoiceLine.DOTs.AddNew();
			dot.US_DOTTireBrandName = "dot";

			var lacyAct = invoiceLine.LaceyActLines.AddNew();
			lacyAct.US_TrackingStatus = "lay";

			var fsis = invoiceLine.FSISLines.AddNew();
			fsis.US_TrackingStatus = "fsi";

			var pst = invoiceLine.PSTLines.AddNew();
			pst.US_TrackingStatus = "pst";

			var hfc = invoiceLine.USHFCHeaders.AddNew();
			hfc.US_TrackingStatus = "hfc";

			var nmf = invoiceLine.NMFSLines.AddNew();
			nmf.US_TrackingStatus = "nmf";

			var ttb = invoiceLine.TTBLines.AddNew();
			ttb.US_TrackingStatus = "ttb";

			var aph = invoiceLine.APHISHeaders.AddNew();
			aph.US_TrackingStatus = "aph";

			var omc = invoiceLine.OMCHeaders.AddNew();
			omc.US_TrackingStatus = "omc";

			var fws = invoiceLine.FWSHeaders.AddNew();
			fws.US_TrackingStatus = "fws";

			var nht = invoiceLine.NHTSALines.AddNew();
			nht.US_TrackingStatus = "nht";

			var atf = invoiceLine.ATFLines.AddNew();
			atf.US_TrackingStatus = "atf";

			var cps = invoiceLine.CPSCHeaders.AddNew();
			cps.US_TrackingStatus = "cps";

			var dea = invoiceLine.DEAHeaders.AddNew();
			dea.US_TrackingStatus = "dea";

			var factory2 = new BusinessObjectFactory();
			var cloned = invoiceLine.Clone(new BusinessObjectCloneArgs(factory2, new string[] { JobComInvoiceLine.Schema.JI_JZ }, typeof(JobComInvoiceLine), false));
			AssertEquals("invoiceLine in current Factory", Factory.GetHashCode(), invoiceLine.Factory.GetHashCode());
			AssertEquals("Cloned line in new Factory", factory2.GetHashCode(), cloned.Factory.GetHashCode());
			Assert(cloned.FDAs.Count > 0);
			Assert(cloned.ACE_FDALines.Count > 0);
			Assert(cloned.VehicleLines.Count > 0);
			Assert(cloned.AMSLines.Count > 0);
			Assert(cloned.FCCs.Count > 0);
			Assert(cloned.DOTs.Count > 0);
			Assert(cloned.LaceyActLines.Count > 0);
			Assert(cloned.FSISLines.Count > 0);
			Assert(cloned.PSTLines.Count > 0);
			Assert(cloned.USHFCHeaders.Count > 0);
			Assert(cloned.NMFSLines.Count > 0);
			Assert(cloned.TTBLines.Count > 0);
			Assert(cloned.APHISHeaders.Count > 0);
			Assert(cloned.OMCHeaders.Count > 0);
			Assert(cloned.FWSHeaders.Count > 0);
			Assert(cloned.NHTSALines.Count > 0);
			Assert(cloned.ATFLines.Count > 0);
			Assert(cloned.CPSCHeaders.Count > 0);
			Assert(cloned.DEAHeaders.Count > 0);
			AssertEquals(cloned.FDAs[0].Factory.GetHashCode(), factory2.GetHashCode());
			AssertEquals(cloned.ACE_FDALines[0].Factory.GetHashCode(), factory2.GetHashCode());
			AssertEquals(cloned.VehicleLines[0].Factory.GetHashCode(), factory2.GetHashCode());
			AssertEquals(cloned.AMSLines[0].Factory.GetHashCode(), factory2.GetHashCode());
			AssertEquals(cloned.FCCs[0].Factory.GetHashCode(), factory2.GetHashCode());
			AssertEquals(cloned.DOTs[0].Factory.GetHashCode(), factory2.GetHashCode());
			AssertEquals(cloned.LaceyActLines[0].Factory.GetHashCode(), factory2.GetHashCode());
			AssertEquals(cloned.FSISLines[0].Factory.GetHashCode(), factory2.GetHashCode());
			AssertEquals(cloned.PSTLines[0].Factory.GetHashCode(), factory2.GetHashCode());
			AssertEquals(cloned.USHFCHeaders[0].Factory.GetHashCode(), factory2.GetHashCode());
			AssertEquals(cloned.NMFSLines[0].Factory.GetHashCode(), factory2.GetHashCode());
			AssertEquals(cloned.TTBLines[0].Factory.GetHashCode(), factory2.GetHashCode());
			AssertEquals(cloned.APHISHeaders[0].Factory.GetHashCode(), factory2.GetHashCode());
			AssertEquals(cloned.OMCHeaders[0].Factory.GetHashCode(), factory2.GetHashCode());
			AssertEquals(cloned.FWSHeaders[0].Factory.GetHashCode(), factory2.GetHashCode());
			AssertEquals(cloned.NHTSALines[0].Factory.GetHashCode(), factory2.GetHashCode());
			AssertEquals(cloned.ATFLines[0].Factory.GetHashCode(), factory2.GetHashCode());
			AssertEquals(cloned.CPSCHeaders[0].Factory.GetHashCode(), factory2.GetHashCode());
			AssertEquals(cloned.DEAHeaders[0].Factory.GetHashCode(), factory2.GetHashCode());
		}

		[TestDate(2018, 12, 19)]
		public void TestDefaultSupTariffForSTNTariffInSets()
		{
			var testHelper = new Chapter98HelperTest();
			var tariff99038801 = testHelper.Test99038801Tariff;
			testHelper.SetTestTariffCodesForA99();

			var job = Factory.NewWithValidTestData<JobDeclaration>();
			job.JE_MessageType = JobMessageTypeList.Codes.Import;
			job.US_EntryFilerCode = "XJ5";
			job.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			job.US_EnableENS = true;
			var invoice = job.Invoices.AddNew();
			var parentLine = invoice.JobComInvoiceLines.AddNew();
			parentLine.US_UC_NKCountryOfExport = "CA";
			parentLine.US_UC_NKCountryOfOrigin = "CN";
			parentLine.JI_LinePrice = 5000m;
			parentLine.JI_Tariff = "9106905510";
			Factory.Save();

			AssertEquals(1, parentLine.SecondaryTariffLines.Count());
			var childLine = parentLine.SecondaryTariffLines.FirstOrDefault();
			AssertEquals(ZString.Empty, childLine.US_SupTariff);
			AssertEquals("99038801", parentLine.US_SupTariff);
		}

		public void TestUniversalImportTariff()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "HSN");  // TariffType
			var ctrlType1 = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, UniversalReferenceConstants.TariffConditionTypes.Codes.IRCMF, "Test IRCMF"); // ConditionType
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
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "TEST55";
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Description = "Test Warning message with HTS FOR MX";
			invoiceLine.JI_Tariff = string.Empty;
			AssertNull("should return null", invoiceLine.UniversalImportTariff);

			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Mexico;
			invoiceLine.JI_Tariff = "0303590000";
			AssertNotNull("should return TariffView", invoiceLine.UniversalImportTariff);
			AssertHasWarningContaining(invoiceLine.JI_TariffInfo, warningMessage);
		}

		public void TestSupTariff()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "HSN");  // TariffType
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "99038801", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "TEST55";
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_SupTariff = string.Empty;
			AssertNull("should return null", invoiceLine.SupTariff);

			invoiceLine.US_SupTariff = "99038801";
			AssertNotNull("should return TariffView", invoiceLine.SupTariff);
		}

		public override void TestConsigneeAddressForDocument()
		{
			var consigneeAddress = Factory.New<OrgAddress>();
			InvoiceLine.JI_OA_ConsigneeAddress = consigneeAddress.PK;
			AssertEquals(InvoiceLine.ConsigneeAddressForDocument, InvoiceLine.ConsigneeAddress);
		}

		public void TestASNRefresh()
		{
			var fallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			var collection = new ASNRefreshOptionsConfigCollection(fallbackLevel, Factory);
			var config1 = collection.AddNew();
			config1.FieldType = DefaultOptions.Codes.Classification;
			var config2 = collection.AddNew();
			config2.FieldType = DefaultOptions.Codes.Tariff;

			CustomsDataRegistry.Instance.ASNRefreshOptions.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			var invoice = Factory.New<JobComInvoiceHeader>();
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "IMPTEST";
			invoice.JZ_OH_Buyer = importer.PK;
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "SUPTEST";
			invoice.RefreshDefaultsWhenInvoiceAttachedToDeclarationRequired = true;
			invoice.JZ_OH_Supplier = supplier.PK;
			invoice.JZ_StandAloneInvoiceDirection = JobMessageTypeList.MoreCodes.AdvanceShippingNotice;
			invoice.JZ_MessageType = JobMessageTypeList.MoreCodes.AdvanceShippingNotice;

			var line = invoice.InvoiceLines.AddNew();
			var classification = Factory.New<CusClassification>();
			classification.FillWithValidTestData();
			classification.CC_TariffNum = "111111111";
			line.JI_CC = classification.PK;

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "AMYTEST";

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;
			pivot.CI_TariffNum = "1010101010";
			pivot.CD_UC_NKCountryOfOrigin = "CA";
			pivot.CD_SPI = "SPI";
			var relatedOrg1 = product.RelatedOrganisations.AddNew();
			relatedOrg1.OU_OH = importer.PK;
			relatedOrg1.OU_Relationship = "OWN";
			var relatedOrg2 = product.RelatedOrganisations.AddNew();
			relatedOrg2.OU_OH = supplier.PK;
			relatedOrg2.OU_Relationship = "SUP";

			line.JI_OP = product.PK;
			line.JI_PartNo = product.OP_PartNum;
			AssertEquals(pivot, line.Pivot);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			invoice.JZ_JE = declaration.PK;
			AssertEquals("Refreshed by the registry", pivot.CI_CC, line.JI_CC);
			AssertEquals("Refreshed by the registry", pivot.CI_TariffNum, line.JI_Tariff);
			AssertEquals("Refreshed by the registry", ZString.Empty, line.US_UC_NKCountryOfOrigin);
			AssertEquals("Refreshed by the registry", ZString.Empty, line.US_SPI);

			var companyData = importer.CompanyData;
			companyData.ImporterOverride = true;
			companyData.OB_IMProductValueDefaultOptions = "OVR,COO,PREFF";

			invoice.JZ_JE = ZGuid.Empty;
			invoice.JZ_MessageType = JobMessageTypeList.MoreCodes.AdvanceShippingNotice;
			invoice.JZ_StandAloneInvoiceDirection = JobMessageTypeList.MoreCodes.AdvanceShippingNotice;
			invoice.RefreshDefaultsWhenInvoiceAttachedToDeclarationRequired = true;
			line.JI_CC = ZGuid.Empty;
			line.JI_Tariff = ZString.Empty;
			invoice.JZ_JE = declaration.PK;
			AssertEquals("Refreshed by the consignee config", ZGuid.Empty, line.JI_CC);
			AssertEquals("Refreshed by the consignee config", pivot.CD_UC_NKCountryOfOrigin, line.US_UC_NKCountryOfOrigin);
			AssertEquals("Refreshed by the consignee config", ZString.Empty, line.JI_Tariff);
			AssertEquals("Refreshed by the consignee config", pivot.CD_SPI, line.US_SPI);
		}

		[TestDate(2018, 10, 22)]
		public void TestSPIChangedForCombinedLines()
		{
			var testHelper = new CombinedLinesHelperTest();
			var testJob = testHelper.CombinedJob;
			var invoiceLine1 = testHelper.InvoiceHeaderForCombined.InvoiceLines.AddNew();
			var invoiceLine2 = testHelper.InvoiceHeaderForCombined.InvoiceLines.AddNew();

			invoiceLine1.US_UC_NKCountryOfOrigin = "CN";
			invoiceLine1.US_UC_NKCountryOfExport = "CN";
			invoiceLine1.US_SupTariff = testHelper.Chapter98TestingHelper.Test99038801Tariff.UE_Tariff;
			invoiceLine1.US_SPI = ZString.Empty;

			invoiceLine2.US_SPI = "K";
			invoiceLine2.US_SupTariff = "9802.00.4020";
			invoiceLine2.JI_Tariff = "8803.30.0060";
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			AssertEquals(ZString.Empty, invoiceLine2.US_SPI);

			invoiceLine2.JI_ParentID = ZGuid.Empty;
			invoiceLine1.US_SPI = "C";
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			AssertEquals("C", invoiceLine2.US_SPI);

			invoiceLine1.US_SPI = "K";
			AssertEquals("K", invoiceLine2.US_SPI);
		}

		public void TestIsTaxRateSpecifiedManually()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_TaxRateS = string.Empty;

			AssertEquals(false, invoiceLine.IsTaxRateSpecifiedManually);

			invoiceLine.US_TaxRateS = AppendixBTaxRateList.Codes.Specify;
			AssertEquals(true, invoiceLine.IsTaxRateSpecifiedManually);

			invoiceLine.US_TaxRateS = AppendixBTaxRateList.CBMAEligible;
			AssertEquals(true, invoiceLine.IsTaxRateSpecifiedManually);

			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine.US_TaxRateS = string.Empty;
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2022, 01, 10);
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.C;
			AssertEquals(true, invoiceLine.IsTaxRateSpecifiedManually);

			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2023, 01, 10);
			invoiceLine.US_SecondarySPI = ZString.Empty;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.C;
			AssertEquals(false, invoiceLine.IsTaxRateSpecifiedManually);
		}

		public void TestDefaultTaxRateSForCBMAIfPossible()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2022, 01, 10);
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine.US_TaxRateS = "ABC";
			invoiceLine.US_TTBRateDesignationCode = "DEF";
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.C;
			AssertEquals(ZString.Empty, invoiceLine.US_TaxRateS);
			AssertEquals("DEF", invoiceLine.US_TTBRateDesignationCode);

			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2023, 01, 10);
			invoiceLine.US_SecondarySPI = ZString.Empty;
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine.US_TaxRateS = "ABC";
			invoiceLine.US_TTBRateDesignationCode = "DEF";
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.C;
			AssertEquals("ABC", invoiceLine.US_TaxRateS);
			AssertEquals(ZString.Empty, invoiceLine.US_TTBRateDesignationCode);
		}

		[TestDate(2018, 8, 29)]
		[ExpectNoExceptions]
		public void TestDateForMPFCalculation()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			AssertNull(invoiceLine.Declaration);
			AssertEquals(new ZDateTime(2018, 8, 29), invoiceLine.DateForMPFCalculation);

			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.US_MPFCalcDate = new ZDateTime(2018, 8, 30);
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			var invoice = declaration.Invoices.AddNew();
			invoiceLine.JI_JZ = invoice.PK;
			AssertNotNull(invoiceLine.Declaration);
			AssertEquals(new ZDateTime(2018, 8, 30), invoiceLine.DateForMPFCalculation);
		}

		public override void TestChargeTypeList()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			Customs.Business.ICommonInvoice commonInvoice = dec.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var chargeTypeList1 = commonInvoice.ChargeTypeList;
			var chargeTypeList2 = commonInvoice.ChargeTypeList;
			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));
			CodeDescriptionPairList customsChargeTypeList = new USCustomsChargeTypeList();
			customsChargeTypeList.Sort();
			AssertEquals(customsChargeTypeList.CodesAsString, chargeTypeList1.CodesAsString);

			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			chargeTypeList1 = commonInvoice.ChargeTypeList;
			chargeTypeList2 = commonInvoice.ChargeTypeList;
			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));
			customsChargeTypeList = new Customs.Business.CustomsChargeTypeList();
			customsChargeTypeList.Sort();
			AssertEquals(customsChargeTypeList.CodesAsString, chargeTypeList1.CodesAsString);
		}

		public void TestIsACSForRecon()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var invoice = reconDeclaration.Invoices.AddNew();
			var line = invoice.InvoiceLines.AddNew();

			AssertEquals("Default to false", false, line.IsACSForRecon);

			var originalEntry = reconDeclaration.OriginalEntries.AddNew();
			originalEntry.US_R_MsgMode = JobApplicationCodeList.Codes.ACE;

			invoice.US_CH_ReconEntry = originalEntry.CH_PK;
			AssertEquals("ReconOriginalEntry", originalEntry, invoice.ReconOriginalEntry);
			AssertEquals("OriginalEntry is not ACS", false, line.IsACSForRecon);

			originalEntry.US_R_MsgMode = JobApplicationCodeList.Codes.ACS;
			AssertEquals("OriginalEntry is ACS", true, line.IsACSForRecon);
		}

		public void TestDeclaredFDAForFTZStandAlonePriorNotice()
		{
			var importTariff = Factory.New<USCTariff>();
			importTariff.UE_Tariff = "0000000000";
			importTariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			importTariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			importTariff.UE_OGACodes = "";
			importTariff.UE_PGACodes = "FD2";
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.US_EnableSPN = true;
			declaration.US_F_PNMode = "P";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = importTariff.UE_Tariff;
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_FDAIndicator);
			Assert(invoiceLine.JI_FDARequirementDesc.Contains("FD2"));
		}

		public void TestUS_ManifestUQ_ReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			declaration.US_EntryType = "06";
			Assert(!invoiceLine.US_ManifestUQInfo.ReadOnly);
			declaration.US_EntryType = "07";
			Assert(invoiceLine.US_ManifestUQInfo.ReadOnly);
		}

		public void TestHasAMSMO4()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			Assert("Has No AMS Disclaimed MO4", !invoiceLine.HasAMSMO4);

			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			var amsLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = AMSProgramList.Codes.MO3;
			Assert("Has No AMS Disclaimed MO4", !invoiceLine.HasAMSMO4);

			var amsLine2 = invoiceLine.AMSLines.AddNew();
			amsLine2.US_Program = AMSProgramList.Codes.MO4;
			Assert("Has AMS Disclaimed MO4", invoiceLine.HasAMSMO4);
		}

		public void TestImportWithoutDisorderOfLineNumbers()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice0 = declaration.Invoices.AddNew();
			invoice0.JZ_InvoiceNumber = "MD17027011";
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "MD17027012";
			var collection = declaration.InvoiceLines;
			var impl = new ImportCollectionInfoImpl(collection);
			impl.Add(new ImportPropertyInfoImpl<JobComInvoiceLine>(JobComInvoiceLine.Schema.US_JI_ParentProduct) { HeaderText = "Product Code" });
			impl.Add(new ImportPropertyInfoImpl<JobComInvoiceLine>(JobComInvoiceLine.Schema.JI_Calc_Invoice) { HeaderText = "Inv. No." });
			var wizard = new ImportWizard(impl, null, new ZArchitecture.DataMapping.Testing.FileMapperForTest());
			wizard.Mapping[0].AddFileColumnIndex(0);
			wizard.Mapping[1].AddFileColumnIndex(1);
			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				var testDocumentPath = resourceRetriever.SaveResourceToFile("Enterprise.Customs.US.Business.Testing.Business.JobComInvoiceLine.TestFiles.Anvil Test.csv");
				wizard.FileName = testDocumentPath;
				wizard.StartingRow = 1;
				wizard.ImportIntoCollection(collection);
				var jobInvoice1Lines = collection.Cast<JobComInvoiceLine>().Where(line => line.InvoiceNumber == "MD17027011").ToList();
				for (int i = 0; i < jobInvoice1Lines.Count; i++)
				{
					AssertEquals(i + 1, jobInvoice1Lines[i].JI_LineNo);
				}
				var jobInvoice2Lines = collection.Cast<JobComInvoiceLine>().Where(line => line.InvoiceNumber == "MD17027012").ToList();
				for (int i = 0; i < jobInvoice2Lines.Count; i++)
				{
					AssertEquals(i + 1, jobInvoice2Lines[i].JI_LineNo);
				}
			}
		}

		public void TestTSCADataMembers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			var invoice = declaration.Invoices.AddNew();
			invoice.US_FDAContactName = "BROKER NAME";
			invoice.US_FDAContactPhoneNo = "2345678";
			invoice.US_FDAContactEmail = "broker@abc.com";

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_TSCAODSCertIndividual = PartyTypeList.Codes.CustomsBroker;
			ITSCAData tscaData = invoiceLine;
			AssertEquals("BROKER NAME", tscaData.ContactName);
			AssertEquals("2345678", tscaData.ContactPhone);
			AssertEquals("broker@abc.com", tscaData.ContactEmail);

			invoiceLine.US_FDAContactName = "TEST NAME";
			invoiceLine.US_FDAContactPhoneNo = "11223344";
			invoiceLine.US_FDAContactEmail = "test@abc.com";
			AssertEquals("TEST NAME", tscaData.ContactName);
			AssertEquals("11223344", tscaData.ContactPhone);
			AssertEquals("test@abc.com", tscaData.ContactEmail);
		}

		public void TestSetDefaultValueToFDADisclaimReason()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertEquals(PGADisclaimReasonList.Codes.A, invoiceLine.US_FDADisclaimReason);
		}

		public void TestUS_LicenseTypeRequiredSpaceCode()
		{
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(Declaration.Factory, new ZString[] { USAESLicenseCode.Codes.C38, USAESLicenseCode.Codes.S00, USAESLicenseCode.Codes.SAG, USAESLicenseCode.Codes.VDO });

			var invoice = Declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "IIVV11";
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_OrderNumber = "IILL33";
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_OrderNumber = "IILL22";
			invoiceLine2.US_LicenseType = USAESLicenseCode.Codes.C38;

			Declaration.US_LicenseType = "";
			Declaration.US_LicenseType = USAESLicenseCode.Codes.C33;
			AssertEquals(LicenseExemptionTypeList.Codes.NLR, Declaration.US_LicenseNo);
			AssertEquals(USAESLicenseCode.Codes.C33, invoice.US_LicenseType);
			AssertEquals(LicenseExemptionTypeList.Codes.NLR, invoice.US_LicenseNo);
			AssertEquals(USAESLicenseCode.Codes.C33, invoiceLine.US_LicenseType);
			AssertEquals(LicenseExemptionTypeList.Codes.NLR, invoiceLine.US_LicenseNo);

			invoice.US_LicenseType = USAESLicenseCode.Codes.S00;
			AssertEquals("Header - LicenseType - S00", USAESLicenseCode.Codes.S00, invoice.US_LicenseType);
			AssertEquals("Header - LicenseNo - Space", ZString.Empty, invoice.US_LicenseNo);

			AssertEquals(USAESLicenseCode.Codes.S00, invoiceLine.US_LicenseType);
			AssertEquals("", invoiceLine.US_LicenseNo);
			AssertEquals(USAESLicenseCode.Codes.C38, invoiceLine2.US_LicenseType);
			AssertEquals(LicenseExemptionTypeList.Codes.TSR, invoiceLine2.US_LicenseNo);

			invoiceLine2.US_LicenseType = USAESLicenseCode.Codes.VDO;
			AssertEquals(USAESLicenseCode.Codes.VDO, invoiceLine2.US_LicenseType);
			AssertEquals(ZString.Empty, invoiceLine2.US_LicenseNo);

			AssertEquals(LicenseExemptionTypeList.Codes.NLR, Declaration.US_LicenseNo);
			AssertEquals(USAESLicenseCode.Codes.C33, Declaration.US_LicenseType);

			invoiceLine2.US_LicenseNo = "23";
			invoiceLine2.US_LicenseType = USAESLicenseCode.Codes.SAG;
			AssertEquals(USAESLicenseCode.Codes.SAG, invoiceLine2.US_LicenseType);
			AssertEquals(ZString.Empty, invoiceLine2.US_LicenseNo);

			invoiceLine2.US_LicenseType = USAESLicenseCode.Codes.SAG;
			invoiceLine2.US_LicenseNo = "23";
			AssertEquals(USAESLicenseCode.Codes.SAG, invoiceLine2.US_LicenseType);
			AssertEquals("23", invoiceLine2.US_LicenseNo);
		}

		public void TestDoNottDefaultDDTCIndicator()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.FillWithValidTestData();
			consignee.OH_Code = "ORGUSCHI";
			consignee.OH_IsConsignee = ZBool.True;
			consignee.OH_RL_NKClosestPort = "USCHI";

			var part1 = Factory.New<OrgSupplierPart>();
			part1.OP_PartNum = "PART1";
			part1.OP_Weight = 100m;
			part1.OP_WeightUQ = "HG";
			part1.RelatedOrganisations.AddOrganisationIfNotExist(consignee.PK, OrgPartRelation.RelationshipTypes.Owner);

			var pivot1 = part1.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot1.CI_TariffNum = USCTariff.CottonFeeApplicable;

			AssertEquals("DDTC is not set when Tariff number is changed on Product", ZString.Empty, pivot1.Details.CD_DDTCIndicator);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.JE_OH_Importer = consignee.PK;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;
			tariff.UE_PGACodes = "TB2";

			invoiceLine1.JI_Tariff = tariff.UE_Tariff;
			AssertEquals("DDTC is not set when Tariff number is changed on Invoice Line", ZString.Empty, pivot1.Details.CD_DDTCIndicator);
		}

		public void TestReNumberinOnLoading()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.FillWithValidTestData();
			consignee.OH_Code = "ORGUSCHI";
			consignee.OH_IsConsignee = ZBool.True;
			consignee.OH_RL_NKClosestPort = "USCHI";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.JE_OH_Importer = consignee.PK;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_Description = "LINE 1";
			invoiceLine1.JI_InvoiceQuantity = 100m;
			invoiceLine1.JI_InvoiceUQ = "PCE";
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_Description = "LINE 2";
			invoiceLine2.JI_PartNo = "PART1";
			invoiceLine2.JI_InvoiceQuantity = 100m;
			var invoiceLine3 = invoice.InvoiceLines.AddNew();
			invoiceLine3.JI_Description = "LINE 3";
			invoiceLine3.JI_InvoiceQuantity = 100m;
			invoiceLine3.JI_InvoiceUQ = "PCE";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var part1 = newFactory.New<OrgSupplierPart>();
			part1.OP_PartNum = "PART1";
			part1.OP_Weight = 100m;
			part1.OP_WeightUQ = "HG";
			part1.RelatedOrganisations.AddOrganisationIfNotExist(consignee.PK, OrgPartRelation.RelationshipTypes.Owner);
			var pivot1 = part1.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot1.CI_TariffNum = USCTariff.CottonFeeApplicable;

			newFactory.Save();

			newFactory = new BusinessObjectFactory();
			declaration = newFactory.Load<JobDeclaration>(declaration.PK);
			declaration.RefreshExRateToLatestRateAvailableIfNeeded();
			AssertEquals(3, declaration.InvoiceLines.Count);
			invoiceLine1 = (JobComInvoiceLine)declaration.InvoiceLines.FindByPK(invoiceLine1.PK);
			AssertEquals("invoiceLine1.JI_LineNo", (short)1, invoiceLine1.JI_LineNo);
			invoiceLine2 = (JobComInvoiceLine)declaration.InvoiceLines.FindByPK(invoiceLine2.PK);
			AssertEquals("invoiceLine2.JI_LineNo", (short)2, invoiceLine2.JI_LineNo);
			AssertEquals("invoiceLine2.JI_OP", part1.PK, invoiceLine2.JI_OP);
			invoiceLine3 = (JobComInvoiceLine)declaration.InvoiceLines.FindByPK(invoiceLine3.PK);
			AssertEquals("invoiceLine3.JI_LineNo", (short)3, invoiceLine3.JI_LineNo);

			invoiceLine2.JI_PartNo = "PART2";
			invoiceLine2.JI_Tariff = ZString.Empty;
			invoiceLine2.JI_CustomsQuantity = ZDecimal.Zero;
			AssertEquals("invoiceLine2.JI_OP", ZGuid.Empty, invoiceLine2.JI_OP);
			newFactory.Save();

			newFactory = new BusinessObjectFactory();
			var part2 = newFactory.New<OrgSupplierPart>();
			part2.OP_PartNum = "PART2";
			part2.OP_Weight = 100m;
			part2.OP_WeightUQ = "HG";
			part2.RelatedOrganisations.AddOrganisationIfNotExist(consignee.PK, OrgPartRelation.RelationshipTypes.Owner);
			var pivot2 = part2.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot2.CI_TariffNum = USCTariff.CottonFeeApplicable;

			var compoment = pivot2.Children.AddNew();
			compoment.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			compoment.CI_TariffNum = USCTariff.AGOABenefitsApplicable;
			newFactory.Save();

			newFactory = new BusinessObjectFactory();
			declaration = newFactory.Load<JobDeclaration>(declaration.PK);
			declaration.RefreshExRateToLatestRateAvailableIfNeeded();
			AssertEquals(4, declaration.InvoiceLines.Count);
			invoiceLine1 = (JobComInvoiceLine)declaration.InvoiceLines.FindByPK(invoiceLine1.PK);
			AssertEquals("invoiceLine1.JI_LineNo", (short)1, invoiceLine1.JI_LineNo);
			invoiceLine2 = (JobComInvoiceLine)declaration.InvoiceLines.FindByPK(invoiceLine2.PK);
			AssertEquals("invoiceLine2.JI_LineNo", (short)2, invoiceLine2.JI_LineNo);
			AssertEquals("invoiceLine2.JI_OP", part2.PK, invoiceLine2.JI_OP);
			var childLines = invoiceLine2.ChildLines.ToArray();
			AssertEquals(1, childLines.Length);
			AssertEquals("childLines[0].JI_LineNo", (short)3, childLines[0].JI_LineNo);
			invoiceLine3 = (JobComInvoiceLine)declaration.InvoiceLines.FindByPK(invoiceLine3.PK);
			AssertEquals("invoiceLine3.JI_LineNo", (short)4, invoiceLine3.JI_LineNo);
		}

		public void TestASNInvoiceKeepsInvoiceUQ()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.FillWithValidTestData();
			consignee.OH_Code = "ORGUSCHI";
			consignee.OH_IsConsignee = ZBool.True;
			consignee.OH_RL_NKClosestPort = "USCHI";
			OrgHeaderWrapper.New(consignee).ZO_DoNotConvertSKU = true;

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PART1";
			part.OP_Weight = 100m;
			part.OP_WeightUQ = "HG";
			part.OP_StockKeepingUnit = "U!";
			part.RelatedOrganisations.AddOwner(consignee);

			var refPack = Factory.New<CusRefPacks>();
			refPack.RP_CustomsCountry = Core.Constants.CountryCodes.UnitedStates;
			refPack.RP_CommercialPack = "U!";
			refPack.RP_CustomsPack = ABIUnitOfMeasureList.Codes.Pieces;
			refPack.RP_ConversionFactor = 1;

			var invoice = Factory.New<JobComInvoiceHeader>();
			new FakeDeclarationCreatorForInvoice(invoice);
			invoice.JZ_OH_Buyer = consignee.PK;
			invoice.JZ_StandAloneInvoiceDirection = JobMessageTypeList.MoreCodes.AdvanceShippingNotice;
			invoice.JZ_MessageType = JobMessageTypeList.MoreCodes.AdvanceShippingNotice;
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_PartNo = "PART1";
			invoiceLine1.JI_InvoiceQuantity = 100m;
			invoiceLine1.JI_InvoiceUQ = "PCE";
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_PartNo = "PART1";
			invoiceLine2.JI_InvoiceQuantity = 100m;
			Factory.Save();

			var invoiceLoaded = new BusinessObjectFactory().Load<JobComInvoiceHeader>(invoice.PK);

			var declaration = invoiceLoaded.Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = consignee.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			invoiceLoaded.RefreshDefaultsWhenInvoiceAttachedToDeclarationRequired = true;
			declaration.Invoices.Add(invoiceLoaded);

			var invoiceLine1Loaded = invoiceLoaded.InvoiceLines[0];
			var invoiceLine2Loaded = invoiceLoaded.InvoiceLines[1];
			if (invoiceLine2Loaded.PK == invoiceLine1.PK)
			{
				invoiceLine1Loaded = invoiceLoaded.InvoiceLines[1];
				invoiceLine2Loaded = invoiceLoaded.InvoiceLines[0];
			}
			AssertEquals("PART1", invoiceLine1Loaded.JI_PartNo);
			AssertEquals(part.PK, invoiceLine1Loaded.JI_OP);
			AssertEquals(100m, invoiceLine1Loaded.JI_InvoiceQuantity);
			AssertEquals("U!", invoiceLine1Loaded.JI_InvoiceUQ);
			AssertEquals("PART1", invoiceLine2Loaded.JI_PartNo);
			AssertEquals(part.PK, invoiceLine2Loaded.JI_OP);
			AssertEquals(100m, invoiceLine2Loaded.JI_InvoiceQuantity);
			AssertEquals("U!", invoiceLine2Loaded.JI_InvoiceUQ);
		}

		public void TestODSOrTSCADeclared()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_TSCAInd = OGAIndicatorList.Codes.Declared;
			Assert(invoiceLine.IsTSCAIndBeDeclared);

			invoiceLine.US_TSCAInd = OGAIndicatorList.Codes.Disclaimed;
			Assert(!invoiceLine.IsTSCAIndBeDeclared);

			invoiceLine.US_TSCAInd = "";
			Assert(!invoiceLine.IsTSCAIndBeDeclared);

			invoiceLine.US_ODSInd = OGAIndicatorList.Codes.Declared;
			Assert(invoiceLine.IsODSIndBeDeclared);

			invoiceLine.US_ODSInd = OGAIndicatorList.Codes.Disclaimed;
			Assert(!invoiceLine.IsODSIndBeDeclared);

			invoiceLine.US_ODSInd = "";
			Assert(!invoiceLine.IsODSIndBeDeclared);

			invoiceLine.US_TSCACertification = TSCAIndicatorList.Codes.TSCANegative;

			invoiceLine.US_ODSInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_TSCAInd = "";
			AssertEquals(PartyTypeList.Codes.Importer, invoiceLine.US_TSCAODSCertIndividual);
			AssertEquals(TSCAIndicatorList.Codes.TSCANegative, invoiceLine.US_TSCACertification);

			invoiceLine.US_TSCAODSCertIndividual = PartyTypeList.Codes.CustomsBroker;
			invoiceLine.US_TSCAInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_ODSInd = OGAIndicatorList.Codes.Disclaimed;
			AssertEquals(PartyTypeList.Codes.CustomsBroker, invoiceLine.US_TSCAODSCertIndividual);
			AssertEquals(TSCAIndicatorList.Codes.TSCANegative, invoiceLine.US_TSCACertification);

			invoiceLine.US_TSCAInd = OGAIndicatorList.Codes.Disclaimed;
			AssertEquals(ZString.Empty, invoiceLine.US_TSCAODSCertIndividual);
			AssertEquals(ZString.Empty, invoiceLine.US_TSCACertification);
		}

		public void TestCopyFWSAndNMFSFromProduct()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "TESTOH";
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "TESTNUM";
			product.OP_Weight = 100m;
			product.OP_WeightUQ = "HG";

			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = importer.PK;
			relOrg.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			var fwsTariff = Factory.New<USCTariff>();
			fwsTariff.UE_Tariff = "8422401190";
			fwsTariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			fwsTariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			fwsTariff.UE_PGACodes = "FW2NM3NM5NM2";
			fwsTariff.UE_OGACodes = "FW2NM3NM5NM2";

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = fwsTariff.UE_Tariff;
			pivot.Details.CD_FWSIndicator = OGAIndicatorList.Codes.Declared;
			pivot.Details.CD_NMFS370Indicator = OGAIndicatorList.Codes.Declared;
			pivot.Details.CD_NMFSHMSIndicator = OGAIndicatorList.Codes.Declared;
			pivot.Details.CD_NMFSAMRIndicator = OGAIndicatorList.Codes.Declared;
			pivot.Details.CD_NMFSSIMPIndicator = OGAIndicatorList.Codes.Declared;

			var productFWS = pivot.FWSLines.AddNew();
			productFWS.US_ProcessingCode = "FTS";
			var license = productFWS.Licenses.AddNew();
			license.US_Type = "TYP";

			var productHMS = pivot.NMFSLines.AddNew();
			productHMS.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
			productHMS.US_DocumentType = "HMS";
			var detailsHMS = productHMS.HarvestingDetails.AddNew();
			detailsHMS.US_HarvestedCountry = Core.Constants.CountryCodes.Ukraine;

			var productAMR = pivot.NMFSLines.AddNew();
			productAMR.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			productAMR.US_DocumentType = "AMR";
			var detailsARM = productAMR.HarvestingDetails.AddNew();
			detailsARM.US_HarvestedCountry = Core.Constants.CountryCodes.Kenya;

			var product370 = pivot.NMFSLines.AddNew();
			product370.US_ProgramType = NMFSProgramCodeList.Codes._370;
			product370.US_DocumentType = "370";
			var details370 = product370.HarvestingDetails.AddNew();
			details370.US_HarvestedCountry = Core.Constants.CountryCodes.Russia;

			var productSIMP = pivot.NMFSLines.AddNew();
			productSIMP.US_ProgramType = NMFSProgramCodeList.Codes.SIM;
			productSIMP.US_SpeciesCode = "AK";
			var detailsSIMP = productSIMP.HarvestingDetails.AddNew();
			detailsSIMP.US_HarvestedCountry = Core.Constants.CountryCodes.China;

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_PartNo = product.OP_PartNum;

			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_FWSInd);
			AssertEquals(1, invoiceLine.FWSHeaders.Count);
			var invoiceLineFWS = invoiceLine.FWSHeaders[0];
			AssertEquals("FTS", invoiceLineFWS.US_ProcessingCode);

			AssertEquals(1, invoiceLineFWS.Licenses.Count);
			var fwsLicense = invoiceLineFWS.Licenses[0];
			AssertEquals("TYP", fwsLicense.US_Type);

			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_NMFS370Ind);
			NMFSLine nmfs370 = invoiceLine.NMFS370Lines.First();
			AssertEquals("370", nmfs370.US_DocumentType);
			AssertEquals(1, nmfs370.HarvestingDetails.Count);
			AssertEquals(Core.Constants.CountryCodes.Russia, nmfs370.HarvestingDetails[0].US_HarvestedCountry);

			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_NMFSHMSInd);
			NMFSLine nmfsHMS = invoiceLine.NMFSHMSLines.First();
			AssertEquals("HMS", nmfsHMS.US_DocumentType);
			AssertEquals(1, nmfsHMS.HarvestingDetails.Count);
			AssertEquals(Core.Constants.CountryCodes.Ukraine, nmfsHMS.HarvestingDetails[0].US_HarvestedCountry);

			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_NMFSAMRInd);
			NMFSLine nmfsAMR = invoiceLine.NMFSAMRLines.First();
			AssertEquals("AMR", nmfsAMR.US_DocumentType);
			AssertEquals(1, nmfsAMR.HarvestingDetails.Count);
			AssertEquals(Core.Constants.CountryCodes.Kenya, nmfsAMR.HarvestingDetails[0].US_HarvestedCountry);

			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_NMFSSIMPInd);
			NMFSLine nmfsSIMP = invoiceLine.NMFSSIMPLines.First();
			AssertEquals("AK", nmfsSIMP.US_SpeciesCode);
			AssertEquals(1, nmfsSIMP.HarvestingDetails.Count);
			AssertEquals(Core.Constants.CountryCodes.China, nmfsSIMP.HarvestingDetails[0].US_HarvestedCountry);
		}

		public void TestCopyAMSFromProduct()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "TESTOH";
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "TESTNUM";
			product.OP_Weight = 100m;
			product.OP_WeightUQ = "HG";

			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = importer.PK;
			relOrg.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			var amsTariff = Factory.New<USCTariff>();
			amsTariff.UE_Tariff = "8422401190";
			amsTariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			amsTariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			amsTariff.UE_PGACodes = "AM4AP2";
			amsTariff.UE_OGACodes = "AM4AP2";

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = amsTariff.UE_Tariff;
			pivot.Details.CD_AMSIndicator = OGAIndicatorList.Codes.Declared;

			var productAMS = pivot.AMSLines.AddNew();
			productAMS.US_CommercialDescription = "AMS TEST";
			productAMS.US_Program = "MO1";

			var amsLineForProduct = productAMS.AMSLines.AddNew();
			amsLineForProduct.US_InspecRemarks = "MARKS";
			amsLineForProduct.US_Packages = 5m;
			amsLineForProduct.US_AuthorizationNumber = "Product4";
			amsLineForProduct.US_PackagesUQ = "KG";
			amsLineForProduct.US_IssueDate = new ZDateTime(2016, 09, 07);

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_PartNo = product.OP_PartNum;

			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_AMSInd);
			AssertEquals(1, invoiceLine.AMSLines.Count);

			var invoiceLineAMS = invoiceLine.AMSLines[0];
			AssertEquals("MO1", invoiceLineAMS.US_Program);
			AssertEquals("AMS TEST", invoiceLineAMS.US_CommercialDescription);

			var amslines = invoiceLineAMS.AMSLines;
			AssertEquals(1, amslines.Count);
			AssertEquals(amsLineForProduct.US_IssueDate, amslines[0].US_IssueDate);
			AssertEquals(5m, amslines[0].US_Packages);
			AssertEquals("Product4", amslines[0].US_AuthorizationNumber);
			AssertEquals("KG", amslines[0].US_PackagesUQ);
			AssertEquals("MARKS", amslines[0].US_InspecRemarks);
		}

		public void TestRemoveProductCodeFromInvoiceLine()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "TESTOH";
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "TESTNUM";
			product.OP_Weight = 100m;
			product.OP_WeightUQ = "HG";

			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = importer.PK;
			relOrg.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			var amsTariff = Factory.New<USCTariff>();
			amsTariff.UE_Tariff = "8422401190";
			amsTariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			amsTariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			amsTariff.UE_PGACodes = "AM4AP2";
			amsTariff.UE_OGACodes = "AM4AP2";

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = amsTariff.UE_Tariff;
			pivot.Details.CD_AMSIndicator = OGAIndicatorList.Codes.Declared;

			var productAMS = pivot.AMSLines.AddNew();
			productAMS.US_CommercialDescription = "AMS TEST";
			productAMS.US_Program = "MO1";

			var amsLineForProduct = productAMS.AMSLines.AddNew();
			amsLineForProduct.US_InspecRemarks = "MARKS";
			amsLineForProduct.US_Packages = 5m;
			amsLineForProduct.US_AuthorizationNumber = "Product4";
			amsLineForProduct.US_PackagesUQ = "KG";
			amsLineForProduct.US_IssueDate = new ZDateTime(2016, 09, 07);

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_PartNo = product.OP_PartNum;

			Factory.Save();

			var invoiceLineAMS = invoiceLine.AMSLines[0];
			invoiceLine.JI_PartNo = ZString.Empty;
			AssertEquals("invoiceLineAMS is deleted", true, invoiceLineAMS.IsDeleted);
		}

		public void TestCopyNOPFromProduct()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "TESTOH";
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "TESTNUM";
			product.OP_Weight = 100m;
			product.OP_WeightUQ = "HG";

			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = importer.PK;
			relOrg.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			var nopTariff = Factory.New<USCTariff>();
			nopTariff.UE_Tariff = "8422401190";
			nopTariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			nopTariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			nopTariff.UE_PGACodes = "AM8AP2";
			nopTariff.UE_OGACodes = "AM8AP2";

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = nopTariff.UE_Tariff;
			pivot.Details.CD_NOPIndicator = OGAIndicatorList.Codes.Declared;

			var productNOP = pivot.AMSLines.AddNew();
			productNOP.US_CommercialDescription = "NOP TEST";
			productNOP.US_Program = "OR1";

			var nopLineForProduct = productNOP.AMSLines.AddNew();
			nopLineForProduct.US_InspecRemarks = "MARKS";
			nopLineForProduct.US_Packages = 5m;
			nopLineForProduct.US_AuthorizationNumber = "Product4";
			nopLineForProduct.US_PackagesUQ = "KG";
			nopLineForProduct.US_IssueDate = new ZDateTime(2016, 09, 07);

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var amsLine = invoiceLine.AMSLines.AddNew();
			var nopLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = AMSProgramList.Codes.MO1;
			nopLine.US_Program = AMSProgramList.Codes.OR1;
			invoiceLine.JI_PartNo = product.OP_PartNum;

			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_NOPInd);
			AssertEquals(1, invoiceLine.AMSLines.Count);

			var invoiceLineNOP = invoiceLine.AMSLines[0];
			AssertEquals("OR1", invoiceLineNOP.US_Program);
			AssertEquals("NOP TEST", invoiceLineNOP.US_CommercialDescription);

			var noplines = invoiceLineNOP.AMSLines;
			AssertEquals(1, noplines.Count);
			AssertEquals(nopLineForProduct.US_IssueDate, noplines[0].US_IssueDate);
			AssertEquals(5m, noplines[0].US_Packages);
			AssertEquals("Product4", noplines[0].US_AuthorizationNumber);
			AssertEquals("KG", noplines[0].US_PackagesUQ);
			AssertEquals("MARKS", noplines[0].US_InspecRemarks);
		}

		public void TestCopyNOPFromProduct_OR2()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "TESTOH";
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "TESTNUM";
			product.OP_Weight = 100m;
			product.OP_WeightUQ = "HG";

			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = importer.PK;
			relOrg.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			var nopTariff = Factory.New<USCTariff>();
			nopTariff.UE_Tariff = "8422401190";
			nopTariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			nopTariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			nopTariff.UE_PGACodes = "AM8AP2";
			nopTariff.UE_OGACodes = "AM8AP2";

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = nopTariff.UE_Tariff;
			pivot.Details.CD_NOPIndicator = OGAIndicatorList.Codes.Declared;
			var productAMSHeader = pivot.AMSLines.AddNew();
			productAMSHeader.US_Program = AMSProgramList.Codes.OR2;
			productAMSHeader.US_IsElecImageSubmitted = true;
			productAMSHeader.US_NetWeight = 12.13m;
			productAMSHeader.US_NetWeightUQ = "KG";
			var productAMSLine = productAMSHeader.AMSLines.AddNew();
			productAMSLine.US_CertType = LPCOTransactionTypeList.Codes.Continuous;
			productAMSLine.US_CertNumber = "123-123-A";
			var productLotCode = productAMSHeader.LotCodes.AddNew();
			productLotCode.CY_Code = LotNumberQualifierList.Codes._3;
			productLotCode.CY_Data = "20240208";
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var amsLine = invoiceLine.AMSLines.AddNew();
			var nopLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = AMSProgramList.Codes.MO1;
			nopLine.US_Program = AMSProgramList.Codes.OR2;
			invoiceLine.JI_PartNo = product.OP_PartNum;

			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_NOPInd);
			AssertEquals(1, invoiceLine.AMSLines.Count);

			var invoiceLineNOP = invoiceLine.AMSLines[0];
			AssertEquals(AMSProgramList.Codes.OR2, invoiceLineNOP.US_Program);
			AssertEquals(true, invoiceLineNOP.US_IsElecImageSubmitted);
			AssertEquals(12.13m, invoiceLineNOP.US_NetWeight);
			AssertEquals("KG", invoiceLineNOP.US_NetWeightUQ);

			var noplines = invoiceLineNOP.AMSLines;
			AssertEquals(1, noplines.Count);
			AssertEquals(LPCOTransactionTypeList.Codes.Continuous, noplines[0].US_CertType);
			AssertEquals("123-123-A", noplines[0].US_CertNumber);

			var lotCodes = invoiceLineNOP.LotCodes;
			AssertEquals(1, lotCodes.Count);
			AssertEquals(LotNumberQualifierList.Codes._3, lotCodes[0].CY_Code);
			AssertEquals("20240208", lotCodes[0].CY_Data);
		}

		public void TestCopyTTBFromProduct()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "INCIMP";
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "INCTEST";

			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = importer.PK;
			relOrg.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = USCTariff.CottonFeeApplicable;
			pivot.CD_TTBIndicator = OGAIndicatorList.Codes.Declared;

			var productTTB = pivot.TTBLines.AddNew();
			productTTB.US_PermitExemptionCode = "PRM";
			productTTB.US_ProgramCode = "BBC";
			productTTB.US_ProcessingCode = "PSS";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "INCTEST";
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_TTBInd);
			AssertEquals(1, invoiceLine.TTBLines.Count);

			var invoiceLineTTB = invoiceLine.TTBLines[0];
			AssertEquals("PRM", invoiceLineTTB.US_PermitExemptionCode);
			AssertEquals("PSS", invoiceLineTTB.US_ProcessingCode);
			AssertEquals("BBC", invoiceLineTTB.US_ProgramCode);
		}

		public void TestCottonCertificateNumberForACSFromProduct()
		{
			var declaration = new DeclarationTestHelper().CreateSimpleImportDeclaration(Factory);

			var importer = Factory.New<OrgHeader>();
			importer.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.Nafta);
			declaration.IOROrgPK = importer.PK;
			declaration.JE_OH_Importer = importer.PK;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;

			declaration.DoMerge();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "ProductForTesting";

			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = importer.PK;
			relOrg.OU_Relationship = "OWN";

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_TariffNum = USCTariff.CottonFeeApplicable;

			var compoment = pivot2.Children.AddNew();
			compoment.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			pivot2.CD_CottonCertificateApply = true;
			pivot2.CD_CottonCertificate = "456";
			invoiceLine.JI_PartNo = product.OP_PartNum;
			AssertEquals("If pivot has Cotton Certificate number we default it to invoice fristly", "456", invoiceLine.US_CottonCertificateNo);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Assert(invoiceLine.LicenceAndPermits.Count > 0);
			AssertEquals("CY_Code for cotton certificate number is ??", "??", invoiceLine.LicenceAndPermits[0].CY_Code);
			AssertEquals("CY_Data should be 456", "456", invoiceLine.LicenceAndPermits[0].CY_Data);
			AssertEquals(string.Empty, invoiceLine.US_CottonCertificateNo);
		}

		public void TestRemoveAndDeleteAll()
		{
			var declaration = new DeclarationTestHelper().CreateSimpleImportDeclaration(Factory);

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "112233";
			importer.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.Nafta);
			declaration.IOROrgPK = importer.PK;
			declaration.JE_OH_Importer = importer.PK;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			declaration.InvoiceLines.RemoveAndDeleteAll();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "ProductForTesting";

			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = importer.PK;
			relOrg.OU_Relationship = "OWN";

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot2.CI_TariffNum = USCTariff.CottonFeeApplicable;

			var compoment = pivot2.Children.AddNew();
			compoment.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			pivot2.CD_CottonCertificateApply = true;
			pivot2.CD_CottonCertificate = "456";
			invoiceLine.JI_PartNo = product.OP_PartNum;
			AssertEquals(2, declaration.InvoiceLines.Count);
			AssertEquals(1, invoiceLine.LicenceAndPermits.Count);

			var licenceAndPermit = invoiceLine.LicenceAndPermits[0];
			AssertNoExceptionThrown(delegate
			{ Factory.Save(); });

			invoiceLine.JI_PartNo = "";
			AssertNoExceptionThrown(delegate
			{ Factory.Save(); });
			AssertEquals(2, declaration.InvoiceLines.Count);
			AssertEquals(0, invoiceLine.LicenceAndPermits.Count);
			AssertEquals(true, licenceAndPermit.IsDeleted);

			invoiceLine.JI_PartNo = product.OP_PartNum;
			AssertEquals(2, declaration.InvoiceLines.Count);
			AssertEquals(1, invoiceLine.LicenceAndPermits.Count);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			invoiceLine.JI_PartNo = "";
			invoiceLine.JI_PartNo = product.OP_PartNum;
			AssertEquals(2, declaration.InvoiceLines.Count);
			AssertEquals(0, invoiceLine.LicenceAndPermits.Count);
			AssertEquals(true, licenceAndPermit.IsDeleted);
		}

		public void TestCottonCertificateNumberForACEFromProduct()
		{
			var declaration = new DeclarationTestHelper().CreateSimpleImportDeclaration(Factory);

			var importer = Factory.New<OrgHeader>();
			importer.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.Nafta);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.IOROrgPK = importer.PK;
			declaration.JE_OH_Importer = importer.PK;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;

			declaration.DoMerge();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "ProductForTesting";

			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = importer.PK;
			relOrg.OU_Relationship = "OWN";

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_TariffNum = USCTariff.CottonFeeApplicable;

			var compoment = pivot2.Children.AddNew();
			compoment.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			pivot2.CD_CottonCertificateApply = true;
			pivot2.CD_CottonCertificate = "456";
			invoiceLine.JI_PartNo = product.OP_PartNum;

			Assert(invoiceLine.LicenceAndPermits.Count > 0);
			AssertEquals("LicenceAndPermits collection CY_Code for cotton certificate number for ACE", "12", invoiceLine.LicenceAndPermits[0].CY_Code);
			AssertEquals("LicenceAndPermits collection CY_Data should be 456 for ACE", "456", invoiceLine.LicenceAndPermits[0].CY_Data);
			AssertEquals(string.Empty, invoiceLine.US_CottonCertificateNo);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			invoiceLine.JI_PartNo = "";
			invoiceLine.JI_PartNo = product.OP_PartNum;
			AssertEquals("pivot has Cotton Certificate number 456 for ACS", "456", invoiceLine.US_CottonCertificateNo);
		}

		public void TestPartPivotType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals("invoiceLine.GetPartPivotType()", ClassificationTypeList.Codes.HTI, invoiceLine.GetPartPivotType());
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.US_TariffType = TariffTypeList.Codes.ScheduleB;
			AssertEquals("invoiceLine.GetPartPivotType()", ClassificationTypeList.Codes.SHB, invoiceLine.GetPartPivotType());
			declaration.US_TariffType = TariffTypeList.Codes.HTS;
			AssertEquals("invoiceLine.GetPartPivotType()", ClassificationTypeList.Codes.HTE, invoiceLine.GetPartPivotType());

			var classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "TestLookup";
			classification.CC_ClassificationType = CusClassification.ClassificationType.EXP;
			invoiceLine.JI_CC = classification.PK;
			AssertEquals("invoiceLine.GetPartPivotType()", ClassificationTypeList.Codes.SHB, invoiceLine.GetPartPivotType());

			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			AssertEquals("invoiceLine.GetPartPivotType()", ClassificationTypeList.Codes.HTE, invoiceLine.GetPartPivotType());

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertEquals("invoiceLine.GetPartPivotType()", ClassificationTypeList.Codes.HTI, invoiceLine.GetPartPivotType());
		}

		public void TestAMSLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = declaration.InvoiceLines.AddNew();

			var ams = invoiceLine.AMSLines.AddNew();
			AssertNotNull(ams);
			invoiceLine.AMSLines.RemoveAndDeleteAll();

			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.AMSLines.AddNew();
			AssertEquals(invoiceLine.AMSLines.Count, 1);
			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Disclaimed;
			AssertEquals(invoiceLine.AMSLines.Count, 1);

			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			AssertEquals(invoiceLine.AMSLines.Count, 1);
			invoiceLine.US_AMSInd = ZString.Empty;
			AssertEquals(invoiceLine.AMSLines.Count, 1);
		}

		public void TestCPSCLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = declaration.InvoiceLines.AddNew();

			var cpsc = invoiceLine.CPSCHeaders.AddNew();
			AssertNotNull(cpsc);
			invoiceLine.CPSCHeaders.RemoveAndDeleteAll();

			invoiceLine.US_CPSCInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.CPSCHeaders.AddNew();
			invoiceLine.CPSCHeaders.AddNew();
			AssertEquals(invoiceLine.CPSCHeaders.Count, 2);
			invoiceLine.US_CPSCInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_CPSCDisclaimReason = "A";
			AssertEquals(invoiceLine.CPSCHeaders.Count, 1);

			invoiceLine.US_CPSCInd = OGAIndicatorList.Codes.Declared;
			AssertEquals("", invoiceLine.US_CPSCDisclaimReason);
			AssertEquals(invoiceLine.CPSCHeaders.Count, 0);
		}

		public void TestTTBDefaulting()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			AssertEquals("invoiceLine.US_TTBIndInfo.ReadOnly", false, invoiceLine.US_TTBIndInfo.ReadOnly);
			AssertEquals("invoiceLine.US_TTBInd", ZString.Empty, invoiceLine.US_TTBInd);
			AssertEquals("invoiceLine.US_TTBDisclaimReasonInfo.ReadOnly", true, invoiceLine.US_TTBDisclaimReasonInfo.ReadOnly);
			AssertEquals("invoiceLine.US_TTBDisclaimReason", ZString.Empty, invoiceLine.US_TTBDisclaimReason);

			invoiceLine.US_TTBDisclaimReason = PGADisclaimReasonList.Codes.A;
			invoiceLine.US_TTBInd = OGAIndicatorList.Codes.Declared;
			AssertEquals("invoiceLine.US_TTBIndInfo.ReadOnly", false, invoiceLine.US_TTBIndInfo.ReadOnly);
			AssertEquals("invoiceLine.US_TTBInd", OGAIndicatorList.Codes.Declared, invoiceLine.US_TTBInd);
			AssertEquals("invoiceLine.US_TTBDisclaimReasonInfo.ReadOnly", true, invoiceLine.US_TTBDisclaimReasonInfo.ReadOnly);
			AssertEquals("invoiceLine.US_TTBDisclaimReason", ZString.Empty, invoiceLine.US_TTBDisclaimReason);

			invoiceLine.US_TTBDisclaimReason = PGADisclaimReasonList.Codes.A;
			invoiceLine.US_TTBInd = OGAIndicatorList.Codes.Disclaimed;
			AssertEquals("invoiceLine.US_TTBIndInfo.ReadOnly", false, invoiceLine.US_TTBIndInfo.ReadOnly);
			AssertEquals("invoiceLine.US_TTBInd", OGAIndicatorList.Codes.Disclaimed, invoiceLine.US_TTBInd);
			AssertEquals("invoiceLine.US_TTBDisclaimReasonInfo.ReadOnly", false, invoiceLine.US_TTBDisclaimReasonInfo.ReadOnly);
			AssertEquals("invoiceLine.US_TTBDisclaimReason", PGADisclaimReasonList.Codes.A, invoiceLine.US_TTBDisclaimReason);

			invoiceLine.US_TTBInd = ZString.Empty;
			AssertEquals("invoiceLine.US_TTBIndInfo.ReadOnly", false, invoiceLine.US_TTBIndInfo.ReadOnly);
			AssertEquals("invoiceLine.US_TTBInd", ZString.Empty, invoiceLine.US_TTBInd);
			AssertEquals("invoiceLine.US_TTBDisclaimReasonInfo.ReadOnly", true, invoiceLine.US_TTBDisclaimReasonInfo.ReadOnly);
			AssertEquals("invoiceLine.US_TTBDisclaimReason", ZString.Empty, invoiceLine.US_TTBDisclaimReason);
		}

		public void TestOMCLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = declaration.InvoiceLines.AddNew();

			var omc = invoiceLine.OMCHeaders.AddNew();
			AssertNotNull(omc);
			invoiceLine.OMCHeaders.RemoveAndDeleteAll();

			invoiceLine.US_OMCInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.OMCHeaders.AddNew();
			AssertEquals(invoiceLine.OMCHeaders.Count, 1);
			invoiceLine.US_OMCInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_OMCDisclaimReason = "A";
			AssertEquals(invoiceLine.OMCHeaders.Count, 1);

			invoiceLine.US_OMCInd = OGAIndicatorList.Codes.Declared;
			AssertEquals("", invoiceLine.US_OMCDisclaimReason);
		}

		public void TestDEAHeaders()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = declaration.InvoiceLines.AddNew();

			var dea = invoiceLine.DEAHeaders.AddNew();
			AssertNotNull(dea);
			invoiceLine.DEAHeaders.RemoveAndDeleteAll();

			invoiceLine.US_DEAInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.DEAHeaders.AddNew();
			AssertEquals(invoiceLine.DEAHeaders.Count, 1);
			invoiceLine.US_DEAInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_DEADisclaimReason = "A";
			AssertEquals(invoiceLine.DEAHeaders.Count, 1);

			invoiceLine.US_DEAInd = OGAIndicatorList.Codes.Declared;
			AssertEquals("", invoiceLine.US_DEADisclaimReason);
		}

		public void TestUSHFCHeaders()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			Assert(!invoiceLine.HasUSHFCHeaders);

			var hfc = invoiceLine.USHFCHeaders.AddNew();
			Assert(invoiceLine.HasUSHFCHeaders);
			AssertNotNull(hfc);
			invoiceLine.USHFCHeaders.RemoveAndDeleteAll();

			invoiceLine.US_HFCInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.USHFCHeaders.AddNew();
			AssertEquals(invoiceLine.USHFCHeaders.Count, 1);
			invoiceLine.US_HFCInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_HFCDisclaimReason = "A";
			AssertEquals(invoiceLine.USHFCHeaders.Count, 1);

			invoiceLine.US_HFCInd = OGAIndicatorList.Codes.Declared;
			AssertEquals("", invoiceLine.US_HFCDisclaimReason);
		}

		public void TestACEPriorNoticeToACECertificationModeWithProduct()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.FillWithValidTestData();
			consignee.OH_Code = "ORGUSCHI";
			consignee.OH_IsConsignee = ZBool.True;
			consignee.OH_RL_NKClosestPort = "USCHI";

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "CHOCOLATE CHEWS";
			part.OP_Weight = 100m;
			part.OP_WeightUQ = "HG";
			part.RelatedOrganisations.AddOwner(consignee);

			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_UsageComment = "U1";
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "1010101010";
			pivot.CD_ACEFDAIndicator = "D";
			pivot.ACEFDAs.AddNew().US_ProductCode = "B";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = consignee.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableSPN = true;

			AssertEquals("declaration.CanHavePGAFDA", true, declaration.CanHavePGAFDA);

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "CHOCOLATE CHEWS";
			AssertEquals("invoiceLine.ACE_FDALines.Count", 1, invoiceLine.ACE_FDALines.Count);

			var pgaFDA = invoiceLine.ACE_FDALines[0];
			pgaFDA.US_ProgramCode = "ABC";

			declaration.US_EnableENS = true;
			Assert("PReCondition:System should not delete all existing ACE FDAs and readd them again from product", declaration.CanHavePGAFDA);
			AssertEquals("invoiceLine.ACE_FDALines.Count", 1, invoiceLine.ACE_FDALines.Count);

			AssertEquals("B", invoiceLine.ACE_FDALines[0].US_ProductCode);
			Assert(!pgaFDA.IsDeleted);
			AssertEquals("ABC", invoiceLine.ACE_FDALines[0].US_ProgramCode);
		}

		public void TestTTBProperties()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;
			tariff.UE_PGACodes = "TB2";
			invoiceLine.US_TTBInd = ZString.Empty;
			invoiceLine.US_TTBDisclaimReason = "A";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			AssertEquals("invoiceLine.US_TTBDisclaimReason", ZString.Empty, invoiceLine.US_TTBDisclaimReason);
			AssertEquals("invoiceLine.US_TTBInd", OGAIndicatorList.Codes.Declared, invoiceLine.US_TTBInd);
			AssertEquals("invoiceLine.US_TTBReqDesc", OGARequirementList.Descriptions.TB2 + " (TB2)", invoiceLine.US_TTBReqDesc);
		}

		public void TestAPHISDefaulting()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			AssertEquals("invoiceLine.US_APHISIndInfo.ReadOnly", false, invoiceLine.US_APHISIndInfo.ReadOnly);
			AssertEquals("invoiceLine.US_APHISInd", ZString.Empty, invoiceLine.US_APHISInd);
			AssertEquals("invoiceLine.US_APHISDisclaimReasonInfo.ReadOnly", true, invoiceLine.US_APHISDisclaimReasonInfo.ReadOnly);
			AssertEquals("invoiceLine.US_APHISDisclaimReason", ZString.Empty, invoiceLine.US_APHISDisclaimReason);

			invoiceLine.US_APHISDisclaimReason = PGADisclaimReasonList.Codes.A;
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Declared;
			AssertEquals("invoiceLine.US_APHISIndInfo.ReadOnly", false, invoiceLine.US_APHISIndInfo.ReadOnly);
			AssertEquals("invoiceLine.US_APHISInd", OGAIndicatorList.Codes.Declared, invoiceLine.US_APHISInd);
			AssertEquals("invoiceLine.US_APHISDisclaimReasonInfo.ReadOnly", true, invoiceLine.US_APHISDisclaimReasonInfo.ReadOnly);
			AssertEquals("invoiceLine.US_APHISDisclaimReason", ZString.Empty, invoiceLine.US_APHISDisclaimReason);

			invoiceLine.US_APHISDisclaimReason = PGADisclaimReasonList.Codes.A;
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Disclaimed;
			AssertEquals("invoiceLine.US_APHISIndInfo.ReadOnly", false, invoiceLine.US_APHISIndInfo.ReadOnly);
			AssertEquals("invoiceLine.US_APHISInd", OGAIndicatorList.Codes.Disclaimed, invoiceLine.US_APHISInd);
			AssertEquals("invoiceLine.US_APHISDisclaimReasonInfo.ReadOnly", false, invoiceLine.US_APHISDisclaimReasonInfo.ReadOnly);
			AssertEquals("invoiceLine.US_APHISDisclaimReason", PGADisclaimReasonList.Codes.A, invoiceLine.US_APHISDisclaimReason);

			invoiceLine.US_APHISInd = ZString.Empty;
			AssertEquals("invoiceLine.US_APHISIndInfo.ReadOnly", false, invoiceLine.US_APHISIndInfo.ReadOnly);
			AssertEquals("invoiceLine.US_APHISInd", ZString.Empty, invoiceLine.US_APHISInd);
			AssertEquals("invoiceLine.US_APHISDisclaimReasonInfo.ReadOnly", true, invoiceLine.US_APHISDisclaimReasonInfo.ReadOnly);
			AssertEquals("invoiceLine.US_APHISDisclaimReason", ZString.Empty, invoiceLine.US_APHISDisclaimReason);
		}

		public void TestASNInvoiceAndCalculationOf9802PriceWhenInvoiceIsAttachedToDeclaration()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.FillWithValidTestData();
			consignee.OH_Code = "ORGUSCHI";
			consignee.OH_IsConsignee = ZBool.True;
			consignee.OH_RL_NKClosestPort = "USCHI";

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PART1";
			part.OP_Weight = 100m;
			part.OP_WeightUQ = "HG";
			part.RelatedOrganisations.AddOwner(consignee);
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_UsageComment = "U1";
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "1010101010";
			pivot.CI_SupplementalTariff = "9802005060";
			pivot.CD_9802ValuePerUnit = 0.65m;
			pivot.CD_RX_NK9802ValuePerUnitCurr = "USD";
			pivot.CD_PerUnitCost = 10m;
			pivot.CD_RX_NKPerUnitCostCurr = "USD";
			pivot.CD_AMMVPerUnit = 0.2m;

			var invoice = Factory.New<JobComInvoiceHeader>();
			new FakeDeclarationCreatorForInvoice(invoice);
			invoice.JZ_OH_Buyer = consignee.PK;
			invoice.JZ_StandAloneInvoiceDirection = JobMessageTypeList.MoreCodes.AdvanceShippingNotice;
			invoice.JZ_MessageType = JobMessageTypeList.MoreCodes.AdvanceShippingNotice;
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "PART1";
			invoiceLine.JI_InvoiceQuantity = 100m;
			invoiceLine.JI_InvoiceUQ = "PCE";
			Factory.Save();

			var invoiceLoaded = new BusinessObjectFactory().Load<JobComInvoiceHeader>(invoice.PK);

			var declaration = invoiceLoaded.Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = consignee.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			invoiceLoaded.RefreshDefaultsWhenInvoiceAttachedToDeclarationRequired = true;
			declaration.Invoices.Add(invoiceLoaded);

			var invoiceLineLoaded = invoiceLoaded.InvoiceLines[0];
			AssertEquals(0.65m, invoiceLineLoaded.US_98InvCurrPerUnit);
			AssertEquals(65m, invoiceLineLoaded.US_98ValueInvCurr);
			AssertEquals(1000m, invoiceLineLoaded.JI_LinePrice);
			AssertEquals(20m, invoiceLineLoaded.ApportionedCharges[0].J7_Amount);
		}

		public void TestFWSDefaulting()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			AssertEquals("invoiceLine.US_FWSIndInfo.ReadOnly", false, invoiceLine.US_FWSIndInfo.ReadOnly);
			AssertEquals("invoiceLine.US_FWSInd", ZString.Empty, invoiceLine.US_FWSInd);
			AssertEquals("invoiceLine.US_FWSDisclaimReasonInfo.ReadOnly", true, invoiceLine.US_FWSDisclaimReasonInfo.ReadOnly);
			AssertEquals("invoiceLine.US_FWSDisclaimReason", ZString.Empty, invoiceLine.US_FWSDisclaimReason);

			invoiceLine.US_FWSDisclaimReason = PGADisclaimReasonList.Codes.A;
			invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Declared;
			AssertEquals("invoiceLine.US_FWSIndInfo.ReadOnly", false, invoiceLine.US_FWSIndInfo.ReadOnly);
			AssertEquals("invoiceLine.US_FWSInd", OGAIndicatorList.Codes.Declared, invoiceLine.US_FWSInd);
			AssertEquals("invoiceLine.US_FWSDisclaimReasonInfo.ReadOnly", true, invoiceLine.US_FWSDisclaimReasonInfo.ReadOnly);
			AssertEquals("invoiceLine.US_FWSDisclaimReason", ZString.Empty, invoiceLine.US_FWSDisclaimReason);

			invoiceLine.US_FWSDisclaimReason = PGADisclaimReasonList.Codes.A;
			invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Disclaimed;
			AssertEquals("invoiceLine.US_FWSIndInfo.ReadOnly", false, invoiceLine.US_FWSIndInfo.ReadOnly);
			AssertEquals("invoiceLine.US_FWSInd", OGAIndicatorList.Codes.Disclaimed, invoiceLine.US_FWSInd);
			AssertEquals("invoiceLine.US_FWSDisclaimReasonInfo.ReadOnly", false, invoiceLine.US_FWSDisclaimReasonInfo.ReadOnly);
			AssertEquals("invoiceLine.US_FWSDisclaimReason", PGADisclaimReasonList.Codes.A, invoiceLine.US_FWSDisclaimReason);

			invoiceLine.US_FWSInd = ZString.Empty;
			AssertEquals("invoiceLine.US_FWSIndInfo.ReadOnly", false, invoiceLine.US_FWSIndInfo.ReadOnly);
			AssertEquals("invoiceLine.US_FWSInd", ZString.Empty, invoiceLine.US_FWSInd);
			AssertEquals("invoiceLine.US_FWSDisclaimReasonInfo.ReadOnly", true, invoiceLine.US_FWSDisclaimReasonInfo.ReadOnly);
			AssertEquals("invoiceLine.US_FWSDisclaimReason", ZString.Empty, invoiceLine.US_FWSDisclaimReason);
		}

		public void TestHFCDefaulting()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			AssertEquals("invoiceLine.US_HFCIndInfo.ReadOnly", false, invoiceLine.US_HFCIndInfo.ReadOnly);
			AssertEquals("invoiceLine.US_HFCInd", ZString.Empty, invoiceLine.US_HFCInd);
			AssertEquals("invoiceLine.US_HFCDisclaimReasonInfo.ReadOnly", true, invoiceLine.US_HFCDisclaimReasonInfo.ReadOnly);
			AssertEquals("invoiceLine.US_HFCDisclaimReason", ZString.Empty, invoiceLine.US_HFCDisclaimReason);

			invoiceLine.US_HFCDisclaimReason = PGADisclaimReasonList.Codes.A;
			invoiceLine.US_HFCInd = OGAIndicatorList.Codes.Declared;
			AssertEquals("invoiceLine.US_HFCIndInfo.ReadOnly", false, invoiceLine.US_HFCIndInfo.ReadOnly);
			AssertEquals("invoiceLine.US_HFCInd", OGAIndicatorList.Codes.Declared, invoiceLine.US_HFCInd);
			AssertEquals("invoiceLine.US_HFCDisclaimReasonInfo.ReadOnly", true, invoiceLine.US_HFCDisclaimReasonInfo.ReadOnly);
			AssertEquals("invoiceLine.US_HFCDisclaimReason", ZString.Empty, invoiceLine.US_HFCDisclaimReason);

			invoiceLine.US_HFCDisclaimReason = PGADisclaimReasonList.Codes.A;
			invoiceLine.US_HFCInd = OGAIndicatorList.Codes.Disclaimed;
			AssertEquals("invoiceLine.US_HFCIndInfo.ReadOnly", false, invoiceLine.US_HFCIndInfo.ReadOnly);
			AssertEquals("invoiceLine.US_HFCInd", OGAIndicatorList.Codes.Disclaimed, invoiceLine.US_HFCInd);
			AssertEquals("invoiceLine.US_HFCDisclaimReasonInfo.ReadOnly", false, invoiceLine.US_HFCDisclaimReasonInfo.ReadOnly);
			AssertEquals("invoiceLine.US_HFCDisclaimReason", PGADisclaimReasonList.Codes.A, invoiceLine.US_HFCDisclaimReason);

			invoiceLine.US_HFCInd = ZString.Empty;
			AssertEquals("invoiceLine.US_HFCIndInfo.ReadOnly", false, invoiceLine.US_HFCIndInfo.ReadOnly);
			AssertEquals("invoiceLine.US_HFCInd", ZString.Empty, invoiceLine.US_HFCInd);
			AssertEquals("invoiceLine.US_HFCDisclaimReasonInfo.ReadOnly", true, invoiceLine.US_HFCDisclaimReasonInfo.ReadOnly);
			AssertEquals("invoiceLine.US_HFCDisclaimReason", ZString.Empty, invoiceLine.US_HFCDisclaimReason);
		}

		public void TestNMFSProperties()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0011223344";
			tariff.UE_PGACodes = "NM2NM4NM6NM8";
			tariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "0000000001";
			tariff2.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff2.UE_DateTo = ZDateTime.Today.AddYears(1);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_NMFS370Ind = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_NMFS370DisclaimReason = PGADisclaimReasonList.Codes.A;
			invoiceLine.US_NMFSAMRInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_NMFSAMRDisclaimReason = PGADisclaimReasonList.Codes.A;
			invoiceLine.US_NMFSHMSInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_NMFSHMSDisclaimReason = PGADisclaimReasonList.Codes.A;
			invoiceLine.JI_Tariff = tariff2.UE_Tariff;
			AssertEquals("US_NMFS370Ind", OGAIndicatorList.Codes.Disclaimed, invoiceLine.US_NMFS370Ind);
			AssertEquals("US_NMFS370DisclaimReason", PGADisclaimReasonList.Codes.A, invoiceLine.US_NMFS370DisclaimReason);
			AssertEquals("US_NMFSAMRInd", OGAIndicatorList.Codes.Disclaimed, invoiceLine.US_NMFSAMRInd);
			AssertEquals("US_NMFSAMRDisclaimReason", PGADisclaimReasonList.Codes.A, invoiceLine.US_NMFSAMRDisclaimReason);
			AssertEquals("US_NMFSHMSInd", OGAIndicatorList.Codes.Disclaimed, invoiceLine.US_NMFSHMSInd);
			AssertEquals("US_NMFSHMSDisclaimReason", PGADisclaimReasonList.Codes.A, invoiceLine.US_NMFSHMSDisclaimReason);

			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			AssertEquals("US_NMFS370Ind", OGAIndicatorList.Codes.Declared, invoiceLine.US_NMFS370Ind);
			AssertEquals("US_NMFS370DisclaimReason", ZString.Empty, invoiceLine.US_NMFS370DisclaimReason);
			AssertEquals("US_NMFSAMRInd", OGAIndicatorList.Codes.Declared, invoiceLine.US_NMFSAMRInd);
			AssertEquals("US_NMFSAMRDisclaimReason", ZString.Empty, invoiceLine.US_NMFSAMRDisclaimReason);
			AssertEquals("US_NMFSHMSInd", OGAIndicatorList.Codes.Declared, invoiceLine.US_NMFSHMSInd);
			AssertEquals("US_NMFSHMSDisclaimReason", ZString.Empty, invoiceLine.US_NMFSHMSDisclaimReason);

			AssertEquals("US_NMFSSIMPInd", OGAIndicatorList.Codes.Declared, invoiceLine.US_NMFSSIMPInd);

			invoiceLine.US_NMFS370Ind = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_NMFS370DisclaimReason = PGADisclaimReasonList.Codes.A;
			AssertEquals("US_NMFS370DisclaimReason", PGADisclaimReasonList.Codes.A, invoiceLine.US_NMFS370DisclaimReason);
			invoiceLine.US_NMFS370Ind = OGAIndicatorList.Codes.Declared;
			AssertEquals("US_NMFS370DisclaimReason", ZString.Empty, invoiceLine.US_NMFS370DisclaimReason);
			invoiceLine.US_NMFSAMRInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_NMFSAMRDisclaimReason = PGADisclaimReasonList.Codes.A;
			AssertEquals("US_NMFSAMRDisclaimReason", PGADisclaimReasonList.Codes.A, invoiceLine.US_NMFSAMRDisclaimReason);
			invoiceLine.US_NMFSAMRInd = OGAIndicatorList.Codes.Declared;
			AssertEquals("US_NMFSAMRDisclaimReason", ZString.Empty, invoiceLine.US_NMFSAMRDisclaimReason);
			invoiceLine.US_NMFSHMSInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_NMFSHMSDisclaimReason = PGADisclaimReasonList.Codes.A;
			AssertEquals("US_NMFSHMSDisclaimReason", PGADisclaimReasonList.Codes.A, invoiceLine.US_NMFSHMSDisclaimReason);
			invoiceLine.US_NMFSHMSInd = OGAIndicatorList.Codes.Declared;
			AssertEquals("US_NMFSHMSDisclaimReason", ZString.Empty, invoiceLine.US_NMFSHMSDisclaimReason);
		}

		public void TestSetDefaultValueToCottonCertificateNo()
		{
			var declaration = new DeclarationTestHelper().CreateSimpleImportDeclaration(Factory);

			var importer = Factory.New<OrgHeader>();
			var nafDoc = importer.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.Nafta);
			declaration.IOROrgPK = importer.PK;
			declaration.JE_OH_Importer = importer.PK;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;

			declaration.DoMerge();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "ProductForTesting";

			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = importer.PK;
			relOrg.OU_Relationship = "OWN";

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_TariffNum = USCTariff.CottonFeeApplicable;

			var compoment = pivot2.Children.AddNew();
			compoment.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			pivot2.CD_CottonCertificateApply = true;
			pivot2.CD_CottonCertificate = "456";
			invoiceLine.JI_PartNo = product.OP_PartNum;
			AssertEquals("If pivot has Cotton Certificate number we default it to invoice fristly", "456", invoiceLine.US_CottonCertificateNo);

			pivot2.CD_CottonCertificate = string.Empty;
			invoiceLine.JI_PartNo = string.Empty;
			var partHasRequiredDocuments = (IHaveRequiredDocuments)product;
			var cottonDocument = partHasRequiredDocuments.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.Cotton);
			cottonDocument.EQ_ValidToDate = ZDateTime.Now.AddDays(-10);
			cottonDocument.EQ_DocNumber = "123";

			invoiceLine.JI_PartNo = product.OP_PartNum;
			AssertEquals("The cotton certificate is invalid currently", string.Empty, invoiceLine.US_CottonCertificateNo);

			invoiceLine.JI_PartNo = "";
			cottonDocument.EQ_ValidToDate = ZDateTime.Now.AddDays(15);
			invoiceLine.JI_PartNo = product.OP_PartNum;
			AssertEquals("Default cotton certificate document number to invoice", "123", invoiceLine.US_CottonCertificateNo);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			invoiceLine.LicenceAndPermits.RemoveAndDeleteAll();
			invoiceLine.JI_PartNo = "";
			invoiceLine.JI_PartNo = product.OP_PartNum;
			AssertEquals(1, invoiceLine.LicenceAndPermits.Count);
			var licenceAndPermit = invoiceLine.LicenceAndPermits[0];
			AssertEquals(LicencePermitTypeList.Codes._22, licenceAndPermit.CY_Code);
			AssertEquals("123", licenceAndPermit.CY_Data);
			AssertEquals(string.Empty, invoiceLine.US_CottonCertificateNo);
		}

		public void TestFDAIndicatorDefaulting()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0407000020";
			tariff.UE_DateFrom = ZDateTime.Today;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(30);
			tariff.UE_OGACodes = "";
			tariff.UE_PGACodes = "FD2";

			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "0407000021";
			tariff2.UE_DateFrom = ZDateTime.Today;
			tariff2.UE_DateTo = ZDateTime.Today.AddDays(30);
			tariff2.UE_OGACodes = "FD2";
			tariff2.UE_PGACodes = "";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0407000020";
			AssertEquals("D", invoiceLine.US_FDAIndicator);
			Assert(invoiceLine.US_FDADisclaimReasonInfo.ReadOnly);

			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			AssertEquals("D", invoiceLine.US_FDAIndicator);

			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			AssertEquals("D", invoiceLine.US_FDAIndicator);

			invoiceLine.JI_Tariff = "0407000021";
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			declaration.US_DomesticCargo = true;
			Factory.Save();
			AssertEquals(ZString.Empty, invoiceLine.US_FDAIndicator);

			var tariff3 = Factory.New<USCTariff>();
			tariff3.UE_Tariff = "0407000028";
			tariff3.UE_DateFrom = ZDateTime.Today.AddDays(-2);
			tariff3.UE_DateTo = ZDateTime.Today.AddDays(30);
			tariff3.UE_PGACodes = "FD3";
			Factory.Save();

			declaration.US_SchDArrival = "3901";
			invoiceLine.JI_Tariff = tariff3.UE_Tariff;
			invoiceLine.ACE_FDALines.AddNew();

			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			Assert(!invoiceLine.US_FDADisclaimReasonInfo.ReadOnly);
		}

		public void TestDefaultTaxApplicabilityNWhenPREntryPort()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "88998899";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;

			USCTariffDutyRate dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Wines;
			dutyRate.UD_TaxFeeFlag = "1";

			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "4925", "Test PR Port", startDate, endDate);
			var attributeNameState = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.State, "Desc", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.UnitedStates);
			var attributeState = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeNameState.ZXE_Name, USStateList.Codes.PuertoRico);
			newFactory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_SchDEntry = "4925";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "88998899";
			AssertEquals(TaxApplyList.Codes.No, invoiceLine.US_TaxApply);
			invoiceLine.JI_Description = "Goods";

			AssertEquals("Should default 'N'", "N", invoiceLine.US_TaxApply);

			invoiceLine.US_TaxApply = TaxApplyList.Codes.Yes;
			AssertHasMessageError("Entry Port is in PR", invoiceLine.US_TaxApplyInfo, FormalImportAddInfoJobComInvoiceLineValidation.PortIsExceptFromIRTaxes);

			invoiceLine.JI_Tariff = "";
			declaration.US_SchDEntry = "3901";
			invoiceLine.JI_Tariff = "88998899";
			AssertNoMessageError("Entry Port is in PR", invoiceLine.US_TaxApplyInfo, FormalImportAddInfoJobComInvoiceLineValidation.PortIsExceptFromIRTaxes);

			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "11223344";
			tariff1.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff1.UE_DateTo = ZDateTime.MaxSmallDateTime;

			var dutyRate1 = tariff1.DutyRates.AddNew();
			dutyRate1.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.DistilledSpirits;
			dutyRate1.UD_TaxFeeFlag = "1";
			dutyRate1.UD_TaxFeeComputationCode = ComputationCodeList.Codes.NoComputationFormulaAvailable;

			declaration.US_SchDEntry = "4925";
			invoiceLine.US_TaxApply = ZString.Empty;
			invoiceLine.JI_Tariff = "11223344";
			AssertEquals(TaxApplyList.Codes.No, invoiceLine.US_TaxApply);
		}

		public void TestDOTIndicatorDefaulting()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0407000020";
			tariff.UE_DateFrom = ZDateTime.Today;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(30);
			tariff.UE_OGACodes = "DT2";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0407000020";
			AssertEquals("DOT indicator should not be defaulted for jobs certified in ACE", "", invoiceLine.US_DOTIndicator);
			AssertNoMessageErrors(invoiceLine.JI_TariffInfo);

			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			AssertEquals("D", invoiceLine.US_DOTIndicator);
		}

		public void TestDoesMatchCertificationMode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			Assert("FDA is declarable in ACE/ACE job", invoiceLine.DoesMatchCertificationMode(GovernmentAgencyProgramCodeList.Codes.FDA));
			Assert("DOT is NOT declarable in ACE/ACE job", !invoiceLine.DoesMatchCertificationMode(GovernmentAgencyProgramCodeList.Codes.DOT));
			Assert("FCC is NOT declarable in ACE/ACE job", !invoiceLine.DoesMatchCertificationMode(GovernmentAgencyProgramCodeList.Codes.FCC));
			Assert("Lacey is declarable in ACE/ACE job", invoiceLine.DoesMatchCertificationMode(GovernmentAgencyProgramCodeList.Codes.Lacey));

			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			Assert("FDA is declarable in ACE/ACS job", invoiceLine.DoesMatchCertificationMode(GovernmentAgencyProgramCodeList.Codes.FDA));
			Assert("DOT is declarable in ACE/ACS job", invoiceLine.DoesMatchCertificationMode(GovernmentAgencyProgramCodeList.Codes.DOT));
			Assert("FCC is declarable in ACE/ACS job", invoiceLine.DoesMatchCertificationMode(GovernmentAgencyProgramCodeList.Codes.FCC));
			Assert("Lacey is NOT declarable in ACE/ACS job", !invoiceLine.DoesMatchCertificationMode(GovernmentAgencyProgramCodeList.Codes.Lacey));

			declaration.US_CargoReleaseType = ZString.Empty;
			Assert("FDA is declarable in ACE job when Cargo Release type is empty", invoiceLine.DoesMatchCertificationMode(GovernmentAgencyProgramCodeList.Codes.FDA));
			Assert("DOT is NOT declarable in ACE job when Cargo Release type is empty", !invoiceLine.DoesMatchCertificationMode(GovernmentAgencyProgramCodeList.Codes.DOT));
			Assert("FCC is NOT declarable in ACE job when Cargo Release type is empty", !invoiceLine.DoesMatchCertificationMode(GovernmentAgencyProgramCodeList.Codes.FCC));
			Assert("Lacey is declarable in ACE job when Cargo Release type is empty", invoiceLine.DoesMatchCertificationMode(GovernmentAgencyProgramCodeList.Codes.Lacey));

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			Assert("FDA is declarable in ACS job", invoiceLine.DoesMatchCertificationMode(GovernmentAgencyProgramCodeList.Codes.FDA));
			Assert("DOT is declarable in ACS job", invoiceLine.DoesMatchCertificationMode(GovernmentAgencyProgramCodeList.Codes.DOT));
			Assert("FCC is declarable in ACS job", invoiceLine.DoesMatchCertificationMode(GovernmentAgencyProgramCodeList.Codes.FCC));
			Assert("Lacey is declarable in ACS job", invoiceLine.DoesMatchCertificationMode(GovernmentAgencyProgramCodeList.Codes.Lacey));

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = false;
			declaration.US_EnableSPN = true;
			Assert("FDA is declarable if standalone PN without ENS and CRL", invoiceLine.DoesMatchCertificationMode(GovernmentAgencyProgramCodeList.Codes.FDA));
		}

		public void TestICustomsBrokerDetailsMembers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			ICustomsBrokerDetails brokerDetails = invoiceLine;
			AssertEquals(declaration.BranchIAddressDetails, brokerDetails.Address);
			AssertEquals("", brokerDetails.ContactName);
			AssertEquals("", brokerDetails.ContactPhone);
			AssertEquals("", brokerDetails.ContactEmail);

			invoice.US_FDAContactName = "BOB";
			invoice.US_FDAContactPhoneNo = "1020304050";
			invoice.US_FDAContactEmail = "bob@where.com";
			AssertEquals("BOB", brokerDetails.ContactName);
			AssertEquals("1020304050", brokerDetails.ContactPhone);
			AssertEquals("bob@where.com", brokerDetails.ContactEmail);
		}

		[TestDate(2008, 9, 11)]
		public void TestHasCottonCertificate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6104220040";
			invoiceLine.LicenceAndPermits.AddNew(LicencePermitTypeList.Codes._12, "007894812");
			Assert(invoiceLine.HasCottonCertificate);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			invoiceLine.US_CottonCertificateNo = ZString.Empty;
			Assert(!invoiceLine.HasCottonCertificate);
		}

		public void TestHasInvoiceLinesWithPGAIndicators()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_PSTDisclaimProgram = "PST";
			Assert(!invoice.HasInvoiceLinesWithPST);
			Assert(!declaration.PGAFlags.HasInvoiceLinesWithPST);
			AssertEquals("PST", invoiceLine.US_PSTDisclaimProgram);

			invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Declared;
			AssertEquals(ZString.Empty, invoiceLine.US_PSTDisclaimProgram);
			Assert(invoice.HasInvoiceLinesWithPST);
			Assert(declaration.PGAFlags.HasInvoiceLinesWithPST);

			invoiceLine.Delete();
			Assert(!invoice.HasInvoiceLinesWithPST);
			Assert(!declaration.PGAFlags.HasInvoiceLinesWithPST);

			invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Disclaimed;
			Assert(!invoice.HasInvoiceLinesWithVNE);
			Assert(!declaration.PGAFlags.HasInvoiceLinesWithVNE);

			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Declared;
			Assert(invoice.HasInvoiceLinesWithVNE);
			Assert(declaration.PGAFlags.HasInvoiceLinesWithVNE);

			invoiceLine.Delete();
			Assert(!invoice.HasInvoiceLinesWithVNE);
			Assert(!declaration.PGAFlags.HasInvoiceLinesWithVNE);

			invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_ODSInd = OGAIndicatorList.Codes.Disclaimed;
			Assert(!invoice.HasInvoiceLinesWithODS);
			Assert(!declaration.PGAFlags.HasInvoiceLinesWithODS);

			invoiceLine.US_ODSInd = OGAIndicatorList.Codes.Declared;
			Assert(invoice.HasInvoiceLinesWithODS);
			Assert(declaration.PGAFlags.HasInvoiceLinesWithODS);

			invoiceLine.Delete();
			Assert(!invoice.HasInvoiceLinesWithODS);
			Assert(!declaration.PGAFlags.HasInvoiceLinesWithODS);

			invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_FSISInd = OGAIndicatorList.Codes.Disclaimed;
			Assert(!invoice.HasInvoiceLinesWithFSIS);
			Assert(!declaration.PGAFlags.HasInvoiceLinesWithFSIS);

			invoiceLine.US_FSISInd = OGAIndicatorList.Codes.Declared;
			Assert(invoice.HasInvoiceLinesWithFSIS);
			Assert(declaration.PGAFlags.HasInvoiceLinesWithFSIS);

			invoiceLine.Delete();
			Assert(!invoice.HasInvoiceLinesWithFSIS);
			Assert(!declaration.PGAFlags.HasInvoiceLinesWithFSIS);

			invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_OMCInd = OGAIndicatorList.Codes.Disclaimed;
			Assert(!invoice.HasInvoiceLinesWithOMC);
			Assert(!declaration.PGAFlags.HasInvoiceLinesWithOMC);

			invoiceLine.US_OMCInd = OGAIndicatorList.Codes.Declared;
			Assert(invoice.HasInvoiceLinesWithOMC);
			Assert(declaration.PGAFlags.HasInvoiceLinesWithOMC);

			invoiceLine.Delete();
			Assert(!invoice.HasInvoiceLinesWithOMC);
			Assert(!declaration.PGAFlags.HasInvoiceLinesWithOMC);

			invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_ATFInd = OGAIndicatorList.Codes.Disclaimed;
			Assert(!invoice.HasInvoiceLinesWithATF);
			Assert(!declaration.PGAFlags.HasInvoiceLinesWithATF);

			invoiceLine.US_ATFInd = OGAIndicatorList.Codes.Declared;
			Assert(invoice.HasInvoiceLinesWithATF);
			Assert(declaration.PGAFlags.HasInvoiceLinesWithATF);

			invoiceLine.Delete();
			Assert(!invoice.HasInvoiceLinesWithATF);
			Assert(!declaration.PGAFlags.HasInvoiceLinesWithATF);

			invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_CPSCInd = OGAIndicatorList.Codes.Disclaimed;
			Assert(!invoice.HasInvoiceLinesWithCPSC);
			Assert(!declaration.PGAFlags.HasInvoiceLinesWithCPSC);

			invoiceLine.US_CPSCInd = OGAIndicatorList.Codes.Declared;
			Assert(invoice.HasInvoiceLinesWithCPSC);
			Assert(declaration.PGAFlags.HasInvoiceLinesWithCPSC);

			invoiceLine.Delete();
			Assert(!invoice.HasInvoiceLinesWithCPSC);
			Assert(!declaration.PGAFlags.HasInvoiceLinesWithCPSC);

			invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_DEAInd = OGAIndicatorList.Codes.Disclaimed;
			Assert(!invoice.HasInvoiceLinesWithDEA);
			Assert(!declaration.PGAFlags.HasInvoiceLinesWithDEA);

			invoiceLine.US_DEAInd = OGAIndicatorList.Codes.Declared;
			Assert(invoice.HasInvoiceLinesWithDEA);
			Assert(declaration.PGAFlags.HasInvoiceLinesWithDEA);

			invoiceLine.Delete();
			Assert(!invoice.HasInvoiceLinesWithDEA);
			Assert(!declaration.PGAFlags.HasInvoiceLinesWithDEA);

			invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_HFCInd = OGAIndicatorList.Codes.Disclaimed;
			Assert(!invoice.HasInvoiceLinesWithHFC);
			Assert(!declaration.PGAFlags.HasInvoiceLinesWithHFC);

			invoiceLine.US_HFCInd = OGAIndicatorList.Codes.Declared;
			Assert(invoice.HasInvoiceLinesWithHFC);
			Assert(declaration.PGAFlags.HasInvoiceLinesWithHFC);
			Assert(declaration.PGAFlags.HasExclusivePGAData);

			invoiceLine.Delete();
			Assert(!invoice.HasInvoiceLinesWithHFC);
			Assert(!declaration.PGAFlags.HasInvoiceLinesWithHFC);
			Assert(!declaration.PGAFlags.HasExclusivePGAData);
		}

		public void TestIsACEFDARelevant()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			Assert(invoiceLine.IsACEFDARelevant);
			Assert(declaration.IsACEFDARelevant);

			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			Assert(!invoiceLine.IsACEFDARelevant);
			Assert(!declaration.IsACEFDARelevant);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			Assert(!invoiceLine.IsACEFDARelevant);
			Assert(!declaration.IsACEFDARelevant);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			Assert(invoiceLine.IsACEFDARelevant);
			Assert(declaration.IsACEFDARelevant);

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableSPN = true;
			Assert("StandAlone PN", declaration.IsACEFDARelevant);
		}

		public void TestGetDefultOrgSupplierPart()
		{
			USCTariff tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "00000002";
			tariff1.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff1.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff1.UE_Unit1 = "T";

			USCTariff tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "00000001";
			tariff2.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff2.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff2.UE_Unit1 = "KG";
			Factory.Save();

			var declarationSupplier = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignor, true));
			var declarationImporter = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignee, true));
			CusClassification classification = Factory.New<CusClassification>();
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			classification.CC_LookupCode = "Z1Z!";
			classification.CC_TariffNum = "30000000";

			var product1 = Factory.New<OrgSupplierPart>();
			product1.OP_PartNum = "Test Product3";
			product1.OP_Weight = 100;
			product1.OP_WeightUQ = Core.Constants.Weight.MetricCarat;
			product1.OP_NetWeight = 9;
			OrgPartRelation orgRel = product1.RelatedOrganisations.AddOrganisationIfNotExist(declarationImporter.PK, OrgPartRelation.RelationshipTypes.Owner);
			product1.RelatedOrganisations.AddOrganisationIfNotExist(declarationSupplier.PK, OrgPartRelation.RelationshipTypes.Supplier);

			var pivot1 = product1.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot1.CI_TariffNum = "00000001";
			pivot1.CI_OH = orgRel.OU_OH;
			pivot1.CI_CC = classification.PK;
			pivot1.CI_DateStart = ZDateTime.Now.AddDays(-20);
			pivot1.CI_DateEnd = ZDateTime.Now.AddDays(10);

			var pivot2 = product1.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot2.CI_TariffNum = "00000002";
			pivot2.CI_OH = orgRel.OU_OH;
			pivot2.CI_CC = classification.PK;
			pivot2.CI_DateStart = ZDateTime.Now.AddDays(-10);
			pivot2.CI_DateEnd = ZDateTime.Now.AddDays(11);

			var pgaOnProduct = pivot2.PGAs.AddNew();
			pgaOnProduct.US_PGACommercialDescription = "This is a test C";
			var pgaOnProduct2 = pivot2.PGAs.AddNew();
			pgaOnProduct2.US_PGACommercialDescription = "This is a test A";
			var pgaOnProduct3 = pivot2.PGAs.AddNew();
			pgaOnProduct3.US_PGACommercialDescription = "This is a test B";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_DeclarationReference = "J00001";
			declaration.JE_OH_Importer = declarationImporter.PK;
			declaration.JE_OH_Supplier = declarationSupplier.PK;
			declaration.JE_MessageType = "IMP";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Buyer = declarationImporter.PK;
			invoice.JZ_OH_Supplier = declarationSupplier.PK;
			Factory.Save();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = product1.OP_PartNum;
			Factory.Save();

			OrgSupplierPart partTest = invoiceLine.Part;
			AssertNotNull(partTest);
			AssertEquals(partTest.PK, product1.PK);
			AssertEquals(partTest.OP_PartNum.ToUpper(), product1.OP_PartNum.ToUpper());
			AssertEquals(invoiceLine.JI_Tariff, tariff1.UE_Tariff);
			AssertEquals(invoiceLine.CustomsUQ, tariff1.UE_Unit1);
			AssertEquals(invoiceLine.JI_CC, classification.PK);

			AssertEquals(invoiceLine.FDAs.Count, 0);

			AssertEquals(invoiceLine.DOTs.Count, 0);

			AssertEquals(invoiceLine.LaceyActLines.Count, 3);
			AssertEquals(ZString.Empty, invoiceLine.US_LaceyIndicator);
		}

		public void TestUpdateFDAMsgStatusWhenProductcopied()
		{
			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "00000002";
			tariff1.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff1.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff1.UE_Unit1 = "T";

			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "00000001";
			tariff2.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff2.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff2.UE_Unit1 = "KG";
			Factory.Save();

			var declarationSupplier = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignor, true));
			var declarationImporter = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignee, true));
			var classification = Factory.New<CusClassification>();
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			classification.CC_LookupCode = "Z1Z!";
			classification.CC_TariffNum = "30000000";

			var product1 = Factory.New<OrgSupplierPart>();
			product1.OP_PartNum = "Test Product1";
			product1.OP_Weight = 100;
			product1.OP_WeightUQ = Core.Constants.Weight.MetricCarat;
			product1.OP_NetWeight = 9;
			var orgRel = product1.RelatedOrganisations.AddOrganisationIfNotExist(declarationImporter.PK, OrgPartRelation.RelationshipTypes.Owner);
			product1.RelatedOrganisations.AddOrganisationIfNotExist(declarationSupplier.PK, OrgPartRelation.RelationshipTypes.Supplier);
			var pivot1 = product1.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot1.CI_TariffNum = "00000001";
			pivot1.CI_OH = orgRel.OU_OH;
			pivot1.CI_CC = classification.PK;

			var product2 = Factory.New<OrgSupplierPart>();
			product2.OP_PartNum = "Test Product2";
			product2.OP_Weight = 100;
			product2.OP_WeightUQ = Core.Constants.Weight.MetricCarat;
			product2.OP_NetWeight = 9;
			var orgRel2 = product1.RelatedOrganisations.AddOrganisationIfNotExist(declarationImporter.PK, OrgPartRelation.RelationshipTypes.Owner);
			product2.RelatedOrganisations.AddOrganisationIfNotExist(declarationSupplier.PK, OrgPartRelation.RelationshipTypes.Supplier);
			var pivot2 = product2.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot2.CI_TariffNum = "00000002";
			pivot2.CI_OH = orgRel2.OU_OH;
			pivot2.CI_CC = classification.PK;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_DeclarationReference = "J00001";
			declaration.JE_OH_Importer = declarationImporter.PK;
			declaration.JE_OH_Supplier = declarationSupplier.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Buyer = declarationImporter.PK;
			invoice.JZ_OH_Supplier = declarationSupplier.PK;
			Factory.Save();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = product1.OP_PartNum;
			Factory.Save();
			var partTest = invoiceLine.Part;
			AssertNotNull(partTest);
			AssertEquals(ZString.Empty, invoiceLine.US_LaceyIndicator);
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			AssertEquals("FDA Msg Status should be set", FDAStatusList.Codes.REQ, declaration.FDAMsgStatus);

			invoiceLine.JI_PartNo = product2.OP_PartNum;
			Factory.Save();
			AssertEquals("FDA Msg Status should be updated", FDAStatusList.Codes.NR, declaration.FDAMsgStatus);

			pivot1.CD_ACEFDAIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.JI_PartNo = product1.OP_PartNum;
			Factory.Save();
			AssertEquals("FDA Msg Status should be updated as new FDA added from Product", FDAStatusList.Codes.REQ, declaration.FDAMsgStatus);

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			AssertEquals("FDA Msg Status should not be updated", FDAStatusList.Codes.REQ, declaration.FDAMsgStatus);
			invoiceLine2.JI_PartNo = product2.OP_PartNum;
			Factory.Save();
			AssertEquals("FDA Msg Status should be REQ,because the job has one invoice line has D ", FDAStatusList.Codes.REQ, declaration.FDAMsgStatus);

			invoiceLine.JI_PartNo = product2.OP_PartNum;
			Factory.Save();
			AssertEquals("FDA Msg Status should be NR,because the job none of invoice line has D ", FDAStatusList.Codes.NR, declaration.FDAMsgStatus);
		}

		public void TestWeightsAreSetFromProductComponents()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "WEIGHTTEST";
			product.OP_Desc = "WEIGHTTEST";
			product.RelatedOrganisations.AddOwner(importer);
			product.OP_Weight = 10;
			product.OP_WeightUQ = Core.Constants.Weight.Kilograms;
			product.OP_NetWeight = 9;

			//main pivot
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "9101000010";

			//component or child
			var pivot2 = pivot.Children.AddNew();
			pivot2.CI_TariffNum = "9201000010";
			pivot2.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			pivot2.CD_GrossWeight = 2;
			pivot2.CD_NetWeight = 1;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = product.OP_PartNum;

			AssertEquals("pre-condition", product.PK, invoiceLine.JI_OP);
			AssertEquals("pre-condition", 2, invoice.InvoiceLines.Count);

			invoiceLine.JI_InvoiceQuantity = 10;
			var childLine = invoiceLine.ChildLines.First();
			childLine.JI_InvoiceQuantity = 7;

			AssertEquals(100m, invoiceLine.JI_Weight);
			AssertEquals(90m, invoiceLine.JI_NetWeight);

			AssertEquals(14m, childLine.JI_Weight);
			AssertEquals(7m, childLine.JI_NetWeight);

			invoiceLine.JI_InvoiceQuantity = 3;
			childLine.JI_InvoiceQuantity = 9;

			AssertEquals(30m, invoiceLine.JI_Weight);
			AssertEquals(27m, invoiceLine.JI_NetWeight);

			AssertEquals(18m, childLine.JI_Weight);
			AssertEquals(9m, childLine.JI_NetWeight);
		}

		public void TestIsGoingIntoBondedWarehouse()
		{
			var org = Factory.New<OrgHeader>();
			org.CompanyData.OB_IMUsedBondedWhs = true;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = org.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = ZString.Empty;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(false, invoiceLine1.IsGoingIntoBondedWarehouse);
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			invoice.JobComInvoiceLines.Add(invoiceLine1);
			AssertEquals(false, invoiceLine1.IsGoingIntoBondedWarehouse);
			var list = new EntryTypeList();
			foreach (var code in new[]
			{
				EntryTypeList.Codes.Warehouse,
				EntryTypeList.Codes.WarehouseFTZ,
				EntryTypeList.Codes.ReWarehouse
			})
			{
				list.RemoveCode(code);
				declaration.US_EntryType = code;
				AssertEquals(true, invoiceLine1.IsGoingIntoBondedWarehouse);
				AssertEquals(false, invoiceLine2.IsGoingIntoBondedWarehouse);
			}

			foreach (ICodeDescription pair in list)
			{
				declaration.US_EntryType = pair.Code;
				AssertEquals(false, invoiceLine1.IsGoingIntoBondedWarehouse);
			}
		}

		public void TestUpdateDetailsOnPartChange_ZoneStatus()
		{
			var supplier = Factory.LoadTop1<OrgHeader>(new ZQuery());
			supplier.OH_IsConsignor = true;
			var part = Factory.New<OrgSupplierPart>();
			part.OP_StockKeepingUnit = "BAG";
			part.OP_PartNum = "NEWPROD1";
			part.OP_Desc = "PRODUCT1";

			var pivot = part.PivotsForBinding.AddNew();
			pivot.CD_ZoneStatus = ZoneStatusList.Codes.PrivilegedForeign;

			var relatedOrganization = part.RelatedOrganisations.AddNew();
			relatedOrganization.OU_OH = supplier.PK;
			relatedOrganization.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var header = declaration.Invoices.AddNew();
			var invoiceLine = header.InvoiceLines.AddNew();
			declaration.JE_OH_Supplier = supplier.PK;

			invoiceLine.JI_PartNo = part.OP_PartNum;
			AssertEquals(ZString.Empty, invoiceLine.US_ZoneStatus);
			invoiceLine.JI_PartNo = ZString.Empty;

			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			invoiceLine.JI_PartNo = part.OP_PartNum;
			AssertEquals("P", invoiceLine.US_ZoneStatus);
			invoiceLine.JI_PartNo = ZString.Empty;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			invoiceLine.JI_PartNo = part.OP_PartNum;
			AssertEquals(ZString.Empty, invoiceLine.US_ZoneStatus);
			invoiceLine.JI_PartNo = ZString.Empty;

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			invoiceLine.JI_PartNo = part.OP_PartNum;
			AssertEquals("P", invoiceLine.US_ZoneStatus);
		}

		public void TestTariffCorrectlyDefaultedFromProduct()
		{
			var declaration = Factory.New<JobDeclaration>();
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "WEIGHTTEST";
			product.OP_Desc = "WEIGHTTEST";
			product.RelatedOrganisations.AddOwner(importer);
			product.OP_Weight = 10;
			product.OP_WeightUQ = Core.Constants.Weight.Kilograms;
			product.OP_NetWeight = 9;
			product.OP_StockKeepingUnit = "UNT";

			//main pivot
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "9101000010";
			pivot.CI_SupplementalTariff = "9101000011";
			pivot.CD_PerUnitCost = 0m;
			pivot.CD_RX_NKPerUnitCostCurr = "USD";
			pivot.SupFormattedAdditionalTariff1 = "9903.88.01";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = product.OP_PartNum;
			invoiceLine.UpdateDetailsOnPartChange();
			AssertEquals("Regular Tariff defaulted", "9101000010", invoiceLine.JI_Tariff);
			AssertEquals("Prov Tariff defaulted", "9101000011", invoiceLine.US_SupTariff);
			AssertEquals("Prov Additional Tariff defaulted", "99038801", invoiceLine.US_SupAdditionalTariff1);
			AssertEquals("Only 1 line created", 1, invoice.InvoiceLines.Count);

			var childLine1 = pivot.Children.AddNew();
			childLine1.CI_ChildType = "COM";
			childLine1.CI_TariffNum = "8101000010";
			childLine1.CI_SupplementalTariff = "8101000011";
			childLine1.SupFormattedAdditionalTariff2 = "9903.88.02";
			var childLine2 = pivot.Children.AddNew();
			childLine2.CI_ChildType = "COM";
			childLine2.CI_TariffNum = "7101000010";
			childLine2.SupFormattedAdditionalTariff3 = "9903.88.03";

			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = product.OP_PartNum;
			invoiceLine.UpdateDetailsOnPartChange();
			AssertEquals("Tariff defaulted", "9101000010", invoiceLine.JI_Tariff);
			AssertEquals("Tariff defaulted", "9101000011", invoiceLine.US_SupTariff);
			AssertEquals("Tariff defaulted", "99038801", invoiceLine.US_SupAdditionalTariff1);
			AssertEquals("Only 2 child lines created", 2, invoiceLine.ChildLines.Count());
			var line3 = invoiceLine.ChildLines.FirstOrDefault(x => x.JI_LineNo == 3);
			var line4 = invoiceLine.ChildLines.FirstOrDefault(x => x.JI_LineNo == 4);
			AssertEquals("Tariff defaulted", "8101000010", line3.JI_Tariff);
			AssertEquals("Tariff defaulted", "8101000011", line3.US_SupTariff);
			AssertEquals("Tariff defaulted", "99038802", line3.US_SupAdditionalTariff2);
			AssertEquals("Tariff defaulted", "7101000010", line4.JI_Tariff);
			AssertEquals("Tariff defaulted", "", line4.US_SupTariff);
			AssertEquals("Tariff defaulted", "99038803", line4.US_SupAdditionalTariff3);
		}

		public void TestZeroUnitPriceIsNotCopiedFromProduct()
		{
			var declaration = Factory.New<JobDeclaration>();

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "WEIGHTTEST";
			product.OP_Desc = "WEIGHTTEST";
			product.RelatedOrganisations.AddOwner(importer);
			product.OP_Weight = 10;
			product.OP_WeightUQ = Core.Constants.Weight.Kilograms;
			product.OP_NetWeight = 9;
			product.OP_StockKeepingUnit = "UNT";

			//main pivot
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "9101000010";
			pivot.CD_PerUnitCost = 0m;
			pivot.CD_RX_NKPerUnitCostCurr = "USD";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_InvoiceQuantity = 10000m;
			invoiceLine.JI_InvoiceUQ = "KG";
			invoiceLine.JI_LinePrice = 1000m;
			invoiceLine.JI_PartNo = product.OP_PartNum;
			AssertEquals("pre-condition", product.PK, invoiceLine.JI_OP);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			invoiceLine.UpdateDetailsOnPartChange();
			AssertEquals("Line price should not be recalculated", 1000m, invoiceLine.JI_LinePrice);
		}

		public void TestCountryOfOriginFieldInfo()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.NewZealand;
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Portugal;
			invoiceLine.CountryOfOriginFieldInfo.Value = (ZString)Core.Constants.CountryCodes.SaintLucia;

			AssertEquals("JI_CountryOfOrigin", Core.Constants.CountryCodes.NewZealand, invoiceLine.JI_CountryOfOrigin);
			AssertEquals("JI_CountryOfOrigin", Core.Constants.CountryCodes.SaintLucia, invoiceLine.US_UC_NKCountryOfOrigin);
		}

		public void TestDefaultFromNetWeightWhenTariffEnteredAfterNetWeight()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_Unit1 = "KG";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_NetWeight = 10m;
			invoiceLine.JI_NetWeightUQ = "KG";

			invoiceLine.JI_Tariff = "00000000";
			AssertEquals("Customs quantity should be copied from net weight", 10m, invoiceLine.JI_CustomsQuantity);
		}

		public void TestFDAInvValueUpdate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 5000m;
			invoiceLine.JI_Description = "TEST DESCRIPTION";

			var fda = invoiceLine.FDAs.AddNew();
			AssertEquals("PreCondition", 5000m, fda.US_InvCurrFDAValue);

			invoiceLine.JI_LinePrice = 4900m;
			AssertEquals("PreCondition", 4900m, fda.US_InvCurrFDAValue);

			var aceFDA = invoiceLine.ACE_FDALines.AddNew();
			invoiceLine.JI_LinePrice = 3200m;
			AssertEquals(3200m, aceFDA.US_InvCurrValue);

			var lacey = invoiceLine.LaceyActLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;
			AssertEquals(10000m, lacey.US_InvCurrPGAValue);
		}

		[TestDate(2009, 1, 1)]
		public void TestCalculateSecondUQFromNetWeight()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_NetWeight = 0.1m;
			invoiceLine.JI_NetWeightUQ = "T";
			invoiceLine.JI_Tariff = "5407532060";
			AssertNotNull("tariff", invoiceLine.ImportTariff);
			AssertEquals("Second qty", "KG", invoiceLine.JI_CustomsSecondUnitQty);
			AssertEquals("Second UQ calcualted from Net weight", 100m, invoiceLine.JI_CustomsSecondQuantity);
		}

		[TestDate(2009, 1, 1)]
		public void TestCalculateSecondUQWhenProductIsHooked()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Test";
			var relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = importer.PK;
			relation.OU_Relationship = "OWN";
			product.OP_StockKeepingUnit = "UNT";
			product.OP_NetWeight = 0.1m;
			product.OP_WeightUQ = "T";

			var conversion = product.PartUnits.AddNew();
			conversion.OF_PackType = "M2";
			conversion.OF_QuantityInParent = 50m;
			conversion.OF_ParentPackType = "UNT";

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = "5407532060";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "Test";

			invoiceLine.JI_InvoiceQuantity = 2m;
			invoiceLine.JI_InvoiceUQ = "UNT";

			AssertEquals("Net weight", 200m, invoiceLine.JI_NetWeight);
			AssertEquals("Net weight", "KG", invoiceLine.JI_NetWeightUQ);
			AssertEquals("Customs Qty", "M2", invoiceLine.JI_CustomsUnitQty);
			AssertEquals("Customs qty", 100m, invoiceLine.JI_CustomsQuantity);
			AssertEquals("Second Qtt", "KG", invoiceLine.JI_CustomsSecondUnitQty);
			AssertEquals("Second qty", 200m, invoiceLine.JI_CustomsSecondQuantity);
		}

		public void TestUpdateUS_IsParentWhenAddANewUncommitChildAndThenCancelIt()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.Invoices.AddNew();
			var parentLine = declaration.FilteredInvoiceLines.AddNew();
			parentLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			declaration.FilteredInvoiceLines.AddNew();
			Assert("Pre-condition", parentLine.US_IsParent);
			((IBindingList)declaration.FilteredInvoiceLines).AddNew();
			((ICancelAddNew)declaration.FilteredInvoiceLines).EndNew(declaration.FilteredInvoiceLines.Count - 1);
			Assert("still a parent Line", parentLine.US_IsParent);
		}

		public void TestNoStackOverflowWhenAccessingAIILine_CS00135416()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;
			declaration.US_EnableAII = true;
			var invoice = declaration.Invoices.AddNew();
			invoice.US_IsLineGrouping = true;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var range = invoiceLine.LineGroupingRanges.AddNew(1, 2);
			var aiiLine1 = invoiceLine.AIILines.AddNew();
			var aiiLine2 = invoiceLine.AIILines.AddNew();
			AssertNoExceptionThrown(delegate
			{ invoiceLine.AIILines.Sort(AIILine.Schema.US_CustomsQty, ListSortDirection.Ascending); });
		}

		public void TestFDAValuesGetDefaultedFromInvoiceLineEvenWithProductCode()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Test";

			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = importer.PK;
			relOrg.OU_Relationship = "OWN";

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = USCTariff.CottonFeeApplicable;
			pivot.CD_ACEFDAIndicator = OGAIndicatorList.Codes.Declared;
			var fdaOnProduct = pivot.ACEFDAs.AddNew();
			fdaOnProduct.US_ProductCode = "123456";
			fdaOnProduct.US_BrandName = "CN";
			fdaOnProduct.US_Description = "TEST DESCRIPTION";
			Factory.Save();

			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer.OH_RL_NKClosestPort = "KRBUS";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			invoice.JZ_OA_ManufacturerAddress = manufacturer.MainAddress.PK;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 5000m;
			invoiceLine.JI_Description = "TEST DESCRIPTION";
			invoiceLine.JI_PartNo = "Test";

			AssertEquals(product, invoiceLine.Part);
		}

		public void TestRefreshTariffDetails()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusTariffTypeEXP = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.Export);
			Factory.Save();

			var tariffEXP = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, cusTariffTypeEXP.PK, "1234567890", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateTariffUOM(tariffEXP, "CU1", "A");
			helper.CreateTariffUOM(tariffEXP, "CU2", "B");
			helper.CreateTariffUOM(tariffEXP, "CU3", "C");

			var tariffEXP2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, cusTariffTypeEXP.PK, "1234567892", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddDays(-20));
			helper.CreateTariffUOM(tariffEXP2, "CU1", "A1");
			helper.CreateTariffUOM(tariffEXP2, "CU2", "B1");
			helper.CreateTariffUOM(tariffEXP2, "CU3", "C1");

			var cusTariffTypeSHB = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.ScheduleB);
			Factory.Save();

			var scheduleB = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, cusTariffTypeSHB.PK, "1234567890", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			helper.CreateTariffUOM(scheduleB, "CU1", "D");
			helper.CreateTariffUOM(scheduleB, "CU2", "E");

			var scheduleB2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, cusTariffTypeSHB.PK, "1234567892", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddDays(-20));
			helper.CreateTariffUOM(scheduleB2, "CU1", "D1");
			helper.CreateTariffUOM(scheduleB2, "CU2", "E1");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.US_TariffType = Universal.Constants.TariffTypes.ScheduleB;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(Universal.Constants.TariffTypes.ScheduleB, invoiceLine.US_TariffType);

			invoiceLine.JI_Tariff = "1234567890";
			AssertEquals("JI_CustomsUnitQty", "D", invoiceLine.JI_CustomsUnitQty);
			AssertEquals("JI_CustomsSecondUnitQty", "E", invoiceLine.JI_CustomsSecondUnitQty);
			AssertEquals("JI_CustomsThirdUnitQty", "", invoiceLine.JI_CustomsThirdUnitQty);

			declaration.US_TariffType = TariffTypeList.Codes.HTS;
			AssertEquals(TariffTypeList.Codes.HTS, invoiceLine.US_TariffType);
			AssertEquals("JI_CustomsUnitQty", "A", invoiceLine.JI_CustomsUnitQty);
			AssertEquals("JI_CustomsSecondUnitQty", "B", invoiceLine.JI_CustomsSecondUnitQty);
			AssertEquals("JI_CustomsThirdUnitQty", "C", invoiceLine.JI_CustomsThirdUnitQty);

			invoiceLine.US_TariffType = TariffTypeList.Codes.ScheduleB;
			AssertEquals(TariffTypeList.Codes.HTS, declaration.US_TariffType);

			AssertEquals("Tariff details should be refreshed when Tariff Type changed on line level",
				TariffTypeList.Codes.ScheduleB, invoiceLine.US_TariffType);
			AssertEquals("JI_CustomsUnitQty", "D", invoiceLine.JI_CustomsUnitQty);
			AssertEquals("JI_CustomsSecondUnitQty", "E", invoiceLine.JI_CustomsSecondUnitQty);
			AssertEquals("JI_CustomsThirdUnitQty", "", invoiceLine.JI_CustomsThirdUnitQty);

			invoiceLine.US_TariffType = TariffTypeList.Codes.HTS;
			AssertEquals("Tariff details should be refreshed when Tariff Type changed on line level",
				TariffTypeList.Codes.HTS, invoiceLine.US_TariffType);
			AssertEquals("JI_CustomsUnitQty", "A", invoiceLine.JI_CustomsUnitQty);
			AssertEquals("JI_CustomsSecondUnitQty", "B", invoiceLine.JI_CustomsSecondUnitQty);
			AssertEquals("JI_CustomsThirdUnitQty", "C", invoiceLine.JI_CustomsThirdUnitQty);

			declaration.US_TariffType = Universal.Constants.TariffTypes.ScheduleB;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(Universal.Constants.TariffTypes.ScheduleB, invoiceLine2.US_TariffType);
			invoiceLine2.JI_Tariff = "1234567892";
			AssertEquals("JI_CustomsUnitQty", "D1", invoiceLine2.JI_CustomsUnitQty);
			AssertEquals("JI_CustomsSecondUnitQty", "E1", invoiceLine2.JI_CustomsSecondUnitQty);
			AssertEquals("JI_CustomsThirdUnitQty", "", invoiceLine2.JI_CustomsThirdUnitQty);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;

			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "1234567891";
			tariff2.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff2.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff2.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;
			tariff2.UE_Column1RateAdValorem = 0.16200000m;

			invoiceLine.US_DRWClaimAmountOverriden_New = true;
			invoiceLine.JI_Tariff = "1234567891";
			Assert(invoiceLine.US_DRWCalcDutyWithAdValoremRate);
			AssertEquals(16.200000m, invoiceLine.US_DRWAdValoremRate);
		}

		public void TestEffectiveCountryOfOriginCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			var invoice = declaration.Invoices.AddNew();
			invoice.US_UC_NKCountryOfOrigin = "AU";

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals("AU", invoiceLine.EffectiveCountryOfOrigin);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceLine.JI_CountryOfOrigin = "US";
			AssertEquals("US", invoiceLine.EffectiveCountryOfOrigin);
		}

		public void TestCompleteEntryNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertContains("XJ5", invoiceLine.CompleteEntryNumber);
		}

		public void TestCalculatingUnitPriceWhenQuantityChanges()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_InvoiceQuantity = 2000m;
			invoiceLine.JI_InvoiceUQ = "M2";

			invoiceLine.JI_LinePrice = 100000m;
			AssertEquals("UnitPrice calculated", 50m, invoiceLine.UnitPrice);

			invoiceLine.JI_InvoiceQuantity = 1999m;
			AssertEquals("Unit Price should be updated, not invoice line price", 100000m, invoiceLine.JI_LinePrice);
			AssertEquals("Unit price updated", 50.0250m, invoiceLine.UnitPrice);

			invoiceLine.UnitPrice = 60m;
			AssertEquals("Line Price should be updated", 119940m, invoiceLine.JI_LinePrice);

			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_InvoiceQuantity = 1999m;
			invoiceLine.UnitPrice = 50.0250m;
			AssertEquals(99999.98m, invoiceLine.JI_LinePrice);
			invoiceLine.UnitPrice = 60m;
			AssertEquals("Line Price should be updated", 119940m, invoiceLine.JI_LinePrice);

			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.UnitPrice = 50.0250m;
			invoiceLine.JI_InvoiceQuantity = 1999m;
			AssertEquals(99999.98m, invoiceLine.JI_LinePrice);
			invoiceLine.JI_InvoiceQuantity = 2000m;
			AssertEquals("UnitPrice calculated", 50m, invoiceLine.UnitPrice);
			AssertEquals(99999.98m, invoiceLine.JI_LinePrice);
		}

		public void TestDefaultRateIndicator()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			declaration.Invoices.AddNew().JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_ADDCaseNo = "A";
			AssertEquals(ZString.Empty, invoiceLine.US_ADDDepositRateIndicator);
			AssertEquals(ZString.Empty, invoiceLine.US_CVDDepositRateIndicator);
		}

		public void TestADDCVDDepositRate()
		{
			var acCase = Factory.New<USCACCase>();
			acCase.U5_CaseNumber = "A1";

			var caseRate = acCase.CaseRates.AddNew();
			caseRate.U6_EffectiveDate = ZDateTime.BrettsBirthday;
			caseRate.U6_AdValoremRate = 0.52m;
			caseRate.U6_SpecificRate = 0.62m;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_ADDCaseNo = "A1";

			AssertNotNull(invoiceLine.AntidumpingDutyCase);
			AssertNull(invoiceLine.CountervailingDutyCase);

			AssertEquals("US_ADDDepositRate:AdValorem", 0.52m, invoiceLine.US_ADDDepositRate);

			invoiceLine.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.Specific;
			AssertEquals("US_ADDDepositRate:Specific", 0.62m, invoiceLine.US_ADDDepositRate);
		}

		public void TestUS_ADDDepositRateDescriptionReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_ADDCaseNo = "A1";
			invoiceLine.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.Specific;
			invoiceLine.US_ADDuty = 100m;

			invoiceLine.US_CVDCaseNo = "C1";
			invoiceLine.US_CVDDepositRateIndicator = DepositRateIndicatorList.Codes.Specific;
			invoiceLine.US_CVDuty = 200m;

			invoiceLine.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.AdValorem;
			AssertEquals(true, invoiceLine.US_ADDDepositRateDescriptionInfo.ReadOnly);
			invoiceLine.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.Specific;
			AssertEquals(true, invoiceLine.US_ADDDepositRateDescriptionInfo.ReadOnly);
			invoiceLine.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.OverrideAdValorem;
			AssertEquals(false, invoiceLine.US_ADDDepositRateDescriptionInfo.ReadOnly);
			invoiceLine.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.OverrideSpecific;
			AssertEquals(false, invoiceLine.US_ADDDepositRateDescriptionInfo.ReadOnly);
		}

		public void TestUS_CVDDepositRateDescriptionReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_ADDCaseNo = "A1";
			invoiceLine.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.Specific;
			invoiceLine.US_ADDuty = 100m;

			invoiceLine.US_CVDCaseNo = "C1";
			invoiceLine.US_CVDDepositRateIndicator = DepositRateIndicatorList.Codes.Specific;
			invoiceLine.US_CVDuty = 200m;

			invoiceLine.US_CVDDepositRateIndicator = DepositRateIndicatorList.Codes.AdValorem;
			AssertEquals(true, invoiceLine.US_CVDDepositRateDescriptionInfo.ReadOnly);
			invoiceLine.US_CVDDepositRateIndicator = DepositRateIndicatorList.Codes.Specific;
			AssertEquals(true, invoiceLine.US_CVDDepositRateDescriptionInfo.ReadOnly);
			invoiceLine.US_CVDDepositRateIndicator = DepositRateIndicatorList.Codes.OverrideAdValorem;
			AssertEquals(false, invoiceLine.US_CVDDepositRateDescriptionInfo.ReadOnly);
			invoiceLine.US_CVDDepositRateIndicator = DepositRateIndicatorList.Codes.OverrideSpecific;
			AssertEquals(false, invoiceLine.US_CVDDepositRateDescriptionInfo.ReadOnly);
		}

		public void TestSetDepositRateIndicator()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_ADDCaseNo = "A1";
			invoiceLine.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.Specific;
			invoiceLine.US_ADDuty = 100m;

			invoiceLine.US_CVDCaseNo = "C1";
			invoiceLine.US_CVDDepositRateIndicator = DepositRateIndicatorList.Codes.Specific;
			invoiceLine.US_CVDuty = 200m;

			invoiceLine.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.AdValorem;
			invoiceLine.US_CVDDepositRateIndicator = DepositRateIndicatorList.Codes.AdValorem;

			AssertEquals("US_ADDuty", 0m, invoiceLine.US_ADDuty);
			AssertEquals("US_CVDuty", 0m, invoiceLine.US_CVDuty);
		}

		public void TestMapValuesBetweenACEAndACSForADD_CVDIndicator()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_ADDCaseNo = "A1";
			invoiceLine.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.AdValorem;

			invoiceLine.US_CVDCaseNo = "C1";
			invoiceLine.US_CVDDepositRateIndicator = DepositRateIndicatorList.Codes.AdValorem;

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			AssertEquals("US_ADDDepositRateIndicator should be defaulted to A for ACE", DepositRateIndicatorList.Codes.AdValorem, invoiceLine.US_ADDDepositRateIndicator);
			AssertEquals("US_CVDDepositRateIndicator should be defaulted to A for ACE", DepositRateIndicatorList.Codes.AdValorem, invoiceLine.US_CVDDepositRateIndicator);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertEquals("US_ADDDepositRateIndicator should be defaulted to A for ACS", DepositRateIndicatorList.Codes.AdValorem, invoiceLine.US_ADDDepositRateIndicator);
			AssertEquals("US_CVDDepositRateIndicator should be defaulted to A for ACS", DepositRateIndicatorList.Codes.AdValorem, invoiceLine.US_CVDDepositRateIndicator);
		}

		public void TestDefaultAndRefreshFDAValue()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			invoice.JZ_IncoTerm = "CIF";

			invoice.Charges.AddNew("OFT", 50m, "USD").J7_IsIncludedInITOT = true;
			invoice.Charges.AddNew("ONS", 5.45m, "USD").J7_IsIncludedInITOT = true;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 5000m;

			var fda = invoiceLine.FDAs.AddNew();

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("CustomsValue is correct", 4945m, fda.US_FDAValue);

			invoiceLine.JI_LinePrice = 6000m;
			fda.US_InvCurrFDAValue = 6000m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("CustomsValue is refreshed", 5945m, fda.US_FDAValue);
		}

		public void TestMapValuesBetweenACEAndACSForSetIndicatorX_V()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLineX = declaration.InvoiceLines.AddNew();
			invoiceLineX.US_SecondarySPI = "X";

			var invoiceLineV = declaration.InvoiceLines.AddNew();
			invoiceLineV.US_SecondarySPI = "V";

			var invoiceLineF = declaration.InvoiceLines.AddNew();
			invoiceLineF.US_SecondarySPI = "F";

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			AssertEquals("US_SetInd should be changed to X in ACE", "X", invoiceLineX.US_SetInd);
			AssertEquals("US_SecondarySPI should be changed to blank in ACE", "", invoiceLineX.US_SecondarySPI);

			AssertEquals("US_SetInd should be changed to V in ACE", "V", invoiceLineV.US_SetInd);
			AssertEquals("US_SecondarySPI should be changed to blank in ACE", "", invoiceLineV.US_SecondarySPI);

			AssertEquals("US_SetInd should be changed to blank in ACE", "", invoiceLineF.US_SetInd);
			AssertEquals("US_SecondarySPI should be left as F in ACE", "F", invoiceLineF.US_SecondarySPI);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertEquals("US_SetInd should be changed to blank for ACS", "", invoiceLineX.US_SetInd);
			AssertEquals("US_SecondarySPI should be changed to X for ACS", "X", invoiceLineX.US_SecondarySPI);

			AssertEquals("US_SetInd should be changed to blank for ACS", "", invoiceLineV.US_SetInd);
			AssertEquals("US_SecondarySPI should be changed to V for ACS", "V", invoiceLineV.US_SecondarySPI);

			AssertEquals("US_SetInd should be changed to blank for ACS", "", invoiceLineF.US_SetInd);
			AssertEquals("US_SecondarySPI should be left as F for ACS", "F", invoiceLineF.US_SecondarySPI);
		}

		public void TestMapValuesBetweenACEAndACSForLicenceTypesAndNumbers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalADDCVD;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			AssertEquals("PreCondition", 0, invoiceLine.LicenceAndPermits.Count);
			invoiceLine.US_AgricultureLicNo = "1-AA-111-1";
			invoiceLine.US_CottonCertificateNo = "123456789";
			invoiceLine.US_WoolLicenceNo = "123456789";
			invoiceLine.US_CAExportCertificate = "12345678";
			invoiceLine.US_CBTPACertificateNo = "1CB45678";
			invoiceLine.JI_Tariff = "2523290000";
			AssertNotNull("PreCondition", invoiceLine.ImportTariff);
			invoiceLine.ImportTariff.UE_PermitLicenseIndicator = "07";
			AssertEquals("PreCondition", "07", invoiceLine.ImportTariff.UE_PermitLicenseIndicator);

			invoiceLine.US_MiscPermitNo = "AAA";

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			AssertEquals("LicenceAndPermits collection should contain US_AgricultureLicNo item for ACS", 6, invoiceLine.LicenceAndPermits.Count);
			AssertEquals("LicenceAndPermits collection CYCode should be US_AgricultureLicNo Type 14 for ACE", "14", invoiceLine.LicenceAndPermits[0].CY_Code);
			AssertEquals("LicenceAndPermits collection CYData should be 1-AA-111-1 for ACE", "1-AA-111-1", invoiceLine.LicenceAndPermits[0].CY_Data);
			AssertEquals("LicenceAndPermits collection CYCode should be US_CottonCertificateNo Type 17 for ACE", "17", invoiceLine.LicenceAndPermits[1].CY_Code);
			AssertEquals("LicenceAndPermits collection CYData should be 123456789 for ACE", "123456789", invoiceLine.LicenceAndPermits[1].CY_Data);
			AssertEquals("Cannot determine LicenceAndPermits collection CY_Code for cotton certificate number for ACE", "??", invoiceLine.LicenceAndPermits[2].CY_Code);
			AssertEquals("LicenceAndPermits collection CYData should be 123456789 for ACE", "123456789", invoiceLine.LicenceAndPermits[2].CY_Data);
			AssertEquals("LicenceAndPermits collection CYCode should be US_CAExportCertificate Type 16 for ACE", "16", invoiceLine.LicenceAndPermits[3].CY_Code);
			AssertEquals("LicenceAndPermits collection CYData should be 12345678 for ACE", "12345678", invoiceLine.LicenceAndPermits[3].CY_Data);
			AssertEquals("LicenceAndPermits collection CYCode should be US_CBTPACertificateNo Type 18 for ACE", "18", invoiceLine.LicenceAndPermits[4].CY_Code);
			AssertEquals("LicenceAndPermits collection CYData should be 1CB45678 for ACE", "1CB45678", invoiceLine.LicenceAndPermits[4].CY_Data);
			AssertEquals("LicenceAndPermits collection CYCode should be 07 for ACE", invoiceLine.ImportTariff.UE_PermitLicenseIndicator, invoiceLine.LicenceAndPermits[5].CY_Code);
			AssertEquals("LicenceAndPermits collection CYData should be AAA for ACE", "AAA", invoiceLine.LicenceAndPermits[5].CY_Data);

			invoiceLine.LicenceAndPermits.AddNew("06", "7102310000");
			invoiceLine.LicenceAndPermits[2].CY_Code = LicencePermitTypeList.Codes._22;

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertEquals("LicenceAndPermits collection should be cleared", 0, invoiceLine.LicenceAndPermits.Count);
			AssertEquals("US_AgricultureLicNo should be 1-AA-111-1 for ACS", "1-AA-111-1", invoiceLine.US_AgricultureLicNo);
			AssertEquals("US_CottonCertificateNo should be 123456789 for ACS", "123456789", invoiceLine.US_CottonCertificateNo);
			AssertEquals("US_WoolLicenceNo should be 123456789 for ACS", "123456789", invoiceLine.US_WoolLicenceNo);
			AssertEquals("US_CAExportCertificate should be 12345678 for ACS", "12345678", invoiceLine.US_CAExportCertificate);
			AssertEquals("US_CBTPACertificateNo should be 1CB45678 for ACS", "1CB45678", invoiceLine.US_CBTPACertificateNo);
			AssertEquals("US_MiscPermitNo should be AAA for ACS", "AAA", invoiceLine.US_MiscPermitNo);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			invoiceLine.LicenceAndPermits.AddNew(LicencePermitTypeList.Codes._22, "111");
			invoiceLine.LicenceAndPermits.AddNew(LicencePermitTypeList.Codes._23, "222");
			invoiceLine.LicenceAndPermits.AddNew(LicencePermitTypeList.Codes._12, "333");
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertEquals(0, invoiceLine.LicenceAndPermits.Count);
			AssertEquals("US_CottonCertificateNo cannot be mapped because more than one exemption certificate and we unable to match", ZString.Empty, invoiceLine.US_CottonCertificateNo);
		}

		public void TestDataImport()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "TSTCOD";
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "12345";
			var orgRel = part.RelatedOrganisations.AddOrganisationIfNotExist(org1.PK, OrgPartRelation.RelationshipTypes.Both);

			var pivot1 = part.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot1.CI_TariffNum = "10000000";
			pivot1.CI_OH = orgRel.OU_OH;
			pivot1.CD_ProductClaim = SecondarySpecProgIndicatorList.Codes.X;

			var child1 = pivot1.Children.AddNew();
			child1.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			child1.CI_TariffNum = "10000001";
			child1.CI_OH = orgRel.OU_OH;
			child1.CD_ProductClaim = SecondarySpecProgIndicatorList.Codes.V;

			var child2 = pivot1.Children.AddNew();
			child2.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			child2.CI_TariffNum = "10000002";
			child2.CI_OH = orgRel.OU_OH;
			child2.CD_ProductClaim = SecondarySpecProgIndicatorList.Codes.V;

			var part2 = Factory.New<OrgSupplierPart>();
			part2.OP_PartNum = "67890";
			orgRel = part2.RelatedOrganisations.AddOrganisationIfNotExist(org1.PK, OrgPartRelation.RelationshipTypes.Both);

			var pivot3 = part2.PivotsForBinding.AddNew();
			pivot3.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot3.CI_TariffNum = "20000000";
			pivot3.CI_OH = orgRel.OU_OH;

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = org1.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = declaration.FilteredInvoiceLines.AddNew();
			invoiceLine1.JI_PartNo = "12345";
			var invoiceLine2 = declaration.FilteredInvoiceLines.AddNew();
			invoiceLine2.JI_PartNo = "67890";

			AssertEquals(4, declaration.FilteredInvoiceLines.Count);
			AssertEquals("Should not be a child line", ZGuid.Empty, invoiceLine2.JI_ParentID);
			AssertEquals("Should be able to add a new product", "67890", invoiceLine2.JI_PartNo);

			invoice.InvoiceLines.RemoveAndDeleteAll();

			ImportCollectionInfoImpl info = new ImportCollectionInfoImpl(declaration.FilteredInvoiceLines);
			info.Add(new ImportPropertyInfoImpl<JobComInvoiceLine>("JI_PartNo"));

			var mockery = new Mock<ImportWizard>(info, null, ObjectFactory.New<IFileMapper>()) { CallBase = true };

			mockery.Setup(m => m.LoadFile(1, -1, false))
				.Returns(new List<string[]>() { new string[] { "12345" }, new string[] { "67890" } });

			mockery.Object.Mapping[0].AddFileColumnIndex(0);

			mockery.Object.ImportIntoCollection(declaration.FilteredInvoiceLines, -1);

			AssertEquals(4, declaration.FilteredInvoiceLines.Count);

			AssertEquals(SecondarySpecProgIndicatorList.Codes.X, declaration.FilteredInvoiceLines[0].US_SetInd);
			AssertEquals(SecondarySpecProgIndicatorList.Codes.V, declaration.FilteredInvoiceLines[1].US_SetInd);
			AssertEquals(SecondarySpecProgIndicatorList.Codes.V, declaration.FilteredInvoiceLines[2].US_SetInd);
			AssertEquals(ZString.Empty, declaration.FilteredInvoiceLines[3].US_SetInd);

			AssertEquals(ZString.Empty, declaration.FilteredInvoiceLines[0].US_SecondarySPI);
			AssertEquals(ZString.Empty, declaration.FilteredInvoiceLines[1].US_SecondarySPI);
			AssertEquals(ZString.Empty, declaration.FilteredInvoiceLines[2].US_SecondarySPI);
			AssertEquals(ZString.Empty, declaration.FilteredInvoiceLines[3].US_SecondarySPI);

			AssertEquals("Should not be a child line after data import", ZGuid.Empty, declaration.FilteredInvoiceLines[3].JI_ParentID);
			AssertEquals("Should be able to add a new product after data import", "67890", declaration.FilteredInvoiceLines[3].JI_PartNo);
		}

		public void TestMap_X_V_ValuesForUpdateDetailsOnPartChange()
		{
			var newFactory = new BusinessObjectFactory();
			var org1 = newFactory.LoadTop1<OrgHeader>(new ZQuery());
			var part = newFactory.New<OrgSupplierPart>();
			part.OP_PartNum = "Z!Z2Z";
			var orgRel = part.RelatedOrganisations.AddOrganisationIfNotExist(org1.PK, OrgPartRelation.RelationshipTypes.Owner);

			var pivot1 = part.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot1.CI_TariffNum = "10000000";
			pivot1.CI_OH = orgRel.OU_OH;
			pivot1.CD_ProductClaim = SecondarySpecProgIndicatorList.Codes.X;
			pivot1.CD_CBTPACertificate = "0AU47243";

			var child1 = pivot1.Children.AddNew();
			child1.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			child1.CI_TariffNum = "10000001";
			child1.CI_OH = orgRel.OU_OH;
			child1.CD_ProductClaim = SecondarySpecProgIndicatorList.Codes.V;

			var child2 = pivot1.Children.AddNew();
			child2.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			child2.CI_TariffNum = "10000002";
			child2.CI_OH = orgRel.OU_OH;
			child2.CD_ProductClaim = SecondarySpecProgIndicatorList.Codes.V;

			newFactory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = org1.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();

			var invoiceLineX = invoice.JobComInvoiceLines.AddNew();
			invoiceLineX.JI_PartNo = "Z!Z2Z";

			AssertEquals("Should be one parent line and two child lines", 3, invoice.JobComInvoiceLines.Count);
			var invoiceLineV1 = invoice.JobComInvoiceLines[1];
			var invoiceLineV2 = invoice.JobComInvoiceLines[2];

			AssertEquals("US_SetInd should be changed to X in ACE", "X", invoiceLineX.US_SetInd);
			AssertEquals("US_SecondarySPI should be changed to blank in ACE", "", invoiceLineX.US_SecondarySPI);

			AssertEquals("US_SetInd should be changed to V in ACE (V1)", "V", invoiceLineV1.US_SetInd);
			AssertEquals("US_SecondarySPI should be changed to blank in ACE (V1)", "", invoiceLineV1.US_SecondarySPI);

			AssertEquals("US_SetInd should be changed to V in ACE (V2)", "V", invoiceLineV2.US_SetInd);
			AssertEquals("US_SecondarySPI should be changed to blank in ACE (V2)", "", invoiceLineV2.US_SecondarySPI);

			AssertEquals("This is an ACS field and should not be entered for ACE", ZString.Empty, invoiceLineX.US_CBTPACertificateNo);
			AssertEquals("LicenceAndPermits collection should have it instead", LicencePermitTypeList.Codes._18, invoiceLineX.LicenceAndPermits[0].CY_Code);
			AssertEquals("LicenceAndPermits collection should have it instead", "0AU47243", invoiceLineX.LicenceAndPermits[0].CY_Data);
		}

		public void TestSoldToPartyEIN()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			OrgHeader party1 = Factory.New<OrgHeader>();
			party1.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "1");

			OrgHeader party2 = Factory.New<OrgHeader>();
			party2.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "2");

			invoice.JZ_OA_SoldToPartyAddress = party1.MainAddress.PK;
			AssertEquals(party1.MainAddress.PK, invoiceLine.JI_OA_SoldToPartyAddress);
			AssertEquals("1", invoiceLine.SoldToPartyEIN);

			invoiceLine.JI_OA_SoldToPartyAddress = party2.MainAddress.PK;
			AssertEquals(party2.MainAddress.PK, invoiceLine.JI_OA_SoldToPartyAddress);
			AssertEquals("2", invoiceLine.SoldToPartyEIN);

			invoice.JZ_OA_SoldToPartyAddress = party2.MainAddress.PK;
			AssertEquals(party2.MainAddress.PK, invoiceLine.JI_OA_SoldToPartyAddress);
			AssertEquals("2", invoiceLine.SoldToPartyEIN);

			invoice.JZ_OA_SoldToPartyAddress = party1.MainAddress.PK;
			AssertEquals(party1.MainAddress.PK, invoiceLine.JI_OA_SoldToPartyAddress);
			AssertEquals("1", invoiceLine.SoldToPartyEIN);
		}

		public void TestForeignExporterMID()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			OrgHeader party1 = Factory.New<OrgHeader>();
			party1.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "1");

			OrgHeader party2 = Factory.New<OrgHeader>();
			party2.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "2");

			invoice.JZ_OA_ExporterAddress = party1.MainAddress.PK;
			AssertEquals(party1.MainAddress.PK, invoiceLine.JI_OA_ExporterAddress);
			AssertEquals("1", invoiceLine.ForeignExporterMID);

			invoiceLine.JI_OA_ExporterAddress = party2.MainAddress.PK;
			AssertEquals(party2.MainAddress.PK, invoiceLine.JI_OA_ExporterAddress);
			AssertEquals("2", invoiceLine.ForeignExporterMID);

			invoice.JZ_OA_ExporterAddress = party2.MainAddress.PK;
			AssertEquals(party2.MainAddress.PK, invoiceLine.JI_OA_ExporterAddress);
			AssertEquals("2", invoiceLine.ForeignExporterMID);

			invoice.JZ_OA_ExporterAddress = party1.MainAddress.PK;
			AssertEquals(party1.MainAddress.PK, invoiceLine.JI_OA_ExporterAddress);
			AssertEquals("1", invoiceLine.ForeignExporterMID);
		}

		public void TestUpdateTaxRelatedFieldsOnTariffChange()
		{
			SetUpTariffsForTaxRelatedFields();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			invoiceLine.JI_Tariff = "00000000";
			AssertEquals(TaxApplyList.Codes.Yes, invoiceLine.US_TaxApply);
			AssertEquals(Core.Constants.USCustoms.FeeCodes.Wines, invoiceLine.US_TaxCode);
			AssertEquals("50c/KG", invoiceLine.US_TaxRateS);
			AssertEquals(ZString.Empty, invoiceLine.US_TaxRateT);
			invoiceLine.US_TaxQty = 100m;

			invoiceLine.JI_Tariff = "00000001";
			AssertEquals(TaxApplyList.Codes.Override, invoiceLine.US_TaxApply);
			AssertEquals(Core.Constants.USCustoms.FeeCodes.OtherExcise, invoiceLine.US_TaxCode);
			AssertEquals(ZString.Empty, invoiceLine.US_TaxRateS);
			AssertEquals(ZString.Empty, invoiceLine.US_TaxRateT);
			AssertEquals(ZDecimal.Zero, invoiceLine.US_TaxQty);

			declaration.US_TaxDeferIndicator = TaxDeferIndicatorList.Codes.BulkLiquorDeferred;
			invoiceLine.JI_Tariff = "2203000090";
			AssertEquals(TaxApplyList.Codes.No, invoiceLine.US_TaxApply);
			AssertEquals(ZString.Empty, invoiceLine.US_TaxCode);
			AssertEquals(ZString.Empty, invoiceLine.US_TaxRateS);
			AssertEquals(ZString.Empty, invoiceLine.US_TaxRateT);
			AssertEquals(ZDecimal.Zero, invoiceLine.US_TaxQty);
		}

		public void TestSetTaxDetailsBackToDefault()
		{
			SetUpTariffsForTaxRelatedFields();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "00000000";
			AssertEquals(TaxApplyList.Codes.Yes, invoiceLine.US_TaxApply);
			invoiceLine.US_TaxQty = 100m;

			invoiceLine.US_TaxApply = TaxApplyList.Codes.No;
			AssertEquals(ZDecimal.Zero, invoiceLine.US_TaxQty);
		}

		public void TestIsTaxQtyRequired()
		{
			SetUpTariffsForTaxRelatedFields();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "00000000";

			Assert(!invoiceLine.IsTaxQtyRequired);

			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine.US_TaxRateS = AppendixBTaxRateList.Codes.Wines_1;
			Assert(invoiceLine.IsTaxQtyRequired);
			AssertEquals("L", invoiceLine.OverriddenTaxRateUQ);
		}

		public void TestTaxComputationCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2023, 05, 05);
			declaration.US_ITDate = new ZDate(2023, 05, 05);
			declaration.US_EstimatedEntryDate = new ZDateTime(2023, 05, 05);
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2208204000";
			invoiceLine.JI_CustomsUnitQty = "L";

			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine.US_TaxCode = Core.Constants.USCustoms.FeeCodes.DistilledSpirits;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.C;
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			AssertEquals("$3.566322/PFL", invoiceLine.US_TaxRateS);
			AssertEquals("IsCBMA23Effective", true, invoiceLine.IsCBMA23Effective);
			AssertEquals(true, invoiceLine.IsCBMAProductClaim);

			AssertEquals("US_SecondarySPI is C = _4 ", ComputationCodeList.CustomComputationCodeForCalculatingIRTax, invoiceLine.TaxComputationCode);
			invoiceLine.US_TaxApply = "";
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine.US_SecondarySPI = "";
			AssertEquals("US_SecondarySPI is empty = _4 ", ComputationCodeList.CustomComputationCodeForCalculatingIRTax, invoiceLine.TaxComputationCode);
		}

		public void TestTaxRateDetail()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2022, 01, 10);
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "00000000";
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine.US_TaxCode = Core.Constants.USCustoms.FeeCodes.OtherExcise;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.C;
			AssertEquals("IsCBMA23Effective", false, invoiceLine.IsCBMA23Effective);
			AssertEquals("invoiceLine.US_TaxRateS", "B01010", invoiceLine.US_TaxRateS);
			AssertEquals("invoiceLine.US_TaxRate", 0.1363469m, invoiceLine.US_TaxRate);
			AssertEquals("invoiceLine.TTBConfirmationRate", 16m, invoiceLine.TTBConfirmationRate);

			invoiceLine.US_TaxCode = Core.Constants.USCustoms.FeeCodes.Wines;
			AssertEquals("invoiceLine.US_TaxRateS", ZString.Empty, invoiceLine.US_TaxRateS);
			AssertEquals("invoiceLine.US_TaxRate", ZDecimal.Zero, invoiceLine.US_TaxRate);
			AssertEquals("invoiceLine.TTBConfirmationRate", ZDecimal.Zero, invoiceLine.TTBConfirmationRate);

			invoiceLine.US_TaxRateS = "W02020";
			AssertEquals("invoiceLine.US_TaxRate", 0.17699530m, invoiceLine.US_TaxRate);
			AssertEquals("invoiceLine.TTBConfirmationRate", 0.67m, invoiceLine.TTBConfirmationRate);
		}

		public void TestADDCVDFieldsVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.Specific;
			invoiceLine.US_CVDDepositRateIndicator = ZString.Empty;
			Assert("Qty should be visible for ADD", invoiceLine.ADDQtyVisible);
			Assert("Qty should be visible for ADD", !invoiceLine.ADDValueVisible);
			Assert("Value should be visible for CVD", !invoiceLine.CVDQtyVisible);
			Assert("Value should be visible for CVD", invoiceLine.CVDValueVisible);

			invoiceLine.US_CVDDepositRateIndicator = DepositRateIndicatorList.Codes.Specific;
			Assert("Qty should be visible for ADD", invoiceLine.ADDQtyVisible);
			Assert("Qty should be visible for ADD", !invoiceLine.ADDValueVisible);
			Assert("Qty should be visible for CVD", invoiceLine.CVDQtyVisible);
			Assert("Qty should be visible for CVD", !invoiceLine.CVDValueVisible);
		}

		public void TestShowHTSDefaultRate()
		{
			SetUpTariffsForTaxRelatedFields();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			AssertEquals(ZString.Empty, invoiceLine.US_TaxRateS);

			invoiceLine.JI_Tariff = "00000000";
			AssertEquals("50c/KG", invoiceLine.US_TaxRateS);

			invoiceLine.JI_Tariff = "00000001";
			AssertEquals(ZString.Empty, invoiceLine.US_TaxRateS);
		}

		public void TestTaxRelatedFieldsReadOnly()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff.UE_Unit1 = "KG";

			USCTariffDutyRate dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Wines;
			dutyRate.UD_TaxFeeFlag = "1";
			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificSpecific;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			AssertEquals(string.Empty, invoiceLine.US_TaxApply);
			Assert(invoiceLine.US_TaxRateTInfo.ReadOnly);
			Assert(invoiceLine.US_TaxRateSInfo.ReadOnly);

			invoiceLine.JI_Tariff = "00000000";
			AssertEquals(TaxApplyList.Codes.Yes, invoiceLine.US_TaxApply);
			Assert(!invoiceLine.US_TaxRateTInfo.ReadOnly);
			Assert(!invoiceLine.US_TaxRateSInfo.ReadOnly);

			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			Assert(!invoiceLine.US_TaxRateSInfo.ReadOnly);
			Assert(invoiceLine.US_TaxRateTInfo.ReadOnly);
		}

		public void TestClearUS_TaxRateTWhenUS_TaxRateTBecomeReadOnly()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff.UE_Unit1 = "KG";

			USCTariffDutyRate dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Wines;
			dutyRate.UD_TaxFeeFlag = "1";
			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificSpecific;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			invoiceLine.JI_Tariff = "00000000";
			invoiceLine.US_TaxRateT = RateTypeList.Codes.Primary;
			Assert(!invoiceLine.US_TaxRateTInfo.ReadOnly);
			AssertEquals(RateTypeList.Codes.Primary, invoiceLine.US_TaxRateT);

			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			Assert(invoiceLine.US_TaxRateTInfo.ReadOnly);
			AssertEquals(ZString.Empty, invoiceLine.US_TaxRateT);
		}

		public void TestIsCBMAProductClaim()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.C;
			Assert(invoiceLine.IsCBMAProductClaim);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			Assert(!invoiceLine.IsCBMAProductClaim);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.G;
			Assert(!invoiceLine.IsCBMAProductClaim);

			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.C;
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Yes;
			Assert(invoiceLine.IsCBMAProductClaim);
			invoiceLine.US_TaxApply = TaxApplyList.Codes.No;
			Assert(invoiceLine.IsCBMAProductClaim);
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			Assert(invoiceLine.IsCBMAProductClaim);
		}

		public void Test98ValueInvCurrAffectsBalance()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;
			invoice.JZ_InvoiceAmount = 100m;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 90m;
			declaration.ResumeApportionment();
			AssertEquals(10m, invoice.JZ_Calc_Balance);

			invoiceLine.US_98ValueInvCurr = 10m;
			declaration.ResumeApportionment();
			AssertEquals(0m, invoice.JZ_Calc_Balance);

			invoiceLine.US_98GoodsValue = 15m;
			declaration.ResumeApportionment();
			AssertEquals(0m, invoice.JZ_Calc_Balance);
		}

		public override void TestPivot()
		{
			var newFactory = new BusinessObjectFactory();
			var org1 = newFactory.LoadTop1<OrgHeader>(new ZQuery());
			var part = newFactory.New<OrgSupplierPart>();
			part.OP_PartNum = "Z!Z2Z";
			var orgRel = part.RelatedOrganisations.AddOrganisationIfNotExist(org1.PK, OrgPartRelation.RelationshipTypes.Owner);
			var pivot1 = part.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot1.CI_TariffNum = "10000000";
			pivot1.CI_OH = orgRel.OU_OH;
			var attrib1 = pivot1.Attributes1.AddNew();
			attrib1.BG_AttributeValue1 = "1";
			var pivot2 = part.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot2.CI_TariffNum = "20000000";
			pivot2.CI_OH = orgRel.OU_OH;
			var attrib2 = pivot2.Attributes1.AddNew();
			attrib2.BG_AttributeValue1 = "2";

			CusClassification classification = newFactory.New<CusClassification>();
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			classification.CC_LookupCode = "Z1Z!";
			classification.CC_TariffNum = "30000000";
			var pivot3 = part.PivotsForBinding.AddNew();
			pivot3.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot3.CI_CC = classification.PK;
			pivot3.CI_OH = orgRel.OU_OH;
			var attrib3 = pivot3.Attributes1.AddNew();
			attrib3.BG_AttributeValue1 = "3";
			newFactory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = org1.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLinePartSynchronisationManager.SetCurrentPartSyncManagerActiveDeciderPK(invoiceLine.Factory, invoiceLine.PartSyncManagerActiveDeciderPK);
			invoiceLine.JI_PartNo = "Z!Z2Z";
			AssertNull(invoiceLine.Pivot);
			AssertEquals("", invoiceLine.JI_Tariff);
			AssertEquals(ZGuid.Empty, invoiceLine.JI_CC);

			invoiceLine.JI_PartAttrib1 = "3";
			AssertEquals(pivot3.PK, invoiceLine.Pivot.PK);
			AssertEquals("30000000", invoiceLine.JI_Tariff);
			AssertEquals(classification.PK, invoiceLine.JI_CC);

			attrib3.BG_AttributeValue1 = "4";
			attrib2.BG_AttributeValue1 = "3";
			newFactory.Save();
			AssertEquals(pivot2.PK, invoiceLine.Pivot.PK);
			AssertEquals("20000000", invoiceLine.JI_Tariff);
			AssertEquals(ZGuid.Empty, invoiceLine.JI_CC);

			pivot2.CI_TariffNum = "20202020";
			newFactory.Save();
			AssertEquals(pivot2.PK, invoiceLine.Pivot.PK);
			AssertEquals("20202020", invoiceLine.JI_Tariff);
			AssertEquals(ZGuid.Empty, invoiceLine.JI_CC);
		}

		public void TestIsConsumptionFTZ()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			AssertEquals(true, invoiceLine.IsConsumptionFTZ);

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			AssertEquals(false, invoiceLine.IsConsumptionFTZ);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(false, invoiceLine.IsConsumptionFTZ);
		}

		public void TestNotDeleteChildLinesWhenAlreadyExisted()
		{
			var tariffNumber0 = "1010111111";
			var tariffNumber1 = "1001101100";
			var tariffNumber2 = "1002202200";

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Test1";
			product.OP_Desc = "UPDATED";
			product.RelatedOrganisations.AddOwner(importer);

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = tariffNumber0;
			pivot.CD_CBTPACertificate = "A";

			var relatedTariffPivot1 = pivot.Children.AddNew();
			relatedTariffPivot1.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			relatedTariffPivot1.CI_TariffNum = tariffNumber1;
			relatedTariffPivot1.CD_CBTPACertificate = "1";
			relatedTariffPivot1.CI_Description = "UPDATED 1";

			var relatedTariffPivot2 = pivot.Children.AddNew();
			relatedTariffPivot2.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			relatedTariffPivot2.CI_TariffNum = tariffNumber2;
			relatedTariffPivot2.CD_CBTPACertificate = "2";
			relatedTariffPivot2.CI_Description = "UPDATED 2";

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			Factory.Save();

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Description = "INV LINE";
			AssertEquals("Before Load Part", 1, invoice.InvoiceLines.Count);

			invoiceLine.JI_PartNo = "Test1";
			AssertEquals("Child Lines Added", 3, invoice.InvoiceLines.Count);

			invoice.InvoiceLines.DeleteAll();
			Factory.Save();

			invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Description = "INV LINE";

			var invoiceLine2 = invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine2.JI_Tariff = "2222333311";
			invoiceLine2.JI_LinePrice = 2020m;
			AssertEquals("Before Load Part", 2, invoice.InvoiceLines.Count);

			invoiceLine.JI_PartNo = "Test1";
			AssertEquals("Child Lines Added", 3, invoice.InvoiceLines.Count);
			AssertEquals("Deleted Lines and Load a Product", false, invoice.InvoiceLines.OfType<JobComInvoiceLine>().Any(x => x.JI_Tariff == "2222333311"));

			invoice.InvoiceLines.DeleteAll();
			Factory.Save();

			invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Description = "INV LINE";

			invoiceLine2 = invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine2.JI_Tariff = "2222333311";
			invoiceLine2.JI_LinePrice = 2020m;

			var invoiceLine3 = invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine3.JI_Tariff = "4444555511";
			invoiceLine3.JI_LinePrice = 4040m;
			AssertEquals("Before Load Part", 3, invoice.InvoiceLines.Count);

			invoiceLine.JI_PartNo = "Test1";
			AssertEquals("Child Lines Added", 3, invoice.InvoiceLines.Count);
			AssertEquals("Should be deleted 2222333311", false, invoice.InvoiceLines.OfType<JobComInvoiceLine>().Any(x => x.JI_Tariff == "2222333311"));
			AssertEquals("Should be deleted 4444555511", false, invoice.InvoiceLines.OfType<JobComInvoiceLine>().Any(x => x.JI_Tariff == "4444555511"));

			invoice.InvoiceLines.DeleteAll();
			Factory.Save();

			invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariffNumber0;

			invoiceLine2 = invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine2.JI_Tariff = tariffNumber1;
			invoiceLine2.JI_LinePrice = 55m;

			invoiceLine3 = invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine3.JI_Tariff = tariffNumber2;
			invoiceLine3.JI_LinePrice = 66m;

			AssertEquals("Before Load Part", 3, invoice.InvoiceLines.Count);

			invoiceLine.JI_PartNo = "Test1";
			AssertEquals("Load Part", 3, invoice.InvoiceLines.Count);

			AssertEquals("child 1: UPDATED 1", "UPDATED 1", invoiceLine2.JI_Description);
			AssertEquals("child 2: UPDATED 2", "UPDATED 2", invoiceLine3.JI_Description);

			AssertEquals("child 1: LinePrice", 55m, invoiceLine2.JI_LinePrice);
			AssertEquals("child 2: LinePrice", 66m, invoiceLine3.JI_LinePrice);
		}

		public void TestRelatedTariffDetailsDefaultFromProductCodeWithValidLineNo()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Test1";
			product.RelatedOrganisations.AddOwner(importer);

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = "1010101011";
			pivot.CD_CBTPACertificate = "A";

			var relatedTariffPivot1 = pivot.Children.AddNew();
			relatedTariffPivot1.CI_ChildType = ClassificationChildTypeList.Codes.Related;
			relatedTariffPivot1.CI_TariffNum = "1010101010";
			relatedTariffPivot1.CD_CBTPACertificate = "1";

			var relatedTariffPivot2 = pivot.Children.AddNew();
			relatedTariffPivot2.CI_ChildType = ClassificationChildTypeList.Codes.Related;
			relatedTariffPivot2.CI_TariffNum = "1010101020";
			relatedTariffPivot2.CD_CBTPACertificate = "2";

			var relatedTariffPivot3 = pivot.Children.AddNew();
			relatedTariffPivot3.CI_ChildType = ClassificationChildTypeList.Codes.Related;
			relatedTariffPivot3.CI_TariffNum = "1010101030";
			relatedTariffPivot3.CD_CBTPACertificate = "3";

			var product2 = Factory.New<OrgSupplierPart>();
			product2.OP_PartNum = "Test2";
			product2.RelatedOrganisations.AddOwner(importer);

			var pivot2 = product2.PivotsForBinding.AddNew();
			pivot2.CI_TariffNum = "1010101022";
			pivot2.CD_CBTPACertificate = "B";

			var pivot2RelatedTariffPivot1 = pivot2.Children.AddNew();
			pivot2RelatedTariffPivot1.CI_ChildType = ClassificationChildTypeList.Codes.Related;
			pivot2RelatedTariffPivot1.CI_TariffNum = "1010101050";
			pivot2RelatedTariffPivot1.CD_CBTPACertificate = "5";

			var pivot2RelatedTariffPivot2 = pivot2.Children.AddNew();
			pivot2RelatedTariffPivot2.CI_ChildType = ClassificationChildTypeList.Codes.Related;
			pivot2RelatedTariffPivot2.CI_TariffNum = "1010101060";
			pivot2RelatedTariffPivot2.CD_CBTPACertificate = "6";

			var pivot2RelatedTariffPivot3 = pivot2.Children.AddNew();
			pivot2RelatedTariffPivot3.CI_ChildType = ClassificationChildTypeList.Codes.Related;
			pivot2RelatedTariffPivot3.CI_TariffNum = "1010101070";
			pivot2RelatedTariffPivot3.CD_CBTPACertificate = "7";

			var pivot2RelatedTariffPivot4 = pivot2.Children.AddNew();
			pivot2RelatedTariffPivot4.CI_ChildType = ClassificationChildTypeList.Codes.Related;
			pivot2RelatedTariffPivot4.CI_TariffNum = "1010101080";
			pivot2RelatedTariffPivot4.CD_CBTPACertificate = "8";

			var pivot2RelatedTariffPivot5 = pivot2.Children.AddNew();
			pivot2RelatedTariffPivot5.CI_ChildType = ClassificationChildTypeList.Codes.Related;
			pivot2RelatedTariffPivot5.CI_TariffNum = "1010101090";
			pivot2RelatedTariffPivot5.CD_CBTPACertificate = "9";

			var product3 = Factory.New<OrgSupplierPart>();
			product3.OP_PartNum = "Test3";
			product3.RelatedOrganisations.AddOwner(importer);

			var pivot3 = product3.PivotsForBinding.AddNew();
			pivot3.CI_TariffNum = "1010101033";
			pivot3.CD_CBTPACertificate = "A";

			var pivot3RelatedTariffPivot1 = pivot3.Children.AddNew();
			pivot3RelatedTariffPivot1.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			pivot3RelatedTariffPivot1.CI_TariffNum = "1010101010";
			pivot3RelatedTariffPivot1.CD_CBTPACertificate = "1";

			var pivot3RelatedTariffPivot2 = pivot3.Children.AddNew();
			pivot3RelatedTariffPivot2.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			pivot3RelatedTariffPivot2.CI_TariffNum = "1010101020";
			pivot3RelatedTariffPivot2.CD_CBTPACertificate = "2";

			var pivot3RelatedTariffPivot3 = pivot3.Children.AddNew();
			pivot3RelatedTariffPivot3.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			pivot3RelatedTariffPivot3.CI_TariffNum = "1010101030";
			pivot3RelatedTariffPivot3.CD_CBTPACertificate = "3";

			var product4 = Factory.New<OrgSupplierPart>();
			product4.OP_PartNum = "Test4";
			product4.RelatedOrganisations.AddOwner(importer);

			var pivot4 = product4.PivotsForBinding.AddNew();
			pivot4.CI_TariffNum = "1010101044";
			pivot4.CD_CBTPACertificate = "B";

			var pivot4RelatedTariffPivot1 = pivot4.Children.AddNew();
			pivot4RelatedTariffPivot1.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			pivot4RelatedTariffPivot1.CI_TariffNum = "1010101020";
			pivot4RelatedTariffPivot1.CD_CBTPACertificate = "5";

			var pivot4RelatedTariffPivot2 = pivot4.Children.AddNew();
			pivot4RelatedTariffPivot2.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			pivot4RelatedTariffPivot2.CI_TariffNum = "1010101030";
			pivot4RelatedTariffPivot2.CD_CBTPACertificate = "6";

			var pivot4RelatedTariffPivot3 = pivot4.Children.AddNew();
			pivot4RelatedTariffPivot3.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			pivot4RelatedTariffPivot3.CI_TariffNum = "1010101040";
			pivot4RelatedTariffPivot3.CD_CBTPACertificate = "7";

			var pivot4RelatedTariffPivot4 = pivot4.Children.AddNew();
			pivot4RelatedTariffPivot4.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			pivot4RelatedTariffPivot4.CI_TariffNum = "1010101050";
			pivot4RelatedTariffPivot4.CD_CBTPACertificate = "8";

			var pivot4RelatedTariffPivot5 = pivot4.Children.AddNew();
			pivot4RelatedTariffPivot5.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			pivot4RelatedTariffPivot5.CI_TariffNum = "1010101060";
			pivot4RelatedTariffPivot5.CD_CBTPACertificate = "9";

			Factory.Save();

			var newFactory = new BusinessObjectFactory(); // use different factory to ensure that ordering works.

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var topInvoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals("topInvoiceLine.JI_LineNo", (ZShort)1, topInvoiceLine.JI_LineNo);
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "Test1";
			AssertEquals("5 invoice lines should have been generated for this part", 5, invoice.InvoiceLines.Count);
			AssertEquals(topInvoiceLine, invoice.InvoiceLines[0]);
			var invoiceLine1 = invoice.InvoiceLines[1];
			var invoiceLine2 = invoice.InvoiceLines[2];
			var invoiceLine3 = invoice.InvoiceLines[3];
			var invoiceLine4 = invoice.InvoiceLines[4];
			AssertInvoiceLinePivot(topInvoiceLine, 1, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty);
			AssertInvoiceLinePivot(invoiceLine1, 2, "1010101011", "A", ZGuid.Empty, ZGuid.Empty);
			AssertInvoiceLinePivot(invoiceLine2, 3, "1010101010", "1", ZGuid.Empty, invoiceLine1.PK);
			AssertInvoiceLinePivot(invoiceLine3, 4, "1010101020", "2", ZGuid.Empty, invoiceLine1.PK);
			AssertInvoiceLinePivot(invoiceLine4, 5, "1010101030", "3", ZGuid.Empty, invoiceLine1.PK);

			invoiceLine.JI_PartNo = "Test2";
			AssertEquals("7 invoice lines should have been generated for this part", 7, invoice.InvoiceLines.Count);
			AssertEquals(topInvoiceLine, invoice.InvoiceLines[0]);
			var invoiceLineB1 = invoice.InvoiceLines[1];
			var invoiceLineB2 = invoice.InvoiceLines[2];
			var invoiceLineB3 = invoice.InvoiceLines[3];
			var invoiceLineB4 = invoice.InvoiceLines[4];
			var invoiceLineB5 = invoice.InvoiceLines[5];
			var invoiceLineB6 = invoice.InvoiceLines[6];
			AssertEquals("invoiceLine2.IsDeleted", true, invoiceLine2.IsDeleted);
			AssertEquals("invoiceLine3.IsDeleted", true, invoiceLine3.IsDeleted);
			AssertEquals("invoiceLine4.IsDeleted", true, invoiceLine4.IsDeleted);
			AssertInvoiceLinePivot(topInvoiceLine, 1, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty);
			AssertInvoiceLinePivot(invoiceLineB1, 2, "1010101022", "B", ZGuid.Empty, ZGuid.Empty);
			AssertInvoiceLinePivot(invoiceLineB2, 3, "1010101050", "5", ZGuid.Empty, invoiceLine1.PK);
			AssertInvoiceLinePivot(invoiceLineB3, 4, "1010101060", "6", ZGuid.Empty, invoiceLine1.PK);
			AssertInvoiceLinePivot(invoiceLineB4, 5, "1010101070", "7", ZGuid.Empty, invoiceLine1.PK);
			AssertInvoiceLinePivot(invoiceLineB5, 6, "1010101080", "8", ZGuid.Empty, invoiceLine1.PK);
			AssertInvoiceLinePivot(invoiceLineB6, 7, "1010101090", "9", ZGuid.Empty, invoiceLine1.PK);

			invoiceLine.JI_PartNo = "Test1";
			AssertEquals("5 invoice lines should have been generated for this part", 5, invoice.InvoiceLines.Count);
			AssertEquals(topInvoiceLine, invoice.InvoiceLines[0]);
			invoiceLine1 = invoice.InvoiceLines[1];
			invoiceLine2 = invoice.InvoiceLines[2];
			invoiceLine3 = invoice.InvoiceLines[3];
			invoiceLine4 = invoice.InvoiceLines[4];
			AssertEquals("invoiceLineB2.IsDeleted", true, invoiceLineB2.IsDeleted);
			AssertEquals("invoiceLineB3.IsDeleted", true, invoiceLineB3.IsDeleted);
			AssertEquals("invoiceLineB4.IsDeleted", true, invoiceLineB4.IsDeleted);
			AssertEquals("invoiceLineB5.IsDeleted", true, invoiceLineB5.IsDeleted);
			AssertEquals("invoiceLineB6.IsDeleted", true, invoiceLineB6.IsDeleted);
			AssertInvoiceLinePivot(topInvoiceLine, 1, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty);
			AssertInvoiceLinePivot(invoiceLine1, 2, "1010101011", "A", ZGuid.Empty, ZGuid.Empty);
			AssertInvoiceLinePivot(invoiceLine2, 3, "1010101010", "1", ZGuid.Empty, invoiceLineB1.PK);
			AssertInvoiceLinePivot(invoiceLine3, 4, "1010101020", "2", ZGuid.Empty, invoiceLineB1.PK);
			AssertInvoiceLinePivot(invoiceLine4, 5, "1010101030", "3", ZGuid.Empty, invoiceLineB1.PK);
			invoiceLine.JI_Tariff = "";
			invoiceLine.JI_PartNo = "Test4";
			AssertEquals("7 invoice lines should have been generated for this part", 7, invoice.InvoiceLines.Count);
			AssertEquals(topInvoiceLine, invoice.InvoiceLines[0]);
			invoiceLineB1 = invoice.InvoiceLines[1];
			invoiceLineB2 = invoice.InvoiceLines[2];
			invoiceLineB3 = invoice.InvoiceLines[3];
			invoiceLineB4 = invoice.InvoiceLines[4];
			invoiceLineB5 = invoice.InvoiceLines[5];
			invoiceLineB6 = invoice.InvoiceLines[6];
			AssertEquals("invoiceLine2.IsDeleted", true, invoiceLine2.IsDeleted);
			AssertEquals("invoiceLine3.IsDeleted", true, invoiceLine3.IsDeleted);
			AssertEquals("invoiceLine4.IsDeleted", true, invoiceLine4.IsDeleted);
			AssertInvoiceLinePivot(topInvoiceLine, 1, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty);
			AssertInvoiceLinePivot(invoiceLineB1, 2, "1010101044", "B", ZGuid.Empty, ZGuid.Empty);
			AssertInvoiceLinePivot(invoiceLineB2, 3, "1010101020", "5", invoiceLineB1.PK, ZGuid.Empty);
			AssertInvoiceLinePivot(invoiceLineB3, 4, "1010101030", "6", invoiceLineB1.PK, ZGuid.Empty);
			AssertInvoiceLinePivot(invoiceLineB4, 5, "1010101040", "7", invoiceLineB1.PK, ZGuid.Empty);
			AssertInvoiceLinePivot(invoiceLineB5, 6, "1010101050", "8", invoiceLineB1.PK, ZGuid.Empty);
			AssertInvoiceLinePivot(invoiceLineB6, 7, "1010101060", "9", invoiceLineB1.PK, ZGuid.Empty);
			invoiceLine.JI_Tariff = "";
			invoiceLine.JI_PartNo = "Test3";
			AssertEquals("5 invoice lines should have been generated for this part", 5, invoice.InvoiceLines.Count);
			AssertEquals(topInvoiceLine, invoice.InvoiceLines[0]);
			invoiceLine1 = invoice.InvoiceLines[1];
			invoiceLine2 = invoice.InvoiceLines[2];
			invoiceLine3 = invoice.InvoiceLines[3];
			invoiceLine4 = invoice.InvoiceLines[4];
			AssertEquals("invoiceLineB2.IsDeleted", true, invoiceLineB2.IsDeleted);
			AssertEquals("invoiceLineB3.IsDeleted", true, invoiceLineB3.IsDeleted);
			AssertEquals("invoiceLineB4.IsDeleted", true, invoiceLineB4.IsDeleted);
			AssertEquals("invoiceLineB5.IsDeleted", true, invoiceLineB5.IsDeleted);
			AssertEquals("invoiceLineB6.IsDeleted", true, invoiceLineB6.IsDeleted);
			AssertInvoiceLinePivot(topInvoiceLine, 1, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty);
			AssertInvoiceLinePivot(invoiceLine1, 2, "1010101033", "A", ZGuid.Empty, ZGuid.Empty);
			AssertInvoiceLinePivot(invoiceLine2, 3, "1010101010", "1", invoiceLineB1.PK, ZGuid.Empty);
			AssertInvoiceLinePivot(invoiceLine3, 4, "1010101020", "2", invoiceLineB1.PK, ZGuid.Empty);
			AssertInvoiceLinePivot(invoiceLine4, 5, "1010101030", "3", invoiceLineB1.PK, ZGuid.Empty);

			var newInvoiceLine = invoice.JobComInvoiceLines.AddNew();
			newInvoiceLine.JI_LineNo = 2;
			invoice.JobComInvoiceLines.Sort(JobComInvoiceLine.Schema.JI_LineNo);
			AssertEquals("6 invoice lines should have been generated for this part", 6, invoice.InvoiceLines.Count);
			AssertEquals(topInvoiceLine, invoice.InvoiceLines[0]);
			AssertEquals(newInvoiceLine, invoice.InvoiceLines[1]);
			invoiceLine1 = invoice.InvoiceLines[2];
			invoiceLine2 = invoice.InvoiceLines[3];
			invoiceLine3 = invoice.InvoiceLines[4];
			invoiceLine4 = invoice.InvoiceLines[5];
			AssertInvoiceLinePivot(topInvoiceLine, 1, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty);
			AssertInvoiceLinePivot(newInvoiceLine, 2, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty);
			AssertInvoiceLinePivot(invoiceLine1, 3, "1010101033", "A", ZGuid.Empty, ZGuid.Empty);
			AssertInvoiceLinePivot(invoiceLine2, 4, "1010101010", "1", invoiceLineB1.PK, ZGuid.Empty);
			AssertInvoiceLinePivot(invoiceLine3, 5, "1010101020", "2", invoiceLineB1.PK, ZGuid.Empty);
			AssertInvoiceLinePivot(invoiceLine4, 6, "1010101030", "3", invoiceLineB1.PK, ZGuid.Empty);

			newInvoiceLine.JI_PartNo = "Test1";
			invoice.JobComInvoiceLines.Sort(JobComInvoiceLine.Schema.JI_LineNo);
			AssertEquals("9 invoice lines should have been generated for this part", 9, invoice.InvoiceLines.Count);
			AssertEquals(topInvoiceLine, invoice.InvoiceLines[0]);
			AssertEquals(newInvoiceLine, invoice.InvoiceLines[1]);
			invoiceLineB1 = invoice.InvoiceLines[2];
			invoiceLineB2 = invoice.InvoiceLines[3];
			invoiceLineB3 = invoice.InvoiceLines[4];
			AssertEquals(invoiceLine1, invoice.InvoiceLines[5]);
			AssertEquals(invoiceLine2, invoice.InvoiceLines[6]);
			AssertEquals(invoiceLine3, invoice.InvoiceLines[7]);
			AssertEquals(invoiceLine4, invoice.InvoiceLines[8]);
			AssertInvoiceLinePivot(topInvoiceLine, 1, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty);
			AssertInvoiceLinePivot(newInvoiceLine, 2, "1010101011", "A", ZGuid.Empty, ZGuid.Empty);
			AssertInvoiceLinePivot(invoiceLineB1, 3, "1010101010", "1", ZGuid.Empty, newInvoiceLine.PK);
			AssertInvoiceLinePivot(invoiceLineB2, 4, "1010101020", "2", ZGuid.Empty, newInvoiceLine.PK);
			AssertInvoiceLinePivot(invoiceLineB3, 5, "1010101030", "3", ZGuid.Empty, newInvoiceLine.PK);
			AssertInvoiceLinePivot(invoiceLine1, 6, "1010101033", "A", ZGuid.Empty, ZGuid.Empty);
			AssertInvoiceLinePivot(invoiceLine2, 7, "1010101010", "1", invoiceLine1.PK, ZGuid.Empty);
			AssertInvoiceLinePivot(invoiceLine3, 8, "1010101020", "2", invoiceLine1.PK, ZGuid.Empty);
			AssertInvoiceLinePivot(invoiceLine4, 9, "1010101030", "3", invoiceLine1.PK, ZGuid.Empty);

			var nonCommittedLine = (JobComInvoiceLine)((IBindingList)declaration.FilteredInvoiceLines).AddNew();
			AssertEquals("nonCommittedLine.JI_LineNo", (ZShort)10, nonCommittedLine.JI_LineNo);
			invoice.JobComInvoiceLines.Sort(JobComInvoiceLine.Schema.JI_LineNo);
			AssertEquals("9 invoice lines should have been generated for this part", 9, invoice.InvoiceLines.Count);
			AssertEquals(topInvoiceLine, invoice.InvoiceLines[0]);
			AssertEquals(newInvoiceLine, invoice.InvoiceLines[1]);
			AssertEquals(invoiceLineB1, invoice.InvoiceLines[2]);
			AssertEquals(invoiceLineB2, invoice.InvoiceLines[3]);
			AssertEquals(invoiceLineB3, invoice.InvoiceLines[4]);
			AssertEquals(invoiceLine1, invoice.InvoiceLines[5]);
			AssertEquals(invoiceLine2, invoice.InvoiceLines[6]);
			AssertEquals(invoiceLine3, invoice.InvoiceLines[7]);
			AssertEquals(invoiceLine4, invoice.InvoiceLines[8]);
			AssertInvoiceLinePivot(topInvoiceLine, 1, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty);
			AssertInvoiceLinePivot(newInvoiceLine, 2, "1010101011", "A", ZGuid.Empty, ZGuid.Empty);
			AssertInvoiceLinePivot(invoiceLineB1, 3, "1010101010", "1", ZGuid.Empty, newInvoiceLine.PK);
			AssertInvoiceLinePivot(invoiceLineB2, 4, "1010101020", "2", ZGuid.Empty, newInvoiceLine.PK);
			AssertInvoiceLinePivot(invoiceLineB3, 5, "1010101030", "3", ZGuid.Empty, newInvoiceLine.PK);
			AssertInvoiceLinePivot(invoiceLine1, 6, "1010101033", "A", ZGuid.Empty, ZGuid.Empty);
			AssertInvoiceLinePivot(invoiceLine2, 7, "1010101010", "1", invoiceLine1.PK, ZGuid.Empty);
			AssertInvoiceLinePivot(invoiceLine3, 8, "1010101020", "2", invoiceLine1.PK, ZGuid.Empty);
			AssertInvoiceLinePivot(invoiceLine4, 9, "1010101030", "3", invoiceLine1.PK, ZGuid.Empty);

			nonCommittedLine.JI_PartNo = "Test1";
			invoice.JobComInvoiceLines.Sort(JobComInvoiceLine.Schema.JI_LineNo);
			AssertEquals("12 invoice lines should have been generated for this part", 12, invoice.InvoiceLines.Count);
			AssertEquals(topInvoiceLine, invoice.InvoiceLines[0]);
			AssertEquals(newInvoiceLine, invoice.InvoiceLines[1]);
			AssertEquals(invoiceLineB1, invoice.InvoiceLines[2]);
			AssertEquals(invoiceLineB2, invoice.InvoiceLines[3]);
			AssertEquals(invoiceLineB3, invoice.InvoiceLines[4]);
			AssertEquals(invoiceLine1, invoice.InvoiceLines[5]);
			AssertEquals(invoiceLine2, invoice.InvoiceLines[6]);
			AssertEquals(invoiceLine3, invoice.InvoiceLines[7]);
			AssertEquals(invoiceLine4, invoice.InvoiceLines[8]);
			invoiceLineB4 = invoice.InvoiceLines[9];
			invoiceLineB5 = invoice.InvoiceLines[10];
			invoiceLineB6 = invoice.InvoiceLines[11];
			AssertInvoiceLinePivot(topInvoiceLine, 1, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty);
			AssertInvoiceLinePivot(newInvoiceLine, 2, "1010101011", "A", ZGuid.Empty, ZGuid.Empty);
			AssertInvoiceLinePivot(invoiceLineB1, 3, "1010101010", "1", ZGuid.Empty, newInvoiceLine.PK);
			AssertInvoiceLinePivot(invoiceLineB2, 4, "1010101020", "2", ZGuid.Empty, newInvoiceLine.PK);
			AssertInvoiceLinePivot(invoiceLineB3, 5, "1010101030", "3", ZGuid.Empty, newInvoiceLine.PK);
			AssertInvoiceLinePivot(invoiceLine1, 6, "1010101033", "A", ZGuid.Empty, ZGuid.Empty);
			AssertInvoiceLinePivot(invoiceLine2, 7, "1010101010", "1", invoiceLine1.PK, ZGuid.Empty);
			AssertInvoiceLinePivot(invoiceLine3, 8, "1010101020", "2", invoiceLine1.PK, ZGuid.Empty);
			AssertInvoiceLinePivot(invoiceLine4, 9, "1010101030", "3", invoiceLine1.PK, ZGuid.Empty);
			AssertInvoiceLinePivot(invoiceLineB4, 11, "1010101010", "1", ZGuid.Empty, nonCommittedLine.PK);
			AssertInvoiceLinePivot(invoiceLineB5, 12, "1010101020", "2", ZGuid.Empty, nonCommittedLine.PK);
			AssertInvoiceLinePivot(invoiceLineB6, 13, "1010101030", "3", ZGuid.Empty, nonCommittedLine.PK);

			((ICancelAddNew)declaration.FilteredInvoiceLines).EndNew(9);
			invoice.JobComInvoiceLines.Sort(JobComInvoiceLine.Schema.JI_LineNo);
			AssertEquals("13 invoice lines should have been generated for this part", 13, invoice.InvoiceLines.Count);
			AssertEquals(topInvoiceLine, invoice.InvoiceLines[0]);
			AssertEquals(newInvoiceLine, invoice.InvoiceLines[1]);
			AssertEquals(invoiceLineB1, invoice.InvoiceLines[2]);
			AssertEquals(invoiceLineB2, invoice.InvoiceLines[3]);
			AssertEquals(invoiceLineB3, invoice.InvoiceLines[4]);
			AssertEquals(invoiceLine1, invoice.InvoiceLines[5]);
			AssertEquals(invoiceLine2, invoice.InvoiceLines[6]);
			AssertEquals(invoiceLine3, invoice.InvoiceLines[7]);
			AssertEquals(invoiceLine4, invoice.InvoiceLines[8]);
			AssertEquals(nonCommittedLine, invoice.InvoiceLines[9]);
			AssertEquals(invoiceLineB4, invoice.InvoiceLines[10]);
			AssertEquals(invoiceLineB5, invoice.InvoiceLines[11]);
			AssertEquals(invoiceLineB6, invoice.InvoiceLines[12]);
			AssertInvoiceLinePivot(topInvoiceLine, 1, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty);
			AssertInvoiceLinePivot(newInvoiceLine, 2, "1010101011", "A", ZGuid.Empty, ZGuid.Empty);
			AssertInvoiceLinePivot(invoiceLineB1, 3, "1010101010", "1", ZGuid.Empty, newInvoiceLine.PK);
			AssertInvoiceLinePivot(invoiceLineB2, 4, "1010101020", "2", ZGuid.Empty, newInvoiceLine.PK);
			AssertInvoiceLinePivot(invoiceLineB3, 5, "1010101030", "3", ZGuid.Empty, newInvoiceLine.PK);
			AssertInvoiceLinePivot(invoiceLine1, 6, "1010101033", "A", ZGuid.Empty, ZGuid.Empty);
			AssertInvoiceLinePivot(invoiceLine2, 7, "1010101010", "1", invoiceLine1.PK, ZGuid.Empty);
			AssertInvoiceLinePivot(invoiceLine3, 8, "1010101020", "2", invoiceLine1.PK, ZGuid.Empty);
			AssertInvoiceLinePivot(invoiceLine4, 9, "1010101030", "3", invoiceLine1.PK, ZGuid.Empty);
			AssertInvoiceLinePivot(nonCommittedLine, 10, "1010101011", "A", ZGuid.Empty, ZGuid.Empty);
			AssertInvoiceLinePivot(invoiceLineB4, 11, "1010101010", "1", ZGuid.Empty, nonCommittedLine.PK);
			AssertInvoiceLinePivot(invoiceLineB5, 12, "1010101020", "2", ZGuid.Empty, nonCommittedLine.PK);
			AssertInvoiceLinePivot(invoiceLineB6, 13, "1010101030", "3", ZGuid.Empty, nonCommittedLine.PK);
		}

		public void TestProductForInvoiceHeaderOrganizations()
		{
			var declarationSupplier = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignor, true));
			var declarationImporter = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignee, true));
			declarationImporter.OH_RL_NKClosestPort = "USLAX";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = declarationSupplier.PK;
			declaration.JE_OH_Importer = declarationImporter.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			var invoice = declaration.Invoices.AddNew();

			var invoiceSupplier = Factory.NewWithValidTestData<OrgHeader>();
			var invoiceImporter = Factory.NewWithValidTestData<OrgHeader>();

			invoice.JZ_OH_Supplier = invoiceSupplier.PK;
			invoice.JZ_OH_Buyer = invoiceImporter.PK;

			AssertEquals("Precondition: Invoice Header should have his own set of Supplier/Importer",
				invoiceSupplier, invoice.Supplier);
			AssertEquals("Precondition: Invoice Header should have his own set of Supplier/Importer",
				invoiceImporter, invoice.Importer);

			var product1 = Factory.New<OrgSupplierPart>();
			product1.OP_PartNum = "Test Product";
			product1.RelatedOrganisations.AddOwner(invoiceImporter);
			product1.RelatedOrganisations.AddSupplier(invoiceSupplier);

			var pivot1 = product1.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot1.CI_TariffNum = "10000000";
			Factory.Save();

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = product1.OP_PartNum;

			AssertNotNull(invoiceLine.Part);
			AssertNotNull(invoiceLine.Pivot);

			AssertEquals("Product should be found and matched, using Invoice Organizations", product1, invoiceLine.Part);
			AssertEquals("Pivot should be found and matched, using Invoice Organizations", pivot1, invoiceLine.Pivot);
		}

		public void TestUS_APHISReqDesc()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			AssertEquals("None", invoiceLine.US_APHISReqDesc);

			declaration.US_EntryType = EntryTypeList.Codes.ReWarehouse;
			AssertEquals(ZString.Empty, invoiceLine.US_APHISReqDesc);
		}

		public void TestJI_FDARequirementDesc()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			AssertEquals("None", invoiceLine.JI_FDARequirementDesc);

			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			AssertEquals(ZString.Empty, invoiceLine.JI_FDARequirementDesc);
		}

		public void TestUS_FSISReqDesc()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			AssertEquals("None", invoiceLine.US_FSISReqDesc);

			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			AssertEquals("None", invoiceLine.US_FSISReqDesc);
		}

		public void TestUS_AMSReqDesc()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			AssertEquals("None", invoiceLine.US_AMSReqDesc);

			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalADDCVDQuotaVisa;
			AssertEquals(ZString.Empty, invoiceLine.US_AMSReqDesc);
		}

		public void TestUS_NOPReqDesc()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			AssertEquals("None", invoiceLine.US_NOPReqDesc);

			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalADDCVDQuotaVisa;
			AssertEquals(ZString.Empty, invoiceLine.US_NOPReqDesc);
		}

		public void TestUS_FWSReqDesc()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			AssertEquals("None", invoiceLine.US_FWSReqDesc);

			declaration.US_EntryType = EntryTypeList.Codes.ReWarehouse;
			AssertEquals(ZString.Empty, invoiceLine.US_FWSReqDesc);
		}

		public void TestUS_LaceyRequirementDesc()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVDQuotaVisa;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			AssertEquals("None", invoiceLine.US_LaceyRequirementDesc);

			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			AssertEquals(ZString.Empty, invoiceLine.US_LaceyRequirementDesc);

			declaration.US_EnableCRL = true;
			declaration.US_EnableENS = false;
			AssertEquals(ZString.Empty, invoiceLine.US_LaceyRequirementDesc);
		}

		public void TestUS_NMFS370ReqDesc()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			AssertEquals("None", invoiceLine.US_NMFS370ReqDesc);

			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			AssertEquals(ZString.Empty, invoiceLine.US_NMFS370ReqDesc);
		}

		public void TestUS_NMFSAMRReqDesc()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			AssertEquals("None", invoiceLine.US_NMFSAMRReqDesc);

			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			AssertEquals(ZString.Empty, invoiceLine.US_NMFSAMRReqDesc);
		}

		public void TestUS_OMCReqDesc()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			AssertEquals("None", invoiceLine.US_OMCReqDesc);

			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			AssertEquals(ZString.Empty, invoiceLine.US_OMCReqDesc);
		}

		public void TestUS_NMFSHMSReqDesc()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			AssertEquals("None", invoiceLine.US_NMFSHMSReqDesc);

			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			AssertEquals(ZString.Empty, invoiceLine.US_NMFSHMSReqDesc);
		}

		public void TestUS_TTBReqDesc()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			AssertEquals("None", invoiceLine.US_TTBReqDesc);

			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			AssertEquals(ZString.Empty, invoiceLine.US_TTBReqDesc);
		}

		public void TestUS_HFCReqDesc()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			AssertEquals("None", invoiceLine.US_HFCReqDesc);

			declaration.US_EntryType = EntryTypeList.Codes.ReWarehouse;
			AssertEquals(ZString.Empty, invoiceLine.US_HFCReqDesc);
		}

		public void TestUS_CPSCReqDesc()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			AssertEquals("None", invoiceLine.US_CPSCReqDesc);

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVDQuotaVisa;
			AssertEquals("None", invoiceLine.US_CPSCReqDesc);
		}

		public void TestUS_DEAReqDesc()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			AssertEquals("None", invoiceLine.US_DEAReqDesc);

			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			AssertEquals(ZString.Empty, invoiceLine.US_DEAReqDesc);
		}

		public void TestUS_NMFSSIMReqDesc()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			AssertEquals("None", invoiceLine.US_NMFSSIMReqDesc);

			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			AssertEquals(ZString.Empty, invoiceLine.US_NMFSSIMReqDesc);
		}

		public void TestUS_PayableMPF_ReadOnly()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			AssertEquals(true, invoiceLine.US_PayableMPFInfo.ReadOnly);
		}

		public void TestUS_SupDuty()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "99034105";
			invoiceLine.JI_Tariff = "4011201005";
			invoiceLine.JI_LinePrice = 10000m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals("JI_Calc_DutyAmount", 4400m, invoiceLine.JI_Calc_DutyAmount);
			AssertEquals("US_Duty", 400m, invoiceLine.US_Duty);
			AssertEquals("US_SupDuty", 4000m, invoiceLine.US_SupDuty);

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVDQuotaVisa;
			invoiceLine.US_ProductExclusion = "02";
			invoiceLine.US_ExclusionNumber = "STL000002";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("US_SupDuty should not be charged for Quota when Product Exclusion No. is entered", 0m, invoiceLine.US_SupDuty);
		}

		public void TestUS_SupDutyForRecon()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
				reconDeclaration.US_EntryFilerCode = "XJ5";

				var entry = reconDeclaration.OriginalEntries.AddNew();
				entry.US_R_DutyRateDate = ZDateTime.Today;
				entry.US_R_CalcOrigDuty = true;
				entry.Invoice.JZ_RX_NKInvoice_Currency = "USD";

				var invoiceLine = entry.Invoice.InvoiceLines.AddNew();
				invoiceLine.US_SupTariff = "99034105";
				invoiceLine.JI_Tariff = "4011201005";
				invoiceLine.JI_LinePrice = 10000m;

				invoiceLine.US_R_OrigSupTariff = "99034105";
				invoiceLine.US_R_OrigTariff = "4011201005";
				invoiceLine.US_R_OrigCV = 1000m;

				reconDeclaration.CalculateDutyFeesForAllEntries();

				AssertEquals("US_Duty", 400m, invoiceLine.US_Duty);
				AssertEquals("US_SupDuty", 4000m, invoiceLine.US_SupDuty);

				AssertEquals("US_R_OrigDuty", 40m, invoiceLine.US_R_OrigDuty);
				AssertEquals("US_R_OrigSupDuty", 400m, invoiceLine.US_R_OrigSupDuty);
			}
		}

		[TestDate(2011, 09, 14)]
		public void TestMPFCalculationForSup()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9903.41.05";
			invoiceLine.JI_Tariff = "4107.11.1020";
			invoiceLine.JI_LinePrice = 10000m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals(21m, invoiceLine.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
		}

		[TestDate(2008, 12, 1)]
		public void TestSupplementaryTariffQuantityFields()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9901.00.52"; // 5.99 cents/litre extra

			AssertEquals("99010052", invoiceLine.US_SupTariff);
			AssertEquals("L", invoiceLine.US_SupUQ1);

			invoiceLine.US_SupQty1 = 5000m;
			AssertEquals(5000m, invoiceLine.US_SupQty1);

			invoiceLine.JI_Tariff = "2909191800";   // 5.5%
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 10000m;

			AssertEquals("KG", invoiceLine.JI_CustomsUnitQty);
			Assert(!invoiceLine.US_SupQty1_ReadOnly);
			AssertEquals(5000m, invoiceLine.US_SupQty1);

			invoiceLine.JI_Tariff = "1702904000";
			AssertEquals("L", invoiceLine.JI_CustomsUnitQty);
			Assert(!invoiceLine.US_SupQty1_ReadOnly);
			AssertEquals(5000m, invoiceLine.US_SupQty1);
			AssertEquals(0m, invoiceLine.JI_CustomsQuantity);

			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000001";
			tariff.UE_Unit1 = "KG";
			tariff.UE_Unit2 = "L";
			tariff.UE_Unit3 = "T";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);

			USCTariff tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "0000000002";
			tariff1.UE_Unit1 = "KG";
			tariff1.UE_Unit2 = "L";
			tariff1.UE_Unit3 = "T";
			tariff1.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff1.UE_DateTo = ZDateTime.Today.AddDays(1);

			USCTariff tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "0000000003";
			tariff2.UE_Unit1 = "DOZ";
			tariff2.UE_Unit2 = "CKG";
			tariff2.UE_Unit3 = "M2";
			tariff2.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff2.UE_DateTo = ZDateTime.Today.AddDays(1);

			invoiceLine.US_SupTariff = "0000000001";
			AssertEquals("KG", invoiceLine.US_SupUQ1);
			AssertEquals("L", invoiceLine.US_SupUQ2);
			AssertEquals("T", invoiceLine.US_SupUQ3);
			invoiceLine.US_SupQty2 = 1250m;
			AssertEquals(1250m, invoiceLine.US_SupQty2);
			invoiceLine.US_SupQty3 = 100m;
			AssertEquals(100m, invoiceLine.US_SupQty3);

			invoiceLine.JI_Tariff = "0000000003";
			AssertEquals("DOZ", invoiceLine.JI_CustomsUnitQty);
			AssertEquals("CKG", invoiceLine.JI_CustomsSecondUnitQty);
			AssertEquals("M2", invoiceLine.JI_CustomsThirdUnitQty);
			AssertEquals(1250m, invoiceLine.US_SupQty2);
			AssertEquals(100m, invoiceLine.US_SupQty3);

			invoiceLine.JI_Tariff = "0000000002";
			AssertEquals("KG", invoiceLine.JI_CustomsUnitQty);
			AssertEquals("L", invoiceLine.JI_CustomsSecondUnitQty);
			AssertEquals("T", invoiceLine.JI_CustomsThirdUnitQty);
			AssertEquals(1250m, invoiceLine.US_SupQty2);
			AssertEquals(100m, invoiceLine.US_SupQty3);
		}

		public void TestSupplementaryTariffFieldForSecondaryInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 500m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			using (declaration.SuspendDefaultingSecondaryTariffLines())
			{
				var line1 = invoice.JobComInvoiceLines.AddNew();
				line1.JI_Tariff = "9102111010";
				line1.US_SupTariff = "9802004040";

				var line2 = line1.AddSecondaryInvoiceLine();
				line2.JI_Tariff = "9102111020";
				AssertEquals("9802004040", line2.US_SupTariff);

				line2.US_SupTariff = "9802004041";
				AssertEquals("9802004041", line2.US_SupTariff);

				var line3 = line1.AddSecondaryInvoiceLine();
				line3.JI_Tariff = "9102111030";
				AssertEquals("9802004040", line3.US_SupTariff);

				line3.US_SupTariff = "9802004041";
				AssertEquals("9802004041", line3.US_SupTariff);

				var line4 = line1.AddSecondaryInvoiceLine();
				line4.JI_Tariff = "9102111040";
				AssertEquals("9802004040", line4.US_SupTariff);

				line4.US_SupTariff = "9802004041";
				AssertEquals("9802004041", line4.US_SupTariff);

				line1.US_SupTariff = "9802004050";
				//Sup Tariff from parent line should be exposed to child lines.
				AssertEquals("9802004050", line2.US_SupTariff);
				AssertEquals("9802004050", line3.US_SupTariff);
				AssertEquals("9802004050", line4.US_SupTariff);

				line1.US_SupTariff = "9813000520";
				//Sup Tariff 9813 from parent line should NOT be exposed to child lines.
				AssertEquals("9802004050", line2.US_SupTariff);
				AssertEquals("9802004050", line3.US_SupTariff);
				AssertEquals("9802004050", line4.US_SupTariff);

				line1.US_SupTariff = "";
				//Sup Tariff from parent line should be exposed to child lines.
				AssertEquals("", line2.US_SupTariff);
				AssertEquals("", line3.US_SupTariff);
				AssertEquals("", line4.US_SupTariff);

				line1.US_SupTariff = "9802004040";

				var line5 = invoice.JobComInvoiceLines.AddNew();
				line5.JI_Tariff = "9102111030";
				line5.JI_ParentID = line1.PK;
				AssertEquals("line5 sup tariff should be set when Parent Id is set", "9802004040", line5.US_SupTariff);

				line1.US_SupTariff = "9813000520";

				var line6 = line1.AddSecondaryInvoiceLine();
				line6.JI_Tariff = "9102111020";
				AssertEquals("", line6.US_SupTariff);

				var line7 = invoice.JobComInvoiceLines.AddNew();
				line7.JI_Tariff = "9102111030";
				line7.JI_ParentID = line1.PK;
				AssertEquals("line7 sup tariff 9813 should NOT be set when Parent Id is set", "", line7.US_SupTariff);
			}
		}

		public void TestTotalNoOfSequences()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;
			declaration.US_EnableAII = true;
			declaration.US_EntryFilerCode = "XJ5";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9901.00.52"; // 5.99 cents/litre extra
			invoiceLine.JI_Tariff = "2909191800";   // 5.5%
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 10000m;

			InvoiceLineGroupingRange range = invoiceLine.LineGroupingRanges[0];

			AssertEquals(1, invoiceLine.TotalNoOfSequences);
			Assert(!invoiceLine.DoesAIIRequireRegeneration);

			invoice.US_GenAIIForSup = true;//set by a db transformation
			AssertEquals(2, invoiceLine.TotalNoOfSequences);
			Assert(invoiceLine.DoesAIIRequireRegeneration);
		}

		[TestDate(2008, 12, 1)]
		public void TestJI_Calc_DutyAmountWithSupTariff()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9901.00.52"; // 5.99 cents/litre extra
			invoiceLine.US_SupQty1 = 1000m;
			invoiceLine.JI_Tariff = "2909.19.1800"; // 5.5%
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_LinePrice = 10000m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals("JI_Calc_DutyAmount should include duty for supplementary tariff.", 609.90m, invoiceLine.JI_Calc_DutyAmount);
		}

		[TestDate(2017, 12, 1)]
		public void TestBackroundingForUS_MPF()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 95238.10m;

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 63333.33m;

			JobComInvoiceLine invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 190000m;

			JobComInvoiceLine invoiceLine4 = declaration.InvoiceLines.AddNew();
			invoiceLine4.JI_LinePrice = 190001m;// Highest
			invoiceLine4.US_SPI = SpecialProgramList.Codes.AU;// MPF exempt

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			DutyTaxEntryFee totalFee = ((ILandedCostHeader)declaration).TotalDutyTaxEntryFeeItems;
			AssertEquals("Total MPF is mapped to SpecialTax1", 485m, totalFee["ST1"]);

			DutyTaxEntryFee lineFee1 = ((IUltimateDistributee)invoiceLine1).LineDutyTaxEntryFeeItems;
			AssertEquals("MPF is apportioned", 132.51m, lineFee1["ST1"]);

			DutyTaxEntryFee lineFee2 = ((IUltimateDistributee)invoiceLine2).LineDutyTaxEntryFeeItems;
			AssertEquals("MPF is apportioned", 88.12m, lineFee2["ST1"]);

			DutyTaxEntryFee lineFee3 = ((IUltimateDistributee)invoiceLine3).LineDutyTaxEntryFeeItems;
			AssertEquals("MPF is apportioned", 264.37m, lineFee3["ST1"]);

			DutyTaxEntryFee lineFee4 = ((IUltimateDistributee)invoiceLine4).LineDutyTaxEntryFeeItems;
			AssertEquals("MPF is apportioned", 0m, lineFee4["ST1"]);
		}

		public void TestCottonFeeForLC()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.US_EnableENS = true;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.No;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 95238.10m;
			invoiceLine1.JI_Tariff = "6104220040";
			invoiceLine1.US_SPI = "AU";
			invoiceLine1.JI_CustomsSecondQuantity = 100m;

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 63333.33m;
			invoiceLine2.JI_Tariff = "6104220040";
			invoiceLine2.US_SPI = "BH";
			invoiceLine2.JI_CustomsSecondQuantity = 150m;

			JobComInvoiceLine invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 63333.33m;
			invoiceLine3.JI_Tariff = "6104220040";
			invoiceLine3.US_SPI = "BH";
			invoiceLine3.JI_CustomsSecondQuantity = 165m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals("PreCondition", invoiceLine2.CusEntryLine, invoiceLine3.CusEntryLine);

			IUltimateDistributee distributee = invoiceLine1;
			AssertEquals("Cotton fee not payable", 0, distributee.Fees.Count());

			distributee = invoiceLine2;
			AssertEquals("Cotton fee payable", 1, distributee.Fees.Count());
			AssertEquals("Cotton fee amount as calculated on the invoice line", 1.40m, distributee.Fees.ElementAt(0).AmountInLocalCurrency);

			distributee = invoiceLine3;
			AssertEquals("Cotton fee payable", 1, distributee.Fees.Count());
			AssertEquals("Cotton fee amount as calculated on the invoice line", 1.40m, distributee.Fees.ElementAt(0).AmountInLocalCurrency);
		}

		public void TestHMFForLC()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.US_EnableENS = true;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 6309.78m;
			invoiceLine1.US_SPI = "AU";

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 524m;
			invoiceLine2.US_SPI = "AU";

			JobComInvoiceLine invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 3160m;
			invoiceLine3.US_SPI = "AU";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			IUltimateDistributee distributee = invoiceLine1;
			AssertEquals("HMF", 1, distributee.Fees.Count());
			AssertEquals(Core.Constants.USCustoms.FeeCodes.HMF, distributee.Fees.ElementAt(0).FeeCode);
			AssertEquals(7.89m, distributee.Fees.ElementAt(0).AmountInLocalCurrency);

			distributee = invoiceLine2;
			AssertEquals("HMF", 1, distributee.Fees.Count());
			AssertEquals(Core.Constants.USCustoms.FeeCodes.HMF, distributee.Fees.ElementAt(0).FeeCode);
			AssertEquals(0.66m, distributee.Fees.ElementAt(0).AmountInLocalCurrency);

			distributee = invoiceLine3;
			AssertEquals("HMF", 1, distributee.Fees.Count());
			AssertEquals(Core.Constants.USCustoms.FeeCodes.HMF, distributee.Fees.ElementAt(0).FeeCode);
			AssertEquals(3.95m, distributee.Fees.ElementAt(0).AmountInLocalCurrency);
		}

		public void TestWhenTaxRateNotOverridenAnyMore()
		{
			JobComInvoiceLine line = InvoiceHeader.InvoiceLines.AddNew();

			line.US_TaxApply = TaxApplyList.Codes.Override;
			line.US_R_OrigTaxApply = TaxApplyList.Codes.Override;

			line.US_TaxRate = 0.9m;
			line.US_R_OrigTaxRate = 0.8m;

			AssertEquals(0.9m, line.US_TaxRate);
			AssertEquals(0.8m, line.US_R_OrigTaxRate);

			line.US_TaxApply = ZString.Empty;
			line.US_R_OrigTaxApply = ZString.Empty;

			AssertEquals("cleared", 0m, line.US_TaxRate);
			AssertEquals("cleared", 0m, line.US_R_OrigTaxRate);
		}

		public void TestIDutyDataOverriddenTaxRate()
		{
			JobComInvoiceLine line = InvoiceHeader.InvoiceLines.AddNew();
			AssertEquals(false, ((IFeeCalculationDataProvider)line).OverriddenTaxRate.HasValue);

			line.US_TaxApply = TaxApplyList.Codes.Override;
			AssertEquals(true, ((IFeeCalculationDataProvider)line).OverriddenTaxRate.HasValue);
			AssertEquals(0m, ((IFeeCalculationDataProvider)line).OverriddenTaxRate.Value);

			line.US_TaxRate = 10m;
			AssertEquals(true, ((IFeeCalculationDataProvider)line).OverriddenTaxRate.HasValue);
			AssertEquals(10m, ((IFeeCalculationDataProvider)line).OverriddenTaxRate.Value);
		}

		public void TestShowDefaultTaxRate()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceLine line = InvoiceHeader.InvoiceLines.AddNew();
			line.JI_Tariff = "2208.20.4000";
			AssertEquals("$3.566322/PFL", line.US_TaxRateS);

			line.US_TaxApply = TaxApplyList.Codes.Override;
			line.US_TaxRateS = AppendixBTaxRateList.Codes.Specify;
			AssertEquals(0m, line.US_TaxRate);

			line.US_TaxRate = 4.5m;
			AssertEquals(4.5m, line.US_TaxRate);

			line.US_TaxApply = TaxApplyList.Codes.Yes;
			AssertEquals("$3.566322/PFL", line.US_TaxRateS);
		}

		public void TestShowDefaultProductClaim()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			declaration.Invoices.AddNew();
			var line = declaration.InvoiceLines.AddNew();

			line.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.F;
			line.US_TaxRateS = AppendixBTaxRateList.Codes.Specify;

			AssertEquals("Should not change as the US_TaxRateS is not CBMS Eligible.", SecondarySpecProgIndicatorList.Codes.F, line.US_SecondarySPI);

			line.US_TaxRateS = AppendixBTaxRateList.CBMAEligible;

			AssertEquals("Should be changed as the US_TaxRateS is CBMS Eligible.", SecondarySpecProgIndicatorList.Codes.C, line.US_SecondarySPI);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			line.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.F;
			line.US_TaxRateS = AppendixBTaxRateList.Codes.Specify;

			line.US_TaxRateS = AppendixBTaxRateList.CBMAEligible;

			AssertEquals("Should not change as the declaration is not ACE.", SecondarySpecProgIndicatorList.Codes.F, line.US_SecondarySPI);
		}

		public void TestShowDefaultTaxRateS()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2022, 01, 10);
			declaration.Invoices.AddNew();
			var line = declaration.InvoiceLines.AddNew();
			line.US_TaxApply = TaxApplyList.Codes.Override;
			line.US_TaxCode = Core.Constants.USCustoms.FeeCodes.OtherExcise;
			line.US_TaxRateS = AppendixBTaxRateList.Codes.Specify;

			line.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.F;

			AssertEquals("IsCBMA23Effective", false, line.IsCBMA23Effective);
			AssertEquals("Should not change as the US_SecondarySPI is C.", AppendixBTaxRateList.Codes.Specify, line.US_TaxRateS);

			line.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.C;

			AssertEquals("Should be changed as the US_SecondarySPI is C.", "B01010", line.US_TaxRateS);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			line.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.F;
			line.US_TaxRateS = AppendixBTaxRateList.Codes.Specify;

			line.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.C;

			AssertEquals("Should not change as the declaration is not ACE.", AppendixBTaxRateList.Codes.Specify, line.US_TaxRateS);
		}

		public void TestUS_CottonFeeExempt()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceLine line = InvoiceHeader.InvoiceLines.AddNew();
			line.JI_Tariff = "6104220040";
			AssertEquals("", line.US_CottonFeeExempt);
			line.US_CottonFeeExempt = YesNoDefaultList.Codes.Yes;
			AssertEquals(YesNoDefaultList.Codes.Yes, line.US_CottonFeeExempt);

			JobComInvoiceLine secondaryLine = line.AddSecondaryInvoiceLine();
			secondaryLine.JI_Tariff = "6104622028";
			AssertEquals("PreCondition", true, secondaryLine.ImportTariff.IsFeeApplicable(Core.Constants.USCustoms.FeeCodes.Cotton));
			AssertEquals(YesNoDefaultList.Codes.Yes, secondaryLine.US_CottonFeeExempt);

			secondaryLine.US_CottonFeeExempt = YesNoDefaultList.Codes.No;
			AssertEquals(YesNoDefaultList.Codes.No, secondaryLine.US_CottonFeeExempt);
		}

		public void TestIsCottonFeeExempt()
		{
			JobComInvoiceLine line = InvoiceHeader.InvoiceLines.AddNew();
			line.JI_Tariff = "9910.61.01";//SG FTA
			AssertEquals("", line.US_CottonFeeExempt);

			JobComInvoiceLine secondaryLine = line.AddSecondaryInvoiceLine();
			secondaryLine.JI_Tariff = "6101.20.0010";//Tariff applicable for a cotton fee
			secondaryLine.US_CottonFeeExempt = YesNoDefaultList.Codes.Yes;

			Assert(line.IsCottonFeeExemptIndicated);
			Assert(secondaryLine.IsCottonFeeExemptIndicated);
		}

		public void TestCombinedCottonFeeExemptIndicators()
		{
			JobComInvoiceLine line = InvoiceHeader.InvoiceLines.AddNew();
			AssertEquals(0, line.CombinedCottonFeeExemptIndicators.Count);
			line.US_CottonFeeExempt = YesNoDefaultList.Codes.No;
			AssertEquals(1, line.CombinedCottonFeeExemptIndicators.Count);

			JobComInvoiceLine secondaryLine = line.AddSecondaryInvoiceLine();
			secondaryLine.US_CottonFeeExempt = YesNoDefaultList.Codes.Yes;
			AssertEquals(2, line.CombinedCottonFeeExemptIndicators.Count);
			AssertEquals(YesNoDefaultList.Codes.No, line.CombinedCottonFeeExemptIndicators[0]);
			AssertEquals(YesNoDefaultList.Codes.Yes, line.CombinedCottonFeeExemptIndicators[1]);

			JobComInvoiceLine secondaryLine2 = line.AddSecondaryInvoiceLine();
			secondaryLine2.US_CottonFeeExempt = YesNoDefaultList.Codes.No;
			AssertEquals(2, line.CombinedCottonFeeExemptIndicators.Count);
			AssertEquals(YesNoDefaultList.Codes.No, line.CombinedCottonFeeExemptIndicators[0]);
			AssertEquals(YesNoDefaultList.Codes.Yes, line.CombinedCottonFeeExemptIndicators[1]);

			line.US_CottonFeeExempt = ZString.Empty;
			AssertEquals(2, line.CombinedCottonFeeExemptIndicators.Count);
			AssertEquals(YesNoDefaultList.Codes.Yes, line.CombinedCottonFeeExemptIndicators[0]);
			AssertEquals(YesNoDefaultList.Codes.No, line.CombinedCottonFeeExemptIndicators[1]);

			AssertEquals(1, secondaryLine.CombinedCottonFeeExemptIndicators.Count);
			AssertEquals(YesNoDefaultList.Codes.Yes, secondaryLine.CombinedCottonFeeExemptIndicators[0]);
		}

		public void TestCottonFeeExemptIndicatorDefaultedFromProductWithSupTariff()
		{
			var tariffRule = Factory.LoadTop1<USCTariffRule>(new ZQuery(USCTariffRuleSchema.U1_RuleCode, TariffRuleList.Codes.CottonFeeExemption));
			if (tariffRule == null)
			{
				tariffRule = Factory.New<USCTariffRule>();
				tariffRule.U1_DateFrom = ZDateTime.BrettsBirthday;
				tariffRule.U1_RuleCode = TariffRuleList.Codes.CottonFeeExemption;
				tariffRule.U1_Tariff = "9802008044";

				var tariff = Factory.New<USCTariff>();
				tariff.UE_Tariff = "9802008044";
				tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
				tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			}

			importer = Factory.NewWithValidTestData<OrgHeader>();
			supplier = Factory.NewWithValidTestData<OrgHeader>();

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "COTTON TEST";
			product.OP_Desc = "BOYS SHIRTS";
			product.RelatedOrganisations.AddOwner(importer);
			product.RelatedOrganisations.AddSupplier(supplier);

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "6105100030";
			pivot.CI_SupplementalTariff = "9802008044";
			pivot.CD_CottonFeeExempt = YesNoDefaultList.Codes.Yes;

			var childPivot1 = pivot.Children.AddNew();
			childPivot1.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			childPivot1.CI_TariffNum = "6211320025";
			childPivot1.CD_CottonFeeExempt = YesNoDefaultList.Codes.Yes;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Supplier = supplier.PK;
			invoice.JZ_InvoiceAmount = 10000m;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = product.OP_PartNum;
			AssertNotNull(invoiceLine.Part);
			AssertEquals("Should be 2 Invoice Lines, when Product selected", 2, invoice.JobComInvoiceLines.Count);
			AssertEquals("Tariff", "6105100030", invoiceLine.JI_Tariff);
			AssertEquals("Sup Tariff", "9802008044", invoiceLine.US_SupTariff);
			AssertEquals("Cotton Fee Exemption indicator", YesNoDefaultList.Codes.Yes, invoiceLine.US_CottonFeeExempt);

			var childInvoiceLine = invoice.JobComInvoiceLines[1];
			AssertEquals("Tariff", "6211320025", childInvoiceLine.JI_Tariff);
			AssertEquals("Sup Tariff get from Parent Line", "", childInvoiceLine.US_SupTariff);
			AssertEquals("Cotton Fee Exemption is taken from product to populate the child lines", YesNoDefaultList.Codes.Yes, childInvoiceLine.US_CottonFeeExempt);
		}

		public void TestADD_CVDRate()
		{
			USCACCase uscCase = Factory.New<USCACCase>();
			uscCase.U5_CaseNumber = "A549822023";

			USCACCaseRate rate1 = uscCase.CaseRates.AddNew();
			rate1.U6_EffectiveDate = new ZDateTime(2009, 9, 15);
			rate1.U6_AdValoremRate = 0.0318m;

			USCACCaseRate rate2 = uscCase.CaseRates.AddNew();
			rate2.U6_EffectiveDate = new ZDateTime(2009, 9, 16);
			rate2.U6_AdValoremRate = 0.0471m;

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;
			declaration.US_EntryDate = new ZDateTime(2009, 9, 11);
			declaration.US_ITDate = new ZDateTime(2009, 9, 14);//should not affect ADD/CVD
			declaration.US_EstimatedEntryDate = new ZDateTime(2009, 9, 17);

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0306.13.0012";
			invoiceLine.US_UC_NKCountryOfOrigin = "TH";
			invoiceLine.US_ADDCaseNo = "A549822023";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			MessageBuilders.ICusEntryLine entryLine = invoiceLine.CusEntryLine;
			AssertEquals("ADD rate", 0.0471m, entryLine.ADDDepositRate);
		}

		public void TestIRegistryAccessingSupporterMembers()
		{
			var company = Factory.New<GlbCompany>();
			var branch = company.Branches.AddNew();

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			declaration.JE_GB = ZGuid.Empty;
			AssertEquals(GlbCompany.CurrentCompany.PK.ToGuid(), invoiceLine.RegistryCompanyPK);
			AssertEquals(GlbBranch.CurrentBranch.PK.ToGuid(), invoiceLine.RegistryBranchPK);

			declaration.JE_GB = branch.PK;
			AssertEquals(company.PK.ToGuid(), invoiceLine.RegistryCompanyPK);
			AssertEquals(branch.PK.ToGuid(), invoiceLine.RegistryBranchPK);

			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			AssertEquals(GlbCompany.CurrentCompany.PK.ToGuid(), invoiceLine.RegistryCompanyPK);
			AssertEquals(GlbBranch.CurrentBranch.PK.ToGuid(), invoiceLine.RegistryBranchPK);

			invoice.JZ_JE = ZGuid.Empty;
			invoice.JZ_GB = ZGuid.Empty;
			AssertEquals(GlbCompany.CurrentCompany.PK.ToGuid(), invoiceLine.RegistryCompanyPK);
			AssertEquals(GlbBranch.CurrentBranch.PK.ToGuid(), invoiceLine.RegistryBranchPK);

			invoice.JZ_GB = branch.PK;
			AssertEquals(company.PK.ToGuid(), invoiceLine.RegistryCompanyPK);
			AssertEquals(branch.PK.ToGuid(), invoiceLine.RegistryBranchPK);

			invoice.JZ_GB = GlbBranch.CurrentBranch.PK;
			AssertEquals(GlbCompany.CurrentCompany.PK.ToGuid(), invoiceLine.RegistryCompanyPK);
			AssertEquals(GlbBranch.CurrentBranch.PK.ToGuid(), invoiceLine.RegistryBranchPK);
		}

		public void TestClearADD_CVDIndicators()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8708806590";
			invoiceLine.US_SupTariff = "6205900710";
			invoiceLine.US_UC_NKCountryOfOrigin = "DE";
			invoiceLine.US_ADD_NA = true;
			invoiceLine.US_CVD_NA = true;

			invoiceLine.JI_Tariff = "8708806580";//different one
			Assert("Cleared so that users can review the applicability with a new tariff and C/O", !invoiceLine.US_ADD_NA);
			Assert("Cleared so that users can review the applicability with a new tariff and C/O", !invoiceLine.US_CVD_NA);

			invoiceLine.US_ADD_NA = true;
			invoiceLine.US_CVD_NA = true;

			invoiceLine.US_SupTariff = "6205900720";
			Assert("ADD/CVD Indicators should not be cleared when changing the sup tariff", invoiceLine.US_ADD_NA);
			Assert("ADD/CVD Indicators should not be cleared when changing the sup tariff", invoiceLine.US_CVD_NA);
		}

		public void TestWhenParentLineBecomesChild()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			declaration.Invoices.AddNew();

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();

			JobComInvoiceLine invoiceLine3 = invoiceLine2.AddSecondaryInvoiceLine();
			AssertEquals(1, invoiceLine2.ChildLines.Count());

			Assert("PreCondition", !invoiceLine.IsParentLine);
			Assert("PreCondition", invoiceLine2.IsParentLine);
			Assert("PreCondition", !invoiceLine3.IsParentLine);

			invoiceLine2.JI_ParentID = invoiceLine.PK;
			Assert("Is not a parent line any more", !invoiceLine2.IsParentLine);
			AssertEquals("Invoice line 2 points to invoice line", invoiceLine.PK, invoiceLine2.JI_ParentID);
			AssertEquals("Invoice line 3 points to invoice line", invoiceLine.PK, invoiceLine3.JI_ParentID);

			AssertEquals(2, invoiceLine.ChildLines.Count());
		}

		public void TestChildLinesRefreshOnChildDelete()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			invoiceLine2.US_VisaNo = "VI2342";
			AssertEquals("VI2342", invoiceLine1.US_VisaNo_Effective);
			AssertEquals(1, invoiceLine1.ChildLines.Count());
			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_ParentID = invoiceLine1.PK;
			invoiceLine2.Delete();
			invoiceLine3.Delete();
			AssertEquals("", invoiceLine1.US_VisaNo_Effective);
			AssertEquals(0, invoiceLine1.ChildLines.Count());
		}

		[ExpectNoExceptions]
		public void TestChildLinesRefreshOnDelete()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_AutoWeightApportion = true;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_Weight = 1;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1;

			var invoiceLine1 = invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine1.JI_LinePrice = 1;

			var invoiceLine2 = invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine2.JI_LinePrice = 1;

			invoiceLine.Delete();
		}

		[TestDate(2009, 1, 1)]
		public void TestDefaultMandatoryFees()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_Tariff = USCTariff.CottonFeeApplicable;
			AssertNotNull("Cotton fee is populated", invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.Cotton));

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNotNull("MPF is populated", invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));

			invoiceLine.JI_Tariff = "2204212000";//wine
			AssertNull("Cotton fee is deleted", invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.Cotton));
			AssertNotNull("wine fee is populated", invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.Wines));
			AssertNotNull("MPF is retained as it is not related to tariff numbers", invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));

			FeeCusCodeData winefee = invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.Wines);
			winefee.CY_IsOverridden = true;

			invoiceLine.JI_Tariff = "2204298000";//wine fee, but not mandatory
			AssertEquals("wine fee is retained", winefee, invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.Wines));
			AssertNotNull("MPF is retained as it is not related to tariff numbers", invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
		}

		public void TestDefaultTariffDescription()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = USCTariff.DOTMayBeApplicable;
			AssertEquals("", invoiceLine.JI_Description);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.US_TariffType = TariffTypeList.Codes.HTS;

			invoiceLine.JI_Tariff = USCTariff.DOTMayBeApplicable;
			AssertEquals("", invoiceLine.JI_Description);
		}

		public void TestICusAddInfoTypeSupporter()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			ICusAddInfoTypeSupporter supporter = invoiceLine;
			supporter.AssertType(typeof(AIILine), CusAddInfoTypeAttribute.Codes.USAIILine);
			supporter.AssertType(typeof(FDA), CusAddInfoTypeAttribute.Codes.USFDA);
			supporter.AssertType(typeof(ACEFDA), CusAddInfoTypeAttribute.Codes.USACEFDA);
			supporter.AssertType(typeof(FCC), CusAddInfoTypeAttribute.Codes.USFCC);
			supporter.AssertType(typeof(DOT), CusAddInfoTypeAttribute.Codes.USDOT);
			supporter.AssertType(typeof(PGA), CusAddInfoTypeAttribute.Codes.USPGACommon);
			supporter.AssertType(typeof(DrawbackNAFTA), CusAddInfoTypeAttribute.Codes.USDrawbackNAFTA);
			supporter.AssertType(typeof(NMFSLine), CusAddInfoTypeAttribute.Codes.USNMFSLine);
			supporter.AssertType(typeof(TTBLine), CusAddInfoTypeAttribute.Codes.USTTBLine);
			supporter.AssertType(typeof(OMCHeader), CusAddInfoTypeAttribute.Codes.USOMCHeader);
			supporter.AssertType(typeof(APHISHeader), CusAddInfoTypeAttribute.Codes.USAPHISHeader);
			supporter.AssertType(typeof(FWSHeader), CusAddInfoTypeAttribute.Codes.USFWSHeader);
			supporter.AssertType(typeof(NHTSAHeader), CusAddInfoTypeAttribute.Codes.USNHTSAHeader);
			supporter.AssertType(typeof(Pesticide), CusAddInfoTypeAttribute.Codes.USPesticide);
			supporter.AssertType(typeof(CPSCHeader), CusAddInfoTypeAttribute.Codes.USCPSCHeader);
			supporter.AssertType(typeof(DEAHeader), CusAddInfoTypeAttribute.Codes.USDEAHeader);
			supporter.AssertType(typeof(USHFCHeader), CusAddInfoTypeAttribute.Codes.USHFCHeader);
			supporter.AssertType(typeof(DrawbackOtherFee), CusAddInfoTypeAttribute.Codes.USDrawbackOtherFee);
			supporter.AssertType(typeof(DrawbackAdditionalImportTariffNumber), CusAddInfoTypeAttribute.Codes.USDrawbackAdditionalImportTariffNumber);
			supporter.AssertType(null, "ZZ!");

			var lineGroup = invoiceLine.LineGroupingRanges.AddNew(1, 1);
			var aiiLine = invoiceLine.AIILines.AddNew(lineGroup);
			aiiLine.US_98InvCurrPerUnit = 1m;
			var fda = invoiceLine.FDAs.AddNew();
			fda.US_ContainerDim1 = 1m;
			var fcc = invoiceLine.FCCs.AddNew();
			fcc.US_FCCCommercialDesc = "1";
			var dot = invoiceLine.DOTs.AddNew();
			dot.US_DOTBondSuretyCode = "1";
			var pga = invoiceLine.LaceyActLines.AddNew();
			pga.US_InvCurrPGAValue = 1m;
			var omcHeader = invoiceLine.OMCHeaders.AddNew();
			omcHeader.US_NetWeight = 100m;
			var drawback = invoiceLine.DrawbackNAFTAs.AddNew();
			drawback.US_DRWEquivalentUSDollarAmountOfNAFTACountryDuty = 1m;
			var nmfsLine = invoiceLine.NMFSLines.AddNew();
			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes._370;
			var aceFDA = invoiceLine.ACE_FDALines.AddNew();
			aceFDA.US_BrandName = "SD";
			var ttbLine = invoiceLine.TTBLines.AddNew();
			ttbLine.US_ProgramCode = TTBProgramCodeList.Codes.Beverage;
			var aphisHeader = invoiceLine.APHISHeaders.AddNew();
			aphisHeader.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			var fwsHeader = invoiceLine.FWSHeaders.AddNew();
			fwsHeader.US_ProcessingCode = FWSProcessingCodeList.Codes.EDS;
			var nHTSAHeader = invoiceLine.NHTSALines.AddNew();
			nHTSAHeader.US_NHTDOTSuretyCode = "891";
			var pesticide = invoiceLine.PSTLines.AddNew();
			pesticide.US_BrandName = "BRAND";
			var hfcHeader = invoiceLine.USHFCHeaders.AddNew();
			hfcHeader.US_ASHRAENumber = "112233";
			var cpscHeader = invoiceLine.CPSCHeaders.AddNew();
			cpscHeader.US_ProcessingCode = CPSCProcessingCodeList.Codes.REF;
			var deaHeader = invoiceLine.DEAHeaders.AddNew();
			deaHeader.US_PermitNumber = "1234567";
			var drawbackOtherFee = invoiceLine.DrawbackOtherFees.AddNew();
			drawbackOtherFee.US_FeeType = DrawbackOtherFeeTypesList.Codes.BeefFee;
			var drawbackAdditionalImpTariff = invoiceLine.DrawbackAdditionalImportTariffNumbers.AddNew();
			drawbackAdditionalImpTariff.US_FormattedTariff = "1101.10.0001";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			CusAddInfo addInfo = newFactory.Load<CusAddInfo>(aiiLine.PK);
			AssertEquals(typeof(AIILine), addInfo.GetType());
			addInfo = newFactory.Load<CusAddInfo>(fda.PK);
			AssertEquals(typeof(FDA), addInfo.GetType());
			addInfo = newFactory.Load<CusAddInfo>(fcc.PK);
			AssertEquals(typeof(FCC), addInfo.GetType());
			addInfo = newFactory.Load<CusAddInfo>(dot.PK);
			AssertEquals(typeof(DOT), addInfo.GetType());
			addInfo = newFactory.Load<CusAddInfo>(pga.PK);
			AssertEquals(typeof(PGA), addInfo.GetType());
			addInfo = newFactory.Load<CusAddInfo>(drawback.PK);
			AssertEquals(typeof(DrawbackNAFTA), addInfo.GetType());
			addInfo = newFactory.Load<CusAddInfo>(nmfsLine.PK);
			AssertEquals(typeof(NMFSLine), addInfo.GetType());
			addInfo = newFactory.Load<CusAddInfo>(aceFDA.PK);
			AssertEquals(typeof(ACEFDA), addInfo.GetType());
			addInfo = newFactory.Load<CusAddInfo>(ttbLine.PK);
			AssertEquals(typeof(TTBLine), addInfo.GetType());
			addInfo = newFactory.Load<CusAddInfo>(aphisHeader.PK);
			AssertEquals(typeof(APHISHeader), addInfo.GetType());
			addInfo = newFactory.Load<CusAddInfo>(omcHeader.PK);
			AssertEquals(typeof(OMCHeader), addInfo.GetType());
			addInfo = newFactory.Load<CusAddInfo>(fwsHeader.PK);
			AssertEquals(typeof(FWSHeader), addInfo.GetType());
			addInfo = newFactory.Load<CusAddInfo>(nHTSAHeader.PK);
			AssertEquals(typeof(NHTSAHeader), addInfo.GetType());
			addInfo = newFactory.Load<CusAddInfo>(pesticide.PK);
			AssertEquals(typeof(Pesticide), addInfo.GetType());
			addInfo = newFactory.Load<CusAddInfo>(hfcHeader.PK);
			AssertEquals(typeof(USHFCHeader), addInfo.GetType());
			addInfo = newFactory.Load<CusAddInfo>(cpscHeader.PK);
			AssertEquals(typeof(CPSCHeader), addInfo.GetType());
			addInfo = newFactory.Load<CusAddInfo>(deaHeader.PK);
			AssertEquals(typeof(DEAHeader), addInfo.GetType());
			addInfo = newFactory.Load<CusAddInfo>(drawbackOtherFee.PK);
			AssertEquals(typeof(DrawbackOtherFee), addInfo.GetType());
			addInfo = newFactory.Load<CusAddInfo>(drawbackAdditionalImpTariff.PK);
			AssertEquals(typeof(DrawbackAdditionalImportTariffNumber), addInfo.GetType());
		}

		public void TestICusCodeDataTypeSupporter()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			ICusCodeDataTypeSupporter supporter = invoiceLine;
			supporter.AssertType(typeof(InvoiceLineGroupingRange), CusCodeDataTypeList.Codes.InvoiceLineNumberRange);
			supporter.AssertType(typeof(FeeCusCodeData), CusCodeDataTypeList.Codes.Fee);
			supporter.AssertType(typeof(ReconEntryOriginalCharge), CusCodeDataTypeList.Codes.ReconEntryOriginalCharge);
			supporter.AssertType(typeof(ReconRefundedCharge), CusCodeDataTypeList.Codes.ReconRefundedCharge);
			supporter.AssertType(typeof(LicenceAndPermit), CusCodeDataTypeList.Codes.LicenceAndPermit);
			supporter.AssertType(typeof(CensusWarningOverride), CusCodeDataTypeList.Codes.CensusWarningOverride);
			supporter.AssertType(typeof(DrawbackAdditionalExportTariffNumber), CusCodeDataTypeList.Codes.DrawbackAdditionalExportTariffNumber);
			supporter.AssertType(null, "ZZ!");

			var range = invoiceLine.LineGroupingRanges.AddNew();
			range.US_EndSequenceNo = 1;
			var fee = invoiceLine.FeeCusCodes.AddNew();
			fee.CY_Code = "D";
			var charge = invoiceLine.ReconOriginalCharges.AddNew();
			charge.CY_Code = "D";
			var permit = invoiceLine.LicenceAndPermits.AddNew();
			permit.CY_Code = "D";
			var warning = invoiceLine.CensusWarningOverrides.AddNew();
			warning.CY_Code = "D";
			var drawbackExp = invoiceLine.DrawbackAdditionalExportTariffNumbers.AddNew();
			drawbackExp.CY_Code = "D";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var codeData = newFactory.Load<CusCodeData>(range.PK);
			AssertEquals(typeof(InvoiceLineGroupingRange), codeData.GetType());
			codeData = newFactory.Load<CusCodeData>(fee.PK);
			AssertEquals(typeof(FeeCusCodeData), codeData.GetType());
			codeData = newFactory.Load<CusCodeData>(charge.PK);
			AssertEquals(typeof(ReconEntryOriginalCharge), codeData.GetType());
			codeData = newFactory.Load<CusCodeData>(permit.PK);
			AssertEquals(typeof(LicenceAndPermit), codeData.GetType());
			codeData = newFactory.Load<CusCodeData>(warning.PK);
			AssertEquals(typeof(CensusWarningOverride), codeData.GetType());
			codeData = newFactory.Load<CusCodeData>(drawbackExp.PK);
			AssertEquals(typeof(DrawbackAdditionalExportTariffNumber), codeData.GetType());
		}

		public void TestIsDomesticMerchandise()
		{
			InvoiceLine.US_ZoneStatus = ZoneStatusList.Codes.Domestic;
			AssertEquals(false, InvoiceLine.IsDomesticMerchandise);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			AssertEquals(true, InvoiceLine.IsDomesticMerchandise);
		}

		public void TestUS_AESOriginIndicator()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.US_AESOriginIndicator = AESOriginIndicatorList.Codes.Foreign;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8211100000";
			AssertEquals("Should return effective value", AESOriginIndicatorList.Codes.Foreign, invoiceLine.US_AESOriginIndicator);

			invoiceLine.US_AESOriginIndicator = AESOriginIndicatorList.Codes.Domestic;
			AssertEquals("Should return line value", AESOriginIndicatorList.Codes.Domestic, invoiceLine.US_AESOriginIndicator);

			invoiceLine.US_AESOriginIndicator = "";
			AssertEquals("Should return effective value again", AESOriginIndicatorList.Codes.Foreign, invoiceLine.US_AESOriginIndicator);

			invoiceLine.US_ExportCode = ExportInformationCodeList.Codes.HH;
			AssertEquals("For household goods, OriginIndicator should return the line value to allow correct entry of space (no indicator for this code)", "", invoiceLine.US_AESOriginIndicator);

			invoiceLine.US_AESOriginIndicator = "#";
			AssertEquals("For household goods, should return line value entered even if invalid value", "#", invoiceLine.US_AESOriginIndicator);

			invoiceLine.US_AESOriginIndicator = "";
			invoiceLine.US_ExportCode = ExportInformationCodeList.Codes.CH;
			AssertEquals("Should return effective value again", AESOriginIndicatorList.Codes.Foreign, invoiceLine.US_AESOriginIndicator);
		}

		public void TestFeesForLandedCosting()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1000m;
			invoiceLine.US_SPI = "AU";//MPF becomes exempt
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("HMF exempt", 0m, declaration.ActiveEntryHeaders.EntrySummaryEntry.HMFAmountForEntry);

			IUltimateDistributee line = invoiceLine;
			AssertEquals(0, line.Fees.Count());

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 2000m;
			invoiceLine2.US_SPI = "AU";//MPF becomes exempt
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("HMF NOT exempt any more", 3.750m, declaration.ActiveEntryHeaders.EntrySummaryEntry.HMFAmountForEntry);

			ICustomsFee fee = line.Fees.ElementAt(0);
			AssertEquals(Core.Constants.USCustoms.FeeCodes.HMF, fee.FeeCode);
			AssertEquals(1.25m, fee.AmountInLocalCurrency);

			line = invoiceLine2;
			fee = line.Fees.ElementAt(0);
			AssertEquals(Core.Constants.USCustoms.FeeCodes.HMF, fee.FeeCode);
			AssertEquals(2.50m, fee.AmountInLocalCurrency);
		}

		public void TestFreightAndInsuranceWhenDutiableFreightExists()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = TermsOfDeliveryList.Codes.CIF;
			invoice.JZ_InvoiceAmount = 13000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 13000m;

			InvoiceCharge freight = invoice.Charges.AddNew();
			freight.J7_ChargeType = Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight;
			freight.J7_Amount = 100m;
			freight.J7_IsDutiable = true;

			InvoiceCharge insurance = invoice.Charges.AddNew();
			insurance.J7_ChargeType = Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance;
			insurance.J7_Amount = 10m;
			insurance.J7_IsDutiable = true;
			declaration.ResumeApportionment();
			AssertEquals("Freight", 100m, invoiceLine.JI_Calc_FreightInInvoiceCurr);
			AssertEquals("Insurance", 10m, invoiceLine.JI_Calc_InsuranceInInvoiceCurr);

			InvoiceCharge adjustingFreight = invoice.Charges.AddNew();
			adjustingFreight.J7_ChargeType = Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight;
			adjustingFreight.J7_Amount = 200m;
			adjustingFreight.J7_IsDutiable = false;
			adjustingFreight.J7_AdjustedCharge = true;
			declaration.ResumeApportionment();
			AssertEquals("Freight", 200m, invoiceLine.JI_Calc_FreightInInvoiceCurr);
			AssertEquals("Insurance", 10m, invoiceLine.JI_Calc_InsuranceInInvoiceCurr);

			InvoiceCharge adjustingInsurance = invoice.Charges.AddNew();
			adjustingInsurance.J7_ChargeType = Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance;
			adjustingInsurance.J7_Amount = 20m;
			adjustingInsurance.J7_IsDutiable = false;
			adjustingInsurance.J7_AdjustedCharge = true;
			declaration.ResumeApportionment();
			AssertEquals("Freight", 200m, invoiceLine.JI_Calc_FreightInInvoiceCurr);
			AssertEquals("Insurance", 20m, invoiceLine.JI_Calc_InsuranceInInvoiceCurr);

			adjustingFreight.J7_AdjustedCharge = false;
			adjustingInsurance.J7_AdjustedCharge = false;
			declaration.ResumeApportionment();
			AssertEquals("Freight", 300m, invoiceLine.JI_Calc_FreightInInvoiceCurr);
			AssertEquals("Insurance", 30m, invoiceLine.JI_Calc_InsuranceInInvoiceCurr);
		}

		public void TestValueForADD_CVDForNon99Or98Parent()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8211100000";
			invoiceLine.JI_InvoiceQuantity = 4000m;
			invoiceLine.JI_CustomsQuantity = 4000m;
			invoiceLine.JI_LinePrice = 0m;

			JobComInvoiceLine line2 = invoiceLine.AddSecondaryInvoiceLine();
			line2.JI_Tariff = "8211929045"; // .4c per unit + 6.1% = 20000 *.004 + 24000 * .061 = $1544
			line2.JI_CustomsQuantity = 16000m;
			line2.JI_LinePrice = 16000m;
			line2.US_ADDCaseNo = "A";

			JobComInvoiceLine line3 = invoiceLine.AddSecondaryInvoiceLine();
			line3.JI_Tariff = "8211930030"; // .3c per unit + 5.4% = (20000 *.03) + (24000 * .054) = $1896
			line3.JI_CustomsQuantity = 4000m;
			line3.JI_LinePrice = 8000m;
			line3.US_CVDCaseNo = "C";

			AssertEquals("ValueForADD for the parent", 0m, invoiceLine.ValueForADD);
			AssertEquals("ValueForCVD for the parent", 0m, invoiceLine.ValueForCVD);

			AssertEquals("ValueForADD for the secondary", 16000m, line2.ValueForADD);
			AssertEquals("ValueForCVD for the secondary", 0m, line2.ValueForCVD);

			AssertEquals("ValueForADD for the secondary", 0m, line3.ValueForADD);
			AssertEquals("ValueForCVD for the secondary", 8000m, line3.ValueForCVD);
		}

		public void TestValueForADD_CVDFor99Or98Parent()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9802";
			invoiceLine.US_98GoodsValue = 250m;
			invoiceLine.JI_LinePrice = 1000m;

			invoiceLine.US_CVDCaseNo = "C";
			invoiceLine.US_ADDCaseNo = "";
			AssertEquals("ValueForADD", 0m, invoiceLine.ValueForADD);
			AssertEquals("ValueForCVD", 1250m, invoiceLine.ValueForCVD);

			invoiceLine.US_CVDCaseNo = "";
			invoiceLine.US_ADDCaseNo = "A";
			AssertEquals("ValueForADD", 1250m, invoiceLine.ValueForADD);
			AssertEquals("ValueForCVD", 0m, invoiceLine.ValueForCVD);
		}

		public void TestValueForADD_CVDForWatchRepair()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			using (declaration.SuspendDefaultingSecondaryTariffLines())
			{
				invoiceHeader.JZ_InvoiceAmount = 3406m;
				JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine.US_SupTariff = "9802004040";    // repairs
				invoiceLine.US_98GoodsValue = 3406m;
				invoiceLine.JI_Tariff = "9102111010";
				invoiceLine.JI_LinePrice = 5258m;
				invoiceLine.JI_CustomsQuantity = 1000m;
				invoiceLine.US_ADDCaseNo = "A";

				JobComInvoiceLine secondRepairLine = invoiceLine.AddSecondaryInvoiceLine();
				secondRepairLine.US_SupTariff = "9802004040";   // repairs
				secondRepairLine.JI_Tariff = "9102111020";
				secondRepairLine.JI_LinePrice = 2619m;
				secondRepairLine.JI_CustomsQuantity = 1000m;

				JobComInvoiceLine thirdRepairLine = invoiceLine.AddSecondaryInvoiceLine();
				thirdRepairLine.US_SupTariff = "9802004040";    // repairs
				thirdRepairLine.US_CVDCaseNo = "C";
				thirdRepairLine.JI_Tariff = "9102111030";
				thirdRepairLine.JI_LinePrice = 1344m;
				thirdRepairLine.JI_CustomsQuantity = 1000m;

				JobComInvoiceLine fourthRepairLine = invoiceLine.AddSecondaryInvoiceLine();
				fourthRepairLine.US_SupTariff = "9802004040";   // repairs
				fourthRepairLine.JI_Tariff = "9102111040";
				fourthRepairLine.JI_LinePrice = 204m;
				fourthRepairLine.JI_CustomsQuantity = 1000m;

				AssertEquals("ValueForADD for first repair", 8664m, invoiceLine.ValueForADD);
				AssertEquals("ValueForCVD for first repair", 0m, invoiceLine.ValueForCVD);

				AssertEquals("ValueForADD for third repair", 0m, thirdRepairLine.ValueForADD);
				AssertEquals("ValueForCVD for third repair", 1344m, thirdRepairLine.ValueForCVD);
			}
		}

		public void TestValueForADD_CVDForXAndV()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine xLine = declaration.InvoiceLines.AddNew();
			xLine.JI_LinePrice = 10000m;
			xLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;

			JobComInvoiceLine vLine = xLine.AddSecondaryInvoiceLine();
			vLine.JI_LinePrice = 5000m;
			vLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			vLine.US_ADDCaseNo = "A";

			JobComInvoiceLine vLine2 = xLine.AddSecondaryInvoiceLine();
			vLine2.JI_LinePrice = 3000m;
			vLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			vLine2.US_ADDCaseNo = "A";

			JobComInvoiceLine vLine3 = xLine.AddSecondaryInvoiceLine();
			vLine3.JI_LinePrice = 2000m;
			vLine3.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			vLine3.US_CVDCaseNo = "C";

			AssertEquals(5000m, vLine.ValueForADD);
			AssertEquals(0m, vLine.ValueForCVD);

			AssertEquals(3000m, vLine2.ValueForADD);
			AssertEquals(0m, vLine2.ValueForCVD);

			AssertEquals(0m, vLine3.ValueForADD);
			AssertEquals(2000m, vLine3.ValueForCVD);
		}

		public void TestValueForADD_CVDForAdditionalSupTariffs()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_FormattedTariff = "4407.13.0000";
			invoiceLine.SupTariffFormatted = "9903.01.14";
			invoiceLine.SupFormattedAdditionalTariff1 = "9802.00.4040";
			invoiceLine.US_98GoodsValue = 250m;
			invoiceLine.JI_LinePrice = 1000m;

			invoiceLine.US_CVDCaseNo = "C";
			invoiceLine.US_ADDCaseNo = "";
			AssertEquals("ValueForADD", 0m, invoiceLine.ValueForADD);
			AssertEquals("ValueForCVD", 1250m, invoiceLine.ValueForCVD);

			invoiceLine.US_CVDCaseNo = "";
			invoiceLine.US_ADDCaseNo = "A";
			AssertEquals("ValueForADD", 1250m, invoiceLine.ValueForADD);
			AssertEquals("ValueForCVD", 0m, invoiceLine.ValueForCVD);
		}

		public void TestChaningJI_JZShouldAlsoChangeChildLineJI_JZ()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoice1 = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2 = invoiceLine1.AddSecondaryInvoiceLine();
			JobComInvoiceLine invoiceLine3 = invoiceLine1.AddSecondaryInvoiceLine();
			JobComInvoiceHeader invoice2 = declaration.Invoices.AddNew();
			invoiceLine1.JI_JZ = invoice2.PK;
			AssertEquals(invoice2.PK, invoiceLine2.JI_JZ);
			AssertEquals(invoice2.PK, invoiceLine3.JI_JZ);
			invoiceLine2.JI_JZ = invoice1.PK;
			AssertEquals(ZGuid.Empty, invoiceLine2.JI_ParentID);
			invoiceLine3.JI_ParentID = invoiceLine2.PK;
			AssertEquals(invoice2.PK, invoiceLine3.JI_JZ);
			invoiceLine3.JI_JZ = invoice1.PK;
			AssertEquals(invoice1.PK, invoiceLine3.JI_JZ);
			AssertEquals(invoiceLine2.PK, invoiceLine3.JI_ParentID);
		}

		public void TestAdditionalIDutyData()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var invoiceLine2 = invoiceLine.AddSecondaryInvoiceLine();

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			IDutyData dutyData = invoiceLine;
			AssertEquals(EntryTypeList.Codes.ConsumptionFreeDutiable, dutyData.EntryType);

			invoiceLine.US_CottonFeeExempt = YesNoDefaultList.Codes.Yes;
			AssertEquals(false, dutyData.IsAMSFeeExempt);
			AssertEquals(true, dutyData.IsCottonFeeExemptIndicated);

			invoiceLine.US_CottonCertificateNo = "ORGANICXX";
			AssertEquals(true, dutyData.IsAMSFeeExempt);

			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			AssertEquals(true, invoiceLine.IsSetVLine);

			Assert("Secondary invoice line will become v line when parent is set to v line", ((IDutyData)invoiceLine2).IsSetVLine);
			AssertEquals(invoiceLine, ((IDutyData)invoiceLine2).ParentTariffLine);
			AssertEquals(false, ((IDutyData)invoiceLine2).IsSecondaryTariffLine);

			invoiceLine2.US_SecondarySPI = ZString.Empty;
			AssertEquals(true, ((IDutyData)invoiceLine2).IsSecondaryTariffLine);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			invoiceLine.US_CottonCertificateNo = ZString.Empty;
			Assert(!dutyData.IsAMSFeeExempt);

			var perm = invoiceLine.LicenceAndPermits.AddNew(LicencePermitTypeList.Codes._22, "ORGANICXX");
			Assert(dutyData.IsAMSFeeExempt);
			Assert(dutyData.IsRaspberryFeeExempt);

			perm.CY_Code = LicencePermitTypeList.Codes._23;
			Assert(!dutyData.IsAMSFeeExempt);
			Assert(dutyData.IsRaspberryFeeExempt);
		}

		public void TestIDutyDataIsSecondaryTariff()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8457.20.0010";
			invoiceLine.US_SupTariff = "9802.00.8068";
			invoiceLine.US_98GoodsValue = 319m;
			invoiceLine.JI_LinePrice = 1356m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			Assert(!invoiceLine.IsSecondaryTariffLine);

			Assert("JobComInvoiceLine.IDutyData is a secondary tariff line of its sup tariff line ", ((IDutyData)invoiceLine).IsSecondaryTariffLine);
			AssertEquals("But invoiceLine2 as IDutyData is a secondary line", 0, ((IFeeCalculationDataProvider)invoiceLine).SecondaryLines.Count());

			var supTariffDutyData = invoiceLine.SupplementaryParentTariffIDutyData;

			AssertEquals("HMF should be calculated on parent line only with the sum of secondary lines", 2.09m, new HarborMaintenanceFeeCalculator(Factory).CalculateFee(supTariffDutyData).Amount.Round(2));
			AssertEquals("HMF should not be calculated on secondary lines level. InvoiceLine as IDutyData should return true for IsSecondaryLine.", 0m, new HarborMaintenanceFeeCalculator(Factory).CalculateFee(invoiceLine).Amount);
			Assert(!supTariffDutyData.IsSecondaryTariffLine);
			AssertEquals(1, ((IFeeCalculationDataProvider)supTariffDutyData).SecondaryLines.Count());

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.US_SupTariff = "9802.00.8068";
			invoiceLine2.JI_Tariff = "9102.11.1010";
			AssertEquals("PreCondition:SecondaryTariffLines are added", 3, invoiceLine2.SecondaryTariffLines.Count());
			AssertEquals("But invoiceLine2 as IDutyData is a secondary line", 0, ((IFeeCalculationDataProvider)invoiceLine2).SecondaryLines.Count());

			AssertEquals("HMF should not be calculated on secondary lines level.", 0m, new HarborMaintenanceFeeCalculator(Factory).CalculateFee(invoiceLine2).Amount);
		}

		public void TestCalculateUS_IsParentLine()
		{
			AssertEquals("PreCondition", false, InvoiceLine.US_IsParent);

			JobComInvoiceLine secondaryLine = InvoiceLine.AddSecondaryInvoiceLine();
			AssertEquals("One secondaryLine is added", true, InvoiceLine.US_IsParent);

			JobComInvoiceLine secondaryLine2 = InvoiceLine.AddSecondaryInvoiceLine();
			AssertEquals("Another secondaryLine is added", true, InvoiceLine.US_IsParent);

			JobComInvoiceLine invoiceLine2 = InvoiceLine.InvoiceHeader.JobComInvoiceLines.AddNew();
			secondaryLine.JI_ParentID = ZGuid.Empty;
			AssertEquals("There is one secondary line left", true, InvoiceLine.US_IsParent);

			secondaryLine2.JI_ParentID = invoiceLine2.PK;
			AssertEquals("No more secondary line left therefore should be recalculated", false, InvoiceLine.US_IsParent);
			AssertEquals("secondary line is just added to invoice line2", true, invoiceLine2.US_IsParent);

			secondaryLine2.Delete();
			AssertEquals("secondary line is just deleted", false, invoiceLine2.US_IsParent);
		}

		public void TestUS_R_QuantityAndUQFieldsReadonly()
		{
			AssertEquals(true, InvoiceLine.US_R_OrigFirstQty_ReadOnly);
			AssertEquals(true, InvoiceLine.US_R_OrigFirstUQ_ReadOnly);
			AssertEquals(true, InvoiceLine.US_R_OrigSecondQty_ReadOnly);
			AssertEquals(true, InvoiceLine.US_R_OrigSecondUQ_ReadOnly);
			AssertEquals(true, InvoiceLine.US_R_OrigThirdQty_ReadOnly);
			AssertEquals(true, InvoiceLine.US_R_OrigThirdUQ_ReadOnly);

			InvoiceLine.US_R_OrigFirstUQ = "KG";
			InvoiceLine.US_R_OrigSecondUQ = "KG";
			InvoiceLine.US_R_OrigThirdUQ = "KG";
			AssertEquals(false, InvoiceLine.US_R_OrigFirstQty_ReadOnly);
			AssertEquals(false, InvoiceLine.US_R_OrigSecondQty_ReadOnly);
			AssertEquals(false, InvoiceLine.US_R_OrigThirdQty_ReadOnly);

			InvoiceLine.US_R_OrigFirstUQ = ABIUnitOfMeasureList.Codes.NoUnitRequired;
			InvoiceLine.US_R_OrigSecondUQ = ABIUnitOfMeasureList.Codes.NoUnitRequired;
			InvoiceLine.US_R_OrigThirdUQ = ABIUnitOfMeasureList.Codes.NoUnitRequired;
			AssertEquals(true, InvoiceLine.US_R_OrigFirstQty_ReadOnly);
			AssertEquals(true, InvoiceLine.US_R_OrigSecondQty_ReadOnly);
			AssertEquals(true, InvoiceLine.US_R_OrigThirdQty_ReadOnly);
		}

		public void TestUS_OverrideDuty()
		{
			AssertEquals("US_Duty should be readonly", true, InvoiceLine.US_Duty_ReadOnly);

			InvoiceLine.US_OverrideDuty = true;
			AssertEquals("US_Duty should be editable", false, InvoiceLine.US_Duty_ReadOnly);

			InvoiceLine.US_Duty = 20m;

			AssertEquals("InvoiceLine.US_Duty", 20m, InvoiceLine.US_Duty);
			AssertEquals("InvoiceLine.JI_Calc_DutyAmount", 20m, InvoiceLine.JI_Calc_DutyAmount);

			ReconDeclaration reconDeclaration = new ReconDeclaration(InvoiceLine.Declaration);
			AssertEquals("PreCondition", true, InvoiceLine.Declaration.IsRecon);

			InvoiceLine.US_OverrideDuty = false;
			InvoiceLine.US_Duty = 20m;
			AssertEquals(20m, InvoiceLine.US_Duty);
			AssertEquals("InvoiceLine.JI_Calc_DutyAmount", 20m, InvoiceLine.JI_Calc_DutyAmount);

			InvoiceLine.US_R_OrigOverrideDuty = true;
			AssertEquals(false, InvoiceLine.US_R_OrigDuty_ReadOnly);

			InvoiceLine.US_R_OrigOverrideDuty = false;
			AssertEquals(true, InvoiceLine.US_R_OrigDuty_ReadOnly);
		}

		public void TestIReconOriginalChargeParent()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.AccountingClassFeeCode, RefCusCodeListTypes.Codes.AccountingClassFeeCode);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.AccountingClassFeeCode,
				"124", "Pecan Fee", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.AccountingClassFeeCode,
				"125", "Christmas Tree Fee", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			IReconOriginalChargeParent parent = InvoiceLine;
			AssertEquals("Pecan Fee", parent.FeeAndChargeList.GetDescriptionFromCode("124"));
			AssertEquals("Christmas Tree Fee", parent.FeeAndChargeList.GetDescriptionFromCode("125"));
		}

		public void TestPrivilegedStatusDateVisible()
		{
			for (int i = 0; i < InvoiceLine.AddInfoLookups.US_ZoneStatusList.Count; i++)
			{
				InvoiceLine.US_ZoneStatus = InvoiceLine.AddInfoLookups.US_ZoneStatusList[i].Code;
				if (InvoiceLine.US_ZoneStatus == ZoneStatusList.Codes.PrivilegedForeign)
				{
					Assert("PrivilegedStatusDateVisible should be true when invoiceLine.US_ZoneStatus == ZoneStatusList.Codes.PrivilegedForeign", InvoiceLine.PrivilegedStatusDateVisible);
				}
				else
				{
					Assert("PrivilegedStatusDateVisible should NOT be true when invoiceLine.US_ZoneStatus != ZoneStatusList.Codes.PrivilegedForeign", !InvoiceLine.PrivilegedStatusDateVisible);
				}
			}
		}

		public void TestFTZCurrentTariffVisible()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Declaration.US_EnableCRL = true;
			Declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;

			for (int i = 0; i < InvoiceLine.AddInfoLookups.US_ZoneStatusList.Count; i++)
			{
				InvoiceLine.US_ZoneStatus = InvoiceLine.AddInfoLookups.US_ZoneStatusList[i].Code;
				if (InvoiceLine.US_ZoneStatus == ZoneStatusList.Codes.PrivilegedForeign)
				{
					Assert("FTZCurrentTariffVisible should be true when invoiceLine.US_ZoneStatus == ZoneStatusList.Codes.PrivilegedForeign", InvoiceLine.FTZCurrentTariffVisible);
				}
				else
				{
					Assert("FTZCurrentTariffVisible should NOT be true when invoiceLine.US_ZoneStatus != ZoneStatusList.Codes.PrivilegedForeign", !InvoiceLine.FTZCurrentTariffVisible);
				}
			}
		}

		public void TestCanSetManufactureAddress()
		{
			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "ABC");
			OrgHeader org2 = Factory.New<OrgHeader>();
			org2.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "DEF");

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.ManufacturerOrgPK = org1.PK;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(org1.PK, invoiceLine.ManufacturerOrgPK);
			AssertEquals(org1.MainAddress.PK, invoiceLine.JI_OA_ManufacturerAddress);
			invoiceLine.ManufacturerOrgPK = org2.PK;
			AssertEquals(org2.PK, invoiceLine.ManufacturerOrgPK);
			AssertEquals(org2.MainAddress.PK, invoiceLine.JI_OA_ManufacturerAddress);
		}

		public void TestJI_OA_ExporterAddress()
		{
			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "ABC");
			OrgHeader org2 = Factory.New<OrgHeader>();
			org2.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "DEF");

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_OA_ExporterAddress_ZAddress.OrgPK = org1.PK;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(org1.PK, invoiceLine.JI_OA_ExporterAddress_ZAddress.OrgPK);
			AssertEquals(org1.MainAddress.PK, invoiceLine.JI_OA_ExporterAddress);
			invoiceLine.JI_OA_ExporterAddress_ZAddress.OrgPK = org2.PK;
			AssertEquals(org2.PK, invoiceLine.JI_OA_ExporterAddress_ZAddress.OrgPK);
			AssertEquals(org2.MainAddress.PK, invoiceLine.JI_OA_ExporterAddress);
		}

		public void TestDefaultDataOnAttachingToAnInvoice()
		{
			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.MiscServ.OM_RN_NKEXDefaultCntryOfOrigin = Core.Constants.CountryCodes.Singapore;

			JobComInvoiceHeader invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_OA_ManufacturerAddress = org1.MainAddress.PK;

			JobComInvoiceLine invoiceLine = Factory.New<JobComInvoiceLine>();
			AssertEquals(ZString.Empty, invoiceLine.US_UC_NKCountryOfOrigin);

			invoiceLine.JI_JZ = invoice.PK;
			AssertEquals(Core.Constants.CountryCodes.Singapore, invoiceLine.US_UC_NKCountryOfOrigin);
		}

		public void TestDefaultDataFromManufacturer()
		{
			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.MiscServ.OM_RN_NKEXDefaultCntryOfOrigin = Core.Constants.CountryCodes.Singapore;
			OrgHeader org2 = Factory.New<OrgHeader>();
			org2.MiscServ.OM_RN_NKEXDefaultCntryOfOrigin = Core.Constants.CountryCodes.Malaysia;

			JobComInvoiceLine invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.ManufacturerOrgPK = org1.PK;
			AssertEquals(Core.Constants.CountryCodes.Singapore, invoiceLine.US_UC_NKCountryOfOrigin);

			invoiceLine.ManufacturerOrgPK = org2.PK;
			AssertEquals(Core.Constants.CountryCodes.Malaysia, invoiceLine.US_UC_NKCountryOfOrigin);
		}

		public void TestUS_PrivilegedStatusDate()
		{
			var dateForPrivilegedStatusDateTest1 = ZDateTime.Today;
			var dateForPrivilegedStatusDateTest2 = ZDateTime.Today.AddDays(-1);
			Assert("Preconditions: DateForPrivilegedStatusDateTest1 should not be equal to DateForPrivilegedStatusDateTest2", dateForPrivilegedStatusDateTest1 != dateForPrivilegedStatusDateTest2);
			var childLine = InvoiceLine.AddSecondaryInvoiceLine();

			Assert("US_PrivilegedStatusDate should not be readonly", !InvoiceLine.US_PrivilegedStatusDateInfo.ReadOnly);
			Assert("US_PrivilegedStatusDate should be readonly", childLine.US_PrivilegedStatusDateInfo.ReadOnly);

			InvoiceLine.US_PrivilegedStatusDate = dateForPrivilegedStatusDateTest1;
			InvoiceHeader.US_PrivilegedStatusDate = dateForPrivilegedStatusDateTest2;
			AssertEquals("US_PrivilegedStatusDate should be get from InvoiceLine", dateForPrivilegedStatusDateTest1, InvoiceLine.US_PrivilegedStatusDate);
			AssertEquals("US_PrivilegedStatusDate should be get from InvoiceLine", dateForPrivilegedStatusDateTest1, childLine.US_PrivilegedStatusDate);

			InvoiceLine.US_PrivilegedStatusDate = ZDateTime.Empty;
			AssertEquals("US_PrivilegedStatusDate should be get from InvoiceHeaderForCombined", dateForPrivilegedStatusDateTest2, InvoiceLine.US_PrivilegedStatusDate);
			AssertEquals("US_PrivilegedStatusDate should be get from InvoiceHeaderForCombined", dateForPrivilegedStatusDateTest2, childLine.US_PrivilegedStatusDate);
		}

		public void TestUS_ZoneStatus()
		{
			JobComInvoiceLine childLine = InvoiceLine.AddSecondaryInvoiceLine();

			Assert("US_ZoneStatus should not be readonly", !InvoiceLine.US_ZoneStatusInfo.ReadOnly);
			Assert("US_ZoneStatus should be readonly", childLine.US_ZoneStatusInfo.ReadOnly);

			InvoiceLine.US_ZoneStatus = ZoneStatusList.Codes.Domestic;
			InvoiceHeader.US_ZoneStatus = ZoneStatusList.Codes.NonPrivilegedForeign;
			AssertEquals("US_ZoneStatus should be get from InvoiceLine", ZoneStatusList.Codes.Domestic, InvoiceLine.US_ZoneStatus);
			AssertEquals("US_ZoneStatus should be get from InvoiceLine", ZoneStatusList.Codes.Domestic, childLine.US_ZoneStatus);

			InvoiceLine.US_ZoneStatus = ZString.Empty;
			AssertEquals("US_ZoneStatus should be get from InvoiceHeaderForCombined", ZoneStatusList.Codes.NonPrivilegedForeign, InvoiceLine.US_ZoneStatus);
			AssertEquals("US_ZoneStatus should be get from InvoiceHeaderForCombined", ZoneStatusList.Codes.NonPrivilegedForeign, childLine.US_ZoneStatus);

			for (int i = 0; i < InvoiceLine.AddInfoLookups.US_ZoneStatusList.Count; i++)
			{
				InvoiceLine.US_PrivilegedStatusDate = ZDateTime.Today;
				InvoiceLine.US_ZoneStatus = InvoiceLine.AddInfoLookups.US_ZoneStatusList[i].Code;
				if (InvoiceLine.US_ZoneStatus == ZoneStatusList.Codes.PrivilegedForeign)
				{
					AssertEquals("invoiceLine.US_PrivilegedStatusDate should NOT be cleared here", ZDateTime.Today, InvoiceLine.US_PrivilegedStatusDate);
				}
				else
				{
					AssertEquals("invoiceLine.US_PrivilegedStatusDate should be cleared here", ZDateTime.Empty, InvoiceLine.US_PrivilegedStatusDate);
				}
			}
		}

		public void TestGrossWeightApportionment()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_AutoWeightApportion = true;
			declaration.JE_TotalWeightUnit = Core.Constants.Weight.Kilograms;
			JobComInvoiceHeader invoice1 = declaration.FilteredInvoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice1.JZ_InvoiceAmount = 3000m;
			JobComInvoiceLine invoice1Line1 = invoice1.JobComInvoiceLines.AddNew();
			invoice1Line1.JI_LinePrice = 3000m;

			JobComInvoiceHeader invoice2 = declaration.FilteredInvoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice2.JZ_InvoiceAmount = 2000m;
			JobComInvoiceLine invoice2Line1 = invoice2.JobComInvoiceLines.AddNew();
			invoice2Line1.JI_LinePrice = 2000m;

			JobComInvoiceHeader invoice3 = declaration.FilteredInvoices.AddNew();
			invoice3.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice3.JZ_InvoiceAmount = 1000m;
			JobComInvoiceLine invoice3Line1 = invoice3.JobComInvoiceLines.AddNew();
			invoice3Line1.JI_LinePrice = 1000m;

			declaration.JE_TotalWeight = 6000m;
			AssertEquals(3000m, invoice1.JZ_Weight);
			AssertEquals(3000m, invoice1Line1.JI_Weight);
			AssertEquals(2000m, invoice2.JZ_Weight);
			AssertEquals(2000m, invoice2Line1.JI_Weight);
			AssertEquals(1000m, invoice3.JZ_Weight);
			AssertEquals(1000m, invoice3Line1.JI_Weight);

			JobComInvoiceLine invoice1Line2 = invoice1.JobComInvoiceLines.AddNew();
			invoice1Line2.JI_LinePrice = 1500m;
			AssertEquals(3000m, invoice1.JZ_Weight);
			AssertEquals(2000m, invoice1Line1.JI_Weight);
			AssertEquals(1000m, invoice1Line2.JI_Weight);
			AssertEquals(2000m, invoice2.JZ_Weight);
			AssertEquals(2000m, invoice2Line1.JI_Weight);
			AssertEquals(1000m, invoice3.JZ_Weight);
			AssertEquals(1000m, invoice3Line1.JI_Weight);

			invoice1Line2.US_98GoodsValue = 3000m;
			invoice1Line2.JI_LinePrice = 0m;
			AssertEquals(3000m, invoice1.JZ_Weight);
			AssertEquals(1500m, invoice1Line1.JI_Weight);
			AssertEquals(1500m, invoice1Line2.JI_Weight);
			AssertEquals(2000m, invoice2.JZ_Weight);
			AssertEquals(2000m, invoice2Line1.JI_Weight);
			AssertEquals(1000m, invoice3.JZ_Weight);
			AssertEquals(1000m, invoice3Line1.JI_Weight);

			invoice1Line1.US_98GoodsValue = 1000m;
			invoice1Line1.JI_LinePrice = 0m;
			AssertEquals(3000m, invoice1.JZ_Weight);
			AssertEquals(750m, invoice1Line1.JI_Weight);
			AssertEquals(2250m, invoice1Line2.JI_Weight);
			AssertEquals(2000m, invoice2.JZ_Weight);
			AssertEquals(2000m, invoice2Line1.JI_Weight);
			AssertEquals(1000m, invoice3.JZ_Weight);
			AssertEquals(1000m, invoice3Line1.JI_Weight);

			invoice1Line1.US_98GoodsValue = 0m;
			invoice1Line1.US_98ValueInvCurr = 1000m;
			invoice1Line1.JI_LinePrice = 0m;
			AssertEquals(3000m, invoice1.JZ_Weight);
			AssertEquals(750m, invoice1Line1.JI_Weight);
			AssertEquals(2250m, invoice1Line2.JI_Weight);
			AssertEquals(2000m, invoice2.JZ_Weight);
			AssertEquals(2000m, invoice2Line1.JI_Weight);
			AssertEquals(1000m, invoice3.JZ_Weight);
			AssertEquals(1000m, invoice3Line1.JI_Weight);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			invoice1Line1.US_98GoodsValue = 3000m;
			AssertEquals(3000m, invoice1.JZ_Weight);
			AssertEquals(750m, invoice1Line1.JI_Weight);
			AssertEquals(2250m, invoice1Line2.JI_Weight);
			AssertEquals(2000m, invoice2.JZ_Weight);
			AssertEquals(2000m, invoice2Line1.JI_Weight);
			AssertEquals(1000m, invoice3.JZ_Weight);
			AssertEquals(1000m, invoice3Line1.JI_Weight);

			invoice1Line1.JI_LinePrice = 2000m;
			invoice1Line2.JI_LinePrice = 2000m;
			AssertEquals(3000m, invoice1.JZ_Weight);
			AssertEquals(1500m, invoice1Line1.JI_Weight);
			AssertEquals(1500m, invoice1Line2.JI_Weight);
			AssertEquals(2000m, invoice2.JZ_Weight);
			AssertEquals(2000m, invoice2Line1.JI_Weight);
			AssertEquals(1000m, invoice3.JZ_Weight);
			AssertEquals(1000m, invoice3Line1.JI_Weight);
		}

		[TestDate(2008, 1, 1)]
		public void TestUS_SPIForInLieuTariff()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceLine.US_SupTariff = "9908.04.05";
			InvoiceLine.US_SPI = "IL";
			AssertEquals("PreCondition:In-lieu tariff", true, InvoiceLine.ImportSupTariff.Applies(TariffRuleList.Codes.InLieuTariffs, InvoiceLine.EffectiveDateForDutyRate));

			InvoiceLine.JI_Tariff = "0406102800";
			AssertEquals("SPI should be the same", "IL", InvoiceLine.US_SPI);

			InvoiceLine.US_SPI = "IL";
			AssertEquals("IL should stay there", "IL", InvoiceLine.US_SPI);
		}

		public void TestUS_OA_UltimateConsignee()
		{
			var ultimateConsignee = Factory.New<OrgHeader>();
			var ultiConsigneeAddressPK = ultimateConsignee.MainAddress.PK;

			var ultimateConsignee2 = Factory.New<OrgHeader>();
			var ultiConsignee2AddressPK = ultimateConsignee2.MainAddress.PK;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OA_ConsigneeAddress = ultiConsigneeAddressPK;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			AssertEquals("returing an effective value", ultiConsigneeAddressPK, invoiceLine.JI_OA_ConsigneeAddress);

			invoiceLine.JI_OA_ConsigneeAddress = ultiConsignee2AddressPK;
			AssertEquals("invoice line still retains the value", ultiConsignee2AddressPK, invoiceLine.JI_OA_ConsigneeAddress);

			var line2 = declaration.InvoiceLines.AddNew();
			AssertEquals("returing an effective value", ultiConsigneeAddressPK, line2.JI_OA_ConsigneeAddress);

			var ultimateConsignee3 = Factory.New<OrgHeader>();
			var ultiConsignee3AddressPK = ultimateConsignee3.MainAddress.PK;
			line2.JI_OA_ConsigneeAddress = ultiConsignee3AddressPK;
			AssertEquals(ultiConsignee3AddressPK, line2.JI_OA_ConsigneeAddress);

			line2.JI_ParentID = invoiceLine.PK;
			AssertEquals("returing an effective value from parent line", ultiConsignee2AddressPK, line2.JI_OA_ConsigneeAddress);
		}

		public void TestJI_OA_ShipToPartyAddress()
		{
			var shipToParty = Factory.New<OrgHeader>();
			var shipToPartyPK = shipToParty.MainAddress.PK;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_OA_ShipToPartyAddress = shipToPartyPK;
			AssertEquals("should return an effective value", shipToPartyPK, invoiceLine.JI_OA_ShipToPartyAddress);

			invoiceLine.JI_OA_ShipToPartyAddress = ZGuid.Empty;
			var ultimateConsignee = Factory.New<OrgHeader>();
			var ultiOrgAddressPK = ultimateConsignee.MainAddress.PK;
			invoiceLine.JI_OA_ConsigneeAddress = ultiOrgAddressPK;
			AssertEquals("should default from Ultimate Consignee", ultiOrgAddressPK, invoiceLine.JI_OA_ShipToPartyAddress);
		}

		public void TestUS_ADDDepositRateDescription()
		{
			USCACCase uscCase = Factory.New<USCACCase>();
			uscCase.U5_CaseNumber = "AXXAAABBB";

			USCACCaseRate rate = uscCase.CaseRates.AddNew();
			rate.U6_EffectiveDate = ZDateTime.Today;
			rate.U6_AdValoremRate = 0.1234m;
			rate.U6_SpecificRate = 0.2345m;
			rate.U6_Unit = "KG";
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_ADDCaseNo = "AXXAAABBB";

			invoiceLine.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.AdValorem;
			AssertEquals("12.34%", invoiceLine.US_ADDDepositRateDescription);
			invoiceLine.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.Specific;
			AssertEquals("23c/KG", invoiceLine.US_ADDDepositRateDescription);
			invoiceLine.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.OverrideAdValorem;
			invoiceLine.US_ADDDepositRateDescription = "12.34";
			AssertEquals("12.34%", invoiceLine.US_ADDDepositRateDescription);
			invoiceLine.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.OverrideSpecific;
			invoiceLine.US_ADDDepositRateDescription = "0.1234";
			AssertEquals("12.34c/KG", invoiceLine.US_ADDDepositRateDescription);
			invoiceLine.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.OverrideAdValorem;
			invoiceLine.US_ADDDepositRateDescription = "12.345%";
			AssertEquals("12.35%", invoiceLine.US_ADDDepositRateDescription);
			invoiceLine.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.OverrideSpecific;
			invoiceLine.US_ADDDepositRateDescription = "0.1235";
			AssertEquals("12.35c/KG", invoiceLine.US_ADDDepositRateDescription);
			invoiceLine.US_ADDDepositRateDescription = "123.45";
			rate.U6_UnitDesc = "KG";
			AssertEquals("123.45$/KG(KG)", invoiceLine.US_ADDDepositRateDescription);
			invoiceLine.US_ADDDepositRateDescription = "123.45C";
			AssertEquals("1.2345$/KG(KG)", invoiceLine.US_ADDDepositRateDescription);
		}

		public void TestUS_CVDDepositRateDescription()
		{
			USCACCase uscCase = Factory.New<USCACCase>();
			uscCase.U5_CaseNumber = "AXXAAABBB";

			USCACCaseRate rate = uscCase.CaseRates.AddNew();
			rate.U6_EffectiveDate = ZDateTime.Today;
			rate.U6_AdValoremRate = 0.1234m;
			rate.U6_SpecificRate = 0.2345m;
			rate.U6_Unit = "KG";
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_CVDCaseNo = "AXXAAABBB";

			invoiceLine.US_CVDDepositRateIndicator = DepositRateIndicatorList.Codes.AdValorem;
			AssertEquals("12.34%", invoiceLine.US_CVDDepositRateDescription);
			invoiceLine.US_CVDDepositRateIndicator = DepositRateIndicatorList.Codes.Specific;
			AssertEquals("23c/KG", invoiceLine.US_CVDDepositRateDescription);
			invoiceLine.US_CVDDepositRateIndicator = DepositRateIndicatorList.Codes.OverrideAdValorem;
			invoiceLine.US_CVDDepositRateDescription = "12.34";
			AssertEquals("12.34%", invoiceLine.US_CVDDepositRateDescription);
			invoiceLine.US_CVDDepositRateIndicator = DepositRateIndicatorList.Codes.OverrideSpecific;
			invoiceLine.US_CVDDepositRateDescription = "0.1234";
			AssertEquals("12.34c/KG", invoiceLine.US_CVDDepositRateDescription);
			invoiceLine.US_CVDDepositRateIndicator = DepositRateIndicatorList.Codes.OverrideAdValorem;
			invoiceLine.US_CVDDepositRateDescription = "12.345%";
			AssertEquals("12.35%", invoiceLine.US_CVDDepositRateDescription);
			invoiceLine.US_CVDDepositRateIndicator = DepositRateIndicatorList.Codes.OverrideSpecific;
			invoiceLine.US_CVDDepositRateDescription = "0.1235";
			AssertEquals("12.35c/KG", invoiceLine.US_CVDDepositRateDescription);
			invoiceLine.US_CVDDepositRateDescription = "123.45";
			rate.U6_UnitDesc = "KG";
			AssertEquals("123.45$/KG(KG)", invoiceLine.US_CVDDepositRateDescription);
			invoiceLine.US_CVDDepositRateDescription = "123.45C";
			AssertEquals("1.2345$/KG(KG)", invoiceLine.US_CVDDepositRateDescription);
		}

		public void TestIFeeCalculationDataProvider()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			ReconOriginalEntryHeader originalEntry = reconDec.OriginalEntries.AddNew();
			originalEntry.US_R_DutyRateDate = new ZDateTime(2008, 8, 1);

			JobComInvoiceHeader invoice = originalEntry.Invoice;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLine secondaryLine = invoiceLine.AddSecondaryInvoiceLine();

			IFeeCalculationDataProvider dataProvider = invoiceLine;
			IFeeCalculationDataProvider dataProvider2 = secondaryLine;

			AssertEquals("IsSecondaryTariffLine", false, dataProvider.IsSecondaryTariffLine);
			AssertEquals("IsSecondaryTariffLine", true, dataProvider2.IsSecondaryTariffLine);

			invoiceLine.US_SecondarySPI = "X";
			AssertEquals("PreCondition:Secondary becomes V line", "V", secondaryLine.US_SecondarySPI);

			AssertEquals("IsSetVLine", false, dataProvider.IsSetVLine);
			AssertEquals("IsSetVLine", true, dataProvider2.IsSetVLine);

			AssertNull("ParentTariffline", dataProvider.ParentTariffLine);
			AssertEquals("ParentTariffline", invoiceLine, dataProvider2.ParentTariffLine);

			invoiceLine.US_CottonFeeExempt = YesNoDefaultList.Codes.Yes;
			invoiceLine.Declaration.US_EntryType = "01";
			AssertEquals(true, dataProvider.IsCottonFeeExemptIndicated);
			AssertEquals("01", dataProvider.EntryType);

			dataProvider.SetFeeResult("AAA", 25m, new FeeCalculationInternalData());
			AssertEquals("AAA", invoiceLine.FeeCusCodes[0].CY_Code);
			AssertEquals(25m, invoiceLine.FeeCusCodes[0].CY_FeeAmount);

			AssertEquals(false, invoiceLine.IsFeeOverriden("AAA"));
			invoiceLine.FeeCusCodes[0].CY_IsOverridden = true;
			AssertEquals(true, invoiceLine.IsFeeOverriden("AAA"));

			dataProvider.SetFeeResult("AAA", 0m, new FeeCalculationInternalData());
			AssertNotNull(invoiceLine.FeeCusCodes.GetFirstElementHaving("AAA"));
		}

		public void TestIsSoftwoodLumberSection804FarmBillRequirement()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "44091020";
			tariff.UE_PermitLicenseIndicator = MiscellaneousPermitLicenseList.Codes.CanadaSoftwoodLumberExportNumber;
			tariff.UE_DateFrom = ZDateTime.Today;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "44091020";
			AssertEquals(true, invoiceLine.IsSoftwoodLumberSection804FarmBillRequirement);

			declaration.US_EntryType = EntryTypeList.Codes.Baggage;
			AssertEquals(false, invoiceLine.IsSoftwoodLumberSection804FarmBillRequirement);

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			tariff.UE_PermitLicenseIndicator = MiscellaneousPermitLicenseList.Codes.AustraliaFreeTradeExportCertificate;
			AssertEquals(false, invoiceLine.IsSoftwoodLumberSection804FarmBillRequirement);

			tariff.UE_PermitLicenseIndicator = MiscellaneousPermitLicenseList.Codes.CanadaSoftwoodLumberExportNumber;
			invoiceLine.JI_Tariff = ZString.Empty;
			AssertEquals(false, invoiceLine.IsSoftwoodLumberSection804FarmBillRequirement);
		}

		public void TestUS_R_OrigTariff()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1234567890";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(-1);
			tariff.UE_Unit1 = "X";
			tariff.UE_Unit2 = "Y";
			tariff.UE_Unit3 = "KG";

			ReconDeclaration declaration = new ReconDeclaration(Factory.New<JobDeclaration>());

			ReconOriginalEntryHeader originalEntry = declaration.OriginalEntries.AddNew();
			JobComInvoiceHeader invoice = originalEntry.Invoice;
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();

			originalEntry.US_R_DutyRateDate = ZDateTime.Today;
			invoiceLine.US_R_OrigTariff = tariff.UE_Tariff;
			AssertNull("No tariff found", invoiceLine.OriginalImportTariff);

			originalEntry.US_R_DutyRateDate = ZDateTime.Today.AddDays(-1);
			invoiceLine.US_R_OrigTariff = tariff.UE_Tariff;
			AssertEquals("X", invoiceLine.US_R_OrigFirstUQ);
			AssertEquals("Y", invoiceLine.US_R_OrigSecondUQ);
			AssertEquals("KG", invoiceLine.US_R_OrigThirdUQ);
		}

		public void TestOriginalTariffFormatted()
		{
			InvoiceLine.US_R_OrigTariff = "1234567890";
			AssertEquals("Formatted Original Tariff", "1234.56.7890", InvoiceLine.OriginalTariffFormatted);
		}

		public void TestJI_OA_ManufacturerAddress()
		{
			OrgHeader manufacturer = Factory.New<OrgHeader>();
			OrgAddress address1 = manufacturer.MainAddress;
			OrgAddress address2 = manufacturer.Addresses.AddNew();

			OrgHeader manufacturer2 = Factory.New<OrgHeader>();
			OrgAddress address3 = manufacturer2.MainAddress;

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_OA_ManufacturerAddress = address1.PK;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(address1.PK, invoiceLine.JI_OA_ManufacturerAddress);
			invoiceLine.JI_OA_ManufacturerAddress = address1.PK;
			AssertEquals(address1.PK, invoiceLine.JI_OA_ManufacturerAddress);
			AssertEquals("Loading/Setting manufacturer default values should not activate save button", false, invoiceLine.HasChanges);

			invoice.JZ_OA_ManufacturerAddress = address2.PK;
			AssertEquals(address2.PK, invoiceLine.JI_OA_ManufacturerAddress);
			invoiceLine.JI_OA_ManufacturerAddress = address1.PK;
			AssertEquals(address1.PK, invoiceLine.JI_OA_ManufacturerAddress);

			invoiceLine.ManufacturerOrgPK = ZGuid.Invalid;
			AssertEquals(ZGuid.Empty, invoiceLine.JI_OA_ManufacturerAddress);
			AssertEquals("Changing manufacturer should activate save button", true, invoiceLine.HasChanges);

			invoiceLine.ManufacturerOrgPK = ZGuid.Empty;
			invoice.ManufacturerOrgPK = manufacturer2.PK;
			AssertEquals(address3.PK, invoiceLine.JI_OA_ManufacturerAddress);
			AssertEquals(manufacturer2.PK, invoiceLine.ManufacturerOrgPK);

			invoice.ManufacturerOrgPK = ZGuid.Invalid;
			AssertEquals(ZGuid.Empty, invoiceLine.JI_OA_ManufacturerAddress);
			AssertEquals(manufacturer2.PK, invoiceLine.ManufacturerOrgPK);

			invoice.ManufacturerOrgPK = manufacturer.PK;
			AssertEquals(address1.PK, invoiceLine.JI_OA_ManufacturerAddress);
			AssertEquals(manufacturer.PK, invoiceLine.ManufacturerOrgPK);

			invoice.ManufacturerOrgPK = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, invoiceLine.JI_OA_ManufacturerAddress);
			AssertEquals(ZGuid.Empty, invoiceLine.ManufacturerOrgPK);
		}

		public void TestManufacturerFallBackToSupplierNumber()
		{
			OrgHeader supplier = Factory.New<OrgHeader>();
			OrgAddress supplierAddress = supplier.Addresses.AddNew(OrgAddressType.Miscellaneous, false);
			supplierAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "SUP12345678");

			OrgHeader manufacturer = Factory.New<OrgHeader>();
			manufacturer.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "MAN12345678");

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Supplier = supplier.PK;
			invoice.JZ_OA_SupplierAddress = supplierAddress.PK;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			AssertEquals("ManufacturerFallBackToSupplierNumber", "MAN12345678", invoiceLine.ManufacturerFallBackToSupplierNumber);

			invoiceLine.JI_OA_ManufacturerAddress = ZGuid.Empty;
			AssertEquals("ManufacturerFallBackToSupplierNumber", "SUP12345678", invoiceLine.ManufacturerFallBackToSupplierNumber);

			invoice.JZ_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			AssertEquals("ManufacturerFallBackToSupplierNumber", "MAN12345678", invoiceLine.ManufacturerFallBackToSupplierNumber);
		}

		public void TestFlags()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			invoiceLine.US_DDTCInd = OGAIndicatorList.Codes.Declared;
			AssertEquals("IsEntrySummaryValidationMode", true, invoiceLine.IsEntrySummaryValidationMode);
			AssertEquals("IsCargoReleaseValidationMode", true, invoiceLine.IsCargoReleaseValidationMode);
			AssertEquals("IsStandAlonePriorNoticeMode", false, invoiceLine.IsStandAlonePriorNoticeMode);
			AssertEquals("IsACEEntrySummaryValidationMode", false, invoiceLine.IsACEEntrySummaryValidationMode);
			AssertEquals("IsPGAValidationOn", false, invoiceLine.IsACECargoReleaseValidationMode);
			AssertEquals("HasACEDDTCData", false, invoiceLine.ShouldDeclareACEDDTCData);

			declaration.ValidationModes = ValidationModes.StandAlonePriorNotice;
			AssertEquals("IsEntrySummaryValidationMode", false, invoiceLine.IsEntrySummaryValidationMode);
			AssertEquals("IsCargoReleaseValidationMode", false, invoiceLine.IsCargoReleaseValidationMode);
			AssertEquals("IsStandAlonePriorNoticeMode", true, invoiceLine.IsStandAlonePriorNoticeMode);
			AssertEquals("IsACEEntrySummaryValidationMode", false, invoiceLine.IsACEEntrySummaryValidationMode);
			AssertEquals("IsPGAValidationOn", false, invoiceLine.IsACECargoReleaseValidationMode);
			AssertEquals("HasACEDDTCData", false, invoiceLine.ShouldDeclareACEDDTCData);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.RecalculateValidationModesOnDeclaration();
			AssertEquals("IsEntrySummaryValidationMode", true, invoiceLine.IsEntrySummaryValidationMode);
			AssertEquals("This is ACE Cargo Release", true, invoiceLine.IsCargoReleaseValidationMode);
			AssertEquals("IsACEEntrySummaryValidationMode", true, invoiceLine.IsACEEntrySummaryValidationMode);
			AssertEquals("IsPGAValidationOn", true, invoiceLine.IsACECargoReleaseValidationMode);
			AssertEquals("HasACEDDTCData", true, invoiceLine.ShouldDeclareACEDDTCData);

			invoiceLine.US_DDTCInd = ZString.Empty;
			AssertEquals("HasACEDDTCData", false, invoiceLine.ShouldDeclareACEDDTCData);
		}

		public void TestIsEntrySummaryOrCargoReleaseValidationMode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = false;
			declaration.US_CertifyCargoRelease = false;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			AssertEquals("IsEntrySummaryOrCargoReleaseValidationMode", false, invoiceLine.IsEntrySummaryOrCargoReleaseValidationMode);

			declaration.US_EnableENS = true;
			AssertEquals("IsEntrySummaryOrCargoReleaseValidationMode", true, invoiceLine.IsEntrySummaryOrCargoReleaseValidationMode);

			declaration.US_EnableCRL = true;
			AssertEquals("IsEntrySummaryOrCargoReleaseValidationMode", true, invoiceLine.IsEntrySummaryOrCargoReleaseValidationMode);

			declaration.US_EnableENS = false;
			AssertEquals("IsEntrySummaryOrCargoReleaseValidationMode", true, invoiceLine.IsEntrySummaryOrCargoReleaseValidationMode);

			declaration.US_EnableCRL = false;
			AssertEquals("IsEntrySummaryOrCargoReleaseValidationMode", false, invoiceLine.IsEntrySummaryOrCargoReleaseValidationMode);

			declaration.US_CertifyCargoRelease = false;
			AssertEquals("IsEntrySummaryOrCargoReleaseValidationMode", false, invoiceLine.IsEntrySummaryOrCargoReleaseValidationMode);
		}

		public void TestFDAStatusIsUpdatedOnTariffChange()
		{
			InvoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertEquals("FDA Msg Status", ZString.Empty, InvoiceLine.Declaration.FDAMsgStatus);

			InvoiceLine.US_SupTariff = USCTariff.AGOABenefitsApplicable;
			InvoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertEquals("FDA Msg Status should be set", FDAStatusList.Codes.NR, InvoiceLine.Declaration.FDAMsgStatus);

			InvoiceLine.JI_Tariff = USCTariff.FDAAdmissibilityReviewRequiredTariff;
			AssertEquals("FDA Msg Status should be updated to required when an FDA required Tariff is entered", FDAStatusList.Codes.REQ, InvoiceLine.Declaration.FDAMsgStatus);

			InvoiceLine.JI_Tariff = "";
			InvoiceLine.US_SupTariff = "9814005000";//FDARequired
			AssertEquals("FDA Msg Status should be updated to required when an FDA required Tariff is entered", FDAStatusList.Codes.REQ, InvoiceLine.Declaration.FDAMsgStatus);

			InvoiceLine.US_SupTariff = USCTariff.AGOABenefitsApplicable;
			AssertEquals("Changing back to Non FDA tariff should re-set FDA Msg Status back to not required", FDAStatusList.Codes.NR, InvoiceLine.Declaration.FDAMsgStatus);
		}

		public void TestFDAStatusIsUpdateWhenALineIsAttach()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			var invoice = Factory.NewWithValidTestData<JobComInvoiceHeader>();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.FillWithValidTestData();
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;

			Factory.Save();

			AssertEquals(ZString.Empty, dec.FDAMsgStatus);
			dec.Invoices.Add(invoice);
			AssertEquals(FDAStatusList.Codes.REQ, dec.FDAMsgStatus);
			dec.Invoices.Delete(invoice);
			AssertEquals(FDAStatusList.Codes.NR, dec.FDAMsgStatus);

			var dec2 = Factory.NewWithValidTestData<JobDeclaration>();
			dec2.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec2.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec2.US_EnableSPN = true;
			var dec2Invoice = Factory.NewWithValidTestData<JobComInvoiceHeader>();
			var dec2InvoiceLine = dec2Invoice.JobComInvoiceLines.AddNew();
			dec2InvoiceLine.FillWithValidTestData();
			dec2InvoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;

			Factory.Save();

			AssertEquals(ZString.Empty, dec2.FDAMsgStatus);
			dec2.Invoices.Add(dec2Invoice);
			AssertEquals(ZString.Empty, dec2.FDAMsgStatus);
			dec2.Invoices.Delete(dec2Invoice);
			AssertEquals(ZString.Empty, dec2.FDAMsgStatus);
		}

		public void TestFDAStatusIsUpdatedOnLineDelete()
		{
			InvoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertEquals("FDA Msg Status", ZString.Empty, InvoiceLine.Declaration.FDAMsgStatus);

			InvoiceLine.US_SupTariff = USCTariff.AGOABenefitsApplicable;
			InvoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertEquals("FDA Msg Status should be set", FDAStatusList.Codes.NR, InvoiceLine.Declaration.FDAMsgStatus);

			InvoiceLine.JI_Tariff = USCTariff.FDAAdmissibilityReviewRequiredTariff;
			AssertEquals("FDA Msg Status should be updated to required", FDAStatusList.Codes.REQ, InvoiceLine.Declaration.FDAMsgStatus);
			Factory.Save();

			InvoiceLine.US_FDAIndicator = "";
			Declaration.InvoiceLines[0].Delete();
			AssertEquals("FDA Msg Status should be re-set back to not required", FDAStatusList.Codes.NR, InvoiceLine.Declaration.FDAMsgStatus);
		}

		public void TestFDAStatusIsUpdatedOnTariffChangedIsInDb()
		{
			InvoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			Factory.Save();
			AssertEquals("FDA Msg Status", ZString.Empty, InvoiceLine.Declaration.FDAMsgStatus);

			InvoiceLine.US_SupTariff = USCTariff.AGOABenefitsApplicable;
			InvoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			Factory.Save();
			AssertEquals("FDA Msg Status should be set", FDAStatusList.Codes.NR, InvoiceLine.Declaration.FDAMsgStatus);

			InvoiceLine.JI_Tariff = USCTariff.FDAAdmissibilityReviewRequiredTariff;
			Factory.Save();
			AssertEquals("FDA Msg Status should be updated to required when an FDA required Tariff is entered", FDAStatusList.Codes.REQ, InvoiceLine.Declaration.FDAMsgStatus);

			InvoiceLine.JI_Tariff = USCTariff.LumberPermitApplicable;
			InvoiceLine.US_SupTariff = USCTariff.AGOABenefitsApplicable;
			Factory.Save();
			AssertEquals("Changing back to Non FDA tariff should re-set FDA Msg Status back to not required", FDAStatusList.Codes.NR, InvoiceLine.Declaration.FDAMsgStatus);
		}

		public void TestFDAStatusIsUpdatedOnLineDeleteWhenLineInDb()
		{
			InvoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			Factory.Save();
			AssertEquals("FDA Msg Status", ZString.Empty, InvoiceLine.Declaration.FDAMsgStatus);

			InvoiceLine.US_SupTariff = USCTariff.AGOABenefitsApplicable;
			InvoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			Factory.Save();
			AssertEquals("FDA Msg Status should be set", FDAStatusList.Codes.NR, InvoiceLine.Declaration.FDAMsgStatus);

			InvoiceLine.JI_Tariff = USCTariff.FDAAdmissibilityReviewRequiredTariff;
			Factory.Save();
			AssertEquals("FDA Msg Status should be updated to required", FDAStatusList.Codes.REQ, InvoiceLine.Declaration.FDAMsgStatus);

			InvoiceLine.US_FDAIndicator = "";
			Declaration.InvoiceLines[0].Delete();
			Factory.Save();
			AssertEquals("FDA Msg Status should be re-set back to not required", FDAStatusList.Codes.NR, InvoiceLine.Declaration.FDAMsgStatus);
		}

		public void TestRefreshingTariffClearsCustomsQtysWhenUQReadOnly()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CustomsUnitQty = "M3";
			invoiceLine.JI_CustomsQuantity = 90m;
			invoiceLine.JI_CustomsSecondUnitQty = "L";
			invoiceLine.JI_CustomsSecondQuantity = 100m;
			invoiceLine.JI_CustomsThirdUnitQty = "KK";
			invoiceLine.JI_CustomsThirdQuantity = 200m;

			invoiceLine.JI_Tariff = USCTariff.LumberPermitApplicable;
			AssertEquals("PreCondition", "M3", invoiceLine.JI_CustomsUnitQty);
			AssertEquals("Does not clear if UQ is the same", 90m, invoiceLine.JI_CustomsQuantity);
			AssertEquals(ZDecimal.Zero, invoiceLine.JI_CustomsSecondQuantity);
			AssertEquals(ZDecimal.Zero, invoiceLine.JI_CustomsThirdQuantity);
		}

		public void TestChangingOrClearingTariffRefreshesMiscLicenceLabel()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "98211119";
			tariff.UE_PermitLicenseIndicator = "07";
			tariff.UE_DateFrom = ZDateTime.Today;
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			InvoiceLine.JI_Tariff = tariff.UE_Tariff;
			AssertEquals("ATPDEA Cert No.", InvoiceLine.CalcMiscLicenseTypeLabel);

			InvoiceLine.JI_Tariff = "";
			AssertEquals("Misc. License No.", InvoiceLine.CalcMiscLicenseTypeLabel);
		}

		public void TestQtyAreReadOnlyWhenUQIsEmpty()
		{
			InvoiceLine.JI_CustomsSecondUnitQty = "";
			AssertEquals("JI_CustomsSecondQuantityInfo.ReadOnly", true, InvoiceLine.JI_CustomsSecondQuantity_ReadOnly);
			InvoiceLine.JI_CustomsSecondUnitQty = "NO";
			AssertEquals("JI_CustomsSecondQuantityInfo.ReadOnly", false, InvoiceLine.JI_CustomsSecondQuantity_ReadOnly);

			InvoiceLine.JI_CustomsThirdUnitQty = "";
			AssertEquals("JI_CustomsThirdQuantityInfo.ReadOnly", true, InvoiceLine.JI_CustomsThirdQuantity_ReadOnly);
			InvoiceLine.JI_CustomsThirdUnitQty = "NO";
			AssertEquals("JI_CustomsThirdQuantityInfo.ReadOnly", false, InvoiceLine.JI_CustomsThirdQuantity_ReadOnly);
		}

		public void TestCustomsQuantityFieldsReadOnly()
		{
			InvoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(true, InvoiceLine.JI_CustomsSecondQuantity_ReadOnly);
			AssertEquals(true, InvoiceLine.JI_CustomsThirdQuantity_ReadOnly);

			InvoiceLine.JI_CustomsSecondUnitQty = "KG";
			InvoiceLine.JI_CustomsThirdUnitQty = "KG";
			AssertEquals(false, InvoiceLine.JI_CustomsSecondQuantity_ReadOnly);
			AssertEquals(false, InvoiceLine.JI_CustomsThirdQuantity_ReadOnly);

			InvoiceLine.JI_CustomsThirdUnitQty = AESUnitOfMeasureList.Codes.NoUnitRequired;
			InvoiceLine.JI_CustomsSecondUnitQty = AESUnitOfMeasureList.Codes.NoUnitRequired;
			AssertEquals(true, InvoiceLine.JI_CustomsSecondQuantity_ReadOnly);
			AssertEquals(true, InvoiceLine.JI_CustomsThirdQuantity_ReadOnly);

			InvoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			InvoiceLine.JI_CustomsSecondUnitQty = "";
			InvoiceLine.JI_CustomsThirdUnitQty = "";
			AssertEquals(true, InvoiceLine.JI_CustomsSecondQuantity_ReadOnly);
			AssertEquals(true, InvoiceLine.JI_CustomsThirdQuantity_ReadOnly);

			InvoiceLine.JI_CustomsSecondUnitQty = "KG";
			InvoiceLine.JI_CustomsThirdUnitQty = "KG";
			AssertEquals(false, InvoiceLine.JI_CustomsSecondQuantity_ReadOnly);
			AssertEquals(false, InvoiceLine.JI_CustomsThirdQuantity_ReadOnly);

			InvoiceLine.JI_CustomsThirdUnitQty = ABIUnitOfMeasureList.Codes.NoUnitRequired;
			InvoiceLine.JI_CustomsSecondUnitQty = ABIUnitOfMeasureList.Codes.NoUnitRequired;
			AssertEquals(true, InvoiceLine.JI_CustomsSecondQuantity_ReadOnly);
			AssertEquals(true, InvoiceLine.JI_CustomsThirdQuantity_ReadOnly);
		}

		public void TestCopyQuantitesToADDQty()
		{
			var acCase = Factory.New<USCACCase>();
			acCase.U5_CaseNumber = "A462105011";
			acCase.U5_CaseStatus = "AC";
			acCase.U5_CaseStatusDate = ZDateTime.BrettsBirthday;
			var caseTariff = acCase.CaseTariffs.AddNew();
			caseTariff.U9_TariffNumber = "7201100000";

			var caseRate = acCase.CaseRates.AddNew();
			caseRate.U6_CaseNumber = "A462105011";
			caseRate.U6_EffectiveDate = ZDateTime.BrettsBirthday;
			caseRate.U6_Unit = "T";
			caseRate.U6_SpecificRate = 0.52m;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "7201.10.0000";
			invoiceLine.JI_CustomsQuantity = 2m;
			invoiceLine.US_ADDCaseNo = "A462105011";
			AssertEquals(DepositRateIndicatorList.Codes.Specific, invoiceLine.US_ADDDepositRateIndicator);
			AssertEquals(2m, invoiceLine.US_ADDQty);
		}

		public void TestCopyQuantitesToCVDQty()
		{
			var acCase = Factory.New<USCACCase>();
			acCase.U5_CaseNumber = "C462105011";
			acCase.U5_CaseStatus = "AC";
			acCase.U5_CaseStatusDate = ZDateTime.BrettsBirthday;
			var caseTariff = acCase.CaseTariffs.AddNew();
			caseTariff.U9_TariffNumber = "7201100000";

			var caseRate = acCase.CaseRates.AddNew();
			caseRate.U6_CaseNumber = "C462105011";
			caseRate.U6_EffectiveDate = ZDateTime.BrettsBirthday;
			caseRate.U6_Unit = "T";
			caseRate.U6_SpecificRate = 0.52m;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "7201.10.0000";
			invoiceLine.JI_CustomsQuantity = 2m;
			invoiceLine.US_CVDCaseNo = "C462105011";
			AssertEquals(DepositRateIndicatorList.Codes.Specific, invoiceLine.US_CVDDepositRateIndicator);
			AssertEquals(2m, invoiceLine.US_CVDQty);
		}

		public void TestChangingCaseNoResetRateIndicator()
		{
			InvoiceLine.US_ADDDepositRateIndicator = ZString.Empty;
			InvoiceLine.US_ADDCaseNo = "A580844004";
			AssertEquals(ZString.Empty, InvoiceLine.US_ADDDepositRateIndicator);

			InvoiceLine.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.AdValorem;
			InvoiceLine.US_ADDCaseNo = "A403801063";
			AssertEquals(ZString.Empty, InvoiceLine.US_ADDDepositRateIndicator);

			InvoiceLine.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.Specific;
			InvoiceLine.US_ADDCaseNo = "A403801063";
			AssertEquals(DepositRateIndicatorList.Codes.Specific, InvoiceLine.US_ADDDepositRateIndicator);

			InvoiceLine.US_CVDDepositRateIndicator = ZString.Empty;
			InvoiceLine.US_CVDCaseNo = "C580844004";
			AssertEquals(ZString.Empty, InvoiceLine.US_CVDDepositRateIndicator);

			InvoiceLine.US_CVDDepositRateIndicator = DepositRateIndicatorList.Codes.AdValorem;
			InvoiceLine.US_CVDCaseNo = "C403801063";
			AssertEquals(ZString.Empty, InvoiceLine.US_CVDDepositRateIndicator);

			InvoiceLine.US_CVDDepositRateIndicator = DepositRateIndicatorList.Codes.Specific;
			InvoiceLine.US_CVDCaseNo = "C403801063";
			AssertEquals(DepositRateIndicatorList.Codes.Specific, InvoiceLine.US_CVDDepositRateIndicator);
		}

		public void TestIDutyData()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			IDutyData dutyData = InvoiceLine;

			InvoiceLine.US_SelectedRateType = RateTypeList.Codes.Primary;
			AssertEquals("SelectedRateType", RateTypeList.Codes.Primary, dutyData.SelectedRateType);

			InvoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.F;
			AssertEquals("SpecialProgramsIndicatorSecondary", SecondarySpecProgIndicatorList.Codes.F, dutyData.SpecialProgramsIndicatorSecondary);

			InvoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.E;
			AssertEquals("SpecialProgramsIndicatorPrimary", PrimarySpecProgramIndicatorList.Codes.E, dutyData.SpecialProgramsIndicatorPrimary);

			InvoiceLine.US_SPI = SpecialProgramList.Codes.AU;
			AssertEquals("SpecialProgramsIndicatorCountry", SpecialProgramList.Codes.AU, dutyData.SpecialProgramsIndicatorCountry);

			InvoiceLine.US_UC_NKCountryOfOrigin = "TT";
			AssertEquals("CountryOfOrigin", "TT", dutyData.CountryOfOrigin);

			InvoiceLine.JI_LinePrice = 10000.20m;
			AssertEquals("CustomsValue", 10000m, dutyData.CustomsValue);

			InvoiceLine.JI_CustomsUnitQty = "KG";
			InvoiceLine.JI_CustomsQuantity = 2.523m;
			InvoiceLine.JI_CustomsSecondUnitQty = "LT";
			InvoiceLine.JI_CustomsSecondQuantity = 4.625m;
			InvoiceLine.JI_CustomsThirdUnitQty = "NO";
			InvoiceLine.JI_CustomsThirdQuantity = 8.867m;

			AssertEquals("Quantity1 rounded", 3m, dutyData.Quantity1);
			AssertEquals("UQ1", "KG", dutyData.UQ1);
			AssertEquals("Quantity2", 5m, dutyData.Quantity2);
			AssertEquals("UQ2", "LT", dutyData.UQ2);
			AssertEquals("Quantity3", 9m, dutyData.Quantity3);
			AssertEquals("UQ3", "NO", dutyData.UQ3);

			InvoiceLine.JI_CustomsUnitQty = ABIUnitOfMeasureList.Codes.ProofLiter;
			InvoiceLine.JI_CustomsQuantity = 2.523m;
			InvoiceLine.JI_CustomsSecondUnitQty = ABIUnitOfMeasureList.Codes.ProofLiter;
			InvoiceLine.JI_CustomsSecondQuantity = 4.625m;
			InvoiceLine.JI_CustomsThirdUnitQty = ABIUnitOfMeasureList.Codes.ProofLiter;
			InvoiceLine.JI_CustomsThirdQuantity = 8.867m;

			AssertEquals("Quantity1 rounded to whole number, even if UQ is ProofLiter", 3m, dutyData.Quantity1);
			AssertEquals("Quantity2 rounded to whole number, even if UQ is ProofLiter", 5m, dutyData.Quantity2);
			AssertEquals("Quantity3 rounded to whole number, even if UQ is ProofLiter", 9m, dutyData.Quantity3);

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "10000000";
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.SpecificRateFirstQuantity;
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);
			tariff.UE_Column1RateSpecific = 1.44m;

			InvoiceLine.JI_Tariff = tariff.UE_Tariff;
			InvoiceLine.JI_CustomsQuantity = 2.523m;

			AssertEquals("Quantity1 rounded to two decimals", 2.52m, dutyData.Quantity1);

			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.SpecificRateSecondQuantity;
			tariff.UE_Column2RateSpecific = 1.44m;
			InvoiceLine.US_UC_NKCountryOfOrigin = "CU";
			InvoiceLine.JI_CustomsSecondQuantity = 4.625m;
			InvoiceLine.JI_CustomsThirdQuantity = 8.867m;

			AssertEquals("Quantity2 rounded to two decimals", 4.63m, dutyData.Quantity2);

			InvoiceLine.US_UC_NKCountryOfOrigin = "AU";
			InvoiceLine.US_TaxRateT = RateTypeList.Codes.Primary;
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.CompoundSpecificAdValorem;
			var dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Wines;
			dutyRate.UD_TaxFeeFlag = "1";
			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.CompoundSpecificAdValorem;
			dutyRate.UD_TaxFeeAdvalorem = 1.4m;
			dutyRate.UD_TaxFeeSpecificRate = 1.6m;

			AssertEquals("Quantity3 rounded to two decimals", 8.87m, dutyData.Quantity3);

			InvoiceLine.JI_Tariff = new TariffFormatter().DisplayFormat(USCTariff.AGOABenefitsApplicable);
			AssertEquals("Tariff", USCTariff.AGOABenefitsApplicable, dutyData.Tariff);
		}

		public override void TestJI_FormattedTariff()
		{
			ZString tariff = "1234567890";
			InvoiceLine.JI_Tariff = tariff;
			AssertEquals("JI_FormattedTariff", "1234.56.7890", InvoiceLine.JI_FormattedTariff);
			tariff = "9876.54.3210";
			InvoiceLine.JI_FormattedTariff = "9876.54.3210";
			AssertEquals("JI_FormattedTariff", tariff, InvoiceLine.JI_FormattedTariff);
		}

		public void TestFTZCurrentTariffFormatted()
		{
			ZString tariff = "1234567890";
			InvoiceLine.US_FTZCurrentTariff = tariff;
			AssertEquals("FTZCurrentTariffFormatted", "1234.56.7890", InvoiceLine.FTZCurrentTariffFormatted);
			tariff = "9876.54.3210";
			InvoiceLine.US_FTZCurrentTariff = "9876.54.3210";
			AssertEquals("FTZCurrentTariffFormatted", tariff, InvoiceLine.FTZCurrentTariffFormatted);
		}

		public void TestCountryOfOriginFromProduct()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;

			var product1 = Factory.New<OrgSupplierPart>();
			product1.OP_PartNum = "Test Product1";
			product1.RelatedOrganisations.AddOwner(importer);
			var pivot = product1.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "9101000010";
			pivot.CD_UC_NKCountryOfOrigin = "AU";

			var product2 = Factory.New<OrgSupplierPart>();
			product2.OP_PartNum = "Test Product2";
			product2.RelatedOrganisations.AddOwner(importer);

			var product3 = Factory.New<OrgSupplierPart>();
			product3.OP_PartNum = "Test Product3";
			product3.RelatedOrganisations.AddOwner(importer);

			var invoice = declaration.Invoices.AddNew();
			invoice.US_UC_NKCountryOfOrigin = "HK";
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = product1.OP_PartNum;

			AssertEquals("pre-condition", "AU", invoiceLine.US_UC_NKCountryOfOrigin);

			invoiceLine.JI_PartNo = product2.OP_PartNum;
			AssertEquals("Country of Origin should not be changed", "AU", invoiceLine.US_UC_NKCountryOfOrigin);

			invoice.US_UC_NKCountryOfOrigin = ZString.Empty;
			invoiceLine.US_UC_NKCountryOfOrigin = "CN";
			invoiceLine.JI_PartNo = product2.OP_PartNum;
			AssertEquals("Country of Origin should not be changed", "CN", invoiceLine.US_UC_NKCountryOfOrigin);
		}

		public void TestCountryOfExportFromProduct()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;

			var product1 = Factory.New<OrgSupplierPart>();
			product1.OP_PartNum = "Test Product1";
			product1.RelatedOrganisations.AddOwner(importer);
			var pivot = product1.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "9101000010";
			pivot.CD_UC_NKCountryOfExport = "AU";

			var product2 = Factory.New<OrgSupplierPart>();
			product2.OP_PartNum = "Test Product2";
			product2.RelatedOrganisations.AddOwner(importer);

			var product3 = Factory.New<OrgSupplierPart>();
			product3.OP_PartNum = "Test Product3";
			product3.RelatedOrganisations.AddOwner(importer);

			var invoice = declaration.Invoices.AddNew();
			invoice.US_UC_NKCountryOfExport = "HK";
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = product1.OP_PartNum;

			AssertEquals("pre-condition", "AU", invoiceLine.US_UC_NKCountryOfExport);

			invoiceLine.JI_PartNo = product2.OP_PartNum;
			AssertEquals("Country of Exoirt should not be changed", "AU", invoiceLine.US_UC_NKCountryOfExport);

			invoice.US_UC_NKCountryOfExport = ZString.Empty;
			invoiceLine.US_UC_NKCountryOfExport = "CN";
			invoiceLine.JI_PartNo = product2.OP_PartNum;
			AssertEquals("Country of Exoirt should not be changed", "CN", invoiceLine.US_UC_NKCountryOfExport);
		}

		public void TestOriginAndExportCountryWhenAddingNewPart()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "TEST NAME";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.HongKong;
			invoiceHeader.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.NewZealand;
			var cusClass = Factory.New<CusClassification>();
			cusClass.CC_LookupCode = "TestLookup";
			cusClass.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			cusClass.CC_TariffNum = "0121323122";
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CC = cusClass.PK;
			invoiceLine.JI_PartNo = "TEST123";
			var collection = new OrgSupplierPartCollection(Factory, invoiceLine, invoiceLine.InvoiceHeader?.IsExport ?? ZBool.False);
			var part = collection.AddNew();
			var importPivot = part.PivotsForBinding.GetImportMatch(org.PK, ZGuid.Empty);
			AssertEquals("Origin Country should be empty", ZString.Empty, importPivot.CD_UC_NKCountryOfOrigin);
			AssertEquals("Export Country should be empty", ZString.Empty, importPivot.CD_UC_NKCountryOfExport);
		}

		public void TestLinesValuesArePopulatedFromProductDetails()
		{
			var product1 = Factory.New<OrgSupplierPart>();
			product1.OP_PartNum = "Test Product1";
			product1.OP_Weight = 100;
			product1.OP_WeightUQ = Core.Constants.Weight.MetricCarat;
			product1.OP_NetWeight = 9;

			var owner = Factory.New<OrgHeader>();
			var orgRel = product1.RelatedOrganisations.AddOrganisationIfNotExist(owner.PK, OrgPartRelation.RelationshipTypes.Owner);

			var pivot = product1.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "9404908522";
			pivot.CI_OH = orgRel.OU_OH;
			pivot.CD_PrimaryCountryNA = true;
			pivot.CD_SecondaryCountryNA = true;

			var component = pivot.Children.AddNew();
			component.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			component.CI_TariffNum = "6302319020";
			component.CD_CottonFeeExempt = YesNoDefaultList.Codes.No;
			component.CD_CottonCertificate = "001COM";
			component.CD_CVDApplicable = true;
			component.CD_RN_NKPrimaryCountry = "RU";
			component.CD_RN_NKSecondaryCountry = "CN";
			component.CD_RN_NKCastCountry = "SG";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = owner.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var header = declaration.Invoices.AddNew();
			header.JZ_JE = declaration.PK;
			var invoiceLine = header.InvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "Test Product1";
			AssertEquals(true, invoiceLine.US_Prim_NA);
			AssertEquals(true, invoiceLine.US_Sec_NA);

			var childInvoiceLine = invoiceLine.ChildLines.ElementAt(0);
			AssertEquals(YesNoDefaultList.Codes.No, childInvoiceLine.US_CottonFeeExempt);
			AssertEquals(1, childInvoiceLine.LicenceAndPermits.Count);
			AssertEquals("001COM", childInvoiceLine.LicenceAndPermits[0].CY_Data);
			AssertEquals(ZString.Empty, childInvoiceLine.US_FCCIndicator);
			AssertEquals(true, childInvoiceLine.US_CVD_NA);
			AssertEquals("RU", childInvoiceLine.US_RN_NKPrimCtry);
			AssertEquals("CN", childInvoiceLine.US_RN_NKSecCtry);
			AssertEquals("SG", childInvoiceLine.US_RN_NKCastCtry);

			var invoiceLine2 = header.InvoiceLines.AddNew();
			invoiceLine2.JI_PartNo = "Test Product1";
			var childInvoiceLine2 = invoiceLine2.ChildLines.ElementAt(0);
			AssertEquals("", childInvoiceLine2.US_FCCIndicator);
		}

		public void TestUS_TariffType()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.US_TariffType = TariffTypeList.Codes.ScheduleB;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_InvoiceQuantity = 100m;
			AssertEquals("Tariff type should default from Declaration", "SHB", invoiceLine.US_TariffType);

			invoiceLine.US_TariffType = TariffTypeList.Codes.HTS;
			Factory.Save();
			AssertEquals("Tariff type should now reflect value entered on Invoice Line", "HTS", invoiceLine.US_TariffType);
		}

		[TestDate(2006, 9, 6)]
		public void TestIUSUltimateDistributee()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();

			USCACCase addCase1 = Factory.New<USCACCase>();
			addCase1.U5_CaseNumber = "A580844004";
			addCase1.U5_ISOCountryCode = "KR";
			addCase1.U5_CaseStatus = ACCaseStatusList.Codes.IO;
			addCase1.CaseTariffs.AddNew().U9_TariffNumber = "72142000";
			addCase1.CaseTariffs.AddNew().U9_TariffNumber = "7222110050";
			addCase1.CaseTariffs.AddNew().U9_TariffNumber = "7228606000";
			addCase1.CaseTariffs.AddNew().U9_TariffNumber = "7228308050";
			var rate1 = addCase1.CaseRates.AddNew();
			rate1.U6_AdValoremRate = 1.02m;
			rate1.U6_EffectiveDate = ZDateTime.Today;

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			declaration.US_EnableENS = true;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_UC_NKCountryOfExport = "KR";
			invoice.US_UC_NKCountryOfOrigin = "KR";

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "7222.11.00 50";
			invoiceLine1.JI_CustomsQuantity = 70m;
			invoiceLine1.US_ADDCaseNo = "A580844004";
			invoiceLine1.JI_LinePrice = 4000m;

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 6000m;
			invoiceLine2.JI_Tariff = "2402103030";
			invoiceLine2.US_SPI = "AU";//MPF exempt
			invoiceLine2.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine2.JI_CustomsQuantity = 1000m;
			invoiceLine2.JI_CustomsSecondQuantity = 1360m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			IUltimateDistributee distributee = invoiceLine1;
			AssertEquals("KR", distributee.CountryOfOriginCode);
			DutyTaxEntryFee entryFee = distributee.LineDutyTaxEntryFeeItems;
			AssertEquals("Duty Amount", 0m, entryFee["TDT"]);
			AssertEquals("MPF 25$", 25m, entryFee["ST1"]);
			AssertEquals("HMF 5$", 5m, entryFee["ST2"]);
			AssertEquals("Other Duty (ADD/CVD)", 4080.00m, entryFee["OTH"]);

			distributee = invoiceLine2;
			AssertEquals("AU", distributee.CountryOfOriginCode);
			entryFee = distributee.LineDutyTaxEntryFeeItems;
			AssertEquals("Duty Amount", 1423.20m, entryFee["TDT"]);
			AssertEquals("MPF 0$", 0m, entryFee["ST1"]);
			AssertEquals("HMF 5$", 7.5m, entryFee["ST2"]);
			AssertEquals("Excise", 1828m, entryFee["EXC"]);
		}

		[TestDate(2008, 3, 25)]
		public void TestADD_CVDCurrency()
		{
			using (DeclarationTestHelper.SetReciprocalFlagForCurrentCompany(true))
			{
				RefCurrency nzCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.NewZealand);
				InvoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = nzCurrency.RX_Code;
				AssertEquals(nzCurrency.PK, InvoiceLine.US_RX_ADDDepositValueCurrency);
				AssertEquals(nzCurrency.PK, InvoiceLine.US_RX_CVDDepositValueCurrency);

				var rate = new RefExchangeRate.Loader(Factory).GetEffectiveRateOn(new ZDateTime(2008, 3, 25), nzCurrency.RX_Code, Core.Constants.ExchangeRateTypes.Code.CustomsRate, GlbCompany.CurrentCompany.PK) ?? InvoiceLine.InvoiceHeader.Invoice_Currency.ExchangeRates.AddNew();
				rate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
				rate.RE_StartDate = new ZDateTime(2008, 3, 25);
				rate.RE_ExpiryDate = new ZDateTime(2008, 3, 25);
				rate.RE_SellRate = 0.78m;

				InvoiceLine.US_ADDDepositValue = 5000m;
				InvoiceLine.US_CVDDepositValue = 6000m;

				AssertEquals("ADD deposit value in local currency", 3900m, InvoiceLine.ADDDepositValueInLocalCurrency);
				AssertEquals("ADD deposit value in local currency", 4680m, InvoiceLine.CVDDepositValueInLocalCurrency);
			}
		}

		[TestDate(2008, 3, 25)]
		public void TestADD_CVDDepositRate()
		{
			USCACCase addCase = Factory.New<USCACCase>();
			addCase.U5_CaseNumber = "AZZZ23423";
			addCase.U5_CaseStatusDate = ZDateTime.Today;
			USCACCaseRate addRate = addCase.CaseRates.AddNew();
			addRate.U6_AdValoremRate = 0.2312m;
			addRate.U6_SpecificRate = 0.5234m;
			addRate.U6_Unit = "KG";
			addRate.U6_EffectiveDate = ZDateTime.Today;

			USCACCase cvdCase = Factory.New<USCACCase>();
			cvdCase.U5_CaseNumber = "CZZZ23423";
			cvdCase.U5_CaseStatusDate = ZDateTime.Today;
			USCACCaseRate cvdRate = cvdCase.CaseRates.AddNew();
			cvdRate.U6_AdValoremRate = 0.4245m;
			cvdRate.U6_SpecificRate = 0.5623m;
			cvdRate.U6_Unit = "KG";
			cvdRate.U6_EffectiveDate = ZDateTime.Today;

			InvoiceLine.US_ADDCaseNo = "AZZZ23423";
			InvoiceLine.US_CVDCaseNo = "CZZZ23423";
			AssertEquals(0.2312m, InvoiceLine.US_ADDDepositRate);
			AssertEquals(0.4245m, InvoiceLine.US_CVDDepositRate);
			InvoiceLine.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.Specific;
			InvoiceLine.US_CVDDepositRateIndicator = DepositRateIndicatorList.Codes.Specific;
			AssertEquals(0.52m, InvoiceLine.US_ADDDepositRate);
			AssertEquals(0.56m, InvoiceLine.US_CVDDepositRate);

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobComInvoiceLine lineLoaded = factory2.Load<JobComInvoiceLine>(InvoiceLine.PK);
			AssertEquals(0.52m, InvoiceLine.US_ADDDepositRate);
			AssertEquals(0.56m, InvoiceLine.US_CVDDepositRate);
		}

		public void TestTotalCustomsValueIncludingSecondaryLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var parent = declaration.InvoiceLines.AddNew();
			parent.JI_LinePrice = 10000m;
			parent.US_CustomsValue = 10000m;
			AssertEquals("TotalCustomsValueIncludingSecondaryLines", 10000m, parent.TotalCustomsValueIncludingSecondaryLines);

			parent.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			AssertEquals("TotalCustomsValueIncludingSecondaryLines", 0m, parent.TotalCustomsValueIncludingSecondaryLines);

			var child = parent.AddSecondaryInvoiceLine();
			child.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			child.JI_LinePrice = 10000m;
			child.US_CustomsValue = 10000m;
			AssertEquals("TotalCustomsValueIncludingSecondaryLines", 10000m, parent.TotalCustomsValueIncludingSecondaryLines);

			parent.US_SecondarySPI = "";
			parent.JI_LinePrice = 10000m;
			AssertEquals("child is not a V line any more", false, child.IsSetVLine);
			child.JI_ParentID = parent.PK;
			AssertEquals("TotalCustomsValueIncludingSecondaryLines", 20000m, parent.TotalCustomsValueIncludingSecondaryLines);
		}

		[TestDate(2019, 12, 31)]
		public void TestJI_Calc_DutyAmountWhenParentDoesNotHaveComponentEntered()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.US_UC_NKCountryOfExport = "SV";
			invoice.US_UC_NKCountryOfOrigin = "SV";

			JobComInvoiceLine parent1 = declaration.InvoiceLines.AddNew();
			parent1.US_SupTariff = "9915.04.90";
			parent1.US_SPI = SpecialProgramList.Codes.PPlus;
			parent1.US_SupQty1 = 70.00000m;
			parent1.JI_Tariff = "0406.10.08 00";
			parent1.JI_CustomsQuantity = 10000.00000m;
			parent1.JI_LinePrice = 10000.00m;

			JobComInvoiceLine parent2 = declaration.InvoiceLines.AddNew();
			parent2.US_SupTariff = "9915.04.90";
			parent2.US_SPI = SpecialProgramList.Codes.PPlus;
			parent2.US_SupQty1 = 70.00000m;
			parent2.JI_Tariff = "0406.10.08 00";
			parent2.JI_CustomsQuantity = 10000.00000m;
			parent2.JI_LinePrice = 10000.00m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals("Users can see duty amount for parent1 even though parent1 does not have a component price entered", 105.63m, parent1.JI_Calc_DutyAmount);
			AssertEquals("Users can see duty amount for parent2 even though parent2 does not have a component price entered", 105.63m, parent2.JI_Calc_DutyAmount);

			AssertEquals(211.26m, declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalDutyAmount);
		}

		public override void TestJI_Calc_MergedLineNumber()
		{
			CusEntryHeader entryHeader = Factory.New<CusEntryHeader>();
			CusEntryLine mergedLine = entryHeader.MergedLines.AddNew();
			AssertEquals("Prior to merge", "Not Merged", InvoiceLine.JI_Calc_MergedLineNumber);
			InvoiceLine.JI_CL = mergedLine.PK;
			mergedLine.CL_LineNumber = 1;
			AssertEquals("After merge", "001", InvoiceLine.JI_Calc_MergedLineNumber);
		}

		public void TestIsDutyFreeSPIClaimed()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;
			tariff.UE_Column1RateAdValorem = 0.0543m;
			tariff.UE_SPICode = "AUMX";

			USCTariffDutyRate dutyRateForMX = tariff.DutyRates.AddNew();
			dutyRateForMX.UD_ISOCountryCode = "MX";
			dutyRateForMX.UD_TaxFeeComputationCode = ComputationCodeList.Codes.AdValorem;
			dutyRateForMX.UD_AdValoremSpecialRate = 0.0243m;

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			CusEntryLine entryLine = Declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			InvoiceLine.JI_Tariff = "0000000000";
			InvoiceLine.JI_CL = entryLine.PK;

			entryLine.RefreshInvoiceLines();
			AssertNotNull(InvoiceLine.ImportTariff);

			AssertEquals("IsDutyFreeSPIClaimed", false, InvoiceLine.IsDutyFreeSPIClaimed);
			AssertEquals("IsDutyFreeSPIClaimed", false, entryLine.IsDutyFreeSPIClaimed);

			InvoiceLine.US_SPI = "AU";
			AssertEquals("IsDutyFreeSPIClaimed", true, InvoiceLine.IsDutyFreeSPIClaimed);
			AssertEquals("IsDutyFreeSPIClaimed", true, entryLine.IsDutyFreeSPIClaimed);

			InvoiceLine.US_SPI = "MX";
			AssertEquals("IsDutyFreeSPIClaimed", false, InvoiceLine.IsDutyFreeSPIClaimed);
			AssertEquals("IsDutyFreeSPIClaimed", false, entryLine.IsDutyFreeSPIClaimed);
		}

		public void TestIsDutyFreeSPIClaimed2()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;
			tariff.UE_Column1RateAdValorem = 0.0543m;
			tariff.UE_SPICode = "AUMX";

			USCTariffDutyRate dutyRateForMX = tariff.DutyRates.AddNew();
			dutyRateForMX.UD_ISOCountryCode = "MX";
			dutyRateForMX.UD_TaxFeeComputationCode = ComputationCodeList.Codes.AdValorem;
			dutyRateForMX.UD_AdValoremSpecialRate = 0.0243m;

			USCTariff supTariff = Factory.New<USCTariff>();
			supTariff.UE_Tariff = "9999999999";
			supTariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			supTariff.UE_DateTo = ZDateTime.Today;
			supTariff.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;
			supTariff.UE_Column1RateAdValorem = 0.0321m;
			supTariff.UE_SPICode = "AUMX";

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			CusEntryLine entryLine = Declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			InvoiceLine.JI_Tariff = "0000000000";
			InvoiceLine.US_SupTariff = "9999999999";
			InvoiceLine.JI_CL = entryLine.PK;

			entryLine.RefreshInvoiceLines();
			AssertNotNull(InvoiceLine.ImportTariff);

			AssertEquals("IsDutyFreeSPIClaimed", false, InvoiceLine.IsDutyFreeSPIClaimed);
			AssertEquals("IsDutyFreeSPIClaimed", false, entryLine.IsDutyFreeSPIClaimed);

			InvoiceLine.US_SPI = "AU";
			AssertEquals("IsDutyFreeSPIClaimed", true, InvoiceLine.IsDutyFreeSPIClaimed);
			AssertEquals("IsDutyFreeSPIClaimed", true, entryLine.IsDutyFreeSPIClaimed);

			InvoiceLine.US_SPI = "MX";
			AssertEquals("IsDutyFreeSPIClaimed", false, InvoiceLine.IsDutyFreeSPIClaimed);
			AssertEquals("IsDutyFreeSPIClaimed", false, entryLine.IsDutyFreeSPIClaimed);
		}

		public void TestSecondarySPIForChildLineForXAndV()
		{
			JobComInvoiceLine parentLine = InvoiceHeader.JobComInvoiceLines.AddNew();
			parentLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			AssertEquals("Secondary SPI", SecondarySpecProgIndicatorList.Codes.X, parentLine.US_SecondarySPI);

			JobComInvoiceLine childLine = parentLine.AddSecondaryInvoiceLine();
			AssertEquals("Secondary SPI of an invoice line which is a child of X line", SecondarySpecProgIndicatorList.Codes.V, childLine.US_SecondarySPI);

			childLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			AssertEquals("Secondary SPI of an invoice line which is a child of X line", SecondarySpecProgIndicatorList.Codes.V, childLine.US_SecondarySPI);
		}

		public void TestClearValuesForChildLines()
		{
			JobComInvoiceLine parentline = InvoiceHeader.JobComInvoiceLines.AddNew();
			parentline.JI_LinePrice = 1000m;
			parentline.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			AssertEquals("Line price should be cleared for non-recon declarations", ZDecimal.Zero, parentline.JI_LinePrice);

			JobComInvoiceLine childLine = InvoiceHeader.JobComInvoiceLines.AddNew();
			childLine.US_UC_NKCountryOfOrigin = "AU";
			childLine.US_UC_NKCountryOfExport = "NZ";
			AssertEquals("AU", childLine.US_UC_NKCountryOfOrigin);
			AssertEquals("NZ", childLine.US_UC_NKCountryOfExport);

			childLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			AssertEquals("IsChildLine", true, childLine.IsChildLine);
			AssertEquals("But it is not secondary line", false, childLine.IsSecondaryTariffLine);

			AssertEquals("AU", childLine.US_UC_NKCountryOfOrigin);
			AssertEquals("NZ", childLine.US_UC_NKCountryOfExport);

			parentline.US_SecondarySPI = "";
			childLine.US_SecondarySPI = "";
			childLine.JI_ParentID = parentline.PK;
			AssertEquals("becomes a secondary line", true, childLine.IsSecondaryTariffLine);
			AssertEquals("AU", childLine.US_UC_NKCountryOfOrigin);
			AssertEquals("NZ", childLine.US_UC_NKCountryOfExport);

			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var originalEntry = reconDec.OriginalEntries.AddNew();

			var invoice = originalEntry.Invoice;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1000m;
			InvoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			AssertEquals("Line price should not be cleared for a recon declaration", ZDecimal.Zero, parentline.JI_LinePrice);
		}

		public void TestUS_DestinationState()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			Declaration.US_DestinationState = "IL";
			AssertEquals("Destination state", "IL", InvoiceLine.US_DestinationState);

			InvoiceHeader.US_DestinationState = "AL";
			AssertEquals("Destination state", "AL", InvoiceLine.US_DestinationState);

			InvoiceLine.US_DestinationState = "CA";
			AssertEquals("Destination state", "CA", InvoiceLine.US_DestinationState);
		}

		public void TestUS_TransactionsRelated()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			InvoiceHeader.US_TransactionsRelated = YesNoDefaultList.Codes.No;
			AssertEquals("Transactions Related - invoice header", "N", InvoiceHeader.US_TransactionsRelated);
			AssertEquals("Transactions Related - invoice line", "N", InvoiceLine.US_TransactionsRelated);

			InvoiceLine.US_TransactionsRelated = YesNoDefaultList.Codes.Yes;
			AssertEquals("Transactions Related - invoice header", "N", InvoiceHeader.US_TransactionsRelated);
			AssertEquals("Transactions Related - invoice line", "Y", InvoiceLine.US_TransactionsRelated);

			JobComInvoiceLine childLine = InvoiceHeader.JobComInvoiceLines.AddNew();
			childLine.JI_ParentID = InvoiceLine.PK;
			AssertEquals("childLine is a secondary line", true, childLine.IsSecondaryTariffLine);
			AssertEquals("childLine trans related should be same as parent line", InvoiceLine.US_TransactionsRelated, childLine.US_TransactionsRelated);

			InvoiceLine.US_TransactionsRelated = YesNoDefaultList.Codes.No;
			AssertEquals("childLine trans related should be same as parent line", InvoiceLine.US_TransactionsRelated, childLine.US_TransactionsRelated);
		}

		public void TestIsTIBEntryType()
		{
			InvoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			InvoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.AircraftVesselSupplyIE;
			AssertEquals(false, InvoiceLine.IsTIBEntryType);

			InvoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			AssertEquals(true, InvoiceLine.IsTIBEntryType);
		}

		public void TestCountryOfOriginAndExportForSecondaryLines()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.US_UC_NKCountryOfExport = "AU";
			invoice.US_UC_NKCountryOfOrigin = "NZ";

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			AssertEquals("AU", invoiceLine.US_UC_NKCountryOfExport);
			AssertEquals("NZ", invoiceLine.US_UC_NKCountryOfOrigin);

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			AssertEquals("AU", invoiceLine2.US_UC_NKCountryOfExport);
			AssertEquals("NZ", invoiceLine2.US_UC_NKCountryOfOrigin);

			invoiceLine.US_UC_NKCountryOfExport = "KR";
			invoiceLine.US_UC_NKCountryOfOrigin = "JP";
			AssertEquals("KR", invoiceLine.US_UC_NKCountryOfExport);
			AssertEquals("JP", invoiceLine.US_UC_NKCountryOfOrigin);
			AssertEquals("AU", invoiceLine2.US_UC_NKCountryOfExport);
			AssertEquals("NZ", invoiceLine2.US_UC_NKCountryOfOrigin);

			invoiceLine2.JI_ParentID = invoiceLine.PK;//is secondary
			AssertEquals("KR", invoiceLine2.US_UC_NKCountryOfExport);
			AssertEquals("JP", invoiceLine2.US_UC_NKCountryOfOrigin);

			invoiceLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;//is not secondary any more
			AssertEquals("AU", invoiceLine2.US_UC_NKCountryOfExport);
			AssertEquals("NZ", invoiceLine2.US_UC_NKCountryOfOrigin);

			invoiceLine2.US_UC_NKCountryOfExport = "FR";
			invoiceLine2.US_UC_NKCountryOfOrigin = "FR";
			AssertEquals("FR", invoiceLine2.US_UC_NKCountryOfExport);
			AssertEquals("FR", invoiceLine2.US_UC_NKCountryOfOrigin);

			invoiceLine2.US_SecondarySPI = "";//is secondary now
			AssertEquals("FR", invoiceLine2.US_UC_NKCountryOfExport);
			AssertEquals("FR", invoiceLine2.US_UC_NKCountryOfOrigin);
		}

		public void TestUS_SchDLoading()
		{
			var parentline = InvoiceHeader.JobComInvoiceLines.AddNew();
			parentline.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			parentline.US_SchDLoading = "52000";

			var childLine = InvoiceHeader.JobComInvoiceLines.AddNew();
			childLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			childLine.US_SchDLoading = "52001";
			AssertEquals("This is a child line, but not secondary line, port of lading should be editable", false, childLine.US_SchDLoading_ReadOnly);
			AssertEquals("52001", childLine.US_SchDLoading);

			parentline.US_SecondarySPI = ZString.Empty;
			childLine.US_SecondarySPI = ZString.Empty;
			childLine.JI_ParentID = ZGuid.Empty;
			AssertEquals("This is not a child line, not a secondary line, port of lading should be editable", false, childLine.US_SchDLoading_ReadOnly);
			AssertEquals("52001", childLine.US_SchDLoading);

			childLine.US_SchDLoading = ZString.Empty;
			childLine.JI_ParentID = parentline.PK;//is secondary
			AssertEquals("This is a secondary line, port of lading should be readonly", true, childLine.US_SchDLoading_ReadOnly);
			AssertEquals("Port Of Loading comes from Parent Line", "52000", childLine.US_SchDLoading);
		}

		public void TestUnitFromProductIsConverted()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			var abiList = new ABIUnitOfMeasureList();
			var supplier = newFactory.New<OrgHeader>();
			supplier.OH_Code = "SUPZZ!123";
			var importer = newFactory.New<OrgHeader>();
			importer.OH_Code = "IMPZZ!123";

			var product = newFactory.New<OrgSupplierPart>();
			product.OP_PartNum = "ProductForTesting";

			OrgPartRelation relation1 = product.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, OrgPartRelation.RelationshipTypes.Supplier);
			relation1.OU_LocalPartNumber = "XXX";
			var refPack1 = newFactory.New<CusRefPacks>();
			refPack1.RP_CustomsCountry = Core.Constants.CountryCodes.UnitedStates;
			refPack1.RP_CommercialPack = Core.Constants.PkgUnit.Reel;
			refPack1.RP_CustomsPack = ABIUnitOfMeasureList.Codes.Pieces;
			refPack1.RP_ConversionFactor = 1;

			var refPack2 = newFactory.New<CusRefPacks>();
			refPack2.RP_CustomsCountry = Core.Constants.CountryCodes.UnitedStates;
			refPack2.RP_CommercialPack = Core.Constants.PkgUnit.Sheet;
			refPack2.RP_CustomsPack = ABIUnitOfMeasureList.Codes.Number;
			refPack2.RP_ConversionFactor = 1;

			var refPack3 = newFactory.New<CusRefPacks>();
			refPack3.RP_CustomsCountry = Core.Constants.CountryCodes.UnitedStates;
			refPack3.RP_CommercialPack = Core.Constants.PkgUnit.Sheet;
			refPack3.RP_CustomsPack = ABIUnitOfMeasureList.Codes.Square;
			refPack3.RP_ConversionFactor = 1;
			refPack3.RP_OH_Supplier = supplier.PK;

			var refPack4 = newFactory.New<CusRefPacks>();
			refPack4.RP_CustomsCountry = Core.Constants.CountryCodes.UnitedStates;
			refPack4.RP_CommercialPack = Core.Constants.Length.Yards;
			refPack4.RP_CustomsPack = AESUnitOfMeasureList.Codes.SilverContentInGrams;
			refPack4.RP_ConversionFactor = 1;
			AssertEquals("ABI list should not have " + AESUnitOfMeasureList.Codes.SilverContentInGrams, false, abiList.ContainsCode(AESUnitOfMeasureList.Codes.SilverContentInGrams));
			AssertEquals("ABI list should have " + Core.Constants.Length.Yards, true, abiList.ContainsCode(Core.Constants.Length.Yards));

			var refPack5 = newFactory.New<CusRefPacks>();
			refPack5.RP_CustomsCountry = Core.Constants.CountryCodes.UnitedStates;
			refPack5.RP_CommercialPack = Core.Constants.Length.Miles;
			refPack5.RP_CustomsPack = AESUnitOfMeasureList.Codes.RunningBales;
			refPack5.RP_ConversionFactor = 1;
			AssertEquals("ABI list should not have " + AESUnitOfMeasureList.Codes.RunningBales, false, abiList.ContainsCode(AESUnitOfMeasureList.Codes.RunningBales));
			AssertEquals("ABI list should not have " + Core.Constants.Length.Miles, false, abiList.ContainsCode(Core.Constants.Length.Miles));
			product.OP_StockKeepingUnit = refPack1.RP_CommercialPack;
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "1010101010";
			newFactory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableAII = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = product.OP_PartNum;
			AssertEquals(AESUnitOfMeasureList.Codes.Pieces, invoiceLine.JI_InvoiceUQ);

			product.OP_StockKeepingUnit = Core.Constants.PkgUnit.Reel;
			newFactory.Save();

			invoiceLine.JI_PartNo = ZString.Empty;
			invoiceLine.JI_InvoiceUQ = ZString.Empty;
			invoiceLine.JI_PartNo = product.OP_PartNum;
			AssertEquals(AESUnitOfMeasureList.Codes.Pieces, invoiceLine.JI_InvoiceUQ);

			product.OP_StockKeepingUnit = Core.Constants.PkgUnit.Sheet;
			newFactory.Save();

			invoiceLine.JI_PartNo = ZString.Empty;
			invoiceLine.JI_InvoiceUQ = ZString.Empty;
			invoiceLine.JI_PartNo = product.OP_PartNum;
			AssertEquals("The supplier specific should be use", ABIUnitOfMeasureList.Codes.Square, invoiceLine.JI_InvoiceUQ);

			product.OP_StockKeepingUnit = Core.Constants.Length.Yards;
			newFactory.Save();

			invoiceLine.JI_PartNo = ZString.Empty;
			invoiceLine.JI_InvoiceUQ = ZString.Empty;
			invoiceLine.JI_PartNo = product.OP_PartNum;
			AssertEquals(Core.Constants.Length.Yards + " should be use as it's valid for ABI List", Core.Constants.Length.Yards, invoiceLine.JI_InvoiceUQ);

			product.OP_StockKeepingUnit = Core.Constants.Length.Miles;
			newFactory.Save();

			invoiceLine.JI_PartNo = ZString.Empty;
			invoiceLine.JI_InvoiceUQ = ZString.Empty;
			invoiceLine.JI_PartNo = product.OP_PartNum;
			AssertEquals("The first mapping should be use as " + Core.Constants.Length.Miles + " is not valid for ABI List", AESUnitOfMeasureList.Codes.RunningBales, invoiceLine.JI_InvoiceUQ);
		}

		public void TestUnitFromProductIsConverted_CIPType()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			var abiList = new ABIUnitOfMeasureList();
			var supplier = newFactory.New<OrgHeader>();
			supplier.OH_Code = "SUPZZ!123";
			var importer = newFactory.New<OrgHeader>();
			importer.OH_Code = "IMPZZ!123";

			var product = newFactory.New<OrgSupplierPart>();
			product.OP_PartNum = "ProductForTesting";

			OrgPartRelation relation1 = product.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, OrgPartRelation.RelationshipTypes.Supplier);
			relation1.OU_LocalPartNumber = "XXX";
			var refPack1 = newFactory.New<CusRefPacks>();
			refPack1.RP_CustomsCountry = Core.Constants.CountryCodes.UnitedStates;
			refPack1.RP_CommercialPack = Core.Constants.PkgUnit.Reel;
			refPack1.RP_CustomsPack = ABIUnitOfMeasureList.Codes.Pieces;
			refPack1.RP_ConversionFactor = 1;
			refPack1.RP_Type = RPTypeList.Codes.CommercialInvoice;

			var refPack2 = newFactory.New<CusRefPacks>();
			refPack2.RP_CustomsCountry = Core.Constants.CountryCodes.UnitedStates;
			refPack2.RP_CommercialPack = Core.Constants.PkgUnit.Sheet;
			refPack2.RP_CustomsPack = ABIUnitOfMeasureList.Codes.Number;
			refPack2.RP_ConversionFactor = 1;
			refPack2.RP_Type = RPTypeList.Codes.CommercialInvoice;

			var refPack3 = newFactory.New<CusRefPacks>();
			refPack3.RP_CustomsCountry = Core.Constants.CountryCodes.UnitedStates;
			refPack3.RP_CommercialPack = Core.Constants.PkgUnit.Sheet;
			refPack3.RP_CustomsPack = ABIUnitOfMeasureList.Codes.Square;
			refPack3.RP_ConversionFactor = 1;
			refPack3.RP_OH_Supplier = supplier.PK;
			refPack3.RP_Type = RPTypeList.Codes.CommercialInvoice;

			var refPack4 = newFactory.New<CusRefPacks>();
			refPack4.RP_CustomsCountry = Core.Constants.CountryCodes.UnitedStates;
			refPack4.RP_CommercialPack = Core.Constants.Length.Yards;
			refPack4.RP_CustomsPack = AESUnitOfMeasureList.Codes.SilverContentInGrams;
			refPack4.RP_ConversionFactor = 1;
			refPack4.RP_Type = RPTypeList.Codes.CommercialInvoice;
			AssertEquals("ABI list should not have " + AESUnitOfMeasureList.Codes.SilverContentInGrams, false, abiList.ContainsCode(AESUnitOfMeasureList.Codes.SilverContentInGrams));
			AssertEquals("ABI list should have " + Core.Constants.Length.Yards, true, abiList.ContainsCode(Core.Constants.Length.Yards));

			var refPack5 = newFactory.New<CusRefPacks>();
			refPack5.RP_CustomsCountry = Core.Constants.CountryCodes.UnitedStates;
			refPack5.RP_CommercialPack = Core.Constants.Length.Miles;
			refPack5.RP_CustomsPack = AESUnitOfMeasureList.Codes.RunningBales;
			refPack5.RP_ConversionFactor = 1;
			AssertEquals("ABI list should not have " + AESUnitOfMeasureList.Codes.RunningBales, false, abiList.ContainsCode(AESUnitOfMeasureList.Codes.RunningBales));
			AssertEquals("ABI list should not have " + Core.Constants.Length.Miles, false, abiList.ContainsCode(Core.Constants.Length.Miles));
			product.OP_StockKeepingUnit = refPack1.RP_CommercialPack;
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "1010101010";
			newFactory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableAII = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = product.OP_PartNum;
			AssertEquals(AESUnitOfMeasureList.Codes.Pieces, invoiceLine.JI_InvoiceUQ);

			product.OP_StockKeepingUnit = Core.Constants.PkgUnit.Reel;
			newFactory.Save();

			invoiceLine.JI_PartNo = ZString.Empty;
			invoiceLine.JI_InvoiceUQ = ZString.Empty;
			invoiceLine.JI_PartNo = product.OP_PartNum;
			AssertEquals(AESUnitOfMeasureList.Codes.Pieces, invoiceLine.JI_InvoiceUQ);

			product.OP_StockKeepingUnit = Core.Constants.PkgUnit.Sheet;
			newFactory.Save();

			invoiceLine.JI_PartNo = ZString.Empty;
			invoiceLine.JI_InvoiceUQ = ZString.Empty;
			invoiceLine.JI_PartNo = product.OP_PartNum;
			AssertEquals("The supplier specific should be use", ABIUnitOfMeasureList.Codes.Square, invoiceLine.JI_InvoiceUQ);

			product.OP_StockKeepingUnit = Core.Constants.Length.Yards;
			newFactory.Save();

			invoiceLine.JI_PartNo = ZString.Empty;
			invoiceLine.JI_InvoiceUQ = ZString.Empty;
			invoiceLine.JI_PartNo = product.OP_PartNum;
			AssertEquals(Core.Constants.Length.Yards + " should be use as it's valid for ABI List", Core.Constants.Length.Yards, invoiceLine.JI_InvoiceUQ);

			product.OP_StockKeepingUnit = Core.Constants.Length.Miles;
			newFactory.Save();

			invoiceLine.JI_PartNo = ZString.Empty;
			invoiceLine.JI_InvoiceUQ = ZString.Empty;
			invoiceLine.JI_PartNo = product.OP_PartNum;
			AssertEquals("The first mapping should be use as " + Core.Constants.Length.Miles + " is not valid for ABI List", AESUnitOfMeasureList.Codes.RunningBales, invoiceLine.JI_InvoiceUQ);
		}

		public void TestDefaultArticleNumberAAndB()
		{
			JobDeclaration declaration = GetDeclarationWithOrgSupplierPart();
			JobComInvoiceLine invoiceLine = declaration.Invoices[0].JobComInvoiceLines[0];

			invoiceLine.JI_PartNo = product.OP_PartNum;
			AssertNotNull(invoiceLine.Part);

			AssertEquals("ArticleA is defaulted", "XXX", invoiceLine.US_ArticleNoA);
			AssertEquals("ArticleB", product.OP_PartNum, invoiceLine.US_ArticleNoB);

			invoiceLine.US_ArticleNoA = ZString.Empty;
			invoiceLine.US_ArticleNoB = ZString.Empty;

			OrgPartRelation relation2 = product.RelatedOrganisations.AddNew();
			relation2.OU_LocalPartNumber = "YYY";
			relation2.OU_OH = declaration.JE_OH_Importer;
			relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			invoiceLine.JI_PartNo = "";
			invoiceLine.JI_PartNo = product.OP_PartNum;

			AssertEquals("ArticleA is defaulted", "XXX", invoiceLine.US_ArticleNoA);
			AssertEquals("ArticleB is defaulted", "YYY", invoiceLine.US_ArticleNoB);
		}

		public void TestJI_CustomsQuantityInfoReadonly()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(true, invoiceLine.JI_CustomsQuantityInfo.ReadOnly);

			invoiceLine.JI_CustomsUnitQty = "KG";
			AssertEquals(false, invoiceLine.JI_CustomsQuantityInfo.ReadOnly);

			invoiceLine.JI_CustomsUnitQty = ABIUnitOfMeasureList.Codes.NoUnitRequired;
			AssertEquals(true, invoiceLine.JI_CustomsQuantityInfo.ReadOnly);
		}

		public void TestIsUSReturnedGoodsTransaction()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(false, invoiceLine.IsUSReturnedGoodsTransaction);

			invoiceLine.US_SupTariff = "9802";
			AssertEquals(true, invoiceLine.IsUSReturnedGoodsTransaction);

			invoiceLine.US_SupTariff = "9822.05";
			AssertEquals(true, invoiceLine.IsUSReturnedGoodsTransaction);

			invoiceLine.JI_Tariff = "9801";
			AssertEquals(true, invoiceLine.IsUSReturnedGoodsTransaction);

			var secondaryLine = invoice.InvoiceLines.AddNew();
			secondaryLine.US_SupTariff = "9802.00.6000";
			Assert(secondaryLine.IsUSReturnedGoodsTransaction);
			secondaryLine.JI_ParentID = invoiceLine.PK;
			Assert(secondaryLine.IsUSReturnedGoodsTransaction);
		}

		public void TestIsValidForAII()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableAII = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.US_PaymentTerms = PaymentTermsTypeList.Codes.Basic;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 3m;
			invoiceLine.US_SecondarySPI = ZString.Empty;
			AssertEquals(true, invoiceLine.IsValidForAII);

			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			AssertEquals(true, invoiceLine.IsValidForAII);

			invoiceLine.US_SecondarySPI = ZString.Empty;
			AssertEquals(true, invoiceLine.IsValidForAII);

			JobComInvoiceLine invoiceLine2 = invoiceLine.AddSecondaryInvoiceLine();
			AssertEquals(true, invoiceLine.IsValidForAII);
			AssertEquals(true, invoiceLine2.IsValidForAII);

			invoiceLine.JI_Tariff = "10";
			AssertEquals(true, invoiceLine.IsValidForAII);
			AssertEquals(true, invoiceLine2.IsValidForAII);

			invoiceLine.US_IsExcludedFromAII = true;
			AssertEquals(false, invoiceLine.IsValidForAII);
			AssertEquals(true, invoiceLine2.IsValidForAII);

			invoiceLine2.US_IsExcludedFromAII = true;
			AssertEquals(false, invoiceLine.IsValidForAII);
			AssertEquals(false, invoiceLine2.IsValidForAII);
		}

		public void TestVisaNumberTextileCategoryNumber_Effective()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_VisaNo = "123";

			JobComInvoiceLine secondaryLine = invoiceLine.AddSecondaryInvoiceLine();
			secondaryLine.US_TextileCategoryNo = "456";

			AssertEquals("Effective visa number", "123", invoiceLine.US_VisaNo_Effective);
			AssertEquals("Effective visa number", "123", secondaryLine.US_VisaNo_Effective);

			AssertEquals("Effective textile category number", "456", invoiceLine.US_TextileCategoryNo_Effective);
			AssertEquals("Effective textile category number", "456", secondaryLine.US_TextileCategoryNo_Effective);
		}

		public void TestDefaultTextileCategoryNumber()
		{
			USCTariff tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "1111111111";
			tariff1.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff1.UE_DateTo = ZDateTime.Today;

			USCTariff tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "222222222";
			tariff2.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff2.UE_DateTo = ZDateTime.Today;
			tariff2.UE_TextileCategoryNumber = "231";

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_TextileCategoryNo = "345";

			invoiceLine.JI_Tariff = tariff1.UE_Tariff;
			AssertEquals("", invoiceLine.US_TextileCategoryNo);

			invoiceLine.JI_Tariff = tariff2.UE_Tariff;
			AssertEquals("231", invoiceLine.US_TextileCategoryNo);

			invoiceLine.US_SupTariff = "9802005060";
			AssertEquals("Visa category # should not be sent with 9802 tariff - do not default value", "", invoiceLine.US_TextileCategoryNo);
		}

		public void TestDoNotDefaultTextileCategoryNumberWhenNotRequired()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "222222222";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;
			tariff.UE_TextileCategoryNumber = "231";

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9802008044";
			AssertEquals("PreCondition", true, invoiceLine.ImportSupTariff.NoCategoryNumberToBeEntered(invoiceLine.EffectiveDateForDutyRate));

			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			AssertEquals("No category number should have been defaulted", ZString.Empty, invoiceLine.US_TextileCategoryNo);
		}

		public void TestDefaultTextileCategoryNumberForHaitiHopeRule()
		{
			USCTariff tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "1111111111";
			tariff1.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff1.UE_DateTo = ZDateTime.Today;

			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "222222222";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;

			USCTariff cBTPATextileBenefits = Factory.New<USCTariff>();
			cBTPATextileBenefits.UE_Tariff = "2222233333";
			cBTPATextileBenefits.UE_DateFrom = ZDateTime.BrettsBirthday;
			cBTPATextileBenefits.UE_DateTo = ZDateTime.Today.AddYears(1);

			USCTariffRule tariffRuleCBTPA = Factory.New<USCTariffRule>();
			tariffRuleCBTPA.U1_RuleCode = TariffRuleList.Codes.EligibleForCBTPATextileClaims;
			tariffRuleCBTPA.U1_DateFrom = ZDateTime.BrettsBirthday;
			tariffRuleCBTPA.U1_Tariff = "2222233333";

			USCTariff tariff3 = Factory.New<USCTariff>();
			tariff3.UE_Tariff = "3333333333";
			tariff3.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff3.UE_DateTo = ZDateTime.Today;
			tariff3.UE_TextileCategoryNumber = "231";

			USCTariffRule tariffRule = Factory.New<USCTariffRule>();
			tariffRule.U1_RuleCode = TariffRuleList.Codes.HaitiTariffHope;
			tariffRule.U1_DateFrom = ZDateTime.BrettsBirthday;
			tariffRule.U1_Tariff = "1111111111";

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3333333333";
			AssertEquals("TextileCategoryNumber defaulted", "231", invoiceLine.US_TextileCategoryNo);

			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_SetInd = "V";
			invoiceLine.US_SupTariff = "1111111111";
			invoiceLine.JI_Tariff = "222222222";
			AssertEquals("Standard number for HAITI is defaulted", "900", invoiceLine.US_TextileCategoryNo);
			invoiceLine.US_SetInd = "X";
			AssertEquals("Setting X clears the field", ZString.Empty, invoiceLine.US_TextileCategoryNo);

			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_SetInd = "X";
			invoiceLine.US_SupTariff = "1111111111";
			invoiceLine.JI_Tariff = "222222222";
			AssertEquals("No defaulting for X line", ZString.Empty, invoiceLine.US_TextileCategoryNo);

			invoiceLine.JI_Tariff = "2222233333";
			AssertEquals("Tariff IsEligibleForCBTPATextileBenefits - no Textile Category No required", ZString.Empty, invoiceLine.US_TextileCategoryNo);
		}

		public void TestUpdateDetailsOnPartChange_FDA()
		{
			var product = GetProductForFDAAndPGATests();
			var pivot = product.PivotsForBinding[0];

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_OH_Importer = importer.PK;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Supplier = supplier.PK;
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = product.OP_PartNum;
			AssertNotNull(invoiceLine.Part);

			AssertEquals(0, invoiceLine.FDAs.Count);
		}

		public void TestUpdateDetailsWithCensusWarningOverrides()
		{
			var product = GetProductForFDAAndPGATests();

			var cwo = product.PivotsForBinding[0].CensusWarningOverrides.AddNew();
			cwo.CY_Code = CensusWarningCodeList.Codes.ChargesDividedByValue;
			cwo.CY_Data = CensusOverrideCodeList.Codes._01;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.JE_OH_Importer = importer.PK;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Supplier = supplier.PK;
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = product.OP_PartNum;
			AssertNotNull(invoiceLine.Part);

			AssertEquals("census warning is copied", 1, invoiceLine.CensusWarningOverrides.Count);
		}

		public void TestUpdateDetailsOnPartChange_ADDCVDAndCountryOfExp()
		{
			var product = GetProductForFDAAndPGATests();
			var pivot = product.PivotsForBinding[0];

			pivot.CD_UC_NKCountryOfExport = Core.Constants.CountryCodes.Antarctica;
			pivot.CD_ADDCaseNo = "A124567";
			pivot.CD_ADDApplicable = true;
			pivot.CD_ADDDepositRateInd = DepositRateIndicatorList.Codes.AdValorem;
			pivot.CD_ADDBonded = false;
			pivot.CD_CVDCaseNo = "C456789456";
			pivot.CD_CVDApplicable = false;
			pivot.CD_CVDDepositRateInd = DepositRateIndicatorList.Codes.Specific;
			pivot.CD_CVDBonded = true;

			var childPivot1 = pivot.Children.AddNew();
			childPivot1.CI_ChildType = ClassificationChildTypeList.Codes.Related;
			childPivot1.CD_UC_NKCountryOfExport = Core.Constants.CountryCodes.Aruba;
			childPivot1.CD_ADDApplicable = true;
			childPivot1.CD_ADDDepositRateInd = DepositRateIndicatorList.Codes.Specific;
			childPivot1.CD_ADDBonded = true;
			childPivot1.CD_CVDApplicable = true;
			childPivot1.CD_CVDDepositRateInd = DepositRateIndicatorList.Codes.Specific;
			childPivot1.CD_CVDBonded = true;

			var childPivot2 = pivot.Children.AddNew();
			childPivot2.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			childPivot2.CD_UC_NKCountryOfExport = Core.Constants.CountryCodes.CapeVerde;
			childPivot2.CD_ADDDepositRateInd = DepositRateIndicatorList.Codes.AdValorem;
			childPivot2.CD_CVDDepositRateInd = DepositRateIndicatorList.Codes.Specific;

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_OH_Importer = importer.PK;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Supplier = supplier.PK;
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			AssertEquals("Precondition: no Invoice Lines for this invoice header", 0, invoice.JobComInvoiceLines.Count);

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = product.OP_PartNum;
			AssertNotNull(invoiceLine.Part);
			AssertEquals("Should be 3 Invoice Lines, when Product selected", 3, invoice.JobComInvoiceLines.Count);

			AssertEquals("Country of Export", Core.Constants.CountryCodes.Antarctica, invoiceLine.US_UC_NKCountryOfExport);
			AssertEquals("US_ADDCaseNo", "A124567", invoiceLine.US_ADDCaseNo);
			AssertEquals("US_ADD_NA", true, invoiceLine.US_ADD_NA);
			AssertEquals("US_ADDDepositRateIndicator", DepositRateIndicatorList.Codes.AdValorem, invoiceLine.US_ADDDepositRateIndicator);
			AssertEquals("US_IsBondedADD", false, invoiceLine.US_IsBondedADD);
			AssertEquals("US_CVDCaseNo", "C456789456", invoiceLine.US_CVDCaseNo);
			AssertEquals("US_CVD_NA", false, invoiceLine.US_CVD_NA);
			AssertEquals("US_CVDDepositRateIndicator", DepositRateIndicatorList.Codes.Specific, invoiceLine.US_CVDDepositRateIndicator);
			AssertEquals("US_IsBondedCVD", true, invoiceLine.US_IsBondedCVD);

			var secondInvoiceLine = invoice.JobComInvoiceLines[1];
			AssertEquals("Country of Export", Core.Constants.CountryCodes.Aruba, secondInvoiceLine.US_UC_NKCountryOfExport);
			AssertEquals("US_ADD_NA", true, secondInvoiceLine.US_ADD_NA);
			AssertEquals("US_ADDDepositRateIndicator", DepositRateIndicatorList.Codes.Specific, secondInvoiceLine.US_ADDDepositRateIndicator);
			AssertEquals("US_IsBondedADD", true, secondInvoiceLine.US_IsBondedADD);
			AssertEquals("US_CVD_NA", true, secondInvoiceLine.US_CVD_NA);
			AssertEquals("US_CVDDepositRateIndicator", DepositRateIndicatorList.Codes.Specific, secondInvoiceLine.US_CVDDepositRateIndicator);
			AssertEquals("US_IsBondedCVD", true, secondInvoiceLine.US_IsBondedCVD);

			var lastInvoiceLine = invoice.JobComInvoiceLines[2];
			AssertEquals("Country of Export from Parent Line", Core.Constants.CountryCodes.CapeVerde, lastInvoiceLine.US_UC_NKCountryOfExport);
			AssertEquals("US_ADDDepositRateIndicator", DepositRateIndicatorList.Codes.AdValorem, lastInvoiceLine.US_ADDDepositRateIndicator);
			AssertEquals("US_IsBondedADD", false, lastInvoiceLine.US_IsBondedADD);
			AssertEquals("US_CVD_NA", false, lastInvoiceLine.US_CVD_NA);
			AssertEquals("US_CVDDepositRateIndicator", DepositRateIndicatorList.Codes.Specific, lastInvoiceLine.US_CVDDepositRateIndicator);
			AssertEquals("US_IsBondedCVD", false, lastInvoiceLine.US_IsBondedCVD);
		}

		public void TestJI_PartNoDefaultUS_FlavorContentCreditInd()
		{
			var declarationSupplier = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignor, true));
			var declarationImporter = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignee, true));
			declarationImporter.OH_RL_NKClosestPort = "USLAX";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = declarationSupplier.PK;
			declaration.JE_OH_Importer = declarationImporter.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();

			var invoiceSupplier = Factory.NewWithValidTestData<OrgHeader>();
			var invoiceImporter = Factory.NewWithValidTestData<OrgHeader>();

			invoice.JZ_OH_Supplier = invoiceSupplier.PK;
			invoice.JZ_OH_Buyer = invoiceImporter.PK;

			AssertEquals("Precondition: Invoice Header should have his own set of Supplier/Importer",
				invoiceSupplier, invoice.Supplier);
			AssertEquals("Precondition: Invoice Header should have his own set of Supplier/Importer",
				invoiceImporter, invoice.Importer);

			var product1 = Factory.New<OrgSupplierPart>();
			product1.OP_PartNum = "Test Product";
			product1.RelatedOrganisations.AddOwner(invoiceImporter);
			product1.RelatedOrganisations.AddSupplier(invoiceSupplier);

			var pivot1 = product1.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot1.CI_TariffNum = "10000000";
			Factory.Save();

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(false, invoiceLine.US_FlavorContentCreditInd);

			invoiceLine.JI_PartNo = product1.OP_PartNum;
			AssertEquals(false, invoiceLine.US_FlavorContentCreditInd);

			invoiceLine.JI_PartNo = ZString.Empty;
			AssertEquals(false, invoiceLine.US_FlavorContentCreditInd);

			pivot1.CD_FlavorContentCreditIndicator = true;
			invoiceLine.JI_PartNo = product1.OP_PartNum;
			AssertEquals(true, invoiceLine.US_FlavorContentCreditInd);
		}

		public void TestJI_PartNo_DeletesRelatedLinesWhenNewPartIsSelected()
		{
			JobDeclaration declaration = GetDeclarationWithOrgSupplierPart();
			JobComInvoiceLine invoiceLine = declaration.Invoices[0].JobComInvoiceLines[0];
			invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine.AddProductRelatedInvoiceLine();

			invoiceLine.JI_PartNo = product.OP_PartNum;
			AssertNotNull(invoiceLine.Part);
			AssertEquals(0, invoiceLine.ChildLines.Count());
			AssertEquals(0, invoiceLine.ProductRelatedLines.Count());
		}

		public void TestJI_PartNo_AdditionalReconciliationDefaults()
		{
			var newFactory = new BusinessObjectFactory();
			var importer = newFactory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignee, true));
			var part = newFactory.New<OrgSupplierPart>();
			part.OP_PartNum = "Z!Z2Z";
			var orgRel = part.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			var pivot1 = part.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot1.CI_TariffNum = "10000000";
			pivot1.CI_OH = orgRel.OU_OH;
			pivot1.CD_ReconIssue = ReconIssueCodeList.Codes.ValueClass9802Recon;
			pivot1.CD_NAFTARecon = true;

			var classification = newFactory.New<CusClassification>();
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			classification.CC_LookupCode = "Z1Z!";
			classification.CC_TariffNum = "30000000";
			pivot1.CI_CC = classification.PK;

			var supplier = newFactory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignor, true));
			var link = importer.SupplierLinks.AddNew(supplier);
			link.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.UnitedStates;
			var addInfo = link.GetAddInfo();
			addInfo.ZO_FirstSale = YesNoList.Codes.Yes;
			addInfo.ZO_OtherReconIndicator = ReconIssueCodeList.Codes.ValueClass9802Recon;
			newFactory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_RL_NKFinalDestination = "USCHI";
			Factory.Save();
			AssertEquals("Precondition: Other Recon Indicator should be from Supplier/Buyer relation",
						ReconIssueCodeList.Codes.ValueClass9802Recon, declaration.US_OtherReconIndicator);

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = part.OP_PartNum;

			AssertEquals("Other Recon Indicator not in the conflict and should be the same",
						ReconIssueCodeList.Codes.ValueClass9802Recon, declaration.US_OtherReconIndicator);

			pivot1.CD_ReconIssue = ReconIssueCodeList.Codes._9802Recon;
			newFactory.Save();

			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = part.OP_PartNum;
			AssertEquals("Other Recon Indicator from declaration is more specific that from Product",
					ReconIssueCodeList.Codes.ValueClass9802Recon, declaration.US_OtherReconIndicator);

			declaration.US_OtherReconIndicator = ReconIssueCodeList.Codes.ClassRecon;
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = part.OP_PartNum;
			Factory.Save();

			AssertEquals("Other Recon Indicator should be AL, because Supplier Link has 'AL'",
				ReconIssueCodeList.Codes.ValueClass9802Recon, declaration.US_OtherReconIndicator);

			addInfo.ZO_OtherReconIndicator = ReconIssueCodeList.Codes.ClassRecon;
			pivot1.CD_ReconIssue = ReconIssueCodeList.Codes.ValueRecon;
			newFactory.Save();

			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = part.OP_PartNum;
			Factory.Save();

			AssertEquals("Other Recon Indicator should be complementary",
				ReconIssueCodeList.Codes.ValueClassRecon, declaration.US_OtherReconIndicator);

			pivot1.CD_ReconIssue = ReconIssueCodeList.Codes.Class9802Recon;
			newFactory.Save();

			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = part.OP_PartNum;
			Factory.Save();

			AssertEquals("Other Recon Indicator calculated from AddInfo 'CL' and Pivot 'C9'. Should be C9.",
				ReconIssueCodeList.Codes.Class9802Recon, declaration.US_OtherReconIndicator);
		}

		public void TestJI_PartNo_ReadOnly()
		{
			JobDeclaration declaration = GetDeclarationWithOrgSupplierPart();
			JobComInvoiceLine invoiceLine = declaration.Invoices[0].JobComInvoiceLines[0];
			JobComInvoiceLine childInvoiceLine = invoiceLine.AddSecondaryInvoiceLine();
			JobComInvoiceLine relatedInvoiceLine = invoiceLine.AddProductRelatedInvoiceLine();

			AssertEquals(false, invoiceLine.JI_PartNoInfo.ReadOnly);
			AssertEquals(true, childInvoiceLine.JI_PartNoInfo.ReadOnly);
			AssertEquals(true, relatedInvoiceLine.JI_PartNoInfo.ReadOnly);
		}

		public void TestJI_PartAttrib1_ReadOnly()
		{
			JobDeclaration declaration = GetDeclarationWithOrgSupplierPart();
			JobComInvoiceLine invoiceLine = declaration.Invoices[0].JobComInvoiceLines[0];
			JobComInvoiceLine childInvoiceLine = invoiceLine.AddSecondaryInvoiceLine();
			JobComInvoiceLine relatedInvoiceLine = invoiceLine.AddProductRelatedInvoiceLine();
			AssertEquals(false, invoiceLine.JI_PartAttrib1Info.ReadOnly);
			AssertEquals(true, childInvoiceLine.JI_PartAttrib1Info.ReadOnly);
			AssertEquals(true, relatedInvoiceLine.JI_PartAttrib1Info.ReadOnly);

			AssertEquals(ZString.Empty, invoiceLine.JI_PartAttrib1);
			AssertEquals(ZString.Empty, childInvoiceLine.JI_PartAttrib1);
			AssertEquals(ZString.Empty, relatedInvoiceLine.JI_PartAttrib1);

			invoiceLine.JI_PartAttrib1 = "~1";
			AssertEquals("~1", invoiceLine.JI_PartAttrib1);
			AssertEquals("~1", childInvoiceLine.JI_PartAttrib1);
			AssertEquals("~1", relatedInvoiceLine.JI_PartAttrib1);
		}

		public void TestJI_PartAttrib2_ReadOnly()
		{
			JobDeclaration declaration = GetDeclarationWithOrgSupplierPart();
			JobComInvoiceLine invoiceLine = declaration.Invoices[0].JobComInvoiceLines[0];
			JobComInvoiceLine childInvoiceLine = invoiceLine.AddSecondaryInvoiceLine();
			JobComInvoiceLine relatedInvoiceLine = invoiceLine.AddProductRelatedInvoiceLine();

			AssertEquals(false, invoiceLine.JI_PartAttrib2Info.ReadOnly);
			AssertEquals(true, childInvoiceLine.JI_PartAttrib2Info.ReadOnly);
			AssertEquals(true, relatedInvoiceLine.JI_PartAttrib2Info.ReadOnly);

			AssertEquals(ZString.Empty, invoiceLine.JI_PartAttrib2);
			AssertEquals(ZString.Empty, childInvoiceLine.JI_PartAttrib2);
			AssertEquals(ZString.Empty, relatedInvoiceLine.JI_PartAttrib2);

			invoiceLine.JI_PartAttrib2 = "~2";
			AssertEquals("~2", invoiceLine.JI_PartAttrib2);
			AssertEquals("~2", childInvoiceLine.JI_PartAttrib2);
			AssertEquals("~2", relatedInvoiceLine.JI_PartAttrib2);
		}

		public void TestJI_PartAttrib3_ReadOnly()
		{
			JobDeclaration declaration = GetDeclarationWithOrgSupplierPart();
			JobComInvoiceLine invoiceLine = declaration.Invoices[0].JobComInvoiceLines[0];
			JobComInvoiceLine childInvoiceLine = invoiceLine.AddSecondaryInvoiceLine();
			JobComInvoiceLine relatedInvoiceLine = invoiceLine.AddProductRelatedInvoiceLine();

			AssertEquals(false, invoiceLine.JI_PartAttrib3Info.ReadOnly);
			AssertEquals(true, childInvoiceLine.JI_PartAttrib3Info.ReadOnly);
			AssertEquals(true, relatedInvoiceLine.JI_PartAttrib3Info.ReadOnly);

			AssertEquals(ZString.Empty, invoiceLine.JI_PartAttrib3);
			AssertEquals(ZString.Empty, childInvoiceLine.JI_PartAttrib3);
			AssertEquals(ZString.Empty, relatedInvoiceLine.JI_PartAttrib3);

			invoiceLine.JI_PartAttrib3 = "~3";
			AssertEquals("~3", invoiceLine.JI_PartAttrib3);
			AssertEquals("~3", childInvoiceLine.JI_PartAttrib3);
			AssertEquals("~3", relatedInvoiceLine.JI_PartAttrib3);
		}

		public void TestJI_CC_ReadOnly()
		{
			JobDeclaration declaration = GetDeclarationWithOrgSupplierPart();
			JobComInvoiceLine invoiceLine = declaration.Invoices[0].JobComInvoiceLines[0];
			JobComInvoiceLine childInvoiceLine = invoiceLine.AddSecondaryInvoiceLine();
			JobComInvoiceLine relatedInvoiceLine = invoiceLine.AddProductRelatedInvoiceLine();

			AssertEquals(false, invoiceLine.JI_CCInfo.ReadOnly);
			AssertEquals(true, childInvoiceLine.JI_CCInfo.ReadOnly);
			AssertEquals(true, relatedInvoiceLine.JI_CCInfo.ReadOnly);
		}

		public void TestJI_ParentID_ReadOnly()
		{
			JobDeclaration declaration = GetDeclarationWithOrgSupplierPart();
			JobComInvoiceLine invoiceLine = declaration.Invoices[0].JobComInvoiceLines[0];
			JobComInvoiceLine relatedInvoiceLine = invoiceLine.AddProductRelatedInvoiceLine();

			AssertEquals(false, invoiceLine.JI_ParentIDInfo.ReadOnly);
			AssertEquals(false, relatedInvoiceLine.JI_ParentIDInfo.ReadOnly);

			invoiceLine.JI_PartNo = "TEST";

			AssertEquals(true, invoiceLine.JI_ParentIDInfo.ReadOnly);
			AssertEquals(true, relatedInvoiceLine.JI_ParentIDInfo.ReadOnly);
		}

		public void TestJI_PartNo_CanBeSetByCustomer()
		{
			JobDeclaration declaration = GetDeclarationWithOrgSupplierPart();
			JobComInvoiceLine invoiceLine = declaration.Invoices[0].JobComInvoiceLines[0];
			JobComInvoiceLine childInvoiceLine = invoiceLine.AddSecondaryInvoiceLine();
			JobComInvoiceLine relatedInvoiceLine = invoiceLine.AddProductRelatedInvoiceLine();

			AssertEquals(true, invoiceLine.JI_PartNo_CanBeSetByCustomer);
			AssertEquals(false, childInvoiceLine.JI_PartNo_CanBeSetByCustomer);
			AssertEquals(false, relatedInvoiceLine.JI_PartNo_CanBeSetByCustomer);
		}

		public void TestUpdateDetailsOnPartChange_9802_9801()
		{
			JobDeclaration declaration = GetImportDeclarationWithBuyerSupplier();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			OrgSupplierPart product = GetPartWithRelationship(declaration.Importer);

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.AIILines.AddNew();
			invoiceLine.JI_Tariff = "1";
			invoiceLine.US_SupTariff = "2";
			invoiceLine.US_9802PerUnit = 4;
			invoiceLine.FirstAIILine.US_UnitPrice = 7m;

			CusClassPartPivot pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "6203434030";
			pivot.CI_SupplementalTariff = "9802008068";
			pivot.CD_9802USDValuePerUnit = 10;
			pivot.CD_PerUnitCost = 3;
			pivot.CD_RX_NKPerUnitCostCurr = invoice.JZ_RX_NKInvoice_Currency;

			CusClassPartPivot relatedPivot = pivot.Children.AddNew();
			relatedPivot.CI_ChildType = ClassificationChildTypeList.Codes.Related;
			relatedPivot.CI_TariffNum = "9801001010";
			relatedPivot.CD_PerUnitCost = 2;
			relatedPivot.CD_RX_NKPerUnitCostCurr = invoice.JZ_RX_NKInvoice_Currency;
			Factory.Save();//gets the addinfo fields populated

			invoiceLine.JI_PartNo = product.OP_PartNum;
			AssertEquals(pivot.CI_TariffNum, invoiceLine.JI_Tariff);
			AssertEquals(pivot.CI_SupplementalTariff, invoiceLine.US_SupTariff);
			AssertEquals(pivot.CD_9802USDValuePerUnit, invoiceLine.US_9802PerUnit);
			AssertEquals(pivot.CD_PerUnitCost, invoiceLine.FirstAIILine.US_UnitPrice);

			AssertEquals(1, invoiceLine.ProductRelatedLines.Count());
			AssertEquals(relatedPivot.CI_TariffNum, invoiceLine.ProductRelatedLines.ElementAt(0).JI_Tariff);
			AssertEquals(relatedPivot.CD_PerUnitCost, invoiceLine.ProductRelatedLines.ElementAt(0).FirstAIILine.US_UnitPrice);
		}

		public void TestManifestUQ()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;

			declaration.JE_TotalNoOfPacksPackType = "AA";
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			AssertEquals("AA", invoiceLine.US_ManifestUQ);
		}

		public void TestUpdateDetailsOnPartChange_ComponentTariff()
		{
			JobDeclaration declaration = GetImportDeclarationWithBuyerSupplier();
			OrgSupplierPart product = GetPartWithRelationship(declaration.Importer);

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			CusClassPartPivot pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "6203434030";

			CusClassPartPivot relatedPivot = pivot.Children.AddNew();
			relatedPivot.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			relatedPivot.CI_TariffNum = "6203434040";
			Factory.Save();//gets the addinfo fields populated

			invoiceLine.JI_PartNo = product.OP_PartNum;

			AssertEquals(pivot.CI_TariffNum, invoiceLine.JI_Tariff);
			AssertEquals(0, invoiceLine.ProductRelatedLines.Count());
			AssertEquals(1, invoiceLine.ChildLines.Count());
			AssertEquals(relatedPivot.CI_TariffNum, invoiceLine.ChildLines.ElementAt(0).JI_Tariff);

			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Importer = header.PK;
			AssertNoExceptionThrown("Collection was modified; enumeration operation may not execute", delegate
			{ Factory.Save(); });
		}

		public void TestUpdateDetailsOnPartChange_AMMV()//add to make market value
		{
			JobDeclaration declaration = GetImportDeclarationWithBuyerSupplier();
			OrgSupplierPart product = GetPartWithRelationship(declaration.Importer);

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			CusClassPartPivot pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "6205202066";
			pivot.CI_SupplementalTariff = "99156101";
			pivot.CD_AMMVPerUnit = 7m;
			Factory.Save();//gets the addinfo fields populated

			invoiceLine.JI_PartNo = product.OP_PartNum;
			AssertEquals(pivot.CI_TariffNum, invoiceLine.JI_Tariff);
			AssertEquals(pivot.CI_SupplementalTariff, invoiceLine.US_SupTariff);
			AssertEquals(7m, invoiceLine.US_AMMVPerUnit);
		}

		public void TestUpdateAMMV()
		{
			#region SetUp Data
			var declaration = GetImportDeclarationWithBuyerSupplier();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Japan;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_InvoiceQuantity = 20m;
			invoiceLine.JI_LinePrice = 2000m;

			var product = GetPartWithRelationship(declaration.Importer);
			product.OP_PartNum = "ProductForTesting";
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CD_AMMVPercentage = 20m;
			pivot.CD_AMMVPerUnit = ZDecimal.Zero;

			var product2 = GetPartWithRelationship(declaration.Importer);
			product2.OP_PartNum = "ProductForTesting2";
			var pivot2 = product2.PivotsForBinding.AddNew();
			pivot2.CD_AMMVPercentage = ZDecimal.Zero;
			pivot2.CD_AMMVPerUnit = 20m;

			var product3 = GetPartWithRelationship(declaration.Importer);
			product3.OP_PartNum = "ProductForTesting3";
			var pivot3 = product3.PivotsForBinding.AddNew();
			pivot3.CD_AMMVPercentage = ZDecimal.Zero;
			pivot3.CD_AMMVPerUnit = ZDecimal.Zero;

			var product4 = GetPartWithRelationship(declaration.Importer);
			product4.OP_PartNum = "ProductForTesting4";
			var pivot4 = product4.PivotsForBinding.AddNew();
			pivot4.CD_AMMVPercentage = ZDecimal.Zero;
			pivot4.CD_AMMVPerUnit = 10m;
			pivot4.CD_AMMVPerUnitCurrency = "CAD";
			#endregion

			CombineAssertions(() =>
			{
				invoiceLine.JI_PartNo = product.OP_PartNum;
				AssertEquals("There should be a new ADD Charge in invoice line if AMMVPercentage was specified in product", 1, invoiceLine.ApportionedCharges.Count);
				var charge = invoiceLine.ApportionedCharges[0];
				AssertEquals(20m, charge.J7_Percentage);
				AssertEquals(400m, charge.J7_Amount);
				AssertEquals(Core.Constants.CurrencyCodes.Japan, charge.J7_RX_NKCurrency);
				AssertEquals(true, charge.IsAMMV());

				invoiceLine.JI_PartNo = product2.OP_PartNum;
				AssertEquals("AMMVPerUnit from the product should be copied across to the invoice line", 20m, invoiceLine.US_AMMVPerUnit);
				AssertEquals(1, invoiceLine.ApportionedCharges.Count);
				charge = invoiceLine.ApportionedCharges[0];
				AssertEquals(true, charge.J7_Percentage.IsEmpty);
				AssertEquals(400m, charge.J7_Amount);
				AssertEquals(Core.Constants.CurrencyCodes.UnitedStates, charge.J7_RX_NKCurrency);
				AssertEquals(true, charge.IsAMMV());

				invoiceLine.JI_PartNo = product4.OP_PartNum;
				AssertEquals("AMMVPerUnit from the product should be copied across to the invoice line", 10m, invoiceLine.US_AMMVPerUnit);
				AssertEquals(1, invoiceLine.ApportionedCharges.Count);
				charge = invoiceLine.ApportionedCharges[0];
				AssertEquals(true, charge.J7_Percentage.IsEmpty);
				AssertEquals(200m, charge.J7_Amount);
				AssertEquals(Core.Constants.CurrencyCodes.Canada, charge.J7_RX_NKCurrency);
				AssertEquals(true, charge.IsAMMV());

				invoiceLine.JI_PartNo = product.OP_PartNum;
				invoiceLine.US_AMMVPercentage = 0m;
				invoiceLine.US_AMMVPerUnit = 30m;
				AssertEquals("Charge should be created upon unit value input", 600m, invoiceLine.ApportionedCharges[0].J7_Amount);
				invoiceLine.JI_InvoiceQuantity = 30m;
				AssertEquals("Charge should be recalculated upon line quantity change", 900m, invoiceLine.ApportionedCharges[0].J7_Amount);
				invoiceLine.US_AMMVPerUnit = 0m;
				AssertEquals("Charge should be none if no amount", 0, invoiceLine.ApportionedCharges.Count);
				invoiceLine.US_AMMVPercentage = 10m;
				AssertEquals("Charge should be created upon percentage input", 1, invoiceLine.ApportionedCharges.Count);
				AssertEquals("Currency should change to line currency if calculate by percentage", Core.Constants.CurrencyCodes.Japan, invoiceLine.ApportionedCharges[0].J7_RX_NKCurrency);
				AssertEquals("Charge should be calculated by percentage and line price", 200m, invoiceLine.ApportionedCharges[0].J7_Amount);
				invoiceLine.JI_LinePrice = 3000m;
				AssertEquals("Charge should be recalculated upon line price change", 300m, invoiceLine.ApportionedCharges[0].J7_Amount);
				invoiceLine.JI_LinePrice = 0m;
				AssertEquals("Charge should be none if no amount", 0, invoiceLine.ApportionedCharges.Count);

				invoiceLine.JI_PartNo = product3.OP_PartNum;
				AssertEquals(0, invoiceLine.ApportionedCharges.Count);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				invoiceLine.US_AMMVPerUnit = 30m;
				AssertEquals("AMMV only works for import", 0, invoiceLine.ApportionedCharges.Count);
				invoiceLine.US_AMMVPercentage = 30m;
				AssertEquals("AMMV only works for import", 0, invoiceLine.ApportionedCharges.Count);
			});
		}

		public void TestAMMVReadOnly()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				AssertEquals(false, invoiceLine.US_AMMVPercentageInfo.ReadOnly);
				AssertEquals(false, invoiceLine.US_AMMVPerUnitInfo.ReadOnly);
				invoiceLine.US_AMMVPerUnit = 10m;
				AssertEquals(true, invoiceLine.US_AMMVPercentageInfo.ReadOnly);
				invoiceLine.US_AMMVPerUnit = 0m;
				AssertEquals(false, invoiceLine.US_AMMVPercentageInfo.ReadOnly);
				invoiceLine.US_AMMVPercentage = 10m;
				AssertEquals(true, invoiceLine.US_AMMVPerUnitInfo.ReadOnly);
				invoiceLine.US_AMMVPercentage = 0m;
				AssertEquals(false, invoiceLine.US_AMMVPerUnitInfo.ReadOnly);
				invoiceLine.AddInfoLookups.Parent.US_AMMVPercentage = 10m;
				invoiceLine.AddInfoLookups.Parent.US_AMMVPerUnit = 10m;
				AssertEquals(false, invoiceLine.US_AMMVPerUnitInfo.ReadOnly);
				AssertEquals(false, invoiceLine.US_AMMVPercentageInfo.ReadOnly);
			});
		}

		public void TestUS_AMSDisclaimRelatedFieldsReadOnly()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();

				invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Disclaimed;
				AssertEquals(false, invoiceLine.US_AMSDisclaimProgramInfo.ReadOnly);
				AssertEquals(false, invoiceLine.US_AMSDisclaimReasonInfo.ReadOnly);

				invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
				AssertEquals(true, invoiceLine.US_AMSDisclaimProgramInfo.ReadOnly);
				AssertEquals(true, invoiceLine.US_AMSDisclaimReasonInfo.ReadOnly);
			});
		}

		public void TestUS_PSTDisclaimRelatedsFieldReadOnly()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();

				invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Disclaimed;
				AssertEquals(false, invoiceLine.US_PSTDisclaimProgramInfo.ReadOnly);
				AssertEquals(false, invoiceLine.US_PSTDisclaimReasonInfo.ReadOnly);

				invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Declared;
				AssertEquals(true, invoiceLine.US_PSTDisclaimProgramInfo.ReadOnly);
				AssertEquals(true, invoiceLine.US_PSTDisclaimReasonInfo.ReadOnly);
			});
		}

		public void TestOnUS_AMMVPercentageChangedUS_AMMVPerUnitIsClearedOut()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.AddInfoLookups.Parent.US_AMMVPercentage = 10m;
				invoiceLine.AddInfoLookups.Parent.US_AMMVPerUnit = 10m;
				AssertEquals(10m, invoiceLine.US_AMMVPerUnit);
				AssertEquals(10m, invoiceLine.US_AMMVPercentage);

				invoiceLine.US_AMMVPercentage = 10m;
				AssertEquals(10m, invoiceLine.US_AMMVPerUnit);

				invoiceLine.US_AMMVPercentage = 0m;
				AssertEquals(10m, invoiceLine.US_AMMVPerUnit);

				invoiceLine.US_AMMVPercentage = 20m;
				AssertEquals(ZDecimal.Zero, invoiceLine.US_AMMVPerUnit);

				invoiceLine.US_AMMVPercentage = 30m;
				AssertEquals(ZDecimal.Zero, invoiceLine.US_AMMVPerUnit);
			});
		}

		public void TestOnUS_AMMVPerUnitChangedUS_AMMVPercentageIsClearedOut()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.AddInfoLookups.Parent.US_AMMVPercentage = 10m;
				invoiceLine.AddInfoLookups.Parent.US_AMMVPerUnit = 10m;
				AssertEquals(10m, invoiceLine.US_AMMVPerUnit);
				AssertEquals(10m, invoiceLine.US_AMMVPercentage);

				invoiceLine.US_AMMVPerUnit = 10m;
				AssertEquals(10m, invoiceLine.US_AMMVPercentage);

				invoiceLine.US_AMMVPerUnit = 0m;
				AssertEquals(10m, invoiceLine.US_AMMVPercentage);

				invoiceLine.US_AMMVPerUnit = 20m;
				AssertEquals(ZDecimal.Zero, invoiceLine.US_AMMVPercentage);

				invoiceLine.US_AMMVPerUnit = 30m;
				AssertEquals(ZDecimal.Zero, invoiceLine.US_AMMVPercentage);
			});
		}

		public void TestSetSupTariffForDerived()
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
			childLine1.JI_Tariff = "8215.99.3500";
			childLine1.JI_LinePrice = 2000;
			childLine1.JI_ParentID = parentLine.PK;
			AssertEquals(ZString.Empty, childLine1.US_SupTariff);

			parentLine.US_SupTariff = testHelper.Test99038801Tariff.UE_Tariff;
			AssertEquals(ZString.Empty, childLine1.US_SupTariff);
		}

		public void TestCalculateLinePriceAnd98GoodsValue()
		{
			JobDeclaration declaration = GetImportDeclarationWithBuyerSupplier();
			OrgSupplierPart product = GetPartWithRelationship(declaration.Importer);

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;

			CusClassPartPivot pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "6203434030";
			pivot.CI_SupplementalTariff = "9802008068";
			pivot.CD_9802USDValuePerUnit = 10;
			pivot.CD_PerUnitCost = 3;
			pivot.CD_RX_NKPerUnitCostCurr = invoice.JZ_RX_NKInvoice_Currency;

			CusClassPartPivot relatedPivot = pivot.Children.AddNew();
			relatedPivot.CI_ChildType = ClassificationChildTypeList.Codes.Related;
			relatedPivot.CI_TariffNum = "9801001010";
			relatedPivot.CD_PerUnitCost = 2;
			relatedPivot.CD_RX_NKPerUnitCostCurr = invoice.JZ_RX_NKInvoice_Currency;
			Factory.Save();//gets the addinfo fields populated

			invoiceLine.JI_PartNo = product.OP_PartNum;
			AssertNotNull(invoiceLine.Part);
			invoiceLine.JI_InvoiceQuantity = 123;
			AssertEquals(369m, invoiceLine.JI_LinePrice);
			AssertEquals(10m, invoiceLine.US_9802PerUnit);
			AssertEquals(1230m, invoiceLine.US_98GoodsValue);

			JobComInvoiceLine relatedInvoiceLine = invoiceLine.ProductRelatedLines.ElementAt(0);
			AssertEquals(invoiceLine.JI_InvoiceQuantity, relatedInvoiceLine.JI_InvoiceQuantity);
			AssertEquals(246m, relatedInvoiceLine.JI_LinePrice);
		}

		public void TestCalculateLinePriceAnd98GoodsValue_2()
		{
			JobDeclaration declaration = GetImportDeclarationWithBuyerSupplier();
			OrgSupplierPart product = GetPartWithRelationship(declaration.Importer);

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			CusClassPartPivot pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "6203434030";
			pivot.CI_SupplementalTariff = "9802008068";
			pivot.CD_9802ValuePerUnit = 10;
			pivot.CD_RX_NK9802ValuePerUnitCurr = invoice.JZ_RX_NKInvoice_Currency;
			pivot.CD_PerUnitCost = 3;
			pivot.CD_RX_NKPerUnitCostCurr = invoice.JZ_RX_NKInvoice_Currency;

			Factory.Save();//gets the addinfo fields populated

			invoiceLine.JI_PartNo = product.OP_PartNum;
			AssertNotNull(invoiceLine.Part);
			invoiceLine.JI_InvoiceQuantity = 123;
			AssertEquals(369m, invoiceLine.JI_LinePrice);
			AssertEquals(10m, invoiceLine.US_98InvCurrPerUnit);
			AssertEquals(1230m, invoiceLine.US_98ValueInvCurr);

			pivot.CD_RX_NK9802ValuePerUnitCurr = ZString.Empty;
			invoiceLine.Delete();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			Factory.Save();
			invoiceLine.JI_PartNo = product.OP_PartNum;
			AssertNotNull(invoiceLine.Part);
			invoiceLine.JI_InvoiceQuantity = 123;
			AssertEquals(369m, invoiceLine.JI_LinePrice);
			AssertEquals(0m, invoiceLine.US_98InvCurrPerUnit);
			AssertEquals(0m, invoiceLine.US_98ValueInvCurr);
		}

		public void TestDeleteFDACollectionsForPriorNotice()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableSPN = true;
			Assert("PreCondition", declaration.CanHavePGAFDA);

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_FDAIndicator = "D";
			invoiceLine.ACE_FDALines.AddNew();

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertEquals("FDA indicator is shared between ACE and ACS. Should stay", "D", invoiceLine.US_FDAIndicator);
			AssertEquals(0, invoiceLine.ACE_FDALines.Count);

			invoiceLine.FDAs.AddNew();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			AssertEquals("FDA indicator is shared between ACE and ACS. Should stay", "D", invoiceLine.US_FDAIndicator);
			AssertEquals(0, invoiceLine.FDAs.Count);
		}

		public void TestCalculateLinePriceAndAMMV()
		{
			JobDeclaration declaration = GetImportDeclarationWithBuyerSupplier();
			OrgSupplierPart product = GetPartWithRelationship(declaration.Importer);

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			CusClassPartPivot pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "6203434030";
			pivot.CI_SupplementalTariff = "9802008068";
			pivot.CD_PerUnitCost = 3;
			pivot.CD_RX_NKPerUnitCostCurr = invoice.JZ_RX_NKInvoice_Currency;
			pivot.CD_AMMVPerUnit = 5.23;
			Factory.Save();//gets the addinfo fields populated

			invoiceLine.JI_PartNo = product.OP_PartNum;

			invoiceLine.JI_InvoiceQuantity = 100;
			AssertEquals(300m, invoiceLine.JI_LinePrice);

			InvoiceLineApportionCharge ammvCharge = null;
			foreach (InvoiceLineApportionCharge charge in invoiceLine.ApportionedCharges)
			{
				if (charge.IsAMMV())
				{
					ammvCharge = charge;
					break;
				}
			}

			AssertEquals(523m, ammvCharge.J7_Amount);
			AssertEquals(Core.Constants.CurrencyCodes.UnitedStates, ammvCharge.J7_RX_NKCurrency);

			invoiceLine.US_AMMVPerUnit = ZDecimal.Zero;
			AssertEquals(true, ammvCharge.IsDeleted);

			ammvCharge = null;
			foreach (InvoiceLineApportionCharge charge in invoiceLine.ApportionedCharges)
			{
				if (charge.IsAMMV())
				{
					ammvCharge = charge;
					break;
				}
			}
			AssertNull(ammvCharge);
		}

		public void TestDeleteAllApportionedChargesOnInvoiceQuantityChange()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_InvoiceQuantity = 2m;
			invoiceLine.US_AMMVPerUnit = 10m;
			var charge = invoiceLine.ApportionedCharges[0];
			var charge2 = invoiceLine.ApportionedCharges.AddNew();
			charge2.J7_IsSystem = true;
			charge2.J7_IsNotIncludedInInvoice = true;
			charge2.J7_IsDutiable = true;
			charge2.J7_ChargeType = Enterprise.Customs.Business.CustomsChargeTypeList.Codes.AdditionCharge;

			AssertEquals(2, invoiceLine.ApportionedCharges.Count);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceLine.JI_InvoiceQuantity = 3m;
			AssertEquals("If the declaration is not of type import, there should not be an Assist Charge", 0, invoiceLine.ApportionedCharges.Count);
			AssertEquals(true, charge.IsDeleted);
			AssertEquals(true, charge2.IsDeleted);
		}

		public void TestSetProductRelatedLineUQ()
		{
			JobDeclaration declaration = GetDeclarationWithOrgSupplierPart();
			JobComInvoiceLine invoiceLine = declaration.Invoices[0].JobComInvoiceLines[0];
			JobComInvoiceLine relatedInvoiceLine = invoiceLine.AddProductRelatedInvoiceLine();

			invoiceLine.JI_InvoiceUQ = "XX";

			AssertEquals(invoiceLine.JI_InvoiceUQ, relatedInvoiceLine.JI_InvoiceUQ);
		}

		public void TestProductRelatedLines()
		{
			JobDeclaration declaration = GetImportDeclarationWithBuyerSupplier();
			OrgSupplierPart product = GetPartWithRelationship(declaration.Importer);
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			CusClassPartPivot pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_DateStart = ZDateTime.Now.AddDays(-10);
			pivot.CI_TariffNum = "00044400";

			CusClassPartPivot childPivot = pivot.Children.AddNew();
			childPivot.CI_ChildType = ClassificationChildTypeList.Codes.Related;
			childPivot.CI_TariffNum = "00044400";
			Factory.Save();//gets the addinfo fields populated

			invoiceLine.JI_PartNo = product.OP_PartNum;
			AssertNotNull(invoiceLine.Part);
			AssertEquals(1, invoiceLine.ProductRelatedLines.Count());
			AssertEquals(childPivot.CI_TariffNum, invoiceLine.ProductRelatedLines.ElementAt(0).JI_Tariff);
			AssertNotEquals(invoiceLine, invoiceLine.ProductRelatedLines.ElementAt(0));
			AssertEquals(invoiceLine, invoiceLine.ProductRelatedLines.ElementAt(0).ProductParentTariffLine);
		}

		public void TestMoveRelatedLineWithProductToAnotherInvoice()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLine relatedInvoiceLine = invoiceLine.AddProductRelatedInvoiceLine();

			JobComInvoiceHeader invoice2 = declaration.Invoices.AddNew();
			invoiceLine.JI_JZ = invoice2.PK;

			AssertEquals(invoice2.PK, relatedInvoiceLine.JI_JZ);
			AssertEquals(invoiceLine, relatedInvoiceLine.ProductParentTariffLine);
		}

		public void TestProductParentTariffLine()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLine relatedInvoiceLine = invoiceLine.AddProductRelatedInvoiceLine();
			AssertEquals(invoiceLine, relatedInvoiceLine.ProductParentTariffLine);
		}

		public void TestUpdateDetailsOnPartChange_PGA()
		{
			OrgSupplierPart product = GetProductForFDAAndPGATests();
			PGA pGA = product.PivotsForBinding[0].PGAs.AddNew();

			ConstituentElement element = pGA.PG04ConstituentElements.AddNew();
			element.US_PGAUnitOfMeasure = LaceyActUnitsOfMeasureList.Codes.Kilograms;
			Factory.Save();

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_OH_Importer = importer.PK;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Supplier = supplier.PK;
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = product.OP_PartNum;
			AssertNotNull(invoiceLine.Part);
			AssertEquals(1, invoiceLine.LaceyActLines[0].PG04ConstituentElements.Count);
			AssertEquals(LaceyActUnitsOfMeasureList.Codes.Kilograms, invoiceLine.LaceyActLines[0].PG04ConstituentElements[0].US_PGAUnitOfMeasure);
		}

		public void TestClassificationDetailsAreUpdatedWhenTariffTypeChanges()
		{
			OrgHeader importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();

			OrgHeader supplier = Factory.New<OrgHeader>();
			supplier.FillWithValidTestData();

			OrgSupplierPart product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "BLUTAK";
			product.OP_Desc = "Poor man's stress ball.";
			product.RelatedOrganisations.AddOwner(importer);
			product.RelatedOrganisations.AddSupplier(supplier);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusTariffTypeSB = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.ScheduleB);
			Factory.Save();

			var scheduleB = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, cusTariffTypeSB.PK, "1234567890", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			helper.CreateTariffUOM(scheduleB, "CU1", "S1");
			helper.CreateTariffUOM(scheduleB, "CU2", "S2");

			CusClassification shbClassification = Factory.New<CusClassification>();
			shbClassification.CC_LookupCode = "1234567890";
			shbClassification.CC_TariffNum = "1234567890";
			shbClassification.CC_ClassificationType = CusClassification.ClassificationType.EXP;

			var cusTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.Export);
			Factory.Save();
			var hte1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, cusTariffType.PK, "0987654321", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			helper.CreateTariffUOM(hte1, "CU1", "H1");
			helper.CreateTariffUOM(hte1, "CU2", "H2");
			CusClassification hteClassification = Factory.New<CusClassification>();
			hteClassification.CC_LookupCode = "0987654321";
			hteClassification.CC_TariffNum = "0987654321";
			hteClassification.CC_ClassificationType = CusClassification.ClassificationType.EXP;

			var hte2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, cusTariffType.PK, "8536509065", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			helper.CreateTariffUOM(hte2, "CU1", "H3");
			helper.CreateTariffUOM(hte2, "CU2", "H4");
			CusClassPartPivot htePivot = product.PivotsForBinding.AddNew();
			htePivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			htePivot.CI_TariffNum = "8536509065";
			htePivot.CD_ECCN = "12345";

			var scheduleB2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, cusTariffTypeSB.PK, "7536509065", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			helper.CreateTariffUOM(scheduleB2, "CU1", "S3");
			helper.CreateTariffUOM(scheduleB2, "CU2", "S4");

			CusClassPartPivot shbPivot = product.PivotsForBinding.AddNew();
			shbPivot.CI_ChildType = ClassificationTypeList.Codes.SHB;
			shbPivot.CI_TariffNum = "7536509065";
			shbPivot.CD_ECCN = "54321";

			Factory.Save();

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.US_ECCN = "AAAAA";
			declaration.US_TariffType = TariffTypeList.Codes.ScheduleB;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = product.OP_PartNum;

			AssertEquals("Tariff type should be Schedule B", TariffTypeList.Codes.ScheduleB, invoiceLine.US_TariffType);
			AssertEquals("Tariff number of invoice line should match the Schedule B product line", "7536509065", invoiceLine.JI_Tariff);
			AssertEquals("Invoice line ECCN", "54321", invoiceLine.US_ECCN);
			AssertEquals("S3", invoiceLine.JI_CustomsUnitQty);
			AssertEquals("S4", invoiceLine.JI_CustomsSecondUnitQty);

			shbPivot.CI_TariffNum = ZString.Empty;
			shbPivot.CI_CC = shbClassification.PK;
			declaration.US_TariffType = TariffTypeList.Codes.HTS;

			AssertEquals("Tariff type should be HTS", TariffTypeList.Codes.HTS, invoiceLine.US_TariffType);
			AssertEquals("Tariff number of invoice line should match the HTS Export product line", "8536509065", invoiceLine.JI_Tariff);
			AssertEquals("Invoice line ECCN", "12345", invoiceLine.US_ECCN);
			AssertEquals("H3", invoiceLine.JI_CustomsUnitQty);
			AssertEquals("H4", invoiceLine.JI_CustomsSecondUnitQty);

			declaration.US_TariffType = TariffTypeList.Codes.ScheduleB;
			AssertEquals("Tariff number of invoice line should correspond to expClassification1", shbClassification.CC_TariffNum, invoiceLine.JI_Tariff);
			AssertEquals("Classification number of invoice line should correspond to expClassification1", shbClassification.PK, invoiceLine.JI_CC);
			AssertEquals("Invoice line ECCN", "54321", invoiceLine.US_ECCN);
			AssertEquals("S1", invoiceLine.JI_CustomsUnitQty);
			AssertEquals("S2", invoiceLine.JI_CustomsSecondUnitQty);

			htePivot.CI_TariffNum = ZString.Empty;
			htePivot.CI_CC = hteClassification.PK;
			declaration.US_TariffType = TariffTypeList.Codes.HTS;
			AssertEquals("Tariff number of invoice line should correspond to expClassification2", hteClassification.CC_TariffNum, invoiceLine.JI_Tariff);
			AssertEquals("Classification number of invoice line should correspond to expClassification2", hteClassification.PK, invoiceLine.JI_CC);
			AssertEquals("Invoice line ECCN", "12345", invoiceLine.US_ECCN);
			AssertEquals("H1", invoiceLine.JI_CustomsUnitQty);
			AssertEquals("H2", invoiceLine.JI_CustomsSecondUnitQty);

			product.PivotsForBinding.RemoveAndDelete(shbPivot);
			declaration.US_TariffType = TariffTypeList.Codes.ScheduleB;
			AssertEquals("Tariff type should be SHB", TariffTypeList.Codes.ScheduleB, invoiceLine.US_TariffType);
			AssertEquals("Should keep previous Tariff Num", hteClassification.CC_TariffNum, invoiceLine.JI_Tariff);
			AssertEquals("Should clear classification", ZGuid.Empty, invoiceLine.JI_CC);
			AssertEquals("Should fall back to Declaration", "AAAAA", invoiceLine.US_ECCN);
			AssertEquals("H1", invoiceLine.JI_CustomsUnitQty);
			AssertEquals("H2", invoiceLine.JI_CustomsSecondUnitQty);
		}

		public void TestUpdateDetailsOnPartChange_AES()
		{
			OrgHeader importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();

			OrgHeader supplier = Factory.New<OrgHeader>();
			supplier.FillWithValidTestData();

			OrgSupplierPart product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "NICE WATCH";
			product.OP_Desc = "Nice Watch Batter-Powered";
			product.RelatedOrganisations.AddOwner(importer);
			product.RelatedOrganisations.AddSupplier(supplier);

			CusClassification expClassification = Factory.New<CusClassification>();
			expClassification.CC_LookupCode = "TestEXPLookup";
			expClassification.CC_ClassificationType = CusClassification.ClassificationType.EXP;

			CusClassification impClassification = Factory.New<CusClassification>();
			impClassification.CC_LookupCode = "TestIMPLookup";
			impClassification.CC_ClassificationType = CusClassification.ClassificationType.IMP;

			CusClassPartPivot htePivot = product.PivotsForBinding.AddNew();
			htePivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			htePivot.CI_CC = impClassification.PK;

			htePivot.CD_OriginIndicator = AESOriginIndicatorList.Codes.Domestic;
			htePivot.CD_ECCN = "EC234";
			htePivot.CD_LicenceType = USAESLicenseCode.Codes.SAG;
			htePivot.CD_LicenceNo = "LC234";
			htePivot.CD_ITARExemptionNo = "EX234";
			htePivot.CD_MilitaryEquipInd = YesNoDefaultList.Codes.Yes;
			htePivot.CD_PartyCertInd = YesNoDefaultList.Codes.Yes;
			htePivot.CD_DDTCRegoNo = "REG322";
			htePivot.CD_DDTCUSMLCategoryCode = USMLCategoryCodes.Codes.Ammunition;
			htePivot.CD_DDTCUnit = ABIUnitOfMeasureList.Codes.Barrels;
			htePivot.CD_PerUnitCost = 5m;
			htePivot.CD_RX_NKPerUnitCostCurr = "GBP";

			CusClassPartPivot schBPivot = product.PivotsForBinding.AddNew();
			schBPivot.CI_ChildType = ClassificationTypeList.Codes.SHB;
			schBPivot.CI_CC = expClassification.PK;

			schBPivot.CD_OriginIndicator = AESOriginIndicatorList.Codes.Foreign;
			schBPivot.CD_ECCN = "EC695";
			schBPivot.CD_LicenceType = USAESLicenseCode.Codes.SCA;
			schBPivot.CD_LicenceNo = "LC685";
			schBPivot.CD_ITARExemptionNo = "EX695";
			schBPivot.CD_MilitaryEquipInd = YesNoDefaultList.Codes.No;
			schBPivot.CD_PartyCertInd = YesNoDefaultList.Codes.No;
			schBPivot.CD_DDTCRegoNo = "REG935";
			schBPivot.CD_DDTCUSMLCategoryCode = USMLCategoryCodes.Codes.ArtilleryProjectors;
			schBPivot.CD_DDTCUnit = ABIUnitOfMeasureList.Codes.Number;
			schBPivot.CD_PerUnitCost = 7.5m;
			schBPivot.CD_RX_NKPerUnitCostCurr = "GBP";

			Factory.Save();

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.US_TariffType = TariffTypeList.Codes.ScheduleB;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "GBP";
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = product.OP_PartNum;
			AssertEquals(AESOriginIndicatorList.Codes.Foreign, invoiceLine.US_AESOriginIndicator);
			AssertEquals("EC695", invoiceLine.US_ECCN);
			AssertEquals("LC685", invoiceLine.US_LicenseNo);
			AssertEquals(USAESLicenseCode.Codes.SCA, invoiceLine.US_LicenseType);
			AssertEquals("EX695", invoiceLine.US_DDTCITARExemptionNo);
			AssertEquals(YesNoDefaultList.Codes.No, invoiceLine.US_DDTCMilitaryEquipmentIndicator);
			AssertEquals(YesNoDefaultList.Codes.No, invoiceLine.US_DDTCPartyCertificationIndicator);
			AssertEquals("REG935", invoiceLine.US_DDTCRegistrationNo);
			AssertEquals(USMLCategoryCodes.Codes.ArtilleryProjectors, invoiceLine.US_DDTCUSMLCategoryCode);
			AssertEquals(ABIUnitOfMeasureList.Codes.Number, invoiceLine.US_DDTCUnit);
			AssertEquals(7.5m, invoiceLine.UnitPrice);
			AssertEquals(true, invoiceLine.US_JurisdictionNumberInfo.ReadOnly);

			invoiceLine.JI_PartNo = ZString.Empty;
			AssertEquals(invoice.US_AESOriginIndicator, invoiceLine.US_AESOriginIndicator);
			AssertEquals("", invoiceLine.US_ECCN);
			AssertEquals(invoice.US_LicenseNo, invoiceLine.US_LicenseNo);
			AssertEquals(invoice.US_LicenseType, invoiceLine.US_LicenseType);
			AssertEquals("", invoiceLine.US_DDTCITARExemptionNo);
			AssertEquals("", invoiceLine.US_DDTCMilitaryEquipmentIndicator);
			AssertEquals("", invoiceLine.US_DDTCPartyCertificationIndicator);
			AssertEquals("", invoiceLine.US_DDTCRegistrationNo);
			AssertEquals("", invoiceLine.US_DDTCUSMLCategoryCode);
			AssertEquals("", invoiceLine.US_DDTCUnit);
			AssertEquals(true, invoiceLine.US_JurisdictionNumberInfo.ReadOnly);

			declaration.US_TariffType = TariffTypeList.Codes.HTS;
			invoiceLine.JI_PartNo = product.OP_PartNum;
			AssertEquals(AESOriginIndicatorList.Codes.Domestic, invoiceLine.US_AESOriginIndicator);
			AssertEquals("EC234", invoiceLine.US_ECCN);
			AssertEquals("LC234", invoiceLine.US_LicenseNo);
			AssertEquals(USAESLicenseCode.Codes.SAG, invoiceLine.US_LicenseType);
			AssertEquals("EX234", invoiceLine.US_DDTCITARExemptionNo);
			AssertEquals(YesNoDefaultList.Codes.Yes, invoiceLine.US_DDTCMilitaryEquipmentIndicator);
			AssertEquals(YesNoDefaultList.Codes.Yes, invoiceLine.US_DDTCPartyCertificationIndicator);
			AssertEquals("REG322", invoiceLine.US_DDTCRegistrationNo);
			AssertEquals(USMLCategoryCodes.Codes.Ammunition, invoiceLine.US_DDTCUSMLCategoryCode);
			AssertEquals(ABIUnitOfMeasureList.Codes.Barrels, invoiceLine.US_DDTCUnit);
			AssertEquals(5m, invoiceLine.UnitPrice);
			AssertEquals(true, invoiceLine.US_JurisdictionNumberInfo.ReadOnly);

			invoice.JZ_RX_NKInvoice_Currency = "USD";
			invoiceLine.JI_PartNo = ZString.Empty;
			invoiceLine.UnitPrice = 3m;
			invoiceLine.JI_PartNo = product.OP_PartNum;
			AssertEquals(3m, invoiceLine.UnitPrice);
		}

		public void TestOGAIndicators()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "10000000";
			tariff.UE_OGACodes = "FD2FC3FC4DT1DT2";
			tariff.UE_DateFrom = ZDateTime.Today.AddDays(-2);
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);
			tariff.UE_Column1RateSpecific = 0.5m;
			InvoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			InvoiceLine.US_DOTIndicator = OGAIndicatorList.Codes.Disclaimed;
			InvoiceLine.JI_Tariff = tariff.UE_Tariff;

			AssertEquals("FDA should be defaulted", OGAIndicatorList.Codes.Declared, InvoiceLine.US_FDAIndicator);
			AssertEquals("DOT should be defaulted", OGAIndicatorList.Codes.Declared, InvoiceLine.US_DOTIndicator);

			tariff.UE_OGACodes = "FD1FD3";
			InvoiceLine.JI_Tariff = ZString.Empty;
			InvoiceLine.JI_Tariff = tariff.UE_Tariff;
			AssertEquals("FDA should be Declared", OGAIndicatorList.Codes.Declared, InvoiceLine.US_FDAIndicator);
		}

		public void TestUS_DOTIndicator()
		{
			InvoiceLine.DOTs.AddNew();
			InvoiceLine.US_DOTIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertEquals("should not delete DOT lines", 1, InvoiceLine.DOTs.Count);

			InvoiceLine.US_DOTIndicator = "";
			AssertEquals("should not delete DOT lines", 1, InvoiceLine.DOTs.Count);
		}

		public void TestIsDutyFree()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "10000000";
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.SpecificRateFirstQuantity;
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);
			tariff.UE_Column1RateSpecific = 0.5m;

			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_LinePrice = 10000m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("IsDutyFree", false, invoiceLine.IsDutyFree);

			tariff.UE_Column1RateSpecific = ZDecimal.Zero;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("IsDutyFree", true, invoiceLine.IsDutyFree);

			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.CompoundSpecificAndAdValoremFirstQuantity;
			tariff.UE_Column1RateSpecific = 0.3m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("IsDutyFree", false, invoiceLine.IsDutyFree);
		}

		public void TestApportionIntoLinesWithPrice()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 5000m;

			JobComInvoiceLine invoiceLine2 = invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine2.JI_LinePrice = 1000m;

			JobComInvoiceLine invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine3.JI_LinePrice = 1000m;

			JobComInvoiceLine invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			AssertEquals("JI_ParentID is set", invoiceLine3, invoiceLine4.ParentTariffLine);

			invoiceLine4.JI_LinePrice = 1000m;

			JobComInvoiceLine invoiceLine5 = invoice.JobComInvoiceLines.AddNew();

			invoice.Charges.AddNew("OFT", 500m, JobDeclaration.LocalCurrencyConstantCode);
			declaration.ResumeApportionment();
			AssertEquals("invoiceLine should have apportioned charges", 1, invoiceLine.ApportionedCharges.Count);
			AssertEquals("invoiceLine2 should have apportioned charges", 1, invoiceLine2.ApportionedCharges.Count);
			AssertEquals("invoiceLine3 is a X line", 0, invoiceLine3.ApportionedCharges.Count);
			AssertEquals("invoiceLine4 is a V line", 1, invoiceLine4.ApportionedCharges.Count);
			AssertEquals("invoiceLine5 does not have a price", 1, invoiceLine5.ApportionedCharges.Count);
			AssertEquals("InvoiceLine5 does not have a price", 0m, invoiceLine5.ApportionedCharges[0].J7_Amount);
		}

		public void TestInvoiceValidationForXLine()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine1.JI_LinePrice = 10000m;

			AssertEquals("JZ_Calc_LinesEntered:Disregard X lines", 0m, invoice.JZ_Calc_LinesEntered);

			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			AssertEquals("JI_ParentID is set", invoiceLine1, invoiceLine2.ParentTariffLine);
			invoiceLine2.JI_LinePrice = 2000m;

			AssertEquals("JZ_Calc_LinesEntered", 2000m, invoice.JZ_Calc_LinesEntered);

			JobComInvoiceLine invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			AssertEquals("JI_ParentID is set", invoiceLine1, invoiceLine3.ParentTariffLine);
			invoiceLine3.JI_LinePrice = 8000m;

			AssertEquals("JZ_Calc_LinesEntered", 10000m, invoice.JZ_Calc_LinesEntered);
		}

		public void TestDateOfExportFromCountryOfOrigin()
		{
			InvoiceHeader.US_DateOfExportFromCountryOfOrigin = ZDateTime.Today;
			AssertEquals(ZDateTime.Today, InvoiceLine.US_DateOfExportFromCountryOfOrigin);

			InvoiceLine.US_DateOfExportFromCountryOfOrigin = ZDateTime.BrettsBirthday;
			AssertEquals(ZDateTime.BrettsBirthday, InvoiceLine.US_DateOfExportFromCountryOfOrigin);
		}

		public void TestCommercialDescription()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.US_EnableAII = true;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			AssertEquals("", invoiceLine.CommercialDescription);
			invoiceLine.JI_Description = "BOB THE BUILDER";
			AssertEquals("BOB THE BUILDER", invoiceLine.CommercialDescription);
			invoiceLine.JI_ExtraInfoForClassification = "WENDY THE DESTROYER";
			AssertEquals("BOB THE BUILDER" + System.Environment.NewLine + "WENDY THE DESTROYER", invoiceLine.CommercialDescription);
		}

		public void TestUS_SelectedRateType()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1010101010";
			tariff.UE_DateFrom = ZDateTime.Today.AddDays(-10);
			tariff.UE_DateTo = ZDateTime.Today.AddDays(+10);
			tariff.UE_DutyComputationCode = "";
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			JobComInvoiceLine invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.US_SelectedRateType = RateTypeList.Codes.Primary;
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			AssertEquals("", invoiceLine.US_SelectedRateType);
			tariff.UE_Tariff = "1010101011";
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.SpecificSpecific;
			invoiceLine.US_SelectedRateType = RateTypeList.Codes.Primary;
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			AssertEquals(RateTypeList.Codes.Primary, invoiceLine.US_SelectedRateType);
		}

		public void TestPartType()
		{
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			OrgHeader importer = factory2.New<OrgHeader>();
			importer.FillWithValidTestData();
			MasterFiles.Business.OrgSupplierPart product = (MasterFiles.Business.OrgSupplierPart)factory2.New<Integration.Customs.US.IOrgSupplierPart>();
			product.OP_PartNum = "TestTEST";
			product.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			factory2.Save();

			GlbCompany usCompany = Factory.New<GlbCompany>();
			usCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			GlbBranch usBranch = usCompany.Branches.AddNew();
			usBranch.GB_RL_NKHomePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedStates)).RL_Code;

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_GB = usBranch.PK;
			JobComInvoiceLine invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "TestTEST";
			AssertEquals("Product type gets changed depending on who is requesting", typeof(OrgSupplierPart), invoiceLine.Part.GetType());
			Factory.Save();

			GlbCompany.CurrentCompany.SetCountry("AU");
			BusinessObjectFactory factory3 = new BusinessObjectFactory();
			JobDeclaration declarationLoaded = factory3.Load<JobDeclaration>(declaration.PK);
			AssertEquals("product type still the right type type", typeof(OrgSupplierPart), declarationLoaded.InvoiceLines[0].Part.GetType());
		}

		public void TestIsNormalOrParentOrSetHeaderLine()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableINB = true;

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			AssertEquals("IsNormalOrParentOrSetHeaderLine", true, invoiceLine.IsNormalOrParentOrSetXLine);

			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			AssertEquals("IsSetComponentLine", true, invoiceLine.IsSetVLine);
			AssertEquals("IsSetHeaderLine", false, invoiceLine.IsSetXLine);
			AssertEquals("IsNormalOrParentOrSetHeaderLine", false, invoiceLine.IsNormalOrParentOrSetXLine);

			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			AssertEquals("IsSetHeaderLine", true, invoiceLine.IsSetXLine);
			AssertEquals("IsSetComponentLine", false, invoiceLine.IsSetVLine);
			AssertEquals("IsNormalOrParentOrSetHeaderLine", true, invoiceLine.IsNormalOrParentOrSetXLine);

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			AssertEquals("IsNormalOrParentOrSetHeaderLine", true, invoiceLine2.IsNormalOrParentOrSetXLine);

			invoiceLine2.JI_ParentID = invoiceLine.PK;
			AssertEquals("IsNormalOrParentOrSetHeaderLine", false, invoiceLine2.IsNormalOrParentOrSetXLine);

			invoiceLine.US_SecondarySPI = "";
			AssertEquals("as it has secondary lines", true, invoiceLine.IsNormalOrParentOrSetXLine);
		}

		public void TestJZ_Calc_Balance()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 1000m;
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			declaration.ResumeApportionment();
			AssertEquals(1000m, invoiceHeader.JZ_Calc_Balance);
			invoiceLine.JI_LinePrice = 500m;
			JobComInvoiceLine secondaryLine = invoiceLine.AddSecondaryInvoiceLine();
			secondaryLine.JI_LinePrice = 500m;

			declaration.ResumeApportionment();
			AssertEquals(0m, invoiceHeader.JZ_Calc_Balance);
		}

		public void TestFDAs()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertNotNull(InvoiceLine.FDAs);
			AssertEquals(false, InvoiceLine.HasChanges);
			InvoiceLine.FDAs.AddNew();
			AssertEquals(true, InvoiceLine.HasChanges);
		}

		public void TestHasFDAData()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertEquals(false, InvoiceLine.HasFDAData);

			FDA fda = InvoiceLine.FDAs.AddNew();
			AssertEquals(true, InvoiceLine.HasFDAData);
		}

		public void TestRequiresPriorNoticeReporting()
		{
			ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.PGAFSIS, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, false);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertEquals(false, InvoiceLine.RequiresPriorNoticeReporting());

			InvoiceLine.JI_Tariff = USCTariff.FDAAdmissibilityReviewDONOTSUBMITTariff;
			AssertEquals(false, InvoiceLine.RequiresPriorNoticeReporting());

			InvoiceLine.JI_Tariff = USCTariff.CottonFeeApplicable;
			AssertEquals(false, InvoiceLine.RequiresPriorNoticeReporting());

			InvoiceLine.JI_Tariff = USCTariff.FDAAdmissibilityReviewMayBeRequiredTariff;
			AssertEquals(false, InvoiceLine.RequiresPriorNoticeReporting());

			InvoiceLine.JI_Tariff = USCTariff.FDAAdmissibilityReviewRequiredTariff;
			AssertEquals(false, InvoiceLine.RequiresPriorNoticeReporting());

			InvoiceLine.JI_Tariff = USCTariff.FDAPriorNoticeMayBeRequiredTariff;
			AssertEquals(false, InvoiceLine.RequiresPriorNoticeReporting());

			InvoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertEquals(false, InvoiceLine.RequiresPriorNoticeReporting());

			InvoiceLine.JI_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
			AssertEquals(true, InvoiceLine.RequiresPriorNoticeReporting());

			InvoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			InvoiceLine.JI_Tariff = USCTariff.FDAAdmissibilityReviewRequiredTariff;
			InvoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			InvoiceLine.FDAs.AddNew().US_FDAForcePN = true;
			AssertEquals(true, InvoiceLine.RequiresPriorNoticeReporting());

			InvoiceLine.FDAs[0].US_FDAForcePN = false;
			InvoiceLine.JI_Tariff = USCTariff.FDAPriorNoticeMayBeRequiredTariff;
			InvoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			InvoiceLine.FDAs[0].US_PND = true;
			AssertEquals(false, InvoiceLine.RequiresPriorNoticeReporting());

			InvoiceLine.FDAs[0].US_PND = false;
			AssertEquals(true, InvoiceLine.RequiresPriorNoticeReporting());

			InvoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			InvoiceLine.Declaration.US_EnableENS = true;
			InvoiceLine.Declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			InvoiceLine.FDAs.RemoveAll();
			InvoiceLine.FDAs.AddNew().US_FDAForcePN = true;
			AssertEquals(true, InvoiceLine.RequiresPriorNoticeReporting());

			InvoiceLine.Declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			InvoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			AssertEquals(false, InvoiceLine.RequiresPriorNoticeReporting());

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddMonths(1);
			tariff.UE_OGACodes = "FD4";
			InvoiceLine.JI_Tariff = tariff.UE_Tariff;
			InvoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			AssertEquals(true, InvoiceLine.RequiresPriorNoticeReporting());

			InvoiceLine.US_FTZCurrentTariff = tariff.UE_Tariff;
			InvoiceLine.JI_Tariff = ZString.Empty;
			AssertEquals(true, InvoiceLine.RequiresPriorNoticeReporting());

			tariff.UE_OGACodes = "";
			tariff.UE_PGACodes = "FD3";
			InvoiceLine.JI_Tariff = tariff.UE_Tariff;
			InvoiceLine.US_FTZCurrentTariff = ZString.Empty;
			InvoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			AssertEquals(false, InvoiceLine.RequiresPriorNoticeReporting());

			InvoiceLine.US_FTZCurrentTariff = tariff.UE_Tariff;
			InvoiceLine.JI_Tariff = ZString.Empty;
			InvoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			AssertEquals(false, InvoiceLine.RequiresPriorNoticeReporting());

			var fdaLine = InvoiceLine.ACE_FDALines.AddNew();
			fdaLine.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			AssertEquals(false, InvoiceLine.RequiresPriorNoticeReporting());
			fdaLine.US_PND = true;
			AssertEquals(false, InvoiceLine.RequiresPriorNoticeReporting());

			fdaLine.US_PND = false;
			tariff.UE_PGACodes = ZString.Empty;
			InvoiceLine.JI_Tariff = tariff.UE_Tariff;
			InvoiceLine.US_FTZCurrentTariff = ZString.Empty;
			InvoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			AssertEquals(false, InvoiceLine.RequiresPriorNoticeReporting());

			InvoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			fdaLine.US_FDAForcePN = true;
			AssertEquals(true, InvoiceLine.RequiresPriorNoticeReporting());

			InvoiceLine.Declaration.US_EnableENS = false;
			InvoiceLine.Declaration.US_EnableCRL = false;
			InvoiceLine.Declaration.US_EnableSPN = true;
			InvoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			fdaLine = InvoiceLine.ACE_FDALines.AddNew();
			fdaLine.US_FDAForcePN = true;
			AssertEquals(true, InvoiceLine.RequiresPriorNoticeReporting());
		}

		public void TestHasFDATariffsToBeDeclared()
		{
			AssertEquals(false, InvoiceLine.IsFDADeclared);

			InvoiceLine.JI_Tariff = USCTariff.FDAAdmissibilityReviewDONOTSUBMITTariff;
			AssertEquals(false, InvoiceLine.IsFDADeclared);

			InvoiceLine.JI_Tariff = USCTariff.CottonFeeApplicable;
			AssertEquals(false, InvoiceLine.IsFDADeclared);

			InvoiceLine.JI_Tariff = USCTariff.FDAAdmissibilityReviewMayBeRequiredTariff;
			InvoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			AssertEquals(true, InvoiceLine.IsFDADeclared);

			InvoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertEquals(false, InvoiceLine.IsFDADeclared);

			InvoiceLine.JI_Tariff = USCTariff.FDAAdmissibilityReviewRequiredTariff;
			InvoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			AssertEquals(true, InvoiceLine.IsFDADeclared);

			InvoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertEquals(false, InvoiceLine.IsFDADeclared);

			InvoiceLine.JI_Tariff = USCTariff.FDAPriorNoticeMayBeRequiredTariff;
			InvoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			AssertEquals(true, InvoiceLine.IsFDADeclared);

			InvoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertEquals(false, InvoiceLine.IsFDADeclared);

			InvoiceLine.JI_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
			InvoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			AssertEquals(true, InvoiceLine.IsFDADeclared);

			InvoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertEquals(false, InvoiceLine.IsFDADeclared);
		}

		public void TestHasFCCData()
		{
			AssertEquals(false, InvoiceLine.HasFCCData);

			InvoiceLine.FCCs.AddNew();
			AssertEquals(true, InvoiceLine.HasFCCData);
		}

		public void TestHasDOTData()
		{
			AssertEquals(false, InvoiceLine.HasDOTData);

			InvoiceLine.DOTs.AddNew();
			AssertEquals(true, InvoiceLine.HasDOTData);
		}

		public void TestChildrenAreDeletedWhenInvoiceLineIsDeleted()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var nmfsLine = invoiceLine.NMFSLines.AddNew();
			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes._370;
			var aphisHeader = invoiceLine.APHISHeaders.AddNew();
			aphisHeader.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			var fwsHeader = invoiceLine.FWSHeaders.AddNew();
			fwsHeader.US_ProcessingCode = FWSProcessingCodeList.Codes.EDS;
			var amsHeader = invoiceLine.AMSLines.AddNew();
			amsHeader.US_Program = AMSProgramList.Codes.MO1;

			invoiceLine.Delete();
			AssertEquals(true, invoiceLine.IsDeleted);
			AssertEquals(true, nmfsLine.IsDeleted);
			AssertEquals(true, aphisHeader.IsDeleted);
			AssertEquals(true, fwsHeader.IsDeleted);
			AssertEquals(true, amsHeader.IsDeleted);
		}

		public void TestFDARequirementCodeDesc()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1234567890";
			tariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff.UE_OGACodes = "AAAFD4";

			USCTariff tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "1234567891";
			tariff2.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff2.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff2.UE_OGACodes = "AAAFD2";

			USCTariff tariff3 = Factory.New<USCTariff>();
			tariff3.UE_Tariff = "2234567890";
			tariff3.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff3.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff3.UE_PGACodes = "FD4TB2";

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1234567890";
			AssertEquals(OGARequirementList.Descriptions.FD4 + " (" + OGARequirementList.Codes.FD4 + ")", invoiceLine.JI_FDARequirementDesc);

			invoiceLine.JI_Tariff = "1234567891";
			AssertEquals(OGARequirementList.Descriptions.FD2 + " (" + OGARequirementList.Codes.FD2 + ")", invoiceLine.JI_FDARequirementDesc);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableSPN = true;
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = false;
			AssertEquals(true, declaration.IsACEStandalonePNWithoutENSAndCRL);
			AssertEquals(true, invoiceLine.IsACECargoCertificationMode);
			invoiceLine.JI_Tariff = "2234567890";
			AssertEquals(OGARequirementList.Descriptions.FD4 + " (" + OGARequirementList.Codes.FD4 + ")", invoiceLine.JI_FDARequirementDesc);
		}

		public void TestTextileCategoryNumberCopiesFromReferenceFiles()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			InvoiceLine.JI_Tariff = "5210516060";
			AssertEquals("315", InvoiceLine.US_TextileCategoryNo);
		}

		public void TestDefaultDGDetailsOnSubstance()
		{
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "0000";
			subs.DG_Variant = "A";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			subs.DG_PSN = "I am very dangerous";
			subs.DG_FlashPoint = "-4 cc";

			var undg = InvoiceLine.UNDGs.AddNew();
			undg.DI_DG = subs.PK;
			undg.LinkDefault(subs);
			AssertEquals("JI_HazMatCodeQualifier", HazMatQualifierList.Codes.UnitedNations, InvoiceLine.JI_HazMatCodeQualifier);
			AssertEquals("JI_HazMatCode", "0000A", InvoiceLine.JI_HazMatCode);
			AssertEquals("US_HazMatDesc", "I am very dangerous", InvoiceLine.US_HazMatDesc);
		}

		public void TestAntidumpingDutyCase()
		{
			USCACCase uscCase = Factory.New<USCACCase>();
			uscCase.U5_CaseNumber = "A427818000";

			InvoiceLine.US_ADDCaseNo = "A427818000";
			AssertEquals("A427818000", InvoiceLine.AntidumpingDutyCase.U5_CaseNumber);
		}

		public void TestCountervailingDutyCase()
		{
			USCACCase uscCase = Factory.New<USCACCase>();
			uscCase.U5_CaseNumber = "C427819000";

			InvoiceLine.US_CVDCaseNo = "C427819000";
			AssertEquals("C427819000", InvoiceLine.CountervailingDutyCase.U5_CaseNumber);
		}

		public void TestOverrideTaxRateProperties()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				Assert(InvoiceLine.US_TaxRateSInfo.ReadOnly);
				Assert(InvoiceLine.US_R_OrigTaxRateSInfo.ReadOnly);

				InvoiceLine.JI_Tariff = "2204100030";
				Assert(InvoiceLine.US_TaxRateSInfo.ReadOnly);

				InvoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
				Assert(!InvoiceLine.US_TaxRateSInfo.ReadOnly);

				InvoiceLine.US_TaxRateS = AppendixBTaxRateList.Codes.Specify;
				InvoiceLine.US_TaxRate = 0.999m;
				InvoiceLine.JI_Tariff = ZString.Empty;

				Assert(!InvoiceLine.IsTaxRateOverridden);
				AssertEquals(ZString.Empty, InvoiceLine.US_TaxApply);
				Assert(InvoiceLine.US_TaxRateSInfo.ReadOnly);
				AssertEquals(0m, InvoiceLine.US_TaxRate);

				InvoiceLine.US_R_OrigTariff = "2204100030";
				Assert(!InvoiceLine.US_R_OrigTaxRateSInfo.ReadOnly);

				InvoiceLine.US_R_OrigTaxRate = 0.999m;
				InvoiceLine.US_R_OrigTariff = ZString.Empty;

				InvoiceLine.US_R_OrigTaxApply = TaxApplyList.Codes.No;
				AssertEquals(TaxApplyList.Codes.No, InvoiceLine.US_R_OrigTaxApply);
				Assert(InvoiceLine.US_R_OrigTaxRateSInfo.ReadOnly);
				AssertEquals(0m, InvoiceLine.US_R_OrigTaxRate);

				InvoiceLine.US_R_OrigTaxApply = TaxApplyList.Codes.Override;
				AssertEquals(TaxApplyList.Codes.Override, InvoiceLine.US_R_OrigTaxApply);
				Assert(!InvoiceLine.US_R_OrigTaxRateSInfo.ReadOnly);

				InvoiceLine.US_R_OrigTaxApply = "";
				Assert(!InvoiceLine.IsOriginalTaxRateOverridden);
				AssertEquals(ZString.Empty, InvoiceLine.US_R_OrigTaxApply);
				Assert(InvoiceLine.US_R_OrigTaxRateSInfo.ReadOnly);
				AssertEquals(0m, InvoiceLine.US_R_OrigTaxRate);

				InvoiceLine.US_R_OrigTaxApply = TaxApplyList.Codes.Yes;
				AssertEquals(TaxApplyList.Codes.Yes, InvoiceLine.US_R_OrigTaxApply);
				Assert(!InvoiceLine.US_R_OrigTaxRateSInfo.ReadOnly);
				AssertEquals(0m, InvoiceLine.US_R_OrigTaxRate);
			}
		}

		public void TestPopulateUnitOfQuantities()
		{
			var tariffCode = "0000000000";
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = tariffCode;
			tariff.UE_Unit1 = "KG";
			tariff.UE_Unit2 = "M3";
			tariff.UE_Unit3 = "TT";
			tariff.UE_DateFrom = ZDateTime.Today;
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusTariffTypeSB = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.ScheduleB);
			Factory.Save();

			var scheduleB = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, cusTariffTypeSB.PK, tariffCode, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			helper.CreateTariffUOM(scheduleB, "CU1", "LB");
			helper.CreateTariffUOM(scheduleB, "CU2", "C3");

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			InvoiceLine.JI_Tariff = tariffCode;
			AssertEquals("FirstUQ", "KG", InvoiceLine.JI_CustomsUnitQty);
			AssertEquals("SecondUQ", "M3", InvoiceLine.JI_CustomsSecondUnitQty);
			AssertEquals("ThirdUQ", "TT", InvoiceLine.JI_CustomsThirdUnitQty);

			InvoiceLine.JI_Tariff = "";
			CombineAssertions("Defaulted values should not be emptied when selecting an invalid tariff.", () =>
			{
				AssertEquals("FirstUQ", "KG", InvoiceLine.JI_CustomsUnitQty);
				AssertEquals("SecondUQ", "M3", InvoiceLine.JI_CustomsSecondUnitQty);
				AssertEquals("ThirdUQ", "TT", InvoiceLine.JI_CustomsThirdUnitQty);
			});

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			InvoiceLine.JI_Tariff = tariffCode;
			AssertEquals("FirstUQ", "LB", InvoiceLine.JI_CustomsUnitQty);
			AssertEquals("SecondUQ", "C3", InvoiceLine.JI_CustomsSecondUnitQty);
			AssertEquals("ThirdUQ", "", InvoiceLine.JI_CustomsThirdUnitQty);

			var classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "TestLookup";
			classification.CC_ClassificationType = CusClassification.ClassificationType.EXP;
			classification.CC_TariffNum = tariffCode;

			InvoiceLine.JI_CC = classification.PK;
			AssertEquals("InvoiceLine.JI_Tariff", tariffCode, InvoiceLine.JI_Tariff);
			AssertEquals("FirstUQ", "LB", InvoiceLine.JI_CustomsUnitQty);
			AssertEquals("SecondUQ", "C3", InvoiceLine.JI_CustomsSecondUnitQty);
			AssertEquals("ThirdUQ", "", InvoiceLine.JI_CustomsThirdUnitQty);
		}

		public void TestUS_ECCN_Effective()
		{
			InvoiceHeader.US_ECCN = "DEC";
			AssertEquals("US_ECCN_Effective", InvoiceHeader.US_ECCN, InvoiceLine.US_ECCN);
			InvoiceLine.US_ECCN = "INV";
			AssertNotEquals("US_ECCN_Effective", InvoiceHeader.US_ECCN, InvoiceLine.US_ECCN);
			AssertEquals("US_ECCN_Effective", InvoiceLine.US_ECCN, InvoiceLine.US_ECCN);
			InvoiceLine.US_ECCN = "";
			AssertEquals("US_ECCN_Effective", InvoiceHeader.US_ECCN, InvoiceLine.US_ECCN);
		}

		public void TestUS_ExportCode_Effective()
		{
			InvoiceHeader.US_ExportCode = ExportInformationCodeList.Codes.TE;
			AssertEquals("US_ExportCode_Effective", InvoiceHeader.US_ExportCode, InvoiceLine.US_ExportCode);
			InvoiceLine.US_ExportCode = ExportInformationCodeList.Codes.TP;
			AssertNotEquals("US_ExportCode_Effective", InvoiceHeader.US_ExportCode, InvoiceLine.US_ExportCode);
			AssertEquals("US_ExportCode_Effective", InvoiceLine.US_ExportCode, InvoiceLine.US_ExportCode);
			InvoiceLine.US_ExportCode = "";
			AssertEquals("US_ExportCode_Effective", InvoiceHeader.US_ExportCode, InvoiceLine.US_ExportCode);
		}

		public void TestUS_LicenseNo()
		{
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(Declaration.Factory, new ZString[] { USAESLicenseCode.Codes.S00 });

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			InvoiceHeader.US_LicenseType = USAESLicenseCode.Codes.S00;
			AssertEquals("US_LicenseNo", "", InvoiceHeader.US_LicenseNo);
			InvoiceHeader.US_LicenseNo = "DEC123";
			AssertEquals("US_LicenseNo", "DEC123", InvoiceHeader.US_LicenseNo);
			InvoiceLine.US_LicenseType = USAESLicenseCode.Codes.S00;
			AssertEquals("US_LicenseNo", "", InvoiceLine.US_LicenseNo);
			AssertNotEquals("US_LicenseNo", InvoiceHeader.US_LicenseNo, InvoiceLine.US_LicenseNo);

			InvoiceLine.US_LicenseType = USAESLicenseCode.Codes.C33;
			InvoiceHeader.US_LicenseType = USAESLicenseCode.Codes.C33;
			AssertEquals("US_LicenseNo", InvoiceHeader.US_LicenseNo, InvoiceLine.US_LicenseNo);
			InvoiceLine.US_LicenseNo = "INV123";
			AssertNotEquals("US_LicenseNo", InvoiceHeader.US_LicenseNo, InvoiceLine.US_LicenseNo);

			InvoiceLine.US_LicenseNo = "";
			AssertEquals("US_LicenseNo", InvoiceHeader.US_LicenseNo, InvoiceLine.US_LicenseNo);
		}

		public override void TestEffectiveCountryOfOrigin()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			base.TestEffectiveCountryOfOrigin();
		}

		public override void TestMergedLineNumber()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			base.TestMergedLineNumber();
		}

		public void TestUS_LicenseType()
		{
			InvoiceLine.US_LicenseType = USAESLicenseCode.Codes.S00;
			InvoiceHeader.US_LicenseType = USAESLicenseCode.Codes.S00;
			AssertEquals("US_LicenseType", InvoiceHeader.US_LicenseType, InvoiceLine.US_LicenseType);

			InvoiceLine.US_LicenseType = USAESLicenseCode.Codes.OPA;
			AssertNotEquals("US_LicenseType", InvoiceHeader.US_LicenseType, InvoiceLine.US_LicenseType);
			InvoiceLine.US_LicenseType = "";
			AssertEquals("US_LicenseType", InvoiceHeader.US_LicenseType, InvoiceLine.US_LicenseType);

			InvoiceHeader.US_LicenseType = USAESLicenseCode.Codes.OPA;
			AssertEquals("US_LicenseType", InvoiceHeader.US_LicenseType, InvoiceLine.US_LicenseType);
			InvoiceLine.US_LicenseType = USAESLicenseCode.Codes.C33;
			AssertNotEquals("US_LicenseType", InvoiceHeader.US_LicenseType, InvoiceLine.US_LicenseType);

			InvoiceLine.US_LicenseType = "";
			AssertEquals("US_LicenseType", InvoiceHeader.US_LicenseType, InvoiceLine.US_LicenseType);
		}

		public void TestUS_SecondarySPI()
		{
			InvoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.F;
			AssertEquals(SecondarySpecProgIndicatorList.Codes.F, InvoiceLine.US_SecondarySPI);

			InvoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.G;
			AssertEquals(SecondarySpecProgIndicatorList.Codes.G, InvoiceLine.US_SecondarySPI);
		}

		public void TestTariff()
		{
			ZString tariffCode = "0000000000";
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = tariffCode;
			tariff.UE_ShortDescription = "Short Description";
			tariff.UE_DateFrom = ZDateTime.Today;
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			InvoiceLine.JI_Tariff = "0000.00.00 00";
			AssertEquals("tariff retrieved", tariff, InvoiceLine.ImportTariff);

			var helper = new UniversalReferenceTestDataHelper(Factory);

			var cusTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.ScheduleB);
			Factory.Save();

			var scheduleB = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, cusTariffType.PK, tariffCode, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("tariff retrieved", scheduleB, InvoiceLine.ScheduleBTariff);

			cusTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.Export);
			Factory.Save();

			var exportTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, cusTariffType.PK, tariffCode, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			AssertEquals("tariff retrieved", exportTariff, InvoiceLine.ExportTariff);
		}

		public void TestIsSoftwoodLumberAgreementTariff()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceLine.JI_Tariff = "0000.00.00 00";
			AssertEquals(false, InvoiceLine.IsSoftwoodLumberAgreementTariff);

			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "44091020";
			tariff.UE_PermitLicenseIndicator = MiscellaneousPermitLicenseList.Codes.CanadaSoftwoodLumberExportNumber;
			tariff.UE_DateFrom = ZDateTime.Today;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);

			InvoiceLine.JI_Tariff = "44091020";
			AssertEquals(true, InvoiceLine.IsSoftwoodLumberAgreementTariff);

			tariff.UE_Tariff = "11111110";
			tariff.UE_PermitLicenseIndicator = "";
			InvoiceLine.JI_Tariff = "11111110";
			AssertEquals(false, InvoiceLine.IsSoftwoodLumberAgreementTariff);
		}

		public void TestDeleteWillDeleteChildrenAndPartRelatedLines()
		{
			JobComInvoiceLine childInvoiceLine = InvoiceLine.AddSecondaryInvoiceLine();
			JobComInvoiceLine partRelatedInvoiceLine = InvoiceLine.AddProductRelatedInvoiceLine();

			InvoiceLine.Delete();
			AssertEquals(true, childInvoiceLine.IsDeleted);
			AssertEquals(true, partRelatedInvoiceLine.IsDeleted);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1044:DoNotUseDatabaseCount", Justification = "Testing")]
		public void TestDeleteOGAAgencyRequirements()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var reqCollection = invoiceLine.OGAAgencyRequirements;
			Assert(reqCollection.Count > 0);
			invoiceLine.Delete();
			AssertEquals(0, reqCollection.Count);
		}

		public void TestDeleteExportPGAAgencyRequirements()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var exportPGAReqCollection = invoiceLine.ExportPGAAgencyRequirements;
			AssertNotEquals(0, exportPGAReqCollection.Count);

			invoiceLine.Delete();
			AssertEquals(0, exportPGAReqCollection.Count);
		}

		public void TestUS_VehicleIDInfoReadOnly()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			InvoiceLine.US_IsUsedVehicle = false;
			AssertEquals("US_VehicleIDInfo.ReadOnly", true, InvoiceLine.US_VehicleIDInfo.ReadOnly);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertEquals("US_VehicleIDInfo.ReadOnly", false, InvoiceLine.US_VehicleIDInfo.ReadOnly);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			InvoiceLine.US_IsUsedVehicle = true;
			AssertEquals("US_VehicleIDInfo.ReadOnly", false, InvoiceLine.US_VehicleIDInfo.ReadOnly);
		}

		public void TestUS_VehicleIDTypeInfoOnly()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			InvoiceLine.US_IsUsedVehicle = false;
			AssertEquals("US_VehicleIDTypeInfo.ReadOnly", true, InvoiceLine.US_VehicleIDTypeInfo.ReadOnly);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertEquals("US_VehicleIDTypeInfo.ReadOnly", false, InvoiceLine.US_VehicleIDTypeInfo.ReadOnly);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			InvoiceLine.US_IsUsedVehicle = true;
			AssertEquals("US_VehicleIDTypeInfo.ReadOnly", false, InvoiceLine.US_VehicleIDTypeInfo.ReadOnly);
		}

		public void TestUS_VehicleTitleNoInfoOnly()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			InvoiceLine.US_IsUsedVehicle = false;
			AssertEquals("US_VehicleTitleNoInfo.ReadOnly", true, InvoiceLine.US_VehicleTitleNoInfo.ReadOnly);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertEquals("US_VehicleTitleNoInfo.ReadOnly", false, InvoiceLine.US_VehicleTitleNoInfo.ReadOnly);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			InvoiceLine.US_IsUsedVehicle = true;
			AssertEquals("US_VehicleTitleNoInfo.ReadOnly", false, InvoiceLine.US_VehicleTitleNoInfo.ReadOnly);
		}

		public void TestUS_VehicleTitleStateInfoOnly()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			InvoiceLine.US_IsUsedVehicle = false;
			AssertEquals("US_VehicleTitleStateInfo.ReadOnly", true, InvoiceLine.US_VehicleTitleStateInfo.ReadOnly);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertEquals("US_VehicleTitleStateInfo.ReadOnly", false, InvoiceLine.US_VehicleTitleStateInfo.ReadOnly);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			InvoiceLine.US_IsUsedVehicle = true;
			AssertEquals("US_VehicleTitleStateInfo.ReadOnly", false, InvoiceLine.US_VehicleTitleStateInfo.ReadOnly);
		}

		[TestDate(2019, 4, 8)]
		public void TestExportCanUseHTSInsteadOfScheduleB()
		{
			ZString tariffCode = "0000000000";
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = tariffCode;
			tariff.UE_DateFrom = ZDateTime.Today;
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			var cusTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.Export);
			Factory.Save();
			var tariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, cusTariffType.PK, tariffCode, startDate, endDate);

			var cusTariffTypeSB = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.ScheduleB);
			Factory.Save();
			var scheduleB = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, cusTariffTypeSB.PK, tariffCode, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.US_TariffType = TariffTypeList.Codes.ScheduleB;
			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_MessageType = JobMessageTypeList.Codes.Export;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariffCode;
			AssertNull("PreCondition: Declaration must be null", invoiceLine.Declaration);
			AssertEquals("UseScheduleB", false, invoiceLine.UseScheduleB);
			AssertEquals("ExportTariff", tariff2, invoiceLine.ExportTariff);

			invoiceLine.JI_JZ = InvoiceHeader.PK;
			AssertNotNull("PreCondition: Declaration must be not null", invoiceLine.Declaration);
			AssertEquals("UseScheduleB", true, invoiceLine.UseScheduleB);
			AssertEquals("ScheduleBTariff", scheduleB, invoiceLine.ScheduleBTariff);

			Declaration.US_TariffType = TariffTypeList.Codes.HTS;
			Declaration.US_DateOfExport = new ZDateTime(2018, 1, 11);
			var tariff3 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, cusTariffType.PK, tariffCode, new ZDateTime(2000, 1, 1), new ZDateTime(2018, 12, 31));
			invoiceLine.JI_Tariff = tariffCode;
			AssertEquals("ExportTariff", tariff3, invoiceLine.ExportTariff);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertEquals("UseScheduleB", false, invoiceLine.UseScheduleB);
			AssertEquals("ImportTariff", tariff, invoiceLine.ImportTariff);
		}

		public void TestIsExportDeclaration()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			JobComInvoiceLine invoiceLine = Factory.New<JobComInvoiceLine>();
			AssertNull("PreCondition: Declaration must be null", invoiceLine.Declaration);
			AssertEquals("IsExportDeclaration", false, invoiceLine.IsExport);
			invoiceLine.JI_JZ = InvoiceHeader.PK;
			AssertNotNull("PreCondition: Declaration must be not null", invoiceLine.Declaration);
			AssertEquals("IsExportDeclaration", true, invoiceLine.IsExport);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertEquals("IsExportDeclaration", false, invoiceLine.IsExport);
		}

		public void TestIHazardousMaterial()
		{
			var invoiceLine = Declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "0000";
			subs.DG_Variant = "A";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			subs.DG_PSN = "Proper Shipping Name";
			subs.DG_FlashPoint = "";

			var contact = Factory.New<OrgContact>();
			contact.OC_ContactName = "Contact name";

			var dgItem = invoiceLine.UNDGs.AddNew();
			dgItem.DI_DG = subs.PK;
			dgItem.LinkDefault(subs);
			dgItem.DI_OC_DGContact = contact.PK;
			dgItem.DI_DGFlashPoint = 0m;

			invoiceLine.JI_HazMatCode = "AAAA";
			invoiceLine.JI_HazMatCodeQualifier = HazMatQualifierList.Codes.UnitedNations;
			invoiceLine.US_HazMatDesc = "Desc";
			invoiceLine.US_HazMatClassDesc = "Class desc";

			IHazardousMaterial line = invoiceLine;
			AssertEquals(true, line.IsHazRelevant);
			AssertEquals(false, line.IsFlashPointTempRelevant);
			AssertEquals("AAAA", line.HazMatCode);
			AssertEquals("0000", line.HazMatClass);
			AssertEquals(HazMatQualifierList.Codes.UnitedNations, line.HazMatQualifier);
			AssertEquals("Desc", line.HazMatDesc);
			AssertEquals("Contact name", line.ContactName);
			AssertEquals(0m, line.FlashPointTemp);
			AssertEquals("Class desc", line.HazMatClassificationDesc);

			dgItem.Substance.DG_FlashPoint = "-23";
			dgItem.DI_DGFlashPoint = -23m;
			AssertEquals(true, line.IsFlashPointTempRelevant);
			AssertEquals(-23m, line.FlashPointTemp);
		}

		public void TestGetAdditionalDataForBorderWise()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			var invoiceLineForBorderWise = InvoiceLine as IHaveAdditionalDataForBorderWise;
			var additionalData = invoiceLineForBorderWise.GetAdditionalDataForBorderWise("");
			AssertEquals("AdditionalData.ParameterForBorderWise", "I", additionalData.ParameterForBorderWise);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			additionalData = invoiceLineForBorderWise.GetAdditionalDataForBorderWise("");
			AssertEquals("AdditionalData.ParameterForBorderWise", "E", additionalData.ParameterForBorderWise);

			Declaration.US_TariffType = TariffTypeList.Codes.HTS;
			additionalData = invoiceLineForBorderWise.GetAdditionalDataForBorderWise("");
			AssertEquals("AdditionalData.ParameterForBorderWise", "I", additionalData.ParameterForBorderWise);
			additionalData = invoiceLineForBorderWise.GetAdditionalDataForBorderWise("US_ExportTariff");
			AssertEquals("AdditionalData.ParameterForBorderWise still I", "I", additionalData.ParameterForBorderWise);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			additionalData = invoiceLineForBorderWise.GetAdditionalDataForBorderWise("US_FormattedExportTariff");
			AssertEquals("AdditionalData.ParameterForBorderWise E for drawback US_ExportTariff", "E", additionalData.ParameterForBorderWise);
			additionalData = invoiceLineForBorderWise.GetAdditionalDataForBorderWise("JI_Tariff");
			AssertEquals("AdditionalData.ParameterForBorderWise I for JI_Traiff", "I", additionalData.ParameterForBorderWise);
		}

		public void TestIssue00851871()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var invoiceLineForBorderWise = invoiceLine as IHaveAdditionalDataForBorderWise;
			var additionalData = invoiceLineForBorderWise.GetAdditionalDataForBorderWise("");
			AssertEquals("AdditionalData.ParameterForBorderWise", "I", additionalData.ParameterForBorderWise);
		}

		public void TestCalculatedEntryLineData()
		{
			DeclarationTestHelper helper = new DeclarationTestHelper(Factory);
			JobDeclaration declaration = helper.CreateSeaExportDeclaration();

			JobComInvoiceLine invoiceLine = Factory.New<JobComInvoiceLine>();
			AssertNull("CusEntryLine", invoiceLine.CusEntryLine);
			AssertEquals("JI_Calc_EntryNumber", "", invoiceLine.JI_Calc_EntryNumber);
			AssertEquals("JI_Calc_MergedLineNumber", "Not Merged", invoiceLine.JI_Calc_MergedLineNumber);
			AssertEquals("JI_Calc_XTN", "", invoiceLine.JI_Calc_XTN);

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			AssertEquals("declaration.CustomsEntryHeaders.Count", 1, declaration.CustomsEntryHeaders.Count);
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.EntryNumber = "ITN1234";
			entryHeader.US_XTN = "XTN1234";

			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			entryLine.CL_LineNumber = 3;
			AssertEquals(entryLine.PK, invoiceLine.CusEntryLine.PK);
			AssertEquals("JI_Calc_EntryNumber", entryLine.CL_Calc_EntryNumber, invoiceLine.JI_Calc_EntryNumber);
			AssertEquals("JI_Calc_MergedLineNumber", entryLine.CL_LineNumberFormatted, invoiceLine.JI_Calc_MergedLineNumber);
			AssertEquals("JI_Calc_XTN", entryLine.CL_Calc_XTN, invoiceLine.JI_Calc_XTN);
		}

		public void TestIsLimitedReportingExportCode()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("IsExportDeclaration", true, InvoiceLine.IsExport);
			InvoiceLine.US_ExportCode = "";
			AssertEquals("IsLimitedReportingExportCode", false, InvoiceLine.IsLimitedReportingExportCode);
			CodeDescriptionPairList list = InvoiceLine.Lookups.LimitedReportingExportCodeList;
			foreach (CodeDescriptionPair pair in list)
			{
				InvoiceLine.US_ExportCode = pair.Code;
				AssertEquals("IsLimitedReportingExportCode", true, InvoiceLine.IsLimitedReportingExportCode);
			}

			InvoiceLine.US_ExportCode = "Z#";
			AssertEquals("IsLimitedReportingExportCode", false, InvoiceLine.IsLimitedReportingExportCode);

			InvoiceLine.US_ExportCode = LimitedReportingExportInformationCodeList.Codes.DD;
			AssertEquals("IsLimitedReportingExportCode", true, InvoiceLine.IsLimitedReportingExportCode);
		}

		public void TestCopyOGAsFromProduct()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Test";

			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = importer.PK;
			relOrg.OU_Relationship = "OWN";

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = USCTariff.CottonFeeApplicable;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			pivot.CD_ACEFDAIndicator = OGAIndicatorList.Codes.Declared;
			var aceFDAOnProduct = pivot.ACEFDAs.AddNew();
			aceFDAOnProduct.US_ProductCode = "123456";
			aceFDAOnProduct.US_Qty1 = 5m;

			invoiceLine.JI_PartNo = "Test";
			invoiceLine.JI_InvoiceQuantity = 1000m;
			invoiceLine.JI_InvoiceUQ = "KG";
			AssertEquals("invoiceLine.FDAs.Count", 0, invoiceLine.FDAs.Count);
			AssertEquals("invoiceLine.ACE_FDALines.Count", 1, invoiceLine.ACE_FDALines.Count);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertEquals("invoiceLine.FDAs.Count", 0, invoiceLine.FDAs.Count);
			AssertEquals("invoiceLine.ACE_FDALines.Count", 0, invoiceLine.ACE_FDALines.Count);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			AssertEquals("invoiceLine.FDAs.Count", 0, invoiceLine.FDAs.Count);
			AssertEquals("invoiceLine.ACE_FDALines.Count", 0, invoiceLine.ACE_FDALines.Count);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_PartNo = "Test";
			AssertEquals("invoiceLine2.ACE_FDALines.Count", 1, invoiceLine2.ACE_FDALines.Count);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			AssertEquals("invoiceLine2.FDAs.Count", 0, invoiceLine2.FDAs.Count);
			AssertEquals("invoiceLine2.ACE_FDALines.Count", 1, invoiceLine2.ACE_FDALines.Count);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertEquals("invoiceLine2.FDAs.Count", 0, invoiceLine2.FDAs.Count);
			AssertEquals("invoiceLine2.ACE_FDALines.Count", 0, invoiceLine2.ACE_FDALines.Count);
		}

		public void TestCopyPGAFDADocAddressDetailsFromProduct()
		{
			using (DataRegistry.Business.USCustomsDataRegistry.Instance.EnableDocAddressForFDA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var importer = Factory.NewWithValidTestData<OrgHeader>();
				var product = Factory.New<OrgSupplierPart>();
				product.OP_PartNum = "PET";
				product.OP_Desc = "PET FOOD";

				var relOrg = product.RelatedOrganisations.AddNew();
				relOrg.OU_OH = importer.PK;
				relOrg.OU_Relationship = "OWN";

				var pivot = product.PivotsForBinding.AddNew();
				pivot.CI_TariffNum = USCTariff.CottonFeeApplicable;
				pivot.CD_ACEFDAIndicator = OGAIndicatorList.Codes.Declared;
				var pivotFda = pivot.ACEFDAs.AddNew();
				pivotFda.US_ProgramCode = "DEV";

				var org1 = Factory.New<OrgHeader>();
				org1.OH_Code = "ABC";
				org1.OH_FullName = "ABC INC.";
				org1.MainAddress.Address1 = "ABC AVENUE";
				pivotFda.DocAddresses.AddNew(org1.MainAddress, DocAddressType.FSVPImporter);

				var org2 = pivotFda.DocAddresses.AddNew(DocAddressType.Manufacturer);
				org2.E2_AddressOverride = true;
				org2.E2_CompanyName = "XYZ INC.";
				org2.E2_Address1 = "XYZ AVENUE";

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_OH_Importer = importer.PK;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_PartNo = "PET";

				var lineFda = invoiceLine.ACE_FDALines[0];
				AssertEquals(2, lineFda.DocAddresses.Count);

				var lineOrg1 = lineFda.DocAddresses.FindByDocAddressType(DocAddressType.FSVPImporter);
				AssertEquals(org1.MainAddress.PK, lineOrg1.E2_OA_Address);

				var lineOrg2 = lineFda.DocAddresses.FindByDocAddressType(DocAddressType.Manufacturer);
				AssertEquals(org2.AddressAsASingleLine, lineOrg2.AddressAsASingleLine);
			}
		}

		public void TestCopyPGAFDAFromProduct()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Test";

			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = importer.PK;
			relOrg.OU_Relationship = "OWN";

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = USCTariff.CottonFeeApplicable;
			pivot.CD_ACEFDAIndicator = OGAIndicatorList.Codes.Declared;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var fdaOnProduct = pivot.ACEFDAs.AddNew();
			fdaOnProduct.US_BrandName = "BRD NAME";

			var affcode = fdaOnProduct.AffirmationCodes.AddNew();
			affcode.CY_Code = "ABC";

			fdaOnProduct.Lots.RemoveAndDeleteAll();
			var lot = fdaOnProduct.Lots.AddNew();
			lot.US_DegreeType = "C";

			var constituentElements = fdaOnProduct.ProductConstituentElements.AddNew();
			constituentElements.US_SpeciesName = "SP";
			constituentElements.US_PGAPercentOfConstituentElement = 88.4444m;

			var pga = pivot.PGAs.AddNew();
			var constituentElement = pga.PG04ConstituentElements.AddNew();
			constituentElement.US_GenusName = "ABC";
			constituentElement.US_UnknownBreakdownCountryCode = "US";
			constituentElement.US_SpeciesName = "SP";
			constituentElement.US_PGAPercentOfConstituentElement = 88.4444m;

			invoiceLine.JI_PartNo = "Test";
			invoiceLine.JI_InvoiceQuantity = 1000m;
			invoiceLine.JI_InvoiceUQ = "KG";
			AssertEquals(invoiceLine.US_FDAIndicator, OGAIndicatorList.Codes.Declared);
			AssertEquals(invoiceLine.ACE_FDALines.Count, 1);

			var newFDA = invoiceLine.ACE_FDALines[0];

			AssertEquals(newFDA.AffirmationCodes.Count, 1);
			AssertEquals(newFDA.AffirmationCodes[0].CY_Code, "ABC");

			AssertEquals(newFDA.Lots.Count, 1);
			AssertEquals(newFDA.Lots[0].US_DegreeType, "C");

			AssertEquals(newFDA.ProductConstituentElements.Count, 1);
			AssertEquals(newFDA.ProductConstituentElements[0].US_SpeciesName, "SP");
			AssertEquals(newFDA.ProductConstituentElements[0].US_PGAPercentOfConstituentElement, 88.4444m);
		}

		public void TestCopyFDAWhenACSWith3DecimalPlace()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Test";

			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = importer.PK;
			relOrg.OU_Relationship = "OWN";

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = USCTariff.CottonFeeApplicable;
			pivot.CD_LaceyActIndicator = OGAIndicatorList.Codes.Declared;
			var pivotPGA = pivot.PGAs.AddNew();
			pivotPGA.US_PGACommercialDescription = "DESC";
			var pivotConsEle = pivotPGA.PG04ConstituentElements.AddNew();
			pivotConsEle.US_PGAPercentOfConstituentElement = 88.4444m;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			var fdaOnProduct = pivot.ACEFDAs.AddNew();
			fdaOnProduct.US_BrandName = "BRD NAME";

			invoiceLine1.JI_PartNo = "Test";
			AssertEquals(invoiceLine1.LaceyActLines.Count, 1);
			AssertEquals(invoiceLine1.LaceyActLines[0].PG04ConstituentElements.Count, 1);
			AssertEquals(invoiceLine1.LaceyActLines[0].PG04ConstituentElements[0].US_PGAPercentOfConstituentElement, 88.444m);
		}

		public void TestProductAutoGenerationDisclaimReasonShowing_Import()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;

			var invoice = declaration.Invoices.AddNew();
			var line = invoice.InvoiceLines.AddNew();
			line.JI_Tariff = "10123";

			line.US_LaceyIndicator = OGAIndicatorList.Codes.Disclaimed;
			line.US_LaceyDisclaimReason = PGADisclaimReasonList.Codes.A;

			line.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			line.US_FDADisclaimReason = PGADisclaimReasonList.Codes.B;

			line.US_NHTSAIndicator = OGAIndicatorList.Codes.Disclaimed;
			line.US_NHTDisclaimReason = PGADisclaimReasonList.Codes.C;

			line.US_ATFInd = OGAIndicatorList.Codes.Declared;

			line.US_ODSInd = OGAIndicatorList.Codes.Disclaimed;
			line.US_ODSDisclaimReason = PGADisclaimReasonList.Codes.D;

			line.US_TSCAInd = OGAIndicatorList.Codes.Disclaimed;
			line.US_TSCADisclaimReason = PGADisclaimReasonList.Codes.A;

			line.US_PSTIndicator = OGAIndicatorList.Codes.Disclaimed;
			line.US_PSTDisclaimReason = PGADisclaimReasonList.Codes.B;
			line.US_PSTDisclaimProgram = PSTProductTypeList.Codes.PS2;

			line.US_OMCInd = OGAIndicatorList.Codes.Disclaimed;
			line.US_OMCDisclaimReason = PGADisclaimReasonList.Codes.C;

			line.US_VNEInd = OGAIndicatorList.Codes.Disclaimed;
			line.US_VNEDisclaimReason = PGADisclaimReasonList.Codes.D;

			line.US_AMSInd = OGAIndicatorList.Codes.Disclaimed;
			line.US_AMSDisclaimReason = PGADisclaimReasonList.Codes.A;
			line.US_AMSDisclaimProgram = AMSProgramList.Codes.MO8;

			line.US_NOPInd = OGAIndicatorList.Codes.Disclaimed;
			line.US_NOPDisclaimReason = PGADisclaimReasonList.Codes.A;

			line.US_TTBInd = OGAIndicatorList.Codes.Disclaimed;
			line.US_TTBDisclaimReason = PGADisclaimReasonList.Codes.B;

			line.US_CPSCInd = OGAIndicatorList.Codes.Disclaimed;
			line.US_CPSCDisclaimReason = PGADisclaimReasonList.Codes.C;

			line.US_DEAInd = OGAIndicatorList.Codes.Disclaimed;
			line.US_DEADisclaimReason = PGADisclaimReasonList.Codes.D;

			line.US_APHISInd = OGAIndicatorList.Codes.Disclaimed;
			line.US_APHISDisclaimReason = PGADisclaimReasonList.Codes.A;

			line.US_DDTCInd = OGAIndicatorList.Codes.Declared;

			line.US_NMFS370Ind = OGAIndicatorList.Codes.Disclaimed;
			line.US_NMFS370DisclaimReason = PGADisclaimReasonList.Codes.B;

			line.US_NMFSAMRInd = OGAIndicatorList.Codes.Disclaimed;
			line.US_NMFSAMRDisclaimReason = PGADisclaimReasonList.Codes.C;

			line.US_NMFSHMSInd = OGAIndicatorList.Codes.Disclaimed;
			line.US_NMFSHMSDisclaimReason = PGADisclaimReasonList.Codes.D;

			line.US_NMFSSIMPInd = OGAIndicatorList.Codes.Declared;

			line.US_FWSInd = OGAIndicatorList.Codes.Disclaimed;
			line.US_FWSDisclaimReason = PGADisclaimReasonList.Codes.A;

			var part = new OrgSupplierPartCollection(Factory, line, line.InvoiceHeader?.IsExport ?? ZBool.False).AddNew();
			AssertEquals(1, part.PivotsForBinding.Count);
			var pivot = part.PivotsForBinding[0];

			AssertEquals(OGAIndicatorList.Codes.Disclaimed, pivot.CD_LaceyActIndicator);
			AssertEquals(PGADisclaimReasonList.Codes.A, pivot.CD_LaceyActDisclaimReason);

			AssertEquals(OGAIndicatorList.Codes.Disclaimed, pivot.CD_ACEFDAIndicator);
			AssertEquals(PGADisclaimReasonList.Codes.B, pivot.CD_ACEFDADisclaimReason);

			AssertEquals(OGAIndicatorList.Codes.Disclaimed, pivot.CD_NHTSAIndicator);
			AssertEquals(PGADisclaimReasonList.Codes.C, pivot.CD_NHTSADisclaimReason);

			AssertEquals(OGAIndicatorList.Codes.Declared, pivot.CD_ATFIndicator);

			AssertEquals(OGAIndicatorList.Codes.Disclaimed, pivot.CD_ODSIndicator);
			AssertEquals(PGADisclaimReasonList.Codes.D, pivot.CD_ODSDisclaimReason);

			AssertEquals(OGAIndicatorList.Codes.Disclaimed, pivot.CD_TSCAClaimIndicator);
			AssertEquals(PGADisclaimReasonList.Codes.A, pivot.CD_TSCADisclaimReason);

			AssertEquals(OGAIndicatorList.Codes.Disclaimed, pivot.CD_PSTIndicator);
			AssertEquals(PGADisclaimReasonList.Codes.B, pivot.CD_PSTDisclaimReason);
			AssertEquals(PSTProductTypeList.Codes.PS2, pivot.CD_PSTDisclaimProgram);

			AssertEquals(OGAIndicatorList.Codes.Disclaimed, pivot.CD_OMCIndicator);
			AssertEquals(PGADisclaimReasonList.Codes.C, pivot.CD_OMCDisclaimReason);

			AssertEquals(OGAIndicatorList.Codes.Disclaimed, pivot.CD_VNEIndicator);
			AssertEquals(PGADisclaimReasonList.Codes.D, pivot.CD_VNEDisclaimReason);

			AssertEquals(OGAIndicatorList.Codes.Disclaimed, pivot.Details.CD_AMSIndicator);
			AssertEquals(PGADisclaimReasonList.Codes.A, pivot.Details.CD_AMSDisclaimReason);
			AssertEquals(AMSProgramList.Codes.MO8, pivot.Details.CD_AMSDisclaimProgram);

			AssertEquals(OGAIndicatorList.Codes.Disclaimed, pivot.Details.CD_NOPIndicator);
			AssertEquals(PGADisclaimReasonList.Codes.A, pivot.Details.CD_NOPDisclaimReason);

			AssertEquals(OGAIndicatorList.Codes.Disclaimed, pivot.CD_TTBIndicator);
			AssertEquals(PGADisclaimReasonList.Codes.B, pivot.CD_TTBDisclaimReason);

			AssertEquals(OGAIndicatorList.Codes.Disclaimed, pivot.CD_CPSCIndicator);
			AssertEquals(PGADisclaimReasonList.Codes.C, pivot.CD_CPSCDisclaimReason);

			AssertEquals(OGAIndicatorList.Codes.Disclaimed, pivot.CD_DEAIndicator);
			AssertEquals(PGADisclaimReasonList.Codes.D, pivot.CD_DEADisclaimReason);

			AssertEquals(OGAIndicatorList.Codes.Disclaimed, pivot.CD_APHISIndicator);
			AssertEquals(PGADisclaimReasonList.Codes.A, pivot.CD_APHISDisclaimReason);

			AssertEquals(OGAIndicatorList.Codes.Declared, pivot.Details.CD_DDTCIndicator);

			AssertEquals(OGAIndicatorList.Codes.Disclaimed, pivot.Details.CD_NMFS370Indicator);
			AssertEquals(PGADisclaimReasonList.Codes.B, pivot.Details.CD_NMFS370DisclaimReason);

			AssertEquals(OGAIndicatorList.Codes.Disclaimed, pivot.Details.CD_NMFSAMRIndicator);
			AssertEquals(PGADisclaimReasonList.Codes.C, pivot.Details.CD_NMFSAMRDisclaimReason);

			AssertEquals(OGAIndicatorList.Codes.Disclaimed, pivot.Details.CD_NMFSHMSIndicator);
			AssertEquals(PGADisclaimReasonList.Codes.D, pivot.Details.CD_NMFSHMSDisclaimReason);

			AssertEquals(OGAIndicatorList.Codes.Declared, pivot.Details.CD_NMFSSIMPIndicator);

			AssertEquals(OGAIndicatorList.Codes.Disclaimed, pivot.Details.CD_FWSIndicator);
			AssertEquals(PGADisclaimReasonList.Codes.A, pivot.Details.CD_FWSDisclaimReason);
		}

		public void TestProductCopiesFromInvoiceLine_Import()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;

			var invoice = declaration.Invoices.AddNew();
			var line = invoice.InvoiceLines.AddNew();
			line.JI_Tariff = "10123";
			line.US_9802PerUnit = 9802m;
			line.US_98InvCurrPerUnit = 98m;
			line.US_ADD_NA = true;
			line.US_ADDCaseNo = "ADDCaseNo";
			line.US_AgricultureLicNo = "Agricultur";
			line.US_CAExportCertificate = "CAExport";
			line.US_CBTPACertificateNo = "CBTPACert";
			line.US_CVD_NA = true;
			line.US_CVDDepositRateIndicator = "c";
			line.US_CottonCertificateNo = "CottonCer";
			line.US_CottonFeeExempt = "F";
			line.US_IsNAFTANet = true;
			line.US_IsBondedADD = true;
			line.US_MiscPermitNo = "MiscPermi";
			line.JI_OA_ManufacturerAddress = ZGuid.BrettsGuid;
			line.JI_OA_ExporterAddress = ZGuid.BrettsGuid;
			line.US_PIRPRulingNo = "PIRPRu";
			line.US_PIRPRulingType = "P";
			line.US_SPI = "SPI";
			line.US_SecondarySPI = "S";
			line.US_TSCAIndicator = "T";
			line.US_TaxApply = "Y";
			line.US_TaxCode = "Tax";
			line.US_TaxRateS = "TaxRateS";
			line.US_UC_NKCountryOfExport = "EX";
			line.US_WoolLicenceNo = "WoolLicen";
			line.US_ZoneStatus = "Z";
			line.US_ProductExclusion = "PE";
			line.US_ExclusionNumber = "Exclusion";
			line.US_AMMVPercentage = 10m;
			line.US_Prim_NA = true;
			line.US_RN_NKPrimCtry = "RU";
			line.US_Sec_NA = true;
			line.US_RN_NKSecCtry = "CN";
			line.US_RN_NKCastCtry = "SG";
			line.US_RN_NKCertOrigin = "SG";
			line.US_RN_NKMeltCtry = "SG";

			var part = (line.Lookups.PartsList as OrgSupplierPartCollection).AddNew();
			var pivot = part.PivotsForBinding[0];

			CombineAssertions(() =>
			{
				AssertEquals("9802PerUnit", 9802m, pivot.CD_9802USDValuePerUnit);
				AssertEquals("98InvCurrPerUnit", 98m, pivot.CD_9802ValuePerUnit);
				AssertEquals("ADD_NA", true, pivot.CD_ADDApplicable);
				AssertEquals("ADDCaseNo", pivot.CD_ADDCaseNo);
				AssertEquals("AgricultureLicNo", "Agricultur", pivot.CD_AgricultureLicenceNo);
				AssertEquals("CAExportCertificate", "CAExport", pivot.CD_SugarCertificate);
				AssertEquals("CBTPACertificateNo", "CBTPACert", pivot.CD_CBTPACertificate);
				AssertEquals("CVD_NA", true, pivot.CD_CVDApplicable);
				AssertEquals("CVDDepositRateIndicator", "c", pivot.CD_CVDDepositRateInd);
				AssertEquals("CottonCertificateNo", "CottonCer", pivot.CD_CottonCertificate);
				AssertEquals("CottonFeeExempt", "F", pivot.CD_CottonFeeExempt);
				AssertEquals("IsNAFTANet", true, pivot.CD_NAFTANetCost);
				AssertEquals("IsBondedADD", true, pivot.CD_ADDBonded);
				AssertEquals("MiscPermitNo", "MiscPermi", pivot.CD_MiscLicenceNo);
				AssertEquals("ManufacturerAddress", ZGuid.BrettsGuid, pivot.CD_OA_Manufacturer);
				AssertEquals("ExporterAddress", ZGuid.BrettsGuid, pivot.CD_OA_Exporter);
				AssertEquals("PIRPRulingNo", "PIRPRu", pivot.CD_RulingNumber);
				AssertEquals("PIRPRulingType", "P", pivot.CD_RulingType);
				AssertEquals("SPI", pivot.CD_SPI);
				AssertEquals("SecondarySPI", "S", pivot.CD_ProductClaim);
				AssertEquals("TaxApply", "Y", pivot.CD_TaxApplicability);
				AssertEquals("TaxCode", "Tax", pivot.CD_TaxCode);
				AssertEquals("TaxRateS", pivot.CD_TaxRateDesc);
				AssertEquals("CountryOfExport", "EX", pivot.CD_UC_NKCountryOfExport);
				AssertEquals("WoolLicenceNo", "WoolLicen", pivot.CD_WoolLicenceNo);
				AssertEquals("ZoneStatus", "Z", pivot.CD_ZoneStatus);
				AssertEquals("ProductExclusion", "PE", pivot.CD_ProductExclusion);
				AssertEquals("ExclusionNumber", "Exclusion", pivot.CD_ExclusionNumber);
				AssertEquals("AMMVPercentage", 10m, pivot.CD_AMMVPercentage);
				line.US_AMMVPerUnit = 20m;
				AssertEquals("AMMVPerUnit", 20m, (line.Lookups.PartsList as OrgSupplierPartCollection).AddNew().PivotsForBinding[0].CD_AMMVPerUnit);
				AssertEquals("CD_PrimaryCountryNA", true, pivot.CD_PrimaryCountryNA);
				AssertEquals("CD_RN_NKPrimaryCountry", "RU", pivot.CD_RN_NKPrimaryCountry);
				AssertEquals("CD_SecondaryCountryNA", true, pivot.CD_SecondaryCountryNA);
				AssertEquals("CD_RN_NKSecondaryCountry", "CN", pivot.CD_RN_NKSecondaryCountry);
				AssertEquals("CD_RN_NKCastCountry", "SG", pivot.CD_RN_NKCastCountry);
				AssertEquals("CD_RN_NKCertificateOrigin", "SG", pivot.CD_RN_NKCertificateOrigin);
				AssertEquals("CD_RN_NKMeltCountry", "SG", pivot.CD_RN_NKMeltCountry);
			});
		}

		public void TestProductCopiesFromInvoiceLine_CBMA()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;

			var invoice = declaration.Invoices.AddNew();
			var line = invoice.InvoiceLines.AddNew();
			line.JI_Tariff = "10123";
			line.US_TaxApply = "O";
			line.US_SecondarySPI = "C";
			line.US_TaxRateS = "TaxRateS";
			line.US_TaxRate = 0.1m;
			line.US_TTBRateDesignationCode = "TTB1";
			line.US_CBMADefaultTaxRate = 0.2m;

			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2022, 01, 10);
			var part = (line.Lookups.PartsList as OrgSupplierPartCollection).AddNew();
			var pivot = part.PivotsForBinding[0];

			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, pivot.CD_TaxRateDesc);
				AssertEquals(ZDecimal.Zero, pivot.CD_TaxRate);
				AssertEquals("TaxRateS", pivot.CD_TTBRateDesignationCode);
				AssertEquals(0.1m, pivot.CD_CBMADefaultTaxRate);
			});

			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2023, 01, 10);
			part = (line.Lookups.PartsList as OrgSupplierPartCollection).AddNew();
			pivot = part.PivotsForBinding[0];

			CombineAssertions(() =>
			{
				AssertEquals("TaxRateS", pivot.CD_TaxRateDesc);
				AssertEquals(0.1m, pivot.CD_TaxRate);
				AssertEquals("TTB1", pivot.CD_TTBRateDesignationCode);
				AssertEquals(0.2m, pivot.CD_CBMADefaultTaxRate);
			});
		}

		public void TestCopyCBMAFieldsFromProduct()
		{
			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer.OH_RL_NKClosestPort = "HKHKG";

			manufacturer.MainAddress.CustomsCodes.AddNew("FPS", "B1232022");
			manufacturer.MainAddress.CustomsCodes.AddNew("FPI", "TTB-FP-1234567");

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "SUNIMP";

			var wrapper = OrgHeaderWrapper.New(importer);
			var allocationQty = wrapper.AllocationQuantityPerFPIs.AddNew();
			allocationQty.US_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			allocationQty.US_ForeignProducerIdentifier = "B1232022";

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "SUNTEST";

			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = importer.PK;
			relOrg.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = USCTariff.CottonFeeApplicable;
			pivot.CD_TaxApplicability = "O";
			pivot.CD_ProductClaim = "C";
			pivot.CD_TaxCode = Core.Constants.USCustoms.FeeCodes.DistilledSpirits;
			pivot.CD_TaxRateDesc = "TaxRateS";
			pivot.CD_TaxRate = 0.1m;
			pivot.CD_TTBRateDesignationCode = "TTB1";
			pivot.CD_CBMADefaultTaxRate = 0.2m;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.JE_OA_ManufacturerAddress = manufacturer.MainAddress.PK;

			var invoice = declaration.Invoices.AddNew();
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2022, 01, 10);
			{
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_PartNo = "SUNTEST";
				CombineAssertions(() =>
				{
					AssertEquals("TTB1", invoiceLine.US_TaxRateS);
					AssertEquals(0.2m, invoiceLine.US_TaxRate);
					AssertEquals(ZString.Empty, invoiceLine.US_TTBRateDesignationCode);
					AssertEquals(ZDecimal.Zero, invoiceLine.US_CBMADefaultTaxRate);
					AssertEquals("B1232022", invoiceLine.US_FPI);
				});
			}

			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2023, 01, 10);
			{
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_PartNo = "SUNTEST";
				CombineAssertions(() =>
				{
					AssertEquals("TaxRateS", invoiceLine.US_TaxRateS);
					AssertEquals(0.1m, invoiceLine.US_TaxRate);
					AssertEquals("TTB1", invoiceLine.US_TTBRateDesignationCode);
					AssertEquals(0.2m, invoiceLine.US_CBMADefaultTaxRate);
					AssertEquals("TTB-FP-1234567", invoiceLine.US_FPI);
				});
			}
		}

		public void TestProductAutoGenerationDisclaimReasonShowing_Export()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;

			var invoice = declaration.Invoices.AddNew();
			var line = invoice.InvoiceLines.AddNew();
			line.JI_Tariff = "10123";

			line.US_AESOriginIndicator = OGAIndicatorList.Codes.Declared;

			line.US_DDTCMilitaryEquipmentIndicator = OGAIndicatorList.Codes.Declared;

			line.US_DDTCPartyCertificationIndicator = OGAIndicatorList.Codes.Declared;

			line.US_AMSInd = OGAIndicatorList.Codes.Disclaimed;
			line.US_AMSDisclaimReason = PGADisclaimReasonList.Codes.A;
			line.US_AMSDisclaimProgram = AMSProgramList.Codes.MO8;

			line.US_PSTIndicator = OGAIndicatorList.Codes.Disclaimed;
			line.US_PSTDisclaimReason = PGADisclaimReasonList.Codes.B;
			line.US_PSTDisclaimProgram = PSTProductTypeList.Codes.PS2;

			line.US_NMFSHMSInd = OGAIndicatorList.Codes.Disclaimed;
			line.US_NMFSHMSDisclaimReason = PGADisclaimReasonList.Codes.C;

			line.US_ATFInd = OGAIndicatorList.Codes.Declared;

			line.US_FWSInd = OGAIndicatorList.Codes.Disclaimed;
			line.US_FWSDisclaimReason = PGADisclaimReasonList.Codes.D;

			line.US_DEAInd = OGAIndicatorList.Codes.Disclaimed;
			line.US_DEADisclaimReason = PGADisclaimReasonList.Codes.A;

			line.US_TTBInd = OGAIndicatorList.Codes.Disclaimed;
			line.US_TTBDisclaimReason = PGADisclaimReasonList.Codes.B;

			var part = new OrgSupplierPartCollection(Factory, line, line.InvoiceHeader?.IsExport ?? ZBool.False).AddNew();
			AssertEquals(1, part.PivotsForBinding.Count);
			var pivot = part.PivotsForBinding[0];

			AssertEquals(OGAIndicatorList.Codes.Declared, pivot.CD_OriginIndicator);

			AssertEquals(OGAIndicatorList.Codes.Declared, pivot.CD_MilitaryEquipInd);

			AssertEquals(OGAIndicatorList.Codes.Declared, pivot.CD_PartyCertInd);

			AssertEquals(OGAIndicatorList.Codes.Disclaimed, pivot.CD_AMSIndicator);
			AssertEquals(PGADisclaimReasonList.Codes.A, pivot.CD_AMSDisclaimReason);
			AssertEquals(AMSProgramList.Codes.MO8, pivot.CD_AMSDisclaimProgram);

			AssertEquals(OGAIndicatorList.Codes.Disclaimed, pivot.CD_PSTIndicator);
			AssertEquals(PGADisclaimReasonList.Codes.B, pivot.CD_PSTDisclaimReason);
			AssertEquals(PSTProductTypeList.Codes.PS2, pivot.CD_PSTDisclaimProgram);

			AssertEquals(OGAIndicatorList.Codes.Disclaimed, pivot.CD_NMFSHMSIndicator);
			AssertEquals(PGADisclaimReasonList.Codes.C, pivot.CD_NMFSHMSDisclaimReason);

			AssertEquals(OGAIndicatorList.Codes.Declared, pivot.CD_ATFIndicator);

			AssertEquals(OGAIndicatorList.Codes.Disclaimed, pivot.CD_FWSIndicator);
			AssertEquals(PGADisclaimReasonList.Codes.D, pivot.CD_FWSDisclaimReason);

			AssertEquals(OGAIndicatorList.Codes.Disclaimed, pivot.CD_DEAIndicator);
			AssertEquals(PGADisclaimReasonList.Codes.A, pivot.CD_DEADisclaimReason);

			AssertEquals(OGAIndicatorList.Codes.Disclaimed, pivot.CD_TTBIndicator);
			AssertEquals(PGADisclaimReasonList.Codes.B, pivot.CD_TTBDisclaimReason);
		}

		public void TestKeepingValidFDAWhenCargoReleaseTypeChanged()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Test";

			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = importer.PK;
			relOrg.OU_Relationship = "OWN";

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = USCTariff.CottonFeeApplicable;
			pivot.CD_ACEFDAIndicator = OGAIndicatorList.Codes.Declared;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var fdaOnProduct = pivot.ACEFDAs.AddNew();
			fdaOnProduct.US_BrandName = "BRD NAME";
			fdaOnProduct.US_TrackingStatus = PGATrackingStatusList.Codes.Adding;

			invoiceLine.JI_PartNo = "Test";

			AssertEquals(invoiceLine.ACE_FDALines.Count, 1);
			var aceFDA = invoiceLine.ACE_FDALines[0];
			AssertEquals("US_BrandName", "BRD NAME", aceFDA.US_BrandName);
			AssertEquals("US_TrackingStatus", ZString.Empty, aceFDA.US_TrackingStatus);

			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			AssertEquals(invoiceLine.ACE_FDALines.Count, 1);
			AssertCollectionContains(aceFDA, invoiceLine.ACE_FDALines);
			AssertEquals(invoiceLine.FDAs.Count, 0);
		}

		public void TestNeedsCustomsQuantity()
		{
			ZString tariffCode = "0000000000";
			ZString customsUQ = "";

			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = tariffCode;
			tariff.UE_Unit1 = customsUQ;
			tariff.UE_DateFrom = ZDateTime.Today;
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusTariffTypeSB = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.ScheduleB);
			Factory.Save();

			var scheduleB = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, cusTariffTypeSB.PK, tariffCode, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertEquals("IsExportDeclaration", false, InvoiceLine.IsExport);
			InvoiceLine.JI_Tariff = tariffCode;
			AssertEquals("CustomsUQ", customsUQ, InvoiceLine.CustomsUQ);
			InvoiceLine.US_ExportCode = "";
			AssertEquals("IsLimitedReportingExportCode", false, InvoiceLine.IsLimitedReportingExportCode);
			AssertEquals("NeedsCustomsQuantity", false, InvoiceLine.NeedsCustomsQuantity);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("IsExportDeclaration", true, InvoiceLine.IsExport);
			AssertEquals("NeedsCustomsQuantity", false, InvoiceLine.NeedsCustomsQuantity);

			customsUQ = AESUnitOfMeasureList.Codes.Number;
			tariff.UE_Unit1 = customsUQ;
			helper.CreateTariffUOM(scheduleB, "CU1", customsUQ);
			scheduleB.UnitsOfMeasure.RefreshFromDb();
			AssertEquals("CustomsUQ", customsUQ, InvoiceLine.CustomsUQ);

			InvoiceLine.JI_CustomsUnitQty = customsUQ;
			AssertEquals("NeedsCustomsQuantity", true, InvoiceLine.NeedsCustomsQuantity);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertEquals("NeedsCustomsQuantity", false, InvoiceLine.NeedsCustomsQuantity);

			InvoiceLine.US_ExportCode = LimitedReportingExportInformationCodeList.Codes.DD;
			AssertEquals("IsLimitedReportingExportCode", true, InvoiceLine.IsLimitedReportingExportCode);
			AssertEquals("NeedsCustomsQuantity", false, InvoiceLine.NeedsCustomsQuantity);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("NeedsCustomsQuantity", false, InvoiceLine.NeedsCustomsQuantity);

			InvoiceLine.US_ExportCode = "";
			AssertEquals("IsLimitedReportingExportCode", false, InvoiceLine.IsLimitedReportingExportCode);
			customsUQ = AESUnitOfMeasureList.Codes.NoUnitRequired;

			AssertEquals("CustomsUQ", tariff.UE_Unit1, InvoiceLine.CustomsUQ);
			InvoiceLine.JI_CustomsUnitQty = customsUQ;
			AssertEquals("NeedsCustomsQuantity", false, InvoiceLine.NeedsCustomsQuantity);

			InvoiceLine.JI_CustomsUnitQty = AESUnitOfMeasureList.Codes.Number;
			AssertEquals("NeedsCustomsQuantity", true, InvoiceLine.NeedsCustomsQuantity);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertEquals("NeedsCustomsQuantity", false, InvoiceLine.NeedsCustomsQuantity);
		}

		// FOB, Insurance, Freight in local currency
		public void TestBindingValuesInLocalCurrency()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				RefCurrency eur = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.Austria);
				eur.SetCustomsRate(ZDateTime.Today, ZDateTime.Today.AddDays(1), 2m);

				InvoiceHeader.JZ_InvoiceAmount = 10000m;
				InvoiceHeader.JZ_RX_NKInvoice_Currency = eur.RX_Code;
				InvoiceHeader.JZ_IncoTerm = "FOB";

				InvoiceLine.JI_LinePrice = 10000m;

				InvoiceHeader.Charges.AddNew(USCustomsChargeTypeList.Codes.OverseasFreight, 100m, eur.RX_Code);
				InvoiceHeader.Charges.AddNew(USCustomsChargeTypeList.Codes.OverseasInsurance, 10m, eur.RX_Code);

				Declaration.ResumeApportionment();
				AssertEquals("Overseas Freight", 100m, InvoiceLine.JI_Calc_FreightInInvoiceCurr);
				AssertEquals("Overseas Insurance", 10m, InvoiceLine.JI_Calc_InsuranceInInvoiceCurr);
				AssertEquals("FOB", 10000m, InvoiceLine.JI_Calc_FOB);

				AssertEquals("Overseas Freight in USD", 200m, InvoiceLine.FreightInLocalCurrency);
				AssertEquals("Insurance in USD", 20m, InvoiceLine.InsuranceInLocalCurrency);
				AssertEquals("FOB in USD", 20000m, InvoiceLine.FOBValueInLocalCurrency);
			}
		}

		public void TestRefreshChildLines()
		{
			var invoice = Declaration.Invoices.AddNew();
			var line1 = invoice.InvoiceLines.AddNew();
			var line2 = invoice.InvoiceLines.AddNew();
			var line3 = invoice.InvoiceLines.AddNew();

			line2.JI_ParentID = line1.PK;
			AssertEquals("One child", 1, line1.ChildLines.Count());

			line3.JI_ParentID = line1.PK;
			AssertEquals("Two children", 2, line1.ChildLines.Count());

			line3.JI_ParentID = line2.PK;
			AssertEquals("one child", 1, line1.ChildLines.Count());
			AssertEquals("one child", 1, line2.ChildLines.Count());

			line2.JI_ParentID = ZGuid.Empty;
			line3.JI_ParentID = ZGuid.Empty;
			AssertEquals("no children", 0, line1.ChildLines.Count());
			AssertEquals("no children", 0, line2.ChildLines.Count());
		}

		public void TestNeedsSecondCustomsQuantity()
		{
			ZString tariffCode = "0000000000";
			ZString customsUQ = "";

			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = tariffCode;
			tariff.UE_Unit2 = customsUQ;
			tariff.UE_DateFrom = ZDateTime.Today;
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusTariffTypeSB = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.ScheduleB);
			Factory.Save();

			var scheduleB = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, cusTariffTypeSB.PK, tariffCode, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertEquals("IsExportDeclaration", false, InvoiceLine.IsExport);
			InvoiceLine.JI_Tariff = tariffCode;
			AssertEquals("JI_CustomsSecondUnitQty", customsUQ, InvoiceLine.JI_CustomsSecondUnitQty);
			InvoiceLine.US_ExportCode = "";
			AssertEquals("IsLimitedReportingExportCode", false, InvoiceLine.IsLimitedReportingExportCode);
			AssertEquals("NeedsSecondCustomsQuantity", false, InvoiceLine.NeedsSecondCustomsQuantity);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("IsExportDeclaration", true, InvoiceLine.IsExport);
			AssertEquals("NeedsSecondCustomsQuantity", false, InvoiceLine.NeedsSecondCustomsQuantity);

			customsUQ = AESUnitOfMeasureList.Codes.Number;
			tariff.UE_Unit2 = customsUQ;
			var uom = helper.CreateTariffUOM(scheduleB, "CU2", customsUQ);
			scheduleB.UnitsOfMeasure.RefreshFromDb();
			InvoiceLine.JI_Tariff = "";
			InvoiceLine.JI_Tariff = tariffCode;
			AssertEquals("JI_CustomsSecondUnitQty", customsUQ, InvoiceLine.JI_CustomsSecondUnitQty);
			AssertEquals("NeedsSecondCustomsQuantity", true, InvoiceLine.NeedsSecondCustomsQuantity);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertEquals("NeedsSecondCustomsQuantity", false, InvoiceLine.NeedsSecondCustomsQuantity);

			InvoiceLine.US_ExportCode = LimitedReportingExportInformationCodeList.Codes.DD;
			AssertEquals("IsLimitedReportingExportCode", true, InvoiceLine.IsLimitedReportingExportCode);
			AssertEquals("NeedsSecondCustomsQuantity", false, InvoiceLine.NeedsSecondCustomsQuantity);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("NeedsSecondCustomsQuantity", false, InvoiceLine.NeedsSecondCustomsQuantity);

			InvoiceLine.US_ExportCode = "";
			AssertEquals("IsLimitedReportingExportCode", false, InvoiceLine.IsLimitedReportingExportCode);
			customsUQ = AESUnitOfMeasureList.Codes.NoUnitRequired;
			tariff.UE_Unit2 = customsUQ;
			uom.ZZ8_UOM = customsUQ;
			InvoiceLine.JI_Tariff = "";
			InvoiceLine.JI_Tariff = tariffCode;
			AssertEquals("JI_CustomsSecondUnitQty", customsUQ, InvoiceLine.JI_CustomsSecondUnitQty);
			AssertEquals("NeedsSecondCustomsQuantity", false, InvoiceLine.NeedsSecondCustomsQuantity);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertEquals("NeedsSecondCustomsQuantity", false, InvoiceLine.NeedsSecondCustomsQuantity);
		}

		public void TestNeedsThirdCustomsQuantity()
		{
			ZString tariffCode = "0000000000";
			ZString customsUQ = "";

			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = tariffCode;
			tariff.UE_Unit3 = customsUQ;
			tariff.UE_DateFrom = ZDateTime.Today;
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			InvoiceLine.JI_Tariff = tariffCode;
			AssertEquals("JI_CustomsThirdUnitQty", customsUQ, InvoiceLine.JI_CustomsThirdUnitQty);
			AssertEquals("NeedsThirdCustomsQuantity", false, InvoiceLine.NeedsThirdCustomsQuantity);

			customsUQ = ABIUnitOfMeasureList.Codes.Number;
			tariff.UE_Unit3 = customsUQ;
			InvoiceLine.JI_Tariff = "";
			InvoiceLine.JI_Tariff = tariffCode;
			AssertEquals("JI_CustomsThirdUnitQty", customsUQ, InvoiceLine.JI_CustomsThirdUnitQty);
			AssertEquals("NeedsThirdCustomsQuantity", true, InvoiceLine.NeedsThirdCustomsQuantity);

			customsUQ = ABIUnitOfMeasureList.Codes.NoUnitRequired;
			tariff.UE_Unit3 = customsUQ;
			InvoiceLine.JI_Tariff = "";
			InvoiceLine.JI_Tariff = tariffCode;
			AssertEquals("JI_CustomsThirdUnitQty", customsUQ, InvoiceLine.JI_CustomsThirdUnitQty);
			AssertEquals("NeedsThirdCustomsQuantity", false, InvoiceLine.NeedsThirdCustomsQuantity);
		}

		public void TestUS_98GoodsValueReadOnly()
		{
			InvoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			InvoiceLine.JI_Tariff = "9801";
			AssertEquals(false, InvoiceLine.US_98GoodsValueInfo.ReadOnly);

			InvoiceLine.JI_Tariff = "9102";
			AssertEquals(true, InvoiceLine.US_98GoodsValueInfo.ReadOnly);

			InvoiceLine.US_UC_NKCountryOfOrigin = "US";
			AssertEquals(false, InvoiceLine.US_98GoodsValueInfo.ReadOnly);

			InvoiceLine.US_SupTariff = "9802";
			AssertEquals(false, InvoiceLine.US_98GoodsValueInfo.ReadOnly);
		}

		public void TestCalcMiscLicenseTypeLabel()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceLine.JI_Tariff = "2922292700";
			AssertEquals("Misc. License No.", InvoiceLine.CalcMiscLicenseTypeLabel);

			var uscTariff = USCTariffTest.CreateUSCTariffWithPermitLicenseIndicator(Factory, "7306191010", MiscellaneousPermitLicenseList.Codes.SteelImportLicense, InvoiceLine.EffectiveDateForDutyRate);
			InvoiceLine.JI_Tariff = uscTariff.UE_Tariff;
			AssertEquals("Steel License No.", InvoiceLine.CalcMiscLicenseTypeLabel);

			uscTariff = USCTariffTest.CreateUSCTariffWithPermitLicenseIndicator(Factory, "99106145", MiscellaneousPermitLicenseList.Codes.SingaporeTPLCertificate, InvoiceLine.EffectiveDateForDutyRate);
			InvoiceLine.JI_Tariff = uscTariff.UE_Tariff;
			AssertEquals("SG TPL License No.", InvoiceLine.CalcMiscLicenseTypeLabel);

			uscTariff = USCTariffTest.CreateUSCTariffWithPermitLicenseIndicator(Factory, "99990056", MiscellaneousPermitLicenseList.Codes.CANAFTATPLCertificate, InvoiceLine.EffectiveDateForDutyRate);
			InvoiceLine.JI_Tariff = uscTariff.UE_Tariff;
			AssertEquals("CA NAFTA Cert. No.", InvoiceLine.CalcMiscLicenseTypeLabel);

			uscTariff = USCTariffTest.CreateUSCTariffWithPermitLicenseIndicator(Factory, "99990060", MiscellaneousPermitLicenseList.Codes.MXNAFTATPLCertificate, InvoiceLine.EffectiveDateForDutyRate);
			InvoiceLine.JI_Tariff = "99990060";
			AssertEquals("MX NAFTA Cert. No.", InvoiceLine.CalcMiscLicenseTypeLabel);

			uscTariff = USCTariffTest.CreateUSCTariffWithPermitLicenseIndicator(Factory, "0201101090", MiscellaneousPermitLicenseList.Codes.BeefExportCertificate, InvoiceLine.EffectiveDateForDutyRate);
			InvoiceLine.JI_Tariff = uscTariff.UE_Tariff;
			AssertEquals("Beef Certificate No.", InvoiceLine.CalcMiscLicenseTypeLabel);

			uscTariff = USCTariffTest.CreateUSCTariffWithPermitLicenseIndicator(Factory, "7102213000", MiscellaneousPermitLicenseList.Codes.DiamondCertificate, InvoiceLine.EffectiveDateForDutyRate);
			InvoiceLine.JI_Tariff = uscTariff.UE_Tariff;
			AssertEquals("Diamond Cert. No.", InvoiceLine.CalcMiscLicenseTypeLabel);

			uscTariff = USCTariffTest.CreateUSCTariffWithPermitLicenseIndicator(Factory, "4407100119", MiscellaneousPermitLicenseList.Codes.CanadaSoftwoodLumberExportNumber, InvoiceLine.EffectiveDateForDutyRate);
			InvoiceLine.JI_Tariff = uscTariff.UE_Tariff;
			AssertEquals("Lumber Permit No.", InvoiceLine.CalcMiscLicenseTypeLabel);

			uscTariff = USCTariffTest.CreateUSCTariffWithPermitLicenseIndicator(Factory, "98211119", MiscellaneousPermitLicenseList.Codes.ATPDEACertificateHTS98211119, InvoiceLine.EffectiveDateForDutyRate);
			InvoiceLine.JI_Tariff = uscTariff.UE_Tariff;
			AssertEquals("ATPDEA Cert No.", InvoiceLine.CalcMiscLicenseTypeLabel);

			uscTariff = USCTariffTest.CreateUSCTariffWithPermitLicenseIndicator(Factory, "99130465", MiscellaneousPermitLicenseList.Codes.AustraliaFreeTradeExportCertificate, InvoiceLine.EffectiveDateForDutyRate);
			InvoiceLine.JI_Tariff = uscTariff.UE_Tariff;
			AssertEquals("AU FT Export Cert.", InvoiceLine.CalcMiscLicenseTypeLabel);

			uscTariff = USCTariffTest.CreateUSCTariffWithPermitLicenseIndicator(Factory, "2523290000", MiscellaneousPermitLicenseList.Codes.MexicanCementImportLicense, InvoiceLine.EffectiveDateForDutyRate);
			InvoiceLine.JI_Tariff = uscTariff.UE_Tariff;
			AssertEquals("MX Cement Imp. Lic.", InvoiceLine.CalcMiscLicenseTypeLabel);

			uscTariff = USCTariffTest.CreateUSCTariffWithPermitLicenseIndicator(Factory, "99156101", MiscellaneousPermitLicenseList.Codes.CAFTATPLCertificate, InvoiceLine.EffectiveDateForDutyRate);
			InvoiceLine.JI_Tariff = uscTariff.UE_Tariff;
			AssertEquals("NI CAFTA TPL Cert.", InvoiceLine.CalcMiscLicenseTypeLabel);

			uscTariff = USCTariffTest.CreateUSCTariffWithPermitLicenseIndicator(Factory, "99025211", MiscellaneousPermitLicenseList.Codes.CottonShirtingFabricLicenseNumber, InvoiceLine.EffectiveDateForDutyRate);
			InvoiceLine.JI_Tariff = uscTariff.UE_Tariff;
			AssertEquals("Cotton Shirting Lic.", InvoiceLine.CalcMiscLicenseTypeLabel);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			AssertEquals("License No.", InvoiceLine.CalcMiscLicenseTypeLabel);
		}

		public void TestUS_DateOfExport()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			InvoiceHeader.US_DateOfExport = new ZDateTime(2007, 2, 4);
			AssertEquals("US_DateOfExport", new ZDateTime(2007, 2, 4), InvoiceLine.US_DateOfExport);

			InvoiceLine.JI_JZ = ZGuid.Missing;
			AssertEquals("US_DateOfExport", ZDateTime.Empty, InvoiceLine.US_DateOfExport);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			InvoiceLine.JI_JZ = InvoiceHeader.PK;

			InvoiceHeader.US_DateOfExport = new ZDateTime(2012, 06, 27);
			AssertEquals(new ZDateTime(2012, 06, 27), InvoiceLine.US_DateOfExport);

			InvoiceLine.US_DateOfExport = new ZDateTime(2012, 06, 28);
			AssertEquals(new ZDateTime(2012, 06, 28), InvoiceLine.US_DateOfExport);

			InvoiceHeader.US_DateOfExport = new ZDateTime(2012, 06, 28);
			InvoiceHeader.US_DateOfExport = new ZDateTime(2012, 06, 29);
			AssertEquals(new ZDateTime(2012, 06, 29), InvoiceLine.US_DateOfExport);
		}

		public void TestCloneInvoiceLineClonesAllCollections()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_DDTCTrackingStatus = PGATrackingStatusList.Codes.Added;
			invoiceLine.US_ODSTrackingStatus = PGATrackingStatusList.Codes.Added;
			invoiceLine.US_TSCATrackingStatus = PGATrackingStatusList.Codes.Added;
			invoiceLine.SetTrackingID();
			invoiceLine.FeeCusCodes.AddNew();
			invoiceLine.FeeCusCodes[0].CY_Code = "CC";

			var fda = invoiceLine.FDAs.AddNew();
			fda.US_FDAQty1 = 5m;
			fda.US_FDAValue = 5m;
			fda.US_FDAConfirmDate = ZDateTime.Today;

			invoiceLine.FCCs.AddNew();
			invoiceLine.FCCs[0].US_FCCID = "ID";

			invoiceLine.DOTs.AddNew();
			invoiceLine.DOTs[0].US_DOTCommercialDesc = "TEST";

			var laceyAct = invoiceLine.LaceyActLines.AddNew();
			laceyAct.US_PGACommercialDescription = "clone pga line";
			laceyAct.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
			var element = laceyAct.PG04ConstituentElements.AddNew();
			element.US_PGANameOfTheConstituentElement = "PINE";
			element.ScientificDataCollection.AddNew();
			element.ScientificDataCollection[0].US_PGAScientificGenusName = "Genus Name";

			invoiceLine.PSTLines.AddNew();
			invoiceLine.PSTLines.AddNew();

			var nmfsLine1 = invoiceLine.NMFSLines.AddNew();
			nmfsLine1.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			nmfsLine1.US_Commodity = FishStateList.Codes.FreshToothfish;
			nmfsLine1.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
			var nmfsLine2 = invoiceLine.NMFSLines.AddNew();
			nmfsLine2.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
			nmfsLine2.US_HMSPermitNumber = "HSM323";

			var ttb1 = invoiceLine.TTBLines.AddNew();
			ttb1.US_ProgramCode = TTBProgramCodeList.Codes.Wine;
			ttb1.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
			var ttb2 = invoiceLine.TTBLines.AddNew();
			ttb2.US_ProgramCode = TTBProgramCodeList.Codes.Tobacco;

			var aphisHeader1 = invoiceLine.APHISHeaders.AddNew();
			aphisHeader1.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			aphisHeader1.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
			var aphisHeader2 = invoiceLine.APHISHeaders.AddNew();
			aphisHeader2.US_ProgramType = APHISProgramCodeList.Codes.AAC;

			var fwsHeader1 = invoiceLine.FWSHeaders.AddNew();
			fwsHeader1.US_ProcessingCode = FWSProcessingCodeList.Codes.LDS;
			fwsHeader1.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
			var fwsHeader2 = invoiceLine.FWSHeaders.AddNew();
			fwsHeader2.US_ProcessingCode = FWSProcessingCodeList.Codes.EDS;

			var lineRange = invoiceLine.LineGroupingRanges.AddNew(1, 3);
			var cloned = invoiceLine.Clone();
			AssertEquals("US_TSCATrackingStatus", ZString.Empty, cloned.US_TSCATrackingStatus);
			AssertEquals("US_DDTCTrackingStatus", ZString.Empty, cloned.US_DDTCTrackingStatus);
			AssertEquals("US_ODSTrackingStatus", ZString.Empty, cloned.US_ODSTrackingStatus);
			AssertEquals("GetTrackingID()", ZString.Empty, cloned.GetTrackingID());

			AssertEquals("should not clone FeeCusCodes", 0, cloned.FeeCusCodes.Count);

			AssertEquals("cloned FDAs", 5m, cloned.FDAs[0].US_FDAQty1);
			AssertEquals("cloned FDAs", 0m, cloned.FDAs[0].US_FDAValue);
			AssertEquals("cloned FDAs", ZDateTime.Empty, cloned.FDAs[0].US_FDAConfirmDate);
			AssertEquals("cloned FCCs", "ID", cloned.FCCs[0].US_FCCID);
			AssertEquals("cloned DOTs", "TEST", cloned.DOTs[0].US_DOTCommercialDesc);
			AssertEquals("cloned LineNumberRanges", 0, cloned.LineGroupingRanges.Count);
			AssertEquals("cloned PGAs", "clone pga line", cloned.LaceyActLines[0].US_PGACommercialDescription);
			AssertEquals("cloned PGAs", "PINE", cloned.LaceyActLines[0].PG04ConstituentElements[0].US_PGANameOfTheConstituentElement);
			AssertEquals("cloned PGAs", "Genus Name", cloned.LaceyActLines[0].PG04ConstituentElements[0].ScientificDataCollection[0].US_PGAScientificGenusName);
			AssertEquals("PST Lines cloned too", 2, cloned.PSTLines.Count);
			AssertEquals("NMFS Lines cloned too", 2, cloned.NMFSLines.Count);
			var nmfsLine1Cloned = cloned.NMFSLines[0];
			AssertEquals("nmfsLine1Cloned.US_ProgramType", NMFSProgramCodeList.Codes.AMR, nmfsLine1Cloned.US_ProgramType);
			AssertEquals("nmfsLine1Cloned.US_Commodity", FishStateList.Codes.FreshToothfish, nmfsLine1Cloned.US_Commodity);
			AssertEquals("nmfsLine1Cloned.US_TrackingStatus", ZString.Empty, nmfsLine1Cloned.US_TrackingStatus);
			var nmfsLine2Cloned = cloned.NMFSLines[1];
			AssertEquals("nmfsLine2Cloned.US_ProgramType", NMFSProgramCodeList.Codes.HMS, nmfsLine2Cloned.US_ProgramType);
			AssertEquals("nmfsLine2Cloned.US_HMSPermitNumber", "HSM323", nmfsLine2Cloned.US_HMSPermitNumber);

			AssertEquals("TTB Lines cloned too", 2, cloned.TTBLines.Count);
			var ttb1Cloned = cloned.TTBLines[0];
			AssertEquals("ttb1Cloned.US_ProgramCode", TTBProgramCodeList.Codes.Wine, ttb1Cloned.US_ProgramCode);
			AssertEquals("ttb1Cloned.US_TrackingStatus", ZString.Empty, ttb1Cloned.US_TrackingStatus);
			var ttb2Cloned = cloned.TTBLines[1];
			AssertEquals("ttb2Cloned.US_ProgramCode", TTBProgramCodeList.Codes.Tobacco, ttb2Cloned.US_ProgramCode);

			AssertEquals("APHIS Lines cloned too", 2, cloned.APHISHeaders.Count);
			var aphisHeader1Cloned = cloned.APHISHeaders[0];
			AssertEquals("aphisHeader1Cloned.US_ProgramType", APHISProgramCodeList.Codes.AVS, aphisHeader1Cloned.US_ProgramType);
			AssertEquals("aphisHeader1Cloned.US_TrackingStatus", ZString.Empty, aphisHeader1Cloned.US_TrackingStatus);
			var aphisHeader2Cloned = cloned.APHISHeaders[1];
			AssertEquals("aphisHeader2Cloned.US_ProgramType", APHISProgramCodeList.Codes.AAC, aphisHeader2Cloned.US_ProgramType);

			AssertEquals("FWS Headers cloned too", 2, cloned.FWSHeaders.Count);
			var fwsHeader1Cloned = cloned.FWSHeaders[0];
			AssertEquals("fwsHeader1Cloned.US_ProcessingCode", FWSProcessingCodeList.Codes.LDS, fwsHeader1Cloned.US_ProcessingCode);
			AssertEquals("fwsHeader1Cloned.US_TrackingStatus", ZString.Empty, fwsHeader1Cloned.US_TrackingStatus);
			var fwsHeader2Cloned = cloned.FWSHeaders[1];
			AssertEquals("fwsHeader2Cloned.US_ProcessingCode", FWSProcessingCodeList.Codes.EDS, fwsHeader2Cloned.US_ProcessingCode);
		}

		public void TestExcludePropetiesFromClone()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.JI_Tariff = USCTariff.FDAAdmissibilityReviewRequiredTariff;
			invoiceLine.US_SupTariff = USCTariff.AdditionalDutyCalculationApplicable;
			invoiceLine.US_TextileCategoryNo = "123";
			invoiceLine.US_UC_NKCountryOfOrigin = "IT";
			invoiceLine.US_UC_NKCountryOfExport = "IT";

			invoiceLine.US_98GoodsValue = 121.1m;
			invoiceLine.US_98ValueInvCurr = 122m;
			invoiceLine.US_ADDDepositValue = 100m;
			invoiceLine.US_ADDuty = 15.1m;
			invoiceLine.US_CVDDepositValue = 150m;
			invoiceLine.US_CVDuty = 18.36m;
			invoiceLine.US_CustomsValue = 1200m;
			invoiceLine.US_OverrideDuty = true;
			invoiceLine.US_Duty = 13.8m;
			invoiceLine.US_OverrideSupDuty = true;
			invoiceLine.US_SupDuty = 50m;
			invoiceLine.US_HasMPF = true;
			invoiceLine.US_PayableMPF = 1.12m;
			invoice.US_DateOfExport = ZDateTime.Now;
			invoiceLine.US_DateOfExport = ZDateTime.Now.AddDays(-1);
			invoiceLine.US_FTADuty = 1m;
			invoiceLine.US_FTAPayableMPF = 1m;
			invoiceLine.US_NonFTADuty = 1m;
			invoiceLine.US_NonFTAPayableMPF = 1m;

			Factory.Save();

			var clonedDeclaration = (JobDeclaration)Declaration.TemplateCopy();
			var clonedLine = clonedDeclaration.InvoiceLines[0];
			AssertEquals("should not clone US_98GoodsValue", ZDecimal.Zero, clonedLine.US_98GoodsValue);
			AssertEquals("should not clone US_98ValueInvCurr", ZDecimal.Zero, clonedLine.US_98ValueInvCurr);
			AssertEquals("should not clone US_ADDDepositValue", ZDecimal.Zero, clonedLine.US_ADDDepositValue);
			AssertEquals("should not clone US_ADDuty", ZDecimal.Zero, clonedLine.US_ADDuty);
			AssertEquals("should not clone US_CVDDepositValue", ZDecimal.Zero, clonedLine.US_CVDDepositValue);
			AssertEquals("should not clone US_CVDuty", ZDecimal.Zero, clonedLine.US_CVDuty);
			AssertEquals("should not clone US_CustomsValue", ZDecimal.Zero, clonedLine.US_CustomsValue);
			AssertEquals("should not clone US_OverrideDuty", false, clonedLine.US_OverrideDuty);
			AssertEquals("should not clone US_Duty", ZDecimal.Zero, clonedLine.US_Duty);
			AssertEquals("should not clone US_OverrideSupDuty", false, clonedLine.US_OverrideSupDuty);
			AssertEquals("should not clone US_SupDuty", ZDecimal.Zero, clonedLine.US_SupDuty);
			AssertEquals("should not clone US_HasMPF", false, clonedLine.US_HasMPF);
			AssertEquals("should not clone US_PayableMPF", ZDecimal.Zero, clonedLine.US_PayableMPF);
			AssertEquals("should not clone US_DateOfExport line", ZDateTime.Empty, clonedLine.US_DateOfExport);
			AssertEquals("should not clone US_DateOfExport header", ZDateTime.Empty, clonedDeclaration.Invoices[0].US_DateOfExport);
			AssertEquals("should not clone US_FTADuty", ZDecimal.Zero, clonedLine.US_FTADuty);
			AssertEquals("should not clone US_FTAPayableMPF", ZDecimal.Zero, clonedLine.US_FTAPayableMPF);
			AssertEquals("should not clone US_NonFTADuty", ZDecimal.Zero, clonedLine.US_NonFTADuty);
			AssertEquals("should not clone US_NonFTAPayableMPF", ZDecimal.Zero, clonedLine.US_NonFTAPayableMPF);
		}

		public void TestSecondTariffDutyRate()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_DateFrom = ZDateTime.Today.AddDays(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);
			tariff.UE_Column1RateAdValorem = 0.1m;
			tariff.UE_Column2RateAdValorem = 0.1m;
			tariff.UE_Tariff = "99038501";
			tariff.UE_Column1RateOther = 0.8m;
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;

			USCTariff tariff2 = Factory.New<USCTariff>();
			tariff2.UE_DateFrom = ZDateTime.Today.AddDays(-1);
			tariff2.UE_DateTo = ZDateTime.Today.AddDays(1);
			tariff2.UE_Column1RateAdValorem = 0.058m;
			tariff2.UE_Column2RateAdValorem = 0.058m;
			tariff2.UE_Tariff = "7607113000";
			tariff2.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();

			invoiceLine1.JI_LinePrice = 0m;
			invoiceLine1.JI_CustomsQuantity = 1000m;
			invoiceLine1.JI_Weight = 1000m;
			invoiceLine1.JI_Tariff = tariff2.UE_Tariff;
			invoiceLine1.US_SupTariff = tariff.UE_Tariff;
			invoiceLine1.US_UC_NKCountryOfOrigin = "US";
			invoiceLine1.US_98GoodsValue = 27720.0m;

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			AssertEquals("CustomsValue", "9903.85.01", invoiceLine1.ProvProgTariff);
			AssertEquals("CustomsValue", "10%", invoiceLine1.ProvProgDutyRate);
		}

		public void TestSecondTariffDutyRateWithChildLines()
		{
			USCTariff tariff1 = Factory.New<USCTariff>();
			tariff1.UE_DateFrom = ZDateTime.Today.AddDays(-1);
			tariff1.UE_DateTo = ZDateTime.Today.AddDays(1);
			tariff1.UE_Column1RateAdValorem = 0.15m;
			tariff1.UE_Tariff = "99038815";
			tariff1.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;

			USCTariff tariff2 = Factory.New<USCTariff>();
			tariff2.UE_DateFrom = ZDateTime.Today.AddDays(-1);
			tariff2.UE_DateTo = ZDateTime.Today.AddDays(1);
			tariff2.UE_Column1RateAdValorem = 0.25m;
			tariff2.UE_Tariff = "99038001";
			tariff2.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;

			USCTariff tariff3 = Factory.New<USCTariff>();
			tariff3.UE_DateFrom = ZDateTime.Today.AddDays(-1);
			tariff3.UE_DateTo = ZDateTime.Today.AddDays(1);
			tariff3.UE_Column1RateAdValorem = 0m;
			tariff3.UE_Column2RateAdValorem = 0.35m;
			tariff3.UE_Tariff = "7304907000";
			tariff3.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 2500m;

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();

			invoiceLine1.JI_LinePrice = 0m;
			invoiceLine1.JI_CustomsQuantity = 0;
			invoiceLine1.JI_Weight = 0;
			invoiceLine1.US_SupTariff = tariff1.UE_Tariff;
			invoiceLine1.US_UC_NKCountryOfOrigin = "US";

			invoiceLine2.JI_LinePrice = 2500m;
			invoiceLine2.JI_CustomsQuantity = 700;
			invoiceLine2.JI_Weight = 100;
			invoiceLine2.JI_Tariff = tariff3.UE_Tariff;
			invoiceLine2.US_SupTariff = tariff2.UE_Tariff;
			invoiceLine2.US_UC_NKCountryOfOrigin = "US";

			invoiceLine1.US_IsParent = true;
			invoiceLine2.JI_ParentLine = invoiceLine1.JI_LineNo;
			invoiceLine2.JI_ParentID = invoiceLine1.PK;

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			AssertEquals("CustomsValue", "9903.88.15", invoiceLine1.ProvProgTariff);
			AssertEquals("CustomsValue", "15%", invoiceLine1.ProvProgDutyRate);

			AssertEquals("CustomsValue", "9903.80.01", invoiceLine2.ProvProgTariff);
			AssertEquals("CustomsValue", "25%", invoiceLine2.ProvProgDutyRate);
		}

		public void TestIsFeeExempt()
		{
			InvoiceLine.US_CottonCertificateNo = "";
			AssertEquals(false, InvoiceLine.IsAMSFeeExempt);
			InvoiceLine.US_CottonCertificateNo = "123456789";
			AssertEquals(true, InvoiceLine.IsAMSFeeExempt);
		}

		public void TestCountryOfOrigin()
		{
			JobComInvoiceHeader invoice = Factory.New<JobComInvoiceHeader>();
			invoice.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(Core.Constants.CountryCodes.SouthAfrica, invoiceLine.US_UC_NKCountryOfOrigin);

			invoice.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.KoreaSouth;
			AssertEquals(Core.Constants.CountryCodes.KoreaSouth, invoiceLine.US_UC_NKCountryOfOrigin);

			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			invoice.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Canada;
			AssertEquals(Core.Constants.CountryCodes.China, invoiceLine.US_UC_NKCountryOfOrigin);
		}

		public void TestCountryOfExport()
		{
			JobComInvoiceHeader invoice = Factory.New<JobComInvoiceHeader>();
			invoice.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.SouthAfrica;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(Core.Constants.CountryCodes.SouthAfrica, invoiceLine.US_UC_NKCountryOfExport);

			invoice.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.KoreaSouth;
			AssertEquals(Core.Constants.CountryCodes.KoreaSouth, invoiceLine.US_UC_NKCountryOfExport);

			invoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.China;
			invoice.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Canada;
			AssertEquals(Core.Constants.CountryCodes.China, invoiceLine.US_UC_NKCountryOfExport);
		}

		public void TestIsCountryOfOriginCanada()
		{
			JobComInvoiceHeader invoice = Factory.New<JobComInvoiceHeader>();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(false, invoiceLine.IsCountryOfOriginCanada);

			invoice.US_UC_NKCountryOfOrigin = CanadaProvinceTerritoryCodes.Codes.XA;
			AssertEquals(true, invoiceLine.IsCountryOfOriginCanada);

			invoice.US_UC_NKCountryOfOrigin = CanadaProvinceTerritoryCodes.Codes.XS;
			AssertEquals(true, invoiceLine.IsCountryOfOriginCanada);

			invoice.US_UC_NKCountryOfOrigin = "CA";
			AssertEquals(true, invoiceLine.IsCountryOfOriginCanada);

			invoice.US_UC_NKCountryOfOrigin = "AU";
			AssertEquals(false, invoiceLine.IsCountryOfOriginCanada);

			invoice.US_UC_NKCountryOfOrigin = CanadaProvinceTerritoryCodes.Codes.XD;
			AssertEquals("For Softwood Lumber from British Columbia, code must be XD (Coastal) or XE (Interior) rather than BC's code of XC", true, invoiceLine.IsCountryOfOriginCanada);

			invoice.US_UC_NKCountryOfOrigin = CanadaProvinceTerritoryCodes.Codes.XE;
			AssertEquals("For Softwood Lumber from British Columbia, code must be XD (Coastal) or XE (Interior) rather than BC's code of XC", true, invoiceLine.IsCountryOfOriginCanada);

			invoice.US_UC_NKCountryOfOrigin = "XJ";
			AssertEquals(false, invoiceLine.IsCountryOfOriginCanada);
		}

		public void TestParent()
		{
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			AssertNull(invoiceLine.ParentTariffLine);
			invoiceLine.JI_ParentID = invoiceLine.PK;//users can set this way.

			AssertNull(invoiceLine.ParentTariffLine);
			AssertEquals(0, invoiceLine.ChildLines.Count());
			AssertEquals("IsParentLine", false, invoiceLine.IsParentLine);
			AssertEquals("IsChildLine", false, invoiceLine.IsChildLine);

			invoiceLine.JI_ParentID = ZGuid.Empty;
			var childLine = invoiceLine.AddSecondaryInvoiceLine();
			AssertEquals(invoiceLine, childLine.ParentTariffLine);
			AssertEquals(childLine, invoiceLine.ChildLines.ElementAt(0));
			AssertEquals("IsParentLine", true, invoiceLine.IsParentLine);
			AssertEquals("IsChildLine", true, childLine.IsChildLine);
		}

		public void TestNoDbHitsForChildren()
		{
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var childLine = invoiceLine.AddSecondaryInvoiceLine();
			AssertEquals("(pre-condition)", 1, invoiceLine.ChildLines.Count());
			Factory.Save();

			var otherFactory = NewFactory();
			var declaration2 = otherFactory.Load<JobDeclaration>(Declaration.PK);
			var invoice2 = declaration2.Invoices.Single(i => i.PK == invoice.PK);
			var invoiceLine2 = (JobComInvoiceLine)invoice2.InvoiceLines.Single(l => l.PK == invoiceLine.PK);

			using (AssertDbHitsForAllFactories(ignoreUnspecified: false, expectedHitCounts: new Dictionary<string, int>
			{
				{ JobComInvoiceLineSchema.Constants.TableName, 0 }
			}))
			{
				AssertEquals("Children.Count", 1, invoiceLine2.ChildLines.Count());
			}
		}

		public void TestCountryOfExportEffectiveRefCountry()
		{
			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.US_UC_NKCountryOfExport = "AD";
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			AssertEquals("AD", invoiceLine.CountryOfExportEffectiveRefCountry.RN_Code);

			invoiceLine.US_UC_NKCountryOfExport = "IT";
			AssertEquals("IT", invoiceLine.CountryOfExportEffectiveRefCountry.RN_Code);
		}

		[TestDate(1971, 9, 18)]
		public void TestEffectiveDateForDutyRateOverride()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertEquals(ZDate.BrettsBirthday, InvoiceLine.EffectiveDateForDutyRate);
			InvoiceLine.US_PrivilegedStatusDate = new ZDate(1965, 5, 30);
			AssertEquals(ZDate.BrettsBirthday, InvoiceLine.EffectiveDateForDutyRate);
			InvoiceLine.US_ZoneStatus = ZoneStatusList.Codes.PrivilegedForeign;
			AssertEquals(new ZDate(1965, 5, 30), InvoiceLine.EffectiveDateForDutyRate);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(ZDate.BrettsBirthday, InvoiceLine.EffectiveDateForDutyRate);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			Declaration.JE_DateOfArrival = new ZDateTime(2009, 1, 1);
			AssertEquals("EffectiveDateForDuty for FTZ", new ZDateTime(2009, 1, 1), InvoiceLine.EffectiveDateForDutyRate);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			Declaration.JE_DateOfArrival = ZDateTime.Empty;
			Declaration.US_EntryDate = new ZDateTime(2020, 05, 19);
			AssertEquals("EffectiveDateForDuty for Drawback", new ZDateTime(2020, 05, 19), InvoiceLine.EffectiveDateForDutyRate);

			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			JobComInvoiceHeader invoice = reconDeclaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals("EffectiveDateForDutyRate", ZDate.Empty, invoiceLine.EffectiveDateForDutyRate);

			ReconOriginalEntryHeader reconEntry = reconDeclaration.OriginalEntries.AddNew();
			reconEntry.US_R_DutyRateDate = new ZDateTime(2008, 1, 1);
			invoice.US_CH_ReconEntry = reconEntry.CH_PK;
			AssertEquals("EffectiveDateForDuty for recon", new ZDateTime(2008, 1, 1), invoiceLine.EffectiveDateForDutyRate);
		}

		[TestDate(1971, 9, 18)]
		public void TestITDateEffectiveDateForDutyRate()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_MasterBill = "Bodgy";
			declaration.US_ITDate = new ZDate(1965, 5, 30);
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(new ZDate(1965, 5, 30), invoiceLine.EffectiveDateForDutyRate);
		}

		public void TestEntryNumberAndMergeLineNumber()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			//JI_Calc_EntryNumber comes from CusEntryLine.CL_Calc_EntryNumber -> CusEntryHeader.EntryNumber
			entryHeader.EntryNumber = "234";
			AssertEquals("Entry Number and Merge Line Number. In messages, entry line numbers are three-length", "234/001", invoiceLine.EntryNumberAndMergeLineNumber);
		}

		public void TestGetNewValidation()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertEquals(typeof(FormalImportJobComInvoiceLineValidation), InvoiceLine.Validation.GetType());
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(typeof(ExportJobComInvoiceLineValidation), InvoiceLine.Validation.GetType());
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			AssertEquals(typeof(ACEDrawbackJobComInvoiceLineValidation), InvoiceLine.Validation.GetType());
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Miscellaneous;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertEquals(typeof(FormalImportJobComInvoiceLineValidation), InvoiceLine.Validation.GetType());

			JobComInvoiceLine orphanLine = Factory.New<JobComInvoiceLine>();
			AssertEquals(typeof(ExportJobComInvoiceLineValidation), orphanLine.Validation.GetType());

			ReconDeclaration reconDeclaration = new ReconDeclaration(Declaration);
			AssertEquals(typeof(ReconJobComInvoiceLineValidation), InvoiceLine.Validation.GetType());
		}

		public void TestTariffMarkedForReferenceFileRequest()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertEquals(false, InvoiceLine.TariffMarkedForReferenceFileRequest);

			InvoiceLine.JI_Tariff = "00000000";
			AssertEquals(false, InvoiceLine.TariffMarkedForReferenceFileRequest);

			InvoiceLine.JI_Tariff = "1101000011";
			AssertEquals(true, InvoiceLine.TariffMarkedForReferenceFileRequest);

			InvoiceLine.JI_Tariff = "1101000012";
			AssertEquals(true, InvoiceLine.TariffMarkedForReferenceFileRequest);

			InvoiceLine.JI_Tariff = "00000000";
			AssertEquals(false, InvoiceLine.TariffMarkedForReferenceFileRequest);
			InvoiceLine.JI_Tariff = "00000000";
			InvoiceLine.JI_Volume = 10;
			AssertEquals(false, InvoiceLine.TariffMarkedForReferenceFileRequest);

			InvoiceLine.JI_Tariff = "1010101012";
			AssertEquals(true, InvoiceLine.TariffMarkedForReferenceFileRequest);

			InvoiceLine.JI_Tariff = "101010101";
			AssertEquals(false, InvoiceLine.TariffMarkedForReferenceFileRequest);

			InvoiceLine.TariffMarkedForReferenceFileRequest = true;
			AssertEquals(true, InvoiceLine.TariffMarkedForReferenceFileRequest);
		}

		public void TestReCalculateWhenChildIsDeleted()
		{
			JobComInvoiceLine secondary = InvoiceLine.AddSecondaryInvoiceLine();
			AssertEquals(1, InvoiceLine.ChildLines.Count());

			secondary.Delete();
			AssertEquals(0, InvoiceLine.ChildLines.Count());
		}

		[TestDate(2019, 11, 11)]
		public void TestApportionmentForDifferentInvoicesWithDifferentCurrencies()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceAmount = 252485m;
			invoice1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 252485m;
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceAmount = 983100m;
			invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Japan;
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 983100m;
			declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew(USCustomsChargeTypeList.Codes.OverseasFreight, 5233m, Core.Constants.CurrencyCodes.UnitedStates);

			var rate = new RefExchangeRate.Loader(Factory).GetEffectiveRateOn(new ZDateTime(2019, 11, 11), Core.Constants.CurrencyCodes.Japan, Core.Constants.ExchangeRateTypes.Code.CustomsRate, GlbCompany.CurrentCompany.PK);
			if (rate != null)
			{
				rate.RE_SellRate = 0.009285m;
			}
			else
			{
				var rateJPY = Factory.New<RefExchangeRate>();
				rateJPY.RE_RX_NKExCurrency = Core.Constants.CurrencyCodes.Japan;
				rateJPY.RE_StartDate = new ZDateTime(2019, 11, 1);
				rateJPY.RE_ExpiryDate = new ZDateTime(2019, 11, 30);
				rateJPY.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
				rateJPY.RE_SellRate = 0.009285m;
			}
			Factory.Save();

			declaration.ResumeApportionment();
			AssertEquals("Apportioned freight for Invoice 1", 5050.41m, invoice1.GroupCharges.GetCharge(USCustomsChargeTypeList.Codes.OverseasFreight, JobDeclaration.GetLocalCurrency()));
			AssertEquals("Apportioned freight for Invoice Line 1", 5050.41m, invoiceLine1.ApportionedCharges.GetCharge(USCustomsChargeTypeList.Codes.OverseasFreight, JobDeclaration.GetLocalCurrency()));
			AssertEquals("Apportioned freight for Invoice 2", 182.59m, invoice2.GroupCharges.GetCharge(USCustomsChargeTypeList.Codes.OverseasFreight, JobDeclaration.GetLocalCurrency()));
			AssertEquals("Apportioned freight for Invoice Line 3", 182.59m, invoiceLine2.ApportionedCharges.GetCharge(USCustomsChargeTypeList.Codes.OverseasFreight, JobDeclaration.GetLocalCurrency()));
		}

		public void TestApportionmentForCombinedLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 20000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 200m, Core.Constants.CurrencyCodes.UnitedStates);
			invoice.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, 100m, Core.Constants.CurrencyCodes.UnitedStates);

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 10000m;
			invoiceLine1.JI_Tariff = "8544300000";
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.US_98GoodsValue = 9000m;
			invoiceLine2.US_SupTariff = "9802004040";
			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 1000m;
			invoiceLine3.JI_Tariff = "8544300000";
			invoiceLine3.US_SupTariff = "99038801";
			invoiceLine3.JI_ParentID = invoiceLine2.PK;

			declaration.ResumeApportionment();
			AssertEquals("Apportioned freight for Line 1", 100m, invoiceLine1.ApportionedCharges.GetCharge("OFT", JobDeclaration.GetLocalCurrency()));
			AssertEquals("Apportioned insurance for Line 1", 50m, invoiceLine1.ApportionedCharges.GetCharge("ONS", JobDeclaration.GetLocalCurrency()));
			AssertEquals("Apportioned freight for Line 1", 0m, invoiceLine2.ApportionedCharges.GetCharge("OFT", JobDeclaration.GetLocalCurrency()));
			AssertEquals("Apportioned insurance for Line 1", 0m, invoiceLine2.ApportionedCharges.GetCharge("ONS", JobDeclaration.GetLocalCurrency()));
			AssertEquals("Apportioned freight for Line 3", 100m, invoiceLine3.ApportionedCharges.GetCharge("OFT", JobDeclaration.GetLocalCurrency()));
			AssertEquals("Apportioned insurance for Line 3", 50m, invoiceLine3.ApportionedCharges.GetCharge("ONS", JobDeclaration.GetLocalCurrency()));
		}

		public void TestMarkApportionmentDirtyWhenComponentPriceChanges()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				JobDeclaration declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

				JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceAmount = 1200m;
				invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
				declaration.ResumeApportionment();
				invoice.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 200m, Core.Constants.CurrencyCodes.UnitedStates);

				JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
				invoiceLine.JI_LinePrice = 1200m;
				declaration.ResumeApportionment();
				AssertEquals("Apportioned freight", 200m, invoiceLine.ApportionedCharges.GetCharge("OFT", JobDeclaration.GetLocalCurrency()));

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

				invoiceLine.JI_LinePrice = 0m;
				declaration.ResumeApportionment();
				AssertEquals("Apportioned freight", 0m, invoiceLine.ApportionedCharges.GetCharge("OFT", JobDeclaration.GetLocalCurrency()));

				invoiceLine.US_98GoodsValue = 1200m;
				declaration.ResumeApportionment();
				AssertEquals("Apportioned freight", 200m, invoiceLine.ApportionedCharges.GetCharge("OFT", JobDeclaration.GetLocalCurrency()));

				invoiceLine.US_98GoodsValue = 0m;
				declaration.ResumeApportionment();
				AssertEquals("Apportioned freight", 0m, invoiceLine.ApportionedCharges.GetCharge("OFT", JobDeclaration.GetLocalCurrency()));

				invoiceLine.US_98ValueInvCurr = 1200m;
				declaration.ResumeApportionment();
				AssertEquals("Apportioned freight", 200m, invoiceLine.ApportionedCharges.GetCharge("OFT", JobDeclaration.GetLocalCurrency()));
			}
		}

		public void TestLinePriceForWeightApportionCalculation()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 100m;
			invoiceLine.US_98GoodsValue = 10m;
			AssertEquals("LinePriceForWeightApportionCalculation", 110m, invoiceLine.LinePriceForWeightApportionCalculation);

			invoiceLine.US_98ValueInvCurr = 1m;
			AssertEquals("LinePriceForWeightApportionCalculation", 111m, invoiceLine.LinePriceForWeightApportionCalculation);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("LinePriceForWeightApportionCalculation", 100m, invoiceLine.LinePriceForWeightApportionCalculation);
		}

		public void TestApportionmentForXVVLinesWithAdditionalTariffs()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_IncoTerm = TermsOfDeliveryList.Codes.FOB;
			invoice.Charges.RemoveAndDeleteAll();
			invoice.Charges.AddNew(USCustomsChargeTypeList.Codes.OverseasFreight, 2000m, Core.Constants.CurrencyCodes.UnitedStates);

			var xInvoiceLine = invoice.JobComInvoiceLines.AddNew();
			xInvoiceLine.JI_FormattedTariff = "9018.31.0080";
			xInvoiceLine.SupTariffFormatted = "9903.91.03";
			xInvoiceLine.JI_LinePrice = 0m;
			xInvoiceLine.US_SetInd = "X";
			var vParentLine = invoice.JobComInvoiceLines.AddNew();
			vParentLine.JI_ParentID = xInvoiceLine.PK;
			vParentLine.US_SetInd = "V";
			vParentLine.JI_FormattedTariff = "9018.31.0080";
			vParentLine.SupTariffFormatted = "9903.91.03";
			vParentLine.JI_LinePrice = 300m;
			var vChildLine1 = invoice.JobComInvoiceLines.AddNew();
			vChildLine1.JI_ParentID = xInvoiceLine.PK;
			vChildLine1.US_SetInd = "V";
			vChildLine1.JI_FormattedTariff = "4818.20.0020";
			vChildLine1.SupTariffFormatted = "9903.88.03";
			vChildLine1.JI_LinePrice = 200m;
			var vChildLine2 = invoice.JobComInvoiceLines.AddNew();
			vChildLine2.JI_ParentID = xInvoiceLine.PK;
			vChildLine2.US_SetInd = "V";
			vChildLine2.JI_FormattedTariff = "3926.90.9910";
			vChildLine2.SupTariffFormatted = "9903.88.69";
			vChildLine2.JI_LinePrice = 400m;
			var vChildLine3 = invoice.JobComInvoiceLines.AddNew();
			vChildLine3.JI_ParentID = xInvoiceLine.PK;
			vChildLine3.US_SetInd = "V";
			vChildLine3.JI_FormattedTariff = "5603.92.0010";
			vChildLine3.SupTariffFormatted = "9903.88.03";
			vChildLine3.JI_LinePrice = 600m;

			declaration.ResumeApportionment();
			AssertEquals("Apportioned freight charge for X line", 0m, xInvoiceLine.ApportionedCharges.GetCharge("OFT", JobDeclaration.GetLocalCurrency()));
			AssertEquals("Apportioned freight charge for V parent line", 400m, vParentLine.ApportionedCharges.GetCharge("OFT", JobDeclaration.GetLocalCurrency()));
			AssertEquals("Apportioned freight charge for V child line 1", 266.67m, vChildLine1.ApportionedCharges.GetCharge("OFT", JobDeclaration.GetLocalCurrency()));
			AssertEquals("Apportioned freight charge for V child line 2", 533.33m, vChildLine2.ApportionedCharges.GetCharge("OFT", JobDeclaration.GetLocalCurrency()));
			AssertEquals("Apportioned freight charge for V child line 3", 800m, vChildLine3.ApportionedCharges.GetCharge("OFT", JobDeclaration.GetLocalCurrency()));

			xInvoiceLine.SupFormattedAdditionalTariff1 = "9903.01.23";
			vParentLine.SupFormattedAdditionalTariff1 = "9903.01.23";
			vChildLine1.SupFormattedAdditionalTariff1 = "9903.01.23";
			vChildLine2.SupFormattedAdditionalTariff1 = "9903.01.23";
			vChildLine3.SupFormattedAdditionalTariff1 = "9903.01.23";
			declaration.ResumeApportionment();
			AssertEquals("Apportioned freight charge for X line", 0m, xInvoiceLine.ApportionedCharges.GetCharge("OFT", JobDeclaration.GetLocalCurrency()));
			AssertEquals("Apportioned freight charge for V parent line", 400m, vParentLine.ApportionedCharges.GetCharge("OFT", JobDeclaration.GetLocalCurrency()));
			AssertEquals("Apportioned freight charge for V child line 1", 266.67m, vChildLine1.ApportionedCharges.GetCharge("OFT", JobDeclaration.GetLocalCurrency()));
			AssertEquals("Apportioned freight charge for V child line 2", 533.33m, vChildLine2.ApportionedCharges.GetCharge("OFT", JobDeclaration.GetLocalCurrency()));
			AssertEquals("Apportioned freight charge for V child line 3", 800m, vChildLine3.ApportionedCharges.GetCharge("OFT", JobDeclaration.GetLocalCurrency()));
		}

		public void TestCustomsValueForReconJob()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var reconDec = new ReconDeclaration(declaration);
			reconDec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var entry = reconDec.OriginalEntries.AddNew();
			var invoice = entry.Invoice;
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			invoiceLine2.JI_ParentID = invoiceLine1.PK;

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			invoiceLine3.JI_ParentID = invoiceLine1.PK;

			invoiceLine1.JI_LinePrice = 3000m;
			invoiceLine2.JI_LinePrice = 1000m;
			invoiceLine3.JI_LinePrice = 500m;
			AssertEquals(3000m, invoiceLine1.JI_CustomsValue);

			invoiceLine1.US_98GoodsValue = 500m;
			invoiceLine2.US_98GoodsValue = 700;
			invoiceLine3.US_98GoodsValue = 300;
			AssertEquals(500m, invoiceLine1.TotalOriginalGoodsValueInUSD);

			invoiceLine1.US_CustomsValue = 100m;
			invoiceLine2.US_CustomsValue = 20m;
			invoiceLine3.US_CustomsValue = 10m;
			AssertEquals(600m, invoiceLine1.TotalCustomsValueIncludingSecondaryLines);
		}

		public void TestCustomsValueAndOriginalGoodsValueForXVVLines()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = "CAF";
			invoice.JZ_InvoiceAmount = 27720.00m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			BaseJobComInvHeaderCharge freight = invoice.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 200m, Core.Constants.CurrencyCodes.UnitedStates);
			freight.J7_IsIncludedInITOT = true;

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 200m;
			invoiceLine1.JI_Tariff = "6110.20.2079";
			invoiceLine1.US_SupTariff = "9802.80.68";
			invoiceLine1.US_UC_NKCountryOfOrigin = "NI";
			invoiceLine1.US_98GoodsValue = 27720.0m;
			invoiceLine1.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 400m;
			invoiceLine2.JI_Tariff = "6110.20.2079";
			invoiceLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			invoiceLine2.US_98GoodsValue = 800m;
			invoiceLine2.JI_ParentID = invoiceLine1.PK;

			JobComInvoiceLine invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 40m;
			invoiceLine3.JI_Tariff = "6110.20.2079";
			invoiceLine3.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			invoiceLine3.US_98GoodsValue = 80m;
			invoiceLine3.JI_ParentID = invoiceLine2.PK;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals("CustomsValue", 101.82m, invoiceLine1.JI_CustomsValue);
			AssertEquals("CustomsValue", 1018.18m, invoiceLine2.JI_CustomsValue);
			AssertEquals("CustomsValue", 101.82m, invoiceLine3.JI_CustomsValue);
			AssertEquals("TotalGoodsValue for X", 80m, invoiceLine1.TotalOriginalGoodsValueInUSD);
			AssertEquals("TotalGoodsValue for V1", 800m, invoiceLine2.TotalOriginalGoodsValueInUSD);
			AssertEquals("TotalGoodsValue for V2", 80m, invoiceLine3.TotalOriginalGoodsValueInUSD);
		}

		public void TestApportionIsBasedOnComponentPrice()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = "CAF";
			invoice.JZ_InvoiceAmount = 27720.00m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			BaseJobComInvHeaderCharge freight = invoice.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 200m, Core.Constants.CurrencyCodes.UnitedStates);
			freight.J7_IsIncludedInITOT = true;

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 0m;
			invoiceLine1.JI_Tariff = "6110.20.2079";
			invoiceLine1.US_SupTariff = "9802.80.68";
			invoiceLine1.US_UC_NKCountryOfOrigin = "NI";
			invoiceLine1.US_98GoodsValue = 27720.0m;

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 0m;
			invoiceLine2.JI_Tariff = "6110.20.2079";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals(200m, invoiceLine1.ApportionedCharges.GetCharge("OFT", JobDeclaration.GetLocalCurrency()));
			AssertEquals(0m, invoiceLine2.ApportionedCharges.GetCharge("OFT", JobDeclaration.GetLocalCurrency()));
			AssertEquals("CustomsValue", 27520.0m, invoiceLine1.JI_CustomsValue);
			AssertEquals("CustomsValue", 0m, invoiceLine2.JI_CustomsValue);
		}

		public void TestTotal98GoodsValueForXAndVLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 200m;
			invoiceLine1.US_SupTariff = "9802.80.68";
			invoiceLine1.US_98GoodsValue = 100m;
			invoiceLine1.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;

			var invoiceLine2 = invoiceLine1.AddSecondaryInvoiceLine();
			invoiceLine2.JI_LinePrice = 400m;
			invoiceLine2.JI_Tariff = "6110.20.2079";
			invoiceLine2.US_98GoodsValue = 800m;
			invoiceLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;

			var invoiceLine3 = invoiceLine1.AddSecondaryInvoiceLine();
			invoiceLine3.JI_LinePrice = 40m;
			invoiceLine3.JI_Tariff = "6110.20.2079";
			invoiceLine3.US_98GoodsValue = 80m;
			invoiceLine3.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;

			AssertEquals("TotalGoodsValue for X", 880m, invoiceLine1.TotalOriginalGoodsValueInUSD);
			AssertEquals("TotalGoodsValue for V1", 800m, invoiceLine2.TotalOriginalGoodsValueInUSD);
			AssertEquals("TotalGoodsValue for V2", 80m, invoiceLine3.TotalOriginalGoodsValueInUSD);
		}

		public void TestIncidentCS00085422Issue()
		{
			DeclarationTestHelper helper = new DeclarationTestHelper(Factory);
			OrgHeader supplier = helper.Consignee;
			OrgHeader importer = helper.Consignor;

			CusClassification classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "LOOKUP123654";
			classification.CC_TariffNum = "1010101010";
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;

			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PART123654";
			part.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, OrgPartRelation.RelationshipTypes.Supplier);
			part.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);

			CusClassPartPivot importPivot = part.PivotsForBinding.AddNew();
			importPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			importPivot.CI_CC = classification.PK;

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			JobComInvoiceHeader invoice = newFactory.New<JobComInvoiceHeader>();
			invoice.JZ_MessageType = JobMessageTypeList.Codes.Import;
			var fakeDeclaration = (JobDeclaration)new FakeDeclarationCreatorForInvoice(invoice).HeaderData;
			invoice.JZ_OH_Supplier = supplier.PK;
			invoice.JZ_OH_Buyer = importer.PK;
			AssertEquals(JobMessageTypeList.Codes.Import, fakeDeclaration.JE_MessageType);
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.LoadChildEditableObjects();
			AssertEquals(0, invoiceLine.FDAs.Count);
			AssertEquals(0, invoiceLine.FCCs.Count);
			invoiceLine.JI_PartNo = part.OP_PartNum;
			newFactory.Save();

			BusinessObjectFactory newFactory2 = new BusinessObjectFactory();
			newFactory2.AllowMultipleBusinessObjectsAroundOneRow = false;
			JobComInvoiceHeader invoiceLoaded = newFactory2.Load<JobComInvoiceHeader>(invoice.PK);
			_ = new FakeDeclarationCreatorForInvoice(invoiceLoaded).HeaderData;
		}

		public void TestFetchHintIsCorrectlyUse()
		{
			JobComInvoiceHeader invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_StandAloneInvoiceDirection = JobMessageTypeList.Codes.Import;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			PGA pga = invoiceLine.LaceyActLines.AddNew();
			pga.US_PGALineValue = 1m;
			FDA fda = invoiceLine.FDAs.AddNew();
			fda.US_OFT = "A";
			DOT dot = invoiceLine.DOTs.AddNew();
			dot.US_DOTBoxNo = "A";
			FCC fcc = invoiceLine.FCCs.AddNew();
			fcc.US_FCCID = "A";
			InvoiceLineGroupingRange range = invoiceLine.LineGroupingRanges.AddNew(1, 1);
			AIILine aiiLine = invoiceLine.AIILines.AddNew(range);
			aiiLine.US_LineNo = 1;
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			newFactory.AllowMultipleBusinessObjectsAroundOneRow = false;
			JobComInvoiceLine invoiceLineLoaded = newFactory.Load<JobComInvoiceLine>(invoiceLine.PK);
			AssertEquals(1, invoiceLineLoaded.AIILines.Count);
			AssertEquals(1, invoiceLineLoaded.DOTs.Count);
			AssertEquals(1, invoiceLineLoaded.FCCs.Count);
			AssertEquals(1, invoiceLineLoaded.FDAs.Count);
			AssertEquals(1, invoiceLineLoaded.LaceyActLines.Count);
		}

		public void TestEffectiveValuesNotAppliedForXAndVLines()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SPI = "AU";
			invoiceLine.US_SecondarySPI = "X";

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_ParentID = invoiceLine.PK;
			AssertEquals("PreCondition", SecondarySpecProgIndicatorList.Codes.V, invoiceLine2.US_SecondarySPI);

			AssertEquals("SPI should not fall back on to parent line for V lines (Origin might be different and declared in 40 record)", "", invoiceLine2.US_SPI);

			invoiceLine.US_SecondarySPI = "";
			AssertEquals("PreCondition", "", invoiceLine.US_SecondarySPI);
			AssertEquals("SPI now falls back to parent line now", "AU", invoiceLine.US_SPI);
		}

		public void TestFDAValueRunningTotal()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 100m;
			FDA fdaLine1 = invoiceLine.FDAs.AddNew();
			fdaLine1.US_InvCurrFDAValue = 100m;
			declaration.ResumeApportionment();

			AssertEquals(true, invoiceLine.HasFDAData);
			AssertEquals(100m, invoiceLine.FDAValueInvCurrRunningTotal);
			AssertEquals("100", invoiceLine.FDAValueUSDRunningTotalString);

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("100", invoiceLine.FDAValueUSDRunningTotalString);

			invoiceLine.JI_LinePrice = 350m;
			fdaLine1.US_InvCurrFDAValue = 100m;
			FDA fdaLine2 = invoiceLine.FDAs.AddNew();
			fdaLine2.US_InvCurrFDAValue = 250m;
			declaration.ResumeApportionment();
			AssertEquals(350m, invoiceLine.FDAValueInvCurrRunningTotal);
			AssertEquals("...", invoiceLine.FDAValueUSDRunningTotalString);

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("350", invoiceLine.FDAValueUSDRunningTotalString);
		}

		public void TestFDAValueRunningTotalsNonUSD()
		{
			using (DeclarationTestHelper.SetReciprocalFlagForCurrentCompany(true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
				declaration.US_EntryFilerCode = "XJ5";
				declaration.US_EnableENS = true;
				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_RX_NKInvoice_Currency = "SGD";
				invoice.JZ_InvoiceCurrExRate = 0.5m;
				invoice.IsJZ_InvoiceCurrExRateUserEnterable = true;
				JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

				FDA fdaLine1 = invoiceLine.FDAs.AddNew();
				fdaLine1.US_InvCurrFDAValue = 100m;
				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals(true, invoiceLine.HasFDAData);
				AssertEquals(100m, invoiceLine.FDAValueInvCurrRunningTotal);
				AssertEquals(50m, invoiceLine.FDAValueUSDRunningTotal);

				FDA fdaLine2 = invoiceLine.FDAs.AddNew();
				fdaLine2.US_InvCurrFDAValue = 250m;
				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals(350m, invoiceLine.FDAValueInvCurrRunningTotal);
				AssertEquals(175m, invoiceLine.FDAValueUSDRunningTotal);
			}
		}

		public void TestCustomsUQsReadOnly()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertEquals("First Customs Qty readonly", true, InvoiceLine.JI_CustomsUnitQtyInfo.ReadOnly);
			AssertEquals("JI_CustomsSecondUnitQty readonly", true, InvoiceLine.JI_CustomsSecondUnitQtyInfo.ReadOnly);
			AssertEquals("JI_CustomsThirdUnitQty readonly", true, InvoiceLine.JI_CustomsThirdUnitQtyInfo.ReadOnly);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.US_TariffType = TariffTypeList.Codes.ScheduleB;
			AssertEquals("First Customs Qty cannot be editable for Schd B", true, InvoiceLine.JI_CustomsUnitQtyInfo.ReadOnly);
			AssertEquals("JI_CustomsSecondUnitQty readonly", true, InvoiceLine.JI_CustomsSecondUnitQtyInfo.ReadOnly);
			AssertEquals("JI_CustomsThirdUnitQty readonly always for export", true, InvoiceLine.JI_CustomsThirdUnitQtyInfo.ReadOnly);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusTariffTypeSB = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.ScheduleB);
			Factory.Save();

			var scheduleB = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, cusTariffTypeSB.PK, "0101100001", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			helper.CreateTariffUOM(scheduleB, "CU1", "X");
			helper.CreateTariffUOM(scheduleB, "CU2", "KG");

			InvoiceLine.JI_Tariff = scheduleB.ZZ1_TariffCode;
			AssertEquals("First Customs Qty cannot be editable for Schd B", true, InvoiceLine.JI_CustomsUnitQtyInfo.ReadOnly);
			AssertEquals("JI_CustomsSecondUnitQty readonly", true, InvoiceLine.JI_CustomsSecondUnitQtyInfo.ReadOnly);
			AssertEquals("JI_CustomsThirdUnitQty readonly", true, InvoiceLine.JI_CustomsThirdUnitQtyInfo.ReadOnly);

			var scheduleB2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, cusTariffTypeSB.PK, "0101100002", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));

			InvoiceLine.JI_Tariff = scheduleB2.ZZ1_TariffCode;
			AssertEquals("First Customs Qty cannot be editable for Schd B", true, InvoiceLine.JI_CustomsUnitQtyInfo.ReadOnly);
			AssertEquals("JI_CustomsSecondUnitQty readonly", true, InvoiceLine.JI_CustomsSecondUnitQtyInfo.ReadOnly);
			AssertEquals("JI_CustomsThirdUnitQty readonly always for export", true, InvoiceLine.JI_CustomsThirdUnitQtyInfo.ReadOnly);

			InvoiceLine.JI_Tariff = ZString.Empty;
			Declaration.US_TariffType = TariffTypeList.Codes.HTS;
			AssertEquals("First Customs Qty readonly", true, InvoiceLine.JI_CustomsUnitQtyInfo.ReadOnly);
			AssertEquals("JI_CustomsSecondUnitQty readonly", true, InvoiceLine.JI_CustomsSecondUnitQtyInfo.ReadOnly);
			AssertEquals("JI_CustomsThirdUnitQty readonly", true, InvoiceLine.JI_CustomsThirdUnitQtyInfo.ReadOnly);

			var cusTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.Export);
			Factory.Save();
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, cusTariffType.PK, "12345678", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			var tariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, cusTariffType.PK, "23456789", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateTariffUOM(tariff2, "CU1", "L");
			helper.CreateTariffUOM(tariff2, "CU2", "M");
			InvoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;
			AssertEquals("First Customs Qty readonly", true, InvoiceLine.JI_CustomsUnitQtyInfo.ReadOnly);
			AssertEquals("JI_CustomsSecondUnitQty readonly", true, InvoiceLine.JI_CustomsSecondUnitQtyInfo.ReadOnly);
			AssertEquals("JI_CustomsThirdUnitQty readonly", true, InvoiceLine.JI_CustomsThirdUnitQtyInfo.ReadOnly);
			InvoiceLine.JI_Tariff = tariff2.ZZ1_TariffCode;
			AssertEquals("First Customs Qty readonly", true, InvoiceLine.JI_CustomsUnitQtyInfo.ReadOnly);
			AssertEquals("JI_CustomsSecondUnitQty readonly", true, InvoiceLine.JI_CustomsSecondUnitQtyInfo.ReadOnly);
			AssertEquals("JI_CustomsThirdUnitQty readonly", true, InvoiceLine.JI_CustomsThirdUnitQtyInfo.ReadOnly);

			InvoiceLine.JI_Tariff = ZString.Empty;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			AssertEquals("First Customs Qty readonly", false, InvoiceLine.JI_CustomsUnitQtyInfo.ReadOnly);
			AssertEquals("JI_CustomsSecondUnitQty readonly", true, InvoiceLine.JI_CustomsSecondUnitQtyInfo.ReadOnly);
			AssertEquals("JI_CustomsThirdUnitQty readonly", true, InvoiceLine.JI_CustomsThirdUnitQtyInfo.ReadOnly);
		}

		public void TestFirstVLine()
		{
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			InvoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			var firstVline = InvoiceLine.AddSecondaryInvoiceLine();
			var secondVline = InvoiceLine.AddSecondaryInvoiceLine();

			AssertEquals("Precondition Line number of first Line < line number of second Line", true, firstVline.JI_LineNo < secondVline.JI_LineNo);
			AssertEquals("First V line", firstVline, InvoiceLine.FirstVLine);
			firstVline.JI_LineNo = 3;
			secondVline.JI_LineNo = 2;
			AssertEquals("Precondition Line number of first Line > line number of second Line", true, firstVline.JI_LineNo > secondVline.JI_LineNo);
			AssertEquals("First V line", secondVline, InvoiceLine.FirstVLine);
		}

		public void TestIsQuantityRequiredForFTZ()
		{
			//----------------FTZFT50-----------------
			//LINE ITEM NUMBER (3-7)                   :1
			//HARMONIZED TARIFF SCHEDULE NUMBER (8-17) :0301100010
			//COUNTRY OF ORIGIN (22-23)                :GB
			//QUANTITY1 (24-35)                        :12
			//UNIT OF MEASURE1 (36-38)                 :X
			//QUANTITY2 (39-50)                        :0

			//----------------FTZNF95-----------------
			//ERROR CODE (3-7) :083
			//REMARKS (8-67)   :ZONE ADMISSION DATA ACCEPTED

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;

			var invoice = declaration.Invoices.AddNew();
			invoice.US_IsLineGrouping = false;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1234567890";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);
			tariff.UE_Unit1 = "X";
			tariff.UE_Unit2 = "Y";
			tariff.UE_Unit3 = "KG";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			AssertEquals("Qty should be readonly UQ = 'X', Manifest Qty is reported in msg", true, invoiceLine.JI_CustomsQuantity_ReadOnly);
		}

		public void TestDeclarationNullReference()
		{
			var testObj = Factory.New<JobComInvoiceLine>();
			AssertNoExceptionThrown("Expected no exception thrown but exception was thrown out", () => testObj.US_SupTariff = "9802");
		}

		public void TestFeeCusCodeInAscendingCodeOrder()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			FeeCusCodeData fee1 = invoiceLine.FeeCusCodes.AddNew();
			fee1.CY_Code = "GHI";
			fee1.CY_FeeAmount = 123m;
			FeeCusCodeData fee2 = invoiceLine.FeeCusCodes.AddNew();
			fee2.CY_Code = "ABC";
			fee2.CY_FeeAmount = 456m;
			FeeCusCodeData fee3 = invoiceLine.FeeCusCodes.AddNew();
			fee3.CY_Code = "DEF";
			fee3.CY_FeeAmount = 222m;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			invoiceLine = newFactory.Load<JobComInvoiceLine>(invoiceLine.PK);

			AssertEquals("Code 1", "ABC", invoiceLine.FeeCusCodes[0].CY_Code);
			AssertEquals("Code 2", "DEF", invoiceLine.FeeCusCodes[1].CY_Code);
			AssertEquals("Code 3", "GHI", invoiceLine.FeeCusCodes[2].CY_Code);
		}

		public void TestCopyWithProduct()
		{
			var owner = Factory.New<OrgHeader>();
			owner.OH_IsConsignee = true;
			owner.OH_Code = "OWNER";

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PARTZ234";
			part.OP_Desc = "Description";
			var relation = part.RelatedOrganisations.AddOrganisationIfNotExist(owner.PK, OrgPartRelation.RelationshipTypes.Both);

			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_MessageType = JobMessageTypeList.Codes.Import;
			invoice.JZ_OH_Buyer = owner.PK;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "PARTZ234";
			AssertNotNull(invoiceLine.Part);

			Factory.Save();

			Factory.AllowMultipleBusinessObjectsAroundOneRow = false;//as set by CommercialInvoiceController
			AssertNoExceptionThrown(delegate
			{ new JobComInvoiceHeaderDeepCopyStrategy(invoice, CloneType.TemplateCopy).Clone(); });
		}

		public void TestReconOriginalChargesInAscendingCodeOrder()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			ReconEntryOriginalCharge recon1 = invoiceLine.ReconOriginalCharges.AddNew();
			recon1.CY_Code = "GHI";
			recon1.CY_Amount = 123m;
			ReconEntryOriginalCharge recon2 = invoiceLine.ReconOriginalCharges.AddNew();
			recon2.CY_Code = "ABC";
			recon2.CY_Amount = 456m;
			ReconEntryOriginalCharge recon3 = invoiceLine.ReconOriginalCharges.AddNew();
			recon3.CY_Code = "DEF";
			recon3.CY_Amount = 222m;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			invoiceLine = newFactory.Load<JobComInvoiceLine>(invoiceLine.PK);

			AssertEquals("Code 1", "ABC", invoiceLine.ReconOriginalCharges[0].CY_Code);
			AssertEquals("Code 2", "DEF", invoiceLine.ReconOriginalCharges[1].CY_Code);
			AssertEquals("Code 3", "GHI", invoiceLine.ReconOriginalCharges[2].CY_Code);
		}

		public void TestWhsFieldsReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_BondedWhsQuantity = 10m;
			invoiceLine1.US_WHSEntryLineNo = 2;
			AssertEquals("JI_BondedWhsQuantity", 10m, invoiceLine1.JI_BondedWhsQuantity);
			AssertEquals("JI_BondedWhsQuantityInfo.ReadOnly", false, invoiceLine1.JI_BondedWhsQuantityInfo.ReadOnly);
			AssertEquals("US_WHSEntryLineNo", (short)2, invoiceLine1.US_WHSEntryLineNo);
			AssertEquals("US_WHSEntryLineNoInfo.ReadOnly", false, invoiceLine1.US_WHSEntryLineNoInfo.ReadOnly);
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_ParentID = invoiceLine2.PK;
			AssertEquals("JI_BondedWhsQuantity", ZDecimal.Zero, invoiceLine1.JI_BondedWhsQuantity);
			AssertEquals("JI_BondedWhsQuantityInfo.ReadOnly", true, invoiceLine1.JI_BondedWhsQuantityInfo.ReadOnly);
			AssertEquals("US_WHSEntryLineNo", ZShort.Zero, invoiceLine1.US_WHSEntryLineNo);
			AssertEquals("US_WHSEntryLineNoInfo.ReadOnly", true, invoiceLine1.US_WHSEntryLineNoInfo.ReadOnly);
			invoiceLine1.JI_ParentID = ZGuid.Empty;
			invoiceLine1.US_JI_ParentProduct = invoiceLine2.PK;
			AssertEquals("JI_BondedWhsQuantity", ZDecimal.Zero, invoiceLine1.JI_BondedWhsQuantity);
			AssertEquals("JI_BondedWhsQuantityInfo.ReadOnly", true, invoiceLine1.JI_BondedWhsQuantityInfo.ReadOnly);
			AssertEquals("US_WHSEntryLineNo", ZShort.Zero, invoiceLine1.US_WHSEntryLineNo);
			AssertEquals("US_WHSEntryLineNoInfo.ReadOnly", true, invoiceLine1.US_WHSEntryLineNoInfo.ReadOnly);
			invoiceLine1.US_JI_ParentProduct = ZGuid.Empty;
			AssertEquals("JI_BondedWhsQuantity", ZDecimal.Zero, invoiceLine1.JI_BondedWhsQuantity);
			AssertEquals("JI_BondedWhsQuantityInfo.ReadOnly", false, invoiceLine1.JI_BondedWhsQuantityInfo.ReadOnly);
			AssertEquals("US_WHSEntryLineNo", ZShort.Zero, invoiceLine1.US_WHSEntryLineNo);
			AssertEquals("US_WHSEntryLineNoInfo.ReadOnly", false, invoiceLine1.US_WHSEntryLineNoInfo.ReadOnly);

			invoiceLine1.JI_BondedWhsQuantity = 10m;
			invoiceLine1.US_WHSEntryLineNo = 2;
			AssertEquals("JI_BondedWhsQuantity", 10m, invoiceLine1.JI_BondedWhsQuantity);
			AssertEquals("JI_BondedWhsQuantityInfo.ReadOnly", false, invoiceLine1.JI_BondedWhsQuantityInfo.ReadOnly);
			AssertEquals("US_WHSEntryLineNo", (short)2, invoiceLine1.US_WHSEntryLineNo);
			AssertEquals("US_WHSEntryLineNoInfo.ReadOnly", false, invoiceLine1.US_WHSEntryLineNoInfo.ReadOnly);

			invoiceLine1.US_JI_ParentProduct = invoiceLine2.PK;
			AssertEquals("JI_BondedWhsQuantity", ZDecimal.Zero, invoiceLine1.JI_BondedWhsQuantity);
			AssertEquals("JI_BondedWhsQuantityInfo.ReadOnly", true, invoiceLine1.JI_BondedWhsQuantityInfo.ReadOnly);
			AssertEquals("US_WHSEntryLineNo", ZShort.Zero, invoiceLine1.US_WHSEntryLineNo);
			AssertEquals("US_WHSEntryLineNoInfo.ReadOnly", true, invoiceLine1.US_WHSEntryLineNoInfo.ReadOnly);
		}

		public void TestIsWHSPackable()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PD3234";
			InvoiceLine.JI_InvoiceQuantity = 1m;
			InvoiceLine.JI_OP = part.PK;
			AssertEquals(true, InvoiceLine.IsWHSPackable);
			InvoiceLine.JI_BondedWhsQuantity = 1m;
			AssertEquals(false, InvoiceLine.IsWHSPackable);
			InvoiceLine.JI_BondedWhsQuantity = ZDecimal.Zero;
			AssertEquals(true, InvoiceLine.IsWHSPackable);
			InvoiceLine.JI_ParentID = ZGuid.Invalid;
			AssertEquals(false, InvoiceLine.IsWHSPackable);
			InvoiceLine.JI_ParentID = ZGuid.Empty;
			AssertEquals(true, InvoiceLine.IsWHSPackable);
			InvoiceLine.JI_OP = ZGuid.Invalid;
			AssertEquals(false, InvoiceLine.IsWHSPackable);
			InvoiceLine.JI_OP = ZGuid.Empty;
			AssertEquals(false, InvoiceLine.IsWHSPackable);
			InvoiceLine.JI_OP = part.PK;
			AssertEquals(true, InvoiceLine.IsWHSPackable);
			InvoiceLine.JI_InvoiceQuantity = 0m;
			AssertEquals(false, InvoiceLine.IsWHSPackable);
		}

		public void TestCalculatedPackData()
		{
			var org = Factory.New<OrgHeader>();
			org.CompanyData.OB_IMUsedBondedWhs = true;
			var classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "LK234";
			classification.CC_TariffNum = "2010304050";
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PART321";
			var orgRel = part.RelatedOrganisations.AddOrganisationIfNotExist(org.PK, OrgPartRelation.RelationshipTypes.Owner);
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_OH = orgRel.OU_OH;
			pivot.CI_CC = classification.PK;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.JE_OH_Importer = org.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_PartNo = part.OP_PartNum;
			invoiceLine1.JI_InvoiceQuantity = 100m;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_InvoiceQuantity = 200m;
			invoiceLine2.JI_PartNo = part.OP_PartNum;
			var whsPack = declaration.WHSPacks.AddNew();
			var whsPackLine1 = declaration.WHSPackLines.AddNew(whsPack);
			whsPackLine1.US_PackedQty = 10m;
			AssertEquals("JI_Calc_AllocatedQty", 0m, invoiceLine1.JI_Calc_AllocatedQty);
			AssertEquals("JI_Calc_AllocatedQtyBalance", 100m, invoiceLine1.JI_Calc_AllocatedQtyBalance);
			AssertEquals("JI_Calc_AllocatedQty", 0m, invoiceLine2.JI_Calc_AllocatedQty);
			AssertEquals("JI_Calc_AllocatedQtyBalance", 200m, invoiceLine2.JI_Calc_AllocatedQtyBalance);
			AssertEquals(0, invoiceLine1.WHSPackLines.Count);
			AssertEquals(0, invoiceLine2.WHSPackLines.Count);
			whsPackLine1.US_JI_InvoiceLine = invoiceLine1.PK;
			AssertEquals("JI_Calc_AllocatedQty", 10m, invoiceLine1.JI_Calc_AllocatedQty);
			AssertEquals("JI_Calc_AllocatedQtyBalance", 90m, invoiceLine1.JI_Calc_AllocatedQtyBalance);
			AssertEquals("JI_Calc_AllocatedQty", 0m, invoiceLine2.JI_Calc_AllocatedQty);
			AssertEquals("JI_Calc_AllocatedQtyBalance", 200m, invoiceLine2.JI_Calc_AllocatedQtyBalance);
			AssertEquals(1, invoiceLine1.WHSPackLines.Count);
			AssertEquals(true, invoiceLine1.WHSPackLines.Contains(whsPackLine1));
			AssertEquals(0, invoiceLine2.WHSPackLines.Count);
			whsPackLine1.US_JI_InvoiceLine = invoiceLine2.PK;
			AssertEquals("JI_Calc_AllocatedQty", 0m, invoiceLine1.JI_Calc_AllocatedQty);
			AssertEquals("JI_Calc_AllocatedQtyBalance", 100m, invoiceLine1.JI_Calc_AllocatedQtyBalance);
			AssertEquals("JI_Calc_AllocatedQty", 10m, invoiceLine2.JI_Calc_AllocatedQty);
			AssertEquals("JI_Calc_AllocatedQtyBalance", 190m, invoiceLine2.JI_Calc_AllocatedQtyBalance);
			AssertEquals(0, invoiceLine1.WHSPackLines.Count);
			AssertEquals(1, invoiceLine2.WHSPackLines.Count);
			AssertEquals(true, invoiceLine2.WHSPackLines.Contains(whsPackLine1));
			var whsPackLine2 = declaration.WHSPackLines.AddNew(whsPack);
			whsPackLine2.US_JI_InvoiceLine = invoiceLine1.PK;
			whsPackLine2.US_PackedQty = 20m;
			AssertEquals("JI_Calc_AllocatedQty", 20m, invoiceLine1.JI_Calc_AllocatedQty);
			AssertEquals("JI_Calc_AllocatedQtyBalance", 80m, invoiceLine1.JI_Calc_AllocatedQtyBalance);
			AssertEquals("JI_Calc_AllocatedQty", 10m, invoiceLine2.JI_Calc_AllocatedQty);
			AssertEquals("JI_Calc_AllocatedQtyBalance", 190m, invoiceLine2.JI_Calc_AllocatedQtyBalance);
			AssertEquals(1, invoiceLine1.WHSPackLines.Count);
			AssertEquals(true, invoiceLine1.WHSPackLines.Contains(whsPackLine2));
			AssertEquals(1, invoiceLine2.WHSPackLines.Count);
			AssertEquals(true, invoiceLine2.WHSPackLines.Contains(whsPackLine1));
			whsPackLine2.US_JI_InvoiceLine = invoiceLine2.PK;
			AssertEquals("JI_Calc_AllocatedQty", 0m, invoiceLine1.JI_Calc_AllocatedQty);
			AssertEquals("JI_Calc_AllocatedQtyBalance", 100m, invoiceLine1.JI_Calc_AllocatedQtyBalance);
			AssertEquals("JI_Calc_AllocatedQty", 30m, invoiceLine2.JI_Calc_AllocatedQty);
			AssertEquals("JI_Calc_AllocatedQtyBalance", 170m, invoiceLine2.JI_Calc_AllocatedQtyBalance);
			AssertEquals(0, invoiceLine1.WHSPackLines.Count);
			AssertEquals(2, invoiceLine2.WHSPackLines.Count);
			AssertEquals(true, invoiceLine2.WHSPackLines.Contains(whsPackLine1));
			AssertEquals(true, invoiceLine2.WHSPackLines.Contains(whsPackLine2));
			invoiceLine2.JI_InvoiceQuantity = 150m;
			AssertEquals("JI_Calc_AllocatedQty", 0m, invoiceLine1.JI_Calc_AllocatedQty);
			AssertEquals("JI_Calc_AllocatedQtyBalance", 100m, invoiceLine1.JI_Calc_AllocatedQtyBalance);
			AssertEquals("JI_Calc_AllocatedQty", 30m, invoiceLine2.JI_Calc_AllocatedQty);
			AssertEquals("JI_Calc_AllocatedQtyBalance", 120m, invoiceLine2.JI_Calc_AllocatedQtyBalance);
			AssertEquals(0, invoiceLine1.WHSPackLines.Count);
			AssertEquals(2, invoiceLine2.WHSPackLines.Count);
			AssertEquals(true, invoiceLine2.WHSPackLines.Contains(whsPackLine1));
			AssertEquals(true, invoiceLine2.WHSPackLines.Contains(whsPackLine2));
			var declaration2 = Factory.New<JobDeclaration>();
			invoice.JZ_JE = declaration2.PK;
			AssertEquals("JI_Calc_AllocatedQty", 0m, invoiceLine1.JI_Calc_AllocatedQty);
			AssertEquals("JI_Calc_AllocatedQtyBalance", 100m, invoiceLine1.JI_Calc_AllocatedQtyBalance);
			AssertEquals("JI_Calc_AllocatedQty", 0m, invoiceLine2.JI_Calc_AllocatedQty);
			AssertEquals("JI_Calc_AllocatedQtyBalance", 150m, invoiceLine2.JI_Calc_AllocatedQtyBalance);
			AssertEquals(0, invoiceLine1.WHSPackLines.Count);
			AssertEquals(0, invoiceLine2.WHSPackLines.Count);
			invoice.JZ_JE = declaration.PK;
			AssertEquals("JI_Calc_AllocatedQty", 0m, invoiceLine1.JI_Calc_AllocatedQty);
			AssertEquals("JI_Calc_AllocatedQtyBalance", 100m, invoiceLine1.JI_Calc_AllocatedQtyBalance);
			AssertEquals("JI_Calc_AllocatedQty", 30m, invoiceLine2.JI_Calc_AllocatedQty);
			AssertEquals("JI_Calc_AllocatedQtyBalance", 120m, invoiceLine2.JI_Calc_AllocatedQtyBalance);
			AssertEquals(0, invoiceLine1.WHSPackLines.Count);
			AssertEquals(2, invoiceLine2.WHSPackLines.Count);
			AssertEquals(true, invoiceLine2.WHSPackLines.Contains(whsPackLine1));
			AssertEquals(true, invoiceLine2.WHSPackLines.Contains(whsPackLine2));
		}

		public void TestJI_BondedWhsQuantityIsDefaultFromJI_InvoiceQuantityIfIsNotPackageType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_InvoiceQuantity = 10m;
			AssertEquals(0m, invoiceLine.JI_BondedWhsQuantity);
			var expectedValue = 10m;
			foreach (var code in new[]
			{
				ABIUnitOfMeasureList.Codes.Barrels,
				ABIUnitOfMeasureList.Codes.Case,
				ABIUnitOfMeasureList.Codes.Dozen,
				ABIUnitOfMeasureList.Codes.DozenPairs,
				ABIUnitOfMeasureList.Codes.DozenPieces,
				ABIUnitOfMeasureList.Codes.Number,
				ABIUnitOfMeasureList.Codes.Packs,
				ABIUnitOfMeasureList.Codes.Pairs,
				ABIUnitOfMeasureList.Codes.Pieces
			})
			{
				invoiceLine.JI_InvoiceUQ = code;
				AssertEquals(0m, invoiceLine.JI_BondedWhsQuantity);
			}

			foreach (var code in new[]
			{
				ABIUnitOfMeasureList.Codes.AlternatingCurrent,
				ABIUnitOfMeasureList.Codes.BolusesDosage,
				ABIUnitOfMeasureList.Codes.CapsulesDosage
			})
			{
				invoiceLine.JI_InvoiceUQ = code;
				AssertEquals(expectedValue, invoiceLine.JI_BondedWhsQuantity);
				expectedValue += 1m;
				invoiceLine.JI_InvoiceQuantity = expectedValue;
				AssertEquals(expectedValue, invoiceLine.JI_BondedWhsQuantity);
			}
			invoiceLine.JI_BondedWhsQuantity = 0m;
			AssertEquals("Should be able to set to a different value", 0m, invoiceLine.JI_BondedWhsQuantity);

			invoiceLine.JI_InvoiceUQ = ABIUnitOfMeasureList.Codes.Barrels;
			AssertEquals(0m, invoiceLine.JI_BondedWhsQuantity);
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_ParentID = invoiceLine2.PK;
			invoiceLine.JI_InvoiceUQ = ABIUnitOfMeasureList.Codes.Carat;
			AssertEquals(0m, invoiceLine.JI_BondedWhsQuantity);
			invoiceLine.JI_ParentID = ZGuid.Empty;
			AssertEquals(expectedValue, invoiceLine.JI_BondedWhsQuantity);

			invoiceLine.JI_InvoiceUQ = ZString.Empty;
			invoiceLine.JI_InvoiceQuantity = 100m;
			AssertEquals(expectedValue, invoiceLine.JI_BondedWhsQuantity);

			invoiceLine.JI_InvoiceUQ = ABIUnitOfMeasureList.Codes.Centigrams;
			AssertEquals(100m, invoiceLine.JI_BondedWhsQuantity);
		}

		public void TestBondedWhsQuantityForGUI()
		{
			var org = Factory.New<OrgHeader>();
			org.CompanyData.OB_IMUsedBondedWhs = true;
			var classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "LK234";
			classification.CC_TariffNum = "2010304050";
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PART321";
			var orgRel = part.RelatedOrganisations.AddOrganisationIfNotExist(org.PK, OrgPartRelation.RelationshipTypes.Owner);
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_OH = orgRel.OU_OH;
			pivot.CI_CC = classification.PK;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.JE_OH_Importer = org.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = part.OP_PartNum;
			invoiceLine.JI_InvoiceQuantity = 100m;
			AssertEquals("invoiceLine.BondedWhsQuantityForGUIDataFieldType", nameof(FieldType.Decimal), invoiceLine.BondedWhsQuantityForGUIDataFieldType);
			AssertEquals("invoiceLine.BondedWhsQuantityForGUI", "0.00000", invoiceLine.BondedWhsQuantityForGUI);

			var whsPack = declaration.WHSPacks.AddNew();
			var whsPackLine = declaration.WHSPackLines.AddNew();
			AssertEquals("invoiceLine.BondedWhsQuantityForGUIDataFieldType", nameof(FieldType.Decimal), invoiceLine.BondedWhsQuantityForGUIDataFieldType);
			AssertEquals("invoiceLine.BondedWhsQuantityForGUI", "0.00000", invoiceLine.BondedWhsQuantityForGUI);

			whsPackLine.US_B7_WHSPack = whsPack.PK;
			AssertEquals("invoiceLine.BondedWhsQuantityForGUIDataFieldType", nameof(FieldType.Decimal), invoiceLine.BondedWhsQuantityForGUIDataFieldType);
			AssertEquals("invoiceLine.BondedWhsQuantityForGUI", "0.00000", invoiceLine.BondedWhsQuantityForGUI);

			whsPackLine.US_JI_InvoiceLine = invoiceLine.PK;
			AssertEquals("invoiceLine.BondedWhsQuantityForGUIDataFieldType", nameof(FieldType.Text), invoiceLine.BondedWhsQuantityForGUIDataFieldType);
			AssertEquals("invoiceLine.BondedWhsQuantityForGUI", "See WHS Packs", invoiceLine.BondedWhsQuantityForGUI);
			var refreshCount = 0;
			invoiceLine.BondedWhsQuantityForGUIInfo.ValueChanged += (object sender, EventArgs e) => { refreshCount++; };

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_PartNo = part.OP_PartNum;
			invoiceLine2.JI_InvoiceQuantity = 200m;

			whsPackLine.US_JI_InvoiceLine = invoiceLine2.PK;
			AssertEquals(1, refreshCount);
			AssertEquals("invoiceLine.BondedWhsQuantityForGUIDataFieldType", nameof(FieldType.Decimal), invoiceLine.BondedWhsQuantityForGUIDataFieldType);
			AssertEquals("invoiceLine.BondedWhsQuantityForGUI", "0.00000", invoiceLine.BondedWhsQuantityForGUI);
			AssertEquals("invoiceLine2.BondedWhsQuantityForGUIDataFieldType", nameof(FieldType.Text), invoiceLine2.BondedWhsQuantityForGUIDataFieldType);
			AssertEquals("invoiceLine2.BondedWhsQuantityForGUI", "See WHS Packs", invoiceLine2.BondedWhsQuantityForGUI);

			whsPackLine.US_JI_InvoiceLine = invoiceLine.PK;
			AssertEquals(2, refreshCount);
			AssertEquals("invoiceLine2.BondedWhsQuantityForGUIDataFieldType", nameof(FieldType.Decimal), invoiceLine2.BondedWhsQuantityForGUIDataFieldType);
			AssertEquals("invoiceLine2.BondedWhsQuantityForGUI", "0.00000", invoiceLine2.BondedWhsQuantityForGUI);
			AssertEquals("invoiceLine.BondedWhsQuantityForGUIDataFieldType", nameof(FieldType.Text), invoiceLine.BondedWhsQuantityForGUIDataFieldType);
			AssertEquals("invoiceLine.BondedWhsQuantityForGUI", "See WHS Packs", invoiceLine.BondedWhsQuantityForGUI);

			whsPack.Delete();
			AssertEquals(3, refreshCount);
			AssertEquals("invoiceLine.BondedWhsQuantityForGUIDataFieldType", nameof(FieldType.Decimal), invoiceLine.BondedWhsQuantityForGUIDataFieldType);
			AssertEquals("invoiceLine.BondedWhsQuantityForGUI", "0.00000", invoiceLine.BondedWhsQuantityForGUI);
			AssertEquals("invoiceLine2.BondedWhsQuantityForGUIDataFieldType", nameof(FieldType.Decimal), invoiceLine2.BondedWhsQuantityForGUIDataFieldType);
			AssertEquals("invoiceLine2.BondedWhsQuantityForGUI", "0.00000", invoiceLine2.BondedWhsQuantityForGUI);

			invoiceLine.BondedWhsQuantityForGUI = "10.23546";
			AssertEquals(10.23546m, invoiceLine.JI_BondedWhsQuantity);
		}

		public void TestDefaultPGAIndicators()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Declaration.US_EnableENS = true;
			Declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			// FSIS is not yet effective
			ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.PGAFSIS, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, false);

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "10000000";
			tariff.UE_PGACodes = "EP2DT1EP4FS3EP5FD4EP8";
			tariff.UE_DateFrom = ZDateTime.Today.AddDays(-2);
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);
			InvoiceLine.JI_Tariff = tariff.UE_Tariff;
			AssertEquals(ZString.Empty, InvoiceLine.US_FSISInd);
			AssertEquals(OGAIndicatorList.Codes.Declared, InvoiceLine.US_FDAIndicator);

			tariff.UE_PGACodes = "FD2EP2DT1EP4FS4EP6FD2EP8";
			InvoiceLine.JI_Tariff = ZString.Empty;
			InvoiceLine.US_SupTariff = tariff.UE_Tariff;
			AssertEquals(ZString.Empty, InvoiceLine.US_FSISInd);
			AssertEquals(OGAIndicatorList.Codes.Declared, InvoiceLine.US_FDAIndicator);

			// FSIS is effective
			ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.PGAFSIS, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true);
			InvoiceLine.US_SupTariff = ZString.Empty;
			InvoiceLine.JI_Tariff = tariff.UE_Tariff;
			AssertEquals(OGAIndicatorList.Codes.Declared, InvoiceLine.US_ODSInd);
			AssertEquals(OGAIndicatorList.Codes.Declared, InvoiceLine.US_VNEInd);
			AssertEquals(OGAIndicatorList.Codes.Declared, InvoiceLine.US_FSISInd);
			AssertEquals(OGAIndicatorList.Codes.Declared, InvoiceLine.US_PSTIndicator);
			AssertEquals(OGAIndicatorList.Codes.Declared, InvoiceLine.US_FDAIndicator);
			AssertEquals(OGAIndicatorList.Codes.Declared, InvoiceLine.US_TSCAInd);
		}

		public void TestACEDDTCData()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Declaration.US_EnableENS = true;
			InvoiceLine.US_DDTCInd = "";
			AssertEquals("InvoiceLine.US_DDTCLicenseNoInfo.ReadOnly", true, InvoiceLine.US_DDTCLicenseNoInfo.ReadOnly);
			AssertEquals("InvoiceLine.US_DDTCLicenseTypeInfo.ReadOnly", true, InvoiceLine.US_DDTCLicenseTypeInfo.ReadOnly);
			AssertEquals("InvoiceLine.US_DDTCExemptionCodeInfo.ReadOnly", true, InvoiceLine.US_DDTCExemptionCodeInfo.ReadOnly);
			AssertEquals("InvoiceLine.US_DDTCArrivalDateInfo.ReadOnly", true, InvoiceLine.US_DDTCArrivalDateInfo.ReadOnly);
			AssertEquals("InvoiceLine.US_DDTCRegistrationNoInfo.ReadOnly", true, InvoiceLine.US_DDTCRegistrationNoInfo.ReadOnly);
			InvoiceLine.US_DDTCInd = OGAIndicatorList.Codes.Declared;
			AssertEquals("InvoiceLine.US_DDTCLicenseNoInfo.ReadOnly", false, InvoiceLine.US_DDTCLicenseNoInfo.ReadOnly);
			AssertEquals("InvoiceLine.US_DDTCLicenseTypeInfo.ReadOnly", false, InvoiceLine.US_DDTCLicenseTypeInfo.ReadOnly);
			AssertEquals("InvoiceLine.US_DDTCExemptionCodeInfo.ReadOnly", false, InvoiceLine.US_DDTCExemptionCodeInfo.ReadOnly);
			AssertEquals("InvoiceLine.US_DDTCArrivalDateInfo.ReadOnly", false, InvoiceLine.US_DDTCArrivalDateInfo.ReadOnly);
			AssertEquals("InvoiceLine.US_DDTCRegistrationNoInfo.ReadOnly", false, InvoiceLine.US_DDTCRegistrationNoInfo.ReadOnly);
			InvoiceLine.US_DDTCLicenseNo = "A";
			InvoiceLine.US_DDTCLicenseType = "A";
			InvoiceLine.US_DDTCRegistrationNo = "A";
			InvoiceLine.US_DDTCExemptionCode = "A";
			InvoiceLine.US_DDTCArrivalDate = ZDateTime.BrettsBirthday;
			InvoiceLine.US_DDTCInd = ZString.Empty;
			AssertEquals("InvoiceLine.US_DDTCLicenseNo", "", InvoiceLine.US_DDTCLicenseNo);
			AssertEquals("InvoiceLine.US_DDTCLicenseType", "", InvoiceLine.US_DDTCLicenseType);
			AssertEquals("InvoiceLine.US_DDTCRegistrationNo", "", InvoiceLine.US_DDTCRegistrationNo);
			AssertEquals("InvoiceLine.US_DDTCExemptionCode", "", InvoiceLine.US_DDTCExemptionCode);
			AssertEquals("InvoiceLine.US_DDTCArrivalDate", ZDateTime.Empty, InvoiceLine.US_DDTCArrivalDate);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			InvoiceLine.US_DDTCInd = "";
			AssertEquals("InvoiceLine.US_DDTCLicenseNoInfo.ReadOnly", true, InvoiceLine.US_DDTCLicenseNoInfo.ReadOnly);
			AssertEquals("InvoiceLine.US_DDTCLicenseTypeInfo.ReadOnly", true, InvoiceLine.US_DDTCLicenseTypeInfo.ReadOnly);
			AssertEquals("InvoiceLine.US_DDTCExemptionCodeInfo.ReadOnly", true, InvoiceLine.US_DDTCExemptionCodeInfo.ReadOnly);
			AssertEquals("InvoiceLine.US_DDTCArrivalDateInfo.ReadOnly", true, InvoiceLine.US_DDTCArrivalDateInfo.ReadOnly);
			AssertEquals("InvoiceLine.US_DDTCRegistrationNoInfo.ReadOnly", false, InvoiceLine.US_DDTCRegistrationNoInfo.ReadOnly);
			InvoiceLine.US_DDTCInd = OGAIndicatorList.Codes.Declared;
			AssertEquals("InvoiceLine.US_DDTCLicenseNoInfo.ReadOnly", true, InvoiceLine.US_DDTCLicenseNoInfo.ReadOnly);
			AssertEquals("InvoiceLine.US_DDTCLicenseTypeInfo.ReadOnly", true, InvoiceLine.US_DDTCLicenseTypeInfo.ReadOnly);
			AssertEquals("InvoiceLine.US_DDTCExemptionCodeInfo.ReadOnly", true, InvoiceLine.US_DDTCExemptionCodeInfo.ReadOnly);
			AssertEquals("InvoiceLine.US_DDTCArrivalDateInfo.ReadOnly", true, InvoiceLine.US_DDTCArrivalDateInfo.ReadOnly);
			AssertEquals("InvoiceLine.US_DDTCRegistrationNoInfo.ReadOnly", false, InvoiceLine.US_DDTCRegistrationNoInfo.ReadOnly);
			InvoiceLine.US_DDTCLicenseNo = "A";
			InvoiceLine.US_DDTCLicenseType = "A";
			InvoiceLine.US_DDTCRegistrationNo = "A";
			InvoiceLine.US_DDTCArrivalDate = ZDateTime.BrettsBirthday;
			InvoiceLine.US_DDTCExemptionCode = "A";
			InvoiceLine.US_DDTCInd = ZString.Empty;
			AssertEquals("InvoiceLine.US_DDTCLicenseNo", "A", InvoiceLine.US_DDTCLicenseNo);
			AssertEquals("InvoiceLine.US_DDTCLicenseType", "A", InvoiceLine.US_DDTCLicenseType);
			AssertEquals("InvoiceLine.US_DDTCRegistrationNo", "A", InvoiceLine.US_DDTCRegistrationNo);
			AssertEquals("InvoiceLine.US_DDTCExemptionCode", "A", InvoiceLine.US_DDTCExemptionCode);
			AssertEquals("InvoiceLine.US_DDTCArrivalDate", ZDateTime.BrettsBirthday, InvoiceLine.US_DDTCArrivalDate);
		}

		public void TestSupTariffFormatted()
		{
			AssertEquals(15, InvoiceLine.SupTariffFormattedInfo.MaxLength);

			InvoiceLine.SupTariffFormatted = "9802.00.4040";
			AssertEquals("9802.00.4040", InvoiceLine.SupTariffFormatted);

			InvoiceLine.SupTariffFormatted = "9802.00.40 40";
			AssertEquals("9802.00.4040", InvoiceLine.SupTariffFormatted);
		}

		public void TestFSISForm95401Bills()
		{
			var mb = Declaration.Bills.AddNew();
			mb.CU_BillType = "MB";
			mb.CU_BillNum = "123";
			mb.US_UI_NKBillIssuerSCAC = "APUL";

			var hb1 = Declaration.Bills.AddNew();
			hb1.CU_BillType = "HB";
			hb1.CU_BillNum = "456";
			hb1.CU_CU_ParentBill = mb.PK;
			hb1.US_UI_NKBillIssuerSCAC = "BANG";

			var hb2 = mb.ChildBills.AddNew();
			hb2.CU_BillType = "HB";
			hb2.CU_BillNum = "789";
			hb2.CU_CU_ParentBill = mb.PK;
			hb2.US_UI_NKBillIssuerSCAC = "BANG";

			var container0 = Declaration.CusContainers.AddNew();
			container0.CO_ContainerNumber = "CONT111111";
			var container1 = Declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CONT222222";

			var package0 = Declaration.Packages.AddNew();
			package0.CW_HouseBill = hb1.CU_BillUniqueCode;
			package0.CW_ContainerNoOrEquipmentNo = "CONT111111";

			var package1 = Declaration.Packages.AddNew();
			package1.CW_HouseBill = hb2.CU_BillUniqueCode;
			package1.CW_ContainerNoOrEquipmentNo = "CONT222222";

			var invoiceHeader = Declaration.Invoices.AddNew();
			var invoiceLine0 = invoiceHeader.InvoiceLines.AddNew();
			AssertEquals(2, invoiceLine0.ContainersForInvoiceLinesForBindingOnly.Count);
			invoiceLine0.ContainersForInvoiceLinesForBindingOnly.OfType<NonPersistentCusContainer>().FirstOrDefault(x => x.ContainerNumber == "CONT111111").IsForInvoiceLine = true;
			AssertEquals("MB:APUL123 HB:BANG456", invoiceLine0.FSISForm95401Bills);

			invoiceLine0.ContainersForInvoiceLinesForBindingOnly.OfType<NonPersistentCusContainer>().FirstOrDefault(x => x.ContainerNumber == "CONT222222").IsForInvoiceLine = true;
			AssertEquals("MB:APUL123 HB:BANG456 HB:BANG789", invoiceLine0.FSISForm95401Bills);

			var mb2 = Declaration.Bills.AddNew();
			mb2.CU_BillType = "MB";
			mb2.CU_BillNum = "234";
			mb2.US_UI_NKBillIssuerSCAC = "APUL";
			hb2.CU_CU_ParentBill = mb2.PK;
			AssertEquals("MB:APUL123 HB:BANG456 MB:APUL234 HB:BANG789", invoiceLine0.FSISForm95401Bills);

			package0.CW_ContainerNoOrEquipmentNo = "CONT111111Diff";
			AssertEquals("MB:APUL234 HB:BANG789", invoiceLine0.FSISForm95401Bills);

			package1.CW_ContainerNoOrEquipmentNo = "CONT222222Diff";
			AssertEquals("MB:APUL123 HB:BANG456 MB:APUL234 HB:BANG789", invoiceLine0.FSISForm95401Bills);
		}

		[TestDate(2017, 12, 1)]
		public void TestApportionInformalFee()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLineOne = invoice.InvoiceLines.AddNew();
			invoiceLineOne.JI_LinePrice = 1000m;
			var invoiceLineTwo = invoice.InvoiceLines.AddNew();
			invoiceLineTwo.JI_LinePrice = 2000m;
			var invoiceLineThree = invoice.InvoiceLines.AddNew();
			invoiceLineThree.JI_LinePrice = 0m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var informalFeeForLineOne = ((IUltimateDistributee)invoiceLineOne).Fees.FirstOrDefault(x => x.FeeCode == Core.Constants.USCustoms.FeeCodes.MerchandiseInformal);
			AssertNotNull(informalFeeForLineOne);
			AssertEquals(0.67m, informalFeeForLineOne.AmountInLocalCurrency);

			var informalFeeForLineTwo = ((IUltimateDistributee)invoiceLineTwo).Fees.FirstOrDefault(x => x.FeeCode == Core.Constants.USCustoms.FeeCodes.MerchandiseInformal);
			AssertNotNull(informalFeeForLineTwo);
			AssertEquals(1.33m, informalFeeForLineTwo.AmountInLocalCurrency.Round(2));

			var informalFeeForLineThree = ((IUltimateDistributee)invoiceLineThree).Fees.FirstOrDefault(x => x.FeeCode == Core.Constants.USCustoms.FeeCodes.MerchandiseInformal);
			AssertNull(informalFeeForLineThree);
		}

		public void TestNeedToApportionNetWeight()
		{
			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "100000001";
			tariff1.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;
			tariff1.UE_QuotaIndicator = false;
			tariff1.UE_DateFrom = ZDateTime.Today;
			tariff1.UE_DateTo = ZDateTime.Today.AddYears(1);
			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "100000002";
			tariff2.UE_DutyComputationCode = ComputationCodeList.Codes.Free;
			tariff2.UE_QuotaIndicator = false;
			tariff2.UE_DateFrom = ZDateTime.Today;
			tariff2.UE_DateTo = ZDateTime.Today.AddYears(1);
			var tariff3 = Factory.New<USCTariff>();
			tariff3.UE_DutyComputationCode = ComputationCodeList.Codes.Free;
			tariff3.UE_Tariff = "100000003";
			tariff3.UE_QuotaIndicator = true;
			tariff3.UE_DateFrom = ZDateTime.Today;
			tariff3.UE_DateTo = ZDateTime.Today.AddYears(1);
			var tariff4 = Factory.New<USCTariff>();
			tariff4.UE_Tariff = "100000004";
			tariff4.UE_DutyComputationCode = ComputationCodeList.Codes.SpecificCompound;
			tariff4.UE_QuotaIndicator = false;
			tariff4.UE_DateFrom = ZDateTime.Today;
			tariff4.UE_DateTo = ZDateTime.Today.AddYears(1);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_AutoWeightApportion = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 1000m;
			invoiceLine1.JI_Tariff = "100000001";
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 1000m;
			invoiceLine2.JI_Tariff = "100000002";
			var invoiceLine3 = invoice.InvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 1000m;
			invoiceLine3.JI_Tariff = "100000003";
			var invoiceLine4 = invoice.InvoiceLines.AddNew();
			invoiceLine4.JI_LinePrice = 1000m;
			invoiceLine4.JI_Tariff = "100000004";
			var invoiceLine5 = invoice.InvoiceLines.AddNew();
			invoiceLine5.JI_LinePrice = 1000m;
			invoiceLine5.JI_Tariff = "100000003";
			var invoiceLine6 = invoiceLine5.AddSecondaryInvoiceLine();
			invoiceLine6.JI_LinePrice = 1000m;
			invoiceLine6.JI_Tariff = "100000002";

			var quotaVisaEntryTypes = new string[]
			{
				EntryTypeList.Codes.ConsumptionQuotaVisa,
				EntryTypeList.Codes.ConsumptionADDCVDQuotaVisa,
				EntryTypeList.Codes.InformalQuotaVisa,
				EntryTypeList.Codes.WarehouseWithdrawalQuota,
				EntryTypeList.Codes.WarehouseWithdrawalADDCVDQuotaVisa
			};

			foreach (var quotaEntryType in quotaVisaEntryTypes)
			{
				declaration.US_EntryType = quotaEntryType;
				Assert(quotaEntryType + "NeedToApportionNetWeight should be true", invoiceLine1.NeedToApportionNetWeight);
				Assert(quotaEntryType + "NeedToApportionNetWeight should be true", invoiceLine2.NeedToApportionNetWeight);
				Assert(quotaEntryType + "NeedToApportionNetWeight should be false", !invoiceLine3.NeedToApportionNetWeight);
				Assert(quotaEntryType + "NeedToApportionNetWeight should be false", !invoiceLine4.NeedToApportionNetWeight);
				Assert(quotaEntryType + "NeedToApportionNetWeight should be true", invoiceLine5.NeedToApportionNetWeight);
				Assert(quotaEntryType + "NeedToApportionNetWeight should be true", invoiceLine6.NeedToApportionNetWeight);
			}

			invoice.JZ_Weight = 600m;
			invoice.JZ_WeightUQ = Core.Constants.Weight.Kilograms;
			invoice.JZ_NetWeight = 540m;
			invoice.JZ_NetWeightUQ = Core.Constants.Weight.Kilograms;

			AssertEquals("JI_Weight should be auto apportioned", 100m, invoiceLine1.JI_Weight);
			AssertEquals("JI_NetWeight should be defaulted", 90m, invoiceLine1.JI_NetWeight);
			AssertEquals("JI_Weight should be auto apportioned", 100m, invoiceLine2.JI_Weight);
			AssertEquals("JI_NetWeight should be defaulted", 90m, invoiceLine2.JI_NetWeight);
			AssertEquals("JI_Weight should be auto apportioned", 100m, invoiceLine3.JI_Weight);
			AssertEquals("JI_NetWeight should not be defaulted", 0m, invoiceLine3.JI_NetWeight);
			AssertEquals("JI_Weight should be auto apportioned", 100m, invoiceLine4.JI_Weight);
			AssertEquals("JI_NetWeight should not be defaulted", 0m, invoiceLine4.JI_NetWeight);
			AssertEquals("JI_Weight should be auto apportioned", 100m, invoiceLine5.JI_Weight);
			AssertEquals("JI_NetWeight should be defaulted", 90m, invoiceLine5.JI_NetWeight);
			AssertEquals("JI_Weight should be auto apportioned", 100m, invoiceLine6.JI_Weight);
			AssertEquals("JI_NetWeight should be defaulted", 90m, invoiceLine6.JI_NetWeight);

			invoiceLine1.JI_Weight = 0m;
			invoiceLine1.JI_NetWeight = 0m;
			invoiceLine5.JI_Weight = 0m;
			invoiceLine5.JI_NetWeight = 0m;
			invoiceLine6.JI_Weight = 0m;
			invoiceLine6.JI_NetWeight = 0m;
			invoiceLine6.JI_Tariff = "100000003";

			AssertEquals("JI_Weight should be auto re-apportioned", 100m, invoiceLine1.JI_Weight);
			AssertEquals("JI_NetWeight should be defaulted", 90m, invoiceLine1.JI_NetWeight);
			AssertEquals("JI_Weight should not be auto apportioned", 100m, invoiceLine5.JI_Weight);
			AssertEquals("JI_NetWeight should not be defaulted", 0m, invoiceLine5.JI_NetWeight);
			AssertEquals("JI_Weight should not be auto apportioned", 100m, invoiceLine6.JI_Weight);
			AssertEquals("JI_NetWeight should not be defaulted", 0m, invoiceLine6.JI_NetWeight);

			invoiceLine1.JI_Weight = 0m;
			invoiceLine1.JI_NetWeight = 0m;
			invoiceLine5.JI_Weight = 0m;
			invoiceLine5.JI_NetWeight = 0m;
			invoiceLine6.JI_Weight = 0m;
			invoiceLine6.JI_NetWeight = 0m;
			invoiceLine6.JI_Tariff = "100000001";

			AssertEquals("JI_Weight should be auto re-apportioned", 100m, invoiceLine1.JI_Weight);
			AssertEquals("JI_NetWeight should be defaulted", 90m, invoiceLine1.JI_NetWeight);
			AssertEquals("JI_Weight should not be auto apportioned", 100m, invoiceLine5.JI_Weight);
			AssertEquals("JI_Weight should be auto re-apportioned", 100m, invoiceLine5.JI_Weight);
			AssertEquals("JI_NetWeight should be defaulted", 90m, invoiceLine5.JI_NetWeight);
			AssertEquals("JI_Weight should be auto re-apportioned", 100m, invoiceLine6.JI_Weight);
			AssertEquals("JI_NetWeight should be defaulted", 90m, invoiceLine6.JI_NetWeight);

			invoiceLine6.JI_Tariff = "100000003";
			invoiceLine1.JI_Weight = 0m;
			invoiceLine1.JI_NetWeight = 0m;
			invoiceLine2.JI_Weight = 0m;
			invoiceLine2.JI_NetWeight = 0m;
			invoiceLine3.JI_Weight = 0m;
			invoiceLine3.JI_NetWeight = 0m;
			invoiceLine4.JI_Weight = 0m;
			invoiceLine4.JI_NetWeight = 0m;
			invoiceLine5.JI_Weight = 0m;
			invoiceLine5.JI_NetWeight = 0m;
			invoiceLine6.JI_Weight = 0m;
			invoiceLine6.JI_NetWeight = 0m;

			declaration.US_EntryType = "01";
			Assert("NeedToApportionNetWeight should be true", invoiceLine1.NeedToApportionNetWeight);
			Assert("NeedToApportionNetWeight should be true", invoiceLine2.NeedToApportionNetWeight);
			Assert("NeedToApportionNetWeight should be true", invoiceLine3.NeedToApportionNetWeight);
			Assert("NeedToApportionNetWeight should be false", !invoiceLine4.NeedToApportionNetWeight);
			Assert("NeedToApportionNetWeight should be true", invoiceLine5.NeedToApportionNetWeight);
			Assert("NeedToApportionNetWeight should be true", invoiceLine6.NeedToApportionNetWeight);

			AssertEquals("JI_Weight should be auto re-apportioned", 100m, invoiceLine1.JI_Weight);
			AssertEquals("JI_NetWeight should be defaulted", 90m, invoiceLine1.JI_NetWeight);
			AssertEquals("JI_Weight should be auto re-apportioned", 100m, invoiceLine2.JI_Weight);
			AssertEquals("JI_NetWeight should be defaulted", 90m, invoiceLine2.JI_NetWeight);
			AssertEquals("JI_Weight should be auto re-apportioned", 100m, invoiceLine3.JI_Weight);
			AssertEquals("JI_NetWeight should be defaulted", 90m, invoiceLine3.JI_NetWeight);
			AssertEquals("JI_Weight should be auto re-apportioned", 100m, invoiceLine4.JI_Weight);
			AssertEquals("JI_NetWeight should be defaulted", 0m, invoiceLine4.JI_NetWeight);
			AssertEquals("JI_Weight should not be auto apportioned", 100m, invoiceLine5.JI_Weight);
			AssertEquals("JI_Weight should be auto re-apportioned", 100m, invoiceLine5.JI_Weight);
			AssertEquals("JI_NetWeight should be defaulted", 90m, invoiceLine5.JI_NetWeight);
			AssertEquals("JI_Weight should be auto re-apportioned", 100m, invoiceLine6.JI_Weight);
			AssertEquals("JI_NetWeight should be defaulted", 90m, invoiceLine6.JI_NetWeight);
		}

		public void TestUpdatePartSyncManagerAndRefresh()
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.FillWithValidTestData();
			consignor.OH_Code = "ORGAUSYD";
			consignor.OH_IsConsignor = ZBool.True;
			consignor.OH_RL_NKClosestPort = "AUSYD";
			var consignee = Factory.New<OrgHeader>();
			consignee.FillWithValidTestData();
			consignee.OH_Code = "ORGUSCHI";
			consignee.OH_IsConsignee = ZBool.True;
			consignee.OH_RL_NKClosestPort = "USCHI";

			var classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "TestLookup";
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PART1";
			part.OP_Weight = 100m;
			part.OP_WeightUQ = "HG";
			part.RelatedOrganisations.AddSupplier(consignor);
			part.RelatedOrganisations.AddOwner(consignee);
			CusClassPartPivot pivot = part.PivotsForBinding.AddNew();
			pivot.CI_UsageComment = "U1";
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "1010101010";
			pivot.CI_SupplementalTariff = "2020202020";
			pivot.CI_CC = classification.PK;
			pivot.CI_OP = part.PK;
			CusClassPartPivot pivotChild1 = pivot.Children.AddNew();
			pivotChild1.CI_UsageComment = "CU1";
			pivotChild1.CI_ChildType = ClassificationChildTypeList.Codes.Related;
			pivotChild1.CI_TariffNum = "1010101011";
			pivotChild1.CI_SupplementalTariff = "2020202021";
			pivotChild1.CI_CC = classification.PK;
			pivotChild1.CI_OP = part.PK;
			CusClassPartPivot pivotChild2 = pivot.Children.AddNew();
			pivotChild2.CI_UsageComment = "CU2";
			pivotChild2.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			pivotChild2.CI_TariffNum = "1010101012";
			pivotChild2.CI_SupplementalTariff = "2020202022";
			pivotChild2.CI_CC = classification.PK;
			pivotChild2.CI_OP = part.PK;

			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_MessageType = JobMessageTypeList.Codes.Import;
			invoice.JZ_OH_Buyer = consignee.PK;
			invoice.JZ_OH_Supplier = consignor.PK;
			new FakeDeclarationCreatorForInvoice(invoice);
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.PartSyncManager.Enabled = false;
			invoiceLine.JI_PartNo = "PART1";
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_PartNo = "";
			var childInvoiceLine = invoice.InvoiceLines.AddNew();
			childInvoiceLine.JI_ParentID = invoiceLine2.PK;

			AssertNull("Precondition:", invoiceLine.PartSyncManager.Part);
			AssertEquals("Precondition:", ZGuid.Empty, invoiceLine.JI_OP);
			AssertEquals("Precondition:", 0, invoiceLine.ChildLines.Count());
			AssertEquals("Precondition:", 1, invoiceLine2.ChildLines.Count());

			invoiceLine.UpdatePartSyncManagerAndRefresh(true);
			Assert("PartSyncManager.Enabled should be set to true", invoiceLine.PartSyncManager.Enabled);
			AssertEquals("PartSyncManager.Part should be set", part.PK, invoiceLine.PartSyncManager.Part.PK);
			AssertEquals("JI_OP should be set", part.PK, invoiceLine.JI_OP);
			AssertEquals("ChildLines should be created", 1, invoiceLine.ChildLines.Count());
			AssertEquals("ProductRelatedLines should be created", 1, invoiceLine.ProductRelatedLines.Count());
			AssertEquals("ChildLines should not be deleted", 1, invoiceLine2.ChildLines.Count());

			invoiceLine.UpdatePartSyncManagerAndRefresh(false);
			Assert("PartSyncManager.Enabled should be set to false", !invoiceLine.PartSyncManager.Enabled);
			AssertNull("PartSyncManager.Part should be cleared", invoiceLine.PartSyncManager.Part);
			AssertEquals("JI_OP should be cleared", ZGuid.Empty, invoiceLine.JI_OP);
			AssertEquals("ChildLines should be deleted", 0, invoiceLine.ChildLines.Count());
			AssertEquals("ProductRelatedLines should be deleted", 0, invoiceLine.ProductRelatedLines.Count());
			AssertEquals("ChildLines should not be deleted", 1, invoiceLine2.ChildLines.Count());
		}

		public void TestRestoreCustomFieldsWhenSettingUpProduct()
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.FillWithValidTestData();
			consignor.OH_Code = "ORGAUSYD";
			consignor.OH_IsConsignor = ZBool.True;
			consignor.OH_RL_NKClosestPort = "AUSYD";
			var consignee = Factory.New<OrgHeader>();
			consignee.FillWithValidTestData();
			consignee.OH_Code = "ORGUSCHI";
			consignee.OH_IsConsignee = ZBool.True;
			consignee.OH_RL_NKClosestPort = "USCHI";

			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_MessageType = JobMessageTypeList.Codes.Import;
			invoice.JZ_OH_Buyer = consignee.PK;
			invoice.JZ_OH_Supplier = consignor.PK;
			new FakeDeclarationCreatorForInvoice(invoice);
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.PartSyncManager.Enabled = false;
			invoiceLine.JI_PartNo = "PART1";
			var childInvoiceLine = invoice.InvoiceLines.AddNew();
			childInvoiceLine.JI_ParentID = invoiceLine.PK;
			childInvoiceLine.JI_Tariff = "1010101011";
			var childInvoiceLine2 = invoice.InvoiceLines.AddNew();
			childInvoiceLine2.JI_ParentID = invoiceLine.PK;
			childInvoiceLine2.JI_Tariff = "1010101012";
			var childInvoiceLine3 = invoice.InvoiceLines.AddNew();
			childInvoiceLine3.JI_ParentID = invoiceLine.PK;
			childInvoiceLine3.JI_Tariff = "1010101019";
			var customField = "Kering 15";
			childInvoiceLine.SetUserDefinedValue(customField, new ZString("YA10"));
			childInvoiceLine2.SetUserDefinedValue(customField, new ZString("YA11"));
			childInvoiceLine3.SetUserDefinedValue(customField, new ZString("YA12"));

			Factory.Save();

			var classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "TestLookup";
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PART1";
			part.OP_Weight = 100m;
			part.OP_WeightUQ = "HG";
			part.RelatedOrganisations.AddSupplier(consignor);
			part.RelatedOrganisations.AddOwner(consignee);
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_UsageComment = "U1";
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "1010101010";
			pivot.CI_SupplementalTariff = "2020202020";
			pivot.CI_CC = classification.PK;
			pivot.CI_OP = part.PK;
			var pivotChild1 = pivot.Children.AddNew();
			pivotChild1.CI_UsageComment = "CU1";
			pivotChild1.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			pivotChild1.CI_TariffNum = "1010101011";
			pivotChild1.CI_SupplementalTariff = "2020202021";
			pivotChild1.CI_CC = classification.PK;
			pivotChild1.CI_OP = part.PK;
			var pivotChild2 = pivot.Children.AddNew();
			pivotChild2.CI_UsageComment = "CU1";
			pivotChild2.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			pivotChild2.CI_TariffNum = "1010101012";
			pivotChild2.CI_SupplementalTariff = "2020202021";
			pivotChild2.CI_CC = classification.PK;
			pivotChild2.CI_OP = part.PK;
			var pivotChild3 = pivot.Children.AddNew();
			pivotChild3.CI_UsageComment = "CU1";
			pivotChild3.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			pivotChild3.CI_TariffNum = "1010101013";
			pivotChild3.CI_SupplementalTariff = "2020202021";
			pivotChild3.CI_CC = classification.PK;
			pivotChild3.CI_OP = part.PK;

			Factory.Save();

			invoiceLine.UpdateDetailsOnPartChange();
			AssertEquals("ChildLines should be created", 3, invoiceLine.ChildLines.Count());
			AssertEquals("Custom value set before product creation should persist", "YA10", invoiceLine.ChildLines.FirstOrDefault(l => l.JI_Tariff == "1010101011").GetCustomField(customField, null));
			AssertEquals("Custom value set before product creation should persist", "YA11", invoiceLine.ChildLines.FirstOrDefault(l => l.JI_Tariff == "1010101012").GetCustomField(customField, null));
			AssertEquals("Custom value set before product creation should persist", "YA12", invoiceLine.ChildLines.FirstOrDefault(l => l.JI_Tariff == "1010101019").GetCustomField(customField, null));
		}

		public void TestDontCopyPGADetailsFromProductForWarehouseWithdrawEntryIfNotSupported()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "INCIMP";
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "INCTEST";

			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = importer.PK;
			relOrg.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = USCTariff.CottonFeeApplicable;
			pivot.CD_ACEFDAIndicator = OGAIndicatorList.Codes.Declared;
			var aceFDA = pivot.ACEFDAs.AddNew();
			aceFDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			pivot.CD_NHTSAIndicator = OGAIndicatorList.Codes.Declared;
			var productNHTSA = pivot.NHTSALines.AddNew();
			productNHTSA.US_NHTProgramCode = NHTSAProgramCodeList.Codes.MVS;
			pivot.CD_LaceyActIndicator = OGAIndicatorList.Codes.Declared;
			var lacey = pivot.PGAs.AddNew();
			lacey.US_PGACommercialDescription = "LACEY";
			pivot.CD_ATFIndicator = OGAIndicatorList.Codes.Declared;
			var atf = pivot.ATFLines.AddNew();
			atf.US_CategoryCode = ATFCategoryCodeList.Codes.ABL;
			pivot.CD_TSCAIndicator = OGAIndicatorList.Codes.Declared;
			pivot.US_TSCACertification = TSCAIndicatorList.Codes.TSCANegative;
			pivot.CD_PSTIndicator = OGAIndicatorList.Codes.Declared;
			var pst = pivot.PSTLines.AddNew();
			pst.US_ProductType = PSTProductTypeList.Codes.PS1;
			pivot.CD_OMCIndicator = OGAIndicatorList.Codes.Declared;
			var omc = pivot.OMCHeaders.AddNew();
			omc.US_SourceCountry = Core.Constants.CountryCodes.UnitedStates;
			pivot.CD_VNEIndicator = OGAIndicatorList.Codes.Declared;
			var vne = pivot.VehicleLines.AddNew();
			vne.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_1;
			pivot.CD_TTBIndicator = OGAIndicatorList.Codes.Declared;
			var ttb = pivot.TTBLines.AddNew();
			ttb.US_NumberForIRC = "11111";
			pivot.CD_CPSCIndicator = OGAIndicatorList.Codes.Declared;
			var cpsc = pivot.CPSCLines.AddNew();
			cpsc.US_ProcessingCode = CPSCProcessingCodeList.Codes.FCP;
			pivot.CD_DEAIndicator = OGAIndicatorList.Codes.Declared;
			var dea = pivot.DEAHeaders.AddNew();
			dea.US_CountryOfShipment = Core.Constants.CountryCodes.Canada;
			pivot.CD_APHISIndicator = OGAIndicatorList.Codes.Declared;
			var aphis = pivot.APHISHeaders.AddNew();
			aphis.US_ProgramType = APHISProgramCodeList.Codes.AAC;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "INCTEST";
			CombineAssertions("PGA Indicator for entry type '31'", () =>
			{
				AssertEquals("FDA Indicator", ZString.Empty, invoiceLine.US_FDAIndicator);
				AssertEquals("FDA Count", 0, invoiceLine.ACE_FDALines.Count);
				AssertEquals("NHTSA Indicator", ZString.Empty, invoiceLine.US_NHTSAIndicator);
				AssertEquals("NHTSA Count", 0, invoiceLine.NHTSALines.Count);
				AssertEquals("Lacey Indicator", OGAIndicatorList.Codes.Declared, invoiceLine.US_LaceyIndicator);
				AssertEquals("Lacey Count", 1, invoiceLine.LaceyActLines.Count);
				AssertEquals("ATF Indicator", ZString.Empty, invoiceLine.US_ATFInd);
				AssertEquals("ATF Count", 0, invoiceLine.ATFLines.Count);
				AssertEquals("TSCA Indicator", ZString.Empty, invoiceLine.US_TSCAInd);
				AssertEquals("TSCA Certification", ZString.Empty, invoiceLine.US_TSCACertification);
				AssertEquals("PST Indicator", ZString.Empty, invoiceLine.US_PSTIndicator);
				AssertEquals("PST Count", 0, invoiceLine.PSTLines.Count);
				AssertEquals("OMC Indicator", OGAIndicatorList.Codes.Declared, invoiceLine.US_OMCInd);
				AssertEquals("OMC Count", 1, invoiceLine.OMCHeaders.Count);
				AssertEquals("VNE Indicator", ZString.Empty, invoiceLine.US_VNEInd);
				AssertEquals("VNE Count", 0, invoiceLine.VehicleLines.Count);
				AssertEquals("TTB Indicator", OGAIndicatorList.Codes.Declared, invoiceLine.US_TTBInd);
				AssertEquals("TTB Count", 1, invoiceLine.TTBLines.Count);
				AssertEquals("CPSC Indicator", ZString.Empty, invoiceLine.US_CPSCInd);
				AssertEquals("CPSC Count", 0, invoiceLine.CPSCHeaders.Count);
				AssertEquals("DEA Indicator", OGAIndicatorList.Codes.Declared, invoiceLine.US_DEAInd);
				AssertEquals("DEA Count", 1, invoiceLine.DEAHeaders.Count);
				AssertEquals("APHIS Indicator", ZString.Empty, invoiceLine.US_APHISInd);
				AssertEquals("APHIS Count", 0, invoiceLine.APHISHeaders.Count);
			});

			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalQuota;
			invoice.InvoiceLines.RemoveAndDeleteAll();
			invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "INCTEST";
			CombineAssertions("PGA Indicator for entry type '32'", () =>
			{
				AssertEquals("FDA Indicator", ZString.Empty, invoiceLine.US_FDAIndicator);
				AssertEquals("FDA Count", 0, invoiceLine.ACE_FDALines.Count);
				AssertEquals("NHTSA Indicator", ZString.Empty, invoiceLine.US_NHTSAIndicator);
				AssertEquals("NHTSA Count", 0, invoiceLine.NHTSALines.Count);
				AssertEquals("Lacey Indicator", OGAIndicatorList.Codes.Declared, invoiceLine.US_LaceyIndicator);
				AssertEquals("Lacey Count", 1, invoiceLine.LaceyActLines.Count);
				AssertEquals("ATF Indicator", ZString.Empty, invoiceLine.US_ATFInd);
				AssertEquals("ATF Count", 0, invoiceLine.ATFLines.Count);
				AssertEquals("TSCA Indicator", ZString.Empty, invoiceLine.US_TSCAInd);
				AssertEquals("TSCA Certification", ZString.Empty, invoiceLine.US_TSCACertification);
				AssertEquals("PST Indicator", ZString.Empty, invoiceLine.US_PSTIndicator);
				AssertEquals("PST Count", 0, invoiceLine.PSTLines.Count);
				AssertEquals("OMC Indicator", OGAIndicatorList.Codes.Declared, invoiceLine.US_OMCInd);
				AssertEquals("OMC Count", 1, invoiceLine.OMCHeaders.Count);
				AssertEquals("VNE Indicator", ZString.Empty, invoiceLine.US_VNEInd);
				AssertEquals("VNE Count", 0, invoiceLine.VehicleLines.Count);
				AssertEquals("TTB Indicator", OGAIndicatorList.Codes.Declared, invoiceLine.US_TTBInd);
				AssertEquals("TTB Count", 1, invoiceLine.TTBLines.Count);
				AssertEquals("CPSC Indicator", ZString.Empty, invoiceLine.US_CPSCInd);
				AssertEquals("CPSC Count", 0, invoiceLine.CPSCHeaders.Count);
				AssertEquals("DEA Indicator", OGAIndicatorList.Codes.Declared, invoiceLine.US_DEAInd);
				AssertEquals("DEA Count", 1, invoiceLine.DEAHeaders.Count);
				AssertEquals("APHIS Indicator", ZString.Empty, invoiceLine.US_APHISInd);
				AssertEquals("APHIS Count", 0, invoiceLine.APHISHeaders.Count);
			});
		}

		public void TestCopyCPSCFromProduct()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "CPSCIMP";
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "CPSCTST";

			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = importer.PK;
			relOrg.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = USCTariff.CottonFeeApplicable;
			pivot.CD_CPSCIndicator = OGAIndicatorList.Codes.Declared;

			var productCPSC = pivot.CPSCLines.AddNew();
			productCPSC.US_ProcessingCode = CPSCProcessingCodeList.Codes.REF;
			productCPSC.US_ReferenceNumber = "123456";

			var pRuleAndLab = productCPSC.RuleAndLabs.AddNew();
			pRuleAndLab.US_RuleCodes = "12C23,45F34";

			var labReportInfo = pRuleAndLab.ReportAndLabs.AddNew();
			labReportInfo.US_RemarksText = "11111";
			labReportInfo.US_RemarksType = LabReportInformationTypeList.Codes.CP1;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "CPSCTST";

			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_CPSCInd);
			AssertEquals(1, invoiceLine.CPSCHeaders.Count);

			var invoiceLineCPSC = invoiceLine.CPSCHeaders[0];
			AssertEquals(CPSCProcessingCodeList.Codes.REF, invoiceLineCPSC.US_ProcessingCode);
			AssertEquals("123456", invoiceLineCPSC.US_ReferenceNumber);

			AssertEquals(1, invoiceLineCPSC.RuleAndLabs.Count);
			var rule = invoiceLineCPSC.RuleAndLabs[0];
			AssertEquals("12C23,45F34", rule.US_RuleCodes);

			AssertEquals(1, rule.ReportAndLabs.Count);
			var report = rule.ReportAndLabs[0];
			AssertEquals("11111", report.US_RemarksText);
			AssertEquals(LabReportInformationTypeList.Codes.CP1, report.US_RemarksType);

			product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "CPSCTST_Disclaimed";
			relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = importer.PK;
			relOrg.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			pivot = product.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = USCTariff.CottonFeeApplicable;
			pivot.CD_CPSCIndicator = OGAIndicatorList.Codes.Disclaimed;
			pivot.CD_CPSCDisclaimReason = PGADisclaimReasonList.Codes.A;
			AssertEquals(1, pivot.CPSCLines.Count);

			productCPSC = pivot.CPSCLines[0];
			productCPSC.US_IntendedUseCode = "980.000";
			productCPSC.US_IntendedUseDescription = "TEST AAA";

			invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "CPSCTST_Disclaimed";
			AssertEquals(1, invoiceLine.CPSCHeaders.Count);

			invoiceLineCPSC = invoiceLine.CPSCHeaders[0];
			AssertEquals("980.000", invoiceLineCPSC.US_IntendedUseCode);
			AssertEquals("TEST AAA", invoiceLineCPSC.US_IntendedUseDescription);
		}

		public void TestCopyNHTSAFromProduct()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "INCIMP";
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "INCTEST";

			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = importer.PK;
			relOrg.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = USCTariff.CottonFeeApplicable;
			pivot.CD_NHTSAIndicator = OGAIndicatorList.Codes.Declared;

			var productNHTSA = pivot.NHTSALines.AddNew();
			productNHTSA.US_NHTProgramCode = NHTSAProgramCodeList.Codes.MVS;
			productNHTSA.US_NHTBoxNumber = DepartmentOfTransportBoxNumberList.Codes._2B;
			productNHTSA.US_NHTDOTSuretyCode = "123";
			productNHTSA.US_TrackingStatus = PGATrackingStatusList.Codes.Adding;

			var productDetails = productNHTSA.NHTSADetails.AddNew();
			productDetails.US_NHTBrandName = "ABC";
			productDetails.US_NHTIdentityNumber = "14561238413";
			productDetails.US_NHTCategoryCode = NHTSACategoryCode_MVSTYPList.Codes.MVS1;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "INCTEST";
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_NHTSAIndicator);
			AssertEquals(1, invoiceLine.NHTSALines.Count);

			var invoiceLineNHTSA = invoiceLine.NHTSALines[0];
			AssertEquals(NHTSAProgramCodeList.Codes.MVS, invoiceLineNHTSA.US_NHTProgramCode);
			AssertEquals(DepartmentOfTransportBoxNumberList.Codes._2B, invoiceLineNHTSA.US_NHTBoxNumber);
			AssertEquals("123", invoiceLineNHTSA.US_NHTDOTSuretyCode);
			AssertEquals(ZString.Empty, invoiceLineNHTSA.US_TrackingStatus);
			AssertEquals(1, invoiceLineNHTSA.NHTSADetails.Count);

			var invoiceNHTSADetails = invoiceLineNHTSA.NHTSADetails[0];
			AssertEquals("ABC", invoiceNHTSADetails.US_NHTBrandName);
			AssertEquals("14561238413", invoiceNHTSADetails.US_NHTIdentityNumber);
			AssertEquals(NHTSACategoryCode_MVSTYPList.Codes.MVS1, invoiceNHTSADetails.US_NHTCategoryCode);
		}

		public void TestCopyPSTFromProduct()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "SUNIMP";
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "SUNTEST";

			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = importer.PK;
			relOrg.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = USCTariff.CottonFeeApplicable;
			pivot.CD_PSTIndicator = OGAIndicatorList.Codes.Declared;
			pivot.CD_PSTDisclaimReason = "A";
			pivot.CD_PSTDisclaimProgram = PSTProductTypeList.Codes.PS2;

			var productPST = pivot.PSTLines.AddNew();
			productPST.US_IntendedUseCode = "2000";
			productPST.US_ProductType = "PS3";
			productPST.US_BrandName = "Brand";
			productPST.US_TrackingStatus = PGATrackingStatusList.Codes.Adding;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "SUNTEST";
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_PSTIndicator);
			AssertEquals("A", invoiceLine.US_PSTDisclaimReason);
			AssertEquals(PSTProductTypeList.Codes.PS2, invoiceLine.US_PSTDisclaimProgram);
			AssertEquals(1, invoiceLine.PSTLines.Count);

			var invoiceLinePST = invoiceLine.PSTLines[0];
			AssertEquals("2000", invoiceLinePST.US_IntendedUseCode);
			AssertEquals("PS3", invoiceLinePST.US_ProductType);
			AssertEquals("Brand", invoiceLinePST.US_BrandName);
			AssertEquals(ZString.Empty, invoiceLinePST.US_TrackingStatus);
		}

		public void TestCopyOMCFromProduct()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "OMCIMP";
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "OMCTST";

			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = importer.PK;
			relOrg.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = USCTariff.CottonFeeApplicable;
			pivot.CD_OMCIndicator = OGAIndicatorList.Codes.Declared;

			var productOMC = pivot.OMCHeaders.AddNew();
			productOMC.US_NetWeight = 10m;

			var aquacultureFacilities = productOMC.AquacultureFacilities.AddNew();
			aquacultureFacilities.US_OA_AquacultureFacility = importer.MainAddress.PK;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "OMCTST";
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_OMCInd);
			AssertEquals(1, invoiceLine.OMCHeaders.Count);

			var invoiceLineOMC = invoiceLine.OMCHeaders[0];
			AssertEquals(10m, invoiceLineOMC.US_NetWeight);
			AssertEquals(1, invoiceLineOMC.AquacultureFacilities.Count);
			var aquacultureFacility = invoiceLineOMC.AquacultureFacilities[0];
			AssertEquals(importer.MainAddress.PK, aquacultureFacility.US_OA_AquacultureFacility);
		}

		public void TestCopyVNEFromProduct()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "SUNIMP";
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "SUNTEST";

			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = importer.PK;
			relOrg.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = USCTariff.CottonFeeApplicable;
			pivot.CD_VNEIndicator = OGAIndicatorList.Codes.Declared;
			pivot.CD_VNEDisclaimReason = "A";

			var productVNE = pivot.VehicleLines.AddNew();
			productVNE.US_FormType = "3520-1";
			productVNE.US_VehicleModel = "IOK";
			productVNE.US_ImportCode = "AAA";
			productVNE.US_ModelYear = "2015";
			productVNE.US_TrackingStatus = PGATrackingStatusList.Codes.Adding;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "SUNTEST";
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_VNEInd);
			AssertEquals("A", invoiceLine.US_VNEDisclaimReason);
			AssertEquals(1, invoiceLine.VehicleLines.Count);

			var invoiceLineVNE = invoiceLine.VehicleLines[0];
			AssertEquals("3520-1", invoiceLineVNE.US_FormType);
			AssertEquals("IOK", invoiceLineVNE.US_VehicleModel);
			AssertEquals("AAA", invoiceLineVNE.US_ImportCode);
			AssertEquals("2015", invoiceLineVNE.US_ModelYear);
			AssertEquals(ZString.Empty, invoiceLineVNE.US_TrackingStatus);
		}

		public void TestCopyODSTSCAFromProduct()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "SUNIMP";
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "SUNTEST";

			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = importer.PK;
			relOrg.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = USCTariff.CottonFeeApplicable;
			pivot.CD_ODSIndicator = OGAIndicatorList.Codes.Declared;
			pivot.CD_ODSDisclaimReason = "A";
			pivot.CD_TSCAClaimIndicator = OGAIndicatorList.Codes.Declared;
			pivot.CD_TSCADisclaimReason = "A";
			pivot.CD_TSCAIndicator = "+";
			pivot.CD_TSCAODSCertIndividual = PartyTypeList.Codes.CustomsBroker;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "SUNTEST";
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_ODSInd);
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_TSCAInd);
			AssertEquals("A", invoiceLine.US_ODSDisclaimReason);
			AssertEquals("A", invoiceLine.US_TSCADisclaimReason);
			AssertEquals("+", invoiceLine.US_TSCACertification);
			AssertEquals(PartyTypeList.Codes.CustomsBroker, invoiceLine.US_TSCAODSCertIndividual);
		}

		public void TestJI_OA_Seller_Effective()
		{
			var sellerOrg1 = Factory.New<OrgHeader>();
			sellerOrg1.OH_Code = "TESTSELLER1";
			var sellerOrg2 = Factory.New<OrgHeader>();
			sellerOrg2.OH_Code = "TESTSELLER2";

			InvoiceHeader.JZ_OA_SellerAddress = sellerOrg1.MainAddress.PK;
			InvoiceLine.JI_OA_Seller = sellerOrg2.MainAddress.PK;
			AssertEquals(sellerOrg2.MainAddress.PK, InvoiceLine.JI_OA_Seller);

			InvoiceLine.JI_OA_Seller = ZGuid.Empty;
			AssertEquals(sellerOrg1.MainAddress.PK, InvoiceLine.JI_OA_Seller);
		}

		public void TestCopyFromDDTCProduct()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "INCIMP";
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "INCTEST";

			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = importer.PK;
			relOrg.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = USCTariff.CottonFeeApplicable;
			pivot.Details.CD_DDTCIndicator = OGAIndicatorList.Codes.Declared;

			pivot.CD_ITARExemptionNo = ExemptionCodesCodeList.Codes._1;
			pivot.CD_DDTCRegoNo = "RegNo";
			pivot.CD_LicenceNo = "LicenceNo";
			pivot.CD_LicenceType = "LTP";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "INCTEST";
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_DDTCInd);

			AssertEquals("RegNo", invoiceLine.US_DDTCRegistrationNo);
			AssertEquals("LicenceNo", invoiceLine.US_LicenseNo);
			AssertEquals("LTP", invoiceLine.US_LicenseType);
			AssertEquals(ExemptionCodesCodeList.Codes._1, invoiceLine.US_DDTCITARExemptionNo);
		}

		public void TestCopyDepositRateOverrideFromProduct()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "INCIMP";
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "INCTEST";

			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = importer.PK;
			relOrg.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = USCTariff.CottonFeeApplicable;
			pivot.CD_ADDDepositRateOverride = 0.234m;
			pivot.CD_CVDDepositRateOverride = 0.235m;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "INCTEST";
			AssertEquals(pivot.CD_ADDDepositRateOverride, invoiceLine.US_ADDDepositRateOverride);
			AssertEquals(pivot.CD_CVDDepositRateOverride, invoiceLine.US_CVDDepositRateOverride);
		}

		public void TestCopyATFFromProduct()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "INCIMP";
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "INCTEST";

			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = importer.PK;
			relOrg.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = USCTariff.CottonFeeApplicable;
			pivot.CD_ATFIndicator = OGAIndicatorList.Codes.Declared;

			var productATF = pivot.ATFLines.AddNew();
			productATF.US_AECAExemptionCode = ExemptionCodesCodeList.Codes._1;
			productATF.US_AECANumber = "ACE1122";
			productATF.US_BarrelLength = 5.00;
			productATF.US_CaliberGaugeSize = "5";
			productATF.US_CategoryCode = "CODE";
			productATF.US_ExtendedDescription = "FROMPRODUCT";
			productATF.US_FELExemptionCode = ExemptionCodesCodeList.Codes._1;
			productATF.US_FELNumber = "FEL2233";
			productATF.US_Model = "MODEL";
			productATF.US_OverallLength = 10.00;
			productATF.US_PermitExemptionCode = ExemptionCodesCodeList.Codes._1;
			productATF.US_PermitNumber = "PER4455";
			productATF.US_TrackingStatus = PGATrackingStatusList.Codes.Adding;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "INCTEST";
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_ATFInd);
			AssertEquals(1, invoiceLine.ATFLines.Count);

			var invoiceLineATF = invoiceLine.ATFLines[0];
			AssertEquals(ExemptionCodesCodeList.Codes._1, invoiceLineATF.US_AECAExemptionCode);

			AssertEquals("ACE1122", invoiceLineATF.US_AECANumber);
			AssertEquals((ZDecimal)5, invoiceLineATF.US_BarrelLength);
			AssertEquals("5", invoiceLineATF.US_CaliberGaugeSize);
			AssertEquals("CODE", invoiceLineATF.US_CategoryCode);
			AssertEquals("FROMPRODUCT", invoiceLineATF.US_ExtendedDescription);

			AssertEquals(ExemptionCodesCodeList.Codes._1, invoiceLineATF.US_FELExemptionCode);
			AssertEquals("FEL2233", invoiceLineATF.US_FELNumber);
			AssertEquals("MODEL", invoiceLineATF.US_Model);

			AssertEquals((ZDecimal)10, invoiceLineATF.US_OverallLength);
			AssertEquals(ExemptionCodesCodeList.Codes._1, invoiceLineATF.US_PermitExemptionCode);
			AssertEquals("PER4455", invoiceLineATF.US_PermitNumber);
			AssertEquals(ZString.Empty, invoiceLineATF.US_TrackingStatus);
		}

		public void TestCopyDEAFromProduct()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "DEAIMP";
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "DEATST";

			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = importer.PK;
			relOrg.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = USCTariff.CottonFeeApplicable;
			pivot.CD_DEAIndicator = OGAIndicatorList.Codes.Declared;

			var productDEA = pivot.DEAHeaders.AddNew();
			productDEA.US_CountryOfShipment = "US";
			productDEA.US_FormID = "DEA-35";
			productDEA.US_RegistrationNumber = "123456789";

			var productConstituent = productDEA.Constituents.AddNew();
			productConstituent.US_ProductCode = "4000";
			productConstituent.US_WeightUQ = "KG";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "DEATST";
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_DEAInd);
			AssertEquals(1, invoiceLine.DEAHeaders.Count);

			var invoiceLineDEA = invoiceLine.DEAHeaders[0];
			AssertEquals("US", invoiceLineDEA.US_CountryOfShipment);
			AssertEquals("DEA-35", invoiceLineDEA.US_FormID);
			AssertEquals("123456789", invoiceLineDEA.US_RegistrationNumber);

			AssertEquals(1, invoiceLineDEA.Constituents.Count);

			var constituent = invoiceLineDEA.Constituents[0];
			AssertEquals("4000", constituent.US_ProductCode);
			AssertEquals("KG", constituent.US_WeightUQ);
		}

		public void TestTSCAPGAContactDetails()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTORG";

			DeclarationTestHelper.AddPGAContact(orgHeader, "IMPORTER", "TEST", "1234567", "importer@abc.com", null);

			Declaration.IOROrgPK = orgHeader.PK;
			InvoiceHeader.US_FDAContactName = "BROKER NAME";
			InvoiceHeader.US_FDAContactPhoneNo = "2345678";
			InvoiceHeader.US_FDAContactEmail = "broker@abc.com";

			InvoiceLine.US_TSCAODSCertIndividual = PartyTypeList.Codes.Importer;
			AssertEquals("IMPORTER TEST", InvoiceLine.US_FDAContactName);
			AssertEquals("1234567", InvoiceLine.US_FDAContactPhoneNo);
			AssertEquals("importer@abc.com", InvoiceLine.US_FDAContactEmail);

			InvoiceLine.US_TSCAODSCertIndividual = PartyTypeList.Codes.CustomsBroker;
			AssertEquals("BROKER NAME", InvoiceLine.US_FDAContactName);
			AssertEquals("2345678", InvoiceLine.US_FDAContactPhoneNo);
			AssertEquals("broker@abc.com", InvoiceLine.US_FDAContactEmail);
		}

		public void TestUS_TSCAODSCertIndividualWhenContactPhoneNoIsTooLarge()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTORG";

			var emailname = "";
			for (int i = 0; i < 23; i++)
			{
				emailname += "1234567890";
			}
			DeclarationTestHelper.AddPGAContact(orgHeader, "IMPORTER12123456789012345", "TEST12345612345678901234", "12345678901234567890", emailname + "12345678importer@abc.com", null);

			Declaration.IOROrgPK = orgHeader.PK;
			InvoiceHeader.US_FDAContactName = "BROKER NAME";
			InvoiceHeader.US_FDAContactPhoneNo = "2345678";
			InvoiceHeader.US_FDAContactEmail = "broker@abc.com";

			InvoiceLine.US_TSCAODSCertIndividual = PartyTypeList.Codes.Importer;
			AssertEquals("IMPORTER12123456789012345 TEST12345612345678901234", InvoiceLine.US_FDAContactName);
			AssertEquals("6789012345", InvoiceLine.US_FDAContactPhoneNo);
			AssertEquals("1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678importer@abc.com", InvoiceLine.US_FDAContactEmail);

			InvoiceLine.US_TSCAODSCertIndividual = PartyTypeList.Codes.CustomsBroker;
			AssertEquals("BROKER NAME", InvoiceLine.US_FDAContactName);
			AssertEquals("2345678", InvoiceLine.US_FDAContactPhoneNo);
			AssertEquals("broker@abc.com", InvoiceLine.US_FDAContactEmail);
		}

		public void TestNHTSAPGAContactDetails()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTORG";

			DeclarationTestHelper.AddPGAContact(orgHeader, "IMPORTER", "TEST", "1234567", "importer@abc.com", null);

			Declaration.IOROrgPK = orgHeader.PK;
			InvoiceHeader.US_FDAContactName = "BROKER NAME";
			InvoiceHeader.US_FDAContactPhoneNo = "2345678";
			InvoiceHeader.US_FDAContactEmail = "broker@abc.com";

			var nhtsa = InvoiceLine.NHTSALines.AddNew();
			nhtsa.US_CertifyingIndividual = PartyTypeList.Codes.Importer;
			AssertEquals("IMPORTER TEST", nhtsa.US_PGAContactName);
			AssertEquals("1234567", nhtsa.US_PGAContactPhoneNo);
			AssertEquals("importer@abc.com", nhtsa.US_PGAContactEmail);

			nhtsa.US_CertifyingIndividual = PartyTypeList.Codes.CustomsBroker;
			AssertEquals("BROKER NAME", nhtsa.US_PGAContactName);
			AssertEquals("2345678", nhtsa.US_PGAContactPhoneNo);
			AssertEquals("broker@abc.com", nhtsa.US_PGAContactEmail);
		}

		public void TestCopyLaceyPGAContactDetailsFromProduct()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTORG";

			DeclarationTestHelper.AddPGAContact(orgHeader, "IMPORTER", "TEST", "1234567", "importer@abc.com", null);

			Declaration.IOROrgPK = orgHeader.PK;
			InvoiceHeader.US_FDAContactName = "BROKER NAME";
			InvoiceHeader.US_FDAContactPhoneNo = "2345678";
			InvoiceHeader.US_FDAContactEmail = "broker@abc.com";

			var laceyLine = InvoiceLine.LaceyActLines.AddNew();
			InvoiceLine.US_LaceyIndicator = OGAIndicatorList.Codes.Declared;

			laceyLine.US_CertifyingIndividual = PartyTypeList.Codes.Importer;
			AssertEquals("IMPORTER TEST", laceyLine.US_PGAContactName);
			AssertEquals("1234567", laceyLine.US_PGAContactPhoneNo);
			AssertEquals("importer@abc.com", laceyLine.US_PGAContactEmail);

			laceyLine.US_CertifyingIndividual = PartyTypeList.Codes.CustomsBroker;
			AssertEquals("BROKER NAME", laceyLine.US_PGAContactName);
			AssertEquals("2345678", laceyLine.US_PGAContactPhoneNo);
			AssertEquals("broker@abc.com", laceyLine.US_PGAContactEmail);
		}

		public void TestCopyVNEPGAContactDetailsFromProduct()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTORG";

			DeclarationTestHelper.AddPGAContact(orgHeader, "IMPORTER", "TEST", "1234567", "importer@abc.com", null);

			Declaration.IOROrgPK = orgHeader.PK;
			InvoiceHeader.US_FDAContactName = "BROKER NAME";
			InvoiceHeader.US_FDAContactPhoneNo = "2345678";
			InvoiceHeader.US_FDAContactEmail = "broker@abc.com";

			var vehicle = InvoiceLine.VehicleLines.AddNew();
			InvoiceLine.US_LaceyIndicator = OGAIndicatorList.Codes.Declared;

			vehicle.US_CertifyingIndividual = PartyTypeList.Codes.Importer;
			AssertEquals("IMPORTER TEST", vehicle.US_ContactName);
			AssertEquals("1234567", vehicle.US_ContactPhoneNo);
			AssertEquals("importer@abc.com", vehicle.US_ContactEmail);

			vehicle.US_CertifyingIndividual = PartyTypeList.Codes.CustomsBroker;
			AssertEquals("BROKER NAME", vehicle.US_ContactName);
			AssertEquals("2345678", vehicle.US_ContactPhoneNo);
			AssertEquals("broker@abc.com", vehicle.US_ContactEmail);
		}

		public void TestCopyHFCPGAContactDetailsFromProduct()
		{
			var declaration = GetImportDeclarationWithBuyerSupplier();
			var product = GetPartWithRelationship(declaration.Importer);
			var invoice = declaration.Invoices.AddNew();

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_DateStart = ZDateTime.Now.AddDays(-10);
			pivot.CI_TariffNum = "00044400";
			pivot.CD_HFCIndicator = OGAIndicatorList.Codes.Declared;
			pivot.CD_HFCDisclaimReason = PGADisclaimReasonList.Codes.A;

			var hfcHeader = pivot.HFCHeaders.AddNew();
			hfcHeader.US_CertifyingIndividual = EntityRoleCodeList.Codes.Consignee;
			hfcHeader.US_NetWeight = 50m;
			hfcHeader.US_HFCImageSent = true;
			var hfcDetail = hfcHeader.USHFCDetails.AddNew();
			hfcDetail.US_LPCONumber = "001";
			hfcDetail.US_ActiveIngredientPercentage = 100m;
			hfcDetail.US_NameOfActiveIngredient = "KG";

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = product.OP_PartNum;

			AssertEquals(1, invoiceLine.USHFCHeaders.Count);
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_HFCInd);
			AssertEquals(PGADisclaimReasonList.Codes.A, invoiceLine.US_HFCDisclaimReason);

			var usHFCHeader = invoiceLine.USHFCHeaders[0];
			AssertEquals("US_NetWeight", 50m, usHFCHeader.US_NetWeight);
			AssertEquals("US_HFCImageSent should not be cloned", false, usHFCHeader.US_HFCImageSent);
			AssertEquals("US_CertifyingIndividual", EntityRoleCodeList.Codes.Consignee, usHFCHeader.US_CertifyingIndividual);
			var usHFCDetail = usHFCHeader.USHFCDetails[0];
			AssertEquals("US_LPCONumber", "001", usHFCDetail.US_LPCONumber);
			AssertEquals("US_ActiveIngredientPercentage", 100m, usHFCDetail.US_ActiveIngredientPercentage);
			AssertEquals("US_NameOfActiveIngredient", "KG", usHFCDetail.US_NameOfActiveIngredient);
		}

		public void TestCopyNHTSAPGAContactDetailsFromProduct()
		{
			var declaration = GetImportDeclarationWithBuyerSupplier();
			var product = GetPartWithRelationship(declaration.Importer);
			var invoice = declaration.Invoices.AddNew();

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_DateStart = ZDateTime.Now.AddDays(-10);
			pivot.CI_TariffNum = "00044400";

			var productNHTSA0 = pivot.NHTSALines.AddNew();
			productNHTSA0.US_CertifyingIndividual = PartyTypeList.Codes.CustomsBroker;
			var productNHTSA1 = pivot.NHTSALines.AddNew();
			productNHTSA1.US_CertifyingIndividual = PartyTypeList.Codes.Importer;
			var productNHTSA2 = pivot.NHTSALines.AddNew();
			productNHTSA2.US_CertifyingIndividual = PartyTypeList.Codes.Owner;

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTORG";
			DeclarationTestHelper.AddPGAContact(orgHeader, "IMPORTER", "TEST", "1234567", "importer@abc.com", null);

			declaration.IOROrgPK = orgHeader.PK;
			invoice.US_FDAContactName = "BROKER NAME";
			invoice.US_FDAContactPhoneNo = "2345678";
			invoice.US_FDAContactEmail = "broker@abc.com";

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = product.OP_PartNum;
			AssertEquals(3, invoiceLine.NHTSALines.Count);

			var firstNHTSA = invoiceLine.NHTSALines[0];
			AssertEquals(PartyTypeList.Codes.CustomsBroker, firstNHTSA.US_CertifyingIndividual);
			AssertEquals("BROKER NAME", firstNHTSA.US_PGAContactName);
			AssertEquals("2345678", firstNHTSA.US_PGAContactPhoneNo);
			AssertEquals("broker@abc.com", firstNHTSA.US_PGAContactEmail);

			var secondNHTSA = invoiceLine.NHTSALines[1];
			AssertEquals(PartyTypeList.Codes.Importer, secondNHTSA.US_CertifyingIndividual);
			AssertEquals("IMPORTER TEST", secondNHTSA.US_PGAContactName);
			AssertEquals("1234567", secondNHTSA.US_PGAContactPhoneNo);
			AssertEquals("importer@abc.com", secondNHTSA.US_PGAContactEmail);

			var thirdNHTSA = invoiceLine.NHTSALines[2];
			AssertEquals(PartyTypeList.Codes.Owner, thirdNHTSA.US_CertifyingIndividual);
			AssertEquals(ZString.Empty, thirdNHTSA.US_PGAContactName);
			AssertEquals(ZString.Empty, thirdNHTSA.US_PGAContactPhoneNo);
			AssertEquals(ZString.Empty, thirdNHTSA.US_PGAContactEmail);

			var ownerOrgHeader = Factory.New<OrgHeader>();
			ownerOrgHeader.OH_Code = "TESTOWNER";
			DeclarationTestHelper.AddPGAContact(ownerOrgHeader, "OWNER", "TEST", "2222222", "owner@abc.com", null);

			thirdNHTSA.US_OA_NHTOwner = ownerOrgHeader.MainAddress.PK;
			AssertEquals("OWNER TEST", thirdNHTSA.US_PGAContactName);
			AssertEquals("2222222", thirdNHTSA.US_PGAContactPhoneNo);
			AssertEquals("owner@abc.com", thirdNHTSA.US_PGAContactEmail);
		}

		public void TestDefaultDisclaimReason()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Description = "TEST";
			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Disclaimed;
			AssertEquals(PGADisclaimReasonList.Codes.B, invoiceLine.US_AMSDisclaimReason);
			invoiceLine.US_AMSInd = ZString.Empty;
			AssertEquals(ZString.Empty, invoiceLine.US_AMSDisclaimReason);
			invoiceLine.US_NOPInd = OGAIndicatorList.Codes.Disclaimed;
			AssertEquals(PGADisclaimReasonList.Codes.A, invoiceLine.US_NOPDisclaimReason);
			invoiceLine.US_NOPInd = ZString.Empty;
			AssertEquals(ZString.Empty, invoiceLine.US_NOPDisclaimReason);
		}

		[TestDate(2023, 01, 02)]
		public void TestDefaultDiclaimReasonForEG1()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0123456789";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = new ZDateTime(2023, 12, 31);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariffView = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, tariff.UE_Tariff, new ZDateTime(2022, 01, 01), new ZDateTime(2024, 01, 01));
			var tariffAttribute = helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, "EG1,PN1", tariffView);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2023, 01, 10);
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals("IsCBMA23Effective", true, invoiceLine.IsCBMA23Effective);
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Disclaimed;
			AssertEquals("US_AMSDisclaimReason for EG1 should be 'A'", PGADisclaimReasonList.Codes.A, invoiceLine.US_AMSDisclaimReason);
			AssertEquals("US_AMSDisclaimProgram", AMSProgramList.Codes.EG1, invoiceLine.US_AMSDisclaimProgram);
		}

		public void TestPGAIndicator()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			Dictionary<string, string> indicatorAndDisclaimReasonMap = new Dictionary<string, string>();
			indicatorAndDisclaimReasonMap["US_VNEInd"] = "US_VNEDisclaimReason";
			indicatorAndDisclaimReasonMap["US_NHTSAIndicator"] = "US_NHTDisclaimReason";
			indicatorAndDisclaimReasonMap["US_NMFS370Ind"] = "US_NMFS370DisclaimReason";
			indicatorAndDisclaimReasonMap["US_NMFSAMRInd"] = "US_NMFSAMRDisclaimReason";
			indicatorAndDisclaimReasonMap["US_NMFSHMSInd"] = "US_NMFSHMSDisclaimReason";
			indicatorAndDisclaimReasonMap["US_ODSInd"] = "US_ODSDisclaimReason";
			indicatorAndDisclaimReasonMap["US_OMCInd"] = "US_OMCDisclaimReason";
			indicatorAndDisclaimReasonMap["US_APHISInd"] = "US_APHISDisclaimReason";
			indicatorAndDisclaimReasonMap["US_TTBInd"] = "US_TTBDisclaimReason";
			indicatorAndDisclaimReasonMap["US_FSISInd"] = "US_FSISDisclaimReason";
			indicatorAndDisclaimReasonMap["US_PSTIndicator"] = "US_PSTDisclaimReason";
			indicatorAndDisclaimReasonMap["US_LaceyIndicator"] = "US_LaceyDisclaimReason";
			indicatorAndDisclaimReasonMap["US_TSCAInd"] = "US_TSCADisclaimReason";
			indicatorAndDisclaimReasonMap["US_FWSInd"] = "US_FWSDisclaimReason";
			indicatorAndDisclaimReasonMap["US_AMSInd"] = "US_AMSDisclaimReason";
			indicatorAndDisclaimReasonMap["US_NOPInd"] = "US_NOPDisclaimReason";
			indicatorAndDisclaimReasonMap["US_CPSCInd"] = "US_CPSCDisclaimReason";
			indicatorAndDisclaimReasonMap["US_DEAInd"] = "US_DEADisclaimReason";
			ZString disclaimReasonValue = "A";
			foreach (var indicatorAndDisclaimReason in indicatorAndDisclaimReasonMap)
			{
				var indicatorPropertyInfo = invoiceLine.FindPropertyInfo(indicatorAndDisclaimReason.Key);
				var disclaimReasonPropertyInfo = invoiceLine.FindPropertyInfo(indicatorAndDisclaimReason.Value);
				indicatorPropertyInfo.Value = (ZString)OGAIndicatorList.Codes.Disclaimed;
				disclaimReasonPropertyInfo.Value = disclaimReasonValue;

				AssertEquals(disclaimReasonValue, disclaimReasonPropertyInfo.Value);
				indicatorPropertyInfo.Value = (ZString)OGAIndicatorList.Codes.Declared;
				AssertEquals(ZString.Empty, disclaimReasonPropertyInfo.Value);
			}
		}

		public void TestExportPGAIndicators()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.OnExportPGAIndicatorChangedEvent += (ZString agencyCode, ZBool hasExportPGAData) =>
			{
				return true;
			};

			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_ExportCertificateNo = "111111";
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_AMSInd);
			AssertEquals("111111", invoiceLine.US_ExportCertificateNo);
			invoiceLine.US_AMSInd = ZString.Empty;
			AssertEquals(ZString.Empty, invoiceLine.US_AMSInd);
			AssertEquals(ZString.Empty, invoiceLine.US_ExportCertificateNo);

			invoiceLine.US_TTBInd = OGAIndicatorList.Codes.Declared;
			var ttbLine = invoiceLine.TTBLines.AddNew();
			ttbLine.US_NumberForIRC = "11111";
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_TTBInd);
			AssertEquals(1, invoiceLine.TTBLines.Count);
			AssertEquals("11111", invoiceLine.TTBLines[0].US_NumberForIRC);
			invoiceLine.US_TTBInd = ZString.Empty;
			AssertEquals(ZString.Empty, invoiceLine.US_TTBInd);
			AssertEquals(0, invoiceLine.TTBLines.Count);

			invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.ExportFWS.US_ConfirmationNum = "11111";
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_FWSInd);
			AssertEquals("11111", invoiceLine.ExportFWS.US_ConfirmationNum);
			invoiceLine.US_FWSInd = ZString.Empty;
			AssertEquals(ZString.Empty, invoiceLine.US_FWSInd);
			AssertEquals(ZString.Empty, invoiceLine.ExportFWS.US_ConfirmationNum);

			invoiceLine.US_ATFInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.ExportATF.US_Quantity = 100m;
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_ATFInd);
			AssertEquals(100m, invoiceLine.ExportATF.US_Quantity);
			invoiceLine.US_ATFInd = ZString.Empty;
			AssertEquals(ZString.Empty, invoiceLine.US_ATFInd);
			AssertEquals(0m, invoiceLine.ExportATF.US_Quantity);

			invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_EPAConsentNumber = "11111";
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_PSTIndicator);
			AssertEquals("11111", invoiceLine.US_EPAConsentNumber);
			invoiceLine.US_PSTIndicator = ZString.Empty;
			AssertEquals(ZString.Empty, invoiceLine.US_PSTIndicator);
			AssertEquals(ZString.Empty, invoiceLine.US_EPAConsentNumber);

			invoiceLine.US_NMFSHMSInd = OGAIndicatorList.Codes.Declared;
			var nmfsHMS = invoiceLine.NMFSLines.AddNew();
			nmfsHMS.US_ReExportNumber = "11111";
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_NMFSHMSInd);
			AssertEquals(1, invoiceLine.NMFSLines.Count);
			AssertEquals("11111", invoiceLine.NMFSLines[0].US_ReExportNumber);
			invoiceLine.US_NMFSHMSInd = ZString.Empty;
			AssertEquals(ZString.Empty, invoiceLine.US_NMFSHMSInd);
			AssertEquals(0, invoiceLine.NMFSLines.Count);

			invoiceLine.OnExportPGAIndicatorChangedEvent = null;
			invoiceLine.OnExportPGAIndicatorChangedEvent += (ZString agencyCode, ZBool hasExportPGAData) =>
			{
				return false;
			};

			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_ExportCertificateNo = "111111";
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_AMSInd);
			AssertEquals("111111", invoiceLine.US_ExportCertificateNo);
			invoiceLine.US_AMSInd = ZString.Empty;
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_AMSInd);
			AssertEquals("111111", invoiceLine.US_ExportCertificateNo);

			invoiceLine.US_TTBInd = OGAIndicatorList.Codes.Declared;
			ttbLine = invoiceLine.TTBLines.AddNew();
			ttbLine.US_NumberForIRC = "11111";
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_TTBInd);
			AssertEquals(1, invoiceLine.TTBLines.Count);
			AssertEquals("11111", invoiceLine.TTBLines[0].US_NumberForIRC);
			invoiceLine.US_TTBInd = ZString.Empty;
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_TTBInd);
			AssertEquals(1, invoiceLine.TTBLines.Count);
			AssertEquals("11111", invoiceLine.TTBLines[0].US_NumberForIRC);

			invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.ExportFWS.US_ConfirmationNum = "11111";
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_FWSInd);
			AssertEquals("11111", invoiceLine.ExportFWS.US_ConfirmationNum);
			invoiceLine.US_FWSInd = ZString.Empty;
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_FWSInd);
			AssertEquals("11111", invoiceLine.ExportFWS.US_ConfirmationNum);

			invoiceLine.US_ATFInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.ExportATF.US_Quantity = 100m;
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_ATFInd);
			AssertEquals(100m, invoiceLine.ExportATF.US_Quantity);
			invoiceLine.US_ATFInd = ZString.Empty;
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_ATFInd);
			AssertEquals(100m, invoiceLine.ExportATF.US_Quantity);

			invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_EPAConsentNumber = "11111";
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_PSTIndicator);
			AssertEquals("11111", invoiceLine.US_EPAConsentNumber);
			invoiceLine.US_PSTIndicator = ZString.Empty;
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_PSTIndicator);
			AssertEquals("11111", invoiceLine.US_EPAConsentNumber);

			invoiceLine.US_NMFSHMSInd = OGAIndicatorList.Codes.Declared;
			nmfsHMS = invoiceLine.NMFSLines.AddNew();
			nmfsHMS.US_ReExportNumber = "11111";
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_NMFSHMSInd);
			AssertEquals(1, invoiceLine.NMFSLines.Count);
			AssertEquals("11111", invoiceLine.NMFSLines[0].US_ReExportNumber);
			invoiceLine.US_NMFSHMSInd = ZString.Empty;
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_NMFSHMSInd);
			AssertEquals(1, invoiceLine.NMFSLines.Count);
			AssertEquals("11111", invoiceLine.NMFSLines[0].US_ReExportNumber);
		}

		public void TestCopyAPHISFromProduct()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "APHIMP";
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "APHISTST";

			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = importer.PK;
			relOrg.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = USCTariff.CottonFeeApplicable;
			pivot.CD_APHISIndicator = OGAIndicatorList.Codes.Declared;

			var productAPHIS = pivot.APHISHeaders.AddNew();
			productAPHIS.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			productAPHIS.US_CategoryType = APHISCategoryTypeCodeList.Codes.LiveAnimals;

			var productInspection1 = productAPHIS.Inspections.AddNew();
			productInspection1.US_TestingStatus = InspectionStatusList.Codes.PreviouslyPerformed;
			var productInspection2 = productAPHIS.Inspections.AddNew();
			productInspection2.US_TestingStatus = InspectionStatusList.Codes.BTAAnticipatedArrivalInformation;

			var productSource = productAPHIS.Sources.AddNew();
			productSource.US_SourceTypeCode = SourceTypeCodesList.Codes.CountryOfSpeciesOrigin;
			productSource.US_CountryCode = Core.Constants.CountryCodes.Argentina;

			var productRouting = productAPHIS.Routings.AddNew();
			productRouting.US_Type = RoutingTypeList.Codes.PlaceOfTransshipment;
			productRouting.US_Country = Core.Constants.CountryCodes.UnitedStates;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "APHISTST";
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_APHISInd);
			AssertEquals(1, invoiceLine.APHISHeaders.Count);

			var invoiceLineAPHIS = invoiceLine.APHISHeaders[0];
			AssertEquals(APHISProgramCodeList.Codes.AVS, invoiceLineAPHIS.US_ProgramType);
			AssertEquals(APHISCategoryTypeCodeList.Codes.LiveAnimals, invoiceLineAPHIS.US_CategoryType);
			AssertEquals(2, invoiceLineAPHIS.Inspections.Count);
			AssertEquals(1, invoiceLineAPHIS.Sources.Count);
			AssertEquals(2, invoiceLineAPHIS.Routings.Count);

			var invoiceLineInspection1 = invoiceLineAPHIS.Inspections[0];
			AssertEquals(InspectionStatusList.Codes.PreviouslyPerformed, invoiceLineInspection1.US_TestingStatus);
			var invoiceLineInspection2 = invoiceLineAPHIS.Inspections[1];
			AssertEquals(InspectionStatusList.Codes.BTAAnticipatedArrivalInformation, invoiceLineInspection2.US_TestingStatus);

			var invoiceLineSource = invoiceLineAPHIS.Sources[0];
			AssertEquals(SourceTypeCodesList.Codes.CountryOfSpeciesOrigin, invoiceLineSource.US_SourceTypeCode);
			AssertEquals(Core.Constants.CountryCodes.Argentina, invoiceLineSource.US_CountryCode);

			var invoiceLineRouting1 = invoiceLineAPHIS.Routings[0];
			AssertEquals(RoutingTypeList.Codes.OriginalLocation, invoiceLineRouting1.US_Type);

			var invoiceLineRouting2 = invoiceLineAPHIS.Routings[1];
			AssertEquals(RoutingTypeList.Codes.PlaceOfTransshipment, invoiceLineRouting2.US_Type);
			AssertEquals(Core.Constants.CountryCodes.UnitedStates, invoiceLineRouting2.US_Country);
		}

		public void TestCopyLastFDAAndPGADetailsToNewLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			declaration.CopyLastFDADetailsToNewLine = true;
			declaration.CopyLastPGADetailsToNewLine = true;

			AssertEquals(true, invoiceLine.CopyLastFDADetailsToNewLine);
			AssertEquals(true, invoiceLine.CopyLastPGADetailsToNewLine);

			using (DataImportIndicatorService.StartDataImport(Factory))
			{
				AssertEquals(false, invoiceLine.CopyLastFDADetailsToNewLine);
				AssertEquals(false, invoiceLine.CopyLastPGADetailsToNewLine);
			}

			AssertEquals(true, invoiceLine.CopyLastFDADetailsToNewLine);
			AssertEquals(true, invoiceLine.CopyLastPGADetailsToNewLine);
		}

		public void TestIsDCSRequired()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.US_LicenseType = USAESLicenseCode.Codes.C30;
			invoiceLine.US_ECCN = "9A610";
			Assert(invoiceLine.IsDCSRequired);
			invoiceLine.US_ECCN = "9A515";
			Assert(invoiceLine.IsDCSRequired);
			invoiceLine.US_LicenseType = USAESLicenseCode.Codes.C43;
			Assert(!invoiceLine.IsDCSRequired);
			invoiceLine.US_LicenseType = USAESLicenseCode.Codes.C30;
			invoiceLine.US_ECCN = "9A410";
			Assert(invoiceLine.IsDCSRequired);
			invoiceLine.US_LicenseType = USAESLicenseCode.Codes.C30;
			invoiceLine.US_ECCN = "EAR99";
			Assert(!invoiceLine.IsDCSRequired);
		}

		public void TestDefaultDDTCArrivalDate()
		{
			var newInvoiceLine = InvoiceHeader.InvoiceLines.AddNew();
			AssertEquals(ZDateTime.Empty, newInvoiceLine.US_DDTCArrivalDate);

			var dateToSet = new ZDateTime(2016, 11, 09, 12, 00, 00);
			Declaration.US_FDAADTA = dateToSet;
			newInvoiceLine = InvoiceHeader.InvoiceLines.AddNew();
			newInvoiceLine.US_DDTCInd = "D";
			AssertEquals(dateToSet, newInvoiceLine.US_DDTCArrivalDate);
		}

		public void TestDefaultFDAWhenSetUS_FDAIndicator()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "AXG";
			staff.GS_FullName = "Amy Xiang";
			staff.GS_WorkPhone = "+86123456789";
			staff.GS_EmailAddress = "amy@wisetech.com";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryDate = new ZDateTime(2017, 03, 21);
			declaration.JE_GS_NKCusAgent = staff.GS_Code;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;
			tariff.UE_OGACodes = "FD1";

			invoiceLine.JI_Tariff = tariff.UE_Tariff;

			invoiceLine.US_FDAIndicator = "D";

			AssertEquals(staff.GS_FullName, declaration.US_FDAContactName);
			AssertEquals("123456789", declaration.US_FDAContactPhoneNo);
			AssertEquals(staff.GS_EmailAddress, declaration.US_FDAContactEmail);
			AssertEquals(declaration.US_EntryDate, declaration.US_FDAADTA);
		}

		public void TestHasFDAAdmissibilityReviewRequirement()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;
			tariff.UE_PGACodes = "FD4";

			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			AssertEquals(true, invoiceLine.HasFDAAdmissibilityReviewRequirement);
		}

		public void TestUpdateReconOriginLineNo()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var reconOriginalEntry1 = reconDeclaration.OriginalEntries.AddNew();
			reconOriginalEntry1.CH_OrigEntryReference = "XJ571032104";
			var reconOriginalEntry2 = reconDeclaration.OriginalEntries.AddNew();
			reconOriginalEntry2.CH_OrigEntryReference = "XJ571032105";
			var reconInvoiceLine1 = reconOriginalEntry1.Invoice.InvoiceLines.AddNew();
			var reconInvoiceLine2 = reconOriginalEntry1.Invoice.InvoiceLines.AddNew();
			var reconInvoiceLine3 = reconOriginalEntry1.Invoice.InvoiceLines.AddNew();

			AssertEquals((ZShort)1, reconInvoiceLine1.JI_LineNo);
			AssertEquals((ZShort)2, reconInvoiceLine2.JI_LineNo);
			AssertEquals((ZShort)3, reconInvoiceLine3.JI_LineNo);
			AssertEquals("1", reconInvoiceLine1.US_R_OrigEntryLineNo);
			AssertEquals("2", reconInvoiceLine2.US_R_OrigEntryLineNo);
			AssertEquals("3", reconInvoiceLine3.US_R_OrigEntryLineNo);

			reconInvoiceLine2.Delete();
			AssertEquals((ZShort)2, reconInvoiceLine3.JI_LineNo);
			AssertEquals("3", reconInvoiceLine3.US_R_OrigEntryLineNo);

			var reconInvoiceLine4 = reconOriginalEntry1.Invoice.InvoiceLines.AddNew();
			AssertEquals((ZShort)3, reconInvoiceLine4.JI_LineNo);
			AssertEquals(ZString.Empty, reconInvoiceLine4.US_R_OrigEntryLineNo);

			var reconInvoiceLine5 = reconOriginalEntry1.Invoice.InvoiceLines.AddNew();
			AssertEquals((ZShort)4, reconInvoiceLine5.JI_LineNo);
			AssertEquals("4", reconInvoiceLine5.US_R_OrigEntryLineNo);

			reconInvoiceLine3.Delete();
			AssertEquals((ZShort)2, reconInvoiceLine4.JI_LineNo);
			AssertEquals("2", reconInvoiceLine4.US_R_OrigEntryLineNo);

			AssertEquals((ZShort)3, reconInvoiceLine5.JI_LineNo);
			AssertEquals("4", reconInvoiceLine5.US_R_OrigEntryLineNo);

			var reconInvoiceLine6 = reconOriginalEntry2.Invoice.InvoiceLines.AddNew();
			AssertEquals((ZShort)1, reconInvoiceLine6.JI_LineNo);
			AssertEquals("1", reconInvoiceLine6.US_R_OrigEntryLineNo);
		}

		public void TestFWSIndDefaultingWithPFUNC()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;
			tariff.UE_PGACodes = "FW2";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_TariffType = TariffTypeList.Codes.HTS;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0000000000";

			AssertEquals("", invoiceLine.US_FWSInd);

			invoiceLine.JI_Tariff = "";

			using (ZZCustomsFunctionality.TemporarilySetupFWSEffective())
			{
				invoiceLine.JI_Tariff = "0000000000";
				AssertEquals("D", invoiceLine.US_FWSInd);
			}
		}

		public void TestFWSUS_InvCurrPGAValueDefaultingFromJI_LinePrice()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			invoiceLine2.JI_LinePrice = 100m;
			invoiceLine2.US_FWSInd = "D";

			var fwsHeader2 = invoiceLine2.FWSHeaders.AddNew();

			AssertEquals("PGAValue is defaulting with the line price", 100m, fwsHeader2.US_InvCurrPGAValue);

			invoiceLine1.JI_LinePrice = 200m;
			AssertEquals("PGA value is updated from line price because there is only one FWS Header", 300m, fwsHeader2.US_InvCurrPGAValue);

			invoiceLine2.JI_LinePrice = 200m;
			AssertEquals("PGA value is updated from line price because there is only one FWS Header", 400m, fwsHeader2.US_InvCurrPGAValue);

			fwsHeader2.US_InvCurrPGAValue = 100m;
			invoiceLine2.JI_LinePrice = 500m;
			AssertEquals("PGA value is not updated from line price because original value is not matching", 100m, fwsHeader2.US_InvCurrPGAValue);

			var fwsHeader1 = invoiceLine1.FWSHeaders.AddNew();
			fwsHeader1.US_InvCurrPGAValue = 600m;

			invoiceLine1.JI_LinePrice = 500m;
			AssertEquals("PGA value is not updated because there are multiple FWS Headers", 100m, fwsHeader2.US_InvCurrPGAValue);
			AssertEquals("PGA value is not updated because there are multiple FWS Headers", 600m, fwsHeader1.US_InvCurrPGAValue);
		}

		public void TestCopyExportPGAsFromProduct()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "INCORGTST";
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "INCEXPPGA";
			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = orgHeader.PK;
			relOrg.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot.CI_TariffNum = USCTariff.CottonFeeApplicable;
			pivot.CD_AMSIndicator = OGAIndicatorList.Codes.Declared;
			pivot.CD_ExportCertificateNo = "14244441321";
			pivot.CD_ATFIndicator = OGAIndicatorList.Codes.Declared;
			pivot.ExportATF.US_Quantity = 123m;
			pivot.ExportATF.US_CategoryCode = ATFCategoryCodeList.Codes.ABL;
			pivot.ExportATF.US_FFLNumber = "TEST NUMBER";
			pivot.ExportATF.US_FFLExemptionCode = ExemptionCodesCodeList.Codes._1;
			pivot.ExportATF.US_PermitNumber = "1234567";
			pivot.ExportATF.US_PermitExemptionCode = ExemptionCodesCodeList.Codes._2;
			pivot.CD_DEAIndicator = OGAIndicatorList.Codes.Declared;
			var productDEA = pivot.DEAHeaders.AddNew();
			productDEA.US_DrugCode = "ABCD";
			productDEA.US_Weight = 100m;
			productDEA.US_UnitOfMeasure = Core.Constants.Weight.Kilograms;
			productDEA.US_PermitNumber = "1233213";
			productDEA.US_RegistrationNumber = "888888888";
			pivot.CD_PSTIndicator = OGAIndicatorList.Codes.Declared;
			pivot.CD_EPAConsentNumber = "123132131";
			pivot.CD_HazWasteTrackingNo = "123456789ABC";
			pivot.CD_EPANetQty = 200m;
			pivot.CD_EPANetQtyUQ = Core.Constants.Weight.Kilograms;
			pivot.CD_FWSIndicator = OGAIndicatorList.Codes.Declared;
			pivot.ExportFWS.US_ConfirmationNum = "123156GF123";
			pivot.ExportFWS.US_TaxonomicSerialNumber = "SN18383801234567890";
			pivot.ExportFWS.US_PurposeCode = FWSPurposeCodeList.Codes.BiomedicalResearch;
			pivot.ExportFWS.US_WildlifeDescriptionCode = FWSWildlifeDescriptionCodesList.Codes.BAL;
			pivot.ExportFWS.US_WildlifeSource = FWSWildlifeSourceList.Codes.W;
			pivot.ExportFWS.US_WildlifeCategoryCode = FWSWildlifeCategoryCodesList.Codes.Arachnids;
			pivot.ExportFWS.US_CertificationCode = FWSCertificationCodeList.Codes.CertificationOfNoWildlife;
			pivot.ExportFWS.US_SpeciesOrigin = Core.Constants.CountryCodes.UnitedStates;
			pivot.ExportFWS.US_USState = USStateList.Codes.Alabama;
			pivot.CD_NMFSHMSIndicator = OGAIndicatorList.Codes.Declared;
			var productNMFS = pivot.NMFSLines.AddNew();
			productNMFS.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
			productNMFS.US_ProcessingType = NMFSProductCategoryCodeList.Codes.Dressed;
			productNMFS.US_Quantity = 1000m;
			productNMFS.US_UnitOfMeasure = Core.Constants.Weight.Tonnes;
			productNMFS.US_VesselCountry = Core.Constants.CountryCodes.Australia;
			productNMFS.US_HarvestedCountry = "ZZ";
			productNMFS.US_GeographicLocation = "A";
			productNMFS.US_DocumentType = NMFSHMSDocumentIdentifierList.Codes.BluefinTunaCatchDocument;
			productNMFS.US_IFTPPermitNumber = "SE52103";
			productNMFS.US_DISDocumentID = "DIS23423";
			productNMFS.US_CatchDocument = "AAAAAA";
			productNMFS.US_ReExportNumber = "BBBB";
			pivot.CD_TTBIndicator = OGAIndicatorList.Codes.Disclaimed;
			var productTTB = pivot.TTBLines.AddNew();
			productTTB.US_NumberForIRC = "TP-OH-77777";
			productTTB.US_Date = new ZDateTime(2017, 1, 19);
			productTTB.US_SerialNumber = "09876543211";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = orgHeader.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.US_TariffType = TariffTypeList.Codes.HTS;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "INCEXPPGA";
			CombineAssertions(() =>
			{
				AssertEquals("invoiceLine.US_AMSInd", OGAIndicatorList.Codes.Declared, invoiceLine.US_AMSInd);
				AssertEquals("invoiceLine.US_ExportCertificateNo", "14244441321", invoiceLine.US_ExportCertificateNo);
				AssertEquals("invoiceLine.US_ATFInd", OGAIndicatorList.Codes.Declared, invoiceLine.US_ATFInd);
				AssertEquals("invoiceLine.ExportATF.US_Quantity is not visible on product", ZDecimal.Zero, invoiceLine.ExportATF.US_Quantity);
				AssertEquals("invoiceLine.ExportATF.US_CategoryCode", ATFCategoryCodeList.Codes.ABL, invoiceLine.ExportATF.US_CategoryCode);
				AssertEquals("invoiceLine.ExportATF.US_FFLNumber", "TEST NUMBER", invoiceLine.ExportATF.US_FFLNumber);
				AssertEquals("invoiceLine.ExportATF.US_FFLExemptionCode", ExemptionCodesCodeList.Codes._1, invoiceLine.ExportATF.US_FFLExemptionCode);
				AssertEquals("invoiceLine.ExportATF.US_PermitNumber", "1234567", invoiceLine.ExportATF.US_PermitNumber);
				AssertEquals("invoiceLine.ExportATF.US_PermitExemptionCode", ExemptionCodesCodeList.Codes._2, invoiceLine.ExportATF.US_PermitExemptionCode);
				AssertEquals("invoiceLine.US_PSTIndicator", OGAIndicatorList.Codes.Declared, invoiceLine.US_PSTIndicator);
				AssertEquals("invoiceLine.US_EPAConsentNumber", "123132131", invoiceLine.US_EPAConsentNumber);
				AssertEquals("invoiceLine.US_HazWasteTrackingNo", "123456789ABC", invoiceLine.US_HazWasteTrackingNo);
				AssertEquals("invoiceLine.US_EPANetQty is not visible on product", ZDecimal.Zero, invoiceLine.US_EPANetQty);
				AssertEquals("invoiceLine.US_EPANetQtyUQ is not visible on product", ZString.Empty, invoiceLine.US_EPANetQtyUQ);
				AssertEquals("invoiceLine.US_DEAInd", OGAIndicatorList.Codes.Declared, invoiceLine.US_DEAInd);
				AssertEquals("invoiceLine.DEAHeaders.Count", 1, invoiceLine.DEAHeaders.Count);
				var invoiceLineDEA = invoiceLine.DEAHeaders[0];
				AssertEquals("invoiceLineDEA.US_DrugCode", "ABCD", invoiceLineDEA.US_DrugCode);
				AssertEquals("invoiceLineDEA.US_Weight, do not copy", 0m, invoiceLineDEA.US_Weight);
				AssertEquals("invoiceLineDEA.US_UnitOfMeasure, do not copy", ZString.Empty, invoiceLineDEA.US_UnitOfMeasure);
				AssertEquals("invoiceLineDEA.US_PermitNumber, do not copy", ZString.Empty, invoiceLineDEA.US_PermitNumber);
				AssertEquals("invoiceLineDEA.US_RegistrationNumber", "888888888", invoiceLineDEA.US_RegistrationNumber);
				AssertEquals("invoiceLine.US_FWSInd", OGAIndicatorList.Codes.Declared, invoiceLine.US_FWSInd);
				AssertEquals("invoiceLine.ExportFWS.US_ConfirmationNum", "123156GF123", invoiceLine.ExportFWS.US_ConfirmationNum);
				AssertEquals("invoiceLine.ExportFWS.US_TaxonomicSerialNumber", "SN18383801234567890", invoiceLine.ExportFWS.US_TaxonomicSerialNumber);
				AssertEquals("invoiceLine.ExportFWS.US_PurposeCode", FWSPurposeCodeList.Codes.BiomedicalResearch, invoiceLine.ExportFWS.US_PurposeCode);
				AssertEquals("invoiceLine.ExportFWS.US_WildlifeDescriptionCode", FWSWildlifeDescriptionCodesList.Codes.BAL, invoiceLine.ExportFWS.US_WildlifeDescriptionCode);
				AssertEquals("invoiceLine.ExportFWS.US_WildlifeSource", FWSWildlifeSourceList.Codes.W, invoiceLine.ExportFWS.US_WildlifeSource);
				AssertEquals("invoiceLine.ExportFWS.US_WildlifeCategoryCode", FWSWildlifeCategoryCodesList.Codes.Arachnids, invoiceLine.ExportFWS.US_WildlifeCategoryCode);
				AssertEquals("invoiceLine.ExportFWS.US_CertificationCode", FWSCertificationCodeList.Codes.CertificationOfNoWildlife, invoiceLine.ExportFWS.US_CertificationCode);
				AssertEquals("invoiceLine.ExportFWS.US_SpeciesOrigin", Core.Constants.CountryCodes.UnitedStates, invoiceLine.ExportFWS.US_SpeciesOrigin);
				AssertEquals("pivot.ExportFWS.US_USState", USStateList.Codes.Alabama, pivot.ExportFWS.US_USState);
				AssertEquals("invoiceLine.US_NMFSHMSInd", OGAIndicatorList.Codes.Declared, invoiceLine.US_NMFSHMSInd);
				AssertEquals("invoiceLine.NMFSLines.Count", 1, invoiceLine.NMFSLines.Count);
				var invoiceLineNMFS = invoiceLine.NMFSLines[0];
				AssertEquals("invoiceLineNMFS.US_ProgramType", NMFSProgramCodeList.Codes.HMS, invoiceLineNMFS.US_ProgramType);
				AssertEquals("invoiceLineNMFS.US_ProcessingType", NMFSProductCategoryCodeList.Codes.Dressed, invoiceLineNMFS.US_ProcessingType);
				AssertEquals("invoiceLineNMFS.US_Quantity is not visible on product", ZDecimal.Zero, invoiceLineNMFS.US_Quantity);
				AssertEquals("invoiceLineNMFS.US_UnitOfMeasure is not visible on product", ZString.Empty, invoiceLineNMFS.US_UnitOfMeasure);
				AssertEquals("invoiceLineNMFS.US_VesselCountry", Core.Constants.CountryCodes.Australia, invoiceLineNMFS.US_VesselCountry);
				AssertEquals("invoiceLineNMFS.US_HarvestedCountry", "ZZ", invoiceLineNMFS.US_HarvestedCountry);
				AssertEquals("invoiceLineNMFS.US_GeographicLocation", "A", invoiceLineNMFS.US_GeographicLocation);
				AssertEquals("invoiceLineNMFS.US_DocumentType", NMFSHMSDocumentIdentifierList.Codes.BluefinTunaCatchDocument, invoiceLineNMFS.US_DocumentType);
				AssertEquals("invoiceLineNMFS.US_IFTPPermitNumber", "SE52103", invoiceLineNMFS.US_IFTPPermitNumber);
				AssertEquals("invoiceLineNMFS.US_DISDocumentID, do not copy", ZString.Empty, invoiceLineNMFS.US_DISDocumentID);
				AssertEquals("invoiceLineNMFS.US_CatchDocument", "AAAAAA", invoiceLineNMFS.US_CatchDocument);
				AssertEquals("invoiceLineNMFS.US_ReExportNumber", "BBBB", invoiceLineNMFS.US_ReExportNumber);
				AssertEquals("invoiceLine.US_TTBInd", OGAIndicatorList.Codes.Disclaimed, invoiceLine.US_TTBInd);
				AssertEquals("invoiceLine.TTBLines.Count", 1, invoiceLine.TTBLines.Count);
				var invoiceLineTTB = invoiceLine.TTBLines[0];
				AssertEquals("invoiceLineTTB.US_NumberForIRC", "TP-OH-77777", invoiceLineTTB.US_NumberForIRC);
				AssertEquals("invoiceLineTTB.US_Date", new ZDateTime(2017, 1, 19), invoiceLineTTB.US_Date);
				AssertEquals("invoiceLineTTB.US_SerialNumber", "09876543211", invoiceLineTTB.US_SerialNumber);
			});
		}

		public void TestIDutyDataDataMembersForChapter98()
		{
			var testHelper = new Chapter98HelperTest();
			testHelper.ParentLine.US_SupTariff = testHelper.Test99038801Tariff.UE_Tariff;
			testHelper.ChildLine.US_SupTariff = testHelper.Test9802005060Tariff.UE_Tariff;
			testHelper.Charpter98Job.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var idutyParent = testHelper.ParentLine as IDutyData;
			Assert(!idutyParent.IsCombineSecondaryTariffLine);
			Assert(idutyParent.SupTariffs.Contains(testHelper.Test99038801Tariff.UE_Tariff));
			Assert(testHelper.ParentLine.IsCombineParentTariffLine);

			var idutyChildLine = testHelper.ChildLine as IDutyData;
			Assert(idutyChildLine.IsCombineSecondaryTariffLine);
			Assert(idutyChildLine.SupTariffs.Contains(testHelper.Test9802005060Tariff.UE_Tariff));
			Assert(!testHelper.ChildLine.IsCombineParentTariffLine);

			testHelper.ParentLine.US_SupTariff = testHelper.Test9802006000Tariff.UE_Tariff;
			testHelper.ChildLine.US_SupTariff = testHelper.Test9802005060Tariff.UE_Tariff;
			testHelper.Charpter98Job.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			idutyParent = testHelper.ParentLine;
			Assert(!idutyParent.IsCombineSecondaryTariffLine);
			Assert(idutyParent.SupTariffs.Contains(testHelper.Test9802006000Tariff.UE_Tariff));
			Assert(!testHelper.ParentLine.IsCombineParentTariffLine);

			idutyChildLine = testHelper.ChildLine;
			Assert(!idutyChildLine.IsCombineSecondaryTariffLine);
			Assert(idutyChildLine.SupTariffs.Contains(testHelper.Test9802005060Tariff.UE_Tariff));
			Assert(!testHelper.ChildLine.IsCombineParentTariffLine);
		}

		[TestDate(2019, 05, 10)]
		public void TestDefault99038809AgnistWithExportDate()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "HSN");
			Factory.Save();

			var progTariff1 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99038804", new ZDateTime(2018, 09, 24), new ZDateTime(2079, 06, 06));
			var progTariff2 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99038809", new ZDateTime(2019, 05, 10), new ZDateTime(2019, 05, 31));
			var dutyRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DTY", dutyRateType.PK);
			Factory.Save();
			var rate1 = helper.CreateRate(progTariff1, rateCode.PK, new ZDateTime(2018, 09, 24), new ZDateTime(2079, 06, 06), "0");
			var rate2 = helper.CreateRate(progTariff2, rateCode.PK, new ZDateTime(2019, 05, 10), new ZDateTime(2019, 05, 31), "0");
			var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.China, "CN", new ZDateTime(2000, 01, 01), new ZDateTime(2079, 06, 06));
			var tradeGroupCountry = helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.China, new ZDate(2000, 01, 01), new ZDate(2079, 06, 06));
			var applicability1 = helper.CreateCusApplicability(rate1, tradeGroup, new ZDateTime(2018, 09, 24), new ZDateTime(2079, 06, 06));
			var applicability2 = helper.CreateCusApplicability(rate2, tradeGroup, new ZDateTime(2019, 05, 10), new ZDateTime(2019, 05, 31));
			var relationship1 = helper.CreateTariffRelationship(progTariff1.PK, hsnTariffType.PK, "8517620010");
			var relationship2 = helper.CreateTariffRelationship(progTariff2.PK, hsnTariffType.PK, "8517620010");
			helper.CreateTariffAttribute("RULE", "A99", progTariff1);
			helper.CreateTariffAttribute("RULE", "A99", progTariff2);
			Factory.Save();

			var tariff8517620010 = Factory.New<USCTariff>();
			tariff8517620010.UE_Tariff = "8517620010";
			tariff8517620010.UE_DateFrom = new ZDateTime(2019, 01, 01);
			tariff8517620010.UE_DateTo = new ZDateTime(2079, 06, 06);
			var tariff99038804 = Factory.New<USCTariff>();
			tariff99038804.UE_Tariff = "99038804";
			tariff99038804.UE_DateFrom = new ZDateTime(2018, 09, 24);
			tariff99038804.UE_DateTo = new ZDateTime(2079, 06, 06);
			var tariff99038809 = Factory.New<USCTariff>();
			tariff99038809.UE_Tariff = "99038809";
			tariff99038809.UE_DateFrom = new ZDateTime(2019, 05, 10);
			tariff99038809.UE_DateTo = new ZDateTime(2019, 05, 31);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EstimatedEntryDate = new ZDateTime(2019, 05, 13);

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfOrigin = "CN";
			invoiceLine.JI_Tariff = "8517620010";
			AssertEquals(2, invoiceLine.ApplicableSupTariffList.Count);
			AssertEquals(ZString.Empty, invoiceLine.US_SupTariff);

			invoice.JobComInvoiceLines.RemoveAndDeleteAll();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_DateOfExport = new ZDateTime(2019, 05, 08);
			invoiceLine.US_UC_NKCountryOfOrigin = "CN";
			invoiceLine.JI_Tariff = "8517620010";
			AssertEquals(2, invoiceLine.ApplicableSupTariffList.Count);
			AssertEquals("99038809", invoiceLine.US_SupTariff);

			invoice.JobComInvoiceLines.RemoveAndDeleteAll();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_DateOfExport = new ZDateTime(2019, 05, 10);
			invoiceLine.US_UC_NKCountryOfOrigin = "CN";
			invoiceLine.JI_Tariff = "8517620010";
			AssertEquals(2, invoiceLine.ApplicableSupTariffList.Count);
			AssertEquals("99038804", invoiceLine.US_SupTariff);
		}

		public void TestDutyFormulaDescription()
		{
			var testHelper = new Chapter98HelperTest();
			testHelper.ParentLine.US_SupTariff = testHelper.Test99038501Tariff.UE_Tariff;//99038501
			testHelper.ChildLine.US_SupTariff = testHelper.Test9802005060Tariff.UE_Tariff;//9802005060
			testHelper.Charpter98Job.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNullOrEmpty(testHelper.ChildLine.DutyFormula);
			AssertEquals(JobComInvoiceLine.CompositeDutyCalculation, testHelper.ChildLine.DutyFormulaDescription);
		}

		public void TestDutyFormulaForCombinedLines()
		{
			var testHelper = new CombinedLinesHelperTest();
			var testJob = testHelper.CombinedJob;
			var invoiceLine1 = testHelper.InvoiceHeaderForCombined.InvoiceLines.AddNew();
			var invoiceLine2 = testHelper.InvoiceHeaderForCombined.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = testHelper.Chapter98TestingHelper.Test99038801Tariff.UE_Tariff;
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			invoiceLine2.US_SupTariff = testHelper.Chapter98TestingHelper.Test99038802Tariff.UE_Tariff;
			invoiceLine2.JI_Tariff = testHelper.Chapter98TestingHelper.TestCTariff.UE_Tariff;
			invoiceLine2.JI_LinePrice = 8000m;
			testJob.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			AssertEquals("40%", invoiceLine1.DutyFormula);
			AssertEquals("70% + 50%", invoiceLine2.DutyFormula);
			AssertEquals(JobComInvoiceLine.CompositeDutyCalculation, invoiceLine2.DutyFormulaDescription);
		}

		public void TestDutyFormulaForDerivedSets()
		{
			#region Setup Tariffs

			USCTariffTest.CreateNewTariffIfNotExist(Factory, "8206000000", "9", 1, "PCS");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "8203204000", "7", 0.12, "DOZ");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030124", "7", 0.2m, "");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99038823", "7", 0.25m, "");

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariffView99030124 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99030124", new ZDateTime(2020, 12, 29), new ZDateTime(2079, 06, 06), "ATRICLE OF CHINA", 1);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariffView99030124);
			var tariffView99038823 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99038823", new ZDateTime(2020, 12, 29), new ZDateTime(2079, 06, 06), "ATRICLE OF CHINA", 1);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariffView99038823);

			#endregion

			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV25031501";
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			invoice.JZ_IncoTerm = TermsOfDeliveryList.Codes.DDP;

			var parentInvoiceLine = invoice.JobComInvoiceLines.AddNew();
			parentInvoiceLine.JI_FormattedTariff = "8206.00.0000";
			parentInvoiceLine.SupTariffFormatted = "9903.01.24";
			parentInvoiceLine.SupFormattedAdditionalTariff1 = "9903.88.23";
			parentInvoiceLine.JI_LinePrice = 0m;
			var childInvoiceLine = invoice.JobComInvoiceLines.AddNew();
			childInvoiceLine.JI_ParentID = parentInvoiceLine.PK;
			childInvoiceLine.JI_FormattedTariff = "8203.20.4000";
			childInvoiceLine.JI_LinePrice = 5000m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("DutyFormula on parent invoice line", "12% + 20% + 25%", parentInvoiceLine.DutyFormula);
			AssertEquals("DutyFormula on child invoice line", "", childInvoiceLine.DutyFormula);
		}

		public void TestSupTariffFormattedFieldType()
		{
			SetDataForSupTariffTest();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLines = invoice.JobComInvoiceLines;
			var invoiceLine1 = invoiceLines.AddNew();
			AssertEquals("invoiceLine.SupTariffFormattedFieldType", nameof(FieldType.TextCodeFindBox), invoiceLine1.SupTariffFormattedFieldType);
			AssertEquals(0, invoiceLine1.ApplicableSupTariffList.Count);

			invoiceLine1.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.HongKong;
			invoiceLine1.JI_Tariff = "7301100000";
			AssertEquals("invoiceLine.SupTariffFormattedFieldType", nameof(FieldType.TextDropEdit), invoiceLine1.SupTariffFormattedFieldType);
			AssertEquals(1, invoiceLine1.ApplicableSupTariffList.Count);

			var invoiceLine2 = invoiceLines.AddNew();
			invoiceLine2.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.HongKong;
			invoiceLine2.JI_Tariff = "8450110010";
			AssertEquals("invoiceLine.SupTariffFormattedFieldType", nameof(FieldType.TextDropEdit), invoiceLine2.SupTariffFormattedFieldType);
			AssertEquals(2, invoiceLine2.ApplicableSupTariffList.Count);

			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			var invoiceLine3 = invoiceLines.AddNew();
			invoiceLine3.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.HongKong;
			AssertEquals("invoiceLine.SupTariffFormattedFieldType", nameof(FieldType.TextCodeFindBox), invoiceLine2.SupTariffFormattedFieldType);
			AssertEquals(2, invoiceLine2.ApplicableSupTariffList.Count);
		}

		public void TestDefaultUS_SupTariff()
		{
			SetDataForSupTariffTest();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLines = invoice.JobComInvoiceLines;
			var invoiceLine1 = invoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "7301100000";
			AssertEquals(ZString.Empty, invoiceLine1.US_SupTariff);

			invoiceLine1.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.HongKong;
			AssertEquals("US_SupTariff has been set", "99038001", invoiceLine1.US_SupTariff);
			AssertEquals(1, invoiceLine1.ApplicableSupTariffList.Count);

			var invoiceLine2 = invoiceLines.AddNew();
			invoiceLine2.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.HongKong;
			invoiceLine2.JI_Tariff = "8450110010";
			AssertEquals(ZString.Empty, invoiceLine2.US_SupTariff);
			AssertEquals(2, invoiceLine2.ApplicableSupTariffList.Count);

			invoiceLine2.JI_Tariff = "1234512345";
			AssertEquals(ZString.Empty, invoiceLine2.US_SupTariff);
			AssertEquals(0, invoiceLine2.ApplicableSupTariffList.Count);

			var invoiceLine3 = invoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "7301100000";
			invoiceLine3.US_ProductExclusion = "02";
			invoiceLine3.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.HongKong;
			invoiceLine3.JI_Tariff = "8450110010";
			AssertEquals("Should not default 990380 tariffs when Product Exclusion is entered", ZString.Empty, invoiceLine3.US_SupTariff);

			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			var invoiceLine4 = invoiceLines.AddNew();
			invoiceLine4.JI_Tariff = "7301100000";
			AssertEquals(ZString.Empty, invoiceLine4.US_SupTariff);

			invoiceLine4.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.HongKong;
			//US_SupTariff should be set for FTZ if it exists in ApplicableSupTariffList.
			AssertEquals("US_SupTariff should not be set for FTZ", ZString.Empty, invoiceLine4.US_SupTariff);
			AssertEquals(1, invoiceLine4.ApplicableSupTariffList.Count);

			invoiceLine4.JI_Tariff = "1234512345";
			//US_SupTariff should be set for FTZ if it exists in ApplicableSupTariffList.
			AssertEquals(ZString.Empty, invoiceLine4.US_SupTariff);
			AssertEquals(0, invoiceLine4.ApplicableSupTariffList.Count);
		}

		public void TestDefaultUS_ZoneStatusForFTZ()
		{
			SetDataForSupTariffTest();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "7301100000";
			AssertEquals("Zone Status", ZString.Empty, invoiceLine.US_ZoneStatus);

			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.HongKong;
			AssertEquals("Zone Status", ZoneStatusList.Codes.PrivilegedForeign, invoiceLine.US_ZoneStatus);

			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Brazil;
			AssertEquals("Zone Status", ZString.Empty, invoiceLine.US_ZoneStatus);

			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.HongKong;
			invoiceLine.JI_Tariff = "1234512345";
			AssertEquals("Zone Status", ZString.Empty, invoiceLine.US_ZoneStatus);
		}

		public void TestDefaultUS_LicenseTypeForFTZ()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FTZLicenseType, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FTZLicenseType);
			var codeList = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FTZLicenseType, "STL", "STL Description", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			var refCusCodePK = codeList.PK;
			helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodePK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.ImportPermitLicenceType, "01");

			codeList = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FTZLicenseType, "ALU", "ALU Description", new ZDateTime(1900, 1, 1), ZDateTime.Today.AddYears(-1));
			refCusCodePK = codeList.PK;
			helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodePK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.ImportPermitLicenceType, "06");

			codeList = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FTZLicenseType, "DIA", "DIA Description", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			refCusCodePK = codeList.PK;
			helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodePK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.ImportPermitLicenceType, "06");

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1234567890";
			tariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff.UE_PermitLicenseIndicator = "01";

			tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1234567899";
			tariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff.UE_PermitLicenseIndicator = "06";

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			AssertEquals("", invoiceLine1.US_LicenseType);
			AssertEquals("", invoiceLine2.US_LicenseType);

			invoiceLine1.JI_Tariff = "1234567890";
			AssertEquals("STL", invoiceLine1.US_LicenseType);
			invoiceLine1.US_SupTariff = "1234567899";
			AssertEquals("DIA", invoiceLine1.US_LicenseType);
			invoiceLine2.JI_Tariff = "1234567890";
			AssertEquals("STL", invoiceLine2.US_LicenseType);
		}

		public void TestDefaultUS_SupTariffForCAProvince()
		{
			SetDataForSupTariffTest();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			var invoiceLines = invoice.JobComInvoiceLines;
			var invoiceLine = invoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Canada;
			invoiceLine.US_SPI = SpecialProgramList.Codes.CA;
			invoiceLine.JI_Tariff = "7616995160";

			AssertEquals("US_SupTariff has been set", "99034105", invoiceLine.US_SupTariff);
			AssertEquals(1, invoiceLine.ApplicableSupTariffList.Count);

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals("JI_Calc_DutyAmount", 4000m, invoiceLine.JI_Calc_DutyAmount);
			AssertEquals("US_Duty", 0m, invoiceLine.US_Duty);
			AssertEquals("US_SupDuty", 4000m, invoiceLine.US_SupDuty);
		}

		public void TestSupTariffList()
		{
			SetDataForSupTariffTest();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLines = invoice.JobComInvoiceLines;
			var invoiceLine1 = invoiceLines.AddNew();
			invoiceLine1.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.HongKong;
			invoiceLine1.JI_Tariff = "7216100010";
			AssertEquals(1, invoiceLine1.ApplicableSupTariffList.Count);

			invoiceLine1.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.KoreaSouth;
			AssertEquals(2, invoiceLine1.ApplicableSupTariffList.Count);
		}

		[TestDate(2021, 09, 01)]
		public void TestAllApplicableRatesSelectionCriteria()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Australia;
			var criteria = invoiceLine.AllApplicableRatesSelectionCriteria;
			CombineAssertions("Criteria when C/O is not Canada", () =>
			{
				AssertEquals("criteria.TradeGroupCountry", Core.Constants.CountryCodes.Australia, criteria.TradeGroupCountry);
				AssertEquals("criteria.EffectiveDate", new ZDateTime(2021, 09, 01), criteria.EffectiveDate);
				AssertEquals("criteria.RateType", Universal.Constants.RateTypes.Duty, criteria.RateType);
				AssertEquals("criteria.RateCode", UniversalReferenceConstants.RateCodes.Codes.Duty, criteria.RateCode);
			});

			invoiceLine.US_UC_NKCountryOfOrigin = CanadaProvinceTerritoryCodes.Codes.XA;
			criteria = invoiceLine.AllApplicableRatesSelectionCriteria;
			CombineAssertions("Criteria when C/O is Canada", () =>
			{
				AssertEquals("criteria.TradeGroupCountry", Core.Constants.CountryCodes.Canada, criteria.TradeGroupCountry);
				AssertEquals("criteria.EffectiveDate", new ZDateTime(2021, 09, 01), criteria.EffectiveDate);
				AssertEquals("criteria.RateType", Universal.Constants.RateTypes.Duty, criteria.RateType);
				AssertEquals("criteria.RateCode", UniversalReferenceConstants.RateCodes.Codes.Duty, criteria.RateCode);
			});
		}

		public void TestUpdateDetailsOnPartChange_ADDCVDForA99()
		{
			SetDataForSupTariffTest();

			var product = GetProductForFDAAndPGATests();
			var pivot = product.PivotsForBinding[0];
			pivot.CD_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.HongKong;
			pivot.CD_ADDApplicable = true;
			pivot.CD_CVDApplicable = true;
			pivot.CD_ACEFDAIndicator = OGAIndicatorList.Codes.Declared;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_OH_Importer = importer.PK;

			var invoice = declaration.Invoices.AddNew();

			var invoiceLines = invoice.JobComInvoiceLines;
			var invoiceLine = invoiceLines.AddNew();
			invoiceLine.JI_PartNo = "NICE WATCH";

			AssertEquals("US_ADD_NA", true, invoiceLine.US_ADD_NA);
			AssertEquals("US_CVD_NA", true, invoiceLine.US_CVD_NA);
			AssertEquals("US_FDAIndicator", "D", invoiceLine.US_FDAIndicator);
		}

		public void TestResetDutyAndFeeAnalysisData()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.US_FTADuty = 1m;
			invoiceLine.US_FTAPayableMPF = 1m;
			invoiceLine.US_NonFTADuty = 1m;
			invoiceLine.US_NonFTAPayableMPF = 1m;
			invoiceLine.ResetDutyAndFeeAnalysisData();
			AssertEquals("invoiceLine.US_FTADuty", ZDecimal.Zero, invoiceLine.US_FTADuty);
			AssertEquals("invoiceLine.US_FTAPayableMPF", ZDecimal.Zero, invoiceLine.US_FTAPayableMPF);
			AssertEquals("invoiceLine.US_NonFTADuty", ZDecimal.Zero, invoiceLine.US_NonFTADuty);
			AssertEquals("invoiceLine.US_NonFTAPayableMPF", ZDecimal.Zero, invoiceLine.US_NonFTAPayableMPF);
		}

		public void TestManufacturerMID()
		{
			DataRegistry.Business.USCustomsDataRegistry.Instance.CreateMIDOrganizationOnUnmatchedImport.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);
			var org = Factory.New<OrgHeader>();
			org.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "USMIDCODE", "US");
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.ManufacturerMID = "USMIDCODE";

			AssertEquals(org.MainAddress.PK, invoiceLine.JI_OA_ManufacturerAddress);
			AssertEquals("USMIDCODE", invoiceLine.ManufacturerMID);

			invoiceLine.ManufacturerMID = "USMIDCODE1";
			var newCreatedOrg = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, "AUTO CREATED FROM MID"));
			AssertNotNull(newCreatedOrg);
			AssertEquals(newCreatedOrg.MainAddress.PK, invoiceLine.JI_OA_ManufacturerAddress);
			AssertEquals("USMIDCODE1", invoiceLine.ManufacturerMID);
		}

		public void TestKRExportSteelCertificateNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertNull(invoiceLine.KRExportSteelCertificateNumber);
			var steelNumber = invoiceLine.LicenceAndPermits.AddNew(LicencePermitTypeList.Codes.KR, "");
			AssertNull(invoiceLine.KRExportSteelCertificateNumber);
			steelNumber.CY_Data = "123456";
			AssertNotNull(invoiceLine.KRExportSteelCertificateNumber);
		}

		public void TestFTZ_ADDCVD_ZoneStatusPopulation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			AssertEquals("Pre: invoiceLine.US_ZoneStatus", ZString.Empty, invoiceLine.US_ZoneStatus);
			invoiceLine.US_ADDCaseNo = "ADD12345";
			AssertEquals("ADD: invoiceLine.US_ZoneStatus", ZoneStatusList.Codes.PrivilegedForeign, invoiceLine.US_ZoneStatus);
			invoiceLine.US_ADDCaseNo = ZString.Empty;
			invoiceLine.US_ZoneStatus = ZString.Empty;
			invoiceLine.US_CVDCaseNo = "CVD12345";
			AssertEquals("CVD: invoiceLine.US_ZoneStatus", ZoneStatusList.Codes.PrivilegedForeign, invoiceLine.US_ZoneStatus);
		}

		public override void TestMakeCustomsQuantityReadOnly()
		{
			InvoiceLine.JI_Tariff = "";
			AssertEquals("There is no tariff and CustomsQuantity should be readonly", true, InvoiceLine.JI_CustomsQuantityInfo.ReadOnly);

			InvoiceLine.JI_Tariff = "00000000 00";
			AssertEquals("Invalid tariff and there is no Customs UQ involved", true, InvoiceLine.JI_CustomsUnitQty.IsEmpty);

			InvoiceLine.JI_CustomsUnitQty = "NO";
			AssertEquals("Customs unit qty exists and Qty field should be open", false, InvoiceLine.JI_CustomsQuantityInfo.ReadOnly);
		}

		public override void TestCustomsQuantityIsReadonlyWhenUnitQtyEmpty()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceLine.JI_CustomsUnitQty = "";
			AssertEquals(true, InvoiceLine.JI_CustomsQuantityInfo.ReadOnly);

			InvoiceLine.JI_CustomsUnitQty = "KG";
			AssertEquals(false, InvoiceLine.JI_CustomsQuantityInfo.ReadOnly);
		}

		public void TestCopyValuesFromParentToChildForCombinedLinesWhenSettingProductCode()
		{
			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			importer.OH_RL_NKClosestPort = "USPHL";

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "2621433";
			var relatedOrg = product.RelatedOrganisations.AddNew();
			relatedOrg.OU_OH = importer.PK;
			relatedOrg.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_SupplementalTariff = "99038803";
			pivot.CD_ProductExclusion = "02";
			pivot.CD_ExclusionNumber = "STL000001";
			var component = product.PivotsForBinding.AddNew();
			component.CI_CI_Parent = pivot.PK;
			component.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			component.CI_TariffNum = "7323999080";
			component.CI_SupplementalTariff = "99021499";
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_InvoiceQuantity = 100m;
			invoiceLine.JI_InvoiceUQ = "PCS";
			invoiceLine.JI_Weight = 10m;
			invoiceLine.JI_WeightUQ = "KG";
			invoiceLine.JI_NetWeight = 9m;
			invoiceLine.JI_NetWeightUQ = "KG";
			invoiceLine.JI_Volume = 5m;
			invoiceLine.JI_VolumeUQ = "M3";
			invoiceLine.JI_PartNo = product.OP_PartNum;
			AssertEquals(2, invoice.JobComInvoiceLines.Count);
			var parentLine = invoice.JobComInvoiceLines[0];
			CombineAssertions("Parent Invoice Line", () =>
			{
				AssertEquals("Tariff from HTI pivot.", ZString.Empty, parentLine.JI_Tariff);
				AssertEquals("Sup tariff from HTI pivot", "99038803", parentLine.US_SupTariff);
				AssertEquals("Line price should be moved to child line.", 0m, parentLine.JI_LinePrice);
				AssertEquals("Invoice quantity should be moved to child line.", 0m, parentLine.JI_InvoiceQuantity);
				AssertEquals("Gross weight should be moved to child line.", 0m, parentLine.JI_Weight);
				AssertEquals("Net weight should be moved to child line.", 0m, parentLine.JI_NetWeight);
				AssertEquals("Volumn should be moved to child line.", 0m, parentLine.JI_Volume);
				AssertEquals("Product exclusion should be empty on parent line", ZString.Empty, parentLine.US_ProductExclusion);
				AssertEquals("Exclusion number should be empty on parent line.", ZString.Empty, parentLine.US_ExclusionNumber);
			});

			var childLine = invoice.JobComInvoiceLines[1];
			CombineAssertions("Child Invoice Line", () =>
			{
				AssertEquals("Tariff from COM pivot.", "7323999080", childLine.JI_Tariff);
				AssertEquals("Sup tariff from COM pivot", "99021499", childLine.US_SupTariff);
				AssertEquals("Line price should be moved from parent line.", 10000m, childLine.JI_LinePrice);
				AssertEquals("Invoice quantity should be moved from parent line.", 100m, childLine.JI_InvoiceQuantity);
				AssertEquals("Gross weight should be moved from parent line.", 10m, childLine.JI_Weight);
				AssertEquals("Net weight should be moved from parent line.", 9m, childLine.JI_NetWeight);
				AssertEquals("Volumn should be moved from parent line.", 5m, childLine.JI_Volume);
				AssertEquals("Product exclusion should have value from HTI pivot.", "02", childLine.US_ProductExclusion);
				AssertEquals("Exclusion number should have value from HTI pivot.", "STL000001", childLine.US_ExclusionNumber);
			});
		}

		[TestDate(2020, 03, 05)]
		public void TestIsEmbroideryChildTariffLine()
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

			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLineOne = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLineOne.JI_Tariff = "5810929080";
			AssertEquals(false, invoiceLineOne.IsEmbroideryChildTariffLine);

			var invoiceLineTwo = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLineTwo.JI_Tariff = "5407532060";
			AssertEquals(false, invoiceLineTwo.IsEmbroideryChildTariffLine);

			invoiceLineTwo.JI_ParentID = invoiceLineOne.PK;
			AssertEquals(true, invoiceLineTwo.IsEmbroideryChildTariffLine);
		}

		public void TestChildLineDescription_HasSecondaryTariff()
		{
			var tariffNumber0 = "1010111111";
			var tariffNumber1 = "1001101100";
			var tariffNumber2 = "1002202200";

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Test1";
			product.OP_Desc = "UPDATED";
			product.RelatedOrganisations.AddOwner(importer);

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = tariffNumber0;
			pivot.CD_CBTPACertificate = "A";

			var relatedTariffPivot1 = pivot.Children.AddNew();
			relatedTariffPivot1.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			relatedTariffPivot1.CI_TariffNum = tariffNumber1;
			relatedTariffPivot1.CD_CBTPACertificate = "1";
			relatedTariffPivot1.CI_Description = "UPDATED 1";

			var relatedTariffPivot2 = pivot.Children.AddNew();
			relatedTariffPivot2.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			relatedTariffPivot2.CI_TariffNum = tariffNumber2;
			relatedTariffPivot2.CD_CBTPACertificate = "2";
			relatedTariffPivot2.CI_Description = "UPDATED 2";

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			Factory.Save();

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "Test1";
			AssertEquals("Load Part", 3, invoice.InvoiceLines.Count);

			AssertEquals("UPDATED", "UPDATED", invoiceLine.JI_Description);
			AssertEquals("child 1: UPDATED", "UPDATED 1", invoice.InvoiceLines[1].JI_Description);
			AssertEquals("child 2: UPDATED", "UPDATED 2", invoice.InvoiceLines[2].JI_Description);
		}

		public void TestNoExceptionThrownWhenChildPivotTypeIsEmpty()
		{
			var tariffNumber0 = "1010111111";
			var tariffNumber1 = "1001101100";

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Test1";
			product.OP_Desc = "UPDATED";
			product.RelatedOrganisations.AddOwner(importer);

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = tariffNumber0;
			pivot.CI_Description = "PARENT";

			var relatedTariffPivot1 = pivot.Children.AddNew();
			relatedTariffPivot1.CI_ChildType = ZString.Empty;
			relatedTariffPivot1.CI_TariffNum = tariffNumber1;
			relatedTariffPivot1.CI_Description = "CHILD";

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = invoice.InvoiceLines.AddNew();
			AssertNoExceptionThrown(() => { invoiceLine.JI_PartNo = "Test1"; });
			AssertEquals("Load Part", 1, invoice.InvoiceLines.Count);
			AssertEquals("PARENT", invoiceLine.JI_Description);
		}

		public void TestClassificationDescriptionFallbackToProductClassification()
		{
			var tariffNumber0 = "1010111111";
			var tariffNumber1 = "1001101100";
			var tariffNumber2 = "1002202200";

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Test1";
			product.OP_Desc = "UPDATED";
			product.RelatedOrganisations.AddOwner(importer);

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = tariffNumber0;
			pivot.CD_CBTPACertificate = "A";

			var relatedTariffPivot1 = pivot.Children.AddNew();
			relatedTariffPivot1.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			relatedTariffPivot1.CI_TariffNum = tariffNumber1;
			relatedTariffPivot1.CD_CBTPACertificate = "1";
			relatedTariffPivot1.CI_Description = "";

			var relatedTariffPivot2 = pivot.Children.AddNew();
			relatedTariffPivot2.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			relatedTariffPivot2.CI_TariffNum = tariffNumber2;
			relatedTariffPivot2.CD_CBTPACertificate = "2";
			relatedTariffPivot2.CI_Description = "UPDATED 2";

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			Factory.Save();

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "Test1";
			AssertEquals("Load Part", 3, invoice.InvoiceLines.Count);

			AssertEquals("UPDATED", "UPDATED", invoiceLine.JI_Description);
			AssertEquals("child 1: UPDATED", "UPDATED", invoice.InvoiceLines[1].JI_Description);
			AssertEquals("child 2: UPDATED", "UPDATED 2", invoice.InvoiceLines[2].JI_Description);

			pivot.CI_Description = "UPDATE CI";
			Factory.Save();

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_PartNo = "Test1";
			AssertEquals("UPDATED", "UPDATE CI", invoiceLine2.JI_Description);
			AssertEquals("child 1: UPDATED", "UPDATE CI", invoice.InvoiceLines[3].JI_Description);
			AssertEquals("child 2: UPDATED", "UPDATED 2", invoice.InvoiceLines[2].JI_Description);
		}

		public void TestCBMANewProperties()
		{
			var manufacturer = Factory.New<OrgHeader>();
			manufacturer.OH_FullName = "THE MANUFACTURER";
			manufacturer.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.ForeignProducerIdentifier, "BACMEBR150520", Core.Constants.CountryCodes.UnitedStates);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_OA_ManufacturerAddress = manufacturer.MainAddress.PK;

			AssertEquals("invoiceLine.ManufacturerCompanyName", "THE MANUFACTURER", invoiceLine.ManufacturerCompanyName);
			manufacturer.MainAddress.OA_CompanyNameOverride = "THE BEST MANUFACTURER";
			AssertEquals("invoiceLine.ManufacturerCompanyName", "THE BEST MANUFACTURER", invoiceLine.ManufacturerCompanyName);

			invoiceLine.US_ControlledGroupName = "CGN23";
			AssertEquals("Controlled Group Name", Enterprise.Customs.Business.ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(invoiceLine.US_ControlledGroupNameInfo).Caption);

			invoiceLine.US_AllocationQuantity = 100m;
			AssertEquals("Allocation Quantity", Enterprise.Customs.Business.ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(invoiceLine.US_AllocationQuantityInfo).Caption);

			invoiceLine.US_FlavorContentCreditInd = ZBool.True;
			AssertEquals("Flavor Content Credit Indicator", Enterprise.Customs.Business.ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(invoiceLine.US_FlavorContentCreditIndInfo).Caption);
			AssertEquals("Foreign Producer Identifier", Enterprise.Customs.Business.ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(invoiceLine.US_FPIInfo).Caption);

			invoiceLine.US_TaxCode = Core.Constants.USCustoms.FeeCodes.OtherExcise;
			invoiceLine.US_TTBRateDesignationCode = "B01010";
			AssertEquals(0.1363469m, invoiceLine.US_CBMADefaultTaxRate);

			invoiceLine.US_CBMADefaultTaxRate = 1m;
			AssertEquals(1m, invoiceLine.US_CBMADefaultTaxRate);

			invoiceLine.JI_OA_ManufacturerAddress = ZGuid.Empty;
			AssertEquals("invoiceLine.ManufacturerCompanyName", ZString.Empty, invoiceLine.ManufacturerCompanyName);

			invoiceLine.US_SecondarySPI = "C";
			invoiceLine.US_TTBRateDesignationCode = "W01020";
			invoiceLine.US_TaxRateS = AppendixBTaxRateList.Codes.Wines_2;
			AssertEquals("US_TaxRateS changed, then US_TTBRateDesignationCode should be tmpey", ZString.Empty, invoiceLine.US_TTBRateDesignationCode);

			invoiceLine.US_TTBRateDesignationCode = "W01010";
			invoiceLine.US_TaxRateS = AppendixBTaxRateList.Codes.Specify;
			AssertEquals("US_TaxRateS changed but is Specify then US_TTBRateDesignationCode should not change", "W01010", invoiceLine.US_TTBRateDesignationCode);
		}

		public void TestDefaultCBMARelatedFieldsWhenProductClaimIsCBMAAndCBMA23IsEffective()
		{
			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			var address = manufacturer.MainAddress;
			address.CustomsCodes.AddNew("FPI", "TTB-FP-0123456");
			address.CustomsCodes.AddNew("FPW", "WABCWIN150521");

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var wrapper = OrgHeaderWrapper.New(importer);

			var allocationQty1 = wrapper.AllocationQuantityPerFPIs.AddNew();
			allocationQty1.US_OA_ManufacturerAddress = address.PK;
			allocationQty1.US_ForeignProducerIdentifier = "TTB-FP-0123456";

			var allocationQty2 = wrapper.AllocationQuantityPerFPIs.AddNew();
			allocationQty2.US_OA_ManufacturerAddress = address.PK;
			allocationQty2.US_ForeignProducerIdentifier = "WABCWIN150521";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OA_ManufacturerAddress = manufacturer.MainAddress.PK;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2023, 01, 10);
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.C;

			AssertEquals("TTB-FP-0123456", invoiceLine.US_FPI);
			AssertEquals(ZString.Empty, invoiceLine.US_TTBRateDesignationCode);
			AssertEquals(ZDecimal.Zero, invoiceLine.US_CBMADefaultTaxRate);
		}

		public void TestDefaultCBMARelatedFieldsWhenProductClaimIsCBMAAndCBMA23IsNotEffective()
		{
			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer.OH_RL_NKClosestPort = "HKHKG";

			manufacturer.MainAddress.CustomsCodes.AddNew("FPS", "B1232022");
			manufacturer.MainAddress.CustomsCodes.AddNew("FPW", "W18042202");

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_RL_NKClosestPort = "USPHL";
			var wrapper = OrgHeaderWrapper.New(importer);
			var controlledGroup = wrapper.ImportersControlledGroupNames.AddNew();
			controlledGroup.US_GroupName = "GROUP1";
			var allocationQty = wrapper.AllocationQuantityPerFPIs.AddNew();
			allocationQty.US_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			allocationQty.US_ForeignProducerIdentifier = "B1232022";
			allocationQty.US_AllocationQuantity = 1000m;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2022, 01, 10);
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.C;
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			AssertEquals("GROUP1", invoiceLine.US_ControlledGroupName);
			AssertEquals("B1232022", invoiceLine.US_FPI);
			AssertEquals(1000m, invoiceLine.US_AllocationQuantity);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedImporter = newFactory.Load<OrgHeader>(importer.PK);
			wrapper = OrgHeaderWrapper.New(loadedImporter);
			controlledGroup = wrapper.ImportersControlledGroupNames.AddNew();
			controlledGroup.US_GroupName = "GROUP2";
			allocationQty = wrapper.AllocationQuantityPerFPIs.AddNew();
			allocationQty.US_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			allocationQty.US_ForeignProducerIdentifier = "W18042202";
			allocationQty.US_AllocationQuantity = 2000m;

			var loadedInvoice = newFactory.Load<JobComInvoiceHeader>(invoice.PK);
			invoiceLine = loadedInvoice.JobComInvoiceLines.AddNew();

			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2022, 01, 10);
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.C;
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			AssertEquals(ZString.Empty, invoiceLine.US_ControlledGroupName);
			AssertEquals(ZString.Empty, invoiceLine.US_FPI);
			AssertEquals(ZDecimal.Zero, invoiceLine.US_AllocationQuantity);

			invoiceLine.US_TaxCode = Core.Constants.USCustoms.FeeCodes.Wines;
			AssertEquals(ZString.Empty, invoiceLine.US_ControlledGroupName);
			AssertEquals("W18042202", invoiceLine.US_FPI);
			AssertEquals(2000m, invoiceLine.US_AllocationQuantity);
		}

		public void TestNoExceptionWhenFirstObjectOnLoadedInCreateBusinessObjectsFromRows()
		{
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var consignee = newFactory.New<OrgHeader>();
			consignee.FillWithValidTestData();
			consignee.OH_Code = "ORGUSCHI";
			consignee.OH_IsConsignee = ZBool.True;
			consignee.OH_RL_NKClosestPort = "USCHI";

			var declaration = newFactory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.JE_OH_Importer = consignee.PK;

			var invoice = declaration.Invoices.AddNew();
			using (invoice.GetLineNumberRenumberingSuspender())
			{
				var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine1.JI_LineNo = 10;
				invoiceLine1.JI_Description = "LINE 2";
				invoiceLine1.JI_PartNo = "PART1";
				invoiceLine1.JI_InvoiceQuantity = 100m;
				invoiceLine1.JI_InvoiceUQ = "PCE";

				var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine2.JI_LineNo = 10;
				invoiceLine2.JI_Description = "LINE 1";
				invoiceLine2.JI_PartNo = "PART1";
				invoiceLine2.JI_InvoiceQuantity = 100m;
				invoiceLine2.JI_InvoiceUQ = "PCE";

				var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine3.JI_LineNo = 10;
				invoiceLine3.JI_Description = "LINE 3";
				invoiceLine3.JI_PartNo = "PART1";
				invoiceLine3.JI_InvoiceQuantity = 100m;
				invoiceLine3.JI_InvoiceUQ = "PCE";
			}
			newFactory.Save();

			newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var part1 = newFactory.New<OrgSupplierPart>();
			part1.OP_PartNum = "PART1";
			part1.OP_Desc = "PART DESC";
			part1.OP_Weight = 100m;
			part1.OP_WeightUQ = "HG";
			part1.RelatedOrganisations.AddOrganisationIfNotExist(consignee.PK, OrgPartRelation.RelationshipTypes.Owner);
			var pivot1 = part1.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot1.CI_TariffNum = USCTariff.CottonFeeApplicable;

			var partChild1 = pivot1.Children.AddNew();
			partChild1.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			partChild1.CI_TariffNum = "8101000010";

			var partChild2 = pivot1.Children.AddNew();
			partChild2.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			partChild2.CI_TariffNum = "7101000010";

			newFactory.Save();

			newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var query = new ZQuery(JobComInvoiceLineSchema.JI_ClusterKey, declaration.JE_ClusterKey);
			query.OrderBy = JobComInvoiceLineSchema.Constants.JI_Description;
			var invoiceLines = newFactory.Load<JobComInvoiceLine>(query);
			void AssertData(JobComInvoiceLine line, ZShort expectedLineNo, ZString expectedDesc)
			{
				CombineAssertions(line.HumanReadableName, () =>
				{
					AssertEquals("JI_Description", expectedDesc, line.JI_Description);
					AssertEquals("JI_LineNo", expectedLineNo, line.JI_LineNo);
				});
			}
			AssertData(invoiceLines[0], 4, "LINE 1");
			AssertData(invoiceLines[1], 1, "LINE 2");
			AssertData(invoiceLines[2], 7, "LINE 3");
		}

		public void TestIsMPFOverridden()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals("Should return false because there is no MPF yet", false, invoiceLine.IsMPFOverridden);

			var hmf = invoiceLine.FeeCusCodes.AddNew(Core.Constants.USCustoms.FeeCodes.HMF);
			hmf.CY_IsOverridden = true;
			AssertEquals("Should return false because there is no MPF yet", false, invoiceLine.IsMPFOverridden);

			var mpf = InvoiceLine.FeeCusCodes.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing);
			AssertEquals("Should return false becasue MPF is not overridden", false, InvoiceLine.IsMPFOverridden);

			mpf.CY_IsOverridden = true;
			AssertEquals("Should return true becasue MPF is overridden", true, InvoiceLine.IsMPFOverridden);
		}

		public void TestIImportWrappedPropertySupporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var wrappedPropertySupporter = invoiceLine as IImportWrappedPropertySupporter;
			var propertyName = wrappedPropertySupporter.GetWrappedProperty("JI_PartNo");
			AssertEquals("should return null", null, propertyName);

			propertyName = wrappedPropertySupporter.GetWrappedProperty("JobUSComInvoiceLine.USI_DRW99ClaimedDuty");
			AssertEquals("should return property name of JobUSComInvoiceLine", "USI_DRW99ClaimedDuty", propertyName);
		}

		public void TestFirstLicensePermitNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var permitLicense = invoiceLine.LicenceAndPermits.AddNew();
			permitLicense.CY_Code = "AA";
			permitLicense.CY_Data = "12345678";
			AssertEquals("AA", invoiceLine.US_FirstPermitLicenseType);
			AssertEquals("12345678", invoiceLine.US_FirstPermitLicenseNumber);

			invoiceLine.US_FirstPermitLicenseType = "BB";
			invoiceLine.US_FirstPermitLicenseNumber = "987654321";
			AssertEquals("BB", permitLicense.CY_Code);
			AssertEquals("987654321", permitLicense.CY_Data);
		}

		public void TestReproduce_WI00517091()
		{
			var sql = @"
INSERT INTO dbo.RefDbEntUs_USCTariff (UE_PK, UE_Tariff, UE_DateFrom, UE_DateTo, UE_PermitLicenseIndicator, UE_DutyComputationCode) VALUES (NEWID(), '147258', '2021-01-01 00:00:00.000', '2049-12-31 00:00:00.000', '01', '7');
INSERT INTO dbo.RefDbEntUs_USCTariff (UE_PK, UE_Tariff, UE_DateFrom, UE_DateTo, UE_PermitLicenseIndicator, UE_DutyComputationCode) VALUES (NEWID(), '258369', '2021-01-01 00:00:00.000', '2049-12-31 00:00:00.000', '01', '7');
INSERT INTO dbo.RefDbEntUs_USCTariff (UE_PK, UE_Tariff, UE_DateFrom, UE_DateTo, UE_PermitLicenseIndicator, UE_DutyComputationCode) VALUES (NEWID(), '123123', '2021-01-01 00:00:00.000', '2049-12-31 00:00:00.000', '01', '7');

";
			Db.Connection.ExecuteNonQuery(sql);

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TOR";

			var partA = Factory.New<Customs.Business.OrgSupplierPart>();
			partA.OP_PartNum = "AAA";
			var partB = Factory.New<Customs.Business.OrgSupplierPart>();
			partB.OP_PartNum = "BBB";

			var partRelationA = Factory.New<OrgPartRelation>();
			partRelationA.OU_OP = partA.PK;
			partRelationA.OU_OH = org.PK;
			partRelationA.OU_LocalPartNumber = "AAA";
			var partRelationB = Factory.New<OrgPartRelation>();
			partRelationB.OU_OP = partB.PK;
			partRelationB.OU_OH = org.PK;
			partRelationB.OU_LocalPartNumber = "BBB";

			var pivotA = Factory.New<CusClassPartPivot>();
			pivotA.CI_CI_Parent = ZGuid.Empty;
			pivotA.CI_RN_NKCountry = "US";
			pivotA.CI_OP = partA.PK;
			pivotA.CI_ChildType = "HTI";
			pivotA.CI_TariffNum = "147258";
			var pivotB = Factory.New<CusClassPartPivot>();
			pivotB.CI_CI_Parent = ZGuid.Empty;
			pivotB.CI_RN_NKCountry = "US";
			pivotB.CI_OP = partB.PK;
			pivotB.CI_ChildType = "HTI";
			pivotB.CI_TariffNum = "258369";

			Factory.Save();

			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			jobDeclaration.US_EntryType = "02";
			jobDeclaration.JE_OH_Importer = org.PK;
			jobDeclaration.JE_AutoWeightApportion = true;
			var invoice = jobDeclaration.Invoices.AddNew();
			invoice.JZ_ClusterKey = 100;
			invoice.JZ_Weight = 777;
			invoice.JZ_WeightUQ = "KG";
			invoice.JZ_NetWeight = 555;
			invoice.JZ_NetWeightUQ = "KG";
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_PartNo = "AAA";
			invoiceLine1.JI_LineNo = 1;
			invoiceLine1.JI_OP = ZGuid.Empty;
			invoiceLine1.JI_ClusterKey = 100;
			invoiceLine1.US_DRWIsForImportSection = true;
			invoiceLine1.US_DRWIsForExportSection = true;
			invoiceLine1.JI_NetWeight = 100;
			invoiceLine1.JI_NetWeightUQ = Core.Constants.Weight.Pounds;
			invoiceLine1.JI_Weight = 123;
			invoiceLine1.JI_WeightUQ = "KG";
			invoiceLine1.JI_LinePrice = 22;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_PartNo = "BBB";
			invoiceLine2.JI_LineNo = 2;
			invoiceLine2.JI_ClusterKey = 100;
			invoiceLine2.US_DRWIsForImportSection = true;
			invoiceLine2.US_DRWIsForExportSection = true;
			invoiceLine2.JI_NetWeight = 200;
			invoiceLine2.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine2.JI_Weight = 2330;
			invoiceLine2.JI_WeightUQ = Core.Constants.Weight.Grams;
			invoiceLine2.JI_LinePrice = 33;
			invoiceLine2.JI_Tariff = "123123";

			Factory.Save();

			sql = $@"Update dbo.JobComInvoiceLine
set JI_NetWeight=200,
    JI_CustomsQuantity=0,
    JI_CustomsUnitQty='LB',
    JI_SystemLastEditTimeUtc = GetUtcDate(),
    JI_SystemLastEditUser = '~BP'
where JI_PK='{invoiceLine2.PK}';

Update dbo.JobComInvoiceLine
set JI_NetWeight=100,
    JI_CustomsQuantity=0,
    JI_CustomsUnitQty='LB',
    JI_SystemLastEditTimeUtc = GetUtcDate(),
    JI_SystemLastEditUser = '~BP'
where JI_PK='{invoiceLine1.PK}'";
			Db.Connection.ExecuteNonQuery(sql);

			var newFactory = new BusinessObjectFactory();
			var query = new ZQuery();
			query.AddToFilter(JoinCondition.Or, JobComInvoiceLineSchema.PK, SQLComparisonOperator.Equal, invoiceLine1.PK);
			query.AddToFilter(JoinCondition.Or, JobComInvoiceLineSchema.PK, SQLComparisonOperator.Equal, invoiceLine2.PK);
			query.OrderBy = JobComInvoiceLineSchema.Constants.JI_LineNo;
			AssertNoExceptionThrown(() => newFactory.Load(typeof(JobComInvoiceLine), query));
		}

		public void TestFPIFieldType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			AssertEquals("invoiceLine.FPIFieldType", nameof(FieldType.TextDropEdit), invoiceLine.FPIFieldType);

			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.C;
			AssertEquals("invoiceLine.FPIFieldType", nameof(FieldType.Text), invoiceLine.FPIFieldType);

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			AssertEquals("invoiceLine.FPIFieldType", nameof(FieldType.TextDropEdit), invoiceLine.FPIFieldType);

			declaration.US_EntryType = EntryTypeList.Codes.AircraftVesselSupplyIE;
			AssertEquals("invoiceLine.FPIFieldType", nameof(FieldType.Text), invoiceLine.FPIFieldType);

			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2022, 12, 12);
			AssertEquals("invoiceLine.FPIFieldType", nameof(FieldType.TextDropEdit), invoiceLine.FPIFieldType);
		}

		public void TestSetDefaultATFIndicatorsWhenTariffOrSubTariffChanges()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0011223344";
			tariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);

			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "4433221100";
			tariff2.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff2.UE_DateTo = ZDateTime.Today.AddYears(1);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_ATFInd = OGAIndicatorList.Codes.Declared;

			invoiceLine.US_SupTariff = tariff.UE_Tariff;
			AssertEquals("ATF Indicator should be 'D', because regular tariff is NOT changed.", OGAIndicatorList.Codes.Declared, invoiceLine.US_ATFInd);

			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			AssertEquals("ATF Indicator should be empty, because regular tariff is changed.", ZString.Empty, invoiceLine.US_ATFInd);

			invoiceLine.US_ATFInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.JI_Tariff = tariff2.UE_Tariff;
			AssertEquals("ATF Indicator should be empty, because regular tariff is changed.", ZString.Empty, invoiceLine.US_ATFInd);

			invoiceLine.US_ATFInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.JI_Tariff = tariff2.UE_Tariff;
			AssertEquals("ATF Indicator should be 'D', because regular tariff is NOT changed.", OGAIndicatorList.Codes.Declared, invoiceLine.US_ATFInd);
		}

		public void TestSetDefaultDEAIndicatorsWhenTariffOrSubTariffChanges()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0011223344";
			tariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);

			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "4433221100";
			tariff2.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff2.UE_DateTo = ZDateTime.Today.AddYears(1);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_DEAInd = OGAIndicatorList.Codes.Declared;

			invoiceLine.US_SupTariff = tariff.UE_Tariff;
			AssertEquals("DEA Indicator should be 'D', because regular tariff is NOT changed.", OGAIndicatorList.Codes.Declared, invoiceLine.US_DEAInd);

			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			AssertEquals("DEA Indicator should be empty, because regular tariff is changed.", ZString.Empty, invoiceLine.US_DEAInd);

			invoiceLine.US_DEAInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.JI_Tariff = tariff2.UE_Tariff;
			AssertEquals("DEA Indicator should be empty, because regular tariff is changed.", ZString.Empty, invoiceLine.US_DEAInd);

			invoiceLine.US_DEAInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.JI_Tariff = tariff2.UE_Tariff;
			AssertEquals("DEA Indicator should be 'D', because regular tariff is NOT changed.", OGAIndicatorList.Codes.Declared, invoiceLine.US_DEAInd);
		}

		public void TestSetDefaultLaceyIndicatorsWhenTariffOrSubTariffChanges()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0011223344";
			tariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);

			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "4433221100";
			tariff2.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff2.UE_DateTo = ZDateTime.Today.AddYears(1);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_LaceyIndicator = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_LaceyDisclaimReason = PGADisclaimReasonList.Codes.A;

			invoiceLine.US_SupTariff = tariff.UE_Tariff;
			AssertEquals("Lacey Indicator should be 'C', because regular tariff is NOT changed.", OGAIndicatorList.Codes.Disclaimed, invoiceLine.US_LaceyIndicator);
			AssertEquals("Lacey Disclaim Reason should be 'A', because regular tariff is NOT changed.", PGADisclaimReasonList.Codes.A, invoiceLine.US_LaceyDisclaimReason);

			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			AssertEquals("Lacey Indicator should be empty, because regular tariff is changed.", ZString.Empty, invoiceLine.US_LaceyIndicator);
			AssertEquals("Lacey Disclaim Reason should be empty, because regular tariff is changed.", ZString.Empty, invoiceLine.US_LaceyDisclaimReason);

			invoiceLine.US_LaceyIndicator = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_LaceyDisclaimReason = PGADisclaimReasonList.Codes.A;
			invoiceLine.JI_Tariff = tariff2.UE_Tariff;
			AssertEquals("Lacey Indicator should be empty, because regular tariff is changed.", ZString.Empty, invoiceLine.US_LaceyIndicator);
			AssertEquals("Lacey Disclaim Reason should be empty, because regular tariff is changed.", ZString.Empty, invoiceLine.US_LaceyDisclaimReason);

			invoiceLine.US_LaceyIndicator = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_LaceyDisclaimReason = PGADisclaimReasonList.Codes.A;
			invoiceLine.JI_Tariff = tariff2.UE_Tariff;
			AssertEquals("Lacey Indicator should be 'C', because regular tariff is NOT changed.", OGAIndicatorList.Codes.Disclaimed, invoiceLine.US_LaceyIndicator);
			AssertEquals("Lacey Disclaim Reason should be 'A', because regular tariff is NOT changed.", PGADisclaimReasonList.Codes.A, invoiceLine.US_LaceyDisclaimReason);
		}

		public void TestSupAdditionalTariffRelatedProperties()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();

			#region Prov Add Tariff 1

			AssertEquals("Prov Add. Tariff 1 is empty", ZString.Empty, invoiceLine.SupFormattedAdditionalTariff1);
			Assert("Override Prov Add. Duty 1 is not override", !invoiceLine.US_OverrideSupAdditionalTariff1Duty);
			AssertEquals("Prov Add. Duty 1 is zero", ZDecimal.Zero, invoiceLine.US_SupAdditionalTariff1Duty);
			Assert("Prov Add. Duty 1 is read-only", invoiceLine.US_SupAdditionalTariff1Duty_ReadOnly);
			Assert("Prov Add. Qty 1 is read-only", invoiceLine.US_SupAdditionalTariff1Qty_ReadOnly);
			AssertEquals("Prov Add. UQ 1 is empty", ZString.Empty, invoiceLine.US_SupAdditionalTariff1UQ);
			AssertEquals("Prov Add. Qty 1 is zero", ZDecimal.Zero, invoiceLine.US_SupAdditionalTariff1Qty);
			AssertEquals("Prov Add. Goods Value 1 is zero", ZDecimal.Zero, invoiceLine.US_SupAdditionalTariff1GoodsValue);

			invoiceLine.SupFormattedAdditionalTariff1 = "1111";
			invoiceLine.US_OverrideSupAdditionalTariff1Duty = true;
			invoiceLine.US_SupAdditionalTariff1Duty = 100m;
			invoiceLine.US_SupAdditionalTariff1UQ = "KG";
			invoiceLine.US_SupAdditionalTariff1Qty = 200m;
			invoiceLine.US_SupAdditionalTariff1GoodsValue = 500m;
			AssertEquals("Prov Add. Tariff 1 is 1111", "1111", invoiceLine.SupFormattedAdditionalTariff1);
			Assert("Override Prov Add. Duty 1 is override", invoiceLine.US_OverrideSupAdditionalTariff1Duty);
			AssertEquals("Prov Add. Duty 1 is 100", 100m, invoiceLine.US_SupAdditionalTariff1Duty);
			Assert("Prov Add. Duty 1 is editable", !invoiceLine.US_SupAdditionalTariff1Duty_ReadOnly);
			Assert("Prov Add. Qty 1 is editable", !invoiceLine.US_SupAdditionalTariff1Qty_ReadOnly);
			AssertEquals("Prov Add. UQ 1 is KG", "KG", invoiceLine.US_SupAdditionalTariff1UQ);
			AssertEquals("Prov Add. Qty 1 is 200", 200m, invoiceLine.US_SupAdditionalTariff1Qty);
			AssertEquals("Prov Add. Goods Value 1 is 500", 500m, invoiceLine.US_SupAdditionalTariff1GoodsValue);

			#endregion

			#region Prov Add Tariff 2

			AssertEquals("Prov Add. Tariff 2 is empty", ZString.Empty, invoiceLine.SupFormattedAdditionalTariff2);
			Assert("Override Prov Add. Duty 2 is not override", !invoiceLine.US_OverrideSupAdditionalTariff2Duty);
			AssertEquals("Prov Add. Duty 2 is zero", ZDecimal.Zero, invoiceLine.US_SupAdditionalTariff2Duty);
			Assert("Prov Add. Duty 2 is read-only", invoiceLine.US_SupAdditionalTariff2Duty_ReadOnly);
			Assert("Prov Add. Qty 2 is read-only", invoiceLine.US_SupAdditionalTariff2Qty_ReadOnly);
			AssertEquals("Prov Add. UQ 2 is empty", ZString.Empty, invoiceLine.US_SupAdditionalTariff2UQ);
			AssertEquals("Prov Add. Qty 2 is zero", ZDecimal.Zero, invoiceLine.US_SupAdditionalTariff2Qty);
			AssertEquals("Prov Add. Goods Value 2 is zero", ZDecimal.Zero, invoiceLine.US_SupAdditionalTariff2GoodsValue);

			invoiceLine.SupFormattedAdditionalTariff2 = "2222";
			invoiceLine.US_OverrideSupAdditionalTariff2Duty = true;
			invoiceLine.US_SupAdditionalTariff2Duty = 200m;
			invoiceLine.US_SupAdditionalTariff2UQ = "KH";
			invoiceLine.US_SupAdditionalTariff2Qty = 300m;
			invoiceLine.US_SupAdditionalTariff2GoodsValue = 600m;
			AssertEquals("Prov Add. Tariff 2 is 2222", "2222", invoiceLine.SupFormattedAdditionalTariff2);
			Assert("Override Prov Add. Duty 2 is override", invoiceLine.US_OverrideSupAdditionalTariff2Duty);
			AssertEquals("Prov Add. Duty 2 is 200", 200m, invoiceLine.US_SupAdditionalTariff2Duty);
			Assert("Prov Add. Duty 2 is editable", !invoiceLine.US_SupAdditionalTariff2Duty_ReadOnly);
			Assert("Prov Add. Qty 2 is editable", !invoiceLine.US_SupAdditionalTariff2Qty_ReadOnly);
			AssertEquals("Prov Add. UQ 2 is KH", "KH", invoiceLine.US_SupAdditionalTariff2UQ);
			AssertEquals("Prov Add. Qty 2 is 300", 300m, invoiceLine.US_SupAdditionalTariff2Qty);
			AssertEquals("Prov Add. Goods Value 2 is 600", 600m, invoiceLine.US_SupAdditionalTariff2GoodsValue);

			#endregion

			#region Prov Add Tariff 3

			AssertEquals("Prov Add. Tariff 3 is empty", ZString.Empty, invoiceLine.SupFormattedAdditionalTariff3);
			Assert("Override Prov Add. Duty 3 is not override", !invoiceLine.US_OverrideSupAdditionalTariff3Duty);
			AssertEquals("Prov Add. Duty 3 is zero", ZDecimal.Zero, invoiceLine.US_SupAdditionalTariff3Duty);
			Assert("Prov Add. Duty 3 is read-only", invoiceLine.US_SupAdditionalTariff3Duty_ReadOnly);
			Assert("Prov Add. Qty 3 is read-only", invoiceLine.US_SupAdditionalTariff3Qty_ReadOnly);
			AssertEquals("Prov Add. UQ 3 is empty", ZString.Empty, invoiceLine.US_SupAdditionalTariff3UQ);
			AssertEquals("Prov Add. Qty 3 is zero", ZDecimal.Zero, invoiceLine.US_SupAdditionalTariff3Qty);
			AssertEquals("Prov Add. Goods Value 3 is zero", ZDecimal.Zero, invoiceLine.US_SupAdditionalTariff3GoodsValue);

			invoiceLine.SupFormattedAdditionalTariff3 = "3333";
			invoiceLine.US_OverrideSupAdditionalTariff3Duty = true;
			invoiceLine.US_SupAdditionalTariff3Duty = 300m;
			invoiceLine.US_SupAdditionalTariff3UQ = "KI";
			invoiceLine.US_SupAdditionalTariff3Qty = 400m;
			invoiceLine.US_SupAdditionalTariff3GoodsValue = 700m;
			AssertEquals("Prov Add. Tariff 3 is 3333", "3333", invoiceLine.SupFormattedAdditionalTariff3);
			Assert("Override Prov Add. Duty 3 is override", invoiceLine.US_OverrideSupAdditionalTariff3Duty);
			AssertEquals("Prov Add. Duty 3 is 100", 300m, invoiceLine.US_SupAdditionalTariff3Duty);
			Assert("Prov Add. Duty 3 is editable", !invoiceLine.US_SupAdditionalTariff3Duty_ReadOnly);
			Assert("Prov Add. Qty 3 is editable", !invoiceLine.US_SupAdditionalTariff3Qty_ReadOnly);
			AssertEquals("Prov Add. UQ 3 is KI", "KI", invoiceLine.US_SupAdditionalTariff3UQ);
			AssertEquals("Prov Add. Qty 3 is 300", 400m, invoiceLine.US_SupAdditionalTariff3Qty);
			AssertEquals("Prov Add. Goods Value 3 is 600", 700m, invoiceLine.US_SupAdditionalTariff3GoodsValue);

			#endregion

			#region Prov Add Tariff 4

			AssertEquals("Prov Add. Tariff 4 is empty", ZString.Empty, invoiceLine.SupFormattedAdditionalTariff4);
			Assert("Override Prov Add. Duty 4 is not override", !invoiceLine.US_OverrideSupAdditionalTariff4Duty);
			AssertEquals("Prov Add. Duty 4 is zero", ZDecimal.Zero, invoiceLine.US_SupAdditionalTariff4Duty);
			Assert("Prov Add. Duty 4 is read-only", invoiceLine.US_SupAdditionalTariff4Duty_ReadOnly);
			Assert("Prov Add. Qty 4 is read-only", invoiceLine.US_SupAdditionalTariff4Qty_ReadOnly);
			AssertEquals("Prov Add. UQ 4 is empty", ZString.Empty, invoiceLine.US_SupAdditionalTariff4UQ);
			AssertEquals("Prov Add. Qty 4 is zero", ZDecimal.Zero, invoiceLine.US_SupAdditionalTariff4Qty);
			AssertEquals("Prov Add. Goods Value 4 is zero", ZDecimal.Zero, invoiceLine.US_SupAdditionalTariff4GoodsValue);

			invoiceLine.SupFormattedAdditionalTariff4 = "4444";
			invoiceLine.US_OverrideSupAdditionalTariff4Duty = true;
			invoiceLine.US_SupAdditionalTariff4Duty = 400m;
			invoiceLine.US_SupAdditionalTariff4UQ = "KJ";
			invoiceLine.US_SupAdditionalTariff4Qty = 500m;
			invoiceLine.US_SupAdditionalTariff4GoodsValue = 800m;
			AssertEquals("Prov Add. Tariff 5 is 4444", "4444", invoiceLine.SupFormattedAdditionalTariff4);
			Assert("Override Prov Add. Duty 4 is override", invoiceLine.US_OverrideSupAdditionalTariff4Duty);
			AssertEquals("Prov Add. Duty 4 is 400", 400m, invoiceLine.US_SupAdditionalTariff4Duty);
			Assert("Prov Add. Duty 4 is editable", !invoiceLine.US_SupAdditionalTariff4Duty_ReadOnly);
			Assert("Prov Add. Qty 4 is editable", !invoiceLine.US_SupAdditionalTariff4Qty_ReadOnly);
			AssertEquals("Prov Add. UQ 4 is KJ", "KJ", invoiceLine.US_SupAdditionalTariff4UQ);
			AssertEquals("Prov Add. Qty 4 is 500", 500m, invoiceLine.US_SupAdditionalTariff4Qty);
			AssertEquals("Prov Add. Goods Value 4 is 800", 800m, invoiceLine.US_SupAdditionalTariff4GoodsValue);

			#endregion

			#region Prov Add Tariff 5

			AssertEquals("Prov Add. Tariff 5 is empty", ZString.Empty, invoiceLine.SupFormattedAdditionalTariff5);
			Assert("Override Prov Add. Duty 5 is not override", !invoiceLine.US_OverrideSupAdditionalTariff5Duty);
			AssertEquals("Prov Add. Duty 5 is zero", ZDecimal.Zero, invoiceLine.US_SupAdditionalTariff5Duty);
			Assert("Prov Add. Duty 5 is read-only", invoiceLine.US_SupAdditionalTariff5Duty_ReadOnly);
			Assert("Prov Add. Qty 5 is read-only", invoiceLine.US_SupAdditionalTariff5Qty_ReadOnly);
			AssertEquals("Prov Add. UQ 5 is empty", ZString.Empty, invoiceLine.US_SupAdditionalTariff5UQ);
			AssertEquals("Prov Add. Qty 5 is zero", ZDecimal.Zero, invoiceLine.US_SupAdditionalTariff5Qty);
			AssertEquals("Prov Add. Goods Value 5 is zero", ZDecimal.Zero, invoiceLine.US_SupAdditionalTariff5GoodsValue);

			invoiceLine.SupFormattedAdditionalTariff5 = "5555";
			invoiceLine.US_OverrideSupAdditionalTariff5Duty = true;
			invoiceLine.US_SupAdditionalTariff5Duty = 500m;
			invoiceLine.US_SupAdditionalTariff5UQ = "KL";
			invoiceLine.US_SupAdditionalTariff5Qty = 600m;
			invoiceLine.US_SupAdditionalTariff5GoodsValue = 900m;
			AssertEquals("Prov Add. Tariff 5 is 5555", "5555", invoiceLine.SupFormattedAdditionalTariff5);
			Assert("Override Prov Add. Duty 5 is override", invoiceLine.US_OverrideSupAdditionalTariff5Duty);
			AssertEquals("Prov Add. Duty 5 is 500", 500m, invoiceLine.US_SupAdditionalTariff5Duty);
			Assert("Prov Add. Duty 5 is editable", !invoiceLine.US_SupAdditionalTariff5Duty_ReadOnly);
			Assert("Prov Add. Qty 5 is editable", !invoiceLine.US_SupAdditionalTariff5Qty_ReadOnly);
			AssertEquals("Prov Add. UQ 5 is KL", "KL", invoiceLine.US_SupAdditionalTariff5UQ);
			AssertEquals("Prov Add. Qty 5 is 600", 600m, invoiceLine.US_SupAdditionalTariff5Qty);
			AssertEquals("Prov Add. Goods Value 5 is 900", 900m, invoiceLine.US_SupAdditionalTariff5GoodsValue);

			#endregion
		}

		public void TestSetDefaultOGAIndicatorsWhenTariffOrSubTariffChanges()
		{
			AssertSetDefaultOGAIndicatorsWhenTariffChanges(true);
			AssertSetDefaultOGAIndicatorsWhenTariffChanges(false);
		}

		void AssertSetDefaultOGAIndicatorsWhenTariffChanges(bool isTariffChange)
		{
			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "0011223344";
			tariff1.UE_PGACodes = "FD2FS4DT2EP2EP4EP6NM2NM4NM5NM6NM8AQ2FW2OM2EP8DT2AM4AM8TB2CP2";
			tariff1.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff1.UE_DateTo = ZDateTime.Today.AddYears(1);
			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "0000000001";
			tariff2.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff2.UE_DateTo = ZDateTime.Today.AddYears(1);
			var tariff3 = Factory.New<USCTariff>();
			tariff3.UE_Tariff = "0000000002";
			tariff3.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff3.UE_DateTo = ZDateTime.Today.AddYears(1);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.PGAFSIS, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.PGAFWS, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AMSNOP, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.USCPSC, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.USOMC, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				if (isTariffChange)
				{
					invoiceLine.JI_Tariff = tariff1.UE_Tariff;
				}
				else
				{
					invoiceLine.US_SupTariff = tariff1.UE_Tariff;
				}

				AssertEquals("US_FDAIndicator", OGAIndicatorList.Codes.Declared, invoiceLine.US_FDAIndicator);
				AssertEquals("US_FSISInd", OGAIndicatorList.Codes.Declared, invoiceLine.US_FSISInd);
				AssertEquals("US_ODSInd", OGAIndicatorList.Codes.Declared, invoiceLine.US_ODSInd);
				AssertEquals("US_VNEInd", OGAIndicatorList.Codes.Declared, invoiceLine.US_VNEInd);
				AssertEquals("US_PSTIndicator", OGAIndicatorList.Codes.Declared, invoiceLine.US_PSTIndicator);
				AssertEquals("US_NMFS370Ind", OGAIndicatorList.Codes.Declared, invoiceLine.US_NMFS370Ind);
				AssertEquals("US_NMFSAMRInd", OGAIndicatorList.Codes.Declared, invoiceLine.US_NMFSAMRInd);
				AssertEquals("US_NMFSHMSInd", OGAIndicatorList.Codes.Declared, invoiceLine.US_NMFSHMSInd);
				AssertEquals("US_NMFSSIMPInd", OGAIndicatorList.Codes.Declared, invoiceLine.US_NMFSSIMPInd);
				AssertEquals("US_APHISInd", OGAIndicatorList.Codes.Declared, invoiceLine.US_APHISInd);
				AssertEquals("US_FWSInd", OGAIndicatorList.Codes.Declared, invoiceLine.US_FWSInd);
				AssertEquals("US_OMCInd", OGAIndicatorList.Codes.Declared, invoiceLine.US_OMCInd);
				AssertEquals("US_TSCAInd", OGAIndicatorList.Codes.Declared, invoiceLine.US_TSCAInd);
				AssertEquals("US_NHTSAIndicator", OGAIndicatorList.Codes.Declared, invoiceLine.US_NHTSAIndicator);
				AssertEquals("US_AMSInd", OGAIndicatorList.Codes.Declared, invoiceLine.US_AMSInd);
				AssertEquals("US_NOPInd", OGAIndicatorList.Codes.Declared, invoiceLine.US_NOPInd);
				AssertEquals("US_TTBInd", OGAIndicatorList.Codes.Declared, invoiceLine.US_TTBInd);
				AssertEquals("US_CPSCInd", OGAIndicatorList.Codes.Declared, invoiceLine.US_CPSCInd);

				if (isTariffChange)
				{
					invoiceLine.JI_Tariff = tariff2.UE_Tariff;
				}
				else
				{
					invoiceLine.US_SupTariff = tariff2.UE_Tariff;
				}

				AssertEquals("US_FDAIndicator", ZString.Empty, invoiceLine.US_FDAIndicator);
				AssertEquals("US_FSISInd", ZString.Empty, invoiceLine.US_FSISInd);
				AssertEquals("US_ODSInd", ZString.Empty, invoiceLine.US_ODSInd);
				AssertEquals("US_VNEInd", ZString.Empty, invoiceLine.US_VNEInd);
				AssertEquals("US_PSTIndicator", ZString.Empty, invoiceLine.US_PSTIndicator);
				AssertEquals("US_NMFS370Ind", ZString.Empty, invoiceLine.US_NMFS370Ind);
				AssertEquals("US_NMFSAMRInd", ZString.Empty, invoiceLine.US_NMFSAMRInd);
				AssertEquals("US_NMFSHMSInd", ZString.Empty, invoiceLine.US_NMFSHMSInd);
				AssertEquals("US_NMFSSIMPInd", ZString.Empty, invoiceLine.US_NMFSSIMPInd);
				AssertEquals("US_APHISInd", ZString.Empty, invoiceLine.US_APHISInd);
				AssertEquals("US_FWSInd", ZString.Empty, invoiceLine.US_FWSInd);
				AssertEquals("US_OMCInd", ZString.Empty, invoiceLine.US_OMCInd);
				AssertEquals("US_TSCAInd", ZString.Empty, invoiceLine.US_TSCAInd);
				AssertEquals("US_NHTSAIndicator", ZString.Empty, invoiceLine.US_NHTSAIndicator);
				AssertEquals("US_AMSInd", ZString.Empty, invoiceLine.US_AMSInd);
				AssertEquals("US_NOPInd", ZString.Empty, invoiceLine.US_NOPInd);
				AssertEquals("US_TTBInd", ZString.Empty, invoiceLine.US_TTBInd);
				AssertEquals("US_CPSCInd", ZString.Empty, invoiceLine.US_CPSCInd);

				invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
				invoiceLine.US_FDADisclaimReason = PGADisclaimReasonList.Codes.A;
				invoiceLine.US_FSISInd = OGAIndicatorList.Codes.Disclaimed;
				invoiceLine.US_FSISDisclaimReason = PGADisclaimReasonList.Codes.A;
				invoiceLine.US_DOTIndicator = OGAIndicatorList.Codes.Disclaimed;
				invoiceLine.US_ODSInd = OGAIndicatorList.Codes.Disclaimed;
				invoiceLine.US_ODSDisclaimReason = PGADisclaimReasonList.Codes.A;
				invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Disclaimed;
				invoiceLine.US_VNEDisclaimReason = PGADisclaimReasonList.Codes.A;
				invoiceLine.US_PSTDisclaimReason = PGADisclaimReasonList.Codes.A;
				invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Disclaimed;
				invoiceLine.US_PSTDisclaimReason = PGADisclaimReasonList.Codes.A;
				invoiceLine.US_NMFS370Ind = OGAIndicatorList.Codes.Disclaimed;
				invoiceLine.US_NMFS370DisclaimReason = PGADisclaimReasonList.Codes.A;
				invoiceLine.US_NMFSCOAInd = OGAIndicatorList.Codes.Disclaimed;
				invoiceLine.US_NMFSAMRInd = OGAIndicatorList.Codes.Disclaimed;
				invoiceLine.US_NMFSAMRDisclaimReason = PGADisclaimReasonList.Codes.A;
				invoiceLine.US_NMFSHMSInd = OGAIndicatorList.Codes.Disclaimed;
				invoiceLine.US_NMFSHMSDisclaimReason = PGADisclaimReasonList.Codes.A;
				invoiceLine.US_NMFSSIMPInd = OGAIndicatorList.Codes.Disclaimed;
				invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Disclaimed;
				invoiceLine.US_APHISDisclaimReason = PGADisclaimReasonList.Codes.A;
				invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Disclaimed;
				invoiceLine.US_FWSDisclaimReason = PGADisclaimReasonList.Codes.A;
				invoiceLine.US_OMCInd = OGAIndicatorList.Codes.Disclaimed;
				invoiceLine.US_OMCDisclaimReason = PGADisclaimReasonList.Codes.A;
				invoiceLine.US_TSCAInd = OGAIndicatorList.Codes.Disclaimed;
				invoiceLine.US_TSCADisclaimReason = PGADisclaimReasonList.Codes.A;
				invoiceLine.US_NHTSAIndicator = OGAIndicatorList.Codes.Disclaimed;
				invoiceLine.US_NHTDisclaimReason = PGADisclaimReasonList.Codes.A;
				invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Disclaimed;
				invoiceLine.US_AMSDisclaimReason = PGADisclaimReasonList.Codes.A;
				invoiceLine.US_NOPInd = OGAIndicatorList.Codes.Disclaimed;
				invoiceLine.US_NOPDisclaimReason = PGADisclaimReasonList.Codes.A;
				invoiceLine.US_TTBInd = OGAIndicatorList.Codes.Disclaimed;
				invoiceLine.US_TTBDisclaimReason = PGADisclaimReasonList.Codes.A;
				invoiceLine.US_CPSCInd = OGAIndicatorList.Codes.Disclaimed;
				invoiceLine.US_CPSCDisclaimReason = PGADisclaimReasonList.Codes.A;

				if (isTariffChange)
				{
					invoiceLine.JI_Tariff = tariff3.UE_Tariff;
				}
				else
				{
					invoiceLine.US_SupTariff = tariff3.UE_Tariff;
				}

				AssertEquals("invoiceLine.US_FDAIndicator", OGAIndicatorList.Codes.Disclaimed, invoiceLine.US_FDAIndicator);
				AssertEquals("invoiceLine.US_FDADisclaimReason", PGADisclaimReasonList.Codes.A, invoiceLine.US_FDADisclaimReason);
				AssertEquals("invoiceLine.US_FSISInd", OGAIndicatorList.Codes.Disclaimed, invoiceLine.US_FSISInd);
				AssertEquals("invoiceLine.US_FSISDisclaimReason", PGADisclaimReasonList.Codes.A, invoiceLine.US_FSISDisclaimReason);
				AssertEquals("invoiceLine.US_DOTIndicator", OGAIndicatorList.Codes.Disclaimed, invoiceLine.US_DOTIndicator);
				AssertEquals("invoiceLine.US_ODSInd", OGAIndicatorList.Codes.Disclaimed, invoiceLine.US_ODSInd);
				AssertEquals("invoiceLine.US_ODSDisclaimReason", PGADisclaimReasonList.Codes.A, invoiceLine.US_ODSDisclaimReason);
				AssertEquals("invoiceLine.US_VNEInd", OGAIndicatorList.Codes.Disclaimed, invoiceLine.US_VNEInd);
				AssertEquals("invoiceLine.US_VNEDisclaimReason", PGADisclaimReasonList.Codes.A, invoiceLine.US_VNEDisclaimReason);
				AssertEquals("invoiceLine.US_PSTDisclaimReason", PGADisclaimReasonList.Codes.A, invoiceLine.US_PSTDisclaimReason);
				AssertEquals("invoiceLine.US_PSTIndicator", OGAIndicatorList.Codes.Disclaimed, invoiceLine.US_PSTIndicator);
				AssertEquals("invoiceLine.US_PSTDisclaimReason", PGADisclaimReasonList.Codes.A, invoiceLine.US_PSTDisclaimReason);
				AssertEquals("invoiceLine.US_NMFS370Ind", OGAIndicatorList.Codes.Disclaimed, invoiceLine.US_NMFS370Ind);
				AssertEquals("invoiceLine.US_NMFS370DisclaimReason", PGADisclaimReasonList.Codes.A, invoiceLine.US_NMFS370DisclaimReason);
				AssertEquals("invoiceLine.US_NMFSCOAInd", OGAIndicatorList.Codes.Disclaimed, invoiceLine.US_NMFSCOAInd);
				AssertEquals("invoiceLine.US_NMFSAMRInd", OGAIndicatorList.Codes.Disclaimed, invoiceLine.US_NMFSAMRInd);
				AssertEquals("invoiceLine.US_NMFSAMRDisclaimReason", PGADisclaimReasonList.Codes.A, invoiceLine.US_NMFSAMRDisclaimReason);
				AssertEquals("invoiceLine.US_NMFSHMSInd", OGAIndicatorList.Codes.Disclaimed, invoiceLine.US_NMFSHMSInd);
				AssertEquals("invoiceLine.US_NMFSHMSDisclaimReason", PGADisclaimReasonList.Codes.A, invoiceLine.US_NMFSHMSDisclaimReason);
				AssertEquals("invoiceLine.US_NMFSSIMPInd", OGAIndicatorList.Codes.Disclaimed, invoiceLine.US_NMFSSIMPInd);
				AssertEquals("invoiceLine.US_APHISInd", OGAIndicatorList.Codes.Disclaimed, invoiceLine.US_APHISInd);
				AssertEquals("invoiceLine.US_APHISDisclaimReason", PGADisclaimReasonList.Codes.A, invoiceLine.US_APHISDisclaimReason);
				AssertEquals("invoiceLine.US_FWSInd", OGAIndicatorList.Codes.Disclaimed, invoiceLine.US_FWSInd);
				AssertEquals("invoiceLine.US_FWSDisclaimReason", PGADisclaimReasonList.Codes.A, invoiceLine.US_FWSDisclaimReason);
				AssertEquals("invoiceLine.US_OMCInd", OGAIndicatorList.Codes.Disclaimed, invoiceLine.US_OMCInd);
				AssertEquals("invoiceLine.US_OMCDisclaimReason", PGADisclaimReasonList.Codes.A, invoiceLine.US_OMCDisclaimReason);
				AssertEquals("invoiceLine.US_TSCAInd", OGAIndicatorList.Codes.Disclaimed, invoiceLine.US_TSCAInd);
				AssertEquals("invoiceLine.US_TSCADisclaimReason", PGADisclaimReasonList.Codes.A, invoiceLine.US_TSCADisclaimReason);
				AssertEquals("invoiceLine.US_NHTSAIndicator", OGAIndicatorList.Codes.Disclaimed, invoiceLine.US_NHTSAIndicator);
				AssertEquals("invoiceLine.US_NHTDisclaimReason", PGADisclaimReasonList.Codes.A, invoiceLine.US_NHTDisclaimReason);
				AssertEquals("invoiceLine.US_AMSInd", OGAIndicatorList.Codes.Disclaimed, invoiceLine.US_AMSInd);
				AssertEquals("invoiceLine.US_AMSDisclaimReason", PGADisclaimReasonList.Codes.A, invoiceLine.US_AMSDisclaimReason);
				AssertEquals("invoiceLine.US_NOPInd", OGAIndicatorList.Codes.Disclaimed, invoiceLine.US_NOPInd);
				AssertEquals("invoiceLine.US_NOPDisclaimReason", PGADisclaimReasonList.Codes.A, invoiceLine.US_NOPDisclaimReason);
				AssertEquals("invoiceLine.US_TTBInd", OGAIndicatorList.Codes.Disclaimed, invoiceLine.US_TTBInd);
				AssertEquals("invoiceLine.US_TTBDisclaimReason", PGADisclaimReasonList.Codes.A, invoiceLine.US_TTBDisclaimReason);
				AssertEquals("invoiceLine.US_CPSCInd", OGAIndicatorList.Codes.Disclaimed, invoiceLine.US_CPSCInd);
				AssertEquals("invoiceLine.US_CPSCDisclaimReason", PGADisclaimReasonList.Codes.A, invoiceLine.US_CPSCDisclaimReason);

				if (isTariffChange)
				{
					invoiceLine.JI_Tariff = tariff1.UE_Tariff;
				}
				else
				{
					invoiceLine.US_SupTariff = tariff1.UE_Tariff;
				}

				AssertEquals("invoiceLine.US_FDAIndicator", OGAIndicatorList.Codes.Declared, invoiceLine.US_FDAIndicator);
				AssertEquals("invoiceLine.US_FDADisclaimReason", ZString.Empty, invoiceLine.US_FDADisclaimReason);
				AssertEquals("invoiceLine.US_FSISInd", OGAIndicatorList.Codes.Declared, invoiceLine.US_FSISInd);
				AssertEquals("invoiceLine.US_FSISDisclaimReason", ZString.Empty, invoiceLine.US_FSISDisclaimReason);
				AssertEquals("invoiceLine.US_DOTIndicator", OGAIndicatorList.Codes.Disclaimed, invoiceLine.US_DOTIndicator);
				AssertEquals("invoiceLine.US_ODSInd", OGAIndicatorList.Codes.Declared, invoiceLine.US_ODSInd);
				AssertEquals("invoiceLine.US_ODSDisclaimReason", ZString.Empty, invoiceLine.US_ODSDisclaimReason);
				AssertEquals("invoiceLine.US_VNEInd", OGAIndicatorList.Codes.Declared, invoiceLine.US_VNEInd);
				AssertEquals("invoiceLine.US_VNEDisclaimReason", ZString.Empty, invoiceLine.US_VNEDisclaimReason);
				AssertEquals("invoiceLine.US_PSTDisclaimReason", ZString.Empty, invoiceLine.US_PSTDisclaimReason);
				AssertEquals("invoiceLine.US_PSTIndicator", OGAIndicatorList.Codes.Declared, invoiceLine.US_PSTIndicator);
				AssertEquals("invoiceLine.US_PSTDisclaimReason", ZString.Empty, invoiceLine.US_PSTDisclaimReason);
				AssertEquals("invoiceLine.US_NMFS370Ind", OGAIndicatorList.Codes.Declared, invoiceLine.US_NMFS370Ind);
				AssertEquals("invoiceLine.US_NMFS370DisclaimReason", ZString.Empty, invoiceLine.US_NMFS370DisclaimReason);
				AssertEquals("invoiceLine.US_NMFSCOAInd", OGAIndicatorList.Codes.Disclaimed, invoiceLine.US_NMFSCOAInd);
				AssertEquals("invoiceLine.US_NMFSAMRInd", OGAIndicatorList.Codes.Declared, invoiceLine.US_NMFSAMRInd);
				AssertEquals("invoiceLine.US_NMFSAMRDisclaimReason", ZString.Empty, invoiceLine.US_NMFSAMRDisclaimReason);
				AssertEquals("invoiceLine.US_NMFSHMSInd", OGAIndicatorList.Codes.Declared, invoiceLine.US_NMFSHMSInd);
				AssertEquals("invoiceLine.US_NMFSHMSDisclaimReason", ZString.Empty, invoiceLine.US_NMFSHMSDisclaimReason);
				AssertEquals("invoiceLine.US_NMFSSIMPInd", OGAIndicatorList.Codes.Declared, invoiceLine.US_NMFSSIMPInd);
				AssertEquals("invoiceLine.US_APHISInd", OGAIndicatorList.Codes.Declared, invoiceLine.US_APHISInd);
				AssertEquals("invoiceLine.US_APHISDisclaimReason", ZString.Empty, invoiceLine.US_APHISDisclaimReason);
				AssertEquals("invoiceLine.US_FWSInd", OGAIndicatorList.Codes.Declared, invoiceLine.US_FWSInd);
				AssertEquals("invoiceLine.US_FWSDisclaimReason", ZString.Empty, invoiceLine.US_FWSDisclaimReason);
				AssertEquals("invoiceLine.US_OMCInd", OGAIndicatorList.Codes.Declared, invoiceLine.US_OMCInd);
				AssertEquals("invoiceLine.US_OMCDisclaimReason", ZString.Empty, invoiceLine.US_OMCDisclaimReason);
				AssertEquals("invoiceLine.US_TSCAInd", OGAIndicatorList.Codes.Declared, invoiceLine.US_TSCAInd);
				AssertEquals("invoiceLine.US_TSCADisclaimReason", ZString.Empty, invoiceLine.US_TSCADisclaimReason);
				AssertEquals("invoiceLine.US_NHTSAIndicator", OGAIndicatorList.Codes.Declared, invoiceLine.US_NHTSAIndicator);
				AssertEquals("invoiceLine.US_NHTDisclaimReason", ZString.Empty, invoiceLine.US_NHTDisclaimReason);
				AssertEquals("invoiceLine.US_AMSInd", OGAIndicatorList.Codes.Declared, invoiceLine.US_AMSInd);
				AssertEquals("invoiceLine.US_AMSDisclaimReason", ZString.Empty, invoiceLine.US_AMSDisclaimReason);
				AssertEquals("invoiceLine.US_NOPInd", OGAIndicatorList.Codes.Declared, invoiceLine.US_NOPInd);
				AssertEquals("invoiceLine.US_NOPDisclaimReason", ZString.Empty, invoiceLine.US_NOPDisclaimReason);
				AssertEquals("invoiceLine.US_TTBInd", OGAIndicatorList.Codes.Declared, invoiceLine.US_TTBInd);
				AssertEquals("invoiceLine.US_TTBDisclaimReason", ZString.Empty, invoiceLine.US_TTBDisclaimReason);
				AssertEquals("invoiceLine.US_CPSCInd", OGAIndicatorList.Codes.Declared, invoiceLine.US_CPSCInd);
				AssertEquals("invoiceLine.US_CPSCDisclaimReason", ZString.Empty, invoiceLine.US_CPSCDisclaimReason);
			}
		}

		public void TestSetDefaultOGAIndicatorsWhenTariffOrSubTariffChanges2()
		{
			AssertSetDefaultOGAIndicatorsWhenTariffChanges2(true);
			AssertSetDefaultOGAIndicatorsWhenTariffChanges2(false);
		}

		protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest)
		{
			return new USBusinessLightValidationTester(bizObjToTest);
		}

		void AssertSetDefaultOGAIndicatorsWhenTariffChanges2(bool isTariffChange)
		{
			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "0011223344";
			tariff1.UE_PGACodes = "EH2";
			tariff1.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff1.UE_DateTo = ZDateTime.Today.AddYears(1);
			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "0000000001";
			tariff2.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff2.UE_DateTo = ZDateTime.Today.AddYears(1);
			var tariff3 = Factory.New<USCTariff>();
			tariff3.UE_Tariff = "0000000002";
			tariff3.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff3.UE_DateTo = ZDateTime.Today.AddYears(1);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			if (isTariffChange)
			{
				invoiceLine.JI_Tariff = tariff1.UE_Tariff;
			}
			else
			{
				invoiceLine.US_SupTariff = tariff1.UE_Tariff;
			}
			AssertEquals("US_HFCInd", OGAIndicatorList.Codes.Declared, invoiceLine.US_HFCInd);

			if (isTariffChange)
			{
				invoiceLine.JI_Tariff = tariff2.UE_Tariff;
			}
			else
			{
				invoiceLine.US_SupTariff = tariff2.UE_Tariff;
			}
			AssertEquals("US_HFCInd", ZString.Empty, invoiceLine.US_HFCInd);

			invoiceLine.US_HFCInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_HFCDisclaimReason = PGADisclaimReasonList.Codes.A;

			if (isTariffChange)
			{
				invoiceLine.JI_Tariff = tariff3.UE_Tariff;
			}
			else
			{
				invoiceLine.US_SupTariff = tariff3.UE_Tariff;
			}
			AssertEquals("invoiceLine.US_HFCInd", OGAIndicatorList.Codes.Disclaimed, invoiceLine.US_HFCInd);
			AssertEquals("invoiceLine.US_HFCDisclaimReason", PGADisclaimReasonList.Codes.A, invoiceLine.US_HFCDisclaimReason);

			if (isTariffChange)
			{
				invoiceLine.JI_Tariff = tariff1.UE_Tariff;
			}
			else
			{
				invoiceLine.US_SupTariff = tariff1.UE_Tariff;
			}
			AssertEquals("invoiceLine.US_HFCInd", OGAIndicatorList.Codes.Declared, invoiceLine.US_HFCInd);
			AssertEquals("invoiceLine.US_HFCDisclaimReason", ZString.Empty, invoiceLine.US_HFCDisclaimReason);
		}

		protected override bool UseUniversalTariff => false;

		protected override Type ExpectedTypeOfApportionedCharges => typeof(InvoiceLineApportionChargeCollection);

		protected override Type ExpectedTypeOfCharges => typeof(JobComInvChargeCollection<InvoiceLineCharge>);

		protected override bool RatesAreReciprocal => true;

		protected override BaseJobDeclaration ImportJobDeclaration
		{
			get
			{
				JobDeclaration result = Factory.New<JobDeclaration>();
				result.JE_MessageType = JobMessageTypeList.Codes.Import;
				result.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
				result.US_EnableENS = true;
				return result;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(Factory, new ZString[] { USAESLicenseCode.Codes.C33 });
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			JobComInvoiceLine invoiceLine = (JobComInvoiceLine)base.GetNewBusinessObjectForDeleteTest(factory);
			invoiceLine.FDAs.AddNew();
			invoiceLine.DOTs.AddNew();
			invoiceLine.FeeCusCodes.AddNew();
			return invoiceLine;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			var invoiceHeader = declaration.Invoices.AddNew();
			return invoiceHeader.InvoiceLines.AddNew();
		}

		protected override Dictionary<string, IZType> CachedValueForSettingValueCallsRefreshBindingTestCore
		{
			get
			{
				var result = base.CachedValueForSettingValueCallsRefreshBindingTestCore;
				if (!result.ContainsKey(JobComInvoiceLine.Schema.JI_Tariff))
				{
					result.Add(JobComInvoiceLine.Schema.JI_Tariff, new ZString("1010101010"));
				}
				if (!result.ContainsKey(JobComInvoiceLine.Schema.JI_FormattedTariff))
				{
					result.Add(JobComInvoiceLine.Schema.JI_FormattedTariff, new ZString("2010101010"));
				}
				if (!result.ContainsKey(JobComInvoiceLine.Schema.US_SupTariff))
				{
					result.Add(JobComInvoiceLine.Schema.US_SupTariff, new ZString("3010101010"));
				}
				if (!result.ContainsKey(JobComInvoiceLine.Schema.SupTariffFormatted))
				{
					result.Add(JobComInvoiceLine.Schema.SupTariffFormatted, new ZString("4010101010"));
				}
				return result;
			}
		}

		new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		new JobComInvoiceHeader InvoiceHeader => (JobComInvoiceHeader)base.InvoiceHeader;

		new JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)base.InvoiceLine;

		void SetUpTariffsForTaxRelatedFields()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff.UE_Unit1 = "KG";

			USCTariffDutyRate dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Wines;
			dutyRate.UD_TaxFeeFlag = "1";
			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificRateFirstQuantity;
			dutyRate.UD_TaxFeeSpecificRate = 0.5m;

			USCTariff tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "00000001";
			tariff2.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff2.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff2.UE_Unit1 = "L";

			USCTariffDutyRate dutyRate2 = tariff2.DutyRates.AddNew();
			dutyRate2.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.OtherExcise;
			dutyRate2.UD_TaxFeeFlag = "1";
			dutyRate2.UD_TaxFeeComputationCode = ComputationCodeList.Codes.NoComputationFormulaAvailable;
		}

		void AssertInvoiceLinePivot(JobComInvoiceLine invoiceLine, ZShort lineNo, ZString tariff, ZString cBTPACertificateNo, ZGuid parentID, ZGuid parentProductPK)
		{
			AssertEquals("invoiceLine.JI_LineNo", lineNo, invoiceLine.JI_LineNo);
			AssertEquals("invoiceLine.JI_Tariff", tariff, invoiceLine.JI_Tariff);
			AssertEquals("invoiceLine.US_CBTPACertificateNo", cBTPACertificateNo, invoiceLine.US_CBTPACertificateNo);
			AssertEquals("invoiceLine.JI_ParentID", parentID, invoiceLine.JI_ParentID);
			AssertEquals("invoiceLine.US_JI_ParentProduct", parentProductPK, invoiceLine.US_JI_ParentProduct);
		}

		void SetDataForSupTariffTest() => SetDataForSupTariffTest(Factory);

		internal static void SetDataForSupTariffTest(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var startDate = new ZDate(2016, 01, 01);
			var endDate = new ZDate(2079, 01, 01);

			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "HSN");
			factory.Save();

			var tariff1 = factory.New<USCTariff>();
			tariff1.UE_Tariff = "7301100000";
			tariff1.UE_DateFrom = startDate;
			tariff1.UE_DateTo = endDate;
			var tariff2 = factory.New<USCTariff>();
			tariff2.UE_Tariff = "8450110010";
			tariff2.UE_DateFrom = startDate;
			tariff2.UE_DateTo = endDate;
			var tariff3 = factory.New<USCTariff>();
			tariff3.UE_Tariff = "7216100010";
			tariff3.UE_DateFrom = startDate;
			tariff3.UE_DateTo = endDate;
			var tariff4 = factory.New<USCTariff>();
			tariff4.UE_Tariff = "7616995160";
			tariff4.UE_DateFrom = startDate;
			tariff4.UE_DateTo = endDate;
			factory.Save();

			var progTariff1 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99038001", startDate, endDate);
			var progTariff2 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99034501", startDate, endDate);
			var progTariff3 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99034502", startDate, endDate);
			var progTariff4 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99038060", startDate, endDate);
			var progTariff5 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99038061", startDate, endDate);
			var progTariff6 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99034105", startDate, endDate);
			var dutyRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode = helper.LoadOrCreateNewCusRateCode(factory, "DTY", dutyRateType.PK);
			factory.Save();
			var rate1 = helper.CreateRate(progTariff1, rateCode.PK, startDate, endDate, "0");
			var rate2 = helper.CreateRate(progTariff2, rateCode.PK, startDate, endDate, "0");
			var rate3 = helper.CreateRate(progTariff3, rateCode.PK, startDate, endDate, "0");
			var rate4 = helper.CreateRate(progTariff4, rateCode.PK, startDate, endDate, "0");
			var rate5 = helper.CreateRate(progTariff5, rateCode.PK, startDate, endDate, "0");
			var rate6 = helper.CreateRate(progTariff6, rateCode.PK, startDate, endDate, "0");
			var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.HongKong, "HK", startDate, endDate);
			var tradeGroupCountry = helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.HongKong, startDate, endDate);
			var tradeGroup2 = helper.CreateTradeGroup(Core.Constants.CountryCodes.KoreaSouth, "KR", startDate, endDate);
			var tradeGroupCountry2 = helper.AddCountry(tradeGroup2, Core.Constants.CountryCodes.KoreaSouth, startDate, endDate);
			var tradeGroup3 = helper.CreateTradeGroup(Core.Constants.CountryCodes.Canada, "CA", startDate, endDate);
			var tradeGroupCountry3 = helper.AddCountry(tradeGroup3, Core.Constants.CountryCodes.Canada, startDate, endDate);
			var applicability1 = helper.CreateCusApplicability(rate1, tradeGroup, startDate, endDate);
			var applicability2 = helper.CreateCusApplicability(rate2, tradeGroup, startDate, endDate);
			var applicability3 = helper.CreateCusApplicability(rate3, tradeGroup, startDate, endDate);
			var applicability4 = helper.CreateCusApplicability(rate4, tradeGroup2, startDate, endDate);
			var applicability5 = helper.CreateCusApplicability(rate5, tradeGroup2, startDate, endDate);
			var applicability6 = helper.CreateCusApplicability(rate6, tradeGroup3, startDate, endDate);
			var relationship1 = helper.CreateTariffRelationship(progTariff1.PK, hsnTariffType.PK, "73");
			var relationship2 = helper.CreateTariffRelationship(progTariff2.PK, hsnTariffType.PK, "84501100");
			var relationship3 = helper.CreateTariffRelationship(progTariff3.PK, hsnTariffType.PK, "84501100");
			var relationship4 = helper.CreateTariffRelationship(progTariff1.PK, hsnTariffType.PK, "721610");
			var relationship5 = helper.CreateTariffRelationship(progTariff4.PK, hsnTariffType.PK, "721610");
			var relationship6 = helper.CreateTariffRelationship(progTariff5.PK, hsnTariffType.PK, "721610");
			var relationship7 = helper.CreateTariffRelationship(progTariff6.PK, hsnTariffType.PK, "7616995160");
			var tariffAttribute1 = helper.CreateTariffAttribute("RULE", "A99", progTariff1);
			var tariffAttribute2 = helper.CreateTariffAttribute("RULE", "A99", progTariff2);
			var tariffAttribute3 = helper.CreateTariffAttribute("RULE", "A99", progTariff3);
			var tariffAttribute4 = helper.CreateTariffAttribute("RULE", "A99", progTariff4);
			var tariffAttribute5 = helper.CreateTariffAttribute("RULE", "A99", progTariff5);
			var tariffAttribute6 = helper.CreateTariffAttribute("RULE", "A99", progTariff6);

			factory.Save();
		}

		JobDeclaration GetImportDeclarationWithBuyerSupplier()
		{
			JobDeclaration result = Factory.NewWithValidTestData<JobDeclaration>();
			result.JE_MessageType = JobMessageTypeList.Codes.Import;
			result.JE_OH_Supplier = Factory.NewWithValidTestData<OrgHeader>().PK;
			result.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			return result;
		}

		OrgSupplierPart GetPartWithRelationship(OrgHeader importer)
		{
			var result = Factory.New<OrgSupplierPart>();
			result.OP_PartNum = "ProductForTesting";
			result.RelatedOrganisations.AddOwner(importer);
			return result;
		}

		JobDeclaration GetDeclarationWithOrgSupplierPart()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = Factory.New<OrgHeader>().PK;
			declaration.JE_OH_Importer = Factory.New<OrgHeader>().PK;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableAII = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "ProductForTesting";

			OrgPartRelation relation1 = product.RelatedOrganisations.AddNew();
			relation1.OU_LocalPartNumber = "XXX";
			relation1.OU_OH = declaration.JE_OH_Supplier;
			relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			return declaration;
		}

		OrgSupplierPart GetProductForFDAAndPGATests()
		{
			importer = Factory.NewWithValidTestData<OrgHeader>();
			supplier = Factory.NewWithValidTestData<OrgHeader>();

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "NICE WATCH";
			product.OP_Desc = "Nice Watch Batter-Powered";

			product.RelatedOrganisations.AddOwner(importer);
			product.RelatedOrganisations.AddSupplier(supplier);
			product.RelatedOrganisations.AddSupplier(Factory.NewWithValidTestData<OrgHeader>());

			var classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "TestLookup";
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;

			var importPivot = product.PivotsForBinding.AddNew();
			importPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			importPivot.CI_CC = classification.PK;

			return product;
		}
		OrgHeader importer;
		OrgHeader supplier;
		OrgSupplierPart product;

		sealed class JobComInvoiceLineForTest : JobComInvoiceLine, IPGADataChangeTrackerSupporter
		{
			public JobComInvoiceLineForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{ }

			public bool IsUnCommittedRow => IsUnCommittedRow_Mock;

			public bool IsUnCommittedRow_Mock;

			public int TrackerCount;

			public PGADataChangeTracker Tracker
			{
				get
				{
					TrackerCount++;
					return Declaration?.PGATrackerHelper;
				}
			}
		}
	}
}

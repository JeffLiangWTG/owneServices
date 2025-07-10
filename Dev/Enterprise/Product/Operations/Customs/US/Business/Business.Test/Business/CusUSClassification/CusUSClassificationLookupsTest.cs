using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CusUSClassificationLookupsTest : TestCaseWithFactory
	{
		public void TestTypeOfSimpeLookups()
		{
			var usClass = Factory.New<CusUSClassification>();
			AssertEquals(typeof(USCCountryCollection), usClass.Lookups.USCountryList.GetType());
			AssertEquals(typeof(SecondarySpecProgIndicatorList), usClass.Lookups.ProductClaimList.GetType());
			AssertEquals(typeof(PIRPRulingTypeList), usClass.Lookups.CD_RulingTypeList.GetType());
			AssertEquals(typeof(OGAIndicatorList), usClass.Lookups.US_OGAIndicatorList.GetType());
			AssertEquals(typeof(CodeDescriptionPairList), usClass.Lookups.US_OGAIndicatorWithoutDisclaimerList.GetType());
			AssertEquals(typeof(TSCAIndicatorList), usClass.Lookups.CD_TSCAIndicatorList.GetType());
			AssertEquals(typeof(TaxApplyList), usClass.Lookups.TaxApplyList.GetType());
			AssertEquals(typeof(RateTypeList), usClass.Lookups.CD_SelectedRateTypeList.GetType());
			AssertEquals(typeof(RefCurrencyCollection), usClass.Lookups.Currencies.GetType());
			AssertEquals(typeof(AESOriginIndicatorList), usClass.Lookups.CD_OriginIndicatorList.GetType());
			AssertEquals(typeof(ExportInformationCodeList), usClass.Lookups.CD_ExportCode_List.GetType());
			AssertEquals(typeof(CodeDescriptionPairList), usClass.Lookups.CD_ITARExemptionNoCodes.GetType());
			AssertEquals(typeof(USMLCategoryCodes), usClass.Lookups.CD_DDTCUSMLCategoryCodes.GetType());
			AssertEquals(typeof(DDTCUnitOfMeasureList), usClass.Lookups.CD_DDTCUnitOfMeasureList.GetType());
			AssertEquals(typeof(ZoneStatusList), usClass.Lookups.CD_ZoneStatusList.GetType());
			AssertEquals(typeof(PGADisclaimReasonList), usClass.Lookups.PGADisclaimReasonList.GetType());
		}

		public void TestCD_TSCAODSCertIndividualList()
		{
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			Assert("Should contain broker", pivot.USClassificationLookups.CD_TSCAODSCertIndividualList.ContainsCode(PartyTypeList.Codes.CustomsBroker));
			Assert("Should contain importer", pivot.USClassificationLookups.CD_TSCAODSCertIndividualList.ContainsCode(PartyTypeList.Codes.Importer));
		}

		public void TestOtherReconIssueList()
		{
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			AssertEquals("OtherReconIssueList", typeof(ReconIssueCodeList), pivot.USClassificationLookups.OtherReconIssueList.GetType());
			AssertEquals("Should not contains ReconIssueCodeList.Codes.FTA code", false, pivot.USClassificationLookups.OtherReconIssueList.ContainsCode(ReconIssueCodeList.Codes.FTA));
			AssertEquals(typeof(ADDCVDNonReimbursementList), pivot.USClassificationLookups.CD_ADDCVDNonReimbursementList.GetType());
			Assert("Should contain Declared", pivot.USClassificationLookups.CD_ADDCVDNonReimbursementList.ContainsCode(ADDCVDNonReimbursementList.Codes.Declared));
			Assert("Should contain Once-off", pivot.USClassificationLookups.CD_ADDCVDNonReimbursementList.ContainsCode(ADDCVDNonReimbursementList.Codes.OnceOff));
		}

		public void TestTaxCodeList()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			USCTariffDutyRate dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Wines;
			dutyRate.UD_TaxFeeFlag = "1";
			USCTariff tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "00000001";
			tariff2.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff2.UE_DateTo = ZDateTime.MaxSmallDateTime;
			USCTariffDutyRate dutyRate2 = tariff2.DutyRates.AddNew();
			dutyRate2.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.OtherExcise;
			dutyRate2.UD_TaxFeeFlag = "1";
			OrgSupplierPart part = Factory.NewWithValidTestData<OrgSupplierPart>();
			CusClassification classification = Factory.New<CusClassification>();
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			classification.CC_LookupCode = "TEST";
			classification.CC_TariffNum = "00000000";
			CusClassPartPivot pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_CC = classification.PK;
			AssertEquals(2, pivot.USClassificationLookups.TaxCodeList.Count);
			Assert(pivot.USClassificationLookups.TaxCodeList.ContainsCode(Core.Constants.USCustoms.FeeCodes.Wines));
			Assert(pivot.USClassificationLookups.TaxCodeList.ContainsCode(Core.Constants.USCustoms.FeeCodes.DistilledSpirits));
			classification.CC_TariffNum = "00000001";
			AssertEquals(1, pivot.USClassificationLookups.TaxCodeList.Count);
			Assert(pivot.USClassificationLookups.TaxCodeList.ContainsCode(Core.Constants.USCustoms.FeeCodes.OtherExcise));
		}

		public void TestSPIListInCusUSClassification()
		{
			OrgSupplierPart part = Factory.NewWithValidTestData<OrgSupplierPart>();
			CusClassPartPivot pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "3902205000";
			pivot.CD_UC_NKCountryOfOrigin = "MX";
			pivot.CD_UC_NKCountryOfExport = "MX";
			Assert(pivot.USClassificationLookups.SPIList.ContainsCode("MX"));
			AssertEquals("SPIList.Count", 2, pivot.USClassificationLookups.SPIList.Count);
		}

		[NUnit.Framework.TestDate(2009, 1, 1)]
		public void TestNAIsNotAnOptionWhenSPIBecomesMandatory()
		{
			OrgSupplierPart part = Factory.NewWithValidTestData<OrgSupplierPart>();
			CusClassPartPivot pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_SupplementalTariff = "99100466";
			pivot.CI_TariffNum = "3902205000";
			pivot.CD_UC_NKCountryOfOrigin = "SG";
			AssertEquals(1, pivot.USClassificationLookups.SPIList.Count);
			Assert(pivot.USClassificationLookups.SPIList.ContainsCode("SG"));
			Assert(!pivot.USClassificationLookups.SPIList.ContainsCode("N/A"));
		}

		public void TestTaxRateList()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff.UE_Unit1 = "PFL";
			USCTariffDutyRate dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.DistilledSpirits;
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
			USCTariff tariff3 = Factory.New<USCTariff>();
			tariff3.UE_Tariff = "00000002";
			tariff3.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff3.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff3.UE_Unit1 = "KG";
			USCTariffDutyRate dutyRate3 = tariff3.DutyRates.AddNew();
			dutyRate3.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Wines;
			dutyRate3.UD_TaxFeeFlag = "1";
			dutyRate3.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificSpecific;
			dutyRate3.UD_TaxFeeAdvalorem = 0.4m;
			dutyRate3.UD_TaxFeeSpecificRate = 0.6m;
			OrgSupplierPart part = Factory.NewWithValidTestData<OrgSupplierPart>();
			CusClassification classification = Factory.New<CusClassification>();
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			classification.CC_LookupCode = "TEST";
			classification.CC_TariffNum = "00000000";
			CusClassPartPivot pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_CC = classification.PK;
			pivot.CD_TaxCode = Core.Constants.USCustoms.FeeCodes.DistilledSpirits;
			AssertEquals(3, pivot.USClassificationLookups.TaxRateList.Count);
			Assert(pivot.USClassificationLookups.TaxRateList.ContainsCode(tariff.GetTaxFeeRateDescription(pivot.CD_TaxCode, ZString.Empty)));
			Assert(pivot.USClassificationLookups.TaxRateList.ContainsCode(AppendixBTaxRateList.Codes.DistilledSpirits));
			Assert(pivot.USClassificationLookups.TaxRateList.ContainsCode(AppendixBTaxRateList.Codes.Specify));
			classification.CC_TariffNum = "00000001";
			AssertEquals(5, pivot.USClassificationLookups.TaxRateList.Count);
			classification.CC_TariffNum = "00000002";
			pivot.CD_TaxCode = Core.Constants.USCustoms.FeeCodes.Wines;
			pivot.CD_TaxRateType = RateTypeList.Codes.Primary;
			Assert(pivot.USClassificationLookups.TaxRateList.ContainsCode("60c/KG")); // primary rate
			Assert(!pivot.USClassificationLookups.TaxRateList.ContainsCode("40c/KG")); // secondary rate
			pivot.CD_TaxRateType = RateTypeList.Codes.Secondary;
			Assert(!pivot.USClassificationLookups.TaxRateList.ContainsCode("60c/KG")); // primary rate
			Assert(pivot.USClassificationLookups.TaxRateList.ContainsCode("40c/KG")); // secondary rate
			pivot.CD_ProductClaim = SecondarySpecProgIndicatorList.Codes.C;
			pivot.CD_TaxCode = Core.Constants.USCustoms.FeeCodes.OtherExcise;
			var list1 = pivot.USClassificationLookups.TaxRateList;
			var list2 = pivot.USClassificationLookups.TaxRateList;
			AssertSame(list2, list1);
			AssertEquals(5, list1.Count);
			AssertEquals("(022)Cigarette papers", list1["3.15c/50"].Description);
		}

		public void TestCBMATaxRateList()
		{
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			var classification = Factory.New<CusClassification>();
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			classification.CC_LookupCode = "TEST";
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_CC = classification.PK;
			AssertEquals(0, pivot.USClassificationLookups.CBMATaxRateList.Count);

			pivot.CD_TaxCode = "017";
			AssertEquals(24, pivot.USClassificationLookups.CBMATaxRateList.Count);
			AssertArrayEqualsByElements(new string[] { "W01010", "W01020", "W01030" }, pivot.USClassificationLookups.CBMATaxRateList.Cast<ICodeDescription>().Select(s => s.Code).Take(3).ToArray());

			pivot.CD_TaxCode = "017";
			pivot.CD_TaxRateDesc = AppendixBTaxRateList.Codes.Wines_5;
			AssertEquals(3, pivot.USClassificationLookups.CBMATaxRateList.Count);
			AssertArrayEqualsByElements(new string[] { "W07010", "W07020", "W07030" }, pivot.USClassificationLookups.CBMATaxRateList.Cast<ICodeDescription>().Select(s => s.Code).ToArray());

			pivot.CD_TaxCode = "017";
			pivot.CD_TaxRateDesc = AppendixBTaxRateList.Codes.Specify;
			AssertEquals(24, pivot.USClassificationLookups.CBMATaxRateList.Count);
			AssertArrayEqualsByElements(new string[] { "W01010", "W01020", "W01030" }, pivot.USClassificationLookups.CBMATaxRateList.Cast<ICodeDescription>().Select(s => s.Code).Take(3).ToArray());
		}

		public void TestDDTCExemptionCodes()
		{
			OrgSupplierPart part = Factory.NewWithValidTestData<OrgSupplierPart>();
			CusClassPartPivot pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "3902205000";
			pivot.Details.CD_DDTCIndicator = "D";
			CodeDescriptionPairList list = pivot.USClassificationLookups.DDTCExemptionCodes;
			AssertEquals(55, list.Count);
			AssertContains("123.11B", ACEDDTCExemptionCodes.Codes._12311B, "123.11B");
			AssertContains("123.12", ACEDDTCExemptionCodes.Codes._12312, "123.12");
			AssertContains("126.4A1", ACEDDTCExemptionCodes.Codes._1264A1, "126.4A1");
			AssertContains("126.4A2", ACEDDTCExemptionCodes.Codes._1264A2, "126.4A2");
			AssertContains("126.4A3", ACEDDTCExemptionCodes.Codes._1264A3, "126.4A3");
			AssertContains("126.4A4", ACEDDTCExemptionCodes.Codes._1264A4, "126.4A4");
			AssertContains("126.4B1", ACEDDTCExemptionCodes.Codes._1264B1, "126.4B1");
			AssertContains("126.4B2", ACEDDTCExemptionCodes.Codes._1264B2, "126.4B2");
			AssertContains("126.4C1", ACEDDTCExemptionCodes.Codes._1264C1, "126.4C1");
			AssertContains("126.4C2", ACEDDTCExemptionCodes.Codes._1264C2, "126.4C2");
		}

		public void TestDDTCLicenseTypeCodes()
		{
			OrgSupplierPart part = Factory.NewWithValidTestData<OrgSupplierPart>();
			CusClassPartPivot pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "3902205000";
			pivot.Details.CD_DDTCIndicator = "D";
			DDTCLicenseTypeCodes list = pivot.USClassificationLookups.DDTCLicenseTypeCodes;
			AssertEquals(5, list.Count);
			AssertContains("S61", DDTCLicenseTypeCodes.Codes.S61, "S61");
			AssertContains("S62", DDTCLicenseTypeCodes.Codes.S62, "S62");
		}

		public void TestUSCACDutyDepositRates()
		{
			var uscCase = Factory.New<USCACCase>();
			uscCase.U5_CaseNumber = "AXXAAABBB";
			var rate = uscCase.CaseRates.AddNew();
			rate.U6_EffectiveDate = ZDateTime.Today;
			rate.U6_AdValoremRate = 0.1234m;
			rate.U6_SpecificRate = 0.2345m;
			rate.U6_Unit = "KG";
			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var classification = Factory.New<CusClassification>();
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			classification.CC_LookupCode = "TEST";
			classification.CC_TariffNum = "00000000";
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_CC = classification.PK;
			pivot.CD_ADDCaseNo = "AXXAAABBB";
			DepositRateIndicatorList list = pivot.USClassificationLookups.AntidumpingDutyDepositRates;
			AssertEquals(4, list.Count);
			AssertCodeDescription(list[0], DepositRateIndicatorList.Codes.AdValorem, "12.34%");
			AssertCodeDescription(list[1], DepositRateIndicatorList.Codes.OverrideAdValorem, DepositRateIndicatorList.Descriptions.OverrideAdValorem);
			AssertCodeDescription(list[2], DepositRateIndicatorList.Codes.Specific, "23c/KG");
			AssertCodeDescription(list[3], DepositRateIndicatorList.Codes.OverrideSpecific, DepositRateIndicatorList.Descriptions.OverrideSpecific);
		}

		public void TestCountervailingDutyDepositRates()
		{
			var uscCase = Factory.New<USCACCase>();
			uscCase.U5_CaseNumber = "CXXAAABBB";
			var rate = uscCase.CaseRates.AddNew();
			rate.U6_EffectiveDate = ZDateTime.Today;
			rate.U6_AdValoremRate = 0.1234m;
			rate.U6_SpecificRate = 0.2345m;
			rate.U6_Unit = "KG";
			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var classification = Factory.New<CusClassification>();
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			classification.CC_LookupCode = "TEST";
			classification.CC_TariffNum = "00000000";
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_CC = classification.PK;
			pivot.CD_CVDCaseNo = "CXXAAABBB";
			DepositRateIndicatorList list = pivot.USClassificationLookups.CountervailingDutyDepositRates;
			AssertEquals(4, list.Count);
			AssertCodeDescription(list[0], DepositRateIndicatorList.Codes.AdValorem, "12.34%");
			AssertCodeDescription(list[1], DepositRateIndicatorList.Codes.OverrideAdValorem, DepositRateIndicatorList.Descriptions.OverrideAdValorem);
			AssertCodeDescription(list[2], DepositRateIndicatorList.Codes.Specific, "23c/KG");
			AssertCodeDescription(list[3], DepositRateIndicatorList.Codes.OverrideSpecific, DepositRateIndicatorList.Descriptions.OverrideSpecific);
		}

		public void TestUSAESLicenseCodes()
		{
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(Factory, new ZString[] { USAESLicenseCode.Codes.C30 });
			var cusClassPartPivot = Factory.New<CusClassPartPivot>();
			var uSAESLicenseCodes = cusClassPartPivot.USClassificationLookups.USAESLicenseCodes;
			uSAESLicenseCodes.Load();
			AssertEquals(USAESLicenseCode.Codes.C30, uSAESLicenseCodes.Cast<ZZRefCusCodeListCombined>().FirstOrDefault().ZZD_Code);
		}

		void AssertCodeDescription(ICodeDescription codeDescription, string expectedCode, string expectedDescription)
		{
			AssertEquals("Code", expectedCode, codeDescription.Code);
			AssertEquals("Description", expectedDescription, codeDescription.Description);
		}
	}
}

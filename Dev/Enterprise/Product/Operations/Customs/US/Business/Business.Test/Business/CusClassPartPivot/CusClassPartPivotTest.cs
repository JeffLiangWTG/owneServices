using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CusClassPartPivot))]
	sealed class CusClassPartPivotTest : EnterpriseBusinessObjectTestCase
	{
		public void TestFWSIndicatorReadOnly()
		{
			var fwsCodeType = Universal.Constants.FunctionalityTypes.EnableFWS;
			var dataGrouping = Core.Constants.CountryCodes.UnitedStates;
			var part = Factory.New<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(fwsCodeType, dataGrouping, ZDateTime.Today, false))
			{
				pivot.CD_FWSIndicator = ZString.Empty;
				AssertEquals(false, pivot.CD_FWSIndicatorInfo.ReadOnly);
				pivot.CD_FWSIndicator = OGAIndicatorList.Codes.Declared;
				AssertEquals(false, pivot.CD_FWSIndicatorInfo.ReadOnly);
				pivot.CD_FWSIndicator = OGAIndicatorList.Codes.Disclaimed;
				AssertEquals(false, pivot.CD_FWSIndicatorInfo.ReadOnly);
			}
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(fwsCodeType, dataGrouping, ZDateTime.Today, true))
			{
				pivot.CD_FWSIndicator = ZString.Empty;
				AssertEquals(false, pivot.CD_FWSIndicatorInfo.ReadOnly);
				pivot.CD_FWSIndicator = OGAIndicatorList.Codes.Declared;
				AssertEquals(false, pivot.CD_FWSIndicatorInfo.ReadOnly);
				pivot.CD_FWSIndicator = OGAIndicatorList.Codes.Disclaimed;
				AssertEquals(false, pivot.CD_FWSIndicatorInfo.ReadOnly);
			}

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(fwsCodeType, dataGrouping, ZDateTime.Today, false))
			{
				pivot.CD_FWSIndicator = ZString.Empty;
				AssertEquals(true, pivot.CD_FWSIndicatorInfo.ReadOnly);
				pivot.CD_FWSIndicator = OGAIndicatorList.Codes.Declared;
				AssertEquals(false, pivot.CD_FWSIndicatorInfo.ReadOnly);
				pivot.CD_FWSIndicator = OGAIndicatorList.Codes.Disclaimed;
				AssertEquals(false, pivot.CD_FWSIndicatorInfo.ReadOnly);
			}
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(fwsCodeType, dataGrouping, ZDateTime.Today, true))
			{
				pivot.CD_FWSIndicator = ZString.Empty;
				AssertEquals(false, pivot.CD_FWSIndicatorInfo.ReadOnly);
				pivot.CD_FWSIndicator = OGAIndicatorList.Codes.Declared;
				AssertEquals(false, pivot.CD_FWSIndicatorInfo.ReadOnly);
				pivot.CD_FWSIndicator = OGAIndicatorList.Codes.Disclaimed;
				AssertEquals(false, pivot.CD_FWSIndicatorInfo.ReadOnly);
			}

			pivot.CI_ChildType = ClassificationTypeList.Codes.SHB;
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(fwsCodeType, dataGrouping, ZDateTime.Today, false))
			{
				pivot.CD_FWSIndicator = ZString.Empty;
				AssertEquals(true, pivot.CD_FWSIndicatorInfo.ReadOnly);
				pivot.CD_FWSIndicator = OGAIndicatorList.Codes.Declared;
				AssertEquals(false, pivot.CD_FWSIndicatorInfo.ReadOnly);
				pivot.CD_FWSIndicator = OGAIndicatorList.Codes.Disclaimed;
				AssertEquals(false, pivot.CD_FWSIndicatorInfo.ReadOnly);
			}
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(fwsCodeType, dataGrouping, ZDateTime.Today, true))
			{
				pivot.CD_FWSIndicator = ZString.Empty;
				AssertEquals(false, pivot.CD_FWSIndicatorInfo.ReadOnly);
				pivot.CD_FWSIndicator = OGAIndicatorList.Codes.Declared;
				AssertEquals(false, pivot.CD_FWSIndicatorInfo.ReadOnly);
				pivot.CD_FWSIndicator = OGAIndicatorList.Codes.Disclaimed;
				AssertEquals(false, pivot.CD_FWSIndicatorInfo.ReadOnly);
			}
		}

		public void TestDefaultPGAIndicatorsWhenExportTariffChanges()
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

			Pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			Pivot.CI_TariffNum = "0000000003";
			AssertExportPGAIndicators(ZString.Empty, Pivot);

			Pivot.CI_ChildType = ClassificationTypeList.Codes.SHB;
			Pivot.CI_TariffNum = "0000000001";
			AssertExportPGAIndicators(ZString.Empty, Pivot);

			Pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			Pivot.CI_TariffNum = "0000000004";
			AssertExportPGAIndicators(OGAIndicatorList.Codes.Declared, Pivot);

			Pivot.CI_TariffNum = "0000000002";
			AssertExportPGAIndicators(ZString.Empty, Pivot);
			Pivot.CI_ChildType = ClassificationTypeList.Codes.SHB;
			AssertExportPGAIndicators(OGAIndicatorList.Codes.Declared, Pivot);
		}

		void AssertExportPGAIndicators(string expectValue, CusClassPartPivot pivot)
		{
			AssertEquals(expectValue, pivot.CD_AMSIndicator);
			AssertEquals(expectValue, pivot.CD_ATFIndicator);
			AssertEquals(expectValue, pivot.CD_FWSIndicator);
			AssertEquals(expectValue, pivot.CD_NMFSHMSIndicator);
			AssertEquals(expectValue, pivot.CD_PSTIndicator);
			AssertEquals(expectValue, pivot.CD_TTBIndicator);
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

		public void TestHasChangeIsFalseWhenForttedValueIsSetToCI_TariffNum()
		{
			Pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			Pivot.CI_TariffNum = "00000000";
			Pivot.CD_ACEFDAIndicator = OGAIndicatorList.Codes.Declared;
			Factory.Save();
			Assert(!Pivot.HasChanges);
			Pivot.CI_TariffNum = "0000.00.00";
			Assert(!Pivot.HasChanges);
			AssertEquals(OGAIndicatorList.Codes.Declared, Pivot.CD_ACEFDAIndicator);
		}

		public void TestDefaultTaxRateDescForCBMAIfPossible()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff.UE_Unit1 = "KG";
			USCTariffDutyRate dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.OtherExcise;
			dutyRate.UD_TaxFeeFlag = "1";
			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificSpecific;
			Pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			Pivot.CI_TariffNum = "00000000";
			Pivot.CD_TaxApplicability = TaxApplyList.Codes.Override;
			Pivot.CD_ProductClaim = SecondarySpecProgIndicatorList.Codes.C;
			Pivot.CD_TaxCode = Core.Constants.USCustoms.FeeCodes.OtherExcise;
			AssertEquals("B01010", Pivot.CD_TTBRateDesignationCode);
		}

		[TestDate(2020, 06, 03)]
		public void TestEffectiveDate()
		{
			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			AssertEquals(new ZDateTime(2020, 06, 03), pivot.EffectiveDate);
			pivot.CI_DateStart = new ZDateTime(2020, 06, 02);
			AssertEquals(new ZDateTime(2020, 06, 03), pivot.EffectiveDate);
			pivot.CI_DateStart = new ZDateTime(2020, 06, 04);
			AssertEquals(new ZDateTime(2020, 06, 04), pivot.EffectiveDate);
			pivot.CI_DateStart = new ZDateTime(2020, 06, 01);
			pivot.CI_DateEnd = new ZDateTime(2020, 06, 02);
			AssertEquals(new ZDateTime(2020, 06, 01), pivot.EffectiveDate);
			pivot.CI_DateEnd = new ZDateTime(2020, 06, 04);
			AssertEquals(new ZDateTime(2020, 06, 03), pivot.EffectiveDate);
		}

		public void TestCusUSClassificationDeletedAfterPivotDeleted()
		{
			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var partPivot = product.PivotsForBinding.AddNew();
			AssertNotNull(partPivot.Details);
			var classPK = partPivot.Details.PK;
			partPivot.Delete();
			Factory.Save();
			AssertNull(new BusinessObjectFactory().Load<CusUSClassification>(classPK));
		}

		public void TestAddInfoWrapperProperties()
		{
			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var partPivot = product.PivotsForBinding.AddNew();
			AddInfoManagerTestHelper.AssertWrappedProperty(partPivot, "CD_ProductExclusion", "US_ProductExclusion", new ZString("01"));
			AddInfoManagerTestHelper.AssertWrappedProperty(partPivot, "CD_ExclusionNumber", "US_ExclusionNumber", new ZString("TEST000"));
		}

		public void TestCusUSClassificationNotCreatedAfterProductIsDeleted()
		{
			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var partPivot = product.PivotsForBinding.AddNew();
			var details = partPivot.Details;
			partPivot.Delete();
			AssertNoExceptionThrown(() => details = partPivot.Details);
			AssertNull("Should not return a CusUSClassification", details);
		}

		public void TestUpdatePivotsOnPart()
		{
			var part1 = Factory.New<OrgSupplierPart>();
			var part2 = Factory.New<OrgSupplierPart>();
			var rootPivot = part1.PivotsForBinding.AddNew();
			var childPivot = rootPivot.Children.AddNew();
			rootPivot.CI_OP = part2.PK;
			Assert("Should have been removed from old Part Pivots", !part1.PivotsForBinding.Contains(rootPivot));
			Assert("Should have been added to new Part Pivots", part2.PivotsForBinding.Contains(rootPivot));
			AssertEquals("Child pivot CI_OP should have been changed", part2.PK, childPivot.CI_OP);
			Assert("Child pivot should not have been added to any Part Pivots", !part1.PivotsForBinding.Contains(childPivot));
			Assert("Child pivot should not have been added to any Part Pivots", !part2.PivotsForBinding.Contains(childPivot));
			var rootPivot2 = part2.PivotsForBinding.AddNew();
			_ = rootPivot2.Children;
			childPivot.CI_CI_Parent = rootPivot2.PK;
			Assert("Child pivot should have been added to new Pivot Children", rootPivot2.Children.Contains(childPivot));
			Assert("Child pivot should have been removed from old Pivot Children", !rootPivot.Children.Contains(childPivot));
			childPivot.Delete();
			Assert("Deleted child pivot should not be added to Part Pivots.", !part2.PivotsForBinding.Contains(childPivot));
			rootPivot2.Delete();
			Assert("Deleted pivot should not be added to Part Pivots.", !part2.PivotsForBinding.Contains(rootPivot2));
		}

		public void TestIsTaxRateSpecifiedManually()
		{
			var partPivot = Factory.New<CusClassPartPivot>();
			partPivot.CD_TaxRateDesc = string.Empty;
			partPivot.CD_ProductClaim = SecondarySpecProgIndicatorList.Codes.C;
			AssertEquals(string.Empty, partPivot.CD_TaxRateDesc);
			partPivot.CD_ProductClaim = SecondarySpecProgIndicatorList.Codes.F;
			Assert(!partPivot.IsTaxRateSpecifiedManually);
			AssertEquals(SecondarySpecProgIndicatorList.Codes.F, partPivot.CD_ProductClaim);
			partPivot.CD_TaxRateDesc = AppendixBTaxRateList.Codes.Specify;
			Assert(partPivot.IsTaxRateSpecifiedManually);
			AssertEquals(SecondarySpecProgIndicatorList.Codes.F, partPivot.CD_ProductClaim);
			partPivot.CD_TaxRateDesc = AppendixBTaxRateList.CBMAEligible;
			Assert(partPivot.IsTaxRateSpecifiedManually);
			AssertEquals(SecondarySpecProgIndicatorList.Codes.C, partPivot.CD_ProductClaim);
			partPivot.CD_TaxApplicability = TaxApplyList.Codes.Override;
			partPivot.CD_TaxRateDesc = string.Empty;
			partPivot.CD_ProductClaim = SecondarySpecProgIndicatorList.Codes.C;
			Assert(!partPivot.IsTaxRateSpecifiedManually);
		}

		public void TestClearCD_TaxRateDescWhenCD_TaxRateTypeIsSet()
		{
			var partPivot = Factory.New<CusClassPartPivot>();
			partPivot.CD_TaxRateDesc = "ABC";
			AssertEquals("ABC", partPivot.CD_TaxRateDesc);
			partPivot.CD_TaxRateType = "R";
			AssertEquals(ZString.Empty, partPivot.CD_TaxRateDesc);
		}

		public void TestTaxRateDetail()
		{
			var part = Factory.New<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CD_TaxApplicability = TaxApplyList.Codes.Override;
			pivot.CD_TaxCode = Core.Constants.USCustoms.FeeCodes.OtherExcise;
			pivot.CD_ProductClaim = SecondarySpecProgIndicatorList.Codes.C;
			AssertEquals("pivot.CD_TaxRateDesc", ZString.Empty, pivot.CD_TaxRateDesc);
			AssertEquals("pivot.CD_TaxRate", ZDecimal.Zero, pivot.CD_TaxRate);
			AssertEquals("pivot.CD_TTBRateDesignationCode", "B01010", pivot.CD_TTBRateDesignationCode);
			AssertEquals("pivot.CD_CBMADefaultTaxRate", 0.1363469m, pivot.CD_CBMADefaultTaxRate);
			pivot.CD_TaxCode = Core.Constants.USCustoms.FeeCodes.Wines;
			AssertEquals("pivot.CD_TaxRateDesc", ZString.Empty, pivot.CD_TaxRateDesc);
			AssertEquals("pivot.CD_TaxRate", ZDecimal.Zero, pivot.CD_TaxRate);
			AssertEquals("pivot.CD_TTBRateDesignationCode", ZString.Empty, pivot.CD_TTBRateDesignationCode);
			AssertEquals("pivot.CD_CBMADefaultTaxRate", ZDecimal.Zero, pivot.CD_CBMADefaultTaxRate);
			pivot.CD_TaxRateDesc = "15.3389c/L";
			AssertEquals("pivot.CD_TaxRate", 0.15338900m, pivot.CD_TaxRate);
			pivot.CD_TTBRateDesignationCode = "W02020";
			AssertEquals("pivot.CD_CBMADefaultTaxRate", 0.17699530m, pivot.CD_CBMADefaultTaxRate);
		}

		public void TestIsCBMAProductClaim()
		{
			var part = Factory.New<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			AssertEquals(false, pivot.IsCBMAProductClaim);
			pivot.CD_TaxApplicability = TaxApplyList.Codes.Override;
			pivot.CD_ProductClaim = SecondarySpecProgIndicatorList.Codes.C;
			AssertEquals(true, pivot.IsCBMAProductClaim);
			pivot.CD_TaxApplicability = TaxApplyList.Codes.No;
			AssertEquals(true, pivot.IsCBMAProductClaim);
			pivot.CD_TaxApplicability = TaxApplyList.Codes.Yes;
			AssertEquals(true, pivot.IsCBMAProductClaim);
			pivot.CD_ProductClaim = SecondarySpecProgIndicatorList.Codes.S;
			AssertEquals(false, pivot.IsCBMAProductClaim);
		}

		public void TestPGAOGADataCopy()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			var pga = pivot.PGAs.AddNew();
			pga.US_CertifyingIndividual = "C";
			var pgaLicense = pga.Licenses.AddNew();
			pgaLicense.US_Type = "A10";
			var pgaCountry = pga.LaceyCountries.AddNew();
			pgaCountry.US_CountryCode = "US";
			var acefda = pivot.ACEFDAs.AddNew();
			acefda.US_ProcessingCode = "D";
			var acefdaAffirmation = acefda.AffirmationCodes.AddNew();
			acefdaAffirmation.CY_Code = "AIN";
			var omc = pivot.OMCHeaders.AddNew();
			omc.US_NetWeight = 120m;
			var ttb = pivot.TTBLines.AddNew();
			ttb.US_ProcessingCode = "E";
			var ttbCertificate = ttb.COLAAndCertificates.AddNew();
			ttbCertificate.US_COLA = "F";
			var nhtsa = pivot.NHTSALines.AddNew();
			nhtsa.US_CertifyingIndividual = "CB";
			var nhtsaDetail = nhtsa.NHTSADetails.AddNew();
			nhtsaDetail.US_NHTIdentityNumber = "G";
			var nhtsaDocument = nhtsa.NHTSADocuments.AddNew();
			nhtsaDocument.US_NHTDocumentType = "871";
			var cpsc = pivot.CPSCLines.AddNew();
			cpsc.US_ProcessingCode = "F";
			var cpscLot = cpsc.Lots.AddNew();
			cpscLot.US_LotNumberType = "1";
			var cpscRule = cpsc.RuleAndLabs.AddNew();
			cpscRule.US_RuleCodes = "2,3";
			var vne = pivot.VehicleLines.AddNew();
			vne.US_BodyCode = "J";
			var pst = pivot.PSTLines.AddNew();
			pst.US_BrandName = "K";
			var dea = pivot.DEAHeaders.AddNew();
			dea.US_DrugCode = "L";
			var aphis = pivot.APHISHeaders.AddNew();
			aphis.US_CategoryCode = "H";
			var atf = pivot.ATFLines.AddNew();
			atf.US_CategoryCode = "N";
			var ams = pivot.AMSLines.AddNew();
			ams.US_Program = "MO1";
			var fws = pivot.FWSLines.AddNew();
			fws.US_ProcessingCode = "FS";
			var fwsLicense = fws.Licenses.AddNew();
			fwsLicense.US_Type = "F10";
			var nmfs370 = pivot.NMFSLines.AddNew();
			nmfs370.US_ProgramType = NMFSProgramCodeList.Codes._370;
			nmfs370.US_DocumentType = NMFS370DocumentIdentifierList.Codes.NOAAForm370;
			var nmfs370HarvestingDetail = nmfs370.HarvestingDetails.AddNew();
			nmfs370HarvestingDetail.US_HarvestedCountry = Core.Constants.CountryCodes.UnitedStates;
			var nmfsAMR = pivot.NMFSLines.AddNew();
			nmfsAMR.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			nmfsAMR.US_DocumentType = NMFS370DocumentIdentifierList.Codes.ObserverStatement;
			var nmfsAMRHarvestingDetail = nmfsAMR.HarvestingDetails.AddNew();
			nmfsAMRHarvestingDetail.US_HarvestedCountry = Core.Constants.CountryCodes.Zambia;
			var nmfsHMS = pivot.NMFSLines.AddNew();
			nmfsHMS.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
			nmfsHMS.US_DocumentType = NMFS370DocumentIdentifierList.Codes.ObserverStatement;
			var nmfsHMSHarvestingDetail = nmfsHMS.HarvestingDetails.AddNew();
			nmfsHMSHarvestingDetail.US_HarvestedCountry = Core.Constants.CountryCodes.Kazakhstan;
			var nmfsSIMP = pivot.NMFSLines.AddNew();
			nmfsSIMP.US_ProgramType = NMFSProgramCodeList.Codes.SIM;
			nmfsSIMP.US_SpeciesCode = "AK";
			var nmfsSIMPHarvestingDetail = nmfsSIMP.HarvestingDetails.AddNew();
			nmfsSIMPHarvestingDetail.US_HarvestedCountry = Core.Constants.CountryCodes.China;
			var newPivot = (CusClassPartPivot)pivot.Clone();
			AssertNotNull(newPivot);
			var pgaCopy = newPivot.PGAs.Cast<PGA>().FirstOrDefault(x => x.US_CertifyingIndividual == "C");
			AssertNotNull("PGA", pgaCopy);
			Assert(pgaCopy.Licenses.Count > 0);
			Assert(pgaCopy.LaceyCountries.Count > 0);
			var aceFDA = newPivot.ACEFDAs.Cast<ACEFDA>().FirstOrDefault(x => x.US_ProcessingCode == "D");
			AssertNotNull("ACE FDA", aceFDA);
			Assert(aceFDA.AffirmationCodes.Any());
			Assert("OMC", newPivot.OMCHeaders.Cast<OMCHeader>().Any(x => x.US_NetWeight == 120m));
			Assert("TTB", newPivot.TTBLines.Cast<TTBLine>().Any(x => x.US_ProcessingCode == "E"));
			var nhtsaCopy = newPivot.NHTSALines.Cast<NHTSAHeader>().FirstOrDefault(x => x.US_CertifyingIndividual == "CB");
			AssertNotNull("NHTSA", nhtsaCopy);
			Assert(nhtsaCopy.NHTSADetails.Count > 0);
			Assert(nhtsaCopy.NHTSADocuments.Count > 0);
			var cpscCopy = newPivot.CPSCLines.Cast<CPSCHeader>().FirstOrDefault(x => x.US_ProcessingCode == "F");
			AssertNotNull("CPSC", cpscCopy);
			Assert(cpscCopy.Lots.Count > 0);
			Assert(cpscCopy.RuleAndLabs.Count > 0);
			Assert("VNE", newPivot.VehicleLines.Cast<Vehicle>().Any(x => x.US_BodyCode == "J"));
			Assert("PST", newPivot.PSTLines.Cast<Pesticide>().Any(x => x.US_BrandName == "K"));
			Assert("DEA", newPivot.DEAHeaders.Cast<DEAHeader>().Any(x => x.US_DrugCode == "L"));
			Assert("APH", newPivot.APHISHeaders.Cast<APHISHeader>().Any(x => x.US_CategoryCode == "H"));
			Assert("ATF", newPivot.ATFLines.Cast<ATF>().Any(x => x.US_CategoryCode == "N"));
			Assert("FS", newPivot.FWSLines.Cast<FWSHeader>().Any(x => x.US_ProcessingCode == "FS"));
			Assert("F10", newPivot.FWSLines.Cast<FWSHeader>().Any(x => x.Licenses.Count > 0 && x.Licenses.Cast<FWSLicense>().Any(y => y.US_Type == "F10")));
			var line370 = newPivot.NMFSLines.Cast<NMFSLine>().FirstOrDefault(x => x.US_ProgramType == NMFSProgramCodeList.Codes._370);
			AssertNotNull(line370);
			Assert("NMFS370", line370.HarvestingDetails.Cast<NMFSHarvestingDetail>().Any(k => k.US_HarvestedCountry == Core.Constants.CountryCodes.UnitedStates));
			var lineAMR = newPivot.NMFSLines.Cast<NMFSLine>().FirstOrDefault(x => x.US_ProgramType == NMFSProgramCodeList.Codes.AMR);
			AssertNotNull(lineAMR);
			Assert("NMFSAMR", lineAMR.HarvestingDetails.Cast<NMFSHarvestingDetail>().Any(k => k.US_HarvestedCountry == Core.Constants.CountryCodes.Zambia));
			var lineHMS = newPivot.NMFSLines.Cast<NMFSLine>().FirstOrDefault(x => x.US_ProgramType == NMFSProgramCodeList.Codes.HMS);
			AssertNotNull(lineHMS);
			Assert("NMFSHMS", lineHMS.HarvestingDetails.Cast<NMFSHarvestingDetail>().Any(k => k.US_HarvestedCountry == Core.Constants.CountryCodes.Kazakhstan));
			var lineSIMP = newPivot.NMFSLines.Cast<NMFSLine>().FirstOrDefault(x => x.US_ProgramType == NMFSProgramCodeList.Codes.SIM);
			AssertNotNull(lineSIMP);
			Assert("NMFSSIMP", lineSIMP.HarvestingDetails.Cast<NMFSHarvestingDetail>().Any(k => k.US_HarvestedCountry == Core.Constants.CountryCodes.China));
		}

		public void TestWhenClassificationChanged()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1010101010";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;
			tariff.UE_PGACodes = "FD2EP4DT2";
			tariff.UE_OGACodes = "FD2EP4DT2";
			var classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "@#$34";
			classification.CC_TariffNum = "1010101010";
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "PART434";
			pivot.CI_CC = classification.PK;
			pivot.CI_OP = product.PK;
			AssertEquals("Vehicle and Engines specific data is required (EP4)", pivot.US_VNERequirementDesc);
			AssertEquals("VNE should be D", OGAIndicatorList.Codes.Declared, pivot.Details.CD_VNEIndicator);
			var oDStariff = Factory.New<USCTariff>();
			oDStariff.UE_Tariff = "1010101011";
			oDStariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			oDStariff.UE_DateTo = ZDateTime.Today;
			oDStariff.UE_PGACodes = "FD2EP2DT2";
			oDStariff.UE_OGACodes = "FD2EP2DT2";
			var classificationODS = Factory.New<CusClassification>();
			classificationODS.CC_LookupCode = "@#$34";
			classificationODS.CC_TariffNum = "1010101011";
			pivot.CI_CC = classificationODS.PK;
			AssertEquals("Ozone Depleting Substances specific data is required (EP2)", pivot.US_ODSRequirementDesc);
			AssertEquals("ODS should be D", OGAIndicatorList.Codes.Declared, pivot.Details.CD_ODSIndicator);
			var tSCAtariff = Factory.New<USCTariff>();
			tSCAtariff.UE_Tariff = "1010101012";
			tSCAtariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tSCAtariff.UE_DateTo = ZDateTime.Today;
			tSCAtariff.UE_PGACodes = "FD2EP8DT2";
			tSCAtariff.UE_OGACodes = "FD2EP8DT2";
			var classificationTSCA = Factory.New<CusClassification>();
			classificationTSCA.CC_LookupCode = "@#$34";
			classificationTSCA.CC_TariffNum = "1010101012";
			pivot.CI_CC = classificationTSCA.PK;
			AssertEquals("Toxic Substances Control Act specific data is required (EP8)", pivot.US_TSCARequirementDesc);
			AssertEquals("TSCA should be D", OGAIndicatorList.Codes.Declared, pivot.Details.CD_TSCAClaimIndicator);
			var pSTtariff = Factory.New<USCTariff>();
			pSTtariff.UE_Tariff = "1010101013";
			pSTtariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			pSTtariff.UE_DateTo = ZDateTime.Today;
			pSTtariff.UE_PGACodes = "FD2EP6DT2";
			pSTtariff.UE_OGACodes = "FD2EP6DT2";
			var classificationPST = Factory.New<CusClassification>();
			classificationPST.CC_LookupCode = "@#$34";
			classificationPST.CC_TariffNum = "1010101013";
			pivot.CI_CC = classificationPST.PK;
			AssertEquals("Pesticides specific data is required (EP6)", pivot.US_PSTRequirementDesc);
			AssertEquals("PST should be D", OGAIndicatorList.Codes.Declared, pivot.Details.CD_PSTIndicator);
		}

		public void TestOGAAgencyRequirements()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			AssertEquals("Should be 22 (Lacey, PGAFDA, NHTSA, ODS, TSCA, PST, HFC, VNE, ATF, TTB, CPSC, OMC, DEA, AMS, NOP, DDTC, FWS, APHIS, NMFS370, NMFSAMR, NMFSHMS, NMFSSIMP, NMFSCOA)", pivot.OGAAgencyRequirements.Count, 23);
			AssertEquals("Should be 7 (AMS, ATF, FWS, DEA, EPA, NMFS, TTB)", pivot.ExportPGAAgencyRequirements.Count, 7);
		}

		public void TestHasDDTCData()
		{
			CusClassPartPivot pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertEquals(false, pivot.HasImportDDTCData);
			pivot.Details.CD_DDTCIndicator = "D";
			AssertEquals(true, pivot.HasImportDDTCData);
		}

		public void TestDDTCIndicator()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1010101010";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;
			tariff.UE_PGACodes = DDTCLicenseTypeCodes.Codes.S73;
			tariff.UE_OGACodes = DDTCLicenseTypeCodes.Codes.S73;
			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			var details = pivot.Details;
			pivot.CI_TariffNum = "1000000001";
			details.CD_DDTCIndicator = "~";
			AssertHasMessageErrorContaining(details.CD_DDTCIndicatorInfo, ListValidation.InvalidCodeMessageError);
			details.CD_DDTCIndicator = "D";
			AssertNoMessageErrorContaining(details.CD_LaceyActIndicatorInfo, ListValidation.InvalidCodeMessageError);
			AssertEquals("DDTC should be D", OGAIndicatorList.Codes.Declared, pivot.Details.CD_DDTCIndicator);
		}

		public void TestAMSIndicatorOnCusClassPartPivot()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0805406000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;
			tariff.UE_PGACodes = "AM3AQ2FD4";
			tariff.UE_OGACodes = "AM3AQ2FD4";
			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			var details = pivot.Details;
			pivot.CI_TariffNum = "1000000001";
			details.CD_AMSIndicator = "~";
			AssertHasMessageErrorContaining(details.CD_AMSIndicatorInfo, ListValidation.InvalidCodeMessageError);
			details.CD_AMSIndicator = OGAIndicatorList.Codes.Declared;
			AssertNoMessageErrorContaining(details.CD_AMSIndicatorInfo, ListValidation.InvalidCodeMessageError);
			AssertEquals("AMS should be D", OGAIndicatorList.Codes.Declared, pivot.Details.CD_AMSIndicator);
			pivot.CD_AMSIndicator = OGAIndicatorList.Codes.Disclaimed;
			pivot.CD_AMSDisclaimProgram = "ABC";
			AssertEquals("ABC", pivot.Details.CD_AMSDisclaimProgram);
			pivot.CD_AMSIndicator = OGAIndicatorList.Codes.Declared;
			AssertEquals(ZString.Empty, pivot.Details.CD_AMSDisclaimProgram);
		}

		public void TestLaceyActIndicatorAndDesclaimedReason()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1010101010";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;
			tariff.UE_PGACodes = "FW1AL1";
			tariff.UE_OGACodes = "FW1AL1";
			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			var details = pivot.Details;
			pivot.CI_TariffNum = "1000000001";
			details.CD_LaceyActIndicator = "~";
			AssertHasMessageErrorContaining(details.CD_LaceyActIndicatorInfo, ListValidation.InvalidCodeMessageError);
			details.CD_LaceyActIndicator = "D";
			AssertNoMessageErrorContaining(details.CD_LaceyActIndicatorInfo, ListValidation.InvalidCodeMessageError);
			var errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGANotRequired, "Lacey Act", "Declared");
			tariff.UE_PGACodes = ZString.Empty;
			details.CD_LaceyActIndicator = OGAIndicatorList.Codes.Declared;
			AssertHasWarning(details.CD_LaceyActIndicatorInfo, errorText);
			details.CD_LaceyActDisclaimReason = ZString.Empty;
			details.CD_LaceyActIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertHasMessageError(details.CD_LaceyActDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
		}

		public void TestFWSIndDefaultingWithPFUNC()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;
			tariff.UE_PGACodes = "FW2";
			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			var details = pivot.Details;
			pivot.CI_TariffNum = "0000000000";
			AssertEquals("", details.CD_FWSIndicator);
			pivot.CI_TariffNum = "";
			using (ZZCustomsFunctionality.TemporarilySetupFWSEffective())
			{
				pivot.CI_TariffNum = "0000000000";
				AssertEquals("D", details.CD_FWSIndicator);
			}
		}

		public void TestPivotIndicatorWithPGAAgencyCode()
		{
			CusClassPartPivot pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_TariffNum = "1234567890";
			pivot.CI_ChildType = "HTI";
			var fpgaProvider = new ProductPGAgencyRequirementProvider(pivot);
			CheckIndicatorValue(GovernmentAgencyProgramCodeList.Codes.APHIS, fpgaProvider);
			CheckIndicatorValue(GovernmentAgencyProgramCodeList.Codes.AMS, fpgaProvider);
			CheckIndicatorValue(GovernmentAgencyProgramCodeList.Codes.NOP, fpgaProvider);
			CheckIndicatorValue(GovernmentAgencyProgramCodeList.Codes.FSIS, fpgaProvider);
			CheckIndicatorValue(GovernmentAgencyProgramCodeList.Codes.Lacey, fpgaProvider);
			CheckIndicatorValue(GovernmentAgencyProgramCodeList.Codes._370, fpgaProvider);
			CheckIndicatorValue(GovernmentAgencyProgramCodeList.Codes.AMR, fpgaProvider);
			CheckIndicatorValue(GovernmentAgencyProgramCodeList.Codes.DDTC, fpgaProvider);
			CheckIndicatorValue(GovernmentAgencyProgramCodeList.Codes.HMS, fpgaProvider);
			CheckIndicatorValue(GovernmentAgencyProgramCodeList.Codes.ODS, fpgaProvider);
			CheckIndicatorValue(GovernmentAgencyProgramCodeList.Codes.PST, fpgaProvider);
			CheckIndicatorValue(GovernmentAgencyProgramCodeList.Codes.HFC, fpgaProvider);
			CheckIndicatorValue(GovernmentAgencyProgramCodeList.Codes.VNE, fpgaProvider);
			CheckIndicatorValue(GovernmentAgencyProgramCodeList.Codes.TSCA, fpgaProvider);
			CheckIndicatorValue(GovernmentAgencyProgramCodeList.Codes.TTB, fpgaProvider);
			CheckIndicatorValue(GovernmentAgencyProgramCodeList.Codes.AMS, fpgaProvider);
			CheckIndicatorValue(GovernmentAgencyProgramCodeList.Codes.OMC, fpgaProvider);
			CheckIndicatorValue(GovernmentAgencyProgramCodeList.Codes.NHTSA, fpgaProvider);
			CheckIndicatorValue(GovernmentAgencyProgramCodeList.Codes.ATF, fpgaProvider);
			CheckIndicatorValue(GovernmentAgencyProgramCodeList.Codes.CPSC, fpgaProvider);
			CheckIndicatorValue(GovernmentAgencyProgramCodeList.Codes.DEA, fpgaProvider);
			CheckIndicatorValue(GovernmentAgencyProgramCodeList.Codes.FWS, fpgaProvider);
			CheckIndicatorValue(GovernmentAgencyProgramCodeList.Codes.HMS, fpgaProvider);
			CheckIndicatorValue(GovernmentAgencyProgramCodeList.Codes.AMR, fpgaProvider);
			CheckIndicatorValue(GovernmentAgencyProgramCodeList.Codes._370, fpgaProvider);
		}

		public void TestPivotDisclaimReasonWithPGAAgencyCode()
		{
			CusClassPartPivot pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_TariffNum = "1234567890";
			pivot.CI_ChildType = "HTI";
			DisclaimedReason(GovernmentAgencyProgramCodeList.Codes.APHIS, PGADisclaimReasonList.Codes.B, pivot);
			DisclaimedReason(GovernmentAgencyProgramCodeList.Codes.AMS, PGADisclaimReasonList.Codes.B, pivot);
			DisclaimedReason(GovernmentAgencyProgramCodeList.Codes.FDA, PGADisclaimReasonList.Codes.B, pivot);
			DisclaimedReason(GovernmentAgencyProgramCodeList.Codes.FSIS, PGADisclaimReasonList.Codes.B, pivot);
			DisclaimedReason(GovernmentAgencyProgramCodeList.Codes.ODS, PGADisclaimReasonList.Codes.B, pivot);
			DisclaimedReason(GovernmentAgencyProgramCodeList.Codes.PST, PGADisclaimReasonList.Codes.B, pivot);
			DisclaimedReason(GovernmentAgencyProgramCodeList.Codes.HFC, PGADisclaimReasonList.Codes.B, pivot);
			DisclaimedReason(GovernmentAgencyProgramCodeList.Codes.VNE, PGADisclaimReasonList.Codes.B, pivot);
			DisclaimedReason(GovernmentAgencyProgramCodeList.Codes.TSCA, PGADisclaimReasonList.Codes.B, pivot);
			DisclaimedReason(GovernmentAgencyProgramCodeList.Codes.AMS, PGADisclaimReasonList.Codes.B, pivot);
			DisclaimedReason(GovernmentAgencyProgramCodeList.Codes.OMC, PGADisclaimReasonList.Codes.A, pivot);
			DisclaimedReason(GovernmentAgencyProgramCodeList.Codes.NHTSA, PGADisclaimReasonList.Codes.A, pivot);
			DisclaimedReason(GovernmentAgencyProgramCodeList.Codes.Lacey, PGADisclaimReasonList.Codes.A, pivot);
			DisclaimedReason(GovernmentAgencyProgramCodeList.Codes.TTB, PGADisclaimReasonList.Codes.A, pivot);
			DisclaimedReason(GovernmentAgencyProgramCodeList.Codes.NOP, PGADisclaimReasonList.Codes.A, pivot);
			DisclaimedReason(GovernmentAgencyProgramCodeList.Codes.CPSC, PGADisclaimReasonList.Codes.A, pivot);
			DisclaimedReason(GovernmentAgencyProgramCodeList.Codes.DEA, PGADisclaimReasonList.Codes.A, pivot);
		}

		public void TestIndicatorValidated()
		{
			CusClassPartPivot pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_TariffNum = "1234567890";
			pivot.CI_ChildType = "HTI";
			CheckIndicatorValidateionWithPivot(GovernmentAgencyProgramCodeList.Codes.APHIS, pivot);
			CheckIndicatorValidateionWithPivot(GovernmentAgencyProgramCodeList.Codes.FDA, pivot);
			CheckIndicatorValidateionWithPivot(GovernmentAgencyProgramCodeList.Codes.VNE, pivot);
			CheckIndicatorValidateionWithPivot(GovernmentAgencyProgramCodeList.Codes._370, pivot);
			CheckIndicatorValidateionWithPivot(GovernmentAgencyProgramCodeList.Codes.NOP, pivot);
			CheckIndicatorValidateionWithPivot(GovernmentAgencyProgramCodeList.Codes.AMR, pivot);
			CheckIndicatorValidateionWithPivot(GovernmentAgencyProgramCodeList.Codes.DDTC, pivot);
			CheckIndicatorValidateionWithPivot(GovernmentAgencyProgramCodeList.Codes.HMS, pivot);
			CheckIndicatorValidateionWithPivot(GovernmentAgencyProgramCodeList.Codes.ODS, pivot);
			CheckIndicatorValidateionWithPivot(GovernmentAgencyProgramCodeList.Codes.FSIS, pivot);
			CheckIndicatorValidateionWithPivot(GovernmentAgencyProgramCodeList.Codes.PST, pivot);
			CheckIndicatorValidateionWithPivot(GovernmentAgencyProgramCodeList.Codes.HFC, pivot);
			CheckIndicatorValidateionWithPivot(GovernmentAgencyProgramCodeList.Codes.Lacey, pivot);
			CheckIndicatorValidateionWithPivot(GovernmentAgencyProgramCodeList.Codes.AMS, pivot);
			CheckIndicatorValidateionWithPivot(GovernmentAgencyProgramCodeList.Codes.TSCA, pivot);
			CheckIndicatorValidateionWithPivot(GovernmentAgencyProgramCodeList.Codes.TTB, pivot);
			CheckIndicatorValidateionWithPivot(GovernmentAgencyProgramCodeList.Codes.NHTSA, pivot);
			CheckIndicatorValidateionWithPivot(GovernmentAgencyProgramCodeList.Codes.OMC, pivot);
			CheckIndicatorValidateionWithPivot(GovernmentAgencyProgramCodeList.Codes.ATF, pivot);
			CheckIndicatorValidateionWithPivot(GovernmentAgencyProgramCodeList.Codes.CPSC, pivot);
			CheckIndicatorValidateionWithPivot(GovernmentAgencyProgramCodeList.Codes.DEA, pivot);
			CheckIndicatorValidateionWithPivot(GovernmentAgencyProgramCodeList.Codes.AMR, pivot);
			CheckIndicatorValidateionWithPivot(GovernmentAgencyProgramCodeList.Codes.HMS, pivot);
			CheckIndicatorValidateionWithPivot(GovernmentAgencyProgramCodeList.Codes.FWS, pivot);
			CheckIndicatorValidateionWithPivot(GovernmentAgencyProgramCodeList.Codes.COA, pivot);
		}

		public void TestDeactivatingProductWithUSClassification()
		{
			var factory1 = new BusinessObjectFactory();
			var org1 = factory1.New<OrgHeader>();
			org1.OH_Code = "H!1";
			org1.OH_FullName = "BOB";
			org1.OH_IsConsignee = true;
			var org2 = factory1.New<OrgHeader>();
			org2.OH_Code = "H!2";
			org2.OH_FullName = "JACK";
			org2.OH_IsConsignor = true;
			var part = factory1.New<OrgSupplierPart>();
			part.OP_PartNum = "$#233";
			var relOrg1 = part.RelatedOrganisations.AddOrganisationIfNotExist(org1.PK, OrgPartRelation.RelationshipTypes.Owner);
			var relOrg2 = part.RelatedOrganisations.AddOrganisationIfNotExist(org2.PK, OrgPartRelation.RelationshipTypes.Supplier);
			var classification = factory1.New<CusClassification>();
			classification.CC_LookupCode = "@#$34";
			classification.CC_TariffNum = "1010101010";
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			var pivot1 = part.PivotsForBinding.AddNew();
			pivot1.CI_UsageComment = "U1";
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot1.CI_OH = relOrg1.OU_OH;
			pivot1.CI_TariffNum = "1010101010";
			pivot1.CI_CC = classification.PK;
			pivot1.CD_LicenceNo = "13";
			factory1.Save();
			var factory2 = new BusinessObjectFactory();
			var declaration = factory2.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Buyer = org1.PK;
			invoice.JZ_OH_Supplier = org2.PK;
			invoice.JZ_InvoiceNumber = "INV234";
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "$#233";
			AssertEquals("invoiceLine.JI_OP", part.PK, invoiceLine.JI_OP);
			factory2.Save();
			bool listChangedWasCalled = false;
			try
			{
				((IBindingList)part.PivotsForBinding).ListChanged += new ListChangedEventHandler((sender, e) =>
				{
					var innerInfo = ((ZWrappedPropertyInfo)pivot1.CD_LicenceNoInfo).InnerInfo;
					listChangedWasCalled = true;
				});
				part.Delete();
				factory1.Save();
			}
			catch (ZSaveException)
			{
				((IBusinessObjectFactoryInternals)factory1).Rollback();
			}
			finally
			{
				AssertEquals("listChangedWasCalled should be set to true on rollback", true, listChangedWasCalled);
			}
		}

		[ExpectNoExceptions]
		public void Test2CusUSClassificationsAreNotCreatedForTheSamePivot()
		{
			var lookup = Factory.NewWithValidTestData<CusClassification>();
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "PartNum";
			var pivot = Factory.New<CusClassPartPivot>();
			var row = (IColumnIndexer)((IBusinessObjectInternals)pivot).Row;
			row[CusClassPartPivot.Schema.CI_CC] = lookup.PK;
			row[CusClassPartPivot.Schema.CI_OP] = product.PK;
			pivot.CD_9802USDValuePerUnit = 1m;
			Factory.Save();
			var anotherFactory = new BusinessObjectFactory();
			var pivotInAnotherFactory = anotherFactory.Load<CusClassPartPivot>(pivot.PK);
			pivotInAnotherFactory.CD_9802USDValuePerUnit = 1m;
			Factory.Save();
			anotherFactory.Save();
		}

		public void TestCloningPartSetCI_OPCorrectlyOnChildPivots_CS00296407()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "SDF@#$";
			org.OH_FullName = "BOB";
			org.MainAddress.OA_Address1 = "ADD 1";
			org.OH_IsConsignee = true;
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "DSDR21";
			product.OP_Desc = "HELLO BOB";
			var relatedOrg = product.RelatedOrganisations.AddOwner(org);
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "1010101010";
			var childPivot = pivot.Children.AddNew();
			childPivot.CI_TariffNum = "2020202020";
			var childPivotChild = childPivot.Children.AddNew();
			childPivotChild.CI_TariffNum = "3030303030";
			product.RunPreSaveValidation();
			AssertNoErrors(product);
			Factory.Save();
			var clonedProduct = (OrgSupplierPart)((ITemplateCopyable)product).TemplateCopy();
			AssertEquals(1, clonedProduct.PivotsForBinding.Count);
			var clonedPivot = clonedProduct.PivotsForBinding[0];
			AssertPivot(clonedPivot, "1010101010", clonedProduct.PK, ZByte.Zero);
			AssertEquals(1, clonedPivot.Children.Count);
			var clonedChildPivot = clonedPivot.Children[0];
			AssertPivot(clonedChildPivot, "2020202020", clonedProduct.PK, 1);
			AssertEquals(1, clonedChildPivot.Children.Count);
			var clonedChildPivotChild = clonedChildPivot.Children[0];
			AssertPivot(clonedChildPivotChild, "3030303030", clonedProduct.PK, 1);
		}

		public void TestWrapperProperties()
		{
			var lookup = Factory.NewWithValidTestData<CusClassification>();
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "PartNum";
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_CC = lookup.PK;
			pivot.CI_OP = product.PK;
			pivot.CD_9802USDValuePerUnit = 1m;
			pivot.CD_9802ValuePerUnit = 2m;
			pivot.CD_ADCVDStat = ADDCVDNonReimbursementList.Codes.Declared;
			pivot.CD_ADDApplicable = true;
			pivot.CD_ADDCaseNo = "ADDCaseNo";
			pivot.CD_ADDDecID = "ADDDecID";
			pivot.CD_ADDDepositRateInd = DepositRateIndicatorList.Codes.AdValorem;
			pivot.CD_OriginIndicator = "Y";
			pivot.CD_AgricultureLicenceNo = "Agri";
			pivot.CD_AMMVPerUnit = 3m;
			pivot.CD_AMMVPerUnitCurrency = "CAD";
			pivot.CD_SugarCertificate = "CACert";
			pivot.CD_CBTPACertificate = "1";
			pivot.CD_CottonCertificate = "Cotton";
			pivot.CD_CottonFeeExempt = "N";
			pivot.CD_CVDApplicable = true;
			pivot.CD_CVDCaseNo = "CVDCaseNo";
			pivot.CD_CVDDecID = "CVDDecID";
			pivot.CD_CVDDepositRateInd = DepositRateIndicatorList.Codes.AdValorem;
			pivot.CD_ITARExemptionNo = "123.12";
			pivot.CD_MilitaryEquipInd = "Y";
			pivot.CD_PartyCertInd = "Y";
			pivot.CD_DDTCRegoNo = "RegNo";
			pivot.CD_DDTCUnit = "BAG";
			pivot.CD_DDTCUSMLCategoryCode = "08";
			pivot.CD_ECCN = "ECCN";
			pivot.CD_ExportCode = "CH";
			pivot.CD_ADDBonded = true;
			pivot.CD_CVDBonded = true;
			pivot.CD_NAFTANetCost = true;
			pivot.CD_LicenceType = "C30";
			pivot.CD_MiscLicenceNo = "MiscPer";
			pivot.CD_NAFTARecon = true;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			pivot.CD_OA_Manufacturer = org.MainAddress.PK;
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			pivot.CD_OA_Exporter = org2.MainAddress.PK;
			pivot.CD_ReconIssue = "98";
			pivot.CD_ActiveIngredientPercentage = 4m;
			pivot.CD_PerUnitCost = 5m;
			pivot.CD_RulingNumber = "RulNo";
			pivot.CD_RulingType = "R";
			pivot.CD_RX_NK9802ValuePerUnitCurr = "USD";
			pivot.CD_RX_NKPerUnitCostCurr = "CAD";
			pivot.CD_ProductClaim = "G";
			pivot.CD_SPI = "B#";
			pivot.CD_TaxApplicability = "O";
			pivot.CD_TaxCode = "018";
			pivot.CD_TaxRate = 1m;
			pivot.CD_TaxRateType = "R";
			pivot.CD_TaxRateDesc = "RateS";
			pivot.CD_TSCAIndicator = "-";
			pivot.CD_UC_NKCountryOfExport = "US";
			pivot.CD_UC_NKCountryOfOrigin = "AU";
			pivot.CD_GrossWeight = 6m;
			pivot.CD_NetWeight = 7m;
			pivot.CD_WeightUQ = "DT";
			pivot.CD_WoolLicenceNo = "WoolNo";
			pivot.CD_ZoneStatus = "D";
			pivot.CD_PrimaryCountryNA = true;
			pivot.CD_RN_NKPrimaryCountry = "RU";
			pivot.CD_SecondaryCountryNA = true;
			pivot.CD_RN_NKSecondaryCountry = "CN";
			pivot.CD_RN_NKCastCountry = "SG";
			pivot.CD_RN_NKCertificateOrigin = "SG";
			pivot.CD_RN_NKMeltCountry = "SG";
			Factory.Save();
			AssertEquals("No fields are saved in CI_AddInfo", ZString.Empty, pivot.CI_AddInfo);
			var newFactory = new BusinessObjectFactory();
			var usClassification = newFactory.LoadTop1<CusUSClassification>(new ZQuery(CusUSClassificationSchema.CD_ParentID, pivot.PK));
			AssertNotNull(usClassification);
			AssertEquals(1m, usClassification.CD_9802USDValuePerUnit);
			AssertEquals(2m, usClassification.CD_9802ValuePerUnit);
			AssertEquals("D", usClassification.CD_ADCVDStat);
			Assert(usClassification.CD_ADDApplicable);
			AssertEquals("ADDCaseNo", usClassification.CD_ADDCaseNo);
			AssertEquals("ADDDecID", usClassification.CD_ADDDecID);
			AssertEquals(DepositRateIndicatorList.Codes.AdValorem, usClassification.CD_ADDDepositRateInd);
			AssertEquals("Y", usClassification.CD_OriginIndicator);
			AssertEquals("Agri", usClassification.CD_AgricultureLicenceNo);
			AssertEquals(3m, usClassification.CD_AMMVPerUnit);
			AssertEquals("CAD", usClassification.CD_AMMVPerUnitCurrency);
			AssertEquals("CACert", usClassification.CD_SugarCertificate);
			AssertEquals("1", usClassification.CD_CBTPACertificate);
			AssertEquals("Cotton", usClassification.CD_CottonCertificate);
			AssertEquals("N", usClassification.CD_CottonFeeExempt);
			Assert(usClassification.CD_CVDApplicable);
			AssertEquals("CVDCaseNo", usClassification.CD_CVDCaseNo);
			AssertEquals("CVDDecID", usClassification.CD_CVDDecID);
			AssertEquals(DepositRateIndicatorList.Codes.AdValorem, usClassification.CD_CVDDepositRateInd);
			Assert(usClassification.CD_ADDBonded);
			Assert(usClassification.CD_CVDBonded);
			AssertEquals("C30", usClassification.CD_LicenceType);
			AssertEquals("MiscPer", usClassification.CD_MiscLicenceNo);
			Assert(usClassification.CD_NAFTARecon);
			AssertEquals(org.MainAddress.PK, usClassification.CD_OA_Manufacturer);
			AssertEquals(org2.MainAddress.PK, usClassification.CD_OA_Exporter);
			AssertEquals("98", usClassification.CD_ReconIssue);
			AssertEquals(4m, usClassification.CD_ActiveIngredientPercentage);
			AssertEquals(5m, usClassification.CD_PerUnitCost);
			AssertEquals("RulNo", usClassification.CD_RulingNumber);
			AssertEquals("R", usClassification.CD_RulingType);
			AssertEquals("USD", usClassification.CD_RX_NK9802ValuePerUnitCurr);
			AssertEquals("CAD", usClassification.CD_RX_NKPerUnitCostCurr);
			AssertEquals("G", usClassification.CD_ProductClaim);
			AssertEquals("B#", usClassification.CD_SPI);
			AssertEquals("O", usClassification.CD_TaxApplicability);
			AssertEquals("018", usClassification.CD_TaxCode);
			AssertEquals("RateS", usClassification.CD_TaxRateDesc);
			AssertEquals("R", usClassification.CD_TaxRateType);
			AssertEquals("-", usClassification.CD_TSCAIndicator);
			AssertEquals("US", usClassification.CD_UC_NKCountryOfExport);
			AssertEquals("AU", usClassification.CD_UC_NKCountryOfOrigin);
			AssertEquals(6m, usClassification.CD_GrossWeight);
			AssertEquals(7m, usClassification.CD_NetWeight);
			AssertEquals("DT", usClassification.CD_WeightUQ);
			AssertEquals("WoolNo", usClassification.CD_WoolLicenceNo);
			AssertEquals("D", usClassification.CD_ZoneStatus);
			AssertEquals("Y", usClassification.CD_PartyCertInd);
			AssertEquals(true, usClassification.CD_PrimaryCountryNA);
			AssertEquals("RU", usClassification.CD_RN_NKPrimaryCountry);
			AssertEquals(true, usClassification.CD_SecondaryCountryNA);
			AssertEquals("CN", usClassification.CD_RN_NKSecondaryCountry);
			AssertEquals("SG", usClassification.CD_RN_NKCastCountry);
			AssertEquals("SG", usClassification.CD_RN_NKCertificateOrigin);
			AssertEquals("SG", usClassification.CD_RN_NKMeltCountry);
		}

		public void TestTariffNumbersIncludingComponents()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_TariffNum = "9101000010";
			AssertEquals("9101000010", pivot.TariffNumbersIncludingComponents);
			pivot.CI_SupplementalTariff = "9801001010";
			AssertEquals("9101000010(9801001010)", pivot.TariffNumbersIncludingComponents);
			var child1 = pivot.Children.AddNew();
			child1.CI_TariffNum = "9101000020";
			var child2 = pivot.Children.AddNew();
			child2.CI_TariffNum = "9101000030";
			var child3 = pivot.Children.AddNew();
			child3.CI_TariffNum = "9101000040";
			AssertEquals("9101000010(9801001010)/9101000020/9101000030/9101000040", pivot.TariffNumbersIncludingComponents);
			var child4 = pivot.Children.AddNew();
			child4.CI_TariffNum = "9101000050";
			AssertEquals("9101000010(9801001010)/9101000020/9101000030/9101000040/9101000050", pivot.TariffNumbersIncludingComponents);
			var child5 = pivot.Children.AddNew();
			child5.CI_TariffNum = "9101000060";
			AssertEquals("9101000010(9801001010)/9101000020/9101000030/9101000040/9101000050/...", pivot.TariffNumbersIncludingComponents);
		}

		public void TestLogWhenTariffNumbersChanged()
		{
			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			Pivot.CI_OP = part.PK;
			Pivot.CI_TariffNum = "1234567890";
			Pivot.CI_TariffNum = "1234566666";
			AssertContainsLog(Pivot.Logs, "Tariff '1234.56.7890' changed to '1234.56.6666'", true);
			AssertContainsLog(part.Logs, "Tariff '1234.56.7890' changed to '1234.56.6666'", false);
		}

		public void TestFormattedTariff()
		{
			ZString tariff1 = "1234567890";
			ZString tariff2 = "9.6.4 32 10";
			CusClassPartPivot pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_TariffNum = tariff1;
			pivot.CI_SupplementalTariff = tariff2;
			AssertEquals("CI_FormattedTariffNum", "1234.56.7890", pivot.CI_FormattedTariffNum);
			AssertEquals("CI_FormattedSupplementalTariff", "9643210", pivot.CI_SupplementalTariff);
			AssertEquals("CI_FormattedSupplementalTariff", "9643.21.0", pivot.CI_FormattedSupplementalTariff);
			pivot.CI_FormattedTariffNum = tariff2;
			pivot.CI_SupplementalTariff = tariff1;
			AssertEquals("CI_FormattedTariffNum", "9643.21.0", pivot.CI_FormattedTariffNum);
			AssertEquals("CI_FormattedSupplementalTariff", "1234.56.7890", pivot.CI_FormattedSupplementalTariff);
		}

		public void TestCI_SupplementalTariffOnlyNumbersWhenSaving()
		{
			var pivot2 = Factory.NewWithValidTestData<BaseCusClassPartPivot>();
			pivot2.CI_SupplementalTariff = "5555.2222 77";
			AssertEquals("Formatted supplementTariff", "5555.22.2277", pivot2.CI_FormattedSupplementalTariff);
			AssertEquals("SupplementTariff without . and space", "5555222277", pivot2.CI_SupplementalTariff);
			Factory.Save();
			var pivotLoaded2 = Factory.Load<BaseCusClassPartPivot>(pivot2.PK);
			AssertEquals("Saved", "5555222277", ((IBusinessObjectInternals)pivotLoaded2).Row[BaseCusClassPartPivot.Schema.CI_SupplementalTariff]);
			AssertEquals("SupplementTariff", "5555222277", pivotLoaded2.CI_SupplementalTariff);
			AssertEquals("Formatted supplementTariff", "5555.22.2277", pivotLoaded2.CI_FormattedSupplementalTariff);
		}

		public void TestTariffDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusTariffTypeSB = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.ScheduleB);
			Factory.Save();
			var pivot = Factory.New<CusClassPartPivot>();
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "PART434";
			pivot.CI_OP = product.PK;
			pivot.CI_TariffNum = "1234567890";
			pivot.CI_ChildType = "HTI";
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1234567890";
			tariff.UE_ShortDescription = "Test Description";
			tariff.UE_DateFrom = ZDateTime.Today.AddDays(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);
			AssertEquals("TariffDescription", "Test Description", pivot.TariffDescription);
			var scheduleB = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, cusTariffTypeSB.PK, "1234567890", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), isSystem: true, ensureDataGroupingExists: true);
			scheduleB.ZZ1_Description = "Test Description 2";
			pivot.CI_ChildType = "SHB";
			AssertEquals("TariffDescription", "Test Description 2", pivot.TariffDescription);
		}

		public void TestHasNMFSLine()
		{
			CusClassPartPivot pivot = Factory.New<CusClassPartPivot>();
			AssertEquals(false, pivot.HasNMFSLines);
			pivot.NMFSLines.AddNew();
			AssertEquals(true, pivot.HasNMFSLines);
		}

		public void TestFWSLine()
		{
			CusClassPartPivot pivot = Factory.New<CusClassPartPivot>();
			AssertEquals(false, pivot.HasFWSLines);
			pivot.FWSLines.AddNew();
			AssertEquals(true, pivot.HasFWSLines);
		}

		public void TestHasOMCData()
		{
			CusClassPartPivot pivot = Factory.New<CusClassPartPivot>();
			AssertEquals(false, pivot.HasOMCHeaders);
			pivot.OMCHeaders.AddNew();
			AssertEquals(true, pivot.HasOMCHeaders);
		}

		public void TestHasAMSData()
		{
			CusClassPartPivot pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertEquals(false, pivot.HasAMSData);
			pivot.AMSLines.AddNew();
			AssertEquals(true, pivot.HasAMSData);
		}

		public void TestHasCPSCData()
		{
			CusClassPartPivot pivot = Factory.New<CusClassPartPivot>();
			AssertEquals(false, pivot.HasCPSCLines);
			pivot.CPSCLines.AddNew();
			AssertEquals(true, pivot.HasCPSCLines);
		}

		public void TestHasLaceyActData()
		{
			CusClassPartPivot pivot = Factory.New<CusClassPartPivot>();
			AssertEquals(false, pivot.HasLaceyActData);
			pivot.PGAs.AddNew();
			AssertEquals(true, pivot.HasLaceyActData);
		}

		public void TestHasDEAData()
		{
			CusClassPartPivot pivot = Factory.New<CusClassPartPivot>();
			AssertEquals(false, pivot.HasDEAHeaders);
			pivot.DEAHeaders.AddNew();
			AssertEquals(true, pivot.HasDEAHeaders);
		}

		public void TestTariffSupplementTariff()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1010101010";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;
			tariff.UE_PGACodes = "FD2EP4AL2";
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			var classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "@#$34";
			classification.CC_TariffNum = "1010101010";
			pivot.CI_CC = classification.PK;
			ZString tariffNumber = "1010101010";
			ZString supTariffNumber = "1010101010";
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "PART434";
			pivot.CI_OP = product.PK;
			pivot.CI_TariffNum = tariffNumber;
			pivot.CI_SupplementalTariff = supTariffNumber;
			var pivottariff = pivot.PGARequirementIndicator;
			AssertEquals(false, pivottariff.RequireFDA);
			AssertEquals(false, pivottariff.RequireDOT);
			AssertEquals(false, pivottariff.RequireACE_LaceyData);
			tariff.UE_PGACodes = "FD2EP4AL1";
			AssertEquals(false, pivottariff.RequireACE_LaceyData);
			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			pivot.CI_OP = part.PK;
			Pivot.CI_SupplementalTariff = "1234567890";
			Pivot.CI_SupplementalTariff = "1234566666";
			AssertContainsLog(Pivot.Logs, "Prov/Prog. Tariff '1234.56.7890' changed to '1234.56.6666'", true);
		}

		[TestDate(2006, 12, 12)]
		public void TestGetAdditionalDataForBorderWise()
		{
			CusClassPartPivot pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			IHaveAdditionalDataForBorderWise pivotForBorderWise = pivot;
			AdditionalDataForBorderWise additionalData = pivotForBorderWise.GetAdditionalDataForBorderWise("");
			AssertEquals("AdditionalData.ParameterForBorderWise", "I", additionalData.ParameterForBorderWise);
			AssertEquals("AdditionalData.DateForDutyRate", ZDateTime.Now, additionalData.DateForDutyRate);
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertEquals("AdditionalData.ParameterForBorderWise", "I", additionalData.ParameterForBorderWise);
			AssertEquals("AdditionalData.DateForDutyRate", ZDateTime.Now, additionalData.DateForDutyRate);
			pivot.CI_ChildType = ClassificationTypeList.Codes.SHB;
			additionalData = pivotForBorderWise.GetAdditionalDataForBorderWise("");
			AssertEquals("AdditionalData.ParameterForBorderWise", "E", additionalData.ParameterForBorderWise);
			AssertEquals("AdditionalData.DateForDutyRate", ZDateTime.Now, additionalData.DateForDutyRate);
		}

		public void TestReValidateSameTypePivotsOnPivotDelete()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.FillWithValidTestData();
			var product = Factory.New<OrgSupplierPart>();
			product.RelatedOrganisations.AddOwner(org);
			product.OP_PartNum = "DZZA";
			var pivot1 = product.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertHasErrors(pivot2.CI_ChildTypeInfo);
			pivot1.Delete();
			AssertNoErrors(pivot2.CI_ChildTypeInfo);
		}

		public void TestLookups()
		{
			CusClassPartPivot pivot = Factory.New<CusClassPartPivot>();
			AssertEquals(typeof(CusClassPartPivotLookups), pivot.Lookups.GetType());
		}

		public void TestValidation()
		{
			CusClassPartPivot pivot = Factory.New<CusClassPartPivot>();
			AssertEquals(typeof(CusClassPartPivotValidation), pivot.Validation.GetType());
		}

		public void TestCottonCertificate()
		{
			CusClassPartPivot pivot = Factory.New<CusClassPartPivot>();
			AssertEquals(false, pivot.CD_CottonCertificateInfo.ReadOnly);
			pivot.CD_CottonCertificate = "CO234";
			pivot.CD_CottonFeeExempt = YesNoDefaultList.Codes.No;
			AssertEquals(false, pivot.CD_CottonCertificateInfo.ReadOnly);
			AssertEquals("CO234", pivot.CD_CottonCertificate);
			pivot.CD_CottonFeeExempt = YesNoDefaultList.Codes.Yes;
			AssertEquals(true, pivot.CD_CottonCertificateInfo.ReadOnly);
			AssertEquals("", pivot.CD_CottonCertificate);
		}

		public void TestCI_ChildTypeDescription()
		{
			CusClassPartPivot pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertEquals(ClassificationTypeList.Descriptions.HTI, pivot.CI_ChildTypeDescription);
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertEquals(ClassificationTypeList.Descriptions.HTE, pivot.CI_ChildTypeDescription);
		}

		public void TestCI_CI_Parent_ReadOnly()
		{
			CusClassPartPivot pivot = Factory.New<CusClassPartPivot>();
			AssertEquals(true, pivot.CI_CI_ParentInfo.ReadOnly);
		}

		public void TestTariffNumReadOnly()
		{
			CusClassPartPivot pivot = Factory.New<CusClassPartPivot>();
			AssertEquals(false, pivot.CI_TariffNumInfo.ReadOnly);
			AssertEquals(false, pivot.CI_FormattedTariffNumInfo.ReadOnly);
			pivot.CI_CC = Factory.New<CusClassification>().PK;
			AssertEquals(true, pivot.CI_TariffNumInfo.ReadOnly);
			AssertEquals(true, pivot.CI_FormattedTariffNumInfo.ReadOnly);
		}

		public void TestSupplementalTariffReadOnly()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			var childLine = pivot.Children.AddNew();
			AssertEquals(true, pivot.CI_FormattedSupplementalTariffInfo.ReadOnly);
			AssertEquals(true, childLine.CI_FormattedSupplementalTariffInfo.ReadOnly);
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertEquals(false, pivot.CI_FormattedSupplementalTariffInfo.ReadOnly);
			AssertEquals(false, childLine.CI_FormattedSupplementalTariffInfo.ReadOnly);
			pivot.CI_ChildType = ClassificationTypeList.Codes.SHB;
			AssertEquals(true, pivot.CI_FormattedSupplementalTariffInfo.ReadOnly);
			AssertEquals(true, childLine.CI_FormattedSupplementalTariffInfo.ReadOnly);
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertEquals(true, pivot.CI_FormattedSupplementalTariffInfo.ReadOnly);
			AssertEquals(true, childLine.CI_FormattedSupplementalTariffInfo.ReadOnly);
			pivot.CI_ChildType = "";
			AssertEquals(true, pivot.CI_FormattedSupplementalTariffInfo.ReadOnly);
			AssertEquals(true, childLine.CI_FormattedSupplementalTariffInfo.ReadOnly);
		}

		public void TestCI_SupplementalTariff()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_SupplementalTariff = "9802000000";
			AssertEquals("9802.00.0000", pivot.CI_FormattedSupplementalTariff);
			var childLine = pivot.Children.AddNew();
			AssertEquals("", childLine.CI_SupplementalTariff);
			AssertEquals("", childLine.CI_FormattedSupplementalTariff);
			childLine.CI_SupplementalTariff = "9802000000";
			AssertEquals("9802000000", childLine.CI_SupplementalTariff);
			AssertEquals("9802.00.0000", childLine.CI_FormattedSupplementalTariff);
			childLine.CI_SupplementalTariff = "9802000001";
			AssertEquals("9802000001", childLine.CI_SupplementalTariff);
			AssertEquals("9802.00.0001", childLine.CI_FormattedSupplementalTariff);
		}

		public void TestCI_CC_ReadOnly()
		{
			CusClassPartPivot pivot = Factory.New<CusClassPartPivot>();
			AssertEquals(false, pivot.CI_CCInfo.ReadOnly);
			pivot.CI_TariffNum = "1010.10.1000";
			AssertEquals(true, pivot.CI_CCInfo.ReadOnly);
		}

		public void TestUseHTSClassification()
		{
			CusClassPartPivot pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertEquals(true, pivot.UseHTSClassification);
			pivot.CI_ChildType = ClassificationTypeList.Codes.SHB;
			AssertEquals(false, pivot.UseHTSClassification);
		}

		public void TestUseSCHBClassification()
		{
			CusClassPartPivot pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.SHB;
			AssertEquals(true, pivot.UseSCHBClassification);
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertEquals(false, pivot.UseSCHBClassification);
		}

		public void TestIsExportClassification()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertEquals(false, pivot.IsExportClassification);
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertEquals(true, pivot.IsExportClassification);
			pivot.CI_ChildType = ClassificationTypeList.Codes.SHB;
			AssertEquals(true, pivot.IsExportClassification);
		}

		public void TestChangeOfValueIsLoggedAndClearsAuditEntry()
		{
			var pivot = (BaseCusClassPartPivot)GetNewBusinessObject();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertChangeOfValueIsLoggedAndClearsAuditEntry(pivot, piv => piv.CI_ChildType = ClassificationTypeList.Codes.HTE);
			AssertChangeOfValueIsLoggedAndClearsAuditEntry(pivot, piv => piv.CI_ChildType = ClassificationTypeList.Codes.SHB);
		}

		public void TestChangeOfValueInDetailsIsLoggedAndClearsAuditEntry()
		{
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_OP = part.PK;
			pivot.CI_LastAuditedUser = "USR";
			pivot.CI_LastAuditedDate = ZDateTime.BrettsBirthday;
			Factory.Save();
			AssertEquals("USR", pivot.CI_LastAuditedUser);
			AssertEquals(ZDateTime.BrettsBirthday, pivot.CI_LastAuditedDate);
			AssertEquals(false, pivot.Details.HasChanges);
			var timeOfChange = ZDateTime.UtcNow;
			pivot.CD_ECCN = "XXX";
			AssertEquals(true, pivot.Details.HasChanges);
			AssertEquals("", pivot.CI_LastAuditedUser);
			AssertEquals(ZDateTime.Empty, pivot.CI_LastAuditedDate);
			Factory.Save();
			var logEntry = pivot.Logs.MostRecentLog;
			AssertEquals(Events.EditedARecord, logEntry.Event);
			AssertLessThanOrEqualTo(timeOfChange, logEntry.SL_PostedTimeUtc);
		}

		public void TestICusAddInfoTypeSupporter()
		{
			var lookup = Factory.New<CusClassification>();
			lookup.CC_LookupCode = "LOOK434";
			lookup.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "PART434";
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_CC = lookup.PK;
			pivot.CI_OP = product.PK;
			ICusAddInfoTypeSupporter supporter = pivot;
			Type type = null;
			supporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.USPGACommon, out type);
			AssertEquals(typeof(PGA), type);
			type = null;
			supporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.USATF, out type);
			AssertEquals(typeof(ATF), type);
			type = null;
			supporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.USNMFSLine, out type);
			AssertEquals(typeof(NMFSLine), type);
			type = null;
			supporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.USAPHISHeader, out type);
			AssertEquals(typeof(APHISHeader), type);
			type = null;
			supporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.USPGAVehicle, out type);
			AssertEquals(typeof(Vehicle), type);
			type = null;
			supporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.USOMCHeader, out type);
			AssertEquals(typeof(OMCHeader), type);
			type = null;
			supporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.USFWSHeader, out type);
			AssertEquals(typeof(FWSHeader), type);
			type = null;
			supporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.USAMS, out type);
			AssertEquals(typeof(AMS), type);
			type = null;
			supporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.USPesticide, out type);
			AssertEquals(typeof(Pesticide), type);
			type = null;
			supporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.USCPSCHeader, out type);
			AssertEquals(typeof(CPSCHeader), type);
			type = null;
			supporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.USDEAHeader, out type);
			AssertEquals(typeof(DEAHeader), type);
			type = null;
			supporter.GetCusAddInfoTypes().TryGetValue("ZZ!", out type);
			AssertNull(type);
			var pga = pivot.PGAs.AddNew();
			pga.US_InvCurrPGAValue = 1m;
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			CusAddInfo addInfo = newFactory.Load<CusAddInfo>(pga.PK);
			AssertEquals(typeof(PGA), addInfo.GetType());
		}

		public void TestICusCodeDataTypeSupporter()
		{
			var lookup = Factory.New<CusClassification>();
			lookup.CC_LookupCode = "LOOK434";
			lookup.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "PART434";
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_CC = lookup.PK;
			pivot.CI_OP = product.PK;
			ICusCodeDataTypeSupporter supporter = pivot;
			Type type = null;
			supporter.GetCusCodeDataTypes().TryGetValue(CusCodeDataTypeList.Codes.CensusWarningOverride, out type);
			AssertEquals(typeof(CensusWarningOverride), type);
			type = null;
			supporter.GetCusCodeDataTypes().TryGetValue("ZZ!", out type);
			AssertNull(type);
			var warning = pivot.CensusWarningOverrides.AddNew();
			warning.CY_Data = "!";
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var codeData = newFactory.Load<CusCodeData>(warning.PK);
			AssertEquals(typeof(CensusWarningOverride), codeData.GetType());
		}

		public void TestFetchHintIsCorrectlyUse()
		{
			CusClassification lookup = Factory.New<CusClassification>();
			lookup.CC_LookupCode = "LOOK434";
			lookup.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			OrgSupplierPart product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "PART434";
			CusClassPartPivot pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_CC = lookup.PK;
			pivot.CI_OP = product.PK;
			PGA pga = pivot.PGAs.AddNew();
			pga.US_PGALineValue = 1m;
			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			newFactory.AllowMultipleBusinessObjectsAroundOneRow = false;
			CusClassPartPivot pivotLoaded = newFactory.Load<CusClassPartPivot>(pivot.PK);
			AssertEquals(1, pivotLoaded.PGAs.Count);
		}

		public void TestFetchForFactorySave()
		{
			AssertEquals(0, Factory.ActiveFetchHintsForTable(CusClassPartPivotSchema.Constants.TableName));
			CusClassification lookup = Factory.New<CusClassification>();
			lookup.CC_LookupCode = "LOOK434";
			lookup.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			OrgSupplierPart product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "PART434";
			CusClassPartPivot pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_CC = lookup.PK;
			pivot.CI_OP = product.PK;
			OrgSupplierPart product1 = Factory.New<OrgSupplierPart>();
			product1.OP_PartNum = "PART2";
			CusClassPartPivot pivot2 = Factory.New<CusClassPartPivot>();
			pivot2.CI_TariffNum = "7306191010";
			pivot2.CI_OP = product1.PK;
			CusClassPartPivot child1 = pivot2.Children.AddNew();
			child1.CI_ChildType = ClassificationChildTypeList.Codes.Related;
			child1.CI_TariffNum = "2922292700";
			Factory.Save();
			AssertEquals(2, Factory.ActiveFetchHintsForTable(CusClassPartPivotSchema.Constants.TableName));
		}

		public void TestCalcMiscLicenseTypeLabel()
		{
			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			CusClassPartPivot importPivot = part.PivotsForBinding.AddNew();
			importPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertEquals("Default should show when no classifcation/tariff entered", "Misc. License No.:", importPivot.CalcMiscLicenseTypeLabel);
			CusClassification classification = Factory.New<CusClassification>();
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			classification.CC_TariffNum = "2922292700";
			importPivot.CI_CC = classification.PK;
			AssertEquals("Misc. License No.:", importPivot.CalcMiscLicenseTypeLabel);

			var uscTariff = USCTariffTest.CreateUSCTariffWithPermitLicenseIndicator(Factory, "7306191010", MiscellaneousPermitLicenseList.Codes.SteelImportLicense, importPivot.EffectiveDate);
			classification.CC_TariffNum = uscTariff.UE_Tariff;
			AssertEquals("Steel License No.:", importPivot.CalcMiscLicenseTypeLabel);

			uscTariff = USCTariffTest.CreateUSCTariffWithPermitLicenseIndicator(Factory, "99106145", MiscellaneousPermitLicenseList.Codes.SingaporeTPLCertificate, importPivot.EffectiveDate);
			classification.CC_TariffNum = uscTariff.UE_Tariff;
			AssertEquals("SG TPL License No.:", importPivot.CalcMiscLicenseTypeLabel);

			uscTariff = USCTariffTest.CreateUSCTariffWithPermitLicenseIndicator(Factory, "99990056", MiscellaneousPermitLicenseList.Codes.CANAFTATPLCertificate, importPivot.EffectiveDate);
			classification.CC_TariffNum = uscTariff.UE_Tariff;
			AssertEquals("CA NAFTA Cert. No.:", importPivot.CalcMiscLicenseTypeLabel);

			uscTariff = USCTariffTest.CreateUSCTariffWithPermitLicenseIndicator(Factory, "99990060", MiscellaneousPermitLicenseList.Codes.MXNAFTATPLCertificate, importPivot.EffectiveDate);
			classification.CC_TariffNum = uscTariff.UE_Tariff;
			AssertEquals("MX NAFTA Cert. No.:", importPivot.CalcMiscLicenseTypeLabel);

			uscTariff = USCTariffTest.CreateUSCTariffWithPermitLicenseIndicator(Factory, "0201101090", MiscellaneousPermitLicenseList.Codes.BeefExportCertificate, importPivot.EffectiveDate);
			classification.CC_TariffNum = uscTariff.UE_Tariff;
			AssertEquals("Beef Certificate No.:", importPivot.CalcMiscLicenseTypeLabel);

			uscTariff = USCTariffTest.CreateUSCTariffWithPermitLicenseIndicator(Factory, "7102213000", MiscellaneousPermitLicenseList.Codes.DiamondCertificate, importPivot.EffectiveDate);
			classification.CC_TariffNum = uscTariff.UE_Tariff;
			AssertEquals("Diamond Cert. No.:", importPivot.CalcMiscLicenseTypeLabel);

			uscTariff = USCTariffTest.CreateUSCTariffWithPermitLicenseIndicator(Factory, "4407100119", MiscellaneousPermitLicenseList.Codes.CanadaSoftwoodLumberExportNumber, importPivot.EffectiveDate);
			classification.CC_TariffNum = uscTariff.UE_Tariff;
			AssertEquals("Lumber Permit No.:", importPivot.CalcMiscLicenseTypeLabel);

			uscTariff = USCTariffTest.CreateUSCTariffWithPermitLicenseIndicator(Factory, "98211119", MiscellaneousPermitLicenseList.Codes.ATPDEACertificateHTS98211119, importPivot.EffectiveDate);
			classification.CC_TariffNum = uscTariff.UE_Tariff;
			AssertEquals("ATPDEA Cert No.:", importPivot.CalcMiscLicenseTypeLabel);

			uscTariff = USCTariffTest.CreateUSCTariffWithPermitLicenseIndicator(Factory, "99130465", MiscellaneousPermitLicenseList.Codes.AustraliaFreeTradeExportCertificate, importPivot.EffectiveDate);
			classification.CC_TariffNum = uscTariff.UE_Tariff;
			AssertEquals("AU FT Export Cert.:", importPivot.CalcMiscLicenseTypeLabel);

			uscTariff = USCTariffTest.CreateUSCTariffWithPermitLicenseIndicator(Factory, "2523290000", MiscellaneousPermitLicenseList.Codes.MexicanCementImportLicense, importPivot.EffectiveDate);
			classification.CC_TariffNum = uscTariff.UE_Tariff;
			AssertEquals("MX Cement Imp. Lic.:", importPivot.CalcMiscLicenseTypeLabel);

			uscTariff = USCTariffTest.CreateUSCTariffWithPermitLicenseIndicator(Factory, "99156101", MiscellaneousPermitLicenseList.Codes.CAFTATPLCertificate, importPivot.EffectiveDate);
			classification.CC_TariffNum = uscTariff.UE_Tariff;
			AssertEquals("NI CAFTA TPL Cert.:", importPivot.CalcMiscLicenseTypeLabel);

			uscTariff = USCTariffTest.CreateUSCTariffWithPermitLicenseIndicator(Factory, "99025211", MiscellaneousPermitLicenseList.Codes.CottonShirtingFabricLicenseNumber, importPivot.EffectiveDate);
			classification.CC_TariffNum = uscTariff.UE_Tariff;
			AssertEquals("Cotton Shirting Lic.:", importPivot.CalcMiscLicenseTypeLabel);
		}

		public void TestCI_ChildType()
		{
			AssertEquals("", Pivot.CI_SupplementalTariff);
			Pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			Pivot.CI_SupplementalTariff = "TEST";
			Pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertEquals("", Pivot.CI_SupplementalTariff);
			Pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			Pivot.CI_SupplementalTariff = "TEST";
			Pivot.CI_ChildType = ClassificationTypeList.Codes.SHB;
			AssertEquals("", Pivot.CI_SupplementalTariff);
		}

		public void TestCI_ChildType_ReadOnly()
		{
			AssertEquals(false, Pivot.CI_ChildTypeInfo.ReadOnly);
			Pivot.Children.AddNew();
			AssertEquals(true, Pivot.CI_ChildTypeInfo.ReadOnly);
			((IBusinessObjectCollection<CusClassPartPivot>)Pivot.Children).DeleteAll();
			AssertEquals(false, Pivot.CI_ChildTypeInfo.ReadOnly);
		}

		public void TestAMSAndPSTRelatedFieldsWhenSupplementalTariffChange()
		{
			Pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			Pivot.CI_SupplementalTariff = "00000000";

			Pivot.CD_PSTDisclaimProgram = OGAIndicatorList.Codes.Declared;
			Pivot.CD_AMSDisclaimProgram = OGAIndicatorList.Codes.Declared;
			AssertEquals(OGAIndicatorList.Codes.Declared, Pivot.CD_PSTDisclaimProgram);
			AssertEquals(OGAIndicatorList.Codes.Declared, Pivot.CD_AMSDisclaimProgram);

			Pivot.CI_SupplementalTariff = "00000001";
			AssertEquals(ZString.Empty, Pivot.CD_PSTDisclaimProgram);
			AssertEquals(ZString.Empty, Pivot.CD_AMSDisclaimProgram);
		}

		public void TestUpdateTaxRelatedFieldsOnTariffChange()
		{
			SetUpTariffsForTaxRelatedFields();
			Pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			CusClassification classification = Factory.New<CusClassification>();
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			classification.CC_TariffNum = "00000000";
			CusClassification classification2 = Factory.New<CusClassification>();
			classification2.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			classification2.CC_TariffNum = "00000001";
			Pivot.CI_CC = classification.PK;
			AssertEquals(TaxApplyList.Codes.Yes, Pivot.CD_TaxApplicability);
			AssertEquals(Core.Constants.USCustoms.FeeCodes.Wines, Pivot.CD_TaxCode);
			AssertEquals("50c/KG", Pivot.CD_TaxRateDesc);
			AssertEquals(ZString.Empty, Pivot.CD_TaxRateType);
			AssertEquals(ZString.Empty, Pivot.CD_TTBRateDesignationCode);
			AssertEquals(ZDecimal.Zero, Pivot.CD_CBMADefaultTaxRate);
			Pivot.CI_CC = classification2.PK;
			AssertEquals(ZString.Empty, Pivot.CD_TaxApplicability);
			AssertEquals(ZString.Empty, Pivot.CD_TaxCode);
			AssertEquals(ZString.Empty, Pivot.CD_TaxRateDesc);
			AssertEquals(ZString.Empty, Pivot.CD_TaxRateType);
			AssertEquals(ZString.Empty, Pivot.CD_TTBRateDesignationCode);
			AssertEquals(ZDecimal.Zero, Pivot.CD_CBMADefaultTaxRate);
			Pivot.CI_ChildType = ZString.Empty;
			AssertEquals(ZString.Empty, Pivot.CD_TaxApplicability);
			AssertEquals(ZString.Empty, Pivot.CD_TaxCode);
			AssertEquals(ZString.Empty, Pivot.CD_TaxRateDesc);
			AssertEquals(ZString.Empty, Pivot.CD_TaxRateType);
			AssertEquals(ZString.Empty, Pivot.CD_TTBRateDesignationCode);
			AssertEquals(ZDecimal.Zero, Pivot.CD_CBMADefaultTaxRate);
		}

		public void TestShowHTSDefaultRate()
		{
			SetUpTariffsForTaxRelatedFields();
			Pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			CusClassification classification = Factory.New<CusClassification>();
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			classification.CC_TariffNum = "00000000";
			CusClassification classification2 = Factory.New<CusClassification>();
			classification2.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			classification2.CC_TariffNum = "00000001";
			AssertEquals(ZString.Empty, Pivot.CD_TaxRateDesc);
			Pivot.CI_CC = classification.PK;
			AssertEquals("50c/KG", Pivot.CD_TaxRateDesc);
			Pivot.CI_CC = classification2.PK;
			AssertEquals(ZString.Empty, Pivot.CD_TaxRateDesc);
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
			Pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			CusClassification classification = Factory.New<CusClassification>();
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			classification.CC_TariffNum = "00000000";
			Assert(Pivot.CD_TaxRateTypeInfo.ReadOnly);
			Assert(Pivot.CD_TaxRateDescInfo.ReadOnly);
			Assert(Pivot.CD_TTBRateDesignationCodeInfo.ReadOnly);
			Assert(Pivot.CD_CBMADefaultTaxRateInfo.ReadOnly);
			Pivot.CI_CC = classification.PK;
			Assert(!Pivot.CD_TaxRateTypeInfo.ReadOnly);
			Assert(Pivot.CD_TaxRateDescInfo.ReadOnly);
			Assert(Pivot.CD_TTBRateDesignationCodeInfo.ReadOnly);
			Assert(Pivot.CD_CBMADefaultTaxRateInfo.ReadOnly);
			Pivot.CD_TaxApplicability = TaxApplyList.Codes.Override;
			Assert(!Pivot.CD_TaxRateDescInfo.ReadOnly);
			Assert(Pivot.CD_TTBRateDesignationCodeInfo.ReadOnly);
			Assert(Pivot.CD_CBMADefaultTaxRateInfo.ReadOnly);
			Pivot.CD_ProductClaim = SecondarySpecProgIndicatorList.Codes.C;
			Assert(!Pivot.CD_TaxRateDescInfo.ReadOnly);
			Assert(!Pivot.CD_TTBRateDesignationCodeInfo.ReadOnly);
			Assert(!Pivot.CD_CBMADefaultTaxRateInfo.ReadOnly);
		}

		public void TestLoadImportTariff()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff.UE_Unit1 = "KG";
			SetUpTariffsForTaxRelatedFields();
			Pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			Pivot.CI_TariffNum = "00000000";
			AssertEquals("Should have loaded USCTariff", tariff, Pivot.ImportTariff);
			CusClassification classification = Factory.New<CusClassification>();
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			classification.CC_TariffNum = "00000000";
			Pivot.CI_TariffNum = "";
			Pivot.CI_CC = classification.PK;
			AssertEquals("Should have loaded USCTariff using Classification tariff number", tariff, Pivot.ImportTariff);
			Pivot.CI_CC = ZGuid.Empty;
			Pivot.CI_SupplementalTariff = "00000000";
			AssertEquals("Should have loaded USCTariff for SupTariff", tariff, Pivot.ImportSupTariff);
			var childPivot = Pivot.Children.AddNew();
			childPivot.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			childPivot.CI_TariffNum = "00000000";
			AssertEquals("Should have loaded USCTariff for COM child pivot", tariff, childPivot.ImportTariff);
		}

		public void TestHasChildren()
		{
			AssertEquals(false, Pivot.HasChildren);
			Pivot.Children.AddNew();
			AssertEquals(true, Pivot.HasChildren);
			((IBusinessObjectCollection<CusClassPartPivot>)Pivot.Children).DeleteAll();
			AssertEquals(false, Pivot.HasChildren);
		}

		public void TestChildren()
		{
			CusClassPartPivot childPivot = Pivot.Children.AddNew();
			AssertEquals(Pivot.PK, childPivot.CI_CI_Parent);
		}

		public void TestCloneUSSpecificData()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			CusClassification classification = Factory.New<CusClassification>();
			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			OrgPartRelation orgRel = part.RelatedOrganisations.AddOrganisationIfNotExist(org.PK, OrgPartRelation.RelationshipTypes.Owner);
			CusClassPartPivot pivot = part.PivotsForBinding.AddNew();
			pivot.CI_UsageComment = "U1";
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_OH = orgRel.OU_OH;
			pivot.CI_TariffNum = "1010101010";
			pivot.CI_SupplementalTariff = "2020202020";
			pivot.CI_CC = classification.PK;
			CusClassPartPivot pivotChild1 = pivot.Children.AddNew();
			pivotChild1.CI_UsageComment = "CU1";
			pivotChild1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivotChild1.CI_OH = orgRel.OU_OH;
			pivotChild1.CI_TariffNum = "1010101011";
			pivotChild1.CI_SupplementalTariff = "2020202021";
			pivotChild1.CI_CC = classification.PK;
			CusClassPartPivot pivotChild2 = pivot.Children.AddNew();
			pivotChild2.CI_UsageComment = "CU2";
			pivotChild2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivotChild2.CI_OH = orgRel.OU_OH;
			pivotChild2.CI_TariffNum = "1010101012";
			pivotChild2.CI_SupplementalTariff = "2020202022";
			PGA pga1 = pivot.PGAs.AddNew();
			pga1.US_PGALineValue = 1m;
			PGA pga2 = pivot.PGAs.AddNew();
			pga2.US_PGALineValue = 2m;
			CusClassPartPivot pivotCloned = (CusClassPartPivot)pivot.Clone();
			AssertClassPartPivotClone(pivotCloned, "U1", ClassificationTypeList.Codes.HTI, org.PK, "1010101010", "2020202020", classification.PK);
			AssertEquals(2, pivotCloned.PGAs.Count);
			AssertContainsExactElementsInAnyOrder(new ZDecimal[] { 1m, 2m }, pivotCloned.PGAs.GetFieldValues(PGA.Schema.US_PGALineValue));
			AssertEquals(2, pivotCloned.Children.Count);
			CusClassPartPivot pivotChild1Cloned = pivotCloned.Children[0];
			CusClassPartPivot pivotChild2Cloned = pivotCloned.Children[1];
			if (pivotChild1Cloned.CI_UsageComment == "CU2")
			{
				pivotChild1Cloned = pivotCloned.Children[1];
				pivotChild2Cloned = pivotCloned.Children[0];
			}

			AssertClassPartPivotClone(pivotChild1Cloned, "CU1", ClassificationTypeList.Codes.HTI, org.PK, "1010101011", "2020202021", classification.PK);
			AssertClassPartPivotClone(pivotChild2Cloned, "CU2", ClassificationTypeList.Codes.HTI, org.PK, "1010101012", "2020202022", ZGuid.Empty);
		}

		public void TestDelete()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.FillWithValidTestData();
			CusClassification classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "TestLookup";
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			OrgSupplierPart product1 = Factory.New<OrgSupplierPart>();
			product1.OP_PartNum = "PartNum";
			OrgPartRelation relation1ForProduct1 = product1.RelatedOrganisations.AddOrganisationIfNotExist(org.PK, OrgPartRelation.RelationshipTypes.Owner);
			CusClassPartPivot pivot1ForProduct1 = product1.PivotsForBinding.AddNew();
			pivot1ForProduct1.CI_UsageComment = "U1";
			pivot1ForProduct1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot1ForProduct1.CI_OH = relation1ForProduct1.OU_OH;
			pivot1ForProduct1.CI_TariffNum = "1010101010";
			pivot1ForProduct1.CI_SupplementalTariff = "2020202020";
			pivot1ForProduct1.CI_CC = classification.PK;
			CusClassPartPivot pivot1Child1 = pivot1ForProduct1.Children.AddNew();
			pivot1Child1.CI_UsageComment = "CU1";
			pivot1Child1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot1Child1.CI_OH = relation1ForProduct1.OU_OH;
			pivot1Child1.CI_TariffNum = "1010101011";
			pivot1Child1.CI_SupplementalTariff = "2020202021";
			pivot1Child1.CI_CC = classification.PK;
			ZGuid pivot1Child1PK = pivot1Child1.PK;
			CusClassPartPivot pivot1Child2 = pivot1ForProduct1.Children.AddNew();
			pivot1Child2.CI_UsageComment = "CU2";
			pivot1Child2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot1Child2.CI_OH = relation1ForProduct1.OU_OH;
			pivot1Child2.CI_TariffNum = "1010101012";
			pivot1Child2.CI_SupplementalTariff = "2020202022";
			ZGuid pivot1Child2PK = pivot1Child2.PK;
			CusAttributeFilter attribute1 = pivot1ForProduct1.Attributes1.AddNew();
			ZGuid attribute1PK = attribute1.PK;
			CusAttributeFilter attribute2 = pivot1ForProduct1.Attributes2.AddNew();
			ZGuid attribute2PK = attribute2.PK;
			Factory.Save();
			ZGuid product1PK = product1.PK;
			product1.Delete();
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			AssertDeleteResult(newFactory, typeof(OrgSupplierPart), product1PK);
			AssertDeleteResult(newFactory, typeof(CusClassPartPivot), pivot1Child1PK);
			AssertDeleteResult(newFactory, typeof(CusClassPartPivot), pivot1Child2PK);
			AssertDeleteResult(newFactory, typeof(CusAttributeFilter), attribute1PK);
			AssertDeleteResult(newFactory, typeof(CusAttributeFilter), attribute2PK);
		}

		public void TestDeleteNullObject()
		{
			CusClassPartPivot pivot1ForProduct1 = Factory.New<CusClassPartPivot>();
			pivot1ForProduct1.Details.IsNull = true;
			AssertNoExceptionThrown("No exception is expected here.", () => pivot1ForProduct1.Delete());
		}

		public void TestADDCVDProperties()
		{
			Pivot.CD_ADDApplicable = true;
			Pivot.CD_ADDBonded = false;
			Pivot.CD_ADDDepositRateInd = DepositRateIndicatorList.Codes.AdValorem;
			AssertEquals("ADD_NA", true, Pivot.CD_ADDApplicable);
			AssertEquals("US_IsBondedADD", false, Pivot.CD_ADDBonded);
			AssertEquals("US_ADDDepositRateIndicator", DepositRateIndicatorList.Codes.AdValorem, Pivot.CD_ADDDepositRateInd);
			Pivot.CD_CVDApplicable = true;
			Pivot.CD_CVDBonded = true;
			Pivot.CD_CVDDepositRateInd = DepositRateIndicatorList.Codes.AdValorem;
			AssertEquals("CVD_NA", true, Pivot.CD_CVDApplicable);
			AssertEquals("US_IsBondedCVD", true, Pivot.CD_CVDBonded);
			AssertEquals("US_CVDDepositRateIndicator", DepositRateIndicatorList.Codes.AdValorem, Pivot.CD_CVDDepositRateInd);
			CusClassPartPivot child1 = Pivot.Children.AddNew();
			child1.CI_ChildType = ClassificationChildTypeList.Codes.Related;
			child1.CD_UC_NKCountryOfExport = Core.Constants.CountryCodes.Aruba;
			child1.CD_ADDApplicable = true;
			child1.CD_ADDDepositRateInd = DepositRateIndicatorList.Codes.AdValorem;
			child1.CD_ADDBonded = true;
			child1.CD_CVDApplicable = true;
			child1.CD_CVDDepositRateInd = DepositRateIndicatorList.Codes.AdValorem;
			child1.CD_CVDBonded = true;
			AssertEquals("Child US_UC_NKCountryOfExport", Core.Constants.CountryCodes.Aruba, Pivot.Children[0].CD_UC_NKCountryOfExport);
			AssertEquals("Child CVD_NA", true, Pivot.Children[0].CD_CVDApplicable);
			AssertEquals("Child US_IsBondedCVD", true, Pivot.Children[0].CD_CVDBonded);
			AssertEquals("Child US_CVDDepositRateIndicator", DepositRateIndicatorList.Codes.AdValorem, Pivot.Children[0].CD_CVDDepositRateInd);
			AssertEquals("Child ADD_NA", true, Pivot.Children[0].CD_ADDApplicable);
			AssertEquals("Child US_IsBondedADD", true, Pivot.Children[0].CD_ADDBonded);
			AssertEquals("Child US_ADDDepositRateIndicator", DepositRateIndicatorList.Codes.AdValorem, Pivot.Children[0].CD_ADDDepositRateInd);
		}

		public void TestCountryOfExport()
		{
			Pivot.CD_UC_NKCountryOfExport = Core.Constants.CountryCodes.Iceland;
			AssertEquals("US_UC_NKCountryOfExport", Core.Constants.CountryCodes.Iceland, Pivot.CD_UC_NKCountryOfExport);
		}

		[TestDate(2008, 3, 25)]
		public void TestADDDutyCase_CVDDutyCaseAndRates()
		{
			var addCase = Factory.New<USCACCase>();
			addCase.U5_CaseNumber = "AZZZ23423";
			addCase.U5_CaseStatusDate = ZDateTime.Today;
			var addRate = addCase.CaseRates.AddNew();
			addRate.U6_SpecificRate = 0.2312m;
			addRate.U6_AdValoremRate = 0.5234m;
			addRate.U6_EffectiveDate = ZDateTime.Today;
			var cvdCase = Factory.New<USCACCase>();
			cvdCase.U5_CaseNumber = "CZZZ23423";
			cvdCase.U5_CaseStatusDate = ZDateTime.Today;
			var cvdRate = cvdCase.CaseRates.AddNew();
			cvdRate.U6_SpecificRate = 0.4245m;
			cvdRate.U6_AdValoremRate = 0.5623m;
			cvdRate.U6_EffectiveDate = ZDateTime.Today;
			Pivot.CD_ADDCaseNo = "AZZZ23423";
			Pivot.CD_CVDCaseNo = "CZZZ23423";
			AssertNotNull(Pivot.AntidumpingDutyCase);
			AssertNotNull(Pivot.CountervailingDutyCase);
			AssertEquals(0.23m, Pivot.GetUSCACCaseRate(Pivot.AntidumpingDutyCase).U6_SpecificRate);
			AssertEquals(0.42m, Pivot.GetUSCACCaseRate(Pivot.CountervailingDutyCase).U6_SpecificRate);
		}

		public void TestCD_ADDDepositRateDescription()
		{
			var acCase = Factory.New<USCACCase>();
			acCase.U5_CaseNumber = "AXXAAABBB";
			var rate = acCase.CaseRates.AddNew();
			rate.U6_EffectiveDate = ZDateTime.Today;
			rate.U6_AdValoremRate = 0.1234m;
			rate.U6_SpecificRate = 0.2345m;
			rate.U6_Unit = "KG";
			Pivot.CD_ADDCaseNo = "AXXAAABBB";
			Pivot.CD_ADDDepositRateInd = DepositRateIndicatorList.Codes.AdValorem;
			AssertEquals("12.34%", Pivot.CD_ADDDepositRateDescription);
			Pivot.CD_ADDDepositRateInd = DepositRateIndicatorList.Codes.Specific;
			AssertEquals("23c/KG", Pivot.CD_ADDDepositRateDescription);
			Pivot.CD_ADDDepositRateInd = DepositRateIndicatorList.Codes.OverrideAdValorem;
			Pivot.CD_ADDDepositRateDescription = "12.345%";
			AssertEquals("12.35%", Pivot.CD_ADDDepositRateDescription);
			Pivot.CD_ADDDepositRateInd = DepositRateIndicatorList.Codes.OverrideSpecific;
			Pivot.CD_ADDDepositRateDescription = "0.12345";
			AssertEquals("12.35c/KG", Pivot.CD_ADDDepositRateDescription);
			Pivot.CD_ADDDepositRateDescription = "123.45";
			rate.U6_UnitDesc = "KG";
			AssertEquals("123.45$/KG(KG)", Pivot.CD_ADDDepositRateDescription);
			Pivot.CD_ADDDepositRateDescription = "123.45C";
			AssertEquals("1.2345$/KG(KG)", Pivot.CD_ADDDepositRateDescription);
		}

		public void TestCD_CVDDepositRateDescription()
		{
			var acCase = Factory.New<USCACCase>();
			acCase.U5_CaseNumber = "CXXAAABBB";
			var rate = acCase.CaseRates.AddNew();
			rate.U6_EffectiveDate = ZDateTime.Today;
			rate.U6_AdValoremRate = 0.1234m;
			rate.U6_SpecificRate = 0.2345m;
			rate.U6_Unit = "KG";
			Pivot.CD_CVDCaseNo = "CXXAAABBB";
			Pivot.CD_CVDDepositRateInd = DepositRateIndicatorList.Codes.AdValorem;
			AssertEquals("12.34%", Pivot.CD_CVDDepositRateDescription);
			Pivot.CD_CVDDepositRateInd = DepositRateIndicatorList.Codes.Specific;
			AssertEquals("23c/KG", Pivot.CD_CVDDepositRateDescription);
			Pivot.CD_CVDDepositRateInd = DepositRateIndicatorList.Codes.OverrideAdValorem;
			Pivot.CD_CVDDepositRateDescription = "12.345%";
			AssertEquals("12.35%", Pivot.CD_CVDDepositRateDescription);
			Pivot.CD_CVDDepositRateInd = DepositRateIndicatorList.Codes.OverrideSpecific;
			Pivot.CD_CVDDepositRateDescription = "0.12345";
			AssertEquals("12.35c/KG", Pivot.CD_CVDDepositRateDescription);
			Pivot.CD_CVDDepositRateDescription = "123.45";
			rate.U6_UnitDesc = "KG";
			AssertEquals("123.45$/KG(KG)", Pivot.CD_CVDDepositRateDescription);
			Pivot.CD_CVDDepositRateDescription = "123.45C";
			AssertEquals("1.2345$/KG(KG)", Pivot.CD_CVDDepositRateDescription);
		}

		public void TestCD_ADDDepositRateDescriptionReadOnly()
		{
			Pivot.CD_ADDDepositRateInd = DepositRateIndicatorList.Codes.AdValorem;
			AssertEquals(true, Pivot.CD_ADDDepositRateDescriptionInfo.ReadOnly);
			Pivot.CD_ADDDepositRateInd = DepositRateIndicatorList.Codes.Specific;
			AssertEquals(true, Pivot.CD_ADDDepositRateDescriptionInfo.ReadOnly);
			Pivot.CD_ADDDepositRateInd = DepositRateIndicatorList.Codes.OverrideAdValorem;
			AssertEquals(false, Pivot.CD_ADDDepositRateDescriptionInfo.ReadOnly);
			Pivot.CD_ADDDepositRateInd = DepositRateIndicatorList.Codes.OverrideSpecific;
			AssertEquals(false, Pivot.CD_ADDDepositRateDescriptionInfo.ReadOnly);
		}

		public void TestCD_CVDDepositRateDescriptionReadOnly()
		{
			Pivot.CD_CVDDepositRateInd = DepositRateIndicatorList.Codes.AdValorem;
			AssertEquals(true, Pivot.CD_CVDDepositRateDescriptionInfo.ReadOnly);
			Pivot.CD_CVDDepositRateInd = DepositRateIndicatorList.Codes.Specific;
			AssertEquals(true, Pivot.CD_CVDDepositRateDescriptionInfo.ReadOnly);
			Pivot.CD_CVDDepositRateInd = DepositRateIndicatorList.Codes.OverrideAdValorem;
			AssertEquals(false, Pivot.CD_CVDDepositRateDescriptionInfo.ReadOnly);
			Pivot.CD_CVDDepositRateInd = DepositRateIndicatorList.Codes.OverrideSpecific;
			AssertEquals(false, Pivot.CD_CVDDepositRateDescriptionInfo.ReadOnly);
		}

		[TestDate(2016, 09, 18)]
		public void TestAPHISIndicatorOnProduct()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0805406000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;
			tariff.UE_PGACodes = "AQ2";
			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			var details = pivot.Details;
			pivot.CI_TariffNum = tariff.UE_Tariff;
			details.CD_APHISIndicator = "~";
			AssertHasMessageErrorContaining(details.CD_APHISIndicatorInfo, ListValidation.InvalidCodeMessageError);
			details.CD_APHISIndicator = "D";
			AssertNoMessageErrorContaining(details.CD_APHISIndicatorInfo, ListValidation.InvalidCodeMessageError);
			AssertEquals("AMS should be D", OGAIndicatorList.Codes.Declared, pivot.Details.CD_APHISIndicator);
		}

		public void TestOGAIndicatorDefaultValueShouldNotBeSetWhenExport()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0805406000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;
			tariff.UE_PGACodes = "FD2FD4";
			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot1 = product.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot1.CI_TariffNum = tariff.UE_Tariff;
			AssertEquals("CD_ACEFDAIndicator should be set default value", OGAIndicatorList.Codes.Declared, pivot1.CD_ACEFDAIndicator);
			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertNotEquals("CD_ACEFDAIndicator shouldn't be set default value", OGAIndicatorList.Codes.Declared, pivot2.CD_ACEFDAIndicator);
			var pivot3 = product.PivotsForBinding.AddNew();
			pivot3.CI_ChildType = ClassificationTypeList.Codes.SHB;
			AssertNotEquals("CD_ACEFDAIndicator shouldn't be set default value", OGAIndicatorList.Codes.Declared, pivot3.CD_ACEFDAIndicator);
		}

		public void TestOGAPGADataBeRemovedWhenChangingTariffTypeToExport()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0805406000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;
			tariff.UE_OGACodes = "FD2FD2";
			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = tariff.UE_Tariff;
			var acefda = pivot.ACEFDAs.AddNew();
			acefda.US_ProcessingCode = "D";
			var acefdaAffirmation = acefda.AffirmationCodes.AddNew();
			acefdaAffirmation.CY_Code = "AIN";
			Factory.Save();
			AssertEquals("pivot should have 1 ACEFDAs", 1, pivot.ACEFDAs.Cast<ACEFDA>().Count());
			pivot.NeedToRemovePGAData = true;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			Factory.Save();
			AssertEquals("pivot's ACEFDAs should be removed", false, pivot.HasACEFDAs);
		}

		public void TestGetReconIssueCalculated()
		{
			Pivot.CD_ReconIssue = ReconIssueCodeList.Codes.ValueClass9802Recon;
			AssertEquals("Recon Issue", ReconIssues._98 | ReconIssues.CL | ReconIssues.VL, Pivot.GetReconIssueCalculated());
		}

		public void TestCusUSClassificationIsMadeOnReloadIfNecessary()
		{
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_OP = part.PK;
			AssertNotNull(pivot.Details);
			pivot.Details.Delete();
			Factory.Save();
			var pivotReloaded = new BusinessObjectFactory().Load<CusClassPartPivot>(pivot.PK);
			AssertNotNull(pivotReloaded.Details);
		}

		public void TestExportPGAIndicators()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot.OnExportPGAIndicatorChangedEvent += (ZString agencyCode, ZBool hasExportPGAData) => true;
			pivot.CD_ATFIndicator = OGAIndicatorList.Codes.Declared;
			pivot.ExportATF.US_Quantity = 123m;
			AssertEquals(OGAIndicatorList.Codes.Declared, pivot.CD_ATFIndicator);
			AssertEquals(123m, pivot.ExportATF.US_Quantity);
			pivot.CD_ATFIndicator = ZString.Empty;
			AssertEquals(ZString.Empty, pivot.CD_ATFIndicator);
			AssertEquals(0m, pivot.ExportATF.US_Quantity);
			pivot.CD_DEAIndicator = OGAIndicatorList.Codes.Declared;
			var exportDEA = pivot.DEAHeaders.AddNew();
			exportDEA.US_DrugCode = "ABCD";
			AssertEquals(OGAIndicatorList.Codes.Declared, pivot.CD_DEAIndicator);
			AssertEquals(1, pivot.DEAHeaders.Count);
			AssertEquals("ABCD", pivot.DEAHeaders[0].US_DrugCode);
			pivot.CD_DEAIndicator = ZString.Empty;
			AssertEquals(ZString.Empty, pivot.CD_DEAIndicator);
			AssertEquals(0, pivot.DEAHeaders.Count);
			pivot.CD_TTBIndicator = OGAIndicatorList.Codes.Disclaimed;
			var exportTTB = pivot.TTBLines.AddNew();
			exportTTB.US_NumberForIRC = "1234567";
			AssertEquals(OGAIndicatorList.Codes.Disclaimed, pivot.CD_TTBIndicator);
			AssertEquals(1, pivot.TTBLines.Count);
			AssertEquals("1234567", pivot.TTBLines[0].US_NumberForIRC);
			pivot.CD_TTBIndicator = ZString.Empty;
			AssertEquals(ZString.Empty, pivot.CD_TTBIndicator);
			AssertEquals(0, pivot.TTBLines.Count);
			pivot.CD_AMSIndicator = OGAIndicatorList.Codes.Declared;
			pivot.CD_ExportCertificateNo = "14244441321";
			AssertEquals(OGAIndicatorList.Codes.Declared, pivot.CD_AMSIndicator);
			AssertEquals("14244441321", pivot.CD_ExportCertificateNo);
			pivot.CD_AMSIndicator = ZString.Empty;
			AssertEquals(ZString.Empty, pivot.CD_AMSIndicator);
			AssertEquals(ZString.Empty, pivot.CD_ExportCertificateNo);
			pivot.CD_PSTIndicator = OGAIndicatorList.Codes.Declared;
			pivot.CD_EPAConsentNumber = "123132131";
			pivot.CD_PSTDisclaimProgram = "PST";
			AssertEquals(OGAIndicatorList.Codes.Declared, pivot.CD_PSTIndicator);
			AssertEquals("123132131", pivot.CD_EPAConsentNumber);
			AssertEquals("PST", pivot.CD_PSTDisclaimProgram);
			pivot.CD_PSTIndicator = ZString.Empty;
			AssertEquals(ZString.Empty, pivot.CD_PSTIndicator);
			AssertEquals(ZString.Empty, pivot.CD_EPAConsentNumber);
			AssertEquals(ZString.Empty, pivot.CD_PSTDisclaimProgram);
			pivot.CD_NMFSHMSIndicator = OGAIndicatorList.Codes.Declared;
			var exportNMFS = pivot.NMFSLines.AddNew();
			exportNMFS.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
			AssertEquals(OGAIndicatorList.Codes.Declared, pivot.CD_NMFSHMSIndicator);
			AssertEquals(1, pivot.NMFSLines.Count);
			AssertEquals(NMFSProgramCodeList.Codes.HMS, pivot.NMFSLines[0].US_ProgramType);
			pivot.CD_NMFSHMSIndicator = ZString.Empty;
			AssertEquals(ZString.Empty, pivot.CD_NMFSHMSIndicator);
			AssertEquals(0, pivot.NMFSLines.Count);
			pivot.CD_FWSIndicator = OGAIndicatorList.Codes.Declared;
			pivot.ExportFWS.US_ConfirmationNum = "123156GF123";
			AssertEquals(OGAIndicatorList.Codes.Declared, pivot.CD_FWSIndicator);
			AssertEquals("123156GF123", pivot.ExportFWS.US_ConfirmationNum);
			pivot.CD_FWSIndicator = ZString.Empty;
			AssertEquals(ZString.Empty, pivot.CD_FWSIndicator);
			AssertEquals(ZString.Empty, pivot.ExportFWS.US_ConfirmationNum);
			pivot.OnExportPGAIndicatorChangedEvent = null;
			pivot.OnExportPGAIndicatorChangedEvent += (ZString agencyCode, ZBool hasExportPGAData) => false;
			pivot.CD_DEAIndicator = OGAIndicatorList.Codes.Declared;
			exportDEA = pivot.DEAHeaders.AddNew();
			exportDEA.US_DrugCode = "ABCD";
			AssertEquals(OGAIndicatorList.Codes.Declared, pivot.CD_DEAIndicator);
			AssertEquals(1, pivot.DEAHeaders.Count);
			AssertEquals("ABCD", pivot.DEAHeaders[0].US_DrugCode);
			pivot.CD_DEAIndicator = ZString.Empty;
			AssertEquals(OGAIndicatorList.Codes.Declared, pivot.CD_DEAIndicator);
			AssertEquals(1, pivot.DEAHeaders.Count);
			AssertEquals("ABCD", pivot.DEAHeaders[0].US_DrugCode);
			pivot.CD_TTBIndicator = OGAIndicatorList.Codes.Disclaimed;
			exportTTB = pivot.TTBLines.AddNew();
			exportTTB.US_NumberForIRC = "1234567";
			AssertEquals(OGAIndicatorList.Codes.Disclaimed, pivot.CD_TTBIndicator);
			AssertEquals(1, pivot.TTBLines.Count);
			AssertEquals("1234567", pivot.TTBLines[0].US_NumberForIRC);
			pivot.CD_TTBIndicator = ZString.Empty;
			AssertEquals(OGAIndicatorList.Codes.Disclaimed, pivot.CD_TTBIndicator);
			AssertEquals(1, pivot.TTBLines.Count);
			AssertEquals("1234567", pivot.TTBLines[0].US_NumberForIRC);
			pivot.CD_AMSIndicator = OGAIndicatorList.Codes.Declared;
			pivot.CD_ExportCertificateNo = "14244441321";
			AssertEquals(OGAIndicatorList.Codes.Declared, pivot.CD_AMSIndicator);
			AssertEquals("14244441321", pivot.CD_ExportCertificateNo);
			pivot.CD_AMSIndicator = ZString.Empty;
			AssertEquals(OGAIndicatorList.Codes.Declared, pivot.CD_AMSIndicator);
			AssertEquals("14244441321", pivot.CD_ExportCertificateNo);
			pivot.CD_PSTIndicator = OGAIndicatorList.Codes.Declared;
			pivot.CD_EPAConsentNumber = "123132131";
			AssertEquals(OGAIndicatorList.Codes.Declared, pivot.CD_PSTIndicator);
			AssertEquals("123132131", pivot.CD_EPAConsentNumber);
			pivot.CD_PSTIndicator = ZString.Empty;
			AssertEquals(OGAIndicatorList.Codes.Declared, pivot.CD_PSTIndicator);
			AssertEquals("123132131", pivot.CD_EPAConsentNumber);
			pivot.CD_NMFSHMSIndicator = OGAIndicatorList.Codes.Declared;
			exportNMFS = pivot.NMFSLines.AddNew();
			exportNMFS.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
			AssertEquals(OGAIndicatorList.Codes.Declared, pivot.CD_NMFSHMSIndicator);
			AssertEquals(1, pivot.NMFSLines.Count);
			AssertEquals(NMFSProgramCodeList.Codes.HMS, pivot.NMFSLines[0].US_ProgramType);
			pivot.CD_NMFSHMSIndicator = ZString.Empty;
			AssertEquals(OGAIndicatorList.Codes.Declared, pivot.CD_NMFSHMSIndicator);
			AssertEquals(1, pivot.NMFSLines.Count);
			AssertEquals(NMFSProgramCodeList.Codes.HMS, pivot.NMFSLines[0].US_ProgramType);
			pivot.CD_FWSIndicator = OGAIndicatorList.Codes.Declared;
			pivot.ExportFWS.US_ConfirmationNum = "123156GF123";
			AssertEquals(OGAIndicatorList.Codes.Declared, pivot.CD_FWSIndicator);
			AssertEquals("123156GF123", pivot.ExportFWS.US_ConfirmationNum);
			pivot.CD_FWSIndicator = ZString.Empty;
			AssertEquals(OGAIndicatorList.Codes.Declared, pivot.CD_FWSIndicator);
			AssertEquals("123156GF123", pivot.ExportFWS.US_ConfirmationNum);
		}

		public void TestPGAAgencyRequirements()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			var ogaReqCollection = pivot.OGAAgencyRequirements;
			AssertNotEquals(0, ogaReqCollection.Count);
			var expPGAReqCollection = pivot.ExportPGAAgencyRequirements;
			AssertNotEquals(0, expPGAReqCollection.Count);
			pivot.Delete();
			AssertEquals(0, ogaReqCollection.Count);
			AssertEquals(0, expPGAReqCollection.Count);
		}

		public void TestDefaultUniversalTariffProperties()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			AssertEquals("Using Universal Tariff", false, typeof(CusClassPartPivot).GetProperty("UseUniversalTariff", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(pivot));
		}

		public void TestAMMVReadOnly()
		{
			CombineAssertions(() =>
			{
				var pivot = Factory.New<CusClassPartPivot>();
				AssertEquals(false, pivot.CD_AMMVPercentageInfo.ReadOnly);
				AssertEquals(false, pivot.CD_AMMVPerUnitInfo.ReadOnly);
				AssertEquals(false, pivot.CD_AMMVPerUnitCurrencyInfo.ReadOnly);
				pivot.CD_AMMVPerUnit = 10m;
				AssertEquals(true, pivot.CD_AMMVPercentageInfo.ReadOnly);
				pivot.CD_AMMVPerUnit = 0m;
				AssertEquals(false, pivot.CD_AMMVPercentageInfo.ReadOnly);
				pivot.CD_AMMVPercentage = 10m;
				AssertEquals(true, pivot.CD_AMMVPerUnitInfo.ReadOnly);
				AssertEquals(true, pivot.CD_AMMVPerUnitCurrencyInfo.ReadOnly);
				pivot.CD_AMMVPercentage = 0m;
				AssertEquals(false, pivot.CD_AMMVPerUnitInfo.ReadOnly);
				AssertEquals(false, pivot.CD_AMMVPerUnitCurrencyInfo.ReadOnly);
				pivot.Details.CD_AMMVPercentage = 10m;
				pivot.Details.CD_AMMVPerUnit = 10m;
				AssertEquals(false, pivot.CD_AMMVPerUnitInfo.ReadOnly);
				AssertEquals(false, pivot.CD_AMMVPercentageInfo.ReadOnly);
				AssertEquals(false, pivot.CD_AMMVPerUnitCurrencyInfo.ReadOnly);
			});
		}

		public void TestCD_AMSDisclaimRelatedFieldsReadOnly()
		{
			CombineAssertions(() =>
			{
				Pivot.CD_AMSIndicator = OGAIndicatorList.Codes.Disclaimed;
				AssertEquals(false, Pivot.CD_AMSDisclaimProgramInfo.ReadOnly);

				Pivot.CD_AMSIndicator = OGAIndicatorList.Codes.Declared;
				AssertEquals(true, Pivot.CD_AMSDisclaimProgramInfo.ReadOnly);
			});
		}

		public void TestCD_PSTDisclaimRelatedFieldsReadOnly()
		{
			CombineAssertions(() =>
			{
				Pivot.CD_PSTIndicator = OGAIndicatorList.Codes.Disclaimed;
				AssertEquals(false, Pivot.CD_PSTDisclaimProgramInfo.ReadOnly);

				Pivot.CD_PSTIndicator = OGAIndicatorList.Codes.Declared;
				AssertEquals(true, Pivot.CD_PSTDisclaimProgramInfo.ReadOnly);
			});
		}

		public void TestOnCD_AMMVPercentageChangedCD_AMMVPerUnitIsClearedOutAndRefresh()
		{
			CombineAssertions(() =>
			{
				var pivot = Factory.New<CusClassPartPivot>();
				pivot.Details.CD_AMMVPercentage = 10m;
				pivot.Details.CD_AMMVPerUnit = 10m;
				var refreshedCount = 0;
				(pivot as IBindingList).ListChanged += (sender, e) => refreshedCount++;
				pivot.CD_AMMVPercentage = ZDecimal.Zero;
				AssertEquals(1, refreshedCount);
				AssertEquals(10m, pivot.CD_AMMVPerUnit);
				pivot.CD_AMMVPercentage = 20m;
				AssertEquals(2, refreshedCount);
				AssertEquals(ZDecimal.Zero, pivot.CD_AMMVPerUnit);
				pivot.CD_AMMVPercentage = 20m;
				AssertEquals("Should not trigger change if value hasn't changed", 2, refreshedCount);
				AssertEquals(ZDecimal.Zero, pivot.CD_AMMVPerUnit);
				pivot.CD_AMMVPercentage = 30m;
				AssertEquals(3, refreshedCount);
				AssertEquals(ZDecimal.Zero, pivot.CD_AMMVPerUnit);
			});
		}

		public void TestOnCD_AMMVPercentageChangedCD_AMMVPerUnitCurrencyIsEmpty()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CD_AMMVPerUnitCurrency = "CAD";
			pivot.CD_AMMVPerUnit = 5m;
			AssertEquals("CAD", pivot.CD_AMMVPerUnitCurrency);
			AssertEquals(5m, pivot.CD_AMMVPerUnit);

			pivot.CD_AMMVPercentage = 10m;
			AssertEquals(10m, pivot.CD_AMMVPercentage);
			AssertEquals(ZString.Empty, pivot.CD_AMMVPerUnitCurrency);
			AssertEquals(0m, pivot.CD_AMMVPerUnit);
		}

		public void TestOnCD_AMMVPerUnitChangedCD_AMMVPercentageIsClearedOutAndRefresh()
		{
			CombineAssertions(() =>
			{
				var pivot = Factory.New<CusClassPartPivot>();
				pivot.Details.CD_AMMVPerUnit = 10m;
				pivot.Details.CD_AMMVPercentage = 10m;
				var valueChangedCount = 0;
				(pivot as IBindingList).ListChanged += (sender, e) => valueChangedCount++;
				pivot.CD_AMMVPerUnit = ZDecimal.Zero;
				AssertEquals(1, valueChangedCount);
				AssertEquals(10m, pivot.CD_AMMVPercentage);
				pivot.CD_AMMVPerUnit = 20m;
				AssertEquals(2, valueChangedCount);
				AssertEquals(ZDecimal.Zero, pivot.CD_AMMVPercentage);
				pivot.CD_AMMVPerUnit = 20m;
				AssertEquals("Should not trigger change if value hasn't changed", 2, valueChangedCount);
				AssertEquals(ZDecimal.Zero, pivot.CD_AMMVPercentage);
				pivot.CD_AMMVPerUnit = 30m;
				AssertEquals(3, valueChangedCount);
				AssertEquals(ZDecimal.Zero, pivot.CD_AMMVPercentage);
			});
		}

		public void TestAdditionalTariffs()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertEquals("Prov Add. Tariff 1 field is editable", false, pivot.SupFormattedAdditionalTariff1Info.ReadOnly);
			AssertEquals("Prov Add. Tariff 2 field is editable", false, pivot.SupFormattedAdditionalTariff2Info.ReadOnly);
			AssertEquals("Prov Add. Tariff 3 field is editable", false, pivot.SupFormattedAdditionalTariff3Info.ReadOnly);
			AssertEquals("Prov Add. Tariff 4 field is editable", false, pivot.SupFormattedAdditionalTariff4Info.ReadOnly);
			AssertEquals("Prov Add. Tariff 5 field is editable", false, pivot.SupFormattedAdditionalTariff5Info.ReadOnly);

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertEquals("Prov Add. Tariff 1 field is readonly", true, pivot.SupFormattedAdditionalTariff1Info.ReadOnly);
			AssertEquals("Prov Add. Tariff 2 field is readonly", true, pivot.SupFormattedAdditionalTariff2Info.ReadOnly);
			AssertEquals("Prov Add. Tariff 3 field is readonly", true, pivot.SupFormattedAdditionalTariff3Info.ReadOnly);
			AssertEquals("Prov Add. Tariff 4 field is readonly", true, pivot.SupFormattedAdditionalTariff4Info.ReadOnly);
			AssertEquals("Prov Add. Tariff 5 field is readonly", true, pivot.SupFormattedAdditionalTariff5Info.ReadOnly);

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.SupFormattedAdditionalTariff1 = "9903.88.01";
			pivot.SupFormattedAdditionalTariff2 = "9903.88.02";
			pivot.SupFormattedAdditionalTariff3 = "9903.88.03";
			pivot.SupFormattedAdditionalTariff4 = "9903.88.04";
			pivot.SupFormattedAdditionalTariff5 = "9903.88.05";
			AssertEquals("Prov Add. Tariff 1 value matches", "9903.88.01", pivot.SupFormattedAdditionalTariff1);
			AssertEquals("Prov Add. Tariff 2 value matches", "9903.88.02", pivot.SupFormattedAdditionalTariff2);
			AssertEquals("Prov Add. Tariff 3 value matches", "9903.88.03", pivot.SupFormattedAdditionalTariff3);
			AssertEquals("Prov Add. Tariff 4 value matches", "9903.88.04", pivot.SupFormattedAdditionalTariff4);
			AssertEquals("Prov Add. Tariff 5 value matches", "9903.88.05", pivot.SupFormattedAdditionalTariff5);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			Pivot.Children.AddNew();
			Pivot.Children.AddNew();
			return Pivot;
		}

		protected override BusinessObject GetNewBusinessObject() => Pivot;

		CusClassPartPivot pivot;
		CusClassPartPivot Pivot
		{
			get
			{
				if (pivot == null)
				{
					var classification = Factory.New<CusClassification>();
					classification.CC_LookupCode = "TestLookup";
					classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
					OrgSupplierPart product = Factory.New<OrgSupplierPart>();
					product.OP_PartNum = product.PK.ToString().Replace("-", "");
					pivot = (CusClassPartPivot)Factory.New(GetExpectedBusinessObjectType());
					pivot.CI_CC = classification.PK;
					pivot.CI_OP = product.PK;
				}

				return pivot;
			}
		}

		void CheckIndicatorValue(string agencyCode, ProductPGAgencyRequirementProvider fpgaProvider)
		{
			var requirement = new OGAAgencyRequirement(fpgaProvider.Factory, () => fpgaProvider.GetRequirementDescription(agencyCode),
				fpgaProvider.GetIndicatorInfo(agencyCode), () => fpgaProvider.ValidateIndicator(agencyCode),
				fpgaProvider.GetDisclaimReasonInfo(agencyCode), () => fpgaProvider.ValidateDisclaimReason(agencyCode),
				() => fpgaProvider.IsPGA(agencyCode) ? fpgaProvider.GetDisclaimReasonList(agencyCode) : null);

			requirement.Indicator = OGAIndicatorList.Codes.Declared;
			AssertEquals(OGAIndicatorList.Codes.Declared, fpgaProvider.GetIndicatorInfo(agencyCode).Value);

			requirement.Indicator = OGAIndicatorList.Codes.Disclaimed;
			AssertEquals(OGAIndicatorList.Codes.Disclaimed, fpgaProvider.GetIndicatorInfo(agencyCode).Value);
		}

		void DisclaimedReason(string agencyCode, string disclaimedReasonCode1, CusClassPartPivot pivot)
		{
			var fpgaProvider = new ProductPGAgencyRequirementProvider(pivot);
			var requirement = new OGAAgencyRequirement(fpgaProvider.Factory, () => fpgaProvider.GetRequirementDescription(agencyCode),
				fpgaProvider.GetIndicatorInfo(agencyCode), () => fpgaProvider.ValidateIndicator(agencyCode),
				fpgaProvider.GetDisclaimReasonInfo(agencyCode), () => fpgaProvider.ValidateDisclaimReason(agencyCode),
				() => fpgaProvider.IsPGA(agencyCode) ? fpgaProvider.GetDisclaimReasonList(agencyCode) : null);

			if (fpgaProvider.GetDisclaimReasonInfo(agencyCode) != null)
			{
				requirement.DisclaimedReason = disclaimedReasonCode1;
				AssertEquals(disclaimedReasonCode1, requirement.DisclaimedReasonInfo.Value);
			}
		}

		void CheckIndicatorValidateionWithPivot(string agencyCode, CusClassPartPivot pivot)
		{
			var fpgaProvider = new ProductPGAgencyRequirementProvider(pivot);
			var requirement = new OGAAgencyRequirement(fpgaProvider.Factory, () => fpgaProvider.GetRequirementDescription(agencyCode),
				fpgaProvider.GetIndicatorInfo(agencyCode), () => fpgaProvider.ValidateIndicator(agencyCode),
				fpgaProvider.GetDisclaimReasonInfo(agencyCode), () => fpgaProvider.ValidateDisclaimReason(agencyCode),
				() => fpgaProvider.IsPGA(agencyCode) ? fpgaProvider.GetDisclaimReasonList(agencyCode) : null);
			requirement.ValidateIndicator();

			AssertEquals(false, requirement.IndicatorInfo.HasMessageErrors());
		}

		void AssertPivot(CusClassPartPivot pivot, ZString tariff, ZGuid productPK, ZByte order)
		{
			AssertEquals("CI_TariffNum", tariff, pivot.CI_TariffNum);
			AssertEquals("CI_OP", productPK, pivot.CI_OP);
			AssertEquals("CI_ChildListOrder", order, pivot.CI_ChildListOrder);
		}

		void AssertContainsLog(Logs logs, string expected, bool result)
		{
			var hasLog = logs.GetAllLogs().ToList().Exists(o => expected == (o as StmALog).SL_Reference);
			AssertEquals(result, hasLog);
		}

		void AssertChangeOfValueIsLoggedAndClearsAuditEntry(BaseCusClassPartPivot pivot, Action<BaseCusClassPartPivot> setPropertyFunc)
		{
			pivot.CI_LastAuditedUser = "USR";
			pivot.CI_LastAuditedDate = ZDateTime.BrettsBirthday;
			Factory.Save();
			AssertEquals("USR", pivot.CI_LastAuditedUser);
			AssertEquals(ZDateTime.BrettsBirthday, pivot.CI_LastAuditedDate);
			var timeOfChange = ZDateTime.UtcNow;
			setPropertyFunc(pivot);
			Factory.Save();
			AssertEquals("", pivot.CI_LastAuditedUser);
			AssertEquals(ZDateTime.Empty, pivot.CI_LastAuditedDate);
			var logEntry = pivot.Logs.MostRecentLog;
			AssertEquals(Events.EditedARecord, logEntry.Event);
			AssertLessThanOrEqualTo(timeOfChange, logEntry.SL_PostedTimeUtc);
		}

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
			dutyRate2.UD_TaxFeeFlag = "2";
			dutyRate2.UD_TaxFeeComputationCode = ComputationCodeList.Codes.NoComputationFormulaAvailable;
		}

		void AssertDeleteResult(BusinessObjectFactory factory, Type objectType, ZGuid objectPK)
		{
			var loadedObject = factory.Load(objectType, objectPK);
			AssertNull(loadedObject);
		}

		void AssertClassPartPivotClone(CusClassPartPivot pivot, ZString usageComment, ZString childType, ZGuid relatedOrgPk, ZString tariffNum, ZString supplimentalTariff, ZGuid classPK)
		{
			AssertEquals(usageComment, pivot.CI_UsageComment);
			AssertEquals(childType, pivot.CI_ChildType);
			AssertEquals(relatedOrgPk, pivot.CI_OH);
			AssertEquals(tariffNum, pivot.CI_TariffNum);
			AssertEquals(supplimentalTariff, pivot.CI_SupplementalTariff);
			AssertEquals(classPK, pivot.CI_CC);
		}

		public void TestDefaultOGAIndicatorsWhenChanges()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1010101010";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff.UE_PGACodes = "CP2OM2";

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.USCPSC, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.USOMC, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				var pivot = Factory.New<CusClassPartPivot>();
				var classification = Factory.New<CusClassification>();
				classification.CC_TariffNum = "1010101010";
				pivot.CI_CC = classification.PK;
				pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
				AssertEquals(OGAIndicatorList.Codes.Declared, pivot.CD_CPSCIndicator);
				AssertEquals(OGAIndicatorList.Codes.Declared, pivot.CD_OMCIndicator);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.USCPSC, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, false))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.USOMC, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, false))
			{
				var pivot = Factory.New<CusClassPartPivot>();
				var classification = Factory.New<CusClassification>();
				classification.CC_TariffNum = "1010101010";
				pivot.CI_CC = classification.PK;
				pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
				AssertEquals(ZString.Empty, pivot.CD_CPSCIndicator);
				AssertEquals(ZString.Empty, pivot.CD_OMCIndicator);
			}
		}
	}
}

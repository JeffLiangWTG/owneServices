using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CusClassPartPivotValidationTest : BusinessObjectValidationTestCase
	{
		public void TestComponentTariffSequenceIsUniqueWithinAParent()
		{
			var part = Factory.New<OrgSupplierPart>();
			var pivotParent = part.PivotsForBinding.AddNew();
			pivotParent.CI_ChildType = "HTI";
			var childA = pivotParent.Children.AddNew();
			AssertNoErrorContaining(childA.CI_ChildListOrderInfo, "unique");
			var childB = pivotParent.Children.AddNew();
			AssertNoErrorContaining(childB.CI_ChildListOrderInfo, "unique");
			childA.CI_ChildListOrder = 99;
			AssertNoErrorContaining(childA.CI_ChildListOrderInfo, "unique");
			childB.CI_ChildListOrder = 99;
			AssertHasErrorContaining(childB.CI_ChildListOrderInfo, "unique");
		}

		public void TestCheckOverlaps()
		{
			USCTariff tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "1200000";
			tariff1.UE_DateFrom = ZDateTime.Now.AddDays(-100);
			tariff1.UE_DateTo = ZDateTime.Now.AddDays(100);
			USCTariff tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "1300000";
			tariff2.UE_DateFrom = ZDateTime.Now.AddDays(-100);
			tariff2.UE_DateTo = ZDateTime.Now.AddDays(100);
			var part = Factory.New<OrgSupplierPart>();
			var relOrg1 = part.RelatedOrganisations.AddNew();
			var pivot1 = part.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = "HTI";
			pivot1.CI_TariffNum = "1200000";
			pivot1.CI_DateStart = ZDateTime.Now;
			pivot1.CI_DateEnd = ZDateTime.Now.AddDays(10);
			pivot1.CI_OH = relOrg1.OU_OH;
			var pivot2 = part.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = "HTI";
			pivot2.CI_TariffNum = "1200000";
			pivot2.CI_DateStart = ZDateTime.Now.AddDays(20);
			pivot2.CI_DateEnd = ZDateTime.Now.AddDays(30);
			pivot2.CI_OH = relOrg1.OU_OH;
			AssertNoErrorContaining(pivot2.CI_ChildTypeInfo, pivot2.Validation.DuplicateAttributeForHTIWithoutAttributes);
			pivot2.CI_DateStart = ZDateTime.Now.AddDays(5);
			pivot2.CI_DateEnd = ZDateTime.Now.AddDays(18);
			pivot2.CI_TariffNum = "1300000";
			pivot2.CI_ChildType = "HTI";
			pivot1.CI_TariffNum = "1300000";
			pivot2.Validation.ValidateAll();
			AssertNoErrorContaining(pivot2.CI_ChildTypeInfo, pivot2.Validation.DuplicateAttributeForHTIWithoutAttributes);
			var pivot3 = part.PivotsForBinding.AddNew();
			pivot3.CI_ChildType = "HTI";
			pivot3.CI_TariffNum = "1200000";
			pivot3.CI_DateStart = ZDateTime.Now.AddDays(5);
			pivot3.CI_DateEnd = ZDateTime.Now.AddDays(18);
			pivot3.CI_TariffNum = "1300000";
			pivot3.CI_ChildType = "HTI";
			pivot3.Validation.ValidateAll();
			AssertHasErrorContaining(pivot3.CI_ChildTypeInfo, pivot3.Validation.DuplicateAttributeForHTIWithoutAttributes);
		}

		public void TestCheckDateStart()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1200000";
			tariff.UE_DateTo = ZDateTime.Now;
			tariff.UE_DateFrom = ZDateTime.Now.AddDays(-1);
			Factory.Save();
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "DZ1234ZD";
			var part = Factory.New<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = "1200000";
			pivot.CI_DateEnd = ZDateTime.Now.AddDays(10);
			pivot.CI_DateStart = ZDateTime.Now.AddDays(20);
			AssertHasErrorContaining(pivot.CI_DateStartInfo, "Start date must be before end date");
		}

		public void TestCheckDateEnd()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "DZ1234ZD";
			var part = Factory.New<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_DateStart = ZDateTime.Now;
			pivot.CI_DateEnd = ZDateTime.Now.AddDays(1);
			string message = "Start date must be before end date";
			AssertNoErrorContaining(pivot.CI_DateEndInfo, message);
			pivot.CI_DateStart = ZDateTime.Now;
			pivot.CI_DateEnd = ZDateTime.Now.AddDays(-1);
			AssertHasErrorContaining(pivot.CI_DateEndInfo, message);
			pivot.CI_DateStart = ZDateTime.Empty;
			pivot.CI_DateEnd = ZDateTime.Now;
			AssertNoErrorContaining(pivot.CI_DateEndInfo, message);
			pivot.CI_DateStart = ZDateTime.Now;
			pivot.CI_DateEnd = ZDateTime.Empty;
			AssertNoErrorContaining(pivot.CI_DateEndInfo, message);
		}

		public void TestAttributesRequirement()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "DZ1234ZD";
			var part = Factory.New<OrgSupplierPart>();
			var partRelate = part.RelatedOrganisations.AddOwner(org);
			var pivot1 = part.PivotsForBinding.AddNew();
			var pivot2 = part.PivotsForBinding.AddNew();
			pivot2.Validation.ValidateAll();
			string message = "must be specified as it's specified on another";
			AssertNoRowErrorContaining(pivot2, message);
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot2.Validation.ValidateAll();
			AssertNoRowErrorContaining(pivot2, message);
			pivot1.Attributes1.AddNew();
			pivot2.Validation.ValidateAll();
			string attrib1Message = CusClassPartPivotValidation.GetAttributeIsRequiredMessage("Attribute 1", ClassificationTypeList.Codes.HTI, "NONE");
			AssertContains(message, attrib1Message);
			string attrib2Message = CusClassPartPivotValidation.GetAttributeIsRequiredMessage("Attribute 2", ClassificationTypeList.Codes.HTI, "NONE");
			AssertContains(message, attrib2Message);
			string attrib3Message = CusClassPartPivotValidation.GetAttributeIsRequiredMessage("Attribute 3", ClassificationTypeList.Codes.HTI, "NONE");
			string attrib1MessageWitRelated = CusClassPartPivotValidation.GetAttributeIsRequiredMessage("Attribute 1", ClassificationTypeList.Codes.HTI, "DZ1234ZD");
			AssertContains(message, attrib1MessageWitRelated);
			string attrib2MessageWitRelated = CusClassPartPivotValidation.GetAttributeIsRequiredMessage("Attribute 2", ClassificationTypeList.Codes.HTI, "DZ1234ZD");
			AssertContains(message, attrib2MessageWitRelated);
			string attrib3MessageWitRelated = CusClassPartPivotValidation.GetAttributeIsRequiredMessage("Attribute 3", ClassificationTypeList.Codes.HTI, "DZ1234ZD");
			AssertContains(message, attrib3MessageWitRelated);
			AssertHasRowErrorContaining(pivot2, message);
			AssertHasRowError(pivot2, attrib1Message);
			AssertNoRowError(pivot2, attrib2Message);
			AssertNoRowError(pivot2, attrib3Message);
			AssertNoRowError(pivot2, attrib1MessageWitRelated);
			AssertNoRowError(pivot2, attrib2MessageWitRelated);
			AssertNoRowError(pivot2, attrib3MessageWitRelated);
			pivot1.Attributes2.AddNew();
			pivot2.Validation.ValidateAll();
			AssertHasRowError(pivot2, attrib1Message);
			AssertHasRowError(pivot2, attrib2Message);
			AssertNoRowError(pivot2, attrib3Message);
			AssertNoRowError(pivot2, attrib1MessageWitRelated);
			AssertNoRowError(pivot2, attrib2MessageWitRelated);
			AssertNoRowError(pivot2, attrib3MessageWitRelated);
			pivot1.Attributes3.AddNew();
			pivot2.Validation.ValidateAll();
			AssertHasRowError(pivot2, attrib1Message);
			AssertHasRowError(pivot2, attrib2Message);
			AssertHasRowError(pivot2, attrib3Message);
			AssertNoRowError(pivot2, attrib1MessageWitRelated);
			AssertNoRowError(pivot2, attrib2MessageWitRelated);
			AssertNoRowError(pivot2, attrib3MessageWitRelated);
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot2.Validation.ValidateAll();
			AssertNoRowErrorContaining(pivot2, message);
			pivot1.CI_OH = partRelate.OU_OH;
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot2.Validation.ValidateAll();
			AssertNoRowError(pivot2, attrib1Message);
			AssertNoRowError(pivot2, attrib2Message);
			AssertNoRowError(pivot2, attrib3Message);
			AssertHasRowError(pivot2, attrib1MessageWitRelated);
			AssertHasRowError(pivot2, attrib2MessageWitRelated);
			AssertHasRowError(pivot2, attrib3MessageWitRelated);
		}

		public void TestCheckCI_ChildType()
		{
			AssertChildType(x => x.Attributes1);
			AssertChildType(x => x.Attributes2);
			AssertChildType(x => x.Attributes3);
		}

		public void TestDuplicateHTIWithAttributes()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "IMP1";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "IMP2";
			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			var relOrg1 = part.RelatedOrganisations.AddOrganisationIfNotExist(org1.PK, Enterprise.MasterFiles.Business.OrgPartRelation.RelationshipTypes.Owner);
			var relOrg2 = part.RelatedOrganisations.AddOrganisationIfNotExist(org2.PK, Enterprise.MasterFiles.Business.OrgPartRelation.RelationshipTypes.Owner);
			var pivot1 = part.PivotsForBinding.AddNew();
			var pivot2 = part.PivotsForBinding.AddNew();
			var attrib1 = pivot1.Attributes1.AddNew();
			var attrib2 = pivot2.Attributes1.AddNew();
			string attribute1Name = nameof(CusAttributeFilter.AttributeFilterName.AT1);
			string attribute2Name = nameof(CusAttributeFilter.AttributeFilterName.AT2);
			string attribute3Name = nameof(CusAttributeFilter.AttributeFilterName.AT3);
			AssertDuplicateHTIWithAttributes(relOrg1.OU_OH, relOrg2.OU_OH, attribute1Name, pivot1, pivot2, attrib1, attrib2, CusClassPartPivotValidation.DuplicateAttributeForHTIWithAttributes("A", "NO VALUE", "NO VALUE", "IMP1"));
			AssertDuplicateHTIWithAttributes(relOrg1.OU_OH, relOrg2.OU_OH, attribute2Name, pivot1, pivot2, attrib1, attrib2, CusClassPartPivotValidation.DuplicateAttributeForHTIWithAttributes("NO VALUE", "A", "NO VALUE", "IMP1"));
			AssertDuplicateHTIWithAttributes(relOrg1.OU_OH, relOrg2.OU_OH, attribute3Name, pivot1, pivot2, attrib1, attrib2, CusClassPartPivotValidation.DuplicateAttributeForHTIWithAttributes("NO VALUE", "NO VALUE", "A", "IMP1"));
			var attrib3 = pivot1.Attributes2.AddNew();
			var attrib4 = pivot2.Attributes2.AddNew();
			attrib3.BG_AttributeValue1 = "C";
			attrib4.BG_AttributeValue1 = "C";
			AssertDuplicateHTIWithAttributes(relOrg1.OU_OH, relOrg2.OU_OH, attribute1Name, pivot1, pivot2, attrib1, attrib2, CusClassPartPivotValidation.DuplicateAttributeForHTIWithAttributes("A", "C", "NO VALUE", "IMP1"));
			AssertDuplicateHTIWithAttributes(relOrg1.OU_OH, relOrg2.OU_OH, attribute3Name, pivot1, pivot2, attrib1, attrib2, CusClassPartPivotValidation.DuplicateAttributeForHTIWithAttributes("NO VALUE", "C", "A", "IMP1"));
			attrib3.BG_AttributeName = attribute3Name;
			attrib4.BG_AttributeName = attribute3Name;
			AssertDuplicateHTIWithAttributes(relOrg1.OU_OH, relOrg2.OU_OH, attribute1Name, pivot1, pivot2, attrib1, attrib2, CusClassPartPivotValidation.DuplicateAttributeForHTIWithAttributes("A", "NO VALUE", "C", "IMP1"));
			AssertDuplicateHTIWithAttributes(relOrg1.OU_OH, relOrg2.OU_OH, attribute2Name, pivot1, pivot2, attrib1, attrib2, CusClassPartPivotValidation.DuplicateAttributeForHTIWithAttributes("NO VALUE", "A", "C", "IMP1"));
			var attrib5 = pivot1.Attributes3.AddNew();
			var attrib6 = pivot2.Attributes3.AddNew();
			attrib5.BG_AttributeValue1 = "D";
			attrib6.BG_AttributeValue1 = "D";
			attrib5.BG_AttributeName = attribute2Name;
			attrib6.BG_AttributeName = attribute2Name;
			AssertDuplicateHTIWithAttributes(relOrg1.OU_OH, relOrg2.OU_OH, attribute1Name, pivot1, pivot2, attrib1, attrib2, CusClassPartPivotValidation.DuplicateAttributeForHTIWithAttributes("A", "D", "C", "IMP1"));
		}

		public void TestCheckTariffValidity()
		{
			var part = Factory.New<OrgSupplierPart>();
			var pivot1 = part.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot1.CI_TariffNum = "0000";
			AssertHasMessageError(pivot1.CI_TariffNumInfo, CusClassPartPivotValidation.InvalidTariff);
			pivot1.CI_SupplementalTariff = "0000";
			AssertHasMessageError(pivot1.CI_SupplementalTariffInfo, CusClassPartPivotValidation.InvalidTariff);
			pivot1.CI_TariffNum = USCTariff.CottonFeeApplicable;
			AssertNoMessageError(pivot1.CI_TariffNumInfo, CusClassPartPivotValidation.InvalidTariff);
			pivot1.CI_SupplementalTariff = USCTariff.AGOABenefitsApplicable;
			AssertNoMessageError(pivot1.CI_SupplementalTariffInfo, CusClassPartPivotValidation.InvalidTariff);
		}

		public void TestCheckCI_CC()
		{
			var pivot1 = Part.PivotsForBinding.AddNew();
			pivot1.Validation.ValidateCI_CC();
			AssertHasError(pivot1.CI_CCInfo, "One of Tariff or Prov/Prog. Tariff or Classification is Mandatory.");
			pivot1.CI_CC = Factory.New<CusClassification>().PK;
			AssertNoError(pivot1.CI_CCInfo, "One of Tariff or Prov/Prog. Tariff or Classification is Mandatory.");
			pivot1.CI_CC = ZGuid.Empty;
			AssertHasError(pivot1.CI_CCInfo, "One of Tariff or Prov/Prog. Tariff or Classification is Mandatory.");
			pivot1.CI_TariffNum = "00000000";
			AssertNoError(pivot1.CI_CCInfo, "One of Tariff or Prov/Prog. Tariff or Classification is Mandatory.");
			var pivot2 = Part.PivotsForBinding.AddNew();
			var childLine = pivot2.Children.AddNew();
			childLine.Validation.ValidateCI_CC();
			AssertHasError(childLine.CI_CCInfo, "One of Tariff or Prov/Prog. Tariff is Mandatory.");
			childLine.CI_CC = Factory.New<CusClassification>().PK;
			AssertNoError(childLine.CI_CCInfo, "One of Tariff or Prov/Prog. Tariff is Mandatory.");
			childLine.CI_CC = ZGuid.Empty;
			AssertHasError(childLine.CI_CCInfo, "One of Tariff or Prov/Prog. Tariff is Mandatory.");
			childLine.CI_TariffNum = "00000000";
			AssertNoError(childLine.CI_CCInfo, "One of Tariff or Prov/Prog. Tariff is Mandatory.");
		}

		public void TestCheckCI_TariffNum()
		{
			var pivot1 = Part.PivotsForBinding.AddNew();
			pivot1.Validation.ValidateCI_TariffNum();
			AssertHasError(pivot1.CI_TariffNumInfo, "One of Tariff or Prov/Prog. Tariff or Classification is Mandatory.");
			AssertNoMessageError(pivot1.CI_TariffNumInfo, "Tariff unable to be found.");
			pivot1.CI_CC = Factory.New<CusClassification>().PK;
			AssertNoError(pivot1.CI_TariffNumInfo, "One of Tariff or Prov/Prog. Tariff or Classification is Mandatory.");
			AssertNoMessageError(pivot1.CI_TariffNumInfo, "Tariff unable to be found.");
			pivot1.CI_CC = ZGuid.Empty;
			AssertHasError(pivot1.CI_TariffNumInfo, "One of Tariff or Prov/Prog. Tariff or Classification is Mandatory.");
			AssertNoMessageError(pivot1.CI_TariffNumInfo, "Tariff unable to be found.");
			pivot1.CI_TariffNum = "00000000";
			AssertNoError(pivot1.CI_TariffNumInfo, "One of Tariff or Prov/Prog. Tariff or Classification is Mandatory.");
			AssertHasMessageError(pivot1.CI_TariffNumInfo, "Tariff unable to be found.");
			var pivot2 = Part.PivotsForBinding.AddNew();
			var childLine = pivot2.Children.AddNew();
			pivot2.Validation.ValidateCI_TariffNum();
			AssertHasError(pivot2.CI_TariffNumInfo, "One of Tariff or Prov/Prog. Tariff or Classification is Mandatory.");
			AssertNoMessageError(pivot2.CI_TariffNumInfo, "Tariff unable to be found.");
			pivot2.CI_CC = Factory.New<CusClassification>().PK;
			AssertNoError(pivot2.CI_TariffNumInfo, "One of Tariff or Prov/Prog. Tariff or Classification is Mandatory.");
			AssertNoMessageError(pivot2.CI_TariffNumInfo, "Tariff unable to be found.");
			pivot2.CI_CC = ZGuid.Empty;
			AssertHasError(pivot2.CI_TariffNumInfo, "One of Tariff or Prov/Prog. Tariff or Classification is Mandatory.");
			AssertNoMessageError(pivot2.CI_TariffNumInfo, "Tariff unable to be found.");
			pivot2.CI_TariffNum = "00000000";
			AssertNoError(pivot2.CI_TariffNumInfo, "One of Tariff or Prov/Prog. Tariff or Classification is Mandatory.");
			AssertHasMessageError(pivot2.CI_TariffNumInfo, "Tariff unable to be found.");
			childLine.Validation.ValidateCI_TariffNum();
			AssertHasError(childLine.CI_TariffNumInfo, "One of Tariff or Prov/Prog. Tariff is Mandatory.");
			AssertNoMessageError(childLine.CI_TariffNumInfo, "Tariff unable to be found.");
			childLine.CI_TariffNum = "00000000";
			AssertNoError(childLine.CI_TariffNumInfo, "One of Tariff or Prov/Prog. Tariff is Mandatory.");
			AssertHasMessageError(childLine.CI_TariffNumInfo, "Tariff unable to be found.");
			childLine.CI_TariffNum = "";
			childLine.CI_SupplementalTariff = "00000000";
			AssertNoError(childLine.CI_TariffNumInfo, "One of Tariff or Prov/Prog. Tariff is Mandatory.");
			AssertNoMessageError(childLine.CI_TariffNumInfo, "Tariff unable to be found.");
		}

		public void TestCheckCD_ADDDepositRateDescription()
		{
			var pivot1 = Part.PivotsForBinding.AddNew();
			pivot1.CD_ADDDepositRateInd = DepositRateIndicatorList.Codes.OverrideAdValorem;
			pivot1.CD_ADDDepositRateDescription = "100%";
			pivot1.Validation.ValidateCD_ADDDepositRateDescription();
			AssertEquals(0, pivot1.CD_ADDDepositRateDescriptionInfo.Notifications.Count());
			pivot1.CD_ADDDepositRateDescription = "100 ";
			pivot1.Validation.ValidateCD_ADDDepositRateDescription();
			AssertEquals(0, pivot1.CD_ADDDepositRateDescriptionInfo.Notifications.Count());

			var pivot2 = Part.PivotsForBinding.AddNew();
			pivot2.CD_ADDDepositRateInd = DepositRateIndicatorList.Codes.OverrideSpecific;
			pivot2.CD_ADDDepositRateDescription = "100%";
			pivot2.Validation.ValidateCD_ADDDepositRateDescription();
			AssertEquals(0, pivot1.CD_ADDDepositRateDescriptionInfo.Notifications.Count());
			pivot2.CD_ADDDepositRateDescription = "100 ";
			pivot2.Validation.ValidateCD_ADDDepositRateDescription();
			AssertEquals(0, pivot1.CD_ADDDepositRateDescriptionInfo.Notifications.Count());
		}

		public void TestCheckCD_CVDDepositRateDescription()
		{
			var pivot1 = Part.PivotsForBinding.AddNew();
			pivot1.CD_CVDDepositRateInd = DepositRateIndicatorList.Codes.OverrideAdValorem;
			pivot1.CD_CVDDepositRateDescription = "100%";
			pivot1.Validation.ValidateCD_CVDDepositRateDescription();
			AssertEquals(0, pivot1.CD_CVDDepositRateDescriptionInfo.Notifications.Count());
			pivot1.CD_CVDDepositRateDescription = "100 ";
			pivot1.Validation.ValidateCD_CVDDepositRateDescription();
			AssertEquals(0, pivot1.CD_CVDDepositRateDescriptionInfo.Notifications.Count());

			var pivot2 = Part.PivotsForBinding.AddNew();
			pivot2.CD_CVDDepositRateInd = DepositRateIndicatorList.Codes.OverrideSpecific;
			pivot2.CD_CVDDepositRateDescription = "100%";
			pivot2.Validation.ValidateCD_CVDDepositRateDescription();
			AssertEquals(0, pivot1.CD_CVDDepositRateDescriptionInfo.Notifications.Count());
			pivot2.CD_CVDDepositRateDescription = "100 ";
			pivot2.Validation.ValidateCD_CVDDepositRateDescription();
			AssertEquals(0, pivot1.CD_CVDDepositRateDescriptionInfo.Notifications.Count());
		}

		public void TestCheckCI_SupplementalTariff()
		{
			var pivot1 = Part.PivotsForBinding.AddNew();
			pivot1.Validation.ValidateCI_SupplementalTariff();
			AssertNoMessageError(pivot1.CI_SupplementalTariffInfo, "Tariff unable to be found.");
			pivot1.CI_CC = Factory.New<CusClassification>().PK;
			AssertNoMessageError(pivot1.CI_SupplementalTariffInfo, "Tariff unable to be found.");
			pivot1.CI_CC = ZGuid.Empty;
			AssertNoMessageError(pivot1.CI_SupplementalTariffInfo, "Tariff unable to be found.");
			pivot1.CI_SupplementalTariff = "00000000";
			AssertHasMessageError(pivot1.CI_SupplementalTariffInfo, "Tariff unable to be found.");
			var pivot2 = Part.PivotsForBinding.AddNew();
			var childLine = pivot2.Children.AddNew();
			pivot2.Validation.ValidateCI_SupplementalTariff();
			AssertHasError(pivot2.CI_SupplementalTariffInfo, "One of Tariff or Prov/Prog. Tariff or Classification is Mandatory.");
			AssertNoMessageError(pivot2.CI_SupplementalTariffInfo, "Tariff unable to be found.");
			pivot2.CI_CC = Factory.New<CusClassification>().PK;
			AssertNoError(pivot2.CI_SupplementalTariffInfo, "One of Tariff or Prov/Prog. Tariff or Classification is Mandatory.");
			AssertNoMessageError(pivot2.CI_SupplementalTariffInfo, "Tariff unable to be found.");
			pivot2.CI_CC = ZGuid.Empty;
			AssertHasError(pivot2.CI_SupplementalTariffInfo, "One of Tariff or Prov/Prog. Tariff or Classification is Mandatory.");
			AssertNoMessageError(pivot2.CI_SupplementalTariffInfo, "Tariff unable to be found.");
			pivot2.CI_SupplementalTariff = "00000000";
			AssertNoError(pivot2.CI_SupplementalTariffInfo, "One of Tariff or Prov/Prog. Tariff or Classification is Mandatory.");
			AssertHasMessageError(pivot2.CI_SupplementalTariffInfo, "Tariff unable to be found.");
			childLine.Validation.ValidateCI_SupplementalTariff();
			AssertHasError(childLine.CI_SupplementalTariffInfo, "One of Tariff or Prov/Prog. Tariff is Mandatory.");
			AssertNoMessageError(childLine.CI_SupplementalTariffInfo, "Tariff unable to be found.");
			childLine.CI_SupplementalTariff = "00000001";
			childLine.Validation.ValidateCI_SupplementalTariff();
			AssertNoError(childLine.CI_SupplementalTariffInfo, "One of Tariff or Prov/Prog. Tariff is Mandatory.");
			AssertHasMessageError(childLine.CI_SupplementalTariffInfo, "Tariff unable to be found.");
			childLine.CI_SupplementalTariff = "";
			childLine.CI_TariffNum = "00000001";
			childLine.Validation.ValidateCI_SupplementalTariff();
			AssertNoError(childLine.CI_SupplementalTariffInfo, "One of Tariff or Prov/Prog. Tariff is Mandatory.");
			AssertNoMessageError(childLine.CI_SupplementalTariffInfo, "Tariff unable to be found.");
		}

		public void TestCheckCI_TariffNumForHTEAndSHB()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var shBTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.ScheduleB);
			var exportTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.Export);
			var shbTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, shBTariffType.PK, "0000000001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var exportTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, exportTariffType.PK, "0000000002", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();
			var pivot1 = Part.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ClassificationTypeList.Codes.SHB;
			pivot1.CI_CC = ZGuid.Empty;
			pivot1.Validation.ValidateCI_TariffNum();
			AssertNoMessageError(pivot1.CI_TariffNumInfo, "Tariff unable to be found.");

			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot1.Validation.ValidateCI_TariffNum();
			AssertNoMessageError(pivot1.CI_TariffNumInfo, "Tariff unable to be found.");

			pivot1.CI_TariffNum = "0000000";
			pivot1.Validation.ValidateCI_TariffNum();
			AssertHasMessageError(pivot1.CI_TariffNumInfo, "Tariff unable to be found.");

			pivot1.CI_ChildType = ClassificationTypeList.Codes.SHB;
			pivot1.Validation.ValidateCI_TariffNum();
			AssertHasMessageError(pivot1.CI_TariffNumInfo, "Tariff unable to be found.");

			pivot1.CI_TariffNum = "0000000001";
			pivot1.Validation.ValidateCI_TariffNum();
			AssertNoMessageError(pivot1.CI_TariffNumInfo, "Tariff unable to be found.");

			pivot1.Validation.ValidateCI_TariffNum();
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertHasMessageError(pivot1.CI_TariffNumInfo, "Tariff unable to be found.");

			pivot1.CI_TariffNum = "0000000002";
			pivot1.Validation.ValidateCI_TariffNum();
			AssertNoMessageError(pivot1.CI_TariffNumInfo, "Tariff unable to be found.");
		}

		public void TestSupAdditionalTariffsOnProductPivot()
		{
			var pivot = Part.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = "7601.10.3000";
			pivot.SupFormattedAdditionalTariff1 = "9903.85.01";
			AssertHasMessageErrorContaining(pivot.SupFormattedAdditionalTariff1Info, "Tariff unable to be found.");
			AssertHasMessageError(pivot.SupFormattedAdditionalTariff1Info, "Only 'Prov/Prog. Tariff' should be filled when there is one supplementary tariff on classification.");

			pivot.CI_SupplementalTariff = "99038501";
			pivot.SupFormattedAdditionalTariff1 = ZString.Empty;
			AssertNoMessageErrorContaining(pivot.SupFormattedAdditionalTariff1Info, "Tariff unable to be found.");
			AssertNoMessageError(pivot.SupFormattedAdditionalTariff1Info, "Only 'Prov/Prog. Tariff' should be filled when there is one supplementary tariff on classification.");

			pivot.SupFormattedAdditionalTariff2 = "9903.85.02";
			AssertHasMessageErrorContaining(pivot.SupFormattedAdditionalTariff2Info, "Tariff unable to be found.");
			AssertHasMessageError(pivot.SupFormattedAdditionalTariff2Info, "Only 'Prov/Prog. Additional Tariff 1' and 'Prov/Prog. Tariff' should be filled when there are two supplementary tariffs on classification.");

			pivot.SupFormattedAdditionalTariff1 = "9903.85.02";
			pivot.SupFormattedAdditionalTariff2 = ZString.Empty;
			AssertNoMessageErrorContaining(pivot.SupFormattedAdditionalTariff2Info, "Tariff unable to be found.");
			AssertNoMessageError(pivot.SupFormattedAdditionalTariff2Info, "Only 'Prov/Prog. Additional Tariff 1' and 'Prov/Prog. Tariff' should be filled when there are two supplementary tariffs on classification.");

			pivot.SupFormattedAdditionalTariff3 = "9903.85.03";
			AssertHasMessageErrorContaining(pivot.SupFormattedAdditionalTariff3Info, "Tariff unable to be found.");
			AssertHasMessageError(pivot.SupFormattedAdditionalTariff3Info, "Only 'Prov/Prog. Additional Tariff 1', 'Prov/Prog. Additional Tariff 2' and 'Prov/Prog. Tariff' should be filled when there are three supplementary tariffs on classification.");

			pivot.SupFormattedAdditionalTariff2 = "9903.85.03";
			pivot.SupFormattedAdditionalTariff3 = ZString.Empty;
			AssertNoMessageErrorContaining(pivot.SupFormattedAdditionalTariff3Info, "Tariff unable to be found.");
			AssertNoMessageError(pivot.SupFormattedAdditionalTariff3Info, "Only 'Prov/Prog. Additional Tariff 1', 'Prov/Prog. Additional Tariff 2' and 'Prov/Prog. Tariff' should be filled when there are three supplementary tariffs on classification.");

			pivot.SupFormattedAdditionalTariff4 = "9903.85.04";
			AssertHasMessageErrorContaining(pivot.SupFormattedAdditionalTariff4Info, "Tariff unable to be found.");
			AssertHasMessageError(pivot.SupFormattedAdditionalTariff4Info, "Only 'Prov/Prog. Additional Tariff 1', 'Prov/Prog. Additional Tariff 2', 'Prov/Prog. Additional Tariff 3' and 'Prov/Prog. Tariff' should be filled when there are four supplementary tariffs on classification.");

			pivot.SupFormattedAdditionalTariff3 = "9903.85.04";
			pivot.SupFormattedAdditionalTariff4 = ZString.Empty;
			AssertNoMessageErrorContaining(pivot.SupFormattedAdditionalTariff4Info, "Tariff unable to be found.");
			AssertNoMessageError(pivot.SupFormattedAdditionalTariff4Info, "Only 'Prov/Prog. Additional Tariff 1', 'Prov/Prog. Additional Tariff 2', 'Prov/Prog. Additional Tariff 3' and 'Prov/Prog. Tariff' should be filled when there are four supplementary tariffs on classification.");

			pivot.SupFormattedAdditionalTariff5 = "9903.85.05";
			AssertHasMessageErrorContaining(pivot.SupFormattedAdditionalTariff5Info, "Tariff unable to be found.");
			AssertHasMessageError(pivot.SupFormattedAdditionalTariff5Info, "Only 'Prov/Prog. Additional Tariff 1', 'Prov/Prog. Additional Tariff 2', 'Prov/Prog. Additional Tariff 3', 'Prov/Prog. Additional Tariff 4' and 'Prov/Prog. Tariff' should be filled when there are five supplementary tariffs on classification.");

			pivot.SupFormattedAdditionalTariff4 = "9903.85.05";
			pivot.SupFormattedAdditionalTariff5 = ZString.Empty;
			AssertNoMessageErrorContaining(pivot.SupFormattedAdditionalTariff5Info, "Tariff unable to be found.");
			AssertNoMessageError(pivot.SupFormattedAdditionalTariff5Info, "Only 'Prov/Prog. Additional Tariff 1', 'Prov/Prog. Additional Tariff 2', 'Prov/Prog. Additional Tariff 3', 'Prov/Prog. Additional Tariff 4' and 'Prov/Prog. Tariff' should be filled when there are five supplementary tariffs on classification.");
		}

		OrgSupplierPart part;
		OrgSupplierPart Part => part ?? (part = Factory.New<OrgSupplierPart>());

		delegate CusAttributeFilterCollection GetAttributesDelegate(CusClassPartPivot pivot);

		void AssertChildType(GetAttributesDelegate getAttributes)
		{
			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			CusClassPartPivot pivot1 = part.PivotsForBinding.AddNew();
			var relOrg1 = part.RelatedOrganisations.AddNew();
			var relOrg2 = part.RelatedOrganisations.AddNew();
			pivot1.CI_ChildType = "XXX";
			AssertHasErrorContaining(pivot1.CI_ChildTypeInfo, ListValidation.InvalidCodeError);
			pivot1.CI_ChildType = ZString.Empty;
			AssertHasError(pivot1.CI_ChildTypeInfo, "Classification Type is mandatory");
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertNoError(pivot1.CI_ChildTypeInfo, "Classification Type is mandatory");
			AssertNoErrorContaining(pivot1.CI_ChildTypeInfo, ListValidation.InvalidCodeError);
			CusClassPartPivot pivot2 = part.PivotsForBinding.AddNew();
			pivot1.CI_OH = relOrg1.OU_OH;
			pivot2.CI_OH = relOrg1.OU_OH;
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertNoError(pivot2.CI_ChildTypeInfo, pivot2.Validation.DuplicateAttributeForHTIWithoutAttributes);
			AssertNoError(pivot2.CI_ChildTypeInfo, pivot1.Validation.DuplicateAttributeForNoneHTI);
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertHasError(pivot2.CI_ChildTypeInfo, pivot2.Validation.DuplicateAttributeForHTIWithoutAttributes);
			AssertNoError(pivot2.CI_ChildTypeInfo, pivot1.Validation.DuplicateAttributeForNoneHTI);
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertNoError(pivot2.CI_ChildTypeInfo, pivot2.Validation.DuplicateAttributeForHTIWithoutAttributes);
			AssertHasError(pivot2.CI_ChildTypeInfo, pivot1.Validation.DuplicateAttributeForNoneHTI);
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot2.CI_OH = relOrg2.OU_OH;
			AssertHasError(pivot2.CI_ChildTypeInfo, pivot2.Validation.DuplicateAttributeForHTIWithoutAttributes);
			AssertNoError(pivot2.CI_ChildTypeInfo, pivot1.Validation.DuplicateAttributeForNoneHTI);
			pivot2.CI_OH = relOrg1.OU_OH;
			getAttributes(pivot2).AddNew();
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertNoError(pivot2.CI_ChildTypeInfo, pivot2.Validation.DuplicateAttributeForHTIWithoutAttributes);
			AssertNoError(pivot2.CI_ChildTypeInfo, pivot1.Validation.DuplicateAttributeForNoneHTI);
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertNoError(pivot2.CI_ChildTypeInfo, pivot2.Validation.DuplicateAttributeForHTIWithoutAttributes);
			AssertNoError(pivot2.CI_ChildTypeInfo, pivot1.Validation.DuplicateAttributeForNoneHTI);
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertNoError(pivot2.CI_ChildTypeInfo, pivot2.Validation.DuplicateAttributeForHTIWithoutAttributes);
			AssertHasError(pivot2.CI_ChildTypeInfo, pivot1.Validation.DuplicateAttributeForNoneHTI);
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot2.CI_OH = relOrg1.OU_OH;
			AssertNoError(pivot2.CI_ChildTypeInfo, pivot2.Validation.DuplicateAttributeForHTIWithoutAttributes);
			AssertNoError(pivot2.CI_ChildTypeInfo, pivot1.Validation.DuplicateAttributeForNoneHTI);
			CusClassPartPivot childPivot1 = pivot2.Children.AddNew();
			childPivot1.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			childPivot1 = pivot2.Children.AddNew();
			childPivot1.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			CusClassPartPivot childPivot2 = pivot2.Children.AddNew();
			childPivot2.CI_ChildType = ClassificationChildTypeList.Codes.Related;
			pivot2.Children.AddNew();
			childPivot2.CI_ChildType = ClassificationChildTypeList.Codes.Related;
			AssertNoError(childPivot1.CI_ChildTypeInfo, pivot2.Validation.DuplicateAttributeForHTIWithoutAttributes);
			AssertNoError(childPivot2.CI_ChildTypeInfo, pivot2.Validation.DuplicateAttributeForHTIWithoutAttributes);
		}

		void AssertDuplicateHTIWithAttributes(ZGuid relOrg1PK, ZGuid relOrg2PK, ZString attributeName, CusClassPartPivot pivot1, CusClassPartPivot pivot2, CusAttributeFilter attrib1, CusAttributeFilter attrib2, string messageError)
		{
			pivot1.CI_OH = relOrg1PK;
			pivot2.CI_OH = relOrg1PK;
			attrib1.BG_AttributeName = attributeName;
			attrib2.BG_AttributeName = attributeName;
			attrib1.BG_AttributeValue1 = "A";
			attrib2.BG_AttributeValue1 = "A";
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertHasError(pivot2.CI_ChildTypeInfo, messageError);
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertNoError(pivot2.CI_ChildTypeInfo, messageError);
			attrib1.BG_AttributeValue1 = "B";
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertNoError(pivot2.CI_ChildTypeInfo, messageError);
			attrib1.BG_AttributeValue1 = "A";
			pivot1.CI_OH = relOrg2PK;
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertNoError(pivot2.CI_ChildTypeInfo, messageError);
			pivot1.CI_OH = relOrg1PK;
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertHasError(pivot2.CI_ChildTypeInfo, messageError);
		}
	}
}

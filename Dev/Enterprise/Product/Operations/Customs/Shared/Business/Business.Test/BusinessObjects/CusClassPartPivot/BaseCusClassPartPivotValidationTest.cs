using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseCusClassPartPivotValidationTest : BusinessObjectValidationTestCase
	{
		public void TestParent()
		{
			BaseCusClassPartPivot parent = Factory.New<BaseCusClassPartPivot>();
			AssertEquals(parent.Validation.Parent, parent);
		}

		#region TestAttributesRequirement

		public void TestAttributesRequirement()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "DZ1234ZD";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "DZ5678ZD";
			var part = Factory.New<OrgSupplierPart>();
			var partRelate1 = part.RelatedOrganisations.AddOwner(org1);
			var partRelate2 = part.RelatedOrganisations.AddOwner(org2);
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
			string attrib1Message = BaseCusClassPartPivotValidation.GetAttributeIsRequiredMessage("Attribute 1", ClassificationTypeList.Codes.HTI, "NONE");
			AssertContains(message, attrib1Message);
			string attrib2Message = BaseCusClassPartPivotValidation.GetAttributeIsRequiredMessage("Attribute 2", ClassificationTypeList.Codes.HTI, "NONE");
			AssertContains(message, attrib2Message);
			string attrib3Message = BaseCusClassPartPivotValidation.GetAttributeIsRequiredMessage("Attribute 3", ClassificationTypeList.Codes.HTI, "NONE");
			string attrib1MessageWitRelated = BaseCusClassPartPivotValidation.GetAttributeIsRequiredMessage("Attribute 1", ClassificationTypeList.Codes.HTI, "DZ1234ZD");
			AssertContains(message, attrib1MessageWitRelated);
			string attrib2MessageWitRelated = BaseCusClassPartPivotValidation.GetAttributeIsRequiredMessage("Attribute 2", ClassificationTypeList.Codes.HTI, "DZ1234ZD");
			AssertContains(message, attrib2MessageWitRelated);
			string attrib3MessageWitRelated = BaseCusClassPartPivotValidation.GetAttributeIsRequiredMessage("Attribute 3", ClassificationTypeList.Codes.HTI, "DZ1234ZD");
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

			pivot1.CI_OH = partRelate1.OU_OH;
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot2.Validation.ValidateAll();
			AssertNoRowError(pivot2, attrib1Message);
			AssertNoRowError(pivot2, attrib2Message);
			AssertNoRowError(pivot2, attrib3Message);
			AssertHasRowError(pivot2, attrib1MessageWitRelated);
			AssertHasRowError(pivot2, attrib2MessageWitRelated);
			AssertHasRowError(pivot2, attrib3MessageWitRelated);
		}

		#endregion

		#region TestCheckCI_ChildType

		public void TestCheckCI_ChildType()
		{
			AssertChildType(x => x.Attributes1);
			AssertChildType(x => x.Attributes2);
			AssertChildType(x => x.Attributes3);
		}

		delegate CusAttributeFilterCollection GetAttributesDelegate(BaseCusClassPartPivot pivot);

		void AssertChildType(GetAttributesDelegate getAttributes)
		{
			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			BaseCusClassPartPivot pivot1 = part.PivotsForBinding.AddNew();
			var relOrg1 = part.RelatedOrganisations.AddNew();
			var relOrg2 = part.RelatedOrganisations.AddNew();
			pivot1.CI_ChildType = "XXX";
			AssertHasErrorContaining(pivot1.CI_ChildTypeInfo, ListValidation.InvalidCodeError);

			pivot1.CI_ChildType = ZString.Empty;
			AssertHasError(pivot1.CI_ChildTypeInfo, BaseCusClassPartPivotValidation.ClassificationTypeIsMandatory);

			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertNoError(pivot1.CI_ChildTypeInfo, BaseCusClassPartPivotValidation.ClassificationTypeIsMandatory);
			AssertNoErrorContaining(pivot1.CI_ChildTypeInfo, ListValidation.InvalidCodeError);

			BaseCusClassPartPivot pivot2 = part.PivotsForBinding.AddNew();
			pivot1.CI_OH = relOrg1.OU_OH;
			pivot2.CI_OH = relOrg1.OU_OH;
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertNoError(pivot2.CI_ChildTypeInfo, pivot1.Validation.DuplicateAttributeForHTIWithoutAttributes);
			AssertNoError(pivot2.CI_ChildTypeInfo, pivot1.Validation.DuplicateAttributeForNoneHTI);

			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertHasError(pivot2.CI_ChildTypeInfo, pivot1.Validation.DuplicateAttributeForHTIWithoutAttributes);
			AssertNoError(pivot2.CI_ChildTypeInfo, pivot1.Validation.DuplicateAttributeForNoneHTI);

			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertNoError(pivot2.CI_ChildTypeInfo, pivot1.Validation.DuplicateAttributeForHTIWithoutAttributes);
			AssertHasError(pivot2.CI_ChildTypeInfo, pivot1.Validation.DuplicateAttributeForNoneHTI);

			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot2.CI_OH = relOrg2.OU_OH;
			AssertHasError(pivot2.CI_ChildTypeInfo, pivot1.Validation.DuplicateAttributeForHTIWithoutAttributes);
			AssertNoError(pivot2.CI_ChildTypeInfo, pivot1.Validation.DuplicateAttributeForNoneHTI);

			pivot2.CI_OH = relOrg1.OU_OH;
			getAttributes(pivot2).AddNew();
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertNoError(pivot2.CI_ChildTypeInfo, pivot1.Validation.DuplicateAttributeForHTIWithoutAttributes);
			AssertNoError(pivot2.CI_ChildTypeInfo, pivot1.Validation.DuplicateAttributeForNoneHTI);

			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertNoError(pivot2.CI_ChildTypeInfo, pivot1.Validation.DuplicateAttributeForHTIWithoutAttributes);
			AssertNoError(pivot2.CI_ChildTypeInfo, pivot1.Validation.DuplicateAttributeForNoneHTI);

			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertNoError(pivot2.CI_ChildTypeInfo, pivot1.Validation.DuplicateAttributeForHTIWithoutAttributes);
			AssertHasError(pivot2.CI_ChildTypeInfo, pivot1.Validation.DuplicateAttributeForNoneHTI);

			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot2.CI_OH = relOrg1.OU_OH;
			AssertNoError(pivot2.CI_ChildTypeInfo, pivot1.Validation.DuplicateAttributeForHTIWithoutAttributes);
			AssertNoError(pivot2.CI_ChildTypeInfo, pivot1.Validation.DuplicateAttributeForNoneHTI);

			//BaseCusClassPartPivot childPivot1 = pivot2.Children.AddNew();
			//childPivot1.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;

			//childPivot1 = pivot2.Children.AddNew();
			//childPivot1.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;

			//CusClassPartPivot childPivot2 = pivot2.Children.AddNew();
			//childPivot2.CI_ChildType = ClassificationChildTypeList.Codes.Related;

			//pivot2.Children.AddNew();
			//childPivot2.CI_ChildType = ClassificationChildTypeList.Codes.Related;

			//AssertNoError(childPivot1.CI_ChildTypeInfo, ValidationConstants.Part.DuplicateAttributeForHTIWithoutAttributes);
			//AssertNoError(childPivot2.CI_ChildTypeInfo, ValidationConstants.Part.DuplicateAttributeForHTIWithoutAttributes);
		}

		public void TestCheckOverlaps()
		{
			var part = Factory.New<OrgSupplierPart>();
			var relOrg1 = part.RelatedOrganisations.AddNew();

			var pivot1 = part.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = "HTI";
			pivot1.CI_TariffNum = "1234567890";
			pivot1.CI_DateStart = ZDateTime.Now;
			pivot1.CI_DateEnd = ZDateTime.Now.AddDays(10);
			pivot1.CI_OH = relOrg1.OU_OH;

			var pivot2 = part.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = "HTI";
			pivot2.CI_TariffNum = "1234567890";
			pivot2.CI_DateStart = ZDateTime.Now.AddDays(20);
			pivot2.CI_DateEnd = ZDateTime.Now.AddDays(30);
			pivot2.CI_OH = relOrg1.OU_OH;
			AssertNoErrorContaining(pivot2.CI_ChildTypeInfo, pivot1.Validation.DuplicateAttributeForHTIWithoutAttributes);

			pivot2.CI_DateStart = ZDateTime.Now.AddDays(5);
			pivot2.CI_DateEnd = ZDateTime.Now.AddDays(18);
			pivot2.CI_TariffNum = "1234567899";
			pivot2.CI_ChildType = "HTI";
			pivot1.CI_TariffNum = "1234567899";
			pivot2.Validation.ValidateAll();
			AssertNoErrorContaining(pivot2.CI_ChildTypeInfo, pivot1.Validation.DuplicateAttributeForHTIWithoutAttributes);

			var pivot3 = part.PivotsForBinding.AddNew();
			pivot3.CI_ChildType = "HTI";
			pivot3.CI_TariffNum = "1234567890";
			pivot3.CI_DateStart = ZDateTime.Now.AddDays(5);
			pivot3.CI_DateEnd = ZDateTime.Now.AddDays(18);
			pivot3.CI_TariffNum = "1234567899";
			pivot3.CI_ChildType = "HTI";
			pivot3.Validation.ValidateAll();
			AssertHasErrorContaining(pivot3.CI_ChildTypeInfo, pivot1.Validation.DuplicateAttributeForHTIWithoutAttributes);
		}

		public void TestDuplicateHTIWithAttributes()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "IMP1";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "IMP2";
			var part = Factory.New<OrgSupplierPart>();
			var relOrg1 = part.RelatedOrganisations.AddOrganisationIfNotExist(org1.PK, Enterprise.MasterFiles.Business.OrgPartRelation.RelationshipTypes.Owner);
			var relOrg2 = part.RelatedOrganisations.AddOrganisationIfNotExist(org2.PK, Enterprise.MasterFiles.Business.OrgPartRelation.RelationshipTypes.Owner);

			var pivot1 = part.PivotsForBinding.AddNew();
			var pivot2 = part.PivotsForBinding.AddNew();
			var attrib1 = pivot1.Attributes1.AddNew();
			var attrib2 = pivot2.Attributes1.AddNew();
			var attribute1Name = nameof(CusAttributeFilter.AttributeFilterName.AT1);
			var attribute2Name = nameof(CusAttributeFilter.AttributeFilterName.AT2);
			var attribute3Name = nameof(CusAttributeFilter.AttributeFilterName.AT3);
			AssertDuplicateHTIWithAttributes(relOrg1.OU_OH, relOrg2.OU_OH, attribute1Name, pivot1, pivot2, attrib1, attrib2, BaseCusClassPartPivotValidation.DuplicateAttributeForHTIWithAttributes("A", "NO VALUE", "NO VALUE", "IMP1"));
			AssertDuplicateHTIWithAttributes(relOrg1.OU_OH, relOrg2.OU_OH, attribute2Name, pivot1, pivot2, attrib1, attrib2, BaseCusClassPartPivotValidation.DuplicateAttributeForHTIWithAttributes("NO VALUE", "A", "NO VALUE", "IMP1"));
			AssertDuplicateHTIWithAttributes(relOrg1.OU_OH, relOrg2.OU_OH, attribute3Name, pivot1, pivot2, attrib1, attrib2, BaseCusClassPartPivotValidation.DuplicateAttributeForHTIWithAttributes("NO VALUE", "NO VALUE", "A", "IMP1"));

			var attrib3 = pivot1.Attributes2.AddNew();
			var attrib4 = pivot2.Attributes2.AddNew();
			attrib3.BG_AttributeValue1 = "C";
			attrib4.BG_AttributeValue1 = "C";
			AssertDuplicateHTIWithAttributes(relOrg1.OU_OH, relOrg2.OU_OH, attribute1Name, pivot1, pivot2, attrib1, attrib2, BaseCusClassPartPivotValidation.DuplicateAttributeForHTIWithAttributes("A", "C", "NO VALUE", "IMP1"));
			AssertDuplicateHTIWithAttributes(relOrg1.OU_OH, relOrg2.OU_OH, attribute3Name, pivot1, pivot2, attrib1, attrib2, BaseCusClassPartPivotValidation.DuplicateAttributeForHTIWithAttributes("NO VALUE", "C", "A", "IMP1"));

			attrib3.BG_AttributeName = attribute3Name;
			attrib4.BG_AttributeName = attribute3Name;
			AssertDuplicateHTIWithAttributes(relOrg1.OU_OH, relOrg2.OU_OH, attribute1Name, pivot1, pivot2, attrib1, attrib2, BaseCusClassPartPivotValidation.DuplicateAttributeForHTIWithAttributes("A", "NO VALUE", "C", "IMP1"));
			AssertDuplicateHTIWithAttributes(relOrg1.OU_OH, relOrg2.OU_OH, attribute2Name, pivot1, pivot2, attrib1, attrib2, BaseCusClassPartPivotValidation.DuplicateAttributeForHTIWithAttributes("NO VALUE", "A", "C", "IMP1"));

			var attrib5 = pivot1.Attributes3.AddNew();
			var attrib6 = pivot2.Attributes3.AddNew();
			attrib5.BG_AttributeValue1 = "D";
			attrib6.BG_AttributeValue1 = "D";
			attrib5.BG_AttributeName = attribute2Name;
			attrib6.BG_AttributeName = attribute2Name;

			AssertDuplicateHTIWithAttributes(relOrg1.OU_OH, relOrg2.OU_OH, attribute1Name, pivot1, pivot2, attrib1, attrib2, BaseCusClassPartPivotValidation.DuplicateAttributeForHTIWithAttributes("A", "D", "C", "IMP1"));
		}

		void AssertDuplicateHTIWithAttributes(ZGuid relOrg1PK, ZGuid relOrg2PK, ZString attributeName, BaseCusClassPartPivot pivot1, BaseCusClassPartPivot pivot2, CusAttributeFilter attrib1, CusAttributeFilter attrib2, string messageError)
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

		#endregion

		#region TestCheckCI_CC

		public void TestCheckCI_CC()
		{
			BaseCusClassPartPivot pivot = Part.PivotsForBinding.AddNew();
			pivot.Validation.ValidateCI_CC();
			AssertHasError(pivot.CI_CCInfo, BaseCusClassPartPivotValidation.OneOfTariffOrClassificationIsMandatory);
			pivot.CI_CC = Factory.New<BaseCusClassification>().PK;
			AssertNoError(pivot.CI_CCInfo, BaseCusClassPartPivotValidation.OneOfTariffOrClassificationIsMandatory);
			pivot.CI_CC = ZGuid.Empty;
			AssertHasError(pivot.CI_CCInfo, BaseCusClassPartPivotValidation.OneOfTariffOrClassificationIsMandatory);
			pivot.CI_TariffNum = "00000000";
			AssertNoError(pivot.CI_CCInfo, BaseCusClassPartPivotValidation.OneOfTariffOrClassificationIsMandatory);

			var invalidClass = Factory.New<BaseCusClassification>();
			invalidClass.CC_LookupCode = "INVALIDCLASS";
			invalidClass.CC_IsActive = false;
			pivot.CI_TariffNum = ZString.Empty;
			pivot.CI_CC = invalidClass.PK;
			AssertHasWarningContaining(pivot.CI_CCInfo, BaseCusClassPartPivotValidation.WarnThatClassificationNoActive);
		}

		#endregion

		#region TestCheckCI_TariffNum

		public void TestCheckCI_TariffNum()
		{
			BaseCusClassPartPivot pivot = Part.PivotsForBinding.AddNew();
			pivot.Validation.ValidateCI_TariffNum();
			AssertHasError(pivot.CI_TariffNumInfo, BaseCusClassPartPivotValidation.OneOfTariffOrClassificationIsMandatory);
			pivot.CI_CC = Factory.New<BaseCusClassification>().PK;
			AssertNoError(pivot.CI_TariffNumInfo, BaseCusClassPartPivotValidation.OneOfTariffOrClassificationIsMandatory);
			pivot.CI_CC = ZGuid.Empty;
			AssertHasError(pivot.CI_TariffNumInfo, BaseCusClassPartPivotValidation.OneOfTariffOrClassificationIsMandatory);
			pivot.CI_TariffNum = "00000000";
			AssertNoError(pivot.CI_TariffNumInfo, BaseCusClassPartPivotValidation.OneOfTariffOrClassificationIsMandatory);
		}

		#endregion

		#region TestCheckDateStart

		public void TestCheckDateStart()
		{
			var part = Factory.New<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = "1200000";

			AssertNoErrorContaining(pivot.CI_DateStartInfo, BaseCusClassPartPivotValidation.StartDateMustBeforeEndDate);
			pivot.CI_DateEnd = ZDateTime.Now.AddDays(10);
			pivot.CI_DateStart = ZDateTime.Now.AddDays(20);
			AssertHasErrorContaining(pivot.CI_DateStartInfo, BaseCusClassPartPivotValidation.StartDateMustBeforeEndDate);
			pivot.CI_DateStart = ZDateTime.Now.AddDays(5);
			AssertNoErrorContaining(pivot.CI_DateStartInfo, BaseCusClassPartPivotValidation.StartDateMustBeforeEndDate);
		}

		#endregion

		#region TestCheckDateEnd

		public void TestCheckDateEnd()
		{
			var part = Factory.New<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = "1200000";

			AssertNoErrorContaining(pivot.CI_DateEndInfo, BaseCusClassPartPivotValidation.StartDateMustBeforeEndDate);
			pivot.CI_DateStart = ZDateTime.Now.AddDays(20);
			pivot.CI_DateEnd = ZDateTime.Now.AddDays(10);
			AssertHasErrorContaining(pivot.CI_DateEndInfo, BaseCusClassPartPivotValidation.StartDateMustBeforeEndDate);
			pivot.CI_DateEnd = ZDateTime.Now.AddDays(30);
			AssertNoErrorContaining(pivot.CI_DateEndInfo, BaseCusClassPartPivotValidation.StartDateMustBeforeEndDate);
		}

		#endregion

		OrgSupplierPart Part => part ?? (part = Factory.New<OrgSupplierPart>());
		OrgSupplierPart part;
	}
}

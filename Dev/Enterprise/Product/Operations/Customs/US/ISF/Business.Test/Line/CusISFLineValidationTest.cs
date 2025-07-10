using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class CusISFLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckBL_RN_NKGoodsOrigin()
		{
			header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			line.BL_RN_NKGoodsOrigin = ZString.Empty;
			AssertHasMessageErrorContaining(line.BL_RN_NKGoodsOriginInfo, MandatoryValidation.YouHaveNotEntered);
			line.BL_RN_NKGoodsOrigin = "Z!";
			AssertNoMessageErrorContaining(line.BL_RN_NKGoodsOriginInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(line.BL_RN_NKGoodsOriginInfo, ListValidation.InvalidCodeMessageError);
			header.BF_EntryType = SubmissionTypeList.Codes.ISF5;
			line.BL_RN_NKGoodsOrigin = ZString.Empty;
			AssertNoMessageErrorContaining(line.BL_RN_NKGoodsOriginInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoWarningContaining(line.BL_RN_NKGoodsOriginInfo, MandatoryValidation.YouHaveNotEntered);
			line.BL_RN_NKGoodsOrigin = "Z!";
			AssertNoMessageErrorContaining(line.BL_RN_NKGoodsOriginInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasWarningContaining(line.BL_RN_NKGoodsOriginInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckBL_ManufacturerDocAddressPK()
		{
			header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			line.BL_ManufacturerDocAddressPK = ZGuid.Empty;
			AssertHasMessageErrorContaining(line.BL_ManufacturerDocAddressPKInfo, CusISFLineValidation.ManufacturerRequired);
			line.BL_ManufacturerDocAddressPK = manufacturerAddress.PK;
			AssertNoMessageErrorContaining(line.BL_ManufacturerDocAddressPKInfo, CusISFLineValidation.ManufacturerRequired);
			header.BF_EntryType = SubmissionTypeList.Codes.ISF5;
			line.BL_ManufacturerDocAddressPK = ZGuid.Empty;
			AssertNoNotifications(line.BL_ManufacturerDocAddressPKInfo);
		}

		public void TestCheckBL_FormattedHarmonisedNum()
		{
			header.BF_NumOfHarmChars = "";
			line.BL_FormattedHarmonisedNum = ZString.Empty;
			AssertHasMessageErrorContaining(line.BL_FormattedHarmonisedNumInfo, MandatoryValidation.YouHaveNotEntered);
			line.BL_FormattedHarmonisedNum = "1010101010";
			AssertNoMessageErrorContaining(line.BL_FormattedHarmonisedNumInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrors(line.BL_FormattedHarmonisedNumInfo);
			line.BL_FormattedHarmonisedNum = "22221010";
			AssertHasMessageErrorContaining(line.BL_FormattedHarmonisedNumInfo, "too short");
			header.BF_NumOfHarmChars = NumberOfHarmonizedDigitsList.Codes.Eight;
			line.BL_FormattedHarmonisedNum = "22221011";
			AssertNoMessageErrorContaining(line.BL_FormattedHarmonisedNumInfo, "too short");
			line.BL_FormattedHarmonisedNum = "111110";
			AssertHasMessageErrorContaining(line.BL_FormattedHarmonisedNumInfo, "too short");
			header.BF_NumOfHarmChars = NumberOfHarmonizedDigitsList.Codes.Six;
			line.BL_FormattedHarmonisedNum = "111111";
			AssertNoMessageErrorContaining(line.BL_FormattedHarmonisedNumInfo, "too short");
			line.BL_FormattedHarmonisedNum = "11111";
			AssertHasMessageErrorContaining(line.BL_FormattedHarmonisedNumInfo, "too short");
		}

		[TestDate(2016, 4, 6)]
		public void TestCheckBL_TextProductCode()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			OrgHeader importer1 = newFactory.New<OrgHeader>();
			importer1.OH_Code = "IMP000000";
			importer1.OH_FullName = "IMP000000";
			importer1.MainAddress.OA_Address1 = "IMPORTER1 ADDRESS 1";
			header.BF_OH_Importer = importer1.PK;
			US.Business.OrgSupplierPart part1 = newFactory.New<US.Business.OrgSupplierPart>();
			part1.RelatedOrganisations.AddOrganisationIfNotExist(importer1.PK, OrgPartRelation.RelationshipTypes.Owner);
			part1.OP_PartNum = "DUMMY2343";
			var pivot1 = part1.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot1.CI_OH = part1.RelatedOrganisations[0].OU_OH;
			pivot1.CI_TariffNum = "1010231031";
			pivot1.Attributes1.AddNew().BG_AttributeValue1 = "1";
			pivot1.Attributes2.AddNew().BG_AttributeValue1 = "2";
			pivot1.Attributes3.AddNew().BG_AttributeValue1 = "3";
			pivot1.CI_DateStart = new ZDateTime(2016, 4, 1);
			pivot1.CI_DateEnd = new ZDateTime(2016, 4, 30);
			OrgHeader importer2 = newFactory.New<OrgHeader>();
			importer2.OH_Code = "IMP111111";
			importer2.MainAddress.OA_Address1 = "IMPORTER2 ADDRESS 1";
			US.Business.OrgSupplierPart part2 = newFactory.New<US.Business.OrgSupplierPart>();
			part2.RelatedOrganisations.AddOrganisationIfNotExist(importer2.PK, OrgPartRelation.RelationshipTypes.Owner);
			part2.OP_PartNum = "DUMMY6344";
			var pivot2 = part2.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot2.CI_OH = part2.RelatedOrganisations[0].OU_OH;
			pivot2.CI_TariffNum = "1010231032";
			pivot2.Attributes1.AddNew().BG_AttributeValue1 = "4";
			pivot2.Attributes2.AddNew().BG_AttributeValue1 = "5";
			pivot2.Attributes3.AddNew().BG_AttributeValue1 = "6";
			pivot2.CI_DateStart = new ZDateTime(2016, 4, 1);
			pivot2.CI_DateEnd = new ZDateTime(2016, 4, 30);
			OrgHeader supplier = newFactory.New<OrgHeader>();
			supplier.OH_Code = "SUP000000";
			supplier.OH_FullName = "SUP000000";
			header.SellingParty.OrganisationPK = supplier.PK;
			newFactory.Save();
			line.BL_TextProductCode = ZString.Empty;
			AssertNoWarning(line.BL_TextProductCodeInfo, CusISFLineValidation.ProductCodeNotBelongToImporter);
			AssertNoWarningContaining(line.BL_TextProductCodeInfo, "Cannot match classification for product");
			line.BL_TextProductCode = part1.OP_PartNum;
			AssertHasWarningContaining(line.BL_TextProductCodeInfo, "Cannot match classification for product");
			AssertHasWarning(line.BL_TextProductCodeInfo, "Cannot match classification for product (DUMMY2343) based on Importer (IMP000000), Supplier (SUP000000), Part Attrib. 1 (), Part Attrib. 2 (), Part Attrib. 3 () and Effective Date (6-Apr-16).");
			line.BL_PartAttrib1 = "1";
			AssertHasWarning(line.BL_TextProductCodeInfo, "Cannot match classification for product (DUMMY2343) based on Importer (IMP000000), Supplier (SUP000000), Part Attrib. 1 (1), Part Attrib. 2 (), Part Attrib. 3 () and Effective Date (6-Apr-16).");
			line.BL_PartAttrib2 = "2";
			AssertHasWarning(line.BL_TextProductCodeInfo, "Cannot match classification for product (DUMMY2343) based on Importer (IMP000000), Supplier (SUP000000), Part Attrib. 1 (1), Part Attrib. 2 (2), Part Attrib. 3 () and Effective Date (6-Apr-16).");
			line.BL_PartAttrib3 = "3";
			AssertNoWarningContaining(line.BL_TextProductCodeInfo, "Cannot match classification for product");
			AssertNoError(line.BL_PartAttrib1Info, "The part 'DUMMY2343' is not set up to use Part Attribute 1 with the Importer 'IMP000000'. Please either remove the value '1' from the Part Attribute 1 field, or set up the Product and Importer to use Part Attribute 1.");
			AssertNoError(line.BL_PartAttrib2Info, "The part 'DUMMY2343' is not set up to use Part Attribute 2 with the Importer 'IMP000000'. Please either remove the value '2' from the Part Attribute 2 field, or set up the Product and Importer to use Part Attribute 2.");
			AssertNoError(line.BL_PartAttrib3Info, "The part 'DUMMY2343' is not set up to use Part Attribute 3 with the Importer 'IMP000000'. Please either remove the value '3' from the Part Attribute 3 field, or set up the Product and Importer to use Part Attribute 3.");
			importer1 = Factory.Load<OrgHeader>(importer1.PK);
			importer1.CompanyData.OB_IMUsedBondedWhs = true;
			line.Validation.ValidateAll();
			AssertHasError(line.BL_PartAttrib1Info, "The part 'DUMMY2343' is not set up to use Part Attribute 1 with the Importer 'IMP000000'. Please either remove the value '1' from the Part Attribute 1 field, or set up the Product and Importer to use Part Attribute 1.");
			AssertHasError(line.BL_PartAttrib2Info, "The part 'DUMMY2343' is not set up to use Part Attribute 2 with the Importer 'IMP000000'. Please either remove the value '2' from the Part Attribute 2 field, or set up the Product and Importer to use Part Attribute 2.");
			AssertHasError(line.BL_PartAttrib3Info, "The part 'DUMMY2343' is not set up to use Part Attribute 3 with the Importer 'IMP000000'. Please either remove the value '3' from the Part Attribute 3 field, or set up the Product and Importer to use Part Attribute 3.");
			line.BL_TextProductCode = part2.OP_PartNum;
			AssertHasWarning(line.BL_TextProductCodeInfo, CusISFLineValidation.ProductCodeNotBelongToImporter);
			AssertNoWarningContaining(line.BL_TextProductCodeInfo, "Cannot match classification for product");
		}

		CusISFHeader header;
		CusISFLine line;
		ISFDocAddress manufacturerAddress;
		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusISFHeader>();
			line = header.Lines.AddNew();
			manufacturerAddress = header.DocAddresses.CreateWithAddressType(DocAddressType.Manufacturer);
		}
	}
}

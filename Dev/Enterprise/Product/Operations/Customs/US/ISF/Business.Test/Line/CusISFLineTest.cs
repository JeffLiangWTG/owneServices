using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	[TestedType(typeof(CusISFLine))]
	sealed class CusISFLineTest : EnterpriseBusinessObjectTestCase
	{
		public void TestPartSyncManagerRefreshWhenBL_TextProductCodeChange()
		{
			var importer = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABIGAS");
			var product = Factory.New<US.Business.OrgSupplierPart>();
			product.OP_PartNum = "DWG Test Product";
			product.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			var header = Factory.New<CusISFHeader>();
			header.BF_OH_Importer = importer.PK;

			var line = header.Lines.AddNew();
			line.BL_LineType = ISFLineTypeList.Codes.Component;
			line.BL_TextProductCode = product.OP_PartNum;
			AssertNotEquals(product.PK, line.BL_OP);

			var line1 = header.Lines.AddNew();
			line1.BL_TextProductCode = product.OP_PartNum;
			AssertEquals(product.PK, line1.BL_OP);
		}

		public void TestIHaveAdditionalDataForBorderWise()
		{
			var header = Factory.New<CusISFHeader>();
			var line = header.Lines.AddNew();
			var addData = ((IHaveAdditionalDataForBorderWise)line).GetAdditionalDataForBorderWise(CusISFLine.Schema.BL_FormattedHarmonisedNum);
			AssertEquals("2010.25.6032", addData.FormatBorderWiseInput("2010256032"));
			AssertEquals("CS00190306: the extra space would count as one char so it should be removed and formatted.", "2010.25.6032", addData.FormatBorderWiseInput("20102560 32"));
		}

		public void TestUpdateDataFromProduct()
		{
			USCTariff tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "1010231032";
			OrgHeader importer = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABIGAS");
			US.Business.OrgSupplierPart part = Factory.New<US.Business.OrgSupplierPart>();
			part.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			CusClassification classification = Factory.New<CusClassification>();
			classification.CC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			classification.CC_TariffNum = "1010231032";
			CusClassPartPivot cusClassPartPivot = part.PivotsForBinding.AddNew();
			cusClassPartPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			cusClassPartPivot.CI_CC = classification.PK;
			cusClassPartPivot.CD_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Australia;
			part.OP_PartNum = "Part123ZZ";
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.BF_OH_Importer = importer.PK;
			CusISFLine line = header.Lines.AddNew();
			line.BL_TextProductCode = "Part123ZZ";
			AssertEquals("1010231032", line.BL_HarmonisedNum);
			AssertEquals(Core.Constants.CountryCodes.Australia, line.BL_RN_NKGoodsOrigin);
			AssertEquals(part.PK, line.BL_OP);
			line.BL_HarmonisedNum = "2010256032";
			AssertEquals("2010256032", line.BL_HarmonisedNum);
			AssertEquals(Core.Constants.CountryCodes.Australia, line.BL_RN_NKGoodsOrigin);
			AssertEquals(part.PK, line.BL_OP);
			line.BL_RN_NKGoodsOrigin = Core.Constants.CountryCodes.UnitedKingdom;
			AssertEquals("2010256032", line.BL_HarmonisedNum);
			AssertEquals(Core.Constants.CountryCodes.UnitedKingdom, line.BL_RN_NKGoodsOrigin);
			AssertEquals(part.PK, line.BL_OP);
		}

		public void TestPartSynchUpdate()
		{
			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "1010231032";
			var importer = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABIGAS");
			var classification = Factory.New<CusClassification>();
			classification.CC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			classification.CC_TariffNum = "1010231032";
			US.Business.OrgSupplierPart part1 = Factory.New<US.Business.OrgSupplierPart>();
			part1.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			var cusClassPart1Pivot = part1.PivotsForBinding.AddNew();
			cusClassPart1Pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			cusClassPart1Pivot.CI_CC = classification.PK;
			cusClassPart1Pivot.CD_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Australia;
			part1.OP_PartNum = "PART123ZZ";
			US.Business.OrgSupplierPart part2 = Factory.New<US.Business.OrgSupplierPart>();
			part2.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			var cusClassPart2Pivot = part2.PivotsForBinding.AddNew();
			cusClassPart2Pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			cusClassPart2Pivot.CI_CC = classification.PK;
			cusClassPart2Pivot.CD_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Australia;
			part2.OP_PartNum = "PART456GGG";
			var header = Factory.New<CusISFHeader>();
			header.BF_OH_Importer = importer.PK;
			CusISFLine line = header.Lines.AddNew();
			line.BL_TextProductCode = "PART123ZZ";
			AssertEquals("1010231032", line.BL_HarmonisedNum);
			AssertEquals(Core.Constants.CountryCodes.Australia, line.BL_RN_NKGoodsOrigin);
			AssertEquals(part1.PK, line.BL_OP);
			AssertEquals(part1, line.USSupplierPart);
			line.BL_OP = part2.PK;
			AssertEquals("1010231032", line.BL_HarmonisedNum);
			AssertEquals(Core.Constants.CountryCodes.Australia, line.BL_RN_NKGoodsOrigin);
			AssertEquals(part2, line.USSupplierPart);
		}

		public void TestBL_FormattedHarmonisedNum()
		{
			CusISFLine line = Factory.New<CusISFLine>();
			line.BL_HarmonisedNum = "101 020 3010";
			AssertEquals("1010.20.3010", line.BL_FormattedHarmonisedNum);
			AssertEquals("1010203010", line.BL_HarmonisedNum);
			line.BL_FormattedHarmonisedNum = "10.1 569 3.534";
			AssertEquals("1015.69.3534", line.BL_FormattedHarmonisedNum);
			AssertEquals("1015693534", line.BL_HarmonisedNum);
			AssertEquals(CusISFLine.Schema.BL_HarmonisedNumMaxLength + 2, line.BL_FormattedHarmonisedNumInfo.MaxLength);
		}

		public void TestBL_RN_NKGoodsOrigin()
		{
			var header = Factory.New<CusISFHeader>();
			var line1 = header.Lines.AddNew();
			line1.BL_RN_NKGoodsOrigin = "US";
			var line2 = line1.AddChildLine();
			line2.BL_RN_NKGoodsOrigin = "CA";
			var line3 = line1.AddChildLine();
			line3.BL_RN_NKGoodsOrigin = "";
			AssertEquals("BL_RN_NKGoodsOrigin", "US", line1.BL_RN_NKGoodsOrigin);
			AssertEquals("BL_RN_NKGoodsOrigin", "CA", line2.BL_RN_NKGoodsOrigin);
			AssertEquals("BL_RN_NKGoodsOrigin", "US", line3.BL_RN_NKGoodsOrigin);
		}

		public void TestDeleteProductLines()
		{
			var header = Factory.New<CusISFHeader>();
			var line1 = header.Lines.AddNew();
			var line2 = line1.AddChildLine();
			var line3 = line1.AddChildLine();
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var existingLines = newFactory.Load<CusISFLine>(new ZQuery(CusISFLineSchema.BL_BF, header.PK)).OrderBy(x => x.BL_BL_Parent.IsEmpty ? 0 : 1).ThenBy(x => x.PK).ToArray();
			AssertNoExceptionThrown(() => existingLines.DeleteAll());
			newFactory.Save();
			line1.Reload();
			AssertEquals(true, line1.IsDeleted);
			line2.Reload();
			AssertEquals(true, line2.IsDeleted);
			line3.Reload();
			AssertEquals(true, line3.IsDeleted);
		}

		public void TestChildLines()
		{
			var header = Factory.New<CusISFHeader>();
			var line1 = header.Lines.AddNew();
			var line2 = line1.AddChildLine();
			var line3 = line1.AddChildLine();
			AssertEquals("BL_BL_Parent", line1.PK, line2.BL_BL_Parent);
			AssertEquals("BL_LineType", "COM", line2.BL_LineType);
			AssertEquals("BL_BL_Parent", line1.PK, line3.BL_BL_Parent);
			AssertEquals("BL_LineType", "COM", line3.BL_LineType);
			AssertCollectionContains("line2 should belong to the header", line2, header.Lines);
			AssertCollectionContains("line3 should belong to the header", line3, header.Lines);
			AssertEquals("ChildLines", 2, line1.ChildLines.Count());
			AssertEquals("ChildLines", 0, line2.ChildLines.Count());
			AssertEquals("ChildLines", 0, line3.ChildLines.Count());
			line1.Delete();
			Assert("ChildLines should be deleted when parent line is deleted", line2.IsDeleted);
			Assert("ChildLines should be deleted when parent line is deleted", line3.IsDeleted);
		}

		public void TestParentTariffLine()
		{
			var line1 = Factory.New<CusISFLine>();
			var line2 = Factory.New<CusISFLine>();
			line2.BL_BL_Parent = line1.PK;
			AssertEquals("ParentTariffLine", line1, line2.ParentTariffLine);
		}

		public void TestProductRelatedLines()
		{
			var header = Factory.New<CusISFHeader>();
			var line1 = header.Lines.AddNew();
			var line2 = line1.AddProductRelatedLine();
			var line3 = line1.AddProductRelatedLine();
			AssertEquals("BL_BL_Parent", line1.PK, line2.BL_BL_Parent);
			AssertEquals("BL_LineType", "REL", line2.BL_LineType);
			AssertEquals("BL_BL_Parent", line1.PK, line3.BL_BL_Parent);
			AssertEquals("BL_LineType", "REL", line3.BL_LineType);
			AssertCollectionContains("line2 should belong to the header", line2, header.Lines);
			AssertCollectionContains("line3 should belong to the header", line3, header.Lines);
			AssertEquals("ProductRelatedLines", 2, line1.ProductRelatedLines.Count());
			AssertEquals("ProductRelatedLines", 0, line2.ProductRelatedLines.Count());
			AssertEquals("ProductRelatedLines", 0, line3.ProductRelatedLines.Count());
			line1.Delete();
			Assert("ProductRelatedLines should be deleted when parent line is deleted", line2.IsDeleted);
			Assert("ProductRelatedLines should be deleted when parent line is deleted", line3.IsDeleted);
		}

		public void TestIsComponentLine()
		{
			var line1 = Factory.New<CusISFLine>();
			var line2 = Factory.New<CusISFLine>();
			line2.BL_BL_Parent = line1.PK;
			line2.BL_LineType = ISFLineTypeList.Codes.Component;
			Assert("IsComponentLine", !line1.IsComponentLine);
			Assert("IsComponentLine", line2.IsComponentLine);
		}

		public void TestIsRelatedLine()
		{
			var header = Factory.New<CusISFHeader>();
			var line1 = header.Lines.AddNew();
			var line2 = header.Lines.AddNew();
			line2.BL_BL_Parent = line1.PK;
			line2.BL_LineType = ISFLineTypeList.Codes.Related;
			Assert("IsRelatedLine", !line1.IsRelatedLine);
			Assert("IsRelatedLine", line2.IsRelatedLine);
		}

		public void TestBL_TextProductCode()
		{
			var line1 = Factory.New<CusISFLine>();
			line1.BL_TextProductCode = "Product";
			var line2 = Factory.New<CusISFLine>();
			AssertEquals("BL_TextProductCode", ZString.Empty, line2.BL_TextProductCode);
			line2.BL_BL_Parent = line1.PK;
			AssertEquals("BL_TextProductCode", "Product", line2.BL_TextProductCode);
		}

		public void TestBL_TextProductCode_CanBeSetByCustomer()
		{
			var line1 = Factory.New<CusISFLine>();
			var line2 = Factory.New<CusISFLine>();
			line2.BL_BL_Parent = line1.PK;
			Assert("BL_TextProductCode_CanBeSetByCustomer", line1.BL_TextProductCode_CanBeSetByCustomer);
			Assert("BL_TextProductCode_CanBeSetByCustomer", !line2.BL_TextProductCode_CanBeSetByCustomer);
		}

		public void TestPropertiesReadOnly()
		{
			var line1 = Factory.New<CusISFLine>();
			var line2 = Factory.New<CusISFLine>();
			line2.BL_BL_Parent = line1.PK;
			Assert("BL_TextProductCodeInfo.ReadOnly", !line1.BL_TextProductCodeInfo.ReadOnly);
			Assert("BL_PartAttrib1Info.ReadOnly", !line1.BL_PartAttrib1Info.ReadOnly);
			Assert("BL_PartAttrib2Info.ReadOnly", !line1.BL_PartAttrib2Info.ReadOnly);
			Assert("BL_PartAttrib3Info.ReadOnly", !line1.BL_PartAttrib3Info.ReadOnly);
			Assert("BL_BL_ParentInfo.ReadOnly", line1.BL_BL_ParentInfo.ReadOnly);
			Assert("BL_TextProductCodeInfo.ReadOnly", line2.BL_TextProductCodeInfo.ReadOnly);
			Assert("BL_PartAttrib1Info.ReadOnly", line2.BL_PartAttrib1Info.ReadOnly);
			Assert("BL_PartAttrib2Info.ReadOnly", line2.BL_PartAttrib2Info.ReadOnly);
			Assert("BL_PartAttrib3Info.ReadOnly", line2.BL_PartAttrib3Info.ReadOnly);
			Assert("BL_BL_ParentInfo.ReadOnly", line2.BL_BL_ParentInfo.ReadOnly);

			Assert("BL_RN_NKGoodsOriginInfo.ReadOnly", !line1.BL_RN_NKGoodsOriginInfo.ReadOnly);
			line1.BL_LineType = ISFLineTypeList.Codes.Component;
			Assert("BL_RN_NKGoodsOriginInfo.ReadOnly", line1.BL_RN_NKGoodsOriginInfo.ReadOnly);
		}

		[TestDate(2016, 4, 6)]
		public void TestISFDate()
		{
			var header = Factory.New<CusISFHeader>();
			var line = header.Lines.AddNew();
			AssertEquals("ISFDate", new ZDateTime(2016, 4, 6), line.ISFDate);
			var transport = header.Transports.AddNew();
			transport.JW_ETA = new ZDateTime(2016, 4, 7);
			AssertEquals("ISFDate", new ZDateTime(2016, 4, 7), line.ISFDate);
			transport.JW_ATA = new ZDateTime(2016, 4, 8);
			AssertEquals("ISFDate", new ZDateTime(2016, 4, 8), line.ISFDate);
		}

		[TestDate(2016, 4, 6)]
		public void TestPivot()
		{
			var importer = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABIGAS");
			var supplier1 = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABIMOT");
			var supplier2 = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "AALSHI");
			US.Business.OrgSupplierPart part1 = Factory.New<US.Business.OrgSupplierPart>();
			part1.OP_PartNum = "PART123ZZ";
			part1.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			part1.RelatedOrganisations.AddOrganisationIfNotExist(supplier1.PK, OrgPartRelation.RelationshipTypes.Supplier);
			var pivot11 = part1.PivotsForBinding.AddNew();
			pivot11.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot11.CI_OH = part1.RelatedOrganisations[0].OU_OH;
			pivot11.CI_TariffNum = "1010231032";
			pivot11.CD_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Australia;
			pivot11.CI_DateStart = new ZDateTime(2016, 4, 1);
			pivot11.CI_DateEnd = new ZDateTime(2016, 4, 30);
			pivot11.Attributes1.AddNew().BG_AttributeValue1 = "1";
			pivot11.Attributes2.AddNew().BG_AttributeValue1 = "1";
			pivot11.Attributes3.AddNew().BG_AttributeValue1 = "1";
			var pivot12 = part1.PivotsForBinding.AddNew();
			pivot12.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot12.CI_OH = part1.RelatedOrganisations[1].OU_OH;
			pivot12.CI_TariffNum = "1010231033";
			pivot12.CD_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			pivot12.CI_DateStart = new ZDateTime(2016, 5, 1);
			pivot12.CI_DateEnd = new ZDateTime(2016, 5, 31);
			pivot12.Attributes1.AddNew().BG_AttributeValue1 = "2";
			pivot12.Attributes2.AddNew().BG_AttributeValue1 = "2";
			pivot12.Attributes3.AddNew().BG_AttributeValue1 = "2";
			US.Business.OrgSupplierPart part2 = Factory.New<US.Business.OrgSupplierPart>();
			part2.OP_PartNum = "PART123ZZ";
			part2.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			part2.RelatedOrganisations.AddOrganisationIfNotExist(supplier2.PK, OrgPartRelation.RelationshipTypes.Supplier);
			var pivot21 = part2.PivotsForBinding.AddNew();
			pivot21.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot21.CI_OH = part2.RelatedOrganisations[1].OU_OH;
			pivot21.CI_TariffNum = "1010231044";
			pivot21.CD_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			pivot21.CI_DateStart = new ZDateTime(2016, 5, 1);
			pivot21.CI_DateEnd = new ZDateTime(2016, 5, 31);
			pivot21.Attributes1.AddNew().BG_AttributeValue1 = "2";
			pivot21.Attributes2.AddNew().BG_AttributeValue1 = "2";
			pivot21.Attributes3.AddNew().BG_AttributeValue1 = "2";
			var header = Factory.New<CusISFHeader>();
			header.BF_OH_Importer = importer.PK;
			var line = header.Lines.AddNew();
			line.BL_TextProductCode = "PART123ZZ";
			AssertNull("Pivot", line.Pivot);
			line.BL_PartAttrib1 = "1";
			AssertNull("Pivot", line.Pivot);
			line.BL_PartAttrib2 = "1";
			AssertNull("Pivot", line.Pivot);
			line.BL_PartAttrib3 = "1";
			AssertEquals("Pivot", pivot11, line.Pivot);
			AssertEquals("BL_HarmonisedNum", "1010231032", line.BL_HarmonisedNum);
			AssertEquals("BL_RN_NKGoodsOrigin", "AU", line.BL_RN_NKGoodsOrigin);
			line.BL_TextProductCode = ZString.Empty;
			AssertEquals("BL_PartAttrib1", ZString.Empty, line.BL_PartAttrib1);
			AssertEquals("BL_PartAttrib2", ZString.Empty, line.BL_PartAttrib2);
			AssertEquals("BL_PartAttrib3", ZString.Empty, line.BL_PartAttrib3);
			AssertNull("Pivot", line.Pivot);
			var transport = header.Transports.AddNew();
			transport.JW_ATA = new ZDateTime(2016, 5, 7);
			line.BL_TextProductCode = "PART123ZZ";
			line.BL_PartAttrib1 = "2";
			line.BL_PartAttrib2 = "2";
			line.BL_PartAttrib3 = "2";
		}

		public void TestLineCountryOfOriginWhenProductCountryIsEmpty()
		{
			var importer = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABIGAS");
			var supplier = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABIMOT");
			var part = Factory.New<US.Business.OrgSupplierPart>();
			part.OP_PartNum = "PART123";
			part.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			part.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, OrgPartRelation.RelationshipTypes.Supplier);
			var pivot = part.PivotsForBinding.AddNew();
			var header = Factory.New<CusISFHeader>();
			header.BF_OH_Importer = importer.PK;
			var manufacturer1 = header.DocAddresses.CreateWithAddressType(DocAddressType.Manufacturer);
			manufacturer1.E2_CompanyName = "Company 1";
			manufacturer1.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var line = header.Lines.AddNew();
			line.BL_TextProductCode = "PART123";
			AssertEquals("AU", line.BL_RN_NKGoodsOrigin);
		}

		public void TestRefreshParentTariffLine()
		{
			var header1 = Factory.New<CusISFHeader>();
			var line1 = header1.Lines.AddNew();
			var childLine = line1.AddChildLine();
			AssertEquals(1, line1.ChildLines.Count());
			var relatedLine = line1.AddProductRelatedLine();
			AssertEquals(1, line1.ProductRelatedLines.Count());
			childLine.BL_BL_Parent = ZGuid.Empty;
			AssertEquals(0, line1.ChildLines.Count());
			var line2 = line1.Header.Lines.AddNew();
			childLine.BL_BL_Parent = line2.PK;
			AssertEquals(1, line2.ChildLines.Count());
			relatedLine.BL_BL_Parent = line2.PK;
			AssertEquals(0, line1.ProductRelatedLines.Count());
			AssertEquals(1, line2.ProductRelatedLines.Count());
			childLine.BL_LineType = ISFLineTypeList.Codes.Related;
			AssertEquals(0, line2.ChildLines.Count());
			AssertEquals(2, line2.ProductRelatedLines.Count());
			childLine.BL_LineType = ISFLineTypeList.Codes.Component;
			AssertEquals(1, line2.ChildLines.Count());
			AssertEquals(1, line2.ProductRelatedLines.Count());
			childLine.Delete();
			AssertEquals(0, line2.ChildLines.Count());
			relatedLine.Delete();
			AssertEquals(0, line2.ProductRelatedLines.Count());
		}

		public void TestManufacturerShortAddress_Updates()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			var orgAddress1 = Factory.New<OrgAddress>();
			orgAddress1.OA_Code = "SHORT 1";
			var orgAddress2 = Factory.New<OrgAddress>();
			orgAddress2.OA_Code = "SHORT 2";
			var manufacturer1 = header.DocAddresses.AddNew();
			manufacturer1.E2_AddressOverride = true;
			manufacturer1.E2_OA_Address = orgAddress1.PK;
			var manufacturer2 = header.DocAddresses.AddNew();
			manufacturer2.E2_AddressOverride = true;
			manufacturer2.E2_OA_Address = orgAddress2.PK;
			CusISFLine line = header.Lines.AddNew();
			line.BL_ManufacturerDocAddressPK = manufacturer1.PK;
			AssertEquals("SHORT 1", line.BL_ManufacturerShortAddress);
			line.BL_ManufacturerDocAddressPK = manufacturer2.PK;
			AssertEquals("SHORT 2", line.BL_ManufacturerShortAddress);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<CusISFHeader>();
			header.BF_OH_Importer = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			return header.Lines.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<CusISFHeader>();
			header.BF_OH_Importer = factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			return header.Lines.AddNew();
		}
	}
}

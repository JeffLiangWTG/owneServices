using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class CusISFLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestSupplierParts()
		{
			OrgHeader org1 = Factory.New<OrgHeader>();
			OrgHeader org2 = Factory.New<OrgHeader>();
			OrgHeader org3 = Factory.New<OrgHeader>();
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.BF_OH_Importer = org2.PK;
			header.SellingParty.OrganisationPK = org1.PK;
			var part = Factory.New<US.Business.OrgSupplierPart>();
			part.OP_PartNum = "AAA";
			part.RelatedOrganisations.AddOrganisationIfNotExist(org2.PK, OrgPartRelation.RelationshipTypes.Owner);
			part.RelatedOrganisations.AddOrganisationIfNotExist(org1.PK, OrgPartRelation.RelationshipTypes.Supplier);
			var pivot1 = part.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = US.Business.ClassificationTypeList.Codes.HTI;
			pivot1.CI_OH = part.RelatedOrganisations[1].OU_OH;
			pivot1.Attributes1.AddNew().BG_AttributeValue1 = "1";
			pivot1.Attributes2.AddNew().BG_AttributeValue1 = "2";
			pivot1.Attributes3.AddNew().BG_AttributeValue1 = "3";
			CusISFLine line1 = header.Lines.AddNew();
			line1.BL_TextProductCode = "AAA";
			line1.BL_PartAttrib1 = "1";
			line1.BL_PartAttrib2 = "2";
			line1.BL_PartAttrib3 = "3";
			ISFDocAddress manufacturer2 = header.ManufacturerAddresses.AddNew();
			manufacturer2.E2_OA_Address = org3.MainAddress.PK;
			CusISFLine line2 = header.Lines.AddNew();
			var parts = line1.Lookups.SupplierParts;
			AssertEquals("OP_PartNum is set", true, parts.FilterBusinessObjectDefaults.ContainsDefaultFor("Product Code:Property"));
			AssertEquals("Supplier is set", true, parts.FilterBusinessObjectDefaults.ContainsDefaultFor("Importer/Supplier:Property2"));
			AssertEquals("Correct Supplier", org1.PK, (ZGuid)parts.FilterBusinessObjectDefaults["Importer/Supplier:Property2"].Value);
			AssertEquals("Buyer is set", true, parts.FilterBusinessObjectDefaults.ContainsDefaultFor("Importer/Supplier:Property1"));
			AssertEquals("Correct Importer", org2.PK, (ZGuid)parts.FilterBusinessObjectDefaults["Importer/Supplier:Property1"].Value);
			header.SellingParty.OrganisationPK = ZGuid.Empty;
			var parts2 = line2.Lookups.SupplierParts;
			AssertEquals("OP_PartNum is not set", false, parts2.FilterBusinessObjectDefaults.ContainsDefaultFor("Product Code:Property"));
			AssertEquals("Supplier is set", false, parts2.FilterBusinessObjectDefaults.ContainsDefaultFor("Importer/Supplier:Property2"));
			AssertEquals("Buyer is set", true, parts2.FilterBusinessObjectDefaults.ContainsDefaultFor("Importer/Supplier:Property1"));
			AssertEquals("Correct Importer", org2.PK, (ZGuid)parts2.FilterBusinessObjectDefaults["Importer/Supplier:Property1"].Value);
		}

		public void TestPartAttribList()
		{
			var importer = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABIGAS");
			var supplier1 = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABIMOT");
			var supplier2 = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "AALSHI");
			US.Business.OrgSupplierPart part = Factory.New<US.Business.OrgSupplierPart>();
			part.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			part.RelatedOrganisations.AddOrganisationIfNotExist(supplier1.PK, OrgPartRelation.RelationshipTypes.Supplier);
			part.RelatedOrganisations.AddOrganisationIfNotExist(supplier2.PK, OrgPartRelation.RelationshipTypes.Supplier);
			part.OP_PartNum = "PART123ZZ";
			var pivot1 = part.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = US.Business.ClassificationTypeList.Codes.HTI;
			pivot1.CI_OH = part.RelatedOrganisations[0].OU_OH;
			pivot1.Attributes1.AddNew().BG_AttributeValue1 = "1";
			pivot1.Attributes2.AddNew().BG_AttributeValue1 = "2";
			pivot1.Attributes3.AddNew().BG_AttributeValue1 = "3";
			var pivot2 = part.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = US.Business.ClassificationTypeList.Codes.HTI;
			pivot2.CI_OH = part.RelatedOrganisations[1].OU_OH;
			pivot2.Attributes1.AddNew().BG_AttributeValue1 = "4";
			pivot2.Attributes2.AddNew().BG_AttributeValue1 = "5";
			pivot2.Attributes3.AddNew().BG_AttributeValue1 = "6";
			var pivot3 = part.PivotsForBinding.AddNew();
			pivot3.CI_ChildType = US.Business.ClassificationTypeList.Codes.HTI;
			pivot3.CI_OH = part.RelatedOrganisations[2].OU_OH;
			pivot3.Attributes1.AddNew().BG_AttributeValue1 = "7";
			pivot3.Attributes2.AddNew().BG_AttributeValue1 = "8";
			pivot3.Attributes3.AddNew().BG_AttributeValue1 = "9";
			Factory.Save();
			var header = Factory.New<CusISFHeader>();
			header.BF_OH_Importer = importer.PK;
			var line = header.Lines.AddNew();
			line.BL_TextProductCode = "PART123ZZ";
			AssertEquals("PartAttrib1List.Count", 1, line.Lookups.PartAttrib1List.Count);
			AssertEquals("PartAttrib1List.Code", "1", line.Lookups.PartAttrib1List[0].Code);
			AssertEquals("PartAttrib2List.Count", 1, line.Lookups.PartAttrib2List.Count);
			AssertEquals("PartAttrib2List.Code", "2", line.Lookups.PartAttrib2List[0].Code);
			AssertEquals("PartAttrib3List.Count", 1, line.Lookups.PartAttrib3List.Count);
			AssertEquals("PartAttrib3List.Code", "3", line.Lookups.PartAttrib3List[0].Code);
			header.SellingParty.OrganisationPK = supplier1.PK;
			AssertEquals("PartAttrib1List.Count", 2, line.Lookups.PartAttrib1List.Count);
			AssertEquals("PartAttrib1List.Code", "1", line.Lookups.PartAttrib1List[0].Code);
			AssertEquals("PartAttrib1List.Code", "4", line.Lookups.PartAttrib1List[1].Code);
			AssertEquals("PartAttrib2List.Count", 2, line.Lookups.PartAttrib2List.Count);
			AssertEquals("PartAttrib2List.Code", "2", line.Lookups.PartAttrib2List[0].Code);
			AssertEquals("PartAttrib2List.Code", "5", line.Lookups.PartAttrib2List[1].Code);
			AssertEquals("PartAttrib3List.Count", 2, line.Lookups.PartAttrib3List.Count);
			AssertEquals("PartAttrib3List.Code", "3", line.Lookups.PartAttrib3List[0].Code);
			AssertEquals("PartAttrib3List.Code", "6", line.Lookups.PartAttrib3List[1].Code);
			header.SellingParty.OrganisationPK = supplier2.PK;
			AssertEquals("PartAttrib1List.Count", 2, line.Lookups.PartAttrib1List.Count);
			AssertEquals("PartAttrib1List.Code", "1", line.Lookups.PartAttrib1List[0].Code);
			AssertEquals("PartAttrib1List.Code", "7", line.Lookups.PartAttrib1List[1].Code);
			AssertEquals("PartAttrib2List.Count", 2, line.Lookups.PartAttrib2List.Count);
			AssertEquals("PartAttrib2List.Code", "2", line.Lookups.PartAttrib2List[0].Code);
			AssertEquals("PartAttrib2List.Code", "8", line.Lookups.PartAttrib2List[1].Code);
			AssertEquals("PartAttrib3List.Count", 2, line.Lookups.PartAttrib3List.Count);
			AssertEquals("PartAttrib3List.Code", "3", line.Lookups.PartAttrib3List[0].Code);
			AssertEquals("PartAttrib3List.Code", "9", line.Lookups.PartAttrib3List[1].Code);
		}
	}
}

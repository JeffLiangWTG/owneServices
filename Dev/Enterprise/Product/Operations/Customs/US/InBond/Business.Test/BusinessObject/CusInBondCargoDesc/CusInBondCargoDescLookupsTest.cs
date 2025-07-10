using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using AMSBusiness = Enterprise.Customs.US.AMS.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	sealed class CusInBondCargoDescLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCarrierTariffs()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew();
			var container = moveDetail.Containers.AddNew();
			var commodity = container.Commodities.AddNew();
			var collection = commodity.Lookups.Tariffs;
			AssertNotNull(collection);
		}

		public void TestSuppliers()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew();
			var container = moveDetail.Containers.AddNew();
			var commodity = container.Commodities.AddNew();
			AssertEquals("Suppliers", typeof(ConsignorCollection), commodity.Lookups.Suppliers.GetType());
		}

		public void TestManifestUnitList()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew();
			var container = moveDetail.Containers.AddNew();
			var commodity = container.Commodities.AddNew();
			var list = commodity.Lookups.ManifestUnitList;
			AssertEquals(typeof(CodeDescriptionPairList), list.GetType());
			AssertEquals(new AMSBusiness.ManifestUnitList().Count, list.Count);
		}

		public void TestWeightUnitList()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew();
			var container = moveDetail.Containers.AddNew();
			var commodity = container.Commodities.AddNew();
			AssertContainsExactElementsInAnyOrder(new CodeDescriptionPairList(OLookUpEditType.Weight), commodity.Lookups.WeightUnitList);
		}

		public void TestParts()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var product = Factory.New<US.Business.OrgSupplierPart>();
			product.OP_PartNum = "Test";
			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = importer.PK;
			relOrg.OU_Relationship = "OWN";
			Factory.Save();
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = importer.MainAddress.PK;
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew();
			var container = moveDetail.Containers.AddNew();
			var commodity = container.Commodities.AddNew();
			var collection = commodity.Lookups.Parts;
			AssertNotNull(collection);
		}

		public void TestPartAttrib1List()
		{
			AssertPartAttribList(x => x.Lookups.PartAttrib1List, y => y.Attributes1.AddNew());
		}

		public void TestPartAttrib2List()
		{
			AssertPartAttribList(x => x.Lookups.PartAttrib2List, y => y.Attributes2.AddNew());
		}

		public void TestPartAttrib3List()
		{
			AssertPartAttribList(x => x.Lookups.PartAttrib3List, y => y.Attributes3.AddNew());
		}

		void AssertPartAttribList(GetPartAttribListDelegate getPartAttribList, GetNewAttributeDelegate getNewAttribute)
		{
			OrgHeader importer1 = Factory.New<OrgHeader>();
			importer1.OH_IsConsignee = true;
			OrgHeader importer2 = Factory.New<OrgHeader>();
			importer2.OH_IsConsignee = true;
			OrgHeader supplier1 = Factory.New<OrgHeader>();
			supplier1.OH_IsConsignor = true;
			OrgHeader supplier2 = Factory.New<OrgHeader>();
			supplier2.OH_IsConsignor = true;
			OrgHeader supplier3 = Factory.New<OrgHeader>();
			supplier3.OH_IsConsignor = true;
			US.Business.OrgSupplierPart part = Factory.New<US.Business.OrgSupplierPart>();
			part.OP_PartNum = "Z123Z456";
			var orgRelationImp1 = part.RelatedOrganisations.AddOrganisationIfNotExist(importer1.PK, OrgPartRelation.RelationshipTypes.Owner);
			var orgRelationImp2 = part.RelatedOrganisations.AddOrganisationIfNotExist(importer2.PK, OrgPartRelation.RelationshipTypes.Owner);
			var orgRelationSup1 = part.RelatedOrganisations.AddOrganisationIfNotExist(supplier1.PK, OrgPartRelation.RelationshipTypes.Supplier);
			var orgRelationSup2 = part.RelatedOrganisations.AddOrganisationIfNotExist(supplier2.PK, OrgPartRelation.RelationshipTypes.Supplier);
			var orgRelationSup3 = part.RelatedOrganisations.AddOrganisationIfNotExist(supplier3.PK, OrgPartRelation.RelationshipTypes.Supplier);
			var pivot9 = AddPivot(part, orgRelationSup2.OU_OH, ClassificationTypeList.Codes.HTE, "17", "18", getNewAttribute);
			var pivot8 = AddPivot(part, orgRelationSup1.OU_OH, ClassificationTypeList.Codes.HTE, "15", "16", getNewAttribute);
			var pivot7 = AddPivot(part, orgRelationImp2.OU_OH, ClassificationTypeList.Codes.HTE, "13", "14", getNewAttribute);
			var pivot6 = AddPivot(part, orgRelationImp1.OU_OH, ClassificationTypeList.Codes.HTE, "11", "12", getNewAttribute);
			var pivot5 = AddPivot(part, ZGuid.Empty, ClassificationTypeList.Codes.HTI, "9", "10", getNewAttribute);
			var pivot4 = AddPivot(part, orgRelationSup2.OU_OH, ClassificationTypeList.Codes.HTI, "7", "8", getNewAttribute);
			var pivot30 = AddPivot(part, ZGuid.Empty, ClassificationTypeList.Codes.SHB, "59", "60", getNewAttribute);
			var pivot3 = AddPivot(part, orgRelationSup1.OU_OH, ClassificationTypeList.Codes.HTI, "5", "6", getNewAttribute);
			var pivot29 = AddPivot(part, orgRelationSup2.OU_OH, ClassificationTypeList.Codes.SHB, "57", "58", getNewAttribute);
			var pivot28 = AddPivot(part, orgRelationSup1.OU_OH, ClassificationTypeList.Codes.SHB, "55", "56", getNewAttribute);
			var pivot27 = AddPivot(part, orgRelationImp2.OU_OH, ClassificationTypeList.Codes.SHB, "53", "54", getNewAttribute);
			var pivot26 = AddPivot(part, orgRelationImp1.OU_OH, ClassificationTypeList.Codes.SHB, "51", "52", getNewAttribute);
			var pivot25 = AddPivot(part, ZGuid.Empty, ClassificationTypeList.Codes.HTE, "49", "50", getNewAttribute);
			var pivot24 = AddPivot(part, orgRelationSup2.OU_OH, ClassificationTypeList.Codes.HTE, "47", "48", getNewAttribute);
			var pivot23 = AddPivot(part, orgRelationSup1.OU_OH, ClassificationTypeList.Codes.HTE, "45", "46", getNewAttribute);
			var pivot22 = AddPivot(part, orgRelationImp2.OU_OH, ClassificationTypeList.Codes.HTE, "43", "44", getNewAttribute);
			var pivot21 = AddPivot(part, orgRelationImp1.OU_OH, ClassificationTypeList.Codes.HTE, "41", "42", getNewAttribute);
			var pivot20 = AddPivot(part, ZGuid.Empty, ClassificationTypeList.Codes.HTI, "39", "40", getNewAttribute);
			var pivot2 = AddPivot(part, orgRelationImp2.OU_OH, ClassificationTypeList.Codes.HTI, "3", "4", getNewAttribute);
			var pivot19 = AddPivot(part, orgRelationSup2.OU_OH, ClassificationTypeList.Codes.HTI, "37", "38", getNewAttribute);
			var pivot18 = AddPivot(part, orgRelationSup1.OU_OH, ClassificationTypeList.Codes.HTI, "35", "36", getNewAttribute);
			var pivot17 = AddPivot(part, orgRelationImp2.OU_OH, ClassificationTypeList.Codes.HTI, "33", "34", getNewAttribute);
			var pivot16 = AddPivot(part, orgRelationImp1.OU_OH, ClassificationTypeList.Codes.HTI, "31", "32", getNewAttribute);
			var pivot15 = AddPivot(part, ZGuid.Empty, ClassificationTypeList.Codes.SHB, "29", "30", getNewAttribute);
			var pivot14 = AddPivot(part, orgRelationSup2.OU_OH, ClassificationTypeList.Codes.SHB, "27", "28", getNewAttribute);
			var pivot13 = AddPivot(part, orgRelationSup1.OU_OH, ClassificationTypeList.Codes.SHB, "25", "26", getNewAttribute);
			var pivot12 = AddPivot(part, orgRelationImp2.OU_OH, ClassificationTypeList.Codes.SHB, "23", "24", getNewAttribute);
			var pivot11 = AddPivot(part, orgRelationImp1.OU_OH, ClassificationTypeList.Codes.SHB, "21", "22", getNewAttribute);
			var pivot10 = AddPivot(part, ZGuid.Empty, ClassificationTypeList.Codes.HTE, "19", "20", getNewAttribute);
			var pivot1 = AddPivot(part, orgRelationImp1.OU_OH, ClassificationTypeList.Codes.HTI, "1", "2", getNewAttribute);
			var header = Factory.New<CusInBondHeader>();
			header.ImporterOrgPK = importer1.PK;
			header.BH_OH_Supplier = supplier1.PK;
			var bill = header.Bills.AddNew();
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container = moveDetail.Containers.AddNew();
			var commodity = container.Commodities.AddNew();
			commodity.BY_OH_Supplier = supplier2.PK;
			commodity.BY_PartNumber = "Z123Z456";
			AssertPartAttribList(commodity, "1, 2, 31, 32", getPartAttribList);
			header.ImporterOrgPK = ZGuid.Empty;
			AssertPartAttribList(commodity, "37, 38, 7, 8", getPartAttribList);
			commodity.BY_OH_Supplier = ZGuid.Empty;
			AssertPartAttribList(commodity, "35, 36, 5, 6", getPartAttribList);
			commodity.BY_OH_Supplier = supplier3.PK;
			AssertPartAttribList(commodity, "10, 39, 40, 9", getPartAttribList);
		}

		void AssertPartAttribList(CusInBondCargoDesc commodity, string expectedList, GetPartAttribListDelegate getPartAttribList)
		{
			var list = getPartAttribList(commodity);
			AssertEquals(expectedList, list.CodesAsString);
		}

		delegate CodeDescriptionPairList GetPartAttribListDelegate(CusInBondCargoDesc commodity);
		delegate CusAttributeFilter GetNewAttributeDelegate(CusClassPartPivot pivot);
		CusClassPartPivot AddPivot(US.Business.OrgSupplierPart part, ZGuid orgRelationPK, ZString childType, ZString value1, ZString value2, GetNewAttributeDelegate getNewAttribute)
		{
			CusClassPartPivot result = part.PivotsForBinding.AddNew();
			result.CI_OH = orgRelationPK;
			result.CI_ChildType = childType;
			var attrib1 = getNewAttribute(result);
			attrib1.BG_AttributeValue1 = value1;
			var attrib2 = getNewAttribute(result);
			attrib2.BG_AttributeValue1 = value2;
			return result;
		}
	}
}

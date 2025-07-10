using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Module.Testing
{
	sealed class EntryLineFilterLookupsTest : TestCaseWithFactory
	{
		public void TestLists()
		{
			AssertEquals("Consignees should not be loaded by the property", false, lookups.Consignees.IsLoaded);
			AssertEquals("Consignees of correct type", typeof(ConsigneeCollection), lookups.Consignees.GetType());
			AssertEquals("ClassificationList should not be loaded by the property", false, lookups.ClassificationList.IsLoaded);
			AssertEquals("ClassificationList of correct type", typeof(BaseClassificationCollection<BaseCusClassification>), lookups.ClassificationList.GetType());
			AssertEquals("CountryList should not be loaded by the property", false, ((IBusinessObjectCollection)lookups.CountryList).IsLoaded);
			AssertEquals("CountryList of correct type", typeof(RefCountryCollection), lookups.CountryList.GetType());
		}

		public void TestPartsList()
		{
			OrgHeader importer = Factory.New<OrgHeader>();
			importer.OH_Code = "Importer";
			importer.MainAddress.OA_Address1 = "address";
			lookups.Importer = importer;
			MasterFiles.Business.OrgSupplierPart part1 = Enterprise.MasterFiles.Business.OrgSupplierPart.New(Factory);
			part1.OP_PartNum = "part1";
			OrgPartRelation relationPart1 = part1.RelatedOrganisations.AddNew();
			relationPart1.OU_OH = importer.PK;
			relationPart1.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			OrgHeader otherImporter = Factory.New<OrgHeader>();
			otherImporter.OH_Code = "Importer2";
			otherImporter.MainAddress.OA_Address1 = "address";
			MasterFiles.Business.OrgSupplierPart part2 = Enterprise.MasterFiles.Business.OrgSupplierPart.New(Factory);
			part2.OP_PartNum = "part2";
			OrgPartRelation relationPart2 = part2.RelatedOrganisations.AddNew();
			relationPart2.OU_OH = otherImporter.PK;
			relationPart2.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			Factory.Save();
			MasterFiles.Business.OrgSupplierPartCollection parts = lookups.PartsList;
			parts.Load();
			AssertEquals("Contains part1", true, parts.Contains(part1.PK));
			AssertEquals("Contains part2", true, parts.Contains(part2.PK));
			parts.FilterBusinessObjectDefaults.ContainsDefaultFor("Importer/Supplier");
		}

		EntryLineFilterLookups lookups;
		EntryLineFilterBusinessObject filterBizObj;
		GlobalCusEntryLineCollection filterCollection;
		protected override void SetUp()
		{
			base.SetUp();
			filterCollection = new GlobalCusEntryLineCollection(Factory);
			filterBizObj = new EntryLineFilterBusinessObject(filterCollection);
			lookups = new EntryLineFilterLookups(filterBizObj);
		}
	}
}

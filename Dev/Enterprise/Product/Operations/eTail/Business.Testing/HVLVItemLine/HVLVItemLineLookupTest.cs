using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.OrgPartRelation;

namespace Enterprise.eTail.Business.Testing
{
	public class HVLVItemLineLookupTest : BusinessObjectLookupsTestCase
	{
		public void TestProducts()
		{
			var eTailer = Factory.LoadTop1<IOrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignor, true));

			var eTailer2 = Factory.LoadTop1<IOrgHeader>(
				new ZQuery(OrgHeaderSchema.OH_IsConsignor, true)
				.AddToFilter(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, eTailer.PK));

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = eTailer.PK;

			var hvlvItem = Factory.NewWithValidTestData<HVLVItem>();
			hvlvItem.HVI_JS_LoadedOnShipment = shipment.PK;

			var product1 = Factory.NewWithValidTestData<OrgSupplierPart>();
			var orgRelation1 = product1.RelatedOrganisations.AddNew();
			orgRelation1.OU_Relationship = RelationshipTypes.Supplier;
			orgRelation1.OU_OH = eTailer.PK;

			var product2 = Factory.NewWithValidTestData<OrgSupplierPart>();
			var orgRelation2 = product2.RelatedOrganisations.AddNew();
			orgRelation2.OU_Relationship = RelationshipTypes.Owner;
			orgRelation2.OU_OH = eTailer2.PK;

			var product3 = Factory.NewWithValidTestData<OrgSupplierPart>();
			var orgRelation3 = product3.RelatedOrganisations.AddNew();
			orgRelation3.OU_Relationship = RelationshipTypes.Supplier;
			orgRelation3.OU_OH = eTailer2.PK;

			var product4 = Factory.NewWithValidTestData<OrgSupplierPart>();
			var orgRelation4 = product4.RelatedOrganisations.AddNew();
			orgRelation4.OU_Relationship = RelationshipTypes.Supplier;
			orgRelation4.OU_OH = eTailer.PK;

			Factory.Save();

			var line = hvlvItem.Lines.AddNew();
			var lookups = line.Lookups.Products;
			lookups.Load();

			AssertContainsExactElementsInAnyOrder(new[] { product1.PK, product4.PK }, lookups.Select(p => p.PK));
		}

		public void TestWeightUnitList()
		{
			var line = Factory.New<HVLVItemLine>();
			AssertEquals("WeightUQList", Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight), line.Lookups.WeightUnitList);
		}
	}
}

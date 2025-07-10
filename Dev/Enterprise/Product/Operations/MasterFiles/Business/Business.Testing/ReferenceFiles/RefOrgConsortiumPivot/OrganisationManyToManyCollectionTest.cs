using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrganisationManyToManyCollection))]
	sealed class OrganisationManyToManyCollectionTest : ActiveBusinessObjectCollectionTestCase<OrganisationManyToManyCollection>
	{
		protected override OrganisationManyToManyCollection GetCollectionToTest()
		{
			RefCarrierConsortium consortium = Factory.New<RefCarrierConsortium>();
			return new OrganisationManyToManyCollection(consortium);
		}

		public void TestOrganisationManyToManyCollectionTest()
		{
			RefCarrierConsortium consortium1 = Factory.New<RefCarrierConsortium>();
			RefCarrierConsortium consortium2 = Factory.New<RefCarrierConsortium>();

			OrgHeader shippingLine1 = Factory.New<OrgHeader>();
			OrgHeader shippingLine2 = Factory.New<OrgHeader>();
			OrgHeader shippingLine3 = Factory.New<OrgHeader>();
			OrgHeader shippingLine4 = Factory.New<OrgHeader>();

			consortium1.OrgHeaders.Add(shippingLine1);
			consortium1.OrgHeaders.Add(shippingLine2);
			consortium2.OrgHeaders.Add(shippingLine2);
			consortium2.OrgHeaders.Add(shippingLine3);

			AssertEquals("Consortium1 has 2 Members", 2, consortium1.OrgHeaders.Count);
			Assert("ShippingLine1 belongs to Consortium1", consortium1.OrgHeaders.Contains(shippingLine1));
			Assert("ShippingLine2 belongs to Consortium1", consortium1.OrgHeaders.Contains(shippingLine2));

			AssertEquals("Consortium2 has 2 Members", 2, consortium2.OrgHeaders.Count);
			Assert("ShippingLine2 belongs to Consortium2", consortium2.OrgHeaders.Contains(shippingLine2));
			Assert("ShippingLine3 belongs to Consortium2", consortium2.OrgHeaders.Contains(shippingLine3));
		}

		public void TestTypedFind()
		{
			RefCarrierConsortium consortium = Factory.New<RefCarrierConsortium>();
			OrganisationManyToManyCollection collection = new OrganisationManyToManyCollection(consortium);
			OrgHeader[] collectionResult = collection.Find(new ZQuery());
			AssertNotNull(collectionResult);
		}
	}
}

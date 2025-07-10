using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgCarrierNamedAccountCollection))]
	sealed class OrgCarrierNamedAccountCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new OrgCarrierNamedAccountCollection(Factory);

		public void TestOrgCarrierNamedAccountCollection_CarrierRelationship()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();

			var cna1 = Factory.New<OrgCarrierNamedAccount>();
			var cna2 = Factory.New<OrgCarrierNamedAccount>();

			cna1.ONA_OH_Carrier = carrier1.PK;
			cna2.ONA_OH_Carrier = carrier2.PK;

			var namedAccounts = new OrgCarrierNamedAccountCollection(carrier1, OrgCarrierNamedAccountSchema.ONA_OH_Carrier);
			namedAccounts.Load();
			AssertEquals(1, namedAccounts.Count);
			AssertEquals(carrier1.PK, namedAccounts[0].ONA_OH_Carrier);
		}

		public void TestOrgCarrierNamedAccountCollection_WithFilter()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();

			var cna1 = Factory.New<OrgCarrierNamedAccount>();
			var cna2 = Factory.New<OrgCarrierNamedAccount>();
			var cna3 = Factory.New<OrgCarrierNamedAccount>();

			cna1.ONA_OH_Carrier = carrier1.PK;
			cna2.ONA_OH_Carrier = carrier2.PK;
			cna3.ONA_OH_Carrier = carrier2.PK;

			var namedAccounts = new OrgCarrierNamedAccountCollection(Factory, new ZQuery(OrgCarrierNamedAccountSchema.ONA_OH_Carrier, carrier2.PK));
			namedAccounts.Load();
			AssertEquals(2, namedAccounts.Count);

			namedAccounts = new OrgCarrierNamedAccountCollection(Factory, ZQuery.NoResultQuery);
			namedAccounts.Load();
			AssertEquals(0, namedAccounts.Count);
		}

		public void TestAddNew_WithRelationship()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();

			var namedAccounts = new OrgCarrierNamedAccountCollection(carrier1, OrgCarrierNamedAccountSchema.ONA_OH_Organization);
			var cna = namedAccounts.AddNew();
			AssertEquals(ZGuid.Empty, cna.ONA_OH_Carrier);
			AssertEquals(carrier1.PK, cna.ONA_OH_Organization);

			namedAccounts = new OrgCarrierNamedAccountCollection(carrier1, OrgCarrierNamedAccountSchema.ONA_OH_Carrier);
			cna = namedAccounts.AddNew();
			AssertEquals(carrier1.PK, cna.ONA_OH_Carrier);
			AssertEquals(ZGuid.Empty, cna.ONA_OH_Organization);
		}
	}
}

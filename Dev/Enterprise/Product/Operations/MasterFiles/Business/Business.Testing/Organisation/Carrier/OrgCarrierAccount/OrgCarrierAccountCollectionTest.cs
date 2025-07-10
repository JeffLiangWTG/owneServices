using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgCarrierAccountCollection))]
	sealed class OrgCarrierAccountCollectionTest : ActiveBusinessObjectCollectionTestCase<OrgCarrierAccountCollection>
	{
		protected override OrgCarrierAccountCollection GetCollectionToTest()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			return new OrgCarrierAccountCollection(header);
		}

		public void TestAllowNew()
		{
			var result = new OrgCarrierAccountCollection(Factory);
			result.AddNew();
			AssertEquals(1, result.Count);
		}

		public void TestOrgCarrierAccountCollection()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();

			var can1 = Factory.New<OrgCarrierAccount>();
			var can2 = Factory.New<OrgCarrierAccount>();

			can1.OAN_OH_Carrier = carrier1.PK;
			can2.OAN_OH_Carrier = carrier2.PK;

			var carrierAccounts = new OrgCarrierAccountCollection(carrier1);
			AssertEquals(1, carrierAccounts.Count);
		}

		public void TestOrgCarrierAccountCollection_WithFilter()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();

			var can1 = Factory.New<OrgCarrierAccount>();
			var can2 = Factory.New<OrgCarrierAccount>();
			var can3 = Factory.New<OrgCarrierAccount>();

			can1.OAN_OH_Carrier = carrier1.PK;
			can2.OAN_OH_Carrier = carrier2.PK;
			can3.OAN_OH_Carrier = carrier2.PK;

			var carrierAccounts1 = new OrgCarrierAccountCollection(Factory, new ZQuery(OrgCarrierAccountSchema.OAN_OH_Carrier, carrier2.PK));
			AssertEquals(2, carrierAccounts1.Count);

			var carrierAccounts2 = new OrgCarrierAccountCollection(Factory, ZQuery.NoResultQuery);
			AssertEquals(0, carrierAccounts2.Count);
		}
	}
}

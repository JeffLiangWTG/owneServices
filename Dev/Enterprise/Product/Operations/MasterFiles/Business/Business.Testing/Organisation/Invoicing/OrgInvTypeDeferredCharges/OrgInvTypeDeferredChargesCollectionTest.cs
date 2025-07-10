using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgInvTypeDeferredChargesCollection))]
	sealed class OrgInvTypeDeferredChargesCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCreateRelationshipFilter()
		{
			var deferredCharge1 = Factory.NewWithValidTestData<OrgInvTypeDeferredCharges>();
			var deferredCharge2 = Factory.NewWithValidTestData<OrgInvTypeDeferredCharges>();
			var collection = (OrgInvTypeDeferredChargesCollection)GetCollectionToTest();
			var chargeCurrentCompany = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK));
			var chargeAnotherCompany = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, new ZGuid("22C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061")));

			deferredCharge1.PO_AC = chargeCurrentCompany.PK;
			deferredCharge2.PO_AC = chargeAnotherCompany.PK;
			deferredCharge1.PO_PI = Master.PK;
			deferredCharge2.PO_PI = Master.PK;
			Factory.Save();

			collection.Load();

			AssertEquals(1, collection.Count);
			Assert(collection.Contains(deferredCharge1));
		}

		OrgInvoiceType Master;

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			Master = Factory.NewWithValidTestData<OrgInvoiceType>();
			return new OrgInvTypeDeferredChargesCollection(Master);
		}
	}
}

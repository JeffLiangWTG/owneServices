using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSalesCallCollection))]
	sealed class OrgSalesCallCollectionTest : ActiveBusinessObjectCollectionTestCase<OrgSalesCallCollection>
	{
		public void TestDefaultValues()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgSalesCall call = org.SalesCalls.AddNew();
			AssertEquals(org.PK, call.OQ_OH);

			var callCollection = new OrgSalesCallCollection(Factory);
			var call2 = callCollection.AddNew();
			AssertEquals(ZGuid.Empty, call2.OQ_OH);
		}

		#region Implementation

		protected override OrgSalesCallCollection GetCollectionToTest()
		{
			return new OrgSalesCallCollection(Factory.New<OrgHeader>());
		}

		#endregion
	}
}

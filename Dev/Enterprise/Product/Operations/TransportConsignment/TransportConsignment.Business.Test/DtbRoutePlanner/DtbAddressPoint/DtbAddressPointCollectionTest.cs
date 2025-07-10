using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbAddressPointCollection))]
	sealed class DtbAddressPointCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DtbAddressPointCollection>
	{
		#region TestAllowNew

		public void TestAllowNew()
		{
			AssertEquals(false, GetCollectionToTestTypeSafe().AllowNew);
		}

		#endregion

		#region TestAllowRemove

		public void TestAllowRemove()
		{
			AssertEquals(false, GetCollectionToTestTypeSafe().AllowRemove);
		}

		#endregion

		#region Implementation

		protected override DtbAddressPointCollection GetCollectionToTest()
		{
			return new DtbAddressPointCollection(Factory);
		}

		DtbAddressPointCollection GetCollectionToTestTypeSafe()
		{
			return GetCollectionToTest();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var org = Helper.CreateOrganisation("Honda");
			return new DtbAddressPoint(org.MainAddress);
		}

		TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}

		TransportBookingConsignmentTestHelper helper;

		#endregion
	}
}

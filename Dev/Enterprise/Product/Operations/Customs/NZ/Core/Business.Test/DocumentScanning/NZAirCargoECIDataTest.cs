using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NZ.Business.Express;

namespace Enterprise.Customs.NZ.Business.Testing
{
	sealed class NZAirCargoECIDataTest : TestCaseWithFactory
	{
		public void TestBusinessObjectType()
		{
			AssertEquals("BusinessObjectType", typeof(Customs.Business.CusMAWB), airCargoData.BusinessObjectType);
		}

		public void TestCollectionType()
		{
			AssertType<CusMAWBCollection>("CollectionType", airCargoData.GetBusinessObjectCollection(Factory));
		}

		public void TestReferenceType()
		{
			AssertEquals("ReferenceType", Core.Constants.ReferenceTypes.SupplyChainLogistics, airCargoData.ReferenceType);
		}

		public void TestHumanReadableName()
		{
			AssertEquals("HumanReadableName", "AirCargo ECI", airCargoData.HumanReadableName.GetUnresolvedValue());
		}

		public void TestIsAllowedForUnallocatedeDocs()
		{
			Assert("IsAllowedForUnallocatedeDocs", airCargoData.IsAllowedForUnallocatedeDocs);
		}

		protected override void SetUp()
		{
			base.SetUp();
			airCargoData = new NZAirCargoECIData();
		}
		NZAirCargoECIData airCargoData;
	}
}

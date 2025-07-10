using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	[TestedType(typeof(QuotedBookingContainerDependentCollection))]
	public class QuotedBookingContainerDependentCollectionTest : QuotedBookingContainerDependentCollectionTest<ForwardingContainer>
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			ForwardingShipment booking = Factory.New<ForwardingShipment>();
			return new QuotedBookingContainerDependentCollection(booking, Factory);
		}

		public void TestCommodityCode()
		{
			var oldCommodityCode = Factory.LoadTop1<RefCommodityCode>(new ZQuery(RefCommodityCodeSchema.RH_IsActive, true));
			var newCommodityCode = Factory.NewWithValidTestData<RefCommodityCode>();
			var booking = Factory.New<ForwardingShipment>();
			var collection = new QuotedBookingContainerDependentCollection(booking, Factory);
			var newContainer = collection.AddNew();
			AssertEquals("No commodity code exists so new container defaults to empty commodity code", ZString.Empty, newContainer.JC_RH_NKContainerCommodityCode);
			collection.CommodityCode = oldCommodityCode.RH_Code;
			newContainer = collection.AddNew();
			AssertEquals("Expected to use user defined commodity code", oldCommodityCode.RH_Code, newContainer.JC_RH_NKContainerCommodityCode);
			collection.CommodityCode = newCommodityCode.RH_Code;
			newContainer = collection.AddNew();
			AssertEquals("Expected to use pre-existing commodity code", newCommodityCode.RH_Code, newContainer.JC_RH_NKContainerCommodityCode);
		}
	}

	public abstract class QuotedBookingContainerDependentCollectionTest<T> : BusinessObjectCollectionTestCase
		where T : ForwardingContainer
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<T>();
		}
	}
}

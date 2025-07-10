using System.ComponentModel;
using Enterprise.TransportCommon.Business.Testing;
using Enterprise.TransportCommon.Shared;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbBookingConsignmentCollection))]
	public class DtbBookingConsignmentCollectionTest : DtbTransportCollectionTest<DtbBookingConsignment, DtbBookingConsignmentCollection>
	{
		#region TestAllowNew

		public void TestAllowNew()
		{
			var consolidation = Helper.CreateConsolidation();

			AssertEquals(false, ((IBindingList)new DtbBookingConsignmentCollection(Factory)).AllowNew);
			AssertEquals(true, ((IBindingList)new DtbBookingConsignmentCollection(consolidation)).AllowNew);
		}

		#endregion

		#region TestCollectionUsingConsolConstructorDoesNotNeedFactorySave

		public void TestCollectionUsingConsolConstructorDoesNotNeedFactorySave()
		{
			var consignment = Helper.CreateBookingConsignment();

			var collection = new DtbBookingConsignmentCollection(consignment.ConsolidationSingleJob);
			AssertEquals(1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { consignment }, collection);
		}

		#endregion

		#region Implementation

		protected override string GetParentType_ForTesting()
		{
			return TransportConsolidationJobTypes.Codes.Consignment;
		}

		protected override DtbBookingConsignment GetTransportToAddToTheCollection()
		{
			return Helper.CreateBookingConsignment();
		}

		TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}

		TransportBookingConsignmentTestHelper helper;

		#endregion
	}
}

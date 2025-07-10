using System;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.TransportBookings.Business.Testing;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Business.Test
{
	[TestedType(typeof(DtbBookingConsolidationProcessTaskCollection))]
	public class DtbBookingConsolidationProcessTaskCollectionTest : ProcessTaskCollectionTest<DtbBookingConsolidationProcessTaskCollection>
	{
		protected override DtbBookingConsolidationProcessTaskCollection GetCollectionToTestCore()
		{
			return new DtbBookingConsolidationProcessTaskCollection(Helper.CreateConsolidation());
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(DtbBookingConsolidationProcessTaskCollection);
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}
		TransportBookingTestHelper helper;
	}
}

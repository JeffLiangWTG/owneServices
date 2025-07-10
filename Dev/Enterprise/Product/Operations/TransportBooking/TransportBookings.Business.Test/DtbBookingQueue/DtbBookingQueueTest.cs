using System;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Business.Test
{
	[TestedType(typeof(DtbBookingQueue))]
	internal class DtbBookingQueueTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIteration()
		{
			var dtbBookingQueue = Factory.New<DtbBookingQueue>();
			AssertEquals("Iteration starts at zero", Convert.ToInt16(dtbBookingQueue.KMQ_Iteration), dtbBookingQueue.Iteration);

			dtbBookingQueue.Iteration = 1;
			AssertEquals("Iteration should now be 1", Convert.ToInt16(dtbBookingQueue.KMQ_Iteration), dtbBookingQueue.Iteration);
		}
	}
}

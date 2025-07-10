using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.TransportBooking;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class TransportJobResultTest : TestCaseWithFactory
	{
		#region TestConstructor_TransportJob

		public void TestConstructor_TransportJob()
		{
			var booking = (IDtbBooking)Factory.NewWithValidTestData(ObjectFactory.GetType<IDtbBooking>());
			var transportJobResult = new TransportJobResult((IRelatedJob)booking);
			AssertEquals(booking, transportJobResult.TransportJob);
			AssertNull(transportJobResult.ReasonForEmptyOverride);
		}

		#endregion

		#region TestEmpty

		public void TestEmpty()
		{
			var empty = TransportJobResult.Empty;
			AssertNull(empty.TransportJob);
			AssertNull(empty.ReasonForEmptyOverride);
		}

		#endregion

		#region TestMultipleJobs

		public void TestMultipleJobs()
		{
			var multipleJobs = TransportJobResult.MultipleJobs;
			AssertNull(multipleJobs.TransportJob);
			AssertEquals("Multiple Jobs", multipleJobs.ReasonForEmptyOverride);
		}

		#endregion
	}
}

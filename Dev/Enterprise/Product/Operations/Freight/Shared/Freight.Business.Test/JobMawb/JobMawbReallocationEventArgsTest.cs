using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Business.Testing
{
	sealed class JobMawbReallocationEventArgsTest : TestCaseWithFactory
	{
		public void TestCreateInstance()
		{
			JobMawb mawb = Factory.New<JobMawb>();
			JobMawbReallocationEventArgs eventArgs = new JobMawbReallocationEventArgs(mawb);

			AssertEquals(eventArgs.Mawb, mawb);
			AssertEquals(false, eventArgs.Cancel);
		}
	}
}

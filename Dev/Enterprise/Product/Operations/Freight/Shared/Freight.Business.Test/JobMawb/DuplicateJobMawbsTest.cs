using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Business.Testing
{
	sealed class DuplicateJobMawbsTest : TestCaseWithFactory
	{
		public void TestDuplicatesExist()
		{
			JobMawb jobMawb = Factory.New<JobMawb>();
			jobMawb.JM_Airline3DigitPrefix = "777";
			jobMawb.JM_MAWB = "00000022";

			Factory.Save();

			DuplicateJobMawbs test1 = new DuplicateJobMawbs(Factory, "00000000", "00000099", "777");
			AssertEquals(1, test1.Count);
			AssertEquals("00000022", test1.FirstDuplicate);

			DuplicateJobMawbs test2 = new DuplicateJobMawbs(Factory, "00000033", "00000099", "777");
			AssertEquals(0, test2.Count);
			AssertEquals("", test2.FirstDuplicate);
		}
	}
}

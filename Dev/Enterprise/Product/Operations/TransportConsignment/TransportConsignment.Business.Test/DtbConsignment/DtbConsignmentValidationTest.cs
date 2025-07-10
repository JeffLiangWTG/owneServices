using CargoWise.EntityFramework.Testing;

namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed class DtbConsignmentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckLTC_JobType()
		{
			var consignment = Helper.CreateConsignment("LT001");
			AssertEquals("Precondition", "", consignment.LTC_JobType);
			consignment.LTC_JobType = "LTL";
			AssertNoErrors("Consignment LTC Job Type should be set to 'FCL', 'FTL', or 'LTL'.", consignment.LTC_JobTypeInfo);
			consignment.LTC_JobType = "AAA";
			AssertHasError(consignment.LTC_JobTypeInfo, "LTC Job Type should be set to 'FCL', 'FTL', or 'LTL'.");

			consignment.LTC_JobType = "FCL";
			AssertNoErrors("FCL is a valid job type.", consignment.LTC_JobTypeInfo);

			consignment.LTC_JobType = "FTL";
			AssertNoErrors("FTL is a valid job type.", consignment.LTC_JobTypeInfo);
		}

		#region Helper

		TransportConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportConsignmentTestHelper(Factory)); }
		}
		TransportConsignmentTestHelper helper;

		#endregion

	}
}

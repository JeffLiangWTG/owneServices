using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	class FreightStatusQueryTest : TestCaseWithFactory
	{
		public void TestStatusRequestCode()
		{
			var freightStatusQuery = new FreightStatusQuery();
			AssertEquals(AIMFreightStatusRequestCodes.Codes.RequestAllInformationForSingleBill, freightStatusQuery.StatusRequestCode);
			freightStatusQuery.StatusRequestCode = AIMFreightStatusRequestCodes.Codes.RequestForNominatedAgent;
			AssertEquals(AIMFreightStatusRequestCodes.Codes.RequestForNominatedAgent, freightStatusQuery.StatusRequestCode);
		}
	}
}

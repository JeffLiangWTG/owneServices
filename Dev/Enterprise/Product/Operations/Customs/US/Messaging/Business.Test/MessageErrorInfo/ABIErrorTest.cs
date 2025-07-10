using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	sealed class ABIErrorTest : TestCaseWithFactory
	{
		public void TestGetErrorInfoByCode()
		{
			AssertNotNull(ABIError.Instance);
			AssertEquals(1716, ABIError.Instance.ABIErrorCollectionCount);
			AssertNull("no this key", ABIError.Instance.GetErrorInfoByCode("abc"));
			MessageError error = ABIError.Instance.GetErrorInfoByCode("VQS");
			AssertNotNull(error);
			AssertEquals("VQS", error.ErrorCode);
			AssertEquals("IMPORT REF NOT ALLOWED", error.ShortDesc);
			AssertEquals("NO NARRATIVE GIVEN", error.Narrative);
		}
	}
}

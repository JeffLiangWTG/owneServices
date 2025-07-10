using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	sealed class ACEErrorTest : TestCaseWithFactory
	{
		public void TestGetErrorInfoByCode()
		{
			AssertNotNull(ACEError.Instance);
			AssertEquals(649, ACEError.Instance.ACEErrorCollectionCount);
			AssertNull("no this key", ACEError.Instance.GetErrorInfoByCode("abc"));
			MessageError error = ACEError.Instance.GetErrorInfoByCode("659"); //Latest update:the data added at 20/09/2014
			AssertNotNull(error);
			AssertEquals("659", error.ErrorCode);
			AssertEquals("Fee Accepted; 'X' Comp Code", error.ShortDesc);
			AssertEquals("This information condition will be generated if a line is submitted with an AMS fee class code/amount pair in a 62-Record and an HTS number on the line requires that AMS fee (may or must), and the formula to compute the fee is a complex computation not verified by the system.", error.Narrative);
		}
	}
}

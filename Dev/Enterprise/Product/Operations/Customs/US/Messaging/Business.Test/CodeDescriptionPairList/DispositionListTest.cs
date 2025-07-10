using NUnit.Framework;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	sealed class DispositionListTest : TestCase
	{
		public void TestIsNotableFTZDispositionCode()
		{
			AssertEquals(false, DispositionList.IsNotableFTZDispositionCode(DispositionList.Codes.B1));
			AssertEquals(false, DispositionList.IsNotableFTZDispositionCode(DispositionList.Codes.B2));
			AssertEquals(false, DispositionList.IsNotableFTZDispositionCode(DispositionList.Codes.B3));
			AssertEquals(true, DispositionList.IsNotableFTZDispositionCode(DispositionList.Codes.B5));
			AssertEquals(true, DispositionList.IsNotableFTZDispositionCode(DispositionList.Codes.BF));
		}
	}
}

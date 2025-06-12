using NUnit.Framework;

namespace CargoWise.eHub.Nudge.Tests
{
	[TestFixture]
	public class NudgeHttpUtilTest
	{
		[Test]
		public void TestMakeHttpRequest_InvalidUrl_CatchUriFormatExceptionReturnStatusCodeMinus1()
		{
			var nudgeHttpUtil = new NudgeHttpUtil();

			var actualStatusCode = nudgeHttpUtil.MakeHttpRequest("ehub-ausyd-test.cargowise.net");

			Assert.AreEqual(-1, actualStatusCode);
		}

		[Test]
		public void TestMakeHttpRequest_ValidUrl_StatusCode200()
		{
			var nudgeHttpUtil = new NudgeHttpUtil();

			var actualStatusCode = nudgeHttpUtil.MakeHttpRequest("https://ehub-ausyd-test.wisegrid.net/eHubGateway/");

			Assert.AreEqual(200, actualStatusCode);
		}
	}
}

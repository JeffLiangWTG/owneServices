using CargoWise.eHub.Products.CACustoms.DocImg.BT.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.CACustoms.DocImg.BT.Tests
{
    [TestClass]
	public class HttpResponseTests
    {
        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestGet()
        {
			var response = new HttpResponse("500", "Internal Error");
			Assert.AreEqual("500", response.Code);
			Assert.AreEqual("Internal Error", response.Description);
        }

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestShouldRetry()
		{
			var retryHttpCodes = new[] {"401", "404", "408", "423", "429", "451", "500", "501", "502", "503", "504", "505", "506", "507", "508", "509", "510", "511", "530" };

			foreach (var httpCode in retryHttpCodes)
			{
				var response = new HttpResponse(httpCode, "HTTP Description");
				Assert.IsTrue(response.ShouldRetry);
			}
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestShouldNotRetry()
		{
			var response = new HttpResponse("400", "Bad Request");
			Assert.IsFalse(response.ShouldRetry);
		}

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestShouldRetry_EmptyCodes()
        {
            var response = new HttpResponse("", "Bad Request", true);
            Assert.IsTrue(response.ShouldRetry);

            response = new HttpResponse("", "Bad Request");
            Assert.IsFalse(response.ShouldRetry);
		}
	}
}

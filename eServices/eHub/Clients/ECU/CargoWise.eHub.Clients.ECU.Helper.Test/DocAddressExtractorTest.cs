using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Clients.ECU.Transforms.Helper.Tests
{
    [TestClass]
    public class DocAddressExtractorTest
    {
        [TestMethod]
        public void TestExtractAddressInfo()
        {
            string addrstring = @"KIM TECH INC. 101 INNOVATION DRIVE UNIT 4  VAUGHN, ONTARIO L4H 0S3 CANADA TEL 31 10 4904600 FAX 31 10 4721294";
			var testExtractor = new DocAddressExtractor();

			Assert.AreEqual<string>("KIM TECH INC. 101 INNOVAT", testExtractor.ExtractOwnerCode(addrstring));
			Assert.AreEqual<string>("KIM TECH INC. 101 INNOVATION DRIVE UNIT 4  VAUGHN,", testExtractor.ExtractName(addrstring));
			Assert.AreEqual<string>("ONTARIO L4H 0S3 CANADA TEL 31 10 4904600 FAX 31 1", testExtractor.ExtractAddressLine1(addrstring));
			Assert.AreEqual<string>("0 4721294", testExtractor.ExtractAddressLine2(addrstring));
			Assert.AreEqual<string>("", testExtractor.ExtractCityOrSuburb(addrstring));
			Assert.AreEqual<string>("31 10 4904600", testExtractor.ExtractPhone(addrstring));
			Assert.AreEqual<string>("31 10 4721294", testExtractor.ExtractFax(addrstring));
        }

		[TestMethod]
		public void TestExtractAddressInfoLight()
		{
			string addrstring = @"KIM TECH INC. 101 INNOVATION DRIVE VAUGHN, ONTARIO TEL: 31 10 4904600 FAX.: 31 10 4721294";

			var testExtractor = new DocAddressExtractor();

			Assert.AreEqual<string>("KIM TECH INC. 101 INNOVATION DRIVE VAUGHN, ONTARIO", testExtractor.ExtractName(addrstring));
			Assert.AreEqual<string>("TEL: 31 10 4904600 FAX.: 31 10 4721294", testExtractor.ExtractAddressLine1(addrstring));
			Assert.AreEqual<string>("", testExtractor.ExtractAddressLine2(addrstring));
			Assert.AreEqual<string>("", testExtractor.ExtractCityOrSuburb(addrstring));
			Assert.AreEqual<string>("31 10 4904600", testExtractor.ExtractPhone(addrstring));
			Assert.AreEqual<string>("31 10 4721294", testExtractor.ExtractFax(addrstring));
		}

		[TestMethod]
		public void TestExtractAddressInfoEmpty()
		{
			string addrstring = @"";

			var testExtractor = new DocAddressExtractor();

			Assert.AreEqual<string>("", testExtractor.ExtractName(addrstring));
			Assert.AreEqual<string>("", testExtractor.ExtractAddressLine1(addrstring));
			Assert.AreEqual<string>("", testExtractor.ExtractAddressLine2(addrstring));
			Assert.AreEqual<string>("", testExtractor.ExtractCityOrSuburb(addrstring));
			Assert.AreEqual<string>("", testExtractor.ExtractPhone(addrstring));
			Assert.AreEqual<string>("", testExtractor.ExtractFax(addrstring));
		}

		[TestMethod]
		public void TestTrimContainerNumber()
		{
			string result = TrimContainerNumber("SUDU 684461/0");
			Assert.AreEqual<string>("SUDU6844610", result);
		}

		public string TrimContainerNumber(string inputContainerName)
		{
			return inputContainerName.Replace(" ", "").Replace("/", "").Substring(0, 11);
		}

		public string GetDocAddressType(string partyType)
		{
			string result = partyType;

			if (!string.IsNullOrEmpty(partyType))
			{
				switch (partyType.ToUpper().Trim())
				{
					case "SHIPPER":
						result = "CRD";
						break;
					case "CONSIGNEE":
						result = "CED";
						break;
					case "NOTIFY1":
						result = "NPP";
						break;
					case "NOTIFY2":
						result = "N2D";
						break;
				}
			}

			return result;
		}
    }
}

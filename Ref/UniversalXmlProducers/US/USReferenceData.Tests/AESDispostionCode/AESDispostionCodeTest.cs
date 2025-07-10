using CargoWise.RefDbRepo.USReferenceData.Business.AESDispostionCode;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.USReferenceData.Tests
{
	[TestFixture]
	sealed class AESDispostionCodeTest
	{
		[Test]
		public void TestAESDispostionCode()
		{
			var dispostionCode = new AESDispostionCode();
			dispostionCode.ResponseCode = "Test";
			dispostionCode.ResponseCode = "ResponseCode";

			dispostionCode.NarrativeText = "Test";
			dispostionCode.NarrativeText = "NarrativeText";

			dispostionCode.Severity = "Test";
			dispostionCode.Severity = "Severity";

			dispostionCode.Reason = "Test";
			dispostionCode.Reason = "Reason";

			dispostionCode.Resolution = "Test";
			dispostionCode.Resolution = "Resolution";

			Assert.AreEqual("ResponseCode", dispostionCode.ResponseCode);
			Assert.AreEqual("TestNarrativeText", dispostionCode.NarrativeText);
			Assert.AreEqual("Severity", dispostionCode.Severity);
			Assert.AreEqual("TestReason", dispostionCode.Reason);
			Assert.AreEqual("TestResolution", dispostionCode.Resolution);
		}
	}
}

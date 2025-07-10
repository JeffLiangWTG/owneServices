using CargoWise.RefDbRepo.EUReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.CUSNumbers.Tests
{
	class CusNumberSOAPServiceTest
	{
		[Test]
		public void TestRequest()
		{
			var expectedString = @"<Envelope xmlns=""http://schemas.xmlsoap.org/soap/envelope/"">
  <Header />
  <Body>
    <chemicalSubstanceForWs xmlns=""http://chemicalsubstanceforWS.ws.ecics.dds.s/"">
      <cusNumber>0010001-6</cusNumber>
      <cusNumber>0010002-7</cusNumber>
    </chemicalSubstanceForWs>
  </Body>
</Envelope>";

			var requestNumbers = new[] { "0010001-6", "0010002-7" };
			var requestString = CusNumberSOAPService.CreateRequest(requestNumbers);

			Assert.That(expectedString, Is.EqualTo(requestString));
		}
	}
}

using System.Text;
using CargoWise.RefDbRepo.ESReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ESReferenceData.Tests;

[TestFixture]
public class WebRequestWrapperTests
{
	class WebRequestWrapperForTest() : WebRequestWrapper(Encoding.UTF8)
	{
		public string SanitizeHtmlExposed(string html) => SanitizeHtml(html);
	}

	[Test]
	public void TestSanitizeHtml()
	{
		var badHtml = "Certificado de autenticidad zumo de naranja concentrado\u001A\x1A";

		var goodHtml = new WebRequestWrapperForTest().SanitizeHtmlExposed(badHtml);
		Assert.That(goodHtml, Is.EqualTo("Certificado de autenticidad zumo de naranja concentrado"));
	}
}

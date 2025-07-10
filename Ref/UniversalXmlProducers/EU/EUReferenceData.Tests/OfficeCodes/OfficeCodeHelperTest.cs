using System.Linq;
using System.Xml.Linq;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.OfficeCodes.Business;
using CargoWise.RefDbRepo.EUReferenceData.Tests;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.OfficeCodes.Tests
{
	[TestFixture]
	class OfficeCodeHelperTest
	{
		[Test]
		public void GetCustomsOfficesRootElement()
		{
			var result = xmlDoc.GetCustomsOfficesRootElement();
			Assert.That(result.Name, Is.EqualTo(Constants.OfficeCodes.RootElementName));
			Assert.That(result.FirstAttribute.Value, Is.EqualTo(Constants.OfficeCodes.RootAttributeValue));
		}

		[Test]
		public void GetCustomsOfficesRootElement_Null()
		{
			var result = OfficeCodeHelper.GetCustomsOfficesRootElement(null);
			Assert.That(result, Is.Null);
		}

		[Test]
		public void GetDateValueFromCustomsOfficeElement()
		{
			var rootElement = xmlDoc.GetCustomsOfficesRootElement();
			var customsOfficeElement = rootElement.Descendants(Constants.OfficeCodes.CustomsOfficeElementName).FirstOrDefault();
			var result = customsOfficeElement.GetDateValueFromCustomsOfficeElement(Constants.OfficeCodes.StartDateElement);
			Assert.That(result, Is.EqualTo("2020-03-23"));
		}

		[Test]
		public void GetDateValueFromCustomsOfficeElement_Null()
		{
			var result = OfficeCodeHelper.GetDateValueFromCustomsOfficeElement(null, Constants.OfficeCodes.StartDateElement);
			Assert.That(result, Is.Null);
		}

		[Test]
		public void GetValueFromElementsWithAttribute()
		{
			var rootElement = xmlDoc.GetCustomsOfficesRootElement();
			var customsOfficeElement = rootElement.Descendants(Constants.OfficeCodes.CustomsOfficeElementName).FirstOrDefault();
			var result = customsOfficeElement.GetValueFromDefaultElementWithAttribute(Constants.OfficeCodes.CountryCodeAttributeValue);
			Assert.That(result, Is.EqualTo("AD"));
		}

		[Test]
		public void GetValueFromElementsWithAttribute_Null()
		{
			var result = OfficeCodeHelper.GetValueFromDefaultElementWithAttribute(null, Constants.OfficeCodes.CountryCodeAttributeValue);
			Assert.That(result, Is.Null);
		}

		[SetUp]
		public void Setup()
		{
			xmlDoc = XDocument.Load(TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.OfficeCodes.TestFiles.Input.COL-Generic-20201222_SingleCustomsOffice.txt"));
		}
		XDocument xmlDoc;
	}
}

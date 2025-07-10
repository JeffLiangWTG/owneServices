using System;
using System.Collections;
using System.Linq;
using System.Xml.Linq;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.OfficeCodes.Business;
using CargoWise.RefDbRepo.EUReferenceData.Tests;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.OfficeCodes.Tests
{
	[TestFixture]
	class OfficeCodeDataProviderTest
	{
		[Test]
		public void ConstructorThrowsArgumentException()
		{
			Assert.Throws<ArgumentNullException>(() => new OfficeCodeDataProvider(null), "CustomsOffice cannot be null.");
		}

		[Test]
		public void ReferenceNumber()
		{
			Assert.That(provider.ReferenceNumber, Is.EqualTo("AD000001"));
		}

		[Test]
		public void ReferenceNumber_Missing()
		{
			RemoveXMLNode(customsOfficeElement, Constants.OfficeCodes.DefaultElementName, Constants.OfficeCodes.ReferenceNumberAttributeValue);
			Assert.That(provider.ReferenceNumber, Is.Null);
		}

		[Test]
		public void CountryCode()
		{
			Assert.That(provider.CountryCode, Is.EqualTo("AD"));
		}

		[Test]
		public void CountryCode_Missing()
		{
			RemoveXMLNode(customsOfficeElement, Constants.OfficeCodes.DefaultElementName, Constants.OfficeCodes.CountryCodeAttributeValue);
			Assert.That(provider.CountryCode, Is.Null);
		}

		[Test]
		public void StartDate()
		{
			Assert.That(provider.StartDate, Is.EqualTo(new DateTime(2020, 03, 23)));
			Assert.That(provider.StartDateSuccessfullyParsed, Is.True);
		}

		[Test]
		public void StartDate_Missing()
		{
			RemoveXMLNode(customsOfficeElement, Constants.OfficeCodes.StartDateElement);
			provider = new OfficeCodeDataProvider(customsOfficeElement);
			Assert.That(provider.StartDate, Is.EqualTo(Constants.MinimumDateTime));
			Assert.That(provider.StartDateSuccessfullyParsed, Is.True);
		}

		[Test]
		public void StartDate_Invalid()
		{
			ModifyDateValue(customsOfficeElement, Constants.OfficeCodes.StartDateElement, "2020-13-01");
			provider = new OfficeCodeDataProvider(customsOfficeElement);
			Assert.That(provider.StartDateSuccessfullyParsed, Is.False);
		}

		[Test]
		public void EndDate()
		{
			Assert.That(provider.EndDate, Is.EqualTo(new DateTime(2020, 05, 23)));
			Assert.That(provider.EndDateSuccessfullyParsed, Is.True);
		}

		[Test]
		public void EndDate_Missing()
		{
			RemoveXMLNode(customsOfficeElement, Constants.OfficeCodes.EndDateElement);
			provider = new OfficeCodeDataProvider(customsOfficeElement);
			Assert.That(provider.EndDate, Is.EqualTo(Constants.MaximumDateTime));
			Assert.That(provider.EndDateSuccessfullyParsed, Is.True);
		}

		[Test]
		public void EndDate_Invalid()
		{
			ModifyDateValue(customsOfficeElement, Constants.OfficeCodes.EndDateElement, "2020-13-01");
			provider = new OfficeCodeDataProvider(customsOfficeElement);
			Assert.That(provider.EndDateSuccessfullyParsed, Is.False);
		}

		[Test]
		public void PostalCode()
		{
			Assert.That(provider.PostalCode, Is.EqualTo("AD600"));
		}

		[Test]
		public void PostalCode_Missing()
		{
			RemoveXMLNode(customsOfficeElement, Constants.OfficeCodes.DefaultElementName, Constants.OfficeCodes.PostalCodeAttributeValue);
			Assert.That(provider.PostalCode, Is.Null);
		}

		[Test]
		public void EMailAddress()
		{
			Assert.That(provider.EMailAddress, Is.EqualTo("Post.ZA3@bmf.gv.at"));
		}

		[Test]
		public void EmailAddress_Missing()
		{
			RemoveXMLNode(customsOfficeElement, Constants.OfficeCodes.DefaultElementName, Constants.OfficeCodes.EMailAddressAttributeValue);
			Assert.That(provider.EMailAddress, Is.Null);
		}

		[Test]
		public void UsualName()
		{
			Assert.That(provider.UsualName, Is.EqualTo("CUSTOMS OFFICE SANT JULIÀ DE LÒRIA"));
		}

		[Test]
		public void UsualName_Missing()
		{
			RemoveXMLNode(languageElements.First(), Constants.OfficeCodes.DefaultElementName, Constants.OfficeCodes.UsualNameAttributeValue);
			Assert.That(provider.UsualName, Is.Null);
		}

		[Test]
		public void City()
		{
			Assert.That(provider.City, Is.EqualTo("SANT JULIÀ DE LÒRIA"));
		}

		[Test]
		public void City_Missing()
		{
			RemoveXMLNode(languageElements.First(), Constants.OfficeCodes.DefaultElementName, Constants.OfficeCodes.CityAttributeValue);
			Assert.That(provider.City, Is.Null);
		}

		[Test]
		public void StreetAndNumber()
		{
			Assert.That(provider.StreetAndNumber, Is.EqualTo("RIU RUNER BORDER"));
		}

		[Test]
		public void StreetAndNumber_Missing()
		{
			RemoveXMLNode(languageElements.First(), Constants.OfficeCodes.DefaultElementName, Constants.OfficeCodes.StreetAndNumberAttributeValue);
			Assert.That(provider.StreetAndNumber, Is.Null);
		}

		[Test]
		public void GetCorrectLanguageElement_DefaultEN()
		{
			Assert.That(provider.UsualName, Is.EqualTo("CUSTOMS OFFICE SANT JULIÀ DE LÒRIA"));
		}

		[Test]
		public void GetCorrectLanguageElement_FallbackToGatheredLanguage()
		{
			languageElements[0].Remove();
			Assert.That(provider.UsualName, Is.EqualTo("ADUANA DE ST. JULIÀ DE LÒRIA"));
		}

		[Test]
		public void GetCorrectLanguageElement_FallbackToFirstLanguage()
		{
			var languages = languageElements;
			languages[0].Remove();
			languages[1].Remove();
			Assert.That(provider.UsualName, Is.EqualTo("BUREAU DE SANT JULIÀ DE LÒRIA"));
		}

		[Test]
		public void Roles()
		{
			var roleParents = customsOfficeElement.Descendants(Constants.OfficeCodes.RoleParentElementName).Where(x => x.Attribute("name").Value == Constants.OfficeCodes.RoleParentAttributeValue);
			var roles = provider.Roles;
			Assert.That(roleParents.Count(), Is.EqualTo(8));
			Assert.That(roles.Count(), Is.EqualTo(4));
			Assert.That(roles, Is.SameAs(provider.Roles));
			Assert.That(roles is ICollection, Is.True);
		}

		[SetUp]
		public void Setup()
		{
			var xmlDoc = XDocument.Load(TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.OfficeCodes.TestFiles.Input.COL-Generic-20201222_SingleCustomsOffice.txt"));
			var rootElement = xmlDoc.GetCustomsOfficesRootElement();
			customsOfficeElement = rootElement.Descendants(Constants.OfficeCodes.CustomsOfficeElementName).FirstOrDefault();
			provider = new OfficeCodeDataProvider(customsOfficeElement);
		}
		XElement customsOfficeElement;
		OfficeCodeDataProvider provider;

		XElement[] languageElements => customsOfficeElement.Descendants(Constants.OfficeCodes.LanguagesElementName).Where(x => x.Attribute("name").Value == Constants.OfficeCodes.LanguagesAttributeValue).ToArray();

		void ModifyDateValue(XElement xmlRootElement, XName elementName, string valueToSet) => xmlRootElement.Descendants(elementName).FirstOrDefault().Value = valueToSet;

		void RemoveXMLNode(XElement xmlRootElement, XName elementName) => xmlRootElement.Descendants(elementName).SingleOrDefault()?.Remove();

		void RemoveXMLNode(XElement xmlRootElement, XName elementName, string nameAttributeValue) => xmlRootElement.Elements(elementName).FirstOrDefault(x => x.Attribute("name").Value == nameAttributeValue)?.Remove();
	}
}

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
	class OfficeCodeRoleProviderTest
	{
		[Test]
		public void Name()
		{
			Assert.That(provider.Name, Is.EqualTo("DEP"));
		}

		[Test]
		public void TransportModes()
		{
			var transportModes = provider.TransportModes;
			CollectionAssert.AreEqual(new[] { "ROA", "RAI" }, transportModes);
			Assert.That(transportModes, Is.SameAs(provider.TransportModes));
			Assert.That(transportModes is ICollection, Is.True);
		}

		[SetUp]
		public void Setup()
		{
			var xmlDoc = XDocument.Load(TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.OfficeCodes.TestFiles.Input.COL-Generic-20201222_SingleCustomsOffice.txt"));
			var rootElement = xmlDoc.GetCustomsOfficesRootElement();
			var customsOfficeElement = rootElement.Descendants(Constants.OfficeCodes.CustomsOfficeElementName).FirstOrDefault();
			var roleParents = customsOfficeElement.Descendants(Constants.OfficeCodes.RoleParentElementName).Where(x => x.Attribute("name").Value == Constants.OfficeCodes.RoleParentAttributeValue);
			provider = new OfficeCodeRoleProvider(roleParents.Where(x => x.GetValueFromDefaultElementWithAttribute(Constants.OfficeCodes.RoleAttributeValue) == "DEP"));
		}
		OfficeCodeRoleProvider provider;
	}
}

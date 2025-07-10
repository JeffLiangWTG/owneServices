using System;
using System.Xml.Linq;
using CargoWise.RefDbRepo.UniversalXmlParser.Models;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXmlParser.Test.Models
{
	[TestFixture]
	public class SourceContentFixtures
	{
		[Test]
		public void TestSourceContentXml()
		{
			const string testXml = @"<test>
  <key1>value1</key1>
  <key2>value2</key2>
  <PublicationTime>2017-08-04T00:00:00</PublicationTime>
  <UpdateType>Full</UpdateType>
  <Schema>
	<EntityType Name=""RefCusTariff"" data=""true"">
	  <Key>
		<PropertyRef Name=""ZZ1_ZZI_NKTariffType"" />
		<PropertyRef Name=""ZZ1_TariffCode"" />
		<PropertyRef Name=""ZZ1_IAMUnique"" />
	  </Key>
	</EntityType>
  </Schema>
</test>";
			var content = new SourceContent("test");
			content.AddContent("key1", "value1");
			content.AddContent("key2", "value2");
			content.AddContent("PublicationTime", "2017-08-04T00:00:00");
			content.AddContent("UpdateType", "Full");
			content.AddXmlContent(@"<Schema>
	<EntityType Name=""RefCusTariff"" data=""true"">
		<Key>
			<PropertyRef Name=""ZZ1_ZZI_NKTariffType"" />
			<PropertyRef Name=""ZZ1_TariffCode"" />
			<PropertyRef Name=""ZZ1_IAMUnique"" />
		</Key>
	</EntityType>
  </Schema>");

			var xml = content.ToXml();

			Assert.IsTrue(!string.IsNullOrEmpty(xml));

			var expectedXml = XDocument.Parse(testXml).ToString(SaveOptions.DisableFormatting);
			Assert.AreEqual(expectedXml, xml);
		}

		[Test]
		public void TransformSchemaSwitchFixture_SwitchOn()
		{
			var mockUniversalXmlTransformer = new Mock<IUniversalXmlTransformer>();
			var content = new SourceContent("Test", mockUniversalXmlTransformer.Object);
			content.AddXmlContent("<Schema></Schema>");
			content.ToXml();
			mockUniversalXmlTransformer.Verify(x => x.Transform(It.IsAny<string>()), Times.Once);
		}

		[Test]
		public void TransformSchemaSwitchFixture_SwitchOff()
		{
			var content = new SourceContent("Test");
			content.AddXmlContent(SchemaXmlForTransform_Trigger);
			content.ToXml();
			var expectedXml = XDocument.Parse($"<Test>{SchemaXmlForTransform_Trigger}</Test>").ToString(SaveOptions.DisableFormatting);
			Assert.That(expectedXml.Equals(content.ToXml(), StringComparison.OrdinalIgnoreCase));
		}

		const string SchemaXmlForTransform_Trigger = @"
<Schema>
  <EntityType Name=""RefCusRate"" >
    <Key>
      <PropertyRef Name=""RefCusApplicability"" />
    </Key>
    <Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
  </EntityType>
  <EntityType Name=""RefCusApplicability"" >
  </EntityType>
</Schema>";
	}
}

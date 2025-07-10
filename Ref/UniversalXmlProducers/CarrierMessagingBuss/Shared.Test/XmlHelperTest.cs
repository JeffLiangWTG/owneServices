using System;
using System.IO;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CarrierMessagingBuss.Shared.Test
{
	class XmlHelperTest
	{
		[TestCase("2021-11-18T16:42:54")]
		[TestCase("2020-12-19T17:43:55")]
		public void GenerateXmlWriterShouldReturnXmlWriter(string publicationDate)
		{
			//Setup
			var dateTime = DateTime.Parse(publicationDate, null);
			var refAccessorialConfiguration = new EntityTypeConfiguration<RefAccessorial>(true);
			refAccessorialConfiguration.IncludeColumn(x => x.ASI_Code, isKeyColumn: true);
			refAccessorialConfiguration.IncludeColumn(x => x.ASI_Description);

			//Act
			var xmlWriter = XmlHelper.GenerateXmlWriter(refAccessorialConfiguration, dateTime, "Accessorial List");

			//Assert
			Assert.That(xmlWriter, Is.InstanceOf<XmlWriter>());
			using var fileHelper = new FileTestHelper(Guid.NewGuid() + ".xml");
			var xmlFilePath = fileHelper.GetPath();
			XmlHelper.ExportToXMLFile(xmlWriter, xmlFilePath);
			fileHelper.AssertFileContent($@"<UniversalReferenceData>
  <DataSource>Accessorial List</DataSource>
  <PublicationTime>{publicationDate}</PublicationTime>
  <UpdateType>Full</UpdateType>
  <Schema>
    <EntityType Name=""RefAccessorial"" Data=""true"">
      <Key>
        <PropertyRef Name=""ASI_Code"" />
      </Key>
      <Property Name=""ASI_Code"" Type=""char"" MaxLength=""3"" />
      <Property Name=""ASI_Description"" Type=""varchar"" MaxLength=""50"" />
    </EntityType>
  </Schema>
</UniversalReferenceData>");
		}

		[Test]
		public void ExportToXMLFileNullHandling()
		{
			Assert.Throws<ArgumentNullException>(() => XmlHelper.ExportToXMLFile(null, null), "writer is null.f");
			Assert.Throws<ArgumentNullException>(() => XmlHelper.ExportToXMLFile(new XmlWriter(null), null), "path is null.");
		}

		[TestCase("TestDir1", true)]
		[TestCase("TestDir2", false)]
		public void ExportToXMLFile(string directoryName, bool hasDirectory)
		{
			//Setup
			if (hasDirectory)
			{
				Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), directoryName));
			}
			using var directoryHelper = new DirectoryTestHelper(directoryName);
			using var fileHelper = new FileTestHelper(Path.Combine(directoryName, Guid.NewGuid() + ".xml"));
			var refAccessorialConfiguration = new EntityTypeConfiguration<RefAccessorial>(true);
			refAccessorialConfiguration.IncludeColumn(x => x.ASI_Code, isKeyColumn: true);
			refAccessorialConfiguration.IncludeColumn(x => x.ASI_Description);

			var filePath = fileHelper.GetPath();
			var xmlWriter = XmlHelper.GenerateXmlWriter(refAccessorialConfiguration, DateTime.Parse("2023-10-17T15:41:53", null), "Accessorial List");

			//Act
			XmlHelper.ExportToXMLFile(xmlWriter, filePath);

			//Assert
			fileHelper.AssertFileContent(@"<UniversalReferenceData>
  <DataSource>Accessorial List</DataSource>
  <PublicationTime>2023-10-17T15:41:53</PublicationTime>
  <UpdateType>Full</UpdateType>
  <Schema>
    <EntityType Name=""RefAccessorial"" Data=""true"">
      <Key>
        <PropertyRef Name=""ASI_Code"" />
      </Key>
      <Property Name=""ASI_Code"" Type=""char"" MaxLength=""3"" />
      <Property Name=""ASI_Description"" Type=""varchar"" MaxLength=""50"" />
    </EntityType>
  </Schema>
</UniversalReferenceData>");
		}
	}
}

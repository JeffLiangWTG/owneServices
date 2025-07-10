using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CarrierMessagingBuss.Shared.Test
{
	public class XmlProducerTest
	{
		[TestCaseSource(nameof(Accessorials))]
		public void ExportXml(IEnumerable<RefAccessorial> accessorials, string publicationDate, string fileName)
		{
			//Setup
			var dateTime = DateTime.Parse(publicationDate, null);
			using var fileHelper = new FileTestHelper(fileName);
			var refAccessorialConfiguration = new EntityTypeConfiguration<RefAccessorial>(true);
			refAccessorialConfiguration.IncludeColumn(x => x.ASI_Code, isKeyColumn: true);
			refAccessorialConfiguration.IncludeColumn(x => x.ASI_Description);
			var xmlProducer = new XmlProducer<RefAccessorial>(refAccessorialConfiguration, dateTime, "Accessorial List", accessorials, fileHelper.GetPath());

			//Act
			xmlProducer.ExportXml();

			//Assert
			var expectedRefAccessorial = string.Empty;
			foreach (var accessorial in accessorials)
			{
				expectedRefAccessorial += $@"
  <RefAccessorial>
    <ASI_Code>{accessorial.ASI_Code}</ASI_Code>
    <ASI_Description>{accessorial.ASI_Description}</ASI_Description>
  </RefAccessorial>";
			}
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
  </Schema>{expectedRefAccessorial}
</UniversalReferenceData>");
		}

		public static IEnumerable<TestCaseData> Accessorials
		{
			get
			{
				var testDataMultipleAccessorials = new TestCaseData(
					new List<RefAccessorial>()
					{
						new()
						{
							ASI_Code = "001",
							ASI_Description = "Test Accessorial 1"
						},
						new()
						{
							ASI_Code = "002",
							ASI_Description = "Test Accessorial 2"
						}
					},
					"2020-12-19T17:43:55",
					"testFile1.xml");
				testDataMultipleAccessorials.SetName("ExportXmlMultipleAccessorials");
				yield return testDataMultipleAccessorials;

				var testDataSingleAccessorial = new TestCaseData(
					new List<RefAccessorial>()
					{
						new()
						{
							ASI_Code = "001",
							ASI_Description = "Test Accessorial 1"
						}
					},
					"2020-12-19T17:43:55",
					"testFile1.xml");
				testDataSingleAccessorial.SetName("ExportXmlSingleAccessorial");
				yield return testDataSingleAccessorial;

				var testDataEmptyAccessorial = new TestCaseData(
					new List<RefAccessorial>(),
					"2020-12-19T17:43:55",
					"testFile1.xml");
				testDataEmptyAccessorial.SetName("ExportXmlEmptyAccessorial");
				yield return testDataEmptyAccessorial;
			}
		}
	}
}

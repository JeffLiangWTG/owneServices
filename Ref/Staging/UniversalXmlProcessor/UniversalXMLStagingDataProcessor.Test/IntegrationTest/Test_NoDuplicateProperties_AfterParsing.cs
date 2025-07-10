using System;
using System.Collections.Generic;
using System.Data;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using NUnit.Framework;
using Stage = CargoWise.RefDbRepo.Staging.Schema_New;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test;

class Test_NoDuplicateProperties_AfterParsing : IXmlProcessIntegrationTest
{
	public string[] FileNames => ["TestFiles\\Test_NoDuplicateProperties_AfterParsing.xml"];

	public XmlProcessIntegrationTestAssertResultHandler[] AssertResults => [AssertResult_AfterProcessing_TestXml];

	public string TestDescription => "Testing no duplicate properties per entity type after parsing xml";
	public void PrepareData(IDbCommand stagingCommand, IDbCommand safeCommand)
	{
		Console.WriteLine("Start Preparing Data");

		Console.WriteLine("Preparing Data Successfully");
	}

	public void AssertResult_AfterProcessing_TestXml(IDbCommand stagingCommand, IDbCommand _)
	{
		AssertStagingDb_SourceData(stagingCommand);
	}

	void AssertStagingDb_SourceData(IDbCommand stagingCommand)
	{
		var sourceData = new Stage.SourceData();
		stagingCommand.CommandText = "SELECT TOP 1 * FROM dbo.SourceData ORDER BY SDA_CreatedTime DESC";
		using var reader = stagingCommand.ExecuteReader();
		reader.Read();
		sourceData.SDA_ContentText = reader[nameof(Stage.SourceData.SDA_ContentText)].ToString();

		Assert.That(sourceData.SDA_ContentText, Is.Not.Null.And.Not.Empty);
		XDocument parsedFile = XDocument.Parse(sourceData.SDA_ContentText!);

		foreach (var entity in parsedFile.Descendants("EntityType"))
		{
			var entityName = entity.Attribute("Name")!.Value;
			var allPropertyNames = new HashSet<string>();
			var duplicates = new HashSet<string>();

			foreach (var prop in entity.Elements("Property"))
			{
				var propName = prop.Attribute("Name")!.Value;

				if (!allPropertyNames.Add(propName))
				{
					duplicates.Add(propName);
				}
			}

			Assert.That(duplicates.Count, Is.EqualTo(0), $"Entity '{entityName}' contains duplicate <Property> names: {string.Join(", ", duplicates)}");
		}
	}
}

using System;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test;

[TestFixture]
partial class RemoteDbExposedObjectFixture
{
	[Test]
	public void OnlySupportView_ForExposedObjects()
	{
		var dacpacFile = Path.Combine(FolderHelper.GetBinFolder(), "RemoteDb.dacpac");
		using Package dacpac = Package.Open(dacpacFile);
		var modelPart = dacpac.GetPart(new Uri("/model.xml", UriKind.Relative));
		using Stream modelStream = modelPart.GetStream();
		var xDoc = XDocument.Load(modelStream);
		var ns = xDoc.Root!.GetDefaultNamespace();

		var regex = DboViewNameRegex();
		var elements = xDoc.Descendants(ns + "Element")
			.Where(e => e.Attribute("Name") != null && regex.IsMatch(e.Attribute("Name")!.Value));

		Assert.Multiple(() =>
		{
			foreach (var element in elements)
			{
				var name = element.Attribute("Name")!.Value;
				var typeAttr = element.Attribute("Type");
				Assert.That(typeAttr, Is.Not.Null, $"Element with Name='{name}' does not have a Type attribute.");
				Assert.That(typeAttr!.Value, Is.EqualTo("SqlView"),
					$"Element with Name='{name}' is not of type SqlView, we only support view currently.");
			}
		});
	}

	[Test]
	public void ShouldHaveMetaData_ForExposedObjects()
	{
		var dacpacFile = Path.Combine(FolderHelper.GetBinFolder(), "RemoteDb.dacpac");
		using Package dacpac = Package.Open(dacpacFile);
		var modelPart = dacpac.GetPart(new Uri("/model.xml", UriKind.Relative));
		using Stream modelStream = modelPart.GetStream();
		var xDoc = XDocument.Load(modelStream);
		var ns = xDoc.Root!.GetDefaultNamespace();

		var regex = DboViewNameRegex();
		var elements = xDoc.Descendants(ns + "Element")
			.Where(e => e.Attribute("Name") != null && regex.IsMatch(e.Attribute("Name")!.Value));
		Assert.Multiple(() =>
		{
			foreach (var element in elements)
			{
				var name = element.Attribute("Name")!.Value;
				var match = regex.Match(name);
				if (match.Success)
				{
					var viewName = match.Groups["viewName"].Value;
					var metaDataJsonFile = $"MetaData/{viewName}.metadata.json";
					Assert.That(File.Exists(metaDataJsonFile), Is.True, $@"Metadata file for {viewName} does not exist: {metaDataJsonFile}, please Run Bin\Tools\net8.0\CargoWise.RefDbRepo.MetaDataGenerator.exe");
				}
			}
		});
	}

	[GeneratedRegex(@"^\[dbo\]\.\[(?<viewName>[A-Za-z0-9_]+_V\d+)\]$")]
	private static partial Regex DboViewNameRegex();
}

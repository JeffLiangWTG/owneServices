using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using CargoWise.RefDbRepo.LLIReferenceData.Business.Vessel;
using CargoWise.RefDbRepo.LLIReferenceData.Services.Entities;
using CargoWise.RefDbRepo.LLIReferenceData.Services.Vessel;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.LLIReferenceData.Tests.Vessel;

[TestFixture]
public class VesselParserTest
{
	Assembly assembly;

	[SetUp]
	public void Setup()
	{
		assembly = Assembly.GetExecutingAssembly();
	}

	[Test]
	public void ExportToXMLFile()
	{
		var vessels = ReadItemsFromJsonResource<LLIReferenceData.Services.Entities.Vessel>("CargoWise.RefDbRepo.LLIReferenceData.Tests.Vessel.TestFiles.Input.LLI_VesselList.json");
		var vesselBasicCharacteristics = ReadItemsFromJsonResource<VesselBasicCharacteristic>("CargoWise.RefDbRepo.LLIReferenceData.Tests.Vessel.TestFiles.Input.LLI_VesselBasicCharacteristics.json");
		var vesselAdvancedCharacteristics = ReadItemsFromJsonResource<VesselAdvancedCharacteristic>("CargoWise.RefDbRepo.LLIReferenceData.Tests.Vessel.TestFiles.Input.LLI_VesselAdvancedCharacteristics.json");

		var expectedImportXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.LLIReferenceData.Tests.Vessel.TestFiles.Output.RefVesselList_LLI_Vessel.xml");
		var outputFolderPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"TestFiles");
		Directory.CreateDirectory(outputFolderPath);
		var outputFileFullName = Path.Combine(outputFolderPath, "RefVesselList_LLI_Vessel.xml");

		VesselParser.ParseAndExport(
			new LliVesselsData(
				vessels.ToArray(),
				vesselBasicCharacteristics.ToArray(),
				vesselAdvancedCharacteristics.ToArray()),
			outputFolderPath,
			new DateTime(2025, 2, 14));

		var realImportXML = File.ReadAllText(outputFileFullName);

		Assert.That(expectedImportXML, Is.EqualTo(realImportXML));

		File.Delete(outputFileFullName);
	}

	static IEnumerable<T> ReadItemsFromJsonResource<T>(string resourceName)
	{
		var jsonString = TestHelper.ReadManifestResourceContent(resourceName);
		return JsonSerializer.Deserialize<IEnumerable<T>>(jsonString);
	}
}

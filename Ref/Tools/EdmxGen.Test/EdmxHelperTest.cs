using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EdmxGen.Test
{
	[TestFixture]
	public class EdmxHelperTest
	{
		[Test]
		public void IsRemovingContentFromMainNodes()
		{
			using (var file = Assembly.GetExecutingAssembly().GetManifestResourceStream("CargoWise.RefDbRepo.EdmxGen.Test.testfiles.ReferenceDataModel.edmx"))
			{
				var currentDoc = XDocument.Load(file);

				var rootElement = EdmxHelper.GetEdmxRootElement(currentDoc);
				var storageModelsNode = EdmxHelper.GetStorageModels(rootElement);
				var conceptualModelsNode = EdmxHelper.GetStorageModels(rootElement);
				var mappingsNode = EdmxHelper.GetMappings(rootElement);

				Assert.True(storageModelsNode.Elements().Any());
				Assert.True(conceptualModelsNode.Elements().Any());
				Assert.True(mappingsNode.Elements().Any());
			}

			using (var file = Assembly.GetExecutingAssembly().GetManifestResourceStream("CargoWise.RefDbRepo.EdmxGen.Test.testfiles.ReferenceDataModel.edmx"))
			{
				var modifiedDocument = EdmxHelper.LoadDocument(file, string.Empty);

				var rootElement2 = EdmxHelper.GetEdmxRootElement(modifiedDocument);
				var storageModelsNode2 = EdmxHelper.GetStorageModels(rootElement2);
				var conceptualModelsNode2 = EdmxHelper.GetStorageModels(rootElement2);
				var mappingsNode2 = EdmxHelper.GetMappings(rootElement2);

				Assert.False(storageModelsNode2.Elements().Any());
				Assert.False(conceptualModelsNode2.Elements().Any());
				Assert.False(mappingsNode2.Elements().Any());
			}
		}

		[Test]
		public void GenerateEmbeddedResources()
		{
			var testFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "testfiles");
			var testEdmxFilePath = Path.Combine(testFolder, "ReferenceDataModel.edmx");
			var tempPath = Path.Combine(Path.GetTempPath(), "{BCBA4881-352B-4A20-84BF-EB699A49C8E8}");
			if (Directory.Exists(tempPath))
			{
				Directory.Delete(tempPath, true);
			}
			Directory.CreateDirectory(tempPath);

			var tempFilePath = Path.Combine(tempPath, "ReferenceDataModel.edmx");
			File.Move(testEdmxFilePath, tempFilePath);
			EdmxHelper.GenerateEmbeddedResources(tempFilePath);
			var resourceFiles = new[] { "ReferenceDataModel.ssdl", "ReferenceDataModel.csdl", "ReferenceDataModel.msl" };
			foreach (var resourceFile in resourceFiles)
			{
				var generatedFile = Path.Combine(tempPath, resourceFile);
				var generatedContents = File.ReadAllText(generatedFile);
				var expectedContents = File.ReadAllText(Path.Combine(testFolder, resourceFile));
				Assert.True(File.Exists(Path.Combine(tempPath, "ReferenceDataModel.ssdl")));
				Assert.AreEqual(expectedContents, generatedContents);
			}

			Directory.Delete(tempPath, true);
		}
	}
}

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.RefDbRepo.XmlProducer.Common;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ProducerStatusCodeListGenerator.Test
{
	[TestFixture]
	public class ProducerStatusCodeListFixture
	{
		[Test]
		public void CheckJsonOutputForProducerStatus()
		{
			var codeList = Enum.GetValues(typeof(ProducerStatus)).Cast<ProducerStatus>().ToList();
			codeList.RemoveAt(0);
			var partialJson = string.Join(",", codeList.Select(x => "{\"flagName\":\"" + x.ToString() + "\",\"flagValue\":" + (int)x + "}"));
			var expectedJson = $"[{partialJson}]";

			var binFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var jsonFilePath = Path.Combine(binFolder, "producerStatusCodeList.json");
			if (File.Exists(jsonFilePath))
			{
				File.Delete(jsonFilePath);
			}

			var producerStatusCodeListGenerator = new ProducerStatusCodeListGenerator(binFolder);
			producerStatusCodeListGenerator.Run();
			Assert.That(File.Exists(jsonFilePath));

			var jsonContent = File.ReadAllText(jsonFilePath);
			Assert.AreEqual(expectedJson, jsonContent);
		}
	}
}

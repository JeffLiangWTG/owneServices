using System;
using System.IO;
using System.Xml.Linq;
using CargoWise.RefDbRepo.PLReferenceData.Business.Helpers;
using CargoWise.RefDbRepo.PLReferenceData.Business.Taric4;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.PLReferenceData.Tests.Taric4;

[TestFixture]
sealed class TaricFileNormalizerTest
{
	[Test]
	public void TestNormalize()
	{
		var tempFile = Path.GetTempFileName();
		using (FileStream fs = File.OpenWrite(tempFile))
		{
			TestHelper.GetManifestResourceStream("CargoWise.RefDbRepo.PLReferenceData.Tests.Taric4.TestFiles.Input.miniBase_notNormalized.xml").CopyTo(fs);
		}

		var expected = XDocument.Load(TestHelper.GetManifestResourceStream("CargoWise.RefDbRepo.PLReferenceData.Tests.Taric4.TestFiles.Output.miniBase_normalized.xml"));

		XDocument result = null;

		try
		{
			TaricFileNormalizer.Normalize(tempFile, tempFile);

			// we need to deserialize and then serialize otherwise its failing (difference in whitespace number)
			var serialized = XmlParser.DeserializeFromFile<IsztarHistoryResponse>(tempFile);
			result = XmlParser.Serialize(serialized);
		}
		catch (Exception ex)
		{
			Assert.Fail($"{ex.Message}");
		}

		Assert.AreEqual(expected.ToString(), result.ToString());

		File.Delete(tempFile);
	}
}

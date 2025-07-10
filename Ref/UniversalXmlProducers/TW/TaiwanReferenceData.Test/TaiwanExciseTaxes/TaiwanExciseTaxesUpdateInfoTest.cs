using System;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using CargoWise.RefDbRepo.TaiwanReferenceData.TurnkeyPlugInUpdateService;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TaiwanReferenceData.Test
{
	[TestFixture]
	class TaiwanExciseTaxesUpdateInfoTest
	{
		[Test]
		public void TestTaiwanExciseTaxes()
		{
			var fileName = Path.Combine(Utility.TempDirectory, string.Format(CultureInfo.InvariantCulture, "{0}.xml", Guid.NewGuid().ToString()));
			var binFolder = FolderHelper.GetBinFolder();
			var inputFilePath = Path.Combine(binFolder, "doc/TaiwanExciseTaxes/TaiwanExciseTaxes.xlsx");
			var expectedFilePath = Path.Combine(binFolder, "doc/TaiwanExciseTaxes/TaiwanExciseTaxes.xml");
			var updateInfo = new TaiwanExciseTaxesUpdateInfo(new updateInfoBean(), inputFilePath, fileName, new DateTime(2024, 04, 12, 11, 27, 31));
			updateInfo.Run();
			var actualXml = XDocument.Load(fileName);
			var expectedXml = XDocument.Load(expectedFilePath);
			TestHelper.AssertXMLEquals(expectedXml.ToString(), actualXml.ToString());
			File.Delete(fileName);
		}
	}
}

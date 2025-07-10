using System.Diagnostics.CodeAnalysis;
using System.IO;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Data.Universal.Testing
{
	static class TestXmlReader
	{
		const string TestFilesPath = @"Enterprise\Product\Operations\Customs\NZ\Core\Business.Test\DataImportExport\Universal\SeaCargo\TestFiles\";

		[SuppressMessage("CargoWiseOne", "CW1051:Do not use BaseSourcePath", Justification = "Consumed tests have SOURCE_CODE")]
		public static string GetFileContents(string fileName)
		{
			return File.ReadAllText(GetFullPath(fileName));
		}

		[SuppressMessage("CargoWiseOne", "CW1051:Do not use BaseSourcePath", Justification = "Consumed tests have SOURCE_CODE")]
		static string GetFullPath(string fileName)
		{
			if (!fileName.EndsWith(".xml"))
			{
				fileName += ".xml";
			}
			return Path.Combine(TestCase.BaseSourcePath, TestFilesPath, fileName);
		}
	}
}

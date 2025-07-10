using System.IO;
using Enterprise.Customs.DataTransfer.Testing;

namespace Enterprise.Customs.DataTransfer.Universal.SeaManifest.Testing
{
	static class TestXmlReader
	{
		public static string GetFileContents(this TestFileHelper testFileHelper, string fileName)
		{
			return File.ReadAllText(GetFullPath(testFileHelper, fileName));
		}

		static string GetFullPath(TestFileHelper testFileHelper, string fileName)
		{
			if (!fileName.EndsWith(".xml"))
			{
				fileName += ".xml";
			}
			return testFileHelper.GetPathForUniversalHVLVSeaTestFiles(fileName);
		}
	}
}

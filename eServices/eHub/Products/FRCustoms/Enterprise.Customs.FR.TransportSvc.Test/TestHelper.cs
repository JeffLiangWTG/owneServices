using System.IO;
using System.Reflection;
using Enterprise.Customs.FR.TransportSvc.Utilities;

namespace Enterprise.Customs.FR.TransportSvc.Test
{
	public static class TestHelper
	{
		public static string GetEmbeddedResourceFileContent(string embeddedResourceFile)
		{
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Enterprise.Customs.FR.TransportSvc.Test.TestFiles." + embeddedResourceFile))
			using (var sr = new StreamReader(stream))
			{
				return sr.ReadToEnd();
			}
		}

		public static Stream GetEmbeddedResourceFileStream(string embeddedResourceFile)
		{
			return Assembly.GetExecutingAssembly().GetManifestResourceStream("Enterprise.Customs.FR.TransportSvc.Test.TestFiles." + embeddedResourceFile);
		}

		public static string CreateTestDirectory()
		{
			var testDirectory = Path.Combine(Path.GetTempPath(), "TransportSvcTest");
			if (!Directory.Exists(testDirectory))
			{
				Directory.CreateDirectory(testDirectory);
			}
			return testDirectory;
		}

		public static string CreateLogDirectory()
		{
			var testDirectory = Path.Combine(Path.GetTempPath(), "TransportSvcLogs");
			if (!Directory.Exists(testDirectory))
			{
				Directory.CreateDirectory(testDirectory);
			}
			return testDirectory;
		}

		public static void CreateFtpKey(string directory)
		{
			var keyPath = Path.Combine(directory, ApplicationConfig.Instance.FTP_User);

			if (!File.Exists(keyPath))
			{
				ApplicationConfig.Instance.FTP_PrivateKeyFileDirectory = directory;
				var keyContent = GetEmbeddedResourceFileContent("cw1_fr_customs_test");
				File.WriteAllText(keyPath, keyContent);
			}
		}
	}
}

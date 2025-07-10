using System.IO;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CarrierMessagingBuss.Shared.Test
{
	[SetUpFixture]
	internal class Setup
	{
		static string testFolderPath = "CarrierMessagingBussTest";

		public static string TestFolderPath => testFolderPath;

		[OneTimeSetUp]
		public void GlobalSetup()
		{
			testFolderPath = Path.Combine(Path.GetTempPath(), testFolderPath);
			if (!Directory.Exists(testFolderPath))
			{
				Directory.CreateDirectory(testFolderPath);
			}
			else
			{
				foreach (var file in Directory.GetFiles(testFolderPath))
				{
					File.Delete(file);
				}
			}
		}

		[OneTimeTearDown]
		public void GlobalTeardown()
		{
			if (Directory.Exists(testFolderPath))
			{
				Directory.Delete(testFolderPath, recursive: true);
			}
		}
	}
}

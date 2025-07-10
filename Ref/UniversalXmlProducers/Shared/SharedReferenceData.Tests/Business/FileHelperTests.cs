using System.IO;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.SharedReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.SharedReferenceData.Tests
{
	[TestFixture]
	class FileHelperTests
	{
		[Test]
		public void TestCreatDirectoryIfRequired()
		{
			if (Directory.Exists(sourceDirectory))
			{
				Directory.Delete(sourceDirectory, true);
			}
			Assert.IsFalse(Directory.Exists(sourceDirectory));
			FileHelper.CreatDirectoryIfRequired(sourceDirectory);
			Assert.IsTrue(Directory.Exists(sourceDirectory));
		}

		[Test]
		public void TestMoveFile()
		{
			FileHelper.CreatDirectoryIfRequired(sourceDirectory);
			FileHelper.CreatDirectoryIfRequired(destinationDirectory);
			var sourceFileName = Path.Combine(sourceDirectory, "move.txt");
			var destFileName = Path.Combine(destinationDirectory, "move.txt");
			using (var fs = File.Create(sourceFileName))
			{
				byte[] info = new UTF8Encoding(true).GetBytes("Test");
				fs.Write(info, 0, info.Length);
			}
			Assert.IsTrue(File.Exists(sourceFileName));
			FileHelper.MoveFile(sourceFileName, destFileName);
			Assert.IsTrue(!File.Exists(sourceFileName));
			Assert.IsTrue(File.Exists(destFileName));
		}

		[Test]
		public void TestGetFileNames()
		{
			var sBuilder = new StringBuilder();
			Assert.IsNull(FileHelper.GetFileNames("", sBuilder));

			FileHelper.GetFileNames("Invalid|||", sBuilder);
			Assert.That(sBuilder.ToString(), Does.Contain("Failed load files, message:"));

			FileHelper.CreatDirectoryIfRequired(sourceDirectory);
			using (File.Create(Path.Combine(sourceDirectory, "get.txt")))
			using (File.Create(Path.Combine(sourceDirectory, "get1.json")))
			using (File.Create(Path.Combine(sourceDirectory, "get2.Json")))
			{
				var files = FileHelper.GetFileNames(sourceDirectory, new StringBuilder());
				Assert.IsTrue(files.Length == 2);
				Assert.IsTrue(files.Any(x => x.EndsWith("get1.json")));
				Assert.IsTrue(files.Any(x => x.EndsWith("get2.Json")));
			}
		}

		[Test]
		public void TestGetFileContents()
		{
			FileHelper.CreatDirectoryIfRequired(sourceDirectory);
			var sBuilder = new StringBuilder();
			FileHelper.GetFileContents("XXX", sBuilder);
			Assert.That(sBuilder.ToString(), Does.Contain("Failed read file:"));
			var path = Path.Combine(sourceDirectory, "content.txt");
			using (var fs = File.Create(path))
			{
				byte[] info = new UTF8Encoding(true).GetBytes("This is some text in the file.\r\n");
				fs.Write(info, 0, info.Length);
			}

			var fileContents = FileHelper.GetFileContents(path, new StringBuilder());
			Assert.AreEqual("This is some text in the file.", fileContents);
		}

		string sourceDirectory => Path.Combine(Path.GetTempPath(), "SourceTest");
		string destinationDirectory => Path.Combine(Path.GetTempPath(), "DestinationTest");
	}
}

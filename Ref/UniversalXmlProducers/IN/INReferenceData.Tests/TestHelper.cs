using System;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.INReferenceData.Tests
{
	static class TestHelper
	{
		internal static string ReadContentString(string relativePath) => ReadContent(relativePath, File.ReadAllText);

		internal static byte[] ReadContentBytes(string relativePath) => ReadContent(relativePath, File.ReadAllBytes);

		static T ReadContent<T>(string relativePath, Func<string, T> readFile)
		{
			var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
			return readFile.Invoke(filePath);
		}

		internal static string RemoveIgnoredArgsFromXml(string xmlValue)
		{
			var result = xmlValue;
			var regex = new Regex(@"<PublicationTime>(.*?)</PublicationTime>");
			result = regex.Replace(result, "<PublicationTime></PublicationTime>");

			regex = new Regex(@"(?s)  <!--Internal Tags Start(.*?)<!--Internal Tags End-->\r\n");
			result = regex.Replace(result, "");
			return result;
		}

		internal static string CreateTempFolder()
		{
			var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			Directory.CreateDirectory(tempFolder);
			return tempFolder;
		}

		internal static void AssertFileCount(string path, int expectedCount, string message)
		{
			Assert.IsTrue(Directory.Exists(path), $"Path {path} does not exist");
			var files = Directory.GetFiles(path);
			Assert.AreEqual(expectedCount, files.Length, message);
		}

		internal static void AssertContainFileNames(string path, string message, params string[] expectedFileNames)
		{
			Assert.IsTrue(Directory.Exists(path), $"Path {path} does not exist");
			var files = Directory.GetFiles(path);
			foreach (var expectedFileName in expectedFileNames)
			{
				Assert.IsTrue(Array.Exists(files, f => f.Contains(expectedFileName)), message);
			}
		}
	}
}

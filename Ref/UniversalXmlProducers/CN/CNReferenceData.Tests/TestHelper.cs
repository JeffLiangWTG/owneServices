using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.CNReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CNReferenceData.Tests
{
	public static class TestHelper
	{
		public const string ExpectedResourceNamespace = "CargoWise.RefDbRepo.CNReferenceData.Tests.CN.Tariffs.TestFiles.Expected";
		public const string InputResourceNamespace = "CargoWise.RefDbRepo.CNReferenceData.Tests.CN.Tariffs.TestFiles.Input";

		public static string ToExpectedResourceFullPath(this string fileName) => $"{ExpectedResourceNamespace}.{fileName}";

		public static string ToInputResourceFullPath(this string fileName) => $"{InputResourceNamespace}.{fileName}";

		static string IgnoreBreakingsAndIndentations(this string input)
		{
			var output = input.Replace(Environment.NewLine, string.Empty);
			output = Regex.Replace(output, @"\s+(?=\<)", string.Empty);
			return output;
		}

		static string IgnoreNodesAndFormatting(this string xml) => RemoveNodesByTag(xml, "AppProgramArgs", "AppName").IgnoreBreakingsAndIndentations();

		static string RemoveNodesByTag(this string xml, params string[] tags)
		{
			foreach (var tag in tags)
			{
				var regex = new Regex($"  <{tag}>(.*?)</{tag}>");
				var match = regex.Match(xml);
				if (match.Success)
				{
					xml = xml.Replace(match.Value, "");
				}
			}
			return xml;
		}

		public static string ReadOutputText(string fileName)
		{
			return File.ReadAllText(GlobalOption.Instance.Setting.GetFullOutputFileName(fileName));
		}

		public static void ClearOutputFiles()
		{
			foreach (var filepath in GlobalOption.Instance.OutputFiles)
			{
				if (File.Exists(filepath))
				{
					File.Delete(filepath);
				}
				Assert.IsTrue(!File.Exists(filepath));
			}
		}

		public static string CreateTempXmlFileName() => $"{Guid.NewGuid()}.xml";

		public static void AssertXmlEquals(string expectedXml, string actualXml, string message = null)
		{
			Assert.AreEqual(expectedXml.IgnoreNodesAndFormatting(), actualXml.IgnoreNodesAndFormatting(), message);
		}

		public static void AssertXmlFileContentEquals(string resourceName, string actualXmlFileName, string message = null)
		{
			AssertXmlEquals(ReadManifestResourceContent(resourceName), ReadOutputText(actualXmlFileName), message);
		}

		public static Stream GetManifestResourceStream(string resourceName)
		{
			return Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
		}

		public static string ReadManifestResourceContent(string resourceName)
		{
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
			using (var reader = new StreamReader(stream))
			{
				return reader.ReadToEnd();
			}
		}
	}
}

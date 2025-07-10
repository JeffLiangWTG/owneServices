using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	public static class StreamCompareHelper
	{
		public static void CompareStreamContent(Stream expectedStream, Stream actualStream)
		{
			Assert.IsNotNull(expectedStream);
			Assert.IsNotNull(actualStream);

			expectedStream.Position = 0;
			actualStream.Position = 0;

			using (var expectedReader = new StreamReader(expectedStream, Encoding.UTF8))
			using (var actualReader = new StreamReader(actualStream, Encoding.UTF8))
			{
				var expected = expectedReader.ReadToEnd().CorrectBeforeComparison();
				var actual = actualReader.ReadToEnd().CorrectBeforeComparison();
				Assert.AreEqual(expected, actual);
			}
		}

		public static T JsonStreamToObject<T>(this Stream input) => JsonConvert.DeserializeObject<T>(StreamToString(input));

		public static string StreamToString(this Stream input)
		{
			using (var actualReader = new StreamReader(input, Encoding.UTF8))
			{
				return actualReader.ReadToEnd();
			}
		}

		static string CorrectBeforeComparison(this string input) => input.RemoveAppProgramArgs().RemoveAppName().RemoveIgnoreChars();

		static string RemoveIgnoreChars(this string input) => input.Replace("\n", "").Replace("\r", "");

		static string RemoveAppProgramArgs(this string xml) => RemoveNodesByTag(xml, "AppProgramArgs");

		static string RemoveAppName(this string xml) => RemoveNodesByTag(xml, "AppName");

		static string RemoveNodesByTag(this string xml, string tag)
		{
			var regex = new Regex($"  <{tag}>(.*?)</{tag}>");
			var match = regex.Match(xml);
			if (match.Success)
			{
				xml = xml.Replace(match.Value, "");
			}
			return xml;
		}
	}
}

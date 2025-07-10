using System.IO;
using System.Linq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.Common.Test
{
	[TestFixture]
	public class CSVHelperTest
	{
		[Test]
		public void SplitCSVIntoListOfStringArrayTest()
		{
			var stringWithHeading = @"Code,Description
MOL,MOL South Africa(Pty) Ltd";
			var stringWithoutHeading = @"MOL,MOL South Africa(Pty) Ltd";
			var resultWithHeading = CSVHelper.GetCSVLinesInArrays(true, stringWithHeading);
			Assert.That(resultWithHeading.Count == 1, Is.EqualTo(true));
			Assert.That(resultWithHeading.ElementAt(0).ElementAt(0) == "MOL", Is.EqualTo(true));
			Assert.That(resultWithHeading.ElementAt(0).ElementAt(1) == "MOL South Africa(Pty) Ltd", Is.EqualTo(true));
			var resultWithoutHeading = CSVHelper.GetCSVLinesInArrays(false, stringWithoutHeading);
			Assert.That(resultWithoutHeading.Count == 1, Is.EqualTo(true));
			Assert.That(resultWithoutHeading.ElementAt(0).ElementAt(0) == "MOL", Is.EqualTo(true));
			Assert.That(resultWithoutHeading.ElementAt(0).ElementAt(1) == "MOL South Africa(Pty) Ltd", Is.EqualTo(true));
		}

		[Test]
		public void SplitCSVWithTextQualifierIntoListOfStringArrayTest()
		{
			var stringWithHeading = @"Code,Description
MOL,""MOL, South Africa(Pty) Ltd""";
			var stringWithoutHeading = @"MOL,""MOL, South Africa(Pty) Ltd""";

			using (var stream = new MemoryStream())
			using (var writer = new StreamWriter(stream))
			{
				writer.Write(stringWithHeading);
				writer.Flush();
				stream.Position = 0;

				var resultWithHeading = CSVHelper.GetCSVLinesInArrays(true, stream, true);
				Assert.That(resultWithHeading.Count == 1, Is.EqualTo(true));
				Assert.That(resultWithHeading.ElementAt(0).ElementAt(0) == "MOL", Is.EqualTo(true));
				Assert.That(resultWithHeading.ElementAt(0).ElementAt(1) == "MOL, South Africa(Pty) Ltd", Is.EqualTo(true));
			}

			using (var stream = new MemoryStream())
			using (var writer = new StreamWriter(stream))
			{
				writer.Write(stringWithoutHeading);
				writer.Flush();
				stream.Position = 0;

				var resultWithoutHeading = CSVHelper.GetCSVLinesInArrays(false, stream, true);
				Assert.That(resultWithoutHeading.Count == 1, Is.EqualTo(true));
				Assert.That(resultWithoutHeading.ElementAt(0).ElementAt(0) == "MOL", Is.EqualTo(true));
				Assert.That(resultWithoutHeading.ElementAt(0).ElementAt(1) == "MOL, South Africa(Pty) Ltd", Is.EqualTo(true));
			}
		}
	}
}

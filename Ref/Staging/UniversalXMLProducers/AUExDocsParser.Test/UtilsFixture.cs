using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.AUExDocsParser.Test
{
	[TestFixture]
	public class UtilsFixture
	{
		[Test]
		public void TestGenerateSqlStringForE21()
		{
			string fileCode = "E21";
			var parser = new E21_ProductTypeParser();
			var result = parser.Parse(Path.Combine(BinPath, $@"Res\{fileCode}Test.TXT"));
			var actual = Utils.GenerateSqlStringForE21(result);
			var expected = File.ReadAllText(Path.Combine(BinPath, $@"Res\ExpectedSql{fileCode}.TXT"));
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void TestGenerateSqlStringForE01()
		{
			string fileCode = "E01";
			var parser = new E01_AqisPlaceParser();
			var result = parser.Parse(Path.Combine(BinPath, $@"Res\{fileCode}Test.TXT"));
			var actual = Utils.GenerateSqlStringForE01(result);
			var expected = File.ReadAllText(Path.Combine(BinPath, $@"Res\ExpectedSql{fileCode}.TXT"));
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void TestGenerateSqlStringForE29()
		{
			string fileCode = "E29";
			var parser = new E29_AqisPlaceParser();
			var result = parser.Parse(Path.Combine(BinPath, $@"Res\{fileCode}Test.TXT"));
			var actual = Utils.GenerateSqlStringForE29(result);
			var expected = File.ReadAllText(Path.Combine(BinPath, $@"Res\ExpectedSql{fileCode}.TXT"));
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void TestGenerateSqlStringForE25()
		{
			string fileCode = "E25";
			var parser = new E25_SupplementaryCodeParser();
			var result = parser.Parse(Path.Combine(BinPath, $@"Res\{fileCode}Test.TXT"));
			var actual = Utils.GenerateSqlStringForE25(result);
			var expected = File.ReadAllText(Path.Combine(BinPath, $@"Res\ExpectedSql{fileCode}.TXT"));
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void TestGenerateSqlStringForE38()
		{
			string fileCode = "E38";
			var parser = new E38_DominantProductParser();
			var result = parser.Parse(Path.Combine(BinPath, $@"Res\{fileCode}Test.TXT"));
			var actual = Utils.GenerateSqlStringForE38(result);
			var expected = File.ReadAllText(Path.Combine(BinPath, $@"Res\ExpectedSql{fileCode}.TXT"));
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void TestGenerateSqlStringForE39()
		{
			string fileCode = "E39";
			var parser = new E39_ApprovedCertifierParser();
			var result = parser.Parse(Path.Combine(BinPath, $@"Res\{fileCode}Test.TXT"));
			var actual = Utils.GenerateSqlStringForE39(result);
			var expected = File.ReadAllText(Path.Combine(BinPath, $@"Res\ExpectedSql{fileCode}.TXT"));
			Assert.AreEqual(expected, actual);
		}

		string BinPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
	}
}

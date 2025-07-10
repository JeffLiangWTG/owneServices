using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	[TestFixture]
	public class DeclarableCodeParserFixture
	{
		[Test]
		public void AssertParse()
		{
			var declarableCodeFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\SampleDeclarableCodes.xlsx");
			var declarableCodeParser = new DeclarableCodeParser();
			var results = declarableCodeParser.Parse(declarableCodeFile).ToList();

			Assert.IsNotNull(results);
			Assert.AreEqual(15, results.Count);

			var firstRecord = results.First();
			Assert.AreEqual(firstRecord.TariffHeader, "0100000000 80");
			Assert.AreEqual(firstRecord.StartDate, new DateTime(1971, 12, 31));
			Assert.AreEqual(firstRecord.DeclarableStartDate, new DateTime(1972, 1, 1));
			Assert.False(firstRecord.IsLeaf);

			var lastRecord = results.Last();
			Assert.AreEqual(lastRecord.TariffHeader, "0102219000 80");
			Assert.AreEqual(lastRecord.StartDate, new DateTime(2012, 01, 01));
			Assert.AreEqual(lastRecord.DeclarableStartDate, new DateTime(2012, 01, 01));
			Assert.True(lastRecord.IsLeaf);
		}
	}
}

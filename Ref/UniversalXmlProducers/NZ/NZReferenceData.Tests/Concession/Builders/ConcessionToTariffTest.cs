using CargoWise.RefDbRepo.NZReferenceData.Business;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NZReferenceData.Tests
{
	class ConcessionToTariffTest
	{
		[Test]
		public void TestChapters_1()
		{
			var concessionToTariff = new ConcessionToTariff("100001C~**~**~**~**~**~1");
			Assert.AreEqual(new[] { "01", "02", "03", "04", "05" }, concessionToTariff.Chapters);
		}

		[Test]
		public void TestChapters_14()
		{
			var concessionToTariff = new ConcessionToTariff("100001C~**~**~**~**~**~14");
			Assert.AreEqual(new[] { "71" }, concessionToTariff.Chapters);
		}

		[Test]
		public void TestChapters_16()
		{
			var concessionToTariff = new ConcessionToTariff("100001C~**~**~**~**~**~16");
			Assert.AreEqual(new[] { "84", "85" }, concessionToTariff.Chapters);
		}

		[Test]
		public void TestSection()
		{
			var concessionToTariff = new ConcessionToTariff("100001C~**~**~**~**~**~14");
			Assert.AreEqual(14, concessionToTariff.Section);
		}

		[Test]
		public void TestTariff_Level0()
		{
			var concessionToTariff = new ConcessionToTariff("100001C~**~**~**~**~**~14");
			Assert.AreEqual("", concessionToTariff.Tariff);
		}

		[Test]
		public void TestTariff_Level3()
		{
			var concessionToTariff = new ConcessionToTariff("100019F~13~02~32~**~**~2");
			Assert.AreEqual("130232", concessionToTariff.Tariff);
		}

		[Test]
		public void TestCode()
		{
			var concessionToTariff = new ConcessionToTariff("100019F~13~02~32~**~**~2");
			Assert.AreEqual("100019F", concessionToTariff.Code);
		}

		[Test]
		public void TestGetConcessionToTariffs()
		{
			var lines = new[] { "100019F~13~02~32~**~**~2", "100010F~13~02~32~**~**~2" };
			Assert.AreEqual(2, ConcessionToTariff.GetConcessionToTariffs(lines, Mock.Of<ILogger>()).Count);
		}

		[Test]
		public void TestGetConcessionToTariffs_LogError()
		{
			var loggerMock = new Mock<ILogger>();
			var lines = new[] { "100019F~13~02~32~**~**~2", "100010F13~02~32~**~**~2" };
			var linesParsed = ConcessionToTariff.GetConcessionToTariffs(lines, loggerMock.Object);
			Assert.AreEqual(1, linesParsed.Count);
			loggerMock.Verify(x => x.LogError("Error during parsing, ConcessionToTariff skipped: Can't process data line. Line string: 100010F13~02~32~**~**~2"), Times.Once);
		}
	}
}

using System;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.NZReferenceData.Business;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NZReferenceData.Tests
{
	class ConcessionRatesTest
	{
		[Test]
		public void TestCode()
		{
			var concessionDetails = new ConcessionRates("100073L~NML~Feb 15 2002 11:59PM~~1~~~~~~");
			Assert.AreEqual("100073L", concessionDetails.Code);
		}

		[Test]
		public void TestExpiryDate()
		{
			var concessionDetails = new ConcessionRates("100073L~NML~Feb 15 2002 11:59PM~~1~~~~~~");
			Assert.AreEqual(DateTime.Parse("Feb 15 2002 11:59PM", CultureInfo.InvariantCulture), concessionDetails.ExpiryDate);
		}

		[Test]
		public void TestExpiryDate_UnableToParse()
		{
			Assert.Throws(Is.TypeOf<RefDataParseException>().And.Message.EqualTo("Unable to parse expiry date. Line string: 100073L~NML~Abc 15 2002 11:59PM~~1~~~~~~"), () => _ = new ConcessionRates("100073L~NML~Abc 15 2002 11:59PM~~1~~~~~~"));
		}

		[Test]
		public void TestRateGroup()
		{
			var concessionDetails = new ConcessionRates("100073L~NML~Feb 15 2002 11:59PM~~1~~~~~~");
			Assert.AreEqual("NML", concessionDetails.RateGroup);
		}

		[Test]
		public void TestFormulaNumber()
		{
			var concessionDetails = new ConcessionRates("100073L~NML~Feb 15 2002 11:59PM~~1~~~~~~");
			Assert.AreEqual(1, concessionDetails.FormulaNumber);
		}

		[Test]
		public void TestFormulaNumber_ThrowsException()
		{
			AssertErrorToDecodeFormula("100073L~NML~Feb 15 2002 11:59PM~~G~~~~~~", "Error during parsing formula: Unable to parse formula number. Line string: 100073L~NML~Feb 15 2002 11:59PM~~G~~~~~~");
		}

		[Test]
		public void TestFormula_Unsupported()
		{
			AssertErrorToDecodeFormula("100073L~NML~Feb 15 2002 11:59PM~~7~~~~~~", "Error during parsing formula: Unsupported formula number. Line string: 100073L~NML~Feb 15 2002 11:59PM~~7~~~~~~");
		}

		[Test]
		public void TestFormula_1()
		{
			var concessionDetails = new ConcessionRates("100073L~NML~Feb 15 2002 11:59PM~~1~~~~~~");
			Assert.AreEqual("0", concessionDetails.Formula);
		}

		[Test]
		public void TestFormula_2()
		{
			var concessionDetails = new ConcessionRates("100073L~NML~Feb 15 2002 11:59PM~~2~~~~~~");
			Assert.AreEqual("0", concessionDetails.Formula);
		}

		[Test]
		public void TestFormula_3()
		{
			var concessionDetails = new ConcessionRates("100073L~NML~Feb 15 2002 11:59PM~~3~30.00000~~~~~");
			Assert.AreEqual("0.3*VFD", concessionDetails.Formula);
		}

		[Test]
		public void TestFormula_3_UnableToParseFactorA()
		{
			AssertErrorToDecodeFormula("100073L~NML~Feb 15 2002 11:59PM~~3~30.F0000~~~~~", "Error during parsing formula: Unable to parse factor '30.F0000'. Line string: 100073L~NML~Feb 15 2002 11:59PM~~3~30.F0000~~~~~");
		}

		[Test]
		public void TestFormula_4()
		{
			var concessionDetails = new ConcessionRates("100073L~NML~Feb 15 2002 11:59PM~~4~1.906700~~~~~");
			Assert.AreEqual("1.9067*CU1", concessionDetails.Formula);
		}

		[Test]
		public void TestFormula_5()
		{
			var concessionDetails = new ConcessionRates("100073L~NML~Feb 15 2002 11:59PM~~5~135.000000~18.640000~~~~");
			Assert.AreEqual("(1.35*VFD)+(18.64*CU1)", concessionDetails.Formula);
		}

		[Test]
		public void TestFormula_5_UnableToParseFactorB()
		{
			AssertErrorToDecodeFormula("100073L~NML~Feb 15 2002 11:59PM~~5~30.0000~D~~~~", "Error during parsing formula: Unable to parse factor 'D'. Line string: 100073L~NML~Feb 15 2002 11:59PM~~5~30.0000~D~~~~");
		}

		void AssertErrorToDecodeFormula(string rateLine, string expectedErrorMessage)
		{
			var loggerMock = new Mock<ILogger>();
			var rate = new ConcessionRates(rateLine, loggerMock.Object);
			Assert.AreEqual(0, rate.FormulaNumber);
			Assert.AreEqual("0", rate.Formula);
			loggerMock.Verify(x => x.LogError(expectedErrorMessage), Times.Once);
		}

		[Test]
		public void TestFormula_6()
		{
			var concessionDetails = new ConcessionRates("100073L~NML~Feb 15 2002 11:59PM~~6~5.000000~7.500000~~~~");
			Assert.AreEqual("(5*[{0}])-(0.075*VFD)", concessionDetails.Formula);
		}

		[Test]
		public void TestGetConcessionRates()
		{
			var lines = new[] { "100073L~NML~Feb 15 2024 11:59PM~~4~1.906700~~~~~", "100013L~NML~Feb 15 2024 11:59PM~~4~1.906700~~~~~" };
			Assert.AreEqual(2, ConcessionRates.GetConcessionRates(lines, Mock.Of<ILogger>(), DateProvider).Count);
		}

		[Test]
		public void TestGetConcessionRates_LogError_Parse()
		{
			var loggerMock = new Mock<ILogger>();
			var lines = new[] { "100073L~NML~Feb 15 2024 11:59PM~~4~1.906700~~~~~", "100023L~NML~Feb 15 2024 11:59PM~~41~1.906700~~~~~" };
			var linesParsed = ConcessionRates.GetConcessionRates(lines, loggerMock.Object, DateProvider);
			Assert.AreEqual(2, linesParsed.Count);
			Assert.AreEqual("0", linesParsed["100023L"].First().Formula);
			loggerMock.Verify(x => x.LogError("Error during parsing formula: Unsupported formula number. Line string: 100023L~NML~Feb 15 2024 11:59PM~~41~1.906700~~~~~"), Times.Once);
		}

		[Test]
		public void TestGetConcessionRates_UnableToParse()
		{
			var loggerMock = new Mock<ILogger>();
			var lines = new[] { "100073L~NML~Feb 15 2024 11:59PM~~4~1.906700~~~~~", "100013L~NML~Feb 15 2020 11:59PM~~4~1.906700~~~~~", "100043L~NML~Feb 15 2010 11:59PM~~4~1.906700~~~~~" };
			var linesParsed = ConcessionRates.GetConcessionRates(lines, loggerMock.Object, DateProvider);
			Assert.AreEqual(2, linesParsed.Count);
		}

		IDateProvider DateProvider => Mock.Of<IDateProvider>(x => x.Today == new DateTime(2022, 1, 1) && x.ActiveDate == x.Today.AddYears(-5));
	}
}

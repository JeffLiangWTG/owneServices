using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests.ExchangeRate
{
	[TestFixture]
	[SetCulture("en-AU")]
	public class XCHAGRATEParserTest : ExchangeRateParserTest<RefExchangeRateZZ>
	{
		protected override string MockTextFileName => "XCHGRATE-P1-EDMAIN-2002220144.txt";
		protected override string MockXmlFileName => "XCHGRATE-P1-EDMAIN-2002220144.xml";
		protected override string MockWebPageFileName => MockXCHAGRATEParser.LocationWebPageFileName;
		protected override DateTime ExpectedPublicationDateTime => new DateTime(2020, 03, 20);

		protected override IEnumerable<RefExchangeRateZZ> ExpectedEntities => new[]
		{
			new RefExchangeRateZZ
			{
				ZZN_RX_NKExCurrency = "AUD",
				ZZN_Rate = 1.0000m,
				ZZN_StartDate = new DateTime(2000, 1, 1),
				ZZN_EndDate = DateTime.Today
			},
			new RefExchangeRateZZ
			{
				ZZN_RX_NKExCurrency = "BRL",
				ZZN_Rate = 2.2022m,
				ZZN_StartDate = new DateTime(2003, 2, 18),
				ZZN_EndDate = new DateTime(2020, 2, 18)
			},
			new RefExchangeRateZZ
			{
				ZZN_RX_NKExCurrency = "ZAR",
				ZZN_Rate = 10.0344m,
				ZZN_StartDate = new DateTime(2020, 2, 19),
				ZZN_EndDate = DateTime.Today
			}
		};

		protected override void CompareEntity(RefExchangeRateZZ expected, RefExchangeRateZZ actual)
		{
			Assert.AreEqual(expected.ZZN_RX_NKExCurrency, actual.ZZN_RX_NKExCurrency, "ZZN_RX_NKExCurrency");
			Assert.AreEqual(expected.ZZN_Rate, actual.ZZN_Rate, "ZZN_Rate");
			Assert.AreEqual(expected.ZZN_StartDate, actual.ZZN_StartDate, "ZZN_StartDate");
			Assert.AreEqual(expected.ZZN_EndDate, actual.ZZN_EndDate, "ZZN_EndDate");
		}

		[Test]
		public void TestFilter()
		{
			var today = DateTime.Today;
			var auCulture = System.Globalization.CultureInfo.GetCultureInfo("en-AU");
			string Today(int days = 0) => today.AddDays(days).ToString("yyyyMMdd", auCulture);

			var sourceText = $@"
AUD 20000101000000000000 20210101000000000000       1.0000 {Today(-20)} {Today(-20)}
AUD 20000101000000000000 20210101000000000000       1.0000 {Today(-19)} {Today(-19)}
AUD 20000101000000000000 20210101000000000000       1.0000 {Today(-18)} {Today(-18)}
AUD 20000101000000000000 20210101000000000000       1.0000 {Today(-17)} {Today(-17)}
AUD 20000101000000000000 20210101000000000000       1.0000 {Today(-16)} {Today(-16)}
AUD 20000101000000000000 20210101000000000000       1.0000 {Today(-15)} {Today(-15)}
AUD 20000101000000000000 20210102000000000000       1.0000 {Today(-14)} {Today(-14)}
AUD 20000101000000000000 20210103000000000000       1.0000 {Today(-13)} {Today(-13)}
AUD 20000101000000000000 20210104000000000000       1.0000 {Today(-12)} {Today(-12)}
AUD 20000101000000000000 20210105000000000000       1.0000 {Today(-11)} {Today(-11)}
AUD 20000101000000000000 20210106000000000000       1.0000 {Today(-10)} {Today(-10)}
AUD 20000101000000000000 20210107000000000000       1.0000 {Today(-09)} {Today(-09)}
AUD 20000101000000000000 20210108000000000000       1.0000 {Today(-08)} {Today(-08)}
AUD 20000101000000000000 20210109000000000000       1.0000 {Today(-07)} {Today(-07)}
AUD 20000101000000000000 20210110000000000000       1.0000 {Today(-06)} {Today(-06)}
AUD 20000101000000000000 20210113000000000000       1.0000 {Today(-05)} {Today(-05)}
AUD 20000101000000000000 20210114000000000000       1.0000 {Today(-04)} {Today(-04)}
AUD 20000101000000000000 20210115000000000000       1.0000 {Today(-03)} {Today(-03)}
AUD 20000101000000000000 20210116000000000000       1.0000 {Today(-02)} {Today(-02)}
AUD 20000101000000000000 20210117000000000000       1.0000 {Today(-01)} {Today(-01)}
AUD 20000101000000000000 20210118000000000000       1.0000 {Today()}
";

			string sourceTextFileName = $"XCHGRATE-P1-EDTEST-{Today()}.txt";
			var textFilePath = Path.Combine(ExchangeRateTestHelper.TestFilesPath, sourceTextFileName);

			try
			{
				using (StreamWriter sw = File.CreateText(textFilePath))
				{
					sw.Write(sourceText);
				}

				var parser = new MockXCHAGRATEParser
				{
					IgnoreFilter = false
				};
				var entities = parser.ConvertData(textFilePath).ToList();

				Assert.AreEqual(14, entities.Count(e => e.ZZN_StartDate > today.AddDays(-14)));
				Assert.AreEqual(0, entities.Count(e => e.ZZN_StartDate < today.AddDays(-14)));
			}
			finally
			{
				if (File.Exists(textFilePath))
				{
					File.Delete(textFilePath);
				}
			}
		}
	}
}

using System;
using System.Linq;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Loader;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Models;
using CargoWise.RefDbRepo.ZAReferenceData.Tests.Helpers;
using CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Tariff.TestClasses;
using NUnit.Framework;
using static CargoWise.RefDbRepo.ZAReferenceData.Services.Common.Constants;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Tariff
{
	[TestFixture]
	public class ProdatLoaderTests
	{
		[Test]
		public void LoadHeader()
		{
			var msgContent = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Tariff.TestFiles.Input.D96B_Valid_01.txt");
			var msg = EdifactLoader.LoadProdatMessage(msgContent);

			var header  = ProdatLoader.PopulateHeader(msg);

			Assert.That(header, Is.Not.Null);
			Assert.That(header.TransactionType, Is.EqualTo(TransactionType.Change));
			Assert.That(header.PublicationDate, Is.EqualTo(new DateTime(2015, 7, 2)));
			Assert.That(header.GovernmentGazettePublicationNumber, Is.EqualTo("456"));
			Assert.That(header.Tariffs, Is.Not.Null.And.Not.Empty);
		}

		[Test]
		public void LoadTariff()
		{
			var msgContent = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Tariff.TestFiles.Input.D96B_Valid_01.txt");
			var msg = EdifactLoader.LoadProdatMessage(msgContent);
			var header = ProdatLoader.PopulateHeader(msg);

			var tariff = header.Tariffs.First();

			Assert.That(tariff.LineNumber, Is.EqualTo("5628"));
			Assert.That(tariff.ItemNumber, Is.EqualTo("123"));
			Assert.That(tariff.Heading, Is.EqualTo("22.08"));
			Assert.That(tariff.Code, Is.EqualTo("45"));
			Assert.That(tariff.SubHeading, Is.EqualTo("2208.30.10"));
			Assert.That(tariff.CheckDigit, Is.EqualTo("0"));

			Assert.That(tariff.StartDate, Is.EqualTo(new DateTime(2015, 1, 1)));
			Assert.That(tariff.EndDate, Is.EqualTo(new DateTime(9999, 12, 31)));

			Assert.That(tariff.StatisticalUnitOriginal, Is.EqualTo("LITRE"));
			Assert.That(tariff.Description, Is.EqualTo("DAAA-1DAAA-2DAAA-3DAAA-4DAAA-5DACB-1DACB-2DACB-3DACB-4DACB-5"));
			Assert.That(tariff.ImportedFrom, Is.EqualTo("I-1I-2I-3I-4I-5"));
			Assert.That(tariff.GovernmentGazetteNoticeNumber, Is.EqualTo("99"));
			Assert.That(tariff.ScheduleTypeCode, Is.EqualTo("1P1"));

			Assert.That(tariff.Rates, Is.Not.Null.And.Not.Empty);
			Assert.That(tariff.Rates.Count, Is.EqualTo(4));
		}

		[TestCase("A01_5", "FTX+A01+++A:B:C:D:E", RateTypes.Standard, "ABC", "D", "", "A01")]
		[TestCase("A01_3", "FTX+A01+++A:B:C: : ", RateTypes.Standard, "AB", "C", "", "A01")]
		[TestCase("A01_2", "FTX+A01+++A:B: : : ", RateTypes.Standard, "AB", "", "", "A01")]
		[TestCase("A02", "FTX+A02+++A:B:C:D:E", RateTypes.SADC, "AB", "C", "", "A02")]
		[TestCase("A03", "FTX+A03+++A:B:C:D:E", RateTypes.EU, "AB", "C", "", "A03")]
		[TestCase("A05", "FTX+A05+++A:B:C:D:E", RateTypes.Standard, "AB", "C", "", "A05")]
		[TestCase("A06", "FTX+A06+++A:B:C:D:E", RateTypes.Standard, "AB", "C", "", "A06")]
		[TestCase("A07_5", "FTX+A07+++A:B:C:D:E", RateTypes.Standard, "ABC", "D", "", "A07")]
		[TestCase("A07_3", "FTX+A07+++A:B:C: : ", RateTypes.Standard, "AB", "C", "", "A07")]
		[TestCase("A07_2", "FTX+A07+++A:B: : : ", RateTypes.Standard, "AB", "", "", "A07")]
		[TestCase("A08", "FTX+A08+++A:B:C:D:E", RateTypes.Standard, "ABC", "D", "", "A08")]
		[TestCase("A09", "FTX+A09+++A:B:C:D:E", RateTypes.Standard, "ABC", "D", "", "A09")]
		[TestCase("A11", "FTX+A11+++A:B:C:D:E", RateTypes.EFTA, "AB", "C", "", "A11")]
		[TestCase("A12", "FTX+A12+++A:B:C:D:E", RateTypes.MERCOSUR, "AB", "C", "", "A12")]
		[TestCase("A12_TO", "FTX+A12+++A T:O B:C:D:E", RateTypes.MERCOSUR, "A", "C", "B", "A12")]
		[TestCase("A13", "FTX+A13+++A:B:C:D:E'", RateTypes.AFCFTA, "AB", "C", "", "A13")]
		[TestCase("A13_TO", "FTX+A13+++A T:O B:C:D:E", RateTypes.AFCFTA, "A", "C", "B", "A13")]
		public void LoadRates(string code, string content, string expRateType, string expDescription, string expFormulaCode, string expCountryCodes, string expRateQualifier)
		{
			var result = loader.PopulateRate(content);

			Assert.That(result.RateType, Is.EqualTo(expRateType), $"{code} RateType");
			Assert.That(result.Description, Is.EqualTo(expDescription), $"{code} Description");
			Assert.That(result.FormulaCode, Is.EqualTo(expFormulaCode), $"{code} FormulaCode");
			Assert.That(result.Countries, Is.EqualTo(expCountryCodes), $"{code} CountryCodes");
			Assert.That(result.RateQualifier, Is.EqualTo(expRateQualifier), $"{code} RateQualifier");
		}

		[TestCase("Original", "BGM+6+0+9", "9")]
		[TestCase("Change", "BGM+6+0+4","4")]
		[TestCase("Deletion", "BGM+6+0+3", "3")]
		[TestCase("Addition", "BGM+6+0+2", "2")]
		[TestCase("Request", "BGM+6+0+13", "0")]
		[TestCase("Retransmission", "BGM+6+0+35", "0")]
		public void TransactionTypeMapping(string code, string content, string expectedTransactionType)
		{
			var result = loader.GetTransactionType(content);
			var valid = Enum.TryParse(expectedTransactionType, out TransactionType expTransType);

			Assert.That(valid, Is.True, $"Invalid Transaction Type ({expectedTransactionType}) for {code}");
			Assert.That(result, Is.EqualTo(expTransType), $"Transaction Type: {code}");
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			loader = new ProdatLoaderForTest();
		}

		ProdatLoaderForTest loader;
	}
}

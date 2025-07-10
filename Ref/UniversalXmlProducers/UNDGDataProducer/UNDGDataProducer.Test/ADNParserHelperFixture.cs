using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer.Test
{
	[TestFixture]
	public class ADNParserHelperFixture
	{
		[Test(ExpectedResult = 0)]
		public byte ConvertStringToByteShouldReturnZeroForEmptyString()
		{
			return ADNParserHelper.ConvertStringToByte("");
		}

		[TestCase("256")]
		[TestCase("1.1")]
		[TestCase("-1")]
		public void ConvertStringToByteShouldThrowExceptionForInvalidString(string str)
		{
			Assert.Throws<ApplicationException>(() => ADNParserHelper.ConvertStringToByte(str));
		}

		[TestCase("255", ExpectedResult = 255)]
		[TestCase("0", ExpectedResult = 0)]
		[TestCase("7", ExpectedResult = 7)]
		public byte ConvertStringToByte(string str)
		{
			return ADNParserHelper.ConvertStringToByte(str);
		}

		[Test]
		public void ConvertCSVIndexToIntShouldThrowExceptionForEmptyString()
		{
			Assert.Throws<ApplicationException>(() => ADNParserHelper.ConvertCSVIndexToInt(""));
		}

		[TestCase("a")]
		[TestCase("@")]
		[TestCase("ABcD")]
		public void ConvertCSVIndexToIntShouldThrowExceptionForInvalidString(string str)
		{
			Assert.Throws<ApplicationException>(() => ADNParserHelper.ConvertCSVIndexToInt(str));
		}

		[TestCase("A", ExpectedResult = 0)]
		[TestCase("B", ExpectedResult = 1)]
		[TestCase("AA", ExpectedResult = 26)]
		[TestCase("AB", ExpectedResult = 27)]
		[TestCase("DAB", ExpectedResult = 2731)]
		public int ConvertCSVIndexToInt(string str)
		{
			return ADNParserHelper.ConvertCSVIndexToInt(str);
		}

		[Test]
		public void CreateADNVariant()
		{
			var substances = new[]
			{
				new UNDGSubstanceADN() { ADN_UNNO = "1010" },
				new UNDGSubstanceADN() { ADN_UNNO = "1010"},
				new UNDGSubstanceADN() { ADN_UNNO = "2020"},
				new UNDGSubstanceADN() { ADN_UNNO = "2020"},
				new UNDGSubstanceADN() { ADN_UNNO = "2020"},
				new UNDGSubstanceADN() { ADN_UNNO = "3030"},
				new UNDGSubstanceADN() { ADN_UNNO = "4040"},
			};

			ADNParserHelper.CreateADNVariant(substances);

			Assert.AreEqual("A", substances[0].ADN_Variant);
			Assert.AreEqual("B", substances[1].ADN_Variant);
			Assert.AreEqual("A", substances[2].ADN_Variant);
			Assert.AreEqual("B", substances[3].ADN_Variant);
			Assert.AreEqual("C", substances[4].ADN_Variant);
			Assert.AreEqual("", substances[5].ADN_Variant);
			Assert.AreEqual("", substances[6].ADN_Variant);
		}

		[Test]
		public void ParseNameAndDescription()
		{
			var nameAndDescription = new[]
			{
				"AMMONIUM PICRATE dry or wetted with less than 10% water, by mass",
				"BLACK POWDER (GUNPOWDER), COMPRESSED or BLACK POWDER (GUNPOWDER), IN PELLETS",
				"GAS OIL or DIESEL FUEL or HEATING OIL, LIGHT (flash-point not more than 60 °C)",
				"MAGNESIUM or MAGNESIUM ALLOYS with more than 50% magnesium in pellets, turnings or ribbons",
			};

			var expected = new[]
			{
				new Tuple<string, string>[] { Tuple.Create("AMMONIUM PICRATE", "dry or wetted with less than 10% water, by mass") },
				new Tuple<string, string>[] { Tuple.Create("BLACK POWDER (GUNPOWDER), COMPRESSED", ""), Tuple.Create("BLACK POWDER (GUNPOWDER), IN PELLETS", "") },
				new Tuple<string, string>[] { Tuple.Create("GAS OIL", "(flash-point not more than 60 °C)"), Tuple.Create("DIESEL FUEL", "(flash-point not more than 60 °C)"), Tuple.Create("HEATING OIL, LIGHT", "(flash-point not more than 60 °C)") },
				new Tuple<string ,string>[] { Tuple.Create("MAGNESIUM", "with more than 50% magnesium in pellets, turnings or ribbons"), Tuple.Create("MAGNESIUM ALLOYS", "with more than 50% magnesium in pellets, turnings or ribbons") },
			};

			for (var i = 0; i < nameAndDescription.Length; i++)
			{
				Assert.AreEqual(expected[i], ADNParserHelper.ParseNameAndDescription(nameAndDescription[i]));
			}
		}
	}
}

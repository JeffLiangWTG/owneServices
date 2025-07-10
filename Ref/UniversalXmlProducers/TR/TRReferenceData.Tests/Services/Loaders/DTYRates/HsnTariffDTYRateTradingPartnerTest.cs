using System.Linq;
using CargoWise.RefDbRepo.TRReferenceData.Services.Loaders;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests.Services.Loaders
{
	[TestFixture]
	public class HsnTariffDTYRateTradingPartnerTests
	{
		[TestCase("IN", HsnTariffDTYRateTradingPartner.HsnTariffDTYRateTradingPartnerType.Country)]
		[TestCase("D8", HsnTariffDTYRateTradingPartner.HsnTariffDTYRateTradingPartnerType.TradeGroup)]
		[TestCase(" USA ", HsnTariffDTYRateTradingPartner.HsnTariffDTYRateTradingPartnerType.TradeGroup)]
		public void TestConstructorAssignment(string inputCode, HsnTariffDTYRateTradingPartner.HsnTariffDTYRateTradingPartnerType expectedType)
		{
			var partner = new HsnTariffDTYRateTradingPartner(inputCode);
			Assert.That(partner.Code, Is.EqualTo(inputCode.Trim()));
			Assert.That(partner.Type, Is.EqualTo(expectedType));
		}

		[Test]
		public void TestFrom_SplitsDelimitedStringIntoTradingPartners()
		{
			var input = "IN, D8, USA";
			var result = HsnTariffDTYRateTradingPartner.From(input);

			var expectedCodes = new[] { "IN", "D8", "USA" };

			Assert.That(result.Select(r => r.Code), Is.EqualTo(expectedCodes));
			Assert.That(result.ElementAt(0).Type, Is.EqualTo(HsnTariffDTYRateTradingPartner.HsnTariffDTYRateTradingPartnerType.Country));
			Assert.That(result.ElementAt(1).Type, Is.EqualTo(HsnTariffDTYRateTradingPartner.HsnTariffDTYRateTradingPartnerType.TradeGroup));
			Assert.That(result.ElementAt(2).Type, Is.EqualTo(HsnTariffDTYRateTradingPartner.HsnTariffDTYRateTradingPartnerType.TradeGroup));
		}

		[Test]
		public void TestEquals_ReturnsTrue_ForSameCodeAndType()
		{
			var p1 = new HsnTariffDTYRateTradingPartner("D8");
			var p2 = new HsnTariffDTYRateTradingPartner("D8");

			Assert.That(p1.Equals(p2), Is.True);
			Assert.That(p1.Equals((object)p2), Is.True);
			Assert.That(p1.GetHashCode(), Is.EqualTo(p2.GetHashCode()));
		}

		[Test]
		public void Equals_ReturnsFalse_WhenOtherIsNull()
		{
			var partner = new HsnTariffDTYRateTradingPartner("D8");
			Assert.That(partner.Equals(null), Is.False);
		}

		[Test]
		public void Equals_ReturnsTrue_WhenComparedWithItself()
		{
			var partner = new HsnTariffDTYRateTradingPartner("D8");
			Assert.That(partner.Equals(partner), Is.True);
		}

		[Test]
		public void Equals_ReturnsFalse_WhenOnlyTypeMatches()
		{
			var country1 = new HsnTariffDTYRateTradingPartner("IN");
			var country2 = new HsnTariffDTYRateTradingPartner("CN");
			Assert.That(country1.Equals(country2), Is.False);
		}

		[Test]
		public void EqualsObject_ReturnsFalse_WhenObjIsNull()
		{
			var partner = new HsnTariffDTYRateTradingPartner("D8");
			Assert.That(partner.Equals((object)null), Is.False);
		}

		[Test]
		public void EqualsObject_ReturnsTrue_WhenComparedWithItself()
		{
			var partner = new HsnTariffDTYRateTradingPartner("D8");
			Assert.That(partner.Equals((object)partner), Is.True);
		}

		[Test]
		public void EqualsObject_ReturnsFalse_WhenComparedWithDifferentType()
		{
			var partner = new HsnTariffDTYRateTradingPartner("D8");
			var notAPartner = "D8";
			Assert.That(partner.Equals(notAPartner), Is.False);
		}

		[Test]
		public void GetHashCode_ReturnsDifferentHash_ForDifferentObjects()
		{
			var partner1 = new HsnTariffDTYRateTradingPartner("IN");
			var partner2 = new HsnTariffDTYRateTradingPartner("D8");

			Assert.That(partner1.Equals(partner2), Is.False);
			Assert.That(partner1.GetHashCode(), Is.Not.EqualTo(partner2.GetHashCode()));
		}


		[Test]
		public void TestEquals_ReturnsFalse_ForDifferentCodesOrTypes()
		{
			var p1 = new HsnTariffDTYRateTradingPartner("D8");
			var p2 = new HsnTariffDTYRateTradingPartner("IN");

			Assert.That(p1.Equals(p2), Is.False);
			Assert.That(p1.Equals((object)p2), Is.False);
		}

		[TestCase("D8", "Code: D8, Type: TradeGroup")]
		[TestCase("IN", "Code: IN, Type: Country")]
		[TestCase("USA", "Code: USA, Type: TradeGroup")]
		public void ToString_ReturnsFormattedString(string inputCode, string expected)
		{
			var partner = new HsnTariffDTYRateTradingPartner(inputCode);
			var result = partner.ToString();

			Assert.That(result, Is.EqualTo(expected));
		}
	}
}

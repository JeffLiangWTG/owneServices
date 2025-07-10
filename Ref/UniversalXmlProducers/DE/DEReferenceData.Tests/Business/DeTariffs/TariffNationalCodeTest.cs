using System.Linq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.DeTariffs.Testing;

class TariffNationalCodeTest
{
	[Test]
	public void GetUpperLevelCodes()
	{
		var code = new TariffNationalCode()
		{
			FullTariffCode = "12345678912",
		};

		var upperLevelCodes = code.GetUpperLevelCodes();

		Assert.That(upperLevelCodes, Is.EqualTo(new [] { "12345678910", "12345678000", "12345600000", "12340000000", "12000000000" }));
	}

	[TestCase(11, 5)]
	[TestCase(10, 4)]
	[TestCase(8, 3)]
	[TestCase(7, 3)]
	[TestCase(2, 0)]
	public void GetUpperLevelCodesLevels(int filledInCode, int expectedUpperLevels)
	{
		var code = new TariffNationalCode()
		{
			FullTariffCode = new string('1', filledInCode).PadRight(11, '0'),
		};

		var upperLevelCodes = code.GetUpperLevelCodes();

		Assert.That(upperLevelCodes, Has.Exactly(expectedUpperLevels).Items);
	}
}


using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.AsycudaReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.AsycudaReferenceData.Tests.Services.Tariff;

[TestFixture]
sealed class NCTariffPDFParserTest
{
	[Test]
	public void TestNCTariffPDFParser()
	{
		Assert.Throws<ArgumentNullException>(() => NCTariffPDFParser.ParsePDF(null));

		var inputFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"AsycudaTestFiles\TariffData.pdf");
		var tariff = NCTariffPDFParser.ParsePDF(inputFilePath);

		Assert.That(tariff[0].StartDate, Is.EqualTo(new DateTime(2023, 01, 01)));
	}
}

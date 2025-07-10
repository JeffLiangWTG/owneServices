using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.TRReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests.Tariffs
{
	[TestFixture]
	public class HsnTariffBanDataParserTest
	{
		[Test]
		public void GetTariffs()
		{
			var tariffs = TariffHSNParser.GetTariffs();
			Assert.That(tariffs.Count, Is.EqualTo(TariffHSNParser.DeclarationTariffs.Length));

			var tariff = tariffs.First();
			Assert.That(tariff.ZZ1_ZZI_NKTariffType, Is.EqualTo("HSN"));
			var tariffAttributes = tariff.RefCusTariffAttributes;
			Assert.That(tariffAttributes.Count, Is.EqualTo(1));

			var tariffAttribute = tariffAttributes.First();
			Assert.That(tariffAttribute.ZZ3_Name, Is.EqualTo(Constants.TariffAttribute.Name.ISETRADEBANDEROL));
			Assert.That(tariffAttribute.ZZ3_Value, Is.EqualTo(Constants.TariffAttribute.Value.Y));

			var tariffWithMultipleRates = tariffs.FirstOrDefault(x => x.ZZ1_TariffCode == "852873000000").RefCusRates;
			Assert.NotNull(tariffWithMultipleRates);
			Assert.That(tariffWithMultipleRates.Count, Is.EqualTo(2));

			var rateBAN16 = tariffWithMultipleRates.FirstOrDefault(x => x.ZZ2_RateFormulaDerivedFrom == "16%");
			Assert.That(rateBAN16.ZZ2_RateFormula, Is.EqualTo("VFD *  0.16"));
			Assert.That(rateBAN16.ZZ2_ZY1_NKRateCode, Is.EqualTo("75"));
			Assert.That(rateBAN16.ZZ2_ZY1_ZZR_NKRateType, Is.EqualTo("BAN"));
			Assert.That(rateBAN16.ZZ2_StartDate, Is.EqualTo(Constants.HsnTariffStartDate));

			var app16 = rateBAN16.RefCusApplicabilities.FirstOrDefault();
			Assert.That(app16.ZZT_AdditionalCode, Is.EqualTo("BAN16"));
			Assert.That(app16.ZZT_StartDate, Is.EqualTo(Constants.HsnTariffStartDate));

			var rateBAN8 = tariffWithMultipleRates.FirstOrDefault(x => x.ZZ2_RateFormulaDerivedFrom == "8%");
			Assert.That(rateBAN8.ZZ2_RateFormula, Is.EqualTo("VFD *  0.08"));
			Assert.That(rateBAN8.ZZ2_ZY1_NKRateCode, Is.EqualTo("75"));
			Assert.That(rateBAN8.ZZ2_ZY1_ZZR_NKRateType, Is.EqualTo("BAN"));
			Assert.That(rateBAN8.ZZ2_StartDate, Is.EqualTo(Constants.HsnTariffStartDate));

			var app8 = rateBAN8.RefCusApplicabilities.FirstOrDefault();
			Assert.That(app8.ZZT_AdditionalCode, Is.EqualTo("BAN8"));
			Assert.That(app8.ZZT_StartDate, Is.EqualTo(Constants.HsnTariffStartDate));

			var tariffWithoutRefCusApplicabilities = tariffs.FirstOrDefault(x => x.ZZ1_TariffCode == "871610980000").RefCusRates;
			Assert.NotNull(tariffWithoutRefCusApplicabilities);
			Assert.That(tariffWithoutRefCusApplicabilities.Count, Is.EqualTo(1));

			var rate = tariffWithoutRefCusApplicabilities.FirstOrDefault();
			Assert.That(rate.ZZ2_RateFormulaDerivedFrom, Is.EqualTo("0.4%"));
			Assert.That(rate.ZZ2_RateFormula, Is.EqualTo("VFD *  0.004"));
			Assert.That(rate.ZZ2_ZY1_NKRateCode, Is.EqualTo("75"));
			Assert.That(rate.ZZ2_ZY1_ZZR_NKRateType, Is.EqualTo("BAN"));
			Assert.That(rate.ZZ2_StartDate, Is.EqualTo(Constants.HsnTariffStartDate));
			Assert.IsNull(rate.RefCusApplicabilities);
		}

		[Test]
		public void GetExistingTariff()
		{
			var tariffs = TariffHSNParser.GetTariffs();
			Assert.That(tariffs.Count, Is.EqualTo(TariffHSNParser.DeclarationTariffs.Length));

			var rate = tariffs.FirstOrDefault(x => x.ZZ1_TariffCode == "852873000000").RefCusRates;
			Assert.NotNull(rate);
			Assert.That(rate.Count, Is.EqualTo(2));
			Assert.That(rate.FirstOrDefault(x => x.ZZ2_RateFormulaDerivedFrom == "8%").RefCusApplicabilities.Length, Is.EqualTo(1));
			Assert.That(rate.FirstOrDefault(x => x.ZZ2_RateFormulaDerivedFrom == "16%").RefCusApplicabilities.Length, Is.EqualTo(1));
		}

		[SetUp]
		public void Setup()
		{
			TariffHSNParser = new HsnTariffBanDataParserForTest();
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(TempFolder);
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			if (Directory.Exists(TempFolder))
			{
				Directory.Delete(TempFolder, true);
			}
		}

		string TempFolder;
		HsnTariffBanDataParserForTest TariffHSNParser;
	}

	public class HsnTariffBanDataParserForTest : HsnTariffBanDataParser
	{
		public IEnumerable<RefCusTariff> GetTariffs() => PopulateTariffs(DeclarationTariffs);

		public RefCusTariff[] DeclarationTariffs => refCusTariffs ?? (refCusTariffs = new RefCusTariff[]
		{
			new RefCusTariff() { ZZ1_ZZI_NKTariffType = "HSN", ZZ1_TariffCode = "392210000011", ZZ1_Description = "Banyo küvetleri" },
			new RefCusTariff() { ZZ1_ZZI_NKTariffType = "HSN", ZZ1_TariffCode = "392210000012", ZZ1_Description = "Duş tekneleri" },
			new RefCusTariff() { ZZ1_ZZI_NKTariffType = "HSN", ZZ1_TariffCode = "691010000000", ZZ1_Description = "Porselenden olanlar" },
			new RefCusTariff() { ZZ1_ZZI_NKTariffType = "HSN", ZZ1_TariffCode = "691090000000", ZZ1_Description = "Diğerleri" },
			new RefCusTariff() { ZZ1_ZZI_NKTariffType = "HSN", ZZ1_TariffCode = "841810200000", ZZ1_Description = "Hacmi 340 litreyi geçenler" },
			new RefCusTariff() { ZZ1_ZZI_NKTariffType = "HSN", ZZ1_TariffCode = "841810800000", ZZ1_Description = "Diğerleri" },
			new RefCusTariff() { ZZ1_ZZI_NKTariffType = "HSN", ZZ1_TariffCode = "841821100000", ZZ1_Description = "Hacmi 340 litreyi geçenler" },
			new RefCusTariff() { ZZ1_ZZI_NKTariffType = "HSN", ZZ1_TariffCode = "841821510000", ZZ1_Description = "Masa modeli olanlar" },
			new RefCusTariff() { ZZ1_ZZI_NKTariffType = "HSN", ZZ1_TariffCode = "871610980000", ZZ1_Description = "Ağırlığı 1600 kg.ı geçenler" },
			new RefCusTariff() { ZZ1_ZZI_NKTariffType = "HSN", ZZ1_TariffCode = "852713000000", ZZ1_Description = "Ses kayıt veya kayıt edilen sesi tekrar vermeye mahsus cihaz ile birlikte olan diğer cihazlar" },
			new RefCusTariff() { ZZ1_ZZI_NKTariffType = "HSN", ZZ1_TariffCode = "852873000000", ZZ1_Description = "Diğerleri, siyah beyaz veya diğer tek renkli olanlar" },
			new RefCusTariff() { ZZ1_ZZI_NKTariffType = "HSN", ZZ1_TariffCode = "852872600000", ZZ1_Description = "Plazma gösterge panel (PDP) teknoloji ekranı olanlar" },
		});

		RefCusTariff[] refCusTariffs;
	}
}

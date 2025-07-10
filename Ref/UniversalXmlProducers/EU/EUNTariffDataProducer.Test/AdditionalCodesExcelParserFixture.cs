using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	[TestFixture]
	class AdditionalCodesExcelParserFixture
	{
		[Test]
		public void TestGetBoundaryDate()
		{
			Assert.That(AdditionalCodesExcelParser.GetBoundaryDate(string.Empty, true), Is.EqualTo(new DateTime(1900, 01, 01, 00, 00, 00)));
			Assert.That(AdditionalCodesExcelParser.GetBoundaryDate(string.Empty, false), Is.EqualTo(new DateTime(2079, 06, 06, 23, 59, 00)));
			Assert.That(AdditionalCodesExcelParser.GetBoundaryDate("18-12-2014", true), Is.EqualTo(new DateTime(2014, 12, 18, 00, 00, 00)));
			Assert.That(AdditionalCodesExcelParser.GetBoundaryDate("18-12-2014", false), Is.EqualTo(new DateTime(2014, 12, 18, 00, 00, 00)));
			Assert.That(AdditionalCodesExcelParser.GetBoundaryDate("xlsjdfg", true), Is.EqualTo(null));
			Assert.That(AdditionalCodesExcelParser.GetBoundaryDate("xlsjdfg", false), Is.EqualTo(null));
		}

		[Test]
		public void TestParseXLSX()
		{
			var multilingualDescriptions_Code2500 = new Dictionary<string, string>()
			{
				{ "BG",  "Приложения от 3 до 6, Част 3, Секция II (фармацевтични субстанции) Регламент 2022/1998 (OВ L 282)" },
				{ "CS",  "Přílohy 3-6, Část 3, Oddíl II (seznamy farmaceutických látek) R 2022/1998 (OJ L 282)" },
				{ "DA",  "Bilag I \"Den Kombinerede Nomenklatur\", Del III, Afsnit II (farmaceutiske stoffer), Toldbilag 3 til 6 -  R 2022/1998(EFT L 282)" },
				{ "DE",  "Anhang I \"Kombinierte Nomenklatur\", Teil III, Abschnitt II (pharmazeutische Stoffe), Anhänge 3 bis 6 zum Zolltarif - Verordnung 2022/1998 (ABl. L  282)" },
				{ "EL",  "Παραρτήματα 3 έως 6, Μέρος 3, Τμήμα ΙΙ (φαρμακευτικές ουσίες) Καν. 2022/1998 (ΕΕ L 282)" },
				{ "EN",  "Annex I \"Combined Nomenclature\", Part Three, Section II (pharmaceutical products), Tariff Annexes 3 to 6 – R 2022/1998 (OJ L 282)" },
				{ "ES",  "Anexo I \"Nomenclatura Combinada\", Tercera Parte, Sección II (sustancias farmaceúticas), Anexos Arancelario 3 a 6 -  R 2022/1998 (DO L 282)" },
				{ "ET",  "Määruse 2022/1998 (ELT L 282 3 lisad 3. kuni 6. osa II jagu (raviained)" },
				{ "FI",  "Liitteet 3-6, kolmas osa, II jakso (lääkeaineet) R 2022/1998 (EUVL L 282)" },
				{ "FR",  "Annexe I \"Nomenclature Combinée\", Troisième Partie, Section II (produits pharmaceutiques), Annexes tarifaires 3 à 6 -  R 2022/1998 (JO L 282)" },
				{ "HR",  "Prilog 3-6, Dio 3, Odsjek II (farmaceutske tvari) R 2022/1998 (OJ L 282)" },
				{ "HU",  "3.-6. mellékletek, 3. rész, II.(bekezdés) szekció (gyógyszerészeti anyagok) 2022/1998 rendelet (HL L 282)" },
				{ "IT",  "Allegato I \"Nomenclatura Combinata\", Parte terza, Sezione II (sostanze farmaceuticae), Allegati 3-6 - R 2022/1998 (GU L 282)" },
				{ "LT",  "Reglamento 2022/1998 I priedo 3 dalies II skyriaus (farmacinės medžiagos) 3-6 priedai (OJ L 282)" },
				{ "LV",  "I pielikums \"Kombinētā nomenklatūra\", trešā daļa, II sadaļa (farmaceitiskie produkti), Tarifa pielikumi no 3. līdz 6. – R 2022/1998 (OV L 282)" },
				{ "NL",  "Bijlage I \"Gecombineerde Nomenclatuur\", Derde Deel, Afdeling II (pharmaceutische stoffen), Bijlagen 3 tot 6 bij het tarief  - R 2022/1998 (PB L 282)" },
				{ "PL",  "Załączniki 3 do 6, Część 3, Sekcja II (wykaz substancji farmaceutycznych) R 2022/1998 (Dz.U. L 282)" },
				{ "PT",  "Anexos 3 a 6, Terceira Parte, Secção II (Substâncias frmacêuticas) Reg. 2022/1998 (JO L 282)" },
				{ "RO",  "Anexele 3-6, Partea a 3-a, Sectiunea II (substante farmaceutice) Regulamentul 2022/1998 (JO L 282)" },
				{ "SK",  "Prílohy 3-6, Časť 3, Trieda II (farmaceutické substancie) R 2022/1998 (OJ L 282)" },
				{ "SL",  "Priloga I \"Kombinirana Nomenklatura\", Tretji del, Oddelek II (farmacevtske snovi), Tarifne Priloge 3 do 6 - R 2022/1998 (UL L 282)" },
				{ "SV",  "Bilaga I \"Kombinerade Nomenklaturen\", Del 3, Avdelning II (farmaceutiska produkter), Taxebilagorna 3-6, R 2022/1998 (EUT L 282)" }
			};

			var xlsParserResult = AdditionalCodesExcelParser.ReadAdditionalCodesXlsxFileIntoResults(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", "AdditionalCodes.xlsx"));
			Assert.That(xlsParserResult.Count, Is.EqualTo(2523));
			var record = xlsParserResult.Single(x => x.Code == "2500");
			Assert.That(record.MultilingualDescriptions.Count, Is.EqualTo(22));

			foreach (var language in new string[] { "BG", "CS", "DA", "DE", "EL", "EN", "ES", "ET", "FI", "FR", "HR", "HU", "IT", "LT", "LV", "NL", "PL", "PT", "RO", "SK", "SL", "SV" })
			{
				Assert.That(record.MultilingualDescriptions.TryGetValue(language, out var actualDescription), Is.True);
				Assert.That(actualDescription, Is.EqualTo(multilingualDescriptions_Code2500[language]));
			}
		}
	}
}

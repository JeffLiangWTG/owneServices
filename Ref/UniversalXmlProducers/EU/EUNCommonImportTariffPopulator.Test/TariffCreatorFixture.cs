using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.SEReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator.Test
{
	[TestFixture]
	internal class TariffCreatorFixture
	{
		[TestCase(true, true, "I", "IMP")]
		[TestCase(false, false, "E", "EXP")]
		public void Create(bool startDateSpecific, bool endDateSpecific, string type, string expectedType)
		{
			var declarable = new declarableGoodsNomenclature
			{
				dateStart = new DateTime(2000, 01, 01),
				dateStartSpecified = startDateSpecific,
				dateEnd = new DateTime(2025, 01, 01),
				dateEndSpecified = endDateSpecific,
				goodsNomenclatureCode = "01010101",
				type = type
			};
			var goodNomenclature = new goodsNomenclature
			{
				goodsNomenclatureCode = "01010101",
				dateStart = new DateTime(2000, 01, 01),
				dateStartSpecified = startDateSpecific,
				dateEnd = new DateTime(2025, 01, 01),
				productLineSuffix = "80",
				goodsNomenclatureDescriptionPeriod = new goodsNomenclatureDescriptionPeriod[]
				{
					new goodsNomenclatureDescriptionPeriod
					{
						dateStart = new DateTime(2000, 01, 01),
						dateStartSpecified = startDateSpecific,
						goodsNomenclatureDescription = new goodsNomenclatureDescription[]
						{
							new goodsNomenclatureDescription
							{
								description = "Av Schwyz- eller Fribourgras",
								languageId = "SV",
								national = 0
							},
							new goodsNomenclatureDescription
							{
								description = "of the Schwyz and Fribourg breeds",
								languageId = "EN",
								national = 0
							}
						}
					}
				}
			};
			var result = new TariffCreator().Create(declarable, new goodsNomenclature[] { goodNomenclature });
			Assert.AreEqual(startDateSpecific ? new DateTime(2000, 01, 01) : new DateTime(1900, 01, 01), result.ZZ1_StartDate);
			Assert.AreEqual(endDateSpecific ? new DateTime(2025, 01, 01) : new DateTime(2079, 06, 06, 23, 59, 00), result.ZZ1_EndDate);
			Assert.AreEqual("01010101", result.ZZ1_TariffCode);
			Assert.AreEqual(expectedType, result.ZZ1_ZZI_NKTariffType);
			Assert.AreEqual("of the Schwyz and Fribourg breeds", result.ZZ1_Description);
		}

		[Test]
		public void CreateMultiplesWithRecordAndMeasures()
		{
			var record = new record
			{
				recordId = 1,
				recordIdSpecified = true,
				Item = new measure
				{
					goodsNomenclatureCode = "0101"
				}
			};
			var monthlyDeclarableGoodsNomenclature = new List<declarableGoodsNomenclature>
			{
				new declarableGoodsNomenclature
				{
					dateStart = new DateTime(2000, 01, 01),
					dateStartSpecified = true,
					dateEnd = new DateTime(2025, 01, 01),
					dateEndSpecified = true,
					goodsNomenclatureCode = "0101",
					type = "I"
				},
				new declarableGoodsNomenclature
				{
					dateStart = new DateTime(2001, 01, 01),
					dateStartSpecified = true,
					dateEnd = new DateTime(2021, 01, 01),
					dateEndSpecified = true,
					goodsNomenclatureCode = "01010020",
					type = "I"
				}
			};
			var goodNomenclature1 = new goodsNomenclature
			{
				goodsNomenclatureCode = "0101",
				dateStart = new DateTime(2000, 01, 01),
				dateStartSpecified = true,
				dateEnd = new DateTime(2025, 01, 01),
				productLineSuffix = "80",
				goodsNomenclatureDescriptionPeriod = new goodsNomenclatureDescriptionPeriod[]
				{
					new goodsNomenclatureDescriptionPeriod
					{
						dateStart = new DateTime(2000, 01, 01),
						dateStartSpecified = true,
						goodsNomenclatureDescription = new goodsNomenclatureDescription[]
						{
							new goodsNomenclatureDescription
							{
								description = "Av Schwyz- eller Fribourgras",
								languageId = "SV",
								national = 0
							},
							new goodsNomenclatureDescription
							{
								description = "of the Schwyz and Fribourg breeds",
								languageId = "EN",
								national = 0
							}
						}
					}
				}
			};
			var goodNomenclature2 = new goodsNomenclature
			{
				goodsNomenclatureCode = "01010020",
				dateStart = new DateTime(2000, 01, 01),
				dateStartSpecified = true,
				dateEnd = new DateTime(2025, 01, 01),
				productLineSuffix = "80",
				goodsNomenclatureDescriptionPeriod = new goodsNomenclatureDescriptionPeriod[]
				{
					new goodsNomenclatureDescriptionPeriod
					{
						dateStart = new DateTime(2000, 01, 01),
						dateStartSpecified = true,
						goodsNomenclatureDescription = new goodsNomenclatureDescription[]
						{
							new goodsNomenclatureDescription
							{
								description = "Andra",
								languageId = "SV",
								national = 0
							},
							new goodsNomenclatureDescription
							{
								description = "Other",
								languageId = "EN",
								national = 0
							}
						}
					}
				}
			};

			var results = new TariffCreator().Create(record.Item as measure, monthlyDeclarableGoodsNomenclature, new goodsNomenclature[] { goodNomenclature1, goodNomenclature2 });
			var result0101 = results.FirstOrDefault(x => x.ZZ1_TariffCode == "0101");
			Assert.IsNotNull(result0101);
			Assert.AreEqual(new DateTime(2000, 01, 01), result0101.ZZ1_StartDate);
			Assert.AreEqual(new DateTime(2025, 01, 01), result0101.ZZ1_EndDate);
			Assert.AreEqual("0101", result0101.ZZ1_TariffCode);
			Assert.AreEqual("IMP", result0101.ZZ1_ZZI_NKTariffType);
			Assert.AreEqual("of the Schwyz and Fribourg breeds", result0101.ZZ1_Description);

			var result01010020 = results.FirstOrDefault(x => x.ZZ1_TariffCode == "01010020");
			Assert.IsNotNull(result01010020);
			Assert.AreEqual(new DateTime(2001, 01, 01), result01010020.ZZ1_StartDate);
			Assert.AreEqual(new DateTime(2021, 01, 01), result01010020.ZZ1_EndDate);
			Assert.AreEqual("01010020", result01010020.ZZ1_TariffCode);
			Assert.AreEqual("IMP", result01010020.ZZ1_ZZI_NKTariffType);
			Assert.AreEqual("Other", result01010020.ZZ1_Description);
		}
	}
}

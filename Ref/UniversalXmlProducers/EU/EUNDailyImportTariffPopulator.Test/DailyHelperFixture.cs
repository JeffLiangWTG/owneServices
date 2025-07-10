using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.SEReferenceData.Services;
using CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNDailyImportTariffPopulator.Test
{
	[TestFixture]
	internal class DailyHelperFixture
	{
		[Test]
		public void GetTariffsFromTodaysMeasureLink()
		{
			var currentTariffList = new List<RefCusTariff>();

			var monthlyDeclarableCodes = new List<EUNCommonImportTariffPopulator.declarableGoodsNomenclature>
			{
				new EUNCommonImportTariffPopulator.declarableGoodsNomenclature
				{
					goodsNomenclatureCode = "01011010",
					dateStartSpecified = true,
					dateStart = new DateTime(2022, 10, 9),
					type = "I"
				},
				new EUNCommonImportTariffPopulator.declarableGoodsNomenclature
				{
					goodsNomenclatureCode = "02021010",
					dateStartSpecified = true,
					dateStart = new DateTime(2022, 10, 9),
					type = "I"
				}
			};
			var measureTypes = new List<EUNCommonImportTariffPopulator.measureType1>
			{
				new EUNCommonImportTariffPopulator.measureType1
				{
					measureTypeSeriesId = "C",
					measureType = "C"
				}
			};

			var recordsOnTodaysMeasureFile = new List<EUNCommonImportTariffPopulator.record> {
				new EUNCommonImportTariffPopulator.record
				{
					Item = new measure {
						goodsNomenclatureCode = "0101",
						dateStartSpecified = true,
						dateStart = new DateTime(2022, 10, 9),
						nationalSpecified = true,
						national = 0L,
						regulationId = "r1",
						measureType = "C",
						measureComponent = new measureComponent[]
						{
							new measureComponent
							{
								dutyAmount = 0,
								measurementUnitCode = "g",
								dutyExpressionId = "01",
								dutyAmountSpecified = true,
								national = 0L,
								nationalSpecified = true
							}
						}
					},
				},
				new EUNCommonImportTariffPopulator.record
				{
					Item = new measure {
						goodsNomenclatureCode = "0202",
						dateStartSpecified = true,
						dateStart = new DateTime(2022, 10, 9),
						nationalSpecified = true,
						national = 0L,
						regulationId = "r1",
						measureType = "C",
						measureComponent = new measureComponent[]
						{
							new measureComponent
							{
								dutyAmount = 0,
								measurementUnitCode = "g",
								dutyExpressionId = "01",
								dutyAmountSpecified = true,
								national = 0L,
								nationalSpecified = true
							}
						}
					},
				},
				new EUNCommonImportTariffPopulator.record
				{
					Item = new measure {
						goodsNomenclatureCode = "0303",
						dateStartSpecified = true,
						dateStart = new DateTime(2022, 10, 9),
						nationalSpecified = true,
						national = 0L,
						regulationId = "r1",
						measureType = "C",
						measureComponent = new measureComponent[]
						{
							new measureComponent
							{
								dutyAmount = 0,
								measurementUnitCode = "g",
								dutyExpressionId = "01",
								dutyAmountSpecified = true,
								national = 0L,
								nationalSpecified = true
							}
						}
					},
				},
			};

			var measureTradeLoader = new Mock<ITraderObjectLoader>();
			measureTradeLoader.Setup(x => x.Get(It.IsAny<IXmlFilter<EUNCommonImportTariffPopulator.record>>(), It.IsAny<DateTime>())).Returns(recordsOnTodaysMeasureFile);

			var measures = dailyHelper.GetMeasures(dailyConfigProvider.Object, measureTradeLoader.Object, new string[] { "r1" }, DateTime.UtcNow);
			var result = dailyHelper.GetTariffsFromMeasureLoaders(new[] { measureTradeLoader.Object },
				measureTypes, GoodsNomenclatures, monthlyDeclarableCodes, dailyConfigProvider.Object,
				new[] { "r1" }, new DateTime(2023, 05, 1), currentTariffList);

			Assert.IsNotEmpty(result);
			Assert.That(result.Count(), Is.EqualTo(2));
			Assert.That(result.First().ZZ1_TariffCode, Is.EqualTo("01011010"));
			Assert.That(result.First().ZZ1_Description, Is.EqualTo("of the Schwyz and Fribourg breeds"));
			Assert.That(result.Last().ZZ1_TariffCode, Is.EqualTo("02021010"));
			Assert.That(result.Last().ZZ1_Description, Is.EqualTo("Other"));
		}

		[Test]
		public void GetMeasures()
		{
			var recordsOnTodaysMeasureFile = new List<EUNCommonImportTariffPopulator.record> {
				new EUNCommonImportTariffPopulator.record
				{
					Item = new measure {
						goodsNomenclatureCode = "0101",
						dateStartSpecified = true,
						dateStart = new DateTime(2022, 10, 9),
						nationalSpecified = true,
						national = 0L,
						regulationId = "r1",
						measureType = "C",
						measureComponent = new measureComponent[]
						{
							new measureComponent
							{
								dutyAmount = 0,
								measurementUnitCode = "g",
								dutyExpressionId = "01",
								dutyAmountSpecified = true,
								national = 0L,
								nationalSpecified = true
							}
						}
					},
				},
				new EUNCommonImportTariffPopulator.record
				{
					Item = new measure {
						goodsNomenclatureCode = "0202",
						dateStartSpecified = true,
						dateStart = new DateTime(2022, 10, 9),
						nationalSpecified = true,
						national = 0L,
						regulationId = "r1",
						measureType = "C",
						measureComponent = new measureComponent[]
						{
							new measureComponent
							{
								dutyAmount = 0,
								measurementUnitCode = "g",
								dutyExpressionId = "01",
								dutyAmountSpecified = true,
								national = 0L,
								nationalSpecified = true
							}
						}
					},
				},
				new EUNCommonImportTariffPopulator.record
				{
					Item = new measure {
						goodsNomenclatureCode = "0303",
						dateStartSpecified = true,
						dateStart = new DateTime(2022, 10, 9),
						nationalSpecified = true,
						national = 0L,
						regulationId = "r1",
						measureType = "C",
						measureComponent = new measureComponent[]
						{
							new measureComponent
							{
								dutyAmount = 0,
								measurementUnitCode = "g",
								dutyExpressionId = "01",
								dutyAmountSpecified = true,
								national = 0L,
								nationalSpecified = true
							}
						}
					},
				},
				new EUNCommonImportTariffPopulator.record
				{
					Item = new measure {
						goodsNomenclatureCode = "0303Invalid",
						dateStartSpecified = true,
						dateStart = new DateTime(2022, 10, 9),
						nationalSpecified = true,
						national = 0L,
						regulationId = "r2",
						measureType = "C",
						measureComponent = new measureComponent[]
						{
							new measureComponent
							{
								dutyAmount = 0,
								measurementUnitCode = "g",
								dutyExpressionId = "01",
								dutyAmountSpecified = true,
								national = 0L,
								nationalSpecified = true
							}
						}
					},
				},
			};
			var measureTradeLoader = new Mock<ITraderObjectLoader>();
			measureTradeLoader.Setup(x => x.Get(It.IsAny<IXmlFilter<EUNCommonImportTariffPopulator.record>>(), It.IsAny<DateTime>())).Returns(recordsOnTodaysMeasureFile);
			var measures = dailyHelper.GetMeasures(dailyConfigProvider.Object, measureTradeLoader.Object, new string[] { "r1" }, DateTime.UtcNow);

			Assert.That(measures.Count(), Is.EqualTo(3));
			Assert.That(!measures.Any(x => x.regulationId == "r2"));
			Assert.That(measures.Any(x => x.goodsNomenclatureCode == "0101"));
			Assert.That(measures.Any(x => x.goodsNomenclatureCode == "0202"));
			Assert.That(measures.Any(x => x.goodsNomenclatureCode == "0303"));
		}

		[Test]
		public void GetRegulations()
		{
			var regulationsFromMonthly = new[] { "r1", "r2" };

			var measureLoader = new Mock<ITraderObjectLoader>();

			var regulationRecords = new List<EUNCommonImportTariffPopulator.record>
			{
				new EUNCommonImportTariffPopulator.record
				{
					Item = new baseRegulation
					{
						nationalSpecified = true,
						national = 0,
						regulationId = "r3"
					}
				},
				new EUNCommonImportTariffPopulator.record
				{
					Item = new baseRegulation
					{
						nationalSpecified = true,
						national = 0,
						regulationId = "r2"
					}
				},
				new EUNCommonImportTariffPopulator.record
				{
					Item = new modificationRegulation
					{
						nationalSpecified = true,
						national = 0,
						modificationRegulationId = "r4"
					}
				}
			};
			measureLoader.Setup(x => x.Get(It.IsAny<IXmlFilter<EUNCommonImportTariffPopulator.record>>(), It.IsAny<DateTime>())).Returns(regulationRecords);

			var result = dailyHelper.GetRegulations(new[] { measureLoader.Object }, regulationsFromMonthly, DateTime.UtcNow);
			Assert.That(result.Length, Is.EqualTo(4));
			Assert.That(result.Contains("r1"));
			Assert.That(result.Contains("r2"));
			Assert.That(result.Contains("r3"));
			Assert.That(result.Contains("r4"));
		}

		[Test]
		public void GetGoodsNomenclature()
		{
			var goodsNomenclatureFromMonthly = new[] {
				new goodsNomenclature{
					national = 0,
					nationalSpecified = true,
					goodsNomenclatureCode="0101",
					dateStart=DateTime.UtcNow
			} };

			var measureLoader = new Mock<ITraderObjectLoader>();
			var regulationRecords = new List<EUNCommonImportTariffPopulator.record>
			{
				new EUNCommonImportTariffPopulator.record
				{
					Item = new goodsNomenclature
					{
						nationalSpecified = true,
						national = 0,
						goodsNomenclatureCode="0102",
						dateStart=DateTime.UtcNow
					}
				},
				new EUNCommonImportTariffPopulator.record
				{
					Item = new goodsNomenclature
					{
						nationalSpecified = true,
						national = 0,
						goodsNomenclatureCode="0101",
						dateStart=DateTime.UtcNow
					}
				},
				new EUNCommonImportTariffPopulator.record
				{
					Item = new goodsNomenclature
					{
						nationalSpecified = true,
						national = 0,
						goodsNomenclatureCode="0103",
						dateStart=DateTime.UtcNow
					}
				}
			};
			measureLoader.Setup(x => x.Get(It.IsAny<IXmlFilter<EUNCommonImportTariffPopulator.record>>(), It.IsAny<DateTime>())).Returns(regulationRecords);

			var result = dailyHelper.GetGoodsNomenclatures(new[] { measureLoader.Object }, dailyConfigProvider.Object, goodsNomenclatureFromMonthly, DateTime.UtcNow);
			Assert.That(result.Count(), Is.EqualTo(3));
		}

		[Test]
		public void GetTariffsFromDeclarableGoodsNomenclatureLinks()
		{
			var currentTariffList = new List<RefCusTariff>();
			var tariffLinks = new List<string> { "link1", "link2", "link3", "link4" };

			var tariffsOnLink1 = new List<EUNCommonImportTariffPopulator.declarableGoodsNomenclature>
			{
				new EUNCommonImportTariffPopulator.declarableGoodsNomenclature
				{
					goodsNomenclatureCode = "01011010",
					dateStartSpecified = true,
					dateStart = new DateTime(2022, 10, 9),
					type = "I",
				}
			};
			var link1TradeLoader = new Mock<ITraderObjectLoader>();
			link1TradeLoader.Setup(x => x.Get(It.IsAny<IXmlFilter<EUNCommonImportTariffPopulator.declarableGoodsNomenclature>>(), It.IsAny<DateTime>())).Returns(tariffsOnLink1);

			var tariffsOnLink2 = new List<EUNCommonImportTariffPopulator.declarableGoodsNomenclature>
			{
				new EUNCommonImportTariffPopulator.declarableGoodsNomenclature
				{
					goodsNomenclatureCode = "02021010",
					dateStartSpecified = true,
					dateStart = new DateTime(2022,10,1),
					type = "I"
				}
			};
			var link2TradeLoader = new Mock<ITraderObjectLoader>();
			link2TradeLoader.Setup(x => x.Get(It.IsAny<IXmlFilter<EUNCommonImportTariffPopulator.declarableGoodsNomenclature>>(), It.IsAny<DateTime>())).Returns(tariffsOnLink2);

			var tariffsOnLink3 = new List<EUNCommonImportTariffPopulator.declarableGoodsNomenclature>
			{
				new EUNCommonImportTariffPopulator.declarableGoodsNomenclature
				{
					goodsNomenclatureCode = "02021010",
					dateStartSpecified = true,
					dateEndSpecified = true,
					dateStart = new DateTime(2022, 10, 5),
					dateEnd = new DateTime(2022, 10, 31),
					type = "I"
				}
			};
			var link3TradeLoader = new Mock<ITraderObjectLoader>();
			link3TradeLoader.Setup(x => x.Get(It.IsAny<IXmlFilter<EUNCommonImportTariffPopulator.declarableGoodsNomenclature>>(), It.IsAny<DateTime>())).Returns(tariffsOnLink3);

			var tariffOnTodaysLink = new List<EUNCommonImportTariffPopulator.declarableGoodsNomenclature>
			{
				new EUNCommonImportTariffPopulator.declarableGoodsNomenclature
				{
					goodsNomenclatureCode = "03031010",
					dateStartSpecified = true,
					dateStart = new DateTime(2022, 10, 10),
					type = "I"
				}
			};
			var todaysTradeLoader = new Mock<ITraderObjectLoader>();
			todaysTradeLoader.Setup(x => x.Get(It.IsAny<IXmlFilter<EUNCommonImportTariffPopulator.declarableGoodsNomenclature>>(), It.IsAny<DateTime>())).Returns(tariffOnTodaysLink);

			declarableLoader.Setup(x => x.GetTraderObjectLoader("link1", tmpPath)).Returns(link1TradeLoader.Object);
			declarableLoader.Setup(x => x.GetTraderObjectLoader("link2", tmpPath)).Returns(link2TradeLoader.Object);
			declarableLoader.Setup(x => x.GetTraderObjectLoader("link3", tmpPath)).Returns(link3TradeLoader.Object);
			declarableLoader.Setup(x => x.GetTraderObjectLoader("link4", tmpPath)).Returns(todaysTradeLoader.Object);

			var tradeLoaders = new[] { link1TradeLoader.Object, link2TradeLoader.Object, link3TradeLoader.Object, todaysTradeLoader.Object };

			var result = dailyHelper.GetTariffsFromDeclarableGoodsNomenclatureLinks(tariffLinks, GoodsNomenclatures, declarableLoader.Object, dailyConfigProvider.Object, new DateTime(2022, 10, 10), tmpPath, currentTariffList).ToArray();
			Assert.IsNotEmpty(result);
			Assert.That(result.Length, Is.EqualTo(3));

			Assert.AreEqual("01011010", result[0].ZZ1_TariffCode);
			Assert.AreEqual("02021010", result[1].ZZ1_TariffCode);
			Assert.AreEqual("03031010", result[2].ZZ1_TariffCode);
		}

		Mock<IDailyConfigProvider> dailyConfigProvider;
		Mock<IDeclarableLoader> declarableLoader;
		DailyHelper dailyHelper;
		string tmpPath;

		public static goodsNomenclature[] GoodsNomenclatures => new goodsNomenclature[] {
			new goodsNomenclature
			{
				goodsNomenclatureCode = "01011010",
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
			},
			new goodsNomenclature
			{
				goodsNomenclatureCode = "02021010",
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
			},
			new goodsNomenclature
			{
				goodsNomenclatureCode = "03031010",
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
								description = "Other",
								languageId = "EN",
								national = 0
							}
						}
					}
				}
			}
		};

		[SetUp]
		public void SetUp()
		{
			dailyConfigProvider = new Mock<IDailyConfigProvider>();
			declarableLoader = new Mock<IDeclarableLoader>();
			dailyHelper = new DailyHelper();

			tmpPath = Path.GetTempPath();
			if (!Directory.Exists(tmpPath))
			{
				Directory.CreateDirectory(tmpPath);
			}
		}
	}
}

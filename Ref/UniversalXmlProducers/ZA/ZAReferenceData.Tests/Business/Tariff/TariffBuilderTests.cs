using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.ZAReferenceData.Business.Tariff;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Configuration;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Helpers;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Loader;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Models;
using CargoWise.RefDbRepo.ZAReferenceData.Tests.Helpers;
using CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Common;
using CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Tariff.TestClasses;
using NUnit.Framework;
using static CargoWise.RefDbRepo.ZAReferenceData.Services.Common.Constants;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.Business.Tariff
{
	[TestFixture]
	public class TariffBuilderTests
	{
		[Test]
		public void FilePrefix()
		{
			var builder = new TariffBuilderForTest(null, null);
			Assert.That(builder.FilePrefix, Is.EqualTo("ZA_RefCusTariff"));
		}

		[Test]
		public void XmlWriterConfig_Original()
		{
			var header = new Header { TransactionType = TransactionType.Original };

			var builder = new TariffBuilderForTest(header, logger);
			var config = builder.GetXmlWriterConfiguration();

			Assert.That(config, Is.Not.Null);
			var refType = typeof(RefCusTariff);
			var tariffConfig = config.GetConfiguration(refType);

			Assert.That(tariffConfig, Is.Not.Null);
			Assert.That(tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_TariffCode))));
			Assert.That(tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_ZZI_NKTariffType))));
			Assert.That(tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_ZZI_ZZZ_NKDataGrouping))));
			Assert.That(tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_ZZZ_NKDataGrouping))));
			Assert.That(tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_Description))));
			Assert.That(tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_StartDate))));
			Assert.That(tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_EndDate))));
			Assert.That(tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_IAMUnique))));
			Assert.That(tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_ZZF_NKTaxOrFeeCode))));
			Assert.That(tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.RefCusRates))));
			Assert.That(tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.RefCusTariffUOMs))));
			Assert.That(tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.RefCusTariffAttributes))));
			Assert.That(tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.RefCusTariffRelationships))));

			XmlWriterConfigTestHelper.AssertKeySets(nameof(RefCusTariff), tariffConfig.GetKeySets(),
				nameof(RefCusTariff.ZZ1_IAMUnique),
				nameof(RefCusTariff.ZZ1_TariffCode),
				nameof(RefCusTariff.ZZ1_ZZI_NKTariffType),
				nameof(RefCusTariff.ZZ1_ZZI_ZZZ_NKDataGrouping),
				nameof(RefCusTariff.ZZ1_ZZZ_NKDataGrouping));

			refType = typeof(RefCusTariffAttribute);
			var attribConfig = config.GetConfiguration(refType);

			Assert.That(attribConfig, Is.Not.Null);
			Assert.That(attribConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariffAttribute.ZZ3_Name))));
			Assert.That(attribConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariffAttribute.ZZ3_Value))));

			XmlWriterConfigTestHelper.AssertKeySets(nameof(RefCusTariffAttribute), attribConfig.GetKeySets(),
				nameof(RefCusTariffAttribute.ZZ3_Name));

			refType = typeof(RefCusTariffUOM);
			var uomConfig = config.GetConfiguration(refType);

			Assert.That(uomConfig, Is.Not.Null);
			Assert.That(uomConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariffUOM.ZZ8_Type))));
			Assert.That(uomConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariffUOM.ZZ8_ZZZ_NKDataGrouping))));
			Assert.That(uomConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariffUOM.ZZ8_UOM))));

			XmlWriterConfigTestHelper.AssertKeySets(nameof(RefCusTariffUOM), uomConfig.GetKeySets(),
				nameof(RefCusTariffUOM.ZZ8_Type),
				nameof(RefCusTariffUOM.ZZ8_ZZZ_NKDataGrouping));

			refType = typeof(RefCusRate);
			var rateConfig = config.GetConfiguration(refType);

			Assert.That(rateConfig, Is.Not.Null);
			Assert.That(rateConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.ZZ2_ZY1_NKRateCode))));
			Assert.That(rateConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.ZZ2_ZY1_ZZR_NKRateType))));
			Assert.That(rateConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping))));
			Assert.That(rateConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.ZZ2_ZZS_NKPreference))));
			Assert.That(rateConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.ZZ2_ZZS_ZZZ_NKDataGrouping))));
			Assert.That(rateConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.ZZ2_ZZZ_NKDataGrouping))));
			Assert.That(rateConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.RefCusApplicabilities))));
			Assert.That(rateConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.ZZ2_StartDate))));
			Assert.That(rateConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.ZZ2_EndDate))));
			Assert.That(rateConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.ZZ2_RateFormula))));
			Assert.That(rateConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.ZZ2_RateFormulaDerivedFrom))));

			XmlWriterConfigTestHelper.AssertKeySets(nameof(RefCusRate), rateConfig.GetKeySets(),
				nameof(RefCusRate.ZZ2_ZY1_NKRateCode),
				nameof(RefCusRate.ZZ2_ZY1_ZZR_NKRateType),
				nameof(RefCusRate.ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping),
				nameof(RefCusRate.ZZ2_ZZZ_NKDataGrouping),
				nameof(RefCusRate.RefCusApplicabilities));

			refType = typeof(RefCusApplicability);
			var appConfig = config.GetConfiguration(refType);

			Assert.That(appConfig, Is.Not.Null);
			Assert.That(appConfig.IsIncluded(refType.GetProperty(nameof(RefCusApplicability.ZZT_ZZA_NKTradeGroup))));
			Assert.That(appConfig.IsIncluded(refType.GetProperty(nameof(RefCusApplicability.ZZT_ZZA_ZZZ_NKDataGrouping))));
			Assert.That(appConfig.IsIncluded(refType.GetProperty(nameof(RefCusApplicability.ZZT_StartDate))));
			Assert.That(appConfig.IsIncluded(refType.GetProperty(nameof(RefCusApplicability.ZZT_EndDate))));

			XmlWriterConfigTestHelper.AssertKeySets(nameof(RefCusApplicability), appConfig.GetKeySets(),
				nameof(RefCusApplicability.ZZT_ZZA_ZZZ_NKDataGrouping),
				nameof(RefCusApplicability.ZZT_ZZA_NKTradeGroup));

			refType = typeof(RefCusRateUOM);
			var rUOMConfig = config.GetConfiguration(refType);

			Assert.That(rUOMConfig.IsIncluded(refType.GetProperty(nameof(RefCusRateUOM.ZXG_UOM))));

			XmlWriterConfigTestHelper.AssertKeySets(nameof(RefCusRateUOM), rUOMConfig.GetKeySets(),
				nameof(RefCusRateUOM.ZXG_UOM));

			refType = typeof(RefCusTariffRelationship);
			var relConfig = config.GetConfiguration(refType);

			Assert.That(relConfig, Is.Not.Null);
			Assert.That(relConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariffRelationship.ZZH_ZZI_NKTariffType))));
			Assert.That(relConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariffRelationship.ZZH_TariffCode))));
			Assert.That(relConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariffRelationship.ZZH_ZZI_ZZZ_NKDataGrouping))));

			XmlWriterConfigTestHelper.AssertKeySets(nameof(RefCusTariffRelationship), relConfig.GetKeySets(),
				nameof(RefCusTariffRelationship.ZZH_ZZI_NKTariffType),
				nameof(RefCusTariffRelationship.ZZH_TariffCode),
				nameof(RefCusTariffRelationship.ZZH_ZZI_ZZZ_NKDataGrouping));
		}

		[Test]
		public void XmlWriterConfig_Delete()
		{
			var header = new Header { TransactionType = TransactionType.Deletion };

			var builder = new TariffBuilderForTest(header, logger);
			var config = builder.GetXmlWriterConfiguration();

			Assert.That(config, Is.Not.Null);
			var refType = typeof(RefCusTariff);
			var tariffConfig = config.GetConfiguration(refType);

			Assert.That(tariffConfig, Is.Not.Null);
			Assert.That(tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_TariffCode))));
			Assert.That(tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_ZZI_NKTariffType))));
			Assert.That(tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_ZZI_ZZZ_NKDataGrouping))));
			Assert.That(tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_ZZZ_NKDataGrouping))));
			Assert.That(tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_Description))));
			Assert.That(tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_StartDate))));
			Assert.That(tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_EndDate))));
			Assert.That(tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_IAMUnique))));
			Assert.That(!tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.RefCusRates))));
			Assert.That(!tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.RefCusTariffUOMs))));
			Assert.That(tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.RefCusTariffAttributes))));
			Assert.That(!tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.RefCusTariffRelationships))));

			XmlWriterConfigTestHelper.AssertKeySets(nameof(RefCusTariff), tariffConfig.GetKeySets(),
				nameof(RefCusTariff.ZZ1_IAMUnique),
				nameof(RefCusTariff.ZZ1_TariffCode),
				nameof(RefCusTariff.ZZ1_ZZI_NKTariffType),
				nameof(RefCusTariff.ZZ1_ZZI_ZZZ_NKDataGrouping),
				nameof(RefCusTariff.ZZ1_ZZZ_NKDataGrouping));

			refType = typeof(RefCusTariffAttribute);
			var attribConfig = config.GetConfiguration(refType);

			Assert.That(attribConfig, Is.Not.Null);
			Assert.That(attribConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariffAttribute.ZZ3_Name))));
			Assert.That(attribConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariffAttribute.ZZ3_Value))));

			XmlWriterConfigTestHelper.AssertKeySets(nameof(RefCusTariffAttribute), attribConfig.GetKeySets(),
				nameof(RefCusTariffAttribute.ZZ3_Name));

			Assert.Null(config.GetConfiguration(typeof(RefCusTariffUOM)));
			Assert.Null(config.GetConfiguration(typeof(RefCusRate)));
			Assert.Null(config.GetConfiguration(typeof(RefCusApplicability)));
			Assert.Null(config.GetConfiguration(typeof(RefCusTariffRelationship)));
		}

		[Test]
		public void ConvertModels()
		{
			var header = new Header
			{
				GovernmentGazettePublicationNumber = "123",
				PublicationDate = new DateTime(2022, 3, 16, 13, 14, 15),
				TransactionType = TransactionType.Original,
				Tariffs = new List<TariffData>
				{
					new TariffData
					{
						CheckDigit = "1",
						Code = "",
						Description = "Tariff Data 1",
						GovernmentGazetteNoticeNumber = "345",
						Heading = "12.34",
						SubHeading = "12.34.56",
						ItemNumber = "",
						LineNumber = "27",
						ScheduleTypeCode = "1P1",
						ImportedFrom = "GERMANY",
						StatisticalUnitConverted = "KG",
						StartDate = new DateTime(2022, 1, 1),
						EndDate = new DateTime(2025, 12, 31, 23, 59, 59),

						ImportCountries = new List<string> { "DE" },
						Schedule = SARSSchedule.Get("1P1", "103"),
						TariffCode = "123456",
						RelationshipTariffCode = "123456",

						Rates = new List<Rate>
						{
							new Rate
							{
								RateType = RateTypes.Standard,
								Countries = "Argentina, Brazil",
								CountryCodes = new List<string> { "AR", "BR" },
								FormulaCode = "1312",
								RateQualifier = "A01",
								Description = "Standard Rate 1",
								Preference = "100",
								Formula = "1.234 * VFD",
								UnitOfMeasureConverted = "LI"
							}
						}
					},

					new TariffData
					{
						CheckDigit = "2",
						Code = "12.34.00",
						Description = "Tariff Data 2",
						GovernmentGazetteNoticeNumber = "345",
						Heading = "12.34",
						SubHeading = "12.34.56",
						ItemNumber = "103.56",
						LineNumber = "27",
						ScheduleTypeCode = "2P1",
						ImportedFrom = "GERMANY",
						StatisticalUnitConverted = "KG",
						StartDate = new DateTime(2022, 1, 1),
						EndDate = new DateTime(2025, 12, 31, 23, 59, 59),

						ImportCountries = new List<string> { "DE" },
						Schedule = SARSSchedule.Get("2P1", "103"),
						TariffCode = "103123400",
						RelationshipTariffCode = "123456",

						Rates = new List<Rate>
						{
							new Rate
							{
								RateType = RateTypes.Standard,
								Countries = "Argentina, Brazil",
								FormulaCode = "1312",
								RateQualifier = "A01",
								Description = "Standard Rate 1",
								Preference = "100",
								Formula = "1.234 * VFD",
								UnitOfMeasureConverted = "KG"
							}
						}
					},

					new TariffData
					{
						CheckDigit = "1",
						Code = "",
						Description = "Tariff Data 3",
						GovernmentGazetteNoticeNumber = "345",
						Heading = "12.34",
						SubHeading = "12.34.56",
						ItemNumber = "",
						LineNumber = "27",
						ScheduleTypeCode = "12A",
						ImportedFrom = "GERMANY",
						StatisticalUnitConverted = "KG",
						StartDate = new DateTime(2022, 1, 1),
						EndDate = new DateTime(2025, 12, 31, 23, 59, 59),

						ImportCountries = new List<string> { "DE" },
						Schedule = SARSSchedule.Get("12A", ""),
						TariffCode = "333222",
						RelationshipTariffCode = "123456",

						Rates = new List<Rate>
						{
							new Rate
							{
								RateType = RateTypes.Standard,
								Countries = "",
								CountryCodes = new List<string>(),
								FormulaCode = "1312",
								RateQualifier = "A01",
								Description = "Standard Rate 1",
								Preference = "100",
								Formula = "1.234 * VFD",
								UnitOfMeasureConverted = "LI"
							}
						}
					}
				}
			};

			var logger = new TestLogger();
			var builder = new TariffBuilderForTest(header, logger);
			var refModels = builder.ConvertToRefModels();

			Assert.That(refModels, Is.Not.Null);
			Assert.That(refModels.Count, Is.EqualTo(3));

			var tariff1 = refModels[0];
			Assert.That(tariff1.ZZ1_ZZI_NKTariffType, Is.EqualTo("1P1"));
			Assert.That(tariff1.ZZ1_TariffCode, Is.EqualTo("123456"));
			Assert.That(tariff1.ZZ1_Description, Is.EqualTo("Tariff Data 1"));
			Assert.That(tariff1.ZZ1_StartDate, Is.EqualTo(new DateTime(2022, 1, 1)));
			Assert.That(tariff1.ZZ1_EndDate, Is.EqualTo(new DateTime(2025, 12, 31, 23, 59, 0)));
			Assert.That(tariff1.ZZ1_IAMUnique, Is.EqualTo(0));

			Assert.That(tariff1.RefCusTariffAttributes, Is.Not.Null);
			Assert.That(tariff1.RefCusTariffAttributes.Count, Is.EqualTo(1));
			Assert.That(tariff1.RefCusTariffAttributes[0].ZZ3_Name, Is.EqualTo("CheckDigit"));
			Assert.That(tariff1.RefCusTariffAttributes[0].ZZ3_Value, Is.EqualTo("1"));

			Assert.That(tariff1.RefCusTariffUOMs, Is.Not.Null);
			Assert.That(tariff1.RefCusTariffUOMs.Count, Is.EqualTo(2));
			Assert.That(tariff1.RefCusTariffUOMs[0].ZZ8_Type, Is.EqualTo("CU1"));
			Assert.That(tariff1.RefCusTariffUOMs[0].ZZ8_UOM, Is.EqualTo("KG"));
			Assert.That(tariff1.RefCusTariffUOMs[1].ZZ8_Type, Is.EqualTo("CU2"));
			Assert.That(tariff1.RefCusTariffUOMs[1].ZZ8_UOM, Is.EqualTo("LI"));

			Assert.That(tariff1.RefCusRates, Is.Not.Null);
			Assert.That(tariff1.RefCusRates.Count, Is.EqualTo(1));
			Assert.That(tariff1.RefCusRates[0].ZZ2_StartDate, Is.EqualTo(new DateTime(2022, 1, 1)));
			Assert.That(tariff1.RefCusRates[0].ZZ2_EndDate, Is.EqualTo(new DateTime(2025, 12, 31, 23, 59, 0)));
			Assert.That(tariff1.RefCusRates[0].ZZ2_RateFormula, Is.EqualTo("1.234 * VFD"));
			Assert.That(tariff1.RefCusRates[0].ZZ2_RateFormulaDerivedFrom, Is.EqualTo("Standard Rate 1"));
			Assert.That(tariff1.RefCusRates[0].ZZ2_ZY1_NKRateCode, Is.EqualTo("1P1"));
			Assert.That(tariff1.RefCusRates[0].ZZ2_ZZS_NKPreference, Is.EqualTo("100"));
			Assert.That(tariff1.RefCusRates[0].ZZ2_ZY1_ZZR_NKRateType, Is.EqualTo("DTY"));

			Assert.That(tariff1.RefCusRates[0].RefCusRateUOMs, Is.Null);

			Assert.That(tariff1.RefCusRates[0].RefCusApplicabilities, Is.Not.Null);
			Assert.That(tariff1.RefCusRates[0].RefCusApplicabilities.Count, Is.EqualTo(2));
			Assert.That(tariff1.RefCusRates[0].RefCusApplicabilities[0].ZZT_StartDate, Is.EqualTo(new DateTime(2022, 1, 1)));
			Assert.That(tariff1.RefCusRates[0].RefCusApplicabilities[0].ZZT_EndDate, Is.EqualTo(new DateTime(2025, 12, 31, 23, 59, 0)));
			Assert.That(tariff1.RefCusRates[0].RefCusApplicabilities[0].ZZT_ZZA_NKTradeGroup, Is.EqualTo("AR"));
			Assert.That(tariff1.RefCusRates[0].RefCusApplicabilities[1].ZZT_ZZA_NKTradeGroup, Is.EqualTo("BR"));

			Assert.That(tariff1.RefCusTariffRelationships, Is.Null);

			var tariff2 = refModels[1];
			Assert.That(tariff2.ZZ1_TariffCode, Is.EqualTo("333222"));
			Assert.That(tariff2.RefCusRates, Is.Not.Null);
			Assert.That(tariff2.RefCusRates.Count, Is.EqualTo(1));
			Assert.That(tariff2.RefCusRates[0].ZZ2_ZY1_NKRateCode, Is.EqualTo("12A"));
			Assert.That(tariff2.RefCusRates[0].ZZ2_ZY1_ZZR_NKRateType, Is.EqualTo("EXC"));

			var tariff3 = refModels[2];
			Assert.That(tariff3.ZZ1_TariffCode, Is.EqualTo("103123400"));
			Assert.That(tariff3.RefCusTariffRelationships, Is.Not.Null);
			Assert.That(tariff3.RefCusTariffRelationships.Count, Is.EqualTo(1));
			Assert.That(tariff3.RefCusTariffRelationships[0].ZZH_TariffCode, Is.EqualTo("123456"));
			Assert.That(tariff3.RefCusTariffRelationships[0].ZZH_ZZI_NKTariffType, Is.EqualTo("1P1"));
		}

		[Test]
		public void SetIAMUnique()
		{
			var rates = new List<Rate> { new Rate { RateType = RateTypes.Standard, Formula = "123" } };
			var sched = SARSSchedule.Get("1P1", "");
			var header = new Header
			{
				Tariffs = new List<TariffData>
				{
					new TariffData { TariffCode = "111111", CheckDigit = "1", Description = "T1", Schedule = sched, Heading = "11", SubHeading = "111111", Rates = rates, UniqueId = 0 },
					new TariffData { TariffCode = "111111", CheckDigit = "2", Description = "T2", Schedule = sched, Heading = "11", SubHeading = "111111", Rates = rates, UniqueId = 2 },
					new TariffData { TariffCode = "111111", CheckDigit = "3", Description = "T3", Schedule = sched, Heading = "11", SubHeading = "111111", Rates = rates, UniqueId = 0 },
					new TariffData { TariffCode = "222222", CheckDigit = "5", Description = "T4", Schedule = sched, Heading = "22", SubHeading = "222222", Rates = rates, UniqueId = 9, TariffKeyExists = true },
					new TariffData { TariffCode = "111111", CheckDigit = "4", Description = "T5", Schedule = sched, Heading = "11", SubHeading = "111111", Rates = rates, UniqueId = 1, TariffKeyExists = true },
					new TariffData { TariffCode = "222222", CheckDigit = "6", Description = "T6", Schedule = sched, Heading = "22", SubHeading = "222222", Rates = rates, UniqueId = 0 },
				}
			};

			var logger = new TestLogger();
			var builder = new TariffBuilderForTest(header, logger);
			var results = builder.ConvertToRefModels();

			Assert.That(results, Is.Not.Null);
			Assert.That(results.Count, Is.EqualTo(6));

			Assert.That(results[0].ZZ1_Description, Is.EqualTo("T1"));
			Assert.That(results[0].ZZ1_IAMUnique, Is.EqualTo(3));

			Assert.That(results[1].ZZ1_Description, Is.EqualTo("T2"));
			Assert.That(results[1].ZZ1_IAMUnique, Is.EqualTo(4));

			Assert.That(results[2].ZZ1_Description, Is.EqualTo("T3"));
			Assert.That(results[2].ZZ1_IAMUnique, Is.EqualTo(5));

			Assert.That(results[3].ZZ1_Description, Is.EqualTo("T5"));
			Assert.That(results[3].ZZ1_IAMUnique, Is.EqualTo(1));

			Assert.That(results[4].ZZ1_Description, Is.EqualTo("T4"));
			Assert.That(results[4].ZZ1_IAMUnique, Is.EqualTo(9));

			Assert.That(results[5].ZZ1_Description, Is.EqualTo("T6"));
			Assert.That(results[5].ZZ1_IAMUnique, Is.EqualTo(10));
		}

		[Test]
		public void GenerateFile_Original()
		{
			var header = new Header
			{
				GovernmentGazettePublicationNumber = "123",
				PublicationDate = new DateTime(2022, 3, 16, 13, 14, 15),
				TransactionType = TransactionType.Original,
				Tariffs = new List<TariffData>
				{
					new TariffData
					{
						CheckDigit = "1",
						Code = "123",
						Description = "Tariff Data 1",
						GovernmentGazetteNoticeNumber = "345",
						Heading = "12.34",
						SubHeading = "12.34.56",
						ItemNumber = "",
						LineNumber = "27",
						ScheduleTypeCode = "1P1",
						ImportedFrom = "GERMANY",
						StatisticalUnitConverted = "KG",
						StartDate = new DateTime(2022, 1, 1),
						EndDate = new DateTime(2025, 12, 31, 23, 59, 59),

						ImportCountries = new List<string> { "DE" },
						Schedule = SARSSchedule.Get("1P1", "103"),
						TariffCode = "123456",
						RelationshipTariffCode = "123456",

						Rates = new List<Rate>
						{
							new Rate
							{
								RateType = RateTypes.Standard,
								Countries = "Argentina, Brazil",
								CountryCodes = new List<string> { "AR", "BR" },
								FormulaCode = "1312",
								RateQualifier = "A01",
								Description = "Standard Rate 1",
								Preference = "100",
								Formula = "1.234 * VFD",
								UnitOfMeasureConverted = "LI"
							}
						},
						AdditionalUOMs = new Dictionary<string, string> { { "RU1", "LI" } },
						AdditionalAttributes = new Dictionary<string, string> { { "AT1", "Attribute1" } }
					},
					new TariffData
					{
						CheckDigit = "2",
						Code = "12.34.00",
						Description = "Tariff Data 2",
						GovernmentGazetteNoticeNumber = "345",
						Heading = "12.34",
						SubHeading = "12.34.56",
						ItemNumber = "103.45",
						LineNumber = "27",
						ScheduleTypeCode = "2P1",
						ImportedFrom = "GERMANY",
						StatisticalUnitConverted = "KG",
						StartDate = new DateTime(2022, 1, 1),
						EndDate = new DateTime(2025, 12, 31, 23, 59, 59),

						ImportCountries = new List<string> { "DE" },
						Schedule = SARSSchedule.Get("2P1", "103"),
						TariffCode = "103123400",
						RelationshipTariffCode = "123456",

						Rates = new List<Rate>
						{
							new Rate
							{
								RateType = RateTypes.Standard,
								Countries = "Argentina, Brazil",
								FormulaCode = "1312",
								RateQualifier = "A01",
								Description = "Standard Rate 1",
								Preference = "100",
								Formula = "1.234 * VFD",
								UnitOfMeasureConverted = "KG"
							}
						}
					}
				}
			};

			var builder = new TariffBuilderForTest(header, logger);
			builder.CreateXmlFile(TempFolder, new DateTime(2022, 5, 6, 13, 14, 15));

			var fileName = Path.Combine(TempFolder, builder.OutputFilename);
			Assert.That(File.Exists(fileName));

			var xml = File.ReadAllText(fileName);

			var expectedXml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.ZAReferenceData.Tests.Business.Tariff.TestFiles.Output.ZATariff_001.xml");

			Assert.That(xml, Is.EqualTo(expectedXml));

			Assert.That(logger.InfoString, Contains.Substring($"Created {fileName}"));
		}

		[Test]
		public void GenerateFile_Delete()
		{
			var header = new Header
			{
				GovernmentGazettePublicationNumber = "123",
				PublicationDate = new DateTime(2022, 3, 16, 13, 14, 15),
				TransactionType = TransactionType.Deletion,
				Tariffs = new List<TariffData>
				{
					new TariffData
					{
						CheckDigit = "1",
						Code = "123",
						Description = "Tariff Data 1",
						GovernmentGazetteNoticeNumber = "345",
						Heading = "12.34",
						SubHeading = "12.34.56",
						ItemNumber = "",
						LineNumber = "27",
						ScheduleTypeCode = "1P1",
						ImportedFrom = "GERMANY",
						StatisticalUnitConverted = "KG",
						StartDate = new DateTime(2022, 1, 1),
						EndDate = new DateTime(2025, 12, 31, 23, 59, 59),

						ImportCountries = new List<string> { "DE" },
						Schedule = SARSSchedule.Get("1P1", "103"),
						TariffCode = "123456",
						RelationshipTariffCode = "123456",

						Rates = new List<Rate>
						{
							new Rate
							{
								RateType = RateTypes.Standard,
								Countries = "Argentina, Brazil",
								CountryCodes = new List<string> { "AR", "BR" },
								FormulaCode = "1312",
								RateQualifier = "A01",
								Description = "Standard Rate 1",
								Preference = "100",
								Formula = "1.234 * VFD",
								UnitOfMeasureConverted = "KG"
							}
						}
					},
					new TariffData
					{
						CheckDigit = "2",
						Code = "12.34.00",
						Description = "Tariff Data 2",
						GovernmentGazetteNoticeNumber = "345",
						Heading = "12.34",
						SubHeading = "12.34.56",
						ItemNumber = "103.56",
						LineNumber = "27",
						ScheduleTypeCode = "2P1",
						ImportedFrom = "GERMANY",
						StatisticalUnitConverted = "KG",
						StartDate = new DateTime(2022, 1, 1),
						EndDate = new DateTime(2025, 12, 31, 23, 59, 59),

						ImportCountries = new List<string> { "DE" },
						Schedule = SARSSchedule.Get("2P1", "103"),
						TariffCode = "103123400",
						RelationshipTariffCode = "123456",

						Rates = new List<Rate>
						{
							new Rate
							{
								RateType = RateTypes.Standard,
								Countries = "Argentina, Brazil",
								FormulaCode = "1312",
								RateQualifier = "A01",
								Description = "Standard Rate 1",
								Preference = "100",
								Formula = "1.234 * VFD",
								UnitOfMeasureConverted = "KG"
							}
						}
					}
				}
			};

			var builder = new TariffBuilderForTest(header, logger);
			builder.CreateXmlFile(TempFolder, new DateTime(2022, 5, 6, 13, 14, 15));

			var fileName = Path.Combine(TempFolder, builder.OutputFilename);
			Assert.That(File.Exists(fileName));

			var xml = File.ReadAllText(fileName);

			var expectedXml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.ZAReferenceData.Tests.Business.Tariff.TestFiles.Output.ZATariff_002.xml");

			Assert.That(xml, Is.EqualTo(expectedXml));
		}

		[Test]
		public void DataFiltering_Tariff()
		{
			var rates = new List<Rate> { new Rate { RateType = RateTypes.Standard, Formula = "123" } };

			var header = new Header
			{
				Tariffs = new List<TariffData>
				{
					new TariffData { TariffCode = "111", Heading = "11", SubHeading = "11", Schedule = SARSSchedule.Get("1P1", ""), Description = "Valid", CheckDigit = "2", Rates = rates },
					new TariffData { TariffCode = "222", Heading = "22", SubHeading = "22", Description = "Not Valid - No Schedule", Rates = rates }
				}
			};

			var logger = new TestLogger();
			var builder = new TariffBuilderForTest(header, logger);
			var models = builder.ConvertToRefModels();

			Assert.That(models, Is.Not.Null);
			Assert.That(models.Count, Is.EqualTo(1));
			Assert.That(models[0].ZZ1_Description, Is.EqualTo("Valid"));
		}

		[Test]
		public void DataFiltering_Rate()
		{
			var header = new Header
			{
				Tariffs = new List<TariffData>
				{
					new TariffData
					{
						TariffCode = "111",
						Heading = "11",
						SubHeading = "11",
						Schedule = SARSSchedule.Get("2P3", ""),
						Description = "Valid",

						Rates = new List<Rate>
						{
							new Rate { RateType = RateTypes.Standard, Formula = "123", Description = "Valid" },
							new Rate { RateType = RateTypes.Standard, Description = "NotValid - NoFormula" },
							new Rate { RateType = RateTypes.SADC, Formula = "123", Description = "NotValid - Not 1P1 for non-standard" },
						}
					}
				}
			};

			var logger = new TestLogger();
			var builder = new TariffBuilderForTest(header, logger);
			var models = builder.ConvertToRefModels();

			Assert.That(models, Is.Not.Null);
			Assert.That(models.Count, Is.EqualTo(1));
			Assert.That(models[0].RefCusRates, Is.Not.Null);
			Assert.That(models[0].RefCusRates.Count, Is.EqualTo(1));
			Assert.That(models[0].RefCusRates[0].ZZ2_RateFormulaDerivedFrom, Is.EqualTo("Valid"));
		}

		[Test]
		public void RateGroupingByType()
		{
			var header = new Header
			{
				Tariffs = new List<TariffData>
				{
					new TariffData
					{
						TariffCode = "111",
						Heading = "11",
						SubHeading = "11",
						Schedule = SARSSchedule.Get("12A", ""),
						Description = "RateGroupingTest - Special case for 12A & 12B with Excise and Customs Rates A05 & A06",

						Rates = new List<Rate>
						{
							new Rate { RateType = RateTypes.Standard, RateQualifier = "A05", Formula = "123", Description = "Rate of Duty - Excise" },
							new Rate { RateType = RateTypes.Standard, RateQualifier = "A06", Formula = "123", Description = "Rate of Duty - Customs" }
						}
					}
				}
			};

			var logger = new TestLogger();
			var builder = new TariffBuilderForTest(header, logger);
			var models = builder.ConvertToRefModels();

			Assert.That(models, Is.Not.Null);
			Assert.That(models.Count, Is.EqualTo(1));
			Assert.That(models[0].RefCusRates, Is.Not.Null);
			Assert.That(models[0].RefCusRates.Count, Is.EqualTo(1));
		}

		[Test]
		public void CombineRatesWithSameFormulaAndPreference()
		{
			var header = new Header
			{
				Tariffs = new List<TariffData>
				{
					new TariffData
					{
						TariffCode = "111",
						Heading = "11",
						SubHeading = "11",
						Schedule = SARSSchedule.Get("1P1", ""),
						Description = "RateGroupingTest",
						CheckDigit = "2",

						Rates = new List<Rate>
						{
							new Rate { RateType = RateTypes.MERCOSUR, RateQualifier = "A05", Formula = "123", Description = "Rate1", Preference = "200", CountryCodes = new List<string> { "AA", "BB" } },
							new Rate { RateType = RateTypes.EFTA, RateQualifier = "A06", Formula = "123", Description = "Rate2", Preference = "200", CountryCodes = new List<string> { "BB", "CC" } },
							new Rate { RateType = RateTypes.EU, RateQualifier = "A05", Formula = "ABC", Description = "Diff Formula", Preference = "200" },
							new Rate { RateType = RateTypes.AFCFTA, RateQualifier = "A06", Formula = "123", Description = "Diff Pref", Preference = "300" }
						}
					}
				}
			};

			var logger = new TestLogger();
			var builder = new TariffBuilderForTest(header, logger);
			var models = builder.ConvertToRefModels();

			Assert.That(models, Is.Not.Null);
			Assert.That(models.Count, Is.EqualTo(1));
			Assert.That(models[0].RefCusRates, Is.Not.Null);
			Assert.That(models[0].RefCusRates.Count, Is.EqualTo(3));

			var combined = models[0].RefCusRates.First(x => x.ZZ2_RateFormula == "123");

			Assert.That(combined.RefCusApplicabilities, Is.Not.Null);
			Assert.That(combined.RefCusApplicabilities.Count, Is.EqualTo(3));
			Assert.That(combined.RefCusApplicabilities.FirstOrDefault(x => x.ZZT_ZZA_NKTradeGroup == "AA"), Is.Not.Null);
			Assert.That(combined.RefCusApplicabilities.FirstOrDefault(x => x.ZZT_ZZA_NKTradeGroup == "BB"), Is.Not.Null);
			Assert.That(combined.RefCusApplicabilities.FirstOrDefault(x => x.ZZT_ZZA_NKTradeGroup == "CC"), Is.Not.Null);
		}

		[Test]
		public void MultipleTariffUOM()
		{
			var header = new Header
			{
				Tariffs = new List<TariffData>
				{
					new TariffData
					{
						TariffCode = "111",
						Heading = "11",
						SubHeading = "11",
						Schedule = SARSSchedule.Get("1P1", ""),
						Description = "Multiple UOMs",
						CheckDigit = "2",

						Rates = new List<Rate>
						{
							new Rate { RateType = RateTypes.Standard, RateQualifier = "A06", Formula = "123", Description = "123", Preference = "100" }
						},

						StatisticalUnitConverted = "KG",
						AdditionalUOMs = new Dictionary<string, string>
						{
							{ "RU1", "LI" },
							{ "RU2", "UNIT" },
							{ "RUK", "" }
						}
					}
				}
			};

			var logger = new TestLogger();
			var builder = new TariffBuilderForTest(header, logger);
			var refModels = builder.ConvertToRefModels();

			Assert.That(refModels, Is.Not.Null);
			Assert.That(refModels.Count, Is.EqualTo(1));
			Assert.That(refModels[0].RefCusTariffUOMs, Is.Not.Null);
			Assert.That(refModels[0].RefCusTariffUOMs.Count, Is.EqualTo(3));
			Assert.That(refModels[0].RefCusTariffUOMs.FirstOrDefault(x => x.ZZ8_Type == "CU1" && x.ZZ8_UOM == "KG"), Is.Not.Null);
			Assert.That(refModels[0].RefCusTariffUOMs.FirstOrDefault(x => x.ZZ8_Type == "RU1" && x.ZZ8_UOM == "LI"), Is.Not.Null);
			Assert.That(refModels[0].RefCusTariffUOMs.FirstOrDefault(x => x.ZZ8_Type == "RU2" && x.ZZ8_UOM == "UNIT"), Is.Not.Null);
		}

		[Test]
		public void UOMSequencing()
		{
			var header = new Header
			{
				Tariffs = new List<TariffData>
				{
					new TariffData
					{
						TariffCode = "111",
						Heading = "11",
						SubHeading = "11",
						Schedule = SARSSchedule.Get("1P1", ""),
						Description = "Multiple UOMs",
						CheckDigit = "2",

						Rates = new List<Rate>
						{
							new Rate { RateType = RateTypes.Standard, RateQualifier = "A06", Formula = "123", Description = "123", Preference = "100", UnitOfMeasureConverted = "KG" }
						},

						StatisticalUnitConverted = "",
					}
				}
			};

			var logger = new TestLogger();
			var builder = new TariffBuilderForTest(header, logger);
			var refModels = builder.ConvertToRefModels();

			Assert.That(refModels, Is.Not.Null);
			Assert.That(refModels.Count, Is.EqualTo(1));
			Assert.That(refModels[0].RefCusTariffUOMs, Is.Not.Null);
			Assert.That(refModels[0].RefCusTariffUOMs.Count, Is.EqualTo(1));
			Assert.That(refModels[0].RefCusTariffUOMs.FirstOrDefault(x => x.ZZ8_Type == "CU1" && x.ZZ8_UOM == "KG"), Is.Not.Null);
		}

		[Test]
		public void AdditionalAttributes()
		{
			var header = new Header
			{
				Tariffs = new List<TariffData>
				{
					new TariffData
					{
						TariffCode = "111",
						Heading = "11",
						SubHeading = "11",
						Schedule = SARSSchedule.Get("1P1", ""),
						Description = "Multiple Attributes",
						CheckDigit = "12",

						Rates = new List<Rate>
						{
							new Rate { RateType = RateTypes.Standard, RateQualifier = "A06", Formula = "123", Description = "123", Preference = "100" }
						},

						AdditionalAttributes = new Dictionary<string, string>
						{
							{ "AT1", "Attrib1" },
							{ "AT2", "Attrib2" }
						}
					}
				}
			};

			var logger = new TestLogger();
			var builder = new TariffBuilderForTest(header, logger);
			var refModels = builder.ConvertToRefModels();

			Assert.That(logger.ErrorString, Is.EqualTo(string.Empty));
			Assert.That(refModels, Is.Not.Null);
			Assert.That(refModels.Count, Is.EqualTo(1));
			Assert.That(refModels[0].RefCusTariffAttributes, Is.Not.Null);
			Assert.That(refModels[0].RefCusTariffAttributes.Count, Is.EqualTo(3));
			Assert.That(refModels[0].RefCusTariffAttributes.FirstOrDefault(x => x.ZZ3_Name == Constants.CheckDigit && x.ZZ3_Value == "12"), Is.Not.Null);
			Assert.That(refModels[0].RefCusTariffAttributes.FirstOrDefault(x => x.ZZ3_Name == "AT1" && x.ZZ3_Value == "Attrib1"), Is.Not.Null);
			Assert.That(refModels[0].RefCusTariffAttributes.FirstOrDefault(x => x.ZZ3_Name == "AT2" && x.ZZ3_Value == "Attrib2"), Is.Not.Null);
		}

		[Test]
		public void Deactivate()
		{
			var header = new Header
			{
				GovernmentGazettePublicationNumber = "123",
				PublicationDate = new DateTime(2022, 3, 16, 13, 14, 15),
				TransactionType = TransactionType.Deletion,
				Tariffs = new List<TariffData>
				{
					new TariffData
					{
						CheckDigit = "1",
						Code = "",
						Description = "Tariff Data 1",
						GovernmentGazetteNoticeNumber = "345",
						Heading = "12.34",
						SubHeading = "12.34.56",
						ItemNumber = "",
						LineNumber = "27",
						ScheduleTypeCode = "1P1",
						ImportedFrom = "GERMANY",
						StatisticalUnitConverted = "KG",
						StartDate = new DateTime(2022, 1, 1),
						EndDate = new DateTime(2025, 12, 31, 23, 59, 59),

						ImportCountries = new List<string> { "DE" },
						Schedule = SARSSchedule.Get("1P1", "103"),
						TariffCode = "123456",
						RelationshipTariffCode = "123456",

						Rates = new List<Rate>
						{
							new Rate
							{
								RateType = RateTypes.Standard,
								Countries = "Argentina, Brazil",
								CountryCodes = new List<string> { "AR", "BR" },
								FormulaCode = "1312",
								RateQualifier = "A01",
								Description = "Standard Rate 1",
								Preference = "100",
								Formula = "1.234 * VFD",
								UnitOfMeasureConverted = "KG"
							}
						}
					}
				}
			};

			var logger = new TestLogger();
			var builder = new TariffBuilderForTest(header, logger);
			var refModels = builder.ConvertToRefModels();

			Assert.That(refModels, Is.Not.Null);
			Assert.That(refModels.Count, Is.EqualTo(1));

			var tariff1 = refModels[0];
			Assert.That(tariff1.ZZ1_ZZI_NKTariffType, Is.EqualTo("1P1"));
			Assert.That(tariff1.ZZ1_TariffCode, Is.EqualTo("123456"));
			Assert.That(tariff1.ZZ1_Description, Is.EqualTo("Tariff Data 1"));
			Assert.That(tariff1.ZZ1_StartDate, Is.EqualTo(new DateTime(2022, 1, 1)));
			Assert.That(tariff1.ZZ1_EndDate, Is.EqualTo(new DateTime(2025, 12, 31, 23, 59, 0)));
			Assert.That(tariff1.ZZ1_IAMUnique, Is.EqualTo(0));

			Assert.That(tariff1.RefCusTariffAttributes, Is.Not.Null);
			Assert.That(tariff1.RefCusTariffUOMs, Is.Null);
			Assert.That(tariff1.RefCusRates, Is.Null);
		}

		[Test]
		public void AllowEmptyRelationships()
		{
			var header = new Header
			{
				GovernmentGazettePublicationNumber = "123",
				PublicationDate = new DateTime(2022, 3, 16, 13, 14, 15),
				TransactionType = TransactionType.Original,
				Tariffs = new List<TariffData>
				{
					new TariffData
					{
						CheckDigit = "1",
						Code = "12.34.50",
						Description = "Tariff Data 1",
						GovernmentGazetteNoticeNumber = "345",
						Heading = "12.34",
						SubHeading = "12.34.56",
						ItemNumber = "103.34",
						LineNumber = "27",
						ScheduleTypeCode = "2P1",
						ImportedFrom = "GERMANY",
						StatisticalUnitConverted = "KG",
						StartDate = new DateTime(2022, 1, 1),
						EndDate = new DateTime(2025, 12, 31, 23, 59, 59),

						ImportCountries = new List<string> { "DE" },
						Schedule = SARSSchedule.Get("2P1", "103"),
						TariffCode = "103123400",
						RelationshipTariffCode = "",

						Rates = new List<Rate>
						{
							new Rate
							{
								RateType = RateTypes.Standard,
								Countries = "Argentina, Brazil",
								FormulaCode = "1312",
								RateQualifier = "A01",
								Description = "Standard Rate 1",
								Preference = "100",
								Formula = "1.234 * VFD",
								UnitOfMeasureConverted = "KG"
							}
						}
					},

					new TariffData
					{
						CheckDigit = "2",
						Code = "12.34.60",
						Description = "Tariff Data 2",
						GovernmentGazetteNoticeNumber = "345",
						Heading = "12.34",
						SubHeading = "12.34.56",
						ItemNumber = "103.56",
						LineNumber = "27",
						ScheduleTypeCode = "2P1",
						ImportedFrom = "GERMANY",
						StatisticalUnitConverted = "KG",
						StartDate = new DateTime(2022, 1, 1),
						EndDate = new DateTime(2025, 12, 31, 23, 59, 59),

						ImportCountries = new List<string> { "DE" },
						Schedule = SARSSchedule.Get("2P1", "103"),
						TariffCode = "103123400",
						RelationshipTariffCode = "123456",

						Rates = new List<Rate>
						{
							new Rate
							{
								RateType = RateTypes.Standard,
								Countries = "Argentina, Brazil",
								FormulaCode = "1312",
								RateQualifier = "A01",
								Description = "Standard Rate 1",
								Preference = "100",
								Formula = "1.234 * VFD",
								UnitOfMeasureConverted = "KG"
							}
						}
					},

					new TariffData
					{
						CheckDigit = "3",
						Code = "12.34.70",
						Description = "Tariff Data 3",
						GovernmentGazetteNoticeNumber = "345",
						Heading = "12.34",
						SubHeading = "12.34.78",
						ItemNumber = "103.78",
						LineNumber = "28",
						ScheduleTypeCode = "1P1",
						ImportedFrom = "GERMANY",
						StatisticalUnitConverted = "KG",
						StartDate = new DateTime(2022, 1, 1),
						EndDate = new DateTime(2025, 12, 31, 23, 59, 59),

						ImportCountries = new List<string> { "DE" },
						Schedule = SARSSchedule.Get("1P1", ""),
						TariffCode = "1122334455",
						RelationshipTariffCode = "WRONG",

						Rates = new List<Rate>
						{
							new Rate
							{
								RateType = RateTypes.Standard,
								Countries = "Argentina, Brazil",
								FormulaCode = "1312",
								RateQualifier = "A01",
								Description = "Standard Rate 1",
								Preference = "100",
								Formula = "1.234 * VFD",
								UnitOfMeasureConverted = "KG"
							}
						}
					}
				}
			};

			var logger = new TestLogger();
			var builder = new TariffBuilderForTest(header, logger);
			var refModels = builder.ConvertToRefModels();

			Assert.That(refModels, Is.Not.Null);
			Assert.That(refModels.Count, Is.EqualTo(3));

			var tariff1 = refModels.First(x => x.ZZ1_Description == "Tariff Data 1");
			Assert.That(tariff1.RefCusTariffRelationships, Is.Not.Null.And.Not.Empty);
			Assert.That(tariff1.RefCusTariffRelationships[0].ZZH_TariffCode, Is.EqualTo(string.Empty));

			var tariff2 = refModels.First(x => x.ZZ1_Description == "Tariff Data 2");
			Assert.That(tariff2.RefCusTariffRelationships, Is.Not.Null.And.Not.Empty);
			Assert.That(tariff2.RefCusTariffRelationships[0].ZZH_TariffCode, Is.EqualTo("123456"));

			Assert.That(refModels.First(x => x.ZZ1_Description == "Tariff Data 3").RefCusTariffRelationships, Is.Null.Or.Empty);
		}

		[Test]
		public void OutputFileName()
		{
			var header = new Header
			{
				GovernmentGazettePublicationNumber = "123",
				PublicationDate = new DateTime(2022, 3, 16, 13, 14, 15),
				TransactionType = TransactionType.Original,
				Tariffs = new List<TariffData>
				{
					new TariffData
					{
						CheckDigit = "1",
						Code = "123",
						Description = "Tariff Data 1",
						GovernmentGazetteNoticeNumber = "345",
						Heading = "12.34",
						SubHeading = "12.34.56",
						ItemNumber = "",
						LineNumber = "27",
						ScheduleTypeCode = "1P1",
						ImportedFrom = "GERMANY",
						StatisticalUnitConverted = "KG",
						StartDate = new DateTime(2022, 1, 1),
						EndDate = new DateTime(2025, 12, 31, 23, 59, 59),

						ImportCountries = new List<string> { "DE" },
						Schedule = SARSSchedule.Get("1P1", "103"),
						TariffCode = "123456",
						RelationshipTariffCode = "123456",

						Rates = new List<Rate>
						{
							new Rate
							{
								RateType = RateTypes.Standard,
								Countries = "Argentina, Brazil",
								CountryCodes = new List<string> { "AR", "BR" },
								FormulaCode = "1312",
								RateQualifier = "A01",
								Description = "Standard Rate 1",
								Preference = "100",
								Formula = "1.234 * VFD",
								UnitOfMeasureConverted = "LI"
							}
						},
						AdditionalUOMs = new Dictionary<string, string> { { "RU1", "LI" } },
						AdditionalAttributes = new Dictionary<string, string> { { "AT1", "Attribute1" } }
					}
				}
			};

			var builder = new TariffBuilderForTest(header, logger);
			builder.CreateXmlFile(TempFolder, new DateTime(2022, 4, 5, 13, 14, 15, 123));

			Assert.That(builder.OutputFilename, Is.EqualTo("ZA_RefCusTariff_20220316_20220405131415123.xml"));
			var fileName = Path.Combine(TempFolder, builder.OutputFilename);
			Assert.That(File.Exists(fileName));
		}

		[Test]
		public void DataSource()
		{
			var builder = new TariffBuilderForTest(null, null);
			Assert.That(builder.DataSource, Is.EqualTo("ZATariffs"));
		}

		[Test]
		public void UpdateType()
		{
			var builder = new TariffBuilderForTest(null, null);
			Assert.That(builder.UpdateType, Is.EqualTo(Common.UniversalXmlWriter.UpdateType.Partial));
		}

		[Test]
		public void OverrideRateApplicabilityDates()
		{
			var defaultStartDate = new DateTime(2025, 1, 1);
			var defaultEndDate = CommonHelper.MaximumDateTime;
			var header = new Header
			{
				Tariffs = new List<TariffData>
				{
					new TariffData
					{
						TariffCode = "111",
						Heading = "11",
						SubHeading = "11",
						Schedule = SARSSchedule.Get("1P1", ""),
						Description = "Valid",
						CheckDigit = "2",
						StartDate = defaultStartDate,
						EndDate = defaultEndDate,
						Rates = new List<Rate>
						{
							new Rate
							{
								RateType = RateTypes.Standard,
								Formula = "123",
								Description = "Standard Rate"
							},
							new Rate
							{
								RateType = RateTypes.SADC,
								Formula = "234",
								Description = "Rate to Expire",
								StartDate = new DateTime(2024, 12, 1),
								EndDate = new DateTime(2024, 12, 31, 23, 59, 0)
							},
						}
					}
				}
			};

			var logger = new TestLogger();
			var builder = new TariffBuilderForTest(header, logger);
			var models = builder.ConvertToRefModels();

			Assert.That(models, Is.Not.Null);
			Assert.That(models.Count, Is.EqualTo(1));
			Assert.That(models[0].RefCusRates, Is.Not.Null);
			Assert.That(models[0].RefCusRates.Count, Is.EqualTo(2));

			var standardRate = models[0].RefCusRates[0];
			Assert.That(standardRate.ZZ2_RateFormula, Is.EqualTo("123"));
			Assert.That(standardRate.ZZ2_StartDate, Is.EqualTo(defaultStartDate));
			Assert.That(standardRate.ZZ2_EndDate, Is.EqualTo(defaultEndDate));
			Assert.That(standardRate.RefCusApplicabilities[0].ZZT_StartDate, Is.EqualTo(defaultStartDate));
			Assert.That(standardRate.RefCusApplicabilities[0].ZZT_EndDate, Is.EqualTo(defaultEndDate));

			var rateToExpire = models[0].RefCusRates[1];
			Assert.That(rateToExpire.ZZ2_RateFormula, Is.EqualTo("234"));
			Assert.That(rateToExpire.ZZ2_StartDate, Is.EqualTo(new DateTime(2024, 12, 1)));
			Assert.That(rateToExpire.ZZ2_EndDate, Is.EqualTo(defaultEndDate));
			Assert.That(rateToExpire.RefCusApplicabilities[0].ZZT_StartDate, Is.EqualTo(new DateTime(2024, 12, 1)));
			Assert.That(rateToExpire.RefCusApplicabilities[0].ZZT_EndDate, Is.EqualTo(new DateTime(2024, 12, 31, 23, 59, 0)));
		}

		[Test]
		[Explicit("Developer Test - Produce full file for testing")]
		public void FullFile()
		{
			var logger = new TestLogger();
			var msgContent = string.Empty;

			var useLocalSafeDbAsSource = false;

			if (useLocalSafeDbAsSource)
			{
				using (var messageHandler = ObjectFactoryHelper.GetMessageHandler(logger, SupportedMessageTypes.Prodat))
				{
					var msgFromDb = messageHandler.GetMessages().FirstOrDefault();

					if (msgFromDb != null)
					{
						msgContent = msgFromDb.Content;
					}
				}
			}
			else
			{
				msgContent = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Tariff.TestFiles.Input.FullPRODAT.txt");
			}

			var msg = EdifactLoader.LoadProdatMessage(msgContent);

			var fullHeader = ProdatLoader.PopulateHeader(msg);

			if (useLocalSafeDbAsSource)
			{
				var dataLoader = new RefDataLoader(ConfigurationProvider.RefDbServiceURI, ConfigurationProvider.IsRefDbServiceSecure);
				fullHeader.ProcessUpdates(new CountryCodeLoaderForTest(), new TariffHelperForTest(dataLoader), logger);
			}
			else
			{
				fullHeader.ProcessUpdates(new CountryCodeLoaderForTest(), new TariffHelperForTest(), logger);
			}

			var folder = @"C:\Temp\ZATariff\UXmlFiles";

			var singleHeader = new Header { TransactionType = TransactionType.Original, PublicationDate = DateTime.Now, Tariffs = new List<TariffData>() };

			var tariffs = fullHeader.Tariffs
							//.Where(x => x.IsValidTariff(singleHeader) && x.TariffCode == "630100100")
							.ToList();

			singleHeader.Tariffs.AddRange(tariffs);

			var builder = new TariffBuilderForTest(singleHeader, logger);
			builder.CreateXmlFile(folder, DateTime.Now);
			var fileName = Path.Combine(folder, builder.OutputFilename);
			Assert.That(File.Exists(fileName));

			Console.WriteLine(logger.ErrorString);
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			logger = new TestLogger();
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			if (Directory.Exists(TempFolder))
			{
				Directory.Delete(TempFolder, true);
			}
		}

		TestLogger logger;
		string TempFolder;
	}

	class TariffBuilderForTest : TariffBuilder
	{
		public TariffBuilderForTest(Header sourceData, ILogger logger) : base(sourceData, logger)
		{
		}

		public new XmlWriterConfiguration GetXmlWriterConfiguration() => base.GetXmlWriterConfiguration();
		public new string FilePrefix => base.FilePrefix;
		public new string OutputFilename => base.OutputFilename;
		public new string DataSource => base.DataSource;
		public new UpdateType UpdateType => base.UpdateType;
		public new List<RefCusTariff> ConvertToRefModels() => base.ConvertToRefModels();
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Helpers;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Interfaces;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Models;
using CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Common;
using CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Tariff.TestClasses;
using Moq;
using NUnit.Framework;
using static CargoWise.RefDbRepo.ZAReferenceData.Services.Common.Constants;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Tariff
{
	[TestFixture]
	public class HeaderTests
	{
		[Test]
		public void GetUniqueCountryNames()
		{
			var header = new Header
			{
				Tariffs = new List<TariffData>
				{
					new TariffData
					{
						ImportedFrom = "abc",
						ScheduleTypeCode = "1P1",
						Rates = new List<Rate>
						{
							new Rate { RateType = RateTypes.Standard, Countries = "def" },
							new Rate { RateType = RateTypes.Standard },
							new Rate { RateType = RateTypes.Standard, Countries = "All countries" }
						}
					},
					new TariffData
					{
						ImportedFrom = "abc, qwe",
						ScheduleTypeCode = "1P1",
						Rates = new List<Rate>
						{
							new Rate { RateType = RateTypes.Standard, Countries = "qwe, rty" },
							new Rate { RateType = RateTypes.Standard, Countries = "abc" },
							new Rate { RateType = RateTypes.Standard, Countries = "Eu" }
						}
					},
					new TariffData
					{
						ScheduleTypeCode = "1P1"
					}
				}
			};

			var result = header.GetUniqueCountryNames().ToList();

			Assert.That(result.Count, Is.EqualTo(6));
			Assert.That(result.Contains("ABC"));
			Assert.That(result.Contains("DEF"));
			Assert.That(result.Contains("QWE"));
			Assert.That(result.Contains("RTY"));
			Assert.That(result.Contains("ALL COUNTRIES"));
			Assert.That(result.Contains("EU"));
		}

		[Test]
		public void CascadeProcessUpdates()
		{
			var logger = new TestLogger();
			var header = new Header
			{
				Tariffs = new List<TariffData>
				{
					new TariffData
					{
						ScheduleTypeCode = "1P1",
						Rates = new List<Rate>
						{
							new Rate { RateType = RateTypes.Standard }
						}
					}
				}
			};

			header.ProcessUpdates(new CountryCodeLoaderForTest(), new TariffHelperForTest(), logger);

			Assert.That(header.Tariffs[0].Schedule, Is.Not.Null);
			Assert.That(header.Tariffs[0].Rates[0].Preference, Is.EqualTo("100"));
		}

		[Test]
		public void Expire1P1TariffsAfterProcessUpdates()
		{
			var mockTariffHelper = new Mock<ITariffHelper>();
			mockTariffHelper.Setup(m => m.GetRules()).Returns(new List<RuleMapping>());
			mockTariffHelper.Setup(m => m.GetTariffs(It.IsAny<string>())).Returns(() =>
			{
				return new List<TariffData>
				{
					new TariffData
					{
						TariffCode = "940370",
						Description = "FURNITURE OF PLASTICS (1P1, 1)",
						Schedule = SARSSchedule.Get("1P1", "940370"),
						StartDate = new DateTime(2024, 1, 1),
						EndDate = CommonHelper.MaximumDateTime,
						UniqueId = 1,
						CheckDigit = "1"
					},
					new TariffData
					{
						TariffCode = "940370",
						Description = "FURNITURE OF PLASTICS (1P1, 2)",
						Schedule = SARSSchedule.Get("1P1", "940370"),
						StartDate = new DateTime(2024, 1, 1),
						EndDate = new DateTime(2024, 1, 6, 23, 59, 0),
						UniqueId = 2,
						CheckDigit = "2"
					},
					new TariffData
					{
						TariffCode = "940370",
						Description = "FURNITURE OF PLASTICS (1P1, 3)",
						Schedule = SARSSchedule.Get("1P1", "940370"),
						StartDate = new DateTime(2024, 2, 1),
						EndDate = CommonHelper.MaximumDateTime,
						UniqueId = 3,
						CheckDigit = "3"
					},
					new TariffData
					{
						TariffCode = "940370",
						Description = "FURNITURE OF PLASTICS (12B)",
						Schedule = SARSSchedule.Get("12B", "940370"),
						StartDate = new DateTime(2024, 1, 1),
						EndDate = CommonHelper.MaximumDateTime,
						UniqueId = 4,
						CheckDigit = "4"
					}
				};
			});

			var startDate = new DateTime(2024, 2, 1);
			var logger = new TestLogger();
			var header = new Header
			{
				Tariffs = new List<TariffData>
				{
					new TariffData
					{
						Description = "FURNITURE OF PLASTICS",
						Heading = "9403",
						SubHeading = "940370",
						TariffCode = "940370",
						ScheduleTypeCode = "1P1",
						StartDate = startDate,
						EndDate = CommonHelper.MaximumDateTime
					},
					new TariffData()
					{
						Heading = "0303",
						SubHeading = "030303",
						TariffCode = "030303",
						CheckDigit = "3",
						ScheduleTypeCode = "1P1",
						StartDate = startDate,
						EndDate = new DateTime(2024, 9, 30, 23, 59, 00),
						Rates = new List<Rate>
						{
							new Rate { RateType = RateTypes.Standard, Formula = "123", FormulaCode = "3410", Description = "10% OR 55C/KG LESS 90%", Preference = Preferences.None }
						}
					}
				}
			};

			header.ProcessUpdates(new CountryCodeLoaderForTest(), mockTariffHelper.Object, logger);

			Assert.That(header.Tariffs.Count, Is.EqualTo(3));

			var tariff1 = header.Tariffs[0];
			Assert.That(tariff1.IsHeading, Is.True);
			Assert.That(tariff1.TariffCode, Is.EqualTo("940370"));
			Assert.That(tariff1.EndDate, Is.EqualTo(CommonHelper.MaximumDateTime));
			Assert.That(tariff1.IsAddedForExpiration, Is.False);

			var tariff2 = header.Tariffs[1];
			Assert.That(tariff2.IsHeading, Is.False);
			Assert.That(tariff2.TariffCode, Is.EqualTo("030303"));
			Assert.That(tariff2.EndDate, Is.EqualTo(new DateTime(2024, 9, 30, 23, 59, 00)));
			Assert.That(tariff2.IsAddedForExpiration, Is.False);

			var tariff3 = header.Tariffs[2];
			Assert.That(tariff3.IsHeading, Is.False);
			Assert.That(tariff3.TariffCode, Is.EqualTo("940370"));
			Assert.That(tariff3.EndDate, Is.EqualTo(startDate.AddMinutes(-1)));
			Assert.That(tariff3.UniqueId, Is.EqualTo(1));
			Assert.That(tariff3.IsAddedForExpiration, Is.True);
		}
	}
}

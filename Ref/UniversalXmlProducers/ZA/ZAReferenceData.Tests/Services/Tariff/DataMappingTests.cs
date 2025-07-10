using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Staging.ApplicationConfig;
using CargoWise.RefDbRepo.Staging.Schema_New;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Helpers;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Loader;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Models;
using CargoWise.RefDbRepo.ZAReferenceData.Tests.Helpers;
using CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Common;
using CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Tariff.TestClasses;
using NUnit.Framework;
using static CargoWise.RefDbRepo.ZAReferenceData.Services.Common.Constants;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Tariff
{
	[TestFixture]
	public class DataMappingTests
	{
		[Test]
		public void TariffSchedules()
		{
			Assert.That(header, Is.Not.Null);
			Assert.That(header.Tariffs, Is.Not.Null.And.Not.Empty);

			foreach (var t in header.Tariffs)
			{
				var sched = SARSSchedule.Get(t.ScheduleTypeCode, t.ItemNumber);
				if (t.ScheduleTypeCode == "13F")
				{
					Assert.That(sched.Schedule, Is.EqualTo(0), "13F schedule should be ignored");
				}
				else if (sched.Schedule == 0)
				{
					Assert.Fail($"ScheduleType mapping missing for '{t.ScheduleTypeCode}' Line Number: {t.LineNumber} ");
				}
			}
		}

		[Test]
		public void TariffsWithDuplicateRateTypes()
		{
			Assert.That(header, Is.Not.Null);
			Assert.That(header.Tariffs, Is.Not.Null.And.Not.Empty);

			foreach (var t in header.Tariffs)
			{
				var rateTypes = t.Rates.GroupBy(r => new { r.RateType })
									.Select(g => new { RateKey = g.Key, RateCount = g.Count(), rates = g.ToList() })
									.Where(x => x.RateCount > 1);

				foreach (var rt in rateTypes)
				{
					var d = rt.rates.Select(x => x.FormulaCode).Distinct().Count();

					if (d > 1)
					{
						Assert.Fail($"Data Check: Tariff code: {t.TariffCode} Line Number: {t.LineNumber} diffs: {d}");
					}
				}
			}
		}

		[Test]
		public void TariffCodesInSafeDb()
		{
			var safeDbTariffTypes = new[]
			{
				"1P1", "12A", "12B", "13A", "13B", "13C", "13D", "13E", "13F", "15A", "15B", "16A", "17A", "1P8",
				"2P1", "2P2", "2P3",
				"3P1", "3P2",
				"4P1", "4P2", "4P3", "4P4", "4P5", "4P6",
				"5P1", "5P2", "5P3", "5P4", "5P5", "5P6",
				"6P1", "6P2", "6P3", "6P4", "6P5", "6P6"
			};

			var missing = new List<SARSSchedule>();

			foreach (var schedule in SARSSchedule.GetAllSchedules())
			{
				var tt = schedule.GetTariffType();
				if (!safeDbTariffTypes.Contains(tt))
				{
					missing.Add(schedule);
				}
			}

			if (missing.Any())
			{
				Assert.Fail($"The following schedule tariff types are missing:\n {string.Join("\n", missing.Select(x => $"Schedule: {x.ScheduleType} Tariff Type: {x.GetTariffType()}"))}");
			}
		}

		[Test]
		public void UnitsOfMeasure()
		{
			Assert.That(header, Is.Not.Null);
			Assert.That(header.Tariffs, Is.Not.Null.And.Not.Empty);

			var units = header.Tariffs.Select(x => x.StatisticalUnitOriginal).ToList();
			units.AddRange(header.Tariffs.SelectMany(x => x.Rates.Select(r => r.UnitOfMeasureOriginal)));

			foreach (var unit in units.Where(x => !string.IsNullOrWhiteSpace(x)))
			{
				var cwUnit = SARSMappingHelper.GetUnitOfMeasure(unit, out var found);
				if (!found)
				{
					Assert.Fail($"Unit conversion for '{unit}' not found");
				}
			}
		}

		[Test]
		[Explicit("Developer Test to view actual formula data")]
		public void FormulaCodes()
		{
			Assert.That(header, Is.Not.Null);
			Assert.That(header.Tariffs, Is.Not.Null.And.Not.Empty);

			foreach (var schGrp in header.Tariffs.GroupBy(x => x.ScheduleTypeCode).OrderBy(x => x.Key))
			{
				var formulaCode = schGrp.SelectMany(x => x.Rates).Select(x => new { x.FormulaCode, x.Description }).Distinct()
									.GroupBy(x => x.FormulaCode)
									.Select(g => new { g.Key, num = g.Count(), descrips = g.Select(x => x.Description).Distinct().OrderBy(x => x).ToList() })
									.OrderBy(g => g.Key)
									.ToList();

				formulaCode.ForEach(x =>
				{
					x.descrips.ForEach(d => Console.WriteLine($"[TestCase(\"{x.Key}\", \"{schGrp.Key}\", \"{d}\", \"to\", \"do\")]"));
				});
			}
		}

		[Test]
		public void AllFormulasMatched()
		{
			Assert.That(header, Is.Not.Null);
			Assert.That(header.Tariffs, Is.Not.Null.And.Not.Empty);

			foreach (var t in header.Tariffs)
			{
				foreach (var r in t.Rates)
				{
					var matched = SARSFormulaHelperForTest.PopulateFormulaData(t, r);

					if (!matched)
					{
						Assert.Fail($"Formula not mapped - Tariff: {t.TariffCode} Line: {t.LineNumber} RateQ: {r.RateQualifier} Code: {r.FormulaCode} Descrip: {r.Description}");
					}
				}
			}
		}

		[Test]
		[Explicit("Developer test to check which tariffs are valid")]
		public void ValidTariffsPerSchedule()
		{
			var logger = new TestLogger();
			Assert.That(header, Is.Not.Null);
			Assert.That(header.Tariffs, Is.Not.Null.And.Not.Empty);

			var perSched = header.Tariffs.GroupBy(x => x.Schedule)
							.Select(x => new { SchedType = x.Key.ScheduleType, Data = x.ToList() })
							.ToList();

			foreach (var ps in perSched.OrderBy(x => x.SchedType))
			{
				var results = ps.Data.GroupBy(x => x.IsValidTariff(header, logger)).Select(x => new { x.Key, Num = x.Count() }).ToList();

				var val = results.FirstOrDefault(x => x.Key);
				var inv = results.FirstOrDefault(x => !x.Key);

				Console.WriteLine($"{ps.SchedType}: {val?.Num ?? -1} {inv?.Num ?? -1}");
			}
		}

		[Test]
		[Explicit("Developer Test to check data on all messages received since 2018 (from local repo)")]
		public void AllFiles_CheckMappings()
		{
			var logger = new TestLogger();
			var errorCollector = new StringBuilder();
			var countryLoader = new CountryCodeLoaderForTest();

			using (var stagingRepo = StagingRepositoryFactory.GetStagingRepository())
			{
				var sourceMessages = stagingRepo.Get<SourceData>()
							.Where(x => x.SDA_Source == DataSourceConstants.Source.eHubZACustomsRepositoryQueue &&
										x.SDA_ContentType == DataSourceConstants.ContentType.ZA_ProDat).ToList();

				foreach (var sMsg in sourceMessages)
				{
					var msg = EdifactLoader.LoadProdatMessage(sMsg.SDA_ContentText);

					if (ProdatValidator.ValidateMessage(msg, errorCollector))
					{
						var header = ProdatLoader.PopulateHeader(msg);
						if (header != null)
						{
							if (header.Tariffs.Any())
							{
								header.ProcessUpdates(countryLoader, new TariffHelperForTest(), logger);
								CheckTariffSchedules(errorCollector, header, sMsg.SDA_PK);
								CheckTariffsWithDuplicateRateTypes(errorCollector, header, sMsg.SDA_PK);
								CheckAllFormulasMatched(errorCollector, header, sMsg.SDA_PK);
								CheckDuplicateTariffCodes(errorCollector, header, sMsg.SDA_PK);
								CheckTariffDatesForDeletes(errorCollector, header, sMsg.SDA_PK);
							}
							else
							{
								errorCollector.AppendLine(CultureInfo.InvariantCulture, $"Warning: No tariffs Header from Prodat message. SDA_PK: {sMsg.SDA_PK}");
							}
						}
						else
						{
							errorCollector.AppendLine(CultureInfo.InvariantCulture, $"Could not populate Header from Prodat message. SDA_PK: {sMsg.SDA_PK}");
						}
					}
					else
					{
						errorCollector.AppendLine(CultureInfo.InvariantCulture, $"Could not load EdiFact message. SDA_PK: {sMsg.SDA_PK}");
					}
				}
			}

			Console.WriteLine(errorCollector.ToString());
		}

		void CheckTariffSchedules(StringBuilder errorCollector, Header header, Guid msgId)
		{
			foreach (var t in header.Tariffs)
			{
				var sched = SARSSchedule.Get(t.ScheduleTypeCode, t.ItemNumber);
				if (sched.Schedule == 0)
				{
					errorCollector.AppendLine(CultureInfo.InvariantCulture, $"ScheduleType mapping missing for ScheduleTypeCode: {t.ScheduleTypeCode} ItemNumnber: {t.ItemNumber} Line Number: {t.LineNumber} SDA_PK: {msgId}");
				}
			}
		}

		void CheckTariffsWithDuplicateRateTypes(StringBuilder errorCollector, Header header, Guid msgId)
		{
			foreach (var t in header.Tariffs)
			{
				var rateTypes = t.Rates.GroupBy(r => new { r.RateType })
									.Select(g => new { RateKey = g.Key, RateCount = g.Count(), rates = g.ToList() })
									.Where(x => x.RateCount > 1);

				foreach (var rt in rateTypes)
				{
					var d = rt.rates
						.Where(x => x.FormulaCode != "3436")
						.Select(x => $"{x.FormulaCode}:{x.Description}").Distinct().Count();

					if (d > 1)
					{
						errorCollector.AppendLine(CultureInfo.InvariantCulture, $"Duplicate Rate Types: Tariff code: {t.TariffCode} Line Number: {t.LineNumber} diffs: {d} SDA_PK: {msgId}");
					}
				}
			}
		}

		void CheckAllFormulasMatched(StringBuilder errorCollector, Header header, Guid msgId)
		{
			foreach (var t in header.Tariffs)
			{
				foreach (var r in t.Rates)
				{
					var matched = SARSFormulaHelperForTest.PopulateFormulaData(t, r);

					if (!matched)
					{
						errorCollector.AppendLine(CultureInfo.InvariantCulture, $"Formula not mapped - Tariff: {t.TariffCode} Line: {t.LineNumber} SDA_PK: {msgId} RateQ: {r.RateQualifier} Code: {r.FormulaCode} Descrip: {r.Description}");
					}
				}
			}
		}

		void CheckDuplicateTariffCodes(StringBuilder errorCollector, Header header, Guid msgId)
		{
			var duplicates = header.Tariffs
								.Where(x => !string.IsNullOrWhiteSpace(x.TariffCode))
								.GroupBy(x => new { x.TariffCode, x.CheckDigit } )
								.Select(g => new { g.Key, Num = g.Count(), dups = g.ToList().Select(x => x.LineNumber) })
								.Where(x => x.Num > 1);

			foreach (var dup in duplicates)
			{
				errorCollector.AppendLine(CultureInfo.InvariantCulture, $"Duplicate Tariff codes: Tariff Code: {dup.Key.TariffCode} Chk: {dup.Key.CheckDigit} Lines: {string.Join("|", dup.dups)} SDA_PK: {msgId}");
			}
		}

		void CheckTariffDatesForDeletes(StringBuilder errorCollector, Header header, Guid msgId)
		{
			if (header.TransactionType == TransactionType.Deletion)
			{
				var maxEnd = header.Tariffs.Select(x => x.CalcEndDate()).Max();
				if ((maxEnd - header.PublicationDate).TotalDays > 7)
				{
					errorCollector.AppendLine(CultureInfo.InvariantCulture, $"Delete Dates: PubDate: {header.PublicationDate} MaxEnd: {maxEnd} SDA_PK: {msgId}");
				}
			}
		}

		[Test]
		[Explicit("Developer Test to create a lookup file to easily find tariffs in the PRODAT")]
		public void CreateLookupFile()
		{
			var logger = new TestLogger();
			var msgContent = System.IO.File.ReadAllText(@"C:\Temp\ZATariff\Prod\PRODAT_MASTER_20220804.txt");
			var msg = EdifactLoader.LoadProdatMessage(msgContent);

			var header = ProdatLoader.PopulateHeader(msg);

			header.ProcessUpdates(new CountryCodeLoaderForTest(), new TariffHelperForTest(), logger);

			var sb = new StringBuilder();
			sb.AppendLine("Lookup for PRODAT_MASTER_20220804");

			sb.AppendLine("Line No\tTariffCode\tCheck Digit\tRelated Tariff\tSchedule\tDescription");

			foreach (var t in header.Tariffs)
			{
				var descrip = t.Description.Replace("\r", "").Replace("\n", "");
				if (descrip.Length > 100)
				{
					descrip = descrip.Substring(0, 100);
				}
				sb.AppendLine(CultureInfo.InvariantCulture, $"{t.LineNumber}\t{t.TariffCode}\t{t.CheckDigit}\t{t.RelationshipTariffCode}\t{t.Schedule.ScheduleType}\t{descrip}");
			}

			System.IO.File.WriteAllText(@"C:\Temp\ZATariff\Prod\PRODAT_MASTER_20220804_Lookup.txt", sb.ToString());
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			var logger = new TestLogger();
			var msgContent = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Tariff.TestFiles.Input.FullPRODAT.txt");
			var msg = EdifactLoader.LoadProdatMessage(msgContent);

			header = ProdatLoader.PopulateHeader(msg);

			header.ProcessUpdates(new CountryCodeLoaderForTest(), new TariffHelperForTest(), logger);
		}

		Header header;
	}
}

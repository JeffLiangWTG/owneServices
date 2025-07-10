using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.GBReferenceData.Business.Tariff;
using CargoWise.RefDbRepo.GBReferenceData.Services.Tariff;
using CargoWise.RefDbRepo.GBReferenceData.Tests.Tariff.Helpers;
using CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff;
using CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Processors;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Loaders;
using NUnit.Framework;
using static CargoWise.RefDbRepo.SharedReferenceData.Business.Common.Tests.CommonHelpers;
using static CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Helpers.Tests.TestHelperClasses;

namespace CargoWise.RefDbRepo.GBReferenceData.Tests.Tariff
{
	[TestFixture]
	class DeveloperTests
	{
		[Test]
		[Explicit("Debug/Developer Test")]
		public void Dev_Step1_CreateFiles()
		{
			var workingFolder = @"C:\Work\GBCustoms\TariffDataFiles";
			var contentFolder = Path.Combine(workingFolder, "Content");

			var xmlDirInfo = new DirectoryInfo(contentFolder);
			var xmlFiles = xmlDirInfo.GetFiles("*.xml");

			if (!xmlFiles.Any())
			{
				var zipDirInfo = new DirectoryInfo(workingFolder);
				var zips = zipDirInfo.GetFiles("*.gzip");

				if (zips.Any())
				{
					foreach (var z in zips)
					{
						FileHelper.UnzipFile(z.FullName, contentFolder);
					}

					xmlFiles = xmlDirInfo.GetFiles("*.xml");
				}
			}

			if (!xmlFiles.Any())
			{
				Assert.Fail("No files -> process aborted");
			}
			else
			{
				var dateTimeProvider = new DateTimeProvider();
				var errorCollector = new StringBuilder();

				dateTimeProvider.TestHistoricalDateTime = dateTimeProvider.UTCDateTime;
				var measureProcessor = new MeasureProcessorTester(dateTimeProvider, new MeasureMappingProvider(), new IRefXmlBuilder[] { new TariffBuilder(dateTimeProvider, errorCollector), new EUNVATBuilder(dateTimeProvider, errorCollector) });
				var goodsNomenclatureProcesor = new GoodsNomenclatureProcessorTester(dateTimeProvider, new IRefXmlBuilder[] { new GoodsNomenclatureBuilder(dateTimeProvider, errorCollector) });

				var itemId = "9022190000";
				var geo = "BY";

				// Use this code to add additinal information to the test log to trace where an error might occur
				measureProcessor.TrackingAction = (action, models) =>
				{
					Console.WriteLine($"Action: {action}");

					var check = models.Where(x => x.ItemId == itemId && x.GeographicalArea == geo).ToList();

					Console.WriteLine($"Num: {check.Count}");
					Console.WriteLine(string.Join(", ", check.Take(10).Select(x => x.HJID)));
				};

				goodsNomenclatureProcesor.TrackingAction = (action, models) =>
				{
					Console.WriteLine($"Action: {action}");

					var check = models.FirstOrDefault(x => x.ItemId == itemId);
					if (check == null)
					{
						Console.WriteLine($"{itemId} not found");
					}
					else
					{
						if (action == "GoodsNomenclature CraeteKeys")
						{
							while (check != null)
							{
								Console.WriteLine($"HJID: {check.HJID} Id: {check.Id} ParentId: {check.ParentId} ItemId: {check.ItemId}");

								check = !check.ParentId.HasValue ? null : models.FirstOrDefault(x => x.Id == check.ParentId.Value);
							}
						}
					}
				};

				var processors = new IProcessor[]
				{
					goodsNomenclatureProcesor,
					measureProcessor,
					new GeographicalAreaProcessor(dateTimeProvider, new IRefXmlBuilder[] { new TradeGroupBuilder(dateTimeProvider, errorCollector) }),
					new AdditionalCodeProcessor(dateTimeProvider, new IRefXmlBuilder[] { new AdditionalCodeBuilder(dateTimeProvider, errorCollector) })
				};

				var loaders = new ILoader[]
				{
					new BaseRegulationLoader(),
					new ModificationRegulationLoader(),
					new MeasureTypeLoader(),
					new MeasureConditionCodeLoader(),
				};

				var process = new ProcessManagerForTest(processors, loaders);
				process.TestChapters = new string[] { "90" };
				process.TestFileManager = new FileManagerForTest(null, contentFolder);

				process.RunProcess(@"C:\Temp\CDSTariff", errorCollector);

				Console.WriteLine($"Errors:\n{errorCollector}"); // See results in Unit Test Additional Output

				Assert.That(errorCollector.ToString(), Does.Not.Contain("duplicate"));
			}
		}

		[Test]
		[Explicit("Debug/Developer Test")]
		public void Dev_Step2_ImportIntoStaging()
		{
			var di = new DirectoryInfo(@"C:\Temp\CDSTariff");
			//var di = new DirectoryInfo(@"C:\RefDataRepo\RefDataRepo\Bin\UXmlFiles");

			foreach (var f in di.GetFiles("*.xml"))
			{
				var proc = Process.Start(@"C:\RefDataRepo\RefDataRepo\Bin\Staging\net8.0\CargoWise.RefDbRepo.UniversalXmlProcessor.exe", $"\"{f.FullName}\"");

				proc.WaitForExit(1000 * 60 * 5);
			}
		}

		[Test]
		[Explicit("Debug/Developer Test")]
		public void Dev_Step3_StagingToSafe()
		{
			var processed = 0;
			var total = new TimeSpan();
			var secs = 5.0;

			var serverProc = Process.Start(@"C:\RefDataRepo\RefDataRepo\Bin\Server\net8.0\CargoWise.RefDbRepo.NewSafeDataUpdateService.exe");

			serverProc.WaitForExit(1000 * 5); // Wait for start-up

			while (secs >= 5)
			{
				Console.Write("Starting...");
				var sw = new Stopwatch();
				sw.Start();
				var proc = Process.Start(@"C:\RefDataRepo\RefDataRepo\Bin\Staging\net8.0\CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.exe");

				proc.WaitForExit(1000 * 60 * 120);

				sw.Stop();
				secs = sw.Elapsed.TotalSeconds;

				Console.WriteLine($"Done {sw.Elapsed}");

				if (secs >= 5)
				{
					processed++;
					total = total.Add(sw.Elapsed);
				}
			}

			Console.WriteLine($"Processed {processed} times in {total}");

			serverProc.Kill();
		}

		[Test]
		[Explicit("Debug/Developer Test")]
		public void Dev_Step4_Publish()
		{
			var proc = Process.Start(@"C:\RefDataRepo\RefDataRepo\Bin\Staging\net8.0\CargoWise.RefdbRepo.Staging.DataPublishingProcessor.exe", "AUTO");
			proc.WaitForExit();

			Console.WriteLine($"Data ready for CW1 Import");
		}

		[Test]
		[Explicit("Debug/Developer Test")]
		public async Task Dev_RetrievePublishedMeasureTypes()
		{
			var binDir = new DirectoryInfo(Path.GetDirectoryName(GetType().Assembly.Location));
			var gitRepoDir = binDir.Parent.Parent.Parent.FullName;
			var measureTypesJsonPath = Path.Combine(gitRepoDir, "UniversalXmlProducers", "GB", "GBReferenceData.Tests", "Tariff", "TestFiles", "measure_types.json");
			Assert.That(File.Exists(measureTypesJsonPath), Is.True);

			const string baseAddress = "https://www.trade-tariff.service.gov.uk";
			const string resource = "/api/v2/measure_types";

			using (var httpClient = new HttpClient { BaseAddress = new Uri(baseAddress) })
			using (var response = await httpClient.GetAsync(new Uri($"{baseAddress}{resource}")))
			{
				response.EnsureSuccessStatusCode();
				var jsonResponse = await response.Content.ReadAsStringAsync();
				var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower, WriteIndented = true };
				var publishedMeasureTypesList = JsonSerializer.Deserialize<MeasureMappingProviderTests.PublishedMeasureTypesList>(jsonResponse, jsonOptions);
				var formattedJson = JsonSerializer.Serialize(publishedMeasureTypesList, jsonOptions);
				await File.WriteAllTextAsync(measureTypesJsonPath, formattedJson);
			}

			Console.WriteLine($"{measureTypesJsonPath} updated");
		}
	}
}

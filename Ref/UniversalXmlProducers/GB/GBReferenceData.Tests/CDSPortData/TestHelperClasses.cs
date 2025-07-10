using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.GBReferenceData.Services.CDSPortData.Config;
using CargoWise.RefDbRepo.GBReferenceData.Services.CDSPortData.Downloader;
using CargoWise.RefDbRepo.GBReferenceData.Services.CDSPortData.Models;
using CargoWise.RefDbRepo.GBReferenceData.Services.CDSPortData.Processing;
using CargoWise.RefDbRepo.GBReferenceData.Services.CDSPortData.RefDbService;
using CargoWise.RefDbRepo.GBReferenceData.Services.Common;
using Moq;
using static CargoWise.RefDbRepo.GBReferenceData.Tests.Common.CommonHelpers;

namespace CargoWise.RefDbRepo.GBReferenceData.Tests.CDSPortData
{
	internal static class TestHelperClasses
	{
		public class ConfigProviderTester : IConfigProvider
		{
			public string ConfigFile { get; set; }
		}

		public class ProcessManagerTester : ProcessManager
		{
			public IConfigProvider TestConfigProvider { get; set; } = new ConfigProviderTester();
			public override IConfigProvider GetConfigProvider() => TestConfigProvider;

			public IDownloadManager TestDownloadManager { get; set; } = new DownloadManager(new ManifestClientWrapper());
			public override IDownloadManager GetDownloadManager(StringBuilder errorCollector) => TestDownloadManager;

			public IPortBuilder TestPortBuilder { get; set; } = new VirtualPortBuilder();
			public override IPortBuilder GetPortBuilder() => TestPortBuilder;

			public IRefDataLoader TestRefDataLoader { get; set; } = new VirtualRefDataLoader();
			public override CCSUKLocationLoader GetCCSUKLocationLoader() => new CCSUKLocationLoaderTester(TestRefDataLoader);

			public void RunLocalProcess(string outputPath, StringBuilder errorCollector)
			{
				try
				{
					var configProvider = GetConfigProvider();
					var downloadManager = GetDownloadManager(errorCollector);
					var builder = GetPortBuilder();
					var ccsukDataTask = GetCCSUKLocationLoader().GetLocations();
					var sourceDataList = ConfigLoader.LoadConfigFile(configProvider);

					Parallel.ForEach(sourceDataList, (source) =>
					{
						try
						{
							var processor = GetProcessor(source, errorCollector, downloadManager);
							Console.WriteLine($"Got {processor.GetType().Name} for '{source.Name}'");

							Console.WriteLine($"{DateTime.Now} Extracting Source '{source.Name}'");
							var data = processor.Extract();

							Console.WriteLine($"{DateTime.Now} Building Source '{source.Name}'");
							builder.BuildXml(DateTime.Now, data, outputPath, ccsukDataTask);

							Console.WriteLine($"{DateTime.Now} Completed Source '{source.Name}'");
						}
						catch (Exception ex)
						{
							Console.WriteLine($"{DateTime.Now} Failed on Source '{source.Name}'");
							errorCollector.AppendLine(CultureInfo.InvariantCulture, $"Processing failure for Source '{source.Name} Exception: {ex.GetBaseException().Message}");
						}
					});
				}
				catch (Exception ex)
				{
					throw new ApplicationException("Processing failed", ex);
				}
			}
		}

		public class VirtualPortBuilder : IPortBuilder
		{
			public int BuildCallCount { get; set; }
			public int BuildModelCount { get; set; }
			public int BuildCCSUKCount { get; set; }

			public void BuildXml(DateTime publicationDate, IEnumerable<PortData> data, string outputPath, Task<IEnumerable<CCSUKLocation>> ccsukData)
			{
				lock (new object())
				{
					BuildCallCount++;
					BuildModelCount += data.Count();

					if (data.Any() && data.First().Source.CheckCCSUKLocation)
					{
						BuildCCSUKCount += ccsukData.Result.Count();
					}
				}
			}
		}

		public class CCSUKLocationLoaderTester : CCSUKLocationLoader
		{
			public CCSUKLocationLoaderTester(IRefDataLoader refDataLoader) : base(refDataLoader)
			{
			}

			public new string CreateUrlQuery() => CCSUKLocationLoader.CreateUrlQuery().OriginalString;

			public IRefDataLoader GetRefDataLoader => base.RefDataLoader;
		}

		public class CSVProcessorTester : CSVProcessor
		{
			public CSVProcessorTester(ICDSPortSource source, IDownloadManager downloadManager) : base(source, downloadManager)
			{
			}

			public List<string[]> ReadContentExposed() => base.ReadContent();
		}

		public class VirtualRefDataLoader : IRefDataLoader
		{
			public Dictionary<string, int> CallCount { get; } = new Dictionary<string, int>();

			public VirtualRefDataLoader()
			{
				RefCusCodeListData = CreateRefCusCodeListTestData;
			}

			public async Task<IEnumerable<T>> LoadData<T>(string urlQuery)
			{
				SetCallCount(typeof(T).Name);
				var list = new List<T>();

				foreach (var item in await Task.Factory.StartNew(() => CreateTestData<T>(urlQuery)))
				{
					list.Add(item);
				}

				return list;
			}

			void SetCallCount(string name)
			{
				if (CallCount.ContainsKey(name))
				{
					CallCount[name]++;
				}
				else
				{
					CallCount.Add(name, 1);
				}
			}

			IEnumerable<T> CreateTestData<T>(string urlQuery)
			{
				var list = new List<T>();

				if (urlQuery.Contains(nameof(RefCusCodeList)))
				{
					foreach (var x in RefCusCodeListData?.Invoke())
					{
						list.Add((T)Convert.ChangeType(x, typeof(T), CultureInfo.CurrentCulture));
					}
				}

				return list;
			}

			public delegate IEnumerable<RefCusCodeList> CreateRefCusCodeListDelegate();
			public CreateRefCusCodeListDelegate RefCusCodeListData { get; set; }

			protected virtual IEnumerable<RefCusCodeList> CreateRefCusCodeListTestData()
			{
				// Sleep to simulate long running process
				yield return CreateRefCusCodeList("ABCDEF");
				System.Threading.Thread.Sleep(SleepInterval);
				yield return CreateRefCusCodeList("QWERTY");
				System.Threading.Thread.Sleep(SleepInterval);
				yield return CreateRefCusCodeList("JHGFDD");
				System.Threading.Thread.Sleep(SleepInterval);
				yield return CreateRefCusCodeList("QWECUK");
				System.Threading.Thread.Sleep(SleepInterval);
				yield return CreateRefCusCodeList("QWERTY");
				System.Threading.Thread.Sleep(SleepInterval);
				yield return CreateRefCusCodeList("THEEND");
			}

			public int SleepInterval { get; set; } = 1000;

			public static RefCusCodeList CreateRefCusCodeList(string code) => new RefCusCodeList { ZZD_Code = code };
		}

		public class VirtualDataProcessor : DataProcessor
		{
			public VirtualDataProcessor(ICDSPortSource source) : this(source, Mocker.GetDownloadManagerForCSV("")) { }
			public VirtualDataProcessor(ICDSPortSource source, IDownloadManager downloadManager) : base(source, downloadManager)
			{
			}

			public bool TestHasContent { get; set; } = true;
			protected override bool HasContent() => TestHasContent;

			public List<string[]> TestReadContent { get; set; } = new List<string[]>
			{
				new [] { "A1", "A2", "A3", "A4" },
				new [] { "B1", "B2", "B3", "B4" },
				new [] { "c1", "c2", "C3", "C4" },
				new string[] { },
				new [] { "dodgy data" },
				new [] { "VALID", "INVALID ITEM", "Validation Test", "" },
				new [] { "CODE1", "some code", "Validation Test2", "" }
			};
			protected override List<string[]> ReadContent() => TestReadContent;
		}

		public static class Mocker
		{
			public static IDownloadManager GetDownloadManagerForCSV(string content)
			{
				var mock = new Mock<IDownloadManager>();
				mock.Setup(x => x.GetCSVContent(It.IsAny<ICDSPortSource>())).Returns(content);
				return mock.Object;
			}

			public static IDownloadManager GetDownloadManagerForBinary(byte[] content)
			{
				var mock = new Mock<IDownloadManager>();
				mock.Setup(x => x.GetBinaryData(It.IsAny<ICDSPortSource>())).Returns(content);
				return mock.Object;
			}
		}
	}
}

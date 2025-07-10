using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Helpers.Tests;
using CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Processors;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Loaders;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using static CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Helpers.Tests.TestHelperClasses;

namespace CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Tests
{
	[TestFixture]
	class ProcessManagerTests
	{
		[Test]
		public void RunProcess()
		{
			var processManager = new ProcessManagerForTest();
			var errorCollector = new StringBuilder();
			var mockFileManager = new Mock<IFileManager>();
			var mockProcessor1 = new Mock<IProcessor>();
			var mockProcessor2 = new Mock<IProcessor>();
			var mockProcessor3 = new Mock<IProcessor>();
			var processCount = new Dictionary<string, int>();

			var workingFolder = TempFolder;

			mockFileManager.Setup(x => x.GetFiles()).Returns(new List<IFileDetails>
			{
				new FileDetails { Filename = "File1", Content = new ContentDetails { ExecutionDate = DateTime.Now } },
				new FileDetails { Filename = "File2", Content = new ContentDetails { ExecutionDate = DateTime.Now } }
			});

			processManager.TestFileManager = mockFileManager.Object;

			Action<IReadOnlyCollection<IFileDetails>> processCounter = (files) =>
			{
				foreach (var f in files)
				{
					var filename = f.Filename;
					if (processCount.ContainsKey(filename))
					{
						processCount[filename]++;
					}
					else
					{
						processCount.Add(filename, 1);
					}
				}
			};

			mockProcessor1.Setup(x => x.LoadData(It.IsAny<string>(), It.IsAny<IReadOnlyCollection<IFileDetails>>(), errorCollector)).Callback<string, IReadOnlyCollection<IFileDetails>, StringBuilder>((chapter, files, sb) => processCounter(files));
			mockProcessor1.Setup(x => x.Models).Returns(() => new List<ITariffModel>());
			mockProcessor1.Setup(x => x.IsChapterSpecific).Returns(() => true);
			mockProcessor2.Setup(x => x.LoadData(It.IsAny<string>(), It.IsAny<IReadOnlyCollection<IFileDetails>>(), errorCollector)).Callback<string, IReadOnlyCollection<IFileDetails>, StringBuilder>((chapter, files, sb) => processCounter(files));
			mockProcessor2.Setup(x => x.Models).Returns(() => new List<ITariffModel>());
			mockProcessor2.Setup(x => x.IsChapterSpecific).Returns(() => true);
			mockProcessor3.Setup(x => x.LoadData(It.IsAny<string>(), It.IsAny<IReadOnlyCollection<IFileDetails>>(), errorCollector)).Callback<string, IReadOnlyCollection<IFileDetails>, StringBuilder>((chapter, files, sb) => processCounter(files));
			mockProcessor3.Setup(x => x.Models).Returns(() => new List<ITariffModel>());
			mockProcessor3.Setup(x => x.IsChapterSpecific).Returns(() => false);

			processManager.TestProcessors = new[] { mockProcessor1.Object, mockProcessor2.Object, mockProcessor3.Object };
			processManager.RunProcess("", errorCollector);

			Assert.That(processCount.Keys.Count, Is.EqualTo(2), "2 Files should be processed");
			foreach (var v in processCount.Values)
			{
				Assert.That(v, Is.EqualTo(21), "Each input file processed 10 times (each chapter) by each processor (x2) + 1 Non-chapter cpecific processor");
			}
		}

		[Test]
		public void NoProcessor()
		{
			var processManager = new ProcessManagerForTest();
			var processed = false;

			processManager.TestProcessors = null;
			var errorCollector = new StringBuilder();

			processManager.RunProcess("", errorCollector);

			Assert.That(processed, Is.EqualTo(false), "No processing should have happened");
			Assert.That(errorCollector.ToString(), Is.EqualTo("No processors available for processing"));
		}

		[Test]
		public void ThrowsInvalidSourceDateException()
		{
			var processManager = new ProcessManagerForTest();
			var errorCollector = new StringBuilder();

			var mockFileManager = new Mock<IFileManager>();
			mockFileManager.Setup(x => x.GetFiles()).Returns(new List<IFileDetails>());
			processManager.TestFileManager = mockFileManager.Object;

			var mockProcessor1 = new Mock<IProcessor>();
			mockProcessor1.Setup(x => x.LoadData(It.IsAny<string>(), It.IsAny<IReadOnlyCollection<IFileDetails>>(), errorCollector))
				.Callback<string, IReadOnlyCollection<IFileDetails>, StringBuilder>((chapter, files, sb) => sb.Append("random message"));
			mockProcessor1.Setup(x => x.Models).Returns(() => new List<ITariffModel>());
			mockProcessor1.Setup(x => x.IsChapterSpecific).Returns(() => false);
			var mockProcessor2 = new Mock<IProcessor>();
			mockProcessor2.Setup(x => x.LoadData(It.IsAny<string>(), It.IsAny<IReadOnlyCollection<IFileDetails>>(), errorCollector))
				.Callback<string, IReadOnlyCollection<IFileDetails>, StringBuilder>((chapter, files, sb) => sb.Append("{InvalidSourceData:messagetext}"));
			mockProcessor2.Setup(x => x.Models).Returns(() => new List<ITariffModel>());
			mockProcessor2.Setup(x => x.IsChapterSpecific).Returns(() => false);
			var mockProcessor3 = new Mock<IProcessor>();
			mockProcessor3.Setup(x => x.LoadData(It.IsAny<string>(), It.IsAny<IReadOnlyCollection<IFileDetails>>(), errorCollector))
				.Callback<string, IReadOnlyCollection<IFileDetails>, StringBuilder>((chapter, files, sb) => sb.Append("{InvalidSourceData:a different error}"));
			mockProcessor3.Setup(x => x.Models).Returns(() => new List<ITariffModel>());
			mockProcessor3.Setup(x => x.IsChapterSpecific).Returns(() => false);

			processManager.TestProcessors = new[] { mockProcessor1.Object, mockProcessor2.Object, mockProcessor3.Object };
			var ex = Assert.Throws<InvalidSourceDataException>(() => processManager.RunProcess("", errorCollector));
			var message = ex.ToString();
			Assert.That(message, Does.Contain("messagetext"));
			Assert.That(message, Does.Contain("One or more errors were identified with the source data, this may need to be raised with the data provider. Please assign this issue to the customs team."));
			Assert.That(message, Does.Not.Contain("random message"));
			Assert.That(message, Does.Not.Contain("InvalidSourceData:"));
			Assert.That(message, Does.Not.Contain("{"));
			Assert.That(message, Does.Not.Contain("}"));
			Assert.That(message, Does.Contain("InvalidSourceDataException:"));
			Assert.That(message, Does.Contain("a different error"));
		}

		[Test]
		public void SourceDataProvider()
		{
			var processManager = new ProcessManagerForTest();
			Assert.That(processManager.SourceDataProvider, Is.EqualTo("the data provider"));
		}

		[Test]
		public void Team()
		{
			var processManager = new ProcessManagerForTest();
			Assert.That(processManager.Team, Is.EqualTo("the customs team"));
		}

		[Test]
		public void TestReferenceDataLoading()
		{
			var processManager = new ProcessManagerForTest();
			var errorCollector = new StringBuilder();
			var mockFileManager = new Mock<IFileManager>();
			var mockLoader1 = new Mock<ILoader>();
			var mockLoader2 = new Mock<ILoader>();
			var mockProcessor1 = new Mock<IProcessor>();
			var mockProcessor2 = new Mock<IProcessor>();

			mockFileManager.Setup(x => x.GetFiles()).Returns(new List<IFileDetails>
			{
				new FileDetails { Filename = "File1", Content = new ContentDetails { ExecutionDate = DateTime.Now } }
			});

			processManager.TestFileManager = mockFileManager.Object;

			var results = new List<string>();
			Action<List<ITariffModel>, string> action = (List<ITariffModel> refData, string chap) =>
			{
				var curData = $"Chapter: {chap} Refs: {string.Join(", ", refData.Cast<CommonData>().Select(x => x.Key))}";
				results.Add(curData);
			};

			mockLoader1.Setup(x => x.LoadData(It.IsAny<IReadOnlyCollection<IFileDetails>>(), errorCollector)).Returns(new List<ITariffModel>() { new LoadDataOne() { Key = "L1-1" }, new LoadDataOne() { Key = "L1-2" } });
			mockLoader2.Setup(x => x.LoadData(It.IsAny<IReadOnlyCollection<IFileDetails>>(), errorCollector)).Returns(new List<ITariffModel>() { new LoadDataOne() { Key = "L2-1" }, new LoadDataOne() { Key = "L2-2" } });

			var allP1Data = new List<ITariffModel>() { new ProcessDataOne() { Chapter = "01", Key = "P1-1" }, new ProcessDataOne() { Chapter = "12", Key = "P1-2" } };
			var allP2Data = new List<ITariffModel>() { new ProcessDataTwo() { Chapter = "11", Key = "P2-1" }, new ProcessDataTwo() { Chapter = "22", Key = "P2-2" } };
			var p1Data = new List<ITariffModel>();

			mockProcessor1.Setup(x => x.LoadData(It.IsAny<string>(), It.IsAny<IReadOnlyCollection<IFileDetails>>(), errorCollector)).Callback<string, IReadOnlyCollection<IFileDetails>, StringBuilder>((chap, fd, sb) =>
			{
				p1Data = allP1Data.Where(x => x.IsInChapter(chap)).ToList();
			});
			mockProcessor1.Setup(x => x.Models).Returns(() => p1Data);
			mockProcessor1.Setup(x => x.UpdateModels(It.IsAny<string>(), It.IsAny<List<ITariffModel>>(), errorCollector)).Callback<string, List<ITariffModel>, StringBuilder>((chap, refData, sb) => action(refData, chap));
			mockProcessor1.Setup(x => x.ProcessChapter(It.IsAny<string>(), It.IsAny<string>())).Callback(() => { });
			mockProcessor1.Setup(x => x.IsChapterSpecific).Returns(() => true);

			var p2Data = new List<ITariffModel>();

			mockProcessor2.Setup(x => x.LoadData(It.IsAny<string>(), It.IsAny<IReadOnlyCollection<IFileDetails>>(), errorCollector)).Callback<string, IReadOnlyCollection<IFileDetails>, StringBuilder>((chap, fd, sb) =>
			{
				p1Data = allP2Data.Where(x => x.IsInChapter(chap)).ToList();
			});
			mockProcessor2.Setup(x => x.Models).Returns(() => p1Data);
			mockProcessor2.Setup(x => x.UpdateModels(It.IsAny<string>(), It.IsAny<List<ITariffModel>>(), errorCollector)).Callback<string, List<ITariffModel>, StringBuilder>((chap, refData, sb) => action(refData, chap));
			mockProcessor2.Setup(x => x.ProcessChapter(It.IsAny<string>(), It.IsAny<string>())).Callback(() => { });
			mockProcessor2.Setup(x => x.IsChapterSpecific).Returns(() => true);

			processManager.TestLoaders = new ILoader[] { mockLoader1.Object, mockLoader2.Object };
			processManager.TestProcessors = new IProcessor[] { mockProcessor1.Object, mockProcessor2.Object };

			processManager.RunProcess("", errorCollector);

			Assert.That(results.Count, Is.EqualTo(20));
			Assert.That(results[0], Is.EqualTo("Chapter: 0 Refs: L1-1, L1-2, L2-1, L2-2, P1-1"), "Result 0 - Processor 1");
			Assert.That(results[1], Is.EqualTo("Chapter: 0 Refs: L1-1, L1-2, L2-1, L2-2, P1-1"), "Result 1 - Processor 2");
			Assert.That(results[2], Is.EqualTo("Chapter: 1 Refs: L1-1, L1-2, L2-1, L2-2, P1-2, P2-1"), "Result 2 - Processor 1");
			Assert.That(results[3], Is.EqualTo("Chapter: 1 Refs: L1-1, L1-2, L2-1, L2-2, P1-2, P2-1"), "Result 3 - Processor 2");
			Assert.That(results[4], Is.EqualTo("Chapter: 2 Refs: L1-1, L1-2, L2-1, L2-2, P2-2"), "Result 4 - Processor 1");
			Assert.That(results[5], Is.EqualTo("Chapter: 2 Refs: L1-1, L1-2, L2-1, L2-2, P2-2"), "Result 5 - Processor 2");
			Assert.That(results[6], Is.EqualTo("Chapter: 3 Refs: L1-1, L1-2, L2-1, L2-2"), "Result 6 - Processor 1");
			Assert.That(results[7], Is.EqualTo("Chapter: 3 Refs: L1-1, L1-2, L2-1, L2-2"), "Result 7 - Processor 2");
		}

		[Test]
		public void TestCrossDependenDataProcessing()
		{
			var processManager = new ProcessManagerForTest();
			var errorCollector = new StringBuilder();
			var mockFileManager = new Mock<IFileManager>();
			var mockProcessor1 = new Mock<IProcessor>();
			var mockProcessor2 = new Mock<IProcessor>();

			mockFileManager.Setup(x => x.GetFiles()).Returns(new List<IFileDetails>
			{
				new FileDetails { Filename = "File1", Content = new ContentDetails { ExecutionDate = DateTime.Now } }
			});

			processManager.TestFileManager = mockFileManager.Object;

			var p1Data = new List<ITariffModel>();

			mockProcessor1.Setup(x => x.LoadData(It.IsAny<string>(), It.IsAny<IReadOnlyCollection<IFileDetails>>(), errorCollector)).Callback<string, IReadOnlyCollection<IFileDetails>, StringBuilder>((chap, fd, sb) =>
			{
				p1Data = new List<ITariffModel>() { new ProcessDataOne() { Chapter = "01", Key = "P1-1" }, new ProcessDataOne() { Chapter = "12", Key = "P1-2" } };
			});
			mockProcessor1.Setup(x => x.Models).Returns(() => p1Data);
			mockProcessor1.Setup(x => x.UpdateModels(It.IsAny<string>(), It.IsAny<List<ITariffModel>>(), errorCollector)).Callback<string, List<ITariffModel>, StringBuilder>((s, x, sb) =>
			{
				p1Data.ForEach(p =>
				{
					var p1 = (ProcessDataOne)p;
					p1.DataOne = $"U1-{p1.Key}";
				});
			});
			mockProcessor1.Setup(x => x.ProcessChapter(It.IsAny<string>(), It.IsAny<string>())).Callback(() => { });
			mockProcessor1.Setup(x => x.IsChapterSpecific).Returns(() => false);

			var p2Data = new List<ITariffModel>();

			mockProcessor2.Setup(x => x.LoadData(It.IsAny<string>(), It.IsAny<IReadOnlyCollection<IFileDetails>>(), errorCollector)).Callback<string, IReadOnlyCollection<IFileDetails>, StringBuilder>((chap, fd, sb) =>
			{
				p2Data = new List<ITariffModel>() { new ProcessDataTwo() { Chapter = "01", Key = "P2-1" }, new ProcessDataTwo() { Chapter = "12", Key = "P2-2" }, new ProcessDataTwo() { Chapter = "12", Key = "P2-3" } };
			});
			mockProcessor2.Setup(x => x.Models).Returns(() => p2Data);
			mockProcessor2.Setup(x => x.UpdateModels(It.IsAny<string>(), It.IsAny<List<ITariffModel>>(), errorCollector)).Callback<string, List<ITariffModel>, StringBuilder>((s, x, sb) =>
			{
				p2Data.ForEach(p =>
				{
					var p2 = (ProcessDataTwo)p;
					var match = x.OfType<ProcessDataOne>().FirstOrDefault(p1 => p1.Key.Substring(2, 2) == p2.Key.Substring(2, 2));
					p2.DataTwo = $"U2-{p2.Key}-" + (match?.DataOne ?? "Not Found");
				});
			});
			mockProcessor2.Setup(x => x.ProcessChapter(It.IsAny<string>(), It.IsAny<string>())).Callback(() => { });
			mockProcessor2.Setup(x => x.IsChapterSpecific).Returns(() => false);

			processManager.TestProcessors = new IProcessor[] { mockProcessor1.Object, mockProcessor2.Object };

			processManager.RunProcess("", errorCollector);

			var pd1 = p1Data.Cast<ProcessDataOne>().ToList();
			var pd2 = p2Data.Cast<ProcessDataTwo>().ToList();

			Assert.That(pd1.Count, Is.EqualTo(2));
			Assert.That(pd2.Count, Is.EqualTo(3));

			Assert.That(pd1[0].DataOne, Is.EqualTo("U1-P1-1"));
			Assert.That(pd1[1].DataOne, Is.EqualTo("U1-P1-2"));

			Assert.That(pd2[0].DataTwo, Is.EqualTo("U2-P2-1-U1-P1-1"));
			Assert.That(pd2[1].DataTwo, Is.EqualTo("U2-P2-2-U1-P1-2"));
			Assert.That(pd2[2].DataTwo, Is.EqualTo("U2-P2-3-Not Found"));
		}

		[Test]
		public void Chapters()
		{
			var processManager = new ProcessManagerForTest();
			var chapters = string.Join(",", processManager.ChaptersExposed);
			Assert.That(chapters, Is.EqualTo("0,1,2,3,4,5,6,7,8,9"));
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

	}
}

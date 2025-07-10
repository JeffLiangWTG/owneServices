using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;
using CargoWise.RefDbRepo.SharedReferenceData.Tests;
using NUnit.Framework;
using static CargoWise.RefDbRepo.SharedReferenceData.Services.Common.CommonHelper;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Loaders.Tests
{
	internal interface ITestLoader<T> where T : ITariffModel
	{
		T ConvertXElementToModel(XElement element);
		string XmlElementName { get; }

		List<T> LoadData(List<FileDetails> files, StringBuilder errorCollector);
	}

	internal abstract class BaseLoaderTests<T> where T : ITariffModel, new()
	{
		[Test]
		public void ConvertXmlElementToModel()
		{
			var element = TestHelper.GetXmlElement(TestLoader.XmlElementName, "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.UT_LoaderTests_001.xml");
			var model = TestLoader.ConvertXElementToModel(element);

			ConvertXmlElementToModelAsserts(model);
		}

		[Test]
		public void ModelIsChapterSpecific()
		{
			var model = GetModel();

			Assert.That(model.IsChapterSpecific, Is.EqualTo(false));
		}

		[Test]
		public void ModelIsInChapter()
		{
			var model = GetModel();

			Assert.That(model.IsInChapter(""), Is.EqualTo(true));
			Assert.That(model.IsInChapter("0"), Is.EqualTo(true));
			Assert.That(model.IsInChapter("ANY"), Is.EqualTo(true));
		}

		[Test]
		public void ModelIsValid()
		{
			var model = GetModel();
			var errorCollector = new StringBuilder();
			var source = "LoaderUnitTest";

			ModelIsValidCore(model, errorCollector, source);
		}

		[Test]
		public void LoadData()
		{
			var filePath1 = Path.Combine(ContentFolder, "UT_LoaderTests_001.xml");
			var filePath2 = Path.Combine(ContentFolder, "UT_LoaderTests_002.xml");
			TestHelper.SimulateDownload(filePath1, "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.UT_LoaderTests_001.xml");
			TestHelper.SimulateDownload(filePath2, "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.UT_LoaderTests_002.xml");

			var file1 = new FileDetails { Content = new ContentDetails { ExecutionDate = new DateTime(2019, 10, 03, 14, 15, 16) }, Filename = filePath1 };
			var file2 = new FileDetails { Content = new ContentDetails { ExecutionDate = new DateTime(2019, 10, 03, 14, 15, 16) }, Filename = filePath2 };
			var files = new List<FileDetails>() { file1, file2 };

			var errorCollector = new StringBuilder();

			var models = TestLoader.LoadData(files, errorCollector);
			LoadDataAsserts(models, errorCollector.ToString());
		}

		public virtual T GetModel() => new T { OpType = MetaInfoOpTypes.Created, OpDate = DateTime.Now };
		public abstract void ConvertXmlElementToModelAsserts(T model);
		public abstract void ModelIsValidCore(T model, StringBuilder errorCollector, string source);
		public abstract void LoadDataAsserts(List<T> models, string errors);

		public ITestLoader<T> TestLoader => testLoader ?? (testLoader = CreateTestLoader());
		ITestLoader<T> testLoader;
		public abstract ITestLoader<T> CreateTestLoader();

		#region Setup
		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(TempFolder);
		}

		[SetUp]
		public void Setup()
		{
			ContentFolder = Path.Combine(TempFolder, Path.GetRandomFileName());
			Directory.CreateDirectory(ContentFolder);
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			if (Directory.Exists(TempFolder))
			{
				Directory.Delete(TempFolder, true);
			}
		}
		string ContentFolder;
		string TempFolder;
		#endregion
	}
}

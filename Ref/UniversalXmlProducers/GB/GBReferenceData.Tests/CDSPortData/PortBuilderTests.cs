using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.GBReferenceData.Business.CDSPortData;
using CargoWise.RefDbRepo.GBReferenceData.Services.CDSPortData.Config;
using CargoWise.RefDbRepo.GBReferenceData.Services.CDSPortData.Models;
using NUnit.Framework;
using static CargoWise.RefDbRepo.GBReferenceData.Tests.CDSPortData.TestHelperClasses;
using static CargoWise.RefDbRepo.GBReferenceData.Tests.CDSPortData.TestHelperClasses.VirtualRefDataLoader;

namespace CargoWise.RefDbRepo.GBReferenceData.Tests.CDSPortData
{
	[TestFixture]
	class PortBuilderTests
	{
		[Test]
		public void FilePrefix()
		{
			Assert.That(builder.FilePrefix, Is.EqualTo("RefCusCodeList"));
		}

		[Test]
		public void ConvertModelToRefModel()
		{
			var portData = new PortData { Code = "GBXX12345678", Description = "Unit Test", Source = new CDSPortSource { Code = "ABC", AttributeName = "ATTN" } };

			var models = builder.ConvertToRefModels(new[] { portData }, CreateCCSUKLocationTask()).ToList();

			Assert.That(models, Is.Not.Null);
			Assert.That(models.Count, Is.EqualTo(1));
			Assert.That(models[0].ZZD_Code, Is.EqualTo("12345678"));
			Assert.That(models[0].ZZD_Description, Is.EqualTo("Unit Test"));

			Assert.That(models[0].RefCusCodeListAttributes, Is.Not.Null);
			Assert.That(models[0].RefCusCodeListAttributes.Count, Is.EqualTo(2));
			Assert.That(models[0].RefCusCodeListAttributes[0].ZZE_ZXE_NKName, Is.EqualTo("FACTY"));
			Assert.That(models[0].RefCusCodeListAttributes[0].ZZE_Value, Is.EqualTo("XX"));
			Assert.That(models[0].RefCusCodeListAttributes[1].ZZE_ZXE_NKName, Is.EqualTo("ATTN"));
			Assert.That(models[0].RefCusCodeListAttributes[1].ZZE_Value, Is.EqualTo("ABC"));
		}

		[Test]
		public void CreateCCSUKAttributes()
		{
			var source = new CDSPortSource { Code = "ABC", AttributeName = "", CheckCCSUKLocation = true };
			var portData = new List<PortData>
			{
				new PortData { Source = source, Code = "GBXXQWEABCDEFAAA", Description = "ABC DEF AAA" },
				new PortData { Source = source, Code = "GBXXASDABCDEFBBB", Description = "ABC DEF BBB" },
				new PortData { Source = source, Code = "GBXXZXCABCDEF", Description = "ABC DEF" },

				new PortData { Source = source, Code = "GBXXQWEXYZDEFAAA", Description = "XYZ DEF AAA" },
				new PortData { Source = source, Code = "GBXXASDXYZDEFBBB", Description = "XYZ DEF BBB" },
				new PortData { Source = source, Code = "GBXXZXCXYZDEFCUK", Description = "XYZ DEF CUK" },

				new PortData { Source = source, Code = "GBXXPOIABCXYZ", Description = "ABC XYZ" }
			};

			CreateRefCusCodeListDelegate testData = () => {
				return new List<RefCusCodeList>
				{
					CreateRefCusCodeList("ABCDEF"),
					CreateRefCusCodeList("XYZDEF")
				};
			};

			var models = builder.ConvertToRefModels(portData, CreateCCSUKLocationTask(testData)).ToList();

			Assert.That(models, Is.Not.Null);
			Assert.That(models.Count, Is.EqualTo(7));

			Assert.That(models.First(x => x.ZZD_Code == "QWEABCDEFAAA").RefCusCodeListAttributes.Any(x => x.ZZE_ZXE_NKName == "CCSUK" && x.ZZE_Value == "ABCDEF"), Is.EqualTo(true));
			Assert.That(models.First(x => x.ZZD_Code == "ASDABCDEFBBB").RefCusCodeListAttributes.Any(x => x.ZZE_ZXE_NKName == "CCSUK" && x.ZZE_Value == "ABCDEF"), Is.EqualTo(true));
			Assert.That(models.First(x => x.ZZD_Code == "ZXCABCDEF").RefCusCodeListAttributes.Any(x => x.ZZE_ZXE_NKName == "CCSUK" && x.ZZE_Value == "ABCDEF"), Is.EqualTo(true));

			Assert.That(models.First(x => x.ZZD_Code == "QWEXYZDEFAAA").RefCusCodeListAttributes.Any(x => x.ZZE_ZXE_NKName == "CCSUK"), Is.EqualTo(false));
			Assert.That(models.First(x => x.ZZD_Code == "ASDXYZDEFBBB").RefCusCodeListAttributes.Any(x => x.ZZE_ZXE_NKName == "CCSUK"), Is.EqualTo(false));
			Assert.That(models.First(x => x.ZZD_Code == "ZXCXYZDEFCUK").RefCusCodeListAttributes.Any(x => x.ZZE_ZXE_NKName == "CCSUK" && x.ZZE_Value == "XYZDEF"), Is.EqualTo(true));

			Assert.That(models.First(x => x.ZZD_Code == "POIABCXYZ").RefCusCodeListAttributes.Any(x => x.ZZE_ZXE_NKName == "CCSUK"), Is.EqualTo(false));
		}

		[Test]
		public void ROROAttributeRules()
		{
			var sourceR = new CDSPortSource { Code = "MIXED", AttributeName = "RORO", AdditionalInfoColumn = 1 };
			var sourceX = new CDSPortSource { Code = "MIXED", AttributeName = "X", AdditionalInfoColumn = 1 };
			var portData = new List<PortData>
			{
				new PortData { Source = sourceR, Code = "GBRRAAAA", Description = "RORO No AddInfo", AdditionalInfo = string.Empty },
				new PortData { Source = sourceR, Code = "GBRRBBBB", Description = "RORO Some AddInfo", AdditionalInfo = "Something" },
				new PortData { Source = sourceR, Code = "GBRRCCCC", Description = "RORO Dash-nothing", AdditionalInfo = "-" },
				new PortData { Source = sourceR, Code = "GBRRDDDD", Description = "RORO Dash2-nothing", AdditionalInfo = new string(new char[] { (char)8212 }) },
				new PortData { Source = sourceX, Code = "GBXXEEEE", Description = "X No AddInfo", AdditionalInfo = string.Empty },
				new PortData { Source = sourceX, Code = "GBXXFFFF", Description = "X Some AddInfo", AdditionalInfo = "Something" },
				new PortData { Source = sourceX, Code = "GBXXGGGG", Description = "X Dash-nothing", AdditionalInfo = "-" },
				new PortData { Source = sourceX, Code = "GBXXHHHH", Description = "X Dash2-nothing", AdditionalInfo = new string(new char[] { (char)8212 }) },
			};

			var models = builder.ConvertToRefModels(portData, CreateCCSUKLocationTask()).ToList();

			Assert.That(models, Is.Not.Null);
			Assert.That(models.Count, Is.EqualTo(8));

			var model = models.First(x => x.ZZD_Code == "AAAA");
			Assert.That(model.RefCusCodeListAttributes.FirstOrDefault(x => x.ZZE_ZXE_NKName == "RORO" && string.IsNullOrEmpty(x.ZZE_Value)), Is.Not.Null);
			model = models.First(x => x.ZZD_Code == "BBBB");
			Assert.That(model.RefCusCodeListAttributes.FirstOrDefault(x => x.ZZE_ZXE_NKName == "RORO" && x.ZZE_Value == "MIXED"), Is.Not.Null);
			model = models.First(x => x.ZZD_Code == "CCCC");
			Assert.That(model.RefCusCodeListAttributes.FirstOrDefault(x => x.ZZE_ZXE_NKName == "RORO" && string.IsNullOrEmpty(x.ZZE_Value)), Is.Not.Null);
			model = models.First(x => x.ZZD_Code == "DDDD");
			Assert.That(model.RefCusCodeListAttributes.FirstOrDefault(x => x.ZZE_ZXE_NKName == "RORO" && string.IsNullOrEmpty(x.ZZE_Value)), Is.Not.Null);
			model = models.First(x => x.ZZD_Code == "EEEE");
			Assert.That(model.RefCusCodeListAttributes.FirstOrDefault(x => x.ZZE_ZXE_NKName == "X" && x.ZZE_Value == "MIXED"), Is.Not.Null);
			model = models.First(x => x.ZZD_Code == "FFFF");
			Assert.That(model.RefCusCodeListAttributes.FirstOrDefault(x => x.ZZE_ZXE_NKName == "X" && x.ZZE_Value == "MIXED"), Is.Not.Null);
			model = models.First(x => x.ZZD_Code == "GGGG");
			Assert.That(model.RefCusCodeListAttributes.FirstOrDefault(x => x.ZZE_ZXE_NKName == "X" && x.ZZE_Value == "MIXED"), Is.Not.Null);
			model = models.First(x => x.ZZD_Code == "HHHH");
			Assert.That(model.RefCusCodeListAttributes.FirstOrDefault(x => x.ZZE_ZXE_NKName == "X" && x.ZZE_Value == "MIXED"), Is.Not.Null);
		}

		[Test]
		public void XmlWriterConfig()
		{
			var config = builder.XmlWriterConfiguration();

			Assert.That(config, Is.Not.Null);
			var refType = typeof(RefCusCodeList);
			var entityConfig = config.GetConfiguration(refType);

			Assert.That(entityConfig, Is.Not.Null);
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusCodeList.ZZD_Code))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusCodeList.ZZD_ZZK_NKCodeType))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusCodeList.ZZD_Description))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusCodeList.ZZD_ZZZ_NKDataGrouping))));

			refType = typeof(RefCusCodeListAttribute);
			entityConfig = config.GetConfiguration(refType);

			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusCodeListAttribute.ZZE_Value))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusCodeListAttribute.ZZE_ZXE_NKName))));
		}

		[Test]
		public void UniqueId()
		{
			var model = new RefCusCodeList { ZZD_Code = "12345", ZZD_ZZK_NKCodeType = "ABCDE" };

			Assert.That(builder.UniqueId(model), Is.EqualTo("12345_ABCDE"));
		}

		[Test]
		public void DuplicateErrors()
		{
			var model = new RefCusCodeList { ZZD_Code = "12345", ZZD_ZZK_NKCodeType = "ABCDE", ZZD_Description = "Something Wrong" };

			builder.DuplicateError(model, "1234");
			Assert.That(errorCollector.ToString(), Contains.Substring("RefCusCodeList duplicate exists. Key: '1234' Description: Something Wrong"));
		}

		[Test]
		public void IsValid()
		{
			var model = new RefCusCodeList();

			Assert.That(builder.IsValid(model), Is.False);
			Assert.That(errorCollector.ToString(), Contains.Substring("RefCusCodeList validation error")
														.And.Contains("ZZD_Code is required")
														.And.Contains("ZZD_Description is required"));

			errorCollector.Clear();
			model.ZZD_Code = "1234";
			Assert.That(builder.IsValid(model), Is.False);
			Assert.That(errorCollector.ToString(), Does.Not.Contain("ZZD_Code is required"));

			errorCollector.Clear();
			model.ZZD_Description = "Unit Test";
			Assert.That(builder.IsValid(model ), Is.True);
			Assert.That(errorCollector.ToString(), Does.Not.Contain("ZZD_Description is required"));
		}

		[Test]
		public void BuildXmlFile()
		{
			var models = new List<PortData>
			{
				new PortData { Code = "GBABMFNCYPORT", Description = "My Fancy Port", Source = new CDSPortSource { Name = "UT001", Code = "ABC", AttributeName = "TYPE", CheckCCSUKLocation = true } },
				new PortData { Code = "GBCDQWERTYUIO", Description = "Top Keyboard Row", Source = new CDSPortSource { Code = "ABC", AttributeName = "ZXCV" } },
				new PortData { Code = "GBEFZAQXSWCDE", Description = "No Source Code", Source = new CDSPortSource { Code = "", AttributeName = "TYPE" } },
				new PortData { Code = "GBCDQWERTYUIO", Description = "Duplicate Test", Source = new CDSPortSource { Code = "ABC", AttributeName = "ZXCV" } },
				new PortData { Code = "", Description = "Invalid1", Source = new CDSPortSource { Code = "ABC", AttributeName = "ZXCV" } },
				new PortData { Code = "INVALID2", Description = "", Source = new CDSPortSource { Code = "ABC", AttributeName = "ZXCV" } }
			};

			CreateRefCusCodeListDelegate testData = () => {	return new List<RefCusCodeList>	{ CreateRefCusCodeList("CYPORT") };	};

			var errorCollector = new StringBuilder();
			var builder = new PortBuilderTester(errorCollector);

			var publicationDate = new DateTime(2019, 11, 03, 13, 14, 15, 678, DateTimeKind.Utc);

			builder.BuildXml(publicationDate, models, TempFolder, CreateCCSUKLocationTask(testData));

			var fileName = Path.Combine(TempFolder, builder.GetOutputFileName("UT001", publicationDate));
			Assert.That(File.Exists(fileName));

			var xml = File.ReadAllText(fileName);
			var expectedXml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.CDSPortData.TestFiles.Output.RefCusCodeList_001.xml");
			Assert.That(xml, Is.EqualTo(expectedXml));
		}

		Task<IEnumerable<CCSUKLocation>> CreateCCSUKLocationTask(CreateRefCusCodeListDelegate testDataCreator = null)
		{
			var refDataLoader = new VirtualRefDataLoader();

			if (testDataCreator != null)
			{
				refDataLoader.RefCusCodeListData = testDataCreator;
			}

			var ccsuk = new CCSUKLocationLoaderTester(refDataLoader);

			return ccsuk.GetLocations();
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			errorCollector = new StringBuilder();
			builder = new PortBuilderTester(errorCollector);
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			if (Directory.Exists(TempFolder))
			{
				Directory.Delete(TempFolder, true);
			}
		}

		[SetUp]
		public void Setup()
		{
			errorCollector.Clear();
		}

		PortBuilderTester builder;
		StringBuilder errorCollector;
		string TempFolder;
	}

	internal class PortBuilderTester : PortBuilder
	{
		public PortBuilderTester(StringBuilder errorCollector) : base(errorCollector)
		{
		}

		public new string FilePrefix => PortBuilder.FilePrefix;
		public new string XMLWriterDataSource => PortBuilder.XMLWriterDataSource;
		public new IEnumerable<RefCusCodeList> ConvertToRefModels(IEnumerable<PortData> data, Task<IEnumerable<CCSUKLocation>> ccsukData) => PortBuilder.ConvertToRefModels(data, ccsukData);
		public new XmlWriterConfiguration XmlWriterConfiguration() => PortBuilder.XmlWriterConfiguration();
		public new bool IsValid(RefCusCodeList refModel) => base.IsValid(refModel);
		public new void DuplicateError(RefCusCodeList refModel, string uniqueId) => base.DuplicateError(refModel, uniqueId);
		public new string UniqueId(RefCusCodeList refModel) => PortBuilder.UniqueId(refModel);
		public new string GetOutputFileName(string sourceCode, DateTime publicationDate) => PortBuilder.GetOutputFileName(sourceCode, publicationDate);
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.ZAReferenceData.Business.Carrier;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Carriers;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;
using CargoWise.RefDbRepo.ZAReferenceData.Tests.Helpers;
using CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Common;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.Business.Carrier
{
	[TestFixture]
	class VesselBuilderTests
	{
		[Test]
		public void XmlWriterConfig()
		{
			var builder = new VesselBuilderForTest(null, null);
			var config = builder.GetXmlWriterConfiguration();

			Assert.That(config, Is.Not.Null);
			var refType = typeof(RefVesselZZ);
			var vesselConfig = config.GetConfiguration(typeof(RefVesselZZ));

			Assert.That(vesselConfig, Is.Not.Null);
			Assert.That(vesselConfig.IsIncluded(refType.GetProperty(nameof(RefVesselZZ.ZZO_Code))));
			Assert.That(vesselConfig.IsIncluded(refType.GetProperty(nameof(RefVesselZZ.ZZO_RadioCallSign))));
			Assert.That(vesselConfig.IsIncluded(refType.GetProperty(nameof(RefVesselZZ.ZZO_ZZZ_NKDataGrouping))));
			Assert.That(vesselConfig.IsIncluded(refType.GetProperty(nameof(RefVesselZZ.ZZO_VesselType))));

			BuilderTestHelper.AssertKeySets(nameof(RefVesselZZ), vesselConfig.GetKeySets(),
				nameof(RefVesselZZ.ZZO_Code),
				nameof(RefVesselZZ.ZZO_RadioCallSign),
				nameof(RefVesselZZ.ZZO_ZZZ_NKDataGrouping));
		}

		[Test]
		public void FilePrefix()
		{
			var builder = new VesselBuilderForTest(null, null);
			Assert.That(builder.FilePrefix, Is.EqualTo("ZA_RefVesselZZ"));
		}

		[Test]
		public void ConvertToRefModels()
		{
			var data = new BaseData<VesselData>
			{
				PublicationDate = DateTime.Now,
				Data = new List<VesselData>
				{
					new VesselData("Radio1", "Vessel Name 1", new CarrierData("Carrier1", "Carrier Name 1", false, true, new string[] { })),
					new VesselData("Radio2WhichIsAlsoLongerThanExpected", "Vessel Name 2 With a really long name which should get cut-off at some point", new CarrierData("Carrier2", "Carrier Name 2", false, true, new string[] { }))
				}
			};

			var builder = new VesselBuilderForTest(data, logger);
			var models = builder.ConvertToRefModels();

			Assert.That(models, Is.Not.Null);
			Assert.That(models.Count, Is.EqualTo(2));

			Assert.That(models[0].ZZO_Code, Is.EqualTo("Vessel Name 1"));
			Assert.That(models[0].ZZO_RadioCallSign, Is.EqualTo("Radio1"));

			Assert.That(models[1].ZZO_Code, Is.EqualTo("Vessel Name 2 With a really long na"));
			Assert.That(models[1].ZZO_RadioCallSign, Is.EqualTo("Radio2Whic"));
		}

		[Test]
		public void OutputFile()
		{
			var data = new BaseData<VesselData>
			{
				PublicationDate = new DateTime(2022, 10, 21, 12, 13, 14),
				Data = new List<VesselData>
				{
					new VesselData("Radio1", "Vessel Name 1", new CarrierData("Carrier1", "Carrier Name 1", false, true, new string[] { })),
					new VesselData("Radio2", "Vessel Name 2", new CarrierData("Carrier2", "Carrier Name 2", false, true, new string[] { }))
				}
			};

			var createdDate = new DateTime(2022, 10, 25, 17, 18, 19);

			var builder = new VesselBuilderForTest(data, logger);
			builder.CreateXmlFile(TempFolder, createdDate);

			var fileName = Path.Combine(TempFolder, builder.OutputFilename);
			Assert.That(File.Exists(fileName));

			var xml = File.ReadAllText(fileName);

			var expectedXml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.ZAReferenceData.Tests.Business.Carrier.TestFiles.Output.ZARefVesselZZ_001.xml");

			Assert.That(xml, Is.EqualTo(expectedXml));

			Assert.That(logger.InfoString, Contains.Substring($"Created {fileName}"));
		}

		[Test]
		public void DataSource()
		{
			var builder = new VesselBuilderForTest(null, null);
			Assert.That(builder.DataSource, Is.EqualTo("ZAVessels"));
		}

		[Test]
		public void UpdateType()
		{
			var builder = new VesselBuilderForTest(null, null);
			Assert.That(builder.UpdateType, Is.EqualTo(Common.UniversalXmlWriter.UpdateType.Full));
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

	class VesselBuilderForTest : VesselBuilder
	{
		public VesselBuilderForTest(BaseData<VesselData> sourceData, ILogger logger) : base(sourceData, logger)
		{
		}

		public new XmlWriterConfiguration GetXmlWriterConfiguration() => base.GetXmlWriterConfiguration();
		public new string FilePrefix => base.FilePrefix;
		public new string OutputFilename => base.OutputFilename;
		public new string DataSource => base.DataSource;
		public new UpdateType UpdateType => base.UpdateType;
		public new List<RefVesselZZ> ConvertToRefModels() => base.ConvertToRefModels();
	}
}

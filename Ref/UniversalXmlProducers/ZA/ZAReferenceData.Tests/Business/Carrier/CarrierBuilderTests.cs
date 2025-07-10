using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
	class CarrierBuilderTests
	{
		[Test]
		public void XmlWriterConfig()
		{
			var builder = new CarrierBuilderForTest(null, null);
			var config = builder.GetXmlWriterConfiguration();

			Assert.That(config, Is.Not.Null);
			var refType = typeof(RefCarrierCode);
			var carrierConfig = config.GetConfiguration(refType);

			Assert.That(carrierConfig, Is.Not.Null);
			Assert.That(carrierConfig.IsIncluded(refType.GetProperty(nameof(RefCarrierCode.ZZ4_Code))));
			Assert.That(carrierConfig.IsIncluded(refType.GetProperty(nameof(RefCarrierCode.ZZ4_Description))));
			Assert.That(carrierConfig.IsIncluded(refType.GetProperty(nameof(RefCarrierCode.ZZ4_ZZZ_NKDataGrouping))));
			Assert.That(carrierConfig.IsIncluded(refType.GetProperty(nameof(RefCarrierCode.ZZ4_IsSea))));
			Assert.That(carrierConfig.IsIncluded(refType.GetProperty(nameof(RefCarrierCode.ZZ4_IsAir))));
			Assert.That(carrierConfig.IsIncluded(refType.GetProperty(nameof(RefCarrierCode.RefCarrierCodeAttributes))));
			Assert.That(carrierConfig.IsIncluded(refType.GetProperty(nameof(RefCarrierCode.RefCarrierVesselPivots))));

			BuilderTestHelper.AssertKeySets(nameof(RefCarrierCode), carrierConfig.GetKeySets(),
				nameof(RefCarrierCode.ZZ4_Code),
				nameof(RefCarrierCode.ZZ4_ZZZ_NKDataGrouping));

			refType = typeof(RefCarrierCodeAttribute);
			var carrierAttributeConfig = config.GetConfiguration(refType);

			Assert.That(carrierAttributeConfig, Is.Not.Null);
			Assert.That(carrierAttributeConfig.IsIncluded(refType.GetProperty(nameof(RefCarrierCodeAttribute.ZZG_Name))));
			Assert.That(carrierAttributeConfig.IsIncluded(refType.GetProperty(nameof(RefCarrierCodeAttribute.ZZG_Value))));

			BuilderTestHelper.AssertKeySets(nameof(RefCarrierCodeAttribute), carrierAttributeConfig.GetKeySets(),
				nameof(RefCarrierCodeAttribute.ZZG_Name),
				nameof(RefCarrierCodeAttribute.ZZG_Value));

			refType = typeof(RefCarrierVesselPivot);
			var carrierVesselPivoteConfig = config.GetConfiguration(refType);

			Assert.That(carrierVesselPivoteConfig, Is.Not.Null);
			Assert.That(carrierVesselPivoteConfig.IsIncluded(refType.GetProperty(nameof(RefCarrierVesselPivot.ZZQ_ZZO_ZZZ_NKDataGrouping))));
			Assert.That(carrierVesselPivoteConfig.IsIncluded(refType.GetProperty(nameof(RefCarrierVesselPivot.ZZQ_ZZO_NKCode))));
			Assert.That(carrierVesselPivoteConfig.IsIncluded(refType.GetProperty(nameof(RefCarrierVesselPivot.ZZQ_ZZO_NKRadioCallSign))));

			BuilderTestHelper.AssertKeySets(nameof(RefCarrierVesselPivot), carrierVesselPivoteConfig.GetKeySets(),
				nameof(RefCarrierVesselPivot.ZZQ_ZZO_ZZZ_NKDataGrouping),
				nameof(RefCarrierVesselPivot.ZZQ_ZZO_NKCode),
				nameof(RefCarrierVesselPivot.ZZQ_ZZO_NKRadioCallSign));
		}

		[Test]
		public void FilePrefix()
		{
			var builder = new CarrierBuilderForTest(null, null);
			Assert.That(builder.FilePrefix, Is.EqualTo("ZA_RefCarrierCode"));
		}

		[Test]
		public void ConvertToRefModels()
		{
			var data = new CarrierVesselData
			{
				PublicationDate = DateTime.Now,
				Data = new List<CarrierData>
				{
					new CarrierData("C0001", "Carrier 1", true, false, new [] { Constants.Attributes.Master }),
					new CarrierData("C0002", "Carrier 2", false, true, new [] { Constants.Attributes.CargoCarrier }),
					new CarrierData("C0003", "Carrier 3", true, false, new [] { Constants.Attributes.CargoCarrier })
				},
			};

			var vesselData = new List<VesselData>
			{
				new VesselData("Radio1", "Vessel 1", data.Data[0]),
				new VesselData("Radio2", "Vessel 2", data.Data[0]),
				new VesselData("Radio3", "Vessel 2", data.Data[0]),
				new VesselData("Radio4", "Vessel 4", new CarrierData("C0003", "Carrier 3", false, true, new [] { Constants.Attributes.Master })),
				new VesselData("Radio5", "Vessel 5", new CarrierData("C0004", "Carrier 4", false, true, new [] { Constants.Attributes.Master })),
				new VesselData("Radio6", "Vessel 6", new CarrierData("C0004", "Carrier 4", false, true, new [] { Constants.Attributes.Master }))
			};

			data.Vessels = vesselData;

			var builder = new CarrierBuilderForTest(data, logger);
			var models = builder.ConvertToRefModels();

			Assert.That(models, Is.Not.Null);
			Assert.That(models.Count, Is.EqualTo(4));
			Assert.That(models[0].ZZ4_Code, Is.EqualTo("C0001"));
			Assert.That(models[0].ZZ4_Description, Is.EqualTo("Carrier 1"));
			Assert.That(models[0].ZZ4_IsAir, Is.EqualTo(true));
			Assert.That(models[0].ZZ4_IsSea, Is.EqualTo(false));
			Assert.That(models[0].RefCarrierCodeAttributes.Count, Is.EqualTo(1));
			Assert.That(models[0].RefCarrierVesselPivots.Count, Is.EqualTo(3));

			var attrib = models[0].RefCarrierCodeAttributes.FirstOrDefault();
			Assert.That(attrib.ZZG_Value, Is.EqualTo(Constants.Attributes.Master));
			Assert.That(attrib.ZZG_Name, Is.EqualTo(Constants.Attributes.Master));

			var vesselPivot = models[0].RefCarrierVesselPivots.FirstOrDefault();
			Assert.That(vesselPivot.ZZQ_ZZO_NKRadioCallSign, Is.EqualTo("Radio1"));
			Assert.That(vesselPivot.ZZQ_ZZO_NKCode, Is.EqualTo("Vessel 1"));

			Assert.That(models[1].ZZ4_Code, Is.EqualTo("C0002"));
			Assert.That(models[1].ZZ4_Description, Is.EqualTo("Carrier 2"));
			Assert.That(models[1].ZZ4_IsAir, Is.EqualTo(false));
			Assert.That(models[1].ZZ4_IsSea, Is.EqualTo(true));

			Assert.That(models[2].ZZ4_Code, Is.EqualTo("C0003"));
			Assert.That(models[2].ZZ4_IsAir, Is.EqualTo(true));
			Assert.That(models[2].ZZ4_IsSea, Is.EqualTo(true));
			Assert.That(models[2].RefCarrierCodeAttributes.Count, Is.EqualTo(2));

			Assert.That(models[3].ZZ4_Code, Is.EqualTo("C0004"));
			Assert.That(models[3].ZZ4_Description, Is.EqualTo("Carrier 4"));
			Assert.That(models[3].RefCarrierVesselPivots.Count, Is.EqualTo(2));
		}

		[Test]
		public void OutputFile()
		{
			var data = new CarrierVesselData
			{
				PublicationDate = new DateTime(2022, 10, 21, 12, 13, 14),
				Data = new List<CarrierData>
				{
					new CarrierData("C0001", "Carrier 1", true, false, new [] { Constants.Attributes.Master }),
					new CarrierData("C0002", "Carrier 2", false, true, new [] { Constants.Attributes.CargoCarrier })
				},
			};

			var vesselData = new List<VesselData>
			{
				new VesselData("Radio1", "Vessel 1", data.Data[0]),
				new VesselData("Radio2", "Vessel 2", data.Data[0])
			};

			data.Vessels = vesselData;

			var createdDate = new DateTime(2022, 10, 25, 17, 18, 19);

			var builder = new CarrierBuilderForTest(data, logger);
			builder.CreateXmlFile(TempFolder, createdDate);

			var fileName = Path.Combine(TempFolder, builder.OutputFilename);
			Assert.That(File.Exists(fileName));

			var xml = File.ReadAllText(fileName);

			var expectedXml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.ZAReferenceData.Tests.Business.Carrier.TestFiles.Output.ZACarrierCode_001.xml");

			Assert.That(xml, Is.EqualTo(expectedXml));

			Assert.That(logger.InfoString, Contains.Substring($"Created {fileName}"));
		}
		
		[Test]
		public void DataSource()
		{
			var builder = new CarrierBuilderForTest(null, null);
			Assert.That(builder.DataSource, Is.EqualTo("ZACarriers"));
		}

		[Test]
		public void UpdateType()
		{
			var builder = new CarrierBuilderForTest(null, null);
			Assert.That(builder.UpdateType, Is.EqualTo(Common.UniversalXmlWriter.UpdateType.Full));
		}

		[Test]
		public void XmlWriterDependencies_NoVessels()
		{
			var data = new CarrierVesselData
			{
				PublicationDate = new DateTime(2022, 10, 21, 12, 13, 14),
				Data = new List<CarrierData>
				{
					new CarrierData("C0001", "Carrier 1", true, false, new[] { Constants.Attributes.Master }),
					new CarrierData("C0002", "Carrier 2", false, true, new[] { Constants.Attributes.CargoCarrier })
				},
				Vessels = new List<VesselData>()
			};

			var createdDate = new DateTime(2022, 10, 25, 17, 18, 19);
			var builder = new CarrierBuilderForTest(data, logger);

			var dependencies = builder.GetDependencies(createdDate).ToList();
			Assert.That(dependencies.Count, Is.EqualTo(0));
		}

		[Test]
		public void XmlWriterDependencies()
		{
			var data = new CarrierVesselData
			{
				PublicationDate = new DateTime(2022, 10, 21, 12, 13, 14),
				Data = new List<CarrierData>
				{
					new CarrierData("C0001", "Carrier 1", true, false, new [] { Constants.Attributes.Master }),
					new CarrierData("C0002", "Carrier 2", false, true, new [] { Constants.Attributes.CargoCarrier })
				},
			};

			var vesselData = new List<VesselData>
			{
				new VesselData("Radio1", "Vessel 1", data.Data[0]),
				new VesselData("Radio2", "Vessel 2", data.Data[0])
			};

			data.Vessels = vesselData;

			var createdDate = new DateTime(2022, 10, 25, 17, 18, 19);

			var builder = new CarrierBuilderForTest(data, logger);
			var dependencies = builder.GetDependencies(createdDate).ToList();
			Assert.That(dependencies.Count, Is.EqualTo(1));

			Assert.That(dependencies[0].DataSource, Is.EqualTo("ZAVessels"));
			Assert.That(dependencies[0].PublicationTime, Is.EqualTo(createdDate));
			Assert.That(dependencies[0].DependencyType, Is.EqualTo(DependencyType.Preferred));
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

	class CarrierBuilderForTest : CarrierBuilder
	{
		public CarrierBuilderForTest(CarrierVesselData sourceData, ILogger logger) : base(sourceData, logger)
		{
		}

		public new XmlWriterConfiguration GetXmlWriterConfiguration() => base.GetXmlWriterConfiguration();
		public new string FilePrefix => base.FilePrefix;
		public new string OutputFilename => base.OutputFilename;
		public new string DataSource => base.DataSource;
		public new UpdateType UpdateType => base.UpdateType;
		public new List<RefCarrierCode> ConvertToRefModels() => base.ConvertToRefModels();

		public new IEnumerable<Dependency> GetDependencies(DateTime publicationDate) => base.GetDependencies(publicationDate);
	}
}

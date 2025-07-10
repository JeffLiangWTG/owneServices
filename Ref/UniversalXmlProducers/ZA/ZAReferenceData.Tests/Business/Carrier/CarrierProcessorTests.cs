using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.ZAReferenceData.Business.Carrier;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Carriers;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;
using CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.Business.Carrier
{
	[TestFixture]
	class CarrierProcessorTests
	{
		[Test]
		public void Run()
		{
			var folder = TempFolder;
			var logger = new TestLogger();
			var processor = new CarrierProcessor(logger, CreateMockCarrierLoader(), CreateMockVesselLoader());

			processor.Run(folder);

			var files = Directory.GetFiles(folder, "*.xml");
			Assert.That(files.Length, Is.EqualTo(2));
			Assert.That(files.Any(x => x.Contains("ZA_RefVesselZZ")), Is.EqualTo(true));
			Assert.That(files.Any(x => x.Contains("ZA_RefCarrierCode")), Is.EqualTo(true));
			Assert.That(logger.InfoString, Contains.Substring("Loading vessels"));
			Assert.That(logger.InfoString, Contains.Substring("Loading carriers"));
			Assert.That(logger.InfoString, Contains.Substring("Creating vessel xml file"));
			Assert.That(logger.InfoString, Contains.Substring("Creating carrier xml file"));
		}

		ILoader<CarrierData> CreateMockCarrierLoader()
		{
			var mock = new Mock<ILoader<CarrierData>>();

			var data = new BaseData<CarrierData>
			{
				PublicationDate = new DateTime(2022, 10, 21, 12, 13, 14),
				Data = new List<CarrierData>
				{
					new CarrierData("C0001", "Carrier 1", true, false, new [] { Constants.Attributes.Master }),
					new CarrierData("C0002", "Carrier 2", false, true, new [] { Constants.Attributes.CargoCarrier })
				}
			};

			mock.Setup(x => x.LoadData()).Returns(data);

			return mock.Object;
		}

		ILoader<VesselData> CreateMockVesselLoader()
		{
			var mock = new Mock<ILoader<VesselData>>();

			var data = new BaseData<VesselData>
			{
				PublicationDate = new DateTime(2022, 10, 18, 19, 20, 21),
				Data = new List<VesselData>
				{
					new VesselData("Radio1", "Vessel 1", new CarrierData("C0001", "Carrier 1", false, true, new [] { Constants.Attributes.Master })),
					new VesselData("Radio2", "Vessel 2", new CarrierData("C0002", "Carrier 2", false, true, new [] { Constants.Attributes.Master }))					
				}
			};

			mock.Setup(x => x.LoadData()).Returns(data);

			return mock.Object;
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			if (!Directory.Exists(TempFolder))
			{
				Directory.CreateDirectory(TempFolder);
			}
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

using System;
using System.IO;
using CargoWise.RefDbRepo.IEReferenceData.Services;
using CargoWise.RefDbRepo.IEReferenceData.Tests;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.IEReferenceData.Tariffs.Tests
{
	[TestFixture]
	class ExciseDutyRatePageReaderTest
	{
		[Test]
		public void TestReadAll()
		{
			var result = ExciseDutyRatePageReader.ReadAll(TestConfig);
			Assert.AreEqual(54, result.Count);
		}

		[Test]
		public void TestReadAll_NoUpdate()
		{
			RuntimeDataRecorder.Write(RuntimeDataRecorder.Keys.ExciseDutyRatePublicateDate_Tobacco_Products, new DateTime(2024, 1, 1));
			RuntimeDataRecorder.Write(RuntimeDataRecorder.Keys.ExciseDutyRatePublicateDate_Mineral_Oil, new DateTime(2024, 1, 1));
			RuntimeDataRecorder.Write(RuntimeDataRecorder.Keys.ExciseDutyRatePublicateDate_Alcohol_Products, new DateTime(2024, 1, 1));

			var result = ExciseDutyRatePageReader.ReadAll(TestConfig);
			Assert.AreEqual(0, result.Count);
		}

		[Test]
		public void TestReadAll_InvalidPaths()
		{
			var configMock = TestHelper.GetBaseMock();
			var path = TestHelper.GetRunningDirectory();
			configMock.Setup(config => config.ExciseDuty_Url_Mineral_Oil).Returns(Path.Combine(path, "Mineral Oil Tax.html"));
			configMock.Setup(config => config.ExciseDuty_Url_Alcohol_Products).Returns(Path.Combine(path, "Alcohol Products Tax.html"));
			configMock.Setup(config => config.ExciseDuty_Url_Tobacco_Products).Returns(Path.Combine(path, "Tobacco Products Tax.html"));

			var result = ExciseDutyRatePageReader.ReadAll(configMock.Object);
			Assert.AreEqual(0, result.Count);
		}

		[SetUp]
		public void SetUp()
		{
			TestHelper.ClearRuntimeData();
		}

		[TearDown]
		public void TearDown()
		{
			TestHelper.ClearRuntimeData();
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			TestConfig = TestHelper.GetBaseMock().SetupTariffTestInputPaths().Object;
		}
		IApplicationConfig TestConfig;
	}
}

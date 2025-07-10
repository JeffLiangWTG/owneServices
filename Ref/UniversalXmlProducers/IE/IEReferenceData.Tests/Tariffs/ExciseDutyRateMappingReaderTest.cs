using CargoWise.RefDbRepo.IEReferenceData.Services;
using CargoWise.RefDbRepo.IEReferenceData.Tests;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.IEReferenceData.Tariffs.Tests
{
	[TestFixture]
	class ExciseDutyRateMappingReaderTest
	{
		[Test]
		public void TestReadAll()
		{
			var result = ExciseDutyRateMappingReader.ReadAll(TestConfig);
			Assert.AreEqual(67, result.Count);
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

using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.Testing
{
	sealed class MockCOSTOContainerTest : TestCaseWithFactory
	{
		public void TestEquipmentType()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport;
			AssertEquals("UL", ((ICOSTCOContainerInformation)new MockCOSTCOContainer(header)).EquipmentType);
			header.AMA_ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport;
			AssertEquals("UL", ((ICOSTCOContainerInformation)new MockCOSTCOContainer(header)).EquipmentType);
			header.AMA_ManifestType = ManifestTypeList.Codes.AirLoadDischarge;
			AssertEquals("UL", ((ICOSTCOContainerInformation)new MockCOSTCOContainer(header)).EquipmentType);
			header.AMA_ManifestType = ManifestTypeList.Codes.VesselOutturnReport;
			AssertEquals("BB", ((ICOSTCOContainerInformation)new MockCOSTCOContainer(header)).EquipmentType);
		}

		public void TestContainerNumber()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals("1", ((ICOSTCOContainerInformation)new MockCOSTCOContainer(header)).ContainerNumber);
		}

		public void TestContainerSize()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals(ZString.Empty, ((ICOSTCOContainerInformation)new MockCOSTCOContainer(header)).ContainerSize);
		}

		public void TestContainerStatusLandedPurpose()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_Nature = NatureList.Codes.Import23;
			AssertEquals("3", ((ICOSTCOContainerInformation)new MockCOSTCOContainer(header)).ContainerStatusLandedPurpose);
			header.AMA_Nature = NatureList.Codes.Export22;
			AssertEquals("2", ((ICOSTCOContainerInformation)new MockCOSTCOContainer(header)).ContainerStatusLandedPurpose);
			header.AMA_Nature = NatureList.Codes.Transhipment28;
			AssertEquals("6", ((ICOSTCOContainerInformation)new MockCOSTCOContainer(header)).ContainerStatusLandedPurpose);
			header.AMA_Nature = NatureList.Codes.Transit24;
			AssertEquals("1", ((ICOSTCOContainerInformation)new MockCOSTCOContainer(header)).ContainerStatusLandedPurpose);
		}

		public void TestServiceType()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals(ZString.Empty, ((ICOSTCOContainerInformation)new MockCOSTCOContainer(header)).ServiceType);
		}

		public void TestDateUnpacked()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.FullyLoadedUnloadedDate = new ZDateTime(2017, 11, 2);
			AssertEquals(new ZDateTime(2017, 11, 2), ((ICOSTCOContainerInformation)new MockCOSTCOContainer(header)).DateUnpacked);
		}

		public void TestDateTimeFullyUnloaded()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.FullyLoadedUnloadedDate = new ZDateTime(2017, 11, 2);
			AssertEquals(new ZDateTime(2017, 11, 2), ((ICOSTCOContainerInformation)new MockCOSTCOContainer(header)).DateTimeFullyUnloaded);
		}

		public void TestSealNumber()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals("", ((ICOSTCOContainerInformation)new MockCOSTCOContainer(header)).SealNumber);
			header.AMA_ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport;
			AssertEquals("", ((ICOSTCOContainerInformation)new MockCOSTCOContainer(header)).SealNumber);
		}

		public void TestSealingParty()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals(ZString.Empty, ((ICOSTCOContainerInformation)new MockCOSTCOContainer(header)).SealingParty);
		}

		public void TestSealStatus()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals(ZString.Empty, ((ICOSTCOContainerInformation)new MockCOSTCOContainer(header)).SealStatus);
		}
	}
}

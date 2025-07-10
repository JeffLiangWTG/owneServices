using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.Testing
{
	sealed class COSTCOContainerTest : TestCaseWithFactory
	{
		public void TestEquipmentType()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();
			header.AMA_ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport;
			AssertEquals("CN", ((ICOSTCOContainerInformation)new COSTCOContainer(container)).EquipmentType);
		}

		public void TestContainerNumber()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();
			container.ACN_ContainerNumber = "123";
			AssertEquals("123", ((ICOSTCOContainerInformation)new COSTCOContainer(container)).ContainerNumber);
		}

		public void TestContainerSize()
		{
			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_Code = "VWG";
			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();
			container.ACN_RC_ContainerType = refContainer.PK;
			AssertEquals("VWG", ((ICOSTCOContainerInformation)new COSTCOContainer(container)).ContainerSize);
		}

		public void TestGetContainerStatusLandedPurpose()
		{
			AssertEquals("3", COSTCOContainer.GetContainerStatusLandedPurpose(NatureList.Codes.Import23));
			AssertEquals("2", COSTCOContainer.GetContainerStatusLandedPurpose(NatureList.Codes.Export22));
			AssertEquals("6", COSTCOContainer.GetContainerStatusLandedPurpose(NatureList.Codes.Transhipment28));
			AssertEquals("1", COSTCOContainer.GetContainerStatusLandedPurpose(NatureList.Codes.Transit24));
		}

		public void TestContainerStatusLandedPurpose()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();
			header.AMA_Nature = NatureList.Codes.Import23;
			AssertEquals("3", ((ICOSTCOContainerInformation)new COSTCOContainer(container)).ContainerStatusLandedPurpose);
			header.AMA_Nature = NatureList.Codes.Export22;
			AssertEquals("2", ((ICOSTCOContainerInformation)new COSTCOContainer(container)).ContainerStatusLandedPurpose);
			header.AMA_Nature = NatureList.Codes.Transhipment28;
			AssertEquals("6", ((ICOSTCOContainerInformation)new COSTCOContainer(container)).ContainerStatusLandedPurpose);
			header.AMA_Nature = NatureList.Codes.Transit24;
			AssertEquals("1", ((ICOSTCOContainerInformation)new COSTCOContainer(container)).ContainerStatusLandedPurpose);
		}

		public void TestServiceType()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();
			container.ACN_EmptyFullIndicator = EmptyFullList.Codes.EmptyContainer;
			AssertEquals("4", ((ICOSTCOContainerInformation)new COSTCOContainer(container)).ServiceType);
			container.ACN_EmptyFullIndicator = EmptyFullList.Codes.FullContainerLoad;
			AssertEquals("5", ((ICOSTCOContainerInformation)new COSTCOContainer(container)).ServiceType);
			container.ACN_EmptyFullIndicator = EmptyFullList.Codes.LessThanFullContainerLoad;
			AssertEquals("7", ((ICOSTCOContainerInformation)new COSTCOContainer(container)).ServiceType);
		}

		public void TestDateUnpacked()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.FullyLoadedUnloadedDate = new ZDateTime(2017, 11, 2);
			var container = header.Containers.AddNew();
			AssertEquals(new ZDateTime(2017, 11, 2), ((ICOSTCOContainerInformation)new COSTCOContainer(container)).DateUnpacked);
			container.ContUnpackTime = new ZDateTime(2018, 3, 21);
			AssertEquals(new ZDateTime(2018, 3, 21), ((ICOSTCOContainerInformation)new COSTCOContainer(container)).DateUnpacked);
		}

		public void TestDateTimeFullyUnloaded()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.FullyLoadedUnloadedDate = new ZDateTime(2017, 11, 2);
			var container = header.Containers.AddNew();
			AssertEquals(new ZDateTime(2017, 11, 2), ((ICOSTCOContainerInformation)new COSTCOContainer(container)).DateTimeFullyUnloaded);
		}

		public void TestSealNumber()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();
			AssertEquals("NO SEAL NO", ((ICOSTCOContainerInformation)new COSTCOContainer(container)).SealNumber);
			container.ACN_Seal1 = "VWG";
			AssertEquals("VWG", ((ICOSTCOContainerInformation)new COSTCOContainer(container)).SealNumber);
			header.AMA_ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport;
			AssertEquals("", ((ICOSTCOContainerInformation)new COSTCOContainer(container)).SealNumber);
		}

		public void TestSealingParty()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();
			container.ACN_SealingPartyType = SealTypeList.Codes.AgentForwarder;
			AssertEquals("AB", ((ICOSTCOContainerInformation)new COSTCOContainer(container)).SealingParty);
			container.ACN_SealingPartyType = SealTypeList.Codes.Carrier;
			AssertEquals("CA", ((ICOSTCOContainerInformation)new COSTCOContainer(container)).SealingParty);
			container.ACN_SealingPartyType = SealTypeList.Codes.Customs;
			AssertEquals("CU", ((ICOSTCOContainerInformation)new COSTCOContainer(container)).SealingParty);
			container.ACN_SealingPartyType = SealTypeList.Codes.ExporterShipper;
			AssertEquals("SH", ((ICOSTCOContainerInformation)new COSTCOContainer(container)).SealingParty);
			container.ACN_SealingPartyType = SealTypeList.Codes.TerminalOperator;
			AssertEquals("TO", ((ICOSTCOContainerInformation)new COSTCOContainer(container)).SealingParty);
		}

		public void TestSealStatus()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();
			AssertEquals("2", ((ICOSTCOContainerInformation)new COSTCOContainer(container)).SealStatus);
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.ContainerPK = container.PK;
			pack.Outturn.C5_SealIntactIndicator = true;
			AssertEquals("1", ((ICOSTCOContainerInformation)new COSTCOContainer(container)).SealStatus);
		}
	}
}

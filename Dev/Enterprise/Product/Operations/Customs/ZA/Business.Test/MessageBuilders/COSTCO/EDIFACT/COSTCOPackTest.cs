using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.Testing
{
	sealed class COSTCOPackTest : TestCaseWithFactory
	{
		public void TestGoodsLineNumber()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			AssertEquals("1", ((ICOSTCOPackLineInformation)new COSTCOPack(pack, 0)).GoodsLineNumber);
		}

		public void TestNumberOfPackages()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.APA_PackQty = 2;
			AssertEquals(2, ((ICOSTCOPackLineInformation)new COSTCOPack(pack, 0)).NumberOfPackages);
			header.AMA_ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport;
			AssertEquals(2, ((ICOSTCOPackLineInformation)new COSTCOPack(pack, 0)).NumberOfPackages);
		}

		public void TestTypeOfPackages()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.APA_PackUQ = "VW";
			header.AMA_ContainerMode = Core.Constants.ContainerModes.Containerised;
			AssertEquals("VW", ((ICOSTCOPackLineInformation)new COSTCOPack(pack, 0)).TypeOfPackages);
			header.AMA_ContainerMode = Core.Constants.ContainerModes.BreakBulk;
			AssertEquals("VW", ((ICOSTCOPackLineInformation)new COSTCOPack(pack, 0)).TypeOfPackages);
			header.AMA_ContainerMode = Core.Constants.ContainerModes.Liquid;
			AssertEquals("VW", ((ICOSTCOPackLineInformation)new COSTCOPack(pack, 0)).TypeOfPackages);
			header.AMA_ContainerMode = Core.Constants.ContainerModes.Bulk;
			AssertEquals("VW", ((ICOSTCOPackLineInformation)new COSTCOPack(pack, 0)).TypeOfPackages);
		}

		public void TestCargoTypeIndicator()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.APA_PackUQ = "VW";
			pack.Outturn.C5_CargoType = "12";
			AssertEquals("12", ((ICOSTCOPackLineInformation)new COSTCOPack(pack, 0)).CargoTypeIndicator);
		}

		public void TestNumberOfPackagesPackedUnpacked()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.Outturn.C5_PackagesOutturned = 14;
			AssertEquals(14, ((ICOSTCOPackLineInformation)new COSTCOPack(pack, 0)).NumberOfPackagesPackedUnpacked);
		}

		public void TestPackageCondition()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.Outturn.C5_PackageCondition = "123";
			AssertEquals("123", ((ICOSTCOPackLineInformation)new COSTCOPack(pack, 0)).PackageCondition);
		}

		public void TestConditionDescription()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.Outturn.PackCondDesc = "123";
			AssertEquals("123", ((ICOSTCOPackLineInformation)new COSTCOPack(pack, 0)).ConditionDescription);
		}

		public void TestContentsFoundToBe()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.Outturn.C5_GoodsDescription = "123";
			AssertEquals("123", ((ICOSTCOPackLineInformation)new COSTCOPack(pack, 0)).ContentsFoundToBe);
		}

		public void TestExcessShortIndicator()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.Outturn.ExcessShortInd = ExcessShortIndicatorList.Codes.Excess;
			AssertEquals("1", ((ICOSTCOPackLineInformation)new COSTCOPack(pack, 0)).ExcessShortIndicator);
			pack.Outturn.ExcessShortInd = ExcessShortIndicatorList.Codes.Short;
			AssertEquals("2", ((ICOSTCOPackLineInformation)new COSTCOPack(pack, 0)).ExcessShortIndicator);
			pack.Outturn.ExcessShortInd = ExcessShortIndicatorList.Codes.None;
			AssertEquals("3", ((ICOSTCOPackLineInformation)new COSTCOPack(pack, 0)).ExcessShortIndicator);
			pack.Outturn.ExcessShortInd = ExcessShortIndicatorList.Codes.Unknown;
			AssertEquals("4", ((ICOSTCOPackLineInformation)new COSTCOPack(pack, 0)).ExcessShortIndicator);
			pack.Outturn.ExcessShortInd = ExcessShortIndicatorList.Codes.PartShipment;
			AssertEquals("5", ((ICOSTCOPackLineInformation)new COSTCOPack(pack, 0)).ExcessShortIndicator);
			pack.Outturn.ExcessShortInd = ExcessShortIndicatorList.Codes.SplitStorage;
			AssertEquals("6", ((ICOSTCOPackLineInformation)new COSTCOPack(pack, 0)).ExcessShortIndicator);
			header.AMA_ManifestType = ManifestTypeList.Codes.VesselOutturnReport;
			AssertEquals("4", ((ICOSTCOPackLineInformation)new COSTCOPack(pack, 0)).ExcessShortIndicator);
			header.AMA_ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport;
			AssertEquals("4", ((ICOSTCOPackLineInformation)new COSTCOPack(pack, 0)).ExcessShortIndicator);
		}

		public void TestContentsShouldToBe()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.Outturn.ContShouldBe = "123";
			AssertEquals("123", ((ICOSTCOPackLineInformation)new COSTCOPack(pack, 0)).ContentsShouldToBe);
		}

		public void TestDescriptionOfGoods()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.APA_GoodsDescription = "123";
			AssertEquals("123", ((ICOSTCOPackLineInformation)new COSTCOPack(pack, 0)).DescriptionOfGoods);
		}

		public void TestGrossWeightInKilograms()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.APA_Weight = 1000;
			pack.APA_WeightUQ = "G";
			AssertEquals(1M, ((ICOSTCOPackLineInformation)new COSTCOPack(pack, 0)).GrossWeightInKilograms);
		}

		public void TestGrossWeightFoundInKilograms()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.Outturn.C5_WeightOutturned = 1000;
			pack.Outturn.C5_WeightOutturnedUQ = "G";
			AssertEquals(1M, ((ICOSTCOPackLineInformation)new COSTCOPack(pack, 0)).GrossWeightFoundInKilograms);
		}

		public void TestVolumeInLitres()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.APA_Volume = 1;
			pack.APA_VolumeUQ = "M3";
			AssertEquals(1000M, ((ICOSTCOPackLineInformation)new COSTCOPack(pack, 0)).VolumeInLitres);
		}

		public void TestVolumeOutturnedInLitres()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.Outturn.C5_VolumeOutturned = 1;
			pack.Outturn.C5_VolumeOutturnedUQ = "M3";
			AssertEquals(1000M, ((ICOSTCOPackLineInformation)new COSTCOPack(pack, 0)).VolumeOutturnedInLitres);
		}

		public void TestMarksAndNumbers()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.APA_MarksAndNumbers = "123";
			AssertEquals("123", ((ICOSTCOPackLineInformation)new COSTCOPack(pack, 0)).MarksAndNumbers);
		}

		public void TestContainerNumber()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();
			container.ACN_ContainerNumber = "RRR";
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.ContainerPK = container.PK;
			AssertEquals("RRR", ((ICOSTCOPackLineInformation)new COSTCOPack(pack, 0)).ContainerNumber);
			header.AMA_ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport;
			AssertEquals("RRR", ((ICOSTCOPackLineInformation)new COSTCOPack(pack, 0)).ContainerNumber);
			pack.ContainerPK = ZGuid.Empty;
			AssertEquals("1", ((ICOSTCOPackLineInformation)new COSTCOPack(pack, 0)).ContainerNumber);
		}

		public void TestContainerPackageContent()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.Outturn.C5_PackagesOutturned = 1;
			AssertEquals(1, ((ICOSTCOPackLineInformation)new COSTCOPack(pack, 0)).ContainerPackageContent);
		}
	}
}

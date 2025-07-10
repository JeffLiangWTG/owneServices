using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Messaging.CUSCAR;

namespace Enterprise.Customs.ZA.Manifest.Business.Testing
{
	class AsycudaPackValidationTest : BusinessObjectValidationTestCase
	{
		public void TestPackQtyAndUQ()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.APA_PackQty = 0;
			AssertHasMessageErrorContaining(pack.APA_PackQtyInfo, "cannot be zero");
			pack.APA_PackQty = 20;
			AssertNoMessageErrorContaining(pack.APA_PackQtyInfo, "cannot be zero");
		}

		public void TestWeight()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.APA_WeightUQ = Core.Constants.Weight.MetricCarat;
			pack.APA_Weight = 0;
			AssertHasMessageErrorContaining(pack.APA_WeightInfo, "cannot be zero");
			pack.APA_Weight = 20;
			AssertNoMessageErrorContaining(pack.APA_WeightInfo, "cannot be zero");
		}

		public void TestWeightUQ()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.APA_Weight = 20;
			pack.APA_WeightUQ = ZString.Empty;
			pack.Validation.ValidateAPA_WeightUQ();
			AssertHasMessageErrorContaining(pack.APA_WeightUQInfo, "You have not entered a Weight Unit.");
			pack.APA_WeightUQ = Core.Constants.Weight.MetricCarat;
			AssertNoMessageErrorContaining(pack.APA_WeightUQInfo, "You have not entered a Weight Unit.");
			pack.APA_Weight = 0;
			AssertNoMessageErrorContaining(pack.APA_WeightUQInfo, "You have not entered a Weight Unit.");
		}

		public void TestValidationDoesntOccurForSingaporeExclusively()
		{
			var headerZA = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			var billZA = headerZA.Bills.AddNew();
			var contZA = headerZA.Containers.AddNew();
			var packZA = billZA.Packs.AddNew();
			packZA.ContainerPK = contZA.PK;
			packZA.APA_WeightUQ = Core.Constants.Weight.Decitons;
			packZA.APA_Weight = 0;
			AssertHasMessageErrorContaining(packZA.APA_WeightInfo, "cannot be zero");
			packZA.APA_Weight = 10;
			AssertNoMessageErrorContaining(packZA.APA_WeightInfo, "cannot be zero");
			packZA.APA_VolumeUQ = Core.Constants.Volume.CubicCentimeters;
			packZA.APA_Volume = 0;
			AssertHasMessageErrorContaining(packZA.APA_VolumeInfo, "cannot be zero");
			packZA.APA_Volume = 10;
			AssertNoMessageErrorContaining(packZA.APA_VolumeInfo, "cannot be zero");
			var headerSG = (ASYCUDA.Business.AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			var billSG = headerSG.Bills.AddNew();
			var contSG = headerSG.Containers.AddNew();
			var packSG = billSG.Packs.AddNew();
			packSG.ContainerPK = contSG.PK;
			packSG.APA_Weight = 0;
			packSG.APA_Volume = 0;
			packSG.APA_WeightUQ = Core.Constants.Weight.Grams;
			packSG.APA_VolumeUQ = Core.Constants.Volume.CubicDecimetres;
			AssertNoMessageErrorContaining(packSG.APA_WeightInfo, "cannot be zero");
			AssertNoMessageErrorContaining(packSG.APA_VolumeInfo, "cannot be zero");
		}

		public void TestCheckContainerPK()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = nameof(ManifestDocumentType.BBB);
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.Validation.ValidateContainerPK();
			AssertNoMessageErrorContaining(pack.ContainerPKInfo, "entered");
			header.AMA_ManifestType = nameof(ManifestDocumentType.COM);
			pack.Validation.ValidateContainerPK();
			AssertHasMessageErrorContaining(pack.ContainerPKInfo, "entered");
			var container = header.Containers.AddNew();
			pack.ContainerPK = container.PK;
			AssertNoMessageErrorContaining(pack.ContainerPKInfo, "entered");
		}
	}
}

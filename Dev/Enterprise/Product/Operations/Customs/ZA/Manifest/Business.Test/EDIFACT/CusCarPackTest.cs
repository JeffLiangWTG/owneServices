using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.ZA.Manifest.Business.EDIFACT.Testing
{
	class CusCarPackTest : TestCaseWithFactory
	{
		public void TestICusCarPackage_GrossVolume()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var carPack = new CusCarPack(pack);
			pack.APA_VolumeUQ = Core.Constants.Volume.Litre;
			pack.APA_Volume = 12.22M;
			AssertEquals(12.22M, ((ICusCarPackage)carPack).GrossVolume);
			pack.APA_VolumeUQ = Core.Constants.Volume.CubicMetres;
			pack.APA_Volume = 12.22M;
			AssertEquals(12M, ((ICusCarPackage)carPack).GrossVolume);
			pack.APA_Volume = 12.52M;
			AssertEquals(13M, ((ICusCarPackage)carPack).GrossVolume);
		}

		public void TestICusCarPackage_GrossVolumeUnitCode()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var carPack = new CusCarPack(pack);
			AssertEquals("MTQ", ((ICusCarPackage)carPack).GrossVolumeUnitCode);
			pack.APA_VolumeUQ = Core.Constants.Volume.Litre;
			AssertEquals("LTR", ((ICusCarPackage)carPack).GrossVolumeUnitCode);
		}

		public void TestICusCarPackage_UNDGNumber()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var carPack = new CusCarPack(pack);
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_Code = "1112C";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var undg = pack.UNDGs.FirstItemForBinding[0];
			undg.DI_DG = subs.PK;
			undg.LinkDefault(subs);
			AssertEquals("1112", ((ICusCarPackage)carPack).UNDGNumber);
		}

		public void TestCusCarPack_NullAsycudaPack()
		{
			AsycudaPack pack = null;
			AssertExceptionThrown<ArgumentNullException>(() => new CusCarPack(pack));
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			pack = bill.Packs.AddNew();
			AssertNoExceptionThrown(() => new CusCarPack(pack));
		}
	}
}

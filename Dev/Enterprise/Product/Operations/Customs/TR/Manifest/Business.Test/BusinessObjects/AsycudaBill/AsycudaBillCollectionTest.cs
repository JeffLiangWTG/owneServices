using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ManifestBase;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaBillCollection))]
	public class AsycudaBillCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(AsycudaBillCollection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			return header.Bills;
		}

		public void TestSetDefaultsForNewChild()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var collection = header.Bills;
			var child0 = collection.AddNew();
			AssertEquals(Core.Constants.ShipmentTypes.StandardHouse, child0.ABL_BolType);
			AssertEquals(Universal.CodeDescriptionPairLists.YesNoList.Codes.No, child0.ABL_SpecialCargoCode);
			header.AMA_ManifestType = TRManifestTypes.Codes.EMANIF;
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			var child1 = collection.AddNew();
			AssertEquals(CusEntryNumberTypes.Turkey.PRV, child1.CustomsEntryNumberType);
			header.AMA_ManifestType = TRManifestTypes.Codes.VARONC;
			var child2 = collection.AddNew();
			AssertEquals(CusEntryNumberTypes.Turkey.PRV, child2.CustomsEntryNumberType);
			header.AMA_ManifestType = TRManifestTypes.Codes.DIGITH;
			var child3 = collection.AddNew();
			AssertNotEquals(CusEntryNumberTypes.Turkey.PRV, child3.CustomsEntryNumberType);
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_ManifestType = TRManifestTypes.Codes.DENITH;
			var bill2 = collection.AddNew();
			AssertEquals("ABL_LocationInformation default value should be GEMİ", "GEMİ", bill2.ABL_LocationInformation);
			header.AMA_ManifestType = TRManifestTypes.Codes.DENIHR;
			var bill3 = collection.AddNew();
			AssertEquals("ABL_LocationInformation default value should be LİMAN", "LİMAN", bill3.ABL_LocationInformation);
			header.AMA_ManifestType = TRManifestTypes.Codes.CIKONC;
			var bill4 = collection.AddNew();
			AssertEquals("ABL_LocationInformation default value should be LİMAN", "LİMAN", bill4.ABL_LocationInformation);
			header.AMA_ManifestType = TRManifestTypes.Codes.DIGIHR;
			var bill5 = collection.AddNew();
			AssertEquals(ZString.Empty, bill5.ABL_LocationInformation);
		}
	}
}

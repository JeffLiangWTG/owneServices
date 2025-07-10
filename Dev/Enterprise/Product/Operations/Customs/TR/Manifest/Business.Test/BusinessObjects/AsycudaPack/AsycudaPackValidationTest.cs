using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	class AsycudaPackValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAPA_PackUQAndQty()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.PackageTypes, "PKG");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, RefCusCodeListTypes.Codes.PackageTypes, "YGT", "YGT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Turkey);
			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.APA_PackUQ = "XX";
			AssertHasMessageErrorContaining(pack.APA_PackUQInfo, "list");
			pack.APA_PackUQ = pack.Lookups.PackUQList[0].Code;
			AssertNoMessageErrorContaining("After set value, there should not be message error on APA_PackUQ", pack.APA_PackUQInfo, "list");
			AssertHasMessageErrorContaining(pack.APA_PackQtyInfo, "zero");
			pack.APA_PackQty = 20;
			AssertNoMessageErrorContaining("After set value, there should not be message error on APA_PackQty", pack.APA_PackQtyInfo, "zero");
		}

		public void TestCheckAPA_WeightUQ()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.APA_Weight = 20;
			pack.APA_WeightUQ = ZString.Empty;
			AssertHasMessageErrorContaining(pack.APA_WeightUQInfo, "You have not entered a Weight Unit.");
			pack.APA_WeightUQ = Core.Constants.Weight.MetricCarat;
			AssertNoMessageErrorContaining("After set value, there should not be message error on APA_WeightUQ", pack.APA_WeightUQInfo, "You have not entered a Weight Unit.");
			pack.APA_Weight = 0;
			AssertNoMessageErrorContaining("After set value, there should not be message error on APA_Weight", pack.APA_WeightUQInfo, "You have not entered a Weight Unit.");
		}

		public void TestCheckAPA_Weight()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.APA_WeightUQ = Core.Constants.Weight.MetricCarat;
			pack.APA_Weight = 0;
			AssertHasMessageErrorContaining(pack.APA_WeightInfo, "cannot be zero");
			pack.APA_Weight = 20;
			AssertNoMessageErrorContaining("After set value, there should not be message error on APA_Weight", pack.APA_WeightInfo, "cannot be zero");
		}

		public void TestCheckContainerNumber()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ContainerMode = Core.Constants.ContainerModes.Containerised;
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.ContainerPK = ZGuid.Empty;
			AssertHasMessageError(pack.ContainerPKInfo, "For containerized cargo, all packs must be linked to a container.");
			var cont = header.Containers.AddNew();
			pack.ContainerPK = cont.PK;
			AssertNoMessageErrorContaining("After set value, there should not be message error on ContainerPK", pack.ContainerPKInfo, "For containerized cargo, all packs must be linked to a container.");
			cont.ACN_ContainerNumber = "FCIU4372541";
			AssertNoMessageErrorContaining("After set value, there should not be message error on ACN_ContainerNumber", cont.ACN_ContainerNumberInfo, "You have not entered a Container Number.");
			cont.ACN_ContainerNumber = "";
			AssertHasMessageError("After set value, there should not be message error on ACN_ContainerNumber", cont.ACN_ContainerNumberInfo, "You have not entered a Container Number.");
		}
	}
}

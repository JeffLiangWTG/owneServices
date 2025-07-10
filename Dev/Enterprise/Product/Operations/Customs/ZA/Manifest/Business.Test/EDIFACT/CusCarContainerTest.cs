using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Manifest.Business.EDIFACT.Testing
{
	sealed class CusCarContainerTest : TestCaseWithFactory
	{
		public void TestGetSeals()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.MSELT, MapDirectionList.Codes.BTH, "Description", false);
			helper.CreateCusMap(RefCusMapTypeList.Codes.MSELT, ASYCUDA.Business.SealTypeList.Codes.MechanicalSeal, "1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateCusMap(RefCusMapTypeList.Codes.MSELT, ASYCUDA.Business.SealTypeList.Codes.ElectronicSeal, "2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.STYPE, MapDirectionList.Codes.BTH, "Description", false);
			helper.CreateCusMap(RefCusMapTypeList.Codes.STYPE, Core.Constants.ContainerSealParties.Codes.CarrierShippingLine, "CA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateCusMap(RefCusMapTypeList.Codes.STYPE, Core.Constants.ContainerSealParties.Codes.Customs, "CU", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateCusMap(RefCusMapTypeList.Codes.STYPE, Core.Constants.ContainerSealParties.Codes.Terminal, "TO", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();
			var header = (AsycudaManifestHeader)ASYCUDA.Business.AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, ZaManifestTypes.Codes.ALM, ApplicationCodeTypeList.Codes.ShippingLine);
			var container = header.Containers.AddNew();
			container.ACN_Seal1 = "SEAL1";
			container.ACN_SealingPartyType = Core.Constants.ContainerSealParties.Codes.CarrierShippingLine;
			container.ACN_SealType1 = ASYCUDA.Business.SealTypeList.Codes.MechanicalSeal;
			container.ACN_Seal2 = "SEAL2";
			container.ACN_SealingPartyType2 = Core.Constants.ContainerSealParties.Codes.Customs;
			container.ACN_SealType2 = ASYCUDA.Business.SealTypeList.Codes.ElectronicSeal;
			container.ACN_Seal3 = "SEAL3";
			container.ACN_SealingPartyType3 = Core.Constants.ContainerSealParties.Codes.Terminal;
			Factory.Save();
			CombineAssertions(() =>
			{
				var ccc = new CusCarContainer(container);
				var seals = ccc.GetSeals(Core.Constants.CountryCodes.SouthAfrica);
				AssertEquals("3 CusCarSeals returned", 3, seals.Count());
				var seal1 = seals.Single(x => x.SealNumber == "SEAL1");
				AssertEquals("SEAL 1 Type", "1", seal1.SealType);
				AssertEquals("SEAL 1 Party", "CA", seal1.SealingParty);
				var seal2 = seals.Single(x => x.SealNumber == "SEAL2");
				AssertEquals("SEAL 2 Type", "2", seal2.SealType);
				AssertEquals("SEAL 2 Party", "CU", seal2.SealingParty);
				var seal3 = seals.Single(x => x.SealNumber == "SEAL3");
				AssertEquals("SEAL 3 Type", ZString.Empty, seal3.SealType);
				AssertEquals("SEAL 3 Party", "TO", seal3.SealingParty);
			});
		}

		public void TestIZACusCarContainer_GetContainerStatus()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var asycudaContainer = header.Containers.AddNew();
			IZACusCarContainer container = new CusCarContainer(asycudaContainer);
			header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			AssertEquals(ContainerStatus.Export, container.GetContainerStatus(Core.Constants.CountryCodes.SouthAfrica));
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			AssertEquals(ContainerStatus.Import, container.GetContainerStatus(Core.Constants.CountryCodes.SouthAfrica));
			header.AMA_Nature = ShipmentTypeList.Codes.Transhipment28;
			AssertEquals(ContainerStatus.Transhipment, container.GetContainerStatus(Core.Constants.CountryCodes.SouthAfrica));
			header.AMA_Nature = ShipmentTypeList.Codes.Transit24;
			AssertEquals(ContainerStatus.Transit, container.GetContainerStatus(Core.Constants.CountryCodes.SouthAfrica));
			header.AMA_Nature = ZaShipmentTypeList.Codes.MutualMultipleZzz;
			AssertEquals(ContainerStatus.None, container.GetContainerStatus(Core.Constants.CountryCodes.SouthAfrica));
		}

		public void TestIZACusCarContainer_GetLandedPurpose()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var asycudaContainer = header.Containers.AddNew();
			asycudaContainer.LandedPurpose = "AR1";
			AssertEquals("AR1", new CusCarContainer(asycudaContainer).GetLandedPurpose());
		}

		public void TestGrossMassInKilos()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var asycudaContainer = header.Containers.AddNew();
			asycudaContainer.ACN_GoodsWeightUQ = "KG";
			asycudaContainer.ACN_GoodsWeight = 2M;
			ICusCarContainer container = new CusCarContainer(asycudaContainer);
			AssertEquals(2M, container.GrossMassInKilos);
		}

		public void TestVerifiedGrossMassInKilos()
		{
			var refContainer = Factory.NewWithValidTestData<RefContainer>();
			refContainer.RC_TareWeight = 3M;
			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			var asycudaContainer = header.Containers.AddNew();
			asycudaContainer.ACN_GoodsWeightUQ = "KG";
			asycudaContainer.ACN_GoodsWeight = 2M;
			ICusCarContainer container = new CusCarContainer(asycudaContainer);
			AssertEquals(2M, container.VerifiedGrossMassInKilos);
			asycudaContainer.ACN_RC_ContainerType = refContainer.PK;
			AssertEquals(5M, container.VerifiedGrossMassInKilos);
		}

		public void TestGetEmptyFullServiceType()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var asycudaContainer = header.Containers.AddNew();
			ICusCarContainer container = new CusCarContainer(asycudaContainer);
			header.AMA_IsBuyersConsolidation = true;
			AssertEquals(EmptyFullServiceType.FullSingleConsignmentFCL, container.GetEmptyFullServiceType(""));
			header.AMA_IsBuyersConsolidation = false;
			AssertEquals(EmptyFullServiceType.None, container.GetEmptyFullServiceType(""));
			asycudaContainer.ACN_EmptyFullIndicator = "MT";
			AssertEquals(EmptyFullServiceType.Empty, container.GetEmptyFullServiceType(""));
			asycudaContainer.ACN_EmptyFullIndicator = "FCG";
			AssertEquals(EmptyFullServiceType.FullFCLGroupage, container.GetEmptyFullServiceType(""));
			asycudaContainer.ACN_EmptyFullIndicator = "LCL";
			AssertEquals(EmptyFullServiceType.FullMixedConsignmentLCL, container.GetEmptyFullServiceType(""));
			asycudaContainer.ACN_EmptyFullIndicator = "FCL";
			AssertEquals(EmptyFullServiceType.FullSingleConsignmentFCL, container.GetEmptyFullServiceType(""));
		}
	}
}

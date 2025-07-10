using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.ZA.Business.BaseAsycudaManifestHeaderCopyBO;
using BaseAsycudaBill = Enterprise.Customs.ManifestBase.AsycudaBill;
using BaseAsycudaContainer = Enterprise.Customs.ManifestBase.AsycudaContainer;
using BaseAsycudaManifestHeader = Enterprise.Customs.ManifestBase.AsycudaManifestHeader;
using BaseAsycudaPack = Enterprise.Customs.ManifestBase.AsycudaPack;
using BaseAsycudaPackageContainerLink = Enterprise.Customs.ManifestBase.AsycudaContainerBillOrPackageLink;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(BaseAsycudaManifestHeaderCopyBO))]
	sealed class BaseAsycudaManifestHeaderCopyBOTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCopyValuesFromGlobalManifest()
		{
			var header = CreateManifestHeader();

			var masterBill = CreateMasterBill();
			masterBill.ABL_AMA = header.PK;

			var otherBill = CreateBill();
			otherBill.ABL_AMA = header.PK;

			UpdateBillCountry(otherBill);

			var container1 = CreateContainer("CVB00001");
			container1.ACN_AMA_Manifest = header.PK;
			var container2 = CreateContainer("CVB00002");
			container2.ACN_AMA_Manifest = header.PK;

			var pack1 = CreatePack();
			pack1.APA_ABL_Bill = otherBill.PK;

			var pack2 = CreatePack();
			pack2.APA_ABL_Bill = otherBill.PK;

			var packContainerLink1 = Factory.New<BaseAsycudaPackageContainerLink>();
			packContainerLink1.APC_ACN_Container = container1.PK;
			packContainerLink1.APC_APA_Pack = pack1.PK;

			var packContainerLink2 = Factory.New<BaseAsycudaPackageContainerLink>();
			packContainerLink2.APC_ACN_Container = container2.PK;
			packContainerLink2.APC_APA_Pack = pack2.PK;

			Factory.Save();

			var destinationHeader = Factory.New<AsycudaManifestHeader>();
			destinationHeader.AMA_JobReference = "OGM0006";
			var copyBO = new BaseAsycudaManifestHeaderCopyBO(header, destinationHeader);
			copyBO.SourceContainerToCopy = container1;
			copyBO.CopyValuesFromGlobalManifest();

			CombineAssertions(() =>
			{
				AssertEquals("AsycudaManifestHeader.AMA_JobReference", "OGM0006", destinationHeader.AMA_JobReference);
				AssertEquals("AsycudaManifestHeader.AMA_JobReference", "SEA", destinationHeader.AMA_TransportMode);
				AssertEquals("AsycudaManifestHeader.AMA_ContainerMode", "CNT", destinationHeader.AMA_ContainerMode);
				AssertEquals("AsycudaManifestHeader.AMA_AgentType", "DRT", destinationHeader.AMA_AgentType);

				AssertEquals("AsycudaManifestHeader.AMA_RN_NKCountry", "ZA", destinationHeader.AMA_RN_NKCountry);
				AssertEquals("AsycudaManifestHeader.AMA_ManifestType", ZString.Empty, destinationHeader.AMA_ManifestType);

				AssertEquals("AsycudaManifestHeader.AMA_ParentTableCode", "JK", header.AMA_ParentTableCode);
				AssertEquals("AsycudaManifestHeader.AMA_ParentTableCode", ZString.Empty, destinationHeader.AMA_ParentTableCode);
				AssertNotEquals("AsycudaManifestHeader.AMA_ParentId", ZGuid.Empty, header.AMA_ParentId);
				AssertEquals("AsycudaManifestHeader.AMA_ParentId", ZGuid.Empty, destinationHeader.AMA_ParentId);

				AssertEquals("AsycudaManifestHeader.MasterBill.ABL_BillNumber", "MBL1966", destinationHeader.MasterBill.ABL_BillNumber);
				AssertEquals("AsycudaManifestHeader.MasterBill.ABL_E_DEP", new ZDateTime(2018, 4, 21), destinationHeader.MasterBill.ABL_E_DEP);
				AssertEquals("AsycudaManifestHeader.MasterBill.ABL_E_ARV", new ZDateTime(2018, 4, 25), destinationHeader.MasterBill.ABL_E_ARV);
				AssertEquals("AsycudaManifestHeader.MasterBill.ABL_RL_NKPortOfLoading", "CNSHA", destinationHeader.MasterBill.ABL_RL_NKPortOfLoading);
				AssertEquals("AsycudaManifestHeader.MasterBill.ABL_RL_NKPortOfDischarge", "AUSYD", destinationHeader.MasterBill.ABL_RL_NKPortOfDischarge);

				AssertEquals("AsycudaManifestHeader.Containers.Count", 1, destinationHeader.Containers.Count);
				AssertEquals("AsycudaManifestHeader.Containers[0].ACN_ContainerNumber", "CVB00001", destinationHeader.Containers[0].ACN_ContainerNumber);
				AssertEquals("AsycudaManifestHeader.Containers[0].ACN_Seal1", "SN0003", destinationHeader.Containers[0].ACN_Seal1);
				AssertEquals("AsycudaManifestHeader.Containers[0].ACN_SealingPartyType", "CAR", destinationHeader.Containers[0].ACN_SealingPartyType);

				AssertEquals("AsycudaManifestHeader.Bills.Count", 1, destinationHeader.Bills.Count);
				AssertEquals("AsycudaManifestHeader.Bills[0].ABL_BillNumber", "HB1708101600", destinationHeader.Bills[0].ABL_BillNumber);
				AssertEquals("AsycudaManifestHeader.Bills[0].ABL_RL_NKOrigin", "CNSHA", destinationHeader.Bills[0].ABL_RL_NKOrigin);
				AssertEquals("AsycudaManifestHeader.Bills[0].ABL_RL_NKFinalDestination", "AUSYD", destinationHeader.Bills[0].ABL_RL_NKFinalDestination);
				AssertEquals("AsycudaManifestHeader.Bills[0].ABL_ManifestQty", 10, destinationHeader.Bills[0].ABL_ManifestQty);

				AssertEquals("AsycudaManifestHeader.Bills[0].ABL_BillIssuer", "Rick", destinationHeader.Bills[0].ABL_BillIssuer);
				AssertEquals("AsycudaManifestHeader.Bills[0].ABL_GoodsLocation", "AUSYD", destinationHeader.Bills[0].ABL_GoodsLocation);

				AssertEquals("AsycudaManifestHeader.Bills[0].Packs.Count", 1, destinationHeader.Bills[0].Packs.Count);
				AssertEquals("AsycudaManifestHeader.Bills[0].Packs[0].APA_Weight", 9.65m, destinationHeader.Bills[0].Packs[0].APA_Weight);
				AssertEquals("AsycudaManifestHeader.Bills[0].Packs[0].APA_Volume", 3.28m, destinationHeader.Bills[0].Packs[0].APA_Volume);
				AssertEquals("AsycudaManifestHeader.Bills[0].Packs[0].ContainerPK", destinationHeader.Containers[0].PK, destinationHeader.Bills[0].Packs[0].ContainerPK);
			});
		}

		public void TestClusterKeyFromSourcesAreNotPopulatedOnDestinations()
		{
			var header = Factory.New<BaseAsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.SouthAfrica;
			header.AMA_JobReference = "CLUSTERKEYZA00001";
			var masterBill = header.Bills.AddNew();
			masterBill.ABL_BolType = AsycudaBill.ChildBolCode;
			masterBill.ABL_BillNumber = "MBL1966";
			var bill = header.Bills.AddNew();
			var container = header.Containers.AddNew();
			var pack = bill.Packs.AddNew();
			pack.ContainerPK = container.PK;

			Factory.Save();

			var destinationHeader = Factory.New<AsycudaManifestHeader>();
			var copyBO = new BaseAsycudaManifestHeaderCopyBO(header, destinationHeader)
			{
				SourceContainerToCopy = container
			};
			copyBO.CopyValuesFromGlobalManifest();
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertNotEquals("Destination Header ClusterKey Not Zero", 0, destinationHeader.AMA_ClusterKey);
				AssertNotEquals("Header ClusterKey Not Equal to Source", header.AMA_ClusterKey, destinationHeader.AMA_ClusterKey);

				var destinationMasterBill = destinationHeader.MasterBill;
				AssertNotEquals("Masterbill ClusterKey Not Zero", 0, destinationMasterBill.ABL_ClusterKey);
				AssertNotEquals("Masterbill ClusterKey Not Equal to Source", masterBill.ABL_ClusterKey, destinationMasterBill.ABL_ClusterKey);

				var destinationBill = destinationHeader.Bills[0];
				AssertNotEquals("Bill ClusterKey Not Zero", 0, destinationBill.ABL_ClusterKey);
				AssertNotEquals("Bill ClusterKey Not Equal to Source", bill.ABL_ClusterKey, destinationBill.ABL_ClusterKey);

				var destinationContainer = destinationHeader.Containers[0];
				AssertNotEquals("Container ClusterKey Not Zero", 0, destinationContainer.ACN_ClusterKey);
				AssertNotEquals("Container ClusterKey Not Equal to Source", container.ACN_ClusterKey, destinationContainer.ACN_ClusterKey);

				var destinationPack = destinationBill.Packs[0];
				AssertNotEquals("Pack ClusterKey Not Zero", 0, destinationPack.APA_ClusterKey);
				AssertNotEquals("Pack ClusterKey Not Equal to Source", pack.APA_ClusterKey, destinationPack.APA_ClusterKey);

				var destinationPackContainerLink = destinationPack.Pivot;
				AssertNotEquals("Pack Container Link ClusterKey Not Zero", 0, destinationPackContainerLink.APC_ClusterKey);
				AssertNotEquals("Pack Container Link ClusterKey Not Equal to Source", pack.Pivot.APC_ClusterKey, destinationPackContainerLink.APC_ClusterKey);

				AssertNoExceptionThrown("All CLusterKeys should be unique", () => Factory.Save());
			});
		}

		BaseAsycudaManifestHeader CreateManifestHeader()
		{
			var manifestHeader = Factory.New<BaseAsycudaManifestHeader>();
			manifestHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.SouthAfrica;
			manifestHeader.AMA_ApplicationCode = "NVC";
			manifestHeader.AMA_TransportMode = "SEA";
			manifestHeader.AMA_ContainerMode = "CNT";
			manifestHeader.AMA_AgentType = "DRT";
			manifestHeader.AMA_Voyage = "V0001";
			manifestHeader.AMA_JobReference = "MANZA00001";
			manifestHeader.AMA_ManifestType = "BBB";

			manifestHeader.AMA_ParentTableCode = JobConsolSchema.Constants.Prefix;
			manifestHeader.AMA_ParentId = Factory.New<Freight.Forwarding.Business.ForwardingConsol>().PK;

			return manifestHeader;
		}

		BaseAsycudaBill CreateMasterBill()
		{
			var masterBill = Factory.New<BaseAsycudaBill>();
			masterBill.ABL_BolType = AsycudaBill.ChildBolCode;
			masterBill.ABL_BillNumber = "MBL1966";
			masterBill.ABL_E_DEP = new ZDateTime(2018, 4, 21);
			masterBill.ABL_E_ARV = new ZDateTime(2018, 4, 25);
			masterBill.ABL_RL_NKPortOfLoading = "CNSHA";
			masterBill.ABL_RL_NKPortOfDischarge = "AUSYD";

			return masterBill;
		}

		BaseAsycudaBill CreateBill()
		{
			var otherBill = Factory.New<BaseAsycudaBill>();
			otherBill.ABL_BillNumber = "HB1708101600";
			otherBill.ABL_RL_NKOrigin = "CNSHA";
			otherBill.ABL_RL_NKFinalDestination = "AUSYD";
			otherBill.ABL_ManifestQty = 10;

			return otherBill;
		}

		void UpdateBillCountry(BaseAsycudaBill bill)
		{
			bill.ABL_BillIssuer = "Rick";
			bill.ABL_GoodsLocation = "AUSYD";
		}

		BaseAsycudaContainer CreateContainer(string conainerNumber)
		{
			var container = Factory.New<BaseAsycudaContainer>();
			container.ACN_ContainerNumber = conainerNumber;
			container.ACN_Seal1 = "SN0003";
			container.ACN_SealingPartyType = "CAR";

			return container;
		}

		BaseAsycudaPack CreatePack()
		{
			var pack = Factory.New<BaseAsycudaPack>();
			pack.APA_Weight = 9.65m;
			pack.APA_Volume = 3.28m;

			return pack;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = CreateManifestHeader();
			var destinationHeader = Factory.New<AsycudaManifestHeader>();
			return new BaseAsycudaManifestHeaderCopyBO(header, destinationHeader);
		}
	}

	[TestedType(typeof(BaseAsycudaContainerCollection))]
	sealed class BaseAsycudaContainerCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(BaseAsycudaContainerCollection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.NewWithValidTestData<BaseAsycudaManifestHeader>();
			return new BaseAsycudaContainerCollection(header);
		}
	}
}

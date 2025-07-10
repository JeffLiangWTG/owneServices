using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.PE.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaBill))]
	sealed class AsycudaBillTest : ManifestBase.Testing.AsycudaBillTest
	{
		public void TestIAsycudaBill()
		{
			var bizObj = GetNewBusinessObject();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.PEManifest.IAsycudaBill>(bizObj.PK).GetType());
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<ASYCUDA.Business.AsycudaBill>(bizObj.PK).GetType());
		}

		public void TestHeader()
		{
			var bill = (AsycudaBill)GetNewBusinessObject();
			AssertType<AsycudaManifestHeader>(bill.Header);
		}

		public void TestGetCountryCode()
		{
			var bill = Factory.New<AsycudaBillForTest>();
			AssertEquals(Core.Constants.CountryCodes.Peru, bill.GetCountryCode());
		}

		public void TestSetDefaultValues()
		{
			var bill = Factory.New<AsycudaBillForTest>();
			AssertEquals(Core.Constants.ShipmentTypes.StandardHouse, bill.ABL_BolType);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			return header.Bills.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.SuspendCheckBusinessObjectType();
			return header.Bills.AddNew();
		}

		public void TestValidation()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var mBill = header.MasterBill;
			var bill = header.Bills.AddNew();

			AssertType<AsycudaBillValidationForMasterChild>(mBill.Validation);
			AssertType<AsycudaBillValidationForRegularBill>(bill.Validation);
		}

		public void TestLookups()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			AssertType<AsycudaBillLookups>(bill.Lookups);
		}

		public void TestPacks()
		{
			var bill = (AsycudaBill)GetNewBusinessObject();
			AssertType<AsycudaPackCollection>(bill.Packs);
		}

		public void TestGetPackTypeCore()
		{
			var bill = Factory.New<AsycudaBillForTest>();
			var pack = bill.Packs.AddNew();
			AssertType<AsycudaPack>(pack);
		}

		public void TestShouldSynchronisePaymentType()
		{
			var bill = Factory.New<AsycudaBillForTest>();
			Assert("ShouldSynchronisePaymentType should be true", bill.SynchronisePaymentType);
		}

		public void TestManifestUQIsNotConverted()
		{
			var pack1 = Factory.New<CusRefPacks>();
			pack1.RP_CustomsPack = "BT";
			pack1.RP_Type = "GMB";
			pack1.RP_CustomsCountry = "PE";
			pack1.RP_ConversionFactor = 2;
			pack1.RP_CommercialPack = "PCS";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_OuterPacks = 10;
			shipment.JS_F3_NKPackType = "PCS";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			Factory.Save();

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.SetParent(consol);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();

			var bill = manifestHeader.Bills[0];

			CombineAssertions(() =>
			{
				AssertEquals("ABL_ManifestQty is not converted", 10, bill.ABL_ManifestQty);
				AssertEquals("ABL_ManifestUQ is not converted", "PCS", bill.ABL_ManifestUQ);
			});
		}

		public void TestShipperRegNo()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "PE";
			org.OH_FullName = "FULL NAME";
			var orgAddress = org.MainAddress;
			org.CustomsCodes.AddNew(OrgCusCode.PeruCodeTypes.DNI, "62318879");

			var org1 = Factory.New<OrgHeader>();
			org1.OH_RL_NKClosestPort = "PE";
			org1.OH_FullName = "SECOND";
			var orgAddress1 = org1.MainAddress;

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			bill.ABL_OA_Shipper = ZGuid.Empty;
			Factory.Save();
			bill.ABL_OA_Shipper = orgAddress.PK;
			AssertEquals("62318879", bill.ABL_ShipperRegNo);

			bill.ABL_OA_Shipper = orgAddress1.PK;
			AssertEquals(ZString.Empty, bill.ABL_ShipperRegNo);
		}

		public void TestShipperWithThreeRegNo()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "PE";
			org.OH_FullName = "FULL NAME";
			var orgAddress = org.MainAddress;
			org.CustomsCodes.AddNew(OrgCusCode.PeruCodeTypes.DNI, "62318879");
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "J074878112");
			org.CustomsCodes.AddNew(OrgCusCode.PeruCodeTypes.GovernmentTaxFileCode, "123456789456");

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			bill.ABL_OA_Shipper = ZGuid.Empty;
			Factory.Save();
			bill.ABL_OA_Shipper = orgAddress.PK;
			AssertEquals("123456789456", bill.ABL_ShipperRegNo);
		}

		public void TestConsigneeRegNo()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "PE";
			org.OH_FullName = "FULL NAME";
			var orgAddress = org.MainAddress;
			org.CustomsCodes.AddNew(OrgCusCode.PeruCodeTypes.DNI, "62318879");

			var org1 = Factory.New<OrgHeader>();
			org1.OH_RL_NKClosestPort = "PE";
			org1.OH_FullName = "SECOND";
			var orgAddress1 = org1.MainAddress;

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			bill.ABL_OA_Consignee = ZGuid.Empty;
			Factory.Save();
			bill.ABL_OA_Consignee = orgAddress.PK;
			AssertEquals("62318879", bill.ABL_ConsigneeRegNo);

			bill.ABL_OA_Consignee = orgAddress1.PK;
			AssertEquals(ZString.Empty, bill.ABL_ConsigneeRegNo);
		}

		public void TestNotifyPartyRegNo()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "PE";
			org.OH_FullName = "FULL NAME";
			var orgAddress = org.MainAddress;
			org.CustomsCodes.AddNew(OrgCusCode.PeruCodeTypes.DNI, "62318879");

			var org1 = Factory.New<OrgHeader>();
			org1.OH_RL_NKClosestPort = "PE";
			org1.OH_FullName = "SECOND";
			var orgAddress1 = org1.MainAddress;

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			bill.ABL_OA_NotifyParty = ZGuid.Empty;
			Factory.Save();
			bill.ABL_OA_NotifyParty = orgAddress.PK;
			AssertEquals("62318879", bill.ABL_NotifyPartyRegNo);

			bill.ABL_OA_NotifyParty = orgAddress1.PK;
			AssertEquals(ZString.Empty, bill.ABL_NotifyPartyRegNo);
		}
	}

	sealed class AsycudaBillForTest : AsycudaBill
	{
		public AsycudaBillForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new ZString GetCountryCode() => base.GetCountryCode();
		public new ManifestBase.IAsycudaPackCollection<ManifestBase.AsycudaPack, ManifestBase.AsycudaBill> CreateNewAsycudaPackCollection() => base.CreateNewAsycudaPackCollection();
		public new Type GetPackTypeCore() => base.GetPackTypeCore();
	}
}

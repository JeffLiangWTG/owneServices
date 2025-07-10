using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.UY.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaBill))]
	public class AsycudaBillTest : ManifestBase.Testing.AsycudaBillTest
	{
		public void TestBillCanBeDelete()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			var bill = header.Bills.AddNew();
			bill.ABL_MessageStatus = "ACP";
			AssertEquals(false, bill.CanDelete);

			AssertEquals("This Bill is already registered with Customs.\r\nIf you need to cancel it from Customs, you must use the Cancel option from Manifest menu.", bill.ReasonForNotAbleToDelete);

			bill.ABL_MessageStatus = "";
			AssertEquals(true, bill.CanDelete);
		}

		public void TestShipperRegNo()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "UY";
			org.OH_FullName = "FULL NAME";
			var orgAddress = org.MainAddress;
			org.CustomsCodes.AddNew(UruguayOrgCusCodeInfo.OrgCusCodes.CID, "62318879");

			var org1 = Factory.New<OrgHeader>();
			org1.OH_RL_NKClosestPort = "UY";
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

		public void TestShipperWithTwoRegNo()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "UY";
			org.OH_FullName = "FULL NAME";
			var orgAddress = org.MainAddress;
			org.CustomsCodes.AddNew(UruguayOrgCusCodeInfo.OrgCusCodes.CID, "62318879");
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "J074878112");
			org.CustomsCodes.AddNew(UruguayOrgCusCodeInfo.OrgCusCodes.RUT, "123456789456");

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
			org.OH_RL_NKClosestPort = "UY";
			org.OH_FullName = "FULL NAME";
			var orgAddress = org.MainAddress;
			org.CustomsCodes.AddNew(UruguayOrgCusCodeInfo.OrgCusCodes.CID, "62318879");

			var org1 = Factory.New<OrgHeader>();
			org1.OH_RL_NKClosestPort = "UY";
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
			org.OH_RL_NKClosestPort = "UY";
			org.OH_FullName = "FULL NAME";
			var orgAddress = org.MainAddress;
			org.CustomsCodes.AddNew(UruguayOrgCusCodeInfo.OrgCusCodes.CID, "62318879");

			var org1 = Factory.New<OrgHeader>();
			org1.OH_RL_NKClosestPort = "UY";
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

		public void TestISelectionItemMembers()
		{
			CombineAssertions(() =>
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				header.AMA_TransportMode = Core.Constants.TransportModes.Air;
				var bill = header.Bills.AddNew();
				bill.ABL_BillNumber = "UYBILL";
				ISelectionItem selectionItem = bill;

				AssertEquals("Bill Number - UYBILL", selectionItem.SelectionDescription(false));
				AssertEquals("Bill Number - UYBILL - Original", selectionItem.SelectionDescription(true));

				bill.ABL_BillStatus = MessageStatusCodeList.Codes.Accepted;
				header.AMA_MessageStatus = MessageStatusCodeList.Codes.Accepted;

				AssertEquals("Bill Number - UYBILL - Cancel", selectionItem.SelectionDescription(true));
			});
		}

		public void TestIAsycudaBill()
		{
			var bizObj = GetNewBusinessObject();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.UYManifest.IAsycudaBill>(bizObj.PK).GetType());
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<ASYCUDA.Business.AsycudaBill>(bizObj.PK).GetType());
		}

		public void TestHeader()
		{
			var bill = (AsycudaBill)GetNewBusinessObject();
			AssertType<AsycudaManifestHeader>(bill.Header);
		}

		public void TestPacks()
		{
			var bill = (AsycudaBill)GetNewBusinessObject();
			AssertType<AsycudaPackCollection<AsycudaPack, AsycudaBill>>(bill.Packs);
		}

		public void TestGetCountryCode()
		{
			var bill = Factory.New<AsycudaBillForTest>();
			AssertEquals(Core.Constants.CountryCodes.Uruguay, bill.GetCountryCode());
		}

		public void TestCreateNewAsycudaPackCollection()
		{
			var bill = Factory.New<AsycudaBillForTest>();
			AssertType<AsycudaPackCollection<AsycudaPack, AsycudaBill>>(bill.CreateNewAsycudaPackCollection());
		}

		public void TestGetPackTypeCore()
		{
			var bill = Factory.New<AsycudaBillForTest>();
			AssertEquals(typeof(AsycudaPack), bill.GetPackTypeCore());
		}

		#region Implementation

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

		class AsycudaBillForTest : AsycudaBill
		{
			public AsycudaBillForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public new ZString GetCountryCode() => base.GetCountryCode();
			public new ManifestBase.IAsycudaPackCollection<ManifestBase.AsycudaPack, ManifestBase.AsycudaBill> CreateNewAsycudaPackCollection() => base.CreateNewAsycudaPackCollection();
			public new Type GetPackTypeCore() => base.GetPackTypeCore();
		}
		#endregion
	}
}

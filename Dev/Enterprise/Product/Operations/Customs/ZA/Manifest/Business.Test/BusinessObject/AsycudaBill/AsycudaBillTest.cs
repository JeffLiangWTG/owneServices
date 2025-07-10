using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Customs.ZA.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaBill))]
	class AsycudaBillTest : ManifestBase.Testing.AsycudaBillTest
	{
		public void TestIAsycudaBill()
		{
			var bizObj = GetNewBusinessObject();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaBill>(bizObj.PK).GetType());
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<ASYCUDA.Business.AsycudaBill>(bizObj.PK).GetType());
		}

		public void TestToOrderConsignee()
		{
			var bill = (AsycudaBill)GetNewBusinessObject();
			bill.ABL_ConsigneeName = "TO ORDER";
			AssertEquals("Required value should default to Street Address field 1", "NO ADDRESS SUPPLIED", bill.ABL_ConsigneeStreet1);
			Assert(bill.ToOrderConsignment);

			bill.ABL_ConsigneeName = "JOHN SMITH";
			bill.ABL_ConsigneeStreet1 = "15 DREWY LANE";
			bill.ABL_ConsigneeStreet2 = "AMBERLEY";
			bill.ABL_ConsigneeCity = "CAPE TOWN";
			bill.ABL_ConsigneeState = "FS";
			bill.ABL_ConsigneePostcode = "6419";
			bill.ABL_RN_NKConsigneeCountry = "ZA";

			bill.ABL_ConsigneeName = "TO ORDER";
			AssertEquals("Required value should default to Street Address field 1", "NO ADDRESS SUPPLIED", bill.ABL_ConsigneeStreet1);
			AssertEquals("ABL_ConsigneeStreet2 should be cleared out", ZString.Empty, bill.ABL_ConsigneeStreet2);
			AssertEquals("ABL_ConsigneeCity should be cleared out", ZString.Empty, bill.ABL_ConsigneeCity);
			AssertEquals("ABL_ConsigneeState should be cleared out", ZString.Empty, bill.ABL_ConsigneeState);
			AssertEquals("ABL_ConsigneePostcode should be cleared out", ZString.Empty, bill.ABL_ConsigneePostcode);
			AssertEquals("ABL_RN_NKConsigneeCountry should be cleared out", ZString.Empty, bill.ABL_RN_NKConsigneeCountry);
		}

		public void TestCaseNumbers()
		{
			var bill = (AsycudaBill)GetNewBusinessObject();
			for (var i = 1; i < 4; i++)
			{
				var caseNumber = Factory.New<CaseNumber>();
				caseNumber.CY_ParentID = bill.PK;
				caseNumber.CY_ParentTableCode = AsycudaBillSchema.Constants.Prefix;
			}
			CombineAssertions(() =>
			{
				AssertEquals("Bill has 3 linked case numbers loaded", 3, bill.CaseNumbers.Count);
				AssertEquals("CaseNumbers Collection is child editable", true, bill.IsRegisteredEditableChildObject(bill.CaseNumbers));
			});
		}

		public void TestFetchStrategy()
		{
			var bill = (AsycudaBill)GetNewBusinessObject();
			AssertType<AsycudaBillFetchStrategy>(bill.FetchStrategy);
		}

		public void TestNoExceptionWhenSettingABL_JS_ShipmentIfBillWithoutHeader()
		{
			var bill = Factory.New<AsycudaBill>();
			var shipment = Factory.New<ForwardingShipment>();
			AssertNoExceptionThrown(() => bill.ABL_JS_Shipment = shipment.PK);
		}

		public void TestDefaultBillIssuer()
		{
			GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.SouthAfricaCodeTypes.BillIssuer, "DJC1", Core.Constants.CountryCodes.SouthAfrica);
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			AssertEquals("", bill.ABL_BillIssuer);
			var consol = Factory.New<ForwardingConsol>();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = "STD";
			bill.ABL_JS_Shipment = shipment.PK;
			AssertEquals("DJC1", bill.ABL_BillIssuer);
		}

		public void TestCargoReleaseStatus()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = nameof(ManifestDocumentType.AQM);
			var bill = header.Bills.AddNew();
			AssertEquals(ZString.Empty, bill.CargoReleaseStatus);
			bill.CustomsEntryNumber = "ABC123";
			bill.CargoReleaseStatus = "1";
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			bill = newFactory.Load<AsycudaBill>(bill.PK);
			AssertEquals("1", bill.CargoReleaseStatus);
		}

		public void TestCargoReleaseStatusOtherDescriptionClearIfNeeded()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = nameof(ManifestDocumentType.AQM);
			var bill = header.Bills.AddNew();
			bill.CargoReleaseStatus = AsycudaBill.CargoReleaseStatus_Other;
			bill.CargoReleaseStatusOtherDescription = "Other Desc.";
			bill.CargoReleaseStatus = "1";
			AssertEquals(ZString.Empty, bill.CargoReleaseStatusOtherDescription);
		}

		public void TestIsCargoReleaseStatusVisible()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = nameof(ManifestDocumentType.AQM);
			var bill = header.Bills.AddNew();
			AssertEquals(true, bill.IsCargoReleaseStatusVisible);
			header.AMA_ManifestType = ZString.Empty;
			AssertEquals(false, bill.IsCargoReleaseStatusVisible);
		}

		public void TestCargoReleaseStatusDescription()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CustomsStatus");
			var goodsReleased = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "1", "Goods released", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(goodsReleased.PK, "AQM", "desc.");
			var goodsStoppedDetained = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "2", "Goods stopped / detained", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(goodsStoppedDetained.PK, "AQM", "desc.");
			var conditionalReleased = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "3", "Goods may move under Customs transfer (Customs Intervention) - Conditional Release", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(conditionalReleased.PK, "AQM", "desc.");
			var releasedToStatesWarehouse = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "50", "Released to States Warehouse", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(releasedToStatesWarehouse.PK, "AQM", "desc.");
			var other = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "51", "Other (Overboard, destroyed, lost etc)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(other.PK, "AQM", "desc.");
			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = nameof(ManifestDocumentType.AQM);
			var bill = header.Bills.AddNew();
			bill.CargoReleaseStatus = "1";
			AssertEquals("Goods released", bill.CargoReleaseStatusDescription);
			bill.CargoReleaseStatus = "2";
			AssertEquals("Goods stopped / detained", bill.CargoReleaseStatusDescription);
			bill.CargoReleaseStatus = "3";
			AssertEquals("Goods may move under Customs transfer (Customs Intervention) - Conditional Release", bill.CargoReleaseStatusDescription);
			bill.CargoReleaseStatus = "50";
			AssertEquals("Released to States Warehouse", bill.CargoReleaseStatusDescription);
			bill.CargoReleaseStatus = "51";
			bill.CargoReleaseStatusOtherDescription = "Other Desc.";
			AssertEquals("Other Desc.", bill.CargoReleaseStatusDescription);
		}

		public void TestCargoReleaseStatusOtherDescription()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = nameof(ManifestDocumentType.AQM);
			var bill = header.Bills.AddNew();
			bill.CargoReleaseStatusOtherDescription = "Other Desc.";
			AssertEquals("Other Desc.", bill.CargoReleaseStatusOtherDescription);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			bill = newFactory.Load<AsycudaBill>(bill.PK);
			AssertEquals("Other Desc.", bill.CargoReleaseStatusOtherDescription);
		}

		public void TestIsCargoReleaseStatusOtherDescriptionVisible()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = nameof(ManifestDocumentType.AQM);
			var bill = header.Bills.AddNew();
			bill.CargoReleaseStatus = AsycudaBill.CargoReleaseStatus_Other;
			AssertEquals(true, bill.IsCargoReleaseStatusVisible);
		}

		public void TestABL_BillStatus_ReadOnly()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = nameof(ManifestDocumentType.ANT);
			var bill = Factory.New<AsycudaBillForTest>();
			header.Bills.Add(bill);
			AssertEquals("Readonly for ANT", true, bill.ABL_BillStatus_ReadOnly_Exposed);
			header.AMA_ManifestType = nameof(ManifestDocumentType.AQM);
			header.Bills.RemoveAndDeleteAll();
			var bill2 = Factory.New<AsycudaBillForTest>();
			header.Bills.Add(bill2);
			AssertEquals("Readonly for AQM", true, bill2.ABL_BillStatus_ReadOnly_Exposed);
		}

		public void TestCustomsEntryNumberCaption()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			AssertEquals("Local Reference No / LRN", bill.CustomsEntryNumberInfo.HumanReadableName);
		}

		public void TestCustomsEntryNumberTypeCaption()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			AssertEquals("LRN Type", bill.CustomsEntryNumberTypeInfo.HumanReadableName);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.SuspendCheckBusinessObjectType();
			var bill = header.Bills.AddNew();
			return bill;
		}

		class AsycudaBillForTest : AsycudaBill
		{
			public AsycudaBillForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public bool ABL_BillStatus_ReadOnly_Exposed => base.ABL_BillStatus_ReadOnly;
		}
	}
}

using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaBill))]
	sealed class AsycudaBillTest : ManifestBase.Testing.AsycudaBillTest
	{
		public void TestEntryNumber()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			CombineAssertions(() =>
			{
				var entryNumber = "5288931722JUN1600001";
				AssertEquals(0, bill.CustomsEntryNumbers.Count);
				bill.EntryNumber = entryNumber;
				AssertEquals(1, bill.CustomsEntryNumbers.Count);
				var first = bill.CustomsEntryNumbers.Cast<ABLEntryNum>().First();
				AssertEquals("CUS", first.CE_Category);
				AssertEquals(MessageTypeList.Codes.FHM, first.CE_EntryType);
				AssertEquals(entryNumber, first.CE_EntryNum);
			});
		}

		public void TestABL_BillStatus_ReadOnly()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			var info = bill.ABL_BillStatusInfo;
			Assert("ReadOnly", info.ReadOnly);
		}

		public void TestLookups()
		{
			var bizObj = (AsycudaBill)GetNewBusinessObject();
			AssertType<AsycudaBillLookups>(bizObj.Lookups);
		}

		public void TestValidationType()
		{
			var bizObj = (AsycudaBill)GetNewBusinessObject();
			AssertType<AsycudaBillValidationForRegularBill>(bizObj.Validation);

			bizObj.ABL_BolType = "BOL";
			AssertType<AsycudaBillValidationForMasterChild>(bizObj.Validation);
		}

		public void TestABL_RL_NKFinalDestination_Caption()
		{
			var bizObj = (AsycudaBill)GetNewBusinessObject();
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(bizObj.ABL_RL_NKFinalDestinationInfo);
			AssertEquals("ABL_RL_NKFinalDestination Caption", "Final Destination", resourceStringData.Caption);
		}

		public void TestIsEscortRequired()
		{
			var bizObj = (AsycudaBill)GetNewBusinessObject();
			bizObj.ABL_SpecialCargoCode = YesNoList.Codes.Yes;
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(bizObj.IsEscortRequiredInfo);
			CombineAssertions(() =>
			{
				AssertEquals("IsEscortRequired Caption", "Is Escort Required?", resourceStringData.Caption);
				Assert("Should be true.", bizObj.IsEscortRequired);
			});

			bizObj.ABL_SpecialCargoCode = YesNoList.Codes.No;
			Assert("Should be false.", !bizObj.IsEscortRequired);

			bizObj.ABL_SpecialCargoCode = ZString.Empty;
			Assert("Should be false.", !bizObj.IsEscortRequired);
		}

		public void TestTariffFormatter()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			AssertType<TaiwanTariffFormatter>(((ITariffFormatProvider)bill).TariffFormatter);
		}

		public void TestABL_RL_NKPortOfLoadingAttribute()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(bill.ABL_RL_NKPortOfLoadingInfo);
			AssertEquals("ABL_RL_NKPortOfLoading Caption", "Port of Loading", resourceStringData.Caption);
		}

		public void TestABL_LocationInformationAttribute()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(bill.ABL_LocationInformationInfo);
			CombineAssertions(() =>
			{
				AssertEquals("ABL_LocationInformation Caption", "Port of Loading Description (Z99)", resourceStringData.Caption);
				AssertEquals("ABL_LocationInformation Short Caption", "Z99 Description", resourceStringData.ShortCaption);
				AssertEquals("ABL_LocationInformation MaxLength", 70, bill.ABL_LocationInformationInfo.MaxLength);
			});
		}

		public void TestIAsycudaBill()
		{
			var bizObj = GetNewBusinessObject();
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.TWManifest.IAsycudaBill>(bizObj.PK).GetType());
				AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<AsycudaBill>(bizObj.PK).GetType());
			});
		}

		public void TestABL_GoodsLocationAttribute()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(bill.ABL_GoodsLocationInfo);
			AssertEquals("ABL_GoodsLocation Caption", "Goods Location", resourceStringData.Caption);
		}

		public void TestABL_ShipmentType()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			var info = bill.ABL_ShipmentTypeInfo;
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(info);
			AssertEquals("ABL_ShipmentType Caption", "Type", resourceStringData.Caption);

			var list = MetaData.GetListDataSource(bill, info.PropertyDescriptor) as CodeDescriptionPairList;
			AssertContainsExactElementsInAnyOrder(bill.Lookups.TWShipmentTypes, list);
		}

		public void TestABL_UCRNumber()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			var info = bill.ABL_UCRNumberInfo;
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(info);
			AssertEquals("ABL_UCRNumber Caption", "UCR", resourceStringData.Caption);
		}

		public void TestABL_Remarks()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			var info = bill.ABL_RemarksInfo;
			AssertEquals("ABL_Remarks MaxLength", 35, info.MaxLength);

			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(info);
			AssertEquals("ABL_Remarks Caption", "Package Description", resourceStringData.Caption);
		}

		public void TestGoodsDescription()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			var info = bill.GoodsDescriptionInfo;
			AssertEquals("GoodsDescription MaxLength", 256, info.MaxLength);

			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(info);
			AssertEquals("GoodsDescription Caption", "Goods Description", resourceStringData.Caption);

			var pack = bill.Packs.AddNew();
			pack.PackedItem.API_GoodsDescription = "test goods description";
			AssertEquals("GoodsDescription Mapping", "test goods description", bill.GoodsDescription);
		}

		public void TestBagNumber()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			var info = bill.BagNumberInfo;
			AssertEquals("BagNumberInfo MaxLength", 16, info.MaxLength);

			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(info);
			AssertEquals("BagNumber Caption", "Bag Number", resourceStringData.Caption);

			var list = MetaData.GetListDataSource(bill, info.PropertyDescriptor) as CodeDescriptionPairList;
			AssertContainsExactElementsInAnyOrder(bill.Lookups.BagNumberList, list);

			AssertEquals("CustomsEntryNumbers should not have any FHM records", 0, bill.CustomsEntryNumbers.Cast<ABLEntryNum>()
				.Count(x => x.CE_EntryType == MessageTypeList.Codes.FHM));
			bill.BagNumber = "1234";
			AssertEquals("CustomsEntryNumbers should have a FHM records", 1, bill.CustomsEntryNumbers.Cast<ABLEntryNum>()
				.Count(x => x.CE_EntryType == MessageTypeList.Codes.FHM && x.CE_EntryLineReference == "1234"
				&& x.CE_Category == "CUS" && x.CE_RN_NKCountryCode == Core.Constants.CountryCodes.Taiwan));
		}

		public void TestShipperReadOnlyMember()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "BOB THE BUILDER";
			org.OH_RL_NKClosestPort = "AU";
			var orgAddress = org.MainAddress;
			orgAddress.OA_Address1 = "ADDRESS 1";
			orgAddress.OA_Address2 = "ADDRESS 2";
			orgAddress.OA_City = "CITY";
			orgAddress.OA_State = "STATE";
			orgAddress.OA_PostCode = "203023";
			orgAddress.OA_Phone = "+4234232";
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_OA_Shipper = ZGuid.Empty;
			CombineAssertions(() =>
			{
				AssertEquals("bill.ABL_ShipperLocalNameInfo.ReadOnly", false, bill.ABL_ShipperLocalNameInfo.ReadOnly);
				AssertEquals("bill.ABL_ShipperLocalStreet1Info.ReadOnly", false, bill.ABL_ShipperLocalStreet1Info.ReadOnly);
				AssertEquals("bill.ABL_ShipperLocalStreet2Info.ReadOnly", false, bill.ABL_ShipperLocalStreet2Info.ReadOnly);
				AssertEquals("bill.ABL_ShipperLocalCityInfo.ReadOnly", false, bill.ABL_ShipperLocalCityInfo.ReadOnly);
				AssertEquals("bill.ABL_ShipperLocalStateInfo.ReadOnly", false, bill.ABL_ShipperLocalStateInfo.ReadOnly);
			});

			bill.ABL_OA_Shipper = orgAddress.PK;
			CombineAssertions(() =>
			{
				AssertEquals("bill.ABL_ShipperLocalNameInfo.ReadOnly", true, bill.ABL_ShipperLocalNameInfo.ReadOnly);
				AssertEquals("bill.ABL_ShipperLocalStreet1Info.ReadOnly", true, bill.ABL_ShipperLocalStreet1Info.ReadOnly);
				AssertEquals("bill.ABL_ShipperLocalStreet2Info.ReadOnly", true, bill.ABL_ShipperLocalStreet2Info.ReadOnly);
				AssertEquals("bill.ABL_ShipperLocalCityInfo.ReadOnly", true, bill.ABL_ShipperLocalCityInfo.ReadOnly);
				AssertEquals("bill.ABL_ShipperLocalStateInfo.ReadOnly", true, bill.ABL_ShipperLocalStateInfo.ReadOnly);
			});
		}

		public void TestSetShipperLocalAddressDefaultValues()
		{
			var header = CreateOrganizationForJobDocAddress();
			var bill = GetNewBusinessObject() as AsycudaBill;
			bill.ABL_OA_Shipper = header.MainAddress.PK;
			CombineAssertions(() =>
			{
				AssertEquals("綠晃科技股份有限公司", bill.ABL_ShipperLocalName);
				AssertEquals("臺北加工出口區園東街6號", bill.ABL_ShipperLocalStreet1);
				AssertEquals("5樓之1", bill.ABL_ShipperLocalStreet2);
				AssertEquals("臺北巿", bill.ABL_ShipperLocalCity);
				AssertEquals("TPE", bill.ABL_ShipperLocalState);
			});

			bill.ABL_OA_Shipper = ZGuid.Empty;
			CombineAssertions(() =>
			{
				AssertEquals("綠晃科技股份有限公司", bill.ABL_ShipperLocalName);
				AssertEquals("臺北加工出口區園東街6號", bill.ABL_ShipperLocalStreet1);
				AssertEquals("5樓之1", bill.ABL_ShipperLocalStreet2);
				AssertEquals("臺北巿", bill.ABL_ShipperLocalCity);
				AssertEquals("TPE", bill.ABL_ShipperLocalState);
			});

			bill.ABL_OA_Shipper = header.Addresses.Cast<OrgAddress>().FirstOrDefault(c => c.OA_CompanyNameOverride == "HAPPY CO., LTD.1").PK;
			CombineAssertions(() =>
			{
				AssertNullOrEmpty(bill.ABL_ShipperLocalName);
				AssertNullOrEmpty(bill.ABL_ShipperLocalStreet1);
				AssertNullOrEmpty(bill.ABL_ShipperLocalStreet2);
				AssertNullOrEmpty(bill.ABL_ShipperLocalCity);
				AssertNullOrEmpty(bill.ABL_ShipperLocalState);
			});
		}

		public void TestShipperRegNoTypes()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			AssertContainsExactElementsInExactOrder(new ZString[] { OrgCusCode.CodeTypes.VATCode, OrgCusCode.CodeTypes.PassportID, OrgCusCode.TaiwanCodeTypes.PID }, bill.ShipperRegNoTypes());
		}

		public void TestConsigneeReadOnlyMember()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "BOB THE BUILDER";
			org.OH_RL_NKClosestPort = "AU";
			var orgAddress = org.MainAddress;
			orgAddress.OA_Address1 = "ADDRESS 1";
			orgAddress.OA_Address2 = "ADDRESS 2";
			orgAddress.OA_City = "CITY";
			orgAddress.OA_State = "STATE";
			orgAddress.OA_PostCode = "203023";
			orgAddress.OA_Phone = "+4234232";
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_OA_Consignee = ZGuid.Empty;
			CombineAssertions(() =>
			{
				AssertEquals("bill.ABL_ConsigneeLocalNameInfo.ReadOnly", false, bill.ABL_ConsigneeLocalNameInfo.ReadOnly);
				AssertEquals("bill.ABL_ConsigneeLocalStreet1Info.ReadOnly", false, bill.ABL_ConsigneeLocalStreet1Info.ReadOnly);
				AssertEquals("bill.ABL_ConsigneeLocalStreet2Info.ReadOnly", false, bill.ABL_ConsigneeLocalStreet2Info.ReadOnly);
				AssertEquals("bill.ABL_ConsigneeLocalCityInfo.ReadOnly", false, bill.ABL_ConsigneeLocalCityInfo.ReadOnly);
				AssertEquals("bill.ABL_ConsigneeLocalStateInfo.ReadOnly", false, bill.ABL_ConsigneeLocalStateInfo.ReadOnly);
			});

			bill.ABL_OA_Consignee = orgAddress.PK;
			CombineAssertions(() =>
			{
				AssertEquals("bill.ABL_ConsigneeLocalNameInfo.ReadOnly", true, bill.ABL_ConsigneeLocalNameInfo.ReadOnly);
				AssertEquals("bill.ABL_ConsigneeLocalStreet1Info.ReadOnly", true, bill.ABL_ConsigneeLocalStreet1Info.ReadOnly);
				AssertEquals("bill.ABL_ConsigneeLocalStreet2Info.ReadOnly", true, bill.ABL_ConsigneeLocalStreet2Info.ReadOnly);
				AssertEquals("bill.ABL_ConsigneeLocalCityInfo.ReadOnly", true, bill.ABL_ConsigneeLocalCityInfo.ReadOnly);
				AssertEquals("bill.ABL_ConsigneeLocalStateInfo.ReadOnly", true, bill.ABL_ConsigneeLocalStateInfo.ReadOnly);
			});
		}

		public void TestSetConsigneeLocalAddressDefaultValues()
		{
			var header = CreateOrganizationForJobDocAddress();
			var bill = GetNewBusinessObject() as AsycudaBill;
			bill.ABL_OA_Consignee = header.MainAddress.PK;
			CombineAssertions(() =>
			{
				AssertEquals("綠晃科技股份有限公司", bill.ABL_ConsigneeLocalName);
				AssertEquals("臺北加工出口區園東街6號", bill.ABL_ConsigneeLocalStreet1);
				AssertEquals("5樓之1", bill.ABL_ConsigneeLocalStreet2);
				AssertEquals("臺北巿", bill.ABL_ConsigneeLocalCity);
				AssertEquals("TPE", bill.ABL_ConsigneeLocalState);
			});

			bill.ABL_OA_Consignee = ZGuid.Empty;
			CombineAssertions(() =>
			{
				AssertEquals("綠晃科技股份有限公司", bill.ABL_ConsigneeLocalName);
				AssertEquals("臺北加工出口區園東街6號", bill.ABL_ConsigneeLocalStreet1);
				AssertEquals("5樓之1", bill.ABL_ConsigneeLocalStreet2);
				AssertEquals("臺北巿", bill.ABL_ConsigneeLocalCity);
				AssertEquals("TPE", bill.ABL_ConsigneeLocalState);
			});

			bill.ABL_OA_Consignee = header.Addresses.Cast<OrgAddress>().FirstOrDefault(c => c.OA_CompanyNameOverride == "HAPPY CO., LTD.1").PK;
			CombineAssertions(() =>
			{
				AssertNullOrEmpty(bill.ABL_ConsigneeLocalName);
				AssertNullOrEmpty(bill.ABL_ConsigneeLocalStreet1);
				AssertNullOrEmpty(bill.ABL_ConsigneeLocalStreet2);
				AssertNullOrEmpty(bill.ABL_ConsigneeLocalCity);
				AssertNullOrEmpty(bill.ABL_ConsigneeLocalState);
			});
		}

		public void TestConsigneeRegNoTypes()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			AssertContainsExactElementsInExactOrder(new ZString[] { OrgCusCode.CodeTypes.VATCode, OrgCusCode.CodeTypes.PassportID, OrgCusCode.TaiwanCodeTypes.PID }, bill.ConsigneeRegNoTypes());
		}

		public void TestNotifyPartyReadOnlyMember()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "BOB THE BUILDER";
			org.OH_RL_NKClosestPort = "AU";
			var orgAddress = org.MainAddress;
			orgAddress.OA_Address1 = "ADDRESS 1";
			orgAddress.OA_Address2 = "ADDRESS 2";
			orgAddress.OA_City = "CITY";
			orgAddress.OA_State = "STATE";
			orgAddress.OA_PostCode = "203023";
			orgAddress.OA_Phone = "+4234232";
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_OA_NotifyParty = ZGuid.Empty;
			CombineAssertions(() =>
			{
				AssertEquals("bill.ABL_NotifyPartyLocalNameInfo.ReadOnly", false, bill.ABL_NotifyPartyLocalNameInfo.ReadOnly);
				AssertEquals("bill.ABL_NotifyPartyLocalStreet1Info.ReadOnly", false, bill.ABL_NotifyPartyLocalStreet1Info.ReadOnly);
				AssertEquals("bill.ABL_NotifyPartyLocalStreet2Info.ReadOnly", false, bill.ABL_NotifyPartyLocalStreet2Info.ReadOnly);
				AssertEquals("bill.ABL_NotifyPartyLocalCityInfo.ReadOnly", false, bill.ABL_NotifyPartyLocalCityInfo.ReadOnly);
				AssertEquals("bill.ABL_NotifyPartyLocalStateInfo.ReadOnly", false, bill.ABL_NotifyPartyLocalStateInfo.ReadOnly);
			});

			bill.ABL_OA_NotifyParty = orgAddress.PK;
			CombineAssertions(() =>
			{
				AssertEquals("bill.ABL_NotifyPartyLocalNameInfo.ReadOnly", true, bill.ABL_NotifyPartyLocalNameInfo.ReadOnly);
				AssertEquals("bill.ABL_NotifyPartyLocalStreet1Info.ReadOnly", true, bill.ABL_NotifyPartyLocalStreet1Info.ReadOnly);
				AssertEquals("bill.ABL_NotifyPartyLocalStreet2Info.ReadOnly", true, bill.ABL_NotifyPartyLocalStreet2Info.ReadOnly);
				AssertEquals("bill.ABL_NotifyPartyLocalCityInfo.ReadOnly", true, bill.ABL_NotifyPartyLocalCityInfo.ReadOnly);
				AssertEquals("bill.ABL_NotifyPartyLocalStateInfo.ReadOnly", true, bill.ABL_NotifyPartyLocalStateInfo.ReadOnly);
			});
		}

		public void TestSetNotifyPartyLocalAddressDefaultValues()
		{
			var header = CreateOrganizationForJobDocAddress();
			var bill = GetNewBusinessObject() as AsycudaBill;
			bill.ABL_OA_NotifyParty = header.MainAddress.PK;
			CombineAssertions(() =>
			{
				AssertEquals("綠晃科技股份有限公司", bill.ABL_NotifyPartyLocalName);
				AssertEquals("臺北加工出口區園東街6號", bill.ABL_NotifyPartyLocalStreet1);
				AssertEquals("5樓之1", bill.ABL_NotifyPartyLocalStreet2);
				AssertEquals("臺北巿", bill.ABL_NotifyPartyLocalCity);
				AssertEquals("TPE", bill.ABL_NotifyPartyLocalState);
			});

			bill.ABL_OA_NotifyParty = ZGuid.Empty;
			CombineAssertions(() =>
			{
				AssertEquals("綠晃科技股份有限公司", bill.ABL_NotifyPartyLocalName);
				AssertEquals("臺北加工出口區園東街6號", bill.ABL_NotifyPartyLocalStreet1);
				AssertEquals("5樓之1", bill.ABL_NotifyPartyLocalStreet2);
				AssertEquals("臺北巿", bill.ABL_NotifyPartyLocalCity);
				AssertEquals("TPE", bill.ABL_NotifyPartyLocalState);
			});

			bill.ABL_OA_NotifyParty = header.Addresses.Cast<OrgAddress>().FirstOrDefault(c => c.OA_CompanyNameOverride == "HAPPY CO., LTD.1").PK;
			CombineAssertions(() =>
			{
				AssertNullOrEmpty(bill.ABL_NotifyPartyLocalName);
				AssertNullOrEmpty(bill.ABL_NotifyPartyLocalStreet1);
				AssertNullOrEmpty(bill.ABL_NotifyPartyLocalStreet2);
				AssertNullOrEmpty(bill.ABL_NotifyPartyLocalCity);
				AssertNullOrEmpty(bill.ABL_NotifyPartyLocalState);
			});
		}

		public void TestNotifyPartyRegNoTypes()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			AssertContainsExactElementsInExactOrder(new ZString[] { OrgCusCode.CodeTypes.VATCode, OrgCusCode.CodeTypes.PassportID, OrgCusCode.TaiwanCodeTypes.PID }, bill.NotifyPartyRegNoTypes());
		}

		public void TestHeaderType()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			AssertType<AsycudaManifestHeader>(bill.Header);
		}

		public void TestABL_ManifestQty()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			bill.ABL_ManifestQty = 2;
			AssertEquals(2, bill.ABL_ManifestQty);
			bill.ABL_ManifestQty = -1;
			AssertEquals(0, bill.ABL_ManifestQty);
		}

		public void TestABL_GrossWeight()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			bill.ABL_GrossWeight = 2m;
			AssertEquals(2m, bill.ABL_GrossWeight);
			bill.ABL_GrossWeight = -1;
			AssertEquals(0m, bill.ABL_GrossWeight);
		}

		public void TestABL_Volume()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			bill.ABL_Volume = 2m;
			AssertEquals(2m, bill.ABL_Volume);
			bill.ABL_Volume = -1;
			AssertEquals(0m, bill.ABL_Volume);
		}

		public void TestABL_SplitQuantity()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			var arrivalHeader = Factory.LoadTop1<AsycudaArrivalHeader>(new ZQuery(AsycudaArrivalHeaderSchema.ATH_AMA_ManifestHeader, bill.Header.PK));
			var arrivalLine = Factory.LoadTop1<AsycudaArrivalLine>(new ZQuery(AsycudaArrivalLineSchema.ATL_ABL_AsycudaBill, bill.PK));
			CombineAssertions(() =>
			{
				AssertNull(arrivalHeader);
				AssertNull(arrivalLine);
				AssertEquals(ZInt.Zero, bill.ABL_SplitQuantity);
			});

			bill.ABL_SplitQuantity = 1;
			arrivalHeader = Factory.LoadTop1<AsycudaArrivalHeader>(new ZQuery(AsycudaArrivalHeaderSchema.ATH_AMA_ManifestHeader, bill.Header.PK));
			arrivalLine = Factory.LoadTop1<AsycudaArrivalLine>(new ZQuery(AsycudaArrivalLineSchema.ATL_ABL_AsycudaBill, bill.PK));
			CombineAssertions(() =>
			{
				AssertEquals(bill.Header.PK, arrivalHeader.ATH_AMA_ManifestHeader);
				AssertEquals(1, arrivalLine.ATL_Quantity);
				AssertEquals(1, bill.ABL_SplitQuantity);
			});
		}

		public void TestNegativeABL_SplitQuantity()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			bill.ABL_SplitQuantity = 1;
			AssertEquals(1, bill.ABL_SplitQuantity);

			bill.ABL_SplitQuantity = -1;
			AssertEquals(0, bill.ABL_SplitQuantity);
		}

		public void TestABL_SplitQuantity_Caption()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			var info = bill.ABL_SplitQuantityInfo;
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(info);
			AssertEquals("ABL_SplitQuantity Caption", "Split Quantity", resourceStringData.Caption);
		}

		public void TestABL_SplitQuantityUQ()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			bill.ABL_ManifestUQ = "ADT";
			AssertEquals("ADT", bill.ABL_SplitQuantityUQ);

			bill.ABL_ManifestUQ = "APZ";
			AssertEquals("APZ", bill.ABL_SplitQuantityUQ);
		}

		public void TestAsycudaBillLinkAsycudaContainers()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			var containers = bill.Header.Containers;
			var container1 = containers.AddNew();
			var container2 = containers.AddNew();
			AssertEquals(2, bill.AsycudaBillLinkAsycudaContainers.Count);
			var container3 = containers.AddNew();
			AssertEquals(3, bill.AsycudaBillLinkAsycudaContainers.Count);
			container2.Delete();
			AssertEquals(2, bill.AsycudaBillLinkAsycudaContainers.Count);
		}

		public void TestIsAsycudaBillLinkAsycudaContainersLoaded()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			AssertEquals(false, bill.IsAsycudaBillLinkAsycudaContainersLoaded);
			_ = bill.AsycudaBillLinkAsycudaContainers;
			AssertEquals(true, bill.IsAsycudaBillLinkAsycudaContainersLoaded);
		}

		public void TestBillLinkContainerDivots()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			var containers = bill.Header.Containers;
			var container1 = containers.AddNew();
			var container2 = containers.AddNew();
			var container3 = containers.AddNew();

			var asycudaContainerBillOrPackageLink1 = Factory.NewWithValidTestData<AsycudaContainerBillOrPackageLink>();
			asycudaContainerBillOrPackageLink1.APC_ABL_Bill = bill.PK;
			asycudaContainerBillOrPackageLink1.APC_ClusterKey = bill.ABL_ClusterKey;
			asycudaContainerBillOrPackageLink1.APC_ACN_Container = container1.PK;

			var asycudaContainerBillOrPackageLink2 = Factory.NewWithValidTestData<AsycudaContainerBillOrPackageLink>();
			asycudaContainerBillOrPackageLink2.APC_ABL_Bill = bill.PK;
			asycudaContainerBillOrPackageLink2.APC_ClusterKey = bill.ABL_ClusterKey;
			asycudaContainerBillOrPackageLink2.APC_ACN_Container = container2.PK;

			var billLinkContainerDivots = bill.BillLinkContainerDivots;
			AssertContainsExactElementsInAnyOrder(new[] { container1.PK, container2.PK }, billLinkContainerDivots.Cast<AsycudaContainerBillOrPackageLink>().Select(c => c.APC_ACN_Container));
		}

		public void TestBillLinkContainerDivots_LoadCorrectType()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			var containers = bill.Header.Containers;
			var container1 = containers.AddNew();
			var container2 = containers.AddNew();
			bill.LinkContainer(container1.PK);
			bill.LinkContainer(container2.PK);
			var pack = bill.Packs.AddNew();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var asycudaManifestHeaderLoaded = newFactory.Load<AsycudaManifestHeader>(bill.Header.PK);
			var billLoaded = asycudaManifestHeaderLoaded.Bills.Cast<AsycudaBill>().FirstOrDefault();
			billLoaded.RunPreSaveValidation();
			AssertNoExceptionThrown("Attempted to return a Enterprise.Customs.ManifestBase.AsycudaContainerBillOrPackageLink when a Enterprise.Customs.ASYCUDA.Business.AsycudaContainerBillOrPackageLink was requested.", () => billLoaded.BillLinkContainerDivots.RefreshBinding());
		}

		public void TestLinkContainer()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			var containers = bill.Header.Containers;
			var container1 = containers.AddNew();
			var container2 = containers.AddNew();
			bill.LinkContainer(container1.PK);
			bill.LinkContainer(container2.PK);
			Factory.Save();

			var asycudaContainerBillOrPackageLinkArray = Factory.Load<AsycudaContainerBillOrPackageLink>(new ZQuery(AsycudaContainerBillOrPackageLinkSchema.APC_ABL_Bill, bill.PK));
			AssertContainsExactElementsInAnyOrder(new[] { container1.PK, container2.PK }, asycudaContainerBillOrPackageLinkArray.Select(c => c.APC_ACN_Container));
		}

		public void TestUnlinkContainer()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			var containers = bill.Header.Containers;
			var container1 = containers.AddNew();
			var container2 = containers.AddNew();
			var container3 = containers.AddNew();

			var asycudaContainerBillOrPackageLink1 = Factory.NewWithValidTestData<AsycudaContainerBillOrPackageLink>();
			asycudaContainerBillOrPackageLink1.APC_ABL_Bill = bill.PK;
			asycudaContainerBillOrPackageLink1.APC_ClusterKey = bill.ABL_ClusterKey;
			asycudaContainerBillOrPackageLink1.APC_ACN_Container = container1.PK;

			var asycudaContainerBillOrPackageLink2 = Factory.NewWithValidTestData<AsycudaContainerBillOrPackageLink>();
			asycudaContainerBillOrPackageLink2.APC_ABL_Bill = bill.PK;
			asycudaContainerBillOrPackageLink2.APC_ClusterKey = bill.ABL_ClusterKey;
			asycudaContainerBillOrPackageLink2.APC_ACN_Container = container2.PK;

			var asycudaContainerBillOrPackageLink3 = Factory.NewWithValidTestData<AsycudaContainerBillOrPackageLink>();
			asycudaContainerBillOrPackageLink3.APC_ABL_Bill = bill.PK;
			asycudaContainerBillOrPackageLink3.APC_ClusterKey = bill.ABL_ClusterKey;
			asycudaContainerBillOrPackageLink3.APC_ACN_Container = container3.PK;

			var billLinkContainerDivots = bill.BillLinkContainerDivots;
			bill.UnlinkContainer(container2.PK);
			AssertContainsExactElementsInAnyOrder(new[] { container1.PK, container3.PK }, billLinkContainerDivots.Cast<AsycudaContainerBillOrPackageLink>().Select(c => c.APC_ACN_Container));
		}

		public void TestABL_Tariff()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			var pack = bill.Packs.AddNew();
			pack.PackedItem.API_Tariff = "87013090001";
			var info = bill.ABL_TariffInfo;
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(info);
			CombineAssertions(() =>
			{
				AssertEquals("ABL_Tariff Caption", "HS Code", resourceStringData.Caption);
				AssertHasCustomAttribute<ListAttribute>(typeof(AsycudaBill), bill.ABL_TariffInfo.Name, true, attribute => attribute.ListDataSourceMember == "Lookups.TariffCollection");
				AssertEquals("ABL_Tariff MaxLength", 35, bill.ABL_TariffInfo.MaxLength);
				AssertEquals("8701.30.90.00-1", bill.ABL_Tariff);
			});
		}

		public void TestUniversalTariff()
		{
			CreateTariffForTest();
			var bill = GetNewBusinessObject() as AsycudaBill;
			bill.ABL_Tariff = "01012100003";
			AssertEquals("01012100003", bill.UniversalTariff.ZZ1_TariffCode);

			bill.ABL_Tariff = "01012100005";
			AssertNull("UniversalTariff should be null because 01012100005 is not a HSN type.", bill.UniversalTariff);
		}

		void CreateTariffForTest()
		{
			var universalTestHelper = new UniversalReferenceTestDataHelper(Factory);
			var tariffTypeHSN = universalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "HSN");
			tariffTypeHSN.ZZI_Description = "Taiwan Harmonized Tariff";
			var atTariffType = universalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "AT");
			atTariffType.ZZI_Description = "Alcohol Tax";
			Factory.Save();
			universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffTypeHSN.PK, "01012100003", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffTypeHSN.PK, "01012100004", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, atTariffType.PK, "01012100005", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, atTariffType.PK, "01012100006", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();
		}

		public void TestIsZ99PortCode()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			CombineAssertions(() =>
			{
				Assert("TWTPE is not a Z99 port.", !bill.IsZ99PortCode("TWTPE"));
				Assert("TWZ99 is a Z99 port.", bill.IsZ99PortCode("TWZ99"));
				Assert("Z99 is not a Z99 port.", !bill.IsZ99PortCode("Z99"));
			});
		}

		public void TestABL_DG_UNNO()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			bill.Packs.RemoveAndDeleteAll();
			var pack = bill.Packs.AddNew();
			pack.UNDGs.UNDGSubstanceManager.Value = "0004";
			var info = bill.ABL_DG_UNNOInfo;
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(info);
			CombineAssertions(() =>
			{
				AssertEquals("ABL_DG_UNNO Caption", "DG Code", resourceStringData.Caption);
				AssertEquals("0004", bill.ABL_DG_UNNO);
				AssertEquals(6, info.MaxLength);
			});

			bill.ABL_DG_UNNO = "0006";
			AssertEquals("0006", bill.ABL_DG_UNNO);
		}

		public void TestABL_LocationInformation()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			bill.ABL_LocationInformation = "Test";
			bill.ABL_RL_NKPortOfLoading = "TWKEL";
			AssertEquals("Should be empty if the last three characters are not Z99.", ZString.Empty, bill.ABL_LocationInformation);

			bill.ABL_LocationInformation = "Test";
			bill.ABL_RL_NKPortOfLoading = "KEL";
			AssertEquals("Should be empty if ABL_RL_NKPortOfLoading is not 5 characters.", ZString.Empty, bill.ABL_LocationInformation);
		}

		public void TestABL_LocationInformation_ReadOnly()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			bill.ABL_RL_NKPortOfLoading = "TWKEL";
			CombineAssertions(() =>
			{
				AssertEquals(true, bill.ABL_LocationInformation_ReadOnly);
				Assert("Should be readonly.", bill.ABL_LocationInformationInfo.ReadOnly);
			});

			bill.ABL_RL_NKPortOfLoading = "KEL";
			CombineAssertions(() =>
			{
				AssertEquals(true, bill.ABL_LocationInformation_ReadOnly);
				Assert("Should be readonly.", bill.ABL_LocationInformationInfo.ReadOnly);
			});

			bill.ABL_RL_NKPortOfLoading = "TWZ99";
			CombineAssertions(() =>
			{
				AssertEquals(false, bill.ABL_LocationInformation_ReadOnly);
				Assert("Should not be readonly.", !bill.ABL_LocationInformationInfo.ReadOnly);
			});
		}

		public void TestABL_LocationInformation_MaxLength()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			AssertEquals(70, bill.ABL_LocationInformationInfo.MaxLength);
		}

		public void TestABL_GoodsLocation_MaxLength()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			AssertEquals(8, bill.ABL_GoodsLocationInfo.MaxLength);
		}

		public void TestIsEscortRequired_Caption()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			var info = bill.IsEscortRequiredInfo;
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(info);
			AssertEquals("ABL_SpecialCargoCode Caption", "Is Escort Required?", resourceStringData.Caption);
		}

		public void TestDefaultValues()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			AssertEquals("KG", bill.ABL_GrossWeightUQ);
			AssertEquals("M3", bill.ABL_VolumeUQ);
			AssertEquals(TWManifestShipmentTypes.Codes.Import, bill.ABL_ShipmentType);
		}

		public void TestDelete()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			bill.ABL_SplitQuantity = 1;
			Factory.Save();
			var arrivalLine = Factory.LoadTop1<AsycudaArrivalLine>(new ZQuery(AsycudaArrivalLineSchema.ATL_ABL_AsycudaBill, bill.PK));
			AssertNotNull(arrivalLine);

			bill.Delete();
			Factory.Save();
			arrivalLine = Factory.LoadTop1<AsycudaArrivalLine>(new ZQuery(AsycudaArrivalLineSchema.ATL_ABL_AsycudaBill, bill.PK));
			AssertNull(arrivalLine);
		}

		public void TestCustomsEntryNumber_ReadOnly()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			AssertEquals("Entry Number should always be read-only.", true, bill.CustomsEntryNumberInfo.ReadOnly);
		}

		public void TestCustomsEntryNumber_Caption()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(bill.CustomsEntryNumberInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Entry Number", resourceStringData.Caption);
				AssertEquals("Short Caption", "Entry #", resourceStringData.ShortCaption);
			});
		}

		public void TestAsycudaPackType()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			var pack = bill.Packs.AddNew();
			pack.APA_GoodsDescription = "Pack1";
			Factory.Save();

			var loadedbill = Factory.Load<AsycudaBill>(bill.PK);
			AssertType<AsycudaPack>(loadedbill.Packs.FirstOrDefault());
		}

		public void TestAsycudaPackCollection()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			var collection = bill.Packs;
			CombineAssertions(() =>
			{
				AssertType<AsycudaPack>(collection.AddNew());
				AssertType<AsycudaBill>(collection.Master);
			});
		}

		OrgHeader CreateOrganizationForJobDocAddress()
		{
			var header = Factory.New<OrgHeader>();
			header.OH_Code = "OrgTest1";
			header.OH_RL_NKClosestPort = "TW";
			header.OH_IsConsignee = true;

			var mainAddress = header.MainAddress;
			mainAddress.OA_CompanyNameOverride = "HAPPY CO., LTD.";
			mainAddress.OA_Language = Core.SharedConstants.Languages.English;
			mainAddress.UnrestrictedAdditionalAddressInformation = "addinfo address";
			mainAddress.OA_Address1 = "1500 HAPPY RD";
			mainAddress.OA_Address2 = "ORANGE DISTRICT";
			mainAddress.OA_RN_NKCountryCode = "TW";
			mainAddress.OA_City = "APPLE CITY";
			mainAddress.Postcode = "12345";
			mainAddress.OA_State = "TPE";
			mainAddress.OA_Phone = "+1 (273) 5495200";
			mainAddress.OA_Email = "001@xx.com";
			mainAddress.OA_Fax = "001FAX";

			var zhTWtranslatedAddress1 = mainAddress.TranslatedAddresses.AddNew();
			zhTWtranslatedAddress1.OTA_Language = Enterprise.Core.SharedConstants.Languages.ChineseTraditional;
			zhTWtranslatedAddress1.UnrestrictedAdditionalAddressInformation = "附加信息2";
			zhTWtranslatedAddress1.OTA_CompanyName = "綠晃科技股份有限公司";
			zhTWtranslatedAddress1.OTA_Address1 = "臺北加工出口區園東街6號";
			zhTWtranslatedAddress1.OTA_Address2 = "5樓之1";
			zhTWtranslatedAddress1.OTA_City = "臺北巿";
			zhTWtranslatedAddress1.OTA_PostCode = "90093";
			zhTWtranslatedAddress1.OTA_State = "TPE";
			zhTWtranslatedAddress1.ClosestPort = "TW";

			var addresse = header.Addresses.AddNew();
			addresse.OA_CompanyNameOverride = "HAPPY CO., LTD.1";
			addresse.UnrestrictedAdditionalAddressInformation = "addinfo address1";
			addresse.OA_Language = Core.SharedConstants.Languages.English;
			addresse.OA_Address1 = "004 HAPPY RD";
			addresse.OA_Address2 = "ORANGE DISTRICT1";
			addresse.OA_RN_NKCountryCode = "TW";
			addresse.OA_City = "APPLE CITY1";
			addresse.Postcode = "001";
			addresse.OA_State = "TPE";
			addresse.OA_Phone = "+2 (273) 5495200";
			addresse.OA_Email = "002@xx.com";
			addresse.OA_Fax = "002FAX";
			Factory.Save();
			return header;
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

		#endregion
	}
}

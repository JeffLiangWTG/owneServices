using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Universal.Helper;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.Asycuda;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.SG.Access.Business.Testing
{
	[TestedType(typeof(AsycudaBill))]
	sealed class AsycudaBillTest : ManifestBase.Testing.AsycudaBillTest
	{
		public void TestIsIBGAccountLinked()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ConsigneeOrgPK = consignee.PK;
			bill.ABL_ShipmentType = ShipmentTypeList.Codes.Export22;
			Factory.InvalidateCachedProperties();
			Assert("Should be false as the shipment type is not import.", !bill.IsIBGAccountLinked);
			bill.ABL_ShipmentType = ShipmentTypeList.Codes.Import23;
			Factory.InvalidateCachedProperties();
			Assert("Should be false as the linked consignee doesnt have a valid IBG number.", !bill.IsIBGAccountLinked);
			consignee.MainAddress.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.InterbankGIRO, "201101", Core.Constants.CountryCodes.Singapore);
			Factory.InvalidateCachedProperties();
			Assert("Should be true as the shipment type is import and linked consignee have a valid IBG number.", bill.IsIBGAccountLinked);
		}

		public void TestABL_BillStatus()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packItem = pack.PackedItem;
			var entryNumber1 = packItem.CustomsEntryNumbers.AddNew();
			entryNumber1.CE_EntryType = "ASY";
			entryNumber1.CE_EntryNum = "1";
			var entryNumber2 = packItem.CustomsEntryNumbers.AddNew();
			entryNumber2.CE_EntryType = "TNP";
			entryNumber2.CE_EntryNum = "2";
			AssertEquals(2, packItem.CustomsEntryNumbers.Count);
			bill.ABL_BillStatus = Common.SG.GlobalManifestStatusList.Codes.Clear;
			AssertEquals(2, packItem.CustomsEntryNumbers.Count);
			bill.ABL_BillStatus = Common.SG.GlobalManifestStatusList.Codes.Cancelled;
			AssertEquals(1, packItem.CustomsEntryNumbers.Count);
			AssertEquals("TNP", packItem.CustomsEntryNumbers[0].CE_EntryType);
		}

		public void TestIAsycudaBill()
		{
			var bizObj = GetNewBusinessObject();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.SGAccess.IAsycudaBill>(bizObj.PK).GetType());
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<ASYCUDA.Business.AsycudaBill>(bizObj.PK).GetType());
		}

		public void TestUpdatePackLinePriceCurrency()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_RX_NKFreightValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			var pack1 = bill.Packs.AddNew();
			AssertEquals(pack1.LinePriceCurrency, Core.Constants.CurrencyCodes.UnitedStates);
			bill.ABL_RX_NKFreightValueCurrency = Core.Constants.CurrencyCodes.Australia;
			var pack2 = bill.Packs.AddNew();
			AssertEquals(pack1.LinePriceCurrency, Core.Constants.CurrencyCodes.Australia);
			AssertEquals(pack2.LinePriceCurrency, Core.Constants.CurrencyCodes.Australia);
		}

		public void TestDefaultSG_PartyIDFromConsigneeSG_UEN()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "AUUEN00001", Core.Constants.CountryCodes.Australia);
			consignee.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "SGUEN00001", Core.Constants.CountryCodes.Singapore);
			consignee.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SingaporeCodeTypes.PartyStatusType, SGPartyStatusList.Codes.N, Core.Constants.CountryCodes.Singapore);
			var bill = (AsycudaBill)GetNewBusinessObject();
			bill.ABL_ShipmentType = ShipmentTypeList.Codes.Import23;
			bill.ABL_OA_Consignee = consignee.MainAddress.PK;
			AssertEquals("bill.SG_PartyID", "SGUEN00001", bill.SG_PartyID);
			AssertEquals("bill.SG_PartyStatus", "N", bill.SG_PartyStatus);
			bill.ABL_OA_Consignee = ZGuid.Empty;
			bill.SG_PartyID = ZString.Empty;
			bill.ABL_ShipmentType = ShipmentTypeList.Codes.Export22;
			bill.ABL_OA_Consignee = consignee.MainAddress.PK;
			AssertEquals("bill.SG_PartyID", ZString.Empty, bill.SG_PartyID);
		}

		public void TestDefaultPartyStatus()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SingaporeCodeTypes.PartyStatusType, "AISS", Core.Constants.CountryCodes.Singapore);
			var bill = (AsycudaBill)GetNewBusinessObject();
			bill.ABL_ShipmentType = ShipmentTypeList.Codes.Import23;
			bill.ABL_OA_Consignee = consignee.MainAddress.PK;
			AssertEquals("bill.SG_PartyStatus should have been truncated to 'A', the valid length", SGPartyStatusList.Codes.A, bill.SG_PartyStatus);
		}

		public void TestDefaultSGDataFromShipper()
		{
			var sgRegistry = ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>();
			sgRegistry.ACCESSEnable.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var shipper = Factory.NewWithValidTestData<OrgHeader>();
			shipper.OH_Code = "SUP3234";
			shipper.MiscServ.OM_IMPaymentMethod = SGPayeeIndicatorList.Codes.T;
			shipper.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "AUUEN00001", Core.Constants.CountryCodes.Australia);
			shipper.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "SGUEN00001", Core.Constants.CountryCodes.Singapore);
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "OWN3234";
			consignee.MiscServ.OM_IMPaymentMethod = SGPayeeIndicatorList.Codes.Q;
			consignee.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "AUUEN00002", Core.Constants.CountryCodes.Australia);
			consignee.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "SGUEN00002", Core.Constants.CountryCodes.Singapore);
			Factory.Save();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RL_NKPortOfLoading = "SGSIN";
			header.AMA_RL_NKPortOfDischarge = "AUSYD";
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Singapore;
			var bill = header.Bills.AddNew();
			bill.ABL_RL_NKOrigin = "SGSIN";
			bill.ABL_RL_NKFinalDestination = "AUSYD";
			bill.ABL_ShipmentType = ShipmentTypeList.Codes.Import23;
			bill.ABL_OA_Shipper = shipper.MainAddress.PK;
			AssertEquals("bill.SG_PartyID", "", bill.SG_PartyID);
			AssertEquals("bill.SG_PayeeIndicator", "", bill.SG_PayeeIndicator);
			bill.ABL_OA_Shipper = ZGuid.Empty;
			bill.ABL_ShipmentType = ShipmentTypeList.Codes.Export22;
			bill.ABL_OA_Shipper = shipper.MainAddress.PK;
			AssertEquals("bill.SG_PartyID", "SGUEN00001", bill.SG_PartyID);
			AssertEquals("bill.SG_PayeeIndicator", "", bill.SG_PayeeIndicator);
		}

		public void TestValidation()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var child = header.MasterBill;
			var realBill = header.Bills.AddNew();
			AssertType<AsycudaBillValidationForRegularBill>(realBill.Validation);
			AssertType<AsycudaBillValidationForMasterChild>(child.Validation);
		}

		public void TestISelectionItem_SelectionDescription_CycleFields()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = Business.Constants.ManifestType.Import;
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "Bill1";
			bill.ABL_ShipmentType = ShipmentTypeList.Codes.Import23;
			bill.CycleDate = new ZDateTime(2018, 6, 7);
			bill.CycleNumber = "2";
			AssertContains("Bill Number - Bill1 - Cycle: 07-Jun-18/2", ((ISelectionItem)bill).SelectionDescription(true));
			AssertNotContains("Cycle: 07-Jun-18/2", ((ISelectionItem)bill).SelectionDescription(false));
		}

		public void TestPacksAreReapportionWhenBillIsSaved()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = Business.Constants.ManifestType.Import;
			var packCountries = new List<AsycudaPackedItem>();
			for (var i = 0; i < 5; i++)
			{
				var bill = header.Bills.AddNew();
				bill.FillWithValidTestData();
				bill.ABL_BillNumber = i.ToString("0000");
				bill.ABL_TransportValue = 10;
				bill.ABL_InsuranceValue = 20m;
				bill.OtherChargesValue = 30m;
				bill.DiscountValue = 40m;
				bill.ABL_CustomsValue = 0m;
				bill.ABL_RX_NKTransportValueCurrency = Core.Constants.CurrencyCodes.Singapore;
				bill.ABL_RX_NKInsuranceValueCurrency = Core.Constants.CurrencyCodes.Singapore;
				bill.OtherChargesValueCurrency = Core.Constants.CurrencyCodes.Singapore;
				bill.DiscountValueCurrency = Core.Constants.CurrencyCodes.Singapore;
				bill.DutyAmount = 0m;
				bill.TaxAmount = 0m;
				var pack = bill.Packs.AddNew();
				pack.LinePrice = 15m;
				pack.LinePriceCurrency = Core.Constants.CurrencyCodes.Singapore;
				var packedItem = pack.PackedItem;
				packedItem.API_CustomsValue = 999m;
				packCountries.Add(packedItem);
			}

			CombineAssertions(() =>
			{
				foreach (var country in packCountries)
				{
					AssertEquals("Should be true as many related values are changed.", true, country.Pack.Bill.ApportionmentDirty);
					AssertEquals(999m, country.API_CustomsValue);
				}
			});
			Factory.Save();
			CombineAssertions(() =>
			{
				foreach (var country in packCountries)
				{
					AssertEquals("Should be false after the calculation.", false, country.Pack.Bill.ApportionmentDirty);
					AssertEquals("15 + (30 + 10 + 20 - 40) * 1", 35m, country.API_CustomsValue);
				}
			});
			var newFactory = NewFactory();
			var query = new ZQuery(AsycudaPackedItemSchema.PK, packCountries.Select(c => c.PK));
			packCountries = newFactory.Load<AsycudaPackedItem>(query).ToList();
			CombineAssertions(() =>
			{
				foreach (var country in packCountries)
				{
					AssertEquals("Should default to false.", false, country.Pack.Bill.ApportionmentDirty);
					AssertEquals("Should are saved.", 35m, country.API_CustomsValue);
				}
			});
		}

		public void TestISetterSuspenderSupporterMembers()
		{
			var bill = Factory.New<AsycudaBill>();
			ISetterSuspenderSupporter supporter = bill;
			var supportedFields = supporter.SupportedFields.ToArray();
			AssertEquals(1, supportedFields.Length);
			AssertEquals(AsycudaBill.Schema.SG_PartyID, supportedFields[0]);
		}

		public void TestCycleFields_Readonly()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			AssertEquals(false, bill.CycleFields_Readonly);
			bill.RegistrationDate = ZDateTime.Today;
			AssertEquals(true, bill.CycleFields_Readonly);
		}

		public void TestCustomBusinessObject()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var customBusinessObject = ((ICustomFieldProvider)bill).GetCustomBusinessObject();
			bill.RegisterEditableChildObject(customBusinessObject);
			bill.SetReadOnlyIncludingChildren(true);
			Assert(!customBusinessObject.ReadOnly);
		}

		public void TestCycleDate()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.CycleDate = new ZDateTime(2017, 9, 1);
			AssertEquals(new ZDateTime(2017, 9, 1), bill.CycleDate);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			bill = newFactory.Load<AsycudaBill>(bill.PK);
			AssertEquals(new ZDateTime(2017, 9, 1), bill.CycleDate);
			bill.CycleDate = new ZDateTime(2017, 9, 5, 14, 30, 21);
			AssertEquals(new ZDateTime(2017, 9, 5), bill.CycleDate);
			bill.CycleDate = ZDateTime.Empty;
			AssertEquals(ZDateTime.Empty, bill.CycleDate);
			bill.CycleDate = ZDateTime.Invalid;
			AssertEquals(ZDateTime.Invalid, bill.CycleDate);
		}

		public void TestCycleNumber()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.CycleNumber = "1";
			AssertEquals("1", bill.CycleNumber);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			bill = newFactory.Load<AsycudaBill>(bill.PK);
			AssertEquals("1", bill.CycleNumber);
		}

		public void TestDefaultSGDataIfNeeded()
		{
			var sgRegistry = ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>();
			sgRegistry.ACCESSEnable.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var shipper = Factory.NewWithValidTestData<OrgHeader>();
			shipper.OH_Code = "SUP3234";
			shipper.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "AUUEN00001", Core.Constants.CountryCodes.Australia);
			shipper.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "SGUEN00001", Core.Constants.CountryCodes.Singapore);
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "OWN3234";
			consignee.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "AUUEN00002", Core.Constants.CountryCodes.Australia);
			consignee.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "SGUEN00002", Core.Constants.CountryCodes.Singapore);
			consignee.MiscServ.OM_IMPaymentMethod = SGPayeeIndicatorList.Codes.Q;
			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RL_NKPortOfLoading = "SGSIN";
			header.AMA_RL_NKPortOfDischarge = "AUSYD";
			var bill = header.Bills.AddNew();
			bill.ABL_OA_Shipper = shipper.MainAddress.PK;
			bill.ABL_OA_Consignee = consignee.MainAddress.PK;
			bill.ABL_RL_NKOrigin = "SGSIN";
			bill.ABL_RL_NKFinalDestination = "AUSYD";
			bill.GSTNReferenceNo = "12312AB";
			bill.SG_PartyStatus = SGPartyStatusList.Codes.A;
			bill.SG_PartyID = ZString.Empty;
			bill.SG_PayeeIndicator = ZString.Empty;
			bill.ABL_ShipmentType = ShipmentTypeList.Codes.Transhipment28;
			AssertSGData(bill, ZString.Empty, ZString.Empty, ZString.Empty);
			AssertEquals("bill.SG_PartyIDInfo.ReadOnly", true, bill.SG_PartyIDInfo.ReadOnly);
			AssertEquals("bill.SG_PartyStatusInfo.ReadOnly", true, bill.SG_PartyStatusInfo.ReadOnly);
			AssertEquals("bill.SG_PayeeIndicatorInfo.ReadOnly", true, bill.SG_PayeeIndicatorInfo.ReadOnly);
			AssertEquals("bill.GSTNReferenceNo", ZString.Empty, bill.GSTNReferenceNo);
			bill.SG_PartyStatus = SGPartyStatusList.Codes.A;
			bill.GSTNReferenceNo = "12312AB";
			bill.ABL_ShipmentType = ShipmentTypeList.Codes.Import23;
			AssertSGData(bill, "SGUEN00002", SGPartyStatusList.Codes.A, SGPayeeIndicatorList.Codes.Q);
			AssertEquals("bill.SG_PartyIDInfo.ReadOnly", false, bill.SG_PartyIDInfo.ReadOnly);
			AssertEquals("bill.SG_PartyStatusInfo.ReadOnly", false, bill.SG_PartyStatusInfo.ReadOnly);
			AssertEquals("bill.SG_PayeeIndicatorInfo.ReadOnly", false, bill.SG_PayeeIndicatorInfo.ReadOnly);
			AssertEquals("bill.GSTNReferenceNo", "12312AB", bill.GSTNReferenceNo);
			bill.ABL_ShipmentType = ShipmentTypeList.Codes.Export22;
			AssertSGData(bill, "SGUEN00001", ZString.Empty, ZString.Empty);
			AssertEquals("bill.SG_PartyIDInfo.ReadOnly", false, bill.SG_PartyIDInfo.ReadOnly);
			AssertEquals("bill.SG_PartyStatusInfo.ReadOnly", true, bill.SG_PartyStatusInfo.ReadOnly);
			AssertEquals("bill.SG_PayeeIndicatorInfo.ReadOnly", true, bill.SG_PayeeIndicatorInfo.ReadOnly);
			AssertEquals("bill.GSTNReferenceNo", ZString.Empty, bill.GSTNReferenceNo);
			bill.ABL_ShipmentType = ShipmentTypeList.Codes.Import23;
			bill.SG_PartyID = ZString.Empty;
			using (bill.SetterSuspender.SuspendSetting(AsycudaBill.Schema.SG_PartyID))
			{
				bill.SG_PartyID = "1";
				AssertEquals(ZString.Empty, bill.SG_PartyID);
			}
		}

		public void TestStatusDescription()
		{
			CombineAssertions(() =>
			{
				AssertStatusDescription(MessageStatusCodeList.Codes.NotSent, MessageStatusCodeList.Descriptions.NotSent);
				AssertStatusDescription(MessageStatusCodeList.Codes.Unknown, MessageStatusCodeList.Descriptions.Unknown);
				AssertStatusDescription(MessageStatusCodeList.Codes.Registered, MessageStatusCodeList.Descriptions.Registered);
				AssertStatusDescription(MessageStatusCodeList.Codes.Sent, MessageStatusCodeList.Descriptions.Sent);
				AssertStatusDescription(MessageStatusCodeList.Codes.Awaiting, MessageStatusCodeList.Descriptions.Awaiting);
				AssertStatusDescription(MessageStatusCodeList.Codes.Updated, MessageStatusCodeList.Descriptions.Updated);
				AssertStatusDescription(MessageStatusCodeList.Codes.Accepted, MessageStatusCodeList.Descriptions.Accepted);
				AssertStatusDescription(MessageStatusCodeList.Codes.Error, "ERR - R01 COUNTRY CODE/REGION/ORIGIN OF GOODS IS INVALID");
			});
		}

		public void TestDefaultSG_PayeeIndicatorWhenCompanyDataNotExist()
		{
			var sgRegistry = ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>();
			sgRegistry.ACCESSEnable.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "CON3234";
			Factory.Save();
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_RL_NKPortOfLoading = "SGSIN";
			header.AMA_RL_NKPortOfDischarge = "AUSYD";
			var bill = header.Bills.AddNew();
			bill.SG_PartyStatus = SGPartyStatusList.Codes.A;
			bill.SG_PartyID = ZString.Empty;
			bill.SG_PayeeIndicator = ZString.Empty;
			bill.ABL_ShipmentType = ShipmentTypeList.Codes.Import23;
			bill.ABL_OA_Consignee = consignee.MainAddress.PK;
			var newFactory = new BusinessObjectFactory();
			var consigneeInNewFactory = newFactory.Load<OrgHeader>(consignee.PK);
			consigneeInNewFactory.MainAddress.Address1 = "test";
			newFactory.Save();
			bool saveFailed = false;
			try
			{
				Factory.Save();
			}
			catch
			{
				saveFailed = true;
			}

			Assert("manifest is saved", !saveFailed);
			var consignee2 = Factory.New<OrgHeader>();
			consignee2.OH_Code = "CON2222";
			consignee2.CompanyData.OB_AREftCustomsPaymentMethod = SGPayeeIndicatorList.Codes.Q;
			bill.ABL_OA_Consignee = consignee2.MainAddress.PK;
			Factory.Save();
			bill.Reload();
			AssertEquals("SG_PayeeIndicator is defaulted", SGPayeeIndicatorList.Codes.Q, bill.SG_PayeeIndicator);
		}

		public void TestSGPartyStatusUpdatesPackGoodsType()
		{
			var sgRegistry = ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>();
			sgRegistry.ACCESSEnable.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Factory.Save();
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_RL_NKPortOfLoading = "AUSYD";
			header.AMA_RL_NKPortOfDischarge = "SGSIN";
			header.AMA_ManifestType = Constants.ManifestType.Import;
			var bill = header.Bills.AddNew();
			bill.ABL_RL_NKOrigin = "AUSYD";
			bill.ABL_RL_NKFinalDestination = "SGSIN";
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItem;
			packedItem.GoodsType = Constants.GoodsType.NormalGoods;
			var pack2 = bill.Packs.AddNew();
			var packedItem2 = pack2.PackedItem;
			packedItem2.GoodsType = Constants.GoodsType.NormalGoods;
			bill.SG_PartyStatus = SGPartyStatusList.Codes.Y;
			AssertEquals(Constants.GoodsType.MajorExporter, packedItem.GoodsType);
			AssertEquals(Constants.GoodsType.MajorExporter, packedItem2.GoodsType);
			bill.SG_PartyStatus = SGPartyStatusList.Codes.A;
			AssertEquals(Constants.GoodsType.NormalGoods, packedItem.GoodsType);
			AssertEquals(Constants.GoodsType.NormalGoods, packedItem2.GoodsType);
			header.AMA_ManifestType = Constants.ManifestType.Export;
			bill.SG_PartyStatus = SGPartyStatusList.Codes.Y;
			AssertEquals(Constants.GoodsType.NormalGoods, packedItem.GoodsType);
			AssertEquals(Constants.GoodsType.NormalGoods, packedItem2.GoodsType);
		}

		public void TestShortStatusDescription()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			AssertStatusDescription(header, MessageStatusCodeList.Codes.NotSent, "Not Sent");
			AssertStatusDescription(header, MessageStatusCodeList.Codes.Unknown, "Unknown");
			AssertStatusDescription(header, MessageStatusCodeList.Codes.Sent, "Sent");
			AssertStatusDescription(header, MessageStatusCodeList.Codes.Awaiting, "Awaiting");
			AssertStatusDescription(header, MessageStatusCodeList.Codes.Updated, "Updated");
			AssertStatusDescription(header, MessageStatusCodeList.Codes.Accepted, "Accepted");
		}

		public void TestShortStatusDescription_ERR()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();
			bill1.ABL_MessageStatus = MessageStatusCodeList.Codes.Error;
			bill1.Logs.AddNew(Events.MessageStatusChange, "Invalid Error Message");
			AssertEquals("ERROR RECEIVED", bill1.ShortStatusDescription);
			var bill2 = header.Bills.AddNew();
			bill2.ABL_MessageStatus = MessageStatusCodeList.Codes.Error;
			bill2.Logs.AddNew(Events.MessageStatusChange, "ERR - R01 Message Error 1, Next Message Error");
			AssertEquals("R01 Message Error 1, Next Message Error", bill2.ShortStatusDescription);
			var bill3 = header.Bills.AddNew();
			bill3.ABL_MessageStatus = MessageStatusCodeList.Codes.Error;
			bill3.Logs.AddNew(Events.MessageStatusChange, "ERR - R01 Message Error 3, Next Message Error - R02 Message Error 2");
			AssertEquals("R01 Message Error 3, Next Message Error...", bill3.ShortStatusDescription);
		}

		public void TestGSTNReferenceNo()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.GSTNReferenceNo = "1A2B3C";
			AssertEquals("1A2B3C", bill.GSTNReferenceNo);
			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			bill = factory2.Load<AsycudaBill>(bill.PK);
			AssertEquals("1A2B3C", bill.GSTNReferenceNo);
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.SuspendCheckBusinessObjectType();
			var bill = header.Bills.AddNew();
			return bill;
		}

		void AssertSGData(AsycudaBill bill, ZString partyID, ZString partyStatus, ZString payeeIndicator)
		{
			AssertEquals("bill.SG_PartyID", partyID, bill.SG_PartyID);
			AssertEquals("bill.SG_PartyStatus", partyStatus, bill.SG_PartyStatus);
			AssertEquals("bill.SG_PayeeIndicator", payeeIndicator, bill.SG_PayeeIndicator);
		}

		void AssertStatusDescription(ZString statusCode, ZString expectedResult)
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = Constants.ManifestType.Import;
			var bill = header.Bills.AddNew();
			bill.ABL_RL_NKOrigin = "AUSYD";
			bill.ABL_RL_NKFinalDestination = "SGSIN";
			bill.Logs.AddNew(Events.MessageStatusChange, "ERR - NOT THE MOST RECENT ERROR", new ZDateTimeOffset(2018, 3, 11));
			bill.Logs.AddNew(Events.MessageStatusChange, "ERR - R01 COUNTRY CODE/REGION/ORIGIN OF GOODS IS INVALID", new ZDateTimeOffset(2018, 3, 12));
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			bill.Logs.AddNew(Events.EditedARecord, "Edited", new ZDateTimeOffset(2018, 3, 13));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			bill.ABL_MessageStatus = statusCode;
			AssertEquals("Status for code " + statusCode, expectedResult, bill.StatusDescription);
		}

		void AssertStatusDescription(AsycudaManifestHeader header, string statusCode, string expectedDescription)
		{
			var bill = header.Bills.AddNew();
			bill.ABL_MessageStatus = statusCode;
			AssertEquals(expectedDescription, bill.ShortStatusDescription);
		}
	}
}

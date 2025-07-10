using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Business.Testing;
using Enterprise.Customs.TW.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Customs.TW.Business.Constants;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeader))]
	sealed class AsycudaManifestHeaderTest : ASYCUDA.Business.Testing.AsycudaManifestHeaderAbstractTest
	{
		public void TestAMA_DeconsolidateVAT()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "96944490", Core.Constants.CountryCodes.Taiwan);
			Factory.Save();

			ManifestHeader.AMA_OA_DeconsolidateAddress_ZAddress.OrgPK = org.PK;
			CombineAssertions(() =>
			{
				BusinessObjectCaptionTestHelper.AssertCaptions(ManifestHeader.AMA_DeconsolidateVATInfo, "De-consolidator VAT", "VAT");
				AssertEquals("De-consolidator VAT number should be 96944490", "96944490", ManifestHeader.AMA_DeconsolidateVAT);
			});
		}

		[TestDate(2023, 03, 06)]
		public void TestSetDefaultValues_AMA_CustomsOffice()
		{
			new TestTWCreator(Factory).CreateRegistryItemCusCustomsOffice();
			AssertEquals("AMA_CustomsOffice", "CE", ManifestHeader.AMA_CustomsOffice);
		}

		public void TestAMA_LoginCompanyPK()
		{
			CombineAssertions(() =>
			{
				BusinessObjectCaptionTestHelper.AssertCaptions(ManifestHeader.AMA_LoginCompanyPKInfo, "Company");
				AssertEquals(GlbCompany.CurrentCompany.PK, ManifestHeader.AMA_LoginCompanyPK);
				AssertHasCustomAttribute<ListAttribute>(ManifestHeader.GetType(), "AMA_LoginCompanyPK", false, attrib => attrib.ListDataSourceMember == "Lookups.Companies");
			});
		}

		public void TestAMA_MailBox()
		{
			var credential = Factory.NewWithValidTestData<GlbCompanyCredential>();
			credential.GP_PasswordType = "TVF";
			credential.GP_MailBoxID = "12345";
			credential.GP_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			CombineAssertions(() =>
			{
				BusinessObjectCaptionTestHelper.AssertCaptions(ManifestHeader.AMA_MailBoxInfo, "Mail Box");
				AssertEquals("12345", ManifestHeader.AMA_MailBox);
			});
		}

		public void TestForwarderCredential()
		{
			var credential = Factory.NewWithValidTestData<GlbCompanyCredential>();
			credential.GP_PasswordType = "TVF";
			credential.GP_UserID = "1";
			credential.GP_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();
			AssertEquals(credential.PK, ManifestHeader.ForwarderCredential.PK);

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.Canada))
			{
				var manifestHeader = new BusinessObjectFactory().NewWithValidTestData<AsycudaManifestHeader>();
				AssertNull(manifestHeader.ForwarderCredential);
			}
		}

		[TestDate(2023, 03, 06)]
		public void TestSetDefaultValues_AMA_GoodsLocationFromMasterBill()
		{
			var factory = Factory;
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "BA", "Taipei office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CE", "TaoYuan office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Facilities");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.CustomsOffice, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, Core.Constants.CountryCodes.Taiwan);
			var facility = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "ANP0060D", "XXXXXX", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			facility.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.CustomsOffice, "BA");
			facility = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "ANP0061D", "XXXXXX", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			facility.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.CustomsOffice, "CE");
			factory.Save();

			var goodsLocations = new CusGoodsLocationCollection();
			var goodsLocation = goodsLocations.AddNew();
			goodsLocation.MessageType = "IMP";
			goodsLocation.CustomsOffice = "BA";
			goodsLocation.GoodsLocation = "ANP0060D";
			goodsLocation = goodsLocations.AddNew();
			goodsLocation.MessageType = "IMP";
			goodsLocation.CustomsOffice = "CE";
			goodsLocation.GoodsLocation = "ANP0061D";
			using (TWCustomsDataRegistry.Instance.CusGoodsLocation.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, goodsLocations))
			{
				var customsOfficeRegistryItem = TWCustomsDataRegistry.Instance.CusCustomsOffice;
				using (customsOfficeRegistryItem.DataType.SuspendValidation())
				{
					var customsOffice = new CusCustomsOffice();
					customsOffice.CustomsOfficeCode = "BA";
					using (customsOfficeRegistryItem.SetTemporaryValue(Guid.Empty, Env.CurrentBranchPK, Env.CurrentDepartmentPK, customsOffice))
					{
						AssertEquals("Default when AsycudaManifestHeader is instantiated", "ANP0060D", ManifestHeader.AMA_GoodsLocationFromMasterBill);
					}

					customsOffice.CustomsOfficeCode = "CE";
					using (customsOfficeRegistryItem.SetTemporaryValue(Guid.Empty, Env.CurrentBranchPK, Env.CurrentDepartmentPK, customsOffice))
					{
						ManifestHeader.AMA_GoodsLocationFromMasterBill = ZString.Empty;
						ManifestHeader.AMA_CustomsOffice = "CE";
						AssertEquals("Default when set AMA_CustomsOffice and AMA_GoodsLocationFromMasterBill is empty", "ANP0061D", ManifestHeader.AMA_GoodsLocationFromMasterBill);
					}

					customsOffice.CustomsOfficeCode = "BA";
					using (customsOfficeRegistryItem.SetTemporaryValue(Guid.Empty, Env.CurrentBranchPK, Env.CurrentDepartmentPK, customsOffice))
					{
						ManifestHeader.AMA_CustomsOffice = "BA";
						AssertEquals("Not default when AMA_GoodsLocationFromMasterBill has value", "ANP0061D", ManifestHeader.AMA_GoodsLocationFromMasterBill);
					}
				}
			}
		}

		public void TestSetDefaultValues_AMA_OA_DeconsolidateAddress_ZAddress()
		{
			var factory = Factory;
			var org1 = factory.New<OrgHeader>();
			org1.OH_Code = "TWTPE";
			org1.MainAddress.Address1 = "TWTPE Address 1";
			var org2 = factory.New<OrgHeader>();
			org2.OH_Code = "TWKHH";
			org2.MainAddress.Address1 = "TWKHH Address 1";
			var company = factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = "TW";
			company.GC_OH_OrgProxy = org1.PK;
			var branch1 = factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = company.PK;
			branch1.GB_OH_OrgProxy = org2.PK;
			var branch2 = factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_GC = company.PK;
			factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch1.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				AssertEquals("Default from CurrentBranch", org2.PK, ManifestHeader.AMA_OA_DeconsolidateAddress_ZAddress.OrgPK);
			}

			manifestHeader = null;
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch2.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				AssertEquals("Default from CurrentCompany when CurrentBranch proxy is empty", org1.PK, ManifestHeader.AMA_OA_DeconsolidateAddress_ZAddress.OrgPK);
			}
		}

		public void TestSetDefaultValues_AMA_TransportMode()
		{
			AssertEquals("AMA_TransportMode", "AIR", ManifestHeader.AMA_TransportMode);
		}

		public void TestIsDeconsolidatorEnabledCore()
		{
			Assert("IsDeconsolidatorEnabled should be true by default in TW", ManifestHeader.IsDeconsolidatorEnabled);
		}

		public void TestUpdateAMA_CarrierCodeOnAMA_OA_CarrierChangedWhenIsAir()
		{
			var airline1 = Factory.NewWithValidTestData<RefAirline>();
			airline1.RM_EagleAddedAirlinePrefixOrAccountingCode = "100";
			airline1.RM_MembershipFlagIATA = false;

			var airline2 = Factory.NewWithValidTestData<RefAirline>();
			airline2.RM_EagleAddedAirlinePrefixOrAccountingCode = "200";
			airline2.RM_MembershipFlagIATA = true;

			var airline3 = Factory.NewWithValidTestData<RefAirline>();
			airline3.RM_EagleAddedAirlinePrefixOrAccountingCode = "300";
			airline3.RM_MembershipFlagIATA = true;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "AAA";
			org.OH_IsAirLine = true;
			var address = org.Addresses.AddNew();
			address.OA_Address1 = "address";
			ManifestHeader.AMA_TransportMode = TransportTypeList.Codes.Air;

			CombineAssertions(() =>
			{
				AssertAMA_CarrierCodeWhenIsAir(org, airline1.PK, address.PK, airline1.RM_EagleAddedAirlinePrefixOrAccountingCode);
				AssertAMA_CarrierCodeWhenIsAir(org, airline2.PK, address.PK, airline2.RM_EagleAddedAirlinePrefixOrAccountingCode);
				AssertAMA_CarrierCodeWhenIsAir(org, airline3.PK, address.PK, airline3.RM_EagleAddedAirlinePrefixOrAccountingCode);
				AssertAMA_CarrierCodeWhenIsAir(org, Guid.Empty, address.PK, ZString.Empty);
			});
		}

		public void TestUpdateAMA_CarrierCodeOnAMA_OA_CarrierChangedWhenIsSea()
		{
			var orgCarrierCode = "1305910";
			var addressCarrierCodeTW = "130591A";
			var addressCarrierCodeUS = "ABCD";

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "AAA";
			org1.OH_IsShippingProvider = true;
			var orgAddress1 = org1.Addresses.AddNew();
			orgAddress1.OA_Address1 = "address";
			org1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, orgCarrierCode, Core.Constants.CountryCodes.Taiwan);

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "BBB";
			var orgAddress2 = org2.Addresses.AddNew();
			orgAddress2.OA_Address1 = "address";
			orgAddress2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, addressCarrierCodeTW, Core.Constants.CountryCodes.Taiwan);
			orgAddress2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, addressCarrierCodeUS, Core.Constants.CountryCodes.UnitedStates);
			Factory.Save();

			ManifestHeader.AMA_TransportMode = TransportTypeList.Codes.Sea;

			CombineAssertions(() =>
			{
				ManifestHeader.AMA_CarrierCode = ZString.Empty;
				ManifestHeader.AMA_OA_Carrier = Guid.Empty;

				ManifestHeader.AMA_OA_Carrier = orgAddress1.PK;
				AssertEquals(orgCarrierCode, ManifestHeader.AMA_CarrierCode);

				ManifestHeader.AMA_CarrierCode = ZString.Empty;
				ManifestHeader.AMA_OA_Carrier = Guid.Empty;

				ManifestHeader.AMA_OA_Carrier = orgAddress2.PK;
				AssertEquals(addressCarrierCodeTW, ManifestHeader.AMA_CarrierCode);
			});
		}

		public void TestSetAMA_CarrierCode()
		{
			var value = new ZString("12345678901234567890");
			var maxLength = ManifestHeader.AMA_CarrierCodeInfo.MaxLength;
			ManifestHeader.AMA_CarrierCode = value;
			AssertEquals(value.Left(maxLength), ManifestHeader.AMA_CarrierCode);
		}

		public void TestAMA_CarrierCodeAttributes()
		{
			var carrierCodeInfo = ManifestHeader.AMA_CarrierCodeInfo;
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(carrierCodeInfo);

			CombineAssertions(() =>
			{
				AssertEquals("AMA_CarrierCode Caption", "Carrier ID", resourceStringData.Caption);
				AssertEquals("AMA_CarrierCode MaxLength", 14, carrierCodeInfo.MaxLength);
			});
		}

		public void TestContainerType()
		{
			var containers = ManifestHeader.Containers;
			var container = containers.AddNew();
			CombineAssertions(() =>
			{
				AssertType<AsycudaContainerCollection>(containers);
				AssertType<AsycudaContainer>(container);
			});
		}

		public void TestIAsycudaManifestHeader()
		{
			var bizObj = GetNewBusinessObject();
			bizObj.FillWithValidTestData();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<AsycudaManifestHeader>(bizObj.PK).GetType());
		}

		public void TestMasterBill()
		{
			var masterBill = ManifestHeader.MasterBill;
			AssertType<AsycudaBill>(masterBill);
		}

		public void TestBills()
		{
			var bills = ManifestHeader.Bills;
			AssertType<AsycudaBillCollection>(bills);
		}

		public void TestBillsCollectionType()
		{
			var billsCollection = ManifestHeader.Bills;
			AssertType<AsycudaBillCollection>(billsCollection);
			var bill = billsCollection.AddNew();
			AssertType<AsycudaBill>(bill);
		}

		public void TestMessages()
		{
			AssertType<AsycudaMessageCollection>(ManifestHeader.Messages);
		}

		public void TestPackedItemRelationship()
		{
			AssertEquals("Should be RelationshipType.One", ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType.One, ManifestHeader.PackedItemRelationship);
		}

		public void TestIsInAStatusAmendmentSendable()
		{
			AssertEquals(false, ManifestHeader.IsInAStatusAmendmentSendable);
		}

		[ExpectException(typeof(NotSupportedException))]
		public void TestGetMessageManagerForAmendmentDetection()
		{
			_ = ManifestHeader.GetMessageManagerForAmendmentDetection();
		}

		[ExpectException(typeof(NotSupportedException))]
		public void TestProcessBeforeDetectingAmendmentAndContinue()
		{
			_ = ManifestHeader.ProcessBeforeDetectingAmendmentAndContinue();
		}

		public void TestValidation()
		{
			AssertType<AsycudaManifestHeaderValidation>(ManifestHeader.Validation);
		}

		public void TestLookups()
		{
			AssertType<AsycudaManifestHeaderLookups>(ManifestHeader.Lookups);
		}

		public void TestAMA_GoodsLocationFromMasterBill()
		{
			var targetInfo = ManifestHeader.AMA_GoodsLocationFromMasterBillInfo;
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(targetInfo);
			AssertEquals("AMA_GoodsLocationFromMasterBill Caption", "Goods Location", resourceStringData.Caption);
			AssertEquals("AMA_GoodsLocationFromMasterBill MaxLength", 8, targetInfo.MaxLength);
		}

		public void TestAMA_VehicleRegistration()
		{
			AssertEquals("AMA_VehicleRegistration MaxLength", 6, ManifestHeader.AMA_VehicleRegistrationInfo.MaxLength);
		}

		public void TestAMA_GoodsLocationFromMasterBill_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(ManifestHeader.AMA_GoodsLocationFromMasterBillInfo);
			AssertEquals("AMA_GoodsLocationFromMasterBill Caption", "Goods Location", resourceStringData.Caption);
		}

		public void TestArrivalHeaderLoaded()
		{
			var newArrivalHeader = Factory.NewWithValidTestData<AsycudaArrivalHeader>();
			newArrivalHeader.ATH_AMA_ManifestHeader = ManifestHeader.PK;
			newArrivalHeader.ATH_ClusterKey = ManifestHeader.AMA_ClusterKey;
			AssertEquals(newArrivalHeader.PK, ManifestHeader.ArrivalHeader.PK);
		}

		public void TestArrivalHeaderCreated()
		{
			var loadArrivalHeader = Factory.LoadTop1<AsycudaArrivalHeader>(new ZQuery(AsycudaArrivalHeaderSchema.ATH_AMA_ManifestHeader, ManifestHeader.PK));
			AssertNull(loadArrivalHeader);

			var arrivalHeader = ManifestHeader.ArrivalHeader;
			loadArrivalHeader = Factory.LoadTop1<AsycudaArrivalHeader>(new ZQuery(AsycudaArrivalHeaderSchema.ATH_AMA_ManifestHeader, ManifestHeader.PK));
			AssertEquals(loadArrivalHeader.PK, arrivalHeader.PK);
		}

		public void TestITWMessageInfoProvider()
		{
			var bill = ManifestHeader.Bills.AddNew();
			bill.CustomsEntryNumber = "ABC12345";

			var provider = (ITWMessageInfoProvider)ManifestHeader;
			CombineAssertions("ITWMessageInfoProvider members", () =>
			{
				AssertEquals("Entry Number", "ABC12345", provider.EntryNumber);
				AssertEquals("Entry Number Type", MessageTypeList.Codes.FHM, provider.EntryNumberType);
				AssertEquals("Staff Code", ZString.Empty, provider.StaffCode);
				AssertEquals("Company ID", GlbCompany.CurrentCompany.GC_Code, provider.CompanyID);
				AssertEquals("Password Type", PasswordTypesList.Codes.TVF, provider.PasswordType);
			});
		}

		public void TestDefaultGetConsolSynchronizerCore()
		{
			var consol = Factory.New<Freight.Forwarding.Business.ForwardingConsol>();
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.SetParent(consol);
			AssertType<AsycudaManifestHeaderSynchroniser>(manifestHeader.Synchroniser);
		}

		public void TestUpdateMessageStatusFromBillsOnSaving()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill1 = manifestHeader.Bills.AddNew();
			bill1.ABL_MessageStatus = TWMessageStatusCodeList.Codes.Acknowledged;
			var bill2 = manifestHeader.Bills.AddNew();
			bill2.ABL_MessageStatus = TWMessageStatusCodeList.Codes.Acknowledged;
			var bill3 = manifestHeader.Bills.AddNew();
			bill3.ABL_MessageStatus = TWMessageStatusCodeList.Codes.Acknowledged;
			Factory.Save();
			AssertEquals("all bill of ABL_MessageStatus are ACK", TWMessageStatusCodeList.Codes.Acknowledged, manifestHeader.AMA_MessageStatus);

			bill1.ABL_MessageStatus = TWMessageStatusCodeList.Codes.AwaitingResponse;
			bill2.ABL_MessageStatus = TWMessageStatusCodeList.Codes.AwaitingResponse;
			bill3.ABL_MessageStatus = TWMessageStatusCodeList.Codes.AwaitingResponse;
			Factory.Save();
			AssertEquals("all bill of ABL_MessageStatus are AWR", TWMessageStatusCodeList.Codes.AwaitingResponse, manifestHeader.AMA_MessageStatus);

			bill1.ABL_MessageStatus = TWMessageStatusCodeList.Codes.Sent;
			bill2.ABL_MessageStatus = TWMessageStatusCodeList.Codes.Sent;
			bill3.ABL_MessageStatus = TWMessageStatusCodeList.Codes.Sent;
			Factory.Save();
			AssertEquals("all bill of ABL_MessageStatus are SNT", TWMessageStatusCodeList.Codes.Sent, manifestHeader.AMA_MessageStatus);

			bill1.ABL_MessageStatus = TWMessageStatusCodeList.Codes.Unknown;
			Factory.Save();
			AssertEquals("The ABL_MessageStatus of one of the bills is UNK", TWMessageStatusCodeList.Codes.Unknown, manifestHeader.AMA_MessageStatus);

			bill2.ABL_MessageStatus = TWMessageStatusCodeList.Codes.NotSent;
			Factory.Save();
			AssertEquals("The ABL_MessageStatus of one of the bills is NOT", TWMessageStatusCodeList.Codes.NotSent, manifestHeader.AMA_MessageStatus);

			bill3.ABL_MessageStatus = TWMessageStatusCodeList.Codes.TransmissionError;
			Factory.Save();
			AssertEquals("The ABL_MessageStatus of one of the bills is ERR", TWMessageStatusCodeList.Codes.TransmissionError, manifestHeader.AMA_MessageStatus);
		}

		public void TestUpdateCustomsStatusFromBillsOnSaving()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill1 = manifestHeader.Bills.AddNew();
			bill1.ABL_BillStatus = Constants.CustomsManifestStatus.AP;
			var bill2 = manifestHeader.Bills.AddNew();
			bill2.ABL_BillStatus = Constants.CustomsManifestStatus.AP;
			var bill3 = manifestHeader.Bills.AddNew();
			bill3.ABL_BillStatus = Constants.CustomsManifestStatus.AP;
			Factory.Save();
			AssertEquals("all bill of ABL_BillStatus are AP", Constants.CustomsManifestStatus.AP, manifestHeader.RegistrationStatus);

			bill1.ABL_BillStatus = Constants.CustomsManifestStatus.AK;
			Factory.Save();
			AssertEquals("The ABL_BillStatus of one of the bills is AK", Constants.CustomsManifestStatus.AK, manifestHeader.RegistrationStatus);

			bill2.ABL_BillStatus = Constants.CustomsManifestStatus.EX;
			Factory.Save();
			AssertEquals("The ABL_BillStatus of one of the bills is EX", Constants.CustomsManifestStatus.EX, manifestHeader.RegistrationStatus);

			bill3.ABL_BillStatus = Constants.CustomsManifestStatus.RE;
			Factory.Save();
			AssertEquals("The ABL_BillStatus of one of the bills is RE", Constants.CustomsManifestStatus.RE, manifestHeader.RegistrationStatus);
		}

		protected override Type ExpectedTypeOfContainer => typeof(AsycudaContainerCollection);

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.SuspendCheckBusinessObjectType();
			header.AMA_JobReference = "C1234";
			return header;
		}

		AsycudaManifestHeader ManifestHeader
		{
			get
			{
				if (manifestHeader == null)
				{
					manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
					manifestHeader.AMA_JobReference = "C4321";
				}
				return manifestHeader;
			}
		}
		AsycudaManifestHeader manifestHeader;

		void AssertAMA_CarrierCodeWhenIsAir(OrgHeader carrierHeader, ZGuid airlinePK, ZGuid carrierPK, string expectedCarrierCode)
		{
			carrierHeader.MiscServ.OM_RM_Airline = airlinePK;
			ManifestHeader.AMA_CarrierCodeInfo.ClearValue();
			ManifestHeader.AMA_OA_CarrierInfo.ClearValue();
			ManifestHeader.AMA_OA_Carrier = carrierPK;
			AssertEquals(expectedCarrierCode, ManifestHeader.AMA_CarrierCode);
		}
	}
}

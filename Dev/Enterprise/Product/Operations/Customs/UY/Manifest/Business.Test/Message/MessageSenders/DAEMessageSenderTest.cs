using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.UY.Manifest.Business.Testing
{
	public class DAEMessageSenderTest : TestCaseWithFactory
	{
		protected AsycudaManifestHeader header;

		public void TestSendDAEMessage()
		{
			PopulateManifestHeader();
			CreateAndPopulateHouseBill();
			CreateAndPopulateMasterBill();
			CreateAndPopulatePack(header.Bills[0]);

			Factory.Save();

			var billsToSend = header.Bills.Cast<AsycudaBill>().ToArray();
			var messageSender = new DAEMessageSender(header, new DAEWrapper(header, billsToSend), "A");
			var messageResult = messageSender.SendDAEMessage(MessageSubTypeCodes.Codes.Original, billsToSend);

			AssertEquals(messageResult, "Message sent successfully");
			AssertEquals(MessageStatusCodeList.Codes.Awaiting, header.AMA_MessageStatus);
			AssertEquals(MessageStatusCodeList.Codes.Sent, header.Bills[0].ABL_BillStatus);
			AssertEquals(MessageStatusCodeList.Codes.Awaiting, header.Bills[0].ABL_MessageStatus);

			AssertEquals(AsycudaManifestHeader.Schema.TableName, header.Messages[0].EM_LinkTable);
			AssertEquals(header.PK, header.Messages[0].EM_LinkUniqueID);
		}

		protected override void SetUp()
		{
			base.SetUp();

			setCertificate();

			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			GlbCompany.CurrentCompany.GC_BusinessRegNo = "210413450015";
			GlbStaff.CurrentUser.GS_EmailAddress = "AC_ADUANAS@dhluy.com";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Uruguay);
			helper.CreateCusMapType(AsycudaBill.UYConstants.CountryMapType, "OUT", AsycudaBill.UYConstants.CountryMapType, true);
			helper.CreateCusMap(AsycudaBill.UYConstants.CountryMapType, Core.Constants.CountryCodes.Uruguay, "858", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.Uruguay);

			Factory.Save();
		}

		public void PopulateManifestHeader()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "123456";
			org.CustomsCodes.AddNew(UruguayOrgCusCodeInfo.OrgCusCodes.CID, "213369370010");

			var orgAddress = org.MainAddress;
			orgAddress.Address1 = "1345";

			var orgCusCode = orgAddress.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = "RUT";
			orgCusCode.OK_CustomsRegNo = "214182520016";
			orgCusCode.OK_RN_NKCodeCountry = "UY";

			header.ShippingAgentOrgPK = org.PK;
			header.ShippingAgentOrg.Addresses.Add(orgAddress);
			header.AMA_OA_Carrier = orgAddress.PK;

			header.AMA_CustomsOffice = "2081";
			header.AMA_DateAtCustomsOffice = new ZDate(2019, 10, 31);
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_Voyage = "UC1103";

			Factory.Save();
		}

		public void CreateAndPopulateHouseBill()
		{
			AsycudaBill bill = header.Bills.AddNew();

			var org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "UY";
			org.OH_FullName = "ConsigneeName";
			var orgAddress = org.MainAddress;
			org.CustomsCodes.AddNew(UruguayOrgCusCodeInfo.OrgCusCodes.RUT, "211110890017");

			orgAddress.OA_Address1 = "ConsigneeAddress1";
			orgAddress.OA_Address2 = "ConsigneeAddress2";
			orgAddress.OA_State = "ConsigneeState";
			orgAddress.OA_RN_NKCountryCode = "UY";
			bill.ABL_OA_Consignee = orgAddress.PK;

			org.OH_FullName = "THERMO ORION/THERMO ELECTRON";
			bill.ABL_OA_Shipper = orgAddress.PK;

			org.OH_FullName = "ELECO S.A.";
			orgAddress.OA_Address1 = "ROMAN GARCIA 1086";
			orgAddress.OA_Phone = "23046888";
			bill.ABL_OA_NotifyParty = orgAddress.PK;

			bill.ABL_BillNumber = "7YM7014";
			bill.ABL_BolType = Core.Constants.ShipmentTypes.StandardHouse;
			bill.ABL_RL_NKOrigin = "USMIA";
			bill.ABL_RL_NKPortOfDischarge = "UYMVD";
			bill.ABL_RL_NKFinalDestination = "UYMVD";
			bill.ABL_Volume = 1.000m;
			bill.ABL_PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			bill.ABL_Transshipment = false;

			Factory.Save();
		}

		public void CreateAndPopulateMasterBill()
		{
			AsycudaBill bill = (AsycudaBill)header.MasterBill;

			bill.ABL_BillNumber = "04507816955";
			bill.ABL_RL_NKPortOfLoading = "USMIA";
			bill.ABL_RL_NKPortOfDischarge = "UYMVD";

			Factory.Save();
		}

		public void CreateAndPopulatePack(AsycudaBill bill)
		{
			AsycudaPack pack = bill.Packs.AddNew();

			pack.APA_Weight = 127.900m;
			pack.APA_PackUQ = "PCS";
			pack.APA_PackQty = 1;
			pack.APA_MarksAndNumbers = "MarksAndNumbers";
			pack.APA_GoodsDescription = "DIAGNOSTIC LABORATORY";

			Factory.Save();
		}

		public void setCertificate()
		{
			var credential = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(GlbCompany.CurrentCompany).GlbExternalPassword;
			credential.GP_Certificate = new ZBlob(X509Certificate2TestHelper.ValidCertificate);
			credential.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

			GlbCompany.CurrentCompany.Factory.Save();
		}
	}
}

using System.Linq;
using CargoWise.Customs.UY.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.UY.Manifest.Business.Testing
{
	sealed class DAEBillWrapperTest : TestCaseWithFactory
	{
		AsycudaManifestHeader header;
		OrgHeader commonOrgHeader;
		OrgAddress commonOrgAddress;

		public void TestDAEBillWrapper()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			PopulateManifestHeader();

			CreateAndPopulateHouseBill();

			IDaeDeclaration wrapper = new DAEWrapper(header, header.Bills.Cast<AsycudaBill>().ToArray());
			var manifest = wrapper.Manifests.First();

			IDaeBillOfLading bill1 = manifest.BillsOfLading.ElementAt(0);

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals((ZShort)1, bill1.SequenceNumber);
				AssertEquals("7YM7014", bill1.BillNumber);
				AssertEquals("US MIA", bill1.PortOfLoading);
				AssertEquals("HWB", bill1.BolType);
				AssertEquals("04507816955", bill1.MasterBillNumber);
				AssertEquals("UY MVD", bill1.PortOfDischarge);
				AssertEquals("211110890017", bill1.ConsigneeDocumentNo);
				AssertEquals("4", bill1.ConsigneeDocumentType);
				AssertEquals("UY MVD", bill1.FinalDestination);
				AssertEquals("THERMO ORION/THERMO ELECTRON", bill1.ShipperName);
				AssertEquals((ZDecimal)1.000, bill1.BOL_Volume);
				AssertEquals("ELECO S.A.", bill1.NotifyName);
				AssertEquals("ROMAN GARCIA 1086", bill1.NotifyAddress);
				AssertEquals("+598 2304 6888", bill1.NotifyPhone);
				AssertEquals("N", bill1.OnDemand);
				AssertEquals("P", bill1.PrepaidCollect);
				AssertEquals("AC_ADUANAS@dhluy.com", bill1.BOL_AgentMail);
				AssertEquals("N", bill1.Transfer);
				AssertEquals("ACA", bill1.Category);
				AssertEquals("US MIA", bill1.NKOrigin);
				AssertEquals("858", bill1.ConsigneeCountry);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			GlbCompany.CurrentCompany.GC_BusinessRegNo = "210413450015";
			GlbStaff.CurrentUser.GS_EmailAddress = "AC_ADUANAS@dhluy.com";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Uruguay);
			helper.CreateCusMapType(AsycudaBill.UYConstants.CountryMapType, "OUT", AsycudaBill.UYConstants.CountryMapType, true);
			helper.CreateCusMap(AsycudaBill.UYConstants.CountryMapType, Core.Constants.CountryCodes.Uruguay, "858", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.Uruguay);

			commonOrgHeader = Factory.New<OrgHeader>();
			commonOrgHeader.OH_Code = "123456";
			commonOrgHeader.CustomsCodes.AddNew(UruguayOrgCusCodeInfo.OrgCusCodes.CID, "213369370010");

			commonOrgAddress = commonOrgHeader.MainAddress;
			commonOrgAddress.Address1 = "1345";

			var orgCusCode = commonOrgAddress.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = UruguayOrgCusCodeInfo.OrgCusCodes.RUT;
			orgCusCode.OK_CustomsRegNo = "214182520016";
			orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Uruguay;
		}

		void PopulateManifestHeader()
		{
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_MasterBill = "04507816955";
			header.AMA_RL_NKPortOfDischarge = "UYMVD";
			header.AMA_RL_NKPortOfLoading = "USMIA";

			header.ShippingAgentOrgPK = commonOrgHeader.PK;
			header.ShippingAgentOrg.Addresses.Add(commonOrgAddress);
			header.AMA_OA_Carrier = commonOrgAddress.PK;
			header.AMA_DateAtCustomsOffice = new ZDate(2019, 10, 31);
			header.AMA_E_ARV = new ZDate(2019, 10, 31);
		}

		void CreateAndPopulateHouseBill(string codeType = UruguayOrgCusCodeInfo.OrgCusCodes.RUT)
		{
			AsycudaBill bill = header.Bills.AddNew();

			var org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = Core.Constants.CountryCodes.Uruguay;
			org.OH_FullName = "ConsigneeName";
			var orgAddress = org.MainAddress;
			org.CustomsCodes.AddNew(codeType, "211110890017");

			orgAddress.OA_Address1 = "ConsigneeAddress1";
			orgAddress.OA_Address2 = "ConsigneeAddress2";
			orgAddress.OA_State = "ConsigneeState";
			orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Uruguay;
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
			bill.ABL_VolumeUQ = Core.Constants.Volume.CubicMetres;
			bill.ABL_PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			bill.ABL_Transshipment = false;
		}
	}
}


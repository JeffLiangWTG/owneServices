using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Customs.UY.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.UY.Manifest.Business.Testing
{
	sealed class DAEWrapperTest : TestCaseWithFactory
	{
		AsycudaManifestHeader header;
		OrgHeader commonOrgHeader;
		OrgAddress commonOrgAddress;

		public void TestDAEWrapper()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			PopulateManifestHeader();

			CreateAndPopulateHouseBill();
			CreateAndPopulateHouseBill();
			CreateAndPopulateMasterBill();

			CreateAndPopulatePack(header.Bills[0]);
			CreateAndPopulatePack(header.Bills[0]);
			CreateAndPopulatePack(header.Bills[1]);

			IDaeDeclaration wrapper = new DAEWrapper(header, header.Bills.Cast<AsycudaBill>().ToArray());
			var manifest = wrapper.Manifests.First();

			IDaeBillOfLading bill1 = manifest.BillsOfLading.ElementAt(0);
			IDaeBillOfLading bill2 = manifest.BillsOfLading.ElementAt(1);

			IDaeBillOfLadingLine pack1 = bill1.Lines.ElementAt(0);
			IDaeBillOfLadingLine pack2 = bill1.Lines.ElementAt(1);
			IDaeBillOfLadingLine pack3 = bill2.Lines.ElementAt(0);

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("4", wrapper.DocumentType);
				AssertEquals("210413450015", wrapper.DocumentID);
				AssertEquals("WS_MANIFIESTO", wrapper.InterchangeCode);
				AssertEquals("<<MSGNO PLACEHOLDER>>", wrapper.TransactionNo);

				Assert(wrapper.Manifests.IsCountEqualTo(1));

				AssertEquals("4", manifest.TransportMode);
				AssertEquals("0", manifest.ManifestNature);
				AssertEquals("UC1103", manifest.ManifestNo);
				AssertEquals("2081", manifest.CustomsOffice);
				AssertEquals((ZLong)20191031, manifest.DateOfArrival);
				AssertEquals("4", manifest.ShippingProviderDocType);
				AssertEquals("214182520016", manifest.ShippingProviderDoc);
				AssertEquals("US MIA", manifest.PlaceDepartureCode);
				AssertEquals("UY MVD", manifest.PlaceOfArrivalCode);

				Assert(manifest.BillsOfLading.IsCountEqualTo(2));

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

				Assert(bill1.Lines.IsCountEqualTo(2));

				AssertEquals((ZShort)1, pack1.LineNo);
				AssertEquals((ZDecimal)127.900, pack1.Weight);
				AssertEquals("BLK", pack1.PackUQ);
				AssertEquals((ZDecimal)1.000, pack1.PackQty);
				AssertEquals("MarksAndNumbers", pack1.MarksAndNumbers);
				AssertEquals("DIAGNOSTIC LABORATORY", pack1.GoodsDescription);

				AssertEquals((ZShort)2, pack2.LineNo);

				AssertEquals((ZShort)2, manifest.BillsOfLading.ElementAt(1).SequenceNumber);
				Assert(bill2.Lines.IsCountEqualTo(1));
				AssertEquals((ZShort)1, pack3.LineNo);
			});

			wrapper = new DAEWrapper(header, header.Bills.Cast<AsycudaBill>().ToArray());
			AssertEquals("<<MSGNO PLACEHOLDER>>", wrapper.TransactionNo);
		}

		public void TestVolumeIsInCubicMeters()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			PopulateManifestHeader();

			AsycudaBill bill = header.Bills.AddNew();
			bill.ABL_Volume = 1000.00;
			bill.ABL_VolumeUQ = Core.Constants.Volume.CubicCentimeters;

			AsycudaBill bill2 = header.Bills.AddNew();
			bill2.ABL_Volume = 1000.00;
			bill2.ABL_VolumeUQ = Core.Constants.Volume.CubicMetres;

			IDaeDeclaration wrapper = new DAEWrapper(header, header.Bills.Cast<AsycudaBill>().ToArray());
			var manifest = wrapper.Manifests.First();

			IDaeBillOfLading billy1 = manifest.BillsOfLading.ElementAt(0);
			IDaeBillOfLading billy2 = manifest.BillsOfLading.ElementAt(1);

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals((ZDecimal)0.001, billy1.BOL_Volume);
				AssertEquals((ZDecimal)1000.00, billy2.BOL_Volume);
			});
		}

		[TestDate(2020, 10, 02, 18, 00, 00)]
		public void TestPopulateConsigneeName()
		{
			var expectedXML = @"<ConsignatarioNombre>ConsigneeName</ConsignatarioNombre>";

			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			PopulateManifestHeader();
			CreateAndPopulateHouseBill();

			Factory.Save();

			AssertNotContains(expectedXML, GetXMLMessageFromWrapper(RecordTypes.Add));

			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			PopulateManifestHeader("MAN0000002");
			CreateAndPopulateHouseBill(UruguayOrgCusCodeInfo.OrgCusCodes.CID);

			Factory.Save();

			AssertContains(expectedXML, GetXMLMessageFromWrapper(RecordTypes.Add));

			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			PopulateManifestHeader("MAN0000003");
			CreateAndPopulateHouseBill();

			Factory.Save();

			AssertNotContains(expectedXML, GetXMLMessageFromWrapper(RecordTypes.Delete));

			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			PopulateManifestHeader("MAN0000004");
			CreateAndPopulateHouseBill(UruguayOrgCusCodeInfo.OrgCusCodes.CID);

			Factory.Save();

			AssertContains(expectedXML, GetXMLMessageFromWrapper(RecordTypes.Delete));
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

		ZString GetXMLMessageFromWrapper(ZString recordType)
		{
			var wrapper = new DAEWrapper(header, header.Bills.Cast<AsycudaBill>().ToArray());
			IXmlMessageBuilder daeBuilder = new DaeMessageBuilder(wrapper, recordType);
			var transactionNumber = Env.NumberFountains.GetOutgoingUYCustomsMessageNumber().GetNext(Factory).ToString();
			return daeBuilder.GenerateXmlMessage().GetSerializedString().Replace(UYMessage.MessageNumberPlaceHolderHtml, transactionNumber);
		}

		void PopulateManifestHeader(string jobReference = "MAN0000001")
		{
			header.ShippingAgentOrgPK = commonOrgHeader.PK;
			header.ShippingAgentOrg.Addresses.Add(commonOrgAddress);
			header.AMA_OA_Carrier = commonOrgAddress.PK;
			header.AMA_CustomsOffice = "2081";
			header.AMA_DateAtCustomsOffice = new ZDate(2019, 10, 31);
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_Voyage = "UC1103";
			header.AMA_JobReference = jobReference;
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

		void CreateAndPopulateMasterBill()
		{
			AsycudaBill bill = (AsycudaBill)header.MasterBill;

			bill.ABL_BillNumber = "04507816955";
			bill.ABL_RL_NKPortOfLoading = "USMIA";
			bill.ABL_RL_NKPortOfDischarge = "UYMVD";
		}

		void CreateAndPopulatePack(AsycudaBill bill)
		{
			AsycudaPack pack = bill.Packs.AddNew();

			pack.APA_Weight = 127.900m;
			pack.APA_WeightUQ = "KG";
			pack.APA_PackUQ = "BBK";
			pack.APA_PackQty = 1;
			pack.APA_MarksAndNumbers = "MarksAndNumbers";
			pack.APA_GoodsDescription = "DIAGNOSTIC LABORATORY";
		}
	}
}

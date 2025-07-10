using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.UY.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.UY.Manifest.Business.Testing
{
	public class DAEManifestWrapperTest : TestCaseWithFactory
	{
		AsycudaManifestHeader header;
		OrgHeader commonOrgHeader;
		OrgAddress commonOrgAddress;

		public void TestDAEManifestWrapper()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			PopulateManifestHeader();

			IDaeDeclaration wrapper = new DAEWrapper(header, header.Bills.Cast<AsycudaBill>().ToArray());
			var manifest = wrapper.Manifests.First();

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
			header.AMA_RL_NKPortOfLoading = "USMIA";
			header.AMA_RL_NKPortOfDischarge = "UYMVD";
		}
	}
}


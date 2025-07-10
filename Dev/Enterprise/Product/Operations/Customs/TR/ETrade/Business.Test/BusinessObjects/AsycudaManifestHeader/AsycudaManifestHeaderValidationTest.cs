using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.ETrade.Business.Testing
{
	public class AsycudaManifestHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheck_MessageMode()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			var importDictionary = header.Validation.ImportDictionary;
			AssertForCustomsStatus(header, importDictionary);

			header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			var exportDictionary = header.Validation.ExportDictionary;
			AssertForCustomsStatus(header, exportDictionary);
		}

		void AssertForCustomsStatus(AsycudaManifestHeader header, IReadOnlyDictionary<string, string> dictionary)
		{
			foreach (var keyValuePair in dictionary)
			{
				header.RegistrationStatus = keyValuePair.Key;

				header.MessageMode = keyValuePair.Value;
				AssertNoMessageErrors(header.MessageModeInfo);

				var anotherPair = dictionary.First(x => x.Value != keyValuePair.Value && x.Value != TRMessageTypes.Codes.CPL);
				header.MessageMode = anotherPair.Value;
				AssertHasMessageErrorContaining(header.MessageModeInfo, $"you should send {keyValuePair.Value}");

				header.MessageMode = TRMessageTypes.Codes.CPL;
				AssertNoMessageErrors(header.MessageModeInfo);

				header.RegistrationStatus = "XXX";
				header.MessageMode = anotherPair.Value;
				AssertNoMessageErrors(header.MessageModeInfo);
			}
		}

		public void TestCheckNumberOfBillsForImport()
		{
			var header = Factory.NewMoq<AsycudaManifestHeader>();
			header.Object.AMA_Nature = ShipmentTypeList.Codes.Import23;
			header.Setup(m => m.NumberOfBills).Returns(new ZInt(2001));
			header.Object.Validation.ValidateNumberOfBills();

			CombineAssertions(() =>
			{
				Assert(header.Object.IsImport);
				AssertHasMessageErrorContaining("Has Message error", header.Object.NumberOfBillsInfo, "Number Of Bills must be lower or equal to");
			});

			var header2 = Factory.NewMoq<AsycudaManifestHeader>();
			header2.Object.AMA_Nature = ShipmentTypeList.Codes.Import23;
			header2.Setup(m => m.NumberOfBills).Returns(new ZInt(20));
			header2.Object.Validation.ValidateNumberOfBills();

			CombineAssertions(() =>
			{
				Assert(header.Object.IsImport);
				AssertNoMessageErrorContaining("No Message error", header2.Object.NumberOfBillsInfo, "Number Of Bills must be lower or equal to");
			});
			header.VerifyAll();
		}

		public void TestCheckNumberOfBillsForExport()
		{
			var header = Factory.NewMoq<AsycudaManifestHeader>();
			header.Object.AMA_Nature = ShipmentTypeList.Codes.Export22;
			header.Setup(m => m.NumberOfBills).Returns(new ZInt(2501));
			header.Object.Validation.ValidateNumberOfBills();

			CombineAssertions(() =>
			{
				Assert(header.Object.IsExport);
				AssertHasMessageErrorContaining("Has Message error", header.Object.NumberOfBillsInfo, "Number Of Bills must be lower or equal to");
			});

			var header2 = Factory.NewMoq<AsycudaManifestHeader>();
			header2.Object.AMA_Nature = ShipmentTypeList.Codes.Export22;
			header2.Setup(m => m.NumberOfBills).Returns(new ZInt(25));
			header2.Object.Validation.ValidateNumberOfBills();

			CombineAssertions(() =>
			{
				Assert(header.Object.IsExport);
				AssertNoMessageErrorContaining("No Message error", header2.Object.NumberOfBillsInfo, "Number Of Bills must be lower or equal to");
			});
			header.VerifyAll();
		}

		public void TestCheckTransshipmentCountry()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			header.TransshipmentCountry = "!";
			AssertHasMessageErrorContaining(header.TransshipmentCountryInfo, "The code you have selected is not in the list");

			header.TransshipmentCountry = Core.Constants.CountryCodes.China;
			AssertNoMessageErrors(header.TransshipmentCountryInfo);
		}
		public void TestCheckDepartureFlight()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.DepartureFlight = "ULUTEST01";
			AssertNoMessageErrorContaining(header.DepartureFlightInfo, MandatoryValidation.YouHaveNotEntered);
			header.DepartureFlight = "";
			AssertHasMessageErrorContaining(header.DepartureFlightInfo, MandatoryValidation.YouHaveNotEntered);
		}
		public void TestCheckDepartureCountryCode()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			header.DepartureCountryCode = Core.Constants.CountryCodes.Turkey;
			AssertNoMessageErrorContaining(header.DepartureCountryCodeInfo, MandatoryValidation.YouHaveNotEntered);

			header.DepartureCountryCode = "";
			AssertHasMessageErrorContaining(header.DepartureCountryCodeInfo, MandatoryValidation.YouHaveNotEntered);

			header.DepartureCountryCode = "!";
			AssertHasMessageErrorContaining(header.DepartureCountryCodeInfo, "The code you have selected is not in the list");

			header.DepartureCountryCode = Core.Constants.CountryCodes.Turkey;
			AssertNoMessageErrors(header.DepartureCountryCodeInfo);
		}

		public void TestCheckAMA_RN_NKConveyanceNationality()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			header.AMA_RN_NKConveyanceNationality = Core.Constants.CountryCodes.Turkey;
			AssertNoMessageErrorContaining(header.AMA_RN_NKConveyanceNationalityInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_RN_NKConveyanceNationality = "";
			AssertHasMessageErrorContaining(header.AMA_RN_NKConveyanceNationalityInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_RN_NKConveyanceNationality = "!";
			AssertHasMessageErrorContaining(header.AMA_RN_NKConveyanceNationalityInfo, "The code you have selected is not in the list");

			header.AMA_RN_NKConveyanceNationality = Core.Constants.CountryCodes.Turkey;
			AssertNoMessageErrors(header.AMA_RN_NKConveyanceNationalityInfo);

			header.TransshipmentConveyanceCountry = "!";
			AssertHasMessageErrorContaining(header.TransshipmentConveyanceCountryInfo, "The code you have selected is not in the list");

			header.TransshipmentConveyanceCountry = Core.Constants.CountryCodes.SouthAfrica;
			AssertNoMessageErrors(header.TransshipmentConveyanceCountryInfo);
		}

		public void TestPresentationCustomsOffice()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.PresentationCustomsOffice = "TR000400";
			AssertNoMessageErrorContaining(header.PresentationCustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);
			header.PresentationCustomsOffice = "";
			AssertHasMessageErrorContaining(header.PresentationCustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestImportExportCustomsOffice()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.ImportExportCustomsOffice = "TR000400";
			AssertNoMessageErrorContaining(header.ImportExportCustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);
			header.ImportExportCustomsOffice = "";
			AssertHasMessageErrorContaining(header.ImportExportCustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestDischargeLoadingCustomsOffice()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.DischargeLoadingCustomsOffice = "TR000400";
			AssertNoMessageErrorContaining(header.DischargeLoadingCustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);
			header.DischargeLoadingCustomsOffice = "";
			AssertHasMessageErrorContaining(header.DischargeLoadingCustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestGoodsDescription()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.GoodsDescription = "TEST001";
			AssertNoMessageErrorContaining(header.GoodsDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
			header.GoodsDescription = "";
			AssertHasMessageErrorContaining(header.GoodsDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestChackAMA_OA_Carrier()
		{
			var carrierOrgAddress = Factory.NewWithValidTestData<OrgAddress>();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			Factory.Save();
			header.AMA_OA_Carrier = ZGuid.Empty;
			AssertHasMessageErrorContaining(header.AMA_OA_CarrierInfo, MandatoryValidation.YouHaveNotEntered);
			header.AMA_OA_Carrier = carrierOrgAddress.PK;
			AssertNoMessageErrors(header.AMA_OA_CarrierInfo);
		}

		public void TestCheckGoodsLocationCode()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.ProcedureCode = Core.Constants.CountryCodes.Turkey;
			header.GoodsLocationCode = "A0001";
			AssertNoMessageErrorContaining(header.GoodsLocationCodeInfo, MandatoryValidation.YouHaveNotEntered);
			header.GoodsLocationCode = ZString.Empty;
			AssertHasMessageErrorContaining(header.GoodsLocationCodeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckLocationInformation()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.ProcedureCode = Core.Constants.CountryCodes.Turkey;
			header.GoodsLocationCode = "A0001";
			AssertNoMessageErrorContaining(header.LocationInformationInfo, MandatoryValidation.YouHaveNotEntered);
			header.LocationInformation = ZString.Empty;
			AssertHasMessageErrorContaining(header.LocationInformationInfo, MandatoryValidation.YouHaveNotEntered);
			header.LocationInformation = "A0001 line 1";
			AssertNoMessageErrorContaining(header.LocationInformationInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckAMA_DateAtCustomsOffice()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			AssertNoMessageErrors(header.AMA_DateAtCustomsOfficeInfo);
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			header.AMA_DateAtCustomsOffice = ZDateTime.Empty;
			AssertHasMessageErrorContaining(header.AMA_DateAtCustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);
			header.AMA_DateAtCustomsOffice = ZDateTime.Now;
			AssertNoMessageErrors(header.AMA_DateAtCustomsOfficeInfo);
		}

		public void TestCheckEmptyAMA_ManifestType()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_ManifestType = null;
			AssertNoMessageErrors(header.AMA_ManifestTypeInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("TRBWH", "TRBWH");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, "TRBWH", "A0001", "A0001 line 1", new ZDateTime(2019, 10, 31), new ZDateTime(2079, 6, 6));
			Factory.Save();
		}
	}
}

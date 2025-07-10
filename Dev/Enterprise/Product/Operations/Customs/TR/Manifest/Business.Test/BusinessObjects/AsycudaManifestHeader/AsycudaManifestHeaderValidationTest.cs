using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using EnterpriseCoreUniversal = Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	public class AsycudaManifestHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestManifestTypesRequiringVoyageAndConveyance()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			bool checkRequiredAMA_Voyage = true;
			var validation = header.Validation;
			AssertEquals(checkRequiredAMA_Voyage, validation.ManifestTypesRequiringVoyageAndConveyance(TRManifestTypes.Codes.CIKONC));
			AssertEquals(checkRequiredAMA_Voyage, validation.ManifestTypesRequiringVoyageAndConveyance(TRManifestTypes.Codes.DENIHR));
			AssertEquals(checkRequiredAMA_Voyage, validation.ManifestTypesRequiringVoyageAndConveyance(TRManifestTypes.Codes.DENITH));
			AssertEquals(checkRequiredAMA_Voyage, validation.ManifestTypesRequiringVoyageAndConveyance(TRManifestTypes.Codes.HAVIHR));
			AssertEquals(checkRequiredAMA_Voyage, validation.ManifestTypesRequiringVoyageAndConveyance(TRManifestTypes.Codes.HAVITH));
			AssertEquals(checkRequiredAMA_Voyage, validation.ManifestTypesRequiringVoyageAndConveyance(TRManifestTypes.Codes.VARONC));
			AssertEquals(checkRequiredAMA_Voyage, validation.ManifestTypesRequiringVoyageAndConveyance(TRManifestTypes.Codes.EMANIF));
			checkRequiredAMA_Voyage = false;
			AssertEquals(checkRequiredAMA_Voyage, validation.ManifestTypesRequiringVoyageAndConveyance(TRManifestTypes.Codes.ATAIHR));
		}

		public void TestCheckAMA_Voyage()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			AssertAMA_VoyageDependOnManifest(header, TRManifestTypes.Codes.CIKONC);
			AssertAMA_VoyageDependOnManifest(header, TRManifestTypes.Codes.DENIHR);
			AssertAMA_VoyageDependOnManifest(header, TRManifestTypes.Codes.DENITH);
			AssertAMA_VoyageDependOnManifest(header, TRManifestTypes.Codes.HAVIHR);
			AssertAMA_VoyageDependOnManifest(header, TRManifestTypes.Codes.HAVITH);
			AssertAMA_VoyageDependOnManifest(header, TRManifestTypes.Codes.VARONC);
			AssertAMA_VoyageDependOnManifest(header, TRManifestTypes.Codes.EMANIF);
		}

		void AssertAMA_VoyageDependOnManifest(AsycudaManifestHeader header, ZString manifestType)
		{
			header.AMA_ManifestType = manifestType;
			header.AMA_Voyage = ZString.Empty;
			AssertHasMessageError(header.AMA_VoyageInfo, MandatoryValidation.YouHaveNotEnteredMessage(header.VoyageFlightNoLabel.Caption));
			header.AMA_Voyage = "ABC";
			AssertNoMessageError(header.AMA_VoyageInfo, MandatoryValidation.YouHaveNotEnteredMessage(header.VoyageFlightNoLabel.Caption));
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_ManifestType = TRManifestTypes.Codes.DENIHR;
			header.AMA_Voyage = "X";
			AssertHasMessageError(header.AMA_VoyageInfo, Enterprise.Customs.ASYCUDA.Business.ValidationConstants.FlightNumberDoesNotStartWithAValidIATAAirCode);
			header.AMA_Voyage = "QF";
			AssertNoMessageErrorContaining("After set value, there should not be message error on AMA_Voyage", header.AMA_VoyageInfo, Enterprise.Customs.ASYCUDA.Business.ValidationConstants.FlightNumberDoesNotStartWithAValidIATAAirCode);
		}

		public void TestCheckAMA_VesselName()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			CombineAssertions("Mandatory Test AMA_VesselName", () =>
			{
				header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
				header.AMA_ManifestType = TRManifestTypes.Codes.DENIHR;
				header.AMA_VesselName = ZString.Empty;
				AssertHasMessageErrorContaining("AMA_VesselName Empty | AMA_ManifestType DENIHR", header.AMA_VesselNameInfo, MandatoryValidation.YouHaveNotEntered);
				header.AMA_ManifestType = TRManifestTypes.Codes.DENITH;
				header.AMA_VesselName = ZString.Empty;
				AssertHasMessageErrorContaining("AMA_VesselName Empty | AMA_ManifestType DENITH", header.AMA_VesselNameInfo, MandatoryValidation.YouHaveNotEntered);
				header.AMA_VesselName = "X";
				AssertNoMessageErrorContaining("AMA_VesselName is not Empty | AMA_ManifestType DENITH", header.AMA_VesselNameInfo, MandatoryValidation.YouHaveNotEntered);
				header.AMA_TransportMode = Core.Constants.TransportModes.Air;
				header.AMA_ManifestType = TRManifestTypes.Codes.HAVIHR;
				header.AMA_VesselName = ZString.Empty;
				AssertNoMessageErrorContaining("AMA_VesselName Empty | AMA_ManifestType HAVIHR", header.AMA_VesselNameInfo, MandatoryValidation.YouHaveNotEntered);
				header.AMA_ManifestType = TRManifestTypes.Codes.HAVITH;
				header.AMA_VesselName = ZString.Empty;
				AssertHasMessageErrorContaining("AMA_VesselName Empty | AMA_ManifestType HAVITH", header.AMA_VesselNameInfo, MandatoryValidation.YouHaveNotEntered);
				header.AMA_VesselName = "X";
				AssertNoMessageErrorContaining("AMA_VesselName is not Empty | AMA_ManifestType HAVITH", header.AMA_VesselNameInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckAMA_VesselName_ListValidation()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			CombineAssertions("ListValidation Test AMA_VesselName", () =>
			{
				var expectedWarning = "The Vessel Name entered does not exist in the reference file.";
				header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
				header.AMA_ManifestType = TRManifestTypes.Codes.DENIHR;
				header.AMA_VesselName = ZString.Empty;
				AssertNoWarning("AMA_VesselName Empty | AMA_ManifestType DENIHR", header.AMA_VesselNameInfo, expectedWarning);
				header.AMA_ManifestType = TRManifestTypes.Codes.DENITH;
				header.AMA_VesselName = ZString.Empty;
				AssertNoWarning("AMA_VesselName Empty | AMA_ManifestType DENITH", header.AMA_VesselNameInfo, expectedWarning);
				header.AMA_VesselName = "X";
				AssertHasWarning("AMA_VesselName is not on file | AMA_ManifestType DENITH", header.AMA_VesselNameInfo, expectedWarning);
				header.AMA_TransportMode = Core.Constants.TransportModes.Air;
				header.AMA_ManifestType = TRManifestTypes.Codes.HAVIHR;
				header.AMA_VesselName = ZString.Empty;
				AssertNoWarning("AMA_VesselName Empty | AMA_ManifestType HAVIHR", header.AMA_VesselNameInfo, expectedWarning);
				header.AMA_ManifestType = TRManifestTypes.Codes.HAVITH;
				header.AMA_VesselName = ZString.Empty;
				AssertNoWarning("AMA_VesselName Empty | AMA_ManifestType HAVITH", header.AMA_VesselNameInfo, expectedWarning);
				var vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Code = "ANL Newcastle";
				vessel.RV_LloydsNumber = "1234567";
				header.AMA_ManifestType = TRManifestTypes.Codes.DENITH;
				header.AMA_VesselName = vessel.RV_Code;
				AssertNoWarning("AMA_VesselName valid | AMA_ManifestType HAVITH", header.AMA_VesselNameInfo, expectedWarning);
			});
		}

		public void TestCheckAMA_RN_NKConveyanceNationality()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			CombineAssertions("AMA_RN_NKConveyanceNationalityDependOnManifest", () =>
			{
				header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
				AssertAMA_RN_NKConveyanceNationalityDependOnManifest(header, TRManifestTypes.Codes.CIKONC);
				AssertAMA_RN_NKConveyanceNationalityDependOnManifest(header, TRManifestTypes.Codes.DENIHR);
				AssertAMA_RN_NKConveyanceNationalityDependOnManifest(header, TRManifestTypes.Codes.DENITH);
				AssertAMA_RN_NKConveyanceNationalityDependOnManifest(header, TRManifestTypes.Codes.HAVIHR);
				AssertAMA_RN_NKConveyanceNationalityDependOnManifest(header, TRManifestTypes.Codes.HAVITH);
				AssertAMA_RN_NKConveyanceNationalityDependOnManifest(header, TRManifestTypes.Codes.VARONC);
				AssertAMA_RN_NKConveyanceNationalityDependOnManifest(header, TRManifestTypes.Codes.EMANIF);
				header.AMA_TransportMode = Core.Constants.TransportModes.Air;
				AssertAMA_RN_NKConveyanceNationalityDependOnManifest(header, TRManifestTypes.Codes.ATAIHR);
				AssertAMA_RN_NKConveyanceNationalityDependOnManifest(header, TRManifestTypes.Codes.ATAITH);
				AssertAMA_RN_NKConveyanceNationalityDependOnManifest(header, TRManifestTypes.Codes.DIGIHR);
				AssertAMA_RN_NKConveyanceNationalityDependOnManifest(header, TRManifestTypes.Codes.DIGITH);
				AssertAMA_RN_NKConveyanceNationalityDependOnManifest(header, TRManifestTypes.Codes.HAVITH);
				AssertAMA_RN_NKConveyanceNationalityDependOnManifest(header, TRManifestTypes.Codes.TESLIM);
			});
		}

		void AssertAMA_RN_NKConveyanceNationalityDependOnManifest(AsycudaManifestHeader header, ZString manifestType)
		{
			header.AMA_ManifestType = manifestType;
			header.AMA_RN_NKConveyanceNationality = ZString.Empty;
			AssertHasMessageErrorContaining("Manifest Type:" + manifestType, header.AMA_RN_NKConveyanceNationalityInfo, "You have not entered");
			header.AMA_RN_NKConveyanceNationality = "AB";
			AssertNoMessageErrorContaining("After set value, there should not be message error on AMA_RN_NKConveyanceNationality", header.AMA_RN_NKConveyanceNationalityInfo, "You have not entered");
		}

		public void TestCheckAMA_CustomsOffice()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(EnterpriseCoreUniversal.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeList(EnterpriseCoreUniversal.RefDataGrouping.Codes.CommonDataGrouping, EnterpriseCoreUniversal.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Turkey, Core.Constants.CountryCodes.Turkey, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeType(EnterpriseCoreUniversal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			const string officeCode = "MVAL";
			var trMVAL = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, EnterpriseCoreUniversal.RefCusCodeListTypes.Codes.CustomsOffice, officeCode, "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeType(EnterpriseCoreUniversal.RefCusCodeListTypes.Codes.ManifestValidationRule, "ManifestValidationRule");
			var zzValidationRule = helper.CreateNewOrGetExistingCusCodeList(EnterpriseCoreUniversal.RefDataGrouping.Codes.CommonDataGrouping, EnterpriseCoreUniversal.RefCusCodeListTypes.Codes.ManifestValidationRule, "OfficeCode", "A Customs Office Code is required", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(zzValidationRule.PK, "MANDATORY", "");
			Factory.Save();
			var headerTR = Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Turkey, "ATAIHR");
			headerTR.AMA_CustomsOffice = "~~";
			AssertHasMessageErrorContaining(headerTR.AMA_CustomsOfficeInfo, "list");
			headerTR.AMA_CustomsOffice = "";
			AssertNoNotifications(headerTR.AMA_CustomsOfficeInfo);
			headerTR.AMA_CustomsOffice = officeCode;
			AssertNoNotifications("After set value, there should not be message error on AMA_CustomsOffice", headerTR.AMA_CustomsOfficeInfo);
		}

		public void TestCheckTransportType()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			manifestHeader.TransportType = "AA";
			AssertHasMessageError(manifestHeader.TransportTypeInfo, ListValidation.InvalidCodeMessageError);
			manifestHeader.TransportType = TRTransportTypes.Codes.TT10;
			AssertNoMessageErrorContaining(manifestHeader.TransportTypeInfo, ListValidation.InvalidCodeMessageError);
			manifestHeader.TransportType = "";
			AssertHasMessageErrorContaining(manifestHeader.TransportTypeInfo, MandatoryValidation.YouHaveNotEntered);

			manifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			manifestHeader.AMA_ManifestType = TRManifestTypes.Codes.GRUPAJ;
			manifestHeader.TransportType = ZString.Empty;
			AssertNoMessageErrorContaining(manifestHeader.TransportTypeInfo, ListValidation.InvalidCodeMessageError);

			manifestHeader.AMA_ManifestType = TRManifestTypes.Codes.DENIHR;
			manifestHeader.TransportType = ZString.Empty;
			AssertHasMessageErrorContaining(manifestHeader.TransportTypeInfo, MandatoryValidation.YouHaveNotEntered);

			manifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Air;
			manifestHeader.AMA_ManifestType = TRManifestTypes.Codes.HAVIHR;
			manifestHeader.TransportType = ZString.Empty;
			AssertHasMessageErrorContaining(manifestHeader.TransportTypeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckTIRNumber()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
			header.AMA_ManifestType = TRManifestTypes.Codes.ATAIHR;
			header.TIRNumber = "TIR2";
			AssertNoMessageErrorContaining("After set value, there should not be message error on TIRNumber", header.TIRNumberInfo, MandatoryValidation.YouHaveNotEntered);
			header.TIRNumber = "";
			AssertHasMessageErrorContaining(header.TIRNumberInfo, MandatoryValidation.YouHaveNotEntered);
			header.AMA_ManifestType = TRManifestTypes.Codes.ATAITH;
			header.TIRNumber = "TIR2";
			AssertNoMessageErrorContaining("After set value, there should not be message error on TIRNumber", header.TIRNumberInfo, MandatoryValidation.YouHaveNotEntered);
			header.TIRNumber = "";
			AssertHasMessageErrorContaining(header.TIRNumberInfo, MandatoryValidation.YouHaveNotEntered);
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			header.TIRNumber = "";
			AssertNoMessageErrorContaining("After set value, there should not be message error on TIRNumber", header.TIRNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckAMA_LloydsNumber()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			AssertValidationOnLloydsNumber(manifestHeader, TRManifestTypes.Codes.CIKONC, true);
			AssertValidationOnLloydsNumber(manifestHeader, TRManifestTypes.Codes.DENIHR, true);
			AssertValidationOnLloydsNumber(manifestHeader, TRManifestTypes.Codes.DENITH, true);
			AssertValidationOnLloydsNumber(manifestHeader, TRManifestTypes.Codes.VARONC, true);
			AssertValidationOnLloydsNumber(manifestHeader, TRManifestTypes.Codes.EMANIF, true);
			AssertValidationOnLloydsNumber(manifestHeader, TRManifestTypes.Codes.ATAIHR, false);
			manifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Air;
			manifestHeader.AMA_ManifestType = TRManifestTypes.Codes.HAVIHR;
			manifestHeader.AMA_LloydsNumber = "";
			AssertNoMessageErrorContaining("There should not be message error on AMA_LloydsNumber", manifestHeader.AMA_LloydsNumberInfo, MandatoryValidation.YouHaveNotEntered);
			manifestHeader.AMA_ManifestType = TRManifestTypes.Codes.HAVITH;
			manifestHeader.AMA_LloydsNumber = "";
			AssertHasMessageErrorContaining(manifestHeader.AMA_LloydsNumberInfo, MandatoryValidation.YouHaveNotEntered);
			manifestHeader.AMA_ManifestType = TRManifestTypes.Codes.VARONC;
			manifestHeader.AMA_LloydsNumber = "";
			AssertHasMessageErrorContaining(manifestHeader.AMA_LloydsNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		void AssertValidationOnLloydsNumber(AsycudaManifestHeader manifestHeader, ZString manifestType, bool shouldHasMessageErrorIfEmpty)
		{
			manifestHeader.AMA_ManifestType = manifestType;
			manifestHeader.AMA_LloydsNumber = "Test";
			AssertNoMessageErrorContaining("After set value, there should not be message error on AMA_LloydsNumber", manifestHeader.AMA_LloydsNumberInfo, MandatoryValidation.YouHaveNotEntered);
			manifestHeader.AMA_LloydsNumber = "";
			if (shouldHasMessageErrorIfEmpty)
			{
				AssertHasMessageErrorContaining(manifestHeader.AMA_LloydsNumberInfo, MandatoryValidation.YouHaveNotEntered);
			}
			else
			{
				AssertNoMessageErrorContaining("After set value, there should not be message error on AMA_LloydsNumber", manifestHeader.AMA_LloydsNumberInfo, MandatoryValidation.YouHaveNotEntered);
			}
		}

		public void TestCheckAMA_DateAtCustomsOffice()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			AssertValidationOnDateAtCustomsOffice(manifestHeader, TRManifestTypes.Codes.CIKONC, false);
			AssertValidationOnDateAtCustomsOffice(manifestHeader, TRManifestTypes.Codes.DENIHR, true);
			AssertValidationOnDateAtCustomsOffice(manifestHeader, TRManifestTypes.Codes.DENITH, true);
			AssertValidationOnDateAtCustomsOffice(manifestHeader, TRManifestTypes.Codes.HAVIHR, true);
			AssertValidationOnDateAtCustomsOffice(manifestHeader, TRManifestTypes.Codes.HAVITH, true);
			AssertValidationOnDateAtCustomsOffice(manifestHeader, TRManifestTypes.Codes.VARONC, false);
			AssertValidationOnDateAtCustomsOffice(manifestHeader, TRManifestTypes.Codes.EMANIF, true);
			AssertValidationOnDateAtCustomsOffice(manifestHeader, TRManifestTypes.Codes.ATAIHR, false);
			manifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Air;
			AssertValidationOnDateAtCustomsOffice(manifestHeader, TRManifestTypes.Codes.CIKONC, false);
			AssertValidationOnDateAtCustomsOffice(manifestHeader, TRManifestTypes.Codes.VARONC, false);
		}

		void AssertValidationOnDateAtCustomsOffice(AsycudaManifestHeader manifestHeader, ZString manifestType, bool shouldHasMessageErrorIfEmpty)
		{
			manifestHeader.AMA_ManifestType = manifestType;
			manifestHeader.AMA_DateAtCustomsOffice = ZDate.Today;
			AssertNoMessageErrorContaining(manifestHeader.AMA_DateAtCustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);
			manifestHeader.AMA_DateAtCustomsOffice = ZDate.Empty;
			if (shouldHasMessageErrorIfEmpty)
			{
				AssertHasMessageErrorContaining(manifestHeader.AMA_DateAtCustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);
			}
			else
			{
				AssertNoMessageErrorContaining("After set value, there should not be message error on AMA_DateAtCustomsOffice", manifestHeader.AMA_DateAtCustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);
			}
		}

		readonly ZString sameRegistrationNoErrorMessage = "Duplicate registration number";
		readonly ZString sameBillNoErrorMessage = "Duplicate bill number";
		readonly ZString sameBillLineNoErrorMessage = "Duplicate bill line number";
		public void TestManifestToOpenDuplicates()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var manifestToOpen1 = manifestHeader.ManifestsToOpenList.AddNew();
			manifestToOpen1.CSI_SubType = SubTypeListForManifestToOpen.Codes.Manifestlevel;
			manifestToOpen1.CSI_ReferenceNumber2 = "X";
			var manifestToOpen2 = manifestHeader.ManifestsToOpenList.AddNew();
			manifestToOpen2.CSI_SubType = SubTypeListForManifestToOpen.Codes.Manifestlevel;
			manifestToOpen2.CSI_ReferenceNumber2 = "X";
			CombineAssertions(() =>
			{
				Assert(manifestToOpen1.RowMessageErrors.Contains(sameRegistrationNoErrorMessage));
				Assert(manifestToOpen2.RowMessageErrors.Contains(sameRegistrationNoErrorMessage));
			});
			manifestToOpen2.CSI_ReferenceNumber2 = "Y";
			CombineAssertions(() =>
			{
				Assert(!manifestToOpen1.RowMessageErrors.Contains(sameRegistrationNoErrorMessage));
				Assert(!manifestToOpen2.RowMessageErrors.Contains(sameRegistrationNoErrorMessage));
			});
			manifestToOpen1.CSI_SubType = SubTypeListForManifestToOpen.Codes.Billlevel;
			manifestToOpen1.BillNo = "22067777IM12345678";
			manifestToOpen2.CSI_SubType = SubTypeListForManifestToOpen.Codes.Billlevel;
			manifestToOpen2.CSI_ReferenceNumber2 = "X";
			manifestToOpen2.BillNo = "22067777IM12345678";
			CombineAssertions(() =>
			{
				Assert(manifestToOpen1.RowMessageErrors.Contains(sameBillNoErrorMessage));
				Assert(manifestToOpen2.RowMessageErrors.Contains(sameBillNoErrorMessage));
			});
			manifestToOpen2.CSI_ReferenceNumber2 = "Y";
			CombineAssertions(() =>
			{
				Assert(!manifestToOpen1.RowMessageErrors.Contains(sameBillNoErrorMessage));
				Assert(!manifestToOpen2.RowMessageErrors.Contains(sameBillNoErrorMessage));
			});
			manifestToOpen2.CSI_ReferenceNumber2 = "X";
			manifestToOpen2.BillNo = "22067777IM12345679";
			CombineAssertions(() =>
			{
				Assert(!manifestToOpen1.RowMessageErrors.Contains(sameBillNoErrorMessage));
				Assert(!manifestToOpen2.RowMessageErrors.Contains(sameBillNoErrorMessage));
			});
			manifestToOpen1.CSI_SubType = SubTypeListForManifestToOpen.Codes.Billlinelevel;
			manifestToOpen1.CSI_LineNo = 1;
			manifestToOpen2.CSI_SubType = SubTypeListForManifestToOpen.Codes.Billlinelevel;
			manifestToOpen2.BillNo = "22067777IM12345678";
			manifestToOpen2.CSI_LineNo = 1;
			CombineAssertions(() =>
			{
				Assert(manifestToOpen1.RowMessageErrors.Contains(sameBillLineNoErrorMessage));
				Assert(manifestToOpen2.RowMessageErrors.Contains(sameBillLineNoErrorMessage));
			});
			manifestToOpen2.CSI_ReferenceNumber2 = "Y";
			CombineAssertions(() =>
			{
				Assert(!manifestToOpen1.RowMessageErrors.Contains(sameBillLineNoErrorMessage));
				Assert(!manifestToOpen2.RowMessageErrors.Contains(sameBillLineNoErrorMessage));
			});
			manifestToOpen2.CSI_ReferenceNumber2 = "X";
			manifestToOpen2.BillNo = "22067777IM12345679";
			CombineAssertions(() =>
			{
				Assert(!manifestToOpen1.RowMessageErrors.Contains(sameBillLineNoErrorMessage));
				Assert(!manifestToOpen2.RowMessageErrors.Contains(sameBillLineNoErrorMessage));
			});
			manifestToOpen2.CSI_ReferenceNumber2 = "X";
			manifestToOpen2.BillNo = "22067777IM12345678";
			manifestToOpen2.CSI_LineNo = 2;
			CombineAssertions(() =>
			{
				Assert(!manifestToOpen1.RowMessageErrors.Contains(sameBillLineNoErrorMessage));
				Assert(!manifestToOpen2.RowMessageErrors.Contains(sameBillLineNoErrorMessage));
			});
		}

		public void TestCheckTR_GM_PresentationCustomsOffice()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			CombineAssertions(() =>
			{
				header.AMA_ManifestType = TRManifestTypes.Codes.TESLIM;
				header.TR_GM_PresentationCustomsOffice = ZString.Empty;
				AssertHasMessageErrorContaining("Error, TR_GM_PresentationCustomsOffice", header.TR_GM_PresentationCustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);
				header.TR_GM_PresentationCustomsOffice = "TR340300";
				AssertNoMessageErrorContaining("No Error, TR_GM_PresentationCustomsOffice", header.TR_GM_PresentationCustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);
				header.AMA_TransportMode = Core.Constants.TransportModes.Air;
				header.AMA_ManifestType = TRManifestTypes.Codes.VARONC;
				header.TR_GM_PresentationCustomsOffice = ZString.Empty;
				AssertNoMessageErrorContaining("No Error, TR_GM_PresentationCustomsOffice", header.TR_GM_PresentationCustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);
				header.AMA_TransportMode = Core.Constants.TransportModes.Air;
				header.AMA_ManifestType = TRManifestTypes.Codes.CIKONC;
				header.TR_GM_PresentationCustomsOffice = ZString.Empty;
				AssertNoMessageErrorContaining("No Error, TR_GM_PresentationCustomsOffice", header.TR_GM_PresentationCustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);
				header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
				header.AMA_ManifestType = TRManifestTypes.Codes.VARONC;
				header.TR_GM_PresentationCustomsOffice = ZString.Empty;
				AssertNoMessageErrorContaining("No Error, TR_GM_PresentationCustomsOffice", header.TR_GM_PresentationCustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);
				header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
				header.AMA_ManifestType = TRManifestTypes.Codes.CIKONC;
				header.TR_GM_PresentationCustomsOffice = ZString.Empty;
				AssertNoMessageErrorContaining("No Error, TR_GM_PresentationCustomsOffice", header.TR_GM_PresentationCustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);
				header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
				header.AMA_ManifestType = TRManifestTypes.Codes.DENIHR;
				header.TR_GM_PresentationCustomsOffice = ZString.Empty;
				AssertHasMessageErrorContaining("Error, TR_GM_PresentationCustomsOffice", header.TR_GM_PresentationCustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);
				header.AMA_TransportMode = Core.Constants.TransportModes.Air;
				header.AMA_ManifestType = TRManifestTypes.Codes.HAVIHR;
				header.TR_GM_PresentationCustomsOffice = ZString.Empty;
				AssertHasMessageErrorContaining("Error, TR_GM_PresentationCustomsOffice", header.TR_GM_PresentationCustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckAMA_OA_Carrier()
		{
			var carrierOrgAddress = Factory.NewWithValidTestData<OrgAddress>();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_OA_Carrier = ZGuid.Empty;
			AssertHasWarningContaining(header.AMA_OA_CarrierInfo, "You have not entered a Carrier");
			header.AMA_OA_Carrier = carrierOrgAddress.PK;
			AssertNoMessageErrorContaining(header.AMA_OA_CarrierInfo, "You have not entered a Carrier");
			header.AMA_ManifestType = TRManifestTypes.Codes.GRUPAJ;
			header.AMA_OA_Carrier = ZGuid.Empty;
			AssertNoWarningContaining(header.AMA_OA_CarrierInfo, "You have not entered a Carrier");
		}

		public void TestCheckAMA_E_ARV()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = "SEA";
			header.AMA_ManifestType = TRManifestTypes.Codes.DENIHR;
			header.AMA_E_ARV = ZDateTime.Empty;
			AssertNoMessageErrors(header.AMA_E_ARVInfo);
		}

		public void TestABL_E_DEP()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = "SEA";
			header.AMA_ManifestType = TRManifestTypes.Codes.DENIHR;
			header.AMA_E_DEP = ZDateTime.Empty;
			AssertNoMessageErrors(header.AMA_E_DEPInfo);
		}

		public void TestCheckAMA_MasterBillForAir()
		{
			var errorMessage = "There is another Global Manifest which is registered with this airway bill number.";
			var currentYear = (ZString)ZDateTime.Today.Year.ToString();
			currentYear = currentYear.SubstringSafe(2);

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = "TR";
			header.AMA_JobReference = "MAN-001";
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			header.AMA_MasterBill = "01234567890";
			header.RegistrationNumber = currentYear + "IM340300IM202356";
			header.RegistrationDate = new ZDateTime(ZDateTime.Today.Year, 10, 10);
			Factory.Save();

			AssertNoErrorContaining(header.AMA_MasterBillInfo, errorMessage);

			var header2 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = "TR";
			header2.AMA_JobReference = "MAN-002";
			header2.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header2.AMA_Nature = ShipmentTypeList.Codes.Import23;
			header2.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			header2.AMA_MasterBill = "01234567890";

			CombineAssertions(() =>
			{
				AssertHasErrorContaining(header2.AMA_MasterBillInfo, errorMessage);

				header2.AMA_Nature = ShipmentTypeList.Codes.Export22;
				header2.Validation.ValidateAll();

				AssertNoErrorContaining(header.AMA_MasterBillInfo, errorMessage);

				header2.AMA_Nature = ShipmentTypeList.Codes.Import23;
				header2.AMA_TransportMode = Core.Constants.TransportModes.Air;
				header2.Validation.ValidateAll();
				AssertNoErrorContaining(header.AMA_MasterBillInfo, errorMessage);
			});

			var header3 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header3.AMA_JobReference = "MAN-003";
			header3.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header3.AMA_Nature = ShipmentTypeList.Codes.Import23;
			header3.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			header3.AMA_MasterBill = "01234567890";
			header3.AMA_ManifestType = TRManifestTypes.Codes.HAVITH;
			CombineAssertions("VARONC & GRUPAJ", () =>
			{
				header3.Validation.ValidateAll();
				AssertHasErrorContaining(header3.AMA_MasterBillInfo, errorMessage);

				header3.AMA_ManifestType = TRManifestTypes.Codes.VARONC;
				header3.Validation.ValidateAll();
				AssertNoErrorContaining("VARONC", header3.AMA_MasterBillInfo, errorMessage);

				header3.AMA_ManifestType = TRManifestTypes.Codes.GRUPAJ;
				header3.Validation.ValidateAll();
				AssertNoErrorContaining("GRUPAJ", header3.AMA_MasterBillInfo, errorMessage);
			});
		}

		public void TestCheckAMA_MasterBillForSea()
		{
			var errorMessage = "There is another Global Manifest which is registered with this BOL number. Please check it to prevent an error.";
			var currentYear = (ZString)ZDateTime.Today.Year.ToString();
			currentYear = currentYear.SubstringSafe(2);

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = "TR";
			header.AMA_JobReference = "MAN-001";
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
			header.AMA_MasterBill = "01234567890";
			header.RegistrationNumber = currentYear + "IM340300IM202356";
			header.RegistrationDate = new ZDateTime(ZDateTime.Today.Year, 10, 10);
			Factory.Save();

			AssertNoErrorContaining(header.AMA_MasterBillInfo, errorMessage);

			var header2 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = "TR";
			header2.AMA_JobReference = "MAN-002";
			header2.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header2.AMA_Nature = ShipmentTypeList.Codes.Import23;
			header2.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
			header2.AMA_MasterBill = "01234567890";

			CombineAssertions(() =>
			{
				AssertHasMessageErrorContaining(header2.AMA_MasterBillInfo, errorMessage);

				header2.AMA_Nature = ShipmentTypeList.Codes.Export22;
				header2.Validation.ValidateAll();

				AssertNoMessageErrorContaining(header.AMA_MasterBillInfo, errorMessage);

				header2.AMA_Nature = ShipmentTypeList.Codes.Import23;
				header2.AMA_TransportMode = Core.Constants.TransportModes.Air;
				header2.Validation.ValidateAll();

				AssertNoMessageErrorContaining(header.AMA_MasterBillInfo, errorMessage);
			});

			var header3 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header3.AMA_JobReference = "MAN-003";
			header3.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header3.AMA_Nature = ShipmentTypeList.Codes.Import23;
			header3.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
			header3.AMA_MasterBill = "01234567890";
			header3.AMA_ManifestType = TRManifestTypes.Codes.DENITH;
			CombineAssertions("VARONC & GRUPAJ", () =>
			{
				header3.Validation.ValidateAll();
				AssertHasMessageErrorContaining(header3.AMA_MasterBillInfo, errorMessage);

				header3.AMA_ManifestType = TRManifestTypes.Codes.VARONC;
				header3.Validation.ValidateAll();
				AssertNoMessageErrorContaining("VARONC", header3.AMA_MasterBillInfo, errorMessage);

				header3.AMA_ManifestType = TRManifestTypes.Codes.GRUPAJ;
				header3.Validation.ValidateAll();
				AssertNoMessageErrorContaining("GRUPAJ", header3.AMA_MasterBillInfo, errorMessage);
			});
		}
	}
}

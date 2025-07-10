using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.SG.Access.Business.Testing
{
	sealed class AsycudaManifestHeaderValidationTest : BusinessObjectValidationTestCase
	{
		[TestDate(2018, 10, 24)]
		public void TestValidateCustomsPort()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(RefCusCodeListTypes.Codes.Port, "Port");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Singapore, RefCusCodeListTypes.Codes.Port, "ADZZZ", "O P ANDORRA", new ZDateTime(2018, 10, 1), new ZDateTime(2018, 10, 31));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Singapore, RefCusCodeListTypes.Codes.Port, "AEAJM", "AJMAN", new ZDateTime(2018, 10, 1), new ZDateTime(2018, 10, 31));
			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = "SG";
			header.MasterBill.Validation.ValidateABL_CustomsDischargePort();
			AssertHasMessageErrorContaining(header.AMA_CustomsDischargePortInfo, MandatoryValidation.YouHaveNotEntered);
			header.AMA_CustomsDischargePort = "XXX";
			AssertHasMessageErrorContaining(header.AMA_CustomsDischargePortInfo, ListValidation.InvalidCodeMessageError);
			header.AMA_CustomsDischargePort = "ADZZZ";
			AssertNoMessageErrorContaining(header.AMA_CustomsDischargePortInfo, ListValidation.InvalidCodeMessageError);
			header.MasterBill.Validation.ValidateABL_CustomsLoadPort();
			AssertHasMessageErrorContaining(header.AMA_CustomsLoadPortInfo, MandatoryValidation.YouHaveNotEntered);
			header.AMA_CustomsLoadPort = "XXX";
			AssertHasMessageErrorContaining(header.AMA_CustomsLoadPortInfo, ListValidation.InvalidCodeMessageError);
			header.AMA_CustomsLoadPort = "AEAJM";
			AssertNoMessageErrorContaining(header.AMA_CustomsLoadPortInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestPortOfLoadingForSG()
		{
			var sgRegistry = ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>();
			sgRegistry.ACCESSEnable.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeList("ZZ", RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.SouthAfrica, "South Africa", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("ZZ", RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Fiji, "Fiji", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("ZZ", RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Vanuatu, "Vanuatu", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("ZZ", RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.SolomonIslands, "Solomon Islands", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("ZZ", RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Singapore, "Singapore", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var air = "ZAR#@";
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = air;
			unloco.RL_PortName = air + " NAME";
			unloco.RL_HasAirport = true;
			var sea = "FJE%#";
			unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = sea;
			unloco.RL_PortName = sea + " NAME";
			unloco.RL_HasSeaport = true;
			var road = "VUL$#";
			unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = road;
			unloco.RL_PortName = road + " NAME";
			unloco.RL_HasRoad = true;
			var mail = "SBH$#";
			unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = mail;
			unloco.RL_PortName = mail + " NAME";
			unloco.RL_HasPost = true;
			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_ManifestType = Business.Constants.ManifestType.Export;
			header.AMA_RL_NKPortOfDischarge = "SGSIN";
			header.AMA_RL_NKPortOfLoading = "ZACPT";
			AssertHasMessageError(header.AMA_RL_NKPortOfLoadingInfo, ValidationConstants.PortMustBeSingaporePortForExport("Load Port"));
			header.AMA_RL_NKPortOfLoading = "SGSIN";
			AssertNoMessageError(header.AMA_RL_NKPortOfLoadingInfo, ValidationConstants.PortMustBeSingaporePortForExport("Load Port"));
			header.AMA_ManifestType = Business.Constants.ManifestType.Import;
			header.AMA_RL_NKPortOfLoading = "SGSIN";
			AssertHasMessageError(header.AMA_RL_NKPortOfLoadingInfo, ValidationConstants.PortCannotBeSingaporePortForImport("Load Port"));
			header.AMA_RL_NKPortOfLoading = "ZACPT";
			AssertNoMessageError(header.AMA_RL_NKPortOfLoadingInfo, ValidationConstants.PortCannotBeSingaporePortForImport("Load Port"));
		}

		public void TestPortOfDischargeForSG()
		{
			var sgRegistry = ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>();
			sgRegistry.ACCESSEnable.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeList("ZZ", RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.SouthAfrica, "South Africa", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("ZZ", RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Fiji, "Fiji", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("ZZ", RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Vanuatu, "Vanuatu", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("ZZ", RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.SolomonIslands, "Solomon Islands", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("ZZ", RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Singapore, "Singapore", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var air = "ZAR#@";
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = air;
			unloco.RL_PortName = air + " NAME";
			unloco.RL_HasAirport = true;
			var sea = "FJE%#";
			unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = sea;
			unloco.RL_PortName = sea + " NAME";
			unloco.RL_HasSeaport = true;
			var road = "VUL$#";
			unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = road;
			unloco.RL_PortName = road + " NAME";
			unloco.RL_HasRoad = true;
			var mail = "SBH$#";
			unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = mail;
			unloco.RL_PortName = mail + " NAME";
			unloco.RL_HasPost = true;
			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_ManifestType = Business.Constants.ManifestType.Export;
			header.AMA_RL_NKPortOfLoading = "SGSIN";
			header.AMA_RL_NKPortOfDischarge = "SGSIN";
			AssertHasMessageError(header.AMA_RL_NKPortOfDischargeInfo, ValidationConstants.PortCannotSingaporePortForExport("Discharge port"));
			header.AMA_RL_NKPortOfDischarge = "ZACPT";
			AssertNoMessageError(header.AMA_RL_NKPortOfDischargeInfo, ValidationConstants.PortCannotSingaporePortForExport("Discharge port"));
			header.AMA_ManifestType = Business.Constants.ManifestType.Import;
			header.AMA_RL_NKPortOfDischarge = "ZACPT";
			AssertHasMessageError(header.AMA_RL_NKPortOfDischargeInfo, ValidationConstants.PortMustBeSingaporePortForImport("Discharge port"));
			header.AMA_RL_NKPortOfDischarge = "SGSIN";
			AssertNoMessageError(header.AMA_RL_NKPortOfDischargeInfo, ValidationConstants.PortMustBeSingaporePortForImport("Discharge port"));
		}

		public void TestTransportModeForSG()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			AssertNoMessageError(header.AMA_TransportModeInfo, ValidationConstants.SGManifestValidOnlyForAirAndRoad);
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			AssertHasMessageError(header.AMA_TransportModeInfo, ValidationConstants.SGManifestValidOnlyForAirAndRoad);
			header.AMA_TransportMode = Core.Constants.TransportModes.Mail;
			AssertHasMessageError(header.AMA_TransportModeInfo, ValidationConstants.SGManifestValidOnlyForAirAndRoad);
			header.AMA_TransportMode = Core.Constants.TransportModes.Road;
			AssertNoMessageError(header.AMA_TransportModeInfo, ValidationConstants.SGManifestValidOnlyForAirAndRoad);
		}

		public void TestSGRoadTransportOnlyToFromMalaysia()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeList("ZZ", RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.SouthAfrica, "South Africa", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("ZZ", RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Fiji, "Fiji", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("ZZ", RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Vanuatu, "Vanuatu", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("ZZ", RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.SolomonIslands, "Solomon Islands", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("ZZ", RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Singapore, "Singapore", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = Business.Constants.ManifestType.Export;
			header.AMA_TransportMode = Core.Constants.TransportModes.Road;
			header.AMA_RL_NKPortOfLoading = "SGCHG";
			header.AMA_RL_NKPortOfDischarge = "FJNAN";
			AssertHasMessageError(header.AMA_RL_NKPortOfLoadingInfo, ValidationConstants.SGRoadManifestValidOnlyForMalaysia);
			AssertHasMessageError(header.AMA_RL_NKPortOfDischargeInfo, ValidationConstants.SGRoadManifestValidOnlyForMalaysia);
			header.AMA_RL_NKPortOfDischarge = "MYAOG";
			AssertNoMessageError(header.AMA_RL_NKPortOfLoadingInfo, ValidationConstants.SGRoadManifestValidOnlyForMalaysia);
			AssertNoMessageError(header.AMA_RL_NKPortOfDischargeInfo, ValidationConstants.SGRoadManifestValidOnlyForMalaysia);
		}
	}
}

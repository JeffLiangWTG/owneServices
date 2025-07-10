using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	public static class USAESLicenseCodeTestHelper
	{
		public static void CreateNewOrGetExistingCusCodeList(BusinessObjectFactory factory, ZString[] codes, bool saveDirectly = true)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USAESLicenseCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USAESLicenseCode);
			foreach (var code in codes)
			{
				switch (code)
				{
					case USAESLicenseCode.Codes.C30:
						var refCusCodePK = CreateNewOrGetExistingCusCodeListAndAddAttribute(helper, code, "Y", "Y", Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.Mandatory, "", System.Array.Empty<ZString>(), "");
						helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodePK, RefCusCodeListAttributeTypes.Codes.LicenseTypeCodes, Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.BureauOfIndustryAndSecurity);
						break;
					case USAESLicenseCode.Codes.C31:
						CreateNewOrGetExistingCusCodeListAndAddAttribute(helper, code, "Y", "N", Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.Mandatory, "", System.Array.Empty<ZString>(), "");
						break;
					case USAESLicenseCode.Codes.C32:
						CreateNewOrGetExistingCusCodeListAndAddAttribute(helper, code, "Y", "N", Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.Mandatory, "NLR", new ZString[] { TransportTypeList.Codes.FixedTransportInstallations }, "");
						break;
					case USAESLicenseCode.Codes.C33:
						CreateNewOrGetExistingCusCodeListAndAddAttribute(helper, code, "Y", "N", "", "NLR", System.Array.Empty<ZString>(), "");
						break;
					case USAESLicenseCode.Codes.C35:
						CreateNewOrGetExistingCusCodeListAndAddAttribute(helper, code, "Y", "N", Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.Mandatory, "LVS", new ZString[] { TransportTypeList.Codes.FixedTransportInstallations }, "");
						break;
					case USAESLicenseCode.Codes.C36:
						CreateNewOrGetExistingCusCodeListAndAddAttribute(helper, code, "Y", "N", Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.Mandatory, "GBS", new ZString[] { TransportTypeList.Codes.FixedTransportInstallations }, "");
						break;
					case USAESLicenseCode.Codes.C38:
						CreateNewOrGetExistingCusCodeListAndAddAttribute(helper, code, "Y", "N", Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.Mandatory, "TSR", new ZString[] { TransportTypeList.Codes.FixedTransportInstallations }, "");
						break;
					case USAESLicenseCode.Codes.C41:
						CreateNewOrGetExistingCusCodeListAndAddAttribute(helper, code, "Y", "N", Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.Mandatory, "RPL", new ZString[] { TransportTypeList.Codes.FixedTransportInstallations }, "");
						break;
					case USAESLicenseCode.Codes.C44:
						CreateNewOrGetExistingCusCodeListAndAddAttribute(helper, code, "Y", "N", Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.Mandatory, "TSU", new ZString[] { TransportTypeList.Codes.FixedTransportInstallations }, "");
						break;
					case USAESLicenseCode.Codes.C45:
						CreateNewOrGetExistingCusCodeListAndAddAttribute(helper, code, "Y", "N", Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.Mandatory, "BAG", new ZString[] { TransportTypeList.Codes.FixedTransportInstallations }, "");
						break;
					case USAESLicenseCode.Codes.C46:
						CreateNewOrGetExistingCusCodeListAndAddAttribute(helper, code, "Y", "N", Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.Mandatory, "AVS", new ZString[] { TransportTypeList.Codes.FixedTransportInstallations }, "");
						break;
					case USAESLicenseCode.Codes.C53:
						CreateNewOrGetExistingCusCodeListAndAddAttribute(helper, code, "Y", "N", Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.Mandatory, "APP", new ZString[] { TransportTypeList.Codes.FixedTransportInstallations }, "");
						break;
					case USAESLicenseCode.Codes.C54:
						CreateNewOrGetExistingCusCodeListAndAddAttribute(helper, code, "Y", "N", Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.Mandatory, "SS-WRC", new ZString[] { TransportTypeList.Codes.FixedTransportInstallations }, "");
						break;
					case USAESLicenseCode.Codes.C57:
						CreateNewOrGetExistingCusCodeListAndAddAttribute(helper, code, "Y", "N", Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.Mandatory, "VEU", new ZString[] { TransportTypeList.Codes.FixedTransportInstallations }, "");
						break;
					case USAESLicenseCode.Codes.C58:
						CreateNewOrGetExistingCusCodeListAndAddAttribute(helper, code, "Y", "N", Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.Mandatory, "CCD", new ZString[] { TransportTypeList.Codes.FixedTransportInstallations }, "");
						break;
					case USAESLicenseCode.Codes.C59:
						CreateNewOrGetExistingCusCodeListAndAddAttribute(helper, code, "Y", "N", "", "STA", System.Array.Empty<ZString>(), "");
						break;
					case USAESLicenseCode.Codes.C60:
						CreateNewOrGetExistingCusCodeListAndAddAttribute(helper, code, "Y", "N", "", "DY6", System.Array.Empty<ZString>(), "");
						break;
					case USAESLicenseCode.Codes.C63:
						CreateNewOrGetExistingCusCodeListAndAddAttribute(helper, code, "Y", "N", "", "YFA", System.Array.Empty<ZString>(), "");
						break;
					case USAESLicenseCode.Codes.E01:
						CreateNewOrGetExistingCusCodeListAndAddAttribute(helper, code, "Y", "N", Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.NotAllowed, "AEA", new ZString[] { TransportTypeList.Codes.Rail, TransportTypeList.Codes.FixedTransportInstallations }, TransportTypeList.Codes.BorderWaterBorne);
						break;
					case USAESLicenseCode.Codes.SAG:
					case USAESLicenseCode.Codes.SCA:
					case USAESLicenseCode.Codes.S00:
						CreateNewOrGetExistingCusCodeListAndAddAttribute(helper, code, "N", "N", "", "", new ZString[] { TransportTypeList.Codes.FixedTransportInstallations }, "");
						break;
					case USAESLicenseCode.Codes.SAU:
					case USAESLicenseCode.Codes.SGB:
						CreateNewOrGetExistingCusCodeListAndAddAttribute(helper, code, "Y", "N", "", "", new ZString[] { TransportTypeList.Codes.FixedTransportInstallations }, "");
						break;
					case USAESLicenseCode.Codes.S05:
					case USAESLicenseCode.Codes.S61:
					case USAESLicenseCode.Codes.S73:
					case USAESLicenseCode.Codes.S85:
						CreateNewOrGetExistingCusCodeListAndAddAttribute(helper, code, "Y", "Y", "", "", new ZString[] { TransportTypeList.Codes.FixedTransportInstallations }, "");
						break;
					case USAESLicenseCode.Codes.S94:
						CreateNewOrGetExistingCusCodeListAndAddAttribute(helper, code, "Y", "Y", "", "", new ZString[] { TransportTypeList.Codes.FixedTransportInstallations }, TransportTypeList.Codes.PassengerHandCarried);
						break;
					case USAESLicenseCode.Codes.VDO:
					case USAESLicenseCode.Codes.VDS:
						CreateNewOrGetExistingCusCodeListAndAddAttribute(helper, code, "N", "N", "", "", System.Array.Empty<ZString>(), "");
						break;
					case USAESLicenseCode.Codes.OPA:
					case USAESLicenseCode.Codes.T10:
						CreateNewOrGetExistingCusCodeListAndAddAttribute(helper, code, "Y", "N", "", "", System.Array.Empty<ZString>(), "");
						break;
					default:
						throw new System.Exception("Create test data for " + code);
				}
			}
			if (saveDirectly)
			{ factory.Save(); }
		}

		static ZGuid CreateNewOrGetExistingCusCodeListAndAddAttribute(UniversalReferenceTestDataHelper helper, ZString code, ZString licenseRequired, ZString licenseValueRequired, ZString eCCNRequired, ZString aESLicenseCode, ZString[] notAllowedTransports, ZString notAllowedSubTransports)
		{
			var refCusCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USAESLicenseCode, code, code, new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			var refCusCodePK = refCusCode.PK;
			helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodePK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.LicenseRequired, licenseRequired);
			if (licenseValueRequired == "Y")
			{
				helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodePK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.LicenseValueRequired, "Y");
			}
			if (!eCCNRequired.IsEmpty)
			{
				helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodePK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.ECCNRequired, eCCNRequired);
			}
			if (!aESLicenseCode.IsEmpty)
			{
				helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodePK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.AESLicenseCode, aESLicenseCode);
			}
			var transports = new ZString[] { "AIR", "SEA", "FIX", "RAI", "ROA", "MAI", "INW" };
			foreach (var transport in transports)
			{
				if (notAllowedTransports == null || !notAllowedTransports.Contains(transport))
				{
					helper.CreateTransportModeForCusCodeList(refCusCodePK, transport);
				}
			}
			if (!notAllowedSubTransports.IsEmpty)
			{
				helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodePK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.NotAllowedTransportMode, notAllowedSubTransports);
			}
			return refCusCodePK;
		}
	}
}

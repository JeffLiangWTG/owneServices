using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.LicenseManager
{
	public static class LicenseValidationHelper
	{
		public static ZString GetECCNError(ZString licenseType, ZString eccn, BusinessObjectFactory factory, ZDateTime exportDate, string countryCode = "", string destinationCountry = null)
		{
			return GetError(ECCNValidation.GetECCNValidate(factory, countryCode, exportDate, destinationCountry), licenseType, eccn);
		}

		public static ZString GetLicenseNumberError(ZString licenseType, ZString licenseNumber, BusinessObjectFactory factory, ZDateTime exportDate)
		{
			return GetError(LicenseNumberValidation.GetLicenseNumberValidate(factory, exportDate), licenseType, licenseNumber);
		}

		public static ZString GetExportCodeError(ZString licenseType, ZString exportCode)
		{
			return GetError(ExportCodeValidation.GetExportCodeValidate(), licenseType, exportCode);
		}

		public static ZString GetTransportModeError(ZString licenseType, ZString transportMode, BusinessObjectFactory factory, ZDateTime exportDate)
		{
			return GetError(TransportModeValidation.GetTransportModeValidate(factory, exportDate), licenseType, transportMode);
		}

		static ZString GetError(IEnumerable<Func<ZString, ZString, string>> validates, ZString licenseType, ZString valueToValidate)
		{
			var result = ZString.Empty;
			foreach (var validate in validates)
			{
				result = validate(licenseType, valueToValidate);
				if (!result.IsEmpty)
				{
					break;
				}
			}
			return result;
		}

		public static ZBool HasAvailableECCNNumber(BusinessObjectFactory factory, ZString licenseType)
		{
			return GetECCNNumbersByLicenseType(factory, licenseType).Count > 0;
		}

		public static CodeDescriptionPairList GetECCNNumbersByLicenseType(BusinessObjectFactory factory, ZString licenseType)
		{
			return RefCusCodeListTypes.GetCachedListMatchAllAttributes(factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCNNumber, ZDateTime.Today,
			new[] { new KeyValuePair<ZString, ZString>(RefCusCodeListAttributeTypes.Codes.LicenseType, licenseType) });
		}

		public static string AddLicenseTypeValidationErrorMessage(ZString countryOfDestination, ZString licenseType)
		{
			var message = string.Empty;

			if (countryOfDestination == Core.Constants.CountryCodes.Cuba && (
				licenseType != USAESLicenseCode.Codes.C30 &&
				licenseType != USAESLicenseCode.Codes.C51 &&
				licenseType != USAESLicenseCode.Codes.C58 &&
				licenseType != USAESLicenseCode.Codes.C62 &&
				licenseType != USAESLicenseCode.Codes.T10))
			{
				message = LicenseTypeForCuba;
			}
			return message;
		}
		internal const string LicenseTypeForCuba = "License Types must be C30, C51, C58, C62 or T10.";
	}
}

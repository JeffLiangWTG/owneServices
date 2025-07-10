using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.US.Business.LicenseManager
{
	public static class TransportModeValidation
	{
		public static IEnumerable<Func<ZString, ZString, string>> GetTransportModeValidate(BusinessObjectFactory factory, ZDateTime exportDate)
		{
			yield return (licenseType, transportMode) => ValidateTransportModeIsInNotAllowedList(licenseType, transportMode, factory, exportDate);
		}

		static string ValidateTransportModeIsInNotAllowedList(ZString licenseType, ZString transportMode, BusinessObjectFactory factory, ZDateTime exportDate)
		{
			if (!transportMode.IsEmpty && exportDate.IsValid)
			{
				var licenseTypeBO = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, licenseType, Core.Constants.CountryCodes.UnitedStates,
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USAESLicenseCode, exportDate);
				if (licenseTypeBO != null)
				{
					var result = false;
					switch (transportMode)
					{
						case TransportTypeList.Codes.Air:
							result = licenseTypeBO.ZZD_IsAir;
							break;
						case TransportTypeList.Codes.FixedTransportInstallations:
							result = licenseTypeBO.ZZD_IsFix;
							break;
						case TransportTypeList.Codes.Mail:
							result = licenseTypeBO.ZZD_IsMai;
							break;
						case TransportTypeList.Codes.Rail:
							result = licenseTypeBO.ZZD_IsRai;
							break;
						case TransportTypeList.Codes.Road:
						case TransportTypeList.Codes.Auto:
						case TransportTypeList.Codes.PassengerHandCarried:
						case TransportTypeList.Codes.Pedestrian:
						case TransportTypeList.Codes.Truck:
							result = licenseTypeBO.ZZD_IsRoa;
							break;
						case TransportTypeList.Codes.Sea:
						case TransportTypeList.Codes.BorderWaterBorne:
							result = licenseTypeBO.ZZD_IsSea;
							break;
						default:
							result = true;
							break;
					}

					RefCusCodeListAttribute[] notAllowedTransportModeList = new RefCusCodeListAttribute.Loader(factory).Load(Core.Constants.CountryCodes.UnitedStates, exportDate,
							Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USAESLicenseCode, licenseType, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.NotAllowedTransportMode);
					if (result && notAllowedTransportModeList.Any(x => x.ZZE_Value.EqualsIgnoringCase(transportMode)))
					{
						result = false;
					}

					if (!result)
					{
						var notAllowedList = GetNotAllowedList(licenseTypeBO, notAllowedTransportModeList);
						return string.Format(LicenseTypeIsInvalidForTransportModeMessage, (notAllowedList.Length > 1 ? "s" : ""), string.Join(",", notAllowedList));
					}
				}
			}
			return string.Empty;
		}
		internal const string LicenseTypeIsInvalidForTransportModeMessage = "License Type entered is not allowed for the following Transport Mode{0}: {1}.";

		static string[] GetNotAllowedList(ZZRefCusCodeListCombined licenseType, RefCusCodeListAttribute[] notAllowedTransportModeList)
		{
			var result = new List<string>();
			if (!licenseType.ZZD_IsAir)
			{
				result.Add(TransportTypeList.Codes.Air);
			}
			if (!licenseType.ZZD_IsFix)
			{
				result.Add(TransportTypeList.Codes.FixedTransportInstallations);
			}
			if (!licenseType.ZZD_IsMai)
			{
				result.Add(TransportTypeList.Codes.Mail);
			}
			if (!licenseType.ZZD_IsRai)
			{
				result.Add(TransportTypeList.Codes.Rail);
			}
			if (!licenseType.ZZD_IsRoa)
			{
				result.Add(TransportTypeList.Codes.Road);
				result.Add(TransportTypeList.Codes.Auto);
				result.Add(TransportTypeList.Codes.PassengerHandCarried);
				result.Add(TransportTypeList.Codes.Pedestrian);
				result.Add(TransportTypeList.Codes.Truck);
			}
			if (!licenseType.ZZD_IsSea)
			{
				result.Add(TransportTypeList.Codes.Sea);
				result.Add(TransportTypeList.Codes.BorderWaterBorne);
			}

			if (notAllowedTransportModeList != null)
			{
				foreach (var refCusCodeListAttribute in notAllowedTransportModeList)
				{
					if (!result.Contains(refCusCodeListAttribute.ZZE_Value))
					{
						result.Add(refCusCodeListAttribute.ZZE_Value);
					}
				}
			}

			return result.ToArray();
		}
	}
}

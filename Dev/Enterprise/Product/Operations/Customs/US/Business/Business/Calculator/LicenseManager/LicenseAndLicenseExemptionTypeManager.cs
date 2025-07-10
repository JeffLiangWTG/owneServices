using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.US.Business.LicenseManager
{
	public static class LicenseAndLicenseExemptionTypeManager
	{
		public static ZString GetLicenseNumberOrLicenseExemptionMessage(ZString licenseType, ZString licenseNumber, BusinessObjectFactory factory, ZDateTime exportDate)
		{
			var result = ZString.Empty;
			if (exportDate.IsValid)
			{
				if (LicenseNumberValidation.IsLicenseNumberRequired(factory, licenseType, exportDate) && !licenseNumber.IsEmpty)
				{
					result = licenseNumber;
				}
				else
				{
					var aESLicenseCode = new RefCusCodeListAttribute.Loader(factory).Load(Core.Constants.CountryCodes.UnitedStates, exportDate,
							  Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USAESLicenseCode, licenseType, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.AESLicenseCode).FirstOrDefault();
					if (aESLicenseCode != null)
					{
						var exemptionCode = aESLicenseCode.ZZE_Value;
						result = string.Format("{0} - {1}", exemptionCode, new LicenseExemptionTypeList().GetDescriptionFromCode(exemptionCode));
					}
				}
			}

			return result;
		}

		public static ZString GetLicenseNumberOrLicenseExemptionCode(ZString licenseType, ZString licenseNumber, BusinessObjectFactory factory, ZDateTime exportDate)
		{
			var result = licenseNumber;
			if (result.IsEmpty && exportDate.IsValid)
			{
				var aESLicenseCode = new RefCusCodeListAttribute.Loader(factory).Load(Core.Constants.CountryCodes.UnitedStates, exportDate,
							Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USAESLicenseCode, licenseType, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.AESLicenseCode).FirstOrDefault();
				if (aESLicenseCode != null)
				{
					result = aESLicenseCode.ZZE_Value;
				}
			}

			return result;
		}

		public static bool IsRequiredSpaceCode(ZString licenseType, BusinessObjectFactory factory, ZDateTime exportDate)
		{
			var result = false;
			if (exportDate.IsValid)
			{
				var licenseRequired = new RefCusCodeListAttribute.Loader(factory).Load(Core.Constants.CountryCodes.UnitedStates, exportDate,
							  Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USAESLicenseCode, licenseType, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.LicenseRequired).FirstOrDefault();
				if (licenseRequired != null && licenseRequired.ZZE_Value.EqualsIgnoringCase(YesNoDefaultList.Codes.No))
				{
					result = true;
				}
			}
			return result;
		}
	}
}

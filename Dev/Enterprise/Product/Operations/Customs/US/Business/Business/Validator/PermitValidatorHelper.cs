using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.US.Business
{
	public static class PermitValidatorHelper
	{
		static (ZZRefCusCodeListCombined PermitIndicatorCodeList, ZString PermitAttributeValue) GetPermitIndicatorCodeWithAttributeValue(BusinessObjectFactory factory, ZString attributeName, ZString permitIndicator)
		{
			return factory.GetCachedValue<(ZZRefCusCodeListCombined, ZString)>($"PermitIndicatorAttributeValueFor_{attributeName}_{permitIndicator}", () =>
			{
				var permitIndicatorCodeList = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, permitIndicator, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, ZDateTime.Today);
				if (permitIndicatorCodeList != null)
				{
					return (permitIndicatorCodeList, permitIndicatorCodeList.GetAttribute(attributeName));
				}

				return (null, ZString.Empty);
			});
		}

		public static ZString GetPermitFormatErrorMessage(BusinessObjectFactory factory, ZString permitIndicator)
		{
			var result = ZString.Empty;
			var permitCodeWithAttributeValue = GetPermitIndicatorCodeWithAttributeValue(factory, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPerLicFormatErrorText, permitIndicator);
			if (permitCodeWithAttributeValue.PermitIndicatorCodeList != null)
			{
				result += $"The {permitCodeWithAttributeValue.PermitIndicatorCodeList.ZZD_Description} number is invalid.";
			}

			if (!permitCodeWithAttributeValue.PermitAttributeValue.IsEmpty)
			{
				result += $" The format should be {permitCodeWithAttributeValue.PermitAttributeValue.TrimEnd('.')}.";
			}

			return result;
		}

		public static ZString GetPermitFormatMask(BusinessObjectFactory factory, ZString permitIndicator)
		{
			var permitCodeWithAttributeValue = GetPermitIndicatorCodeWithAttributeValue(factory, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPerLicFormatMask, permitIndicator);
			return permitCodeWithAttributeValue.PermitAttributeValue;
		}

		public static PermitValidator GetPermitValidator(ZString permitIndicator, JobComInvoiceLine invoiceLine)
		{
			PermitValidator validator = null;

			if (!permitIndicator.IsEmpty && invoiceLine != null && LicencePermitTypeList.GetLicencePermitTypeList(invoiceLine.Factory).ContainsCode(permitIndicator))
			{
				switch (permitIndicator)
				{
					case LicencePermitTypeList.Codes._01:
						validator = new SteelPermitValidator(invoiceLine);
						break;
					case LicencePermitTypeList.Codes._05:
						validator = new BeefCertificateValidator(invoiceLine);
						break;
					case LicencePermitTypeList.Codes._09:
						validator = new MXCementImportLicenseValidator(invoiceLine);
						break;
					case LicencePermitTypeList.Codes._11:
						validator = new CanadaSoftwoodLumberExportPermitValidator(invoiceLine);
						break;
					case LicencePermitTypeList.Codes._12:
						validator = new CottonShirtingFabricLicenseNumberValidator(invoiceLine);
						break;
					case LicencePermitTypeList.Codes._14:
						validator = new AgriculturalLicenseValidator(invoiceLine);
						break;
					case LicencePermitTypeList.Codes._16:
						validator = new CanadaExportSugarCertificateValidator(invoiceLine);
						break;
					case LicencePermitTypeList.Codes._17:
						validator = new WoolLicenseValidator(invoiceLine);
						break;
					case LicencePermitTypeList.Codes._18:
						validator = new CaribbeanBasinTradePartnershipActCertificateValidator(invoiceLine);
						break;
					case LicencePermitTypeList.Codes._21:
						validator = new USDASugarCertificateValidator(invoiceLine);
						break;
					case LicencePermitTypeList.Codes._22:
						validator = new OrganicProductExemptionCertificateValidator(invoiceLine);
						break;
					case LicencePermitTypeList.Codes._28:
						validator = new AluminumImportLicenseValidator(invoiceLine);
						break;
					default:
						validator = new PermitValidator(permitIndicator, invoiceLine);
						break;
				}
			}

			return validator;
		}
	}
}

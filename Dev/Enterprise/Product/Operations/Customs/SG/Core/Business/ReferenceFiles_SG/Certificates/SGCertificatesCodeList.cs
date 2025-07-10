using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.SG.V4.Business
{
	public static class SGCertificatesCodeList
	{
		public static ZZRefCusCodeListCombined GetCurrentOrMatchingCertificate(BusinessObjectFactory factory, ZString certificateCode) => factory.GetCachedValue("GetCurrentOrMatchingCert" + certificateCode, delegate
		{
			return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, certificateCode, Core.Constants.CountryCodes.Singapore,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CertificateOfOriginType, ZDateTime.Now);
		});

		#region TemplateType

		static bool IsTemplate2(this ZZRefCusCodeListCombined certificateCode)
		{
			return certificateCode?.HasAttribute(SGConstants.Attributes.Names.COOTemplateCode, SGCertificates.Attributes.CertificateTemplate2) ?? false;
		}

		static bool IsTemplate4(this ZZRefCusCodeListCombined certificateCode)
		{
			return certificateCode?.HasAttribute(SGConstants.Attributes.Names.COOTemplateCode, SGCertificates.Attributes.CertificateTemplate4) ?? false;
		}

		static bool IsTemplate5(this ZZRefCusCodeListCombined certificateCode)
		{
			return certificateCode?.HasAttribute(SGConstants.Attributes.Names.COOTemplateCode, SGCertificates.Attributes.CertificateTemplate5) ?? false;
		}

		static bool IsTemplate999(this ZZRefCusCodeListCombined certificateCode)
		{
			return certificateCode?.HasAttribute(SGConstants.Attributes.Names.COOTemplateCode, SGCertificates.Attributes.CertificateTemplate999) ?? false;
		}

		#endregion

		public static bool IsItemValueAllowed(this ZZRefCusCodeListCombined certificateCode)
		{
			var isItemValueAllowed = false;
			if (certificateCode != null)
			{
				isItemValueAllowed = IsTemplate2(certificateCode) || IsTemplate4(certificateCode) || IsTemplate5(certificateCode);
			}

			return isItemValueAllowed;
		}

		public static bool IsOriginCriterionDetailsRequired(this ZZRefCusCodeListCombined certificateCode)
		{
			var isOriginCriterionReqd = false;
			if (certificateCode != null)
			{
				isOriginCriterionReqd = IsTemplate2(certificateCode) || IsTemplate4(certificateCode);
			}

			return isOriginCriterionReqd;
		}

		public static bool IsTextileDetailsRequired(this ZZRefCusCodeListCombined certificateCode)
		{
			return certificateCode?.IsTemplate5() ?? false;
		}

		public static bool IsManufacturerRequired(this ZZRefCusCodeListCombined certificateCode)
		{
			var isManufacturerReqd = false;
			if (certificateCode != null)
			{
				isManufacturerReqd = IsTemplate2(certificateCode) || IsTemplate5(certificateCode) || IsTemplate999(certificateCode);
			}

			return isManufacturerReqd;
		}

		public static bool IsSGOriginRequired(this ZZRefCusCodeListCombined certificateCode)
		{
			return certificateCode?.IsTemplate2() ?? false;
		}

		public static bool IsInvoiceDetailsAllowed(this ZZRefCusCodeListCombined certificateCode)
		{
			var isInvoiceDetailsAllowed = false;
			if (certificateCode != null)
			{
				isInvoiceDetailsAllowed = IsTemplate2(certificateCode) || IsTemplate4(certificateCode);
			}

			return isInvoiceDetailsAllowed;
		}

		public static bool IsEntryYearRequired(this ZZRefCusCodeListCombined certificateCode)
		{
			var isEntryYearReqd = false;
			if (certificateCode != null)
			{
				isEntryYearReqd = IsTemplate5(certificateCode) || IsTemplate999(certificateCode);
			}

			return isEntryYearReqd;
		}
	}
}

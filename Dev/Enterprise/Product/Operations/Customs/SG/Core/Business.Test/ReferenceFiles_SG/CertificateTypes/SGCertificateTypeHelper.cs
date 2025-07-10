using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public static class SGCertificateTypeHelper
	{
		public static void CreateCertificateTypes(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			CreateSGCoOCodeType(factory, helper);
			var startDate = ZDateTime.Today.AddYears(-15);
			var expiryDate = ZDateTime.Today.AddYears(15);
			var expiredDate = new ZDateTime(2021, 01, 01);
			CreateSGCertificate(helper, "1", "GSP Form A", "COT002");
			CreateSGCertificate(helper, "2", "GSP Form A under Cumulative ASEAN", "COT001", startDate, expiredDate);
			CreateSGCertificate(helper, "3", "Back-to-Back GSP Form A", "COT001", startDate, expiredDate);
			CreateSGCertificate(helper, "4", "Ordinary Certificate of Origin", "COT003", startDate, expiryDate);
			CreateSGCertificate(helper, "4A", "Certificate of Processing", "COT004", startDate, expiryDate);
			CreateSGCertificate(helper, "5", "Commonwealth Preference Certificate", "COT999", startDate, expiredDate);
			CreateSGCertificate(helper, "7", "Form W – reserve", "COT002", startDate, expiryDate);
			CreateSGCertificate(helper, "9", "Ordinary Certificate of Origin for textile products to EU countries only", "COT005", startDate, expiryDate);
			CreateSGCertificate(helper, "10", "Export Certificate for fresh cut orchids", "COT999", startDate, expiredDate);
			CreateSGCertificate(helper, "12", "Global System of Trade Preference (GSTP)", "COT002", startDate, expiryDate);
			CreateSGCertificate(helper, "16", "ATIGA Form D", "COT002", startDate, expiryDate);
			CreateSGCertificate(helper, "17", "Back-to-Back ATIGA Form D", "COT004", startDate, expiryDate);
			CreateSGCertificate(helper, "18", "Preferential CO for FTA", "COT999", startDate, expiryDate);
			CreateSGCertificate(helper, "19", "Asean-China FTA Form E", "COT002", startDate, expiryDate);
			CreateSGCertificate(helper, "20", "Back-to-Back ACFTA Form E", "COT004", startDate, expiryDate);
			CreateSGCertificate(helper, "21", "India-Singapore CECA CO", "COT002", startDate, expiryDate);
			CreateSGCertificate(helper, "22", "Back-to-Back AKFTA Form AK", "COT004", startDate, expiryDate);
			CreateSGCertificate(helper, "23", "Asean-Korea FTA Form AK", "COT002", startDate, expiryDate);
			CreateSGCertificate(helper, "24", "Certificate of Origin Generic Form Z", "COT004", startDate, expiredDate);
			CreateSGCertificate(helper, "25", "Asean-Japan CEP Form AJ", "COT002", startDate, expiryDate);
			CreateSGCertificate(helper, "26", "Back-to-Back AJCEP Form AJ", "COT004", startDate, expiryDate);
			CreateSGCertificate(helper, "27", "Asean-India FTA Form AI", "COT002", startDate, expiryDate);
			CreateSGCertificate(helper, "28", "Back-to-Back AIFTA Form AI", "COT004", startDate, expiryDate);
			CreateSGCertificate(helper, "29", "Asean-Australia-New Zealand FTA Form AANZ", "COT002", startDate, expiryDate);
			CreateSGCertificate(helper, "30", "Back-to-Back AANZFTA Form AANZ", "COT004", startDate, expiryDate);
			CreateSGCertificate(helper, "31", "ASEAN-Hong Kong FTA Form AHK", "COT002", startDate, expiryDate);
			CreateSGCertificate(helper, "32", "Back-to-Back AHKFTA Form AHK", "COT004", startDate, expiryDate);
			CreateSGCertificate(helper, "33", "Reginal Comprehensive Economic Partnership (RCEP) Form RCEP", "COT002", startDate, expiryDate);
			CreateSGCertificate(helper, "34", "Back-to-Back AHKFTA Form AHK", "COT004", startDate, expiryDate);
		}

		public static void CreateSGCoOCodeType(BusinessObjectFactory factory, UniversalReferenceTestDataHelper helper)
		{
			var codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CertificateOfOriginType;
			var sgCoOCodeTypeQuery = new ZQuery(RefCusCodeTypeSchema.ZZK_CodeType, codeType);
			sgCoOCodeTypeQuery.AddToFilter(RefCusCodeTypeSchema.ZZK_ZZZ_NKDataGrouping, Core.Constants.CountryCodes.Singapore);
			var cusCodeType = factory.LoadTop1<RefCusCodeType>(sgCoOCodeTypeQuery);
			if (cusCodeType == null)
			{
				helper.CreateNewOrGetExistingCusCodeType(codeType, "COO Certificate Types", Core.Constants.CountryCodes.Singapore);
			}
		}

		public static RefCusCodeList CreateSGCertificate(UniversalReferenceTestDataHelper helper, string certificateCode, string certificateDesc, string certificateTemplate, ZDateTime? effectiveDate = null, ZDateTime? expiryDate = null)
		{
			var sgCertificateType = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CertificateOfOriginType, certificateCode, certificateDesc, effectiveDate ?? ZDateTime.MinSmallDateTimeValue, expiryDate ?? ZDateTime.MaxSmallDateTimeValue);
			if (!string.IsNullOrEmpty(certificateTemplate))
			{
				helper.CreateNewOrGetExistingCusCodeListAttribute(sgCertificateType.PK, "COOTemplate", certificateTemplate);
			}

			return sgCertificateType;
		}
	}
}

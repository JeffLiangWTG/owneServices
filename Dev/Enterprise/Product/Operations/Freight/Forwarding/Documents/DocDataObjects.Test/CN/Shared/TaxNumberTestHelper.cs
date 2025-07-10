using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Documents.CN.Testing
{
	class TaxNumberTestHelper
	{
		public static void AddNewRefDocOrgCusCode(ZString regulatingCountry, ZString codeCountry, ZString codeType, ZString documentType)
		{
			var factory = new BusinessObjectFactory();
			var refDocOrgCusCodeChina = factory.NewWithValidTestData<RefDocOrgCusCode>();
			refDocOrgCusCodeChina.DOC_DocumentType = documentType;
			refDocOrgCusCodeChina.DOC_RN_NKCodeCountry = codeCountry;
			refDocOrgCusCodeChina.DOC_RN_NKRegulatingCountry = regulatingCountry;
			refDocOrgCusCodeChina.DOC_CodeType = codeType;
			refDocOrgCusCodeChina.DOC_Priority = 1;
			refDocOrgCusCodeChina.DOC_ShortLabel = codeType + "(s)";
			refDocOrgCusCodeChina.DOC_LongLabel = codeType + "(l)";
			refDocOrgCusCodeChina.DOC_Description = codeType + "(desc)";

			factory.Save();
		}
	}
}

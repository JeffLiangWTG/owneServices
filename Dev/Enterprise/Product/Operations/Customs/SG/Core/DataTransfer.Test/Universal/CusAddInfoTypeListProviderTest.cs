using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.DataTransfer.Universal.Testing
{
	partial class UniversalCustomsDataObjectProviderTest
	{
		public void TestCusAddInfoGetListForJobComInvoiceLine()
		{
			var list = new UniversalCustomsDataObjectProvider().TableSpecificCusAddInfoTypeList(JobDeclarationSchema.Constants.Prefix);
			AssertEquals(1, list.Count);
			AssertEquals(CusAddInfoTypeAttribute.Codes.SGCustomsProcedureCode, "Customs Procedure Code", list.GetDescriptionFromCode(CusAddInfoTypeAttribute.Codes.SGCustomsProcedureCode));
		}
	}
}

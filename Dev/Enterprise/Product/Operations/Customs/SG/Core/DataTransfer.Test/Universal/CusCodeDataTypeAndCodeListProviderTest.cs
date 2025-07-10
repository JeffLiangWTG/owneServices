using Enterprise.Customs.Common.SG;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.DataTransfer.Universal.Testing
{
	partial class UniversalCustomsDataObjectProviderTest
	{
		public void TestCusCodeDataGetTypeListForJobDeclaration()
		{
			var list = new UniversalCustomsDataObjectProvider().TableSpecificCusCodeDataTypeList(JobDeclarationSchema.Constants.Prefix);
			AssertEquals(1, list.Count);
			AssertEquals(CusCodeDataTypeList.Codes.CALicenceNumber, CusCodeDataTypeList.Descriptions.CALicenceNumber, list.GetDescriptionFromCode(CusCodeDataTypeList.Codes.CALicenceNumber));
		}

		public void TestCusCodeDataGetTypeListForJobComInvoiceLine()
		{
			var list = new UniversalCustomsDataObjectProvider().TableSpecificCusCodeDataTypeList(JobComInvoiceLineSchema.Constants.Prefix);
			AssertEquals(3, list.Count);
			AssertEquals(CusCodeDataTypeList.Codes.CASCode1, CusCodeDataTypeList.Descriptions.CASCode1, list.GetDescriptionFromCode(CusCodeDataTypeList.Codes.CASCode1));
			AssertEquals(CusCodeDataTypeList.Codes.CASCode2, CusCodeDataTypeList.Descriptions.CASCode2, list.GetDescriptionFromCode(CusCodeDataTypeList.Codes.CASCode2));
			AssertEquals(CusCodeDataTypeList.Codes.CASCode3, CusCodeDataTypeList.Descriptions.CASCode3, list.GetDescriptionFromCode(CusCodeDataTypeList.Codes.CASCode3));
		}
	}
}

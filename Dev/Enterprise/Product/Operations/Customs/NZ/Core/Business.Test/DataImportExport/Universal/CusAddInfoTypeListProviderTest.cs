using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.Data.Universal.Testing
{
	partial class UniversalCustomsDataObjectProviderTest
	{
		public void TestCusAddInfoGetListForJobDeclaration()
		{
			var list = new UniversalCustomsDataObjectProvider().TableSpecificCusAddInfoTypeList(JobDeclarationSchema.Constants.Prefix);
			AssertEquals(1, list.Count);
			AssertEquals(CusAddInfoTypeAttribute.Codes.NZMAFFiles, "MPI Files", list.GetDescriptionFromCode(CusAddInfoTypeAttribute.Codes.NZMAFFiles));
		}
	}
}

using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.MultiLineAddInfos.Testing
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.TypeCodeForTesting)]
	sealed class AddInfoWithTypeCode : Business.Testing.TestAddInfo
	{
		public AddInfoWithTypeCode(ZPropertyInfo addInfoPropertyInfo)
			: base(addInfoPropertyInfo)
		{
		}
	}
}

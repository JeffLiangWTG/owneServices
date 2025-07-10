using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.Data.Universal
{
	static class CusAddInfoTypeListProvider
	{
		public static ICodeDescriptionPairList TableSpecificCusAddInfoTypeList(ZString tableCode)
		{
			ICodeDescriptionPairList result = null;
			switch (tableCode)
			{
				case JobDeclarationSchema.Constants.Prefix:
					result = GetListForJobDeclaration();
					break;
			}
			return result;
		}

		#region Implementation

		static CodeDescriptionPairList GetListForJobDeclaration()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(CusAddInfoTypeAttribute.Codes.NZMAFFiles, Res.GetString("D9133799-C569-4F74-BE4A-F68F405D6E69", "MPI Files"));
			return result;
		}

		#endregion
	}
}

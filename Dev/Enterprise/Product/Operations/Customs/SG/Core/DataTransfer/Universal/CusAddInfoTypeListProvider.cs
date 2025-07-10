using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.DataTransfer.Universal
{
	class CusAddInfoTypeListProvider
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
			result.AddPair(CusAddInfoTypeAttribute.Codes.SGCustomsProcedureCode, Res.GetString("02F4D19B-0764-44A4-9F79-987BDFF4773A", "Customs Procedure Code"));
			return result;
		}

		#endregion
	}
}

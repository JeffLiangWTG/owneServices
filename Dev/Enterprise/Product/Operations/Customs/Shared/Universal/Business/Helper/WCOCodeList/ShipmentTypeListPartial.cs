using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Universal.Helper
{
	public partial class ShipmentTypeList
	{
		public static CodeDescriptionPairList Import23Only()
		{
			return new CodeDescriptionPairList()
			{
				new CodeDescriptionPair(Codes.Import23, Descriptions.Import23)
			};
		}

		public static CodeDescriptionPairList Export22Only()
		{
			return new CodeDescriptionPairList()
			{
				new CodeDescriptionPair(Codes.Export22, Descriptions.Export22)
			};
		}

		public static CodeDescriptionPairList Export22AndImport23()
		{
			return new CodeDescriptionPairList()
			{
				new CodeDescriptionPair(Codes.Export22, Descriptions.Export22),
				new CodeDescriptionPair(Codes.Import23, Descriptions.Import23)
			};
		}
	}
}

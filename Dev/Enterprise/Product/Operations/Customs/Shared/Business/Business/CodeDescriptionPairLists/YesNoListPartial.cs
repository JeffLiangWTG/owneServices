using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	partial class YesNoList :
		Integration.Customs.IYesNoListProvider,
		DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		#region ICodeDescriptionPairListProvider Members

		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return this;
		}

		#endregion

		public static bool IsYes(string value) => value == Codes.Yes;
		public static bool IsNo(string value) => value == Codes.No;
		public static bool IsYesOrNo(string value) => IsYes(value) || IsNo(value);
	}
}

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business
{
	public partial class CustomsPrintIndicatorList : CodeDescriptionPairList, Integration.Customs.ZA.IReleasePrintIndicatorProvider, DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return this;
		}
	}
}

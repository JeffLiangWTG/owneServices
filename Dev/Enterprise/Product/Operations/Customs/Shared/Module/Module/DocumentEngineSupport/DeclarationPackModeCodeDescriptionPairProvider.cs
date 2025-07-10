using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Module
{
	public class DeclarationPackModeCodeDescriptionPairProvider : DeclarationFilterLookupCodeDescriptionPairProvider, Integration.Customs.Shared.IDeclarationPackModeCodeDescriptionPairProvider
	{
		#region ICodeDescriptionPairListProvider Members

		public override ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return FilterBizo.Lookups.ContainerModeList;
		}

		#endregion

	}
}

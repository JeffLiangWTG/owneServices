using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Module
{
	public class DeclarationEntryStatusCodeDescriptionPairProvider : DeclarationFilterLookupCodeDescriptionPairProvider, Integration.Customs.Shared.IDeclarationEntryStatusCodeDescriptionPairProvider
	{
		#region ICodeDescriptionPairListProvider Members

		public override ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return FilterBizo.Lookups.EntryStatusList();
		}

		#endregion
	}
}

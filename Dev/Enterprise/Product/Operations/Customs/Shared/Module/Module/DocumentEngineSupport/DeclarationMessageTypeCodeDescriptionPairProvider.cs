using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Module
{
	public class DeclarationMessageTypeCodeDescriptionPairProvider : DeclarationFilterLookupCodeDescriptionPairProvider, Integration.Customs.Shared.IDeclarationMessageTypeCodeDescriptionPairProvider
	{
		#region ICodeDescriptionPairListProvider Members
		public override ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return FilterBizo.Lookups.MessageTypeList;
		}
		#endregion
	}
}

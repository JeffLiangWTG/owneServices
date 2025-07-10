using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Module
{
	public class DeclarationTransportModeCodeDescriptionPairProvider : DeclarationFilterLookupCodeDescriptionPairProvider, Integration.Customs.Shared.IDeclarationTransportModeCodeDescriptionPairProvider
	{
		#region ICodeDescriptionPairListProvider Members

		public override ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return FilterBizo.Lookups.TransportTypeList;
		}

		#endregion

	}
}

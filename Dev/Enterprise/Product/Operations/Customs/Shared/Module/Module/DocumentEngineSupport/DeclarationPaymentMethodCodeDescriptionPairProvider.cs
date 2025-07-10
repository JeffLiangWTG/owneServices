using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Module
{
	public class DeclarationPaymentMethodCodeDescriptionPairProvider : DeclarationFilterLookupCodeDescriptionPairProvider, Integration.Customs.Shared.IDeclarationPaymentMethodCodeDescriptionPairProvider
	{
		#region ICodeDescriptionPairListProvider Members

		public override ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return FilterBizo.Lookups.PaymentPartyList();
		}

		#endregion
	}
}

using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class AccEPaymentBeneficiaryLookups : AutoAccEPaymentBeneficiaryLookups
	{
		public AccEPaymentBeneficiaryLookups(AutoAccEPaymentBeneficiary parent) : base(parent)
		{
		}

		public CodeDescriptionPairList ProviderCodeList => EPaymentProviderCodes.CodesList;
	}
}

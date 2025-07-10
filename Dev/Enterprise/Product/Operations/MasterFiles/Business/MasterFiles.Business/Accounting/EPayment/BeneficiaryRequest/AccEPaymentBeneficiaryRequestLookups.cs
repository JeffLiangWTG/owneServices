using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class AccEPaymentBeneficiaryRequestLookups : AutoAccEPaymentBeneficiaryRequestLookups
	{
		public AccEPaymentBeneficiaryRequestLookups(AutoAccEPaymentBeneficiaryRequest parent) : base(parent)
		{
		}

		public CodeDescriptionPairList StatusCodeList => EPaymentStatusCodes.BeneficiaryRequest.CodesList;

		public CodeDescriptionPairList ProviderCodeList => EPaymentProviderCodes.CodesList;
	}
}

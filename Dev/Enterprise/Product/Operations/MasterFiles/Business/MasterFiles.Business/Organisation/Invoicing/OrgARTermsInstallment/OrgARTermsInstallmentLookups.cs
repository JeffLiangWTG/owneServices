//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgARTermsInstallmentLookups
//
//    This class should be used for overriding collections in AutoOrgARTermsInstallmentLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgARTermsInstallmentLookups : AutoOrgARTermsInstallmentLookups
	{
		public OrgARTermsInstallmentLookups(AutoOrgARTermsInstallment parent) : base(parent)
		{
		}

		public ReadOnlyCodeDescriptionPairList AgreedPaymentMethodList
		{
			get
			{
				return OrganisationsDataRegistry.Instance.ReceivablesCreditAgreedPaymentMethodsList.Value.GetCodeDescriptionPairList();
			}
		}
	}
}

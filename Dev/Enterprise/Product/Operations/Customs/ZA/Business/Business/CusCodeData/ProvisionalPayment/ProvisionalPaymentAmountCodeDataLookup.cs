using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business
{
	public class ProvisionalPaymentAmountCodeDataLookup : CusCodeDataLookups
	{
		public ProvisionalPaymentAmountCodeDataLookup(AutoCusCodeData parent) : base(parent)
		{
		}

		public CodeDescriptionPairList ProvisionalPaymentTypes
		{
			get
			{
				var ppAmount = (Parent as ProvisionalPaymentAmountCodeData);

				if (ppAmount?.ParentEntryLine?.IsImport ?? true)
				{
					return Factory.GetCachedValue<LineLevelProvisionalPayments>();
				}
				else
				{
					return Factory.GetCachedValue<LineLevelProvisionalPaymentsForExports>();
				}
			}
		}

		public override CodeDescriptionPairList CY_CodeList => ProvisionalPaymentTypes;
	}
}

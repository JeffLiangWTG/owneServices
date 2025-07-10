using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class CusEntryHeaderChargesLookups : Customs.Business.CusEntryHeaderChargesLookups
	{
		public CusEntryHeaderChargesLookups(AutoCusEntryHeaderCharges parent) : base(parent)
		{
		}

		protected new CusEntryHeaderCharges Parent
		{
			get { return (CusEntryHeaderCharges)base.Parent; }
		}

		public CodeDescriptionPairList PaymentTypeCodesList
		{
			get
			{
				if (Parent.IsExporterUnionType)
				{
					return Factory.GetCachedValue<PaymentTypeCodesList>();
				}

				if (Parent.IsStampDutyType)
				{
					return PaymentMethodsList;
				}

				return base.PaymentMethodsList;
			}
		}

		public override CodeDescriptionPairList PaymentMethodsList => Factory.GetCachedValue<MethodOfPaymentList>();

		public CodeDescriptionPairList RateOverrideReasonCodeList => Factory.GetCachedValue<RateOverrideReasonList>();

		public CodeDescriptionPairList TaxOrFeeCodeList => UniversalReferenceDataHelper.GetTaxTypeList(Factory);
	}
}

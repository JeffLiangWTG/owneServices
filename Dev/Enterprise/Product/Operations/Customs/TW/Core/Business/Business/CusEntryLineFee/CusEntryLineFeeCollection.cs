using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.Customs.TW.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.TW.Business
{
	public class CusEntryLineFeeCollection : Customs.Business.CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>
	{
		public CusEntryLineFeeCollection(CusEntryLine entryLine, BusinessObjectFactory factory)
			: base(entryLine, factory)
		{
		}

		public bool HasOverriddenFeeOfGivenCode(ZString feeType)
		{
			return Elements.Cast<CusEntryLineFee>().Any(f => f.CF_ChargeType == feeType && f.CF_RateOverrideReasonCode == TWRateOverrideReasonList.Codes.Override);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			if ((Master.Header?.IsExport ?? false) && child != null)
			{
				var entryLineFee = (CusEntryLineFee)child;
				entryLineFee.TW_Type = RefCusTaxOrFeeCodes.TPF;
				entryLineFee.TW_MethodOfPayment = DutyTaxPaymentMethodList.Codes.CashPayment;
				entryLineFee.TW_RateDuty = UniversalReferenceConstants.MethodOfCalculation.Percentage;
			}
		}
	}
}

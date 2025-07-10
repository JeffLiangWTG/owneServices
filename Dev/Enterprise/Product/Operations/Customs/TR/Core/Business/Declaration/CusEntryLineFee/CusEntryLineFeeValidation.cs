using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.TR.Business.Declaration;

namespace Enterprise.Customs.TR.Business
{
	public class CusEntryLineFeeValidation : EU.Business.Declaration.CusEntryLineFeeValidation
	{
		public CusEntryLineFeeValidation(CusEntryLineFee parent) : base(parent)
		{
		}

		protected new CusEntryLineFee Parent => (CusEntryLineFee)base.Parent;

		protected override void CheckCF_MethodOfCalculation()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.CF_MethodOfCalculationInfo);
			base.CheckCF_MethodOfCalculation();
		}

		protected override void CheckCF_MethodOfPayment()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.CF_MethodOfPaymentInfo);
			base.CheckCF_MethodOfPayment();
		}

		protected override void CheckCF_ChargeType()
		{
			base.CheckCF_ChargeType();
			var entryLine = Parent?.EntryLine;
			if (entryLine != null && entryLine.Fees.Cast<CusEntryLineFee>().Any(x => x.PK != Parent.PK && x.CF_ChargeType == Parent.CF_ChargeType))
			{
				Parent.CF_ChargeTypeInfo.AddError(ResString.GetMultilingualString("FC6B180D-2F60-4826-8BD5-A0387C255EB2", "Duplicate Charge Type"));
			}
		}
	}
}

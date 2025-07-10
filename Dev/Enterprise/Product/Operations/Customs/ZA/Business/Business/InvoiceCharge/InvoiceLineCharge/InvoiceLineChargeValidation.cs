using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.ZA.Business
{
	public class InvoiceLineChargeValidation : Customs.Business.InvoiceLineChargeValidation
	{
		public InvoiceLineChargeValidation(InvoiceLineCharge invoiceLineCharge)
			: base(invoiceLineCharge)
		{
		}

		public new InvoiceLineCharge InvoiceLineCharge
		{
			get { return Parent; }
		}

		protected new InvoiceLineCharge Parent
		{
			get { return (InvoiceLineCharge)base.Parent; }
		}

		protected override void CheckJ7_ChargeType()
		{
			base.CheckJ7_ChargeType();
			var targetInfo = Parent.J7_ChargeTypeInfo;
			if (!targetInfo.HasErrors())
			{
				ListValidation.MessageErrorIfInvalidCode(targetInfo);
			}
		}
	}
}

namespace Enterprise.Customs.SG.V4.Business
{
	public class InvoiceChargeValidation : Customs.Business.BaseInvoiceChargeValidation
	{
		public InvoiceChargeValidation(InvoiceCharge invoiceCharge)
			: base(invoiceCharge)
		{
		}

		public InvoiceCharge InvoiceCharge
		{
			get { return Parent; }
		}

		protected new InvoiceCharge Parent
		{
			get { return (InvoiceCharge)base.Parent; }
		}

		protected override void CheckJ7_ChargeType()
		{
			base.CheckJ7_ChargeType();

			var chargeType = Parent.J7_ChargeType;
			if (!chargeType.IsEmpty
				 && (chargeType == Core.Constants.Customs.CustomsCharges.Codes.OverseasFreight || chargeType == Core.Constants.Customs.CustomsCharges.Codes.OverseasInsurance)
				 && IsInwardDecType
				 && Parent.Invoice.JZ_IncoTerm == Core.Constants.IncoTerms.CostInsuranceAndFreight)
			{
				Parent.J7_ChargeTypeInfo.AddMessageError(Res.GetString("80E99505-29D0-41DB-945C-88DCB6482548", "Freight and Insurance is not permitted when the INCO Term is CIF."));
			}
		}

		bool IsInwardDecType
		{
			get
			{
				var messageType = InvoiceCharge?.Parent?.JobDeclaration?.JE_MessageType ?? string.Empty;
				return messageType == MessageTypeCodeList.Codes.IPT || messageType == MessageTypeCodeList.Codes.INP;
			}
		}
	}
}

namespace Enterprise.Customs.NO.Business
{
	internal class ImportGroupInvoiceChargeValidation : GroupInvoiceChargeValidation
	{
		public ImportGroupInvoiceChargeValidation(GroupInvoiceCharge groupInvoiceCharge)
			: base(groupInvoiceCharge)
		{
		}

		protected override void CheckJ7_Amount()
		{
			base.CheckJ7_Amount();

			if (Parent.J7_ChargeType == NOInvoiceChargeTypesImport.Codes.ValueOfGoodsExported && Parent.J7_Amount == 0)
			{
				string message = Res.GetString("4AF7B1D0-9172-B185-4A14-ED5B89CB5CB3", "Value of Goods Exported should be typed in. This will not be part of calculations (of Duties and VAT). The VGE amount is only used as a part of statistical value.");
				Parent.J7_AmountInfo.AddMessageError(message);
			}
		}
	}
}

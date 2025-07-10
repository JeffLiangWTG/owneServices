namespace Enterprise.Customs.US.Business
{
	static class InvoiceLineEnteredValueForOGA
	{
		public static decimal GetEnteredValueForOGA(this JobComInvoiceLine invoiceLine)
		{
			return invoiceLine.CusEntryLine != null ? invoiceLine.US_CustomsValue : invoiceLine.JI_CustomsValue.Round(0);
		}

		public static void RefreshOGAValueValidation(this JobComInvoiceLine invoiceLine)
		{
			foreach (FDA fda in invoiceLine.FDAs)
			{
				fda.AddInfoValidation.ValidateUS_FDAValue();
			}

			foreach (PGA pga in invoiceLine.LaceyActLines)
			{
				pga.AddInfoValidation.ValidateUS_PGALineValue();
			}

			foreach (FWSHeader fws in invoiceLine.FWSHeaders)
			{
				fws.AddInfoValidation.ValidateUS_Value();
			}

			invoiceLine.FDAValueUSDRunningTotalStringInfo.RefreshBinding();
		}
	}
}

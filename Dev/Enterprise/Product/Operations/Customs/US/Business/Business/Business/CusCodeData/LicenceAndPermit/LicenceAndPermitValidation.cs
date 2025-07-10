namespace Enterprise.Customs.US.Business
{
	public class LicenceAndPermitValidation : Customs.Business.CusCodeDataValidation
	{
		public LicenceAndPermitValidation(LicenceAndPermit licenceAndPermit)
			: base(licenceAndPermit)
		{
			this.invoiceLine = licenceAndPermit.Parent as JobComInvoiceLine;
		}

		readonly JobComInvoiceLine invoiceLine;

		protected override void CheckCY_Data()
		{
			base.CheckCY_Data();

			if (invoiceLine != null && invoiceLine.IsEntrySummaryValidationMode)
			{
				if (invoiceLine.IsSetXLine && (!Parent.CY_Code.IsEmpty || !Parent.CY_Data.IsEmpty))
				{
					Parent.AddRowMessageError(string.Format(CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.DataIsNotNeededForXLine, CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.LicenseAndPermit));
				}
				else if (!Parent.CY_Code.IsEmpty && !Parent.CY_Data.IsEmpty)
				{
					new LicenceValidator().ValidateMiscPermitTypeFormat(Parent.CY_DataInfo, invoiceLine, (x) => Parent.CY_Code);
				}
				else if (Parent.CY_Data.IsEmpty)
				{
					Parent.CY_DataInfo.AddMessageError(PleaseEnterANumber);
				}
			}
		}
		internal const string PleaseEnterANumber = "Please enter a Number";

		protected override void CheckCY_Code()
		{
			base.CheckCY_Code();
			ValidateCY_Data();
		}
	}
}

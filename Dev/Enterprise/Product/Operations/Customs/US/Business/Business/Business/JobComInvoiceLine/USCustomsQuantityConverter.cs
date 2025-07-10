using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class USCustomsQuantityConverter : Customs.Business.BaseCustomsQuantityConverter
	{
		public USCustomsQuantityConverter(JobComInvoiceLine invoiceLine, ZPropertyInfo customsQuantityInfo, ZPropertyInfo customsUnitOfQuantityInfo)
			: base(invoiceLine, customsQuantityInfo, customsUnitOfQuantityInfo)
		{
		}

		public new JobComInvoiceLine InvoiceLine
		{
			get { return (JobComInvoiceLine)base.InvoiceLine; }
		}

		protected override void CalculateCountrySpecificQuantity()
		{
			if ((ZString)customsUnitOfQuantityInfo.Value == ABIUnitOfMeasureList.Codes.Dozen &&
					(InvoiceLine.JI_InvoiceUQ == ABIUnitOfMeasureList.Codes.Pieces ||
					InvoiceLine.JI_InvoiceUQ == ABIUnitOfMeasureList.Codes.Packs ||
					InvoiceLine.JI_InvoiceUQ == ABIUnitOfMeasureList.Codes.Number))
			{
				InvoiceLine.JI_CustomsQuantity = InvoiceLine.JI_InvoiceQuantity / 12;
			}
		}
	}
}

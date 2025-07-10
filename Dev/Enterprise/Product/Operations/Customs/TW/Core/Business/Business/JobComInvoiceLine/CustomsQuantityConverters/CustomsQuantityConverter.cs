namespace Enterprise.Customs.TW.Business
{
	public class CustomsQuantityConverter : BaseCustomsQuantityConverter
	{
		public CustomsQuantityConverter(JobComInvoiceLine invoiceLine)
		: base(invoiceLine, invoiceLine.JI_CustomsQuantityInfo, invoiceLine.JI_CustomsUnitQtyInfo)
		{ }
	}
}

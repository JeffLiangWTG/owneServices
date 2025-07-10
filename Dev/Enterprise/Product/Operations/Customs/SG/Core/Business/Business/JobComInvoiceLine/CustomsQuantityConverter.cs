
using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.V4.Business
{
	public class CustomsQuantityConverter : Customs.Business.BaseCustomsQuantityConverter
	{
		public CustomsQuantityConverter(JobComInvoiceLine invoiceLine, ZPropertyInfoDecimal customsQuantityInfo, ZPropertyInfoString customsUnitOfQuantityInfo)
			: base(invoiceLine, customsQuantityInfo, customsUnitOfQuantityInfo)
		{
		}
	}
}

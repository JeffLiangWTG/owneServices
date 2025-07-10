using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public partial class InvoiceCharge : AutoInvoiceCharge, Integration.Customs.US.IInvoiceCharge
	{
		public InvoiceCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZString J7_RX_NKCurrency
		{
			get => base.J7_RX_NKCurrency;
			set
			{
				var oldValue = J7_RX_NKCurrency;
				base.J7_RX_NKCurrency = value;
				if (!IsCopying && oldValue != J7_RX_NKCurrency && Invoice is JobComInvoiceHeader invoice)
				{
					invoice.EffectiveValuationDateInfo.RefreshBinding();
				}
			}
		}

		protected override void ResetExchangeRateData()
		{
			base.ResetExchangeRateData();

			if (Invoice is JobComInvoiceHeader invoice && invoice.JobDeclaration is JobDeclaration declaration)
			{
				declaration.RefreshExchangeRates();
			}
		}
	}
}

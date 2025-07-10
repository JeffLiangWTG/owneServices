using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public partial class GroupInvoiceCharge : AutoGroupInvoiceCharge, Integration.Customs.US.IGroupInvoiceCharge
	{
		public GroupInvoiceCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new JobComInvoiceGroupHeader GroupInvoice
		{
			get { return (JobComInvoiceGroupHeader)base.GroupInvoice; }
		}

		public override ZString J7_RX_NKCurrency
		{
			get => base.J7_RX_NKCurrency;
			set
			{
				var oldValue = J7_RX_NKCurrency;
				base.J7_RX_NKCurrency = value;
				if (!IsCopying && oldValue != J7_RX_NKCurrency && GroupInvoice is JobComInvoiceGroupHeader groupInvoice)
				{
					groupInvoice.JobComInvoiceHeaders.ForEach(x => x.EffectiveValuationDateInfo.RefreshBinding());
				}
			}
		}

		protected override void ResetExchangeRateData()
		{
			base.ResetExchangeRateData();

			if (GroupInvoice is JobComInvoiceGroupHeader groupInvoice && groupInvoice.JobDeclaration is JobDeclaration declaration)
			{
				declaration.RefreshExchangeRates();
			}
		}
	}
}

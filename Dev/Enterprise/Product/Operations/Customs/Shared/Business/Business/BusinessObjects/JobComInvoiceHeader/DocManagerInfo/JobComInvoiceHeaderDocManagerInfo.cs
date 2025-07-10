using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	class JobComInvoiceHeaderDocManagerInfo : DocManagerInfo
	{
		public JobComInvoiceHeaderDocManagerInfo(BaseJobComInvoiceHeader parent)
			: base(parent, Core.Constants.DocManagerCodes.CommercialInvoice)
		{
		}

		protected BaseJobComInvoiceHeader Invoice
		{
			get { return (BaseJobComInvoiceHeader)BusinessEntity; }
		}

		protected override BusinessObject[] GetRelatedObjects()
		{
			var result = new List<BusinessObject>();
			if (Invoice.AttachedOrders != null)
			{
				result.AddRange(Invoice.AttachedOrders);
			}
			return result.ToArray();
		}
	}
}

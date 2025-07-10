using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Business
{
	public class CusPackageCusPackableItemRelationCollection : Customs.Business.CusPackageCusPackableItemRelationCollection
	{
		public CusPackageCusPackableItemRelationCollection(CusPackage package)
			: base(package)
		{
		}

		protected override IEnumerable<BaseJobComInvoiceLine> GetSortedInvoiceLines(Customs.Business.CusPackingList packingList)
		{
			var result = Enumerable.Empty<BaseJobComInvoiceLine>();
			if (packingList.Declaration != null)
			{
				result = base.GetSortedInvoiceLines(packingList);
			}
			else if (packingList.Invoice is BaseJobComInvoiceHeader header)
			{
				result = header.InvoiceLines.Cast<BaseJobComInvoiceLine>().OrderBy(x => x, new BaseJobComInvoiceLine.LineComparer());
			}
			return result;
		}
	}
}

using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business.N5203
{
	public class BondedGoodsInvoices : IBondedGoodsInvoice
	{
		public BondedGoodsInvoices(GovernmentUniformInvoiceData governmentUniformInvoiceData)
		{
			this.governmentUniformInvoiceData = governmentUniformInvoiceData;
		}

		readonly GovernmentUniformInvoiceData governmentUniformInvoiceData;

		public ZString ID => governmentUniformInvoiceData.CY_Code;

		public ZDecimal ValueAmount => governmentUniformInvoiceData.Amount;
	}
}

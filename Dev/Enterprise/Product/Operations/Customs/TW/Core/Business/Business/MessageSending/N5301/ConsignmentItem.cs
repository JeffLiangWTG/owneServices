using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business.N5301
{
	public class ConsignmentItem : IConsignmentItem
	{
		public ConsignmentItem(CusInBondHeader header)
		{
			this.header = header;
			moveLineItem = header.MovementHeader?.InBondMoveDetail?.InBondMoveLineItem;
		}

		readonly CusInBondHeader header;
		readonly CusInBondMoveLineItem moveLineItem;

		public ZString Split => ZString.Empty;

		public ICommodity Commodity => new Commodity(moveLineItem);

		public IGoodsMeasure GoodsMeasure
		{
			get
			{
				GoodsMeasure result = null;
				var quantity = moveLineItem?.BI_Quantity ?? ZDecimal.Zero;
				var quantityUQ = moveLineItem?.BI_QuantityUQ ?? ZString.Empty;
				if (!quantity.IsEmpty && !quantityUQ.IsEmpty)
				{
					result = new GoodsMeasure(quantity, quantityUQ);
				}
				return result;
			}
		}

		public IPackaging Packaging => new Packaging(moveLineItem, header);

		public IEnumerable<ITransportContractDocument> TransportContractDocuments => header.GetTransportContractDocuments(header.MovementBill, (id, typeCode) => new TransportContractDocumentWrapper(id, typeCode));

		public IOrigin Origin => null;

		public ZString AssociatedGovernmentProcedureCode => null;
	}
}

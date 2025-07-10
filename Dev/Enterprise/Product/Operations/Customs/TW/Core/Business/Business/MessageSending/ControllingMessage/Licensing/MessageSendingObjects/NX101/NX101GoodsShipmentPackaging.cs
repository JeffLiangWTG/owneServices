using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class NX101GoodsShipmentPackaging : IPackaging
	{
		readonly CusTWControllingMessageHeader header;
		readonly JobComInvoiceLine invoiceLine;

		public NX101GoodsShipmentPackaging(CusTWControllingMessageHeader header, JobComInvoiceLine invoiceLine)
		{
			this.header = header;
			this.invoiceLine = invoiceLine;
		}

		ZDecimal IPackaging.QuantityQuantity => ZDecimal.Zero;

		ZString IPackaging.TypeCode => null;

		ZString IPackaging.MarksNumbers => header.IsCertificate15 ? invoiceLine.NX101ShippingMarks : ZString.Empty;

		ZString IPackaging.PackagingMaterialDescription => null;

		ZString IPackaging.Combination => null;

		ZDate IPackaging.PackingDateTime => ZDate.Empty;
	}
}

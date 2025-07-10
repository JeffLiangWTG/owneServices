using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class NX101Packaging : IPackaging
	{
		readonly CusTWControllingMessageHeader header;

		public NX101Packaging(CusTWControllingMessageHeader header)
		{
			this.header = header;
		}

		public ZString MarksNumbers => header.IsCertificate15 ? ZString.Empty : (((CusEntryHeader)header.EntryInstruction?.EntryHeader)?.MarksAndNumbers ?? ZString.Empty);

		public ZString PackagingMaterialDescription => null;

		public ZString Combination => null;

		public ZString TypeCode => null;

		public ZDecimal QuantityQuantity => ZDecimal.Zero;

		ZDate IPackaging.PackingDateTime => ZDate.Empty;
	}
}

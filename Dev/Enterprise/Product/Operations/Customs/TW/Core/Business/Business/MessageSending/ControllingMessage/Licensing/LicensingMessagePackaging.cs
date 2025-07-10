using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class LicensingMessagePackaging : IPackaging
	{
		CusTWControllingMessageHeader Header { get; }

		public LicensingMessagePackaging(CusTWControllingMessageHeader header)
		{
			Header = Argument.NotNull(header, nameof(header));
		}

		public ZString MarksNumbers => ((CusEntryHeader)Header.EntryInstruction?.EntryHeader)?.MarksAndNumbers ?? ZString.Empty;

		public ZString PackagingMaterialDescription => null;

		public ZString Combination => null;

		public ZString TypeCode => null;

		public ZDecimal QuantityQuantity => ZDecimal.Zero;

		ZDate IPackaging.PackingDateTime => ZDate.Empty;
	}
}

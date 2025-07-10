using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business.N5301
{
	public class Packaging : IPackaging
	{
		public Packaging(CusInBondMoveLineItem moveLineItem, CusInBondHeader header)
		{
			this.moveLineItem = moveLineItem;
			this.header = header;
		}

		readonly CusInBondMoveLineItem moveLineItem;

		readonly CusInBondHeader header;

		public ZDecimal QuantityQuantity => ZDecimal.Zero;

		public ZString TypeCode => header?.ArrivalBill?.B0_ManifestUQ ?? ZString.Empty;

		public ZString MarksNumbers => moveLineItem?.BI_MarksAndNumbers ?? ZString.Empty;

		public ZString PackagingMaterialDescription => Combination.Equals(YesNoList.Codes.Yes) ? moveLineItem?.BI_PackagingDescription ?? ZString.Empty : ZString.Empty;

		public ZString Combination => moveLineItem?.TW_IsCoPackaged ?? ZBool.False ? new ZString(YesNoList.Codes.Yes) : ZString.Empty;

		ZDate IPackaging.PackingDateTime => ZDate.Empty;
	}
}

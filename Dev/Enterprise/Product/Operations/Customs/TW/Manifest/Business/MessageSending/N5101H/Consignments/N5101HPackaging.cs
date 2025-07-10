using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class N5101HPackaging : IPackaging
	{
		public N5101HPackaging(AsycudaBill bill)
		{
			this.bill = bill;
		}

		readonly AsycudaBill bill;

		ZDecimal IPackaging.QuantityQuantity => ZDecimal.Zero;

		ZString IPackaging.TypeCode => bill.ABL_ManifestUQ;

		ZString IPackaging.MarksNumbers => bill.ABL_MarksAndNumbers;

		ZString IPackaging.PackagingMaterialDescription => bill.ABL_Remarks;

		ZString IPackaging.Combination => YesNoList.Codes.Yes;

		ZDate IPackaging.PackingDateTime => ZDate.Empty;
	}
}

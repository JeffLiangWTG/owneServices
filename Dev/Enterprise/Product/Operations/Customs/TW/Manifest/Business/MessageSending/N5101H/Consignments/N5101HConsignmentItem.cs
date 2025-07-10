using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.Customs.Universal.CodeDescriptionPairLists;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class N5101HConsignmentItem : IN5101HConsignmentItem
	{
		public N5101HConsignmentItem(AsycudaBill bill)
		{
			this.bill = bill;
		}

		readonly AsycudaBill bill;

		ZString IN5101HConsignmentItem.Split => TotalPackageQuantity.IsEmpty ? string.Empty : YesNoList.Codes.Yes;

		public ZDecimal TotalPackageQuantity => new ZDecimal(bill.ABL_SplitQuantity);

		IEnumerable<IAdditionalInformation> IN5101HConsignmentItem.AdditionalInformations => null;

		IN5101HCommodity IN5101HConsignmentItem.Commodity => new N5101HCommodity(bill);

		IN5101HGoodsMeasure IN5101HConsignmentItem.GoodsMeasure => new N5101HGoodsMeasure(bill);

		IPackaging IN5101HConsignmentItem.Packaging => new N5101HPackaging(bill);

		ZString IN5101HConsignmentItem.UCRId => bill.ABL_UCRNumber;
	}
}

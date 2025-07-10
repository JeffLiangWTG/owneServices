using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.TW.Business
{
	public static class ChargeCollectionHelper
	{
		public static IEnumerable<InvoiceCharge> AdditionCharges(this IJobComInvChargeCollection<InvoiceCharge> collection)
		{
			return collection.Where(charge => !charge.Parent.IsGroup && !charge.J7_IsIncludedInITOT && charge.J7_ChargeType != CustomsChargeTypeList.Codes.DeductionCharge && !charge.J7_Amount.IsEmpty);
		}

		public static IEnumerable<InvoiceCharge> DeductionCharges(this IJobComInvChargeCollection<InvoiceCharge> collection)
		{
			return collection.Where(charge => !charge.Parent.IsGroup && charge.J7_ChargeType == CustomsChargeTypeList.Codes.DeductionCharge && !charge.J7_Amount.IsEmpty);
		}
	}
}

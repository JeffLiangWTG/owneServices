using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public static class CusEntryLineFeeCollectionExtensions
	{
		public static ZDecimal CalculateTotalFeeAmount(this IEnumerable<CusEntryLineFee> fees, ZString feeType, bool includeLandedCostOnly)
		{
			Argument.NotNull(fees, nameof(fees));
			return fees.Where(x => x.CF_ChargeType == feeType && (includeLandedCostOnly || !x.CF_IsLandedCostOnly)).Sum(x => x.CF_ChargeAmount);
		}
	}
}

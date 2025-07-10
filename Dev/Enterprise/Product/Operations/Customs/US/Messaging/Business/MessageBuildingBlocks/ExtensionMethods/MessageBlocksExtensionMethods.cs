using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	public static class MessageBlocksExtensionMethods
	{
		public static ZDecimal GetAmount(this IEnumerable<AENS89> aens89s, ZString feeType)
		{
			ZDecimal result = ZDecimal.Zero;

			foreach (AENS89 aENS89 in aens89s)
			{
				if (aENS89.AccountingClassCode1 == feeType)
				{
					result = aENS89.TotalFeeAmount1;
					break;
				}
				else if (aENS89.AccountingClassCode2 == feeType)
				{
					result = aENS89.TotalFeeAmount2;
					break;
				}
				else if (aENS89.AccountingClassCode3 == feeType)
				{
					result = aENS89.TotalFeeAmount3;
					break;
				}
				else if (aENS89.AccountingClassCode4 == feeType)
				{
					result = aENS89.TotalFeeAmount4;
					break;
				}
				else if (aENS89.AccountingClassCode5 == feeType)
				{
					result = aENS89.TotalFeeAmount5;
					break;
				}
			}

			return result;
		}
	}
}

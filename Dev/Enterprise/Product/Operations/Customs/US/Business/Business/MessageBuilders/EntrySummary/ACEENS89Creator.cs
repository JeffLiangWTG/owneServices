using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	class ACEENS89Creator
	{
		public IEnumerable<MessageBlock> MakeENS89(IEnumerable<IFee> fees)
		{
			int rangeNumber = -1;   // Value not used
			AENS89 ens89 = null;

			foreach (IFee fee in fees)
			{
				if (CusFeeCodeConstants.IsLineLevel62Record(fee.Code) || CusFeeCodeConstants.IsHeaderLevelFee(fee.Code))
				{
					if (ens89 == null)
					{
						ens89 = new AENS89();
						rangeNumber = 1;
					}
					AddRangeDetails(ens89, rangeNumber, fee);
					rangeNumber++;
					if (rangeNumber == 6)
					{
						yield return ens89;
						ens89 = null;
					}
				}
			}

			if (ens89 != null)
			{
				yield return ens89;
			}
		}

		void AddRangeDetails(AENS89 ens89, int rangeNumber, IFee fee)
		{
			ZDecimal feeAmount = fee.Amount.Round(2);

			switch (rangeNumber)
			{
				case 1:
					{
						ens89.AccountingClassCode1 = fee.Code;
						ens89.TotalFeeAmount1 = feeAmount;
						break;
					}
				case 2:
					{
						ens89.AccountingClassCode2 = fee.Code;
						ens89.TotalFeeAmount2 = feeAmount;
						break;
					}
				case 3:
					{
						ens89.AccountingClassCode3 = fee.Code;
						ens89.TotalFeeAmount3 = feeAmount;
						break;
					}
				case 4:
					{
						ens89.AccountingClassCode4 = fee.Code;
						ens89.TotalFeeAmount4 = feeAmount;
						break;
					}
				case 5:
					{
						ens89.AccountingClassCode5 = fee.Code;
						ens89.TotalFeeAmount5 = feeAmount;
						break;
					}
				default:
					throw new ArgumentOutOfRangeException(nameof(rangeNumber), rangeNumber, "Invalid Value");
			}
		}
	}
}

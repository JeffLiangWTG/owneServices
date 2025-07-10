using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.Input;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	class ENS89Creator
	{
		public IEnumerable<MessageBlock> MakeENS89(IEnumerable<IFee> fees, bool buildEmpty89EvenIfNoFees)
		{
			int rangeNumber = -1;   // Value not used
			ENS89 ens89 = null;
			foreach (IFee fee in fees)
			{
				if (fee.Amount != 0)
				{
					if (ens89 == null)
					{
						ens89 = new ENS89();
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

			if (rangeNumber == -1 && buildEmpty89EvenIfNoFees)
			{
				rangeNumber++;
				ens89 = new ENS89();
				ens89.ClassCode = "";
				ens89.TotalAmount = 0m;
				yield return ens89;
			}
		}

		void AddRangeDetails(ENS89 ens89, int rangeNumber, IFee fee)
		{
			ZDecimal feeAmount = fee.Amount.Round(2);

			switch (rangeNumber)
			{
				case 1:
					{
						ens89.ClassCode = fee.Code;
						ens89.TotalAmount = feeAmount;
						break;
					}
				case 2:
					{
						ens89.ClassCode1 = fee.Code;
						ens89.TotalAmount1 = feeAmount;
						break;
					}
				case 3:
					{
						ens89.ClassCode2 = fee.Code;
						ens89.TotalAmount2 = feeAmount;
						break;
					}
				case 4:
					{
						ens89.ClassCode3 = fee.Code;
						ens89.TotalAmount3 = feeAmount;
						break;
					}
				case 5:
					{
						ens89.ClassCode4 = fee.Code;
						ens89.TotalAmount4 = feeAmount;
						break;
					}
				default:
					throw new ArgumentOutOfRangeException(nameof(rangeNumber), rangeNumber, "Invalid Value");
			}
		}
	}
}

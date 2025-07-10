using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Accounting.CriticalValidation
{
	public class NegativeJobChargeOSSellExRateWhenSaveRecorder : IService
	{
		public void AddOrUpdate(ZGuid jobChargePk, ZDecimal exchangeRate)
		{
			if (exchangeRate < ZDecimal.Zero)
			{
				negativeExRateJobChargePKs.Add(jobChargePk);
			}
			else
			{
				negativeExRateJobChargePKs.Remove(jobChargePk);
			}
		}

		public bool IsJobChargeExRateNegative(ZGuid jobChargePk)
		{
			if (negativeExRateJobChargePKs.Contains(jobChargePk))
			{
				return true;
			}

			return false;
		}

		public void Remove(ZGuid jobChargePk)
		{
			negativeExRateJobChargePKs.Remove(jobChargePk);
		}

		readonly HashSet<ZGuid> negativeExRateJobChargePKs = new HashSet<ZGuid>();
	}
}

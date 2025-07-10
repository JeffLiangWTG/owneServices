using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.ZA.Business
{
	static class DutyCalculationHelper
	{
		internal class CusTariffTypeOrderComparer : IComparer<TariffView>
		{
			readonly IList<ZString> orderKey;

			public CusTariffTypeOrderComparer(IList<ZString> orderKey)
			{
				this.orderKey = orderKey;
			}

			public int Compare(TariffView x, TariffView y)
			{
				int result = 0;

				if (x == null && y == null)
				{
					result = 0;
				}
				else if (x == null)
				{
					result = -1;
				}
				else if (y == null)
				{
					result = 1;
				}
				else
				{
					result = orderKey.IndexOf(x.Rates.FirstOrDefault()?.ZZ2_ZZR_RateTypeCode ?? ZString.Empty).CompareTo(orderKey.IndexOf(y.Rates.FirstOrDefault()?.ZZ2_ZZR_RateTypeCode ?? ZString.Empty));
					if (result == 0)
					{
						result = x.ZZ1_ZZI_TariffTypeCode.CompareTo(y.ZZ1_ZZI_TariffTypeCode);
					}
				}

				return result;
			}
		}

		internal class CusRateTypeOrderComparer : IComparer<CusRefRateCodeView>
		{
			readonly IList<ZString> orderKey;

			public CusRateTypeOrderComparer(IList<ZString> orderKey)
			{
				this.orderKey = orderKey;
			}

			public int Compare(CusRefRateCodeView x, CusRefRateCodeView y)
			{
				int result = 0;

				if (x == null && y == null)
				{
					result = 0;
				}
				else if (x == null)
				{
					result = -1;
				}
				else if (y == null)
				{
					result = 1;
				}
				else
				{
					result = orderKey.IndexOf(x?.RateType.ZZR_RateType ?? ZString.Empty).CompareTo(orderKey.IndexOf(y?.RateType.ZZR_RateType ?? ZString.Empty));
					if (result == 0)
					{
						result = x.ZY1_RateCode.CompareTo(y.ZY1_RateCode);
					}
				}

				return result;
			}
		}
	}
}

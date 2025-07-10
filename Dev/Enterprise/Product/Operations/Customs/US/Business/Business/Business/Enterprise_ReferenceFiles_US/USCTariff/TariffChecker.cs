using System;
using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public static class TariffChecker
	{
		public static bool HasSpecifiedCode(ZString specifiedCode, IEnumerable<ZString> codes)
		{
			bool result = false;
			if (!specifiedCode.IsEmpty)
			{
				specifiedCode = specifiedCode.PadRight(3);
				foreach (string oga in codes)
				{
					result = oga.PadRight(3).Equals(specifiedCode, StringComparison.OrdinalIgnoreCase);
					if (result)
					{
						break;
					}
				}
			}
			return result;
		}
	}
}

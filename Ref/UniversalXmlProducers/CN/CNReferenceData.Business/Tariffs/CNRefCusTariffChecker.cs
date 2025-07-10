using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.CNReferenceData.Business
{
	public class CNRefCusTariffChecker
	{
		readonly HashSet<string> singles;

		public CNRefCusTariffChecker()
		{
			singles = new HashSet<string>();
		}

		public bool CheckDuplicated(RefCusTariff cusTariff)
		{
			var result = true;

			var tariffCode = cusTariff.ZZ1_TariffCode;
			if (singles.Contains(tariffCode))
			{
				result = false;
				GlobalOption.Instance.Log.Error($"Tariff '{tariffCode}' is duplicated.");
			}
			else
			{
				singles.Add(tariffCode);
			}
			return result;
		}
	}
}

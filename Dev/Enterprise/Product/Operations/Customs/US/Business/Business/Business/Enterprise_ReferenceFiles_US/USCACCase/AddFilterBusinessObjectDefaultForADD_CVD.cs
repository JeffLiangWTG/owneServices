using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	static class AddFilterBusinessObjectDefaultForADD_CVD
	{
		public static IEnumerable<FilterBusinessObjectDefault> DefaultFilters(ZString countryOfOrigin, ZString caseNumber, ZString casePrifix, params ZString[] tariffNumbers)
		{
			if (caseNumber.IsEmpty)
			{
				yield return new FilterBusinessObjectDefault("Country Code", "Property", countryOfOrigin);

				yield return new FilterBusinessObjectDefault("Case Status", "Property0", ZBool.True);

				yield return new FilterBusinessObjectDefault("Case Number", "Property", casePrifix);

				List<ZString> tariffNumberList = new List<ZString>();

				foreach (ZString one in tariffNumbers)
				{
					if (!one.IsEmpty)
					{
						tariffNumberList.Add(one);
					}
				}

				if (tariffNumberList.Count >= 1)
				{
					yield return new FilterBusinessObjectDefault("Tariff Number", "Property", tariffNumberList[0]);
				}
			}
			else
			{
				yield return new FilterBusinessObjectDefault("Case Number", "Property", caseNumber);
			}
		}
	}
}

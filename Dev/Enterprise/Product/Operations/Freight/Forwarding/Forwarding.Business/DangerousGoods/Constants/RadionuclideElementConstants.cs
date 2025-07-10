using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Business.DangerousGoods
{
	public class RadionuclideElementList : CodeDescriptionPairList
	{
		public RadionuclideElementList()
		{
			var radionuclideElementKeyValuePairs = RadionuclideElementConstants.GetRadionuclideElementKeyValuePairs();
			foreach (var item in  radionuclideElementKeyValuePairs)
			{
				AddPair(item.Key, item.Value);
			}
		}
	}

	public class RadionuclideElementSuffixList : CodeDescriptionPairList
	{
		IReadOnlyDictionary<string, IEnumerable<string>> RadionuclideElementsToSuffixesDictionary
		{
			get
			{
				if (radionuclideElementsToSuffixDictionary == null)
				{
					radionuclideElementsToSuffixDictionary = RadionuclideElementSuffixConstants.RadionuclideElementsToSuffixesKeyValuePairs();
				}
				return radionuclideElementsToSuffixDictionary;
			}
		}
		IReadOnlyDictionary<string, IEnumerable<string>> radionuclideElementsToSuffixDictionary;

		public RadionuclideElementSuffixList(ZString radionuclideElement)
		{
			if (RadionuclideElementsToSuffixesDictionary.TryGetValue(radionuclideElement, out var radionuclideIsotopesList))
			{
				var radioNuclideElementList = new RadionuclideElementList();
				radionuclideIsotopesList.ForEach(isotope =>
				{
					var radionuclideDescription = radioNuclideElementList.GetDescriptionFromCode(radionuclideElement);
					if (!radionuclideDescription.IsNullOrEmpty())
					{
						var radionuclideIsotope = radionuclideDescription + " - " + isotope;
						AddPair(isotope, radionuclideIsotope);
					}
				});
			}
		}
	}
}

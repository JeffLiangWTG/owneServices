using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers
{
	internal class AdditionalInformationComparer : IComparer<AdditionalInformationDocWrapper>
	{
		public AdditionalInformationComparer(IEnumerable<ZZRefCusCodeListCombined> refAdditionalInfoCollection)
		{
			additionalInformations = refAdditionalInfoCollection;
		}

		readonly IEnumerable<ZZRefCusCodeListCombined> additionalInformations;
		public int Compare(AdditionalInformationDocWrapper x, AdditionalInformationDocWrapper y)
		{
			var codeX = x.Code;
			var codeY = y.Code;
			var groupingX = GetGroupingIDFromPairAttribute(codeX);
			var groupingY = GetGroupingIDFromPairAttribute(codeY);

			var result = 0;
			if (codeX == VDNCode)
			{
				result = -1;
			}
			else if (codeY == VDNCode)
			{
				result = 1;
			}
			else if (!groupingX.IsEmpty && groupingY.IsEmpty)
			{
				result = -1;
			}
			else if (groupingX.IsEmpty && !groupingY.IsEmpty)
			{
				result = 1;
			}
			else if (!groupingX.IsEmpty && !groupingY.IsEmpty)
			{
				result = StringComparer.Compare(groupingX, groupingY);
			}

			if (result == 0)
			{
				result = StringComparer.Compare(codeX, codeY);
			}

			return result;
		}

		ZString GetGroupingIDFromPairAttribute(ZString code)
		{
			var result = ZString.Empty;
			var pairCode = additionalInformations?.FirstOrDefault(addinfo => addinfo.ZZD_Code == code)?.GetAttribute(RefCusCodeListAttributeTypes.Codes.Pair) ?? ZString.Empty;
			if (!pairCode.IsEmpty)
			{
				result = string.Join("-", new ZString[] { code, pairCode }.OrderBy(x => x));
			}
			return result;
		}

		Comparer<ZString> StringComparer => stringComparer ?? (stringComparer = Comparer<ZString>.Default);
		Comparer<ZString> stringComparer;

		const string VDNCode = "VDN";
	}
}

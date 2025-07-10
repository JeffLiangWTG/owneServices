using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	static class HarmonisedCodeHelper
	{
		static ZString AddingTrailingZerosIfLessThanSixDigits(ZString hsCode)
		{
			return !string.IsNullOrEmpty(hsCode) ? hsCode.PadRight(6, '0') : hsCode;
		}

		public static StringCollectionX GetOuterPackLineHarmonisedCodes(CommonShipment shipment, ZString hsCodeCountry)
		{
			var result = new StringCollectionX();

			foreach (PackLine packLine in shipment.OuterPackLines.ToArray())
			{
				var hsCodes = packLine.HarmonisedCodes
					.Where(h => h.JLH_RN_NKCountry == hsCodeCountry)
					.Select(h => h.JLH_Code);
				if (hsCodes.Any())
				{
					foreach (var hsCode in hsCodes)
					{
						if (!string.IsNullOrEmpty(hsCode) && !result.Contains(hsCode))
						{
							result.Add(hsCode);
						}
					}
				}
				else
				{
					var hsCode = packLine.JL_HarmonisedCode;
					var populatedHsCode = packLine.HarmonisedCodeTariff != null
						? AddingTrailingZerosIfLessThanSixDigits(hsCode)
						: hsCode;

					if (!string.IsNullOrEmpty(hsCode) && !result.Contains(populatedHsCode))
					{
						result.Add(populatedHsCode);
					}
				}
			}

			return result;
		}
	}
}

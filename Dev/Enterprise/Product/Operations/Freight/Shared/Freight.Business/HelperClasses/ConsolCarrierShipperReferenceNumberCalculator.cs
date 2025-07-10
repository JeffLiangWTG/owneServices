using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Business
{
	public static class ConsolCarrierShipperReferenceNumberCalculator
	{
		public static ZString GetCarrierShipperReferenceNumber(CommonConsol consol)
		{
			var scrReferenceNumber = FindCSRAdditionalReferenceNumber(consol);
			return scrReferenceNumber?.CE_EntryNum ?? ZString.Empty;
		}

		public static ZBool IsCRSNumberOverLimit(CommonConsol consol)
		{
			if (consol == null)
			{
				return false;
			}

			var csrReferenceNumber = FindCSRAdditionalReferenceNumber(consol);

			if (csrReferenceNumber != null)
			{
				var csrPattern = GetCSRPattern(consol.JK_UniqueConsignRef);
				var version = GetVersionFromCarrierShipperReferenceNumber(csrReferenceNumber.CE_EntryNum, csrPattern);

				return version >= MaxCarrierShipperReferenceNumber;
			}

			return false;
		}

		public static void PopulateShipperReferenceNumber(CommonConsol consol)
		{
			var csrReferenceNumber = FindCSRAdditionalReferenceNumber(consol) ?? CreateCSRAdditionalReferenceNumber(consol);
			var csrPattern = GetCSRPattern(consol.JK_UniqueConsignRef);
			var version = GetVersionFromCarrierShipperReferenceNumber(csrReferenceNumber.CE_EntryNum, csrPattern);

			csrReferenceNumber.CE_EntryNum = ZString.Format("{0}-V{1}", consol.JK_UniqueConsignRef, ++version);
		}

		public static int GetVersionFromCarrierShipperReferenceNumber(string csrNumber)
		{
			return GetVersionFromCarrierShipperReferenceNumber(csrNumber, GetCSRPattern());
		}

		public static string GetConsolIDFromCarrierShipperReferenceNumber(string csrNumber)
		{
			var rgx = new Regex(GetCSRPattern());
			var numberMatches = rgx.Matches(csrNumber);

			if (numberMatches.Count == 1)
			{
				return numberMatches[0].Groups[1].Value;
			}

			return string.Empty;
		}

		public static bool IsValidCarrierShipperReferenceNumber(string csrNumber)
		{
			return Regex.IsMatch(csrNumber, GetCSRPattern());
		}

		static int GetVersionFromCarrierShipperReferenceNumber(string csrNumber, string pattern)
		{
			var rgx = new Regex(pattern);
			var numberMatches = rgx.Matches(csrNumber);

			if (numberMatches.Count == 1 && numberMatches[0].Groups.Count > 2 && ZInt.TryParse(numberMatches[0].Groups[2].Value, out var number))
			{
				return number;
			}

			return 0;
		}

		static CusEntryNumber FindCSRAdditionalReferenceNumber(CommonConsol consol)
		{
			return consol.Numbers.Find(num => num.CE_EntryType == ConsolNonCustomsAdditionalReferenceCodesCodeList.Codes.CarrierShipperReference
									&& num.CE_EntryIsSystemGenerated).FirstOrDefault();
		}

		static CusEntryNumber CreateCSRAdditionalReferenceNumber(CommonConsol consol)
		{
			var result = consol.Numbers.AddNew();
			result.CE_EntryType = ConsolNonCustomsAdditionalReferenceCodesCodeList.Codes.CarrierShipperReference;
			result.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			result.CE_EntryIsSystemGenerated = true;

			result.SetReadOnlyIncludingChildren(true);

			return result;
		}

		static ZString GetCSRPattern(string consolID = "")
		{
			return string.IsNullOrEmpty(consolID) ? (ZString)(NoResString)@"^([a-zA-Z0-9]+)-V(\d+)$" : ZString.Format((NoResString)@"^({0})-V(\d+)$", consolID);
		}

		const int MaxCarrierShipperReferenceNumber = 9;
	}
}

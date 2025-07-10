using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.BRReferenceData.Business
{
	public static class RefCusNomenclatureGroupUtils
	{
		public static Dictionary<string, int[]> GetSessions()
		{
			if (sessions == null)
			{
				sessions = new Dictionary<string, int[]>();
				sessions.Add("01", new int[] { 1, 5 });
				sessions.Add("02", new int[] { 6, 14 });
				sessions.Add("03", new int[] { 15, 15 });
				sessions.Add("04", new int[] { 16, 24 });
				sessions.Add("05", new int[] { 25, 27 });
				sessions.Add("06", new int[] { 28, 38 });
				sessions.Add("07", new int[] { 39, 40 });
				sessions.Add("08", new int[] { 41, 43 });
				sessions.Add("09", new int[] { 44, 46 });
				sessions.Add("10", new int[] { 47, 49 });
				sessions.Add("11", new int[] { 50, 63 });
				sessions.Add("12", new int[] { 64, 67 });
				sessions.Add("13", new int[] { 68, 70 });
				sessions.Add("14", new int[] { 71, 71 });
				sessions.Add("15", new int[] { 72, 83 });
				sessions.Add("16", new int[] { 84, 85 });
				sessions.Add("17", new int[] { 86, 89 });
				sessions.Add("18", new int[] { 90, 92 });
				sessions.Add("19", new int[] { 93, 93 });
				sessions.Add("20", new int[] { 94, 96 });
				sessions.Add("21", new int[] { 97, 97 });
			}

			return sessions;
		}

		public static string GetNcmSession(string ncmCode)
		{
			int firstTwoCodes = int.Parse(ncmCode.Substring(0, 2), CultureInfo.CurrentCulture);

			foreach (string session in GetSessions().Keys)
			{
				if (GetSessions().TryGetValue(session, out var range))
				{
					if (firstTwoCodes >= range[0] && firstTwoCodes <= range[1])
					{
						return session;
					}
				}
			}

			return null;
		}

		public static void BuildZZ1_CompositeKeyOnZZ5(RefCusNomenclatureGroup refCusNomenclatureGroup)
		{
			var tariffCode = refCusNomenclatureGroup?.ZZ5_Value;
			Contract.Assume(!string.IsNullOrEmpty(tariffCode));

			string compositeKey = GetNcmSession(tariffCode);
			int length = tariffCode.Length;
			switch (length)
			{
				case 2:
					compositeKey += $".{tariffCode.Substring(0, 2)}";
					break;
				case 4:
					compositeKey += $".{tariffCode.Substring(0, 2)}..{tariffCode.Substring(2, 2)}";
					break;
				case 5:
					compositeKey += $".{tariffCode.Substring(0, 2)}..{tariffCode.Substring(2, 2)}.{tariffCode.Substring(4, 1)}";
					break;
				case 6:
					compositeKey += $".{tariffCode.Substring(0, 2)}..{tariffCode.Substring(2, 2)}.{tariffCode.Substring(4, 1)}.{tariffCode.Substring(5, 1)}";
					break;
				case 7:
					compositeKey += $".{tariffCode.Substring(0, 2)}..{tariffCode.Substring(2, 2)}.{tariffCode.Substring(4, 1)}.{tariffCode.Substring(5, 1)}.{tariffCode.Substring(6, 1)}";
					break;
				default:
					break;
			}

			refCusNomenclatureGroup.ZZ5_CompositeKey = compositeKey;
		}

		static Dictionary<string, int[]> sessions;
	}
}

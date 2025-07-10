using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Messaging
{
	public static class TRManifestMessageBuilderHelper
	{
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard Coded")]
		public static ZBool IsTagNull(ZString manifestTypeCode, ZString fieldName, ZBool isValueEmpty)
		{
			ZBool isTagNull = ZBool.False;
			ZString[] manifestList;

			switch (fieldName)
			{
				case "EsyaninBulunduguYer":
					manifestList = new ZString[] { (NoResString)"ATAİTH", (NoResString)"DENİTH", "CIKONC", "VARONC" };
					isTagNull = isValueEmpty && Array.IndexOf(manifestList, manifestTypeCode) >= 0;
					break;

				case "EkBelgeSayisi":
					manifestList = new ZString[] { "EMANIF", "GRUPAJ", "TESLİM" };
					isTagNull = isValueEmpty && Array.IndexOf(manifestList, manifestTypeCode) >= 0;
					break;

				case "Kurye":
					manifestList = new ZString[] { "CIKONC", "DENİTH", "EMANIF" };
					isTagNull = isValueEmpty && Array.IndexOf(manifestList, manifestTypeCode) >= 0;
					break;

				case "OdemeSekli":
					manifestList = new ZString[] { "DENİHR", "DEMİHR", "GRUPAJ", "VARONC" };
					isTagNull = Array.IndexOf(manifestList, manifestTypeCode) >= 0;
					break;

				case "AliciVergiNo":
					manifestList = new ZString[] { "GRUPAJ", "TESLİM", "VARONC" };
					isTagNull = isValueEmpty && Array.IndexOf(manifestList, manifestTypeCode) >= 0;
					break;

				case "AcentaVergiNo":
					manifestList = new ZString[] { "ATAİTH", "DEMİTH", "DEMİHR" };
					isTagNull = isValueEmpty && Array.IndexOf(manifestList, manifestTypeCode) >= 0;
					break;

				case "FaturaDoviz":
				case "OzetBeyanNo":
					manifestList = new ZString[] { "EMANIF", "GRUPAJ" };
					isTagNull = isValueEmpty && Array.IndexOf(manifestList, manifestTypeCode) >= 0;
					break;

				case "Diger":
					manifestList = new ZString[] { "CIKONC", "VARONC" };
					isTagNull = isValueEmpty || Array.IndexOf(manifestList, manifestTypeCode) >= 0;
					break;

				case "AktarmaTipi":
				case "AktarmaYapilacakMi":
					manifestList = new ZString[] { "GRUPAJ", "TESLİM" };
					isTagNull = Array.IndexOf(manifestList, manifestTypeCode) >= 0;
					break;

				case "RoroMu":
				case "VarisCikisGumrukIdaresi":
					manifestList = new ZString[] { "ATAİTH", "EMANIF" };
					isTagNull = isValueEmpty && Array.IndexOf(manifestList, manifestTypeCode) >= 0;
					break;

				case "GondericiVergiNo":
					manifestList = new ZString[] { "CIKONC", "EMANIF" };
					isTagNull = isValueEmpty && Array.IndexOf(manifestList, manifestTypeCode) == -1;
					break;

				case "NavlunDoviz":
					manifestList = new ZString[] { "CIKONC", "HAVİTH" };
					isTagNull = isValueEmpty && Array.IndexOf(manifestList, manifestTypeCode) == -1;
					break;

				case "KonteynerTipi":
				case "MuhurNumarasi":
					isTagNull = !(manifestTypeCode == "ATAİTH" || manifestTypeCode == "DEMİHR") && Convert.ToBoolean(isValueEmpty);
					break;

				case "AcentaAdi":
					isTagNull = !(manifestTypeCode == "CIKONC" || manifestTypeCode == "EMANIF") && Convert.ToBoolean(isValueEmpty);
					break;

				case "BildirimTarafiAdi":
				case "BildirimTarafiVergiNo":
					isTagNull = manifestTypeCode != "CIKONC" && Convert.ToBoolean(isValueEmpty);
					break;

				case "AmbarHaricimi":
					isTagNull = manifestTypeCode != "CIKONC" && Convert.ToBoolean(isValueEmpty);
					break;

				case "OncekiBeyanNo":
					manifestList = new ZString[] { "ATAİTH" };
					isTagNull = Array.IndexOf(manifestList, manifestTypeCode) >= 0;
					break;

				case "HareketTarihSaati":
					isTagNull = isValueEmpty && manifestTypeCode == "CIKONC";
					break;

				default:
					break;
			}
			return isTagNull;
		}

		[SuppressMessage("Microsoft.Reliability", "CA2000:NonExceptionEdge")] // false positive
		public static string MD5Hash(string input)
		{
			var hash = new StringBuilder();
			var md5provider = MD5.Create();
			var bytes = md5provider.ComputeHash(new UTF8Encoding().GetBytes(input));

			for (var i = 0; i < bytes.Length; i++)
			{
				hash.Append(bytes[i].ToString("x2", CultureInfo.CurrentCulture));
			}
			return hash.ToString();
		}
	}
}

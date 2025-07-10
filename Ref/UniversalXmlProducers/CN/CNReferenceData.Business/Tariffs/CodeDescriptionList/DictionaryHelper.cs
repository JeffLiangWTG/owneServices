using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CargoWise.RefDbRepo.CNReferenceData.Services;
using Microsoft.Data.SqlClient;

namespace CargoWise.RefDbRepo.CNReferenceData.Business
{
	#region DictionaryHelper

	public abstract class DictionaryHelper<T>
	{
		protected DictionaryHelper()
		{
			dictionary = new Dictionary<string, T>();
		}

		internal Dictionary<string, T> dictionary;

		protected abstract T CreateValue(SqlDataReader reader);
		public virtual T GetValue(string code) => dictionary[code];
		public T GetValueOrDefault(string code) => dictionary.ContainsKey(code) ? dictionary[code] : default;
		public string GetCode(T value) => dictionary.First(x => x.Value.Equals(value)).Key;

		public IEnumerable<KeyValuePair<string, T>> All => dictionary.OrderBy(x => x.Key);
		public IEnumerable<string> Keys => dictionary.Keys.OrderBy(x => x);
	}

	#endregion

	#region TaxOrFeeHelper

	public class TaxOrFeeHelper : DictionaryHelper<int>
	{
		public static TaxOrFeeHelper Instance { get; } = new TaxOrFeeHelper();

		TaxOrFeeHelper() : base()
		{
			dictionary.Add("VDG", 3);
			dictionary.Add("VAT", 13);
			dictionary.Add("VRD", 9);
			dictionary.Add("VZR", 0);
		}

		protected override int CreateValue(SqlDataReader reader) => reader.GetInt32(1);
	}

	#endregion

	#region TradeGroupHelper

	public class TradeGroupHelper : DictionaryHelper<string>
	{
		public static TradeGroupHelper Instance { get; } = new TradeGroupHelper();

		TradeGroupHelper() : base()
		{
			dictionary = TradeGroups.ToDictionary(x => x.TradeGroup, x => x.Description);
		}

		protected override string CreateValue(SqlDataReader reader) => reader.GetString(1);

		public IEnumerable<KeyValuePair<string, string>> GetFTATradeGroups() => dictionary.Where(x => !x.Key.StartsWith("LDC", System.StringComparison.Ordinal)).OrderBy(x => x.Key);

		public IEnumerable<KeyValuePair<string, string>> GetLDCTradeGroups() => dictionary.Where(x => x.Key.StartsWith("LDC", System.StringComparison.Ordinal)).OrderBy(x => x.Key);

		public string GetAdditionalCode(string tradeGroup) => tradeGroup != null && AdditionalCodeMapping.TryGetValue(tradeGroup, out var additionalCode) ? additionalCode : string.Empty;

		Dictionary<string, string> AdditionalCodeMapping => additionalCodeMapping ?? (additionalCodeMapping = TradeGroups.ToDictionary(x => x.TradeGroup, x => x.AdditionalCode));
		Dictionary<string, string> additionalCodeMapping;

		public string[] GetExcludeTradeGroups(string tradeGroup) => tradeGroup != null && ExcludeTradeGroups.TryGetValue(tradeGroup, out var excludeTradeGroups) ? excludeTradeGroups : null;

		Dictionary<string, string[]> ExcludeTradeGroups => excludeTradeGroups ?? (excludeTradeGroups = TradeGroups.ToDictionary(x => x.TradeGroup, x => x.ExcludeTradeGroups));
		Dictionary<string, string[]> excludeTradeGroups;

		readonly IEnumerable<(string AdditionalCode, string TradeGroup, string Description, string[] ExcludeTradeGroups)> TradeGroups = new[]
		{
			("01", "APEC", "亚太贸易协定", null),
			("02", "ASEAN", "中国-东盟自贸协定", null),
			("03", "HK", "香港CEPA", null),
			("04", "MO", "澳门CEPA", null),
			("07", "PK", "中国-巴基斯坦自贸协定", null),
			("08", "CL", "中国-智利自贸协定", null),
			("10", "NZ", "中国-新西兰自贸协定", null),
			("11", "SG", "中国-新加坡自贸协定", null),
			("12", "PE", "中国-秘鲁自贸协定", null),
			("14", "TW", "台澎/ECFA", null),
			("15", "CR", "中国-哥斯达黎加自贸协定", null),
			("16", "IS", "中国-冰岛自贸协定", null),
			("17", "CH", "中国-瑞士自贸协定", null),
			("18", "AU", "中国-澳大利亚自贸协定", null),
			("19", "KR", "中国-韩国自贸协定", null),
			("20", "GE", "中国-格鲁吉亚自贸协定", null),
			("21", "MU", "中国-毛里求斯自贸协定", null),
			("22", "RCEPASEAN", "区域全面经济伙伴关系协定-东盟", null),
			("22", "RCEPAU", "区域全面经济伙伴关系协定-澳大利亚", null),
			("22", "RCEPJP", "区域全面经济伙伴关系协定-日本", null),
			("22", "RCEPNZ", "区域全面经济伙伴关系协定-新西兰", null),
			("22", "RCEPKR", "区域全面经济伙伴关系协定-韩国", null),
			("23", "KH", "中国-柬埔寨自贸协定", null),
			("24", "NI", "中国-尼加拉瓜自贸协定", null),
			("25", "EC", "中国-厄瓜多尔自贸协定", null),
			("26", "RS", "中国-塞尔维亚自贸协定", null),
			("27", "HN", "中国-洪都拉斯自贸协定", null),
			("28", "MV", "中国-马尔代夫自贸协定", null),
			(string.Empty, "LDCAPEC", "亚太2国", new[] { "LDCLA" }),
			(string.Empty, "LDCLA", "东盟-老挝", null),
			(string.Empty, "LDCKH", "东盟-柬埔寨", null),
			(string.Empty, "LDCMM", "东盟-缅甸", null),
			(string.Empty, "LDC", "最不发达国家", new[] { "LDCLA", "LDCKH", "LDCMM", "LDCAPEC" }),
			(string.Empty, "LDC1", "最不发达国家(LDC1)", new[] { "LDCKH" }), // Obsoleted
			(string.Empty, "LDC2", "最不发达国家(LDC2)", null), // Obsoleted
			(string.Empty, "LDC3", "最不发达国家(LDC3)", null), // Obsoleted
		};
	}

	#endregion

	#region AdditionalElementHelper

	public class AdditionalElementHelper : DictionaryHelper<string>
	{
		public AdditionalElementHelper()
		{
			AlternativeHashCodes = new Dictionary<string, string>();
			CodeDescriptionDict = new Dictionary<string, string>();

			var assembly = typeof(AdditionalElementHelper).Assembly;
			var resourceName = assembly.GetManifestResourceNames().FirstOrDefault(x => x.EndsWith("ADIELHashCodes.csv", System.StringComparison.Ordinal));

			using (var resourceStream = assembly.GetManifestResourceStream(resourceName))
			using (var sr = new StreamReader(resourceStream))
			{
				sr.ReadLine();

				string line;
				while ((line = sr.ReadLine()) != null)
				{
					var strs = line.Split(',');
					AlternativeHashCodes.Add(strs[0], strs[1]);
				}
			}
		}

		public Dictionary<string, string> AlternativeHashCodes { get; set; }
		public Dictionary<string, string> CodeDescriptionDict { get; set; }

		protected override string CreateValue(SqlDataReader reader) => reader.GetString(1);

		public string GetAdditionElementCode(string description)
		{
			description = description.TrimEnd('；', ';');
			string code = GetValueOrDefault(description);
			string hashCode = null;

			if (string.IsNullOrEmpty(code))
			{
				switch (description)
				{
					case "品名":
						code = "00000";
						break;
					case "品牌类型":
						code = "00422";
						break;
					case "出口享惠情况":
						code = "00069";
						break;
					case "GTIN":
						code = "00009";
						break;
					case "CAS":
						code = "00005";
						break;
					case "包装规格":
						code = "99997";
						break;
					case "规格型号":
						code = "99998";
						break;
					case "其他":
						code = "99999";
						break;
					default:
						{
							hashCode = GetHash(description);
							code = AlternativeHashCodes.ContainsKey(hashCode) ? AlternativeHashCodes[hashCode] : hashCode;
							break;
						}
				}
			}

			if (!dictionary.ContainsKey(description))
			{
				dictionary.Add(description, code);
			}

			if (!CodeDescriptionDict.ContainsKey(code))
			{
				CodeDescriptionDict.Add(code, description);
			}
			else if (AlternativeHashCodes.ContainsValue(code) && code == hashCode)
			{
				CodeDescriptionDict[code] = description;
			}

			return dictionary[description];
		}

		public static string GetHash(string input)
		{
			const string salt = "73F85B92-3427-4FB3-9CDC-EC2480A0B7CA";
#pragma warning disable CA5351 // Do Not Use Broken Cryptographic Algorithms
			using (var md5 = MD5.Create())
			{
				var data = md5.ComputeHash(Encoding.UTF8.GetBytes(salt + input));

				var sBuilder = new StringBuilder();

				foreach (var t in data)
				{
					sBuilder.Append(t.ToString("x2", CultureInfo.InvariantCulture));
				}
				return sBuilder.ToString();
			}
#pragma warning restore CA5351 // Do Not Use Broken Cryptographic Algorithms
		}

		public void Clear()
		{
			dictionary.Clear();
		}

		public static IEnumerable<(int index, string description)> Parse(string tariffCode, string additionalInfoString, ILog logger)
		{
			if (!string.IsNullOrEmpty(additionalInfoString))
			{
				var additionalInfo = additionalInfoString.Replace("(以下要素仅上海海关要求)", "").Replace("1:品名", "").Replace('（', '(').Replace('）', ')').Replace('：', ':').Trim();
				var startWithZero = additionalInfo.StartsWith("0:", System.StringComparison.Ordinal);

				if (!string.IsNullOrEmpty(additionalInfo) && additionalInfo.Contains(':'))
				{
					for (int i = 1; i < 20; i++)
					{
						var nextSeq = (startWithZero ? i : (i + 1)).ToString(CultureInfo.InvariantCulture);
						var indexOfNextSeq = additionalInfo.IndexOf(nextSeq + ':', System.StringComparison.Ordinal);

						var segment = indexOfNextSeq > -1 ? additionalInfo.Substring(0, indexOfNextSeq) : additionalInfo;
						var indexAndDescription = segment.Split(':');
						var index = int.Parse(indexAndDescription.First().Trim(), CultureInfo.InvariantCulture);
						var description = indexAndDescription.Last().TrimEnd(';', '；').Trim();
						if (index != (startWithZero ? i - 1 : i) || string.IsNullOrEmpty(description) || description.IndexOf(':') > -1 || description == "品名")
						{
							logger?.Error($"Additional Info Parse error: {tariffCode} Additional Info: {additionalInfo}/{segment} => {i}");
							break;
						}
						yield return (i + 1, description);

						if (indexOfNextSeq > -1)
						{
							additionalInfo = additionalInfo.Remove(0, indexOfNextSeq);
						}
						else
						{
							break;
						}
					}
				}
			}
		}
	}

	#endregion

	#region UnitOfMeasurementHelper

	public class UnitOfMeasurementHelper : DictionaryHelper<string>
	{
		public static UnitOfMeasurementHelper Instance { get; } = new UnitOfMeasurementHelper();

		UnitOfMeasurementHelper() : base()
		{
			dictionary.Add("台", "001");
			dictionary.Add("座", "002");
			dictionary.Add("辆", "003");
			dictionary.Add("艘", "004");
			dictionary.Add("架", "005");
			dictionary.Add("套", "006");
			dictionary.Add("个", "007");
			dictionary.Add("只", "008");
			dictionary.Add("头", "009");
			dictionary.Add("张", "010");
			dictionary.Add("件", "011");
			dictionary.Add("支", "012");
			dictionary.Add("枝", "013");
			dictionary.Add("根", "014");
			dictionary.Add("条", "015");
			dictionary.Add("把", "016");
			dictionary.Add("块", "017");
			dictionary.Add("卷", "018");
			dictionary.Add("副", "019");
			dictionary.Add("片", "020");
			dictionary.Add("组", "021");
			dictionary.Add("份", "022");
			dictionary.Add("幅", "023");
			dictionary.Add("双", "025");
			dictionary.Add("对", "026");
			dictionary.Add("棵", "027");
			dictionary.Add("株", "028");
			dictionary.Add("井", "029");
			dictionary.Add("米", "030");
			dictionary.Add("盘", "031");
			dictionary.Add("平方米", "032");
			dictionary.Add("立方米", "033");
			dictionary.Add("筒", "034");
			dictionary.Add("千克", "035");
			dictionary.Add("克", "036");
			dictionary.Add("盆", "037");
			dictionary.Add("万个", "038");
			dictionary.Add("具", "039");
			dictionary.Add("百副", "040");
			dictionary.Add("百支", "041");
			dictionary.Add("百把", "042");
			dictionary.Add("百个", "043");
			dictionary.Add("百片", "044");
			dictionary.Add("刀", "045");
			dictionary.Add("疋", "046");
			dictionary.Add("公担", "047");
			dictionary.Add("扇", "048");
			dictionary.Add("百枝", "049");
			dictionary.Add("千只", "050");
			dictionary.Add("千块", "051");
			dictionary.Add("千盒", "052");
			dictionary.Add("千枝", "053");
			dictionary.Add("千个", "054");
			dictionary.Add("亿支", "055");
			dictionary.Add("亿个", "056");
			dictionary.Add("万套", "057");
			dictionary.Add("千张", "058");
			dictionary.Add("万张", "059");
			dictionary.Add("千伏安", "060");
			dictionary.Add("千瓦", "061");
			dictionary.Add("千瓦时", "062");
			dictionary.Add("千升", "063");
			dictionary.Add("英尺", "067");
			dictionary.Add("吨", "070");
			dictionary.Add("长吨", "071");
			dictionary.Add("短吨", "072");
			dictionary.Add("司马担", "073");
			dictionary.Add("司马斤", "074");
			dictionary.Add("斤", "075");
			dictionary.Add("磅", "076");
			dictionary.Add("担", "077");
			dictionary.Add("英担", "078");
			dictionary.Add("短担", "079");
			dictionary.Add("两", "080");
			dictionary.Add("市担", "081");
			dictionary.Add("盎司", "083");
			dictionary.Add("克拉", "084");
			dictionary.Add("市尺", "085");
			dictionary.Add("码", "086");
			dictionary.Add("英寸", "088");
			dictionary.Add("寸", "089");
			dictionary.Add("升", "095");
			dictionary.Add("毫升", "096");
			dictionary.Add("英加仑", "097");
			dictionary.Add("美加仑", "098");
			dictionary.Add("立方英尺", "099");
			dictionary.Add("立方尺", "101");
			dictionary.Add("平方码", "110");
			dictionary.Add("平方英尺", "111");
			dictionary.Add("平方尺", "112");
			dictionary.Add("英制马力", "115");
			dictionary.Add("公制马力", "116");
			dictionary.Add("令", "118");
			dictionary.Add("箱", "120");
			dictionary.Add("批", "121");
			dictionary.Add("罐", "122");
			dictionary.Add("桶", "123");
			dictionary.Add("扎", "124");
			dictionary.Add("包", "125");
			dictionary.Add("箩", "126");
			dictionary.Add("打", "127");
			dictionary.Add("筐", "128");
			dictionary.Add("罗", "129");
			dictionary.Add("匹", "130");
			dictionary.Add("册", "131");
			dictionary.Add("本", "132");
			dictionary.Add("发", "133");
			dictionary.Add("枚", "134");
			dictionary.Add("捆", "135");
			dictionary.Add("袋", "136");
			dictionary.Add("粒", "139");
			dictionary.Add("盒", "140");
			dictionary.Add("合", "141");
			dictionary.Add("瓶", "142");
			dictionary.Add("千支", "143");
			dictionary.Add("万双", "144");
			dictionary.Add("万粒", "145");
			dictionary.Add("千粒", "146");
			dictionary.Add("千米", "147");
			dictionary.Add("千英尺", "148");
			dictionary.Add("百万贝可", "149");
			dictionary.Add("部", "163");
			dictionary.Add("亿株", "164");
		}

		protected override string CreateValue(SqlDataReader reader) => reader.GetString(1);
	}

	#endregion
}

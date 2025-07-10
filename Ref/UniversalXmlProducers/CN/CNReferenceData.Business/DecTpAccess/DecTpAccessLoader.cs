using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using FlexCel.XlsAdapter;

namespace CargoWise.RefDbRepo.CNReferenceData.Business
{
	public class DecTpAccessLoader
	{
		public DecTpAccessLoader(LoadSetting setting = null)
		{
			this.setting = setting ?? new LoadSetting();
		}
		readonly LoadSetting setting;
		public LoadSetting Setting => setting;

		public IEnumerable<DesignatedSupervisionSite> GetDesignatedSupervisionSites(Stream inputStream)
		{
			var rawData = LoadDesignatedSupervisionSites(inputStream);
			var processedData = ProcessDesignatedSupervisionSites(rawData);
			return processedData;
		}

		static IEnumerable<DesignatedSupervisionSite> ProcessDesignatedSupervisionSites(IEnumerable<DesignatedSupervisionSite> rawDesignatedSupervisionSites)
		{
			var result = Enumerable.Empty<DesignatedSupervisionSite>();

			if (rawDesignatedSupervisionSites.Any())
			{
				var differentOfficesOfSameCode = rawDesignatedSupervisionSites.GroupBy(x => new { x.CustomsCode, x.CustomsDistrict }).Select(x => x.Key)
				.GroupBy(x => x.CustomsCode).Where(x => x.Count() > 1);
				if (differentOfficesOfSameCode.Any())
				{
					throw new InvalidDataException($"Invalid data exists: same customs code {differentOfficesOfSameCode.Select(x => x.Key)} with different customs offices.");
				}

				result = rawDesignatedSupervisionSites.GroupBy(x => new { x.CustomsCode, x.Type }, (key, elements) =>
				{
					var first = elements.First();
					return new DesignatedSupervisionSite
					{
						CustomsCode = key.CustomsCode.Trim(),
						Type = key.Type.Trim(),
						CustomsDistrict = first.CustomsDistrict.Trim(),
						Name = elements.Select(x => x.Name).Aggregate((accumulator, next) => accumulator + "/" + next),
					};
				});
			}

			return result;
		}

		IEnumerable<DesignatedSupervisionSite> LoadDesignatedSupervisionSites(Stream inputStream)
		{
			var result = new List<DesignatedSupervisionSite>();

			XlsFile xls = new XlsFile(inputStream, false)
			{
				ActiveSheet = 1
			};

			var startRow = Setting.StartRow;
			for (int row = startRow; row <= xls.RowCount; row++)
			{
				var name = xls.GetStringFromCell(row, Setting.ColumnIndexForName).Trim();

				if (!string.IsNullOrEmpty(name))
				{
					string districtName = xls.GetStringFromCell(row, Setting.ColumnIndexForCustomsDistrict).Trim();
					if (districtName.EndsWith(CustomsOfficeSuffix, System.StringComparison.Ordinal))
					{
						districtName = districtName.Substring(0, districtName.Length - CustomsOfficeSuffix.Length);
					}
					var districtCode = GetCodeByDistrictName(districtName);

					string type = Setting.FixedType;
					if (Setting.ColumnIndexForType > 0)
					{
						type = GetCodeByDssCodeTypeDescription(xls.GetStringFromCell(row, Setting.ColumnIndexForType).Trim().ToString(CultureInfo.InvariantCulture));
					}

					var record = new DesignatedSupervisionSite
					{
						CustomsCode = xls.GetStringFromCell(row, Setting.ColumnIndexForCustomsCode).Trim(),
						Type = type,
						CustomsDistrict = districtCode,
						Name = name
					};
					result.Add(record);
				}
			}

			return result;
		}

		const string CustomsOfficeSuffix = "海关";

		public IEnumerable<DssCodeType> DssCodeTypes
		{
			get
			{
				if (codeTypes == null)
				{
					codeTypes = new DssCodeType[]
					{
						new DssCodeType { Code = "DSSMT", EnglishDescription = "Designated Sites under Supervision for Imported Meat", ChineseDescription = "进境肉类指定监管场地" },
						new DssCodeType { Code = "DSSSF", EnglishDescription = "Designated Sites under Supervision for Imported Frozen and Fresh Seafood Products", ChineseDescription = "进境冰鲜水产品指定监管场地" },
						new DssCodeType { Code = "DSSGN", EnglishDescription = "Designated Sites under Supervision for Imported Grain", ChineseDescription = "进境粮食指定监管场地" },
						new DssCodeType { Code = "DSSFT", EnglishDescription = "Designated Sites under Supervision for Imported Fruit", ChineseDescription = "进境水果指定监管场地" },
						new DssCodeType { Code = "DSSAA", EnglishDescription = "Designated Sites under Supervision for Imported Edible Aquatic Animals", ChineseDescription = "进境食用水生动物指定监管场地" },
						new DssCodeType { Code = "DSSPS", EnglishDescription = "Designated Sites under Supervision for Imported Plant Seedlings", ChineseDescription = "进境植物种苗指定监管场地" },
						new DssCodeType { Code = "DSSLG", EnglishDescription = "Designated Sites under Supervision for Imported Logs", ChineseDescription = "进境原木指定监管场地" },
						new DssCodeType { Code = "ISQAN", EnglishDescription = "Isolation Sites for Quarantine of Imported Animals", ChineseDescription = "进境动物隔离检疫场" },
					};
				}
				return codeTypes;
			}
		}
		IEnumerable<DssCodeType> codeTypes;

		string GetCodeByDssCodeTypeDescription(string description) => DssCodeTypes.First(x => x.ChineseDescription == description).Code;

		public IEnumerable<DssAttributeName> DssAttributeNames
		{
			get
			{
				if (attributeNames == null)
				{
					attributeNames = new DssAttributeName[]
					{
						new DssAttributeName { Name = "CodeSuffix", EnglishDescription = "Code Suffix of the Designated Site Code", ChineseDescription = "监管场地代码后缀", Caption = "Code Suffix", ChineseCaption = "代码后缀" },
						new DssAttributeName { Name = "CustomsOffice", EnglishDescription = "Customs Office Directly under GACC of the Designated Site", ChineseDescription = "监管场地所属直属海关", Caption = "Customs Office", ChineseCaption = "直属海关" },
					};
				}
				return attributeNames;
			}
		}
		IEnumerable<DssAttributeName> attributeNames;

		IDictionary<string, string> DistrictCodes
		{
			get
			{
				if (districtCodes == null)
				{
					districtCodes = new Dictionary<string, string>
					{
						{ "北京", "01" },
						{ "天津", "02" },
						{ "石家庄", "04" },
						{ "太原", "05" },
						{ "满洲里", "06" },
						{ "呼和浩特", "07" },
						{ "沈阳", "08" },
						{ "大连", "09" },
						{ "长春", "15" },
						{ "哈尔滨", "19" },
						{ "上海", "22" },
						{ "南京", "23" },
						{ "杭州", "29" },
						{ "宁波", "31" },
						{ "合肥", "33" },
						{ "福州", "35" },
						{ "厦门", "37" },
						{ "南昌", "40" },
						{ "青岛", "42" },
						{ "济南", "43" },
						{ "郑州", "46" },
						{ "武汉", "47" },
						{ "长沙", "49" },
						{ "广州", "51" },
						{ "黄埔", "52" },
						{ "深圳", "53" },
						{ "拱北", "57" },
						{ "汕头", "60" },
						{ "海口", "64" },
						{ "湛江", "67" },
						{ "江门", "68" },
						{ "南宁", "72" },
						{ "成都", "79" },
						{ "重庆", "80" },
						{ "贵阳", "83" },
						{ "昆明", "86" },
						{ "拉萨", "88" },
						{ "西安", "90" },
						{ "乌鲁木齐", "94" },
						{ "兰州", "95" },
						{ "银川", "96" },
						{ "西宁", "97" },
					};
				}

				return districtCodes;
			}
		}
		IDictionary<string, string> districtCodes;

		string GetCodeByDistrictName(string name) => DistrictCodes[name];

		public class LoadSetting
		{
			public int StartRow { get; set; } = 2;

			public int ColumnIndexForCustomsCode { get; set; } = 2;
			public int ColumnIndexForType { get; set; } = 3;
			public int ColumnIndexForCustomsDistrict { get; set; } = 4;
			public int ColumnIndexForName { get; set; } = 5;

			public string FixedType { get; set; }
		}
	}
}

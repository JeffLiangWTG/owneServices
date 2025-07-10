using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader
{
	public class PopulateCurrenciesTransformation : DataTransformation, IDataTransformationTask
	{
		public PopulateCurrenciesTransformation(int version) : base(version)
		{ }

		public void Run(IDbTransaction trans)
		{
			var sql = @"SELECT COUNT(*)
FROM NamedEntityClassification
WHERE NEC_Class = 'CURRENCY'
AND NEC_Language = 'EN'";
			var count = DbHelper.ExecuteScalar(trans, sql);
			if ((int)count == 0)
			{
				var binFolder = FolderHelper.GetBinFolder();
				var allText = File.ReadAllText(Path.Combine(binFolder, @"DataFiles\Currencies.xml"));
				var xml = XElement.Parse(allText);
				var dataSql = new StringBuilder(@"INSERT NamedEntityClassification(NEC_PK, NEC_Name, NEC_Class, NEC_Language, NEC_Code)
VALUES");

				dataSql.Append(@"
(newid(), 'South Korean Won', 'CURRENCY', 'EN', 'KRW'),
(newid(), 'Vietnamese Dong', 'CURRENCY', 'EN', 'VND'),
(newid(), 'RMB', 'CURRENCY', 'EN', 'CNY'),
(newid(), 'Renminbi', 'CURRENCY', 'EN', 'CNY'),
(newid(), 'Chinese Renminbi', 'CURRENCY', 'EN', 'CNY'),
(newid(), 'VND', 'CURRENCY', 'EN', 'VND'),
(newid(), 'Rupee', 'CURRENCY', 'EN', 'INR'),
(newid(), 'Euros', 'CURRENCY', 'EN', 'EUR'),
(newid(), 'TL', 'CURRENCY', 'EN', 'TRY'),
");

				var currencies = xml.Descendants().FirstOrDefault()?.Descendants(XName.Get("CcyNtry"));
				var codesAlreadyIn = new List<string>();
				foreach (var currency in currencies)
				{
					var code = currency.Descendants(XName.Get("Ccy")).FirstOrDefault()?.Value;
					if (code == null || codesAlreadyIn.Contains(code))
					{
						continue;
					}
					var name = currency.Descendants(XName.Get("CcyNm")).FirstOrDefault()?.Value;
					dataSql.Append(CultureInfo.InvariantCulture, $@"
(newid(), '{name}', 'CURRENCY', 'EN', '{code}'),");
					codesAlreadyIn.Add(code);
				}
				var insertSql = dataSql.ToString().TrimEnd(',');
				DbHelper.ExecuteNonQuery(trans, insertSql);
			}
		}
	}
}

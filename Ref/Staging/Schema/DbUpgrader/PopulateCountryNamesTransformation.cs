using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader
{
	public class PopulateCountryNamesTransformation : DataTransformation, IDataTransformationTask
	{
		public PopulateCountryNamesTransformation(int version) : base(version)
		{ }

		public void Run(IDbTransaction trans)
		{
			var sql = @"SELECT COUNT(*)
FROM NamedEntityClassification
WHERE NEC_Class = 'COUNTRY'";
			var count = DbHelper.ExecuteScalar(trans, sql);
			if ((int)count == 0)
			{
				var binFolder = FolderHelper.GetBinFolder();
				var allText = File.ReadAllText(Path.Combine(binFolder, @"DataFiles\Country_names.xml"));
				var xml = XElement.Parse(allText);
				var dataSql = new StringBuilder(@"INSERT NamedEntityClassification(NEC_PK, NEC_Name, NEC_Class, NEC_Language, NEC_Code)
VALUES");
				dataSql.Append(@"
(newid(), 'Chinese Taipei', 'COUNTRY', 'EN', 'TW'),
(newid(), 'the Federative Republic of Brazil', 'COUNTRY', 'EN', 'BR'),
(newid(), 'the Italian Republic', 'COUNTRY', 'EN', 'IT'),
(newid(), 'the Republic of Korea', 'COUNTRY', 'EN', 'KR'),
(newid(), 'the Republic of Indonesia', 'COUNTRY', 'EN', 'ID'),
(newid(), 'Korea', 'COUNTRY', 'EN', 'KR'),
");
				foreach (var countryCode in xml.Descendants(XName.Get("country")))
				{
					var code = countryCode.Attribute(XName.Get("code"))?.Value?.ToUpper(CultureInfo.InvariantCulture);
					foreach (var countryName in countryCode.Descendants(XName.Get("name")))
					{
						var lang = countryName.Attribute(XName.Get("lang"))?.Value?.ToUpper(CultureInfo.InvariantCulture);
						if (lang != null && lang.StartsWith("EN", StringComparison.InvariantCulture))
						{
							dataSql.Append(CultureInfo.InvariantCulture, $@"
(newid(), '{countryName.Value}', 'COUNTRY', '{lang}', '{code}'),");
						}
					}
				}
				var insertSql = dataSql.ToString().TrimEnd(',');
				DbHelper.ExecuteNonQuery(trans, insertSql);
			}
		}
	}
}

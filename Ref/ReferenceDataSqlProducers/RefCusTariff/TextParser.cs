using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace CargoWise.RefDbRepo.ReferenceDataSqlProducers
{
	public static class TextParser
	{
		public static string GenerateInsertSQL(string fileName)
		{
			var stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(InitializeSQL);

			using (var streamReader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(fileName)))
			{
				string line;
				while ((line = streamReader.ReadLine()) != null)
				{
					var tariffCode = line.Substring(0, 10);
					var description = line.Substring(15, 155).TrimEnd();
					var tariffUOM1 = line.Substring(170, 8).TrimEnd();
					var tariffUOM2 = line.Substring(178).TrimEnd();
					if (!string.IsNullOrWhiteSpace(tariffCode) && !string.IsNullOrWhiteSpace(description))
					{
						var tariffPK = Guid.NewGuid();
						stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, InsertRefCusTariffSql, tariffPK, tariffCode, description.Replace("'", "''")));
						if (!string.IsNullOrWhiteSpace(tariffUOM1))
						{
							stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, InsertRefCusTariffUOMSql, tariffPK, "CU1", tariffUOM1));
							if (!string.IsNullOrWhiteSpace(tariffUOM2))
							{
								stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, InsertRefCusTariffUOMSql, tariffPK, "CU2", tariffUOM2));

							}
						}
					}
				}
			}

			return stringBuilder.ToString();
		}

		const string InitializeSQL = @"INSERT INTO RefCusTariffType (ZZI_TariffType, ZZI_Description, ZZI_ZZZ_NKDataGrouping)
SELECT ZZI_TariffType, ZZI_Description, ZZI_ZZZ_NKDataGrouping
FROM
(
	SELECT 'EXP' AS ZZI_TariffType, 'AES Import Concordance (HTS)' AS ZZI_Description, 'US' AS ZZI_ZZZ_NKDataGrouping
) AS Data
WHERE NOT EXISTS (SELECT NULL FROM RefCusTariffType WHERE Data.ZZI_TariffType = ZZI_TariffType AND Data.ZZI_ZZZ_NKDataGrouping = ZZI_ZZZ_NKDataGrouping)

DECLARE @tariffTypePK UNIQUEIDENTIFIER = 
(
	SELECT ZZI_PK from RefCusTariffType WHERE ZZI_TariffType='EXP' AND ZZI_ZZZ_NKDataGrouping='US'
)";

		const string InsertRefCusTariffSql = @"INSERT INTO RefCusTariff (ZZ1_PK, ZZ1_ZZI_TariffType, ZZ1_TariffCode, ZZ1_Description, ZZ1_StartDate, ZZ1_ZZZ_NKDataGrouping)
VALUES ('{0}', @tariffTypePK, '{1}', '{2}', '2018-01-01', 'US')";

		const string InsertRefCusTariffUOMSql = @"INSERT INTO RefCusTariffUOM (ZZ8_PK, ZZ8_ZZ1_Tariff, ZZ8_Type, ZZ8_UOM, ZZ8_ZZZ_NKDataGrouping) 
VALUES (NEWID(), '{0}', '{1}', '{2}', 'US')";

	}
}

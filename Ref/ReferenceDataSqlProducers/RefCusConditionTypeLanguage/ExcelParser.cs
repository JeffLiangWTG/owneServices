using System.Globalization;
using System.Reflection;
using System.Text;
using FlexCel.XlsAdapter;

namespace CargoWise.RefDbRepo.ReferenceDataSqlProducers
{
	class ExcelParser
	{
		public static string GenerateInsertSQL(string fileName)
		{
			var stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(InitializeSQL);

			var startRowIndex = 2;

			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fileName))
			{
				var xlsFile = new XlsFile(stream, false);
				var sheetIndex = xlsFile.GetSheetIndex("Measure types");
				xlsFile.SetSheetSelected(sheetIndex, true);

				var rowCount = xlsFile.GetRowCount(xlsFile.ActiveSheet);
				var currentMeasureType = "";

				for (var rowId = startRowIndex; rowId <= rowCount; rowId++)
				{
					var measureType = xlsFile.GetCellValue(rowId, 1)?.ToString().Trim(' ', '\n');
					var language = xlsFile.GetCellValue(rowId, 2)?.ToString().Trim(' ', '\n');
					var description = xlsFile.GetCellValue(rowId, 3)?.ToString().Trim(' ', '\n','\t').Replace("'", "''").Replace("\n", " ");

					if (currentMeasureType != measureType)
					{
						if (!string.IsNullOrEmpty(currentMeasureType))
						{
							stringBuilder.AppendLine(CloseMeasureSQL);
						}
						currentMeasureType = measureType;

						stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, OpenMeasureSQL, currentMeasureType));

					}

					stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, InsertMeasureSQL, language, description));
				}

				if (!string.IsNullOrEmpty(currentMeasureType))
				{
					stringBuilder.AppendLine(CloseMeasureSQL);
				}

			}

			return stringBuilder.ToString();
		}
		const string InitializeSQL = "DECLARE @conditionTypeId varchar(50)";

		const string OpenMeasureSQL = @"
SET @conditionTypeId = null;
SELECT @conditionTypeId = ZX2_PK
 FROM [RefCusConditionType]
 WHERE ZX2_ZZZ_NKDataGrouping = 'EUN'
	AND ZX2_ConditionType = {0}

IF @conditionTypeId IS NOT NULL
BEGIN";

		const string CloseMeasureSQL = "END";

		const string InsertMeasureSQL = @"	IF NOT EXISTS(SELECT NULL FROM RefCusConditionTypeLanguage WHERE ZXW_ZX2_ConditionType = @conditionTypeId AND ZXW_ZX6_NKLanguage = '{0}')
		INSERT INTO RefCusConditionTypeLanguage(ZXW_PK, ZXW_ZX2_ConditionType, ZXW_ZX6_NKLanguage, ZXW_Description) VALUES (NEWID(), @conditionTypeId, '{0}', N'{1}');";
	}
}

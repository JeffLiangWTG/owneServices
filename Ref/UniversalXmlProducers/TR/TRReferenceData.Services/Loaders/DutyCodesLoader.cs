using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using FlexCel.XlsAdapter;

namespace CargoWise.RefDbRepo.TRReferenceData.Services.Loaders
{
	public static class DutyCodesLoader
	{
		public static IEnumerable<RefCusRateType> LoadData(string path)
		{
			var records = new List<RefCusRateType>();
			XlsFile xls = new XlsFile(path);
			xls.ActiveSheet = 1;

			var columnIndex = GetColumnIndexes(xls);

			for (int row = 2; row <= xls.RowCount; row++)
			{
				var rateCode = xls.GetStringFromCell(row, columnIndex["ZY1_RateCode"]).Trim();
				var rateType = xls.GetStringFromCell(row, columnIndex["ZY1_ZZR_RateType"]).Trim();
				var descriptionEN = xls.GetStringFromCell(row, columnIndex["ZY1_Description"]).Trim();
				var descriptionTR = xls.GetStringFromCell(row, columnIndex["ZY1_Description_TR"]).Trim();

				var existingType = records.Find(r => r.ZZR_RateType == rateType);
				if (existingType != null)
				{
					var updatedRateCodes = new List<RefCusRateCode>(existingType.RefCusRateCodes)
					{
						new RefCusRateCode
						{
							ZY1_RateCode = rateCode,
							ZY1_Description = descriptionEN,
							RefCusRateCodeLanguages = new[]
							{
								new RefCusRateCodeLanguage { ZXC_Description = descriptionTR }
							}
						}
					};
					existingType.RefCusRateCodes = updatedRateCodes.ToArray();
				}
				else
				{
					records.Add(new RefCusRateType
					{
						ZZR_RateType = rateType,
						RefCusRateCodes = new[]
						{
							new RefCusRateCode
							{
								ZY1_RateCode = rateCode,
								ZY1_Description = descriptionEN,
								RefCusRateCodeLanguages = new[]
								{
									new RefCusRateCodeLanguage { ZXC_Description = descriptionTR }
								}
							}
						}
					});
				}
			}

			return records;
		}

		static Dictionary<string, int> GetColumnIndexes(XlsFile xls)
		{
			var columnIndexes = new Dictionary<string, int>();
			for (int col = 1; col <= xls.ColCount; col++)
			{
				var columnName = xls.GetStringFromCell(1, col).Trim();
				if (!string.IsNullOrEmpty(columnName))
				{
					columnIndexes[columnName] = col;
				}
			}
			return columnIndexes;
		}
	}
}

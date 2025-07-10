using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using FlexCel.XlsAdapter;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public abstract class ExcelDrivenCodeListDataFileGenerator : CodeListDataFileGenerator
	{
		public sealed override IEnumerable<string> InputFiles => InputFileNames;

		public sealed override string OutputFile => $"{DataSource}.xml";

		protected sealed override List<RefCusCodeList> GetDataCollection(DateTime publicationDate, ref Errors error)
		{
			var result = new List<RefCusCodeList>();

			if (InputFiles.Any())
			{
				using (var stream = GetInputFileStream())
				{
					var workbook = new XlsFile(stream, false);
					AppendCodes(result, workbook, SheetName);
				}
			}

			return UniversalDataHelper.FilterOutputListByDate(result).OrderBy(x => x.ZZD_ZZK_NKCodeType).ThenBy(x => x.ZZD_Code).ToList();
		}

		public virtual Stream GetInputFileStream() => Assembly.GetExecutingAssembly().GetManifestResourceStream("CargoWise.RefDbRepo.FRReferenceData.Services.Resources." + InputFiles.First());

		static void AppendCodes(List<RefCusCodeList> result, XlsFile workbook, string codeListId)
		{
			workbook.ActiveSheetByName = codeListId;

			for (int row = 2; row <= workbook.RowCount; row++)
			{
				var code = workbook.GetStringFromCell(row, 1);

				if (!string.IsNullOrEmpty(code))
				{
					var description = workbook.GetStringFromCell(row, 2);
					description = description.Replace("Â", string.Empty).Replace("â‚¬", "Euros").Replace("â", string.Empty);

					result.Add(new RefCusCodeList()
					{
						ZZD_Code = code,
						ZZD_Description = description,
						ZZD_StartDate = UniversalDataHelper.MinimumDateTime,
						ZZD_EndDate = UniversalDataHelper.MaximumDateTime
					});
				}
			}
		}

		protected abstract string SheetName { get; }
	}
}

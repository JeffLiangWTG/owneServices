using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using FlexCel.XlsAdapter;

namespace CargoWise.RefDbRepo.BRReferenceData.Business
{
	public class SpecialClearanceAttributesProcedureParser : BaseParser
	{
		public SpecialClearanceAttributesProcedureParser(string dataSource) : base(dataSource)
		{
		}

		public void ExportToXMLFile(Stream stream, string outputFileName, DateTime publicationTime)
		{
			var refCusProcedureLists = GetRefCusProcedureList(stream);
			var writer = Helper.GetRefCusProcedureAttributesWriterConfiguration();
			Helper.ExportToXMLFile(outputFileName, dataSource, publicationTime, writer, refCusProcedureLists);
		}

		static IEnumerable<RefCusProcedure> GetRefCusProcedureList(Stream stream)
		{
			Contract.Assume(stream != null);

			var xls = new XlsFile(stream, true);

			Contract.Assume(xls != null);

			xls.ActiveSheet = 1;
			var codeStartRow = xls.Find("CÓDIGO", xls.GetAutoFilterRange(), new FlexCel.Core.TCellAddress("A1"), true, true, false, true);
			var specialSituationColumn = xls.Find("SITUAÇÃO ESPECIAL", xls.GetAutoFilterRange(), codeStartRow, false, true, false, true).Col;
			var rowCount = xls.GetRowCount(xls.ActiveSheet);

			var result = new HashSet<RefCusProcedure>();
			for (var rowId = codeStartRow.Row + 1; rowId <= rowCount; rowId++)
			{
				var code = xls.GetCellValue(rowId, 1)?.ToString() ?? string.Empty;
				if (!string.IsNullOrEmpty(code) && code.All(char.IsDigit))
				{
					var specialSituation = xls.GetCellValue(rowId, specialSituationColumn)?.ToString() ?? string.Empty;
					if (!string.IsNullOrEmpty(specialSituation))
					{
						result.Add(new RefCusProcedure()
						{
							ZZ6_ProcedureCode = code,
							ZZ6_Description = code,
							RefCusProcedureAttributes = GetRefCusProcedureAttributeList(specialSituation).ToArray(),
						});
					}
				}
			}
			return result;
		}

		static IEnumerable<RefCusProcedureAttribute> GetRefCusProcedureAttributeList(string specialSituation)
		{
			var result = new HashSet<RefCusProcedureAttribute>();
			var situationList = specialSituation.Split(splitStrings, StringSplitOptions.TrimEntries);
			foreach (var situation in situationList)
			{
				result.Add(new RefCusProcedureAttribute() { ZXB_Value = GetSpecialSituationCode(situation) });
			}
			return result;
		}

		static string GetSpecialSituationCode(string specialSituation)
		{
			switch (specialSituation.ToUpperInvariant())
			{
				case "POSTERIORI":
					return Constants.SpecialSituation.Code._2001;
				case "EMBARQUE ANTECIPADO":
					return Constants.SpecialSituation.Code._2002;
				case "SEM SAÍDA (FICTA)":
					return Constants.SpecialSituation.Code._2003;
				default:
					return string.Empty;
			}
		}

		static readonly string[] splitStrings = { ",", "OU" };
	}
}

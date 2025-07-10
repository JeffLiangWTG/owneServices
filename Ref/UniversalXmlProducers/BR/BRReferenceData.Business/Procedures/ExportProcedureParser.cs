using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.BRReferenceData.Business
{
	public class ExportProcedureParser : BaseParser
	{
		public ExportProcedureParser(string dataSource) : base(dataSource)
		{
		}

		public void ExportToXMLFile(TextReader reader, string outputFileName, DateTime publicationTime)
		{
			var refCusProcedureLists = GetRefProcedureLists(reader);
			var writer = Helper.GetRefCusProcedureWriterConfiguration(Constants.ShipmentTypes.Export);
			Helper.ExportToXMLFile(outputFileName, dataSource, publicationTime, writer, refCusProcedureLists);
		}

		public static List<RefCusProcedure> GetRefProcedureLists(TextReader reader)
		{
			var result = new List<RefCusProcedure>();

			string line;
			while ((line = reader.ReadLine()) != null)
			{
				string[] data = line.Split(',');

				if (data.Length < 2)
				{
					ParserErrorCollector.Instance.AppendLine($"Please check the format of the csv file: {line}");
				}
				else
				{
					if (data.Length > 16)
					{
						int firstQuotationMark = line.IndexOf('"', 0);
						int lastQuotationMark = line.LastIndexOf('"');

						if (firstQuotationMark > -1 && lastQuotationMark > -1 && firstQuotationMark != lastQuotationMark)
						{
							string description = line.Substring(firstQuotationMark + 1, lastQuotationMark - firstQuotationMark);
							data = line.Substring(0, firstQuotationMark).TrimEnd(',').Split(',')
								.Append(description)
								.Concat(line.Substring(lastQuotationMark + 1).Split(',').Skip(1)).ToArray();
						}
					}

					int.TryParse(data[0], NumberStyles.Number, CultureInfo.InvariantCulture, out var isNumber);
					if (isNumber > 0)
					{
						var categoryValue = data[12];
						var procedureCodeValue = data[0];
						var descriptionValue = data[1].Trim('"').Replace("\"\"", "\"");
						var groupValue = procedureCodeValue + " - " + descriptionValue;
						var startDateValue = data[14];

						if (DateTime.TryParse(startDateValue, out var startDateTimeValue))
						{
							var refCusCodeList = new RefCusProcedure()
							{
								ZZ6_Category = categoryValue,
								ZZ6_ProcedureCode = procedureCodeValue,
								ZZ6_Description = descriptionValue,
								ZZ6_Group = groupValue,
								ZZ6_StartDate = startDateTimeValue,
							};
							result.Add(refCusCodeList);
						}
					}
				}
			}

			return result;
		}
	}
}

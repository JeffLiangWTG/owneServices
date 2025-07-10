using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.ESReferenceData.Business
{
	public class ExportCPCParser : CPCParser<ExportCPCItemMap, ExportCPCConcessionItemMap>
	{
		public ExportCPCParser(IDateTimeProvider dateTimeProvider) : base(dateTimeProvider)
		{
		}

		protected override XmlWriterConfiguration XMLWriterConfiguration => CodeListHelper.GetRefCusProcedureWriterConfiguration("EXP");

		protected override string XMLWriterDataSource => Constants.DataSources.Export_CPC_Codes;

		protected override List<RefCusProcedure> PopulateRefCusProcedures(List<IItem> records, List<IConcessionItem> concessionRecords)
		{
			var result = new List<RefCusProcedure>();
			foreach (var record in records.Cast<ExportCPCItem>())
			{
				var description = record.Description;
				var groupTypes = record.GroupTypes.Replace(' ', ',');
				var startDate = Helper.GetDateTime(record.StartDate, CusProcedureDateFormat);
				var endDate = Helper.GetDateTime(record.EndDate, CusProcedureDateFormat);

				if (CheckDataIsValid(record.Code, description, startDate.SuccessfullyParsed, record.StartDate, endDate.SuccessfullyParsed, record.EndDate, false))
				{
					createCPC(result, record, description, startDate.DateTime, endDate.DateTime, groupTypes, string.Empty);
					var concessions = new List<string>();
					concessions.AddRange(SplitConcessions(record.Concessions1));
					concessions.AddRange(SplitConcessions(record.Concessions2));

					if (concessions.Count > 0)
					{
						foreach (var concession in concessionRecords.Cast<ExportCPCConcessionItem>().Where(r => concessions.Any(c => r.Code.StartsWith(c, StringComparison.InvariantCulture))))
						{
							startDate = Helper.GetDateTime(concession.StartDate, CusProcedureDateFormat);
							endDate = Helper.GetDateTime(concession.EndDate, CusProcedureDateFormat);

							if (CheckDataIsValid(concession.Code, concession.Description, startDate.SuccessfullyParsed, concession.StartDate, endDate.SuccessfullyParsed, concession.EndDate, true))
							{
								description = string.Join("-", record.Description, concession.Description);
								var combinedGroupTypes = record.GroupTypes.Split(' ').ToList();
								combinedGroupTypes.AddRange(concession.GroupTypes.Split(' '));
								createCPC(result, record, description, startDate.DateTime, endDate.DateTime, string.Join(",", combinedGroupTypes.Distinct()), concession.Code);
							}
						}
					}
				}
			}
			return result;
		}

		void createCPC(List<RefCusProcedure> result, ExportCPCItem record, string description, DateTime startDate, DateTime endDate, string groupTypes, string concession)
		{
			var code = record.Code.PadLeft(4, '0');
			var outOfWarehouseCode = code.Substring(2, 2);
			var outOfWarehouse = Constants.No;

			if (WarehouseValidCodes.Contains(outOfWarehouseCode))
			{
				outOfWarehouse = Constants.Yes;
			}
			AddToRefList(result, code, concession, description, groupTypes, false, false, Constants.No, false, outOfWarehouse, startDate, endDate);
		}

		bool CheckDataIsValid(string code, string description, bool startDateSuccessfullyParsed, string startDateString, bool endDateSuccessfullyParsed, string endDateString, bool isConcession)
		{
			var result = true;
			if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(description) || !startDateSuccessfullyParsed || !endDateSuccessfullyParsed)
			{
				var concessionPrefix = isConcession ? "Concession " : string.Empty;
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Unable to import {concessionPrefix}CPC as there is a missing or wrong attribute 'code', 'description', 'start date' or 'end date'. Details:");
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Code: {code}");
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Description: {description}");
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Start Date: {startDateString}");
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"End Date: {endDateString}");
				result = false;
			}
			return result;
		}
	}
}

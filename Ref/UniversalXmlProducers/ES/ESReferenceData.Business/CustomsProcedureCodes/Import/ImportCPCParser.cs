using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.ESReferenceData.Business
{
	public class ImportCPCParser : CPCParser<ImportCPCItemMap, ImportCPCConcessionItemMap>
	{
		public ImportCPCParser(IDateTimeProvider dateTimeProvider) : base(dateTimeProvider)
		{
		}

		protected override XmlWriterConfiguration XMLWriterConfiguration => CodeListHelper.GetRefCusProcedureWriterConfiguration("IMP");

		protected override string XMLWriterDataSource => Constants.DataSources.Import_CPC_Codes;

		protected override List<RefCusProcedure> PopulateRefCusProcedures(List<IItem> records, List<IConcessionItem> concessionRecords)
		{
			var result = new List<RefCusProcedure>();
			foreach (var record in records.Cast<ImportCPCItem>())
			{
				var description = record.Description;
				var groupTypes = record.GroupTypes.Replace(' ', ',');
				var startDate = Helper.GetDateTime(record.StartDate, CusProcedureDateFormat);
				var endDate = Helper.GetDateTime(record.EndDate, CusProcedureDateFormat);

				if (CheckDataIsValid(record.Code, description, startDate.SuccessfullyParsed, record.StartDate, endDate.SuccessfullyParsed, record.EndDate, record.Duty, record.VAT, false))
				{
					var recordDuty = record.Duty.Substring(0, 2);
					var recordVAT = record.VAT.Substring(0, 2);

					var calculateDuty = recordDuty.Contains('S');
					var landedCost = recordDuty.Equals("NS", StringComparison.Ordinal);
					var calculateVAT = recordVAT.Contains('S');

					createCPC(result, record, description, startDate.DateTime, endDate.DateTime, groupTypes, string.Empty, calculateDuty, landedCost, calculateVAT);

					var concessions = new List<string>();
					concessions.AddRange(SplitConcessions(record.Concessions1));
					concessions.AddRange(SplitConcessions(record.Concessions2));
					concessions.AddRange(SplitConcessions(record.Concessions3));
					concessions.AddRange(SplitConcessions(record.Concessions4));
					concessions.AddRange(SplitConcessions(record.Concessions5));

					if (concessions.Count > 0)
					{
						foreach (var concession in concessionRecords.Cast<ImportCPCConcessionItem>().Where(r => concessions.Any(c => r.Code.StartsWith(c, StringComparison.InvariantCulture))))
						{
							startDate = Helper.GetDateTime(concession.StartDate, CusProcedureDateFormat);
							endDate = Helper.GetDateTime(concession.EndDate, CusProcedureDateFormat);

							if (CheckDataIsValid(concession.Code, concession.Description, startDate.SuccessfullyParsed, concession.StartDate, endDate.SuccessfullyParsed, concession.EndDate, concession.Duty, concession.VAT, true))
							{
								description = string.Join("-", record.Description, concession.Description);
								var combinedGroupTypes = record.GroupTypes.Split(' ').ToList();
								combinedGroupTypes.AddRange(concession.GroupTypes.Split(' '));

								var recordConcessionDuty = concession.Duty.Substring(0, 2);
								var recordConcessionVAT = concession.VAT.Substring(0, 2);

								bool RecordConcessionDutyEqualsTo(string codeToCompare) => recordConcessionDuty.Equals(codeToCompare, StringComparison.Ordinal);

								var calculateDutyConcession = recordConcessionDuty.Contains('S') || (!RecordConcessionDutyEqualsTo("NN") && calculateDuty);
								var landedCostConcession = RecordConcessionDutyEqualsTo("NS") || (!RecordConcessionDutyEqualsTo("NN") && landedCost);
								var calculateVATConcession = recordConcessionVAT.Contains('S') || (!recordConcessionVAT.Equals("NN", StringComparison.Ordinal) && calculateVAT);

								createCPC(result, record, description, startDate.DateTime, endDate.DateTime, string.Join(",", combinedGroupTypes.Distinct()), concession.Code, calculateDutyConcession, landedCostConcession, calculateVATConcession);
							}
						}
					}
				}
			}
			return result;
		}

		void createCPC(List<RefCusProcedure> result, ImportCPCItem record, string description, DateTime startDate, DateTime endDate, string groupTypes, string concession, bool calculateDuty, bool landedCost, bool calculateVAT)
		{
			var code = record.Code.PadLeft(4, '0');

			var intoWarehouseCode = code.Substring(0, 2);
			var intoWarehouse = Constants.No;
			if (WarehouseValidCodes.Contains(intoWarehouseCode))
			{
				intoWarehouse = Constants.Yes;
			}

			var outOfWarehouseCode = code.Substring(2, 2);
			var outOfWarehouse = Constants.No;
			if (WarehouseValidCodes.Contains(outOfWarehouseCode))
			{
				outOfWarehouse = Constants.Yes;
			}

			AddToRefList(result, code, concession, description, groupTypes, calculateDuty, landedCost, intoWarehouse, calculateVAT, outOfWarehouse, startDate, endDate);
		}

		bool CheckDataIsValid(string code, string description, bool startDateSuccessfullyParsed, string startDateString, bool endDateSuccessfullyParsed, string endDateString, string duty, string vat, bool isConcession)
		{
			var result = true;
			if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(description) || !startDateSuccessfullyParsed || !endDateSuccessfullyParsed || IsInvalidCode(duty) || IsInvalidCode(vat))
			{
				var concessionPrefix = isConcession ? "Concession " : string.Empty;
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Unable to import {concessionPrefix}CPC as there is a missing or wrong attribute 'code', 'description', 'start date', 'end date', 'duty' or 'vat'. Details:");
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Code: {code}");
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Description: {description}");
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Start Date: {startDateString}");
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"End Date: {endDateString}");
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Duty: {duty}");
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"VAT: {vat}");
				result = false;
			}
			return result;
		}

		static bool IsInvalidCode(string code) => string.IsNullOrEmpty(code) || code.Length < 2;
	}
}

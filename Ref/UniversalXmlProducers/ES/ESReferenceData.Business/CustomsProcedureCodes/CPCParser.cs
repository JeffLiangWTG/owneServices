using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CsvHelper;
using CsvHelper.Configuration;

namespace CargoWise.RefDbRepo.ESReferenceData.Business
{
	public abstract class CPCParser<TItemMap, TConcessionItemMap> : CommonParser
		where TItemMap : ClassMap
		where TConcessionItemMap : ClassMap
	{
		protected CPCParser(IDateTimeProvider dateTimeProvider) : base(dateTimeProvider) { }

		public string ConvertRecordsToXMLFile<TItem, TConcessionItem>(string inputCsvFile, string inputSupportingCsvFile, string outPutFileWithPath)
			where TItem : IItem
			where TConcessionItem : IConcessionItem
		{
			ErrorBuilder.Clear();
			var records = GetRecordsFromCsv<TItem, TItemMap>(inputCsvFile);
			if (records.Count > 0)
			{
				var concessionRecords = GetRecordsFromCsv<TConcessionItem, TConcessionItemMap>(inputSupportingCsvFile);

				if (concessionRecords.Count > 0)
				{
					var result = PopulateRefCusProcedures(records.Cast<IItem>().ToList(), concessionRecords.Cast<IConcessionItem>().ToList());

					Helper.ExportToXMLFile(XMLWriterDataSource, outPutFileWithPath, XMLWriterConfiguration, DateTimeProvider.CurrentLocalDate, result);
				}
				else
				{
					ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Unable to locate any CPC Concession records for CPC CSV. File may only contain header record. Input file details: {inputSupportingCsvFile}");
				}
			}
			else
			{
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Unable to locate any CPC Item records for CPC CSV. File may only contain header record. Input file details: {inputCsvFile}");
			}
			return ErrorBuilder.ToString();
		}

		protected void AddToRefList(List<RefCusProcedure> result, string code, string concession, string description, string groupTypes, bool calculateDuty, bool landedCost, string intoWarehouse, bool calculateVAT, string outOfWarehouse, DateTime startDate, DateTime endDate)
		{
			result.Add(new RefCusProcedure
			{
				ZZ6_ProcedureCode = code.Substring(0, 2),
				ZZ6_PreviousProcedureCode = code.Substring(2, 2),
				ZZ6_Concession = concession,
				ZZ6_Description = description,
				ZZ6_Group = groupTypes,
				ZZ6_CalculateDuty = calculateDuty,
				ZZ6_LandedCost = landedCost,
				ZZ6_IntoWarehouse = intoWarehouse,
				ZZ6_OutOfWarehouse = outOfWarehouse,
				ZZ6_CalculateVAT = calculateVAT,
				ZZ6_StartDate = startDate,
				ZZ6_EndDate = endDate
			});
		}

		protected abstract string XMLWriterDataSource { get; }
		protected abstract List<RefCusProcedure> PopulateRefCusProcedures(List<IItem> records, List<IConcessionItem> concessionRecords);

		protected IEnumerable<string> SplitConcessions(string concession)
		{
			if (!string.IsNullOrEmpty(concession))
			{
				return concession.Replace("*", string.Empty).Split(' ');
			}
			else
			{
				return Enumerable.Empty<string>();
			}
		}

		protected const string CusProcedureDateFormat = "dd-MM-yyyy";

		static List<T> GetRecordsFromCsv<T, TMap>(string inputCsvString) where TMap : ClassMap
		{
			var itemList = new List<T>();
			using (TextReader reader = new StringReader(inputCsvString))
			using (var csvReader = new CsvReader(reader))
			{
				csvReader.Configuration.Delimiter = ";";
				csvReader.Configuration.MissingFieldFound = null;
				csvReader.Configuration.Encoding = Encoding.UTF8;
				csvReader.Configuration.RegisterClassMap<TMap>();
				while (csvReader.Read())
				{
					var record = csvReader.GetRecord<T>();
					if (record != null)
					{
						itemList.Add(record);
					}
				}
			}
			return itemList;
		}

		protected static readonly ImmutableHashSet<string> WarehouseValidCodes = ImmutableHashSet.Create("07", "71", "73", "78");
	}
}

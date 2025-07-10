using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater.DataAmendment;
using CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater.Mapping;
using CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater.Schema;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater
{
	public class UNECEUpdater
	{
		readonly ISafeRepository safeRepository;
		public Dictionary<string, int> ColumnIndexes { get; set; }
		string TableName { get; set; }
		public List<UNLOCO> UNLOCOes { get; private set; }
		IUNLOCOAmendStrategy<UNLOCO> nkLOCOAmendStrategy;
		bool isAmendStrategyVerify;

		public UNECEUpdater(ISafeRepository safeRepository)
		{
			Argument.NotNull(safeRepository, nameof(safeRepository));
			this.safeRepository = safeRepository;
			UNLOCOes = new List<UNLOCO>();
			ColumnIndexes = new Dictionary<string, int>();
		}
		public static string MDBWhereClause => "WHERE Country <> '' AND Location <> '' AND NameWoDiacritics <> '' AND Status NOT IN ('XX', 'UR', 'RR')";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Dynamic table name and where construction")]
		public void Read(string mdbFilePath)
		{
			InitializeAmendMdb();
			using (var odbcConnection = OdbcConnectionHelper.GetOdbcConnection(mdbFilePath))
			{
				odbcConnection.Open();
				TableName = OdbcHelper.GetTableNameFromDB(odbcConnection, null);
				ColumnIndexes = OdbcHelper.CreateColumnIndexesByTableName(odbcConnection, TableName);
				using (var odbcCmd = new OdbcCommand($"SELECT * FROM [{TableName}] {MDBWhereClause}", odbcConnection))
				{
					using (var odbcReader = odbcCmd.ExecuteReader())
					{
						while (odbcReader.Read())
						{
							PopulateObjectXML(odbcReader);
						}
					}
				}
			}
			nkLOCOAmendStrategy.FinalizeAmendment();
			CheckDuplicate();
		}

		void InitializeAmendMdb()
		{
			nkLOCOAmendStrategy = new NKCodeAmendStrategy<UNLOCO>();
			isAmendStrategyVerify = nkLOCOAmendStrategy.VerifyStrategy();
			if (isAmendStrategyVerify)
			{
				nkLOCOAmendStrategy.LoadAmendmentData();
			}
			else
			{
				var verifiedErrorMessage = "Failed verifying NKCode amendment strategy";
				throw new DataAmendmentException(verifiedErrorMessage);
			}
		}

		public void PopulateObjectXML(OdbcDataReader reader)
		{
			var obj = new UNLOCO()
			{
				Country = OdbcHelper.GetStringValue(reader, ColumnIndexes[MDBSchema.Country], MDBSchema.CountryMaxLength),
				Location = OdbcHelper.GetStringValue(reader, ColumnIndexes[MDBSchema.Location], MDBSchema.LocationMaxLength),
				Name = OdbcHelper.GetStringValue(reader, ColumnIndexes[MDBSchema.Name], MDBSchema.NameMaxLength),
				NameWoDiacritics = OdbcHelper.GetStringValue(reader, ColumnIndexes[MDBSchema.NameWoDiacritics], MDBSchema.NameWoDiacriticsMaxLength),
				Subdivision = OdbcHelper.GetStringValue(reader, ColumnIndexes[MDBSchema.Subdivision], MDBSchema.SubdivisionMaxLength),
				Status = OdbcHelper.GetStringValue(reader, ColumnIndexes[MDBSchema.Status], MDBSchema.StatusMaxLength),
				Function = OdbcHelper.GetStringValue(reader, ColumnIndexes[MDBSchema.Function], MDBSchema.FunctionMaxLength),
				Date = OdbcHelper.GetStringValue(reader, ColumnIndexes[MDBSchema.Date], MDBSchema.DateMaxLength),
				Coordinates = OdbcHelper.GetStringValue(reader, ColumnIndexes[MDBSchema.Coordinates], MDBSchema.CoordinatesMaxLength)
			};
			if (isAmendStrategyVerify)
				nkLOCOAmendStrategy.Amend(obj);
			if (obj.Function.HasAirport())
			{
				var iata = OdbcHelper.GetStringValue(reader, ColumnIndexes[MDBSchema.IATA], MDBSchema.IATAMaxLength);
				obj.IATA = !string.IsNullOrEmpty(iata) ? iata : obj.Location;
			}
			var matched = MappingUNLOCOHelper.CorrectUNLOCOes.FirstOrDefault(o => o.UniqueIdentifier == obj.UniqueIdentifier);
			UNLOCOes.Add(matched != null ? MappingUNLOCOHelper.UpdateUNLOCOWithCorrectIATA(obj, matched) : obj);
		}

		void CheckDuplicate()
		{
			var duplicateUNLOCO = UNLOCOes.GroupBy(o => o.UniqueIdentifier).Where(g => g.Count() > 1);
			if (duplicateUNLOCO.Any())
			{
				foreach (var group in duplicateUNLOCO)
				{
					for (var i = 0; i < group.Count(); i++)
					{
						if (i == 0)
						{
							continue;
						}
						else
						{
							var actualUnloco = group.ElementAt(i);
							var lastUnloco = group.ElementAt(i - 1);
							CheckDuplicate(actualUnloco, lastUnloco);
						}
					}
				}
			}
		}

		void CheckDuplicate(UNLOCO duplicate, UNLOCO obj)
		{
			Argument.NotNull(duplicate, nameof(duplicate));
			Argument.NotNull(obj, nameof(obj));
			var yearIdx = 0;
			var monthIdx = 2;
			var dupYear = duplicate.Date.Substring(yearIdx, 2);
			var objYear = obj.Date.Substring(yearIdx, 2);
			var dupMonth = duplicate.Date.Substring(monthIdx, 2);
			var objMonth = obj.Date.Substring(monthIdx, 2);
			if (dupYear != objYear)
			{
				if (int.Parse(dupYear, CultureInfo.InvariantCulture) < int.Parse(objYear, CultureInfo.InvariantCulture))
				{
					RemoveDuplicate(duplicate);
				}
				return;
			}
			else if (dupMonth != objMonth)
			{
				if (int.Parse(dupMonth, CultureInfo.InvariantCulture) < int.Parse(objMonth, CultureInfo.InvariantCulture))
				{
					RemoveDuplicate(duplicate);
				}
				return;
			}
			CheckDuplicateInDatabase(duplicate, obj);
		}

		void RemoveDuplicate(UNLOCO duplicate)
		{
			Argument.NotNull(duplicate, nameof(duplicate));
			UNLOCOes.Remove(duplicate);
		}

		void CheckDuplicateInDatabase(UNLOCO duplicate, UNLOCO obj)
		{
			Argument.NotNull(duplicate, nameof(duplicate));
			Argument.NotNull(obj, nameof(obj));
			var dbUnlocoes = safeRepository.Get<RefUNLOCO>()?.Where(o => o.RL_Code == duplicate.UniqueIdentifier).ExecuteAsync();
			if (dbUnlocoes.Result != null)
			{
				var unloco = dbUnlocoes.Result.FirstOrDefault();
				if (unloco != null)
				{
					if (duplicate.Name != unloco.RL_PortName)
					{
						RemoveDuplicate(duplicate);
					}
					else
					{
						RemoveDuplicate(obj);
					}
				}
			}
		}
	}
}

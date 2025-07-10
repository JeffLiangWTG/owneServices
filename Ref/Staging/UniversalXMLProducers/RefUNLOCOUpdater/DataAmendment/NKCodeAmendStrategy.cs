using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater.Schema;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater.DataAmendment
{
	/// <summary>
	/// NKCode Amend Strategy
	/// </summary>
	/// <typeparam name="T">UNLOCO</typeparam>
	public class NKCodeAmendStrategy<T> : IUNLOCOAmendStrategy<T> where T : UNLOCO
	{
		readonly List<NKCode> nKCodes;
		Dictionary<string, int> columnIndexes;
		readonly string tableName;
		int effectedRecordsCount ;

		public NKCodeAmendStrategy()
		{
			nKCodes = new List<NKCode>();
			tableName = "DivisionCodeMapping";
		}

		private void PopulateObject(OdbcDataReader reader)
		{
			var obj = new NKCode()
			{
				Country = OdbcHelper.GetStringValue(reader, columnIndexes[nameof(NKCode.Country)], MDBSchema.CountryMaxLength),
				OldSubdivision = OdbcHelper.GetStringValue(reader, columnIndexes[nameof(NKCode.OldSubdivision)], MDBSchema.SubdivisionMaxLength),
				NewSubdivision = OdbcHelper.GetStringValue(reader, columnIndexes[nameof(NKCode.NewSubdivision)], MDBSchema.SubdivisionMaxLength),
			};
			nKCodes.Add(obj);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Dynamic table name")]
		public void LoadAmendmentData()
		{
			var mdbfile = ConfigurationProvider.ProgramSpecificConfigurationsAmendmentDataFilePath;
			using (var odbcConnection = OdbcConnectionHelper.GetOdbcConnection(mdbfile))
			{
				odbcConnection.Open();
				columnIndexes = OdbcHelper.CreateColumnIndexesByTableName(odbcConnection, tableName);
				using (var cmdMdb = new OdbcCommand($"SELECT * FROM {tableName}", odbcConnection))
				{
					using (var odbcReader = cmdMdb.ExecuteReader())
					{
						while (odbcReader.Read())
						{
							PopulateObject(odbcReader);
						}
					}
				}
			}
		}

		/// <summary>
		/// To Amend specified entity
		/// </summary>
		/// <param name="entity"></param>
		public void Amend(T entity)
		{
			var existObj = nKCodes.FirstOrDefault(r => r.Country == entity.Country && r.OldSubdivision == entity.Subdivision);
			if (existObj != null)
			{
				entity.Subdivision = existObj.NewSubdivision;
				effectedRecordsCount++;
				Console.WriteLine($"Data : RL_RW_NKCode : {existObj.OldSubdivision} RL_RW_RN_NKCountryCode : {existObj.Country} mapped to RL_RW_NKCode : {existObj.NewSubdivision}");
			}
		}

		/// <summary>
		/// Verify Strategy whether works or not
		/// </summary>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Dynamic table name")]
		public bool VerifyStrategy()
		{
			var bResult = false;
			var mdbfile = ConfigurationProvider.ProgramSpecificConfigurationsAmendmentDataFilePath;
			string errorMessage;
			if (!File.Exists(mdbfile))
			{
				errorMessage = $"Amendment datafile located in {mdbfile} does not exist";
				throw new FileNotFoundException(mdbfile);
			}
			using (var odbcConnection = OdbcConnectionHelper.GetOdbcConnection(mdbfile))
			{
				odbcConnection.Open();
				columnIndexes = OdbcHelper.CreateColumnIndexesByTableName(odbcConnection, tableName);
				if (columnIndexes.ContainsKey(nameof(NKCode.Country)) &&
					columnIndexes.ContainsKey(nameof(NKCode.NewSubdivision)) &&
					columnIndexes.ContainsKey(nameof(NKCode.OldSubdivision)))
				{
					var tableExistsQuery = $"SELECT Count(*) FROM  {tableName}";
					using (var cmdMdb = new OdbcCommand(tableExistsQuery, odbcConnection))
					{
						var count = (int)cmdMdb.ExecuteScalar();
						bResult = count > 0;
						if (!bResult)
						{
							errorMessage = $"There are no amendment data in {tableName} table";
							throw new DataAmendmentException(errorMessage);
						}
					}
				}
				else
				{
					errorMessage = $"{tableName} has no correct amendment table fields";
					throw new DataAmendmentException(errorMessage);
				}
			}
			return bResult;
		}

		public void FinalizeAmendment()
		{
			Console.WriteLine($"Amendments were made to {effectedRecordsCount} records");
		}
	}
}

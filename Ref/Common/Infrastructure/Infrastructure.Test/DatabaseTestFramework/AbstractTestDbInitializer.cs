using System;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using Microsoft.Data.SqlClient;

namespace CargoWise.RefDbRepo.Common.Infrastructure.Test;

abstract class AbstractTestDbInitializer
{
	public void SetUp(SqlConnection connection, string collation = "")
	{
		Conn = connection;
		dacPacFileHash = DacPacHashCalculator.GetHash(DacPacFilePath);

		var isDbExist = CheckIfDbExist(GetDbName);
		Console.WriteLine($"Checking database {GetDbName}, {(isDbExist ? "exist" : "does not exist")}");
		if (!isDbExist)
		{
			CreateDb(collation);
			return;
		}

		var isDbLatest = CheckIfDbIsLatest();
		Console.WriteLine($"Checking database {GetDbName} is latest or not, {(isDbLatest ? "Yes" : "No")}");
		if (!isDbLatest)
		{
			ReCreateDb(collation);
		}
	}

	void ReCreateDb(string collation)
	{
		DropDb();
		CreateDb(collation);
	}

	void DropDb()
	{
		DBHelper.TearDown(Conn, null, GetDbName);
	}

	bool CheckIfDbIsLatest()
	{
		var hash = DBHelper.GetExtendedProperty(Conn, GetDbName, TestDbExtendedProperties.DacPacFileModelHash);
		return string.Equals(hash, dacPacFileHash, StringComparison.OrdinalIgnoreCase);
	}

	void CreateDb(string collation)
	{
		Deploy(collation);
		CreateLoginInfo();

		DBHelper.SetTestDbExtendedProperties(Conn, dacPacFileHash, GetDbName);
	}

	void CreateLoginInfo()
	{
		DBHelper.CreateLogin(Conn, GetDbName, TestConnectionString.Reader, TestConnectionString.ReaderPassword,
			ReaderMemeberships);
		DBHelper.CreateLogin(Conn, GetDbName, TestConnectionString.WriterUsername, TestConnectionString.WriterPassword,
			GetWriterMemeberships);
	}

	bool CheckIfDbExist(string dbName)
	{
		return DBHelper.CheckIfDbExist(Conn, dbName);
	}

	protected abstract void Deploy(string collation);

	protected abstract string GetDbName { get; }
	protected abstract string DacPacFilePath { get; }
	protected abstract string[] GetWriterMemeberships { get; }

	string dacPacFileHash;
	protected SqlConnection Conn;

	internal static string[] ReaderMemeberships => ["db_datareader"];
}

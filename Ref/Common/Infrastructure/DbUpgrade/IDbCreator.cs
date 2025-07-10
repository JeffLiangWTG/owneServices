using System.Data;

namespace CargoWise.RefDbRepo.Common.DbUpgrade
{
	public interface IDbCreator
	{
		void ExcuteDbScript(string dbName, string sqlScript, IDbTransaction transaction = null, bool retryConnection = false);
		void CreateDatabase(string dbName, string filePath, string collation = null);
		void DropDatabase(string dbName);
		void Deploy(string dacpacPath, string dbName, string connectionString);
	}
}

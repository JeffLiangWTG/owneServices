using System.Data.SqlClient;
using Moq;
using NUnit.Framework;

namespace CargoWise.eHub.DataAccess.Tests.SQL
{
	internal class DatabaseHelperTests
	{
		[Test]
		public void GeteHubDbConnection_CallsSharedDataAccess()
		{
			var sharedDatabaseHelper = new Mock<eServices.eHubDataAccess.Sql.DatabaseHelper>();
			var eHubDatabaseHelper = new CargoWise.eHub.DataAccess.Sql.DatabaseHelper(sharedDatabaseHelper.Object);
			eHubDatabaseHelper
				.GeteHubDbConnection();
			sharedDatabaseHelper.Verify(x => x
				.GeteHubDbConnection(), Times.Once);
		}

		[Test]
		public void GetEdiProdCacheDbConnection_CallsSharedDataAccess()
		{
			var sharedDatabaseHelper = new Mock<eServices.eHubDataAccess.Sql.DatabaseHelper>();
			var eHubDatabaseHelper = new CargoWise.eHub.DataAccess.Sql.DatabaseHelper(sharedDatabaseHelper.Object);
			eHubDatabaseHelper
				.GetEdiProdCacheDbConnection();
			sharedDatabaseHelper.Verify(x => x
				.GetEdiProdCacheDbConnection(), Times.Once);
		}

		[Test]
		public void GetEdiProdDbConnection_CallsSharedDataAccess()
		{
			var sharedDatabaseHelper = new Mock<eServices.eHubDataAccess.Sql.DatabaseHelper>();
			var eHubDatabaseHelper = new CargoWise.eHub.DataAccess.Sql.DatabaseHelper(sharedDatabaseHelper.Object);
			eHubDatabaseHelper
				.GetEdiProdDbConnection();
			sharedDatabaseHelper.Verify(x => x
				.GetEdiProdDbConnection(), Times.Once);
		}

		[Test]
		public void GetCommand_CallsSharedDataAccess()
		{
			var sharedDatabaseHelper = new Mock<eServices.eHubDataAccess.Sql.DatabaseHelper>();
			var eHubDatabaseHelper = new CargoWise.eHub.DataAccess.Sql.DatabaseHelper(sharedDatabaseHelper.Object);
			eHubDatabaseHelper
				.GetCommand("commandText", (SqlConnection)null);
			sharedDatabaseHelper.Verify(x => x
				.GetCommand("commandText", (SqlConnection)null), Times.Once);
		}

		[Test]
		public void GetCommand_Transaction_CallsSharedDataAccess()
		{
			var sharedDatabaseHelper = new Mock<eServices.eHubDataAccess.Sql.DatabaseHelper>();
			var eHubDatabaseHelper = new CargoWise.eHub.DataAccess.Sql.DatabaseHelper(sharedDatabaseHelper.Object);
			eHubDatabaseHelper
				.GetCommand("commandText", (SqlConnection)null, (SqlTransaction)null);
			sharedDatabaseHelper.Verify(x => x
				.GetCommand("commandText", (SqlConnection)null, (SqlTransaction)null), Times.Once);
		}
	}
}

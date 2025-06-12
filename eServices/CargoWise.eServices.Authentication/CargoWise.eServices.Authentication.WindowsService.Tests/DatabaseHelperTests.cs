using System;
using System.Data.SqlClient;
using Moq;
using NUnit.Framework;

namespace CargoWise.eServices.Authentication.WindowsService.Tests
{
	[TestFixture]
	public class DatabaseHelperTests
	{
		[Test]
		public void TestGetDbConnectionFromConnectionString()
		{
			var helper = new Databasehelper();
			var connectionString = "data source=localhost";
			Assert.IsInstanceOf<SqlConnection>(helper.GetDbConnection(connectionString));
		}

		[Test]
		public void TestUnhandledException()
		{
			var dbMock = new Mock<Databasehelper> { CallBase = true };
			dbMock.Setup(_ => _.GetDbConnection(It.IsAny<string>())).Throws(new ArgumentException("Unknown Exception"));

			try
			{
				dbMock.Object.TransferData(string.Empty, string.Empty);
				Assert.Fail();
			}
			catch(ArgumentException ex)
			{
				Assert.AreEqual("Unknown Exception", ex.Message);
			}
			dbMock.VerifyAll();
		}

		[Test]
		public void TestHandledExceptions()
		{
			var dbMock = new Mock<Databasehelper> { CallBase = true };
			var connection = new SqlConnection(@"Data Source=.;Database=THIS_IS_NOT_AN_EXISTING_DATABASE;Connection Timeout=1");
			dbMock.Setup(_ => _.GetDbConnection(It.IsAny<string>())).Throws(new InvalidOperationException("Invalid Operation Exception"));
			dbMock.Setup(_ => _.GetDbConnection(It.IsAny<string>())).Returns(connection);
			dbMock.Object.TransferData(string.Empty, string.Empty);
			dbMock.Object.TransferData(string.Empty, string.Empty);
			dbMock.VerifyAll();
		}
	}
}

using System;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	class NumberFountainHelperTest : TestCase
	{
		#region TestGetNextReferenceNumber

		[UseSnapshotProtection]
		public void TestGetNextReferenceNumber()
		{
			var newFactory = new BusinessObjectFactory();
			using (SetupTemporaryTable(newFactory))
			{
				var connection = ((IDbConnected)newFactory).Connection;

				var numberFountainMock = new Mock<INumberFountainProxy>();
				numberFountainMock.Setup(n => n.GetNextFormatted(newFactory))
					.Callback(
						() =>
						{
							Assert("Get next method must be inside a transaction.", newFactory.IsInTransaction);
						}).Returns(() =>
							{
								connection.ExecuteNonQuery("INSERT INTO #TestTransaction VALUES('ABC')");
								return "NextNumber";
							});

				var nextReferenceNumber = NumberFountainHelper.GetNextReferenceNumber(newFactory, numberFountainMock.Object);
				AssertEquals("NextNumber", nextReferenceNumber);
				int result = Convert.ToInt32(connection.ExecuteScalar("SELECT COUNT(*) FROM #TestTransaction"));
				AssertEquals("Committed One Row", 1, result);
			}
		}

		#endregion

		#region TestGetNextReferenceNumber_NotNull

		public void TestGetNextReferenceNumber_NotNull()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => NumberFountainHelper.GetNextReferenceNumber(null, new Mock<INumberFountainProxy>().Object));
			AssertExceptionThrown(typeof(ArgumentNullException), () => NumberFountainHelper.GetNextReferenceNumber(new BusinessObjectFactory(), null));
		}

		#endregion

		#region TestGetNextReferenceNumber_ThrowErrorWhenFails

		[UseSnapshotProtection]
		public void TestGetNextReferenceNumber_ThrowErrorWhenFails()
		{
			var newFactory = new BusinessObjectFactory();
			using (SetupTemporaryTable(newFactory))
			{
				var connection = ((IDbConnected)newFactory).Connection;
				var numberFountainMock = new Mock<INumberFountainProxy>();
				numberFountainMock.Setup(n => n.GetNextFormatted(newFactory))
					.Returns(() =>
					{
						connection.ExecuteNonQuery("INSERT INTO #TestTransaction VALUES('ABC')");
						throw new Exception("Unexpected exception");
					});
				AssertExceptionThrown<Exception>("Exception must be thrown.", () => NumberFountainHelper.GetNextReferenceNumber(newFactory, numberFountainMock.Object));
				int result = Convert.ToInt32(connection.ExecuteScalar("SELECT COUNT(*) FROM #TestTransaction"));
				AssertEquals("Row must be rollbacked.", 0, result);
			}
		}

		#endregion

		#region SetupTemporaryTable

		IDisposable SetupTemporaryTable(BusinessObjectFactory factory)
		{
			var connection = ((IDbConnected)factory).Connection;
			DropTransactionTempTable(connection);
			connection.ExecuteNonQuery("CREATE TABLE #TestTransaction (One varchar(3))");

			return new DisposableAction(() => DropTransactionTempTable(connection));
		}

		void DropTransactionTempTable(DbConnection connection)
		{
			connection.ExecuteNonQuery("IF (OBJECT_ID('tempdb..#TestTransaction') IS NOT NULL) DROP TABLE #TestTransaction");
		}

		#endregion
	}
}

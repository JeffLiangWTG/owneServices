using System.Data.SqlClient;
using System;
using System.Reflection;
using CargoWise.eServices.USCustoms.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eServices.USCustoms.Tests.GatewayServices
{
	[TestClass]
	public class DatabaseHelperTests
	{
		[TestMethod]
		public void TestDatabaseHelper_AccessDatabaseWithRetries_ShouldNotRetry()
		{
			try
			{
				DatabaseHelper.AccessDatabaseWithRetries(() => throw new InsufficientMemoryException("Exception should be passed to caller"));
				Assert.Fail("Should throw");
			}
			catch (InsufficientMemoryException ex)
			{
				Assert.AreEqual("Exception should be passed to caller", ex.Message);
			}

			try
			{
				DatabaseHelper.AccessDatabaseWithRetries(() => throw new InvalidOperationException("Not related to Timeout"));
				Assert.Fail("Should throw");
			}
			catch (InvalidOperationException ex)
			{
				Assert.AreEqual("Not related to Timeout", ex.Message);
			}
		}

		[TestMethod]
		public void TestDatabaseHelper_AccessDatabaseWithRetries_ShouldNotRetry_SqlExceptionBlacklist()
		{
			try
			{
				DatabaseHelper.AccessDatabaseWithRetries(() => throw CreateSqlException(2601));
				Assert.Fail("Should throw");
			}
			catch (SqlException ex)
			{
				Assert.IsTrue(ex.Number == 2601);
			}

			try
			{
				DatabaseHelper.AccessDatabaseWithRetries(() => throw CreateSqlException(2627));
				Assert.Fail("Should throw");
			}
			catch (SqlException ex)
			{
				Assert.IsTrue(ex.Number == 2627);
			}

			try
			{
				DatabaseHelper.AccessDatabaseWithRetries(() => throw CreateSqlException(8152));
				Assert.Fail("Should throw");
			}
			catch (SqlException ex)
			{
				Assert.IsTrue(ex.Number == 8152);
			}

			try
			{
				DatabaseHelper.AccessDatabaseWithRetries(() => throw CreateSqlException(50000));
				Assert.Fail("Should throw");
			}
			catch (SqlException ex)
			{
				Assert.IsTrue(ex.Number == 50000);
			}

			try
			{
				DatabaseHelper.AccessDatabaseWithRetries(() => throw CreateSqlException(547));
				Assert.Fail("Should throw");
			}
			catch (SqlException ex)
			{
				Assert.IsTrue(ex.Number == 547);
			}
		}

		[TestMethod]
		public void TestDatabaseHelper_AccessDatabaseWithRetries_ShouldRetry_SqlExceptionWhitelist()
		{
			int retry = 2;
			DatabaseHelper.AccessDatabaseWithRetries(() =>
			{
				if (retry > 0)
				{
					retry--;
					throw CreateSqlException(1205);
				}
			});
		}

		/// <summary>
		/// Create SqlException using reflection.
		/// </summary>
		/// <param name="infoNumber"></param>
		/// <param name="errorState"></param>
		/// <param name="errorClass"></param>
		/// <param name="server"></param>
		/// <param name="errorMessage"></param>
		/// <param name="procedure"></param>
		/// <param name="lineNumber"></param>
		/// <returns></returns>
		internal static SqlException CreateSqlException(int infoNumber = 0, byte errorState = 0, byte errorClass = 0, string server = "", string errorMessage = "", string procedure = "", int lineNumber = 0)
		{
			var collection = typeof(SqlErrorCollection)
				.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[0], null).Invoke(new object[] { }) as SqlErrorCollection;

			var error = typeof(SqlError)
				.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null,
					new[]
					{
						typeof (int), typeof (byte), typeof (byte), typeof (string), typeof(string), typeof (string),
						typeof (int)
					},
					null).Invoke(new object[] { infoNumber, errorState, errorClass, server, errorMessage, procedure, lineNumber }) as SqlError;

			typeof(SqlErrorCollection)
				.GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Instance)
				.Invoke(collection, new object[] { error });


			var e = typeof(SqlException)
				.GetMethod("CreateException", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(SqlErrorCollection), typeof(string) }, null)
				.Invoke(null, new object[] { collection, "7.0.0" }) as SqlException;

			return e;
		}
	}
}

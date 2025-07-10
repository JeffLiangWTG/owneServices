using System;
using System.Linq;
using System.Reflection;
using Microsoft.Data.SqlClient;

namespace CargoWise.RefDbRepo.Common.Utils
{
	public class SqlExceptionBuilder
	{
		int errorNumber;
		string errorMessage;

		public SqlException Build()
		{
			var error = CreateError();
			var errorCollection = CreateErrorCollection(error);
			var exception = CreateException(errorCollection);

			return exception;
		}

		public SqlExceptionBuilder WithErrorNumber(int number)
		{
			errorNumber = number;
			return this;
		}

		public SqlExceptionBuilder WithErrorMessage(string message)
		{
			errorMessage = message;
			return this;
		}

		SqlError CreateError()
		{
			var ctors = typeof(SqlError).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
			var firstSqlErrorCtor = ctors.FirstOrDefault(ctor => ctor.GetParameters().Length == 8);
			var error = firstSqlErrorCtor.Invoke(
				new object[]
				{
					errorNumber,
					new byte(),
					new byte(),
					string.Empty,
					string.Empty,
					string.Empty,
					new int()
					,null
				}) as SqlError;

			return error;
		}

		static SqlErrorCollection CreateErrorCollection(SqlError error)
		{
			var sqlErrorCollectionCtor = typeof(SqlErrorCollection).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault();
			var errorCollection = sqlErrorCollectionCtor.Invoke(new object[] { }) as SqlErrorCollection;
			typeof(SqlErrorCollection).GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(errorCollection, new object[] { error });

			return errorCollection;
		}

		SqlException CreateException(SqlErrorCollection errorCollection)
		{
			Argument.Argument.NotNull(errorCollection, nameof(errorCollection));

			var ctor = typeof(SqlException).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault();
			var sqlException = ctor.Invoke(
				new object[]
				{
				errorMessage,
				errorCollection,
				null,
				Guid.NewGuid()
				}) as SqlException;

			return sqlException;
		}
	}

	public static class KnownSqlExceptionsNumbers
	{
		public const int Timeout = -2;
		public const int TransactionDeadlock = 1205;
	}
}

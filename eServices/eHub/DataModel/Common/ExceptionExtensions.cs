using System;
using System.Data;
using System.Data.Entity.Core;
using System.Data.SqlClient;

namespace CargoWise.eHub.DataModel.Common
{
	public static class ExceptionExtensions
	{
		public static bool IsPrimaryKeyViolation(this DataException ex)
		{
			return IsSqlExceptionNumber(ex, 2627);
		}

		public static bool IsDuplicateKeyViolation(this DataException ex)
		{
			return IsSqlExceptionNumber(ex, 2601);
		}

		public static bool IsDeadlockVictim(this DataException ex)
		{
			return IsSqlExceptionNumber(ex, 1205);
		}

		static bool IsSqlExceptionNumber(Exception ex, int number)
		{
			if (ex.InnerException is UpdateException)
				if (ex.InnerException.InnerException is SqlException)
					return (ex.InnerException.InnerException as SqlException).Number == number;

			return false;
		}
	}
}

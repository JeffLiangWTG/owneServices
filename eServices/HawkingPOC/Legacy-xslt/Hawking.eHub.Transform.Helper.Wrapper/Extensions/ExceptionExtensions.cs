using System;
using System.Data;
using System.Data.SqlClient;

namespace Hawking.eHub.Transform.Helper.Wrapper.Extensions
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
            if (ex.InnerException.InnerException is SqlException)
                return (ex.InnerException.InnerException as SqlException).Number == number;

            return false;
        }
    }
}

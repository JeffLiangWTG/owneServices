using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;
using Common.Logging;
using Common.Logging.Simple;

namespace CargoWise.eHub.DataModel.Common
{
	public class DatabaseAccessHelpers
	{
		static DatabaseAccessHelpers()
		{
			if (int.TryParse(ConfigurationManager.AppSettings["DatabaseAccessRetryTimeLimit"], out int result) && result > 0)
				RetryTimeLimit = result * 1000;
		}

		public static int RetryTimeLimit = 600000;

		public static T AccessDatabaseWithRetries<T>(Func<T> processing, ILog logger = null)
		{
			T result = default;
			AccessDatabaseWithRetries(() => { result = processing(); }, logger);
			return result;
		}

		public static void AccessDatabaseWithRetries(Action processing, ILog logger = null)
		{
			if (processing == null) throw new ArgumentNullException("processing");
			if (logger == null) logger = new NoOpLogger();

			var stopwatch = Stopwatch.StartNew();

			var retry = true;
			while (retry)
			{
				try
				{
					processing();
					retry = false;
				}
				catch (InvalidOperationException ex) when (ex.Message.StartsWith("Timeout expired"))
				{
					RetryOrThrow(stopwatch, ex, logger);
				}
				catch (SqlException ex) when (IsRetryableSqlException(ex))
				{
					/*  Same with DataAccess.Sql.DatabaseHelper
					 *
					 *  Currently we use a blacklist here that retry on all SqlExceptions except some special cases, which is not a good design
						We'll maintain a whitelist later to only catch essential exceptions
						To collect enough exceptions and maintain a whitelist will take about more than 6 months,
						so after 6 months, we can start a new refactor WI.

						SqlConnectionBroken = -1,
						SqlTimeout = -2,
						SqlLoginError_SharedMemoryProvider = 233,
						SqlOutOfMemory = 701,
						SqlFailoverClusterAvailabilityIssue = 983,
						SqlOutOfLocks = 1204,
						SqlDeadlockVictim = 1205,
						SqlLockRequestTimeout = 1222,
						SqlTimeoutWaitingForMemoryResource = 8645,
						SqlLowMemoryCondition = 8651,
						SqlWordbreakerTimeout = 30053

					 */
					RetryOrThrow(stopwatch, ex, logger);
				}
				catch (DataException ex)
				{
					Exception innerEx = ex;
					var shouldThrow = true;
					while (innerEx.InnerException != null)
                    {
						innerEx = innerEx.InnerException;
						if (innerEx is SqlException && IsRetryableSqlException(innerEx as SqlException))
                        {
							RetryOrThrow(stopwatch, innerEx, logger);
							shouldThrow = false;
							break;
						}
					}
					if(shouldThrow)
						throw;
				}
			}
		}

		static void RetryOrThrow(Stopwatch stopwatch, Exception ex, ILog logger)
		{
			var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
			if (elapsedMilliseconds < RetryTimeLimit)
			{
				int retryInterval;
				if (elapsedMilliseconds < 5000)
				{
					retryInterval = 500;
				}
				else if (elapsedMilliseconds < 10000)
				{
					retryInterval = 1000;
				}
				else if (elapsedMilliseconds < 60000)
				{
					retryInterval = 5000;
				}
				else
				{
					retryInterval = 60000;
				}

				if (elapsedMilliseconds + retryInterval < RetryTimeLimit)
				{
					logger.Debug("Exception accessing database. Will be retried. Exception ---> " + ex.Message);
					Task.Delay(retryInterval).Wait();
					return;
				}
			}

			logger.Debug("Exception accessing database. RetryTimeLimit has reached so won't retry. Exception ---> " + ex.Message);
			throw new TimeoutException(string.Format("Time limit '{0}' exceeded while accessing database. See inner exception for the actual error.", TimeSpan.FromMilliseconds(RetryTimeLimit)), ex);
		}

		internal static Func<SqlException, bool> IsRetryableSqlException = (ex) => !IsConflictKeyConstrainViolation(ex) && !IsDuplicateKeyViolation(ex) && !IsPrimaryKeyViolation(ex) && !IsDataTruncatedError(ex) && !IsCustomRaiseError(ex);

		internal static Func<SqlException, bool> IsConflictKeyConstrainViolation = ex => ex.Number == 547;

		internal static Func<SqlException, bool> IsDuplicateKeyViolation = ex => ex.Number == 2601;

		internal static Func<SqlException, bool> IsPrimaryKeyViolation = ex => ex.Number == 2627;

		internal static Func<SqlException, bool> IsDataTruncatedError = ex => ex.Number == 8152;

		internal static Func<SqlException, bool> IsCustomRaiseError = ex => ex.Number == 50000;
	}
}

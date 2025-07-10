using System;
using CargoWise.Data;

namespace Enterprise.Services.ServiceHost
{
	static class PaveControllerHelper
	{
		internal static bool ShouldReportError(Exception ex)
		{
			return !(ex is SqlException sqlException) || !DoesExceptionIndicateThatDatabaseIsUpgrading(sqlException);
		}

		static bool DoesExceptionIndicateThatDatabaseIsUpgrading(SqlException ex)
		{
			return new DbErrorMatch(ex).ExceptionType == DbErrorType.LoginDisabled;
		}
	}
}

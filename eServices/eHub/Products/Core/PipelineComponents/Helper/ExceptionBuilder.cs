using System;
using System.Collections;
using System.Data.SqlClient;

namespace CargoWise.eHub.Products.Core.PipelineComponents
{
	public static class ExceptionBuilder
	{
		public static Exception New(SqlException ex)
		{
			if (!IsApplicationException(ex.Number))
			{
				return new InfrastructureException(String.Concat(ex.Message, handlerMessage), ex);
			}

			return new InvalidOperationException(string.Format("Invalid Message: {0}", ex));
		}

		static bool IsApplicationException(int sqlErrorCode)
		{
			int[] sqlApplicationErrorCode = {50000, 8152};
			return ((IList) sqlApplicationErrorCode).Contains(sqlErrorCode);
		}

		const string handlerMessage = "The pipeline component threw database related exception.";
	}
}
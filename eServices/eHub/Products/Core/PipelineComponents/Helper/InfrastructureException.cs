using System;
using System.Data.SqlClient;

namespace CargoWise.eHub.Products.Core.PipelineComponents
{
	class InfrastructureException : Exception
	{
		public InfrastructureException(string message, SqlException ex): base(message, ex)
		{
		}
	}
}
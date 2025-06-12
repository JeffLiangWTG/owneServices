using System;
using System.Data;
using System.Linq;

namespace CargoWise.eServices.Billing.DataAccess
{
	public class SPExecutionParam
	{
		public string Name { get; set; }
		public int TimeoutInSeconds { get; set; }
		public SqlSerializableParameter[] SqlParams { get; set; }
		public override string ToString() => $"Store Procedure Name: {Name}. Timeout: {TimeoutInSeconds}. Params: {string.Join(",", SqlParams.Select(x => $"[{x}]").ToArray())}";
	}

	public class SqlSerializableParameter
	{
		public string Name { get; set; }
		public SqlDbType DbType { get; set; }
		public object Value { get; set; }

		public override string ToString() => $"Name: {Name}, DbType: {DbType}, Value: {Value}";
	}
}

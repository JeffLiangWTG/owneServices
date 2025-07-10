using System;
using System.Linq;
using System.Reflection;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	static class SqlExceptionTestHelper
	{
		static T Construct<T>(params object[] p)
		{
			var ctors = typeof(T).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
			return (T)ctors.First(ctor => ctor.GetParameters().Length == p.Length).Invoke(p);
		}

		public static SqlException NewSqlException(int number, string errorMessage = "error message", string proc = "proc", string serverName = "server name")
		{
			var collection = Construct<SqlErrorCollection>();
#if !WINZOR
			var error = Construct<SqlError>(number, (byte)2, (byte)3, serverName, errorMessage, proc, 100);
#else
			var error = Construct<SqlError>(number, (byte)2, (byte)3, serverName, errorMessage, proc, 100, null);
#endif
			typeof(SqlErrorCollection)
				.GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Instance)
				.Invoke(collection, new object[] { error });

			return (SqlException)typeof(SqlException)
				.GetMethod("CreateException", BindingFlags.NonPublic | BindingFlags.Static,
					null,
					CallingConventions.ExplicitThis,
					new[] { typeof(SqlErrorCollection), typeof(string) },
					Array.Empty<ParameterModifier>())
				.Invoke(null, new object[] { collection, "7.0.0" });
		}
	}
}

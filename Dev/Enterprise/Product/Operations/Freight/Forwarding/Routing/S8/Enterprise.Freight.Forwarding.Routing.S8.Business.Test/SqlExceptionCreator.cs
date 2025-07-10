using System.Linq;
using System.Reflection;

namespace Enterprise.Freight.Forwarding.Routing.S8.Business.Test
{
	public static class SqlExceptionCreator
	{
		static T Construct<T>(params object[] p)
		{
			var ctors = typeof(T).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
			return (T)ctors.First(ctor => ctor.GetParameters().Length == p.Length).Invoke(p);
		}

		internal static SqlException NewSqlException(string message, int number = 1)
		{
			SqlErrorCollection collection = Construct<SqlErrorCollection>();
			SqlError error = Construct<SqlError>(number, (byte)2, (byte)3, "server name", message, "proc", 100);

			typeof(SqlErrorCollection)
				.GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Instance)
				.Invoke(collection, new object[] { error });

			return typeof(SqlException)
				.GetMethod("CreateException", BindingFlags.NonPublic | BindingFlags.Static,
					null,
					CallingConventions.ExplicitThis,
					new[] { typeof(SqlErrorCollection), typeof(string) },
					System.Array.Empty<ParameterModifier>())
				.Invoke(null, new object[] { collection, "7.0.0" }) as SqlException;
		}
	}
}

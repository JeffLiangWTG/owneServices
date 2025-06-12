using System.Linq;
using Xtrade.Core.Objects;
using static System.Reflection.BindingFlags;

namespace XT.Internal.API
{
	public static class Extensions
	{
		public static TOut InvokeInternalFunction<T, TOut, TIn>(this T obj, string name, TIn parameter)
		{
			var method = typeof(T)
				.GetMethods(NonPublic | Instance)
				.Where(x => x.Name == name)
				.Single(x => x.GetParameters().Single().ParameterType == typeof(TIn) && x.ReturnParameter.ParameterType == typeof(TOut));
			return (TOut)method.Invoke(obj, new object[] { parameter });
		}

		public static TOut InvokeInternalFunction<T, TOut, TIn1, TIn2>(this T obj, string name, TIn1 param1, TIn2 param2)
		{
			var method = typeof(T)
				.GetMethods(NonPublic | Instance)
				.Where(x => x.Name == name)
				.Single(x =>
				{
					var parameters = x.GetParameters();
					return parameters.Length == 2 &&
						parameters.First().ParameterType == typeof(TIn1) &&
						parameters.Last().ParameterType == typeof(TIn2) &&
						x.ReturnParameter.ParameterType == typeof(TOut);
				});
			return (TOut)method.Invoke(obj, new object[] { param1, param2 });
		}
	}
}

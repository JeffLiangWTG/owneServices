using System.Diagnostics;

namespace CargoWise.Data.SqlProxyServer.Core;

public class ArgumentNullException(string? message) : ArgumentException(message)
{
	public static void ThrowIfNull(object? instance = null)
	{
		if (instance is null)
		{
			Throw($"{new StackTrace()}");
		}
	}

	internal static void Throw(string? paramName) => throw new ArgumentNullException(paramName);
}

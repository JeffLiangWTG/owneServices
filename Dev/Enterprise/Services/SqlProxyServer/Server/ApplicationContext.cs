namespace CargoWise.Data.SqlProxyServer;

public static class ApplicationContext
{
	public static void Initialize(string serverName, string databaseName)
	{
		if (!string.IsNullOrEmpty(ServerName) || !string.IsNullOrEmpty(DatabaseName))
		{
			throw new InvalidOperationException("ApplicationContext has already been initialized.");
		}

		ServerName = serverName;
		DatabaseName = databaseName;
	}

	public static bool IsMainDb(string? serverName, string? databaseName)
	{
		if (
			string.IsNullOrEmpty(ServerName)
			|| string.IsNullOrEmpty(DatabaseName)
			|| string.IsNullOrEmpty(serverName)
			|| string.IsNullOrEmpty(databaseName))
		{
			return false;
		}
		
		return
			string.Equals(ServerName, serverName, StringComparison.OrdinalIgnoreCase)
			&& string.Equals(DatabaseName, databaseName, StringComparison.OrdinalIgnoreCase);
	}

	public static string? ServerName { get; private set; }

	public static string? DatabaseName { get; private set; }
}

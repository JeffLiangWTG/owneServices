namespace CargoWise.Data.SqlProxyServer.Core;

public sealed class DisposableConnection(SqlConnection connection, SqlTransaction? transaction) : IDisposable
{
	public void Dispose()
	{
		if (Transaction == null)
		{
			Connection.Dispose();
		}

		Disposing?.Invoke(this, EventArgs.Empty);
	}

	public event EventHandler? Disposing;

	public SqlTransaction? Transaction { get; } = transaction;
	public SqlConnection Connection { get; } = connection;
}

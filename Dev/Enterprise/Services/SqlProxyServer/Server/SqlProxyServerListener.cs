using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipes;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using CargoWise.Data.SqlProxy.Interface;
using WTG.StaticAnalysis.Annotation;
using static System.FormattableString;

namespace CargoWise.Data.SqlProxyServer.Server;

[SuppressMessage("CargoWiseOne", "CW1106:DebugMessages")]
[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer")]
[SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccess")]
static class SqlProxyServerListener
{
	internal static async Task StartClientConnectionsListenerAsync(string serverName, string databaseName, string apiPort, CancellationToken existingCancellationToken)
	{
		var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(existingCancellationToken);
		var cancellationToken = cancellationTokenSource.Token;
		var pipeName = SqlProxyNamingConvention.SqlProxyServiceNamedPipeName(serverName, databaseName);
		var clientHandlerTasks = new ConcurrentDictionary<Task, byte>();

		const int initialNumberOfListeners = 3;

		try
		{
			for (var i = 0; i < initialNumberOfListeners; i++)
			{
				var task = Task.Run(() => ListenForClientsAsync(pipeName, apiPort, clientHandlerTasks, cancellationToken), cancellationToken)
					.ContinueWith(t => clientHandlerTasks.TryRemove(t, out _), cancellationToken);

				clientHandlerTasks.TryAdd(task, 0);
			}

			Environment.SetEnvironmentVariable("GlowLoaderService_PipeName", $"{pipeName}", EnvironmentVariableTarget.Process);

			// Keep server running until cancelled
			await Task.Delay(-1, cancellationToken);
		}
		catch (OperationCanceledException)
		{
			// Expected on cancellation
		}
		finally
		{
			await cancellationTokenSource.CancelAsync();

			if (clientHandlerTasks.Any())
			{
				Console.WriteLine("Waiting for active clients to complete...");
				await Task.WhenAll(clientHandlerTasks.Keys);
				clientHandlerTasks.Clear();
			}

			cancellationTokenSource.Dispose();
		}
	}

	static async Task ListenForClientsAsync(string pipeName, string apiPort, ConcurrentDictionary<Task, byte> clientHandlerTasks, CancellationToken cancellationToken)
	{
		while (!cancellationToken.IsCancellationRequested)
		{
			var namedPipeServerStream = new NamedPipeServerStream(
				pipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

			try
			{
				await namedPipeServerStream.WaitForConnectionAsync(cancellationToken);

				if (!cancellationToken.IsCancellationRequested)
				{
					// always create a new listener for the next client after accepting the current one
					var task = Task.Run(() => ListenForClientsAsync(pipeName, apiPort, clientHandlerTasks, cancellationToken), cancellationToken)
						.ContinueWith(t => clientHandlerTasks.TryRemove(t, out _), cancellationToken);

					clientHandlerTasks.TryAdd(task, 0);
				}

				// Handle current client
				var clientTask = Task.Run(() => HandleClientConnection(namedPipeServerStream, apiPort, cancellationToken), cancellationToken)
					.ContinueWith(t =>
					{
						namedPipeServerStream.Dispose();
						clientHandlerTasks.TryRemove(t, out _);
					}, cancellationToken);

				clientHandlerTasks.TryAdd(clientTask, 0);
			}
			catch (OperationCanceledException)
			{
				await namedPipeServerStream.DisposeAsync();
				break;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[{Task.CurrentId}] Error accepting client: {ex.Message}");
				await namedPipeServerStream.DisposeAsync();
			}
		}
	}

	static async Task HandleClientConnection(NamedPipeServerStream namedPipeServerStream, string apiPort, CancellationToken cancellationToken)
	{
		try
		{
			// Try to get the client process ID
			if (GetNamedPipeClientProcessId(namedPipeServerStream.SafePipeHandle.DangerousGetHandle(), out var clientPid))
			{
				ActiveClientProcesses.TryAdd((int)clientPid, 0);
			}

			// Send the port number to the client
			var bytes = Encoding.UTF8.GetBytes(apiPort);
			await namedPipeServerStream.WriteAsync(bytes, cancellationToken);

			var buffer = new byte[1024];
			while (namedPipeServerStream.IsConnected && !cancellationToken.IsCancellationRequested)
			{
				try
				{
					Array.Clear(buffer);

					var bytesRead = await namedPipeServerStream.ReadAsync(buffer, cancellationToken);
					if (bytesRead == 0)
					{
						break; // Client disconnected
					}

					var query = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

					// Process client query and prepare a response
					var response = ParseClientMessage(query, out string command, out var value);
					switch (command)
					{
						case ExitCommand:
							Console.WriteLine($"info|client {query} has exited.");
							ActiveClientProcesses.TryRemove((int)clientPid, out _);
							break;
						case "pid" when uint.TryParse(value, out var pid):
							ActiveClientProcesses.TryAdd((int)pid, 0);
							break;
						case "port":
							Console.WriteLine($"info|client {query} has requested the port.");
							break;
					}

					var responseBytes = Encoding.UTF8.GetBytes(Invariant($"{command}|{response}"));

					// Send the response back to the client
					await namedPipeServerStream.WriteAsync(responseBytes, cancellationToken);
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error handling query: {ex.Message}");
					break;
				}
			}

			string ParseClientMessage(string query, out string command, out string value)
			{
				var queryString = query.ToLower().Split('|');
				command = queryString[0];
				value = queryString.Length > 1 ? queryString[1] : string.Empty;

				return query.ToLower() switch
				{
					"status" => "running",
					"port" => apiPort,
					ExitCommand => "Exiting",
					_ => query
				};
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"[{Task.CurrentId}] Error handling client: {ex.Message}");
		}
	}

	internal static async Task StartParentProcessMonitorAsync(CancellationTokenSource cancellationTokenSource)
	{
		var parentProcessId = GetParentProcessId(out var parentProcessName);
		_ = ActiveClientProcesses.TryAdd(parentProcessId, 0);

		var stopWatch = new Stopwatch();
		var nonCargoWiseParentPid = -1;
		if (!parentProcessName.Contains("CargoWise"))
		{
			nonCargoWiseParentPid = parentProcessId;
			stopWatch.Start();
		}
		Console.WriteLine($"info|Started by parent process {parentProcessId} ({parentProcessName}).");

		while (!cancellationTokenSource.Token.IsCancellationRequested && ActiveClientProcesses.Count > 0)
		{
			var clientProcessIds = ActiveClientProcesses.Keys.ToArray();
			foreach (var pid in clientProcessIds)
			{
				try
				{
					_ = Process.GetProcessById(pid);
				}
				catch
				{
					Console.WriteLine($"info|process {pid} has exited.");
					ActiveClientProcesses.TryRemove(pid, out _);

					break;
				}
			}

			await Task.Delay(1000, cancellationTokenSource.Token);

			if (nonCargoWiseParentPid > 0 && ActiveClientProcesses.Count <= 1 && stopWatch.Elapsed > TimeSpan.FromMinutes(5))
			{
				ActiveClientProcesses.TryRemove(nonCargoWiseParentPid, out _);
			}
		}

		// when all clients have exited, cancel the cancellation token to terminate the server
		await cancellationTokenSource.CancelAsync();

		return;

		static int GetParentProcessId(out string processName)
		{
			processName = string.Empty;

			var parentId = 0;
			var currentPid = Process.GetCurrentProcess().Id;
			using (var managementObject = new ManagementObject($"win32_process.Handle='{currentPid}'"))
			{
				managementObject.Get();
				parentId = Convert.ToInt32(managementObject["ParentProcessID"]);
			}

			if (parentId > 0)
			{
				using var managementObject = new ManagementObject($"win32_process.Handle='{parentId}'");
				processName = managementObject["Description"]?.ToString() ?? string.Empty;
			}

			return parentId;
		}
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	static extern bool GetNamedPipeClientProcessId(IntPtr pipe, out uint clientProcessId);

	const string ExitCommand = "exit";

	[ThreadSafe]
	static readonly ConcurrentDictionary<int, byte> ActiveClientProcesses = new();
}

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter;
using CargoWise.eServices.Encryption.Server.Decryptor;
using Common.Logging;

namespace CargoWise.eHub.BizTalkAdapters.WinScp.Common
{
	public abstract class WinScpReceiverEndpoint : ReceiverEndpoint
	{
		internal readonly IWinScpClientFactory winScpClientFactory;
		private readonly Type configurationType;
		private const int MaxRetries = 10;

		private const string ClientRegistrationQuery = @"
SELECT
	CX_Attr1 AS URI,
	CX_Password1 AS Password
FROM eHubClientRegistration AS Registration
	JOIN eHubRegistrationType AS RegistrationType ON RT_PK = CX_RT
WHERE RT_ID = @RT_ID";

		internal static readonly Lazy<DataTable> CustomDataTable = new(() => new DataTable
		{
			Columns = { new DataColumn("URI", typeof(string)), new DataColumn("Password", typeof(string)) }
		});

		internal virtual WinScpReceiveConfiguration Config { get; private set; }

		protected WinScpReceiverEndpoint(IWinScpClientFactory winScpClientFactory, Type configurationType)
		{
			this.winScpClientFactory = winScpClientFactory ?? throw new ArgumentNullException(nameof(winScpClientFactory));
			this.configurationType = configurationType ?? throw new ArgumentNullException(nameof(configurationType));
		}

		public override async Task EndpointTask(CancellationToken cancelToken)
		{
			var endpointActivityId = GetNewActivityId();
			Logger.Log(endpointActivityId, LogLevel.Debug, "Starting endpoint task.");
			Config = (WinScpReceiveConfiguration)Activator.CreateInstance(configurationType, ConfigXml);
			var pollingStagger = (int)(new Random().NextDouble() * Math.Min(Config.PollingIntervalMs, 60000));
			var startMaxDelay = Math.Min(60000, Config.PollingIntervalMs);
			var timeToNextPoll = (int)(startMaxDelay - ((DateTimeOffset.UtcNow.TimeOfDay.TotalMilliseconds + 60000 - pollingStagger) % startMaxDelay));
			Logger.Log(endpointActivityId, LogLevel.Debug, "Polling interval is {0:N0} seconds with a stagger of {1} seconds.", Config.PollingIntervalMs / 1000, pollingStagger / 1000);

			while (!cancelToken.IsCancellationRequested)
			{
				Logger.Log(endpointActivityId, LogLevel.Debug, "Waiting {0:N0} seconds for next polling time at {1:s}.", timeToNextPoll / 1000, DateTime.Now.AddMilliseconds(timeToNextPoll));
				await Task.Delay(timeToNextPoll, cancelToken);

				if (!string.IsNullOrWhiteSpace(Config.RegistrationConnectionStringName) && !string.IsNullOrWhiteSpace(Config.RegistrationType))
				{
					try
					{
						await RefreshClientRegistrationLocationsAsync(endpointActivityId, cancelToken);
					}
					catch (Exception ex)
					{
						Logger.Log(endpointActivityId, LogLevel.Error, "Encountered error when trying to refresh Locations from client registration", ex);
						continue;
					}
				}

				cancelToken.ThrowIfCancellationRequested();
				Logger.Log(endpointActivityId, LogLevel.Debug, "Starting receive polling for {0} location(s) with {1} concurrent process(es).",
					Config.Locations.Count, Config.MaximumConcurrentDownloads);

				try
				{
					var locationSemaphore = new SemaphoreSlim(Config.MaximumConcurrentDownloads);
					var startedLocations = Config.Locations.Select(async (location, idx) =>
					{
						var locnActivityId = $"{endpointActivityId}+{idx:D4}";
						await locationSemaphore.WaitAsync(cancelToken);
						try
						{
							Logger.Log(locnActivityId, LogLevel.Debug, "Starting download and submit process [{0}/{1}] for location: {2}", idx + 1, Config.Locations.Count, location);
							await DownloadAndSubmitLocation(locnActivityId, location, cancelToken);
						}
						catch (OperationCanceledException) { }
						catch (Exception ex)
						{
							TransportReceiver.Handler.LogInterfaceError(Logger, locnActivityId, "Error in receive location '{0}' for location {1}", ex, PortName, location);
						}
						finally
						{
							Logger.Log(locnActivityId, LogLevel.Debug, "Finished download and submit process [{0}/{1}] for location: {2}", idx + 1, Config.Locations.Count, location);
							locationSemaphore.Release();
						}
					}).ToList();

					await Task.WhenAll(startedLocations);
					startedLocations.Clear();
					timeToNextPoll = (int)(Config.PollingIntervalMs - ((DateTimeOffset.UtcNow.TimeOfDay.TotalMilliseconds + Config.PollingIntervalMs + pollingStagger) % Config.PollingIntervalMs));
				}
				catch (OperationCanceledException) { }
				Logger.Log(endpointActivityId, LogLevel.Debug, "Finished receive polling process.");
			}
		}

		internal virtual async Task DownloadAndSubmitLocation(string locnActivityId, WinScpLocation location, CancellationToken cancelToken)
		{
			using (var receiverEndpointLocation = new WinScpReceiverEndpointLocation(this, locnActivityId, location, cancelToken))
			{
				await receiverEndpointLocation.DownloadAndSubmitLocation();

				cancelToken.ThrowIfCancellationRequested();
				if (Config.ReceiveProcessingTimeWarning > 0
					&& receiverEndpointLocation.DownloadTime.TotalSeconds > Config.ReceiveProcessingTimeWarning)
				{
					var exceptionMessage = $"{nameof(TimeSpan.TotalSeconds)}: {receiverEndpointLocation.DownloadTime.TotalSeconds} exceeded {nameof(WinScpReceiveConfiguration.ReceiveProcessingTimeWarning)}: {Config.ReceiveProcessingTimeWarning}";
					await IssueManager.ReportToIssueManagerAsync(locnActivityId, $"{PortName} ReceiveProcessingTimeWarning exceeded the limit", new Exception(exceptionMessage), Logger, cancelToken);
				}

				cancelToken.ThrowIfCancellationRequested();
				if ((Config.DownloadExcludedFilesLimit > 0
					&& (receiverEndpointLocation.EmptyFileCount + receiverEndpointLocation.SkippedFileCount)
						> Config.DownloadExcludedFilesLimit))
				{
					var exceptionMessage = $@"These polling states
{nameof(WinScpReceiverEndpointLocation.EmptyFileCount)}: {receiverEndpointLocation.EmptyFileCount},
{nameof(WinScpReceiverEndpointLocation.SkippedFileCount)}: {receiverEndpointLocation.SkippedFileCount}
exceeded the condition
{nameof(WinScpReceiveConfiguration.DownloadExcludedFilesLimit)}:{Config.DownloadExcludedFilesLimit}";
					await IssueManager.ReportToIssueManagerAsync(locnActivityId, $"{PortName} Excluded files exceeded the limit", new Exception(exceptionMessage), Logger, cancelToken);
				}
			}
		}

		private async Task RefreshClientRegistrationLocationsAsync(string activityId, CancellationToken cancelToken)
		{
			var multipleLocations = new List<WinScpLocation>();

			foreach (DataRow row in await GetClientRegistrationAsync(cancelToken))
			{
				var uri = string.Empty;
				try
				{
					uri = row.Field<string>("URI");
					var password = EhubServerDecryptor.Decrypt(row.Field<string>("Password"));
					var location = new WinScpLocation(uri)
					{
						Password = password
					};
					multipleLocations.Add(location);
				}
				catch (Exception ex)
				{
					var key = $"{PortName} {Config.RegistrationType} {uri} {ex.Message}";
					await IssueManager.ReportToIssueManagerAsync(activityId, key, ex, Logger, cancelToken);
				}
			}

			Config.Locations = multipleLocations;
		}

		internal virtual async Task<IEnumerable<DataRow>> GetClientRegistrationAsync(CancellationToken cancelToken)
		{
			var list = new List<DataRow>();
			var connectionString = ConfigurationManager.ConnectionStrings[Config.RegistrationConnectionStringName].ConnectionString;
			using var con = new SqlConnection(connectionString);
			await con.OpenAsync(cancelToken);
			using var command = new SqlCommand(ClientRegistrationQuery, con);
			command.Parameters.AddWithValue("@RT_ID", Config.RegistrationType);
			using var reader = await command.ExecuteReaderAsync(cancelToken);
			while (await reader.ReadAsync(cancelToken))
			{
				var row = CustomDataTable.Value.NewRow();
				row["URI"] = reader.GetValue(0);
				row["Password"] = reader.GetValue(1);
				list.Add(row);

				cancelToken.ThrowIfCancellationRequested();
			}

			return list;
		}
	}
}

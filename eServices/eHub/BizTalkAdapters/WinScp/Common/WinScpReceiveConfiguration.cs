using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.eHub.BizTalkAdapters.Transferrer.UI;
using Microsoft.Samples.BizTalk.Adapter.Common;

namespace CargoWise.eHub.BizTalkAdapters.WinScp.Common
{
	public class WinScpReceiveConfiguration : WinScpConfiguration
	{
		[XmlIgnore]
		public List<WinScpLocation> Locations { get; set; }
		public string MultipleLocations { get; set; }
		public string MultipleLocationsCredentials { get; set; }
		public string RegistrationConnectionStringName { get; set; }
		public string RegistrationType { get; set; }
		public string Folder { get; set; }
		public string FileMask { get; set; }
		public string FlagFile { get; set; }
		public string RenameBeforeDownload { get; set; }
		public string MoveBeforeDownload { get; set; }
		public string RenameAfterDownload { get; set; }
		public string MoveAfterDownload { get; set; }
		public WinScpSortOrder SortOrder { get; set; } = WinScpSortOrder.None;
		public string EmptyFileOption { get; set; }
		[XmlElement("pollingInterval")]
		public int PollingInterval { get; set; }
		[XmlElement("pollingUnitOfMeasure")]
		public string PollingUnitOfMeasure { get; set; }
		[XmlIgnore]
		public long PollingIntervalMs { get; set; }
		public bool TransferErrorsDisablePort { get; set; } = true;
		public int TransferErrorsRetryCount { get; set; } = 10;
		public int TransferErrorsRetryInterval { get; set; } = 5;
		public int MaximumConcurrentDownloads { get; set; } = 1;
		public int ReceiveProcessingTimeWarning { get; set; } = DefaultReceiveProcessingTimeWarning;
		public int DownloadExcludedFilesLimit { get; set; } = DefaultDownloadExcludedFilesLimit;
		public IAdapterManagementUI UI { get; }

		public WinScpReceiveConfiguration() { }

		public WinScpReceiveConfiguration(XmlDocument configXml) : this(configXml, null) { }
		public WinScpReceiveConfiguration(XmlDocument configXml, IAdapterManagementUI adapterManagementUI)
			: this(configXml, "winscp", adapterManagementUI) { }

		public WinScpReceiveConfiguration(XmlDocument configXml, string scheme, IAdapterManagementUI adapterManagementUI) : base(configXml)
		{
			UI = adapterManagementUI;
			bool singleLocation = false;

			if (ConfigProperties.IfExistsExtract(configXml, "/Config/MultipleLocations", null) is string multiLocns && multiLocns.Length > 0)
			{
				var credentialLocations =
					ReadAllLines(ConfigProperties.IfExistsExtract(configXml, "/Config/MultipleLocationsCredentials",
							null))
						.Select(l => new WinScpLocation(l));
				var locations = ReadAllLines(multiLocns).Select(l => new WinScpLocation(l));
				var creds = ExtractCredentials(locations, credentialLocations);

				Locations = locations.Select(locn =>
				{
					locn.Password = creds.TryGetValue(locn.GetIdentity(), out var cred)
						? cred
						: UI?.GetPasswordPrompt($"Please enter password for '{locn}'.")
							?? throw new AdapterException("Password is required");
					return locn;
				}).ToList();
				Uri = GetMultipleLocationsUri(scheme);
				MultipleLocations = string.Join("\r\n", Locations.Select(l => l.GetUri()));
				MultipleLocationsCredentials = string.Join("\r\n", Locations.Where(l => l.Password != null)
					.Select(l => l.GetIdentity(true)).Distinct());
			}
			else if (ConfigProperties.IfExistsExtract(configXml, "/Config/RegistrationConnectionStringName", null) is string connectionStringName
						&& ConfigProperties.IfExistsExtract(configXml, "/Config/RegistrationType", null) is string registrationType
						&& connectionStringName.Length > 0 && registrationType.Length > 0)
			{
				RegistrationConnectionStringName = connectionStringName;
				RegistrationType = registrationType;
				Uri = GetMultipleLocationsUri(scheme);
			}
			else
			{
				singleLocation = true;
				var location = new WinScpLocation
				{
					Scheme = scheme,
					UserName = UserName,
					Password = Password,
					Server = Server,
					Port = Port,
					Folder = Folder = ConfigProperties.IfExistsExtract(configXml, "/Config/Folder", null),
					FileName = FileMask = ConfigProperties.IfExistsExtract(configXml, "/Config/FileMask", null)
				};
				Uri = location.GetUri();
				Locations = new List<WinScpLocation> { location };
			}

			FlagFile = ConfigProperties.IfExistsExtract(configXml, "/Config/FlagFile", null);
			RenameBeforeDownload = ConfigProperties.IfExistsExtract(configXml, "/Config/RenameBeforeDownload", null);
			MoveBeforeDownload = ConfigProperties.IfExistsExtract(configXml, "/Config/MoveBeforeDownload", null);
			RenameAfterDownload = ConfigProperties.IfExistsExtract(configXml, "/Config/RenameAfterDownload", null);
			MoveAfterDownload = ConfigProperties.IfExistsExtract(configXml, "/Config/MoveAfterDownload", null);
			SortOrder = (Enum.TryParse<WinScpSortOrder>(ConfigProperties.IfExistsExtract(configXml, "/Config/SortOrder", WinScpSortOrder.None.ToString()), out var sortOrder))
				? sortOrder : throw new AdapterException($"{nameof(SortOrder)} must be one of: {string.Join(", ", Enum.GetNames(typeof(WinScpSortOrder)))}.");
			EmptyFileOption = ConfigProperties.IfExistsExtract(configXml, "/Config/EmptyFileOption", "Ignore");
			PollingInterval = ConfigProperties.IfExistsExtractInt(configXml, "/Config/pollingInterval", 1);
			PollingUnitOfMeasure = ConfigProperties.IfExistsExtract(configXml, "/Config/pollingUnitOfMeasure", "Minutes");
			PollingIntervalMs = ConfigProperties.ExtractPollingInterval(configXml) * 1000;
			TransferErrorsDisablePort = ConfigProperties.IfExistsExtractBool(configXml, "/Config/TransferErrorsDisablePort", true);
			TransferErrorsRetryCount = ConfigProperties.IfExistsExtractInt(configXml, "/Config/TransferErrorsRetryCount", 10);
			TransferErrorsRetryInterval = ConfigProperties.IfExistsExtractInt(configXml, "/Config/TransferErrorsRetryInterval", 5);
			switch (MaximumConcurrentDownloads = ConfigProperties.IfExistsExtractInt(configXml, "/Config/MaximumConcurrentDownloads", 1))
			{
				case < 1:
					throw new AdapterException($"{nameof(MaximumConcurrentDownloads)} must be a positive integer.");

				case > 1 when singleLocation && (MoveBeforeDownload?.Length > 0 || RenameBeforeDownload?.Length > 0):
					throw new AdapterException($"{nameof(MoveBeforeDownload)} and {nameof(RenameBeforeDownload)} " +
						$"are not supported when {nameof(MaximumConcurrentDownloads)} > 1.");
			}
			ReceiveProcessingTimeWarning = ConfigProperties.IfExistsExtractInt(configXml, "/Config/ReceiveProcessingTimeWarning", DefaultReceiveProcessingTimeWarning);
			DownloadExcludedFilesLimit = ConfigProperties.IfExistsExtractInt(configXml, "/Config/DownloadExcludedFilesLimit", DefaultDownloadExcludedFilesLimit);
		}

		private Dictionary<string, string> ExtractCredentials(IEnumerable<WinScpLocation> locations, IEnumerable<WinScpLocation> credentialLocations)
		{
			var credentials = credentialLocations.ToDictionary(k => k.GetIdentity(), e => e.Password);

			foreach (var location in locations)
			{
				if (!string.IsNullOrWhiteSpace(location.Password) && !string.IsNullOrWhiteSpace(location.GetIdentity()))
				{
					credentials[location.GetIdentity()] = location.Password;
				}
			}

			return credentials;
		}

		private string GetMultipleLocationsUri(string scheme)
		{
			if (!string.IsNullOrWhiteSpace(RegistrationConnectionStringName) && !string.IsNullOrWhiteSpace(RegistrationType))
			{
				return new UriBuilder
				{
					Scheme = scheme,
					Host = "ClientRegistration",
					UserName = $"[{RegistrationType}]",
					Path = $"[{RegistrationConnectionStringName}]"
				}.Uri.ToString();
			}

			var users = Locations.Select(l => l.UserName).Distinct().ToArray();
			var servers = Locations.Select(l => (l.Server, l.Port)).Distinct().ToArray();
			var folders = Locations.Select(l => l.Folder).Distinct().ToArray();
			var fileMasks = Locations.Select(l => l.FileName).Distinct().ToArray();
			var builder = new UriBuilder
			{
				Scheme = scheme,
				UserName = users.Length == 1 ? System.Uri.EscapeDataString(users[0]) : "[MULTIPLE]"
			};
			if (servers.Length == 1)
			{
				builder.Host = servers[0].Server;
				if (servers[0].Port.HasValue)
					builder.Port = servers[0].Port.Value;
			}
			else
			{
				builder.Host = "host_multiple";
			}
			var paths = new[]
			{
				folders.Length == 1 ? folders[0] : "[MULTIPLE]",
				fileMasks.Length == 1 ? fileMasks[0] : "[MULTIPLE]"
			}.Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();
			if (paths.Length > 0)
			{
				builder.Path = string.Join("/", paths);
			}
			string uri = builder.Uri.ToString();
			return uri.Replace("host_multiple", "[MULTIPLE]");
		}

		static IEnumerable<string> ReadAllLines(string text)
		{
			using (var rdr = new StringReader(text ?? string.Empty))
			{
				while (rdr.ReadLine() is string line)
				{
					yield return line;
				}
			}
		}

		private const int DefaultReceiveProcessingTimeWarning = 300;
		private const int DefaultDownloadExcludedFilesLimit = 100;
	}
}

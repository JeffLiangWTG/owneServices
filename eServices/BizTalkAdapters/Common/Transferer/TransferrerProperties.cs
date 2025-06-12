using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using CargoWise.eServices.Encryption.Server.Decryptor;
using Common.Logging;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.Samples.BizTalk.Adapter.Common;

namespace CargoWise.eHub.BizTalkAdapters.Common
{
	public class TransferrerProperties : LoggingProperties
	{
		#region TransferrerProperties
		public string Scheme { get; set; }
		public string Uri { get; set; }
		public string Server { get; set; }
		public int Port { get; set; }
		public string User { get; set; }
		public string Password { get; set; }
		public int Timeout { get; set; }
		public string Folder { get; set; }
		public string FlagFile { get; set; }
		public bool UseContextConfiguration { get; set; }
		public TransferrerProperties() { }

		public TransferrerProperties(string uri)
		{
			Uri = uri;
			Scheme = uri.Remove(uri.IndexOf(':'));
		}

		public override void ReadLocationConfiguration(XmlDocument configDOM, string portName, CancellationToken cancelToken = default)
		{
			base.ReadLocationConfiguration(configDOM, portName);

			Server = IfExistsExtract(configDOM, "Config/Server", null);
			Port = IfExistsExtractInt(configDOM, "Config/Port", 0);
			User = IfExistsExtract(configDOM, "Config/User", null);
			Password = IfExistsExtract(configDOM, "Config/Password", null);
			Timeout = IfExistsExtractInt(configDOM, "Config/Timeout", 90000);
			Folder = IfExistsExtract(configDOM, "Config/Folder", String.Empty);
			FlagFile = IfExistsExtract(configDOM, "Config/FlagFile", String.Empty);
		}

		public override void ReadConfiguration(IBaseMessage message, XmlDocument configDOM, string portName)
		{
			base.ReadConfiguration(message, configDOM, portName);

			Server = ReadStringContextFallbackToLocation("Server", message, configDOM, null);
			Port = ReadIntContextFallbackToLocation("Port", message, configDOM, 0);
			User = ReadStringContextFallbackToLocation("User", message, configDOM, null);
			Password = ReadStringContextFallbackToLocation("Password", message, configDOM, null);
			Timeout = ReadIntContextFallbackToLocation("Timeout", message, configDOM, 90000);

			Folder = ReadStringContextFallbackToLocation("Folder", message, configDOM, String.Empty);
			FlagFile = ReadStringContextFallbackToLocation("FlagFile", message, configDOM, String.Empty);
		}

		protected string ReadStringContextFallbackToLocation(string propertyName, IBaseMessage message, XmlDocument configDOM, string defaultValue)
		{
			if (UseContextConfiguration)
			{
				var value = (string)message.Context.Read(propertyName, ContextConfigurationPropertyNamespace);
				if (!string.IsNullOrWhiteSpace(value)) return value;
			}

			return IfExistsExtract(configDOM, GetPath(propertyName), defaultValue);
		}

		protected int ReadIntContextFallbackToLocation(string propertyName, IBaseMessage message, XmlDocument configDOM, int defaultValue)
		{
			if (UseContextConfiguration)
			{
				var value = message.Context.Read(propertyName, ContextConfigurationPropertyNamespace);
				if (value != null)
				{
					int result = 0;
					if (value is string && Int32.TryParse((string)value, out result))
					{
						return result;
					}
					else if (value is int)
					{
						return (int)value;
					}
				}
			}

			return IfExistsExtractInt(configDOM, GetPath(propertyName), defaultValue);
		}

		protected bool ReadBoolContextFallbackToLocation(string propertyName, IBaseMessage message, XmlDocument configDOM, bool defaultValue)
		{
			if (UseContextConfiguration)
			{
				var value = message.Context.Read(propertyName, ContextConfigurationPropertyNamespace);
				if (value != null) return (bool)value;
			}

			return IfExistsExtractBool(configDOM, GetPath(propertyName), defaultValue);
		}

		string GetPath(string propertyName)
		{
			return "Config/" + propertyName;
		}

		public const string ContextConfigurationPropertyNamespace = "http://cargowise.com/ehub/biztalkadapters/transferrer-properties";

		#endregion TransferrerProperties

		#region Receive

		public interface IReceiveFactory
		{
			Receive Create(string uri);
		}

		public class ReceiveFactory : IReceiveFactory
		{
			public Receive Create(string uri) => new Receive(uri);
		}

		public class Receive : TransferrerProperties
		{
			public string FileMask { get; private set; }
			public bool ServerSideFiltering { get; private set; }
			public string SortOrder { get; private set; }
			public int PollingInterval { get; private set; }
			public string EmptyFileOption { get; private set; }
			public string MoveBeforeDownload { get; private set; }
			public string RenameBeforeDownload { get; private set; }
			public string MoveAfterDownload { get; private set; }
			public string RenameAfterDownload { get; private set; }
			public Location SingleLocation { get; private set; }
			public List<Location> MultipleLocations { get; private set; }
			public int ReceiveProcessingTimeWarning { get; set; }
			public int DownloadExcludedFilesLimit { get; set; }
			public int MaximumConcurrentDownloads { get; set; }
			public int DownloadRetries { get; set; }
			public int DownloadRetryDelay { get; set; }
			public string connectionStringName { get; set; }
			public string registrationType { get; set; }
			public ConcurrentDictionary<string, string> exceptionStrings { get; private set; }

			public Receive(string uri)
				: base(uri)
			{
				exceptionStrings = new ConcurrentDictionary<string, string>();
			}

			public override void ReadLocationConfiguration(XmlDocument configDOM, string portName, CancellationToken cancelToken = default)
			{
				base.ReadLocationConfiguration(configDOM, portName);

				FileMask = IfNotEmptyExtract(configDOM, "Config/FileMask", false, "*");
				ServerSideFiltering = IfExistsExtractBool(configDOM, "Config/ServerSideFiltering", false);
				SortOrder = IfExistsExtract(configDOM, "Config/SortOrder", "None");
				ReceiveProcessingTimeWarning = IfExistsExtractInt(configDOM, "Config/ReceiveProcessingTimeWarning", 300);
				DownloadExcludedFilesLimit = IfExistsExtractInt(configDOM, "Config/DownloadExcludedFilesLimit", 100);
				PollingInterval = GetTimeInterval(IfExistsExtractInt(configDOM, "Config/PollingInterval", 1), IfExistsExtract(configDOM, "Config/PollingUnit", "Minutes"));
				EmptyFileOption = IfExistsExtract(configDOM, "Config/EmptyFileOption", "Ignore");
				MoveBeforeDownload = IfExistsExtract(configDOM, "Config/MoveBeforeDownload", String.Empty);
				RenameBeforeDownload = IfExistsExtract(configDOM, "Config/RenameBeforeDownload", String.Empty);
				MoveAfterDownload = IfExistsExtract(configDOM, "Config/MoveAfterDownload", String.Empty);
				RenameAfterDownload = IfExistsExtract(configDOM, "Config/RenameAfterDownload", String.Empty);
				MaximumConcurrentDownloads = IfExistsExtractInt(configDOM, "Config/MaximumConcurrentDownloads", 1);
				DownloadRetries = IfExistsExtractInt(configDOM, "Config/DownloadRetries", 0);
				DownloadRetryDelay = IfExistsExtractInt(configDOM, "Config/DownloadRetryDelay", 0);
				connectionStringName = IfExistsExtract(configDOM, "Config/ConnectionStringName", null);
				registrationType = IfExistsExtract(configDOM, "Config/RegistrationType", null);
				string multiLocns = IfExistsExtract(configDOM, "Config/MultipleLocations", null);
				string multiLocnsCreds = IfExistsExtract(configDOM, "Config/MultipleLocationsCredentials", null);
				if (cancelToken.IsCancellationRequested) return;
				try
				{
					if (!string.IsNullOrWhiteSpace(connectionStringName) && !string.IsNullOrWhiteSpace(registrationType))
					{
						var task = DoWithRetriesAsync((token) => GetMultipleLocationsFromClientRegistration(connectionStringName, registrationType, token), Logger, cancelToken);
						task.Wait(cancelToken);
						MultipleLocations = task.Result;
					}
					else if (!string.IsNullOrWhiteSpace(multiLocns) && !string.IsNullOrWhiteSpace(multiLocnsCreds))
					{
						MultipleLocations = GetMultipleLocationsFromConfig(multiLocns, multiLocnsCreds, cancelToken);
					}
					else
					{
						SingleLocation = new Location
						{
							Uri = Uri,
							UserName = User,
							Password = Password,
							Server = Server,
							Port = Port,
							Folder = Folder,
							FileMask = FileMask
						};
					}
				}
				catch (OperationCanceledException)
				{
					Logger.Debug($"ReadLocationConfiguration operation has been cancelled by the user.");
				}
				catch (AggregateException ex) when (ex.InnerException is OperationCanceledException)
				{
					Logger.Debug($"ReadLocationConfiguration operation has been cancelled by the user.");
				}
				catch (Exception ex)
				{
					Logger.Debug($"An error has occurred while trying to read the location configuration. Exception: {ex}.");
					TransferrerHelpers.ReportToIssueManager(this, this.Logger, "Error occurred when reading location configuration.", ex.Message, this, Uri, ex, cancelToken);
				}
			}

			internal static int GetTimeInterval(int interval, string units)
			{
				switch (units.ToUpperInvariant())
				{
					case "SECONDS":
						return interval * 1000;
					case "MINUTES":
						return interval * 1000 * 60;
					case "HOURS":
						return interval * 1000 * 60 * 60;
					default:
						return 0;
				}
			}

			internal async Task<List<Location>> GetMultipleLocationsFromClientRegistration(string connectionStringName, string registrationType, CancellationToken cancelToken)
			{
				var multipleLocations = new List<Location>();
				var connectionString = ConfigurationManager.ConnectionStrings[connectionStringName]?.ConnectionString;
				using (var connection = new SqlConnection(connectionString))
				{
					await connection.OpenAsync(cancelToken);
					using (var command = connection.CreateCommand())
					{
						command.CommandType = CommandType.Text;
						command.CommandText = @"SELECT CX_Attr1, CX_Password1
												FROM eHubClientRegistration AS Registration
												JOIN eHubRegistrationType AS RegistrationType ON RT_PK = CX_RT
												WHERE RT_ID = @RT_ID";
						command.Parameters.Add(new SqlParameter("@RT_ID", SqlDbType.NVarChar, 50) { Value = registrationType });

						using (var reader = await command.ExecuteReaderAsync(cancelToken))
						{
							while (await reader.ReadAsync(cancelToken))
							{
								try
								{
									var location = ReadLocation(reader);
									multipleLocations.Add(location);
								}
								catch (Exception ex)
								{
									await TransferrerHelpers.ReportToIssueManagerAsync(this, this.Logger, "Invalid Credentials", ex.Message, this, Uri, ex, cancelToken);
									continue;
								}
							}
						}
					}
				}
				return multipleLocations;
			}

			internal List<Location> GetMultipleLocationsFromConfig(string multiLocns, string multiLocnsCreds, CancellationToken cancelToken)
			{
				cancelToken.ThrowIfCancellationRequested();
				var credentials = GetCredentials(multiLocnsCreds);
				var multipleLocations = new List<Location>();
				using (var rdr = new StringReader(multiLocns))
				{
					string line;
					Location location;
					while ((line = rdr.ReadLine()) != null)
					{
						cancelToken.ThrowIfCancellationRequested();
						try
						{
							location = TryParseUri(line);
						}
						catch (Exception ex)
						{
							TransferrerHelpers.ReportToIssueManager(this, this.Logger, "Invalid Credentials", ex.Message, this, line, ex, cancelToken);
							continue;
						}
						location.Password = credentials[line.Split(new[] { '/', '\\' })[2]];
						multipleLocations.Add(location);
					}
				}
				return multipleLocations;
			}

			internal Location ReadLocation(IDataReader reader)
			{
				var uri = reader["CX_Attr1"] ?? throw new AdapterException("The credential uri is null.");
				var location = TryParseUri(uri.ToString());
				var password = reader["CX_Password1"] ?? throw new AdapterException("The credential password is null.");
				location.Password = EhubServerDecryptor.Decrypt(password.ToString());
				return location;
			}


			internal Location TryParseUri(string line)
			{
				string uriPattern = @"^(?<scheme>.+)://" +
										@"(?<user>.+)@" +
										@"(?<server>.+):" +
										@"(?<port>\d+)" +
										@"([/\\](?<path>.*))?$";
				var locn = new Location();
				var match = Regex.Match(line, uriPattern);
				if (!match.Success) throw new AdapterException(String.Format("The credential {0} is not a valid URI.", line));
				locn.Uri = line;
				if (match.Groups["scheme"].Success)
				{
					if (!validSchemes(match.Groups["scheme"].Value)) throw new AdapterException(String.Format("The credential {0} has a invalid scheme.", line));
				}
				else
				{
					throw new AdapterException(String.Format("The credential {0} does not contain a scheme.", line));
				}
				if (match.Groups["user"].Success)
				{
					if (!TransferrerHelpers.HasValidCharacters(match.Groups["user"].Value)) throw new AdapterException(String.Format("The credential username {0} has invalid characters.", line));
					locn.UserName = System.Uri.UnescapeDataString(match.Groups["user"].Value);
				}
				else 
				{
					throw new AdapterException(String.Format("The credential {0} does not contain a username.", line));
				}
				if (match.Groups["server"].Success)
				{
					if (!TransferrerHelpers.HasValidCharacters(match.Groups["server"].Value)) throw new AdapterException(String.Format("The credential server {0} has invalid characters.", line));
					locn.Server = match.Groups["server"].Value;
				}
				else
				{
					throw new AdapterException(String.Format("The credential {0} does not contain a server name.", line));
				}
				if (match.Groups["port"].Success)
				{
					locn.Port = int.Parse(match.Groups["port"].Value);
				}
				else
				{
					throw new AdapterException(String.Format("The credential {0} does not contain a port number.", line));
				}

				string path = match.Groups["path"].Value;

				string lastPart = path.Split(new[] { '/', '\\' }).Last();
				string folder;
				if (lastPart.Contains('*'))
				{
					locn.FileMask = lastPart;
					folder = path.Remove(path.Length - lastPart.Length - 1);
				}
				else
				{
					folder = path;
				}
				if (!TransferrerHelpers.HasValidCharacters(folder, "/")) throw new AdapterException(String.Format("The credential folder {0} has invalid characters.", line));
				locn.Folder = folder; 
				return locn;
			}

			internal static Dictionary<string, string> GetCredentials(string multiLocnsCreds)
			{
				var creds = new Dictionary<string, string>();
				using (var rdr = new StringReader(multiLocnsCreds))
				{
					string line;
					while ((line = rdr.ReadLine()) != null)
					{
						string ident = Regex.Replace(line, @"\:(.*)\@", "@", RegexOptions.Compiled);
						string pwd = System.Uri.UnescapeDataString(Regex.Match(line, @"\:(.*)\@", RegexOptions.Compiled).Groups[1].Value);
						creds[ident] = pwd;
					}
				}
				return creds;
			}

			public static bool validSchemes(string scheme)
			{
				var validSchemes = new List<string> { "sftpex", "ftpex", "ftpwinscp" };
				return validSchemes.Contains(scheme);
			}

			public async Task<T> DoWithRetriesAsync<T>(Func<CancellationToken, Task<T>> action, ILog logger, CancellationToken cancelToken)
			{
				while (true)
				{
					cancelToken.ThrowIfCancellationRequested();
					try
					{
						var result = await action(cancelToken);
						exceptionStrings.Clear();
						retryInterval = 0;
						return result;
					}
					catch (SqlException sqlException) when (IsTransientError(sqlException.Number))
					{
						logger.Debug($"An error has occurred when getting client registrations. The function will retry immediately. Exception: {sqlException}");
					}
					catch (Exception ex) when (!(ex is OperationCanceledException) && !cancelToken.IsCancellationRequested)
					{
						if (!exceptionStrings.ContainsKey(ex.Message))
						{
							exceptionStrings.TryAdd(ex.Message, null);
							await TransferrerHelpers.ReportToIssueManagerAsync(this, Logger, "Client Registration database error", ex.Message, this, Uri, ex, cancelToken);
						}
						retryInterval = GetRetryInterval(retryInterval);
						logger.Debug($"An error has occurred when getting client registrations. The function will retry after {retryInterval} milliseconds. Exception: {ex}");
						await Task.Delay(retryInterval, cancelToken);
					}
				}
			}

			public int GetRetryInterval(int retryInterval)
			{
				if (retryInterval < 10000)
				{
					retryInterval += 1000;
				}
				else if (retryInterval < 30000)
				{
					retryInterval += 5000;
				}
				else if (retryInterval < 60000)
				{
					retryInterval += 10000;
				}
				else
				{
					retryInterval = 60000;
				}
				return retryInterval;
			}

			public static bool IsTransientError(int errorCode)
			{
				return !NonTransientErrors.Contains(errorCode);
			}

			public class Location
			{
				public string Uri;
				public string UserName;
				public string Password;
				public string Server;
				public int Port;
				public string Folder;
				public string FileMask;
			}

			internal virtual IDbConnection CreateConnection(string connectionString)
			{
				return new SqlConnection(connectionString);
			}
		}

		

		#endregion Receive

		#region Transmit
		public class Transmit : TransferrerProperties
		{
			public string TargetFileName { get; private set; }
			public string TemporaryFolder { get; private set; }
			public string TemporaryFileName { get; private set; }
			public int ConnectionLimit { get; private set; }
			public bool KeepAlive { get; private set; }

			public Transmit(IBaseMessage message, string propertyNamespace, string uri)
				: base(uri)
			{
				IBaseMessageContext context = message.Context;
				string config = (string)context.Read("AdapterConfig", propertyNamespace);

				if (config != null)
				{
					var portName = (string)context.Read("SPName", "http://schemas.microsoft.com/BizTalk/2003/system-properties");
					var configDom = new XmlDocument();
					configDom.LoadXml(config);

					ReadConfiguration(message, configDom, portName);
				}
				else
					throw new NotImplementedException("This adapter does not support dynamic sends.");
			}

			public override void ReadConfiguration(IBaseMessage message, XmlDocument configDOM, string portName)
			{
				UseContextConfiguration = IfExistsExtractBool(configDOM, "Config/UseContextConfiguration", false);

				base.ReadConfiguration(message, configDOM, portName);
				TargetFileName = ReadStringContextFallbackToLocation("TargetFileName", message, configDOM, "%MessageID%.xml");
				TemporaryFolder = ReadStringContextFallbackToLocation("TemporaryFolder", message, configDOM, String.Empty);
				TemporaryFileName = ReadStringContextFallbackToLocation("TemporaryFileName", message, configDOM, String.Empty);

				ConnectionLimit = ReadIntContextFallbackToLocation("ConnectionLimit", message, configDOM, 0);

				if (UseContextConfiguration)
				{
					KeepAlive = false;
				}
				else
				{
					KeepAlive = ReadBoolContextFallbackToLocation("KeepAlive", message, configDOM, true);
				}
			}
		}

		#endregion Transmit

		#region Constants
		public const int TERMINATE_WAIT_LIMIT = 30000;
		#endregion Constants
		
		public static List<int> NonTransientErrors = new List<int> { 547, 2601, 2627, 8152, 50000 };
		public int retryInterval = 0;
	}
}
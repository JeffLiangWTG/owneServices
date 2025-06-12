using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;
using CargoWise.eHub.BizTalkAdapters.Common.UI;
using Microsoft.BizTalk.Adapter.Framework;
using Microsoft.Samples.BizTalk.Adapter.Common;

namespace CargoWise.eHub.BizTalkAdapters.Common
{
	public abstract class TransferrerAdapterManagement :
		AdapterManagementBase,
		IAdapterConfig,
		IAdapterConfigValidation
	{
		#region IAdapterConfig

		public abstract string GetConfigSchema(ConfigType configType);

		public Result GetSchema(string uri, string namespaceName, out string fileLocation)
		{
			fileLocation = string.Empty;
			return Result.Continue;
		}

		#endregion

		#region TransferrerAdapterManagement

		public virtual string Scheme() { return "null"; }

		public string ValidateConfiguration(ConfigType configType, string configuration)
		{
			switch (configType)
			{
				case ConfigType.TransmitHandler:
					return configuration;
				case ConfigType.TransmitLocation:
					return ValidateTransmitLocation(configuration);
				case ConfigType.ReceiveHandler:
					return configuration;
				case ConfigType.ReceiveLocation:
					return ValidateReceiveLocation(configuration);
				default:
					return String.Empty;
			}
		}

		string ValidateTransmitLocation(string configuration)
		{
			XDocument configDoc = XDocument.Parse(configuration);
			var configElem = configDoc.Element("Config");

			string folder = GetElementValue(configElem, "Folder");
			string targetFileName = GetElementValue(configElem, "TargetFileName");

			var useContextConfiguration = ToBool(GetElementValue(configElem, "UseContextConfiguration"));

			if (useContextConfiguration && ToBool(GetElementValue(configElem, "KeepAlive")))
			{
				throw new AdapterException("KeepAlive must be false when UseContextConfiguration is true");
			}

			var uri = useContextConfiguration
				? BuildUriWhenUseContextConfiguration(configElem, folder ?? "", targetFileName ?? "")
				: BuildUri(configElem, folder ?? "", targetFileName ?? "");

			AddOrUpdateElement(configElem, "uri", uri);

			return configDoc.ToString(SaveOptions.DisableFormatting);
		}

		string ValidateReceiveLocation(string configuration)
		{
			XDocument configDoc = XDocument.Parse(configuration);
			var configElem = configDoc.Element("Config");

			string uri = GetElementValue(configElem, "uri");
			if (GetElementValue(configElem, "RegistrationType") != null)
			{
				if (GetElementValue(configElem, "ConnectionStringName") == null)
					throw new AdapterException("ConnectionStringName field must not be left blank when RegistrationType is used.");
				
				uri = String.Format("{0}://{1}@{2}", Scheme(), GetElementValue(configElem, "RegistrationType"), GetElementValue(configElem, "ConnectionStringName"));
			}
			else if (GetElementValue(configElem, "MultipleLocations") != null)
			{
				var multiLocns = GetElementValue(configElem, "MultipleLocations");
				if (GetElementValue(configElem, "User") != null)
					throw new AdapterException("User field must be left blank when Multiple Locations are used.");
				if (GetElementValue(configElem, "Server") != null)
					throw new AdapterException("Server field must be left blank when Multiple Locations are used.");
				if (GetElementValue(configElem, "Folder") != null)
					throw new AdapterException("Folder field must be left blank when Multiple Locations are used.");
				if (GetElementValue(configElem, "FileMask") != null)
					throw new AdapterException("File Mask field must be left blank when Multiple Locations are used.");
				string multiLocnsCreds = GetElementValue(configElem, "MultipleLocationsCredentials");
				if (multiLocnsCreds != "******")
					uri = ValidateMultipleLocations(configElem, multiLocns, multiLocnsCreds);
			}
			else
			{
				string folder = GetElementValue(configElem, "Folder");
				string fileMask = GetElementValue(configElem, "FileMask");
				uri = BuildUri(configElem, folder ?? "", fileMask ?? "");
			}
			AddOrUpdateElement(configElem, "uri", uri);

			int? pollingInterval = TryParseNullableInt(GetElementValue(configElem, "PollingInterval"));
			string pollingUnit = GetElementValue(configElem, "PollingUnit");
			string renameBefore = GetElementValue(configElem, "RenameBeforeDownload");
			string renameAfter = GetElementValue(configElem, "RenameAfterDownload");

			if (pollingInterval.GetValueOrDefault() <= 0)
				throw new AdapterException("Polling Interval is required and must be a positive integer.");
			if (pollingUnit == null)
				throw new AdapterException("Polling Unit is required.");
			if (renameBefore != null && !Regex.IsMatch(renameBefore, @"\{[fnx]\}"))
				throw new AdapterException("File name placeholder missing in Rename Before Download.");
			if (renameAfter != null && !Regex.IsMatch(renameAfter, @"\{[fnx]\}"))
				throw new AdapterException("File name placeholder missing in Rename After Download.");

			int? maxConcurrentDownloads = TryParseNullableInt(GetElementValue(configElem, "MaximumConcurrentDownloads"));
			string moveBeforeDownload = GetElementValue(configElem, "MoveBeforeDownload");

			if (maxConcurrentDownloads != null && (maxConcurrentDownloads < 1 || maxConcurrentDownloads > 1023))
				throw new AdapterException("Maximum Concurrent Downloads should be set to a value between 1 and 1023.");
			if (maxConcurrentDownloads != null && maxConcurrentDownloads > 1 && !string.IsNullOrEmpty(moveBeforeDownload))
				throw new AdapterException("Move Before Download cannot be used while Maximum Concurrent Downloads is greater than 1.");

			int? downloadRetries = TryParseNullableInt(GetElementValue(configElem, "DownloadRetries"));
			if (downloadRetries != null && downloadRetries < 0)
				throw new AdapterException("Download Retries should be 0 or a positive integer.");

			int? downloadRetryDelay = TryParseNullableInt(GetElementValue(configElem, "DownloadRetryDelay"));
			if (downloadRetryDelay != null && downloadRetryDelay < 0)
				throw new AdapterException("Connection Retry Delay should be 0 or a positive integer.");

			return configDoc.ToString(SaveOptions.DisableFormatting);
		}

		string BuildUriWhenUseContextConfiguration(XElement configElem, string folder, string fileName)
		{
			string user;
			string server;
			int? port;
			GetElementsForUri(configElem, out user, out server, out port);
			ValidateFileName(fileName);
			var result = ComposeUri(Scheme(), FirstNonEmpty(user, "[ContextUser]"), FirstNonEmpty(server, "[ContextServer]"), (port.HasValue ? port.Value : 0), FirstNonEmpty(folder, "[ContextFolder]"), FirstNonEmpty(fileName, "[ContextTargetFileName]"));
			return result.Replace(":0", ":[ContextPort]");
		}

		string BuildUri(XElement configElem, string folder, string fileName)
		{
			string user;
			string server;
			int? port;
			GetElementsForUri(configElem, out user, out server, out port);
			ValidateUserServerPort(user, server, port);
			ValidateFileName(fileName);
			return ComposeUri(Scheme(), user, server, port.Value, folder, fileName);
		}

		void GetElementsForUri(XElement configElem, out string user, out string server, out int? port)
		{
			user = GetElementValue(configElem, "User");
			if (user != null) user = Uri.EscapeDataString(user);
			server = GetElementValue(configElem, "Server");
			if (server != null) server = Uri.EscapeDataString(server);
			port = TryParseNullableInt(GetElementValue(configElem, "Port"));
		}

		void ValidateUserServerPort(string user, string server, int? port)
		{
			if (string.IsNullOrWhiteSpace(user)) throw new AdapterException("User is required");
			if (string.IsNullOrWhiteSpace(server)) throw new AdapterException("Server is required");
			if (!port.HasValue) throw new AdapterException("Port is required");
		}

		void ValidateFileName(string fileName)
		{
			if (!string.IsNullOrWhiteSpace(fileName) && fileName.Contains("\\")) throw new AdapterException("Backslash is not supported in file names.");
		}

		string FirstNonEmpty(string input, string fallback)
		{
			if (!string.IsNullOrWhiteSpace(input)) return input;
			return fallback;
		}

		string Concatenate(string input1, string input2)
		{
			if (string.IsNullOrWhiteSpace(input2)) return string.Empty;
			return "[" + input1 + "|" + input2 + "]";
		}

		string ComposeUri(string scheme, string userName, string host, int port, string folder, string fileName)
		{
			string uri = new UriBuilder
			{
				Scheme = scheme,
				UserName = userName,
				Host = host,
				Port = port
			}.ToString();

			string path = Path.Combine(folder ?? "", fileName ?? "");
			if (!String.IsNullOrWhiteSpace(path))
				uri += path.Replace('\\', '/').TrimStart('/');
			else
				uri = uri.TrimEnd('/');

			return uri;
		}

		private string ValidateMultipleLocations(XElement configElem, string multiLocns, string multiLocnsCreds)
		{
			List<string> usernames = new List<string>();
			List<string> servers = new List<string>();
			List<string> folders = new List<string>();
			List<string> fileMasks = new List<string>();

			var oldCreds = new Dictionary<string, string>();
			var rxCreds = new Regex(@"\:(.*)\@");
			if (multiLocnsCreds != null)
				using (var sr = new StringReader(multiLocnsCreds))
				{
					string line;
					while ((line = sr.ReadLine()) != null)
					{
						if (String.IsNullOrWhiteSpace(line))
							break;
						string ident = rxCreds.Replace(line, "@");
						string pwd = rxCreds.Match(line).Groups[1].Value;
						oldCreds[ident] = pwd;
					}
				}

			var newCreds = new Dictionary<string, string>();

			var locnsBldr = new StringBuilder();
			using (var sr = new StringReader(multiLocns))
			{
				string uriPattern = @"^(?<scheme>.+?)://" +
									@"((?<user>.+?)(:(?<password>.*?))?@)?" +
									@"(?<server>[^/\\:]+)" +
									@"(:(?<port>[^/\\]+))?" +
									@"(?<path>.*?)?$";
				int count = 0;
				string line = sr.ReadLine();
				while (line != null)
				{
					count++;
					if (String.IsNullOrWhiteSpace(line))
						continue;
					var match = Regex.Match(line, uriPattern);
					if (!match.Success)
						throw new AdapterException(String.Format("Error in Multiple Locations value '{0}' at line {1}.\r\nNot a valid URI.", line, count));
					if (match.Groups["scheme"].Value != Scheme())
						throw new AdapterException(String.Format("Error in Multiple Locations value '{0}' at line {1}.\r\nURI does not match adapter's transport scheme. Should be '{2}'.", line, count, Scheme()));
					string locnUser = GetMatchValue(match, "user");
					string locnServer = GetMatchValue(match, "server");
					int? locnPort = TryParseNullableInt(GetMatchValue(match, "port"));
					string locnPwd = GetMatchValue(match, "password");
					string locnPath = match.Groups["path"].Value.Replace('\\', '/');
					if (locnUser == null)
						throw new AdapterException(String.Format("Error in Multiple Locations value '{0}' at line {1}.\r\nUser is required.", line, count));
					else
						if (!TransferrerHelpers.HasValidCharacters(locnUser))
						throw new AdapterException(String.Format("Error in Multiple Locations value '{0}' at line {1}.\r\n\r\nUser Name contains unescaped special characters.\r\n{2}", line, count, FormatInvalidCharacterEscapes(locnUser)));
					if (locnPwd != null)
						if (!TransferrerHelpers.HasValidCharacters(locnPwd))
							throw new AdapterException(String.Format("Error in Multiple Locations value '{0}' at line {1}.\r\n\r\nPassword contains unescaped special characters.\r\n{2}", line, count, FormatInvalidCharacterEscapes(locnPwd)));
					if (locnServer == null)
						throw new AdapterException(String.Format("Error in Multiple Locations value '{0}' at line {1}.\r\nServer is required.", line, count));
					if (locnPort == null)
						throw new AdapterException(String.Format("Error in Multiple Locations value '{0}' at line {1}.\r\nPort is required.", line, count));
					if (locnPath.Contains("*") && locnPath.IndexOf('*') < locnPath.LastIndexOf('/'))
						throw new AdapterException(String.Format("Error in Multiple Locations value '{0}' at line {1}.\r\nWildcards are only allowed in file masks.", line, count));
					string locnIdent = String.Format("{0}@{1}:{2}", locnUser, locnServer, locnPort);
					if (!newCreds.ContainsKey(locnIdent))
						if (locnPwd != null)
							newCreds[locnIdent] = locnPwd;
						else if (oldCreds.ContainsKey(locnIdent))
							newCreds[locnIdent] = oldCreds[locnIdent];
						else
						{
							var passwordPrompt = GetPasswordPrompt("Please enter password for '" + locnIdent + "'.");
							if (passwordPrompt.ShowDialog() == DialogResult.OK)
								newCreds[locnIdent] = Uri.EscapeDataString(passwordPrompt.Password);
							else
								throw new AdapterException("Password is required");
						}
					locnsBldr.AppendFormat("{0}://{1}{2}", Scheme(), locnIdent, locnPath).AppendLine();
					usernames.Add(locnUser);
					servers.Add(String.Format("{0}:{1}", locnServer, locnPort));
					if (locnPath.Contains("*"))
					{
						int splitPos = locnPath.LastIndexOf('/') + 1;
						folders.Add(locnPath.Remove(splitPos));
						fileMasks.Add(locnPath.Substring(splitPos));
					}
					else
					{
						folders.Add(locnPath);
						fileMasks.Add(String.Empty);
					}
					line = sr.ReadLine();
				}
			}
			AddOrUpdateElement(configElem, "MultipleLocations", locnsBldr.ToString().Trim());

			var credsBldr = new StringBuilder();
			foreach (var cred in newCreds)
				credsBldr.AppendLine(cred.Key.Insert(cred.Key.IndexOf('@'), ':' + cred.Value));
			AddOrUpdateElement(configElem, "MultipleLocationsCredentials", credsBldr.ToString().Trim());

			var uriBldr = new UriBuilder();
			uriBldr.Scheme = Scheme();
			uriBldr.UserName = usernames.Distinct().Count() > 1 ? "[MULTIPLE]" : usernames.First();
			if (servers.Distinct().Count() == 1)
			{
				var hostParts = servers.First().Split(':');
				uriBldr.Host = hostParts[0];
				uriBldr.Port = Convert.ToInt32(hostParts[1]);
			}
			else
				uriBldr.Host = String.Join("+", servers.Distinct());

			string uri = uriBldr.ToString();
			if (folders.Distinct().Count() > 1)
			{
				uri += "[MULTIPLE]";
				if (fileMasks.Distinct().Count() == 1 && fileMasks.First() != String.Empty)
					uri += ("/" + fileMasks.First());
			}
			else
			{
				uri += folders.First().TrimStart('/');
				uri += fileMasks.Distinct().Count() > 1 ? "[MULTIPLE]" : fileMasks.First();
			}

			return uri.TrimEnd('/');
		}

		private string FormatInvalidCharacterEscapes(string text)
		{
			var msg = new StringBuilder("Char:\tReplace with:").AppendLine();
			string nonEscapedText = Regex.Replace(text, @"%[0-9a-fA-F]{2}", "");
			var matches = Regex.Matches(nonEscapedText, @"[^" + validUriChars + "]");
			foreach (var match in matches.OfType<Match>().Select(m => m.Value).Distinct().OrderBy(m => m))
			{
				msg.AppendFormat("{0}\t{1}", match, Uri.EscapeDataString(match)).AppendLine();
			}
			return msg.ToString();
		}

		internal virtual PasswordPrompt GetPasswordPrompt(string prompt)
		{
			return new PasswordPrompt { Prompt = prompt };
		}

		static string GetElementValue(XElement parentElement, string elementName)
		{
			var element = parentElement.Element(elementName);
			if (element == null || String.IsNullOrWhiteSpace(element.Value))
				return null;
			else
				return element.Value;
		}

		static XElement AddOrUpdateElement(XElement parentElem, string name, string value)
		{
			var childElem = parentElem.Element(name);
			if (childElem == null)
				parentElem.Add(childElem = new XElement(name));
			childElem.Value = value;
			return childElem;
		}

		static int? TryParseNullableInt(string text)
		{
			int num;
			if (Int32.TryParse(text, out num))
				return num;
			else
				return null;
		}

		static bool ToBool(string text)
		{
			bool result;
			if (Boolean.TryParse(text, out result)) return result;

			return false;
		}

		static string GetMatchValue(Match match, string groupName)
		{
			string value = match.Groups[groupName].Value;
			if (String.IsNullOrWhiteSpace(value))
				return null;
			else
				return value;
		}

		internal virtual IDbConnection CreateConnection(string connectionString)
		{
			return new SqlConnection(connectionString);
		}

		const string validUriChars = @"!'()=\-._~a-zA-Z0-9";
		#endregion
	}
}

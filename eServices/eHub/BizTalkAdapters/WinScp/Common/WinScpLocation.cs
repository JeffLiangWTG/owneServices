using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CargoWise.eHub.BizTalkAdapters.WinScp.Common
{
	public class WinScpLocation
	{
		public string SourceUri { get; }
		public string Scheme { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }
		public string Server { get; set; }
		public int? Port { get; set; }
		public string Folder { get; set; }
		public string FileName { get; set; }
		private static readonly string[] FileMaskPattern = new[] { "*", "?" };

		public WinScpLocation() : this(string.Empty) { }

		public WinScpLocation(string uri)
		{
			SourceUri = uri;
			var match = RegexUri.Match(uri.Replace("\\", "/"));
			if (match.Groups["scheme"].Success)
				Scheme = match.Groups["scheme"].Value;
			if (match.Groups["user"].Success)
				UserName = Uri.UnescapeDataString(match.Groups["user"].Value);
			if (match.Groups["password"].Success)
				Password = Uri.UnescapeDataString(match.Groups["password"].Value);
			if (match.Groups["server"].Success)
				Server = match.Groups["server"].Value;
			if (match.Groups["port"].Success && int.TryParse(match.Groups["port"].Value, out int port))
				Port = port;
			if (match.Groups["path"].Success)
			{
				string path = match.Groups["path"].Value;
				string[] parts = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
				if (FileMaskPattern.Any(parts.Last().Contains))
				{
					FileName = parts.Last();
					if (parts.Length > 1)
						Folder = string.Join("/", parts.Take(parts.Length - 1));
				}
				else
				{
					Folder = string.Join("/", parts);
				}
			}
		}

		public WinScpLocation Clone()
		{
			return (WinScpLocation)MemberwiseClone();
		}

		public override string ToString()
		{
			return GetUri();
		}

		public string GetUri(bool includePassword = false)
		{
			var builder = new StringBuilder();
			if (!string.IsNullOrWhiteSpace(Scheme))
				builder.Append($"{Scheme}://");
			if (!string.IsNullOrWhiteSpace(UserName))
			{
				builder.Append(Uri.EscapeDataString(UserName));
				if (includePassword && !string.IsNullOrWhiteSpace(Password))
					builder.Append($":{Uri.EscapeDataString(Password)}");
				builder.Append('@');
			}
			builder.Append(Server);
			if (Port.HasValue)
				builder.Append($":{Port.Value}");
			if (!string.IsNullOrWhiteSpace(Folder) || !string.IsNullOrWhiteSpace(FileName))
			{
				builder.Append($"/{GetPath()}");
			}
			return builder.ToString();
		}

		public string GetContextUri()
		{
			var builder = new StringBuilder();
			builder.Append($"{Scheme}://[ContextUser]@[ContextServer]");
			if (Port.HasValue)
				builder.Append($":{Port.Value}");
			builder.Append("/[ContextFolder]");
			if (!string.IsNullOrWhiteSpace(FileName))
				builder.Append($"/{FileName}");
			return builder.ToString();
		}

		public string GetPath() => string.Join("/", new[] { Folder, FileName }.Where(p => !string.IsNullOrWhiteSpace(p)));

		public string GetIdentity(bool includePassword = false)
		{
			var builder = new StringBuilder();
			if (!string.IsNullOrWhiteSpace(UserName))
				builder.Append($"{Uri.EscapeDataString(UserName)}");
			if (includePassword && !string.IsNullOrWhiteSpace(Password))
				builder.Append($":{Uri.EscapeDataString(Password)}");
			builder.Append($"@{Server}");
			if (Port.HasValue)
				builder.Append($":{Port.Value}");
			return builder.ToString();
		}

		private static readonly Regex RegexUri = new Regex(@"^((?<scheme>.+)://)?((?<user>.+?)(:(?<password>.*?))?@)?(?<server>.+?)(:(?<port>\d+))?(/(?<path>.*))?$", RegexOptions.Compiled);
	}
}

using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace CargoWise.eHub.BizTalkAdapters.Transferrer.Core
{
	public class Location
	{
		public string Uri { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }
		public string Server { get; set; }
		public int? Port { get; set; }
		public string Folder { get; set; }
		public string FileMask { get; set; }

		public Location(string uri)
		{
			Uri = uri.Replace("\\", "/");

			const string uriPattern = @"^(?<scheme>.+)://" +
			@"((?<user>.+?)(:(?<password>.*?))?@)?" +
			@"(?<server>.+?)(:(?<port>\d+))?" +
			@"(/(?<path>.*))?$";

			var match = Regex.Match(Uri, uriPattern);
			if (match.Groups["user"].Success)
				UserName = match.Groups["user"].Value;
			if (match.Groups["password"].Success)
				Password = match.Groups["password"].Value;
			if (match.Groups["server"].Success)
				Server = match.Groups["server"].Value;
			if (match.Groups["port"].Success)
			{
				int port;
				if (Int32.TryParse(match.Groups["port"].Value, out port))
					Port = port;
			}
			if (match.Groups["path"].Success)
			{
				string path = match.Groups["path"].Value;
				string[] parts = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
				if (parts.Last().Contains('*'))
				{
					FileMask = parts.Last();
					if (parts.Length > 1)
						Folder = String.Join("/", parts.Take(parts.Length - 1));
				}
				else
				{
					Folder = path;
				}
			}
		}
	}
}

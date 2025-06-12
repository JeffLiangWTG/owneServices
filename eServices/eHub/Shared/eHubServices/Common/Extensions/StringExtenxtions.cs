using System;
using System.Text;

namespace CargoWise.eHub.Share.eHubServices.eHubSender.Common.Extensions
{
	public static class StringExtensions
	{
		public static string TruncateForLogging(this string message)
		{
			if (string.IsNullOrEmpty(message)) return string.Empty;
			var text = message.Length > 300 ? message.Substring(0, 300) : message;
			return text.Replace("\r", " ").Replace("\n", " ");
		}

		public static string Base64Decode(this string text)
		{
			var decodedData = Convert.FromBase64String(text);
			return Encoding.ASCII.GetString(decodedData);
		}
	}
}
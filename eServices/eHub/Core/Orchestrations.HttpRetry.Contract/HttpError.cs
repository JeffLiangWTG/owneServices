using System;
using System.Text.RegularExpressions;

namespace CargoWise.eHub.Core.Orchestrations.HttpRetry.Contract
{
	[Serializable]
	public class HttpError
	{
		private static readonly Regex HttpErrorPattern = new Regex(
			@"The remote server returned an error: \((?<code>\d{3})\)( (?<message>[\w\s]+))?\.?",
			RegexOptions.Singleline | RegexOptions.Compiled);

		public ushort StatusCode { get; set; }

		public string Message { get; set; }

		public static HttpError Parse(string exceptionMessage)
		{
			if (string.IsNullOrEmpty(exceptionMessage))
			{
				throw new ArgumentNullException("exceptionMessage");
			}

			var match = HttpErrorPattern.Match(exceptionMessage);

			if (!match.Success)
			{
				return new HttpError
				{
					StatusCode = ushort.MaxValue,
					Message = exceptionMessage
				};
			}

			return new HttpError
			{
				StatusCode = ushort.Parse(match.Groups["code"].Value),
				Message = match.Groups["message"].Value
			};
		}
	}
}

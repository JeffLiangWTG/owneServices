using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.ROSErrors.Services
{
	public sealed class DownloadROSErrorList
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
		public static (string Error, IReadOnlyDictionary<string, string> ExtractedErrorList) Download(string errorCodeListURL)
		{
			var errorText = string.Empty;
			var extractedErrorList = new Dictionary<string, string>();
			try
			{
				using (var textLoader = TextLoader.New(new Uri(errorCodeListURL), ApplicationConfig.Instance.DefaultHttpUserAgent))
				{
					var pageText = textLoader.LoadAsync().GetAwaiter().GetResult();
					ExtractErrorCodeList(extractedErrorList, pageText);
				}
			}
			catch (Exception ex)
			{
				errorText = $"Unable to Download the IE ROS Error Codes List File from the following URL: {errorCodeListURL} {Environment.NewLine} {ex.Message}";
				Console.WriteLine(errorText);
			}
			return (errorText, extractedErrorList);
		}

		static void ExtractErrorCodeList(Dictionary<string, string> extractedErrorList, string fileContents)
		{
			fileContents = Regex.Replace(fileContents, string.Format(CultureInfo.InvariantCulture, "\r?\n(?!{0})", string.Join("|", CodePrefixes)), " ");

			foreach (var line in fileContents.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).Skip(1))
			{
				var parts = line.Split((char[])null, 2, StringSplitOptions.RemoveEmptyEntries);
				extractedErrorList[Regex.Replace(parts[0], @"[^a-zA-Z0-9 -]", "")] = parts[1].Replace("\t", "").Trim();
			}
		}

		static string[] CodePrefixes => new string[]
		{
			"ROS-", "111", "ECLR", "CMPV", "MCLR", "MITP", "FFM", "ERF", "TAR", "ENQ", "ECS", "EMAN"
		};
	}
}

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.GBReferenceData.Services.CDSPortData.Config;
using CargoWise.RefDbRepo.GBReferenceData.Services.CDSPortData.Downloader;
using CargoWise.RefDbRepo.GBReferenceData.Services.CDSPortData.Models;

namespace CargoWise.RefDbRepo.GBReferenceData.Services.CDSPortData.Processing
{
	public abstract class DataProcessor
	{
		protected DataProcessor(ICDSPortSource source, IDownloadManager downloadManager)
		{
			CDSPortSource = source;
			DownloadManager = downloadManager;
		}

		protected ICDSPortSource CDSPortSource { get; private set; }
		protected IDownloadManager DownloadManager { get; private set; }

		public List<PortData> Extract()
		{
			var data = new List<PortData>();

			if (HasContent())
			{
				var fieldsList = ReadContent();
				foreach (var fields in fieldsList)
				{
					var pd = ExtractRow(fields);

					if (pd != null && !data.Any(x => x.Code == pd.Code))
					{
						data.Add(pd);
					}
				}
			}

			return data;
		}

		protected abstract bool HasContent();
		protected abstract List<string[]> ReadContent();

		PortData ExtractRow(string[] fields)
		{
			var code = GetData(fields, new[] { CDSPortSource.CodeColumn }).ToUpper(CultureInfo.CurrentCulture);
			var description = GetData(fields, CDSPortSource.DescriptionColumns);
			var addInfo = GetData(fields, new[] { CDSPortSource.AdditionalInfoColumn });

			return !Validate(code, description) ? null : new PortData
			{
				Code = code,
				Description = description,
				AdditionalInfo = addInfo,
				Source = CDSPortSource
			};
		}

		static bool Validate(string code, string description)
		{
			var valid = !string.IsNullOrEmpty(code);

			valid &= !string.IsNullOrEmpty(description);
			valid &= !code.Contains(" ");
			valid &= !code.Contains("CODE");

			return valid;
		}

		static string GetData(string[] fields, int[] keys)
		{
			var data = string.Empty;

			foreach (var k in keys)
			{
				if (k >= 0 && k < fields.Length)
				{
					data = $"{data} {fields[k]}".Trim();
				}
			}

			return data;
		}
	}
}

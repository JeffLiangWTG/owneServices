using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CargoWise.RefDbRepo.Staging.Common;

namespace CargoWise.RefDbRepo.ZAReferenceData.Services.Carriers
{
	public abstract class BaseLoader<T> : ILoader<T> where T : class
	{
		protected BaseLoader(string baseUrl)
		{
			this.baseUrl = baseUrl;
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
		}
		readonly string baseUrl;

		protected virtual IFileDownloader GetDownloader(string baseUrl, string fileName) => new FileDownloader(new Uri(Path.Combine(baseUrl, fileName)));


		public BaseData<T> LoadData()
		{
			var result = new BaseData<T>
			{
				PublicationDate = DateTime.MinValue,
				Data = new List<T>()
			};

			var fileNames = GetFileNamesToDownload();

			foreach (var fileName in fileNames)
			{
				var info = GetDataFromUrl(fileName);

				if (result.PublicationDate < info.FileDate)
				{
					result.PublicationDate = info.FileDate;
				}
				MergeData(result.Data, info.Data);
			}

			return result;
		}

		(DateTime FileDate, List<T> Data) GetDataFromUrl(string fileName)
		{
			var fileDate = DateTime.MinValue;
			var data = new List<T>();

			var downloader = GetDownloader(baseUrl, fileName);

			if (downloader != null)
			{
				fileDate = downloader.GetCreationTime();

				using (var response = downloader.GetFileStream())
				using (var stream = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(1252)))
				{
					var csvData = CSVHelper.GetCSVLinesInArrays(true, stream.ReadToEnd());
					data = ExtractCsv(fileName, csvData);
				}
			}

			return (fileDate, data);
		}

		protected virtual void MergeData(List<T> currentData, List<T> newData) => currentData.AddRange(newData);

		protected abstract List<string> GetFileNamesToDownload();
		protected abstract List<T> ExtractCsv(string fileName, List<string[]> csvData);
		
	}
}

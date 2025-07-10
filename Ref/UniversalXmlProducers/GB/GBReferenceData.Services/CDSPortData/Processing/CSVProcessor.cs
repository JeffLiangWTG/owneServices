using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.GBReferenceData.Services.CDSPortData.Config;
using CargoWise.RefDbRepo.GBReferenceData.Services.CDSPortData.Downloader;
using Microsoft.VisualBasic.FileIO;

namespace CargoWise.RefDbRepo.GBReferenceData.Services.CDSPortData.Processing
{
	public class CSVProcessor : DataProcessor
	{
		public CSVProcessor(ICDSPortSource source, IDownloadManager downloadManager) : base(source, downloadManager)
		{
		}

		string Content => content ?? (content = GetContent());
		string content;
		string GetContent()
		{
			return DownloadManager.GetCSVContent(CDSPortSource);
		}

		protected override bool HasContent() => !string.IsNullOrEmpty(Content);
		protected override List<string[]> ReadContent()
		{
			return ReadContent(Content);
		}

		static List<string[]> ReadContent(string content)
		{
			var data = new List<string[]>();

			using (var parser = new TextFieldParser(new StringReader(content)))
			{
				parser.HasFieldsEnclosedInQuotes = true;
				parser.SetDelimiters(",");

				while (!parser.EndOfData)
				{
					var currentRow = parser.ReadFields();

					data.Add(currentRow);
				}
			}

			return data;
		}
	}
}

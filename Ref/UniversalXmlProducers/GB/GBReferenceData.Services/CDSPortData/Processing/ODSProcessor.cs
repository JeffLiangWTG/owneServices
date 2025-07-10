using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.GBReferenceData.Services.CDSPortData.Config;
using CargoWise.RefDbRepo.GBReferenceData.Services.CDSPortData.Downloader;
using CargoWise.RefDbRepo.GBReferenceData.Services.Common;

namespace CargoWise.RefDbRepo.GBReferenceData.Services.CDSPortData.Processing
{
	public class ODSProcessor : DataProcessor
	{
		public ODSProcessor(ICDSPortSource source, StringBuilder errorCollector, IDownloadManager downloadManager) : base(source, downloadManager)
		{
			this.errorCollector = errorCollector;
		}

		DataTable Content => content ??= GetContent();
		DataTable content;

		DataTable GetContent()
		{
			var data = DownloadManager.GetBinaryData(CDSPortSource);
			return ODTFileHelper.GetTableFromCellContent(data, errorCollector, CDSPortSource.ODSDataTag);
		}

		protected override bool HasContent() => Content != null && Content.Rows.Count > 0;

		protected override List<string[]> ReadContent()
		{
			var rows = new List<string[]>();

			foreach (DataRow row in Content.Rows)
			{
				rows.Add(row.ItemArray.Select(x => x.ToString()).ToArray());
			}

			return rows;
		}

		readonly StringBuilder errorCollector;
	}
}

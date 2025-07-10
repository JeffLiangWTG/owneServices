using System;
using System.Data;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.GBReferenceData.Services.Common;
using CargoWise.RefDbRepo.SharedReferenceData.Business;

namespace CargoWise.RefDbRepo.GBReferenceData.Business.CDSStandingData
{
	public abstract class CommonCodeParser
	{
		protected IXmlWriter xmlWriter { get; set; }
		internal readonly IWebClientWrapper webClientWrapper;

		protected CommonCodeParser(IWebClientWrapper webWrapper)
		{
			webClientWrapper = webWrapper;
		}

		public void ExportXml(string outputPath)
		{
			ProduceData();

			var publishDate = GetPublishDate();

			xmlWriter.SetPublicationTime(publishDate);
			xmlWriter.SetUpdateType(UpdateType.Full);
			xmlWriter.SetDataSource($"CDS Standing Data {CodeType}");

			xmlWriter.SaveXml(Path.Combine(outputPath, $"GB_RefCusCodeListZZ_CDS_{CodeType}.xml"));
		}

		protected virtual IXmlWriter CreateXmlWriter() => new XmlWriter(Helper.GetRefCusCodeListWriterConfigurationWithAttributes("CDS", CodeType));

		protected abstract string CodeType { get; }
		protected abstract void ProduceData();
		protected abstract DateTime GetPublishDate();

		protected byte[] GetByteArrayFromLink(IWebpageTableToDataTable page, string anchorText)
		{
			return GetDatedByteArrayFromLink(page, anchorText).Content;
		}

		protected (DateTime LastModified, byte[] Content) GetDatedByteArrayFromLink(IWebpageTableToDataTable page, string anchorText)
		{
			var url = page.ExtractUrlFromAnchor(anchorText);
			return url == null ? (DateTime.UtcNow, new byte[0]) : webClientWrapper.GetDatedContentAsByteArray(url.OriginalString);
		}

		protected static DateTime ScrapePublishDate(IWebpageTableToDataTable page)
		{
			return ScrapePublishDate(page, DateTime.UtcNow);
		}
		protected static DateTime ScrapePublishDate(IWebpageTableToDataTable page, DateTime defaultDate)
		{
			return page.ExtractPublishDate(PublishDateXPath, defaultDate);
		}

		protected static string ConvertLineFeeds(string content)
		{
			var lines = content.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

			var result = string.Join("; ", lines.Where(x => !string.IsNullOrWhiteSpace(x)));

			return result;
		}

		static string FirstLineOnly(string content)
		{
			var lines = content.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
			return lines.FirstOrDefault();
		}

		protected static bool AdjustColumnsForMerge(DataTable tbl, string[] expectedColumns, string[] newColumns)
		{
			var success = true;
			if (tbl.Columns.Count == expectedColumns.Length)
			{
				for (int i = 0; i < expectedColumns.Length; i++)
				{
					if (FirstLineOnly(tbl.Columns[i].ColumnName) == expectedColumns[i])
					{
						tbl.Columns[i].ColumnName = newColumns[i];
					}
					else
					{
						success = false;
						break;
					}
				}
			}
			else
			{
				success = false;
			}

			return success;
		}

		const string PublishDateXPath = @"//*[@id='history']/text()[2]";
	}
}

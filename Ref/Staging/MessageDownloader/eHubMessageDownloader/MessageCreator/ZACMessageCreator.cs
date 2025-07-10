using System;
using System.Globalization;
using System.Threading;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Enterprise.Edifact;
using Enterprise.Edifact.Generic;

namespace CargoWise.RefDbRepo.Staging.eHubMessageDownloader
{
	public class ZACMessageCreator : MessageCreator
	{
		protected override SourceData GetSourceDataFromStreamTextMain(string streamText, SourceData data)
		{
			data.SDA_Source = DataSourceConstants.Source.eHubZACustomsRepositoryQueue;
			var unbSegment = GetUNB(streamText);
			var contentType = GetContentType(unbSegment);

			if (!string.IsNullOrWhiteSpace(contentType))
			{
				data.SDA_SubSource = contentType == DataSourceConstants.ContentType.ZA_ProDat ? Constants.ZATariffSubSource : Constants.ZAExchangeRateSubSource;
				data.SDA_ContentType = contentType;
				data.SDA_SourceTime = GetSourceTime(streamText);
				data.SDA_Status = StatusProvider.GetQUEStatus();
				data.SDA_CreatedTime = DateTime.UtcNow;
				Thread.Sleep(1000);
			}

			return data;
		}

		static UNBSegment GetUNB(string contentText)
		{
			Argument.NotNullOrEmpty(contentText, nameof(contentText));
			var newLine = contentText.IndexOf(Environment.NewLine, StringComparison.OrdinalIgnoreCase);
			var firstLine = contentText.Substring(0, newLine).Trim('\'');
			var result = new UNBSegment();
			result.Parse(new UNOACharacterSet(), firstLine);
			return result;
		}

		static DateTime GetSourceTime(string contentText)
		{
			Argument.NotNullOrEmpty(contentText, nameof(contentText));
			var dtmLineIndex = contentText.IndexOf("DTM+", StringComparison.OrdinalIgnoreCase);
			var endDTMLine = contentText.IndexOf(Environment.NewLine, dtmLineIndex, StringComparison.OrdinalIgnoreCase);
			var dtmLineLength = endDTMLine - dtmLineIndex + 1;
			var dtmLine = contentText.Substring(dtmLineIndex, dtmLineLength).Trim('\'');
			var dtm = new Enterprise.Edifact.D96B.Segments.DTMSegment();
			dtm.Parse(new UNOACharacterSet(), dtmLine);
			return DateTime.ParseExact(dtm.DateTimePeriod?.DateTimePeriod, "yyyyMMdd", CultureInfo.InvariantCulture);
		}

		static string GetContentType(UNBSegment unbSegment)
		{
			Argument.NotNull(unbSegment, nameof(unbSegment));
			if (unbSegment.ApplicationReference == Constants.PRODATApplicationReference)
			{
				return DataSourceConstants.ContentType.ZA_ProDat;
			}
			else if (unbSegment.ApplicationReference == Constants.GESMESApplicationReference)
			{
				return DataSourceConstants.ContentType.ZA_Gesmes;
			}
			else
			{
				return string.Empty;
			}
		}
	}
}

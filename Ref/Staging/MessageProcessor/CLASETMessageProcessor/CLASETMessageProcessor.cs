using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Staging.Schema_New;

namespace CargoWise.RefDbRepo.Staging.CLASETMessageProcessor
{
	public class CLASETMessageProcessor
	{
		public CLASETMessageProcessor(IStagingRepository staging, string outputPath)
		{
			Argument.NotNull(staging, nameof(staging));
			Argument.NotNullOrEmpty(outputPath, nameof(outputPath));

			this.staging = staging;
			this.outputPath = outputPath;
		}

		public int Process()
		{
			var updatedRecords = 0;
			var messages = GetMessages();

			foreach (var message in messages)
			{
				updatedRecords += ProcessSourceData(message);
			}

			staging.SaveChanges();
			return updatedRecords;
		}

		SourceData[] GetMessages()
		{
			return (from message in staging.Get<SourceData>()
					where message.SDA_Source == DataSourceConstants.Country.Singapore &&
					message.SDA_SubSource == DataSourceConstants.SubSource.CLASET &&
					(message.SDA_ContentType == DataSourceConstants.ContentType.Edifact || message.SDA_ContentType == DataSourceConstants.ContentType.XML) &&
					message.SDA_Status == StatusProvider.GetQUEStatus()
					orderby message.SDA_CreatedTime
					select message).ToArray();
		}

		int ProcessSourceData(SourceData sourceData)
		{
			Argument.NotNull(sourceData, nameof(sourceData));

			var messageText = sourceData.SDA_ContentText.Trim();

			var processor = sourceData.SDA_ContentType == DataSourceConstants.ContentType.Edifact
				? (BaseMessageProcessor)new EDIFACTMessageProcessor(outputPath)
				: new XMLMessageProcessor(outputPath);

			return processor.Process(sourceData, messageText);
		}

		readonly IStagingRepository staging;
		readonly string outputPath;
	}
}

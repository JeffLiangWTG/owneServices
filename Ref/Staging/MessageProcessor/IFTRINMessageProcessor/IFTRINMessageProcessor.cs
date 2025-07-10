using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Staging.Schema_New;

namespace CargoWise.RefDbRepo.Staging.IFTRINMessageProcessor
{
	public abstract class IFTRINMessageProcessor
	{
		protected IFTRINMessageProcessor(IStagingRepository staging, string country, string outputPath)
		{
			Argument.NotNull(staging, nameof(staging));
			Argument.NotNullOrEmpty(country, nameof(country));
			Argument.NotNullOrEmpty(outputPath, nameof(outputPath));

			this.staging = staging;
			this.country = country;
			this.outputPath = outputPath;
		}

		public int Process()
		{
			var updatedRecords = 0;

			var messages = GetMessages();

			foreach (var message in messages)
			{
				updatedRecords += GetProcessor(message.SDA_ContentType).Process(message);
			}

			staging.SaveChanges();
			return updatedRecords;
		}

		protected abstract BaseMessageProcessor GetProcessor(string contentType);

		IEnumerable<SourceData> GetMessages()
		{
			return (from message in staging.Get<SourceData>()
					where message.SDA_Source == country &&
					message.SDA_SubSource == DataSourceConstants.SubSource.IFTRIN &&
					(message.SDA_ContentType == DataSourceConstants.ContentType.Edifact || message.SDA_ContentType == DataSourceConstants.ContentType.XML) &&
					message.SDA_Status == StatusProvider.GetQUEStatus()
					orderby message.SDA_CreatedTime
					select message);
		}

		readonly IStagingRepository staging;
		readonly string country;
		internal readonly string outputPath;
	}
}

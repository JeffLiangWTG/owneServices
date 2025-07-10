using System;
using System.Globalization;
using System.IO;
using System.ServiceModel;
using System.Threading;
using CargoWise.eHub.Adapter;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Staging.Schema_New;

namespace CargoWise.RefDbRepo.Staging.eHubMessageDownloader
{
	public class Downloader
	{
		public Downloader(IStagingRepository repository, IeHubAdapter adapter)
		{
			Argument.NotNull(repository, nameof(repository));
			Argument.NotNull(adapter, nameof(adapter));

			this.repository = repository;
			this.adapter = adapter;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types")]
		public int Run()
		{
			var count = 0;
			if (CanRetrieveMessages())
			{
				if (adapter.Inbox.Count > 0)
				{
					foreach (var message in adapter.Inbox)
					{
						var messageCreator = MessageCreator.GetMessageCreator(message.SchemaName);

						if (messageCreator == null)
						{
							Console.WriteLine($"SchemaName: {message.SchemaName} from eHubMessage is not supported");
							continue;
						}

						using (var stream = new StreamReader(message.MessageStream))
						{
							var sourceData = new SourceData();
							sourceData.SDA_PK = Guid.NewGuid();
							sourceData.SDA_Filetype = DataSourceConstants.FileType.TXT.ToString();
							sourceData.SDA_SubSource = Constants.SubSourceUnknown;
							sourceData.SDA_Status = StatusProvider.GetERRStatus();
							sourceData.SDA_Source = Constants.SourceError;
							var streamText = stream.ReadToEnd();
							sourceData.SDA_ContentText = streamText;

							try
							{
								sourceData = messageCreator.GetSourceDataFromStreamText(streamText, sourceData);
								if (sourceData.SDA_SubSource.Equals(Constants.SubSourceUnknown, StringComparison.OrdinalIgnoreCase))
								{
									Console.WriteLine($"SchemaName: {message.SchemaName}");
									Console.WriteLine($"Missing SubSource from eHubMessage, Message: {streamText}");
								}
								else
								{
									repository.Add(sourceData);
									count++;
								}
							}
							catch (Exception ex)
							{
								Console.Error.WriteLine(ex);
								Console.WriteLine($"SchemaName: {message.SchemaName}");
								Console.WriteLine($"Message: {streamText}");
							}
						}
					}
					repository.SaveChanges();
					adapter.Inbox.MarkAsRead();
				}
			}
			return count;
		}

		bool CanRetrieveMessages()
		{
			var retryTimes = Convert.ToInt32(ApplicationConfig.RetryTimes, CultureInfo.InvariantCulture);
			if (retryTimes == 0)
			{
				retryTimes = 1;
			}
			for (var retryCount = 0; retryCount < retryTimes; retryCount++)
			{
				try
				{
					adapter.RetrieveMessages();
					return true;
				}
				catch (ServerTooBusyException)
				{
					SleepOnRetry();
					continue;
				}
				catch (EndpointNotFoundException)
				{
					SleepOnRetry();
					continue;
				}
			}
			return false;
		}

		static void SleepOnRetry()
		{
			var millisecondsBetweenRetries = Convert.ToInt32(ApplicationConfig.MillisecondsBetweenRetries, CultureInfo.InvariantCulture);
			Thread.Sleep(millisecondsBetweenRetries);
		}

		readonly IStagingRepository repository;
		readonly IeHubAdapter adapter;
	}
}

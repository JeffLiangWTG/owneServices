using CargoWise.RefDbRepo.Staging.Schema_New;

namespace CargoWise.RefDbRepo.Staging.eHubMessageDownloader
{
	public abstract class MessageCreator
	{
		public static MessageCreator GetMessageCreator(string applicationCode)
		{
			MessageCreator result = null;
			switch (applicationCode)
			{
				case Constants.SupportedSchemaName.ZACustoms:
					result = new ZACMessageCreator();
					break;
				case Constants.SupportedSchemaName.GenericMessageDelivery:
					result = new GMDMessageCreator();
					break;
			}
			return result;
		}

		protected MessageCreator() { }

		public SourceData GetSourceDataFromStreamText(string streamText, SourceData data)
		{
			return GetSourceDataFromStreamTextMain(streamText, data);
		}

		protected abstract SourceData GetSourceDataFromStreamTextMain(string streamText, SourceData data);
	}
}

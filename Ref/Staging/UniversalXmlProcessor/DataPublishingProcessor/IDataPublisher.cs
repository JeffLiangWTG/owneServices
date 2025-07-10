namespace CargoWise.RefDbRepo.Staging.DataPublishingProcessor
{
	internal interface IDataPublisher
	{
		void PublishData(int commandTimeoutInSeconds);
		void PublishData(bool isPush, IMessageFactory messageFactory, int commandTimeoutInSeconds);
	}
}

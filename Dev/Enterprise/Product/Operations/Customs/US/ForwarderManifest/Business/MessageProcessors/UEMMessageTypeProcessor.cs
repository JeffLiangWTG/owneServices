namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public interface IUEMMessageTypeProcessor
	{
		bool Process(UEMEDIMessage message);
	}

	public abstract class UEMMessageTypeProcessor<T> : IUEMMessageTypeProcessor
		where T : UEMEDIMessage
	{
		bool IUEMMessageTypeProcessor.Process(UEMEDIMessage uemMessage)
		{
			var succeeded = false;
			if (uemMessage is T message)
			{
				succeeded = ProcessMessageCore(message);
			}
			return succeeded;
		}

		protected abstract bool ProcessMessageCore(T message);
	}
}

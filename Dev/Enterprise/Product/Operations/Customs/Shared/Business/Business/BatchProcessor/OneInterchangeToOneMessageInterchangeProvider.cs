using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;

namespace Enterprise.Customs.Business.BatchProcessor
{
	public abstract class OneInterchangeToOneMessageInterchangeProvider : InterchangeProviderBase
	{
		protected OneInterchangeToOneMessageInterchangeProvider(NonDependentEDIMessageCollection messages) : base(messages)
		{
		}

		protected override void BuildInterchangeBatchesCore(NonDependentEDIMessageCollection messages)
		{
			foreach (EDIMessage message in messages)
			{
				NonDependentEDIMessageCollection newBatch = new NonDependentEDIMessageCollection(messages.Factory);
				newBatch.Add(message);
				AddMessageCollection(newBatch);
			}
		}
	}
}

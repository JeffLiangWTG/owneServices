namespace Enterprise.Freight.Agency.DataTransfer.MessageProcessing.Testing
{
	sealed class CMMContainerMovementAUCountryProcessorTest : ContainerMovementAUCountryProcessorTest
	{
		public override ICMMProcessingAdapter GetAdapter()
		{
			return adapter ?? (adapter = new EdifactCMMProcessingAdapter(Message));
		}

		ICMMProcessingAdapter adapter;
	}
}

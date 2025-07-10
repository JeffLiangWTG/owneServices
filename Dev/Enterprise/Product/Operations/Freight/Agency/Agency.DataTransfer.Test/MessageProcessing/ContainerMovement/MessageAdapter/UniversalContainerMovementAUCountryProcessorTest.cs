using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.Agency.DataTransfer.MessageProcessing.Testing
{
	sealed class UniversalContainerMovementAUCountryProcessorTest : ContainerMovementAUCountryProcessorTest
	{
		public override ICMMProcessingAdapter GetAdapter()
		{
			return adapter ?? (adapter = new UniversalCMMProcessingAdapter(Factory, new UniversalEvent()));
		}

		ICMMProcessingAdapter adapter;
	}
}

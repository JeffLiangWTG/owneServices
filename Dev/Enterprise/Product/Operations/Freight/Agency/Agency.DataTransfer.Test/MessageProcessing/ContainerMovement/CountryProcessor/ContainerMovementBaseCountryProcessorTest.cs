using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Agency.Business;

namespace Enterprise.Freight.Agency.DataTransfer.MessageProcessing.Testing
{
	internal abstract class ContainerMovementBaseCountryProcessorTest<T> : TestCaseWithFactory where T : CMMBaseCountryProcessor
	{
		#region Implementation
		protected T Processor
		{
			get
			{
				return processor ?? (processor = NewProcessor());
			}
		}

		protected abstract T NewProcessor();
		T processor;
		protected CMMEmailGenerator Generator
		{
			get
			{
				return generator ?? (generator = new CMMEmailGenerator());
			}
		}

		CMMEmailGenerator generator;
		protected ContainerManagementEDIMessage Message
		{
			get
			{
				return message ?? (message = Factory.New<ContainerManagementEDIMessage>());
			}
		}

		ContainerManagementEDIMessage message;
		public abstract ICMMProcessingAdapter GetAdapter();
		#endregion
	}
}

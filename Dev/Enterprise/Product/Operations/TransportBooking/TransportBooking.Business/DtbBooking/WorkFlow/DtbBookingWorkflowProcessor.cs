using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.TransportBookings.Business
{
	public class DtbBookingWorkflowProcessor : IWrappingProcessor
	{
		public DtbBookingWorkflowProcessor(IProcessor wrappedProcessor, BusinessObjectFactory factory)
		{
			this.WrappedProcessor = wrappedProcessor;
			this.Factory = factory;
		}

		public void Process(INotifications notifications, CancellationToken token = default)
		{
			if (Factory != null)
			{
				DtbFormStateService.SetState(Factory, DtbFormState.Booking);
			}
			WrappedProcessor.Process(notifications, token);
		}

		public IProcessor WrappedProcessor { get; private set; }
		public BusinessObjectFactory Factory { get; private set; }
	}
}

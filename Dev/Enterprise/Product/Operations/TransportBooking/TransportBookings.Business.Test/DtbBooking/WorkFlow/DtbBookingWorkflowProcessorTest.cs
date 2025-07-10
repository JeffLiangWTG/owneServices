using System;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.TransportBookings.Business.Testing
{
	public class DtbBookingWorkflowProcessorTest : TestCaseWithFactory
	{
		public void TestProcess()
		{
			Action preProcessAssertion = () => AssertEquals("DtbFormState has been set to Booking", DtbFormState.Booking, DtbFormStateService.GetState(Factory));
			var wrappedProcessor = new DummyProcessor(preProcessAssertion);
			var dtbBookingWorkflowProcessor = new DtbBookingWorkflowProcessor(wrappedProcessor, Factory);
			var notifications = new NotificationCollection();

			AssertEquals("Precondition: DbFormState has not been set", DtbFormState.Parent, DtbFormStateService.GetState(Factory));
			AssertEquals("Precondition: Wrapped processor has not been run", 0, wrappedProcessor.TimesRun);
			dtbBookingWorkflowProcessor.Process(notifications);
			AssertEquals("Wrapped processor has been run", 1, wrappedProcessor.TimesRun);
		}

		public void TestProcess_FactoryNull()
		{
			Action preProcessAssertion = () => Assert("Dummy assertion", true);
			var wrappedProcessor = new DummyProcessor(preProcessAssertion);
			var dtbBookingWorkflowProcessor = new DtbBookingWorkflowProcessor(wrappedProcessor, null);
			var notifications = new NotificationCollection();

			AssertEquals("Precondition: Wrapped processor has not been run", 0, wrappedProcessor.TimesRun);
			AssertNoExceptionThrown("Doesn't throw an error on null factory", () => dtbBookingWorkflowProcessor.Process(notifications));
			AssertEquals("Wrapped processor has been run", 1, wrappedProcessor.TimesRun);
		}
	}

	class DummyProcessor : IProcessor
	{
		public DummyProcessor(Action preProcessAssertion)
		{
			this.preProcessAssertion = preProcessAssertion;
		}

		public void Process(INotifications notifications, CancellationToken token = default)
		{
			preProcessAssertion();
			TimesRun += 1;
		}

		public int TimesRun { get; set; }

		readonly Action preProcessAssertion;
	}
}

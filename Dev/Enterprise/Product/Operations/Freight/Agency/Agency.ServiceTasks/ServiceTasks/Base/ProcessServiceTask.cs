using System;
using System.Threading;
using CargoWise.EntityFramework;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.Freight.Agency.ServiceTasks
{
	public abstract class ProcessServiceTask : ServiceProviderImpl
	{
		protected ProcessServiceTask(IProcessor processor)
		{
			this.processor = processor ?? throw new ArgumentNullException(nameof(processor));
		}

		public sealed override void RunTask(CancellationToken token)
		{
			Processor.Process(ServiceLogger.GetTaskNotificationSubscriber(), token);
		}

		public IProcessor Processor
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return processor; }
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly IProcessor processor;
	}
}

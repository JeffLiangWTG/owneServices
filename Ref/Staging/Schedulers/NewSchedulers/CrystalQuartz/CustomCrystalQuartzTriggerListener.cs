using System.Threading;
using System.Threading.Tasks;
using Quartz;

namespace CargoWise.RefDbRepo.Staging.NewSchedulers.CrystalQuartz
{
	/// <summary>
	/// Custom CrystalQuartz Trigger Listener to wrap CrystalQuartz default TriggerListener and also allow changing
	/// the name of the listener to avoid conflict
	/// </summary>
	public class CustomCrystalQuartzTriggerListener : ITriggerListener
	{
		private readonly ITriggerListener _wrappedListener;
		public string Name { get; }

		public CustomCrystalQuartzTriggerListener(string name, ITriggerListener wrappedListener)
		{
			_wrappedListener = wrappedListener;
			Name = name;
		}

		public Task TriggerFired(ITrigger trigger, IJobExecutionContext context, CancellationToken cancellationToken = default)
		{
			return _wrappedListener.TriggerFired(trigger, context, cancellationToken);
		}

		public Task<bool> VetoJobExecution(ITrigger trigger, IJobExecutionContext context, CancellationToken cancellationToken = default)
		{
			return _wrappedListener.VetoJobExecution(trigger, context, cancellationToken);
		}

		public Task TriggerMisfired(ITrigger trigger, CancellationToken cancellationToken = default)
		{
			return _wrappedListener.TriggerMisfired(trigger, cancellationToken);
		}

		public Task TriggerComplete(ITrigger trigger, IJobExecutionContext context, SchedulerInstruction triggerInstructionCode, CancellationToken cancellationToken = default)
		{
			return _wrappedListener.TriggerComplete(trigger, context, triggerInstructionCode, cancellationToken);
		}
	}
}

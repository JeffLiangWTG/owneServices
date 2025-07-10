using System.Threading;
using System.Threading.Tasks;
using Quartz;

namespace CargoWise.RefDbRepo.Staging.NewSchedulers.CrystalQuartz
{
	/// <summary>
	/// Custom CrystalQuartz Job Listener to wrap CrystalQuartz default JobListener and also allow changing the name
	/// of the listener to avoid conflict
	/// </summary>
	public class CustomCrystalQuartzJobListener : IJobListener
	{
		private readonly IJobListener _wrappedListener;
		public string Name { get; }

		public CustomCrystalQuartzJobListener(string name, IJobListener wrappedListener)
		{
			_wrappedListener = wrappedListener;
			Name = name;
		}

		public Task JobToBeExecuted(IJobExecutionContext context,
			CancellationToken cancellationToken = new CancellationToken())
		{
			return _wrappedListener.JobToBeExecuted(context, cancellationToken);
		}

		public Task JobExecutionVetoed(IJobExecutionContext context,
			CancellationToken cancellationToken = new CancellationToken())
		{
			return _wrappedListener.JobExecutionVetoed(context, cancellationToken);
		}

		public Task JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException,
			CancellationToken cancellationToken = new CancellationToken())
		{
			return _wrappedListener.JobWasExecuted(context, jobException, cancellationToken);
		}
	}
}

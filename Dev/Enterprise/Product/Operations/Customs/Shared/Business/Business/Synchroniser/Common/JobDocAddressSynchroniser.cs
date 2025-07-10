using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class JobDocAddressSynchroniser : BusinessObjectSynchroniser
	{
		public JobDocAddressSynchroniser(JobDocAddress destination, JobDocAddress source)
			: base(destination, source)
		{ }

		public new JobDocAddress Destination
		{
			get { return (JobDocAddress)base.Destination; }
		}

		public new JobDocAddress Source
		{
			get { return (JobDocAddress)base.Source; }
		}

		protected override void ForceSynchroniseCore()
		{
			base.ForceSynchroniseCore();
			if (!SyncChangesDetected && !Destination.IsDeleted && !Source.IsDeleted)
			{
				if (Destination.SynchroniseWithParentWithDetection(Source, DetectEnabled) && DetectEnabled)
				{
					SyncChangesDetected = true;
					return;
				}
			}
			Destination.ReadOnly = true;
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			if (!SyncChangesDetected && !Destination.IsDeleted && !Source.IsDeleted)
			{
				if (Destination.SynchroniseWithParentWithDetection(Source, DetectEnabled, false) && DetectEnabled)
				{
					SyncChangesDetected = true;
					return;
				}
			}
			Destination.ReadOnly = true;
		}

		protected override void UnHookSynchronisers()
		{
			base.UnHookSynchronisers();
			if (!Destination.IsDeleted)
			{
				Destination.DeSynchroniseWithParent();
			}
			Destination.ReadOnly = false;
		}
	}
}

using System;
using CargoWise.Common;
using Enterprise.Integration.Freight;

namespace Enterprise.Customs.Business
{
	public class PackLineSynchronisersArg
	{
		public Func<bool> SyncEnabled;
		public Func<bool> SyncDetectEnabled;
		public Func<CusContainerCollectionSynchroniser> GetContainerSynchroniser;
		public Func<PackingSynchroniser> GetPackingSynchroniser;
		public Func<BillsSynchroniser> GetBillsSynchroniser;

		public Action RemovePackSynchFromForwardingContainers;
		public Action<PackLineSynchronisers> AddPackSynchToForwardingContainers;

		public Action RemovePackSynchFromRelevantCusEntryNums;
		public Action<PackLineSynchronisers> AddPackSynchToRelevantCusEntryNums;

		public Action CleanForConcurrency;

		public Func<bool, bool> SyncChangesDetectedForExtraSynchronisers;
	}

	public sealed class PackLineSynchronisers : IPackLineSynchronise, IDisposable
	{
		public PackLineSynchronisers(PackLineSynchronisersArg args)
		{
			this.args = args;
			Argument.NotNull(args, "args");
		}

		public bool SyncChangesDetected
		{
			get { return syncChangesDetected; }
			private set
			{
				syncChangesDetected = value;
			}
		}
		bool syncChangesDetected;

		public void Synchronise(bool forceSynchronise)
		{
			SyncChangesDetected = false;
			if (forceSynchronise)
			{
				var detectEnabled = args.SyncDetectEnabled();
				args.RemovePackSynchFromForwardingContainers();
				ContainerSynchroniser.DetectEnabled = detectEnabled;
				ContainerSynchroniser.Synchronise(true); //this should be called first before Bill & packing synchronisation
				if (detectEnabled && ContainerSynchroniser.SyncChangesDetected)
				{
					SyncChangesDetected = true;
					return;
				}
				args.AddPackSynchToForwardingContainers(this);

				args.RemovePackSynchFromRelevantCusEntryNums();

				BillsSynchroniser.DetectEnabled = detectEnabled;
				BillsSynchroniser.Synchronise(true);
				if (detectEnabled && BillsSynchroniser.SyncChangesDetected)
				{
					SyncChangesDetected = true;
					return;
				}

				args.AddPackSynchToRelevantCusEntryNums(this);

				var packingSynchroniser = args.GetPackingSynchroniser();
				packingSynchroniser.DetectEnabled = detectEnabled;
				packingSynchroniser.Synchronise();
				if (detectEnabled && packingSynchroniser.SyncChangesDetected)
				{
					SyncChangesDetected = true;
					return;
				}

				if (args.SyncChangesDetectedForExtraSynchronisers != null)
				{
					var syncChangesDetected = args.SyncChangesDetectedForExtraSynchronisers(detectEnabled);
					if (detectEnabled && syncChangesDetected)
					{
						SyncChangesDetected = true;
						return;
					}
				}
			}
		}

		#region Implementation of IPackLineSynchronise

		void IPackLineSynchronise.MarkSyncDirty()
		{
			Synchronise(args.SyncEnabled());
		}

		void IPackLineSynchronise.CleanForConcurrency()
		{
			args.CleanForConcurrency();
		}

		#endregion

		#region Implementation of IDisposable

		public void Dispose()
		{
			if (сontainerSynchroniser != null)
			{
				сontainerSynchroniser.SetEnabled(false, ContainerSynchroniser.DetectEnabled);
			}

			if (billsSynchroniser != null)
			{
				billsSynchroniser.SetEnabled(false, billsSynchroniser.DetectEnabled);
			}
		}

		#endregion

		#region Synchronisers

		CusContainerCollectionSynchroniser ContainerSynchroniser
		{
			get { return сontainerSynchroniser ?? (сontainerSynchroniser = args.GetContainerSynchroniser()); }
		}

		CusContainerCollectionSynchroniser сontainerSynchroniser;

		BillsSynchroniser BillsSynchroniser
		{
			get { return billsSynchroniser ?? (billsSynchroniser = args.GetBillsSynchroniser()); }
		}

		BillsSynchroniser billsSynchroniser;

		#endregion

		readonly PackLineSynchronisersArg args;
	}
}

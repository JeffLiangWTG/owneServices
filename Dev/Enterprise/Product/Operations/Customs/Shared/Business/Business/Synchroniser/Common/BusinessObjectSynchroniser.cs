using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public abstract class BusinessObjectSynchroniser : BaseSynchroniser
	{
		protected BusinessObjectSynchroniser(BusinessObject destination, BusinessObject source)
		{
			Destination = Argument.NotNull(destination, nameof(destination));
			Source = Argument.NotNull(source, nameof(source));
			Synchronisers = new List<ISynchroniser>();
		}

		protected sealed override void ForceSynchronise()
		{
			if (DetectEnabled && Destination.HasChanges)
			{
				ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture, "{0}.ForceSynchronise was called with Detection but Destination '{1}' already has changes.", GetType().FullName, Destination.GetType().FullName));
			}
			ForceSynchroniseCore();
		}

		protected virtual void ForceSynchroniseCore()
		{
			if (!SyncChangesDetected && Synchronisers.Any())
			{
				var isDeleted = SourceOrDestinationDeleted;
				var detectEnabled = DetectEnabled;
				foreach (var synchroniser in Synchronisers.ToArray())
				{
					if (isDeleted)
					{
						synchroniser.SetEnabled(false, synchroniser.DetectEnabled);
						Synchronisers.Remove(synchroniser);
					}
					else
					{
						synchroniser.Synchronise();
						if (detectEnabled && synchroniser.SyncChangesDetected)
						{
							SyncChangesDetected = true;
							return;
						}
					}
				}
			}
		}

		protected override void OnEnabledChanged()
		{
			base.OnEnabledChanged();
			if (Synchronisers.Any())
			{
				var isDeleted = SourceOrDestinationDeleted;
				var enabled = !isDeleted && IsEnabled;
				var detectEnabled = DetectEnabled;
				foreach (var synchroniser in Synchronisers.ToArray())
				{
					synchroniser.SetEnabled(enabled, isDeleted ? synchroniser.DetectEnabled : detectEnabled);
					if (isDeleted)
					{
						Synchronisers.Remove(synchroniser);
					}
				}
			}
		}

		protected sealed override void HookEvents()
		{
			//	if (Synchronisers.Count == 0)  // This is commented because some so-called synchronisationis done without using proper synchronisers.  e.g. JobDeclarationSynchroniser.HookOwnerRefSynchronisers() just wires event handlers. The right answer is to either turn all of those into real Synchronisers or to re-hook upon Enabling.  Then uncomment this line. 

			if (!SourceOrDestinationDeleted)
			{
				HookSynchronisers();
			}
		}

		/// <summary>
		/// Override this method to hook field (FieldSynchroniser), child objects (BusinessObjectSynchroniser) 
		/// and collections (BusinessObjectCollectionSynchroniser) synchronisers to Synchronisers collection.
		/// </summary>
		protected virtual void HookSynchronisers()
		{
		}

		protected sealed override void UnHookEvents()
		{
			UnHookSynchronisers();
		}

		protected virtual void UnHookSynchronisers()
		{
		}

		protected override void OnDetectEnabledChanged()
		{
			if (Synchronisers.Any())
			{
				var detectEnabled = DetectEnabled;
				foreach (var synchroniser in Synchronisers)
				{
					synchroniser.DetectEnabled = detectEnabled;
				}
			}
		}

		protected List<ISynchroniser> Synchronisers { get; }
		protected internal BusinessObject Destination { get; protected set; }
		protected internal BusinessObject Source { get; protected set; }

		protected void UpdateInfoEventsAndReSynchronise(FieldSynchroniser synchroniser)
		{
			synchroniser?.UpdateInfoEventsAndReSynchronise();
		}

		bool SourceOrDestinationDeleted => Destination.IsDeleted || Source.IsDeleted;
	}
}

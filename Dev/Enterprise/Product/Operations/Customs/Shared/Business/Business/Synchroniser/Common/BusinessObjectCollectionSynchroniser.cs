using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public abstract class BusinessObjectCollectionSynchroniser : BaseSynchroniser
	{
		protected BusinessObjectCollectionSynchroniser(BusinessObject parentSource, BusinessObject parentDestination)
		{
			Argument.NotNull(parentSource, "parentSource");
			Argument.NotNull(parentDestination, "parentDestination");
			Source = parentSource;
			Destination = parentDestination;
			ElementSynchronisers = new BusinessObjectSynchroniserList();
			SetEnabled(true, DetectEnabled);
#if DEBUG
			SynchroniserDetectionHelper.SetupDetection(this);
#endif
		}

		#region ForceSynchronise

		protected sealed override void ForceSynchronise()
		{
			if (!SyncChangesDetected)
			{
				var isDeleted = Destination.IsDeleted || Source.IsDeleted;
				if (!isDeleted)
				{
					HookElementSynchronisers();
#if DEBUG
					if (ElementSynchronisers.Count > 0)
					{
						SynchroniserDetectionHelper.MarkHasData(this);
					}
#endif

					if (SyncChangesDetected)
					{
						return;
					}
					ForceSynchroniseCore(isDeleted);
				}
			}
		}

		protected virtual void ForceSynchroniseCore(bool isDeleted)
		{
			foreach (var synchroniser in ElementSynchronisers.ToArray())
			{
				if (isDeleted || synchroniser.Source.IsDeleted || synchroniser.Destination.IsDeleted)
				{
					synchroniser.SetEnabled(false, synchroniser.DetectEnabled);
					ElementSynchronisers.Remove(synchroniser);
				}
				else
				{
					synchroniser.Synchronise();
					if (DetectEnabled && synchroniser.SyncChangesDetected)
					{
						SyncChangesDetected = true;
						return;
					}
				}
			}
		}

		/// <summary>
		///  Override this method to hook element synchronisers to ElementSynchronisers collection.
		/// </summary>
		protected abstract void HookElementSynchronisers();

		#endregion

		#region Hook/UnHook Events

		protected override void OnEnabledChanged()
		{
			base.OnEnabledChanged();
			var isDeleted = Destination.IsDeleted || Source.IsDeleted;
			var enabled = !isDeleted && IsEnabled;
			foreach (var synchroniser in ElementSynchronisers.ToArray())
			{
				synchroniser.SetEnabled(enabled, DetectEnabled);
				if (isDeleted)
				{
					ElementSynchronisers.Remove(synchroniser);
				}
			}
		}

		protected override void HookEvents()
		{
			collectionsToHookCountChangedEvent = null;
			foreach (var collection in CollectionsToHookAndUnHookCountChangedEvent)
			{
				if (collection != null)
				{
					collection.CountChanged -= Collection_CountChanged;
					collection.CountChanged += Collection_CountChanged;
				}
			}
		}

		protected override void UnHookEvents()
		{
			foreach (var collection in CollectionsToHookAndUnHookCountChangedEvent)
			{
				collection.CountChanged -= Collection_CountChanged;
			}
		}

		protected override void OnDetectEnabledChanged()
		{
			foreach (var synchroniser in ElementSynchronisers)
			{
				synchroniser.DetectEnabled = DetectEnabled;
			}
		}

		protected virtual void Collection_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemRemoved)
			{
				ElementSynchronisers.Remove(e.BizObject);
			}

			Synchronise();
		}

		#region CollectionsToHookAndUnHookCountChangedEvent

		protected List<BusinessObjectCollection> CollectionsToHookAndUnHookCountChangedEvent
		{
			get { return collectionsToHookCountChangedEvent ?? (collectionsToHookCountChangedEvent = GetCollectionsToHookCountChangedEvent().ToList()); }
		}
		List<BusinessObjectCollection> collectionsToHookCountChangedEvent;

		protected abstract IEnumerable<BusinessObjectCollection> GetCollectionsToHookCountChangedEvent();

		#endregion

		#endregion

		protected BusinessObject Source { get; private set; }
		protected BusinessObject Destination { get; private set; }
		protected BusinessObjectSynchroniserList ElementSynchronisers { get; private set; }
	}
}

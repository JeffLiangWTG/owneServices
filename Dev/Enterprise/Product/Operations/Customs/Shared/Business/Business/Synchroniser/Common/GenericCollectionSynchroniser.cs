namespace Enterprise.Customs.Business
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using CargoWise.EntityFramework;

	public abstract class GenericCollectionSynchroniser<T> : BusinessObjectCollectionSynchroniser
		where T : BusinessObjectSynchroniser
	{
		protected GenericCollectionSynchroniser(BusinessObject source, BusinessObject destination,
				IBusinessObjectCollection sourceCollection, IBusinessObjectCollection destinationCollection)
			: base(source, destination)
		{
			this.sourceCollection = sourceCollection;
			this.destinationCollection = destinationCollection;
			base.HookEvents();
		}
		protected readonly IBusinessObjectCollection sourceCollection;
		protected readonly IBusinessObjectCollection destinationCollection;

		protected virtual IEnumerable<BusinessObject> GetSourceBusinessObjects() => sourceCollection.OfType<BusinessObject>();

		#region Synchronise

		protected override void ForceSynchroniseCore(bool isDeleted)
		{
			base.ForceSynchroniseCore(isDeleted);
			if (!isDeleted && !SyncChangesDetected)
			{
				DeleteOrAddBOs();
			}
		}

		protected override void HookElementSynchronisers()
		{
			if (!Destination.IsDeleted)
			{
				HookSynchronisationForExistingBOs();
			}
		}

		protected override void OnDetectEnabledChanged()
		{
			if (DetectEnabled)
			{
				ReloadCollectionForDetection();
			}
			base.OnDetectEnabledChanged();
		}

		protected virtual void ReloadCollectionForDetection()
		{
			var collection = destinationCollection as BusinessObjectCollection;
			if (collection != null)
			{
				collection.Load();
			}
		}

		void HookSynchronisationForExistingBOs()
		{
			var destinationList = new List<BusinessObject>();
			foreach (BusinessObject bizo in destinationCollection)
			{
				destinationList.Add(bizo);
			}
			foreach (BusinessObject sourceBizo in GetSourceBusinessObjects())
			{
				var shouldSynchronise = ShouldSynchronise(sourceBizo);
				var synchroniser = ElementSynchronisers.FindMatchingSource<T>(sourceBizo);
				if (synchroniser != null)
				{
					if (shouldSynchronise)
					{
						synchroniser.SetEnabled(IsEnabled, DetectEnabled);
					}
					else
					{
						synchroniser.SetEnabled(false, synchroniser.DetectEnabled);
						ElementSynchronisers.Remove(synchroniser);
					}
					destinationList.Remove(synchroniser.Destination);
				}
				else if (shouldSynchronise)
				{
					BusinessObject destinationBizo = null;
					while ((destinationBizo = destinationList.FirstOrDefault(x => !x.IsDeleted && CompareBizosEqual(sourceBizo, x))) != null)
					{
						synchroniser = ElementSynchronisers.FindMatchingDestination<T>(destinationBizo);
						if (synchroniser == null)
						{
							ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(CreateNewSynchroniser(destinationBizo, sourceBizo), IsEnabled, DetectEnabled);
							destinationList.Remove(destinationBizo);
							break;
						}
						else
						{
							synchroniser.SetEnabled(IsEnabled, DetectEnabled);
							destinationList.Remove(destinationBizo);
						}
					}
				}
			}
		}

		protected abstract bool CompareBizosEqual(BusinessObject source, BusinessObject destination);
		protected abstract T CreateNewSynchroniser(BusinessObject destination, BusinessObject source);

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void DeleteOrAddBOs()
		{
			var sourceList = new List<BusinessObject>();
			foreach (BusinessObject bizo in GetSourceBusinessObjects())
			{
				if (ShouldSynchronise(bizo))
				{
					sourceList.Add(bizo);
				}
			}
			var destinationList = new List<BusinessObject>();
			foreach (BusinessObject bizo in destinationCollection)
			{
				destinationList.Add(bizo);
			}

			if (sourceList.Count > 0)
			{
				while (destinationList.Count > 0)
				{
					var existingDestinatinationBizo = destinationList[0];
					destinationList.Remove(existingDestinatinationBizo);
					var synchroniser = ElementSynchronisers.FindMatchingDestination<T>(existingDestinatinationBizo);
					if (existingDestinatinationBizo.IsDeleted || existingDestinatinationBizo.IsDeleting)
					{
						if (synchroniser != null)
						{
							ElementSynchronisers.Remove(synchroniser);
						}
					}
					else
					{
						if (synchroniser != null)
						{
							if (sourceList.Contains(synchroniser.Source))
							{
								sourceList.Remove(synchroniser.Source);
								synchroniser.Synchronise();
								if (DetectEnabled && synchroniser.SyncChangesDetected)
								{
									SyncChangesDetected = true;
									return;
								}
								continue;
							}
						}
						else
						{
							existingDestinatinationBizo = FindMatchingBizoAndAddSynchroniser(sourceList, destinationList, existingDestinatinationBizo);
							if (SyncChangesDetected)
							{
								return;
							}
						}

						if (existingDestinatinationBizo != null)
						{
							if (DetectEnabled)
							{
								SyncChangesDetected = true;
								return;
							}
							DeleteBizo(existingDestinatinationBizo);
						}
					}
				}

				if (sourceList.Count > 0)
				{
					if (DetectEnabled)
					{
						SyncChangesDetected = true;
						return;
					}
					foreach (var sourceBizo in sourceList)
					{
						var destinationBizo = destinationCollection.AddNew();
						var synchroniser = CreateNewSynchroniser(destinationBizo, sourceBizo);
						ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(synchroniser, IsEnabled, DetectEnabled);
						synchroniser.Synchronise();
						if (DetectEnabled && synchroniser.SyncChangesDetected)
						{
							SyncChangesDetected = true;
							return;
						}
					}
				}
			}
			else
			{
				if (destinationList.Count > 0)
				{
					if (DetectEnabled)
					{
						SyncChangesDetected = true;
						return;
					}
					destinationList.ForEach(bizo => DeleteBizo(bizo));
				}
			}
		}

		protected virtual bool ShouldSynchronise(BusinessObject bizo)
		{
			return true;
		}

		protected virtual void DeleteBizo(BusinessObject bizo)
		{
			bizo.Delete();
		}

		BusinessObject FindMatchingBizoAndAddSynchroniser(List<BusinessObject> sourceList, List<BusinessObject> destinationList, BusinessObject bizoToFind)
		{
			BusinessObject existingBizo = null;
			var alreadyProcessedBizos = new List<BusinessObject>();
			while ((existingBizo = sourceList.FirstOrDefault(bizo => !alreadyProcessedBizos.Contains(bizo) && CompareBizosEqual(bizo, bizoToFind))) != null)
			{
				alreadyProcessedBizos.Add(existingBizo);
				var synchroniser = ElementSynchronisers.FindMatchingSource<T>(existingBizo);
				if (synchroniser != null)
				{
					sourceList.Remove(existingBizo);
					destinationList.Remove(synchroniser.Destination);
					synchroniser.Synchronise();
					if (DetectEnabled && synchroniser.SyncChangesDetected)
					{
						SyncChangesDetected = true;
						return null;
					}
					break;
				}
				else
				{
					synchroniser = CreateNewSynchroniser(bizoToFind, existingBizo);
					ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(synchroniser, IsEnabled, DetectEnabled);
					synchroniser.Synchronise();
					if (DetectEnabled && synchroniser.SyncChangesDetected)
					{
						SyncChangesDetected = true;
						return null;
					}
					sourceList.Remove(existingBizo);
					bizoToFind = null;
					break;
				}
			}
			return bizoToFind;
		}

		/// <summary>
		/// To support ActiveBusinessObjectCollections you must override base HookEvents and UNHookEvents to hook CountChanged event and Synchronise() when change occurs
		/// </summary>
		protected override IEnumerable<BusinessObjectCollection> GetCollectionsToHookCountChangedEvent()
		{
			var collection = sourceCollection as BusinessObjectCollection;
			return collection == null ? Array.Empty<BusinessObjectCollection>() : new[] { collection };
		}

		#endregion
	}
}

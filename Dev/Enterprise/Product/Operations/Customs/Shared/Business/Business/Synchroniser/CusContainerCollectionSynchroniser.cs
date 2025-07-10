using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class CusContainerCollectionSynchroniser : BusinessObjectCollectionSynchroniser
	{
		public CusContainerCollectionSynchroniser(BaseJobDeclaration declaration)
			: base(declaration.Shipment, declaration)
		{
		}

		protected new BaseJobDeclaration Destination
		{
			get { return (BaseJobDeclaration)base.Destination; }
		}

		protected new ForwardingShipment Source
		{
			get { return (ForwardingShipment)base.Source; }
		}

		protected override void OnDetectEnabledChanged()
		{
			if (DetectEnabled)
			{
				Destination.CusContainers.Load();
			}
			base.OnDetectEnabledChanged();
		}

		#region Synchronise

		protected override void HookElementSynchronisers()
		{
			if (!Destination.JE_OverrideFreightDefaults)
			{
				SynchroniseCusContainers(GetSourceContainersToSynchronise());
			}
		}

		IEnumerable<CommonContainer> GetSourceContainersToSynchronise()
		{
			var consol = Destination.RelevantConsol;
			var containers = new List<CommonContainer>();
			if (consol != null)
			{
				foreach (var sourceShipment in Destination.GetShipmentsToSync())  // Ensure we look at all containers on all relevant shipment, e.g. containers for a secondary/child shipment
				{
					var containersForThisSourceShipmentOnThisConsol = sourceShipment.ContainersOnConsol(consol).ToArray<CommonContainer>();
					if (containersForThisSourceShipmentOnThisConsol != null)
					{
						containers.AddRange(containersForThisSourceShipmentOnThisConsol);
					}
				}
			}
			return containers.Where(x => x != null && !x.IsDeleted
						 && x.Consol != null && !x.Consol.IsDomestic()).Distinct();
		}

		void SynchroniseCusContainers(IEnumerable<CommonContainer> sourceContainers)
		{
			DeleteCusContainersIfNotInSource(sourceContainers);
			if (!SyncChangesDetected)
			{
				var sourceContainersNeedSync = sourceContainers.ToList();
				if (sourceContainersNeedSync.Count > 0)
				{
					var existingContainers = Destination.CusContainers.OfType<BaseCusContainer>().ToList();

					foreach (var sourceContainer in sourceContainers)
					{
						var existingContainer = existingContainers.FirstOrDefault(x => x.CO_JC == sourceContainer.PK);
						if (existingContainer != null)
						{
							existingContainers.Remove(existingContainer);
							sourceContainersNeedSync.Remove(sourceContainer);
							ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(new CusContainerSynchroniser(existingContainer, sourceContainer), IsEnabled, DetectEnabled);
						}
					}
					sourceContainersNeedSync.ForEach(sourceContainer =>
					{
						var cusContainer = GetOrCreateCusContainer(sourceContainer, existingContainers);
						if (SyncChangesDetected)
						{
							return;
						}
						if (cusContainer != null)
						{
							existingContainers.Remove(cusContainer);
							ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(new CusContainerSynchroniser(cusContainer, sourceContainer), IsEnabled, DetectEnabled);
						}
					});
				}
			}
		}

		void DeleteCusContainersIfNotInSource(IEnumerable<CommonContainer> sourceCollection)
		{
			var shouldDeleteContainers = Destination.ShouldDeleteContainers;
			var collection = new List<CommonContainer>(sourceCollection);
			foreach (var container in Destination.CusContainers.OfType<BaseCusContainer>().OrderBy(x => x.CO_JC))
			{
				if (!shouldDeleteContainers)
				{
					var sourceContainer = collection.FirstOrDefault(x => x.PK == container.CO_JC) ?? collection.FirstOrDefault(x => x.JC_ContainerNum == container.CO_ContainerNumber);
					if (sourceContainer != null)
					{
						collection.Remove(sourceContainer);
						continue;
					}
				}
				if (DetectEnabled)
				{
					SyncChangesDetected = true;
					return;
				}
				container.Delete();
			}
		}

		BaseCusContainer GetOrCreateCusContainer(CommonContainer sourceContainer, List<BaseCusContainer> existingContainers)
		{
			var containerNumber = sourceContainer.JC_ContainerNum;
			var sourcePK = sourceContainer.PK;
			var result = existingContainers.FirstOrDefault(x => x.CO_JC == sourcePK) ?? existingContainers.FirstOrDefault(x => x.CO_ContainerNumber == containerNumber);
			if (result != null)
			{
				if (result.CO_JC != sourceContainer.PK)
				{
					if (DetectEnabled)
					{
						SyncChangesDetected = true;
						return null;
					}
					result.CO_JC = sourceContainer.PK;
				}
			}
			else if (!Destination.ShouldDeleteContainers)
			{
				if (DetectEnabled)
				{
					SyncChangesDetected = true;
					return null;
				}
				using (Destination.CusContainers.SuspendCusContainerListChanged())
				{
					result = Destination.CusContainers.AddNew();
					result.CO_ContainerNumber = sourceContainer.JC_ContainerNum;
					result.CO_JC = sourceContainer.PK;
				}
			}
			return result;
		}

#if DEBUG
		public bool LoadJobContainerForTesting { get; set; }
#endif

		#endregion

		#region Hook/UnHook Events

		protected override IEnumerable<BusinessObjectCollection> GetCollectionsToHookCountChangedEvent()
		{
			return Array.Empty<BusinessObjectCollection>();//It is notified when users change the container on PackLine
		}

		#endregion
	}
}

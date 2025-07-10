using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.Business
{
	public class CusInBondContainerCollectionSynchroniser : ContainerCollectionSynchroniser<CusInBondContainer>
	{
		public CusInBondContainerCollectionSynchroniser(ForwardingShipment source, BusinessObject destination, ForwardingConsol consolSource, bool shouldSynchronise, ICusInBondContainerCollection containers)
			: base(source, destination, consolSource, shouldSynchronise, containers)
		{ }

		protected override ContainerSynchroniser<CusInBondContainer> GetContainerSynchroniser(CusInBondContainer billContainer, ForwardingContainer container)
		{
			return new CusInBondContainerSynchroniser(billContainer, container, Source);
		}

		protected override GenericNonContainerizedNumberSynchroniser<CusInBondContainer> GetNewNonContainerisedNumberSynchroniser(CusInBondContainer cusContainer, ForwardingShipment source)
		{
			return new NonContainerizedNumberSynchroniser(cusContainer, source);
		}
	}

	public abstract class ContainerCollectionSynchroniser<TDestContainer> : BusinessObjectCollectionSynchroniser
		where TDestContainer : BusinessObject, ISynchableContainer
	{
		protected ContainerCollectionSynchroniser(ForwardingShipment source, BusinessObject destination, ForwardingConsol consolSource, bool shouldSynchronise, ICusInBondContainerCollection containers)
			: base(source, destination)
		{
			this.consolSource = consolSource;
			this.shouldSynchronise = shouldSynchronise;
			this.containers = containers;
		}
		readonly ForwardingConsol consolSource;
		readonly bool shouldSynchronise;
		readonly ICusInBondContainerCollection containers;

		public new ForwardingShipment Source
		{
			get { return (ForwardingShipment)base.Source; }
		}

		#region Synchronise

		protected override void ForceSynchroniseCore(bool isDeleted)
		{
			base.ForceSynchroniseCore(isDeleted);
			if (!isDeleted && !SyncChangesDetected && shouldSynchronise)
			{
				DeleteOrAddCusInBondContainers();
			}
		}

		protected override void HookElementSynchronisers()
		{
			if (!Destination.IsDeleted && shouldSynchronise)
			{
				HookSynchronisationForExistingContainers();
			}
		}

		IEnumerable<ForwardingContainer> GetSourceContainersToSynchronise()
		{
			if (consolSource != null)
			{
				var containers = Source.ContainersOnConsol(consolSource).ToArray<ForwardingContainer>();

				return from ForwardingContainer container in containers
					   where container != null && !container.IsDeleted && container.JC_ContainerMode != Core.Constants.ContainerModes.BreakBulk
					   select container;
			}

			return new List<ForwardingContainer>();
		}

		void HookSynchronisationForExistingContainers()
		{
			var containerList = new List<TDestContainer>(new TypedEnumerable<TDestContainer>(containers));
			foreach (var sourceContainer in GetSourceContainersToSynchronise())
			{
				var synchroniser = ElementSynchronisers.FindMatchingSource<ContainerSynchroniser<TDestContainer>>(sourceContainer, true);
				if (synchroniser != null)
				{
					synchroniser.SetEnabled(IsEnabled, DetectEnabled);
					containerList.Remove(synchroniser.Destination);
				}
				else
				{
					var containerNumber = sourceContainer.JC_ContainerNum;
					TDestContainer container = null;
					while ((container = containerList.FirstOrDefault(x => !x.IsDeleted && x.ContainerNumInfo.Value.ToString() == containerNumber)) != null)
					{
						synchroniser = ElementSynchronisers.FindMatchingDestination<ContainerSynchroniser<TDestContainer>>(container, true);
						if (synchroniser == null)
						{
							ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(GetContainerSynchroniser(container, sourceContainer), IsEnabled, DetectEnabled);
							containerList.Remove(container);
							break;
						}
						else
						{
							synchroniser.SetEnabled(IsEnabled, DetectEnabled);
							containerList.Remove(container);
						}
					}
				}
			}

			if (HasNonContainerizedGoods())
			{
				var cusContainer = (TDestContainer)containers[CusInBondContainer.NonContainerizedNumber];
				if (cusContainer != null)
				{
					ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(GetNewNonContainerisedNumberSynchroniser(cusContainer, Source), IsEnabled, DetectEnabled);
				}
			}
		}

		void DeleteOrAddCusInBondContainers()
		{
			var containersList = new List<ForwardingContainer>(GetSourceContainersToSynchronise());
			var billContainers = new List<TDestContainer>(new TypedEnumerable<TDestContainer>(containers));
			var hasNonContainerizedGoods = HasNonContainerizedGoods();
			if (containersList.Count > 0)
			{
				while (billContainers.Count > 0)
				{
					var billContainer = billContainers[0];
					billContainers.Remove(billContainer);
					if (billContainer.IsDeleted)
					{
						var synchroniser = ElementSynchronisers.FindMatchingDestination<ContainerSynchroniser<TDestContainer>>(billContainer, true);
						if (synchroniser != null)
						{
							ElementSynchronisers.Remove(synchroniser);
						}
					}
					else
					{
						if (billContainer.IsNonContainerized)
						{
							if (!hasNonContainerizedGoods)
							{
								billContainer.Delete();
							}
						}
						else
						{
							var synchroniser = ElementSynchronisers.FindMatchingDestination<ContainerSynchroniser<TDestContainer>>(billContainer, true);
							if (synchroniser != null)
							{
								if (containersList.Contains(synchroniser.Source))
								{
									containersList.Remove(synchroniser.Source);
									synchroniser.Synchronise();
									continue;
								}
							}
							else
							{
								billContainer = FindMatchingContainerAndAddSynchroniser(containersList, billContainers, billContainer);
							}

							if (billContainer != null)
							{
								billContainer.Delete();
							}
						}
					}
				}

				foreach (var container in containersList)
				{
					var billContainer = (TDestContainer)containers.AddNew();
					var synchroniser = GetContainerSynchroniser(billContainer, container);
					ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(synchroniser, IsEnabled, DetectEnabled);
					synchroniser.Synchronise();
				}
			}
			else
			{
				billContainers.ForEach(billContainer =>
					{
						if (!hasNonContainerizedGoods || !billContainer.IsNonContainerized)
						{
							billContainer.Delete();
						}
					});
			}

			var nonContainerized = containers[CusInBondContainer.NonContainerizedNumber];
			if (hasNonContainerizedGoods)
			{
				if (nonContainerized == null && RequiresNonContainerisedRows)
				{
					var cusContainer = (TDestContainer)containers.AddNew();
					var synchroniser = GetNewNonContainerisedNumberSynchroniser(cusContainer, Source);
					ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(synchroniser, IsEnabled, DetectEnabled);
					synchroniser.Synchronise();
				}
			}
			else if (nonContainerized != null)
			{
				ElementSynchronisers.Remove(nonContainerized, Source);
				nonContainerized.Delete();
			}
		}

		protected abstract ContainerSynchroniser<TDestContainer> GetContainerSynchroniser(TDestContainer billContainer, ForwardingContainer container);

		TDestContainer FindMatchingContainerAndAddSynchroniser(List<ForwardingContainer> containers, List<TDestContainer> billContainers, TDestContainer billContainer)
		{
			ForwardingContainer existingContainer = null;
			var alreadyProcessedContainers = new List<ForwardingContainer>();
			while ((existingContainer = containers.FirstOrDefault(container => !alreadyProcessedContainers.Contains(container) && container.JC_ContainerNum == billContainer.ContainerNumInfo.Value.ToString())) != null)
			{
				alreadyProcessedContainers.Add(existingContainer);
				var synchroniser = ElementSynchronisers.FindMatchingSource<ContainerSynchroniser<TDestContainer>>(existingContainer, true);
				if (synchroniser != null)
				{
					containers.Remove(existingContainer);
					billContainers.Remove(synchroniser.Destination);
					synchroniser.Synchronise();
					break;
				}
				else
				{
					synchroniser = GetContainerSynchroniser(billContainer, existingContainer);
					ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(synchroniser, IsEnabled, DetectEnabled);
					synchroniser.Synchronise();
					containers.Remove(existingContainer);
					billContainer = null;
					break;
				}
			}
			return billContainer;
		}

		bool HasNonContainerizedGoods()
		{
			bool result = false;
			foreach (PackLine packLine in Source.OuterPackLines)
			{
				var container = packLine.GetContainer(consolSource);
				if (container == null || container.JC_ContainerMode == Core.Constants.ContainerModes.BreakBulk)
				{
					result = true;
					break;
				}
			}
			return result;
		}

		#endregion

		protected abstract GenericNonContainerizedNumberSynchroniser<TDestContainer> GetNewNonContainerisedNumberSynchroniser(TDestContainer cusContainer, ForwardingShipment source);

		#region Hook/UnHook Events

		protected override void HookEvents()
		{
			base.HookEvents();
			foreach (var containers in from PackLine pack in Source.OuterPackLines
									   where !pack.IsDeleted
									   select (BusinessObjectCollection)pack.Containers)
			{
				foreach (CommonContainer container in containers)
				{
					HookContainerModeChangeEvent(container);
				}
			}
		}

		protected override void UnHookEvents()
		{
			foreach (var containers in from PackLine pack in Source.OuterPackLines
									   where !pack.IsDeleted
									   select (BusinessObjectCollection)pack.Containers)
			{
				foreach (CommonContainer container in containers)
				{
					UnHookContainerModeChangeEvent(container);
				}
			}
			base.UnHookEvents();
		}

		protected override IEnumerable<BusinessObjectCollection> GetCollectionsToHookCountChangedEvent()
		{
			return new[] { Source.OuterPackLines }.Concat((from PackLine pack in Source.OuterPackLines where !pack.IsDeleted select (BusinessObjectCollection)pack.Containers));
		}

		protected override void Collection_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemAdded)
			{
				var packLine = e.BizObject as PackLine;
				if (packLine != null)
				{
					var containers = packLine.Containers;
					containers.CountChanged -= Collection_CountChanged;
					containers.CountChanged += Collection_CountChanged;

					if (!CollectionsToHookAndUnHookCountChangedEvent.Contains(containers))
					{
						CollectionsToHookAndUnHookCountChangedEvent.Add(containers);
					}
					foreach (CommonContainer container in containers)
					{
						HookContainerModeChangeEvent(container);
					}
				}
				else
				{
					var container = e.BizObject as CommonContainer;
					if (container != null)
					{
						HookContainerModeChangeEvent(container);
					}
				}
			}
			else
			{
				var packLine = e.BizObject as PackLine;
				if (packLine != null)
				{
					var containers = packLine.Containers;
					containers.CountChanged -= Collection_CountChanged;
					CollectionsToHookAndUnHookCountChangedEvent.Remove(containers);
					foreach (var container in containers)
					{
						ElementSynchronisers.Remove(container);
					}
					foreach (CommonContainer container in containers)
					{
						UnHookContainerModeChangeEvent(container);
					}
				}
				else
				{
					var container = e.BizObject as CommonContainer;
					if (container != null)
					{
						UnHookContainerModeChangeEvent(container);
					}
				}
			}
			base.Collection_CountChanged(sender, e);
		}

		void UnHookContainerModeChangeEvent(CommonContainer container)
		{
			container.JC_ContainerModeInfo.ValueChanged -= JC_ContainerModeInfo_ValueChanged;
		}

		void HookContainerModeChangeEvent(CommonContainer container)
		{
			container.JC_ContainerModeInfo.ValueChanged -= JC_ContainerModeInfo_ValueChanged;
			container.JC_ContainerModeInfo.ValueChanged += JC_ContainerModeInfo_ValueChanged;
		}

		void JC_ContainerModeInfo_ValueChanged(object sender, EventArgs e)
		{
			Synchronise();
		}

		#endregion

		/// <summary>
		/// Indicates whether we shoudl make "noncontainerised" container rows, i.e. whether to make (half-) empty container rows on destination when source has no containers. Yes for base & US, no for Ayscuda. 
		/// </summary>
		protected virtual bool RequiresNonContainerisedRows
		{
			get { return true; }
		}
	}
}

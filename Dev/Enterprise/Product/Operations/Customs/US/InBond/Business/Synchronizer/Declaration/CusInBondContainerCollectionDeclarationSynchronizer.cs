using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.InBond.Business
{
	class CusInBondContainerCollectionDeclarationSynchroniser : Customs.Business.BusinessObjectCollectionSynchroniser
	{
		internal CusInBondContainerCollectionDeclarationSynchroniser(Bill source, CusInBondMoveDetail destination)
			: base(source, destination)
		{
			destination.Factory.Saving += Factory_Saving;
		}

		protected new Bill Source
		{
			get { return (Bill)base.Source; }
		}

		protected new CusInBondMoveDetail Destination
		{
			get { return (CusInBondMoveDetail)base.Destination; }
		}

		#region Synchronise

		protected override void ForceSynchroniseCore(bool isDeleted)
		{
			base.ForceSynchroniseCore(isDeleted);
			if (!isDeleted && Destination.ShouldSynchronise)
			{
				DeleteOrAddCusInBondContainers();
			}
		}

		protected override void HookElementSynchronisers()
		{
			if (!Destination.IsDeleted && Destination.ShouldSynchronise)
			{
				HookSynchronisationForExistingContainers();
			}
		}

		CusInBondContainer GetOphantInBondContainerMatching(ZString containerNumber)
		{
			foreach (var container in OphantInBondContainers)
			{
				if (container.BC_ContainerNum == containerNumber)
				{
					return container;
				}
			}
			return null;
		}

		void AddToOphantInBondContainer(CusInBondContainerCommonDeclarationSynchroniser value)
		{
			value.SetEnabled(false, value.DetectEnabled);
			value.MarkCommoditySynchroniserAsOphant();
			AddToOphantInBondContainer(value.Destination);
		}

		void AddToOphantInBondContainer(CusInBondContainer value)
		{
			if (value.IsDeleted || value.IsDeleting)
			{
				OphantInBondContainers.Remove(value);
			}
			else
			{
				value.BC_ParentID = ZGuid.Empty;
				if (!OphantInBondContainers.Contains(value))
				{
					OphantInBondContainers.Add(value);
				}
			}
		}

		void RemoveFromOphantInBondContainer(CusInBondContainer container)
		{
			if (OphantInBondContainers.Contains(container))
			{
				OphantInBondContainers.Remove(container);
			}
		}

		List<CusInBondContainer> OphantInBondContainers
		{
			get { return ophantInBondContainers ?? (ophantInBondContainers = new List<CusInBondContainer>()); }
		}
		List<CusInBondContainer> ophantInBondContainers;

		void CleanUpOphantInBondContainers()
		{
			if (ophantInBondContainers != null)
			{
				foreach (var inBondContainer in ophantInBondContainers)
				{
					if (!inBondContainer.IsDeleted)
					{
						inBondContainer.Delete();
					}
				}
				ophantInBondContainers.Clear();
			}
		}

		void Factory_Saving(BusinessObjectFactory factory)
		{
			CleanUpOphantInBondContainers();
		}

		protected override void DisposeCore()
		{
			if (Destination != null)
			{
				Destination.Factory.Saving -= Factory_Saving;
			}
			base.DisposeCore();
		}

		IEnumerable<CusContainer> GetSourceContainerToSynchronise()
		{
			return Source.AllPackages.OfType<Package>().Select(y =>
				{
					var packingGroup = y.PackingGroup;
					return packingGroup == null ? null : packingGroup.Container;
				}).Where(x => x != null).Distinct();
		}

		protected override void OnEnabledChanged()
		{
			if (Destination.IsDeleted || Destination.IsDeleting)
			{
				foreach (var synchroniser in ElementSynchronisers.OfType<CusInBondContainerCommonDeclarationSynchroniser>().ToArray())
				{
					AddToOphantInBondContainer(synchroniser);
					ElementSynchronisers.Remove(synchroniser);
				}
			}
			base.OnEnabledChanged();
		}

		void HookSynchronisationForExistingContainers()
		{
			var containerList = new List<CusInBondContainer>(Destination.Containers);
			foreach (var sourceContainer in GetSourceContainerToSynchronise())
			{
				var synchroniser = ElementSynchronisers.FindMatchingSource<CusInBondContainerDeclarationSynchroniser>(sourceContainer);
				if (synchroniser != null)
				{
					synchroniser.SetEnabled(IsEnabled, synchroniser.DetectEnabled);
					containerList.Remove(synchroniser.Destination);
				}
				else
				{
					var containerNumber = sourceContainer.CO_ContainerNumber;
					CusInBondContainer container = null;
					while ((container = containerList.FirstOrDefault(x => !x.IsDeleted && x.BC_ContainerNum == containerNumber)) != null)
					{
						synchroniser = ElementSynchronisers.FindMatchingDestination<CusInBondContainerDeclarationSynchroniser>(container);
						if (synchroniser == null)
						{
							ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(GetContainerSynchroniser(container, sourceContainer), IsEnabled, DetectEnabled);
							containerList.Remove(container);
							break;
						}
						else
						{
							synchroniser.SetEnabled(IsEnabled, synchroniser.DetectEnabled);
							containerList.Remove(container);
						}
					}
				}
			}

			if (HasNonContainerizedGoods())
			{
				var inBondContainer = Destination.Containers[CusInBondContainer.NonContainerizedNumber];
				if (inBondContainer != null)
				{
					ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(new NonContainerizedNumberDeclarationSynchroniser(inBondContainer, Source), IsEnabled, DetectEnabled);
				}
			}
		}

		bool HasNonContainerizedGoods(Bill bill)
		{
			bool result = false;
			foreach (PackingGroup packingGroup in bill.PackingGroups)
			{
				if (packingGroup.CR_CO_Container.IsEmpty)
				{
					result = true;
					break;
				}
			}
			return result;
		}

		bool HasNonContainerizedGoods()
		{
			bool result = false;
			if (Source.AllPackages.Count > 0)
			{
				result = HasNonContainerizedGoods(Source);
				if (!result)
				{
					foreach (Bill childBill in Source.ChildBills)
					{
						result = HasNonContainerizedGoods(childBill);
						if (result)
						{
							break;
						}
					}
				}
			}
			return result;
		}

		void DeleteOrAddCusInBondContainers()
		{
			var containerList = new List<CusContainer>(GetSourceContainerToSynchronise());
			var inBondContainers = new List<CusInBondContainer>(Destination.Containers);
			var hasNonContainerizedGoods = HasNonContainerizedGoods();
			if (containerList.Count > 0)
			{
				var alreadyFoundNonContainerized = false;
				while (inBondContainers.Count > 0)
				{
					var inBondContainer = inBondContainers[0];
					inBondContainers.Remove(inBondContainer);
					var synchroniser = inBondContainer.IsDeleted ? ElementSynchronisers.FindMatchingDestination<CusInBondContainerCommonDeclarationSynchroniser>(inBondContainer, true) : (inBondContainer.IsNonContainerized ?
						ElementSynchronisers.FindMatchingDestination<NonContainerizedNumberDeclarationSynchroniser>(inBondContainer) :
						ElementSynchronisers.FindMatchingDestination<CusInBondContainerDeclarationSynchroniser>(inBondContainer));
					if (inBondContainer.IsDeleted || inBondContainer.IsDeleting)
					{
						if (synchroniser != null)
						{
							synchroniser.MarkCommoditySynchroniserAsOphant();
							ElementSynchronisers.Remove(synchroniser);
						}
					}
					else
					{
						if (inBondContainer.IsNonContainerized)
						{
							if (!hasNonContainerizedGoods || alreadyFoundNonContainerized)
							{
								if (synchroniser != null)
								{
									AddToOphantInBondContainer(synchroniser);
									ElementSynchronisers.Remove(synchroniser);
								}
								else
								{
									AddToOphantInBondContainer(inBondContainer);
								}
							}
							else if (hasNonContainerizedGoods)
							{
								alreadyFoundNonContainerized = true;
								if (synchroniser == null)
								{
									synchroniser = new NonContainerizedNumberDeclarationSynchroniser(inBondContainer, Source);
									ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(synchroniser, IsEnabled, DetectEnabled);
									synchroniser.Synchronise();
								}
							}
						}
						else
						{
							var containerizedSynchroniser = (CusInBondContainerDeclarationSynchroniser)synchroniser;
							if (containerizedSynchroniser != null)
							{
								if (containerList.Contains(containerizedSynchroniser.Source))
								{
									containerList.Remove(containerizedSynchroniser.Source);
									containerizedSynchroniser.Synchronise();
									continue;
								}
							}
							else
							{
								inBondContainer = FindMatchingContainerAndAddSynchroniser(containerList, inBondContainers, inBondContainer);
							}

							if (inBondContainer != null)
							{
								containerizedSynchroniser = ElementSynchronisers.FindMatchingDestination<CusInBondContainerDeclarationSynchroniser>(inBondContainer);
								if (containerizedSynchroniser != null)
								{
									AddToOphantInBondContainer(containerizedSynchroniser);
									ElementSynchronisers.Remove(containerizedSynchroniser);
								}
								else
								{
									AddToOphantInBondContainer(inBondContainer);
								}
							}
						}
					}
				}

				foreach (var container in containerList)
				{
					var inBondContainer = GetOphantInBondContainerMatching(container.CO_ContainerNumber);
					if (inBondContainer == null)
					{
						inBondContainer = Destination.Containers.AddNew();
					}
					else
					{
						RemoveFromOphantInBondContainer(inBondContainer);
						Destination.Containers.Add(inBondContainer);
					}
					var synchroniser = GetContainerSynchroniser(inBondContainer, container);
					ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(synchroniser, IsEnabled, DetectEnabled);
					synchroniser.Synchronise();
				}

				if (!alreadyFoundNonContainerized && hasNonContainerizedGoods)
				{
					var inBondContainer = GetOphantInBondContainerMatching(CusInBondContainer.NonContainerizedNumber);
					if (inBondContainer == null)
					{
						inBondContainer = Destination.Containers.AddNew();
					}
					else
					{
						RemoveFromOphantInBondContainer(inBondContainer);
						Destination.Containers.Add(inBondContainer);
					}
					var synchroniser = new NonContainerizedNumberDeclarationSynchroniser(inBondContainer, Source);
					ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(synchroniser, IsEnabled, DetectEnabled);
					synchroniser.Synchronise();
				}
			}
			else
			{
				if (hasNonContainerizedGoods)
				{
					var nonContainerized = Destination.Containers[CusInBondContainer.NonContainerizedNumber];
					if (nonContainerized == null)
					{
						nonContainerized = GetOphantInBondContainerMatching(CusInBondContainer.NonContainerizedNumber);
						if (nonContainerized == null)
						{
							nonContainerized = Destination.Containers.AddNew();
						}
						else
						{
							RemoveFromOphantInBondContainer(nonContainerized);
							Destination.Containers.Add(nonContainerized);
						}
					}
					else
					{
						inBondContainers.Remove(nonContainerized);
					}

					var synchroniser = ElementSynchronisers.FindMatchingDestination<NonContainerizedNumberDeclarationSynchroniser>(nonContainerized) ?? new NonContainerizedNumberDeclarationSynchroniser(nonContainerized, Source);
					ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(synchroniser, IsEnabled, DetectEnabled);
					synchroniser.Synchronise();
				}

				inBondContainers.ForEach(inBondContainer =>
				{
					var synchroniser = ElementSynchronisers.FindMatchingDestination<CusInBondContainerDeclarationSynchroniser>(inBondContainer);
					if (synchroniser != null)
					{
						AddToOphantInBondContainer(synchroniser);
						ElementSynchronisers.Remove(synchroniser);
					}
					else
					{
						AddToOphantInBondContainer(inBondContainer);
					}
				});
			}
		}

		CusInBondContainerDeclarationSynchroniser GetContainerSynchroniser(CusInBondContainer inBondContainer, CusContainer container)
		{
			return new CusInBondContainerDeclarationSynchroniser(inBondContainer, container, Source);
		}

		CusInBondContainer FindMatchingContainerAndAddSynchroniser(List<CusContainer> containers, List<CusInBondContainer> inBondContainers, CusInBondContainer inBondContainer)
		{
			CusContainer existingContainer = null;
			var alreadyProcessedContainers = new List<CusContainer>();
			while ((existingContainer = containers.FirstOrDefault(container => !alreadyProcessedContainers.Contains(container) && container.CO_ContainerNumber == inBondContainer.BC_ContainerNum)) != null)
			{
				alreadyProcessedContainers.Add(existingContainer);
				var synchroniser = ElementSynchronisers.FindMatchingSource<CusInBondContainerDeclarationSynchroniser>(existingContainer);
				if (synchroniser != null)
				{
					containers.Remove(existingContainer);
					inBondContainers.Remove(synchroniser.Destination);
					synchroniser.Synchronise();
					break;
				}
				else
				{
					synchroniser = GetContainerSynchroniser(inBondContainer, existingContainer);
					ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(synchroniser, IsEnabled, DetectEnabled);
					synchroniser.Synchronise();
					containers.Remove(existingContainer);
					inBondContainer = null;
					break;
				}
			}
			return inBondContainer;
		}

		#endregion

		#region Hook/UnHook Events

		protected override void Collection_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemRemoved)
			{
				var container = e.BizObject as CusContainer;
				if (container != null)
				{
					var synchroniser = ElementSynchronisers.FindMatchingSource<CusInBondContainerDeclarationSynchroniser>(container);
					if (synchroniser != null)
					{
						synchroniser.MarkCommoditySynchroniserAsOphant();
						if (container.IsDeleted || container.IsDeleting)
						{
							if (!synchroniser.Destination.IsDeleted)
							{
								synchroniser.Destination.Delete();
							}
						}
						ElementSynchronisers.Remove(synchroniser);
					}
				}
			}
			base.Collection_CountChanged(sender, e);
		}

		protected override IEnumerable<BusinessObjectCollection> GetCollectionsToHookCountChangedEvent()
		{
			yield return Source.AllPackages;
			yield return (BusinessObjectCollection)Source.Declaration.CusContainers;
		}

		#endregion
	}
}

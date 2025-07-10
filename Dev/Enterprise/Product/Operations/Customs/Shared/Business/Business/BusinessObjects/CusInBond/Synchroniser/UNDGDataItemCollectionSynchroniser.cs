using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class UNDGDataItemCollectionSynchroniser : BusinessObjectCollectionSynchroniser
	{
		public UNDGDataItemCollectionSynchroniser(ForwardingShipment source, CusInBondContainer destination)
			: base(source, destination)
		{
		}

		public new CusInBondContainer Destination
		{
			get { return (CusInBondContainer)base.Destination; }
		}

		public new ForwardingShipment Source
		{
			get { return (ForwardingShipment)base.Source; }
		}

		#region Synchronise

		protected override void ForceSynchroniseCore(bool isDeleted)
		{
			base.ForceSynchroniseCore(isDeleted);
			if (!isDeleted && !SyncChangesDetected && Destination.ShouldSynchronise)
			{
				DeleteOrAddUNDGs();
			}
		}

		void DeleteOrAddUNDGs()
		{
			var sourceUNDGs = new List<UNDGDataItem>(GetSourceUNDGsToSynchronise());
			if (sourceUNDGs.Count > 0)
			{
				var undgs = new List<UNDGDataItem>(Destination.UNDGs);
				while (undgs.Count > 0)
				{
					var undg = undgs[0];
					undgs.Remove(undg);
					if (undg.IsDeleted)
					{
						var synchroniser = UNDGSynchronisers.FirstOrDefault(x => x.destination == undg);
						if (synchroniser != null)
						{
							synchroniser.SetEnabled(false, synchroniser.DetectEnabled);
							UNDGSynchronisers.Remove(synchroniser);
						}
					}
					else
					{
						var synchroniser = UNDGSynchronisers.FirstOrDefault(x => x.destination == undg);
						if (synchroniser != null)
						{
							bool found = false;
							foreach (var sourceUNDG in synchroniser.Sources)
							{
								if (sourceUNDGs.Contains(sourceUNDG))
								{
									sourceUNDGs.Remove(sourceUNDG);
									found = true;
								}
							}
							synchroniser.Synchronise();
							if (found)
							{
								continue;
							}
						}
						else
						{
							undg = FindMatchingUNDGAndAddSynchroniser(sourceUNDGs, undgs, undg);
						}

						if (undg != null)
						{
							undg.Delete();
						}
					}
				}

				foreach (var sourceUNDG in sourceUNDGs)
				{
					var synchroniser = AddUNDGSynchroniserIfNotExists(sourceUNDG);
					synchroniser.Synchronise();
				}
			}
			else
			{
				Destination.UNDGs.DeleteAll();
			}
		}

		UNDGDataItem FindMatchingUNDGAndAddSynchroniser(List<UNDGDataItem> sourceUNDGs, List<UNDGDataItem> undgs, UNDGDataItem undg)
		{
			UNDGDataItem existingUNDG = null;
			var alreadyProcessedPackLines = new List<UNDGDataItem>();
			while ((existingUNDG = sourceUNDGs.FirstOrDefault(sourceUNDG => !alreadyProcessedPackLines.Contains(sourceUNDG) && HasUNDGGotSameData(sourceUNDG, undg))) != null)
			{
				alreadyProcessedPackLines.Add(existingUNDG);
				var synchroniser = UNDGSynchronisers.FirstOrDefault(x => x.ContainsSource(existingUNDG));
				if (synchroniser != null)
				{
					sourceUNDGs.Remove(existingUNDG);
					undgs.Remove(synchroniser.destination);
					synchroniser.Synchronise();
					break;
				}
				else
				{
					synchroniser = new UNDGDataItemSynchroniser(undg, existingUNDG, this);
					UNDGSynchronisers.Add(synchroniser);
					synchroniser.SetEnabled(IsEnabled, DetectEnabled);
					synchroniser.Synchronise();
					sourceUNDGs.Remove(existingUNDG);
					undg = null;
					break;
				}
			}
			return undg;
		}

		protected override void HookElementSynchronisers()
		{
			if (!Destination.IsDeleted && Destination.ShouldSynchronise)
			{
				HookSynchronisationForExistingUNDGs();
			}
		}

		IEnumerable<UNDGDataItem> GetSourceUNDGsToSynchronise()
		{
			if (!Destination.IsDeleted)
			{
				var containerNum = Destination.BC_ContainerNum;
				var isNonContainerized = Destination.IsNonContainerized;
				foreach (UNDGDataItemCollection undgs in
							(from PackLine line in Source.OuterPackLines
							 where line != null && !line.IsDeleted && IsForContainer(line, isNonContainerized, containerNum)
							 select line.UNDGs))
				{
					foreach (UNDGDataItem undg in undgs)
					{
						yield return undg;
					}
				}
			}
		}

		bool IsForContainer(PackLine line, bool isNonContainerized, ZString containerNum)
		{
			bool result = false;
			if (isNonContainerized)
			{
				var header = Destination.Header;
				var consolSource = header == null ? null : header.Consol;
				result = line.GetContainer(consolSource) == null;
			}
			else if (line.Containers.Count > 0)
			{
				var header = Destination.Header;
				var consolSource = header == null ? null : header.Consol;
				var container = line.GetContainer(consolSource);
				result = container != null && container.JC_ContainerNum == containerNum;
			}
			return result;
		}

		void HookSynchronisationForExistingUNDGs()
		{
			var undgs = new List<UNDGDataItem>(Destination.UNDGs);
			foreach (var sourceUNDG in GetSourceUNDGsToSynchronise())
			{
				var synchroniser = UNDGSynchronisers.FirstOrDefault(x => x.ContainsSource(sourceUNDG));
				if (synchroniser != null)
				{
					synchroniser.SetEnabled(IsEnabled, DetectEnabled);
					undgs.Remove(synchroniser.destination);
				}
				else
				{
					UNDGDataItem undg = null;
					while ((undg = undgs.FirstOrDefault(x => !x.IsDeleted && HasUNDGGotSameData(x, sourceUNDG))) != null)
					{
						synchroniser = UNDGSynchronisers.FirstOrDefault(x => x.destination == undg);
						if (synchroniser == null)
						{
							synchroniser = new UNDGDataItemSynchroniser(undg, sourceUNDG, this);
							UNDGSynchronisers.Add(synchroniser);
							synchroniser.SetEnabled(IsEnabled, DetectEnabled);
							undgs.Remove(undg);
							break;
						}
						else
						{
							synchroniser.SetEnabled(IsEnabled, DetectEnabled);
							undgs.Remove(undg);
						}
					}
				}
			}
		}

		void OnUNDGWasUpdatedByDataRefreshIncludingChildren(object sender, EventArgs e)
		{
			Synchronise();
		}

		internal UNDGDataItemSynchroniser AddUNDGSynchroniserIfNotExists(UNDGDataItem sourceUNDG)
		{
			var synchroniser = UNDGSynchronisers.FirstOrDefault(x => !x.destination.IsDeleted && (x.ContainsSource(sourceUNDG) || HasUNDGGotSameData(x.destination, sourceUNDG)));
			if (synchroniser == null)
			{
				synchroniser = new UNDGDataItemSynchroniser(Destination.UNDGs.AddNew(), sourceUNDG, this);
				UNDGSynchronisers.Add(synchroniser);
			}
			else
			{
				synchroniser.AddSource(sourceUNDG);
			}
			synchroniser.SetEnabled(IsEnabled, DetectEnabled);
			return synchroniser;
		}

		List<UNDGDataItemSynchroniser> UNDGSynchronisers
		{
			get { return fUNDGSynchronisers ?? (fUNDGSynchronisers = new List<UNDGDataItemSynchroniser>()); }
		}
		List<UNDGDataItemSynchroniser> fUNDGSynchronisers;

		public bool HasUNDGGotSameData(UNDGDataItem undgA, UNDGDataItem undgB)
		{
			return undgB.DI_DG == undgA.DI_DG && undgB.DI_DGFlashPoint == undgA.DI_DGFlashPoint && undgB.DI_OC_DGContact == undgA.DI_OC_DGContact && undgB.DI_TechnicalName == undgA.DI_TechnicalName;
		}

		internal UNDGDataItemSynchroniser GetOtherSynchroniser(UNDGDataItemSynchroniser notThisSynchroniser, UNDGDataItem undg)
		{
			return UNDGSynchronisers.FirstOrDefault(x => !x.destination.IsDeleted && x != notThisSynchroniser && HasUNDGGotSameData(x.destination, undg));
		}

		internal void RemoveSynchroniser(UNDGDataItemSynchroniser synchroniser)
		{
			if (!SyncChangesDetected && UNDGSynchronisers.Contains(synchroniser))
			{
				synchroniser.destination.Delete();
				UNDGSynchronisers.Remove(synchroniser);
			}
		}

		#endregion

		#region Hook/UnHook Events

		protected override void HookEvents()
		{
			base.HookEvents();
			foreach (PackLine pack in Source.OuterPackLines)
			{
				pack.UNDGs.CountChanged -= UNDGs_CountChanged;
				pack.UNDGs.CountChanged += UNDGs_CountChanged;
			}
			SetReadOnlyIncludingChildren(true);
			foreach (var sourceUNDG in GetSourceUNDGsToSynchronise())
			{
#pragma warning disable
				((IBusinessObjectState)sourceUNDG).UpdatedByDataRefreshIncludingChildren -= OnUNDGWasUpdatedByDataRefreshIncludingChildren;
				((IBusinessObjectState)sourceUNDG).UpdatedByDataRefreshIncludingChildren += OnUNDGWasUpdatedByDataRefreshIncludingChildren;
#pragma warning restore
			}
		}

		protected virtual void SetReadOnlyIncludingChildren(bool isReadOnly)
		{
			Destination.UNDGs.SetReadOnlyIncludingChildren(isReadOnly);
		}

		protected override void UnHookEvents()
		{
			SetReadOnlyIncludingChildren(false);
			foreach (PackLine pack in Source.OuterPackLines)
			{
				pack.UNDGs.CountChanged -= UNDGs_CountChanged;
			}
			foreach (var synchroniser in UNDGSynchronisers.ToArray())
			{
				if (synchroniser.IsEmpty)
				{
					synchroniser.SetEnabled(false, synchroniser.DetectEnabled);
					UNDGSynchronisers.Remove(synchroniser);
				}
			}
			foreach (var sourceUNDG in GetSourceUNDGsToSynchronise())
			{
#pragma warning disable
				((IBusinessObjectState)sourceUNDG).UpdatedByDataRefreshIncludingChildren -= OnUNDGWasUpdatedByDataRefreshIncludingChildren;
#pragma warning restore
			}
			base.UnHookEvents();
		}

		void UNDGs_CountChanged(object sender, EventArgs e)
		{
			ForceSynchronise();
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
					packLine.Containers.CountChanged -= Collection_CountChanged;
					packLine.Containers.CountChanged += Collection_CountChanged;
					packLine.UNDGs.CountChanged -= UNDGs_CountChanged;
					packLine.UNDGs.CountChanged += UNDGs_CountChanged;

					if (!CollectionsToHookAndUnHookCountChangedEvent.Contains(packLine.Containers))
					{
						CollectionsToHookAndUnHookCountChangedEvent.Add(packLine.Containers);
					}
				}
			}
			else
			{
				var packLine = e.BizObject as PackLine;
				if (packLine != null)
				{
					packLine.Containers.CountChanged -= Collection_CountChanged;
					packLine.UNDGs.CountChanged -= UNDGs_CountChanged;
					CollectionsToHookAndUnHookCountChangedEvent.Remove(packLine.Containers);
					foreach (var container in packLine.Containers)
					{
						ElementSynchronisers.Remove(container);
					}
				}
			}
			base.Collection_CountChanged(sender, e);
		}

		#endregion
	}
}

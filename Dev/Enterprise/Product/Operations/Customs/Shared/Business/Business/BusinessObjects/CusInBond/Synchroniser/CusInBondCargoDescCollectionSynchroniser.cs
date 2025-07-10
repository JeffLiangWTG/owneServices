using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.Business
{
	public class CusInBondCargoDescCollectionSynchroniser : BusinessObjectCollectionSynchroniser
	{
		public CusInBondCargoDescCollectionSynchroniser(ForwardingShipment source, CusInBondContainer destination)
			: base(source, destination)
		{
			var header = destination.Header;
			consolSource = header == null ? null : header.Consol;
		}

		public new CusInBondContainer Destination
		{
			get { return (CusInBondContainer)base.Destination; }
		}

		public new ForwardingShipment Source
		{
			get { return (ForwardingShipment)base.Source; }
		}

		readonly ForwardingConsol consolSource;

		#region Synchronise

		protected override void ForceSynchroniseCore(bool isDeleted)
		{
			base.ForceSynchroniseCore(isDeleted);
			if (!isDeleted && !SyncChangesDetected && Destination.ShouldSynchronise)
			{
				DeleteOrAddCusInBondCargoDescs();
			}
		}

		protected virtual void DeleteOrAddCusInBondCargoDescs()
		{
			var packLines = new List<PackLine>(GetSourcePackLinesToSynchronise());
			if (packLines.Count > 0)
			{
				var commodities = new List<CusInBondCargoDesc>(new TypedEnumerable<CusInBondCargoDesc>(Destination.Commodities));
				while (commodities.Count > 0)
				{
					var commodity = commodities[0];
					commodities.Remove(commodity);
					if (commodity.IsDeleted)
					{
						var synchroniser = ElementSynchronisers.FindMatchingDestination<CusInBondCargoDescSynchroniser>(commodity);
						if (synchroniser != null)
						{
							synchroniser.SetEnabled(false, synchroniser.DetectEnabled);
							ElementSynchronisers.Remove(synchroniser);
						}
					}
					else
					{
						var synchroniser = ElementSynchronisers.FindMatchingDestination<CusInBondCargoDescSynchroniser>(commodity);
						if (synchroniser != null)
						{
							if (packLines.Contains(synchroniser.Source))
							{
								packLines.Remove(synchroniser.Source);
								synchroniser.Synchronise();
								continue;
							}
						}
						else
						{
							commodity = FindMatchingPackLineAndAddSynchroniser(packLines, commodities, commodity);
						}

						if (commodity != null)
						{
							commodity.Delete();
						}
					}
				}

				foreach (var packLine in packLines)
				{
					var commodity = Destination.Commodities.AddNew();
					var synchroniser = new CusInBondCargoDescSynchroniser(commodity, packLine);
					ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(synchroniser, IsEnabled, DetectEnabled);
					synchroniser.Synchronise();
				}
			}
			else
			{
				Destination.Commodities.DeleteAll();
			}
		}

		protected CusInBondCargoDesc FindMatchingPackLineAndAddSynchroniser(List<PackLine> packLines, List<CusInBondCargoDesc> commodities, CusInBondCargoDesc commodity)
		{
			PackLine existingPackLine = null;
			var alreadyProcessedPackLines = new List<PackLine>();
			while ((existingPackLine = packLines.FirstOrDefault(packLine => !alreadyProcessedPackLines.Contains(packLine) && TariffFormatter.Format(packLine.JL_HarmonisedCode) == commodity.BY_HarmonisedTariff)) != null)
			{
				alreadyProcessedPackLines.Add(existingPackLine);
				var synchroniser = ElementSynchronisers.FindMatchingSource<CusInBondCargoDescSynchroniser>(existingPackLine);
				if (synchroniser != null)
				{
					packLines.Remove(existingPackLine);
					commodities.Remove(synchroniser.Destination);
					synchroniser.Synchronise();
					break;
				}
				else
				{
					synchroniser = new CusInBondCargoDescSynchroniser(commodity, existingPackLine);
					ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(synchroniser, IsEnabled, DetectEnabled);
					synchroniser.Synchronise();
					packLines.Remove(existingPackLine);
					commodity = null;
					break;
				}
			}
			return commodity;
		}

		protected override void HookElementSynchronisers()
		{
			if (!Destination.IsDeleted && Destination.ShouldSynchronise)
			{
				HookSynchronisationForExistingCommodities();
			}
		}

		protected virtual IEnumerable<PackLine> GetSourcePackLinesToSynchronise()
		{
			var containerNum = Destination.BC_ContainerNum;
			var isNonContainerized = Destination.IsNonContainerized;
			return from PackLine line in Source.OuterPackLines
				   where line != null && !line.IsDeleted && IsForContainer(line, isNonContainerized, containerNum)
				   select line;
		}

		protected bool IsForContainer(PackLine line, bool isNonContainerized, ZString containerNum)
		{
			bool result = false;
			if (isNonContainerized)
			{
				var container = line.GetContainer(consolSource);
				result = container == null || container.JC_ContainerMode == Core.Constants.ContainerModes.BreakBulk;
			}
			else if (line.Containers.Count > 0)
			{
				var container = line.GetContainer(consolSource);
				result = container != null && container.JC_ContainerNum == containerNum;
			}
			return result;
		}

		protected virtual void HookSynchronisationForExistingCommodities()
		{
			var commodities = new List<CusInBondCargoDesc>(new TypedEnumerable<CusInBondCargoDesc>(Destination.Commodities));
			foreach (var sourcePackLine in GetSourcePackLinesToSynchronise())
			{
				var synchroniser = ElementSynchronisers.FindMatchingSource<CusInBondCargoDescSynchroniser>(sourcePackLine);
				if (synchroniser != null)
				{
					synchroniser.SetEnabled(IsEnabled, DetectEnabled);
					commodities.Remove(synchroniser.Destination);
				}
				else
				{
					var harmonisedCode = TariffFormatter.Format(sourcePackLine.JL_HarmonisedCode);
					CusInBondCargoDesc commodity = null;
					while ((commodity = commodities.FirstOrDefault(x => !x.IsDeleted && x.BY_HarmonisedTariff == harmonisedCode)) != null)
					{
						synchroniser = ElementSynchronisers.FindMatchingDestination<CusInBondCargoDescSynchroniser>(commodity);
						if (synchroniser == null)
						{
							ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(new CusInBondCargoDescSynchroniser(commodity, sourcePackLine), IsEnabled, DetectEnabled);
							commodities.Remove(commodity);
							break;
						}
						else
						{
							synchroniser.SetEnabled(IsEnabled, DetectEnabled);
							commodities.Remove(commodity);
						}
					}
				}
			}
		}

		protected TariffFormatter TariffFormatter
		{
			get { return tariffFormatter ?? (tariffFormatter = new TariffFormatter()); }
		}
		TariffFormatter tariffFormatter;

		#endregion

		#region Hook/UnHook Events

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

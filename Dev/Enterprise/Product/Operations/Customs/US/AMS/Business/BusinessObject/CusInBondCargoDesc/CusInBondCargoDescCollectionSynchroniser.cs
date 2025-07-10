using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.US.AMS.Business
{
	public class CusInBondCargoDescCollectionSynchroniser : Customs.Business.CusInBondCargoDescCollectionSynchroniser
	{
		public CusInBondCargoDescCollectionSynchroniser(ForwardingShipment source, CusInBondContainer destination)
			: base(source, destination)
		{
		}

		protected override IEnumerable<BusinessObjectCollection> GetCollectionsToHookCountChangedEvent()
		{
			return new ForwardingPackLineCollection[1] { Source.OuterPackLines }
			.Concat((from PackLine pack in Source.OuterPackLines
					 where !pack.IsDeleted
					 select pack).Select((Func<PackLine, BusinessObjectCollection>)((PackLine pack) => pack.Containers)))
			.Concat(new InnerPackLineCollection[1] { Source.InnerPackLines });
		}

		protected override void DeleteOrAddCusInBondCargoDescs()
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
						var synchroniser = ElementSynchronisers.FindMatchingDestination<BusinessObjectSynchroniser>(commodity);
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
							var commodityPK = commodity.PK;
							commodity = FindMatchingPackLineAndAddSynchroniser(packLines, commodities.OfType<Customs.Business.CusInBondCargoDesc>().ToList(), commodity) as CusInBondCargoDesc;
							if (commodity == null)
							{
								commodity = Destination.Commodities.FindByPK(commodityPK) as CusInBondCargoDesc;
							}
						}

						if (commodity != null)
						{
							commodity.Delete();
						}
					}
				}

				foreach (var packLine in packLines)
				{
					var commodity = (CusInBondCargoDesc)Destination.Commodities.AddNew();
					var synchroniser = new CusInBondCargoDescSynchroniser(commodity, packLine, GetLinkedPackLineOrItself(packLine));
					ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(synchroniser, IsEnabled, DetectEnabled);
					synchroniser.Synchronise();
				}
			}
			else
			{
				Destination.Commodities.DeleteAll();
			}
		}

		protected override void HookSynchronisationForExistingCommodities()
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
					var linkedPackLine = GetLinkedPackLineOrItself(sourcePackLine);
					var harmonisedCodeOrigin = linkedPackLine?.JL_HarmonisedCode ?? ZString.Empty;
					var harmonisedCode = TariffFormatter.Format(harmonisedCodeOrigin);
					CusInBondCargoDesc commodity = null;
					while ((commodity = commodities.FirstOrDefault(x => !x.IsDeleted && x.BY_HarmonisedTariff == harmonisedCode)) != null)
					{
						synchroniser = ElementSynchronisers.FindMatchingDestination<CusInBondCargoDescSynchroniser>(commodity);
						if (synchroniser == null)
						{
							synchroniser = new CusInBondCargoDescSynchroniser(commodity, sourcePackLine, linkedPackLine);
							ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(synchroniser, IsEnabled, DetectEnabled);
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

		protected override IEnumerable<PackLine> GetSourcePackLinesToSynchronise()
		{
			var containerNum = Destination.BC_ContainerNum;
			var isNonContainerized = Destination.IsNonContainerized;
			return Source.MergeOuterPackLinesIntoInnerPackLines()
				.Where(line => line != null && !line.IsDeleted)
				.Where(line =>
				{
					var line4ContainerCheck = GetLinkedPackLineOrItself(line);
					return line4ContainerCheck != null && IsForContainer(line4ContainerCheck, isNonContainerized, containerNum);
				});
		}

		PackLine GetLinkedPackLineOrItself(PackLine packLine)
		{
			return packLine.IsInnerPackType ? Source.GetInnerPackLineLinkedOuterPackLine(packLine) : packLine;
		}
	}
}

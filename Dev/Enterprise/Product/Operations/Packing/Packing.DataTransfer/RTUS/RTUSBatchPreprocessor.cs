using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using WTG.RTUS.Interface;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Packing.DataTransfer
{
	public static class RTUSBatchPreprocessor
	{
		public static IReadOnlyCollection<UniversalShipment> Process(IDataObjectWriterStrategy strategy, UniversalShipment batch, IReadOnlyCollection<int> customSegments, IRTUSProcessor processor)
		{
			var packages = ExtractOrderedPackages(batch);
			return SplitBatchBy(strategy, packages, customSegments, processor.IdealPackagesInBatchSegment);
		}

		static IOrderedEnumerable<PackageMeta> ExtractOrderedPackages(UniversalShipment batch)
		{
			var packages = new List<PackageMeta>();
			foreach (var packingParent in batch.SubShipmentCollection)
			{
				foreach (var package in packingParent.PackingLineCollection)
				{
					packages.Add(new PackageMeta(packingParent, package));
				}
			}
			return packages.OrderBy(x => x.Package.ItemNo);
		}

		static IReadOnlyCollection<UniversalShipment> SplitBatchBy(IDataObjectWriterStrategy strategy, IOrderedEnumerable<PackageMeta> packages, IReadOnlyCollection<int> customSegments, int standardSegmentSize)
		{
			var currentNo = 0;
			var segments = new List<UniversalShipment>();
			var segmentPackages = new List<PackageMeta>();
			foreach (var package in packages)
			{
				if (package.Package.ItemNo != ++currentNo)
				{
					throw new FormatException(string.Format(CultureInfo.InvariantCulture, "ItemNo {0} missing.", currentNo));
				}
				segmentPackages.Add(package);
				if (customSegments != null && customSegments.Contains(currentNo) || segmentPackages.Count == standardSegmentSize)
				{
					segments.Add(BuildSegment(strategy, segmentPackages));
					segmentPackages.Clear();
				}
			}
			if (segmentPackages.Count != 0)
			{
				segments.Add(BuildSegment(strategy, segmentPackages));
			}
			return segments;
		}

		static UniversalShipment BuildSegment(IDataObjectWriterStrategy strategy, IEnumerable<PackageMeta> segmentPackages)
		{
			var segment = new UniversalShipment(strategy)
			{
				DataContext = DataContextFactory.New()
			};
			segment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());

			var packingParents = new Dictionary<string, UniversalShipment>();

			foreach (var package in segmentPackages)
			{
				var packingParentId = AttachPackingParentIfRequired(strategy, packingParents, segment, package.PackingParent);
				packingParents[packingParentId].PackingLineCollection?.Add(package.Package);
			}

			return segment;
		}

		static ZString AttachPackingParentIfRequired(IDataObjectWriterStrategy strategy, Dictionary<string, UniversalShipment> packingParents, UniversalShipment segment, UniversalShipment packingParent)
		{
			var packingParentId = (ZString)packingParent.DataContext.DataSourceCollection.Single().Key;
			if (!packingParents.ContainsKey(packingParentId))
			{
				segment.DataContext.AddDataSource(DataContextType.WarehouseOrder, packingParentId);
				var clonePackingParent = (UniversalShipment)packingParent.Clone();
				clonePackingParent.SetWriterStrategy(strategy);
				clonePackingParent.SetPackingLineCollection(() => new DataObjectList<PackingLine>());
				packingParents.Add(packingParentId, clonePackingParent);
				segment.SubShipmentCollection.Add(clonePackingParent);
			}
			return packingParentId;
		}

		class PackageMeta
		{
			internal PackageMeta(UniversalShipment packingParent, PackingLine package)
			{
				PackingParent = packingParent ?? throw new ArgumentNullException(nameof(packingParent));
				Package = package ?? throw new ArgumentNullException(nameof(package));
			}
			internal UniversalShipment PackingParent { get; }
			internal PackingLine Package { get; }
		}
	}
}
